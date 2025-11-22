#if PW_STORM_ECS

using Unity.Entities;
using Unity.Mathematics;

/// Copyright 2025 Procedural Worlds Ltd
/// Written by Jonathan Williams as part of Storm

namespace ProceduralWorlds.Storm.Utils
{
    /// <summary>
    /// Component containing data for Entities to read/write at runtime
    /// (This is split off from the EntityData to minimize how much data is passed back to be written)
    /// </summary>
    public struct StormECSEntityInstanceComponent : IComponentData
    {
        /// <summary>
        /// Current Scale Multiplier for this instance
        /// </summary>
        public float ScaleMult;
    }
}

#endif