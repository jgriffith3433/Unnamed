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
    public struct DataCopyJobBool : IJobParallelFor
    {
        /// <summary>
        /// Position of Input map in world space
        /// </summary>
        [ReadOnly]
        public float2 InputPosition;
        /// <summary>
        /// Size of Input map in world space
        /// </summary>
        [ReadOnly]
        public float2 InputSize;
        /// <summary>
        /// Number of samples along each axis in Input map
        /// </summary>
        [ReadOnly]
        public int2 InputResolution;
        /// <summary>
        /// Input map as 2D float array packed as [Y * resolution + x]
        /// </summary>
        [ReadOnly]
        public NativeArray<bool> Input;
        /// <summary>
        /// Value to interpret True as
        /// </summary>
        public float TrueValue;
        /// <summary>
        /// Value to interpret False as
        /// </summary>
        public float FalseValue;

        /// <summary>
        /// Position of Output map in world space
        /// </summary>
        [ReadOnly]
        public float2 OutputPosition;
        /// <summary>
        /// Size of Output map in world space
        /// </summary>
        [ReadOnly]
        public float2 OutputSize;
        /// <summary>
        /// Number of samples along each axis on Output map
        /// </summary>
        [ReadOnly]
        public int2 OutputResolution;

        /// <summary>
        /// Output array
        /// </summary>
        public NativeArray<float> Output;

        /// <summary>
        /// Offset to apply to Sampled data prior to scaling
        /// </summary>
        [ReadOnly]
        public float DataOffset;
        /// <summary>
        /// Scale applied to Sampled data
        /// </summary>
        [ReadOnly]
        public float DataScale;
        [ReadOnly]
        public float2 PixelOffset;

        public void Execute(int index)
        {
            //output uv position from 1-d index value
            float2 outputUV = math.float2(index % OutputResolution.x, index / OutputResolution.x) / (OutputResolution);
            //position on Input array to sample (Convert output UV to world space, then back into UV space for Input)
            float2 inputUV = ((OutputPosition - InputPosition) + (OutputSize * outputUV)) / InputSize;
            if (inputUV.x < 0 || inputUV.x > 1 || inputUV.y < 0 || inputUV.y > 1)
                return; //if Input UV is out of range of input, just abort
            float val = DataOffset + (StormJobUtils.Sample(ref Input, ref InputResolution, ref inputUV, TrueValue, FalseValue) * DataScale);
            if (DataScale < 0)
                Output[index] = math.min(Output[index], val);
            else
                Output[index] = math.max(Output[index], val);
        }
    }
}