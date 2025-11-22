using ProceduralWorlds.Storm.Utils;
using PWCommon5;
using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEngine;

/// Copyright 2023 Procedural Worlds Ltd
/// Written by Jonathan Williams as part of Storm

namespace ProceduralWorlds.Storm.StormEditor
{
    [CustomEditor(typeof(Utils.StormTRSList))]
    public class StormTRSListEditor : PWEditor
    {
        private EditorUtils m_editorUtils;
        private SerializedProperty m_TransformListProperty;
        private SerializedProperty m_PreviewOffsetProperty;
        private SerializedProperty m_PreviewScaleProperty;
        private AnimBool[] m_transformItemExpanded;
        private GUIStyle m_footerLabelStyle;
        private Vector2 m_scrollPos;
        private int m_transformListOffset = 0;
        private void OnEnable()
        {
            m_editorUtils = StormApp.GetEditorUtils(this);
            m_TransformListProperty = serializedObject.FindProperty("Transforms");
            m_PreviewOffsetProperty = serializedObject.FindProperty("PreviewOffset");
            m_PreviewScaleProperty = serializedObject.FindProperty("PreviewScale");
            m_transformItemExpanded = new AnimBool[20];
            for (int i = 0; i < m_transformItemExpanded.Length; ++i)
                m_transformItemExpanded[i] = new AnimBool(false);
            m_transformListOffset = 0;
            m_footerLabelStyle = null;
            m_scrollPos = Vector2.zero;
            SceneView.duringSceneGui -= SceneViewDrawGizmos;
            SceneView.duringSceneGui += SceneViewDrawGizmos;
        }
        private void OnDisable()
        {
            SceneView.duringSceneGui -= SceneViewDrawGizmos;
            if (m_editorUtils != null)
            {
                m_editorUtils.Dispose();
            }
        }
        public override void OnInspectorGUI()
        {
            m_editorUtils.Initialize();
            if (m_footerLabelStyle == null)
            {
                m_footerLabelStyle = new GUIStyle(m_editorUtils.Styles.panelLabel);
                m_footerLabelStyle.alignment = TextAnchor.MiddleCenter;
            }
            m_TransformListProperty.arraySize = m_editorUtils.DelayedIntField("TransformList", m_TransformListProperty.arraySize);
            ++EditorGUI.indentLevel;
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            {
                for (int i = 0; i < 20 && i + m_transformListOffset < m_TransformListProperty.arraySize; ++i)
                {
                    EditorGUILayout.BeginVertical(m_editorUtils.Styles.box);
                    {
                        EditorGUILayout.BeginHorizontal();
                        {
                            if (GUILayout.Button(EditorGUIUtility.IconContent("Camera Gizmo"), GUILayout.Width(EditorGUIUtility.singleLineHeight * 1.5f), GUILayout.Height(EditorGUIUtility.singleLineHeight)))
                            {
                                SerializedProperty transformItem = m_TransformListProperty.GetArrayElementAtIndex(i + m_transformListOffset);
                                SerializedProperty translationProp = transformItem.FindPropertyRelative("Translation");
                                Vector3 offset = m_PreviewOffsetProperty.vector3Value;
                                Vector3 size = m_PreviewScaleProperty.vector3Value;
                                SceneView.lastActiveSceneView.pivot = offset + Vector3.Scale(translationProp.vector3Value, size);
                                SceneView.lastActiveSceneView.Repaint();
                            }
                            GUIContent labelContent = m_editorUtils.GetContent("TransformListItemHeader");
                            m_transformItemExpanded[i].target = EditorGUILayout.Foldout(m_transformItemExpanded[i].target, new GUIContent(labelContent.text + (i + m_transformListOffset + 1)), true);
                            if (m_editorUtils.ButtonRight("TransformListItemRemoveButton", GUILayout.ExpandWidth(false)))
                            {
                                m_TransformListProperty.DeleteArrayElementAtIndex(i + m_transformListOffset);
                            }
                        }
                        EditorGUILayout.EndHorizontal();
                        if (m_transformItemExpanded[i].value)
                        {
                            EditorGUILayout.BeginFadeGroup(m_transformItemExpanded[i].faded);
                            {
                                ++EditorGUI.indentLevel;
                                SerializedProperty transformItem = m_TransformListProperty.GetArrayElementAtIndex(i + m_transformListOffset);
                                SerializedProperty translationProp = transformItem.FindPropertyRelative("Translation");
                                SerializedProperty rotationProp = transformItem.FindPropertyRelative("Rotation");
                                SerializedProperty scaleProp = transformItem.FindPropertyRelative("Scale");
                                translationProp.vector3Value = m_editorUtils.Vector3Field("TransformListItemTranslation", translationProp.vector3Value);
                                rotationProp.quaternionValue = Quaternion.Euler(m_editorUtils.Vector3Field("TransformListItemRotation", rotationProp.quaternionValue.eulerAngles));
                                scaleProp.vector3Value = m_editorUtils.Vector3Field("TransformListItemScale", scaleProp.vector3Value);
                                --EditorGUI.indentLevel;
                            }
                            EditorGUILayout.EndFadeGroup();
                        }
                    }
                    EditorGUILayout.EndVertical();
                }
                --EditorGUI.indentLevel;
                EditorGUILayout.Space();
                EditorGUILayout.BeginHorizontal();
                {
                    EditorGUI.BeginDisabledGroup(m_transformListOffset <= 0);
                    if (m_editorUtils.Button("TransformListPrevButton", EditorStyles.miniButton, GUILayout.ExpandWidth(false)))
                    {
                        m_transformListOffset -= 20;
                    }
                    EditorGUI.EndDisabledGroup();
                    GUIContent labelContent = m_editorUtils.GetContent("TransformListFooter");
                    GUILayout.Label(string.Format(labelContent.text, m_transformListOffset + 1, Mathf.Min(m_transformListOffset + 20, m_TransformListProperty.arraySize), m_TransformListProperty.arraySize), m_footerLabelStyle);
                    EditorGUI.BeginDisabledGroup(m_transformListOffset + 20 >= m_TransformListProperty.arraySize);
                    if (m_editorUtils.Button("TransformListNextButton", EditorStyles.miniButton, GUILayout.ExpandWidth(false)))
                    {
                        m_transformListOffset += 20;
                    }
                    EditorGUI.EndDisabledGroup();
                }
                EditorGUILayout.EndHorizontal();
                m_scrollPos = EditorGUILayout.BeginScrollView(m_scrollPos);
                {
                    EditorGUILayout.BeginHorizontal();
                    for (int p = 0; p < Mathf.CeilToInt(m_TransformListProperty.arraySize / 20f); ++p)
                    {
                        bool selected = m_transformListOffset == p * 20;
                        EditorGUI.BeginDisabledGroup(selected);
                        if (GUILayout.Button((p + 1).ToString(), EditorStyles.miniButtonMid))
                        {
                            m_transformListOffset = p * 20;
                        }
                        EditorGUI.EndDisabledGroup();
                    }
                    EditorGUILayout.EndHorizontal();
                }
                EditorGUILayout.EndScrollView();
            }
            EditorGUILayout.EndVertical();
            m_editorUtils.PropertyField("PreviewOffset", m_PreviewOffsetProperty);
            m_editorUtils.PropertyField("PreviewScale", m_PreviewScaleProperty);
            serializedObject.ApplyModifiedProperties();

            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            bool doExport = m_editorUtils.Button("ExportButton");
            bool doImport = m_editorUtils.Button("ImportButton");
            EditorGUILayout.EndHorizontal();

            if (doExport)
            {
                foreach (StormTRSList item in targets)
                {
                    GUIContent dialogContent = m_editorUtils.GetContent("ExportDialog");
                    string path = EditorUtility.SaveFilePanel(string.Format(dialogContent.text, item.name), Application.dataPath, item.name + ".bytes", "bytes");
                    if (!string.IsNullOrEmpty(path))
                    {
                        item.Save(path);
                    }
                }
                AssetDatabase.Refresh();
            }
            if (doImport)
            {
                foreach (StormTRSList item in targets)
                {
                    GUIContent dialogContent = m_editorUtils.GetContent("ImportDialog");
                    string path = EditorUtility.OpenFilePanel(string.Format(dialogContent.text, item.name), Application.dataPath, "bytes");
                    if (!string.IsNullOrEmpty(path))
                    {
                        Undo.RecordObject(item, "Loaded binary data");
                        if (item.Load(path))
                        {
                            EditorUtility.SetDirty(item);
                        }
                    }
                }
                serializedObject.Update();
            }
        }
        public override bool RequiresConstantRepaint()
        {
            if (m_transformItemExpanded != null)
            {
                for (int i = 0; i < m_transformItemExpanded.Length; ++i)
                {
                    if (m_transformItemExpanded[i].isAnimating)
                        return true;
                }
            }
            return base.RequiresConstantRepaint();
        }

        private void SceneViewDrawGizmos(SceneView obj)
        {
            if (m_TransformListProperty == null)
                return;
            Vector3 offset = m_PreviewOffsetProperty.vector3Value;
            Vector3 size = m_PreviewScaleProperty.vector3Value;
            for (int i = 0; i < 20 && i + m_transformListOffset < m_TransformListProperty.arraySize; ++i)
            {
                if (m_transformItemExpanded[i].value)
                {
                    SerializedProperty transformItem = m_TransformListProperty.GetArrayElementAtIndex(i + m_transformListOffset);
                    SerializedProperty translationProp = transformItem.FindPropertyRelative("Translation");
                    SerializedProperty rotationProp = transformItem.FindPropertyRelative("Rotation");
                    SerializedProperty scaleProp = transformItem.FindPropertyRelative("Scale");
                    EditorGUI.BeginChangeCheck();
                    Vector3 pos = offset + Vector3.Scale(translationProp.vector3Value, size);
                    Quaternion rot = rotationProp.quaternionValue;
                    Vector3 scale = scaleProp.vector3Value;
                    Handles.TransformHandle(ref pos, ref rot, ref scale);
                    if (EditorGUI.EndChangeCheck())
                    {
                        translationProp.vector3Value = Vector3.Scale(pos - offset, new Vector3(1 / size.x, 1 / size.y, 1 / size.z));
                        rotationProp.vector3Value = rot.eulerAngles;
                        scaleProp.vector3Value = scale;
                        serializedObject.ApplyModifiedProperties();
                    }
                }
            }
        }
    }
}