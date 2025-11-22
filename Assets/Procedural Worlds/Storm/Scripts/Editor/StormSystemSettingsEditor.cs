using UnityEditor;
using PWCommon5;
using ProceduralWorlds.Storm.Core;
using ProceduralWorlds.Storm.Utils;

/// Copyright 2023 Procedural Worlds Ltd
/// Written by Jonathan Williams as part of Storm

namespace ProceduralWorlds.Storm.StormEditor
{
    [CustomEditor(typeof(StormSystemSettings))]
    public class StormSystemSettingsEditor : PWEditor
    {
        private StormSystemSettings m_worldSettings;
        private EditorUtils m_editorUtils;

        SerializedProperty p_loadDistance;
        SerializedProperty p_unloadDistance;
        SerializedProperty p_maxSimultaneousLoad;
        SerializedProperty p_maxLoadMasksPerFrame;
        SerializedProperty p_maxInstancesPerFrame;
        SerializedProperty p_instantFirstLoad;
        SerializedProperty p_cacheTerrainData;
        SerializedProperty p_preventDuplicateInstances;
        SerializedProperty p_createColliders;
        SerializedProperty p_hideColliders;
        SerializedProperty p_renderCellSize;
        SerializedProperty p_splitDenseCells;
        SerializedProperty p_splitInstanceCount;
        SerializedProperty p_splitSubgridCount;
        SerializedProperty p_curveResolution;
        SerializedProperty p_MapPadding;
        SerializedProperty p_useJobs;
        SerializedProperty p_allowFrameDelay;
        SerializedProperty p_cullingTurnOffset;
        SerializedProperty p_cullViewPadding;
        SerializedProperty p_shadowDistanceMultiplier;
        SerializedProperty p_splitShadowRendering;
        SerializedProperty p_renderType;
        SerializedProperty p_combineType;
        SerializedProperty p_allowedCameraTypes;
        SerializedProperty p_cullType;
        SerializedProperty p_cullShader;
        SerializedProperty p_cullSplitShader;
        SerializedProperty p_cullKernelStandard;
        SerializedProperty p_cullKernelSimple;
        SerializedProperty p_cullKernelShadowOnly;
        SerializedProperty p_lodSplitShader;
        SerializedProperty p_lodSplitKernel;
        SerializedProperty p_cullCountShader;
        SerializedProperty p_cullCountKernel;
        SerializedProperty p_cullCountClearShader;
        SerializedProperty p_cullCountClearKernel;
        SerializedProperty p_finalizeShader;
        SerializedProperty p_finalizeKernel;
        SerializedProperty p_MinUpdateMovement;
        SerializedProperty p_MinUpdateTurn;
        SerializedProperty p_ShadowDistanceMult;
        SerializedProperty p_shadowDistanceSRP;
        SerializedProperty p_AlwaysRasterizePolygons;
        SerializedProperty p_MaxTotalPolygonResolution;
        SerializedProperty p_DefaultColorTargetData;
        SerializedProperty p_ColorTargetRange;
        SerializedProperty p_ColorTargetFalloff;
        SerializedProperty p_MeshTerrainTags;
        SerializedProperty p_MeshTerrainResolution;
        SerializedProperty p_ExtraMaterialMode;
        SerializedProperty p_DefaultPolygonResolution;
        SerializedProperty p_DefaultLODProgression;
        SerializedProperty p_DefaultDetailLODs;
        SerializedProperty p_CreateDetailImpostors;
        SerializedProperty p_DefaultLODFade;
        SerializedProperty p_DefaultLODMix;
        SerializedProperty p_DefaultDensity;
        SerializedProperty p_DefaultRenderType;
        SerializedProperty p_IgnoredPrefabs;
        SerializedProperty p_IgnoredLayers;
        SerializedProperty p_IgnoredTags;
        SerializedProperty p_IngestStatic;
        SerializedProperty p_IngestNonStatic;
        SerializedProperty p_ObjectSmallSize;
        SerializedProperty p_ObjectMediumSize;
        SerializedProperty p_IngestGrassNames;
        SerializedProperty p_IngestDetailNames;
        SerializedProperty p_IngestPlantNames;
        SerializedProperty p_IngestTreeNames;
        SerializedProperty p_CreateImpostors;
        SerializedProperty p_BillboardStrings;
        SerializedProperty p_ImpostorBakeShaders;
        SerializedProperty p_ImpostorPostProccess;
        SerializedProperty p_ImpostorFlatReplacement;
        SerializedProperty p_ImpostorType;
        SerializedProperty p_AllowImpostorTypeOverride;
        SerializedProperty p_ImpostorResolution;
        SerializedProperty p_ImpostorOctoSteps;
        SerializedProperty p_ImpostorExportDirectory;
        SerializedProperty p_DebugLODMaterial;
        SerializedProperty p_DebugLODColorProperty;
        SerializedProperty p_DebugLODColorStart;
        SerializedProperty p_DebugLODColorEnd;
        SerializedProperty p_ReplaceRenderShaders;
        SerializedProperty p_ReplacementShader;

        SerializedProperty p_DiffusionProfileAsset;

        bool m_expertMode = false;

        bool m_needsFullReset = false;
        bool m_needsFeaturesetUpdate = false;
        bool m_needsLODUpdate = false;

        bool LoadingHelpShown;
        bool InternalHelpShown;
        bool IngestionHelpShown;
        bool PrefabCreationHelpShown;
        bool ImpostorHelpShown;

        private void OnEnable()
        {
            _ = ((StormSystemSettings)target).ImpostorBakeShaders;
            _ = ((StormSystemSettings)target).ImpostorPostProcess;
            _ = ((StormSystemSettings)target).ImpostorFlatReplacement; //call the get functions to check for empty
            m_needsFullReset = false;
            p_loadDistance = serializedObject.FindProperty("m_loadDistance");
            p_unloadDistance = serializedObject.FindProperty("m_unloadDistance");
            p_maxSimultaneousLoad = serializedObject.FindProperty("m_maxSimultaneousLoad");
            p_maxLoadMasksPerFrame = serializedObject.FindProperty("m_maxLoadMasksPerFrame");
            p_maxInstancesPerFrame = serializedObject.FindProperty("m_maxInstancesPerFrame");
            p_instantFirstLoad = serializedObject.FindProperty("m_instantFirstLoad");
            p_cacheTerrainData = serializedObject.FindProperty("m_cacheTerrainData");
            p_preventDuplicateInstances = serializedObject.FindProperty("m_preventDuplicateInstances");
            p_createColliders = serializedObject.FindProperty("m_createColliders");
            p_hideColliders = serializedObject.FindProperty("m_hideColliders");
            p_renderCellSize = serializedObject.FindProperty("m_renderCellSize");
            p_splitDenseCells = serializedObject.FindProperty("m_splitDenseCells");
            p_splitInstanceCount = serializedObject.FindProperty("m_splitInstanceCount");
            p_splitSubgridCount = serializedObject.FindProperty("m_splitSubgridCount");
            p_curveResolution = serializedObject.FindProperty("m_curveResolution");
            p_MapPadding = serializedObject.FindProperty("m_loadingMapPadding");
            p_useJobs = serializedObject.FindProperty("m_useJobs");
            p_allowFrameDelay = serializedObject.FindProperty("m_allowFrameDelay");
            p_cullingTurnOffset = serializedObject.FindProperty("m_cullTurnOffset");
            p_cullViewPadding = serializedObject.FindProperty("m_cullViewPadding");
            p_shadowDistanceMultiplier = serializedObject.FindProperty("m_shadowDistanceMultiplier");
            p_splitShadowRendering = serializedObject.FindProperty("m_splitShadowRendering");
            p_renderType = serializedObject.FindProperty("m_renderType");
            p_combineType = serializedObject.FindProperty("m_combineType");
            p_allowedCameraTypes = serializedObject.FindProperty("m_allowedCameraTypes");
            p_cullType = serializedObject.FindProperty("m_cullType");
            p_cullShader = serializedObject.FindProperty("m_cullShader");
            p_cullSplitShader = serializedObject.FindProperty("m_cullSplitShader");
            p_cullKernelStandard = serializedObject.FindProperty("m_cullKernelStandard");
            p_cullKernelSimple = serializedObject.FindProperty("m_cullKernelSimple");
            p_cullKernelShadowOnly = serializedObject.FindProperty("m_cullKernelShadowOnly");
            p_lodSplitShader = serializedObject.FindProperty("m_lodSplitShader");
            p_lodSplitKernel = serializedObject.FindProperty("m_lodSplitKernel");
            p_cullCountShader = serializedObject.FindProperty("m_cullCountShader");
            p_cullCountKernel = serializedObject.FindProperty("m_cullCountKernel");
            p_cullCountClearShader = serializedObject.FindProperty("m_cullCountClearShader");
            p_cullCountClearKernel = serializedObject.FindProperty("m_cullCountClearKernel");
            p_finalizeShader = serializedObject.FindProperty("m_finalizeShader");
            p_finalizeKernel = serializedObject.FindProperty("m_finalizeKernel");
            p_MinUpdateMovement = serializedObject.FindProperty("m_minUpdateMovement");
            p_MinUpdateTurn = serializedObject.FindProperty("m_minUpdateTurn");
            p_ShadowDistanceMult = serializedObject.FindProperty("m_shadowDistanceMult");
            p_shadowDistanceSRP = serializedObject.FindProperty("m_shadowDistanceSRP");
            p_AlwaysRasterizePolygons = serializedObject.FindProperty("AlwaysRasterizePolygons");
            p_MaxTotalPolygonResolution = serializedObject.FindProperty("MaxTotalPolygonResolution");
            p_DefaultColorTargetData = serializedObject.FindProperty("DefaultColorTargetData");
            p_ColorTargetRange = serializedObject.FindProperty("ColorTargetRange");
            p_ColorTargetFalloff = serializedObject.FindProperty("ColorTargetFalloff");
            p_MeshTerrainTags = serializedObject.FindProperty("MeshTerrainTags");
            p_MeshTerrainResolution = serializedObject.FindProperty("MeshTerrainResolution");
            p_ExtraMaterialMode = serializedObject.FindProperty("ExtraMaterialMode");
            p_DefaultPolygonResolution = serializedObject.FindProperty("DefaultPolygonResolution");
            p_DefaultLODProgression = serializedObject.FindProperty("DefaultLODProgression");
            p_DefaultDetailLODs = serializedObject.FindProperty("DefaultDetailLODs");
            p_CreateDetailImpostors = serializedObject.FindProperty("DefaultDetailCreateImpostor");
            p_DefaultLODFade = serializedObject.FindProperty("DefaultLODFade");
            p_DefaultLODMix = serializedObject.FindProperty("DefaultLODMix");
            p_DefaultDensity = serializedObject.FindProperty("DefaultDensity");
            p_DefaultRenderType = serializedObject.FindProperty("DefaultRenderType");
            p_IgnoredPrefabs = serializedObject.FindProperty("IgnoredPrefabs");
            p_IgnoredLayers = serializedObject.FindProperty("IgnoredLayers");
            p_IgnoredTags = serializedObject.FindProperty("IgnoredTags");
            p_IngestStatic = serializedObject.FindProperty("IngestStatic");
            p_IngestNonStatic = serializedObject.FindProperty("IngestNonStatic");
            p_ObjectSmallSize = serializedObject.FindProperty("ObjectSmallSize");
            p_ObjectMediumSize = serializedObject.FindProperty("ObjectMediumSize");
            p_IngestGrassNames = serializedObject.FindProperty("ObjectGrassNames");
            p_IngestDetailNames = serializedObject.FindProperty("ObjectDetailNames");
            p_IngestPlantNames = serializedObject.FindProperty("ObjectPlantNames");
            p_IngestTreeNames = serializedObject.FindProperty("ObjectTreeNames");
            p_CreateImpostors = serializedObject.FindProperty("CreateImpostors");
            p_BillboardStrings = serializedObject.FindProperty("m_billboardStrings");
            switch (StormUtils.GetRenderPipline())
            {
                case StormRP.HDRP:
                    p_ImpostorBakeShaders = serializedObject.FindProperty("m_impostorBakeShadersHDRP");
                    p_ImpostorPostProccess = serializedObject.FindProperty("m_impostorPostProcessHDRP");
                    p_ImpostorFlatReplacement = serializedObject.FindProperty("m_impostorFlatReplacementHDRP");
                    p_DebugLODMaterial = serializedObject.FindProperty("DebugLODMaterial_HDRP");
                    p_DebugLODColorProperty = serializedObject.FindProperty("DebugLODColorProperty_HDRP");
                    p_DiffusionProfileAsset = serializedObject.FindProperty("m_defaultDiffusionProfile_HDRP");
                    break;
                case StormRP.URP:
                    p_ImpostorBakeShaders = serializedObject.FindProperty("m_impostorBakeShadersURP");
                    p_ImpostorPostProccess = serializedObject.FindProperty("m_impostorPostProcessURP");
                    p_ImpostorFlatReplacement = serializedObject.FindProperty("m_impostorFlatReplacementURP");
                    p_DebugLODMaterial = serializedObject.FindProperty("DebugLODMaterial_URP");
                    p_DebugLODColorProperty = serializedObject.FindProperty("DebugLODColorProperty_URP");
                    break;
                default:
                    p_ImpostorBakeShaders = serializedObject.FindProperty("m_impostorBakeShadersBuiltIn");
                    p_ImpostorPostProccess = serializedObject.FindProperty("m_impostorPostProcessBuiltIn");
                    p_ImpostorFlatReplacement = serializedObject.FindProperty("m_impostorFlatReplacementBuiltIn");
                    p_DebugLODMaterial = serializedObject.FindProperty("DebugLODMaterial_BuiltIn");
                    p_DebugLODColorProperty = serializedObject.FindProperty("DebugLODColorProperty_BuiltIn");
                    break;
            }
            p_DebugLODColorStart = serializedObject.FindProperty("DebugLODColorStart");
            p_DebugLODColorEnd = serializedObject.FindProperty("DebugLODColorEnd");
            p_ReplaceRenderShaders = serializedObject.FindProperty("ReplaceRenderShaders");
            p_ReplacementShader = serializedObject.FindProperty("ReplacementShader");
            p_ImpostorType = serializedObject.FindProperty("ImpostorType");
            p_AllowImpostorTypeOverride = serializedObject.FindProperty("AllowImpostorTypeOverride");
            p_ImpostorResolution = serializedObject.FindProperty("ImpostorResolution");
            p_ImpostorOctoSteps = serializedObject.FindProperty("ImpostorOctoSteps");
            p_ImpostorExportDirectory = serializedObject.FindProperty("ImpostorExportDirectory");

            Undo.undoRedoPerformed -= OnUndo;
            Undo.undoRedoPerformed += OnUndo;
        }
        private void OnDisable()
        {
            Undo.undoRedoPerformed -= OnUndo;
        }

        public override void OnInspectorGUI()
        {
            if (m_worldSettings == null)
            {
                m_worldSettings = (StormSystemSettings)target;
            }

            if (m_editorUtils == null)
                m_editorUtils = StormApp.GetEditorUtils(this);
            m_editorUtils.Initialize();

            bool LoadingExpanded = p_loadDistance.isExpanded;
            bool InternalExpanded = p_instantFirstLoad.isExpanded;
            bool IngestionExpanded = p_IngestStatic.isExpanded;
            bool PrefabCreationExpanded = p_DefaultDensity.isExpanded;
            bool ImpostorExpanded = p_ImpostorType.isExpanded;

            m_editorUtils.ToggleButton("ExpertMode", ref m_expertMode);
            m_editorUtils.Panel("LoadingPanel", LoadingPanel, ref LoadingExpanded, ref LoadingHelpShown);
            m_editorUtils.Panel("InternalDataPanel", InternalDataPanel, ref InternalExpanded, ref InternalHelpShown);
            m_editorUtils.Panel("IngestionPanel", IngestionPanel, ref IngestionExpanded, ref IngestionHelpShown);
            m_editorUtils.Panel("PrefabCreationPanel", PrefabCreationPanel, ref PrefabCreationExpanded, ref PrefabCreationHelpShown);
            m_editorUtils.Panel("ImpostorPanel", ImpostorPanel, ref ImpostorExpanded, ref ImpostorHelpShown);
            serializedObject.ApplyModifiedProperties();

            p_loadDistance.isExpanded = LoadingExpanded;
            p_instantFirstLoad.isExpanded = InternalExpanded;
            p_IngestStatic.isExpanded = IngestionExpanded;
            p_DefaultDensity.isExpanded = PrefabCreationExpanded;
            p_ImpostorType.isExpanded = ImpostorExpanded;

            if (m_needsFullReset)
            {
                m_needsFullReset = false;
                if (Core.StormWorld.Instance != null && Core.StormWorld.Instance.SystemSettings == target as StormSystemSettings)
                    StormWorld.Instance.ForceFullRefresh();
            }
            if (m_needsFeaturesetUpdate)
            {
                m_needsFeaturesetUpdate = false;
                if (Core.StormWorld.Instance != null && Core.StormWorld.Instance.SystemSettings == target as StormSystemSettings)
                    StormWorld.Instance.UpdateFeaturesets(true);
            }
            if (m_needsLODUpdate)
            {
                m_needsLODUpdate = false;
                if (Core.StormWorld.Instance != null && Core.StormWorld.Instance.SystemSettings == target as StormSystemSettings)
                    StormAPI.OnLODSettingsChanged?.Invoke((StormFeatureType)255);
            }
        }
        private void LoadingPanel(bool showHelp)
        {
            m_editorUtils.PropertyField("InstantFirstLoad", p_instantFirstLoad, showHelp);
            m_editorUtils.PropertyField("CacheTerrainData", p_cacheTerrainData, showHelp);
            m_editorUtils.PropertyField("PreventDuplicateInstances", p_preventDuplicateInstances, showHelp);
            m_editorUtils.PropertyField("CreateColliders", p_createColliders, showHelp);
            m_editorUtils.PropertyField("HideColliders", p_hideColliders, showHelp);
            p_loadDistance.floatValue = m_editorUtils.DelayedFloatField("loadDistance", p_loadDistance.floatValue, showHelp);
            p_unloadDistance.floatValue = m_editorUtils.DelayedFloatField("unloadDistance", p_unloadDistance.floatValue, showHelp);
            p_maxSimultaneousLoad.intValue = m_editorUtils.DelayedIntField("maxSimultaneousLoad", p_maxSimultaneousLoad.intValue, showHelp);
            p_maxLoadMasksPerFrame.intValue = m_editorUtils.DelayedIntField("maxLoadMasksPerFrame", p_maxLoadMasksPerFrame.intValue, showHelp);
            p_maxInstancesPerFrame.intValue = m_editorUtils.DelayedIntField("MaxInstancesPerFrame", p_maxInstancesPerFrame.intValue, showHelp);
        }
        private void InternalDataPanel(bool showHelp)
        {
            EditorGUI.BeginChangeCheck();
            p_renderCellSize.floatValue = m_editorUtils.DelayedFloatField("worldTileSize", p_renderCellSize.floatValue, showHelp);
            m_editorUtils.PropertyField("SplitDenseCells", p_splitDenseCells, showHelp);
            if (p_splitDenseCells.boolValue)
            {
                p_splitInstanceCount.intValue = m_editorUtils.DelayedIntField("SplitInstanceCount", p_splitInstanceCount.intValue, showHelp);
                p_splitSubgridCount.intValue = m_editorUtils.DelayedIntField("SplitSubgridCount", p_splitSubgridCount.intValue, showHelp);
            }
            p_curveResolution.intValue = m_editorUtils.DelayedIntField("curveResolution", p_curveResolution.intValue, showHelp);
            p_MapPadding.floatValue = m_editorUtils.DelayedFloatField("MapPadding", p_MapPadding.floatValue, showHelp);
            if (EditorGUI.EndChangeCheck())
                m_needsFullReset = true;
            m_editorUtils.PropertyField("useJobs", p_useJobs, showHelp);
            EditorGUI.BeginChangeCheck();
            m_editorUtils.PropertyField("allowFrameDelay", p_allowFrameDelay, showHelp);
            if (m_expertMode)
            {
                p_cullingTurnOffset.floatValue = m_editorUtils.FloatField("CullingTurnOffset", p_cullingTurnOffset.floatValue, showHelp);
                p_cullViewPadding.floatValue = m_editorUtils.FloatField("ViewPadding", p_cullViewPadding.floatValue, showHelp);
            }
            else
            {
                m_editorUtils.Slider(p_cullingTurnOffset, 0f, 2f, "CullingTurnOffset", showHelp);
                m_editorUtils.Slider(p_cullViewPadding, 0, 50f, "ViewPadding", showHelp);
            }
            m_editorUtils.PropertyField("MinUpdateMovement", p_MinUpdateMovement, showHelp);
            m_editorUtils.PropertyField("MinUpdateTurn", p_MinUpdateTurn, showHelp);
            m_editorUtils.PropertyField("ShadowExtensionMult", p_ShadowDistanceMult, showHelp);
            //m_editorUtils.PropertyField("UseNewShadowCalc", p_useNewShadowCalc, showHelp);
            m_editorUtils.PropertyField("SplitShadowRendering", p_splitShadowRendering, showHelp);
            if (EditorGUI.EndChangeCheck() && StormWorld.Instance != null)
                m_needsLODUpdate = true;
            EditorGUI.BeginChangeCheck();
            m_editorUtils.PropertyField("p_AlwaysRasterizePolygons", p_AlwaysRasterizePolygons, showHelp);
            if (p_AlwaysRasterizePolygons.boolValue)
            {
                m_editorUtils.PropertyField("p_DefaultPolygonResolution", p_DefaultPolygonResolution, showHelp);
                m_editorUtils.PropertyField("p_MaxTotalPolygonResolution", p_MaxTotalPolygonResolution, showHelp);
            }
            if (EditorGUI.EndChangeCheck())
                m_needsFullReset = true;
            EditorGUI.BeginChangeCheck();
            if (m_expertMode)
            {
                if (StormUtils.GetRenderPipline() == StormRP.Builtin)
                    p_shadowDistanceMultiplier.floatValue = m_editorUtils.FloatField("ShadowDistanceMultiplier", p_shadowDistanceMultiplier.floatValue, showHelp);
                else
                    p_shadowDistanceSRP.floatValue = m_editorUtils.FloatField("ShadowDistanceSRP", p_shadowDistanceSRP.floatValue, showHelp);
            }
            else
            {
                if (StormUtils.GetRenderPipline() == StormRP.Builtin)
                    m_editorUtils.Slider(p_shadowDistanceMultiplier, 0f, 5f, "ShadowDistanceMultiplier", showHelp);
                else
                    m_editorUtils.Slider(p_shadowDistanceSRP, 0f, 2000f, "ShadowDistanceSRP", showHelp);
            }
            if (EditorGUI.EndChangeCheck() && StormWorld.Instance != null)
                m_needsLODUpdate = true;
            EditorGUI.BeginChangeCheck();
            m_editorUtils.PropertyField("ExtraMaterialMode", p_ExtraMaterialMode, showHelp);
            if (EditorGUI.EndChangeCheck() && StormWorld.Instance != null)
                m_needsFeaturesetUpdate = true;
            EditorGUI.BeginChangeCheck();
            m_editorUtils.PropertyField("renderType", p_renderType, showHelp);
            m_editorUtils.PropertyField("allowedCameraTypes", p_allowedCameraTypes, showHelp);
            //*/
            m_editorUtils.PropertyField("cullType", p_cullType, showHelp);
            if (EditorGUI.EndChangeCheck())
                m_needsFullReset = true;
            if (p_cullType.enumValueIndex == 1)
            {
                EditorGUI.BeginChangeCheck();
                m_editorUtils.PropertyField("combineType", p_combineType, showHelp);
                if (EditorGUI.EndChangeCheck())
                {
                    StormWorld.ForceCullingUpdate = true;
                    StormUtils.ForceEditorSceneviewRedraw();
                    if (StormWorld.Instance != null)
                        StormWorld.Instance.Update();
                }
            }
            else
            {
                EditorGUI.BeginChangeCheck();
                if (p_cullType.enumValueIndex == 0)
                    m_editorUtils.PropertyField("cullShader", p_cullShader, showHelp);
                else
                    m_editorUtils.PropertyField("cullShader", p_cullSplitShader, showHelp);
                m_editorUtils.PropertyField("cullKernelStandard", p_cullKernelStandard, showHelp);
                m_editorUtils.PropertyField("cullKernelSimple", p_cullKernelSimple, showHelp);
                m_editorUtils.PropertyField("cullKernelShadowOnly", p_cullKernelShadowOnly, showHelp);

                if (p_cullType.enumValueIndex == 2)
                {
                    m_editorUtils.PropertyField("lodShader", p_lodSplitShader, showHelp);
                    m_editorUtils.PropertyField("lodSplitKernel", p_lodSplitKernel, showHelp);
                }

                m_editorUtils.PropertyField("cullCountShader", p_cullCountShader, showHelp);
                m_editorUtils.PropertyField("cullCountKernel", p_cullCountKernel, showHelp);
                m_editorUtils.PropertyField("cullCountClearShader", p_cullCountClearShader, showHelp);
                m_editorUtils.PropertyField("cullCountClearKernel", p_cullCountClearKernel, showHelp);
                m_editorUtils.PropertyField("finalizeShader", p_finalizeShader, showHelp);
                m_editorUtils.PropertyField("finalizeKernel", p_finalizeKernel, showHelp);
                if (EditorGUI.EndChangeCheck())
                    m_needsFullReset = true;
            }
            //*/
#if PW_STORM_HDRP
            EditorGUI.BeginChangeCheck();
            if (StormUtils.GetRenderPipline() == StormRP.HDRP)
                p_DiffusionProfileAsset.objectReferenceValue = m_editorUtils.ObjectField("p_DiffusionProfileAsset", p_DiffusionProfileAsset.objectReferenceValue, typeof(UnityEngine.Rendering.HighDefinition.DiffusionProfileSettings), false, showHelp);
            if (EditorGUI.EndChangeCheck())
                m_needsFullReset = true;
#endif
            m_editorUtils.PropertyField("DebugLODMaterial", p_DebugLODMaterial, showHelp);
            m_editorUtils.PropertyField("DebugLODColorProperty", p_DebugLODColorProperty, showHelp);
            m_editorUtils.PropertyField("DebugLODColorStart", p_DebugLODColorStart, showHelp);
            m_editorUtils.PropertyField("DebugLODColorEnd", p_DebugLODColorEnd, showHelp);

            EditorGUI.BeginChangeCheck();
            m_editorUtils.PropertyField("ReplaceRenderShaders", p_ReplaceRenderShaders, showHelp);
            if (p_ReplaceRenderShaders.boolValue)
            {
                ++EditorGUI.indentLevel;
                m_editorUtils.PropertyField("ReplacementShader", p_ReplacementShader, showHelp);
                --EditorGUI.indentLevel;
            }
            if (EditorGUI.EndChangeCheck())
                m_needsFullReset = true;
        }
        private void IngestionPanel(bool showHelp)
        {
            m_editorUtils.PropertyField("MeshTerrainResolution", p_MeshTerrainResolution, showHelp);
            ++EditorGUI.indentLevel;
            m_editorUtils.PropertyField("MeshTerrainTags", p_MeshTerrainTags, showHelp);
            --EditorGUI.indentLevel;
            m_editorUtils.PropertyField("DefaultColorTargetData", p_DefaultColorTargetData, showHelp);
            m_editorUtils.PropertyField("ColorTargetRange", p_ColorTargetRange, showHelp);
            m_editorUtils.PropertyField("ColorTargetFalloff", p_ColorTargetFalloff, showHelp);
            ++EditorGUI.indentLevel;
            m_editorUtils.PropertyField("IgnoredPrefabs", p_IgnoredPrefabs, showHelp);
            m_editorUtils.PropertyField("p_IgnoredLayers", p_IgnoredLayers, showHelp);
            m_editorUtils.PropertyField("p_IgnoredTags", p_IgnoredTags, showHelp);
            --EditorGUI.indentLevel;
            m_editorUtils.PropertyField("p_IngestStatic", p_IngestStatic, showHelp);
            m_editorUtils.PropertyField("p_IngestNonStatic", p_IngestNonStatic, showHelp);
        }
        private void PrefabCreationPanel(bool showHelp)
        {
            p_DefaultLODProgression.animationCurveValue = m_editorUtils.CurveField("DefaultLODProgression", p_DefaultLODProgression.animationCurveValue, showHelp);
            ++EditorGUI.indentLevel;
            m_editorUtils.PropertyField("DefaultDetailLODs", p_DefaultDetailLODs, showHelp);
            --EditorGUI.indentLevel;
            m_editorUtils.PropertyField("p_CreateDetailImpostors", p_CreateDetailImpostors, showHelp);
            if (m_expertMode)
            {
                p_DefaultLODFade.floatValue = m_editorUtils.FloatField("DefaultLODFade", p_DefaultLODFade.floatValue, showHelp);
                p_DefaultLODMix.floatValue = m_editorUtils.FloatField("DefaultLODMix", p_DefaultLODMix.floatValue, showHelp);
                p_DefaultDensity.floatValue = m_editorUtils.FloatField("DefaultDensity", p_DefaultDensity.floatValue, showHelp);
            }
            else
            {
                m_editorUtils.Slider(p_DefaultLODFade, 0, 1, "DefaultLODFade", showHelp);
                m_editorUtils.Slider(p_DefaultLODMix, 0, 1, "DefaultLODMix", showHelp);
                m_editorUtils.Slider(p_DefaultDensity, 1, 200, "DefaultDensity", showHelp);
            }
            m_editorUtils.PropertyField("DefaultRenderType", p_DefaultRenderType, showHelp);
            ++EditorGUI.indentLevel;
            m_editorUtils.PropertyField("p_IngestGrassNames", p_IngestGrassNames, showHelp);
            m_editorUtils.PropertyField("p_IngestDetailNames", p_IngestDetailNames, showHelp);
            m_editorUtils.PropertyField("p_IngestPlantNames", p_IngestPlantNames, showHelp);
            m_editorUtils.PropertyField("p_IngestTreeNames", p_IngestTreeNames, showHelp);
            --EditorGUI.indentLevel;
            m_editorUtils.PropertyField("p_ObjectSmallSize", p_ObjectSmallSize, showHelp);
            m_editorUtils.PropertyField("p_ObjectMediumSize", p_ObjectMediumSize, showHelp);
        }
        private void ImpostorPanel(bool showHelp)
        {
            p_CreateImpostors.boolValue = m_editorUtils.Toggle("p_CreateImpostors", p_CreateImpostors.boolValue, showHelp);
            ++EditorGUI.indentLevel;
            m_editorUtils.PropertyField("p_BillboardStrings", p_BillboardStrings, showHelp);
            m_editorUtils.PropertyField("p_ImpostorBakeShaders", p_ImpostorBakeShaders, showHelp);
            m_editorUtils.PropertyField("p_ImpostorPostProccess", p_ImpostorPostProccess, showHelp);
            m_editorUtils.PropertyField("p_ImpostorFlatReplacement", p_ImpostorFlatReplacement, showHelp);
            --EditorGUI.indentLevel;
            m_editorUtils.PropertyField("p_ImpostorType", p_ImpostorType, showHelp);
            m_editorUtils.PropertyField("p_AllowImpostorTypeOverride", p_AllowImpostorTypeOverride, showHelp);
            m_editorUtils.PropertyField("p_ImpostorResolution", p_ImpostorResolution, showHelp);
            m_editorUtils.PropertyField("p_ImpostorOctoSteps", p_ImpostorOctoSteps, showHelp);
            m_editorUtils.PropertyField("p_ImpostorExportDirectory", p_ImpostorExportDirectory, showHelp);
        }
        private void OnUndo()
        {
            serializedObject.Update();
            serializedObject.ApplyModifiedProperties();
        }
    }
}