
using ProceduralWorlds.Storm.StormEditor;
using UnityEngine;
using UnityEditor;
using PWCommon5;
using System.IO;
using System.Collections;
using System;
using System.Linq;
using System.Collections.Generic;

namespace ProceduralWorlds.Storm.Geospatial.Editor
{
    /// <summary>
    /// Editor window for importing Storm Maps geo packages
    /// </summary>
    public class GeoPackageImporterWindow : StormPluginEditorBase, IPWEditor
    {
        #region Properties
        public override GUIContent Label => new GUIContent("Storm Geospatial");
        public bool PositionChecked { get; set; }
        #endregion

        #region Variables
        private bool m_isEmbedded;
        public override bool IsEmbedded
        {
            get => m_isEmbedded;
            set => m_isEmbedded = value;
        }
        private TabSet m_mainTabs;
        private EditorUtils m_editorUtils;
        private Vector2 m_scrollPosition = Vector2.zero;
        private GUIStyle m_boxStyle;
        private GeoPackageImporter m_importer;
        private GeoPackageImportSettings m_settings;
        private bool m_isImporting = false;
        private string m_importProgress = "";
        private float m_importProgressValue = 0f;
        private IEnumerator m_terrainCoroutine;
        private bool m_cancelImport = false;
        private double m_lastProgressUpdateTime = 0;
        private bool m_importJustCompleted = false;
        private double m_completionTime = 0;

        #endregion

        /// <summary>
        /// Create tabs for the window
        /// </summary>
        private void OnEnable()
        {
            if (m_editorUtils == null)
            {
                m_editorUtils = StormApp.GetEditorUtils(this);
            }

            //Set up the tabs
            Tab[] mainTabs = new Tab[]
            {
                new Tab("SourceGeoDataTab", SourceGeoDataTab),
                new Tab("LoadGeoDataTab", LoadGeoPackageTab),
            };
            m_mainTabs = new TabSet(m_editorUtils, mainTabs);
        }

        /// <summary>
        /// OnGUI - Display the window GUI
        /// </summary>
        public override void OnGUI()
        {
            //Get editor utils for this
            if (m_editorUtils == null)
            {
                m_editorUtils = StormApp.GetEditorUtils(this);

            }

            // Load or create default settings
            if (m_settings == null)
            {
                LoadOrCreateSettings();
            }

            // Load the package if it exists
            if (!string.IsNullOrEmpty(m_settings.GeoPackagePath) && Directory.Exists(m_settings.GeoPackagePath))
            {
                if (m_importer == null || !m_importer.IsPackageLoaded)
                {
                    m_importer = new GeoPackageImporter();
                    m_importer.LoadGeoPackage(m_settings.GeoPackagePath, m_settings);
                }
            }

            //Initialize editor utils
            m_editorUtils.Initialize(); // Do not remove this!

            //Set up the box style
            if (m_boxStyle == null)
            {
                m_boxStyle = new GUIStyle(GUI.skin.box);
                m_boxStyle.normal.textColor = GUI.skin.label.normal.textColor;
                m_boxStyle.fontStyle = FontStyle.Bold;
                m_boxStyle.alignment = TextAnchor.UpperLeft;
            }

            //Set up the scroll position & heading
            if (IsEmbedded)
            {
                m_scrollPosition = EditorGUILayout.BeginScrollView(m_scrollPosition);
            }
            else
            {
                m_editorUtils.GUIHeader();
                m_scrollPosition = EditorGUILayout.BeginScrollView(m_scrollPosition);
            }

            //Set up the tabs
            m_editorUtils.Tabs(m_mainTabs);

            //End the scroll view
            EditorGUILayout.EndScrollView();

            //End with a footer
            if (!IsEmbedded)
            {
                m_editorUtils.GUINewsFooter();
            }
        }

        /// <summary>
        /// Show the window
        /// </summary>
        public static void ShowWindow()
        {
            Debug.Log("ShowWindow");
            if (EmbeddedInstance.TryGetValue(typeof(GeoPackageImporterWindow), out var win) && win != null)
                win.OnOpen?.Invoke();

            if (!WindowInstance.TryGetValue(typeof(GeoPackageImporterWindow), out win) || win == null)
            {
                win = GetWindow<GeoPackageImporterWindow>(false, "Geo Package Importer");
                while (win.IsEmbedded)
                {
                    win.Close();
                    win = GetWindow<GeoPackageImporterWindow>(false, "Geo Package Importer");
                }
            }
            Vector2 initialSize = new Vector2(200f, 200f);
            win.position = new Rect(new Vector2((Screen.currentResolution.width - initialSize.x) / 2f, (Screen.currentResolution.height - initialSize.y) / 2f), initialSize);
            WindowInstance[typeof(GeoPackageImporterWindow)] = win;
            win.Show();
        }

        /// <summary>
        /// Open the window
        /// </summary>
        public override void OpenWindow()
        {
            Debug.Log("OpenWindow");
            ShowWindow();
        }

        public override void HideWindow()
        {
            Debug.Log("HideWindow");
            if (WindowInstance.TryGetValue(typeof(GeoPackageImporterWindow), out var win) && win != null)
                win.Close();
            WindowInstance.Remove(typeof(GeoPackageImporterWindow));
        }

        #region Source Geo Data

        private void SourceGeoDataTab()
        {
            EditorGUILayout.BeginVertical(m_boxStyle);

            EditorGUILayout.BeginVertical(m_boxStyle);
            m_editorUtils.Title("HowToGetItTitle");            
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical(m_boxStyle);
            EditorGUILayout.Space();
            m_editorUtils.Text("HowToGetItDetail");
            EditorGUILayout.Space();
            m_editorUtils.Link("WhereToGetItLink");
            EditorGUILayout.Space();
            EditorGUILayout.EndVertical();

            EditorGUILayout.EndVertical();
        }

        #endregion

        #region Import GeoPackage 

        private void LoadGeoPackageTab()
        {
            m_editorUtils.Panel("LoadGeoPackagePanel", LoadGeoPackagePanel, true);

            if (m_importer != null && m_importer.IsPackageLoaded)
            {
                m_editorUtils.Panel("ImportDetailsPanel", ImportDetailsPanel, false);
                m_editorUtils.Panel("ImportSettingsPanel", ImportSettingsPanel, false);
                m_editorUtils.Panel("ImportGeoPackagePanel", ImportGeoPackagePanel, true);
            }
        }

        #region Load GeoPackage Panel
        /// <summary>
        /// Import settings panel
        /// </summary>
        /// <param name="helpActive"></param>
        private void LoadGeoPackagePanel(bool helpActive)
        {
            EditorGUI.BeginChangeCheck();

            //Display / load the path / load the package
            EditorGUILayout.BeginVertical(m_boxStyle);
            EditorGUILayout.BeginHorizontal();

            //Get the final section of the path
            string finalPath = m_settings.GeoPackagePath.Substring(m_settings.GeoPackagePath.LastIndexOf('/') + 1);
            GUIContent pathContent = new GUIContent(finalPath);
            m_editorUtils.LabelField("GeoPackagePath", pathContent, helpActive);

            // Browse button
            if (m_editorUtils.Button("BrowseGeoPackageDirectory", helpActive, GUILayout.Width(60f)))
            {
                string selectedPath = EditorUtility.OpenFolderPanel("Select Geopackage Directory", 
                    string.IsNullOrEmpty(m_settings.GeoPackagePath) ? Application.dataPath : m_settings.GeoPackagePath, "");
                if (!string.IsNullOrEmpty(selectedPath))
                {
                    m_settings.GeoPackagePath = selectedPath;
                    m_importer?.Dispose();
                    m_importer = new GeoPackageImporter();
                    m_importer.LoadGeoPackage(m_settings.GeoPackagePath, m_settings);
                }
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();

            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(m_settings);
            }
        }
        #endregion

        #region Import Details Panel

        /// <summary>
        /// Import details panel
        /// </summary>
        /// <param name="helpActive"></param>
        private void ImportDetailsPanel(bool helpActive)
        {
            EditorGUI.BeginChangeCheck();

            EditorGUILayout.BeginVertical(m_boxStyle);
            
            // Package Information Section
            EditorGUILayout.LabelField("Package Information", EditorStyles.boldLabel);
            GUIContent packageNameContent = new GUIContent(m_importer.Metadata.title);
            m_editorUtils.LabelField("PackageName", packageNameContent, helpActive);
            GUIContent packageAbstractContent = new GUIContent(m_importer.Metadata.@abstract);
            m_editorUtils.LabelField("PackageAbstract", packageAbstractContent, helpActive);
            
            // Storm Map Link
            if (m_importer.Metadata.geographicMetadata != null && m_importer.Metadata.geographicMetadata.IsValid())
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Storm Map", GUILayout.Width(EditorGUIUtility.labelWidth));
                if (GUILayout.Button("Open in Browser", GUILayout.Width(120)))
                {
                    Application.OpenURL(m_importer.Metadata.geographicMetadata.GetStormMapUrl());
                }
                EditorGUILayout.EndHorizontal();
            }
            
            EditorGUILayout.Space();
            
            // Geographic Information Section
            if (m_importer.Metadata.geographicMetadata != null && m_importer.Metadata.geographicMetadata.IsValid())
            {
                EditorGUILayout.LabelField("Geographic Information", EditorStyles.boldLabel);
                
                // Coordinate Reference System
                if (!string.IsNullOrEmpty(m_importer.Metadata.geographicMetadata.crs))
                {
                    GUIContent crsContent = new GUIContent(m_importer.Metadata.geographicMetadata.crs);
                    m_editorUtils.LabelField("CoordinateSystem", crsContent, helpActive);
                }
                
                // Centroid (Center Point)
                if (m_importer.Metadata.geographicMetadata.centroid != null)
                {
                    string centroidText = $"{m_importer.Metadata.geographicMetadata.centroid.latitude:F6}°, {m_importer.Metadata.geographicMetadata.centroid.longitude:F6}°";
                    GUIContent centroidContent = new GUIContent(centroidText);
                    m_editorUtils.LabelField("Location", centroidContent, helpActive);
                }
                
                // Dimensions in meters
                if (m_importer.Metadata.geographicMetadata.dimensions != null)
                {
                    string sizeText = $"{m_importer.Metadata.geographicMetadata.dimensions.widthMeters:F0}m × {m_importer.Metadata.geographicMetadata.dimensions.heightMeters:F0}m";
                    GUIContent sizeContent = new GUIContent(sizeText);
                    m_editorUtils.LabelField("Dimensions", sizeContent, helpActive);
                    
                    if (m_importer.Metadata.geographicMetadata.dimensions.areaSquareMeters > 0)
                    {
                        string areaText = $"{m_importer.Metadata.geographicMetadata.dimensions.areaSquareMeters / 1000000:F2} km²";
                        GUIContent areaContent = new GUIContent(areaText);
                        m_editorUtils.LabelField("Area", areaContent, helpActive);
                    }
                }
            }
            else
            {
                EditorGUILayout.LabelField("Geographic Information", EditorStyles.boldLabel);
                EditorGUILayout.LabelField("No geographic metadata available", EditorStyles.helpBox);
            }
            
            // Errors Section
            if (m_importer.Errors.Count > 0)
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Package Validation Errors", EditorStyles.boldLabel);
                foreach (var error in m_importer.Errors)
                {
                    EditorGUILayout.LabelField(error, EditorStyles.helpBox);
                }
            }

            // Warnings Section - Only show MODERATE or higher severity
            var significantWarnings = m_importer.Warnings.Where(warning => 
            {
                // Parse severity from warning message
                if (warning.Contains("Severity: SMALL"))
                    return false;
                return true; // Show MODERATE, LARGE, and any other severities
            }).ToList();
            
            if (significantWarnings.Count > 0)
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Package Validation Warnings", EditorStyles.boldLabel);
                foreach (var warning in significantWarnings)
                {
                    EditorGUILayout.LabelField(warning, EditorStyles.helpBox);
                }
            }

            // Data Distribution Summary Section
            if (m_importer.Distributions != null && m_importer.Distributions.Count > 0)
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Package Data Files", EditorStyles.boldLabel);
                
                // Show selection summary
                var selectedDistributions = GetSelectedDistributions();
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Selected:", GUILayout.Width(60));
                EditorGUILayout.LabelField($"{selectedDistributions.Count} of {m_importer.Distributions.Count(d => !ShouldExcludeFromDisplay(d.name))} files", EditorStyles.miniLabel);
                EditorGUILayout.EndHorizontal();
                
                EditorGUILayout.Space();
                
                // Selection controls
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Select All", GUILayout.Width(80)))
                {
                    SelectAllDistributions(true);
                }
                if (GUILayout.Button("Deselect All", GUILayout.Width(80)))
                {
                    SelectAllDistributions(false);
                }
                if (GUILayout.Button("Select by Type", GUILayout.Width(100)))
                {
                    ShowTypeSelectionMenu();
                }
                EditorGUILayout.EndHorizontal();
                
                EditorGUILayout.Space();
                
                // Group files by type for better organization, excluding boundary.kml and data-summary.json
                var groupedFiles = m_importer.Distributions
                    .Where(d => !ShouldExcludeFromDisplay(d.name))
                    .GroupBy(d => GetFileTypeCategory(d.dataType))
                    .OrderBy(g => GetTypeOrder(g.Key));
                
                foreach (var group in groupedFiles)
                {
                    // Group header
                    EditorGUILayout.LabelField($"{group.Key} Files", EditorStyles.miniLabel);
                    
                    foreach (var distribution in group.OrderBy(d => ParseResolutionForSorting(d.resolution)))
                    {
                        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                        
                        // File selection checkbox and name
                        EditorGUILayout.BeginHorizontal();
                        bool isSelected = GetDistributionSelection(distribution);
                        bool newSelection = EditorGUILayout.Toggle(isSelected, GUILayout.Width(20));
                        if (newSelection != isSelected)
                        {
                            SetDistributionSelection(distribution, newSelection);
                        }
                        
                        // File name and type
                        EditorGUILayout.LabelField(distribution.name, EditorStyles.boldLabel, GUILayout.ExpandWidth(true));
                        
                        // File size
                        if (distribution.sizeInBytes > 0)
                        {
                            EditorGUILayout.LabelField(FormatFileSize(distribution.sizeInBytes), EditorStyles.miniLabel, GUILayout.Width(60));
                        }
                        EditorGUILayout.EndHorizontal();
                        
                        // File details (2-line summary)
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField("", GUILayout.Width(20)); // Indent
                        
                        // Single line: Source, type, format, resolution
                        string infoLine = "";
                        if (!string.IsNullOrEmpty(distribution.source))
                        {
                            infoLine += $"Source: {distribution.source}";
                        }
                        if (!string.IsNullOrEmpty(distribution.dataType))
                        {
                            if (!string.IsNullOrEmpty(infoLine)) infoLine += " • ";
                            string capitalizedType = char.ToUpper(distribution.dataType[0]) + distribution.dataType.Substring(1);
                            infoLine += $"Type: {capitalizedType}";
                        }
                        if (!string.IsNullOrEmpty(distribution.format))
                        {
                            if (!string.IsNullOrEmpty(infoLine)) infoLine += " • ";
                            infoLine += $"Format: {distribution.format}";
                        }
                        if (!string.IsNullOrEmpty(distribution.resolution))
                        {
                            if (!string.IsNullOrEmpty(infoLine)) infoLine += " • ";
                            infoLine += $"Resolution: {distribution.resolution}";
                        }
                        if (string.IsNullOrEmpty(infoLine))
                        {
                            infoLine = "No information available";
                        }
                        EditorGUILayout.LabelField(infoLine, EditorStyles.miniLabel, GUILayout.ExpandWidth(true));
                        EditorGUILayout.EndHorizontal();
                        
                        // Show errors if any
                        var errors = GetDistributionErrors(distribution);
                        if (errors != null && errors.Count > 0)
                        {
                            EditorGUILayout.BeginHorizontal();
                            EditorGUILayout.LabelField("", GUILayout.Width(20)); // Indent
                            EditorGUILayout.HelpBox(string.Join("; ", errors), MessageType.Error);
                            EditorGUILayout.EndHorizontal();
                        }
                        
                        EditorGUILayout.EndVertical();
                        EditorGUILayout.Space(2);
                    }
                    
                    EditorGUILayout.Space(5);
                }
            }
            
            // Documentation link if available
            if (!string.IsNullOrEmpty(m_importer.Metadata.documentationURL))
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Documentation", EditorStyles.boldLabel);
                if (GUILayout.Button("Open Documentation", GUILayout.Height(20)))
                {
                    Application.OpenURL(m_importer.Metadata.documentationURL);
                }
            }

            EditorGUILayout.EndVertical();

            if (EditorGUI.EndChangeCheck())
            {
            }
        }
        
        /// <summary>
        /// Format file size in human-readable format
        /// </summary>
        /// <param name="bytes">Size in bytes</param>
        /// <returns>Formatted size string</returns>
        private string FormatFileSize(long bytes)
        {
            if (bytes == 0) return "0 B";
            
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            double size = bytes;
            int order = 0;
            
            while (size >= 1024 && order < sizes.Length - 1)
            {
                order++;
                size = size / 1024;
            }
            
            return $"{size:0.##} {sizes[order]}";
        }
        
        /// <summary>
        /// Parse resolution string for sorting (lower number = higher resolution)
        /// </summary>
        /// <param name="resolutionString">Resolution string like "10m", "30m", "1m"</param>
        /// <returns>Resolution value for sorting</returns>
        private float ParseResolutionForSorting(string resolutionString)
        {
            if (string.IsNullOrEmpty(resolutionString))
                return float.MaxValue; // Put invalid resolutions at the end
            
            try
            {
                // Remove common suffixes and trim
                string cleanString = resolutionString.ToLower()
                    .Replace("m", "")
                    .Replace("meters", "")
                    .Replace("meter", "")
                    .Trim();
                
                if (float.TryParse(cleanString, out float result))
                {
                    return result;
                }
                
                return float.MaxValue; // Put unparseable resolutions at the end
            }
            catch
            {
                return float.MaxValue;
            }
        }
        
        /// <summary>
        /// Get file type category for grouping
        /// </summary>
        /// <param name="dataType">Data type string</param>
        /// <returns>Category name</returns>
        private string GetFileTypeCategory(string dataType)
        {
            if (string.IsNullOrEmpty(dataType))
                return "Other";
            
            string typeLower = dataType.ToLower();
            if (typeLower.Contains("elevation"))
                return "Elevation";
            if (typeLower.Contains("imagery"))
                return "Imagery";
            if (typeLower.Contains("boundary"))
                return "Boundary";
            return "Other";
        }
        
        /// <summary>
        /// Get order for file type categories
        /// </summary>
        /// <param name="category">Category name</param>
        /// <returns>Order value</returns>
        private int GetTypeOrder(string category)
        {
            switch (category)
            {
                case "Elevation": return 0;
                case "Imagery": return 1;
                case "Boundary": return 2;
                case "Other": return 3;
                default: return 4;
            }
        }
        
        /// <summary>
        /// Check if a file should be excluded from display
        /// </summary>
        /// <param name="fileName">Name of the file to check</param>
        /// <returns>True if the file should be excluded</returns>
        private bool ShouldExcludeFromDisplay(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                return false;
                
            string lowerFileName = fileName.ToLower();
            return lowerFileName == "boundary.kml" || lowerFileName == "data-summary.json";
        }
        
        /// <summary>
        /// Get whether a distribution is selected for import
        /// </summary>
        /// <param name="distribution">Distribution to check</param>
        /// <returns>True if selected</returns>
        private bool GetDistributionSelection(GeoDistribution distribution)
        {
            if (distribution == null)
                return false;
                
            // Use the built-in isActive flag from the distribution
            return distribution.isActive;
        }
        
        /// <summary>
        /// Set whether a distribution is selected for import
        /// </summary>
        /// <param name="distribution">Distribution to update</param>
        /// <param name="isSelected">Whether to select</param>
        private void SetDistributionSelection(GeoDistribution distribution, bool isSelected)
        {
            if (distribution == null)
                return;
                
            distribution.isActive = isSelected;
            
            // Force repaint to update the UI
            Repaint();
        }
        
        /// <summary>
        /// Get all selected distributions for import
        /// </summary>
        /// <returns>List of selected distributions</returns>
        private List<GeoDistribution> GetSelectedDistributions()
        {
            if (m_importer?.Distributions == null)
                return new List<GeoDistribution>();
                
            return m_importer.Distributions
                .Where(d => !ShouldExcludeFromDisplay(d.name) && GetDistributionSelection(d))
                .ToList();
        }
        
        /// <summary>
        /// Select or deselect all distributions
        /// </summary>
        /// <param name="select">True to select all, false to deselect all</param>
        private void SelectAllDistributions(bool select)
        {
            if (m_importer?.Distributions != null)
            {
                foreach (var distribution in m_importer.Distributions.Where(d => !ShouldExcludeFromDisplay(d.name)))
                {
                    distribution.isActive = select;
                }
                
                // Force repaint to update the UI
                Repaint();
            }
        }
        
        /// <summary>
        /// Show type-based selection menu
        /// </summary>
        private void ShowTypeSelectionMenu()
        {
            var menu = new GenericMenu();
            
            menu.AddItem(new GUIContent("Select Elevation Files"), false, () => SelectDistributionsByType("elevation", true));
            menu.AddItem(new GUIContent("Select Imagery Files"), false, () => SelectDistributionsByType("imagery", true));
            menu.AddItem(new GUIContent("Select Boundary Files"), false, () => SelectDistributionsByType("boundary", true));
            menu.AddSeparator("");
            menu.AddItem(new GUIContent("Deselect Elevation Files"), false, () => SelectDistributionsByType("elevation", false));
            menu.AddItem(new GUIContent("Deselect Imagery Files"), false, () => SelectDistributionsByType("imagery", false));
            menu.AddItem(new GUIContent("Deselect Boundary Files"), false, () => SelectDistributionsByType("boundary", false));
            
            menu.ShowAsContext();
        }
        
        /// <summary>
        /// Select or deselect distributions by type
        /// </summary>
        /// <param name="typeKeyword">Type keyword to match</param>
        /// <param name="select">True to select, false to deselect</param>
        private void SelectDistributionsByType(string typeKeyword, bool select)
        {
            if (m_importer?.Distributions != null)
            {
                var matchingDistributions = m_importer.Distributions
                    .Where(d => !ShouldExcludeFromDisplay(d.name) && d.dataType?.ToLower().Contains(typeKeyword) == true);
                
                foreach (var distribution in matchingDistributions)
                {
                    distribution.isActive = select;
                }
                
                Debug.Log($"[StormGeo] {(select ? "Selected" : "Deselected")} {matchingDistributions.Count()} {typeKeyword} distributions");
                
                // Force repaint to update the UI
                Repaint();
            }
        }
        
        /// <summary>
        /// Get any errors for a distribution
        /// </summary>
        /// <param name="distribution">Distribution to check</param>
        /// <returns>List of errors, or null if none</returns>
        private List<string> GetDistributionErrors(GeoDistribution distribution)
        {
            var errors = new List<string>();
            
            // Check for common issues
            if (string.IsNullOrEmpty(distribution.dataType))
                errors.Add("Missing data type");
            
            if (string.IsNullOrEmpty(distribution.format))
                errors.Add("Missing format");
            
            if (distribution.sizeInBytes == 0)
                errors.Add("Unknown file size");
            
            // Check if file exists locally
            if (!string.IsNullOrEmpty(distribution.localFilePath) && !System.IO.File.Exists(distribution.localFilePath))
                errors.Add("Local file not found");
            
            return errors.Count > 0 ? errors : null;
        }
        
        #endregion

        #region Import Settings Panel
        /// <summary>
        /// Import settings panel
        /// </summary>
        /// <param name="helpActive"></param>
        private void ImportSettingsPanel(bool helpActive)
        {
            EditorGUI.BeginChangeCheck();

            EditorGUILayout.BeginVertical(m_boxStyle);

            // Generation Options Section
            EditorGUILayout.LabelField("Generation Options", EditorStyles.boldLabel);
            bool generateTerrain = m_editorUtils.Toggle("GenerateTerrain", m_settings.GenerateTerrain, helpActive);
            bool applyImagery = m_editorUtils.Toggle("ApplyImagery", m_settings.ApplyImagery, helpActive);

            EditorGUILayout.Space();

            // Terrain Configuration Section
            EditorGUILayout.LabelField("Terrain Configuration", EditorStyles.boldLabel);
            GameObject parentObject = (GameObject)m_editorUtils.ObjectField("ParentGameObject", m_settings.ParentGameObject, typeof(GameObject), true, helpActive);
            float tileSize = m_editorUtils.DelayedFloatField("TileSize", m_settings.TileSize, helpActive);

            EditorGUILayout.Space();

            // Resolution Settings Section
            EditorGUILayout.LabelField("Resolution Settings", EditorStyles.boldLabel);
            int heightmapResolution = m_editorUtils.IntField("HeightmapResolution", m_settings.HeightmapResolution, helpActive);
            int controlTextureResolution = m_editorUtils.IntField("ControlTextureResolution", m_settings.ControlTextureResolution, helpActive);
            int baseMapResolution = m_editorUtils.IntField("BaseMapResolution", m_settings.BaseMapResolution, helpActive);
            int detailResolution = m_editorUtils.IntField("DetailResolution", m_settings.DetailResolution, helpActive);
            int detailResolutionPerPatch = m_editorUtils.IntField("DetailResolutionPerPatch", m_settings.DetailResolutionPerPatch, helpActive);

            EditorGUILayout.Space();

            // Quality Settings Section
            EditorGUILayout.LabelField("Quality Settings", EditorStyles.boldLabel);
            float pixelError = m_editorUtils.FloatField("PixelError", m_settings.PixelError, helpActive);

            EditorGUILayout.Space();

            EditorGUILayout.EndVertical();

            if (EditorGUI.EndChangeCheck())
            {
                //Record the undo state
                Undo.RecordObject(m_settings, "Import Settings Changed");

                m_settings.ParentGameObject = parentObject;
                m_settings.GenerateTerrain = generateTerrain;
                m_settings.ApplyImagery = applyImagery;

                //If the tile size has changed, we need to update import settings and use intelligent defaults
                if (m_settings.TileSize != tileSize)
                {
                    m_settings.TileSize = tileSize;
                    m_settings.UpdateSettingsForTileSize(tileSize);
                }
                else
                {
                    m_settings.HeightmapResolution = heightmapResolution;
                    m_settings.ControlTextureResolution = controlTextureResolution;
                    m_settings.BaseMapResolution = baseMapResolution;
                    m_settings.DetailResolution = detailResolution;
                    m_settings.DetailResolutionPerPatch = detailResolutionPerPatch;
                }

                m_settings.PixelError = pixelError;

                EditorUtility.SetDirty(m_settings);
            }
        }
        
        #endregion

        #region Import GeoPackage Panel
        /// <summary>
        /// Import panel
        /// </summary>
        /// <param name="helpActive"></param>
        private void ImportGeoPackagePanel(bool helpActive)
        {
            //Display the package name
            EditorGUILayout.BeginVertical(m_boxStyle);
            EditorGUILayout.LabelField("Package Name", m_importer.Metadata.title);
            EditorGUILayout.LabelField("Package Abstract", m_importer.Metadata.@abstract);
            // Calculate and display total terrains
            if (m_importer.Metadata.geographicMetadata?.dimensions != null && m_settings != null)
            {
                int tilesX = Mathf.CeilToInt((float)m_importer.Metadata.geographicMetadata.dimensions.widthMeters / m_settings.TileSize);
                int tilesZ = Mathf.CeilToInt((float)m_importer.Metadata.geographicMetadata.dimensions.heightMeters / m_settings.TileSize);
                int totalTerrains = tilesX * tilesZ;
                EditorGUILayout.LabelField("Total Terrains", $"{totalTerrains} ({tilesX} × {tilesZ})");
            }
            else
            {
                EditorGUILayout.LabelField("Total Terrains", "Calculating...");
            }
            EditorGUILayout.EndVertical();
            DrawImportButton();
        }

        /// <summary>
        /// Draw the import button
        /// </summary>
        private void DrawImportButton()
        {
            EditorGUILayout.Space(10);

            bool canImport = m_importer.IsPackageLoaded && 
                           m_importer.Errors.Count == 0 &&
                           m_settings != null &&
                           !m_isImporting &&
                           GetSelectedDistributions().Count > 0;

            if (!m_isImporting)
            {
                // Import button
                EditorGUI.BeginDisabledGroup(!canImport);
                
                if (GUILayout.Button("Import Terrain", GUILayout.Height(30)))
                {
                    ImportTerrain();
                }
                
                EditorGUI.EndDisabledGroup();

                if (!canImport)
                {
                    string reason = "";
                    if (!m_importer.IsPackageLoaded)
                        reason = "No package loaded.";
                    else if (m_importer.Errors.Count > 0)
                        reason = "Package has errors.";
                    else if (m_settings == null)
                        reason = "Settings not configured.";
                    else if (GetSelectedDistributions().Count == 0)
                        reason = "No files selected for import.";

                    EditorGUILayout.HelpBox($"Import disabled: {reason}", MessageType.Info);
                }
            }
            else
            {
                // Show progress and cancel button when importing
                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField(m_importProgress, EditorStyles.wordWrappedLabel);
                EditorGUI.ProgressBar(EditorGUILayout.GetControlRect(false, 20), m_importProgressValue, $"{m_importProgressValue * 100:F0}%");
                
                EditorGUILayout.Space(5);
                
                // Cancel button
                GUI.backgroundColor = Color.red;
                if (GUILayout.Button("Cancel Import", GUILayout.Height(25)))
                {
                    CancelImport();
                }
                GUI.backgroundColor = Color.white;
            }
        }

        /// <summary>
        /// Import the terrain
        /// </summary>
        private void ImportTerrain()
        {
            if (!m_importer.IsPackageLoaded || m_settings == null)
                return;
                
            // Check if any distributions are selected
            var selectedDistributions = GetSelectedDistributions();
            if (selectedDistributions.Count == 0)
            {
                EditorUtility.DisplayDialog("No Files Selected", "Please select at least one file to import.", "OK");
                return;
            }

            // Reset cancel flag and start the import process
            m_cancelImport = false;
            m_isImporting = true;
            m_importJustCompleted = false;
            m_importProgress = "Starting import...";
            m_importProgressValue = 0f;
            
            // Create progress callback with improved UI updates
            TerrainGenerationProgressCallback progressCallback = (message, progress) =>
            {
                m_importProgress = message;
                m_importProgressValue = progress;
                
                // Throttle updates to 30 FPS (update every 33ms)
                double currentTime = EditorApplication.timeSinceStartup;
                bool shouldForceUpdate = (currentTime - m_lastProgressUpdateTime) > 0.033;
                
                // Always update on significant progress changes or if enough time has passed
                if (shouldForceUpdate || Mathf.Abs(progress - m_importProgressValue) > 0.01f)
                {
                    m_lastProgressUpdateTime = currentTime;
                    
                    // Force immediate repaint
                    Repaint();
                    
                    // Ensure the editor processes events
                    EditorApplication.QueuePlayerLoopUpdate();
                    UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
                }
            };

            // Create cancellation callback
            TerrainGenerationCancellationCallback cancellationCallback = () => m_cancelImport;

            // Start the terrain generation coroutine
            m_terrainCoroutine = m_importer.ConvertPackageToTerrainCoroutine(m_settings, progressCallback, cancellationCallback);
            
            // Register for editor updates to drive the coroutine
            EditorApplication.update += OnEditorUpdate;
        }

        /// <summary>
        /// Cancel the import
        /// </summary>
        private void CancelImport()
        {
            if (!m_isImporting)
                return;

            // Set cancel flag
            m_cancelImport = true;
            
            // Update progress message
            m_importProgress = "Cancelling import...";
            m_importProgressValue = -1f;
            
            // Force repaint to show cancellation message
            Repaint();
        }

        /// <summary>
        /// OnEditorUpdate - Update the import progress
        /// </summary>
        private void OnEditorUpdate()
        {
            // Handle the completion state with a delay to ensure UI updates
            if (m_importJustCompleted)
            {
                // Wait for at least 0.2 seconds after completion to ensure UI has updated
                double timeSinceCompletion = EditorApplication.timeSinceStartup - m_completionTime;
                
                if (timeSinceCompletion > 0.2)
                {
                    m_importJustCompleted = false;
                    
                    // Now unregister from editor updates
                    EditorApplication.update -= OnEditorUpdate;
                }
                else
                {
                    // Force aggressive UI updates to ensure the cancel button is removed
                    Repaint();
                    UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
                    
                    // Force the GUI to update by marking the window as dirty
                    EditorUtility.SetDirty(this);
                    
                    // Force event processing to simulate user interaction
                    Event e = Event.current;
                    if (e != null && e.type == EventType.Layout)
                    {
                        // Force a layout event
                        GUIUtility.ExitGUI();
                    }
                    
                    // Force focus update to trigger event processing
                    if (timeSinceCompletion > 0.1)
                    {
                        Focus();
                        EditorApplication.QueuePlayerLoopUpdate();
                    }
                }
                
                return;
            }
            
            if (!m_isImporting || m_terrainCoroutine == null)
                return;

            // Check for cancellation
            if (m_cancelImport)
            {
                // Cancel the import process
                m_isImporting = false;
                m_importProgress = "Import cancelled by user.";
                m_importProgressValue = -1f;
                m_terrainCoroutine = null;
                m_cancelImport = false;
                
                // Mark as just completed to ensure one more update
                m_importJustCompleted = true;
                m_completionTime = EditorApplication.timeSinceStartup;
                
                // Force immediate aggressive UI update
                Repaint();
                UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
                EditorUtility.SetDirty(this);
                
                // Schedule an immediate delayed call to force refresh
                EditorApplication.delayCall += () => {
                    Repaint();
                    UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
                };
                
                return;
            }

            try
            {
                // Move the coroutine forward
                if (!m_terrainCoroutine.MoveNext())
                {
                    // Coroutine completed
                    m_isImporting = false;
                    m_importProgress = "Import completed successfully!";
                    m_importProgressValue = 1.0f;
                    m_terrainCoroutine = null;
                    
                    // Mark as just completed to ensure one more update
                    m_importJustCompleted = true;
                    m_completionTime = EditorApplication.timeSinceStartup;
                    
                    // Force immediate aggressive UI update
                    Repaint();
                    UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
                    EditorUtility.SetDirty(this);
                    
                    // Schedule an immediate delayed call to force refresh
                    EditorApplication.delayCall += () => {
                        Repaint();
                        UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
                    };
                }
                else
                {
                    // Force Unity to continue processing and update the window
                    Repaint();
                    
                    // Force the editor to refresh more frequently during import
                    if (Event.current != null && Event.current.type == EventType.Repaint)
                    {
                        EditorApplication.QueuePlayerLoopUpdate();
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle any errors
                m_isImporting = false;
                m_importProgress = $"Import failed: {ex.Message}";
                m_importProgressValue = -1f;
                m_terrainCoroutine = null;
                
                // Mark as just completed to ensure one more update
                m_importJustCompleted = true;
                m_completionTime = EditorApplication.timeSinceStartup;
                
                Debug.LogError($"[StormGeo] Import failed: {ex.Message}\n{ex.StackTrace}");
                
                // Force immediate aggressive UI update
                Repaint();
                UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
                EditorUtility.SetDirty(this);
                
                // Schedule an immediate delayed call to force refresh
                EditorApplication.delayCall += () => {
                    Repaint();
                    UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
                };
            }
        }
        #endregion

        /// <summary>
        /// Load or create the import settings asset
        /// </summary>
        private void LoadOrCreateSettings()
        {
            // Try to find existing settings asset
            string[] guids = AssetDatabase.FindAssets("t:GeoPackageImportSettings");
            if (guids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                m_settings = AssetDatabase.LoadAssetAtPath<GeoPackageImportSettings>(path);
            }

            // Create default settings if none found
            if (m_settings == null)
            {
                m_settings = CreateInstance<GeoPackageImportSettings>();
                
                // Save as asset
                string settingsPath = "Assets/Storm User Data/Settings/GeoPackageImportSettings.asset";
                string directory = Path.GetDirectoryName(settingsPath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                
                AssetDatabase.CreateAsset(m_settings, settingsPath);
                AssetDatabase.SaveAssets();
            }

            // Inject the mark dirty action
            m_settings.SetMarkDirtyAction(MarkDirtyAction);
        }

        /// <summary>
        /// Mark the asset as dirty for serialization
        /// </summary>
        /// <param name="obj">The asset to mark as dirty</param>
        private static void MarkDirtyAction(UnityEngine.Object obj)
        {
            EditorUtility.SetDirty(obj);
        }

        #endregion
    }
}
