using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

/// Copyright 2023 Procedural Worlds Ltd
/// Written by Jonathan Williams as part of Storm

namespace ProceduralWorlds.Storm.Utils
{
    public struct CopyToRenderBufferJob : IJob
    {
        [ReadOnly]
        public NativeList<StormRenderData> InputA;
        public NativeArray<StormRenderData> OutputA;
        [ReadOnly]
        public NativeList<StormRenderData> InputB;
        public NativeArray<StormRenderData> OutputB;
        [WriteOnly]
        public NativeList<int2> Counts;

        public void Execute()
        {
            int2 len = new int2(math.min(OutputA.Length, InputA.Length), math.min(OutputB.Length, InputB.Length));
            Counts.Add(len);
            NativeArray<StormRenderData>.Copy(InputA.AsArray(), OutputA, len.x);
            NativeArray<StormRenderData>.Copy(InputB.AsArray(), OutputB, len.y);
        }
    }
}