using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Rendering;
using Unity.Collections;
using UnityEditor;



#if PW_STORM_HDRP
using UnityEngine.Rendering.HighDefinition;
#endif

namespace ProceduralWorlds.Storm.Terrain
{
    public class StormVTFeedbackManager : MonoBehaviour
    {

        #region Public Variables

        /// <summary>
        /// Reference to the storm terrain settings
        /// </summary>
        public StormVTSettings stormVTSettings;

        /// <summary>
        /// Reference to the terrain grid settings
        /// </summary>
        public StormVTTerrainGrid stormVTTerrainGrid;

        /// <summary>
        /// Reference to the physical texture manager component
        /// </summary>
        public StormVTPhysicalTextureManager physicalTextureManager;

        /// <summary>
        /// Reference to the feedback clipmap component
        /// </summary>
        public StormVTFeedbackClipmap stormVTClipmap;

        /// <summary>
        /// Reference to the custom pass feedback renderers
        /// </summary>
        [HideInInspector]
        public StormVTFeedbackRendererCustomPass feedbackRendererCustomPass;

        /// <summary>
        /// Reference to the compute shader to use for reading back the render texture data
        /// </summary>
        [HideInInspector]
        public ComputeShader m_readbackComputeShader;

        /// <summary>
        /// Reference to the material to use to blit the clipmap into the feedback / terrain ID textures
        /// </summary>
        [HideInInspector]
        public Material m_feedbackRendererMatClipmap;

        /// <summary>
        /// A hashset to store unique Tile IDs when read back from the compute shader
        /// </summary>
        [HideInInspector]
        public HashSet<int4> tileIDs;

        /// <summary>
        /// The number of terrains used in a scene's setup
        /// </summary>
        [HideInInspector]
        public int m_numGrids;

        /// <summary>
        /// The bottom left position of the terrain
        /// </summary>
        [HideInInspector]
        public Vector2 m_terrainPivot;

        /// <summary>
        /// Reference to the main camera currently in use, used for position tracking
        /// </summary>
        public Camera m_mainCamera;

        /// <summary>
        /// Reference to the camera to use for rendering the clipmap into
        /// The camera's rendering ability is disabled at runtime, it is just used for
        /// gathering the view and projection matrices for the command buffer
        /// </summary>
        public Camera m_feedbackCamera;

        /// <summary>
        /// The snapped position of the camera, based on the terrain total scale
        /// </summary>
        [HideInInspector]
        public Vector2 snappedCameraPos;

        /// <summary>
        /// Up close scale of high definition tiles to readback
        /// </summary>
        [HideInInspector]
        public float terrainScale = 16f;

        /// <summary>
        /// Total size of terrain used in scene
        /// </summary>
        [HideInInspector]
        public float terrainTotalScale;

        /// <summary>
        /// Bottom left position of the terrain based off the total terrain scale
        /// </summary>
        [HideInInspector]
        public Vector2 bottomLeftTerrainScalePos;

        /// <summary>
        /// Keeps track of the current speed the camera is moving, in normalized ranges (0-1)
        /// </summary>
        [HideInInspector]
        private float currentSpeedNormalized;

        /// <summary>
        /// Keeps track of the previous speed the camera is moving, in normalized ranges (0-1)
        /// </summary>
        [HideInInspector]
        private float previousSpeedNormalized;

        /// <summary>
        /// The total size of the terrain setup
        /// </summary>
        [HideInInspector]
        public float m_totalGridSize;

        #endregion

        #region Private Variables

        /// <summary>
        /// A reference to the current async request that has been launched. 
        /// Used to prevent multiple requests being launched at the same time
        /// </summary>
        private AsyncGPUReadbackRequest? m_currentRequest;

        /// <summary>
        /// Reference to the shader property ID of the number of terrains used in a scene
        /// </summary>
        private static readonly int m_numTerrainsID = Shader.PropertyToID("_NumTerrains");

        /// <summary>
        /// Reference to the shader property ID of the terrain bottom left position
        /// </summary>
        private static readonly int m_bottomLeftTerrainPosID = Shader.PropertyToID("_TerrainBottomLeftPos");

        /// <summary>
        /// Reference to the shader property ID of the total size of the terrain setup
        /// </summary>
        private static readonly int m_boundsSizeID = Shader.PropertyToID("_BoundsSize");

        /// <summary>
        /// Reference to the shader property ID of Readback render texture
        /// </summary>
        static readonly int m_readbackTextureID = Shader.PropertyToID("_ReadbackTexture");

        /// <summary>
        /// Reference to the shader property ID of Terrain ID render texture
        /// </summary>
        static readonly int m_terrainTextureID = Shader.PropertyToID("_TerrainIDTexture");

        /// <summary>
        /// Constant value of the terrain scale. Set at 4096, so that each terrain ID works in chunks of this size.
        /// This is independant to the actual size of the terrain
        /// </summary>
        static readonly float terrainIDScale = 4096f;

        /// <summary>
        /// Width of the full resolution feedback / terrain id texture
        /// </summary>
        private int m_feedbackWidth;

        /// <summary>
        /// Height of the full resolution feedback / terrain id texture
        /// </summary>
        private int m_feedbackHeight;

        /// <summary>
        /// Width of the lower resolution feedback / terrain id texture
        /// </summary>
        private int m_readbackWidth;

        /// <summary>
        /// Height of the lower resolution feedback / terrain id texture
        /// </summary>
        private int m_readbackHeight;

        /// <summary>
        /// Native array of float4 values to store the read back data from the GPU
        /// </summary>
        private NativeArray<float4> m_readbackArray;

        /// <summary>
        /// Reference to the kernal index on the readback compute shader
        /// </summary>
        private int m_readbackKernelID;

        /// <summary>
        /// The compute buffer to store the readback tile IDs
        /// </summary>
        private ComputeBuffer m_tileBuffer;

        /// <summary>
        /// The maximuum size of the readback pixels (readbackWidth * readbackHeight)
        /// </summary>
        private int m_pixelCapacity;

        /// <summary>
        /// The last known position stored since the last update
        /// </summary>
        private Vector3 lastPosition;

        /// <summary>
        /// The last known rotation stored since the last update
        /// </summary>
        private Quaternion lastRotation;

        /// <summary>
        /// A constant reference to the maximuum zoom level that storm terrain supports, which is 23
        /// </summary>
        private const int MAX_LEVEL = 23;

        /// <summary>
        /// timer to keep track of frames used
        /// </summary>
        private float m_feedbackTimer = 0f;
        /// <summary>
        /// Whether or not we have finished the initial feedback request
        /// </summary>
        private bool m_initialFeedbackRequested = false;

        /// <summary>
        /// Delay used on start of the update to perform an initial feedback request
        /// </summary>
        private const float INITIAL_DELAY = 1f;

        /// <summary>
        /// The minmum mip bias to use when the camera is moving slowly
        /// </summary>
        private float minMipBias = 7;

        /// <summary>
        /// The maximum mip bias to use when the camera is moving fast
        /// </summary>
        private float maxMipBias = 7;

        #endregion





        /// <summary>
        /// Assign references to the custom pass
        /// </summary>
        private void OnEnable()
        {
            if(feedbackRendererCustomPass == null)
            {
                feedbackRendererCustomPass = gameObject.GetComponent<StormVTFeedbackRendererCustomPass>();
                if(feedbackRendererCustomPass == null)
                {
                    feedbackRendererCustomPass = gameObject.AddComponent<StormVTFeedbackRendererCustomPass>();
                }
                feedbackRendererCustomPass.stormVTTerrainGrid = stormVTTerrainGrid;
                feedbackRendererCustomPass.stormVTClipmap = stormVTClipmap;
                feedbackRendererCustomPass.m_feedbackRendererMatClipmap = m_feedbackRendererMatClipmap;
                feedbackRendererCustomPass.m_feedbackCamera = m_feedbackCamera;
                feedbackRendererCustomPass.ReInitialize();
            }
            else
            {
                feedbackRendererCustomPass.stormVTTerrainGrid = stormVTTerrainGrid;
                feedbackRendererCustomPass.stormVTClipmap = stormVTClipmap;
                feedbackRendererCustomPass.m_feedbackRendererMatClipmap = m_feedbackRendererMatClipmap;
                feedbackRendererCustomPass.m_feedbackCamera = m_feedbackCamera;
                feedbackRendererCustomPass.ReInitialize();
            }
            Initialize();
        }


        // Start is called before the first frame update
        void Start()
        {
            Initialize();

            StormVTFeedbackRendererCustomPass.OnFeedbackTexturesCreated += OnFeedbackTexturesCreated;
            StormVTManager.OnUpdateStormVTCamera += OnStormVTCamera;
        }

        //Reintialize when the main camera reference changes
        private void OnStormVTCamera(Camera camera)
        {
            Initialize();
        }

        /// <summary>
        /// Set the render textures on the compute shader once we know that they have been created
        /// </summary>
        public void OnFeedbackTexturesCreated()
        {
            m_readbackComputeShader.SetTexture(m_readbackKernelID, m_readbackTextureID, feedbackRendererCustomPass.readbackRenderTexture);
            m_readbackComputeShader.SetTexture(m_readbackKernelID, m_terrainTextureID, feedbackRendererCustomPass.terrainIDRenderTexture);
        }

        /// <summary>
        /// Calculate the tile id position used to store terrain grid data for proper tile id readback 
        /// </summary>
        public void CalculateTileIDPosition()
        {
            Profiler.BeginSample("Storm Terrain - Feedback Renderer - Calculate Tile ID Position");
            
            int numGrids = math.max(stormVTTerrainGrid.m_numTerrainsX, stormVTTerrainGrid.m_numTerrainsY);

            Vector2 bottomLeftPos = stormVTTerrainGrid.m_terrainBottomLeftPosition;
            int2 coord = new int2(
                (int)Mathf.Floor(bottomLeftPos.x / terrainIDScale),
                (int)Mathf.Floor(bottomLeftPos.y / terrainIDScale));

            m_terrainPivot = new Vector2(coord.x * terrainIDScale, coord.y * terrainIDScale);

            float totalTerrainSize = numGrids * stormVTTerrainGrid.m_terrainSize;
            int numTerrainGridScale =  (int)Mathf.Ceil(totalTerrainSize / terrainIDScale);


            m_totalGridSize = numTerrainGridScale * terrainIDScale;

            m_numGrids = numTerrainGridScale;

            feedbackRendererCustomPass.m_feedbackRendererMatClipmap.SetFloat(m_numTerrainsID, m_numGrids);
            feedbackRendererCustomPass.m_feedbackRendererMatClipmap.SetFloat(m_boundsSizeID, m_totalGridSize);
            feedbackRendererCustomPass.m_feedbackRendererMatClipmap.SetVector(m_bottomLeftTerrainPosID, m_terrainPivot);

            Profiler.EndSample();
        }

        /// <summary>
        /// Initialize the variables needed for the manager to run
        /// </summary>
        private void Initialize()
        {
            terrainTotalScale = terrainScale * 256f;

            m_feedbackWidth = 1920 / (int)stormVTSettings.m_feedbackResolution;
            m_feedbackHeight = 1080 / (int)stormVTSettings.m_feedbackResolution;
            m_readbackWidth = m_feedbackWidth / (int)stormVTSettings.m_readbackResolution;
            m_readbackHeight = m_feedbackHeight / (int)stormVTSettings.m_readbackResolution;
            feedbackRendererCustomPass.feedbackWidth = m_feedbackWidth;
            feedbackRendererCustomPass.feedbackHeight = m_feedbackHeight;
            feedbackRendererCustomPass.readbackWidth = m_readbackWidth;
            feedbackRendererCustomPass.readbackHeight = m_readbackHeight;
            feedbackRendererCustomPass.ReInitialize();

            CalculateTileIDPosition();

            m_readbackKernelID = m_readbackComputeShader.FindKernel("CSMain");
            m_pixelCapacity = feedbackRendererCustomPass.readbackWidth * feedbackRendererCustomPass.readbackHeight;

            feedbackRendererCustomPass.m_feedbackRendererMatClipmap = m_feedbackRendererMatClipmap;
            feedbackRendererCustomPass.m_feedbackCamera = m_feedbackCamera;

            m_readbackComputeShader.SetInt("size", m_pixelCapacity);
            m_readbackComputeShader.SetInt("width", feedbackRendererCustomPass.readbackWidth);
            m_readbackComputeShader.SetInt("height", feedbackRendererCustomPass.readbackHeight);
            m_readbackComputeShader.SetVector("terrainBottomLeftPos", m_terrainPivot);
            m_readbackComputeShader.SetInts("_TextureSize", new int[] { feedbackRendererCustomPass.readbackWidth, feedbackRendererCustomPass.readbackHeight });
            
            
            if(m_tileBuffer != null)
            {
                m_tileBuffer.Dispose();
                m_tileBuffer = null;
            }
            m_tileBuffer = new ComputeBuffer(m_pixelCapacity, sizeof(int) * 4, ComputeBufferType.Structured);
            m_readbackComputeShader.SetBuffer(m_readbackKernelID, "_AppendBuffer", m_tileBuffer);

            tileIDs = new HashSet<int4>(m_pixelCapacity);

            if (m_readbackArray.IsCreated)
            {
                m_readbackArray.Dispose();
            }
            m_readbackArray = new NativeArray<float4>(m_pixelCapacity, Allocator.Persistent);

            feedbackRendererCustomPass.m_feedbackCamera = m_feedbackCamera;

            m_feedbackTimer = 0f;
            m_initialFeedbackRequested = false;

            minMipBias = stormVTSettings.m_mipBias;
            maxMipBias = stormVTSettings.m_mipBiasWhenMoving;
            previousSpeedNormalized = 0;
        }

        /// <summary>
        /// Cleanup memory variables, unassign events
        /// </summary>
        private void OnDestroy()
        {
            if(m_tileBuffer != null)
            {
                m_tileBuffer.Dispose();
                m_tileBuffer = null;
            }

            if (m_readbackArray.IsCreated)
            {
                m_readbackArray.Dispose();
            }
            StormVTFeedbackRendererCustomPass.OnFeedbackTexturesCreated -= OnFeedbackTexturesCreated;
            StormVTManager.OnUpdateStormVTCamera -= OnStormVTCamera;

            stormVTSettings.m_mipBias = minMipBias;
            #if UNITY_EDITOR
            EditorUtility.SetDirty(stormVTSettings);
            #endif
        }

        /// <summary>
        /// Cleanup memory variables
        /// </summary>
        private void OnDisable()
        {
            if(m_tileBuffer != null)
            {
                m_tileBuffer.Dispose();
                m_tileBuffer = null;
            }
            if (m_readbackArray.IsCreated)
            {
                m_readbackArray.Dispose();
            }
        }

       
        /// <summary>
        /// If necessary conditions are met, perform a request of the feedback tiles when the camera has changed
        /// </summary>
        private void Update()
        {
            if (!m_initialFeedbackRequested)
            {
                m_feedbackTimer += Time.deltaTime;
                if (m_feedbackTimer >= INITIAL_DELAY)
                {
                    m_initialFeedbackRequested = true;
                    RequestFeedback();
                    m_feedbackTimer = 0f;
                }
                return;
            }

            m_feedbackTimer += Time.deltaTime;
            if (m_feedbackTimer >= stormVTSettings.m_readbackDelay)
            {

                // Calculate velocity
                Vector3 velocity = (m_mainCamera.transform.position - lastPosition) / Time.deltaTime;
                float speed = velocity.magnitude;
                
                // Normalize to 0-1 range
                currentSpeedNormalized = Mathf.Clamp01(speed / stormVTSettings.m_maxMoveSpeed);

                stormVTSettings.m_mipBias = Mathf.Lerp(minMipBias, maxMipBias, currentSpeedNormalized);
                stormVTSettings.UpdateMipBias();

                //If the velocity has changed, request the feedback tiles
                if(currentSpeedNormalized != previousSpeedNormalized)
                {
                    RequestFeedback();
                    previousSpeedNormalized = currentSpeedNormalized;
                }
                else
                {
                    //If the velocity is the same, but the camera has moved or rotated, then request the feedback
                    if (m_mainCamera.transform.position != lastPosition || m_mainCamera.transform.rotation != lastRotation)
                    {
                        RequestFeedback();
                    }
                }



                m_feedbackTimer = 0f;
            }
        }


        /// <summary>
        /// Launches an asynchronous request to gather feedback tiles
        /// </summary>
        public void RequestFeedback()
        {
            if (m_currentRequest.HasValue && !m_currentRequest.Value.done)
            {
                return; // Skip if previous request is still pending
            }

            if (!stormVTSettings.m_stormTerrainActive || !stormVTClipmap.m_clipmapInitialized || m_tileBuffer == null)
            {
                return;
            }

            lastPosition = m_mainCamera.transform.position;
            lastRotation = m_mainCamera.transform.rotation;

            m_feedbackCamera.fieldOfView = m_mainCamera.fieldOfView;
            m_feedbackCamera.transform.position = lastPosition - (m_mainCamera.transform.forward * stormVTSettings.m_feedbackPush);
            m_feedbackCamera.transform.rotation = lastRotation;
            m_feedbackCamera.farClipPlane = m_mainCamera.farClipPlane;

            snappedCameraPos = new Vector2(
                MathF.Floor(lastPosition.x / terrainScale) * terrainScale,
                MathF.Floor(lastPosition.z / terrainScale) * terrainScale);

            bottomLeftTerrainScalePos = snappedCameraPos - new Vector2(terrainTotalScale * 0.5f, terrainTotalScale * 0.5f);

            m_feedbackRendererMatClipmap.SetFloat(m_numTerrainsID, 256);
            m_feedbackRendererMatClipmap.SetFloat(m_boundsSizeID, terrainTotalScale);
            m_feedbackRendererMatClipmap.SetVector(m_bottomLeftTerrainPosID, bottomLeftTerrainScalePos);

            m_readbackComputeShader.SetVector(m_bottomLeftTerrainPosID, bottomLeftTerrainScalePos);
            m_readbackComputeShader.SetTexture(m_readbackKernelID, m_terrainTextureID, feedbackRendererCustomPass.terrainIDRenderTexture);

            if (feedbackRendererCustomPass != null)
            {
                if (feedbackRendererCustomPass.readbackRenderTexture != null && feedbackRendererCustomPass.feedbackRenderTexture != null)
                {
                    m_readbackComputeShader.SetTexture(m_readbackKernelID, m_readbackTextureID, feedbackRendererCustomPass.readbackRenderTexture);
                    m_readbackComputeShader.SetTexture(m_readbackKernelID, m_terrainTextureID, feedbackRendererCustomPass.readbackTerrainIDRenderTexture);
                    feedbackRendererCustomPass.ManualRender();

                    //Perform the compute shader tile id calculations
                    Profiler.BeginSample("Storm Terrain - Feedback Renderer - Dispatch");
                    m_readbackComputeShader.Dispatch(m_readbackKernelID, m_feedbackWidth / 8, m_feedbackHeight / 8, 1);
                    Profiler.EndSample();

                    //Launch the async request
                    Profiler.BeginSample("Storm Terrain - Feedback Renderer - Launch Async Request");
                    m_currentRequest = AsyncGPUReadback.RequestIntoNativeArray(ref m_readbackArray, m_tileBuffer, OnReadbackComplete);
                    Profiler.EndSample();
                }
            }
        }

        /// <summary>
        /// Once the readback is completed, load tiles into the hashset, and update the pending requests
        /// </summary>
        /// <param name="request">The request object that contains the buffer data</param>
        private void OnReadbackComplete(AsyncGPUReadbackRequest request)
        {
            Profiler.BeginSample("Storm Terrain - Feedback Renderer - Readback");
            m_currentRequest = null;

            if (request.hasError)
            {
                return;
            }

            //Copy native array data into m_readbackArray
            var data = request.GetData<float4>();
            NativeArray<float4>.Copy(data, m_readbackArray, data.Length);

            //Clear the previous hashset of tiles
            tileIDs.Clear();
            
            //Add unique tile ids and their parent id into the hashset
            int length = m_readbackArray.Length;
            for (int i = 0; i < length; i++)
            {
                float4 pixel = m_readbackArray[i];
                int z = (int)pixel.z;
                
                if (z != -1 && z < MAX_LEVEL)
                {
                    tileIDs.Add(new int4((int)pixel.x, (int)pixel.y, z, 0));
                    
                    if (z + 1 < MAX_LEVEL)
                    {
                        tileIDs.Add(new int4((int)pixel.x >> 1, (int)pixel.y >> 1, z + 1, 1));
                    }
                }
            }

            //Update the physical texture manager with the new hashset
            physicalTextureManager.OnUpdateFeedbackTileIDs(tileIDs);

            //Tell the manager to add these tiles to the pending requests to generate
            physicalTextureManager.OnPlayerUpdated();
            Profiler.EndSample();
        }
    }

}