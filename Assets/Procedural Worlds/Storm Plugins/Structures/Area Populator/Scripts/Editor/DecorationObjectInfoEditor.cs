using UnityEditor;
using ProceduralWorlds.Storm.StormEditor;
using PWCommon5;

namespace ProceduralWorlds.Storm.AreaPopulator
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(DecorationObjectInfo))]
    public class DecorationObjectInfoEditor : PWEditor
    {
        public bool m_defaultOpenPanels = false;

        // Panel expansion state properties
        public bool objectDataPanelExpanded { get { return m_DecorationObjectInfoInstance.objectDataPanelExpanded; } set { m_DecorationObjectInfoInstance.objectDataPanelExpanded = value; } }
        public bool placementSettingsPanelExpanded { get { return m_DecorationObjectInfoInstance.placementSettingsPanelExpanded; } set { m_DecorationObjectInfoInstance.placementSettingsPanelExpanded = value; } }

        EditorUtils m_editorUtils;

        SerializedProperty p_BranchObjects;
        SerializedProperty p_ObjectPolygon;
        SerializedProperty p_ObjectSize;

        SerializedProperty p_PlacementFilter;
        SerializedProperty p_OccupySpace;
        SerializedProperty p_SnapToWall;
        SerializedProperty p_SnapToFloor;
        SerializedProperty p_SnapToCeiling;

        SerializedProperty p_RandomizeObjectPos;
        SerializedProperty p_PosRandomization;
        SerializedProperty p_RandomizeObjectRotation;
        SerializedProperty p_RotRandomization;

        SerializedProperty p_UseObjectNoSpawnZone;
        SerializedProperty p_ObjectNoSpawnZone;
        SerializedProperty p_PlacementType;

        DecorationObjectInfo m_DecorationObjectInfoInstance = null;

        private EdgePlacementEditor edgePlacementEditor;
        private LinePlacementEditor linePlacementEditor;

        private void OnEnable()
        {
            // Initialize the instance first to ensure it's available for property accessors
            m_DecorationObjectInfoInstance = (DecorationObjectInfo)target;
            
            p_BranchObjects = serializedObject.FindProperty("branchObjects");
            p_ObjectPolygon = serializedObject.FindProperty("objectPolygon");
            p_ObjectSize = serializedObject.FindProperty("objectSize");

            p_PlacementFilter = serializedObject.FindProperty("placementFilter");
            p_OccupySpace = serializedObject.FindProperty("occupySpace");
            p_SnapToWall = serializedObject.FindProperty("snapToWall");
            p_SnapToFloor = serializedObject.FindProperty("snapToFloor");
            p_SnapToCeiling = serializedObject.FindProperty("snapToCeiling");

            p_RandomizeObjectPos = serializedObject.FindProperty("randomizeObjectPos");
            p_PosRandomization = serializedObject.FindProperty("posRandomization");
            p_RandomizeObjectRotation = serializedObject.FindProperty("randomizeObjectRotation");
            p_RotRandomization = serializedObject.FindProperty("rotRandomization");

            p_UseObjectNoSpawnZone = serializedObject.FindProperty("useObjectNoSpawnZone");
            p_ObjectNoSpawnZone = serializedObject.FindProperty("objectNoSpawnZone");
            p_PlacementType = serializedObject.FindProperty("placementType");

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
            if (m_editorUtils == null)
            {
                m_editorUtils = StormApp.GetEditorUtils(this);
            }
            m_DecorationObjectInfoInstance = (DecorationObjectInfo)target;

            m_editorUtils.Initialize();

            if(m_defaultOpenPanels)
            {
                objectDataPanelExpanded = m_editorUtils.Panel("ObjectDataPanel", ObjectDataPanel, m_defaultOpenPanels);
                placementSettingsPanelExpanded = m_editorUtils.Panel("PlacementSettingsPanel", PlacementSettingsPanel, m_defaultOpenPanels);
            }
            else
            {
                objectDataPanelExpanded = m_editorUtils.Panel("ObjectDataPanel", ObjectDataPanel, objectDataPanelExpanded);
                placementSettingsPanelExpanded = m_editorUtils.Panel("PlacementSettingsPanel", PlacementSettingsPanel, placementSettingsPanelExpanded);
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void ObjectDataPanel(bool helpActive)
        {
            EditorGUI.indentLevel++;
            PropField("BranchObjects", p_BranchObjects, helpActive);
            EditorGUI.indentLevel--;

            EditorGUILayout.BeginHorizontal();
            if(m_editorUtils.Button("BakeBranchObjects"))
            {
                m_DecorationObjectInfoInstance.BakeBranchObjects();
            }
            if(m_editorUtils.Button("SpawnBranchObjects"))
            {
                m_DecorationObjectInfoInstance.SpawnBranchObjects();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUI.indentLevel++;
            PropField("ObjectPolygon", p_ObjectPolygon, helpActive);
            EditorGUI.indentLevel--;
            PropField("ObjectSize", p_ObjectSize, helpActive);
        }

        private void PlacementSettingsPanel(bool helpActive)
        {
            PropField("PlacementFilter", p_PlacementFilter, helpActive);
            
            if (p_PlacementFilter.objectReferenceValue != null)
            {
                if (p_PlacementFilter.objectReferenceValue is EdgePlacement)
                {
                    if (edgePlacementEditor == null)
                    {
                        // Create an editor for the sub-object and draw its inspector
                        edgePlacementEditor = CreateEditor(p_PlacementFilter.objectReferenceValue) as EdgePlacementEditor;
                        edgePlacementEditor.m_defaultOpenPanels = false;
                    }
                    edgePlacementEditor.OnInspectorGUI();
                }
                else if (p_PlacementFilter.objectReferenceValue is LinePlacement)
                {
                    if (linePlacementEditor == null)
                    {
                        // Create an editor for the sub-object and draw its inspector
                        linePlacementEditor = CreateEditor(p_PlacementFilter.objectReferenceValue) as LinePlacementEditor;
                        linePlacementEditor.m_defaultOpenPanels = false;
                    }
                    linePlacementEditor.OnInspectorGUI();
                }
            }
            else
            {
                EditorGUILayout.HelpBox("No Placement Filter Assigned.", MessageType.Warning);
            }
        
            PropField("OccupySpace", p_OccupySpace, helpActive);
            PropField("SnapToWall", p_SnapToWall, helpActive);
            PropField("SnapToFloor", p_SnapToFloor, helpActive);
            PropField("SnapToCeiling", p_SnapToCeiling, helpActive);

            PropField("RandomizeObjectPos", p_RandomizeObjectPos, helpActive);
            if (m_DecorationObjectInfoInstance.randomizeObjectPos)
            {
                ++EditorGUI.indentLevel;
                PropField("PosRandomization", p_PosRandomization, helpActive);
                --EditorGUI.indentLevel;
            }

            PropField("RandomizeObjectRotation", p_RandomizeObjectRotation, helpActive);
            if (m_DecorationObjectInfoInstance.randomizeObjectRotation)
            {
                ++EditorGUI.indentLevel;
                PropField("RotRandomization", p_RotRandomization, helpActive);
                --EditorGUI.indentLevel;
            }

            PropField("UseObjectNoSpawnZone", p_UseObjectNoSpawnZone, helpActive);
            if (m_DecorationObjectInfoInstance.useObjectNoSpawnZone)
            {
                ++EditorGUI.indentLevel;
                PropField("ObjectNoSpawnZone", p_ObjectNoSpawnZone, helpActive);
                --EditorGUI.indentLevel;
            }

            PropField("PlacementType", p_PlacementType, helpActive);
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
