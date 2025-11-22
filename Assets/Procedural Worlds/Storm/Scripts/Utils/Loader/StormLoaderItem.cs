using System;
using Unity.Mathematics;

/// Copyright 2023 Procedural Worlds Ltd
/// Written by Jonathan Williams as part of Storm

namespace ProceduralWorlds.Storm.Utils
{
    /// <summary>
    /// Holds information about a Data Item to load in the world
    /// </summary>
    [Serializable]
    public struct StormLoaderItem
    {
        /// <summary>
        /// Offset of this tile from the World Position
        /// </summary>
        public float3 Offset;
        /// <summary>
        /// Extents of this tile from Offset position
        /// </summary>
        public float3 Extents;

        /// <summary>
        /// Type of data to load for this Tile
        /// </summary>
        public string DataType;
        /// <summary>
        /// String to pass to loader for this tile
        /// </summary>
        public string Path;
    }
}