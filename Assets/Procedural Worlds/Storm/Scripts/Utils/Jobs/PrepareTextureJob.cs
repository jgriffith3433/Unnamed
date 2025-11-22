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
    /// Writes a set value to fill a data buffer
    /// </summary>
    [BurstCompile]
#if UNITY_2022_2_OR_NEWER
    [GenerateTestsForBurstCompatibility(CompileTarget = GenerateTestsForBurstCompatibilityAttribute.BurstCompatibleCompileTarget.Player)]
#else
    [BurstCompatible(CompileTarget = BurstCompatibleAttribute.BurstCompatibleCompileTarget.PlayerAndEditor)]
#endif
    public struct PrepareTextureJob : IJobParallelFor
    {
        public NativeArray<float> OutRArray;
        public NativeArray<float> OutGArray;
        public NativeArray<float> OutBArray;
        public NativeArray<float> OutAArray;

        [ReadOnly]
        [DeallocateOnJobCompletion]
        public NativeArray<Color> TextureData;
        [ReadOnly]
        public int2 TextureResolution;

        [ReadOnly]
        public bool HasR;
        [ReadOnly]
        public bool HasG;
        [ReadOnly]
        public bool HasB;
        [ReadOnly]
        public bool HasA;

        public void Execute(int index)
        {
            Color val = TextureData[index];
            if (HasR)
            {
                OutRArray[index] = val.r;
            }
            if (HasG)
            {
                OutGArray[index] = val.g;
            }
            if (HasB)
            {
                OutBArray[index] = val.b;
            }
            if (HasA)
            {
                OutAArray[index] = val.a;
            }
        }
    }
}