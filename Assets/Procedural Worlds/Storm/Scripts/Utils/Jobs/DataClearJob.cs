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
    public struct ByteDataClearJob : IJobParallelFor
    {
        /// <summary>
        /// value to set in Output buffer
        /// </summary>
        public byte ClearValue;
        /// <summary>
        /// Output array
        /// </summary>
        [WriteOnly]
        public NativeArray<byte> Output;

        public void Execute(int index)
        {
            Output[index] = ClearValue;
        }
    }
    /// <summary>
    /// Writes a set value to fill a data buffer
    /// </summary>
    [BurstCompile]
#if UNITY_2022_2_OR_NEWER
    [GenerateTestsForBurstCompatibility(CompileTarget = GenerateTestsForBurstCompatibilityAttribute.BurstCompatibleCompileTarget.Player)]
#else
    [BurstCompatible(CompileTarget = BurstCompatibleAttribute.BurstCompatibleCompileTarget.PlayerAndEditor)]
#endif
    public struct FloatDataClearJob : IJobParallelFor
    {
        /// <summary>
        /// value to set in Output buffer
        /// </summary>
        public float ClearValue;
        /// <summary>
        /// Output array
        /// </summary>
        [WriteOnly]
        public NativeArray<float> Output;

        public void Execute(int index)
        {
            Output[index] = ClearValue;
        }
    }
}