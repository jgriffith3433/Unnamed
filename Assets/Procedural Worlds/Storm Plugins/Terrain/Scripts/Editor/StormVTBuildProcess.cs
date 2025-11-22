using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.Callbacks;
using UnityEngine;

#if GAIA_PRO_PRESENT || GAIA_2023_PRO
using Gaia;
#endif

namespace ProceduralWorlds.Storm.Terrain
{
    public class StormVTBuildProcess : IPreprocessBuildWithReport
    {

        public int callbackOrder { get { return 0; } }

        /// <summary>
        /// Called before a build starts
        /// Strip unused texture if setting is enabled
        /// </summary>
        /// <param name="report"></param>
        public void OnPreprocessBuild(BuildReport report)
        {

            Application.logMessageReceived += OnBuildError;

#if UNITY_EDITOR
            string VTUserDataPath = StormVTConstants.AssetsFolderPath;
            //Find user data path
            if (Directory.Exists(VTUserDataPath))
            {
                string[] directories = Directory.GetDirectories(VTUserDataPath);

                //Loop through all folders in vt user data path
                foreach (string directory in directories)
                {
                    //Get scene name
                    string sceneName = directory.Substring(directory.LastIndexOf('\\') + 1);

                    //Get settings assets
                    string vtSettingsAssetFilePath = $"{sceneName}_StormVTSettings.asset";
                    string vtSettingsFilePath = $"{directory}/{vtSettingsAssetFilePath}";
                    StormVTSettings settings = AssetDatabase.LoadAssetAtPath<StormVTSettings>(vtSettingsFilePath);

                    #region Update Terrains

                    if (settings != null)
                    {
                        settings.m_drawClipmap = true;
                        EditorUtility.SetDirty(settings);

                    }
                    #endregion


                    if (settings != null)
                    {
                        if (settings.m_stripUnusedTexturesOnBuild)
                        {
                            //Get terrain texture layers
                            StormVTTerrainTextureLayers stormVTTerrainTextureLayers = FetchTerrainLayers(sceneName, directory);

                            if (stormVTTerrainTextureLayers != null)
                            {
                                //Fetch texture layer holder (if it exists)
                                StormVTTextureLayerReferenceHolder stormVTTextureLayerReferenceHolder = FetchTerrainLayersHolder(sceneName, directory);

                                if (stormVTTextureLayerReferenceHolder != null)
                                {
                                    //Fill terrain texture holder
                                    stormVTTextureLayerReferenceHolder.FillTerrainLayerTextureReferences(stormVTTerrainTextureLayers.stormVTTerrainTextureLayers);

                                    //SetDirty(terrainlayersholder)
                                    EditorUtility.SetDirty(stormVTTextureLayerReferenceHolder);

                                    //Empty terrain layers
                                    stormVTTextureLayerReferenceHolder.EmptyTextureLayerReferences(stormVTTerrainTextureLayers.stormVTTerrainTextureLayers);

                                    //Empty unity terrain layers textures
                                    stormVTTextureLayerReferenceHolder.EmptyTerrainLayerTextureReferences();

                                    //SetDirty(terrainlayersholder)
                                    EditorUtility.SetDirty(stormVTTextureLayerReferenceHolder);

                                    //SetDirty(terrainlayers)
                                    EditorUtility.SetDirty(stormVTTerrainTextureLayers);

                                    //Save if dirty
                                    AssetDatabase.SaveAssetIfDirty(stormVTTerrainTextureLayers);
                                    AssetDatabase.SaveAssetIfDirty(stormVTTextureLayerReferenceHolder);

                                }

                            }
                        }
                    }

                           

                }



                //Save new holder container asset if created
                AssetDatabase.SaveAssets();
            }

#endif



        }

        /// <summary>
        /// Retreives the terrain texture layers scriptable object
        /// </summary>
        /// <param name="sceneName">Name of the scene to grab object from</param>
        /// <param name="directory">File path of the object</param>
        /// <returns></returns>
        public StormVTTerrainTextureLayers FetchTerrainLayers(string sceneName, string directory)
        {
            StormVTTerrainTextureLayers stormVTTerrainTextureLayers;

#if UNITY_EDITOR
            string vtAssetFilePath = $"{sceneName}_TerrainTextureLayers.asset";
            string vtTerrainLayersFilePath = $"{directory}/{vtAssetFilePath}";

            stormVTTerrainTextureLayers = (StormVTTerrainTextureLayers)AssetDatabase.LoadAssetAtPath(vtTerrainLayersFilePath, typeof(StormVTTerrainTextureLayers));
#endif
            return stormVTTerrainTextureLayers;
        }

        /// <summary>
        /// Retreives the terrain layer holder scriptable object
        /// </summary>
        /// <param name="sceneName">Name of the scene to grab object from</param>
        /// <param name="directory">File path of the object</param>
        /// <returns></returns>
        public StormVTTextureLayerReferenceHolder FetchTerrainLayersHolder(string sceneName, string directory)
        {

            StormVTTextureLayerReferenceHolder stormVTTextureLayerReferenceHolder;

#if UNITY_EDITOR
            string vtAssetFilePath = $"{sceneName}_TerrainTextureLayerReferenceHolder.asset";
            string vtTerrainLayersFilePath = $"{directory}/{vtAssetFilePath}";


            stormVTTextureLayerReferenceHolder = (StormVTTextureLayerReferenceHolder)AssetDatabase.LoadAssetAtPath(vtTerrainLayersFilePath, typeof(StormVTTextureLayerReferenceHolder));

            if (stormVTTextureLayerReferenceHolder == null)
            {
                stormVTTextureLayerReferenceHolder = ScriptableObject.CreateInstance<StormVTTextureLayerReferenceHolder>();
                AssetDatabase.CreateAsset(stormVTTextureLayerReferenceHolder, vtTerrainLayersFilePath);
                stormVTTextureLayerReferenceHolder = (StormVTTextureLayerReferenceHolder)AssetDatabase.LoadAssetAtPath(vtTerrainLayersFilePath, typeof(StormVTTextureLayerReferenceHolder));
                EditorUtility.SetDirty(stormVTTextureLayerReferenceHolder);
                AssetDatabase.SaveAssetIfDirty(stormVTTextureLayerReferenceHolder);
            }
#endif

            return stormVTTextureLayerReferenceHolder;

        }

        /// <summary>
        /// Retrieves the terrain texture layers scriptable object
        /// </summary>
        /// <param name="sceneName">Name of the scene to grab object from</param>
        /// <param name="directory">File path of the object</param>
        /// <returns></returns>
        public static StormVTTerrainTextureLayers FetchTerrainLayersStatic(string sceneName, string directory)
        {
            StormVTTerrainTextureLayers stormVTTerrainTextureLayers;

#if UNITY_EDITOR
            string vtAssetFilePath = $"{sceneName}_TerrainTextureLayers.asset";
            string vtTerrainLayersFilePath = $"{directory}/{vtAssetFilePath}";

            stormVTTerrainTextureLayers = (StormVTTerrainTextureLayers)AssetDatabase.LoadAssetAtPath(vtTerrainLayersFilePath, typeof(StormVTTerrainTextureLayers));
#endif
            return stormVTTerrainTextureLayers;
        }

        /// <summary>
        /// Retreives the terrain layer reference holder scriptable object
        /// </summary>
        /// <param name="sceneName">Name of the scene to grab object from</param>
        /// <param name="directory">File path of the object</param>
        /// <returns></returns>
        public static StormVTTextureLayerReferenceHolder FetchTerrainLayersHolderStatic(string sceneName, string directory)
        {

            StormVTTextureLayerReferenceHolder stormVTTextureLayerReferenceHolder;

#if UNITY_EDITOR
            string vtAssetFilePath = $"{sceneName}_TerrainTextureLayerReferenceHolder.asset";
            string vtTerrainLayersFilePath = $"{directory}/{vtAssetFilePath}";


            stormVTTextureLayerReferenceHolder = (StormVTTextureLayerReferenceHolder)AssetDatabase.LoadAssetAtPath(vtTerrainLayersFilePath, typeof(StormVTTextureLayerReferenceHolder));
#endif

            return stormVTTextureLayerReferenceHolder;

        }




        /// <summary>
        /// Called after a successful build
        /// Re-enalbe editor terrain material
        /// </summary>
        /// <param name="target"></param>
        /// <param name="pathToBuiltProject"></param>
        [PostProcessBuild(1)]
        public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject)
        {

#if UNITY_EDITOR
            string VTUserDataPath = StormVTConstants.AssetsFolderPath;

            //Find user data path
            if (Directory.Exists(VTUserDataPath))
            {
                string[] directories = Directory.GetDirectories(VTUserDataPath);

                //Loop through all folders in vt user data path
                foreach (string directory in directories)
                {
                    //Get scene name
                    string sceneName = directory.Substring(directory.LastIndexOf('\\') + 1);

                    //Get settings assets
                    string vtSettingsAssetFilePath = $"{sceneName}_StormVTSettings.asset";
                    string vtSettingsFilePath = $"{directory}/{vtSettingsAssetFilePath}";
                    StormVTSettings settings = AssetDatabase.LoadAssetAtPath<StormVTSettings>(vtSettingsFilePath);

                    #region Update Terrains

                    if (settings != null)
                    {
                        settings.m_drawClipmap = false;
                        EditorUtility.SetDirty(settings);
                    }

                    #endregion

                    if (settings != null)
                    {
                        if (settings.m_stripUnusedTexturesOnBuild)
                        {
                            //Get terrain texture layers
                            StormVTTerrainTextureLayers stormVTTerrainTextureLayers = FetchTerrainLayersStatic(sceneName, directory);

                            if (stormVTTerrainTextureLayers != null)
                            {
                                //Fetch texture layer holder (if it exists)
                                StormVTTextureLayerReferenceHolder stormVTTextureLayerReferenceHolder = FetchTerrainLayersHolderStatic(sceneName, directory);

                                if (stormVTTextureLayerReferenceHolder != null)
                                {
                                    //Populate unity terrain layers textures
                                    stormVTTextureLayerReferenceHolder.PopulateTerrainLayerTextureReferences();

                                    EditorUtility.SetDirty(stormVTTextureLayerReferenceHolder);

                                    //Populate terrain layers
                                    stormVTTextureLayerReferenceHolder.PopulateTerrainTextureLayerReferences(stormVTTerrainTextureLayers.stormVTTerrainTextureLayers);

                                    //SetDirty(terrainlayers)
                                    EditorUtility.SetDirty(stormVTTerrainTextureLayers);

                                    AssetDatabase.SaveAssetIfDirty(stormVTTerrainTextureLayers);
                                    AssetDatabase.SaveAssetIfDirty(stormVTTextureLayerReferenceHolder);

                                }
                            }
                        }
                    }
 
                }
                AssetDatabase.SaveAssets();
            }


            //making sure the StormVTGaia instance exists
            if (StormVTGaia.Instance == null)
            {
                System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(typeof(StormVTSOInstanceLoader).TypeHandle);
            }
            if (StormVTGaia.Instance != null)
            {
                StormVTGaia.Instance.ForceLoadGaiaTerrains();
            }
#endif




        }

        /// <summary>
        /// Perform cleanup operations if the build failed.
        /// Re-enalbe terrain drawing
        /// </summary>
        public void CleanupOnBuildFailed()
        {
#if UNITY_EDITOR
            string VTUserDataPath = StormVTConstants.AssetsFolderPath;

            //Find user data path
            if (Directory.Exists(VTUserDataPath))
            {
                string[] directories = Directory.GetDirectories(VTUserDataPath);

                //Loop through all folders in vt user data path
                foreach (string directory in directories)
                {
                    //Get scene name
                    string sceneName = directory.Substring(directory.LastIndexOf('\\') + 1);

                    //Get settings assets
                    string vtSettingsAssetFilePath = $"{sceneName}_StormVTSettings.asset";
                    string vtSettingsFilePath = $"{directory}/{vtSettingsAssetFilePath}";
                    StormVTSettings settings = AssetDatabase.LoadAssetAtPath<StormVTSettings>(vtSettingsFilePath);

                    #region Update Terrains

                    if (settings != null)
                    {
                        settings.m_drawClipmap = false;
                        EditorUtility.SetDirty(settings);
                        
                    }

                    #endregion

                    if (settings != null)
                    {
                        if (settings.m_stripUnusedTexturesOnBuild)
                        {
                            //Get terrain texture layers
                            StormVTTerrainTextureLayers stormVTTerrainTextureLayers = FetchTerrainLayersStatic(sceneName, directory);

                            if (stormVTTerrainTextureLayers != null)
                            {
                                //Fetch texture layer holder (if it exists)
                                StormVTTextureLayerReferenceHolder stormVTTextureLayerReferenceHolder = FetchTerrainLayersHolderStatic(sceneName, directory);

                                if (stormVTTextureLayerReferenceHolder != null)
                                {

                                    //Populate unity terrain layers textures
                                    stormVTTextureLayerReferenceHolder.PopulateTerrainLayerTextureReferences();

                                    EditorUtility.SetDirty(stormVTTextureLayerReferenceHolder);

                                    //Populate terrain layers
                                    stormVTTextureLayerReferenceHolder.PopulateTerrainTextureLayerReferences(stormVTTerrainTextureLayers.stormVTTerrainTextureLayers);

                                    //SetDirty(terrainlayers)
                                    EditorUtility.SetDirty(stormVTTerrainTextureLayers);

                                    AssetDatabase.SaveAssetIfDirty(stormVTTerrainTextureLayers);
                                    AssetDatabase.SaveAssetIfDirty(stormVTTextureLayerReferenceHolder);
                                }

                            }
                        }
                    }

                            


                }
            }

            //making sure the StormVTGaia instance exists
            if (StormVTGaia.Instance == null)
            {
                System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(typeof(StormVTSOInstanceLoader).TypeHandle);
            }
            if (StormVTGaia.Instance != null)
            {
                StormVTGaia.Instance.ForceLoadGaiaTerrains();
            }
#endif




        }

        /// <summary>
        /// CALLED DURING BUILD TO CHECK FOR ERRORS
        /// </summary>
        /// <param name="condition">Condition of the build error</param>
        /// <param name="stacktrace">Stack trace of build process</param>
        /// <param name="type">Type of error</param>
        private void OnBuildError(string condition, string stacktrace, LogType type)
        {
            if (type == LogType.Error)
            {
                // FAILED TO BUILD, STOP LISTENING FOR ERRORS
                Application.logMessageReceived -= OnBuildError;
                if (this != null)
                {
                    CleanupOnBuildFailed();
                }
            }
        }


    }
}


