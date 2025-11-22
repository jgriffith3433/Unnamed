using ProceduralWorlds.Storm.Core;
using ProceduralWorlds.Storm.Utils;
using PWCommon5;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEditorInternal;
using UnityEngine;

/// Copyright 2023 Procedural Worlds Ltd
/// Written by Jonathan Williams and Josh Savage as part of Storm

namespace ProceduralWorlds.Storm.StormEditor
{
    [CustomEditor(typeof(StormLayerRemap), true)]
    public class StormLayerRemapsEditor : PWEditor
    {

        EditorUtils m_editorUtils;
        SerializedProperty p_Layers;
        ReorderableList m_layersList;
        private void OnEnable()
        {
            p_Layers = serializedObject.FindProperty("Remaps");
            m_layersList = new ReorderableList(serializedObject, p_Layers, true, false, true, true);
            m_layersList.elementHeightCallback += GetLayerElementHeight;
            m_layersList.drawElementCallback += DrawLayerElement;
        }
        public override void OnInspectorGUI()
        {
            if (m_editorUtils == null)
                m_editorUtils = StormApp.GetEditorUtils(this);
            m_editorUtils.Initialize();

            m_layersList.DoLayoutList();

            serializedObject.ApplyModifiedProperties();
        }

        float GetLayerElementHeight(int index)
        {
            SerializedProperty layerData = p_Layers.GetArrayElementAtIndex(index);
            SerializedProperty sourceLayer = layerData.FindPropertyRelative("m_original");
            SerializedProperty remapLayers = layerData.FindPropertyRelative("m_remappedLayers");
            return EditorGUI.GetPropertyHeight(sourceLayer) + EditorGUI.GetPropertyHeight(remapLayers);
        }
        void DrawLayerElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            SerializedProperty layerData = p_Layers.GetArrayElementAtIndex(index);
            SerializedProperty sourceLayer = layerData.FindPropertyRelative("m_original");
            SerializedProperty remapLayers = layerData.FindPropertyRelative("m_remappedLayers");
            rect.height = EditorGUI.GetPropertyHeight(sourceLayer);
            EditorGUI.PropertyField(rect, sourceLayer);
            rect.y += rect.height;
            rect.height = EditorGUI.GetPropertyHeight(remapLayers);
            ++EditorGUI.indentLevel;
            EditorGUI.PropertyField(rect, remapLayers, true);
            --EditorGUI.indentLevel;
        }
    }
}