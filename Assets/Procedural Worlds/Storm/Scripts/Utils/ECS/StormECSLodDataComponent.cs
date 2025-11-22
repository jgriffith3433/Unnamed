#if PW_STORM_ECS

using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

/// Copyright 2025 Procedural Worlds Ltd
/// Written by Jonathan Williams as part of Storm

namespace ProceduralWorlds.Storm.Utils
{
    /// <summary>
    /// Shared Component used to contain lod data for instances
    /// </summary>
    public struct StormECSLodDataSharedComponent : ISharedComponentData
    {
        public float StartDist;
        public float EndDist;
        public float BeginScaleoutDist;
        public float EndScaleoutDist;
        public float ScaleoutMin;
    }
}

#endif