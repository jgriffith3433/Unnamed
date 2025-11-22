using UnityEditor;
using UnityEngine;
using ProceduralWorlds.Storm.Core;
using ProceduralWorlds.Storm.Utils;

namespace ProceduralWorlds.Storm.StormEditor
{
    [CustomPropertyDrawer(typeof(Utils.TerrainSourceData))]
    public class StormTerrainDataEditorDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (!property.isExpanded)
                return EditorGUIUtility.singleLineHeight;
            else
            {
                SerializedProperty nameProp = property.FindPropertyRelative("_layerName");
                SerializedProperty sourceProp = property.FindPropertyRelative("_source");
                int lines;
                if (sourceProp.enumValueIndex == 0 || sourceProp.enumValueIndex == 4 || sourceProp.enumValueIndex == 3)
                    lines = 9;
                else
                    lines = 11;
                if (sourceProp.enumValueIndex != 3)
                    ++lines;
                if (!property.serializedObject.isEditingMultipleObjects)
                    ++lines;
                float ret = (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing) * lines;
                if (!property.serializedObject.isEditingMultipleObjects && sourceProp.isExpanded)
                    ret += 128;
                return EditorGUI.GetPropertyHeight(nameProp) + ret;
            }
        }
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            bool changed = false;
            position.height = EditorGUIUtility.singleLineHeight;
            SerializedProperty nameProp = property.FindPropertyRelative("_layerName");
            property.isExpanded = EditorGUI.Foldout(position, property.isExpanded, new GUIContent(string.IsNullOrEmpty(nameProp.stringValue) ? label.text : nameProp.stringValue, label.tooltip), true);
            position.y += position.height + EditorGUIUtility.standardVerticalSpacing;
            if (property.isExpanded)
            {
                SerializedProperty priorityProp = property.FindPropertyRelative("_priority");
                SerializedProperty terrainProp = property.FindPropertyRelative("_terrain");
                SerializedProperty sourceProp = property.FindPropertyRelative("_source");
                position.height = EditorGUI.GetPropertyHeight(nameProp);
                EditorGUI.PropertyField(position, nameProp);
                position.y += position.height + EditorGUIUtility.standardVerticalSpacing;
                position.height = EditorGUIUtility.singleLineHeight;
                EditorGUI.BeginChangeCheck();
                EditorGUI.showMixedValue = priorityProp.hasMultipleDifferentValues;
                int prioVal = EditorGUI.IntField(position, "Priority", priorityProp.intValue);
                if (EditorGUI.EndChangeCheck())
                    priorityProp.intValue = prioVal;
                position.y += position.height + EditorGUIUtility.standardVerticalSpacing;
                EditorGUI.BeginChangeCheck();
                EditorGUI.showMixedValue = terrainProp.hasMultipleDifferentValues;
                TerrainData dataVal = EditorGUI.ObjectField(position, "Terrain Data", terrainProp.objectReferenceValue, typeof(TerrainData), false) as TerrainData;
                if (EditorGUI.EndChangeCheck())
                    terrainProp.objectReferenceValue = dataVal;
                position.y += position.height + EditorGUIUtility.standardVerticalSpacing;
                EditorGUI.BeginChangeCheck();
                EditorGUI.PropertyField(position, sourceProp, new GUIContent("Data Type"));
                if (EditorGUI.EndChangeCheck())
                    changed = true;
                position.y += position.height + EditorGUIUtility.standardVerticalSpacing;

                SerializedProperty sourceIndexProp = property.FindPropertyRelative("_sourceIndex");
                SerializedProperty areaProp = property.FindPropertyRelative("_sourceArea");
                SerializedProperty exportedProp = property.FindPropertyRelative(sourceProp.enumValueIndex == 3 ? "_exportedTRS" : "_exportedTexture");
                SerializedProperty exportedColorProp = sourceProp.enumValueIndex == 3 ?  null : property.FindPropertyRelative("_exportedTextureColor");
                if (sourceProp.enumValueIndex != 0 && sourceProp.enumValueIndex != 4)
                {
                    TerrainData tData = terrainProp.objectReferenceValue as TerrainData;
                    if (tData != null)
                    {
                        switch (sourceProp.enumValueIndex)
                        {
                            case 1:
                                EditorGUI.BeginChangeCheck();
                                int iVal = EditorGUI.IntSlider(position, new GUIContent("Texture"), sourceIndexProp.intValue, 0, Mathf.Max(0, tData.alphamapLayers - 1));
                                if (EditorGUI.EndChangeCheck())
                                {
                                    sourceIndexProp.intValue = iVal;
                                    changed = true;
                                }
                                position.y += position.height + EditorGUIUtility.standardVerticalSpacing;
                                if (sourceIndexProp.intValue < 0 || sourceIndexProp.intValue >= tData.terrainLayers.Length)
                                {
                                    GUI.Label(position, new GUIContent("No Textures in Terrain Data"));
                                    position.y += position.height + EditorGUIUtility.standardVerticalSpacing;
                                }
                                else
                                {
                                    Texture2D tex = tData.terrainLayers[sourceIndexProp.intValue].diffuseTexture;
                                    if (tex != null)
                                        GUI.Label(position, new GUIContent(tex.name));
                                    else
                                        GUI.Label(position, new GUIContent("NULL Texture"));
                                    position.y += position.height + EditorGUIUtility.standardVerticalSpacing;
                                }
                                break;
                            case 2:
                                EditorGUI.BeginChangeCheck();
                                int dVal = EditorGUI.IntSlider(position, new GUIContent("Detail"), sourceIndexProp.intValue, 0, Mathf.Max(0, tData.detailPrototypes.Length - 1));
                                if (EditorGUI.EndChangeCheck())
                                {
                                    sourceIndexProp.intValue = dVal;
                                    changed = true;
                                }
                                position.y += position.height + EditorGUIUtility.standardVerticalSpacing;
                                if (sourceIndexProp.intValue < 0 || sourceIndexProp.intValue >= tData.detailPrototypes.Length)
                                {
                                    GUI.Label(position, new GUIContent("No Detail Prototypes in Terrain Data"));
                                    position.y += position.height + EditorGUIUtility.standardVerticalSpacing;
                                }
                                else
                                {
                                    DetailPrototype prototype = tData.detailPrototypes[sourceIndexProp.intValue];
                                    if (prototype.prototype != null)
                                        GUI.Label(position, new GUIContent(prototype.prototype.name));
                                    else if (prototype.prototypeTexture != null)
                                        GUI.Label(position, new GUIContent(prototype.prototypeTexture.name));
                                    else
                                        GUI.Label(position, new GUIContent("NULL Detail"));
                                    position.y += position.height + EditorGUIUtility.standardVerticalSpacing;
                                }
                                break;
                            case 3:
                                EditorGUI.BeginChangeCheck();
                                int tVal = EditorGUI.IntSlider(position, new GUIContent("Tree"), sourceIndexProp.intValue, 0, Mathf.Max(0, tData.treePrototypes.Length - 1));
                                if (EditorGUI.EndChangeCheck())
                                {
                                    sourceIndexProp.intValue = tVal;
                                    changed = true;
                                }
                                position.y += position.height + EditorGUIUtility.standardVerticalSpacing;
                                if (sourceIndexProp.intValue < 0 || sourceIndexProp.intValue >= tData.treePrototypes.Length)
                                {
                                    GUI.Label(position, new GUIContent("No Trees in Terrain Data"));
                                    position.y += position.height + EditorGUIUtility.standardVerticalSpacing;
                                }
                                else
                                {
                                    TreePrototype tree = tData.treePrototypes[sourceIndexProp.intValue];
                                    if (tree.prefab != null)
                                        GUI.Label(position, new GUIContent(tree.prefab.name));
                                    else
                                        GUI.Label(position, new GUIContent("NULL Tree"));
                                    position.y += position.height + EditorGUIUtility.standardVerticalSpacing;
                                }
                                break;
                        }
                    }
                    else
                    {
                        position.height = EditorGUIUtility.singleLineHeight * 2 + EditorGUIUtility.standardVerticalSpacing;
                        GUI.Label(position, new GUIContent("Please Select a valid Terrain Data"), EditorStyles.helpBox);
                        position.y += position.height + EditorGUIUtility.standardVerticalSpacing;
                    }
                }
                if (sourceProp.enumValueIndex != 3)
                {
                    position.height = EditorGUIUtility.singleLineHeight;
                    SerializedProperty rangeProp = property.FindPropertyRelative("_valueRange");
                    EditorGUI.PropertyField(position, rangeProp);
                    position.y += position.height + EditorGUIUtility.standardVerticalSpacing;
                }
                position.height = EditorGUIUtility.singleLineHeight * 2 + EditorGUIUtility.standardVerticalSpacing;
                EditorGUI.BeginChangeCheck();
                EditorGUI.PropertyField(position, areaProp, new GUIContent("Data Area"));
                if (EditorGUI.EndChangeCheck())
                    changed = true;
                position.y += position.height + EditorGUIUtility.standardVerticalSpacing;

                position.height = EditorGUIUtility.singleLineHeight;
                EditorGUI.BeginChangeCheck();
                EditorGUI.PropertyField(position, exportedProp, new GUIContent("Exported File"));
                if (EditorGUI.EndChangeCheck())
                    changed = true;
                position.y += position.height + EditorGUIUtility.standardVerticalSpacing;

                if (sourceProp.enumValueIndex != 3)
                {
                    EditorGUI.BeginChangeCheck();
                    EditorGUI.PropertyField(position, exportedColorProp, new GUIContent("Exported Color"));
                    if (EditorGUI.EndChangeCheck())
                        changed = true;
                    position.y += position.height + EditorGUIUtility.standardVerticalSpacing;
                }

                TerrainSourceData sourceData = null;
                StormTile sourceTile = null;
                if (changed)
                {
                    sourceData = GetData(property, out sourceTile);
                    sourceData.SetDirty();
                }
                EditorGUI.BeginDisabledGroup(terrainProp.objectReferenceValue == null);
                if (!property.serializedObject.isEditingMultipleObjects &&
                    GUI.Button(position, new GUIContent(exportedProp.objectReferenceValue == null ? "Export Data" : "Update Data")))
                {
                    if (sourceProp.enumValueIndex != 3)
                    {
                        string path;
                        if (exportedProp.objectReferenceValue == null)
                        {
                            path = EditorUtility.SaveFilePanelInProject("Export Terrain Data", terrainProp.objectReferenceValue.name + "_" + nameProp.stringValue, "exr", "Select file location to save exported data");
                            if (string.IsNullOrEmpty(path))
                                EditorGUIUtility.ExitGUI();
                        }
                        else
                            path = StormUtils.GetAssetPath(exportedProp.objectReferenceValue);
                        if (!string.IsNullOrEmpty(path))
                        {
                            for (int so = 0; so < property.serializedObject.targetObjects.Length; ++so)
                            {
                                sourceData = GetData(property, out sourceTile);
                                NativeArrayRef<float> data = sourceData.GetFloatLayer(sourceTile.TileData.Position, sourceTile.TileData.Size, null, false);
                                if (sourceData.IsLoading)
                                    sourceData.LoadingJob.Complete();
                                Texture2D tex = StormUtils.GenerateTexture(data, 0, 1, sourceData.Resolution);
                                sourceData.ExportedTexture = StormUtils.SaveTexture(tex, path, StormTextureExportFormat.EXR);
                                sourceData.TerrainUpdateTime = StormUtils.GetAssetWriteTime(sourceData.Terrain);
                            }
                            property.serializedObject.ApplyModifiedProperties();
                            EditorGUIUtility.ExitGUI();
                        }
                    }
                    else
                    {
                        string path;
                        if (exportedProp.objectReferenceValue == null)
                        {
                            path = EditorUtility.SaveFilePanelInProject("Export Terrain Data", terrainProp.objectReferenceValue.name + "_" + nameProp.stringValue, "asset", "Select file location to save exported data");
                            if (string.IsNullOrEmpty(path))
                                EditorGUIUtility.ExitGUI();
                        }
                        else
                            path = StormUtils.GetAssetPath(exportedProp.objectReferenceValue);
                        if (!string.IsNullOrEmpty(path))
                        {
                            for (int so = 0; so < property.serializedObject.targetObjects.Length; ++so)
                            {
                                sourceData = GetData(property, out sourceTile);
                                NativeListRef<StormItemData> data = sourceData.GetInstanceLayer(sourceTile.TileData.Position, sourceTile.TileData.Size, null, false);
                                if (sourceData.IsLoading)
                                    sourceData.LoadingJob.Complete();
                                StormTRSList trsList = new StormTRSList();
                                trsList.Transforms = new StormUnityItemData[data.Length];
                                for (int i = 0; i < trsList.Transforms.Length; ++i)
                                    trsList.Transforms[i] = data[i];
                                trsList.PreviewOffset = sourceTile.TileData.Position;
                                trsList.PreviewScale = sourceTile.TileData.Size;
                                sourceData.ExportedTRS = StormUtils.SaveAsset(trsList, path);
                                sourceData.TerrainUpdateTime = StormUtils.GetAssetWriteTime(sourceData.Terrain);
                            }
                            property.serializedObject.ApplyModifiedProperties();
                            EditorGUIUtility.ExitGUI();
                        }
                    }
                }
                EditorGUI.EndDisabledGroup();
                position.y += position.height + EditorGUIUtility.standardVerticalSpacing;

                position.height = EditorGUIUtility.singleLineHeight;
                position.x += EditorGUIUtility.labelWidth;
                position.xMax -= EditorGUIUtility.labelWidth;
                if (sourceProp.enumValueIndex != 3 && !property.serializedObject.isEditingMultipleObjects)
                {
                    sourceProp.isExpanded = GUI.Toggle(position, sourceProp.isExpanded, "Preview", EditorStyles.miniButton);
                    position.y += position.height + EditorGUIUtility.standardVerticalSpacing;
                    if (sourceProp.isExpanded)
                    {
                        Texture2D tex;
                        if (exportedProp.objectReferenceValue is Texture2D expTex)
                        {
                            tex = expTex;
                        }
                        else
                        {
                            if (sourceData == null)
                                sourceData = GetData(property, out sourceTile);
                            NativeArrayRef<float> data = sourceData.GetFloatLayer(sourceTile.TileData.Position, sourceTile.TileData.Size, null, false);
                            if (sourceData.IsLoading)
                                sourceData.LoadingJob.Complete();
                            tex = StormUtils.GenerateTexture(data, 0, 1, sourceData.Resolution);
                        }
                        position.height = 128;
                        GUI.Box(position, new GUIContent(""), EditorStyles.helpBox);
                        GUI.DrawTexture(position, tex, ScaleMode.ScaleToFit);
                        position.y += position.height + EditorGUIUtility.standardVerticalSpacing;
                        position.height = EditorGUIUtility.singleLineHeight;
                    }
                }
                else if (sourceProp.isExpanded && sourceProp.enumValueIndex == 1)
                    sourceProp.isExpanded = false;
            }
        }

        private Utils.TerrainSourceData GetData(SerializedProperty property, out StormTile tile, int objectIndex = 0)
        {
            int startPos = property.propertyPath.LastIndexOf('[');
            int endPos = property.propertyPath.LastIndexOf(']');
            string indexStr = property.propertyPath.Substring(startPos + 1, endPos - startPos - 1);
            if (int.TryParse(indexStr, out int index))
            {
                tile = (StormTile)property.serializedObject.targetObjects[objectIndex];
                return tile.TileData.TerrainData[index];
            }
            tile = null;
            return null;
        }
    }
}