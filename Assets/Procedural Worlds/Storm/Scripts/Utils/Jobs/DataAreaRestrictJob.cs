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
    public struct DataAreaRestrictJob : IJobParallelFor
    {
        public NativeArray<float> Data;
        public int2 DataResolution;
        public float4 Area;

        public void Execute(int index)
        {
            //output uv position from 1-d index value
            float2 UV = math.float2(index % DataResolution.x, index / DataResolution.x) / (DataResolution);
            if (UV.x < Area.x || UV.y < Area.y || UV.x > Area.z || UV.y > Area.w)
                Data[index] = 0;
        }
    }
}