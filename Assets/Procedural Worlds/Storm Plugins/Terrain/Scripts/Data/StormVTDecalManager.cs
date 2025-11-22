using System.Collections.Generic;
using UnityEngine.Rendering;
using UnityEngine;
using UnityEngine.Profiling;

namespace ProceduralWorlds.Storm.Terrain
{
    public class StormVTDecalManager : MonoBehaviour
    {
        #region Instance Setup
        /// <summary>
        /// Singleton instance of this manager
        /// </summary>
        private static StormVTDecalManager instance = null;

        /// <summary>
        /// Singleton instance of this manager
        /// </summary>
        public static StormVTDecalManager Instance
        {
            get
            {
                if (instance == null)
                {
                    GameObject managerObject = GameObject.Find("Physical Texture Manager");
                    if (managerObject != null)
                    {
                        StormVTDecalManager decalManager = managerObject.GetComponent<StormVTDecalManager>();
                        if(decalManager != null)
                        {
                            instance = decalManager;
                        }
                    }
                }

                return instance;
            }
        }
        #endregion

        #region Punlic Variables
        /// <summary>
        /// StormVTSettings reference
        /// </summary>
        public StormVTSettings stormVTSettings;

        /// <summary>
        /// Storm Terrain Grid reference
        /// </summary>
        public StormVTTerrainGrid stormVTTerrainGrid;

        /// <summary>
        /// Master shader used to create decal instances
        /// </summary>
        [HideInInspector]
        public Shader m_decalShader;

        /// <summary>
        /// List of all known decal objects currently loaded in the scene, sorted by pivot positions
        /// </summary>
        [HideInInspector]
        public List<StormVTDecal> m_decals = new List<StormVTDecal>();

        /// <summary>
        /// Stores where there are any decals loaded in, thus allowing the drawing code to execute
        /// Used for performance to prevent unneccessary code running if no decals are present
        /// </summary>
        [HideInInspector]
        public bool m_canDrawDecals = false;

        [Header("Render Settings")]
        /// <summary>
        /// Buffer array of render textures for Source0 type (Diffuse.rgb, Alpha). Number of elements is MaxRequestsPerFrame from the StormVTSettings
        /// </summary>
        [HideInInspector]
        public RenderTexture[] m_source0RT;
        /// <summary>
        /// Buffer array of render textures for Source1 type (WorldNormal.xz, Ambient Occlusion, Alpha). Number of elements is MaxRequestsPerFrame from the StormVTSettings
        /// </summary>
        [HideInInspector]
        public RenderTexture[] m_source1RT;

        /// <summary>
        /// Buffer array of render textures for Source2 type (Smoothness, Height, null, Alpha). Number of elements is MaxRequestsPerFrame from the StormVTSettings
        /// </summary>
        [HideInInspector]
        public RenderTexture[] m_source2RT;

        /// <summary>
        /// The maximum height (in world space) of the camera to capture decal elements
        /// </summary>
        [HideInInspector]
        public float maxHeight = 500f;

        /// <summary>
        /// The maximum depth (far plane) for the camera to capture decal elements
        /// </summary>
        [HideInInspector]
        public float projectionDepth = 1000f;

        /// <summary>
        /// Reference to the camera used to calculate matrix information. 
        /// The camera component does not need to be enabled for use in this system.
        /// </summary>
        public Camera m_camera;
        #endregion

        #region Private Variables
        /// <summary>
        /// Reference to the Command Buffer used for building up the list of renderering commands
        /// </summary>
        private CommandBuffer cmd;

        /// <summary>
        /// Used to load property block reference from a decal's material for drawing
        /// </summary>
        private MaterialPropertyBlock propertyBlock;

        /// <summary>
        /// Reference to the shader ID for temporary render texture 1 for the command buffer
        /// </summary>
        private static readonly int tempRT = Shader.PropertyToID("_TempRT");

        /// <summary>
        /// Reference to the shader ID for temporary render texture 2 for the command buffer
        /// </summary>
        private static readonly int tempRT2 = Shader.PropertyToID("_TempRT2");

        /// <summary>
        /// Reference to the shader ID for temporary render texture 3 for the command buffer
        /// </summary>
        private static readonly int tempRT3 = Shader.PropertyToID("_TempRT3");

        /// <summary>
        /// Reference to the array of render targets, used for drawing to 3 different render textures in one Blit call
        /// </summary>
        private RenderTargetIdentifier[] renderTargets;
        #endregion

        #region Events
        /// <summary>
        /// Event system to call when a new terrain has been loaded in the scene
        /// </summary>
        /// <param name="decal">A reference to the decal to be added</param>
        public delegate void NewDecalAdded(StormVTDecal decal);
        public static event NewDecalAdded OnNewDecalAdded;
        #endregion


        /// <summary>
        /// Adds a decal into the managed list, and re-sorts the list based on pivot position
        /// </summary>
        /// <param name="decal"></param>
        public void AddDecal(StormVTDecal decal)
        {
            if(!m_decals.Contains(decal))
            {
                m_decals.Add(decal);
                m_decals.Sort((a, b) => a.m_pivot.CompareTo(b.m_pivot));
                if(OnNewDecalAdded != null)
                    OnNewDecalAdded(decal);
            }
            if(m_decals.Count > 0)
            {
                m_canDrawDecals = true;
            }
            else
            {
                m_canDrawDecals = false;
            }

            
        }

        /// <summary>
        /// Removes a decal from the managed list
        /// </summary>
        /// <param name="decal"></param>
        public void RemoveDecal(StormVTDecal decal)
        {
            if(m_decals.Contains(decal))
            {
                m_decals.Remove(decal);
            }
            if (m_decals.Count > 0)
            {
                m_canDrawDecals = true;
            }
            else
            {
                m_canDrawDecals = false;
            }
        }

        /// <summary>
        /// Create the render textures for the command buffer
        /// </summary>
        private void Start()
        {
            CreateRenderTextures();
        }
        
        /// <summary>
        /// Initialize the command buffer for drawing decals
        /// </summary>
        public void OnEnable()
        {
             InitializeCommandBuffer();
        }

        /// <summary>
        /// Removing the static instance when this object gets destroyed (e.g. scene close, etc.)
        /// </summary>
        private void OnDestroy()
        {

            instance = null;
        }

        /// <summary>
        /// Creates the command buffer and initializes variables used within it
        /// </summary>
        private void InitializeCommandBuffer()
        {
            CleanupCommandBuffer();
            cmd = new CommandBuffer { name = "CustomObjectRenderer" };
            propertyBlock = new MaterialPropertyBlock();
            renderTargets = new RenderTargetIdentifier[3];
        }
        
        /// <summary>
        /// Remove buffers when not used anymore
        /// </summary>
        private void CleanupCommandBuffer()
        {
            if (cmd != null)
            {
                cmd.Release();
                cmd = null;
            }
        }

        /// <summary>
        /// Sets up the render textures based on max requests per frame
        /// </summary>
        private void CreateRenderTextures()
        {
            m_source0RT = new RenderTexture[stormVTSettings.m_maxRequestsPerFrame];
            m_source1RT = new RenderTexture[stormVTSettings.m_maxRequestsPerFrame];
            m_source2RT = new RenderTexture[stormVTSettings.m_maxRequestsPerFrame];
            for (int i = 0;i < stormVTSettings.m_maxRequestsPerFrame; i++)
            {
                m_source0RT[i] = new RenderTexture(stormVTSettings.m_tileSizedPadded, stormVTSettings.m_tileSizedPadded, 16, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
                m_source1RT[i] = new RenderTexture(stormVTSettings.m_tileSizedPadded, stormVTSettings.m_tileSizedPadded, 16, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
                m_source2RT[i] = new RenderTexture(stormVTSettings.m_tileSizedPadded, stormVTSettings.m_tileSizedPadded, 16, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
            }

        }

        Matrix4x4 CreateOrthographicMatrix(float orthoSize, float aspectRatio, float nearClip, float farClip)
        {
            float top = orthoSize;
            float right = orthoSize * aspectRatio;
            
            Matrix4x4 orthoMatrix = Matrix4x4.identity;
            
            // Unity's projection matrix format
            orthoMatrix.m00 = 1f / right;
            orthoMatrix.m11 = 1f / top;
            orthoMatrix.m22 = -2f / (farClip - nearClip);
            orthoMatrix.m23 = -(farClip + nearClip) / (farClip - nearClip);
            
            return orthoMatrix;
        }

        Matrix4x4 CreateOrthographicMatrix(float worldSize, float farClip)
        {
            float right = worldSize * 0.5f; // Half width
            float top = worldSize * 0.5f;   // Half height
            float nearClip = 0f;              // Assume near clip is at 0

            Matrix4x4 orthoMatrix = Matrix4x4.identity;

            // Unity's projection matrix format
            orthoMatrix.m00 = 1f / right;
            orthoMatrix.m11 = 1f / top;
            orthoMatrix.m22 = -2f / (farClip - nearClip);
            orthoMatrix.m23 = -(farClip + nearClip) / (farClip - nearClip);

            return orthoMatrix;
        }

        Matrix4x4 CreateViewMatrix(Vector2 tilePos, float tileSize)
        {
            Matrix4x4 viewMatrix = Matrix4x4.TRS(
                new Vector3(tilePos.x, maxHeight, tilePos.y), // Negative position for view matrix
                Quaternion.Euler(90, 0, 0), // Look down along Y axis
                new Vector3(tileSize, tileSize, tileSize)
            ).inverse; // Inverse for view matrix

            viewMatrix.m21 = -viewMatrix.m21;
            viewMatrix.m23 = -viewMatrix.m23;

            return viewMatrix;
        }
        
        /// <summary>
        /// Loops through the list of decals and draws them into the 3 source render textures
        /// </summary>
        /// <param name="bufferIndex">Index of the MaxRequestsPerFrame to draw into</param>
        /// <param name="tilePos">The position of the tile we are drawing into, in world space</param>
        /// <param name="tileSize">The size of the tile we are drawing into, in world space</param>
        /// <param name="source0">The already created Source0 texture of the tile, 
        /// used for height blending as height is stored in the Alpha channel of this texture</param>
        public void Render(int bufferIndex, Vector2 tilePos, float tileSize, RenderTexture source0 = null)
        { 
            Profiler.BeginSample("Storm Terrain - DrawRenderers - Render");

            //Don't draw if the command buffer is null
            if (cmd == null)
                return;

            Profiler.BeginSample("Storm Terrain - DrawRenderers - Clear RTs");  
            //Recreate the command buffer's commands so that we don't stack old requests
            cmd.Clear();
            
            //Get a reference to the new temporary render textures
            cmd.GetTemporaryRT(tempRT, m_source0RT[bufferIndex].descriptor);
            cmd.GetTemporaryRT(tempRT2, m_source1RT[bufferIndex].descriptor);
            cmd.GetTemporaryRT(tempRT3, m_source2RT[bufferIndex].descriptor);

            Profiler.BeginSample("Storm Terrain - DrawRenderers - New MRT");   
            // Set all render targets
            renderTargets[0] = tempRT;
            renderTargets[1] = tempRT2;
            renderTargets[2] = tempRT3;
            Profiler.EndSample();

            Profiler.BeginSample("Storm Terrain - DrawRenderers - Set RT"); 
            cmd.SetRenderTarget(renderTargets, tempRT);

            //Clear all render targets as the drawing is additive blend mode
            cmd.ClearRenderTarget(true, true, Color.clear);
            Profiler.EndSample();
            
            Profiler.EndSample();

            Profiler.BeginSample("Storm Terrain - DrawRenderers - Set View Matrix");

            //Set the camera to the correct position in world space
            m_camera.transform.position = new Vector3(tilePos.x, maxHeight, tilePos.y);
            m_camera.orthographicSize = tileSize * 0.5f;

            //Retrieve the view and projection matrices from the camera's newly assigned position
            cmd.SetViewMatrix(m_camera.worldToCameraMatrix);
            cmd.SetProjectionMatrix(m_camera.projectionMatrix);

            Profiler.EndSample();

            // Create bounds that represent the camera's view volume
            Bounds orthographicBounds = new Bounds();
            float orthoWidth = m_camera.orthographicSize;
            orthographicBounds.center = m_camera.transform.position + new Vector3(0, -(m_camera.farClipPlane * 0.5f), 0);
            orthographicBounds.extents = new Vector3(orthoWidth, m_camera.farClipPlane * 0.5f, orthoWidth);

            Profiler.BeginSample("Storm Terrain - DrawRenderers - Draw Renderers");
            // Draw all renderers directly since they all use the same shader
            foreach(var decal in m_decals)
            {
                if(decal.m_Mesh != null && decal.m_decalMaterial != null)
                {
                    /*Profiler.BeginSample("Storm Terrain - DrawRenderers - Draw Renderers - Bounds Test");
                    bool intersects = orthographicBounds.Intersects(decal.m_renderer.bounds);
                    Profiler.EndSample();
                    if(intersects)
                    {*/

                        if(decal.m_decalType == StormVTEnums.StormVTDecalType._Road)
                        {
                            Profiler.BeginSample("Storm Terrain - DrawRenderers - Draw Renderers - Set Road Data");
                            decal.m_decalMaterial.SetFloat(StormVTShaderIDs.m_stormVT_DecalTileSize, tileSize);
                            decal.m_decalMaterial.SetVector(StormVTShaderIDs.m_stormVT_DecalTilePos, tilePos);
                            decal.m_decalMaterial.SetTexture(StormVTShaderIDs.m_stormVT_TileCaptureBlend_TileSource0, source0);
                            Profiler.EndSample();
                        }
                        Profiler.BeginSample("Storm Terrain - DrawRenderers - Draw Renderers - DrawMesh");
                        decal.m_renderer.GetPropertyBlock(propertyBlock);
                        
                        cmd.DrawMesh(decal.m_Mesh, decal.m_transform, decal.m_decalMaterial, 0, 0, propertyBlock);
                        Profiler.EndSample();
                    //}

                }
            }
            Profiler.EndSample();
            
            Profiler.BeginSample("Storm Terrain - DrawRenderers - Copy Texture");
            //Copy the temporary render textures into the buffer array ones, for permenant keeping
            cmd.CopyTexture(tempRT, m_source0RT[bufferIndex]);
            cmd.CopyTexture(tempRT2, m_source1RT[bufferIndex]);
            cmd.CopyTexture(tempRT3, m_source2RT[bufferIndex]);
            //Release temporary render textures for this command buffer
            cmd.ReleaseTemporaryRT(tempRT);
            cmd.ReleaseTemporaryRT(tempRT2);
            cmd.ReleaseTemporaryRT(tempRT3);
            Profiler.EndSample();
            
            Profiler.BeginSample("Storm Terrain - DrawRenderers - Execute Command Buffer");
            //Execute the command buffer
            Graphics.ExecuteCommandBuffer(cmd);
            Profiler.EndSample();

            Profiler.EndSample();
        }

    }
}

