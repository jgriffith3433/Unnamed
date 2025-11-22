using UnityEditor;
using ProceduralWorlds.Storm.DungeonGen;
using ProceduralWorlds.Storm.StormEditor;
using PWCommon5;


[CustomEditor(typeof(CustomChamber))]
public class CustomChamberEditor : PWEditor
{
    public bool m_defaultOpenPanels = true;

    EditorUtils m_editorUtils;

    SerializedProperty p_Procedural;
    SerializedProperty p_ChamberFormationProfile;
    SerializedProperty p_ChamberTheme;
    SerializedProperty p_Origin;
    SerializedProperty p_Bounds;
    SerializedProperty p_LevelHeight;
    SerializedProperty p_StartingLevel;
    SerializedProperty p_RoomPrefab;
    SerializedProperty p_ChamberConnectionType;
    SerializedProperty p_DefinedEntranceType;
    SerializedProperty p_AllowedFloors;
    SerializedProperty p_HallsAvoidChamber;

    CustomChamber m_CustomChamberInstance = null;

    private void OnEnable()
    {
        p_Procedural = serializedObject.FindProperty("procedural");
        p_ChamberFormationProfile = serializedObject.FindProperty("chamberFormationProfile");
        p_ChamberTheme = serializedObject.FindProperty("chamberTheme");
        p_Origin = serializedObject.FindProperty("origin");
        p_Bounds = serializedObject.FindProperty("bounds");
        p_LevelHeight = serializedObject.FindProperty("levelHeight");
        p_StartingLevel = serializedObject.FindProperty("startingLevel");
        p_ChamberConnectionType = serializedObject.FindProperty("chamberConnectionType");
        p_DefinedEntranceType = serializedObject.FindProperty("definedEnteranceType");
        p_AllowedFloors = serializedObject.FindProperty("allowedFloors");
        p_HallsAvoidChamber = serializedObject.FindProperty("hallsAvoidChamber");
        p_RoomPrefab = serializedObject.FindProperty("roomPrefab");
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
        m_CustomChamberInstance = (CustomChamber)target;

        m_editorUtils.Initialize();

        m_editorUtils.Panel("ChamberSettingsPanel", ChamberSettingsPanel, m_defaultOpenPanels);
        m_editorUtils.Panel("StructureSettingsPanel", StructureSettingsPanel, m_defaultOpenPanels);
        m_editorUtils.Panel("ConnectionSettingsPanel", ConnectionSettingsPanel, m_defaultOpenPanels);

        serializedObject.ApplyModifiedProperties();
    }

    public void ChamberSettingsPanel(bool helpActive)
    {
        PropField("Procedural", p_Procedural, helpActive);
        PropField("ChamberFormationProfile", p_ChamberFormationProfile, helpActive);
        PropField("ChamberTheme", p_ChamberTheme, helpActive);
        PropField("Origin", p_Origin, helpActive);
        PropField("Bounds", p_Bounds, helpActive);
    }

    public void StructureSettingsPanel(bool helpActive)
    {
        PropField("LevelHeight", p_LevelHeight, helpActive);
        PropField("StartingLevel", p_StartingLevel, helpActive);
        PropField("RoomPrefab", p_RoomPrefab, helpActive);
    }

    public void ConnectionSettingsPanel(bool helpActive)
    {
        PropField("ChamberConnectionType", p_ChamberConnectionType, helpActive);
        PropField("DefinedEntranceType", p_DefinedEntranceType, helpActive);
        PropField("AllowedFloors", p_AllowedFloors, helpActive);
        PropField("HallsAvoidChamber", p_HallsAvoidChamber, helpActive);
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
