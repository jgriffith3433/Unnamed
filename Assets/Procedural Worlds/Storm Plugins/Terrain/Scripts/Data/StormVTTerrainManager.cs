using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;


namespace ProceduralWorlds.Storm.Terrain
{


    public class StormVTTerrainManager : MonoBehaviour
    {
        /// <summary>
        /// Stores a table of currently loaded terrains in the scene
        /// </summary>
        public Dictionary<int2, StormVTTerrainInfo> m_activeTerrains = new Dictionary<int2, StormVTTerrainInfo>();

        public StormVTSettings stormVTSettings;

        /// <summary>
        /// Event system to call when a new terrain has been loaded in the scene
        /// </summary>
        /// <param name="terrainInfo"></param>
        public delegate void NewTerrainAdded(StormVTTerrainInfo terrainInfo);
        public static event NewTerrainAdded OnNewTerrainAdded;

        /// <summary>
        /// Singleton instance of this manager
        /// </summary>
        private static StormVTTerrainManager instance = null;

        public static StormVTTerrainManager Instance
        {
            get
            {
                if (instance == null)
                {
                    GameObject managerObject = GameObject.Find("Physical Texture Manager");
                    if (managerObject != null)
                    {
                        StormVTTerrainManager terrainManager = managerObject.GetComponent<StormVTTerrainManager>();
                        if(terrainManager != null)
                        {
                            instance = terrainManager;
                        }
                    }
                }

                return instance;
            }
        }

        /// <summary>
        /// Returns the storm terrain settings scriptable object, used for TerrainInfo component
        /// </summary>
        /// <returns></returns>
        public StormVTSettings GetSettings()
        {
            return stormVTSettings;
        }

        /// <summary>
        /// Adds a terrain into the dictionary, calls the callback
        /// </summary>
        /// <param name="terrainInfo"></param>
        public void AddTerrain(StormVTTerrainInfo terrainInfo)
        {
            if (!m_activeTerrains.ContainsKey(terrainInfo.terrainGridIndex))
            {
                m_activeTerrains.Add(terrainInfo.terrainGridIndex, terrainInfo);
                if(StormVTTerrainManager.OnNewTerrainAdded != null)
                    StormVTTerrainManager.OnNewTerrainAdded(terrainInfo);
            }

        }

        /// <summary>
        /// Removes a terrain from the dictionary
        /// </summary>
        /// <param name="terrainInfo"></param>
        public void RemoveTerrain(StormVTTerrainInfo terrainInfo)
        {
            if (m_activeTerrains.ContainsKey(terrainInfo.terrainGridIndex))
            {
                m_activeTerrains.Remove(terrainInfo.terrainGridIndex);
            }
        }

        /// <summary>
        /// Removing the static instance when this object gets destroyed (e.g. scene close, etc.)
        /// </summary>
        private void OnDestroy()
        {
            instance = null;
        }

    }
}
