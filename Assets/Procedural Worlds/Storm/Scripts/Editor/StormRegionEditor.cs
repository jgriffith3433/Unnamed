using UnityEditor;
using UnityEngine;
using PWCommon5;
using ProceduralWorlds.Storm.Utils;
using UnityEditorInternal;
using System;
using System.Collections.Generic;
using System.IO;

/// Copyright 2023 Procedural Worlds Ltd
/// Written by Jonathan Williams as part of Storm

namespace ProceduralWorlds.Storm.StormEditor
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(StormRegion))]
    public class StormRegionEditor : PWEditor
    {
        abstract class StormSpawnDataBase
        {

        }
        class StormRegionMaskData : StormSpawnDataBase
        {
            public StormRegionMask SpawnMask;
        }
        class StormRegionSpawnData : StormSpawnDataBase
        {
            public SerializedProperty Layer;
        }

        List<StormSpawnDataBase> m_spawnData = new List<StormSpawnDataBase>();
        void UpdateSpawnData()
        {
            if (m_spawnData == null)
                m_spawnData = new List<StormSpawnDataBase>();
            m_spawnData.Clear();
            for (int i = 0; i < m_region.RegionMasks.Count; ++i)
            {
                if (m_region.RegionMasks[i] == null)
                    m_region.RegionMasks.RemoveAt(i--);
                else
                    m_spawnData.Add(new StormRegionMaskData() { SpawnMask = m_region.RegionMasks[i] });
            }
            for (int i = 0; i < p_SpawnLayers.arraySize; ++i)
            {
                m_spawnData.Add(new StormRegionSpawnData() { Layer = p_SpawnLayers.GetArrayElementAtIndex(i) });
            }
        }

        public bool IsEmbedded = false;
        const int StrengthMin = -2;
        const int StrengthMax = 2;

        EditorUtils m_editorUtils;
        private GUIContent AddButtonContent;
        private GUIContent RemoveButtonContent;
        private GUIContent EnableButtonContent;
        private GUIContent DisableButtonContent;

        SerializedProperty p_SpawnLayers;
        SerializedProperty p_ExclusionLayers;
        SerializedProperty p_Featuresets;
        StormScriptableUI<StormFeatureset>[] m_featuresetUIs = new StormScriptableUI<StormFeatureset>[0];

        ReorderableList m_spawnLayersList;
        ReorderableList m_exclusionLayersList;
        ReorderableList m_featuresetsList;
        static bool ExpertMode = false;
        bool needStormUpdate = false;
        string m_newRegionMaskName = "";
        bool IsPickingMask = false;

        StormRegion m_region;


        private void OnEnable()
        {
            m_region = target as StormRegion;
            if (m_region == null)
                return;
            p_SpawnLayers = serializedObject.FindProperty("SpawnLayers");
            p_ExclusionLayers = serializedObject.FindProperty("ExclusionLayers");
            p_Featuresets = serializedObject.FindProperty("Featuresets");
            UpdateSpawnData();
            m_spawnLayersList = new ReorderableList(m_spawnData, typeof(StormSpawnDataBase), false, true, true, true);
            m_spawnLayersList.drawHeaderCallback += DrawHeaderSpawn;
            m_spawnLayersList.drawElementCallback += DrawSpawnElement;
            m_spawnLayersList.onAddDropdownCallback += OnAddSpawnLayer;
            m_spawnLayersList.onRemoveCallback += OnRemoveSpawnLayer;
            m_spawnLayersList.drawNoneElementCallback += DrawSpawnEmpty;
            m_spawnLayersList.elementHeightCallback += GetSpawnElementHeight;
            m_exclusionLayersList = new ReorderableList(serializedObject, p_ExclusionLayers);
            m_exclusionLayersList.drawHeaderCallback += DrawHeaderExclude;
            m_exclusionLayersList.onAddCallback += OnAddLayer;
            m_exclusionLayersList.drawElementCallback += DrawExcludeElement;
            m_exclusionLayersList.drawNoneElementCallback += DrawExcludeEmpty;
            m_exclusionLayersList.elementHeightCallback += GetExcludeElementHeight;
            m_featuresetsList = new ReorderableList(serializedObject, p_Featuresets, true, false, true, true);
            m_featuresetsList.drawElementCallback += DrawFeaturesetElement;

            AddButtonContent = EditorGUIUtility.IconContent("d_Toolbar Plus@2x");
            RemoveButtonContent = EditorGUIUtility.IconContent("d_Toolbar Minus@2x");
            EnableButtonContent = EditorGUIUtility.IconContent("d_scenevis_hidden_hover@2x");
            DisableButtonContent = EditorGUIUtility.IconContent("d_scenevis_visible_hover@2x");

            Undo.undoRedoPerformed -= OnUndo;
            Undo.undoRedoPerformed += OnUndo;
        }
        private void OnDisable()
        {
            Undo.undoRedoPerformed -= OnUndo;
        }

        void DrawHeaderSpawn(Rect r)
        {
            Rect lr = r;
            lr.width -= 100;
            Rect cr = lr;
            cr.x = cr.xMax;
            cr.width = 50;
            Rect pr = cr;
            pr.x = pr.xMax;
            GUI.Label(lr, m_editorUtils.GetContent("SpawnLayers"));
            EditorGUI.BeginDisabledGroup(p_SpawnLayers.arraySize == 0);
            if (GUI.Button(cr, m_editorUtils.GetContent("CopyButton"), EditorStyles.miniButtonLeft))
            {
                string layerString = StormFeaturesetEditor.GetLayerString(p_SpawnLayers);
                if (GUIUtility.systemCopyBuffer != layerString)
                {
                    GUIUtility.systemCopyBuffer = layerString;
                    //Debug.Log("Copied \"Spawn Layers\" list to clipboard");
                }
            }
            EditorGUI.EndDisabledGroup();
            string clipboardData = GUIUtility.systemCopyBuffer;
            EditorGUI.BeginDisabledGroup(!clipboardData.StartsWith("StormLayers"));
            if (GUI.Button(pr, m_editorUtils.GetContent("PasteButton"), EditorStyles.miniButtonRight))
            {
                StormFeaturesetEditor.PasteLayerString(p_SpawnLayers, clipboardData, out int numNew, out int numSkipped);
                //Debug.Log("Pasted " + numNew + " new and " + numSkipped + " existing layers into \"Spawn Layers\" list.");
            }
            EditorGUI.EndDisabledGroup();
        }
        void DrawHeaderExclude(Rect r)
        {
            Rect lr = r;
            lr.width -= 100;
            Rect cr = lr;
            cr.x = cr.xMax;
            cr.width = 50;
            Rect pr = cr;
            pr.x = pr.xMax;
            GUI.Label(lr, m_editorUtils.GetContent("ExclusionLayers"));
            EditorGUI.BeginDisabledGroup(p_ExclusionLayers.arraySize == 0);
            if (GUI.Button(cr, m_editorUtils.GetContent("CopyButton"), EditorStyles.miniButtonLeft))
            {
                string layerString = StormFeaturesetEditor.GetLayerString(p_ExclusionLayers);
                if (GUIUtility.systemCopyBuffer != layerString)
                {
                    GUIUtility.systemCopyBuffer = layerString;
                    //Debug.Log("Copied \"Exclusion Layers\" list to clipboard");
                }
            }
            EditorGUI.EndDisabledGroup();
            string clipboardData = GUIUtility.systemCopyBuffer;
            EditorGUI.BeginDisabledGroup(!clipboardData.StartsWith("StormLayers"));
            if (GUI.Button(pr, m_editorUtils.GetContent("PasteButton"), EditorStyles.miniButtonRight))
            {
                StormFeaturesetEditor.PasteLayerString(p_ExclusionLayers, clipboardData, out int numNew, out int numSkipped);
                //Debug.Log("Pasted " + numNew + " new and " + numSkipped + " existing layers into \"Exclusion Layers\" list.");
            }
            EditorGUI.EndDisabledGroup();
        }
        float GetSpawnElementHeight(int index)
        {
            if (m_spawnData[index] is StormRegionSpawnData sData)
            {
                SerializedProperty layer = sData.Layer.FindPropertyRelative("LayerName");
                return EditorGUI.GetPropertyHeight(layer) + EditorGUIUtility.standardVerticalSpacing + EditorGUIUtility.singleLineHeight;
            }
            else
                return EditorGUIUtility.singleLineHeight;
        }
        float GetExcludeElementHeight(int index)
        {
            SerializedProperty layerData = p_ExclusionLayers.GetArrayElementAtIndex(index);
            SerializedProperty layer = layerData.FindPropertyRelative("LayerName");
            return EditorGUI.GetPropertyHeight(layer) + EditorGUIUtility.standardVerticalSpacing + EditorGUIUtility.singleLineHeight;
        }
        void DrawSpawnElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            if (m_spawnData[index] is StormRegionMaskData mData)
            {
                rect.height = EditorGUIUtility.singleLineHeight;
                GUIContent label = m_editorUtils.GetContent("RegionMaskPrefix");
                Vector2 labelSize = EditorStyles.label.CalcSize(label);
                GUI.Label(rect, label, EditorStyles.label);
                rect.x += labelSize.x + 5;
                rect.width -= labelSize.x + 5;
                EditorGUI.ObjectField(rect, mData.SpawnMask, typeof(StormRegionMask), true);
            }
            else if (m_spawnData[index] is StormRegionSpawnData sData)
            {
                SerializedProperty layer = sData.Layer.FindPropertyRelative("LayerName");
                SerializedProperty strength = sData.Layer.FindPropertyRelative("Strength");
                rect.height = EditorGUI.GetPropertyHeight(layer);
                EditorGUI.PropertyField(rect, layer);
                rect.y += rect.height + EditorGUIUtility.standardVerticalSpacing;
                rect.height = EditorGUIUtility.singleLineHeight;
                DoFloatField(rect, strength);
            }
        }
        void DrawSpawnEmpty(Rect rect)
        {
            GUI.Label(rect, m_editorUtils.GetContent(m_region.RegionMasks.Count > 0 || m_region.RequireRegionMasks ? "EmptySpawnMaskedMessage" : "EmptySpawnMessage"));
        }
        void DrawExcludeElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            SerializedProperty layerData = p_ExclusionLayers.GetArrayElementAtIndex(index);
            SerializedProperty layer = layerData.FindPropertyRelative("LayerName");
            SerializedProperty strength = layerData.FindPropertyRelative("Strength");
            rect.height = EditorGUI.GetPropertyHeight(layer);
            EditorGUI.PropertyField(rect, layer);
            rect.y += rect.height + EditorGUIUtility.standardVerticalSpacing;
            rect.height = EditorGUIUtility.singleLineHeight;
            DoFloatField(rect, strength);
        }
        void DrawExcludeEmpty(Rect rect)
        {
            GUI.Label(rect, m_editorUtils.GetContent("EmptyExclusionMessage"));
        }
        void OnAddSpawnLayer(Rect buttonRect, ReorderableList list)
        {
            EditorUtility.DisplayCustomMenu(buttonRect, new GUIContent[] {
                m_editorUtils.GetContent("CreateSpawnMaskOption"),
                m_editorUtils.GetContent("AddSpawnMaskOption"),
                m_editorUtils.GetContent("AddSpawnLayerOption"),
            }, -1, (data, options, selected) => {
                if (selected == 0)
                {
                    string MaskName = $"Storm Region -{DateTime.Now:ffffff}";

                    StormRegionMask mask = new GameObject(MaskName).AddComponent<StormRegionMask>();
                    mask.AddRegionLink(m_region);
                    EditorGUIUtility.PingObject(mask);

                    StormUtils.SetAssetDirty(m_region);
                    UpdateSpawnData();
                    Repaint();
                }
                else if (selected == 1)
                {
                    IsPickingMask = true;
                    EditorGUIUtility.ShowObjectPicker<StormRegionMask>(null, true, "t:StormRegionMask", EditorGUIUtility.GetControlID(FocusType.Passive));
                }
                else
                {
                    int index = p_SpawnLayers.arraySize;
                    p_SpawnLayers.InsertArrayElementAtIndex(index);
                    if (index == 0)
                    {
                        SerializedProperty itemProp = p_SpawnLayers.GetArrayElementAtIndex(index);
                        SerializedProperty strengthProp = itemProp.FindPropertyRelative("Strength");
                        strengthProp.floatValue = 1f;
                    }
                    UpdateSpawnData();
                    Repaint();
                }
            }, null);
        }
        void OnRemoveSpawnLayer(ReorderableList list)
        {
            foreach (int idx in list.selectedIndices)
            {
                if (m_spawnData[idx] is StormRegionMaskData mData)
                {
                    mData.SpawnMask.RemoveRegionLink(m_region);
                }
                else if (m_spawnData[idx] is StormRegionSpawnData sData)
                {
                    sData.Layer.DeleteCommand();
                }
            }
            UpdateSpawnData();
        }
        void OnAddLayer(ReorderableList list)
        {
            int index = list.serializedProperty.arraySize;
            list.serializedProperty.InsertArrayElementAtIndex(index);
            if (index == 0)
            {
                SerializedProperty itemProp = list.serializedProperty.GetArrayElementAtIndex(index);
                SerializedProperty strengthProp = itemProp.FindPropertyRelative("Strength");
                strengthProp.floatValue = 1f;
            }
        }

        void DrawFeaturesetElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            EditorGUI.BeginChangeCheck();
            EditorGUI.PropertyField(rect, p_Featuresets.GetArrayElementAtIndex(index));
            if (EditorGUI.EndChangeCheck())
            {
                string grPath = StormUtils.GetAssetPath(m_region);
                if (!string.IsNullOrEmpty(grPath))
                {
                    StormFeatureset featureset = p_Featuresets.GetArrayElementAtIndex(index).objectReferenceValue as StormFeatureset;
                    string gfPath = StormUtils.GetAssetPath(featureset);
                    if (string.IsNullOrEmpty(gfPath))
                    {
                        gfPath = grPath.Substring(0, grPath.Length - Path.GetFileName(grPath).Length);
                        gfPath += StormUtils.EnsureUniqueName(grPath, string.IsNullOrEmpty(featureset.name) ? m_region.name + "_Featureset" : featureset.name, "asset");
                        featureset = StormUtils.SaveAsset(featureset, gfPath);
                        p_Featuresets.GetArrayElementAtIndex(index).objectReferenceValue = featureset;
                        StormUtils.SetAssetDirty(m_region);
                        Debug.LogWarning("Embedded Featuresets do not persist inside of a Region asset.\nYour featureset has been saved at the path: " + gfPath, featureset);
                    }
                }
            }
        }

        public override void OnInspectorGUI()
        {
            if (m_editorUtils == null)
                m_editorUtils = StormApp.GetEditorUtils(this);
            m_editorUtils.Initialize();
            //m_editorUtils.Panel("RegionMaskSettingsPanel", RegionMaskSettingsPanel, m_region.RequireRegionMasks || m_region.RegionMasks?.Count > 0);
            m_editorUtils.Panel("MaskSettingsPanel", MaskSettingsPanel, true);
            m_editorUtils.Panel("FeaturesetsPanel", FeaturesetsPanel, true);
            serializedObject.ApplyModifiedProperties();
            if (needStormUpdate && Core.StormWorld.Instance != null)
            {
                needStormUpdate = false;
                Core.StormWorld.Instance.UpdateFeaturesets();
            }

            if (IsPickingMask)
            {
                string commandName = Event.current.commandName;
                //if (commandName == "ObjectSelectorUpdated")
                if (commandName == "ObjectSelectorClosed")
                {
                    IsPickingMask = false;

                    StormRegionMask mask;
                    UnityEngine.Object obj = EditorGUIUtility.GetObjectPickerObject();
                    if (obj is GameObject go)
                        mask = go.GetComponent<StormRegionMask>();
                    else if (obj is StormRegionMask rm)
                        mask = rm;
                    else
                        return;
                    
                    if (mask != null)
                    {
                        mask.AddRegionLink(m_region);
                        EditorGUIUtility.PingObject(mask);

                        StormUtils.SetAssetDirty(m_region);
                        UpdateSpawnData();
                        Repaint();
                    }
                }
            }
        }
        public void RegionMaskSettingsPanel(bool helpActive)
        {
            EditorGUI.BeginChangeCheck();
            m_region.RequireRegionMasks = m_editorUtils.Toggle("RequireRegionMasks", m_region.RequireRegionMasks, helpActive);
            if (EditorGUI.EndChangeCheck())
                StormAPI.UpdateRegion(m_region);
            GUILayout.Label(string.Format(m_editorUtils.GetContent("RegionMaskCount").text, m_region.RegionMasks.Count), EditorStyles.boldLabel);
            for (int i = 0; i < m_region.RegionMasks.Count; ++i)
                EditorGUILayout.ObjectField(m_region.RegionMasks[i], typeof(StormRegionMask), true);

            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            m_newRegionMaskName = EditorGUILayout.TextField(m_newRegionMaskName, GUILayout.ExpandWidth(true));
            GUIContent addButton = m_editorUtils.GetContent("AddRegionMaskButton");
            addButton.image = AddButtonContent.image;
            if (GUILayout.Button(addButton, GUILayout.Height(EditorGUIUtility.singleLineHeight), GUILayout.ExpandWidth(false)))
            {
                if (string.IsNullOrEmpty(m_newRegionMaskName))
                    m_newRegionMaskName = $"Storm Region -{DateTime.Now:ffffff}";

                StormRegionMask mask = new GameObject(m_newRegionMaskName).AddComponent<StormRegionMask>();
                mask.AddRegionLink(m_region);

                StormUtils.SetAssetDirty(m_region);
                m_newRegionMaskName = "";
                GUIUtility.hotControl = 0;
                EditorGUIUtility.editingTextField = false;
            }
            EditorGUILayout.EndHorizontal();
        }
        public void MaskSettingsPanel(bool helpActive)
        {
            m_editorUtils.ToggleButton("ExpertMode", ref ExpertMode);
            m_editorUtils.InlineHelp("ExpertMode", helpActive);
            EditorGUI.BeginChangeCheck();
            m_region.RequireRegionMasks = m_editorUtils.Toggle("RequireRegionMasks", m_region.RequireRegionMasks, helpActive);
            if (EditorGUI.EndChangeCheck())
                StormAPI.UpdateRegion(m_region);
            ++EditorGUI.indentLevel;
            m_spawnLayersList.DoLayoutList();
            m_editorUtils.InlineHelp("SpawnLayers", helpActive);
            EditorGUILayout.Space();
            m_exclusionLayersList.DoLayoutList();
            m_editorUtils.InlineHelp("ExclusionLayers", helpActive);
            --EditorGUI.indentLevel;
        }
        public void FeaturesetsPanel(bool helpActive)
        {
            for (int f = 0; f < p_Featuresets.arraySize; ++f)
            {
                GetFeaturesetUI(f).DoLayout();
            }
            EditorGUILayout.Space();
            GUIContent addButton = m_editorUtils.GetContent("FeaturesetAddButton");
            addButton.image = AddButtonContent.image;
            if (GUILayout.Button(addButton, GUILayout.Height(EditorGUIUtility.singleLineHeight)))
            {
                p_Featuresets.InsertArrayElementAtIndex(p_Featuresets.arraySize);
                needStormUpdate = true;
            }
        }

        private void OnUndo()
        {
            serializedObject.Update();
            serializedObject.ApplyModifiedProperties();
        }
        private static void DoFloatField(Rect rect, SerializedProperty prop)
        {
            float val;
            EditorGUI.BeginChangeCheck();
            if (ExpertMode)
                val = EditorGUI.FloatField(rect, prop.floatValue);
            else
                val = EditorGUI.Slider(rect, prop.floatValue, StrengthMin, StrengthMax);
            if (EditorGUI.EndChangeCheck())
                prop.floatValue = val;
        }

        StormScriptableUI<StormFeatureset> GetFeaturesetUI(int index)
        {
            if (index < 0 || index >= p_Featuresets.arraySize)
                return null;
            if (m_featuresetUIs.Length != p_Featuresets.arraySize)
                Array.Resize(ref m_featuresetUIs, p_Featuresets.arraySize);
            if (m_featuresetUIs[index] == null)
                m_featuresetUIs[index] = new StormScriptableUI<StormFeatureset>(p_Featuresets.GetArrayElementAtIndex(index).objectReferenceValue as StormFeatureset, null, Repaint, m_editorUtils, (fs) => UpdateFeatureset(fs, index), true, () => FeaturesetsButtons(index));
            m_featuresetUIs[index].Target = p_Featuresets.GetArrayElementAtIndex(index).objectReferenceValue as StormFeatureset;
            return m_featuresetUIs[index];
        }
        void UpdateFeatureset(StormFeatureset fs, int index)
        {
            SerializedProperty fsProp = p_Featuresets.GetArrayElementAtIndex(index);
            if (fsProp.objectReferenceValue == fs)
                return;
            Undo.RecordObject(target, "Updated Featureset");
            fsProp.objectReferenceValue = fs;
            string grPath = StormUtils.GetAssetPath(m_region);
            if (!string.IsNullOrEmpty(grPath))
            {
                string gfPath = StormUtils.GetAssetPath(fs);
                if (string.IsNullOrEmpty(gfPath))
                {
                    gfPath = grPath.Substring(0, grPath.Length - Path.GetFileName(grPath).Length);
                    gfPath += StormUtils.EnsureUniqueName(gfPath, string.IsNullOrEmpty(fs.name) ? m_region.name + "_Featureset" : fs.name, "asset");
                    fs = StormUtils.SaveAsset(fs, gfPath);
                    p_Featuresets.GetArrayElementAtIndex(index).objectReferenceValue = fs;
                    StormUtils.SetAssetDirty(m_region);
                    Debug.LogWarning("Embedded Featuresets do not persist inside of a Region asset.\nYour featureset has been saved at the path: " + gfPath, fs);
                }
            }
            needStormUpdate = true;
        }
        void FeaturesetsButtons(int index)
        {
            Color startColor = GUI.backgroundColor;
            GUIContent enableButton;
            SerializedProperty fsProp = p_Featuresets.GetArrayElementAtIndex(index);
            StormFeatureset fs = fsProp.objectReferenceValue as StormFeatureset;
            if (fs != null)
            {
                if (fs.IsEnabled)
                {
                    enableButton = m_editorUtils.GetContent("DisableButton");
                    enableButton.image = DisableButtonContent.image;
                }
                else
                {
                    enableButton = m_editorUtils.GetContent("EnableButton");
                    enableButton.image = EnableButtonContent.image;
                    GUI.backgroundColor = Color.grey;
                }
                if (GUILayout.Button(enableButton, GUILayout.Height(EditorGUIUtility.singleLineHeight), GUILayout.Width(80)))
                {
                    Undo.RecordObject(target, "Show/Hide Featureset");
                    fs.IsEnabled = !fs.IsEnabled;
                    needStormUpdate = true;
                }
                GUI.backgroundColor = startColor;
                GUIContent remove = m_editorUtils.GetContent("FeaturesetRemoveButton");
                remove.image = RemoveButtonContent.image;
                if (GUILayout.Button(remove, GUILayout.Height(EditorGUIUtility.singleLineHeight), GUILayout.ExpandWidth(false)))
                {
                    Undo.RecordObject(target, "Removed Featureset");
                    p_Featuresets.DeleteArrayElementAtIndex(index);
                    Array.Resize(ref m_featuresetUIs, m_featuresetUIs.Length - 1);
                    needStormUpdate = true;
                }
            }
        }
    }
}