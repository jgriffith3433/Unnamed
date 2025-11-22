using UnityEditor;
using UnityEngine;
using ProceduralWorlds.Storm.Core;

namespace ProceduralWorlds.Storm.StormEditor
{
    [CustomPropertyDrawer(typeof(Utils.FloatData))]
    public class StormFloatDataDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (property.isExpanded)
            {
                SerializedProperty nProp = property.FindPropertyRelative("_layerName");
                float ret = EditorGUI.GetPropertyHeight(nProp) + (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing) * 6;
                SerializedProperty dProp = property.FindPropertyRelative("m_data");
                if (dProp.isExpanded)
                    ret += 128;
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
                SerializedProperty pProp = property.FindPropertyRelative("_priority");
                SerializedProperty wProp = property.FindPropertyRelative("Width");
                SerializedProperty dProp = property.FindPropertyRelative("m_data");
                SerializedProperty tProp = property.FindPropertyRelative("Tex");
                SerializedProperty rProp = property.FindPropertyRelative("_valueRange");

                EditorGUI.BeginChangeCheck();
                r.y += r.height + EditorGUIUtility.standardVerticalSpacing;
                r.height = EditorGUI.GetPropertyHeight(nProp);
                EditorGUI.PropertyField(r, nProp);
                r.y += r.height + EditorGUIUtility.standardVerticalSpacing;
                r.height = EditorGUIUtility.singleLineHeight;
                EditorGUI.PropertyField(r, pProp);
                r.y += r.height + EditorGUIUtility.standardVerticalSpacing;
                EditorGUI.PropertyField(r, wProp);
                r.y += r.height + EditorGUIUtility.standardVerticalSpacing;
                r.width -= 100;
                EditorGUI.PropertyField(r, tProp);
                r.x += r.width;
                r.width = 50;
                bool isValidData = dProp.arraySize > 2 && wProp.intValue > 2;
                EditorGUI.BeginDisabledGroup(!isValidData);
                if (GUI.Button(r, "Export"))
                {
                    string path = EditorUtility.SaveFilePanelInProject("Export Raw Data", "Texture", "png", "Select file location to save exported texture.");
                    if (!string.IsNullOrEmpty(path))
                    {
                        Utils.FloatData sourceData = GetData(property, out Utils.StormTile tile);
                        Texture2D tex = Utils.StormUtils.GenerateTexture(sourceData.m_data, Unity.Mathematics.math.int2(sourceData.Width, sourceData.Width));
                        sourceData.Tex = Utils.StormUtils.SaveTexture(tex, path, Utils.StormTextureExportFormat.PNG, true);
                        EditorUtility.SetDirty(tile);
                        GUIUtility.ExitGUI();
                    }
                }
                EditorGUI.EndDisabledGroup();
                r.x += r.width;
                EditorGUI.BeginDisabledGroup(tProp.objectReferenceValue == null);
                if (GUI.Button(r, "Import"))
                {
                    Texture2D texture = tProp.objectReferenceValue as Texture2D;
                    ushort[] newData = StormInternalUtils.ExtractUShorts(texture);
                    if (newData != null)
                    {
                        Utils.FloatData sourceData = GetData(property, out _);
                        sourceData.m_data = newData;
                        sourceData.Width = texture.width;
                        sourceData.SetDirty();
                    }
                }
                EditorGUI.EndDisabledGroup();
                r.y += r.height + EditorGUIUtility.standardVerticalSpacing;
                r.x = position.x;
                r.width = position.width;
                EditorGUI.PropertyField(r, rProp);
                if (EditorGUI.EndChangeCheck())
                {
                    Utils.FloatData sourceData = GetData(property, out _);
                    sourceData.SetDirty();
                }
                r.y += r.height + EditorGUIUtility.standardVerticalSpacing;
                r.x = position.x + EditorGUIUtility.labelWidth;
                r.width = position.width - EditorGUIUtility.labelWidth;
                dProp.isExpanded = GUI.Toggle(r, dProp.isExpanded, "Preview", EditorStyles.miniButton);
                if (dProp.isExpanded)
                {
                    if (isValidData)
                    {
                        r.y += r.height + EditorGUIUtility.standardVerticalSpacing;
                        Utils.FloatData sourceData = GetData(property, out _);
                        Utils.NativeArrayRef<float> data = sourceData.GetLayer();
                        Texture2D tex = Utils.StormUtils.GenerateTexture(data, 0, 1, sourceData.Resolution);
                        r.height = 128;
                        GUI.Box(r, new GUIContent(""), EditorStyles.helpBox);
                        GUI.DrawTexture(r, tex, ScaleMode.ScaleToFit);
                    }
                    else
                    {
                        dProp.isExpanded = false;
                    }
                }
            }
        }
        private Utils.FloatData GetData(SerializedProperty property, out Utils.StormTile tile)
        {
            int startPos = property.propertyPath.LastIndexOf('[');
            int endPos = property.propertyPath.LastIndexOf(']');
            string indexStr = property.propertyPath.Substring(startPos + 1, endPos - startPos - 1);
            if (int.TryParse(indexStr, out int index))
            {
                tile = (Utils.StormTile)(property.serializedObject.targetObject);
                return tile.TileData.RawData[index];
            }
            tile = null;
            return null;
        }
    }
}