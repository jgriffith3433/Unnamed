using ProceduralWorlds.Storm.AreaPopulator;
using ProceduralWorlds.Storm.StormEditor;
using PWCommon5;
using UnityEditor;
using ProceduralWorlds.Storm.Core;
using UnityEngine;
using ProceduralWorlds.Storm.UI;
[CustomEditor(typeof(DecorationVolume))]
public class DecorationVolumeEditor : PWEditor
{
    public bool m_defaultOpenPanels = false;

    // Panel expansion state properties
    public bool basicSettingsPanelExpanded { get { return m_DecorationVolumeInstance.basicSettingsPanelExpanded; } set { m_DecorationVolumeInstance.basicSettingsPanelExpanded = value; } }
    public bool areaSettingsPanelExpanded { get { return m_DecorationVolumeInstance.areaSettingsPanelExpanded; } set { m_DecorationVolumeInstance.areaSettingsPanelExpanded = value; } }
    public bool populationSettingsPanelExpanded { get { return m_DecorationVolumeInstance.populationSettingsPanelExpanded; } set { m_DecorationVolumeInstance.populationSettingsPanelExpanded = value; } }
    public bool visualizationSettingsPanelExpanded { get { return m_DecorationVolumeInstance.visualizationSettingsPanelExpanded; } set { m_DecorationVolumeInstance.visualizationSettingsPanelExpanded = value; } }

    EditorUtils m_editorUtils;

    SerializedProperty p_DecorationData;
    SerializedProperty p_UseStorm;

    SerializedProperty p_Height;

    SerializedProperty p_Seed;
    SerializedProperty p_PopulationWaitTime;
    SerializedProperty p_PlacementWaitTime;

    SerializedProperty p_ShowGizmos;

    SerializedProperty p_ShowCorners;

    DecorationVolume m_DecorationVolumeInstance = null;

    private DecorationDataEditor decorationDataEditor;

    private StormPolygonMask polygonMask = null;

    private StormPolygonMaskEditor polygonMaskEditor = null;

    private bool polyChanged = false;

    private void OnEnable()
    {
        // Initialize the instance first to ensure it's available for property accessors
        m_DecorationVolumeInstance = (DecorationVolume)target;
        
        p_DecorationData = serializedObject.FindProperty("decorationData");
        p_UseStorm = serializedObject.FindProperty("useStorm");

        p_Height = serializedObject.FindProperty("height");

        p_Seed = serializedObject.FindProperty("seed");
        p_PopulationWaitTime = serializedObject.FindProperty("populationWaitTime");
        p_PlacementWaitTime = serializedObject.FindProperty("placementWaitTime");

        p_ShowGizmos = serializedObject.FindProperty("showGizmos");

        p_ShowCorners = serializedObject.FindProperty("showCorners");

        polygonMask = m_DecorationVolumeInstance.GetComponent<PolygonMask>();
        if(polygonMask != null)
        {
            polygonMaskEditor = (StormPolygonMaskEditor)CreateEditor(polygonMask, typeof(StormPolygonMaskEditor));
            polygonMask.polygonData.polygonChanged += PolygonData_polygonChanged;
            polygonMask.hideFlags = HideFlags.HideInInspector;
        }
        Undo.undoRedoPerformed -= OnUndo;
        Undo.undoRedoPerformed += OnUndo;
    }

    private void PolygonData_polygonChanged(bool obj)
    {
        polyChanged = true;
    }

    private void OnDisable()
    {
        Undo.undoRedoPerformed -= OnUndo;
        if (polygonMask != null)
        {
            polygonMask.polygonData.polygonChanged -= PolygonData_polygonChanged;
        }
    }

    private void OnDestroy()
    {
        OnDisable();
    }
    public void OnSceneGUI()
    {
        if(polygonMaskEditor != null)
        {
            polygonMaskEditor.polygonEditor.SceneGUI();
        }
    }


    public override void OnInspectorGUI()
    {
        if (m_editorUtils == null)
        {
            m_editorUtils = StormApp.GetEditorUtils(this);
        }
        m_DecorationVolumeInstance = (DecorationVolume)target;

        m_editorUtils.Initialize();

        if(m_defaultOpenPanels)
        {
            basicSettingsPanelExpanded = m_editorUtils.Panel("BasicSettingsPanel", BasicSettingsPanel, m_defaultOpenPanels);
            areaSettingsPanelExpanded = m_editorUtils.Panel("AreaSettingsPanel", AreaSettingsPanel, m_defaultOpenPanels);
            populationSettingsPanelExpanded = m_editorUtils.Panel("PopulationSettingsPanel", PopulationSettingsPanel, m_defaultOpenPanels);
            visualizationSettingsPanelExpanded = m_editorUtils.Panel("VisualizationSettingsPanel", VisualizationSettingsPanel, m_defaultOpenPanels);
        }
        else
        {
            basicSettingsPanelExpanded = m_editorUtils.Panel("BasicSettingsPanel", BasicSettingsPanel, basicSettingsPanelExpanded);
            areaSettingsPanelExpanded = m_editorUtils.Panel("AreaSettingsPanel", AreaSettingsPanel, areaSettingsPanelExpanded);
            populationSettingsPanelExpanded = m_editorUtils.Panel("PopulationSettingsPanel", PopulationSettingsPanel, populationSettingsPanelExpanded);
            visualizationSettingsPanelExpanded = m_editorUtils.Panel("VisualizationSettingsPanel", VisualizationSettingsPanel, visualizationSettingsPanelExpanded);
        }
        



        if(m_editorUtils.Button("Iterate"))
        {
            m_DecorationVolumeInstance.Iterate();
        }
        if(m_editorUtils.Button("Clear"))
        {
            m_DecorationVolumeInstance.Clear();
        }
        serializedObject.ApplyModifiedProperties();

        if (polyChanged)
        {
            m_DecorationVolumeInstance.volumePolygon = m_DecorationVolumeInstance.GetVector2PolygonFromStormPolygon();
            polyChanged = false;
        }
    }

    public void BasicSettingsPanel(bool helpActive)
    {
        PropField("DecorationData", p_DecorationData, helpActive);
        if (p_DecorationData.objectReferenceValue != null)
        {
            if (decorationDataEditor == null)
            {
                // Create an editor for the sub-object and draw its inspector
                decorationDataEditor = CreateEditor(p_DecorationData.objectReferenceValue) as DecorationDataEditor;
                decorationDataEditor.m_defaultOpenPanels = false;
            }
            decorationDataEditor.OnInspectorGUI();

        }
        else
        {
            EditorGUILayout.HelpBox("No Decoration Data Assigned.", MessageType.Warning);
        }
        PropField("UseStorm", p_UseStorm, helpActive);
    }


    public void AreaSettingsPanel(bool helpActive)
    {
        PropField("Height", p_Height, helpActive);
        if(polygonMaskEditor != null)
        {
            polygonMaskEditor.QuickStartPanel(helpActive);
            polygonMaskEditor.DrawPolygonDataObjectNoLayers(helpActive);
        }
    }

    public void PopulationSettingsPanel(bool helpActive)
    {
        PropField("Seed", p_Seed, helpActive);
        PropField("PopulationWaitTime", p_PopulationWaitTime, helpActive);
        PropField("PlacementWaitTime", p_PlacementWaitTime, helpActive);
    }

    public void VisualizationSettingsPanel(bool helpActive)
    {
        PropField("ShowGizmos", p_ShowGizmos, helpActive);
        PropField("ShowCorners", p_ShowCorners, helpActive);
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
