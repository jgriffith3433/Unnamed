using UnityEditor;
using UnityEngine;
using ProceduralWorlds.Storm.Utils;
using ProceduralWorlds.Storm.Wizard;
using PWCommon5;
using System;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

/// Copyright 2023 Procedural Worlds Ltd
/// Written by Jonathan Williams as part of Storm

namespace ProceduralWorlds.Storm.StormEditor
{
    public static class StormEditorUtils
    {
        /// <summary>
        /// GUI that shows the clear world button
        /// Can only be called in Editor scripts or OnGUI
        /// </summary>
        public static void ClearWorldGUI(EditorUtils editorUtils)
        {
            if (editorUtils.Button("ClearWorld"))
            {
                int response = EditorUtility.DisplayDialogComplex("Clearing Storm World!", "You are about to clear the storm world from the scene. This will remove all the storm tiles and re-enable all the tracked transforms. Would you like to clear all the features too?", "Yes", "No Thanks", "Cancel");
                switch (response)
                {
                    case 0:
                    {
                        if (EditorUtility.DisplayDialog("Clearing Features!", "Are you sure you want to clear all features? This will delete them from your project and can not be undone.", "Yes", "No"))
                        {
                            if (EditorUtility.DisplayDialog("Remove Storm World!", "Would you like to remove storm world from the scene?", "Yes", "No"))
                            {
                                StormWizardUtils.ClearWorld(true, true, true);
                            }
                            else
                            {
                                StormWizardUtils.ClearWorld(true, true, false);
                            }
                        }
                        else
                        {
                            StormWizardUtils.ClearWorld(false, false, false);
                        }
                        break;
                    }
                    case 1:
                    {
                        if (EditorUtility.DisplayDialog("Remove Storm World!", "Would you like to remove storm world from the scene?", "Yes", "No"))
                        {
                            StormWizardUtils.ClearWorld(false, false, true);
                        }
                        else
                        {
                            StormWizardUtils.ClearWorld(false, false, false);
                        }
                        break;
                    }
                }
            }
        }
        /// <summary>
        /// Button that allows you to enable all the ingest objects that have been tracked
        /// </summary>
        /// <param name="editorUtils"></param>
        public static void ReEnableIngestedGUI(EditorUtils editorUtils)
        {
            if (editorUtils.Button("EnableAllIngestedGameObjects"))
            {
                StormTransformList[] tLists = StormUtils.FindObjectsOfType<StormTransformList>(true);
                foreach (StormTransformList tList in tLists)
                {
                    foreach (Transform t in tList.Transforms)
                        if (t != null)
                            t.gameObject.SetActive(true);
                }
            }
        }
        /// <summary>
        /// Draws the instance count and button GUI to clean up featuresets
        /// </summary>
        /// <param name="editorUtils"></param>
        /// <param name="instanceCount"></param>
        public static void CleanUpFeatureSetsGUI(EditorUtils editorUtils, StormEditorSettings editorSettings, bool helpEnabled)
        {
            int instanceCount = editorSettings.TransformInstanceCount;
            bool ignoreTRS = editorSettings.IgnoreTRSInstanceCount;
            bool disableTerrainFoliageRendering = editorSettings.DisableTerrainFoliageRendering;
            bool deleteEmptyTiles = editorSettings.DeleteEmptyStormTiles;

            EditorGUI.BeginChangeCheck();
            ignoreTRS = editorUtils.Toggle("IgnoreTRSInstances", ignoreTRS, helpEnabled);
            disableTerrainFoliageRendering = editorUtils.Toggle("DisableTerrainFoliageRendering", disableTerrainFoliageRendering, helpEnabled);
            deleteEmptyTiles = editorUtils.Toggle("DeleteEmptyStormTiles", deleteEmptyTiles, helpEnabled);
            instanceCount = editorUtils.IntField("CleanUpInstanceCount", instanceCount, helpEnabled);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(editorSettings, "Changed Instance Check Settings");
                editorSettings.TransformInstanceCount = instanceCount;
                editorSettings.IgnoreTRSInstanceCount = ignoreTRS;
                editorSettings.DeleteEmptyStormTiles = deleteEmptyTiles;
                editorSettings.DisableTerrainFoliageRendering = disableTerrainFoliageRendering;
            }

            if (editorUtils.Button("CleanUpFeatureSets"))
            {
                if (EditorUtility.DisplayDialog("Cleaning Featuresets", "You are about to clean featuresets. This will remove any featuresets to be rendered by storm if they don't meet the minimum instance render count. This can not be undone, are you sure you want to proceed?", "Yes", "No"))
                {
                    StormWizardUtils.CleanUpFeatureSets(editorSettings.TransformInstanceCount, editorSettings.IgnoreTRSInstanceCount, editorSettings.DisableTerrainFoliageRendering, editorSettings.DeleteEmptyStormTiles);
                }
            }
        }
        /// <summary>
        /// Creates a add runtime ui button that will instatiate the UI as a prefab
        /// </summary>
        /// <param name="editorUtils"></param>
        public static void AddRuntimeUI()
        {
            GameObject prefab = StormUtils.LoadResource<GameObject>("Content/Storm Runtime UI");
            if (prefab != null)
            {
                GameObject go;
                GameObject[] gos = PrefabUtility.FindAllInstancesOfPrefab(prefab);
                if (gos.Length > 0)
                {
                    go = gos[0];
                }
                else
                {
                    go = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
                    go.name = prefab.name;
                }
                Selection.activeGameObject = go;
                EditorGUIUtility.PingObject(go);
                return;
            }
            else
            {
                Debug.LogError("Unable to locate 'Storm Runtime UI' in project resources.");
            }
        }
        public static Type[] GetAllPlugins()
        {
            var basetype = typeof(StormPluginEditorBase);
            List<Type> ret = new List<Type>();

            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies)
            {
                var types = assembly.GetTypes();
                foreach (var type in types)
                {
                    if (!type.IsAbstract && type.IsClass && type.IsSubclassOf(basetype))
                    {
                        ret.Add(type);
                    }
                }
            }
            ret.Sort((t1, t2) => t1.Name.CompareTo(t2.Name));
            return ret.ToArray();
        }

        public static bool LongFoldout(bool show, string key, EditorUtils editorUtils, bool helpActive, Color bgColor = default)
        {
            if (bgColor == default)
            {
                bgColor = new Color(0, 0, 0, 0.15f);
            }

            //Workaround to get a nice foldout across the entire Editor
            // Draw a full-width background for the foldout
            Rect foldoutRect = GUILayoutUtility.GetRect(1f, 20f, GUILayout.ExpandWidth(true));
            EditorGUI.DrawRect(foldoutRect, bgColor);
            //Draw the foldout on top
            show = EditorGUI.Foldout(foldoutRect, show, editorUtils.GetContent(key), true);
            editorUtils.InlineHelp(key, helpActive);
            return show;
        }

        public static void UnhideObjects()
        {
            GameObject[] unhidden = new GameObject[0];
            foreach (GameObject obj in Resources.FindObjectsOfTypeAll<GameObject>())
            {
                if ((obj.hideFlags & HideFlags.HideInHierarchy) != 0)
                {
                    obj.hideFlags ^= HideFlags.HideInHierarchy;
                    StormUtils.Add(ref unhidden, obj);
                }
            }
            Core.StormWorld.Instance.UnhiddenObjects = unhidden;
            StormUtils.SetAssetDirty(Core.StormWorld.Instance);
        }
        public static void RehideObjects()
        {
            GameObject[] unhidden = Core.StormWorld.Instance.UnhiddenObjects;
            foreach (GameObject go in unhidden)
                if (go != null)
                    go.hideFlags |= HideFlags.HideInHierarchy;
            Core.StormWorld.Instance.UnhiddenObjects = null;
            StormUtils.SetAssetDirty(Core.StormWorld.Instance);
        }
        public static int GetUnhiddenCount()
        {
            if (Core.StormWorld.Instance == null)
                return 0;
            return Core.StormWorld.Instance.UnhiddenObjects?.Length ?? 0;
        }
    }
}