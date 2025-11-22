using System;
using Unity.Mathematics;
using UnityEngine;

namespace ProceduralWorlds.Storm.Utils
{
    public static class StormAPI
    {
        #region Functions
        public static void AddStormTile(StormTile tile)
        {
            if (tile != null)
            {
                if (tile.TileData?.HasData() ?? false)
                    OnStormTileAdded?.Invoke(tile.TileData);
                if (tile.TileRegions?.Regions?.Length > 0)
                    OnStormTileRegionAdded?.Invoke(tile.TileRegions);
            }
        }
        public static void AddStormTile(StormTileData tile)
        {
            if (tile?.HasData() ?? false)
                OnStormTileAdded?.Invoke(tile);
        }
        public static void AddStormTile(StormTileRegion tile)
        {
            if (tile != null && tile.Regions?.Length > 0)
                OnStormTileRegionAdded?.Invoke(tile);
        }
        public static void UpdateStormTile(StormTile tile, Bounds area)
        {
            if (tile != null)
            {
                OnStormTileUpdated?.Invoke(tile.TileData, area);
                OnStormTileRegionUpdated?.Invoke(tile.TileRegions, area);
            }
        }
        public static void UpdateStormTile(StormTileData tile, Bounds area)
        {
            OnStormTileUpdated?.Invoke(tile, area);
        }
        public static void UpdateStormTile(StormTileRegion tile, Bounds area)
        {
            OnStormTileRegionUpdated?.Invoke(tile, area);
        }
        public static void RemoveStormTile(StormTile tile)
        {
            if (tile != null)
            {
                OnStormTileRemoved?.Invoke(tile.TileData);
                OnStormTileRegionRemoved?.Invoke(tile.TileRegions);
            }
        }
        public static void RemoveStormTile(StormTileData tile)
        {
            if (tile != null)
                OnStormTileRemoved?.Invoke(tile);
        }
        public static void RemoveStormTile(StormTileRegion tile)
        {
            if (tile != null)
                OnStormTileRegionRemoved?.Invoke(tile);
        }

        public static void SetWorldOffset(double3 Offset)
        {
            OnSetWorldOffset?.Invoke(Offset);
        }
        public static void SetSeaLevel(float seaLevel) => SeaLevel = seaLevel;
        static float m_seaLevel;
        public static float SeaLevel
        {
            get => m_seaLevel;
            set {
                if (m_seaLevel != value)
                {
                    m_seaLevel = value;
                    OnSetSeaLevel?.Invoke(value);
                }
            }
        }
        public static void ShiftWorldOffset(double3 Offset)
        {
            OnWorldOffsetChanged?.Invoke(Offset);
        }
        public static void UpdateLayer(string layerName)
        {
            OnStormLayerUpdated?.Invoke(layerName);
        }
        public static void UpdateLayer(string layerName, Bounds area)
        {
            OnStormLayerUpdatedArea?.Invoke(area, layerName);
        }

        public static void UpdateRegion(StormRegion region)
        {
            OnRegionUpdated?.Invoke(region);
        }

        public static void AddDataProvider(StormDataProvider provider)
        {
            OnAddDataProvider?.Invoke(provider);
        }
        public static void RemoveDataProvider(StormDataProvider provider)
        {
            OnRemoveDataProvider?.Invoke(provider);
        }

        public static void AddWorldLayerRemap(StormLayerRemap remap)
        {
            OnWorldLayerRemapAdded?.Invoke(remap);
        }
        public static void RemoveWorldLayerRemap(StormLayerRemap remap)
        {
            OnWorldLayerRemapRemoved?.Invoke(remap);
        }

        public static ReadOnlySpan<float3> GetCameraPositions()
        {
            if (GetCameraPositionsCallback == null)
                return new ReadOnlySpan<float3>();
            return GetCameraPositionsCallback.Invoke();
        }
        #endregion
        #region Callbacks
        /// <summary>
        /// Is a StormWorld currently active?
        /// </summary>
        public static bool IsStormWorldActive = false;
        /// <summary>
        /// Reference to the GameObject the StormWorld is currently active on.
        /// </summary>
        public static MonoBehaviour StormWorldObject = null;
        /// <summary>
        /// Action Invoked when StormWorld instance has changed
        /// </summary>
        public static Action<MonoBehaviour> OnStormInstanceChanged;
        /// <summary>
        /// Action to Invoke when adding a new StormTile
        /// </summary>
        public static Action<StormTileData> OnStormTileAdded;
        /// <summary>
        /// Action to Invoke when updating a new StormTile with Bounds of affected area
        /// </summary>
        public static Action<StormTileData, Bounds> OnStormTileUpdated;
        /// <summary>
        /// Action to Invoke when removing a new StormTile
        /// </summary>
        public static Action<StormTileData> OnStormTileRemoved;
        /// <summary>
        /// Action to Invoke when adding a new FeaturesetTile
        /// </summary>
        public static Action<StormTileRegion> OnStormTileRegionAdded;
        /// <summary>
        /// Action to Invoke when updating a FeaturesetTile with Bounds of affected area
        /// </summary>
        public static Action<StormTileRegion, Bounds> OnStormTileRegionUpdated;
        /// <summary>
        /// Action to Invoke when removing a FeaturesetTile
        /// </summary>
        public static Action<StormTileRegion> OnStormTileRegionRemoved;
        /// <summary>
        /// Action to Invoke when a Feature is disabled
        /// </summary>
        public static Action<StormFeatureset, StormFeatureSettings> OnFeatureDisabled;
        /// <summary>
        /// Action to Invoke when a Feature is enabled
        /// </summary>
        public static Action<StormFeatureset, StormFeatureSettings> OnFeatureEnabled;
        /// <summary>
        /// Action to Invoke when a Data Provider is being added
        /// </summary>
        public static Action<StormDataProvider> OnAddDataProvider;
        /// <summary>
        /// Action to Invoke when a Data Provider is being removed
        /// </summary>
        public static Action<StormDataProvider> OnRemoveDataProvider;

        public static Action<StormRegion> OnRegionUpdated;


        /// <summary>
        /// Action to Invoke when updating a Layer with Bounds of affected area
        /// </summary>
        public static Action<Bounds, string> OnStormLayerUpdatedArea;
        /// <summary>
        /// Action to Invoke when updating a Layer without any bounds
        /// </summary>
        public static Action<string> OnStormLayerUpdated;

        /// <summary>
        /// Action invoked when settings used in Generation are changed.
        /// </summary>
        public static Action<StormFeatureType> OnGenerationSettingsChanged;
        /// <summary>
        /// Action invoked when settings for LOD distances change
        /// </summary>
        public static Action<StormFeatureType> OnLODSettingsChanged;
        /// <summary>
        /// Action invoked when Feature Set Resolution setting has changed
        /// </summary>
        public static Action OnFeatureMaskResolutionChanged;
        /// <summary>
        /// Action invoked when Height Resolution setting has changed
        /// </summary>
        public static Action OnLoadingMapResolutionChanged;
        /// <summary>
        /// Action invoked when a Feature's Render Settings change
        /// </summary>
        public static Action<StormFeatureSettings> OnRenderSettingsChanged;

        /// <summary>
        /// Action invoked when the world offset changes. value is change from last position
        /// </summary>
        public static Action<double3> OnWorldOffsetChanged;
        /// <summary>
        /// Action invoked when the world offset is being set to a specific value (not relative)
        /// </summary>
        public static Action<double3> OnSetWorldOffset;
        /// <summary>
        /// Action invoked when the world sea level is being set to a specific value (not relative)
        /// </summary>
        public static Action<float> OnSetSeaLevel;
        /// <summary>
        /// Action invoked when the Layer Remap object has been changed
        /// </summary>
        public static Action OnLayerRemapsChanged;
        /// <summary>
        /// Action Invoked when layer indexes from StormUtils are reset
        /// </summary>
        public static Action OnLayerIndexesChanged;

        /// <summary>
        /// Action invoked when a Layer Remap object is being Added to the world
        /// </summary>
        public static Action<StormLayerRemap> OnWorldLayerRemapAdded;
        /// <summary>
        /// Action invoked when a Layer Remap object is being Removed from the World
        /// </summary>
        public static Action<StormLayerRemap> OnWorldLayerRemapRemoved;

        public delegate ReadOnlySpan<float3> CameraPositionDelegate();

        public static CameraPositionDelegate GetCameraPositionsCallback;
        #endregion
    }
}