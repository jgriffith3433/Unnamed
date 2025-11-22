using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using ProceduralWorlds.Storm.Utils;
using ProceduralWorlds.Storm.Core;

/// Copyright 2023 Procedural Worlds Ltd
/// Written by Jonathan Williams as part of Storm

namespace ProceduralWorlds.Storm.UI
{
    [ExecuteAlways]
    [AddComponentMenu("Procedural Worlds/Storm/UI/Save Load Settings")]
    public class UISaveLoadSettings : MonoBehaviour
    {
        public UIStormFloatValueDisplay[] ValueDisplays;
        public UIStormBoolValueDisplay[] boolDisplays;
        public TMPro.TMP_Dropdown PresetDropdown = null;
        public StormQualitySettings[] Presets;
        public UnityEngine.UI.Toggle AutoLoadToggle = null;
        public TMPro.TMP_Dropdown CullingDropdown = null;
        public GameObject CPUCullArea = null;
        public TMPro.TMP_Dropdown CPUCombineDropdown = null;

        public void Awake()
        {
            if (!gameObject.scene.IsValid())
                return;
            if (ValueDisplays == null || ValueDisplays.Length == 0)
                ValueDisplays = GetComponentsInChildren<UIStormFloatValueDisplay>(true);
            if (boolDisplays == null || boolDisplays.Length == 0)
                boolDisplays = GetComponentsInChildren<UIStormBoolValueDisplay>(true);
            bool autoLoad = PlayerPrefs.GetInt("STORM_AUTO_LOAD_CUSTOM", 0) == 1;
            if (AutoLoadToggle != null)
            {
                AutoLoadToggle.isOn = autoLoad;
                AutoLoadToggle.onValueChanged.RemoveListener(SetAutoLoad);
                AutoLoadToggle.onValueChanged.AddListener(SetAutoLoad);
            }
            if (PresetDropdown != null)
            {
                List<string> options = new List<string>();
                options.Add("Custom");
                for (int i = 0; i < Presets.Length; ++i)
                {
                    string name;
                    if (Presets[i] == null)
                        name = "NULL";
                    else
                        name = Presets[i].name.Replace("Custom", "").Replace("StormPreset", "").Replace("StormGlobalSettings", "").Trim();
                    options.Add("Preset " + name);
                }
                PresetDropdown.ClearOptions();
                PresetDropdown.AddOptions(options);
                PresetDropdown.value = 0;
                PresetDropdown.onValueChanged.RemoveListener(ChangedPreset);
                PresetDropdown.onValueChanged.AddListener(ChangedPreset);
            }
            if (CullingDropdown != null)
            {
                List<string> options = new List<string>(new string[] { "CPU", "GPU" });
                CullingDropdown.ClearOptions();
                CullingDropdown.AddOptions(options);
                CullingDropdown.value = StormSettings.CullType switch {
                    StormCullType.CPU => 0,
                    StormCullType.GPU => 1,
                    _ => 0,
                };
                CullingDropdown.onValueChanged.RemoveListener(ChangedCulling);
                CullingDropdown.onValueChanged.AddListener(ChangedCulling);
            }
            if (CPUCullArea != null)
            {
                CPUCullArea.SetActive(StormSettings.CullType == StormCullType.CPU);
            }
            if (CPUCombineDropdown != null)
            {
                List<string> options = new List<string>(new string[] { "Standard", "Simple", "NoCull" });
                CPUCombineDropdown.ClearOptions();
                CPUCombineDropdown.AddOptions(options);
                CPUCombineDropdown.value = StormSettings.CombineType switch {
                    StormCombineType.STANDARD => 0,
                    StormCombineType.SIMPLE => 1,
                    StormCombineType.NO_CULL => 2,
                    _ => 0,
                };
                CPUCombineDropdown.onValueChanged.RemoveListener(ChangedCombine);
                CPUCombineDropdown.onValueChanged.AddListener(ChangedCombine);
            }
            if (autoLoad)
                Load();
            foreach (UIStormFloatValueDisplay valueDisplay in ValueDisplays)
            {
                valueDisplay.UpdateValues();
                valueDisplay.OnChanged += OnChangedSetting;
            }
            foreach (UIStormBoolValueDisplay valueDisplay in boolDisplays)
            {
                valueDisplay.UpdateValues();
                valueDisplay.OnValueChanged += OnChangedSetting;
            }
            if (StormUtils.FindObjectsOfType<EventSystem>().Length > 1)
            {
                EventSystem myEventSystem = GetMyEventSystem();
                if (myEventSystem != null)
                    myEventSystem.gameObject.SetActive(false);
            }
        }

        private EventSystem GetMyEventSystem()
        {
            EventSystem ret = GetComponentInChildren<EventSystem>();
            Transform t = transform;
            if (ret != null)
            {
                while (t.parent != null)
                {
                    t = t.parent;
                    ret = GetComponentInChildren<EventSystem>();
                }
            }
            return ret;
        }

        public void SetAutoLoad(bool val)
        {
            if (val)
                Save();
            else
                Clear();
        }
        public void OnChangedSetting()
        {
            if (PresetDropdown != null)
            {
                PresetDropdown.value = 0;
            }
        }
        public void ChangedPreset(int index)
        {
            if (index < 1 || index > Presets.Length)
                return;
            StormQualitySettings preset = Presets[index - 1];
            if (preset == null)
                return;
            LoadFrom(preset);
        }
        public void ChangedCulling(int index)
        {
            if (index < 0 || index >= 2)
                return;
            StormSettings.CullType = index switch {
                0 => StormCullType.CPU,
                1 => StormCullType.GPU,
                _ => StormCullType.CPU,
            };
            if (CPUCullArea != null)
                CPUCullArea.SetActive(StormSettings.CullType == StormCullType.CPU);
            if (StormWorld.Instance != null)
                StormWorld.Instance.UpdateFeaturesetsForced();
        }
        public void ChangedCombine(int index)
        {
            if (index < 0 || index >= 3)
                return;
            StormSettings.CombineType = index switch {
                0 => StormCombineType.STANDARD,
                1 => StormCombineType.SIMPLE,
                2 => StormCombineType.NO_CULL,
                _ => StormCombineType.STANDARD,
            };
        }

        public void Save()
        {
            PlayerPrefs.SetInt("STORM_AUTO_LOAD_CUSTOM", 1);
            PlayerPrefs.Save();
            foreach (var display in ValueDisplays)
                display.Save();
            foreach (var display in boolDisplays)
                display.Save();
        }
        public void Clear()
        {
            PlayerPrefs.DeleteKey("STORM_AUTO_LOAD_CUSTOM");
            PlayerPrefs.Save();
        }

        public void Load()
        {
            foreach (var display in ValueDisplays)
                display.Load();
            foreach (var display in boolDisplays)
                display.Load();
        }
        public void LoadFrom(StormQualitySettings settings)
        {
            StormSettings.CopyFrom(settings);
            foreach (var valueDisplay in ValueDisplays)
                valueDisplay.UpdateValues();
            foreach (var valueDisplay in boolDisplays)
                valueDisplay.UpdateValues();
        }

        public void DoStormRefresh()
        {
            if (StormWorld.Instance != null)
                StormWorld.Instance.ForceFullRefresh();
        }
    }
}