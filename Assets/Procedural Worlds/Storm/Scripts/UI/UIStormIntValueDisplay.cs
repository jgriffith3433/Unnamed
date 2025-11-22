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
    [AddComponentMenu("Procedural Worlds/Storm/UI/Integer Value Display")]
    public class UIStormIntValueDisplay : MonoBehaviour
    {
        [Serializable]
        public enum StormGlobalSetting : short
        {
            CUSTOM = -1,
            LOADING_MAX_COUNT,
            LOADING_MASKS_PER_FRAME,
        }

        public Slider m_slider;
        public TMP_InputField m_input;
        [SerializeField]
        public StormGlobalSetting m_setting = StormGlobalSetting.LOADING_MAX_COUNT;

        private string m_settingFormat = "N";
        private bool m_isSettingValues = false;
        private int m_layerIndex = -1;

        public UnityEvent<float> OnValueChanged;
        public Action OnChanged;

        private void OnEnable()
        {
            int val = GetValue();
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
        }

        public void UpdateValues()
        {
            int val = GetValue();
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
            OnValueChanged?.Invoke(val);
        }
        public void SetValue(float val)
        {
            if (m_isSettingValues || !Application.isPlaying)
                return;
            int v = (int)val;
            m_isSettingValues = true;
            SetValueInternal(v);
            Save();
            if (m_input != null)
                m_input.text = v.ToString(m_settingFormat);
            if (m_slider != null)
                m_slider.value = v;
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
            int v = (int)val;
            m_isSettingValues = true;
            SetValueInternal(v);
            Save();
            if (m_input != null)
                m_input.text = v.ToString(m_settingFormat);
            m_isSettingValues = false;
        }
        private void OnTextValueChanged(string txt)
        {
            if (m_isSettingValues || !Application.isPlaying)
                return;
            if (int.TryParse(txt, out int val))
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
                    case StormGlobalSetting.LOADING_MAX_COUNT:
                        m_settingFormat = "N";
                        minValue = 1;
                        maxValue = 50;
                        break;
                    case StormGlobalSetting.LOADING_MASKS_PER_FRAME:
                        m_settingFormat = "N";
                        minValue = 1;
                        maxValue = 100;
                        break;
                }
                if (m_slider != null)
                {
                    m_slider.minValue = minValue;
                    m_slider.maxValue = maxValue;
                }
            }
        }
        private int GetValue()
        {
            switch (m_setting)
            {
                case StormGlobalSetting.LOADING_MAX_COUNT:
                    return StormSettings.MaxSimultaneousLoad;
                case StormGlobalSetting.LOADING_MASKS_PER_FRAME:
                    return StormSettings.MaxLoadMasksPerFrame;
                case StormGlobalSetting.CUSTOM:
                    if (m_slider != null)
                        return (int)m_slider.value;
                    else if (m_input != null && int.TryParse(m_input.text, out int val))
                        return val;
                    return 0;
                default:
                    return 0;
            }
        }
        private void SetValueInternal(int val)
        {
            switch (m_setting)
            {
                case StormGlobalSetting.LOADING_MAX_COUNT:
                    StormSettings.MaxSimultaneousLoad = val;
                    break;
                case StormGlobalSetting.LOADING_MASKS_PER_FRAME:
                    StormSettings.MaxLoadMasksPerFrame = val;
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
            PlayerPrefs.SetInt("STORM_" + m_setting.ToString(), GetValue());
            PlayerPrefs.Save();
        }
        public void Load()
        {
            int val = PlayerPrefs.GetInt("STORM_" + m_setting.ToString(), GetValue());
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