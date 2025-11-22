using ProceduralWorlds.Storm.Core;
using UnityEngine;
using UnityEngine.UI;
using System;

/// Copyright 2024 Procedural Worlds Ltd
/// Written by Jonathan Williams as part of Storm

namespace ProceduralWorlds.Storm.UI
{
    [AddComponentMenu("Procedural Worlds/Storm/UI/Bool Value Display")]
    public class UIStormBoolValueDisplay : MonoBehaviour
    {
        public enum StormBoolSetting
        {
            CUSTOM = -1,
            AUTO_LOAD,
            SHOW_LOD_DEBUG,
            SHOW_FPS,
            SHOW_PERFORMANCE,
        }

        public Toggle m_toggle;
        public StormBoolSetting m_setting = StormBoolSetting.AUTO_LOAD;

        private bool m_isSettingValues = false;

        public Action OnValueChanged;

        private void Start()
        {
            bool val = GetValue();
            Init();
            m_isSettingValues = true;
            if (m_toggle != null)
            {
                m_toggle.isOn = val;
                m_toggle.onValueChanged.AddListener(OnToggleChanged);
            }
            m_isSettingValues = false;
        }

        public void UpdateValues()
        {
            bool val = GetValue();
            m_isSettingValues = true;
            if (m_toggle != null)
            {
                m_toggle.isOn = val;
            }
            m_isSettingValues = false;
        }

        private void OnToggleChanged(bool val)
        {
            if (m_isSettingValues || !Application.isPlaying)
                return;
            m_isSettingValues = true;
            SetValue(val);
            Save();
            m_isSettingValues = false;
        }

        private void Init()
        {
        }
        private bool GetValue()
        {
            switch (m_setting)
            {
                case StormBoolSetting.AUTO_LOAD:
                case StormBoolSetting.SHOW_FPS:
                    return PlayerPrefs.GetInt("STORM_" + m_setting.ToString(), 0) == 1;
                case StormBoolSetting.SHOW_LOD_DEBUG:
                    return StormWorld.Instance == null ? false : StormWorld.Instance.DebugSettings.ShowLODs;
                case StormBoolSetting.CUSTOM:
                    if (m_toggle != null)
                        return m_toggle.isOn;
                    return false;
                default:
                    return false;
            }
        }
        private void SetValue(bool val)
        {
            switch (m_setting)
            {
                case StormBoolSetting.AUTO_LOAD:
                case StormBoolSetting.SHOW_FPS:
                case StormBoolSetting.SHOW_PERFORMANCE:
                    PlayerPrefs.SetInt("STORM_" + m_setting.ToString(), val ? 1 : 0);
                    PlayerPrefs.Save();
                    break;
                case StormBoolSetting.SHOW_LOD_DEBUG:
                    if (StormWorld.Instance != null)
                        StormWorld.Instance.DebugSettings.ShowLODs = val;
                    break;
            }
            OnValueChanged?.Invoke();
        }

        public void Save()
        {
            if (m_setting == StormBoolSetting.SHOW_LOD_DEBUG)
                return;
            PlayerPrefs.SetInt("STORM_" + m_setting.ToString(), GetValue() ? 1 : 0);
            PlayerPrefs.Save();
        }
        public void Load()
        {
            if (m_setting == StormBoolSetting.SHOW_LOD_DEBUG)
                return;
            bool val = PlayerPrefs.GetInt("STORM_" + m_setting.ToString(), GetValue() ? 1 : 0) == 1;
            SetValue(val);
        }
    }
}