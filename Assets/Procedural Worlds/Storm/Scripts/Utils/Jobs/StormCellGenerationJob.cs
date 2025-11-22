using System;
using Unity.Jobs;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Burst;

/// Copyright 2023 Procedural Worlds Ltd
/// Written by Jonathan Williams as part of Storm

namespace ProceduralWorlds.Storm.Utils
{
    /// <summary>
    /// Iterates through possible spawn positions generating instances for rendering
    /// </summary>
    [BurstCompile]
#if UNITY_2022_2_OR_NEWER
    [GenerateTestsForBurstCompatibility(CompileTarget = GenerateTestsForBurstCompatibilityAttribute.BurstCompatibleCompileTarget.PlayerAndEditor)]
#else
    [BurstCompatible(CompileTarget = BurstCompatibleAttribute.BurstCompatibleCompileTarget.PlayerAndEditor)]
#endif
    public struct StormCellGenerationJob : IJob
    {
        float2 SAMPLE_STEP;

        /// <summary>
        /// Position of tile in world space
        /// </summary>
        [ReadOnly]
        public double2 CellPosition;

        /// <summary>
        /// Size of tile in world space
        /// </summary>
        [ReadOnly]
        public float2 CellSize;

        /// <summary>
        /// World Space added to the edges of Height maps for this job
        /// </summary>
        [ReadOnly]
        public int2 HeightPadding;
        /// <summary>
        /// World Space added to the edges of Splat maps for this job
        /// </summary>
        [ReadOnly]
        public int2 SplatPadding;

        /// <summary>
        /// Array of height values (0-1)
        /// </summary>
        [ReadOnly]
        public NativeArray<float> Height;
        /// <summary>
        /// Offset applied to Height values for sea-level or other modifiers
        /// </summary>
        public float HeightOffset;
        /// <summary>
        /// Resolution of Height array along X axis
        /// </summary>
        [ReadOnly]
        public int2 HeightResolution;
        /// <summary>
        /// Array of Splat values (0-1)
        /// </summary>
        [ReadOnly]
        public NativeArray<float> Splat;
        /// <summary>
        /// Resolution of Splat array along X axis
        /// </summary>
        [ReadOnly]
        public int2 SplatResolution;

        [ReadOnly]
        public StormGenerationSettings Settings;

        /// <summary>
        /// List of items generated
        /// </summary>
        [WriteOnly]
        public NativeList<StormItemData> Items;

        public bool IsValid()
        {
            return Height.IsCreated && Items.IsCreated && Splat.IsCreated && Settings.IsValid();
        }
        public string InvalidReason()
        {
            string ret = "";
            if (!Height.IsCreated || !Items.IsCreated || !Splat.IsCreated)
            {
                ret += "Invalid Arrays: ";
                if (!Height.IsCreated)
                    ret += "Height, ";
                if (!Items.IsCreated)
                    ret += "Items, ";
                if (!Splat.IsCreated)
                    ret += "Splat, ";
            }
            ret += Settings.InvalidReason();
            return ret;
        }

        //RNG state values
        private ulong m_rngSeed;
        private ulong m_rngStateA, m_rngStateB, m_rngStateC, m_rngStateD;
        
        // Improved xoshiro256** implementation for better statistical properties
        public float NextRNG()
        {
            ulong result = ((m_rngStateA * 5) << 7 | (m_rngStateA * 5) >> 57) * 9;
            ulong t = m_rngStateB << 17;
            
            m_rngStateB ^= m_rngStateA;
            m_rngStateC ^= m_rngStateB;
            m_rngStateD ^= m_rngStateC;
            m_rngStateA ^= m_rngStateD;
            
            m_rngStateB ^= t;
            m_rngStateC = (m_rngStateC << 45) | (m_rngStateC >> 19);
            m_rngStateD = (m_rngStateD << 27) | (m_rngStateD >> 37);
            
            return (float)(result / (double)ulong.MaxValue);
        }
        
        public float NextRNG(float min, float max)
        {
            return min + (NextRNG() * (max - min));
        }

        public void Execute()
        {
            //get our world step size and our first instance position
            SAMPLE_STEP = math.max(math.float2(0.0001f, 0.0001f), 100f / Settings.Density);
            float2 pos = (float2)((math.round(CellPosition / SAMPLE_STEP) * SAMPLE_STEP) - CellPosition) / CellSize;
            //convert step to local coordinates (0-1)
            SAMPLE_STEP /= CellSize;
            float startPosX = pos.x;
            //step along each instance position on Z axis until we are outside of our cell
            while (pos.y < 1f)
            {
                pos.x = startPosX;
                //step along each instance position on X axis until we are outside of our cell
                while (pos.x < 1f)
                {
                    ulong seed = (ulong)Settings.RandomSeed;
                    seed ^= (math.asulong(CellPosition.x + (pos.x * CellSize.x)) * 73856093UL) ^ (math.asulong(CellPosition.y + (pos.y * CellSize.y)) * 19349663UL);
                    SetSeed(seed);
                    Execute(pos);

                    pos.x += SAMPLE_STEP.x;
                }
                pos.y += SAMPLE_STEP.y;
            }
        }
        void SetSeed(ulong seed)
        {
            m_rngSeed = seed;
            if (m_rngSeed == 0)
                //use a preset random number to use for all 0 seeds as 0 is not allowed
                //not too close to 0 to avoid duplicate values from being close to eachother
                m_rngSeed = 8378122;
            
            // Improved seed mixing using SplitMix64 algorithm
            ulong z = m_rngSeed;
            z = (z ^ (z >> 30)) * 0xbf58476d1ce4e5b9UL;
            z = (z ^ (z >> 27)) * 0x94d049bb133111ebUL;
            m_rngStateA = z ^ (z >> 31);
            
            z = (z ^ (z >> 30)) * 0xbf58476d1ce4e5b9UL;
            z = (z ^ (z >> 27)) * 0x94d049bb133111ebUL;
            m_rngStateB = z ^ (z >> 31);
            
            z = (z ^ (z >> 30)) * 0xbf58476d1ce4e5b9UL;
            z = (z ^ (z >> 27)) * 0x94d049bb133111ebUL;
            m_rngStateC = z ^ (z >> 31);
            
            z = (z ^ (z >> 30)) * 0xbf58476d1ce4e5b9UL;
            z = (z ^ (z >> 27)) * 0x94d049bb133111ebUL;
            m_rngStateD = z ^ (z >> 31);
        }

        public void Execute(float2 pos)
        {
            //get jitter
            float2 jitter = OnCircle(NextRNG(-math.PI, math.PI), NextRNG(0, 1f)) * Settings.PositionJitter.xy * SAMPLE_STEP;
            float posYOffset = NextRNG(Settings.PositionJitter.z, Settings.PositionJitter.w);
            //get tex coord position
            float2 retPosXZ = pos + jitter;

            //get alpha and check first (most restrictive condition)
            float splatSample = SampleMap(retPosXZ);
            splatSample = math.clamp(math.unlerp(Settings.SplatCutoff, Settings.SplatMax, splatSample), 0, 1) * Settings.SpawnChance;

            // Early exit for spawn chance
            float spawnChance = NextRNG();
            if (spawnChance > splatSample)
                return; //alpha sample below our cutoff
                
            //get height values
            float retPosY = SampleHeightMap(retPosXZ);

            // Early exit for height limits
            if ((Settings.LimitMinHeight && retPosY < Settings.MinHeight + HeightOffset) || (Settings.LimitMaxHeight && retPosY > Settings.MaxHeight + HeightOffset))
                return;

            float3 slopeSample = GetHeightSlope(retPosXZ, retPosY);
            float spawnSlope = (1 - math.abs(math.dot(math.normalize(slopeSample), math.up()))) * 90;

            //check our min/max slope for spawn fitness
            if ((Settings.LimitMinSlope && spawnSlope < Settings.MinSlope) || (Settings.LimitMaxSlope && spawnSlope > Settings.MaxSlope))
                return;


            float retScale = NextRNG(Settings.RandomMinScale, Settings.RandomMaxScale);
            retScale *= GetScaleNoise(retPosXZ);

            //apply Scale By curves if set
            if (Settings.useScaleBySplat)
                retScale *= SampleCurve(Settings.ScaleBySplat, splatSample);

            if (Settings.useScaleByHeight)
                retScale *= SampleCurve(Settings.ScaleByHeight, ((retPosY - HeightOffset - Settings.MinHeight) / (Settings.MaxHeight - Settings.MinHeight)));

            if (Settings.useScaleBySlope)
                retScale *= SampleCurve(Settings.ScaleBySlope, spawnSlope / 90f);

            if (retScale <= 0)
                return; //scale is 0 or less. skip this item

            float rngRotX = NextRNG(-Settings.RotRange.x, Settings.RotRange.x);
            float rngRotY = NextRNG(-Settings.RotRange.y, Settings.RotRange.y);
            float rngRotZ = NextRNG(-Settings.RotRange.z, Settings.RotRange.z);
            float rngColor = NextRNG();
            float retRng = NextRNG();

            //get our other misc values
            float retColor = math.saturate(SampleCurve(Settings.ColorTransition, retScale) + rngColor * Settings.ColorRandomization);
            posYOffset *= math.saturate(retScale);
            retScale = math.lerp(Settings.MinScale, Settings.MaxScale, math.saturate(retScale));

            //get our Rotation
            float3 slope;
            if (Settings.SlopeType == SlopeType.Splat)
            {
                slope = GetSlope(retPosXZ, splatSample);
            }
            else if (Settings.SlopeType == SlopeType.Noise && Settings.SlopeNoiseType != RandomType.NONE)
            {
                slope = GetNoiseSlope(retPosXZ);
            }
            else //use Height
            {
                slope = slopeSample;
            }
            
            // Cache rotation calculations
            float3 slopeDir = math.down() - (math.dot(math.up(), slope) * slope);
            quaternion retRotation = UnityEngine.Quaternion.FromToRotation(math.up(), math.lerp(slope, math.up(), Settings.AlignToUp));
            retRotation = math.mul(retRotation, quaternion.LookRotationSafe(math.lerp(math.forward(), math.float3(slopeDir.x, 0, slopeDir.z), Settings.AlignForwardToSlope), math.up()));
            
            // Combine rotation calculations
            float rotY = ((Settings.RotBase.y + rngRotY) / 180f) * math.PI;
            float rotX = ((Settings.RotBase.x + rngRotX) / 180f) * math.PI;
            float rotZ = ((Settings.RotBase.z + rngRotZ) / 180f) * math.PI;
            
            retRotation = math.mul(retRotation, quaternion.RotateY(rotY));
            retRotation = math.mul(retRotation, quaternion.RotateX(rotX));
            retRotation = math.mul(retRotation, quaternion.RotateZ(rotZ));
            retRotation = math.normalizesafe(retRotation);
            //add to output lists
            Items.Add(new StormItemData() {
                Translation = math.float3(retPosXZ.x, retPosY + posYOffset, retPosXZ.y),
                Rotation = retRotation,
                Scale = new half(retScale),
                ExtraData = math.float2(retColor, retRng)
            });
        }
        private float GetScaleNoise(float2 uv)
        {
            if (Settings.ScaleNoiseType == RandomType.NONE)
                return 1;
            float noiseVal = StormNoise.SampleNoise(Settings.ScaleNoiseType, (uint)Settings.RandomSeed, (Settings.NoiseOffset + (uv * Settings.NoiseSize)), Settings.ScaleNoiseSize * 0.1f, Settings.ScaleNoiseLacunarity, Settings.ScaleNoisePersistence, Settings.ScaleNoiseMod, Settings.ScaleNoiseMult, Settings.ScaleNoiseOctaves);
            if (Settings.ScaleNoise2Type != RandomType.NONE)
            {
                float noise2Val = StormNoise.SampleNoise(Settings.ScaleNoise2Type, (uint)Settings.RandomSeed, (Settings.NoiseOffset + (uv * Settings.NoiseSize)), Settings.ScaleNoise2Size * 0.1f, Settings.ScaleNoise2Lacunarity, Settings.ScaleNoise2Persistence, Settings.ScaleNoise2Mod, Settings.ScaleNoise2Mult, Settings.ScaleNoise2Octaves);
                switch (Settings.ScaleNoise2Method)
                {
                    case CombineMethod.MULTIPLY:
                        noiseVal *= noise2Val;
                        break;
                    case CombineMethod.ADD:
                        noiseVal += noise2Val;
                        break;
                    case CombineMethod.POWER:
                        noiseVal = math.pow(noiseVal, noise2Val);
                        break;
                }
            }
            return noiseVal;
        }
        private float GetSlopeNoise(float2 uv)
        {
            if (Settings.SlopeNoiseType == RandomType.NONE)
                return 1;
            uv = Settings.NoiseOffset + (uv * Settings.NoiseSize);
            float noiseVal = StormNoise.SampleNoise(Settings.SlopeNoiseType, (uint)Settings.RandomSeed, uv, Settings.SlopeNoiseSize, Settings.SlopeNoiseLacunarity, Settings.SlopeNoisePersistence, Settings.SlopeNoiseMod, Settings.SlopeNoiseMult, Settings.SlopeNoiseOctaves);
            if (Settings.SlopeNoise2Type != RandomType.NONE)
            {
                float noise2Val = StormNoise.SampleNoise(Settings.SlopeNoise2Type, (uint)Settings.RandomSeed, uv, Settings.SlopeNoise2Size, Settings.SlopeNoise2Lacunarity, Settings.SlopeNoise2Persistence, Settings.SlopeNoise2Mod, Settings.SlopeNoise2Mult, Settings.SlopeNoise2Octaves);
                switch (Settings.SlopeNoise2Method)
                {
                    case CombineMethod.MULTIPLY:
                        noiseVal *= noise2Val;
                        break;
                    case CombineMethod.ADD:
                        noiseVal += noise2Val;
                        break;
                    case CombineMethod.POWER:
                        noiseVal = math.pow(noiseVal, noise2Val);
                        break;
                }
            }
            return noiseVal;
        }

        public static quaternion FromToRotation(float3 from, float3 to)
        {
            return UnityEngine.Quaternion.FromToRotation(from, to);
        }
        public static quaternion FromTo(float3 from, float3 to)
        {
            float3 axis = math.cross(from, to);
            float angle = Angle(from, to);
            return quaternion.AxisAngle(math.normalizesafe(axis), angle);
        }
        //Code From 'git:UnityCsReference/Runtime/Export/Math/Vector3.cs' modified for Mathematics library
        public static float Angle(float3 from, float3 to)
        {
            // sqrt(a) * sqrt(b) = sqrt(a * b) -- valid for real numbers
            float denominator = (float)Math.Sqrt(math.lengthsq(from) * math.lengthsq(to));
            if (denominator < 1e-15F)
                return 0F;

            float dot = math.clamp(math.dot(from, to) / denominator, -1F, 1F);
            return math.degrees((float)Math.Acos(dot));
        }
        private float SampleHeightMap(float2 coord)
        {
            float2 offset = float2.zero;
            return StormJobUtils.Sample(ref Height, ref HeightResolution, ref coord, ref offset, ref HeightPadding, true);
        }
        private float SampleMap(float2 coord)
        {
            float2 offset = new float2(-0.5f, -0.5f);
            return StormJobUtils.Sample(ref Splat, ref SplatResolution, ref coord, ref offset, ref SplatPadding);
        }
        private float SampleCurve(NativeArray<float> curve, float val)
        {
            val = math.clamp(val, 0f, 1f) * (curve.Length - 1);
            return math.lerp(curve[(int)math.floor(val)], curve[(int)math.ceil(val)], math.frac(val));
        }
        private float2 OnCircle(float angle, float radius)
        {
            float2 ret = math.float2(math.sin(angle), math.cos(angle)) * radius;
            return ret;
        }
        private float3 GetHeightSlope(float2 uv, float sampleVal)
        {
            float xSlope = SampleHeightMap(uv + math.float2(SAMPLE_STEP.x * Settings.SlopeCheckSize, 0));
            float zSlope = SampleHeightMap(uv + math.float2(0, SAMPLE_STEP.y * Settings.SlopeCheckSize));
            float xSlope2 = SampleHeightMap(uv - math.float2(SAMPLE_STEP.x * Settings.SlopeCheckSize, 0));
            float zSlope2 = SampleHeightMap(uv - math.float2(0, SAMPLE_STEP.y * Settings.SlopeCheckSize));
            return CalcSlope(sampleVal, xSlope, zSlope, xSlope2, zSlope2);
        }
        private float3 GetSlope(float2 uv, float sampleVal)
        {
            float xSlope = SampleMap(uv + math.float2(SAMPLE_STEP.x * Settings.SlopeCheckSize, 0));
            float zSlope = SampleMap(uv + math.float2(0, SAMPLE_STEP.y * Settings.SlopeCheckSize));
            float xSlope2 = SampleMap(uv - math.float2(SAMPLE_STEP.x * Settings.SlopeCheckSize, 0));
            float zSlope2 = SampleMap(uv - math.float2(0, SAMPLE_STEP.y * Settings.SlopeCheckSize));
            return CalcSlope(sampleVal, xSlope, zSlope, xSlope2, zSlope2);
        }
        private float3 GetNoiseSlope(float2 uv)
        {
            float sampleVal = GetSlopeNoise(uv);
            float xSlope = GetSlopeNoise(uv + math.float2(SAMPLE_STEP.x * Settings.SlopeCheckSize, 0));
            float zSlope = GetSlopeNoise(uv + math.float2(0, SAMPLE_STEP.y * Settings.SlopeCheckSize));
            float xSlope2 = GetSlopeNoise(uv - math.float2(SAMPLE_STEP.x * Settings.SlopeCheckSize, 0));
            float zSlope2 = GetSlopeNoise(uv - math.float2(0, SAMPLE_STEP.y * Settings.SlopeCheckSize));
            return CalcSlope(sampleVal, xSlope, zSlope, xSlope2, zSlope2);
        }

        private float3 CalcSlope(float midVal, float xVal, float zVal, float xVal2, float zVal2)
        {
            float3 xVec = math.float3(SAMPLE_STEP.x * Settings.SlopeCheckSize * CellSize.x, (xVal - midVal), 0);
            float3 zVec = math.float3(0, zVal - midVal, SAMPLE_STEP.y * Settings.SlopeCheckSize * CellSize.y);
            float3 xVec2 = math.float3(SAMPLE_STEP.x * Settings.SlopeCheckSize * CellSize.x, (midVal - xVal2), 0);
            float3 zVec2 = math.float3(0, midVal - zVal2, SAMPLE_STEP.y * Settings.SlopeCheckSize * CellSize.y);
            return math.normalizesafe(math.cross(zVec, xVec) + math.cross(zVec2, xVec2));
        }
    }
}