using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Profiling;

namespace ProceduralWorlds.Storm.Terrain
{

    public class StormVTFeedbackRendererCustomPass : MonoBehaviour
    {
        #region Public Variables
        /// <summary>
        /// Reference to the feedback clipmap component
        /// </summary>
        public StormVTFeedbackClipmap stormVTClipmap;

        /// <summary>
        /// Reference to the storm terrain grid
        /// </summary>
        public StormVTTerrainGrid stormVTTerrainGrid;

        /// <summary>
        /// Reference to the feedback rendereer material to blit with
        /// </summary>
        public Material m_feedbackRendererMatClipmap;

        /// <summary>
        /// The render texture to store full resolution feedback results into
        /// </summary>
        public RenderTexture feedbackRenderTexture;

        /// <summary>
        /// The render texture to store lower resolution feedback results into, for compute shader readback
        /// </summary>
        public RenderTexture readbackRenderTexture;

        /// <summary>
        /// The render texture to store full resolution terrain id results into
        /// </summary>
        public RenderTexture terrainIDRenderTexture;

        /// <summary>
        /// The render texture to store lower resolution terrain id results into, for compute shader readback
        /// </summary>
        public RenderTexture readbackTerrainIDRenderTexture;

        /// <summary>
        /// Reference to the camera to use for rendering the clipmap into
        /// The camera's rendering ability is disabled at runtime, it is just used for
        /// gathering the view and projection matrices for the command buffer
        /// </summary>
        public Camera m_feedbackCamera;

        /// <summary>
        /// Width of the full resolution feedback / terrain id texture
        /// </summary>
        public int feedbackWidth = 240;

        /// <summary>
        /// Height of the full resolution feedback / terrain id texture
        /// </summary>
        public int feedbackHeight = 135;

        /// <summary>
        /// Width of the lower resolution feedback / terrain id texture
        /// </summary>
        public int readbackWidth = 30;

        /// <summary>
        /// Height of the lower resolution feedback / terrain id texture
        /// </summary>
        public int readbackHeight = 16;

        #endregion

        #region Private Variables

        /// <summary>
        /// Reference to the temporary render texture for feedback data, used in command buffer
        /// </summary>
        private static readonly int m_tempFeedbackRTID = Shader.PropertyToID("_TempFeedbackID");

        /// <summary>
        /// Reference to the temporary render texture for terrain ID data, used in command buffer
        /// </summary>
        private static readonly int m_tempTerrainRTID = Shader.PropertyToID("_TempTerrainID");

        /// <summary>
        /// Multiple Render Target (MRT) setup for rendering feedback and terrain ID textures in a single blit
        /// </summary>
        private RenderTargetIdentifier[] renderTargets;

        /// <summary>
        /// The command buffer to use for blitting data into the textures
        /// </summary>
        private CommandBuffer m_cmdBuffer;

        /// <summary>
        /// Command buffer name reference, useful for profiling support
        /// </summary>
        private string cmdBufferName = "Feedback Command Buffer";
        #endregion

        #region Events
        /// <summary>
        /// Callback event when the feedback textures have been created, used for debugging UI elements
        /// </summary>
        public delegate void FeedbackTexturesCreated();
        public static event FeedbackTexturesCreated OnFeedbackTexturesCreated;
        #endregion

        /// <summary>
        /// Recreate the render textures and assign them to MRTs
        /// </summary>
        public void ReInitialize()
        {
            RenderTextureFormat format = RenderTextureFormat.ARGB32;

            feedbackRenderTexture = new RenderTexture(feedbackWidth, feedbackHeight, 32, format, RenderTextureReadWrite.Linear);
            feedbackRenderTexture.useMipMap = false;
            feedbackRenderTexture.autoGenerateMips = false;
            feedbackRenderTexture.filterMode = FilterMode.Point;
            feedbackRenderTexture.wrapMode = TextureWrapMode.Clamp;
            feedbackRenderTexture.name = "FeedbackRenderer";
            feedbackRenderTexture.enableRandomWrite = true;
            feedbackRenderTexture.Create();

            readbackRenderTexture = new RenderTexture(readbackWidth, readbackHeight, 32, format, RenderTextureReadWrite.Linear);
            readbackRenderTexture.useMipMap = false;
            readbackRenderTexture.autoGenerateMips = false;
            readbackRenderTexture.filterMode = FilterMode.Point;
            readbackRenderTexture.wrapMode = TextureWrapMode.Clamp;
            readbackRenderTexture.enableRandomWrite = true;
            readbackRenderTexture.name = "ReadbackRenderer";

            terrainIDRenderTexture = new RenderTexture(feedbackWidth, feedbackHeight, 32, format, RenderTextureReadWrite.Linear);
            terrainIDRenderTexture.useMipMap = false;
            terrainIDRenderTexture.autoGenerateMips = false;
            terrainIDRenderTexture.filterMode = FilterMode.Point;
            terrainIDRenderTexture.wrapMode = TextureWrapMode.Clamp;
            terrainIDRenderTexture.name = "TerrainIDRenderer";
            terrainIDRenderTexture.enableRandomWrite = true;

            readbackTerrainIDRenderTexture = new RenderTexture(readbackWidth, readbackHeight, 32, format, RenderTextureReadWrite.Linear);
            readbackTerrainIDRenderTexture.useMipMap = false;
            readbackTerrainIDRenderTexture.autoGenerateMips = false;
            readbackTerrainIDRenderTexture.filterMode = FilterMode.Point;
            readbackTerrainIDRenderTexture.wrapMode = TextureWrapMode.Clamp;
            readbackTerrainIDRenderTexture.name = "ReadbackTerrainIDRenderer";
            readbackTerrainIDRenderTexture.enableRandomWrite = true;

            renderTargets = new RenderTargetIdentifier[2];

            if (OnFeedbackTexturesCreated != null)
            {
                OnFeedbackTexturesCreated();
            }

        }

        /// <summary>
        /// Initialize the command buffer
        /// </summary>
        private void InitCommandBuffer()
        {
            if (m_cmdBuffer == null)
            {
                m_cmdBuffer = new CommandBuffer();
                m_cmdBuffer.name = cmdBufferName;
            }
        }

        /// <summary>
        /// Perform the rendering of the clipmap into the feedback and terrain id textures
        /// </summary>
        public void ManualRender()
        {
            Profiler.BeginSample("Storm Terrain - Feedback Renderer - Manual Render");

            InitCommandBuffer();
            m_cmdBuffer.Clear();

            var clipmapReady = stormVTClipmap != null && stormVTClipmap.m_clipmapInitialized;


            Profiler.BeginSample("Storm Terrain - Feedback Renderer - Manual Render - Temp RT Setup");
            m_cmdBuffer.GetTemporaryRT(m_tempFeedbackRTID, feedbackRenderTexture.descriptor);
            m_cmdBuffer.GetTemporaryRT(m_tempTerrainRTID, feedbackRenderTexture.descriptor);

            renderTargets[0] = m_tempFeedbackRTID;
            renderTargets[1] = m_tempTerrainRTID;


            m_cmdBuffer.SetRenderTarget(renderTargets, m_tempFeedbackRTID);
            m_cmdBuffer.ClearRenderTarget(true, true, Color.clear, 1.0f);
            Profiler.EndSample();

            m_cmdBuffer.SetViewMatrix(m_feedbackCamera.worldToCameraMatrix);
            m_cmdBuffer.SetProjectionMatrix(m_feedbackCamera.projectionMatrix);


            if (clipmapReady && m_feedbackRendererMatClipmap != null)
            {
                stormVTClipmap.ClipmapRefresh();
                stormVTClipmap.DrawClipmapCommandBuffer(m_cmdBuffer, m_feedbackRendererMatClipmap, 0);
            }
            

            m_cmdBuffer.CopyTexture(m_tempFeedbackRTID,feedbackRenderTexture);
            m_cmdBuffer.CopyTexture(m_tempTerrainRTID,terrainIDRenderTexture);

            m_cmdBuffer.ReleaseTemporaryRT(m_tempFeedbackRTID);
            m_cmdBuffer.ReleaseTemporaryRT(m_tempTerrainRTID);

            m_cmdBuffer.Blit(feedbackRenderTexture, readbackRenderTexture);
            m_cmdBuffer.Blit(terrainIDRenderTexture, readbackTerrainIDRenderTexture);

            Profiler.BeginSample("Storm Terrain - Feedback Renderer - Manual Render - ExecuteCommandBuffer");
            Graphics.ExecuteCommandBuffer(m_cmdBuffer);
            Profiler.EndSample();

            Profiler.EndSample();
        }

    }
}