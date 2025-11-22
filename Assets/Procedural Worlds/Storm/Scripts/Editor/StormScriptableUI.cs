using ProceduralWorlds.Storm.Utils;
using PWCommon5;
using System;
using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEngine;
using UnityEngine.Events;

namespace ProceduralWorlds.Storm.StormEditor
{
    public class StormScriptableUI<T> where T : ScriptableObject
    {
        SerializedProperty targetProperty;
        T targetObject;
        Editor m_editor;
        AnimBool m_editorExpanded;
        AnimBool m_toolsExpanded;
        string m_key;
        EditorUtils m_editorUtils;
        Action<T> OnChanged;
        Action CustomButtons;
        Action<bool> OnExpandContract;
        bool m_showToolsButton;
        GUIContent m_toolsButton;
        float m_editorGUIHeight;
        Vector2 m_editorScroll;
        bool m_isRenaming;
        string m_newName;

        public T Target
        {
            get => targetObject;
            set {
                if (targetObject != value)
                {
                    targetObject = value;
                    if (targetProperty != null)
                        targetProperty.objectReferenceValue = value;
                }
            }
        }

        public bool IsExpanded
        {
            get => m_editorExpanded.target;
            set {
                if (value != m_editorExpanded.target)
                {
                    m_editorExpanded.target = value;
                    OnExpandContract?.Invoke(value);
                }
            }
        }

        /// <summary>
        /// Create Scriptable UI with a reference
        /// </summary>
        /// <param name="target">Current value of Object</param>
        /// <param name="key">Key to use for label</param>
        /// <param name="repaint">Action to invoke when AnimBools update</param>
        /// <param name="editorUtils">EditorUtils to use for labels and styles</param>
        /// <param name="onChanged">Action to invoke when target changes</param>
        /// <param name="showToolsButton">Should we show tool buttons? (create/rename/save asset buttons)</param>
        /// <param name="customButtons">Action to invoke when drawing custom button area (put your GUILayout controls here)</param>
        /// <param name="startExpanded">Should imbedded Editor start expanded?</param>
        /// <param name="GUIEditorHeight">Height of window to use when embedding an Editor in GUI function</param>
        public StormScriptableUI(T target, string key, Action repaint, EditorUtils editorUtils, Action<T> onChanged, bool showToolsButton = true, Action customButtons = null, bool startExpanded = false, float GUIEditorHeight = 250)
        {
            targetObject = target;
            UnityAction repaintAction = new UnityAction(repaint);
            m_editorExpanded = new AnimBool(startExpanded, repaintAction);
            m_showToolsButton = showToolsButton;
            m_toolsExpanded = new AnimBool(false, repaintAction);
            m_key = key;
            m_editorUtils = editorUtils;
            OnChanged = onChanged;
            CustomButtons = customButtons;
            m_toolsButton = EditorGUIUtility.IconContent("d_CustomTool@2x");
            m_editorGUIHeight = GUIEditorHeight;
        }
        /// <summary>
        /// Create Scriptable UI with a reference
        /// </summary>
        /// <param name="target">Current value of Object</param>
        /// <param name="key">Key to use for label</param>
        /// <param name="repaint">Action to invoke when AnimBools update</param>
        /// <param name="editorUtils">EditorUtils to use for labels and styles</param>
        /// <param name="onChanged">Action invoked when target changes</param>
        /// <param name="onExpandContract">Action invoked when the view is expanded or contracted</param>
        /// <param name="showToolsButton">Should we show tool buttons? (create/rename/save asset buttons)</param>
        /// <param name="customButtons">Action to invoke when drawing custom button area (put your GUILayout controls here)</param>
        /// <param name="startExpanded">Should imbedded Editor start expanded?</param>
        /// <param name="GUIEditorHeight">Height of window to use when embedding an Editor in GUI function</param>
        public StormScriptableUI(T target, string key, Action repaint, EditorUtils editorUtils, Action<T> onChanged, Action<bool> onExpandContract, bool showToolsButton = true, Action customButtons = null, bool startExpanded = false, float GUIEditorHeight = 250)
        {
            targetObject = target;
            UnityAction repaintAction = new UnityAction(repaint);
            m_editorExpanded = new AnimBool(startExpanded, repaintAction);
            m_showToolsButton = showToolsButton;
            m_toolsExpanded = new AnimBool(false, repaintAction);
            m_key = key;
            m_editorUtils = editorUtils;
            OnChanged = onChanged;
            OnExpandContract = onExpandContract;
            CustomButtons = customButtons;
            m_toolsButton = EditorGUIUtility.IconContent("d_CustomTool@2x");
            m_editorGUIHeight = GUIEditorHeight;
        }
        /// <summary>
        /// Create Scriptable UI with a SerializedProperty
        /// </summary>
        /// <param name="prop">Property of Object</param>
        /// <param name="key">Key to use for label</param>
        /// <param name="repaint">Action to invoke when AnimBools update</param>
        /// <param name="editorUtils">EditorUtils to use for labels and styles</param>
        /// <param name="onChanged">Action to invoke when target changes</param>
        /// <param name="showToolsButton">Should we show tool buttons? (create/rename/save asset buttons)</param>
        /// <param name="customButtons">Action to invoke when drawing custom button area (put your GUILayout controls here)</param>
        /// <param name="startExpanded">Should imbedded Editor start expanded?</param>
        /// <param name="GUIEditorHeight">Height of window to use when embedding an Editor in GUI function</param>
        public StormScriptableUI(SerializedProperty prop, string key, Action repaint, EditorUtils editorUtils, Action<T> onChanged = null, bool showToolsButton = true, Action customButtons = null, bool startExpanded = false, float GUIEditorHeight = 250)
        {
            targetProperty = prop;
            targetObject = prop.objectReferenceValue as T;
            UnityAction repaintAction = new UnityAction(repaint);
            targetProperty.isExpanded = startExpanded;
            m_editorExpanded = new AnimBool(startExpanded, repaintAction);
            m_showToolsButton = showToolsButton;
            m_toolsExpanded = new AnimBool(false, repaintAction);
            m_key = key;
            m_editorUtils = editorUtils;
            OnChanged = onChanged;
            CustomButtons = customButtons;
            m_toolsButton = EditorGUIUtility.IconContent("d_CustomTool@2x");
            m_editorGUIHeight = GUIEditorHeight;
        }
        /// <summary>
        /// Create Scriptable UI with a SerializedProperty
        /// </summary>
        /// <param name="prop">Property of Object</param>
        /// <param name="key">Key to use for label</param>
        /// <param name="repaint">Action to invoke when AnimBools update</param>
        /// <param name="editorUtils">EditorUtils to use for labels and styles</param>
        /// <param name="onChanged">Action to invoke when target changes</param>
        /// <param name="showToolsButton">Should we show tool buttons? (create/rename/save asset buttons)</param>
        /// <param name="customButtons">Action to invoke when drawing custom button area (put your GUILayout controls here)</param>
        /// <param name="GUIEditorHeight">Height of window to use when embedding an Editor in GUI function</param>
        public StormScriptableUI(SerializedProperty prop, string key, Action repaint, EditorUtils editorUtils, Action<T> onChanged = null, bool showToolsButton = true, Action customButtons = null, float GUIEditorHeight = 250)
        {
            targetProperty = prop;
            targetObject = prop.objectReferenceValue as T;
            UnityAction repaintAction = new UnityAction(repaint);
            m_editorExpanded = new AnimBool(targetProperty.isExpanded, repaintAction);
            m_showToolsButton = showToolsButton;
            m_toolsExpanded = new AnimBool(false, repaintAction);
            m_key = key;
            m_editorUtils = editorUtils;
            OnChanged = onChanged;
            CustomButtons = customButtons;
            m_toolsButton = EditorGUIUtility.IconContent("d_CustomTool@2x");
            m_editorGUIHeight = GUIEditorHeight;
        }

        public void DoLayout()
        {
            EditorGUILayout.BeginVertical(m_editorUtils.Styles.panelFrame);
            {
                EditorGUILayout.BeginHorizontal();
                {
                    bool status = m_editorExpanded.target;
                    status = GUILayout.Toggle(status, status ? "-" : "+", m_editorUtils.Styles.panelLabel, GUILayout.ExpandWidth(false));
                    if (!string.IsNullOrEmpty(m_key))
                    {
                        Rect r = GUILayoutUtility.GetLastRect();
                        GUILayout.Space(-5f);
                        status = GUILayout.Toggle(status, m_editorUtils.GetContent(m_key), m_editorUtils.Styles.panelLabel, GUILayout.Width(EditorGUIUtility.labelWidth - r.width));
                    }
                    if (status != m_editorExpanded.target)
                    {
                        m_editorExpanded.target = status;
                        OnExpandContract?.Invoke(status);
                    }
                    if (targetProperty != null)
                    {
                        targetProperty.isExpanded = m_editorExpanded.target;
                        EditorGUI.BeginDisabledGroup(OnChanged == null);
                        targetProperty.objectReferenceValue = EditorGUILayout.ObjectField(targetProperty.objectReferenceValue, typeof(T), false, GUILayout.ExpandWidth(true));
                        EditorGUI.EndDisabledGroup();
                        if (targetObject != targetProperty.objectReferenceValue)
                        {
                            targetObject = targetProperty.objectReferenceValue as T;
                            OnChanged?.Invoke(targetObject);
                            if (m_isRenaming)
                                m_newName = targetObject.name;
                        }
                    }
                    else
                    {
                        T originalTarget = targetObject;
                        EditorGUI.BeginDisabledGroup(OnChanged == null);
                        targetObject = EditorGUILayout.ObjectField(targetObject, typeof(T), false, GUILayout.ExpandWidth(true)) as T;
                        EditorGUI.EndDisabledGroup();
                        if (targetObject != originalTarget)
                        {
                            OnChanged?.Invoke(targetObject);
                            if (m_isRenaming)
                                m_newName = targetObject.name;
                        }
                    }
                    CustomButtons?.Invoke();
                    if (m_showToolsButton)
                        m_toolsExpanded.target = GUILayout.Toggle(m_toolsExpanded.target, m_toolsButton, m_editorUtils.Styles.helpToggle, GUILayout.ExpandWidth(false));
                }
                EditorGUILayout.EndHorizontal();
                if (EditorGUILayout.BeginFadeGroup(m_toolsExpanded.faded) && m_showToolsButton)
                {
                    if (m_isRenaming)
                    {
                        EditorGUILayout.BeginHorizontal();
                        {
                            m_newName = EditorGUILayout.TextField(m_newName, GUILayout.ExpandWidth(true));
                            bool disable = false;
                            string path = StormUtils.GetAssetPath(targetObject);
                            if (!string.IsNullOrEmpty(path))
                            {
                                string newPath = path.Substring(0, System.IO.Path.GetFileName(path).Length) + m_newName + ".asset";
                                if (System.IO.File.Exists(newPath))
                                    disable = true;
                            }
                            EditorGUI.BeginDisabledGroup(disable);
                            if (m_editorUtils.Button("Scriptable_RenameButton", EditorGUIUtility.currentViewWidth < 300 ? EditorStyles.miniButtonRight : EditorStyles.miniButtonMid, GUILayout.ExpandWidth(false)))
                            {
                                if (!string.IsNullOrEmpty(path))
                                {
                                    string newPath = m_newName + ".asset";
                                    string result = AssetDatabase.RenameAsset(path, newPath);
                                    if (!string.IsNullOrEmpty(result))
                                        EditorUtility.DisplayDialog("Error Renaming Asset", result, "Ok");
                                    else
                                        m_isRenaming = false;
                                }
                                else
                                {
                                    targetObject.name = m_newName;
                                    m_isRenaming = false;
                                }
                            }
                            EditorGUI.EndDisabledGroup();
                            if (EditorGUIUtility.currentViewWidth < 300)
                            {
                                EditorGUILayout.EndHorizontal();
                                EditorGUILayout.BeginHorizontal();
                            }
                            if (m_editorUtils.Button("Scriptable_CancelButton", EditorGUIUtility.currentViewWidth < 300 ? EditorStyles.miniButton : EditorStyles.miniButtonRight, GUILayout.ExpandWidth(false)))
                            {
                                m_isRenaming = false;
                                EditorGUIUtility.hotControl = -1;
                                EditorGUIUtility.editingTextField = false;
                            }
                        }
                        EditorGUILayout.EndHorizontal();
                    }
                    else
                    {
                        EditorGUILayout.BeginHorizontal();
                        {
                            EditorGUI.BeginDisabledGroup(targetObject == null);
                            if (m_editorUtils.Button("Scriptable_SaveAsButton", EditorGUIUtility.currentViewWidth < 300 ? EditorStyles.miniButton : EditorStyles.miniButtonLeft))
                            {
                                GUIContent headerContent = m_editorUtils.GetContent("Scriptable_SaveAsHeader");
                                string path = StormUtils.GetAssetPath(targetObject);
                                if (string.IsNullOrEmpty(path))
                                    path = StormUtils.SaveFilePanelInProject(headerContent.text, targetObject.name, "asset", headerContent.tooltip);
                                else
                                    path = StormUtils.SaveFilePanelInProject(headerContent.text, targetObject.name, "asset", headerContent.tooltip, System.IO.Path.GetDirectoryName(path));
                                if (!string.IsNullOrEmpty(path))
                                {
                                    AssetDatabase.CreateAsset(targetObject, path);
                                    targetObject = AssetDatabase.LoadAssetAtPath<T>(path);
                                    if (targetProperty != null)
                                        targetProperty.objectReferenceValue = targetObject;
                                    OnChanged?.Invoke(targetObject);
                                    GUIUtility.ExitGUI();
                                }
                            }
                            if (EditorGUIUtility.currentViewWidth < 300)
                            {
                                EditorGUI.EndDisabledGroup();
                                EditorGUILayout.EndHorizontal();
                                EditorGUILayout.BeginHorizontal();
                                EditorGUI.BeginDisabledGroup(targetObject == null);
                            }
                            if (m_editorUtils.Button("Scriptable_RenameButton", EditorGUIUtility.currentViewWidth < 300 ? EditorStyles.miniButton : (EditorGUIUtility.currentViewWidth < 400 ? EditorStyles.miniButtonRight : EditorStyles.miniButtonMid)))
                            {
                                m_isRenaming = true;
                                m_newName = targetObject.name;
                            }
                            EditorGUI.EndDisabledGroup();
                            if (EditorGUIUtility.currentViewWidth < 400)
                            {
                                EditorGUILayout.EndHorizontal();
                                EditorGUILayout.BeginHorizontal();
                            }
                            if (m_editorUtils.Button("Scriptable_NewAssetButton", EditorGUIUtility.currentViewWidth < 300 ? EditorStyles.miniButton : (EditorGUIUtility.currentViewWidth < 400 ? EditorStyles.miniButtonLeft : EditorStyles.miniButtonMid)))
                            {
                                GUIContent headerContent = m_editorUtils.GetContent("Scriptable_SaveAsHeader");
                                string path = Utils.StormUtils.SaveFilePanelInProject(headerContent.text, typeof(T).Name, "asset", headerContent.tooltip);
                                if (!string.IsNullOrEmpty(path))
                                {
                                    targetObject = ScriptableObject.CreateInstance<T>();
                                    AssetDatabase.CreateAsset(targetObject, path);
                                    targetObject = AssetDatabase.LoadAssetAtPath<T>(path);
                                    if (targetProperty != null)
                                        targetProperty.objectReferenceValue = targetObject;
                                    OnChanged?.Invoke(targetObject);
                                    GUIUtility.ExitGUI();
                                }
                            }
                            if (EditorGUIUtility.currentViewWidth < 300)
                            {
                                EditorGUILayout.EndHorizontal();
                                EditorGUILayout.BeginHorizontal();
                            }
                            if (m_editorUtils.Button("Scriptable_NewEmbedButton", EditorGUIUtility.currentViewWidth < 300 ? EditorStyles.miniButton : EditorStyles.miniButtonRight))
                            {
                                targetObject = ScriptableObject.CreateInstance<T>();
                                if (targetProperty != null)
                                    targetProperty.objectReferenceValue = targetObject;
                                OnChanged?.Invoke(targetObject);
                            }
                        }
                        EditorGUILayout.EndHorizontal();
                    }
                }
                EditorGUILayout.EndFadeGroup();
                if (EditorGUILayout.BeginFadeGroup(m_editorExpanded.faded))
                {
                    if (targetObject == null)
                    {
                        m_editor = null;
                    }
                    else
                    {
                        if (m_editor == null || m_editor.target != targetObject)
                        {
                            m_editor = null;
                            m_editor = Editor.CreateEditor(targetObject);
                        }
                        m_editor.OnInspectorGUI();
                    }
                }
                EditorGUILayout.EndFadeGroup();
            }
            EditorGUILayout.EndVertical();
        }
        /*
        public float GetGUIHeight()
        {
            return ((1 + m_toolsExpanded.faded) * (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing)) + (m_editorExpanded.faded * m_editorGUIHeight);
        }
        public void DoGUI(Rect area)
        {
            GUI.Box(area, "", m_editorUtils.Styles.panelFrame);
            Rect lineRect = area;
            lineRect.height = EditorGUIUtility.singleLineHeight;
            if (string.IsNullOrEmpty(m_key))
            {
                lineRect.width = 20;
                m_editorExpanded.target = GUI.Toggle(lineRect, m_editorExpanded.target, m_editorExpanded.target ? "-" : "+", m_editorUtils.Styles.panelLabel);
            }
            else
            {
                lineRect.width = EditorGUIUtility.labelWidth;
                m_editorExpanded.target = GUI.Toggle(lineRect, m_editorExpanded.target, m_editorExpanded.target ? "-" : "+", m_editorUtils.Styles.panelLabel);
                lineRect.x += 15;
                lineRect.width -= 15;
                GUI.Label(lineRect, m_editorUtils.GetContent(m_key), m_editorUtils.Styles.panelLabel);
            }
            lineRect.x = lineRect.xMax;
            lineRect.width = area.width - lineRect.x - 20;
            T newObject = EditorGUI.ObjectField(lineRect, targetObject, typeof(T), false) as T;
            if (newObject != targetObject)
            {
                targetObject = newObject;
                OnChanged.Invoke(targetObject);
            }
            lineRect.x += lineRect.width;
            lineRect.width = 20;
            m_toolsExpanded.target = GUI.Toggle(lineRect, m_toolsExpanded.target, m_toolsButton, m_editorUtils.Styles.helpToggle);
            if (m_toolsExpanded.faded > 0)
            {
                lineRect.x = area.x;
                lineRect.width = area.width;
                lineRect.y += lineRect.height + EditorGUIUtility.singleLineHeight;

                lineRect.height *= m_toolsExpanded.faded;
                lineRect.width /= 3;
                EditorGUI.BeginDisabledGroup(targetObject == null);
                if (GUI.Button(lineRect, m_editorUtils.GetContent("Scriptable_SaveAsButton"), EditorStyles.miniButtonLeft))
                {
                    GUIContent headerContent = m_editorUtils.GetContent("Scriptable_SaveAsHeader");
                    string path = StormUtils.GetAssetPath(targetObject);
                    if (string.IsNullOrEmpty(path))
                        path = StormUtils.SaveFilePanelInProject(headerContent.text, targetObject.name, "asset", headerContent.tooltip);
                    else
                        path = StormUtils.SaveFilePanelInProject(headerContent.text, targetObject.name, "asset", headerContent.tooltip, System.IO.Path.GetDirectoryName(path));
                    if (!string.IsNullOrEmpty(path))
                    {
                        AssetDatabase.CreateAsset(targetObject, path);
                        targetObject = AssetDatabase.LoadAssetAtPath<T>(path);
                        OnChanged.Invoke(targetObject);
                        GUIUtility.ExitGUI();
                    }
                }
                EditorGUI.EndDisabledGroup();
                lineRect.x += lineRect.width;
                if (GUI.Button(lineRect, m_editorUtils.GetContent("Scriptable_NewAssetButton"), EditorStyles.miniButtonMid))
                {
                    GUIContent headerContent = m_editorUtils.GetContent("Scriptable_SaveAsHeader");
                    string path = Utils.StormUtils.SaveFilePanelInProject(headerContent.text, targetObject.name, "asset", headerContent.tooltip);
                    if (!string.IsNullOrEmpty(path))
                    {
                        targetObject = ScriptableObject.CreateInstance<T>();
                        AssetDatabase.CreateAsset(targetObject, path);
                        targetObject = AssetDatabase.LoadAssetAtPath<T>(path);
                        OnChanged.Invoke(targetObject);
                        GUIUtility.ExitGUI();
                    }
                }
                lineRect.x += lineRect.width;
                if (GUI.Button(lineRect, m_editorUtils.GetContent("Scriptable_NewEmbedButton"), EditorStyles.miniButtonRight))
                {
                    targetObject = ScriptableObject.CreateInstance<T>();
                    OnChanged.Invoke(targetObject);
                }
            }
            if (m_editorExpanded.faded > 0)
            {
                lineRect.x = area.x;
                lineRect.width = area.width;
                lineRect.y += lineRect.height + EditorGUIUtility.singleLineHeight;

                lineRect.height = m_editorGUIHeight * m_editorExpanded.faded;
                m_editorScroll = GUI.BeginScrollView(lineRect, m_editorScroll, Rect.zero);

                if (targetObject == null)
                {
                    m_editor = null;
                    return;
                }
                if (m_editor == null || m_editor.target != targetObject)
                {
                    m_editor = null;
                    m_editor = Editor.CreateEditor(targetObject);
                }
                m_editor.OnInspectorGUI();

                GUI.EndScrollView();
            }
        }
        //*/
    }
}