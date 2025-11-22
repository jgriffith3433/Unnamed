using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using UnityEngine.Profiling;

namespace ProceduralWorlds.Storm.Terrain
{
    /// <summary>
    /// A static class to retrieve the list of zoom layers
    /// </summary>
    public static class StormVTZoomLayers
    {
        /// <summary>
        /// Contains a list of zoom layers (in base 2 world scale)
        /// </summary>
        public static Dictionary<int, VTZoomLayer> m_vtZoomLayers = new Dictionary<int, VTZoomLayer>();

        /// <summary>
        /// Constructs the list of VTZoomLayers
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void InitializeVTZoomLayers()
        {
            if (m_vtZoomLayers.Count > 0)
                m_vtZoomLayers.Clear();

            int count = 30;
            float scale = 0.0625f;
            for (int i = 0; i < count; i++)
            {
                VTZoomLayer tempTile = new VTZoomLayer();

                tempTile.m_scale = scale;
                tempTile.m_texelSize = tempTile.m_scale / 256f;
                tempTile.m_zoomLevel = i;
                m_vtZoomLayers.Add(i, tempTile);

                scale *= 2;
            }
        }

        /// <summary>
        /// Returns the closest rounded zoom layer (in base 2) for the provided texel size
        /// </summary>
        /// <param name="tileTexelSize">Texel size used for the tile - can be odd and non base 2 numbers</param>
        /// <returns></returns>
        public static VTZoomLayer GetRoundedZoomLayer(float tileTexelSize)
        {
            VTZoomLayer tempTile = new VTZoomLayer();

            int i = 0;
            while ((i < m_vtZoomLayers.Count) && (tileTexelSize != m_vtZoomLayers[i].m_texelSize) &&
                   (tileTexelSize > m_vtZoomLayers[i].m_texelSize))
            {
                i++;
            }

            tempTile = m_vtZoomLayers[i];

            return tempTile;
        }

        /// <summary>
        /// Returns a zoom tile coordinate (Zoom Coord X, Zoom Coord Y, Zoom Level) based on center position and zoom level
        /// </summary>
        /// <param name="centerPosition">The center world space position of the tile</param>
        /// <param name="zoomLevel">The zoom level this tile resides in</param>
        /// <returns></returns>
        public static int3 GetZoomTileCoordinate(Vector2 centerPosition, byte zoomLevel)
        {
            Profiler.BeginSample("Storm Terrain - QuadTree Updated - GetZoomTileCoordinate");

            float scale = m_vtZoomLayers[zoomLevel].m_scale;
            float invScale = 1f / scale;
            float halfScale = scale * 0.5f;
            
            int2 zoomTileCoordinate = new int2(
                (int)math.floor((centerPosition.x - halfScale) * invScale),
                (int)math.floor((centerPosition.y - halfScale) * invScale));

            Profiler.EndSample();

            return new int3(zoomTileCoordinate.x, zoomTileCoordinate.y, zoomLevel);
        }

        /// <summary>
        /// Returns a zoom tile coordinate (Zoom Coord X, Zoom Coord Y, Zoom Level) based on center position and zoom level
        /// </summary>
        /// <param name="quadNode">Reference to quad node to convert to zoom tile coordinate</param>
        /// <param name="maxDepth">The max depth of the Quad Tree</param>
        /// <param name="lowestZoomLevel">The lowest zoom level that the system will render.</param>
        /// <returns></returns>
        public static int3 GetZoomTileCoordinate(Rect bounds, Vector2 center, byte depth, int highestZoomLevel)
        {
            byte zoomLevel = (byte)(highestZoomLevel - depth + 2);

            return GetZoomTileCoordinate(center, zoomLevel);
        }

        /// <summary>
        /// Returns a world space center position and size based on a Grid Coordinate (Coord X, Coord Y, Zoom Level)
        /// </summary>
        /// <param name="zoomCoord"></param>
        /// <returns></returns>
        public static (Vector3, float) ConvertZoomCoordinateToWorldSpace(int3 zoomCoord)
        {
            float worldSize = m_vtZoomLayers[zoomCoord.z].m_scale;
            float halfWorldSize = worldSize * 0.5f;
            Vector2 bottomLeftPos = new Vector2(
                zoomCoord.x * worldSize,
                zoomCoord.y * worldSize);

            return (new Vector3(bottomLeftPos.x + halfWorldSize, 0, bottomLeftPos.y + halfWorldSize), worldSize);
        }

        /// <summary>
        /// Returns a zoom tile coordinate (Zoom Coord X, Zoom Coord Y, Zoom Level) based on position and zoom level
        /// </summary>
        /// <param name="position">The center world space position of the tile</param>
        /// <param name="zoomLevel">The zoom level this tile resides in</param>
        /// <returns></returns>
        public static int3 GetTileCoordinate(Vector2 position, int zoomLevel)
        {
            float scale = StormVTZoomLayers.m_vtZoomLayers[zoomLevel].m_scale;

            int2 zoomTileCoordinate = new int2(
                (int)Mathf.Floor(position.x / scale),
                (int)Mathf.Floor(position.y / scale));

            return new int3(zoomTileCoordinate.x, zoomTileCoordinate.y, zoomLevel);
        }


        /// <summary>
        /// Converts an int3 tile coordinate to a string ID
        /// </summary>
        /// <param name="tile"></param>
        /// <returns></returns>
        public static string GetRVTTileID(int3 tile)
        {
            return $"{tile.z}/{tile.x}/{tile.y}";
        }
    }

    public class VTZoomLayer
    {
        /// <summary>
        /// ID of the zoom level
        /// </summary>
        public int m_zoomLevel;

        /// <summary>
        /// Texel Size of zoom level - describes the meters per pixel
        /// </summary>
        public float m_texelSize;

        /// <summary>
        /// World scale of the bounds for the zoom layer
        /// </summary>
        public float m_scale;

        public VTZoomLayer()
        {
            m_zoomLevel = 0;
            m_texelSize = 0;
            m_scale = 0;
        }
    }
}