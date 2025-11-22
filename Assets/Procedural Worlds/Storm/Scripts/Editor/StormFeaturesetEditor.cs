using UnityEditor;
using UnityEngine;
using PWCommon5;
using ProceduralWorlds.Storm.Utils;
using UnityEditorInternal;
using System;
using System.Collections.Generic;

/// Copyright 2023 Procedural Worlds Ltd
/// Written by Jonathan Williams as part of Storm

namespace ProceduralWorlds.Storm.StormEditor
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(StormFeatureset))]
    public class StormFeaturesetEditor : PWEditor
    {
        public bool IsEmbedded = false;
        const int StrengthMin = -2;
        const int StrengthMax = 2;
        private class FeatureLayerData
        {
            EditorUtils m_editorUtils;
            SerializedProperty Spawn;
            SerializedProperty Exclude;
            public ReorderableList SpawnLayersList;
            public ReorderableList ExclusionLayersList;

            public FeatureLayerData(StormFeaturesetEditor editor, SerializedObject obj, SerializedProperty spawn, SerializedProperty exclude)
            {
                m_editorUtils = editor.m_editorUtils;
                Spawn = spawn;
                Exclude = exclude;
                SpawnLayersList = new ReorderableList(obj, spawn);
                ExclusionLayersList = new ReorderableList(obj, exclude);
                SpawnLayersList.drawHeaderCallback += DrawHeaderSpawn;
                SpawnLayersList.onAddCallback += editor.OnAddLayer;
                SpawnLayersList.drawElementCallback += DrawSpawnElement;
                SpawnLayersList.drawNoneElementCallback += DrawSpawnEmpty;
                SpawnLayersList.elementHeightCallback += GetSpawnElementHeight;
                ExclusionLayersList.drawHeaderCallback += DrawHeaderExclude;
                ExclusionLayersList.onAddCallback += editor.OnAddLayer;
                ExclusionLayersList.drawElementCallback += DrawExcludeElement;
                ExclusionLayersList.drawNoneElementCallback += DrawExcludeEmpty;
                ExclusionLayersList.elementHeightCallback += GetExcludeElementHeight;
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
                if (GUI.Button(cr, m_editorUtils.GetContent("CopyButton"), EditorStyles.miniButtonLeft))
                {
                    string layerString = GetLayerString(Spawn);
                    if (GUIUtility.systemCopyBuffer != layerString)
                    {
                        GUIUtility.systemCopyBuffer = layerString;
                        Debug.Log("Copied \"Spawn Layers\" list to clipboard");
                    }
                }
                string clipboardData = GUIUtility.systemCopyBuffer;
                EditorGUI.BeginDisabledGroup(!clipboardData.StartsWith("StormLayers"));
                if (GUI.Button(pr, m_editorUtils.GetContent("PasteButton"), EditorStyles.miniButtonRight))
                {
                    PasteLayerString(Spawn, clipboardData, out int numNew, out int numSkipped);
                    Debug.Log("Pasted " + numNew + " new and " + numSkipped + " existing layers into \"Spawn Layers\" list.");
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
                if (GUI.Button(cr, m_editorUtils.GetContent("CopyButton"), EditorStyles.miniButtonLeft))
                {
                    string layerString = GetLayerString(Exclude);
                    if (GUIUtility.systemCopyBuffer != layerString)
                    {
                        GUIUtility.systemCopyBuffer = layerString;
                        Debug.Log("Copied \"Exclusion Layers\" list to clipboard");
                    }
                }
                string clipboardData = GUIUtility.systemCopyBuffer;
                EditorGUI.BeginDisabledGroup(!clipboardData.StartsWith("StormLayers"));
                if (GUI.Button(pr, m_editorUtils.GetContent("PasteButton"), EditorStyles.miniButtonRight))
                {
                    PasteLayerString(Exclude, clipboardData, out int numNew, out int numSkipped);
                    Debug.Log("Pasted " + numNew + " new and " + numSkipped + " existing layers into \"Exclusion Layers\" list.");
                }
                EditorGUI.EndDisabledGroup();
            }
            float GetSpawnElementHeight(int index)
            {
                SerializedProperty layerData = Spawn.GetArrayElementAtIndex(index);
                SerializedProperty layer = layerData.FindPropertyRelative("LayerName");
                return EditorGUI.GetPropertyHeight(layer) + EditorGUIUtility.standardVerticalSpacing + EditorGUIUtility.singleLineHeight;
            }
            float GetExcludeElementHeight(int index)
            {
                SerializedProperty layerData = Exclude.GetArrayElementAtIndex(index);
                SerializedProperty layer = layerData.FindPropertyRelative("LayerName");
                return EditorGUI.GetPropertyHeight(layer) + EditorGUIUtility.standardVerticalSpacing + EditorGUIUtility.singleLineHeight;
            }
            void DrawSpawnElement(Rect rect, int index, bool isActive, bool isFocused)
            {
                SerializedProperty layerData = Spawn.GetArrayElementAtIndex(index);
                SerializedProperty layer = layerData.FindPropertyRelative("LayerName");
                SerializedProperty strength = layerData.FindPropertyRelative("Strength");
                rect.height = EditorGUI.GetPropertyHeight(layer);
                EditorGUI.PropertyField(rect, layer);
                rect.y += rect.height + EditorGUIUtility.standardVerticalSpacing;
                rect.height = EditorGUIUtility.singleLineHeight;
                DoFloatField(rect, strength);
            }
            void DrawExcludeElement(Rect rect, int index, bool isActive, bool isFocused)
            {
                rect.height = EditorGUIUtility.singleLineHeight;
                SerializedProperty layerData = Exclude.GetArrayElementAtIndex(index);
                SerializedProperty layer = layerData.FindPropertyRelative("LayerName");
                SerializedProperty strength = layerData.FindPropertyRelative("Strength");
                EditorGUI.PropertyField(rect, layer);
                rect.y += rect.height + EditorGUIUtility.standardVerticalSpacing;
                DoFloatField(rect, strength);
            }
            void DrawSpawnEmpty(Rect rect)
            {
                GUI.Label(rect, m_editorUtils.GetContent("SpawnFeatureEmpty"));
            }
            void DrawExcludeEmpty(Rect rect)
            {
                GUI.Label(rect, m_editorUtils.GetContent("ExcludeFeatureEmpty"));
            }
        }

        EditorUtils m_editorUtils;
        private GUIContent AddButtonContent;
        private GUIContent RemoveButtonContent;
        private GUIContent EnableButtonContent;
        private GUIContent DisableButtonContent;

        SerializedProperty p_SpawnLayers;
        SerializedProperty p_ExclusionLayers;
        SerializedProperty p_Features;
        StormScriptableUI<StormFeatureSettings>[] m_featureUIs = new StormScriptableUI<StormFeatureSettings>[0];

        ReorderableList m_spawnLayersList;
        ReorderableList m_exclusionLayersList;
        FeatureLayerData[] FeatureData = new FeatureLayerData[0];
        static bool ExpertMode = false;
        bool needStormUpdate = false;

        private List<StormLayerRemap> m_remaps;
        private StormScriptableUI<StormLayerRemap>[] m_remapUIs = new StormScriptableUI<StormLayerRemap>[0];
        bool IsPickingRemap;
        int pickingRemapIndex;

        private void OnEnable()
        {
            if (target as StormFeatureset == null)
                return;
            p_SpawnLayers = serializedObject.FindProperty("SpawnLayers");
            p_ExclusionLayers = serializedObject.FindProperty("ExclusionLayers");
            p_Features = serializedObject.FindProperty("Features");

            m_spawnLayersList = new ReorderableList(serializedObject, p_SpawnLayers);
            m_exclusionLayersList = new ReorderableList(serializedObject, p_ExclusionLayers);
            m_spawnLayersList.drawHeaderCallback += DrawHeaderSpawn;
            m_spawnLayersList.onAddCallback += OnAddLayer;
            m_spawnLayersList.drawElementCallback += DrawSpawnElement;
            m_spawnLayersList.drawNoneElementCallback += DrawSpawnEmpty;
            m_spawnLayersList.elementHeightCallback += GetSpawnElementHeight;
            m_exclusionLayersList.drawHeaderCallback += DrawHeaderExclude;
            m_exclusionLayersList.onAddCallback += OnAddLayer;
            m_exclusionLayersList.drawElementCallback += DrawExcludeElement;
            m_exclusionLayersList.drawNoneElementCallback += DrawExcludeEmpty;
            m_exclusionLayersList.elementHeightCallback += GetExcludeElementHeight;

            AddButtonContent = EditorGUIUtility.IconContent("d_Toolbar Plus@2x");
            RemoveButtonContent = EditorGUIUtility.IconContent("d_Toolbar Minus@2x");
            EnableButtonContent = EditorGUIUtility.IconContent("d_scenevis_hidden_hover@2x");
            DisableButtonContent = EditorGUIUtility.IconContent("d_scenevis_visible_hover@2x");

            m_remaps = new List<StormLayerRemap>((target as StormFeatureset).LayerRemaps);

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
                string layerString = GetLayerString(p_SpawnLayers);
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
                PasteLayerString(p_SpawnLayers, clipboardData, out int numNew, out int numSkipped);
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
                string layerString = GetLayerString(p_ExclusionLayers);
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
                PasteLayerString(p_ExclusionLayers, clipboardData, out int numNew, out int numSkipped);
                //Debug.Log("Pasted " + numNew + " new and " + numSkipped + " existing layers into \"Exclusion Layers\" list.");
            }
            EditorGUI.EndDisabledGroup();
        }
        float GetSpawnElementHeight(int index)
        {
            SerializedProperty layerData = p_SpawnLayers.GetArrayElementAtIndex(index);
            SerializedProperty layer = layerData.FindPropertyRelative("LayerName");
            return EditorGUI.GetPropertyHeight(layer) + EditorGUIUtility.standardVerticalSpacing + EditorGUIUtility.singleLineHeight;
        }
        float GetExcludeElementHeight(int index)
        {
            SerializedProperty layerData = p_ExclusionLayers.GetArrayElementAtIndex(index);
            SerializedProperty layer = layerData.FindPropertyRelative("LayerName");
            return EditorGUI.GetPropertyHeight(layer) + EditorGUIUtility.standardVerticalSpacing + EditorGUIUtility.singleLineHeight;
        }
        void DrawSpawnElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            SerializedProperty layerData = p_SpawnLayers.GetArrayElementAtIndex(index);
            SerializedProperty layer = layerData.FindPropertyRelative("LayerName");
            SerializedProperty strength = layerData.FindPropertyRelative("Strength");
            rect.height = EditorGUI.GetPropertyHeight(layer);
            EditorGUI.PropertyField(rect, layer);
            rect.y += rect.height + EditorGUIUtility.standardVerticalSpacing;
            rect.height = EditorGUIUtility.singleLineHeight;
            DoFloatField(rect, strength);
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
        void DrawSpawnEmpty(Rect rect)
        {
            GUI.Label(rect, m_editorUtils.GetContent("SpawnMaskEmpty"));
        }
        void DrawExcludeEmpty(Rect rect)
        {
            GUI.Label(rect, m_editorUtils.GetContent("ExcludeMaskEmpty"));
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

        public override void OnInspectorGUI()
        {
            if (m_editorUtils == null)
                m_editorUtils = StormApp.GetEditorUtils(this);
            m_editorUtils.Initialize();
            if (!IsEmbedded)
                m_editorUtils.ToggleButton("ExpertMode", ref ExpertMode);
            m_editorUtils.Panel("LayerRemapPanel", LayerRemapPanel, false);
            m_editorUtils.Panel("MaskSettingsPanel", MaskSettingsPanel, true);
            m_editorUtils.Panel("FeaturesPanel", FeaturesPanel, true);
            serializedObject.ApplyModifiedProperties();
            if (needStormUpdate)
            {
                needStormUpdate = false;
                if (Core.StormWorld.Instance != null)
                    Core.StormWorld.Instance.UpdateFeaturesets();
            }


            if (IsPickingRemap)
            {
                string commandName = Event.current.commandName;
                if (commandName == "ObjectSelectorUpdated")
                {
                    UnityEngine.Object obj = EditorGUIUtility.GetObjectPickerObject();
                    if (obj is StormLayerRemap remap)
                    {
                        if (pickingRemapIndex < 0)
                        {
                            pickingRemapIndex = m_remaps.Count;
                            m_remaps.Add(null);
                        }
                        UpdateRemap(remap, pickingRemapIndex);
                    }
                }
                else if (commandName == "ObjectSelectorClosed")
                {
                    IsPickingRemap = false;
                    UnityEngine.Object obj = EditorGUIUtility.GetObjectPickerObject();
                    if (obj is StormLayerRemap remap)
                    {
                        if (pickingRemapIndex < 0)
                        {
                            pickingRemapIndex = m_remaps.Count;
                            m_remaps.Add(null);
                        }
                        UpdateRemap(remap, pickingRemapIndex);
                    }
                }
            }
        }
        public void MaskSettingsPanel(bool helpActive)
        {
            ++EditorGUI.indentLevel;
            m_spawnLayersList.DoLayoutList();
            m_editorUtils.InlineHelp("SpawnLayers", helpActive);
            EditorGUILayout.Space();
            m_exclusionLayersList.DoLayoutList();
            m_editorUtils.InlineHelp("ExclusionLayers", helpActive);
            --EditorGUI.indentLevel;
        }
        public void FeaturesPanel(bool helpActive)
        {
            ++EditorGUI.indentLevel;
            int numFeatures = p_Features.arraySize;
            if (FeatureData.Length != numFeatures)
                Array.Resize(ref FeatureData, numFeatures);
            int toRemove = -1;
            GUIContent remButton = m_editorUtils.GetContent("RemoveFeatureButton");
            remButton.image = RemoveButtonContent.image;
            Color startColor = GUI.backgroundColor;
            GUIContent disableButton = m_editorUtils.GetContent("DisableButton");
            disableButton.image = DisableButtonContent.image;
            GUIContent enableButton = m_editorUtils.GetContent("EnableButton");
            enableButton.image = EnableButtonContent.image;
            for (int f = 0; f < numFeatures; ++f)
            {
                SerializedProperty fData = p_Features.GetArrayElementAtIndex(f);

                SerializedProperty feature = fData.FindPropertyRelative("Feature");
                if (FeatureData[f] == null)
                {
                    SerializedProperty spawn = fData.FindPropertyRelative("SpawnLayers");
                    SerializedProperty exclude = fData.FindPropertyRelative("ExclusionLayers");
                    FeatureData[f] = new FeatureLayerData(this, serializedObject, spawn, exclude);
                }
                EditorGUILayout.BeginHorizontal();
                fData.isExpanded = EditorGUILayout.Foldout(fData.isExpanded, feature == null || feature.objectReferenceValue == null ? m_editorUtils.GetContent("FeatureSettingsNull") : new GUIContent(((StormFeatureSettings)feature.objectReferenceValue).name), true);

                if (feature.objectReferenceValue is StormFeatureSettings fObj)
                {
                    StormFeatureset fsObj = target as StormFeatureset;
                    bool isDisabled = !fsObj.IsEnabled || Core.StormSettings.SystemSettings.IsFeatureDisabled(fsObj, fObj);
                    if (isDisabled)
                        GUI.backgroundColor = Color.grey;
                    EditorGUI.BeginDisabledGroup(!fsObj.IsEnabled);
                    if (GUILayout.Button(isDisabled ? enableButton : disableButton, GUILayout.Height(EditorGUIUtility.singleLineHeight), GUILayout.Width(80)))
                    {
                        Undo.RecordObject(target, "Show/Hide Feature");
                        Core.StormSettings.SystemSettings.SetFeatureDisabled(fsObj, fObj, !isDisabled);
                        needStormUpdate = true;
                    }
                    EditorGUI.EndDisabledGroup();
                }
                GUI.backgroundColor = startColor;

                if (m_editorUtils.Button(remButton, GUILayout.Height(EditorGUIUtility.singleLineHeight), GUILayout.ExpandWidth(false)))
                    toRemove = f;
                EditorGUILayout.EndHorizontal();
                if (fData.isExpanded)
                {
                    int indent = EditorGUI.indentLevel;
                    ++EditorGUI.indentLevel;
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    FeatureData[f].SpawnLayersList.DoLayoutList();
                    EditorGUILayout.Space();
                    FeatureData[f].ExclusionLayersList.DoLayoutList();
                    EditorGUI.indentLevel = 0;
                    GetFeatureUI(f).DoLayout();
                    //m_editorUtils.PropertyField("Feature", feature, true);
                    EditorGUILayout.EndVertical();
                    EditorGUI.indentLevel = indent;
                }
            }
            EditorGUILayout.Space();
            GUIContent addButton = m_editorUtils.GetContent("AddFeatureButton");
            addButton.image = AddButtonContent.image;
            if (m_editorUtils.Button(addButton, GUILayout.Height(EditorGUIUtility.singleLineHeight)))
            {
                p_Features.InsertArrayElementAtIndex(numFeatures);
            }
            --EditorGUI.indentLevel;
            if (toRemove >= 0)
            {
                p_Features.DeleteArrayElementAtIndex(toRemove);
                StormUtils.RemoveAt(ref m_featureUIs, toRemove);
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


        StormScriptableUI<StormFeatureSettings> GetFeatureUI(int index)
        {
            if (index < 0 || index >= p_Features.arraySize)
                return null;
            if (m_featureUIs.Length != p_Features.arraySize)
                Array.Resize(ref m_featureUIs, p_Features.arraySize);
            if (m_featureUIs[index] == null)
                m_featureUIs[index] = new StormScriptableUI<StormFeatureSettings>(p_Features.GetArrayElementAtIndex(index).FindPropertyRelative("Feature").objectReferenceValue as StormFeatureSettings, null, Repaint, m_editorUtils, (f) => UpdateFeature(f, index));
            return m_featureUIs[index];
        }
        void UpdateFeature(StormFeatureSettings f, int index)
        {
            p_Features.GetArrayElementAtIndex(index).FindPropertyRelative("Feature").objectReferenceValue = f;
            if (Core.StormWorld.Instance != null)
                Core.StormWorld.Instance.Refresh(f);
            needStormUpdate = true;
        }


        private void LayerRemapPanel(bool helpEnabled)
        {
            for (int r = 0; r < m_remaps.Count; ++r)
            {
                GetRemapUI(r).DoLayout();
            }
            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            GUIContent addButton = m_editorUtils.GetContent("RemapAddButton");
            addButton.image = AddButtonContent.image;
            if (GUILayout.Button(addButton, EditorGUIUtility.currentViewWidth < 300 ? EditorStyles.miniButtonRight : EditorStyles.miniButtonMid, GUILayout.Height(EditorGUIUtility.singleLineHeight), GUILayout.ExpandWidth(false)))
            {
                IsPickingRemap = true;
                pickingRemapIndex = m_remaps.Count;
                m_remaps.Add(null);
                m_remapUIs = new StormScriptableUI<StormLayerRemap>[m_remaps.Count];
                EditorGUIUtility.ShowObjectPicker<StormLayerRemap>(null, true, "t:StormLayerRemap", EditorGUIUtility.GetControlID(FocusType.Passive));
            }
            EditorGUILayout.EndHorizontal();
        }
        StormScriptableUI<StormLayerRemap> GetRemapUI(int index)
        {
            if (index < 0 || index >= m_remaps.Count)
                return null;
            if (m_remapUIs.Length != m_remaps.Count)
                Array.Resize(ref m_remapUIs, m_remaps.Count);
            if (m_remapUIs[index] == null)
                m_remapUIs[index] = new StormScriptableUI<StormLayerRemap>(m_remaps[index], null, Repaint, m_editorUtils, (r) => UpdateRemap(r, index), true, () => RemapButtons(index));
            return m_remapUIs[index];
        }
        void UpdateRemap(StormLayerRemap remap, int index)
        {
            if (m_remaps[index] != remap)
            {
                m_remaps[index] = remap;
                (target as StormFeatureset).LayerRemaps = m_remaps.ToArray();
                GetRemapUI(index).Target = remap;
                StormUtils.SetAssetDirty(target);
            }
        }
        void RemapButtons(int index)
        {
            GUIContent remove = m_editorUtils.GetContent("RemapRemoveButton");
            remove.image = RemoveButtonContent.image;
            if (GUILayout.Button(remove, GUILayout.Height(EditorGUIUtility.singleLineHeight), GUILayout.ExpandWidth(false)))
            {
                m_remaps.RemoveAt(index);
                (target as StormFeatureset).LayerRemaps = m_remaps.ToArray();
                m_remapUIs = new StormScriptableUI<StormLayerRemap>[0];
                StormUtils.SetAssetDirty(target);
            }
        }

        static internal string GetLayerString(SerializedProperty LayerList)
        {
            string layerString = "StormLayers";
            for (int l = 0; l < LayerList.arraySize; ++l)
            {
                SerializedProperty layerData = LayerList.GetArrayElementAtIndex(l);
                SerializedProperty layer = layerData.FindPropertyRelative("LayerName");
                SerializedProperty strength = layerData.FindPropertyRelative("Strength");
                layerString += "\n" + strength.floatValue + ":" + layer.stringValue;
            }
            return layerString;
        }
        static internal void PasteLayerString(SerializedProperty LayerList, string clipboardData, out int numNew, out int numSkipped)
        {
            numNew = 0;
            numSkipped = 0;
            string[] splitLayers = clipboardData.Split('\n', StringSplitOptions.None);
            for (int index = 1; index < splitLayers.Length; ++index)
            {
                int splitIndex = splitLayers[index].IndexOf(':');
                if (splitIndex > 0 && float.TryParse(splitLayers[index].Substring(0, splitIndex), out float layerStrength))
                {
                    string layerName = splitLayers[index].Substring(splitIndex + 1);
                    bool exists = false;
                    for (int l = 0; l < LayerList.arraySize; ++l)
                    {
                        SerializedProperty layerData = LayerList.GetArrayElementAtIndex(l);
                        SerializedProperty layer = layerData.FindPropertyRelative("LayerName");
                        SerializedProperty strength = layerData.FindPropertyRelative("Strength");
                        if (layer.stringValue == layerName)
                        {
                            strength.floatValue = layerStrength;
                            exists = true;
                            ++numSkipped;
                            break;
                        }
                    }
                    if (!exists)
                    {
                        int l = LayerList.arraySize;
                        LayerList.InsertArrayElementAtIndex(l);
                        SerializedProperty layerData = LayerList.GetArrayElementAtIndex(l);
                        SerializedProperty layer = layerData.FindPropertyRelative("LayerName");
                        SerializedProperty strength = layerData.FindPropertyRelative("Strength");
                        layer.stringValue = layerName;
                        strength.floatValue = layerStrength;
                        ++numNew;
                    }
                }
            }
        }
    }
}