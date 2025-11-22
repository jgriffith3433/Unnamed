using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;

/// Copyright 2024 Procedural Worlds Ltd
/// Written by Jonathan Williams as part of Storm

namespace ProceduralWorlds.Storm.UI
{
    public class UIThemes : MonoBehaviour
    {
        [Serializable]
        public struct ColorInfo
        {
            public string Source;
            public Color Color;
            public bool SetAlpha;

            public void Set(RectTransform t)
            {
                UIColor.SetColor(t, Source, Color, SetAlpha);
            }
        }
        [Serializable]
        public struct ThemeInfo
        {
            public string Name;
            public ColorInfo[] Colors;
        }

        static string ThemeSetPrefix = "STORM_THEME_";
        public string ThemeSetName = "";
        public RectTransform ParentTransform;
        public ThemeInfo[] Themes;
        public UnityEvent<int> OnSetTheme;
        public Dropdown Dropdown;
        public TMP_Dropdown TMPDropdown;
        bool isSettingValues = false;

        [ExecuteAlways]
        public void Start()
        {
            if (ParentTransform == null)
                ParentTransform = transform as RectTransform;
            int themeIdx = 0;
            if (!string.IsNullOrEmpty(ThemeSetName) && PlayerPrefs.HasKey(ThemeSetPrefix + ThemeSetName))
            {
                themeIdx = PlayerPrefs.GetInt(ThemeSetPrefix + ThemeSetName, 0);
                SetTheme(themeIdx);
            }
            isSettingValues = true;
            if (Dropdown != null)
            {
                List<string> options = new List<string>(Themes.Length);
                for (int t = 0; t < Themes.Length; ++t)
                    options.Add(Themes[t].Name);
                Dropdown.ClearOptions();
                Dropdown.AddOptions(options);
                Dropdown.SetValueWithoutNotify(themeIdx);
                Dropdown.onValueChanged.AddListener(SetTheme);
            }
            if (TMPDropdown != null)
            {
                List<string> options = new List<string>(Themes.Length);
                for (int t = 0; t < Themes.Length; ++t)
                    options.Add(Themes[t].Name);
                TMPDropdown.ClearOptions();
                TMPDropdown.AddOptions(options);
                TMPDropdown.SetValueWithoutNotify(themeIdx);
                TMPDropdown.onValueChanged.AddListener(SetTheme);
            }
            isSettingValues = false;
        }

        public void SetTheme(int index)
        {
            if (isSettingValues || index < 0 || index >= Themes.Length)
                return;
            if (!string.IsNullOrEmpty(ThemeSetName))
            {
                PlayerPrefs.SetInt(ThemeSetPrefix + ThemeSetName, index);
                PlayerPrefs.Save();
            }
            isSettingValues = true;
            for(int c = 0; c < Themes[index].Colors.Length; ++c)
            {
                Themes[index].Colors[c].Set(ParentTransform);
            }
            OnSetTheme?.Invoke(index);
            isSettingValues = false;
        }
    }
}