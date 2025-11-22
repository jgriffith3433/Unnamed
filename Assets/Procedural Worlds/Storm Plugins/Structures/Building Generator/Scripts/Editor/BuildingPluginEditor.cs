using ProceduralWorlds.Storm.StormEditor;
using ProceduralWorlds.Storm.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class BuildingPluginEditor : StormPluginEditorBase
{
    public override GUIContent Label => new GUIContent("Building Generator","Allows the procedural generation of multi-story buildings out of modular prefabs. The generated buildings will be generated to fill an arbitary polygon shape.");
    bool m_isEmbedded;
    public override bool IsEmbedded
    {
        get => m_isEmbedded;
        set => m_isEmbedded = value;
    }

    public override void OnGUI()
    {
        if (GUILayout.Button("Add Building Generator Tool to Scene"))
        {
          StormUtils.AddPrefabAsTool("Building Generator.prefab");
        }

        if (GUILayout.Button("Open Building Generator Config Builder"))
        {
            var window = GetWindow<StructureCreationWindow>(false, "Generator Config Builder");
            Vector2 initialSize = new Vector2(600f, 450f);
            window.position = new Rect(new Vector2((Screen.currentResolution.width - initialSize.x) / 2f, (Screen.currentResolution.height - initialSize.y) / 2f), initialSize);
            window.Show();
        }

        if (GUILayout.Button("Open Building Generator Documentation"))
        {
            StormUtils.OpenFileInBrowser("Storm Plugin - Building Generator - User Guide.pdf");
        }

    }

    public static void ShowWindow()
    {
        if (EmbeddedInstance.TryGetValue(typeof(BuildingPluginEditor), out var win) && win != null)
            win.OnOpen?.Invoke();

        if (!WindowInstance.TryGetValue(typeof(BuildingPluginEditor), out win) || win == null)
        {
            win = GetWindow<BuildingPluginEditor>(false, "Storm Building Generator");
            while (win.IsEmbedded)
            {
                win.Close();
                win = GetWindow<BuildingPluginEditor>(false, "Storm Building Generator");
            }
        }
        Vector2 initialSize = new Vector2(600f, 450f);
        win.position = new Rect(new Vector2((Screen.currentResolution.width - initialSize.x) / 2f, (Screen.currentResolution.height - initialSize.y) / 2f), initialSize);
        WindowInstance[typeof(BuildingPluginEditor)] = win;
        win.Show();
    }
    public override void OpenWindow()
    {
        ShowWindow();
    }
    public override void HideWindow()
    {
        if (WindowInstance.TryGetValue(typeof(BuildingPluginEditor), out var win) && win != null)
            win.Close();
        WindowInstance.Remove(typeof(BuildingPluginEditor));
    }
    private void OnDestroy()
    {
        if (WindowInstance.TryGetValue(typeof(BuildingPluginEditor), out var win) && win == this)
            WindowInstance.Remove(typeof(BuildingPluginEditor));
        if (EmbeddedInstance.TryGetValue(typeof(BuildingPluginEditor), out win) && win == this)
            EmbeddedInstance.Remove(typeof(BuildingPluginEditor));
    }
}
