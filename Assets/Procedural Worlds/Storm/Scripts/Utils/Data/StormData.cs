using System;
using System.Collections.Generic;
using System.IO;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;

/// Copyright 2023 Procedural Worlds Ltd
/// Written by Jonathan Williams as part of Storm

namespace ProceduralWorlds.Storm.Utils
{
    public class StormDataLoadingContainer : IDisposable
    {
        public bool IsLoading;
        public JobHandle Handle;

        public void Dispose()
        {
            IsLoading = false;
            Handle.Complete();
        }
    }
    /// <summary>
    /// Base class used for raw tile data
    /// </summary>
    [Serializable]
    public abstract class StormDataBase : IDisposable
    {
        public virtual bool IsValid => false;
        [NonSerialized]
        internal bool IsNew = true;
        [Obsolete("Use InstanceGUID instead.")]
        [HideInInspector]
        public ushort InstanceID = 0;
        [SerializeField]
        [UnityEngine.Serialization.FormerlySerializedAs("InstanceGUID")]
        public uint4 m_instanceGUID;
        public uint4 InstanceGUID
        {
            get {
                if (math.all(m_instanceGUID == uint4.zero))
                    m_instanceGUID = StormUtils.GetNewGUID();
                return m_instanceGUID;
            }
        }
        public GameObject ParentObject = null;

        public bool ShowDebug = false;

        public StormDataLoadingContainer LoadingData;
        public bool IsLoading { get => LoadingData?.IsLoading ?? false; }

        public virtual void OnTileDataUpdate() { }
        public virtual void OnTileEnabled() { }
        public virtual void OnTileDisabled() { }

        public virtual bool HasLayer(string layerName)
        {
            if (!IsValid)
                return false;
            int len = GetLayerCount();
            for (int l = 0; l < len; ++l)
                if (GetLayerName(l) == layerName)
                    return true;
            return false;
        }
        public virtual bool HasLayer(int index)
        {
            if (!IsValid)
                return false;
            int len = GetLayerCount();
            for (int l = 0; l < len; ++l)
                if (GetLayerIndex(l) == index)
                    return true;
            return false;
        }

        public virtual void SetLayers(string[] layers, byte[] priorities)
        {
            int len = layers?.Length ?? 0;
            int prioLen = priorities?.Length ?? 0;
            for (int i = 0; i < len; ++i)
            {
                byte prio = (i < prioLen ? priorities[i] : (byte)0);
                SetLayerName(i, layers[i]);
                SetLayerPriority(i, prio);
            }
        }
        public abstract int GetLayerCount();
        public abstract string GetLayerName(int l);
        public abstract void SetLayerName(int l, string name);
        public abstract int GetLayerIndex(int l);
        public abstract Vector2 GetLayerValueRange(int l);
        public abstract byte GetLayerPriority(int l);
        public abstract void SetLayerPriority(int l, byte val);

        public virtual bool CheckMatch(StormDataBase data)
        {
            int numLayers = data.GetLayerCount();
            if (GetLayerCount() == numLayers)
            {
                bool allMatch = true;
                for (int l = 0; l < numLayers; ++l)
                    if (GetLayerName(l) != data.GetLayerName(l) || GetLayerPriority(l) != data.GetLayerPriority(l))
                    {
                        allMatch = false;
                        break;
                    }
                if (allMatch)
                    return true;
            }
            return false;
        }
        public virtual bool MergeData(StormDataBase data) { return false; }

        /// <summary>
        /// Action invoked when the Texture or Layer Names change
        /// </summary>
        public Action<string> OnChanged;

        ~StormDataBase()
        {
            Dispose();
        }

        /// <summary>
        /// Dispose of all runtime data
        /// </summary>
        public abstract void Dispose();
    }
    public abstract class StormFloatDataBase : StormDataBase
    {
        /// <summary>
        /// Called when StormWorld is initalizing this Data
        /// (may be called during actual init, when feature(set)s change, and any other time StormWorld refreshes data)
        /// </summary>
        public abstract void Init();
        /// <summary>
        /// Return array of Layers this Data contains
        /// </summary>
        /// <returns>Array of Layers</returns>
        public abstract string[] GetLayers();
        /// <summary>
        /// Set the layers for this data
        /// </summary>
        /// <param name="layers">Layers to set for this data</param>
        public abstract void SetLayers(string[] layers);
        /// <summary>
        /// Get NativeArray for requested layer
        /// </summary>
        /// <param name="layerIndex">Index of requested Layer</param>
        /// <returns>NativeArray filled with requested data</returns>
        public abstract NativeArrayRef<float> GetLayer(int layerIndex);
        /// <summary>
        /// Get resolution of data along both axis for requested Layer
        /// </summary>
        /// <param name="layerIndex">Index of requested Layer</param>
        /// <returns>Pixels along each axis of data</returns>
        public abstract int2 GetResolution(int layerIndex);

        /// <summary>
        /// Save data for this data type into a file
        /// </summary>
        /// <param name="filename">File name to save data to</param>
        public virtual void Save(string filename)
        {
            if (File.Exists(filename))
                File.Delete(filename);
            StormUtils.EnsureDirExists(filename);
            Save(File.Create(filename));
        }
        /// <summary>
        /// Save data for this data type into a data stream
        /// </summary>
        /// <param name="stream">Stream to write to</param>
        public abstract void Save(Stream stream);
        /// <summary>
        /// Load data for this data type from a file
        /// </summary>
        /// <param name="filename">File name to load data from</param>
        public virtual void Load(string filename)
        {
            if (File.Exists(filename))
                Load(File.OpenRead(filename));
        }
        /// <summary>
        /// Load data for this data type from a data stream
        /// </summary>
        /// <param name="stream">Stream to read from</param>
        public abstract void Load(Stream stream);


        /// <summary>
        /// Action to invoke when data on a layer changes
        /// </summary>
        public Action<string> OnLayerChanged;
    }
    public abstract class StormInstanceDataBase : StormDataBase
    {
        /// <summary>
        /// Called when StormWorld is initalizing this Data
        /// (may be called during actual init, when feature(set)s change, and any other time StormWorld refreshes data)
        /// </summary>
        public abstract void Init();
        /// <summary>
        /// Return array of Layers this Data contains
        /// </summary>
        /// <returns>Array of Layers</returns>
        public abstract string[] GetLayers();
        /// <summary>
        /// Set the layers for this data
        /// </summary>
        /// <param name="layers">Layers to set for this data</param>
        public abstract void SetLayers(string[] layers);
        /// <summary>
        /// Get NativeArray for requested layer
        /// </summary>
        /// <param name="layerIndex">Index of requested Layer</param>
        /// <param name="position">Lower-Left position of requested area (if needed)</param>
        /// <param name="size">Size of requested area (if needed)</param>
        /// <returns>NativeArray filled with requested data</returns>
        public abstract NativeArrayRef<StormItemData> GetLayer(int layerIndex, float3 position, float3 size);

        /// <summary>
        /// Save data for this data type into a file
        /// </summary>
        /// <param name="filename">File name to save data to</param>
        public virtual void Save(string filename)
        {
            if (File.Exists(filename))
                File.Delete(filename);
            StormUtils.EnsureDirExists(filename);
            Save(File.Create(filename));
        }
        /// <summary>
        /// Save data for this data type into a data stream
        /// </summary>
        /// <param name="stream">Stream to write to</param>
        public abstract void Save(Stream stream);
        /// <summary>
        /// Load data for this data type from a file
        /// </summary>
        /// <param name="filename">File name to load data from</param>
        public virtual void Load(string filename)
        {
            if (File.Exists(filename))
                Load(File.OpenRead(filename));
        }
        /// <summary>
        /// Load data for this data type from a data stream
        /// </summary>
        /// <param name="stream">Stream to read from</param>
        public abstract void Load(Stream stream);

        /// <summary>
        /// Action to invoke when data on a layer changes with layer name and index of instance
        /// </summary>
        public Action<string> OnLayerChanged;
    }
    /// <summary>
    /// Base class for Tile data stored in a Texture2D
    /// </summary>
    [Serializable]
    public abstract class TextureDataBase : StormDataBase
    {
        /// <summary>
        /// Does this Texture Data need to be re-read?
        /// </summary>
        [NonSerialized]
        public bool m_isDirty = true;
        /// <summary>
        /// Internal value for Tex property
        /// </summary>
        [SerializeField]
        private Texture2D _texture;

        /// <summary>
        /// Texture referenced
        /// </summary>
        public Texture2D Tex
        {
            get => _texture;
            set {
                if (_texture != value)
                {
                    _texture = value;
                    SetDirty();
                }
            }
        }

        [SerializeField]
        private Rect _rect = new Rect(0, 0, 1, 1);
        public float4 Rect
        {
            get {
                if (_rect.x == 0 && _rect.y == 0 && _rect.width == 0 && _rect.height == 0)
                {
                    _rect = new Rect(0, 0, 1, 1);
                    SetDirty();
                }
                return math.float4(_rect.x, _rect.y, _rect.width, _rect.height);
            }
            set {
                if (_rect.x != value.x || _rect.y != value.y || _rect.width != value.z || _rect.height != value.w)
                {
                    _rect = new Rect(value.x, value.y, value.z, value.w);
                    SetDirty();
                }
            }
        }

        public override bool IsValid => _texture != null;
        /// <summary>
        /// Set this Texture layer as dirty
        /// </summary>
        public virtual void SetDirty()
        {
            lock (this)
                m_isDirty = true;
        }

        //Internal Native Array values
        [NonSerialized]
        public bool m_isCreated = false;
        [NonSerialized]
        public int2 m_resolution = int2.zero;
        [NonSerialized]
        public int m_len = 0;
        public int2 Resolution => m_isCreated ? m_resolution : (_texture == null ? int2.zero : math.int2((int)math.ceil(_texture.width), (int)math.ceil(_texture.height)));
        public int Width => m_isCreated ? m_resolution.x : (_texture == null ? 0 : (int)math.ceil(_texture.width));
        public int Height => m_isCreated ? m_resolution.y : (_texture == null ? 0 : (int)math.ceil(_texture.height));

        /// <summary>
        /// Dispose of allocated Native Arrays for this Texture Data
        /// </summary>
        public override void Dispose()
        {
            lock (this)
                m_isDirty = true;
            m_isCreated = false;
            m_resolution = int2.zero;
            m_len = 0;
            if (IsLoading)
                LoadingData.Handle.Complete();
        }
    }
    /// <summary>
    /// Tile data stored in a Texture2D where each channel is a dataset (R, G, B, A)
    /// </summary>
    [Serializable]
    public class TextureData : TextureDataBase
    {
        [SerializeField]
        private int _updateVersion = 0;
        /// <summary>
        /// Internal value for Layer R Name
        /// </summary>
        [SerializeField]
        [FormerlySerializedAs("_layerR")]
        [StormLayer("Red")]
        private string _layerNameR;
        [SerializeField]
        private byte _priorityR;
        /// <summary>
        /// Internal value for Layer G Name
        /// </summary>
        [SerializeField]
        [FormerlySerializedAs("_layerG")]
        [StormLayer("Green")]
        private string _layerNameG;
        [SerializeField]
        private byte _priorityG;
        /// <summary>
        /// Internal value for Layer B Name
        /// </summary>
        [SerializeField]
        [FormerlySerializedAs("_layerB")]
        [StormLayer("Blue")]
        private string _layerNameB;
        [SerializeField]
        private byte _priorityB;
        /// <summary>
        /// Internal value for Layer A Name
        /// </summary>
        [SerializeField]
        [FormerlySerializedAs("_layerA")]
        [StormLayer("Alpha")]
        private string _layerNameA;
        [SerializeField]
        private byte _priorityA;

        [NonSerialized]
        public int _layerIndexR = -1;
        [NonSerialized]
        public int _layerIndexG = -1;
        [NonSerialized]
        public int _layerIndexB = -1;
        [NonSerialized]
        public int _layerIndexA = -1;

        [SerializeField]
        private Vector2 _valueRangeR = Vector2.up;
        [SerializeField]
        private Vector2 _valueRangeG = Vector2.up;
        [SerializeField]
        private Vector2 _valueRangeB = Vector2.up;
        [SerializeField]
        private Vector2 _valueRangeA = Vector2.up;

        /// <summary>
        /// Layer Name held in Red channel of Tex
        /// </summary>
        public string LayerNameR
        {
            get => _layerNameR;
            set {
                if (_layerNameR != value)
                {
                    string oldName = _layerNameR;
                    _layerNameR = value;
                    _layerIndexR = StormUtils.GetDataLayerIndex(value, StormLayerType.TEXTURE);
                    OnChanged?.Invoke(oldName);
                    OnChanged?.Invoke(value);
                }
            }
        }
        /// <summary>
        /// Layer Name held in Green channel of Tex
        /// </summary>
        public string LayerNameG
        {
            get => _layerNameG;
            set {
                if (_layerNameG != value)
                {
                    string oldName = _layerNameG;
                    _layerNameG = value;
                    _layerIndexG = StormUtils.GetDataLayerIndex(value, StormLayerType.TEXTURE);
                    OnChanged?.Invoke(oldName);
                    OnChanged?.Invoke(value);
                }
            }
        }
        /// <summary>
        /// Layer Name held in Blue channel of Tex
        /// </summary>
        public string LayerNameB
        {
            get => _layerNameB;
            set {
                if (_layerNameB != value)
                {
                    string oldName = _layerNameB;
                    _layerNameB = value;
                    _layerIndexB = StormUtils.GetDataLayerIndex(value, StormLayerType.TEXTURE);
                    OnChanged?.Invoke(oldName);
                    OnChanged?.Invoke(value);
                }
            }
        }
        /// <summary>
        /// Layer Name held in Alpha channel of Tex
        /// </summary>
        public string LayerNameA
        {
            get => _layerNameA;
            set {
                if (_layerNameA != value)
                {
                    string oldName = _layerNameA;
                    _layerNameA = value;
                    _layerIndexA = StormUtils.GetDataLayerIndex(value, StormLayerType.TEXTURE);
                    OnChanged?.Invoke(oldName);
                    OnChanged?.Invoke(value);
                }
            }
        }
        /// <summary>
        /// Priority of Layer in Red Channel for layer merging
        /// </summary>
        public byte PriorityR
        {
            get => _priorityR;
            set {
                if (_priorityR != value)
                {
                    _priorityR = value;
                    OnChanged?.Invoke(LayerNameR);
                }
            }
        }
        /// <summary>
        /// Priority of Layer in Green Channel for layer merging
        /// </summary>
        public byte PriorityG
        {
            get => _priorityG;
            set {
                if (_priorityG != value)
                {
                    _priorityG = value;
                    OnChanged?.Invoke(LayerNameG);
                }
            }
        }
        /// <summary>
        /// Priority of Layer in Blue Channel for layer merging
        /// </summary>
        public byte PriorityB
        {
            get => _priorityB;
            set {
                if (_priorityB != value)
                {
                    _priorityB = value;
                    OnChanged?.Invoke(LayerNameB);
                }
            }
        }
        /// <summary>
        /// Priority of Layer in Alpha Channel for layer merging
        /// </summary>
        public byte PriorityA
        {
            get => _priorityA;
            set {
                if (_priorityA != value)
                {
                    _priorityA = value;
                    OnChanged?.Invoke(LayerNameA);
                }
            }
        }

        /// <summary>
        /// Value Range of height (as percent of tile bounds) to output for Red channel when getting height
        /// </summary>
        public Vector2 ValueRangeR
        {
            get => _valueRangeR;
            set {
                if (_valueRangeR != value)
                {
                    _valueRangeR = value;
                    OnChanged?.Invoke(LayerNameR);
                }
            }
        }
        /// <summary>
        /// Value Range of height (as percent of tile bounds) to output for Green channel when getting height
        /// </summary>
        public Vector2 ValueRangeG
        {
            get => _valueRangeG;
            set {
                if (_valueRangeG != value)
                {
                    _valueRangeG = value;
                    OnChanged?.Invoke(LayerNameG);
                }
            }
        }
        /// <summary>
        /// Value Range of height (as percent of tile bounds) to output for Blue channel when getting height
        /// </summary>
        public Vector2 ValueRangeB
        {
            get => _valueRangeB;
            set {
                if (_valueRangeB != value)
                {
                    _valueRangeB = value;
                    OnChanged?.Invoke(LayerNameB);
                }
            }
        }
        /// <summary>
        /// Value Range of height (as percent of tile bounds) to output for Alpha channel when getting height
        /// </summary>
        public Vector2 ValueRangeA
        {
            get => _valueRangeA;
            set {
                if (_valueRangeA != value)
                {
                    _valueRangeA = value;
                    OnChanged?.Invoke(LayerNameA);
                }
            }
        }

        [NonSerialized]
        public NativeArrayRef<float> m_arrayR;
        [NonSerialized]
        public NativeArrayRef<float> m_arrayG;
        [NonSerialized]
        public NativeArrayRef<float> m_arrayB;
        [NonSerialized]
        public NativeArrayRef<float> m_arrayA;
        
        public override int GetLayerCount()
        {
            return 4;
        }
        public override void SetLayers(string[] layers, byte[] priorities)
        {
            int len = layers?.Length ?? 0;
            int prioLen = priorities?.Length ?? 0;
            for (int i = 0; i < len && i < 4; ++i)
            {
                byte prio = (i < prioLen ? priorities[i] : (byte)0);
                SetLayerName(i, layers[i]);
                SetLayerPriority(i, prio);
            }
        }
        public override string GetLayerName(int l)
        {
            switch (l)
            {
                case 0:
                    return LayerNameR;
                case 1:
                    return LayerNameG;
                case 2:
                    return LayerNameB;
                case 3:
                    return LayerNameA;
                default:
                    return "";
            }
        }
        public override void SetLayerName(int l, string name)
        {
            switch (l)
            {
                case 0:
                    LayerNameR = name;
                    break;
                case 1:
                    LayerNameG = name;
                    break;
                case 2:
                    LayerNameB = name;
                    break;
                case 3:
                    LayerNameA = name;
                    break;
            }
        }
        public override int GetLayerIndex(int l)
        {
            switch (l)
            {
                case 0:
                    return _layerIndexR;
                case 1:
                    return _layerIndexG;
                case 2:
                    return _layerIndexB;
                case 3:
                    return _layerIndexA;
                default:
                    return -1;
            }
        }
        public override Vector2 GetLayerValueRange(int l)
        {
            switch (l)
            {
                case 0:
                    return _valueRangeR;
                case 1:
                    return _valueRangeG;
                case 2:
                    return _valueRangeB;
                case 3:
                    return _valueRangeA;
                default:
                    return new Vector2(0,1);
            }
        }

        /// <summary>
        /// Set this Texture layer as dirty
        /// </summary>
        public override void SetDirty()
        {
            base.SetDirty();
            OnChanged?.Invoke(LayerNameR);
            OnChanged?.Invoke(LayerNameG);
            OnChanged?.Invoke(LayerNameB);
            OnChanged?.Invoke(LayerNameA);
        }

        public override byte GetLayerPriority(int l)
        {
            return l switch {
                0 => _priorityR,
                1 => _priorityG,
                2 => _priorityB,
                3 => _priorityA,
                _ => _priorityR
            };
        }
        public override void SetLayerPriority(int l, byte val)
        {
            switch (l)
            {
                case 0:
                    PriorityR = val;
                    break;
                case 1:
                    PriorityG = val;
                    break;
                case 2:
                    PriorityB = val;
                    break;
                case 3:
                    PriorityA = val;
                    break;
            }
        }

        public override void OnTileDataUpdate()
        {
            _layerIndexR = StormUtils.GetDataLayerIndex(LayerNameR, StormLayerType.TEXTURE);
            _layerIndexG = StormUtils.GetDataLayerIndex(LayerNameG, StormLayerType.TEXTURE);
            _layerIndexB = StormUtils.GetDataLayerIndex(LayerNameB, StormLayerType.TEXTURE);
            _layerIndexA = StormUtils.GetDataLayerIndex(LayerNameA, StormLayerType.TEXTURE);
        }

        /// <summary>
        /// Dispose of allocated Native Arrays for this Texture Data
        /// </summary>
        public override void Dispose()
        {
            base.Dispose();
            if (m_arrayR != null)
                m_arrayR.Dispose();
            if (m_arrayG != null)
                m_arrayG.Dispose();
            if (m_arrayB != null)
                m_arrayB.Dispose();
            if (m_arrayA != null)
                m_arrayA.Dispose();
            m_arrayR = null;
            m_arrayG = null;
            m_arrayB = null;
            m_arrayA = null;
        }
        public void OnBeforeSerialize()
        {
            if (_updateVersion != 1)
            {
                if (_valueRangeR == Vector2.zero)
                    _valueRangeR = Vector2.up;
                if (_valueRangeG == Vector2.zero)
                    _valueRangeG = Vector2.up;
                if (_valueRangeB == Vector2.zero)
                    _valueRangeB = Vector2.up;
                if (_valueRangeA == Vector2.zero)
                    _valueRangeA = Vector2.up;
                _updateVersion = 1;
            }
        }

        public void OnAfterDeserialize()
        {
            if (_updateVersion != 1)
            {
                if (_valueRangeR == Vector2.zero)
                    _valueRangeR = Vector2.up;
                if (_valueRangeG == Vector2.zero)
                    _valueRangeG = Vector2.up;
                if (_valueRangeB == Vector2.zero)
                    _valueRangeB = Vector2.up;
                if (_valueRangeA == Vector2.zero)
                    _valueRangeA = Vector2.up;
                _updateVersion = 1;
            }
        }
    }
    [Serializable]
    public struct ColorRangeData
    {
        [SerializeField]
        public Color Color;
        [SerializeField]
        public float Range;
        [SerializeField]
        public float Falloff;

        public ColorRangeData(Color c, float r, float f)
        {
            Color = c;
            Range = r;
            Falloff = f;
        }
    }
    [Serializable]
    public struct ColorLayerData
    {
        [SerializeField]
        public Color Color;
        [SerializeField]
        [Range(0, 2)]
        public float ColorRange;
        [SerializeField]
        [Range(0, 2)]
        public float ColorFalloff;
        [SerializeField]
        [StormLayer(null, StormLayerRemapMode.DATA)]
        public string Layer;
        [SerializeField]
        public byte Priority;
        [SerializeField]
        public Vector2 ValueRange;

        public ColorLayerData(ColorRangeData colorData, string layer)
        {
            Color = colorData.Color;
            ColorRange = colorData.Range;
            ColorFalloff = colorData.Falloff;
            Layer = layer;
            Priority = 0;
            ValueRange = Vector2.up;
        }
        public ColorLayerData(ColorRangeData colorData, string layer, Vector2 valueRange)
        {
            Color = colorData.Color;
            ColorRange = colorData.Range;
            ColorFalloff = colorData.Falloff;
            Layer = layer;
            Priority = 0;
            ValueRange = valueRange;
        }
        public ColorLayerData(Color c, float r = 0.15f, float f = 0.1f, string l = "", Vector2 valueRange = default, byte p = 0)
        {
            Color = c;
            ColorRange = r;
            ColorFalloff = f;
            Layer = l;
            Priority = p;
            if (valueRange == default)
                valueRange = Vector2.up;
            ValueRange = valueRange;
        }
    }
    /// <summary>
    /// Tile data stored in a Texture2D
    /// </summary>
    [Serializable]
    public partial class TextureColorData : TextureDataBase
    {
        public StormColorTargetData m_targetData;
        public ColorLayerData[] m_colors = Array.Empty<ColorLayerData>();
        [NonSerialized]
        private int[] m_colorIndexes = Array.Empty<int>();

        public NativeArrayRef<Color32> m_textureData;
        public Texture m_lastTexture;

        [SerializeField]
        private Color _previewColor = Color.red;

        public long LastUpdateTS;

        public Action<TextureColorData, StormColorTargetData> OnTargetDataChanged;

        public override void OnTileDataUpdate()
        {
            m_colorIndexes = new int[m_colors.Length];
            for (int c = 0; c < m_colors.Length; ++c)
                m_colorIndexes[c] = StormUtils.GetDataLayerIndex(m_colors[c].Layer, StormLayerType.TEXTURE);
        }

        public override int GetLayerCount()
        {
            return m_colors.Length;
        }
        public override string GetLayerName(int l)
        {
            if (l < 0 || l >= m_colors.Length)
                return "";
            return m_colors[l].Layer;
        }
        public override void SetLayerName(int l, string name)
        {
            if (l < 0 || l >= m_colors.Length)
            {
                Array.Resize(ref m_colors, l + 1);
                m_colors[l] = new ColorLayerData() { Layer = name, Color = Color.white, ColorRange = 0.1f };
                return;
            }
            ColorLayerData cData = m_colors[l];
            cData.Layer = name;
            m_colors[l] = cData;
            if (m_colorIndexes.Length != m_colors.Length)
            {
                m_colorIndexes = new int[m_colors.Length];
                for (int c = 0; c < m_colors.Length; ++c)
                    m_colorIndexes[c] = StormUtils.GetDataLayerIndex(m_colors[c].Layer, StormLayerType.TEXTURE);
            }
            else
                m_colorIndexes[l] = StormUtils.GetDataLayerIndex(name, StormLayerType.TEXTURE);
        }
        public override int GetLayerIndex(int l)
        {
            if (m_colorIndexes.Length != m_colors.Length)
            {
                m_colorIndexes = new int[m_colors.Length];
                for (int c = 0; c < m_colors.Length; ++c)
                    m_colorIndexes[c] = StormUtils.GetDataLayerIndex(m_colors[c].Layer, StormLayerType.TEXTURE);
            }
            if (l < 0 || l >= m_colorIndexes.Length)
                return -1;
            return m_colorIndexes[l];
        }

        public override byte GetLayerPriority(int l)
        {
            if (l < 0 || l >= m_colors.Length)
                return 0;
            return m_colors[l].Priority;
        }
        public override void SetLayerPriority(int l, byte prio)
        {
            if (l < 0 || l >= m_colors.Length)
                return;
            m_colors[l].Priority = prio;
        }

        public override Vector2 GetLayerValueRange(int l)
        {
            if (l < 0 || l >= m_colors.Length)
                return Vector2.up;
            return m_colors[l].ValueRange;
        }

        /// <summary>
        /// Dispose of allocated Native Arrays for this Texture Data
        /// </summary>
        public override void Dispose()
        {
            base.Dispose();
            if (m_textureData != null)
                m_textureData.Dispose();
            m_textureData = null;
            m_colorIndexes = Array.Empty<int>();
        }
        public override void SetDirty()
        {
            base.SetDirty();
            m_colorIndexes = Array.Empty<int>();
        }
    }
    /// <summary>
    /// Tile data stored as a 2D float array
    /// </summary>
    [Serializable]
    public class FloatData : StormDataBase, ISerializationCallbackReceiver
    {
        [SerializeField]
        private int _updateVersion = 0;
        [SerializeField]
        [FormerlySerializedAs("_layer")]
        [StormLayer]
        private string _layerName;
        /// <summary>
        /// Raw Data referenced (X, Z) stored as ushort to save data
        /// </summary>
        [SerializeField]
        [HideInInspector]
        public ushort[] m_data;

        public int Width;
        /// <summary>
        /// Texture this data was exported to (if exported from terrain tile)
        /// </summary>
        [SerializeField]
        public Texture2D Tex;
        [SerializeField]
        private byte _priority;
        [SerializeField]
        private Vector2 _valueRange = Vector2.up;
        [NonSerialized]
        public int _layerIndex;

        /// <summary>
        /// Priority of Layer for layer merging
        /// </summary>
        public byte Priority
        {
            get => _priority;
            set {
                if (_priority != value)
                {
                    _priority = value;
                    OnChanged?.Invoke(_layerName);
                }
            }
        }
        /// <summary>
        /// Value Range of Layer
        /// </summary>
        public Vector2 ValueRange
        {
            get => _valueRange;
            set {
                if (_valueRange != value)
                {
                    _valueRange = value;
                    OnChanged?.Invoke(_layerName);
                }
            }
        }
        /// <summary>
        /// Layer name of Data
        /// </summary>
        public string LayerName
        {
            get => _layerName;
            set {
                if (_layerName != value)
                {
                    string oldName = _layerName;
                    _layerName = value;
                    _layerIndex = StormUtils.GetDataLayerIndex(value, StormLayerType.FLOAT);
                    OnChanged?.Invoke(oldName);
                    OnChanged?.Invoke(_layerName);
                }
            }
        }

        public override bool IsValid => m_data != null;
        [NonSerialized]
        public bool m_isDirty = true;
        /// <summary>
        /// Set this Texture layer as dirty
        /// </summary>
        public void SetDirty()
        {
            lock (this)
                m_isDirty = true;
            OnChanged?.Invoke(_layerName);
        }
        public int2 Resolution => math.int2(Width, Mathf.FloorToInt(m_data.Length / (float)Width));

        [NonSerialized]
        public NativeArrayRef<float> m_array;

        public override void OnTileDataUpdate()
        {
            _layerIndex = StormUtils.GetDataLayerIndex(LayerName, StormLayerType.FLOAT);
        }

        public override int GetLayerCount() => 1;
        public override string GetLayerName(int l) => LayerName;
        public override void SetLayerName(int l, string name)
        {
            if (l == 0)
                LayerName = name;
        }
        public override int GetLayerIndex(int l) => _layerIndex;
        public override byte GetLayerPriority(int l) => Priority;
        public override void SetLayerPriority(int l, byte val)
        {
            if (l == 0)
                Priority = val;
        }
        public override Vector2 GetLayerValueRange(int l) => ValueRange;


        public override void Dispose()
        {
            if (m_array != null)
                m_array.Dispose();
            m_array = null;
        }

        public void OnBeforeSerialize()
        {
            if (_updateVersion != 1 && _valueRange == Vector2.zero)
            {
                _valueRange = Vector2.up;
                _updateVersion = 1;
            }
        }

        public void OnAfterDeserialize()
        {
            if (_updateVersion != 1 && _valueRange == Vector2.zero)
            {
                _valueRange = Vector2.up;
                _updateVersion = 1;
            }
        }
    }
    /// <summary>
    /// Tile data containing TRS data
    /// </summary>
    [Serializable]
    public class TRSData : StormDataBase
    {
        [SerializeField]
        [FormerlySerializedAs("_layer")]
        [StormLayer]
        private string _layerName;
        [SerializeField]
        private StormTRSList _data;
        [NonSerialized]
        public int _layerIndex;

        /// <summary>
        /// TRS Data referenced
        /// </summary>
        public StormTRSList Data
        {
            get => _data;
            set {
                if (_data != value)
                {
                    if (_data != null)
                    {
                        _data.OnTransformChanged -= OnDataChanged;
                        _data.OnTransformListChanged -= SetDirty;
                    }
                    _data = value;
                    value.OnTransformChanged -= OnDataChanged;
                    value.OnTransformListChanged -= SetDirty;
                    value.OnTransformChanged += OnDataChanged;
                    value.OnTransformListChanged += SetDirty;
                    SetDirty();
                }
            }
        }
        /// <summary>
        /// Layer name in Data
        /// </summary>
        public string LayerName
        {
            get => _layerName;
            set {
                if (_layerName != value)
                {
                    string oldName = _layerName;
                    _layerName = value;
                    _layerIndex = StormUtils.GetDataLayerIndex(value, StormLayerType.TRS);
                    OnChanged?.Invoke(oldName);
                    InvokeOnChanged();
                }
            }
        }

        public override bool IsValid => _data != null;

        [HideInInspector]
        public List<int> UpdatedInstances = new List<int>();

        public TRSData()
        {
            if (Data != null)
            {
                Data.OnTransformChanged -= OnDataChanged;
                Data.OnTransformListChanged -= SetDirty;
                Data.OnTransformChanged += OnDataChanged;
                Data.OnTransformListChanged += SetDirty;
            }
        }
        ~TRSData()
        {
            if (Data != null)
            {
                Data.OnTransformChanged -= OnDataChanged;
                Data.OnTransformListChanged -= SetDirty;
            }
            Dispose();
        }
        private void OnDataChanged(int i)
        {
            if (i < 0)
            {
                lock (this)
                    m_isDirty = true;
            }
            else
                StormUtils.AddIfNew(UpdatedInstances, i);
            SetDirty();
        }
        public void SetDirty()
        {
            lock (this)
                m_isDirty = true;
            OnChanged?.Invoke(_layerName);
        }

        public override void OnTileDataUpdate()
        {
            _layerIndex = StormUtils.GetDataLayerIndex(LayerName, StormLayerType.TRS);
        }

        public override int GetLayerCount() => 1;
        public override string GetLayerName(int l) => LayerName;
        public override void SetLayerName(int l, string name)
        {
            if (l == 0)
                LayerName = name;
        }
        public override int GetLayerIndex(int l) => _layerIndex;
        public override byte GetLayerPriority(int l) => 1;
        public override void SetLayerPriority(int l, byte val)
        {
        }
        public override Vector2 GetLayerValueRange(int l) => Vector2.up;


        private void InvokeOnChanged()
        {
            OnChanged?.Invoke(_layerName);
        }
        [NonSerialized]
        public bool m_isDirty = true;
        [NonSerialized]
        public NativeArrayRef<StormItemData> m_array;
        public override void Dispose()
        {
            if (m_array != null)
                m_array.Dispose();
            m_array = null;
            UpdatedInstances.Clear();
            UpdatedInstances.TrimExcess();
        }
    }
    /// <summary>
    /// Tile data containing data about Transform objects
    /// </summary>
    [Serializable]
    public class TransformData : StormDataBase
    {
        [SerializeField]
        [FormerlySerializedAs("_layer")]
        [StormLayer]
        private string _layerName;
        [NonSerialized]
        public int _layerIndex;

        /// <summary>
        /// Layer name in Data
        /// </summary>
        public string LayerName
        {
            get => _layerName;
            set {
                if (_layerName != value)
                {
                    string oldName = _layerName;
                    _layerName = value;
                    _layerIndex = StormUtils.GetDataLayerIndex(value, StormLayerType.TRANSFORM);
                    OnChanged?.Invoke(oldName);
                    OnChanged?.Invoke(value);
                }
            }
        }
        [SerializeField]
        public GameObject PrefabBase;
        [SerializeField]
        [FormerlySerializedAs("ExportedTRSList")]
        private StormTRSList _exportedTRSList;
        public StormTRSList ExportedTRSList
        {
            get => _exportedTRSList;
            set {
                if (value != _exportedTRSList)
                {
                    _exportedTRSList = value;
                    if (value != null && _data != null)
                    {
                        if (_exportedTRSList != null)
                        {
                            _exportedTRSList.OnTransformChanged -= OnDataChanged;
                            _exportedTRSList.OnTransformListChanged -= SetDirty;
                        }
                        _exportedTRSList = value;
                        value.OnTransformChanged -= OnDataChanged;
                        value.OnTransformListChanged -= SetDirty;
                        value.OnTransformChanged += OnDataChanged;
                        value.OnTransformListChanged += SetDirty;
                        OnDataChanged(-1);
                    }
                }
            }
        }
        [SerializeField]
        private StormTransformList _data;
        /// <summary>
        /// TRS Data referenced
        /// </summary>
        public StormTransformList Data
        {
            get {
                return _data;
            }
            set {
                if (_data != value)
                {
                    if (_data != null)
                        _data.OnItemChanged -= OnDataChanged;
                    _data = value;
                    _data.OnItemChanged -= OnDataChanged;
                    _data.OnItemChanged += OnDataChanged;
                    OnDataChanged(-1);
                }
            }
        }

        public override bool IsValid => _data != null || ExportedTRSList != null;

        [HideInInspector]
        public List<int> UpdatedInstances = new List<int>();

        public TransformData()
        {
            if (_data != null)
            {
                _data.OnItemChanged -= OnDataChanged;
                _data.OnItemChanged += OnDataChanged;
            }
        }
        ~TransformData()
        {
            Dispose();
        }
        public void SetDirty()
        {
            lock (this)
                m_isDirty = true;
        }
        private void OnDataChanged(int idx)
        {
            if (idx < 0)
            {
                m_isDirty = true;
            }
            else if (idx < m_array.Length)
            {
                m_array[idx] = Data.GetItem(idx);
                StormUtils.AddIfNew(UpdatedInstances, idx);
            }
            OnChanged?.Invoke(_layerName);
        }

        public override void OnTileDataUpdate()
        {
            _layerIndex = StormUtils.GetDataLayerIndex(LayerName, StormLayerType.TRANSFORM);
        }

        public override int GetLayerCount() => 1;
        public override string GetLayerName(int l) => LayerName;
        public override void SetLayerName(int l, string name)
        {
            if (l == 0)
                LayerName = name;
        }
        public override int GetLayerIndex(int l) => _layerIndex;
        public override byte GetLayerPriority(int l) => 1;
        public override void SetLayerPriority(int l, byte val)
        {
        }
        public override Vector2 GetLayerValueRange(int l) => Vector2.up;

        public override void OnTileEnabled()
        {
            if (_data != null)
            {
                var items = _data.Transforms;
                int itemsCount = items.Length;
                for (int i = 0; i < itemsCount; ++i)
                {
                    if (items[i] != null)
                        items[i].gameObject.SetActive(false);
                }
            }
        }
        public override void OnTileDisabled()
        {
            if (_data != null)
            {
                var items = _data.Transforms;
                int itemsCount = items.Length;
                for (int i = 0; i < itemsCount; ++i)
                {
                    if (items[i] != null)
                        items[i].gameObject.SetActive(true);
                }
            }
        }

        [NonSerialized]
        public bool m_isDirty = true;
        [NonSerialized]
        public NativeArrayRef<StormItemData> m_array;
        public override void Dispose()
        {
            if (Data != null)
            {
                Data.OnItemChanged -= OnDataChanged;
            }
            if (m_array != null)
                m_array.Dispose();
            m_array = null;
        }
    }
    /// <summary>
    /// Tile data stored as a 2D float array
    /// </summary>
    [Serializable]
    public class PolygonData : StormDataBase, ISerializationCallbackReceiver
    {
        [SerializeField]
        private int _updateVersion = 0;
        [SerializeField]
        [FormerlySerializedAs("_layer")]
        [StormLayer("")]
        private string _layerName;
        [NonSerialized]
        public int _layerIndex;

        /// <summary>
        /// Polygons in 2-d Array
        /// </summary>
        [SerializeField]
        public PolyPointSet[] Data;
        [SerializeField]
        public float[] Values;
        [SerializeField]
        public float[] Falloffs;

        [SerializeField]
        private byte _priority;
        [SerializeField]
        private Vector2 _valueRange = Vector2.up;
        /// <summary>
        /// Priority of Layer for layer merging
        /// </summary>
        public byte Priority
        {
            get => _priority;
            set {
                if (_priority != value)
                {
                    _priority = value;
                    OnChanged?.Invoke(_layerName);
                }
            }
        }
        /// <summary>
        /// Priority of Layer for layer merging
        /// </summary>
        public Vector2 ValueRange
        {
            get => _valueRange;
            set {
                if (_valueRange != value)
                {
                    _valueRange = value;
                    OnChanged?.Invoke(_layerName);
                }
            }
        }
        /// <summary>
        /// Layer name of Data
        /// </summary>
        public string LayerName
        {
            get => _layerName;
            set {
                if (_layerName != value)
                {
                    string oldName = _layerName;
                    _layerName = value;
                    _layerIndex = StormUtils.GetDataLayerIndex(value, StormLayerType.POLYGON);
                    OnChanged?.Invoke(oldName);
                    OnChanged?.Invoke(_layerName);
                }
            }
        }

        public override bool IsValid => Data != null;

        [NonSerialized]
        public bool m_isDirty = true;
        /// <summary>
        /// Set this Texture layer as dirty
        /// </summary>
        public void SetDirty()
        {
            if (!m_isDirty)
            {
                m_isDirty = true;
                OnChanged?.Invoke(_layerName);
            }
        }

        public override void OnTileDataUpdate()
        {
            _layerIndex = StormUtils.GetDataLayerIndex(LayerName, StormLayerType.POLYGON);
        }

        public override int GetLayerCount() => 1;
        public override string GetLayerName(int l) => LayerName;
        public override void SetLayerName(int l, string name)
        {
            if (l == 0)
                LayerName = name;
        }
        public override int GetLayerIndex(int l) => _layerIndex;
        public override byte GetLayerPriority(int l) => Priority;
        public override void SetLayerPriority(int l, byte val)
        {
            if (l == 0)
                Priority = val;
        }
        public override Vector2 GetLayerValueRange(int l) => ValueRange;

        public override bool MergeData(StormDataBase data)
        {
            if (data is PolygonData pData)
            {
                int newPolys = pData.Data.Length;
                int numPolys = Data.Length;
                bool[] skipPolys = new bool[newPolys];
                int addPolys = newPolys;
                for (int x = 0; x < newPolys; ++x)
                {
                    for (int y = 0; y < numPolys; ++y)
                    {
                        if (StormUtils.Match(pData.Data[x].Points, Data[y].Points))
                        {
                            skipPolys[x] = true;
                            --addPolys;
                            break;
                        }
                    }
                }
                if ((Falloffs?.Length ?? 0) < numPolys + addPolys)
                {
                    int startSize = (Falloffs?.Length ?? 0);
                    float defaultVal = startSize > 0 ? Falloffs[startSize - 1] : 0;
                    Array.Resize(ref Falloffs, numPolys + addPolys);
                    for (int x = startSize; x < numPolys; ++x)
                        Falloffs[x] = defaultVal;
                }
                if ((Values?.Length ?? 0) < numPolys + addPolys)
                {
                    int startSize = (Values?.Length ?? 0);
                    float defaultVal = startSize > 0 ? Values[startSize - 1] : 1;
                    Array.Resize(ref Values, numPolys + newPolys);
                    for (int x = startSize; x < numPolys; ++x)
                        Values[x] = defaultVal;
                }
                Array.Resize(ref Data, numPolys + addPolys);
                int i = 0;
                for (int x = 0; x < newPolys; ++x)
                {
                    if (skipPolys[x])
                        continue;
                    Vector2[] points = new Vector2[pData.Data[i].Points.Length];
                    pData.Data[i].Points.CopyTo(points);
                    Data[numPolys + i] = new PolyPointSet(points);
                    Values[numPolys + i] = ((pData.Values?.Length ?? 0) == 0) ? Values[numPolys - 1] : (pData.Values.Length > i ? pData.Values[i] : pData.Values[pData.Values.Length - 1]);
                    Falloffs[numPolys + i] = ((pData.Falloffs?.Length ?? 0) == 0) ? Falloffs[numPolys - 1] : (pData.Falloffs.Length > i ? pData.Falloffs[i] : pData.Values[pData.Falloffs.Length - 1]);
                    ++i;
                }
                return true;
            }
            return false;
        }

        public override void Dispose()
        {
            m_isDirty = true;
        }

        public static bool operator ==(PolygonData a, PolygonData b)
        {
            if (a is null || b is null)
                return a is null && b is null;
            if (!StormUtils.Match(a.Data, b.Data))
                return false;
            if (!StormUtils.Match(a.Falloffs, b.Falloffs))
                return false;
            if (!StormUtils.Match(a.Values, b.Values))
                return false;
            return true;
        }
        public static bool operator !=(PolygonData a, PolygonData b)
        {
            return !(a == b);
        }
        public override bool Equals(object obj)
        {
            if (obj is PolygonData other)
                return this == other;
            return false;
        }
        public override int GetHashCode()
        {
            return Data.GetHashCode();
        }

        public void OnBeforeSerialize()
        {
            if (_updateVersion != 1 && _valueRange == Vector2.zero)
            {
                _valueRange = Vector2.up;
                _updateVersion = 1;
            }
        }

        public void OnAfterDeserialize()
        {
            if (_updateVersion != 1 && _valueRange == Vector2.zero)
            {
                _valueRange = Vector2.up;
                _updateVersion = 1;
            }
        }
    }

    [Serializable]
    public class TerrainSourceData : StormDataBase, ISerializationCallbackReceiver
    {
        [SerializeField]
        private int _updateVersion = 0;
        public enum TerrainSource
        {
            HEIGHT,
            TEXTURE,
            DETAIL,
            TREE,
            HOLES,
        }
        public enum TextureColor
        {
            Red = 0,
            Green = 1,
            Blue = 2,
            Alpha = 3,
        }

        [SerializeField]
        [StormLayer]
        private string _layerName;
        [NonSerialized]
        public int _layerIndex;
        /// <summary>
        /// Layer name of Data
        /// </summary>
        public string LayerName
        {
            get => _layerName;
            set {
                if (_layerName != value)
                {
                    string oldName = _layerName;
                    _layerName = value;
                    _layerIndex = StormUtils.GetDataLayerIndex(value, _source == TerrainSource.TREE ? StormLayerType.TRS : StormLayerType.TEXTURE);
                    OnChanged?.Invoke(oldName);
                    OnChanged?.Invoke(_layerName);
                }
            }
        }
        [SerializeField]
        private byte _priority;
        /// <summary>
        /// Priority of Layer for layer merging
        /// </summary>
        public byte Priority
        {
            get => _priority;
            set {
                if (_priority != value)
                {
                    _priority = value;
                    OnChanged?.Invoke(_layerName);
                }
            }
        }

        [SerializeField]
        private TerrainData _terrain;
        public TerrainData Terrain
        {
            get => _terrain;
            set {
                if (_terrain != value)
                {
                    _terrain = value;
                    SetDirty();
                }
            }
        }
        public long TerrainUpdateTime;

        [SerializeField]
        private TerrainSource _source;
        public TerrainSource Source
        {
            get => _source;
            set {
                if (_source != value)
                {
                    _source = value;
                    OnChanged?.Invoke(_layerName);
                }
            }
        }
        [SerializeField]
        private int _sourceIndex;
        public int SourceIndex
        {
            get => _sourceIndex;
            set {
                if (_sourceIndex != value)
                {
                    _sourceIndex = value;
                    SetDirty();
                }
            }
        }
        [SerializeField]
        private Rect _sourceArea;
        public Rect SourceArea
        {
            get => _sourceArea;
            set {
                if (_sourceArea != value)
                {
                    _sourceArea = value;
                    SetDirty();
                }
            }
        }

        [SerializeField]
        [Obsolete("Use ValueRange instead")]
        private float _sourceYOffset;
        [Obsolete("Use ValueRange instead")]
        public float SourceYOffset
        {
            get => ValueRange.x;
            set {
                if (ValueRange.x != value)
                {
                    ValueRange = new Vector2(value, ValueRange.y);
                    SetDirty();
                }
            }
        }

        [SerializeField]
        private Vector2 _valueRange = Vector2.up;
        public Vector2 ValueRange
        {
            get => _valueRange;
            set {
                if (_valueRange != value)
                {
                    _valueRange = value;
                    SetDirty();
                }
            }
        }
        

        [SerializeField]
        private Texture2D _exportedTexture = null;
        public Texture2D ExportedTexture
        {
            get => _exportedTexture;
            set {
                if (_exportedTexture != value)
                {
                    _exportedTexture = value;
                    SetDirty();
                }
            }
        }

        [SerializeField]
        private float2 _exportedOffset = float2.zero;
        public float2 ExportedOffset
        {
            get => _exportedOffset;
            set {
                if (math.any(_exportedOffset != value))
                {
                    _exportedOffset = value;
                    SetDirty();
                }
            }
        }

        [SerializeField]
        private TextureColor _exportedTextureColor;
        public TextureColor ExportedTextureColor
        {
            get => _exportedTextureColor;
            set {
                if (_exportedTextureColor != value)
                {
                    _exportedTextureColor = value;
                    SetDirty();
                }
            }
        }

        [SerializeField]
        private StormTRSList _exportedTRS = null;
        public StormTRSList ExportedTRS
        {
            get => _exportedTRS;
            set {
                if (_exportedTRS != value)
                {
                    ExportedTRS = value;
                    SetDirty();
                }
            }
        }

        public override bool IsValid
        {
            get {
                if (_sourceArea.width <= 0 || _sourceArea.height <= 0)
                    return false;
                if (_terrain != null)
                {
                    switch (_source)
                    {
                        case TerrainSource.HEIGHT:
                        case TerrainSource.HOLES:
                            return true;
                        case TerrainSource.TEXTURE:
                            return _sourceIndex < _terrain.terrainLayers.Length && _sourceIndex >= 0;
                        case TerrainSource.DETAIL:
                            return _sourceIndex < _terrain.detailPrototypes.Length && _sourceIndex >= 0;
                        case TerrainSource.TREE:
                            return _sourceIndex < _terrain.treePrototypes.Length && _sourceIndex >= 0;
                    }
                }
                return false;
            }
        }

        [NonSerialized]
        public bool m_isDirty = true;

        [NonSerialized]
        public NativeArrayRef<float> _floatData;
        [NonSerialized]
        public int2 Resolution;
        [NonSerialized]
        public NativeListRef<StormItemData> _instanceData;

        [NonSerialized]
        public new bool IsLoading;
        [NonSerialized]
        public JobHandle LoadingJob;

        [NonSerialized]
        public Vector3 last_tilePosition;
        public Vector3 last_tileSize;

        public override void OnTileDataUpdate()
        {
            _layerIndex = StormUtils.GetDataLayerIndex(LayerName, Source == TerrainSource.TREE ? StormLayerType.TRS : StormLayerType.TEXTURE);
        }

        public override int GetLayerCount() => 1;
        public override string GetLayerName(int l) => LayerName;
        public override void SetLayerName(int l, string name)
        {
            if (l == 0)
                LayerName = name;
        }
        public override int GetLayerIndex(int l) => _layerIndex;
        public override byte GetLayerPriority(int l) => Priority;
        public override void SetLayerPriority(int l, byte val)
        {
            if (l == 0)
                Priority = val;
        }
        public override Vector2 GetLayerValueRange(int l) => ValueRange;

        public override bool CheckMatch(StormDataBase data)
        {
            if (base.CheckMatch(data) && data is TerrainSourceData tData)
                return
                    Terrain == tData.Terrain &&
                    LayerName == tData.LayerName &&
                    Source == tData.Source &&
                    SourceIndex == tData.SourceIndex &&
                    SourceArea == tData.SourceArea &&
                    ValueRange == tData.ValueRange;
            return false;
        }

        /// <summary>
        /// Set this layer as dirty
        /// </summary>
        public void SetDirty()
        {
            if (!m_isDirty)
            {
                Dispose();
                OnChanged?.Invoke(_layerName);
            }
        }
        public void Use()
        {
            if (_floatData != null)
                _floatData.Use();
            if (_instanceData != null)
                _instanceData.Use();
        }
        public override void Dispose()
        {
            m_isDirty = true;
            if (_floatData != null)
                _floatData.Dispose();
            if (_instanceData != null)
                _instanceData.Dispose();
            _floatData = null;
            _instanceData = null;
        }

        public void OnBeforeSerialize()
        {
            if (_updateVersion != 1 && _valueRange == Vector2.zero)
            {
                _valueRange = Vector2.up;
                _updateVersion = 1;
            }
        }

        public void OnAfterDeserialize()
        {
#pragma warning disable CS0618 // Type or member is obsolete
            if (_sourceYOffset != -1)
            {
                _valueRange = new Vector2(_sourceYOffset, 1);
                _sourceYOffset = -1;
            }
#pragma warning restore CS0618 // Type or member is obsolete
            if (_updateVersion != 1 && _valueRange == Vector2.zero)
            {
                _valueRange = Vector2.up;
                _updateVersion = 1;
            }
        }
    }
}