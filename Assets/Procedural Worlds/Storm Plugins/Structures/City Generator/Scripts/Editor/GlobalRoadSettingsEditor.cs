using ProceduralWorlds.Storm.StormEditor;
using PWCommon5;
using UnityEditor;
using UnityEngine;
namespace ProceduralWorlds.Storm.CityBuilder
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(GlobalRoadSettings))]
    public class GlobalRoadSettingsEditor : PWEditor
    {
        public bool m_defaultOpenPanels = false;

        // Property accessors for panel expansion states - point directly to the GlobalRoadSettings instance
        public bool generalSettingsPanelExpanded { get { return m_GlobalRoadSettingsInstance.generalSettingsPanelExpanded; } set { m_GlobalRoadSettingsInstance.generalSettingsPanelExpanded = value; } }
        public bool populationMapSettingsPanelExpanded { get { return m_GlobalRoadSettingsInstance.populationMapSettingsPanelExpanded; } set { m_GlobalRoadSettingsInstance.populationMapSettingsPanelExpanded = value; } }
        public bool waterMapSettingsPanelExpanded { get { return m_GlobalRoadSettingsInstance.waterMapSettingsPanelExpanded; } set { m_GlobalRoadSettingsInstance.waterMapSettingsPanelExpanded = value; } }
        public bool slopeMapSettingsPanelExpanded { get { return m_GlobalRoadSettingsInstance.slopeMapSettingsPanelExpanded; } set { m_GlobalRoadSettingsInstance.slopeMapSettingsPanelExpanded = value; } }

        EditorUtils m_editorUtils;

        SerializedProperty p_RoadType;
        SerializedProperty p_MajorRoadPlacementType;
        SerializedProperty p_MinorRoadPlacementType;
        SerializedProperty p_RoadSegmentLength;
        SerializedProperty p_MaxRoadCurve;
        SerializedProperty p_DegreeCheckInterval;
        SerializedProperty p_RoadBranchChance;
       // SerializedProperty p_IntersectionDeletionProbability;
        SerializedProperty p_MaxRoadConnections;
        SerializedProperty p_MinAngleBetweenConnections;
        SerializedProperty p_StretchConnectionRadius;
        SerializedProperty p_RoadWidth;
        SerializedProperty p_MinorRoadOffshootFrequency;
        SerializedProperty p_OffshootVariance;
        SerializedProperty p_OffshootFailChance;
        SerializedProperty p_SidewalkWidth;
        SerializedProperty p_SeekPopulation;
        SerializedProperty p_FailIfLowPopulation;
        SerializedProperty p_BasePopCutoffValue;
        SerializedProperty p_CutoffPopModifyMin;
        SerializedProperty p_CutoffPopModifyMax;
        SerializedProperty p_AvoidWater;
        SerializedProperty p_BaseWaterCutoffValue;
        SerializedProperty p_CutoffWaterModifyMin;
        SerializedProperty p_CutoffWaterModifyMax;
        SerializedProperty p_AllowBridges;
        SerializedProperty p_BridgeStretchDistance;
        SerializedProperty p_ReactToSlope;
        SerializedProperty p_BaseSlopeCutoffValue;
        SerializedProperty p_CutoffSlopeModifyMin;
        SerializedProperty p_CutoffSlopeModifyMax;

        GlobalRoadSettings m_GlobalRoadSettingsInstance = null;

        private void OnEnable()
        {
            p_RoadType = serializedObject.FindProperty("roadType");
            p_MajorRoadPlacementType = serializedObject.FindProperty("majorRoadPlacementType");
            p_MinorRoadPlacementType = serializedObject.FindProperty("minorRoadPlacementType");
            p_RoadSegmentLength = serializedObject.FindProperty("roadSegmentLength");
            p_MaxRoadCurve = serializedObject.FindProperty("maxRoadCurve");
            p_DegreeCheckInterval = serializedObject.FindProperty("degreeCheckInterval");
            p_RoadBranchChance = serializedObject.FindProperty("roadBranchChance");
            //p_IntersectionDeletionProbability = serializedObject.FindProperty("intersectionDeletionProbability");
            p_MaxRoadConnections = serializedObject.FindProperty("maxRoadConnections");
            p_MinAngleBetweenConnections = serializedObject.FindProperty("minAngleBetweenConnections");
            p_StretchConnectionRadius = serializedObject.FindProperty("stretchConnectionRadius");
            p_RoadWidth = serializedObject.FindProperty("roadWidth");
            p_MinorRoadOffshootFrequency = serializedObject.FindProperty("minorRoadOffshootFrequency");
            p_OffshootVariance = serializedObject.FindProperty("offshootVariance");
            p_OffshootFailChance = serializedObject.FindProperty("offshootFailChance");
            p_SidewalkWidth = serializedObject.FindProperty("sidewalkWidth");
            p_SeekPopulation = serializedObject.FindProperty("seekPopulation");
            p_FailIfLowPopulation = serializedObject.FindProperty("failIfLowPopulation");
            p_BasePopCutoffValue = serializedObject.FindProperty("basePopCutoffValue");
            p_CutoffPopModifyMin = serializedObject.FindProperty("cutoffPopModifyMin");
            p_CutoffPopModifyMax = serializedObject.FindProperty("cutoffPopModifyMax");
            p_AvoidWater = serializedObject.FindProperty("avoidWater");
            p_BaseWaterCutoffValue = serializedObject.FindProperty("baseWaterCutoffValue");
            p_CutoffWaterModifyMin = serializedObject.FindProperty("cutoffWaterModifyMin");
            p_CutoffWaterModifyMax = serializedObject.FindProperty("cutoffWaterModifyMax");
            p_AllowBridges = serializedObject.FindProperty("allowBridges");
            p_BridgeStretchDistance = serializedObject.FindProperty("bridgeStretchDistance");
            p_ReactToSlope = serializedObject.FindProperty("reactToSlope");
            p_BaseSlopeCutoffValue = serializedObject.FindProperty("baseSlopeCutoffValue");
            p_CutoffSlopeModifyMin = serializedObject.FindProperty("cutoffSlopeModifyMin");
            p_CutoffSlopeModifyMax = serializedObject.FindProperty("cutoffSlopeModifyMax");

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
            m_GlobalRoadSettingsInstance = (GlobalRoadSettings)target;

            m_editorUtils.Initialize();

            if(m_defaultOpenPanels)
            {
                generalSettingsPanelExpanded = m_editorUtils.Panel("GeneralSettingsPanel", GeneralSettingsPanel, m_defaultOpenPanels);
                populationMapSettingsPanelExpanded = m_editorUtils.Panel("PopulationMapSettingsPanel", PopulationMapSettingsPanel, m_defaultOpenPanels);
                waterMapSettingsPanelExpanded = m_editorUtils.Panel("WaterMapSettingsPanel", WaterMapSettingsPanel, m_defaultOpenPanels);
                slopeMapSettingsPanelExpanded = m_editorUtils.Panel("SlopeMapSettingsPanel", SlopeMapSettingsPanel, m_defaultOpenPanels);
            }
            else
            {
                generalSettingsPanelExpanded = m_editorUtils.Panel("GeneralSettingsPanel", GeneralSettingsPanel, generalSettingsPanelExpanded);
                populationMapSettingsPanelExpanded = m_editorUtils.Panel("PopulationMapSettingsPanel", PopulationMapSettingsPanel, populationMapSettingsPanelExpanded);
                waterMapSettingsPanelExpanded = m_editorUtils.Panel("WaterMapSettingsPanel", WaterMapSettingsPanel, waterMapSettingsPanelExpanded);
                slopeMapSettingsPanelExpanded = m_editorUtils.Panel("SlopeMapSettingsPanel", SlopeMapSettingsPanel, slopeMapSettingsPanelExpanded);
            }

            serializedObject.ApplyModifiedProperties();
        }

        public void GeneralSettingsPanel(bool helpActive)
        {
            PropField("RoadType", p_RoadType, helpActive);
            if(p_RoadType.enumValueIndex == 0)
            {
                PropField("MajorRoadPlacementType", p_MajorRoadPlacementType, helpActive);
            }
            else
            {
                PropField("MinorRoadPlacementType", p_MinorRoadPlacementType, helpActive);
            }


            PropField("RoadSegmentLength", p_RoadSegmentLength, helpActive);
            PropField("MaxRoadCurve", p_MaxRoadCurve, helpActive);
            PropField("DegreeCheckInterval", p_DegreeCheckInterval, helpActive);
            PropField("RoadBranchChance", p_RoadBranchChance, helpActive);
            //PropField("IntersectionDeletionProbability", p_IntersectionDeletionProbability, helpActive);

            
            // Always show the warning if the value is above 0.5, even if it wasn't just changed
            //if (p_IntersectionDeletionProbability.floatValue > 0.5f)
            //{
            //    EditorGUILayout.HelpBox("Warning: Setting intersection deletion probability above 0.5 may result in strange road layouts, as more than half of the intersections will be removed.", MessageType.Warning);
            //}
            
            PropField("MaxRoadConnections", p_MaxRoadConnections, helpActive);
            PropField("MinAngleBetweenConnections", p_MinAngleBetweenConnections, helpActive);
            
            // Enforce constraint: StretchConnectionRadius cannot exceed 4/5ths of RoadSegmentLength
            EditorGUI.BeginChangeCheck();
            PropField("StretchConnectionRadius", p_StretchConnectionRadius, helpActive);
            if (EditorGUI.EndChangeCheck())
            {
                // Calculate the maximum allowed value (4/5ths of RoadSegmentLength)
                float maxAllowedRadius = p_RoadSegmentLength.floatValue * 0.8f;
                
                // Ensure the value doesn't exceed the maximum
                if (p_StretchConnectionRadius.floatValue > maxAllowedRadius)
                {
                    p_StretchConnectionRadius.floatValue = maxAllowedRadius;
                    Debug.LogWarning("StretchConnectionRadius cannot exceed 4/5ths of RoadSegmentLength. Value has been clamped.");
                }
            }

            if(p_RoadType.enumValueIndex == 0)
            {
                PropField("MinorRoadOffshootFrequency", p_MinorRoadOffshootFrequency, helpActive);
                PropField("OffshootVariance", p_OffshootVariance, helpActive);
                
                // check when MinorRoadOffshootFrequency changes
                if (p_OffshootVariance.floatValue >= p_MinorRoadOffshootFrequency.floatValue - 1f)
                {
                    Debug.LogWarning("OffshootVariance cannot exceed (MinorRoadOffshootFrequency - 1). Value has been clamped.");
                    p_OffshootVariance.floatValue = p_MinorRoadOffshootFrequency.floatValue - 1f;
                }

                PropField("OffshootFailChance", p_OffshootFailChance, helpActive);
            }

            PropField("RoadWidth", p_RoadWidth, helpActive);
            PropField("SidewalkWidth", p_SidewalkWidth, helpActive);
        }

        public void PopulationMapSettingsPanel(bool helpActive)
        {
            PropField("SeekPopulation", p_SeekPopulation, helpActive);
            if (p_SeekPopulation.boolValue)
            {
                PropField("FailIfLowPopulation", p_FailIfLowPopulation, helpActive);
                PropField("BasePopCutoffValue", p_BasePopCutoffValue, helpActive);
                PropField("CutoffPopModifyMin", p_CutoffPopModifyMin, helpActive);
                PropField("CutoffPopModifyMax", p_CutoffPopModifyMax, helpActive);
            }
        }

        public void WaterMapSettingsPanel(bool helpActive)
        {
            PropField("AvoidWater", p_AvoidWater, helpActive);
            if(p_AvoidWater.boolValue)
            {
                //PropField("BaseWaterCutoffValue", p_BaseWaterCutoffValue, helpActive);
                //PropField("CutoffWaterModifyMin", p_CutoffWaterModifyMin, helpActive);
                //PropField("CutoffWaterModifyMax", p_CutoffWaterModifyMax, helpActive);

            }
            PropField("AllowBridges", p_AllowBridges, helpActive);
            if (p_AllowBridges.boolValue)
            {
                PropField("BridgeStretchDistance", p_BridgeStretchDistance, helpActive);
            }
        }

        public void SlopeMapSettingsPanel(bool helpActive)
        {
            PropField("ReactToSlope", p_ReactToSlope, helpActive);
            if (p_ReactToSlope.boolValue)
            {
                PropField("BaseSlopeCutoffValue", p_BaseSlopeCutoffValue, helpActive);
                PropField("CutoffSlopeModifyMin", p_CutoffSlopeModifyMin, helpActive);
                PropField("CutoffSlopeModifyMax", p_CutoffSlopeModifyMax, helpActive);
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
