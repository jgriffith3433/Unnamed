using UnityEditor;
using UnityEngine;
using ProceduralWorlds.Storm.Utils;

namespace ProceduralWorlds.Storm.StormEditor
{
    public class StormEditorSettings : ScriptableObject
    {
        private static StormEditorSettings m_instance = null;
        public static StormEditorSettings Instance
        {
            get {
                if (m_instance == null)
                {
                    StormEditorSettings settings = null;
                    string[] assets = AssetDatabase.FindAssets("t:StormEditorSettings");
                    if (assets.Length > 0)
                    {
                        settings = AssetDatabase.LoadAssetAtPath<StormEditorSettings>(AssetDatabase.GUIDToAssetPath(assets[0]));
                    }
                    if (settings == null)
                    {
                        settings = CreateInstance<StormEditorSettings>();
                        StormUtils.SaveAsset(settings, "Assets/Storm User Data/StormEditorSettings.asset");
                    }
                    m_instance = settings;
                }
                return m_instance;
            }
        }

        [SerializeField]
        int m_ingestTileSize = 1024;
        [SerializeField]
        bool m_ingestExpandTile = true;
        [SerializeField]
        bool m_ingestClearTile = false;
        [SerializeField]
        bool m_ingestCaptureAlphamaps = true;
        [SerializeField]
        bool m_ingestIgnoreDisabled = false;
        [SerializeField]
        bool m_ingestCreateFeatures = false;
        [SerializeField]
        bool m_ingestCreateFeaturesets = false;
        [SerializeField]
        bool m_ingestSingleFile = false;
        [SerializeField]
        bool m_ingestRawAsTexture = false;
        [SerializeField]
        int m_ingestSourceType = 1;
        [SerializeField]
        string m_ingestFilePath = "Assets/";
        [SerializeField]
        int m_ingestTargetType = 0;
        [SerializeField]
        StormRegion m_globalRegion;
        [SerializeField]
        StormFeatureset m_globalFeatureset;
        [SerializeField]
        int m_globalFeaturesetIndex;
        [SerializeField]
        float m_globalRegionAddReq = 0.5f;

        [SerializeField]
        bool m_ingestManualBounds = false;
        [SerializeField]
        Bounds m_ingestBounds;
        [SerializeField]
        bool m_ignoreTRSInstanceCount = true;
        [SerializeField]
        bool m_disableTerrainFoliageRendering = true;
        [SerializeField]
        bool m_deleteEmptyStormTiles = true;
        [SerializeField]
        int m_transformInstanceCount = 50;

        [SerializeField]
        bool m_bakeManualBounds = false;
        [SerializeField]
        Bounds m_bakeBounds;
        //[SerializeField]
        //float m_bakeTileSize = 256;
        [SerializeField]
        int m_bakeSaveFrequency = 6;
        [SerializeField]
        GameObject m_bakePrefab = null;
        [SerializeField]
        int m_bakeSaveType = 0;
        [SerializeField]
        SceneAsset m_bakeSaveScene = null;
        [SerializeField]
        string m_bakeSavePath = "Assets/";
        [SerializeField]
        string m_bakeSavePrefix = "BakedColliders";
        [SerializeField]
        float m_bakeSaveSceneSize = 4096;

        public int IngestTileSize
        {
            get => m_ingestTileSize;
            set {
                if (m_ingestTileSize != value)
                {
                    m_ingestTileSize = value;
                    EditorUtility.SetDirty(this);
                }
            }
        }
        public bool IngestResizeTile
        {
            get => m_ingestExpandTile;
            set {
                if (m_ingestExpandTile != value)
                {
                    m_ingestExpandTile = value;
                    EditorUtility.SetDirty(this);
                }
            }
        }
        public bool IngestClearTile
        {
            get => m_ingestClearTile;
            set {
                if (m_ingestClearTile != value)
                {
                    m_ingestClearTile = value;
                    EditorUtility.SetDirty(this);
                }
            }
        }
        public bool IngestCaptureAlphamaps
        {
            get => m_ingestCaptureAlphamaps;
            set {
                if (m_ingestCaptureAlphamaps != value)
                {
                    m_ingestCaptureAlphamaps = value;
                    EditorUtility.SetDirty(this);
                }
            }
        }
        public bool IngestCaptureDisabled
        {
            get => !m_ingestIgnoreDisabled;
            set {
                if (m_ingestIgnoreDisabled == value)
                {
                    m_ingestIgnoreDisabled = !value;
                    EditorUtility.SetDirty(this);
                }
            }
        }
        public bool IngestCreateFeatures
        {
            get => m_ingestCreateFeatures;
            set {
                if (m_ingestCreateFeatures != value)
                {
                    m_ingestCreateFeatures = value;
                    EditorUtility.SetDirty(this);
                }
            }
        }
        public bool IngestCreateFeaturesets
        {
            get => m_ingestCreateFeaturesets;
            set {
                if (m_ingestCreateFeaturesets != value)
                {
                    m_ingestCreateFeaturesets = value;
                    EditorUtility.SetDirty(this);
                }
            }
        }
        public bool IngestSingleFile
        {
            get => m_ingestSingleFile;
            set {
                if (m_ingestSingleFile != value)
                {
                    m_ingestSingleFile = value;
                    EditorUtility.SetDirty(this);
                }
            }
        }
        public bool IngestRawAsTexture
        {
            get => m_ingestRawAsTexture;
            set {
                if (m_ingestRawAsTexture != value)
                {
                    m_ingestRawAsTexture = value;
                    EditorUtility.SetDirty(this);
                }
            }
        }
        public int IngestSourceType
        {
            get => m_ingestSourceType;
            set {
                if (m_ingestSourceType != value)
                {
                    m_ingestSourceType = value;
                    EditorUtility.SetDirty(this);
                }
            }
        }
        public string IngestFilePath
        {
            get => m_ingestFilePath;
            set {
                if (m_ingestFilePath != value)
                {
                    m_ingestFilePath = value;
                    EditorUtility.SetDirty(this);
                }
            }
        }
        public int IngestTargetType
        {
            get => m_ingestTargetType;
            set {
                if (m_ingestTargetType != value)
                {
                    m_ingestTargetType = value;
                    EditorUtility.SetDirty(this);
                }
            }
        }
        [System.NonSerialized]
        public bool CheckGlobalRegion = true;
        [System.NonSerialized]
        public bool CheckGlobalFeatureset = true;
        public StormRegion GlobalRegion
        {
            get {
                if (m_globalRegion == null && CheckGlobalRegion)
                {
                    if (Core.StormWorld.Instance != null && Core.StormWorld.Instance.Regions.Count > 0)
                    {
                        m_globalRegion = Core.StormWorld.Instance.Regions[0];
                        m_globalFeatureset = null;
                        EditorUtility.SetDirty(this);
                    }
                }
                CheckGlobalRegion = false;
                return m_globalRegion;
            }
            set {
                if (m_globalRegion != value)
                {
                    m_globalRegion = value;
                    m_globalFeatureset = null;
                    if (value != null)
                        CheckGlobalFeatureset = true;
                    EditorUtility.SetDirty(this);
                }
            }
        }
        public StormFeatureset GlobalFeatureset
        {
            get {
                if (m_globalFeatureset == null && GlobalRegion != null && CheckGlobalFeatureset)
                {
                    if (GlobalRegion.Featuresets?.Length > 0)
                    {
                        for (int fs = 0; fs < GlobalRegion.Featuresets.Length; ++fs)
                        {
                            if (GlobalRegion.Featuresets[fs] != null && GlobalRegion.Featuresets[fs].SpawnLayers?.Length == 0)
                            {
                                m_globalFeatureset = GlobalRegion.Featuresets[fs];
                                EditorUtility.SetDirty(this);
                                m_globalFeaturesetIndex = fs;
                                return m_globalFeatureset;
                            }
                        }
                        if (GlobalRegion.Featuresets[0] == null)
                        {
                            m_globalFeatureset = CreateInstance<StormFeatureset>();
                            m_globalFeatureset.name = "Global Featurset";
                            GlobalRegion.Featuresets[0] = m_globalFeatureset;
                            m_globalFeaturesetIndex = 0;
                        }
                    }
                    else
                    {
                        GlobalRegion.Featuresets = new StormFeatureset[1];
                        m_globalFeatureset = CreateInstance<StormFeatureset>();
                        m_globalFeatureset.name = "Global Featureset";
                        GlobalRegion.Featuresets[0] = m_globalFeatureset;
                        m_globalFeaturesetIndex = 0;
                    }
                    EditorUtility.SetDirty(this);
                }
                CheckGlobalFeatureset = false;
                return m_globalFeatureset;
            }
            set {
                if (m_globalFeatureset != value)
                {
                    m_globalFeaturesetIndex = ArrayUtility.IndexOf(m_globalRegion.Featuresets, m_globalFeatureset);
                    m_globalFeatureset = value;
                    CheckGlobalFeatureset = false;
                    EditorUtility.SetDirty(this);
                }
            }
        }
        public float GlobalRegionAddReq
        {
            get => m_globalRegionAddReq;
            set {
                if (m_globalRegionAddReq != value)
                {
                    m_globalRegionAddReq = value;
                    EditorUtility.SetDirty(this);
                }
            }
        }
        public bool IngestManualBounds
        {
            get => m_ingestManualBounds;
            set {
                if (m_ingestManualBounds != value)
                {
                    m_ingestManualBounds = value;
                    EditorUtility.SetDirty(this);
                }
            }
        }
        public Bounds IngestBounds
        {
            get => m_ingestBounds;
            set {
                if (m_ingestBounds != value)
                {
                    m_ingestBounds = value;
                    EditorUtility.SetDirty(this);
                }
            }
        }
        public bool IgnoreTRSInstanceCount
        {
            get => m_ignoreTRSInstanceCount;
            set
            {
                if (m_ignoreTRSInstanceCount != value)
                {
                    m_ignoreTRSInstanceCount = value;
                    EditorUtility.SetDirty(this);
                }
            }
        }
        public bool DisableTerrainFoliageRendering
        {
            get => m_disableTerrainFoliageRendering;
            set
            {
                if (m_disableTerrainFoliageRendering != value)
                {
                    m_disableTerrainFoliageRendering = value;
                    EditorUtility.SetDirty(this);
                }
            }
        }
        public bool DeleteEmptyStormTiles
        {
            get => m_deleteEmptyStormTiles;
            set
            {
                if (m_deleteEmptyStormTiles != value)
                {
                    m_deleteEmptyStormTiles = value;
                    EditorUtility.SetDirty(this);
                }
            }
        }
        public int TransformInstanceCount
        {
            get => m_transformInstanceCount;
            set
            {
                if (m_transformInstanceCount != value)
                {
                    m_transformInstanceCount = value;
                    EditorUtility.SetDirty(this);
                }
            }
        }
        public bool BakeManualBounds
        {
            get => m_bakeManualBounds;
            set {
                if (m_bakeManualBounds != value)
                {
                    m_bakeManualBounds = value;
                    EditorUtility.SetDirty(this);
                }
            }
        }
        public Bounds BakeBounds
        {
            get => m_bakeBounds;
            set {
                if (m_bakeBounds != value)
                {
                    m_bakeBounds = value;
                    EditorUtility.SetDirty(this);
                }
            }
        }
        public float BakeTileSize
        {
            get => Core.StormSettings.SystemSettings.m_renderCellSize;// m_bakeTileSize;
            /*
            set {
                if (m_bakeTileSize != value)
                {
                    m_bakeTileSize = value;
                    EditorUtility.SetDirty(this);
                }
            }
            //*/
        }
        public int BakeSaveFrequency
        {
            get => m_bakeSaveFrequency;
            set {
                if (m_bakeSaveFrequency != value)
                {
                    m_bakeSaveFrequency = value;
                    EditorUtility.SetDirty(this);
                }
            }
        }
        public GameObject BakePrefab
        {
            get => m_bakePrefab;
            set {
                if (m_bakePrefab != value)
                {
                    m_bakePrefab = value;
                    EditorUtility.SetDirty(this);
                }
            }
        }
        public float BakeSaveSceneSize
        {
            get => m_bakeSaveSceneSize;
            set {
                if (m_bakeSaveSceneSize != value)
                {
                    m_bakeSaveSceneSize = value;
                    EditorUtility.SetDirty(this);
                }
            }
        }
        /// <summary>
        /// Save Type to use
        /// 0 = single
        /// 1 = one each
        /// 2 = area based
        /// </summary>
        public int BakeSaveType
        {
            get => m_bakeSaveType;
            set {
                if (m_bakeSaveType != value)
                {
                    m_bakeSaveType = value;
                    EditorUtility.SetDirty(this);
                }
            }
        }
        public string BakeSavePath
        {
            get => m_bakeSavePath;
            set {
                if (m_bakeSavePath != value)
                {
                    m_bakeSavePath = value;
                    EditorUtility.SetDirty(this);
                }
            }
        }
        public string BakeSavePrefix
        {
            get => m_bakeSavePrefix;
            set {
                if (m_bakeSavePrefix != value)
                {
                    m_bakeSavePrefix = value;
                    EditorUtility.SetDirty(this);
                }
            }
        }
        public SceneAsset BakeSaveScene
        {
            get => m_bakeSaveScene;
            set {
                if (m_bakeSaveScene != value)
                {
                    m_bakeSaveScene = value;
                    EditorUtility.SetDirty(this);
                }
            }
        }



        GameObject m_featureCreationPrefab;
        StormFeatureType m_featureFeatureType = StormFeatureType.LARGE_OBJECT;
        string m_featureSavePath = StormUtils.DefaultSavePath + Core.StormAssetUtils.FeatureSubPath;
        bool m_featureAddToFeatureSet = false;
        StormFeatureset m_featureFeatureSet;
        [StormLayer(null, StormLayerRemapMode.FEATURESET)]
        [SerializeField]
        string m_featureLayerName;
        bool m_featureSelectAfterCreation = true;


        public GameObject FeatureCreationPrefab
        {
            get => m_featureCreationPrefab;
            set {
                if (m_featureCreationPrefab != value)
                {
                    m_featureCreationPrefab = value;
                    if (value != null)
                        m_featureLayerName = value.name;
                    EditorUtility.SetDirty(this);
                }
            }
        }
        public string FeatureCreationLayerName
        {
            get => m_featureLayerName;
            set {
                if (m_featureLayerName != value)
                {
                    m_featureLayerName = value;
                    EditorUtility.SetDirty(this);
                }
            }
        }
        public StormFeatureType FeatureFeatureType
        {
            get => m_featureFeatureType;
            set {
                if (m_featureFeatureType != value)
                {
                    m_featureFeatureType = value;
                    EditorUtility.SetDirty(this);
                }
            }
        }
        public string FeatureSavePath
        {
            get => m_featureSavePath;
            set {
                if (m_featureSavePath != value)
                {
                    m_featureSavePath = value;
                    EditorUtility.SetDirty(this);
                }
            }
        }
        public bool FeatureAddToFeatureSet
        {
            get => m_featureAddToFeatureSet;
            set {
                if (m_featureAddToFeatureSet != value)
                {
                    m_featureAddToFeatureSet = value;
                    EditorUtility.SetDirty(this);
                }
            }
        }
        public StormFeatureset FeatureFeatureSet
        {
            get => m_featureFeatureSet;
            set {
                if (m_featureFeatureSet != value)
                {
                    m_featureFeatureSet = value;
                    EditorUtility.SetDirty(this);
                }
            }
        }
        public bool FeatureSelectAfterCreation
        {
            get => m_featureSelectAfterCreation;
            set {
                if (m_featureSelectAfterCreation != value)
                {
                    m_featureSelectAfterCreation = value;
                    EditorUtility.SetDirty(this);
                }
            }
        }

        [SerializeField]
        private bool m_tileSplitPolygons = true;
        [SerializeField]
        private bool m_tileSplitTextures = false;
        [SerializeField]
        private bool m_tileSplitTRS = true;
        [SerializeField]
        private bool m_tileSplitTerrain = false;
        [SerializeField]
        private bool m_tileSplitRemoveMerged = false;
        [SerializeField]
        private Vector2Int m_tileSplitCount = Vector2Int.one * 2;
        [SerializeField]
        private string[] m_tileBlacklistedLayers = new string[0];

        public bool TileSplitPolygons
        {
            get => m_tileSplitPolygons;
            set {
                if (m_tileSplitPolygons != value)
                {
                    m_tileSplitPolygons = value;
                    EditorUtility.SetDirty(this);
                }
            }
        }
        public bool TileSplitTextures
        {
            get => m_tileSplitTextures;
            set {
                if (m_tileSplitTextures != value)
                {
                    m_tileSplitTextures = value;
                    EditorUtility.SetDirty(this);
                }
            }
        }
        public bool TileSplitTRS
        {
            get => m_tileSplitTRS;
            set {
                if (m_tileSplitTRS != value)
                {
                    m_tileSplitTRS = value;
                    EditorUtility.SetDirty(this);
                }
            }
        }
        public bool TileSplitTerrain
        {
            get => m_tileSplitTerrain;
            set {
                if (m_tileSplitTerrain != value)
                {
                    m_tileSplitTerrain = value;
                    EditorUtility.SetDirty(this);
                }
            }
        }
        public bool TileSplitRemoveMerged
        {
            get => m_tileSplitRemoveMerged;
            set {
                if (m_tileSplitRemoveMerged != value)
                {
                    m_tileSplitRemoveMerged = value;
                    EditorUtility.SetDirty(this);
                }
            }
        }
        public Vector2Int TileSplitCount
        {
            get => m_tileSplitCount;
            set {
                if (m_tileSplitCount != value)
                {
                    m_tileSplitCount = value;
                    EditorUtility.SetDirty(this);
                }
            }
        }
        public string[] TileBlacklistedLayers
        {
            get => m_tileBlacklistedLayers;
            set {
                if (m_tileBlacklistedLayers != value)
                {
                    m_tileBlacklistedLayers = value;
                    EditorUtility.SetDirty(this);
                }
            }
        }


        [SerializeField]
        float m_stormLoaderVisualMapHeight = 200f;
        public float StormLoaderVisualMapHeight
        {
            get => m_stormLoaderVisualMapHeight;
            set {
                if (m_stormLoaderVisualMapHeight != value)
                {
                    m_stormLoaderVisualMapHeight = value;
                    EditorUtility.SetDirty(this);
                }
            }
        }
    }
}