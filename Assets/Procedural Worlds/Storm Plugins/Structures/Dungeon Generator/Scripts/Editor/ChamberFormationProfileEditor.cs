using UnityEditor;
using ProceduralWorlds.Storm.DungeonGen;
using ProceduralWorlds.Storm.StormEditor;
using PWCommon5;

[CustomEditor(typeof(ChamberFormationProfile))]
public class ChamberFormationProfileEditor : PWEditor
{
    public bool m_defaultOpenPanels = true;

    EditorUtils m_editorUtils;

    SerializedProperty p_ShapeIterationRange;
    SerializedProperty p_CircleRadiusRange;
    SerializedProperty p_RectangleLengthRange;
    SerializedProperty p_RectangleWidthRange;
    SerializedProperty p_ChamberLevelRange;
    SerializedProperty p_RectangleChance;

    ChamberFormationProfile m_ChamberFormationProfileInstance = null;

    private void OnEnable()
    {
        p_ShapeIterationRange = serializedObject.FindProperty("shapeIterationRange");
        p_CircleRadiusRange = serializedObject.FindProperty("circleRadisRange");
        p_RectangleLengthRange = serializedObject.FindProperty("rectangleLengthRange");
        p_RectangleWidthRange = serializedObject.FindProperty("rectangleWidthRange");
        p_ChamberLevelRange = serializedObject.FindProperty("chamberLevelRange");
        p_RectangleChance = serializedObject.FindProperty("rectangleChance");

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
        m_ChamberFormationProfileInstance = (ChamberFormationProfile)target;

        m_editorUtils.Initialize();

        m_editorUtils.Panel("ChamberSettingsPanel", ChamberSettingsPanel, m_defaultOpenPanels);

        serializedObject.ApplyModifiedProperties();
    }

    public void ChamberSettingsPanel(bool helpActive)
    {
        PropField("ShapeIterationRange", p_ShapeIterationRange, helpActive);
        PropField("CircleRadiusRange", p_CircleRadiusRange, helpActive);
        PropField("RectangleLengthRange", p_RectangleLengthRange, helpActive);
        PropField("RectangleWidthRange", p_RectangleWidthRange, helpActive);
        PropField("ChamberLevelRange", p_ChamberLevelRange, helpActive);
        PropField("RectangleChance", p_RectangleChance, helpActive);
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

