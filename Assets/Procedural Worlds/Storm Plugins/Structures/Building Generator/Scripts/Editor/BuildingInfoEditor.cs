using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using ProceduralWorlds.Storm.StormEditor;
using PWCommon5;
using System.Linq;
using UnityEditorInternal;

namespace ProceduralWorlds.Storm.BuildingGen
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(BuildingConfig))]
    public class BuildingInfoEditor : PWEditor
    {
        public bool m_defaultOpenPanels = true;

        public bool generalSettingsExpanded { get { return m_BuildingInfoInstance.generalSettingsExpanded; } set { m_BuildingInfoInstance.generalSettingsExpanded = value; } }
        public bool roomSettingsExpanded { get { return m_BuildingInfoInstance.roomSettingsExpanded; } set { m_BuildingInfoInstance.roomSettingsExpanded = value; } }
        public bool wallSettingsExpanded { get { return m_BuildingInfoInstance.wallSettingsExpanded; } set { m_BuildingInfoInstance.wallSettingsExpanded = value; } }
        public bool roofSettingsExpanded { get { return m_BuildingInfoInstance.roofSettingsExpanded; } set { m_BuildingInfoInstance.roofSettingsExpanded = value; } }

        EditorUtils m_editorUtils;

        SerializedProperty p_StyleTag;
        SerializedProperty p_YOffset;

        SerializedProperty p_WindowGenType;

        SerializedProperty p_WindowAngleIncrement;

        SerializedProperty p_ChanceOfWindow;
        //SerializedProperty p_RoomsToInclude;
        SerializedProperty p_Floors;
        SerializedProperty p_GenerateInteriorLayout;
        SerializedProperty p_LevelInfomation;
        SerializedProperty p_XZGridSize;
        SerializedProperty p_YGridSize;
        SerializedProperty p_RoofYGridSize;

        SerializedProperty p_Rooms;



        SerializedProperty p_ExtimatedTotalArea;
        SerializedProperty p_PolygonLists;
        SerializedProperty p_RoofType;
        SerializedProperty p_UseRoofPeakJoinEdgeCaseFix;
        SerializedProperty p_CreateRoofTrim;
        SerializedProperty p_UseAlternateWallFirstLayer;
        SerializedProperty p_CreateFrontStep;
        SerializedProperty p_CreateWallTrim;
        SerializedProperty p_SpawnPos;
        SerializedProperty p_BuildingBlocks;

        BuildingConfig m_BuildingInfoInstance = null;


        private RoomInfoEditor roomsEditor;
        private RoomInfo selectedRoom;

        private int selectedRoomInt { get { return m_BuildingInfoInstance.selectedRoomInt; } set { m_BuildingInfoInstance.selectedRoomInt = value; } }
        
        public bool changesMade = false;

        private ReorderableList roomsReorderable;

        private void OnEnable()
        {
            p_YOffset = serializedObject.FindProperty("yOffset");
            p_StyleTag = serializedObject.FindProperty("styleTag");

            p_WindowGenType = serializedObject.FindProperty("windowGenType");
            p_WindowAngleIncrement = serializedObject.FindProperty("windowAngleIncrement");
            p_ChanceOfWindow = serializedObject.FindProperty("chanceOfWindow");
            //p_RoomsToInclude = serializedObject.FindProperty("roomsToInclude");
            p_Floors = serializedObject.FindProperty("floors");
            p_GenerateInteriorLayout = serializedObject.FindProperty("generateInteriorLayout");
            //p_LevelInfomation = serializedObject.FindProperty("levelInfomation");
            p_XZGridSize = serializedObject.FindProperty("XZGridSize");
            p_YGridSize = serializedObject.FindProperty("YGridSize");
            p_RoofYGridSize = serializedObject.FindProperty("RoofYGridSize");
            p_Rooms= serializedObject.FindProperty("rooms");



            p_ExtimatedTotalArea = serializedObject.FindProperty("extimatedTotalArea");
            p_PolygonLists = serializedObject.FindProperty("polygonLists");
            p_RoofType = serializedObject.FindProperty("roofType");
            p_UseRoofPeakJoinEdgeCaseFix = serializedObject.FindProperty("useRoofPeakJoinEdgeCaseFix");
            p_CreateRoofTrim = serializedObject.FindProperty("createRoofTrim");
            p_UseAlternateWallFirstLayer = serializedObject.FindProperty("useAlternateWallFirstLayer");
            p_CreateFrontStep = serializedObject.FindProperty("createFrontStep");
            p_CreateWallTrim = serializedObject.FindProperty("createWallTrim");
            p_SpawnPos = serializedObject.FindProperty("spawnPos");
            p_BuildingBlocks = serializedObject.FindProperty("buildingBlocks");

            Undo.undoRedoPerformed -= OnUndo;
            Undo.undoRedoPerformed += OnUndo;

            // Initialize the reorderable list for rooms
            roomsReorderable = new ReorderableList(serializedObject, p_Rooms, true, true, true, true);
            SetUpReorderableList(roomsReorderable, "Building Rooms");
        }

        private void SetUpReorderableList(ReorderableList list, string header)
        {
            
            list.drawHeaderCallback = (Rect rect) => {
                EditorGUI.LabelField(rect, header);
            };
            
            list.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) => {
                var element = list.serializedProperty.GetArrayElementAtIndex(index);
                RoomInfo roomInfo = element.objectReferenceValue as RoomInfo;
                string roomName = roomInfo != null ? roomInfo.name : "Empty Room";
                
                // Calculate rects for each property
                float padding = 2f;
                
                // Save the original label width
                float originalLabelWidth = EditorGUIUtility.labelWidth;
                
                // Main object field rect (for the RoomInfo reference)
                Rect objectFieldRect = new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight);
                
                // Set a smaller label width for the main field
                EditorGUIUtility.labelWidth = 0f; // No label, just use the content
                
                // Display the main RoomInfo object field
                EditorGUI.PropertyField(objectFieldRect, element, GUIContent.none);
                
                // If we have a valid RoomInfo, display its properties
                if (roomInfo != null)
                {
                    // Create a SerializedObject for the RoomInfo
                    SerializedObject roomSO = new SerializedObject(roomInfo);
                    
                    // Find the properties we want to display
                    SerializedProperty priorityProp = roomSO.FindProperty("priority");
                    SerializedProperty colorProp = roomSO.FindProperty("roomVisualizationColor");
                    
                    // Create a rect for the second line with priority and color side by side
                    Rect secondLineRect = new Rect(
                        rect.x + 20f, // Indent a bit
                        rect.y + EditorGUIUtility.singleLineHeight + padding,
                        rect.width - 20f, // Account for indentation
                        EditorGUIUtility.singleLineHeight
                    );
                    
                    // Split the second line into two parts
                    float priorityWidth = secondLineRect.width * 0.5f; // Priority gets 50% of the width
                    float colorWidth = secondLineRect.width * 0.5f;    // Color gets 50% of the width
                    
                    Rect priorityRect = new Rect(
                        secondLineRect.x,
                        secondLineRect.y,
                        priorityWidth - 5f, // Small gap between fields
                        secondLineRect.height
                    );
                    
                    Rect colorRect = new Rect(
                        secondLineRect.x + priorityWidth,
                        secondLineRect.y,
                        colorWidth,
                        secondLineRect.height
                    );
                    
                    // Begin checking for changes
                    EditorGUI.BeginChangeCheck();
                    
                    // Set a smaller label width for our compact layout
                    EditorGUIUtility.labelWidth = 50f;
                    
                    // Display the priority value
                    EditorGUI.PropertyField(priorityRect, priorityProp, new GUIContent("Priority"));
                    
                    // Display the color value
                    EditorGUI.PropertyField(colorRect, colorProp, new GUIContent("Color"));
                    
                    // Restore the original label width
                    EditorGUIUtility.labelWidth = originalLabelWidth;
                    
                    // If changes were made, apply them
                    if (EditorGUI.EndChangeCheck())
                    {
                        roomSO.ApplyModifiedProperties();
                        EditorUtility.SetDirty(roomInfo);
                    }
                }
                else
                {
                    // Restore the original label width if no RoomInfo
                    EditorGUIUtility.labelWidth = originalLabelWidth;
                }
            };
            
            // Update the element height to accommodate the additional properties
            list.elementHeightCallback = (int index) => {
                var element = list.serializedProperty.GetArrayElementAtIndex(index);
                RoomInfo roomInfo = element.objectReferenceValue as RoomInfo;
                
                if (roomInfo != null)
                {
                    // Height for the main object field + one line for priority and color + padding
                    return EditorGUIUtility.singleLineHeight * 2 + 4f;
                }
                else
                {
                    // Just the height for the main object field
                    return EditorGUIUtility.singleLineHeight;
                }
            };

            list.onAddCallback = (ReorderableList list) =>
            {
                int lastIndex = list.serializedProperty.arraySize;
                list.serializedProperty.InsertArrayElementAtIndex(lastIndex);
            };
            
            list.onRemoveCallback = (ReorderableList list) =>
            {
                // Get the RoomInfo that's being removed
                var element = list.serializedProperty.GetArrayElementAtIndex(list.index);
                RoomInfo roomToRemove = element.objectReferenceValue as RoomInfo;
                
                // Remove the element from the list
                ReorderableList.defaultBehaviours.DoRemoveButton(list);
            };
            
            list.onSelectCallback = (ReorderableList list) =>
            {
                selectedRoomInt = list.index;
                if (list.index >= 0 && list.index < m_BuildingInfoInstance.rooms.Count)
                {
                    selectedRoom = m_BuildingInfoInstance.rooms[list.index];
                    if (selectedRoom != null && roomsEditor == null)
                    {
                        roomsEditor = (RoomInfoEditor)CreateEditor(selectedRoom, typeof(RoomInfoEditor));
                        roomsEditor.m_defaultOpenPanels = false;
                    }
                    else if (selectedRoom != null)
                    {
                        DestroyImmediate(roomsEditor);
                        roomsEditor = (RoomInfoEditor)CreateEditor(selectedRoom, typeof(RoomInfoEditor));
                        roomsEditor.m_defaultOpenPanels = false;
                    }
                }
            };
        }

        private void OnDisable()
        {
            Undo.undoRedoPerformed -= OnUndo;
        }

        private void OnDestroy()
        {
            OnDisable();
        }

        public override void OnInspectorGUI()
        {
            changesMade = false;
            if (m_editorUtils == null)
            {
                m_editorUtils = StormApp.GetEditorUtils(this);
            }
            m_BuildingInfoInstance = (BuildingConfig)target;

            m_editorUtils.Initialize();

            if(m_defaultOpenPanels)
            {
                generalSettingsExpanded =m_editorUtils.Panel("GeneralSettingsPanel", GeneralSettingsPanel, m_defaultOpenPanels);
                roomSettingsExpanded = m_editorUtils.Panel("RoomSettingsPanel", RoomSettingsPanel, m_defaultOpenPanels);
                wallSettingsExpanded = m_editorUtils.Panel("WallSettingsPanel", WallSettingsPanel, m_defaultOpenPanels);
                roofSettingsExpanded = m_editorUtils.Panel("RoofSettingsPanel", RoofSettingsPanel, m_defaultOpenPanels);
            }
            else
            {
                generalSettingsExpanded = m_editorUtils.Panel("GeneralSettingsPanel", GeneralSettingsPanel, generalSettingsExpanded);
                roomSettingsExpanded = m_editorUtils.Panel("RoomSettingsPanel", RoomSettingsPanel, roomSettingsExpanded);
                wallSettingsExpanded = m_editorUtils.Panel("WallSettingsPanel", WallSettingsPanel, wallSettingsExpanded);
                roofSettingsExpanded = m_editorUtils.Panel("RoofSettingsPanel", RoofSettingsPanel, roofSettingsExpanded);
            }


            serializedObject.ApplyModifiedProperties();
        }

        public void GeneralSettingsPanel(bool helpActive)
        {
            EditorGUI.BeginChangeCheck();

            // Style Tag IntPopup - MOVED TO BuildingGeneratorEditor
            // This functionality has been moved to BuildingGeneratorEditor for better workflow
            /*
            var styleTags = StyleTagDirectory.StyleTags;
            string[] styleTagNames = new string[styleTags.Count + 1];
            int[] styleTagIndices = new int[styleTags.Count + 1];
            
            // Add "None" option at index 0
            styleTagNames[0] = "None";
            styleTagIndices[0] = 0; // Use 0 for "None"
            
            // Add all available style tags
            for (int i = 0; i < styleTags.Count; i++)
            {
                styleTagNames[i + 1] = styleTags[i].name;
                styleTagIndices[i + 1] = i + 1; // Start from 1 for actual style tags
            }
            
            // Find current selection index
            int currentIndex = 0; // Default to "None" (value 0)
            string currentStyleTag = p_StyleTag.stringValue;
            if (!string.IsNullOrEmpty(currentStyleTag))
            {
                for (int i = 0; i < styleTags.Count; i++)
                {
                    if (styleTags[i].name == currentStyleTag)
                    {
                        currentIndex = i + 1; // +1 because "None" is value 0
                        break;
                    }
                }
            }
            
            // Display the IntPopup
            EditorGUI.BeginChangeCheck();
            int styleTagSelectedIndex = EditorGUILayout.IntPopup("Style Tag", currentIndex, styleTagNames, styleTagIndices);
            if (EditorGUI.EndChangeCheck())
            {
                if (styleTagSelectedIndex == 0) // "None" selected
                {
                    p_StyleTag.stringValue = ""; // Set to empty for "None"
                }
                else if (styleTagSelectedIndex > 0 && styleTagSelectedIndex - 1 < styleTags.Count)
                {
                    p_StyleTag.stringValue = styleTags[styleTagSelectedIndex - 1].name;
                }
                
                // Ensure the serialized object is updated
                serializedObject.ApplyModifiedProperties();
            }
            */
            
            // Show read-only style tag for reference
            //EditorGUI.BeginDisabledGroup(true);
            //PropField("StyleTag", p_StyleTag, helpActive);
            //EditorGUI.EndDisabledGroup();
            //EditorGUILayout.HelpBox("Style Tag is now configured in the Building Generator inspector.", MessageType.Info);

            PropField("Floors", p_Floors, helpActive);
            PropField("GenerateInteriorLayout", p_GenerateInteriorLayout, helpActive);
            
            PropField("XZGridSize", p_XZGridSize, helpActive);
            PropField("YGridSize", p_YGridSize, helpActive);
            PropField("RoofYGridSize", p_RoofYGridSize, helpActive);

            PropField("BuildingBlocks", p_BuildingBlocks, helpActive);
            if(EditorGUI.EndChangeCheck())
            {
                changesMade = true;
            }
        }

        public void RoomSettingsPanel(bool helpActive)
        {
            EditorGUI.BeginChangeCheck();
            ++EditorGUI.indentLevel;
            
            // Replace PropField with the reorderable list
            roomsReorderable.DoLayoutList();
            
            --EditorGUI.indentLevel;

            if(EditorGUI.EndChangeCheck())
            {
                changesMade = true;
            }
            bool hasStaircase = false;
            bool hasHallway = false;

            foreach (RoomInfo room in m_BuildingInfoInstance.rooms)
            {
                if(room == null)
                {
                    continue;
                }
                
                if (room.stairCase)
                {
                    hasStaircase = true;
                    break;
                }
                if (room.hallway)
                {
                    hasHallway = true;
                    break;
                }
            }
            if(!hasStaircase && p_Floors.intValue > 1)
            {
                EditorGUILayout.HelpBox("Your building has more than one floor, but no staircases. Staircases are recommended for multi-floor buildings.", MessageType.Warning);
            }
            if(!hasHallway && m_BuildingInfoInstance.rooms.Count > 3)
            {
                //EditorGUILayout.HelpBox("Your building has more than 3 rooms, but no hallways. Hallways are recommended for buildings with more than 3 rooms.", MessageType.Warning);
            }



            m_editorUtils.Heading("RoomSettings", GUILayout.ExpandWidth(false));
            List<RoomInfo> rooms = m_BuildingInfoInstance.rooms;
            if (rooms.Count == 0)
            {
                EditorGUILayout.HelpBox("No Desired Rooms Assigned.", MessageType.Warning);
                return;
            }
            
            // Filter out null rooms and create a filtered list
            List<RoomInfo> validRooms = rooms.Where(r => r != null).ToList();
            if (validRooms.Count == 0)
            {
                EditorGUILayout.HelpBox("All rooms in the list are null. Please add valid rooms.", MessageType.Warning);
                return;
            }

            // Create arrays for the popup with only valid rooms
            string[] roomNames = validRooms.Select(x => x.name).ToArray();
            int[] roomIndices = validRooms.Select((x, i) => i).ToArray();

            // Find the current room index in the filtered list
            int filteredSelectedIndex = 0;
            if (selectedRoomInt >= 0 && selectedRoomInt < rooms.Count && rooms[selectedRoomInt] != null)
            {
                int indexInFilteredList = validRooms.IndexOf(rooms[selectedRoomInt]);
                if (indexInFilteredList >= 0)
                {
                    filteredSelectedIndex = indexInFilteredList;
                }
            }

            // Display the popup with only valid rooms
            int newSelectedIndex = EditorGUILayout.IntPopup("Selected Room:", filteredSelectedIndex, roomNames, roomIndices);
            
            // Map the selected index back to the original list
            if (newSelectedIndex >= 0 && newSelectedIndex < validRooms.Count)
            {
                RoomInfo selectedValidRoom = validRooms[newSelectedIndex];
                selectedRoomInt = rooms.IndexOf(selectedValidRoom);
            }

            if (selectedRoomInt >= rooms.Count || selectedRoomInt < 0 || rooms[selectedRoomInt] == null)
            {
                EditorGUILayout.HelpBox("Selected Room Outside of Range or is null.", MessageType.Warning);
                return;
            }

            if (selectedRoom != rooms[selectedRoomInt] || roomsEditor == null)
            {
                selectedRoom = rooms[selectedRoomInt];
                bool behaviorSettingsExpanded = false;
                bool placementSettingsExpanded = false;

                if(roomsEditor != null)
                {
                    behaviorSettingsExpanded = roomsEditor.behaviorSettingsExpanded;
                    placementSettingsExpanded = roomsEditor.placementSettingsExpanded;
                }

                // Create an editor for the sub-object and draw its inspector
                roomsEditor = CreateEditor(selectedRoom) as RoomInfoEditor;
                roomsEditor.m_defaultOpenPanels = false;
                roomsEditor.behaviorSettingsExpanded = behaviorSettingsExpanded;
                roomsEditor.placementSettingsExpanded = placementSettingsExpanded;
            }

            roomsEditor.OnInspectorGUI();
            if(roomsEditor.changesMade)
            {
                changesMade = true;
            }
        }
    

        public void WallSettingsPanel(bool helpActive)
        {
            EditorGUI.BeginChangeCheck();
            PropField("UseAlternateWallFirstLayer", p_UseAlternateWallFirstLayer, helpActive);
            PropField("CreateWallTrim", p_CreateWallTrim, helpActive);
            PropField("CreateFrontStep", p_CreateFrontStep, helpActive);
            m_editorUtils.Label("ExteriorOnlySettings", EditorStyles.boldLabel);
            PropField("WindowGenType", p_WindowGenType, helpActive);
            if(p_WindowGenType.enumValueIndex == 1)
            {
                EditorGUILayout.HelpBox("Room Based Window Generation does not work with Exterior Only Buildings.", MessageType.Warning);
            }
            else if(p_WindowGenType.enumValueIndex == 2)
            {          
                PropField("WindowAngleIncrement", p_WindowAngleIncrement, helpActive);
                if(p_WindowAngleIncrement.floatValue <= 0)
                {
                    EditorGUILayout.HelpBox("Window Angle Increment must be greater than 0.", MessageType.Warning);
                }

            }
            else if(p_WindowGenType.enumValueIndex == 3)
            {
                PropField("ChanceOfWindow", p_ChanceOfWindow, helpActive);
            }

            if(EditorGUI.EndChangeCheck())
            {
                changesMade = true;
            }
        }

        public void RoofSettingsPanel(bool helpActive)
        {
            EditorGUI.BeginChangeCheck();
            PropField("RoofType", p_RoofType, helpActive);
            PropField("UseRoofPeakJoinEdgeCaseFix", p_UseRoofPeakJoinEdgeCaseFix, helpActive);
            PropField("CreateRoofTrim", p_CreateRoofTrim, helpActive);
            if(EditorGUI.EndChangeCheck())
            {
                changesMade = true;
            }
        }



        private void PropField(string key, SerializedProperty prop, bool helpActive)
        {
            EditorGUI.showMixedValue = prop.hasMultipleDifferentValues;
            m_editorUtils.PropertyField(key, true, prop, helpActive);

            EditorGUI.showMixedValue = false;
        }

        private void OnUndo()
        {
            serializedObject.Update();
            serializedObject.ApplyModifiedProperties();
        }
    }
}
