using UnityEngine;

/// Copyright 2023 Procedural Worlds Ltd
/// Written by Jonathan Williams as part of Storm

namespace ProceduralWorlds.Storm.Utils
{
    /// <summary>
    /// Scriptable Object to hold settings used by all instances of Storm
    /// </summary>
    [System.Obsolete("Use StormQualitySettings or StormSystemSettings instead.")]
    public class StormWorldSettings : ScriptableObject
    {
        [Header("LOD Bias")]
        /// <summary>
        /// LODBias applied to all distances for LOD selection (as Pow() function)
        /// </summary>
        [Min(0.001f)]
        [Tooltip("LODBias applied to all distances for LOD selection (as Pow() function)")]
        public float m_globalLODBias = 1;
        public float m_globalLODBiasMaxUI = 10;
        /// <summary>
        /// LODBias applied to Grass distances for LOD selection (as Pow() function)
        /// </summary>
        [Min(0.001f)]
        [Tooltip("LODBias applied to Grass distances for LOD selection (as Pow() function)")]
        public float m_grassLODBias = 1;
        public float m_grassLODBiasMaxUI = 10;
        /// <summary>
        /// LODBias applied to Detail distances for LOD selection (as Pow() function)
        /// </summary>
        [Min(0.001f)]
        [Tooltip("LODBias applied to Detail distances for LOD selection (as Pow() function)")]
        public float m_detailLODBias = 1;
        public float m_detailLODBiasMaxUI = 10;
        /// <summary>
        /// LODBias applied to Plant distances for LOD selection (as Pow() function)
        /// </summary>
        [Min(0.001f)]
        [Tooltip("LODBias applied to Plant distances for LOD selection (as Pow() function)")]
        public float m_plantLODBias = 1;
        public float m_plantLODBiasMaxUI = 10;
        /// <summary>
        /// LODBias applied to Tree distances for LOD selection (as Pow() function)
        /// </summary>
        [Min(0.001f)]
        [Tooltip("LODBias applied to Tree distances for LOD selection (as Pow() function)")]
        public float m_treeLODBias = 1;
        public float m_treeLODBiasMaxUI = 10;
        /// <summary>
        /// LODBias applied to Small Object distances for LOD selection (as Pow() function)
        /// </summary>
        [Min(0.001f)]
        [Tooltip("LODBias applied to Small Object distances for LOD selection (as Pow() function)")]
        public float m_smallObjLODBias = 1;
        public float m_smallObjLODBiasMaxUI = 10;
        /// <summary>
        /// LODBias applied to Medium Object distances for LOD selection (as Pow() function)
        /// </summary>
        [Min(0.001f)]
        [Tooltip("LODBias applied to Medium Object distances for LOD selection (as Pow() function)")]
        public float m_mediumObjLODBias = 1;
        public float m_mediumObjLODBiasMaxUI = 10;
        /// <summary>
        /// LODBias applied to Large Object distances for LOD selection (as Pow() function)
        /// </summary>
        [Min(0.001f)]
        [Tooltip("LODBias applied to Large Object distances for LOD selection (as Pow() function)")]
        public float m_largeObjLODBias = 1;
        public float m_largeObjLODBiasMaxUI = 10;

        [Header("Density Settings")]
        /// <summary>
        /// Density multiplier to apply to Grass objects
        /// </summary>
        [Tooltip("Density multiplier to apply to Grass objects")]
        public float m_grassDensity = 1;
        public float m_grassDensityMaxUI = 10;
        /// <summary>
        /// Density multiplier to apply to Detail objects
        /// </summary>
        [Tooltip("Density multiplier to apply to Detail objects")]
        public float m_detailDensity = 1;
        public float m_detailDensityMaxUI = 10;
        /// <summary>
        /// Density multiplier to apply to Plant objects
        /// </summary>
        [Tooltip("Density multiplier to apply to Plant objects")]
        public float m_plantDensity = 1;
        public float m_plantDensityMaxUI = 10;
        /// <summary>
        /// Density multiplier to apply to Tree objects
        /// </summary>
        [Tooltip("Density multiplier to apply to Tree objects")]
        public float m_treeDensity = 1;
        public float m_treeDensityMaxUI = 10;
        /// <summary>
        /// Density multiplier to apply to Small objects
        /// </summary>
        [Tooltip("Density multiplier to apply to Small objects")]
        public float m_smallObjDensity = 1;
        public float m_smallObjDensityMaxUI = 10;
        /// <summary>
        /// Density multiplier to apply to Medium objects
        /// </summary>
        [Tooltip("Density multiplier to apply to Medium objects")]
        public float m_mediumObjDensity = 1;
        public float m_mediumObjDensityMaxUI = 10;
        /// <summary>
        /// Density multiplier to apply to Large objects
        /// </summary>
        [Tooltip("Density multiplier to apply to Large objects")]
        public float m_largeObjDensity = 1;
        public float m_largeObjDensityMaxUI = 10;

        [Header("Draw Distances")]
        /// <summary>
        /// Distance to draw Grass objects out to
        /// </summary>
        [Tooltip("Distance to draw Grass objects out to")]
        public float m_grassDistance = 50;
        public float m_grassDistanceMaxUI = 1024;
        /// <summary>
        /// Distance to draw Detail objects out to
        /// </summary>
        [Tooltip("Distance to draw Detail objects out to")]
        public float m_detailDistance = 100;
        public float m_detailDistanceMaxUI = 1024;
        /// <summary>
        /// Distance to draw Plant objects out to
        /// </summary>
        [Tooltip("Distance to draw Plant objects out to")]
        public float m_plantDistance = 200;
        public float m_plantDistanceMaxUI = 2048;
        /// <summary>
        /// Distance to draw Tree objects out to
        /// </summary>
        [Tooltip("Distance to draw Tree objects out to")]
        public float m_treeDistance = 1200;
        public float m_treeDistanceMaxUI = 4096;
        /// <summary>
        /// Distance to draw Small objects out to
        /// </summary>
        [Tooltip("Distance to draw Small objects out to")]
        public float m_smallObjDistance = 50;
        public float m_smallObjDistanceMaxUI = 1024;
        /// <summary>
        /// Distance to draw Medium objects out to
        /// </summary>
        [Tooltip("Distance to draw Medium objects out to")]
        public float m_mediumObjDistance = 100;
        public float m_mediumObjDistanceMaxUI = 2048;
        /// <summary>
        /// Distance to draw Large objects out to
        /// </summary>
        [Tooltip("Distance to draw Large objects out to")]
        public float m_largeObjDistance = 1200;
        public float m_largeObjDistanceMaxUI = 4098;
        /// <summary>
        /// Distance to create Colliders out to
        /// </summary>
        [Tooltip("Distance to create Colliders out to")]
        public float m_colliderDistance = 200;
        public float m_colliderDistanceMaxUI = 1024;

        [Header("Scaleout Multipliers")]
        /// <summary>
        /// Multiplier to apply to all Grass Scaleout values
        /// </summary>
        [Min(0)]
        [Tooltip("Multiplier to apply to all Grass Scaleout values")]
        public float m_grassScaleoutMult = 1;
        public float m_grassScaleoutMultMaxUI = 100;
        /// <summary>
        /// Multiplier to apply to all Detail Scaleout values
        /// </summary>
        [Min(0)]
        [Tooltip("Multiplier to apply to all Detail Scaleout values")]
        public float m_detailScaleoutMult = 1;
        public float m_detailScaleoutMultMaxUI = 100;
        /// <summary>
        /// Multiplier to apply to all Plant Scaleout values
        /// </summary>
        [Min(0)]
        [Tooltip("Multiplier to apply to all Plant Scaleout values")]
        public float m_plantScaleoutMult = 1;
        public float m_plantScaleoutMultMaxUI = 100;
        /// <summary>
        /// Multiplier to apply to all Tree Scaleout values
        /// </summary>
        [Min(0)]
        [Tooltip("Multiplier to apply to all Tree Scaleout values")]
        public float m_treeScaleoutMult = 1;
        public float m_treeScaleoutMultMaxUI = 100;
        /// <summary>
        /// Multiplier to apply to all Small Object Scaleout values
        /// </summary>
        [Min(0)]
        [Tooltip("Multiplier to apply to all Small Object Scaleout values")]
        public float m_smallObjScaleoutMult = 1;
        public float m_smallObjScaleoutMultMaxUI = 100;
        /// <summary>
        /// Multiplier to apply to all Medium Object Scaleout values
        /// </summary>
        [Min(0)]
        [Tooltip("Multiplier to apply to all Medium Object Scaleout values")]
        public float m_mediumObjScaleoutMult = 1;
        public float m_mediumObjScaleoutMultMaxUI = 100;
        /// <summary>
        /// Multiplier to apply to all Large Object Scaleout values
        /// </summary>
        [Min(0)]
        [Tooltip("Multiplier to apply to all Large Object Scaleout values")]
        public float m_largeObjScaleoutMult = 1;
        public float m_largeObjScaleoutMultMaxUI = 100;

        [Header("LOD Mix Multipliers")]
        /// <summary>
        /// Multiplier to apply to all Grass LODMix values
        /// </summary>
        [Min(0)]
        [Tooltip("Multiplier to apply to all Grass LODMix values")]
        public float m_grassLODMixMult = 1;
        public float m_grassLODMixMultMaxUI = 100;
        /// <summary>
        /// Multiplier to apply to all Detail LODMix values
        /// </summary>
        [Min(0)]
        [Tooltip("Multiplier to apply to all Detail LODMix values")]
        public float m_detailLODMixMult = 1;
        public float m_detailLODMixMultMaxUI = 100;
        /// <summary>
        /// Multiplier to apply to all Plant LODMix values
        /// </summary>
        [Min(0)]
        [Tooltip("Multiplier to apply to all Plant LODMix values")]
        public float m_plantLODMixMult = 1;
        public float m_plantLODMixMultMaxUI = 100;
        /// <summary>
        /// Multiplier to apply to all Tree LODMix values
        /// </summary>
        [Min(0)]
        [Tooltip("Multiplier to apply to all Tree LODMix values")]
        public float m_treeLODMixMult = 1;
        public float m_treeLODMixMultMaxUI = 100;
        /// <summary>
        /// Multiplier to apply to all Small Object LODMix values
        /// </summary>
        [Min(0)]
        [Tooltip("Multiplier to apply to all Small Object LODMix values")]
        public float m_smallObjLODMixMult = 1;
        public float m_smallObjLODMixMultMaxUI = 100;
        /// <summary>
        /// Multiplier to apply to all Medium Object LODMix values
        /// </summary>
        [Min(0)]
        [Tooltip("Multiplier to apply to all Medium Object LODMix values")]
        public float m_mediumObjLODMixMult = 1;
        public float m_mediumObjLODMixMultMaxUI = 100;
        /// <summary>
        /// Multiplier to apply to all Large Object LODMix values
        /// </summary>
        [Min(0)]
        [Tooltip("Multiplier to apply to all Large Object LODMix values")]
        public float m_largeObjLODMixMult = 1;
        public float m_largeObjLODMixMultMaxUI = 100;

        [Header("Density Fade Multipliers")]
        /// <summary>
        /// Multiplier to apply to all Grass Density Fade values
        /// </summary>
        [Min(0)]
        [Tooltip("Multiplier to apply to all Grass Density Fade values")]
        public float m_grassDensityFadeMult = 1;
        public float m_grassDensityFadeMultMaxUI = 100;
        /// <summary>
        /// Multiplier to apply to all Detail Density Fade values
        /// </summary>
        [Min(0)]
        [Tooltip("Multiplier to apply to all Detail Density Fade values")]
        public float m_detailDensityFadeMult = 1;
        public float m_detailDensityFadeMultMaxUI = 100;
        /// <summary>
        /// Multiplier to apply to all Plant Density Fade values
        /// </summary>
        [Min(0)]
        [Tooltip("Multiplier to apply to all Plant Density Fade values")]
        public float m_plantDensityFadeMult = 1;
        public float m_plantDensityFadeMultMaxUI = 100;
        /// <summary>
        /// Multiplier to apply to all Tree Density Fade values
        /// </summary>
        [Min(0)]
        [Tooltip("Multiplier to apply to all Tree Density Fade values")]
        public float m_treeDensityFadeMult = 1;
        public float m_treeDensityFadeMultMaxUI = 100;
        /// <summary>
        /// Multiplier to apply to all Small Object Density Fade values
        /// </summary>
        [Min(0)]
        [Tooltip("Multiplier to apply to all Small Object Density Fade values")]
        public float m_smallObjDensityFadeMult = 1;
        public float m_smallObjDensityFadeMultMaxUI = 100;
        /// <summary>
        /// Multiplier to apply to all Medium Object Density Fade values
        /// </summary>
        [Min(0)]
        [Tooltip("Multiplier to apply to all Medium Object Density Fade values")]
        public float m_mediumObjDensityFadeMult = 1;
        public float m_mediumObjDensityFadeMultMaxUI = 100;
        /// <summary>
        /// Multiplier to apply to all Large Object Density Fade values
        /// </summary>
        [Min(0)]
        [Tooltip("Multiplier to apply to all Large Object Density Fade values")]
        public float m_largeObjDensityFadeMult = 1;
        public float m_largeObjDensityFadeMultMaxUI = 100;

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

        [Header("Target FPS")]
        [Tooltip("Target FPS to attempt to maintain (values <=0 disables this system)")]
        /// <summary>
        /// Target FPS to attempt to maintain (values <=0 disables this system)
        /// </summary>
        public int m_targetFPS = -1;
        [Tooltip("Maximum FPS over TargetFPS before starting to increase values")]
        /// <summary>
        /// Maximum FPS over TargetFPS before starting to increase values
        /// </summary>
        public int m_targetFPSMaxOver = 10;
        [Tooltip("Number of frames between each TargetFPS update")]
        /// <summary>
        /// Number of frames between each TargetFPS update
        /// </summary>
        public int m_targetFPSFrequency = 30;
        /// <summary>
        /// How much should Grass Draw Distance be adjusted when adjusting for FPS target?
        /// </summary>
        [Range(0, 1)]
        [Tooltip("How much should Grass Draw Distance be adjusted when adjusting for FPS target?")]
        public float m_fpsAdjustDistGrass = 0.2f;
        /// <summary>
        /// How much should Detail Draw Distance be adjusted when adjusting for FPS target?
        /// </summary>
        [Range(0, 1)]
        [Tooltip("How much should Detail Draw Distance be adjusted when adjusting for FPS target?")]
        public float m_fpsAdjustDistDetail = 0.2f;
        /// <summary>
        /// How much should Plant Draw Distance be adjusted when adjusting for FPS target?
        /// </summary>
        [Range(0, 1)]
        [Tooltip("How much should Plant Draw Distance be adjusted when adjusting for FPS target?")]
        public float m_fpsAdjustDistPlant = 0.2f;
        /// <summary>
        /// How much should Tree Draw Distance be adjusted when adjusting for FPS target?
        /// </summary>
        [Range(0, 1)]
        [Tooltip("How much should Tree Draw Distance be adjusted when adjusting for FPS target?")]
        public float m_fpsAdjustDistTree = 0.2f;
        /// <summary>
        /// How much should Small Object Draw Distance be adjusted when adjusting for FPS target?
        /// </summary>
        [Range(0, 1)]
        [Tooltip("How much should Small Object Draw Distance be adjusted when adjusting for FPS target?")]
        public float m_fpsAdjustDistSmallObj = 0.2f;
        /// <summary>
        /// How much should Medium Object Draw Distance be adjusted when adjusting for FPS target?
        /// </summary>
        [Range(0, 1)]
        [Tooltip("How much should Medium Object Draw Distance be adjusted when adjusting for FPS target?")]
        public float m_fpsAdjustDistMediumObj = 0.2f;
        /// <summary>
        /// How much should Large Object Draw Distance be adjusted when adjusting for FPS target?
        /// </summary>
        [Range(0, 1)]
        [Tooltip("How much should Large Object Draw Distance be adjusted when adjusting for FPS target?")]
        public float m_fpsAdjustDistLargeObj = 0.2f;
        [Tooltip("How much should LOD be adjusted when adjusting for FPS target?")]
        /// <summary>
        /// How much should LOD be adjusted when adjusting for FPS target?
        /// </summary>
        [Range(0, 1)]
        public float m_fpsAdjustLODBias = 0.5f;
        [Space]
        /// <summary>
        /// Minimum FPSBias modifier to allow
        /// </summary>
        [Tooltip("Minimum FPSBias modifier to allow")]
        [Range(0.01f, 1)]
        public float m_fpsMinLODBias = 0.1f;
        /// <summary>
        /// Minimum Grass Distance modifier to allow
        /// </summary>
        [Tooltip("Minimum Grass Distance modifier to allow")]
        [Range(0.01f, 1)]
        public float m_fpsMinDistGrass = 0.1f;
        /// <summary>
        /// Minimum Detail Distance modifier to allow
        /// </summary>
        [Tooltip("Minimum Detail Distance modifier to allow")]
        [Range(0.01f, 1)]
        public float m_fpsMinDistDetail = 0.1f;
        /// <summary>
        /// Minimum Plant Distance modifier to allow
        /// </summary>
        [Tooltip("Minimum Plant Distance modifier to allow")]
        [Range(0.01f, 1)]
        public float m_fpsMinDistPlant = 0.1f;
        /// <summary>
        /// Minimum Tree Distance modifier to allow
        /// </summary>
        [Tooltip("Minimum Tree Distance modifier to allow")]
        [Range(0.01f, 1)]
        public float m_fpsMinDistTree = 0.1f;
        /// <summary>
        /// Minimum Small Object Distance modifier to allow
        /// </summary>
        [Tooltip("Minimum Small Object Distance modifier to allow")]
        [Range(0.01f, 1)]
        public float m_fpsMinDistSmallObj = 0.1f;
        /// <summary>
        /// Minimum Medium Object Distance modifier to allow
        /// </summary>
        [Tooltip("Minimum Medium Object Distance modifier to allow")]
        [Range(0.01f, 1)]
        public float m_fpsMinDistMediumObj = 0.1f;
        /// <summary>
        /// Minimum Large Object Distance modifier to allow
        /// </summary>
        [Tooltip("Minimum Large Object Distance modifier to allow")]
        [Range(0.01f, 1)]
        public float m_fpsMinDistLargeObj = 0.1f;

        [Header("Data Resolutions")]
        [SerializeField]
        [UnityEngine.Serialization.FormerlySerializedAs("m_worldTileSize")]
        [Min(1f)]
        [Tooltip("Size of Render Cells used for culling and loading. larger = better for fewer objects. dense worlds should use lower values (around 128)")]
        public float m_renderCellSize = 128f;
        /// <summary>
        /// Number of steps to use for sampling Curves for Jobs
        /// </summary>
        [Tooltip("Number of steps to use for sampling Curves for Jobs")]
        public int m_curveResolution = 16;
        /// <summary>
        /// Resolution of Height data per 100m
        /// </summary>
        [Tooltip("Resolution of Height data per 100m")]
        public int m_heightResolution = 32;

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
        /// Shader used for counting instances and preparing indirect buffer for Finalize shader's execution
        /// </summary>
        [Tooltip("Shader used for counting instances and preparing indirect buffer for Finalize shader's execution")]
        public ComputeShader m_cullCountShader = null;
        /// <summary>
        /// Kernel index in the Cull Count Shader to use
        /// </summary>
        public int m_cullCountKernel = 0;

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
        /// Multiplier applied to turn speed of camera (starts as a unit length vector) to prevent pop-in from turning
        /// </summary>
        public float m_cullTurnOffset = 1;
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
        /// Minimum distance in world units that the camera must move before forcing a culling update
        /// </summary>
        public float m_minUpdateMovement = 0.5f;
        /// <summary>
        /// Minimum distance in world units that the camera or sun light must turn before forcing a culling update
        /// </summary>
        public float m_minUpdateTurn = 0.25f;
        /// <summary>
        /// Minimum distance in world units that the camera or sun light must turn before forcing a culling update
        /// </summary>
        public float m_shadowExtensionMult = 10f;
        /// <summary>
        /// Should we use the new long shadow detection algorithm?
        /// (currently slightly broken so this is exposed to allow testing)
        /// </summary>
        public bool m_useNewShadowCalc = false;

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

        [Header("Prefab Creation Settings")]
        /// <summary>
        /// Progression of LODs to use when creating new Prefabs. X axis is LOD level, Y axis is distance
        /// </summary>
        [Tooltip("Progression of LODs to use when creating new Prefabs. X axis is LOD level, Y axis is distance")]
        public AnimationCurve DefaultLODProgression = AnimationCurve.EaseInOut(0,0,1f,0.5f);
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
        public int DefaultDensity = 10;
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
        /// Max Size of object prefab to ingest as "Small"
        /// </summary>
        [Tooltip("Max Size of object prefab to ingest as \"Small\"")]
        public float ObjectSmallSize = 5;
        /// <summary>
        /// Max Size of object prefab to ingest as "Medium"
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
                    ret= GetDefaultDebugLODColorProp(pipeline);
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
                    OnFeatureDisabled?.Invoke(featureset, feature);
                    return true;
                }
            }
            else
            {
                if (idx >= 0)
                {
                    StormUtils.RemoveAt(ref m_disabledFeatures, idx);
                    StormUtils.SetAssetDirty(this);
                    OnFeatureEnabled?.Invoke(featureset, feature);
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Is a StormWorld currently active?
        /// </summary>
        public static bool IsStormWorldActive = false;
        /// <summary>
        /// Action Invoked when StormWorld instance has changed
        /// </summary>
        public static System.Action<MonoBehaviour> OnStormInstanceChanged;
        /// <summary>
        /// Action to Invoke when adding a new StormTile
        /// </summary>
        public static System.Action<StormTileData> OnStormTileAdded;
        /// <summary>
        /// Action to Invoke when updating a new StormTile
        /// </summary>
        public static System.Action<StormTileData, Bounds> OnStormTileUpdated;
        /// <summary>
        /// Action to Invoke when removing a new StormTile
        /// </summary>
        public static System.Action<StormTileData> OnStormTileRemoved;
        /// <summary>
        /// Action to Invoke when adding a new FeaturesetTile
        /// </summary>
        public static System.Action<StormTileFeatureset> OnStormTileFeaturesetAdded;
        /// <summary>
        /// Action to Invoke when updating a new FeaturesetTile
        /// </summary>
        public static System.Action<StormTileFeatureset, Bounds> OnStormTileFeaturesetUpdated;
        /// <summary>
        /// Action to Invoke when removing a new FeaturesetTile
        /// </summary>
        public static System.Action<StormTileFeatureset> OnStormTileFeaturesetRemoved;
        /// <summary>
        /// Action to Invoke when removing a new FeaturesetTile
        /// </summary>
        public static System.Action<StormFeatureset, StormFeatureSettings> OnFeatureDisabled;
        /// <summary>
        /// Action to Invoke when removing a new FeaturesetTile
        /// </summary>
        public static System.Action<StormFeatureset, StormFeatureSettings> OnFeatureEnabled;

        /// <summary>
        /// Action invoked when a StormFeatureSettings OnEnable() is called (for updating the list of all known featuresets when one is added in editor)
        /// </summary>
        public static System.Action<StormFeatureSettings> OnFeatureSettingsEnabled;
        /// <summary>
        /// Action invoked when a Impostor OnEnable() is called (for updating the list of all known Impostors when one is added in editor)
        /// </summary>
        public static System.Action<Impostor> OnImpostorEnabled;

        public void CopyFrom(StormWorldSettings settings)
        {
            m_loadDistance = settings.m_loadDistance;
            m_unloadDistance = settings.m_unloadDistance;
            m_renderCellSize = settings.m_renderCellSize;
            m_heightResolution = settings.m_heightResolution;

            m_grassDistance = settings.m_grassDistance;
            m_detailDistance = settings.m_detailDistance;
            m_plantDistance = settings.m_plantDistance;
            m_treeDistance = settings.m_treeDistance;
            m_smallObjDistance = settings.m_smallObjDistance;
            m_mediumObjDistance = settings.m_mediumObjDistance;
            m_largeObjDistance = settings.m_largeObjDistance;
            m_colliderDistance = settings.m_colliderDistance;
            m_grassDensity = settings.m_grassDensity;
            m_detailDensity = settings.m_detailDensity;
            m_plantDensity = settings.m_plantDensity;
            m_treeDensity = settings.m_treeDensity;
            m_smallObjDensity = settings.m_smallObjDensity;
            m_mediumObjDensity = settings.m_mediumObjDensity;
            m_largeObjDensity = settings.m_largeObjDensity;
            m_grassDensityFadeMult = settings.m_grassDensityFadeMult;
            m_detailDensityFadeMult = settings.m_detailDensityFadeMult;
            m_plantDensityFadeMult = settings.m_plantDensityFadeMult;
            m_treeDensityFadeMult = settings.m_treeDensityFadeMult;
            m_smallObjDensityFadeMult = settings.m_smallObjDensityFadeMult;
            m_mediumObjDensityFadeMult = settings.m_mediumObjDensityFadeMult;
            m_largeObjDensityFadeMult = settings.m_largeObjDensityFadeMult;
            m_globalLODBias = settings.m_globalLODBias;
            m_grassLODBias = settings.m_grassLODBias;
            m_detailLODBias = settings.m_detailLODBias;
            m_plantLODBias = settings.m_plantLODBias;
            m_treeLODBias = settings.m_treeLODBias;
            m_smallObjLODBias = settings.m_smallObjLODBias;
            m_mediumObjLODBias = settings.m_mediumObjLODBias;
            m_largeObjLODBias = settings.m_largeObjLODBias;
            m_grassLODMixMult = settings.m_grassLODMixMult;
            m_detailLODMixMult = settings.m_detailLODMixMult;
            m_plantLODMixMult = settings.m_plantLODMixMult;
            m_treeLODMixMult = settings.m_treeLODMixMult;
            m_smallObjLODMixMult = settings.m_smallObjLODMixMult;
            m_mediumObjLODMixMult = settings.m_mediumObjLODMixMult;
            m_largeObjLODMixMult = settings.m_largeObjLODMixMult;
            m_grassScaleoutMult = settings.m_grassScaleoutMult;
            m_detailScaleoutMult = settings.m_detailScaleoutMult;
            m_plantScaleoutMult = settings.m_plantScaleoutMult;
            m_treeScaleoutMult = settings.m_treeScaleoutMult;
            m_smallObjScaleoutMult = settings.m_smallObjScaleoutMult;
            m_mediumObjScaleoutMult = settings.m_mediumObjScaleoutMult;
            m_largeObjScaleoutMult = settings.m_largeObjScaleoutMult;

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
            m_shadowExtensionMult = settings.m_shadowExtensionMult;
            m_splitShadowRendering = settings.m_splitShadowRendering;
            m_useJobs = settings.m_useJobs;
            m_useNewShadowCalc = settings.m_useNewShadowCalc;

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