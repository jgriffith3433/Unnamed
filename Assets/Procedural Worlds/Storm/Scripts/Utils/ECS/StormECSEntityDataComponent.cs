#if PW_STORM_ECS

using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

/// Copyright 2025 Procedural Worlds Ltd
/// Written by Jonathan Williams as part of Storm

namespace ProceduralWorlds.Storm.Utils
{
    /// <summary>
    /// Component for each Entity that contains the instance data needed during updates
    /// </summary>
    public struct StormECSEntityDataComponent : IComponentData
    {
        /// <summary>
        /// Offset of this instance from world center
        /// </summary>
        public float3 Translation;
        /// <summary>
        /// Rotation of this instance
        /// </summary>
        public quaternion Rotation;
        /// <summary>
        /// Base scale of this instance
        /// </summary>
        public float3 Scale;
    }
}

#endif