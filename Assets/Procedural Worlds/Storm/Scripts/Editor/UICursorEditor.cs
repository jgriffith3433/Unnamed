using UnityEngine;
using UnityEditor;

/// Copyright 2024 Procedural Worlds Ltd
/// Written by Jonathan Williams as part of Storm

namespace ProceduralWorlds.Storm.UI
{
    [CustomPropertyDrawer(typeof(UICursor.CursorSelection))]
    public class UICursorCursorSelectionDrawer : UnityEditor.PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (UICursor.Instance == null)
            {
                return EditorGUIUtility.GetBuiltinSkin(EditorSkin.Inspector).label.CalcHeight(label, EditorGUIUtility.currentViewWidth);
            }
            return (EditorGUIUtility.GetBuiltinSkin(EditorSkin.Inspector).label.CalcHeight(label, EditorGUIUtility.currentViewWidth) * 2) + 40f;
        }
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (UICursor.Instance == null)
            {
                GUI.Label(position, "No UICursor in scene.");
                return;
            }
            GUISkin skin = EditorGUIUtility.GetBuiltinSkin(EditorSkin.Inspector);
            float labelHeight = (skin.label.CalcHeight(label, EditorGUIUtility.currentViewWidth));
            SerializedProperty cursorIndexProp = property.FindPropertyRelative("cursor");
            SerializedProperty priorityProp = property.FindPropertyRelative("priority");
            string[] options = new string[(UICursor.Instance.Cursors == null ? 0 : UICursor.Instance.Cursors.Length) + 1];
            options[0] = "(Pointer)";
            for (int c = 0; c < UICursor.Instance.Cursors.Length; ++c)
            {
                options[c + 1] = UICursor.Instance.Cursors[c].Name;
            }
            cursorIndexProp.intValue = EditorGUI.Popup(new Rect(position.x, position.y, position.width, labelHeight), label.text, cursorIndexProp.intValue + 1, options) - 1;
            priorityProp.intValue = EditorGUI.IntField(new Rect(position.x, position.y + labelHeight, position.width, labelHeight), "Priority:", priorityProp.intValue);
            if (cursorIndexProp.intValue >= 0 && cursorIndexProp.intValue < UICursor.Instance.Cursors.Length && UICursor.Instance.Cursors[cursorIndexProp.intValue].cursor != null)
            {
                Texture2D cursorImage = UICursor.Instance.Cursors[cursorIndexProp.intValue].cursor;
                EditorGUI.LabelField(new Rect(position.x, position.yMax - 40f, position.width, 40f), new GUIContent("Selected Cursor:"), new GUIContent(cursorImage));
            }
            else
            {
                EditorGUI.LabelField(new Rect(position.x, position.yMax - 40f, position.width, 40f), new GUIContent("Selected Cursor:"), new GUIContent("None (Pointer)"));
            }
        }
    }
}