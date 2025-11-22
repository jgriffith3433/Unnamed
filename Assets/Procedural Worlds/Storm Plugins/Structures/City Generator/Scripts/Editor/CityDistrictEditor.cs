using UnityEditor;
using ProceduralWorlds.Storm.StormEditor;
using PWCommon5;
using ProceduralWorlds.Storm.UI;
using UnityEngine;
namespace ProceduralWorlds.Storm.CityBuilder
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(CityDistrict))]
    public class CityDistrictEditor : PWEditor
    {
        public bool m_defaultOpenPanels = false;

        // Property accessors for panel expansion states - point directly to the CityDistrict instance
        public bool polygonPanelExpanded { get { return m_CityDistrictInstance.polygonPanelExpanded; } set { m_CityDistrictInstance.polygonPanelExpanded = value; } }
        public bool roadGenerationSettingsPanelExpanded { get { return m_CityDistrictInstance.roadGenerationSettingsPanelExpanded; } set { m_CityDistrictInstance.roadGenerationSettingsPanelExpanded = value; } }
        public bool lotGenerationSettingsPanelExpanded { get { return m_CityDistrictInstance.lotGenerationSettingsPanelExpanded; } set { m_CityDistrictInstance.lotGenerationSettingsPanelExpanded = value; } }
        public bool districtBuildingSettingsPanelExpanded { get { return m_CityDistrictInstance.districtBuildingSettingsPanelExpanded; } set { m_CityDistrictInstance.districtBuildingSettingsPanelExpanded = value; } }

        EditorUtils m_editorUtils;

        SerializedProperty p_UseCityPolygon;
        SerializedProperty p_DistrictPolygon;
        SerializedProperty p_MajorRoadSettings;
        SerializedProperty p_MinorRoadSettings;
        SerializedProperty p_GenerateExteriorPlots;
        SerializedProperty p_MaxLotSize;
        SerializedProperty p_AllowInternalLots;
        SerializedProperty p_MinLotSideLength;
        SerializedProperty p_BaseBuildingHeight;
        SerializedProperty p_BuildingRandomness;
        SerializedProperty p_BuildingHeightCurve;
        SerializedProperty p_DistrictBuildingTypes;


        private GlobalRoadSettingsEditor majorGlobalRoadSettingsEditor;
        private GlobalRoadSettingsEditor minorGlobalRoadSettingsEditor;

        private PolygonMask polygonMask;
        private StormPolygonMaskEditor polygonMaskEditor;
        //private bool polyChanged = false;

        CityDistrict m_CityDistrictInstance = null;

        private void OnEnable()
        {
            m_CityDistrictInstance = (CityDistrict)target;
            p_UseCityPolygon = serializedObject.FindProperty("useCityPolygon");
            p_DistrictPolygon = serializedObject.FindProperty("districtPolygon");
            p_MajorRoadSettings = serializedObject.FindProperty("majorRoadSettings");
            p_MinorRoadSettings = serializedObject.FindProperty("minorRoadSettings");
            p_GenerateExteriorPlots = serializedObject.FindProperty("generateExteriorPlots");
            p_MaxLotSize = serializedObject.FindProperty("maxLotSize");

            p_AllowInternalLots = serializedObject.FindProperty("allowInternalLots");
            p_MinLotSideLength = serializedObject.FindProperty("minLotSideLength");
            p_BaseBuildingHeight = serializedObject.FindProperty("baseBuildingHeight");
            p_BuildingRandomness = serializedObject.FindProperty("buildingRandomness");
            p_BuildingHeightCurve = serializedObject.FindProperty("buildingHeightCurve");
            p_DistrictBuildingTypes = serializedObject.FindProperty("districtBuildingTypes");

            polygonMask = m_CityDistrictInstance.GetComponent<PolygonMask>();
            if(polygonMask != null)
            {
                polygonMaskEditor = (StormPolygonMaskEditor)CreateEditor(polygonMask, typeof(StormPolygonMaskEditor));
                polygonMask.polygonData.polygonChanged += PolygonData_polygonChanged;
                polygonMask.hideFlags = HideFlags.HideInInspector;
            }
            if(!p_UseCityPolygon.boolValue)
            {
                m_CityDistrictInstance.GetDistrictPolygon(polygonMask, m_CityDistrictInstance.transform);
            }

            Undo.undoRedoPerformed -= OnUndo;
            Undo.undoRedoPerformed += OnUndo;
        }

        private void PolygonData_polygonChanged(bool obj)
        {
            if(!p_UseCityPolygon.boolValue)
            {
                m_CityDistrictInstance.GetDistrictPolygon(polygonMask, m_CityDistrictInstance.transform);
            }
        }
        public void OnSceneGUI()
        {
            if(polygonMaskEditor != null && !p_UseCityPolygon.boolValue)
            {
                polygonMaskEditor.polygonEditor.SceneGUI();
            }
        }
        private void OnDisable()
        {
            Undo.undoRedoPerformed -= OnUndo;
            polygonMask.polygonData.polygonChanged -= PolygonData_polygonChanged;
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
            if(polygonMask == null)
            {
                polygonMask = m_CityDistrictInstance.gameObject.GetComponent<PolygonMask>();
            }
            m_editorUtils.Initialize();

            //m_editorUtils.Panel("GeneralSettingsPanel", GeneralSettingsPanel, m_defaultOpenPanels);
            if(m_defaultOpenPanels)
            {
                polygonPanelExpanded = m_editorUtils.Panel("PolygonPanel", PolygonPanel, m_defaultOpenPanels);
                roadGenerationSettingsPanelExpanded = m_editorUtils.Panel("RoadGenerationSettingsPanel", RoadGenerationSettingsPanel, m_defaultOpenPanels);
                lotGenerationSettingsPanelExpanded = m_editorUtils.Panel("LotGenerationSettingsPanel", LotGenerationSettingsPanel, m_defaultOpenPanels);
                districtBuildingSettingsPanelExpanded = m_editorUtils.Panel("DistrictBuildingSettingsPanel", DistrictBuildingSettingsPanel, m_defaultOpenPanels);
            }
            else
            {
                polygonPanelExpanded = m_editorUtils.Panel("PolygonPanel", PolygonPanel, polygonPanelExpanded);
                roadGenerationSettingsPanelExpanded = m_editorUtils.Panel("RoadGenerationSettingsPanel", RoadGenerationSettingsPanel, roadGenerationSettingsPanelExpanded);
                lotGenerationSettingsPanelExpanded = m_editorUtils.Panel("LotGenerationSettingsPanel", LotGenerationSettingsPanel, lotGenerationSettingsPanelExpanded);
                districtBuildingSettingsPanelExpanded = m_editorUtils.Panel("DistrictBuildingSettingsPanel", DistrictBuildingSettingsPanel, districtBuildingSettingsPanelExpanded);
            }


            serializedObject.ApplyModifiedProperties();
        }
        public void PolygonPanel(bool helpActive)
        {
            PropField("UseCityPolygon", p_UseCityPolygon, helpActive);
            if(!p_UseCityPolygon.boolValue)
            {
                polygonMaskEditor.QuickStartPanel(helpActive);
                polygonMaskEditor.DrawPolygonDataObjectNoLayers(helpActive);
            }

        }

        public void GeneralSettingsPanel(bool helpActive)
        {
            PropField("DistrictPolygon", p_DistrictPolygon, helpActive);
        }

        public void RoadGenerationSettingsPanel(bool helpActive)
        {
            PropField("MajorRoadSettings", p_MajorRoadSettings, helpActive);
            m_editorUtils.Panel("MajorRoadSettingsPanel", MajorRoadSettingsPanel, false);
            PropField("MinorRoadSettings", p_MinorRoadSettings, helpActive);
            m_editorUtils.Panel("MinorRoadSettingsPanel", MinorRoadSettingsPanel, false);
        }

        public void MajorRoadSettingsPanel(bool helpActive)
        {
            if (p_MajorRoadSettings.objectReferenceValue != null)
            {
                if (majorGlobalRoadSettingsEditor == null)
                {
                    // Create an editor for the sub-object and draw its inspector
                    majorGlobalRoadSettingsEditor = CreateEditor(p_MajorRoadSettings.objectReferenceValue) as GlobalRoadSettingsEditor;
                    majorGlobalRoadSettingsEditor.m_defaultOpenPanels = false;
                }
                majorGlobalRoadSettingsEditor.OnInspectorGUI();
            }
            else
            {
                EditorGUILayout.HelpBox("No Major Road Settings Assigned.", MessageType.Warning);
            }
        }

        public void MinorRoadSettingsPanel(bool helpActive)
        {
            if (p_MinorRoadSettings.objectReferenceValue != null)
            {

                if (minorGlobalRoadSettingsEditor == null)
                {
                    // Create an editor for the sub-object and draw its inspector
                    minorGlobalRoadSettingsEditor = CreateEditor(p_MinorRoadSettings.objectReferenceValue) as GlobalRoadSettingsEditor;
                    minorGlobalRoadSettingsEditor.m_defaultOpenPanels = false;
                }
                minorGlobalRoadSettingsEditor.OnInspectorGUI();
            }
            else
            {
                EditorGUILayout.HelpBox("No Minor Road Settings Assigned.", MessageType.Warning);
            }
        }

        public void LotGenerationSettingsPanel(bool helpActive)
        {
            PropField("GenerateExteriorPlots", p_GenerateExteriorPlots, helpActive);
            PropField("MaxLotSize", p_MaxLotSize, helpActive);
            PropField("MinLotSideLength", p_MinLotSideLength, helpActive);
            PropField("AllowInternalLots", p_AllowInternalLots, helpActive);
        }

        public void DistrictBuildingSettingsPanel(bool helpActive)
        {
            PropField("BaseBuildingHeight", p_BaseBuildingHeight, helpActive);
            PropField("BuildingRandomness", p_BuildingRandomness, helpActive);
            PropField("BuildingHeightCurve", p_BuildingHeightCurve, helpActive);
            EditorGUI.indentLevel++;
            PropField("DistrictBuildingTypes", p_DistrictBuildingTypes, helpActive);
            EditorGUI.indentLevel--;
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

