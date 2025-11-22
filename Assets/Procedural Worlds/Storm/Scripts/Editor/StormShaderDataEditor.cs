using ProceduralWorlds.Storm.Utils;
using PWCommon5;
using UnityEditor;
using UnityEngine;

/// Copyright 2023 Procedural Worlds Ltd
/// Written by Jonathan Williams and Josh Savage as part of Storm

namespace ProceduralWorlds.Storm.StormEditor
{
    [CustomEditor(typeof(StormShaderData), true)]
    public class StormShaderDataEditor : PWEditor
    {
        private EditorUtils m_editorUtils;
        SerializedProperty p_unityVersion;
        SerializedProperty p_isCustom;
        SerializedProperty p_converterVersion;

        string customPath = "";
        int PerformOperation = -1;

        public void OnEnable()
        {
            StormUtils.GetRenderPipline();
            m_editorUtils = StormApp.GetEditorUtils(this);

            p_unityVersion = serializedObject.FindProperty("ShaderVersion");
            p_isCustom = serializedObject.FindProperty("IsCustom");
            p_converterVersion = serializedObject.FindProperty("ShaderConverterVersion");

            if (p_isCustom.boolValue)
            {
                customPath = p_unityVersion.stringValue;
            }
        }

        public override void OnInspectorGUI()
        {
            if (m_editorUtils == null)
                m_editorUtils = StormApp.GetEditorUtils(this);
            m_editorUtils.Initialize();

            m_editorUtils.Panel("ShaderPanel", ShaderPanel, true);
            m_editorUtils.Panel("UtilsPanel", UtilsPanel, true);

            serializedObject.ApplyModifiedProperties();
            if (PerformOperation == 1)
                DoBrowse();
            else if (PerformOperation == 2)
                DoConvert();
            else if (PerformOperation == 3)
                DoConvertCustom();
        }

        public void ShaderPanel(bool helpActive)
        {
            if (p_isCustom.boolValue)
            {
                m_editorUtils.PropertyField("CustomPath", p_unityVersion, helpActive);
                m_editorUtils.PropertyField("ConverterVersion", p_converterVersion, helpActive);
            }
            else
            {
                m_editorUtils.PropertyField("UnityVersion", p_unityVersion, helpActive);
                m_editorUtils.PropertyField("ConverterVersion", p_converterVersion, helpActive);
            }
        }
        public void UtilsPanel(bool helpActive)
        {
            if (m_editorUtils.Button("UpdateButton", GUILayout.ExpandWidth(true)))
            {
                PerformOperation = 2;
            }
            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            customPath = GUILayout.TextField(customPath, GUILayout.ExpandWidth(true));

            if (m_editorUtils.Button("BrowseButton", GUILayout.ExpandWidth(false)))
            {
                PerformOperation = 1;
            }
            EditorGUILayout.EndHorizontal();
            EditorGUI.BeginDisabledGroup(string.IsNullOrEmpty(customPath));
            if (m_editorUtils.Button("ConvertCustom", GUILayout.ExpandWidth(true)))
            {
                PerformOperation = 3;
            }
            EditorGUI.EndDisabledGroup();
        }

        void DoBrowse()
        {
            PerformOperation = -1;
            string path = EditorUtility.OpenFilePanelWithFilters(m_editorUtils.GetContent("BrowseDialog").text, "", new string[] { "Zip files", "zip", "All Files", "*" });
            if (!string.IsNullOrEmpty(path))
            {
                customPath = path;
            }
        }
        void DoConvert()
        {
            PerformOperation = -1;
            p_isCustom.boolValue = false;
            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDialogOptOutDecision(DialogOptOutDecisionType.ForThisMachine, "StormStandardShadersIgnore-" + Application.unityVersion, false);
            StormShaderConverter.UpdateStandardShaders(true);
        }
        void DoConvertCustom()
        {
            PerformOperation = -1;
            StormShaderConverter.UpdateStandardShaders(customPath);
        }
    }
}