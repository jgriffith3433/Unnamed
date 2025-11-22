using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
#endif

namespace ProceduralWorlds.Storm.Terrain
{
    [ExecuteAlways]
    public class StormVTManager : MonoBehaviour
    {

        #region Scriptable Objects

        /// <summary>
        /// Reference to the storm terrain settings scriptable object
        /// </summary>
        public StormVTSettings stormVTSettings;

        /// <summary>
        /// Reference to the storm terrain grid scriptable object
        /// </summary>
        public StormVTTerrainGrid stormVTTerrainGrid;

        /// <summary>
        /// Reference to the storm terrain texture layers scriptable object
        /// </summary>
        public StormVTTerrainTextureLayers stormVTTerrainTextureLayers;

        /// <summary>
        /// Reference to the storm terrain data container scriptable object.
        /// This contains TerrainData GUIDs, used for loading and unloading at will
        /// So as to not load all TerrainData assets at once, which would take up a lot of memory
        /// </summary>
        [HideInInspector]
        public StormVTTerrainDataContainer stormVTTerrainDataContainer;

        #endregion

        #region Storm Terrain Components

        /// <summary>
        /// Reference to the physical texture manager component
        /// </summary>
        public StormVTPhysicalTextureManager stormVTPhysicalTextureManager;

        /// <summary>
        /// Reference to the terrain tile component
        /// </summary>
        public StormVTTerrainTile stormVTTerrainTile;

        /// <summary>
        /// Reference to the quad tree component
        /// </summary>
        public StormVTQuadTree stormVTQuadTree;

        /// <summary>
        /// Reference to the clipmap component
        /// </summary>
        public StormVTClipmap stormVTClipmap;

        /// <summary>
        /// Reference to the page table clipmap component
        /// </summary>
        public StormVTPageTableClipmap stormVTPageTableClipmap;

        /// <summary>
        /// Reference to the terrain texture manager
        /// </summary>
        public StormVTTerrainTextureManager stormVTTerrainTextureManager;

        /// <summary>
        /// Reference to the terrain collider manager
        /// </summary>
        public StormVTTerrainColliderManager stormVTTerrainColliderManager;

        /// <summary>
        /// Reference to the feedback manager
        /// </summary>
        public StormVTFeedbackManager stormVTFeedbackManager;

        /// <summary>
        /// Reference to the feedback clipmap component
        /// </summary>
        public StormVTFeedbackClipmap stormVTFeedbackClipmap;

        #endregion

        #region Terrain Settings


        [Header("Terrain Settings")]

        /// <summary>
        /// The minimum position of all terrains in the scene
        /// </summary>
        private Vector2 minTerrainBounds;

        /// <summary>
        /// The maximum position of all terrains in the scene
        /// </summary>
        private Vector2 maxTerrainBounds;

        /// <summary>
        /// The number of terrains in the grid in the X dimension
        /// </summary>
        private int m_numTerrainsX;

        /// <summary>
        /// The number of terrains in the grid in the Y dimension
        /// </summary>
        private int m_numTerrainsY;

        /// <summary>
        /// The overall height scale used by all terrains in the scene
        /// </summary>
        private float m_terrainHeightScale = 2048;

        /// <summary>
        /// The overall height offset used by all terrains in the scene
        /// </summary>
        private float m_terrainHeightOffset = 0;
#if UNITY_EDITOR
        /// <summary>
        /// Thr resolution of the heightmap used on all terrains in the scene
        /// </summary>
        private int m_heightmapResolution = 0;

        /// <summary>
        /// Thr resolution of the heightmap used on all terrains in the scene
        /// </summary>
        private int m_terrainHeightmapSize = 2048;


        /// <summary>
        /// The overal size in meters of the terrains used in the scene
        /// </summary>
        private float m_terrainSize = 0;
#endif

        /// <summary>
        /// The center position of total terrain bounds
        /// </summary>
        private Vector2 m_terrainBoundsCenter;

        /// <summary>
        /// The size of the total terrain bounds
        /// </summary>
        private Vector2 m_terrainBoundsSize;

        /// <summary>
        /// The texel size (Pixels per meter) of the terrain
        /// </summary>
        private float m_terrainTexelSize;

        /// <summary>
        /// The lowest zoom level to create tiles from
        /// </summary>
        private VTZoomLayer m_lowestZoomLayer;

        /// <summary>
        /// Highest zoom level to create tiles from
        /// </summary>
        private VTZoomLayer m_highestZoomLayer;

        /// <summary>
        /// The lowest zoom level to create tiles from
        /// </summary>
        private int m_lowestZoomLevel;

        /// <summary>
        /// Highest zoom level to create tiles from
        /// </summary>
        private int m_highestZoomLevel;

        #endregion

        [Header("Physical Texture settings")]

        /// <summary>
        /// Prevents too many textures being created and saved in memory all at once
        /// </summary>
        public int m_maxMemoryAllocationSaving = 1000;

        /// <summary>
        /// Tracks the minimum posiition of the minmum terrain in the grid
        /// </summary>
        private Vector3 m_minTerrainPos;

        /// <summary>
        /// Tracks the maximum position of the maximum terrain in the grid
        /// </summary>
        private Vector3 m_maxTerrainPos;

#if UNITY_EDITOR
        /// <summary>
        /// Tracks how many normal maps have been saved out, used for progress bar
        /// </summary>
        private int normalmapCount = 0;
#endif

        /// <summary>
        /// A single large render texture used to store the merged heightmap of neighbouring terrains
        /// </summary>
        private RenderTexture m_mergedTerrainHeightmap;

        /// <summary>
        /// A single large render texture used to store the merged curvature of neighbouring terrains
        /// </summary>
        private RenderTexture m_mergedTerrainCurvature;

        /// <summary>
        /// A single render texture used to store the normal of a single terrain
        /// </summary>
        private RenderTexture m_singleTerrainNormal;

        /// <summary>
        /// A single render texture used to store the curvature of a single terrain
        /// </summary>
        private RenderTexture m_singleTerrainCurvature;

        /// <summary>
        /// A single render texture used to store the merged normal map that contains curvature in its alpha channel
        /// </summary>
        private RenderTexture m_normalMapWithCurvature;

        /// <summary>
        /// The maximum amount of textures to save out at a time, used for preventing memory overload.
        /// </summary>
        public int m_terrainTextureSavingLimit = 4;


        #region Editor Update
        /// <summary>
        /// Tracks the current coroutine running for editor operations
        /// </summary>
        private IEnumerator m_currentCoroutine;

        /// <summary>
        /// Tracks the time since the last editor update
        /// </summary>
        private DateTime m_timeSinceLastUpdate;
#if UNITY_EDITOR

        /// <summary>
        /// Used to stall a coroutine for a number of miliseconds
        /// </summary>
        private int m_currentWaitTimeMS = 0;

        /// <summary>
        /// The frequency at which an editor update occurs
        /// </summary>
        private int m_UpdateFrequencyMS = 250;

        /// <summary>
        /// Prevent too many updates being called e.g. when painting on the terrain
        /// A single click while terrain painting can fire this event 10+ times, so a delay is needed
        /// </summary>
        private int m_textureUpdateThresholdMS = 5000;

        /// <summary>
        /// Keeps track of the last time the terrain texture update was called
        /// </summary>
        private long m_lastTextureUpdateTimeStamp;

        /// <summary>
        /// Keeps track of the last name of the terrain that was updated
        /// </summary>
        private string m_lastTextureUpdateTerrainName;
#endif
        #endregion


        /// <summary>
        /// A struct that contains small information about the terrain's position.
        /// Used for sorting into a grid structure
        /// </summary>
        public struct TerrainPosData
        {
            public Vector3 terrainPos;
            public float terrainHeight;
            public string terrainDataPath;
        }

        /// <summary>
        /// A list to keep track of all terrain positional data, for sorting into a grid
        /// </summary>
        private List<TerrainPosData> m_terrainPosData;

        #region Event System
        /// <summary>
        /// Called when the camera reference on this script is changed
        /// </summary>
        /// <param name="camera"></param>
        public delegate void UpdateStormVTCamera(Camera camera);
        public static event UpdateStormVTCamera OnUpdateStormVTCamera;

        /// <summary>
        /// Camera used in the Storm Terrain System
        /// </summary>
        public Camera MainCamera;

        /// <summary>
        /// The last known camera that was in use by the Storm Terrain System
        /// </summary>
        private Camera PrevCamera;

        #endregion


        /// <summary>
        /// Returns the main camera that this system finds.
        /// Used for when the camera reference is null
        /// </summary>
        /// <returns></returns>
        public Camera GetMainCamera()
        {

            Camera camera = Camera.main;

            if (camera == null || camera.enabled == false)
            {
                List<Camera> cameras = StormVTUtilities.FindObjectsByType<Camera>(false).ToList();
                for (int i = 0; i < cameras.Count; i++)
                {
                    if (!cameras[i].name.Equals("TileCapturer") && !cameras[i].name.Equals("FrustumCamera") && cameras[i].enabled)
                    {
                        camera = cameras[i];
                    }
                }
            }

            return camera;
        }

        /// <summary>
        /// Checks if the camera reference has changed since the last update
        /// </summary>
        public void CheckCameraUpdate()
        {
            if (MainCamera == null)
            {
                MainCamera = GetMainCamera();
                PrevCamera = MainCamera;
                UpdateCameraReferences();
            }
            else
            {
                if (PrevCamera != MainCamera)
                {
                    PrevCamera = MainCamera;
                    UpdateCameraReferences();
                }
            }

        }

        /// <summary>
        /// Calls the event to update all components that reference the main camera
        /// </summary>
        /// <param name="camera">Camera to assign</param>
        public static void SetStormTerrainCamera(Camera camera)
        {
            if(OnUpdateStormVTCamera != null)
            {
                OnUpdateStormVTCamera(camera);
            }
        }

        /// <summary>
        /// Overrides the referenced camera that Storm Terrain uses
        /// </summary>
        /// <param name="camera">Camera to assign</param>
        public void SetStormVTCamera(Camera camera)
        {
            MainCamera = camera;
            UpdateCameraReferences();
        }

        /// <summary>
        /// Check if the camera reference has changed since last update
        /// </summary>
        public void Update()
        {
            CheckCameraUpdate();
        }

        /// <summary>
        /// Update all the components that reference the Storm Terrain Camera
        /// </summary>
        public void UpdateCameraReferences()
        {
            if(stormVTClipmap != null)
                stormVTClipmap.m_mainCamera = MainCamera;
            if(stormVTQuadTree != null)
            {
                stormVTQuadTree.m_mainCamera = MainCamera;
            }
            if(stormVTPageTableClipmap != null)
                stormVTPageTableClipmap.m_mainCamera = MainCamera;
            if(stormVTTerrainTextureManager != null)
                stormVTTerrainTextureManager.m_mainCamera = MainCamera;
            if (stormVTTerrainColliderManager != null)
                stormVTTerrainColliderManager.m_mainCamera = MainCamera;
            if (stormVTFeedbackManager != null)
                stormVTFeedbackManager.m_mainCamera = MainCamera;
            if(stormVTFeedbackClipmap != null)
                stormVTFeedbackClipmap.m_mainCamera = MainCamera;
        }

        /// <summary>
        /// When changed, resubscribe to events and check for a camera update
        /// </summary>
        public void OnValidate()
        {
            SubscribeEvents();
            CheckCameraUpdate();
        }

        /// <summary>
        /// Subscribe to events and set default values on terrain variables
        /// </summary>
        public void OnEnable()
        {
            SubscribeEvents();
            if (stormVTTerrainGrid != null)
            {
                m_terrainHeightScale = stormVTTerrainGrid.m_terrainHeightScale;
                m_terrainHeightOffset = stormVTTerrainGrid.m_terrainHeightOffset;
            }

            if (stormVTSettings != null)
                stormVTSettings.m_stormTerrainActive = true;

        }

        /// <summary>
        /// Set default values on terrain variables
        /// </summary>
        public void OnAwake()
        {
            if(stormVTTerrainGrid != null)
            {
                m_terrainHeightScale = stormVTTerrainGrid.m_terrainHeightScale;
                m_terrainHeightOffset = stormVTTerrainGrid.m_terrainHeightOffset;
            }

            if(stormVTSettings != null)
                stormVTSettings.m_stormTerrainActive = true;
        }

        /// <summary>
        /// Unsubscribe to the events, stop the editor updates
        /// </summary>
        public void OnDestroy()
        {
            UnSubscribeEvents();
            StopEditorUpdates();
        }

        /// <summary>
        /// Unsubscribe to the events, reset draw modes if in a build
        /// </summary>
        public void OnDisable()
        {
            UnSubscribeEvents();

            if (stormVTSettings != null)
                stormVTSettings.m_stormTerrainActive = false;

#if UNITY_EDITOR
            if (!UnityEditor.BuildPipeline.isBuildingPlayer)
            {
                if (playmodeState == PlayModeStateChange.EnteredEditMode)
                {
                    if (stormVTSettings.m_stormVTMode == StormVTEnums.StormVTMode.Clipmap && stormVTSettings.m_disableGaiaTerrainLoadedScenes)
                    {
                        if (StormVTGaia.Instance.IsGaiaTerrainLoadedScene())
                        {
                            StormVTGaia.Instance.SetTerrainLoadingEnabled(true);
                        }
                    }
                    stormVTSettings.m_drawClipmap = false;
                }
            }
#endif

        }

        /// <summary>
        /// Subscribe to terrain callback events
        /// </summary>
        public void SubscribeEvents()
        {
#if UNITY_EDITOR
            TerrainCallbacks.textureChanged += OnTerrainTextureChanged;
            TerrainCallbacks.heightmapChanged += OnTerrainHeightmapChanged;
            EditorSceneManager.sceneSaving += OnSceneSaving;
            EditorApplication.playModeStateChanged += OnPlaymodeStateChanged;
#endif
            StormVTManager.OnUpdateStormVTCamera += SetStormVTCamera;
        }

        /// <summary>
        /// Unsubscribe from terrain callback events
        /// </summary>
        public void UnSubscribeEvents()
        {
#if UNITY_EDITOR
            TerrainCallbacks.textureChanged -= OnTerrainTextureChanged;
            TerrainCallbacks.heightmapChanged -= OnTerrainHeightmapChanged;
            EditorSceneManager.sceneSaving -= OnSceneSaving;
            EditorApplication.playModeStateChanged -= OnPlaymodeStateChanged;
#endif
            StormVTManager.OnUpdateStormVTCamera -= SetStormVTCamera;
        }

#if UNITY_EDITOR
        private PlayModeStateChange playmodeState;
#endif

#if UNITY_EDITOR
        /// <summary>
        /// Updates data on the grid asset if the playmode state changed
        /// </summary>
        /// <param name="change"></param>
        private void OnPlaymodeStateChanged(PlayModeStateChange change)
        {
            playmodeState = change;
            //Check for null terrain layers
            FetchTerrainGrid();
            if(stormVTTerrainGrid != null)
            {
                if(stormVTTerrainGrid.m_gridList.Length > 0)
                {
                    if (stormVTTerrainGrid.m_gridList[0].terrainTileLayers == null)
                    {
                        stormVTTerrainGrid.UpdateTerrainLayerGridData();
                        stormVTTerrainGrid.UpdateTerrainIndices();
                        stormVTTerrainGrid = null;

                    }
                    else if (stormVTTerrainGrid.m_gridList[0].terrainTileLayers.Length < 1)
                    {
                        stormVTTerrainGrid.UpdateTerrainLayerGridData();
                        stormVTTerrainGrid.UpdateTerrainIndices();
                        stormVTTerrainGrid = null;

                    }
                }
            }
            

            if(stormVTTerrainGrid != null)
            {
                EditorUtility.SetDirty(stormVTTerrainGrid);
            }

            UpdateTerrainData();
            if(change == PlayModeStateChange.EnteredPlayMode)
            {
                if(!UnityEditor.BuildPipeline.isBuildingPlayer)
                {
                    if (stormVTSettings != null && stormVTSettings.m_stormVTMode == StormVTEnums.StormVTMode.Clipmap && stormVTSettings.m_disableGaiaTerrainLoadedScenes)
                    {
                        if (StormVTGaia.Instance.IsGaiaTerrainLoadedScene())
                        {

                            StormVTGaia.Instance.SetTerrainLoadingEnabled(false);
                            StormVTGaia.Instance.UnloadAllGaiaTerrains(true);

                            StormVTGaia.Instance.ForceUnloadGaiaTerrains();
                        }
                    }
                }
                if(stormVTSettings != null)
                {
                    if (stormVTSettings.m_stormVTMode == StormVTEnums.StormVTMode.Clipmap)
                        stormVTSettings.m_drawClipmap = true;
                }



            }
            if (change == PlayModeStateChange.ExitingPlayMode)
            {
                if (StormVTGaia.Instance.IsGaiaTerrainLoadedScene())
                {
                    StormVTGaia.Instance.SetTerrainLoadingEnabled(true);
                }
                if (stormVTSettings != null)
                    stormVTSettings.m_drawClipmap = false;
            }
        }
#endif

#if UNITY_EDITOR
        /// <summary>
        /// Update the terrain data when the scene is saved
        /// </summary>
        /// <param name="scene">Scene callback reference</param>
        /// <param name="path">Path of the scene</param>
        private void OnSceneSaving(Scene scene, string path)
        {
            UpdateTerrainData();

            if(stormVTSettings != null)
            {
                if (stormVTSettings.m_stormVTMode == StormVTEnums.StormVTMode.Clipmap)
                {
                    StartTerrainDataListExportEditorUpdates();
                }
            }


        }
#endif
 
        /// <summary>
        /// Re-save out the heightmap is it was changed
        /// </summary>
        /// <param name="terrain">Terrain that was altered</param>
        /// <param name="heightRegion">Height region on the heightmap</param>
        /// <param name="synched">Whether the change was synced or not</param>
        private void OnTerrainHeightmapChanged(UnityEngine.Terrain terrain, RectInt heightRegion, bool synched)
        {
#if UNITY_EDITOR
            if (this == null)
            {
                UnSubscribeEvents();
                return;
            }
            EditorUtility.SetDirty(terrain);


            string sceneName = SceneManager.GetActiveScene().name;
            string m_userDataFolderPath = $"{StormVTConstants.GetAssetsFolder(sceneName)}";
            if (stormVTTerrainGrid == null)
            {
                string vtGridAssetFilePath = $"{sceneName}_TerrainGrid.asset";
                string vtGridFilePath = $"{m_userDataFolderPath}/{vtGridAssetFilePath}";

                stormVTTerrainGrid = AssetDatabase.LoadAssetAtPath<StormVTTerrainGrid>(vtGridFilePath);
            }

            if (stormVTTerrainGrid != null)
            {
                StormVTTerrainInfo terrainInfo = terrain.GetComponent<StormVTTerrainInfo>();
                if (terrainInfo != null)
                {
                    int2 gridIndex = terrainInfo.terrainGridIndex;

                    if (gridIndex.x != -1 && gridIndex.y != -1)
                    {
                        stormVTTerrainGrid.m_gridList[stormVTTerrainGrid.GetIndex(gridIndex.x, gridIndex.y)].heightmapChanged = true;
                    }
                }
            }

           
#endif
        }

        /// <summary>
        /// Re-save out all terrain textures if changed
        /// </summary>
        /// <param name="terrain">Terrain reference</param>
        /// <param name="textureName">Name of the terrain</param>
        /// <param name="texelRegion">Area on the texture that was affected</param>
        /// <param name="synched">Whether the change was synced or not</param>
        private void OnTerrainTextureChanged(UnityEngine.Terrain terrain, string textureName, RectInt texelRegion, bool synched)
        {
#if UNITY_EDITOR
            //prevent too many updates being called e.g. when painting on the terrain - a single click while terrain painting can fire this event 10+ times
            if (terrain !=null && terrain.name == m_lastTextureUpdateTerrainName && m_lastTextureUpdateTimeStamp + m_textureUpdateThresholdMS > StormVTUtilities.GetUnixTimestamp())
            {
                return;
            }
            m_lastTextureUpdateTimeStamp = StormVTUtilities.GetUnixTimestamp();

            if (terrain != null)
            {
                m_lastTextureUpdateTerrainName = terrain.name;
            }
            else
            {
                m_lastTextureUpdateTerrainName = "";
            }
            //prevent stray subscriptions from still triggering the update events
            if (this == null || this.gameObject.scene.name == null )
            {
                UnSubscribeEvents();
                return;
            }

            EditorUtility.SetDirty(terrain);

            StormVTTerrainInfo terrainInfo = terrain.GetComponent<StormVTTerrainInfo>();
            if(terrainInfo == null || stormVTSettings == null)
            {
                return;
            }
            int2 gridIndex = terrainInfo.terrainGridIndex;
            if (stormVTTerrainGrid == null)
            {
                FetchTerrainGrid();
            }
            if(stormVTTerrainGrid != null)
            {
                if (gridIndex.x != -1 && gridIndex.y != -1)
                {
                    int index = stormVTTerrainGrid.GetIndex(gridIndex.x, gridIndex.y);
                    stormVTTerrainGrid.m_gridList[stormVTTerrainGrid.GetIndex(gridIndex.x, gridIndex.y)].texturesChanged = true;
                    List<TerrainLayer> terrainLayers = terrain.terrainData.terrainLayers.ToList();
                    stormVTTerrainGrid.m_gridList[index].terrainLayers = terrainLayers;
                    stormVTTerrainGrid.m_gridList[index].terrainTileLayers = StormVTTerrainGrid.SetTerrainTileLayers(terrainLayers, stormVTTerrainGrid.m_terrainSize, terrain.name);
                    EditorUtility.SetDirty(stormVTTerrainGrid);
                }
            }
            if(stormVTTerrainTextureLayers != null)
                stormVTTerrainTextureLayers.RefreshTerrainLayers(terrain, false);
#endif
        }

        /// <summary>
        /// Resaves out all the height and normal maps for all terrains
        /// </summary>
        public void UpdateTerrainData()
        {
            if(stormVTTerrainGrid != null)
            {
                for (int x = 0; x < stormVTTerrainGrid.m_numTerrainsX; x++)
                {
                    for (int y = 0; y < stormVTTerrainGrid.m_numTerrainsY; y++)
                    {
                        StormVTTerrainGridData gridData = stormVTTerrainGrid.m_gridList[stormVTTerrainGrid.GetIndex(x, y)];
                        if (gridData.heightmapChanged)
                        {

                            UpdateNormalmap(new int2(x, y));
                            stormVTTerrainGrid.m_gridList[stormVTTerrainGrid.GetIndex(x, y)].heightmapChanged = false;
#if UNITY_EDITOR
                            EditorUtility.SetDirty(stormVTTerrainGrid);
#endif
                        }
                       
                    }
                }
            }
            stormVTTerrainDataContainer = null;

        }


        /// <summary>
        /// Creates the normal map and saves it out
        /// </summary>
        /// <param name="gridIndex">Index of the terrain to re-save out the normalmap</param>
        public void UpdateNormalmap(int2 gridIndex)
        {

#if UNITY_EDITOR

            if (stormVTTerrainDataContainer == null)
            {
                //grid asset not loaded, try to load it in
                FetchTerrainDataContainer();
            }

            if (stormVTTerrainDataContainer != null)
            {
                int terrainSize = stormVTTerrainGrid.m_heightmapResolution;

                int singleTerrainSize = terrainSize - 1;
                int finalBigSize = (terrainSize - 1) * 3;

                m_mergedTerrainHeightmap = new RenderTexture(finalBigSize, finalBigSize, 0,
                    RenderTextureFormat.R16, RenderTextureReadWrite.Linear);

                m_mergedTerrainCurvature = new RenderTexture(finalBigSize, finalBigSize, 0,
                    RenderTextureFormat.R8, RenderTextureReadWrite.Linear);

                m_singleTerrainCurvature = new RenderTexture(terrainSize, terrainSize, 0, RenderTextureFormat.R8, RenderTextureReadWrite.Linear);
                m_normalMapWithCurvature = new RenderTexture(terrainSize, terrainSize, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
                m_singleTerrainNormal = new RenderTexture(terrainSize, terrainSize, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);


                int index = gridIndex.x * stormVTTerrainGrid.m_numTerrainsY + gridIndex.y;

                string assetsFolderPath = StormVTConstants.GetTerrainTextureAssetsFolder();

                StormVTTerrainGridData terrainGridData = stormVTTerrainGrid.m_gridList[stormVTTerrainGrid.GetIndex(gridIndex.x, gridIndex.y)];
                TerrainPosData currentTerrain = GetTerrainAtPosition(terrainGridData.position);

                if (currentTerrain.terrainPos != Vector3.positiveInfinity)
                {
                    RenderTexture normalMapWithCurvature = CreateCurvature(gridIndex.x, gridIndex.y, stormVTTerrainGrid.m_gridList, terrainSize);
                    RenderTexture.active = normalMapWithCurvature;


                    Texture2D tempTerrainNormalMapTex2D = new Texture2D(terrainSize, terrainSize, TextureFormat.RGBA32, false, true);
                    tempTerrainNormalMapTex2D.ReadPixels(new Rect(0, 0, terrainSize, terrainSize), 0, 0);
                    tempTerrainNormalMapTex2D.Apply();

                    tempTerrainNormalMapTex2D.name = $"Terrain_{terrainGridData.gridIndex.x}_{terrainGridData.gridIndex.y}_NormalMap";

                    string filePath = "";

                    try
                    {
                        AssetDatabase.StartAssetEditing();
                        filePath = SaveNormalmapTexture2DToDiskNoImport(assetsFolderPath, tempTerrainNormalMapTex2D);
                    }
                    finally
                    {
                        AssetDatabase.StopAssetEditing();
                        AssetDatabase.SaveAssets();
                        AssetDatabase.Refresh();
                    }

                    try
                    {
                        AssetDatabase.StartAssetEditing();
                        SaveWorldNormalmapTexture2DToDiskWithImport(assetsFolderPath, tempTerrainNormalMapTex2D);
                    }
                    finally
                    {
                        AssetDatabase.StopAssetEditing();
                        AssetDatabase.SaveAssets();
                        AssetDatabase.Refresh();
                    }

                    EditorUtility.SetDirty(stormVTTerrainGrid);



                    DestroyImmediate(tempTerrainNormalMapTex2D);



                    RenderTexture.active = null;
                }

                m_mergedTerrainHeightmap.Release();
                m_mergedTerrainCurvature.Release();
                m_singleTerrainCurvature.Release();
                m_singleTerrainNormal.Release();
                m_normalMapWithCurvature.Release();

                DestroyImmediate(m_mergedTerrainCurvature);
                DestroyImmediate(m_mergedTerrainHeightmap);
                DestroyImmediate(m_singleTerrainCurvature);
                DestroyImmediate(m_singleTerrainNormal);
                DestroyImmediate(m_normalMapWithCurvature);
            }

            stormVTTerrainDataContainer = null;
#endif

        }

        /// <summary>
        /// Set terrain bottom left position and check if the camera has changed
        /// </summary>
        public void Start()
        {
            stormVTTerrainGrid.SetTerrainPivot();
            CheckCameraUpdate();
        }

        /// <summary>
        /// Remove the terrainInfo components from all terrains
        /// </summary>
        public void RemoveStormTerrain()
        {
            //Check for gaia loaded scene setup
            if (StormVTGaia.Instance.IsGaiaTerrainLoadedScene())
            {
                void Act(UnityEngine.Terrain t)
                {

                    StormVTTerrainInfo terrainInfo = t.gameObject.GetComponent<StormVTTerrainInfo>();
                    if(terrainInfo != null)
                        GameObject.DestroyImmediate(terrainInfo);

                    t.materialTemplate = StormVTDefaults.GetDefaultTerrainMaterial();
                }

                StormVTGaia.Instance.CallFunctionOnGaiaLoadedTerrains(Act, true, null, "Removing Data From Terrain Scenes...");
                StormVTGaia.Instance.UnloadAllGaiaTerrains(true);
            }
            else
            {
                List<UnityEngine.Terrain> terrains = StormVTUtilities.FindObjectsByType<UnityEngine.Terrain>(true).ToList();
                for (int i = 0; i < terrains.Count; i++)
                {
                    UnityEngine.Terrain t = terrains[i];

                    StormVTTerrainInfo terrainInfo = t.gameObject.GetComponent<StormVTTerrainInfo>();
                    if (terrainInfo != null)
                        GameObject.DestroyImmediate(terrainInfo);

                    t.materialTemplate = StormVTDefaults.GetDefaultTerrainMaterial();
                }
            }
            UnSubscribeEvents();
            Debug.Log("Storm Terrain: Finished Removing Storm Terrain at: " + DateTime.Now.ToLongTimeString());
        }

        #region Terrain Quad Tree Function

        /// <summary>
        /// Returns TerrainPosData based on the position passed in from the terrain grid
        /// </summary>
        /// <param name="position">Position to search a terrain for</param>
        /// <returns></returns>
        public TerrainPosData GetTerrainAtPositionFromGrid(Vector2 position)
        {
            // Get closest terrain to position
            TerrainPosData closestTerrain = new TerrainPosData();
            closestTerrain.terrainPos = Vector3.positiveInfinity;
            float shortestDistance = float.MaxValue;

            if (stormVTTerrainGrid == null)
            {
                FetchTerrainGrid();

            }

            for (int i = 0; i < m_terrainPosData.Count; i++)
            {
                TerrainPosData terrain = m_terrainPosData[i];
                Vector3 terrainPos = terrain.terrainPos;
                Vector2 terrainPos2D = new Vector2(terrainPos.x, terrainPos.z);
                float distance = Vector2.Distance(position, terrainPos2D);
                if (distance < shortestDistance)
                {
                    closestTerrain = terrain;
                    shortestDistance = distance;
                }
            }

            return closestTerrain;

        }

        /// <summary>
        /// Returns TerrainPosData based on the position passed in
        /// </summary>
        /// <param name="position">Position to search a terrain for</param>
        /// <returns></returns>
        public TerrainPosData GetTerrainAtPosition(Vector2 position)
        {

            // Get closest terrain to position
            TerrainPosData closestTerrain = new TerrainPosData();
            closestTerrain.terrainPos = Vector3.positiveInfinity;
            float shortestDistance = float.MaxValue;

            if (stormVTTerrainGrid == null)
            {
                FetchTerrainGrid();
            }
            if(stormVTTerrainDataContainer == null)
            {
                FetchTerrainDataContainer();
            }

            if(stormVTTerrainGrid != null && stormVTTerrainDataContainer != null)
            {
                for (int i = 0; i < stormVTTerrainGrid.m_gridList.Length; i++)
                {
                    Vector2 terrainPos2D = new Vector2(stormVTTerrainGrid.m_gridList[i].position.x, stormVTTerrainGrid.m_gridList[i].position.y);
                    float distance = Vector2.Distance(position, terrainPos2D);
                    if (distance < shortestDistance)
                    {
                        closestTerrain = new TerrainPosData();
                        closestTerrain.terrainPos = new Vector2(stormVTTerrainGrid.m_gridList[i].position.x, stormVTTerrainGrid.m_gridList[i].position.y);
                        closestTerrain.terrainDataPath = stormVTTerrainDataContainer.m_terrainDataList[i];
                        shortestDistance = distance;
                    }
                }
            }

            return closestTerrain;
        }

        /// <summary>
        /// Retrieves the terrain grid scriptable object
        /// </summary>
        public void FetchTerrainGrid()
        {
#if UNITY_EDITOR
            string sceneName = SceneManager.GetActiveScene().name;
            string m_userDataFolderPath = $"{StormVTConstants.GetAssetsFolder(sceneName)}";
            string m_oldUserDataFolderPath = $"{StormVTConstants.GetAssetsFolderOld(sceneName)}";
            if (stormVTTerrainGrid == null)
            {
                string vtGridAssetFilePath = $"{sceneName}_TerrainGrid.asset";
                string vtGridFilePath = $"{m_userDataFolderPath}/{vtGridAssetFilePath}";

                if(System.IO.File.Exists(vtGridFilePath))
                {
                    vtGridFilePath = $"{m_oldUserDataFolderPath}/{vtGridAssetFilePath}";
                }

                stormVTTerrainGrid = AssetDatabase.LoadAssetAtPath<StormVTTerrainGrid>(vtGridFilePath);
            }
#endif
        }

        /// <summary>
        /// Retrieves the terrain data container scriptable object
        /// </summary>
        public void FetchTerrainDataContainer()
        {
#if UNITY_EDITOR
            //grid asset not loaded, try to load it in
            Scene activeScene = SceneManager.GetActiveScene();
            string vtAssetFilePath = $"{activeScene.name}_TerrainDataContainer.asset";
            string vtGridFilePath = $"{StormVTConstants.GetAssetsFolder(activeScene.name)}/{vtAssetFilePath}";

            if(!System.IO.File.Exists(vtGridFilePath))
            {
                vtGridFilePath = $"{StormVTConstants.GetAssetsFolderOld(activeScene.name)}/{vtAssetFilePath}";
            }

            //Load asset at the file path
            stormVTTerrainDataContainer =
                (StormVTTerrainDataContainer)AssetDatabase.LoadAssetAtPath(vtGridFilePath, typeof(StormVTTerrainDataContainer));
#endif
        }

        /// <summary>
        /// Retrieves the terrain texutte layers scriptable object
        /// </summary>
        public void FetchTerrainLayers()
        {
#if UNITY_EDITOR
            if (stormVTTerrainTextureLayers == null)
            {
                Scene activeScene = SceneManager.GetActiveScene();
                string vtAssetFilePath = $"{activeScene.name}_TerrainTextureLayers.asset";
                string vtTerrainLayersFilePath = $"{StormVTConstants.GetAssetsFolder(activeScene.name)}/{vtAssetFilePath}";

                if (!System.IO.File.Exists(vtTerrainLayersFilePath))
                {
                    vtTerrainLayersFilePath = $"{StormVTConstants.GetAssetsFolderOld(activeScene.name)}/{vtAssetFilePath}";
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
        /// Creates a unique terrain material for the passed in terrain. 
        /// Used for editor side material for supporting more than 8 terrain layers
        /// </summary>
        /// <param name="terrain">Terrain to generate material for</param>
        /// <returns></returns>
        private Material CreatePerTerrainMaterial(UnityEngine.Terrain terrain)
        {
#if UNITY_EDITOR
            string dataFolder = StormVTConstants.GetTerrainTextureAssetsFolder();

            string vtMaterialAssetFilePath = $"{terrain.name}_Mat.mat";
            string vtMaterialFilePath = $"{dataFolder}/{vtMaterialAssetFilePath}";

            Material stormVTMat = (Material)AssetDatabase.LoadAssetAtPath(vtMaterialFilePath, typeof(Material));
            if (stormVTMat == null)
            {
                stormVTMat = new Material(StormVTDefaults.GetVirtualTerrainMaterial());
                AssetDatabase.CreateAsset(stormVTMat, vtMaterialFilePath);
                stormVTMat = (Material)AssetDatabase.LoadAssetAtPath(vtMaterialFilePath, typeof(Material));
            }
            return stormVTMat;
#else
            return null;
#endif

        }

#if UNITY_EDITOR
        /// <summary>
        /// Get all terrains in the scene
        /// </summary>
        public void GetAllTerrainsInScene()
        {
            stormVTTerrainTextureLayers.stormVTTerrainTextureLayers.Clear();
            //Check for gaia loaded scene setup
            if (StormVTGaia.Instance.IsGaiaTerrainLoadedScene())
            {
                void Act(UnityEngine.Terrain t)
                {
                    //terrains.Add(t);
                    m_terrainSize = t.terrainData.size.x;
                    //Set terrain heightmap texture size
                    m_terrainHeightmapSize = (t.terrainData.heightmapResolution) - 1;

                    //Texel size of terrain setup (e.g number of pixels per meter)
                    m_terrainTexelSize = m_terrainSize / m_terrainHeightmapSize;

                    m_terrainHeightScale = t.terrainData.heightmapScale.y;
                    m_terrainHeightOffset = t.transform.position.y;
                    m_heightmapResolution = t.terrainData.heightmapResolution;

                    StormVTTerrainInfo terrainInfo = t.gameObject.GetComponent<StormVTTerrainInfo>();
                    if(terrainInfo == null)
                        terrainInfo = t.gameObject.AddComponent<StormVTTerrainInfo>();
                    terrainInfo.m_terrrainOrigin = t.GetPosition();
                    terrainInfo.m_terrainSize = m_terrainSize;
                    terrainInfo.terrain = t;
                    terrainInfo.stormVTSettings = stormVTSettings;
                    terrainInfo.normalmap = null;
                    if (terrainInfo.virtualTerrainMat == null)
                    {
                        terrainInfo.virtualTerrainMat = CreatePerTerrainMaterial(t);
                    }
                    terrainInfo.UpdateTerrainLayers();


                    if (t.GetPosition().x <= m_minTerrainPos.x && t.GetPosition().z <= m_minTerrainPos.y)
                    {
                        m_minTerrainPos.x = t.GetPosition().x;
                        m_minTerrainPos.y = t.GetPosition().z;
                    }

                    if (t.GetPosition().x >= m_maxTerrainPos.x && t.GetPosition().z >= m_maxTerrainPos.y)
                    {
                        m_maxTerrainPos.x = t.GetPosition().x;
                        m_maxTerrainPos.y = t.GetPosition().z;
                    }

                    TerrainPosData terrainPosData = new TerrainPosData();
                    terrainPosData.terrainPos = t.GetPosition();
                    string assetPath = AssetDatabase.GetAssetPath(t.terrainData);
                    if(!string.IsNullOrEmpty(assetPath))
                    {
                        // Get the GUID for the selected asset
                        string guid = AssetDatabase.AssetPathToGUID(assetPath);
                        terrainPosData.terrainDataPath = guid;
                    }
                    
                    m_terrainPosData.Add(terrainPosData);

                    terrainInfo = null;
                    EditorUtility.UnloadUnusedAssetsImmediate();
                }

                StormVTGaia.Instance.CallFunctionOnGaiaLoadedTerrains(Act, true, null, "Adding Data To Terrain Scenes...");
                StormVTGaia.Instance.UnloadAllGaiaTerrains(true);
            }
            else
            {
                List<UnityEngine.Terrain> terrains = StormVTUtilities.FindObjectsByType<UnityEngine.Terrain>(true).ToList();
                for (int i = 0; i < terrains.Count; i++)
                {
                    UnityEngine.Terrain t = terrains[i];

                    m_terrainSize = t.terrainData.size.x;
                    //Set terrain heightmap texture size
                    m_terrainHeightmapSize = (t.terrainData.heightmapResolution) - 1;

                    //Texel size of terrain setup (e.g number of pixels per meter)
                    m_terrainTexelSize = m_terrainSize / m_terrainHeightmapSize;

                    m_terrainHeightScale = t.terrainData.heightmapScale.y;
                    m_terrainHeightOffset = t.transform.position.y;
                    m_heightmapResolution = t.terrainData.heightmapResolution;

                    StormVTTerrainInfo terrainInfo = t.gameObject.GetComponent<StormVTTerrainInfo>();
                    if(terrainInfo == null)
                        terrainInfo = t.gameObject.AddComponent<StormVTTerrainInfo>();
                    terrainInfo.m_terrrainOrigin = t.GetPosition();
                    terrainInfo.m_terrainSize = m_terrainSize;
                    terrainInfo.terrain = t;
                    terrainInfo.stormVTSettings = stormVTSettings;
                    if (terrainInfo.virtualTerrainMat == null)
                    {
                        terrainInfo.virtualTerrainMat = CreatePerTerrainMaterial(t);
                    }
                    terrainInfo.UpdateTerrainLayers();


                    if (t.GetPosition().x <= m_minTerrainPos.x && t.GetPosition().z <= m_minTerrainPos.y)
                    {
                        m_minTerrainPos.x = t.GetPosition().x;
                        m_minTerrainPos.y = t.GetPosition().z;
                    }

                    if (t.GetPosition().x >= m_maxTerrainPos.x && t.GetPosition().z >= m_maxTerrainPos.y)
                    {
                        m_maxTerrainPos.x = t.GetPosition().x;
                        m_maxTerrainPos.y = t.GetPosition().z;
                    }

                    TerrainPosData terrainPosData = new TerrainPosData();
                    terrainPosData.terrainPos = t.GetPosition();
                    string assetPath = AssetDatabase.GetAssetPath(t.terrainData);
                    if (!string.IsNullOrEmpty(assetPath))
                    {
                        // Get the GUID for the selected asset
                        string guid = AssetDatabase.AssetPathToGUID(assetPath);
                        terrainPosData.terrainDataPath = guid;
                    }
                    m_terrainPosData.Add(terrainPosData);
                    terrainInfo = null;
                    EditorUtility.UnloadUnusedAssetsImmediate();
                }
            }
            EditorUtility.UnloadUnusedAssetsImmediate();
        }
#endif

        /// <summary>
        /// Populate the terrain texture layers object with every Unity Terrain Layer found in the scene
        /// </summary>
        public void GetAllTerrainLayers()
        {

            FetchTerrainLayers();

            stormVTTerrainTextureLayers.stormVTTerrainTextureLayers.Clear();
            //Check for gaia loaded scene setup
            if (StormVTGaia.Instance.IsGaiaTerrainLoadedScene())
            {
                void Act(UnityEngine.Terrain t)
                {
                    TerrainLayer[] terrainLayers = t.terrainData.terrainLayers;
                    foreach (TerrainLayer layer in terrainLayers)
                    {
                        bool foundLayer = false;
                        for (int tl = 0; tl < stormVTTerrainTextureLayers.stormVTTerrainTextureLayers.Count; tl++)
                        {
                            if (stormVTTerrainTextureLayers.stormVTTerrainTextureLayers[tl].m_terrainLayerReference == layer)
                            {
                                foundLayer = true;
                                stormVTTerrainTextureLayers.stormVTTerrainTextureLayers[tl].UpdateTerrainLayer(layer);
                                break;
                            }
                        }
                        if (!foundLayer)
                        {
                            StormVTTerrainTextureLayer terrainTextureLayer = new StormVTTerrainTextureLayer();
                            terrainTextureLayer.SetTerrainLayer(layer);
                            stormVTTerrainTextureLayers.stormVTTerrainTextureLayers.Add(terrainTextureLayer);
                        }
                    }
                    terrainLayers = null;
#if UNITY_EDITOR
                    EditorUtility.UnloadUnusedAssetsImmediate();
#endif
                }
                StormVTGaia.Instance.CallFunctionOnGaiaLoadedTerrains(Act, false, null, "Finding New Terrain Layers On Terrain Scenes...");
                StormVTGaia.Instance.UnloadAllGaiaTerrains(true);
            }
            else
            {
                List<UnityEngine.Terrain> terrains = StormVTUtilities.FindObjectsByType<UnityEngine.Terrain>(true).ToList();
                for (int i = 0; i < terrains.Count; i++)
                {
                    UnityEngine.Terrain t = terrains[i];
                    TerrainLayer[] terrainLayers = t.terrainData.terrainLayers;
                    foreach(TerrainLayer layer in terrainLayers)
                    {
                        bool foundLayer = false;
                        for(int tl = 0; tl < stormVTTerrainTextureLayers.stormVTTerrainTextureLayers.Count; tl++)
                        {
                            if (stormVTTerrainTextureLayers.stormVTTerrainTextureLayers[tl].m_terrainLayerReference == layer)
                            {
                                foundLayer = true;
                                stormVTTerrainTextureLayers.stormVTTerrainTextureLayers[tl].UpdateTerrainLayer(layer);
                                break;
                            }
                        }
                        if(!foundLayer)
                        {
                            StormVTTerrainTextureLayer terrainTextureLayer = new StormVTTerrainTextureLayer();
                            terrainTextureLayer.SetTerrainLayer(layer);
                            stormVTTerrainTextureLayers.stormVTTerrainTextureLayers.Add(terrainTextureLayer);
                        }
                    }
                    terrainLayers = null;
#if UNITY_EDITOR
                    EditorUtility.UnloadUnusedAssetsImmediate();
#endif
                }
            }

            stormVTTerrainTextureLayers.RefreshTerrainLayers();
            stormVTTerrainTextureLayers.ConvertTexturesToPackedFormat();
            RenderTexture.active = null;
#if UNITY_EDITOR
            EditorUtility.UnloadUnusedAssetsImmediate();
#endif
        }

        /// <summary>
        /// Returns a grid data object based off the position passed in
        /// </summary>
        /// <param name="position">Position to search for within the grid</param>
        /// <returns></returns>
        public StormVTTerrainGridData GetGridDataAtPosition(Vector2 position)
        {
            StormVTTerrainGridData gridData = null;
            for(int i = 0; i < stormVTTerrainGrid.m_gridList.Length; i++)
            {
                if (stormVTTerrainGrid.m_gridList[i].position.x == position.x && stormVTTerrainGrid.m_gridList[i].position.y == position.y)
                {
                    gridData = stormVTTerrainGrid.m_gridList[i];
                }
            }

            return gridData;
        }

        /// <summary>
        /// Performs a maintainance check on all TerrainInfo components
        /// </summary>
        public void UpdateTerrainInfo()
        {
#if UNITY_EDITOR
            //Check for gaia loaded scene setup
            if (StormVTGaia.Instance.IsGaiaTerrainLoadedScene())
            {


                void Act(UnityEngine.Terrain t)
                {
                    StormVTTerrainInfo terrainInfo = t.GetComponent<StormVTTerrainInfo>();
                    if(terrainInfo != null)
                    {
                        StormVTTerrainGridData gridData = GetGridDataAtPosition(new Vector2(t.transform.position.x, t.transform.position.z));
                        if(gridData != null)
                        {
                            terrainInfo.terrainGridIndex = gridData.gridIndex;
                            string filePath = stormVTTerrainGrid.m_normalmapFilePaths[stormVTTerrainGrid.GetIndex(gridData.gridIndex.x, gridData.gridIndex.y)];
                            terrainInfo.normalmap = AssetDatabase.LoadAssetAtPath<Texture2D>(filePath);
                            terrainInfo.virtualTerrainMat = CreatePerTerrainMaterial(t);
                        }
                    }
                }
                StormVTGaia.Instance.CallFunctionOnGaiaLoadedTerrains(Act, true, null, "Updating Data On Terrain Scenes...");
                StormVTGaia.Instance.UnloadAllGaiaTerrains(true);

            }
            else
            {
                List<UnityEngine.Terrain> terrains = StormVTUtilities.FindObjectsByType<UnityEngine.Terrain>(true).ToList();
                for (int i = 0; i < terrains.Count; i++)
                {
                    UnityEngine.Terrain t = terrains[i];
                    StormVTTerrainInfo terrainInfo = t.GetComponent<StormVTTerrainInfo>();
                    if (terrainInfo != null)
                    {
                        StormVTTerrainGridData gridData = GetGridDataAtPosition(new Vector2(t.transform.position.x, t.transform.position.z));
                        if (gridData != null)
                        {
                            terrainInfo.terrainGridIndex = gridData.gridIndex;
                            string filePath = stormVTTerrainGrid.m_normalmapFilePaths[stormVTTerrainGrid.GetIndex(gridData.gridIndex.x, gridData.gridIndex.y)];
                            terrainInfo.normalmap = AssetDatabase.LoadAssetAtPath<Texture2D>(filePath);
                            terrainInfo.virtualTerrainMat = CreatePerTerrainMaterial(t);
                        }
                    }
                }
            }
#endif
        }

        /// <summary>
        /// Recreates the terrain material on every terrain in the scene
        /// </summary>
        public void PerformMaintenanceOnTerrains()
        {
            //Check for gaia loaded scene setup
            if (StormVTGaia.Instance.IsGaiaTerrainLoadedScene())
            {
                void Act(UnityEngine.Terrain t)
                {

                    StormVTTerrainInfo terrainInfo = t.gameObject.GetComponent<StormVTTerrainInfo>();
                    if (terrainInfo != null)
                    {
                        terrainInfo.virtualTerrainMat = CreatePerTerrainMaterial(t);
                    }

                }

                StormVTGaia.Instance.CallFunctionOnGaiaLoadedTerrains(Act, true, null, "Updating Data On Terrain Scenes...");
                StormVTGaia.Instance.UnloadAllGaiaTerrains(true);
            }
            else
            {
                List<UnityEngine.Terrain> terrains = StormVTUtilities.FindObjectsByType<UnityEngine.Terrain>(true).ToList();
                for (int i = 0; i < terrains.Count; i++)
                {
                    UnityEngine.Terrain t = terrains[i];

                    StormVTTerrainInfo terrainInfo = t.gameObject.GetComponent<StormVTTerrainInfo>();
                    if (terrainInfo != null)
                    {
                        terrainInfo.virtualTerrainMat = CreatePerTerrainMaterial(t);
                    }
                }
            }
        }

        /// <summary>
        /// Determines the min and max positions of the total terrain bounds
        /// </summary>
        /// <param name="terrains">Terrains to look through to determine bounds</param>
        /// <param name="minTerrainPos">Minimum position of the total terrain bounds</param>
        /// <param name="maxTerrainPos">Maximum position of the total terrain bounds</param>
        public void GetTerrainBounds(List<TerrainPosData> terrains, out Vector2 minTerrainPos, out Vector2 maxTerrainPos)
        {
            //Create and fill min and max value for terrain position
            Vector2 minTerrainPosition = Vector2.positiveInfinity;
            Vector2 maxTerrainPosition = Vector2.negativeInfinity;

            //Loop through all terrains and check for newest min value
            for (int i = 0; i < terrains.Count; i++)
            {
                if (terrains[i].terrainPos.x <= minTerrainPosition.x &&
                    terrains[i].terrainPos.z <= minTerrainPosition.y)
                {
                    minTerrainPosition.x = terrains[i].terrainPos.x;
                    minTerrainPosition.y = terrains[i].terrainPos.z;
                }

                if (terrains[i].terrainPos.x >= maxTerrainPosition.x &&
                    terrains[i].terrainPos.z >= maxTerrainPosition.y)
                {
                    maxTerrainPosition.x = terrains[i].terrainPos.x;
                    maxTerrainPosition.y = terrains[i].terrainPos.z;
                }
            }

            minTerrainPos = minTerrainPosition;
            maxTerrainPos = maxTerrainPosition;
        }

        
        /// <summary>
        /// Create all terrain data
        /// </summary>
        public void Initialize()
        {
            FetchTerrainDataContainer();

            FetchTerrainLayers();

            {
#if UNITY_EDITOR

                if (m_terrainPosData != null)
                {
                    m_terrainPosData.Clear();
                }

                m_terrainPosData = new List<TerrainPosData>();


                m_minTerrainPos = Vector2.positiveInfinity;
                m_maxTerrainPos = Vector2.negativeInfinity;

                //Get all terrains in the scene
                GetAllTerrainsInScene();
                
                if (m_terrainPosData == null)
                {
                    Debug.Log("Data was null terrains");
                }

                if (m_terrainPosData.Count == 0)
                {
                    Debug.Log("Found no terrains");
                }

                //If we found any terrains
                if (m_terrainPosData.Count > 0)
                {

                    //Calculate terrain bounds
                    GetTerrainBounds(m_terrainPosData, out minTerrainBounds, out maxTerrainBounds);

                    m_terrainBoundsSize = (maxTerrainBounds + new Vector2(m_terrainSize, m_terrainSize)) - minTerrainBounds;
                    m_terrainBoundsCenter = minTerrainBounds + (m_terrainBoundsSize / 2.0f);

                    //Calculate grid dimensions
                    m_numTerrainsX = (int)(((m_maxTerrainPos.x + m_terrainSize) - m_minTerrainPos.x) / m_terrainSize);
                    m_numTerrainsY = (int)(((m_maxTerrainPos.y + m_terrainSize) - m_minTerrainPos.y) / m_terrainSize);


                    if (stormVTTerrainDataContainer == null)
                    {
                        //grid asset not loaded, try to load it in
                        Scene activeScene = SceneManager.GetActiveScene();
                        string vtAssetFilePath = $"{activeScene.name}_TerrainDataContainer.asset";
                        string vtGridFilePath = $"{StormVTConstants.GetAssetsFolder()}/{vtAssetFilePath}";

                        //Load asset at the file path
                        stormVTTerrainDataContainer =
                            (StormVTTerrainDataContainer)AssetDatabase.LoadAssetAtPath(vtGridFilePath, typeof(StormVTTerrainDataContainer));

                        //Check if the grid asset doesn't exist, if so create a new one
                        if (stormVTTerrainDataContainer == null)
                        {
                            stormVTTerrainDataContainer = ScriptableObject.CreateInstance<StormVTTerrainDataContainer>();
                            AssetDatabase.CreateAsset(stormVTTerrainDataContainer, vtGridFilePath);
                            stormVTTerrainDataContainer =
                                (StormVTTerrainDataContainer)AssetDatabase.LoadAssetAtPath(vtGridFilePath,
                                    typeof(StormVTTerrainDataContainer));
                        }
                    }


                    StormVTTerrainGridData[] m_terrainGridData = new StormVTTerrainGridData[m_numTerrainsX * m_numTerrainsY];


                    stormVTTerrainDataContainer.m_terrainDataList = new string[m_numTerrainsX * m_numTerrainsY];

                    int c = 0;

                    //Set Terrain Grid
                    for (int x = 0; x < m_numTerrainsX; x++)
                    {
                        for (int y = 0; y < m_numTerrainsY; y++)
                        {

                            //Fetch current terrain position (the pivot starts at the bottom left)
                            Vector2 terrainPosition = new Vector2(m_minTerrainPos.x + (x * m_terrainSize),
                                m_minTerrainPos.y + (y * m_terrainSize));
                            //Retrieve the current terrain based on this position
                            TerrainPosData currentTerrain = GetTerrainAtPositionFromGrid(terrainPosition);

                            stormVTTerrainDataContainer.m_terrainDataList[c] = currentTerrain.terrainDataPath;

                            //Create new grid data
                            StormVTTerrainGridData tempGridData = new StormVTTerrainGridData
                            {
                                position = terrainPosition,
                                gridIndex = new int2(x, y),
                                heightmapChanged = false,
                                texturesChanged = false,
                            };

                            string assetPath = AssetDatabase.GUIDToAssetPath(currentTerrain.terrainDataPath);
                            TerrainData terrainData = AssetDatabase.LoadAssetAtPath<TerrainData>(assetPath);

                            List<TerrainLayer> terrainLayers = terrainData.terrainLayers.ToList();
                            tempGridData.terrainLayers = terrainLayers;
                            tempGridData.terrainTileLayers = StormVTTerrainGrid.SetTerrainTileLayers(terrainLayers, m_terrainSize, terrainData.name);
                            tempGridData.heightScale = terrainData.heightmapScale.y;

                            m_terrainGridData[c] = tempGridData;
                            c++;

                            terrainData = null;
                            Resources.UnloadUnusedAssets();
                        }
                    }

                    EditorUtility.SetDirty(stormVTTerrainDataContainer);

                    //Load / Create terrain grid asset to remove terrain references in scene
                    if (stormVTTerrainGrid == null)
                    {
                        //grid asset not loaded, try to load it in
                        Scene activeScene = SceneManager.GetActiveScene();
                        string vtAssetFilePath = $"{activeScene.name}_TerrainGrid.asset";
                        string vtGridFilePath = $"{StormVTConstants.GetAssetsFolder()}/{vtAssetFilePath}";

                        //Load asset at the file path
                        stormVTTerrainGrid = (StormVTTerrainGrid)AssetDatabase.LoadAssetAtPath(vtGridFilePath, typeof(StormVTTerrainGrid));

                        //Check if the grid asset doesn't exist, if so create a new one
                        if (stormVTTerrainGrid == null)
                        {
                            stormVTTerrainGrid = ScriptableObject.CreateInstance<StormVTTerrainGrid>();
                            AssetDatabase.CreateAsset(stormVTTerrainGrid, vtGridFilePath);
                            stormVTTerrainGrid = (StormVTTerrainGrid)AssetDatabase.LoadAssetAtPath(vtGridFilePath, typeof(StormVTTerrainGrid));
                        }
                    }




                    //Set Grid Data
                    stormVTTerrainGrid.m_numTerrainsX = m_numTerrainsX;
                    stormVTTerrainGrid.m_numTerrainsY = m_numTerrainsY;
                    stormVTTerrainGrid.m_gridList = m_terrainGridData;

                    stormVTTerrainGrid.m_terrainHeightScale = m_terrainHeightScale;
                    stormVTTerrainGrid.m_terrainHeightOffset = m_terrainHeightOffset;
                    stormVTTerrainGrid.m_terrainBoundsCenter = m_terrainBoundsCenter;
                    stormVTTerrainGrid.m_terrainBoundsSize = m_terrainBoundsSize;
                    stormVTTerrainGrid.m_heightmapResolution = m_heightmapResolution;
                    stormVTTerrainGrid.m_terrainSize = m_terrainSize;

                    stormVTTerrainGrid.UpdateTerrainIndices();

                    EditorUtility.SetDirty(stormVTTerrainGrid);
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();

                }
                #endif
            }

            stormVTTerrainDataContainer = null;

        }

        

        /// <summary>
        /// Saves a heightmap texture2D to disk
        /// </summary>
        /// <param name="userDataPath">Path to save texture to</param>
        /// <param name="texture">The texture to save to disk</param>
        /// <returns></returns>
        public string SaveTextureToDiskNoImport(string userDataPath, Texture2D texture)
        {
            if (texture == null)
                return string.Empty;

            string assetFilePath = $"{texture.name}.png";
            string filePath = $"{userDataPath}/{assetFilePath}";

            StormVTConstants.WriteTexture2D(filePath, texture);

            return filePath;
        }

        /// <summary>
        /// Save normal map to disc without performing AssetDatabase.ImportAsset
        /// </summary>
        /// <param name="userDataPath">Path to save the texture to</param>
        /// <param name="texture2D">Texture to save</param>
        /// <param name="filterMode">Filter mode for the texture</param>
        /// <returns></returns>
        public string SaveNormalmapTexture2DToDiskNoImport(string userDataPath, Texture2D texture2D,
            FilterMode filterMode = FilterMode.Bilinear)
        {
            string path = string.Empty;

            if (texture2D == null)
                return path;

#if UNITY_EDITOR
            path = AssetDatabase.GetAssetPath(texture2D);
            if (string.IsNullOrEmpty(path))
            {
                string assetFilePath = $"{texture2D.name}.png";
                path = $"{userDataPath}{assetFilePath}";
            }

            StormVTConstants.WriteTexture2D(path, texture2D);
            AssetDatabase.ImportAsset(path);
#endif
            return path;
        }

        /// <summary>
        /// Save normal map to disc and import it
        /// </summary>
        /// <param name="userDataPath">File path to save texture to</param>
        /// <param name="texture2D">Texture to save</param>
        /// <param name="filterMode">Filter mode of the texture</param>
        /// <returns></returns>
        public string SaveNormalmapTexture2DToDiskWithImport(string userDataPath, Texture2D texture2D,
    FilterMode filterMode = FilterMode.Bilinear)
        {
            string path = string.Empty;

            if (texture2D == null)
                return path;

#if UNITY_EDITOR
            path = AssetDatabase.GetAssetPath(texture2D);
            if (string.IsNullOrEmpty(path))
            {
                string assetFilePath = $"{texture2D.name}.png";
                path = $"{userDataPath}/{assetFilePath}";
            }


            AssetDatabase.ImportAsset(path);

            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Default;
                importer.sRGBTexture = true;
                importer.alphaSource = TextureImporterAlphaSource.None;
                importer.alphaIsTransparency = false;
                importer.ignorePngGamma = false;
                // -> Advanced
                importer.npotScale = TextureImporterNPOTScale.None;
                importer.isReadable = false;
                importer.streamingMipmaps = false;
                importer.vtOnly = false;
                importer.mipmapEnabled = false;
                // <- Advanced
                importer.wrapMode = TextureWrapMode.Clamp;
                importer.filterMode = filterMode;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.maxTextureSize = texture2D.width;
                // <- Importer Platform Settings
                TextureImporterPlatformSettings settings = importer.GetDefaultPlatformTextureSettings();
                settings.maxTextureSize = texture2D.width;
                settings.resizeAlgorithm = TextureResizeAlgorithm.Bilinear;
                settings.format = TextureImporterFormat.RGBA32;
                settings.textureCompression = TextureImporterCompression.Uncompressed;
                importer.SetPlatformTextureSettings(settings);
                importer.SaveAndReimport();
            }
#endif
            return path;
        }

        /// <summary>
        /// Saves the world normal map to disc and performs AssetDatabase.ImportAsset
        /// </summary>
        /// <param name="userDataPath">File path to save to</param>
        /// <param name="texture2D">Texture to save</param>
        /// <param name="filterMode">Filter mode of the texture</param>
        /// <returns></returns>
        public string SaveWorldNormalmapTexture2DToDiskWithImport(string userDataPath, Texture2D texture2D,
    FilterMode filterMode = FilterMode.Bilinear)
        {
            string path = string.Empty;

            if (texture2D == null)
                return path;

#if UNITY_EDITOR
            path = AssetDatabase.GetAssetPath(texture2D);
            if (string.IsNullOrEmpty(path))
            {
                string assetFilePath = $"{texture2D.name}.png";
                path = $"{userDataPath}/{assetFilePath}";
            }

            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Default;
                importer.sRGBTexture = false;
                importer.alphaSource = TextureImporterAlphaSource.None;
                importer.alphaIsTransparency = false;
                importer.ignorePngGamma = false;
                // -> Advanced
                importer.npotScale = TextureImporterNPOTScale.None;
                importer.isReadable = false;
                importer.streamingMipmaps = false;
                importer.vtOnly = false;
                importer.mipmapEnabled = false;
                // <- Advanced
                importer.wrapMode = TextureWrapMode.Clamp;
                importer.filterMode = filterMode;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.maxTextureSize = texture2D.width;
                // <- Importer Platform Settings
                TextureImporterPlatformSettings settings = importer.GetDefaultPlatformTextureSettings();
                settings.maxTextureSize = texture2D.width;
                settings.resizeAlgorithm = TextureResizeAlgorithm.Bilinear;
                settings.format = TextureImporterFormat.RGBA32;
                settings.textureCompression = TextureImporterCompression.Uncompressed;
                importer.SetPlatformTextureSettings(settings);
                importer.SaveAndReimport();
            }
#endif
            return path;
        }



        #endregion

        /// <summary>
        /// Save out individual page tile textures to disc
        /// </summary>
        /// <param name="heightTileTextureList">The list of height tiles to export</param>
        /// <param name="normalTileTextureList">The list of normal maps to export</param>
        /// <param name="saveNormalMaps"></param>
        public void SaveAndImportTileTextures(ref List<Texture2D> heightTileTextureList,
            ref List<Texture2D> normalTileTextureList, bool saveNormalMaps)
        {
            int count = heightTileTextureList.Count;
            List<string> heightFilePaths = new List<string>();
            List<string> normalFilePaths = new List<string>();

            string tileFolderPath = StormVTConstants.GetTileFolder();
            for (int i = 0; i < count; i++)
            {
                string filePath = SaveTextureToDiskNoImport(tileFolderPath, heightTileTextureList[i]);
                heightFilePaths.Add(filePath);
                DestroyImmediate(heightTileTextureList[i]);
                heightTileTextureList[i] = null;

                if (saveNormalMaps)
                {
                    filePath = SaveTextureToDiskNoImport(tileFolderPath, normalTileTextureList[i]);
                    normalFilePaths.Add(filePath);
                    DestroyImmediate(normalTileTextureList[i]);
                    normalTileTextureList[i] = null;
                }
            }

            heightTileTextureList.Clear();
            normalTileTextureList.Clear();
            heightFilePaths.Clear();
            normalFilePaths.Clear();
        }

        /// <summary>
        /// Saves the world normal map list to disc
        /// </summary>
        /// <param name="textureList">The list of textures to save</param>
        /// <param name="currentNormalMapIndex">The index of the last normal map that was saved out</param>
        public void SaveAndImportWorldNormalMapList(ref List<Texture2D> textureList, int currentNormalMapIndex)
        {
#if UNITY_EDITOR
            string assetsFolderPath = StormVTConstants.GetTerrainTextureAssetsFolder();
            string[] filePaths = new string[textureList.Count];

            //Save Textures
            try
            {
                AssetDatabase.StartAssetEditing();
                for(int i = 0; i < textureList.Count; i++)
                {
                    string filePath = SaveNormalmapTexture2DToDiskNoImport(assetsFolderPath, textureList[i]);
                    filePaths[i] = filePath;
                    DestroyImmediate(textureList[i]);
                    textureList[i] = null;
                }
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }

            //Assign Textures
            try
            {
                AssetDatabase.StartAssetEditing();
                for(int i = 0; i < textureList.Count; i++)
                {
                    //stormVTTerrainGrid.m_normalTextureList[currentNormalMap] = AssetDatabase.LoadAssetAtPath<Texture2D>(filePaths[i]);
                    stormVTTerrainGrid.m_normalmapFilePaths[currentNormalMap] = filePaths[i];
                    currentNormalMap++;
                }
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }

            EditorUtility.SetDirty(stormVTTerrainGrid);

            textureList.Clear();
#endif
        }

#if UNITY_EDITOR

        /// <summary>
        /// Creates a curvature map for a single terrain
        /// </summary>
        /// <param name="x">The X index of the terrain in the grid</param>
        /// <param name="y">The Y index of the terrain in the grid</param>
        /// <param name="terrainGrid">The terrain grid</param>
        /// <param name="terrainSize">The resolution of the terrain</param>
        /// <returns></returns>
        public RenderTexture CreateCurvature(int x, int y, StormVTTerrainGridData[] terrainGrid, int terrainSize)
        {

            int maxX = Math.Max(m_numTerrainsX - 1, 1);
            int maxY = Math.Max(m_numTerrainsY - 1, 1);

            int maxIndex = stormVTTerrainGrid.m_gridList.Length - 1;

            if (m_numTerrainsX == 1)
                maxX = 0;
            if (m_numTerrainsY == 1)
                maxY = 0;

            StormVTTerrainGridData terrainGridDataCenter = terrainGrid[stormVTTerrainGrid.GetIndex(x, y)];
            StormVTTerrainGridData terrainGridDataMiddleLeft = terrainGrid[Math.Clamp(stormVTTerrainGrid.GetIndex(Math.Clamp(x - 1, 0, maxX), y), 0, maxIndex)];
            StormVTTerrainGridData terrainGridDataMiddleRight = terrainGrid[Math.Clamp(stormVTTerrainGrid.GetIndex(Math.Clamp(x + 1, 0, maxX), y), 0, maxIndex)]; // +1,0

            StormVTTerrainGridData terrainGridDataBottomLeft = terrainGrid[Math.Clamp(stormVTTerrainGrid.GetIndex(Math.Clamp(x - 1, 0, maxX), Math.Clamp(y - 1, 0, maxY)), 0, maxIndex)];
            StormVTTerrainGridData terrainGridDataBottomCenter = terrainGrid[Math.Clamp(stormVTTerrainGrid.GetIndex(x, Math.Clamp(y - 1, 0, maxY)), 0, maxIndex)];
            StormVTTerrainGridData terrainGridDataBottomRight = terrainGrid[Math.Clamp(stormVTTerrainGrid.GetIndex(Math.Clamp(x + 1, 0, maxX), Math.Clamp(y - 1, 0, maxY)), 0, maxIndex)];

            StormVTTerrainGridData terrainGridDataTopLeft = terrainGrid[Math.Clamp(stormVTTerrainGrid.GetIndex(Math.Clamp(x - 1, 0, maxY), Math.Clamp(y + 1, 0, maxY)), 0, maxIndex)];
            StormVTTerrainGridData terrainGridDataTopCenter = terrainGrid[Math.Clamp(stormVTTerrainGrid.GetIndex(x, Math.Clamp(y + 1, 0, maxY)), 0, maxIndex)];
            StormVTTerrainGridData terrainGridDataTopRight = terrainGrid[Math.Clamp(stormVTTerrainGrid.GetIndex(Math.Clamp(x + 1, 0, maxX), Math.Clamp(y + 1, 0, maxY)), 0, maxIndex)];

            TerrainPosData terrainCenter = GetTerrainAtPosition(terrainGridDataCenter.position);
            TerrainPosData terrainMiddleLeft = GetTerrainAtPosition(terrainGridDataMiddleLeft.position);
            TerrainPosData terrainMiddleRight = GetTerrainAtPosition(terrainGridDataMiddleRight.position);

            TerrainPosData terrainBottomLeft = GetTerrainAtPosition(terrainGridDataBottomLeft.position);
            TerrainPosData terrainBottomCenter = GetTerrainAtPosition(terrainGridDataBottomCenter.position);
            TerrainPosData terrainBottomRight = GetTerrainAtPosition(terrainGridDataBottomRight.position);

            TerrainPosData terrainTopLeft = GetTerrainAtPosition(terrainGridDataTopLeft.position);
            TerrainPosData terrainTopCenter = GetTerrainAtPosition(terrainGridDataTopCenter.position);
            TerrainPosData terrainTopRight = GetTerrainAtPosition(terrainGridDataTopRight.position);


            //Load in terrain data
            //string assetPath = AssetDatabase.GUIDToAssetPath(terrainCenter.terrainDataPath);
            TerrainData terrainCenterData = AssetDatabase.LoadAssetAtPath<TerrainData>(AssetDatabase.GUIDToAssetPath(terrainCenter.terrainDataPath));
            TerrainData terrainMiddleLeftData = AssetDatabase.LoadAssetAtPath<TerrainData>(AssetDatabase.GUIDToAssetPath(terrainMiddleLeft.terrainDataPath));
            TerrainData terrainMiddleRightData = AssetDatabase.LoadAssetAtPath<TerrainData>(AssetDatabase.GUIDToAssetPath(terrainMiddleRight.terrainDataPath));

            TerrainData terrainBottomLeftData = AssetDatabase.LoadAssetAtPath<TerrainData>(AssetDatabase.GUIDToAssetPath(terrainBottomLeft.terrainDataPath));
            TerrainData terrainBottomCenterData = AssetDatabase.LoadAssetAtPath<TerrainData>(AssetDatabase.GUIDToAssetPath(terrainBottomCenter.terrainDataPath));
            TerrainData terrainBottomRightData = AssetDatabase.LoadAssetAtPath<TerrainData>(AssetDatabase.GUIDToAssetPath(terrainBottomRight.terrainDataPath));

            TerrainData terrainTopLeftData = AssetDatabase.LoadAssetAtPath<TerrainData>(AssetDatabase.GUIDToAssetPath(terrainTopLeft.terrainDataPath));
            TerrainData terrainTopCenterData = AssetDatabase.LoadAssetAtPath<TerrainData>(AssetDatabase.GUIDToAssetPath(terrainTopCenter.terrainDataPath));
            TerrainData terrainTopRightData = AssetDatabase.LoadAssetAtPath<TerrainData>(AssetDatabase.GUIDToAssetPath(terrainTopRight.terrainDataPath));

            int singleTerrainSize = terrainSize - 1;
            int finalBigSize = (terrainSize - 1) * 3;

            //Copy center
            Graphics.CopyTexture(terrainCenterData.heightmapTexture, 0, 0, 0, 0, singleTerrainSize, singleTerrainSize, m_mergedTerrainHeightmap, 0, 0, singleTerrainSize, singleTerrainSize);
            if (terrainMiddleLeftData != null)
            {
                Graphics.CopyTexture(terrainMiddleLeftData.heightmapTexture, 0, 0, 0, 0, singleTerrainSize, singleTerrainSize, m_mergedTerrainHeightmap, 0, 0, 0, singleTerrainSize);
            }
            if (terrainMiddleRightData != null)
            {
                Graphics.CopyTexture(terrainMiddleRightData.heightmapTexture, 0, 0, 0, 0, singleTerrainSize, singleTerrainSize, m_mergedTerrainHeightmap, 0, 0, singleTerrainSize * 2, singleTerrainSize);
            }


            if (terrainBottomLeftData != null)
            {
                Graphics.CopyTexture(terrainBottomLeftData.heightmapTexture, 0, 0, 0, 0, singleTerrainSize, singleTerrainSize, m_mergedTerrainHeightmap, 0, 0, 0, 0);
            }
            if (terrainBottomCenterData != null)
            {
                Graphics.CopyTexture(terrainBottomCenterData.heightmapTexture, 0, 0, 0, 0, singleTerrainSize, singleTerrainSize, m_mergedTerrainHeightmap, 0, 0, singleTerrainSize, 0);
            }
            if (terrainBottomRightData != null)
            {
                Graphics.CopyTexture(terrainBottomRightData.heightmapTexture, 0, 0, 0, 0, singleTerrainSize, singleTerrainSize, m_mergedTerrainHeightmap, 0, 0, singleTerrainSize * 2, 0);
            }

            if (terrainTopLeftData != null)
            {
                Graphics.CopyTexture(terrainTopLeftData.heightmapTexture, 0, 0, 0, 0, singleTerrainSize, singleTerrainSize, m_mergedTerrainHeightmap, 0, 0, 0, singleTerrainSize * 2);
            }
            if (terrainTopCenterData != null)
            {
                Graphics.CopyTexture(terrainTopCenterData.heightmapTexture, 0, 0, 0, 0, singleTerrainSize, singleTerrainSize, m_mergedTerrainHeightmap, 0, 0, singleTerrainSize, singleTerrainSize * 2);
            }
            if (terrainTopRightData != null)
            {
                Graphics.CopyTexture(terrainTopRightData.heightmapTexture, 0, 0, 0, 0, singleTerrainSize, singleTerrainSize, m_mergedTerrainHeightmap, 0, 0, singleTerrainSize * 2, singleTerrainSize * 2);
            }

            StormVTUtilities.CreateWorldNormalMaterial.SetTexture(StormVTShaderIDs.m_stormVT_CreateWorldNormalMap_Heightmap, m_mergedTerrainHeightmap);
            //float heightScale = terrainCenter.terrainData.heightmapScale.y * 4f;

            float heightmapScale = terrainCenterData.heightmapScale.y;

            float heightScale = 1;
            if(terrainSize < 65)
            {
                heightScale = heightmapScale / 32f;
            }
            else if(terrainSize < 129)
            {
                heightScale = heightmapScale / 16f;
            }
            else if(terrainSize < 257)
            {
                heightScale = heightmapScale / 8f;
            }
            else if(terrainSize < 513)
            {
                heightScale = heightmapScale / 4f;
            }
            else if(terrainSize < 1025)
            {
                heightScale = heightmapScale / 2f;
            }
            else if (terrainSize < 2049)
            {
                heightScale = heightmapScale;
            }
            else if (terrainSize < 4097)
            {
                heightScale = heightmapScale * 2f;
            }
            else
                heightScale = heightmapScale * 2f;

            /*heightScale = 300;
            if(heightScale > 8192)
                heightScale = 2048;*/
            //StormVTUtilities.CreateWorldNormalMaterial.SetFloat(StormVTShaderIDs.m_stormVT_CreateWorldNormalMap_Height, terrainCenter.terrainData.heightmapScale.y * 2f);
            StormVTUtilities.CreateWorldNormalMaterial.SetFloat(StormVTShaderIDs.m_stormVT_CreateWorldNormalMap_Height, heightScale);
            StormVTUtilities.CreateWorldNormalMaterial.SetFloat(StormVTShaderIDs.m_stormVT_CreateWorldNormalMap_TexelSize, terrainSize);

            //Create merged curvature map
            Graphics.Blit(m_mergedTerrainHeightmap, m_mergedTerrainCurvature, StormVTUtilities.CreateWorldNormalMaterial, 0);

            Graphics.CopyTexture(m_mergedTerrainCurvature, 0, 0, singleTerrainSize, singleTerrainSize, terrainSize, terrainSize, m_singleTerrainCurvature, 0, 0, 0, 0);

            StormVTUtilities.CreateWorldNormalMaterial.SetTexture(StormVTShaderIDs.m_stormVT_CreateWorldNormalMap_Heightmap, terrainCenterData.heightmapTexture);
            Graphics.Blit(null, m_singleTerrainNormal, StormVTUtilities.CreateWorldNormalMaterial, 1);

            StormVTUtilities.AlphaMergeMaterial.SetTexture(StormVTShaderIDs.m_stormVT_AlphaMerge_Alpha, m_singleTerrainCurvature);
            Graphics.Blit(m_singleTerrainNormal, m_normalMapWithCurvature, StormVTUtilities.AlphaMergeMaterial);

            //Cleanup
            RenderTexture.active = null;

            terrainCenterData = null;
            terrainMiddleLeftData = null;
            terrainMiddleRightData = null;

            terrainBottomLeftData = null;
            terrainBottomCenterData = null;
            terrainBottomRightData = null;

            terrainTopLeftData = null;
            terrainTopCenterData = null;
            terrainTopRightData = null;

            Resources.UnloadUnusedAssets();

            return m_normalMapWithCurvature;
        }
#endif

        /// <summary>
        /// Allows an update tick in the editor for performing editor only code
        /// </summary>
        /// <param name="updateTerrainNormalReferences">Whether the terrain normal</param>
        public void StartEditorUpdates(bool updateTerrainNormalReferences = true)
        {
#if UNITY_EDITOR
            //remember what we are working on
            m_currentCoroutine = ConvertUnityTerrainsToZoomLayers(updateTerrainNormalReferences);
            //subscribe to the editor update event
            EditorApplication.update -= EditorUpdate;
            EditorApplication.update += EditorUpdate;
#endif
        }

        private void StopEditorUpdates()
        {
#if UNITY_EDITOR
            //forget what we are working on
            m_currentCoroutine = null;
            //unsubscribe to the editor update event
            EditorApplication.update -= EditorUpdate;
#endif
        }

        private void EditorUpdate()
        {
#if UNITY_EDITOR
            if (m_currentCoroutine != null)
            {
                double timeSinceLastUpdateinMS = (DateTime.Now - m_timeSinceLastUpdate).TotalMilliseconds;

                //force a update frequency between update calls - you can leave that out if not needed / wanted
                if (timeSinceLastUpdateinMS > m_UpdateFrequencyMS)
                {
                    if (timeSinceLastUpdateinMS > m_currentWaitTimeMS)
                    {
                        //reset wait time & timestamp
                        m_currentWaitTimeMS = 0;
                        m_timeSinceLastUpdate = DateTime.Now;

                        //this continues the execution of the coroutine function from the last yield where we stopped
                        m_currentCoroutine.MoveNext();
                    }
                }
            }
#endif
        }

  

        /// <summary>
        /// Saves out the terrain data
        /// </summary>
        public void SaveTerrainData()
        {
#if UNITY_EDITOR
            StartCoroutine(ConvertUnityTerrainsToZoomLayers());
#endif
        }

#if UNITY_EDITOR
        private int currentNormalMap = 0;
#endif

        /// <summary>
        /// Creates zoom tile textures and saves them out
        /// </summary>
        /// <param name="updateTerrainNormalReferences">Whether to reassign the normal maps when recreated</param>
        /// <returns></returns>
        public IEnumerator ConvertUnityTerrainsToZoomLayers(bool updateTerrainNormalReferences = true)
        {
#if UNITY_EDITOR
            FetchTerrainDataContainer();
            if(stormVTTerrainDataContainer == null)
            {
                Debug.Log("Terrain Data Container was null, performing re-ingest again");
                Initialize();
            }
            

            EditorUtility.DisplayProgressBar("Terrain Preprocessor", "Starting Work On Saving Terrain Data For Storm Terrain", 1);


            yield return null;

            //Force unload of terrains to clear memory
            if (StormVTGaia.Instance.IsGaiaTerrainLoadedScene())
            {
                StormVTGaia.Instance.UnloadAllGaiaTerrains(true);
            }


            EditorUtility.DisplayProgressBar("Terrain Preprocessor", "Starting Work On Saving Terrain Data For Storm Terrain", 1);

            Resources.UnloadUnusedAssets();
            GC.Collect();
            m_currentWaitTimeMS = 1000;
            yield return null;

            Debug.Log("Storm Terrain: Preprocessing Terrain Grid Data at: " + DateTime.Now.ToLongTimeString());



            EditorUtility.ClearProgressBar();

            int numTerrainsX = stormVTTerrainGrid.m_numTerrainsX;
            int numTerrainsY = stormVTTerrainGrid.m_numTerrainsY;
            StormVTTerrainGridData[] terrainGrid = stormVTTerrainGrid.m_gridList;


            #region Save out all Texture2Ds (using Texture Importer)

            //Calculate max amount of terrain normal maps to save out at once (to limit memory usage)
            double terrainNormalBits = 32;
            double terrainNormalResolution = 2049.0f;
            if (terrainGrid != null)
            {
                terrainNormalResolution = stormVTTerrainGrid.m_heightmapResolution;
            }

            //Calculate megabytes per terrain normal map
            double terrainNormalMapMemoryMegaBytes =
                (terrainNormalBits * terrainNormalResolution * terrainNormalResolution) / 8e+6;
            int terrainNormalMemoryLimit = (int)(m_maxMemoryAllocationSaving / terrainNormalMapMemoryMegaBytes);

            //Calculate max amount of terrain tiles (height and world normals) to save out at once (to limit memory usage)

            double tileBits = 16;

            tileBits += 32;
            

            //Calculate megabytes per terrain tile (16 bits for height, 32 bits for world normal)
            double tileMemoryMegabytes = (tileBits * stormVTSettings.m_tileSizedPadded * stormVTSettings.m_tileSizedPadded) / 8e+6;
            int tileMemoryLimit = (int)(m_maxMemoryAllocationSaving / tileMemoryMegabytes) / 9;


            
            string assetsFolderPath = StormVTConstants.GetTerrainTextureAssetsFolder();


            #endregion


            int currentCount = 0;
            int totalTexturesSaved = 0;

            //Save terrain height maps
            int totalTerrainMaps = numTerrainsX * numTerrainsY;
           

            

            #region Normal maps

            normalmapCount = 0;
            currentNormalMap = 0;

            int terrainSize = stormVTTerrainGrid.m_heightmapResolution;

            int singleTerrainSize = terrainSize - 1;
            int finalBigSize = (terrainSize - 1) * 3;

            m_mergedTerrainHeightmap = new RenderTexture(finalBigSize, finalBigSize, 0,
                RenderTextureFormat.R16, RenderTextureReadWrite.Linear);

            m_mergedTerrainCurvature = new RenderTexture(finalBigSize, finalBigSize, 0,
                RenderTextureFormat.R8, RenderTextureReadWrite.Linear);

            m_singleTerrainCurvature = new RenderTexture(terrainSize, terrainSize, 0, RenderTextureFormat.R8, RenderTextureReadWrite.Linear);
            m_normalMapWithCurvature = new RenderTexture(terrainSize, terrainSize, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
            m_singleTerrainNormal = new RenderTexture(terrainSize, terrainSize, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);

            List<Texture2D> m_currentWorldNormalMaps = new List<Texture2D>();
            currentCount = 0;
            totalTexturesSaved = 0;

            stormVTTerrainGrid.m_normalmapFilePaths = new string[numTerrainsX * numTerrainsY];

            //Save terrain normal maps
            for (int x = 0; x < numTerrainsX; x++)
            {
                for (int y = 0; y < numTerrainsY; y++)
                {
                    EditorUtility.DisplayProgressBar("Processing Normalmaps", $"Processing Normalmap {normalmapCount}...", (float)normalmapCount / totalTerrainMaps);
                    StormVTTerrainGridData terrainGridData = terrainGrid[stormVTTerrainGrid.GetIndex(x, y)];

                    TerrainPosData currentTerrain = GetTerrainAtPosition(terrainGridData.position);
                    if (currentTerrain.terrainPos != Vector3.positiveInfinity)
                    {

                        RenderTexture normalMapWithCurvature = CreateCurvature(x, y, terrainGrid, terrainSize);
                        Texture2D tempTerrainNormalMapTex2D = new Texture2D(terrainSize, terrainSize, TextureFormat.RGBA32, false, true);
                        RenderTexture.active = normalMapWithCurvature;
                        tempTerrainNormalMapTex2D.ReadPixels(new Rect(0, 0, terrainSize, terrainSize), 0, 0);
                        tempTerrainNormalMapTex2D.Apply();
                        RenderTexture.active = null;

                        tempTerrainNormalMapTex2D.name = $"Terrain_{terrainGridData.gridIndex.x}_{terrainGridData.gridIndex.y}_NormalMap";

                        
                        m_currentWorldNormalMaps.Add(tempTerrainNormalMapTex2D);

                        if(currentCount > m_terrainTextureSavingLimit)
                        {
                            EditorUtility.DisplayProgressBar("Processing Normalmaps", $"Saving Normalmaps {totalTexturesSaved}-{(totalTexturesSaved + m_terrainTextureSavingLimit)} out of {totalTerrainMaps}...", (float)normalmapCount / totalTerrainMaps);
                            SaveAndImportWorldNormalMapList(ref m_currentWorldNormalMaps, totalTexturesSaved);
                            totalTexturesSaved += m_terrainTextureSavingLimit;
                            currentCount = 0;
                            Resources.UnloadUnusedAssets();
                            GC.Collect();
                            m_currentWaitTimeMS = 1000;
                            yield return null;
                        }
                        else
                        {
                            currentCount++;
                        }
                        

                        normalmapCount++;
                        
                    }
                    else
                    {
                        Debug.Log("Terrain was null");
                    }

                    
                }
            }


            SaveAndImportWorldNormalMapList(ref m_currentWorldNormalMaps, totalTexturesSaved);
            m_currentWaitTimeMS = 1000;
            yield return null;
            

            Debug.Log("Storm Terrain: Finished Exporting Normalmaps at: " + DateTime.Now.ToLongTimeString());

            Resources.UnloadUnusedAssets();

            m_mergedTerrainHeightmap.Release();
            m_mergedTerrainCurvature.Release();
            m_singleTerrainCurvature.Release();
            m_singleTerrainNormal.Release();
            m_normalMapWithCurvature.Release();

            DestroyImmediate(m_mergedTerrainCurvature);
            DestroyImmediate(m_mergedTerrainHeightmap);
            DestroyImmediate(m_singleTerrainCurvature);
            DestroyImmediate(m_singleTerrainNormal);
            DestroyImmediate(m_normalMapWithCurvature);

            Resources.UnloadUnusedAssets();
            GC.Collect();

            m_currentWaitTimeMS = 1000;
            yield return null;

            ImportTerrainGridNormalMaps();

            Resources.UnloadUnusedAssets();
            GC.Collect();

            m_currentWaitTimeMS = 1000;
            yield return null;


            if(updateTerrainNormalReferences)
            {
                //Adding references to normalmaps on all terrains
                UpdateTerrainInfo();
            }


            Debug.Log("Storm Terrain: Finished Updating Terrain Info at: " + DateTime.Now.ToLongTimeString());

            #endregion



            #region Tile Saving

            // Find the lowest zoom level we need
            m_lowestZoomLayer = StormVTZoomLayers.GetRoundedZoomLayer(m_terrainTexelSize);

            // Find the highest zoom level we need
            // Determine the max boundary of the combined terrains
            Vector2 maxTerrainArea;
            maxTerrainArea.x = m_numTerrainsX * m_terrainSize;
            maxTerrainArea.y = m_numTerrainsY * m_terrainSize;

            // Find the highest boundary extent - either on the X or the Y
            float maxTerrainSize = maxTerrainArea.x > maxTerrainArea.y ? maxTerrainArea.x : maxTerrainArea.y;
            // Work out this highest boundry's texel size
            float maxTerrainTexelSize = maxTerrainSize / (int)stormVTSettings.m_tileSize;
            // Get the matching zoom layer from this texel size
            m_highestZoomLayer = StormVTZoomLayers.GetRoundedZoomLayer(maxTerrainTexelSize);

            // Set high and low levels
            m_lowestZoomLevel = m_lowestZoomLayer.m_zoomLevel;
            m_highestZoomLevel = m_highestZoomLayer.m_zoomLevel;

            //stormVTSettings.m_minHeightLevel = m_lowestZoomLevel;
            EditorUtility.SetDirty(stormVTSettings);

            //Update Settings
            EditorUtility.SetDirty(stormVTSettings);

            Vector2 maxTerrainPoint = minTerrainBounds + maxTerrainArea;

            // Amount of textures to store in memory before we bulk save them out

            if (stormVTSettings.m_tileLoadOption == StormVTEnums.LoadOption.File)
            {
                stormVTTerrainTile.stormVTSettings = stormVTSettings;
                stormVTTerrainTile.stormVTTerrainGrid = stormVTTerrainGrid;

                //Declare managable list of textures to save out, then reimport
                List<Texture2D> tileHeightTexturesList = new List<Texture2D>();
                List<Texture2D> tileNormalTexturesList = new List<Texture2D>();


                stormVTPhysicalTextureManager.SetupTileMaterials();


                int current = 0;
                int count = m_highestZoomLevel - m_lowestZoomLevel + 1;
                EditorUtility.DisplayProgressBar("Saving Tiles", $"Saving Zoom level {m_lowestZoomLevel}...", (float)current / count);


                // Loop through every tile and create a padded version
                for (int currentZoomLevel = m_lowestZoomLevel; currentZoomLevel <= m_highestZoomLevel; currentZoomLevel++)
                {

                    EditorUtility.DisplayProgressBar("Saving Tiles", $"Saving Zoom Level {currentZoomLevel}...", (float)current / count);
                    current++;

                    // Store a reference to the zoom scale
                    float currentZoomLayerScale = StormVTZoomLayers.m_vtZoomLayers[currentZoomLevel].m_scale;

                    // Compute the starting zoom tile's coordinate (e.g. (0,1) , (1,1))
                    int2 currentZoomTileStartingCoordinate = new int2(
                        (int)Mathf.Floor(minTerrainBounds.x / currentZoomLayerScale),
                        (int)Mathf.Floor(minTerrainBounds.y / currentZoomLayerScale));

                    // Figure out how many zoom tiles fit into the terrain area
                    int2 currentZoomMinTile = new int2(
                        (int)Mathf.Floor(minTerrainBounds.x / currentZoomLayerScale),
                        (int)Mathf.Floor(minTerrainBounds.y / currentZoomLayerScale));

                    int2 currentZoomMaxTile = new int2(
                        (int)Mathf.Floor(maxTerrainPoint.x / currentZoomLayerScale),
                        (int)Mathf.Floor(maxTerrainPoint.y / currentZoomLayerScale));

                    int2 currentZoomTileDistance = new int2(
                        Math.Abs(currentZoomMinTile.x - currentZoomMaxTile.x) + 1,
                        Math.Abs(currentZoomMinTile.y - currentZoomMaxTile.y) + 1);

                    int numZoomTilesX = currentZoomTileDistance.x;
                    int numZoomTilesY = currentZoomTileDistance.y;

                    // Loop through each zoom tile
                    for (int x = 0; x < numZoomTilesX; x++)
                    {
                        for (int y = 0; y < numZoomTilesY; y++)
                        {
                            // Retrieve current zoom tile coordinate
                            int2 currentZoomTileCoordinate = currentZoomTileStartingCoordinate + new int2(x, y);


                            int4 curZoomCoord = new int4(currentZoomTileCoordinate.x, currentZoomTileCoordinate.y, currentZoomLevel, 0);
                           
                            int bufferIndex = 0;

                            stormVTTerrainTile.GenerateTileRuntime(curZoomCoord, bufferIndex);

                            Texture2D zoomLevelTileTexture2D = stormVTTerrainTile.m_heightRenderBuffer[bufferIndex].GetActiveBuffer().ToTexture2D();

                            string tileName =
                                $"{currentZoomLevel}_{currentZoomTileCoordinate.x}_{currentZoomTileCoordinate.y}";



                            Texture2D zoomLevelTileNormalTexture2D = stormVTTerrainTile.m_normalRenderBuffer[bufferIndex].GetActiveBuffer().ToTexture2D();

                            // Save this texture here
                            zoomLevelTileNormalTexture2D.name = $"{tileName}_Normal";

                            tileNormalTexturesList.Add(zoomLevelTileNormalTexture2D);


                            

                            // Save this texture here
                            zoomLevelTileTexture2D.name = $"{tileName}_Height";

                            tileHeightTexturesList.Add(zoomLevelTileTexture2D);

                            if (tileHeightTexturesList.Count >= tileMemoryLimit)
                            {
                                SaveAndImportTileTextures(ref tileHeightTexturesList, ref tileNormalTexturesList,
                                    true);
                                m_currentWaitTimeMS = 1000;
                                yield return null;
                            }

                        }
                    }

                    SaveAndImportTileTextures(ref tileHeightTexturesList, ref tileNormalTexturesList,
                        true);
                    m_currentWaitTimeMS = 1000;
                    yield return null;
                }

                EditorUtility.ClearProgressBar();

                Debug.Log("Storm Terrain: Finished Exporting Tile Data at: " + DateTime.Now.ToLongTimeString());

            }

            #endregion

            StopEditorUpdates();

            stormVTTerrainDataContainer = null;

            EditorUtility.ClearProgressBar();
            yield return null;
#endif
            yield return null;
        }


        /// <summary>
        /// Imports all the terrain grid normal maps
        /// </summary>
        public void ImportTerrainGridNormalMaps()
        {
#if UNITY_EDITOR
            //Import Textures
            try
            {
                AssetDatabase.StartAssetEditing();
                for (int i = 0; i < stormVTTerrainGrid.m_normalmapFilePaths.Length; i++)
                {
                    ImportNormalmap(stormVTTerrainGrid.m_normalmapFilePaths[i]);
                }
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();


            }
#endif
        }

        /// <summary>
        /// Imports a normal map texture from a file path
        /// </summary>
        /// <param name="path">File path to import the texture from</param>
        public void ImportNormalmap(string path)
        {
#if UNITY_EDITOR
            //string path = AssetDatabase.GetAssetPath(texture2D);
            Texture2D texture2D = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            if (string.IsNullOrEmpty(path))
            {
                string assetFilePath = $"{texture2D.name}.png";
                path = $"{StormVTConstants.GetTerrainTextureAssetsFolder()}/{assetFilePath}";
            }

            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Default;
                importer.sRGBTexture = false;
                importer.alphaSource = TextureImporterAlphaSource.None;
                importer.alphaIsTransparency = false;
                importer.ignorePngGamma = false;
                // -> Advanced
                importer.npotScale = TextureImporterNPOTScale.None;
                importer.isReadable = false;
                importer.streamingMipmaps = false;
                importer.vtOnly = false;
                importer.mipmapEnabled = true;
                // <- Advanced
                importer.wrapMode = TextureWrapMode.Clamp;
                importer.filterMode = FilterMode.Bilinear;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.maxTextureSize = (texture2D.width * 2);
                // <- Importer Platform Settings
                TextureImporterPlatformSettings settings = importer.GetDefaultPlatformTextureSettings();
                settings.maxTextureSize = (texture2D.width * 2);
                settings.resizeAlgorithm = TextureResizeAlgorithm.Bilinear;
                settings.format = TextureImporterFormat.RGBA32;
                settings.textureCompression = TextureImporterCompression.Uncompressed;
                importer.SetPlatformTextureSettings(settings);
                importer.SaveAndReimport();
            }
#endif
        }





        /// <summary>
        /// Starts the terrain data bulk export editor update
        /// </summary>
        public void StartTerrainDataBulkExportEditorUpdates()
        {
#if UNITY_EDITOR
            //remember what we are working on
            m_currentCoroutine = ExportTerrainDataToFiles();
            //subscribe to the editor update event
            EditorApplication.update -= EditorUpdate;
            EditorApplication.update += EditorUpdate;
#endif
        }

        /// <summary>
        /// Stops the terrain data bulk export editor update
        /// </summary>
        private void StopTerrainDataBulkExportEditorUpdates()
        {
#if UNITY_EDITOR
            //forget what we are working on
            m_currentCoroutine = null;
            //unsubscribe to the editor update event
            EditorApplication.update -= EditorUpdate;
#endif
        }

        /// <summary>
        /// Starts the terrain data editor update
        /// </summary>
        public void StartTerrainDataListExportEditorUpdates()
        {
#if UNITY_EDITOR
            //remember what we are working on
            m_currentCoroutine = ExportTerrainListToFiles();
            //subscribe to the editor update event
            EditorApplication.update -= EditorUpdate;
            EditorApplication.update += EditorUpdate;
#endif
        }

        /// <summary>
        /// Stops the terrain editor update
        /// </summary>
        private void StopTerrainDataListExportEditorUpdates()
        {
#if UNITY_EDITOR
            //forget what we are working on
            m_currentCoroutine = null;
            //unsubscribe to the editor update event
            EditorApplication.update -= EditorUpdate;
#endif
        }

        /// <summary>
        /// Export the Terrain Data objects to separate textures
        /// </summary>
        /// <returns></returns>
        public IEnumerator ExportTerrainDataToFiles()
        {
            yield return null;

#if UNITY_EDITOR
            EditorUtility.DisplayProgressBar("Terrain Data Preprocessor", "Starting Work On Exporting Terrain Data", 1);


            yield return null;

            FetchTerrainDataContainer();

            int numTerrains = stormVTTerrainGrid.m_numTerrainsX * stormVTTerrainGrid.m_numTerrainsY;

            string tileFolderPath = StormVTConstants.GetTileFolder();

            //Normalmap render textures
            int terrainSize = stormVTTerrainGrid.m_heightmapResolution;
            int singleTerrainSize = terrainSize - 1;
            int finalBigSize = (terrainSize - 1) * 3;

            m_mergedTerrainHeightmap = new RenderTexture(finalBigSize, finalBigSize, 0,
                RenderTextureFormat.R16, RenderTextureReadWrite.Linear);

            m_mergedTerrainCurvature = new RenderTexture(finalBigSize, finalBigSize, 0,
                RenderTextureFormat.R8, RenderTextureReadWrite.Linear);

            m_singleTerrainCurvature = new RenderTexture(terrainSize, terrainSize, 0, RenderTextureFormat.R8, RenderTextureReadWrite.Linear);
            m_normalMapWithCurvature = new RenderTexture(terrainSize, terrainSize, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
            m_singleTerrainNormal = new RenderTexture(terrainSize, terrainSize, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);



            //Save terrain normal maps
            for (int x = 0; x < stormVTTerrainGrid.m_numTerrainsX; x++)
            {
                for (int y = 0; y < stormVTTerrainGrid.m_numTerrainsY; y++)
                {


                    int terrainIndex = stormVTTerrainGrid.GetIndex(x, y);

                    if (EditorUtility.DisplayCancelableProgressBar("Terrain Data Export", $"Exporting Data Textures on Terrain  {terrainIndex} of {numTerrains}...", (float)((float)terrainIndex / (float)numTerrains)))
                    {
                        break;
                    }
                    else
                    {
                        string terrainDataPath = stormVTTerrainDataContainer.m_terrainDataList[Math.Clamp(stormVTTerrainGrid.GetIndex(x, y), 0, stormVTTerrainDataContainer.m_terrainDataList.Length - 1)];
                        string assetPath = AssetDatabase.GUIDToAssetPath(terrainDataPath);
                        TerrainData terrainData = AssetDatabase.LoadAssetAtPath<TerrainData>(assetPath);

                        //Save out Splatmaps
                        for (int splatIndex = 0; splatIndex < terrainData.alphamapTextures.Length; splatIndex++)
                        {
                            if (terrainData.alphamapTextures[splatIndex] != null)
                            {
                                Texture2D currentSplat = terrainData.alphamapTextures[splatIndex];
                                int splatResolution = currentSplat.width;

                                Texture2D splatmapTex2D = new Texture2D(splatResolution, splatResolution, TextureFormat.RGBA32, true, true);
                                RenderTexture splatmapRenderTexture = new RenderTexture(splatResolution, splatResolution, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
                                Graphics.Blit(currentSplat, splatmapRenderTexture);

                                RenderTexture.active = splatmapRenderTexture;
                                splatmapTex2D.ReadPixels(new Rect(0, 0, splatResolution, splatResolution), 0, 0);
                                splatmapTex2D.Apply();
                                RenderTexture.active = null;


                                splatmapTex2D.name = $"Terrain_{x}_{y}_Splatmap_{splatIndex}";


                                string filePath = SaveTextureToDiskNoImport(tileFolderPath, splatmapTex2D);


                                //Low Res Version
                                Texture2D splatmapTex2DLowRes = new Texture2D(256, 256, TextureFormat.RGBA32, true, true);
                                RenderTexture splatmapRenderTextureLowRes = new RenderTexture(256, 256, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
                                Graphics.Blit(currentSplat, splatmapRenderTextureLowRes);

                                RenderTexture.active = splatmapRenderTextureLowRes;
                                splatmapTex2DLowRes.ReadPixels(new Rect(0, 0, 256, 256), 0, 0);
                                splatmapTex2DLowRes.Apply();
                                RenderTexture.active = null;

                                splatmapTex2DLowRes.name = $"Terrain_{x}_{y}_Splatmap_{splatIndex}_LowRes";
                                SaveTextureToDiskNoImport(tileFolderPath, splatmapTex2DLowRes);

                                DestroyImmediate(splatmapTex2D);
                                DestroyImmediate(splatmapTex2DLowRes);
                                splatmapRenderTexture.Release();
                                DestroyImmediate(splatmapRenderTexture);
                                DestroyImmediate(splatmapRenderTextureLowRes);

                            }
                            else
                            {
                                Debug.Log($"Terrain Data {terrainData.name}, {x}_{y} found no splats at splat index: {splatIndex}");
                            }

                        }


                        //Save out Heightmaps
                        {
                            int heightmapResolution = terrainData.heightmapResolution;
                            Texture2D heightmapTex2D = new Texture2D(heightmapResolution, heightmapResolution, TextureFormat.R16, false, true);

                            RenderTexture.active = terrainData.heightmapTexture;
                            heightmapTex2D.ReadPixels(new Rect(0, 0, heightmapResolution, heightmapResolution), 0, 0);
                            heightmapTex2D.Apply();
                            RenderTexture.active = null;

                            heightmapTex2D.name = $"Terrain_{x}_{y}_Heightmap";

                            string filePath = SaveTextureToDiskNoImport(tileFolderPath, heightmapTex2D);


                            //Low Res Version
                            RenderTexture heightmapRTLowRes = new RenderTexture(257, 257, 0, RenderTextureFormat.R16, RenderTextureReadWrite.Linear);
                            Graphics.Blit(terrainData.heightmapTexture, heightmapRTLowRes);
                            Texture2D heightmapTex2DLowRes = new Texture2D(257, 257, TextureFormat.R16, false, true);

                            RenderTexture.active = heightmapRTLowRes;
                            heightmapTex2DLowRes.ReadPixels(new Rect(0, 0, 257, 257), 0, 0);
                            heightmapTex2DLowRes.Apply();
                            RenderTexture.active = null;

                            heightmapTex2DLowRes.name = $"Terrain_{x}_{y}_Heightmap_LowRes";
                            SaveTextureToDiskNoImport(tileFolderPath, heightmapTex2DLowRes);

                            DestroyImmediate(heightmapRTLowRes);
                            DestroyImmediate(heightmapTex2D);
                            DestroyImmediate(heightmapTex2DLowRes);

                        }

                        //Save out bit data (holes)
                        {
                            int heightmapResolution = terrainData.heightmapResolution;
                            Texture2D holesmapTex2D = new Texture2D(heightmapResolution, heightmapResolution, TextureFormat.RGBA32, false, true);

                            RenderTexture holesRT = new RenderTexture(heightmapResolution, heightmapResolution, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
                            Graphics.Blit(terrainData.holesTexture, holesRT);

                            RenderTexture.active = holesRT;
                            holesmapTex2D.ReadPixels(new Rect(0, 0, heightmapResolution, heightmapResolution), 0, 0);
                            holesmapTex2D.Apply();
                            RenderTexture.active = null;

                            holesmapTex2D.name = $"Terrain_{x}_{y}_Holemap";

                            string filePath = SaveTextureToDiskNoImport(tileFolderPath, holesmapTex2D);


                            //Low Res Version
                            RenderTexture holesmapRTLowRes = new RenderTexture(257, 257, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
                            Graphics.Blit(terrainData.holesTexture, holesmapRTLowRes);
                            Texture2D holesmapTex2DLowRes = new Texture2D(257, 257, TextureFormat.RGBA32, false, true);

                            RenderTexture.active = holesmapRTLowRes;
                            holesmapTex2DLowRes.ReadPixels(new Rect(0, 0, 257, 257), 0, 0);
                            holesmapTex2DLowRes.Apply();
                            RenderTexture.active = null;

                            holesmapTex2DLowRes.name = $"Terrain_{x}_{y}_Holemap_LowRes";
                            SaveTextureToDiskNoImport(tileFolderPath, holesmapTex2DLowRes);

                            DestroyImmediate(holesmapRTLowRes);
                            DestroyImmediate(holesmapTex2D);
                            DestroyImmediate(holesmapTex2DLowRes);
                        }

                        //Save out normalmaps
                        {
                            RenderTexture normalMapWithCurvature = CreateCurvature(x, y, stormVTTerrainGrid.m_gridList, terrainSize);
                            Texture2D normalmapTex2D = new Texture2D(terrainSize, terrainSize, TextureFormat.RGBA32, false, true);
                            RenderTexture.active = normalMapWithCurvature;
                            normalmapTex2D.Apply();
                            normalmapTex2D.ReadPixels(new Rect(0, 0, terrainSize, terrainSize), 0, 0);
                            RenderTexture.active = null;

                            normalmapTex2D.name = $"Terrain_{x}_{y}_Normalmap";

                            SaveTextureToDiskNoImport(tileFolderPath, normalmapTex2D);


                            //Low Res Version
                            RenderTexture normalMapWithCurvatureLowRes = new RenderTexture(257,257, 0 ,RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
                            Graphics.Blit(normalMapWithCurvature, normalMapWithCurvatureLowRes);
                            Texture2D normalmapTex2DLowRes = new Texture2D(257, 257, TextureFormat.RGBA32, false, true);
                            RenderTexture.active = normalMapWithCurvatureLowRes;
                            normalmapTex2DLowRes.Apply();
                            normalmapTex2DLowRes.ReadPixels(new Rect(0, 0, 257, 257), 0, 0);
                            RenderTexture.active = null;

                            normalmapTex2DLowRes.name = $"Terrain_{x}_{y}_Normalmap_LowRes";
                            SaveTextureToDiskNoImport(tileFolderPath, normalmapTex2DLowRes);


                            DestroyImmediate(normalmapTex2D);
                            DestroyImmediate(normalmapTex2DLowRes);
                        }

                        terrainData = null;

                        Resources.UnloadUnusedAssets();
                        GC.Collect();
                        m_currentWaitTimeMS = 100;
                        yield return null;
                    }
                    

                    

                   
                }
            }


            EditorUtility.DisplayProgressBar("Terrain Data Preprocessor", "Cleaning Up Memory", 1);
            m_mergedTerrainHeightmap.Release();
            m_mergedTerrainCurvature.Release();
            m_singleTerrainCurvature.Release();
            m_singleTerrainNormal.Release();
            m_normalMapWithCurvature.Release();

            DestroyImmediate(m_mergedTerrainCurvature);
            DestroyImmediate(m_mergedTerrainHeightmap);
            DestroyImmediate(m_singleTerrainCurvature);
            DestroyImmediate(m_singleTerrainNormal);
            DestroyImmediate(m_normalMapWithCurvature);

            Resources.UnloadUnusedAssets();
            GC.Collect();
            m_currentWaitTimeMS = 5000;
            yield return null;

            stormVTTerrainDataContainer = null;
            StopTerrainDataBulkExportEditorUpdates();
            EditorUtility.ClearProgressBar();
            yield return null;
#endif
        }

        /// <summary>
        /// Returns a list of terrains that been edited
        /// </summary>
        /// <returns></returns>
        public int2[] GetEditedTerrains()
        {
            List<int2> editedTerrains = new List<int2>();
            for(int i = 0; i < stormVTTerrainGrid.m_gridList.Length; i++)
            {
                var gridTerrain = stormVTTerrainGrid.m_gridList[i];
                if (gridTerrain.heightmapChanged || gridTerrain.texturesChanged)
                {
                    editedTerrains.Add(gridTerrain.gridIndex);
                }
            }
            return editedTerrains.ToArray();
        }

        /// <summary>
        /// Exports terrain maps (Height and Normal textures) to disc
        /// </summary>
        /// <returns></returns>
        public IEnumerator ExportTerrainListToFiles()
        {
            int2[] terrains = GetEditedTerrains();

            if (terrains.Length > 0)
            {
                yield return null;

    #if UNITY_EDITOR
                EditorUtility.DisplayProgressBar("Terrain Data Preprocessor", "Starting Work On Exporting Terrain Data", 1);

                yield return null;

                FetchTerrainDataContainer();


                int numTerrains = terrains.Length;
                string tileFolderPath = StormVTConstants.GetTileFolder();

                //Normalmap render textures
                int terrainSize = stormVTTerrainGrid.m_heightmapResolution;
                int singleTerrainSize = terrainSize - 1;
                int finalBigSize = (terrainSize - 1) * 3;

                m_mergedTerrainHeightmap = new RenderTexture(finalBigSize, finalBigSize, 0,
                    RenderTextureFormat.R16, RenderTextureReadWrite.Linear);

                m_mergedTerrainCurvature = new RenderTexture(finalBigSize, finalBigSize, 0,
                    RenderTextureFormat.R8, RenderTextureReadWrite.Linear);

                m_singleTerrainCurvature = new RenderTexture(terrainSize, terrainSize, 0, RenderTextureFormat.R8, RenderTextureReadWrite.Linear);
                m_normalMapWithCurvature = new RenderTexture(terrainSize, terrainSize, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
                m_singleTerrainNormal = new RenderTexture(terrainSize, terrainSize, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);

                for(int i = 0; i < terrains.Length; i++)
                {
                    int x = terrains[i].x;
                    int y = terrains[i].y;

                    int terrainIndex = stormVTTerrainGrid.GetIndex(x, y);

                    if (EditorUtility.DisplayCancelableProgressBar("Terrain Data Export", $"Exporting Data Textures on Terrain {x}_{y}, {i} of {numTerrains}...", (float)((float)terrainIndex / (float)numTerrains)))
                    {
                        break;
                    }
                    else
                    {
                        
                        string terrainDataPath = stormVTTerrainDataContainer.m_terrainDataList[Math.Clamp(stormVTTerrainGrid.GetIndex(x, y), 0, stormVTTerrainDataContainer.m_terrainDataList.Length - 1)];
                        string assetPath = AssetDatabase.GUIDToAssetPath(terrainDataPath);
                        TerrainData terrainData = AssetDatabase.LoadAssetAtPath<TerrainData>(assetPath);

                        //Save out Splatmaps
                        for (int splatIndex = 0; splatIndex < terrainData.alphamapTextures.Length; splatIndex++)
                        {
                            if (terrainData.alphamapTextures[splatIndex] != null)
                            {
                                Texture2D currentSplat = terrainData.alphamapTextures[splatIndex];
                                int splatResolution = currentSplat.width;

                                Texture2D splatmapTex2D = new Texture2D(splatResolution, splatResolution, TextureFormat.RGBA32, true, true);
                                RenderTexture splatmapRenderTexture = new RenderTexture(splatResolution, splatResolution, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
                                Graphics.Blit(currentSplat, splatmapRenderTexture);

                                RenderTexture.active = splatmapRenderTexture;
                                splatmapTex2D.ReadPixels(new Rect(0, 0, splatResolution, splatResolution), 0, 0);
                                splatmapTex2D.Apply();
                                RenderTexture.active = null;


                                splatmapTex2D.name = $"Terrain_{x}_{y}_Splatmap_{splatIndex}";


                                string filePath = SaveTextureToDiskNoImport(tileFolderPath, splatmapTex2D);


                                //Low Res Version
                                Texture2D splatmapTex2DLowRes = new Texture2D(256, 256, TextureFormat.RGBA32, true, true);
                                RenderTexture splatmapRenderTextureLowRes = new RenderTexture(256, 256, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
                                Graphics.Blit(currentSplat, splatmapRenderTextureLowRes);

                                RenderTexture.active = splatmapRenderTextureLowRes;
                                splatmapTex2DLowRes.ReadPixels(new Rect(0, 0, 256, 256), 0, 0);
                                splatmapTex2DLowRes.Apply();
                                RenderTexture.active = null;

                                splatmapTex2DLowRes.name = $"Terrain_{x}_{y}_Splatmap_{splatIndex}_LowRes";
                                SaveTextureToDiskNoImport(tileFolderPath, splatmapTex2DLowRes);

                                DestroyImmediate(splatmapTex2D);
                                DestroyImmediate(splatmapTex2DLowRes);
                                splatmapRenderTexture.Release();
                                DestroyImmediate(splatmapRenderTexture);
                                DestroyImmediate(splatmapRenderTextureLowRes);
                            }
                            else
                            {
                                Debug.Log($"Terrain Data {terrainData.name}, {x}_{y} found no splats at splat index: {splatIndex}");
                            }

                        }


                        //Save out Heightmaps
                        {
                            int heightmapResolution = terrainData.heightmapResolution;
                            Texture2D heightmapTex2D = new Texture2D(heightmapResolution, heightmapResolution, TextureFormat.R16, false, true);

                            RenderTexture.active = terrainData.heightmapTexture;
                            heightmapTex2D.ReadPixels(new Rect(0, 0, heightmapResolution, heightmapResolution), 0, 0);
                            heightmapTex2D.Apply();
                            RenderTexture.active = null;

                            heightmapTex2D.name = $"Terrain_{x}_{y}_Heightmap";

                            string filePath = SaveTextureToDiskNoImport(tileFolderPath, heightmapTex2D);


                            //Low Res Version
                            RenderTexture heightmapRTLowRes = new RenderTexture(257, 257, 0, RenderTextureFormat.R16, RenderTextureReadWrite.Linear);
                            Graphics.Blit(terrainData.heightmapTexture, heightmapRTLowRes);
                            Texture2D heightmapTex2DLowRes = new Texture2D(257, 257, TextureFormat.R16, false, true);

                            RenderTexture.active = heightmapRTLowRes;
                            heightmapTex2DLowRes.ReadPixels(new Rect(0, 0, 257, 257), 0, 0);
                            heightmapTex2DLowRes.Apply();
                            RenderTexture.active = null;

                            heightmapTex2DLowRes.name = $"Terrain_{x}_{y}_Heightmap_LowRes";
                            SaveTextureToDiskNoImport(tileFolderPath, heightmapTex2DLowRes);

                            DestroyImmediate(heightmapRTLowRes);
                            DestroyImmediate(heightmapTex2D);
                            DestroyImmediate(heightmapTex2DLowRes);

                        }

                        //Save out bit data (holes)
                        {
                            int heightmapResolution = terrainData.heightmapResolution;
                            Texture2D holesmapTex2D = new Texture2D(heightmapResolution, heightmapResolution, TextureFormat.RGBA32, false, true);

                            RenderTexture holesRT = new RenderTexture(heightmapResolution, heightmapResolution, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
                            Graphics.Blit(terrainData.holesTexture, holesRT);

                            RenderTexture.active = holesRT;
                            holesmapTex2D.ReadPixels(new Rect(0, 0, heightmapResolution, heightmapResolution), 0, 0);
                            holesmapTex2D.Apply();
                            RenderTexture.active = null;

                            holesmapTex2D.name = $"Terrain_{x}_{y}_Holemap";

                            string filePath = SaveTextureToDiskNoImport(tileFolderPath, holesmapTex2D);


                            //Low Res Version
                            RenderTexture holesmapRTLowRes = new RenderTexture(257, 257, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
                            Graphics.Blit(terrainData.holesTexture, holesmapRTLowRes);
                            Texture2D holesmapTex2DLowRes = new Texture2D(257, 257, TextureFormat.RGBA32, false, true);

                            RenderTexture.active = holesmapRTLowRes;
                            holesmapTex2DLowRes.ReadPixels(new Rect(0, 0, 257, 257), 0, 0);
                            holesmapTex2DLowRes.Apply();
                            RenderTexture.active = null;

                            holesmapTex2DLowRes.name = $"Terrain_{x}_{y}_Holemap_LowRes";
                            SaveTextureToDiskNoImport(tileFolderPath, holesmapTex2DLowRes);

                            DestroyImmediate(holesmapRTLowRes);
                            DestroyImmediate(holesmapTex2D);
                            DestroyImmediate(holesmapTex2DLowRes);
                        }

                        //Save out normalmaps
                        {
                            RenderTexture normalMapWithCurvature = CreateCurvature(x, y, stormVTTerrainGrid.m_gridList, terrainSize);
                            Texture2D normalmapTex2D = new Texture2D(terrainSize, terrainSize, TextureFormat.RGBA32, false, true);
                            RenderTexture.active = normalMapWithCurvature;
                            normalmapTex2D.Apply();
                            normalmapTex2D.ReadPixels(new Rect(0, 0, terrainSize, terrainSize), 0, 0);
                            RenderTexture.active = null;

                            normalmapTex2D.name = $"Terrain_{x}_{y}_Normalmap";

                            SaveTextureToDiskNoImport(tileFolderPath, normalmapTex2D);


                            //Low Res Version
                            RenderTexture normalMapWithCurvatureLowRes = new RenderTexture(257, 257, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
                            Graphics.Blit(normalMapWithCurvature, normalMapWithCurvatureLowRes);
                            Texture2D normalmapTex2DLowRes = new Texture2D(257, 257, TextureFormat.RGBA32, false, true);
                            RenderTexture.active = normalMapWithCurvatureLowRes;
                            normalmapTex2DLowRes.Apply();
                            normalmapTex2DLowRes.ReadPixels(new Rect(0, 0, 257, 257), 0, 0);
                            RenderTexture.active = null;

                            normalmapTex2DLowRes.name = $"Terrain_{x}_{y}_Normalmap_LowRes";
                            SaveTextureToDiskNoImport(tileFolderPath, normalmapTex2DLowRes);


                            DestroyImmediate(normalmapTex2D);
                            DestroyImmediate(normalmapTex2DLowRes);
                        }

                        terrainData = null;

                        Resources.UnloadUnusedAssets();
                        GC.Collect();
                        m_currentWaitTimeMS = 100;
                        yield return null;
                    }

                }

            
            
                EditorUtility.DisplayProgressBar("Terrain Data Preprocessor", "Cleaning Up Memory", 1);
                m_mergedTerrainHeightmap.Release();
                m_mergedTerrainCurvature.Release();
                m_singleTerrainCurvature.Release();
                m_singleTerrainNormal.Release();
                m_normalMapWithCurvature.Release();

                DestroyImmediate(m_mergedTerrainCurvature);
                DestroyImmediate(m_mergedTerrainHeightmap);
                DestroyImmediate(m_singleTerrainCurvature);
                DestroyImmediate(m_singleTerrainNormal);
                DestroyImmediate(m_normalMapWithCurvature);

                //Unset all changes
                for (int i = 0; i < stormVTTerrainGrid.m_gridList.Length; i++)
                {
                    stormVTTerrainGrid.m_gridList[i].heightmapChanged = false;
                    stormVTTerrainGrid.m_gridList[i].texturesChanged = false;
                }
                EditorUtility.SetDirty(stormVTTerrainGrid);


                Resources.UnloadUnusedAssets();
                GC.Collect();
                m_currentWaitTimeMS = 5000;
                yield return null;

                stormVTTerrainDataContainer = null;
                StopTerrainDataListExportEditorUpdates();
                EditorUtility.ClearProgressBar();
                yield return null;



#endif
            }
        }

    }
}