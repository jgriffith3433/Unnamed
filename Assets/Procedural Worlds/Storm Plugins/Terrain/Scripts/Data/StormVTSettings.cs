using System;
using Unity.Mathematics;
using UnityEngine;

using static ProceduralWorlds.Storm.Terrain.StormVTEnums;

namespace ProceduralWorlds.Storm.Terrain
{
    [CreateAssetMenu(fileName = "StormVTSettings", menuName = "Procedural Worlds/Storm/Terrain/StormVTSettings",
        order = 0)]
    [Serializable]
    public class StormVTSettings : ScriptableObject
    {

        #region Terrain Mode
        [Header("Storm Terrain Mode")]
        /// <summary>
        /// The target platform. Used for qualtiy settings
        /// </summary>
        public StormVTEnums.StormVTPlatform m_targetPlatform;

        /// <summary>
        /// Whether or not the Storm Terrain Manager component is enabled or disabled
        /// </summary>
        public bool m_stormTerrainActive = true;

        /// <summary>
        /// Whether or not to unload all Gaia Terrain Scenes on Play. Used for Clipmap Mode
        /// </summary>
        public bool m_disableGaiaTerrainLoadedScenes = false;

        /// <summary>
        /// Toggle to keep track of whether the clipmap is drawing or not
        /// </summary>
        public bool m_drawClipmap = true;

        /// <summary>
        /// Toggle to remove terrain textures referenced in the Unity Terrain Layers
        /// </summary>
        public bool m_stripUnusedTexturesOnBuild = false;

        /// <summary>
        /// Current mode the system is in. Unity Terrain or Clipmap mode
        /// </summary>
        public StormVTEnums.StormVTMode m_stormVTMode = StormVTMode.UnityTerrain;

        /// <summary>
        /// Reference to the scene's terrain material (used for Unity Terrain Mode)
        /// </summary>
        public Material m_stormTerrainMaterial;

        /// <summary>
        /// Reference to the scene's Clipmap material (used for Clipmap Mode)
        /// </summary>
        public Material m_stormTerrainClipmapMaterial;
        #endregion

        #region Texture Settings

        [Header("Texture Cache Settings")]
        /// <summary>
        /// Resolution in pixels of the page tile size
        /// </summary>
        public StormVTEnums.TileSize m_tileSize = StormVTEnums.TileSize._256;

        /// <summary>
        /// Number of tiles that can fix across physical texture (Columns)
        /// </summary>
        public StormVTEnums.TileCount m_tileWidth = StormVTEnums.TileCount._32;

        /// <summary>
        /// Number of tiles that can fix down the physical texture (Rows)
        /// </summary>
        public StormVTEnums.TileCount m_tileHeight = StormVTEnums.TileCount._32;

        /// <summary>
        /// Calculated width of the physical texture (TileSize + Padding) * Tile Width
        /// </summary>
        public int m_physicalTextureSizeX = 8448;

        /// <summary>
        /// Calculated height of the physical texture (TileSize + Padding) * Tile Height
        /// </summary>
        public int m_physicalTextureSizeY = 8448;

        /// <summary>
        /// Calculated capacity that the physical texture can hold of page tiles (Tile Width * Tile Height)
        /// </summary>
        public int m_tileCapacity  = 256;

        /// <summary>
        /// Amount in pixels of the padding to add to each side of a page tile
        /// </summary>
        public StormVTEnums.Padding m_padding = StormVTEnums.Padding._4;

        /// <summary>
        /// Calculated resolution of the page tile when padding is factored in
        /// </summary>
        public int m_tileSizedPadded = 264;

        /// <summary>
        /// Anisotrophic filtering qualtiy to apply the physical texture
        /// </summary>
        public StormVTEnums.AnisoLevel m_anisoLevel = StormVTEnums.AnisoLevel._8x;

        /// <summary>
        /// Whether to use coroutines to process pending requests.
        /// If disabled, the system uses the Update() method to process pending requests
        /// </summary>
        public bool m_tileGenerationCoroutine = true;

        /// <summary>
        /// Number of tiles to create per frame
        /// Higher numbers will result in less performance
        /// Lower numbers will take more time to complete a view
        /// </summary>
        public int m_maxRequestsPerFrame = 20;

        /// <summary>
        /// Separate resolution used on terrain page tiles, for performance purposes
        /// </summary>
        public StormVTEnums.TileSize m_terrainTileSize = StormVTEnums.TileSize._64;

        /// <summary>
        /// Calculated resolution of the terrain page tile when padding is factored in
        /// </summary>
        public int m_terrainTileSizePadded = 72;

        /// <summary>
        /// Calculated width of the physical texture (TerrainTileSize + Padding) * Tile Width
        /// </summary>
        public int m_physicalTerrainTextureSizeX = 8448;

        /// <summary>
        /// Calculated height of the physical texture (TerrainTileSize + Padding) * Tile Height
        /// </summary>
        public int m_physicalTerrainTextureSizeY = 8448;

        #endregion

        #region Material Settings
        [Header("Material Settings")]
        /// <summary>
        /// Debug toggle to swap between Unity Terraina and Storm Terrain. For use only with Unity Terrain Mode
        /// </summary>
        public bool m_toggleTerrain = false;

        /// <summary>
        /// Debug toggle to swap between showing page tile square borders to show how the terrain is composited
        /// </summary>
        public bool m_debugTiles = false;

        /// <summary>
        /// Toggle to enable trilinear filtering on the physical textures. 
        /// Adds slightly more memory due because trilinear needs 2 mip levels to work
        /// </summary>
        public bool m_allowTrilinear = true;

        /// <summary>
        /// Toggle to enable height blending on all terrain layers
        /// </summary>
        public bool m_heightBlend = true;

        /// <summary>
        /// The contrast of height blending between layers
        /// </summary>
        [Range(0.001f, 0.99f)]
        public float m_heightBlendFactor = 0.9f;

        /// <summary>
        /// The feedback page tile mip bias. 
        /// Higher values produce a stronger bias for lower resolution tiles
        /// Lower values produce a stronger bias for higher resolution tiles
        /// </summary>
        [Range(0.0f, 10.0f)]
        public float m_mipBias = 7f;

        /// <summary>
        /// Mip bias override to use when moving quickly across the world. 
        /// Should be higher in value than the m_mipBias, as we don't need to produce high quality tiles when moving quickly.
        /// </summary>
        [Range(0.0f, 10.0f)]
        public float m_mipBiasWhenMoving = 9f;

        /// <summary>
        /// Move speed to determine the blend between mipBias and mipBiasWhenMoving. 
        /// Should be the maximum moving speed you think a player would ever need to accelerate to. 
        /// </summary>
        public float m_maxMoveSpeed = 500f;

        /// <summary>
        /// Minimum mip level (Zoom tile) for the feedback shader to process. 
        /// </summary>
        public int m_minMipLevel = 0;

        /// <summary>
        /// Maximum mip level (Zoom tile) for the feedback shader to process.
        /// </summary>
        public int m_maxMipLevel = 16;
        #endregion

        #region Imposter Features
        [Header("Imposter Features")]
        /// <summary>
        /// Toggles drawing the grass color map on the terrain
        /// </summary>
        public bool m_grassMapColor = false;

        /// <summary>
        /// Toggles drawing the grass normal map on the terrain
        /// </summary>
        public bool m_grassMapNormal = false;

        /// <summary>
        /// Toggles drawing the tree color map on the terrain
        /// </summary>
        public bool m_treeMapColor = false;

        /// <summary>
        /// Toggles drawing the tree normal map on the terrain
        /// </summary>
        public bool m_treeMapNormal = false;
        
        /// <summary>
        /// The distance to fade in the grass map
        /// </summary>
        [Range(0, 5000f)]
        public float m_grassBlendDistance = 1000f;

        /// <summary>
        /// The contrast of fade in for the grass map
        /// </summary>
        [Range(0, 1f)]
        public float m_grassBlendSharpness = 0f;

        /// <summary>
        /// The opacity of the grass map on the terrain
        /// </summary>
        [Range(0, 1f)]
        public float m_grassBlendAlpha = 1f;

        /// <summary>
        /// The opacity of the grass normal map on the terrain
        /// </summary>
        [Range(0, 1f)]
        public float m_grassBlendNormalAlpha = 1f;

        /// <summary>
        /// The distance to fade in the tree map
        /// </summary>
        [Range(0, 5000f)]
        public float m_treesBlendDistance = 1000f;

        /// <summary>
        /// The contrast of fade in for the tree map
        /// </summary>
        [Range(0, 1f)]
        public float m_treesBlendSharpness = 0f;

        /// <summary>
        /// The opacity of the tree map on the terrain
        /// </summary>
        [Range(0, 1f)]
        public float m_treesBlendAlpha = 1f;

        /// <summary>
        /// The opacity of the tree normal map on the terrain
        /// </summary>
        [Range(0, 1f)]
        public float m_treesBlendNormalAlpha = 1f;
        #endregion

        #region Feedback Settings
        [Header("Feedback Settings")]
        /// <summary>
        /// Relative resolution of the feedback renderer texture based on screen size
        /// </summary>
        public StormVTEnums.FeedbackResolution m_feedbackResolution = StormVTEnums.FeedbackResolution._Eighth;

        /// <summary>
        /// The relative resolution of the readback renderer based on the feedback renderer's size
        /// </summary>
        public StormVTEnums.FeedbackResolution m_readbackResolution = StormVTEnums.FeedbackResolution._Eighth;

        /// <summary>
        /// The delay between each feedback rendering interval
        /// </summary>
        public float m_readbackDelay = 0.1f;

        /// <summary>
        /// The distance behind the main camera that the feedback renderer should draw at
        /// </summary>
        public float m_feedbackPush = 1;

        /// <summary>
        /// The Field of View for the feedback renderer
        /// </summary>
        [Range(40f, 120f)]
        public float m_feedbackFOV = 60f;
        #endregion

        #region Misc Settings
        /// <summary>
        /// Detail Normal Texture
        /// </summary>
        [Header("Detail Normals")]
        public Texture m_detailNormalTexture = null;

        /// <summary>
        /// Macro Variation Texture
        /// </summary>
        [Header("Macro Variation")]
        public Texture m_macroVariationTexture = null;

        #endregion

        #region Displacement Settings
        /// <summary>
        /// Toggles Displacement on
        /// </summary>
        [Header("Displacement Settings")]
        public bool m_displacement = false;

        /// <summary>
        /// Displacement intensity
        /// </summary>
        [Range(0.0f, 1.0f)]
        public float m_displacementAmount = 0.15f;

        /// <summary>
        /// Displacement range
        /// </summary>
        [Range(0.0f, 100.0f)]
        public float m_displacementDistance = 30f;

        /// <summary>
        /// Tesellation quality
        /// </summary>
        public StormVTEnums.StormVTClipmapDensity m_clipmapDensity = StormVTClipmapDensity._1_Vertex_Per_Meter;
        #endregion


        #region Snow Settings
        [Header("Snow Settings")]
        /// <summary>
        /// Toggles snow on
        /// </summary>
        public bool m_Snow = true;

        /// <summary>
        /// Mode to generate snow
        /// Runtime generates snow after the page tiles have been generated
        /// Tile generates snow as part of the page tile
        /// </summary>
        public StormVTEnums.StormVTFeatureMode m_SnowGenerationMode = StormVTEnums.StormVTFeatureMode._Runtime;

        /// <summary>
        /// World Y position of the snow to start
        /// </summary>
        public float m_SnowHeight = 100f;

        /// <summary>
        /// Fade falloff contrast between the starting SnowHeight location
        /// </summary>
        [Range(0.0f, 1000.0f)]
        public float m_SnowFalloff = 65f;

        /// <summary>
        /// The intensity of the curvature map to influence where snow can appear
        /// </summary>
        [Range(0.0f, 10.0f)]
        public float m_SnowCurvatureInfluence = 0.5f;

        /// <summary>
        /// Color of the snow
        /// </summary>
        public Color m_SnowColor = Color.white;

        /// <summary>
        /// World UV scale of the snow
        /// </summary>
        public float m_SnowUVScale = 10.0f;

        /// <summary>
        /// Snow albedo map
        /// </summary>
        public Texture m_SnowAlbedo;

        /// <summary>
        /// Snow normal map
        /// </summary>
        public Texture m_SnowNormal;

        /// <summary>
        /// Snow mask map
        /// </summary>
        public Texture m_SnowMask;
        #endregion

        #region Zoom Tile Settings
        [Header("Zoom Settings")]
        /// <summary>
        /// Lowest zoom level to process
        /// </summary>
        public int m_lowestZoomLevel = 1;

        /// <summary>
        /// Highest zoom level to process
        /// </summary>
        public int m_highestZoomLevel = 16;

        /// <summary>
        /// Lowest terrain zoom level, used for capping the smallest quad tree node size
        /// </summary>
        public int m_lowestTerrainZoomLevel = 11;

        /// <summary>
        /// Highest terrain zoom level, used for capping the highest quad tree node size
        /// </summary>
        public int m_highestTerrainZoomLevel = 16;

        /// <summary>
        /// Number of zoom levels between m_lowestZoomLevel and m_highestZoomLevel
        /// </summary>
        [HideInInspector]
        public byte m_numZoomLevels = 2;

        /// <summary>
        /// Number of page table array slices needed to represent all zoom levels
        /// </summary>
        [HideInInspector]
        public int m_numPageTables = 2;

        /// <summary>
        /// A bias on the lowest zoom level to render. Higher values produce blurrier results
        /// </summary>
        [Range(0, 23)]
        public int m_minZoomLevel = 0;

        /// <summary>
        /// A bias on the minimum height level to sample for displacement. Higher values smoothes out jaggy edges and compression artifacts
        /// </summary>
        [Range(0, 23)]
        public int m_minHeightLevel = 0;





        /// <summary>
        /// Number of zoom levels between m_lowestTerrainZoomLevel and m_highestTerrainZoomLevel
        /// Used for calculating the max quad tree subdivision levels
        /// </summary>
        [HideInInspector]
        public byte m_numTerrainZoomLevels = 2;

        /// <summary>
        /// Resolution dropdown of the page table
        /// </summary>
        public StormVTEnums.StormVTPageTableResolution m_pageTableSize = StormVTPageTableResolution._64;

        /// <summary>
        /// Resolution of the page table
        /// </summary>
        [HideInInspector]
        public int m_pageTableResolution = 64;

        #endregion

        #region Quad Tree Settings
        [Header("Quad Tree Settings")]
        
        [Range(0.0f, 10.0f)]
        /// <summary>
        /// Threshold to test against magnitude of each node
        /// </summary>
        public float m_quadTreeSubdivideThreshold = 1f;

        /// <summary>
        /// The maximum number of page table slices in the array
        /// </summary>
        public const int MAX_PAGE_TABLES_COUNT = 32;
        #endregion

        #region Performance Settings
        [Header("Performance Settings")]
        /// <summary>
        /// The setting to gather page tiles. 
        /// </summary>
        public StormVTEnums.LoadOption m_tileLoadOption = StormVTEnums.LoadOption.Runtime;

        /// <summary>
        /// The compression format for the physical textures. 
        /// </summary>
        public StormVTTextureSettings.StormCompressionFormat m_colorCompressedFormat =
            StormVTTextureSettings.StormCompressionFormat.RGBA32;

        /// <summary>
        /// Toggles whether this system uses a floating point fix
        /// </summary>
        public bool m_floatingPointFix = false;

        /// <summary>
        /// The threshold (megnitude) to test terrains against the camera to see if we need to remap the position
        /// </summary>
        public float m_floatingPointRange = 1000f;
        #endregion

        #region Collision Settings
        [Header("Collision Settings")]
        /// <summary>
        /// Toggles the ability to generate collision meshes at runtime. Used only with the Clipmap system
        /// </summary>
        public bool m_collisionEnabled = true;

        /// <summary>
        /// Toggles whether to show a debug shader of the generated collision meshes
        /// </summary>
        public bool m_debugColliderMeshes = false;

        /// <summary>
        /// Determines the quality of the collision, based off the m_collisionMeshResolution
        /// </summary>
        public StormVTEnums.StormVTCollisionResolution m_collisionQuality = StormVTCollisionResolution._Half;

        /// <summary>
        /// Number of vertices to use per collision mesh
        /// </summary>
        public int m_collisionMeshResolution = 64;

        /// <summary>
        /// Distance (magnitude) from the camera of where to generate collision meshes
        /// </summary>
        public float m_colliderLoadingRange = 250.0f;
        #endregion

        #region Texture Loading Settings
        [Header("Texture Loading Settings")]

        /// <summary>
        /// The maximum number of tasks executed in parallel
        /// </summary>
        [Range(0, 20)]
        public int m_maxParallelTasks = 20;

        /// <summary>
        /// The byte size each chunk should read
        /// </summary>
        public int m_chunkSize = 1048576;

        /// <summary>
        /// The maximum number of chunks read in parallel
        /// </summary>
        [Range(1, 20)]
        public int m_maxParallelChunks = 20;

        /// <summary>
        /// The distance (magnitude) from the camera to determine which terrains are close by for low resolution terrains
        /// </summary>
        public float m_lowResloadingRange = 5001f;

        /// <summary>
        /// The distance (magnitude) from the camera to determine which terrains are close by for high resolution terrains
        /// </summary>
        public float m_highResloadingRange = 2001f;

        /// <summary>
        /// The maximum number of low resolution terrain textures that can be loaded in at any given time
        /// </summary>
        public int m_lowResPoolCount = 16;

        /// <summary>
        /// The maximum number of higher resolution terrain textures that can be loaded in at any given time
        /// Should be less than the m_lowResPoolCount
        /// </summary>
        public int m_highResPoolCount = 4;
        #endregion

        [Header("VRAM Allocated")]
        /// <summary>
        /// The total amount of Video Memory that this system takes up
        /// </summary>
        public string m_physicalTextureMemory;

        [Header("Texture Array Settings")]
        /// Maxmimum size that the texture arrays are allowed to be.
        /// Used for managing memory
        public StormVTEnums.TextureArrayMaxSize m_maxTextureArraySize = StormVTEnums.TextureArrayMaxSize._2048;

        /// <summary>
        /// Compression format to use for texture arrays
        /// </summary>
        public StormVTEnums.StormCompressionFormat m_textureArrayCompressionFormat = StormVTEnums.StormCompressionFormat.ARGB32;

        #region Event System
        /// <summary>
        /// Event called when any variable in this script is changed
        /// </summary>
        public delegate void UpdateStormVTSettings();
        public static event UpdateStormVTSettings OnUpdateStormVTSettings;

        /// <summary>
        /// event called when any snow variable is changed
        /// </summary>
        public delegate void SnowSettingsUpdated();
        public static event SnowSettingsUpdated OnSnowSettingsUpdated;

        #endregion

        /// <summary>
        /// Returns the number of decimal places that a number contains
        /// </summary>
        /// <param name="number">Number to check how many decimal places it has</param>
        /// <returns></returns>
        public bool NumberDigits(double number)
        {
            // Convert the number to a string
            string numberString = number.ToString();

            // Find the index of the decimal point
            int decimalIndex = numberString.IndexOf('.');

            // Count the characters before the decimal point
            int digitsBeforeDecimal = decimalIndex >= 0 ? decimalIndex : numberString.Length;

            return digitsBeforeDecimal > 3 ? true : false;
        }

        /// <summary>
        /// Recomputes essential data used creating and managing the physical textures
        /// </summary>
        public void RecomputeData()
        {
            //Calculate tile size with padding taken into account
            m_tileSizedPadded = (int)m_tileSize + ((int)m_padding * 2);
            m_terrainTileSizePadded = (int)m_terrainTileSize + ((int)m_padding * 2);

            //Calculate final size of physical texture
            m_physicalTextureSizeX = m_tileSizedPadded * (int)m_tileWidth;
            m_physicalTextureSizeY = m_tileSizedPadded * (int)m_tileHeight;

            m_physicalTerrainTextureSizeX = m_terrainTileSizePadded * (int)m_tileWidth;
            m_physicalTerrainTextureSizeY = m_terrainTileSizePadded * (int)m_tileHeight;

            //Calculate number of zoom levels
            m_numZoomLevels = (byte)(m_highestZoomLevel - m_lowestZoomLevel + 1);
            m_numTerrainZoomLevels = (byte)(m_highestTerrainZoomLevel - m_lowestTerrainZoomLevel + 1);

            //Calculate number of needed page tables
            m_numPageTables = math.abs(m_numZoomLevels - 5) + 1;

            m_numPageTables = math.min(m_numPageTables, MAX_PAGE_TABLES_COUNT);
            
            //Calculate tile capacity that fits into physical texture
            m_tileCapacity = (int)m_tileWidth * (int)m_tileHeight;

            //Height Format
            int physicalTextureBits = 32;

            //Terrain Holes
            physicalTextureBits += 8;
            physicalTextureBits += 32;
            physicalTextureBits += 64;

            double physicalTextureMemoryBytes = ((double)physicalTextureBits * ((double)m_physicalTextureSizeX) *
                                                 (double)m_physicalTextureSizeY) / 8e+6;

            string physicalTextureMemorySuffix = "MB";
            if (NumberDigits(physicalTextureMemoryBytes))
            {
                physicalTextureMemoryBytes /= 1000f;
                physicalTextureMemorySuffix = "GB";
            }

            m_physicalTextureMemory = physicalTextureMemoryBytes.ToString("F2") + physicalTextureMemorySuffix;
        }

        /// <summary>
        /// Computes essential constant data used for sampling the physical texture acorss scripts and shaders
        /// </summary>
        public void ComputeConstants()
        {

            m_pageTableResolution = (int)m_pageTableSize;

            float pageWidth = m_tileSizedPadded;
            float pageBorder = (int)m_padding;
            float2 physPagesWide = new float2((int)m_tileWidth, (int)m_tileHeight);

            float2 pageFracScale = (pageWidth - 2 * pageBorder) / pageWidth / physPagesWide;
            float2 borderOffset = pageBorder / pageWidth / physPagesWide;
            float4 texScale = new float4
            (
                (float)(255.0 / physPagesWide.x),
                (float)(255.0 / physPagesWide.y),
                (float)(255.0),
                (float)(255.0)
            );

            float4 texBias = new float4(borderOffset.x, borderOffset.y, 0, 0);

            //Set global shader variables
            Shader.SetGlobalVector(StormVTShaderIDs.m_stormVT_PageFracScaleID, new Vector4(pageFracScale.x, pageFracScale.y, m_allowTrilinear ? m_mipBias : 0, 0));
            Shader.SetGlobalVector(StormVTShaderIDs.m_stormVT_TexBiasID, texBias);
            Shader.SetGlobalVector(StormVTShaderIDs.m_stormVT_TexScaleID, texScale);


            float terrainPageWidth = m_terrainTileSizePadded;
            float2 terrainPageFracScale = (terrainPageWidth - 2 * pageBorder) / terrainPageWidth / physPagesWide;
            float2 terrainBorderOffset = pageBorder / terrainPageWidth / physPagesWide;
            float4 terrainTexBias = new float4(terrainBorderOffset.x, terrainBorderOffset.y, 0, 0);

            Shader.SetGlobalVector(StormVTShaderIDs.m_stormVT_Terrain_PageFracScaleID, new Vector4(pageFracScale.x, pageFracScale.y, m_allowTrilinear ? m_mipBias : 0, 0));
            Shader.SetGlobalVector(StormVTShaderIDs.m_stormVT_Terrain_TexBiasID, texBias);
            Shader.SetGlobalVector(StormVTShaderIDs.m_stormVT_Terrain_TexScaleID, texScale);

            Shader.SetGlobalInteger(StormVTShaderIDs.m_stormVT_DebugTiles, m_debugTiles ? 1 : 0);
            Shader.SetGlobalVector(StormVTShaderIDs.m_stormVT_PhysicalTextureTileData, new Vector4((int)m_tileSize, m_tileSizedPadded, (int)m_padding, 0));
            Shader.SetGlobalVector(StormVTShaderIDs.m_stormVT_PhysicalTextureGridSize, new Vector4((int)m_tileWidth, (int)m_tileHeight, 0, 0));
            Shader.SetGlobalVector(StormVTShaderIDs.m_stormVT_TileMipData, new Vector4(m_minMipLevel, m_maxMipLevel, m_mipBias, ((int)m_tileSize * (int)m_tileWidth)));
            Vector4 heightVector = Shader.GetGlobalVector(StormVTShaderIDs.m_stormVT_TerrainHeightScale);
            heightVector.z = m_displacementAmount;
            heightVector.w = m_displacementDistance;
            Shader.SetGlobalVector(StormVTShaderIDs.m_stormVT_TerrainHeightScale, heightVector);
            Shader.SetGlobalVector(StormVTShaderIDs.m_stormVT_HeightBlendData, new Vector4(m_heightBlend ? 1 : 0, 1.0f - m_heightBlendFactor, 0,0));
            Shader.SetGlobalInt(StormVTShaderIDs.m_stormVT_Mode, (int)m_tileLoadOption);

            Vector4 pageTableData = Shader.GetGlobalVector(StormVTShaderIDs.m_stormVT_ArrayDataID);
            pageTableData.x = m_minZoomLevel;
            pageTableData.y = m_minHeightLevel;
            Shader.SetGlobalVector(StormVTShaderIDs.m_stormVT_ArrayDataID, pageTableData);

            Shader.SetGlobalVector(StormVTShaderIDs.m_stormVT_GrassMapData, new Vector4(m_grassBlendDistance, m_grassBlendSharpness, m_grassBlendAlpha, m_grassBlendNormalAlpha));
            Shader.SetGlobalVector(StormVTShaderIDs.m_stormVT_TreeMapData, new Vector4(m_treesBlendDistance, m_treesBlendSharpness, m_treesBlendAlpha, m_treesBlendNormalAlpha));
            Shader.SetGlobalVector(StormVTShaderIDs.m_stormVT_ImposterMapData, new Vector4(
                m_grassMapColor == true ? 1 : 0,
                m_grassMapNormal == true ? 1 : 0,
                m_treeMapColor == true ? 1 : 0,
                m_treeMapNormal == true ? 1 : 0
            ));


            SetGlobalSnowVariables();
        }

        /// <summary>
        /// Updates the global vector that contains the mip bias data for the feedback renderer. 
        /// Used when moving across the terrain quickly
        /// </summary>
        public void UpdateMipBias()
        {
            Shader.SetGlobalVector(StormVTShaderIDs.m_stormVT_TileMipData, new Vector4(m_minMipLevel, m_maxMipLevel, m_mipBias, ((int)m_tileSize * (int)m_tileWidth)));
        }

        /// <summary>
        /// Called at start to setup essential data
        /// </summary>
        public void Start()
        {

            ComputeConstants();
            ForceSettingsUpdate();
            if (OnUpdateStormVTSettings != null)
                OnUpdateStormVTSettings();
            Shader.SetGlobalTexture(StormVTShaderIDs.m_stormVT_SnowPass_SnowAlbedo, m_SnowAlbedo);
            Shader.SetGlobalTexture(StormVTShaderIDs.m_stormVT_SnowPass_SnowNormal, m_SnowNormal);
            Shader.SetGlobalTexture(StormVTShaderIDs.m_stormVT_SnowPass_SnowMask, m_SnowMask);
        }

        /// <summary>
        /// Update the data once the any variable has changed
        /// </summary>
        public void OnValidate()
        {
            if (OnUpdateStormVTSettings != null)
                OnUpdateStormVTSettings();

            SetSnowVariables(m_stormTerrainMaterial);


        }

        /// <summary>
        /// Tells the scriptable object to call the settings updated event
        /// </summary>
        public void ForceSettingsUpdate()
        {
            if (OnUpdateStormVTSettings != null)
                OnUpdateStormVTSettings();
        }


        /// <summary>
        /// Sets the snow variables of the given material to the values stored in this script
        /// </summary>
        /// <param name="mat"></param>
        public void SetSnowVariables(Material mat)
        {
            if(mat != null)
            {
                mat.SetFloat(StormVTShaderIDs.m_stormVT_SnowPass_SnowHeight, m_SnowHeight);
                mat.SetFloat(StormVTShaderIDs.m_stormVT_SnowPass_SnowFalloff, m_SnowFalloff);
                mat.SetFloat(StormVTShaderIDs.m_stormVT_SnowPass_SnowCurvatureInfluence, m_SnowCurvatureInfluence);
                mat.SetVector(StormVTShaderIDs.m_stormVT_SnowPass_SnowColor, m_SnowColor);
                mat.SetFloat(StormVTShaderIDs.m_stormVT_SnowPass_SnowUVScale, m_SnowUVScale);
                mat.SetTexture(StormVTShaderIDs.m_stormVT_SnowPass_SnowAlbedo, m_SnowAlbedo);
                mat.SetTexture(StormVTShaderIDs.m_stormVT_SnowPass_SnowNormal, m_SnowNormal);
                mat.SetTexture(StormVTShaderIDs.m_stormVT_SnowPass_SnowMask, m_SnowMask);

                mat.SetTexture(StormVTShaderIDs.m_stormVT_SnowPass_SnowAlbedo, m_SnowAlbedo);
                mat.SetTexture(StormVTShaderIDs.m_stormVT_SnowPass_SnowNormal, m_SnowNormal);
                mat.SetTexture(StormVTShaderIDs.m_stormVT_SnowPass_SnowMask, m_SnowMask);
            }
           

            Shader.SetGlobalTexture(StormVTShaderIDs.m_stormVT_SnowPass_SnowAlbedo, m_SnowAlbedo);
            Shader.SetGlobalTexture(StormVTShaderIDs.m_stormVT_SnowPass_SnowNormal, m_SnowNormal);
            Shader.SetGlobalTexture(StormVTShaderIDs.m_stormVT_SnowPass_SnowMask, m_SnowMask);

           

            if (StormVTSettings.OnSnowSettingsUpdated != null)
                StormVTSettings.OnSnowSettingsUpdated();
        }

        /// <summary>
        /// Sets the global snow variables to be the values stored in this script
        /// </summary>
        public void SetGlobalSnowVariables()
        {
            Shader.SetGlobalFloat(StormVTShaderIDs.m_stormVT_SnowPass_SnowHeight, m_SnowHeight);
            Shader.SetGlobalFloat(StormVTShaderIDs.m_stormVT_SnowPass_SnowFalloff, m_SnowFalloff);
            Shader.SetGlobalFloat(StormVTShaderIDs.m_stormVT_SnowPass_SnowCurvatureInfluence, m_SnowCurvatureInfluence);
            Shader.SetGlobalVector(StormVTShaderIDs.m_stormVT_SnowPass_SnowColor, m_SnowColor);
            Shader.SetGlobalFloat(StormVTShaderIDs.m_stormVT_SnowPass_SnowUVScale, m_SnowUVScale);
            Shader.SetGlobalTexture(StormVTShaderIDs.m_stormVT_SnowPass_SnowAlbedo, m_SnowAlbedo);
            Shader.SetGlobalTexture(StormVTShaderIDs.m_stormVT_SnowPass_SnowNormal, m_SnowNormal);
            Shader.SetGlobalTexture(StormVTShaderIDs.m_stormVT_SnowPass_SnowMask, m_SnowMask);
        }

        /// <summary>
        /// Assigns the textures references of this scriptable obejct to be from the Storm Terrain Defaults script
        /// </summary>
        public void UpdateTextures()
        {

#if UNITY_EDITOR
            if (m_SnowAlbedo == null)
            {
                m_SnowAlbedo = StormVTDefaults.Instance.snowDiffuse;
            }
            if (m_SnowNormal == null)
            {
                m_SnowNormal = StormVTDefaults.Instance.snowNormal;
            }
            if (m_SnowMask == null)
            {
                m_SnowMask = StormVTDefaults.Instance.snowMask;
            }
            if (m_macroVariationTexture == null)
            {
                m_macroVariationTexture = StormVTDefaults.Instance.macroVariation;
            }
            if (m_detailNormalTexture == null)
            {
                m_detailNormalTexture = StormVTDefaults.Instance.detailNormal;
            }
#endif
        }

    }
}
