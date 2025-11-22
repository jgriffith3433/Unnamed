using UnityEditor;
using UnityEngine;
using PWCommon5;
using UnityEditor.AnimatedValues;
using ProceduralWorlds.Storm.Utils;
using ProceduralWorlds.Storm.Core;
using Mono.Cecil;
using System.Data;

/// Copyright 2023 Procedural Worlds Ltd
/// Written by Jonathan Williams as part of Storm

namespace ProceduralWorlds.Storm.StormEditor
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(StormFeatureSettings))]
    public class StormFeatureSettingsEditor : PWEditor
    {
        EditorUtils m_editorUtils;

        SerializedProperty p_ObjectType;
        SerializedProperty p_SourceType;
        SerializedProperty p_SourcePrefab;
        SerializedProperty p_SourceTerrain;
        SerializedProperty p_SourceDetailIndex;
        SerializedProperty p_Jitter;
        SerializedProperty p_Density;
        SerializedProperty p_DensityFadeStrength;
        SerializedProperty p_DensityFadeStart;
        SerializedProperty p_DensityFadeScaleoutRange;
        SerializedProperty p_SplatCutoff;
        SerializedProperty p_SpawnChance;
        SerializedProperty p_SplatMaxValue;
        SerializedProperty p_LimitMinHeight;
        SerializedProperty p_IgnoreSeaLevel;
        SerializedProperty p_LimitMaxHeight;
        SerializedProperty p_MinHeight;
        SerializedProperty p_MaxHeight;
        SerializedProperty p_MinScale;
        SerializedProperty p_MaxScale;
        SerializedProperty p_MinScaleRandom;
        SerializedProperty p_MaxScaleRandom;
        SerializedProperty p_LimitMinSlope;
        SerializedProperty p_LimitMaxSlope;
        SerializedProperty p_MinSlope;
        SerializedProperty p_MaxSlope;
        SerializedProperty p_useScaleBySplat;
        SerializedProperty p_ScaleBySplat;
        SerializedProperty p_useScaleByHeight;
        SerializedProperty p_ScaleByHeight;
        SerializedProperty p_useScaleBySlope;
        SerializedProperty p_ScaleBySlope;
        SerializedProperty p_RotBase;
        SerializedProperty p_RotRange;
        SerializedProperty p_AlignToUp;
        SerializedProperty p_SlopeCheckSize;
        SerializedProperty p_AlignForwardToSlope;
        SerializedProperty p_SlopeType;
        SerializedProperty p_SlopeNoiseType;
        SerializedProperty p_SlopeNoiseSize;
        SerializedProperty p_SlopeNoiseLacunarity;
        SerializedProperty p_SlopeNoisePersistence;
        SerializedProperty p_SlopeNoiseMult;
        SerializedProperty p_SlopeNoiseMod;
        SerializedProperty p_SlopeNoiseOctaves;
        SerializedProperty p_SlopeNoise2Type;
        SerializedProperty p_SlopeNoise2Method;
        SerializedProperty p_SlopeNoise2Size;
        SerializedProperty p_SlopeNoise2Lacunarity;
        SerializedProperty p_SlopeNoise2Persistence;
        SerializedProperty p_SlopeNoise2Mult;
        SerializedProperty p_SlopeNoise2Mod;
        SerializedProperty p_SlopeNoise2Octaves;
        SerializedProperty p_RandomSeed;
        SerializedProperty p_ScaleNoiseType;
        SerializedProperty p_ScaleNoiseSize;
        SerializedProperty p_ScaleNoiseLacunarity;
        SerializedProperty p_ScaleNoisePersistence;
        SerializedProperty p_ScaleNoiseMult;
        SerializedProperty p_ScaleNoiseMod;
        SerializedProperty p_ScaleNoiseOctaves;
        SerializedProperty p_ScaleNoise2Type;
        SerializedProperty p_ScaleNoise2Method;
        SerializedProperty p_ScaleNoise2Size;
        SerializedProperty p_ScaleNoise2Lacunarity;
        SerializedProperty p_ScaleNoise2Persistence;
        SerializedProperty p_ScaleNoise2Mult;
        SerializedProperty p_ScaleNoise2Mod;
        SerializedProperty p_ScaleNoise2Octaves;
        SerializedProperty p_ColorRandomization;
        SerializedProperty p_ColorTransition;
        SerializedProperty p_ColorAPropName;
        SerializedProperty p_ColorBPropName;
        SerializedProperty p_ColorA;
        SerializedProperty p_ColorB;
        SerializedProperty p_DisableRender;
        SerializedProperty p_RenderType;
        SerializedProperty p_CombineType;
        SerializedProperty p_LODSplits;
        SerializedProperty p_LODFade;
        SerializedProperty p_LODMix;
        SerializedProperty p_LODs;
        SerializedProperty p_ShadowsUseLOD;
        SerializedProperty p_customShadowRenderer;
        SerializedProperty p_RenderOffset;
        SerializedProperty p_RenderRotation;
        SerializedProperty p_RenderScale;
        SerializedProperty p_DrawLongShadows;
        SerializedProperty p_MaxLongShadowDist;
        SerializedProperty p_ScaleoutRange;
        SerializedProperty p_ScaleoutMinScale;
        SerializedProperty p_Colliders;

        AnimBool m_showScaleNoise;
        AnimBool m_showSlopeNoise;
        AnimBool m_showSlopeNoiseSettings;
        AnimBool m_showScaleNoise2;
        AnimBool m_showSlopeNoise2Settings;

        AnimBool m_lodFoldout;
        AnimBool m_shadowLODFoldout;
        AnimBool m_shadowLODItemFoldout;
        AnimBool[] m_shadowLODRenderGroupFoldouts = new AnimBool[0];
        AnimBool[] m_lodItemFoldouts = new AnimBool[0];
        AnimBool[][] m_lodRenderGroupFoldouts = new AnimBool[0][];

        int m_shadowCopyFromLOD = 0;

        bool m_needUpdateGeneration = false;
        bool m_needUpdateLOD = false;
        bool m_needUpdateRender = false;

        bool m_showScaleHelp = false;

        int m_removeLOD = -1;
        Vector2Int m_removeRenderData = new Vector2Int(-1, -1);

        MeshPreview m_preview;

        const int previewResolution = 128;
        Vector2 previewScaleOffset;
        Vector2 previewSlopeOffset;
        float previewScaleRange = 50;
        float previewSlopeRange = 50;

        bool isDraggingPreviewScale = false;
        bool isDraggingPreviewSlope = false;

        AnimBool m_showPreviewScaleNoise;
        Texture2D m_previewScaleNoise;
        int last_ScaleNoiseSeed;
        int last_ScaleNoiseType;
        float last_ScaleNoiseMod;
        float last_ScaleNoiseMult;
        float last_ScaleNoiseSize;
        float last_ScaleNoiseLacunarity;
        float last_ScaleNoisePersistence;
        int last_ScaleNoiseOctaves;
        int last_ScaleNoise2Type;
        int last_ScaleNoise2Method;
        float last_ScaleNoise2Mod;
        float last_ScaleNoise2Mult;
        float last_ScaleNoise2Size;
        float last_ScaleNoise2Lacunarity;
        float last_ScaleNoise2Persistence;
        int last_ScaleNoise2Octaves;
        Vector2 last_PreviewScaleOffset;
        float last_PreviewScaleRange;

        AnimBool m_showPreviewSlopeNoise;
        Texture2D m_previewSlopeNoise;
        int last_SlopeNoiseSeed;
        int last_SlopeNoiseType;
        float last_SlopeNoiseMod;
        float last_SlopeNoiseMult;
        float last_SlopeNoiseSize;
        float last_SlopeNoiseLacunarity;
        float last_SlopeNoisePersistence;
        int last_SlopeNoiseOctaves;
        int last_SlopeNoise2Type;
        int last_SlopeNoise2Method;
        float last_SlopeNoise2Mod;
        float last_SlopeNoise2Mult;
        float last_SlopeNoise2Size;
        float last_SlopeNoise2Lacunarity;
        float last_SlopeNoise2Persistence;
        int last_SlopeNoise2Octaves;
        Vector2 last_PreviewSlopeOffset;
        float last_PreviewSlopeRange;
        float m_displayMinRotX;
        float m_displayMinRotY;
        float m_displayMinRotZ;
        float m_displayMaxRotX;
        float m_displayMaxRotY;
        float m_displayMaxRotZ;
        Vector2 m_jitterYminMax = new Vector2(-5, 5f);


        //Generation Settings Foldouts
        bool m_showSeedSettings;
        bool m_showInstanceGeneratorSettings = true;
        bool m_showInstanceModifierSettings = true;
        bool m_showGridSettings;
        bool m_showSpawnSettings;
        bool m_showNoiseSettings;
        bool m_showScaleSettings;
        bool m_showRotationSettings;
        bool m_showColorVariationSettings;

        //Render Settings Foldouts
        bool m_showRenderingSettings;
        bool m_showLODSettings;
        bool m_showColorVariationSettings2;
        bool m_showDensityFadingSettings;
        bool m_showShadowSettings;
        bool m_showScaleoutSettings;
        bool m_showCollisionSettings;


        StormFeatureSettings m_featureSettings;
        GUIContent RandomizeButtonContent;
        private Texture2D m_previewTexture;

        bool m_allowEditSource = false;

        private void OnEnable()
        {
            RandomizeButtonContent = EditorGUIUtility.IconContent("d_RotateTool On@2x");
            p_ObjectType = serializedObject.FindProperty("ObjectType");
            p_SourceType = serializedObject.FindProperty("SourceType");
            p_SourcePrefab = serializedObject.FindProperty("SourcePrefab");
            p_SourceTerrain = serializedObject.FindProperty("SourceTerrain");
            p_SourceDetailIndex = serializedObject.FindProperty("SourceDetailIndex");
            p_Jitter = serializedObject.FindProperty("PositionJitter");
            p_Density = serializedObject.FindProperty("Density");
            p_DensityFadeStrength = serializedObject.FindProperty("DensityFadeStrength");
            p_DensityFadeStart = serializedObject.FindProperty("DensityFadeStart");
            p_DensityFadeScaleoutRange = serializedObject.FindProperty("DensityFadeScaleoutRange");
            p_SplatCutoff = serializedObject.FindProperty("SplatCutoff");
            p_SpawnChance = serializedObject.FindProperty("SpawnChance");
            p_SplatMaxValue = serializedObject.FindProperty("SplatMaxValue");
            p_LimitMinHeight = serializedObject.FindProperty("LimitMinHeight");
            p_IgnoreSeaLevel = serializedObject.FindProperty("IgnoreSeaLevel");
            p_LimitMaxHeight = serializedObject.FindProperty("LimitMaxHeight");
            p_MinHeight = serializedObject.FindProperty("MinHeight");
            p_MaxHeight = serializedObject.FindProperty("MaxHeight");
            p_MinScale = serializedObject.FindProperty("MinScale");
            p_MaxScale = serializedObject.FindProperty("MaxScale");
            p_MinScaleRandom = serializedObject.FindProperty("RandomScaleMin");
            p_MaxScaleRandom = serializedObject.FindProperty("RandomScaleMax");
            p_LimitMinSlope = serializedObject.FindProperty("LimitMinSlope");
            p_LimitMaxSlope = serializedObject.FindProperty("LimitMaxSlope");
            p_MinSlope = serializedObject.FindProperty("MinSlope");
            p_MaxSlope = serializedObject.FindProperty("MaxSlope");
            p_useScaleBySplat = serializedObject.FindProperty("useScaleBySplat");
            p_ScaleBySplat = serializedObject.FindProperty("ScaleBySplat");
            p_useScaleByHeight = serializedObject.FindProperty("useScaleByHeight");
            p_ScaleByHeight = serializedObject.FindProperty("ScaleByHeight");
            p_useScaleBySlope = serializedObject.FindProperty("useScaleBySlope");
            p_ScaleBySlope = serializedObject.FindProperty("ScaleBySlope");
            p_RotBase = serializedObject.FindProperty("RotBases");
            p_RotRange = serializedObject.FindProperty("RotRanges");
            p_AlignToUp = serializedObject.FindProperty("AlignToUp");
            p_SlopeCheckSize = serializedObject.FindProperty("SlopeCheckSize");
            p_AlignForwardToSlope = serializedObject.FindProperty("AlignForwardToSlope");
            p_SlopeType = serializedObject.FindProperty("SlopeType");
            p_SlopeNoiseType = serializedObject.FindProperty("SlopeNoiseType");
            p_SlopeNoiseSize = serializedObject.FindProperty("SlopeNoiseSize");
            p_SlopeNoiseLacunarity = serializedObject.FindProperty("SlopeNoiseLacunarity");
            p_SlopeNoisePersistence = serializedObject.FindProperty("SlopeNoisePersistence");
            p_SlopeNoiseMult = serializedObject.FindProperty("SlopeNoiseMult");
            p_SlopeNoiseMod = serializedObject.FindProperty("SlopeNoiseMod");
            p_SlopeNoiseOctaves = serializedObject.FindProperty("SlopeNoiseOctaves");
            p_SlopeNoise2Type = serializedObject.FindProperty("SlopeNoise2Type");
            p_SlopeNoise2Method = serializedObject.FindProperty("SlopeNoise2Method");
            p_SlopeNoise2Size = serializedObject.FindProperty("SlopeNoise2Size");
            p_SlopeNoise2Lacunarity = serializedObject.FindProperty("SlopeNoise2Lacunarity");
            p_SlopeNoise2Persistence = serializedObject.FindProperty("SlopeNoise2Persistence");
            p_SlopeNoise2Mult = serializedObject.FindProperty("SlopeNoise2Mult");
            p_SlopeNoise2Mod = serializedObject.FindProperty("SlopeNoise2Mod");
            p_SlopeNoise2Octaves = serializedObject.FindProperty("SlopeNoise2Octaves");
            p_RandomSeed = serializedObject.FindProperty("RandomSeed");
            p_ScaleNoiseType = serializedObject.FindProperty("ScaleNoiseType");
            p_ScaleNoiseSize = serializedObject.FindProperty("ScaleNoiseSize");
            p_ScaleNoiseLacunarity = serializedObject.FindProperty("ScaleNoiseLacunarity");
            p_ScaleNoisePersistence = serializedObject.FindProperty("ScaleNoisePersistence");
            p_ScaleNoiseMult = serializedObject.FindProperty("ScaleNoiseMult");
            p_ScaleNoiseMod = serializedObject.FindProperty("ScaleNoiseMod");
            p_ScaleNoiseOctaves = serializedObject.FindProperty("ScaleNoiseOctaves");
            p_ScaleNoise2Type = serializedObject.FindProperty("ScaleNoise2Type");
            p_ScaleNoise2Method = serializedObject.FindProperty("ScaleNoise2Method");
            p_ScaleNoise2Size = serializedObject.FindProperty("ScaleNoise2Size");
            p_ScaleNoise2Lacunarity = serializedObject.FindProperty("ScaleNoise2Lacunarity");
            p_ScaleNoise2Persistence = serializedObject.FindProperty("ScaleNoise2Persistence");
            p_ScaleNoise2Mult = serializedObject.FindProperty("ScaleNoise2Mult");
            p_ScaleNoise2Mod = serializedObject.FindProperty("ScaleNoise2Mod");
            p_ScaleNoise2Octaves = serializedObject.FindProperty("ScaleNoise2Octaves");
            p_ColorRandomization = serializedObject.FindProperty("ColorRandomization");
            p_ColorTransition = serializedObject.FindProperty("ColorTransition");
            p_ColorAPropName = serializedObject.FindProperty("ColorAPropName");
            p_ColorBPropName = serializedObject.FindProperty("ColorBPropName");
            p_ColorA = serializedObject.FindProperty("ColorA");
            p_ColorB = serializedObject.FindProperty("ColorB");
            p_DisableRender = serializedObject.FindProperty("DisableRender");
            p_RenderType = serializedObject.FindProperty("RenderType");
            p_CombineType = serializedObject.FindProperty("CombineType");
            p_LODSplits = serializedObject.FindProperty("LODSplits");
            p_LODFade = serializedObject.FindProperty("LODFade");
            p_LODMix = serializedObject.FindProperty("LODMix");
            p_LODs = serializedObject.FindProperty("LODs");
            p_ShadowsUseLOD = serializedObject.FindProperty("ShadowsUseLOD");
            p_customShadowRenderer = serializedObject.FindProperty("m_shadowRenderer");
            p_RenderOffset = serializedObject.FindProperty("RenderOffset");
            p_RenderRotation = serializedObject.FindProperty("RenderRotation");
            p_RenderScale = serializedObject.FindProperty("RenderScale");
            p_DrawLongShadows = serializedObject.FindProperty("DrawLongShadows");
            p_MaxLongShadowDist = serializedObject.FindProperty("MaxLongShadowDist");
            p_ScaleoutRange = serializedObject.FindProperty("ScaleoutRange");
            p_ScaleoutMinScale = serializedObject.FindProperty("ScaleoutMinScale");
            p_Colliders = serializedObject.FindProperty("Colliders");

            m_showScaleNoise = new AnimBool(p_ScaleNoiseType.enumValueIndex != 0, Repaint);
            m_showSlopeNoise = new AnimBool(p_SlopeType.enumValueIndex == 2, Repaint);
            m_showSlopeNoiseSettings = new AnimBool(p_SlopeNoiseType.enumValueIndex != 0, Repaint);
            m_showScaleNoise2 = new AnimBool(p_ScaleNoise2Type.enumValueIndex != 0, Repaint);
            m_showSlopeNoise2Settings = new AnimBool(p_SlopeNoise2Type.enumValueIndex != 0, Repaint);
            m_showPreviewScaleNoise = new AnimBool(p_ScaleNoiseType.isExpanded, Repaint);
            m_showPreviewSlopeNoise = new AnimBool(p_SlopeNoiseType.isExpanded, Repaint);
            m_lodFoldout = new AnimBool(p_LODs.isExpanded, Repaint);
            m_shadowLODFoldout = new AnimBool(p_ShadowsUseLOD.intValue < 0, Repaint);

            Undo.undoRedoPerformed -= OnUndo;
            Undo.undoRedoPerformed += OnUndo;
        }

        private void OnDisable()
        {
            if (m_preview != null)
                m_preview.Dispose();
            m_preview = null;
            Undo.undoRedoPerformed -= OnUndo;
            m_previewTexture = null;
        }
        private void OnDestroy()
        {
            OnDisable();
        }
        public override void OnInspectorGUI()
        {
            if (m_editorUtils == null)
                m_editorUtils = StormApp.GetEditorUtils(this);

            m_featureSettings = (StormFeatureSettings)target;

            m_editorUtils.Initialize();
            m_editorUtils.Panel("Asset", AssetPanel, true);
            m_editorUtils.Panel("GenerationSettingsPanel", GenerationSettingsPanel, true);
            m_editorUtils.Panel("RenderSettingsPanel", RenderSettingsPanel, true);
            if (m_removeLOD >= 0)
            {
                p_LODs.DeleteArrayElementAtIndex(m_removeLOD);
                m_removeLOD = -1;
            }
            if (m_removeRenderData.x >= 0 && m_removeRenderData.y >= 0)
            {
                SerializedProperty lodProp = p_LODs.GetArrayElementAtIndex(m_removeRenderData.x);
                SerializedProperty renderData = lodProp.FindPropertyRelative("renderData");
                renderData.DeleteArrayElementAtIndex(m_removeRenderData.y);
                m_removeRenderData = new Vector2Int(-1, -1);
            }
            serializedObject.ApplyModifiedProperties();
            if (m_needUpdateGeneration)
            {
                m_needUpdateGeneration = false;
                foreach (StormFeatureSettings obj in targets)
                {
                    StormUtils.SetAssetDirty(obj);
                    obj.OnGenerationSettingsChanged?.Invoke();
                }
                StormUtils.ForceEditorSceneviewRedraw();
                if (StormWorld.Instance != null)
                    StormWorld.Instance.Update();
            }
            if (m_needUpdateRender)
            {
                m_needUpdateRender = false;
                foreach (StormFeatureSettings obj in targets)
                {
                    StormUtils.SetAssetDirty(obj);
                    obj.OnRenderSettingsChanged?.Invoke();
                    StormAPI.OnRenderSettingsChanged?.Invoke(obj);
                }
                StormUtils.ForceEditorSceneviewRedraw();
                if (StormWorld.Instance != null)
                    StormWorld.Instance.Update();
            }
            if (m_needUpdateLOD)
            {
                m_needUpdateLOD = false;
                foreach (StormFeatureSettings obj in targets)
                {
                    StormUtils.SetAssetDirty(obj);
                    obj.OnLODSettingsChanged?.Invoke();
                }
                StormUtils.ForceEditorSceneviewRedraw();
                if (StormWorld.Instance != null)
                    StormWorld.Instance.Update();
            }

        }

        public void AssetPanel(bool helpActive)
        {
            if (targets.Length == 1)
            {
                //Overriding the default label width because it looks odd to have those large labels in this layout
                float originalLabelWidth = EditorGUIUtility.labelWidth;
                EditorGUIUtility.labelWidth = originalLabelWidth / 2f;
                EditorGUILayout.BeginHorizontal();
                {
                    EditorGUILayout.BeginVertical(GUILayout.Width(150), GUILayout.Height(155));
                    {
                        if (m_featureSettings.SourcePrefab != null)
                        {
                            if (m_previewTexture == null)
                            {
                                m_previewTexture = AssetPreview.GetAssetPreview(m_featureSettings.SourcePrefab);
                            }
                            m_editorUtils.Image(m_previewTexture, 150, 150);
                        }
                    }
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.BeginVertical();
                    {
                        m_editorUtils.LabelField("FeatureName", new GUIContent(m_featureSettings.name, "The name of the feature you are viewing / editing."), helpActive);

                        m_allowEditSource = m_editorUtils.Toggle(m_allowEditSource, "AllowEditSourceToggle", helpActive);
                        EditorGUI.BeginDisabledGroup(!m_allowEditSource);
                        m_editorUtils.EnumPopupLocalized("SourceType", "Source_", p_SourceType, helpActive);
                        if (m_featureSettings.SourceType == StormFeatureSource.PREFAB)
                        {
                            m_featureSettings.SourcePrefab = m_editorUtils.ObjectField("Prefab", m_featureSettings.SourcePrefab, typeof(GameObject), false, helpActive) as GameObject;
                        }
                        else if (m_featureSettings.SourceType == StormFeatureSource.TERRAIN_DETAIL)
                        {
                            m_featureSettings.SourcePrefab = m_editorUtils.ObjectField("Prefab", m_featureSettings.SourcePrefab, typeof(GameObject), false, helpActive) as GameObject;
                            m_featureSettings.SourceTerrain = m_editorUtils.ObjectField("DetailTerrain", m_featureSettings.SourceTerrain, typeof(TerrainData), false, helpActive) as TerrainData;
                            if (m_featureSettings.SourceTerrain != null)
                                m_featureSettings.SourceDetailIndex = m_editorUtils.IntSlider("DetailIndex", m_featureSettings.SourceDetailIndex, 0, m_featureSettings.SourceTerrain == null ? 0 : m_featureSettings.SourceTerrain.detailPrototypes.Length - 1, helpActive);
                            if (m_featureSettings.SourceTerrain != null && m_featureSettings.SourceDetailIndex >= 0 && m_featureSettings.SourceDetailIndex < m_featureSettings.SourceTerrain.detailPrototypes.Length && m_featureSettings.SourceTerrain.detailPrototypes[m_featureSettings.SourceDetailIndex].prototype != null)
                                GUILayout.Label(m_featureSettings.SourceTerrain.detailPrototypes[m_featureSettings.SourceDetailIndex].prototype.name);
                        }
                        EditorGUI.EndDisabledGroup();
                    }
                    EditorGUILayout.EndVertical();
                }
                EditorGUILayout.EndHorizontal();
                EditorGUI.BeginDisabledGroup(m_featureSettings.SourceType == StormFeatureSource.CUSTOM || !((m_featureSettings.SourceType == StormFeatureSource.PREFAB && m_featureSettings.SourcePrefab != null) || (m_featureSettings.SourceType == StormFeatureSource.TERRAIN_DETAIL && m_featureSettings.SourceTerrain != null)));
                if (m_editorUtils.Button("Reingest Prefab"))
                {
                    for (int o = 0; o < targets.Length; ++o)
                    {
                        StormFeatureSettings feature = targets[o] as StormFeatureSettings;
                        if (feature.SourceType == StormFeatureSource.PREFAB && feature.SourcePrefab != null)
                        {
                            feature = Core.StormAssetUtils.CreateSettingsFromPrefab(feature.SourcePrefab, feature.ObjectType, StormUtils.GetAssetPath(feature), true, feature, false);
                            StormUtils.SetAssetDirty(feature);
                            serializedObject.Update();
                            StormWorld.Instance.RefreshFeature(feature);
                            StormWorld.Instance.UpdateFeaturesets();
                            feature.OnRenderSettingsChanged?.Invoke();
                            feature.OnLODSettingsChanged?.Invoke();
                            feature.OnGenerationSettingsChanged?.Invoke();
                        }
                        else if (feature.SourceType == StormFeatureSource.TERRAIN_DETAIL && feature.SourceTerrain != null && feature.SourceDetailIndex >= 0 && feature.SourceDetailIndex < feature.SourceTerrain.detailPrototypes.Length)
                        {
                            feature = Core.StormAssetUtils.CreateSettingsFromDetail(feature.SourceTerrain, feature.SourceDetailIndex, StormSettings.SystemSettings, StormUtils.GetAssetPath(feature), false, feature);
                            StormUtils.SetAssetDirty(feature);
                            serializedObject.Update();
                            StormWorld.Instance.RefreshFeature(feature);
                            StormWorld.Instance.UpdateFeaturesets();
                            feature.OnRenderSettingsChanged?.Invoke();
                            feature.OnLODSettingsChanged?.Invoke();
                            feature.OnGenerationSettingsChanged?.Invoke();
                        }
                    }
                    AssetDatabase.SaveAssets();
                    GUIUtility.ExitGUI();
                }
                EditorGUI.EndDisabledGroup();
                EditorGUIUtility.labelWidth = originalLabelWidth;
            }
            else
            {
                m_editorUtils.LabelField("MultipleObjectsSelected", EditorStyles.wordWrappedLabel);
            }

        }

        public void GenerationSettingsPanel(bool helpActive)
        {
            EditorGUI.indentLevel++;
            bool originalGUIState = GUI.enabled;
            EditorGUI.BeginChangeCheck();
            Vector4 jitterVal = p_Jitter.vector4Value;


            ////////////////////////////////////
            ///SEED
            ////////////////////////////////////
            m_showSeedSettings = StormEditorUtils.LongFoldout(m_showSeedSettings, "SeedSectionLabel", m_editorUtils, helpActive, new Color(0f, 0f, 0f, 0.33f));
            if (m_showSeedSettings)
            {
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.BeginHorizontal();
                PropField("RandomSeed", p_RandomSeed, false);
                if (GUILayout.Button(RandomizeButtonContent, GUILayout.ExpandWidth(false), GUILayout.Height(EditorGUIUtility.singleLineHeight)))
                {
                    m_needUpdateGeneration = true;
                    p_RandomSeed.intValue = Random.Range(int.MinValue, int.MaxValue);
                }
                EditorGUILayout.EndHorizontal();
                m_editorUtils.InlineHelp("RandomSeed", helpActive);
                if (EditorGUI.EndChangeCheck())
                {
                    m_needUpdateGeneration = true;
                }
            }
            EditorGUILayout.Space();

            ////////////////////////////////////
            ///INSTANCE GENERATOR
            ////////////////////////////////////
            m_showInstanceGeneratorSettings = StormEditorUtils.LongFoldout(m_showInstanceGeneratorSettings, "InstanceGeneratorLabel", m_editorUtils, helpActive, new Color(0f, 0f, 0f, 0.33f));
            if (m_showInstanceGeneratorSettings)
            {
                EditorGUILayout.Space();
                EditorGUI.indentLevel++;
                ////////////////////////////////////
                ///GRID
                ////////////////////////////////////

                m_showGridSettings = StormEditorUtils.LongFoldout(m_showGridSettings, "GridLabel", m_editorUtils, helpActive);

                //EditorGUILayout.LabelField("", EditorStyles.foldoutPreDrop);
                if (m_showGridSettings)
                {
                    EditorGUI.BeginChangeCheck();
                    PropField("Density", p_Density, helpActive);
                    EditorGUI.showMixedValue = p_Jitter.hasMultipleDifferentValues;
                    jitterVal.x = m_editorUtils.Slider("JitterX", jitterVal.x * 100f, 0, 100, helpActive) / 100f;
                    jitterVal.y = m_editorUtils.Slider("JitterZ", jitterVal.y * 100f, 0, 100, helpActive) / 100f;

                    m_editorUtils.SliderRange("JitterYMinMax", ref jitterVal.z, ref jitterVal.w, m_jitterYminMax, false);
                    EditorGUI.showMixedValue = false;
                    m_editorUtils.InlineHelp("JitterYMinMax", helpActive);
                    if (EditorGUI.EndChangeCheck() || jitterVal != p_Jitter.vector4Value)
                    {
                        p_Jitter.vector4Value = jitterVal;
                        m_needUpdateGeneration = true;
                    }
                }
                EditorGUILayout.Space();

                ////////////////////////////////////
                ///FILTER
                ////////////////////////////////////

                m_showSpawnSettings = StormEditorUtils.LongFoldout(m_showSpawnSettings, "FilterSectionLabel", m_editorUtils, helpActive);
                if (m_showSpawnSettings)
                {
                    float splatCutoffVal = p_SplatCutoff.floatValue * 100f;
                    float splatMaxVal = p_SplatMaxValue.floatValue * 100f;
                    bool sliderRangeChanged = false;

                    Vector2 m_fitnessMinMax = new Vector2(0f, 100f);

                    EditorGUI.BeginChangeCheck();
                    EditorGUI.showMixedValue = p_SplatCutoff.hasMultipleDifferentValues || p_SplatMaxValue.hasMultipleDifferentValues;
                    sliderRangeChanged = m_editorUtils.SliderRange("LayerDataCutoff", ref splatCutoffVal, ref splatMaxVal, m_fitnessMinMax, false);
                    EditorGUI.showMixedValue = false;
                    m_editorUtils.InlineHelp("LayerDataCutoff", helpActive);
                    if (sliderRangeChanged)
                    {
                        p_SplatCutoff.floatValue = splatCutoffVal / 100f;
                        p_SplatMaxValue.floatValue = splatMaxVal / 100f;
                    }
                    ToggleMinMax("LimitHeight", p_LimitMinHeight, p_LimitMaxHeight, p_MinHeight, p_MaxHeight, helpActive);
                    ToggleMinMax("LimitSlope", p_LimitMinSlope, p_LimitMaxSlope, p_MinSlope, p_MaxSlope, helpActive);

                    p_SpawnChance.floatValue = m_editorUtils.Slider("SpawnChance", p_SpawnChance.floatValue * 100, 0, 200, helpActive) / 100;
                    m_editorUtils.PropertyField("IgnoreSeaLevel", p_IgnoreSeaLevel, helpActive);
                    if (EditorGUI.EndChangeCheck() || sliderRangeChanged)
                    {
                        m_needUpdateGeneration = true;
                    }
                }
                EditorGUILayout.Space();


                ////////////////////////////////////
                ///NOISE
                ////////////////////////////////////

                m_showNoiseSettings = StormEditorUtils.LongFoldout(m_showNoiseSettings, "NoiseLabel", m_editorUtils, helpActive);
                if (m_showNoiseSettings)
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    p_ScaleNoiseType.isExpanded = m_editorUtils.Toggle(p_ScaleNoiseType.isExpanded, "ScaleNoisePreviewButton", EditorStyles.miniButton, GUILayout.ExpandWidth(false));
                    if (m_editorUtils.Button("PasteScaleButton", GUILayout.ExpandWidth(false)))
                    {
                        if (StormNoise.StringToNoise(GUIUtility.systemCopyBuffer, out RandomType t, out float s, out float l, out float p, out float mo, out float mu, out int st, out CombineMethod cm, out RandomType t2, out float s2, out float l2, out float p2, out float mo2, out float mu2, out int st2))
                        {
                            p_ScaleNoiseType.enumValueIndex = (int)t;
                            p_ScaleNoiseSize.floatValue = s;
                            p_ScaleNoiseLacunarity.floatValue = l;
                            p_ScaleNoisePersistence.floatValue = p;
                            p_ScaleNoiseMod.floatValue = mo;
                            p_ScaleNoiseMult.floatValue = mu;
                            p_ScaleNoiseOctaves.intValue = st;
                            p_ScaleNoise2Type.enumValueIndex = (int)t2;
                            p_ScaleNoise2Method.enumValueIndex = (int)cm;
                            p_ScaleNoise2Size.floatValue = s2;
                            p_ScaleNoise2Lacunarity.floatValue = l2;
                            p_ScaleNoise2Persistence.floatValue = p2;
                            p_ScaleNoise2Mod.floatValue = mo2;
                            p_ScaleNoise2Mult.floatValue = mu2;
                            p_ScaleNoise2Octaves.intValue = st2;
                            m_needUpdateGeneration = true;
                        }
                    }
                    if (m_editorUtils.Button("CopyScaleButton", GUILayout.ExpandWidth(false)))
                    {
                        GUIUtility.systemCopyBuffer = StormNoise.NoiseToString(
                            (RandomType)p_ScaleNoiseType.enumValueIndex, p_ScaleNoiseSize.floatValue, p_ScaleNoiseLacunarity.floatValue, p_ScaleNoisePersistence.floatValue, p_ScaleNoiseMod.floatValue, p_ScaleNoiseMult.floatValue, p_ScaleNoiseOctaves.intValue,
                                (CombineMethod)p_ScaleNoise2Method.enumValueIndex, (RandomType)p_ScaleNoise2Type.enumValueIndex, p_ScaleNoise2Size.floatValue, p_ScaleNoise2Lacunarity.floatValue, p_ScaleNoise2Persistence.floatValue, p_ScaleNoise2Mod.floatValue, p_ScaleNoise2Mult.floatValue, p_ScaleNoise2Octaves.intValue);
                    }
                    EditorGUILayout.EndHorizontal();

                    if (EditorGUILayout.BeginFadeGroup(m_showPreviewScaleNoise.faded))
                    {
                        if (m_previewScaleNoise == null
                            || ((p_ScaleNoiseType.enumValueIndex >= 5 || p_ScaleNoise2Type.enumValueIndex >= 5) && last_ScaleNoiseSeed != p_RandomSeed.intValue)
                            || last_ScaleNoiseType != p_ScaleNoiseType.enumValueIndex
                            || last_ScaleNoiseSize != p_ScaleNoiseSize.floatValue
                            || last_ScaleNoiseLacunarity != p_ScaleNoiseLacunarity.floatValue
                            || last_ScaleNoisePersistence != p_ScaleNoisePersistence.floatValue
                            || last_ScaleNoiseMod != p_ScaleNoiseMod.floatValue
                            || last_ScaleNoiseMult != p_ScaleNoiseMult.floatValue
                            || last_ScaleNoiseOctaves != p_ScaleNoiseOctaves.intValue
                            || last_ScaleNoise2Type != p_ScaleNoise2Type.enumValueIndex
                            || last_ScaleNoise2Method != p_ScaleNoise2Method.enumValueIndex
                            || last_ScaleNoise2Size != p_ScaleNoise2Size.floatValue
                            || last_ScaleNoise2Lacunarity != p_ScaleNoise2Lacunarity.floatValue
                            || last_ScaleNoise2Persistence != p_ScaleNoise2Persistence.floatValue
                            || last_ScaleNoise2Mod != p_ScaleNoise2Mod.floatValue
                            || last_ScaleNoise2Mult != p_ScaleNoise2Mult.floatValue
                            || last_ScaleNoise2Octaves != p_ScaleNoise2Octaves.intValue
                            || last_PreviewScaleOffset != previewScaleOffset
                            || last_PreviewScaleRange != previewScaleRange)
                        {
                            last_ScaleNoiseSeed = p_RandomSeed.intValue;
                            last_ScaleNoiseType = p_ScaleNoiseType.enumValueIndex;
                            last_ScaleNoiseSize = p_ScaleNoiseSize.floatValue;
                            last_ScaleNoiseLacunarity = p_ScaleNoiseLacunarity.floatValue;
                            last_ScaleNoisePersistence = p_ScaleNoisePersistence.floatValue;
                            last_ScaleNoiseMod = p_ScaleNoiseMod.floatValue;
                            last_ScaleNoiseMult = p_ScaleNoiseMult.floatValue;
                            last_ScaleNoiseOctaves = p_ScaleNoiseOctaves.intValue;
                            last_ScaleNoise2Type = p_ScaleNoise2Type.enumValueIndex;
                            last_ScaleNoise2Method = p_ScaleNoise2Method.enumValueIndex;
                            last_ScaleNoise2Size = p_ScaleNoise2Size.floatValue;
                            last_ScaleNoise2Lacunarity = p_ScaleNoise2Lacunarity.floatValue;
                            last_ScaleNoise2Persistence = p_ScaleNoise2Persistence.floatValue;
                            last_ScaleNoise2Mod = p_ScaleNoise2Mod.floatValue;
                            last_ScaleNoise2Mult = p_ScaleNoise2Mult.floatValue;
                            last_ScaleNoise2Octaves = p_ScaleNoise2Octaves.intValue;
                            last_PreviewScaleOffset = previewScaleOffset;
                            last_PreviewScaleRange = previewScaleRange;
                            StormNoise.UpdateNoiseTexture(ref m_previewScaleNoise, previewResolution, (uint)p_RandomSeed.intValue, previewScaleOffset, previewScaleRange,
                                (RandomType)p_ScaleNoiseType.enumValueIndex, p_ScaleNoiseSize.floatValue, p_ScaleNoiseLacunarity.floatValue, p_ScaleNoisePersistence.floatValue, p_ScaleNoiseMod.floatValue, p_ScaleNoiseMult.floatValue, p_ScaleNoiseOctaves.intValue,
                                (CombineMethod)p_ScaleNoise2Method.enumValueIndex, (RandomType)p_ScaleNoise2Type.enumValueIndex, p_ScaleNoise2Size.floatValue, p_ScaleNoise2Lacunarity.floatValue, p_ScaleNoise2Persistence.floatValue, p_ScaleNoise2Mod.floatValue, p_ScaleNoise2Mult.floatValue, p_ScaleNoise2Octaves.intValue);
                        }
                        if (m_previewScaleNoise != null)
                        {
                            EditorGUILayout.BeginHorizontal();
                            GUILayout.FlexibleSpace();
                            Rect r = EditorGUILayout.GetControlRect(true, 256, GUILayout.Width(256));
                            EditorGUILayout.BeginVertical();
                            EditorGUILayout.BeginHorizontal();
                            GUILayout.Space(15);
                            previewScaleRange = GUILayout.VerticalSlider(previewScaleRange, 10, 150, GUILayout.Width(40), GUILayout.Height((256) - EditorGUIUtility.singleLineHeight - (EditorGUIUtility.standardVerticalSpacing * 2)));
                            EditorGUILayout.EndHorizontal();
                            EditorGUI.BeginChangeCheck();
                            string txt = GUILayout.TextField(previewScaleRange.ToString("N0"), GUILayout.Width(40));
                            if (EditorGUI.EndChangeCheck() && float.TryParse(txt, out float newVal))
                                previewScaleRange = newVal;
                            EditorGUILayout.EndVertical();
                            GUILayout.Space(20f);
                            EditorGUILayout.EndHorizontal();
                            GUI.DrawTexture(r, m_previewScaleNoise, ScaleMode.ScaleToFit, false, 1);

                            Event curEvent = Event.current;
                            if (isDraggingPreviewScale && curEvent.type == EventType.MouseDrag)
                            {
                                previewScaleOffset.x -= curEvent.delta.x / 256 * previewScaleRange;
                                previewScaleOffset.y += curEvent.delta.y / 256 * previewScaleRange;
                                curEvent.Use();
                            }
                            else if (curEvent.type == EventType.MouseDown && r.Contains(curEvent.mousePosition))
                            {
                                isDraggingPreviewScale = true;
                                curEvent.Use();
                            }
                            else if (isDraggingPreviewScale && curEvent.type == EventType.MouseUp)
                                isDraggingPreviewScale = false;
                        }
                    }
                    EditorGUILayout.EndFadeGroup();

                    m_editorUtils.InlineHelp("NoiseLabel", helpActive);
                    if (m_showScaleHelp)
                    {
                        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                        m_editorUtils.Label("ScaleHelpText");
                        bool isAffected = false;
                        if (p_MaxScaleRandom.floatValue > p_MinScaleRandom.floatValue)
                        {
                            isAffected = true;
                            m_editorUtils.Label("ScaleRandom");
                        }
                        if (p_ScaleNoiseType.enumValueIndex != 0)
                        {
                            isAffected = true;
                            m_editorUtils.Label("ScaleNoiseType");
                        }
                        if (p_useScaleBySplat.boolValue)
                        {
                            isAffected = true;
                            m_editorUtils.Label("ScaleBySplat");
                        }
                        if (p_useScaleByHeight.boolValue)
                        {
                            isAffected = true;
                            m_editorUtils.Label("ScaleByHeight");
                        }
                        if (p_useScaleBySlope.boolValue)
                        {
                            isAffected = true;
                            m_editorUtils.Label("ScaleBySlope");
                        }
                        if (!isAffected)
                            GUILayout.Label(string.Format(m_editorUtils.GetContent("ScaleHelpText_Nothing").text, p_MaxScaleRandom.floatValue * p_MaxScale.floatValue));
                        EditorGUILayout.EndVertical();
                    }
                    EditorGUILayout.BeginHorizontal();
                    EditorGUI.BeginChangeCheck();
                    PropField("NoiseType", p_ScaleNoiseType, false);
                    if (EditorGUI.EndChangeCheck())
                    {
                        m_needUpdateGeneration = true;
                    }
                    m_showPreviewScaleNoise.target = p_ScaleNoiseType.isExpanded;
                    EditorGUILayout.EndHorizontal();
                    m_editorUtils.InlineHelp("NoiseType", helpActive);
                    EditorGUI.BeginChangeCheck();
                    {
                        m_showScaleNoise.target = p_ScaleNoiseType.enumValueIndex != 0;
                        if (EditorGUILayout.BeginFadeGroup(m_showScaleNoise.faded))
                        {
                            //We invert the noise size value and offer a slider between 0 and 300
                            //a value of 1 = small pattern and 300 = large pattern is easier to understand
                            //than setting the size to 0.0001 when you want large noise patterns
                            FloatField("NoiseSize", p_ScaleNoiseSize, 1, -1, -300, 300, helpActive);
                            PropField("NoiseOctaves", p_ScaleNoiseOctaves, helpActive);
                            EditorGUI.indentLevel++;
                            if (p_ScaleNoiseOctaves.intValue <= 1)
                            {
                                GUI.enabled = false;
                            }
                            FloatField("NoiseLacunarity", p_ScaleNoiseLacunarity, 2, -2, -200, 200, helpActive);
                            FloatField("NoisePersistence", p_ScaleNoisePersistence, 0, 10, helpActive);
                            GUI.enabled = originalGUIState;
                            EditorGUI.indentLevel--;
                            FloatField("NoiseMult", p_ScaleNoiseMult, -10, 10, helpActive);
                            FloatField("NoiseMod", p_ScaleNoiseMod, -1, 1, -100, 100, helpActive);
                            EditorGUILayout.Space();
                        }
                        EditorGUILayout.EndFadeGroup();
                        PropField("Noise2Type", p_ScaleNoise2Type, helpActive);
                        m_showScaleNoise2.target = p_ScaleNoise2Type.enumValueIndex != 0;
                        if (EditorGUILayout.BeginFadeGroup(m_showScaleNoise2.faded))
                        {
                            PropField("Noise2Method", p_ScaleNoise2Method, helpActive);
                            //We invert the noise size value and offer a slider between 0 and 300
                            //a value of 1 = small pattern and 300 = large pattern is easier to understand
                            //than setting the size to 0.0001 when you want large noise patterns
                            FloatField("NoiseSize", p_ScaleNoise2Size, -1, 1, -300, 300, helpActive);
                            PropField("NoiseOctaves", p_ScaleNoise2Octaves, helpActive);
                            EditorGUI.indentLevel++;
                            if (p_ScaleNoise2Octaves.intValue <= 1)
                            {
                                GUI.enabled = false;
                            }
                            FloatField("NoiseLacunarity", p_ScaleNoise2Lacunarity, 2, -2, -200, 200, helpActive);
                            FloatField("NoisePersistence", p_ScaleNoise2Persistence, 0, 10, helpActive);
                            GUI.enabled = originalGUIState;
                            EditorGUI.indentLevel--;
                            FloatField("NoiseMult", p_ScaleNoise2Mult, -10, 10, helpActive);
                            FloatField("NoiseMod", p_ScaleNoise2Mod, -1, 1, -100, 100, helpActive);
                            EditorGUILayout.Space();
                        }
                        else
                        {
                            //switch the scale noise 2 method back to "multiply" if no noise 2 is selected, otherwise it causes issues with applying the first 
                            p_ScaleNoise2Method.enumValueIndex = (int)CombineMethod.MULTIPLY;
                        }
                        EditorGUILayout.EndFadeGroup();
                    }

                    if (EditorGUI.EndChangeCheck())
                    {
                        m_needUpdateGeneration = true;
                    }
                }

                EditorGUI.indentLevel--;
                //End of Instance Generation
            }

            EditorGUILayout.Space();

            ////////////////////////////////////
            ///INSTANCE MODIFIER
            ////////////////////////////////////
            m_showInstanceModifierSettings = StormEditorUtils.LongFoldout(m_showInstanceModifierSettings, "InstanceModifierLabel", m_editorUtils, helpActive, new Color(0f, 0f, 0f, 0.33f));
            if (m_showInstanceModifierSettings)
            {
                EditorGUILayout.Space();
                EditorGUI.indentLevel++;




                ////////////////////////////////////
                ///SCALE
                ////////////////////////////////////

                m_showScaleSettings = StormEditorUtils.LongFoldout(m_showScaleSettings, "ScaleLabel", m_editorUtils, helpActive);
                if (m_showScaleSettings)
                {
                    EditorGUI.BeginChangeCheck();

                    //some custom controls are not covered by the change check, need to roll our own
                    bool wasChanged = m_editorUtils.SliderRange("BaseScale", p_MinScale, p_MaxScale, helpActive, new Vector2(0, 100));

                    float displayRandomScaleMin = m_featureSettings.RandomScaleMin * 100;
                    float displayRandomScaleMax = m_featureSettings.RandomScaleMax * 100;

                    wasChanged |= ToggleCurveField("ScaleBySplat", p_useScaleBySplat, p_ScaleBySplat, helpActive);
                    wasChanged |= ToggleCurveField("ScaleByHeight", p_useScaleByHeight, p_ScaleByHeight, helpActive);
                    wasChanged |= ToggleCurveField("ScaleBySlope", p_useScaleBySlope, p_ScaleBySlope, helpActive);

                    if (m_editorUtils.SliderRange("ScaleRandom", ref displayRandomScaleMin, ref displayRandomScaleMax, helpActive, new Vector2(1, 100)))
                    {
                        m_featureSettings.RandomScaleMin = displayRandomScaleMin / 100;
                        m_featureSettings.RandomScaleMax = displayRandomScaleMax / 100;
                        wasChanged = true;
                    }
                    if (EditorGUI.EndChangeCheck() || wasChanged)
                    {
                        m_needUpdateGeneration = true;
                    }
                }
                EditorGUILayout.Space();

                ////////////////////////////////////
                ///ROTATION
                ////////////////////////////////////

                m_showRotationSettings = StormEditorUtils.LongFoldout(m_showRotationSettings, "RotationSectionLabel", m_editorUtils, helpActive);
                if (m_showRotationSettings)
                {
                    EditorGUI.showMixedValue = p_RotBase.hasMultipleDifferentValues | p_RotRange.hasMultipleDifferentValues;
                    Vector3 rotVal = p_RotRange.vector3Value;
                    m_displayMinRotX = p_RotBase.vector3Value.x - rotVal.x;
                    m_displayMinRotY = p_RotBase.vector3Value.y - rotVal.y;
                    m_displayMinRotZ = p_RotBase.vector3Value.z - rotVal.z;
                    m_displayMaxRotX = p_RotBase.vector3Value.x + rotVal.x;
                    m_displayMaxRotY = p_RotBase.vector3Value.y + rotVal.y;
                    m_displayMaxRotZ = p_RotBase.vector3Value.z + rotVal.z;

                    bool rangesChanged = false;

                    if (m_editorUtils.SliderRange("RotRangeX", ref m_displayMinRotX, ref m_displayMaxRotX, helpActive, 0, 360) ||
                    m_editorUtils.SliderRange("RotRangeY", ref m_displayMinRotY, ref m_displayMaxRotY, helpActive, 0, 360) ||
                    m_editorUtils.SliderRange("RotRangeZ", ref m_displayMinRotZ, ref m_displayMaxRotZ, helpActive, 0, 360))
                    {
                        rangesChanged = true;
                    }
                    if (rangesChanged)
                    {
                        //The base rotation lies in the middle or the "average" between minimum and maximum range
                        Vector3 rotBase = new Vector3(
                            (m_displayMaxRotX + m_displayMinRotX) * 0.5f,
                            (m_displayMaxRotY + m_displayMinRotY) * 0.5f,
                            (m_displayMaxRotZ + m_displayMinRotZ) * 0.5f
                            );

                        //The rotation range is half of the difference between max and min
                        rotVal = new Vector3(
                            (m_displayMaxRotX - m_displayMinRotX) * 0.5f,
                            (m_displayMaxRotY - m_displayMinRotY) * 0.5f,
                            (m_displayMaxRotZ - m_displayMinRotZ) * 0.5f
                            );

                        m_needUpdateGeneration = true;
                        p_RotBase.vector3Value = rotBase;
                        p_RotRange.vector3Value = rotVal;
                    }
                    EditorGUI.showMixedValue = false;
                    EditorGUI.BeginChangeCheck();
                    FloatField("AlignToUp", p_AlignToUp, 0, 1, 0, 100, helpActive);
                    FloatField("AlignForwardToSlope", p_AlignForwardToSlope, 0, 1, 0, 100, helpActive);
                    PropField("SlopeCheckSize", p_SlopeCheckSize, helpActive);
                    PropField("SlopeType", p_SlopeType, helpActive);
                    if (EditorGUI.EndChangeCheck())
                    {
                        m_needUpdateGeneration = true;
                    }
                    m_showSlopeNoise.target = p_SlopeType.enumValueIndex == 2;
                    if (EditorGUILayout.BeginFadeGroup(m_showSlopeNoise.faded))
                    {
                        EditorGUILayout.Space();
                        EditorGUILayout.BeginHorizontal();
                        m_editorUtils.Label("SlopeNoiseHeader", EditorStyles.boldLabel);
                        GUILayout.FlexibleSpace();
                        p_SlopeNoiseType.isExpanded = m_editorUtils.Toggle(p_SlopeNoiseType.isExpanded, "ScaleNoisePreviewButton", EditorStyles.miniButton, GUILayout.ExpandWidth(false));
                        m_showPreviewSlopeNoise.target = p_SlopeNoiseType.isExpanded;
                        if (m_editorUtils.Button("PasteScaleButton", GUILayout.ExpandWidth(false)))
                        {
                            if (StormNoise.StringToNoise(GUIUtility.systemCopyBuffer, out RandomType t, out float s, out float l, out float p, out float mo, out float mu, out int st, out CombineMethod cm, out RandomType t2, out float s2, out float l2, out float p2, out float mo2, out float mu2, out int st2))
                            {
                                p_SlopeNoiseType.enumValueIndex = (int)t;
                                p_SlopeNoiseSize.floatValue = s;
                                p_SlopeNoiseLacunarity.floatValue = l;
                                p_SlopeNoisePersistence.floatValue = p;
                                p_SlopeNoiseMod.floatValue = mo;
                                p_SlopeNoiseMult.floatValue = mu;
                                p_SlopeNoiseOctaves.intValue = st;
                                p_SlopeNoise2Type.enumValueIndex = (int)t2;
                                p_SlopeNoise2Method.enumValueIndex = (int)cm;
                                p_SlopeNoise2Size.floatValue = s2;
                                p_SlopeNoise2Lacunarity.floatValue = l2;
                                p_SlopeNoise2Persistence.floatValue = p2;
                                p_SlopeNoise2Mod.floatValue = mo2;
                                p_SlopeNoise2Mult.floatValue = mu2;
                                p_SlopeNoise2Octaves.intValue = st2;
                                m_needUpdateGeneration = true;
                            }
                        }
                        if (m_editorUtils.Button("CopyScaleButton", GUILayout.ExpandWidth(false)))
                        {
                            GUIUtility.systemCopyBuffer = StormNoise.NoiseToString(
                                (RandomType)p_SlopeNoiseType.enumValueIndex, p_SlopeNoiseSize.floatValue, p_SlopeNoiseLacunarity.floatValue, p_SlopeNoisePersistence.floatValue, p_SlopeNoiseMod.floatValue, p_SlopeNoiseMult.floatValue, p_SlopeNoiseOctaves.intValue,
                                    (CombineMethod)p_SlopeNoise2Method.enumValueIndex, (RandomType)p_SlopeNoise2Type.enumValueIndex, p_SlopeNoise2Size.floatValue, p_SlopeNoise2Lacunarity.floatValue, p_SlopeNoise2Persistence.floatValue, p_SlopeNoise2Mod.floatValue, p_SlopeNoise2Mult.floatValue, p_SlopeNoise2Octaves.intValue);
                        }
                        EditorGUILayout.EndHorizontal();
                        if (EditorGUILayout.BeginFadeGroup(m_showPreviewSlopeNoise.faded))
                        {
                            if (m_previewSlopeNoise == null
                                || ((p_SlopeNoiseType.enumValueIndex >= 5 || p_SlopeNoise2Type.enumValueIndex >= 5) && last_SlopeNoiseSeed != p_RandomSeed.intValue)
                                || last_SlopeNoiseType != p_SlopeNoiseType.enumValueIndex
                                || last_SlopeNoiseSize != p_SlopeNoiseSize.floatValue
                                || last_SlopeNoiseLacunarity != p_SlopeNoiseLacunarity.floatValue
                                || last_SlopeNoisePersistence != p_SlopeNoisePersistence.floatValue
                                || last_SlopeNoiseMod != p_SlopeNoiseMod.floatValue
                                || last_SlopeNoiseMult != p_SlopeNoiseMult.floatValue
                                || last_SlopeNoiseOctaves != p_SlopeNoiseOctaves.intValue
                                || last_SlopeNoise2Type != p_SlopeNoise2Type.enumValueIndex
                                || last_SlopeNoise2Method != p_SlopeNoise2Method.enumValueIndex
                                || last_SlopeNoise2Size != p_SlopeNoise2Size.floatValue
                                || last_SlopeNoise2Lacunarity != p_SlopeNoise2Lacunarity.floatValue
                                || last_SlopeNoise2Persistence != p_SlopeNoise2Persistence.floatValue
                                || last_SlopeNoise2Mod != p_SlopeNoise2Mod.floatValue
                                || last_SlopeNoise2Mult != p_SlopeNoise2Mult.floatValue
                                || last_SlopeNoise2Octaves != p_SlopeNoise2Octaves.intValue
                                || last_PreviewSlopeOffset != previewSlopeOffset
                                || last_PreviewSlopeRange != previewSlopeRange)
                            {
                                last_SlopeNoiseSeed = p_RandomSeed.intValue;
                                last_SlopeNoiseType = p_SlopeNoiseType.enumValueIndex;
                                last_SlopeNoiseSize = p_SlopeNoiseSize.floatValue;
                                last_SlopeNoiseLacunarity = p_SlopeNoiseLacunarity.floatValue;
                                last_SlopeNoisePersistence = p_SlopeNoisePersistence.floatValue;
                                last_SlopeNoiseMod = p_SlopeNoiseMod.floatValue;
                                last_SlopeNoiseMult = p_SlopeNoiseMult.floatValue;
                                last_SlopeNoiseOctaves = p_SlopeNoiseOctaves.intValue;
                                last_SlopeNoise2Type = p_SlopeNoise2Type.enumValueIndex;
                                last_SlopeNoise2Method = p_SlopeNoise2Method.enumValueIndex;
                                last_SlopeNoise2Size = p_SlopeNoise2Size.floatValue;
                                last_SlopeNoise2Lacunarity = p_SlopeNoise2Lacunarity.floatValue;
                                last_SlopeNoise2Persistence = p_SlopeNoise2Persistence.floatValue;
                                last_SlopeNoise2Mod = p_SlopeNoise2Mod.floatValue;
                                last_SlopeNoise2Mult = p_SlopeNoise2Mult.floatValue;
                                last_SlopeNoise2Octaves = p_SlopeNoise2Octaves.intValue;
                                last_PreviewSlopeOffset = previewSlopeOffset;
                                last_PreviewSlopeRange = previewSlopeRange;
                                StormNoise.UpdateNoiseTexture(ref m_previewSlopeNoise, previewResolution, (uint)p_RandomSeed.intValue, previewSlopeOffset, previewSlopeRange,
                                    (RandomType)p_SlopeNoiseType.enumValueIndex, p_SlopeNoiseSize.floatValue, p_SlopeNoiseLacunarity.floatValue, p_SlopeNoisePersistence.floatValue, p_SlopeNoiseMod.floatValue, p_SlopeNoiseMult.floatValue, p_SlopeNoiseOctaves.intValue,
                                    (CombineMethod)p_SlopeNoise2Method.enumValueIndex, (RandomType)p_SlopeNoise2Type.enumValueIndex, p_SlopeNoise2Size.floatValue, p_SlopeNoise2Lacunarity.floatValue, p_SlopeNoise2Persistence.floatValue, p_SlopeNoise2Mod.floatValue, p_SlopeNoise2Mult.floatValue, p_SlopeNoise2Octaves.intValue);
                            }
                            if (m_previewSlopeNoise != null)
                            {
                                EditorGUILayout.BeginHorizontal();
                                GUILayout.FlexibleSpace();
                                Rect r = EditorGUILayout.GetControlRect(true, 256, GUILayout.Width(256));
                                EditorGUILayout.BeginVertical();
                                EditorGUILayout.BeginHorizontal();
                                GUILayout.Space(15);
                                previewSlopeRange = GUILayout.VerticalSlider(previewSlopeRange, 10, 150, GUILayout.Width(40), GUILayout.Height((256) - EditorGUIUtility.singleLineHeight - (EditorGUIUtility.standardVerticalSpacing * 2)));
                                EditorGUILayout.EndHorizontal();
                                EditorGUI.BeginChangeCheck();
                                string txt = GUILayout.TextField(previewSlopeRange.ToString("N0"), GUILayout.Width(40));
                                if (EditorGUI.EndChangeCheck() && float.TryParse(txt, out float newVal))
                                    previewSlopeRange = newVal;
                                EditorGUILayout.EndVertical();
                                GUILayout.Space(20f);
                                EditorGUILayout.EndHorizontal();
                                GUI.DrawTexture(r, m_previewSlopeNoise, ScaleMode.ScaleToFit, false, 1);

                                Event curEvent = Event.current;
                                if (isDraggingPreviewSlope && curEvent.type == EventType.MouseDrag)
                                {
                                    previewSlopeOffset.x -= curEvent.delta.x / 256 * previewSlopeRange;
                                    previewSlopeOffset.y += curEvent.delta.y / 256 * previewSlopeRange;
                                    curEvent.Use();
                                }
                                else if (curEvent.type == EventType.MouseDown && r.Contains(curEvent.mousePosition))
                                {
                                    isDraggingPreviewSlope = true;
                                    curEvent.Use();
                                }
                                else if (isDraggingPreviewSlope && curEvent.type == EventType.MouseUp)
                                    isDraggingPreviewSlope = false;
                            }
                        }
                        EditorGUILayout.EndFadeGroup();
                        EditorGUILayout.BeginHorizontal();
                        EditorGUI.BeginChangeCheck();
                        PropField("NoiseType", p_SlopeNoiseType, helpActive);
                        if (EditorGUI.EndChangeCheck())
                        {
                            m_needUpdateGeneration = true;
                        }
                        EditorGUILayout.EndHorizontal();

                        EditorGUI.BeginChangeCheck();
                        m_showSlopeNoiseSettings.target = p_SlopeNoiseType.enumValueIndex != 0;
                        if (EditorGUILayout.BeginFadeGroup(m_showSlopeNoiseSettings.faded))
                        {
                            //We invert the noise size value and offer a slider between 0 and 300
                            //a value of 1 = small pattern and 300 = large pattern is easier to understand
                            //than setting the size to 0.0001 when you want large noise patterns
                            FloatField("NoiseSize", p_SlopeNoiseSize, 1, -1, -300, 300, helpActive);
                            PropField("NoiseOctaves", p_SlopeNoiseOctaves, helpActive);
                            EditorGUI.indentLevel++;
                            if (p_SlopeNoiseOctaves.intValue <= 1)
                            {
                                GUI.enabled = false;
                            }
                            FloatField("NoiseLacunarity", p_SlopeNoiseLacunarity, 2, -2, -200, 200, helpActive);
                            FloatField("NoisePersistence", p_SlopeNoisePersistence, 0, 10, helpActive);
                            GUI.enabled = originalGUIState;
                            EditorGUI.indentLevel--;
                            FloatField("NoiseMult", p_SlopeNoiseMult, -10, 10, helpActive);
                            FloatField("NoiseMod", p_SlopeNoiseMod, -1, 1, -100, 100, helpActive);
                            EditorGUILayout.Space();
                            PropField("Noise2Type", p_SlopeNoise2Type, helpActive);
                            m_showSlopeNoise2Settings.target = p_SlopeNoise2Type.enumValueIndex != 0;
                            if (EditorGUILayout.BeginFadeGroup(m_showSlopeNoise2Settings.faded))
                            {
                                PropField("Noise2Method", p_SlopeNoise2Method, helpActive);
                                //We invert the noise size value and offer a slider between 0 and 300
                                //a value of 1 = small pattern and 300 = large pattern is easier to understand
                                //than setting the size to 0.0001 when you want large noise patterns
                                FloatField("NoiseSize", p_SlopeNoise2Size, -1, 1, -300, 300, helpActive);
                                PropField("NoiseOctaves", p_SlopeNoise2Octaves, helpActive);
                                EditorGUI.indentLevel++;
                                if (p_SlopeNoise2Octaves.intValue <= 1)
                                {
                                    GUI.enabled = false;
                                }
                                FloatField("NoiseLacunarity", p_SlopeNoise2Lacunarity, 2, -2, -200, 200, helpActive);
                                FloatField("NoisePersistence", p_SlopeNoise2Persistence, 0, 10, helpActive);
                                GUI.enabled = originalGUIState;
                                EditorGUI.indentLevel--;
                                FloatField("NoiseMult", p_SlopeNoise2Mult, -10, 10, helpActive);
                                FloatField("NoiseMod", p_SlopeNoise2Mod, -1, 1, -100, 100, helpActive);
                                EditorGUILayout.Space();
                            }
                            EditorGUILayout.EndFadeGroup();
                        }
                        EditorGUILayout.EndFadeGroup();
                        if (EditorGUI.EndChangeCheck())
                        {
                            m_needUpdateGeneration = true;
                        }
                    }
                    EditorGUILayout.EndFadeGroup();
                }

                EditorGUILayout.Space();


                ////////////////////////////////////
                ///COLOR VARIATION
                ////////////////////////////////////

                m_showColorVariationSettings = StormEditorUtils.LongFoldout(m_showColorVariationSettings, "ColorSectionLabel", m_editorUtils, helpActive);
                if (m_showColorVariationSettings)
                {
                    EditorGUI.BeginChangeCheck();

                    PropField("ColorTransition", p_ColorTransition, helpActive);
                    //PropField("ColorRandomization", p_ColorRandomization, helpActive);
                    p_ColorRandomization.floatValue = m_editorUtils.Slider("ColorRandomization", p_ColorRandomization.floatValue * 100f, 0f, 200f, helpActive) / 100f;
                    if (EditorGUI.EndChangeCheck())
                    {
                        m_needUpdateGeneration = true;
                    }
                }
                EditorGUI.indentLevel--;
            }
            EditorGUI.indentLevel--;
        }

        public void RenderSettingsPanel(bool helpActive)
        {
            EditorGUI.indentLevel++;
            bool originalGUIState = GUI.enabled;

            ////////////////////////////////////
            ///RENDERING
            ////////////////////////////////////

            m_showRenderingSettings = StormEditorUtils.LongFoldout(m_showRenderingSettings, "RenderSectionLabel", m_editorUtils, helpActive, new Color(0f, 0f, 0f, 0.33f));
            if (m_showRenderingSettings)
            {
                EditorGUI.BeginChangeCheck();
                PropField("ObjectType", p_ObjectType, helpActive);
                PropField("DisableRender", p_DisableRender, helpActive);
                PropField("RenderType", p_RenderType, helpActive);
                PropField("CombineType", p_CombineType, helpActive);
                if (EditorGUI.EndChangeCheck())
                {
                    m_needUpdateRender = true;
                }
                EditorGUI.BeginChangeCheck();
                PropField("RenderOffset", p_RenderOffset, helpActive);
                PropField("RenderRotation", p_RenderRotation, helpActive);
                PropField("RenderScale", p_RenderScale, helpActive);
                if (EditorGUI.EndChangeCheck())
                {
                    m_needUpdateLOD = true;
                }
            }


            EditorGUILayout.Space();

            ////////////////////////////////////
            ///RENDERING
            ////////////////////////////////////

            m_showLODSettings = StormEditorUtils.LongFoldout(m_showLODSettings, "LODSectionLabel", m_editorUtils, helpActive, new Color(0f, 0f, 0f, 0.33f));
            if (m_showLODSettings)
            {
                EditorGUI.BeginChangeCheck();
                //PropField("LODFade", p_LODFade, helpActive);
                p_LODMix.floatValue = m_editorUtils.Slider("LODMix", p_LODMix.floatValue * 100f, 0f, 100f, helpActive) / 100f;
                if (EditorGUI.EndChangeCheck())
                {
                    m_needUpdateLOD = true;
                }

                //LODs
                ++EditorGUI.indentLevel;
                int numLODs = p_LODs.arraySize;
                GUIContent lodFoldoutContent = m_editorUtils.GetContent("LODs");
                lodFoldoutContent.text = numLODs + lodFoldoutContent.text;
                p_LODs.isExpanded = m_editorUtils.Foldout(p_LODs.isExpanded, lodFoldoutContent, true);
                m_lodFoldout.target = p_LODs.isExpanded;
                m_editorUtils.InlineHelp("LODs", helpActive);
                if (EditorGUILayout.BeginFadeGroup(m_lodFoldout.faded))
                {
                    ++EditorGUI.indentLevel;
                    if (p_LODs.hasMultipleDifferentValues)
                    {
                        m_editorUtils.Label("LODsMultipleSelectedMessage");
                    }
                    else
                    {
                        if (m_lodItemFoldouts.Length != numLODs)
                            System.Array.Resize(ref m_lodItemFoldouts, numLODs);
                        if (m_lodRenderGroupFoldouts.Length != numLODs)
                            System.Array.Resize(ref m_lodRenderGroupFoldouts, numLODs);
                        Vector4 lodSplits = p_LODSplits.vector4Value;
                        bool changedLODSplit = false;

                        for (int lod = 0; lod < numLODs; ++lod)
                        {
                            SerializedProperty lodProp = p_LODs.GetArrayElementAtIndex(lod);
                            LODDrawer(lodProp, lod, ref m_lodItemFoldouts[lod], ref m_lodRenderGroupFoldouts[lod], helpActive);
                            --EditorGUI.indentLevel;
                            switch (lod)
                            {
                                case 0:
                                    if (numLODs > 1)
                                    {
                                        EditorGUI.BeginChangeCheck();
                                        float x = m_editorUtils.Slider("LODSplit0", lodSplits.x, 0, 1, helpActive);
                                        if (x < 0)
                                            x = 0;
                                        else if (x > 1)
                                            x = 1;
                                        if (EditorGUI.EndChangeCheck() || lodSplits.x != x)
                                        {
                                            lodSplits.x = x;
                                            changedLODSplit = true;
                                        }
                                    }
                                    else if (lodSplits.x != 1)
                                    {
                                        lodSplits.x = 1;
                                        changedLODSplit = true;
                                    }
                                    break;
                                case 1:
                                    if (numLODs > 2)
                                    {
                                        EditorGUI.BeginChangeCheck();
                                        float y = m_editorUtils.Slider("LODSplit1", lodSplits.y, 0, 1, helpActive);
                                        if (y < lodSplits.x)
                                            y = lodSplits.x;
                                        else if (y > 1)
                                            y = 1;
                                        if (EditorGUI.EndChangeCheck() || lodSplits.y != y)
                                        {
                                            lodSplits.y = y;
                                            changedLODSplit = true;
                                        }
                                    }
                                    else if (lodSplits.y != 1)
                                    {
                                        lodSplits.y = 1;
                                        changedLODSplit = true;
                                    }
                                    break;
                                case 2:
                                    if (numLODs > 3)
                                    {
                                        EditorGUI.BeginChangeCheck();
                                        float z = m_editorUtils.Slider("LODSplit2", lodSplits.z, 0, 1, helpActive);
                                        if (z < lodSplits.y)
                                            z = lodSplits.y;
                                        else if (z > 1)
                                            z = 1;
                                        if (EditorGUI.EndChangeCheck() || lodSplits.z != z)
                                        {
                                            lodSplits.z = z;
                                            changedLODSplit = true;
                                        }
                                    }
                                    else if (lodSplits.z != 1)
                                    {
                                        lodSplits.z = 1;
                                        changedLODSplit = true;
                                    }
                                    break;
                                case 3:
                                    if (numLODs > 4)
                                    {
                                        EditorGUI.BeginChangeCheck();
                                        float w = m_editorUtils.Slider("LODSplit3", lodSplits.w, 0, 1, helpActive);
                                        if (w < lodSplits.z)
                                            w = lodSplits.z;
                                        else if (w > 1)
                                            w = 1;
                                        if (EditorGUI.EndChangeCheck())
                                        {
                                            lodSplits.w = w;
                                            changedLODSplit = true;
                                        }
                                    }
                                    else if (lodSplits.w != 1)
                                    {
                                        lodSplits.w = 1;
                                        changedLODSplit = true;
                                    }
                                    break;
                                default:
                                    break;
                            }
                            ++EditorGUI.indentLevel;
                        }
                        if (p_LODs.arraySize < 5 && m_editorUtils.Button("AddLODButton"))
                        {
                            int idx = p_LODs.arraySize;
                            p_LODs.InsertArrayElementAtIndex(idx);
                            SerializedProperty lodProp = p_LODs.GetArrayElementAtIndex(idx);
                            SerializedProperty propRenderDatas = lodProp.FindPropertyRelative("renderData");
                            if (propRenderDatas.arraySize == 0)
                            {
                                serializedObject.ApplyModifiedProperties();
                                StormFeatureSettings fSettings = (StormFeatureSettings)serializedObject.targetObject;
                                fSettings.LODs[0].renderData = new StormObjectRenderData[1];
                                fSettings.LODs[0].renderData[0] = new StormObjectRenderData();
                                fSettings.LODs[0].renderData[0].Rotation = Quaternion.identity;
                                fSettings.LODs[0].renderData[0].Scale = Vector3.one;
                                serializedObject.Update();
                            }
                            m_needUpdateLOD = true;
                        }
                        for (int lod = numLODs; lod < 4; ++lod)
                        {
                            switch (lod)
                            {
                                case 0:
                                    if (lodSplits.x != -1)
                                    {
                                        lodSplits.x = -1;
                                        changedLODSplit = true;
                                    }
                                    break;
                                case 1:
                                    if (lodSplits.y != -1)
                                    {
                                        lodSplits.y = -1;
                                        changedLODSplit = true;
                                    }
                                    break;
                                case 2:
                                    if (lodSplits.z != -1)
                                    {
                                        lodSplits.z = -1;
                                        changedLODSplit = true;
                                    }
                                    break;
                                case 3:
                                    if (lodSplits.w != -1)
                                    {
                                        lodSplits.w = -1;
                                        changedLODSplit = true;
                                    }
                                    break;
                                default:
                                    break;
                            }
                        }
                        if (changedLODSplit)
                        {
                            p_LODSplits.vector4Value = lodSplits;
                            m_needUpdateLOD = true;
                        }
                    }
                    --EditorGUI.indentLevel;
                }
                EditorGUILayout.EndFadeGroup();
                --EditorGUI.indentLevel;
            }
            EditorGUILayout.Space();



            ////////////////////////////////////
            //SHADOWS
            ////////////////////////////////////

            m_showShadowSettings = StormEditorUtils.LongFoldout(m_showShadowSettings, "LongShadowsSectionLabel", m_editorUtils, helpActive, new Color(0f, 0f, 0f, 0.33f));
            if (m_showShadowSettings)
            {
                if (!StormWorld.Instance.SystemSettings.m_splitShadowRendering)
                {
                    GUI.enabled = false;
                }

                if (!p_customShadowRenderer.hasMultipleDifferentValues)
                {
                    EditorGUI.BeginChangeCheck();
                    m_editorUtils.IntSlider(p_ShadowsUseLOD, -1, p_LODs.arraySize - 1, "ShadowsUseLOD", helpActive);
                    if (EditorGUI.EndChangeCheck() && Core.StormWorld.Instance != null)
                    {
                        Core.StormWorld.Instance.UpdateFeaturesets(true);
                    }
                    m_shadowLODFoldout.target = p_ShadowsUseLOD.intValue < 0;
                    if (EditorGUILayout.BeginFadeGroup(m_shadowLODFoldout.faded))
                    {
                        LODDrawer(p_customShadowRenderer, -1, ref m_shadowLODItemFoldout, ref m_shadowLODRenderGroupFoldouts, helpActive);
                    }
                    EditorGUILayout.EndFadeGroup();
                }

                GUI.enabled = originalGUIState;

                PropField("DrawLongShadows", p_DrawLongShadows, helpActive);
                EditorGUI.indentLevel++;
                if (!p_DrawLongShadows.boolValue)
                {
                    GUI.enabled = false;
                }
                PropField("MaxLongShadowDist", p_MaxLongShadowDist, helpActive);
                GUI.enabled = originalGUIState;
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.Space();

            ////////////////////////////////////
            //FADEOUT EFFECTS
            ////////////////////////////////////

            m_showDensityFadingSettings = StormEditorUtils.LongFoldout(m_showDensityFadingSettings, "DensityFadeSectionLabel", m_editorUtils, helpActive, new Color(0f, 0f, 0f, 0.33f));
            if (m_showDensityFadingSettings)
            {
                EditorGUI.BeginChangeCheck();
                p_DensityFadeStrength.floatValue = m_editorUtils.Slider("DensityFadeStrength", p_DensityFadeStrength.floatValue * 100, 0f, 100f, helpActive) / 100f;
                p_DensityFadeStart.floatValue = m_editorUtils.Slider("DensityFadeStart", p_DensityFadeStart.floatValue * 100, 0f, 100f, helpActive) / 100f;
                p_DensityFadeScaleoutRange.floatValue = m_editorUtils.Slider("DensityFadeScaleoutRange", p_DensityFadeScaleoutRange.floatValue * 100, 0f, 100f, helpActive) / 100f;
                p_ScaleoutRange.floatValue = m_editorUtils.Slider("ScaleoutRange", p_ScaleoutRange.floatValue * 100, 0f, 100f, helpActive) / 100f;
                p_ScaleoutMinScale.floatValue = m_editorUtils.Slider("ScaleoutMinScale", p_ScaleoutMinScale.floatValue * 100, 0f, 100f, helpActive) / 100f;
                if (EditorGUI.EndChangeCheck())
                {
                    m_needUpdateRender = true;
                }
            }

            EditorGUILayout.Space();


            ////////////////////////////////////
            ///COLOR VARIATION
            ////////////////////////////////////

            m_showColorVariationSettings2 = StormEditorUtils.LongFoldout(m_showColorVariationSettings2, "RenderColorSectionLabel", m_editorUtils, helpActive, new Color(0f, 0f, 0f, 0.33f));
            if (m_showColorVariationSettings2)
            {
                EditorGUI.BeginChangeCheck();
                PropField("ColorAPropName", p_ColorAPropName, helpActive);
                PropField("ColorBPropName", p_ColorBPropName, helpActive);
                PropField("ColorA", p_ColorA, helpActive);
                PropField("ColorB", p_ColorB, helpActive);
                if (EditorGUI.EndChangeCheck())
                {
                    m_needUpdateRender = true;
                }
            }
            EditorGUILayout.Space();

            ////////////////////////////////////
            //COLLISION
            ////////////////////////////////////

            m_showCollisionSettings = StormEditorUtils.LongFoldout(m_showCollisionSettings, "CollidersSectionLabel", m_editorUtils, helpActive, new Color(0f, 0f, 0f, 0.33f));
            if (m_showCollisionSettings)
            {
                ++EditorGUI.indentLevel;
                PropField("Colliders", p_Colliders, helpActive);
                --EditorGUI.indentLevel;
            }
            EditorGUI.indentLevel--;
        }

        private void CopyLOD(SerializedProperty original, SerializedProperty copyTo)
        {
            var propRenderDatasA = original.FindPropertyRelative("renderData");
            var propRenderDatasB = copyTo.FindPropertyRelative("renderData");
            propRenderDatasB.arraySize = propRenderDatasA.arraySize;
            for (int o = 0; o < propRenderDatasA.arraySize; ++o)
            {
                var propRenderDataA = propRenderDatasA.GetArrayElementAtIndex(o);
                var propRenderDataB = propRenderDatasB.GetArrayElementAtIndex(o);

                SerializedProperty prop_MeshA = propRenderDataA.FindPropertyRelative("Mesh");
                SerializedProperty prop_MatsA = propRenderDataA.FindPropertyRelative("Materials");
                SerializedProperty prop_OffsetA = propRenderDataA.FindPropertyRelative("Offset");
                SerializedProperty prop_RotationA = propRenderDataA.FindPropertyRelative("Rotation").FindPropertyRelative("value");
                SerializedProperty prop_ScaleA = propRenderDataA.FindPropertyRelative("Scale");
                SerializedProperty prop_shadowModeA = propRenderDataA.FindPropertyRelative("shadowMode");
                SerializedProperty prop_receiveShadowsA = propRenderDataA.FindPropertyRelative("receiveShadows");

                SerializedProperty prop_MeshB = propRenderDataB.FindPropertyRelative("Mesh");
                SerializedProperty prop_MatsB = propRenderDataB.FindPropertyRelative("Materials");
                SerializedProperty prop_OffsetB = propRenderDataB.FindPropertyRelative("Offset");
                SerializedProperty prop_RotationB = propRenderDataB.FindPropertyRelative("Rotation").FindPropertyRelative("value");
                SerializedProperty prop_ScaleB = propRenderDataB.FindPropertyRelative("Scale");
                SerializedProperty prop_shadowModeB = propRenderDataB.FindPropertyRelative("shadowMode");
                SerializedProperty prop_receiveShadowsB = propRenderDataB.FindPropertyRelative("receiveShadows");

                prop_MeshB.objectReferenceValue = prop_MeshA.objectReferenceValue;
                prop_MatsB.arraySize = prop_MatsA.arraySize;
                for (int m = 0; m < prop_MatsA.arraySize; ++m)
                    prop_MatsB.GetArrayElementAtIndex(m).objectReferenceValue = prop_MatsA.GetArrayElementAtIndex(m).objectReferenceValue;
                prop_OffsetB.FindPropertyRelative("x").floatValue = prop_OffsetA.FindPropertyRelative("x").floatValue;
                prop_OffsetB.FindPropertyRelative("y").floatValue = prop_OffsetA.FindPropertyRelative("y").floatValue;
                prop_OffsetB.FindPropertyRelative("z").floatValue = prop_OffsetA.FindPropertyRelative("z").floatValue;
                prop_RotationB.FindPropertyRelative("x").floatValue = prop_RotationA.FindPropertyRelative("x").floatValue;
                prop_RotationB.FindPropertyRelative("y").floatValue = prop_RotationA.FindPropertyRelative("y").floatValue;
                prop_RotationB.FindPropertyRelative("z").floatValue = prop_RotationA.FindPropertyRelative("z").floatValue;
                prop_RotationB.FindPropertyRelative("w").floatValue = prop_RotationA.FindPropertyRelative("w").floatValue;
                prop_ScaleB.FindPropertyRelative("x").floatValue = prop_ScaleA.FindPropertyRelative("x").floatValue;
                prop_ScaleB.FindPropertyRelative("y").floatValue = prop_ScaleA.FindPropertyRelative("y").floatValue;
                prop_ScaleB.FindPropertyRelative("z").floatValue = prop_ScaleA.FindPropertyRelative("z").floatValue;
                prop_shadowModeB.enumValueIndex = prop_shadowModeA.enumValueIndex;
                prop_receiveShadowsB.boolValue = prop_receiveShadowsA.boolValue;
            }
        }

        private void LODDrawer(SerializedProperty lodProp, int lod, ref AnimBool lodFoldout, ref AnimBool[] renderGroupFoldouts, bool helpActive)
        {

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            if (lodFoldout == null)
                lodFoldout = new AnimBool(lodProp.isExpanded, Repaint);
            GUIContent lodPropHeader;
            if (lod < 0)
            {
                lodPropHeader = m_editorUtils.GetContent("ShadowLODLabel");
            }
            else
            {
                lodPropHeader = m_editorUtils.GetContent("LODLabel");
                lodPropHeader.text = string.Format(lodPropHeader.text, lod);
            }
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(EditorGUI.indentLevel * 7 * EditorGUIUtility.pixelsPerPoint);
            lodProp.isExpanded = GUILayout.Toggle(lodProp.isExpanded, lodPropHeader, EditorStyles.foldout, GUILayout.ExpandWidth(true));
            if (lod >= 0 && m_editorUtils.ButtonRight("RemLODButton", GUILayout.ExpandWidth(false)))
            {
                m_removeLOD = lod;
                m_needUpdateRender = true;
            }
            EditorGUILayout.EndHorizontal();
            lodFoldout.target = lodProp.isExpanded;
            if (lod < 0)
            {
                EditorGUILayout.BeginHorizontal();
                m_shadowCopyFromLOD = m_editorUtils.IntSlider("ShadowCopyFromLOD", m_shadowCopyFromLOD, 0, p_LODs.arraySize - 1);
                if (m_editorUtils.Button("ShadowCopyFromLODButton", GUILayout.ExpandWidth(false)))
                {
                    CopyLOD(p_LODs.GetArrayElementAtIndex(m_shadowCopyFromLOD), p_customShadowRenderer);
                    m_needUpdateRender = true;
                }
                EditorGUILayout.EndHorizontal();
                m_editorUtils.InlineHelp("ShadowCopyFromLOD", helpActive);
            }
            if (EditorGUILayout.BeginFadeGroup(lodFoldout.faded))
            {
                var propRenderDatas = lodProp.FindPropertyRelative("renderData");
                if (renderGroupFoldouts?.Length != propRenderDatas.arraySize)
                    System.Array.Resize(ref renderGroupFoldouts, propRenderDatas.arraySize);
                ++EditorGUI.indentLevel;
                for (int o = 0; o < propRenderDatas.arraySize; ++o)
                {
                    var propRenderData = propRenderDatas.GetArrayElementAtIndex(o);
                    if (renderGroupFoldouts[o] == null)
                        renderGroupFoldouts[o] = new AnimBool(propRenderData.isExpanded, Repaint);
                    GUIContent rgPropHeader = m_editorUtils.GetContent("RenderGroupLabel");
                    rgPropHeader.text = string.Format(rgPropHeader.text, o);
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(EditorGUI.indentLevel * 7 * EditorGUIUtility.pixelsPerPoint);
                    propRenderData.isExpanded = GUILayout.Toggle(propRenderData.isExpanded, rgPropHeader, EditorStyles.foldout, GUILayout.ExpandWidth(true));
                    if (m_editorUtils.ButtonRight("RemRenderDataButton", GUILayout.ExpandWidth(false)))
                    {
                        m_removeRenderData = new Vector2Int(lod, o);
                        m_needUpdateRender = true;
                    }
                    EditorGUILayout.EndHorizontal();
                    renderGroupFoldouts[o].target = propRenderData.isExpanded;
                    if (EditorGUILayout.BeginFadeGroup(renderGroupFoldouts[o].faded))
                    {
                        SerializedProperty prop_Mesh = propRenderData.FindPropertyRelative("Mesh");
                        SerializedProperty prop_Mats = propRenderData.FindPropertyRelative("Materials");
                        SerializedProperty prop_Offset = propRenderData.FindPropertyRelative("Offset");
                        SerializedProperty prop_Rotation = propRenderData.FindPropertyRelative("Rotation");
                        SerializedProperty prop_Scale = propRenderData.FindPropertyRelative("Scale");
                        SerializedProperty prop_shadowMode = propRenderData.FindPropertyRelative("shadowMode");
                        SerializedProperty prop_receiveShadows = propRenderData.FindPropertyRelative("receiveShadows");
                        EditorGUI.BeginChangeCheck();
                        m_editorUtils.PropertyField("LOD_Mesh", prop_Mesh, helpActive);
                        Rect r = EditorGUILayout.GetControlRect(true, EditorGUI.GetPropertyHeight(prop_Mats, true));
                        float adjust = EditorGUI.indentLevel * 15f * EditorGUIUtility.pixelsPerPoint;
                        r.x += adjust;
                        r.width -= adjust;
                        int oldIndentLevel = EditorGUI.indentLevel;
                        EditorGUI.indentLevel = 0;
                        EditorGUI.PropertyField(r, prop_Mats, m_editorUtils.GetContent("LOD_Mats"), true);
                        EditorGUI.indentLevel = oldIndentLevel;
                        if (EditorGUI.EndChangeCheck())
                            m_needUpdateRender = true;
                        m_editorUtils.InlineHelp("LOD_Mats", helpActive);
                        m_editorUtils.PropertyField("LOD_Offset", prop_Offset, helpActive);
                        m_editorUtils.PropertyField("LOD_Rotation", prop_Rotation, helpActive);
                        m_editorUtils.PropertyField("LOD_Scale", prop_Scale, helpActive);
                        if (EditorGUI.EndChangeCheck())
                            m_needUpdateLOD = true;
                        EditorGUI.BeginChangeCheck();
                        m_editorUtils.PropertyField("LOD_ShadowMode", prop_shadowMode, helpActive);
                        m_editorUtils.PropertyField("LOD_ReceiveShadows", prop_receiveShadows, helpActive);
                        if (EditorGUI.EndChangeCheck())
                            m_needUpdateRender = true;
                    }
                    EditorGUILayout.EndFadeGroup();
                }
                --EditorGUI.indentLevel;
                SerializedProperty prop_DisableRender = lodProp.FindPropertyRelative("DisableRender");
                m_editorUtils.PropertyField("LOD_DisableRender", prop_DisableRender, helpActive);
                if (propRenderDatas.arraySize < 5 && m_editorUtils.Button("AddRenderDataButton"))
                {
                    int idx = propRenderDatas.arraySize;
                    propRenderDatas.InsertArrayElementAtIndex(idx);
                    if (idx == 0)
                    {
                        SerializedProperty propRenderData = propRenderDatas.GetArrayElementAtIndex(idx);
                        SerializedProperty prop_Rotation = propRenderData.FindPropertyRelative("Rotation");
                        SerializedProperty prop_Scale = propRenderData.FindPropertyRelative("Scale");
                        prop_Rotation.quaternionValue = Quaternion.identity;
                        prop_Scale.vector3Value = Vector3.one;
                    }
                }
            }
            EditorGUILayout.EndFadeGroup();
            EditorGUILayout.EndVertical();
        }

        private void FloatField(string key, SerializedProperty prop, float min, float max, bool helpActive)
        {
            EditorGUI.showMixedValue = prop.hasMultipleDifferentValues;
            EditorGUI.BeginChangeCheck();
            float newVal = m_editorUtils.Slider(key, prop.floatValue, min, max, helpActive);
            if (EditorGUI.EndChangeCheck())
                prop.floatValue = newVal;
            EditorGUI.showMixedValue = false;
        }
        private void FloatField(string key, SerializedProperty prop, float min, float max, float displayMin, float displayMax, bool helpActive)
        {
            EditorGUI.showMixedValue = prop.hasMultipleDifferentValues;
            EditorGUI.BeginChangeCheck();
            float val = Mathf.Lerp(displayMin, displayMax, Mathf.InverseLerp(min, max, prop.floatValue));
            float newVal = m_editorUtils.Slider(key, val, displayMin, displayMax, helpActive);
            if (EditorGUI.EndChangeCheck())
                prop.floatValue = Mathf.Lerp(min, max, Mathf.InverseLerp(displayMin, displayMax, newVal));
            EditorGUI.showMixedValue = false;
        }
        private void PropField(string key, SerializedProperty prop, bool helpActive)
        {
            EditorGUI.showMixedValue = prop.hasMultipleDifferentValues;
            m_editorUtils.PropertyField(key, true, prop, helpActive);
            EditorGUI.showMixedValue = false;
        }

        private void ToggleMinMax(string key, SerializedProperty limitMin, SerializedProperty limitMax, SerializedProperty minValue, SerializedProperty maxValue, bool helpActive = false)
        {
            //This is a combination of workarounds to get those controls rendering on one line
            //with sensible spacing between them. If you change anything in here, please test
            //if all controls still work afterwards and the controls behave nicely when resizing the
            //width of the Inspector.

            bool originalGUIState = GUI.enabled;

            EditorGUILayout.BeginHorizontal();
            //A bit extra space to align with the other indented labels
            GUILayout.Space(30);
            //General Label
            m_editorUtils.Label(key, GUILayout.Width(EditorGUIUtility.labelWidth - 30));

            //temporarily overwrite the label width, otherwise the min max labels will break the layout.
            //we will store the real label width and restore it later, if we do not do this this may
            //affect other editors!
            float currentLabelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 25;

            //Min Label
            if (EditorGUIUtility.currentViewWidth > 350f)
                m_editorUtils.Label("Min");

            //Min Checkbox
            if (EditorGUIUtility.currentViewWidth > 280f)
            {
                GUILayout.Space(-30);
                bool tempBool = limitMin.boolValue;
                if (limitMin.hasMultipleDifferentValues)
                {
                    EditorGUI.showMixedValue = true;
                }
                EditorGUI.BeginChangeCheck();
                tempBool = EditorGUILayout.Toggle(tempBool);//, GUILayout.Width(30));
                if (EditorGUI.EndChangeCheck())
                {
                    limitMin.boolValue = tempBool;
                }
                EditorGUI.showMixedValue = false;

            }

            //Min Value
            if (!limitMin.boolValue)
            {
                GUI.enabled = false;
            }
            GUILayout.Space(-25);
            if (minValue.hasMultipleDifferentValues)
            {
                EditorGUI.showMixedValue = true;
            }
            float tempFloatValue = minValue.floatValue;
            EditorGUI.BeginChangeCheck();
            tempFloatValue = EditorGUILayout.FloatField(tempFloatValue, GUILayout.Width(80));
            if (EditorGUI.EndChangeCheck())
            {
                minValue.floatValue = tempFloatValue;
            }
            EditorGUI.showMixedValue = false;

            GUI.enabled = originalGUIState;
            GUILayout.FlexibleSpace();

            //Max Label
            if (EditorGUIUtility.currentViewWidth > 350f)
                m_editorUtils.Label("Max");

            //Max Checkbox
            if (EditorGUIUtility.currentViewWidth > 280f)
            {
                GUILayout.Space(-30);

                if (limitMax.hasMultipleDifferentValues)
                {
                    EditorGUI.showMixedValue = true;
                }

                bool tempBoolValue = limitMax.boolValue;
                EditorGUI.BeginChangeCheck();
                tempBoolValue = EditorGUILayout.Toggle(tempBoolValue); //, GUILayout.Width(30)); ;
                if (EditorGUI.EndChangeCheck())
                {
                    limitMax.boolValue = tempBoolValue;
                }
                EditorGUI.showMixedValue = false;
            }

            //Max Value
            if (!limitMax.boolValue)
            {
                GUI.enabled = false;
            }
            GUILayout.Space(-25);

            if (maxValue.hasMultipleDifferentValues)
            {
                EditorGUI.showMixedValue = true;
            }
            tempFloatValue = maxValue.floatValue;
            EditorGUI.BeginChangeCheck();
            tempFloatValue = EditorGUILayout.FloatField(tempFloatValue, GUILayout.Width(80));
            if (EditorGUI.EndChangeCheck())
            {
                maxValue.floatValue = tempFloatValue;
            }
            EditorGUI.showMixedValue = false;
            GUI.enabled = originalGUIState;

            EditorGUIUtility.labelWidth = currentLabelWidth;
            EditorGUILayout.EndHorizontal();

            m_editorUtils.InlineHelp(key, helpActive);
        }

        private bool ToggleCurveField(string key, SerializedProperty toggleCurveProp, SerializedProperty animCurve, bool helpActive)
        {
            bool wasChanged = false;
            bool originalGUIState = GUI.enabled;
            EditorGUILayout.BeginHorizontal();
            EditorGUI.BeginChangeCheck();
            PropField(key, toggleCurveProp, false);
            if (!toggleCurveProp.boolValue)
            {
                GUI.enabled = false;
            }
            GUILayout.Space(-50);
            EditorGUILayout.CurveField(animCurve, Color.green, Rect.zero, GUIContent.none);
            GUI.enabled = originalGUIState;
            if (EditorGUI.EndChangeCheck()) { wasChanged = true; }
            EditorGUILayout.EndHorizontal();
            m_editorUtils.InlineHelp(key, helpActive);
            return wasChanged;
        }

        public override bool HasPreviewGUI()
        {
            if (serializedObject.isEditingMultipleObjects)
                return false;
            var propLODs = serializedObject.FindProperty("LODs");
            for (int l = 0; l < propLODs.arraySize; ++l)
            {
                var propLOD = propLODs.GetArrayElementAtIndex(l);
                var propRenderDatas = propLOD.FindPropertyRelative("renderData");
                for (int o = 0; o < propRenderDatas.arraySize; ++o)
                {
                    var propRenderData = propRenderDatas.GetArrayElementAtIndex(o);
                    var propMesh = propRenderData.FindPropertyRelative("Mesh");
                    var propMats = propRenderData.FindPropertyRelative("Materials");
                    if (propMesh.objectReferenceValue != null && propMats.arraySize > 0)
                    {
                        var propMat = propMats.GetArrayElementAtIndex(0);
                        if (propMat.objectReferenceValue != null)
                            return true;
                    }
                }
            }
            return false;
        }
        public override void OnInteractivePreviewGUI(Rect r, GUIStyle background)
        {
            var propLODs = serializedObject.FindProperty("LODs");
            for (int l = 0; l < propLODs.arraySize; ++l)
            {
                var propLOD = propLODs.GetArrayElementAtIndex(l);
                var propRenderDatas = propLOD.FindPropertyRelative("renderData");
                for (int o = 0; o < propRenderDatas.arraySize; ++o)
                {
                    var propRenderData = propRenderDatas.GetArrayElementAtIndex(o);
                    var propMesh = propRenderData.FindPropertyRelative("Mesh");
                    var propMats = propRenderData.FindPropertyRelative("Materials");
                    if (propMesh.objectReferenceValue != null && propMats.arraySize > 0)
                    {
                        var propMat = propMats.GetArrayElementAtIndex(0);
                        if (propMat.objectReferenceValue != null)
                        {
                            background = EditorStyles.textField;
                            if (m_preview == null)
                                m_preview = new MeshPreview(propMesh.objectReferenceValue as Mesh);
                            else if (m_preview.mesh != propMesh.objectReferenceValue as Mesh)
                                m_preview.mesh = propMesh.objectReferenceValue as Mesh;
                            m_preview.OnPreviewGUI(r, background);
                            return;
                        }
                    }
                }
            }
        }

        private void OnUndo()
        {
            serializedObject.Update();
            serializedObject.ApplyModifiedProperties();
        }
        private float MinValue(Keyframe[] keys)
        {
            float ret = float.MaxValue;
            for (int k = 0; k < keys.Length; ++k)
                if (keys[k].value < ret)
                    ret = keys[k].value;
            if (ret == float.MaxValue)
                ret = 0;
            return ret;
        }
    }
}