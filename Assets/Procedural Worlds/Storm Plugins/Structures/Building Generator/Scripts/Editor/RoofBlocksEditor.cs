using ProceduralWorlds.Storm.StormEditor;
using PWCommon5;
using UnityEditor;

namespace ProceduralWorlds.Storm.BuildingGen
{
    [CustomEditor(typeof(RoofBlocks))]
    public class RoofBlocksEditor : PWEditor
    {
        public bool m_defaultOpenPanels = true;

        EditorUtils m_editorUtils;

        SerializedProperty p_ParentBuildingConfig;
        SerializedProperty p_RoofCorner;
        SerializedProperty p_RoofSegment;
        SerializedProperty p_RoofTop;
        SerializedProperty p_RoofOuterConnection;
        SerializedProperty p_RoofInnerCorner;
        SerializedProperty p_SmallPeak;
        SerializedProperty p_RoofPeakJoin;
        SerializedProperty p_RoofSlantEdgeTrim;
        SerializedProperty p_RoofEdgeTrim;
        SerializedProperty p_RoofPeakTrim;
        SerializedProperty p_RoofCornerToSmallPeakBlendLeft;
        SerializedProperty p_RoofCornerToSmallPeakBlendRight;
        SerializedProperty p_RoofStraightToSmallPeakBlend;
        SerializedProperty p_RoofSmallCorner;

        RoofBlocks m_RoofBlocksInstance = null;
        private void OnEnable()
        {
            p_ParentBuildingConfig = serializedObject.FindProperty("parentBuildingConfig");
            p_RoofCorner = serializedObject.FindProperty("roofCorner");
            p_RoofSegment = serializedObject.FindProperty("roofSegment");
            p_RoofTop = serializedObject.FindProperty("roofTop");
            p_RoofOuterConnection = serializedObject.FindProperty("roofOuterConnection");
            p_RoofInnerCorner = serializedObject.FindProperty("roofInnerCorner");
            p_SmallPeak = serializedObject.FindProperty("smallPeak");
            p_RoofPeakJoin = serializedObject.FindProperty("roofPeakJoin");
            p_RoofSlantEdgeTrim = serializedObject.FindProperty("roofSlantEdgeTrim");
            p_RoofEdgeTrim = serializedObject.FindProperty("roofEdgeTrim");
            p_RoofPeakTrim = serializedObject.FindProperty("roofPeakTrim");
            p_RoofCornerToSmallPeakBlendLeft = serializedObject.FindProperty("roofCornerToSmallPeakBlendLeft");
            p_RoofCornerToSmallPeakBlendRight = serializedObject.FindProperty("roofCornerToSmallPeakBlendRight");
            p_RoofStraightToSmallPeakBlend = serializedObject.FindProperty("roofStraightToSmallPeakBlend");
            p_RoofSmallCorner = serializedObject.FindProperty("roofSmallCorner");

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
            m_RoofBlocksInstance = (RoofBlocks)target;

            m_editorUtils.Initialize();
            

            m_editorUtils.Panel("RoofPanel", RoofPanel, m_defaultOpenPanels);

            serializedObject.ApplyModifiedProperties();
        }
        public void RoofPanel(bool helpActive)
        {
            PropField("ParentBuildingConfig", p_ParentBuildingConfig, helpActive);
            
            string message = m_RoofBlocksInstance.CheckForMissingBlocks();

            if(m_RoofBlocksInstance.parentBuildingConfig == null)
            {
                EditorGUILayout.HelpBox(message, MessageType.Warning);
                
                PropField("RoofTop", p_RoofTop, helpActive);
                PropField("RoofCorner", p_RoofCorner, helpActive);
                PropField("RoofSegment", p_RoofSegment, helpActive);

                PropField("RoofOuterConnection", p_RoofOuterConnection, helpActive);
                PropField("RoofInnerCorner", p_RoofInnerCorner, helpActive);
                PropField("SmallPeak", p_SmallPeak, helpActive);

                PropField("RoofCornerToSmallPeakBlendLeft", p_RoofCornerToSmallPeakBlendLeft, helpActive);
                PropField("RoofCornerToSmallPeakBlendRight", p_RoofCornerToSmallPeakBlendRight, helpActive);
                PropField("RoofStraightToSmallPeakBlend", p_RoofStraightToSmallPeakBlend, helpActive);
                PropField("RoofSmallCorner", p_RoofSmallCorner, helpActive);

                PropField("RoofSlantEdgeTrim", p_RoofSlantEdgeTrim, helpActive);
                PropField("RoofEdgeTrim", p_RoofEdgeTrim, helpActive);
                PropField("RoofPeakTrim", p_RoofPeakTrim, helpActive);

                PropField("RoofPeakJoin", p_RoofPeakJoin, helpActive);
            }
            else
            {
                if(message != "")
                {
                    EditorGUILayout.HelpBox(message, MessageType.Error);
                }
                switch (m_RoofBlocksInstance.parentBuildingConfig.roofType)
                {
                    case RoofType.Flat:
                        PropField("RoofTop", p_RoofTop, helpActive);
                        break;
                    case RoofType.Peaked:
                        PropField("RoofCorner", p_RoofCorner, helpActive);
                        PropField("RoofSegment", p_RoofSegment, helpActive);

                        PropField("RoofOuterConnection", p_RoofOuterConnection, helpActive);
                        PropField("RoofInnerCorner", p_RoofInnerCorner, helpActive);
                        PropField("SmallPeak", p_SmallPeak, helpActive);

                        PropField("RoofCornerToSmallPeakBlendLeft", p_RoofCornerToSmallPeakBlendLeft, helpActive);
                        PropField("RoofCornerToSmallPeakBlendRight", p_RoofCornerToSmallPeakBlendRight, helpActive);
                        PropField("RoofStraightToSmallPeakBlend", p_RoofStraightToSmallPeakBlend, helpActive);
                        PropField("RoofSmallCorner", p_RoofSmallCorner, helpActive);
                        break;
                    case RoofType.PartialPeaked:
                        PropField("RoofTop", p_RoofTop, helpActive);

                        PropField("RoofCorner", p_RoofCorner, helpActive);
                        PropField("RoofSegment", p_RoofSegment, helpActive);

                        PropField("RoofOuterConnection", p_RoofOuterConnection, helpActive);
                        PropField("RoofInnerCorner", p_RoofInnerCorner, helpActive);
                        PropField("SmallPeak", p_SmallPeak, helpActive);

                        PropField("RoofCornerToSmallPeakBlendLeft", p_RoofCornerToSmallPeakBlendLeft, helpActive);
                        PropField("RoofCornerToSmallPeakBlendRight", p_RoofCornerToSmallPeakBlendRight, helpActive);
                        PropField("RoofStraightToSmallPeakBlend", p_RoofStraightToSmallPeakBlend, helpActive);
                        PropField("RoofSmallCorner", p_RoofSmallCorner, helpActive);
                        break;
                }


                if (m_RoofBlocksInstance.parentBuildingConfig.createRoofTrim)
                {
                    PropField("RoofSlantEdgeTrim", p_RoofSlantEdgeTrim, helpActive);
                    PropField("RoofEdgeTrim", p_RoofEdgeTrim, helpActive);
                    PropField("RoofPeakTrim", p_RoofPeakTrim, helpActive);
                }

                if (m_RoofBlocksInstance.parentBuildingConfig.useRoofPeakJoinEdgeCaseFix)
                {
                    PropField("RoofPeakJoin", p_RoofPeakJoin, helpActive);
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
