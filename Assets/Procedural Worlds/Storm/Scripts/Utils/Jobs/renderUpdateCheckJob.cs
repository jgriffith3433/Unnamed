using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

/// Copyright 2023 Procedural Worlds Ltd
/// Written by Jonathan Williams as part of Storm

namespace ProceduralWorlds.Storm.Utils
{
    /// <summary>
    /// Finds sections of render array to update trying to keep updates to a minimum
    /// </summary>
    [BurstCompile]
#if UNITY_2022_2_OR_NEWER
    [GenerateTestsForBurstCompatibility(CompileTarget = GenerateTestsForBurstCompatibilityAttribute.BurstCompatibleCompileTarget.PlayerAndEditor)]
#else
    [BurstCompatible(CompileTarget = BurstCompatibleAttribute.BurstCompatibleCompileTarget.PlayerAndEditor)]
#endif
    public struct renderUpdateCheckJob : IJob
    {
        /// <summary>
        /// Output update list of [StartIndex, Length] for which sections of array to update
        /// </summary>
        public NativeList<int2> UpdateArray;

        /// <summary>
        /// The index in PreviousArray that matches each item in RenderArray, -1 if  not found
        /// </summary>
        [ReadOnly]
        public NativeList<int2> IndexArray;

        /// <summary>
        /// Array of items that should be in the render array now
        /// </summary>
        public NativeList<StormRenderData> RenderArray;
        /// <summary>
        /// Array of items that were in the render array last update
        /// </summary>
        public NativeList<StormRenderData> PreviousArray;

        public void Execute()
        {
            //get the length of the index array (should match render array)
            int arrLen = IndexArray.Length;
            //if no data is in the array at all, just exit
            if (arrLen == 0)
                return;
            PreviousArray.Length = arrLen;
            //first we set up an array for which indexes need updated
            NativeArray<bool> updated = new NativeArray<bool>(arrLen, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
            for (int i = 0; i < arrLen; ++i)
                updated[i] = true;
            //go through index array and set indexes that were matches as false
            for (int i = 0; i < arrLen; ++i)
            {
                int2 idx = IndexArray[i];
                if (idx.y >= 0)
                    updated[idx.y] = false;
            }
            //go through and fill in new items into empty spots
            int copyIdx = 0;
            for (int i = 0; i < arrLen; ++i)
            {
                int2 idx = IndexArray[i];
                if (idx.y < 0)
                {
                    for (; copyIdx < arrLen; ++copyIdx)
                    {
                        if (updated[copyIdx])
                            break;
                    }
                    PreviousArray[copyIdx++] = RenderArray[idx.x];
                }
            }
            //create our update list by finding the points where updated value changes
            //also copy our new data into the proper places in RenderArray for these updates
            int startIdx = 0;
            bool lastUpdated = false;
            for (int i = 0; i < arrLen; ++i)
            {
                RenderArray[i] = PreviousArray[i];
                if (updated[i] != lastUpdated)
                {
                    if (lastUpdated) //if we were in a section to update, mark this end position
                        UpdateArray.Add(new int2(startIdx, i - startIdx));
                    lastUpdated = !lastUpdated;
                    startIdx = i;
                }
            }
            if (lastUpdated) //if we were in a section to update, mark this end position
                UpdateArray.Add(new int2(startIdx, arrLen - startIdx));
            updated.Dispose();
        }
    }
}