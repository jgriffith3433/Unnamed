using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

/// Copyright 2025 Procedural Worlds Ltd
/// Written by Jonathan Williams as part of Storm

namespace ProceduralWorlds.Storm.Utils
{
    /// <summary>
    /// Processes a Texture and gathers the most common colors and groups similar colors based on a given color range
    /// </summary>
    [BurstCompile]
#if UNITY_2022_2_OR_NEWER
    [GenerateTestsForBurstCompatibility(CompileTarget = GenerateTestsForBurstCompatibilityAttribute.BurstCompatibleCompileTarget.PlayerAndEditor)]
#else
    [BurstCompatible(CompileTarget = BurstCompatibleAttribute.BurstCompatibleCompileTarget.PlayerAndEditor)]
#endif
    public struct ProccessTextureColorJob : IJob
    {
        [ReadOnly]
        public NativeArray<Color32> Pixels;
        public NativeList<ColorRangeData> Output;
        public float ColorRange;
        public float ColorFalloff;

        int ColorToInt(Color32 c) =>
            (c.r << 24) |
            (c.g << 16) |
            (c.b << 8) |
            c.a;
        Color32 IntToColor(int v) => new Color32(
                (byte)((v >> 24) & 0xFF),
                (byte)((v >> 16) & 0xFF),
                (byte)((v >> 8) & 0xFF),
                (byte)(v & 0xFF));

        public void Execute()
        {
            NativeHashMap<int, int> colorCounts = new NativeHashMap<int, int>(Pixels.Length, Allocator.Temp);
            for (int i = 0; i < Pixels.Length; i++)
            {
                int pixel = ColorToInt(Pixels[i]);
                if (!colorCounts.ContainsKey(pixel))
                    colorCounts.Add(pixel, 1);
                else
                    colorCounts[pixel]++;
            }

            NativeArray<int> mostCommonColors = new NativeArray<int>(colorCounts.Count, Allocator.Temp);
            NativeArray<int> mostCommonCounts = new NativeArray<int>(colorCounts.Count, Allocator.Temp);
            int commonIndex = 0;
            foreach (var color in colorCounts)
            {
                mostCommonColors[commonIndex] = color.Key;
                mostCommonCounts[commonIndex] = color.Value;
                commonIndex++;
            }
            colorCounts.Dispose();

            //sort
            for (int i = 0; i < mostCommonColors.Length - 1; i++)
            {
                for (int j = 0; j < mostCommonColors.Length - 1 - i; j++)
                {
                    if (mostCommonCounts[j] < mostCommonCounts[j + 1])
                    {
                        int temp = mostCommonCounts[j];
                        mostCommonCounts[j] = mostCommonCounts[j + 1];
                        mostCommonCounts[j + 1] = temp;

                        temp = mostCommonColors[j];
                        mostCommonColors[j] = mostCommonColors[j + 1];
                        mostCommonColors[j + 1] = temp;
                    }
                }
            }

            //create output
            NativeHashSet<int> usedColors = new NativeHashSet<int>(mostCommonColors.Length, Allocator.Temp);
            for (int i = 0; i < mostCommonColors.Length; i++)
            {
                if (usedColors.Contains(mostCommonColors[i]))
                    continue;

                usedColors.Add(mostCommonColors[i]);
                Color32 baseColor = IntToColor(mostCommonColors[i]);
                float maxRange = 0f;

                for (int j = i + 1; j < mostCommonColors.Length; j++)
                {
                    if (usedColors.Contains(mostCommonColors[j]))
                        continue;

                    float distance = ColorDistance(baseColor, IntToColor(mostCommonColors[j]));
                    if (distance <= ColorRange)
                    {
                        usedColors.Add(mostCommonColors[j]);
                        maxRange = Mathf.Max(maxRange, distance);
                    }
                }

                Output.Add(new ColorRangeData(baseColor, maxRange, ColorFalloff));
            }

            mostCommonColors.Dispose();
            mostCommonCounts.Dispose();
            usedColors.Dispose();
        }

        private float ColorDistance(Color32 a, Color32 b)
        {
            return math.length(new float4(a.r - b.r, a.g - b.g, a.b - b.b, a.a - b.a) / 255f);
        }
    }
}