using UnityEditor;
using ProceduralWorlds.Storm.StormEditor;
using PWCommon5;



namespace ProceduralWorlds.Storm.BuildingGen
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(BuildingBlockWrapper))]
    public class BuildingBlockEditor : PWEditor
    {
        public bool m_defaultOpenPanels = true;

        EditorUtils m_editorUtils;

        SerializedProperty p_YRotation;
        SerializedProperty p_Offset;
        SerializedProperty p_BlockObjList;
        SerializedProperty p_BlockDirection;

        BuildingBlock m_BuildingBlockInstance = null;
        
        private void OnEnable()
        {
            p_YRotation = serializedObject.FindProperty("yRotation");
            p_Offset = serializedObject.FindProperty("offset");
            p_BlockObjList = serializedObject.FindProperty("blockObjList");
            p_BlockDirection = serializedObject.FindProperty("blockDirection");

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
            m_BuildingBlockInstance = (BuildingBlock)target;

            m_editorUtils.Initialize();

            m_editorUtils.Panel("GeneralSettingsPanel", GeneralSettingsPanel, m_defaultOpenPanels);


            if(m_editorUtils.Button("GetOffset"))
            {
                // Calculate offset from the first prefab in the list
                m_BuildingBlockInstance.GetOffset();
                
                // Mark the object as dirty to ensure changes are saved
                EditorUtility.SetDirty(m_BuildingBlockInstance);
                
                // Force the inspector to update immediately
                serializedObject.Update();
                p_Offset.vector3Value = m_BuildingBlockInstance.offset;
                serializedObject.ApplyModifiedProperties();
                Repaint();
            }


            serializedObject.ApplyModifiedProperties();
        }

        public void GeneralSettingsPanel(bool helpActive)
        {
            PropField("YRotation", p_YRotation, helpActive);
            PropField("Offset", p_Offset, helpActive);
            EditorGUI.indentLevel++;
            PropField("BlockObjList", p_BlockObjList, helpActive);
            EditorGUI.indentLevel--;
            PropField("BlockDirection", p_BlockDirection, helpActive);
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