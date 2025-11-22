using System.Collections.Generic;
using System.Linq;
using ProceduralWorlds.Storm.Terrain;
using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using static ProceduralWorlds.Storm.Terrain.StormVTEnums;
using UnityEditor.SceneManagement;
using PWCommon5;
using ProceduralWorlds.Storm.StormEditor;
using UnityEngine.TextCore.Text;

public class StormVTWindow : StormPluginEditorBase, IPWEditor
{
    public override GUIContent Label => new GUIContent("Storm Terrain");
    bool m_isEmbedded;
    public override bool IsEmbedded
    {
        get => m_isEmbedded;
        set => m_isEmbedded = value;
    }

    private TabSet m_mainTabs;
    private EditorUtils m_editorUtils;
    private Vector2 m_scrollPosition = Vector2.zero;
    private GUIStyle m_boxStyle;

    public StormVTSettings stormVTSettings;
    public StormVTTerrainGrid stormVTTerrainGrid;
    public StormVTTerrainTextureLayers stormVTTerrainTextureLayers;
    
    private GameObject vtPrefab;
    private StormVTManager stormVTManager;
    private StormVTClipmap clipmap;
    private StormVTPageTableClipmap pageTableClipmap;
    private StormVTPhysicalTextureManager physicalTextureManager;
    private StormVTTerrainTile terrainTile;
    private StormVTQuadTree quadTree;
    private StormVTTerrainManager terrainManager;
    private StormVTTerrainTextureManager textureManager;
    private StormVTTextureLoadingSystem textureLoadingSystem;
    private StormVTTerrainColliderManager colliderManager;
    private StormVTTextureCompression textureCompression;
    private StormVTFeedbackManager feedbackManager;
    private StormVTFeedbackClipmap feedbackClipmap;
    private StormVTDecalManager decalManager;

    //Storm Settings
    public StormVTEnums.StormVTPlatform m_targetPlatform = StormVTEnums.StormVTPlatform.PC;
    public StormVTEnums.StormVTMode m_stormVTMode = StormVTEnums.StormVTMode.UnityTerrain;
    public StormVTEnums.TileCount m_tileWidth = StormVTEnums.TileCount._32;
    public StormVTEnums.TileCount m_tileHeight = StormVTEnums.TileCount._32;
    public StormVTEnums.TileSize m_tileSize = StormVTEnums.TileSize._256;
    public bool m_floatingPointFix = false;
    public float m_floatingPointRange = 1000;
    public bool m_bakeSceneObjects = false;
    public bool m_snowEnabled;
    public StormVTEnums.StormVTFeatureMode m_snowGenerationMode;
    public float m_snowStartHeight;
    public float m_snowFalloff;
    public float m_snowCurvatureInfluence;
    public bool m_stochasticTiling;
    //Displacement
    public bool m_displacement = false;
    [Range(0.0f, 1.0f)]
    public float m_displacementAmount = 0.15f;
    public float m_displacementDistance = 500f;
    public StormVTEnums.StormVTClipmapDensity m_clipmapDensity = StormVTClipmapDensity._1_Vertex_Per_Meter;
    public bool m_heightBlend;
    public float m_heightBlendFactor;
    public bool m_debugTiles = false;
    public bool m_toggleTerrain = false;
    public bool m_disableGaiaTerrainLoadedScenes = false;

    //Collision
    public bool m_collisionEnabled = true;
    public bool m_debugColliderMeshes = false;
    public StormVTEnums.StormVTCollisionResolution m_collisionQuality = StormVTCollisionResolution._Half;
    public int m_collisionMeshResolution = 64;
    public float m_colliderLoadingRange = 250.0f;

    //Data Loading
    public int m_maxParallelTasks = 8;
    public int m_chunkSize = 32768;
    [Range(1, 20)]
    public int m_maxParallelChunks = 8;
    public float m_lowResloadingRange = 1000f;
    public float m_highResloadingRange = 500f;
    public int m_lowResPoolCount = 10;
    public int m_highResPoolCount = 4;

    //Imposter features
    public bool m_grassMapColor = false;
    public bool m_grassMapNormal = false;
    public bool m_treeMapColor = false;
    public bool m_treeMapNormal = false;
    public float m_grassBlendDistance = 1000f;
    public float m_grassBlendAlpha = 1f;
    public float m_grassBlendSharpness = 0f;
    public float m_grassBlendNormalAlpha = 0f;
    public float m_treesBlendDistance = 1000f;
    public float m_treesBlendAlpha = 1f;
    public float m_treesBlendSharpness = 0f;
    public float m_treesBlendNormalAlpha = 0f;




    public bool m_stripUnusedTexturesOnBuild = false;

    public bool PositionChecked { get; set; }

    private StormVTTerrainTextureLayersEditor m_terrainLayerEditor;

    [MenuItem("Window/Procedural Worlds/Storm/Storm Terrain Manager...", false, 20)]
    public static void ShowWindow()
    {
        if (EmbeddedInstance.TryGetValue(typeof(StormVTWindow), out var win) && win != null)
            win.OnOpen?.Invoke();

        if (!WindowInstance.TryGetValue(typeof(StormVTWindow), out win) || win == null)
        {
            win = GetWindow<StormVTWindow>(false, "Storm Terrain Manager");
            while (win.IsEmbedded)
            {
                win.Close();
                win = GetWindow<StormVTWindow>(false, "Storm Terrain Manager");
            }
        }
        Vector2 initialSize = new Vector2(600f, 450f);
        win.position = new Rect(new Vector2((Screen.currentResolution.width - initialSize.x) / 2f, (Screen.currentResolution.height - initialSize.y) / 2f), initialSize);
        WindowInstance[typeof(StormVTWindow)] = win;
        win.Show();
    }
    public override void OpenWindow()
    {
        ShowWindow();
    }
    public override void HideWindow()
    {
        if (WindowInstance.TryGetValue(typeof(StormVTWindow), out var win) && win != null)
            win.Close();
        WindowInstance.Remove(typeof(StormVTWindow));
    }

    /// <summary>
    /// Create tabs for the window
    /// </summary>
    private void OnEnable()
    {

        if (m_editorUtils == null)
        {
            // Get editor utils for this
            m_editorUtils = StormApp.GetEditorUtils(this);
        }



        Tab[] mainTabs = new Tab[]
            {
                new Tab ("MainSettingsTab", MainSettingsTab),
                new Tab ("FeaturesTab", FeaturesTab),
                new Tab ("LayersTab", LayersTab),
            };
        m_mainTabs = new TabSet(m_editorUtils, mainTabs);



        StormVTEditorUtility.PerformMaintenance();
    }
    private void OnDestroy()
    {
        if (WindowInstance.TryGetValue(typeof(StormVTWindow), out var win) && win == this)
            WindowInstance.Remove(typeof(StormVTWindow));
        if (EmbeddedInstance.TryGetValue(typeof(StormVTWindow), out win) && win == this)
            EmbeddedInstance.Remove(typeof(StormVTWindow));
    }

    public bool FindStormTerrainManagerInScene()
    {
        if(stormVTManager != null)
        {
            return true;
        }
        else
        {
            stormVTManager = StormVTUtilities.FindObjectByType<StormVTManager>();
            if(stormVTManager != null )
            {
                return true;
            }
            else
            {
                return false;
            }
        }
            
    }

    /// <summary>
    /// Configuration panel
    /// </summary>
    /// <param name="helpActive"></param>
    private void ConfigurationPanel(bool helpActive)
    {
        EditorGUILayout.BeginVertical(m_boxStyle);


        
        string sceneName = SceneManager.GetActiveScene().name;
        string m_userDataFolderPath = $"{StormVTConstants.GetAssetsFolder(sceneName)}";
        string m_oldUserDataFolderPath = $"{StormVTConstants.GetAssetsFolderOld(sceneName)}";


        if (stormVTSettings == null)
        {

            if(FindStormTerrainManagerInScene() && stormVTManager.stormVTSettings != null)
            {
                stormVTSettings = stormVTManager.stormVTSettings;
            }
            else
            {
                string vtSettingsAssetFilePath = $"{sceneName}_StormVTSettings.asset";
                string vtSettingsFilePath = $"{m_userDataFolderPath}/{vtSettingsAssetFilePath}";

                if (!System.IO.File.Exists(vtSettingsFilePath))
                {
                    vtSettingsFilePath = $"{m_oldUserDataFolderPath}/{vtSettingsAssetFilePath}";
                }

                stormVTSettings = AssetDatabase.LoadAssetAtPath<StormVTSettings>(vtSettingsFilePath);
            }

            
        }
        if (stormVTTerrainGrid == null)
        {
            if (FindStormTerrainManagerInScene() && stormVTManager.stormVTTerrainGrid != null)
            {
                stormVTTerrainGrid = stormVTManager.stormVTTerrainGrid;
            }
            else
            {
                string vtGridAssetFilePath = $"{sceneName}_TerrainGrid.asset";
                string vtGridFilePath = $"{m_userDataFolderPath}/{vtGridAssetFilePath}";

                if (!System.IO.File.Exists(vtGridFilePath))
                {
                    vtGridFilePath = $"{m_oldUserDataFolderPath}/{vtGridAssetFilePath}"; ;
                }

                stormVTTerrainGrid = AssetDatabase.LoadAssetAtPath<StormVTTerrainGrid>(vtGridFilePath);
            }
                
        }
        if (stormVTTerrainTextureLayers == null)
        {
            if (FindStormTerrainManagerInScene() && stormVTManager.stormVTTerrainTextureLayers != null)
            {
                stormVTTerrainTextureLayers = stormVTManager.stormVTTerrainTextureLayers;
            }
            else
            {
                string vtTerrainLayersAssetFilePath = $"{sceneName}_TerrainTextureLayers.asset";
                string vtTerrainLayersFilePath = $"{m_userDataFolderPath}/{vtTerrainLayersAssetFilePath}";

                if (!System.IO.File.Exists(vtTerrainLayersFilePath))
                {
                    vtTerrainLayersFilePath = $"{m_oldUserDataFolderPath}/{vtTerrainLayersAssetFilePath}";
                }

                stormVTTerrainTextureLayers = AssetDatabase.LoadAssetAtPath<StormVTTerrainTextureLayers>(vtTerrainLayersFilePath);
            }
           
        }


        EditorGUILayout.BeginHorizontal();
        stormVTSettings = (StormVTSettings)m_editorUtils.ObjectField("StormVTSettings", stormVTSettings, typeof(StormVTSettings), false, helpActive);
        if (m_editorUtils.Button("CreateNewSettings", helpActive))
        {

            CreateNewSettings();

        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        stormVTTerrainGrid = (StormVTTerrainGrid)m_editorUtils.ObjectField("StormVTTerrainGrid", stormVTTerrainGrid, typeof(StormVTTerrainGrid), false, helpActive);
        if (m_editorUtils.Button("CreateNewGridData", helpActive))
        {
            string vtAssetFilePath = $"{sceneName}_TerrainGrid.asset";
            string vtGridFilePath = $"{m_userDataFolderPath}/{vtAssetFilePath}";

            stormVTTerrainGrid = ScriptableObject.CreateInstance<StormVTTerrainGrid>();
            AssetDatabase.CreateAsset(stormVTTerrainGrid, vtGridFilePath);
            stormVTTerrainGrid = (StormVTTerrainGrid)AssetDatabase.LoadAssetAtPath(vtGridFilePath, typeof(StormVTTerrainGrid));
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        stormVTTerrainTextureLayers = (StormVTTerrainTextureLayers)m_editorUtils.ObjectField("StormVTTerrainLayers", stormVTTerrainTextureLayers, typeof(StormVTTerrainTextureLayers), false, helpActive);
        if (m_editorUtils.Button("CreateNewLayerData", helpActive))
        {
            string vtTerrainLayersAssetFilePath = $"{sceneName}_TerrainTextureLayers.asset";
            string vtTerrainLayersFilePath = $"{m_userDataFolderPath}/{vtTerrainLayersAssetFilePath}";

            stormVTTerrainTextureLayers = ScriptableObject.CreateInstance<StormVTTerrainTextureLayers>();
            AssetDatabase.CreateAsset(stormVTTerrainTextureLayers, vtTerrainLayersFilePath);
            stormVTTerrainTextureLayers = (StormVTTerrainTextureLayers)AssetDatabase.LoadAssetAtPath(vtTerrainLayersFilePath, typeof(StormVTTerrainTextureLayers));
        }
        EditorGUILayout.EndHorizontal();


        //Set variables to match settings 
        if (stormVTSettings != null)
        {
            m_targetPlatform = stormVTSettings.m_targetPlatform;
            m_stormVTMode = stormVTSettings.m_stormVTMode;
            m_tileWidth = stormVTSettings.m_tileWidth;
            m_tileHeight = stormVTSettings.m_tileHeight;
            m_tileSize = stormVTSettings.m_tileSize;
        }

        EditorGUI.BeginChangeCheck();


        //Override settings
        m_editorUtils.LabelField("StormTerrainTextureOverrideSettings", EditorStyles.boldLabel, helpActive);

        m_targetPlatform = (StormVTEnums.StormVTPlatform)m_editorUtils.EnumPopup("StormTargetPlatform", m_targetPlatform, helpActive);

        m_stormVTMode = (StormVTEnums.StormVTMode)m_editorUtils.EnumPopup("StormTerrainMode", m_stormVTMode, helpActive);

        EditorGUILayout.BeginHorizontal();
        m_tileWidth = (StormVTEnums.TileCount)m_editorUtils.EnumPopup("TileWidth", m_tileWidth, helpActive);
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        m_tileHeight = (StormVTEnums.TileCount)m_editorUtils.EnumPopup("TileHeight", m_tileHeight, helpActive);
        EditorGUILayout.EndHorizontal();

        m_tileSize = (StormVTEnums.TileSize)m_editorUtils.EnumPopup("TileSize", m_tileSize, helpActive);

        //End change check
        if (EditorGUI.EndChangeCheck())
        {
            if(stormVTSettings != null)
            {
                if(stormVTSettings.m_stormVTMode == StormVTMode.UnityTerrain && m_stormVTMode == StormVTMode.Clipmap)
                {
                    if(EditorUtility.DisplayDialog("Extract Terrain Textures", 
                        "The clipmap system requires loading in textures that are extracted from Unity Terrains. " +
                        "Would you like to perform the process to extract these textures now? (This process can also be performed from the Storm Terrain object in the scene)", 
                        "Yes", 
                        "No"))
                    {
                        GameObject vtPrefab = GameObject.Find("Storm Terrain");
                        if (vtPrefab != null)
                        {
                            StormVTManager stormVTManager = vtPrefab.GetComponentInChildren<StormVTManager>();
                            if (stormVTManager != null)
                            {
                                stormVTManager.StartTerrainDataBulkExportEditorUpdates();
                            }
                        }
                    }
                }
                stormVTSettings.m_targetPlatform = m_targetPlatform;
                stormVTSettings.m_stormVTMode = m_stormVTMode;
                stormVTSettings.m_tileWidth = m_tileWidth;
                stormVTSettings.m_tileHeight = m_tileHeight;
                stormVTSettings.m_tileSize = m_tileSize;
                EditorUtility.SetDirty(stormVTSettings);
                stormVTSettings.ComputeConstants();
                ComputeConstants();
            }

        }


        EditorGUILayout.EndVertical();
    }

    /// <summary>
    /// Setup ingestion panel
    /// </summary>
    /// <param name="helpActive"></param>
    private void SetupIngestionPanel(bool helpActive)
    {
        EditorGUILayout.BeginVertical(m_boxStyle);

        if (m_editorUtils.Button("IngestStormTerrain", helpActive))
        {
            
            AddPrefabToScene();
            GenerateTilesForScene();
        }

        EditorGUILayout.EndVertical();
    }

    /// <summary>
    /// Functional ingestion panel
    /// </summary>
    /// <param name="helpActive"></param>
    private void FunctionsIngestionPanel(bool helpActive)
    {
        EditorGUILayout.BeginVertical(m_boxStyle);

        if (m_editorUtils.Button("PerformMaintenance", helpActive))
        {
            StormVTEditorUtility.PerformMaintenance();
 
            if(stormVTSettings.m_stormTerrainMaterial == null || stormVTSettings.m_stormTerrainClipmapMaterial == null)
            {
                CreateMaterial();
            }

            vtPrefab = GameObject.Find("Storm Terrain");
            if(vtPrefab != null)
            {
                DestroyImmediate(vtPrefab);
                AddPrefabToScene();
            }
            UpdateReferences();
        }


        if (m_editorUtils.Button("AddStormTerrainManagerToScene", helpActive))
        {
            AddPrefabToScene();
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        }

        if (m_editorUtils.Button("RegenerateDataForScene", helpActive))
        {
            GenerateTilesForScene();
        }

        if(m_editorUtils.Button("RegenerateNormalMapsForScene", helpActive))
        {
            GenerateNormalMapsForScene();
        }

        if(m_editorUtils.Button("RecreateTerrainMaterial", helpActive))
        {
            CreateMaterial();
        }
        if(m_editorUtils.Button("PerformMaintenanceOnTerrains", helpActive))
        {
            PerformMaintenanceOnTerrains();
        }

        if (m_editorUtils.Button("RemoveStormTerrainFromScene(s)", helpActive))
        {

            vtPrefab = GameObject.Find("Storm Terrain");
            if (vtPrefab != null)
            {
                stormVTManager = vtPrefab.GetComponentInChildren<StormVTManager>();
                stormVTManager.RemoveStormTerrain();
                DestroyImmediate(vtPrefab);
            }
        }

        EditorGUILayout.EndVertical();
    }

    /// <summary>
    /// Performs maintaince on terrains
    /// </summary>
    private void PerformMaintenanceOnTerrains()
    {
        if (vtPrefab == null)
        {
            vtPrefab = GameObject.Find("Storm Terrain");
        }
        if (vtPrefab != null)
        {
            if (stormVTSettings.m_stormTerrainMaterial == null)
            {
                CreateMaterial();
            }

            terrainManager = vtPrefab.GetComponentInChildren<StormVTTerrainManager>();
            terrainManager.stormVTSettings = stormVTSettings;

            clipmap = vtPrefab.GetComponentInChildren<StormVTClipmap>();
            clipmap.m_clipmapMaterial = stormVTSettings.m_stormTerrainClipmapMaterial;


            if (stormVTManager == null)
            {
                stormVTManager = vtPrefab.GetComponentInChildren<StormVTManager>();
            }
            if(stormVTManager != null)
            {
                stormVTManager.PerformMaintenanceOnTerrains();
            }

        }
    }

    /// <summary>
    /// Terrain features panel
    /// </summary>
    /// <param name="helpActive"></param>
    private void TerrainFeaturesPanel(bool helpActive)
    {
        EditorGUILayout.BeginVertical(m_boxStyle);

        bool currentGUIState = GUI.enabled;
        if(stormVTSettings.m_stormVTMode == StormVTMode.Clipmap)
        {
            GUI.enabled = false;
        }
        m_toggleTerrain = m_editorUtils.Toggle("ToggleTerrain", m_toggleTerrain, helpActive);
        GUI.enabled = currentGUIState;
        m_debugTiles = m_editorUtils.Toggle("DebugTiles", m_debugTiles, helpActive);

        if (stormVTSettings.m_stormVTMode == StormVTMode.UnityTerrain)
        {
            GUI.enabled = false;
        }
        m_disableGaiaTerrainLoadedScenes = m_editorUtils.Toggle("DisableGaiaLoadedTerrains", m_disableGaiaTerrainLoadedScenes, helpActive);
        GUI.enabled = currentGUIState;


        EditorGUILayout.EndVertical();
    }

    /// <summary>
    /// Snow features panel
    /// </summary>
    /// <param name="helpActive"></param>
    private void SnowFeaturePanel(bool helpActive)
    {
        EditorGUILayout.BeginVertical(m_boxStyle);

        m_snowEnabled = m_editorUtils.Toggle("SnowEnabled", m_snowEnabled, helpActive);
        m_snowGenerationMode = (StormVTEnums.StormVTFeatureMode)m_editorUtils.EnumPopup("SnowGenerationMode", m_snowGenerationMode, helpActive);
        m_snowStartHeight = m_editorUtils.FloatField("SnowStartHeight", m_snowStartHeight, helpActive);
        m_snowFalloff = m_editorUtils.Slider("SnowFalloff", m_snowFalloff, 0f, 1000f, helpActive);
        m_snowCurvatureInfluence = m_editorUtils.Slider("SnowCurvatureInfluence", m_snowCurvatureInfluence, 0, 10.0f, helpActive);

        EditorGUILayout.EndVertical();
    }

    /// <summary>
    /// Floating point fix panel
    /// </summary>
    /// <param name="helpActive"></param>
    private void FloatingPointFixFeaturePanel(bool helpActive)
    {
        EditorGUILayout.BeginVertical(m_boxStyle);

        m_floatingPointFix = m_editorUtils.Toggle("FloatingPointFixEnabled", m_floatingPointFix, helpActive);
        m_floatingPointRange = m_editorUtils.FloatField("FloatingPointRange", m_floatingPointRange, helpActive);


        if (m_floatingPointFix)
        {
            if(stormVTManager == null)
            {
                if(vtPrefab != null)
                {
                    stormVTManager = vtPrefab.GetComponentInChildren<StormVTManager>();
                }
                else
                {
                    vtPrefab = GameObject.Find("Storm Terrain");
                    if(vtPrefab != null)
                    {
                        stormVTManager = vtPrefab.GetComponentInChildren<StormVTManager>();
                    }
                }
            }
            if (stormVTManager != null)
            {
                Camera mainCam = stormVTManager.MainCamera;
                if (mainCam != null)
                {
                    StormVTFloatingPointFix stormFloatingPointComponent = mainCam.GetComponent<StormVTFloatingPointFix>();
                    if (stormFloatingPointComponent != null)
                    {
                        stormFloatingPointComponent.stormVTSettings = stormVTSettings;
                    }
                    else
                    {
                        stormFloatingPointComponent = mainCam.gameObject.AddComponent<StormVTFloatingPointFix>();
                        stormFloatingPointComponent.stormVTSettings = stormVTSettings;
                        stormFloatingPointComponent.threshold = stormVTSettings.m_floatingPointRange;
                    }
                }
            }
                
        }

        EditorGUILayout.EndVertical();
    }

    /// <summary>
    /// Material features panel
    /// </summary>
    /// <param name="helpActive"></param>
    private void MaterialFeaturesPanel(bool helpActive)
    {
        EditorGUILayout.BeginVertical(m_boxStyle);

        m_heightBlend = m_editorUtils.Toggle("HeightBlend", m_heightBlend, helpActive);
        m_heightBlendFactor = m_editorUtils.Slider("HeightBlendFactor", m_heightBlendFactor,0, 0.95f, helpActive);

        bool currentGUIState = GUI.enabled;
        if (stormVTSettings.m_stormVTMode == StormVTMode.UnityTerrain)
        {
            GUI.enabled = false;
        }

        m_displacement = m_editorUtils.Toggle("Displacement", m_displacement, helpActive);
        m_displacementAmount = m_editorUtils.Slider("DisplacementAmount", m_displacementAmount, 0, 1.0f, helpActive);
        m_displacementDistance = m_editorUtils.FloatField("DisplacementDistance", m_displacementDistance, helpActive);
        m_clipmapDensity = (StormVTClipmapDensity)m_editorUtils.EnumPopup("TessellationAmount", m_clipmapDensity, helpActive);

        GUI.enabled = currentGUIState;

        EditorGUILayout.EndVertical();
    }

    /// <summary>
    /// Build settings panel
    /// </summary>
    /// <param name="helpActive"></param>
    private void BuildSettingsPanel(bool helpActive)
    {
        EditorGUILayout.BeginVertical(m_boxStyle);
        m_stripUnusedTexturesOnBuild = m_editorUtils.Toggle("StripUnusedTexturesOnBuild", m_stripUnusedTexturesOnBuild, helpActive);
      
        EditorGUILayout.EndVertical();
    }

    /// <summary>
    /// Collision features panel
    /// </summary>
    /// <param name="helpActive"></param>
    private void CollisionFeaturesPanel(bool helpActive)
    {
        EditorGUILayout.BeginVertical(m_boxStyle);

        bool currentGUIState = GUI.enabled;
        if (stormVTSettings.m_stormVTMode == StormVTMode.UnityTerrain)
        {
            GUI.enabled = false;
        }

        m_collisionEnabled = m_editorUtils.Toggle("CollisionEnabled", m_collisionEnabled, helpActive);
        m_debugColliderMeshes = m_editorUtils.Toggle("DebugColliderMeshes", m_debugColliderMeshes, helpActive);
        m_collisionQuality = (StormVTEnums.StormVTCollisionResolution)m_editorUtils.EnumPopup("CollisionQuality", m_collisionQuality, helpActive);
        m_collisionMeshResolution = m_editorUtils.IntField("CollisionMeshResolution", m_collisionMeshResolution, helpActive);
        m_colliderLoadingRange = m_editorUtils.FloatField("CollisionLoadingRange", m_colliderLoadingRange, helpActive);

        GUI.enabled = currentGUIState;

        EditorGUILayout.EndVertical();
    }

    /// <summary>
    /// Data loading features panel
    /// </summary>
    /// <param name="helpActive"></param>
    private void DataLoadingFeaturesPanel(bool helpActive)
    {
        EditorGUILayout.BeginVertical(m_boxStyle);

        bool currentGUIState = GUI.enabled;
        if (stormVTSettings.m_stormVTMode == StormVTMode.UnityTerrain)
        {
            GUI.enabled = false;
        }

        m_maxParallelTasks = m_editorUtils.IntSlider("MaxParallelTasks", m_maxParallelTasks, 0, 20, helpActive);
        m_chunkSize = m_editorUtils.IntField("ChunkSize", m_chunkSize, helpActive);
        m_maxParallelChunks = m_editorUtils.IntSlider("MaxParallelChunks", m_maxParallelChunks, 0, 20, helpActive);


        m_lowResloadingRange = m_editorUtils.FloatField("LowResLoadingRange", m_lowResloadingRange, helpActive);
        m_highResloadingRange = m_editorUtils.FloatField("HighResLoadingRange", m_highResloadingRange, helpActive);
        m_lowResPoolCount = m_editorUtils.IntField("LowResPoolCount", m_lowResPoolCount, helpActive);
        m_highResPoolCount = m_editorUtils.IntField("HighResPoolCount", m_highResPoolCount, helpActive);

        GUI.enabled = currentGUIState;

        EditorGUILayout.EndVertical();
    }

    /// <summary>
    /// Imposter features panel
    /// </summary>
    /// <param name="helpActive"></param>
    private void ImposterFeaturesPanel(bool helpActive)
    {
        EditorGUILayout.BeginVertical(m_boxStyle);

        bool currentGUIState = GUI.enabled;
        if (stormVTSettings.m_stormVTMode == StormVTMode.Clipmap)
        {
            GUI.enabled = false;
        }

        m_grassMapColor = m_editorUtils.Toggle("GrassMapColor", m_grassMapColor, helpActive);
        m_grassMapNormal = m_editorUtils.Toggle("GrassMapNormal", m_grassMapNormal, helpActive);
        m_treeMapColor = m_editorUtils.Toggle("TreeMapColor", m_treeMapColor, helpActive);
        m_treeMapNormal = m_editorUtils.Toggle("TreeMapNormal", m_treeMapNormal, helpActive);
        m_grassBlendDistance = m_editorUtils.Slider("GrassBlendDistance", m_grassBlendDistance, 0, 5000.0f, helpActive);
        m_grassBlendAlpha = m_editorUtils.Slider("GrassBlendAlpha", m_grassBlendAlpha, 0, 1.0f, helpActive);
        m_grassBlendSharpness = m_editorUtils.Slider("GrassBlendSharpness", m_grassBlendSharpness, 0, 1.0f, helpActive);
        m_grassBlendNormalAlpha = m_editorUtils.Slider("GrassBlendNormalAlpha", m_grassBlendNormalAlpha, 0, 1.0f, helpActive);
        m_treesBlendDistance = m_editorUtils.Slider("TreesBlendDistance", m_treesBlendDistance, 0, 5000.0f, helpActive);
        m_treesBlendAlpha = m_editorUtils.Slider("TreesBlendAlpha", m_treesBlendAlpha, 0, 1.0f, helpActive);
        m_treesBlendSharpness = m_editorUtils.Slider("TreesBlendSharpness", m_treesBlendSharpness, 0, 1.0f, helpActive);
        m_treesBlendNormalAlpha = m_editorUtils.Slider("TreesBlendNormalAlpha", m_treesBlendNormalAlpha, 0, 1.0f, helpActive);

        GUI.enabled = currentGUIState;

        EditorGUILayout.EndVertical();

    }

    /// <summary>
    /// Main settings tab
    /// </summary>
    private void MainSettingsTab()
    {
        m_editorUtils.Panel("SetupIngestion", SetupIngestionPanel, true);
        m_editorUtils.Panel("Configuration", ConfigurationPanel, true);
        m_editorUtils.Panel("FunctionsIngestion", FunctionsIngestionPanel, false);
    }

    /// <summary>
    /// Features tab
    /// </summary>
    private void FeaturesTab()
    {

        if(vtPrefab == null)
        {
            vtPrefab = GameObject.Find("Storm Terrain");
            if(vtPrefab == null)
            {
                EditorGUILayout.HelpBox(m_editorUtils.GetTooltip("TerrainManagerMissingHelp"), MessageType.Warning);
                GUI.enabled = false;
            }

        }

        //Set variables to match settings 
        if (stormVTSettings != null)
        {
            m_floatingPointFix = stormVTSettings.m_floatingPointFix;
            m_floatingPointRange = stormVTSettings.m_floatingPointRange;
            m_snowEnabled = stormVTSettings.m_Snow;
            m_snowGenerationMode = stormVTSettings.m_SnowGenerationMode;
            m_snowStartHeight = stormVTSettings.m_SnowHeight;
            m_snowFalloff = stormVTSettings.m_SnowFalloff;
            m_snowCurvatureInfluence = stormVTSettings.m_SnowCurvatureInfluence;

            m_displacement = stormVTSettings.m_displacement;
            m_displacementAmount = stormVTSettings.m_displacementAmount;
            m_displacementDistance = stormVTSettings.m_displacementDistance;
            m_clipmapDensity = stormVTSettings.m_clipmapDensity;
            m_heightBlend = stormVTSettings.m_heightBlend;
            m_heightBlendFactor = stormVTSettings.m_heightBlendFactor;

            m_collisionEnabled = stormVTSettings.m_collisionEnabled;
            m_debugColliderMeshes = stormVTSettings.m_debugColliderMeshes;
            m_collisionQuality = stormVTSettings.m_collisionQuality;
            m_collisionMeshResolution = stormVTSettings.m_collisionMeshResolution;
            m_colliderLoadingRange = stormVTSettings.m_colliderLoadingRange;

            m_maxParallelTasks = stormVTSettings.m_maxParallelTasks;
            m_chunkSize = stormVTSettings.m_chunkSize;
            m_maxParallelChunks = stormVTSettings.m_maxParallelChunks;

            m_lowResloadingRange = stormVTSettings.m_lowResloadingRange;
            m_highResloadingRange = stormVTSettings.m_highResloadingRange;
            m_lowResPoolCount = stormVTSettings.m_lowResPoolCount;
            m_highResPoolCount = stormVTSettings.m_highResPoolCount;

            m_debugTiles = stormVTSettings.m_debugTiles;
            m_toggleTerrain = stormVTSettings.m_toggleTerrain;
            m_disableGaiaTerrainLoadedScenes = stormVTSettings.m_disableGaiaTerrainLoadedScenes;

            m_stripUnusedTexturesOnBuild = stormVTSettings.m_stripUnusedTexturesOnBuild;

            m_grassMapColor = stormVTSettings.m_grassMapColor;
            m_grassMapNormal = stormVTSettings.m_grassMapNormal;
            m_treeMapColor = stormVTSettings.m_treeMapColor;
            m_treeMapNormal = stormVTSettings.m_treeMapNormal;
            m_grassBlendDistance = stormVTSettings.m_grassBlendDistance;
            m_grassBlendAlpha = stormVTSettings.m_grassBlendAlpha;
            m_grassBlendSharpness = stormVTSettings.m_grassBlendSharpness;
            m_grassBlendNormalAlpha = stormVTSettings.m_grassBlendNormalAlpha;
            m_treesBlendDistance = stormVTSettings.m_treesBlendDistance;
            m_treesBlendAlpha = stormVTSettings.m_treesBlendAlpha;
            m_treesBlendSharpness = stormVTSettings.m_treesBlendSharpness;
            m_treesBlendNormalAlpha = stormVTSettings.m_treesBlendNormalAlpha;
        }

        EditorGUI.BeginChangeCheck();

        float labelWidthBefore = EditorGUIUtility.labelWidth;
        EditorGUIUtility.labelWidth = 200f;

        m_editorUtils.Panel("TerrainFeatures", TerrainFeaturesPanel, true);
        m_editorUtils.Panel("MaterialFeatures", MaterialFeaturesPanel, true);
        m_editorUtils.Panel("Snow", SnowFeaturePanel, true);
        m_editorUtils.Panel("Imposter", ImposterFeaturesPanel, false);
        m_editorUtils.Panel("FloatingPointFix", FloatingPointFixFeaturePanel, false);
        m_editorUtils.Panel("CollisionFeatures", CollisionFeaturesPanel, false);
        m_editorUtils.Panel("BuildSettingsPanel", BuildSettingsPanel, false);
        m_editorUtils.Panel("DataLoadingFeatures", DataLoadingFeaturesPanel, false);

        EditorGUIUtility.labelWidth = labelWidthBefore;

        if (EditorGUI.EndChangeCheck())
        {
            if (stormVTSettings != null)
            {
                stormVTSettings.m_floatingPointFix = m_floatingPointFix;
                stormVTSettings.m_floatingPointRange = m_floatingPointRange;
                stormVTSettings.m_Snow = m_snowEnabled;
                stormVTSettings.m_SnowGenerationMode = m_snowGenerationMode;
                stormVTSettings.m_SnowHeight = m_snowStartHeight;
                stormVTSettings.m_SnowFalloff = m_snowFalloff;
                stormVTSettings.m_SnowCurvatureInfluence = m_snowCurvatureInfluence;

                stormVTSettings.m_displacement = m_displacement;
                stormVTSettings.m_displacementAmount = m_displacementAmount;
                stormVTSettings.m_displacementDistance = m_displacementDistance;
                stormVTSettings.m_clipmapDensity = m_clipmapDensity;
                stormVTSettings.m_heightBlend = m_heightBlend;
                stormVTSettings.m_heightBlendFactor = m_heightBlendFactor;
                


                stormVTSettings.m_collisionEnabled = m_collisionEnabled;
                stormVTSettings.m_debugColliderMeshes = m_debugColliderMeshes;
                stormVTSettings.m_collisionQuality = m_collisionQuality;
                stormVTSettings.m_collisionMeshResolution = m_collisionMeshResolution;
                stormVTSettings.m_colliderLoadingRange = m_colliderLoadingRange;

                stormVTSettings.m_maxParallelTasks = m_maxParallelTasks;
                stormVTSettings.m_chunkSize = m_chunkSize;
                stormVTSettings.m_maxParallelChunks = m_maxParallelChunks;

                stormVTSettings.m_lowResloadingRange = m_lowResloadingRange;
                stormVTSettings.m_highResloadingRange = m_highResloadingRange;
                stormVTSettings.m_lowResPoolCount = m_lowResPoolCount;
                stormVTSettings.m_highResPoolCount = m_highResPoolCount;

                stormVTSettings.m_debugTiles = m_debugTiles;
                stormVTSettings.m_toggleTerrain = m_toggleTerrain;
                stormVTSettings.m_disableGaiaTerrainLoadedScenes = m_disableGaiaTerrainLoadedScenes;

                stormVTSettings.m_stripUnusedTexturesOnBuild = m_stripUnusedTexturesOnBuild;

                stormVTSettings.m_grassMapColor = m_grassMapColor;
                stormVTSettings.m_grassMapNormal = m_grassMapNormal;
                stormVTSettings.m_treeMapColor = m_treeMapColor;
                stormVTSettings.m_treeMapNormal = m_treeMapNormal;
                stormVTSettings.m_grassBlendDistance = m_grassBlendDistance;
                stormVTSettings.m_grassBlendAlpha = m_grassBlendAlpha;
                stormVTSettings.m_grassBlendSharpness = m_grassBlendSharpness;
                stormVTSettings.m_grassBlendNormalAlpha = m_grassBlendNormalAlpha;
                stormVTSettings.m_treesBlendDistance = m_treesBlendDistance;
                stormVTSettings.m_treesBlendAlpha = m_treesBlendAlpha;
                stormVTSettings.m_treesBlendSharpness = m_treesBlendSharpness;
                stormVTSettings.m_treesBlendNormalAlpha = m_treesBlendNormalAlpha;

                EditorUtility.SetDirty(stormVTSettings);
                stormVTSettings.ComputeConstants();
                ComputeConstants();
                stormVTSettings.ForceSettingsUpdate();
                RegenerateTiles();
            }
            
        }

    }

    /// <summary>
    /// Force tiles to regenerate
    /// </summary>
    private void RegenerateTiles()
    {

        //find the physcial texture manager
        List<StormVTPhysicalTextureManager> physicalTextureManager = StormVTUtilities.FindObjectsByType<StormVTPhysicalTextureManager>(false).ToList();
        if (physicalTextureManager.Count > 0)
        {
            physicalTextureManager[0].RefreshTiles();
        }
    }

    /// <summary>
    /// Retrieves the terrain texture layers sciptable object
    /// </summary>
    public void FetchTerrainTextureLayers()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        string m_userDataFolderPath = $"{StormVTConstants.GetAssetsFolder(sceneName)}";
        string m_userOldDataFolderPath = $"{StormVTConstants.GetAssetsFolderOld(sceneName)}";

        if (stormVTTerrainTextureLayers == null)
        {
            string vtAssetFilePath = $"{sceneName}_TerrainTextureLayers.asset";
            string vtTerrainLayersFilePath = $"{StormVTConstants.GetAssetsFolder()}/{vtAssetFilePath}";

            if(!System.IO.File.Exists(vtTerrainLayersFilePath))
            {
                vtTerrainLayersFilePath = $"{StormVTConstants.GetAssetsFolderOld()}/{vtAssetFilePath}";
            }

            stormVTTerrainTextureLayers = (StormVTTerrainTextureLayers)AssetDatabase.LoadAssetAtPath(vtTerrainLayersFilePath, typeof(StormVTTerrainTextureLayers));

            if (stormVTTerrainTextureLayers == null)
            {
                stormVTTerrainTextureLayers = ScriptableObject.CreateInstance<StormVTTerrainTextureLayers>();
                AssetDatabase.CreateAsset(stormVTTerrainTextureLayers, vtTerrainLayersFilePath);
                stormVTTerrainTextureLayers = (StormVTTerrainTextureLayers)AssetDatabase.LoadAssetAtPath(vtTerrainLayersFilePath, typeof(StormVTTerrainTextureLayers));
            }
        }
    }

    /// <summary>
    /// Layers tab
    /// </summary>
    private void LayersTab()
    {

        if(stormVTTerrainTextureLayers != null)
            FetchTerrainTextureLayers();

        if (stormVTTerrainTextureLayers != null)
        {
            if (m_terrainLayerEditor == null || m_terrainLayerEditor.target != stormVTTerrainTextureLayers)
            {
                if (m_terrainLayerEditor != null)
                    DestroyImmediate(m_terrainLayerEditor);
                m_terrainLayerEditor = Editor.CreateEditor(stormVTTerrainTextureLayers) as StormVTTerrainTextureLayersEditor;
                m_terrainLayerEditor.m_isInWindow = true;
            }
            m_terrainLayerEditor.OnInspectorGUI();
            if (m_terrainLayerEditor == null)
                GUIUtility.ExitGUI();
        }

        else if (m_terrainLayerEditor != null)
            m_terrainLayerEditor = null;

    }

    /// <summary>
    /// When GUI is drawn
    /// </summary>
    public override void OnGUI()
    {
        //Set up the box style
        if (m_boxStyle == null)
        {
            m_boxStyle = new GUIStyle(GUI.skin.box);
            m_boxStyle.normal.textColor = GUI.skin.label.normal.textColor;
            m_boxStyle.fontStyle = FontStyle.Bold;
            m_boxStyle.alignment = TextAnchor.UpperLeft;
        }

        m_editorUtils.Initialize(); // Do not remove this!
        if (!IsEmbedded)
            m_editorUtils.GUIHeader();

        if (IsEmbedded)
            m_scrollPosition = EditorGUILayout.BeginScrollView(m_scrollPosition, GUILayout.MinHeight(300), GUILayout.MaxHeight(float.MaxValue));
        else
            m_scrollPosition = EditorGUILayout.BeginScrollView(m_scrollPosition);

        m_editorUtils.Tabs(m_mainTabs);

        EditorGUILayout.EndScrollView();
        m_editorUtils.GUINewsFooter();

       
    }

    /// <summary>
    /// Adds the storm terrain into the scene
    /// </summary>
    void AddPrefabToScene()
    {
        
        string sceneName = SceneManager.GetActiveScene().name;
        string m_userDataFolderPath = $"{StormVTConstants.GetAssetsFolder(sceneName)}";
        string m_oldUserDataFolderPath = $"{StormVTConstants.GetAssetsFolderOld(sceneName)}";
        
        vtPrefab = GameObject.Find("Storm Terrain");
        if(vtPrefab == null)
            vtPrefab = Instantiate(StormVTDefaults.GetVTPrefab(), Vector3.zero, Quaternion.identity);
        if (vtPrefab != null)
        {
            vtPrefab.name = "Storm Terrain";
            vtPrefab.gameObject.name = "Storm Terrain";

            stormVTManager = vtPrefab.GetComponentInChildren<StormVTManager>();
            clipmap = vtPrefab.GetComponentInChildren<StormVTClipmap>();
            pageTableClipmap = vtPrefab.GetComponentInChildren<StormVTPageTableClipmap>();
            physicalTextureManager = vtPrefab.GetComponentInChildren<StormVTPhysicalTextureManager>();
            quadTree = vtPrefab.GetComponentInChildren<StormVTQuadTree>();
            terrainManager = vtPrefab.GetComponentInChildren<StormVTTerrainManager>();
            textureManager = vtPrefab.GetComponentInChildren<StormVTTerrainTextureManager>();
            textureLoadingSystem = vtPrefab.GetComponent<StormVTTextureLoadingSystem>();
            colliderManager = vtPrefab.GetComponentInChildren<StormVTTerrainColliderManager>();
            textureCompression = vtPrefab.GetComponentInChildren<StormVTTextureCompression>();
            feedbackManager = vtPrefab.GetComponentInChildren<StormVTFeedbackManager>();
            feedbackClipmap = vtPrefab.GetComponentInChildren<StormVTFeedbackClipmap>();
            decalManager = vtPrefab.GetComponentInChildren<StormVTDecalManager>();
            terrainTile = vtPrefab.GetComponentInChildren<StormVTTerrainTile>();



            //Remove missing components (can by caused by pipeline changes)
            GameObject tileCapturerObj = GameObject.Find("TileCapturer");
            if (tileCapturerObj != null)
            {
                GameObjectUtility.RemoveMonoBehavioursWithMissingScript(tileCapturerObj);
            }
                
            GameObject frustumCameraObj = GameObject.Find("FrustumCamera");
            if (frustumCameraObj != null)
            {
                GameObjectUtility.RemoveMonoBehavioursWithMissingScript(frustumCameraObj);
            }

            if(stormVTSettings == null)
            {
                CreateNewSettings();   
            }
            if(stormVTTerrainGrid == null)
            {
                string vtGridAssetFilePath = $"{sceneName}_TerrainGrid.asset";
                string vtGridFilePath = $"{m_userDataFolderPath}/{vtGridAssetFilePath}";

                if(System.IO.File.Exists(vtGridFilePath))
                {
                    vtGridFilePath = $"{m_oldUserDataFolderPath}/{vtGridAssetFilePath}";
                }

                stormVTTerrainGrid = AssetDatabase.LoadAssetAtPath<StormVTTerrainGrid>(vtGridFilePath);
                if (stormVTTerrainGrid == null)
                {
                    stormVTTerrainGrid = ScriptableObject.CreateInstance<StormVTTerrainGrid>();
                    AssetDatabase.CreateAsset(stormVTTerrainGrid, vtGridFilePath);
                    stormVTTerrainGrid = (StormVTTerrainGrid)AssetDatabase.LoadAssetAtPath(vtGridFilePath, typeof(StormVTTerrainGrid));
                }
            }
            if (stormVTTerrainTextureLayers == null)
            {
                string vtAssetFilePath = $"{sceneName}_TerrainTextureLayers.asset";
                string vtTerrainLayersFilePath = $"{StormVTConstants.GetAssetsFolder()}/{vtAssetFilePath}";

                if (System.IO.File.Exists(vtTerrainLayersFilePath))
                {
                    vtTerrainLayersFilePath = $"{StormVTConstants.GetAssetsFolderOld()}/{vtAssetFilePath}";
                }

                stormVTTerrainTextureLayers = (StormVTTerrainTextureLayers)AssetDatabase.LoadAssetAtPath(vtTerrainLayersFilePath, typeof(StormVTTerrainTextureLayers));

                if (stormVTTerrainTextureLayers == null)
                {
                    stormVTTerrainTextureLayers = ScriptableObject.CreateInstance<StormVTTerrainTextureLayers>();
                    AssetDatabase.CreateAsset(stormVTTerrainTextureLayers, vtTerrainLayersFilePath);
                    stormVTTerrainTextureLayers = (StormVTTerrainTextureLayers)AssetDatabase.LoadAssetAtPath(vtTerrainLayersFilePath, typeof(StormVTTerrainTextureLayers));
                }
            }

            UpdateReferences();
        }

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
    }
    
    /// <summary>
    /// Update all component references under the Storm Terrain Manager prefab
    /// </summary>
    void UpdateReferences()
    {

        stormVTManager = vtPrefab.GetComponentInChildren<StormVTManager>();
        clipmap = vtPrefab.GetComponentInChildren<StormVTClipmap>();
        pageTableClipmap = vtPrefab.GetComponentInChildren<StormVTPageTableClipmap>();
        physicalTextureManager = vtPrefab.GetComponentInChildren<StormVTPhysicalTextureManager>();
        quadTree = vtPrefab.GetComponentInChildren<StormVTQuadTree>();
        terrainManager = vtPrefab.GetComponentInChildren<StormVTTerrainManager>();
        textureManager = vtPrefab.GetComponentInChildren<StormVTTerrainTextureManager>();
        textureLoadingSystem = vtPrefab.GetComponent<StormVTTextureLoadingSystem>();
        colliderManager = vtPrefab.GetComponentInChildren<StormVTTerrainColliderManager>();
        textureCompression = vtPrefab.GetComponentInChildren<StormVTTextureCompression>();
        feedbackManager = vtPrefab.GetComponentInChildren<StormVTFeedbackManager>();
        feedbackClipmap = vtPrefab.GetComponentInChildren<StormVTFeedbackClipmap>();
        decalManager = vtPrefab.GetComponentInChildren<StormVTDecalManager>();
        terrainTile = vtPrefab.GetComponentInChildren<StormVTTerrainTile>();

        FetchTerrainLayers();

        stormVTManager.stormVTSettings = stormVTSettings;
        stormVTManager.stormVTTerrainGrid = stormVTTerrainGrid;
        stormVTManager.stormVTTerrainTextureLayers = stormVTTerrainTextureLayers;

        physicalTextureManager.stormVTSettings = stormVTSettings;
        physicalTextureManager.stormVTTerrainGrid = stormVTTerrainGrid;
        physicalTextureManager.stormVTTerrainTextureLayers = stormVTTerrainTextureLayers;

        pageTableClipmap.stormVTSettings = stormVTSettings;
        terrainManager.stormVTSettings = stormVTSettings;

        clipmap.stormVTSettings = stormVTSettings;
        clipmap.stormVTTerrainGrid = stormVTTerrainGrid;
        clipmap.m_clipmapMaterial = stormVTSettings.m_stormTerrainClipmapMaterial;

        textureManager.stormVTTerrainGrid = stormVTTerrainGrid;
        textureManager.stormVTSettings = stormVTSettings;
        if(textureLoadingSystem != null)
            textureLoadingSystem.stormVTSettings = stormVTSettings;

        quadTree.stormVTSettings = stormVTSettings;
        quadTree.stormVTTerrainGrid = stormVTTerrainGrid;

        if(colliderManager != null)
        {
            colliderManager.stormVTSettings = stormVTSettings;
            colliderManager.stormVTTerrainGrid = stormVTTerrainGrid;
            colliderManager.stormVTTerrainTextureManager = textureManager;
        }

        textureCompression.stormVTSettings = stormVTSettings;

        feedbackManager.stormVTSettings = stormVTSettings;
        feedbackManager.stormVTTerrainGrid = stormVTTerrainGrid;
        feedbackManager.stormVTClipmap = feedbackClipmap;

        feedbackClipmap.stormVTSettings = stormVTSettings;
        feedbackClipmap.stormVTTerrainGrid = stormVTTerrainGrid;

        decalManager.stormVTSettings = stormVTSettings;
        decalManager.stormVTTerrainGrid = stormVTTerrainGrid;

        terrainTile.stormVTSettings = stormVTSettings;
    }

    /// <summary>
    /// Retrieves the terrain texture layers sciptable object
    /// </summary>
    public void FetchTerrainLayers()
    {
    #if UNITY_EDITOR
        if (stormVTTerrainTextureLayers == null)
        {
            Scene activeScene = SceneManager.GetActiveScene();
            string vtAssetFilePath = $"{activeScene.name}_TerrainTextureLayers.asset";
            string vtTerrainLayersFilePath = $"{StormVTConstants.GetAssetsFolder()}/{vtAssetFilePath}";

            if(System.IO.File.Exists(vtTerrainLayersFilePath))
            {
                vtTerrainLayersFilePath = $"{StormVTConstants.GetAssetsFolderOld()}/{vtAssetFilePath}";
            }

            stormVTTerrainTextureLayers = (StormVTTerrainTextureLayers)AssetDatabase.LoadAssetAtPath(vtTerrainLayersFilePath, typeof(StormVTTerrainTextureLayers));

            if (stormVTTerrainTextureLayers == null)
            {
                stormVTTerrainTextureLayers = ScriptableObject.CreateInstance<StormVTTerrainTextureLayers>();
                AssetDatabase.CreateAsset(stormVTTerrainTextureLayers, vtTerrainLayersFilePath);
                stormVTTerrainTextureLayers = (StormVTTerrainTextureLayers)AssetDatabase.LoadAssetAtPath(vtTerrainLayersFilePath, typeof(StormVTTerrainTextureLayers));
            }
        }
    #endif
    }

    /// <summary>
    /// Creates a new Storm VT Settings scriptable object
    /// </summary>
    void CreateNewSettings()
    {

        string sceneName = SceneManager.GetActiveScene().name;
        string m_userDataFolderPath = $"{StormVTConstants.GetAssetsFolder(sceneName)}";

        string vtAssetFilePath = $"{sceneName}_StormVTSettings.asset";
        string vtSettingsFilePath = $"{m_userDataFolderPath}/{vtAssetFilePath}";
        string vtMaterialAssetFilePath = $"{sceneName}_StormVTMat.mat";
        string vtMaterialAssetFilePathClipmap = $"{sceneName}_StormVTMat_Clipmap.mat";
        string vtMaterialFilePath = $"{m_userDataFolderPath}/{vtMaterialAssetFilePath}";
        string vtMaterialFilePathClipmap = $"{m_userDataFolderPath}/{vtMaterialAssetFilePathClipmap}";

        stormVTSettings = AssetDatabase.LoadAssetAtPath<StormVTSettings>(vtSettingsFilePath);
        if (stormVTSettings == null)
        {
            stormVTSettings = ScriptableObject.CreateInstance<StormVTSettings>();
            AssetDatabase.CreateAsset(stormVTSettings, vtSettingsFilePath);
            stormVTSettings = (StormVTSettings)AssetDatabase.LoadAssetAtPath(vtSettingsFilePath, typeof(StormVTSettings));

            //set up Floating point fix if we find it in the Gaia setup
            if (StormVTGaia.Instance.IsGaiaFloatingPointFixScene())
            {
                stormVTSettings.m_floatingPointFix = true;
                stormVTSettings.m_floatingPointRange = StormVTGaia.Instance.GetFloatingPointFixRange();
                
                Camera mainCam = stormVTManager.MainCamera;
                if (mainCam != null)
                {
                    StormVTFloatingPointFix stormFloatingPointComponent = mainCam.GetComponent<StormVTFloatingPointFix>();
                    if (stormFloatingPointComponent != null)
                    {
                        stormFloatingPointComponent.stormVTSettings = stormVTSettings;
                    }
                    else
                    {
                        stormFloatingPointComponent = mainCam.gameObject.AddComponent<StormVTFloatingPointFix>();
                        stormFloatingPointComponent.stormVTSettings = stormVTSettings;
                        stormFloatingPointComponent.threshold = stormVTSettings.m_floatingPointRange;
                    }
                }
            }


            //Create base material
            Material stormVTMat = new Material(StormVTDefaults.GetVirtualTerrainMaterial());
            AssetDatabase.CreateAsset(stormVTMat, vtMaterialFilePath);
            stormVTMat = (Material)AssetDatabase.LoadAssetAtPath(vtMaterialFilePath, typeof(Material));
            stormVTSettings.m_stormTerrainMaterial = stormVTMat;

            //Create clipmap material
            Material stormVTMatClipmap = new Material(StormVTDefaults.GetVirtualTerrainMaterial());
            AssetDatabase.CreateAsset(stormVTMatClipmap, vtMaterialFilePathClipmap);
            stormVTMatClipmap = (Material)AssetDatabase.LoadAssetAtPath(vtMaterialFilePathClipmap, typeof(Material));
            stormVTMatClipmap.shader = StormVTDefaults.GetClipmapTerrainShader();
            stormVTSettings.m_stormTerrainClipmapMaterial = stormVTMatClipmap;

            SetKeyword(stormVTMat, StormVTShaderIDs.m_stormVT_UnityTerrain, StormVTShaderIDs.m_stormVT_UnityTerrainOn, false);
            SetKeyword(stormVTMatClipmap, StormVTShaderIDs.m_stormVT_UnityTerrain, StormVTShaderIDs.m_stormVT_UnityTerrainOn, false);
            SetKeyword(stormVTMatClipmap, StormVTShaderIDs.m_stormVT_ClipmapTerrain, StormVTShaderIDs.m_stormVT_ClipmapTerrainOn, true);

            stormVTSettings.UpdateTextures();
            stormVTSettings.RecomputeData();
            stormVTSettings.ComputeConstants();
            ComputeConstants();
            EditorUtility.SetDirty(stormVTSettings);
            AssetDatabase.SaveAssetIfDirty(stormVTSettings);
        }

            
    }

    /// <summary>
    /// Generates all the terrain texture data for scene
    /// </summary>
    void GenerateTilesForScene()
    {
        if (vtPrefab != null)
        {
            stormVTManager.Initialize();
            stormVTManager.StartEditorUpdates();
            stormVTManager.GetAllTerrainLayers();

            stormVTTerrainGrid.UpdateTerrainLayerGridData();
            stormVTTerrainGrid.SetTerrainPivot();
            stormVTTerrainGrid.UpdateTerrainIndices();
            EditorUtility.SetDirty(stormVTTerrainGrid);
        }
        else
        {
            vtPrefab = GameObject.Find("Storm Terrain");
            if (vtPrefab != null)
            {
                stormVTManager = vtPrefab.GetComponentInChildren<StormVTManager>();
                clipmap = vtPrefab.GetComponentInChildren<StormVTClipmap>();
                pageTableClipmap = vtPrefab.GetComponentInChildren<StormVTPageTableClipmap>();
                physicalTextureManager = vtPrefab.GetComponentInChildren<StormVTPhysicalTextureManager>();
                quadTree = vtPrefab.GetComponentInChildren<StormVTQuadTree>();
                terrainManager = vtPrefab.GetComponentInChildren<StormVTTerrainManager>();
                textureManager = vtPrefab.GetComponentInChildren<StormVTTerrainTextureManager>();
                decalManager = vtPrefab.GetComponentInChildren<StormVTDecalManager>();

                UpdateReferences();

                stormVTManager.Initialize();
                stormVTManager.StartEditorUpdates();
                stormVTManager.GetAllTerrainLayers();

                stormVTTerrainGrid.UpdateTerrainLayerGridData();
                stormVTTerrainGrid.SetTerrainPivot();
                stormVTTerrainGrid.UpdateTerrainIndices();
                EditorUtility.SetDirty(stormVTTerrainGrid);

            }
            else
            {
                Debug.Log("Please add prefab into scene first");
            }
                
        }
    }

    /// <summary>
    /// Generates all normal maps for the scene
    /// </summary>
    void GenerateNormalMapsForScene()
    {
        if (vtPrefab != null)
        {
            stormVTManager.StartEditorUpdates(false);
            EditorUtility.SetDirty(stormVTTerrainGrid);
        }
        else
        {
            vtPrefab = GameObject.Find("Storm Terrain");
            if (vtPrefab != null)
            {
                stormVTManager = vtPrefab.GetComponentInChildren<StormVTManager>();
                clipmap = vtPrefab.GetComponentInChildren<StormVTClipmap>();
                pageTableClipmap = vtPrefab.GetComponentInChildren<StormVTPageTableClipmap>();
                physicalTextureManager = vtPrefab.GetComponentInChildren<StormVTPhysicalTextureManager>();
                quadTree = vtPrefab.GetComponentInChildren<StormVTQuadTree>();
                terrainManager = vtPrefab.GetComponentInChildren<StormVTTerrainManager>();
                textureManager = vtPrefab.GetComponentInChildren<StormVTTerrainTextureManager>();

                UpdateReferences();

                stormVTManager.StartEditorUpdates(false);
                EditorUtility.SetDirty(stormVTTerrainGrid);

            }
            else
            {
                Debug.Log("Please add prefab into scene first");
            }

        }
    }

    /// <summary>
    /// Creates the Unity Terrain and Clipmap materials for this scene setup
    /// </summary>
    private void CreateMaterial()
    {
        if (stormVTSettings == null)
        {
            //Load in settings if null, or create it a new 
            CreateNewSettings();
        }


        string sceneName = SceneManager.GetActiveScene().name;
        string m_userDataFolderPath = $"{StormVTConstants.GetAssetsFolder(sceneName)}";

        string vtAssetFilePath = $"{sceneName}_StormVTSettings.asset";
        string vtMaterialAssetFilePath = $"{sceneName}_StormVTMat.mat";
        string vtMaterialAssetFilePathClipmap = $"{sceneName}_StormVTMat_Clipmap.mat";
        string vtMaterialFilePath = $"{m_userDataFolderPath}/{vtMaterialAssetFilePath}";
        string vtMaterialFilePathClipmap = $"{m_userDataFolderPath}/{vtMaterialAssetFilePathClipmap}";

        Material stormVTMat = (Material)AssetDatabase.LoadAssetAtPath(vtMaterialFilePath, typeof(Material));
        Material stormVTMatClipmap = (Material)AssetDatabase.LoadAssetAtPath(vtMaterialFilePathClipmap, typeof(Material));
        if(stormVTMat == null)
        {
            //Create base material
            stormVTMat = new Material(StormVTDefaults.GetVirtualTerrainMaterial());
            AssetDatabase.CreateAsset(stormVTMat, vtMaterialFilePath);
            stormVTMat = (Material)AssetDatabase.LoadAssetAtPath(vtMaterialFilePath, typeof(Material));
            stormVTSettings.m_stormTerrainMaterial = stormVTMat;
        }

        if (stormVTMatClipmap == null)
        {
            //Create clipmap material
            stormVTMatClipmap = new Material(StormVTDefaults.GetVirtualTerrainMaterial());
            stormVTMatClipmap.shader = StormVTDefaults.GetClipmapTerrainShader();
            AssetDatabase.CreateAsset(stormVTMatClipmap, vtMaterialFilePathClipmap);
            stormVTMatClipmap = (Material)AssetDatabase.LoadAssetAtPath(vtMaterialFilePathClipmap, typeof(Material));
            stormVTSettings.m_stormTerrainClipmapMaterial = stormVTMatClipmap;
        }

        SetKeyword(stormVTMat, StormVTShaderIDs.m_stormVT_UnityTerrain, StormVTShaderIDs.m_stormVT_UnityTerrainOn, false);
        SetKeyword(stormVTMatClipmap, StormVTShaderIDs.m_stormVT_UnityTerrain, StormVTShaderIDs.m_stormVT_UnityTerrainOn, false);
        SetKeyword(stormVTMatClipmap, StormVTShaderIDs.m_stormVT_ClipmapTerrain, StormVTShaderIDs.m_stormVT_ClipmapTerrainOn, true);

        //Update material reference everywhere
        if (vtPrefab != null)
        {
            if(clipmap == null)
            {
                clipmap = vtPrefab.GetComponentInChildren<StormVTClipmap>();
            }
            if(clipmap != null)
                clipmap.m_clipmapMaterial = stormVTSettings.m_stormTerrainClipmapMaterial;
            if(terrainManager == null)
            {
                terrainManager = vtPrefab.GetComponentInChildren<StormVTTerrainManager>();
                terrainManager.stormVTSettings = stormVTSettings;
            }
        }
    }
   
    /// <summary>
    /// Sets a keyword state on the given material
    /// </summary>
    /// <param name="mat">Material to set a keyword on</param>
    /// <param name="keywordID"></param>
    /// <param name="keywordName"></param>
    /// <param name="enabled"></param>
    private void SetKeyword(Material mat, int keywordID, string keywordName, bool enabled)
    {

        mat.SetInt(keywordID, enabled ? 1 : 0);
        if (enabled)
            mat.EnableKeyword(keywordName);
        else
            mat.DisableKeyword(keywordName);
    }

    /// <summary>
    /// Compute constant data for the settings object
    /// </summary>
    public void ComputeConstants()
    {
    #if UNITY_EDITOR
        Material stormVTMaterial = stormVTSettings.m_stormTerrainMaterial;
        Material stormVTMaterialClipmap = stormVTSettings.m_stormTerrainClipmapMaterial;

        //Snow settings
        if (stormVTSettings.m_Snow)
        {
            if (stormVTSettings.m_SnowGenerationMode == StormVTFeatureMode._Runtime)
            {
                SetKeyword(stormVTMaterial, StormVTShaderIDs.m_stormVT_SnowTerrain, StormVTShaderIDs.m_stormVT_SnowTerrainOn, true);
                SetKeyword(stormVTMaterialClipmap, StormVTShaderIDs.m_stormVT_SnowTerrain, StormVTShaderIDs.m_stormVT_SnowTerrainOn, true);
            }
            else
            {
                SetKeyword(stormVTMaterial, StormVTShaderIDs.m_stormVT_SnowTerrain, StormVTShaderIDs.m_stormVT_SnowTerrainOn, false);
                SetKeyword(stormVTMaterialClipmap, StormVTShaderIDs.m_stormVT_SnowTerrain, StormVTShaderIDs.m_stormVT_SnowTerrainOn, false);
            }

            stormVTSettings.SetSnowVariables(stormVTMaterial);
            stormVTSettings.SetSnowVariables(stormVTMaterialClipmap);
        }
        else
        {
            SetKeyword(stormVTMaterial, StormVTShaderIDs.m_stormVT_SnowTerrain, StormVTShaderIDs.m_stormVT_SnowTerrainOn, false);
            SetKeyword(stormVTMaterialClipmap, StormVTShaderIDs.m_stormVT_SnowTerrain, StormVTShaderIDs.m_stormVT_SnowTerrainOn, false);
        }

        SetKeyword(stormVTMaterial, StormVTShaderIDs.m_stormVT_DisplacementTerrain, StormVTShaderIDs.m_stormVT_DisplacementTerrainOn, stormVTSettings.m_displacement);
        SetKeyword(stormVTMaterialClipmap, StormVTShaderIDs.m_stormVT_DisplacementTerrain, StormVTShaderIDs.m_stormVT_DisplacementTerrainOn, stormVTSettings.m_displacement);
        
    #endif
    }

}
