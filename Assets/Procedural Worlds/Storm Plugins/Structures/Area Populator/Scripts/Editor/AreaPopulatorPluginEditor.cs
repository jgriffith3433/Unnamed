using ProceduralWorlds.Storm.StormEditor;
using ProceduralWorlds.Storm.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class AreaPopulatorPluginEditor : StormPluginEditorBase
{
    public override GUIContent Label => new GUIContent("Area Populator","Can procedurally spawn content while taking the available space and other objects into account. Can populate rooms, backyards, alleys, etc.");
    bool m_isEmbedded;
    public override bool IsEmbedded
    {
        get => m_isEmbedded;
        set => m_isEmbedded = value;
    }

    public override void OnGUI()
    {
        if (GUILayout.Button("Add Volume Decorator Tool to Scene"))
        {
          StormUtils.AddPrefabAsTool("Volume Decorator.prefab");
        }

        if (GUILayout.Button("Open Area Populator Config Builder"))
        {
            var window = GetWindow<StructureCreationWindow>(false, "Generator Config Builder");
            Vector2 initialSize = new Vector2(600f, 450f);
            window.position = new Rect(new Vector2((Screen.currentResolution.width - initialSize.x) / 2f, (Screen.currentResolution.height - initialSize.y) / 2f), initialSize);
            window.Show();
        }

        if (GUILayout.Button("Open Area Populator Documentation"))
        {
            StormUtils.OpenFileInBrowser("Storm Plugin - Area Populator - User Guide.pdf");
        }

    }

    public static void ShowWindow()
    {
        if (EmbeddedInstance.TryGetValue(typeof(AreaPopulatorPluginEditor), out var win) && win != null)
            win.OnOpen?.Invoke();

        if (!WindowInstance.TryGetValue(typeof(AreaPopulatorPluginEditor), out win) || win == null)
        {
            win = GetWindow<AreaPopulatorPluginEditor>(false, "Storm Area Populator");
            while (win.IsEmbedded)
            {
                win.Close();
                win = GetWindow<AreaPopulatorPluginEditor>(false, "Storm Area Populator");
            }
        }
        Vector2 initialSize = new Vector2(600f, 450f);
        win.position = new Rect(new Vector2((Screen.currentResolution.width - initialSize.x) / 2f, (Screen.currentResolution.height - initialSize.y) / 2f), initialSize);
        WindowInstance[typeof(AreaPopulatorPluginEditor)] = win;
        win.Show();
    }
    public override void OpenWindow()
    {
        ShowWindow();
    }
    public override void HideWindow()
    {
        if (WindowInstance.TryGetValue(typeof(AreaPopulatorPluginEditor), out var win) && win != null)
            win.Close();
        WindowInstance.Remove(typeof(AreaPopulatorPluginEditor));
    }
    private void OnDestroy()
    {
        if (WindowInstance.TryGetValue(typeof(AreaPopulatorPluginEditor), out var win) && win == this)
            WindowInstance.Remove(typeof(AreaPopulatorPluginEditor));
        if (EmbeddedInstance.TryGetValue(typeof(AreaPopulatorPluginEditor), out win) && win == this)
            EmbeddedInstance.Remove(typeof(AreaPopulatorPluginEditor));
    }
}
