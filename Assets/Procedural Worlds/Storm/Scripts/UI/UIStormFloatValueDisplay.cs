using ProceduralWorlds.Storm.Core;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using UnityEngine.Events;

/// Copyright 2023 Procedural Worlds Ltd
/// Written by Jonathan Williams as part of Storm

namespace ProceduralWorlds.Storm.UI
{
    [AddComponentMenu("Procedural Worlds/Storm/UI/Float Value Display")]
    public class UIStormFloatValueDisplay : MonoBehaviour
    {
        [Serializable]
        public enum StormGlobalSetting : short
        {
            CUSTOM = -1,
            LOD_GLOBAL,
            LOD_GRASS,
            LOD_DETAIL,
            LOD_PLANT,
            LOD_TREE,
            LOD_SMALL_OBJECT,
            LOD_MEDIUM_OBJECT,
            LOD_LARGE_OBJECT,
            LODMIX_GRASS,
            LODMIX_DETAIL,
            LODMIX_PLANT,
            LODMIX_TREE,
            LODMIX_SMALL_OBJECT,
            LODMIX_MEDIUM_OBJECT,
            LODMIX_LARGE_OBJECT,
            DISTANCE_GRASS,
            DISTANCE_DETAIL,
            DISTANCE_PLANT,
            DISTANCE_TREE,
            DISTANCE_SMALL_OBJECT,
            DISTANCE_MEDIUM_OBJECT,
            DISTANCE_LARGE_OBJECT,
            DISTANCE_COLLIDERS,
            DENSITYFADE_GRASS,
            DENSITYFADE_DETAIL,
            DENSITYFADE_PLANT,
            DENSITYFADE_TREE,
            DENSITYFADE_SMALL_OBJECT,
            DENSITYFADE_MEDIUM_OBJECT,
            DENSITYFADE_LARGE_OBJECT,
            SCALEOUT_GRASS,
            SCALEOUT_DETAIL,
            SCALEOUT_PLANT,
            SCALEOUT_TREE,
            SCALEOUT_SMALL_OBJECT,
            SCALEOUT_MEDIUM_OBJECT,
            SCALEOUT_LARGE_OBJECT,
            DENSITY_GRASS,
            DENSITY_DETAIL,
            DENSITY_PLANT,
            DENSITY_TREE,
            DENSITY_SMALL_OBJECT,
            DENSITY_MEDIUM_OBJECT,
            DENSITY_LARGE_OBJECT,

            LOD_GLOBAL_MAX_UI = 200,
            LOD_GRASS_MAX_UI,
            LOD_DETAIL_MAX_UI,
            LOD_PLANT_MAX_UI,
            LOD_TREE_MAX_UI,
            LOD_SMALL_OBJECT_MAX_UI,
            LOD_MEDIUM_OBJECT_MAX_UI,
            LOD_LARGE_OBJECT_MAX_UI,
            LODMIX_GRASS_MAX_UI,
            LODMIX_DETAIL_MAX_UI,
            LODMIX_PLANT_MAX_UI,
            LODMIX_TREE_MAX_UI,
            LODMIX_SMALL_OBJECT_MAX_UI,
            LODMIX_MEDIUM_OBJECT_MAX_UI,
            LODMIX_LARGE_OBJECT_MAX_UI,
            DISTANCE_GRASS_MAX_UI,
            DISTANCE_DETAIL_MAX_UI,
            DISTANCE_PLANT_MAX_UI,
            DISTANCE_TREE_MAX_UI,
            DISTANCE_SMALL_OBJECT_MAX_UI,
            DISTANCE_MEDIUM_OBJECT_MAX_UI,
            DISTANCE_LARGE_OBJECT_MAX_UI,
            DISTANCE_COLLIDERS_MAX_UI,
            DENSITYFADE_GRASS_MAX_UI,
            DENSITYFADE_DETAIL_MAX_UI,
            DENSITYFADE_PLANT_MAX_UI,
            DENSITYFADE_TREE_MAX_UI,
            DENSITYFADE_SMALL_OBJECT_MAX_UI,
            DENSITYFADE_MEDIUM_OBJECT_MAX_UI,
            DENSITYFADE_LARGE_OBJECT_MAX_UI,
            SCALEOUT_GRASS_MAX_UI,
            SCALEOUT_DETAIL_MAX_UI,
            SCALEOUT_PLANT_MAX_UI,
            SCALEOUT_TREE_MAX_UI,
            SCALEOUT_SMALL_OBJECT_MAX_UI,
            SCALEOUT_MEDIUM_OBJECT_MAX_UI,
            SCALEOUT_LARGE_OBJECT_MAX_UI,
            DENSITY_GRASS_MAX_UI,
            DENSITY_DETAIL_MAX_UI,
            DENSITY_PLANT_MAX_UI,
            DENSITY_TREE_MAX_UI,
            DENSITY_SMALL_OBJECT_MAX_UI,
            DENSITY_MEDIUM_OBJECT_MAX_UI,
            DENSITY_LARGE_OBJECT_MAX_UI,

            PERFORMANCE_UPDATE_RATE = 1000,
        }

        public Slider m_slider;
        public TMP_InputField m_input;
        [SerializeField]
        public StormGlobalSetting m_setting = StormGlobalSetting.LOD_GLOBAL;

        private string m_settingFormat = "F2";
        private bool m_isSettingValues = false;
        private int m_layerIndex = -1;

        public UnityEvent<float> OnValueChanged;
        public Action OnChanged;

        private void OnEnable()
        {
            float val = GetValue();
            Init();
            m_isSettingValues = true;
            if (m_slider != null)
            {
                m_slider.value = val;
                m_slider.onValueChanged.RemoveAllListeners();
                m_slider.onValueChanged.AddListener(OnSliderChanged);
            }
            if (m_input != null)
            {
                m_input.text = val.ToString(m_settingFormat);
                m_input.onValueChanged.RemoveAllListeners();
                m_input.onValueChanged.AddListener(OnTextValueChanged);
            }
            m_isSettingValues = false;
            if (m_setting.ToString().StartsWith("DISTANCE"))
            {
                m_layerIndex = LayerMask.NameToLayer(m_setting.ToString());
                SetCameraLayerCulling(val);
            }
        }

        public void UpdateValues()
        {
            float val = GetValue();
            m_isSettingValues = true;
            if (m_slider != null)
            {
                m_slider.value = val;
                m_slider.onValueChanged.RemoveListener(OnSliderChanged);
                m_slider.onValueChanged.AddListener(OnSliderChanged);
            }
            if (m_input != null)
            {
                m_input.text = val.ToString(m_settingFormat);
                m_input.onValueChanged.RemoveListener(OnTextValueChanged);
                m_input.onValueChanged.AddListener(OnTextValueChanged);
            }
            m_isSettingValues = false;
            if (m_setting.ToString().StartsWith("DISTANCE"))
            {
                m_layerIndex = LayerMask.NameToLayer(m_setting.ToString());
                SetCameraLayerCulling(val);
            }
            OnValueChanged?.Invoke(val);
        }
        public void SetValue(float val)
        {
            if (m_isSettingValues || !Application.isPlaying)
                return;
            m_isSettingValues = true;
            SetValueInternal(val);
            Save();
            if (m_input != null)
                m_input.text = val.ToString(m_settingFormat);
            if (m_slider != null)
                m_slider.value = val;
            m_isSettingValues = false;
        }
        public void SetMax(float max)
        {
            if (m_slider != null)
                m_slider.maxValue = max;
        }
        private void OnSliderChanged(float val)
        {
            if (m_isSettingValues || !Application.isPlaying)
                return;
            m_isSettingValues = true;
            SetValueInternal(val);
            Save();
            if (m_input != null)
                m_input.text = val.ToString(m_settingFormat);
            m_isSettingValues = false;
        }
        private void OnTextValueChanged(string txt)
        {
            if (m_isSettingValues || !Application.isPlaying)
                return;
            if (float.TryParse(txt, out float val))
            {
                m_isSettingValues = true;
                SetValueInternal(val);
                Save();
                if (m_slider != null)
                    m_slider.value = val;
                m_isSettingValues = false;
            }
        }

        private void Init()
        {
            if (m_setting != StormGlobalSetting.CUSTOM)
            {
                float minValue = 0f;
                float maxValue = 100f;
                switch (m_setting)
                {
                    case StormGlobalSetting.CUSTOM:
                        break;
                    case StormGlobalSetting.LOD_GLOBAL:
                    case StormGlobalSetting.LOD_GRASS:
                    case StormGlobalSetting.LOD_DETAIL:
                    case StormGlobalSetting.LOD_PLANT:
                    case StormGlobalSetting.LOD_TREE:
                    case StormGlobalSetting.LOD_SMALL_OBJECT:
                    case StormGlobalSetting.LOD_MEDIUM_OBJECT:
                    case StormGlobalSetting.LOD_LARGE_OBJECT:
                        m_settingFormat = "F2";
                        minValue = 0.0001f;
                        break;
                    case StormGlobalSetting.DISTANCE_GRASS:
                    case StormGlobalSetting.DISTANCE_DETAIL:
                    case StormGlobalSetting.DISTANCE_PLANT:
                    case StormGlobalSetting.DISTANCE_TREE:
                    case StormGlobalSetting.DISTANCE_SMALL_OBJECT:
                    case StormGlobalSetting.DISTANCE_MEDIUM_OBJECT:
                    case StormGlobalSetting.DISTANCE_LARGE_OBJECT:
                    case StormGlobalSetting.DISTANCE_COLLIDERS:
                        m_settingFormat = "F0";
                        minValue = -1;
                        break;
                    case StormGlobalSetting.LODMIX_GRASS:
                    case StormGlobalSetting.LODMIX_DETAIL:
                    case StormGlobalSetting.LODMIX_PLANT:
                    case StormGlobalSetting.LODMIX_TREE:
                    case StormGlobalSetting.LODMIX_SMALL_OBJECT:
                    case StormGlobalSetting.LODMIX_MEDIUM_OBJECT:
                    case StormGlobalSetting.LODMIX_LARGE_OBJECT:
                    case StormGlobalSetting.DENSITYFADE_GRASS:
                    case StormGlobalSetting.DENSITYFADE_DETAIL:
                    case StormGlobalSetting.DENSITYFADE_PLANT:
                    case StormGlobalSetting.DENSITYFADE_TREE:
                    case StormGlobalSetting.DENSITYFADE_SMALL_OBJECT:
                    case StormGlobalSetting.DENSITYFADE_MEDIUM_OBJECT:
                    case StormGlobalSetting.DENSITYFADE_LARGE_OBJECT:
                    case StormGlobalSetting.SCALEOUT_GRASS:
                    case StormGlobalSetting.SCALEOUT_DETAIL:
                    case StormGlobalSetting.SCALEOUT_PLANT:
                    case StormGlobalSetting.SCALEOUT_TREE:
                    case StormGlobalSetting.SCALEOUT_SMALL_OBJECT:
                    case StormGlobalSetting.SCALEOUT_MEDIUM_OBJECT:
                    case StormGlobalSetting.SCALEOUT_LARGE_OBJECT:
                        m_settingFormat = "F2";
                        minValue = 0;
                        break;
                    case StormGlobalSetting.DENSITY_GRASS:
                    case StormGlobalSetting.DENSITY_DETAIL:
                    case StormGlobalSetting.DENSITY_PLANT:
                    case StormGlobalSetting.DENSITY_TREE:
                    case StormGlobalSetting.DENSITY_SMALL_OBJECT:
                    case StormGlobalSetting.DENSITY_MEDIUM_OBJECT:
                    case StormGlobalSetting.DENSITY_LARGE_OBJECT:
                        m_settingFormat = "F2";
                        minValue = 0.01f;
                        break;
                    case StormGlobalSetting.PERFORMANCE_UPDATE_RATE:
                        m_settingFormat = "F2";
                        minValue = 0f;
                        break;
                }
                switch (m_setting)
                {
                    case StormGlobalSetting.LOD_GLOBAL:
                        maxValue = StormSettings.GlobalLODBiasMaxUI;
                        break;
                    case StormGlobalSetting.LOD_GRASS:
                        maxValue = StormSettings.GrassLODBiasMaxUI;
                        break;
                    case StormGlobalSetting.LOD_DETAIL:
                        maxValue = StormSettings.DetailLODBiasMaxUI;
                        break;
                    case StormGlobalSetting.LOD_PLANT:
                        maxValue = StormSettings.PlantLODBiasMaxUI;
                        break;
                    case StormGlobalSetting.LOD_TREE:
                        maxValue = StormSettings.TreeLODBiasMaxUI;
                        break;
                    case StormGlobalSetting.LOD_SMALL_OBJECT:
                        maxValue = StormSettings.SmallObjLODBiasMaxUI;
                        break;
                    case StormGlobalSetting.LOD_MEDIUM_OBJECT:
                        maxValue = StormSettings.MediumObjLODBiasMaxUI;
                        break;
                    case StormGlobalSetting.LOD_LARGE_OBJECT:
                        maxValue = StormSettings.LargeObjLODBiasMaxUI;
                        break;
                    case StormGlobalSetting.DISTANCE_GRASS:
                        maxValue = StormSettings.GrassDistanceMaxUI;
                        break;
                    case StormGlobalSetting.DISTANCE_DETAIL:
                        maxValue = StormSettings.DetailDistanceMaxUI;
                        break;
                    case StormGlobalSetting.DISTANCE_PLANT:
                        maxValue = StormSettings.PlantDistanceMaxUI;
                        break;
                    case StormGlobalSetting.DISTANCE_TREE:
                        maxValue = StormSettings.TreeDistanceMaxUI;
                        break;
                    case StormGlobalSetting.DISTANCE_SMALL_OBJECT:
                        maxValue = StormSettings.SmallObjDistanceMaxUI;
                        break;
                    case StormGlobalSetting.DISTANCE_MEDIUM_OBJECT:
                        maxValue = StormSettings.MediumObjDistanceMaxUI;
                        break;
                    case StormGlobalSetting.DISTANCE_LARGE_OBJECT:
                        maxValue = StormSettings.LargeObjDistanceMaxUI;
                        break;
                    case StormGlobalSetting.DISTANCE_COLLIDERS:
                        maxValue = StormSettings.ColliderDistanceMaxUI;
                        break;
                    case StormGlobalSetting.LODMIX_GRASS:
                        maxValue = StormSettings.GrassLODMixMultMaxUI;
                        break;
                    case StormGlobalSetting.LODMIX_DETAIL:
                        maxValue = StormSettings.DetailLODMixMultMaxUI;
                        break;
                    case StormGlobalSetting.LODMIX_PLANT:
                        maxValue = StormSettings.PlantLODMixMultMaxUI;
                        break;
                    case StormGlobalSetting.LODMIX_TREE:
                        maxValue = StormSettings.TreeLODMixMultMaxUI;
                        break;
                    case StormGlobalSetting.LODMIX_SMALL_OBJECT:
                        maxValue = StormSettings.SmallObjLODMixMultMaxUI;
                        break;
                    case StormGlobalSetting.LODMIX_MEDIUM_OBJECT:
                        maxValue = StormSettings.MediumObjLODMixMultMaxUI;
                        break;
                    case StormGlobalSetting.LODMIX_LARGE_OBJECT:
                        maxValue = StormSettings.LargeObjLODMixMultMaxUI;
                        break;
                    case StormGlobalSetting.DENSITYFADE_GRASS:
                        maxValue = StormSettings.GrassDensityFadeMultMaxUI;
                        break;
                    case StormGlobalSetting.DENSITYFADE_DETAIL:
                        maxValue = StormSettings.DetailDensityFadeMultMaxUI;
                        break;
                    case StormGlobalSetting.DENSITYFADE_PLANT:
                        maxValue = StormSettings.PlantDensityFadeMultMaxUI;
                        break;
                    case StormGlobalSetting.DENSITYFADE_TREE:
                        maxValue = StormSettings.TreeDensityFadeMultMaxUI;
                        break;
                    case StormGlobalSetting.DENSITYFADE_SMALL_OBJECT:
                        maxValue = StormSettings.SmallObjDensityFadeMultMaxUI;
                        break;
                    case StormGlobalSetting.DENSITYFADE_MEDIUM_OBJECT:
                        maxValue = StormSettings.MediumObjDensityFadeMultMaxUI;
                        break;
                    case StormGlobalSetting.DENSITYFADE_LARGE_OBJECT:
                        maxValue = StormSettings.LargeObjDensityFadeMultMaxUI;
                        break;
                    case StormGlobalSetting.SCALEOUT_GRASS:
                        maxValue = StormSettings.GrassScaleoutMultMaxUI;
                        break;
                    case StormGlobalSetting.SCALEOUT_DETAIL:
                        maxValue = StormSettings.DetailScaleoutMultMaxUI;
                        break;
                    case StormGlobalSetting.SCALEOUT_PLANT:
                        maxValue = StormSettings.PlantScaleoutMultMaxUI;
                        break;
                    case StormGlobalSetting.SCALEOUT_TREE:
                        maxValue = StormSettings.TreeScaleoutMultMaxUI;
                        break;
                    case StormGlobalSetting.SCALEOUT_SMALL_OBJECT:
                        maxValue = StormSettings.SmallObjScaleoutMultMaxUI;
                        break;
                    case StormGlobalSetting.SCALEOUT_MEDIUM_OBJECT:
                        maxValue = StormSettings.MediumObjScaleoutMultMaxUI;
                        break;
                    case StormGlobalSetting.SCALEOUT_LARGE_OBJECT:
                        maxValue = StormSettings.LargeObjScaleoutMultMaxUI;
                        break;
                    case StormGlobalSetting.DENSITY_GRASS:
                        maxValue = StormSettings.GrassDensityMultMaxUI;
                        break;
                    case StormGlobalSetting.DENSITY_DETAIL:
                        maxValue = StormSettings.DetailDensityMultMaxUI;
                        break;
                    case StormGlobalSetting.DENSITY_PLANT:
                        maxValue = StormSettings.PlantDensityMultMaxUI;
                        break;
                    case StormGlobalSetting.DENSITY_TREE:
                        maxValue = StormSettings.TreeDensityMultMaxUI;
                        break;
                    case StormGlobalSetting.DENSITY_SMALL_OBJECT:
                        maxValue = StormSettings.SmallObjDensityMultMaxUI;
                        break;
                    case StormGlobalSetting.DENSITY_MEDIUM_OBJECT:
                        maxValue = StormSettings.MediumObjDensityMultMaxUI;
                        break;
                    case StormGlobalSetting.DENSITY_LARGE_OBJECT:
                        maxValue = StormSettings.LargeObjDensityMultMaxUI;
                        break;
                    case StormGlobalSetting.PERFORMANCE_UPDATE_RATE:
                        maxValue = 30;
                        break;
                }
                if (m_slider != null)
                {
                    m_slider.minValue = minValue;
                    m_slider.maxValue = maxValue;
                }
            }
        }
        private float GetValue()
        {
            switch (m_setting)
            {
                case StormGlobalSetting.LOD_GLOBAL:
                    return StormSettings.GlobalLODBias;
                case StormGlobalSetting.LOD_GRASS:
                    return StormSettings.GrassLODBias;
                case StormGlobalSetting.LOD_DETAIL:
                    return StormSettings.DetailLODBias;
                case StormGlobalSetting.LOD_PLANT:
                    return StormSettings.PlantLODBias;
                case StormGlobalSetting.LOD_TREE:
                    return StormSettings.TreeLODBias;
                case StormGlobalSetting.LOD_SMALL_OBJECT:
                    return StormSettings.SmallObjLODBias;
                case StormGlobalSetting.LOD_MEDIUM_OBJECT:
                    return StormSettings.MediumObjLODBias;
                case StormGlobalSetting.LOD_LARGE_OBJECT:
                    return StormSettings.LargeObjLODBias;
                case StormGlobalSetting.DISTANCE_GRASS:
                    return StormSettings.GrassDistance;
                case StormGlobalSetting.DISTANCE_DETAIL:
                    return StormSettings.DetailDistance;
                case StormGlobalSetting.DISTANCE_PLANT:
                    return StormSettings.PlantDistance;
                case StormGlobalSetting.DISTANCE_TREE:
                    return StormSettings.TreeDistance;
                case StormGlobalSetting.DISTANCE_SMALL_OBJECT:
                    return StormSettings.SmallObjDistance;
                case StormGlobalSetting.DISTANCE_MEDIUM_OBJECT:
                    return StormSettings.MediumObjDistance;
                case StormGlobalSetting.DISTANCE_LARGE_OBJECT:
                    return StormSettings.LargeObjDistance;
                case StormGlobalSetting.DISTANCE_COLLIDERS:
                    return StormSettings.ColliderDistance;
                case StormGlobalSetting.LODMIX_GRASS:
                    return StormSettings.GrassLODMixMult;
                case StormGlobalSetting.LODMIX_DETAIL:
                    return StormSettings.DetailLODMixMult;
                case StormGlobalSetting.LODMIX_PLANT:
                    return StormSettings.PlantLODMixMult;
                case StormGlobalSetting.LODMIX_TREE:
                    return StormSettings.TreeLODMixMult;
                case StormGlobalSetting.LODMIX_SMALL_OBJECT:
                    return StormSettings.SmallObjLODMixMult;
                case StormGlobalSetting.LODMIX_MEDIUM_OBJECT:
                    return StormSettings.MediumObjLODMixMult;
                case StormGlobalSetting.LODMIX_LARGE_OBJECT:
                    return StormSettings.LargeObjLODMixMult;
                case StormGlobalSetting.DENSITYFADE_GRASS:
                    return StormSettings.GrassDensityFadeMult;
                case StormGlobalSetting.DENSITYFADE_DETAIL:
                    return StormSettings.DetailDensityFadeMult;
                case StormGlobalSetting.DENSITYFADE_PLANT:
                    return StormSettings.PlantDensityFadeMult;
                case StormGlobalSetting.DENSITYFADE_TREE:
                    return StormSettings.TreeDensityFadeMult;
                case StormGlobalSetting.DENSITYFADE_SMALL_OBJECT:
                    return StormSettings.SmallObjDensityFadeMult;
                case StormGlobalSetting.DENSITYFADE_MEDIUM_OBJECT:
                    return StormSettings.MediumObjDensityFadeMult;
                case StormGlobalSetting.DENSITYFADE_LARGE_OBJECT:
                    return StormSettings.LargeObjDensityFadeMult;
                case StormGlobalSetting.SCALEOUT_GRASS:
                    return StormSettings.GrassScaleoutMult;
                case StormGlobalSetting.SCALEOUT_DETAIL:
                    return StormSettings.DetailScaleoutMult;
                case StormGlobalSetting.SCALEOUT_PLANT:
                    return StormSettings.PlantScaleoutMult;
                case StormGlobalSetting.SCALEOUT_TREE:
                    return StormSettings.TreeScaleoutMult;
                case StormGlobalSetting.SCALEOUT_SMALL_OBJECT:
                    return StormSettings.SmallObjScaleoutMult;
                case StormGlobalSetting.SCALEOUT_MEDIUM_OBJECT:
                    return StormSettings.MediumObjScaleoutMult;
                case StormGlobalSetting.SCALEOUT_LARGE_OBJECT:
                    return StormSettings.LargeObjScaleoutMult;
                case StormGlobalSetting.DENSITY_GRASS:
                    return StormSettings.GrassDensityMult;
                case StormGlobalSetting.DENSITY_DETAIL:
                    return StormSettings.DetailDensityMult;
                case StormGlobalSetting.DENSITY_PLANT:
                    return StormSettings.PlantDensityMult;
                case StormGlobalSetting.DENSITY_TREE:
                    return StormSettings.TreeDensityMult;
                case StormGlobalSetting.DENSITY_SMALL_OBJECT:
                    return StormSettings.SmallObjDensityMult;
                case StormGlobalSetting.DENSITY_MEDIUM_OBJECT:
                    return StormSettings.MediumObjDensityMult;
                case StormGlobalSetting.DENSITY_LARGE_OBJECT:
                    return StormSettings.LargeObjDensityMult;
                case StormGlobalSetting.LOD_GLOBAL_MAX_UI:
                    return StormSettings.GlobalLODBiasMaxUI;
                case StormGlobalSetting.LOD_GRASS_MAX_UI:
                    return StormSettings.GrassLODBiasMaxUI;
                case StormGlobalSetting.LOD_DETAIL_MAX_UI:
                    return StormSettings.DetailLODBiasMaxUI;
                case StormGlobalSetting.LOD_PLANT_MAX_UI:
                    return StormSettings.PlantLODBiasMaxUI;
                case StormGlobalSetting.LOD_TREE_MAX_UI:
                    return StormSettings.TreeLODBiasMaxUI;
                case StormGlobalSetting.LOD_SMALL_OBJECT_MAX_UI:
                    return StormSettings.SmallObjLODBiasMaxUI;
                case StormGlobalSetting.LOD_MEDIUM_OBJECT_MAX_UI:
                    return StormSettings.MediumObjLODBiasMaxUI;
                case StormGlobalSetting.LOD_LARGE_OBJECT_MAX_UI:
                    return StormSettings.LargeObjLODBiasMaxUI;
                case StormGlobalSetting.DISTANCE_GRASS_MAX_UI:
                    return StormSettings.GrassDistanceMaxUI;
                case StormGlobalSetting.DISTANCE_DETAIL_MAX_UI:
                    return StormSettings.DetailDistanceMaxUI;
                case StormGlobalSetting.DISTANCE_PLANT_MAX_UI:
                    return StormSettings.PlantDistanceMaxUI;
                case StormGlobalSetting.DISTANCE_TREE_MAX_UI:
                    return StormSettings.TreeDistanceMaxUI;
                case StormGlobalSetting.DISTANCE_SMALL_OBJECT_MAX_UI:
                    return StormSettings.SmallObjDistanceMaxUI;
                case StormGlobalSetting.DISTANCE_MEDIUM_OBJECT_MAX_UI:
                    return StormSettings.MediumObjDistanceMaxUI;
                case StormGlobalSetting.DISTANCE_LARGE_OBJECT_MAX_UI:
                    return StormSettings.LargeObjDistanceMaxUI;
                case StormGlobalSetting.DISTANCE_COLLIDERS_MAX_UI:
                    return StormSettings.ColliderDistanceMaxUI;
                case StormGlobalSetting.LODMIX_GRASS_MAX_UI:
                    return StormSettings.GrassLODMixMultMaxUI;
                case StormGlobalSetting.LODMIX_DETAIL_MAX_UI:
                    return StormSettings.DetailLODMixMultMaxUI;
                case StormGlobalSetting.LODMIX_PLANT_MAX_UI:
                    return StormSettings.PlantLODMixMultMaxUI;
                case StormGlobalSetting.LODMIX_TREE_MAX_UI:
                    return StormSettings.TreeLODMixMultMaxUI;
                case StormGlobalSetting.LODMIX_SMALL_OBJECT_MAX_UI:
                    return StormSettings.SmallObjLODMixMultMaxUI;
                case StormGlobalSetting.LODMIX_MEDIUM_OBJECT_MAX_UI:
                    return StormSettings.MediumObjLODMixMultMaxUI;
                case StormGlobalSetting.LODMIX_LARGE_OBJECT_MAX_UI:
                    return StormSettings.LargeObjLODMixMultMaxUI;
                case StormGlobalSetting.DENSITYFADE_GRASS_MAX_UI:
                    return StormSettings.GrassDensityFadeMultMaxUI;
                case StormGlobalSetting.DENSITYFADE_DETAIL_MAX_UI:
                    return StormSettings.DetailDensityFadeMultMaxUI;
                case StormGlobalSetting.DENSITYFADE_PLANT_MAX_UI:
                    return StormSettings.PlantDensityFadeMultMaxUI;
                case StormGlobalSetting.DENSITYFADE_TREE_MAX_UI:
                    return StormSettings.TreeDensityFadeMultMaxUI;
                case StormGlobalSetting.DENSITYFADE_SMALL_OBJECT_MAX_UI:
                    return StormSettings.SmallObjDensityFadeMultMaxUI;
                case StormGlobalSetting.DENSITYFADE_MEDIUM_OBJECT_MAX_UI:
                    return StormSettings.MediumObjDensityFadeMultMaxUI;
                case StormGlobalSetting.DENSITYFADE_LARGE_OBJECT_MAX_UI:
                    return StormSettings.LargeObjDensityFadeMultMaxUI;
                case StormGlobalSetting.SCALEOUT_GRASS_MAX_UI:
                    return StormSettings.GrassScaleoutMultMaxUI;
                case StormGlobalSetting.SCALEOUT_DETAIL_MAX_UI:
                    return StormSettings.DetailScaleoutMultMaxUI;
                case StormGlobalSetting.SCALEOUT_PLANT_MAX_UI:
                    return StormSettings.PlantScaleoutMultMaxUI;
                case StormGlobalSetting.SCALEOUT_TREE_MAX_UI:
                    return StormSettings.TreeScaleoutMultMaxUI;
                case StormGlobalSetting.SCALEOUT_SMALL_OBJECT_MAX_UI:
                    return StormSettings.SmallObjScaleoutMultMaxUI;
                case StormGlobalSetting.SCALEOUT_MEDIUM_OBJECT_MAX_UI:
                    return StormSettings.MediumObjScaleoutMultMaxUI;
                case StormGlobalSetting.SCALEOUT_LARGE_OBJECT_MAX_UI:
                    return StormSettings.LargeObjScaleoutMultMaxUI;
                case StormGlobalSetting.DENSITY_GRASS_MAX_UI:
                    return StormSettings.GrassDensityMultMaxUI;
                case StormGlobalSetting.DENSITY_DETAIL_MAX_UI:
                    return StormSettings.DetailDensityMultMaxUI;
                case StormGlobalSetting.DENSITY_PLANT_MAX_UI:
                    return StormSettings.PlantDensityMultMaxUI;
                case StormGlobalSetting.DENSITY_TREE_MAX_UI:
                    return StormSettings.TreeDensityMultMaxUI;
                case StormGlobalSetting.DENSITY_SMALL_OBJECT_MAX_UI:
                    return StormSettings.SmallObjDensityMultMaxUI;
                case StormGlobalSetting.DENSITY_MEDIUM_OBJECT_MAX_UI:
                    return StormSettings.MediumObjDensityMultMaxUI;
                case StormGlobalSetting.DENSITY_LARGE_OBJECT_MAX_UI:
                    return StormSettings.LargeObjDensityMultMaxUI;
                case StormGlobalSetting.PERFORMANCE_UPDATE_RATE:
                    UIStormPerformanceDisplay perf = transform.root.GetComponentInChildren<UIStormPerformanceDisplay>(true);
                    if (perf != null)
                        return perf.UpdateInterval;
                    UIStormMemoryDisplay mem = transform.root.GetComponentInChildren<UIStormMemoryDisplay>(true);
                    if (mem != null)
                        return mem.UpdateInterval;
                    return 1;
                case StormGlobalSetting.CUSTOM:
                    if (m_slider != null)
                        return m_slider.value;
                    else if (m_input != null && float.TryParse(m_input.text, out float val))
                            return val;
                    return 0;
                default:
                    return 0;
            }
        }
        private void SetValueInternal(float val)
        {
            switch (m_setting)
            {
                case StormGlobalSetting.LOD_GLOBAL:
                    StormSettings.GlobalLODBias = val;
                    break;
                case StormGlobalSetting.LOD_GRASS:
                    StormSettings.GrassLODBias = val;
                    break;
                case StormGlobalSetting.LOD_DETAIL:
                    StormSettings.DetailLODBias = val;
                    break;
                case StormGlobalSetting.LOD_PLANT:
                    StormSettings.PlantLODBias = val;
                    break;
                case StormGlobalSetting.LOD_TREE:
                    StormSettings.TreeLODBias = val;
                    break;
                case StormGlobalSetting.LOD_SMALL_OBJECT:
                    StormSettings.SmallObjLODBias = val;
                    break;
                case StormGlobalSetting.LOD_MEDIUM_OBJECT:
                    StormSettings.MediumObjLODBias = val;
                    break;
                case StormGlobalSetting.LOD_LARGE_OBJECT:
                    StormSettings.LargeObjLODBias = val;
                    break;
                case StormGlobalSetting.DISTANCE_GRASS:
                    StormSettings.GrassDistance = val;
                    break;
                case StormGlobalSetting.DISTANCE_DETAIL:
                    StormSettings.DetailDistance = val;
                    break;
                case StormGlobalSetting.DISTANCE_PLANT:
                    StormSettings.PlantDistance = val;
                    break;
                case StormGlobalSetting.DISTANCE_TREE:
                    StormSettings.TreeDistance = val;
                    break;
                case StormGlobalSetting.DISTANCE_SMALL_OBJECT:
                    StormSettings.SmallObjDistance = val;
                    break;
                case StormGlobalSetting.DISTANCE_MEDIUM_OBJECT:
                    StormSettings.MediumObjDistance = val;
                    break;
                case StormGlobalSetting.DISTANCE_LARGE_OBJECT:
                    StormSettings.LargeObjDistance = val;
                    break;
                case StormGlobalSetting.DISTANCE_COLLIDERS:
                    StormSettings.ColliderDistance = val;
                    break;
                case StormGlobalSetting.LODMIX_GRASS:
                    StormSettings.GrassLODMixMult = val;
                    break;
                case StormGlobalSetting.LODMIX_DETAIL:
                    StormSettings.DetailLODMixMult = val;
                    break;
                case StormGlobalSetting.LODMIX_PLANT:
                    StormSettings.PlantLODMixMult = val;
                    break;
                case StormGlobalSetting.LODMIX_TREE:
                    StormSettings.TreeLODMixMult = val;
                    break;
                case StormGlobalSetting.LODMIX_SMALL_OBJECT:
                    StormSettings.SmallObjLODMixMult = val;
                    break;
                case StormGlobalSetting.LODMIX_MEDIUM_OBJECT:
                    StormSettings.MediumObjLODMixMult = val;
                    break;
                case StormGlobalSetting.LODMIX_LARGE_OBJECT:
                    StormSettings.LargeObjLODMixMult = val;
                    break;
                case StormGlobalSetting.DENSITYFADE_GRASS:
                    StormSettings.GrassDensityFadeMult = val;
                    break;
                case StormGlobalSetting.DENSITYFADE_DETAIL:
                    StormSettings.DetailDensityFadeMult = val;
                    break;
                case StormGlobalSetting.DENSITYFADE_PLANT:
                    StormSettings.PlantDensityFadeMult = val;
                    break;
                case StormGlobalSetting.DENSITYFADE_TREE:
                    StormSettings.TreeDensityFadeMult = val;
                    break;
                case StormGlobalSetting.DENSITYFADE_SMALL_OBJECT:
                    StormSettings.SmallObjDensityFadeMult = val;
                    break;
                case StormGlobalSetting.DENSITYFADE_MEDIUM_OBJECT:
                    StormSettings.MediumObjDensityFadeMult = val;
                    break;
                case StormGlobalSetting.DENSITYFADE_LARGE_OBJECT:
                    StormSettings.LargeObjDensityFadeMult = val;
                    break;
                case StormGlobalSetting.SCALEOUT_GRASS:
                    StormSettings.GrassScaleoutMult = val;
                    break;
                case StormGlobalSetting.SCALEOUT_DETAIL:
                    StormSettings.DetailScaleoutMult = val;
                    break;
                case StormGlobalSetting.SCALEOUT_PLANT:
                    StormSettings.PlantScaleoutMult = val;
                    break;
                case StormGlobalSetting.SCALEOUT_TREE:
                    StormSettings.TreeScaleoutMult = val;
                    break;
                case StormGlobalSetting.SCALEOUT_SMALL_OBJECT:
                    StormSettings.SmallObjScaleoutMult = val;
                    break;
                case StormGlobalSetting.SCALEOUT_MEDIUM_OBJECT:
                    StormSettings.MediumObjScaleoutMult = val;
                    break;
                case StormGlobalSetting.SCALEOUT_LARGE_OBJECT:
                    StormSettings.LargeObjScaleoutMult = val;
                    break;
                case StormGlobalSetting.DENSITY_GRASS:
                    StormSettings.GrassDensityMult = val;
                    break;
                case StormGlobalSetting.DENSITY_DETAIL:
                    StormSettings.DetailDensityMult = val;
                    break;
                case StormGlobalSetting.DENSITY_PLANT:
                    StormSettings.PlantDensityMult = val;
                    break;
                case StormGlobalSetting.DENSITY_TREE:
                    StormSettings.TreeDensityMult = val;
                    break;
                case StormGlobalSetting.DENSITY_SMALL_OBJECT:
                    StormSettings.SmallObjDensityMult = val;
                    break;
                case StormGlobalSetting.DENSITY_MEDIUM_OBJECT:
                    StormSettings.MediumObjDensityMult = val;
                    break;
                case StormGlobalSetting.DENSITY_LARGE_OBJECT:
                    StormSettings.LargeObjDensityMult = val;
                    break;
                case StormGlobalSetting.LOD_GLOBAL_MAX_UI:
                    StormSettings.GlobalLODBiasMaxUI = val;
                    break;
                case StormGlobalSetting.LOD_GRASS_MAX_UI:
                    StormSettings.GrassLODBiasMaxUI = val;
                    break;
                case StormGlobalSetting.LOD_DETAIL_MAX_UI:
                    StormSettings.DetailLODBiasMaxUI = val;
                    break;
                case StormGlobalSetting.LOD_PLANT_MAX_UI:
                    StormSettings.PlantLODBiasMaxUI = val;
                    break;
                case StormGlobalSetting.LOD_TREE_MAX_UI:
                    StormSettings.TreeLODBiasMaxUI = val;
                    break;
                case StormGlobalSetting.LOD_SMALL_OBJECT_MAX_UI:
                    StormSettings.SmallObjLODBiasMaxUI = val;
                    break;
                case StormGlobalSetting.LOD_MEDIUM_OBJECT_MAX_UI:
                    StormSettings.MediumObjLODBiasMaxUI = val;
                    break;
                case StormGlobalSetting.LOD_LARGE_OBJECT_MAX_UI:
                    StormSettings.LargeObjLODBiasMaxUI = val;
                    break;
                case StormGlobalSetting.DISTANCE_GRASS_MAX_UI:
                    StormSettings.GrassDistanceMaxUI = val;
                    break;
                case StormGlobalSetting.DISTANCE_DETAIL_MAX_UI:
                    StormSettings.DetailDistanceMaxUI = val;
                    break;
                case StormGlobalSetting.DISTANCE_PLANT_MAX_UI:
                    StormSettings.PlantDistanceMaxUI = val;
                    break;
                case StormGlobalSetting.DISTANCE_TREE_MAX_UI:
                    StormSettings.TreeDistanceMaxUI = val;
                    break;
                case StormGlobalSetting.DISTANCE_SMALL_OBJECT_MAX_UI:
                    StormSettings.SmallObjDistanceMaxUI = val;
                    break;
                case StormGlobalSetting.DISTANCE_MEDIUM_OBJECT_MAX_UI:
                    StormSettings.MediumObjDistanceMaxUI = val;
                    break;
                case StormGlobalSetting.DISTANCE_LARGE_OBJECT_MAX_UI:
                    StormSettings.LargeObjDistanceMaxUI = val;
                    break;
                case StormGlobalSetting.DISTANCE_COLLIDERS_MAX_UI:
                    StormSettings.ColliderDistanceMaxUI = val;
                    break;
                case StormGlobalSetting.LODMIX_GRASS_MAX_UI:
                    StormSettings.GrassLODMixMultMaxUI = val;
                    break;
                case StormGlobalSetting.LODMIX_DETAIL_MAX_UI:
                    StormSettings.DetailLODMixMultMaxUI = val;
                    break;
                case StormGlobalSetting.LODMIX_PLANT_MAX_UI:
                    StormSettings.PlantLODMixMultMaxUI = val;
                    break;
                case StormGlobalSetting.LODMIX_TREE_MAX_UI:
                    StormSettings.TreeLODMixMultMaxUI = val;
                    break;
                case StormGlobalSetting.LODMIX_SMALL_OBJECT_MAX_UI:
                    StormSettings.SmallObjLODMixMultMaxUI = val;
                    break;
                case StormGlobalSetting.LODMIX_MEDIUM_OBJECT_MAX_UI:
                    StormSettings.MediumObjLODMixMultMaxUI = val;
                    break;
                case StormGlobalSetting.LODMIX_LARGE_OBJECT_MAX_UI:
                    StormSettings.LargeObjLODMixMultMaxUI = val;
                    break;
                case StormGlobalSetting.DENSITYFADE_GRASS_MAX_UI:
                    StormSettings.GrassDensityFadeMultMaxUI = val;
                    break;
                case StormGlobalSetting.DENSITYFADE_DETAIL_MAX_UI:
                    StormSettings.DetailDensityFadeMultMaxUI = val;
                    break;
                case StormGlobalSetting.DENSITYFADE_PLANT_MAX_UI:
                    StormSettings.PlantDensityFadeMultMaxUI = val;
                    break;
                case StormGlobalSetting.DENSITYFADE_TREE_MAX_UI:
                    StormSettings.TreeDensityFadeMultMaxUI = val;
                    break;
                case StormGlobalSetting.DENSITYFADE_SMALL_OBJECT_MAX_UI:
                    StormSettings.SmallObjDensityFadeMultMaxUI = val;
                    break;
                case StormGlobalSetting.DENSITYFADE_MEDIUM_OBJECT_MAX_UI:
                    StormSettings.MediumObjDensityFadeMultMaxUI = val;
                    break;
                case StormGlobalSetting.DENSITYFADE_LARGE_OBJECT_MAX_UI:
                    StormSettings.LargeObjDensityFadeMultMaxUI = val;
                    break;
                case StormGlobalSetting.SCALEOUT_GRASS_MAX_UI:
                    StormSettings.GrassScaleoutMultMaxUI = val;
                    break;
                case StormGlobalSetting.SCALEOUT_DETAIL_MAX_UI:
                    StormSettings.DetailScaleoutMultMaxUI = val;
                    break;
                case StormGlobalSetting.SCALEOUT_PLANT_MAX_UI:
                    StormSettings.PlantScaleoutMultMaxUI = val;
                    break;
                case StormGlobalSetting.SCALEOUT_TREE_MAX_UI:
                    StormSettings.TreeScaleoutMultMaxUI = val;
                    break;
                case StormGlobalSetting.SCALEOUT_SMALL_OBJECT_MAX_UI:
                    StormSettings.SmallObjScaleoutMultMaxUI = val;
                    break;
                case StormGlobalSetting.SCALEOUT_MEDIUM_OBJECT_MAX_UI:
                    StormSettings.MediumObjScaleoutMultMaxUI = val;
                    break;
                case StormGlobalSetting.SCALEOUT_LARGE_OBJECT_MAX_UI:
                    StormSettings.LargeObjScaleoutMultMaxUI = val;
                    break;
                case StormGlobalSetting.DENSITY_GRASS_MAX_UI:
                    StormSettings.GrassDensityMultMaxUI = val;
                    break;
                case StormGlobalSetting.DENSITY_DETAIL_MAX_UI:
                    StormSettings.DetailDensityMultMaxUI = val;
                    break;
                case StormGlobalSetting.DENSITY_PLANT_MAX_UI:
                    StormSettings.PlantDensityMultMaxUI = val;
                    break;
                case StormGlobalSetting.DENSITY_TREE_MAX_UI:
                    StormSettings.TreeDensityMultMaxUI = val;
                    break;
                case StormGlobalSetting.DENSITY_SMALL_OBJECT_MAX_UI:
                    StormSettings.SmallObjDensityMultMaxUI = val;
                    break;
                case StormGlobalSetting.DENSITY_MEDIUM_OBJECT_MAX_UI:
                    StormSettings.MediumObjDensityMultMaxUI = val;
                    break;
                case StormGlobalSetting.DENSITY_LARGE_OBJECT_MAX_UI:
                    StormSettings.LargeObjDensityMultMaxUI = val;
                    break;
                case StormGlobalSetting.PERFORMANCE_UPDATE_RATE:
                    UIStormPerformanceDisplay perf = transform.root.GetComponentInChildren<UIStormPerformanceDisplay>(true);
                    if (perf != null)
                        perf.UpdateInterval = val;
                    UIStormMemoryDisplay mem = transform.root.GetComponentInChildren<UIStormMemoryDisplay>(true);
                    if (mem != null)
                        mem.UpdateInterval = val;
                    break;
            }
            SetCameraLayerCulling(val);
            m_isSettingValues = true;
            OnChanged?.Invoke();
            OnValueChanged?.Invoke(val);
            m_isSettingValues = false;
        }

        public void Save()
        {
            PlayerPrefs.SetFloat("STORM_" + m_setting.ToString(), GetValue());
            PlayerPrefs.Save();
        }
        public void Load()
        {
            float val = PlayerPrefs.GetFloat("STORM_" + m_setting.ToString(), GetValue());
            SetValueInternal(val);
        }

        private void SetCameraLayerCulling(float val)
        {
            if (m_layerIndex > -1)
            {
                float[] cullDistances = Camera.main.layerCullDistances;
                cullDistances[m_layerIndex] = val;
                Camera.main.layerCullDistances = cullDistances;
            }
        }
    }
}