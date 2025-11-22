using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using static ProceduralWorlds.Storm.Terrain.StormVTEnums;

namespace ProceduralWorlds.Storm.Terrain
{

    #region Public Classes

    /// <summary>
    /// Terrain texture data object used to store textures (Height, Normal, Splat, etc) of a terrain
    /// </summary>
    [Serializable]
    public class StormVTTerrainTextureData
    {
        public int2 gridIndex;
        public bool texturesLoaded;
        public bool lowRes;
        public Texture2D heightmap;
        public Texture2D normalmap;
        public Texture2D[] splatmaps;
        public Texture holesmap;
        public float heightScale;
    }

    /// <summary>
    /// Small class used to store each terrain's grid index and distance from the camera
    /// </summary>
    public class TerrainDistanceID
    {
        public int2 gridIndex;
        public float distance;
    }

    #endregion



    [Serializable]
    public class StormVTTerrainTextureManager : MonoBehaviour
    {

        #region Public Variables

        /// <summary>
        /// Reference to the storm terrain settings scriptable object
        /// </summary>
        public StormVTSettings stormVTSettings;

        /// <summary>
        /// Reference to the storm terrain grid settings scriptable object
        /// </summary>
        public StormVTTerrainGrid stormVTTerrainGrid;

        /// <summary>
        /// Reference to the texture loading system component
        /// </summary>
        public StormVTTextureLoadingSystem textureLoadingSystem;

        /// <summary>
        /// A list of low resolution terrain texture data 
        /// </summary>
        [HideInInspector]
        public StormVTTerrainTextureData[] m_debugLowResTerrainData;

        /// <summary>
        /// A list of high resolution terrain texture data
        /// </summary>
        [HideInInspector]
        public StormVTTerrainTextureData[] m_debugHighResTerrainData;

        /// <summary>
        /// Dictionary to keep track of low resolution texture data
        /// </summary>
        private DictionaryQueue<int2, StormVTTerrainTextureData> m_lowResTerrainDataQueue = new DictionaryQueue<int2, StormVTTerrainTextureData>();

        /// <summary>
        /// Dictionary to keep track of high resolution texture data
        /// </summary>
        private DictionaryQueue<int2, StormVTTerrainTextureData> m_highResTerrainDataQueue = new DictionaryQueue<int2, StormVTTerrainTextureData>();

        /// <summary>
        /// List of low resolution terrains loaded in
        /// </summary>
        private List<TerrainDistanceID> m_lowResTerrainsList = new List<TerrainDistanceID>();

        /// <summary>
        /// List of high resolution terrains loaded in
        /// </summary>
        private List<TerrainDistanceID> m_highResTerrainsList = new List<TerrainDistanceID>();

        /// <summary>
        /// Reference to the main camera, used for position tracking
        /// </summary>
        public Camera m_mainCamera;

        /// <summary>
        /// Amount of memory consumed by high resolution terrain textures loaded in
        /// </summary>
        public string m_totalHighResMemory;

        /// <summary>
        /// Amount of memory consumed by low resolution terrain textures loaded in
        /// </summary>
        public string m_totalLowResMemory;



        #endregion

        #region Private Variables

        /// <summary>
        /// Last tracked position
        /// </summary>
        private Vector3 prevPosition = new Vector3(0.0001f, 0, 0.0001f);

        /// <summary>
        /// Current position of main camera
        /// </summary>
        private Vector3 currentPosition;

        /// <summary>
        /// Size used to determine terrain bounds
        /// </summary>
        private Vector2 m_terrainSizeHalfVector;

        /// <summary>
        /// Whether or not the system has been initialized
        /// </summary>
        private bool m_initialized = false;

        #endregion

        #region Events
        /// <summary>
        /// Called when a new terrain loads in
        /// </summary>
        /// <param name="gridIndex">Grid index of the terrain</param>
        /// <param name="lowRes">Whether or not the terrain is low or high resolution</param>
        public delegate void NewTerrainAdded(int2 gridIndex, bool lowRes);
        public static event NewTerrainAdded OnNewTerrainAdded;
        #endregion

        /// <summary>
        /// Initialize all the data
        /// </summary>
        public void Start()
        {
            //Only setup if mode is clipmap
            if(stormVTSettings.m_stormVTMode == StormVTMode.Clipmap)
            {
                textureLoadingSystem.stormVTSettings = stormVTSettings;
                textureLoadingSystem.StormSettingsUpdated();


                PopulateTerrainTextureData();

                if (m_debugLowResTerrainData == null)
                    m_debugLowResTerrainData = new StormVTTerrainTextureData[stormVTSettings.m_lowResPoolCount];
                else
                {
                    if(m_debugLowResTerrainData.Length == 0)
                    {
                        m_debugLowResTerrainData = new StormVTTerrainTextureData[stormVTSettings.m_lowResPoolCount];
                    }
                }
                if(m_debugHighResTerrainData == null)
                    m_debugHighResTerrainData = new StormVTTerrainTextureData[stormVTSettings.m_highResPoolCount];
                else
                {
                    if(m_debugHighResTerrainData.Length == 0)
                    {
                        m_debugHighResTerrainData = new StormVTTerrainTextureData[stormVTSettings.m_highResPoolCount];
                    }
                }

                if (m_mainCamera == null)
                    m_mainCamera = Camera.main;
                m_terrainSizeHalfVector = new Vector2(stormVTTerrainGrid.m_terrainSize * 0.5f, stormVTTerrainGrid.m_terrainSize * 0.5f);



                m_initialized = true;
            }


        }

        /// <summary>
        /// Create initial memory to hold texture data in the queue. Data will be replaced once a terrain texure is loaded in
        /// </summary>
        public void PopulateTerrainTextureData()
        {
            if(m_debugLowResTerrainData == null || m_debugHighResTerrainData == null)
            {
                return;
            }

            m_lowResTerrainDataQueue.Clear();
            for(int i = 0; i < m_debugLowResTerrainData.Length; i++)
            {
                StormVTTerrainTextureData data = m_debugLowResTerrainData[i];
                if(!m_lowResTerrainDataQueue.ContainsKey(data.gridIndex))
                {
                    m_lowResTerrainDataQueue.Add(data.gridIndex, data);
                }
            }

            m_highResTerrainDataQueue.Clear();
            for (int i = 0; i < m_debugHighResTerrainData.Length; i++)
            {
                StormVTTerrainTextureData data = m_debugHighResTerrainData[i];
                if (!m_highResTerrainDataQueue.ContainsKey(data.gridIndex))
                {
                    m_highResTerrainDataQueue.Add(data.gridIndex, data);
                }
            }
        }

        /// <summary>
        /// Checks the low and high distances to see if new terrain textures should be loaded in
        /// </summary>
        public void UpdatePool()
        {
            
            if (m_initialized)
            {

                Vector2 playerPos = new Vector2(m_mainCamera.transform.position.x, m_mainCamera.transform.position.z);

                m_lowResTerrainsList.Clear();
                m_highResTerrainsList.Clear();
                for (int i = 0; i < stormVTTerrainGrid.m_gridList.Length; i++)
                {
                    //Calculate distance
                    float distance = Vector2.Distance(stormVTTerrainGrid.m_gridList[i].position + m_terrainSizeHalfVector, playerPos);
                    if (distance <= stormVTSettings.m_lowResloadingRange)
                    {
                        //Reshuffle this terrain if already in list
                        TerrainDistanceID terrainID = new TerrainDistanceID()
                        {
                            gridIndex = stormVTTerrainGrid.m_gridList[i].gridIndex,
                            distance = distance
                        };
                        m_lowResTerrainsList.Add(terrainID);
                    }

                    if (distance <= stormVTSettings.m_highResloadingRange)
                    {
                        //Reshuffle this terrain if already in list
                        TerrainDistanceID terrainID = new TerrainDistanceID()
                        {
                            gridIndex = stormVTTerrainGrid.m_gridList[i].gridIndex,
                            distance = distance
                        };
                        m_highResTerrainsList.Add(terrainID);
                    }

                }

                m_lowResTerrainsList.Sort((node1, node2) => node1.distance.CompareTo(node2.distance));
                m_highResTerrainsList.Sort((node1, node2) => node1.distance.CompareTo(node2.distance));
                for (int i = 0; i < m_lowResTerrainsList.Count; i++)
                {
                    if(i < stormVTSettings.m_lowResPoolCount)
                        AddTerrain(m_lowResTerrainsList[i].gridIndex, m_lowResTerrainDataQueue, stormVTSettings.m_lowResPoolCount, true);
                }
                for (int i = 0; i < m_highResTerrainsList.Count; i++)
                {
                    if (i < stormVTSettings.m_highResPoolCount)
                        AddTerrain(m_highResTerrainsList[i].gridIndex, m_highResTerrainDataQueue, stormVTSettings.m_highResPoolCount, false);
                }
            }
            
        }

        /// <summary>
        /// An editor side debug list of all textures loaded into both queues
        /// </summary>
        public void PopulateDebug()
        {
            int index = 0;
            foreach(var a in m_lowResTerrainDataQueue.Values)
            {
                m_debugLowResTerrainData[index] = a;
                index++;
            }

            index = 0;
            foreach (var a in m_highResTerrainDataQueue.Values)
            {
                m_debugHighResTerrainData[index] = a;
                index++;
            }
        }

        /// <summary>
        /// Returns terrain data object if it is loaded in
        /// </summary>
        /// <param name="gridIndex">Coordinate of the terrain</param>
        /// <param name="lowResOnly">Whether or not the data should be high or low resolution</param>
        /// <returns></returns>
        public StormVTTerrainTextureData GetTerrainData(int2 gridIndex, bool lowResOnly = false)
        {
            if(lowResOnly)
            {
                //Check if terrain is in low res queue
                if (m_lowResTerrainDataQueue.ContainsKey(gridIndex))
                {
                    if (m_lowResTerrainDataQueue[gridIndex].texturesLoaded)
                        return m_lowResTerrainDataQueue[gridIndex];
                }
            }
            else
            {
                //Check if terrain is in high res queue
                if (m_highResTerrainDataQueue.ContainsKey(gridIndex))
                {
                    if (m_highResTerrainDataQueue[gridIndex].texturesLoaded)
                        return m_highResTerrainDataQueue[gridIndex];
                }


                //If not, check if terrain is in low res queue
                if (m_lowResTerrainDataQueue.ContainsKey(gridIndex))
                {
                    if (m_lowResTerrainDataQueue[gridIndex].texturesLoaded)
                        return m_lowResTerrainDataQueue[gridIndex];
                }
            }
            

            return null;
        }

        /// <summary>
        /// Each fixed update, check if we need to add / remove terrains
        /// </summary>
        private void FixedUpdate()
        {
            currentPosition = m_mainCamera.transform.position;
            currentPosition.y = 0;
            if (stormVTSettings.m_stormVTMode == StormVTMode.Clipmap)
            {
                if (prevPosition != currentPosition)
                {
                    prevPosition = currentPosition;
                    UpdatePool();
                }

            }

        }

        /// <summary>
        /// If in range, add a terrain into the releveant queue, and request its texture data
        /// </summary>
        /// <param name="terrainIndex">Coordinate of the terrain</param>
        /// <param name="terrainDataQueue">Low or high resolution queue to use</param>
        /// <param name="maxPoolCount">The max amount of data we can add to the queue</param>
        /// <param name="lowRes">Whether or not the data should be high or low resolution</param>
        public void AddTerrain(int2 terrainIndex, DictionaryQueue<int2, StormVTTerrainTextureData> terrainDataQueue, int maxPoolCount, bool lowRes = false)
        {
            if (!terrainDataQueue.ContainsKey(terrainIndex))
            {
                //Create new request
                TerrainRequest[] request = GenerateTerrainRequest(terrainIndex, lowRes);

                //Remove last recently used terrainTextureData, find new spot
                if (terrainDataQueue.Count >= maxPoolCount)
                {
                    if(terrainDataQueue.Count > 0)
                    {
                        var oldData = terrainDataQueue.Dequeue();

                        //Delete old data
                        if (oldData.Value.heightmap != null)
                            DestroyImmediate(oldData.Value.heightmap);
                        if (oldData.Value.normalmap != null)
                            DestroyImmediate(oldData.Value.normalmap);
                        if (oldData.Value.splatmaps != null)
                        {
                            for (int i = 0; i < oldData.Value.splatmaps.Length; i++)
                            {
                                DestroyImmediate(oldData.Value.splatmaps[i]);
                            }
                        }
                        if (oldData.Value.holesmap != null)
                            DestroyImmediate(oldData.Value.holesmap);
                        oldData.Value.texturesLoaded = false;
                    }
                   


                }

                //Create new terrainTextureData
                StormVTTerrainTextureData terrainTextureData = new StormVTTerrainTextureData();
                terrainTextureData.gridIndex = terrainIndex;
                terrainTextureData.texturesLoaded = false;

                //Add new terrainTextureData into queue
                terrainDataQueue.Enqueue(terrainIndex, terrainTextureData);


                //Async Request textures for terrainTextureData
                string quality = lowRes ? "_LowRes" : "_HighRes";
                string id = $"{terrainIndex.x}_{terrainIndex.y}{quality}";
                textureLoadingSystem.RequestTextureLoad(id, request,
                texturesLoaded =>
                {
                    //Check if index hasn't been deleted since the async request was started
                    if (terrainDataQueue.ContainsKey(terrainIndex))
                    {
                        terrainDataQueue[terrainIndex].texturesLoaded = true;
                        terrainDataQueue[terrainIndex].lowRes = lowRes;

                        int textureIndex = 0;
                        int splatmapIndex = 0;
                        int splatmapCount = texturesLoaded.Count - 2;

                        terrainDataQueue[terrainIndex].heightScale = stormVTTerrainGrid.m_gridList[stormVTTerrainGrid.GetIndex(terrainIndex.x, terrainIndex.y)].heightScale;

                        //Create new array to store splatmaps
                        terrainDataQueue[terrainIndex].splatmaps = new Texture2D[splatmapCount];

                        //Assign textures based on type
                        foreach (TextureCallback textureCallback in texturesLoaded)
                        {
                            switch (textureCallback.type)
                            {
                                case TextureType.Height:
                                    terrainDataQueue[terrainIndex].heightmap = textureCallback.texture;
                                    break;
                                case TextureType.Normal:
                                    terrainDataQueue[terrainIndex].normalmap = textureCallback.texture;
                                    break;
                                case TextureType.Splat:
                                    terrainDataQueue[terrainIndex].splatmaps[splatmapIndex] = textureCallback.texture;
                                    splatmapIndex++;
                                    break;
                                case TextureType.Hole:
                                    terrainDataQueue[terrainIndex].holesmap = textureCallback.texture;
                                    break;
                            }
                            textureIndex++;
                        }

                        

                        PopulateDebug();

                        if (StormVTTerrainTextureManager.OnNewTerrainAdded != null)
                            StormVTTerrainTextureManager.OnNewTerrainAdded(terrainIndex, lowRes);

                    }

                });
            }

            

        
        }

        /// <summary>
        /// Return the image path of the terrain coordinate
        /// </summary>
        /// <param name="coord">Terrain coordinate</param>
        /// <param name="sourceDataType">Type of terrain source data</param>
        /// <param name="splatIndex">Index of the splatmap, if used</param>
        /// <param name="lowRes">Whether or not the data should be high or low resolution</param>
        /// <returns></returns>
        string GetImageFilePath(int2 coord, StormVTTerrainSourceData sourceDataType, int splatIndex, bool lowRes = false)
        {

            string resolution = lowRes ? "_LowRes" : "";
            string splat = sourceDataType == StormVTTerrainSourceData._Splat ? $"_{splatIndex}" : "";

            string filePath = $"{StormVTConstants.GetTileFolder()}/Terrain_{coord.x}_{coord.y}{sourceDataType.ToString()}map{splat}{resolution}.png";

            return filePath;
        }

        /// <summary>
        /// Returns a TerrainRequest that is used for texture loading
        /// </summary>
        /// <param name="coord">Coordinate of the terrain</param>
        /// <param name="lowRes">Whether or not the data should be high or low resolution</param>
        /// <returns></returns>
        TerrainRequest[] GenerateTerrainRequest(int2 coord, bool lowRes = false)
        {
            int numberOfSplats = 0;
            int index = stormVTTerrainGrid.GetIndex(coord.x, coord.y);
            if (stormVTTerrainGrid.m_gridList[index] != null)
            {
                //Determine how many times we need to loop through the terrain layers (1 per 4 layers)
                numberOfSplats = (int)math.ceil((float)stormVTTerrainGrid.m_gridList[index].terrainLayers.Count / 4.0f);
            }

            int numberOfTerrainTextures = 3 + numberOfSplats;

            TerrainRequest[] terrainData = new TerrainRequest[numberOfTerrainTextures];

            //Height texture
            terrainData[0] = new TerrainRequest()
            {
                FilePath = GetImageFilePath(coord, StormVTTerrainSourceData._Height, 0, lowRes),
                index = 0,
                TextureType = TextureType.Height
            };

            //Normal texture
            terrainData[1] = new TerrainRequest()
            {
                FilePath = GetImageFilePath(coord, StormVTTerrainSourceData._Normal, 0, lowRes),
                index = 0,
                TextureType = TextureType.Normal
            };

            //Hole texture
            terrainData[2] = new TerrainRequest()
            {
                FilePath = GetImageFilePath(coord, StormVTTerrainSourceData._Hole, 0, lowRes),
                index = 0,
                TextureType = TextureType.Hole
            };


            int splatPositionIndex = 3;
            int splatIndex = 0;
            while (splatPositionIndex < numberOfTerrainTextures)
            {
                terrainData[splatPositionIndex] = new TerrainRequest()
                {
                    FilePath = GetImageFilePath(coord, StormVTTerrainSourceData._Splat, splatIndex, lowRes),
                    index = splatIndex,
                    TextureType = TextureType.Splat
                };
                splatPositionIndex++;
                splatIndex++;
            }

            return terrainData;

        }
    }
}
