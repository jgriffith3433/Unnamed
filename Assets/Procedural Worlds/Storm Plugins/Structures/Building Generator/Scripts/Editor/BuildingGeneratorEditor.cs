using ProceduralWorlds.Storm.Core;
using ProceduralWorlds.Storm.StormEditor;
using ProceduralWorlds.Storm.StructureGenCore;
using PWCommon5;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using XorshiftPlus = ProceduralWorlds.Storm.StructureGenCore.XorshiftPlus;
using Transform = UnityEngine.Transform;
using ProceduralWorlds.Storm.UI;
using System.Linq;
using Random = UnityEngine.Random;
namespace ProceduralWorlds.Storm.BuildingGen
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(BuildingGenerator))]
    public class BuildingGeneratorEditor : PWEditor
    {
        public bool m_defaultOpenPanels = false;

        public bool generationSettingsExpanded { get { return m_BuildingGenInstance.generationSettingsExpanded; } set { m_BuildingGenInstance.generationSettingsExpanded = value; } }
        public bool polygonPanelExpanded { get { return m_BuildingGenInstance.polygonPanelExpanded; } set { m_BuildingGenInstance.polygonPanelExpanded = value; } }
        public bool visualizationSettingsExpanded { get { return m_BuildingGenInstance.visualizationSettingsExpanded; } set { m_BuildingGenInstance.visualizationSettingsExpanded = value; } }
        public bool buildingConfigExpanded { get { return m_BuildingGenInstance.buildingConfigExpanded; } set { m_BuildingGenInstance.buildingConfigExpanded = value; } }
        public bool stormSettingsExpanded { get { return m_BuildingGenInstance.stormSettingsExpanded; } set { m_BuildingGenInstance.stormSettingsExpanded = value; } }

        EditorUtils m_editorUtils;


        SerializedProperty p_BuildingConfig;
        SerializedProperty p_BuildingParentPrefab;

        SerializedProperty p_Seed;

        SerializedProperty p_DecorateInteriors;
        SerializedProperty p_FlattenTerrain;

        SerializedProperty p_UseEntranceDirection;
        SerializedProperty p_EntranceDirectionAngle;


        SerializedProperty p_UseStorm;
        SerializedProperty p_ConvertObjectsToStormOnApply;
        SerializedProperty p_PolygonLayers;



        SerializedProperty p_ShowVisualization;
        SerializedProperty p_VisualizationType;
        // Property accessor for fitnessDebugIndex that points directly to the BuildingGenerator instance
        private int fitnessDebugIndex { get { return m_BuildingGenInstance.fitnessDebugIndex; } set { m_BuildingGenInstance.fitnessDebugIndex = value; } }

        SerializedProperty p_CurrentDebugRoom;
        SerializedProperty p_RoomLabelColor;
        SerializedProperty p_DebugLevel;


        BuildingGenerator m_BuildingGenInstance = null;

        /// <summary>
        /// The co-routine we are currently working on
        /// </summary>
        private IEnumerator m_currentCoroutine;
        private DateTime m_timeSinceLastUpdate;
        //private int m_currentWaitTimeMS = 0;


        private GeneratedBuildingParent buildingParent { get { return m_BuildingGenInstance.buildingParent; } }

        private ObjectInstantiationManager objectInstantiationManager;

        //private BuildingInfoEditor buildingInfoEditor = null;

        //private BuildingConfig currentShownBuildingInfo = null;
        private StormPolygonMask polygonMask = null;

        private StormPolygonMaskEditor polygonMaskEditor = null;

        private bool polyChanged = false;

        private DateTime lastChangeTime;
        private bool gizmosUpToDate = false;

        private StormScriptableUI<BuildingConfig> buildingInfoUI = null;
        int seed = 0;

        GUIContent RandomizeButtonContent;
        private void StartEditorUpdates()
        {
            //remember what we are working on
            m_currentCoroutine = Spawn();
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

                if (EditorUtility.DisplayCancelableProgressBar("Structure Generator", "Creating Building...", 0f))
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
                if (timeSinceLastUpdateinMS > 100 && gizmosUpToDate == false)
                {
                    polyChanged = false;
                    UpdateGizmos();
                    gizmosUpToDate = true;
                }

            }
        }

        private void OnEnable()
        {
            RandomizeButtonContent = EditorGUIUtility.IconContent("d_RotateTool On@2x");
            p_BuildingConfig = serializedObject.FindProperty("buildingConfig");
            p_BuildingParentPrefab = serializedObject.FindProperty("buildingParentPrefab");
            p_Seed = serializedObject.FindProperty("m_seed");
            
            p_DecorateInteriors = serializedObject.FindProperty("decorateInteriors");
            p_FlattenTerrain = serializedObject.FindProperty("flattenTerrain");

            p_UseEntranceDirection = serializedObject.FindProperty("useEntranceDirection");
            p_EntranceDirectionAngle = serializedObject.FindProperty("entranceDirectionAngle");

            p_ConvertObjectsToStormOnApply = serializedObject.FindProperty("convertObjectsToStormOnApply");
            p_PolygonLayers = serializedObject.FindProperty("polygonLayers");


            p_ShowVisualization = serializedObject.FindProperty("showVisualization");
            p_VisualizationType = serializedObject.FindProperty("visualizationType");
            // We're using a property accessor for fitnessDebugIndex instead of a serialized property
            p_CurrentDebugRoom = serializedObject.FindProperty("currentDebugRoom");
            p_RoomLabelColor = serializedObject.FindProperty("roomLabelColor");
            p_DebugLevel = serializedObject.FindProperty("debugLevel");

            Undo.undoRedoPerformed -= OnUndo;
            Undo.undoRedoPerformed += OnUndo;

            EditorApplication.update -= EditorUpdate;
            EditorApplication.update += EditorUpdate;

            m_BuildingGenInstance = (BuildingGenerator)target;
            polygonMask = m_BuildingGenInstance.GetComponent<PolygonMask>();
            if(polygonMask == null)
            {
                polygonMask = m_BuildingGenInstance.gameObject.AddComponent<PolygonMask>();
            }
            if(polygonMask != null)
            {
                polygonMaskEditor = (StormPolygonMaskEditor)CreateEditor(polygonMask, typeof(StormPolygonMaskEditor));
                polygonMask.polygonData.polygonChanged += PolygonData_polygonChanged;
                polygonMask.hideFlags = HideFlags.HideInInspector;
            }

            UpdateGizmos();
        }

        private void PolygonData_polygonChanged(bool obj)
        {
            polyChanged = true;
            StormPolygon polygon = polygonMask.polygonData.GetPolygon(0);
            if(polygon != null && polygon.points.Count == 1)
            {
                StormPolygonPoint polygonPoint = polygon.GetPoint(0);
                if(polygonPoint != null)
                {
                    Vector3 point = new Vector3((float)polygonPoint.x, 0, (float)polygonPoint.z);
                    Vector3 worldPoint = m_BuildingGenInstance.transform.TransformPoint(point);

                    float height = StructureCreatorUtils.GetHeightAtPointVec2(new Vector2(worldPoint.x, worldPoint.z));
                    m_BuildingGenInstance.transform.position = new Vector3(m_BuildingGenInstance.transform.position.x, height, m_BuildingGenInstance.transform.position.z);
                }
            }
        }

        private void OnDisable()
        {
            Undo.undoRedoPerformed -= OnUndo;
            if (polygonMask != null)
            {
                polygonMask.polygonData.polygonChanged -= PolygonData_polygonChanged;
            }
            EditorApplication.update -= EditorUpdate;
            StopEditorUpdates();
        }

        private void OnDestroy()
        {
            OnDisable();
        }

        public void OnSceneGUI()
        {

            if(m_BuildingGenInstance.transform.hasChanged == true)
            {
                UpdateGizmos();
            }

            if(polygonMaskEditor != null)
            {
                polygonMaskEditor.polygonEditor.SceneGUI(false);
            }

            if (!m_BuildingGenInstance.showVisualization)
            {
                return;
            }



            BuildingConfig buildingConfig = m_BuildingGenInstance.buildingConfig;
            if (buildingConfig == null)
            {
                return;
            }

            if(buildingConfig.levelInfomation == null)
            {
                return;
            }
            int debugLevel = m_BuildingGenInstance.debugLevel;

            BuildingLevelInfo structureLevelInfo = null;
            if (debugLevel >= 0 && debugLevel < buildingConfig.levelInfomation.Count)
            {
                structureLevelInfo = m_BuildingGenInstance.buildingConfig.levelInfomation[debugLevel];
            }
           
            if (structureLevelInfo == null) return;

            CellInfo[,] cellTable = structureLevelInfo.gridCellTable;


            float yOffset = buildingConfig.yOffset + debugLevel * buildingConfig.YGridSize;
            Quaternion rotation = Quaternion.Euler(0, m_BuildingGenInstance.rotationAmount, 0);



            Handles.color = m_BuildingGenInstance.roomLabelColor;
            GUI.color = m_BuildingGenInstance.roomLabelColor;


            for (int i = 0; i < structureLevelInfo.rooms.Count; i++)
            {
                RoomData roomData = structureLevelInfo.rooms[i];
                
                RoomStoreyData roomStoreyData = BuildingGeneratorCore.GetRoomStoreyDataAtCurrentLevel(roomData, debugLevel);
                Vector3 averagePos = GetAveragePosOfRoomStorey(roomStoreyData, cellTable);
                averagePos.y = yOffset + (buildingConfig.YGridSize/2);
                // Calculate rotated position around the pivot point
                Vector3 direction = averagePos - m_BuildingGenInstance.currentCentrioid;
                Vector3 rotatedPosition = m_BuildingGenInstance.currentCentrioid + rotation * direction;

                // Set the transformation matrix for the gizmo
                Handles.matrix = Matrix4x4.TRS(rotatedPosition, rotation, Vector3.one);
                Handles.Label(Vector3.zero, roomData.info.name);
            }

            Transform transform = m_BuildingGenInstance.transform;

            // display an orange disc where the object is

            //Handles.DrawWireDisc(pos, tr.up, 1.0f);
            // display object "value" in scene


        }

        public override void OnInspectorGUI()
        {
            if (m_editorUtils == null)
            {
                m_editorUtils = StormApp.GetEditorUtils(this);
            }
            m_BuildingGenInstance = (BuildingGenerator)target;




            m_editorUtils.Initialize();


            // Check if gizmos are disabled in the editor and show a warning
            if (!SceneView.lastActiveSceneView.drawGizmos)
            {
                EditorGUILayout.HelpBox("Gizmos are disabled in the editor. Please enable gizmos to see visualizations in the scene.", MessageType.Warning);
            }


            if(m_defaultOpenPanels)
            {
                polygonPanelExpanded = m_editorUtils.Panel("PolygonPanel", PolygonPanel, m_defaultOpenPanels);
                generationSettingsExpanded = m_editorUtils.Panel("GenerationPanel", GenerationPanel, m_defaultOpenPanels);
                stormSettingsExpanded = m_editorUtils.Panel("StormSettingsPanel", StormSettingsPanel, m_defaultOpenPanels);
                visualizationSettingsExpanded = m_editorUtils.Panel("VisualizationPanel", VisualizationPanel, m_defaultOpenPanels);
            }
            else
            {
                polygonPanelExpanded = m_editorUtils.Panel("PolygonPanel", PolygonPanel, polygonPanelExpanded);
                generationSettingsExpanded = m_editorUtils.Panel("GenerationPanel", GenerationPanel, generationSettingsExpanded);
                stormSettingsExpanded = m_editorUtils.Panel("StormSettingsPanel", StormSettingsPanel, stormSettingsExpanded);
                visualizationSettingsExpanded = m_editorUtils.Panel("VisualizationPanel", VisualizationPanel, visualizationSettingsExpanded);
            }


            bool disableIterateSpawn = false;
            List<Vector2> globalPolygon = StructureCreatorUtils.GetPolygonPointsGlobal(polygonMask, m_BuildingGenInstance.transform);
            if (globalPolygon.Count < 3)
            {
                disableIterateSpawn = true;
                EditorGUILayout.HelpBox("Building Shape has less than three points, please add at least 3 points to Iterate or Spawn.", MessageType.Error);
            }

            if(m_BuildingGenInstance.buildingConfig == null)
            {
                disableIterateSpawn = true;
                EditorGUILayout.HelpBox("Building Info is null, please assign a Building Info in the Generation Panel.", MessageType.Error);
            }
            else
            {
                if(m_BuildingGenInstance.buildingConfig.buildingBlocks == null)
                {
                    disableIterateSpawn = true;
                    EditorGUILayout.HelpBox("Building Blocks is null, please assign a Building Blocks in the Generation Panel.", MessageType.Error);
                }
                else
                {
                    BuildingBlocks buildingBlocks = m_BuildingGenInstance.buildingConfig.buildingBlocks;
                    if (buildingBlocks.wallBlocksList.Count == 0) 
                    { 
                        disableIterateSpawn = true;
                        EditorGUILayout.HelpBox("Building Blocks asset has no wall blocks, please add some to the asset.", MessageType.Error);
                    }
                    else
                    {
                        foreach(WallBlocksData wallBlocksData in buildingBlocks.wallBlocksList)
                        {
                            if(wallBlocksData.wallBlocks == null)
                            {
                                disableIterateSpawn = true;
                                EditorGUILayout.HelpBox("Building Blocks asset has a null wall blocks, please assign a Wall Blocks to the asset.", MessageType.Error);
                                continue;
                            }
                            wallBlocksData.wallBlocks.parentBuildingConfig = m_BuildingGenInstance.buildingConfig;
                            string wallBlocksMessage = wallBlocksData.wallBlocks.CheckForMissingBlocks();
                            if(wallBlocksMessage != "")
                            {
                                disableIterateSpawn = true;
                                EditorGUILayout.HelpBox(wallBlocksMessage, MessageType.Error);
                            }
                        }
                    }
                    if(buildingBlocks.roofBlocksList.Count == 0)
                    {
                        disableIterateSpawn = true;
                        EditorGUILayout.HelpBox("Building Blocks asset has no roof blocks, please add some to the asset.", MessageType.Error);
                    }
                    else
                    {
                        foreach(RoofBlocksData roofBlocksData in buildingBlocks.roofBlocksList)
                        {
                            if(roofBlocksData.roofBlocks == null)
                            {
                                disableIterateSpawn = true;
                                EditorGUILayout.HelpBox("Building Blocks asset has a null roof blocks, please assign a Roof Blocks to the asset.", MessageType.Error);
                                continue;
                            }
                            roofBlocksData.roofBlocks.parentBuildingConfig = m_BuildingGenInstance.buildingConfig;
                            string roofBlocksMessage = roofBlocksData.roofBlocks.CheckForMissingBlocks();
                            if(roofBlocksMessage != "")
                            {
                                disableIterateSpawn = true;
                                EditorGUILayout.HelpBox(roofBlocksMessage, MessageType.Error);
                            }
                        }
                    }
                }

            }

            bool originalGUIState = GUI.enabled;
            if(disableIterateSpawn)
            {
                GUI.enabled = false;
            }

            if (m_editorUtils.Button("Iterate"))
            {
                Iterate();
            }
            if (m_editorUtils.Button("Spawn"))
            {
                StartEditorUpdates();
            }
            GUI.enabled = originalGUIState;
            if (m_editorUtils.Button("Clear"))
            {
                Clear();
            }
            if (m_editorUtils.Button("Apply"))
            {
                Apply();
            }
            serializedObject.ApplyModifiedProperties();
            if (polyChanged)
            {
                lastChangeTime = DateTime.Now;
                gizmosUpToDate = false;
            }

        }

        public void GenerationPanel(bool helpActive)
        {
            EditorGUI.BeginChangeCheck();
            if(m_BuildingGenInstance.buildingConfig == null)
            {
                EditorGUILayout.HelpBox("No Building Config Assigned.", MessageType.Warning);
            }
            Action<BuildingConfig> onChanged = (BuildingConfig buildingConfig) =>
            {
                
            };
            if (buildingInfoUI == null)
                buildingInfoUI = new StormScriptableUI<BuildingConfig>(p_BuildingConfig, "BuildingConfigPanel", Repaint, m_editorUtils, onChanged, false, null, buildingConfigExpanded);
            
            
            // Store the current expansion state before DoLayout
            bool wasExpanded = buildingConfigExpanded;
            buildingInfoUI.DoLayout();
            
            m_editorUtils.InlineHelp("BuildingConfigPanel", helpActive);
            
            // Check if the expansion state changed and sync it back
            bool isExpanded = buildingInfoUI.IsExpanded;
            if (isExpanded != wasExpanded)
            {
                buildingConfigExpanded = isExpanded;
            }
            PropField("BuildingParentPrefab", p_BuildingParentPrefab, helpActive);
            
            if (EditorGUI.EndChangeCheck())
            {
                lastChangeTime = DateTime.Now;
                gizmosUpToDate = false;
            }

            EditorGUILayout.Space();

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.BeginHorizontal();
            PropField("Seed", p_Seed, helpActive);
            if (GUILayout.Button(RandomizeButtonContent, GUILayout.ExpandWidth(false), GUILayout.Height(EditorGUIUtility.singleLineHeight)))
            {
                p_Seed.intValue = Random.Range(int.MinValue, int.MaxValue);
            }
            EditorGUILayout.EndHorizontal();

            
            // Style Tag IntPopup to prevent invalid entries
            if (m_BuildingGenInstance.buildingConfig != null)
            {
                var styleTags = StyleTagDirectory.StyleTags;
                string[] styleTagNames = new string[styleTags.Count + 1];
                int[] styleTagIndices = new int[styleTags.Count + 1];
                
                // Add "None" option at index 0
                styleTagNames[0] = "None";
                styleTagIndices[0] = 0; // Use 0 for "None"
                
                // Add all available style tags
                for (int i = 0; i < styleTags.Count; i++)
                {
                    styleTagNames[i + 1] = styleTags[i].name;
                    styleTagIndices[i + 1] = i + 1; // Start from 1 for actual style tags
                }
                
                // Find current selection index
                int currentIndex = 0; // Default to "None" (value 0)
                string currentStyleTag = m_BuildingGenInstance.buildingConfig.styleTag;
                if (!string.IsNullOrEmpty(currentStyleTag))
                {
                    for (int i = 0; i < styleTags.Count; i++)
                    {
                        if (styleTags[i].name == currentStyleTag)
                        {
                            currentIndex = i + 1; // +1 because "None" is value 0
                            break;
                        }
                    }
                }
                
                // Display the IntPopup
                EditorGUI.BeginChangeCheck();
                int styleTagSelectedIndex = EditorGUILayout.IntPopup("Style Tag", currentIndex, styleTagNames, styleTagIndices);
                if (EditorGUI.EndChangeCheck())
                {
                    if (styleTagSelectedIndex == 0) // "None" selected
                    {
                        m_BuildingGenInstance.buildingConfig.styleTag = ""; // Set to empty for "None"
                    }
                    else if (styleTagSelectedIndex > 0 && styleTagSelectedIndex - 1 < styleTags.Count)
                    {
                        m_BuildingGenInstance.buildingConfig.styleTag = styleTags[styleTagSelectedIndex - 1].name;
                    }
                    
                    // Mark the BuildingConfig as dirty
                    EditorUtility.SetDirty(m_BuildingGenInstance.buildingConfig);
                }
            }
            else
            {
                EditorGUILayout.HelpBox("No BuildingConfig assigned. Please assign a BuildingConfig to configure the Style Tag.", MessageType.Warning);
            }

            if(m_BuildingGenInstance.buildingConfig != null && m_BuildingGenInstance.buildingConfig.generateInteriorLayout)
            {
                PropField("DecorateInteriors", p_DecorateInteriors, helpActive);
            }
            PropField("FlattenTerrain", p_FlattenTerrain, helpActive);

            PropField("UseEntranceDirection", p_UseEntranceDirection, helpActive);
            if(p_UseEntranceDirection.boolValue)
            {
                PropField("EntranceDirectionAngle", p_EntranceDirectionAngle, helpActive);
            }

            if (m_editorUtils.Button("SnapToTerrain"))
            {
                m_BuildingGenInstance.SnapToTerrain();
            }

            if (EditorGUI.EndChangeCheck())
            {
                lastChangeTime = DateTime.Now;
                gizmosUpToDate = false;
            }
        }

        

        public void PolygonPanel(bool helpActive)
        {
            polygonMaskEditor.QuickStartPanel(helpActive);
            polygonMaskEditor.DrawPolygonDataObjectNoLayers(helpActive, true);
        }

        public void StormSettingsPanel(bool helpActive)
        {
            EditorGUI.BeginChangeCheck();
            //PropField("UseStorm", p_UseStorm, helpActive);
            PropField("ConvertObjectsToStormOnApply", p_ConvertObjectsToStormOnApply, helpActive);
            ++EditorGUI.indentLevel;
            PropField("PolygonLayers", p_PolygonLayers, helpActive);
            --EditorGUI.indentLevel;
            
            if (EditorGUI.EndChangeCheck())
            {
                lastChangeTime = DateTime.Now;
                gizmosUpToDate = false;
            }
        }
        
        public void VisualizationPanel(bool helpActive)
        {
            EditorGUI.BeginChangeCheck();
            PropField("ShowVisualization", p_ShowVisualization, helpActive);
            if (p_ShowVisualization.boolValue)
            {
                PropField("VisualizationType", p_VisualizationType, helpActive);
                if (m_BuildingGenInstance.buildingConfig != null && m_BuildingGenInstance.buildingConfig.levelInfomation.Count > 0)
                {
                    GUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Debug Level", GUILayout.Width(EditorGUIUtility.labelWidth - 4));
                    if (GUILayout.Button("-", GUILayout.Width(30)))
                    {
                        if (m_BuildingGenInstance.debugLevel > 0)
                        {
                            m_BuildingGenInstance.debugLevel--;
                        }
                    }
                    EditorGUILayout.LabelField(m_BuildingGenInstance.debugLevel.ToString(), GUILayout.Width(30));
                    if (GUILayout.Button("+", GUILayout.Width(30)))
                    {
                        if (m_BuildingGenInstance.debugLevel < m_BuildingGenInstance.buildingConfig.levelInfomation.Count - 1)
                        {
                            m_BuildingGenInstance.debugLevel++;
                        }
                    }
                    GUILayout.EndHorizontal();
                    bool originalGUIState = GUI.enabled;
                    GUI.enabled = false;
                    PropField("CurrentDebugRoom", p_CurrentDebugRoom, helpActive);
                    GUI.enabled = originalGUIState;

                    PropField("RoomLabelColor", p_RoomLabelColor, helpActive);
                }
                else if (p_VisualizationType.enumValueIndex == 0)
                {
                    PropField("RoomLabelColor", p_RoomLabelColor, helpActive);
                }
                int maxSliderRange = 0;
                if(m_BuildingGenInstance.buildingConfig != null)
                {
                    maxSliderRange = m_BuildingGenInstance.buildingConfig.levelInfomation.Count-1;
                }
                p_DebugLevel.intValue = (int)m_editorUtils.Slider("BuildingLevel", (float)m_BuildingGenInstance.debugLevel, 0, maxSliderRange, helpActive);
            }
        }

        private void PropField(string key, SerializedProperty prop, bool helpActive, bool expandWidth = true)
        {
            EditorGUI.showMixedValue = prop.hasMultipleDifferentValues;
            m_editorUtils.PropertyField(key, true, prop, helpActive, GUILayout.ExpandWidth(expandWidth));
            EditorGUI.showMixedValue = false;
        }

        private void OnUndo()
        {
            serializedObject.Update();
            serializedObject.ApplyModifiedProperties();
        }



        private void UpdateGizmos()
        {
            //cashe buildingConfig
            BuildingConfig buildingConfig = m_BuildingGenInstance.buildingConfig;
            if (buildingConfig == null)
            {
                return;
            }

            List<Vector2> globalPolygon = StructureCreatorUtils.GetPolygonPointsGlobal(polygonMask, m_BuildingGenInstance.transform);
            if (globalPolygon.Count < 3)
            {
                Debug.LogWarning("Polygon has less than 3 points, please ensure that there are at least 3 points in your polygon");
                return;
            }

            seed = p_Seed.intValue;
            XorshiftPlus randomGen = new XorshiftPlus(seed);
            //p_Seed.intValue = seed;

            //find axis aligned polygon and centroid
            Vector2 centriod;
            float rotationAmount;
            List<Vector2> axisAlignedPolygon = StructureCreatorUtils.AlignPolygonWithAxis(globalPolygon, out rotationAmount, out centriod);

            //get y offset?
            float yOffset = m_BuildingGenInstance.transform.position.y;
            //set spawn pos
            Vector3 spawnPos = new Vector3(centriod.x, yOffset, centriod.y);

            m_BuildingGenInstance.currentCentrioid = spawnPos;
            m_BuildingGenInstance.rotationAmount = rotationAmount;

            BuildingGeneratorCore.PrepBuildingInfo(buildingConfig, axisAlignedPolygon, yOffset, spawnPos, buildingConfig.floors);

            //find our object instantiation ref again(THIS IS IF ITS CALLED FROM OTHER SCRIPTS ONLY?)
            if (m_BuildingGenInstance.buildingConfig.generateInteriorLayout)
            {
                //find total area of the building
                //calculate rooms to include and populate that information into the building info
                BuildingGeneratorCore.PrepareForInteriorGen(buildingConfig, axisAlignedPolygon);


                Action<List<Vector2>> levelCreationFinished = (List<Vector2> list) =>
                {
                    //m_BuildingGenInstance.StartCoroutine(BuildingGeneratorCore.InstatiateBuilding(buildingConfig, buildingParent.GetComponent<GeneratedBuildingParent>(), rotationAmount, parentTransform, randomGen, objectInstantiationManager, m_BuildingGenInstance.decorateInteriors, m_BuildingGenInstance.useStorm, null));
                };

                Action<BuildingLevelInfo> resetLayout = (BuildingLevelInfo buildingLevelInfo) =>
                {
                    //ResetLayout(ref buildingLevelInfo);
                };
                // Start the coroutine for creating levels and begin the building generation process
                BuildingGeneratorCore.CreateLevels(buildingConfig, randomGen, buildingParent, resetLayout, null, null, p_UseEntranceDirection.boolValue, rotationAmount + p_EntranceDirectionAngle.floatValue, levelCreationFinished);
            }
            else
            {
                // Generate and set up level information for each level
                for (int i = 0; i < buildingConfig.polygonLists.Count; i++)
                {
                    BuildingLevelInfo structureLevelInfo = new BuildingLevelInfo();
                    structureLevelInfo.XZGridSize = buildingConfig.XZGridSize;

                    structureLevelInfo.polygonVertecies = buildingConfig.polygonLists[0];
                    structureLevelInfo.levelNumber = i;
                    buildingConfig.levelInfomation.Add(structureLevelInfo);
                    BuildingGeneratorCore.CreateGridInPolygon(ref structureLevelInfo, buildingConfig, buildingParent);

                    //assign the structure level the revised "grid" polygon
                    List<Vector2> pointCloud = BuildingGeneratorCore.GetPointCloudFromGrid(structureLevelInfo.XZGridSize, structureLevelInfo.gridCellTable);
                    structureLevelInfo.polygonVertecies = BuildingGeneratorCore.GeneratePolygonFromPointCloud(BuildingGeneratorCore.GetLowestPointInList(pointCloud), pointCloud, structureLevelInfo.XZGridSize);
                }
            }
            SceneView.RepaintAll();
        }

        private void Iterate()
        {
            UpdateGizmos();
        }

        private IEnumerator Spawn()
        {
            //Clear
            Clear();

            //cashe buildingConfig
            BuildingConfig buildingConfig = m_BuildingGenInstance.buildingConfig;
            if (buildingConfig == null)
            {
                Debug.LogWarning("Building Info Missing");
                StopEditorUpdates();
                EditorUtility.ClearProgressBar();
                yield break;
            }

            bool useStorm = m_BuildingGenInstance.useStorm;

            //find object instantiation manager
            objectInstantiationManager = FindAnyObjectByType<ObjectInstantiationManager>();
            if (objectInstantiationManager == null)
            {
                Debug.LogError("No Object Instantiation Manager In Scene, Please Add One");
                StopEditorUpdates();
                EditorUtility.ClearProgressBar();
                yield break;
            }


            //create random gen instance with seed, potentially generate new random seed

            XorshiftPlus randomGen = new XorshiftPlus(seed);

            //get the vertices from the polygon object

            List<Vector2> globalPolygon = StructureCreatorUtils.GetPolygonPointsGlobal(polygonMask, m_BuildingGenInstance.transform);
            if (globalPolygon.Count < 3)
            {
                Debug.LogWarning("Polygon has less than 3 points, please ensure that there are at least 3 points in your polygon");
                StopEditorUpdates();
                EditorUtility.ClearProgressBar();
                yield break;
            }


            //branch off into main thread of generation, depending on if we want interiors or not


            //assign polygon to storm mask(THIS IS IF ITS CALLED FROM OTHER SCRIPTS ONLY?)
            bool decorateInteriors = m_BuildingGenInstance.decorateInteriors;


            //get levels
            int levels = buildingConfig.floors;

            //find axis aligned polygon and centroid
            Vector2 centriod;
            float rotationAmount;
            List<Vector2> axisAlignedPolygon = StructureCreatorUtils.AlignPolygonWithAxis(globalPolygon, out rotationAmount, out centriod);

            //get y offset?
            float yOffset = m_BuildingGenInstance.transform.position.y;
            //set spawn pos
            Vector3 spawnPos = new Vector3(centriod.x, yOffset, centriod.y);
            List<Vector2> accuratePolygon = new List<Vector2>();
            m_BuildingGenInstance.currentCentrioid = spawnPos;
            m_BuildingGenInstance.rotationAmount = rotationAmount;
            //create building parent object
            m_BuildingGenInstance.buildingParent = Instantiate(m_BuildingGenInstance.buildingParentPrefab, spawnPos, Quaternion.identity);

            buildingParent.gameObject.name = "Building " + "Parent";

            BuildingGeneratorCore.PrepBuildingInfo(buildingConfig, axisAlignedPolygon, yOffset, spawnPos, levels);

            //set up for accurate polygon


            //if interiors do this
            if (m_BuildingGenInstance.buildingConfig.generateInteriorLayout)
            {
                //find total area of the building
                //calculate rooms to include and populate that information into the building info
                BuildingGeneratorCore.PrepareForInteriorGen(buildingConfig, axisAlignedPolygon);

            }
            else
            {
                //generate and add grid data to each building level, also generate refined(accurate) polygons for each levels grid set up

                //set accurate polygon to be accurate on level 0 only?
                BuildingGeneratorCore.PrepareForExteriorGen(buildingConfig, buildingParent, out accuratePolygon);
            }

            List<BorderCell> additionalExitCells = new List<BorderCell>();
            float exitDirection = rotationAmount + m_BuildingGenInstance.entranceDirectionAngle;
            if (m_BuildingGenInstance.buildingConfig.generateInteriorLayout)
            {
                Action<List<Vector2>> levelCreationFinished = (List<Vector2> list) =>
                {
                    accuratePolygon = list;
                    //m_BuildingGenInstance.StartCoroutine(BuildingGeneratorCore.InstatiateBuilding(buildingConfig, buildingParent.GetComponent<GeneratedBuildingParent>(), rotationAmount, parentTransform, randomGen, objectInstantiationManager, m_BuildingGenInstance.decorateInteriors, m_BuildingGenInstance.useStorm, null));
                };

                Action<BuildingLevelInfo> resetLayout = (BuildingLevelInfo buildingLevelInfo) =>
                {
                    //ResetLayout(ref buildingLevelInfo);
                };

                if(!p_UseEntranceDirection.boolValue)
                {
                    exitDirection = randomGen.Next(0, 360);
                }

                // Start the coroutine for creating levels and begin the building generation process
                BuildingGeneratorCore.CreateLevels(buildingConfig, randomGen, buildingParent, resetLayout, null, null, true, exitDirection, levelCreationFinished);
            }
            else
            {
                if(!p_UseEntranceDirection.boolValue)
                {
                    exitDirection = randomGen.Next(0, 360);
                }
                BorderCell exitCell = BuildingGeneratorCore.GetEdgeCellInDirection(buildingConfig.levelInfomation[0].gridCellTable, StructureCreatorUtils.GetCentroid(accuratePolygon), exitDirection);
                additionalExitCells.Add(exitCell); 
            }

            //move onto beinging building generation or immediatly start instatniating the building extyerior and finish there
            BuildingGeneratorCore.SpawnBuilding(buildingConfig, randomGen, objectInstantiationManager, buildingParent, m_BuildingGenInstance.transform, decorateInteriors, m_BuildingGenInstance.buildingConfig.generateInteriorLayout, useStorm, rotationAmount, yOffset, m_BuildingGenInstance.flattenTerrain, m_BuildingGenInstance.polygonLayers, accuratePolygon, additionalExitCells);


            m_BuildingGenInstance.buildingConfig = buildingConfig;
            buildingParent.transform.position = m_BuildingGenInstance.currentCentrioid;

            StopEditorUpdates();
            EditorUtility.ClearProgressBar();
            yield break;
        }

       

        public void Clear()
        {
            if (buildingParent != null)
            {
                DestroyImmediate(buildingParent.gameObject);
            }
        }

        public void Apply()
        {
            if (buildingParent == null || objectInstantiationManager == null)
            {
                return;
            }
            if (m_BuildingGenInstance.convertObjectsToStormOnApply)
            {
                if (objectInstantiationManager.localUnConvertedStormObjects.Count > 0)
                {
                    buildingParent.unConvertedObjects = objectInstantiationManager.CreateDictionaryCopy(objectInstantiationManager.localUnConvertedStormObjects);
                }

                buildingParent.ConvertToStorm();
            }
            else
            {
                if (objectInstantiationManager.localUnConvertedStormObjects.Count > 0)
                {
                    buildingParent.unConvertedObjects = objectInstantiationManager.CreateDictionaryCopy(objectInstantiationManager.localUnConvertedStormObjects);
                }
            }

            //buildingParent.GetComponent<GeneratedBuildingParent>().AssignPolygonMaskData(accuratePolygon, polygonObject, transform, flattenTerrain);
            buildingParent.transform.parent = null;
            m_BuildingGenInstance.buildingParent = null;
        }



        public Vector3 GetAveragePosOfRoomStorey(RoomStoreyData roomStoreyData, CellInfo[,] gridCellTable)
        {
            Vector2 sum = Vector2.zero;

            // Iterate through each cell in the roomStoreyData
            for (int i = 0; i < roomStoreyData.cells.Count; i++)
            {
                Vector2Int currentCellPos = roomStoreyData.cells[i];
                CellInfo currentCell = gridCellTable[currentCellPos.x, currentCellPos.y];
                Vector2 cellCenter = currentCell.cellCenter;

                // Add the current cell center to the sum
                sum += cellCenter;
            }

            // Calculate the average position by dividing the sum by the number of cells
            Vector2 average = sum / roomStoreyData.cells.Count;

            // Return the result as a Vector3
            return new Vector3(average.x, 0, average.y);
        }

    }
}
