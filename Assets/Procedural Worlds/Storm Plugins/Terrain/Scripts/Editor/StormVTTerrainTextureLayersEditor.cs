using ProceduralWorlds.Storm.StormEditor;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using static ProceduralWorlds.Storm.Terrain.StormVTEnums;


namespace ProceduralWorlds.Storm.Terrain
{
    [CustomEditor(typeof(StormVTTerrainTextureLayers))]
    public class StormVTTerrainTextureLayersEditor : StormVTEditor
    {

        /// <summary>
        /// Reference to the terrain texture layers object
        /// </summary>
        public StormVTTerrainTextureLayers terrainTextureLayers;

        /// <summary>
        /// Track the position of the mouse wheel scroll
        /// </summary>
        private Vector2 scrollPosition;

        /// <summary>
        /// A reorderable list for group individual terrain layers
        /// </summary>
        private ReorderableList stormVTTerrainLayersList;

        /// <summary>
        /// Toggle the layer settings panel
        /// </summary>
        public static bool m_textureLayerSettingsPanel = true;

        /// <summary>
        /// Toggle the functions panel
        /// </summary>
        public static bool m_functionsPanel = true;

        /// <summary>
        /// Checks whether the UI is in windowed mode
        /// </summary>
        public bool m_isInWindow = false;

        /// <summary>
        /// Boolean variables used for toggling individual channels of a texture
        /// 0 Toggles Red Channel
        /// 1 Toggles Green Channel
        /// 2 Toggles Blue Channel
        /// 3 Toggles Alpha Channel
        /// </summary>
        private bool[] m_puddleMaskToggles = new bool[4];

        /// <summary>
        /// Boolean variables used for toggling individual channels of a texture
        /// 0 Toggles Red Channel
        /// 1 Toggles Green Channel
        /// 2 Toggles Blue Channel
        /// 3 Toggles Alpha Channel
        /// </summary>
        private bool[] m_variationMaskToggles = new bool[4];

        /// <summary>
        /// Reference to the current Storm Terrain Texture Layer that is highlighted
        /// </summary>
        StormVTTerrainTextureLayer stormVTLayer;

        /// <summary>
        /// Style used for this window
        /// </summary>
        private GUIStyle m_boxStyle;

        /// <summary>
        /// Draw the header component of the layers
        /// </summary>
        /// <param name="rect"></param>
        private void OnDrawLayersHeader(Rect rect)
        {
            EditorGUI.LabelField(rect, "Storm Terrain Layers");
        }


        /// <summary>
        /// Draws all the elements of a Storm Terrain Layer
        /// </summary>
        /// <param name="rect"></param>
        /// <param name="index"></param>
        /// <param name="isActive"></param>
        /// <param name="isFocused"></param>
        private void OnDrawLayersElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            if (stormVTTerrainLayersList.count == 0)
                return;
            stormVTLayer = terrainTextureLayers.stormVTTerrainTextureLayers[index];

            // The 'level' property
            // The label field for level (width 100, height of a single line)
            EditorGUI.LabelField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), stormVTLayer.m_name);
            if (isActive)
            {
                int oldIndent = EditorGUI.indentLevel;

                m_puddleMaskToggles[0] = stormVTLayer.m_puddlesRandomMask.x == 1 ? true : false;
                m_puddleMaskToggles[1] = stormVTLayer.m_puddlesRandomMask.y == 1 ? true : false;
                m_puddleMaskToggles[2] = stormVTLayer.m_puddlesRandomMask.z == 1 ? true : false;
                m_puddleMaskToggles[3] = stormVTLayer.m_puddlesRandomMask.w == 1 ? true : false;

                m_variationMaskToggles[0] = stormVTLayer.m_macroVariationMask.x == 1 ? true : false;
                m_variationMaskToggles[1] = stormVTLayer.m_macroVariationMask.y == 1 ? true : false;
                m_variationMaskToggles[2] = stormVTLayer.m_macroVariationMask.z == 1 ? true : false;
                m_variationMaskToggles[3] = stormVTLayer.m_macroVariationMask.w == 1 ? true : false;

                EditorGUILayout.BeginVertical(StormVTStyles.gpanel);

                m_editorUtils.Panel("OverviewPanel", OverviewPanel, true);

                EditorGUILayout.EndVertical();
                EditorGUI.BeginChangeCheck();

                m_editorUtils.Panel("TexturesPanel", TexturesPanel, true);
                m_editorUtils.Panel("TilingPanel", TilingPanel, true);
                m_editorUtils.Panel("GeneralPanel", GeneralPanel, true);
                m_editorUtils.Panel("PuddlesPanel", PuddlesPanel, true);
                m_editorUtils.Panel("MacroVariationPanel", MacroVariationPanel, true);
                m_editorUtils.Panel("DetailNormalPanel", DetailNormalPanel, true);
                m_editorUtils.Panel("HeightBlendingPanel", HeightBlendingPanel, true);
                m_editorUtils.Panel("DisplacementPanel", DisplacementPanel, true);

                if (EditorGUI.EndChangeCheck())
                {

                    stormVTLayer.m_puddlesRandomMask.x = m_puddleMaskToggles[0] == true ? 1 : 0;
                    stormVTLayer.m_puddlesRandomMask.y = m_puddleMaskToggles[1] == true ? 1 : 0;
                    stormVTLayer.m_puddlesRandomMask.z = m_puddleMaskToggles[2] == true ? 1 : 0;
                    stormVTLayer.m_puddlesRandomMask.w = m_puddleMaskToggles[3] == true ? 1 : 0;

                    stormVTLayer.m_macroVariationMask.x = m_variationMaskToggles[0] == true ? 1 : 0;
                    stormVTLayer.m_macroVariationMask.y = m_variationMaskToggles[1] == true ? 1 : 0;
                    stormVTLayer.m_macroVariationMask.z = m_variationMaskToggles[2] == true ? 1 : 0;
                    stormVTLayer.m_macroVariationMask.w = m_variationMaskToggles[3] == true ? 1 : 0;

                    stormVTLayer.PackLayerSettings();
                    stormVTLayer.BroadcastTerrainTileLayerUpdated();
                    EditorUtility.SetDirty(terrainTextureLayers);
                    
                    StormVTPhysicalTextureManager physicalTextureManager = StormVTUtilities.FindObjectByType<StormVTPhysicalTextureManager>();
                    if(physicalTextureManager != null)
                    {
                        physicalTextureManager.RefreshTiles();
                        physicalTextureManager.m_stormVTTerrainTile.UpdateTerrainLayersBuffer();
                    }
                }

                
               

                EditorGUI.indentLevel = oldIndent;





            }
        }


        /// <summary>
        /// Overview panel
        /// </summary>
        /// <param name="helpActive"></param>
        private void OverviewPanel(bool helpActive)
        {
            EditorGUILayout.BeginVertical(m_boxStyle);

            EditorGUILayout.BeginVertical(StormVTStyles.gpanel);
            {
                EditorGUI.indentLevel = 0;
                {
                    stormVTLayer.m_name = m_editorUtils.TextField("LayerName", stormVTLayer.m_name, helpActive);
                    stormVTLayer.m_terrainLayerReference = (TerrainLayer)m_editorUtils.ObjectField("TerrainLayerReference", stormVTLayer.m_terrainLayerReference, typeof(TerrainLayer), false, helpActive);
                }
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.EndVertical();
        }

        /// <summary>
        /// Texture Settings panel
        /// </summary>
        /// <param name="helpActive"></param>
        private void TexturesPanel(bool helpActive)
        {
            EditorGUILayout.BeginVertical(m_boxStyle);
            {
                stormVTLayer.m_diffuseMap = (Texture2D)m_editorUtils.ObjectField("LayerAlbedoMapTexture", stormVTLayer.m_diffuseMap, typeof(Texture2D), false, helpActive, GUILayout.MaxHeight(16));
                stormVTLayer.m_normalMap = (Texture2D)m_editorUtils.ObjectField("LayerNormalMapTexture", stormVTLayer.m_normalMap, typeof(Texture2D), false, helpActive, GUILayout.MaxHeight(16));
                stormVTLayer.m_maskMap = (Texture2D)m_editorUtils.ObjectField("LayerMaskMapTexture", stormVTLayer.m_maskMap, typeof(Texture2D), false, helpActive, GUILayout.MaxHeight(16));

                stormVTLayer.m_packedDiffuseMap = (Texture2D)m_editorUtils.ObjectField("LayerPackedDiffuseMap", stormVTLayer.m_packedDiffuseMap, typeof(Texture2D), false, helpActive, GUILayout.MaxHeight(16));
                stormVTLayer.m_packedNormalMap = (Texture2D)m_editorUtils.ObjectField("LayerPackedNormalMap", stormVTLayer.m_packedNormalMap, typeof(Texture2D), false, helpActive, GUILayout.MaxHeight(16));

            }

            EditorGUILayout.EndVertical();
        }

        /// <summary>
        /// General settings panel
        /// </summary>
        /// <param name="helpActive"></param>
        private void GeneralPanel(bool helpActive)
        {
            EditorGUILayout.BeginVertical(m_boxStyle);

            {
                stormVTLayer.m_colorTint = m_editorUtils.ColorField("ColorTint", stormVTLayer.m_colorTint, helpActive);
                stormVTLayer.m_normalStrength = m_editorUtils.Slider("NormalStrength", stormVTLayer.m_normalStrength, 0.0f, 5.0f, helpActive);

                m_editorUtils.MinMaxSliderWithFields("LayerAmbientOcclusionMinMax", ref stormVTLayer.m_maskMapRemapMin.y, ref stormVTLayer.m_maskMapRemapMax.y, 0f, 1f, helpActive);
                m_editorUtils.MinMaxSliderWithFields("LayerSmoothnessMinMax", ref stormVTLayer.m_maskMapRemapMin.w, ref stormVTLayer.m_maskMapRemapMax.w, 0f, 1f, helpActive);
            }

            EditorGUILayout.EndVertical();
        }

        /// <summary>
        /// Tiling panel
        /// </summary>
        /// <param name="helpActive"></param>
        private void TilingPanel(bool helpActive)
        {
            EditorGUILayout.BeginVertical(m_boxStyle);
            {
                stormVTLayer.m_tileSize = m_editorUtils.Vector2Field("TileSize", stormVTLayer.m_tileSize, helpActive);
                stormVTLayer.m_tileOffset = m_editorUtils.Vector2Field("TileOffset", stormVTLayer.m_tileOffset, helpActive);

                stormVTLayer.m_worldSpaceTiling = m_editorUtils.Toggle("WorldSpaceTiling", stormVTLayer.m_worldSpaceTiling, helpActive);
                if (stormVTLayer.m_worldSpaceTiling)
                {
                    GUI.enabled = true;
                }
                else
                {
                    GUI.enabled = false;
                }
                EditorGUI.indentLevel++;
                stormVTLayer.m_worldSpaceSize = m_editorUtils.Vector2Field("WorldSpaceSize", stormVTLayer.m_worldSpaceSize, helpActive);
                EditorGUI.indentLevel--;

                GUI.enabled = true;

                stormVTLayer.m_stochasticTiling = m_editorUtils.Toggle("SeamlessTiling", stormVTLayer.m_stochasticTiling, helpActive);
                if(stormVTLayer.m_stochasticTiling)
                {
                    GUI.enabled = true;
                }
                else
                {
                    GUI.enabled = false;
                }
                {
                    EditorGUI.indentLevel++;
                    float labelWidthBefore = EditorGUIUtility.labelWidth;
                    EditorGUIUtility.labelWidth = 250f;
                    stormVTLayer.m_stochasticHeightContrast = m_editorUtils.Slider("StochasticHeightContrast", stormVTLayer.m_stochasticHeightContrast, 0f, 5f, helpActive);
                    stormVTLayer.m_stochasticHeightBrightness = m_editorUtils.Slider("StochasticHeightBrightness", stormVTLayer.m_stochasticHeightBrightness, 0f, 5f, helpActive);
                    stormVTLayer.m_stochasticHeightPosition = m_editorUtils.Slider("StochasticHeightPosition", stormVTLayer.m_stochasticHeightPosition, -1f, 1f, helpActive);
                    stormVTLayer.m_stochasticHeightBlendFactor = m_editorUtils.Slider("StochasticHeightBlendFactor", stormVTLayer.m_stochasticHeightBlendFactor, 0f, 1f, helpActive);
                    EditorGUIUtility.labelWidth = labelWidthBefore;
                    EditorGUI.indentLevel--;
                }
                GUI.enabled = true;

            }

            EditorGUILayout.EndVertical();
        }

        /// <summary>
        /// Puddles panel
        /// </summary>
        /// <param name="helpActive"></param>
        private void PuddlesPanel(bool helpActive)
        {
            EditorGUILayout.BeginVertical(m_boxStyle);

            {
                stormVTLayer.m_puddles = m_editorUtils.Toggle("Puddles", stormVTLayer.m_puddles, helpActive);
                if(stormVTLayer.m_puddles)
                {
                    GUI.enabled = true;
                }
                else
                {
                    GUI.enabled = false;
                }
                {
                    stormVTLayer.m_puddlesColor = m_editorUtils.ColorField("PuddlesColor", stormVTLayer.m_puddlesColor, helpActive);
                    stormVTLayer.m_puddlesColor.a = m_editorUtils.Slider("PuddlesColorInfluence", stormVTLayer.m_puddlesColor.a, 0f, 1f, helpActive);
                    stormVTLayer.m_puddlesHeight = m_editorUtils.Slider("PuddlesHeight", stormVTLayer.m_puddlesHeight, 0f, 1f, helpActive);
                    stormVTLayer.m_puddlesSmoothness = m_editorUtils.Slider("PuddlesSmoothness", stormVTLayer.m_puddlesSmoothness, 0f, 1f, helpActive);
                    stormVTLayer.m_puddlesNormal = m_editorUtils.Slider("PuddlesNormal", stormVTLayer.m_puddlesNormal, 0f, 1f, helpActive);
                    stormVTLayer.m_puddlesBlendFactor = m_editorUtils.Slider("PuddlesBlendFactor", stormVTLayer.m_puddlesBlendFactor, 0f, 1f, helpActive);

                    stormVTLayer.m_puddlesRandom = m_editorUtils.Toggle("PuddlesRandom", stormVTLayer.m_puddlesRandom, helpActive);

                    if(stormVTLayer.m_puddles && stormVTLayer.m_puddlesRandom)
                    {
                        GUI.enabled = true;
                    }
                    else
                    {
                        GUI.enabled = false;
                    }
                    {
                        m_editorUtils.LabelField("PuddlesRandomMask", helpActive);
                        EditorGUI.indentLevel++;
                        m_puddleMaskToggles[0] = m_editorUtils.ToggleLeft("Red", m_puddleMaskToggles[0], helpActive);
                        m_puddleMaskToggles[1] = m_editorUtils.ToggleLeft("Green", m_puddleMaskToggles[1], helpActive);
                        m_puddleMaskToggles[2] = m_editorUtils.ToggleLeft("Blue", m_puddleMaskToggles[2], helpActive);
                        m_puddleMaskToggles[3] = m_editorUtils.ToggleLeft("Alpha", m_puddleMaskToggles[3], helpActive);
                        EditorGUI.indentLevel--;
                    }
                }
                GUI.enabled = true;
            }

            EditorGUILayout.EndVertical();
        }

        /// <summary>
        /// Macro variation panel
        /// </summary>
        /// <param name="helpActive"></param>
        private void MacroVariationPanel(bool helpActive)
        {
            EditorGUILayout.BeginVertical(m_boxStyle);

            {
                stormVTLayer.m_macroVariation = m_editorUtils.Toggle("MacroVariation", stormVTLayer.m_macroVariation, helpActive);
                {
                    stormVTLayer.m_macroVariationColor = m_editorUtils.ColorField("MacroVariationColor", stormVTLayer.m_macroVariationColor, helpActive);
                    stormVTLayer.m_macroVariationColor.a = m_editorUtils.Slider("MacroVariationColorInfluence", stormVTLayer.m_macroVariationColor.a, 0f, 1f, helpActive);

                    m_editorUtils.LabelField("MacroVariationMask", helpActive);
                    EditorGUI.indentLevel++;
                    m_variationMaskToggles[0] = m_editorUtils.ToggleLeft("Red", m_variationMaskToggles[0], helpActive);
                    m_variationMaskToggles[1] = m_editorUtils.ToggleLeft("Green", m_variationMaskToggles[1], helpActive);
                    m_variationMaskToggles[2] = m_editorUtils.ToggleLeft("Blue", m_variationMaskToggles[2], helpActive);
                    m_variationMaskToggles[3] = m_editorUtils.ToggleLeft("Alpha", m_variationMaskToggles[3], helpActive);
                    EditorGUI.indentLevel--;
                }
            }

            EditorGUILayout.EndVertical();
        }

        /// <summary>
        /// Detail normal panel
        /// </summary>
        /// <param name="helpActive"></param>
        private void DetailNormalPanel(bool helpActive)
        {
            EditorGUILayout.BeginVertical(m_boxStyle);

            {
                stormVTLayer.m_detailNormal = m_editorUtils.Toggle("DetailNormal", stormVTLayer.m_detailNormal, helpActive);
                {
                    stormVTLayer.m_detailNormalNearInfluence = m_editorUtils.Slider("DetailNormalNearInfluence", stormVTLayer.m_detailNormalNearInfluence, 0f, 1f, helpActive);
                    stormVTLayer.m_detailNormalFarInfluence = m_editorUtils.Slider("DetailNormalFarInfluence", stormVTLayer.m_detailNormalFarInfluence, 0f, 1f, helpActive);
                }
            }

            EditorGUILayout.EndVertical();
        }

        /// <summary>
        /// Height blending panel
        /// </summary>
        /// <param name="helpActive"></param>
        private void HeightBlendingPanel(bool helpActive)
        {
            EditorGUILayout.BeginVertical(m_boxStyle);

            {
                stormVTLayer.m_heightContrast = m_editorUtils.Slider("HeightContrast", stormVTLayer.m_heightContrast, 0f, 5f, helpActive);
                stormVTLayer.m_heightBrightness = m_editorUtils.Slider("HeightBrightness", stormVTLayer.m_heightBrightness, 0f, 5f, helpActive);
                stormVTLayer.m_heightPosition = m_editorUtils.Slider("HeightPosition", stormVTLayer.m_heightPosition, -1f, 1f, helpActive);
            }

            EditorGUILayout.EndVertical();
        }

        /// <summary>
        /// Displacement panel
        /// </summary>
        /// <param name="helpActive"></param>
        private void DisplacementPanel(bool helpActive)
        {
            EditorGUILayout.BeginVertical(m_boxStyle);

            {
                bool currentGUIState = GUI.enabled;
                if (!m_clipmapOn)
                {
                    GUI.enabled = false;
                }

                stormVTLayer.m_displacementContrast = m_editorUtils.Slider("DisplacementContrast", stormVTLayer.m_displacementContrast, 0f, 5f, helpActive);
                stormVTLayer.m_displacementBrightness = m_editorUtils.Slider("DisplacementBrightness", stormVTLayer.m_displacementBrightness, 0f, 5f, helpActive);
                stormVTLayer.m_displacementPosition = m_editorUtils.Slider("DisplacementPosition", stormVTLayer.m_displacementPosition, -1f, 1f, helpActive);

                GUI.enabled = currentGUIState;
            }

            EditorGUILayout.EndVertical();
        }

        /// <summary>
        /// Functions panel
        /// </summary>
        /// <param name="helpActive"></param>
        private void FunctionsPanel(bool helpActive)
        {
            EditorGUILayout.BeginVertical(m_boxStyle);

            if (m_editorUtils.Button("RefreshTerrainLayers", helpActive))
            {
                terrainTextureLayers.RefreshTerrainLayers();
                EditorUtility.SetDirty(terrainTextureLayers);
            }

            if (m_editorUtils.Button("UpdateTerrainLayers", helpActive))
            {
                for (int i = 0; i < terrainTextureLayers.stormVTTerrainTextureLayers.Count; i++)
                {
                    if (terrainTextureLayers.stormVTTerrainTextureLayers[i] == null)
                    {
                        Debug.LogError($"Storm Terrain Texture Layer at index {i} seems to be missing, please check the Terrain Texture Layer file for empty entries!");
                        continue;
                    }

                    if (terrainTextureLayers.stormVTTerrainTextureLayers[i].m_terrainLayerReference == null)
                    {
                        Debug.LogError($"The Terrain Texture Layer at index {i}, Name '{terrainTextureLayers.stormVTTerrainTextureLayers[i].m_name}' has a missing terrain texture layer file assigned! Please assign the matching layer file so that the file stays in sync with the actual layers used on the terrains.");
                        continue;
                    }

                    terrainTextureLayers.stormVTTerrainTextureLayers[i].UpdateTerrainLayer(terrainTextureLayers.stormVTTerrainTextureLayers[i].m_terrainLayerReference);
                }
                EditorUtility.SetDirty(terrainTextureLayers);
            }

            if (m_editorUtils.Button("ExportTerrainTexturesToPackedFormat", helpActive))
            {
                terrainTextureLayers.ConvertTexturesToPackedFormat();
                EditorUtility.SetDirty(terrainTextureLayers);
            }

            if(m_editorUtils.Button("ExportTerrainTexturesToTexture2DArray", helpActive))
            {
                terrainTextureLayers.ConvertLayersToTexture2DArray();
                EditorUtility.SetDirty(terrainTextureLayers);
            }

            terrainTextureLayers.albedoArray = (Texture2DArray)m_editorUtils.ObjectField("AlbedoArray", terrainTextureLayers.albedoArray, typeof(Texture2DArray), false, helpActive, GUILayout.MaxHeight(16));
            terrainTextureLayers.normalArray = (Texture2DArray)m_editorUtils.ObjectField("NormalArray", terrainTextureLayers.normalArray, typeof(Texture2DArray), false, helpActive, GUILayout.MaxHeight(16));

            EditorGUILayout.EndVertical();
        }

        
        /// <summary>
        /// Redraw the storm terrain texture layers
        /// </summary>
        public void RefreshStormLayersList()
        {
            if (terrainTextureLayers == null)
                return;

            stormVTTerrainLayersList = new ReorderableList(terrainTextureLayers.stormVTTerrainTextureLayers, typeof(StormVTTerrainTextureLayer), false, true, false, false);
            stormVTTerrainLayersList.drawHeaderCallback = OnDrawLayersHeader;
            stormVTTerrainLayersList.drawElementCallback = OnDrawLayersElement;
        }

        /// <summary>
        /// Redraw the storm terrain texture layers
        /// </summary>
        public void OnEnable()
        {
            if(!m_isInWindow)
            {
                terrainTextureLayers = (StormVTTerrainTextureLayers)target;
                RefreshStormLayersList();
                if (m_editorUtils == null)
                    m_editorUtils = StormApp.GetEditorUtils(this);
                RefreshStormLayersList();
            }


        }

        /// <summary>
        /// Grab a reference to the texture layers target
        /// </summary>
        public void OnStart()
        {
            terrainTextureLayers = (StormVTTerrainTextureLayers)target;
        }

        /// <summary>
        /// Draw the GUI
        /// </summary>
        public override void OnInspectorGUI()
        {
            terrainTextureLayers = (StormVTTerrainTextureLayers)target;
            //Set up the box style
            if (m_boxStyle == null)
            {
                m_boxStyle = new GUIStyle(GUI.skin.box);
                m_boxStyle.normal.textColor = GUI.skin.label.normal.textColor;
                m_boxStyle.fontStyle = FontStyle.Bold;
                m_boxStyle.alignment = TextAnchor.UpperLeft;
            }

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            base.OnInspectorGUI();
            m_editorUtils.Initialize();
            GUI.enabled = true;
            if (!m_isInWindow)
                m_editorUtils.GUIHeader();

            FetchStormSettings();

            m_textureLayerSettingsPanel = m_editorUtils.Panel("TextureLayerSettingsPanel", TextureLayerSettingsPanel, m_textureLayerSettingsPanel);
            m_editorUtils.Panel("FunctionsPanel", FunctionsPanel, true);

            EditorUtility.SetDirty(terrainTextureLayers);

            if (GUI.changed)
            {
                MarkProfileDirty();
            }

            EditorGUILayout.EndScrollView();
        }

        public void FetchStormSettings()
        {
            if(FindStormTerrainManagerInScene())
            {
                if(stormVTManager.stormVTSettings != null)
                {
                    m_clipmapOn = stormVTManager.stormVTSettings.m_stormVTMode == StormVTMode.Clipmap;
                }
            }
            else
            {
                StormVTSettings stormVTSettings = null;
#if UNITY_EDITOR
                string vtSettingsFilePath = AssetDatabase.GetAssetPath(this).Replace("_TerrainTextureLayers.asset", "_StormVTSettings.asset");

                stormVTSettings = AssetDatabase.LoadAssetAtPath<StormVTSettings>(vtSettingsFilePath);
                if(stormVTSettings != null)
                {
                    m_clipmapOn = stormVTSettings.m_stormVTMode == StormVTMode.Clipmap;
                }

#endif
            }
        }

        StormVTManager stormVTManager;
        bool m_clipmapOn;

        public bool FindStormTerrainManagerInScene()
        {
            if (stormVTManager != null)
            {
                return true;
            }
            else
            {
                stormVTManager = StormVTUtilities.FindObjectByType<StormVTManager>();
                if (stormVTManager != null)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

        }

        /// <summary>
        /// Set this object as dirty
        /// </summary>
        public void MarkProfileDirty()
        {
            EditorUtility.SetDirty(target);
        }

        /// <summary>
        /// Texture layer settings panel
        /// </summary>
        /// <param name="helpEnabled"></param>
        public void TextureLayerSettingsPanel(bool helpEnabled)
        {
            EditorGUI.indentLevel++;
            {               
                EditorGUILayout.Space();
                stormVTTerrainLayersList.DoLayoutList();
            }
            EditorGUI.indentLevel--;
        }
    }
}