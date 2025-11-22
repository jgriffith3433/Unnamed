/// Copyright 2024 Procedural Worlds Ltd
/// Part of Storm Geospatial Plugin

using UnityEngine;
using UnityEditor;

namespace ProceduralWorlds.Storm.Geospatial
{
    /// <summary>
    /// Custom editor for GeoTerrainTag component
    /// Provides a compact interface for viewing geographic metadata
    /// </summary>
    [CustomEditor(typeof(GeoTerrainTag))]
    public class GeoTerrainTagEditor : UnityEditor.Editor
    {
        private GeoTerrainTag m_target;
        private bool m_showDetails = false;

        void OnEnable()
        {
            m_target = (GeoTerrainTag)target;
        }

        public override void OnInspectorGUI()
        {
            EditorGUI.BeginChangeCheck();

            // Compact header with key info
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("GeoTerrainTag", EditorStyles.boldLabel, GUILayout.Width(100));
            EditorGUILayout.LabelField($"Tile ({m_target.TileCoordinates.x},{m_target.TileCoordinates.y})", GUILayout.ExpandWidth(true));
            m_showDetails = EditorGUILayout.Toggle(m_showDetails, GUILayout.Width(20));
            EditorGUILayout.EndHorizontal();

            var geoRegion = m_target.GeographicRegion;
            if (geoRegion != null)
            {
                // Compact geographic summary
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Location", GUILayout.Width(60));
                if (geoRegion.centroid != null)
                {
                    EditorGUILayout.LabelField($"{geoRegion.centroid.latitude:F4}°, {geoRegion.centroid.longitude:F4}°", GUILayout.ExpandWidth(true));
                }
                else
                {
                    EditorGUILayout.LabelField("No centroid data", GUILayout.ExpandWidth(true));
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.LabelField($"Sea Level  {m_target.SeaLevelInWorldUnits:F0}m");

                // Compact elevation info
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Elevation", GUILayout.Width(60));
                float realWorldRange = m_target.GlobalElevationRange.maxElevation - m_target.GlobalElevationRange.minElevation;
                float totalPadding = m_target.GlobalElevationRange.bufferBelow + m_target.GlobalElevationRange.bufferAbove;
                EditorGUILayout.LabelField($"{m_target.GlobalElevationRange.elevationRangeMin:F0}m - {m_target.GlobalElevationRange.elevationRangeMax:F0}m (Δ{m_target.GlobalElevationRange.elevationRangeMax - m_target.GlobalElevationRange.elevationRangeMin:F0}m) | Real: {m_target.GlobalElevationRange.minElevation:F0}m - {m_target.GlobalElevationRange.maxElevation:F0}m (Δ{realWorldRange:F0}m) | Padding: -{m_target.GlobalElevationRange.bufferBelow:F0}m/+{m_target.GlobalElevationRange.bufferAbove:F0}m (Δ{totalPadding:F0}m)", GUILayout.ExpandWidth(true));
                EditorGUILayout.EndHorizontal();

                // Compact technical info
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Config", GUILayout.Width(60));
                EditorGUILayout.LabelField($"1:{m_target.WorldScale:F0} | {m_target.HeightmapResolution}px", GUILayout.ExpandWidth(true));
                EditorGUILayout.EndHorizontal();
            }
            else
            {
                EditorGUILayout.HelpBox("No geographic data available", MessageType.Warning);
            }

            // Status indicator
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Status", GUILayout.Width(60));
            string statusText = m_target.IsProcessed ? "Processed" : "Not Processed";
            MessageType statusType = m_target.IsProcessed ? MessageType.Info : MessageType.Warning;
            EditorGUILayout.HelpBox(statusText, statusType);
            EditorGUILayout.EndHorizontal();

            // Details section (collapsible)
            if (m_showDetails)
            {
                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField("Detailed Information", EditorStyles.boldLabel);
                
                if (geoRegion != null)
                {
                    EditorGUI.indentLevel++;
                    
                    // CRS and metadata
                    EditorGUILayout.LabelField("CRS", geoRegion.crs ?? "Unknown");
                    
                    // Processing timestamps
                    EditorGUILayout.LabelField("Created", m_target.CreationTime.ToString("yyyy-MM-dd HH:mm"));
                    EditorGUILayout.LabelField("Modified", m_target.LastModifiedTime.ToString("yyyy-MM-dd HH:mm"));
                    
                    EditorGUI.indentLevel--;
                }
            }

            // Action buttons
            EditorGUILayout.Space(5);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Open in Storm Maps", GUILayout.ExpandWidth(true)))
            {
                OpenInStormMaps();
            }
            if (GUILayout.Button("Copy Summary", GUILayout.Width(100)))
            {
                CopySummaryToClipboard();
            }
            EditorGUILayout.EndHorizontal();

            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(m_target);
            }
        }
        
        /// <summary>
        /// Open the terrain tile region in Storm Maps web interface
        /// </summary>
        private void OpenInStormMaps()
        {
            var geoRegion = m_target.GeographicRegion;
            if (geoRegion?.boundingBox == null)
            {
                EditorUtility.DisplayDialog("Storm Maps", "No geographic data available for this terrain tile.", "OK");
                return;
            }
            
            var bounds = geoRegion.boundingBox;
            var dimensions = geoRegion.dimensions;
            
            // Calculate centroid
            double centerLat = geoRegion.centroid.latitude;
            double centerLon = geoRegion.centroid.longitude;
            
            // Round dimensions to nearest 1000m
            int widthKm = Mathf.RoundToInt((float)dimensions.widthMeters / 1000f);
            int heightKm = Mathf.RoundToInt((float)dimensions.heightMeters / 1000f);
            
            // Ensure minimum size of 1km
            widthKm = Mathf.Max(1, widthKm);
            heightKm = Mathf.Max(1, heightKm);
            
            // Build URL with parameters
            string url = $"https://maps.procedural-worlds.com/main/?lat={centerLat:F6}&lon={centerLon:F6}&width={widthKm}&height={heightKm}";
            
            // Open in default browser
            Application.OpenURL(url);
        }

        /// <summary>
        /// Copy terrain summary to clipboard
        /// </summary>
        private void CopySummaryToClipboard()
        {
            string summary = m_target.GetSummary();
            EditorGUIUtility.systemCopyBuffer = summary;
            Debug.Log($"[StormGeo] Copied terrain summary to clipboard: {summary}");
        }
    }
} 