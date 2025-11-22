using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProceduralWorlds.Storm.Terrain
{

    [CreateAssetMenu(fileName = "StormVTTerrainTextureReferenceHolder", menuName = "Procedural Worlds/Storm/Terrain/TerrainTextureReferenceHolder",
           order = 6)]
    [Serializable]
    public class StormVTTextureLayerReferenceHolder : ScriptableObject
    {
        [Serializable]
        public class TerrainLayerTextureReference
        {
            /// <summary>
            /// Reference to the terrain layer 
            /// </summary>
            public TerrainLayer terrainLayer;

            /// <summary>
            /// Reference to the terrain layer's diffuse texture
            /// </summary>
            public Texture2D diffuseTexture;

            /// <summary>
            /// Reference to the terrain layer's normal texture
            /// </summary>
            public Texture2D normalTexture;

            /// <summary>
            /// Reference to the terrain layer's mask map texture
            /// </summary>
            public Texture2D maskTexture;
        }

        [SerializeReference]
        /// <summary>
        /// Reference to all the terrain layer references
        /// </summary>
        public TerrainLayerTextureReference[] terrainLayers;

        /// <summary>
        /// Populates the terrain layer references with the relevant textures that should not be included in a build
        /// </summary>
        /// <param name="stormVTTerrainTextureLayers"></param>
        public void FillTerrainLayerTextureReferences(List<StormVTTerrainTextureLayer> stormVTTerrainTextureLayers)
        {

            if(terrainLayers == null)
            {
                terrainLayers = new TerrainLayerTextureReference[stormVTTerrainTextureLayers.Count];
                for (int i = 0; i < stormVTTerrainTextureLayers.Count; i++)
                {
                    terrainLayers[i] = new TerrainLayerTextureReference();
                }
            }


            for (int i = 0; i < stormVTTerrainTextureLayers.Count; i++)
            {

                StormVTTerrainTextureLayer terrainTextureLayer = stormVTTerrainTextureLayers[i];
                if(terrainTextureLayer.m_terrainLayerReference != null)
                    terrainLayers[i].terrainLayer = terrainTextureLayer.m_terrainLayerReference;
                if (terrainTextureLayer.m_diffuseMap != null)
                    terrainLayers[i].diffuseTexture = terrainTextureLayer.m_diffuseMap;
                if(terrainTextureLayer.m_normalMap != null)
                    terrainLayers[i].normalTexture = terrainTextureLayer.m_normalMap;
                if(terrainTextureLayer.m_maskMap != null)
                    terrainLayers[i].maskTexture = terrainTextureLayer.m_maskMap;
            }
        }

        /// <summary>
        /// Empties the existing Storm Terrain Texture Layers of their texture references
        /// </summary>
        /// <param name="stormVTTerrainTextureLayers">List of Terrain Texture Layers to empty</param>
        public void EmptyTextureLayerReferences(List<StormVTTerrainTextureLayer> stormVTTerrainTextureLayers)
        {
            for (int i = 0; i < stormVTTerrainTextureLayers.Count; i++)
            {
                stormVTTerrainTextureLayers[i].m_terrainLayerReference = null;
                stormVTTerrainTextureLayers[i].m_diffuseMap = null;
                stormVTTerrainTextureLayers[i].m_normalMap = null;
                stormVTTerrainTextureLayers[i].m_maskMap = null;
            }
        }

        /// <summary>
        /// Repopulates the Storm Terrain Texture Layers of their texture references
        /// </summary>
        /// <param name="stormVTTerrainTextureLayers">List of Terrain Texture Layers to repopulate</param>
        public void PopulateTerrainTextureLayerReferences(List<StormVTTerrainTextureLayer> stormVTTerrainTextureLayers)
        {
            for (int i = 0; i < stormVTTerrainTextureLayers.Count; i++)
            {
                stormVTTerrainTextureLayers[i].m_terrainLayerReference = terrainLayers[i].terrainLayer;
                stormVTTerrainTextureLayers[i].m_diffuseMap = terrainLayers[i].diffuseTexture;
                stormVTTerrainTextureLayers[i].m_normalMap = terrainLayers[i].normalTexture;
                stormVTTerrainTextureLayers[i].m_maskMap = terrainLayers[i].maskTexture;
            }
        }

        /// <summary>
        /// Empties the existing Unity Terrain Layers of their texture references
        /// </summary>
        public void EmptyTerrainLayerTextureReferences()
        {
            if (terrainLayers != null)
            {
                for(int i = 0; i < terrainLayers.Length; i++)
                {
                    terrainLayers[i].terrainLayer.diffuseTexture = null;
                    terrainLayers[i].terrainLayer.normalMapTexture = null;
                    terrainLayers[i].terrainLayer.maskMapTexture = null;
                }
            }
        }

        /// <summary>
        /// Repopulates the existing Unity Terrain Layers of their texture references
        /// </summary>
        public void PopulateTerrainLayerTextureReferences()
        {
            if (terrainLayers != null)
            {
                for (int i = 0; i < terrainLayers.Length; i++)
                {
                    terrainLayers[i].terrainLayer.diffuseTexture = terrainLayers[i].diffuseTexture;
                    terrainLayers[i].terrainLayer.normalMapTexture = terrainLayers[i].normalTexture;
                    terrainLayers[i].terrainLayer.maskMapTexture = terrainLayers[i].maskTexture;
                }
            }
        }
    }

}