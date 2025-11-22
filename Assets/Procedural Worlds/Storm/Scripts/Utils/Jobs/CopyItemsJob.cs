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
    public struct CopyItemsJob : IJob
    {
        [ReadOnly]
        public NativeList<StormItemData> SourceItems;
        [ReadOnly]
        public float3 SourceOffset;
        [ReadOnly]
        public float3 SourceSize;
        [ReadOnly]
        public float3 TargetOffset;
        [ReadOnly]
        public float3 TargetSize;

        public NativeList<StormItemData> TargetItems;

        /// <summary>
        /// Mask values to multiply values by when adding
        /// </summary>
        [ReadOnly]
        public NativeArray<float> Mask;
        /// <summary>
        /// Number of samples along each axis on Mask
        /// </summary>
        [ReadOnly]
        public int2 MaskResolution;

        public void Execute()
        {
            for (int i = 0; i < SourceItems.Length; ++i)
                Execute(i);
        }

        public void Execute(int index)
        {
            StormItemData item = SourceItems[index];
            item.Translation = (item.Translation * SourceSize) + SourceOffset - TargetOffset;
            if (item.Translation.x >= 0 && item.Translation.x < TargetSize.x && item.Translation.z >= 0 && item.Translation.z < TargetSize.z)
            {
                float3 pos = item.Translation;
                pos.x /= TargetSize.x;
                pos.z /= TargetSize.z;
                item.Translation = pos;

                float2 posXZ = pos.xz;

                //get mask value
                float mask = math.clamp(StormJobUtils.Sample(ref Mask, ref MaskResolution, ref posXZ), 0, 1);
                if (mask <= item.ExtraData.y)
                    return; //mask value nullifies the input so just return

                TargetItems.Add(item);
            }
        }
    }
}