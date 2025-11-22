using UnityEditor;
using ProceduralWorlds.Storm.StormEditor;
using PWCommon5;
using ProceduralWorlds.Storm;

namespace ProceduralWorlds.Storm.StructureGenCore
{
    [CustomEditor(typeof(ObjectInstantiationManager))]
    public class ObjectInstantiationManagerEditor : PWEditor
    {
        EditorUtils m_editorUtils;

        SerializedProperty p_SpawnImmediately;
        //SerializedProperty p_LocalUnConvertedStormObjects;
        SerializedProperty p_DefaultStormFeatureSet;
        //SerializedProperty p_StormTRSData;
        SerializedProperty p_FeatureSet;
        SerializedProperty p_StoredTileData;
        SerializedProperty p_StormTileFeatureSet;
        SerializedProperty p_StoredTileDataBounds;

        ObjectInstantiationManager m_ObjectInstantiationManagerInstance = null;

        private void OnEnable()
        {
            p_SpawnImmediately = serializedObject.FindProperty("spawnImmediately");
            //p_LocalUnConvertedStormObjects = serializedObject.FindProperty("localUnConvertedStormObjects");
            p_DefaultStormFeatureSet = serializedObject.FindProperty("defaultStormFeatureSet");
            //p_StormTRSData = serializedObject.FindProperty("stormTRSData");
            p_FeatureSet = serializedObject.FindProperty("featureSet");

            EditorGUI.indentLevel++;
            p_StoredTileData = serializedObject.FindProperty("storedTileData");
            p_StormTileFeatureSet = serializedObject.FindProperty("stormTileFeatureSet");
            EditorGUI.indentLevel--;

            p_StoredTileDataBounds = serializedObject.FindProperty("storedTileDataBounds");

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
            m_ObjectInstantiationManagerInstance = (ObjectInstantiationManager)target;

            m_editorUtils.Initialize();

            m_editorUtils.Panel("GeneralSettingsPanel", GeneralSettingsPanel, true);
            m_editorUtils.Panel("StormDataPanel", StormDataPanel, true);

            serializedObject.ApplyModifiedProperties();
        }

        public void GeneralSettingsPanel(bool helpActive)
        {
            PropField("SpawnImmediately", p_SpawnImmediately, helpActive);
            //PropField("LocalUnConvertedStormObjects", p_LocalUnConvertedStormObjects, helpActive);
            PropField("DefaultStormFeatureSet", p_DefaultStormFeatureSet, helpActive);
        }

        public void StormDataPanel(bool helpActive)
        {
            //PropField("StormTRSData", p_StormTRSData, helpActive);
            PropField("FeatureSet", p_FeatureSet, helpActive);
            PropField("StoredTileData", p_StoredTileData, helpActive);
            //PropField("StormTileFeatureSet", p_StormTileFeatureSet, helpActive);
            //PropField("StoredTileDataBounds", p_StoredTileDataBounds, helpActive);
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

