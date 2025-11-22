using System.Collections.Generic;
using System;
using System.IO;
using UnityEngine;
using System.Reflection;
using System.Linq;
using System.Threading;
using UnityEngine.Scripting;
using UnityEngine.Rendering;

#if PW_STORM_PRESENT
using ProceduralWorlds.Storm.Core;
using ProceduralWorlds.Storm.Utils;
#endif
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.SceneManagement;
using System.Diagnostics;
using Debug = UnityEngine.Debug;
using TreeEditor;
#endif


namespace ProceduralWorlds.Storm.Wizard
{
    public enum StormWizardRP
    {
        Builtin,
        URP,
        HDRP
    }

    public enum SearchMode { Equals, StartsWith, Contains, EndsWith }

    /// <summary>
    /// Delete feature data class that holds all the information for deleting features from tiles
    /// </summary>
    [Serializable]
    public class StormDeleteFeatureData
    {
        public string TileName;
        public List<string> FeatureLayerName = new List<string>();
#if PW_STORM_PRESENT
        public StormRegion[] Regions = Array.Empty<StormRegion>();
#endif
    }

    /// <summary>
    /// Handy wizard utilities that helps the wizard editor
    /// </summary>
    [InitializeOnLoad]
    public class StormWizardUtils : MonoBehaviour
    {
        #region Constant Variables

        public const string ShowWizardOnStartup = "STORMSHOWWIZARDONSTARTUP";
        public const string CanShowOnStartup = "STORMCANSHOWONSTARTUP";
        public const string StormPackageName = "Storm Installation Package";
        public const string StormPackagePath = "Assets/Procedural Worlds/Storm/" + StormPackageName + ".unitypackage";
        public const string TerrainPackageBuiltInName = "StormVT_BuiltIn";
        public const string TerrainPackageBuiltInPath = "Assets/Procedural Worlds/Storm Plugins/Terrain/Content/Terrain Materials/" + TerrainPackageBuiltInName + ".unitypackage";
        public const string TerrainPackageURPName = "StormVT_URP";
        public const string TerrainPackageURPPath = "Assets/Procedural Worlds/Storm Plugins/Terrain/Content/Terrain Materials/" + TerrainPackageURPName + ".unitypackage";
        public const string TerrainPackageHDRPName = "StormVT_HDRP";
        public const string TerrainPackageHDRPPath = "Assets/Procedural Worlds/Storm Plugins/Terrain/Content/Terrain Materials/" + TerrainPackageHDRPName + ".unitypackage";
        public const string GaiaShaderUpdatePackageName = "Gaia Storm Shader Update (HDRP-URP)";
        public const string GaiaShaderUpdatePackagePath = "Assets/Procedural Worlds/Storm/Packages/" + GaiaShaderUpdatePackageName + ".unitypackage";
        public static bool CheckWizardFinished;

        #endregion

        #region Static Constructor
        static StormWizardUtils()
        {
            AssetDatabase.importPackageCompleted -= OnImportPackageCompleted;
            AssetDatabase.importPackageCompleted += OnImportPackageCompleted;
        }

        private static void OnImportPackageCompleted(string packageName)
        {
            //Debug.Log("Import completed:" + packageName);
            switch (packageName)
            {
                case StormPackageName:
                    StormInstallCompleted();
                    break;
                case GaiaShaderUpdatePackageName:
                    GaiaShaderInstallCompleted();
                    break;
                case TerrainPackageBuiltInName:
                    TerrainShaderInstallCompleted();
                    break;
                case TerrainPackageURPName:
                    TerrainShaderInstallCompleted();
                    break;
                case TerrainPackageHDRPName:
                    TerrainShaderInstallCompleted();
                    break;


            }
        }
        #endregion

        #region Wizard Util Functions

        /// <summary>
        /// Gets the current installed SRP
        /// </summary>
        /// <returns></returns>
        public static StormWizardRP GetActivePipeline()
        {
            RenderPipelineAsset rpAsset = null;

            //look at the RP asset in quality settings first, that would be the one that overrides the default
            if (QualitySettings.renderPipeline != null)
            {
                rpAsset =  QualitySettings.renderPipeline;
            }
            //Fallback: take the default pipeline asset if none provided in the default quality settings
#if UNITY_6000_0_OR_NEWER
            rpAsset = GraphicsSettings.defaultRenderPipeline;
#else
            rpAsset = GraphicsSettings.renderPipelineAsset;
#endif
            if (rpAsset == null)
            {
                return StormWizardRP.Builtin;
            }
            else if (rpAsset.GetType().ToString().Contains("HDRenderPipelineAsset"))
            {
                return StormWizardRP.HDRP;
            }
            else
            {
                return StormWizardRP.URP;
            }
        }
        /// <summary>
        /// Return GaiaSettings or null;
        /// </summary>
        /// <returns>Gaia settings or null if not found</returns>
        public static StormWizardSettings GetStormWizardSettings()
        {
#if UNITY_EDITOR
            StormWizardSettings ret = AssetDatabase.LoadAssetAtPath<StormWizardSettings>("Assets/Procedural Worlds/Storm/Scripts/Wizard/Wizard Settings/Storm Wizard Settings.asset");
            if (ret == null)
                ret = AssetDatabase.LoadAssetAtPath<StormWizardSettings>(GetAssetPath("Storm Wizard Settings.asset"));
            return ret;
#else
            return null;
#endif
        }
        /// <summary>
        /// Imports a unity package. Allows to pass in an "onComplete" Action that will be triggered when the package import is done.
        /// </summary>
        /// <param name="packageName"></param>
        /// <param name="onComplete"></param>
        /// <param name="interactable"></param>
        public static void ImportPackage(string packageName, bool interactable = false)
        {
#if UNITY_EDITOR
            //// Temporary handler to capture the import completion
            //void Handler(string packageName)
            //{
            //    // Unregister the handler again to avoid side effects on other imports
            //    AssetDatabase.importPackageCompleted -= Handler;

            //    // Call the provided callback, if any
            //    onComplete?.Invoke();
            //}

            //// Register the handler
            //AssetDatabase.importPackageCompleted += Handler;

            // Import the package 
            AssetDatabase.ImportPackage(packageName, interactable);
#endif
        }
        /// <summary>
        /// Installs the storm core package
        /// </summary>
        public static bool InstallStormCore(bool showSafetyPopup = true)
        {
#if UNITY_EDITOR         
            if (!File.Exists(StormPackagePath))
            {
                Debug.LogError("Unable to find the storm package. Make sure the unity package " + StormPackagePath + " is present.");
                return false;
            }

            if (showSafetyPopup)
            {
                if (!EditorUtility.DisplayDialog("Installing/Updating Storm Core!", "You are about to install or update Storm Core. Please make sure to backup your project or to use a source control system before installing.\r\n\r\n Are you sure you want to proceed?", "Yes", "No"))
                {
                    return false;
                }
            }

            AssetDatabase.SaveAssets();

            if (EditorUtility.DisplayDialog("Open User Quick Start?", "The installation process can take some time. Would you like to view the User Quick Start document while the installation is running?", "Yes", "No"))
            {
                StormWizardUtils.OpenUserQuickStart();
            }

#if PW_STORM_PRESENT
            //turn off storm rendering for the installation to avoid glitches in the scene view etc.
            if (Storm.Core.StormWorld.Instance != null)
            {
                Storm.Core.StormWorld.Instance.enabled = false;
            }
#endif

            //We need to delete the Storm scripting define script first, then we need to remove the scripting define itself to make sure it is not present during the installation process
            //otherwise every software that has dependencies into storm will cause a compilation error during the installation when storm is not there for a brief moment.

            //Delete scripting define script
            string path = StormWizardUtils.GetAssetPath("StormScriptDefine.cs");
            if (!String.IsNullOrEmpty(path))
            {
                FileUtil.DeleteFileOrDirectory(StormWizardUtils.GetAssetPath("StormScriptDefine.cs"));
            }

            //remove the Storm script define
            bool updateScripting = false;
            string symbols = PlayerSettings.GetScriptingDefineSymbols(NamedBuildTarget.FromBuildTargetGroup(EditorUserBuildSettings.selectedBuildTargetGroup));

            if (symbols.Contains("PW_STORM_PRESENT"))
            {
                updateScripting = true;
                if (symbols.EndsWith("PW_STORM_PRESENT"))
                {
                    //The scripting define is the last in the list
                    symbols = symbols.Replace("PW_STORM_PRESENT", "");
                }
                else
                {
                    //The scripting define is somewhere inbetween
                    symbols = symbols.Replace("PW_STORM_PRESENT;", "");
                }
            }

            if (updateScripting)
            {
                PlayerSettings.SetScriptingDefineSymbols(NamedBuildTarget.FromBuildTargetGroup(EditorUserBuildSettings.selectedBuildTargetGroup), symbols);
            }


            //Need to delete all target folders before the package install
            StormWizardSettings settings = GetStormWizardSettings();
            if (settings == null)
            {
                Debug.LogError("Could not find Storm Wizard Settings, could not start the installation!");
                return false;
            }

            foreach (string folder in settings.m_directoriesToDeleteOnInstall)
            {
                if (File.Exists(folder + ".meta"))
                {
                    FileUtil.DeleteFileOrDirectory(folder + ".meta");
                }
                FileUtil.DeleteFileOrDirectory(folder);
            }

            AssetDatabase.Refresh();

            ImportPackage(StormPackagePath, false);

            return true;
#else
            return false;
#endif
        }

        private static void StormInstallCompleted()
        {
            Debug.Log("StormInstallCompleted");

            //Prepare GDAL dylibs on macOs for first access
            if (SystemInfo.operatingSystemFamily == OperatingSystemFamily.MacOSX)
            {
                string assetsPath = Application.dataPath;
                string geoRuntimePAth = Path.Combine(assetsPath, "Procedural Worlds/Storm Plugins/Geospatial/Plugins/runtimes");

                if (!Directory.Exists(geoRuntimePAth))
                {
                    UnityEngine.Debug.LogError("Geospatial runtime folder not found at: " + geoRuntimePAth);
                    return;
                }

                string[] dylibFiles = Directory.GetFiles(geoRuntimePAth, "*.dylib", SearchOption.AllDirectories);

                if (dylibFiles.Length == 0)
                {
                    UnityEngine.Debug.LogWarning("No .dylib files found in: " + geoRuntimePAth);
                    return;
                }

                foreach (string dylibPath in dylibFiles)
                {
                    StormWizardUtils.RunXattrCommand(dylibPath);
                }
            }

#if GAIA_2023
            if (!File.Exists(GaiaShaderUpdatePackagePath))
            {
                Debug.LogError("Unable to find the Gaia Shader Update package. The Storm installation is still working, but you will need to update the Gaia Shaders for Storm usage. Please make sure the unity package " + GaiaShaderUpdatePackagePath + " is present.");
            }
            else
            {
                ImportPackage(GaiaShaderUpdatePackagePath, false);
            }
#else
            GaiaShaderInstallCompleted();
#endif
        }

        private static void GaiaShaderInstallCompleted()
        {
            //Install the correct shader support pack for the terrain system according to the current render pipeline
            StormWizardRP rp = StormWizardUtils.GetActivePipeline();
            switch (rp)
            {
                case StormWizardRP.Builtin:
                    if (!File.Exists(TerrainPackageBuiltInPath))
                    {
                        Debug.LogError("Unable to find the Terrain System Shader Update package for the Built-In render pipeline. The Storm installation is still working, but you may need to update the Terrain System Shaders for your current render pipeline. Please check if you can find " + TerrainPackageBuiltInPath + " in your project and install it manually.");
                    }
                    else
                    {
                        Debug.Log("Importing BuiltIn Terrain Package");
                        ImportPackage(TerrainPackageBuiltInPath, false);
                    }
                    break;
                case StormWizardRP.URP:
                    if (!File.Exists(TerrainPackageURPPath))
                    {
                        Debug.LogError("Unable to find the Terrain System Shader Update package for the Universal render pipeline. The Storm installation is still working, but you may need to update the Terrain System Shaders for your current render pipeline. Please check if you can find " + TerrainPackageURPPath + " in your project and install it manually.");
                    }
                    else
                    {
                        Debug.Log("Importing URP Terrain Package");
                        ImportPackage(TerrainPackageURPPath, false);
                    }
                    break;
                case StormWizardRP.HDRP:
                    if (!File.Exists(TerrainPackageHDRPPath))
                    {
                        Debug.LogError("Unable to find the Terrain System Shader Update package for the HD render pipeline. The Storm installation is still working, but you may need to update the Terrain System Shaders for your current render pipeline. Please check if you can find " + TerrainPackageHDRPPath + " in your project and install it manually.");
                    }
                    else
                    {
                        Debug.Log("Importing HDRP Terrain Package");
                        ImportPackage(TerrainPackageHDRPPath, false);
                    }
                    break;
            }

        }

        private static void TerrainShaderInstallCompleted()
        {
#if UNITY_EDITOR
            StormWizardUtils.DeletePackage(StormWizardUtils.StormPackagePath);
#endif

#if PW_STORM_PRESENT
            //turn storm rendering back on after the installation, if necessary
            if (Storm.Core.StormWorld.Instance != null)
            {
                Storm.Core.StormWorld.Instance.enabled = true;
            }
#endif

        }

        /// <summary>
        /// Compares two version string in the common Major.Minor.Patch format. Will return true if the first version string "versionA" is equal or higher than "versionB".
        /// </summary>
        /// <param name="versionA"></param>
        /// <param name="versionB"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static bool IsVersionGreaterOrEqual(string versionA, string versionB)
        {
            if (string.IsNullOrWhiteSpace(versionA) || string.IsNullOrWhiteSpace(versionB))
                throw new ArgumentException("Version strings cannot be null or empty.");

            // Split and parse each part
            string[] partsA = versionA.Split('.');
            string[] partsB = versionB.Split('.');

            if (partsA.Length != 3 || partsB.Length != 3)
                throw new ArgumentException("Version strings must be in the format Major.Minor.Patch");

            for (int i = 0; i < 3; i++)
            {
                if (!int.TryParse(partsA[i], out int a) || !int.TryParse(partsB[i], out int b))
                    throw new ArgumentException("Version parts must be integers.");

                if (a > b) return true;   // A is higher
                if (a < b) return false;  // A is lower
            }

            // All parts are equal
            return true;
        }


        //        /// <summary>
        //        /// Updates the version in the storm wizard to the 'version'
        //        /// </summary>
        //        /// <param name="version"></param>
        //        public static void UpdateStormCoreVersion(string version)
        //        {
        //            //Updates the installed version
        //            StormWizardSettings settings = GetStormWizardSettings();
        //            if (settings != null && version != settings.Version)
        //            {
        //                settings.Version = version;
        //#if UNITY_EDITOR
        //                EditorUtility.SetDirty(settings);
        //                AssetDatabase.SaveAssets();
        //#endif
        //            }
        //        }
        /// <summary>
        /// Displays a dialog popup when core storm is not installed giving option to fix it. 
        /// </summary>
        public static void DisplayStormCoreMissing()
        {
#if UNITY_EDITOR
            if (EditorUtility.DisplayDialog("Storm Core Not Installed!", "Storm Core has not been installed, please make sure you install this first from the Storm Wizard before 'Fixing' the script defines.", "Fix Now", "Ok Got It"))
            {
                InstallStormCore();
            }
#endif
        }
        /// <summary>
        /// Checks to see if the storm core type exists in the project
        /// </summary>
        /// <returns></returns>
        public static bool StormCoreInstalled()
        {
            return Type.GetType("ProceduralWorlds.Storm.Core.StormWorld, ProceduralWorlds.Storm.Core, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null") != null;// && !VersionMismatch;
        }
        /// <summary>
        /// Checks to see if type exists
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool GetTypeOfExists(string type)
        {
            return Type.GetType(type) != null;
        }
        /// <summary>
        /// Returns a method info based on the type and method string you are searching for
        /// Can be null
        /// </summary>
        /// <param name="type"></param>
        /// <param name="method"></param>
        /// <returns></returns>
        public static MethodInfo GetMethodFromType(string type, string method)
        {
            Type getType = Type.GetType(type);
            if (getType != null)
            {
                return getType.GetMethod(method);
            }

            return null;
        }
        /// <summary>
        /// Deletes a unity package
        /// </summary>
        /// <param name="packageName"></param>
        /// <param name="interactable"></param>
        public static void DeletePackage(string packageName)
        {
#if UNITY_EDITOR
            if (File.Exists(packageName))
            {
                AssetDatabase.DeleteAsset(packageName);
                packageName = packageName.Replace(".unityPackage", ".meta");
                if (File.Exists(packageName))
                {
                    AssetDatabase.DeleteAsset(packageName);
                }
            }
#endif
        }
        /// <summary>
        /// Get the asset path of the first thing that matches the name
        /// </summary>
        /// <param name="fileName">File name to search for</param>
        /// <returns></returns>
        public static string GetAssetPath(string fileName)
        {
#if UNITY_EDITOR
            string fName = Path.GetFileNameWithoutExtension(fileName);
            string[] assets = UnityEditor.AssetDatabase.FindAssets(fName, null);
            for (int idx = 0; idx < assets.Length; idx++)
            {
                string path = UnityEditor.AssetDatabase.GUIDToAssetPath(assets[idx]);
                if (Path.GetFileName(path) == fileName)
                {
                    return path;
                }
            }
#endif
            return "";
        }
        /// <summary>
        /// Returns the storm wizard options root path.
        /// </summary>
        /// <returns></returns>
        public static string GetStormSettingsDirectory()
        {
            string path = GetAssetPath("Storm Wizard Options Read Me.txt");
            path = path.Replace("/Storm Wizard Options Read Me.txt", "");
            return CreatePathIfDoesNotExist(path);
        }
        /// <summary>
        /// Returns the storm wizard options root path.
        /// </summary>
        /// <returns></returns>
        public static string GetStormWorldRenderSettingsDirectory()
        {
            string path = GetAssetPath("Storm Wizard Options Read Me.txt");
            path = path.Replace("Wizard Options/Storm Wizard Options Read Me.txt", "Storm Render Settings");
            return CreatePathIfDoesNotExist(path);
        }
        /// <summary>
        /// Checks if a path exists, if not it will be created.
        /// </summary>
        /// <param name="path"></param>
        public static string CreatePathIfDoesNotExist(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            return path;
        }
        /// <summary>
        /// Gets a List of types for all classes that inherit from a certain source class type.
        /// </summary>
        /// <typeparam name="T">The type you want to get the inherited classes for</typeparam>
        /// <returns></returns>
        public static List<Type> GetInheritedTypes<T>() where T : class
        {
            List<Type> returnTypes = new List<Type>();

            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies().Where(x => x.FullName.Contains("Assembly-CSharp") || x.FullName.Contains("GaiaCore")).ToArray();

            for (int i = 0; i < assemblies.Length; i++)
            {
                Assembly assembly = assemblies[i];
                foreach (Type type in
                assembly.GetTypes()
                .Where(myType => myType.IsClass && myType.IsSubclassOf(typeof(T))))
                {
                    returnTypes.Add(type);
                }
            }

            return returnTypes;
        }

        #endregion
        #region Storm Utils Tab Functions

        /// <summary>
        /// Clears the storm world from the current scene
        /// </summary>
        /// <param name="clearFeatureSets">Can be used to clear all the features from the feature sets and deletes the generated assets</param>
        /// <param name="clearMaterials">Can be used to clear all the materials in the feature sets and deletes the generated assets</param>
        public static void ClearWorld(bool clearFeatureSets, bool clearMaterials, bool removeStormWorld)
        {
#if PW_STORM_PRESENT && UNITY_EDITOR
            //Enable the tracked objects and remove the TransformList component
            StormTransformList[] tLists = StormUtils.FindObjectsOfType<StormTransformList>(true);
            foreach (StormTransformList tList in tLists)
            {
                foreach (Transform t in tList.Transforms)
                    if (t != null)
                        t.gameObject.SetActive(true);
                if (tList.GetComponents<Component>().Length < 3)
                    DestroyImmediate(tList.gameObject);
                else
                    DestroyImmediate(tList);
            }

            //Cached Regions we will sure to clean up on
            List<StormRegion> stormRegions = new List<StormRegion>();
            //Process storm tiles
            StormTile[] stormTiles = StormUtils.FindObjectsOfType<StormTile>(true);
            foreach (StormTile tile in stormTiles)
            {
                //Add tile Regions
                StormRegion[] regions = tile.TileRegions.Regions;
                for (int i = 0; i < regions.Length; i++)
                {
                    if (!stormRegions.Contains(regions[i]))
                    {
                        stormRegions.Add(regions[i]);
                    }
                }

                DestroyImmediate(tile.gameObject);
            }

            //Add the global Regions
            StormWorld stormWorld = StormWorld.Instance;
            if (stormWorld != null)
            {
                //Add world Regions
                StormUtils.AddIfNew(stormRegions, stormWorld.Regions);
            }

            GameObject tileTransform = GameObject.Find("Storm Tiles");
            if (tileTransform != null)
            {
                DestroyImmediate(tileTransform);
            }

            if (clearFeatureSets)
            {
                List<string> deleteAssetPaths = new List<string>();
                if (stormRegions.Count > 0)
                {
                    for (int i = 0; i < stormRegions.Count; i++)
                    {
                        StormRegion region = stormRegions[i];
                        if (region == null)
                            continue;
                        for (int fs = 0; fs < region.Featuresets?.Length; ++fs)
                        {
                            StormFeatureset featureset = region.Featuresets[fs];
                            if (featureset != null)
                            {
                                float progressA = (float)i / stormRegions.Count;
                                StormFeatureItem[] featureItems = featureset.Features;
                                if (featureItems != null && featureItems.Length > 0)
                                {
                                    for (int j = 0; j < featureItems.Length; j++)
                                    {
                                        StormFeatureItem item = featureItems[j];
                                        StormFeatureSettings featureSetting = item.Feature;
                                        //Assign delete LOD materials
                                        if (clearMaterials)
                                        {
                                            StormLODData[] lods = featureSetting.LODs;
                                            if (lods.Length > 0)
                                            {
                                                for (int k = 0; k < lods.Length; k++)
                                                {
                                                    EditorUtility.DisplayProgressBar("Deleting Feature Sets", "We are removing the generated feature sets and materials for this scene. Please Wait...", progressA);
                                                    StormLODData lod = lods[k];
                                                    StormObjectRenderData[] renderData = lod.renderData;
                                                    if (renderData.Length > 0)
                                                    {
                                                        for (int l = 0; l < renderData.Length; l++)
                                                        {
                                                            StormObjectRenderData stormObjectRenderData = renderData[l];
                                                            for (int m = 0; m < stormObjectRenderData.Materials.Length; m++)
                                                            {
                                                                Material mat = stormObjectRenderData.Materials[m];
                                                                string path = AssetDatabase.GetAssetPath(mat);
                                                                if (path.Contains("Storm User Data"))
                                                                {
                                                                    if (!deleteAssetPaths.Contains(path))
                                                                    {
                                                                        deleteAssetPaths.Add(path);
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }

                                        //Assign delete feature set
                                        if (featureSetting != null)
                                        {
                                            string path = AssetDatabase.GetAssetPath(featureSetting);
                                            if (!deleteAssetPaths.Contains(path))
                                            {
                                                deleteAssetPaths.Add(path);
                                            }
                                        }
                                    }
                                }

                                EditorUtility.ClearProgressBar();
                                featureset.Features = Array.Empty<StormFeatureItem>();
                                EditorUtility.SetDirty(featureset);
                            }
                        }
                    }

                    //Batch delete assets
                    if (deleteAssetPaths.Count > 0)
                    {
                        List<string> failedPaths = new List<string>();
                        AssetDatabase.DeleteAssets(deleteAssetPaths.ToArray(), failedPaths);
                        AssetDatabase.Refresh();
                    }
                }
            }

            //Remove storm material keyword on mesh renderers
            MeshRenderer[] meshRenderers = StormUtils.FindObjectsOfType<MeshRenderer>(true);
            if (meshRenderers.Length > 0)
            {
                foreach (MeshRenderer meshRenderer in meshRenderers)
                {
                    for (int i = 0; i < meshRenderer.sharedMaterials.Length; i++)
                    {
                        meshRenderer.sharedMaterials[i].DisableKeyword("STORM_ENABLED");
                        meshRenderer.sharedMaterials[i].DisableKeyword("_STORM_ENABLED");
                        meshRenderer.sharedMaterials[i].DisableKeyword("STORM_ENABLED_ON");
                        meshRenderer.sharedMaterials[i].DisableKeyword("PROCEDURAL_INSTANCING_ON");
                        meshRenderer.sharedMaterials[i].DisableKeyword("UNITY_PROCEDURAL_INSTANCING_ENABLED");
                    }
                }
            }

            //Remove storm material keyword on terrains
            UnityEngine.Terrain[] terrains = UnityEngine.Terrain.activeTerrains;
            if (terrains.Length > 0)
            {
                for (int i = 0; i < terrains.Length; i++)
                {
                    UnityEngine.Terrain terrain = terrains[i];
                    if (terrain == null)
                    {
                        continue;
                    }

                    terrain.drawTreesAndFoliage = true;
                    TerrainData terrainData = terrain.terrainData;
                    TreePrototype[] treePrototypes = terrainData.treePrototypes;
                    if (treePrototypes.Length > 0)
                    {
                        foreach (TreePrototype treePrototype in treePrototypes)
                        {
                            if (treePrototype.prefab == null)
                            {
                                continue;
                            }

                            MeshRenderer[] meshRenders = treePrototype.prefab.GetComponentsInChildren<MeshRenderer>(true);
                            foreach (MeshRenderer meshRenderer in meshRenders)
                            {
                                for (int r = 0; r < meshRenderer.sharedMaterials.Length; r++)
                                {
                                    meshRenderer.sharedMaterials[r].DisableKeyword("STORM_ENABLED");
                                    meshRenderer.sharedMaterials[r].DisableKeyword("_STORM_ENABLED");
                                    meshRenderer.sharedMaterials[r].DisableKeyword("STORM_ENABLED_ON");
                                    meshRenderer.sharedMaterials[r].DisableKeyword("PROCEDURAL_INSTANCING_ON");
                                    meshRenderer.sharedMaterials[r].DisableKeyword("UNITY_PROCEDURAL_INSTANCING_ENABLED");
                                    EditorUtility.SetDirty(meshRenderer.sharedMaterials[r]);
                                }
                            }
                        }
                    }
                }
            }

            StormWorld[] worlds = StormUtils.FindObjectsOfType<StormWorld>(true);
            foreach (StormWorld world in worlds)
            {
                world.Dispose();
                world.ForceFullRefresh();

                if (removeStormWorld)
                {
                    if (world.GetComponents<Component>().Length < 3)
                        DestroyImmediate(world.gameObject);
                    else
                        DestroyImmediate(world);
                }
            }

            //Cleanup if changes happened
            PerformEditorCleanUpAndRefreshStorm(2, false);

#if UNITY_EDITOR
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
            EditorUtility.ClearProgressBar();
#endif
#endif
        }

        /// <summary>
        /// Returns the first asset that matches the file path and name passed. Will try
        /// full path first, then will try just the file name.
        /// </summary>
        /// <param name="fileNameOrPath">File name as standalone or fully pathed</param>
        /// <returns>Object or null if it was not found</returns>
        public static UnityEngine.Object GetAsset(string fileNameOrPath, Type assetType)
        {
#if UNITY_EDITOR
            if (!string.IsNullOrEmpty(fileNameOrPath))
            {
                UnityEngine.Object obj = AssetDatabase.LoadAssetAtPath(fileNameOrPath, assetType);
                if (obj != null)
                {
                    return obj;
                }
                else
                {
                    string path = GetAssetPath(Path.GetFileName(fileNameOrPath));
                    if (!string.IsNullOrEmpty(path))
                    {
                        return AssetDatabase.LoadAssetAtPath(path, assetType);
                    }
                }
            }
#endif
            return null;
        }

        /// <summary>
        /// Expects a path starting at the asset folder, and will return a full file system path starting at the Drive Letter
        /// </summary>
        /// <param name="inputPath">Unity path starting at the asset folder</param>
        /// <returns></returns>
        public static string GetFullFileSystemPath(string inputPath)
        {
            return Application.dataPath.Substring(0, Application.dataPath.Length - "Assets".Length) + inputPath;
        }

        /// <summary>
        /// Cleans up the featuresets by removing them from being rendered if their instance count isn't high enough
        /// </summary>
        /// <param name="transformInstanceCount"></param>
        public static void CleanUpFeatureSets(int transformInstanceCount, bool ignoreTRSLists, bool disableTerrainFoliageRendering, bool deleteEmptyTiles)
        {
#if PW_STORM_PRESENT && UNITY_EDITOR
            //Data we will use later
            List<StormDeleteFeatureData> dataToRemove = new List<StormDeleteFeatureData>();

            //Get all storm tiles
            StormTile[] stormTiles = StormUtils.FindObjectsOfType<StormTile>(true);
            int currentTile = 0;
            int trsClearedCount = 0;
            int transformsClearedCount = 0;
            int feautresClearedCount = 0;
            int featureSetsRemovedFromTileCount = 0;
            int stormTileDeletedCount = 0;
            if (stormTiles.Length > 0)
            {
                //Go through all the tiles
                for (int i = 0; i < stormTiles.Length; i++)
                {
                    //Go through each tile
                    StormTile tile = stormTiles[i];
                    currentTile++;
                    EditorUtility.DisplayProgressBar("Cleaning Up Tile... Please Wait", "Cleaning TRS, Transforms and Feature Sets on Tile: " + tile.name, (float)currentTile / stormTiles.Length);
                    if (tile != null)
                    {
                        //Make sure feature set is there
                        StormTileRegion tileRegions = tile.TileRegions;
                        if (tileRegions != null)
                        {
                            //Create delete data
                            dataToRemove.Add(new StormDeleteFeatureData
                            {
                                TileName = tile.name,
                                Regions = tileRegions.Regions
                            });

                            //Go through TRS datas
                            if (!ignoreTRSLists)
                            {
                                List<TRSData> tRSDatas = tile.TileData.TRSData.ToList();
                                if (tRSDatas.Count > 0)
                                {
                                    //Loop through all counts
                                    for (int j = tRSDatas.Count; j-- > 0;)
                                    {
                                        //Data
                                        TRSData tRSData = tRSDatas[j];
                                        StormTRSList trsList = tRSData.Data;
                                        if (trsList != null && trsList.Transforms.Length > 0)
                                        {
                                            //Needs to be removed if true
                                            if (trsList.Transforms.Length <= transformInstanceCount)
                                            {
                                                //Add count
                                                trsClearedCount++;
                                                //Add the layer name that needs to be removed from feature sets
                                                if (!dataToRemove[i].FeatureLayerName.Contains(tRSData.LayerName))
                                                {
                                                    dataToRemove[i].FeatureLayerName.Add(tRSData.LayerName);
                                                }

                                                //Empty the array
                                                tRSData.Data.Transforms = Array.Empty<StormUnityItemData>();
                                                //Remove the data completely from rendering
                                                tRSDatas.RemoveAt(j);
                                            }
                                        }
                                    }

                                    //Re assign the updated data array
                                    tile.TileData.TRSData = tRSDatas.ToArray();
                                }
                            }

                            ///Go through transform data
                            List<TransformData> transformDatas = tile.TileData.TransformData.ToList();
                            if (transformDatas.Count > 0)
                            {
                                //Loop through all counts
                                for (int j = transformDatas.Count; j-- > 0;)
                                {
                                    //Data
                                    TransformData transformData = transformDatas[j];
                                    StormTransformList transformList = transformData.Data;
                                    if (transformList != null && transformList.ItemCount > 0)
                                    {
                                        //Needs to be removed if true
                                        if (transformList.ItemCount <= transformInstanceCount)
                                        {
                                            //Add count
                                            transformsClearedCount++;
                                            //Add the layer name that needs to be removed from feature sets
                                            if (!dataToRemove[i].FeatureLayerName.Contains(transformData.LayerName))
                                            {
                                                dataToRemove[i].FeatureLayerName.Add(transformData.LayerName);
                                            }

                                            //Loop through all the transforms to re-enable the objects and remove the transform tracker script
                                            for (int c = transformList.Transforms.Length; c-- > 0;)
                                            {
                                                Transform transform = transformList.Transforms[c];
                                                if (transform != null)
                                                {
                                                    //Enable the object
                                                    transform.gameObject.SetActive(true);
                                                }
                                            }

                                            //Empty the array
                                            transformData.Data.Clear();
                                            //Remove the data completely from rendering
                                            transformDatas.RemoveAt(j);
                                        }
                                    }
                                }

                                //Re assign the updated data array
                                tile.TileData.TransformData = transformDatas.ToArray();
                                EditorUtility.SetDirty(tile);
                            }
                        }
                    }
                }

                //Go through the data
                if (dataToRemove.Count > 0)
                {
                    for (int t = 0; t < stormTiles.Length; t++)
                    {
                        StormTile tile = stormTiles[t];
                        for (int i = dataToRemove.Count; i-- > 0;)
                        {
                            //Delete data
                            StormDeleteFeatureData deleteData = dataToRemove[i];
                            //Featuresets
                            StormRegion[] stormRegions = deleteData.Regions;
                            //Tile name
                            string tileName = deleteData.TileName;

                            //Continue if the tile name is wrong
                            if (tile.name != tileName)
                            {
                                continue;
                            }

                            //Removes the data from the Regions to avoid storm trying to render it
                            if (stormRegions.Length > 0)
                            {
                                //Loop through all counts
                                for (int j = 0; j < stormRegions.Length; j++)
                                {
                                    //Loop through the feature names to remove on the tile
                                    for (int l = 0; l < deleteData.FeatureLayerName.Count; l++)
                                    {
                                        //Layer to remove
                                        string layerToRemove = deleteData.FeatureLayerName[l];
                                        //Featureset data
                                        StormRegion region = stormRegions[j];
                                        if (region == null)
                                            continue;
                                        for (int fs = 0; fs < region.Featuresets?.Length; ++fs)
                                        {
                                            StormFeatureset featureset = region.Featuresets[fs];
                                            if (featureset != null && featureset.Features.Length > 0)
                                            {
                                                //List of the feature set items
                                                List<StormFeatureItem> featureItems = featureset.Features.ToList();
                                                for (int s = 0; s < featureItems.Count; s++)
                                                {
                                                    //Feature item
                                                    StormFeatureItem item = featureItems[s];
                                                    if (item.Feature != null)
                                                    {
                                                        //Remove if it's the same name
                                                        if (item.Feature.name == layerToRemove)
                                                        {
                                                            //Add count
                                                            feautresClearedCount++;
                                                            //Remove instance from rendering
                                                            featureItems.RemoveAt(s);
                                                        }
                                                    }
                                                }

                                                //Re assign the updated data array
                                                featureset.Features = featureItems.ToArray();
                                                EditorUtility.SetDirty(featureset);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                //Go through all the tile again and remove and Regions that are empty
                for (int i = stormTiles.Length; i-- > 0;)
                {
                    //Get the tile
                    StormTile tile = stormTiles[i];
                    if (tile != null)
                    {
                        //Make sure Regions are there
                        StormTileRegion tileRegions = tile.TileRegions;
                        if (tileRegions != null && tileRegions.Regions?.Length > 0)
                        {
                            //Get a list of Regions
                            List<StormRegion> regions = tileRegions.Regions.ToList();
                            //Loop through backwards
                            for (int j = regions.Count; j-- > 0;)
                            {
                                //If it's empty remove it
                                if (regions[j].Featuresets != null && regions[j].Featuresets.Length <= 0)
                                {
                                    featureSetsRemovedFromTileCount++;
                                    regions.RemoveAt(j);
                                }
                            }
                            //Re assign the updated data array
                            tileRegions.Regions = regions.ToArray();
                        }

                        //Should we check to delete tile?
                        if (deleteEmptyTiles)
                        {
                            //Delete tile if there are no regions
                            if (tileRegions.Regions.Length <= 0)
                            {
                                stormTileDeletedCount++;
                                //If there is no terrain remove the whole object otherwise just the component
                                if (tile.GetComponent<UnityEngine.Terrain>() == null)
                                {
                                    DestroyImmediate(tile.gameObject);
                                }
                                else
                                {
                                    DestroyImmediate(tile);
                                }
                            }
                        }
                    }
                }
            }

            //Disable draw vegetation
            if (disableTerrainFoliageRendering)
            {
                //Get all terrains
                UnityEngine.Terrain[] terrains = UnityEngine.Terrain.activeTerrains;
                if (terrains != null && terrains.Length > 0)
                {
                    //Foreach terrain disable vegetation rendering
                    foreach (UnityEngine.Terrain terrain in terrains)
                    {
                        terrain.drawTreesAndFoliage = false;
                    }
                }
            }

            //Log the results
            if (trsClearedCount > 0 || transformsClearedCount > 0 || feautresClearedCount > 0 || featureSetsRemovedFromTileCount > 0 || stormTileDeletedCount > 0)
            {
                //Cleanup if changes happened
                PerformEditorCleanUpAndRefreshStorm(2);

                Debug.Log("Successfully cleaned up Feature Sets." +
                    "\nRemoved: <color=green>" + trsClearedCount + "</color> TRS lists." +
                    "\nRemoved: <color=red>" + transformsClearedCount + "</color> Transform lists." +
                    "\nRemoved <color=blue>" + feautresClearedCount + "</color> Feature sets from storm rendering." +
                    "\nRemoved <color=yellow>" + featureSetsRemovedFromTileCount + "</color> Feature sets from storm tiles as they had no features to render." +
                    "\nDeleted <color=cyan>" + stormTileDeletedCount + "</color> Storm tiles from the scene as there was not features sets to render.");
            }
            else
            {
                Debug.Log("<color=green> Everything has already been cleaned up.</color>");
            }

            //Clear any progress bars
            EditorUtility.ClearProgressBar();
#endif
        }
        /// <summary>
        /// Creates StormWorld object
        /// </summary>
#if PW_STORM_PRESENT
        public static StormWorld CreateStormWorld()
#else
        public static void CreateStormWorld()
#endif
        {
#if PW_STORM_PRESENT
            GameObject go = new GameObject("Storm World");
            go.transform.position = Vector3.zero;
            go.transform.localScale = Vector3.one;
            go.transform.rotation = Quaternion.identity;
            StormWorld stormWorld = go.AddComponent<UI.StormWorldManager>();
            stormWorld.QualitySettings = StormWizardSettings.GetRenderQuality();
            CheckWizardFinished = true;
            return stormWorld;
#else
            Debug.LogError("The Storm script define 'PW_STORM_PRESENT' has not be set in the project. You can go to the Storm Wizard to resolve this or you can add the define in yourself via the project settings.");
#endif
        }

        /// <summary>
        /// Performs an editor cleanup to help unload unused assets and free up memory
        /// </summary>
        /// <param name="finishUpSeconds"></param>
        /// <param name="refreshStorm"></param>
        private static void PerformEditorCleanUpAndRefreshStorm(int finishUpSeconds, bool refreshStorm = true)
        {
#if UNITY_EDITOR
            //Update storm rendering
            if (refreshStorm)
            {
#if PW_STORM_PRESENT
                StormWorld stormWorld = StormWorld.Instance;
                if (stormWorld != null)
                {
                    stormWorld.ForceFullRefresh();
                    EditorUtility.SetDirty(stormWorld);
                }
#endif
            }

            //Check there is at least 1 second
            if (finishUpSeconds < 1)
            {
                finishUpSeconds = 1;
            }

            //Step for progress
            float step = 0.1f;
            //How long it will take
            for (float t = 0; t < (float)finishUpSeconds; t += step)
            {
                //Cleanup
                if (t == 0.5f)
                {
                    //Unload unused assets to clean up
                    Resources.UnloadUnusedAssets();
                }
                else if (t == 1f)
                {
                    //Do garbage collection to clean up more memory
                    GarbageCollector.CollectIncremental();
                }

                EditorUtility.DisplayProgressBar("Cleaning Up!", "Finishing up and cleaning up the editor. Please Wait...", t / finishUpSeconds);
                Thread.Sleep((int)(step * 1000.0f));
            }

            //Refresh database
            AssetDatabase.Refresh();
            SceneView.RepaintAll();
#endif
        }

        /// <summary>
        /// Returns the path to the maintenance token for the Storm Setup Wizard
        /// </summary>
        /// <returns></returns>
        public static string GetTokenDirectory()
        {
            return GetAssetPath("Storm" + Path.AltDirectorySeparatorChar + "Resources", false, SearchMode.EndsWith);
        }

        /// <summary>
        /// Get the asset path of the first thing that matches the name
        /// </summary>
        /// <param name="name">File name to search for</param>
        /// <returns></returns>
        public static string GetAssetPath(string name, bool isFile = true, SearchMode searchMode = SearchMode.Equals)
        {
#if UNITY_EDITOR
            string fName = Path.GetFileNameWithoutExtension(name);
            string[] assets = AssetDatabase.FindAssets(fName, null);
            for (int idx = 0; idx < assets.Length; idx++)
            {
                string path = AssetDatabase.GUIDToAssetPath(assets[idx]);

                string compareString = isFile ? Path.GetFileName(path) : path;

                switch (searchMode)
                {
                    case SearchMode.Equals:
                        if (compareString == name)
                        {
                            return path;
                        }
                        break;
                    case SearchMode.StartsWith:
                        if (compareString.StartsWith(name))
                        {
                            return path;
                        }
                        break;
                    case SearchMode.Contains:
                        if (compareString.Contains(name))
                        {
                            return path;
                        }
                        break;
                    case SearchMode.EndsWith:
                        if (compareString.EndsWith(name))
                        {
                            return path;
                        }
                        break;
                }
            }
#endif
            return "";
        }

        public static bool StormCorePackageExists()
        {
            return File.Exists(StormPackagePath);
        }

        //Helper to deal with the FindObjectOfType API changes
        public static new T FindObjectOfType<T>(bool findInactive) where T : UnityEngine.Object
        {
#if UNITY_6000_0_OR_NEWER
            return GameObject.FindFirstObjectByType(typeof(T), findInactive ? FindObjectsInactive.Include: FindObjectsInactive.Exclude) as T;
#else
            return GameObject.FindObjectOfType(typeof(T), findInactive) as T;
#endif
        }
        //Helper to deal with the FindObjectsOfType API changes
        public static new T[] FindObjectsOfType<T>(bool findInactive) where T : UnityEngine.Object
        {
#if UNITY_6000_0_OR_NEWER
            return GameObject.FindObjectsByType(typeof(T), findInactive ? FindObjectsInactive.Include : FindObjectsInactive.Exclude, FindObjectsSortMode.None) as T[];
#else
            return GameObject.FindObjectsOfType(typeof(T), findInactive) as T[];
#endif
        }

        public static RenderPipelineAsset GetRenderPipelineAsset()
        {
#if UNITY_6000_0_OR_NEWER
            return GraphicsSettings.defaultRenderPipeline;
#else
            return GraphicsSettings.renderPipelineAsset;
#endif
        }

        public static void OpenUserQuickStart()
        {
            UnityEngine.Object guide = (UnityEngine.Object)StormWizardUtils.GetAsset("Storm - Quick Start Guide.pdf", typeof(UnityEngine.Object));
            if (guide != null)
            {
                Application.OpenURL(StormWizardUtils.GetFullFileSystemPath(AssetDatabase.GetAssetPath(guide)));
                //EditorGUIUtility.PingObject(guide);
            }
        }

        public static void RunXattrCommand(string filePath)
        {
            if (SystemInfo.operatingSystemFamily != OperatingSystemFamily.MacOSX)
            {
                return;
            }

            string command = "xattr";
            string arguments = $"-rd com.apple.quarantine \"{filePath}\"";

            try
            {
                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = command,
                    Arguments = arguments,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (Process proc = new Process { StartInfo = psi })
                {
                    proc.Start();

                    string stdout = proc.StandardOutput.ReadToEnd();
                    string stderr = proc.StandardError.ReadToEnd();

                    proc.WaitForExit();

                    if (!string.IsNullOrEmpty(stdout))
                        UnityEngine.Debug.Log($"xattr output for {filePath}:\n{stdout}");

                    if (!string.IsNullOrEmpty(stderr))
                        UnityEngine.Debug.LogWarning($"xattr error for {filePath}:\n{stderr}");
                }
            }
            catch (System.Exception ex)
            {
                UnityEngine.Debug.LogError("Exception while running xattr: " + ex.Message);
            }
        }

        #endregion
    }
}