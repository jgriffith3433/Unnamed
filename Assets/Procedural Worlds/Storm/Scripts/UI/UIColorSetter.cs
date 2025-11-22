using UnityEngine;

/// Copyright 2024 Procedural Worlds Ltd
/// Written by Jonathan Williams as part of Storm

namespace ProceduralWorlds.Storm.UI
{
    public class UIColorSetter : MonoBehaviour
    {
        public RectTransform WindowParent = null;
        public string SourceName = "";
        [ExecuteAlways]
        public void Awake()
        {
            if (WindowParent == null)
                WindowParent = transform as RectTransform;
        }
        public void SetColor(Color c)
        {
            UIColor.SetColor(WindowParent, SourceName, c, false);
        }
        public void SetColorAlpha(Color c)
        {
            UIColor.SetColor(WindowParent, SourceName, c, true);
        }
        public void SetAlpha(float a)
        {
            UIColor.SetAlpha(WindowParent, SourceName, a);
        }
    }
}