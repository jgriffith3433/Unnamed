using ProceduralWorlds.Storm.StormEditor;
using PWCommon5;
using UnityEditor;

namespace ProceduralWorlds.Storm.BuildingGen
{
    [CustomEditor(typeof(WallBlocks))]
    public class WallBlocksEditor : PWEditor
    {
        public bool m_defaultOpenPanels = true;

        EditorUtils m_editorUtils;
        SerializedProperty p_ParentBuildingConfig;
        SerializedProperty p_Foundation;
        SerializedProperty p_FrontStep;
        SerializedProperty p_Floor;
        SerializedProperty p_ExteriorWall;
        SerializedProperty p_ExteriorWallTrim;
        SerializedProperty p_AlternateExteriorWall;
        SerializedProperty p_AlternateExteriorWallTrim;
        SerializedProperty p_InteriorWall;
        SerializedProperty p_InteriorWallTrim;
        SerializedProperty p_Ceiling;
        SerializedProperty p_Railing;
        SerializedProperty p_SupportPillar;
        SerializedProperty p_AlternateWindow;
        SerializedProperty p_Window;
        SerializedProperty p_InteriorDoor;
        SerializedProperty p_ExteriorDoor;
        SerializedProperty p_Nothing;
        SerializedProperty p_Staircase;

        WallBlocks m_WallBlocksInstance = null;
        private void OnEnable()
        {
            p_ParentBuildingConfig = serializedObject.FindProperty("parentBuildingConfig");
            p_Foundation = serializedObject.FindProperty("foundation");
            p_FrontStep = serializedObject.FindProperty("frontStep");
            p_Floor = serializedObject.FindProperty("floor");
            p_ExteriorWall = serializedObject.FindProperty("exteriorWall");
            p_ExteriorWallTrim = serializedObject.FindProperty("exteriorWallTrim");
            p_AlternateExteriorWall = serializedObject.FindProperty("alternateExteriorWall");
            p_AlternateExteriorWallTrim = serializedObject.FindProperty("alternateExteriorWallTrim");
            p_InteriorWall = serializedObject.FindProperty("interiorWall");
            p_InteriorWallTrim = serializedObject.FindProperty("interiorWallTrim");
            p_Ceiling = serializedObject.FindProperty("ceiling");
            p_Railing = serializedObject.FindProperty("railing");
            p_SupportPillar = serializedObject.FindProperty("supportPillar");
            p_AlternateWindow = serializedObject.FindProperty("alternateWindow");
            p_Window = serializedObject.FindProperty("window");
            p_InteriorDoor = serializedObject.FindProperty("interiorDoor");
            p_ExteriorDoor = serializedObject.FindProperty("exteriorDoor");
            p_Nothing = serializedObject.FindProperty("nothing");
            p_Staircase = serializedObject.FindProperty("staircase");

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
            m_WallBlocksInstance = (WallBlocks)target;

            m_editorUtils.Initialize();
            

            m_editorUtils.Panel("StructurePanel", StructurePanel, m_defaultOpenPanels);

            serializedObject.ApplyModifiedProperties();
        }

        public void StructurePanel(bool helpActive)
        {
            PropField("ParentBuildingConfig", p_ParentBuildingConfig, helpActive);

            string message = m_WallBlocksInstance.CheckForMissingBlocks();

            if(m_WallBlocksInstance.parentBuildingConfig == null)
            {              
                EditorGUILayout.HelpBox(message, MessageType.Warning);
                PropField("Foundation", p_Foundation, helpActive);
                PropField("ExteriorWall", p_ExteriorWall, helpActive);
                PropField("ExteriorWallTrim", p_ExteriorWallTrim, helpActive);
                PropField("ExteriorDoor", p_ExteriorDoor, helpActive);
                PropField("InteriorWall", p_InteriorWall, helpActive);
                PropField("InteriorWallTrim", p_InteriorWallTrim, helpActive);
                PropField("SupportPillar", p_SupportPillar, helpActive);
                PropField("Window", p_Window, helpActive);
                PropField("InteriorDoor", p_InteriorDoor, helpActive);
                PropField("Ceiling", p_Ceiling, helpActive);
                PropField("Floor", p_Floor, helpActive);
                PropField("Railing", p_Railing, helpActive);
                PropField("Staircase", p_Staircase, helpActive);
                PropField("Nothing", p_Nothing, helpActive);
                //PropField("FrontStep", p_FrontStep, helpActive);
                PropField("AlternateExteriorWall", p_AlternateExteriorWall, helpActive);
                PropField("AlternateWindow", p_AlternateWindow, helpActive);
                PropField("AlternateExteriorWallTrim", p_AlternateExteriorWallTrim, helpActive);
            }
            else
            {
                if(message != "")
                {
                    EditorGUILayout.HelpBox(message, MessageType.Error);
                }

                PropField("Foundation", p_Foundation, helpActive);
                PropField("ExteriorWall", p_ExteriorWall, helpActive);
                if (m_WallBlocksInstance.parentBuildingConfig.createWallTrim)
                {
                    PropField("ExteriorWallTrim", p_ExteriorWallTrim, helpActive);
                }

                if (m_WallBlocksInstance.parentBuildingConfig.generateInteriorLayout)
                {
                    PropField("ExteriorDoor", p_ExteriorDoor, helpActive);
                    
                    PropField("InteriorWall", p_InteriorWall, helpActive);
                    if (m_WallBlocksInstance.parentBuildingConfig.createWallTrim)
                    {
                        PropField("InteriorWallTrim", p_InteriorWallTrim, helpActive);
                        PropField("SupportPillar", p_SupportPillar, helpActive);
                    }

                    PropField("Window", p_Window, helpActive);

                    PropField("InteriorDoor", p_InteriorDoor, helpActive);

                    PropField("Ceiling", p_Ceiling, helpActive);
                    PropField("Floor", p_Floor, helpActive);


                    PropField("Railing", p_Railing, helpActive);
                    PropField("Staircase", p_Staircase, helpActive);
                    PropField("Nothing", p_Nothing, helpActive);

                    if (m_WallBlocksInstance.parentBuildingConfig.createFrontStep)
                    {
                        //PropField("FrontStep", p_FrontStep, helpActive);
                    }
                }
                if (m_WallBlocksInstance.parentBuildingConfig.useAlternateWallFirstLayer)
                {
                    PropField("AlternateExteriorWall", p_AlternateExteriorWall, helpActive);
                    PropField("AlternateWindow", p_AlternateWindow, helpActive);
                    if (m_WallBlocksInstance.parentBuildingConfig.createWallTrim)
                    {
                        PropField("AlternateExteriorWallTrim", p_AlternateExteriorWallTrim, helpActive);
                    }
                }
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