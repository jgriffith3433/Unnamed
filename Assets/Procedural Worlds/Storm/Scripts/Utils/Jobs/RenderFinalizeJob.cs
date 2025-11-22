using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

/// Copyright 2023 Procedural Worlds Ltd
/// Written by Jonathan Williams as part of Storm

namespace ProceduralWorlds.Storm.Utils
{
    /// <summary>
    /// Adds Instances to Output after transforming by an Offset, Rotation, and Scale
    /// </summary>
    [BurstCompile(FloatMode = FloatMode.Fast, FloatPrecision = FloatPrecision.Low, OptimizeFor = OptimizeFor.Performance)]
#if UNITY_2022_2_OR_NEWER
    [GenerateTestsForBurstCompatibility(CompileTarget = GenerateTestsForBurstCompatibilityAttribute.BurstCompatibleCompileTarget.PlayerAndEditor)]
#else
    [BurstCompatible(CompileTarget = BurstCompatibleAttribute.BurstCompatibleCompileTarget.PlayerAndEditor)]
#endif
    public struct RenderFinalizeJob : IJob
    {
        public float3 Offset;
        public quaternion Rotation;
        public float3 Scale;

        [ReadOnly]
        public NativeList<StormItemData> Instances;
        public NativeList<StormRenderData> Output;
        public NativeList<StormRenderData> OutputInverse;
        public void Execute()
        {
            for (int i = 0; i < Instances.Length; ++i)
            {
                StormItemData item = Instances[i];
                float3 offset = math.float3(Offset.x * item.Scale.x, Offset.y * item.Scale.y, Offset.z * item.Scale.z);
                item.Translation += math.rotate(item.Rotation, offset);
                item.Scale = math.float3(item.Scale.x * Scale.x, item.Scale.y * Scale.y, item.Scale.z * Scale.z);
                item.Rotation = math.normalizesafe(math.mul(item.Rotation, Rotation));
                bool3 scaleNeg = item.Scale < 0;
                if (scaleNeg.x ^ scaleNeg.y ^ scaleNeg.z)
                    OutputInverse.Add(new StormRenderData(item));
                else
                    Output.Add(new StormRenderData(item));
            }
        }
    }
}