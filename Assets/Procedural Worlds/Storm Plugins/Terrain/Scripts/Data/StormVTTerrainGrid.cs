using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ProceduralWorlds.Storm.Terrain
{

    [CreateAssetMenu(fileName = "StormVTTerrainGrid", menuName = "Procedural Worlds/Storm/Terrain/StormVTTerrainGrid",
        order = 2)]
    [Serializable]
    public class StormVTTerrainGrid : ScriptableObject
    {

        /// <summary>
        /// X Dimension of the Terrain Grid
        /// </summary>
        public int m_numTerrainsX;

        /// <summary>
        /// Y Dimension of the Terrain Grid
        /// </summary>
        public int m_numTerrainsY;

        /// <summary>
        /// Maximum height scale of the terrains used in the scene
        /// </summary>
        public float m_terrainHeightScale;

        /// <summary>
        /// Maximum height offset of the terrain used in the scene
        /// </summary>
        public float m_terrainHeightOffset;

        /// <summary>
        /// The center position of the total bounds of all terrains in the scene
        /// </summary>
        public Vector2 m_terrainBoundsCenter;

        /// <summary>
        /// The total size of all terrains in the scene
        /// </summary>
        public Vector2 m_terrainBoundsSize;

        /// <summary>
        /// The maximum size of each terrain in the scene (e.g. 1000 meters, or 1024 meters)
        /// </summary>
        public float m_terrainSize;

        /// <summary>
        /// The resolution of the heightmaps used on each terrain in the scene (e.g. 1025, 2049)
        /// </summary>
        public int m_heightmapResolution;

        /// <summary>
        /// The bottom left position of the minimum terrain in the scene
        /// </summary>
        public Vector2 m_terrainBottomLeftPosition;
        
        /// <summary>
        /// 1D array of terrain grid data
        /// </summary>
        [SerializeField] public StormVTTerrainGridData[] m_gridList;

        /// <summary>
        /// A list of the all normal map file paths
        /// </summary>
        [SerializeField] public string[] m_normalmapFilePaths;

        /// <summary>
        /// Sets the global shader variables of total terrain bounds info
        /// </summary>
        public void SetGlobalTerrainBoundsInfo()
        {
            Vector4 heightVector = Shader.GetGlobalVector(StormVTShaderIDs.m_stormVT_TerrainHeightScale);
            heightVector.x = m_terrainHeightScale * 2;
            heightVector.y = m_terrainHeightOffset;
            Shader.SetGlobalVector(StormVTShaderIDs.m_stormVT_TerrainHeightScale, heightVector);
            Shader.SetGlobalVector(StormVTShaderIDs.m_stormVT_TotalTerrainBounds, new Vector4(m_terrainBoundsCenter.x, m_terrainBoundsCenter.y, m_terrainBoundsSize.x, m_terrainBoundsSize.y));
        }

        /// <summary>
        /// Returs a 1 dimension index into an array from a 2D grid array
        /// </summary>
        /// <param name="x">X index of the array</param>
        /// <param name="y">Y index of the array</param>
        /// <returns></returns>
        public int GetIndex(int x, int y)
        {
            return x * m_numTerrainsY + y;
        }

        /// <summary>
        /// Populates data from the TerrainLayer list into the StormVTTerrainTileLayer data
        /// </summary>
        /// <param name="terrainLayers">Unity Terrain Layer list</param>
        /// <param name="terrainSize">Size of the terrain</param>
        /// <param name="terrainName">Name of the terrain</param>
        /// <returns></returns>
        public static StormVTTerrainTileLayer[] SetTerrainTileLayers(List<TerrainLayer> terrainLayers, float terrainSize, string terrainName = "")
        {
            StormVTTerrainTileLayer[] terrainTileLayers = new StormVTTerrainTileLayer[terrainLayers.Count];

            for(int i = 0; i < terrainLayers.Count; i++)
            {
                if (terrainLayers[i] == null)
                {
                    Debug.LogWarning($"Terrain layer from terrain {terrainName} on index {i} is null! Please review the terrain texture layers on this terrain.");

                    terrainTileLayers[i] = new StormVTTerrainTileLayer()
                    {
                        normalStrength = 1,
                        smoothness = 1,
                        remapMax = 1,
                        remapMin = 0,
                        tilingOffset = new Vector4(
                        terrainSize / 8,
                        terrainSize / 8,
                        0,
                        0),
                        color = Color.white,
                    };
                    continue;
                }

                terrainTileLayers[i] = new StormVTTerrainTileLayer()
                {
                    normalStrength = terrainLayers[i].normalScale,
                    smoothness = terrainLayers[i].smoothness,
                    remapMax = terrainLayers[i].maskMapRemapMax,
                    remapMin = terrainLayers[i].maskMapRemapMin,
                    tilingOffset = new Vector4(
                        terrainSize / terrainLayers[i].tileSize.x,
                        terrainSize / terrainLayers[i].tileSize.y,
                        terrainLayers[i].tileOffset.x,
                        terrainLayers[i].tileOffset.y),
                    color = terrainLayers[i].diffuseRemapMax
                };
            }

            return terrainTileLayers;
        }

        /// <summary>
        /// Updates the terrains layer information
        /// </summary>
        /// <param name="targetTerrain"></param>
        public void UpdateTerrainLayerGridData(UnityEngine.Terrain targetTerrain = null)
        {
#if UNITY_EDITOR
            
            if (targetTerrain != null)
            {
                //if a target terrain is passed in, we will only look at that specific entry in the terrain grid list
                StormVTTerrainInfo stormVTTerrainInfo = targetTerrain.GetComponent<StormVTTerrainInfo>();

                if (stormVTTerrainInfo == null)
                {
                    return;
                }

                int targetIndex = GetIndex(stormVTTerrainInfo.terrainGridIndex.x, stormVTTerrainInfo.terrainGridIndex.y);

                StormVTTerrainGridData targetGrid = m_gridList[targetIndex];
                if (targetGrid != null)
                { 
                    targetGrid.terrainLayers = targetTerrain.terrainData.terrainLayers.ToList();
                    targetGrid.terrainTileLayers = SetTerrainTileLayers(targetGrid.terrainLayers, m_terrainSize, targetTerrain.terrainData.name);
                }
            }
            else
            {
                StormVTTerrainDataContainer stormVTTerrainDataContainer = FetchTerrainDataContainer();
                if (stormVTTerrainDataContainer != null)
                {
                    foreach (var gridTerrain in m_gridList)
                    {
                        int index = GetIndex(gridTerrain.gridIndex.x, gridTerrain.gridIndex.y);

                        string assetPath = AssetDatabase.GUIDToAssetPath(stormVTTerrainDataContainer.m_terrainDataList[index]);
                        TerrainData terrainData = AssetDatabase.LoadAssetAtPath<TerrainData>(assetPath);

                        gridTerrain.terrainLayers = terrainData.terrainLayers.ToList();
                        gridTerrain.terrainTileLayers = SetTerrainTileLayers(gridTerrain.terrainLayers, m_terrainSize, terrainData.name);

                        terrainData = null;
                        Resources.UnloadUnusedAssets();
                    }
                    stormVTTerrainDataContainer = null;
                    DestroyImmediate(stormVTTerrainDataContainer);
                }

            }
            
#endif
        }

#if UNITY_EDITOR

        /// <summary>
        /// Retrieves the terrain data container object
        /// </summary>
        /// <returns></returns>
        public StormVTTerrainDataContainer FetchTerrainDataContainer()
        {

            //grid asset not loaded, try to load it in
            Scene activeScene = SceneManager.GetActiveScene();
            string vtAssetFilePath = $"{activeScene.name}_TerrainDataContainer.asset";
            string vtGridFilePath = $"{StormVTConstants.GetAssetsFolder()}/{vtAssetFilePath}";

            //Load asset at the file path
            StormVTTerrainDataContainer stormVTTerrainDataContainer =
                (StormVTTerrainDataContainer)AssetDatabase.LoadAssetAtPath(vtGridFilePath, typeof(StormVTTerrainDataContainer));

            return stormVTTerrainDataContainer;

        }
#endif

#if UNITY_EDITOR
        /// <summary>
        /// Retrieves the terrain texture layers object
        /// </summary>
        /// <returns></returns>
        public StormVTTerrainTextureLayers FetchTerrainLayers()
        {
            Scene activeScene = SceneManager.GetActiveScene();
            string vtAssetFilePath = $"{activeScene.name}_TerrainTextureLayers.asset";
            string vtTerrainLayersFilePath = $"{StormVTConstants.GetAssetsFolder()}/{vtAssetFilePath}";


            StormVTTerrainTextureLayers stormVTTerrainTextureLayers = (StormVTTerrainTextureLayers)AssetDatabase.LoadAssetAtPath(vtTerrainLayersFilePath, typeof(StormVTTerrainTextureLayers));
                
            return stormVTTerrainTextureLayers;
        }
#endif

        /// <summary>
        /// Checks to see if new terrain layers have been added to this terrain since it's last update
        /// And assigns the new layer if it is not present
        /// </summary>
        /// <param name="targetTerrain">The terrain we want to update</param>
        /// <param name="stormVTTerrainTextureLayers">The existing list of terrain texutre layers</param>
        public void UpdateTerrainIndices(UnityEngine.Terrain targetTerrain = null, StormVTTerrainTextureLayers stormVTTerrainTextureLayers = null)
        {
#if UNITY_EDITOR

            if (stormVTTerrainTextureLayers == null)
            {
                stormVTTerrainTextureLayers = FetchTerrainLayers();
            }

            if (targetTerrain != null)
            {
                //if a target terrain is passed in, we will only look at that specific entry in the terrain grid list
                StormVTTerrainInfo stormVTTerrainInfo = targetTerrain.GetComponent<StormVTTerrainInfo>();  

                if(stormVTTerrainInfo == null) 
                {
                    return;
                }

                int targetIndex = GetIndex(stormVTTerrainInfo.terrainGridIndex.x, stormVTTerrainInfo.terrainGridIndex.y);

                StormVTTerrainGridData targetGrid = m_gridList[targetIndex];
                if (targetGrid != null)
                {
                    targetGrid.terrainLayers = targetTerrain.terrainData.terrainLayers.ToList();
                    targetGrid.terrainLayerIndices = new int[targetGrid.terrainLayers.Count];
                    for (int i = 0; i < targetGrid.terrainLayers.Count; i++)
                    {
                        for (int j = 0; j < stormVTTerrainTextureLayers.stormVTTerrainTextureLayers.Count; j++)
                        {
                            if (stormVTTerrainTextureLayers.stormVTTerrainTextureLayers[j].m_terrainLayerReference == targetGrid.terrainLayers[i])
                            {
                                targetGrid.terrainLayerIndices[i] = j;
                                break;
                            }
                        }
                    }
                }

            }
            else
            {
                StormVTTerrainDataContainer stormVTTerrainDataContainer = FetchTerrainDataContainer();

                if (stormVTTerrainDataContainer != null)
                {
                    foreach (var gridTerrain in m_gridList)
                    {
                        int index = GetIndex(gridTerrain.gridIndex.x, gridTerrain.gridIndex.y);

                        string assetPath = AssetDatabase.GUIDToAssetPath(stormVTTerrainDataContainer.m_terrainDataList[index]);
                        TerrainData terrainData = AssetDatabase.LoadAssetAtPath<TerrainData>(assetPath);

                        gridTerrain.terrainLayers = terrainData.terrainLayers.ToList();
                        gridTerrain.terrainLayerIndices = new int[gridTerrain.terrainLayers.Count];
                        for (int i = 0; i < gridTerrain.terrainLayers.Count; i++)
                        {
                            for (int j = 0; j < stormVTTerrainTextureLayers.stormVTTerrainTextureLayers.Count; j++)
                            {
                                if (stormVTTerrainTextureLayers.stormVTTerrainTextureLayers[j].m_terrainLayerReference == gridTerrain.terrainLayers[i])
                                {
                                    gridTerrain.terrainLayerIndices[i] = j;
                                    break;
                                }
                            }
                        }

                        terrainData = null;
                        Resources.UnloadUnusedAssets();
                    }
                }
            }
            
#endif
        }

        /// <summary>
        /// Update the bottom left position of the terrain
        /// </summary>
        public void SetTerrainPivot()
        {
            m_terrainBottomLeftPosition = m_terrainBoundsCenter - new Vector2(m_terrainBoundsSize.x * 0.5f, m_terrainBoundsSize.y * 0.5f);

            Vector2 relativeBottomLeftPos = new Vector2(
                m_terrainBottomLeftPosition.x / m_terrainSize,
                m_terrainBottomLeftPosition.y / m_terrainSize);

        }


    }
}
