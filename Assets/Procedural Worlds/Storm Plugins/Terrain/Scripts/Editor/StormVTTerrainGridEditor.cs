using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ProceduralWorlds.Storm.Terrain
{

    [CustomEditor(typeof(StormVTTerrainGrid))]
    public class StormVTTerrainGridEditor : Editor
    {
        /// <summary>
        /// Draw the inspector normally
        /// </summary>
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            StormVTTerrainGrid stormVTTerrainGrid = (StormVTTerrainGrid)target;

            //Add additional buttons
            if(GUILayout.Button("Update Terrain Layers"))
            {
                stormVTTerrainGrid.UpdateTerrainLayerGridData();
                stormVTTerrainGrid.SetTerrainPivot();
                stormVTTerrainGrid.UpdateTerrainIndices();
                EditorUtility.SetDirty(stormVTTerrainGrid);
            }

            if(GUILayout.Button("Update Data"))
            {
                UpdateTerrainData(stormVTTerrainGrid);
            }

            

        }

        /// <summary>
        /// Refreshes the data stored for the terrain bounds
        /// </summary>
        /// <param name="stormVTTerrainGrid">terrain grid object</param>
        public void UpdateTerrainData(StormVTTerrainGrid stormVTTerrainGrid)
        {

            stormVTTerrainGrid.UpdateTerrainLayerGridData();

            //Check for gaia loaded scene setup
            if (StormVTGaia.Instance.IsGaiaTerrainLoadedScene())
            {
                void Act(UnityEngine.Terrain t)
                {
                    if(stormVTTerrainGrid.m_terrainHeightScale < t.terrainData.heightmapScale.y)
                    {
                        stormVTTerrainGrid.m_terrainHeightScale = t.terrainData.heightmapScale.y;
                        EditorUtility.SetDirty(stormVTTerrainGrid);
                    }
                    if (stormVTTerrainGrid.m_terrainSize < t.terrainData.size.x)
                    {
                        stormVTTerrainGrid.m_terrainSize = t.terrainData.size.x;
                        EditorUtility.SetDirty(stormVTTerrainGrid);
                    }
                    if (stormVTTerrainGrid.m_heightmapResolution < t.terrainData.heightmapResolution)
                    {
                        stormVTTerrainGrid.m_heightmapResolution = t.terrainData.heightmapResolution;
                        EditorUtility.SetDirty(stormVTTerrainGrid);
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
                    if (stormVTTerrainGrid.m_terrainHeightScale < t.terrainData.heightmapScale.y)
                    {
                        stormVTTerrainGrid.m_terrainHeightScale = t.terrainData.heightmapScale.y;
                        EditorUtility.SetDirty(stormVTTerrainGrid);
                    }
                    if (stormVTTerrainGrid.m_terrainSize < t.terrainData.size.x)
                    {
                        stormVTTerrainGrid.m_terrainSize = t.terrainData.size.x;
                        EditorUtility.SetDirty(stormVTTerrainGrid);
                    }
                    if (stormVTTerrainGrid.m_heightmapResolution < t.terrainData.heightmapResolution)
                    {
                        stormVTTerrainGrid.m_heightmapResolution = t.terrainData.heightmapResolution;
                        EditorUtility.SetDirty(stormVTTerrainGrid);
                    }
                }
            }

            stormVTTerrainGrid.SetTerrainPivot();

            EditorUtility.SetDirty(stormVTTerrainGrid);

        } 

    }
}
