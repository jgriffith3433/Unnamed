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
    public struct PrepareTextureColorJob : IJobParallelFor
    {
        [ReadOnly]
        public NativeArray<Color32> TextureData;
        [ReadOnly]
        public int2 TextureResolution;
        [ReadOnly]
        public float4 Rect;

        [WriteOnly]
        public NativeArray<float> Output;
        [ReadOnly]
        public int2 OutputResolution;
        public float4 TargetColor;
        [ReadOnly]
        public float TargetRange;
        public float Falloff;

        public void Execute(int index)
        {
            float2 UV = math.float2(index % OutputResolution.x, index / OutputResolution.x) / (OutputResolution);
            UV = (UV * Rect.zw) + Rect.xy;
            //position on Input array to sample (Convert output UV to world space, then back into UV space for Input)
            if (UV.x < 0 || UV.x > 1 || UV.y < 0 || UV.y > 1)
            {
                Output[index] = 0;
                return; //if Input UV is out of range of input, just abort
            }

            Color color = Sample(ref TextureData, TextureResolution, UV);
            float val = 1f - ((math.distance(math.float4(color.r, color.g, color.b, color.a), TargetColor) - TargetRange) / math.max(0.00001f, Falloff));
            Output[index] = math.clamp(val, 0, 1);
        }

        Color Sample(ref NativeArray<Color32> data, int2 resolution, float2 uv)
        {
            uv *= resolution;
            if (math.any(uv < 0) || math.any(uv >= resolution))
                return Color.black;
            int2 p1 = math.int2(math.floor(uv));
            int2 p2 = math.clamp(p1 + 1, 0, resolution - 1);
            p1 = math.clamp(p1, 0, resolution - 1);
            uv = math.frac(uv);
            Color v1 = data[(p1.y * resolution.x) + p1.x];
            Color v2 = data[(p1.y * resolution.x) + p2.x];
            Color v3 = data[(p2.y * resolution.x) + p1.x];
            Color v4 = data[(p2.y * resolution.x) + p2.x];

            Color ret = Color.Lerp(Color.Lerp(v1, v2, uv.x), Color.Lerp(v3, v4, uv.x), uv.y);
            return ret;
        }
    }
}