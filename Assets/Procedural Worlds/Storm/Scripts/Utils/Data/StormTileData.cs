using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

/// Copyright 2023 Procedural Worlds Ltd
/// Written by Jonathan Williams as part of Storm

namespace ProceduralWorlds.Storm.Utils
{
    /// <summary>
    /// Holds information about layers and TRS data for a 'tile' in the world.
    /// </summary>
    [Serializable]
    public class StormTileData
    {
        /// <summary>
        /// ID used for this TileData source for Instance IDs
        /// </summary>
        [HideInInspector]
        [Obsolete("Use sourceGUID instead")]
        public ulong source_ID;
        [SerializeField]
        [HideInInspector]
        [UnityEngine.Serialization.FormerlySerializedAs("sourceGUID")]
        public uint4 m_sourceGUID;
        public uint4 sourceGUID
        {
            get {
                if (math.all(m_sourceGUID == uint4.zero))
                {
                    Debug.LogWarning("Invalid GUID found: " + m_sourceGUID);
                    m_sourceGUID = StormUtils.GetNewGUID();
                    StormUtils.SetAssetDirty(m_myObject);
                }
                return m_sourceGUID;
            }
        }
        /// <summary>
        /// Position of this Tile in world space
        /// </summary>
        [HideInInspector]
        [Tooltip("Position of this Tile in world space")]
        public Vector3 Position;
        /// <summary>
        /// Size of this Tile in world space
        /// </summary>
        [HideInInspector]
        [Tooltip("Size of this Tile in world space")]
        public Vector3 Size;
        /// <summary>
        /// Helper to generate a Bounds of this tile from the Position and Size
        /// </summary>
        public Bounds Bounds => new Bounds(Position + (Size * 0.5f), Size);
        [HideInInspector]
        public Bounds LastBounds;
        /// <summary>
        /// Texture data set on this tile
        /// </summary>
        [Tooltip("Texture data set on this tile")]
        public TextureData[] Textures = Array.Empty<TextureData>();
        /// <summary>
        /// Texture Color data set on this tile
        /// </summary>
        [Tooltip("Texture Color data set on this tile")]
        public TextureColorData[] TextureColors = Array.Empty<TextureColorData>();
        /// <summary>
        /// Raw data set on this tile
        /// </summary>
        [Tooltip("Raw data set on this tile")]
        public FloatData[] RawData = Array.Empty<FloatData>();
        /// <summary>
        /// TRS data set on this tile
        /// </summary>
        [Tooltip("TRS data set on this tile")]
        public TRSData[] TRSData = Array.Empty<TRSData>();
        /// <summary>
        /// Transform data set on this Tile
        /// </summary>
        [Tooltip("Transform data set on this Tile")]
        public TransformData[] TransformData = Array.Empty<TransformData>();
        /// <summary>
        /// Transform data set on this Tile
        /// </summary>
        [Tooltip("Transform data set on this Tile")]
        public PolygonData[] PolygonData = Array.Empty<PolygonData>();
        /// <summary>
        /// Terrain data on this Tile
        /// </summary>
        [Tooltip("Terrain data on this Tile")]
        public TerrainSourceData[] TerrainData = Array.Empty<TerrainSourceData>();
        /// <summary>
        /// Other Instance Data set on this Tile
        /// </summary>
        [Tooltip("Other Instance Data set on this Tile")]
        public StormInstanceDataBase[] OtherInstanceData = Array.Empty<StormInstanceDataBase>();
        /// <summary>
        /// Other Float Data set on this Tile
        /// </summary>
        [Tooltip("Other Float Data set on this Tile")]
        public StormFloatDataBase[] OtherFloatData = Array.Empty<StormFloatDataBase>();

        /// <summary>
        /// Action to invoke when changing a layer's data
        /// </summary>
        public Action<Bounds, string> OnLayerChanged;
        public Action OnTileEnabled;
        public Action OnTileDisabled;

        /// <summary>
        /// Should this tile accept polygon data added through StormWorld.AddPolygonToTiles functions?
        /// </summary>
        [Tooltip("Should this tile accept polygon data added through StormWorld.AddPolygonToTiles functions?")]
        public bool AcceptAddedPolygonData = true;

        /// <summary>
        /// Array of Terrains this tile ingested (to disable items on when storm is active)
        /// </summary>
        [HideInInspector]
        [Tooltip("Array of Terrains this tile ingested (to disable items on when storm is active)")]
        public UnityEngine.Terrain[] IngestedTerrains = new UnityEngine.Terrain[0];

        private object _curWorld;
        public object CurrentWorld
        {
            get => _curWorld;
            set {
                _curWorld = value;
            }
        }
        [HideInInspector]
        public GameObject m_myObject;

        public StormTileData()
        {
            if (math.all(m_sourceGUID == uint4.zero))
                m_sourceGUID = StormUtils.GetNewGUID();
            UpdateDatasets();
            LastBounds = Bounds;
        }

        private string[] m_layerNames;
        public string[] GetLayerNames()
        {
            if (m_layerNames == null)
            {
                List<string> layerNames = new List<string>(1);
                AddLayerNames(Textures, layerNames);
                AddLayerNames(TextureColors, layerNames);
                AddLayerNames(RawData, layerNames);
                AddLayerNames(TRSData, layerNames);
                AddLayerNames(TransformData, layerNames);
                AddLayerNames(PolygonData, layerNames);
                AddLayerNames(TerrainData, layerNames);
                AddLayerNames(OtherFloatData, layerNames);
                AddLayerNames(OtherInstanceData, layerNames);
                layerNames.Sort();
                m_layerNames = layerNames.ToArray();
            }
            return m_layerNames;
        }
        void AddLayerNames<T>(T[] array, List<string> list) where T : StormDataBase
        {
            int len = array.Length;
            for (int i = 0; i < len; ++i)
            {
                StormDataBase data = array[i];
                int cnt = data.GetLayerCount();
                for (int l = 0; l < cnt; ++l)
                    StormUtils.AddIfNew(list, data.GetLayerName(l));
            }
        }
        public bool HasData()
        {
            return (Textures?.Length > 0 || TextureColors?.Length > 0 || RawData?.Length > 0 || TRSData?.Length > 0 || TransformData?.Length > 0 || PolygonData?.Length > 0 || TerrainData?.Length > 0 || OtherFloatData?.Length > 0 || OtherInstanceData?.Length > 0);
        }

        public void UpdateDatasets(bool sendUpdates = true)
        {
            LastBounds = Bounds;
            TerrainCallbacks.heightmapChanged -= OnTerrainHeightChanged;
            TerrainCallbacks.heightmapChanged += OnTerrainHeightChanged;
            TerrainCallbacks.textureChanged -= OnTerrainTextureChanged;
            TerrainCallbacks.textureChanged += OnTerrainTextureChanged;
            StormAPI.OnLayerIndexesChanged -= OnLayerIndexesChanged;
            StormAPI.OnLayerIndexesChanged += OnLayerIndexesChanged;
            bool changed = false;
            DoArrayUpdate(Textures, sendUpdates, ref changed);
            DoArrayUpdate(TextureColors, sendUpdates, ref changed);
            DoArrayUpdate(RawData, sendUpdates, ref changed);
            DoArrayUpdate(TRSData, sendUpdates, ref changed);
            DoArrayUpdate(TransformData, sendUpdates, ref changed);
            DoArrayUpdate(PolygonData, sendUpdates, ref changed);
            DoArrayUpdate(TerrainData, sendUpdates, ref changed);
            DoArrayUpdate(OtherFloatData, sendUpdates, ref changed);
            DoArrayUpdate(OtherInstanceData, sendUpdates, ref changed);
            if (changed && m_myObject != null)
            {
                StormUtils.SetAssetDirty(m_myObject);
            }
        }
        void DoArrayUpdate(StormDataBase[] array, bool sendUpdates, ref bool changed)
        {
            int len = array.Length;
            for (int l = 0; l < len; ++l)
            {
                var data = array[l];
                if (data == null)
                {
                    changed = true;
                    for (int i = l; i < array.Length - 1; ++i)
                        array[i] = array[i + 1];
                    Array.Resize(ref array, array.Length - 1);
                    --l;
                    --len;
                    continue;
                }
                data.ParentObject = m_myObject;
                data.OnTileDataUpdate();
                if (data.IsNew)
                    data.IsNew = false;
                if (sendUpdates)
                {
                    int numLayers = data.GetLayerCount();
                    for (int n = 0; n < numLayers; ++n)
                        DoLayerChanged(data.GetLayerName(n));
                }
                data.OnChanged -= DoLayerChanged;
                data.OnChanged += DoLayerChanged;
                OnTileEnabled -= data.OnTileEnabled;
                OnTileEnabled += data.OnTileEnabled;
                OnTileDisabled -= data.OnTileDisabled;
                OnTileDisabled += data.OnTileDisabled;
            }
        }
        public StormDataBase GetData(uint4 id)
        {
            if (GetData(id, Textures, out StormDataBase ret))
                return ret;
            if (GetData(id, TextureColors, out ret))
                return ret;
            if (GetData(id, RawData, out ret))
                return ret;
            if (GetData(id, TRSData, out ret))
                return ret;
            if (GetData(id, TransformData, out ret))
                return ret;
            if (GetData(id, PolygonData, out ret))
                return ret;
            if (GetData(id, TerrainData, out ret))
                return ret;
            if (GetData(id, OtherFloatData, out ret))
                return ret;
            if (GetData(id, OtherInstanceData, out ret))
                return ret;
            return null;
        }
        bool GetData<T>(uint4 id, T[] array, out StormDataBase data) where T : StormDataBase
        {
            if (array != null)
            {
                for (int d = 0; d < array.Length; ++d)
                {
                    if (array[d] != null && math.all(array[d].InstanceGUID == id))
                    {
                        data = array[d];
                        return true;
                    }
                }
            }
            data = null;
            return false;
        }
        [Obsolete("Use GetData(uint4) instead")]
        public StormDataBase GetData(ushort id)
        {
            if (GetData(id, Textures, out StormDataBase ret))
                return ret;
            if (GetData(id, TextureColors, out ret))
                return ret;
            if (GetData(id, RawData, out ret))
                return ret;
            if (GetData(id, TRSData, out ret))
                return ret;
            if (GetData(id, TransformData, out ret))
                return ret;
            if (GetData(id, PolygonData, out ret))
                return ret;
            if (GetData(id, TerrainData, out ret))
                return ret;
            if (GetData(id, OtherFloatData, out ret))
                return ret;
            if (GetData(id, OtherInstanceData, out ret))
                return ret;
            return null;
        }
        [Obsolete("Use GetData(uint4) instead")]
        bool GetData<T>(ushort id, T[] array, out StormDataBase data) where T : StormDataBase
        {
            if (array != null)
            {
                for (int d = 0; d < array.Length; ++d)
                {
                    if (array[d] != null && array[d].InstanceID == id)
                    {
                        data = array[d];
                        return true;
                    }
                }
            }
            data = null;
            return false;
        }

        public void Dispose()
        {
            TerrainCallbacks.heightmapChanged -= OnTerrainHeightChanged;
            TerrainCallbacks.textureChanged -= OnTerrainTextureChanged;
            StormAPI.OnLayerIndexesChanged -= OnLayerIndexesChanged;

            if (Textures != null)
                for (int i = 0; i < Textures.Length; ++i)
                    if (Textures[i] != null)
                        Textures[i].Dispose();
            if (TextureColors != null)
                for (int i = 0; i < TextureColors.Length; ++i)
                    if (TextureColors[i] != null)
                        TextureColors[i].Dispose();
            if (RawData != null)
                for (int i = 0; i < RawData.Length; ++i)
                    if (RawData[i] != null)
                        RawData[i].Dispose();
            if (TRSData != null)
                for (int i = 0; i < TRSData.Length; ++i)
                    if (TRSData[i] != null)
                        TRSData[i].Dispose();
            if (TransformData != null)
                for (int i = 0; i < TransformData.Length; ++i)
                    if (TransformData[i] != null)
                        TransformData[i].Dispose();
            if (PolygonData != null)
                for (int i = 0; i < PolygonData.Length; ++i)
                    if (PolygonData[i] != null)
                        PolygonData[i].Dispose();
            if (TerrainData != null)
                for (int i = 0; i < TerrainData.Length; ++i)
                    if (TerrainData[i] != null)
                        TerrainData[i].Dispose();
            if (OtherFloatData != null)
                for (int i = 0; i < OtherFloatData.Length; ++i)
                    if (OtherFloatData[i] != null)
                        OtherFloatData[i].Dispose();
            if (OtherInstanceData != null)
                for (int i = 0; i < OtherInstanceData.Length; ++i)
                    if (OtherInstanceData[i] != null)
                        OtherInstanceData[i].Dispose();
        }

        void OnLayerIndexesChanged()
        {
            UpdateDatasets(false);
        }
        #region Internal Functions
        public void DoLayerChanged(string layer)
        {
            m_layerNames = null;
            Bounds newBounds = Bounds;
            LastBounds.Encapsulate(newBounds);
            OnLayerChanged?.Invoke(LastBounds, layer);
            LastBounds = newBounds;
        }
        public void AddLayerData(StormDataBase data, bool merge = true)
        {
            if (!data.IsValid)
                return; //invalid data ... no sense adding it
            if (data is TerrainSourceData tsData)
                CheckAddLayer(tsData, ref TerrainData, merge);
            else if (data is PolygonData pData)
                CheckAddLayer(pData, ref PolygonData, merge);
            else if (data is TRSData trsData)
                CheckAddLayer(trsData, ref TRSData, merge);
            else if (data is TransformData trData)
                CheckAddLayer(trData, ref TransformData, merge);
            else if (data is FloatData fData)
                CheckAddLayer(fData, ref RawData, merge);
            else if (data is TextureData texData)
                CheckAddLayer(texData, ref Textures, merge);
            else if (data is TextureColorData cData)
                CheckAddLayer(cData, ref TextureColors, merge);
            else if (data is StormFloatDataBase ofData)
                CheckAddLayer(ofData, ref OtherFloatData, merge);
            else if (data is StormInstanceDataBase oiData)
                CheckAddLayer(oiData, ref OtherInstanceData, merge);
            else
                return;
            UpdateDatasets();
        }
        void CheckAddLayer<T>(T data, ref T[] array, bool merge) where T : StormDataBase
        {
            int len = array.Length;
            for (int i = 0; i < len; ++i)
            {
                if (array[i].CheckMatch(data))
                {
                    if (merge)
                        array[i].MergeData(data);
                    return; //found a match already.
                }
            }
            Array.Resize(ref array, len + 1);
            array[len] = data;
        }
        public void RemoveLayerData(StormDataBase data)
        {
            if (data is TerrainSourceData tsData)
                RemoveLayerData(tsData, ref TerrainData);
            if (data is PolygonData pData)
                RemoveLayerData(pData, ref PolygonData);
            if (data is TRSData tData)
                RemoveLayerData(tData, ref TRSData);
            else if (data is TransformData trData)
                RemoveLayerData(trData, ref TransformData);
            else if (data is FloatData fData)
                RemoveLayerData(fData, ref RawData);
            else if (data is TextureData xData)
                RemoveLayerData(xData, ref Textures);
            else if (data is TextureColorData cData)
                RemoveLayerData(cData, ref TextureColors);
            else if (data is StormFloatDataBase ofData)
                RemoveLayerData(ofData, ref OtherFloatData);
            else if (data is StormInstanceDataBase oiData)
                RemoveLayerData(oiData, ref OtherInstanceData);
        }
        void RemoveLayerData<T>(T data, ref T[] array) where T : StormDataBase
        {
            for (int t = 0; t < array.Length; ++t)
            {
                if (array[t] == data)
                {
                    StormUtils.RemoveAt(ref array, t);
                    int len = data.GetLayerCount();
                    for (int l = 0; l < len; ++l)
                    {
                        string layer = data.GetLayerName(l);
                        if (!string.IsNullOrEmpty(layer))
                            DoLayerChanged(layer);
                    }
                    return;
                }
            }
        }

        public void OnTerrainHeightChanged(UnityEngine.Terrain terrain, RectInt heightRegion, bool synched)
        {
            int hRes = terrain.terrainData.heightmapResolution;
            for (int d = 0; d < TerrainData.Length; ++d)
            {
                if (!TerrainData[d].m_isDirty && terrain.terrainData == TerrainData[d].Terrain && (TerrainData[d].Source == TerrainSourceData.TerrainSource.HEIGHT || TerrainData[d].Source == TerrainSourceData.TerrainSource.TREE))
                {
                    Rect tArea = new Rect(heightRegion.xMin / (float)hRes, heightRegion.yMin / (float)hRes, heightRegion.width / (float)hRes, heightRegion.height / (float)hRes);
                    if (TerrainData[d].SourceArea.Overlaps(tArea))
                    {
                        terrain.Flush();
                        TerrainData[d].SetDirty();
                    }
                }
            }
        }
        public void OnTerrainTextureChanged(UnityEngine.Terrain terrain, string textureName, RectInt texelRegion, bool synched)
        {
            for (int d = 0; d < TerrainData.Length; ++d)
            {
                if (!TerrainData[d].m_isDirty && terrain.terrainData == TerrainData[d].Terrain)
                {
                    if (TerrainData[d].Source == TerrainSourceData.TerrainSource.TEXTURE)
                    {
                        if (textureName != "alphamap" || terrain.terrainData.terrainLayers.Length <= TerrainData[d].SourceIndex || TerrainData[d].SourceIndex < 0)
                            continue;
                        float tRes = terrain.terrainData.alphamapResolution;
                        Rect tArea = new Rect(texelRegion.xMin / tRes, texelRegion.yMin / tRes, texelRegion.width / tRes, texelRegion.height / tRes);
                        if (TerrainData[d].SourceArea.Overlaps(tArea))
                        {
                            terrain.Flush();
                            TerrainData[d].SetDirty();
                        }
                    }
                }
            }
        }
        #endregion
    }
}