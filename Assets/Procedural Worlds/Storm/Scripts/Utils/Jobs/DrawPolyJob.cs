using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;


/// Copyright 2023 Procedural Worlds Ltd
/// Written by Jonathan Williams as part of Storm

namespace ProceduralWorlds.Storm.Utils
{
    /// <summary>
    /// Reads values from Input and copys any data within area to Output array applying a mask
    /// </summary>
    [BurstCompile]
#if UNITY_2022_2_OR_NEWER
    [GenerateTestsForBurstCompatibility(CompileTarget = GenerateTestsForBurstCompatibilityAttribute.BurstCompatibleCompileTarget.PlayerAndEditor)]
#else
    [BurstCompatible(CompileTarget = BurstCompatibleAttribute.BurstCompatibleCompileTarget.PlayerAndEditor)]
#endif
    public struct DrawPolyJob : IJobParallelFor
    {
        /// <summary>
        /// Position of Input map in world space
        /// </summary>
        [ReadOnly]
        public float2 InputPosition;
        /// <summary>
        /// Size of Input map in world space
        /// </summary>
        [ReadOnly]
        public float2 InputSize;
        /// <summary>
        /// Priority of this Input map
        /// </summary>
        [ReadOnly]
        public NativeArray<byte> InputPriority;
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
        /// Position of Output map in world space
        /// </summary>
        [ReadOnly]
        public float2 OutputPosition;
        /// <summary>
        /// Size of Output map in world space
        /// </summary>
        [ReadOnly]
        public float2 OutputSize;
        /// <summary>
        /// Number of samples along each axis on Output map
        /// </summary>
        [ReadOnly]
        public int2 OutputResolution;

        /// <summary>
        /// Output array
        /// </summary>
        public NativeArray<float> Output;
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
        /// <summary>
        /// Mask values to multiply values by when adding
        /// </summary>
        public NativeArray<byte> Priorities;

        /// <summary>
        /// Offset to apply to Sampled data prior to scaling
        /// </summary>
        [ReadOnly]
        public float DataOffset;
        /// <summary>
        /// Scale applied to Sampled data
        /// </summary>
        [ReadOnly]
        public float DataScale;

        public void Execute(int index)
        {
            //output uv position from 1-d index value
            float2 outputUV = math.float2(index % OutputResolution.x, index / OutputResolution.x) / (OutputResolution - 1);
            //position on Input array to sample (Convert output UV to world space, then back into UV space for Input)
            float2 inputUV = (OutputPosition - InputPosition + (OutputSize * outputUV)) / InputSize;
            if (inputUV.x < 0 || inputUV.x > 1 || inputUV.y < 0 || inputUV.y > 1)
                return; //if Input UV is out of range of input, just abort
            //get mask value
            float mask = math.clamp(StormJobUtils.Sample(ref Mask, ref MaskResolution, ref outputUV), 0, 1);
            if (mask == 0)
                return; //mask value nullifies the input so just return
            if (!SamplePolys(inputUV, index, out byte priority, out float val))
                return; //no poly was hit on this point.
            val = (DataOffset + (val * DataScale)) * mask;
            if (Priorities[index] == priority)
            {
                if (DataScale < 0)
                    Output[index] = math.min(Output[index], val);
                else
                    Output[index] = math.max(Output[index], val);
            }
            else
                Output[index] = val;
            Priorities[index] = priority;
        }
        bool SamplePolys(float2 inputUV, int index, out byte priority, out float val)
        {
            int numPolys = InputCounts.Length;
            int offsetIdx = 0;
            bool ret = false;
            priority = Priorities[index];
            val = 0;
            for (int polyIdx = 0; polyIdx < numPolys; ++polyIdx)
            {
                byte pri = InputPriority[polyIdx];
                if (pri < priority)
                    continue;
                float4 bounds = InputBounds[polyIdx];
                int numPoints = InputCounts[polyIdx];
                float falloff = InputStrengths[polyIdx * 2 + 1];
                if (numPoints > 1 && bounds.x <= inputUV.x + falloff && bounds.y <= inputUV.y + falloff && bounds.z >= inputUV.x - falloff && bounds.w >= inputUV.y - falloff)
                {
                    bool isInside = false;
                    float closest = float.MaxValue;
                    int lastIdx = numPoints - 1;
                    for (int pIdx = 0; pIdx < numPoints; ++pIdx)
                    {
                        float2 pointA = InputPoints[offsetIdx + lastIdx];
                        float2 pointB = InputPoints[offsetIdx + pIdx];
                        float xLine = pointB.x - pointA.x;   //C
                        float yLine = pointB.y - pointA.y;   //D
                        float xPoint = inputUV.x - pointA.x; //A
                        float yPoint = inputUV.y - pointA.y; //B

                        float cProd = (yPoint * xLine) - (xPoint * yLine);
                        /*
                        float sqLen = (xLine * xLine) + (yLine * yLine);
                        float dProd = (xPoint * xLine) + (yPoint * yLine);

                        float lineT = dProd / math.min(0.001f, sqLen);
                        float lineX;
                        float lineY;
                        if (lineT <= 0)
                        {
                            lineX = pointA.x;
                            lineY = pointA.y;
                        }
                        else if (lineT >= 1)
                        {
                            lineX = pointB.x;
                            lineY = pointB.y;
                        }
                        else
                        {
                            lineX = pointA.x + (lineT * xLine);
                            lineY = pointA.y + (lineT * yLine);
                        }
                        lineX = inputUV.x - lineX;
                        lineY = inputUV.y - lineY;
                        //*/
                        if (((cProd < 0) != (pointA.y < pointB.y)) && ((pointB.y > inputUV.y) != (pointA.y > inputUV.y)))
                        {
                            isInside = !isInside;
                        }

                        //old logic, distance base on poly point
                        //float pointDist = math.sqrt(lineX * lineX + lineY * lineY);
                        //closest = math.min(closest, pointDist);

                        //new logic, distance base on closest point of the poly line
                        float lineDist;
                        float2 lineDirection = pointB - pointA;
                        float lengthsq= math.lengthsq(lineDirection);
                        if (lengthsq == 0)
                        {
                            lineDist = math.distance(inputUV, pointA);
                        }
                        else
                        {
                            float t = math.dot(inputUV - pointA, lineDirection) / lengthsq;
                            t = math.clamp(t, 0, 1);
                            float2 closestPoint = pointA + (t * lineDirection);
                            lineDist = math.distance(inputUV, closestPoint);
                        }
                        closest = math.min(closest, lineDist);
                        lastIdx = pIdx;
                    }
                    if (isInside)
                    {
                        if (pri > priority)
                        {
                            val = 0;
                            priority = pri;
                        }
                        ret = true;
                        val = math.max(val, InputStrengths[polyIdx * 2]);
                    }
                    else if (closest < falloff)
                    {
                        ret = true;
                        float fade = closest / falloff;
                        val = math.max(val, (val * fade) + (InputStrengths[polyIdx * 2] * (1f - fade)));
                    }
                }
                offsetIdx += numPoints;
            }
            return ret;
        }
    }
}