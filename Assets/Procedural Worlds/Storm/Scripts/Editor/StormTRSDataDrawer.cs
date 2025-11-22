using UnityEditor;
using UnityEngine;

namespace ProceduralWorlds.Storm.StormEditor
{
    [CustomPropertyDrawer(typeof(Utils.TRSData))]
    public class StormTRSDataDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (property.isExpanded)
            {
                SerializedProperty nProp = property.FindPropertyRelative("_layerName");
                float ret = EditorGUI.GetPropertyHeight(nProp) + (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing) * 2;
                SerializedProperty dProp = property.FindPropertyRelative("_data");
                if (dProp.objectReferenceValue != null)
                    ret += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                return ret;
            }
            return EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
        }
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            Rect r = position;
            r.height = EditorGUIUtility.singleLineHeight;
            SerializedProperty nProp = property.FindPropertyRelative("_layerName");
            string text;
            if (string.IsNullOrEmpty(nProp.stringValue))
                text = label.text;
            else
                text = nProp.stringValue;
            property.isExpanded = EditorGUI.Foldout(r, property.isExpanded, text, true);
            if (property.isExpanded)
            {
                SerializedProperty dProp = property.FindPropertyRelative("_data");
                r.y += r.height + EditorGUIUtility.standardVerticalSpacing;
                r.height = EditorGUI.GetPropertyHeight(nProp);
                EditorGUI.PropertyField(r, nProp);
                r.y += r.height + EditorGUIUtility.standardVerticalSpacing;
                r.height = EditorGUIUtility.singleLineHeight;
                EditorGUI.PropertyField(r, dProp);
                Utils.StormTRSList trsList = dProp.objectReferenceValue as Utils.StormTRSList;
                if (trsList != null)
                {
                    r.y += r.height + EditorGUIUtility.standardVerticalSpacing;
                    r.x += EditorGUIUtility.labelWidth;
                    r.width -= EditorGUIUtility.labelWidth;
                    GUI.Label(r, trsList.Transforms?.Length + " Instances.");
                }
            }
        }
    }
}