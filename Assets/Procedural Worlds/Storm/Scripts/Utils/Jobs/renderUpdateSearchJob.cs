using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

/// Copyright 2023 Procedural Worlds Ltd
/// Written by Jonathan Williams as part of Storm

namespace ProceduralWorlds.Storm.Utils
{
    /// <summary>
    /// Searches through last render array and finds index of matching instance
    /// </summary>
    [BurstCompile]
#if UNITY_2022_2_OR_NEWER
    [GenerateTestsForBurstCompatibility(CompileTarget = GenerateTestsForBurstCompatibilityAttribute.BurstCompatibleCompileTarget.PlayerAndEditor)]
#else
    [BurstCompatible(CompileTarget = BurstCompatibleAttribute.BurstCompatibleCompileTarget.PlayerAndEditor)]
#endif
    public struct renderUpdateSearchJob : IJobParallelForDefer
    {
        /// <summary>
        /// The index in PreviousArray that matches each item in RenderArray, -1 if  not found
        /// </summary>
        [WriteOnly]
        public NativeList<int2>.ParallelWriter IndexArray;
        /// <summary>
        /// Array of items that should be in the render array now
        /// </summary>
        [ReadOnly]
        public NativeList<StormRenderData> RenderArray;
        /// <summary>
        /// Array of items that were in the render array last update
        /// </summary>
        [ReadOnly]
        public NativeList<StormRenderData> PreviousArray;
        public void Execute(int index) {
            if (RenderArray.Length <= index)
                return;
            StormRenderData data = RenderArray[index];
            int len = math.min(RenderArray.Length, PreviousArray.Length); //limit our search to what would be within the new length
            int ret = -1;
            for(int x = 0; x < len; ++x)
            {
                if (PreviousArray[x] == data)
                {
                    ret = x;
                    break;
                }
            }
            IndexArray.AddNoResize(new (index, ret));
        }
    }
}