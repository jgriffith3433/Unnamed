using System;
using System.Collections.Generic;
using UnityEngine;


namespace ProceduralWorlds.Storm.Terrain
{

    /// <summary>
    /// Base class with overrideable virtual methods to connect to Gaia, the real implementation of those methods happens outside 
    /// of the DLL code.
    /// </summary>
    public class StormVTGaiaConnectorBase : ScriptableObject
    {

        /// <summary>
        /// Does the current scene have dynamic loaded terrains under control of the Gaia Terrain Loader Manager
        /// </summary>
        public virtual bool HasDynamicLoadedTerrains()
        {
            //if this method is not overridden / implemented, we can assume there is no terrain loading
            return false;
        }

        /// <summary>
        /// Does the current scene use the floating point fix solution?
        /// </summary>
        public virtual bool HasFloatingPointFix()
        {
            //if this method is not overridden / implemented, we can assume there is no FPF
            return false;
        }

        /// <summary>
        /// Sets a new floating point fix origin in the Gaia Terrain Loader Manager
        /// </summary>
        /// <param name="X"></param>
        /// <param name="Y"></param>
        /// <param name="Z"></param>
        public virtual void SetFloatingPointOrigin(double X, double Y, double Z)
        {

        }

        /// <summary>
        /// Allows you to send an action that you want to perform on all terrains under control of the Gaia Terrain Loader Manager. The terrains will be loaded in one after another,
        /// the action will be peformed on the loaded terrain and then the next terrain will be loaded, etc.
        /// </summary>
        /// <param name="terrainAction">Function to perform</param>
        /// <param name="dirtyScenes">Whether to set the loaded scene to dirty afterwards</param>
        /// <param name="terrainNames">List of terrains to perform the action on</param>
        /// <param name="progressBarText">Progress of the action</param>
        public virtual void CallFunctionOnLoadedTerrains(Action<UnityEngine.Terrain> terrainAction, bool dirtyScenes, List<string> terrainNames, string progressBarText)
        {

        }

        /// <summary>
        /// Unloads all Gaia controlled terrains from the scene
        /// </summary>
        /// <param name="forceUnload"></param>
        public virtual void UnloadAllTerrains(bool forceUnload)
        {
            
        }

        /// <summary>
        /// Enables / Disables whether Gaia can load the Terrain Scenes
        /// </summary>
        /// <param name="loadingEnabled">State of the terrain loading system</param>
        public virtual void SetTerrainLoadingEnabled(bool loadingEnabled)
        {

        }

        /// <summary>
        /// Forces an unload of all Gaia Terrain Scenes
        /// </summary>
        public virtual void ForceUnloadGaiaTerrains()
        {

        }

        /// <summary>
        /// Forces a load of all Gaia Terrain Scenes
        /// </summary>
        public virtual void ForceLoadGaiaTerrains()
        {

        }

        /// <summary>
        /// Disbales the main Gaia floating point fix script
        /// </summary>
        public virtual void DisableGaiaFPF()
        {

        }

        /// <summary>
        /// Enables the main Gaia floating point fix script
        /// </summary>
        public virtual void EnableGaiaFPF()
        {

        }

        /// <summary>
        /// Get the current range at which a floating point fix scene shift would be triggered from Gaia 
        /// </summary>
        public virtual float GetFloatingPointFixRange()
        {
            return 0f;
        }

    }
}
