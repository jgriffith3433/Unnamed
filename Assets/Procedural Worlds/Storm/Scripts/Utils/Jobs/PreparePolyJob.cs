using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

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
    public struct PreparePolyJob : IJob
    {
        /// <summary>
        /// Input polygon points as a single array.
        /// </summary>
        public NativeList<float2> OutputPoints;
        /// <summary>
        /// Input polygon counts. number of points for each polygon (should add up to length of Input)
        /// </summary>
        public NativeList<int> OutputCounts;
        /// <summary>
        /// Bounding Rect of the polygon as (MinX, MinY, MaxX, MaxY)
        /// </summary>
        public NativeList<float4> OutputBounds;
        /// <summary>
        /// Value and Falloff to use for each Polygon
        /// </summary>
        public NativeList<float> OutputStrengths;
        /// <summary>
        /// Priority to use for each Polygon
        /// </summary>
        public NativeList<byte> OutputPriorities;

        /// <summary>
        /// Input polygon points as a single array.
        /// </summary>
        [ReadOnly]
        public NativeArray<float2> InputPoints;
        /// <summary>
        /// Input polygon counts. number of points for each polygon (should add up to length of Input)
        /// </summary>
        [ReadOnly]
        public NativeArray<int> InputCounts;
        /// <summary>
        /// Bounding Rect of the polygon as (MinX, MinY, MaxX, MaxY)
        /// </summary>
        [ReadOnly]
        public NativeArray<float4> InputBounds;
        /// <summary>
        /// Value and Falloff to use for each Polygon
        /// </summary>
        [ReadOnly]
        public NativeArray<float> InputStrengths;
        /// <summary>
        /// Value Range to adjust input values to fit (Input 0-1 = InputRange X-Y)
        /// </summary>
        [ReadOnly]
        public float2 InputRange;
        public byte InputPriority;

        public void Execute()
        {
            int offset = 0;
            for (int a = 0; a < InputCounts.Length; ++a)
            {
                int cntA = InputCounts[a];
                if (!HasMatch(a, offset))
                {
                    OutputCounts.Add(cntA);
                    OutputBounds.Add(InputBounds[a]);
                    OutputStrengths.Add(math.lerp(InputRange.x, InputRange.y, InputStrengths[a * 2]));
                    OutputStrengths.Add(InputStrengths[(a * 2) + 1]);
                    OutputPriorities.Add(InputPriority);
                    for (int p = 0; p < cntA; ++p)
                    {
                        OutputPoints.Add(InputPoints[offset + p]);
                    }
                }
                offset += cntA;
            }
        }
        bool HasMatch(int index, int offset)
        {
            int cntA = InputCounts[index];
            int offsetB = 0;
            for (int i = 0; i < OutputCounts.Length; ++ i)
            {
                int cntB = OutputCounts[i];
                if (cntA == cntB)
                {
                    if (InputStrengths[index * 2] == OutputStrengths[i * 2] && InputStrengths[(index * 2) + 1] == OutputStrengths[(i * 2) + 1] &&
                        math.all(InputBounds[index] == OutputBounds[i]))
                    {
                        bool allMatch = true;
                        for (int p = 0; p < cntB; ++p)
                        {
                            if (!math.all(InputPoints[offset + p] == OutputPoints[offsetB + p]))
                            {
                                allMatch = false;
                                break;
                            }
                        }
                        if (allMatch)
                            return true;
                    }
                }
                offsetB += cntB;
            }
            return false;
        }
    }
}