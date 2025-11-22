using Unity.Mathematics;

/// Copyright 2023 Procedural Worlds Ltd
/// Written by Jonathan Williams as part of Storm

namespace ProceduralWorlds.Storm.Utils
{
    /// <summary>
    /// Array used by culling job to hold location and range information
    /// </summary>
    public struct JobRenderCellData
    {
        public const ulong Size = (sizeof(int) * 2) + (sizeof(float) * 6) + sizeof(StormFeatureType);
        /// <summary>
        /// Tile index of this tile
        /// </summary>
        public int2 Index;
        /// <summary>
        /// World center Position of this tile
        /// </summary>
        public float3 Position;
        /// <summary>
        /// Extents of the bounding box of this tile (half of box size)
        /// </summary>
        public float3 Extents;
        /// <summary>
        /// Enum flags for which types are loaded on this tile
        /// </summary>
        public StormFeatureType LoadedTypes;
    }
}