using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using static ProceduralWorlds.Storm.Terrain.StormVTEnums;

namespace ProceduralWorlds.Storm.Terrain
{
    [CustomEditor(typeof(StormVTSettings))]
    public class StormVTSettingsEditor : Editor
    {

        /// <summary>
        /// Reference to the storm terrain settings scriptable object
        /// </summary>
        StormVTSettings stormVTSettings;

        /// <summary>
        /// Grab a reference to the settings script target
        /// Set textures if missing
        /// </summary>
        public void OnEnable()
        {
            stormVTSettings = (StormVTSettings)target;
            UpdateTextures();
        }

        /// <summary>
        /// Grab a reference to the settings script target
        /// Set textures if missing
        /// </summary>
        public void Start()
        {
            stormVTSettings = (StormVTSettings)target;
            UpdateTextures();
        }

        /// <summary>
        /// Draw the inspector normally
        /// </summary>
        public override void OnInspectorGUI()
        {
            stormVTSettings = (StormVTSettings)target;
            var stormMode = stormVTSettings.m_stormVTMode;
            EditorGUI.BeginChangeCheck();

            DrawDefaultInspector();
            if (EditorGUI.EndChangeCheck())
            {
                if(stormVTSettings.m_stormVTMode != stormMode)
                {
                    if(stormVTSettings.m_stormVTMode == StormVTMode.Clipmap)
                    {

                        if (EditorUtility.DisplayDialog("Extract Terrain Textures",
                        "The clipmap system requires loading in textures that are extracted from Unity Terrains. " +
                        "Would you like to perform the process to extract these textures now? (This process can also be performed from the Storm Terrain object in the scene)",
                        "Yes",
                        "No"))
                        {
                            GameObject vtPrefab = GameObject.Find("Storm Terrain");
                            if (vtPrefab != null)
                            {
                                StormVTManager stormVTManager = vtPrefab.GetComponentInChildren<StormVTManager>();
                                if(stormVTManager != null)
                                {
                                    stormVTManager.StartTerrainDataBulkExportEditorUpdates();
                                }
                            }
                                
                        }

                    }
                }
                RegenerateTiles();
            }

            if (GUILayout.Button("Regenerate Tiles"))
            {
                RegenerateTiles();
            }

            stormVTSettings.RecomputeData();
            stormVTSettings.ComputeConstants();
            ComputeConstants();
            UpdateTextures();
        }

        /// <summary>
        /// Force tiles to regenerate
        /// </summary>
        private void RegenerateTiles()
        {

            //find the physcial texture manager
            List<StormVTPhysicalTextureManager> physicalTextureManager = StormVTUtilities.FindObjectsByType<StormVTPhysicalTextureManager>(false).ToList();
            if (physicalTextureManager.Count > 0)
            {
                physicalTextureManager[0].RefreshTiles();
            }
        }

        /// <summary>
        /// Changes a keyword state on a material
        /// </summary>
        /// <param name="mat">Material to</param>
        /// <param name="keywordID"></param>
        /// <param name="keywordName"></param>
        /// <param name="enabled"></param>
        private void SetKeyword(Material mat, int keywordID, string keywordName, bool enabled)
        {

            mat.SetInt(keywordID, enabled ? 1 : 0);
            if (enabled)
                mat.EnableKeyword(keywordName);
            else
                mat.DisableKeyword(keywordName);
        }

        /// <summary>
        /// Compute constant data for the terrain
        /// </summary>
        public void ComputeConstants()
        {
#if UNITY_EDITOR
            Material stormVTMaterial = stormVTSettings.m_stormTerrainMaterial;
            Material stormVTMaterialClipmap = stormVTSettings.m_stormTerrainClipmapMaterial;

            if (stormVTMaterial != null)
            {
                //Snow settings
                if (stormVTSettings.m_Snow)
                {
                    if (stormVTSettings.m_SnowGenerationMode == StormVTFeatureMode._Runtime)
                    {
                        SetKeyword(stormVTMaterial, StormVTShaderIDs.m_stormVT_SnowTerrain, StormVTShaderIDs.m_stormVT_SnowTerrainOn, true);
                        SetKeyword(stormVTMaterialClipmap, StormVTShaderIDs.m_stormVT_SnowTerrain, StormVTShaderIDs.m_stormVT_SnowTerrainOn, true);
                    }
                    else
                    {
                        SetKeyword(stormVTMaterial, StormVTShaderIDs.m_stormVT_SnowTerrain, StormVTShaderIDs.m_stormVT_SnowTerrainOn, false);
                        SetKeyword(stormVTMaterialClipmap, StormVTShaderIDs.m_stormVT_SnowTerrain, StormVTShaderIDs.m_stormVT_SnowTerrainOn, false);
                    }

                    stormVTSettings.SetSnowVariables(stormVTMaterial);
                    stormVTSettings.SetSnowVariables(stormVTMaterialClipmap);
                }
                else
                {
                    SetKeyword(stormVTMaterial, StormVTShaderIDs.m_stormVT_SnowTerrain, StormVTShaderIDs.m_stormVT_SnowTerrainOn, false);
                    SetKeyword(stormVTMaterialClipmap, StormVTShaderIDs.m_stormVT_SnowTerrain, StormVTShaderIDs.m_stormVT_SnowTerrainOn, false);
                }

                SetKeyword(stormVTMaterialClipmap, StormVTShaderIDs.m_stormVT_DisplacementTerrain, StormVTShaderIDs.m_stormVT_DisplacementTerrainOn, stormVTSettings.m_displacement);
                SetKeyword(stormVTMaterial, StormVTShaderIDs. m_stormVT_DeviceTargetKeyword, StormVTShaderIDs.m_stormVT_DeviceTargetKeywordOn, stormVTSettings.m_targetPlatform == StormVTPlatform.Low_End_Mobile_VR ? true : false);
                SetKeyword(stormVTMaterialClipmap, StormVTShaderIDs. m_stormVT_DeviceTargetKeyword, StormVTShaderIDs.m_stormVT_DeviceTargetKeywordOn, stormVTSettings.m_targetPlatform == StormVTPlatform.Low_End_Mobile_VR ? true : false);
            }
#endif
        }

        /// <summary>
        /// Set textures on the storm terrain object if null
        /// </summary>
        public void UpdateTextures()
        {

#if UNITY_EDITOR
            if (stormVTSettings.m_SnowAlbedo == null)
            {
                stormVTSettings.m_SnowAlbedo = StormVTDefaults.Instance.snowDiffuse;
            }
            if (stormVTSettings.m_SnowNormal == null)
            {
                stormVTSettings.m_SnowNormal = StormVTDefaults.Instance.snowNormal;
            }
            if (stormVTSettings.m_SnowMask == null)
            {
                stormVTSettings.m_SnowMask = StormVTDefaults.Instance.snowMask;
            }
            if (stormVTSettings.m_macroVariationTexture == null)
            {
                stormVTSettings.m_macroVariationTexture = StormVTDefaults.Instance.macroVariation;
            }
            if (stormVTSettings.m_detailNormalTexture == null)
            {
                stormVTSettings.m_detailNormalTexture = StormVTDefaults.Instance.detailNormal;
            }
#endif
        }
    }
}


