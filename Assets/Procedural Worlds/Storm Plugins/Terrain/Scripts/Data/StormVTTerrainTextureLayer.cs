using System;
using Unity.Mathematics;
using UnityEngine;
using static ProceduralWorlds.Storm.Terrain.StormVTTerrainTile;

namespace ProceduralWorlds.Storm.Terrain
{
    [Serializable]
    public class StormVTTerrainTextureLayer
    {
        /// <summary>
        /// Reference to the base Unity Terrain Layer
        /// </summary>
        public TerrainLayer m_terrainLayerReference;

        /// <summary>
        /// Name of this layer
        /// </summary>
        public string m_name = "";

        #region Textures
        /// <summary>
        /// Reference to the diffuse map
        /// </summary>
        public Texture2D m_diffuseMap;

        /// <summary>
        /// Reference to the normal map
        /// </summary>
        public Texture2D m_normalMap;

        /// <summary>
        /// Reference to the mask map
        /// </summary>
        public Texture2D m_maskMap;

        /// <summary>
        /// Reference to the packed diffuse map
        /// Diffuse.rgb, Height in Alpha
        /// </summary>
        public Texture2D m_packedDiffuseMap;

        /// <summary>
        /// Reference to the packed normal map
        /// Tangent Normal.xy, Ambient Occlusion, Smoothness
        /// </summary>
        public Texture2D m_packedNormalMap;
        #endregion

        #region General
        /// <summary>
        /// Color tint of the layer
        /// </summary>
        public Color m_colorTint = Color.white;

        /// <summary>
        /// Normal strength
        /// </summary>
        public float m_normalStrength = 1.0f;
        #endregion


        #region Tiling Settings
        [Header("Tiling")]
        /// <summary>
        /// Toggles whether to perform world space tiling instead of per terrain uv
        /// </summary>
        public bool m_worldSpaceTiling = false;

        /// <summary>
        /// Size of UV tiling in world space
        /// </summary>
        public Vector2 m_worldSpaceSize = new Vector2(5.0f, 5.0f);

        /// <summary>
        /// Tile size of the UV per terrain
        /// </summary>
        public Vector2 m_tileSize = new Vector2(8, 8);

        /// <summary>
        /// Tile offset of the UV per terrain
        /// </summary>
        public Vector2 m_tileOffset = Vector2.zero;

        /// <summary>
        /// Mask map remap minimum values
        /// </summary>
        public Vector4 m_maskMapRemapMin = Vector4.zero;

        /// <summary>
        /// Mask map rema maximum values
        /// </summary>
        public Vector4 m_maskMapRemapMax = Vector4.one;
        #endregion


        #region Stochastic
        [Header("Stochastic")]
        /// <summary>
        /// Toggles stochastic tiling
        /// </summary>
        public bool m_stochasticTiling = true;

        /// <summary>
        /// Height blending contrast beween stochastic samples
        /// </summary>
        public float m_stochasticHeightContrast = 1.0f;

        /// <summary>
        /// Height blending remap brightness between stochastic samples
        /// </summary>
        public float m_stochasticHeightBrightness = 1.0f;

        /// <summary>
        /// Height blending remap position between stochastic samples
        /// </summary>
        [Range(-1, 1)]
        public float m_stochasticHeightPosition = 0.0f;

        /// <summary>
        /// Height blending factor between stochastic samples
        /// </summary>
        [Range(0, 1f)]
        public float m_stochasticHeightBlendFactor = 0.9f;

        /// <summary>
        /// Packed data of the all height blending variables
        /// </summary>
        [HideInInspector]
        public Vector4 m_stochasticHeightRemapPackedData;
        #endregion

        #region Puddles
        [Header("Puddles")]
        /// <summary>
        /// Toggles puddles on
        /// </summary>
        public bool m_puddles = false;

        /// <summary>
        /// Color of the puddles
        /// </summary>
        [ColorUsage(true, true)]
        public Color m_puddlesColor = new Color(0.5f, 0.5f, 0.5f, 0);

        /// <summary>
        /// Height level to blending puddles into exisitng terrain layer
        /// </summary>
        [Range(0.0f, 1.0f)]
        public float m_puddlesHeight = 0.1f;

        /// <summary>
        /// Smoothness value of the puddles
        /// </summary>
        [Range(0.0f, 1.0f)]
        public float m_puddlesSmoothness = 0.8f;

        /// <summary>
        /// Normal intensity of the puddles
        /// </summary>
        [Range(0.0f, 1.0f)]
        public float m_puddlesNormal = 0.1f;

        /// <summary>
        /// Blend factor of the puddles
        /// </summary>
        [Range(0f, 1.0f)]
        public float m_puddlesBlendFactor = 0.5f;

        /// <summary>
        /// Toggles whether to use variation mask to distrubute where puddles spawn
        /// </summary>
        public bool m_puddlesRandom = true;

        /// <summary>
        /// Channel mask from variation texture for mask distrubution
        /// </summary>
        public float4 m_puddlesRandomMask = new float4(1, 0, 0, 0);

        /// <summary>
        /// Packed data for the puddles variables
        /// </summary>
        [HideInInspector]
        public Vector4 m_puddlesData;

        #endregion

        #region Macro Variation
        [Header("Macro Variation")]
        /// <summary>
        /// Toggles macro variation
        /// </summary>
        public bool m_macroVariation = true;

        /// <summary>
        /// Color to multiply on the terrain layer for macro variation
        /// </summary>
        [ColorUsage(true, true)]
        public Color m_macroVariationColor = new Color(1, 1, 1, 1);

        /// <summary>
        /// Channel mask to distrubute macro variation
        /// </summary>
        public float4 m_macroVariationMask = new Vector4(0, 0, 0, 1);
        #endregion

        #region Detail Normals
        [Header("Detail Normals")]
        /// <summary>
        /// Toggles detail normals on
        /// </summary>
        public bool m_detailNormal = true;

        /// <summary>
        /// World UV scale of the near detail normals
        /// </summary>
        public float m_detailNormalNearScale = 10f;

        /// <summary>
        /// Near detail normal intensity
        /// </summary>
        public float m_detailNormalNearInfluence = 0.4f;

        /// <summary>
        /// World UV scale of the far detail normals
        /// </summary>
        public float m_detailNormalFarScale = 1000f;

        /// <summary>
        /// Far detail normal intensity
        /// </summary>
        public float m_detailNormalFarInfluence = 0.8f;

        /// <summary>
        /// Packed data for detail normals
        /// </summary>
        public float4 m_detailNormalPackedData = Vector4.zero;
        #endregion

        #region Height Remap
        [Header("Height Remap")]
        /// <summary>
        /// Contrast for the height map
        /// </summary>
        public float m_heightContrast = 1.0f;

        /// <summary>
        /// Brightness of the height map
        /// </summary>
        public float m_heightBrightness = 1.0f;

        /// <summary>
        /// Position remap of the height map
        /// </summary>
        [Range(-1, 1)]
        public float m_heightPosition = 0.0f;

        /// <summary>
        /// Height map blend factor
        /// </summary>
        [Range(0f, 1.0f)]
        public float m_heightBlendFactor = 0.5f;

        /// <summary>
        /// Packed heightmap data
        /// </summary>
        [HideInInspector]
        public Vector4 m_heightRemapPackedData;
        #endregion

        #region Displacement Remap
        [Header("Displacement Remap")]
        /// <summary>
        /// Contrast for the displacement
        /// </summary>
        public float m_displacementContrast = 1.0f;

        /// <summary>
        /// Brightness of the displacement
        /// </summary>
        public float m_displacementBrightness = 1.0f;

        /// <summary>
        /// Position remap of the displacement
        /// </summary>
        [Range(-1, 1)]
        public float m_displacementPosition = 0.0f;

        /// <summary>
        /// Packed displacement data
        /// </summary>
        [HideInInspector]
        public Vector4 m_displacementRemapPackedData;
        #endregion

        #region Misc
        /// <summary>
        /// Packed toggle booleans into a single bit
        /// </summary>
        public uint m_bitPackedLayerSettings;

        /// <summary>
        /// Reference to the terrain tile layer
        /// </summary>
        public TerrainTileLayer m_terrainTileLayer;
        #endregion

        #region Event System

        /// <summary>
        /// event called when any snow variable is changed
        /// </summary>
        public delegate void TerrainTileLayerUpdated();
        public static event TerrainTileLayerUpdated OnTerrainTileLayerUpdated;

        #endregion

        public void BroadcastTerrainTileLayerUpdated()
        {
            if (StormVTTerrainTextureLayer.OnTerrainTileLayerUpdated != null)
                StormVTTerrainTextureLayer.OnTerrainTileLayerUpdated();
        }

        /// <summary>
        /// Initialize a default terrain texture layer
        /// </summary>
        public StormVTTerrainTextureLayer()
        {
            m_name = "";
            m_colorTint = Color.white;
            m_normalStrength = 1.0f;

            //Tiling Settings
            m_worldSpaceTiling = false;
            m_worldSpaceSize = new Vector2(5.0f, 5.0f);
            m_tileSize = new Vector2(8, 8);
            m_tileOffset = Vector2.zero;

            m_maskMapRemapMin = new Vector4(0, 0, 0, 0);
            m_maskMapRemapMax = new Vector4(1, 1, 1, 1);

            m_puddles = false;
            m_puddlesHeight = 0;

            m_stochasticTiling = false;

            m_macroVariation = false;
            m_macroVariationColor = new Color(0.5f, 0.5f, 0.5f, 1.0f);


            //Height Remap
            m_heightContrast = 1;
            m_heightBrightness = 1;
            m_heightPosition = 0;

            //Displacement Remap
            m_displacementContrast = 1;
            m_displacementBrightness = 1;
            m_displacementPosition = 0;

        }

        /// <summary>
        /// Set values on the terrain layer based off a Unity Terrain Layer
        /// </summary>
        /// <param name="terrainLayer"></param>
        public void SetTerrainLayer(TerrainLayer terrainLayer)
        {
            if (terrainLayer == null)
            {
                Debug.LogError("Trying to set a null terrain layer, skipping this layer...");
                return;
            }

            m_terrainLayerReference = terrainLayer;
            m_name = terrainLayer.name;
            m_colorTint = terrainLayer.diffuseRemapMax;
            m_normalStrength = terrainLayer.normalScale;

            //Tiling Settings
            m_worldSpaceTiling = false;
            m_worldSpaceSize = new Vector2(5.0f, 5.0f);
            m_tileSize = new Vector2(terrainLayer.tileSize.x, terrainLayer.tileSize.y);
            m_tileOffset = new Vector2(terrainLayer.tileOffset.x, terrainLayer.tileOffset.y);

            m_maskMapRemapMin = terrainLayer.maskMapRemapMin;
            m_maskMapRemapMax = terrainLayer.maskMapRemapMax;

            //Textures
            m_diffuseMap = terrainLayer.diffuseTexture;
            m_normalMap = terrainLayer.normalMapTexture;
            m_maskMap = terrainLayer.maskMapTexture;

            if (m_diffuseMap != null)
            {
                m_name = terrainLayer.name + "_" + m_diffuseMap.name;
            }

            m_puddles = false;
            m_puddlesHeight = 0.1f;

            m_stochasticTiling = true;

            m_macroVariation = true;
            m_macroVariationColor = new Color(0.5f, 0.5f, 0.5f, 1.0f);

            //Detail Normal
            m_detailNormalNearScale = 10f;
            m_detailNormalFarScale = 1000f;

            //Height Remap
            m_heightContrast = 1;
            m_heightBrightness = 1;
            m_heightPosition = 0;

            //Displacement Remap
            m_displacementContrast = 1;
            m_displacementBrightness = 1;
            m_displacementPosition = 0;
        }

        /// <summary>
        /// Update values on the terrain layer through a Unity Terrain Layer
        /// </summary>
        /// <param name="terrainLayer"></param>
        public void UpdateTerrainLayer(TerrainLayer terrainLayer)
        {
            m_diffuseMap = terrainLayer.diffuseTexture;
            m_normalMap = terrainLayer.normalMapTexture;
            m_maskMap = terrainLayer.maskMapTexture;

            m_colorTint = terrainLayer.diffuseRemapMax;
            m_normalStrength = terrainLayer.normalScale;

            m_tileSize = new Vector2(terrainLayer.tileSize.x, terrainLayer.tileSize.y);
            m_tileOffset = new Vector2(terrainLayer.tileOffset.x, terrainLayer.tileOffset.y);

            m_maskMapRemapMin = terrainLayer.maskMapRemapMin;
            m_maskMapRemapMax = terrainLayer.maskMapRemapMax;
        }

        /// <summary>
        /// Pack layer toggles into a single bit
        /// Pack variables into single float4 values
        /// </summary>
        public void PackLayerSettings()
        {
            SetBoolean(0, m_worldSpaceTiling);
            SetBoolean(1, m_stochasticTiling);
            SetBoolean(2, m_puddles);
            SetBoolean(3, m_puddlesRandom);
            SetBoolean(4, m_macroVariation);
            SetBoolean(5, m_detailNormal);

            //clamp height so we don't get black values
            float clampedHeight = Mathf.Clamp(m_heightBlendFactor, 0, 0.96f);
            m_heightRemapPackedData = new Vector4(m_heightContrast, m_heightBrightness, m_heightPosition, Mathf.Clamp(1f - clampedHeight, 0.01f, 1.0f));
            m_stochasticHeightRemapPackedData = new Vector4(m_stochasticHeightContrast, m_stochasticHeightBrightness, m_stochasticHeightPosition, 1.0f - m_stochasticHeightBlendFactor);
            m_displacementRemapPackedData = new Vector4(m_displacementContrast, m_displacementBrightness, m_displacementPosition, 0);
            m_puddlesData = new Vector4(m_puddlesHeight, m_puddlesSmoothness, m_puddlesNormal, m_puddlesBlendFactor);
            m_detailNormalPackedData = new Vector4(m_detailNormalNearInfluence, m_detailNormalFarInfluence, m_detailNormalNearScale, m_detailNormalFarScale);
        }

        /// <summary>
        /// Pack a boolean into a single bit, based off its bit position
        /// </summary>
        /// <param name="position">Position in the bit to store this on and off state</param>
        /// <param name="value">Enabled state</param>
        public void SetBoolean(int position, bool value)
        {
            if (value)
            {
                m_bitPackedLayerSettings |= (1U << position);
            }
            else
            {
                m_bitPackedLayerSettings &= ~(1U << position);
            }
        }

        /// <summary>
        /// Return the boolean state from the packed bit
        /// </summary>
        /// <param name="position">Position of the boolean in the bit to retrieve</param>
        /// <returns></returns>
        public bool GetBoolean(int position)
        {
            return (m_bitPackedLayerSettings & (1U << position)) != 0;
        }

        /// <summary>
        /// Populate the terrain layer variables into the terrain tile layer struct type
        /// </summary>
        /// <param name="terrainSize"></param>
        public void PackToTerrainTileLayerStruct(float terrainSize)
        {
            m_terrainTileLayer.normalStrength = m_normalStrength;
            m_terrainTileLayer.remapMax = m_maskMapRemapMax;
            m_terrainTileLayer.remapMin = m_maskMapRemapMin;
            m_terrainTileLayer.tilingOffset = new Vector4(terrainSize / m_tileSize.x, terrainSize / m_tileSize.y, m_tileOffset.x, m_tileOffset.y);
            m_terrainTileLayer.worldSpaceSize = m_worldSpaceSize;
            m_terrainTileLayer.color = (Vector4)m_colorTint;
            m_terrainTileLayer.packedLayerSettings = m_bitPackedLayerSettings;
            m_terrainTileLayer.puddlesColor = (Vector4)m_puddlesColor;
            m_terrainTileLayer.puddlesData = m_puddlesData;
            m_terrainTileLayer.puddlesRandomMask = m_puddlesRandomMask;
            m_terrainTileLayer.macroVariationColor = (Vector4)m_macroVariationColor;
            m_terrainTileLayer.macroVariationMask = m_macroVariationMask;
            m_terrainTileLayer.heightPackedData = m_heightRemapPackedData;
            m_terrainTileLayer.displacementPackedData = m_displacementRemapPackedData;
            m_terrainTileLayer.stochasticPackedData = m_stochasticHeightRemapPackedData;
            m_terrainTileLayer.detailNormalPackedData = m_detailNormalPackedData;
        }

    }

}