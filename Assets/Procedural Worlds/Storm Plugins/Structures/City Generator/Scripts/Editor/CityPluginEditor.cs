using ProceduralWorlds.Storm.StormEditor;
using ProceduralWorlds.Storm.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class CityPluginEditor : StormPluginEditorBase
{
    public override GUIContent Label => new GUIContent("City Generator","Generates a road network and plots - the road network can then generate Roads with GeNa, and the plots are being populated by a building generator to create an entire city.");
    bool m_isEmbedded;
    public override bool IsEmbedded
    {
        get => m_isEmbedded;
        set => m_isEmbedded = value;
    }

    public override void OnGUI()
    {
        
        if (GUILayout.Button("Add City Generator Tool to Scene"))
        {
          StormUtils.AddPrefabAsTool("Example City.prefab");
        }

        //if (GUILayout.Button("Open Building Generator Config Builder"))
        //{
        //    var window = GetWindow<StructureCreationWindow>(false, "Generator Config Builder");
        //    Vector2 initialSize = new Vector2(600f, 450f);
        //    window.position = new Rect(new Vector2((Screen.currentResolution.width - initialSize.x) / 2f, (Screen.currentResolution.height - initialSize.y) / 2f), initialSize);
        //    window.Show();
        //}

        if (GUILayout.Button("Open City Generator Documentation"))
        {
            StormUtils.OpenFileInBrowser("Storm Plugin - City Generator - User Guide.pdf");
        }
        

    }

    public static void ShowWindow()
    {
        /*
        if (EmbeddedInstance.TryGetValue(typeof(CityPluginEditor), out var win) && win != null)
            win.OnOpen?.Invoke();

        if (!WindowInstance.TryGetValue(typeof(CityPluginEditor), out win) || win == null)
        {
            win = GetWindow<CityPluginEditor>(false, "Storm City Generator");
            while (win.IsEmbedded)
            {
                win.Close();
                win = GetWindow<CityPluginEditor>(false, "Storm City Generator");
            }
        }
        Vector2 initialSize = new Vector2(600f, 450f);
        win.position = new Rect(new Vector2((Screen.currentResolution.width - initialSize.x) / 2f, (Screen.currentResolution.height - initialSize.y) / 2f), initialSize);
        WindowInstance[typeof(CityPluginEditor)] = win;
        win.Show();
        */
    }
    public override void OpenWindow()
    {
        ShowWindow();
    }
    public override void HideWindow()
    {
        /*
        if (WindowInstance.TryGetValue(typeof(CityPluginEditor), out var win) && win != null)
            win.Close();
        WindowInstance.Remove(typeof(CityPluginEditor));
        */
    }
    private void OnDestroy()
    {
        if (WindowInstance.TryGetValue(typeof(CityPluginEditor), out var win) && win == this)
            WindowInstance.Remove(typeof(CityPluginEditor));
        if (EmbeddedInstance.TryGetValue(typeof(CityPluginEditor), out win) && win == this)
            EmbeddedInstance.Remove(typeof(CityPluginEditor));
    }
}
