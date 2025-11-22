using UnityEditor;
using UnityEngine;

namespace ProceduralWorlds.Storm.StormEditor
{
    [CustomPropertyDrawer(typeof(Utils.TextureData))]
    public class StormTextureDataDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (property.isExpanded)
            {
                float ret = (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing) * 9 + EditorGUIUtility.standardVerticalSpacing;
                SerializedProperty rName = property.FindPropertyRelative("_layerNameR");
                SerializedProperty gName = property.FindPropertyRelative("_layerNameG");
                SerializedProperty bName = property.FindPropertyRelative("_layerNameB");
                SerializedProperty aName = property.FindPropertyRelative("_layerNameA");
                ret += EditorGUI.GetPropertyHeight(rName) + EditorGUI.GetPropertyHeight(gName) + EditorGUI.GetPropertyHeight(bName) + EditorGUI.GetPropertyHeight(aName);
                SerializedProperty tProp = property.FindPropertyRelative("_texture");
                if (tProp.isExpanded)
                    ret += 128;
                return ret;
            }
            return EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
        }
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            int guiIndent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;
            Rect r = position;
            r.height = EditorGUIUtility.singleLineHeight;
            string text = "";
            SerializedProperty rName = property.FindPropertyRelative("_layerNameR");
            SerializedProperty gName = property.FindPropertyRelative("_layerNameG");
            SerializedProperty bName = property.FindPropertyRelative("_layerNameB");
            SerializedProperty aName = property.FindPropertyRelative("_layerNameA");
            bool isFirstName = true;
            if (!string.IsNullOrEmpty(rName.stringValue))
            {
                if (isFirstName)
                    isFirstName = false;
                else
                    text += ", ";
                text += rName.stringValue;
            }
            if (!string.IsNullOrEmpty(gName.stringValue))
            {
                if (isFirstName)
                    isFirstName = false;
                else
                    text += ", ";
                text += gName.stringValue;
            }
            if (!string.IsNullOrEmpty(bName.stringValue))
            {
                if (isFirstName)
                    isFirstName = false;
                else
                    text += ", ";
                text += bName.stringValue;
            }
            if (!string.IsNullOrEmpty(aName.stringValue))
            {
                if (isFirstName)
                    isFirstName = false;
                else
                    text += ", ";
                text += aName.stringValue;
            }

            if (isFirstName)
                text = label.text;

            property.isExpanded = EditorGUI.Foldout(r, property.isExpanded, text, true);
            if (property.isExpanded)
            {
                SerializedProperty redPriority = property.FindPropertyRelative("_priorityR");
                SerializedProperty greenPriority = property.FindPropertyRelative("_priorityG");
                SerializedProperty bluePriority = property.FindPropertyRelative("_priorityB");
                SerializedProperty alphaPriority = property.FindPropertyRelative("_priorityA");
                SerializedProperty redRange = property.FindPropertyRelative("_valueRangeR");
                SerializedProperty greenRange = property.FindPropertyRelative("_valueRangeG");
                SerializedProperty blueRange = property.FindPropertyRelative("_valueRangeB");
                SerializedProperty alphaRange = property.FindPropertyRelative("_valueRangeA");
                SerializedProperty rectProp = property.FindPropertyRelative("_rect");
                SerializedProperty tProp = property.FindPropertyRelative("_texture");

                r.y += r.height + EditorGUIUtility.standardVerticalSpacing;
                r.x += EditorGUIUtility.labelWidth;
                float LayerWidth = position.width * 0.75f;
                float PriorityWidth = position.width - LayerWidth;
                r.width = LayerWidth - EditorGUIUtility.labelWidth;
                Rect PriorityRect = r;
                PriorityRect.x += r.width;
                PriorityRect.width = PriorityWidth;
                GUI.Label(r, new GUIContent("Layer Name"));
                GUI.Label(PriorityRect, new GUIContent("Priority"));

                r.y += r.height;
                PriorityRect.y = r.y;
                r.x = position.x;
                r.width = LayerWidth;
                r.height = EditorGUI.GetPropertyHeight(rName);
                EditorGUI.PropertyField(r, rName);
                EditorGUI.BeginChangeCheck();
                int rpVal = Mathf.Clamp(EditorGUI.IntField(PriorityRect, redPriority.intValue), 0, 255);
                if (EditorGUI.EndChangeCheck())
                    redPriority.intValue = rpVal;
                r.y += r.height;
                r.x = position.x;
                r.width = position.width;
                EditorGUI.PropertyField(r, redRange);

                r.y += r.height + EditorGUIUtility.standardVerticalSpacing;
                r.width = LayerWidth;
                PriorityRect.y = r.y;
                r.height = EditorGUI.GetPropertyHeight(gName);
                EditorGUI.PropertyField(r, gName);
                EditorGUI.BeginChangeCheck();
                int gpVal = Mathf.Clamp(EditorGUI.IntField(PriorityRect, greenPriority.intValue), 0, 255);
                if (EditorGUI.EndChangeCheck())
                    greenPriority.intValue = gpVal;
                r.y += r.height;
                r.x = position.x;
                r.width = position.width;
                EditorGUI.PropertyField(r, greenRange);

                r.y += r.height + EditorGUIUtility.standardVerticalSpacing;
                r.width = LayerWidth;
                PriorityRect.y = r.y;
                r.height = EditorGUI.GetPropertyHeight(bName);
                EditorGUI.PropertyField(r, bName);
                EditorGUI.BeginChangeCheck();
                int bpVal = Mathf.Clamp(EditorGUI.IntField(PriorityRect, bluePriority.intValue), 0, 255);
                if (EditorGUI.EndChangeCheck())
                    bluePriority.intValue = bpVal;
                r.y += r.height;
                r.x = position.x;
                r.width = position.width;
                EditorGUI.PropertyField(r, blueRange);

                r.y += r.height + EditorGUIUtility.standardVerticalSpacing;
                r.width = LayerWidth;
                PriorityRect.y = r.y;
                r.height = EditorGUI.GetPropertyHeight(aName);
                EditorGUI.PropertyField(r, aName);
                EditorGUI.BeginChangeCheck();
                int apVal = Mathf.Clamp(EditorGUI.IntField(PriorityRect, alphaPriority.intValue), 0, 255);
                if (EditorGUI.EndChangeCheck())
                    alphaPriority.intValue = apVal;
                r.y += r.height;
                r.x = position.x;
                r.width = position.width;
                EditorGUI.PropertyField(r, alphaRange);

                r.y += r.height + EditorGUIUtility.standardVerticalSpacing;
                r.width = LayerWidth;
                r.height = EditorGUIUtility.standardVerticalSpacing + (EditorGUIUtility.singleLineHeight * 2);
                r.width = position.width;
                if (rectProp.rectValue == Rect.zero)
                    rectProp.rectValue = new Rect(0, 0, 1, 1);
                EditorGUI.PropertyField(r, rectProp, new GUIContent("Rect"));

                r.y += r.height + EditorGUIUtility.standardVerticalSpacing;
                r.width = LayerWidth;
                PriorityRect.y = r.y;
                r.height = EditorGUIUtility.singleLineHeight;
                EditorGUI.PropertyField(r, tProp);
                bool isValidTexture = tProp.objectReferenceValue != null;
                EditorGUI.BeginDisabledGroup(!isValidTexture);
                tProp.isExpanded = GUI.Toggle(PriorityRect, tProp.isExpanded, "Preview", EditorStyles.miniButton);
                r.y += r.height + EditorGUIUtility.standardVerticalSpacing;
                r.x = position.x + EditorGUIUtility.labelWidth;
                r.width = position.width - EditorGUIUtility.labelWidth;
                if (!isValidTexture && tProp.isExpanded)
                    tProp.isExpanded = false;
                if (tProp.isExpanded)
                {
                    r.width = Mathf.Min(r.width, 128);
                    r.height = r.width;
                    GUI.Box(r, new GUIContent(""), EditorStyles.helpBox);
                    Rect coords = rectProp.rectValue;
                    //coords.y = (1f - coords.y) - coords.height;
                    GUI.DrawTextureWithTexCoords(r, tProp.objectReferenceValue as Texture, coords, true);
                }
                EditorGUI.EndDisabledGroup();
            }
            EditorGUI.indentLevel = guiIndent;
        }
    }
}