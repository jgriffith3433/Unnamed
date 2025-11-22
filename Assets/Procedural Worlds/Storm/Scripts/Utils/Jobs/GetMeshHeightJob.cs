using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;

/// Copyright 2025 Procedural Worlds Ltd
/// Written by Jonathan Williams as part of Storm

namespace ProceduralWorlds.Storm.Utils
{
    [BurstCompile(FloatPrecision.Standard, FloatMode.Fast, OptimizeFor = OptimizeFor.Performance)]
#if UNITY_2022_2_OR_NEWER
    [GenerateTestsForBurstCompatibility(CompileTarget = GenerateTestsForBurstCompatibilityAttribute.BurstCompatibleCompileTarget.PlayerAndEditor)]
#else
    [BurstCompatible(CompileTarget = BurstCompatibleAttribute.BurstCompatibleCompileTarget.PlayerAndEditor)]
#endif
    public struct GetMeshHeightJob : IJob
    {
        [ReadOnly] public NativeArray<float3> meshVertici;
        [ReadOnly] public NativeArray<int> meshTriangles;
        [ReadOnly] public float3x2 meshBounds;
        [ReadOnly] public int2 OutputResolution;
        public NativeArray<float> OutputHeights;

        public void Execute()
        {
            int width = OutputResolution.x;
            int height = OutputResolution.y;
            int pixelCount = width * height;

            // Initialize output heights to minimum value
            for (int i = 0; i < pixelCount; i++)
                OutputHeights[i] = 0f;

            int triangleCount = meshTriangles.Length / 3;
            float minX = meshBounds.c0.x;
            float maxX = meshBounds.c1.x;
            float minZ = meshBounds.c0.z;
            float maxZ = meshBounds.c1.z;

            float dx = (maxX - minX) / (width - 1);
            float dz = (maxZ - minZ) / (height - 1);
            float minY = meshBounds.c0.y;
            float maxY = meshBounds.c1.y;
            float yRange = maxY - minY;
            float yMod = 1f / yRange;

            float px0 = minX * dx;
            float pz0 = minZ * dz;

            for (int t = 0; t < triangleCount; t++)
            {
                float3 v1 = meshVertici[meshTriangles[t * 3]];
                float3 v2 = meshVertici[meshTriangles[t * 3 + 1]];
                float3 v3 = meshVertici[meshTriangles[t * 3 + 2]];

                // Project triangle to XZ plane
                float2 v1xz = new float2(v1.x, v1.z);
                float2 v2xz = new float2(v2.x, v2.z);
                float2 v3xz = new float2(v3.x, v3.z);

                // Find bounding box in grid space
                float triMinX = math.min(math.min(v1xz.x, v2xz.x), v3xz.x);
                float triMaxX = math.max(math.max(v1xz.x, v2xz.x), v3xz.x);
                float triMinZ = math.min(math.min(v1xz.y, v2xz.y), v3xz.y);
                float triMaxZ = math.max(math.max(v1xz.y, v2xz.y), v3xz.y);

                int minGridX = (int)math.floor((triMinX - minX) / dx);
                int maxGridX = (int)math.ceil((triMaxX - minX) / dx);
                int minGridZ = (int)math.floor((triMinZ - minZ) / dz);
                int maxGridZ = (int)math.ceil((triMaxZ - minZ) / dz);

                minGridX = math.clamp(minGridX, 0, width - 1);
                maxGridX = math.clamp(maxGridX, 0, width - 1);
                minGridZ = math.clamp(minGridZ, 0, height - 1);
                maxGridZ = math.clamp(maxGridZ, 0, height - 1);

                // Precompute barycentric denominator
                float2 v0 = v2xz - v1xz;
                float2 v1v = v3xz - v1xz;
                float denom = v0.x * v1v.y - v1v.x * v0.y;
                if (math.abs(denom) < 1e-8f) continue; // Degenerate triangle
                float invDenom = 1.0f / denom;

                for (int gz = minGridZ; gz <= maxGridZ; gz++)
                {
                    float pz = pz0 + gz * dz;
                    int gx = minGridX;
                    // Batch process 4 pixels at a time
                    for (; gx <= maxGridX - 3; gx += 4)
                    {
                        float4 px4 = new float4(px0 + gx * dx, px0 + (gx + 1) * dx, px0 + (gx + 2) * dx, px0 + (gx + 3) * dx);
                        float4 pz4 = new float4(pz);
                        float4x2 p4 = new float4x2(px4, pz4);

                        // Compute barycentric coordinates for 4 pixels
                        float4 vpx = p4.c0 - v1xz.x;
                        float4 vpz = p4.c1 - v1xz.y;
                        float4 a4 = (vpx * v1v.y - v1v.x * vpz) * invDenom;
                        float4 b4 = (v0.x * vpz - vpx * v0.y) * invDenom;
                        float4 c4 = 1.0f - a4 - b4;

                        float baryEps = 1e-5f;
                        bool4 inside = (a4 >= -baryEps) & (b4 >= -baryEps) & (c4 >= -baryEps) & (a4 <= 1 + baryEps) & (b4 <= 1 + baryEps) & (c4 <= 1 + baryEps);

                        float4 y4 = a4 * v2.y + b4 * v3.y + c4 * v1.y;
                        y4 = math.clamp((y4 - minY + yMod) / yRange, 0f, 1f);

                        for (int i = 0; i < 4; i++)
                        {
                            if (inside[i])
                            {
                                int outIdx = gz * width + (gx + i);
                                if (y4[i] > OutputHeights[outIdx])
                                    OutputHeights[outIdx] = y4[i];
                            }
                        }
                    }
                    // Handle remaining pixels (less than 4)
                    for (; gx <= maxGridX; gx++)
                    {
                        float px = px0 + gx * dx;
                        float2 p = new float2(px, pz);
                        float2 vp = p - v1xz;
                        float a = (vp.x * v1v.y - v1v.x * vp.y) * invDenom;
                        float b = (v0.x * vp.y - vp.x * v0.y) * invDenom;
                        float c = 1.0f - a - b;
                        float baryEps = 1e-5f;
                        if (a >= -baryEps && b >= -baryEps && c >= -baryEps && a <= 1 + baryEps && b <= 1 + baryEps && c <= 1 + baryEps)
                        {
                            float y = a * v2.y + b * v3.y + c * v1.y;
                            int outIdx = gz * width + gx;
                            y = math.clamp((y - minY + yMod) / yRange, 0f, 1f);
                            if (y > OutputHeights[outIdx])
                                OutputHeights[outIdx] = y;
                        }
                    }
                }
            }
        }
    }
}