using Unity.Collections;
using Unity.Jobs;

/// Copyright 2023 Procedural Worlds Ltd
/// Written by Jonathan Williams as part of Storm

namespace ProceduralWorlds.Storm.Utils
{
    public static class StormJobExtensions
    {
#if UNITY_2022_2_OR_NEWER
        public static JobHandle Append<T>(this T jobData, NativeList<int> indices, int arrayLength, int innerloopBatchCount, JobHandle dependsOn = default) where T : struct, IJobFilter
#else
        public static JobHandle Append<T>(this T jobData, NativeList<int> indices, int arrayLength, int innerloopBatchCount, JobHandle dependsOn = default) where T : struct, IJobParallelForFilter
#endif
        {
#if UNITY_2022_1_OR_NEWER
            return jobData.ScheduleAppend(indices, arrayLength, dependsOn);
#else
            return jobData.ScheduleAppend(indices, arrayLength, innerloopBatchCount, dependsOn);
#endif
        }
    }
}