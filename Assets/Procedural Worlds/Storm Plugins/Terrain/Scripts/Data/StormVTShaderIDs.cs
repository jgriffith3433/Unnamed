using UnityEngine;

namespace ProceduralWorlds.Storm.Terrain
{
    public static class StormVTShaderIDs
    {
        // Keywords
        public static readonly int m_stormVT_ArrayDataID;
        public static readonly int m_stormVT_PositionDataID;
        public static readonly int m_stormVT_PageTableDataBufferID;
        public static readonly int m_stormVT_PageTableDataArrayID;
        public static readonly int m_stormVT_PhysicalTextureGridSize;
        public static readonly int m_stormVT_PhysicalTextureTileData;
        public static readonly int m_stormVT_ZoomLevelData;
        public static readonly int m_stormVT_PageTableTextureArray;
        public static readonly int m_stormVT_SnappedCameraPosition;
        public static readonly int m_stormVT_PhysicalTexturesInitialized;
        public static readonly int m_stormVT_DisplacementAmount;
        public static readonly int m_stormVT_DebugTiles;
        public static readonly int m_stormVT_Mode;


        public static readonly int m_stormVT_TileMipData;
        public static readonly int m_stormVT_TileFeedbackPositionData;
        public static readonly int m_stormVT_HeightBlendData;

        public static readonly int m_stormVT_TerrainHeightScale;
        public static readonly int m_stormVT_TotalTerrainBounds;
        public static readonly int m_stormVT_WorldOffset;

        public static readonly int m_stormVT_TexScaleID;
        public static readonly int m_stormVT_TexBiasID;
        public static readonly int m_stormVT_PageFracScaleID;

        public static readonly int m_stormVT_Terrain_TexScaleID;
        public static readonly int m_stormVT_Terrain_TexBiasID;
        public static readonly int m_stormVT_Terrain_PageFracScaleID;


        public static readonly int m_stormVT_UnityTerrain;
        public static readonly string m_stormVT_UnityTerrainOn = "_UNITY_TERRAIN_ON";
        public static readonly int m_stormVT_ClipmapTerrain;
        public static readonly string m_stormVT_ClipmapTerrainOn = "_CLIPMAP_ON";
        public static readonly int m_stormVT_DisplacementTerrain;
        public static readonly string m_stormVT_DisplacementTerrainOn = "_DISPLACEMENT_ON";
        public static readonly int m_stormVT_SnowTerrain;
        public static readonly string m_stormVT_SnowTerrainOn = "_SNOW_ON";
        public static readonly int m_stormVT_DetailNormal;
        public static readonly string m_stormVT_DetailNormalOn = "_DETAIL_NORMAL_ON";
        public static readonly int m_stormVT_MacroVariation;
        public static readonly string m_stormVT_MacroVariationOn = "_MACRO_VARIATION_ON";
        public static readonly int m_stormVT_DeviceTargetKeyword;
        public static readonly string m_stormVT_DeviceTargetKeywordOn = "_STORMVT_LOW_END_ON";

        public static readonly int m_stormVT_ClipmapTerrainState;



        public static readonly int m_stormVT_PhysicalTextureHeight;
        public static readonly int m_stormVT_PhysicalTextureNormal;
        public static readonly int m_stormVT_PhysicalTextureSource0;
        public static readonly int m_stormVT_PhysicalTextureSource1;
        public static readonly int m_stormVT_PhysicalTextureSatellite;
        public static readonly int m_stormVT_PhysicalTextureHoles;

        public static readonly int m_stormVT_PhysicalTextureDistanceMapColor;
        public static readonly int m_stormVT_PhysicalTextureDistanceMapNormal;
        public static readonly int m_stormVT_PhysicalTextureGrassColor;
        public static readonly int m_stormVT_PhysicalTextureTreeColor;

        public static readonly int m_stormVT_DistanceMapData;
        public static readonly int m_stormVT_GrassMapData;
        public static readonly int m_stormVT_TreeMapData;
        public static readonly int m_stormVT_ImposterMapData;

        public static readonly int[,] m_stormVT_TileTextureNames = new int[3,3];
        public static readonly int[,] m_stormVT_TileTextureNamesEnabled = new int[3,3];

        //Tilecopy material
        public static readonly int m_stormVT_TileCopy_BaseTexture;
        public static readonly int m_stormVT_TileCopy_TileTexture;
        public static readonly int m_stormVT_TileCopy_BaseWorldPos;
        public static readonly int m_stormVT_TileCopy_BaseWorldSize;
        public static readonly int m_stormVT_TileCopy_TileWorldPos;
        public static readonly int m_stormVT_TileCopy_TileWorldSize;
        public static readonly int m_stormVT_TileCopy_TerrainHeightmapRecipSize;
        public static readonly int m_stormVT_TileCopy_TerrainHeightScale;
        
        //Neighbour padding material
        public static readonly int m_stormVT_Neighbour_Padding;
        public static readonly int m_stormVT_Neighbour_TexelSize;
        
        public static readonly int m_stormVT_Neighbour_TopLeft;
        public static readonly int m_stormVT_Neighbour_TopLeftEnabled;
        
        public static readonly int m_stormVT_Neighbour_TopCenter;
        public static readonly int m_stormVT_Neighbour_TopCenterEnabled;
        
        public static readonly int m_stormVT_Neighbour_TopRight;
        public static readonly int m_stormVT_Neighbour_TopRightEnabled;
        
        public static readonly int m_stormVT_Neighbour_MiddleLeft;
        public static readonly int m_stormVT_Neighbour_MiddleLeftEnabled;
        
        public static readonly int m_stormVT_Neighbour_MiddleCenter;
        public static readonly int m_stormVT_Neighbour_MiddleCenterEnabled;
        
        public static readonly int m_stormVT_Neighbour_MiddleRight;
        public static readonly int m_stormVT_Neighbour_MiddleRightEnabled;
        
        public static readonly int m_stormVT_Neighbour_BottomLeft;
        public static readonly int m_stormVT_Neighbour_BottomLeftEnabled;
        
        public static readonly int m_stormVT_Neighbour_BottomCenter;
        public static readonly int m_stormVT_Neighbour_BottomCenterEnabled;
        
        public static readonly int m_stormVT_Neighbour_BottomRight;
        public static readonly int m_stormVT_Neighbour_BottomRightEnabled;
        
        
        
        //FineTileCopy
        public static readonly int m_stormVT_FineTileCopy_NewZoomTilePos;
        public static readonly int m_stormVT_FineTileCopy_NewZoomTileSize;
        public static readonly int m_stormVT_FineTileCopy_HigherTilePos;
        public static readonly int m_stormVT_FineTileCopy_HigherTileSize;

        public static readonly int m_stormVT_AlphaMerge_Alpha;
        
        
        //Terrain tile generation
        public static readonly int m_stormVT_TerrainTileGen_TilePosition;
        public static readonly int m_stormVT_TerrainTileGen_TileSize;
        public static readonly int m_stormVT_TerrainTileGen_BasePass;
        public static readonly int m_stormVT_TerrainTileGen_TerrainPosition;
        public static readonly int m_stormVT_TerrainTileGen_WorldNormalMap;
        public static readonly int m_stormVT_TerrainTileGen_TerrainSize;
        public static readonly int m_stormVT_TerrainTileGen_RecipSize;
        public static readonly int m_stormVT_TerrainTileGen_ControlMap;
        public static readonly int m_stormVT_TerrainTileGen_NormalMap;
        public static readonly int m_stormVT_TerrainTileGen_HeightMap;
        public static readonly int m_stormVT_TerrainTileGen_terrainLayersBuffer;
        public static readonly int m_stormVT_TerrainTileGen_TileTexture;
        public static readonly int m_stormVT_TerrainTileGen_PassIndex;
        public static readonly int m_stormVT_TerrainTileGen_TangentNormalMap;
        public static readonly int m_stormVT_TerrainTileGen_First4Layers;
        public static readonly int m_stormVT_TerrainTileGen_HeightBlend;
        public static readonly int m_stormVT_TerrainTileGen_HeightBlendFactor;
        public static readonly int m_stomrVT_TerrainTileGen_TileMatrix;
        public static readonly int m_stormVT_TerrainTileGen_Stochastic;
        public static readonly int m_stormVT_TerrainTileGen_LayerAmount;
        public static readonly int m_stormVT_TerrainTileGen_TerrainHeightmapRecipSize;
        public static readonly int m_stormVT_TerrainTileGen_TerrainTexture;
        public static readonly int m_stormVT_TerrainTileGen_MacroVariationMap;
        public static readonly int m_stormVT_TerrainTileGen_DetailNormalMap;
        public static readonly int m_stormVT_TerrainTileGen_DetailNormalScale;

        public static readonly int m_stormVT_TerrainTileGen_AlbedoArray;
        public static readonly int m_stormVT_TerrainTileGen_NormalArray;


        public static readonly int[] m_stormVT_TerrainTileGen_DiffuseTextures = new int[4];
        public static readonly int[] m_stormVT_TerrainTileGen_NormalTextures = new int[4];
        public static readonly int[] m_stormVT_TerrainTileGen_MaskTextures = new int[4];

        public static readonly int[] m_stormVT_TerrainTileGen_ControlMaps = new int[4];

        //Compute Shader Names
        public static readonly int m_stormVT_PageTableUpdate_PageTableCount;
        public static readonly int m_stormVT_PageTableUpdate_NodeCount;
        public static readonly int m_stormVT_PageTableUpdate_SliceIndex;
        public static readonly int m_stormVT_PageTableUpdate_NodeDataBuffer;
        public static readonly int m_stormVT_PageTableUpdate_PageTableDataBuffer;

        public static readonly int m_stormVT_CreateWorldNormalMap_Height;
        public static readonly int m_stormVT_CreateWorldNormalMap_Heightmap;
        public static readonly int m_stormVT_CreateWorldNormalMap_TexelSize;
        public static readonly int m_stormVT_CreateWorldNormalMap_TileSize;
        public static readonly int m_stormVT_CreateWorldNormalMap_TileCenter;
        public static readonly int m_stormVT_CreateWorldNormalMap_WorldDeriv;

        public static readonly int m_stormVT_TileCaptureBlend_TileSource0;
        public static readonly int m_stormVT_TileCaptureBlend_TileSource1;
        public static readonly int m_stormVT_TileCaptureBlend_CaptureSource0;
        public static readonly int m_stormVT_TileCaptureBlend_CaptureSource1;

        public static readonly int m_stormVT_DecalBlend_Source0;
        public static readonly int m_stormVT_DecalBlend_Source1;
        public static readonly int m_stormVT_DecalBlend_Source2;

        public static readonly int m_stormVT_CreatePageTable_NodeCount;
        public static readonly int m_stormVT_CreatePageTable_NodeBuffer;
        public static readonly int m_stormVT_CreatePageTable_PageTableInfo;



        //Snow capture pass
        public static readonly int m_stormVT_SnowPass_NormalMap;
        public static readonly int m_stormVT_SnowPass_Source0;
        public static readonly int m_stormVT_SnowPass_Source1;
        public static readonly int m_stormVT_SnowPass_HeightMap;

        public static readonly int m_stormVT_SnowPass_SnowWorldAlignedRemap;
        public static readonly int m_stormVT_SnowPass_SnowCurvatureInfluence;
        public static readonly int m_stormVT_SnowPass_SnowCurvatureIntensity;
        public static readonly int m_stormVT_SnowPass_SnowMaskMinMax;
        public static readonly int m_stormVT_SnowPass_SnowColor;
        public static readonly int m_stormVT_SnowPass_SnowHeight;
        public static readonly int m_stormVT_SnowPass_SnowFalloff;
        public static readonly int m_stormVT_SnowPass_HeightScale;
        public static readonly int m_stormVT_SnowPass_HeightOffset;
        public static readonly int m_stormVT_SnowPass_SnowUVScale;
        public static readonly int m_stormVT_SnowPass_SnowAlbedo;
        public static readonly int m_stormVT_SnowPass_SnowNormal;
        public static readonly int m_stormVT_SnowPass_SnowMask;

        //Terrain Height - Normal - Holes
        public static readonly int m_stormVT_TileCopy_TerrainHeightMap;
        public static readonly int m_stormVT_TileCopy_TerrainWorldNormalMap;
        public static readonly int m_stormVT_TileCopy_TerrainHolesMap;
        public static readonly int m_stormVT_TileCopy;
        public static readonly int m_stormVT_TileCopy_TileHeightMap;
        public static readonly int m_stormVT_TileCopy_TileWorldNormalMap;
        public static readonly int m_stormVT_TileCopy_TileHolesMap;
        public static readonly int m_stormVT_TileCopy_TerrainDistanceMapColor;
        public static readonly int m_stormVT_TileCopy_TerrainDistanceMapNormal;
        public static readonly int m_stormVT_TileCopy_TerrainGrassMapColor;
        public static readonly int m_stormVT_TileCopy_TerrainTreeMapColor;
        public static readonly int m_stormVT_TileCopy_TerrainGrassMapNormal;
        public static readonly int m_stormVT_TileCopy_TerrainTreeMapNormal;


        public static readonly int m_stormVT_TileCopy_TileDistanceMapColor;
        public static readonly int m_stormVT_TileCopy_TileDistanceMapNormal;
        public static readonly int m_stormVT_TileCopy_TileGrassMapColor;
        public static readonly int m_stormVT_TileCopy_TileTreeMapColor;

        public static readonly int m_stormVT_TerrainTileGen_TileSource0;
        public static readonly int m_stormVT_TerrainTileGen_TileSource1;

        public static readonly int m_stormVT_TerrainTileGen_PreviousControl;
        public static readonly int m_stormVT_TerrainTileGen_PreviousMaxHeight;
        public static readonly int m_stormVT_TerrainTileGen_PreviousSumHeight;

        public static readonly int m_stormVT_SampleHeightPosition;

        public static readonly int m_stormVT_DecalTileSize;
        public static readonly int m_stormVT_DecalTilePos;

        static StormVTShaderIDs()
        {
            m_stormVT_ArrayDataID = Shader.PropertyToID("_StormVT_PageTableArrayData");
            m_stormVT_PositionDataID = Shader.PropertyToID("_StormVT_PageTablePositionData");
            m_stormVT_PageTableDataBufferID = Shader.PropertyToID("_StormVT_PageTableDataBuffer");
            m_stormVT_PageTableDataArrayID = Shader.PropertyToID("_StormVT_PageTableDataArray");
            m_stormVT_ZoomLevelData = Shader.PropertyToID("_StormVT_ZoomLevelData");
            m_stormVT_PageTableTextureArray = Shader.PropertyToID("_StormVT_PageTableTex2DArray");
            m_stormVT_SnappedCameraPosition = Shader.PropertyToID("_StormVT_SnappedCameraPosition");
            m_stormVT_PhysicalTexturesInitialized = Shader.PropertyToID("StormVT_PhysicalTexturesInitialized");
            m_stormVT_DisplacementAmount = Shader.PropertyToID("_StormVT_DisplacementAmount");
            m_stormVT_DebugTiles = Shader.PropertyToID("_StormVT_DebugTiles");
            m_stormVT_PhysicalTextureGridSize = Shader.PropertyToID("_StormVT_PhysicalTextureGridSize");
            m_stormVT_PhysicalTextureTileData = Shader.PropertyToID("_StormVT_PhysicalTextureTileData");
            m_stormVT_Mode = Shader.PropertyToID("_StormVT_Mode");


            m_stormVT_TileMipData = Shader.PropertyToID("_StormVT_TileMipData");
            m_stormVT_TileFeedbackPositionData = Shader.PropertyToID("_StormVT_TileFeedbackPositionData");
            m_stormVT_HeightBlendData = Shader.PropertyToID("_StormVT_HeightBlendData");

            m_stormVT_TerrainHeightScale = Shader.PropertyToID("_StormVT_TerrainHeightScale");
            m_stormVT_TotalTerrainBounds = Shader.PropertyToID("_StormVT_TotalTerrainBounds");
            m_stormVT_WorldOffset = Shader.PropertyToID("_StormVT_WorldOffset");
            m_stormVT_SampleHeightPosition = Shader.PropertyToID("position");

            m_stormVT_TexScaleID = Shader.PropertyToID("_StormVT_TexScale");
            m_stormVT_TexBiasID = Shader.PropertyToID("_StormVT_TexBias");
            m_stormVT_PageFracScaleID = Shader.PropertyToID("_StormVT_PageFracScale");

            m_stormVT_Terrain_TexScaleID = Shader.PropertyToID("_StormVT_Terrain_TexScale");
            m_stormVT_Terrain_TexBiasID = Shader.PropertyToID("_StormVT_Terrain_TexBias");
            m_stormVT_Terrain_PageFracScaleID = Shader.PropertyToID("_StormVT_Terrain_PageFracScale");

            m_stormVT_UnityTerrain = Shader.PropertyToID("_UNITY_TERRAIN");
            m_stormVT_ClipmapTerrain = Shader.PropertyToID("_CLIPMAP");
            m_stormVT_DisplacementTerrain = Shader.PropertyToID("_DISPLACEMENT");
            m_stormVT_SnowTerrain = Shader.PropertyToID("_SNOW");
            m_stormVT_DeviceTargetKeyword = Shader.PropertyToID("_STORMVT_LOW_END");
            m_stormVT_ClipmapTerrainState = Shader.PropertyToID("_StormVT_ClipmapTerrainState");

            m_stormVT_PhysicalTextureHeight = Shader.PropertyToID("_StormVT_PhysicalTextureHeight");
            m_stormVT_PhysicalTextureNormal= Shader.PropertyToID("_StormVT_PhysicalTextureNormal");
            m_stormVT_PhysicalTextureSource0 = Shader.PropertyToID("_StormVT_PhysicalTextureSource0");
            m_stormVT_PhysicalTextureSource1 = Shader.PropertyToID("_StormVT_PhysicalTextureSource1");
            m_stormVT_PhysicalTextureSatellite = Shader.PropertyToID("_StormVT_PhysicalTextureSatellite");
            m_stormVT_PhysicalTextureHoles = Shader.PropertyToID("_StormVT_PhysicalTextureHoles");
            m_stormVT_PhysicalTextureDistanceMapColor = Shader.PropertyToID("_StormVT_PhysicalTextureDistanceMapColor");
            m_stormVT_PhysicalTextureDistanceMapNormal = Shader.PropertyToID("_StormVT_PhysicalTextureDistanceMapNormal");
            m_stormVT_PhysicalTextureGrassColor = Shader.PropertyToID("_StormVT_PhysicalTextureGrassColor");
            m_stormVT_PhysicalTextureTreeColor = Shader.PropertyToID("_StormVT_PhysicalTextureTreeColor");
            m_stormVT_DistanceMapData = Shader.PropertyToID("_StormVT_DistanceMapData");
            m_stormVT_GrassMapData = Shader.PropertyToID("_StormVT_GrassMapData");
            m_stormVT_TreeMapData = Shader.PropertyToID("_StormVT_TreeMapData");
            m_stormVT_ImposterMapData = Shader.PropertyToID("_StormVT_ImposterMapData");
            
            
            //TileCopy id
            m_stormVT_TileCopy_BaseTexture = Shader.PropertyToID("_Base");
            m_stormVT_TileCopy_TileTexture = Shader.PropertyToID("_Tile");
            m_stormVT_TileCopy_BaseWorldPos = Shader.PropertyToID("_BaseWorldPos");
            m_stormVT_TileCopy_BaseWorldSize = Shader.PropertyToID("_BaseWorldSize");
            m_stormVT_TileCopy_TileWorldPos = Shader.PropertyToID("_TileWorldPos");
            m_stormVT_TileCopy_TileWorldSize = Shader.PropertyToID("_TileWorldSize");
            m_stormVT_TileCopy_TerrainHeightmapRecipSize = Shader.PropertyToID("_TerrainHeightmapRecipSize");
            m_stormVT_TileCopy_TerrainHeightScale = Shader.PropertyToID("_TerrainHeightmapScale");
            
            //Neighbour padding
            m_stormVT_Neighbour_Padding = Shader.PropertyToID("_Padding");
            m_stormVT_Neighbour_TexelSize = Shader.PropertyToID("_TexelSize");
            
            
            m_stormVT_Neighbour_TopLeft = Shader.PropertyToID("_TopLeft");
            m_stormVT_Neighbour_TopLeftEnabled = Shader.PropertyToID("_TopLeftEnabled");
            
            m_stormVT_Neighbour_TopCenter = Shader.PropertyToID("_TopCenter");
            m_stormVT_Neighbour_TopCenterEnabled = Shader.PropertyToID("_TopCenterEnabled");
            
            m_stormVT_Neighbour_TopRight = Shader.PropertyToID("_TopRight");
            m_stormVT_Neighbour_TopRightEnabled = Shader.PropertyToID("_TopRightEnabled");
            
            m_stormVT_Neighbour_MiddleLeft = Shader.PropertyToID("_MiddleLeft");
            m_stormVT_Neighbour_MiddleLeftEnabled = Shader.PropertyToID("_MiddleLeftEnabled");
            
            m_stormVT_Neighbour_MiddleCenter = Shader.PropertyToID("_MiddleCenter");
            m_stormVT_Neighbour_MiddleCenterEnabled = Shader.PropertyToID("_MiddleCenterEnabled");
            
            m_stormVT_Neighbour_MiddleRight = Shader.PropertyToID("_MiddleRight");
            m_stormVT_Neighbour_MiddleRightEnabled = Shader.PropertyToID("_MiddleRightEnabled");
            
            m_stormVT_Neighbour_BottomLeft = Shader.PropertyToID("_BottomLeft");
            m_stormVT_Neighbour_BottomLeftEnabled = Shader.PropertyToID("_BottomLeftEnabled");
            
            m_stormVT_Neighbour_BottomCenter = Shader.PropertyToID("_BottomCenter");
            m_stormVT_Neighbour_BottomCenterEnabled = Shader.PropertyToID("_BottomCenterEnabled");
            
            m_stormVT_Neighbour_BottomRight = Shader.PropertyToID("_BottomRight");
            m_stormVT_Neighbour_BottomRightEnabled = Shader.PropertyToID("_BottomRightEnabled");
            
            //Fine tile copy
            m_stormVT_FineTileCopy_NewZoomTilePos = Shader.PropertyToID("_NewZoomTilePos");
            m_stormVT_FineTileCopy_NewZoomTileSize = Shader.PropertyToID("_NewZoomTileSize");
            m_stormVT_FineTileCopy_HigherTilePos = Shader.PropertyToID("_HigherTilePos");
            m_stormVT_FineTileCopy_HigherTileSize = Shader.PropertyToID("_HigherTileSize");
            
            //Terrain Tile Generation
            m_stormVT_TerrainTileGen_TilePosition = Shader.PropertyToID("_TilePosition");
            m_stormVT_TerrainTileGen_TileSize = Shader.PropertyToID("_TileSize");
            m_stormVT_TerrainTileGen_BasePass = Shader.PropertyToID("_BasePass");
            m_stormVT_TerrainTileGen_TerrainPosition = Shader.PropertyToID("_TerrainPosition");
            m_stormVT_TerrainTileGen_WorldNormalMap = Shader.PropertyToID("_WorldNormal");
            m_stormVT_TerrainTileGen_TangentNormalMap = Shader.PropertyToID("_TangentNormal");
            m_stormVT_TerrainTileGen_TerrainSize = Shader.PropertyToID("_TerrainSize");
            m_stormVT_TerrainTileGen_RecipSize = Shader.PropertyToID("_RemapSize");
            m_stormVT_TerrainTileGen_ControlMap = Shader.PropertyToID("_ControlMap");
            m_stormVT_TerrainTileGen_NormalMap = Shader.PropertyToID("_NormalMap");
            m_stormVT_TerrainTileGen_HeightMap = Shader.PropertyToID("_HeightMap");
            m_stormVT_TerrainTileGen_terrainLayersBuffer = Shader.PropertyToID("terrainLayers");
            m_stormVT_TerrainTileGen_TileTexture = Shader.PropertyToID("_TileTexture");
            m_stormVT_TerrainTileGen_PassIndex = Shader.PropertyToID("_PassIndex");
            m_stormVT_TerrainTileGen_First4Layers = Shader.PropertyToID("_First4Layers");
            m_stormVT_TerrainTileGen_HeightBlend = Shader.PropertyToID("_HeightBlend");
            m_stormVT_TerrainTileGen_HeightBlendFactor = Shader.PropertyToID("_HeightBlendFactor");
            m_stomrVT_TerrainTileGen_TileMatrix = Shader.PropertyToID("_TileTRSMatrix");
            m_stormVT_TerrainTileGen_Stochastic = Shader.PropertyToID("_Stochastic");
            m_stormVT_TerrainTileGen_LayerAmount = Shader.PropertyToID("_LayerAmount");
            m_stormVT_TerrainTileGen_TerrainHeightmapRecipSize = Shader.PropertyToID("_TerrainHeightmapRecipSize");
            m_stormVT_TerrainTileGen_TerrainTexture = Shader.PropertyToID("_TerrainTexture");
            m_stormVT_TerrainTileGen_MacroVariationMap = Shader.PropertyToID("_MacroVariationMap");
            m_stormVT_TerrainTileGen_DetailNormalMap = Shader.PropertyToID("_DetailNormalMap");
            m_stormVT_TerrainTileGen_DetailNormalScale = Shader.PropertyToID("_DetailNormalScale");

            m_stormVT_TerrainTileGen_AlbedoArray = Shader.PropertyToID("_AlbedoArray");
            m_stormVT_TerrainTileGen_NormalArray = Shader.PropertyToID("_NormalArray");

            m_stormVT_AlphaMerge_Alpha = Shader.PropertyToID("_Alpha");


            //Page Table Update
            m_stormVT_PageTableUpdate_PageTableCount = Shader.PropertyToID("_PageTableCount");
            m_stormVT_PageTableUpdate_NodeCount = Shader.PropertyToID("_NodeCount");
            m_stormVT_PageTableUpdate_SliceIndex = Shader.PropertyToID("_SliceIndex");
            m_stormVT_PageTableUpdate_NodeDataBuffer = Shader.PropertyToID("_NodeDataBuffer");
            m_stormVT_PageTableUpdate_PageTableDataBuffer = Shader.PropertyToID("_PageTableDataBuffer");
            
            m_stormVT_CreateWorldNormalMap_Height = Shader.PropertyToID("_Height");
            m_stormVT_CreateWorldNormalMap_Heightmap = Shader.PropertyToID("_Heightmap");
            m_stormVT_CreateWorldNormalMap_TexelSize = Shader.PropertyToID("_TexelSize");

            m_stormVT_CreateWorldNormalMap_TileSize = Shader.PropertyToID("_TileSize");
            m_stormVT_CreateWorldNormalMap_TileCenter = Shader.PropertyToID("_TileCenter");
            m_stormVT_CreateWorldNormalMap_WorldDeriv = Shader.PropertyToID("_WorldDeriv");


            m_stormVT_SnowPass_NormalMap = Shader.PropertyToID("_NormalMap");
            m_stormVT_SnowPass_Source0 = Shader.PropertyToID("_Source0");
            m_stormVT_SnowPass_HeightMap = Shader.PropertyToID("_HeightMap");

            m_stormVT_SnowPass_SnowHeight = Shader.PropertyToID("_SnowHeight");
            m_stormVT_SnowPass_SnowFalloff = Shader.PropertyToID("_SnowFalloff");
            m_stormVT_SnowPass_SnowWorldAlignedRemap = Shader.PropertyToID("_SnowWorldAlignedRemap");
            m_stormVT_SnowPass_SnowCurvatureInfluence = Shader.PropertyToID("_SnowCurvatureInfluence");
            m_stormVT_SnowPass_SnowCurvatureIntensity = Shader.PropertyToID("_SnowCurvatureIntensity");
            m_stormVT_SnowPass_SnowMaskMinMax = Shader.PropertyToID("_SnowMaskMinMax");
            m_stormVT_SnowPass_SnowColor = Shader.PropertyToID("_SnowColor");
            m_stormVT_SnowPass_SnowUVScale = Shader.PropertyToID("_SnowUVScale");
            m_stormVT_SnowPass_SnowAlbedo = Shader.PropertyToID("_SnowAlbedo");
            m_stormVT_SnowPass_SnowNormal = Shader.PropertyToID("_SnowNormal");
            m_stormVT_SnowPass_SnowMask = Shader.PropertyToID("_SnowMask");


            m_stormVT_SnowPass_HeightScale = Shader.PropertyToID("_HeightScale");
            m_stormVT_SnowPass_HeightOffset = Shader.PropertyToID("_HeightOffset");

            //Height - Normal - Holes
            m_stormVT_TileCopy_TerrainHeightMap = Shader.PropertyToID("_TerrainHeightMap");
            m_stormVT_TileCopy_TerrainWorldNormalMap = Shader.PropertyToID("_TerrainWorldNormalMap");
            m_stormVT_TileCopy_TerrainHolesMap = Shader.PropertyToID("_TerrainHolesMap");
            m_stormVT_TileCopy_TileHeightMap = Shader.PropertyToID("_TileHeightMap");
            m_stormVT_TileCopy_TileWorldNormalMap = Shader.PropertyToID("_TileWorldNormalMap");
            m_stormVT_TileCopy_TileHolesMap = Shader.PropertyToID("_TileHolesMap");

            //Distance Map
            m_stormVT_TileCopy_TerrainDistanceMapColor = Shader.PropertyToID("_TerrainDistanceMapColor");
            m_stormVT_TileCopy_TerrainDistanceMapNormal = Shader.PropertyToID("_TerrainDistanceMapNormal");
            m_stormVT_TileCopy_TileDistanceMapColor = Shader.PropertyToID("_TileDistanceMapColor");
            m_stormVT_TileCopy_TileDistanceMapNormal = Shader.PropertyToID("_TileDistanceMapNormal");
            m_stormVT_TileCopy_TerrainGrassMapNormal = Shader.PropertyToID("_TerrainGrassMapNormal");
            m_stormVT_TileCopy_TerrainTreeMapNormal = Shader.PropertyToID("_TerrainTreeMapNormal");

            m_stormVT_TileCopy_TerrainGrassMapColor = Shader.PropertyToID("_TerrainGrassMapColor");
            m_stormVT_TileCopy_TerrainTreeMapColor = Shader.PropertyToID("_TerrainTreeMapColor");
            m_stormVT_TileCopy_TileGrassMapColor = Shader.PropertyToID("_TileGrassMapColor");
            m_stormVT_TileCopy_TileTreeMapColor = Shader.PropertyToID("_TileTreeMapColor");

            m_stormVT_TerrainTileGen_TileSource0 = Shader.PropertyToID("_TileSource0");
            m_stormVT_TerrainTileGen_TileSource1 = Shader.PropertyToID("_TileSource1");
            m_stormVT_TerrainTileGen_PreviousControl = Shader.PropertyToID("_PreviousControl");
            m_stormVT_TerrainTileGen_PreviousMaxHeight = Shader.PropertyToID("_PreviousMaxHeight");
            m_stormVT_TerrainTileGen_PreviousSumHeight = Shader.PropertyToID("_PreviousSumHeight");

            for (int i = 0; i < 4; i++)
            {
                m_stormVT_TerrainTileGen_DiffuseTextures[i] = Shader.PropertyToID("TerrainLayerDiffuse" + i);
                m_stormVT_TerrainTileGen_NormalTextures[i] = Shader.PropertyToID("TerrainLayerNormal" + i);
                m_stormVT_TerrainTileGen_MaskTextures[i] = Shader.PropertyToID("TerrainLayerMask" + i);
            }

            for(int i = 0; i < 4; i++)
            {
                m_stormVT_TerrainTileGen_ControlMaps[i] = Shader.PropertyToID("_Control" + i);
            }
            
            // Index = Offset = Name
            // 0, 0  = -1, 1  = _TopLeft
            // 1, 0  =  0, 1  = _TopCenter
            // 2, 0  =  1, 1  = _TopRight
            // 0, 1  = -1, 0  = _MiddleLeft
            // 1, 1  =  0, 0  = _MiddleCenter
            // 2, 1  =  1, 0  = _MiddleRight
            // 0, 2  = -1,-1  = _BottomLeft
            // 1, 2  =  0,-1  = _BottomCenter
            // 2, 2  =  1,-1  = _BottomRight
            string[,] tileShaderNames = new string[3,3]
            {
                { "_TopLeft",    "_TopCenter",    "_TopRight" },
                { "_MiddleLeft", "_MiddleCenter", "_MiddleRight" },
                { "_BottomLeft", "_BottomCenter", "_BottomRight" }
            };

            for (int x = 0; x < 3; x++)
            {
                for (int y = 0; y < 3; y++)
                {
                    string tileName = tileShaderNames[x, y];
                    string tileNameEnabled = $"{tileName}Enabled";
                    m_stormVT_TileTextureNames[x, y] = Shader.PropertyToID(tileName);
                    m_stormVT_TileTextureNamesEnabled[x, y] = Shader.PropertyToID(tileNameEnabled);
                }
            }
            
            
            m_stormVT_TileCaptureBlend_TileSource0 = Shader.PropertyToID("_TileSource0");
            m_stormVT_TileCaptureBlend_TileSource1 = Shader.PropertyToID("_TileSource1");
            m_stormVT_TileCaptureBlend_CaptureSource0 = Shader.PropertyToID("_CaptureSource0");
            m_stormVT_TileCaptureBlend_CaptureSource1 = Shader.PropertyToID("_CaptureSource1");

            m_stormVT_DecalBlend_Source0 = Shader.PropertyToID("_DecalSource0");
            m_stormVT_DecalBlend_Source1 = Shader.PropertyToID("_DecalSource1");
            m_stormVT_DecalBlend_Source2 = Shader.PropertyToID("_DecalSource2");
            
            
            m_stormVT_CreatePageTable_NodeCount = Shader.PropertyToID("_NodeCount");
            m_stormVT_CreatePageTable_NodeBuffer = Shader.PropertyToID("_NodeDataBuffer");
            m_stormVT_CreatePageTable_PageTableInfo = Shader.PropertyToID("_PageTableInfo");

            m_stormVT_DecalTileSize = Shader.PropertyToID("_TileSize");
            m_stormVT_DecalTilePos = Shader.PropertyToID("_TilePos");
        }
    }
}

