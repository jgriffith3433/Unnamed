using UnityEditor;
using UnityEngine;

namespace ProceduralWorlds.Storm.StormEditor
{
    [CustomPropertyDrawer(typeof(Utils.TransformData))]
    public class StormTransformDataDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (property.isExpanded)
            {
                SerializedProperty nProp = property.FindPropertyRelative("_layerName");
                float ret = EditorGUI.GetPropertyHeight(nProp) + (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing) * 4;
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
                SerializedProperty lProp = property.FindPropertyRelative("_exportedTRSList");
                SerializedProperty pProp = property.FindPropertyRelative("PrefabBase");
                r.y += r.height + EditorGUIUtility.standardVerticalSpacing;
                r.height = EditorGUI.GetPropertyHeight(nProp);
                EditorGUI.PropertyField(r, nProp);
                r.y += r.height + EditorGUIUtility.standardVerticalSpacing;
                r.height = EditorGUIUtility.singleLineHeight;
                EditorGUI.PropertyField(r, pProp);
                r.y += r.height + EditorGUIUtility.standardVerticalSpacing;
                EditorGUI.PropertyField(r, dProp);
                Utils.StormTransformList trsList = dProp.objectReferenceValue as Utils.StormTransformList;
                int numInstances = 0;
                if (trsList != null)
                {
                    r.y += r.height + EditorGUIUtility.standardVerticalSpacing;
                    r.x += EditorGUIUtility.labelWidth;
                    r.width -= EditorGUIUtility.labelWidth;
                    numInstances = trsList.ItemCount;
                    GUI.Label(r, numInstances + " Instances.");
                }
                r.x = position.x;
                r.width = position.width - 50;
                r.y += r.height + EditorGUIUtility.standardVerticalSpacing;
                EditorGUI.PropertyField(r, lProp);
                r.x += r.width;
                r.width = 50;
                bool isValidData = numInstances > 0;
                EditorGUI.BeginDisabledGroup(!isValidData);
                if (GUI.Button(r, "Export"))
                {
                    string path = EditorUtility.SaveFilePanelInProject("Export Transform List", "TRSList", "asset", "Select file location to save exported TRS List.");
                    if (!string.IsNullOrEmpty(path))
                    {
                        Utils.TransformData data = GetData(property, out Utils.StormTile tile);
                        Utils.StormTRSList exportList;
                        if (lProp.objectReferenceValue != null)
                            exportList = lProp.objectReferenceValue as Utils.StormTRSList;
                        else
                            exportList = ScriptableObject.CreateInstance<Utils.StormTRSList>();
                        exportList.Transforms = new Utils.StormUnityItemData[numInstances];
                        trsList.Items.CopyTo(exportList.Transforms);
                        exportList = Utils.StormUtils.SaveAsset(exportList, path);
                        data.ExportedTRSList = exportList;
                        EditorUtility.SetDirty(tile);
                        GUIUtility.ExitGUI();
                    }
                }
                EditorGUI.EndDisabledGroup();
            }
        }
        private Utils.TransformData GetData(SerializedProperty property, out Utils.StormTile tile)
        {
            int startPos = property.propertyPath.LastIndexOf('[');
            int endPos = property.propertyPath.LastIndexOf(']');
            string indexStr = property.propertyPath.Substring(startPos + 1, endPos - startPos - 1);
            if (int.TryParse(indexStr, out int index))
            {
                tile = (Utils.StormTile)(property.serializedObject.targetObject);
                return tile.TileData.TransformData[index];
            }
            tile = null;
            return null;
        }
    }
}