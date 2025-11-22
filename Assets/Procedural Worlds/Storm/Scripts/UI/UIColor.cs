using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/// Copyright 2024 Procedural Worlds Ltd
/// Written by Jonathan Williams as part of Storm

namespace ProceduralWorlds.Storm.UI
{
    public class UIColor : MonoBehaviour
    {
        static string SourcePrefix = "STORM_COLOR_";
        public string SourceName = "";
        public Color DefaultColor = Color.white;
        public bool DefaultSetAlpha = false;
        public Graphic[] Items = new Graphic[0];

        public UnityEvent<float> OnLoadAlpha;
        public UnityEvent<Color> OnLoadColor;

        private void Start()
        {
            if (!LoadColor() && !LoadAlpha())
            {
                SetColor(DefaultColor, DefaultSetAlpha);
            }
        }

        bool LoadColor()
        {
            if (GetColor(SourceName, out Color c, out bool hasAlpha))
            {
                SetColor(c, hasAlpha);
                OnLoadColor?.Invoke(c);
                if (hasAlpha)
                    OnLoadAlpha?.Invoke(c.a);
                return true;
            }
            return false;
        }
        bool LoadAlpha()
        {
            if (GetAlpha(SourceName, out float A))
            {
                SetAlpha(A);
                OnLoadAlpha?.Invoke(A);
                return true;
            }
            return false;
        }

        public void SetColor(Color color, bool setAlpha)
        {
            foreach (var item in Items)
            {
                if (item != null)
                {
                    if (!setAlpha)
                        color.a = item.color.a;
                    item.color = color;
                    item.SetMaterialDirty();
                }
            }
        }
        public void SetAlpha(float alpha)
        {
            foreach (var item in Items)
            {
                if (item != null)
                {
                    Color c = item.color;
                    c.a = alpha;
                    item.color = c;
                    item.SetMaterialDirty();
                }
            }
        }

        public static void SetColor(RectTransform parent, string Source, Color color, bool setAlpha)
        {
            if (!string.IsNullOrEmpty(Source))
            {
                if (setAlpha)
                {
                    PlayerPrefs.SetString(SourcePrefix + Source + "_C", color.r + "," + color.g + "," + color.b + "," + color.a);
                    PlayerPrefs.SetFloat(SourcePrefix + Source + "_A", color.a);
                }
                else
                    PlayerPrefs.SetString(SourcePrefix + Source + "_C", color.r + "," + color.g + "," + color.b);
                PlayerPrefs.Save();
            }
            foreach (UIColor c in parent.GetComponentsInChildren<UIColor>(true))
            {
                if (c.SourceName == Source)
                    c.SetColor(color, setAlpha);
            }
        }
        public static void SetAlpha(RectTransform parent, string Source, float alpha)
        {
            if (!string.IsNullOrEmpty(Source))
            {
                PlayerPrefs.SetFloat(SourcePrefix + Source + "_A", alpha);
                PlayerPrefs.Save();
            }
            foreach (UIColor c in parent.GetComponentsInChildren<UIColor>(true))
            {
                if (c.SourceName == Source)
                    c.SetAlpha(alpha);
            }
        }
        public static bool GetColor(string Source, out Color color, out bool hasAlpha)
        {
            color = Color.white;
            hasAlpha = false;
            if (!string.IsNullOrEmpty(Source) && PlayerPrefs.HasKey(SourcePrefix + Source + "_C"))
            {
                string str = PlayerPrefs.GetString(SourcePrefix + Source + "_C");
                string[] parts = str.Split(",");
                if (parts.Length == 3)
                {
                    Color c;
                    if (!float.TryParse(parts[0], out c.r))
                    {
                        Debug.LogWarning("Invalid color string found for Window Color '" + Source + "' (" + str + ")\nUnable to parse Red value");
                        return false;
                    }
                    if (!float.TryParse(parts[1], out c.g))
                    {
                        Debug.LogWarning("Invalid color string found for Window Color '" + Source + "' (" + str + ")\nUnable to parse Green value");
                        return false;
                    }
                    if (!float.TryParse(parts[2], out c.b))
                    {
                        Debug.LogWarning("Invalid color string found for Window Color '" + Source + "' (" + str + ")\nUnable to parse Blue value");
                        return false;
                    }
                    if (PlayerPrefs.HasKey(SourcePrefix + Source + "_A"))
                    {
                        c.a = PlayerPrefs.GetFloat(SourcePrefix + Source + "_A");
                        hasAlpha = true;
                    }
                    else
                        c.a = 1;
                    color = c;
                    return true;
                }
                else if (parts.Length == 4)
                {
                    Color c;
                    if (!float.TryParse(parts[0], out c.r))
                    {
                        Debug.LogWarning("Invalid color string found for Window Color '" + Source + "' (" + str + ")\nUnable to parse Red value");
                        return false;
                    }
                    if (!float.TryParse(parts[1], out c.g))
                    {
                        Debug.LogWarning("Invalid color string found for Window Color '" + Source + "' (" + str + ")\nUnable to parse Green value");
                        return false;
                    }
                    if (!float.TryParse(parts[2], out c.b))
                    {
                        Debug.LogWarning("Invalid color string found for Window Color '" + Source + "' (" + str + ")\nUnable to parse Blue value");
                        return false;
                    }
                    if (!float.TryParse(parts[3], out c.a))
                    {
                        Debug.LogWarning("Invalid color string found for Window Color '" + Source + "' (" + str + ")\nUnable to parse Alpha value");
                        return false;
                    }
                    color = c;
                    hasAlpha = true;
                    return true;

                }
                else
                    Debug.LogWarning("Invalid color string found for Window Color '" + Source + "' (" + str + ")\nExpected 3 or 4 parts. Found " + parts.Length);
            }
            return false;
        }
        public static bool GetAlpha(string Source, out float alpha)
        {
            if (!string.IsNullOrEmpty(Source) && PlayerPrefs.HasKey(SourcePrefix + Source + "_A"))
            {
                alpha = PlayerPrefs.GetFloat(SourcePrefix + Source + "_A");
                return true;
            }
            alpha = 1;
            return false;
        }
    }
}