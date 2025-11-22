using ProceduralWorlds.Storm.StormEditor;
using PWCommon5;
using UnityEditor;

[CustomEditor(typeof(ProceduralWorlds.Storm.DungeonGen.DungeonLayer))]
public class DungeonLayerEditor : PWEditor
{
    public bool m_defaultOpenPanels = true;

    EditorUtils m_editorUtils;

    SerializedProperty p_LevelSize;
    SerializedProperty p_MaxBounds;
    SerializedProperty p_Levels;
    SerializedProperty p_CustomChambers;
    SerializedProperty p_MirrorRooms;
    SerializedProperty p_UseAstarHalls;
    SerializedProperty p_DefaultChamberFormationProfile;
    SerializedProperty p_DefaultChamberTheme;
    SerializedProperty p_StartPoint;
    SerializedProperty p_CollisionCheckLayer;
    SerializedProperty p_CheckResolution;
    SerializedProperty p_ChamberCount;
    SerializedProperty p_LayerConnectionCount;
    SerializedProperty p_GridSize;

    ProceduralWorlds.Storm.DungeonGen.DungeonLayer m_DungeonLayerInstance = null;

    private void OnEnable()
    {
        p_LevelSize = serializedObject.FindProperty("levelSize");
        p_MaxBounds = serializedObject.FindProperty("maxBounds");
        p_Levels = serializedObject.FindProperty("levels");
        p_CustomChambers = serializedObject.FindProperty("customChambers");
        p_MirrorRooms = serializedObject.FindProperty("mirrorRooms");
        p_UseAstarHalls = serializedObject.FindProperty("useAstarHalls");
        p_DefaultChamberFormationProfile = serializedObject.FindProperty("defaultChamberFormationProfile");
        p_DefaultChamberTheme = serializedObject.FindProperty("defaultChamberTheme");
        p_StartPoint = serializedObject.FindProperty("startPoint");
        p_CollisionCheckLayer = serializedObject.FindProperty("collisionCheckLayer");
        p_CheckResolution = serializedObject.FindProperty("checkResolution");
        p_ChamberCount = serializedObject.FindProperty("chamberCount");
        p_LayerConnectionCount = serializedObject.FindProperty("layerConnectionCount");
        p_GridSize = serializedObject.FindProperty("gridSize");

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
        m_DungeonLayerInstance = (ProceduralWorlds.Storm.DungeonGen.DungeonLayer)target;

        m_editorUtils.Initialize();

        m_editorUtils.Panel("GeneralSettingsPanel", GeneralSettingsPanel, m_defaultOpenPanels);
        m_editorUtils.Panel("DungeonSettingsPanel", DungeonSettingsPanel, m_defaultOpenPanels);
        m_editorUtils.Panel("GridSettingsPanel", GridSettingsPanel, m_defaultOpenPanels);

        serializedObject.ApplyModifiedProperties();
    }

    public void GeneralSettingsPanel(bool helpActive)
    {
        PropField("LevelSize", p_LevelSize, helpActive);
        PropField("MaxBounds", p_MaxBounds, helpActive);
        PropField("Levels", p_Levels, helpActive);
    }

    public void DungeonSettingsPanel(bool helpActive)
    {
        PropField("CustomChambers", p_CustomChambers, helpActive);
        PropField("MirrorRooms", p_MirrorRooms, helpActive);
        PropField("UseAstarHalls", p_UseAstarHalls, helpActive);
        PropField("DefaultChamberFormationProfile", p_DefaultChamberFormationProfile, helpActive);
        PropField("DefaultChamberTheme", p_DefaultChamberTheme, helpActive);
    }

    public void GridSettingsPanel(bool helpActive)
    {
        PropField("StartPoint", p_StartPoint, helpActive);
        PropField("CollisionCheckLayer", p_CollisionCheckLayer, helpActive);
        PropField("CheckResolution", p_CheckResolution, helpActive);
        PropField("ChamberCount", p_ChamberCount, helpActive);
        PropField("LayerConnectionCount", p_LayerConnectionCount, helpActive);
        PropField("GridSize", p_GridSize, helpActive);
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