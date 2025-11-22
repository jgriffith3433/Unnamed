using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

/// Copyright 2023 Procedural Worlds Ltd
/// Written by Jonathan Williams as part of Storm

namespace ProceduralWorlds.Storm.Utils
{
    /// <summary>
    /// iterates through Source list, adding all items within target area to TargetItems list
    /// </summary>
    [BurstCompile]
#if UNITY_2022_2_OR_NEWER
    [GenerateTestsForBurstCompatibility(CompileTarget = GenerateTestsForBurstCompatibilityAttribute.BurstCompatibleCompileTarget.PlayerAndEditor)]
#else
    [BurstCompatible(CompileTarget = BurstCompatibleAttribute.BurstCompatibleCompileTarget.PlayerAndEditor)]
#endif
    public struct InstanceRestrictedCopyJob : IJob
    {
        [ReadOnly]
        public NativeArray<StormItemData> SourceItems;
        [ReadOnly]
        public float4 SourceArea;
        [ReadOnly]
        public float TerrainSizeY;
        [ReadOnly]
        public float3 TileOffset;
        [ReadOnly]
        public float3 TileSize;

        public NativeList<StormItemData> TargetItems;

        public void Execute()
        {
            for (int i = 0; i < SourceItems.Length; ++i)
                Execute(i);
        }

        public void Execute(int index)
        {
            StormItemData item = SourceItems[index];
            float3 pos = item.Translation;

            if (pos.x < SourceArea.x || pos.x > (SourceArea.x + SourceArea.z) || pos.z < SourceArea.y || pos.z > (SourceArea.y + SourceArea.w))
                return;
            pos.x = (pos.x - SourceArea.x) / SourceArea.z;
            pos.z = (pos.z - SourceArea.y) / SourceArea.w;
            pos.y = ((pos.y * TerrainSizeY) - TileOffset.y) / TileSize.y;
            item.Translation = pos;
            TargetItems.Add(item);
        }
    }
}