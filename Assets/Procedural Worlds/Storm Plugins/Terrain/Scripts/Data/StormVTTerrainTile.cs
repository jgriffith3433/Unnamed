using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Profiling;
using System.Runtime.InteropServices;
using static ProceduralWorlds.Storm.Terrain.StormVTEnums;

namespace ProceduralWorlds.Storm.Terrain
{
    public class StormVTTerrainTile : MonoBehaviour
    {

        /// <summary>
        /// Settings object
        /// </summary>
        public StormVTSettings stormVTSettings;

        /// <summary>
        /// Grid data for existing terrains
        /// </summary>
        [HideInInspector]
        public StormVTTerrainGrid stormVTTerrainGrid;

        /// <summary>
        /// Reference to the terrain texture manager
        /// </summary>
        public StormVTTerrainTextureManager stormVTTerrainTextureManager;

        /// <summary>
        /// Reference to the terrain texture layers component
        /// </summary>
        [HideInInspector]
        public StormVTTerrainTextureLayers stormVTTerrainTextureLayers;

        /// <summary>
        /// Reference to the tile capturer camera game object
        /// </summary>
        [HideInInspector]
        public GameObject m_tileCapturer; 
        
        /// <summary>
        /// Buffer used for storing terrain layer information
        /// </summary>
        private ComputeBuffer m_terrainLayerBuffer;

        /// <summary>
        /// Shader used for creating terrain tiles (based on Unity's terrain shader)
        /// </summary>
        [HideInInspector]
        public Shader m_terrainTilePassShader;

        /// <summary>
        /// Material used for blitting for creating terrain tiles
        /// </summary>
        private Material m_terrainTileMaterial;

        /// <summary>
        /// The world space transform matrix of the current tile
        /// </summary>
        [HideInInspector]
        public Matrix4x4 m_tileMatrix;

        /// <summary>
        /// Recip size for the terrains in the scene
        /// </summary>
        private Vector4 m_terrainRecipSize;

        /// <summary>
        /// Lower resolution recip size for the terrains in the scene (257 * 257)
        /// </summary>
        private Vector4 m_terrainRecipSizeLowRes;

        /// <summary>
        /// Terrain size for all terrains in scene
        /// </summary>
        private float m_terrainSize;

        /// <summary>
        /// Half the terrain size for all terrains in scene
        /// </summary>
        private float m_terrainSizeHalf;

        /// <summary>
        /// Vector2 of the half terrain size for faster calculations
        /// </summary>
        private Vector2 m_terrainSizeHalfVector;

        /// <summary>
        /// Height render texture buffer object. Used for tile creation with multiple Blit() calls
        /// </summary>
        public StormVTRenderBuffer[] m_heightRenderBuffer;

        /// <summary>
        /// Normal render texture buffer object. Used for tile creation with multiple Blit() calls
        /// </summary>
        public StormVTRenderBuffer[] m_normalRenderBuffer;

        /// <summary>
        /// Source0 render texture buffer object. Used for tile creation with multiple Blit() calls
        /// </summary>
        public StormVTRenderBuffer[] m_source0RenderBuffer;

        /// <summary>
        /// Source1 render texture buffer object. Used for tile creation with multiple Blit() calls
        /// </summary>
        public StormVTRenderBuffer[] m_source1RenderBuffer;

        /// <summary>
        /// Height blending control summation texture
        /// </summary>
        public StormVTRenderBuffer[] m_controlSumRenderBuffer;

        /// <summary>
        /// Height blending max height texture
        /// </summary>
        public StormVTRenderBuffer[] m_maxHeightRenderBuffer;

        /// <summary>
        /// Height blending heightmap summation texture
        /// </summary>
        public StormVTRenderBuffer[] m_sumHeightRenderBuffer;

        /// <summary>
        /// Multiple Render Targets (MRT) buffer array for tile textures
        /// </summary>
        private StormVTMRTBuffer[] m_tileDataMRTBuffer;

        /// <summary>
        /// Render buffer array of distance color map
        /// </summary>
        public StormVTRenderBuffer[] m_distanceMapColorBuffer;

        /// <summary>
        /// Render buffer array of distance normal map
        /// </summary>
        public StormVTRenderBuffer[] m_distanceMapNormalBuffer;

        /// <summary>
        /// Render buffer array of grass color map
        /// </summary>
        public StormVTRenderBuffer[] m_grassMapColorBuffer;

        /// <summary>
        /// Render buffer array of tree color map
        /// </summary>
        public StormVTRenderBuffer[] m_treeMapColorBuffer;

        /// <summary>
        /// The amount of render texture double buffers in use. Amount is the same as the max requests per frame amount.
        /// </summary>
        [HideInInspector]
        public int m_bufferPoolCount = 1;

        /// <summary>
        /// Used to set on holes texture if it is null
        /// </summary>
        private Texture2D m_blankWhiteTex;

        /// <summary>
        /// List of number of overlapping terrains on a tile
        /// </summary>
        private int[] m_foundTerrains;

        /// <summary>
        /// Count of overlapping terrains
        /// </summary>
        private int m_foundTerrainsCount;

        /// <summary>
        /// Stores whether or not we have initialized this system
        /// </summary>
        [HideInInspector]
        public bool m_initialized = false;

        #region Shaer Passes
        /// <summary>
        /// Pass to blit the Height, Normal and Holes map
        /// </summary>
        private int m_passHeightNormalHoles;

        /// <summary>
        /// Pass to blit the color sources (runtime option)
        /// </summary>
        private int m_passColorSources;

        /// <summary>
        /// Pass to blend tangent normal onto the world normal, for final Source1 creation (worldNormal.xz, AO, Smoothness)
        /// </summary>
        private int m_passWorldNormalBlend;

        /// <summary>
        /// Pass to blit the color sources (low end mobile option)
        /// </summary>
        private int m_passColorSourcesSimple;

        /// <summary>
        /// Pass to create and blend snow onto existing source0 and source1 textures
        /// </summary>
        private int m_passSnow;

        /// <summary>
        /// Pass to create the max value of all terrain layers height, used for height blending
        /// </summary>
        private int m_passMaxHeight;

        /// <summary>
        /// Pass to create the summed height value of all terrain layers height, used for height blending
        /// </summary>
        private int m_passSumHeight;

        /// <summary>
        /// Pass to blit the color source (height blending)
        /// </summary>
        private int m_passColorSourcesHeight;

        /// <summary>
        /// Pass to create procedural Height, Normal and Holes texture (Procedural option)
        /// </summary>
        private int m_passProceduralHeightNormalHoles;

        /// <summary>
        /// Pass to create procedural color textures (Procedural option)
        /// </summary>
        private int m_passProceduralColorSources;

        /// <summary>
        /// Pass to create the color sources (Texture array option)
        /// </summary>
        private int m_passArrayColorSources;

        /// <summary>
        /// Pass to create the color sources for low end devices (Low end option)
        /// </summary>
        private int m_passArrayColorSourcesSimple;

        /// <summary>
        /// Pass to blend the decals into the source0 and source1 textures
        /// </summary>
        private int m_passBlendCapture;

        /// <summary>
        /// Pass to create the distance tree and grass maps
        /// </summary>
        private int m_passDistanceMap;

        /// <summary>
        /// Pass to blend the decals into the source0 and source1 textures
        /// </summary>
        private int m_passDecalBlend;
        #endregion

        /// <summary>
        /// Stores the number of supported tile layers.
        /// Defaults to 4 when Texture Array option is used
        /// </summary>
        private int m_supportedTileLayers;

        /// <summary>
        /// List of terrain tile layers
        /// </summary>
        private TerrainTileLayer[] m_terrainTileLayers;

        /// <summary>
        /// Stores information about a Unity Terrain Layer
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct TerrainTileLayer
        {
            public static readonly int STRIDE = sizeof(float) * (19 + 8+8+8+4+4+4) + sizeof(uint);
            public float4 tilingOffset;
            public float4 remapMin;
            public float4 remapMax;
            public float4 color;
            public float4 puddlesColor;
            public float4 puddlesData;
            public float4 puddlesRandomMask;
            public float4 macroVariationColor;
            public float4 macroVariationMask;
            public float4 heightPackedData;
            public float4 displacementPackedData;
            public float4 stochasticPackedData;
            public float4 detailNormalPackedData;
            public float2 worldSpaceSize;
            public float normalStrength;
            public uint packedLayerSettings;
        }

        /// <summary>
        /// Initializer method
        /// </summary>
        public void Start()
        {
            StormVTSettings.OnUpdateStormVTSettings += SetSnowVariables;
            StormVTTerrainTextureLayer.OnTerrainTileLayerUpdated += UpdateTerrainTileLayer;

        }

        /// <summary>
        /// Repacked all the terrain layer data
        /// </summary>
        public void UpdateTerrainTileLayer()
        {
            //Initialize TerrainTileLayer Structs in the layer properties
            for (int i = 0; i < stormVTTerrainTextureLayers.stormVTTerrainTextureLayers.Count; i++)
            {
                stormVTTerrainTextureLayers.stormVTTerrainTextureLayers[i].PackToTerrainTileLayerStruct(m_terrainSize);
            }
        }

        /// <summary>
        /// Initializer materials and render textures
        /// </summary>
        /// <param name="stormSettings"> Settings data</param>
        public void InitializeData(StormVTSettings stormSettings)
        {
            m_initialized = false;
            m_supportedTileLayers = 4;
            if (stormSettings.m_tileLoadOption == LoadOption.TextureArray)
            {
                m_supportedTileLayers = stormVTTerrainTextureLayers.stormVTTerrainTextureLayers.Count;
            }

            //Setup terrain tile buffer
            m_terrainLayerBuffer = new ComputeBuffer(m_supportedTileLayers, TerrainTileLayer.STRIDE, ComputeBufferType.Structured);

            m_terrainTileMaterial = new Material(m_terrainTilePassShader);

            //Snow settings
            if (stormSettings.m_Snow)
            {
                SetSnowVariables();
            }

            stormVTSettings = stormSettings;

            //Loop through tile layers and set their default data
            m_terrainTileLayers = new TerrainTileLayer[m_supportedTileLayers];
            for(int i = 0; i < m_supportedTileLayers; i++)
            {
                if (stormSettings.m_tileLoadOption == LoadOption.TextureArray)
                {
                    if(i < stormVTTerrainTextureLayers.stormVTTerrainTextureLayers.Count)
                    {
                        m_terrainTileLayers[i] = stormVTTerrainTextureLayers.stormVTTerrainTextureLayers[i].m_terrainTileLayer;
                    }
                    else
                    {
                        m_terrainTileLayers[i] = new TerrainTileLayer()
                        {
                            normalStrength = 1,
                            remapMin = 0,
                            remapMax = 1,
                            tilingOffset = new Vector4(128, 128, 1, 1)
                        };
                    }

                }
                else
                {
                    m_terrainTileLayers[i] = new TerrainTileLayer()
                    {
                        normalStrength = 1,
                        remapMin = 0,
                        remapMax = 1,
                        tilingOffset = new Vector4(128, 128, 1, 1)
                    };
                }
                    
            }

            //Assign terrain tile layers to buffer data
            m_terrainLayerBuffer.SetData(m_terrainTileLayers);
            m_terrainTileMaterial.SetBuffer(StormVTShaderIDs.m_stormVT_TerrainTileGen_terrainLayersBuffer, m_terrainLayerBuffer);

            InitializeRenderBuffers(m_bufferPoolCount);


            //Set constant terrain data
            float terrainResolution = stormVTTerrainGrid.m_heightmapResolution;
            m_terrainRecipSize = new Vector4(1.0f / terrainResolution, 1.0f / terrainResolution, 1.0f / (terrainResolution - 1.0f), 1.0f / (terrainResolution - 1.0f));
            terrainResolution = 257.0f;
            m_terrainRecipSizeLowRes = new Vector4(1.0f / terrainResolution, 1.0f / terrainResolution, 1.0f / (terrainResolution - 1.0f), 1.0f / (terrainResolution - 1.0f));
            m_terrainSize = stormVTTerrainGrid.m_terrainSize;
            m_terrainSizeHalf = m_terrainSize * 0.5f;
            m_terrainSizeHalfVector = new Vector2(m_terrainSizeHalf, m_terrainSizeHalf);

            m_terrainTileMaterial.SetVector(StormVTShaderIDs.m_stormVT_TerrainTileGen_TerrainHeightmapRecipSize, m_terrainRecipSize);
            m_terrainTileMaterial.SetVector(StormVTShaderIDs.m_stormVT_TerrainTileGen_TerrainSize, new Vector4(m_terrainSize, m_terrainSize));

            m_terrainTileMaterial.SetFloat(StormVTShaderIDs.m_stormVT_TerrainTileGen_HeightBlendFactor, 0.3f);

            m_foundTerrains = new int[stormVTTerrainGrid.m_gridList.Length];
            m_foundTerrainsCount = 0;

            m_terrainTileMaterial.SetTexture(StormVTShaderIDs.m_stormVT_TerrainTileGen_MacroVariationMap, stormSettings.m_macroVariationTexture);
            m_terrainTileMaterial.SetTexture(StormVTShaderIDs.m_stormVT_TerrainTileGen_DetailNormalMap, stormSettings.m_detailNormalTexture);


            m_blankWhiteTex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            Color[] pixels = new Color[1];
            pixels[0] = Color.white;
            m_blankWhiteTex.SetPixels(pixels);
            m_blankWhiteTex.Apply(false, true);

            //Initialize TerrainTileLayer Structs in the layer properties
            for(int i = 0; i < stormVTTerrainTextureLayers.stormVTTerrainTextureLayers.Count; i++)
            {
                stormVTTerrainTextureLayers.stormVTTerrainTextureLayers[i].PackToTerrainTileLayerStruct(m_terrainSize);
            }

            //Assign shader id pass references
            m_passHeightNormalHoles = m_terrainTileMaterial.FindPass("Height - Normal - Holes Copy");
            m_passColorSources = m_terrainTileMaterial.FindPass("Color Sources Pass");
            m_passWorldNormalBlend = m_terrainTileMaterial.FindPass("World Normal Blend");
            m_passColorSourcesSimple = m_terrainTileMaterial.FindPass("Color Sources Pass Simple");
            m_passSnow = m_terrainTileMaterial.FindPass("Snow Source Combined");
            m_passMaxHeight = m_terrainTileMaterial.FindPass("Max Height");
            m_passSumHeight = m_terrainTileMaterial.FindPass("Sum Height");
            m_passColorSourcesHeight = m_terrainTileMaterial.FindPass("Color Sources Pass Height");

            m_passProceduralHeightNormalHoles = m_terrainTileMaterial.FindPass("Procedural - Height Normal Holes");
            m_passProceduralColorSources = m_terrainTileMaterial.FindPass("Procedural - Color Sources Pass");
            m_passArrayColorSources = m_terrainTileMaterial.FindPass("Array Color Sources Pass");
            m_passArrayColorSourcesSimple = m_terrainTileMaterial.FindPass("Array Color Sources Pass Simple");

            m_passBlendCapture = m_terrainTileMaterial.FindPass("All Sources");
            m_passDistanceMap = m_terrainTileMaterial.FindPass("Distance Map Copy");
            m_passDecalBlend = m_terrainTileMaterial.FindPass("Decal Blend");

            m_initialized = true;

        }

        /// <summary>
        /// Update terrain layer information when edited from the Terrain Texture Layers scriptable object
        /// </summary>
        public void UpdateTerrainLayersBuffer()
        {
            if(m_initialized)
            {
                if (stormVTSettings.m_tileLoadOption == LoadOption.TextureArray)
                {
                    for (int i = 0; i < stormVTTerrainTextureLayers.stormVTTerrainTextureLayers.Count; i++)
                    {
                        m_terrainTileLayers[i] = stormVTTerrainTextureLayers.stormVTTerrainTextureLayers[i].m_terrainTileLayer;
                    }
                    m_terrainLayerBuffer.SetData(m_terrainTileLayers);

                    BufferCheck();
                }
            }
           
                
        }

        /// <summary>
        /// Initializes the render buffers, based on MaxRequetsPerFrame from StormVTSettings
        /// </summary>
        /// <param name="count"></param>
        public void InitializeRenderBuffers(int count)
        {
            m_bufferPoolCount = count;
            if(stormVTSettings != null)
            {
                int tileRes = stormVTSettings.m_tileSizedPadded;
                int terrainTileRes = stormVTSettings.m_terrainTileSizePadded;

                //Create array sizes
                m_heightRenderBuffer = new StormVTRenderBuffer[m_bufferPoolCount];
                m_normalRenderBuffer = new StormVTRenderBuffer[m_bufferPoolCount];
                m_source0RenderBuffer = new StormVTRenderBuffer[m_bufferPoolCount];
                m_source1RenderBuffer = new StormVTRenderBuffer[m_bufferPoolCount];
                m_controlSumRenderBuffer = new StormVTRenderBuffer[m_bufferPoolCount];
                m_maxHeightRenderBuffer = new StormVTRenderBuffer[m_bufferPoolCount];
                m_sumHeightRenderBuffer = new StormVTRenderBuffer[m_bufferPoolCount];
                m_tileDataMRTBuffer = new StormVTMRTBuffer[m_bufferPoolCount];

                m_distanceMapColorBuffer = new StormVTRenderBuffer[m_bufferPoolCount];
                m_distanceMapNormalBuffer = new StormVTRenderBuffer[m_bufferPoolCount];
                m_grassMapColorBuffer = new StormVTRenderBuffer[m_bufferPoolCount];
                m_treeMapColorBuffer = new StormVTRenderBuffer[m_bufferPoolCount];

                //Initialise each render buffer within the array
                for (int i = 0; i < m_bufferPoolCount; i++)
                {
                    m_heightRenderBuffer[i] = new StormVTRenderBuffer(terrainTileRes, StormVTConstants.GetHeightmapRenderTextureFormat(), RenderTextureReadWrite.Linear);
                    m_normalRenderBuffer[i] = new StormVTRenderBuffer(terrainTileRes, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB, true);
                    m_source0RenderBuffer[i] = new StormVTRenderBuffer(tileRes, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB, true);
                    m_source1RenderBuffer[i] = new StormVTRenderBuffer(tileRes, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear, true);
                    m_controlSumRenderBuffer[i] = new StormVTRenderBuffer(tileRes, RenderTextureFormat.R8, RenderTextureReadWrite.Linear);
                    m_maxHeightRenderBuffer[i] = new StormVTRenderBuffer(tileRes, RenderTextureFormat.R8, RenderTextureReadWrite.Linear, true);
                    m_sumHeightRenderBuffer[i] = new StormVTRenderBuffer(tileRes, RenderTextureFormat.R8, RenderTextureReadWrite.Linear, true);
                    m_tileDataMRTBuffer[i] = new StormVTMRTBuffer();

                    m_distanceMapColorBuffer[i] = new StormVTRenderBuffer(terrainTileRes, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB, true);
                    m_distanceMapNormalBuffer[i] = new StormVTRenderBuffer(terrainTileRes, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear, true);
                    m_grassMapColorBuffer[i] = new StormVTRenderBuffer(terrainTileRes, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB, true);
                    m_treeMapColorBuffer[i] = new StormVTRenderBuffer(terrainTileRes, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB, true);
                }
            }

        }

        /// <summary>
        /// When destroyed, release the layer buffers to prevent memory leaks
        /// </summary>
        public void OnDestroy()
        {
            if (m_terrainLayerBuffer != null)
                m_terrainLayerBuffer.Release();

            StormVTSettings.OnUpdateStormVTSettings -= SetSnowVariables;
            StormVTTerrainTextureLayer.OnTerrainTileLayerUpdated -= UpdateTerrainTileLayer;
        }
        
        /// <summary>
        /// Returns a list of all terrains within the desired bounds, list is gathered from the StormVTTerrainGrid object
        /// </summary>
        /// <param name="stormVTTerrainGrid"> Settings data</param>
        /// <param name="centerPosition"> Terrain grid data</param>
        /// <param name="size"> World size of the area to find terrains in</param>
        /// <returns></returns>
        public void GetAllTerrainsOverlappingTile(Vector2 centerPos, float size)
        {
            m_foundTerrainsCount = 0;

            centerPos -= stormVTTerrainGrid.m_terrainBottomLeftPosition;

            Vector2 halfSize = new Vector2(size * 0.5f, size * 0.5f);
            Vector2 bottomLeftPos = centerPos - halfSize;
            Vector2 topRightPos = centerPos + halfSize;

            int2 closestBottomLeftCoord = new int2(
                    (int)Mathf.Floor(bottomLeftPos.x / stormVTTerrainGrid.m_terrainSize),
                    (int)Mathf.Floor(bottomLeftPos.y / stormVTTerrainGrid.m_terrainSize));

            int2 closestTopRightCoord = new int2(
                (int)Mathf.Floor(topRightPos.x / stormVTTerrainGrid.m_terrainSize),
                (int)Mathf.Floor(topRightPos.y / stormVTTerrainGrid.m_terrainSize));

            for(int x = closestBottomLeftCoord.x; x <= closestTopRightCoord.x; x++) //The size of this range changes depending on the size of the tile!
            {
                for(int y = closestBottomLeftCoord.y; y <= closestTopRightCoord.y; y++)
                {
                    int2 closestTerrainCoord = new int2(x, y);
                    if (closestTerrainCoord.x > -1 && closestTerrainCoord.x < stormVTTerrainGrid.m_numTerrainsX && closestTerrainCoord.y > -1 && closestTerrainCoord.y < stormVTTerrainGrid.m_numTerrainsY)
                    {
                        int index = stormVTTerrainGrid.GetIndex(closestTerrainCoord.x, closestTerrainCoord.y);
                        if(m_foundTerrainsCount < stormVTTerrainGrid.m_gridList.Length)
                        {
                            m_foundTerrains[m_foundTerrainsCount] = index;
                            m_foundTerrainsCount++;
                        }
                    }
                }
            }

        }

        /// <summary>
        /// Creates height, normal, and color data textures for desired tile
        /// </summary>
        /// <param name="coordinate">Coordinate of the tile to generate textures for</param>
        /// <param name="heightmap">Heightmap RenderTexture to blit into </param>
        /// <param name="normalmap">Normalmap RenderTexture to blit into</param>
        /// <param name="source0">ColorSource0 RenderTexture to blit into</param>
        /// <param name="source1">ColorSource1 RenderTexture to blit into</param>
        public void GenerateTileRuntime(int4 coordinate, int bufferIndex)
        {
            Profiler.BeginSample("Storm Terrain - Tile Generation");

            Profiler.BeginSample("Storm Terrain - Tile Generation - Coord To World Space");
            //Convert coordinate to bounds info
            (Vector3, float) tileBounds = StormVTZoomLayers.ConvertZoomCoordinateToWorldSpace(coordinate.xyz);
            float tileSize = tileBounds.Item2;
            //Convert pos to flat pos
            Vector2 centerPos = new Vector2(
                tileBounds.Item1.x,
                tileBounds.Item1.z);

            //Calculate world size padded
            float texelSize = (float)(tileSize / (int)stormVTSettings.m_tileSize);
            float worldSizeWithPadding = tileSize + (texelSize * 8);

            

            Profiler.EndSample();


            Profiler.BeginSample("Storm Terrain - Tile Generation - Clear Prev RTs");


            //Reset all previous settings
            RenderTexture.active = m_source0RenderBuffer[bufferIndex].GetActiveBuffer();
            GL.Clear(false, true, Color.clear);
            RenderTexture.active = m_source1RenderBuffer[bufferIndex].GetActiveBuffer();
            GL.Clear(false, true, Color.clear);
            RenderTexture.active = m_heightRenderBuffer[bufferIndex].GetActiveBuffer();
            GL.Clear(false, true, Color.clear);

            if (stormVTSettings.m_heightBlend)
            {
                //Clear max and sum height buffers
                RenderTexture.active = m_maxHeightRenderBuffer[bufferIndex].GetActiveBuffer();
                GL.Clear(false, true, Color.clear);
                RenderTexture.active = m_sumHeightRenderBuffer[bufferIndex].GetActiveBuffer();
                GL.Clear(false, true, Color.clear);
            }

            if(stormVTSettings.m_grassMapColor)
            {
                RenderTexture.active  = m_grassMapColorBuffer[bufferIndex].GetActiveBuffer();
                GL.Clear(false, true, Color.clear);
            }
            if(stormVTSettings.m_treeMapColor)
            {
                RenderTexture.active = m_treeMapColorBuffer[bufferIndex].GetActiveBuffer();
                GL.Clear(false, true, Color.clear);
            }
            if(stormVTSettings.m_grassMapNormal || stormVTSettings.m_treeMapNormal)
            {
                RenderTexture.active = m_distanceMapNormalBuffer[bufferIndex].GetActiveBuffer();
                GL.Clear(false, true, Color.clear);
            }

            Profiler.EndSample();


            Profiler.BeginSample("Storm Terrain - Tile Generation - Find All Overlapping Terrains");
            //Get all terrains covering this tile
            GetAllTerrainsOverlappingTile(centerPos, worldSizeWithPadding);
            Profiler.EndSample();


            Profiler.BeginSample("Storm Terrain - Tile Generation - Set Tile Matrix");
            m_tileMatrix = Matrix4x4.TRS(new Vector3(centerPos.x, 0, centerPos.y), Quaternion.identity, new Vector3(worldSizeWithPadding, worldSizeWithPadding, worldSizeWithPadding));
            m_terrainTileMaterial.SetMatrix(StormVTShaderIDs.m_stomrVT_TerrainTileGen_TileMatrix, m_tileMatrix);
            Profiler.EndSample();


            Profiler.BeginSample("Storm Terrain - Tile Generation - Set Base Material Variables");

            //Define tile center position and size
            m_terrainTileMaterial.SetVector(StormVTShaderIDs.m_stormVT_TerrainTileGen_TilePosition, centerPos);
            m_terrainTileMaterial.SetVector(StormVTShaderIDs.m_stormVT_TerrainTileGen_TileSize, new Vector4(worldSizeWithPadding, worldSizeWithPadding));

            Profiler.EndSample();

            Profiler.BeginSample("Storm Terrain - Tile Generation - Reset Terrain Layer Textures");

            Profiler.EndSample();

            if(stormVTSettings.m_tileLoadOption == LoadOption.Runtime)
            {
                //--- LOOP THROUGH ALL TERRAINS OVERLAPPING TILE---\\
                for (int terrainIndex = 0; terrainIndex < m_foundTerrainsCount; terrainIndex++)
                {
                    //Grab ref to current terrain
                    StormVTTerrainGridData curTerrain = stormVTTerrainGrid.m_gridList[m_foundTerrains[terrainIndex]];

                    StormVTTerrainTextureData terrainTextureData = stormVTTerrainTextureManager.GetTerrainData(stormVTTerrainGrid.m_gridList[m_foundTerrains[terrainIndex]].gridIndex);

                    //Grab terrain info
                    StormVTTerrainInfo terrainInfo;
                    bool foundTerrainInfo = StormVTTerrainManager.Instance.m_activeTerrains.TryGetValue(curTerrain.gridIndex, out terrainInfo);

                    Profiler.BeginSample("Storm Terrain - Tile Generation - Set Shader Tile Data");

                    //Set shader terrain position and size
                    Vector2 terrainPos = curTerrain.position + m_terrainSizeHalfVector;
                    m_terrainTileMaterial.SetVector(StormVTShaderIDs.m_stormVT_TerrainTileGen_TerrainPosition, terrainPos);


                    Profiler.EndSample();

                    //--- HEIGHT & NORMALMAP & TERRAIN HOLES TILE CREATION ---\\
                    #region Height And Normalmap Creation
                    GenerateHeightAndNormalMapsPerTerrain(bufferIndex, foundTerrainInfo, terrainInfo, terrainTextureData);

                    #endregion

                    GenerateColorMapsPerTerrain(bufferIndex, foundTerrainInfo, terrainInfo, terrainTextureData);
                    
                    //--- COLOR TILE CREation ---\\
                    #region Color Source Creation

                    Profiler.BeginSample("Storm Terrain - Tile Generation - Color Data");


                    Profiler.BeginSample("Storm Terrain - Tile Generation - Color Data - Grab Normalmap");
                    if (stormVTSettings.m_stormVTMode == StormVTMode.UnityTerrain)
                    {
                        if (foundTerrainInfo)
                        {
                            m_terrainTileMaterial.SetTexture(StormVTShaderIDs.m_stormVT_TerrainTileGen_WorldNormalMap, terrainInfo.normalmap);
                        }
                    }
                    else
                    {
                        if (terrainTextureData != null)
                        {
                            m_terrainTileMaterial.SetTexture(StormVTShaderIDs.m_stormVT_TerrainTileGen_WorldNormalMap, terrainTextureData.normalmap);
                        }
                        else
                        {
                            m_terrainTileMaterial.SetTexture(StormVTShaderIDs.m_stormVT_TerrainTileGen_WorldNormalMap, null);
                        }
                    }
                    Profiler.EndSample();

                    //Grab a reference to the count of this terrain's layers
                    int terrainLayerCount = curTerrain.terrainLayers.Count;

                    //Determine how many times we need to loop through the terrain layers (1 per 4 layers)
                    int loopCount = (int)math.ceil((float)terrainLayerCount / 4.0f);

                    #region Height Blending
                    if (stormVTSettings.m_heightBlend)
                    {
                        Profiler.BeginSample("Storm Terrain - Tile Generation - Color Data - Height Blend Max Height Pass");
                        //Max Height Pass
                        for (int passIndex = 0; passIndex < loopCount; passIndex++)
                        {
                            Profiler.BeginSample("Storm Terrain - Tile Generation - Color Data - Height Blend Max Height Control Set");
                            #region Set Control Map
                            //Set control map (splat map)
                            if (stormVTSettings.m_stormVTMode == StormVTMode.UnityTerrain)
                            {
                                if (foundTerrainInfo)
                                {
                                    if (passIndex < terrainInfo.splatmaps.Length)
                                    {
                                        //Set control map (splat map)
                                        m_terrainTileMaterial.SetTexture(StormVTShaderIDs.m_stormVT_TerrainTileGen_ControlMap, terrainInfo.splatmaps[passIndex]);
                                    }

                                }
                            }
                            else
                            {
                                //Set control map (splat map)
                                //Grab / Load control map from elsewhere
                                if (terrainTextureData != null)
                                {
                                    if (terrainTextureData.splatmaps != null)
                                    {
                                        if (passIndex < terrainTextureData.splatmaps.Length)
                                            m_terrainTileMaterial.SetTexture(StormVTShaderIDs.m_stormVT_TerrainTileGen_ControlMap, terrainTextureData.splatmaps[passIndex]);
                                    }

                                }
                                else
                                {
                                    m_terrainTileMaterial.SetTexture(StormVTShaderIDs.m_stormVT_TerrainTileGen_ControlMap, null);
                                }
                            }
                            #endregion

                            Profiler.EndSample();

                            //index offset is pass index * 4 layers
                            int offset = passIndex * 4;

                            //Set terrain layer data on material
                            SetPassTerrianLayerData(curTerrain, offset, true);

                            Profiler.BeginSample("Storm Terrain - Tile Generation - Color Data - Height Blend Max Height SetData");
                            //Now that we have the terrain tile layers set up, we can pass it into the blit material
                            m_terrainLayerBuffer.SetData(m_terrainTileLayers);
                            Profiler.EndSample();

                            BufferCheck();

                            Profiler.BeginSample("Storm Terrain - Tile Generation - Color Data - Height Blend Max Height Blit Group");
                            //Set previous max height
                            m_terrainTileMaterial.SetTexture(StormVTShaderIDs.m_stormVT_TerrainTileGen_PreviousMaxHeight, m_maxHeightRenderBuffer[bufferIndex].GetActiveBuffer());
                            m_maxHeightRenderBuffer[bufferIndex].SwapBuffers();
                            Graphics.Blit(null, m_maxHeightRenderBuffer[bufferIndex].GetActiveBuffer(), m_terrainTileMaterial, m_passMaxHeight);
                            m_terrainTileMaterial.SetTexture(StormVTShaderIDs.m_stormVT_TerrainTileGen_PreviousMaxHeight, m_maxHeightRenderBuffer[bufferIndex].GetActiveBuffer());
                            Profiler.EndSample();
                        }
                        Profiler.EndSample();

                        Profiler.BeginSample("Storm Terrain - Tile Generation - Color Data - Height Blend Sum Height Pass");
                        //Sum Height Pass
                        for (int passIndex = 0; passIndex < loopCount; passIndex++)
                        {
                            Profiler.BeginSample("Storm Terrain - Tile Generation - Color Data - Height Blend Sum Height Control Set");
                            #region Set Control Map
                            //Set control map (splat map)
                            if (stormVTSettings.m_stormVTMode == StormVTMode.UnityTerrain)
                            {
                                if (foundTerrainInfo)
                                {
                                    if (passIndex < terrainInfo.splatmaps.Length)
                                    {
                                        //Set control map (splat map)
                                        m_terrainTileMaterial.SetTexture(StormVTShaderIDs.m_stormVT_TerrainTileGen_ControlMap, terrainInfo.splatmaps[passIndex]);
                                    }

                                }
                            }
                            else
                            {
                                //Set control map (splat map)
                                //Grab / Load control map from elsewhere
                                if (terrainTextureData != null)
                                {
                                    if (terrainTextureData.splatmaps != null)
                                    {
                                        if (passIndex < terrainTextureData.splatmaps.Length)
                                            m_terrainTileMaterial.SetTexture(StormVTShaderIDs.m_stormVT_TerrainTileGen_ControlMap, terrainTextureData.splatmaps[passIndex]);
                                    }

                                }
                                else
                                {
                                    m_terrainTileMaterial.SetTexture(StormVTShaderIDs.m_stormVT_TerrainTileGen_ControlMap, null);
                                }
                            }
                            #endregion

                            Profiler.EndSample();

                            //index offset is pass index * 4 layers
                            int offset = passIndex * 4;

                            //Set terrain layer data on material
                            SetPassTerrianLayerData(curTerrain, offset, true);

                            Profiler.BeginSample("Storm Terrain - Tile Generation - Color Data - Height Blend Sum Height SetData");
                            //Now that we have the terrain tile layers set up, we can pass it into the blit material
                            m_terrainLayerBuffer.SetData(m_terrainTileLayers);
                            Profiler.EndSample();

                            BufferCheck();

                            Profiler.BeginSample("Storm Terrain - Tile Generation - Color Data - Height Blend Max Height Blit Group");
                            m_terrainTileMaterial.SetTexture(StormVTShaderIDs.m_stormVT_TerrainTileGen_PreviousSumHeight, m_sumHeightRenderBuffer[bufferIndex].GetActiveBuffer());
                            m_sumHeightRenderBuffer[bufferIndex].SwapBuffers();
                            Graphics.Blit(null, m_sumHeightRenderBuffer[bufferIndex].GetActiveBuffer(), m_terrainTileMaterial, m_passSumHeight);
                            m_terrainTileMaterial.SetTexture(StormVTShaderIDs.m_stormVT_TerrainTileGen_PreviousSumHeight, m_sumHeightRenderBuffer[bufferIndex].GetActiveBuffer());
                            Profiler.EndSample();
                        }
                        Profiler.EndSample();

                    }


                    #endregion

                    #region Color Passes
                    //Loop through terrain per pass (every 4 layers)
                    for (int passIndex = 0; passIndex < loopCount; passIndex++)
                    {
                        Profiler.BeginSample("Storm Terrain - Tile Generation - Color Passes");

                        //m_terrainTileMaterial.SetInt(StormVTShaderIDs.m_stormVT_TerrainTileGen_First4Layers, passIndex == 0 ? 1 : 0);

                        Profiler.BeginSample("Storm Terrain - Tile Generation - Color Data - Grab Splatmap");
                        //Set control map (splat map)
                        if (stormVTSettings.m_stormVTMode == StormVTMode.UnityTerrain)
                        {
                            if (foundTerrainInfo)
                            {
                                if (passIndex < terrainInfo.splatmaps.Length)
                                {
                                    //Set control map (splat map)
                                    m_terrainTileMaterial.SetTexture(StormVTShaderIDs.m_stormVT_TerrainTileGen_ControlMap, terrainInfo.splatmaps[passIndex]);
                                }

                            }
                        }
                        else
                        {
                            //Set control map (splat map)
                            //Grab / Load control map from elsewhere
                            if (terrainTextureData != null)
                            {
                                if (terrainTextureData.splatmaps != null)
                                {
                                    if (passIndex < terrainTextureData.splatmaps.Length)
                                        m_terrainTileMaterial.SetTexture(StormVTShaderIDs.m_stormVT_TerrainTileGen_ControlMap, terrainTextureData.splatmaps[passIndex]);
                                }

                            }
                            else
                            {
                                m_terrainTileMaterial.SetTexture(StormVTShaderIDs.m_stormVT_TerrainTileGen_ControlMap, null);
                            }
                        }

                        Profiler.EndSample();

                        //index offset is pass index * 4 layers
                        int offset = passIndex * 4;

                        Profiler.BeginSample("Storm Terrain - Tile Generation - Collect Pass Data");

                        //Loop through current set of layers
                        for (int i = 0; i < 4; i++)
                        {

                            //Determine index into the terrain alyers
                            int layerIndex = offset + i;

                            //Safety check so we won't go out of bounds
                            if (layerIndex < curTerrain.terrainLayers.Count && layerIndex < curTerrain.terrainLayerIndices.Count())
                            {
                                //Grab a reference to the terrain layer
                                StormVTTerrainTextureLayer stormVTTerrainTextureLayer = stormVTTerrainTextureLayers.stormVTTerrainTextureLayers[curTerrain.terrainLayerIndices[layerIndex]];

                                Profiler.BeginSample("Storm Terrain - Tile Generation - Collect Pass Data - Set TerrainTileLayer");

                                m_terrainTileLayers[i] = stormVTTerrainTextureLayer.m_terrainTileLayer;

                                Profiler.EndSample();

                                Profiler.BeginSample("Storm Terrain - Tile Generation - Collect Pass Data - Set Layer Textures");



                                if (stormVTTerrainTextureLayer != null)
                                {
                                    //Assign terrain layer textures to the blit material
                                    if (stormVTTerrainTextureLayer.m_packedDiffuseMap != null)
                                    {
                                        m_terrainTileMaterial.SetTexture(StormVTShaderIDs.m_stormVT_TerrainTileGen_DiffuseTextures[i], stormVTTerrainTextureLayer.m_packedDiffuseMap);
                                    }
                                    if (stormVTTerrainTextureLayer.m_packedNormalMap != null)
                                    {
                                        m_terrainTileMaterial.SetTexture(StormVTShaderIDs.m_stormVT_TerrainTileGen_NormalTextures[i], stormVTTerrainTextureLayer.m_packedNormalMap);
                                    }
                                }


                                Profiler.EndSample();
                            }
                        }

                        //Now that we have the terrain tile layers set up, we can pass it into the blit material
                        m_terrainLayerBuffer.SetData(m_terrainTileLayers);

                        BufferCheck();



                        Profiler.EndSample();

                        if (stormVTSettings.m_targetPlatform != StormVTPlatform.Low_End_Mobile_VR)
                        {
                            //Set Terrain Prev Accumulated Textures
                            m_terrainTileMaterial.SetTexture(StormVTShaderIDs.m_stormVT_TerrainTileGen_TileSource0, m_source0RenderBuffer[bufferIndex].GetActiveBuffer());
                            m_terrainTileMaterial.SetTexture(StormVTShaderIDs.m_stormVT_TerrainTileGen_TileSource1, m_source1RenderBuffer[bufferIndex].GetActiveBuffer());

                            //Swap Buffers for output
                            m_source0RenderBuffer[bufferIndex].SwapBuffers();
                            m_source1RenderBuffer[bufferIndex].SwapBuffers();



                            //Set MRT buffers
                            m_tileDataMRTBuffer[bufferIndex].SetSourceBuffers(m_source0RenderBuffer[bufferIndex].GetActiveBuffer(), m_source1RenderBuffer[bufferIndex].GetActiveBuffer());

                            Profiler.BeginSample("Storm Terrain - Tile Generation - Color Sources Blit");
                            Graphics.SetRenderTarget(m_tileDataMRTBuffer[bufferIndex].m_colorDataMRTBuffer, m_source0RenderBuffer[bufferIndex].GetActiveBuffer().depthBuffer);
                            //Blit data into the buffers
                            if (stormVTSettings.m_heightBlend)
                                Graphics.Blit(null, m_terrainTileMaterial, m_passColorSourcesHeight);
                            else
                            {
                                Graphics.Blit(null, m_terrainTileMaterial, m_passColorSources);
                            }
                            Profiler.EndSample();


                        }
                        else
                        {
                            //Set Terrain Prev Accumulated Textures
                            m_terrainTileMaterial.SetTexture(StormVTShaderIDs.m_stormVT_TerrainTileGen_TileSource0, m_source0RenderBuffer[bufferIndex].GetActiveBuffer());
                            m_terrainTileMaterial.SetTexture(StormVTShaderIDs.m_stormVT_TerrainTileGen_TileSource1, m_source1RenderBuffer[bufferIndex].GetActiveBuffer());

                            //Swap Buffers for output
                            m_source0RenderBuffer[bufferIndex].SwapBuffers();
                            m_source1RenderBuffer[bufferIndex].SwapBuffers();
                            //Set MRT buffers
                            m_tileDataMRTBuffer[bufferIndex].SetSourceBuffers(m_source0RenderBuffer[bufferIndex].GetActiveBuffer(), m_source1RenderBuffer[bufferIndex].GetActiveBuffer());

                            Profiler.BeginSample("Storm Terrain - Tile Generation - Color Sources Blit");
                            Graphics.SetRenderTarget(m_tileDataMRTBuffer[bufferIndex].m_colorDataMRTBuffer, m_source0RenderBuffer[bufferIndex].GetActiveBuffer().depthBuffer);
                            //Blit data into the buffers
                            Graphics.Blit(null, m_terrainTileMaterial, m_passColorSourcesSimple);
                            Profiler.EndSample();
                        }


                        Profiler.EndSample();
                    }
                    #endregion

                    Profiler.EndSample();


                    #endregion


                    //Snow settings
                    if (stormVTSettings.m_Snow && stormVTSettings.m_SnowGenerationMode == StormVTFeatureMode._Tile)
                    {
                        //Set height map (splat map)
                        if (stormVTSettings.m_stormVTMode == StormVTMode.UnityTerrain)
                        {
                            if (foundTerrainInfo)
                            {
                                m_terrainTileMaterial.SetTexture(StormVTShaderIDs.m_stormVT_TerrainTileGen_NormalMap, terrainInfo.normalmap);
                                m_terrainTileMaterial.SetTexture(StormVTShaderIDs.m_stormVT_TerrainTileGen_HeightMap, terrainInfo.terrain.terrainData.heightmapTexture);
                            }
                        }
                        else
                        {
                            //Grab / Load heightmap and normalmap from elsewhere
                            if (terrainTextureData != null)
                            {
                                m_terrainTileMaterial.SetTexture(StormVTShaderIDs.m_stormVT_TerrainTileGen_NormalMap, terrainTextureData.normalmap);
                                m_terrainTileMaterial.SetTexture(StormVTShaderIDs.m_stormVT_TerrainTileGen_HeightMap, terrainTextureData.heightmap);
                            }
                            else
                            {
                                m_terrainTileMaterial.SetTexture(StormVTShaderIDs.m_stormVT_TerrainTileGen_NormalMap, null);
                                m_terrainTileMaterial.SetTexture(StormVTShaderIDs.m_stormVT_TerrainTileGen_HeightMap, null);
                            }
                        }


                        //Set Terrain Prev Accumulated Textures
                        m_terrainTileMaterial.SetTexture(StormVTShaderIDs.m_stormVT_TerrainTileGen_TileSource0, m_source0RenderBuffer[bufferIndex].GetActiveBuffer());
                        m_terrainTileMaterial.SetTexture(StormVTShaderIDs.m_stormVT_TerrainTileGen_TileSource1, m_source1RenderBuffer[bufferIndex].GetActiveBuffer());

                        //Swap Buffers for output
                        m_source0RenderBuffer[bufferIndex].SwapBuffers();
                        m_source1RenderBuffer[bufferIndex].SwapBuffers();
                        //Set MRT buffers
                        m_tileDataMRTBuffer[bufferIndex].SetSourceBuffers(m_source0RenderBuffer[bufferIndex].GetActiveBuffer(), m_source1RenderBuffer[bufferIndex].GetActiveBuffer());

                        Profiler.BeginSample("Storm Terrain - Tile Generation - Color Sources Blit");
                        Graphics.SetRenderTarget(m_tileDataMRTBuffer[bufferIndex].m_colorDataMRTBuffer, m_source0RenderBuffer[bufferIndex].GetActiveBuffer().depthBuffer);
                        //Blit data into the buffers
                        Graphics.Blit(null, m_terrainTileMaterial, m_passSnow);
                        Profiler.EndSample();
                    }

                }

            }
            else if (stormVTSettings.m_tileLoadOption == LoadOption.Procedural)
            {
                //Set current buffers to write to
                m_tileDataMRTBuffer[bufferIndex].SetTerrainBuffers(
                    m_heightRenderBuffer[bufferIndex].GetActiveBuffer(),
                    m_normalRenderBuffer[bufferIndex].GetActiveBuffer());

                Profiler.BeginSample("Storm Terrain - Tile Generation - Height and Normal Blit BLIT CALL");
                //Set the render textures as the target
                Graphics.SetRenderTarget(m_tileDataMRTBuffer[bufferIndex].m_terrainDataMRTBuffer, m_heightRenderBuffer[bufferIndex].GetActiveBuffer().depthBuffer);
                //Blit data into the buffers
                Graphics.Blit(null, m_terrainTileMaterial, m_passProceduralHeightNormalHoles);


                //Set MRT buffers
                m_tileDataMRTBuffer[bufferIndex].SetSourceBuffers(m_source0RenderBuffer[bufferIndex].GetActiveBuffer(), m_source1RenderBuffer[bufferIndex].GetActiveBuffer());
                Profiler.BeginSample("Storm Terrain - Tile Generation - Color Sources Blit");
                Graphics.SetRenderTarget(m_tileDataMRTBuffer[bufferIndex].m_colorDataMRTBuffer, m_source0RenderBuffer[bufferIndex].GetActiveBuffer().depthBuffer);
                //Blit data into the buffers
                Graphics.Blit(null, m_terrainTileMaterial, m_passProceduralColorSources);
                Profiler.EndSample();
            }
            else if(stormVTSettings.m_tileLoadOption == LoadOption.TextureArray)
            {
                Profiler.BeginSample("Storm Terrain - Tile Generation - Set Texture Arrays");
                m_terrainTileMaterial.SetTexture(StormVTShaderIDs.m_stormVT_TerrainTileGen_AlbedoArray, stormVTTerrainTextureLayers.albedoArray);
                m_terrainTileMaterial.SetTexture(StormVTShaderIDs.m_stormVT_TerrainTileGen_NormalArray, stormVTTerrainTextureLayers.normalArray);
                Profiler.EndSample();

                //--- LOOP THROUGH ALL TERRAINS OVERLAPPING TILE---\\
                for (int terrainIndex = 0; terrainIndex < m_foundTerrainsCount; terrainIndex++)
                {
                    //Grab ref to current terrain
                    StormVTTerrainGridData curTerrain = stormVTTerrainGrid.m_gridList[m_foundTerrains[terrainIndex]];

                    StormVTTerrainTextureData terrainTextureData = stormVTTerrainTextureManager.GetTerrainData(stormVTTerrainGrid.m_gridList[m_foundTerrains[terrainIndex]].gridIndex);

                    //Grab terrain info
                    StormVTTerrainInfo terrainInfo;
                    bool foundTerrainInfo = StormVTTerrainManager.Instance.m_activeTerrains.TryGetValue(curTerrain.gridIndex, out terrainInfo);

                    Profiler.BeginSample("Storm Terrain - Tile Generation - Set Shader Tile Data");

                    //Set shader terrain position and size
                    Vector2 terrainPos = curTerrain.position + m_terrainSizeHalfVector;
                    m_terrainTileMaterial.SetVector(StormVTShaderIDs.m_stormVT_TerrainTileGen_TerrainPosition, terrainPos);



                    Profiler.EndSample();

                    //--- HEIGHT & NORMALMAP & TERRAIN HOLES TILE CREATION ---\\
                    #region Height And Normalmap Creation
                    GenerateHeightAndNormalMapsPerTerrain(bufferIndex, foundTerrainInfo, terrainInfo, terrainTextureData);

                    #endregion
                    
                    GenerateColorMapsPerTerrain(bufferIndex, foundTerrainInfo, terrainInfo, terrainTextureData);
                    
                    //--- COLOR TILE CREATION ---\\
                    
                    Profiler.BeginSample("Storm Terrain - Tile Generation - Color Data");

                    #region Set Control Maps
                    //Set control maps
                    if (stormVTSettings.m_stormVTMode == StormVTMode.UnityTerrain)
                    {
                        if (foundTerrainInfo)
                        {
                            int splatCount = terrainInfo.splatmaps.Length;
                            for (int i = 0; i < splatCount; i++)
                            {
                                m_terrainTileMaterial.SetTexture(StormVTShaderIDs.m_stormVT_TerrainTileGen_ControlMaps[i], terrainInfo.splatmaps[i]);
                            }
                            if(splatCount < 4)
                            {
                                for(int i = splatCount; i < 4; i++)
                                {
                                    m_terrainTileMaterial.SetTexture(StormVTShaderIDs.m_stormVT_TerrainTileGen_ControlMaps[i], null);
                                }
                            }
                        }
                    }
                    else
                    {
                        if (terrainTextureData != null)
                        {
                            if (terrainTextureData.splatmaps != null)
                            {
                                int splatCount = terrainTextureData.splatmaps.Length;
                                for (int i = 0; i < splatCount; ++i)
                                {
                                    m_terrainTileMaterial.SetTexture(StormVTShaderIDs.m_stormVT_TerrainTileGen_ControlMaps[i], terrainTextureData.splatmaps[i]);
                                }
                                if (splatCount < 4)
                                {
                                    for (int i = splatCount; i < 4; i++)
                                    {
                                        m_terrainTileMaterial.SetTexture(StormVTShaderIDs.m_stormVT_TerrainTileGen_ControlMaps[i], null);
                                    }
                                }
                            }
                        }
                        else
                        {
                            for (int i = 0; i < 4; ++i)
                            {
                                m_terrainTileMaterial.SetTexture(StormVTShaderIDs.m_stormVT_TerrainTileGen_ControlMaps[i], null);
                            }
                        }
                    }
                    #endregion

                    #region Set Normalmap
                    Profiler.BeginSample("Storm Terrain - Tile Generation - Color Data - Grab Normalmap");
                    
                    if(stormVTSettings.m_Snow)
                    {
                        m_terrainTileMaterial.SetTexture(StormVTShaderIDs.m_stormVT_TerrainTileGen_NormalMap, m_normalRenderBuffer[bufferIndex].GetActiveBuffer());
                        m_terrainTileMaterial.SetTexture(StormVTShaderIDs.m_stormVT_TerrainTileGen_HeightMap, m_heightRenderBuffer[bufferIndex].GetActiveBuffer());
                    }


                    Profiler.EndSample();
                    #endregion

                    for (int i = 0; i < m_supportedTileLayers; i++)
                    {
                        if (i < stormVTTerrainTextureLayers.stormVTTerrainTextureLayers.Count)
                        {
                            m_terrainTileLayers[i] = stormVTTerrainTextureLayers.stormVTTerrainTextureLayers[i].m_terrainTileLayer;
                        }
                    }

                    m_terrainLayerBuffer.SetData(m_terrainTileLayers);

                    BufferCheck();

                    //Set Terrain Prev Accumulated Textures
                    m_terrainTileMaterial.SetTexture(StormVTShaderIDs.m_stormVT_TerrainTileGen_TileSource0, m_source0RenderBuffer[bufferIndex].GetActiveBuffer());
                    m_terrainTileMaterial.SetTexture(StormVTShaderIDs.m_stormVT_TerrainTileGen_TileSource1, m_source1RenderBuffer[bufferIndex].GetActiveBuffer());

                    //Swap Buffers for output
                    m_source0RenderBuffer[bufferIndex].SwapBuffers();
                    m_source1RenderBuffer[bufferIndex].SwapBuffers();



                    //Set MRT buffers
                    m_tileDataMRTBuffer[bufferIndex].SetSourceBuffers(m_source0RenderBuffer[bufferIndex].GetActiveBuffer(), m_source1RenderBuffer[bufferIndex].GetActiveBuffer());

                    Profiler.BeginSample("Storm Terrain - Tile Generation - Color Sources Blit");
                    Graphics.SetRenderTarget(m_tileDataMRTBuffer[bufferIndex].m_colorDataMRTBuffer, m_source0RenderBuffer[bufferIndex].GetActiveBuffer().depthBuffer);

                    if (stormVTSettings.m_targetPlatform != StormVTPlatform.Low_End_Mobile_VR)
                    {
                        Graphics.Blit(null, m_terrainTileMaterial, m_passArrayColorSources);
                    }
                    else
                    {
                        Graphics.Blit(null, m_terrainTileMaterial, m_passArrayColorSourcesSimple);
                    }

                        
                    
                    Profiler.EndSample();

                    Profiler.EndSample();

                }
            }

            Profiler.BeginSample("Storm Terrain - Tile Generation - Color Sources World Normal Blit");
            //World normal blend for source1
            m_terrainTileMaterial.SetTexture(StormVTShaderIDs.m_stormVT_TerrainTileGen_TileSource1, m_source1RenderBuffer[bufferIndex].GetActiveBuffer());
            m_source1RenderBuffer[bufferIndex].SwapBuffers();
            m_terrainTileMaterial.SetTexture(StormVTShaderIDs.m_stormVT_TerrainTileGen_WorldNormalMap, m_normalRenderBuffer[bufferIndex].GetActiveBuffer());
            Graphics.Blit(null, m_source1RenderBuffer[bufferIndex].GetActiveBuffer(), m_terrainTileMaterial, m_passWorldNormalBlend);
            Profiler.EndSample();

            if (StormVTDecalManager.Instance.m_canDrawDecals)
            {
                StormVTDecalManager.Instance.Render(bufferIndex, centerPos, worldSizeWithPadding, m_source0RenderBuffer[bufferIndex].GetActiveBuffer());
                m_terrainTileMaterial.SetTexture(StormVTShaderIDs.m_stormVT_TileCaptureBlend_TileSource0, m_source0RenderBuffer[bufferIndex].GetActiveBuffer());
                m_terrainTileMaterial.SetTexture(StormVTShaderIDs.m_stormVT_TileCaptureBlend_TileSource1, m_source1RenderBuffer[bufferIndex].GetActiveBuffer());

                //Swap Buffers for output
                m_source0RenderBuffer[bufferIndex].SwapBuffers();
                m_source1RenderBuffer[bufferIndex].SwapBuffers();

                m_terrainTileMaterial.SetTexture(StormVTShaderIDs.m_stormVT_DecalBlend_Source0, StormVTDecalManager.Instance.m_source0RT[bufferIndex]);
                m_terrainTileMaterial.SetTexture(StormVTShaderIDs.m_stormVT_DecalBlend_Source1, StormVTDecalManager.Instance.m_source1RT[bufferIndex]);
                m_terrainTileMaterial.SetTexture(StormVTShaderIDs.m_stormVT_DecalBlend_Source2, StormVTDecalManager.Instance.m_source2RT[bufferIndex]);

                //Set MRT buffers
                m_tileDataMRTBuffer[bufferIndex].SetSourceBuffers(m_source0RenderBuffer[bufferIndex].GetActiveBuffer(), m_source1RenderBuffer[bufferIndex].GetActiveBuffer());

                Graphics.SetRenderTarget(m_tileDataMRTBuffer[bufferIndex].m_colorDataMRTBuffer, m_source0RenderBuffer[bufferIndex].GetActiveBuffer().depthBuffer);
                Graphics.Blit(null, m_terrainTileMaterial, m_passDecalBlend);

            }

            Profiler.EndSample();

        }

        /// <summary>
        /// Grabs data from terrain or texture loader, then creates normal, holes and heightmap textures
        /// </summary>
        /// <param name="bufferIndex">MaxRequestsPerFrame buffer index</param>
        /// <param name="foundTerrainInfo">Whether or not we have retrieved any terrainInfo component relating to this terrain</param>
        /// <param name="terrainInfo">The terrain info component</param>
        /// <param name="terrainTextureData">The texture data object</param>
        public void GenerateHeightAndNormalMapsPerTerrain(int bufferIndex, bool foundTerrainInfo, StormVTTerrainInfo terrainInfo, StormVTTerrainTextureData terrainTextureData)
        {
            Profiler.BeginSample("Storm Terrain - Tile Generation - Height and Normal Blit");


            Profiler.BeginSample("Storm Terrain - Tile Generation - Height and Normal Blit Set Textures");
            //Grab height texture
            if (stormVTSettings.m_stormVTMode == StormVTMode.UnityTerrain)
            {
                if (foundTerrainInfo)
                {
                    m_terrainTileMaterial.SetTexture(StormVTShaderIDs.m_stormVT_TileCopy_TerrainHeightMap, terrainInfo.heightmap);
                    if (terrainInfo.heightmap == null)
                    {
                        if(terrainInfo.terrain != null)
                        {
                            terrainInfo.heightmap = terrainInfo.terrain.terrainData.heightmapTexture;
                        }
                    }
                    if(terrainInfo.terrain != null)
                        m_terrainTileMaterial.SetFloat(StormVTShaderIDs.m_stormVT_TileCopy_TerrainHeightScale, terrainInfo.terrain.terrainData.heightmapScale.y);
                }
            }

            else
            {
                //Grab / Load heightmap from elsewhere
                if (terrainTextureData != null)
                {
                    m_terrainTileMaterial.SetTexture(StormVTShaderIDs.m_stormVT_TileCopy_TerrainHeightMap, terrainTextureData.heightmap);
                    if (terrainTextureData.lowRes)
                    {
                        m_terrainTileMaterial.SetVector(StormVTShaderIDs.m_stormVT_TerrainTileGen_TerrainHeightmapRecipSize, m_terrainRecipSizeLowRes);
                    }
                    else
                    {
                        m_terrainTileMaterial.SetVector(StormVTShaderIDs.m_stormVT_TerrainTileGen_TerrainHeightmapRecipSize, m_terrainRecipSize);
                    }
                    m_terrainTileMaterial.SetFloat(StormVTShaderIDs.m_stormVT_TileCopy_TerrainHeightScale, terrainTextureData.heightScale);
                }
                else
                {
                    m_terrainTileMaterial.SetTexture(StormVTShaderIDs.m_stormVT_TileCopy_TerrainHeightMap, null);
                }

            }

            Profiler.EndSample();

            Profiler.BeginSample("Storm Terrain - Tile Generation - Height and Normal Blit Set Textures");

            //Set terrain normalmap to blit into tile
            if (stormVTSettings.m_stormVTMode == StormVTMode.UnityTerrain)
            {
                if (foundTerrainInfo)
                {
                    m_terrainTileMaterial.SetTexture(StormVTShaderIDs.m_stormVT_TileCopy_TerrainWorldNormalMap, terrainInfo.normalmap);
                }
            }
            else
            {
                //Grab / Load normalmap from elsewhere
                if (terrainTextureData != null)
                {
                    m_terrainTileMaterial.SetTexture(StormVTShaderIDs.m_stormVT_TileCopy_TerrainWorldNormalMap, terrainTextureData.normalmap);
                }
                else
                {
                    m_terrainTileMaterial.SetTexture(StormVTShaderIDs.m_stormVT_TileCopy_TerrainWorldNormalMap, null);
                }
            }

            Profiler.EndSample();

            Profiler.BeginSample("Storm Terrain - Tile Generation - Height and Normal Blit Set Textures");
            //Set terrain holes texture to blit into tile
            if (stormVTSettings.m_stormVTMode == StormVTMode.UnityTerrain)
            {
                if (foundTerrainInfo)
                {
                    m_terrainTileMaterial.SetTexture(StormVTShaderIDs.m_stormVT_TileCopy_TerrainHolesMap, terrainInfo.holesmap);
                }
            }
            else
            {
                //Grab / Load holesmap from elsewhere
                if (terrainTextureData != null && terrainTextureData.holesmap != null)
                {
                    m_terrainTileMaterial.SetTexture(StormVTShaderIDs.m_stormVT_TileCopy_TerrainHolesMap, terrainTextureData.holesmap);
                }
                else
                {
                    m_terrainTileMaterial.SetTexture(StormVTShaderIDs.m_stormVT_TileCopy_TerrainHolesMap, m_blankWhiteTex);
                }
            }

            Profiler.EndSample();

            Profiler.BeginSample("Storm Terrain - Tile Generation - Height and Normal Swap Buffers");

            //Set previous tile accumulation textures
            m_terrainTileMaterial.SetTexture(StormVTShaderIDs.m_stormVT_TileCopy_TileHeightMap, m_heightRenderBuffer[bufferIndex].GetActiveBuffer());
            m_terrainTileMaterial.SetTexture(StormVTShaderIDs.m_stormVT_TileCopy_TileWorldNormalMap, m_normalRenderBuffer[bufferIndex].GetActiveBuffer());

            m_heightRenderBuffer[bufferIndex].SwapBuffers();
            m_normalRenderBuffer[bufferIndex].SwapBuffers();

            //Set current buffers to write to
            m_tileDataMRTBuffer[bufferIndex].SetTerrainBuffers(
                m_heightRenderBuffer[bufferIndex].GetActiveBuffer(),
                m_normalRenderBuffer[bufferIndex].GetActiveBuffer());

            Profiler.EndSample();

            Profiler.BeginSample("Storm Terrain - Tile Generation - Height and Normal Blit BLIT CALL");
            //Set the render textures as the target
            Graphics.SetRenderTarget(m_tileDataMRTBuffer[bufferIndex].m_terrainDataMRTBuffer, m_heightRenderBuffer[bufferIndex].GetActiveBuffer().depthBuffer);
            //Blit data into the buffers
            Graphics.Blit(null, m_terrainTileMaterial, m_passHeightNormalHoles);
            Profiler.EndSample();


            Profiler.EndSample();
        }

        /// <summary>
        /// Grabs data from terrain or texture loader, then creates Color Map data textures
        /// </summary>
        /// <param name="bufferIndex">MaxRequestsPerFrame buffer index</param>
        /// <param name="foundTerrainInfo">Whether or not we have retrieved any terrainInfo component relating to this terrain</param>
        /// <param name="terrainInfo">The terrain info component</param>
        /// <param name="terrainTextureData">The texture data object</param>
        public void GenerateColorMapsPerTerrain(int bufferIndex, bool foundTerrainInfo, StormVTTerrainInfo terrainInfo, StormVTTerrainTextureData terrainTextureData)
        {
            if(stormVTSettings.m_grassMapColor || stormVTSettings.m_treeMapColor || stormVTSettings.m_grassMapNormal || stormVTSettings.m_treeMapNormal)
            {
                Profiler.BeginSample("Storm Terrain - Tile Generation - DistanceMap Set Textures");

                //Set terrain normalmap to blit into tile
                if (stormVTSettings.m_stormVTMode == StormVTMode.UnityTerrain)
                {
                    if (foundTerrainInfo)
                    {
                        m_terrainTileMaterial.SetTexture(StormVTShaderIDs.m_stormVT_TileCopy_TerrainGrassMapColor, terrainInfo.grassMapColor);
                        m_terrainTileMaterial.SetTexture(StormVTShaderIDs.m_stormVT_TileCopy_TerrainTreeMapColor, terrainInfo.treeMapColor);
                        m_terrainTileMaterial.SetTexture(StormVTShaderIDs.m_stormVT_TileCopy_TerrainGrassMapNormal, terrainInfo.grassMapNormal);
                        m_terrainTileMaterial.SetTexture(StormVTShaderIDs.m_stormVT_TileCopy_TerrainTreeMapNormal, terrainInfo.treeMapNormal);
                    }
                }
                else
                {

                }

                Profiler.EndSample();

                m_terrainTileMaterial.SetTexture(StormVTShaderIDs.m_stormVT_TileCopy_TileGrassMapColor, m_grassMapColorBuffer[bufferIndex].GetActiveBuffer());
                m_terrainTileMaterial.SetTexture(StormVTShaderIDs.m_stormVT_TileCopy_TileTreeMapColor, m_treeMapColorBuffer[bufferIndex].GetActiveBuffer());
                m_terrainTileMaterial.SetTexture(StormVTShaderIDs.m_stormVT_TileCopy_TileDistanceMapNormal, m_distanceMapNormalBuffer[bufferIndex].GetActiveBuffer());
                m_grassMapColorBuffer[bufferIndex].SwapBuffers();
                m_treeMapColorBuffer[bufferIndex].SwapBuffers();
                m_distanceMapNormalBuffer[bufferIndex].SwapBuffers();

                m_tileDataMRTBuffer[bufferIndex].SetDistanceMapBuffer(
                    m_grassMapColorBuffer[bufferIndex].GetActiveBuffer(),
                    m_treeMapColorBuffer[bufferIndex].GetActiveBuffer(),
                    m_distanceMapNormalBuffer[bufferIndex].GetActiveBuffer()
                );

                Profiler.BeginSample("Storm Terrain - Tile Generation - DistanceMap Blit");
                Graphics.SetRenderTarget(m_tileDataMRTBuffer[bufferIndex].m_distanceMapMRTBuffer, m_distanceMapColorBuffer[bufferIndex].GetActiveBuffer().depthBuffer);
                //Blit data into the buffers
                Graphics.Blit(null, m_terrainTileMaterial, m_passDistanceMap);
                Profiler.EndSample();
            }
        }

        /// <summary>
        /// Sets the terrain layer data for a single pass, which uses 4 terrain layers max per pass
        /// </summary>
        /// <param name="curTerrain">The current terrain we are drawing on</param>
        /// <param name="offset">The pass index offset, i.e. pass 0 = 0, pass 1 = 4, pass 2 = 8</param>
        /// <param name="heightBlend">Whether or not to perform a height blend</param>
        public void SetPassTerrianLayerData(StormVTTerrainGridData curTerrain, int offset, bool heightBlend = false)
        {
            //Loop through current set of layers
            for (int i = 0; i < 4; i++)
            {

                //Determine index into the terrain alyers
                int layerIndex = offset + i;

                //Safety check so we won't go out of bounds
                if (layerIndex < curTerrain.terrainLayers.Count && layerIndex < curTerrain.terrainLayerIndices.Count())
                {
                    //Grab a reference to the terrain layer
                    StormVTTerrainTextureLayer stormVTTerrainTextureLayer = stormVTTerrainTextureLayers.stormVTTerrainTextureLayers[curTerrain.terrainLayerIndices[layerIndex]];

                    Profiler.BeginSample("Storm Terrain - Tile Generation - Collect Pass Data - Set TerrainTileLayer");

                    m_terrainTileLayers[i] = stormVTTerrainTextureLayer.m_terrainTileLayer;

                    Profiler.EndSample();

                    Profiler.BeginSample("Storm Terrain - Tile Generation - Collect Pass Data - Set Layer Textures");



                    if (stormVTTerrainTextureLayer != null)
                    {
                        //Assign terrain layer textures to the blit material
                        if (stormVTTerrainTextureLayer.m_packedDiffuseMap != null)
                        {
                            m_terrainTileMaterial.SetTexture(StormVTShaderIDs.m_stormVT_TerrainTileGen_DiffuseTextures[i], stormVTTerrainTextureLayer.m_packedDiffuseMap);
                        }

                        if(!heightBlend)
                        {
                            if (stormVTTerrainTextureLayer.m_packedNormalMap != null)
                            {
                                m_terrainTileMaterial.SetTexture(StormVTShaderIDs.m_stormVT_TerrainTileGen_NormalTextures[i], stormVTTerrainTextureLayer.m_packedNormalMap);
                            }
                        }


                    }


                    Profiler.EndSample();
                }
            }
        }

        /// <summary>
        /// Checks if the buffer reference on the material is null or has been lost between last tile generation.
        /// This often occurs if the screen was clicked off for a second, or a shader / script was recompiled.
        /// If so, this function re-assigns the buffer data
        /// </summary>
        public void BufferCheck()
        {
            Profiler.BeginSample("Storm Terrain - Tile Generation - Collect Pass Data - Has Buffer Check");
            //Check if we have the buffer assigned
            bool hasBuffer = m_terrainTileMaterial.HasBuffer(StormVTShaderIDs.m_stormVT_TerrainTileGen_terrainLayersBuffer);
            Profiler.EndSample();

            //If we don't have the buffer assigned, then set it on the terrain tile material
            if (!hasBuffer)
            {
                m_terrainTileMaterial.SetBuffer(StormVTShaderIDs.m_stormVT_TerrainTileGen_terrainLayersBuffer, m_terrainLayerBuffer);
            }
        }

        /// <summary>
        /// Sets the snow variable data on the terrain tile material, for use with the 'Tile' mode for generating snow
        /// </summary>
        public void SetSnowVariables()
        {
            Profiler.BeginSample("Storm Terrain - Tile Generation - Set Snow Variables");
            if (m_terrainTileMaterial != null)
            {
                if(stormVTSettings.m_SnowGenerationMode == StormVTFeatureMode._Tile)
                {
                    SetKeyword(m_terrainTileMaterial, StormVTShaderIDs.m_stormVT_SnowTerrain, StormVTShaderIDs.m_stormVT_SnowTerrainOn, stormVTSettings.m_Snow);

                    m_terrainTileMaterial.SetFloat(StormVTShaderIDs.m_stormVT_SnowPass_HeightScale, stormVTTerrainGrid.m_terrainHeightScale * 2);
                    m_terrainTileMaterial.SetFloat(StormVTShaderIDs.m_stormVT_SnowPass_HeightOffset, stormVTTerrainGrid.m_terrainHeightOffset);

                    m_terrainTileMaterial.SetFloat(StormVTShaderIDs.m_stormVT_SnowPass_SnowHeight, stormVTSettings.m_SnowHeight);
                    m_terrainTileMaterial.SetFloat(StormVTShaderIDs.m_stormVT_SnowPass_SnowFalloff, stormVTSettings.m_SnowFalloff);
                    m_terrainTileMaterial.SetFloat(StormVTShaderIDs.m_stormVT_SnowPass_SnowCurvatureInfluence, stormVTSettings.m_SnowCurvatureInfluence);
                    m_terrainTileMaterial.SetVector(StormVTShaderIDs.m_stormVT_SnowPass_SnowColor, stormVTSettings.m_SnowColor);
                    m_terrainTileMaterial.SetFloat(StormVTShaderIDs.m_stormVT_SnowPass_SnowUVScale, stormVTSettings.m_SnowUVScale);
                    m_terrainTileMaterial.SetTexture(StormVTShaderIDs.m_stormVT_SnowPass_SnowAlbedo, stormVTSettings.m_SnowAlbedo);
                    m_terrainTileMaterial.SetTexture(StormVTShaderIDs.m_stormVT_SnowPass_SnowNormal, stormVTSettings.m_SnowNormal);
                    m_terrainTileMaterial.SetTexture(StormVTShaderIDs.m_stormVT_SnowPass_SnowMask, stormVTSettings.m_SnowMask);
                }
                else
                {
                    SetKeyword(m_terrainTileMaterial, StormVTShaderIDs.m_stormVT_SnowTerrain, StormVTShaderIDs.m_stormVT_SnowTerrainOn, false);
                }
                
            }

            Profiler.EndSample();

        }

        /// <summary>
        /// Sets a keyword on the material to the passed in boolean state
        /// </summary>
        /// <param name="mat">Material to set the keyword on</param>
        /// <param name="keywordID">The integar value of the keyword reference</param>
        /// <param name="keywordName">The string value of the keyword reference</param>
        /// <param name="enabled">The state of which the keyword should be</param>
        private void SetKeyword(Material mat, int keywordID, string keywordName, bool enabled)
        {

            mat.SetInt(keywordID, enabled ? 1 : 0);
            if (enabled)
                mat.EnableKeyword(keywordName);
            else
                mat.DisableKeyword(keywordName);
        }

        

    }
}
