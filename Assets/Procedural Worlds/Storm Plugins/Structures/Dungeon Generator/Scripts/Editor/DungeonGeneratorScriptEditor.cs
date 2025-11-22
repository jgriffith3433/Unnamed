using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using ProceduralWorlds.Storm.StormEditor;
using PWCommon5;
using ProceduralWorlds.Storm.DungeonGen;
using System.Collections;
using System;
using ProceduralWorlds.Storm.StructureGenCore;
using ProceduralWorlds.Storm.BuildingGen;
#if PW_STORM_HDRP
using UnityEngine.Rendering;
#endif


[CustomEditor(typeof(DungeonGeneratorScript))]
public class DungeonGeneratorScriptEditor : PWEditor
{
    public bool m_defaultOpenPanels = true;

    EditorUtils m_editorUtils;

    //SerializedProperty p_Spawns;
    SerializedProperty p_Seed;
    SerializedProperty p_DungeonLayers;
    SerializedProperty p_SelectedDungeonLayerInt;
    SerializedProperty p_HallwayBuildingInfo;
    SerializedProperty p_GridSize;
    SerializedProperty p_VerticalGridSize;
    SerializedProperty p_StartPoint;

    SerializedProperty p_UseStorm;

    SerializedProperty p_ShowGizmos;
    SerializedProperty p_ViewingLevel;
    SerializedProperty p_LayerViewingLevel;
    SerializedProperty p_ResultList;
    SerializedProperty p_InitialStart;
    SerializedProperty p_Rect;
    SerializedProperty p_Radius;
    SerializedProperty p_Volume;

    SerializedProperty p_BuildingParentPrefab;

#if PW_STORM_HDRP
    SerializedProperty p_VolumeProfile;
#endif

    SerializedProperty p_TerrainHoleSize;
    SerializedProperty p_TerrainHolePosition;

    SerializedProperty p_EntranceRoomPrefab;
    SerializedProperty p_EntranceRoomPos;

    DungeonGeneratorScript m_DungeonGeneratorScriptInstance = null;

    Transform transform { get { return m_DungeonGeneratorScriptInstance.transform; } }

    XorshiftPlus randomGen;


    private ObjectInstantiationManager objectInstantiationManager;

    /// <summary>
    /// The co-routine we are currently working on
    /// </summary>
    private IEnumerator m_currentCoroutine;
    private DateTime m_timeSinceLastUpdate;
    //private int m_currentWaitTimeMS = 0;

    bool useStorm = false;

    bool decorateInteriors = true;

    private DungeonLayerEditor dungeonLayerEditor;
    private DungeonLayer selectedDungeonLayer;
    private void StartEditorUpdates(IEnumerator function)
    {
        //remember what we are working on
        m_currentCoroutine = function;
        //subscribe to the editor update event
        EditorApplication.update -= EditorUpdate;
        EditorApplication.update += EditorUpdate;
    }

    private void StopEditorUpdates()
    {
        //forget what we are working on
        m_currentCoroutine = null;
        //unsubscribe to the editor update event
        EditorApplication.update -= EditorUpdate;
    }

    private void EditorUpdate()
    {
        if (m_currentCoroutine != null)
        {

            if (EditorUtility.DisplayCancelableProgressBar("Dungeon Generator", "Performing Action...", 0f))
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
    }

    private void OnEnable()
    {
        //p_Spawns = serializedObject.FindProperty("spawns");
        p_Seed = serializedObject.FindProperty("seed");
        p_DungeonLayers = serializedObject.FindProperty("dungoenLayers");
        p_SelectedDungeonLayerInt = serializedObject.FindProperty("selectedDungeonLayerInt");
        p_HallwayBuildingInfo = serializedObject.FindProperty("hallwayBuildingInfo");
        p_GridSize = serializedObject.FindProperty("gridSize");
        p_VerticalGridSize = serializedObject.FindProperty("verticalGridSize");
        p_StartPoint = serializedObject.FindProperty("startPoint");

        p_UseStorm = serializedObject.FindProperty("useStorm");

        p_ShowGizmos = serializedObject.FindProperty("showGizmos");
        p_ViewingLevel = serializedObject.FindProperty("viewingLevel");
        p_LayerViewingLevel = serializedObject.FindProperty("layerViewingLevel");
        p_ResultList = serializedObject.FindProperty("resultList");
        p_InitialStart = serializedObject.FindProperty("initalstart");
        p_Rect = serializedObject.FindProperty("rect");
        p_Radius = serializedObject.FindProperty("raduis");
        p_Volume = serializedObject.FindProperty("volume");

        p_BuildingParentPrefab = serializedObject.FindProperty("buildingParentPrefab");

#if PW_STORM_HDRP
        p_VolumeProfile = serializedObject.FindProperty("volumeProfile");
#endif

        p_TerrainHoleSize = serializedObject.FindProperty("terrainHoleSize");
        p_TerrainHolePosition = serializedObject.FindProperty("terrainHolePosition");

        p_EntranceRoomPrefab = serializedObject.FindProperty("entranceRoomPrefab");
        p_EntranceRoomPos = serializedObject.FindProperty("entranceRoomPos");

        Undo.undoRedoPerformed -= OnUndo;
        Undo.undoRedoPerformed += OnUndo;
        m_DungeonGeneratorScriptInstance = (DungeonGeneratorScript)target;
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
        m_DungeonGeneratorScriptInstance = (DungeonGeneratorScript)target;

        m_editorUtils.Initialize();

        m_editorUtils.Panel("GeneralSettingsPanel", GeneralSettingsPanel, m_defaultOpenPanels);

        m_editorUtils.Panel("GizmosSettingsPanel", GizmosSettingsPanel, m_defaultOpenPanels);



        if (m_editorUtils.Button("Iterate"))
        {
            StartEditorUpdates(Iterate());
        }
        if (m_editorUtils.Button("Apply"))
        {
            StartEditorUpdates(Apply());
        }
        if (m_editorUtils.Button("Clear"))
        {
            Clear();
        }

        serializedObject.ApplyModifiedProperties();
    }

    public void GeneralSettingsPanel(bool helpActive)
    {
        //PropField("Spawns", p_Spawns, helpActive);
        PropField("Seed", p_Seed, helpActive);
        ++EditorGUI.indentLevel;
        PropField("DungeonLayers", p_DungeonLayers, helpActive);
        --EditorGUI.indentLevel;
        m_editorUtils.Panel("DungeonLayersPanel", DungeonLayersPanel, false);
        PropField("HallwayBuildingInfo", p_HallwayBuildingInfo, helpActive);
        PropField("GridSize", p_GridSize, helpActive);
        PropField("VerticalGridSize", p_VerticalGridSize, helpActive);
        PropField("StartPoint", p_StartPoint, helpActive);

        PropField("UseStorm", p_UseStorm, helpActive);

        PropField("BuildingParentPrefab", p_BuildingParentPrefab, helpActive);

#if PW_STORM_HDRP
        PropField("VolumeProfile", p_VolumeProfile, helpActive);
#endif
        PropField("TerrainHoleSize", p_TerrainHoleSize, helpActive);
        PropField("TerrainHolePosition", p_TerrainHolePosition, helpActive);

        PropField("EntranceRoomPrefab", p_EntranceRoomPrefab, helpActive);
        PropField("EntranceRoomPos", p_EntranceRoomPos, helpActive);
    }

    public void DungeonLayersPanel(bool helpActive)
    {
        List<DungeonLayer> dungeonLayers = m_DungeonGeneratorScriptInstance.dungoenLayers;
        if (dungeonLayers.Count == 0)
        {
            EditorGUILayout.HelpBox("No City Districts Assigned.", MessageType.Warning);
            return;
        }
        PropField("SelectedDungeonLayerInt", p_SelectedDungeonLayerInt, helpActive);
        if (p_SelectedDungeonLayerInt.intValue >= dungeonLayers.Count || p_SelectedDungeonLayerInt.intValue < 0)
        {
            EditorGUILayout.HelpBox("Selected District Outside of Range.", MessageType.Warning);
            return;
        }

        if (selectedDungeonLayer != dungeonLayers[p_SelectedDungeonLayerInt.intValue] || dungeonLayerEditor == null)
        {
            selectedDungeonLayer = dungeonLayers[p_SelectedDungeonLayerInt.intValue];

            // Create an editor for the sub-object and draw its inspector
            dungeonLayerEditor = CreateEditor(selectedDungeonLayer) as DungeonLayerEditor;
            dungeonLayerEditor.m_defaultOpenPanels = false;
        }

        dungeonLayerEditor.OnInspectorGUI();
    }

    public void GizmosSettingsPanel(bool helpActive)
    {
        PropField("ShowGizmos", p_ShowGizmos, helpActive);
        PropField("ViewingLevel", p_ViewingLevel, helpActive);
        PropField("LayerViewingLevel", p_LayerViewingLevel, helpActive);
        ++EditorGUI.indentLevel;
        PropField("ResultList", p_ResultList, helpActive);
        --EditorGUI.indentLevel;
        PropField("InitialStart", p_InitialStart, helpActive);
        PropField("Rect", p_Rect, helpActive);
        PropField("Radius", p_Radius, helpActive);
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

    public IEnumerator Iterate()
    {
        Clear();

        Vector3 startPoint = transform.position + m_DungeonGeneratorScriptInstance.startPoint;

        //find object instantiation manager
        objectInstantiationManager = FindAnyObjectByType<ObjectInstantiationManager>();
        if (objectInstantiationManager == null)
        {
            Debug.LogError("No Object Instantiation Manager In Scene, Please Add One");
            StopEditorUpdates();
            EditorUtility.ClearProgressBar();
            yield break;
        }

        int seed = UnityEngine.Random.Range(0, 999999999);
        randomGen = new XorshiftPlus(seed);

        int chamberCount = 0;

        List<DungeonLayer> dungeonLayers = m_DungeonGeneratorScriptInstance.dungoenLayers;
        float gridSize = m_DungeonGeneratorScriptInstance.gridSize;
        float verticalGridSize = m_DungeonGeneratorScriptInstance.verticalGridSize;
        float levels = 0;
        for (int i = 0; i < dungeonLayers.Count; i++)
        {
            dungeonLayers[i].gridSize = gridSize;
            levels += dungeonLayers[i].levels;
            dungeonLayers[i].CreateLevelsAndAssignGrid(gridSize, verticalGridSize, startPoint);
        }

        //Assigns and builds the interior lighting volume
        float dungeonXZ = dungeonLayers[0].levelSize * gridSize;
        float dungeonY = levels * verticalGridSize;



#if PW_STORM_HDRP
        GameObject volumeObject = new GameObject();
        volumeObject.transform.parent = transform;
        volumeObject.name = "Volume";
        volumeObject.transform.localPosition = Vector3.zero;
         
        Volume volume = volumeObject.AddComponent<Volume>();


        m_DungeonGeneratorScriptInstance.volume = volume.gameObject;
        volume.profile = m_DungeonGeneratorScriptInstance.volumeProfile;
        volume.weight = 1;
        volume.isGlobal = false;

        BoxCollider volumeBounds = volumeObject.AddComponent<BoxCollider>();



        Vector3 position = new Vector3(0, m_DungeonGeneratorScriptInstance.startPoint.y + (-dungeonY / 2), 0);
        Vector3 bounds = new Vector3(dungeonXZ + 5f, dungeonY + 5f, dungeonXZ + 5f);
        volumeBounds.size = bounds;
        volumeBounds.center = position;
#endif
        GameObject entranceRoom = Instantiate(m_DungeonGeneratorScriptInstance.entranceRoomPrefab, transform);
        entranceRoom.transform.localPosition = m_DungeonGeneratorScriptInstance.entranceRoomPos;

        //connect layers
        for (int i = 0; i < dungeonLayers.Count - 1; i++)
        {
            DungeonLevel higherLevel = dungeonLayers[i].layerLevels[dungeonLayers[i].layerLevels.Count - 1];
            DungeonLevel lowerLevel = dungeonLayers[i + 1].layerLevels[0];
            for (int k = 0; k < dungeonLayers[i].layerConnectionCount; k++)
            {
                DungeonGeneratorUtils.CreateNewChamberBetweenLayers(dungeonLayers[i], higherLevel, lowerLevel, chamberCount, randomGen);
                chamberCount++;
            }
        }

        DungeonLayer dungeon = dungeonLayers[0];
        dungeonLayers[0].CreateRooms(ref dungeon, ref chamberCount, randomGen, transform);
        dungeonLayers[0].SetLayerChamberExits(randomGen);
        dungeonLayers[0].CreateHalls(randomGen);


        //spawn dungeon
        List<DungeonChamber> spawnedChambers = new List<DungeonChamber>();
        DungeonLevel lastLevel = null;


        for (int i = 0; i < dungeonLayers.Count; i++)
        {
            GameObject layerParent = new GameObject();
            layerParent.name = "Layer " + i;
            layerParent.transform.parent = transform;
            layerParent.transform.localPosition = Vector3.zero;
            for (int k = 0; k < dungeonLayers[i].layerLevels.Count; k++)
            {
                GameObject levelParent = new GameObject();
                int level = (i + 1) * k;
                levelParent.name = "Level " + level;
                levelParent.transform.parent = layerParent.transform;
                levelParent.transform.localPosition = Vector3.zero;
                DungeonLevel currentLevel = dungeonLayers[i].layerLevels[k];
                for (int l = 0; l < currentLevel.chamberCenters.Count; l++)
                {
                    DungeonChamber chamber = currentLevel.chambersOnLevelDictionary[currentLevel.chamberCenters[l]].parentChamber;
                    if (spawnedChambers.Contains(chamber))
                    {
                        continue;
                    }
                    if (chamber.custom)
                    {
                        Instantiate(chamber.roomPrefab, chamber.spawnPoint, Quaternion.identity, levelParent.transform);
                        continue;
                    }

                    //calcuate rooms to include
                    List<List<BorderCell>> exitCellsList;
                    float yLevel;
                    List<CellInfo[,]> cellTableList = DungeonGeneratorUtils.ConvertDungeonChamber(chamber, dungeonLayers[i], dungeonLayers[i].layerLevels, out exitCellsList, out yLevel);

                    chamber.chamberTheme.roomsToInclude.Clear();
                    chamber.chamberTheme.yOffset = yLevel;
                    chamber.chamberTheme.levelInfomation.Clear();
                    chamber.chamberTheme.polygonLists.Clear();
 

                    BuildingGeneratorCore.PrepareForInteriorGen(chamber.chamberTheme, cellTableList);

                    // Instantiate the building parent object
                    GameObject buildingParent = Instantiate(m_DungeonGeneratorScriptInstance.buildingParentPrefab.gameObject, new Vector3(cellTableList[0][0, 0].cellCenter.x, yLevel, cellTableList[0][0, 0].cellCenter.y), Quaternion.identity, levelParent.transform);

                    //spawn chamber

                    BuildingConfig buildingConfig = chamber.chamberTheme;

                    
                    Action<BuildingLevelInfo> resetLayout = (BuildingLevelInfo buildingLevelInfo) =>
                    {
                        //ResetLayout(ref buildingLevelInfo);
                    };

                    // Start the coroutine for creating levels and begin the building generation process
                    BuildingGeneratorCore.CreateLevels(buildingConfig, randomGen, buildingParent.GetComponent<GeneratedBuildingParent>(), resetLayout, cellTableList, exitCellsList);


                    BuildingGeneratorCore.SpawnBuilding(buildingConfig, randomGen, objectInstantiationManager, buildingParent.GetComponent<GeneratedBuildingParent>(), levelParent.transform, decorateInteriors, true, useStorm, 0, yLevel, false);


                    spawnedChambers.Add(chamber);

                }
                GameObject hallwayParent = new GameObject();

                hallwayParent.name = "Hallways";
                hallwayParent.transform.parent = levelParent.transform;
                hallwayParent.transform.localPosition = Vector3.zero;
                foreach (Hallway hallway in currentLevel.hallways)
                {
                    if (hallway.hallwayCells.Count == 0)
                    {
                        continue;
                    }

                    List<List<BorderCell>> exitCellsList;
                    float yLevel;
                    List<CellInfo[,]> cellTableList = DungeonGeneratorUtils.ConvertDungeonHallway(hallway, currentLevel, out exitCellsList, out yLevel, gridSize);


                    BuildingConfig buildingConfig = m_DungeonGeneratorScriptInstance.hallwayBuildingInfo;

                    buildingConfig.roomsToInclude.Clear();
                    buildingConfig.yOffset = yLevel;
                    buildingConfig.levelInfomation.Clear();
                    buildingConfig.polygonLists.Clear();


                    BuildingGeneratorCore.PrepareForInteriorGen(buildingConfig, cellTableList);

                    // Instantiate the building parent object
                    GameObject buildingParent = Instantiate(m_DungeonGeneratorScriptInstance.buildingParentPrefab.gameObject, new Vector3(cellTableList[0][0, 0].cellCenter.x, yLevel, cellTableList[0][0, 0].cellCenter.y), Quaternion.identity, hallwayParent.transform);

                    //spawn chamber
                    Action<BuildingLevelInfo> resetLayout = (BuildingLevelInfo buildingLevelInfo) =>
                    {
                        //ResetLayout(ref buildingLevelInfo);
                    };
                    // Start the coroutine for creating levels and begin the building generation process
                    BuildingGeneratorCore.CreateLevels(buildingConfig, randomGen, buildingParent.GetComponent<GeneratedBuildingParent>(), resetLayout, cellTableList, exitCellsList);


                    BuildingGeneratorCore.SpawnBuilding(buildingConfig, randomGen, objectInstantiationManager, buildingParent.GetComponent<GeneratedBuildingParent>(), hallwayParent.transform, decorateInteriors, true, useStorm, 0, yLevel, false);
                }
                int n = k + 1;
                int nl = i;
                if (n > dungeonLayers[i].layerLevels.Count - 1)
                {
                    n = 0;
                    nl = i + 1;
                }
                DungeonLevel nextLevel = null;
                if (nl <= dungeonLayers.Count - 1)
                {
                    nextLevel = dungeonLayers[nl].layerLevels[n];
                }
                lastLevel = currentLevel;
            }
        }
        //AddTRSListsToStorm();


        TerrainHoleCutter.CutHole(m_DungeonGeneratorScriptInstance.terrainHoleSize, m_DungeonGeneratorScriptInstance.terrainHolePosition + transform.position);

        StopEditorUpdates();
        EditorUtility.ClearProgressBar();
        yield break;
    }

    public IEnumerator Apply()
    {
        GameObject newParent = new GameObject();
        newParent.transform.position = m_DungeonGeneratorScriptInstance.transform.position;
        newParent.transform.rotation = m_DungeonGeneratorScriptInstance.transform.rotation;
        newParent.transform.name = "Dungeon Parent";
        if (m_DungeonGeneratorScriptInstance.useStorm)
        {
            GeneratedBuildingParent[] childBuildings = m_DungeonGeneratorScriptInstance.GetComponentsInChildren<GeneratedBuildingParent>();
            foreach (GeneratedBuildingParent childBuilding in childBuildings)
            {
                childBuilding.ConvertToStorm();
            }
        }

        int childCount = transform.childCount;
        for (int i = childCount-1; i >= 0; i--)
        {
            transform.GetChild(i).gameObject.transform.parent = newParent.transform;
        }


        StopEditorUpdates();
        EditorUtility.ClearProgressBar();
        yield break;
    }

    public void Clear()
    {
        List<GameObject> children = new List<GameObject>();
        int childCount = transform.childCount;

        for (int i = 0; i < childCount; i++)
        {
            Transform childTransform = transform.GetChild(i);
            children.Add(childTransform.gameObject);
        }

        for (int i = 0; i < children.Count; i++)
        {
            DestroyImmediate(children[i].gameObject);
        }

        TerrainHoleCutter.FillHole(m_DungeonGeneratorScriptInstance.terrainHoleSize, m_DungeonGeneratorScriptInstance.terrainHolePosition + transform.position);
    }
       
}
