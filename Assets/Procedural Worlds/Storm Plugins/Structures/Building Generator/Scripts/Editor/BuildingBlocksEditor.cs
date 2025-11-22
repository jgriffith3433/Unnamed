using ProceduralWorlds.Storm.StormEditor;
using PWCommon5;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

namespace ProceduralWorlds.Storm.BuildingGen
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(BuildingBlocks))]
    public class BuildingBlocksEditor : PWEditor
    {
        public bool m_defaultOpenPanels = true;

        EditorUtils m_editorUtils;

        SerializedProperty p_WallBlocksList;

        private int selectedWallBlocksInt { get { return m_BuildingBlocksInstance.selectedWallBlocksInt; } set { m_BuildingBlocksInstance.selectedWallBlocksInt = value; } }

        private WallBlocksEditor wallBlocksEditor;
        private WallBlocksData selectedWallBlocksData;

        SerializedProperty p_RoofBlocksList;

        private int selectedRoofBlocksInt { get { return m_BuildingBlocksInstance.selectedRoofBlocksInt; } set { m_BuildingBlocksInstance.selectedRoofBlocksInt = value; } }

        private RoofBlocksEditor roofBlocksEditor;
        private RoofBlocksData selectedRoofBlocksData;

        BuildingBlocks m_BuildingBlocksInstance = null;

        private void OnEnable()
        {
            p_WallBlocksList = serializedObject.FindProperty("wallBlocksList"); 


            p_RoofBlocksList = serializedObject.FindProperty("roofBlocksList");

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
            m_BuildingBlocksInstance = (BuildingBlocks)target;

            m_editorUtils.Initialize();

            m_editorUtils.Panel("WallBlocksPanel", WallBlocksPanel, m_defaultOpenPanels);
            m_editorUtils.Panel("RoofBlocksPanel", RoofBlocksPanel, m_defaultOpenPanels);
            serializedObject.ApplyModifiedProperties();
        }

        public void WallBlocksPanel(bool helpActive)
        {
            EditorGUI.indentLevel++;
            PropField("WallBlocksList", p_WallBlocksList, false);
            EditorGUI.indentLevel--;
            
            List<WallBlocksData> wallBlocksList = m_BuildingBlocksInstance.wallBlocksList;
            if (wallBlocksList.Count == 0)
            {
                EditorGUILayout.HelpBox("No Wall Blocks Assigned.", MessageType.Warning);
                return;
            }
            
            // Filter out null wall blocks and create a filtered list
            List<WallBlocksData> validWallBlocks = wallBlocksList.Where(w => w != null && w.wallBlocks != null).ToList();
            if (validWallBlocks.Count == 0)
            {
                EditorGUILayout.HelpBox("All wall blocks in the list are null or have no data assigned. Please add valid wall blocks.", MessageType.Warning);
                return;
            }

            // Create arrays for the popup with only valid wall blocks
            string[] wallBlockNames = validWallBlocks.Select(x => x.wallBlocks.name).ToArray();
            int[] wallBlockIndices = validWallBlocks.Select((x, i) => i).ToArray();

            // Find the current wall block index in the filtered list
            int filteredSelectedIndex = 0;
            if (selectedWallBlocksInt >= 0 && 
                selectedWallBlocksInt < wallBlocksList.Count && 
                wallBlocksList[selectedWallBlocksInt] != null && 
                wallBlocksList[selectedWallBlocksInt].wallBlocks != null)
            {
                int indexInFilteredList = validWallBlocks.IndexOf(wallBlocksList[selectedWallBlocksInt]);
                if (indexInFilteredList >= 0)
                {
                    filteredSelectedIndex = indexInFilteredList;
                }
            }

            // Display the popup with only valid wall blocks
            int newSelectedIndex = EditorGUILayout.IntPopup("Selected Wall Block:", filteredSelectedIndex, wallBlockNames, wallBlockIndices);
            
            // Map the selected index back to the original list
            if (newSelectedIndex >= 0 && newSelectedIndex < validWallBlocks.Count)
            {
                WallBlocksData selectedValidWallBlock = validWallBlocks[newSelectedIndex];
                m_BuildingBlocksInstance.selectedWallBlocksInt = wallBlocksList.IndexOf(selectedValidWallBlock);
                selectedWallBlocksInt = m_BuildingBlocksInstance.selectedWallBlocksInt;
                serializedObject.ApplyModifiedProperties();
            }

            if (selectedWallBlocksInt >= wallBlocksList.Count || 
                selectedWallBlocksInt < 0 || 
                wallBlocksList[selectedWallBlocksInt] == null || 
                wallBlocksList[selectedWallBlocksInt].wallBlocks == null)
            {
                EditorGUILayout.HelpBox("Selected Wall Block Outside of Range or has No Data Assigned.", MessageType.Warning);
                return;
            }

            if (selectedWallBlocksData != wallBlocksList[selectedWallBlocksInt] || wallBlocksEditor == null)
            {
                selectedWallBlocksData = wallBlocksList[selectedWallBlocksInt];
                // Create an editor for the sub-object and draw its inspector
                wallBlocksEditor = CreateEditor(selectedWallBlocksData.wallBlocks) as WallBlocksEditor;
                wallBlocksEditor.m_defaultOpenPanels = false;
            }

            wallBlocksEditor.OnInspectorGUI();
        }

        public void RoofBlocksPanel(bool helpActive)
        {
            EditorGUI.indentLevel++;
            PropField("RoofBlocksList", p_RoofBlocksList, false);
            EditorGUI.indentLevel--;
            
            List<RoofBlocksData> roofBlocksList = m_BuildingBlocksInstance.roofBlocksList;
            if (roofBlocksList.Count == 0)
            {
                EditorGUILayout.HelpBox("No Roof Blocks Assigned.", MessageType.Warning);
                return;
            }
            
            // Filter out null roof blocks and create a filtered list
            List<RoofBlocksData> validRoofBlocks = roofBlocksList.Where(r => r != null && r.roofBlocks != null).ToList();
            if (validRoofBlocks.Count == 0)
            {
                EditorGUILayout.HelpBox("All roof blocks in the list are null or have no data assigned. Please add valid roof blocks.", MessageType.Warning);
                return;
            }

            // Create arrays for the popup with only valid roof blocks
            string[] roofBlockNames = validRoofBlocks.Select(x => x.roofBlocks.name).ToArray();
            int[] roofBlockIndices = validRoofBlocks.Select((x, i) => i).ToArray();

            // Find the current roof block index in the filtered list
            int filteredSelectedIndex = 0;
            if (selectedRoofBlocksInt >= 0 && 
                selectedRoofBlocksInt < roofBlocksList.Count && 
                roofBlocksList[selectedRoofBlocksInt] != null && 
                roofBlocksList[selectedRoofBlocksInt].roofBlocks != null)
            {
                int indexInFilteredList = validRoofBlocks.IndexOf(roofBlocksList[selectedRoofBlocksInt]);
                if (indexInFilteredList >= 0)
                {
                    filteredSelectedIndex = indexInFilteredList;
                }
            }

            // Display the popup with only valid roof blocks
            int newSelectedIndex = EditorGUILayout.IntPopup("Selected Roof Block:", filteredSelectedIndex, roofBlockNames, roofBlockIndices);
            
            // Map the selected index back to the original list
            if (newSelectedIndex >= 0 && newSelectedIndex < validRoofBlocks.Count)
            {
                RoofBlocksData selectedValidRoofBlock = validRoofBlocks[newSelectedIndex];
                m_BuildingBlocksInstance.selectedRoofBlocksInt = roofBlocksList.IndexOf(selectedValidRoofBlock);
                selectedRoofBlocksInt = m_BuildingBlocksInstance.selectedRoofBlocksInt;
                serializedObject.ApplyModifiedProperties();
            }

            if (selectedRoofBlocksInt >= roofBlocksList.Count || 
                selectedRoofBlocksInt < 0 || 
                roofBlocksList[selectedRoofBlocksInt] == null || 
                roofBlocksList[selectedRoofBlocksInt].roofBlocks == null)
            {
                EditorGUILayout.HelpBox("Selected Roof Block Outside of Range or has No Data Assigned.", MessageType.Warning);
                return;
            }


            if (selectedRoofBlocksData != roofBlocksList[selectedRoofBlocksInt] || roofBlocksEditor == null)
            {
                selectedRoofBlocksData = roofBlocksList[selectedRoofBlocksInt];
                // Create an editor for the sub-object and draw its inspector
                roofBlocksEditor = CreateEditor(selectedRoofBlocksData.roofBlocks) as RoofBlocksEditor;
                roofBlocksEditor.m_defaultOpenPanels = false;
            }

            roofBlocksEditor.OnInspectorGUI();
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
