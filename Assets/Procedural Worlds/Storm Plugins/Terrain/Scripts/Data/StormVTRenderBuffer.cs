using System;
using UnityEngine;

namespace ProceduralWorlds.Storm.Terrain
{
    /// <summary>
    /// A class used to perform double buffering
    /// </summary>
    public class StormVTRenderBuffer
    {
        /// <summary>
        /// First buffer
        /// </summary>
        public RenderTexture buffer0;

        /// <summary>
        /// Second buffer
        /// </summary>
        public RenderTexture buffer1;

        /// <summary>
        /// Keeps track on which render texture was most recently used
        /// </summary>
        bool activeBuffer0;

        /// <summary>
        /// Create a render buffer with default values
        /// </summary>
        public StormVTRenderBuffer()
        {
            buffer0 = new RenderTexture(1,1,0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
            buffer1 = new RenderTexture(1,1,0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
            activeBuffer0 = true;
        }

        /// <summary>
        /// Create a render buffer with custom inputs
        /// </summary>
        /// <param name="size">Resolution of the render textures</param>
        /// <param name="format">Format of the render textures</param>
        /// <param name="renderTextureReadWrite">Linear or sRGB option</param>
        /// <param name="useMipmap">Whether or not to use mipmaps</param>
        public StormVTRenderBuffer(int size, RenderTextureFormat format, RenderTextureReadWrite renderTextureReadWrite, bool useMipmap = false)
        {
            buffer0 = new RenderTexture(size, size, 0, format, renderTextureReadWrite);
            buffer1 = new RenderTexture(size, size, 0, format, renderTextureReadWrite);
            activeBuffer0 = true;

            if(useMipmap)
            {
                buffer0.useMipMap = true;
                buffer0.autoGenerateMips = false;
                buffer0.anisoLevel = 9;
                buffer1.useMipMap = true;
                buffer1.autoGenerateMips = false;
                buffer1.anisoLevel = 9;
            }

            buffer0.Create();
            buffer1.Create();

        }

        /// <summary>
        /// Returns the render texture that was most recently used
        /// </summary>
        /// <returns></returns>
        public ref RenderTexture GetActiveBuffer()
        {
            if(activeBuffer0)
            {
                return ref buffer0;
            }
            else
            {
                return ref buffer1;
            }
            
        }

        /// <summary>
        /// Swaps the active render texture
        /// </summary>
        public void SwapBuffers()
        {
            activeBuffer0 = !activeBuffer0;
        }

    }

    /// <summary>
    /// A class used to contain Multiple Render Targets (MRT) handling capabilities for the Terrain Tile Generation
    /// </summary>
    [Serializable]
    public class StormVTMRTBuffer
    {
        /// <summary>
        /// MRT setup for Height and Normal textures
        /// </summary>
        public RenderBuffer[] m_terrainDataMRTBuffer;

        /// <summary>
        /// MRT setup for source0 and source1 textures
        /// </summary>
        public RenderBuffer[] m_colorDataMRTBuffer;

        /// <summary>
        /// MRT setup for distance map textures (Tree, Grass and Normals)
        /// </summary>
        public RenderBuffer[] m_distanceMapMRTBuffer;

        /// <summary>
        /// Creates the MRT buffers
        /// </summary>
        public StormVTMRTBuffer()
        {
            m_terrainDataMRTBuffer = new RenderBuffer[2];
            m_colorDataMRTBuffer = new RenderBuffer[2];
            m_distanceMapMRTBuffer = new RenderBuffer[3];
        }

        /// <summary>
        /// Assigns the render textures into the MRT slots for the terrain data
        /// </summary>
        /// <param name="height">Heightmap</param>
        /// <param name="worldNormal">world normal map</param>
        public void SetTerrainBuffers(RenderTexture height, RenderTexture worldNormal)
        {
            m_terrainDataMRTBuffer[0] = height.colorBuffer;
            m_terrainDataMRTBuffer[1] = worldNormal.colorBuffer;
        }

        /// <summary>
        /// Assigns the render textures into the MRT slots for the color source data
        /// </summary>
        /// <param name="source0">Source0 texture</param>
        /// <param name="source1">Source1 texture</param>
        public void SetSourceBuffers(RenderTexture source0, RenderTexture source1)
        {
            m_colorDataMRTBuffer[0] = source0.colorBuffer;
            m_colorDataMRTBuffer[1] = source1.colorBuffer;
        }

        /// <summary>
        /// Assigns the render textures into the MRT slots for the distance map data
        /// </summary>
        /// <param name="grassColor">Grass Color map texture</param>
        /// <param name="treeColor">Tree Color map texture</param>
        /// <param name="normal">Normal map texture</param>
        public void SetDistanceMapBuffer(RenderTexture grassColor, RenderTexture treeColor, RenderTexture normal)
        {
            m_distanceMapMRTBuffer[0] = grassColor.colorBuffer;
            m_distanceMapMRTBuffer[1] = treeColor.colorBuffer;
            m_distanceMapMRTBuffer[2] = normal.colorBuffer;
        }
    }

}


