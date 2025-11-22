#if GAIA_2023
using Gaia;
#endif

#if EASYROADS3D_PRO
using EasyRoads3Dv3;
#endif

#if GENA_PRO
using GeNa.Core;
#endif

using ProceduralWorlds.Storm.BuildingGen;
using ProceduralWorlds.Storm.Core;
using ProceduralWorlds.Storm.StormEditor;
using ProceduralWorlds.Storm.StructureGenCore;
using PWCommon5;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

using XorshiftPlus = ProceduralWorlds.Storm.StructureGenCore.XorshiftPlus;

using ProceduralWorlds.Storm.UI;

namespace ProceduralWorlds.Storm.CityBuilder
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(CityManager))]
    public class CityGeneratorEditor : PWEditor
    {
        public bool m_defaultOpenPanels = false;
        
        // Property accessors for panel expansion states - point directly to the CityManager instance
        public bool polygonPanelExpanded { get { return m_CityGenInstance.polygonPanelExpanded; } set { m_CityGenInstance.polygonPanelExpanded = value; } }
        public bool generationPanelExpanded { get { return m_CityGenInstance.generationPanelExpanded; } set { m_CityGenInstance.generationPanelExpanded = value; } }
        public bool visualizationPanelExpanded { get { return m_CityGenInstance.visualizationPanelExpanded; } set { m_CityGenInstance.visualizationPanelExpanded = value; } }
        
        EditorUtils m_editorUtils;

        SerializedProperty p_AutoGenSeed;
        SerializedProperty p_Seed;

        SerializedProperty p_CityDistricts;

        SerializedProperty p_RoadNetworkData;
        SerializedProperty p_DefaultMajorRoadSettings;
        SerializedProperty p_MajorRoadStartType;
        //SerializedProperty p_MajorRadialPoints;

        SerializedProperty p_DefaultMinorRoadSettings;
        //SerializedProperty p_MinorRadialPoints;

        SerializedProperty p_OverrideWaterLevel;
        SerializedProperty p_WaterLevel;
        SerializedProperty p_PopulationMap;


        SerializedProperty p_ShowGizmos;
        SerializedProperty p_AutoUpdatePreviewOnVarChange;
        SerializedProperty p_ShowBlockPlots;
        SerializedProperty p_ShowAllowedSlope;
        SerializedProperty p_SlopePreviewColor;


        SerializedProperty p_buildingParentPrefab;

        SerializedProperty p_polygonMaskLayerSettings;


        #region RoadGen
        SerializedProperty p_CityRoadSpawnType;
#if EASYROADS3D_PRO
        SerializedProperty p_TConnectionObjMajor;
        SerializedProperty p_XConnectionObjMajor;
        SerializedProperty p_LConnectionObjMajor;
        SerializedProperty p_TConnectionObjMinor;
        SerializedProperty p_XConnectionObjMinor;
        SerializedProperty p_LConnectionObjMinor;

        SerializedProperty p_MajorRoadSideObjects;
        SerializedProperty p_MinorRoadSideObjects;
#endif
        #endregion

        CityManager m_CityGenInstance = null;


        /// <summary>
        /// The co-routine we are currently working on
        /// </summary>
        private IEnumerator m_currentCoroutine;
        private DateTime m_timeSinceLastUpdate;
        //private int m_currentWaitTimeMS = 0;



        Vector3CompareBehavior vector3Comparer = new Vector3CompareBehavior();

        Dictionary<Vector3, RoadPointInformation> majorRoadPoints { get { return m_CityGenInstance.roadNetworkData.majorRoadPoints; } set { m_CityGenInstance.roadNetworkData.majorRoadPoints = majorRoadPoints; } }


        Dictionary<Vector3, RoadPointInformation> minorRoadPoints { get { return m_CityGenInstance.roadNetworkData.minorRoadPoints; } set { m_CityGenInstance.roadNetworkData.minorRoadPoints = minorRoadPoints; } }



        Dictionary<Vector3, RoadPointInformation> allRoadPoints;


        List<IntersectionInformation> intersections { get { return m_CityGenInstance.roadNetworkData.intersections; } set { m_CityGenInstance.roadNetworkData.intersections = intersections; } }


        List<BlockInfo> blockInfo { get { return m_CityGenInstance.blockInfo; } set { m_CityGenInstance.blockInfo = blockInfo; } }

        List<LandPlot> exteriorPlots { get { return m_CityGenInstance.exteriorPlots; } set { m_CityGenInstance.exteriorPlots = exteriorPlots; } }

        List<Vector3> pointStorage { get { return m_CityGenInstance.pointStorage; } set { m_CityGenInstance.pointStorage = pointStorage; } }

        List<GameObject> spawnedObjects = new List<GameObject>();


        Transform transform;

        XorshiftPlus randomGen;

        private GameObject buildingParent;

        private ObjectInstantiationManager objectInstantiationManager;



        bool flattenTerrain = false;

        private GameObject roadObject;


        private CityDistrictEditor cityDistrictEditor;
        private CityDistrict selectedCityDistrictInfo;
        private GlobalRoadSettingsEditor majorGlobalRoadSettingsEditor;
        private GlobalRoadSettingsEditor minorGlobalRoadSettingsEditor;

        private DateTime lastChangeTime;
        private bool gizmosUpToDate = false;

        private StormPolygonMask polygonMask = null;

        private StormPolygonMaskEditor polygonMaskEditor = null;

        // Property accessor for selected district index - points directly to the CityManager instance
        private int SelectedDistrictInt
        {
            get { return m_CityGenInstance.selectedDistrictInt; }
            set 
            { 
                if (m_CityGenInstance.selectedDistrictInt != value)
                {
                    m_CityGenInstance.selectedDistrictInt = value;
                    EditorUtility.SetDirty(m_CityGenInstance);
                }
            }
        }

        private void StartEditorUpdates(IEnumerator function)
        {
            //remember what we are working on
            m_currentCoroutine = function;
            //subscribe to the editor update event
            //commented this so i can update gizmos in that consistant function as well
            //EditorApplication.update -= EditorUpdate;
            //EditorApplication.update += EditorUpdate;
        }

        private void StopEditorUpdates()
        {
            //forget what we are working on
            m_currentCoroutine = null;
            //unsubscribe to the editor update event
            //commented this so i can update gizmos in that consistant function as well
            //EditorApplication.update -= EditorUpdate;

        }

        private void EditorUpdate()
        {
            if (m_currentCoroutine != null)
            {

                if (EditorUtility.DisplayCancelableProgressBar("City Generator", "Performing Action...", 0f))
                {
                    StopEditorUpdates();
                    EditorUtility.ClearProgressBar();
                    return;
                }

                m_currentCoroutine.MoveNext();
                /*
                double timeSinceLastUpdateinMS = (DateTime.Now - m_timeSinceLastUpdate).TotalMilliseconds;

                //force a update frequency between update calls - you can leave that out if not needed / wanted
                if (timeSinceLastUpdateinMS > ((EditorCoroutineExample)target).m_UpdateFrequencyMS)
                {
                    if (timeSinceLastUpdateinMS > m_currentWaitTimeMS)
                    {
                        //reset wait time & timestamp
                        m_currentWaitTimeMS = 0;
                        m_timeSinceLastUpdate = DateTime.Now;

                        //this continues the execution of the coroutine function from the last yield where we stopped
                        
                    }
                }
                */
            }
            else
            {
                double timeSinceLastUpdateinMS = (DateTime.Now - lastChangeTime).TotalMilliseconds;
                if (timeSinceLastUpdateinMS > 2000 && gizmosUpToDate == false)
                {
                    m_CityGenInstance.m_previewDirty = true;
                    UpdateGizmos();
                    gizmosUpToDate = true;
                }
            }
        }

        private void OnEnable()
        {
            p_AutoGenSeed = serializedObject.FindProperty("autoGenSeed");
            p_Seed = serializedObject.FindProperty("m_seed");

            p_CityDistricts = serializedObject.FindProperty("m_cityDistricts");

            p_RoadNetworkData = serializedObject.FindProperty("roadNetworkData");

            p_DefaultMajorRoadSettings = serializedObject.FindProperty("m_defaultMajorRoadSettings");
            p_MajorRoadStartType = serializedObject.FindProperty("majorRoadStartType");
            //p_MajorRadialPoints = serializedObject.FindProperty("majorRadialPoints");

            p_DefaultMinorRoadSettings = serializedObject.FindProperty("m_defaultMinorRoadSettings");
            //p_MinorRadialPoints = serializedObject.FindProperty("minorRadialPoints");

            p_OverrideWaterLevel = serializedObject.FindProperty("overrideWaterLevel");
            p_WaterLevel = serializedObject.FindProperty("waterLevel");
            p_PopulationMap = serializedObject.FindProperty("m_populationMap");

            p_ShowGizmos = serializedObject.FindProperty("showGizmos");
            p_AutoUpdatePreviewOnVarChange = serializedObject.FindProperty("autoUpdatePreviewOnVarChange");
            p_ShowBlockPlots = serializedObject.FindProperty("showBlockPlots");
            p_ShowAllowedSlope = serializedObject.FindProperty("showAllowedSlope");
            p_SlopePreviewColor = serializedObject.FindProperty("slopePreviewColor");

            p_buildingParentPrefab = serializedObject.FindProperty("buildingParentPrefab");
            p_polygonMaskLayerSettings = serializedObject.FindProperty("polygonMaskLayerSettings");

            p_CityRoadSpawnType = serializedObject.FindProperty("cityRoadSpawnType");

#if EASYROADS3D_PRO
            p_TConnectionObjMajor = serializedObject.FindProperty("TConnectionObjMajor");
            p_XConnectionObjMajor = serializedObject.FindProperty("XConnectionObjMajor");
            p_LConnectionObjMajor = serializedObject.FindProperty("LConnectionObjMajor");
            p_TConnectionObjMinor = serializedObject.FindProperty("TConnectionObjMinor");
            p_XConnectionObjMinor = serializedObject.FindProperty("XConnectionObjMinor");
            p_LConnectionObjMinor = serializedObject.FindProperty("LConnectionObjMinor");

            p_MajorRoadSideObjects = serializedObject.FindProperty("majorRoadSideObjects");
            p_MinorRoadSideObjects = serializedObject.FindProperty("minorRoadSideObjects");
#endif
            Undo.undoRedoPerformed -= OnUndo;
            Undo.undoRedoPerformed += OnUndo;
            EditorApplication.update -= EditorUpdate;
            EditorApplication.update += EditorUpdate;
            m_CityGenInstance = (CityManager)target;
            polygonMask = m_CityGenInstance.GetComponent<PolygonMask>();
            if(polygonMask == null)
            {
                polygonMask = m_CityGenInstance.gameObject.AddComponent<PolygonMask>();
            }
            if(polygonMask != null)
            {
                polygonMaskEditor = (StormPolygonMaskEditor)CreateEditor(polygonMask, typeof(StormPolygonMaskEditor));
                polygonMask.polygonData.polygonChanged += PolygonData_polygonChanged;
                polygonMask.hideFlags = HideFlags.HideInInspector;
            }
        }

        private void PolygonData_polygonChanged(bool obj)
        {

        }

        private void OnDisable()
        {
            StopEditorUpdates();
            Undo.undoRedoPerformed -= OnUndo;
            EditorApplication.update -= EditorUpdate;
            if(polygonMask != null)
            {
                polygonMask.polygonData.polygonChanged -= PolygonData_polygonChanged;
            }
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
            m_CityGenInstance = (CityManager)target;


            m_editorUtils.Initialize();
            EditorGUI.BeginChangeCheck();
            if(m_defaultOpenPanels)
            {
                polygonPanelExpanded = m_editorUtils.Panel("PolygonPanel", PolygonPanel, m_defaultOpenPanels);
                generationPanelExpanded = m_editorUtils.Panel("GenerationPanel", GenerationPanel, m_defaultOpenPanels);
                visualizationPanelExpanded = m_editorUtils.Panel("VisualizationPanel", VisualizationPanel, m_defaultOpenPanels);
            }
            else
            {
                polygonPanelExpanded = m_editorUtils.Panel("PolygonPanel", PolygonPanel, polygonPanelExpanded);
                generationPanelExpanded = m_editorUtils.Panel("GenerationPanel", GenerationPanel, generationPanelExpanded);
                visualizationPanelExpanded = m_editorUtils.Panel("VisualizationPanel", VisualizationPanel, visualizationPanelExpanded);
            }
            bool changesMade = EditorGUI.EndChangeCheck();


            if(m_CityGenInstance.m_defaultMajorRoadSettings == null)
            {
                EditorGUILayout.HelpBox("No Major Road Settings Assigned, please assign one to use the city generator", MessageType.Warning);
            }
            if(m_CityGenInstance.m_defaultMinorRoadSettings == null)
            {
                EditorGUILayout.HelpBox("No Minor Road Settings Assigned, please assign one to use the city generator", MessageType.Warning);
            }
            if(m_CityGenInstance.roadNetworkData == null)
            {
                EditorGUILayout.HelpBox("No Road Network Data Assigned, please assign one to use the city generator", MessageType.Warning);
            }

            bool originalGUIState = GUI.enabled;
            if(m_CityGenInstance.m_defaultMajorRoadSettings == null || m_CityGenInstance.m_defaultMinorRoadSettings == null || m_CityGenInstance.roadNetworkData == null)
            {
                GUI.enabled = false;
            }


            if (m_editorUtils.Button("IterateRoads"))
            {
                StartEditorUpdates(IterateRoads());
            }

            
            if (m_editorUtils.Button("IteratePlots"))
            {
                StartEditorUpdates(IteratePlots());
            }


            if (m_editorUtils.Button("Spawn"))
            {
                StartEditorUpdates(Spawn());
            }
            if (m_editorUtils.Button("Clear"))
            {
                Clear();
            }
            if (m_editorUtils.Button("Apply"))
            {
                Apply();
            }
            GUI.enabled = originalGUIState;

            serializedObject.ApplyModifiedProperties();

            if (changesMade)
            {
                gizmosUpToDate = false;
                lastChangeTime = DateTime.Now;
            }
        }

        public void OnSceneGUI()
        {
            if(polygonMaskEditor != null)
            {
                polygonMaskEditor.polygonEditor.SceneGUI();
            }
        }

        public void PolygonPanel(bool helpActive)
        {
            polygonMaskEditor.QuickStartPanel(helpActive);
            polygonMaskEditor.DrawPolygonDataObjectNoLayers(helpActive, true);
        }
        public void GenerationPanel(bool helpActive)
        {
            m_editorUtils.Heading("GeneralSettings", GUILayout.ExpandWidth(false));
            PropField("AutoGenSeed", p_AutoGenSeed, helpActive);
            PropField("Seed", p_Seed, helpActive);

            PropField("OverrideWaterLevel", p_OverrideWaterLevel, helpActive);
            if (p_OverrideWaterLevel.boolValue)
            {
                PropField("WaterLevel", p_WaterLevel, helpActive);
            }
            else
            {
#if GAIA_2023 || GAIA_2023_PRO
                m_CityGenInstance.waterLevel = GaiaAPI.GetSeaLevel();
#endif
                bool originalState = GUI.enabled;
                GUI.enabled = false;
                PropField("WaterLevel", p_WaterLevel, helpActive);
                GUI.enabled = originalState;
            }

            //PropField("PopulationMap", p_PopulationMap, helpActive);



            m_editorUtils.Panel("CityDistrictsPanel", CityDistrictsPanel, false);

            
            m_editorUtils.Panel("RoadSettingsPanel", RoadSettingsPanel, false);


            m_editorUtils.Heading("BuildingSettings", GUILayout.ExpandWidth(false));
            PropField("BuildingParentPrefab", p_buildingParentPrefab, helpActive);
            EditorGUI.indentLevel++;
            PropField("PolygonMaskLayerSettings", p_polygonMaskLayerSettings, helpActive);
            EditorGUI.indentLevel--;
        }

        public void CityDistrictsPanel(bool helpActive)
        {
            ++EditorGUI.indentLevel;
            PropField("CityDistricts", p_CityDistricts, helpActive);
            --EditorGUI.indentLevel;

            m_editorUtils.Heading("CityDistrictSettings", GUILayout.ExpandWidth(false));
            List<CityDistrict> cityDistricts = m_CityGenInstance.m_cityDistricts;
            if (cityDistricts.Count == 0)
            {
                EditorGUILayout.HelpBox("No City Districts Assigned.", MessageType.Warning);
                return;
            }

            // Filter out null entries
            List<CityDistrict> validDistricts = new List<CityDistrict>();
            foreach (CityDistrict district in cityDistricts)
            {
                if (district != null)
                {
                    validDistricts.Add(district);
                }
            }

            if (validDistricts.Count == 0)
            {
                EditorGUILayout.HelpBox("No valid City Districts found.", MessageType.Warning);
                return;
            }

            // Create display names and indices arrays for the IntPopup
            string[] districtNames = new string[validDistricts.Count];
            int[] districtIndices = new int[validDistricts.Count];

            for (int i = 0; i < validDistricts.Count; i++)
            {
                districtNames[i] = validDistricts[i].name;
                districtIndices[i] = cityDistricts.IndexOf(validDistricts[i]);
            }

            // Map the current selection to the filtered list
            int filteredSelectedIndex = 0;
            if (SelectedDistrictInt >= 0 && SelectedDistrictInt < cityDistricts.Count && cityDistricts[SelectedDistrictInt] != null)
            {
                int indexInFilteredList = validDistricts.IndexOf(cityDistricts[SelectedDistrictInt]);
                if (indexInFilteredList >= 0)
                {
                    filteredSelectedIndex = indexInFilteredList;
                }
            }

            // Begin change check to detect when the selection changes
            EditorGUI.BeginChangeCheck();
            
            // Draw the IntPopup with filtered valid districts
            int newSelectedIndex = EditorGUILayout.IntPopup("Selected District:", filteredSelectedIndex, districtNames, districtIndices);
            
            // If selection changed, update the property and recreate the editor
            if (EditorGUI.EndChangeCheck())
            {
                if (newSelectedIndex >= 0 && newSelectedIndex < districtIndices.Length)
                {
                    SelectedDistrictInt = districtIndices[newSelectedIndex];
                }
            }

            // Check if selection is valid
            if (SelectedDistrictInt >= cityDistricts.Count || SelectedDistrictInt < 0)
            {
                EditorGUILayout.HelpBox("Selected District Outside of Range.", MessageType.Warning);
                return;
            }

            // Create or update the editor for the selected district
            if (selectedCityDistrictInfo != cityDistricts[SelectedDistrictInt] || cityDistrictEditor == null)
            {
                selectedCityDistrictInfo = cityDistricts[SelectedDistrictInt];

                // Create an editor for the sub-object and draw its inspector
                cityDistrictEditor = CreateEditor(selectedCityDistrictInfo) as CityDistrictEditor;
                cityDistrictEditor.m_defaultOpenPanels = false;
            }

            // Draw the district editor
            cityDistrictEditor.OnInspectorGUI();
        }

        public void RoadSettingsPanel(bool helpActive)
        {
            PropField("DefaultMajorRoadSettings", p_DefaultMajorRoadSettings, helpActive);
            m_editorUtils.Panel("MajorRoadSettingsPanel", MajorRoadSettingsPanel, false);
            PropField("MajorRoadStartType", p_MajorRoadStartType, helpActive);
            /*
            if (m_CityGenInstance.majorRoadStartType == MajorRoadStartType.Radial)
            {
                ++EditorGUI.indentLevel;
                //PropField("MajorRadialPoints", p_MajorRadialPoints, helpActive);
                --EditorGUI.indentLevel;
            }
            */

            PropField("DefaultMinorRoadSettings", p_DefaultMinorRoadSettings, helpActive);
            m_editorUtils.Panel("MinorRoadSettingsPanel", MinorRoadSettingsPanel, false);
            ++EditorGUI.indentLevel;
            //PropField("MinorRadialPoints", p_MinorRadialPoints, helpActive);
            --EditorGUI.indentLevel;
            EditorGUILayout.Space();
            PropField("CityRoadSpawnType", p_CityRoadSpawnType, helpActive);
            PropField("RoadNetworkData", p_RoadNetworkData, helpActive);

#if EASYROADS3D_PRO
            if (m_CityGenInstance.cityRoadSpawnType == CityRoadSpawnType.EasyRoads)
            {
                PropField("TConnectionObjMajor", p_TConnectionObjMajor, helpActive);
                PropField("XConnectionObjMajor", p_XConnectionObjMajor, helpActive);
                PropField("LConnectionObjMajor", p_LConnectionObjMajor, helpActive);
                PropField("TConnectionObjMinor", p_TConnectionObjMinor, helpActive);
                PropField("XConnectionObjMinor", p_XConnectionObjMinor, helpActive);
                PropField("LConnectionObjMinor", p_LConnectionObjMinor, helpActive);
                ++EditorGUI.indentLevel;
                PropField("MajorRoadSideObjects", p_MajorRoadSideObjects, helpActive);
                PropField("MinorRoadSideObjects", p_MinorRoadSideObjects, helpActive);
                --EditorGUI.indentLevel;
            }
#endif
        }

        public void MajorRoadSettingsPanel(bool helpActive)
        {
            if (p_DefaultMajorRoadSettings.objectReferenceValue != null)
            {
                if (majorGlobalRoadSettingsEditor == null)
                {
                    // Create an editor for the sub-object and draw its inspector
                    majorGlobalRoadSettingsEditor = CreateEditor(p_DefaultMajorRoadSettings.objectReferenceValue) as GlobalRoadSettingsEditor;
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
            if (p_DefaultMinorRoadSettings.objectReferenceValue != null)
            {

                if (minorGlobalRoadSettingsEditor == null)
                {
                    // Create an editor for the sub-object and draw its inspector
                    minorGlobalRoadSettingsEditor = CreateEditor(p_DefaultMinorRoadSettings.objectReferenceValue) as GlobalRoadSettingsEditor;
                    minorGlobalRoadSettingsEditor.m_defaultOpenPanels = false;
                }
                minorGlobalRoadSettingsEditor.OnInspectorGUI();
            }
            else
            {
                EditorGUILayout.HelpBox("No Minor Road Settings Assigned.", MessageType.Warning);
            }
        }

        public void VisualizationPanel(bool helpActive)
        {
            /*
            EditorGUILayout.BeginHorizontal();
            m_showHelp = GUILayout.Toggle(m_showHelp, new GUIContent(m_showHelp ? m_editorUtils.Styles.helpOn : m_editorUtils.Styles.helpOff), EditorStyles.iconButton, GUILayout.ExpandWidth(false));
            EditorGUILayout.EndHorizontal();
            */

            PropField("ShowGizmos", p_ShowGizmos, helpActive);
            if (m_CityGenInstance.showGizmos)
            {
                PropField("ShowBlockPlots", p_ShowBlockPlots, helpActive);
                PropField("ShowAllowedSlope", p_ShowAllowedSlope, helpActive);
                if (p_ShowAllowedSlope.boolValue)
                {
                    PropField("SlopePreviewColor", p_SlopePreviewColor, helpActive);
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


        private void UpdateGizmos()
        {
            return;
        }

        private IEnumerator IterateRoads()
        {
            UpdateDistrictPolygons();
            if(polygonMask != null)
            {
                m_CityGenInstance.cityArea = StructureCreatorUtils.GetPolygonPointsGlobal(polygonMask, m_CityGenInstance.transform);
            }
            PrepareForCityGen();


            Action<GameObject> fininshedGeneration = (GameObject obj) => { };


            float waterLevel = m_CityGenInstance.waterLevel;
            if (!m_CityGenInstance.overrideWaterLevel)
            {
#if GAIA_2023 || GAIA_2023_PRO
                waterLevel = GaiaAPI.GetSeaLevel();
#endif
            }

            CityGeneratorCore.PlaceMajorRoads(randomGen, m_CityGenInstance.m_cityDistricts,
                 m_CityGenInstance.majorRoadStartType, m_CityGenInstance.majorRadialPoints, intersections, majorRoadPoints
                 , m_CityGenInstance.m_defaultMajorRoadSettings, m_CityGenInstance.m_defaultMinorRoadSettings,
                m_CityGenInstance.cityArea, m_CityGenInstance.m_waterMap, waterLevel, m_CityGenInstance.m_populationMap, m_CityGenInstance.m_slopeMap, transform);
            CityGeneratorCore.PlaceMinorRoads(randomGen, m_CityGenInstance.m_cityDistricts,
                m_CityGenInstance.minorRadialPoints, intersections,
                minorRoadPoints, m_CityGenInstance.m_defaultMinorRoadSettings, m_CityGenInstance.m_defaultMajorRoadSettings, majorRoadPoints,
                m_CityGenInstance.cityArea, m_CityGenInstance.m_waterMap, waterLevel, m_CityGenInstance.m_populationMap, transform);

            RoadNetwork roadNetwork = m_CityGenInstance.GetComponent<RoadNetwork>();
            
            if(roadNetwork != null)
            {
                roadNetwork.GetAllRoadPoints();
            }

            SceneView.RepaintAll();
            EditorUtility.ClearProgressBar();
            StopEditorUpdates();
            
            yield break;
        }

        private IEnumerator IteratePlots()
        {
            UpdateDistrictPolygons();
            transform = m_CityGenInstance.transform;
            blockInfo.Clear();
            exteriorPlots.Clear();
            if(minorRoadPoints.Count == 0 && majorRoadPoints.Count == 0)
            {
                Debug.LogWarning("Cannot Create Plots Without Any Road Points");
                StopEditorUpdates();
                EditorUtility.ClearProgressBar();
                yield break;
            }
            CityGeneratorCore.CreateBlockInfoList(blockInfo, m_CityGenInstance.m_defaultMinorRoadSettings, m_CityGenInstance.m_defaultMajorRoadSettings, randomGen,
                majorRoadPoints, minorRoadPoints, allRoadPoints,
                m_CityGenInstance.m_cityDistricts, transform);

            CityGeneratorCore.CreateExteriorPlots(majorRoadPoints, ref m_CityGenInstance.exteriorPlots, m_CityGenInstance.m_cityDistricts, m_CityGenInstance.m_defaultMajorRoadSettings, m_CityGenInstance.m_defaultMinorRoadSettings, pointStorage, allRoadPoints, transform);

            SceneView.RepaintAll();
            
            StopEditorUpdates();
            EditorUtility.ClearProgressBar();
            yield break;
        }

        private IEnumerator Spawn()
        {
            if(allRoadPoints == null)
            {
                //Debug.LogError("No Road Points Found");
                StopEditorUpdates();
                EditorUtility.ClearProgressBar();
                yield break;
            }

            //find object instantiation manager
            objectInstantiationManager = FindAnyObjectByType<ObjectInstantiationManager>();
            if (objectInstantiationManager == null)
            {
                Debug.LogError("No Object Instantiation Manager In Scene, Please Add One");
                StopEditorUpdates();
                EditorUtility.ClearProgressBar();
                yield break;
            }

            if (randomGen == null)
            {
                randomGen = new XorshiftPlus(m_CityGenInstance.m_seed);
            }

            if (transform == null)
            {
                transform = m_CityGenInstance.transform;
            }


            GameObject blocksParent = new GameObject();
            blocksParent.transform.parent = transform;
            blocksParent.transform.position = transform.position;
            blocksParent.name = "Blocks";

            for (int i = 0; i < blockInfo.Count; i++)
            {
                GameObject blockBuildings = new GameObject();
                blockBuildings.transform.position = transform.position;
                blockBuildings.transform.parent = blocksParent.transform;
                blockBuildings.name = "Block Building #" + i;
                if(blockInfo[i].blockLandPlots == null)
                {
                    continue;
                    
                }

                Transform blockBuildingsTransform = blockBuildings.transform;
                foreach (LandPlot landPlot in blockInfo[i].blockLandPlots)
                {
                    if (landPlot.lotPoints.Count == 0)
                    {
                        continue;
                    }

                    CityDistrict blockDistrict = blockInfo[i].blockDistrict;
                    Vector3 centroid = CityBuilderUtils.GetCentroid(landPlot.lotPoints);

                    if (CityGeneratorCore.IsPointInPolygon(landPlot.lotPoints, centroid))
                    {
                        float districtCurveEvaluation = blockDistrict.buildingHeightCurve.Evaluate(1);
                        float buildingRandomness = (blockDistrict.buildingRandomness * randomGen.Next()) -
                                                   blockDistrict.buildingRandomness / 2;
                        int buildingLevels = Mathf.RoundToInt((blockDistrict.baseBuildingHeight + buildingRandomness) *
                                                              districtCurveEvaluation);
                        
                        DistrictBuildingType districtBuildingInfo = blockDistrict.GetBuildingToSpawn();
                        if(districtBuildingInfo == null)
                        {
                            continue;
                        }
                        if(randomGen.Next() < districtBuildingInfo.failChance)
                        {
                            continue;
                        }
                        List<Vector3> polygon = landPlot.lotPoints;


                        //cashe buildingConfig
                        BuildingConfig buildingConfig = districtBuildingInfo.houseInfo;


                        //get levels
                        int levels = buildingLevels;

                        //find axis aligned polygon and centroid
                        Vector2 centriod;
                        float rotationAmount;
                        List<Vector2> axisAlignedPolygon = StructureCreatorUtils.AlignPolygonWithAxis(polygon, out rotationAmount, out centriod);


                        //get y offset?
                        float yOffset = CityBuilderUtils.GetHeightAtPointVec3(centroid);
                        //set spawn pos
                        Vector3 spawnPos = new Vector3(centriod.x, yOffset, centriod.y);



                        //create building parent object
                        buildingParent = Instantiate(m_CityGenInstance.buildingParentPrefab.gameObject, spawnPos, Quaternion.identity);
                        buildingParent.name = "Building " + "Parent";


                        BuildingGeneratorCore.PrepBuildingInfo(buildingConfig, axisAlignedPolygon, yOffset, spawnPos, levels);

                        //set up for accurate polygon
                        List<Vector2> accuratePolygon = new List<Vector2>();

                        if(districtBuildingInfo.generateInteriors)
                        {
                            //find total area of the building
                            //calculate rooms to include and populate that information into the building info
                            BuildingGeneratorCore.PrepareForInteriorGen(buildingConfig, axisAlignedPolygon);

                            Action<List<Vector2>> levelCreationFinished = (List<Vector2> list) =>
                            {
                                accuratePolygon = list;
                            };

                            Action<BuildingLevelInfo> resetLayout = (BuildingLevelInfo buildingLevelInfo) =>
                            {
                                //ResetLayout(ref buildingLevelInfo);
                            };
                            

                            // Start the coroutine for creating levels and begin the building generation process
                            float combinedAngle = rotationAmount + landPlot.angleToRoad*Mathf.Rad2Deg;
                            /*
                            // Draw a line to visualize the combined direction
                            Vector3 direction = new Vector3(Mathf.Cos(combinedAngle * Mathf.Deg2Rad), 0, Mathf.Sin(combinedAngle * Mathf.Deg2Rad));
                            Debug.DrawLine(spawnPos, spawnPos + direction * 5f, Color.red, 30f);
                            */
                            
                            BuildingGeneratorCore.CreateLevels(buildingConfig, randomGen, buildingParent.GetComponent<GeneratedBuildingParent>(), resetLayout, null, null, true, combinedAngle, levelCreationFinished);
                        }
                        else
                        {
                            BuildingGeneratorCore.PrepareForExteriorGen(buildingConfig, buildingParent.GetComponent<GeneratedBuildingParent>(), out accuratePolygon);
                        }

                        



                        //find parent transform
                        Transform parentTransform = blockBuildingsTransform;
                        BuildingGeneratorCore.SpawnBuilding(buildingConfig, randomGen, objectInstantiationManager, buildingParent.GetComponent<GeneratedBuildingParent>(), parentTransform, districtBuildingInfo.decorateInteriors, districtBuildingInfo.generateInteriors, false, rotationAmount, yOffset, flattenTerrain, m_CityGenInstance.polygonMaskLayerSettings, accuratePolygon);
                    }

                }
            }

            GameObject exteriorPlotBuildings = new GameObject();
            exteriorPlotBuildings.transform.parent = transform;
            exteriorPlotBuildings.transform.position = transform.position;
            exteriorPlotBuildings.name = "Exterior Plot Buildings";

            Transform exteriorPlotBuildingsTransform = exteriorPlotBuildings.transform;

            for (int i = 0; i < exteriorPlots.Count; i++)
            {
                Vector3 centroidOfPlot = CityBuilderUtils.GetCentroid(exteriorPlots[i].lotPoints);

                Vector2 centerOfPlotVector2 = new Vector2(centroidOfPlot.x, centroidOfPlot.z);
                CityDistrict plotDistrict = m_CityGenInstance.m_cityDistricts[0];
                if (CityGeneratorCore.IsPointInPolygon(exteriorPlots[i].lotPoints, centroidOfPlot))
                {
                    float districtCurveEvaluation = plotDistrict.buildingHeightCurve.Evaluate(1);
                    float buildingRandomness = (plotDistrict.buildingRandomness * randomGen.Next()) -
                                               plotDistrict.buildingRandomness / 2;
                    int buildingLevels = Mathf.RoundToInt((plotDistrict.baseBuildingHeight + buildingRandomness) *
                                                          districtCurveEvaluation);

                    DistrictBuildingType districtBuildingInfo = plotDistrict.GetBuildingToSpawn();
                    if(districtBuildingInfo == null)
                    {
                        continue;
                    }
                    if(randomGen.Next() < districtBuildingInfo.failChance)
                    {
                        continue;
                    }

                    List<Vector2> polygon = new List<Vector2>();
                    for (int l = 0; l < exteriorPlots[i].lotPoints.Count; l++)
                    {
                        polygon.Add(new Vector2(exteriorPlots[i].lotPoints[l].x, exteriorPlots[i].lotPoints[l].z));
                    }

                    /*
                    m_buildingGenerator.ExteriorBuildingGenFromPolygonListAndTheme(polygon,
                        CityBuilderUtils.GetHeightAtPoint(centroidOfPlot), buildingLevels, ref buildingToSpawn,
                        gameObject, buildingDone, 0, false);
                    */

                    BuildingConfig buildingConfig = districtBuildingInfo.houseInfo;

                    //get levels
                    int levels = buildingLevels;

                    //find axis aligned polygon and centroid
                    Vector2 centriod;
                    float rotationAmount;
                    List<Vector2> axisAlignedPolygon = StructureCreatorUtils.AlignPolygonWithAxis(polygon, out rotationAmount, out centriod);

                    //get y offset?
                    float yOffset = CityBuilderUtils.GetHeightAtPointVec3(centroidOfPlot);
                    //set spawn pos
                    Vector3 spawnPos = new Vector3(centriod.x, yOffset, centriod.y);

                    //create building parent object
                    buildingParent = Instantiate(m_CityGenInstance.buildingParentPrefab.gameObject, spawnPos, Quaternion.identity);
                    buildingParent.name = "Building " + "Parent";


                    BuildingGeneratorCore.PrepBuildingInfo(buildingConfig, axisAlignedPolygon, yOffset, spawnPos, levels);

                    //set up for accurate polygon
                    List<Vector2> accuratePolygon = new List<Vector2>();


                    if(districtBuildingInfo.generateInteriors)
                    {
                        //find total area of the building
                        //calculate rooms to include and populate that information into the building info
                        BuildingGeneratorCore.PrepareForInteriorGen(buildingConfig, axisAlignedPolygon);
                        Action<List<Vector2>> levelCreationFinished = (List<Vector2> list) =>
                        {
                            accuratePolygon = list;
                                
                        };

                        Action<BuildingLevelInfo> resetLayout = (BuildingLevelInfo buildingLevelInfo) =>
                        {
                            //ResetLayout(ref buildingLevelInfo);
                        };


                        // Start the coroutine for creating levels and begin the building generation process
                        BuildingGeneratorCore.CreateLevels(buildingConfig, randomGen, buildingParent.GetComponent<GeneratedBuildingParent>(), resetLayout, null, null, true, rotationAmount + exteriorPlots[i].angleToRoad, levelCreationFinished);
                    }
                    else
                    {
                        BuildingGeneratorCore.PrepareForExteriorGen(buildingConfig, buildingParent.GetComponent<GeneratedBuildingParent>(), out accuratePolygon);
                    }
                    

                    //find parent transform
                    Transform parentTransform = exteriorPlotBuildingsTransform;
                    BuildingGeneratorCore.SpawnBuilding(buildingConfig, randomGen, objectInstantiationManager, buildingParent.GetComponent<GeneratedBuildingParent>(), parentTransform, districtBuildingInfo.decorateInteriors, districtBuildingInfo.generateInteriors, false, rotationAmount, yOffset, flattenTerrain, m_CityGenInstance.polygonMaskLayerSettings, accuratePolygon);
                }
            }


            if (m_CityGenInstance.cityRoadSpawnType == CityRoadSpawnType.Gena)
            {
#if GENA_PRO
                roadObject = CreateRoads().gameObject;
#endif
            }
#if EASYROADS3D_PRO
            else if(m_CityGenInstance.cityRoadSpawnType == CityRoadSpawnType.EasyRoads)
            {
                roadObject = m_CityGenInstance.CreateNetwork();

                AddPolyMasksToRoads(roadObject);
            }
#endif
            StopEditorUpdates();
            EditorUtility.ClearProgressBar();
            yield break;
        }

        private void Clear()
        {
            Transform transform = m_CityGenInstance.transform;
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                GameObject child = transform.GetChild(i).gameObject;
                if(child.GetComponentInChildren<CityDistrict>() != null)
                {
                    continue;
                }
                DestroyImmediate(child);
            }

#if GENA_PRO
            if (roadObject != null)
            {
                DestroyImmediate(roadObject.gameObject);
            }
#endif
            StopEditorUpdates();
            EditorUtility.ClearProgressBar();
        }

        private void Apply()
        {
            transform = m_CityGenInstance.transform;
            GameObject cityParent = new GameObject();
            cityParent.name = "City";
            cityParent.transform.position = transform.position;
            GeneratedBuildingParent[] buildings = transform.gameObject.GetComponentsInChildren<GeneratedBuildingParent>();

            for (int i = 0; i < buildings.Length; i++)
            {
                buildings[i].transform.parent = cityParent.transform;
                buildings[i].ConvertToStorm();
            }

#if GENA_PRO
            if (roadObject != null)
            {
                roadObject.transform.parent = cityParent.transform;
            }
#endif
            if (m_CityGenInstance.useStorm)
            {
                //GetComponent<ObjectInstantiationManager>().ConvertLocalSpawnedObjectsToStorm();
            }

            Clear();
        }

        private void PrepareForCityGen()
        {
            transform = m_CityGenInstance.transform;
            InitalizeDictionarys();
            ResetBuildingSpawnCounts();

            majorRoadPoints.Clear();
            minorRoadPoints.Clear();
            intersections.Clear();
            allRoadPoints.Clear();
            blockInfo.Clear();
            exteriorPlots.Clear();


#if UNITY_EDITOR
            GizmoUtility.SetGizmoEnabled(typeof(MeshCollider), false);
            GizmoUtility.SetGizmoEnabled(typeof(BoxCollider), false);
#endif

            //create random gen instance with seed, potentially generate new random seed
            if (m_CityGenInstance.autoGenSeed)
            {
                m_CityGenInstance.m_seed = Random.Range(0, 999999999);
            }
            randomGen = new XorshiftPlus(m_CityGenInstance.m_seed);
        }

        /// <summary>
        /// sets the dictionary compare behavior to my custom one
        /// </summary>
        private void InitalizeDictionarys()
        {
            majorRoadPoints = new Dictionary<Vector3, RoadPointInformation>(vector3Comparer);
            minorRoadPoints = new Dictionary<Vector3, RoadPointInformation>(vector3Comparer);
            allRoadPoints = new Dictionary<Vector3, RoadPointInformation>(vector3Comparer);
        }

        private void ResetBuildingSpawnCounts()
        {
            foreach (CityDistrict cityDistrict in m_CityGenInstance.m_cityDistricts)
            {
                if (cityDistrict.districtBuildingTypes == null)
                {
                    continue;
                }
                foreach (DistrictBuildingType buildingType in cityDistrict.districtBuildingTypes)
                {
                    buildingType.currentSpawns = 0;
                }
            }
        }


        private void UpdateDistrictPolygons()
        {
            foreach (CityDistrict cityDistrict in m_CityGenInstance.m_cityDistricts)
            {
                if(cityDistrict.useCityPolygon)
                {
                    cityDistrict.GetDistrictPolygon(m_CityGenInstance.gameObject.GetComponent<PolygonMask>(), m_CityGenInstance.transform);
                }
                else
                {
                    cityDistrict.GetDistrictPolygon(cityDistrict.GetComponent<PolygonMask>(), cityDistrict.transform);
                }
                
            }
        }
#if GENA_PRO
        public Spline CreateRoads()
        {
            if(allRoadPoints == null)
            {
                return null;
            }
            // Generate a GeNa Spline
            Spline spline = Spline.CreateSpline("GeNa Spline - City Roads");

            // Add Road Extension
            spline.AddExtension<GeNaRoadExtension>();
            spline.GetExtension<GeNaRoadExtension>().ConformToGround = true;

            // Traverse node list and create Spline
            Dictionary<Vector3, GeNaNode> nodes = new Dictionary<Vector3, GeNaNode>();


            foreach (Vector3 index in allRoadPoints.Keys)
            {
                RoadPointInformation roadPoint = allRoadPoints[index];
                if (!nodes.ContainsKey(index))
                {
                    GeNaNode node = spline.CreateNewNode(roadPoint.pointLocation /*+ new Vector3(0, CityBuilderUtils.GetHeightAtPointVec3(roadPoint.pointLocation), 0)*/);
                    nodes.Add(index, node);
                }
            }

            foreach (Vector3 index in allRoadPoints.Keys)
            {
                RoadPointInformation roadPoint = allRoadPoints[index];
                GeNaNode thisNode = nodes[index];
                foreach (RoadConnection roadConnection in roadPoint.roadConnections)
                {
                    if (roadConnection.exitingConnection)
                    {
                        GeNaNode nextNode = nodes[roadConnection.connectionLocation];
                        spline.AddCurve(thisNode, nextNode);
                    }
                }
            }

            spline.Subdivide();

            spline.AddExtension<GeNaCarveExtension>().Carve();
            m_CityGenInstance.StartCoroutine(AddPolyMasksToRoads(spline));
            return spline;
        }

        public IEnumerator AddPolyMasksToRoads(Spline spline)
        {
            yield return new WaitForSeconds(6f);
            if (spline == null)
            {
                yield break;
            }
            Transform roadParent = spline.transform.GetChild(0);
            List<StormPolygonLayer> layers = GetStormPolyLayers();

            PolygonMask mask = roadParent.gameObject.AddComponent<PolygonMask>();
            StormPolygonShapeType shapeType = StormPolygonShapeType.Auto;
            mask.polygonData.polygons = StormPolygonUtility.CalculatePolygons(mask.transform, shapeType);
            AttemptToAddLayersToPolygonObject(mask, layers);
            mask.polygonData.ApplyToStorm();
            StormWorld.Instance.ForceFullRefresh();

        }

        public void AddPolyMasksToRoads(GameObject roadNetworkParent)
        {
#if EASYROADS3D_PRO
            ERCrossingPrefabs[] connections = roadNetworkParent.GetComponentsInChildren<ERCrossingPrefabs>();
            ERModularRoad[] roads = roadNetworkParent.GetComponentsInChildren<ERModularRoad>();
#endif
            List<StormPolygonLayer> layers = GetStormPolyLayers();
            AddPolyMaskToRoad(roadNetworkParent.transform.GetChild(0), layers);
            AddPolyMaskToRoad(roadNetworkParent.transform.GetChild(1), layers);
        }

        public void AddPolyMaskToRoad(Transform roadParent, List<StormPolygonLayer> layers)
        {
            StormPolygonMask mask = roadParent.gameObject.AddComponent<StormPolygonMask>();
            StormPolygonShapeType shapeType = StormPolygonShapeType.Auto;
            mask.polygonData.polygons = StormPolygonUtility.CalculatePolygons(mask.transform, shapeType);
            AttemptToAddLayersToPolygonObject(mask, layers);
            mask.polygonData.ApplyToStorm();
        }
#endif
        public List<StormPolygonLayer> GetStormPolyLayers()
        {
            List<StormPolygonLayer> layers = new List<StormPolygonLayer>();

            for (int i = 0; i < m_CityGenInstance.polygonMaskLayerSettings.Count; i++)
            {
                StormPolygonLayer stormPolygonLayer = new StormPolygonLayer();
                stormPolygonLayer.name = m_CityGenInstance.polygonMaskLayerSettings[i].name;
                stormPolygonLayer.joinType = Clipper2Lib.JoinType.Round;
                stormPolygonLayer.color = Color.red;
                stormPolygonLayer.offset = m_CityGenInstance.polygonMaskLayerSettings[i].offset;
                layers.Add(stormPolygonLayer);
            }
            return layers;
        }

        public void AttemptToAddLayersToPolygonObject(StormPolygonMask mask, List<StormPolygonLayer> layersToAdd)
        {
            List<StormPolygonLayer> stormPolygonLayers = mask.polygonData.layers;

            for (int k = 0; k < layersToAdd.Count; k++)
            {
                StormPolygonLayer layerToAdd = layersToAdd[k];
                for (int i = 0; i < stormPolygonLayers.Count; i++)
                {
                    if (stormPolygonLayers[i].name == layerToAdd.name)
                    {
                        break;
                    }
                }
                stormPolygonLayers.Add(layerToAdd);
            }


        }
    }




}

