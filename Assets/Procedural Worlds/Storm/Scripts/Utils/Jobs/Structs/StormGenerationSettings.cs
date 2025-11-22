using Unity.Mathematics;
using Unity.Collections;

/// Copyright 2023 Procedural Worlds Ltd
/// Written by Jonathan Williams as part of Storm

namespace ProceduralWorlds.Storm.Utils
{
    /// <summary>
    /// Contains settings used by Generation Job
    /// </summary>
    public struct StormGenerationSettings
    {
        #region Generation Options
        /// <summary>
        /// Amount (of space between items) to adjust position of items randomly
        /// </summary>
        [ReadOnly]
        public float4 PositionJitter;
        /// <summary>
        /// Number of items to generate along each axis
        /// </summary>
        [ReadOnly]
        public float Density;
        /// <summary>
        /// Splat value to stop generating items
        /// </summary>
        [ReadOnly]
        public float SplatCutoff;
        /// <summary>
        /// Splat value at which maximum scale is reached (values over this will be max scale)
        /// </summary>
        [ReadOnly]
        public float SplatMax;
        /// <summary>
        /// Chance to spawn this feature multiplied by the input data
        /// </summary>
        [ReadOnly]
        public float SpawnChance;

        /// <summary>
        /// Minimum Height value to allow spawning
        /// </summary>
        [ReadOnly]
        public float MaxHeight;
        /// <summary>
        /// Maximum Height value to allow spawning
        /// </summary>
        [ReadOnly]
        public float MinHeight;
        #endregion

        #region Scale Options
        /// <summary>
        /// Minimum scale value
        /// </summary>
        [ReadOnly]
        public float MinScale;
        /// <summary>
        /// Maximum scale value
        /// </summary>
        [ReadOnly]
        public float MaxScale;
        /// <summary>
        /// Minimum Random value for scale
        /// </summary>
        [ReadOnly]
        public float RandomMinScale;
        /// <summary>
        /// Maximum Random value for scale
        /// </summary>
        [ReadOnly]
        public float RandomMaxScale;
        /// <summary>
        /// Minimum slope value to allow spawning
        /// </summary>
        [ReadOnly]
        public float MinSlope;
        /// <summary>
        /// Maximum slope value to allow spawning
        /// </summary>
        [ReadOnly]
        public float MaxSlope;
        public bool LimitMinHeight;
        public bool LimitMaxHeight;
        public bool LimitMinSlope;
        public bool LimitMaxSlope;
        /// <summary>
        /// Should items scale by Splat
        /// </summary>
        [ReadOnly]
        public bool useScaleBySplat;
        /// <summary>
        /// Should items scale by Height
        /// </summary>
        [ReadOnly]
        public bool useScaleByHeight;
        /// <summary>
        /// Should items scale by Slope
        /// </summary>
        [ReadOnly]
        public bool useScaleBySlope;
        /// <summary>
        /// Samples from ScaleBySplat curve
        /// </summary>
        [ReadOnly]
        public NativeArray<float> ScaleBySplat;
        /// <summary>
        /// Samples from ScaleByHeight curve
        /// </summary>
        [ReadOnly]
        public NativeArray<float> ScaleByHeight;
        /// <summary>
        /// Samples from ScaleBySlope curve
        /// </summary>
        [ReadOnly]
        public NativeArray<float> ScaleBySlope;
        #endregion

        #region Rotations
        /// <summary>
        /// Amount to rotate all items
        /// </summary>
        [ReadOnly]
        public float3 RotBase;
        /// <summary>
        /// Amount to randomly rotate all items
        /// </summary>
        [ReadOnly]
        public float3 RotRange;
        /// <summary>
        /// Size of area to check for slope calculations
        /// </summary>
        [ReadOnly]
        public float SlopeCheckSize;
        /// <summary>
        /// Amount to push rotation toward identity
        /// </summary>
        [ReadOnly]
        public float AlignToUp;
        /// <summary>
        /// Amount to push rotation toward slope direction
        /// </summary>
        [ReadOnly]
        public float AlignForwardToSlope;
        /// <summary>
        /// Which map to sample for slope calculations
        /// </summary>
        [ReadOnly]
        public SlopeType SlopeType;
        /// <summary>
        /// Type of Noise to be used for slope
        /// </summary>
        [ReadOnly]
        public RandomType SlopeNoiseType;
        /// <summary>
        /// Multiplier to apply to noise uv coordinates for slope
        /// </summary>
        [ReadOnly]
        public float SlopeNoiseSize;
        /// <summary>
        /// Multiplier to apply to noise uv coordinates for slope
        /// </summary>
        [ReadOnly]
        public float SlopeNoiseLacunarity;
        /// <summary>
        /// Multiplier to apply to noise uv coordinates for slope
        /// </summary>
        [ReadOnly]
        public float SlopeNoisePersistence;
        /// <summary>
        /// Multiplier to apply to noise values for slope
        /// </summary>
        [ReadOnly]
        public float SlopeNoiseMult;
        /// <summary>
        /// Modifier to apply to noise values for slope
        /// </summary>
        [ReadOnly]
        public float SlopeNoiseMod;
        /// <summary>
        /// Modifier to apply to noise values for slope
        /// </summary>
        [ReadOnly]
        public int SlopeNoiseOctaves;
        /// <summary>
        /// Type of Noise2 to be used for slope
        /// </summary>
        [ReadOnly]
        public RandomType SlopeNoise2Type;
        /// <summary>
        /// Method to use to apply Noise2 for slope
        /// </summary>
        [ReadOnly]
        public CombineMethod SlopeNoise2Method;
        /// <summary>
        /// Multiplier to apply to noise uv coordinates for slope noise2
        /// </summary>
        [ReadOnly]
        public float SlopeNoise2Size;
        /// <summary>
        /// Multiplier to apply to noise uv coordinates for slope noise2
        /// </summary>
        [ReadOnly]
        public float SlopeNoise2Lacunarity;
        /// <summary>
        /// Multiplier to apply to noise uv coordinates for slope noise2
        /// </summary>
        [ReadOnly]
        public float SlopeNoise2Persistence;
        /// <summary>
        /// Multiplier to apply to noise values for slope noise2
        /// </summary>
        [ReadOnly]
        public float SlopeNoise2Mult;
        /// <summary>
        /// Modifier to apply to noise values for slope noise2
        /// </summary>
        [ReadOnly]
        public float SlopeNoise2Mod;
        /// <summary>
        /// Modifier to apply to noise values for slope noise2
        /// </summary>
        [ReadOnly]
        public int SlopeNoise2Octaves;
        #endregion

        #region Randomization
        /// <summary>
        /// Seed to use for Randomization
        /// </summary>
        [ReadOnly]
        public int RandomSeed;
        /// <summary>
        /// Type of Noise to be applied to scale
        /// </summary>
        [ReadOnly]
        public RandomType ScaleNoiseType;
        /// <summary>
        /// Multiplier to apply to noise uv coordinates for scale
        /// </summary>
        [ReadOnly]
        public float ScaleNoiseSize;
        /// <summary>
        /// Multiplier to apply to noise uv coordinates for scale
        /// </summary>
        [ReadOnly]
        public float ScaleNoiseLacunarity;
        /// <summary>
        /// Multiplier to apply to noise uv coordinates for scale
        /// </summary>
        [ReadOnly]
        public float ScaleNoisePersistence;
        /// <summary>
        /// Multiplier to apply to noise values for scale
        /// </summary>
        [ReadOnly]
        public float ScaleNoiseMult;
        /// <summary>
        /// Modifier to apply to noise values for scale
        /// </summary>
        [ReadOnly]
        public float ScaleNoiseMod;
        /// <summary>
        /// Modifier to apply to noise values for scale
        /// </summary>
        [ReadOnly]
        public int ScaleNoiseOctaves;
        /// <summary>
        /// Type of Noise2 to be applied to scale
        /// </summary>
        [ReadOnly]
        public RandomType ScaleNoise2Type;
        /// <summary>
        /// Method to use to apply Noise2
        /// </summary>
        [ReadOnly]
        public CombineMethod ScaleNoise2Method;
        /// <summary>
        /// Multiplier to apply to noise2 uv coordinates for scale
        /// </summary>
        [ReadOnly]
        public float ScaleNoise2Size;
        /// <summary>
        /// Multiplier to apply to noise2 uv coordinates for scale
        /// </summary>
        [ReadOnly]
        public float ScaleNoise2Lacunarity;
        /// <summary>
        /// Multiplier to apply to noise2 uv coordinates for scale
        /// </summary>
        [ReadOnly]
        public float ScaleNoise2Persistence;
        /// <summary>
        /// Multiplier to apply to noise values for scale2
        /// </summary>
        [ReadOnly]
        public float ScaleNoise2Mult;
        /// <summary>
        /// Modifier to apply to noise values for scale2
        /// </summary>
        [ReadOnly]
        public float ScaleNoise2Mod;
        /// <summary>
        /// Modifier to apply to noise values for scale2
        /// </summary>
        [ReadOnly]
        public int ScaleNoise2Octaves;
        /// <summary>
        /// Offset to apply to noise2 uv coordinates
        /// </summary>

        /// <summary>
        /// Amount to randomize color from instance color value
        /// </summary>
        [ReadOnly]
        public float ColorRandomization;
        /// <summary>
        /// Samples from Color Transition curve
        /// </summary>
        [ReadOnly]
        public NativeArray<float> ColorTransition;

        /// <summary>
        /// Offset to apply to all noise uv coordinates
        /// </summary>
        [ReadOnly]
        public float2 NoiseOffset;
        /// <summary>
        /// Scale to apply to all noise uv coordinates
        /// </summary>
        [ReadOnly]
        public float2 NoiseSize;
        #endregion

        public bool IsValid()
        {
            return ColorTransition.IsCreated && ScaleByHeight.IsCreated && ScaleBySlope.IsCreated && ScaleBySplat.IsCreated;
        }
        public string InvalidReason()
        {
            string ret = "";
            if (!ColorTransition.IsCreated)
                ret += "ColorTransition, ";
            if (!ScaleByHeight.IsCreated)
                ret += "ScaleByHeight, ";
            if (!ScaleBySlope.IsCreated)
                ret += "ScaleBySlope, ";
            if (!ScaleBySplat.IsCreated)
                ret += "ScaleBySplat, ";
            return ret;
        }
    }
}