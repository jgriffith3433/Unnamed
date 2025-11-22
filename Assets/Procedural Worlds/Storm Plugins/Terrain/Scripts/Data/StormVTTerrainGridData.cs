using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace ProceduralWorlds.Storm.Terrain
{

    [Serializable]
    public class StormVTTerrainGridData
    {
        /// <summary>
        /// Position of this terrain in world space
        /// </summary>
        [SerializeField] public Vector2 position;

        /// <summary>
        /// Grid index of this terrain
        /// </summary>
        [SerializeField] public int2 gridIndex;

        /// <summary>
        /// List of this terrain's Unity Terrain Layers
        /// </summary>
        [SerializeReference] public List<TerrainLayer> terrainLayers;

        /// <summary>
        /// Array of this terrain's Storm Terrain Tile Layers
        /// </summary>
        [SerializeReference] public StormVTTerrainTileLayer[] terrainTileLayers;

        /// <summary>
        /// Remapped indices of the terrain layers from global indices
        /// </summary>
        [SerializeReference] public int[] terrainLayerIndices;

        /// <summary>
        /// Height scale of this terrain
        /// </summary>
        [SerializeReference] public float heightScale;

        /// <summary>
        /// Tracks whether the heightmap has changed on this terrain, used for saving out changed data
        /// </summary>
        [SerializeField] public bool heightmapChanged;

        /// <summary>
        /// Tracks whether the splat textures have changed on this terrain, used for saving out changed data
        /// </summary>
        [SerializeField] public bool texturesChanged;
    }
}
