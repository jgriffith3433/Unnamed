using Unity.Mathematics;

/// Copyright 2025 Procedural Worlds Ltd
/// Written by Jonathan Williams as part of Storm

namespace ProceduralWorlds.Storm.Utils
{
    /// <summary>
    /// Contains grid cell information including item count and bounds
    /// </summary>
    public struct StormSubgridData
    {
        public static int Size = 28; // 4bits * 7;

        public int ItemCount;
        public float3x2 Bounds;
    }
}