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
    public struct PrepareFloatJob : IJobParallelFor
    {
        /// <summary>
        /// Number of samples along each axis in Input map
        /// </summary>
        [ReadOnly]
        public int2 InputResolution;
        /// <summary>
        /// Priority of this Input map
        /// </summary>
        [ReadOnly]
        public byte InputPriority;
        /// <summary>
        /// Input map as 2D float array packed as [Y * resolution + x]
        /// </summary>
        [ReadOnly]
        public NativeArray<float> Input;
        /// <summary>
        /// Value Range to adjust input values to fit (Input 0-1 = InputRange X-Y)
        /// </summary>
        [ReadOnly]
        public float2 InputRange;

        /// <summary>
        /// Number of samples along each axis on Output map
        /// </summary>
        [ReadOnly]
        public int2 OutputResolution;
        /// <summary>
        /// Mask values to multiply values by when adding
        /// </summary>
        public NativeArray<byte> OutputPriorities;
        /// <summary>
        /// Output array
        /// </summary>
        public NativeArray<float> Output;

        public void Execute(int index)
        {
            if (OutputPriorities[index] > InputPriority)
                return;
            float2 UV = math.float2(index % OutputResolution.x, index / OutputResolution.x) / OutputResolution;
            float val = math.lerp(InputRange.x, InputRange.y, StormJobUtils.Sample(ref Input, ref InputResolution, ref UV));
            if (OutputPriorities[index] == InputPriority)
            {
                Output[index] = math.max(Output[index], val);
            }
            else
            {
                Output[index] = val;
                OutputPriorities[index] = InputPriority;
            }
        }
    }
}