using UnityEngine;
using System;

/// Copyright 2023 Procedural Worlds Ltd
/// Written by Jonathan Williams as part of Storm

namespace ProceduralWorlds.Storm.Utils
{
    [ExecuteAlways]
    [AddComponentMenu("Procedural Worlds/Storm/Storm Data Tile")]
    
    public class StormTile : MonoBehaviour
    {
        public bool UpdatePosition = true;
        public bool UpdateScale = true;
        public StormTileData TileData = null;
        public StormTileRegion TileRegions = null;
        [Obsolete("Use TileRegions instead")]
        public StormTileFeatureset TileFeaturesets = null;
        public StormRegion SavedRegion = null;
        public bool DisableTerrainDetails = true;
        public bool IngestCaptureAlphamaps = true;
        public bool IngestCaptureDisabled = true;
        public bool IngestCreateFeatures = true;
        public bool DisplayPolygonsInEditor = false;
        [System.NonSerialized]
        private bool m_lastDisableDetails;
        public bool SaveTileSingleFile = false;
        public bool SaveRawAsTexture = false;
        public bool ShowSelectionGizmo = false;

        bool HasPostedData = false;
        bool HasPostedRegions = false;
        public void OnEnable()
        {
#pragma warning disable CS0618 // Type or member is obsolete
            if (TileFeaturesets != null)
            {
                if (TileFeaturesets.m_featuresets?.Length > 0)
                {
                    TileRegions = new StormTileRegion();
                    TileRegions.Position = TileFeaturesets.Position;
                    TileRegions.Size = TileFeaturesets.Size;
                    TileRegions.Regions = new StormRegion[] { ScriptableObject.CreateInstance<StormRegion>() };
                    TileRegions.Regions[0].Featuresets = TileFeaturesets.m_featuresets;
                    TileRegions.m_myObject = gameObject;
                }
                TileFeaturesets = null;
            }
#pragma warning restore CS0618 // Type or member is obsolete
            StormAPI.OnStormInstanceChanged -= OnStormInstanceChanged;
            StormAPI.OnStormInstanceChanged += OnStormInstanceChanged;
            if (TileRegions == null)
                TileRegions = new StormTileRegion();
            if (TileData == null)
            {
                TileData = new StormTileData();
            }
            TileData.m_myObject = gameObject;
            if (UpdatePosition)
            {
                TileData.Position = transform.position;
            }
            if (UpdateScale)
            {
                UnityEngine.Terrain terrain = GetComponent<UnityEngine.Terrain>();
                if (terrain != null)
                    TileData.Size = terrain.terrainData.size;
                else
                    TileData.Size = transform.localScale;
            }
            TileRegions.m_myObject = gameObject;
            TileRegions.Position = TileData.Position;
            TileRegions.Size = TileData.Size;
            UpdateData(true);
            bool shouldDisable = DisableTerrainDetails && StormAPI.IsStormWorldActive;
            m_lastDisableDetails = shouldDisable;
            for (int t = 0; t < TileData.IngestedTerrains.Length; ++t)
            {
                UnityEngine.Terrain m_terrain = TileData.IngestedTerrains[t];
                if (m_terrain != null)
                    m_terrain.drawTreesAndFoliage = !shouldDisable;
            }
        }
        public void Update()
        {
            if (transform.hasChanged && (UpdatePosition || UpdateScale))
            {
                Bounds oldBounds = TileData.Bounds;
                transform.hasChanged = false;
                if (UpdatePosition)
                {
                    TileData.Position = transform.position;
                }
                if (UpdateScale)
                {
                    UnityEngine.Terrain terrain = GetComponent<UnityEngine.Terrain>();
                    if (terrain != null)
                        TileData.Size = terrain.terrainData.size;
                    else
                        TileData.Size = transform.localScale;
                }
                if (TileRegions != null)
                {
                    TileRegions.Position = TileData.Position;
                    TileRegions.Size = TileData.Size;
                }
                oldBounds.Encapsulate(TileData.Bounds);
                if (HasPostedData)
                    StormAPI.UpdateStormTile(TileData, oldBounds);
                if (HasPostedRegions)
                    StormAPI.UpdateStormTile(TileRegions, oldBounds);
            }
            bool shouldDisable = DisableTerrainDetails && StormAPI.IsStormWorldActive;
            if (m_lastDisableDetails != shouldDisable)
            {
                m_lastDisableDetails = shouldDisable;
                for (int t = 0; t < TileData.IngestedTerrains.Length; ++t)
                {
                    UnityEngine.Terrain m_terrain = TileData.IngestedTerrains[t];
                    if (m_terrain != null)
                        m_terrain.drawTreesAndFoliage = !shouldDisable;
                }
            }
        }
        public void OnDisable()
        {
            StormAPI.OnStormInstanceChanged -= OnStormInstanceChanged;
            {
                if (HasPostedData)
                {
                    StormAPI.RemoveStormTile(TileData);
                    TileData.OnTileDisabled?.Invoke();
                }
                if (HasPostedRegions)
                    StormAPI.RemoveStormTile(TileRegions);
                HasPostedData = false;
                HasPostedRegions = false;
                int num = TileData.TransformData.Length;
                for (int t = 0; t < num; ++t)
                {
                    StormTransformList list = TileData.TransformData[t]?.Data;
                    if (list != null)
                    {
                        var items = list.Transforms;
                        int itemsCount = items.Length;
                        for (int i = 0; i < itemsCount; ++i)
                        {
                            if (items[i] != null)
                                items[i].gameObject.SetActive(true);
                        }
                    }
                }
            }
            bool shouldDisable = !(DisableTerrainDetails && StormAPI.IsStormWorldActive);
            if (m_lastDisableDetails != shouldDisable)
            {
                m_lastDisableDetails = shouldDisable;
                for (int t = 0; t < TileData.IngestedTerrains.Length; ++t)
                {
                    UnityEngine.Terrain m_terrain = TileData.IngestedTerrains[t];
                    if (m_terrain != null)
                        m_terrain.drawTreesAndFoliage = !shouldDisable;
                }
            }
            TileData.Dispose();
        }
        private void OnDestroy()
        {
            OnDisable();
        }
        private void OnStormInstanceChanged(MonoBehaviour world)
        {
            if (world != null)
            {
                bool hasData = TileData != null && (TileData.HasData());
                bool hasRegions = TileRegions?.Regions?.Length > 0;
                if (hasData)
                    StormAPI.AddStormTile(TileData);
                if (hasRegions)
                    StormAPI.AddStormTile(TileRegions);
            }
            int num = TileData.TransformData.Length;
            for (int t = 0; t < num; ++t)
            {
                StormTransformList list = TileData.TransformData[t]?.Data;
                if (list != null)
                {
                    var items = list.Transforms;
                    int itemsCount = items.Length;
                    for (int i = 0; i < itemsCount; ++i)
                    {
                        if (items[i] != null)
                            items[i].gameObject.SetActive(!StormAPI.IsStormWorldActive);
                    }
                }
            }
            bool shouldDisable = DisableTerrainDetails && StormAPI.IsStormWorldActive;
            num = TileData.IngestedTerrains.Length;
            for (int t = 0; t < num; ++t)
            {
                if (TileData.IngestedTerrains[t] != null)
                    TileData.IngestedTerrains[t].drawTreesAndFoliage = !shouldDisable;
            }
            UnityEngine.Terrain m_terrain = GetComponent<UnityEngine.Terrain>();
            if (m_terrain != null)
                m_terrain.drawTreesAndFoliage = !shouldDisable;
        }
        private void OnDrawGizmosSelected()
        {
            if (!ShowSelectionGizmo || TileData == null)
                return;
            Color oColor = Gizmos.color;
            Color gColor = Color.green;
            Gizmos.color = gColor;
            Gizmos.DrawWireCube(TileData.Position + TileData.Size * 0.5f, TileData.Size);
            gColor.a = 0.2f;
            Gizmos.color = gColor;
            Gizmos.DrawCube(TileData.Position + TileData.Size * 0.5f, TileData.Size);
            if (DisplayPolygonsInEditor)
            {
                for (int s = 0; s < TileData.PolygonData.Length; ++s)
                {
                    var polys = TileData.PolygonData[s];
                    for (int poly = 0; poly < polys.Data.Length; ++poly)
                    {
                        PolyPointSet polySet = polys.Data[poly];
                        if (polySet.Length < 2)
                            continue;
                        Color c = Color.Lerp(Color.red, Color.blue, poly / (polys.Data.Length));
                        c.a = 1;
                        Gizmos.color = c;
#if UNITY_2022_1_OR_NEWER
                        Vector3[] points = new Vector3[polySet.Length];
                        for (int p = 0; p < polySet.Length; ++p)
                        {
                            points[p] = new Vector3(TileData.Position.x + (polySet[p].x * TileData.Size.x), TileData.Position.y, TileData.Position.z + (polySet[p].y * TileData.Size.z));
                        }
                        Gizmos.DrawLineStrip(points, true);
#else
                        Vector3 lastPoint = new Vector3(TileData.Position.x + (polySet[polySet.Length - 1].x * TileData.Size.x), TileData.Position.y, TileData.Position.z + (polySet[polySet.Length - 1].y * TileData.Size.z));
                        for (int p = 0; p < polySet.Length; ++p)
                        {
                            Vector3 point = new Vector3(TileData.Position.x + (polySet[p].x * TileData.Size.x), TileData.Position.y, TileData.Position.z + (polySet[p].y * TileData.Size.z));
                            Gizmos.DrawLine(lastPoint, point);
                            lastPoint = point;
                        }
#endif
                    }
                }
            }
            Gizmos.color = oColor;
        }

        public void UpdateData(bool dataChanged = false)
        {
            if (HasPostedData)
                TileData.UpdateDatasets(dataChanged);
            if (HasPostedRegions)
                TileRegions.UpdateData();
            bool hasData = TileData.HasData();
            bool hasRegions = TileRegions.Regions.Length > 0;
            if (hasData != HasPostedData)
            {
                HasPostedData = hasData;
                if (hasData)
                {
                    TileData.UpdateDatasets(dataChanged);
                    StormAPI.AddStormTile(TileData);
                    TileData.OnTileEnabled?.Invoke();
                }
                else
                {
                    StormAPI.RemoveStormTile(TileData);
                    TileData.OnTileDisabled?.Invoke();
                }
            }
            if (hasRegions != HasPostedRegions)
            {
                HasPostedRegions = hasRegions;
                if (hasRegions)
                    StormAPI.AddStormTile(TileRegions);
                else
                    StormAPI.RemoveStormTile(TileRegions);
            }
        }
    }
}