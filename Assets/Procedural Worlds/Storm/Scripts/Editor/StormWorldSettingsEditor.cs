using UnityEditor;
using PWCommon5;
using ProceduralWorlds.Storm.Core;
using ProceduralWorlds.Storm.Utils;
using UnityEngine;

/// Copyright 2023 Procedural Worlds Ltd
/// Written by Jonathan Williams as part of Storm

namespace ProceduralWorlds.Storm.StormEditor
{
    [CustomEditor(typeof(StormWorldSettings))]
    [System.Obsolete]
    public class StormWorldSettingsEditor : PWEditor
    {
        private StormWorldSettings m_worldSettings;
        private EditorUtils m_editorUtils;

        SerializedProperty p_globalLODBias;
        SerializedProperty p_grassLODBias;
        SerializedProperty p_detailLODBias;
        SerializedProperty p_plantLODBias;
        SerializedProperty p_treeLODBias;
        SerializedProperty p_smallObjLODBias;
        SerializedProperty p_mediumObjLODBias;
        SerializedProperty p_largeObjLODBias;
        SerializedProperty p_grassDensity;
        SerializedProperty p_detailDensity;
        SerializedProperty p_plantDensity;
        SerializedProperty p_treeDensity;
        SerializedProperty p_smallObjDensity;
        SerializedProperty p_mediumObjDensity;
        SerializedProperty p_largeObjDensity;
        SerializedProperty p_grassDistance;
        SerializedProperty p_detailDistance;
        SerializedProperty p_plantDistance;
        SerializedProperty p_treeDistance;
        SerializedProperty p_smallObjDistance;
        SerializedProperty p_mediumObjDistance;
        SerializedProperty p_largeObjDistance;
        SerializedProperty p_colliderDistance;
        SerializedProperty p_grassScaleoutMult;
        SerializedProperty p_detailScaleoutMult;
        SerializedProperty p_plantScaleoutMult;
        SerializedProperty p_treeScaleoutMult;
        SerializedProperty p_smallObjScaleoutMult;
        SerializedProperty p_mediumObjScaleoutMult;
        SerializedProperty p_largeObjScaleoutMult;
        SerializedProperty p_grassLODMixMult;
        SerializedProperty p_detailLODMixMult;
        SerializedProperty p_plantLODMixMult;
        SerializedProperty p_treeLODMixMult;
        SerializedProperty p_smallObjLODMixMult;
        SerializedProperty p_mediumObjLODMixMult;
        SerializedProperty p_largeObjLODMixMult;
        SerializedProperty p_grassDensityFadeMult;
        SerializedProperty p_detailDensityFadeMult;
        SerializedProperty p_plantDensityFadeMult;
        SerializedProperty p_treeDensityFadeMult;
        SerializedProperty p_smallObjDensityFadeMult;
        SerializedProperty p_mediumObjDensityFadeMult;
        SerializedProperty p_largeObjDensityFadeMult;
        SerializedProperty p_globalLODBiasMaxUI;
        SerializedProperty p_grassLODBiasMaxUI;
        SerializedProperty p_detailLODBiasMaxUI;
        SerializedProperty p_plantLODBiasMaxUI;
        SerializedProperty p_treeLODBiasMaxUI;
        SerializedProperty p_smallObjLODBiasMaxUI;
        SerializedProperty p_mediumObjLODBiasMaxUI;
        SerializedProperty p_largeObjLODBiasMaxUI;
        SerializedProperty p_grassDensityMaxUI;
        SerializedProperty p_detailDensityMaxUI;
        SerializedProperty p_plantDensityMaxUI;
        SerializedProperty p_treeDensityMaxUI;
        SerializedProperty p_smallObjDensityMaxUI;
        SerializedProperty p_mediumObjDensityMaxUI;
        SerializedProperty p_largeObjDensityMaxUI;
        SerializedProperty p_grassDistanceMaxUI;
        SerializedProperty p_detailDistanceMaxUI;
        SerializedProperty p_plantDistanceMaxUI;
        SerializedProperty p_treeDistanceMaxUI;
        SerializedProperty p_smallObjDistanceMaxUI;
        SerializedProperty p_mediumObjDistanceMaxUI;
        SerializedProperty p_largeObjDistanceMaxUI;
        SerializedProperty p_colliderDistanceMaxUI;
        SerializedProperty p_grassScaleoutMultMaxUI;
        SerializedProperty p_detailScaleoutMultMaxUI;
        SerializedProperty p_plantScaleoutMultMaxUI;
        SerializedProperty p_treeScaleoutMultMaxUI;
        SerializedProperty p_smallObjScaleoutMultMaxUI;
        SerializedProperty p_mediumObjScaleoutMultMaxUI;
        SerializedProperty p_largeObjScaleoutMultMaxUI;
        SerializedProperty p_grassLODMixMultMaxUI;
        SerializedProperty p_detailLODMixMultMaxUI;
        SerializedProperty p_plantLODMixMultMaxUI;
        SerializedProperty p_treeLODMixMultMaxUI;
        SerializedProperty p_smallObjLODMixMultMaxUI;
        SerializedProperty p_mediumObjLODMixMultMaxUI;
        SerializedProperty p_largeObjLODMixMultMaxUI;
        SerializedProperty p_grassDensityFadeMultMaxUI;
        SerializedProperty p_detailDensityFadeMultMaxUI;
        SerializedProperty p_plantDensityFadeMultMaxUI;
        SerializedProperty p_treeDensityFadeMultMaxUI;
        SerializedProperty p_smallObjDensityFadeMultMaxUI;
        SerializedProperty p_mediumObjDensityFadeMultMaxUI;
        SerializedProperty p_largeObjDensityFadeMultMaxUI;
        SerializedProperty p_loadDistance;
        SerializedProperty p_unloadDistance;
        SerializedProperty p_maxSimultaneousLoad;
        SerializedProperty p_maxLoadMasksPerFrame;
        SerializedProperty p_maxInstancesPerFrame;
        SerializedProperty p_instantFirstLoad;
        SerializedProperty p_renderCellSize;
        SerializedProperty p_curveResolution;
        SerializedProperty p_heightResolution;
        SerializedProperty p_useJobs;
        SerializedProperty p_allowFrameDelay;
        SerializedProperty p_cullingTurnOffset;
        SerializedProperty p_shadowDistanceMultiplier;
        SerializedProperty p_splitShadowRendering;
        SerializedProperty p_renderType;
        SerializedProperty p_cullType;
        SerializedProperty p_cullShader;
        SerializedProperty p_cullKernelStandard;
        SerializedProperty p_cullKernelSimple;
        SerializedProperty p_cullKernelShadowOnly;
        SerializedProperty p_cullCountShader;
        SerializedProperty p_cullCountKernel;
        SerializedProperty p_finalizeShader;
        SerializedProperty p_finalizeKernel;
        SerializedProperty p_MinUpdateMovement;
        SerializedProperty p_MinUpdateTurn;
        SerializedProperty p_ShadowExtensionMult;
        SerializedProperty p_shadowDistanceSRP;
        SerializedProperty p_useNewShadowCalc;
        SerializedProperty p_AlwaysRasterizePolygons;
        SerializedProperty p_MaxTotalPolygonResolution;
        SerializedProperty p_DefaultPolygonResolution;
        SerializedProperty p_DefaultLODProgression;
        SerializedProperty p_DefaultDetailLODs;
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

        bool m_needsUpdateLOD = false;
        StormFeatureType LODUpdateType;
        bool m_needsUpdateGeneration = false;
        StormFeatureType GenerationUpdateType;
        bool m_needsFullReset = false;

        private void EnableAutoLoad()
        {
            PlayerPrefs.SetInt("STORM_AUTO_LOAD_CUSTOM", 1);
        }
        private void DisableAutoLoad()
        {
            PlayerPrefs.DeleteKey("STORM_AUTO_LOAD_CUSTOM");
        }
        private void SaveAutoSettings()
        {
            SaveAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.LOD_GLOBAL, p_globalLODBias.floatValue);
            SaveAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.LOD_GLOBAL_MAX_UI, p_globalLODBiasMaxUI.floatValue);
            SaveAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.LOD_GRASS, p_grassLODBias.floatValue);
            SaveAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.LOD_GRASS_MAX_UI, p_grassLODBiasMaxUI.floatValue);
            SaveAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.LOD_DETAIL, p_detailLODBias.floatValue);
            SaveAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.LOD_DETAIL_MAX_UI, p_detailLODBiasMaxUI.floatValue);
            SaveAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.LOD_PLANT, p_plantLODBias.floatValue);
            SaveAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.LOD_PLANT_MAX_UI, p_plantLODBiasMaxUI.floatValue);
            SaveAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.LOD_TREE, p_treeLODBias.floatValue);
            SaveAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.LOD_TREE_MAX_UI, p_treeLODBiasMaxUI.floatValue);
            SaveAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.LOD_SMALL_OBJECT, p_smallObjLODBias.floatValue);
            SaveAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.LOD_SMALL_OBJECT_MAX_UI, p_smallObjLODBiasMaxUI.floatValue);
            SaveAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.LOD_MEDIUM_OBJECT, p_mediumObjLODBias.floatValue);
            SaveAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.LOD_MEDIUM_OBJECT_MAX_UI, p_mediumObjLODBiasMaxUI.floatValue);
            SaveAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.LOD_LARGE_OBJECT, p_largeObjLODBias.floatValue);
            SaveAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.LOD_LARGE_OBJECT_MAX_UI, p_largeObjLODBiasMaxUI.floatValue);
            SaveAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.DENSITY_GRASS, p_grassDensity.floatValue);
            SaveAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.DENSITY_GRASS_MAX_UI, p_grassDensityMaxUI.floatValue);
            SaveAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.DENSITY_DETAIL, p_detailDensity.floatValue);
            SaveAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.DENSITY_DETAIL_MAX_UI, p_detailDensityMaxUI.floatValue);
            SaveAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.DENSITY_PLANT, p_plantDensity.floatValue);
            SaveAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.DENSITY_PLANT_MAX_UI, p_plantDensityMaxUI.floatValue);
            SaveAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.DENSITY_TREE, p_treeDensity.floatValue);
            SaveAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.DENSITY_TREE_MAX_UI, p_treeDensityMaxUI.floatValue);
            SaveAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.DENSITY_SMALL_OBJECT, p_smallObjDensity.floatValue);
            SaveAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.DENSITY_SMALL_OBJECT_MAX_UI, p_smallObjDensityMaxUI.floatValue);
            SaveAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.DENSITY_MEDIUM_OBJECT, p_mediumObjDensity.floatValue);
            SaveAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.DENSITY_MEDIUM_OBJECT_MAX_UI, p_mediumObjDensityMaxUI.floatValue);
            SaveAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.DENSITY_LARGE_OBJECT, p_largeObjDensity.floatValue);
            SaveAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.DENSITY_LARGE_OBJECT_MAX_UI, p_largeObjDensityMaxUI.floatValue);
            SaveAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.DISTANCE_GRASS, p_grassDistance.floatValue);
            SaveAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.DISTANCE_GRASS_MAX_UI, p_grassDistanceMaxUI.floatValue);
            SaveAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.DISTANCE_DETAIL, p_detailDistance.floatValue);
            SaveAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.DISTANCE_DETAIL_MAX_UI, p_detailDistanceMaxUI.floatValue);
            SaveAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.DISTANCE_PLANT, p_plantDistance.floatValue);
            SaveAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.DISTANCE_PLANT_MAX_UI, p_plantDistanceMaxUI.floatValue);
            SaveAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.DISTANCE_TREE, p_treeDistance.floatValue);
            SaveAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.DISTANCE_TREE_MAX_UI, p_treeDistanceMaxUI.floatValue);
            SaveAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.DISTANCE_SMALL_OBJECT, p_smallObjDistance.floatValue);
            SaveAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.DISTANCE_SMALL_OBJECT_MAX_UI, p_smallObjDistanceMaxUI.floatValue);
            SaveAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.DISTANCE_MEDIUM_OBJECT, p_mediumObjDistance.floatValue);
            SaveAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.DISTANCE_MEDIUM_OBJECT_MAX_UI, p_mediumObjDistanceMaxUI.floatValue);
            SaveAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.DISTANCE_LARGE_OBJECT, p_largeObjDistance.floatValue);
            SaveAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.DISTANCE_LARGE_OBJECT_MAX_UI, p_largeObjDistanceMaxUI.floatValue);
            SaveAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.SCALEOUT_GRASS, p_grassScaleoutMult.floatValue);
            SaveAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.SCALEOUT_GRASS_MAX_UI, p_grassScaleoutMultMaxUI.floatValue);
            SaveAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.SCALEOUT_DETAIL, p_detailScaleoutMult.floatValue);
            SaveAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.SCALEOUT_DETAIL_MAX_UI, p_detailScaleoutMultMaxUI.floatValue);
            SaveAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.SCALEOUT_PLANT, p_plantScaleoutMult.floatValue);
            SaveAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.SCALEOUT_PLANT_MAX_UI, p_plantScaleoutMultMaxUI.floatValue);
            SaveAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.SCALEOUT_TREE, p_treeScaleoutMult.floatValue);
            SaveAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.SCALEOUT_TREE_MAX_UI, p_treeScaleoutMultMaxUI.floatValue);
            SaveAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.SCALEOUT_SMALL_OBJECT, p_smallObjScaleoutMult.floatValue);
            SaveAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.SCALEOUT_SMALL_OBJECT_MAX_UI, p_smallObjScaleoutMultMaxUI.floatValue);
            SaveAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.SCALEOUT_MEDIUM_OBJECT, p_mediumObjScaleoutMult.floatValue);
            SaveAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.SCALEOUT_MEDIUM_OBJECT_MAX_UI, p_mediumObjScaleoutMultMaxUI.floatValue);
            SaveAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.SCALEOUT_LARGE_OBJECT, p_largeObjScaleoutMult.floatValue);
            SaveAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.SCALEOUT_LARGE_OBJECT_MAX_UI, p_largeObjScaleoutMultMaxUI.floatValue);
            SaveAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.LODMIX_GRASS, p_grassLODMixMult.floatValue);
            SaveAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.LODMIX_GRASS_MAX_UI, p_grassLODMixMultMaxUI.floatValue);
            SaveAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.LODMIX_DETAIL, p_detailLODMixMult.floatValue);
            SaveAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.LODMIX_DETAIL_MAX_UI, p_detailLODMixMultMaxUI.floatValue);
            SaveAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.LODMIX_PLANT, p_plantLODMixMult.floatValue);
            SaveAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.LODMIX_PLANT_MAX_UI, p_plantLODMixMultMaxUI.floatValue);
            SaveAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.LODMIX_TREE, p_treeLODMixMult.floatValue);
            SaveAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.LODMIX_TREE_MAX_UI, p_treeLODMixMultMaxUI.floatValue);
            SaveAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.LODMIX_SMALL_OBJECT, p_smallObjLODMixMult.floatValue);
            SaveAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.LODMIX_SMALL_OBJECT_MAX_UI, p_smallObjLODMixMultMaxUI.floatValue);
            SaveAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.LODMIX_MEDIUM_OBJECT, p_mediumObjLODMixMult.floatValue);
            SaveAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.LODMIX_MEDIUM_OBJECT_MAX_UI, p_mediumObjLODMixMultMaxUI.floatValue);
            SaveAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.LODMIX_LARGE_OBJECT, p_largeObjLODMixMult.floatValue);
            SaveAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.LODMIX_LARGE_OBJECT_MAX_UI, p_largeObjLODMixMultMaxUI.floatValue);
            SaveAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.DENSITYFADE_GRASS, p_grassDensityFadeMult.floatValue);
            SaveAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.DENSITYFADE_GRASS_MAX_UI, p_grassDensityFadeMultMaxUI.floatValue);
            SaveAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.DENSITYFADE_DETAIL, p_detailDensityFadeMult.floatValue);
            SaveAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.DENSITYFADE_DETAIL_MAX_UI, p_detailDensityFadeMultMaxUI.floatValue);
            SaveAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.DENSITYFADE_PLANT, p_plantDensityFadeMult.floatValue);
            SaveAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.DENSITYFADE_PLANT_MAX_UI, p_plantDensityFadeMultMaxUI.floatValue);
            SaveAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.DENSITYFADE_TREE, p_treeDensityFadeMult.floatValue);
            SaveAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.DENSITYFADE_TREE_MAX_UI, p_treeDensityFadeMultMaxUI.floatValue);
            SaveAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.DENSITYFADE_SMALL_OBJECT, p_smallObjDensityFadeMult.floatValue);
            SaveAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.DENSITYFADE_SMALL_OBJECT_MAX_UI, p_smallObjDensityFadeMultMaxUI.floatValue);
            SaveAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.DENSITYFADE_MEDIUM_OBJECT, p_mediumObjDensityFadeMult.floatValue);
            SaveAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.DENSITYFADE_MEDIUM_OBJECT_MAX_UI, p_mediumObjDensityFadeMultMaxUI.floatValue);
            SaveAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.DENSITYFADE_LARGE_OBJECT, p_largeObjDensityFadeMult.floatValue);
            SaveAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.DENSITYFADE_LARGE_OBJECT_MAX_UI, p_largeObjDensityFadeMultMaxUI.floatValue);
        }
        private void LoadAutoSettings()
        {
            p_globalLODBias.floatValue = GetAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.LOD_GLOBAL, p_globalLODBias.floatValue);
            p_globalLODBiasMaxUI.floatValue = GetAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.LOD_GLOBAL_MAX_UI, p_globalLODBiasMaxUI.floatValue);
            p_grassLODBias.floatValue = GetAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.LOD_GRASS, p_grassLODBias.floatValue);
            p_grassLODBiasMaxUI.floatValue = GetAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.LOD_GRASS_MAX_UI, p_grassLODBiasMaxUI.floatValue);
            p_detailLODBias.floatValue = GetAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.LOD_DETAIL, p_detailLODBias.floatValue);
            p_detailLODBiasMaxUI.floatValue = GetAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.LOD_DETAIL_MAX_UI, p_detailLODBiasMaxUI.floatValue);
            p_plantLODBias.floatValue = GetAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.LOD_PLANT, p_plantLODBias.floatValue);
            p_plantLODBiasMaxUI.floatValue = GetAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.LOD_PLANT_MAX_UI, p_plantLODBiasMaxUI.floatValue);
            p_treeLODBias.floatValue = GetAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.LOD_TREE, p_treeLODBias.floatValue);
            p_treeLODBiasMaxUI.floatValue = GetAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.LOD_TREE_MAX_UI, p_treeLODBiasMaxUI.floatValue);
            p_smallObjLODBias.floatValue = GetAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.LOD_SMALL_OBJECT, p_smallObjLODBias.floatValue);
            p_smallObjLODBiasMaxUI.floatValue = GetAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.LOD_SMALL_OBJECT_MAX_UI, p_smallObjLODBiasMaxUI.floatValue);
            p_mediumObjLODBias.floatValue = GetAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.LOD_MEDIUM_OBJECT, p_mediumObjLODBias.floatValue);
            p_mediumObjLODBiasMaxUI.floatValue = GetAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.LOD_MEDIUM_OBJECT_MAX_UI, p_mediumObjLODBiasMaxUI.floatValue);
            p_largeObjLODBias.floatValue = GetAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.LOD_LARGE_OBJECT, p_largeObjLODBias.floatValue);
            p_largeObjLODBiasMaxUI.floatValue = GetAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.LOD_LARGE_OBJECT_MAX_UI, p_largeObjLODBiasMaxUI.floatValue);
            p_grassDensity.floatValue = GetAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.DENSITY_GRASS, p_grassDensity.floatValue);
            p_grassDensityMaxUI.floatValue = GetAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.DENSITY_GRASS_MAX_UI, p_grassDensityMaxUI.floatValue);
            p_detailDensity.floatValue = GetAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.DENSITY_DETAIL, p_detailDensity.floatValue);
            p_detailDensityMaxUI.floatValue = GetAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.DENSITY_DETAIL_MAX_UI, p_detailDensityMaxUI.floatValue);
            p_plantDensity.floatValue = GetAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.DENSITY_PLANT, p_plantDensity.floatValue);
            p_plantDensityMaxUI.floatValue = GetAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.DENSITY_PLANT_MAX_UI, p_plantDensityMaxUI.floatValue);
            p_treeDensity.floatValue = GetAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.DENSITY_TREE, p_treeDensity.floatValue);
            p_treeDensityMaxUI.floatValue = GetAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.DENSITY_TREE_MAX_UI, p_treeDensityMaxUI.floatValue);
            p_smallObjDensity.floatValue = GetAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.DENSITY_SMALL_OBJECT, p_smallObjDensity.floatValue);
            p_smallObjDensityMaxUI.floatValue = GetAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.DENSITY_SMALL_OBJECT_MAX_UI, p_smallObjDensityMaxUI.floatValue);
            p_mediumObjDensity.floatValue = GetAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.DENSITY_MEDIUM_OBJECT, p_mediumObjDensity.floatValue);
            p_mediumObjDensityMaxUI.floatValue = GetAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.DENSITY_MEDIUM_OBJECT_MAX_UI, p_mediumObjDensityMaxUI.floatValue);
            p_largeObjDensity.floatValue = GetAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.DENSITY_LARGE_OBJECT, p_largeObjDensity.floatValue);
            p_largeObjDensityMaxUI.floatValue = GetAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.DENSITY_LARGE_OBJECT_MAX_UI, p_largeObjDensityMaxUI.floatValue);
            p_grassDistance.floatValue = GetAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.DISTANCE_GRASS, p_grassDistance.floatValue);
            p_grassDistanceMaxUI.floatValue = GetAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.DISTANCE_GRASS_MAX_UI, p_grassDistanceMaxUI.floatValue);
            p_detailDistance.floatValue = GetAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.DISTANCE_DETAIL, p_detailDistance.floatValue);
            p_detailDistanceMaxUI.floatValue = GetAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.DISTANCE_DETAIL_MAX_UI, p_detailDistanceMaxUI.floatValue);
            p_plantDistance.floatValue = GetAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.DISTANCE_PLANT, p_plantDistance.floatValue);
            p_plantDistanceMaxUI.floatValue = GetAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.DISTANCE_PLANT_MAX_UI, p_plantDistanceMaxUI.floatValue);
            p_treeDistance.floatValue = GetAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.DISTANCE_TREE, p_treeDistance.floatValue);
            p_treeDistanceMaxUI.floatValue = GetAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.DISTANCE_TREE_MAX_UI, p_treeDistanceMaxUI.floatValue);
            p_smallObjDistance.floatValue = GetAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.DISTANCE_SMALL_OBJECT, p_smallObjDistance.floatValue);
            p_smallObjDistanceMaxUI.floatValue = GetAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.DISTANCE_SMALL_OBJECT_MAX_UI, p_smallObjDistanceMaxUI.floatValue);
            p_mediumObjDistance.floatValue = GetAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.DISTANCE_MEDIUM_OBJECT, p_mediumObjDistance.floatValue);
            p_mediumObjDistanceMaxUI.floatValue = GetAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.DISTANCE_MEDIUM_OBJECT_MAX_UI, p_mediumObjDistanceMaxUI.floatValue);
            p_largeObjDistance.floatValue = GetAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.DISTANCE_LARGE_OBJECT, p_largeObjDistance.floatValue);
            p_largeObjDistanceMaxUI.floatValue = GetAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.DISTANCE_LARGE_OBJECT_MAX_UI, p_largeObjDistanceMaxUI.floatValue);
            p_grassScaleoutMult.floatValue = GetAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.SCALEOUT_GRASS, p_grassScaleoutMult.floatValue);
            p_grassScaleoutMultMaxUI.floatValue = GetAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.SCALEOUT_GRASS_MAX_UI, p_grassScaleoutMultMaxUI.floatValue);
            p_detailScaleoutMult.floatValue = GetAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.SCALEOUT_DETAIL, p_detailScaleoutMult.floatValue);
            p_detailScaleoutMultMaxUI.floatValue = GetAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.SCALEOUT_DETAIL_MAX_UI, p_detailScaleoutMultMaxUI.floatValue);
            p_plantScaleoutMult.floatValue = GetAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.SCALEOUT_PLANT, p_plantScaleoutMult.floatValue);
            p_plantScaleoutMultMaxUI.floatValue = GetAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.SCALEOUT_PLANT_MAX_UI, p_plantScaleoutMultMaxUI.floatValue);
            p_treeScaleoutMult.floatValue = GetAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.SCALEOUT_TREE, p_treeScaleoutMult.floatValue);
            p_treeScaleoutMultMaxUI.floatValue = GetAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.SCALEOUT_TREE_MAX_UI, p_treeScaleoutMultMaxUI.floatValue);
            p_smallObjScaleoutMult.floatValue = GetAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.SCALEOUT_SMALL_OBJECT, p_smallObjScaleoutMult.floatValue);
            p_smallObjScaleoutMultMaxUI.floatValue = GetAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.SCALEOUT_SMALL_OBJECT_MAX_UI, p_smallObjScaleoutMultMaxUI.floatValue);
            p_mediumObjScaleoutMult.floatValue = GetAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.SCALEOUT_MEDIUM_OBJECT, p_mediumObjScaleoutMult.floatValue);
            p_mediumObjScaleoutMultMaxUI.floatValue = GetAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.SCALEOUT_MEDIUM_OBJECT_MAX_UI, p_mediumObjScaleoutMultMaxUI.floatValue);
            p_largeObjScaleoutMult.floatValue = GetAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.SCALEOUT_LARGE_OBJECT, p_largeObjScaleoutMult.floatValue);
            p_largeObjScaleoutMultMaxUI.floatValue = GetAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.SCALEOUT_LARGE_OBJECT_MAX_UI, p_largeObjScaleoutMultMaxUI.floatValue);
            p_grassLODMixMult.floatValue = GetAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.LODMIX_GRASS, p_grassLODMixMult.floatValue);
            p_grassLODMixMultMaxUI.floatValue = GetAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.LODMIX_GRASS_MAX_UI, p_grassLODMixMultMaxUI.floatValue);
            p_detailLODMixMult.floatValue = GetAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.LODMIX_DETAIL, p_detailLODMixMult.floatValue);
            p_detailLODMixMultMaxUI.floatValue = GetAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.LODMIX_DETAIL_MAX_UI, p_detailLODMixMultMaxUI.floatValue);
            p_plantLODMixMult.floatValue = GetAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.LODMIX_PLANT, p_plantLODMixMult.floatValue);
            p_plantLODMixMultMaxUI.floatValue = GetAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.LODMIX_PLANT_MAX_UI, p_plantLODMixMultMaxUI.floatValue);
            p_treeLODMixMult.floatValue = GetAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.LODMIX_TREE, p_treeLODMixMult.floatValue);
            p_treeLODMixMultMaxUI.floatValue = GetAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.LODMIX_TREE_MAX_UI, p_treeLODMixMultMaxUI.floatValue);
            p_smallObjLODMixMult.floatValue = GetAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.LODMIX_SMALL_OBJECT, p_smallObjLODMixMult.floatValue);
            p_smallObjLODMixMultMaxUI.floatValue = GetAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.LODMIX_SMALL_OBJECT_MAX_UI, p_smallObjLODMixMultMaxUI.floatValue);
            p_mediumObjLODMixMult.floatValue = GetAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.LODMIX_MEDIUM_OBJECT, p_mediumObjLODMixMult.floatValue);
            p_mediumObjLODMixMultMaxUI.floatValue = GetAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.LODMIX_MEDIUM_OBJECT_MAX_UI, p_mediumObjLODMixMultMaxUI.floatValue);
            p_largeObjLODMixMult.floatValue = GetAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.LODMIX_LARGE_OBJECT, p_largeObjLODMixMult.floatValue);
            p_largeObjLODMixMultMaxUI.floatValue = GetAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.LODMIX_LARGE_OBJECT_MAX_UI, p_largeObjLODMixMultMaxUI.floatValue);
            p_grassDensityFadeMult.floatValue = GetAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.DENSITYFADE_GRASS, p_grassDensityFadeMult.floatValue);
            p_grassDensityFadeMultMaxUI.floatValue = GetAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.DENSITYFADE_GRASS_MAX_UI, p_grassDensityFadeMultMaxUI.floatValue);
            p_detailDensityFadeMult.floatValue = GetAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.DENSITYFADE_DETAIL, p_detailDensityFadeMult.floatValue);
            p_detailDensityFadeMultMaxUI.floatValue = GetAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.DENSITYFADE_DETAIL_MAX_UI, p_detailDensityFadeMultMaxUI.floatValue);
            p_plantDensityFadeMult.floatValue = GetAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.DENSITYFADE_PLANT, p_plantDensityFadeMult.floatValue);
            p_plantDensityFadeMultMaxUI.floatValue = GetAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.DENSITYFADE_PLANT_MAX_UI, p_plantDensityFadeMultMaxUI.floatValue);
            p_treeDensityFadeMult.floatValue = GetAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.DENSITYFADE_TREE, p_treeDensityFadeMult.floatValue);
            p_treeDensityFadeMultMaxUI.floatValue = GetAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.DENSITYFADE_TREE_MAX_UI, p_treeDensityFadeMultMaxUI.floatValue);
            p_smallObjDensityFadeMult.floatValue = GetAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.DENSITYFADE_SMALL_OBJECT, p_smallObjDensityFadeMult.floatValue);
            p_smallObjDensityFadeMultMaxUI.floatValue = GetAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.DENSITYFADE_SMALL_OBJECT_MAX_UI, p_smallObjDensityFadeMultMaxUI.floatValue);
            p_mediumObjDensityFadeMult.floatValue = GetAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.DENSITYFADE_MEDIUM_OBJECT, p_mediumObjDensityFadeMult.floatValue);
            p_mediumObjDensityFadeMultMaxUI.floatValue = GetAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.DENSITYFADE_MEDIUM_OBJECT_MAX_UI, p_mediumObjDensityFadeMultMaxUI.floatValue);
            p_largeObjDensityFadeMult.floatValue = GetAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.DENSITYFADE_LARGE_OBJECT, p_largeObjDensityFadeMult.floatValue);
            p_largeObjDensityFadeMultMaxUI.floatValue = GetAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.DENSITYFADE_LARGE_OBJECT_MAX_UI, p_largeObjDensityFadeMultMaxUI.floatValue);
        }
        private void ClearAutoSettings()
        {
            ClearAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.LOD_GLOBAL);
            ClearAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.LOD_GLOBAL_MAX_UI);
            ClearAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.LOD_GRASS);
            ClearAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.LOD_GRASS_MAX_UI);
            ClearAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.LOD_DETAIL);
            ClearAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.LOD_DETAIL_MAX_UI);
            ClearAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.LOD_PLANT);
            ClearAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.LOD_PLANT_MAX_UI);
            ClearAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.LOD_TREE);
            ClearAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.LOD_TREE_MAX_UI);
            ClearAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.LOD_SMALL_OBJECT);
            ClearAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.LOD_SMALL_OBJECT_MAX_UI);
            ClearAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.LOD_MEDIUM_OBJECT);
            ClearAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.LOD_MEDIUM_OBJECT_MAX_UI);
            ClearAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.LOD_LARGE_OBJECT);
            ClearAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.LOD_LARGE_OBJECT_MAX_UI);
            ClearAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.DENSITY_GRASS);
            ClearAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.DENSITY_GRASS_MAX_UI);
            ClearAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.DENSITY_DETAIL);
            ClearAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.DENSITY_DETAIL_MAX_UI);
            ClearAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.DENSITY_PLANT);
            ClearAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.DENSITY_PLANT_MAX_UI);
            ClearAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.DENSITY_TREE);
            ClearAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.DENSITY_TREE_MAX_UI);
            ClearAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.DENSITY_SMALL_OBJECT);
            ClearAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.DENSITY_SMALL_OBJECT_MAX_UI);
            ClearAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.DENSITY_MEDIUM_OBJECT);
            ClearAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.DENSITY_MEDIUM_OBJECT_MAX_UI);
            ClearAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.DENSITY_LARGE_OBJECT);
            ClearAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.DENSITY_LARGE_OBJECT_MAX_UI);
            ClearAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.DISTANCE_GRASS);
            ClearAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.DISTANCE_GRASS_MAX_UI);
            ClearAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.DISTANCE_DETAIL);
            ClearAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.DISTANCE_DETAIL_MAX_UI);
            ClearAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.DISTANCE_PLANT);
            ClearAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.DISTANCE_PLANT_MAX_UI);
            ClearAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.DISTANCE_TREE);
            ClearAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.DISTANCE_TREE_MAX_UI);
            ClearAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.DISTANCE_SMALL_OBJECT);
            ClearAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.DISTANCE_SMALL_OBJECT_MAX_UI);
            ClearAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.DISTANCE_MEDIUM_OBJECT);
            ClearAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.DISTANCE_MEDIUM_OBJECT_MAX_UI);
            ClearAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.DISTANCE_LARGE_OBJECT);
            ClearAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.DISTANCE_LARGE_OBJECT_MAX_UI);
            ClearAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.SCALEOUT_GRASS);
            ClearAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.SCALEOUT_GRASS_MAX_UI);
            ClearAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.SCALEOUT_DETAIL);
            ClearAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.SCALEOUT_DETAIL_MAX_UI);
            ClearAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.SCALEOUT_PLANT);
            ClearAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.SCALEOUT_PLANT_MAX_UI);
            ClearAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.SCALEOUT_TREE);
            ClearAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.SCALEOUT_TREE_MAX_UI);
            ClearAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.SCALEOUT_SMALL_OBJECT);
            ClearAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.SCALEOUT_SMALL_OBJECT_MAX_UI);
            ClearAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.SCALEOUT_MEDIUM_OBJECT);
            ClearAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.SCALEOUT_MEDIUM_OBJECT_MAX_UI);
            ClearAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.SCALEOUT_LARGE_OBJECT);
            ClearAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.SCALEOUT_LARGE_OBJECT_MAX_UI);
            ClearAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.LODMIX_GRASS);
            ClearAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.LODMIX_GRASS_MAX_UI);
            ClearAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.LODMIX_DETAIL);
            ClearAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.LODMIX_DETAIL_MAX_UI);
            ClearAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.LODMIX_PLANT);
            ClearAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.LODMIX_PLANT_MAX_UI);
            ClearAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.LODMIX_TREE);
            ClearAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.LODMIX_TREE_MAX_UI);
            ClearAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.LODMIX_SMALL_OBJECT);
            ClearAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.LODMIX_SMALL_OBJECT_MAX_UI);
            ClearAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.LODMIX_MEDIUM_OBJECT);
            ClearAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.LODMIX_MEDIUM_OBJECT_MAX_UI);
            ClearAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.LODMIX_LARGE_OBJECT);
            ClearAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.LODMIX_LARGE_OBJECT_MAX_UI);
            ClearAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.DENSITYFADE_GRASS);
            ClearAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.DENSITYFADE_GRASS_MAX_UI);
            ClearAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.DENSITYFADE_DETAIL);
            ClearAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.DENSITYFADE_DETAIL_MAX_UI);
            ClearAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.DENSITYFADE_PLANT);
            ClearAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.DENSITYFADE_PLANT_MAX_UI);
            ClearAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.DENSITYFADE_TREE);
            ClearAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.DENSITYFADE_TREE_MAX_UI);
            ClearAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.DENSITYFADE_SMALL_OBJECT);
            ClearAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.DENSITYFADE_SMALL_OBJECT_MAX_UI);
            ClearAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.DENSITYFADE_MEDIUM_OBJECT);
            ClearAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.DENSITYFADE_MEDIUM_OBJECT_MAX_UI);
            ClearAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.DENSITYFADE_LARGE_OBJECT);
            ClearAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.DENSITYFADE_LARGE_OBJECT_MAX_UI);
        }
        private void SaveAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting setting, float val)
        {
            PlayerPrefs.SetFloat("STORM_" + setting.ToString(), val);
            PlayerPrefs.Save();
        }
        private float GetAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting setting, float defaultVal)
        {
            return PlayerPrefs.GetFloat("STORM_" + setting.ToString(), defaultVal);
        }
        private void ClearAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting setting)
        {
            PlayerPrefs.DeleteKey("STORM_" + setting.ToString());
            PlayerPrefs.Save();
        }


        private void OnEnable()
        {
            _ = ((StormWorldSettings)target).ImpostorBakeShaders;
            _ = ((StormWorldSettings)target).ImpostorPostProcess;
            _ = ((StormWorldSettings)target).ImpostorFlatReplacement; //call the get functions to check for empty
            m_needsUpdateLOD = false;
            m_needsUpdateGeneration = false;
            m_needsFullReset = false;
            p_globalLODBias = serializedObject.FindProperty("m_globalLODBias");
            p_grassLODBias = serializedObject.FindProperty("m_grassLODBias");
            p_detailLODBias = serializedObject.FindProperty("m_detailLODBias");
            p_plantLODBias = serializedObject.FindProperty("m_plantLODBias");
            p_treeLODBias = serializedObject.FindProperty("m_treeLODBias");
            p_smallObjLODBias = serializedObject.FindProperty("m_smallObjLODBias");
            p_mediumObjLODBias = serializedObject.FindProperty("m_mediumObjLODBias");
            p_largeObjLODBias = serializedObject.FindProperty("m_largeObjLODBias");
            p_smallObjDensity = serializedObject.FindProperty("m_smallObjDensity");
            p_detailDensity = serializedObject.FindProperty("m_detailDensity");
            p_plantDensity = serializedObject.FindProperty("m_plantDensity");
            p_treeDensity = serializedObject.FindProperty("m_treeDensity");
            p_grassDensity = serializedObject.FindProperty("m_grassDensity");
            p_mediumObjDensity = serializedObject.FindProperty("m_mediumObjDensity");
            p_largeObjDensity = serializedObject.FindProperty("m_largeObjDensity");
            p_smallObjDistance = serializedObject.FindProperty("m_smallObjDistance");
            p_detailDistance = serializedObject.FindProperty("m_detailDistance");
            p_plantDistance = serializedObject.FindProperty("m_plantDistance");
            p_treeDistance = serializedObject.FindProperty("m_treeDistance");
            p_grassDistance = serializedObject.FindProperty("m_grassDistance");
            p_mediumObjDistance = serializedObject.FindProperty("m_mediumObjDistance");
            p_largeObjDistance = serializedObject.FindProperty("m_largeObjDistance");
            p_colliderDistance = serializedObject.FindProperty("m_colliderDistance");
            p_smallObjScaleoutMult = serializedObject.FindProperty("m_smallObjScaleoutMult");
            p_detailScaleoutMult = serializedObject.FindProperty("m_detailScaleoutMult");
            p_plantScaleoutMult = serializedObject.FindProperty("m_plantScaleoutMult");
            p_treeScaleoutMult = serializedObject.FindProperty("m_treeScaleoutMult");
            p_grassScaleoutMult = serializedObject.FindProperty("m_grassScaleoutMult");
            p_mediumObjScaleoutMult = serializedObject.FindProperty("m_mediumObjScaleoutMult");
            p_largeObjScaleoutMult = serializedObject.FindProperty("m_largeObjScaleoutMult");
            p_grassLODMixMult = serializedObject.FindProperty("m_grassLODMixMult");
            p_detailLODMixMult = serializedObject.FindProperty("m_detailLODMixMult");
            p_plantLODMixMult = serializedObject.FindProperty("m_plantLODMixMult");
            p_treeLODMixMult = serializedObject.FindProperty("m_treeLODMixMult");
            p_smallObjLODMixMult = serializedObject.FindProperty("m_smallObjLODMixMult");
            p_mediumObjLODMixMult = serializedObject.FindProperty("m_mediumObjLODMixMult");
            p_largeObjLODMixMult = serializedObject.FindProperty("m_largeObjLODMixMult");
            p_grassDensityFadeMult = serializedObject.FindProperty("m_grassDensityFadeMult");
            p_detailDensityFadeMult = serializedObject.FindProperty("m_detailDensityFadeMult");
            p_plantDensityFadeMult = serializedObject.FindProperty("m_plantDensityFadeMult");
            p_treeDensityFadeMult = serializedObject.FindProperty("m_treeDensityFadeMult");
            p_smallObjDensityFadeMult = serializedObject.FindProperty("m_smallObjDensityFadeMult");
            p_mediumObjDensityFadeMult = serializedObject.FindProperty("m_mediumObjDensityFadeMult");
            p_largeObjDensityFadeMult = serializedObject.FindProperty("m_largeObjDensityFadeMult");
            p_globalLODBiasMaxUI = serializedObject.FindProperty("m_globalLODBiasMaxUI");
            p_grassLODBiasMaxUI = serializedObject.FindProperty("m_grassLODBiasMaxUI");
            p_detailLODBiasMaxUI = serializedObject.FindProperty("m_detailLODBiasMaxUI");
            p_plantLODBiasMaxUI = serializedObject.FindProperty("m_plantLODBiasMaxUI");
            p_treeLODBiasMaxUI = serializedObject.FindProperty("m_treeLODBiasMaxUI");
            p_smallObjLODBiasMaxUI = serializedObject.FindProperty("m_smallObjLODBiasMaxUI");
            p_mediumObjLODBiasMaxUI = serializedObject.FindProperty("m_mediumObjLODBiasMaxUI");
            p_largeObjLODBiasMaxUI = serializedObject.FindProperty("m_largeObjLODBiasMaxUI");
            p_smallObjDensityMaxUI = serializedObject.FindProperty("m_smallObjDensityMaxUI");
            p_detailDensityMaxUI = serializedObject.FindProperty("m_detailDensityMaxUI");
            p_plantDensityMaxUI = serializedObject.FindProperty("m_plantDensityMaxUI");
            p_treeDensityMaxUI = serializedObject.FindProperty("m_treeDensityMaxUI");
            p_grassDensityMaxUI = serializedObject.FindProperty("m_grassDensityMaxUI");
            p_mediumObjDensityMaxUI = serializedObject.FindProperty("m_mediumObjDensityMaxUI");
            p_largeObjDensityMaxUI = serializedObject.FindProperty("m_largeObjDensityMaxUI");
            p_smallObjDistanceMaxUI = serializedObject.FindProperty("m_smallObjDistanceMaxUI");
            p_detailDistanceMaxUI = serializedObject.FindProperty("m_detailDistanceMaxUI");
            p_plantDistanceMaxUI = serializedObject.FindProperty("m_plantDistanceMaxUI");
            p_treeDistanceMaxUI = serializedObject.FindProperty("m_treeDistanceMaxUI");
            p_grassDistanceMaxUI = serializedObject.FindProperty("m_grassDistanceMaxUI");
            p_mediumObjDistanceMaxUI = serializedObject.FindProperty("m_mediumObjDistanceMaxUI");
            p_largeObjDistanceMaxUI = serializedObject.FindProperty("m_largeObjDistanceMaxUI");
            p_colliderDistanceMaxUI = serializedObject.FindProperty("m_colliderDistanceMaxUI");
            p_smallObjScaleoutMultMaxUI = serializedObject.FindProperty("m_smallObjScaleoutMultMaxUI");
            p_detailScaleoutMultMaxUI = serializedObject.FindProperty("m_detailScaleoutMultMaxUI");
            p_plantScaleoutMultMaxUI = serializedObject.FindProperty("m_plantScaleoutMultMaxUI");
            p_treeScaleoutMultMaxUI = serializedObject.FindProperty("m_treeScaleoutMultMaxUI");
            p_grassScaleoutMultMaxUI = serializedObject.FindProperty("m_grassScaleoutMultMaxUI");
            p_mediumObjScaleoutMultMaxUI = serializedObject.FindProperty("m_mediumObjScaleoutMultMaxUI");
            p_largeObjScaleoutMultMaxUI = serializedObject.FindProperty("m_largeObjScaleoutMultMaxUI");
            p_grassLODMixMultMaxUI = serializedObject.FindProperty("m_grassLODMixMultMaxUI");
            p_detailLODMixMultMaxUI = serializedObject.FindProperty("m_detailLODMixMultMaxUI");
            p_plantLODMixMultMaxUI = serializedObject.FindProperty("m_plantLODMixMultMaxUI");
            p_treeLODMixMultMaxUI = serializedObject.FindProperty("m_treeLODMixMultMaxUI");
            p_smallObjLODMixMultMaxUI = serializedObject.FindProperty("m_smallObjLODMixMultMaxUI");
            p_mediumObjLODMixMultMaxUI = serializedObject.FindProperty("m_mediumObjLODMixMultMaxUI");
            p_largeObjLODMixMultMaxUI = serializedObject.FindProperty("m_largeObjLODMixMultMaxUI");
            p_grassDensityFadeMultMaxUI = serializedObject.FindProperty("m_grassDensityFadeMultMaxUI");
            p_detailDensityFadeMultMaxUI = serializedObject.FindProperty("m_detailDensityFadeMultMaxUI");
            p_plantDensityFadeMultMaxUI = serializedObject.FindProperty("m_plantDensityFadeMultMaxUI");
            p_treeDensityFadeMultMaxUI = serializedObject.FindProperty("m_treeDensityFadeMultMaxUI");
            p_smallObjDensityFadeMultMaxUI = serializedObject.FindProperty("m_smallObjDensityFadeMultMaxUI");
            p_mediumObjDensityFadeMultMaxUI = serializedObject.FindProperty("m_mediumObjDensityFadeMultMaxUI");
            p_largeObjDensityFadeMultMaxUI = serializedObject.FindProperty("m_largeObjDensityFadeMultMaxUI");
            p_loadDistance = serializedObject.FindProperty("m_loadDistance");
            p_unloadDistance = serializedObject.FindProperty("m_unloadDistance");
            p_maxSimultaneousLoad = serializedObject.FindProperty("m_maxSimultaneousLoad");
            p_maxLoadMasksPerFrame = serializedObject.FindProperty("m_maxLoadMasksPerFrame");
            p_maxInstancesPerFrame = serializedObject.FindProperty("m_maxInstancesPerFrame");
            p_instantFirstLoad = serializedObject.FindProperty("m_instantFirstLoad");
            p_renderCellSize = serializedObject.FindProperty("m_renderCellSize");
            p_curveResolution = serializedObject.FindProperty("m_curveResolution");
            p_heightResolution = serializedObject.FindProperty("m_heightResolution");
            p_useJobs = serializedObject.FindProperty("m_useJobs");
            p_allowFrameDelay = serializedObject.FindProperty("m_allowFrameDelay");
            p_cullingTurnOffset = serializedObject.FindProperty("m_cullTurnOffset");
            p_shadowDistanceMultiplier = serializedObject.FindProperty("m_shadowDistanceMultiplier");
            p_splitShadowRendering = serializedObject.FindProperty("m_splitShadowRendering");
            p_renderType = serializedObject.FindProperty("m_renderType");
            p_cullType = serializedObject.FindProperty("m_cullType");
            p_cullShader = serializedObject.FindProperty("m_cullShader");
            p_cullKernelStandard = serializedObject.FindProperty("m_cullKernelStandard");
            p_cullKernelSimple = serializedObject.FindProperty("m_cullKernelSimple");
            p_cullKernelShadowOnly = serializedObject.FindProperty("m_cullKernelShadowOnly");
            p_cullCountShader = serializedObject.FindProperty("m_cullCountShader");
            p_cullCountKernel = serializedObject.FindProperty("m_cullCountKernel");
            p_finalizeShader = serializedObject.FindProperty("m_finalizeShader");
            p_finalizeKernel = serializedObject.FindProperty("m_finalizeKernel");
            p_MinUpdateMovement = serializedObject.FindProperty("m_minUpdateMovement");
            p_MinUpdateTurn = serializedObject.FindProperty("m_minUpdateTurn");
            p_ShadowExtensionMult = serializedObject.FindProperty("m_shadowExtensionMult");
            p_shadowDistanceSRP = serializedObject.FindProperty("m_shadowDistanceSRP");
            p_useNewShadowCalc = serializedObject.FindProperty("m_useNewShadowCalc");
            p_AlwaysRasterizePolygons = serializedObject.FindProperty("AlwaysRasterizePolygons");
            p_MaxTotalPolygonResolution = serializedObject.FindProperty("MaxTotalPolygonResolution");
            p_DefaultPolygonResolution = serializedObject.FindProperty("DefaultPolygonResolution");
            p_DefaultLODProgression = serializedObject.FindProperty("DefaultLODProgression");
            p_DefaultDetailLODs = serializedObject.FindProperty("DefaultDetailLODs");
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
                m_worldSettings = (StormWorldSettings)target;
            }

            if (m_editorUtils == null)
                m_editorUtils = StormApp.GetEditorUtils(this);
            m_editorUtils.Initialize();
            if (GUILayout.Button("Split to Quality and System"))
            {
                StormQualitySettings qSettings = StormUtils.FindAsset<StormQualitySettings>(m_worldSettings.name + " t:StormQualitySettings", "Assets");
                if (qSettings != null)
                {
                    qSettings.CopyFrom(m_worldSettings);
                }
                else
                {
                    string path = StormUtils.GetAssetPath(m_worldSettings);
                    path = System.IO.Path.ChangeExtension(path, "_Quality.asset").Replace("._", "_");
                    qSettings = ScriptableObject.CreateInstance<StormQualitySettings>();
                    qSettings.CopyFrom(m_worldSettings);
                    StormUtils.SaveAsset(qSettings, path);
                }
                StormSystemSettings sSettings = StormUtils.FindAsset<StormSystemSettings>(m_worldSettings.name + " t:StormSystemSettings", "Assets");
                if (sSettings != null)
                {
                    sSettings.CopyFrom(m_worldSettings);
                }
                else
                {
                    string path = StormUtils.GetAssetPath(m_worldSettings);
                    path = System.IO.Path.ChangeExtension(path, "_System.asset").Replace("._", "_");
                    sSettings = ScriptableObject.CreateInstance<StormSystemSettings>();
                    sSettings.CopyFrom(m_worldSettings);
                    StormUtils.SaveAsset(sSettings, path);
                }
            }
            EditorGUILayout.Space();
            m_editorUtils.ToggleButton("ExpertMode", ref m_expertMode);
            m_editorUtils.Panel("UnityPlayerSettings", UnityPlayerSettings);
            m_editorUtils.Panel("DrawDistancePanel", DrawDistancePanel);
            m_editorUtils.Panel("DensityPanel", DensityPanel);
            m_editorUtils.Panel("DensityFadePanel", DensityFadePanel);
            m_editorUtils.Panel("LODBiasPanel", LODBiasPanel);
            m_editorUtils.Panel("LODMixPanel", LODMixPanel);
            m_editorUtils.Panel("ScaleoutPanel", ScaleoutPanel);
            m_editorUtils.Panel("LoadingPanel", LoadingPanel);
            /*
            m_editorUtils.Panel("TargetFPSPanel", TargetFPSPanel);
            //*/
            m_editorUtils.Panel("InternalDataPanel", InternalDataPanel);
            m_editorUtils.Panel("PrefabCreationPanel", PrefabCreationPanel);
            m_editorUtils.Panel("ImpostorPanel", ImpostorPanel);
            serializedObject.ApplyModifiedProperties();

            if (m_needsUpdateLOD)
            {
                StormAPI.OnLODSettingsChanged?.Invoke(LODUpdateType);
                m_needsUpdateLOD = false;
                LODUpdateType = 0;
            }
            if (m_needsUpdateGeneration)
            {
                StormAPI.OnGenerationSettingsChanged?.Invoke(GenerationUpdateType);
                m_needsUpdateGeneration = false;
                GenerationUpdateType = 0;
            }
            if (m_needsFullReset)
            {
                m_needsFullReset = false;
                if (StormWorld.Instance != null)
                    StormWorld.Instance.ForceFullRefresh();
            }
        }
        private void UnityPlayerSettings(bool showHelp)
        {
            bool autoLoad = PlayerPrefs.GetInt("STORM_AUTO_LOAD_CUSTOM", 0) == 1;
            bool newAutoLoad = m_editorUtils.Toggle("AutoLoadSettings", autoLoad, showHelp);
            if (newAutoLoad != autoLoad)
            {
                if (newAutoLoad)
                    EnableAutoLoad();
                else
                    DisableAutoLoad();
            }
            if (autoLoad)
            {
                EditorGUILayout.BeginHorizontal();
                if (m_editorUtils.Button("AutoLoadSettingsSaveButton"))
                    SaveAutoSettings();
                EditorGUI.BeginDisabledGroup(!autoLoad);
                if (m_editorUtils.Button("AutoLoadSettingsLoadButton"))
                    LoadAutoSettings();
                if (m_editorUtils.Button("AutoLoadSettingsClearButton"))
                    ClearAutoSettings();
                EditorGUI.EndDisabledGroup();
                EditorGUILayout.EndHorizontal();
                m_editorUtils.Label("AutoLoadCurrentSettings");
                EditorGUILayout.LabelField("LOD_GLOBAL", GetAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.LOD_GLOBAL, 0) + "\t Max:" + GetAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.LOD_GLOBAL_MAX_UI, 0));
                EditorGUILayout.LabelField("LOD_GRASS", GetAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.LOD_GRASS, 0) + "\t Max:" + GetAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.LOD_GRASS_MAX_UI, 0));
                EditorGUILayout.LabelField("LOD_DETAIL", GetAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.LOD_DETAIL, 0) + "\t Max:" + GetAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.LOD_DETAIL_MAX_UI, 0));
                EditorGUILayout.LabelField("LOD_PLANT", GetAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.LOD_PLANT, 0) + "\t Max:" + GetAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.LOD_PLANT_MAX_UI, 0));
                EditorGUILayout.LabelField("LOD_TREE", GetAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.LOD_TREE, 0) + "\t Max:" + GetAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.LOD_TREE_MAX_UI, 0));
                EditorGUILayout.LabelField("LOD_SMALL", GetAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.LOD_SMALL_OBJECT, 0) + "\t Max:" + GetAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.LOD_SMALL_OBJECT_MAX_UI, 0));
                EditorGUILayout.LabelField("LOD_MEDIUM", GetAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.LOD_MEDIUM_OBJECT, 0) + "\t Max:" + GetAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.LOD_MEDIUM_OBJECT_MAX_UI, 0));
                EditorGUILayout.LabelField("LOD_LARGE", GetAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.LOD_LARGE_OBJECT, 0) + "\t Max:" + GetAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.LOD_LARGE_OBJECT_MAX_UI, 0));
                EditorGUILayout.LabelField("DENSITY_GRASS", GetAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.DENSITY_GRASS, 0) + "\t Max:" + GetAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.DENSITY_GRASS_MAX_UI, 0));
                EditorGUILayout.LabelField("DENSITY_DETAIL", GetAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.DENSITY_DETAIL, 0) + "\t Max:" + GetAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.DENSITY_DETAIL_MAX_UI, 0));
                EditorGUILayout.LabelField("DENSITY_PLANT", GetAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.DENSITY_PLANT, 0) + "\t Max:" + GetAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.DENSITY_PLANT_MAX_UI, 0));
                EditorGUILayout.LabelField("DENSITY_TREE", GetAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.DENSITY_TREE, 0) + "\t Max:" + GetAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.DENSITY_TREE_MAX_UI, 0));
                EditorGUILayout.LabelField("DENSITY_SMALL", GetAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.DENSITY_SMALL_OBJECT, 0) + "\t Max:" + GetAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.DENSITY_SMALL_OBJECT_MAX_UI, 0));
                EditorGUILayout.LabelField("DENSITY_MEDIUM", GetAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.DENSITY_MEDIUM_OBJECT, 0) + "\t Max:" + GetAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.DENSITY_MEDIUM_OBJECT_MAX_UI, 0));
                EditorGUILayout.LabelField("DENSITY_LARGE", GetAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.DENSITY_LARGE_OBJECT, 0) + "\t Max:" + GetAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.DENSITY_LARGE_OBJECT_MAX_UI, 0));
                EditorGUILayout.LabelField("DISTANCE_GRASS", GetAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.DISTANCE_GRASS, 0) + "\t Max:" + GetAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.DISTANCE_GRASS_MAX_UI, 0));
                EditorGUILayout.LabelField("DISTANCE_DETAIL", GetAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.DISTANCE_DETAIL, 0) + "\t Max:" + GetAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.DISTANCE_DETAIL_MAX_UI, 0));
                EditorGUILayout.LabelField("DISTANCE_PLANT", GetAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.DISTANCE_PLANT, 0) + "\t Max:" + GetAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.DISTANCE_PLANT_MAX_UI, 0));
                EditorGUILayout.LabelField("DISTANCE_TREE", GetAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.DISTANCE_TREE, 0) + "\t Max:" + GetAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.DISTANCE_TREE_MAX_UI, 0));
                EditorGUILayout.LabelField("DISTANCE_SMALL", GetAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.DISTANCE_SMALL_OBJECT, 0) + "\t Max:" + GetAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.DISTANCE_SMALL_OBJECT_MAX_UI, 0));
                EditorGUILayout.LabelField("DISTANCE_MEDIUM", GetAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.DISTANCE_MEDIUM_OBJECT, 0) + "\t Max:" + GetAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.DISTANCE_MEDIUM_OBJECT_MAX_UI, 0));
                EditorGUILayout.LabelField("DISTANCE_LARGE", GetAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.DISTANCE_LARGE_OBJECT, 0) + "\t Max:" + GetAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.DISTANCE_LARGE_OBJECT_MAX_UI, 0));
                EditorGUILayout.LabelField("SCALEOUT_GRASS", GetAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.SCALEOUT_GRASS, 0) + "\t Max:" + GetAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.SCALEOUT_GRASS_MAX_UI, 0));
                EditorGUILayout.LabelField("SCALEOUT_DETAIL", GetAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.SCALEOUT_DETAIL, 0) + "\t Max:" + GetAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.SCALEOUT_DETAIL_MAX_UI, 0));
                EditorGUILayout.LabelField("SCALEOUT_PLANT", GetAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.SCALEOUT_PLANT, 0) + "\t Max:" + GetAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.SCALEOUT_PLANT_MAX_UI, 0));
                EditorGUILayout.LabelField("SCALEOUT_TREE", GetAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.SCALEOUT_TREE, 0) + "\t Max:" + GetAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.SCALEOUT_TREE_MAX_UI, 0));
                EditorGUILayout.LabelField("SCALEOUT_SMALL", GetAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.SCALEOUT_SMALL_OBJECT, 0) + "\t Max:" + GetAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.SCALEOUT_SMALL_OBJECT_MAX_UI, 0));
                EditorGUILayout.LabelField("SCALEOUT_MEDIUM", GetAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.SCALEOUT_MEDIUM_OBJECT, 0) + "\t Max:" + GetAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.SCALEOUT_MEDIUM_OBJECT_MAX_UI, 0));
                EditorGUILayout.LabelField("SCALEOUT_LARGE", GetAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.SCALEOUT_LARGE_OBJECT, 0) + "\t Max:" + GetAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.SCALEOUT_LARGE_OBJECT_MAX_UI, 0));
                EditorGUILayout.LabelField("LODMIX_GRASS", GetAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.LODMIX_GRASS, 0) + "\t Max:" + GetAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.LODMIX_GRASS_MAX_UI, 0));
                EditorGUILayout.LabelField("LODMIX_DETAIL", GetAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.LODMIX_DETAIL, 0) + "\t Max:" + GetAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.LODMIX_DETAIL_MAX_UI, 0));
                EditorGUILayout.LabelField("LODMIX_PLANT", GetAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.LODMIX_PLANT, 0) + "\t Max:" + GetAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.LODMIX_PLANT_MAX_UI, 0));
                EditorGUILayout.LabelField("LODMIX_TREE", GetAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.LODMIX_TREE, 0) + "\t Max:" + GetAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.LODMIX_TREE_MAX_UI, 0));
                EditorGUILayout.LabelField("LODMIX_SMALL", GetAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.LODMIX_SMALL_OBJECT, 0) + "\t Max:" + GetAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.LODMIX_SMALL_OBJECT_MAX_UI, 0));
                EditorGUILayout.LabelField("LODMIX_MEDIUM", GetAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.LODMIX_MEDIUM_OBJECT, 0) + "\t Max:" + GetAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.LODMIX_MEDIUM_OBJECT_MAX_UI, 0));
                EditorGUILayout.LabelField("LODMIX_LARGE", GetAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.LODMIX_LARGE_OBJECT, 0) + "\t Max:" + GetAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.LODMIX_LARGE_OBJECT_MAX_UI, 0));
                EditorGUILayout.LabelField("DENSITYFADE_GRASS", GetAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.DENSITYFADE_GRASS, 0) + "\t Max:" + GetAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.DENSITYFADE_GRASS_MAX_UI, 0));
                EditorGUILayout.LabelField("DENSITYFADE_DETAIL", GetAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.DENSITYFADE_DETAIL, 0) + "\t Max:" + GetAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.DENSITYFADE_DETAIL_MAX_UI, 0));
                EditorGUILayout.LabelField("DENSITYFADE_PLANT", GetAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.DENSITYFADE_PLANT, 0) + "\t Max:" + GetAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.DENSITYFADE_PLANT_MAX_UI, 0));
                EditorGUILayout.LabelField("DENSITYFADE_TREE", GetAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.DENSITYFADE_TREE, 0) + "\t Max:" + GetAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.DENSITYFADE_TREE_MAX_UI, 0));
                EditorGUILayout.LabelField("DENSITYFADE_SMALL", GetAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.DENSITYFADE_SMALL_OBJECT, 0) + "\t Max:" + GetAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.DENSITYFADE_SMALL_OBJECT_MAX_UI, 0));
                EditorGUILayout.LabelField("DENSITYFADE_MEDIUM", GetAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.DENSITYFADE_MEDIUM_OBJECT, 0) + "\t Max:" + GetAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.DENSITYFADE_MEDIUM_OBJECT_MAX_UI, 0));
                EditorGUILayout.LabelField("DENSITYFADE_LARGE", GetAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.DENSITYFADE_LARGE_OBJECT, 0) + "\t Max:" + GetAutoSetting(UI.UIStormFloatValueDisplay.StormGlobalSetting.DENSITYFADE_LARGE_OBJECT_MAX_UI, 0));
            }
        }
        private void LODBiasPanel(bool showHelp)
        {
            DoFloatField("globalLODBias", p_globalLODBias, 0, p_globalLODBiasMaxUI, showHelp, (StormFeatureType)0xFF, false);
            DoFloatField("grassLODBias", p_grassLODBias, 0, p_grassLODBiasMaxUI, showHelp, StormFeatureType.GRASS, false);
            DoFloatField("detailLODBias", p_detailLODBias, 0, p_detailLODBiasMaxUI, showHelp, StormFeatureType.DETAIL, false);
            DoFloatField("plantLODBias", p_plantLODBias, 0, p_plantLODBiasMaxUI, showHelp, StormFeatureType.PLANT, false);
            DoFloatField("treeLODBias", p_treeLODBias, 0, p_treeLODBiasMaxUI, showHelp, StormFeatureType.TREE, false);
            DoFloatField("smallObjLODBias", p_smallObjLODBias, 0, p_smallObjLODBiasMaxUI, showHelp, StormFeatureType.SMALL_OBJECT, false);
            DoFloatField("mediumObjLODBias", p_mediumObjLODBias, 0, p_mediumObjLODBiasMaxUI, showHelp, StormFeatureType.MEDIUM_OBJECT, false);
            DoFloatField("largeObjLODBias", p_largeObjLODBias, 0, p_largeObjLODBiasMaxUI, showHelp, StormFeatureType.LARGE_OBJECT, false);
        }
        private void DensityPanel(bool showHelp)
        {
            DoFloatField("grassDensity", p_grassDensity, 0.01f, p_grassDensityMaxUI, showHelp, StormFeatureType.GRASS, true);
            DoFloatField("detailDensity", p_detailDensity, 0.01f, p_detailDensityMaxUI, showHelp, StormFeatureType.DETAIL, true);
            DoFloatField("plantDensity", p_plantDensity, 0.01f, p_plantDensityMaxUI, showHelp, StormFeatureType.PLANT, true);
            DoFloatField("treeDensity", p_treeDensity, 0.01f, p_treeDensityMaxUI, showHelp, StormFeatureType.TREE, true);
            DoFloatField("smallObjDensity", p_smallObjDensity, 0.01f, p_smallObjDensityMaxUI, showHelp, StormFeatureType.SMALL_OBJECT, true);
            DoFloatField("mediumObjDensity", p_mediumObjDensity, 0.01f, p_mediumObjDensityMaxUI, showHelp, StormFeatureType.MEDIUM_OBJECT, true);
            DoFloatField("largeObjDensity", p_largeObjDensity, 0.01f, p_largeObjDensityMaxUI, showHelp, StormFeatureType.LARGE_OBJECT, true);
        }
        private void DrawDistancePanel(bool showHelp)
        {
            DoFloatField("grassDistance", p_grassDistance, 0, p_grassDistanceMaxUI, showHelp, StormFeatureType.GRASS, false);
            DoFloatField("detailDistance", p_detailDistance, 0, p_detailDistanceMaxUI, showHelp, StormFeatureType.DETAIL, false);
            DoFloatField("plantDistance", p_plantDistance, 0, p_plantDistanceMaxUI, showHelp, StormFeatureType.PLANT, false);
            DoFloatField("treeDistance", p_treeDistance, 0, p_treeDistanceMaxUI, showHelp, StormFeatureType.TREE, false);
            DoFloatField("smallObjDistance", p_smallObjDistance, 0, p_smallObjDistanceMaxUI, showHelp, StormFeatureType.SMALL_OBJECT, false);
            DoFloatField("mediumObjDistance", p_mediumObjDistance, 0, p_mediumObjDistanceMaxUI, showHelp, StormFeatureType.MEDIUM_OBJECT, false);
            DoFloatField("largeObjDistance", p_largeObjDistance, 0, p_largeObjDistanceMaxUI, showHelp, StormFeatureType.LARGE_OBJECT, false);
            DoFloatField("colliderDistance", p_colliderDistance, 0, p_colliderDistanceMaxUI, showHelp, StormFeatureType.COLLIDERS, false);
        }
        private void ScaleoutPanel(bool showHelp)
        {
            DoFloatField("grassScaleoutMult", p_grassScaleoutMult, 0, p_grassScaleoutMultMaxUI, showHelp, StormFeatureType.GRASS, false);
            DoFloatField("detailScaleoutMult", p_detailScaleoutMult, 0, p_detailScaleoutMultMaxUI, showHelp, StormFeatureType.DETAIL, false);
            DoFloatField("plantScaleoutMult", p_plantScaleoutMult, 0, p_plantScaleoutMultMaxUI, showHelp, StormFeatureType.PLANT, false);
            DoFloatField("treeScaleoutMult", p_treeScaleoutMult, 0, p_treeScaleoutMultMaxUI, showHelp, StormFeatureType.TREE, false);
            DoFloatField("smallObjScaleoutMult", p_smallObjScaleoutMult, 0, p_smallObjScaleoutMultMaxUI, showHelp, StormFeatureType.SMALL_OBJECT, false);
            DoFloatField("mediumObjScaleoutMult", p_mediumObjScaleoutMult, 0, p_mediumObjScaleoutMultMaxUI, showHelp, StormFeatureType.MEDIUM_OBJECT, false);
            DoFloatField("largeObjScaleoutMult", p_largeObjScaleoutMult, 0, p_largeObjScaleoutMultMaxUI, showHelp, StormFeatureType.LARGE_OBJECT, false);
        }
        private void LODMixPanel(bool showHelp)
        {
            DoFloatField("grassLODMixMult", p_grassLODMixMult, 0, p_grassLODMixMultMaxUI, showHelp, StormFeatureType.GRASS, false);
            DoFloatField("detailLODMixMult", p_detailLODMixMult, 0, p_detailLODMixMultMaxUI, showHelp, StormFeatureType.DETAIL, false);
            DoFloatField("plantLODMixMult", p_plantLODMixMult, 0, p_plantLODMixMultMaxUI, showHelp, StormFeatureType.PLANT, false);
            DoFloatField("treeLODMixMult", p_treeLODMixMult, 0, p_treeLODMixMultMaxUI, showHelp, StormFeatureType.TREE, false);
            DoFloatField("smallObjLODMixMult", p_smallObjLODMixMult, 0, p_smallObjLODMixMultMaxUI, showHelp, StormFeatureType.SMALL_OBJECT, false);
            DoFloatField("mediumObjLODMixMult", p_mediumObjLODMixMult, 0, p_mediumObjLODMixMultMaxUI, showHelp, StormFeatureType.MEDIUM_OBJECT, false);
            DoFloatField("largeObjLODMixMult", p_largeObjLODMixMult, 0, p_largeObjLODMixMultMaxUI, showHelp, StormFeatureType.LARGE_OBJECT, false);
        }
        private void DensityFadePanel(bool showHelp)
        {
            DoFloatField("grassDensityFadeMult", p_grassDensityFadeMult, 0, p_grassDensityFadeMultMaxUI, showHelp, StormFeatureType.GRASS, false);
            DoFloatField("detailDensityFadeMult", p_detailDensityFadeMult, 0, p_detailDensityFadeMultMaxUI, showHelp, StormFeatureType.DETAIL, false);
            DoFloatField("plantDensityFadeMult", p_plantDensityFadeMult, 0, p_plantDensityFadeMultMaxUI, showHelp, StormFeatureType.PLANT, false);
            DoFloatField("treeDensityFadeMult", p_treeDensityFadeMult, 0, p_treeDensityFadeMultMaxUI, showHelp, StormFeatureType.TREE, false);
            DoFloatField("smallObjDensityFadeMult", p_smallObjDensityFadeMult, 0, p_smallObjDensityFadeMultMaxUI, showHelp, StormFeatureType.SMALL_OBJECT, false);
            DoFloatField("mediumObjDensityFadeMult", p_mediumObjDensityFadeMult, 0, p_mediumObjDensityFadeMultMaxUI, showHelp, StormFeatureType.MEDIUM_OBJECT, false);
            DoFloatField("largeObjDensityFadeMult", p_largeObjDensityFadeMult, 0, p_largeObjDensityFadeMultMaxUI, showHelp, StormFeatureType.LARGE_OBJECT, false);
        }
        private void LoadingPanel(bool showHelp)
        {
            m_editorUtils.PropertyField("InstantFirstLoad", p_instantFirstLoad, showHelp);
            p_loadDistance.floatValue = m_editorUtils.FloatField("loadDistance", p_loadDistance.floatValue, showHelp);
            p_unloadDistance.floatValue = m_editorUtils.FloatField("unloadDistance", p_unloadDistance.floatValue, showHelp);
            p_maxSimultaneousLoad.intValue = m_editorUtils.IntField("maxSimultaneousLoad", p_maxSimultaneousLoad.intValue, showHelp);
            p_maxLoadMasksPerFrame.intValue = m_editorUtils.IntField("maxLoadMasksPerFrame", p_maxLoadMasksPerFrame.intValue, showHelp);
            p_maxInstancesPerFrame.intValue = m_editorUtils.IntField("MaxInstancesPerFrame", p_maxInstancesPerFrame.intValue, showHelp);
        }
        private void InternalDataPanel(bool showHelp)
        {
            EditorGUI.BeginChangeCheck();
            p_renderCellSize.floatValue = m_editorUtils.FloatField("worldTileSize", p_renderCellSize.floatValue, showHelp);
            p_curveResolution.intValue = m_editorUtils.IntField("curveResolution", p_curveResolution.intValue, showHelp);
            p_heightResolution.intValue = m_editorUtils.IntField("heightResolution", p_heightResolution.intValue, showHelp);
            if (EditorGUI.EndChangeCheck())
                m_needsFullReset = true;
            m_editorUtils.PropertyField("useJobs", p_useJobs, showHelp);
            m_editorUtils.PropertyField("allowFrameDelay", p_allowFrameDelay, showHelp);
            if (m_expertMode)
            {
                p_cullingTurnOffset.floatValue = m_editorUtils.FloatField("CullingTurnOffset", p_cullingTurnOffset.floatValue, showHelp);
            }
            else
            {
                m_editorUtils.Slider(p_cullingTurnOffset, 0f, 2f, "CullingTurnOffset", showHelp);
            }
            m_editorUtils.PropertyField("MinUpdateMovement", p_MinUpdateMovement, showHelp);
            m_editorUtils.PropertyField("MinUpdateTurn", p_MinUpdateTurn, showHelp);
                m_editorUtils.PropertyField("ShadowExtensionMult", p_ShadowExtensionMult, showHelp);
            EditorGUI.BeginChangeCheck();
            //m_editorUtils.PropertyField("UseNewShadowCalc", p_useNewShadowCalc, showHelp);
            m_editorUtils.PropertyField("SplitShadowRendering", p_splitShadowRendering, showHelp);
            if (EditorGUI.EndChangeCheck() && StormWorld.Instance != null)
                StormAPI.OnLODSettingsChanged?.Invoke((StormFeatureType)255);
            EditorGUI.BeginChangeCheck();
            m_editorUtils.PropertyField("p_AlwaysRasterizePolygons", p_AlwaysRasterizePolygons, showHelp);
            if (p_AlwaysRasterizePolygons.boolValue)
            {
                m_editorUtils.PropertyField("p_DefaultPolygonResolution", p_DefaultPolygonResolution, showHelp);
                m_editorUtils.PropertyField("p_MaxTotalPolygonResolution", p_MaxTotalPolygonResolution, showHelp);
            }
            if (EditorGUI.EndChangeCheck() && StormWorld.Instance != null)
                StormWorld.Instance.ForceFullRefresh();
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
            EditorGUI.BeginChangeCheck();
            m_editorUtils.PropertyField("renderType", p_renderType, showHelp);
            //*/
            m_editorUtils.PropertyField("cullType", p_cullType, showHelp);
            if (p_cullType.enumValueIndex == 0)
            {
                m_editorUtils.PropertyField("cullShader", p_cullShader, showHelp);
                m_editorUtils.PropertyField("cullKernelStandard", p_cullKernelStandard, showHelp);
                m_editorUtils.PropertyField("cullKernelSimple", p_cullKernelSimple, showHelp);
                m_editorUtils.PropertyField("cullKernelShadowOnly", p_cullKernelShadowOnly, showHelp);
                m_editorUtils.PropertyField("cullCountShader", p_cullCountShader, showHelp);
                m_editorUtils.PropertyField("cullCountKernel", p_cullCountKernel, showHelp);
                m_editorUtils.PropertyField("finalizeShader", p_finalizeShader, showHelp);
                m_editorUtils.PropertyField("finalizeKernel", p_finalizeKernel, showHelp);
            }
            //*/
#if PW_STORM_HDRP
            if (StormUtils.GetRenderPipline() == StormRP.HDRP)
                p_DiffusionProfileAsset.objectReferenceValue = m_editorUtils.ObjectField("p_DiffusionProfileAsset", p_DiffusionProfileAsset.objectReferenceValue, typeof(UnityEngine.Rendering.HighDefinition.DiffusionProfileSettings), false, showHelp);
#endif
            if (EditorGUI.EndChangeCheck() && StormWorld.Instance != null)
                StormWorld.Instance.ForceFullRefresh(true);
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
            if (EditorGUI.EndChangeCheck() && StormWorld.Instance != null)
                StormWorld.Instance.UpdateFeaturesets(true);
        }
        private void PrefabCreationPanel(bool showHelp)
        {
            p_DefaultLODProgression.animationCurveValue = m_editorUtils.CurveField("DefaultLODProgression", p_DefaultLODProgression.animationCurveValue, showHelp);
            ++EditorGUI.indentLevel;
            m_editorUtils.PropertyField("DefaultDetailLODs", p_DefaultDetailLODs, showHelp);
            --EditorGUI.indentLevel;
            if (m_expertMode)
            {
                p_DefaultLODFade.floatValue = m_editorUtils.FloatField("DefaultLODFade", p_DefaultLODFade.floatValue, showHelp);
                p_DefaultLODMix.floatValue = m_editorUtils.FloatField("DefaultLODMix", p_DefaultLODMix.floatValue, showHelp);
                p_DefaultDensity.intValue = m_editorUtils.IntField("DefaultDensity", p_DefaultDensity.intValue, showHelp);
            }
            else
            {
                m_editorUtils.Slider(p_DefaultLODFade, 0, 1, "DefaultLODFade", showHelp);
                m_editorUtils.Slider(p_DefaultLODMix, 0, 1, "DefaultLODMix", showHelp);
                m_editorUtils.IntSlider(p_DefaultDensity, 1, 200, "DefaultDensity", showHelp);
            }
            m_editorUtils.PropertyField("DefaultRenderType", p_DefaultRenderType, showHelp);
            ++EditorGUI.indentLevel;
            m_editorUtils.PropertyField("IgnoredPrefabs", p_IgnoredPrefabs, showHelp);
            m_editorUtils.PropertyField("p_IgnoredLayers", p_IgnoredLayers, showHelp);
            m_editorUtils.PropertyField("p_IgnoredTags", p_IgnoredTags, showHelp);
            --EditorGUI.indentLevel;
            m_editorUtils.PropertyField("p_IngestStatic", p_IngestStatic, showHelp);
            m_editorUtils.PropertyField("p_IngestNonStatic", p_IngestNonStatic, showHelp);
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
            m_needsUpdateGeneration = true;
            serializedObject.Update();
            serializedObject.ApplyModifiedProperties();
        }

        private void DoFloatField(string key, SerializedProperty prop, float min, SerializedProperty maxProp, bool showHelp, StormFeatureType featureType, bool isGeneration)
        {
            float val;
            float max = maxProp.floatValue;
            EditorGUI.BeginChangeCheck();
            if (m_expertMode)
            {
                EditorGUILayout.BeginHorizontal();
                val = m_editorUtils.FloatField(key, prop.floatValue, showHelp);
                m_editorUtils.Label("MaxUILabel", UnityEngine.GUILayout.ExpandWidth(false));
                max = EditorGUILayout.FloatField(max, UnityEngine.GUILayout.Width(50f));
                EditorGUILayout.EndHorizontal();
            }
            else
                val = m_editorUtils.Slider(key, prop.floatValue, min, max, showHelp);
            if (EditorGUI.EndChangeCheck())
            {
                prop.floatValue = val;
                maxProp.floatValue = max;
                if (isGeneration)
                {
                    m_needsUpdateGeneration = true;
                    GenerationUpdateType |= featureType;
                }
                else
                {
                    m_needsUpdateLOD = true;
                    LODUpdateType |= featureType;
                }
            }
        }
    }
}