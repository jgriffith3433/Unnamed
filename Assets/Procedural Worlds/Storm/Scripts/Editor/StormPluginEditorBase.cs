using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ProceduralWorlds.Storm.StormEditor
{
    public abstract class StormPluginEditorBase : EditorWindow
    {
        public abstract GUIContent Label { get; }
        public abstract bool IsEmbedded { get; set; }
        public bool IsWindowOpened(Type type) => WindowInstance.TryGetValue(type, out var win) && win != null;

        public System.Action OnOpen;

        public abstract void OnGUI();
        public abstract void OpenWindow();
        public abstract void HideWindow();
        public virtual void SetEmbeddedInstance(Type type, StormPluginEditorBase window)
        {
            if (EmbeddedInstance.TryGetValue(type, out StormPluginEditorBase win) && win != null)
            {
                win.Show();
                win.Close();
            }
            EmbeddedInstance[type] = window;
        }
        public static Dictionary<Type, StormPluginEditorBase> WindowInstance = new Dictionary<Type, StormPluginEditorBase>();
        public static Dictionary<Type, StormPluginEditorBase> EmbeddedInstance = new Dictionary<Type, StormPluginEditorBase>();
    }
}