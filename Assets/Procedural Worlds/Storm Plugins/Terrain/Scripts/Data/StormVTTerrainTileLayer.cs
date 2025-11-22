using System;
using Unity.Mathematics;
using UnityEngine;

namespace ProceduralWorlds.Storm.Terrain
{
    [Serializable]
    public class StormVTTerrainTileLayer
    {
        /// <summary>
        /// Normal strength of the terrain layer
        /// </summary>
        [SerializeField] public float normalStrength;

        /// <summary>
        /// Smoothness value of the terrain layer
        /// </summary>
        [SerializeField] public float smoothness;

        /// <summary>
        /// Mask Map remap minimum of the terrain layer
        /// </summary>
        [SerializeField] public float4 remapMin;

        /// <summary>
        /// Mask Map remap maximum of the terrain layer
        /// </summary>
        [SerializeField] public float4 remapMax;

        /// <summary>
        /// Tiling offset of the terrain layer
        /// </summary>
        [SerializeField] public float4 tilingOffset;

        /// <summary>
        /// Color tint of the terrain layer
        /// </summary>
        [SerializeField] public Color color;

    }

}

