using System;
using UnityEngine;

/// Copyright 2023 Procedural Worlds Ltd
/// Written by Jonathan Williams as part of Storm

namespace ProceduralWorlds.Storm.Utils
{
    public enum StormFeatureSource : byte
    {
        CUSTOM,
        PREFAB,
        TERRAIN_DETAIL
    }
    /// <summary>
    /// Contains all settings for an item to render through Storm
    /// </summary>
    [CreateAssetMenu(menuName = "Procedural Worlds/Storm/Object Settings")]
    public class StormFeatureSettings : ScriptableObject
    {
        #region Feature Source Settings
        [HideInInspector]
        public uint Version = 0;
        public StormFeatureSource SourceType;
        public GameObject SourcePrefab = null;
        public TerrainData SourceTerrain = null;
        public int SourceDetailIndex = -1;
        public Impostor SavedImpostor = null;
        #endregion

        /// <summary>
        /// Type of Storm object this Settings object is for
        /// </summary>
        public StormFeatureType ObjectType;
        #region Generation Settings
        #region Spawn
        /// <summary>
        /// Old Jitter setting kept for updating new PositionJitter on old ScriptableObjects
        /// </summary>
        [HideInInspector]
        [Obsolete]
        public float Jitter;
        /// <summary>
        /// Amount (of space between items) to adjust position of items randomly
        /// X,Y for X/Z offset between 0-1, Z,W for Y position min/max offset in worldspace
        /// </summary>
        public Vector4 PositionJitter;
        /// <summary>
        /// Number of items to generate along each axis
        /// </summary>
        [Min(0.01f)]
        public float Density;
        /// <summary>
        /// Chance to spawn this feature multiplied by the input data
        /// </summary>
        [Range(0f, 2f)]
        public float SpawnChance = 1f;
        /// <summary>
        /// Splat value to stop generating items
        /// </summary>
        [Range(0, 1)]
        public float SplatCutoff;
        /// <summary>
        /// Splat value at which maximum scale is reached (values over this will be max scale)
        /// </summary>
        [Range(0, 1)]
        public float SplatMaxValue = 1;
        /// <summary>
        /// Should Height values ignore the world SeaLevel value?
        /// </summary>
        public bool IgnoreSeaLevel = false;
        /// <summary>
        /// Should item generation be limited by Min Height?
        /// </summary>
        public bool LimitMinHeight = true;
        /// <summary>
        /// Minimum height to use for LimitMinHeight and ScaleByHeight
        /// </summary>
        public float MinHeight = 0;
        /// <summary>
        /// Should item generation be limited by Max Height?
        /// </summary>
        public bool LimitMaxHeight = false;
        /// <summary>
        /// Maximum height to use for LimitMinHeight and ScaleByHeight
        /// </summary>
        public float MaxHeight = 2048;
        /// <summary>
        /// Should item generation be limited by Min Slope?
        /// </summary>
        public bool LimitMinSlope = false;
        /// <summary>
        /// Minimum Slope value this Feature is allowed to spawn on. (in degrees)
        /// </summary>
        [Range(0,90)]
        public float MinSlope = 0f;
        /// <summary>
        /// Should item generation be limited by Max Slope?
        /// </summary>
        public bool LimitMaxSlope = false;
        /// <summary>
        /// Maximum Slope value this Feature is allowed to spawn on. (in degrees)
        /// </summary>
        [Range(0, 90)]
        public float MaxSlope = 20f;
        #endregion
        #region Scale Options
        /// <summary>
        /// Minimum scale value
        /// </summary>
        public float MinScale;
        /// <summary>
        /// Maximum scale value
        /// </summary>
        public float MaxScale;
        /// <summary>
        /// What is the minimum scale value possible for starting value
        /// </summary>
        public float RandomScaleMin = 0.1f;
        /// <summary>
        /// What is the maximum scale value possible for starting value
        /// </summary>
        public float RandomScaleMax = 1;
        /// <summary>
        /// Should items scale by Splat
        /// </summary>
        public bool useScaleBySplat;
        /// <summary>
        /// Curve of how to scale object by Splat value
        /// </summary>
        public AnimationCurve ScaleBySplat;
        /// <summary>
        /// Should items scale by Height
        /// </summary>
        public bool useScaleByHeight;
        /// <summary>
        /// Curve of how to scale object by Height
        /// </summary>
        public AnimationCurve ScaleByHeight;
        /// <summary>
        /// Should items scale by Slope
        /// </summary>
        public bool useScaleBySlope;
        /// <summary>
        /// Curve of how to scale object by Slope (0=flat, 1 = vertical)
        /// </summary>
        public AnimationCurve ScaleBySlope;
        #endregion
        #region Rotations
        [HideInInspector]
        public float RotBase;
        [HideInInspector]
        public float RotRange;

        /// <summary>
        /// Amount to rotate all items
        /// </summary>
        public Vector3 RotBases;
        /// <summary>
        /// Amount to randomly rotate all items
        /// </summary>
        public Vector3 RotRanges;
        /// <summary>
        /// Amount to push rotation toward identity
        /// </summary>
        [Range(0, 1)]
        public float AlignToUp;
        /// <summary>
        /// Size of area to check for slope calculations
        /// </summary>
        [Range(0.1f, 5)]
        public float SlopeCheckSize = 1f;
        /// <summary>
        /// Amount to push rotation toward slope direction
        /// </summary>
        [Range(0, 1)]
        public float AlignForwardToSlope;
        /// <summary>
        /// Which map to sample for slope calculations
        /// </summary>
        public SlopeType SlopeType;
        /// <summary>
        /// Type of Noise to be used for slope
        /// </summary>
        public RandomType SlopeNoiseType = RandomType.UNITY_PERLIN;
        /// <summary>
        /// Multiplier to apply to noise uv coordinates for slope noise
        /// </summary>
        public float SlopeNoiseSize = 1f;
        /// <summary>
        /// Frequency multiplier between successive octaves for slope noise
        /// </summary>
        public float SlopeNoiseLacunarity = 2f;
        /// <summary>
        /// Controls how quickly amplitudes diminish for each successive octave for slope noise
        /// </summary>
        public float SlopeNoisePersistence = 0.5f;
        [Range(-10,10)]
        /// <summary>
        /// Multiplier to apply to noise values for slope noise
        /// </summary>
        public float SlopeNoiseMult = 1f;
        /// <summary>
        /// Modifier to apply to noise values for slope noise
        /// </summary>
        [Range(-1, 1)]
        public float SlopeNoiseMod;
        /// <summary>
        /// Number of Octaves to perform for slope noise
        /// </summary>
        [Range(1, 4)]
        public int SlopeNoiseOctaves = 3;
        /// <summary>
        /// Type of Mixed Noise to be used for slope
        /// </summary>
        public RandomType SlopeNoise2Type = RandomType.NONE;
        /// <summary>
        /// Method to use to apply Mixed Noise for slope noise
        /// </summary>
        public CombineMethod SlopeNoise2Method = CombineMethod.MULTIPLY;
        /// <summary>
        /// Multiplier to apply to Mixed Noise uv coordinates for slope noise
        /// </summary>
        public float SlopeNoise2Size = 1f;
        /// <summary>
        /// Frequency multiplier between successive octaves for slope noise
        /// </summary>
        public float SlopeNoise2Lacunarity = 2f;
        /// <summary>
        /// Controls how quickly amplitudes diminish for each successive octave for slope noise
        /// </summary>
        public float SlopeNoise2Persistence = 0.5f;
        /// <summary>
        /// Multiplier to apply to Mixed Noise values for slope noise
        /// </summary>
        public float SlopeNoise2Mult = 1f;
        /// <summary>
        /// Modifier to apply to Mixed Noise values for slope noise
        /// </summary>
        [Range(-1, 1)]
        public float SlopeNoise2Mod;
        /// <summary>
        /// Number of Octavies to perform for slope noise
        /// </summary>
        [Range(1, 4)]
        public int SlopeNoise2Octaves = 3;
        #endregion
        #region Randomization
        /// <summary>
        /// Should Noise be applied?
        /// </summary>
        public int RandomSeed;
        /// <summary>
        /// Should Noise be applied?
        /// </summary>
        public RandomType ScaleNoiseType = RandomType.UNITY_PERLIN;
        /// <summary>
        /// Scale to apply to noise uv coordinates
        /// </summary>
        public float ScaleNoiseSize = 1;
        /// <summary>
        /// Frequency multiplier between successive octaves for Scale noise
        /// </summary>
        public float ScaleNoiseLacunarity = 2f;
        /// <summary>
        /// Controls how quickly amplitudes diminish for each successive octave for Scale noise
        /// </summary>
        public float ScaleNoisePersistence = 0.5f;
        /// <summary>
        /// Multiplier to apply to noise values for scale
        /// </summary>
        public float ScaleNoiseMult = 1f;
        /// <summary>
        /// Modifier to apply to noise values for scale
        /// </summary>
        [Range(-1, 1)]
        public float ScaleNoiseMod = 0f;
        /// <summary>
        /// Number of octaves to iterate through for multi-sampled noise
        /// </summary>
        [Range(1, 4)]
        public int ScaleNoiseOctaves = 3;
        /// <summary>
        /// Should Mixed Noise be applied?
        /// </summary>
        public RandomType ScaleNoise2Type = RandomType.NONE;
        /// <summary>
        /// How to apply Mixed Noise
        /// </summary>
        public CombineMethod ScaleNoise2Method = CombineMethod.MULTIPLY;
        /// <summary>
        /// Scale to apply to Mixed Noise uv coordinates
        /// </summary>
        public float ScaleNoise2Size;
        /// <summary>
        /// Frequency multiplier between successive octaves in Mixed Noise for Scale
        /// </summary>
        public float ScaleNoise2Lacunarity = 2f;
        /// <summary>
        /// Controls how quickly amplitudes diminish for each successive octave in Mixed Noise for Scale
        /// </summary>
        public float ScaleNoise2Persistence = 0.5f;
        /// <summary>
        /// Multiplier to apply to Mixed Noise values for scale
        /// </summary>
        public float ScaleNoise2Mult = 1f;
        /// <summary>
        /// Modifier to apply to Mixed Noise values for scale
        /// </summary>
        [Range(-1, 1)]
        public float ScaleNoise2Mod = 0f;
        /// <summary>
        /// Number of octaves to iterate through for multi-sampled Mixed Noise for scale
        /// </summary>
        [Range(1, 4)]
        public int ScaleNoise2Octaves = 3;
        /*
        /// <summary>
        /// Should Noise be applied?
        /// </summary>
        [UnityEngine.Serialization.FormerlySerializedAs("NoiseType")]
        public RandomType NoiseOffsetType;
        /// <summary>
        /// Scale to apply to noise uv coordinates
        /// </summary>
        [UnityEngine.Serialization.FormerlySerializedAs("NoiseScale")]
        public float NoiseOffsetSize;
        /// <summary>
        /// Multiplier to apply to noise values for offset
        /// </summary>
        [UnityEngine.Serialization.FormerlySerializedAs("NoiseMult")]
        [Range(-1,1)]
        public float NoiseOffsetMult = 0.5f;
        /// <summary>
        /// Modifier to apply to noise values for offset
        /// </summary>
        [Range(-1,1)]
        public float NoiseOffsetMod = 1f;
        //*/
        /// <summary>
        /// Alpha value to stop generating items
        /// </summary>
        public float ColorRandomization;
        #endregion
        #region Color Settings
        /// <summary>
        /// Curve of how to color object by Scale
        /// </summary>
        public AnimationCurve ColorTransition;
        /// <summary>
        /// Property Name on Material to set color value of "Color A" to
        /// (Ignored if Blank)
        /// </summary>
        public string ColorAPropName = "";
        /// <summary>
        /// Property Name on Material to set color value of "Color B" to
        /// (Ignored if Blank)
        /// </summary>
        public string ColorBPropName = "";
        /// <summary>
        /// Color to use at ColorTransition values of 0
        /// </summary>
        public Color ColorA = new Color(0.5f, 0.5f, 0.5f, 0f);
        /// <summary>
        /// Color to use at ColorTransition values of 1
        /// </summary>
        public Color ColorB = new Color(0.5f, 0.5f, 0.5f, 0f);
        #endregion
        #endregion
        #region Render Settings
        /// <summary>
        /// Should rendering be disabled for this object?
        /// </summary>
        public bool DisableRender = false;
        /// <summary>
        /// Render method to use for this object
        /// </summary>
        public StormRenderType RenderType;
        /// <summary>
        /// Render method to use for this object
        /// </summary>
        public StormCombineType CombineType;
        /// <summary>
        /// Distance Percent to switch LODs
        /// </summary>
        public Vector4 LODSplits = new Vector4(0.2f, 0.3f, 0.5f, 0.7f);
        /// <summary>
        /// Distance Percent to fade between each LOD
        /// </summary>
        [Range(0, 1)]
        public float LODFade = 0.05f;
        /// <summary>
        /// Maximum percent of render distance to mix up LOD changes to soften LOD change line
        /// </summary>
        [Range(0,1)]
        public float LODMix = 0.05f;
        /// <summary>
        /// Array of all LODs to use for this object
        /// </summary>
        public StormLODData[] LODs;
        /// <summary>
        /// Shadows for this object should use this LOD value for rendering (-1 = custom)
        /// </summary>
        [Range(-1, 5)]
        public int ShadowsUseLOD = 0;
        /// <summary>
        /// LOD data to use when rendering shadows for this object
        /// </summary>
        public StormLODData ShadowRenderer
        {
            get {
                if (ShadowsUseLOD < 0)
                {
                    if (m_shadowRenderer == null)
                    {
                        m_shadowRenderer = new StormLODData(LODs[0]);
                        StormUtils.SetAssetDirty(this);
                    }
                    return m_shadowRenderer;
                }
                else
                    return LODs[Mathf.Min(ShadowsUseLOD, LODs.Length - 1)];
            }
        }
        [SerializeField]
        private StormLODData m_shadowRenderer;
        /// <summary>
        /// Additional Offset to apply to all rendered instances of this Feature
        /// </summary>
        public Vector3 RenderOffset = Vector3.zero;
        /// <summary>
        /// Additional Rotation to apply to all rendered instances of this Feature
        /// </summary>
        public Vector3 RenderRotation = Vector3.zero;
        /// <summary>
        /// Additional Scale to apply to all rendered instances of this Feature
        /// </summary>
        public Vector3 RenderScale = Vector3.one;
        /// <summary>
        /// Should "Long Shadow" calculations be done to prevent shadow pop-in/out
        /// </summary>
        public bool DrawLongShadows = false;
        /// <summary>
        /// Maximum distance from camera to check for long shadows
        /// </summary>
        public float MaxLongShadowDist = 200f;
        /// <summary>
        /// Multiplier to apply to "Density Fade" randomization
        /// </summary>
        [Min(0)]
        public float DensityFadeStrength;
        /// <summary>
        /// Percent of view range to start fading out Density values
        /// </summary>
        [Range(0, 1)]
        public float DensityFadeStart;
        /// <summary>
        /// Percent of "Density Fade" range to scale objects out
        /// </summary>
        [Range(0, 1)]
        public float DensityFadeScaleoutRange;
        /// <summary>
        /// Percent of view distance to scale-out objects (shrink as they approach max range instead of popping out of existence)
        /// </summary>
        public float ScaleoutRange;
        /// <summary>
        /// Minimum Scale value Scale-out achieves
        /// </summary>
        [Range(0, 1)]
        public float ScaleoutMinScale;

        /// <summary>
        /// Array of Colliders to spawn for this object
        /// </summary>
        public StormColliderData[] Colliders = new StormColliderData[0];
        #endregion

        /// <summary>
        /// Action invoked when LOD Settings change
        /// </summary>
        public Action OnLODSettingsChanged;
        /// <summary>
        /// Action invoked when Generation Settings change
        /// </summary>
        public Action OnGenerationSettingsChanged;
        /// <summary>
        /// Action invoked when Render Settings change (Mesh, Material, and/or LODs)
        /// </summary>
        public Action OnRenderSettingsChanged;

#pragma warning disable CS0618 // Type or member is obsolete
        [ExecuteInEditMode]
        public void OnEnable()
        {
            if (Version < 1)
            {
                if (RotBase > 0)
                {
                    RotBases = new Vector3(0, RotBase * 180, 0);
                    RotBase = 0;
                }
                if (RotRange > 0)
                {
                    RotRanges = new Vector3(0, RotRange * 180, 0);
                    RotRange = 0;
                }
#pragma warning disable CS0612 // Type or member is obsolete
                if (PositionJitter.x == 0 && PositionJitter.y == 0 && PositionJitter.z == 0 && PositionJitter.w == 0 && Jitter != 0)
                {
                    PositionJitter = new Vector4(Jitter, Jitter, 0, 0);
                    Jitter = 0;
                }
#pragma warning restore CS0612 // Type or member is obsolete
                
                //Search through existing Asset Ref files to find where we came from
                StormAssetRefs[] assetRefs = StormUtils.FindAssets<StormAssetRefs>("t:StormAssetRefs", "");
                for (int a = 0; a < assetRefs.Length; ++a)
                {
                    for (int d = 0; d < assetRefs[a].PrefabObjects.Length; ++d)
                    {
                        if (assetRefs[a].PrefabObjects[d].Settings == this)
                        {
                            SourceType = StormFeatureSource.PREFAB;
                            SourcePrefab = assetRefs[a].PrefabObjects[d].Source;
                        }
                    }
                }
                Version = 1;
                StormUtils.SetAssetDirty(this);
            }
            if (Version == 1)
            {
                if (SourcePrefab != null)
                {
                    RenderScale = SourcePrefab.transform.localScale;
                }
                Version = 2;
                StormUtils.SetAssetDirty(this);
            }
            StormSystemSettings.OnFeatureSettingsEnabled?.Invoke(this);
        }
#pragma warning restore CS0618 // Type or member is obsolete
    }
}