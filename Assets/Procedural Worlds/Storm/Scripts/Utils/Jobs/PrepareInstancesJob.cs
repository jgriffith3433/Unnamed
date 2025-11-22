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
    public struct PrepareInstancesJob : IJob
    {
        public NativeList<StormItemData> Output;
        [ReadOnly]
        public NativeArray<StormItemData> Input;

        public bool checkDuplicates;
        public void Execute()
        {
            for (int i = 0; i < Input.Length; ++i)
                Execute(i);
        }
        void Execute(int index)
        {
            StormItemData item = Input[index];
            if (checkDuplicates)
                for (int i = 0; i < Output.Length; ++i)
                    if (Output[i] == item)
                        return;
            Output.Add(item);
        }
    }
}