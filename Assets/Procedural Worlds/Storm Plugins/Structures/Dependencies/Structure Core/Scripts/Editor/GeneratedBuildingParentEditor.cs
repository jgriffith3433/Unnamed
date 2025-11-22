using UnityEditor;
using ProceduralWorlds.Storm.StormEditor;
using PWCommon5;

namespace ProceduralWorlds.Storm.StructureGenCore
{
    [CustomEditor(typeof(GeneratedBuildingParentWrapper))]
    public class GeneratedBuildingParentEditor : PWEditor
    {
        EditorUtils m_editorUtils;

        SerializedProperty p_StoredStormTileData;
        SerializedProperty p_StoredStormTileRegion;
        //SerializedProperty p_UnConvertedObjects;
        SerializedProperty p_PolygonMask;

        SerializedProperty p_DecorationVolumePrefab;

        GeneratedBuildingParent m_GeneratedBuildingParentInstance = null;

        private void OnEnable()
        {
            p_StoredStormTileData = serializedObject.FindProperty("storedStormTileData");
            p_StoredStormTileRegion = serializedObject.FindProperty("storedStormTileRegion");
            //p_UnConvertedObjects = serializedObject.FindProperty("unConvertedObjects");
            p_PolygonMask = serializedObject.FindProperty("polygonmask");

            p_DecorationVolumePrefab = serializedObject.FindProperty("decorationVolumePrefab");

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
            m_GeneratedBuildingParentInstance = (GeneratedBuildingParent)target;

            m_editorUtils.Initialize();

            m_editorUtils.Panel("GeneralSettingsPanel", GeneralSettingsPanel, true);

            if (m_editorUtils.Button("ConvertToStorm"))
            {
                m_GeneratedBuildingParentInstance.ConvertToStorm();
            }

            if (m_editorUtils.Button("FlattenTerrain"))
            {
                m_GeneratedBuildingParentInstance.FlattenTerrain();
            }

            serializedObject.ApplyModifiedProperties();
        }

        public void GeneralSettingsPanel(bool helpActive)
        {
            PropField("DecorationVolumePrefab", p_DecorationVolumePrefab, helpActive);
            EditorGUI.indentLevel++;
            PropField("StoredStormTileData", p_StoredStormTileData, helpActive);
            PropField("StoredStormTileRegion", p_StoredStormTileRegion, helpActive);
            //PropField("StoredStormTileFeatureSet", p_StoredStormTileFeatureSet, helpActive);
            EditorGUI.indentLevel--;
            //PropField("UnConvertedObjects", p_UnConvertedObjects, helpActive);
            PropField("PolygonMask", p_PolygonMask, helpActive);
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

