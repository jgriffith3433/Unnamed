using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProceduralWorlds.Storm.Terrain
{
    //keep commented out, since this is not useful for the average user
    //[CreateAssetMenu(fileName = "StormVTGaia", menuName = "Procedural Worlds/Storm/Terrain/StormVTGaia")]
    public class StormVTGaia : ScriptableObject
    {
        /// <summary>
        /// Reference to the Gaia Connector 
        /// </summary>
        [SerializeField] public StormVTGaiaConnectorBase gaiaConnectorBase;

        #region Singleton

        /// <summary>
        /// Member variable of the Storm Terrain Gaia Instance
        /// </summary>
        private static StormVTGaia m_instance = null;

        /// <summary>
        /// Storm Terrain Gaia Instance
        /// </summary>
        public static StormVTGaia Instance
        {
            get
            {
                return m_instance;
            }
            set { m_instance = value; }
        }

        #endregion

        #region Gaia Functions

        /// <summary>
        /// Returns whether Gaia uses Terrain Scenes to load in terrains
        /// </summary>
        /// <returns></returns>
        public bool IsGaiaTerrainLoadedScene()
        {
            return gaiaConnectorBase.HasDynamicLoadedTerrains();
        }

        /// <summary>
        /// Returns whether Gaia uses the Floating Point Fix
        /// </summary>
        /// <returns></returns>
        public bool IsGaiaFloatingPointFixScene()
        {
            return gaiaConnectorBase.HasFloatingPointFix();
        }

        /// <summary>
        /// Sets the floating point fix origin
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        public void SetGaiaFloatingPointFixOrigin(double x, double y, double z)
        {
            gaiaConnectorBase.SetFloatingPointOrigin(x, y, z);
        }

        /// <summary>
        /// Allows you to send an action that you want to perform on all terrains under control of the Gaia Terrain Loader Manager. The terrains will be loaded in one after another,
        /// the action will be peformed on the loaded terrain and then the next terrain will be loaded, etc.
        /// </summary>
        /// <param name="terrainAction">Function to perform</param>
        /// <param name="dirtyScenes">Whether to set the loaded scene to dirty afterwards</param>
        /// <param name="terrainNames">List of terrains to perform the action on</param>
        /// <param name="progressBarText">Progress of the action</param>
        public void CallFunctionOnGaiaLoadedTerrains(Action<UnityEngine.Terrain> terrainAction, bool dirtyScenes, List<string> terrainNames = null, string progressBarText = "")
        {
            gaiaConnectorBase.CallFunctionOnLoadedTerrains(terrainAction, dirtyScenes, terrainNames, progressBarText);
        }

        /// <summary>
        /// Unloads all the currently loaded Gaia Terrain Scenes
        /// </summary>
        /// <param name="forceUnload"></param>
        public void UnloadAllGaiaTerrains(bool forceUnload)
        {
            gaiaConnectorBase.UnloadAllTerrains(forceUnload);
        }

        /// <summary>
        /// Enables / Disables whether Gaia can load the Terrain Scenes
        /// </summary>
        /// <param name="loadingEnabled"></param>
        public void SetTerrainLoadingEnabled(bool loadingEnabled)
        {
            gaiaConnectorBase.SetTerrainLoadingEnabled(loadingEnabled);
        }

        /// <summary>
        /// Forces an unload of all Gaia Terrain Scenes
        /// </summary>
        public void ForceUnloadGaiaTerrains()
        {
            gaiaConnectorBase.ForceUnloadGaiaTerrains();
        }

        /// <summary>
        /// Forces a load of all Gaia Terrain Scenes
        /// </summary>
        public void ForceLoadGaiaTerrains()
        {
            gaiaConnectorBase.ForceLoadGaiaTerrains();
        }

        /// <summary>
        /// Disables the main Gaia Floating Point Fix script
        /// </summary>
        public void DisableGaiaFPF()
        {
            gaiaConnectorBase.DisableGaiaFPF();
        }

        /// <summary>
        /// Enables the main Gaia Floating Point Fix script
        /// </summary>
        public void EnableGaiaFPF()
        {
            gaiaConnectorBase.EnableGaiaFPF();
        }

        /// <summary>
        /// Get the current range at which a floating point fix scene shift would be triggered from Gaia
        /// </summary>
        /// <returns></returns>
        public float GetFloatingPointFixRange()
        {
            return gaiaConnectorBase.GetFloatingPointFixRange();
        }

        #endregion

    }
}
