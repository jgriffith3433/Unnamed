using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using ProceduralWorlds.Storm.Core;
using ProceduralWorlds.Storm.Utils;
using UnityEditor.Experimental.GraphView;

namespace ProceduralWorlds.Storm.StormEditor
{
    [CustomPropertyDrawer(typeof(StormLayerAttribute))]
    public class StormLayerDrawer : PropertyDrawer
    {
        private Dictionary<string, bool> insertNewLayerHash = new Dictionary<string, bool>();
        
        private static string insertNewLayerText = "";
        private static bool focusOnInsertNewLayerField = false;
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            int numLines = 1;
            StormLayerAttribute stormLayer = attribute as StormLayerAttribute;
            if (stormLayer.mode != StormLayerRemapMode.HIDE)
            {
                if (StormWorld.Instance == null || StormDataCache.LayerRemaps == null)
                {
                    if (property.isExpanded)
                        ++numLines;
                }
                else
                {
                    if (property.isExpanded)
                    {
                        ++numLines;
                        int cnt;
                        if (stormLayer.mode == StormLayerRemapMode.FEATURESET)
                            cnt = StormDataCache.GetLayerRemapCount(property.stringValue);
                        else
                            cnt = StormDataCache.GetRemapedLayerCount(property.stringValue);
                        if (cnt > 0)
                            numLines += cnt;
                    }
                }
            }
            return (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing) * numLines;
        }
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (Event.current.type == EventType.ExecuteCommand)
            {
                if (Event.current.commandName == "ObjectSelectorUpdated")
                {
                    StormAPI.AddWorldLayerRemap(EditorGUIUtility.GetObjectPickerObject() as StormLayerRemap);
                }
                return;
            }
            StormLayerAttribute stormLayer = attribute as StormLayerAttribute;
            if (stormLayer == null)
                return;

            string hashID = property.propertyPath;
            if (property.stringValue == "\nADD_NEW_LAYER\n")
            {
                insertNewLayerHash[hashID] = true;
                property.stringValue = "";
                focusOnInsertNewLayerField = true;
            }
            else if (!insertNewLayerHash.ContainsKey(hashID))
            {
                insertNewLayerHash[hashID] = false;
            }

            EditorGUI.BeginProperty(position, label, property);
            bool insertNewLayer = insertNewLayerHash[hashID];
            Rect r = position;
            r.height = EditorGUIUtility.singleLineHeight;
            if (stormLayer.mode != StormLayerRemapMode.HIDE)
                r.width -= 25;
            DrawStormLayerDropdown(r, stormLayer.title == null ? label.text : stormLayer.title, property.stringValue, insertNewLayer,
                s =>
                {
                    if (s == "ADD_NEW_LAYER")
                    {
                        insertNewLayerHash[property.propertyPath] = true;
                    }
                    else
                    {
                        string oldVal = property.stringValue;
                        property.stringValue = s;
                        property.serializedObject.ApplyModifiedProperties();
                        if (StormWorld.Instance != null)
                        {
                            StormWorld.Instance.UpdateFeaturesets();
                        }
                    }
                },
                s => {
                    string oldVal = property.stringValue;
                    property.stringValue = s;
                    property.serializedObject.ApplyModifiedProperties();
                    insertNewLayerHash[property.propertyPath] = false;
                    if (StormWorld.Instance != null)
                    {
                        StormWorld.Instance.UpdateFeaturesets();
                    }
                });
            if (stormLayer.mode != StormLayerRemapMode.HIDE)
            {
                r.x += r.width + 2;
                r.width = 23;
                property.isExpanded = GUI.Toggle(r, property.isExpanded, EditorGUIUtility.IconContent("Dependency@2x"), EditorStyles.miniButtonRight);
                if (property.isExpanded)
                {
                    r.x = position.x;
                    r.width = position.width;
                    r.y += r.height + EditorGUIUtility.standardVerticalSpacing;
                    r.yMax = position.yMax;
                    DrawRemappingData(r, property, stormLayer);
                }
            }
            EditorGUI.EndProperty();
        }
        void DrawRemappingData(Rect position, SerializedProperty property, StormLayerAttribute stormLayer)
        {
            bool isFeatureset = stormLayer.mode == StormLayerRemapMode.FEATURESET;
            float propertyEndPos = position.yMax;
            GUI.Box(position, new GUIContent(""), EditorStyles.helpBox);
            position.x += 2;
            position.y += 2;
            position.width -= 4;
            position.height = EditorGUIUtility.singleLineHeight;
            if (position.yMax > propertyEndPos)
                return;
            if (StormDataCache.LayerRemaps == null)
            {
                Rect r = position;
                r.width /= 2;
                GUI.Label(r, "No Remapping Asset");
                r.x += r.width;
                r.width /= 2;
                if (GUI.Button(r, "Select"))
                {
                    EditorGUIUtility.ShowObjectPicker<StormLayerRemap>(null, false, "", 0);
                }
                r.x += r.width;
                if (GUI.Button(r, "Create"))
                {
                    string filename = EditorUtility.SaveFilePanelInProject("Create Remap Asset", "StormLayerRemap", "asset", "Create Remap Asset to be used");
                    if (!string.IsNullOrEmpty(filename))
                    {
                        StormLayerRemap newRemap = ScriptableObject.CreateInstance<StormLayerRemap>();
                        AssetDatabase.CreateAsset(newRemap, filename);
                        newRemap = AssetDatabase.LoadAssetAtPath<StormLayerRemap>(filename);
                        StormAPI.AddWorldLayerRemap(newRemap);
                    }
                    GUIUtility.ExitGUI();
                }
            }
            else
            {
                Rect remRect = position;
                remRect.width = 21;
                position.width -= 23;
                remRect.x += position.width + 2;
                List<string> remaps;
                string propString = property.stringValue;
                if (isFeatureset)
                    remaps = StormDataCache.GetLayerRemaps(propString);
                else
                    remaps = StormDataCache.GetRemapedLayer(propString, false);
                Rect r = position;
                r.width = EditorGUIUtility.labelWidth - 4;
                GUI.Label(r, isFeatureset ? "Includes:" : "Included In:");
                position.x += r.width;
                position.width -= r.width;
                for (int i = isFeatureset ? 1 : 0; i < remaps.Count; ++i)
                {
                    string hashID = property.propertyPath + ".rebinds[" + i + "]";
                    bool insertMode = false;
                    if (insertNewLayerHash.ContainsKey(hashID))
                        insertMode = insertNewLayerHash[hashID];
                    int index = i - 1;
                    if (isFeatureset)
                    {
                        DrawStormLayerDropdown(position, remaps[i], insertMode,
                          s => {
                              if (s == "ADD_NEW_LAYER")
                              {
                                  insertNewLayerHash[hashID] = true;
                              }
                              else
                              {
                                  StormDataCache.SetLayerRemap(propString, index, s);
                              }
                          },
                          s => {
                              StormDataCache.SetLayerRemap(propString, index, s);
                              insertNewLayerHash[hashID] = false;
                          });
                        if (GUI.Button(remRect, EditorGUIUtility.IconContent("d_Toolbar Minus@2x"), EditorStyles.miniButtonRight))
                        {
                            StormDataCache.RemoveLayerRemap(propString, index);
                        }
                    }
                    else
                    {
                        string oldRemap = remaps[i];
                        DrawStormLayerDropdown(position, oldRemap, insertMode,
                          s => {
                              if (s == "ADD_NEW_LAYER")
                              {
                                  insertNewLayerHash[hashID] = true;
                              }
                              else
                              {
                                  StormDataCache.RemoveLayerRemap(oldRemap, propString);
                                  StormDataCache.AddLayerRemap(s, propString);
                              }
                          },
                          s => {
                              StormDataCache.RemoveLayerRemap(oldRemap, propString);
                              StormDataCache.AddLayerRemap(s, propString);
                              insertNewLayerHash[hashID] = false;
                          });
                        if (GUI.Button(remRect, EditorGUIUtility.IconContent("d_Toolbar Minus@2x"), EditorStyles.miniButtonRight))
                        {
                            StormDataCache.RemoveLayerRemap(oldRemap, propString);
                        }
                    }
                    position.y += position.height + EditorGUIUtility.standardVerticalSpacing;
                    remRect.y = position.y;
                    if (position.yMax > propertyEndPos)
                        return;
                }
                if (GUI.Button(position, new GUIContent("  Add Layer", EditorGUIUtility.IconContent("d_Toolbar Plus More@2x").image)))
                {
                    string hashID = property.propertyPath + ".rebinds[" + remaps.Count + "]";
                    if (isFeatureset)
                    {
                        ShowLayerDropdown(GUIUtility.GUIToScreenPoint(position.min), s => {
                            if (s == "ADD_NEW_LAYER")
                            {
                                insertNewLayerHash[hashID] = true;
                                s = "";
                            }
                            StormDataCache.AddLayerRemap(propString, s);
                        });
                    }
                    else
                    {
                        ShowLayerDropdown(GUIUtility.GUIToScreenPoint(position.min), s => {
                            if (s == "ADD_NEW_LAYER")
                            {
                                insertNewLayerHash[hashID] = true;
                                s = "";
                            }
                            StormDataCache.AddLayerRemap(s, propString);
                        });
                    }
                }
            }
        }

        public static void DrawStormLayerDropdown(Rect position, string stormLayer, bool insertMode,
            Action<string> onSelected, Action<string> onInserted)
        {
            DrawStormLayerDropdown(position, null, stormLayer, insertMode, onSelected, onInserted);
        }

        public static void DrawStormLayerDropdown(Rect position, string title, string stormLayer, bool insertMode,
            Action<string> onSelected, Action<string> onInserted)
        {
            StormWorld stormWorld = StormWorld.Instance;

            // Calculate the width for the help box and the button
            float buttonWidth = 30;
            float spacing = 5; // Space between help box and button
            float helpBoxWidth = position.width - buttonWidth - spacing;
            float helpBoxHeight = EditorGUIUtility.singleLineHeight; // Height of help box

            // Define the rects for help box and button
            Rect helpBoxRect = new Rect(position.x, position.y, helpBoxWidth, helpBoxHeight);
            Rect buttonRect = new Rect(position.x + helpBoxWidth + spacing, position.y, buttonWidth,
                EditorGUIUtility.singleLineHeight);

            if (stormWorld == null)
            {
                EditorGUI.HelpBox(helpBoxRect, "Storm World not yet created in the scene.", MessageType.Warning);
                if (GUI.Button(buttonRect, "Fix"))
                {
                    // Action to create StormWorld or open a specific window
                    //CreateStormWorld(); // Implement this method based on your requirements
                    StormManagerWindow.ShowManager();
                }

                return;
            }

            StormLayerSet globalLayerSet = stormWorld.GlobalLayerSet;
            if (globalLayerSet == null)
            {
                EditorGUI.HelpBox(helpBoxRect, "The GlobalLayerSet has not been assigned in StormWorld.",
                    MessageType.Warning);
                if (GUI.Button(buttonRect, "Fix"))
                {
                    Selection.activeObject = stormWorld;
                    EditorGUIUtility.PingObject(stormWorld);
                }

                return;
            }

            List<string> layers = new List<string>(globalLayerSet.Layers);
            List<StormLayerType> layerTypes = new List<StormLayerType>(globalLayerSet.LayerTypes);

            // Check if the current string value exists in the layers
            int selectedIndex;
            bool exists;
            string buttonTitle;
            if (string.IsNullOrEmpty(stormLayer))
            {
                selectedIndex = -1;
                buttonTitle = "(None)";
                exists = true;
            }
            else
            {
                selectedIndex = layers.IndexOf(stormLayer);
                exists = selectedIndex >= 0;
                if (exists)
                    buttonTitle = stormLayer;
                else
                    buttonTitle = stormLayer + " (Missing)";
            }

            // Adjust position for warning icon if needed
            Rect dropdownRect = new Rect(position);
            if (!exists)
            {
                dropdownRect.width -= 20; // Adjust width for warning icon
            }
            
            Rect labelRect = position;
            if (!string.IsNullOrEmpty(title))
            {
                labelRect.width = EditorGUIUtility.labelWidth;
                EditorGUI.LabelField(labelRect, title);
                dropdownRect.x += labelRect.width;
                dropdownRect.width -= labelRect.width;
            }

            if (insertMode)
            {
                dropdownRect.width -= 55f;
                buttonRect.x -= 20f;
                buttonRect.width += 20f;
                buttonRect.height += 1f;
                // Set the internal name of the textfield
                GUI.SetNextControlName ("InsertField");
                insertNewLayerText = GUI.TextField(dropdownRect, insertNewLayerText);

                if (focusOnInsertNewLayerField)
                {
                    GUI.FocusControl("InsertField");
                    focusOnInsertNewLayerField = false;
                }

                Event e = Event.current;
                bool returnPressed = e.type == EventType.KeyUp && e.keyCode == KeyCode.Return;
                if (GUI.Button(buttonRect, "Save") || returnPressed)
                {
                    Undo.RecordObject(globalLayerSet, $"Added Layer: {insertNewLayerText}");
                    globalLayerSet.AddLayer(insertNewLayerText);
                    EditorUtility.SetDirty(globalLayerSet);
                    onInserted?.Invoke(insertNewLayerText);
                }
            }
            else
            {
                // Dropdown
                if (GUI.Button(dropdownRect, buttonTitle, EditorStyles.popup))
                {
                    Vector2 screenPosition = GUIUtility.GUIToScreenPoint(dropdownRect.center);
                    ShowLayerDropdown(screenPosition, result => { OnSelected(stormLayer); onSelected?.Invoke(result); }, layers, layerTypes);
                }
            }

            // Warning Icon
            if (!exists && !insertMode)
            {
                Rect warningRect = new Rect(dropdownRect.xMax + 2, position.y, 18, position.height);
                if (GUI.Button(warningRect, new GUIContent(EditorGUIUtility.IconContent("console.warnicon").image, "Layer does not exist in known layers data.\nClick to add it."), EditorStyles.miniButtonLeft))
                {
                    globalLayerSet.Layers.Add(stormLayer);
                    globalLayerSet.LayerTypes.Add(StormLayerType.NONE);
                }
            }
        }
        public static void ShowLayerDropdown(Vector2 screenPosition, Action<string> onSelect, List<string> layerList = null, List<StormLayerType> typeList = null)
        {
            StormLayerSearchProvider searchProvider =
                ScriptableObject.CreateInstance<StormLayerSearchProvider>();
            if (layerList == null)
                layerList = new List<string>(StormWorld.Instance?.GlobalLayerSet?.Layers);
            if (typeList == null)
                typeList = new List<StormLayerType>(StormWorld.Instance?.GlobalLayerSet?.LayerTypes);
            searchProvider.layers = layerList.ToArray();
            searchProvider.layerTypes = typeList.ToArray();
            searchProvider.onSelectedCallback = onSelect;
            SearchWindow.Open(new SearchWindowContext(screenPosition), searchProvider);
        }
        private static void OnSelected(string s)
        {
            insertNewLayerText = s;
            focusOnInsertNewLayerField = true;
        }
    }
}