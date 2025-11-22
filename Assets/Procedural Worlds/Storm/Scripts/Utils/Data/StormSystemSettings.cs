using UnityEngine;
using System;
using Object = UnityEngine.Object;

/// Copyright 2025 Procedural Worlds Ltd
/// Written by Jonathan Williams as part of Storm

namespace ProceduralWorlds.Storm.Utils
{
    [System.Serializable]
    public struct FeaturesetFeature
    {
        public StormFeatureset FeatureSet;
        public StormFeatureSettings Feature;

        public FeaturesetFeature(StormFeatureset fs, StormFeatureSettings f)
        {
            FeatureSet = fs;
            Feature = f;
        }
    }
    [System.Serializable]
    public struct ReplacementShaderInfo
    {
        public Shader m_shader;
        public ShaderOption<string[]>[] m_optionsToCopy;
        public ShaderOption<bool>[] m_optionsBool;
        public ShaderOption<float>[] m_optionsFloat;
        public ShaderOption<int>[] m_optionsInt;
        public ShaderOption<Vector4>[] m_optionsVector;
    }
    /// <summary>
    /// Scriptable Object to hold core settings used by Storm
    /// </summary>
    [CreateAssetMenu(menuName = "Procedural Worlds/Storm/System Settings")]
    public class StormSystemSettings : ScriptableObject
    {
        [Header("Loading Distances")]
        /// <summary>
        /// Distance to Load objects added to their draw distance
        /// </summary>
        [Min(0)]
        [Tooltip("Distance to Load objects added to their draw distance")]
        public float m_loadDistance = 50f;
        /// <summary>
        /// Distance to Unload objects added to their draw distance
        /// </summary>
        [Min(0)]
        [Tooltip("Distance to Unload objects added to their draw distance")]
        public float m_unloadDistance = 150f;

        [Header("Loading")]
        /// <summary>
        /// Maximum number of tiles to load at a time
        /// </summary>
        [Tooltip("Maximum number of tiles to load at a time")]
        public int m_maxSimultaneousLoad = 20;
        /// <summary>
        /// Number of masks to process per frame for loading job setup
        /// Lower numbers reduce framerate impact in exchange for longer load times
        /// </summary>
        [Tooltip("Number of masks to process per frame for loading job setup\nLower numbers reduce framerate impact in exchange for longer overall load times")]
        public int m_maxLoadMasksPerFrame = 20;
        /// <summary>
        /// Number of instances to process per frame after a loading job finishes
        /// Lower numbers reduce framerate impact in exchange for longer overall load times
        /// </summary>
        [Tooltip("Number of instances to process per frame after a loading job finishes\nLower numbers reduce framerate impact in exchange for longer overall load times")]
        public int m_maxInstancesPerFrame = 2000;
        /// <summary>
        /// Should the first load of a StormWorld be instant? (if false, first load will use loading settings above)
        /// </summary>
        [Tooltip("Should the first load of a StormWorld be instant? (if false, first load will use loading settings above)")]
        public bool m_instantFirstLoad = true;
        /// <summary>
        /// Should Terrain data be cached for faster loading?
        /// </summary>
        [Tooltip("Should Terrain data be cached for faster loading?")]
        public bool m_cacheTerrainData = false;
        /// <summary>
        /// Should we check if duplicate instances exist when loading TRS data?
        /// </summary>
        [Tooltip("Should we check if duplicate instances exist when loading TRS data?")]
        public bool m_preventDuplicateInstances = false;
        /// <summary>
        /// Should colliders be created for Features that have them?
        /// </summary>
        [Tooltip("Should colliders be created for Features that have them?")]
        public bool m_createColliders = true;
        /// <summary>
        /// Should the Collider parent object be hidden in Hierarchy?
        /// </summary>
        [Tooltip("Should the Collider parent object be hidden in Hierarchy?")]
        public bool m_hideColliders = true;

        [Header("Data Resolutions")]
        /// <summary>
        /// Size of Render Cells used for culling and loading. larger = better for fewer objects. dense worlds should use lower values (around 128)
        /// </summary>
        [UnityEngine.Serialization.FormerlySerializedAs("m_worldTileSize")]
        [Min(1f)]
        [Tooltip("Size of Render Cells used for culling and loading. larger = better for fewer objects. dense worlds should use lower values (around 128)")]
        public float m_renderCellSize = 128f;
        /// <summary>
        /// Should we split high density cell data into sub-grids?
        /// This helps primarily with grass but could help with culling any high-density cell data
        /// </summary>
        [Tooltip("Should we split high density cell data into sub-grids?\nThis helps primarily with grass but could help with culling any high-density cell data")]
        public bool m_splitDenseCells = true;
        /// <summary>
        /// Number of instances in a cell before splitting into sub-grids
        /// </summary>
        [Tooltip("Number of instances in a cell before splitting into sub-grids")]
        public int m_splitInstanceCount = 200;
        /// <summary>
        /// Number of sub-grid splits along each axis to create
        /// </summary>
        [Tooltip("Number of sub-grid splits along each axis to create")]
        public int m_splitSubgridCount = 4;
        /// <summary>
        /// Number of steps to use for sampling Curves for Jobs
        /// </summary>
        [Tooltip("Number of steps to use for sampling Curves for Jobs")]
        public int m_curveResolution = 16;
        /// <summary>
        /// Padding to add to maps during loading (extra data for items that are on edges of tiles)
        /// </summary>
        [Tooltip("Padding to add to maps during loading (extra data for items that are on edges of tiles)")]
        public float m_loadingMapPadding = 5;

        /// <summary>
        /// Camera types to render to
        /// </summary>
        [Tooltip("Camera types to render to")]
        public CameraType m_allowedCameraTypes = ~CameraType.Preview;

        /// <summary>
        /// Type of Object Culling to use
        /// </summary>
        [Tooltip("Type of Object Culling to use")]
        public StormCullType m_cullType = StormCullType.GPU;
        /// <summary>
        /// Shader to use for GPU Culling
        /// </summary>
        [Tooltip("Shader to use for GPU Culling")]
        public ComputeShader m_cullShader = null;
        /// <summary>
        /// Shader to use for GPU Culling in Split mode
        /// </summary>
        [Tooltip("Shader to use for GPU Culling in Split mode")]
        public ComputeShader m_cullSplitShader = null;
        /// <summary>
        /// Kernel index in the Cull Shader to use for standard culling
        /// </summary>
        public int m_cullKernelStandard = 0;
        /// <summary>
        /// Kernel index in the Cull Shader to use for simplified culling
        /// (for Fully Visible Cells)
        /// </summary>
        public int m_cullKernelSimple = 1;
        /// <summary>
        /// Kernel index in the Cull Shader to use for Shadow Only culling
        /// (for Shadow Only Cells)
        /// </summary>
        public int m_cullKernelShadowOnly = 2;
        /// <summary>
        /// Shader to use for GPU LOD splitting in Split mode
        /// </summary>
        [Tooltip("Shader to use for GPU LOD splitting in Split mode")]
        public ComputeShader m_lodSplitShader = null;
        /// <summary>
        /// Kernel index to use in the LOD Shader
        /// </summary>
        public int m_lodSplitKernel = 0;

        /// <summary>
        /// Shader used for counting instances and preparing indirect buffer for Finalize shader's execution
        /// </summary>
        [Tooltip("Shader used for counting instances and preparing indirect buffer for Finalize shader's execution")]
        public ComputeShader m_cullCountShader = null;
        /// <summary>
        /// Shader used for clearing instance counts
        /// </summary>
        [Tooltip("Shader used for clearing instance counts")]
        public ComputeShader m_cullCountClearShader = null;
        /// <summary>
        /// Kernel index in the Cull Count Shader to use
        /// </summary>
        public int m_cullCountKernel = 0;
        /// <summary>
        /// Kernel index in the Cull Count Clear Shader to use
        /// </summary>
        public int m_cullCountClearKernel = 0;

        /// <summary>
        /// Shader used for render preperation of instances
        /// </summary>
        [Tooltip("Shader used for render preperation of instances")]
        public ComputeShader m_finalizeShader = null;
        /// <summary>
        /// Kernel index in the Finalize Shader to use
        /// </summary>
        public int m_finalizeKernel = 0;
        /// <summary>
        /// Should jobs be used? If disabled, all functions will run on main thread!
        /// </summary>
        [Header("Data Handling Methods")]
        [Tooltip("Should jobs be used? If disabled, all functions will run on main thread!")]
        public bool m_useJobs = true;
        /// <summary>
        /// Should jobs be completed on next frame or forced to finish on current?
        /// </summary>
        [Tooltip("Should jobs be completed on next frame or forced to finish on current?")]
        public bool m_allowFrameDelay = true;
        /// <summary>
        /// Multiplier applied to turn speed of camera for culling to prevent pop-in from turning
        /// </summary>
        public float m_cullTurnOffset = 1;
        /// <summary>
        /// Padding added to frustrum of camera for culling
        /// </summary>
        public float m_cullViewPadding = 5;
        /// <summary>
        /// Should shadow rendering be split into separate calls? (testing)
        /// </summary>
        public bool m_splitShadowRendering = false;
        /// <summary>
        /// Multiplier applied to Unity shadow distance when using split shadow rendering and in standard shadow culling
        /// </summary>
        public float m_shadowDistanceMultiplier = 1f;
        /// <summary>
        /// distance to calculate shadow culling or max shadow range when using split shadow rendering
        /// </summary>
        public float m_shadowDistanceSRP = 500f;
        /// <summary>
        /// Method to use for Rendering for Features using "DEFAULT" value
        /// </summary>
        public StormRenderType m_renderType = StormRenderType.RENDER_INDIRECT;
        /// <summary>
        /// Method to use for Instance Combine for Features using "DEFAULT" value
        /// </summary>
        public StormCombineType m_combineType = StormCombineType.STANDARD;
        /// <summary>
        /// Minimum distance in world units that the camera must move before forcing a culling update
        /// </summary>
        public float m_minUpdateMovement = 0.5f;
        /// <summary>
        /// Minimum distance in world units that the camera or sun light must turn before forcing a culling update
        /// </summary>
        public float m_minUpdateTurn = 0.25f;
        /// <summary>
        /// Multiplier applied to shadow distance when determining long shadow culling
        /// This is applied to the smaller of either the SystemSettings or FeatureSettings Max Shadow Distance
        /// </summary>
        public float m_shadowDistanceMult = 1f;

        /// <summary>
        /// Should polygon data always be rasterized when loaded (improves loading time if there is a lot of polygon data)
        /// </summary>
        public bool AlwaysRasterizePolygons = false;
        /// <summary>
        /// Resolution to rasterize polygons at if no other float data is found per 100m
        /// </summary>
        public int DefaultPolygonResolution = 512;
        /// <summary>
        /// Maximum Resolution to rasterize polygons at if no other float data is found (total)
        /// </summary>
        public int MaxTotalPolygonResolution = 2048;

        /// <summary>
        /// Reference to ColorTargetData to use when doing color based ingestion
        /// </summary>
        public StormColorTargetData DefaultColorTargetData = null;
        /// <summary>
        /// Default color range to use when doing color based ingestion
        /// </summary>
        [Range(0, 2)]
        public float ColorTargetRange = 0.25f;
        /// <summary>
        /// Default color Falloff to use when doing color based ingestion
        /// </summary>
        [Range(0, 2)]
        public float ColorTargetFalloff = 0.1f;
        /// <summary>
        /// List of Tags to consider MeshTerrain during ingestion
        /// </summary>
        public string[] MeshTerrainTags = new string[] { "Terrain" };
        /// <summary>
        /// Resolution of MeshTerrain Heightmap to generate per 100m in world space the object ingested takes up
        /// </summary>
        public float MeshTerrainResolution = 128f;

        /// <summary>
        /// Method to use when extra materials are applied to a mesh.
        /// Last = Extra Materials are applied to only the last submesh (Unity Style)
        /// Loop = Extra Materials are applied to all submeshes, looping continuously
        /// None = Extra Materials are ignored.
        /// </summary>
        public StormExtraMaterialMode ExtraMaterialMode = StormExtraMaterialMode.LAST;

        [Header("Prefab Creation Settings")]
        /// <summary>
        /// Progression of LODs to use when creating new Prefabs. X axis is LOD level, Y axis is distance
        /// </summary>
        [Tooltip("Progression of LODs to use when creating new Prefabs. X axis is LOD level, Y axis is distance")]
        public AnimationCurve DefaultLODProgression = AnimationCurve.EaseInOut(0, 0, 1f, 0.5f);
        /// <summary>
        /// LODs to generate for Detail objects when creating new Prefabs from Terrain
        /// </summary>
        [Tooltip("LODs to generate for Detail objects when creating new Prefabs from Terrain")]
        public StormLODData[] DefaultDetailLODs;
        /// <summary>
        /// Should an Impostor be created as a final LOD for ingested Detail objects from Terrain?
        /// </summary>
        [Tooltip("Should an Impostor be created as a final LOD for ingested Detail objects from Terrain?")]
        public bool DefaultDetailCreateImpostor;
        /// <summary>
        /// LOD Fade value to set on new Prefabs
        /// </summary>
        [Tooltip("LOD Fade value to set on new Prefabs")]
        public float DefaultLODFade = 0;
        /// <summary>
        /// LOD Mix value to set on new Prefabs
        /// </summary>
        [Tooltip("LOD Mix value to set on new Prefabs")]
        public float DefaultLODMix = 0.05f;
        /// <summary>
        /// Default density to set on new Prefabs
        /// </summary>
        [Tooltip("Default density to set on new Prefabs")]
        public float DefaultDensity = 15;
        /// <summary>
        /// Render type to set on new Prefabs
        /// </summary>
        [Tooltip("Render type to set on new Prefabs")]
        public StormRenderType DefaultRenderType = StormRenderType.RENDER_INDIRECT;
        /// <summary>
        /// List of Prefabs to not include in Ingestion if found in scenes
        /// </summary>
        [Tooltip("List of Prefabs to not include in Ingestion if found in scenes")]
        public GameObject[] IgnoredPrefabs = new GameObject[0];
        /// <summary>
        /// List of Unity Layers to ignore when ingesting objects.
        /// </summary>
        [Tooltip("List of Unity Layers to ignore when ingesting objects.")]
        public string[] IgnoredLayers = new string[] { "UI", "Water" };
        /// <summary>
        /// List of Tags to ignore when ingesting objects.
        /// </summary>
        [Tooltip("List of Tags to ignore when ingesting objects.")]
        public string[] IgnoredTags = new string[] { };
        /// <summary>
        /// Should we to ingest static objects?
        /// </summary>
        [Tooltip("Should we to ingest static objects?")]
        public bool IngestStatic = true;
        /// <summary>
        /// Should we to ingest non-static objects?
        /// </summary>
        [Tooltip("Should we to ingest non-static objects?")]
        public bool IngestNonStatic = true;

        /// <summary>
        /// List of prefab names to count as "Grass" objects during ingestion
        /// If any of these are found in the name of the prefab, they will be counted as Grass
        /// </summary>
        public string[] ObjectGrassNames = new string[] { "grass" };

        /// <summary>
        /// List of prefab names to count as "Detail" objects during ingestion
        /// If any of these are found in the name of the prefab, they will be counted as Detail
        /// </summary>
        public string[] ObjectDetailNames = new string[] { "detail" };

        /// <summary>
        /// List of prefab names to count as "Plant" objects during ingestion
        /// If any of these are found in the name of the prefab, they will be counted as Plant
        /// </summary>
        public string[] ObjectPlantNames = new string[] { "plant", "fern" };

        /// <summary>
        /// List of prefab names to count as "Tree" objects during ingestion
        /// If any of these are found in the name of the prefab, they will be counted as Tree
        /// </summary>
        public string[] ObjectTreeNames = new string[] { "tree" };

        /// <summary>
        /// Max Size of object prefab to ingest as "Small"
        /// </summary>
        [Tooltip("Max Size of object prefab to ingest as \"Small\"")]
        public float ObjectSmallSize = 5;
        /// <summary>
        /// Max Size of object prefab to ingest as "Medium"
        /// Anything above this size is counted as "Large"
        /// </summary>
        [Tooltip("Max Size of object prefab to ingest as \"Medium\"")]
        public float ObjectMediumSize = 20;

        [Header("Impostors")]
        /// <summary>
        /// Should impostors be created during ingestion?
        /// </summary>
        [Tooltip("Should impostors be created during ingestion?")]
        public bool CreateImpostors = true;
        /// <summary>
        /// List of strings to search Material and Shader names for to detect existing billboards
        /// </summary>
        [Tooltip("List of strings to search Material and Shader names for to detect existing billboards")]
        public string[] BillboardStrings
        {
            get {
                if (m_billboardStrings == null || m_billboardStrings.Length == 0)
                {
                    m_billboardStrings = new string[] { "billboard", "impostor", "billboad", "_PW_SF_BILLBOARD_ON" };
                    StormUtils.SetAssetDirty(this);
                }
                return m_billboardStrings;
            }
            set {
                m_billboardStrings = value;
            }
        }
        [SerializeField]
        [UnityEngine.Serialization.FormerlySerializedAs("BillboardStrings")]
        internal string[] m_billboardStrings = new string[] { "billboard", "impostor", "billboad", "_PW_SF_BILLBOARD_ON" };
        /// <summary>
        /// Override Bake Shaders to use during ingestion
        /// If left empty will use standard shaders
        /// </summary>
        [Tooltip("Override Bake Shaders to use during ingestion\nIf left empty will use standard shaders")]
        public BakeShaderInfo[] ImpostorBakeShaders
        {
            get {
                BakeShaderInfo[] ret;

                StormRP pipeline = StormUtils.GetRenderPipline();
                switch (pipeline)
                {
                    case StormRP.HDRP:
                        ret = m_impostorBakeShadersHDRP;
                        break;
                    case StormRP.URP:
                        ret = m_impostorBakeShadersURP;
                        break;
                    default:
                        ret = m_impostorBakeShadersBuiltIn;
                        break;
                }
                if (ret == null || ret.Length == 0)
                {
                    ret = GetDefaultBakeShaders(pipeline);
                    switch (pipeline)
                    {
                        case StormRP.HDRP:
                            m_impostorBakeShadersHDRP = ret;
                            break;
                        case StormRP.URP:
                            m_impostorBakeShadersURP = ret;
                            break;
                        default:
                            m_impostorBakeShadersBuiltIn = ret;
                            break;
                    }
                    StormUtils.SetAssetDirty(this);
                }
                return ret;
            }
            set {
                StormRP pipeline = StormUtils.GetRenderPipline();
                switch (pipeline)
                {
                    case StormRP.HDRP:
                        m_impostorBakeShadersHDRP = value;
                        break;
                    case StormRP.URP:
                        m_impostorBakeShadersURP = value;
                        break;
                    default:
                        m_impostorBakeShadersBuiltIn = value;
                        break;
                }
                StormUtils.SetAssetDirty(this);
            }
        }
        /// <summary>
        /// Bake Shaders to use for Built-In pipeline
        /// </summary>
        [SerializeField]
        internal BakeShaderInfo[] m_impostorBakeShadersBuiltIn = new BakeShaderInfo[0];
        /// <summary>
        /// Bake Shaders to use for HDRP pipeline
        /// </summary>
        [SerializeField]
        internal BakeShaderInfo[] m_impostorBakeShadersHDRP = new BakeShaderInfo[0];
        /// <summary>
        /// Bake Shaders to use for URP pipeline
        /// </summary>
        [SerializeField]
        internal BakeShaderInfo[] m_impostorBakeShadersURP = new BakeShaderInfo[0];
        /// <summary>
        /// Override Post Proccessing to use during impostor Generation
        /// If left empty will use standard shaders
        /// </summary>
        [Tooltip("Override Post Proccessing to use during impostor Generation\nIf left empty will use standard shaders")]
        public PostProcessingInfo[] ImpostorPostProcess
        {
            get {
                PostProcessingInfo[] ret;

                StormRP pipeline = StormUtils.GetRenderPipline();
                switch (pipeline)
                {
                    case StormRP.HDRP:
                        ret = m_impostorPostProcessHDRP;
                        break;
                    case StormRP.URP:
                        ret = m_impostorPostProcessURP;
                        break;
                    default:
                        ret = m_impostorPostProcessBuiltIn;
                        break;
                }
                if (ret == null || ret.Length == 0)
                {
                    ret = GetDefaultPostProcessings(pipeline);
                    switch (pipeline)
                    {
                        case StormRP.HDRP:
                            m_impostorPostProcessHDRP = ret;
                            break;
                        case StormRP.URP:
                            m_impostorPostProcessURP = ret;
                            break;
                        default:
                            m_impostorPostProcessBuiltIn = ret;
                            break;
                    }
                    StormUtils.SetAssetDirty(this);
                }
                return ret;
            }
            set {
                StormRP pipeline = StormUtils.GetRenderPipline();
                switch (pipeline)
                {
                    case StormRP.HDRP:
                        m_impostorPostProcessHDRP = value;
                        break;
                    case StormRP.URP:
                        m_impostorPostProcessURP = value;
                        break;
                    default:
                        m_impostorPostProcessBuiltIn = value;
                        break;
                }
                StormUtils.SetAssetDirty(this);
            }
        }
        /// <summary>
        /// Post Proccessing to use for Built-In pipeline
        /// </summary>
        [SerializeField]
        internal PostProcessingInfo[] m_impostorPostProcessBuiltIn = new PostProcessingInfo[0];
        /// <summary>
        /// Post Proccessing to use for HDRP pipeline
        /// </summary>
        [SerializeField]
        internal PostProcessingInfo[] m_impostorPostProcessHDRP = new PostProcessingInfo[0];
        /// <summary>
        /// Post Proccessing to use for URP pipeline
        /// </summary>
        [SerializeField]
        internal PostProcessingInfo[] m_impostorPostProcessURP = new PostProcessingInfo[0];
        /// <summary>
        /// Replacement shader info for Flat impostors
        /// </summary>
        [Tooltip("Replacement shader info for Flat impostors")]
        public ImpostorShaderInfo ImpostorFlatReplacement
        {
            get {
                ImpostorShaderInfo ret;

                StormRP pipeline = StormUtils.GetRenderPipline();
                switch (pipeline)
                {
                    case StormRP.HDRP:
                        ret = m_impostorFlatReplacementHDRP;
                        break;
                    case StormRP.URP:
                        ret = m_impostorFlatReplacementURP;
                        break;
                    default:
                        ret = m_impostorFlatReplacementBuiltIn;
                        break;
                }
                if (ret.m_shader == null)
                {
                    ret = GetDefaultFlatReplacement(pipeline);
                    switch (pipeline)
                    {
                        case StormRP.HDRP:
                            m_impostorFlatReplacementHDRP = ret;
                            break;
                        case StormRP.URP:
                            m_impostorFlatReplacementURP = ret;
                            break;
                        default:
                            m_impostorFlatReplacementBuiltIn = ret;
                            break;
                    }
                    StormUtils.SetAssetDirty(this);
                }
                return ret;
            }
            set {
                StormRP pipeline = StormUtils.GetRenderPipline();
                switch (pipeline)
                {
                    case StormRP.HDRP:
                        m_impostorFlatReplacementHDRP = value;
                        break;
                    case StormRP.URP:
                        m_impostorFlatReplacementURP = value;
                        break;
                    default:
                        m_impostorFlatReplacementBuiltIn = value;
                        break;
                }
                StormUtils.SetAssetDirty(this);
            }
        }
        /// <summary>
        /// Post Proccessing to use for Built-In pipeline
        /// </summary>
        [SerializeField]
        internal ImpostorShaderInfo m_impostorFlatReplacementBuiltIn;
        /// <summary>
        /// Post Proccessing to use for HDRP pipeline
        /// </summary>
        [SerializeField]
        internal ImpostorShaderInfo m_impostorFlatReplacementHDRP;
        /// <summary>
        /// Post Proccessing to use for URP pipeline
        /// </summary>
        [SerializeField]
        internal ImpostorShaderInfo m_impostorFlatReplacementURP;
        /// <summary>
        /// Impostor type to generate
        /// </summary>
        [Tooltip("Impostor type to generate")]
        public ImpostorType ImpostorType = ImpostorType.NINE_SLICE;
        /// <summary>
        /// Should Impostor type overrides be allowed?
        /// (flat items create flat impostors)
        /// </summary>
        [Tooltip("Should Impostor type overrides be allowed? (flat items create flat impostors)")]
        public bool AllowImpostorTypeOverride = true;
        /// <summary>
        /// Maximum Percent of size other sides can be before choosing flat impostor
        /// </summary>
        [Tooltip("Maximum Percent of size other sides can be before choosing flat impostor")]
        public float AllowTypeOverrideSize = 0.1f;
        /// <summary>
        /// Resolution to generate impostors at
        /// </summary>
        [Tooltip("Resolution to generate impostors at")]
        public int ImpostorResolution = 512;
        /// <summary>
        /// Number of steps to use for Octohedral Impostors
        /// </summary>
        [Tooltip("Number of steps to use for Octohedral Impostors")]
        public int ImpostorOctoSteps = 8;
        /// <summary>
        /// Directory to save Impostor data to
        /// </summary>
        [Tooltip("Directory to save Impostor data to")]
        public string ImpostorExportDirectory = StormUtils.DefaultSavePath + "Impostors";

        /// <summary>
        /// Array of known Loader Types and their load/unload ranges
        /// </summary>
        [Tooltip("Array of known Loader Types and their load/unload ranges")]
        public StormLoaderTypeData[] LoaderTypeRanges = new StormLoaderTypeData[] {
            new StormLoaderTypeData() {
                TypeName = "StormLoaderData",
                LoadRange = 1000,
                UnloadRange = 1200,
            },
            new StormLoaderTypeData() {
                TypeName = "StormTile",
                LoadRange = 1000,
                UnloadRange = 1200,
            }
        };

        [Header("Debug LOD Settings")]
        [Tooltip("Material to use for 'Debug LOD' display")]
        /// <summary>
        /// Material to use for 'Debug LOD' display
        /// </summary>
        public Material DebugLODMaterial_BuiltIn = null;
        [Tooltip("Property name to set Color of LOD level")]
        /// <summary>
        /// Property name to set Color of LOD level
        /// </summary>
        public string DebugLODColorProperty_BuiltIn = "_Color";
        [Tooltip("Material to use for 'Debug LOD' display")]
        /// <summary>
        /// Material to use for 'Debug LOD' display
        /// </summary>
        public Material DebugLODMaterial_URP = null;
        [Tooltip("Property name to set Color of LOD level")]
        /// <summary>
        /// Property name to set Color of LOD level
        /// </summary>
        public string DebugLODColorProperty_URP = "_BaseColor";
        [Tooltip("Material to use for 'Debug LOD' display")]
        /// <summary>
        /// Material to use for 'Debug LOD' display
        /// </summary>
        public Material DebugLODMaterial_HDRP = null;
        [Tooltip("Property name to set Color of LOD level")]
        /// <summary>
        /// Property name to set Color of LOD level
        /// </summary>
        public string DebugLODColorProperty_HDRP = "_BaseColor";

        /// <summary>
        /// Material to use for 'Debug LOD' display
        /// </summary>
        public Material DebugLODMaterial
        {
            get {
                Material ret;

                StormRP pipeline = StormUtils.GetRenderPipline();
                switch (pipeline)
                {
                    case StormRP.HDRP:
                        ret = DebugLODMaterial_HDRP;
                        break;
                    case StormRP.URP:
                        ret = DebugLODMaterial_URP;
                        break;
                    default:
                        ret = DebugLODMaterial_BuiltIn;
                        break;
                }
                if (ret == null)
                {
                    ret = GetDefaultDebugLODMaterial(pipeline);
                    string colorProp = GetDefaultDebugLODColorProp(pipeline);
                    switch (pipeline)
                    {
                        case StormRP.Builtin:
                        default:
                            DebugLODMaterial_BuiltIn = ret;
                            DebugLODColorProperty_BuiltIn = colorProp;
                            break;
                        case StormRP.HDRP:
                            DebugLODMaterial_HDRP = ret;
                            DebugLODColorProperty_HDRP = colorProp;
                            break;
                        case StormRP.URP:
                            DebugLODMaterial_URP = ret;
                            DebugLODColorProperty_URP = colorProp;
                            break;
                    }
                    StormUtils.SetAssetDirty(this);
                }
                return ret;
            }
            set {
                StormRP pipeline = StormUtils.GetRenderPipline();
                switch (pipeline)
                {
                    case StormRP.HDRP:
                        DebugLODMaterial_HDRP = value;
                        break;
                    case StormRP.URP:
                        DebugLODMaterial_URP = value;
                        break;
                    default:
                        DebugLODMaterial_BuiltIn = value;
                        break;
                }
                StormUtils.SetAssetDirty(this);
            }
        }
        /// <summary>
        /// Property name to set Color of LOD level
        /// </summary>
        public string DebugLODColorProperty
        {
            get {
                string ret;

                StormRP pipeline = StormUtils.GetRenderPipline();
                switch (pipeline)
                {
                    case StormRP.HDRP:
                        ret = DebugLODColorProperty_HDRP;
                        break;
                    case StormRP.URP:
                        ret = DebugLODColorProperty_URP;
                        break;
                    default:
                        ret = DebugLODColorProperty_BuiltIn;
                        break;
                }
                if (ret == null)
                {
                    ret = GetDefaultDebugLODColorProp(pipeline);
                    switch (pipeline)
                    {
                        case StormRP.Builtin:
                        default:
                            DebugLODColorProperty_BuiltIn = ret;
                            break;
                        case StormRP.HDRP:
                            DebugLODColorProperty_HDRP = ret;
                            break;
                        case StormRP.URP:
                            DebugLODColorProperty_URP = ret;
                            break;
                    }
                    StormUtils.SetAssetDirty(this);
                }
                return ret;
            }
            set {
                StormRP pipeline = StormUtils.GetRenderPipline();
                switch (pipeline)
                {
                    case StormRP.HDRP:
                        DebugLODColorProperty_HDRP = value;
                        break;
                    case StormRP.URP:
                        DebugLODColorProperty_URP = value;
                        break;
                    default:
                        DebugLODColorProperty_BuiltIn = value;
                        break;
                }
                StormUtils.SetAssetDirty(this);
            }
        }


        [Tooltip("Color to use for LOD0 and to begining of the fade")]
        /// <summary>
        /// Color to use for LOD0 and to begining of the fade
        /// </summary>
        public Color DebugLODColorStart = Color.blue;
        [Tooltip("Color to use for the final LOD and end of the fade")]
        /// <summary>
        /// Color to use for the final LOD and end of the fade
        /// </summary>
        public Color DebugLODColorEnd = Color.red;

        [SerializeField]
        internal Object m_defaultDiffusionProfile_HDRP = null;
        private bool _hasLoggedDiffProf = false;
        internal Object _defaultDiffusionProfile = null;
        public Object DefaultDiffusionProfile
        {
            get {
                if (_defaultDiffusionProfile == null && m_defaultDiffusionProfile_HDRP != null)
                {
                    _defaultDiffusionProfile = m_defaultDiffusionProfile_HDRP;
                    StormUtils.AddDiffusionProfile(ref _defaultDiffusionProfile);
                    if (_defaultDiffusionProfile != m_defaultDiffusionProfile_HDRP && !_hasLoggedDiffProf)
                    {
                        _hasLoggedDiffProf = true;
                        Debug.LogWarning("Diffusion Profile '" + m_defaultDiffusionProfile_HDRP.name + "' was not able to be added to Diffusion Profile List. The List is already at max capacity. Selected First profile in list.", m_defaultDiffusionProfile_HDRP);
                    }
                    StormUtils.SetAssetDirty(this);
                }
                return _defaultDiffusionProfile;
            }
            set {
                if (m_defaultDiffusionProfile_HDRP != value)
                {
                    m_defaultDiffusionProfile_HDRP = value;
                    _defaultDiffusionProfile = m_defaultDiffusionProfile_HDRP;
                    StormUtils.AddDiffusionProfile(ref _defaultDiffusionProfile);
                }
            }
        }

        /// <summary>
        /// Should shaders be replaced for all objects rendered by storm?
        /// (This is used in certain bake processes but can be used for other effects potentially)
        /// </summary>
        [Header("Shader Replacement")]
        [Tooltip("Should shaders be replaced for all objects rendered by storm? (This is used in certain bake processes but can be used for other effects potentially)")]
        public bool ReplaceRenderShaders = false;
        /// <summary>
        /// Information for how to replace shaders if enabled
        /// </summary>
        [Tooltip("Information for how to replace shaders if enabled")]
        public ReplacementShaderInfo ReplacementShader;

        /// <summary>
        /// Get Loader Load and Unload ranges for a type. If not found, it will be added to the array using default values.
        /// </summary>
        /// <param name="typeName">Name of Loader data Type</param>
        /// <param name="load">Load range for Type</param>
        /// <param name="unload">Unload range for Type</param>
        public bool GetLoaderRanges(string typeName, ref float load, ref float unload)
        {
            int len = LoaderTypeRanges.Length;
            for (int l = 0; l < len; ++l)
            {
                if (LoaderTypeRanges[l].TypeName == typeName)
                {
                    load = LoaderTypeRanges[l].LoadRange;
                    unload = LoaderTypeRanges[l].UnloadRange;
                    return true;
                }
            }
            System.Array.Resize(ref LoaderTypeRanges, len + 1);
            LoaderTypeRanges[len] = new StormLoaderTypeData() {
                TypeName = typeName,
                LoadRange = load,
                UnloadRange = unload,
            };
            StormUtils.SetAssetDirty(this);
            return false;
        }
        /// <summary>
        /// Set Loader Load range for a type.
        /// </summary>
        /// <param name="typeName">Name of Loader data Type</param>
        /// <param name="range">Range to set for Load value</param>
        public void SetLoaderLoadRange(string typeName, float range)
        {
            int len = LoaderTypeRanges.Length;
            for (int l = 0; l < len; ++l)
            {
                if (LoaderTypeRanges[l].TypeName == typeName)
                {
                    StormLoaderTypeData loader = LoaderTypeRanges[l];
                    loader.LoadRange = range;
                    LoaderTypeRanges[l] = loader;
                    StormUtils.SetAssetDirty(this);
                    return;
                }
            }
            System.Array.Resize(ref LoaderTypeRanges, len + 1);
            LoaderTypeRanges[len] = new StormLoaderTypeData() {
                TypeName = typeName,
                LoadRange = range,
                UnloadRange = range * 1.1f,
            };
            StormUtils.SetAssetDirty(this);
        }
        /// <summary>
        /// Set Loader Unload range for a type.
        /// </summary>
        /// <param name="typeName">Name of Loader data Type</param>
        /// <param name="range">Range to set for unload value</param>
        public void SetLoaderUnloadRange(string typeName, float range)
        {
            int len = LoaderTypeRanges.Length;
            for (int l = 0; l < len; ++l)
            {
                if (LoaderTypeRanges[l].TypeName == typeName)
                {
                    StormLoaderTypeData loader = LoaderTypeRanges[l];
                    loader.UnloadRange = range;
                    LoaderTypeRanges[l] = loader;
                    StormUtils.SetAssetDirty(this);
                    return;
                }
            }
            System.Array.Resize(ref LoaderTypeRanges, len + 1);
            LoaderTypeRanges[len] = new StormLoaderTypeData() {
                TypeName = typeName,
                LoadRange = range * 0.9f,
                UnloadRange = range,
            };
            StormUtils.SetAssetDirty(this);
        }

        /// <summary>
        /// Get default BakeShader setup for a pipeline used in Impostor Baking
        /// </summary>
        /// <param name="pipeline">Pipeline to get default setup for</param>
        /// <returns>Array of BakeShader steps to use for Impostor Baking</returns>
        public static BakeShaderInfo[] GetDefaultBakeShaders(StormRP pipeline)
        {
            switch (pipeline)
            {
                case StormRP.HDRP:
                    return new BakeShaderInfo[] {
                                new BakeShaderInfo {
                                    m_shader = Shader.Find("PWStorm/Shader Graphs/SH_ImposterAlbedoBake_HDRP"),
                                    m_fieldName = "_MainTex",
                                    m_texName = "_MainTex_0",
                                    m_type = BakeShaderOutputType.DEFAULT,
                                    m_alphaUpdate = AlphaUpdateType.NONE,
                                    m_optionsToCopy = new ShaderOption<string[]>[] {
                                        new ShaderOption<string[]> {
                                            Key = "_BaseColorMap",
                                            Value = new string[] { "_MainTex" }
                                        }
                                    },
                                    m_isTemporary = true,
                                },
                                new BakeShaderInfo {
                                    m_shader = Shader.Find("PWStorm/Shader Graphs/SH_ImposterAlbedoBake_HDRP"),
                                    m_fieldName = "_MainTex",
                                    m_texName = "_MainTex_1",
                                    m_type = BakeShaderOutputType.DEFAULT,
                                    m_alphaUpdate = AlphaUpdateType.NONE,
                                    m_optionsToCopy = new ShaderOption<string[]>[] {
                                        new ShaderOption<string[]> {
                                            Key = "_BaseColorMap",
                                            Value = new string[] { "_MainTex" }
                                        }
                                    },
                                    m_optionsBool = new ShaderOption<bool>[] {
                                        new ShaderOption<bool> {
                                            Key = "_OutputAlpha",
                                            Value = true
                                        }
                                    },
                                    m_isTemporary = true,
                                },
                                new BakeShaderInfo {
                                    m_shader = Shader.Find("PWStorm/Shader Graphs/SH_ImposterMaskBake_HDRP"),
                                    m_fieldName = "_MetallicGlossMap",
                                    m_texName = "_MetallicGlossMap_0",
                                    m_type = BakeShaderOutputType.MASK,
                                    m_alphaUpdate = AlphaUpdateType.DEFAULT,
                                    m_optionsToCopy = new ShaderOption<string[]>[] {
                                        new ShaderOption<string[]> {
                                            Key = "_BaseColorMap",
                                            Value = new string[] { "_MainTex" }
                                        }
                                    },
                                    m_optionsBool = new ShaderOption<bool>[] {
                                        new ShaderOption<bool> {
                                            Key = "_OutputSmoothness",
                                            Value = false
                                        }
                                    },
                                    m_isTemporary = true,
                                },
                                new BakeShaderInfo {
                                    m_shader = Shader.Find("PWStorm/Shader Graphs/SH_ImposterMaskBake_HDRP"),
                                    m_fieldName = "_MetallicGlossMap",
                                    m_texName = "_MetallicGlossMap_1",
                                    m_type = BakeShaderOutputType.MASK,
                                    m_alphaUpdate = AlphaUpdateType.DEFAULT,
                                    m_optionsToCopy = new ShaderOption<string[]>[] {
                                        new ShaderOption<string[]> {
                                            Key = "_BaseColorMap",
                                            Value = new string[] { "_MainTex" }
                                        }
                                    },
                                    m_optionsBool = new ShaderOption<bool>[] {
                                        new ShaderOption<bool> {
                                            Key = "_OutputSmoothness",
                                            Value = true
                                        }
                                    },
                                    m_isTemporary = true,
                                },
                                new BakeShaderInfo {
                                    m_shader = Shader.Find("PWStorm/Shader Graphs/SH_ImposterNormalBake_HDRP"),
                                    m_fieldName = "_BumpMap",
                                    m_texName = "_BumpMap",
                                    m_type = BakeShaderOutputType.NORMAL,
                                    m_alphaUpdate = AlphaUpdateType.DEFAULT,
                                    m_optionsToCopy = new ShaderOption<string[]>[] {
                                        new ShaderOption<string[]> {
                                            Key = "_NormalMap",
                                            Value = new string[] { "_BumpMap" }
                                        }
                                    }
                                },
                            };
                case StormRP.URP:
                    return new BakeShaderInfo[] {
                                new BakeShaderInfo {
                                    m_shader = Shader.Find("PWStorm/Shader Graphs/SH_ImposterAlbedoBake_URP"),
                                    m_fieldName = "_MainTex",
                                    m_texName = "_MainTex_0",
                                    m_type = BakeShaderOutputType.DEFAULT,
                                    m_alphaUpdate = AlphaUpdateType.NONE,
                                    m_optionsToCopy = new ShaderOption<string[]>[] {
                                        new ShaderOption<string[]> {
                                            Key = "_BaseMap",
                                            Value = new string[] { "_MainTex" }
                                        }
                                    },
                                    m_isTemporary = true,
                                },
                                new BakeShaderInfo {
                                    m_shader = Shader.Find("PWStorm/Shader Graphs/SH_ImposterAlbedoBake_URP"),
                                    m_fieldName = "_MainTex",
                                    m_texName = "_MainTex_1",
                                    m_type = BakeShaderOutputType.DEFAULT,
                                    m_alphaUpdate = AlphaUpdateType.NONE,
                                    m_optionsToCopy = new ShaderOption<string[]>[] {
                                        new ShaderOption<string[]> {
                                            Key = "_BaseMap",
                                            Value = new string[] { "_MainTex" }
                                        }
                                    },
                                    m_optionsBool = new ShaderOption<bool>[] {
                                        new ShaderOption<bool> {
                                            Key = "_OutputAlpha",
                                            Value = true
                                        }
                                    },
                                    m_isTemporary = true,
                                },
                                new BakeShaderInfo {
                                    m_shader = Shader.Find("PWStorm/Shader Graphs/SH_ImposterMaskBake_URP"),
                                    m_fieldName = "_MetallicGlossMap",
                                    m_texName = "_MetallicGlossMap_0",
                                    m_type = BakeShaderOutputType.MASK,
                                    m_alphaUpdate = AlphaUpdateType.DEFAULT,
                                    m_optionsToCopy = new ShaderOption<string[]>[] {
                                        new ShaderOption<string[]> {
                                            Key = "_BaseMap",
                                            Value = new string[] { "_MainTex" }
                                        }
                                    },
                                    m_optionsBool = new ShaderOption<bool>[] {
                                        new ShaderOption<bool> {
                                            Key = "_OutputSmoothness",
                                            Value = false
                                        }
                                    },
                                    m_isTemporary = true,
                                },
                                new BakeShaderInfo {
                                    m_shader = Shader.Find("PWStorm/Shader Graphs/SH_ImposterMaskBake_URP"),
                                    m_fieldName = "_MetallicGlossMap",
                                    m_texName = "_MetallicGlossMap_1",
                                    m_type = BakeShaderOutputType.MASK,
                                    m_alphaUpdate = AlphaUpdateType.DEFAULT,
                                    m_optionsToCopy = new ShaderOption<string[]>[] {
                                        new ShaderOption<string[]> {
                                            Key = "_BaseMap",
                                            Value = new string[] { "_MainTex" }
                                        }
                                    },
                                    m_optionsBool = new ShaderOption<bool>[] {
                                        new ShaderOption<bool> {
                                            Key = "_OutputSmoothness",
                                            Value = true
                                        }
                                    },
                                    m_isTemporary = true,
                                },
                                new BakeShaderInfo {
                                    m_shader = Shader.Find("PWStorm/Shader Graphs/SH_ImposterNormalBake_URP"),
                                    m_fieldName = "_BumpMap",
                                    m_texName = "_BumpMap",
                                    m_type = BakeShaderOutputType.NORMAL,
                                    m_alphaUpdate = AlphaUpdateType.DEFAULT,
                                    m_optionsToCopy = new ShaderOption<string[]>[] {
                                        new ShaderOption<string[]> {
                                            Key = "_NormalMap",
                                            Value = new string[] { "_BumpMap" }
                                        }
                                    }
                                },
                            };
                default:
                    return new BakeShaderInfo[] {
                                new BakeShaderInfo {
                                    m_shader = null,
                                    m_fieldName = "Albedo",
                                    m_texName = "_Albedo",
                                    m_type = BakeShaderOutputType.DEFAULT,
                                    m_alphaUpdate = AlphaUpdateType.FILL
                                },
                                new BakeShaderInfo {
                                    m_shader = Shader.Find("Hidden/PWStorm/AmplifyBake/SpecularSmoothness"),
                                    m_fieldName = "SpecularSmoothness",
                                    m_texName = "_Specular",
                                    m_type = BakeShaderOutputType.DEFAULT,
                                    m_alphaUpdate = AlphaUpdateType.DEFAULT
                                },
                                new BakeShaderInfo {
                                    m_shader = Shader.Find("Hidden/PWStorm/AmplifyBake/EmissionOcclusion"),
                                    m_fieldName = "EmissionOcclusion",
                                    m_texName = "_Emission",
                                    m_type = BakeShaderOutputType.MASK,
                                    m_alphaUpdate = AlphaUpdateType.DEFAULT
                                },
                                new BakeShaderInfo {
                                    m_shader = Shader.Find("Hidden/PWStorm/AmplifyBake/NormalDepth"),
                                    m_fieldName = "NormalDepth",
                                    m_texName = "_Normals",
                                    m_type = BakeShaderOutputType.NORMAL,
                                    m_alphaUpdate = AlphaUpdateType.SATURATE
                                },
                            };
            }
        }
        /// <summary>
        /// Get default PostProcessing setup for a pipeline used in Impostor Baking
        /// </summary>
        /// <param name="pipeline">Pipeline to get default setup for</param>
        /// <returns>Array of PostProcessing steps to use for Impostor Baking</returns>
        public static PostProcessingInfo[] GetDefaultPostProcessings(StormRP pipeline)
        {
            switch (pipeline)
            {
                case StormRP.HDRP:
                case StormRP.URP:
                    return new PostProcessingInfo[] {
                                new PostProcessingInfo {
                                    Shader = Shader.Find("Hidden/ImpostorMergeTex"),
                                    Input = "_MainTex_0",
                                    Output = "_MainTex",
                                    OutputField = "_MainTex",
                                    Type = BakeShaderOutputType.DEFAULT,
                                    AlphaUpdate = AlphaUpdateType.SATURATE,
                                    Textures = new ShaderOption<string[]>[] {
                                        new ShaderOption<string[]> {
                                            Key = "_AlphaMap",
                                            Value = new string[] { "_MainTex_1" }
                                        }
                                    },
                                    IsMainAlpha = true,
                                },
                                new PostProcessingInfo {
                                    Shader = Shader.Find("Hidden/ImpostorMergeTex"),
                                    Input = "_MetallicGlossMap_0",
                                    Output = "_MetallicGlossMap",
                                    OutputField = "_MetallicGlossMap",
                                    Type = BakeShaderOutputType.MASK,
                                    AlphaUpdate = AlphaUpdateType.FILL,
                                    Textures = new ShaderOption<string[]>[] {
                                        new ShaderOption<string[]> {
                                            Key = "_AlphaMap",
                                            Value = new string[] { "_MetallicGlossMap_1" }
                                        }
                                    }
                                },
                            };
                default:
                    return new PostProcessingInfo[0];
            }
        }
        public static ImpostorShaderInfo GetDefaultFlatReplacement(StormRP pipeline)
        {
            switch (pipeline)
            {
                default:
                case StormRP.Builtin:
                    return new ImpostorShaderInfo {
                        m_shader = Shader.Find("Standard"),
                        m_propertyReplacements = new ShaderOption<string>[0]
                    };
                case StormRP.HDRP:
                    return new ImpostorShaderInfo {
                        m_shader = Shader.Find("HDRP/Lit"),
                        m_propertyReplacements = new ShaderOption<string>[] {
                            new ShaderOption<string>() {
                                Key = "_MainTex",
                                Value = "_BaseColorMap"
                            },
                            new ShaderOption<string>() {
                                Key = "_MetallicGlossMap",
                                Value = "_MaskMap"
                            },
                            new ShaderOption<string>() {
                                Key = "_BumpMap",
                                Value = "_NormalMap"
                            }
                        }
                    };
                case StormRP.URP:
                    return new ImpostorShaderInfo {
                        m_shader = Shader.Find("Universal Render Pipeline/Simple Lit"),
                        m_propertyReplacements = new ShaderOption<string>[] {
                            new ShaderOption<string>() {
                                Key = "_MainTex",
                                Value = "_BaseMap"
                            },
                            new ShaderOption<string>() {
                                Key = "_MetallicGlossMap",
                                Value = "_MaskMap"
                            }
                        }
                    };
            }
        }
        public static Material GetDefaultDebugLODMaterial(StormRP pipeline)
        {
            Shader shader = null;
            string shaderName = "DebugLODMat_";
            switch (pipeline)
            {
                case StormRP.Builtin:
                    shader = Shader.Find("Standard");
                    shaderName += "_builtin";
                    break;
                case StormRP.URP:
                    shader = Shader.Find("Universal Render Pipeline/Simple Lit");
                    shaderName += "_URP";
                    break;
                case StormRP.HDRP:
                    shader = Shader.Find("HDRP/Lit");
                    shaderName += "_HDRP";
                    break;
            }
            if (shader == null)
                shader = Shader.Find("Standard");
            Material ret = new Material(shader);
            ret.name = shaderName;
            return StormUtils.SaveAsset(ret, StormUtils.DefaultSavePath + "Materials/DebugLODMat.mat");
        }
        public static string GetDefaultDebugLODColorProp(StormRP pipeline)
        {
            return pipeline switch {
                StormRP.Builtin => "_Color",
                StormRP.HDRP => "_BaseColor",
                StormRP.URP => "_BaseColor",
                _ => "_Color"
            };
        }

        [SerializeField]
        internal FeaturesetFeature[] m_disabledFeatures = System.Array.Empty<FeaturesetFeature>();
        public bool IsFeatureDisabled(StormFeatureset featureset, StormFeatureSettings feature)
        {
            for (int i = 0; i < m_disabledFeatures.Length; ++i)
            {
                if (m_disabledFeatures[i].FeatureSet == featureset && m_disabledFeatures[i].Feature == feature)
                    return true;
            }
            return false;
        }
        public bool SetFeatureDisabled(StormFeatureset featureset, StormFeatureSettings feature, bool disabled)
        {
            int idx = -1;
            for (int i = 0; i < m_disabledFeatures.Length; ++i)
            {
                if (m_disabledFeatures[i].FeatureSet == featureset && m_disabledFeatures[i].Feature == feature)
                {
                    idx = i;
                    break;
                }
            }
            if (disabled)
            {
                if (idx < 0)
                {
                    idx = m_disabledFeatures.Length;
                    System.Array.Resize(ref m_disabledFeatures, idx + 1);
                    m_disabledFeatures[idx] = new FeaturesetFeature(featureset, feature);
                    StormUtils.SetAssetDirty(this);
                    StormAPI.OnFeatureDisabled?.Invoke(featureset, feature);
                    return true;
                }
            }
            else
            {
                if (idx >= 0)
                {
                    StormUtils.RemoveAt(ref m_disabledFeatures, idx);
                    StormUtils.SetAssetDirty(this);
                    StormAPI.OnFeatureEnabled?.Invoke(featureset, feature);
                    return true;
                }
            }
            return false;
        }

        public void Cleanup()
        {
            for (int d = 0; d < m_disabledFeatures.Length; ++d)
            {
                if (m_disabledFeatures[d].Feature == null || m_disabledFeatures[d].FeatureSet == null)
                    StormUtils.RemoveAt(ref m_disabledFeatures, d--);
            }
        }

        /// <summary>
        /// Is a StormWorld currently active?
        /// </summary>
        [Obsolete("Use StormAPI for these callbacks")]
        public static bool IsStormWorldActive => StormAPI.IsStormWorldActive;
        /// <summary>
        /// Action Invoked when StormWorld instance has changed
        /// </summary>
        [Obsolete("Use StormAPI for these callbacks")]
        public static System.Action<MonoBehaviour> OnStormInstanceChanged => StormAPI.OnStormInstanceChanged;
        /// <summary>
        /// Action to Invoke when adding a new StormTile
        /// </summary>
        [Obsolete("Use StormAPI for these callbacks")]
        public static System.Action<StormTileData> OnStormTileAdded => StormAPI.OnStormTileAdded;
        /// <summary>
        /// Action to Invoke when updating a new StormTile
        /// </summary>
        [Obsolete("Use StormAPI for these callbacks")]
        public static System.Action<StormTileData, Bounds> OnStormTileUpdated => StormAPI.OnStormTileUpdated;
        /// <summary>
        /// Action to Invoke when removing a new StormTile
        /// </summary>
        [Obsolete("Use StormAPI for these callbacks")]
        public static System.Action<StormTileData> OnStormTileRemoved => StormAPI.OnStormTileRemoved;
        /// <summary>
        /// Action to Invoke when adding a new FeaturesetTile
        /// </summary>
        [Obsolete("Use StormAPI for these callbacks")]
        public static System.Action<StormTileRegion> OnStormTileFeaturesetAdded => StormAPI.OnStormTileRegionAdded;
        /// <summary>
        /// Action to Invoke when updating a new FeaturesetTile
        /// </summary>
        [Obsolete("Use StormAPI for these callbacks")]
        public static System.Action<StormTileRegion, Bounds> OnStormTileFeaturesetUpdated => StormAPI.OnStormTileRegionUpdated;
        /// <summary>
        /// Action to Invoke when removing a new FeaturesetTile
        /// </summary>
        [Obsolete("Use StormAPI for these callbacks")]
        public static System.Action<StormTileRegion> OnStormTileFeaturesetRemoved => StormAPI.OnStormTileRegionRemoved;
        /// <summary>
        /// Action to Invoke when removing a new FeaturesetTile
        /// </summary>
        [Obsolete("Use StormAPI for these callbacks")]
        public static System.Action<StormFeatureset, StormFeatureSettings> OnFeatureDisabled => StormAPI.OnFeatureDisabled;
        /// <summary>
        /// Action to Invoke when removing a new FeaturesetTile
        /// </summary>
        [Obsolete("Use StormAPI for these callbacks")]
        public static System.Action<StormFeatureset, StormFeatureSettings> OnFeatureEnabled => StormAPI.OnFeatureEnabled;

        /// <summary>
        /// Action invoked when a StormFeatureSettings OnEnable() is called (for updating the list of all known featuresets when one is added in editor)
        /// </summary>
        public static System.Action<StormFeatureSettings> OnFeatureSettingsEnabled;
        /// <summary>
        /// Action invoked when a Impostor OnEnable() is called (for updating the list of all known Impostors when one is added in editor)
        /// </summary>
        public static System.Action<Impostor> OnImpostorEnabled;

        public void CopyFrom(StormSystemSettings settings)
        {
            m_loadDistance = settings.m_loadDistance;
            m_unloadDistance = settings.m_unloadDistance;
            m_renderCellSize = settings.m_renderCellSize;
            m_splitDenseCells = settings.m_splitDenseCells;
            m_splitInstanceCount = settings.m_splitInstanceCount;
            m_splitSubgridCount = settings.m_splitSubgridCount;
            m_loadingMapPadding = settings.m_loadingMapPadding;

            AlwaysRasterizePolygons = settings.AlwaysRasterizePolygons;
            DefaultPolygonResolution = settings.DefaultPolygonResolution;
            MaxTotalPolygonResolution = settings.MaxTotalPolygonResolution;
            ExtraMaterialMode = settings.ExtraMaterialMode;
            m_allowFrameDelay = settings.m_allowFrameDelay;
            m_cullTurnOffset = settings.m_cullTurnOffset;
            m_curveResolution = settings.m_curveResolution;
            m_maxLoadMasksPerFrame = settings.m_maxLoadMasksPerFrame;
            m_maxSimultaneousLoad = settings.m_maxSimultaneousLoad;
            m_minUpdateMovement = settings.m_minUpdateMovement;
            m_minUpdateTurn = settings.m_minUpdateTurn;
            m_renderType = settings.m_renderType;
            m_shadowDistanceMultiplier = settings.m_shadowDistanceMultiplier;
            m_shadowDistanceSRP = settings.m_shadowDistanceSRP;
            m_shadowDistanceMult = settings.m_shadowDistanceMult;
            m_splitShadowRendering = settings.m_splitShadowRendering;
            m_useJobs = settings.m_useJobs;

            m_cullType = settings.m_cullType;
            m_cullShader = settings.m_cullShader;
            m_cullKernelStandard = settings.m_cullKernelStandard;
            m_cullKernelSimple = settings.m_cullKernelSimple;
            m_cullKernelShadowOnly = settings.m_cullKernelShadowOnly;
            m_disabledFeatures = StormUtils.CopyArray(settings.m_disabledFeatures);
            ReplaceRenderShaders = settings.ReplaceRenderShaders;
            ReplacementShader = settings.ReplacementShader;
            _defaultDiffusionProfile = settings._defaultDiffusionProfile;
            m_defaultDiffusionProfile_HDRP = settings.m_defaultDiffusionProfile_HDRP;
            DebugLODColorEnd = settings.DebugLODColorEnd;
            DebugLODColorStart = settings.DebugLODColorStart;
            DebugLODMaterial = settings.DebugLODMaterial;
            DebugLODColorProperty_HDRP = settings.DebugLODColorProperty_HDRP;
            DebugLODMaterial_HDRP = settings.DebugLODMaterial_HDRP;
            DebugLODColorProperty_URP = settings.DebugLODColorProperty_URP;
            DebugLODMaterial_URP = settings.DebugLODMaterial_URP;
            DebugLODColorProperty_BuiltIn = settings.DebugLODColorProperty_BuiltIn;
            DebugLODMaterial_BuiltIn = settings.DebugLODMaterial_BuiltIn;
            LoaderTypeRanges = StormUtils.CopyArray(settings.LoaderTypeRanges);
            ImpostorExportDirectory = settings.ImpostorExportDirectory;
            ImpostorOctoSteps = settings.ImpostorOctoSteps;
            ImpostorResolution = settings.ImpostorResolution;
            AllowTypeOverrideSize = settings.AllowTypeOverrideSize;
            AllowImpostorTypeOverride = settings.AllowImpostorTypeOverride;
            ImpostorType = settings.ImpostorType;
            m_impostorFlatReplacementURP = settings.m_impostorFlatReplacementURP;
            m_impostorFlatReplacementHDRP = settings.m_impostorFlatReplacementHDRP;
            m_impostorFlatReplacementBuiltIn = settings.m_impostorFlatReplacementBuiltIn;
            m_impostorPostProcessHDRP = StormUtils.CopyArray(settings.m_impostorPostProcessHDRP);
            m_impostorPostProcessURP = StormUtils.CopyArray(settings.m_impostorPostProcessURP);
            m_impostorPostProcessBuiltIn = StormUtils.CopyArray(settings.m_impostorPostProcessBuiltIn);
            m_impostorBakeShadersHDRP = StormUtils.CopyArray(settings.m_impostorBakeShadersHDRP);
            m_impostorBakeShadersURP = StormUtils.CopyArray(settings.m_impostorBakeShadersURP);
            m_impostorBakeShadersBuiltIn = StormUtils.CopyArray(settings.m_impostorBakeShadersBuiltIn);
            m_billboardStrings = StormUtils.CopyArray(settings.m_billboardStrings);
            CreateImpostors = settings.CreateImpostors;
            ObjectMediumSize = settings.ObjectMediumSize;
            ObjectSmallSize = settings.ObjectSmallSize;
            IngestNonStatic = settings.IngestNonStatic;
            IngestStatic = settings.IngestStatic;
            IgnoredTags = StormUtils.CopyArray(settings.IgnoredTags);
            IgnoredLayers = StormUtils.CopyArray(settings.IgnoredLayers);
            IgnoredPrefabs = StormUtils.CopyArray(settings.IgnoredPrefabs);
            DefaultRenderType = settings.DefaultRenderType;
            DefaultDensity = settings.DefaultDensity;
            DefaultLODMix = settings.DefaultLODMix;
            DefaultLODFade = settings.DefaultLODFade;
            DefaultDetailCreateImpostor = settings.DefaultDetailCreateImpostor;
            DefaultDetailLODs = StormUtils.CopyArray(settings.DefaultDetailLODs);
            DefaultLODProgression = new AnimationCurve(settings.DefaultLODProgression.keys);
            m_instantFirstLoad = settings.m_instantFirstLoad;
            m_cacheTerrainData = settings.m_cacheTerrainData;
            m_preventDuplicateInstances = settings.m_preventDuplicateInstances;
            m_maxInstancesPerFrame = settings.m_maxInstancesPerFrame;
        }
#pragma warning disable CS0618 // Type or member is obsolete
        public void CopyFrom(StormWorldSettings settings)
#pragma warning restore CS0618 // Type or member is obsolete
        {
            m_loadDistance = settings.m_loadDistance;
            m_unloadDistance = settings.m_unloadDistance;
            m_renderCellSize = settings.m_renderCellSize;

            AlwaysRasterizePolygons = settings.AlwaysRasterizePolygons;
            DefaultPolygonResolution = settings.DefaultPolygonResolution;
            MaxTotalPolygonResolution = settings.MaxTotalPolygonResolution;
            m_allowFrameDelay = settings.m_allowFrameDelay;
            m_cullTurnOffset = settings.m_cullTurnOffset;
            m_curveResolution = settings.m_curveResolution;
            m_maxLoadMasksPerFrame = settings.m_maxLoadMasksPerFrame;
            m_maxSimultaneousLoad = settings.m_maxSimultaneousLoad;
            m_minUpdateMovement = settings.m_minUpdateMovement;
            m_minUpdateTurn = settings.m_minUpdateTurn;
            m_renderType = settings.m_renderType;
            m_shadowDistanceMultiplier = settings.m_shadowDistanceMultiplier;
            m_shadowDistanceSRP = settings.m_shadowDistanceSRP;
            m_shadowDistanceMult = settings.m_shadowExtensionMult;
            m_splitShadowRendering = settings.m_splitShadowRendering;
            m_useJobs = settings.m_useJobs;

            m_cullType = settings.m_cullType;
            m_cullShader = settings.m_cullShader;
            m_cullKernelStandard = settings.m_cullKernelStandard;
            m_cullKernelSimple = settings.m_cullKernelSimple;
            m_cullKernelShadowOnly = settings.m_cullKernelShadowOnly;
            m_disabledFeatures = StormUtils.CopyArray(settings.m_disabledFeatures);
            ReplaceRenderShaders = settings.ReplaceRenderShaders;
            ReplacementShader = settings.ReplacementShader;
            _defaultDiffusionProfile = settings._defaultDiffusionProfile;
            m_defaultDiffusionProfile_HDRP = settings.m_defaultDiffusionProfile_HDRP;
            DebugLODColorEnd = settings.DebugLODColorEnd;
            DebugLODColorStart = settings.DebugLODColorStart;
            DebugLODMaterial = settings.DebugLODMaterial;
            DebugLODColorProperty_HDRP = settings.DebugLODColorProperty_HDRP;
            DebugLODMaterial_HDRP = settings.DebugLODMaterial_HDRP;
            DebugLODColorProperty_URP = settings.DebugLODColorProperty_URP;
            DebugLODMaterial_URP = settings.DebugLODMaterial_URP;
            DebugLODColorProperty_BuiltIn = settings.DebugLODColorProperty_BuiltIn;
            DebugLODMaterial_BuiltIn = settings.DebugLODMaterial_BuiltIn;
            LoaderTypeRanges = StormUtils.CopyArray(settings.LoaderTypeRanges);
            ImpostorExportDirectory = settings.ImpostorExportDirectory;
            ImpostorOctoSteps = settings.ImpostorOctoSteps;
            ImpostorResolution = settings.ImpostorResolution;
            AllowTypeOverrideSize = settings.AllowTypeOverrideSize;
            AllowImpostorTypeOverride = settings.AllowImpostorTypeOverride;
            ImpostorType = settings.ImpostorType;
            m_impostorFlatReplacementURP = settings.m_impostorFlatReplacementURP;
            m_impostorFlatReplacementHDRP = settings.m_impostorFlatReplacementHDRP;
            m_impostorFlatReplacementBuiltIn = settings.m_impostorFlatReplacementBuiltIn;
            m_impostorPostProcessHDRP = StormUtils.CopyArray(settings.m_impostorPostProcessHDRP);
            m_impostorPostProcessURP = StormUtils.CopyArray(settings.m_impostorPostProcessURP);
            m_impostorPostProcessBuiltIn = StormUtils.CopyArray(settings.m_impostorPostProcessBuiltIn);
            m_impostorBakeShadersHDRP = StormUtils.CopyArray(settings.m_impostorBakeShadersHDRP);
            m_impostorBakeShadersURP = StormUtils.CopyArray(settings.m_impostorBakeShadersURP);
            m_impostorBakeShadersBuiltIn = StormUtils.CopyArray(settings.m_impostorBakeShadersBuiltIn);
            m_billboardStrings = StormUtils.CopyArray(settings.m_billboardStrings);
            CreateImpostors = settings.CreateImpostors;
            ObjectMediumSize = settings.ObjectMediumSize;
            ObjectSmallSize = settings.ObjectSmallSize;
            IngestNonStatic = settings.IngestNonStatic;
            IngestStatic = settings.IngestStatic;
            IgnoredTags = StormUtils.CopyArray(settings.IgnoredTags);
            IgnoredLayers = StormUtils.CopyArray(settings.IgnoredLayers);
            IgnoredPrefabs = StormUtils.CopyArray(settings.IgnoredPrefabs);
            DefaultRenderType = settings.DefaultRenderType;
            DefaultDensity = settings.DefaultDensity;
            DefaultLODMix = settings.DefaultLODMix;
            DefaultLODFade = settings.DefaultLODFade;
            DefaultDetailCreateImpostor = settings.DefaultDetailCreateImpostor;
            DefaultDetailLODs = StormUtils.CopyArray(settings.DefaultDetailLODs);
            DefaultLODProgression = new AnimationCurve(settings.DefaultLODProgression.keys);
            m_instantFirstLoad = settings.m_instantFirstLoad;
            m_maxInstancesPerFrame = settings.m_maxInstancesPerFrame;
        }
    }
}