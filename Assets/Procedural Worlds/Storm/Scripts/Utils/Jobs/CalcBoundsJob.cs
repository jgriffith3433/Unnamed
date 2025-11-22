using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

/// Copyright 2023 Procedural Worlds Ltd
/// Written by Jonathan Williams as part of Storm

namespace ProceduralWorlds.Storm.Utils
{
    /// <summary>
    /// job to calc min/max values for instances
    /// </summary>
    [BurstCompile]
#if UNITY_2022_2_OR_NEWER
    [GenerateTestsForBurstCompatibility(CompileTarget = GenerateTestsForBurstCompatibilityAttribute.BurstCompatibleCompileTarget.PlayerAndEditor)]
#else
    [BurstCompatible(CompileTarget = BurstCompatibleAttribute.BurstCompatibleCompileTarget.PlayerAndEditor)]
#endif
    public struct CalcBoundsJob : IJob
    {
        [ReadOnly]
        public Bounds ItemBounds;
        [ReadOnly]
        public float2 CellSize;
        [ReadOnly]
        public NativeList<StormItemData> items;

        public NativeArray<float3x2> Output;

        public void Execute()
        {
            int numItems = items.Length;
            float3 min = Output[0].c0;
            float3 max = Output[0].c1;
            for (int i = 0; i < numItems; ++i)
            {
                GetMinMax(items[i], out float3 x, out float3 y);
                min = math.min(min, x);
                max = math.max(max, y);
            }
            Output[0] = new float3x2(min, max);
        }
        void GetMinMax(StormItemData item, out float3 min, out float3 max)
        {
            float3 translation = math.float3(item.Translation.x * CellSize.x, item.Translation.y, item.Translation.z * CellSize.y);
            float3 bMin = ItemBounds.extents * item.Scale;
            float3 bMax = -ItemBounds.extents * item.Scale;
            float3 center = translation + (float3)ItemBounds.center;
            min = center;
            max = center;
            float3 point = center + math.rotate(item.Rotation, bMin);
            min = math.min(min, point);
            max = math.max(max, point);
            point = center + math.rotate(item.Rotation, math.float3(bMin.x, bMin.y, bMax.x));
            min = math.min(min, point);
            max = math.max(max, point);
            point = center + math.rotate(item.Rotation, math.float3(bMin.x, bMax.y, bMin.x));
            min = math.min(min, point);
            max = math.max(max, point);
            point = center + math.rotate(item.Rotation, math.float3(bMin.x, bMax.y, bMax.x));
            min = math.min(min, point);
            max = math.max(max, point);
            point = center + math.rotate(item.Rotation, bMax);
            min = math.min(min, point);
            max = math.max(max, point);
            point = center + math.rotate(item.Rotation, math.float3(bMax.x, bMax.y, bMin.x));
            min = math.min(min, point);
            max = math.max(max, point);
            point = center + math.rotate(item.Rotation, math.float3(bMax.x, bMin.y, bMax.x));
            min = math.min(min, point);
            max = math.max(max, point);
            point = center + math.rotate(item.Rotation, math.float3(bMax.x, bMin.y, bMin.x));
            min = math.min(min, point);
            max = math.max(max, point);
        }

    }
    /// <summary>
    /// job to calc min/max values for sub-grids with instances
    /// </summary>
    [BurstCompile]
#if UNITY_2022_2_OR_NEWER
    [GenerateTestsForBurstCompatibility(CompileTarget = GenerateTestsForBurstCompatibilityAttribute.BurstCompatibleCompileTarget.PlayerAndEditor)]
#else
    [BurstCompatible(CompileTarget = BurstCompatibleAttribute.BurstCompatibleCompileTarget.PlayerAndEditor)]
#endif
    public struct CalcBoundsJobSubGrid : IJob
    {
        [ReadOnly]
        public NativeArray<StormSubgridData> GridCells;

        public NativeArray<float3x2> Output;

        public void Execute()
        {
            int numItems = GridCells.Length;
            float3 min = Output[0].c0;
            float3 max = Output[0].c1;
            for (int i = 0; i < numItems; ++i)
            {
                if (GridCells[i].ItemCount > 0)
                {
                    min = math.min(min, GridCells[i].Bounds.c0);
                    max = math.max(max, GridCells[i].Bounds.c1);
                }
            }
            Output[0] = new float3x2(min, max);
        }
    }
}