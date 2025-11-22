using UnityEditor;
using ProceduralWorlds.Storm.StormEditor;
using PWCommon5;
using System.Collections.Generic;
using System.Linq;

namespace ProceduralWorlds.Storm.AreaPopulator
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(DecorationData))]
    public class DecorationDataEditor : PWEditor
    {
        public bool m_defaultOpenPanels = false;

        // Panel expansion state properties
        public bool areaSettingsPanelExpanded { get { return m_DecorationDataInstance.areaSettingsPanelExpanded; } set { m_DecorationDataInstance.areaSettingsPanelExpanded = value; } }
        public bool subdivisionSettingsPanelExpanded { get { return m_DecorationDataInstance.subdivisionSettingsPanelExpanded; } set { m_DecorationDataInstance.subdivisionSettingsPanelExpanded = value; } }
        public bool decorationObjectsPanelExpanded { get { return m_DecorationDataInstance.decorationObjectsPanelExpanded; } set { m_DecorationDataInstance.decorationObjectsPanelExpanded = value; } }

        private EditorUtils m_editorUtils;

        SerializedProperty p_CollisionLayers;
        SerializedProperty p_TerrainLayer;
        SerializedProperty p_CheckForOpenings;



        SerializedProperty p_OpeningCheckDistance;
        SerializedProperty p_OpeningPadding;

        SerializedProperty p_OccupyPathways;
        SerializedProperty p_PathWidth;
        SerializedProperty p_CheckForGround;
        SerializedProperty p_SegmentSize;
        SerializedProperty p_CornerCheckDepth;


        SerializedProperty p_DecorationObjects;

        DecorationData m_DecorationDataInstance = null;


        private DecorationObjectInfoEditor decorationObjectInfoEditor;
        private DecorationObjectInfo selectedDecorationObjectInfo;

        private int selectedDecorationObjectInt { get { return m_DecorationDataInstance.selectedDecorationObjectInt; } set { m_DecorationDataInstance.selectedDecorationObjectInt = value; } }
        private void OnEnable()
        {
            // Initialize the instance first to ensure it's available for property accessors
            m_DecorationDataInstance = (DecorationData)target;
            
            p_CollisionLayers = serializedObject.FindProperty("collisionLayers");
            p_TerrainLayer = serializedObject.FindProperty("terrainLayer");
            p_CheckForOpenings = serializedObject.FindProperty("checkForOpenings");
            p_OpeningCheckDistance = serializedObject.FindProperty("openingCheckDistance");
            p_OpeningPadding = serializedObject.FindProperty("openingPadding");

            p_OccupyPathways = serializedObject.FindProperty("occupyPathways");
            p_PathWidth = serializedObject.FindProperty("pathWidth");

            p_CheckForGround = serializedObject.FindProperty("checkForGround");
            p_SegmentSize = serializedObject.FindProperty("segmentSize");
            p_CornerCheckDepth = serializedObject.FindProperty("cornerCheckDepth");

            p_DecorationObjects = serializedObject.FindProperty("decorationObjects");

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
            m_DecorationDataInstance = (DecorationData)target;

            m_editorUtils.Initialize();

            if(m_defaultOpenPanels)
            {
                areaSettingsPanelExpanded = m_editorUtils.Panel("AreaSettingsPanel", AreaSettingsPanel, m_defaultOpenPanels);
                subdivisionSettingsPanelExpanded = m_editorUtils.Panel("SubdivisionSettingsPanel", SubdivisionSettingsPanel, m_defaultOpenPanels);
                decorationObjectsPanelExpanded = m_editorUtils.Panel("DecorationObjectsPanel", DecorationObjectsPanel, m_defaultOpenPanels);
            }
            else
            {
                areaSettingsPanelExpanded = m_editorUtils.Panel("AreaSettingsPanel", AreaSettingsPanel, areaSettingsPanelExpanded);
                subdivisionSettingsPanelExpanded = m_editorUtils.Panel("SubdivisionSettingsPanel", SubdivisionSettingsPanel, subdivisionSettingsPanelExpanded);
                decorationObjectsPanelExpanded = m_editorUtils.Panel("DecorationObjectsPanel", DecorationObjectsPanel, decorationObjectsPanelExpanded);
            }

            serializedObject.ApplyModifiedProperties();
        }

        public void AreaSettingsPanel(bool helpActive)
        {
            PropField("CollisionLayers", p_CollisionLayers, helpActive);
            PropField("TerrainLayer", p_TerrainLayer, helpActive);
            PropField("CheckForOpenings", p_CheckForOpenings, helpActive);

            if (p_CheckForOpenings.boolValue)
            {
                EditorGUI.indentLevel++;
                PropField("OpeningCheckDistance", p_OpeningCheckDistance, helpActive);
                PropField("OpeningPadding", p_OpeningPadding, helpActive);
                EditorGUI.indentLevel--;
            }
            PropField("OccupyPathways", p_OccupyPathways, helpActive);
            if (p_OccupyPathways.boolValue)
            {
                EditorGUI.indentLevel++;
                PropField("PathWidth", p_PathWidth, helpActive);
                EditorGUI.indentLevel--;
            }

            PropField("CheckForGround", p_CheckForGround, helpActive);
        }

        public void SubdivisionSettingsPanel(bool helpActive)
        {
            PropField("SegmentSize", p_SegmentSize, helpActive);
            PropField("CornerCheckDepth", p_CornerCheckDepth, helpActive);
        }

        public void DecorationObjectsPanel(bool helpActive)
        {
            EditorGUI.indentLevel++;
            PropField("DecorationObjects", p_DecorationObjects, helpActive);
            EditorGUI.indentLevel--;

            List<DecorationObjectInfo> decorationObjects = m_DecorationDataInstance.decorationObjects;
            if (decorationObjects.Count == 0)
            {
                EditorGUILayout.HelpBox("No Decoration Objects Assigned.", MessageType.Warning);
                return;
            }

            List<DecorationObjectInfo> validDecorationObjects = decorationObjects.Where(r => r != null).ToList();
            if (validDecorationObjects.Count == 0)
            {
                EditorGUILayout.HelpBox("All decoration objects in the list are null. Please add valid decoration objects.", MessageType.Warning);
                return;
            }


            // Create arrays for the popup with only valid decoration objects
            string[] decorationObjectNames = validDecorationObjects.Select(x => x.name).ToArray();
            int[] decorationObjectIndices = validDecorationObjects.Select((x, i) => i).ToArray();

            // Find the current decoration object index in the filtered list
            int filteredSelectedIndex = 0;
            if (selectedDecorationObjectInt >= 0 && selectedDecorationObjectInt < decorationObjects.Count && decorationObjects[selectedDecorationObjectInt] != null)
            {
                int indexInFilteredList = validDecorationObjects.IndexOf(decorationObjects[selectedDecorationObjectInt]);
                if (indexInFilteredList >= 0)
                {
                    filteredSelectedIndex = indexInFilteredList;
                }
            }

            // Display the popup with only valid decoration objects
            int newSelectedIndex = EditorGUILayout.IntPopup("Selected Decoration Object:", filteredSelectedIndex, decorationObjectNames, decorationObjectIndices);

            // Map the selected index back to the original list
            if (newSelectedIndex >= 0 && newSelectedIndex < decorationObjects.Count)
            {
                DecorationObjectInfo selectedValidDecorationObject = decorationObjects[newSelectedIndex];
                selectedDecorationObjectInt = decorationObjects.IndexOf(selectedValidDecorationObject);
            }

            if (selectedDecorationObjectInt >= decorationObjects.Count || selectedDecorationObjectInt < 0 || decorationObjects[selectedDecorationObjectInt] == null)
            {
                EditorGUILayout.HelpBox("Selected Decoration Object Outside of Range or is null.", MessageType.Warning);
                return;
            }

            if (selectedDecorationObjectInfo != decorationObjects[selectedDecorationObjectInt] || decorationObjectInfoEditor == null)
            {
                selectedDecorationObjectInfo = decorationObjects[selectedDecorationObjectInt];
                // Create an editor for the sub-object and draw its inspector
                decorationObjectInfoEditor = CreateEditor(selectedDecorationObjectInfo) as DecorationObjectInfoEditor;
                decorationObjectInfoEditor.m_defaultOpenPanels = false;
            }

            decorationObjectInfoEditor.OnInspectorGUI();
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
