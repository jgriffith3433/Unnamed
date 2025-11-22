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
    public struct InstanceCopyJob : IJob
    {
        [ReadOnly]
        public NativeList<StormItemData> SourceItems;

        public NativeList<StormItemData> TargetItems;

        public void Execute()
        {
            for (int i = 0; i < SourceItems.Length; ++i)
                Execute(i);
        }

        public void Execute(int index)
        {
            StormItemData item = SourceItems[index];
            TargetItems.Add(item);
        }
    }
}