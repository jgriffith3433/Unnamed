using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

/// Copyright 2025 Procedural Worlds Ltd
/// Written by Jonathan Williams as part of Storm

namespace ProceduralWorlds.Storm.Utils
{
    [BurstCompile(FloatMode = FloatMode.Fast, FloatPrecision = FloatPrecision.Standard, OptimizeFor = OptimizeFor.Performance)]
#if UNITY_2022_2_OR_NEWER
    [GenerateTestsForBurstCompatibility(CompileTarget = GenerateTestsForBurstCompatibilityAttribute.BurstCompatibleCompileTarget.PlayerAndEditor)]
#else
        [BurstCompatible(CompileTarget = BurstCompatibleAttribute.BurstCompatibleCompileTarget.PlayerAndEditor)]
#endif
    public static class StormJobUtils
    {
        [BurstCompile(FloatMode = FloatMode.Fast, FloatPrecision = FloatPrecision.Standard, OptimizeFor = OptimizeFor.Performance)]
#if UNITY_2022_2_OR_NEWER
        [GenerateTestsForBurstCompatibility(CompileTarget = GenerateTestsForBurstCompatibilityAttribute.BurstCompatibleCompileTarget.PlayerAndEditor)]
#else
        [BurstCompatible(CompileTarget = BurstCompatibleAttribute.BurstCompatibleCompileTarget.PlayerAndEditor)]
#endif
        static bool GetSamplePoints<T>(ref float2 uv, ref NativeArray<T> data, ref int2 resolution, ref float2 PixelOffset, ref int2 padding, bool clampEdges, out T v1, out T v2, out T v3, out T v4) where T : struct
        {
            v1 = v2 = v3 = v4 = default;
            uv = (uv * (resolution - (padding * 2))) + padding + PixelOffset;
            int2 p1 = math.int2(math.floor(uv));
            uv = math.frac(uv);
            int2 p2 = p1 + 1;
            if (!clampEdges && (math.any(p2 < 0) || math.any(p1 >= resolution)))
                return false;
            p2 = math.clamp(p2, int2.zero, resolution - 1);
            p1 = math.clamp(p1, int2.zero, resolution - 1);
            v1 = data[p1.y * resolution.x + p1.x];
            v2 = data[p1.y * resolution.x + p2.x];
            v3 = data[p2.y * resolution.x + p1.x];
            v4 = data[p2.y * resolution.x + p2.x];
            return true;
        }
        [BurstCompile(FloatMode = FloatMode.Fast, FloatPrecision = FloatPrecision.Standard, OptimizeFor = OptimizeFor.Performance)]
#if UNITY_2022_2_OR_NEWER
        [GenerateTestsForBurstCompatibility(CompileTarget = GenerateTestsForBurstCompatibilityAttribute.BurstCompatibleCompileTarget.PlayerAndEditor)]
#else
        [BurstCompatible(CompileTarget = BurstCompatibleAttribute.BurstCompatibleCompileTarget.PlayerAndEditor)]
#endif
        static bool GetSamplePoints<T>(ref float2 uv, ref NativeArray<T> data, ref int2 resolution, ref float2 PixelOffset, ref int2 padding, bool clampEdges, int SplitIndex, int SplitLayers, out T v1, out T v2, out T v3, out T v4) where T : struct
        {
            v1 = v2 = v3 = v4 = default;
            uv = (uv * (resolution - (padding * 2))) + padding + PixelOffset;
            int2 p1 = math.int2(math.floor(uv));
            uv = math.frac(uv);
            int2 p2 = p1 + 1;
            if (!clampEdges && (math.any(p2 < 0) || math.any(p1 >= resolution)))
                return false;
            p2 = math.clamp(p2, int2.zero, resolution - 1);
            p1 = math.clamp(p1, int2.zero, resolution - 1);
            v1 = data[SplitIndex + (SplitLayers * p1.x) + (resolution.x * SplitLayers * p1.y)];
            v2 = data[SplitIndex + (SplitLayers * p2.x) + (resolution.x * SplitLayers * p1.y)];
            v3 = data[SplitIndex + (SplitLayers * p1.x) + (resolution.x * SplitLayers * p2.y)];
            v4 = data[SplitIndex + (SplitLayers * p2.x) + (resolution.x * SplitLayers * p2.y)];
            return true;
        }

        [BurstCompile(FloatMode = FloatMode.Fast, FloatPrecision = FloatPrecision.Standard, OptimizeFor = OptimizeFor.Performance)]
#if UNITY_2022_2_OR_NEWER
        [GenerateTestsForBurstCompatibility(CompileTarget = GenerateTestsForBurstCompatibilityAttribute.BurstCompatibleCompileTarget.PlayerAndEditor)]
#else
        [BurstCompatible(CompileTarget = BurstCompatibleAttribute.BurstCompatibleCompileTarget.PlayerAndEditor)]
#endif
        public static float Sample(ref NativeArray<float> data, ref int2 resolution, ref float2 UV)
        {
            float2 uv = UV;
            float2 PixelOffset = default;
            int2 mapPadding = default;
            if (!GetSamplePoints(ref uv, ref data, ref resolution, ref PixelOffset, ref mapPadding, false, out float v1, out float v2, out float v3, out float v4))
                return 0;
            return math.lerp(math.lerp(v1, v2, uv.x), math.lerp(v3, v4, uv.x), uv.y);
        }
        [BurstCompile(FloatMode = FloatMode.Fast, FloatPrecision = FloatPrecision.Standard, OptimizeFor = OptimizeFor.Performance)]
#if UNITY_2022_2_OR_NEWER
        [GenerateTestsForBurstCompatibility(CompileTarget = GenerateTestsForBurstCompatibilityAttribute.BurstCompatibleCompileTarget.PlayerAndEditor)]
#else
        [BurstCompatible(CompileTarget = BurstCompatibleAttribute.BurstCompatibleCompileTarget.PlayerAndEditor)]
#endif
        public static float Sample(ref NativeArray<float> data, ref int2 resolution, ref float2 UV, ref float2 PixelOffset)
        {
            float2 uv = UV;
            int2 mapPadding = default;
            if (!GetSamplePoints(ref uv, ref data, ref resolution, ref PixelOffset, ref mapPadding, false, out float v1, out float v2, out float v3, out float v4))
                return 0;
            return math.lerp(math.lerp(v1, v2, uv.x), math.lerp(v3, v4, uv.x), uv.y);
        }
        [BurstCompile(FloatMode = FloatMode.Fast, FloatPrecision = FloatPrecision.Standard, OptimizeFor = OptimizeFor.Performance)]
#if UNITY_2022_2_OR_NEWER
        [GenerateTestsForBurstCompatibility(CompileTarget = GenerateTestsForBurstCompatibilityAttribute.BurstCompatibleCompileTarget.PlayerAndEditor)]
#else
        [BurstCompatible(CompileTarget = BurstCompatibleAttribute.BurstCompatibleCompileTarget.PlayerAndEditor)]
#endif
        public static float Sample(ref NativeArray<float> data, ref int2 resolution, ref float2 UV, ref float2 PixelOffset, ref int2 mapPadding, bool clampEdges = false)
        {
            float2 uv = UV;
            if (!GetSamplePoints(ref uv, ref data, ref resolution, ref PixelOffset, ref mapPadding, clampEdges, out float v1, out float v2, out float v3, out float v4))
                return 0;
            return math.lerp(math.lerp(v1, v2, uv.x), math.lerp(v3, v4, uv.x), uv.y);
        }
        [BurstCompile(FloatMode = FloatMode.Fast, FloatPrecision = FloatPrecision.Standard, OptimizeFor = OptimizeFor.Performance)]
#if UNITY_2022_2_OR_NEWER
        [GenerateTestsForBurstCompatibility(CompileTarget = GenerateTestsForBurstCompatibilityAttribute.BurstCompatibleCompileTarget.PlayerAndEditor)]
#else
        [BurstCompatible(CompileTarget = BurstCompatibleAttribute.BurstCompatibleCompileTarget.PlayerAndEditor)]
#endif
        public static float Sample(ref NativeArray<float> data, ref int2 resolution, ref float2 UV, ref float2 PixelOffset, int SplitIndex, int SplitLayers)
        {
            float2 uv = UV;
            int2 mapPadding = default;
            if (!GetSamplePoints(ref uv, ref data, ref resolution, ref PixelOffset, ref mapPadding, false, SplitIndex, SplitLayers, out float v1, out float v2, out float v3, out float v4))
                return 0;
            return math.lerp(math.lerp(v1, v2, uv.x), math.lerp(v3, v4, uv.x), uv.y);
        }
        [BurstCompile(FloatMode = FloatMode.Fast, FloatPrecision = FloatPrecision.Standard, OptimizeFor = OptimizeFor.Performance)]
#if UNITY_2022_2_OR_NEWER
        [GenerateTestsForBurstCompatibility(CompileTarget = GenerateTestsForBurstCompatibilityAttribute.BurstCompatibleCompileTarget.PlayerAndEditor)]
#else
        [BurstCompatible(CompileTarget = BurstCompatibleAttribute.BurstCompatibleCompileTarget.PlayerAndEditor)]
#endif
        public static float Sample(ref NativeArray<float> data, ref int2 resolution, ref float2 UV, ref float2 PixelOffset, int SplitIndex, int SplitLayers, ref int2 mapPadding, bool clampEdges = false)
        {
            float2 uv = UV;
            if (!GetSamplePoints(ref uv, ref data, ref resolution, ref PixelOffset, ref mapPadding, clampEdges, SplitIndex, SplitLayers, out float v1, out float v2, out float v3, out float v4))
                return 0;
            return math.lerp(math.lerp(v1, v2, uv.x), math.lerp(v3, v4, uv.x), uv.y);
        }

        [BurstCompile(FloatMode = FloatMode.Fast, FloatPrecision = FloatPrecision.Standard, OptimizeFor = OptimizeFor.Performance)]
#if UNITY_2022_2_OR_NEWER
        [GenerateTestsForBurstCompatibility(CompileTarget = GenerateTestsForBurstCompatibilityAttribute.BurstCompatibleCompileTarget.PlayerAndEditor)]
#else
        [BurstCompatible(CompileTarget = BurstCompatibleAttribute.BurstCompatibleCompileTarget.PlayerAndEditor)]
#endif
        public static float Sample(ref NativeArray<half> data, ref int2 resolution, ref float2 UV)
        {
            float2 uv = UV;
            float2 PixelOffset = default;
            int2 mapPadding = default;
            if (!GetSamplePoints(ref uv, ref data, ref resolution, ref PixelOffset, ref mapPadding, false, out half v1, out half v2, out half v3, out half v4 ))
                return half.zero;
            return math.half(math.lerp(math.lerp(v1, v2, uv.x), math.lerp(v3, v4, uv.x), uv.y));
        }
        [BurstCompile(FloatMode = FloatMode.Fast, FloatPrecision = FloatPrecision.Standard, OptimizeFor = OptimizeFor.Performance)]
#if UNITY_2022_2_OR_NEWER
        [GenerateTestsForBurstCompatibility(CompileTarget = GenerateTestsForBurstCompatibilityAttribute.BurstCompatibleCompileTarget.PlayerAndEditor)]
#else
        [BurstCompatible(CompileTarget = BurstCompatibleAttribute.BurstCompatibleCompileTarget.PlayerAndEditor)]
#endif
        public static float Sample(ref NativeArray<half> data, ref int2 resolution, ref float2 UV, ref float2 PixelOffset)
        {
            float2 uv = UV;
            int2 mapPadding = default;
            if (!GetSamplePoints(ref uv, ref data, ref resolution, ref PixelOffset, ref mapPadding, false, out half v1, out half v2, out half v3, out half v4 ))
                return half.zero;
            return math.half(math.lerp(math.lerp(v1, v2, uv.x), math.lerp(v3, v4, uv.x), uv.y));
        }
        [BurstCompile(FloatMode = FloatMode.Fast, FloatPrecision = FloatPrecision.Standard, OptimizeFor = OptimizeFor.Performance)]
#if UNITY_2022_2_OR_NEWER
        [GenerateTestsForBurstCompatibility(CompileTarget = GenerateTestsForBurstCompatibilityAttribute.BurstCompatibleCompileTarget.PlayerAndEditor)]
#else
        [BurstCompatible(CompileTarget = BurstCompatibleAttribute.BurstCompatibleCompileTarget.PlayerAndEditor)]
#endif
        public static float Sample(ref NativeArray<half> data, ref int2 resolution, ref float2 UV, ref float2 PixelOffset, ref int2 mapPadding, bool clampEdges = false)
        {
            float2 uv = UV;
            if (!GetSamplePoints(ref uv, ref data, ref resolution, ref PixelOffset, ref mapPadding, clampEdges, out half v1, out half v2, out half v3, out half v4 ))
                return half.zero;
            return math.half(math.lerp(math.lerp(v1, v2, uv.x), math.lerp(v3, v4, uv.x), uv.y));
        }

        [BurstCompile(FloatMode = FloatMode.Fast, FloatPrecision = FloatPrecision.Standard, OptimizeFor = OptimizeFor.Performance)]
#if UNITY_2022_2_OR_NEWER
        [GenerateTestsForBurstCompatibility(CompileTarget = GenerateTestsForBurstCompatibilityAttribute.BurstCompatibleCompileTarget.PlayerAndEditor)]
#else
        [BurstCompatible(CompileTarget = BurstCompatibleAttribute.BurstCompatibleCompileTarget.PlayerAndEditor)]
#endif
        public static float Sample(ref NativeArray<half> data, ref int2 resolution, ref float2 UV, ref float2 PixelOffset, int SplitIndex, int SplitLayers)
        {
            float2 uv = UV;
            int2 mapPadding = default;
            if (!GetSamplePoints(ref uv, ref data, ref resolution, ref PixelOffset, ref mapPadding, false, SplitIndex, SplitLayers, out half v1, out half v2, out half v3, out half v4))
                return half.zero;
            return math.half(math.lerp(math.lerp(v1, v2, uv.x), math.lerp(v3, v4, uv.x), uv.y));
        }
        [BurstCompile(FloatMode = FloatMode.Fast, FloatPrecision = FloatPrecision.Standard, OptimizeFor = OptimizeFor.Performance)]
#if UNITY_2022_2_OR_NEWER
        [GenerateTestsForBurstCompatibility(CompileTarget = GenerateTestsForBurstCompatibilityAttribute.BurstCompatibleCompileTarget.PlayerAndEditor)]
#else
        [BurstCompatible(CompileTarget = BurstCompatibleAttribute.BurstCompatibleCompileTarget.PlayerAndEditor)]
#endif
        public static float Sample(ref NativeArray<half> data, ref int2 resolution, ref float2 UV, ref float2 PixelOffset, int SplitIndex, int SplitLayers, ref int2 mapPadding, bool clampEdges = false)
        {
            float2 uv = UV;
            if (!GetSamplePoints(ref uv, ref data, ref resolution, ref PixelOffset, ref mapPadding, clampEdges, SplitIndex, SplitLayers, out half v1, out half v2, out half v3, out half v4))
                return half.zero;
            return math.half(math.lerp(math.lerp(v1, v2, uv.x), math.lerp(v3, v4, uv.x), uv.y));
        }

        [BurstCompile(FloatMode = FloatMode.Fast, FloatPrecision = FloatPrecision.Standard, OptimizeFor = OptimizeFor.Performance)]
#if UNITY_2022_2_OR_NEWER
        [GenerateTestsForBurstCompatibility(CompileTarget = GenerateTestsForBurstCompatibilityAttribute.BurstCompatibleCompileTarget.PlayerAndEditor)]
#else
        [BurstCompatible(CompileTarget = BurstCompatibleAttribute.BurstCompatibleCompileTarget.PlayerAndEditor)]
#endif
        public static byte Sample(ref NativeArray<byte> data, ref int2 resolution, ref float2 UV)
        {
            float2 uv = UV;
            float2 PixelOffset = default;
            int2 mapPadding = default;
            if (!GetSamplePoints(ref uv, ref data, ref resolution, ref PixelOffset, ref mapPadding, false, out byte v1, out byte v2, out byte v3, out byte v4))
                return 0;
            return (byte)math.lerp(math.lerp(v1, v2, uv.x), math.lerp(v3, v4, uv.x), uv.y);
        }
        [BurstCompile(FloatMode = FloatMode.Fast, FloatPrecision = FloatPrecision.Standard, OptimizeFor = OptimizeFor.Performance)]
#if UNITY_2022_2_OR_NEWER
        [GenerateTestsForBurstCompatibility(CompileTarget = GenerateTestsForBurstCompatibilityAttribute.BurstCompatibleCompileTarget.PlayerAndEditor)]
#else
        [BurstCompatible(CompileTarget = BurstCompatibleAttribute.BurstCompatibleCompileTarget.PlayerAndEditor)]
#endif
        public static byte Sample(ref NativeArray<byte> data, ref int2 resolution, ref float2 UV, ref float2 PixelOffset)
        {
            float2 uv = UV;
            int2 mapPadding = default;
            if (!GetSamplePoints(ref uv, ref data, ref resolution, ref PixelOffset, ref mapPadding, false, out byte v1, out byte v2, out byte v3, out byte v4))
                return 0;
            return (byte)math.lerp(math.lerp(v1, v2, uv.x), math.lerp(v3, v4, uv.x), uv.y);
        }
        [BurstCompile(FloatMode = FloatMode.Fast, FloatPrecision = FloatPrecision.Standard, OptimizeFor = OptimizeFor.Performance)]
#if UNITY_2022_2_OR_NEWER
        [GenerateTestsForBurstCompatibility(CompileTarget = GenerateTestsForBurstCompatibilityAttribute.BurstCompatibleCompileTarget.PlayerAndEditor)]
#else
        [BurstCompatible(CompileTarget = BurstCompatibleAttribute.BurstCompatibleCompileTarget.PlayerAndEditor)]
#endif
        public static byte Sample(ref NativeArray<byte> data, ref int2 resolution, ref float2 UV, ref float2 PixelOffset, ref int2 mapPadding, bool clampEdges = false)
        {
            float2 uv = UV;
            if (!GetSamplePoints(ref uv, ref data, ref resolution, ref PixelOffset, ref mapPadding, clampEdges, out byte v1, out byte v2, out byte v3, out byte v4))
                return 0;
            return (byte)math.lerp(math.lerp(v1, v2, uv.x), math.lerp(v3, v4, uv.x), uv.y);
        }
        [BurstCompile(FloatMode = FloatMode.Fast, FloatPrecision = FloatPrecision.Standard, OptimizeFor = OptimizeFor.Performance)]
#if UNITY_2022_2_OR_NEWER
        [GenerateTestsForBurstCompatibility(CompileTarget = GenerateTestsForBurstCompatibilityAttribute.BurstCompatibleCompileTarget.PlayerAndEditor)]
#else
        [BurstCompatible(CompileTarget = BurstCompatibleAttribute.BurstCompatibleCompileTarget.PlayerAndEditor)]
#endif
        public static byte Sample(ref NativeArray<byte> data, ref int2 resolution, ref float2 UV, ref float2 PixelOffset, int SplitIndex, int SplitLayers)
        {
            float2 uv = UV;
            int2 mapPadding = default;
            if (!GetSamplePoints(ref uv, ref data, ref resolution, ref PixelOffset, ref mapPadding, false, SplitIndex, SplitLayers, out byte v1, out byte v2, out byte v3, out byte v4))
                return 0;
            return (byte)math.lerp(math.lerp(v1, v2, uv.x), math.lerp(v3, v4, uv.x), uv.y);
        }
        [BurstCompile(FloatMode = FloatMode.Fast, FloatPrecision = FloatPrecision.Standard, OptimizeFor = OptimizeFor.Performance)]
#if UNITY_2022_2_OR_NEWER
        [GenerateTestsForBurstCompatibility(CompileTarget = GenerateTestsForBurstCompatibilityAttribute.BurstCompatibleCompileTarget.PlayerAndEditor)]
#else
        [BurstCompatible(CompileTarget = BurstCompatibleAttribute.BurstCompatibleCompileTarget.PlayerAndEditor)]
#endif
        public static byte Sample(ref NativeArray<byte> data, ref int2 resolution, ref float2 UV, ref float2 PixelOffset, int SplitIndex, int SplitLayers, ref int2 mapPadding, bool clampEdges = false)
        {
            float2 uv = UV;
            if (!GetSamplePoints(ref uv, ref data, ref resolution, ref PixelOffset, ref mapPadding, clampEdges, SplitIndex, SplitLayers, out byte v1, out byte v2, out byte v3, out byte v4))
                return 0;
            return (byte)math.lerp(math.lerp(v1, v2, uv.x), math.lerp(v3, v4, uv.x), uv.y);
        }

        [BurstCompile(FloatMode = FloatMode.Fast, FloatPrecision = FloatPrecision.Standard, OptimizeFor = OptimizeFor.Performance)]
#if UNITY_2022_2_OR_NEWER
        [GenerateTestsForBurstCompatibility(CompileTarget = GenerateTestsForBurstCompatibilityAttribute.BurstCompatibleCompileTarget.PlayerAndEditor)]
#else
        [BurstCompatible(CompileTarget = BurstCompatibleAttribute.BurstCompatibleCompileTarget.PlayerAndEditor)]
#endif
        public static float Sample(ref NativeArray<int> data, ref int2 resolution, ref float2 UV)
        {
            float2 uv = UV;
            float2 PixelOffset = default;
            int2 mapPadding = default;
            if (!GetSamplePoints(ref uv, ref data, ref resolution, ref PixelOffset, ref mapPadding, false, out int v1, out int v2, out int v3, out int v4))
                return 0;
            return math.lerp(math.lerp(v1, v2, uv.x), math.lerp(v3, v4, uv.x), uv.y);
        }
        [BurstCompile(FloatMode = FloatMode.Fast, FloatPrecision = FloatPrecision.Standard, OptimizeFor = OptimizeFor.Performance)]
#if UNITY_2022_2_OR_NEWER
        [GenerateTestsForBurstCompatibility(CompileTarget = GenerateTestsForBurstCompatibilityAttribute.BurstCompatibleCompileTarget.PlayerAndEditor)]
#else
        [BurstCompatible(CompileTarget = BurstCompatibleAttribute.BurstCompatibleCompileTarget.PlayerAndEditor)]
#endif
        public static float Sample(ref NativeArray<int> data, ref int2 resolution, ref float2 UV, ref float2 PixelOffset)
        {
            float2 uv = UV;
            int2 mapPadding = default;
            if (!GetSamplePoints(ref uv, ref data, ref resolution, ref PixelOffset, ref mapPadding, false, out int v1, out int v2, out int v3, out int v4))
                return 0;
            return math.lerp(math.lerp(v1, v2, uv.x), math.lerp(v3, v4, uv.x), uv.y);
        }
        [BurstCompile(FloatMode = FloatMode.Fast, FloatPrecision = FloatPrecision.Standard, OptimizeFor = OptimizeFor.Performance)]
#if UNITY_2022_2_OR_NEWER
        [GenerateTestsForBurstCompatibility(CompileTarget = GenerateTestsForBurstCompatibilityAttribute.BurstCompatibleCompileTarget.PlayerAndEditor)]
#else
        [BurstCompatible(CompileTarget = BurstCompatibleAttribute.BurstCompatibleCompileTarget.PlayerAndEditor)]
#endif
        public static float Sample(ref NativeArray<int> data, ref int2 resolution, ref float2 UV, ref float2 PixelOffset, ref int2 mapPadding, bool clampEdges = false)
        {
            float2 uv = UV;
            if (!GetSamplePoints(ref uv, ref data, ref resolution, ref PixelOffset, ref mapPadding, clampEdges, out int v1, out int v2, out int v3, out int v4))
                return 0;
            return math.lerp(math.lerp(v1, v2, uv.x), math.lerp(v3, v4, uv.x), uv.y);
        }
        [BurstCompile(FloatMode = FloatMode.Fast, FloatPrecision = FloatPrecision.Standard, OptimizeFor = OptimizeFor.Performance)]
#if UNITY_2022_2_OR_NEWER
        [GenerateTestsForBurstCompatibility(CompileTarget = GenerateTestsForBurstCompatibilityAttribute.BurstCompatibleCompileTarget.PlayerAndEditor)]
#else
        [BurstCompatible(CompileTarget = BurstCompatibleAttribute.BurstCompatibleCompileTarget.PlayerAndEditor)]
#endif
        public static float Sample(ref NativeArray<int> data, ref int2 resolution, ref float2 UV, ref float2 PixelOffset, int SplitIndex, int SplitLayers)
        {
            float2 uv = UV;
            int2 mapPadding = default;
            if (!GetSamplePoints(ref uv, ref data, ref resolution, ref PixelOffset, ref mapPadding, false, SplitIndex, SplitLayers, out int v1, out int v2, out int v3, out int v4))
                return 0;
            return math.lerp(math.lerp(v1, v2, uv.x), math.lerp(v3, v4, uv.x), uv.y);
        }
        [BurstCompile(FloatMode = FloatMode.Fast, FloatPrecision = FloatPrecision.Standard, OptimizeFor = OptimizeFor.Performance)]
#if UNITY_2022_2_OR_NEWER
        [GenerateTestsForBurstCompatibility(CompileTarget = GenerateTestsForBurstCompatibilityAttribute.BurstCompatibleCompileTarget.PlayerAndEditor)]
#else
        [BurstCompatible(CompileTarget = BurstCompatibleAttribute.BurstCompatibleCompileTarget.PlayerAndEditor)]
#endif
        public static float Sample(ref NativeArray<int> data, ref int2 resolution, ref float2 UV, ref float2 PixelOffset, int SplitIndex, int SplitLayers, ref int2 mapPadding, bool clampEdges = false)
        {
            float2 uv = UV;
            if (!GetSamplePoints(ref uv, ref data, ref resolution, ref PixelOffset, ref mapPadding, clampEdges, SplitIndex, SplitLayers, out int v1, out int v2, out int v3, out int v4))
                return 0;
            return math.lerp(math.lerp(v1, v2, uv.x), math.lerp(v3, v4, uv.x), uv.y);
        }

        [BurstCompile(FloatMode = FloatMode.Fast, FloatPrecision = FloatPrecision.Standard, OptimizeFor = OptimizeFor.Performance)]
#if UNITY_2022_2_OR_NEWER
        [GenerateTestsForBurstCompatibility(CompileTarget = GenerateTestsForBurstCompatibilityAttribute.BurstCompatibleCompileTarget.PlayerAndEditor)]
#else
        [BurstCompatible(CompileTarget = BurstCompatibleAttribute.BurstCompatibleCompileTarget.PlayerAndEditor)]
#endif
        public static float Sample(ref NativeArray<bool> data, ref int2 resolution, ref float2 UV, float trueValue, float falseValue)
        {
            float2 uv = UV;
            float2 PixelOffset = default;
            int2 mapPadding = default;
            if (!GetSamplePoints(ref uv, ref data, ref resolution, ref PixelOffset, ref mapPadding, false, out bool v1, out bool v2, out bool v3, out bool v4))
                return 0;
            return math.lerp(math.lerp(v1 ? trueValue : falseValue, v2 ? trueValue : falseValue, uv.x), math.lerp(v3 ? trueValue : falseValue, v4 ? trueValue : falseValue, uv.x), uv.y);
        }
        [BurstCompile(FloatMode = FloatMode.Fast, FloatPrecision = FloatPrecision.Standard, OptimizeFor = OptimizeFor.Performance)]
#if UNITY_2022_2_OR_NEWER
        [GenerateTestsForBurstCompatibility(CompileTarget = GenerateTestsForBurstCompatibilityAttribute.BurstCompatibleCompileTarget.PlayerAndEditor)]
#else
        [BurstCompatible(CompileTarget = BurstCompatibleAttribute.BurstCompatibleCompileTarget.PlayerAndEditor)]
#endif
        public static float Sample(ref NativeArray<bool> data, ref int2 resolution, ref float2 UV, float trueValue, float falseValue, ref float2 PixelOffset)
        {
            float2 uv = UV;
            int2 mapPadding = default;
            if (!GetSamplePoints(ref uv, ref data, ref resolution, ref PixelOffset, ref mapPadding, false, out bool v1, out bool v2, out bool v3, out bool v4))
                return 0;
            return math.lerp(math.lerp(v1 ? trueValue : falseValue, v2 ? trueValue : falseValue, uv.x), math.lerp(v3 ? trueValue : falseValue, v4 ? trueValue : falseValue, uv.x), uv.y);
        }
        [BurstCompile(FloatMode = FloatMode.Fast, FloatPrecision = FloatPrecision.Standard, OptimizeFor = OptimizeFor.Performance)]
#if UNITY_2022_2_OR_NEWER
        [GenerateTestsForBurstCompatibility(CompileTarget = GenerateTestsForBurstCompatibilityAttribute.BurstCompatibleCompileTarget.PlayerAndEditor)]
#else
        [BurstCompatible(CompileTarget = BurstCompatibleAttribute.BurstCompatibleCompileTarget.PlayerAndEditor)]
#endif
        public static float Sample(ref NativeArray<bool> data, ref int2 resolution, ref float2 UV, float trueValue, float falseValue, ref float2 PixelOffset, ref int2 mapPadding, bool clampEdges = false)
        {
            float2 uv = UV;
            if (!GetSamplePoints(ref uv, ref data, ref resolution, ref PixelOffset, ref mapPadding, clampEdges, out bool v1, out bool v2, out bool v3, out bool v4))
                return 0;
            return math.lerp(math.lerp(v1 ? trueValue : falseValue, v2 ? trueValue : falseValue, uv.x), math.lerp(v3 ? trueValue : falseValue, v4 ? trueValue : falseValue, uv.x), uv.y);
        }
        [BurstCompile(FloatMode = FloatMode.Fast, FloatPrecision = FloatPrecision.Standard, OptimizeFor = OptimizeFor.Performance)]
#if UNITY_2022_2_OR_NEWER
        [GenerateTestsForBurstCompatibility(CompileTarget = GenerateTestsForBurstCompatibilityAttribute.BurstCompatibleCompileTarget.PlayerAndEditor)]
#else
        [BurstCompatible(CompileTarget = BurstCompatibleAttribute.BurstCompatibleCompileTarget.PlayerAndEditor)]
#endif
        public static float Sample(ref NativeArray<bool> data, ref int2 resolution, ref float2 UV, float trueValue, float falseValue, ref float2 PixelOffset, int SplitIndex, int SplitLayers)
        {
            float2 uv = UV;
            int2 mapPadding = default;
            if (!GetSamplePoints(ref uv, ref data, ref resolution, ref PixelOffset, ref mapPadding, false, SplitIndex, SplitLayers, out bool v1, out bool v2, out bool v3, out bool v4))
                return 0;
            return math.lerp(math.lerp(v1 ? trueValue : falseValue, v2 ? trueValue : falseValue, uv.x), math.lerp(v3 ? trueValue : falseValue, v4 ? trueValue : falseValue, uv.x), uv.y);
        }
        [BurstCompile(FloatMode = FloatMode.Fast, FloatPrecision = FloatPrecision.Standard, OptimizeFor = OptimizeFor.Performance)]
#if UNITY_2022_2_OR_NEWER
        [GenerateTestsForBurstCompatibility(CompileTarget = GenerateTestsForBurstCompatibilityAttribute.BurstCompatibleCompileTarget.PlayerAndEditor)]
#else
        [BurstCompatible(CompileTarget = BurstCompatibleAttribute.BurstCompatibleCompileTarget.PlayerAndEditor)]
#endif
        public static float Sample(ref NativeArray<bool> data, ref int2 resolution, ref float2 UV, float trueValue, float falseValue, ref float2 PixelOffset, int SplitIndex, int SplitLayers, ref int2 mapPadding, bool clampEdges = false)
        {
            float2 uv = UV;
            if (!GetSamplePoints(ref uv, ref data, ref resolution, ref PixelOffset, ref mapPadding, clampEdges, SplitIndex, SplitLayers, out bool v1, out bool v2, out bool v3, out bool v4))
                return 0;
            return math.lerp(math.lerp(v1 ? trueValue : falseValue, v2 ? trueValue : falseValue, uv.x), math.lerp(v3 ? trueValue : falseValue, v4 ? trueValue : falseValue, uv.x), uv.y);
        }
    }
}