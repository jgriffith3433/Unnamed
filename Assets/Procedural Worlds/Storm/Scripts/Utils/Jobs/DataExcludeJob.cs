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
    public struct DataExcludeJob : IJobParallelFor
    {
        public NativeArray<float> Data;
        public int2 DataResolution;
        [ReadOnly]
        public NativeArray<float> Exclusion;
        public int2 ExclusionResolution;

        public void Execute(int index)
        {
            //output uv position from 1-d index value
            float2 UV = math.float2(index % DataResolution.x, index / DataResolution.x) / (DataResolution);
            //position on Input array to sample (Convert output UV to world space, then back into UV space for Input)
            if (UV.x < 0 || UV.x > 1 || UV.y < 0 || UV.y > 1)
                return; //if Input UV is out of range of input, just abort
            float e = math.clamp(StormJobUtils.Sample(ref Exclusion, ref ExclusionResolution, ref UV), 0, 1);
            Data[index] = Data[index] * e;
        }
    }
}