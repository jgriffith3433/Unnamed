using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

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
    public struct MaxByteJob : IJobParallelFor
    {
        /// <summary>
        /// Priority of this Input map
        /// </summary>
        [ReadOnly]
        public byte InputValue;

        /// <summary>
        /// Mask values to multiply values by when adding
        /// </summary>
        public NativeArray<byte> Output;

        public void Execute(int index)
        {
            Output[index] = (byte)math.max((int)Output[index], InputValue);
        }
    }
}