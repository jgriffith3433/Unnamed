using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

/// Copyright 2023 Procedural Worlds Ltd
/// Written by Jonathan Williams as part of Storm

namespace ProceduralWorlds.Storm.Utils
{
    /// <summary>
    /// Writes a set value to fill a data buffer
    /// </summary>
    [BurstCompile]
#if UNITY_2022_2_OR_NEWER
    [GenerateTestsForBurstCompatibility(CompileTarget = GenerateTestsForBurstCompatibilityAttribute.BurstCompatibleCompileTarget.Player)]
#else
    [BurstCompatible(CompileTarget = BurstCompatibleAttribute.BurstCompatibleCompileTarget.PlayerAndEditor)]
#endif
    public struct TranslateInstancesJob : IJobParallelForBatch
    {
        public NativeArray<StormComputeItemData> Output;
        [ReadOnly]
        public NativeArray<StormItemData> Input;

        public void Execute(int startIndex, int count)
        {
            for (int i = 0; i < count; ++i)
            {
                int index = startIndex + i;
                Output[index] = Input[index];
            }
        }
    }
}