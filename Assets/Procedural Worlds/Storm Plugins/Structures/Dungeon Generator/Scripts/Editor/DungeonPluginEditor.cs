using ProceduralWorlds.Storm.StormEditor;
using ProceduralWorlds.Storm.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class DungeonPluginEditor : StormPluginEditorBase
{
    public override GUIContent Label => new GUIContent("Dungeon Generator","Generates an underground dungeon or cave structure with varied rooms on multiple levels.");
    bool m_isEmbedded;
    public override bool IsEmbedded
    {
        get => m_isEmbedded;
        set => m_isEmbedded = value;
    }

    public override void OnGUI()
    {
        if (GUILayout.Button("Add Dungeon Generator Tool to Scene"))
        {
          StormUtils.AddPrefabAsTool("Dungeon Generator.prefab");
        }

        //if (GUILayout.Button("Open Building Generator Config Builder"))
        //{
        //    var window = GetWindow<StructureCreationWindow>(false, "Generator Config Builder");
        //    Vector2 initialSize = new Vector2(600f, 450f);
        //    window.position = new Rect(new Vector2((Screen.currentResolution.width - initialSize.x) / 2f, (Screen.currentResolution.height - initialSize.y) / 2f), initialSize);
        //    window.Show();
        //}

        if (GUILayout.Button("Open Dungeon Generator Documentation"))
        {
            StormUtils.OpenFileInBrowser("Storm Plugin - Dungeon Generator - User Guide.pdf");
        }

    }

    public static void ShowWindow()
    {
        if (EmbeddedInstance.TryGetValue(typeof(DungeonPluginEditor), out var win) && win != null)
            win.OnOpen?.Invoke();

        if (!WindowInstance.TryGetValue(typeof(DungeonPluginEditor), out win) || win == null)
        {
            win = GetWindow<DungeonPluginEditor>(false, "Storm Dungeon Generator");
            while (win.IsEmbedded)
            {
                win.Close();
                win = GetWindow<DungeonPluginEditor>(false, "Storm Dungeon Generator");
            }
        }
        Vector2 initialSize = new Vector2(600f, 450f);
        win.position = new Rect(new Vector2((Screen.currentResolution.width - initialSize.x) / 2f, (Screen.currentResolution.height - initialSize.y) / 2f), initialSize);
        WindowInstance[typeof(DungeonPluginEditor)] = win;
        win.Show();
    }
    public override void OpenWindow()
    {
        ShowWindow();
    }
    public override void HideWindow()
    {
        if (WindowInstance.TryGetValue(typeof(DungeonPluginEditor), out var win) && win != null)
            win.Close();
        WindowInstance.Remove(typeof(DungeonPluginEditor));
    }
    private void OnDestroy()
    {
        if (WindowInstance.TryGetValue(typeof(DungeonPluginEditor), out var win) && win == this)
            WindowInstance.Remove(typeof(DungeonPluginEditor));
        if (EmbeddedInstance.TryGetValue(typeof(DungeonPluginEditor), out win) && win == this)
            EmbeddedInstance.Remove(typeof(DungeonPluginEditor));
    }
}
