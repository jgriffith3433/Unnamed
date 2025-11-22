using Unity.Mathematics;

/// Copyright 2023 Procedural Worlds Ltd
/// Written by Jonathan Williams as part of Storm

namespace ProceduralWorlds.Storm.Utils
{
    /// <summary>
    /// Contains data used for World Loader Tiles in Jobs
    /// </summary>
    public struct StormLoaderTileData
    {
        public float3 Position;
        public float3 Size;
        public int TypeIndex;
        public bool IsLoaded;
    }
}