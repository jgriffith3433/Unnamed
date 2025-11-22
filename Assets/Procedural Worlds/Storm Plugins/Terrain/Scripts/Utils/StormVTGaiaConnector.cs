using System.Collections.Generic;
using UnityEngine;
using System;

#if UNITY_EDITOR
#if GAIA_PRO_PRESENT || GAIA_2023_PRO
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
#endif
#endif

#if GAIA_PRO_PRESENT || GAIA_2023_PRO
using Gaia;
#endif

namespace ProceduralWorlds.Storm.Terrain
{
    //keep commented out, since this is not useful for the average user
    //[CreateAssetMenu(fileName = "StormVTGaiaConnector", menuName = "Procedural Worlds/Storm/Terrain/StormVTGaiaConnector")]
    public class StormVTGaiaConnector : StormVTGaiaConnectorBase
    {
        public override bool HasDynamicLoadedTerrains()
        {
#if GAIA_PRO_PRESENT || GAIA_2023_PRO
            return GaiaUtils.HasDynamicLoadedTerrains();
#else
            return false;
#endif
        }

        public override bool HasFloatingPointFix()
        {
#if GAIA_PRO_PRESENT || GAIA_2023_PRO
            return GaiaUtils.UsesFloatingPointFix();
#else
            return false;
#endif
        }

        public override void SetFloatingPointOrigin(double X, double Y, double Z)
        {
#if GAIA_PRO_PRESENT || GAIA_2023_PRO
            TerrainLoaderManager.Instance.SetOrigin(new Gaia.Vector3Double(X,Y,Z));
#endif
        }

        public override void CallFunctionOnLoadedTerrains(Action<UnityEngine.Terrain> terrainAction, bool dirtyScenes, List<string> terrainNames, string progressBarText)
        {
#if GAIA_PRO_PRESENT || GAIA_2023_PRO
            GaiaUtils.CallFunctionOnDynamicLoadedTerrains(terrainAction, dirtyScenes, terrainNames, progressBarText);
#endif
        }


        public override void UnloadAllTerrains(bool forceUnload)
        {
#if GAIA_PRO_PRESENT || GAIA_2023_PRO
            //TerrainLoaderManager.Instance.UnloadAll(forceUnload);
#endif
        }

        public override void SetTerrainLoadingEnabled(bool loadingEnabled)
        {
#if GAIA_PRO_PRESENT || GAIA_2023_PRO
            TerrainLoaderManager.Instance.TerrainSceneStorage.m_terrainLoadingEnabled = loadingEnabled;
#endif
        }

        public override void ForceUnloadGaiaTerrains()
        {
#if UNITY_EDITOR
#if GAIA_PRO_PRESENT || GAIA_2023_PRO
            //Collect Terrains in Gaia Terrain Loader
            List<Scene> terrainScenes = new List<Scene>();
            for (int i = 0; i < TerrainLoaderManager.Instance.TerrainSceneStorage.m_terrainScenes.Count; i++)
            {

                Scene scene = EditorSceneManager.GetSceneByPath(TerrainLoaderManager.Instance.TerrainSceneStorage.m_terrainScenes[i].m_scenePath);
                terrainScenes.Add(scene);
            }

            //Loop through all terrain scenes, if loaded, unload them
            for (int i = 0; i < terrainScenes.Count; i++)
            {
                if (terrainScenes[i].isLoaded)
                {
                    SceneManager.UnloadSceneAsync(terrainScenes[i]);
                }
            }
#endif
#endif
        }

        public override void ForceLoadGaiaTerrains()
        {
#if UNITY_EDITOR
#if GAIA_PRO_PRESENT || GAIA_2023_PRO

            //nothing to do if there is no terrain loader in the scene
            GameObject loaderManager = GaiaUtils.GetTerrainLoaderManagerObject(false);
            if (loaderManager == null)
            {
                return;
            }

            //Collect Terrains in Gaia Terrain Loader
            List<Scene> terrainScenes = new List<Scene>();
            for (int i = 0; i < TerrainLoaderManager.Instance.TerrainSceneStorage.m_terrainScenes.Count; i++)
            {

                Scene scene = EditorSceneManager.GetSceneByPath(TerrainLoaderManager.Instance.TerrainSceneStorage.m_terrainScenes[i].m_scenePath);
                terrainScenes.Add(scene);
            }

            //Loop through all terrain scenes, if loaded, unload them
            for (int i = 0; i < terrainScenes.Count; i++)
            {
                if (terrainScenes[i].isLoaded)
                {
                    Scene terrainScene = terrainScenes[i];
                    GameObject[] rootObjects = terrainScene.GetRootGameObjects();

                    // Loop through all root GameObjects
                    foreach (GameObject rootObject in rootObjects)
                    {
                        UnityEngine.Terrain[] sceneTerrains = rootObject.GetComponentsInChildren<UnityEngine.Terrain>();
                        foreach(UnityEngine.Terrain terrain in sceneTerrains)
                        {
                            terrain.drawHeightmap = true;
                        }
                    }
                }
            }
#endif
#endif
        }

        public override void DisableGaiaFPF()
        {
#if UNITY_EDITOR
#if GAIA_PRO_PRESENT || GAIA_2023_PRO
            if (FloatingPointFix.Instance != null)
            {
                FloatingPointFix.Instance.enabled = false;
            }
#endif
#endif
        }

        public override void EnableGaiaFPF()
        {
#if UNITY_EDITOR
#if GAIA_PRO_PRESENT || GAIA_2023_PRO
            if (FloatingPointFix.Instance != null)
            {
                FloatingPointFix.Instance.enabled = true;
            }
#endif
#endif
        }

        public override float GetFloatingPointFixRange()
        {
#if UNITY_EDITOR
#if GAIA_PRO_PRESENT || GAIA_2023_PRO
            if (FloatingPointFix.Instance != null)
            {
                return FloatingPointFix.Instance.threshold;
            }
#endif
#endif
            return 0f;
        }

    }
}
