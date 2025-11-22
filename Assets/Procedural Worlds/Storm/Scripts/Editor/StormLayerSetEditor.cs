using System;
using System.Collections.Generic;
using System.Linq;
using ProceduralWorlds.Storm.Core;
using PWCommon5;
using UnityEditor;
using ProceduralWorlds.Storm.Utils;
using UnityEditorInternal;
using UnityEngine;

namespace ProceduralWorlds.Storm.StormEditor
{
    [CustomEditor(typeof(StormLayerSet))]
    public class StormLayerSetEditor : PWEditor
    {
        private static bool m_overviewPanel = true;
        private StormLayerSet m_stormLayerSet;

        protected EditorUtils m_editorUtils;
        private ReorderableList layerList;
        private int selectedIndex = -1;
        private List<int> sortedLayerIndexes = new List<int>();

        private void OnEnable()
        {
            // If there isn't any Editor Utils Initialized
            if (m_editorUtils == null)
                m_editorUtils = StormApp.GetEditorUtils(this);

            m_stormLayerSet = target as StormLayerSet;

            RefreshList();
        }

        public override void OnInspectorGUI()
        {
            if (m_editorUtils != null)
            {
                m_editorUtils.Initialize();

                serializedObject.Update();

                m_overviewPanel = m_editorUtils.Panel("OverviewPanel", OverviewPanel, m_overviewPanel);

                serializedObject.ApplyModifiedProperties();
            }
            else
            {
                EditorGUILayout.HelpBox(new GUIContent("Localization failed to initialize."));
            }
        }

        private float GetElementHeight(int index)
        {
            // Sorted Layer Index is not zero and doesn't contain index?
            if (sortedLayerIndexes.Count > 0 && !sortedLayerIndexes.Contains(index))
                return 0f; // Minimize it
            return EditorGUIUtility.singleLineHeight + 3f;
        }

        private void RefreshList()
        {
            layerList = new ReorderableList(m_stormLayerSet.Layers, typeof(StormPolygonLayer), false, true, true, true)
            {
                elementHeightCallback = GetElementHeight,
                drawHeaderCallback = (Rect rect) => EditorGUI.LabelField(rect, "Layers"),
                drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
                {
                    // Doesn't contain sorted layer?
                    if (sortedLayerIndexes.Count > 0 && !sortedLayerIndexes.Contains(index))
                        return; // Exit

                    if (index < m_stormLayerSet.Layers.Count)
                    {
                        if (isActive || isFocused)
                        {
                            selectedIndex = index;
                        }

                        float paddingY = 2f;
                        float labelWidth = 60f;

                        // Draw Color Square
                        Rect layerIndexRect = new Rect(rect.x, rect.y + 3f, labelWidth,
                            EditorGUIUtility.singleLineHeight);
                        EditorGUI.LabelField(layerIndexRect, $"Layer {index}:");

                        string layer = m_stormLayerSet.Layers[index];
                        StormLayerType layerType = m_stormLayerSet.LayerTypes[index];

                        float labelTypeWidth = 100f;
                        float spacing = 3f;
                        float layerNameWidth = rect.width - labelTypeWidth - labelWidth;
                        Rect layerTypeRect = new Rect(rect.x + labelWidth + layerNameWidth + spacing * 2, rect.y + paddingY, labelTypeWidth,
                            EditorGUIUtility.singleLineHeight + 2);

                        // Layer Label and Field
                        Rect layerNameRect = new Rect(rect.x + labelWidth + spacing, rect.y + paddingY,
                            layerNameWidth, EditorGUIUtility.singleLineHeight + 2);
                        layer = EditorGUI.TextField(layerNameRect, layer);

                        var options = EnumToString<StormLayerType>(layerType, StormLayerType.NONE);
                        var labelText = "";
                        var tooltipText = "";
                        for (int i = 0; i < options.Length; i++)
                        {
                            if (i > 0)
                            {
                                labelText += ", ";
                                tooltipText += "\n";
                            }
                            
                            labelText += $"{options[i]}";
                            tooltipText += $"{options[i]}";
                        }
                        labelText = TruncateLabel(labelText, layerTypeRect.width); // 50 is the max
                        // Cast the enum to an int to work with the MaskField
                        EditorGUI.BeginDisabledGroup(true);
                        {
                            EditorGUI.LabelField(layerTypeRect, new GUIContent(labelText, tooltipText));
                        }
                        EditorGUI.EndDisabledGroup();

                        m_stormLayerSet.Layers[index] = layer;
                        m_stormLayerSet.LayerTypes[index] = layerType;
                    }
                },
                onAddCallback = OnAddLayer,
                onRemoveCallback = OnRemoveLayer
            };
        }
        public static string TruncateLabel(string source, float rectWidth)
        {
            string text = source;
            var style = GUI.skin.label;
            while (style.CalcSize(new GUIContent(text)).x > rectWidth)
            {
                text = text.Substring(0, text.Length - 4) + "...";
            }
            return text;
        }
        public static string[] EnumToString<EnumType>(EnumType layerType, params EnumType[] ignoredFlags) where EnumType : Enum
        {
            List<string> flagNames = new List<string>();

            foreach (EnumType flag in Enum.GetValues(typeof(EnumType)))
            {
                if (ignoredFlags.Contains(flag))
                    continue;
                if (layerType.HasFlag(flag))
                {
                    flagNames.Add(flag.ToString());
                }
            }

            return flagNames.ToArray();
        }

        private void OnRemoveLayer(ReorderableList list)
        {
            if (selectedIndex < 0)
                return;
            m_stormLayerSet.RemoveLayer(selectedIndex);
            sortedLayerIndexes.Remove(selectedIndex);
            EditorUtility.SetDirty(m_stormLayerSet);
        }

        private void OnAddLayer(object layerObj)
        {
            Undo.RecordObject(m_stormLayerSet, $"Added New Layer");
            // Set the offset to the index, which is the current count of elements in the dictionary
            m_stormLayerSet.AddLayer();
            EditorUtility.SetDirty(m_stormLayerSet);
        }

        private string searchString = "";

        private void OverviewPanel(bool helpEnabled)
        {
            GUILayout.BeginHorizontal();
            {
                if (GUILayout.Button("Get All Storm Layers"))
                {
                    Undo.RecordObject(m_stormLayerSet, "Get All Storm Layers");
                    m_stormLayerSet.ClearAllLayers();
                    m_stormLayerSet.AddStormLayers();

                    EditorUtility.SetDirty(m_stormLayerSet);
                }

                EditorGUI.BeginDisabledGroup(m_stormLayerSet.Layers.Count == 0);
                {
                    if (GUILayout.Button("Clear All Layers"))
                    {
                        Undo.RecordObject(m_stormLayerSet, "Clear All Layers");
                        m_stormLayerSet.ClearAllLayers();
                        EditorUtility.SetDirty(m_stormLayerSet);
                    }
                }
                EditorGUI.EndDisabledGroup();
            }
            GUILayout.EndHorizontal();


            GUILayout.BeginHorizontal(GUI.skin.FindStyle("Toolbar"));

            EditorGUI.BeginChangeCheck();
            {
                searchString = GUILayout.TextField(searchString, GUI.skin.FindStyle("ToolbarSearchTextField"));
            }
            if (EditorGUI.EndChangeCheck())
            {
                sortedLayerIndexes.Clear();
                if (!string.IsNullOrEmpty(searchString))
                {
                    var layers = m_stormLayerSet.Layers;
                    for (int i = 0; i < layers.Count; i++)
                    {
                        string layerName = layers[i];
                        if (layerName.ToLower().Contains(searchString.ToLower()))
                        {
                            sortedLayerIndexes.Add(i);
                        }
                    }
                }

                RefreshList();
            }

            if (!string.IsNullOrEmpty(searchString))
            {
                if (GUILayout.Button("", GUI.skin.FindStyle("ToolbarSearchCancelButton")))
                {
                    sortedLayerIndexes.Clear();
                    // Remove focus if cleared
                    searchString = "";
                    GUI.FocusControl(null);
                }
            }

            GUILayout.EndHorizontal();

            layerList?.DoLayoutList();

            m_editorUtils.InlineHelp("Layers", helpEnabled);
        }
    }
}