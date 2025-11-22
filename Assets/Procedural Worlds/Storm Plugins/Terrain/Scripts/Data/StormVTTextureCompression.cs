using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

namespace ProceduralWorlds.Storm.Terrain
{

    public class StormVTTextureCompression : MonoBehaviour
    {

        #region Public Variables

        /// <summary>
        /// Reference to the storm terrain settings scriptable object
        /// </summary>
        public StormVTSettings stormVTSettings;

        /// <summary>
        /// Format of the encoder, BC3 for RGBA compression
        /// </summary>
        [HideInInspector]
        public EncodeBCn.Format m_Format = EncodeBCn.Format.BC3_XDK;

        /// <summary>
        /// Quality of the compression
        /// </summary>
        [HideInInspector]
        [Range(0, 1)] public float m_Quality = 0.25f;

        /// <summary>
        /// Reference to the compute shader to encode a texture to DXT format
        /// </summary>
        [HideInInspector]
        public ComputeShader m_EncodeShaderDXT;

        /// <summary>
        /// Source0 Tex array used for CopyTexture commands to get compression right
        /// </summary>
        [HideInInspector]
        public Texture2D[] m_source0Tex;


        /// <summary>
        /// Source1 Tex array used for CopyTexture commands to get compression right
        /// </summary>
        [HideInInspector]
        public Texture2D[] m_source1Tex;


        /// <summary>
        /// Normal Tex array used for CopyTexture commands to get compression right
        /// </summary>
        [HideInInspector]
        public Texture2D[] m_normalTex;


        /// <summary>
        /// Distance Map Normal Tex array used for CopyTexture commands to get compression right
        /// </summary>
        [HideInInspector]
        public Texture2D[] m_distanceMapNormalTex;


        /// <summary>
        /// Grass Map Color Tex array used for CopyTexture commands to get compression right
        /// </summary>
        [HideInInspector]
        public Texture2D[] m_grassMapColorTex;


        /// <summary>
        /// Tree Map Color Tex array used for CopyTexture commands to get compression right
        /// </summary>
        [HideInInspector]
        public Texture2D[] m_treeMapColorTex;

        #endregion

        #region Private Variables

        /// <summary>
        /// Reference to the encoder class
        /// </summary>
        private EncodeBCn m_Encoder;

        /// <summary>
        /// Command buffer to reuse for encoding functions
        /// </summary>
        private CommandBuffer m_Cmd;

        #endregion

        /// <summary>
        /// allocate memory to texture arrays
        /// </summary>
        void Start()
        {
            if(stormVTSettings != null)
            {
                if(stormVTSettings.m_colorCompressedFormat != StormVTTextureSettings.StormCompressionFormat.RGBA32)
                {
                    m_Encoder = new EncodeBCn(m_EncodeShaderDXT);

                    m_source0Tex = new Texture2D[stormVTSettings.m_maxRequestsPerFrame];
                    m_source1Tex = new Texture2D[stormVTSettings.m_maxRequestsPerFrame];
                    m_normalTex = new Texture2D[stormVTSettings.m_maxRequestsPerFrame];
                    m_distanceMapNormalTex = new Texture2D[stormVTSettings.m_maxRequestsPerFrame];
                    m_grassMapColorTex = new Texture2D[stormVTSettings.m_maxRequestsPerFrame];
                    m_treeMapColorTex = new Texture2D[stormVTSettings.m_maxRequestsPerFrame];

                    switch (stormVTSettings.m_colorCompressedFormat)
                    {
                        default:
                        case StormVTTextureSettings.StormCompressionFormat.DXT5:
                            SetupTexturesDXT();
                            m_Cmd = new CommandBuffer();
                            m_Cmd.name = "MY Compression Buffer";
                            break;
                    }
                }
            }

            

        }

        /// <summary>
        /// Encodes a texture with no mips
        /// </summary>
        /// <param name="tex">Texture to compress</param>
        /// <param name="destTex">Destination texture to store compresssed version in</param>
        /// <param name="size">Resolution of the texture</param>
        public void Encode(RenderTexture sourceTex, Texture2D destTex, int size)
        {
            UpdateCommandBuffer(sourceTex, destTex, size);
            Graphics.ExecuteCommandBuffer(m_Cmd);
        }

        /// <summary>
        /// Encodes a texture with mips
        /// </summary>
        /// <param name="tex">Texture to compress</param>
        /// <param name="destTex">Destination texture to store compresssed version in</param>
        /// <param name="size">Resolution of the texture</param>
        public void EncodeWithMips(RenderTexture sourceTex, Texture2D destTex, int size)
        {
            UpdateCommandBufferWithMips(sourceTex, destTex, size);
            Graphics.ExecuteCommandBuffer(m_Cmd);
        }


        /// <summary>
        /// Cleanup memroy allocated variables once destroyed
        /// </summary>
        public void OnDestroy()
        {
            if(stormVTSettings.m_colorCompressedFormat != StormVTTextureSettings.StormCompressionFormat.RGBA32)
            {
                for (int i = 0; i < stormVTSettings.m_maxRequestsPerFrame; i++)
                {
                    DestroyImmediate(m_source0Tex[i]);
                    m_source0Tex[i] = null;
                    DestroyImmediate(m_source1Tex[i]);
                    m_source1Tex[i] = null;
                    DestroyImmediate(m_normalTex[i]);
                    m_normalTex[i] = null;
                    DestroyImmediate(m_distanceMapNormalTex[i]);
                    m_distanceMapNormalTex[i] = null;

                }

                m_Cmd?.Dispose(); m_Cmd = null;
            }
           
        }

        /// <summary>
        /// Encode the texture with no mips
        /// </summary>
        /// <param name="tex">Texture to compress</param>
        /// <param name="destTex">Destination texture to store compresssed version in</param>
        /// <param name="size">Resolution of the texture</param>
        void UpdateCommandBuffer(RenderTexture tex, Texture2D destTex, int size)
        {
            m_Cmd.Clear();
            m_Encoder.Encode(m_Cmd, tex, size, size, destTex, m_Format, m_Quality);
        }

        /// <summary>
        /// Encode the texture with mips
        /// </summary>
        /// <param name="tex">Texture to compress</param>
        /// <param name="destTex">Destination texture to store compresssed version in</param>
        /// <param name="size">Resolution of the texture</param>
        void UpdateCommandBufferWithMips(RenderTexture sourceTex, Texture2D destTex, int size)
        {
            m_Cmd.Clear();
            m_Encoder.EncodeWithMips(m_Cmd, sourceTex, size, size, destTex, m_Format, m_Quality);
        }

        /// <summary>
        /// Initialize the textures for DXT compression
        /// </summary>
        void SetupTexturesDXT()
        {
            TextureCreationFlags flags = TextureCreationFlags.DontInitializePixels | TextureCreationFlags.DontUploadUponCreate | TextureCreationFlags.MipChain;
            TextureCreationFlags flagsNoMip = TextureCreationFlags.DontInitializePixels | TextureCreationFlags.DontUploadUponCreate;
            for (int i = 0; i < stormVTSettings.m_maxRequestsPerFrame; i++)
            {
                int width = stormVTSettings.m_tileSizedPadded;
                int height = stormVTSettings.m_tileSizedPadded;
                if (m_Format != EncodeBCn.Format.None)
                {
                    width = (width + 3) / 4 * 4;
                    height = (height + 3) / 4 * 4;
                }

                GraphicsFormat gfxFormat = m_Format switch
                {
                    EncodeBCn.Format.BC1_XDK => GraphicsFormat.RGBA_DXT1_SRGB,
                    EncodeBCn.Format.BC3_XDK => GraphicsFormat.RGBA_DXT5_SRGB,
                    _ => GraphicsFormat.None
                };


                m_source0Tex[i] = new Texture2D(width, height, gfxFormat, 2, flags);
                m_source0Tex[i].Apply(false, true);
                m_source1Tex[i] = new Texture2D(width, height, gfxFormat, 2, flags);
                m_source1Tex[i].Apply(false, true);
                m_normalTex[i] = new Texture2D(width, height, gfxFormat, flagsNoMip);
                m_normalTex[i].Apply(false, true);
                m_distanceMapNormalTex[i] = new Texture2D(width, height, gfxFormat, flagsNoMip);
                m_distanceMapNormalTex[i].Apply(false, true);
                m_treeMapColorTex[i] = new Texture2D(width, height, gfxFormat, 2, flags);
                m_treeMapColorTex[i].Apply(false, true);
                m_grassMapColorTex[i] = new Texture2D(width, height, gfxFormat, 2, flags);
                m_grassMapColorTex[i].Apply(false, true);
            }


            
        }

    }

}



