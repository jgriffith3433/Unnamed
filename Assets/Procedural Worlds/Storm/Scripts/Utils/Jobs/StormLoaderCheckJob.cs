using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

/// Copyright 2023 Procedural Worlds Ltd
/// Written by Jonathan Williams as part of Storm

namespace ProceduralWorlds.Storm.Utils
{
    /// <summary>
    /// Reads values from Input and copys any data within area to Output array applying a mask
    /// </summary>
    [BurstCompile]
#if UNITY_2022_2_OR_NEWER
    [GenerateTestsForBurstCompatibility(CompileTarget = GenerateTestsForBurstCompatibilityAttribute.BurstCompatibleCompileTarget.PlayerAndEditor)]
#else
    [BurstCompatible(CompileTarget = BurstCompatibleAttribute.BurstCompatibleCompileTarget.PlayerAndEditor)]
#endif
    public struct StormLoaderCheckJob : IJobParallelFor
    {
        /// <summary>
        /// Position of Camera in world space
        /// </summary>
        public float3 CameraPosition;

        /// <summary>
        /// Array of Tiles to check for Load/Unload
        /// </summary>
        [ReadOnly] public NativeArray<StormLoaderTileData> Tiles;
        /// <summary>
        /// Array of ranges to load Tile types at
        /// </summary>
        [ReadOnly] public NativeArray<float> LoadRanges;
        /// <summary>
        /// Array of ranges to unload Tile types at
        /// </summary>
        [ReadOnly] public NativeArray<float> UnloadRanges;

        /// <summary>
        /// Output List of Tile indexes to Load
        /// </summary>
        [WriteOnly] public NativeList<int>.ParallelWriter ToLoad;
        /// <summary>
        /// Output List of Tile indexes to Unload
        /// </summary>
        [WriteOnly] public NativeList<int>.ParallelWriter ToUnload;
        public void Execute(int index)
        {
            StormLoaderTileData tile = Tiles[index];
            float dist = math.distancesq(CameraPosition, math.clamp(CameraPosition, tile.Position, tile.Position + tile.Size));
            if (tile.IsLoaded)
            {
                if (dist >= UnloadRanges[tile.TypeIndex])
                {
                    ToUnload.AddNoResize(index);
                }
            }
            else
            {
                if (dist <= LoadRanges[tile.TypeIndex])
                {
                    ToLoad.AddNoResize(index);
                }
            }
        }
    }
}