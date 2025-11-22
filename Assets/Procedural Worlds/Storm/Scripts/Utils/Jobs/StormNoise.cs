using Unity.Burst;
using Unity.Mathematics;
using UnityEngine;

/// Copyright 2024 Procedural Worlds Ltd
/// Written by Jonathan Williams as part of Storm

namespace ProceduralWorlds.Storm.Utils
{
    public class StormNoise : MonoBehaviour
    {
        public static float SampleNoise(RandomType type, uint seed, float2 coord, float scale, float lacunarity, float persistence, float mod, float mult, int steps)
        {
            if (mult == 0)
                return 0;
            float ret;
            switch (type)
            {
                case RandomType.UNITY_PERLIN:
                case RandomType.UNITY_CELLULAR:
                case RandomType.UNITY_SIMPLEX:
                case RandomType.UNITY_RIDGE:
                    ret = SampleNoiseStep(type, coord, scale, persistence, lacunarity, steps);
                    break;
                case RandomType.STORM_PERLIN:
                    ret = Noise.FractalNoise2D(seed, coord * scale, steps, 1, 1, lacunarity, persistence);
                    break;
                case RandomType.STORM_SIMPLEX:
                    ret = Noise.FractalSimplexNoise2D(seed, coord * scale, steps, 1, 1, lacunarity, persistence);
                    break;
                case RandomType.STORM_RIDGE:
                    ret = Noise.FractalRidgeNoise2D(seed, coord * scale, steps, 1, 1, lacunarity, persistence);
                    break;
                case RandomType.STORM_CELLULAR:
                    ret = Noise.FractalCellularNoise2D(seed, coord * scale, steps, 1, 1, lacunarity, persistence);
                    break;
                default:
                    ret = 1;
                    break;
            }
            ret = mod + ret * mult;
            return math.clamp(ret, 0, 1);
        }
        private static float SampleNoiseStep(RandomType type, float2 coord, float scale, float persistence, float lacunarity, int stepCount, int step = 1, float weight = 1f, float ridgeFrequency = 1f)
        {
            float ret;
            switch (type)
            {
                case RandomType.UNITY_PERLIN:
                    ret = noise.cnoise(coord * scale);
                    break;
                case RandomType.UNITY_SIMPLEX:
                case RandomType.UNITY_RIDGE:
                    ret = noise.snoise(coord * scale);
                    break;
                case RandomType.UNITY_CELLULAR:
                    float2 n = noise.cellular2x2(coord * scale);
                    ret = (n.x * 2) - 1f;
                    break;
                default:
                    ret = 1;
                    break;
            }
            if (step < stepCount)
            {
                if (type == RandomType.UNITY_RIDGE)
                {
                    ret = math.abs(ret);
                    ret = 1f - ret;
                    ret *= ret;
                    ret *= weight;
                    weight = math.saturate(ret * persistence);
                    ret *= math.pow(ridgeFrequency, -1);
                    ridgeFrequency *= lacunarity;
                }
                else
                {
                    ret *= weight;
                    weight *= persistence;
                }
                ret += SampleNoiseStep(type, coord.yx, scale * lacunarity, persistence, lacunarity, stepCount, step + 1, weight, ridgeFrequency);
            }
            else
                ret *= weight;
            return ret;
        }

        public static void UpdateNoiseTexture(ref Texture2D tex, int texRes, uint seed, float2 texOffset, float2 texRange, RandomType type, float scale, float lacunarity, float persistence, float mod, float mult, int steps, CombineMethod method2, RandomType type2, float scale2, float lacunarity2, float persistence2, float mod2, float mult2, int steps2)
        {
            if (tex == null || tex.width != texRes)
                tex = new Texture2D(texRes, texRes, TextureFormat.RGBAFloat, false);
            Color32[] pixels = new Color32[texRes * texRes];
            for (int y = 0; y < texRes; ++y)
            {
                for (int x = 0; x < texRes; ++x)
                {
                    float val = SampleNoise(type, seed, texOffset + math.float2(x / (float)texRes * texRange.x, y / (float)texRes * texRange.y), scale, lacunarity, persistence, mod, mult, steps);
                    if (type2 != 0)
                    {
                        float val2 = SampleNoise(type2, seed, texOffset + math.float2(x / (float)texRes * texRange.x, y / (float)texRes * texRange.y), scale2, lacunarity2, persistence2, mod2, mult2, steps2);
                        switch (method2)
                        {
                            case CombineMethod.MULTIPLY:
                                val *= val2;
                                break;
                            case CombineMethod.ADD:
                                val += val2;
                                break;
                            case CombineMethod.POWER:
                                val = Mathf.Pow(val, val2);
                                break;
                        }
                    }
                    byte b = (byte)(val * 255);
                    pixels[y * texRes + x] = new Color32(b, b, b, 1);
                }
            }
            tex.SetPixels32(pixels);
            tex.Apply(false, false);
        }

        public static string NoiseToString(RandomType type, float scale, float lacunarity, float persistence, float mod, float mult, int steps, CombineMethod method2, RandomType type2, float scale2, float lacunarity2, float persistence2, float mod2, float mult2, int steps2)
        {
            System.Text.StringBuilder ret = new System.Text.StringBuilder();
            switch (type)
            {
                case RandomType.NONE:
                    return "";
                case RandomType.UNITY_PERLIN:
                    ret.Append("p(");
                    break;
                case RandomType.UNITY_SIMPLEX:
                    ret.Append("s(");
                    break;
                case RandomType.UNITY_CELLULAR:
                    ret.Append("c(");
                    break;
                case RandomType.UNITY_RIDGE:
                    ret.Append("r(");
                    break;
            }
            ret.Append(scale).Append(',');
            ret.Append(lacunarity).Append(',');
            ret.Append(persistence).Append(',');
            ret.Append(mod).Append(',');
            ret.Append(mult).Append(',');
            ret.Append(steps).Append(')');
            if (type2 != RandomType.NONE)
            {
                switch (method2)
                {
                    case CombineMethod.ADD:
                        ret.Append('+');
                        break;
                    case CombineMethod.MULTIPLY:
                        ret.Append('*');
                        break;
                    case CombineMethod.POWER:
                        ret.Append('^');
                        break;
                }
                switch (type2)
                {
                    case RandomType.UNITY_PERLIN:
                        ret.Append("p(");
                        break;
                    case RandomType.UNITY_SIMPLEX:
                        ret.Append("s(");
                        break;
                    case RandomType.UNITY_CELLULAR:
                        ret.Append("c(");
                        break;
                    case RandomType.UNITY_RIDGE:
                        ret.Append("r(");
                        break;
                }
                ret.Append(scale2).Append(',');
                ret.Append(lacunarity2).Append(',');
                ret.Append(persistence2).Append(',');
                ret.Append(mod2).Append(',');
                ret.Append(mult2).Append(',');
                ret.Append(steps2).Append(')');
            }
            return ret.ToString();
        }
        public static bool StringToNoise(string str, out RandomType type, out float scale, out float lacunarity, out float persistence, out float mod, out float mult, out int steps, out CombineMethod method2, out RandomType type2, out float scale2, out float lacunarity2, out float persistence2, out float mod2, out float mult2, out int steps2)
        {
            type = RandomType.NONE;
            scale = 1f;
            lacunarity = 2f;
            persistence = 0.5f;
            mod = 0f;
            mult = 1f;
            steps = 1;
            method2 = CombineMethod.MULTIPLY;
            type2 = RandomType.NONE;
            scale2 = 1f;
            lacunarity2 = 2f;
            persistence2 = 0.5f;
            mod2 = 0f;
            mult2 = 1f;
            steps2 = 1;
            if (string.IsNullOrEmpty(str) || str.Length < 4)
                return true;
            switch (str[0])
            {
                case 'p':
                    type = RandomType.UNITY_PERLIN;
                    break;
                case 'c':
                    type = RandomType.UNITY_CELLULAR;
                        break;
                case 's':
                    type = RandomType.UNITY_SIMPLEX;
                    break;
                case 'r':
                    type = RandomType.UNITY_RIDGE;
                    break;
                default:
                    return false;
            }
            if (str[1] != '(')
                return false;
            int endPos = str.IndexOf(')', 2);
            string[] parts = str.Substring(2, endPos - 2).Split(',');
            if (parts.Length != 6)
                return false;
            if (!float.TryParse(parts[0], out scale))
                return false;
            if (!float.TryParse(parts[1], out lacunarity))
                return false;
            if (!float.TryParse(parts[2], out persistence))
                return false;
            if (!float.TryParse(parts[3], out mod))
                return false;
            if (!float.TryParse(parts[4], out mult))
                return false;
            if (!int.TryParse(parts[5], out steps))
                return false;
            if (str.Length > endPos + 4)
            {
                switch (str[endPos + 1])
                {
                    case '+':
                        method2 = CombineMethod.ADD;
                        break;
                    case '*':
                        method2 = CombineMethod.MULTIPLY;
                        break;
                    case '^':
                        method2 = CombineMethod.POWER;
                        break;
                    default:
                        return false;
                }
                switch (str[endPos + 2])
                {
                    case 'p':
                        type2 = RandomType.UNITY_PERLIN;
                        break;
                    case 'c':
                        type2 = RandomType.UNITY_CELLULAR;
                        break;
                    case 's':
                        type2 = RandomType.UNITY_SIMPLEX;
                        break;
                    case 'r':
                        type2 = RandomType.UNITY_RIDGE;
                        break;
                    default:
                        return false;
                }
                if (str[endPos + 3] != '(')
                    return false;
                int startPos = endPos + 4;
                endPos = str.IndexOf(')', startPos);
                parts = str.Substring(startPos, endPos - startPos).Split(',');
                if (parts.Length != 6)
                    return false;
                if (!float.TryParse(parts[0], out scale2))
                    return false;
                if (!float.TryParse(parts[1], out lacunarity2))
                    return false;
                if (!float.TryParse(parts[2], out persistence2))
                    return false;
                if (!float.TryParse(parts[3], out mod2))
                    return false;
                if (!float.TryParse(parts[4], out mult2))
                    return false;
                if (!int.TryParse(parts[5], out steps2))
                    return false;
            }
            return true;
        }
    }

    /// <summary>
    /// Seed-based noise functions optimized for Unity Jobs and Burst compilation.
    /// Provides deterministic noise generation with configurable seeds.
    /// </summary>
    [BurstCompile]
    public static class Noise
    {
        #region Variables and Init
        // Constants for noise generation
        private const uint NOISE_PRIME_X = 0x5205402B;
        private const uint NOISE_PRIME_Y = 0x598CD444;
        private const uint NOISE_PRIME_Z = 0x5BCC226E;
        private const uint NOISE_PRIME_W = 0x56CC522B;

        private const float NOISE_SCALE_1D = 1.0f / 2147483648.0f;
        private const float NOISE_SCALE_2D = 1.0f / 2147483648.0f;
        private const float NOISE_SCALE_3D = 1.0f / 2147483648.0f;
        private const float NOISE_SCALE_4D = 1.0f / 2147483648.0f;


        // Simplex noise constants
        private const float SIMPLEX_F2 = 0.3660254037844386f; // (sqrt(3) - 1) / 2
        private const float SIMPLEX_G2 = 0.2113248654051871f; // (3 - sqrt(3)) / 6
        private const float SIMPLEX_F3 = 1.0f / 3.0f;
        private const float SIMPLEX_G3 = 1.0f / 6.0f;
        private const float SIMPLEX_F4 = 0.309016994374947f; // (sqrt(5) - 1) / 4
        private const float SIMPLEX_G4 = 0.1381966011250105f; // (5 - sqrt(5)) / 20

        // Simplex noise gradient tables
        private static readonly int3[] SIMPLEX_GRAD3 = new int3[]
        {
            new int3(1, 1, 0), new int3(-1, 1, 0), new int3(1, -1, 0), new int3(-1, -1, 0),
            new int3(1, 0, 1), new int3(-1, 0, 1), new int3(1, 0, -1), new int3(-1, 0, -1),
            new int3(0, 1, 1), new int3(0, -1, 1), new int3(0, 1, -1), new int3(0, -1, -1)
        };

        private static readonly int4[] SIMPLEX_GRAD4 = new int4[]
        {
            new int4(0, 1, 1, 1), new int4(0, 1, 1, -1), new int4(0, 1, -1, 1), new int4(0, 1, -1, -1),
            new int4(0, -1, 1, 1), new int4(0, -1, 1, -1), new int4(0, -1, -1, 1), new int4(0, -1, -1, -1),
            new int4(1, 0, 1, 1), new int4(1, 0, 1, -1), new int4(1, 0, -1, 1), new int4(1, 0, -1, -1),
            new int4(-1, 0, 1, 1), new int4(-1, 0, 1, -1), new int4(-1, 0, -1, 1), new int4(-1, 0, -1, -1),
            new int4(1, 1, 0, 1), new int4(1, 1, 0, -1), new int4(1, -1, 0, 1), new int4(1, -1, 0, -1),
            new int4(-1, 1, 0, 1), new int4(-1, 1, 0, -1), new int4(-1, -1, 0, 1), new int4(-1, -1, 0, -1),
            new int4(1, 1, 1, 0), new int4(1, 1, -1, 0), new int4(1, -1, 1, 0), new int4(1, -1, -1, 0),
            new int4(-1, 1, 1, 0), new int4(-1, 1, -1, 0), new int4(-1, -1, 1, 0), new int4(-1, -1, -1, 0)
        };
        #endregion
        #region Utility Functions
        /// <summary>
        /// Generates a hash value from a seed and coordinates
        /// </summary>
        [BurstCompile]
        public static uint Hash(uint seed, int x)
        {
            uint hash = seed;
            hash ^= (uint)x;
            hash *= NOISE_PRIME_X;
            hash ^= hash >> 16;
            hash *= NOISE_PRIME_Y;
            hash ^= hash >> 16;
            hash *= NOISE_PRIME_Z;
            hash ^= hash >> 16;
            return hash;
        }

        /// <summary>
        /// Generates a hash value from a seed and 2D coordinates
        /// </summary>
        [BurstCompile]
        public static uint Hash(uint seed, int x, int y)
        {
            uint hash = seed;
            hash ^= (uint)x;
            hash *= NOISE_PRIME_X;
            hash ^= (uint)y;
            hash *= NOISE_PRIME_Y;
            hash ^= hash >> 16;
            hash *= NOISE_PRIME_Z;
            hash ^= hash >> 16;
            return hash;
        }

        /// <summary>
        /// Generates a hash value from a seed and 3D coordinates
        /// </summary>
        [BurstCompile]
        public static uint Hash(uint seed, int x, int y, int z)
        {
            uint hash = seed;
            hash ^= (uint)x;
            hash *= NOISE_PRIME_X;
            hash ^= (uint)y;
            hash *= NOISE_PRIME_Y;
            hash ^= (uint)z;
            hash *= NOISE_PRIME_Z;
            hash ^= hash >> 16;
            hash *= NOISE_PRIME_W;
            hash ^= hash >> 16;
            return hash;
        }

        /// <summary>
        /// Generates a hash value from a seed and 4D coordinates
        /// </summary>
        [BurstCompile]
        public static uint Hash(uint seed, int x, int y, int z, int w)
        {
            uint hash = seed;
            hash ^= (uint)x;
            hash *= NOISE_PRIME_X;
            hash ^= (uint)y;
            hash *= NOISE_PRIME_Y;
            hash ^= (uint)z;
            hash *= NOISE_PRIME_Z;
            hash ^= (uint)w;
            hash *= NOISE_PRIME_W;
            hash ^= hash >> 16;
            hash *= NOISE_PRIME_X;
            hash ^= hash >> 16;
            return hash;
        }

        /// <summary>
        /// Converts a hash to a float value between -1 and 1 for 1D noise
        /// </summary>
        [BurstCompile]
        public static float HashToFloat1D(uint hash)
        {
            return (float)((int)hash) * NOISE_SCALE_1D;
        }

        /// <summary>
        /// Converts a hash to a float value between -1 and 1 for 2D noise
        /// </summary>
        [BurstCompile]
        public static float HashToFloat2D(uint hash)
        {
            return (float)((int)hash) * NOISE_SCALE_2D;
        }

        /// <summary>
        /// Converts a hash to a float value between -1 and 1 for 3D noise
        /// </summary>
        [BurstCompile]
        public static float HashToFloat3D(uint hash)
        {
            return (float)((int)hash) * NOISE_SCALE_3D;
        }

        /// <summary>
        /// Converts a hash to a float value between -1 and 1 for 4D noise
        /// </summary>
        [BurstCompile]
        public static float HashToFloat4D(uint hash)
        {
            return (float)((int)hash) * NOISE_SCALE_4D;
        }

        /// <summary>
        /// Smooth step function for interpolation
        /// </summary>
        [BurstCompile]
        private static float SmoothStep(float t)
        {
            return t * t * (3.0f - 2.0f * t);
        }
        #endregion
        #region Basic Noise
        /// <summary>
        /// 1D noise function with seed
        /// </summary>
        [BurstCompile]
        public static float Noise1D(uint seed, float x)
        {
            int xi = (int)math.floor(x);
            float xf = x - xi;

            uint hash0 = Hash(seed, xi);
            uint hash1 = Hash(seed, xi + 1);

            float noise0 = HashToFloat1D(hash0);
            float noise1 = HashToFloat1D(hash1);

            // Smooth interpolation
            float t = SmoothStep(xf);
            return math.lerp(noise0, noise1, t);
        }

        /// <summary>
        /// 2D noise function with seed
        /// </summary>
        [BurstCompile]
        public static float Noise2D(uint seed, in float2 pos)
        {
            int xi = (int)math.floor(pos.x);
            int yi = (int)math.floor(pos.y);
            float xf = pos.x - xi;
            float yf = pos.y - yi;

            uint hash00 = Hash(seed, xi, yi);
            uint hash10 = Hash(seed, xi + 1, yi);
            uint hash01 = Hash(seed, xi, yi + 1);
            uint hash11 = Hash(seed, xi + 1, yi + 1);

            float noise00 = HashToFloat2D(hash00);
            float noise10 = HashToFloat2D(hash10);
            float noise01 = HashToFloat2D(hash01);
            float noise11 = HashToFloat2D(hash11);

            // Smooth interpolation
            float tx = SmoothStep(xf);
            float ty = SmoothStep(yf);

            float noise0 = math.lerp(noise00, noise10, tx);
            float noise1 = math.lerp(noise01, noise11, tx);

            return math.lerp(noise0, noise1, ty);
        }

        /// <summary>
        /// 3D noise function with seed
        /// </summary>
        [BurstCompile]
        public static float Noise3D(uint seed, in float3 pos)
        {
            int xi = (int)math.floor(pos.x);
            int yi = (int)math.floor(pos.y);
            int zi = (int)math.floor(pos.z);
            float xf = pos.x - xi;
            float yf = pos.y - yi;
            float zf = pos.z - zi;

            uint hash000 = Hash(seed, xi, yi, zi);
            uint hash100 = Hash(seed, xi + 1, yi, zi);
            uint hash010 = Hash(seed, xi, yi + 1, zi);
            uint hash110 = Hash(seed, xi + 1, yi + 1, zi);
            uint hash001 = Hash(seed, xi, yi, zi + 1);
            uint hash101 = Hash(seed, xi + 1, yi, zi + 1);
            uint hash011 = Hash(seed, xi, yi + 1, zi + 1);
            uint hash111 = Hash(seed, xi + 1, yi + 1, zi + 1);

            float noise000 = HashToFloat3D(hash000);
            float noise100 = HashToFloat3D(hash100);
            float noise010 = HashToFloat3D(hash010);
            float noise110 = HashToFloat3D(hash110);
            float noise001 = HashToFloat3D(hash001);
            float noise101 = HashToFloat3D(hash101);
            float noise011 = HashToFloat3D(hash011);
            float noise111 = HashToFloat3D(hash111);

            // Smooth interpolation
            float tx = SmoothStep(xf);
            float ty = SmoothStep(yf);
            float tz = SmoothStep(zf);

            float noise00 = math.lerp(noise000, noise100, tx);
            float noise10 = math.lerp(noise010, noise110, tx);
            float noise01 = math.lerp(noise001, noise101, tx);
            float noise11 = math.lerp(noise011, noise111, tx);

            float noise0 = math.lerp(noise00, noise10, ty);
            float noise1 = math.lerp(noise01, noise11, ty);

            return math.lerp(noise0, noise1, tz);
        }

        /// <summary>
        /// 4D noise function with seed
        /// </summary>
        [BurstCompile]
        public static float Noise4D(uint seed, in float4 pos)
        {
            int xi = (int)math.floor(pos.x);
            int yi = (int)math.floor(pos.y);
            int zi = (int)math.floor(pos.z);
            int wi = (int)math.floor(pos.w);
            float xf = pos.x - xi;
            float yf = pos.y - yi;
            float zf = pos.z - zi;
            float wf = pos.w - wi;

            uint hash0000 = Hash(seed, xi, yi, zi, wi);
            uint hash1000 = Hash(seed, xi + 1, yi, zi, wi);
            uint hash0100 = Hash(seed, xi, yi + 1, zi, wi);
            uint hash1100 = Hash(seed, xi + 1, yi + 1, zi, wi);
            uint hash0010 = Hash(seed, xi, yi, zi + 1, wi);
            uint hash1010 = Hash(seed, xi + 1, yi, zi + 1, wi);
            uint hash0110 = Hash(seed, xi, yi + 1, zi + 1, wi);
            uint hash1110 = Hash(seed, xi + 1, yi + 1, zi + 1, wi);
            uint hash0001 = Hash(seed, xi, yi, zi, wi + 1);
            uint hash1001 = Hash(seed, xi + 1, yi, zi, wi + 1);
            uint hash0101 = Hash(seed, xi, yi + 1, zi, wi + 1);
            uint hash1101 = Hash(seed, xi + 1, yi + 1, zi, wi + 1);
            uint hash0011 = Hash(seed, xi, yi, zi + 1, wi + 1);
            uint hash1011 = Hash(seed, xi + 1, yi, zi + 1, wi + 1);
            uint hash0111 = Hash(seed, xi, yi + 1, zi + 1, wi + 1);
            uint hash1111 = Hash(seed, xi + 1, yi + 1, zi + 1, wi + 1);

            float noise0000 = HashToFloat4D(hash0000);
            float noise1000 = HashToFloat4D(hash1000);
            float noise0100 = HashToFloat4D(hash0100);
            float noise1100 = HashToFloat4D(hash1100);
            float noise0010 = HashToFloat4D(hash0010);
            float noise1010 = HashToFloat4D(hash1010);
            float noise0110 = HashToFloat4D(hash0110);
            float noise1110 = HashToFloat4D(hash1110);
            float noise0001 = HashToFloat4D(hash0001);
            float noise1001 = HashToFloat4D(hash1001);
            float noise0101 = HashToFloat4D(hash0101);
            float noise1101 = HashToFloat4D(hash1101);
            float noise0011 = HashToFloat4D(hash0011);
            float noise1011 = HashToFloat4D(hash1011);
            float noise0111 = HashToFloat4D(hash0111);
            float noise1111 = HashToFloat4D(hash1111);

            // Smooth interpolation
            float tx = SmoothStep(xf);
            float ty = SmoothStep(yf);
            float tz = SmoothStep(zf);
            float tw = SmoothStep(wf);

            float noise000 = math.lerp(noise0000, noise1000, tx);
            float noise100 = math.lerp(noise0100, noise1100, tx);
            float noise010 = math.lerp(noise0010, noise1010, tx);
            float noise110 = math.lerp(noise0110, noise1110, tx);
            float noise001 = math.lerp(noise0001, noise1001, tx);
            float noise101 = math.lerp(noise0101, noise1101, tx);
            float noise011 = math.lerp(noise0011, noise1011, tx);
            float noise111 = math.lerp(noise0111, noise1111, tx);

            float noise00 = math.lerp(noise000, noise100, ty);
            float noise10 = math.lerp(noise010, noise110, ty);
            float noise01 = math.lerp(noise001, noise101, ty);
            float noise11 = math.lerp(noise011, noise111, ty);

            float noise0 = math.lerp(noise00, noise10, tz);
            float noise1 = math.lerp(noise01, noise11, tz);

            return math.lerp(noise0, noise1, tw);
        }

        /// <summary>
        /// Value noise for 1D (returns values between 0 and 1)
        /// </summary>
        [BurstCompile]
        public static float ValueNoise1D(uint seed, float x)
        {
            return (Noise1D(seed, x) + 1.0f) * 0.5f;
        }

        /// <summary>
        /// Value noise for 2D (returns values between 0 and 1)
        /// </summary>
        [BurstCompile]
        public static float ValueNoise2D(uint seed, in float2 pos)
        {
            return (Noise2D(seed, pos) + 1.0f) * 0.5f;
        }

        /// <summary>
        /// Value noise for 3D (returns values between 0 and 1)
        /// </summary>
        [BurstCompile]
        public static float ValueNoise3D(uint seed, in float3 pos)
        {
            return (Noise3D(seed, pos) + 1.0f) * 0.5f;
        }

        /// <summary>
        /// Fractal noise (multiple octaves) for 1D
        /// </summary>
        [BurstCompile]
        public static float FractalNoise1D(uint seed, float x, int octaves = 4, float frequency = 1.0f, float amplitude = 1.0f, float lacunarity = 2.0f, float persistence = 0.5f)
        {
            float result = 0.0f;
            float currentAmplitude = amplitude;
            float currentFrequency = frequency;
            float maxAmplitude = 0.0f;

            for (int i = 0; i < octaves; i++)
            {
                result += Noise1D(seed + (uint)i, x * currentFrequency) * currentAmplitude;
                maxAmplitude += currentAmplitude;
                currentAmplitude *= persistence;
                currentFrequency *= lacunarity;
            }

            // Normalize the result
            return result / maxAmplitude;
        }

        /// <summary>
        /// Fractal noise (multiple octaves) for 2D
        /// </summary>
        [BurstCompile]
        public static float FractalNoise2D(uint seed, in float2 pos, int octaves = 4, float frequency = 1.0f, float amplitude = 1.0f, float lacunarity = 2.0f, float persistence = 0.5f)
        {
            float result = 0.0f;
            float currentAmplitude = amplitude;
            float currentFrequency = frequency;
            float maxAmplitude = 0.0f;

            for (int i = 0; i < octaves; i++)
            {
                result += Noise2D(seed + (uint)i, pos * currentFrequency) * currentAmplitude;
                maxAmplitude += currentAmplitude;
                currentAmplitude *= persistence;
                currentFrequency *= lacunarity;
            }

            // Normalize the result
            return result / maxAmplitude;
        }

        /// <summary>
        /// Fractal noise (multiple octaves) for 3D
        /// </summary>
        [BurstCompile]
        public static float FractalNoise3D(uint seed, in float3 pos, int octaves = 4, float frequency = 1.0f, float amplitude = 1.0f, float lacunarity = 2.0f, float persistence = 0.5f)
        {
            float result = 0.0f;
            float currentAmplitude = amplitude;
            float currentFrequency = frequency;
            float maxAmplitude = 0.0f;

            for (int i = 0; i < octaves; i++)
            {
                result += Noise3D(seed + (uint)i, pos * currentFrequency) * currentAmplitude;
                maxAmplitude += currentAmplitude;
                currentAmplitude *= persistence;
                currentFrequency *= lacunarity;
            }

            // Normalize the result
            return result / maxAmplitude;
        }
        #endregion
        #region Cellular
        /// <summary>
        /// Cellular/Worley noise for 2D
        /// </summary>
        [BurstCompile]
        public static float CellularNoise2D(uint seed, in float2 pos)
        {
            int2 cell = (int2)math.floor(pos);
            float2 localPos = pos - cell;

            float minDistance = float.MaxValue;

            // Check current cell and neighboring cells
            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    int2 neighborCell = cell + new int2(x, y);
                    uint hash = Hash(seed, neighborCell.x, neighborCell.y);

                    // Generate feature point within the cell
                    float2 featurePoint = new float2(
                        (float)(hash & 0xFFFF) / 65536.0f,
                        (float)((hash >> 16) & 0xFFFF) / 65536.0f
                    );

                    float2 distance = localPos - new float2(x, y) - featurePoint;
                    float distSq = math.lengthsq(distance);

                    if (distSq < minDistance)
                    {
                        minDistance = distSq;
                    }
                }
            }

            // Normalize the result to be in a reasonable range
            return math.sqrt(minDistance) * 2.0f - 1.0f;
        }

        /// <summary>
        /// Cellular/Worley noise for 3D
        /// </summary>
        [BurstCompile]
        public static float CellularNoise3D(uint seed, in float3 pos)
        {
            float3 scaledPos = pos;
            int3 cell = (int3)math.floor(scaledPos);
            float3 localPos = scaledPos - cell;

            float minDistance = float.MaxValue;

            // Check current cell and neighboring cells
            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    for (int z = -1; z <= 1; z++)
                    {
                        int3 neighborCell = cell + new int3(x, y, z);
                        uint hash = Hash(seed, neighborCell.x, neighborCell.y, neighborCell.z);

                        // Generate feature point within the cell
                        float3 featurePoint = new float3(
                            (float)(hash & 0xFF) / 256.0f,
                            (float)((hash >> 8) & 0xFF) / 256.0f,
                            (float)((hash >> 16) & 0xFF) / 256.0f
                        );

                        float3 distance = localPos - new float3(x, y, z) - featurePoint;
                        float distSq = math.lengthsq(distance);

                        if (distSq < minDistance)
                        {
                            minDistance = distSq;
                        }
                    }
                }
            }

            // Normalize the result to be in a reasonable range
            return math.sqrt(minDistance) * 2.0f - 1.0f;
        }
        /// <summary>
        /// Value Cellular noise for 2D (returns values between 0 and 1)
        /// </summary>
        [BurstCompile]
        public static float ValueCellularNoise2D(uint seed, in float2 pos)
        {
            return (CellularNoise2D(seed, pos) + 1.0f) * 0.5f;
        }
        /// <summary>
        /// Value Cellular noise for 3D (returns values between 0 and 1)
        /// </summary>
        [BurstCompile]
        public static float ValueCellularNoise3D(uint seed, in float3 pos)
        {
            return (CellularNoise3D(seed, pos) + 1.0f) * 0.5f;
        }

        /// <summary>
        /// Fractal Cellular noise for 2D (multiple octaves)
        /// </summary>
        [BurstCompile]
        public static float FractalCellularNoise2D(uint seed, in float2 pos, int octaves = 4, float frequency = 1.0f, float amplitude = 1.0f, float lacunarity = 2.0f, float persistence = 0.5f)
        {
            float result = 0.0f;
            float currentAmplitude = amplitude;
            float currentFrequency = frequency;
            float maxAmplitude = 0.0f;

            for (int i = 0; i < octaves; i++)
            {
                result += CellularNoise2D(seed + (uint)i, pos * currentFrequency) * currentAmplitude;
                maxAmplitude += currentAmplitude;
                currentAmplitude *= persistence;
                currentFrequency *= lacunarity;
            }

            // Normalize the result
            return result / maxAmplitude;
        }

        /// <summary>
        /// Fractal Cellular noise for 3D (multiple octaves)
        /// </summary>
        [BurstCompile]
        public static float FractalCellularNoise3D(uint seed, in float3 pos, int octaves = 4, float frequency = 1.0f, float amplitude = 1.0f, float lacunarity = 2.0f, float persistence = 0.5f)
        {
            float result = 0.0f;
            float currentAmplitude = amplitude;
            float currentFrequency = frequency;
            float maxAmplitude = 0.0f;

            for (int i = 0; i < octaves; i++)
            {
                result += CellularNoise3D(seed + (uint)i, pos * currentFrequency) * currentAmplitude;
                maxAmplitude += currentAmplitude;
                currentAmplitude *= persistence;
                currentFrequency *= lacunarity;
            }

            // Normalize the result
            return result / maxAmplitude;
        }
        #endregion
        #region Ridge
        /// <summary>
        /// Ridge noise for 1D (creates sharp ridges)
        /// </summary>
        [BurstCompile]
        public static float RidgeNoise1D(uint seed, float x)
        {
            float noise = Noise1D(seed, x);
            return 1.0f - math.abs(noise);
        }

        /// <summary>
        /// Ridge noise for 2D (creates sharp ridges)
        /// </summary>
        [BurstCompile]
        public static float RidgeNoise2D(uint seed, in float2 pos)
        {
            float noise = Noise2D(seed, pos);
            return 1.0f - math.abs(noise);
        }

        /// <summary>
        /// Ridge noise for 3D (creates sharp ridges)
        /// </summary>
        [BurstCompile]
        public static float RidgeNoise3D(uint seed, in float3 pos)
        {
            float noise = Noise3D(seed, pos);
            return 1.0f - math.abs(noise);
        }

        /// <summary>
        /// Value Ridge noise for 2D (returns values between 0 and 1)
        /// </summary>
        [BurstCompile]
        public static float ValueRidgeNoise1D(uint seed, float pos)
        {
            return (RidgeNoise1D(seed, pos) + 1.0f) * 0.5f;
        }
        /// <summary>
        /// Value Ridge noise for 2D (returns values between 0 and 1)
        /// </summary>
        [BurstCompile]
        public static float ValueRidgeNoise2D(uint seed, in float2 pos)
        {
            return (RidgeNoise2D(seed, pos) + 1.0f) * 0.5f;
        }
        /// <summary>
        /// Value Ridge noise for 3D (returns values between 0 and 1)
        /// </summary>
        [BurstCompile]
        public static float ValueRidgeNoise3D(uint seed, in float3 pos)
        {
            return (RidgeNoise3D(seed, pos) + 1.0f) * 0.5f;
        }

        /// <summary>
        /// Fractal Ridge noise for 1D (multiple octaves)
        /// </summary>
        [BurstCompile]
        public static float FractalRidgeNoise1D(uint seed, float x, int octaves = 4, float frequency = 1.0f, float amplitude = 1.0f, float lacunarity = 2.0f, float persistence = 0.5f)
        {
            float result = 0.0f;
            float currentAmplitude = amplitude;
            float currentFrequency = frequency;
            float maxAmplitude = 0.0f;

            for (int i = 0; i < octaves; i++)
            {
                result += RidgeNoise1D(seed + (uint)i, x * currentFrequency) * currentAmplitude;
                maxAmplitude += currentAmplitude;
                currentAmplitude *= persistence;
                currentFrequency *= lacunarity;
            }

            // Normalize the result
            return result / maxAmplitude;
        }

        /// <summary>
        /// Fractal Ridge noise for 2D (multiple octaves)
        /// </summary>
        [BurstCompile]
        public static float FractalRidgeNoise2D(uint seed, in float2 pos, int octaves = 4, float frequency = 1.0f, float amplitude = 1.0f, float lacunarity = 2.0f, float persistence = 0.5f)
        {
            float result = 0.0f;
            float currentAmplitude = amplitude;
            float currentFrequency = frequency;
            float maxAmplitude = 0.0f;

            for (int i = 0; i < octaves; i++)
            {
                result += RidgeNoise2D(seed + (uint)i, pos * currentFrequency) * currentAmplitude;
                maxAmplitude += currentAmplitude;
                currentAmplitude *= persistence;
                currentFrequency *= lacunarity;
            }

            // Normalize the result
            return result / maxAmplitude;
        }

        /// <summary>
        /// Fractal Ridge noise for 3D (multiple octaves)
        /// </summary>
        [BurstCompile]
        public static float FractalRidgeNoise3D(uint seed, in float3 pos, int octaves = 4, float frequency = 1.0f, float amplitude = 1.0f, float lacunarity = 2.0f, float persistence = 0.5f)
        {
            float result = 0.0f;
            float currentAmplitude = amplitude;
            float currentFrequency = frequency;
            float maxAmplitude = 0.0f;

            for (int i = 0; i < octaves; i++)
            {
                result += RidgeNoise3D(seed + (uint)i, pos * currentFrequency) * currentAmplitude;
                maxAmplitude += currentAmplitude;
                currentAmplitude *= persistence;
                currentFrequency *= lacunarity;
            }

            // Normalize the result
            return result / maxAmplitude;
        }
        #endregion
        #region Simplex
        /// <summary>
        /// 2D Simplex noise function with seed
        /// </summary>
        [BurstCompile]
        public static float SimplexNoise2D(uint seed, in float2 pos)
        {
            float2 input = pos;
            
            // Skew the input space to determine which simplex cell we're in
            float s = (input.x + input.y) * SIMPLEX_F2;
            float2 skewed = input + s;
            int2 i = (int2)math.floor(skewed);
            
            // Unskew the cell origin back to (x,y) space
            float t = (i.x + i.y) * SIMPLEX_G2;
            float2 unskewed = new float2(i.x - t, i.y - t);
            float2 d0 = input - unskewed;
            
            // For the 2D case, the simplex shape is an equilateral triangle.
            // Determine which simplex we are in.
            int2 i1;
            if (d0.x > d0.y)
            {
                i1 = new int2(1, 0); // lower triangle, XY order: (0,0)->(1,0)->(1,1)
            }
            else
            {
                i1 = new int2(0, 1); // upper triangle, YX order: (0,0)->(0,1)->(1,1)
            }
            
            // A step of (1,0) in (i,j) means a step of (1-c,-c) in (x,y), and
            // a step of (0,1) in (i,j) means a step of (-c,1-c) in (x,y), where
            // c = (3-sqrt(3))/6
            float2 d1 = d0 - i1 + SIMPLEX_G2;
            float2 d2 = d0 - 1.0f + 2.0f * SIMPLEX_G2;
            
            // Work out the hashed gradient indices of the three simplex corners
            uint n0 = Hash(seed, i.x, i.y);
            uint n1 = Hash(seed, i.x + i1.x, i.y + i1.y);
            uint n2 = Hash(seed, i.x + 1, i.y + 1);
            
            // Calculate the contribution from the three corners
            float t0 = 0.5f - math.lengthsq(d0);
            float t1 = 0.5f - math.lengthsq(d1);
            float t2 = 0.5f - math.lengthsq(d2);
            
            float noise = 0.0f;
            
            if (t0 >= 0.0f)
            {
                t0 *= t0;
                noise += t0 * t0 * Grad2D(seed, n0, d0);
            }
            
            if (t1 >= 0.0f)
            {
                t1 *= t1;
                noise += t1 * t1 * Grad2D(seed, n1, d1);
            }
            
            if (t2 >= 0.0f)
            {
                t2 *= t2;
                noise += t2 * t2 * Grad2D(seed, n2, d2);
            }
            
            // The result is scaled to return values in the interval [-1,1].
            return noise * 70.0f;
        }

        /// <summary>
        /// 3D Simplex noise function with seed
        /// </summary>
        [BurstCompile]
        public static float SimplexNoise3D(uint seed, in float3 pos)
        {
            float3 input = pos;
            
            // Skew the input space to determine which simplex cell we're in
            float s = (input.x + input.y + input.z) * SIMPLEX_F3;
            float3 skewed = input + s;
            int3 i = (int3)math.floor(skewed);
            
            // Unskew the cell origin back to (x,y,z) space
            float t = (i.x + i.y + i.z) * SIMPLEX_G3;
            float3 unskewed = new float3(i.x - t, i.y - t, i.z - t);
            float3 d0 = input - unskewed;
            
            // For the 3D case, the simplex shape is a slightly irregular tetrahedron.
            // Determine which simplex we are in.
            int3 i1, i2;
            if (d0.x >= d0.y)
            {
                if (d0.y >= d0.z)
                {
                    i1 = new int3(1, 0, 0);
                    i2 = new int3(1, 1, 0);
                }
                else if (d0.x >= d0.z)
                {
                    i1 = new int3(1, 0, 0);
                    i2 = new int3(1, 0, 1);
                }
                else
                {
                    i1 = new int3(0, 0, 1);
                    i2 = new int3(1, 0, 1);
                }
            }
            else
            {
                if (d0.y < d0.z)
                {
                    i1 = new int3(0, 0, 1);
                    i2 = new int3(0, 1, 1);
                }
                else if (d0.x < d0.z)
                {
                    i1 = new int3(0, 1, 0);
                    i2 = new int3(0, 1, 1);
                }
                else
                {
                    i1 = new int3(0, 1, 0);
                    i2 = new int3(1, 1, 0);
                }
            }
            
            // A step of (1,0,0) in (i,j,k) means a step of (1-c,-c,-c) in (x,y,z),
            // a step of (0,1,0) in (i,j,k) means a step of (-c,1-c,-c) in (x,y,z), and
            // a step of (0,0,1) in (i,j,k) means a step of (-c,-c,1-c) in (x,y,z), where
            // c = 1/6.
            float3 d1 = d0 - i1 + SIMPLEX_G3;
            float3 d2 = d0 - i2 + 2.0f * SIMPLEX_G3;
            float3 d3 = d0 - 1.0f + 3.0f * SIMPLEX_G3;
            
            // Work out the hashed gradient indices of the four simplex corners
            uint n0 = Hash(seed, i.x, i.y, i.z);
            uint n1 = Hash(seed, i.x + i1.x, i.y + i1.y, i.z + i1.z);
            uint n2 = Hash(seed, i.x + i2.x, i.y + i2.y, i.z + i2.z);
            uint n3 = Hash(seed, i.x + 1, i.y + 1, i.z + 1);
            
            // Calculate the contribution from the four corners
            float t0 = 0.6f - math.lengthsq(d0);
            float t1 = 0.6f - math.lengthsq(d1);
            float t2 = 0.6f - math.lengthsq(d2);
            float t3 = 0.6f - math.lengthsq(d3);
            
            float noise = 0.0f;
            
            if (t0 >= 0.0f)
            {
                t0 *= t0;
                noise += t0 * t0 * Grad3D(seed, n0, d0);
            }
            
            if (t1 >= 0.0f)
            {
                t1 *= t1;
                noise += t1 * t1 * Grad3D(seed, n1, d1);
            }
            
            if (t2 >= 0.0f)
            {
                t2 *= t2;
                noise += t2 * t2 * Grad3D(seed, n2, d2);
            }
            
            if (t3 >= 0.0f)
            {
                t3 *= t3;
                noise += t3 * t3 * Grad3D(seed, n3, d3);
            }
            
            // The result is scaled to return values in the interval [-1,1].
            return noise * 32.0f;
        }

        /// <summary>
        /// 1D Simplex noise function with seed
        /// </summary>
        [BurstCompile]
        public static float SimplexNoise1D(uint seed, float x)
        {
            // For 1D, we can use a simplified approach or fall back to regular noise
            // Since true 1D simplex noise is complex, we'll use a simplified version
            return Noise1D(seed, x);
        }

        /// <summary>
        /// Helper function for 2D gradient calculation
        /// </summary>
        [BurstCompile]
        private static float Grad2D(uint seed, uint hash, in float2 d)
        {
            // Use the hash to select a gradient direction
            uint h = hash & 0x7;
            float u = h < 4 ? d.x : d.y;
            float v = h < 4 ? d.y : d.x;
            return ((h & 1) != 0 ? -u : u) + ((h & 2) != 0 ? -2.0f * v : 2.0f * v);
        }

        /// <summary>
        /// Helper function for 3D gradient calculation
        /// </summary>
        [BurstCompile]
        private static float Grad3D(uint seed, uint hash, in float3 d)
        {
            // Use the hash to select a gradient direction from the gradient table
            uint h = hash & 0xF; // 12 possible gradients (0-11), but we use 0xF to get 0-15
            if (h >= 12) h = h % 12; // Ensure we stay within bounds
            int3 grad = SIMPLEX_GRAD3[h];
            return grad.x * d.x + grad.y * d.y + grad.z * d.z;
        }

        /// <summary>
        /// Value simplex noise for 1D (returns values between 0 and 1)
        /// </summary>
        [BurstCompile]
        public static float ValueSimplexNoise1D(uint seed, float x)
        {
            return (SimplexNoise1D(seed, x) + 1.0f) * 0.5f;
        }

        /// <summary>
        /// Value simplex noise for 2D (returns values between 0 and 1)
        /// </summary>
        [BurstCompile]
        public static float ValueSimplexNoise2D(uint seed, in float2 pos)
        {
            return (SimplexNoise2D(seed, pos) + 1.0f) * 0.5f;
        }

        /// <summary>
        /// Value simplex noise for 3D (returns values between 0 and 1)
        /// </summary>
        [BurstCompile]
        public static float ValueSimplexNoise3D(uint seed, in float3 pos)
        {
            return (SimplexNoise3D(seed, pos) + 1.0f) * 0.5f;
        }

        /// <summary>
        /// Fractal simplex noise (multiple octaves) for 1D (returns values between 0 and 1)
        /// </summary>
        [BurstCompile]
        public static float FractalSimplexNoise1D(uint seed, float x, int octaves = 4, float frequency = 1.0f, float amplitude = 1.0f, float lacunarity = 2.0f, float persistence = 0.5f)
        {
            float result = 0.0f;
            float currentAmplitude = amplitude;
            float currentFrequency = frequency;
            float maxAmplitude = 0.0f;

            for (int i = 0; i < octaves; i++)
            {
                result += SimplexNoise1D(seed + (uint)i, x * currentFrequency) * currentAmplitude;
                maxAmplitude += currentAmplitude;
                currentAmplitude *= persistence;
                currentFrequency *= lacunarity;
            }

            // Normalize the result
            return result / maxAmplitude;
        }

        /// <summary>
        /// Fractal simplex noise (multiple octaves) for 2D
        /// </summary>
        [BurstCompile]
        public static float FractalSimplexNoise2D(uint seed, in float2 pos, int octaves = 4, float frequency = 1.0f, float amplitude = 1.0f, float lacunarity = 2.0f, float persistence = 0.5f)
        {
            float result = 0.0f;
            float currentAmplitude = amplitude;
            float currentFrequency = frequency;
            float maxAmplitude = 0.0f;

            for (int i = 0; i < octaves; i++)
            {
                result += ValueSimplexNoise2D(seed + (uint)i, pos * currentFrequency) * currentAmplitude;
                maxAmplitude += currentAmplitude;
                currentAmplitude *= persistence;
                currentFrequency *= lacunarity;
            }

            // Normalize the result
            return result / maxAmplitude;
        }

        /// <summary>
        /// Fractal simplex noise (multiple octaves) for 3D
        /// </summary>
        [BurstCompile]
        public static float FractalSimplexNoise3D(uint seed, in float3 pos, int octaves = 4, float frequency = 1.0f, float amplitude = 1.0f, float lacunarity = 2.0f, float persistence = 0.5f)
        {
            float result = 0.0f;
            float currentAmplitude = amplitude;
            float currentFrequency = frequency;
            float maxAmplitude = 0.0f;

            for (int i = 0; i < octaves; i++)
            {
                result += SimplexNoise3D(seed + (uint)i, pos * currentFrequency) * currentAmplitude;
                maxAmplitude += currentAmplitude;
                currentAmplitude *= persistence;
                currentFrequency *= lacunarity;
            }

            // Normalize the result
            return result / maxAmplitude;
        }
        #endregion
    }
}