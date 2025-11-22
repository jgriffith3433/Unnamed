namespace ProceduralWorlds.Storm.BuildingGen
{
    using UnityEditor;
    using UnityEngine;
    using ProceduralWorlds.Storm.StormEditor;
    using PWCommon5;

    [CanEditMultipleObjects]
    [CustomEditor(typeof(RoomInfo))]
    public class RoomInfoEditor : PWEditor
    {

        public bool m_defaultOpenPanels = true;

        public bool behaviorSettingsExpanded = true;
        public bool placementSettingsExpanded = true;

        EditorUtils m_editorUtils;

        SerializedProperty p_Expand;
        SerializedProperty p_PersistLocationThroughFloors;
        SerializedProperty p_StairCase;
        SerializedProperty p_ForceDesiredConnections;
        SerializedProperty p_Hallway;
        SerializedProperty p_CapRoomCount;
        SerializedProperty p_MaxRoomCount;
        SerializedProperty p_ConnectionType;

        SerializedProperty p_CreateWindows;

        SerializedProperty p_DecorateRoom;
        SerializedProperty p_DecorationData;

        SerializedProperty p_RoomVisualizationColor;

        SerializedProperty p_DesiredRoomSize;
        SerializedProperty p_FirstFloorDesiredTiles;
        SerializedProperty p_ExtraFloorsDesiredTiles;

        SerializedProperty p_UseExitCellWeighting;
        SerializedProperty p_ExitCellWeight;
        SerializedProperty p_edgeFitnessModifier;
        SerializedProperty p_connectivityFitnessModifier;
        SerializedProperty p_avoidFitnessModifier;
        SerializedProperty p_UseDesiredStartFloor;
        SerializedProperty p_DesiredStartFloor;
        SerializedProperty p_AllowShifting;
        SerializedProperty p_DesiredFloors;
        SerializedProperty p_Priority;
        SerializedProperty p_DesiredConnections;
        SerializedProperty p_UndesiredConnections;
        SerializedProperty p_MaxConnections;
        SerializedProperty p_RoomStoreyBehaviours;

        RoomInfo m_RoomInfoInstance = null;

        public bool changesMade = false;

        private void OnEnable()
        {
            p_Expand = serializedObject.FindProperty("expand");
            p_PersistLocationThroughFloors = serializedObject.FindProperty("persistLocationThroughFloors");
            p_StairCase = serializedObject.FindProperty("stairCase");
            p_ForceDesiredConnections = serializedObject.FindProperty("forceDesiredConnections");
            p_Hallway = serializedObject.FindProperty("hallway");
            p_CapRoomCount = serializedObject.FindProperty("capRoomCount");
            p_MaxRoomCount = serializedObject.FindProperty("maxRoomCount");
            p_ConnectionType = serializedObject.FindProperty("connectionType");

            p_CreateWindows = serializedObject.FindProperty("createWindows");

            p_DecorateRoom = serializedObject.FindProperty("decorateRoom");
            p_DecorationData = serializedObject.FindProperty("decorationData");

            p_RoomVisualizationColor = serializedObject.FindProperty("roomVisualizationColor");

            p_DesiredRoomSize = serializedObject.FindProperty("desiredRoomSize");
            p_FirstFloorDesiredTiles = serializedObject.FindProperty("firstFloorDesiredTiles");
            p_ExtraFloorsDesiredTiles = serializedObject.FindProperty("extraFloorsDesiredTiles");

            p_UseExitCellWeighting = serializedObject.FindProperty("useExitCellWeighting");
            p_ExitCellWeight = serializedObject.FindProperty("exitCellWeight");
            p_edgeFitnessModifier = serializedObject.FindProperty("edgeFitnessModifier");
            p_connectivityFitnessModifier = serializedObject.FindProperty("connectivityFitnessModifier");
            p_avoidFitnessModifier = serializedObject.FindProperty("avoidFitnessModifier");
            p_UseDesiredStartFloor = serializedObject.FindProperty("useDesiredStartFloor");
            p_DesiredStartFloor = serializedObject.FindProperty("desiredStartFloor");
            p_AllowShifting = serializedObject.FindProperty("allowShifting");
            p_DesiredFloors = serializedObject.FindProperty("desiredFloors");
            p_Priority = serializedObject.FindProperty("priority");
            p_DesiredConnections = serializedObject.FindProperty("desiredConnections");
            p_UndesiredConnections = serializedObject.FindProperty("undesiredConnections");
            p_MaxConnections = serializedObject.FindProperty("maxConnections");
            p_RoomStoreyBehaviours = serializedObject.FindProperty("roomStoreyBehaviors");

            Undo.undoRedoPerformed -= OnUndo;
            Undo.undoRedoPerformed += OnUndo;
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
            m_RoomInfoInstance = (RoomInfo)target;

            m_editorUtils.Initialize();
            if(m_defaultOpenPanels)
            {
                behaviorSettingsExpanded = m_editorUtils.Panel("BehaviorSettingsPanel", BehaviorSettingsPanel, m_defaultOpenPanels);
                placementSettingsExpanded = m_editorUtils.Panel("PlacementSettingsPanel", PlacementSettingsPanel, m_defaultOpenPanels);
            }
            else
            {
                behaviorSettingsExpanded = m_editorUtils.Panel("BehaviorSettingsPanel", BehaviorSettingsPanel, behaviorSettingsExpanded);
                placementSettingsExpanded = m_editorUtils.Panel("PlacementSettingsPanel", PlacementSettingsPanel, placementSettingsExpanded);
            }



            serializedObject.ApplyModifiedProperties();
        }

        public void BehaviorSettingsPanel(bool helpActive)
        {
            EditorGUI.BeginChangeCheck();
            PropField("Priority", p_Priority, helpActive);

            PropField("CapRoomCount", p_CapRoomCount, helpActive);
            if(p_CapRoomCount.boolValue)
            {
                PropField("MaxRoomCount", p_MaxRoomCount, helpActive);
            }

            bool originalState = GUI.enabled;
            if(p_Hallway.boolValue)
            {
                GUI.enabled = false;
            }

            PropField("StairCase", p_StairCase, helpActive);

            if(p_Hallway.boolValue)
            {
                GUI.enabled = originalState;
            }


            
            if(p_StairCase.boolValue)
            {
                originalState = GUI.enabled;
                m_RoomInfoInstance.persistLocationThroughFloors = true;
                //m_RoomInfoInstance.desiredRoomSize = new Vector2Int(2, 2);
                GUI.enabled = false;
            }


            PropField("Hallway", p_Hallway, helpActive);

            if(p_Hallway.boolValue)
            {
                originalState = GUI.enabled;
                GUI.enabled = false;
            }
            
            PropField("Expand", p_Expand, helpActive);

            PropField("PersistLocationThroughFloors", p_PersistLocationThroughFloors, helpActive);

            if(p_StairCase.boolValue)
            {
                GUI.enabled = originalState;
            }

            PropField("CreateWindows", p_CreateWindows, helpActive);

            if(p_Hallway.boolValue)
            {
                GUI.enabled = originalState;
            }



            if(p_StairCase.boolValue)
            {
            originalState = GUI.enabled;
                GUI.enabled = false;
            }

            PropField("DecorateRoom", p_DecorateRoom, helpActive);
            if (p_DecorateRoom.boolValue)
            {
                PropField("DecorationData", p_DecorationData, helpActive);
            }

            if(p_StairCase.boolValue)
            {
                GUI.enabled = originalState;
            }


            PropField("RoomVisualizationColor", p_RoomVisualizationColor, helpActive);
            if(EditorGUI.EndChangeCheck())
            {
                changesMade = true;
            }
        }

        public void PlacementSettingsPanel(bool helpActive)
        {
            EditorGUI.BeginChangeCheck();
            m_editorUtils.Heading("Size");
            bool originalState = GUI.enabled;
            if(p_Hallway.boolValue)
            {
                GUI.enabled = false;
            }
            PropField("DesiredRoomSize", p_DesiredRoomSize, helpActive);

            if(p_Hallway.boolValue)
            {
                GUI.enabled = originalState;
            }
            PropField("DesiredFloors", p_DesiredFloors, helpActive);

            PropField("UseDesiredStartFloor", p_UseDesiredStartFloor, helpActive);
            if (p_UseDesiredStartFloor.boolValue == true)
            {
                PropField("DesiredStartFloor", p_DesiredStartFloor, helpActive);
            }
            PropField("AllowShifting", p_AllowShifting, helpActive);

            originalState = GUI.enabled;
            GUI.enabled = false;
            PropField("FirstFloorDesiredTiles", p_FirstFloorDesiredTiles, helpActive);
            PropField("ExtraFloorsDesiredTiles", p_ExtraFloorsDesiredTiles, helpActive);
            GUI.enabled = originalState;

            originalState = GUI.enabled;
            if(p_StairCase.boolValue || p_Hallway.boolValue)
            {
                GUI.enabled = false;
            }

            EditorGUI.indentLevel++;
            PropField("RoomStoreyBehaviours", p_RoomStoreyBehaviours, helpActive);
            EditorGUI.indentLevel--;
            if(p_StairCase.boolValue || p_Hallway.boolValue)
            {
                GUI.enabled = originalState;
            }


            m_editorUtils.Heading("Connectivity");
            PropField("ForceDesiredConnections", p_ForceDesiredConnections, helpActive);

            EditorGUI.indentLevel++;
            PropField("DesiredConnections", p_DesiredConnections, helpActive);
            EditorGUI.indentLevel--;

            EditorGUI.indentLevel++;
            PropField("UndesiredConnections", p_UndesiredConnections, helpActive);
            EditorGUI.indentLevel--;

            PropField("ConnectionType", p_ConnectionType, helpActive);

            PropField("MaxConnections", p_MaxConnections, helpActive);

            m_editorUtils.Heading("FitnessMods");
            PropField("UseExitCellWeighting", p_UseExitCellWeighting, helpActive);
            if (p_UseExitCellWeighting.boolValue)
            {
                PropField("ExitCellWeight", p_ExitCellWeight, helpActive);
            }

            PropField("ConnectivityFitnessModifier", p_connectivityFitnessModifier, helpActive);
            PropField("AvoidFitnessModifier", p_avoidFitnessModifier, helpActive);
            PropField("EdgeFitnessModifier", p_edgeFitnessModifier, helpActive);
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
