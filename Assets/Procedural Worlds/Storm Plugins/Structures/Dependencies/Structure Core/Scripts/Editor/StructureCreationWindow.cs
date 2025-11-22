using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using PWCommon5;
using ProceduralWorlds.Storm.StormEditor;
using ProceduralWorlds.Storm.StructureGenCore;
using ProceduralWorlds.Storm.BuildingGen;
using ProceduralWorlds.Storm.AreaPopulator;
using ProceduralWorlds.Storm.Core;

using UnityEditorInternal;

public class StructureCreationWindow : EditorWindow, IPWEditor
{
    public bool PositionChecked { get; set; }


    private TabSet m_mainTabs;
    private EditorUtils m_editorUtils;



    SerializedObject serializedObject;

    #region Areas

    #region Object Baker
    public string objectBakerBuildingConfigPath = "Assets/";

    SerializedProperty p_ObjectBakerBuildingConfigPath;

    public List<GameObject> objectsToBake = new List<GameObject>();

    SerializedProperty p_ObjectsToBake;

    public double concavity = 0.01;

    SerializedProperty p_Concavity;

    public string prefixRemoval = "";

    SerializedProperty p_PrefixRemoval;

    #endregion

    #region PlacementFilter
    public List<DecorationObjectInfo> filterAssignmentObjects = new List<DecorationObjectInfo>();

    SerializedProperty p_FilterAssignmentObjects;

    public PlacementFilterType placementFilterType;
    private PlacementFilterEditor placementFilterEditor;
    public PlacementFilter placementFilterInstance;
    SerializedProperty p_PlacementFilterType;

    public string placementFilterPath = "Assets/";

    SerializedProperty p_PlacementFilterPath;

    public string placementFilterName = "New Filter";

    SerializedProperty p_PlacementFilterName;
    #endregion

    #region Prefab Creation
    public GameObject parentObject;

    SerializedProperty p_ParentObject;

    public string prefabPath = "Assets/";

    SerializedProperty p_PrefabPath;

    #endregion
    #endregion

    #region Buildings
    private int buildingConfigCreationStep = 0;
    private const int buildingConfigCreationEndStep = 3;
    #region Step 1
    public BuildingConfig buildingConfig;
    SerializedProperty p_BuildingConfig;
    #endregion
    #region Step 2
    public BuildingBlocks buildingBlocks;
    public WallBlocks wallBlocks;
    public RoofBlocks roofBlocks;
    SerializedProperty p_BuildingBlocks;
    SerializedProperty p_WallBlocks;
    SerializedProperty p_RoofBlocks;

    private BuildingBlocksEditor buildingBlocksEditor;
    #endregion
    #region Step 3
    public List<SimpleRoomData> rooms = new List<SimpleRoomData>();

    public ReorderableList roomsReorderable;
    #endregion
    #region Step 4
    public string buildingName = "New Building";
    public int floors;
    public string buildingConfigPath = "Assets/";

    public SerializedProperty p_BuildingName;
    public SerializedProperty p_Floors;
    public SerializedProperty p_BuildingConfigPath;
    #endregion

    #endregion
    //Commented out for now, the buttons in the Plugins tab should be enough to open / access
    //[MenuItem("Window/" + PWConst.COMMON_MENU + "/Storm/Generator Config Builder", false, 100)]
    public static void ShowManager()
    {
        var window = GetWindow<StructureCreationWindow>(false, "Generator Config Builder");
        Vector2 initialSize = new Vector2(600f, 450f);
        window.position = new Rect(new Vector2((Screen.currentResolution.width - initialSize.x) / 2f, (Screen.currentResolution.height - initialSize.y) / 2f), initialSize);
        window.Show();
    }

    public void OnGUI()
    {
        if (m_editorUtils == null)
        {
            return;
        }
        m_editorUtils.Initialize(); // Do not remove this!
        m_editorUtils.GUIHeader();
        EditorGUI.BeginChangeCheck();
        m_editorUtils.Tabs(m_mainTabs);

        if (EditorGUI.EndChangeCheck())
        {
            serializedObject.ApplyModifiedProperties();
        }


    }

    public void OnEnable()
    {
        serializedObject = new SerializedObject(this);
        #region Areas
        p_ObjectBakerBuildingConfigPath = serializedObject.FindProperty("objectBakerBuildingConfigPath");
        p_ObjectsToBake = serializedObject.FindProperty("objectsToBake");
        p_Concavity = serializedObject.FindProperty("concavity");
        p_PrefixRemoval = serializedObject.FindProperty("prefixRemoval");


        p_FilterAssignmentObjects = serializedObject.FindProperty("filterAssignmentObjects");
        p_PlacementFilterType = serializedObject.FindProperty("placementFilterType");
        p_PlacementFilterPath = serializedObject.FindProperty("placementFilterPath");
        p_PlacementFilterName = serializedObject.FindProperty("placementFilterName");


        p_ParentObject = serializedObject.FindProperty("parentObject");
        p_PrefabPath = serializedObject.FindProperty("prefabPath");
        #endregion

        #region Buildings
        p_BuildingConfig = serializedObject.FindProperty("buildingConfig");
        p_BuildingBlocks = serializedObject.FindProperty("buildingBlocks");
        p_WallBlocks = serializedObject.FindProperty("wallBlocks");
        p_RoofBlocks = serializedObject.FindProperty("roofBlocks");
        
        ResetBuildingConfigCreator();

        roomsReorderable = new ReorderableList(serializedObject, serializedObject.FindProperty("rooms"), true, true, true, true);
        SetUpReorderableList(roomsReorderable, "Desired Rooms");

        p_BuildingName = serializedObject.FindProperty("buildingName");
        p_Floors = serializedObject.FindProperty("floors");
        p_BuildingConfigPath = serializedObject.FindProperty("buildingConfigPath");
        #endregion

        serializedObject.ApplyModifiedProperties();

        if (m_editorUtils == null)
        {
            // Get editor utils for this
            m_editorUtils = StormApp.GetEditorUtils(this);
        }

        Tab[] mainTabs = new Tab[]
        {
            //new Tab ("General", General),
            new Tab ("Areas", Areas),
            new Tab ("Buildings", Buildings),
            //new Tab ("Cities", Cities),
            //new Tab ("Dungeons", Dungeons),

        };
        m_mainTabs = new TabSet(m_editorUtils, mainTabs);
    }

    private void SetUpReorderableList(ReorderableList list, string header)
    {
        list.elementHeightCallback = (int index) =>
        {
            var element = list.serializedProperty.GetArrayElementAtIndex(index);
            return EditorGUI.GetPropertyHeight(element);
        };
        list.drawHeaderCallback = (Rect rect) => {
            EditorGUI.LabelField(rect, header);
        };
        list.drawElementCallback =
        (Rect rect, int index, bool isActive, bool isFocused) => {
            var element = list.serializedProperty.GetArrayElementAtIndex(index);

            EditorGUI.PropertyField(rect, element, new GUIContent("Room " + (index + 1)), true);
        };

        list.onAddCallback = (ReorderableList list) =>
        {
            int lastIndex = list.serializedProperty.arraySize;
            list.serializedProperty.InsertArrayElementAtIndex(lastIndex);
            var element = list.serializedProperty.GetArrayElementAtIndex(lastIndex);
            element.FindPropertyRelative("roomName").stringValue = "New Room";
            element.FindPropertyRelative("priority").intValue = 0;
            element.FindPropertyRelative("desiredFloors").intValue = 1;
            element.FindPropertyRelative("useDesiredStartFloor").boolValue = false;
            element.FindPropertyRelative("desiredStartFloor").intValue = 0;
            element.FindPropertyRelative("capRoomCount").boolValue = true;
            element.FindPropertyRelative("maxRoomCount").intValue = 4;
            element.FindPropertyRelative("allowShifting").boolValue = true;
            element.FindPropertyRelative("desiredRoomSize").vector2IntValue = new Vector2Int(2, 2);
            element.FindPropertyRelative("maxConnections").intValue = 3;
            element.FindPropertyRelative("connectionType").enumValueIndex = 0;
            element.FindPropertyRelative("connectToStaircase").boolValue = false;
        };
    }

    public void General()
    {
        m_editorUtils.Panel("GeneralPanel", GeneralPanel, true);
    }

    public void GeneralPanel(bool helpActive)
    {

    }

    public void Areas()
    {
        //m_editorUtils.Panel("AreasPanel", AreasPanel, false);
        m_editorUtils.Panel("ObjectBakerPanel", ObjectBakerPanel, false);
        m_editorUtils.Panel("PlacementFilterPanel", PlacementFilterPanel, false);
        m_editorUtils.Panel("PrefabCreationPanel", PrefabCreationPanel, false);
    }

    public void AreasPanel(bool helpActive)
    {

    }

    public void ObjectBakerPanel(bool helpActive)
    {
        PropField("ObjectsToBake", p_ObjectsToBake, helpActive);
        PropField("Concavity", p_Concavity, helpActive);
        PropField("ObjectBakerBuildingConfigPath", p_ObjectBakerBuildingConfigPath, helpActive);
        PropField("PrefixRemoval", p_PrefixRemoval, helpActive);

        if (m_editorUtils.Button("Bake"))
        {
            Bake(objectsToBake, objectBakerBuildingConfigPath, concavity, prefixRemoval);
        }

        serializedObject.ApplyModifiedProperties();
    }



    public void Bake(List<GameObject> objectsToBake, string objectBakerBuildingConfigPath, double concavity, string prefixRemoval)
    {
        // Create the directory if it doesn't exist
        if (!System.IO.Directory.Exists(objectBakerBuildingConfigPath))
        {
            System.IO.Directory.CreateDirectory(objectBakerBuildingConfigPath);
        }

        for (int i = 0; i < objectsToBake.Count; i++)
        {
            GameObject sourceObject = objectsToBake[i];
            string objectName = sourceObject.name;
            if (prefixRemoval != "")
            {
                objectName = objectName.Replace(prefixRemoval, "");
            }

            // Reset position and rotation of the source object
            sourceObject.transform.position = Vector3.zero;
            sourceObject.transform.rotation = Quaternion.identity;

            // Find the mesh renderer
            MeshRenderer meshRenderer = sourceObject.GetComponent<MeshRenderer>();
            if (meshRenderer == null)
            {
                meshRenderer = sourceObject.GetComponentInChildren<MeshRenderer>();
                if (meshRenderer == null)
                {
                    Debug.LogError("No mesh renderer found on object: " + sourceObject.name);
                    continue;
                }
            }

            // Get the mesh filter associated with the renderer
            MeshFilter meshFilter = meshRenderer.GetComponent<MeshFilter>();
            if (meshFilter == null || meshFilter.sharedMesh == null)
            {
                Debug.LogError("No valid mesh filter or mesh found on object: " + sourceObject.name);
                continue;
            }

            // Check if it's a prefab instance and unpack if needed
            bool isPrefabInstance = PrefabUtility.IsPartOfPrefabInstance(sourceObject);
            if (isPrefabInstance)
            {
                PrefabUtility.UnpackPrefabInstance(sourceObject, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
            }

            // Create a path for the new prefab
            string prefabPath = System.IO.Path.Combine(objectBakerBuildingConfigPath, objectName + ".prefab").Replace("\\", "/");

            // Create a duplicate of the source object
            GameObject duplicateObject = GameObject.Instantiate(sourceObject);
            duplicateObject.name = objectName;

            // Add DecorationObjectInfo component
            DecorationObjectInfo decorationObjectInfo = duplicateObject.AddComponent<DecorationObjectInfo>();

            // Set bounds
            decorationObjectInfo.objectSize = meshRenderer.bounds;

            // Calculate polygon
            StormPolygon stormPolygon = StormPolygonUtility.CalculatePolygon(meshFilter);

            Vector3 lastPoint = new Vector3((float)stormPolygon.points[stormPolygon.points.Count - 1].x, 0, (float)stormPolygon.points[stormPolygon.points.Count - 1].z);

            for (int k = 0; k < stormPolygon.points.Count; k++)
            {
                StormPolygonPoint point = stormPolygon.points[k];

                Vector3 pointV3 = new Vector3((float)point.x, 0, (float)point.z);

                Debug.DrawLine(lastPoint, pointV3, Color.red, 40);

                lastPoint = pointV3;

                Vector2 vector2 = new Vector2((float)point.x, (float)point.z);

                decorationObjectInfo.objectPolygon.Add(vector2);
            }

            // Save as prefab
            GameObject prefabAsset = PrefabUtility.SaveAsPrefabAsset(duplicateObject, prefabPath);
            if (prefabAsset != null)
            {
                Debug.Log($"Created decoration object prefab: {prefabPath}");
            }
            else
            {
                Debug.LogError($"Failed to create prefab: {prefabPath}");
            }

            // Destroy the temporary duplicate
            GameObject.DestroyImmediate(duplicateObject);
        }
    }

    public void PlacementFilterPanel(bool helpActive)
    {
        PropField("FilterAssignmentObjects", p_FilterAssignmentObjects, helpActive);
        PropField("PrefixRemoval", p_PrefixRemoval, helpActive);
        PropField("PlacementFilterType", p_PlacementFilterType, helpActive);

        switch (placementFilterType)
        {
            case PlacementFilterType.Line:
                if (placementFilterInstance == null)
                {
                    placementFilterInstance = CreateInstance<LinePlacement>();
                    placementFilterEditor = Editor.CreateEditor(placementFilterInstance) as PlacementFilterEditor;
                }
                else if (placementFilterInstance is not LinePlacement)
                {
                    DestroyImmediate(placementFilterInstance);
                    DestroyImmediate(placementFilterEditor);
                    placementFilterInstance = CreateInstance<LinePlacement>();
                    placementFilterEditor = Editor.CreateEditor(placementFilterInstance) as PlacementFilterEditor;
                }
                else
                {
                    placementFilterEditor.OnInspectorGUI();
                }
                break;
            case PlacementFilterType.Edge:
                if (placementFilterInstance == null)
                {
                    placementFilterInstance = CreateInstance<EdgePlacement>();
                    placementFilterEditor = Editor.CreateEditor(placementFilterInstance) as PlacementFilterEditor;
                }
                else if (placementFilterInstance is not EdgePlacement)
                {
                    DestroyImmediate(placementFilterInstance);
                    DestroyImmediate(placementFilterEditor);
                    placementFilterInstance = CreateInstance<EdgePlacement>();
                    placementFilterEditor = Editor.CreateEditor(placementFilterInstance) as PlacementFilterEditor;
                }
                else
                {
                    placementFilterEditor.OnInspectorGUI();
                }
                break;
        }

        PropField("PlacementFilterPath", p_PlacementFilterPath, helpActive);
        PropField("PlacementFilterName", p_PlacementFilterName, helpActive);

        if (m_editorUtils.Button("CreateAndAssign"))
        {
            CreateAndAssign();
        }
    }


    public void PrefabCreationPanel(bool helpActive)
    {
        PropField("ParentObject", p_ParentObject, helpActive);
        PropField("PrefabPath", p_PrefabPath, helpActive);
        PropField("PrefixRemoval", p_PrefixRemoval, helpActive);

        if (m_editorUtils.Button("Create"))
        {
            PrefabCreation(parentObject, p_PrefabPath.stringValue, p_PrefixRemoval.stringValue);
        }

        serializedObject.ApplyModifiedProperties();
    }

    public void PrefabCreation(GameObject objectParent, string prefabPath, string prefixRemoval)
    {
        if (objectParent == null)
        {
            Debug.LogError("Object parent is null.");
            return;
        }

        // Create the directory if it doesn't exist
        if (!System.IO.Directory.Exists(prefabPath))
        {
            System.IO.Directory.CreateDirectory(prefabPath);
        }

        // Find all objects with unique mesh renderers
        List<GameObject> uniqueMeshObjects = new List<GameObject>();
        FindUniqueRenderers(objectParent, uniqueMeshObjects);

        // Process each unique mesh object
        foreach (GameObject obj in uniqueMeshObjects)
        {
            // Check if it's a prefab instance
            bool isPrefabInstance = PrefabUtility.IsPartOfPrefabInstance(obj);

            if (isPrefabInstance)
            {
                // Unpack the prefab completely
                PrefabUtility.UnpackPrefabInstance(obj, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
            }

            // Store the original scale
            Vector3 originalScale = obj.transform.localScale;

            // Reset position and rotation, but keep the scale
            obj.transform.position = Vector3.zero;
            obj.transform.rotation = Quaternion.identity;

            // Get the object name and remove prefix if necessary
            string objName = obj.name;
            if (!string.IsNullOrEmpty(prefixRemoval) && objName.StartsWith(prefixRemoval))
            {
                objName = objName.Substring(prefixRemoval.Length);
            }

            // Create a path for the new prefab
            string prefabPathNew = System.IO.Path.Combine(prefabPath, objName + ".prefab").Replace("\\", "/");

            // Create the prefab
            GameObject prefabAsset = PrefabUtility.SaveAsPrefabAsset(obj, prefabPathNew);
            if (prefabAsset != null)
            {
                Debug.Log($"Created prefab: {prefabPathNew}");
            }
            else
            {
                Debug.LogError($"Failed to create prefab: {prefabPathNew}");
            }
        }

        Debug.Log($"Processed {uniqueMeshObjects.Count} unique mesh objects and saved them as prefabs in {prefabPath}");
    }

    private void FindUniqueRenderers(GameObject parent, List<GameObject> uniqueMeshObjects)
    {
        // Get all mesh renderers in this object and its children
        MeshRenderer[] renderers = parent.GetComponentsInChildren<MeshRenderer>(true);

        // Dictionary to track unique meshes by their instance ID
        Dictionary<int, bool> processedMeshes = new Dictionary<int, bool>();
        // Dictionary to track processed prefab roots
        Dictionary<GameObject, bool> processedPrefabRoots = new Dictionary<GameObject, bool>();

        foreach (MeshRenderer renderer in renderers)
        {
            // Get the mesh filter associated with this renderer
            MeshFilter meshFilter = renderer.GetComponent<MeshFilter>();
            if (meshFilter != null && meshFilter.sharedMesh != null)
            {
                int meshID = meshFilter.sharedMesh.GetInstanceID();

                // If we haven't processed this mesh yet
                if (!processedMeshes.ContainsKey(meshID))
                {
                    processedMeshes[meshID] = true;

                    // Check if this renderer is part of a prefab
                    GameObject prefabRoot = null;
                    if (PrefabUtility.IsPartOfPrefabInstance(renderer.gameObject))
                    {
                        prefabRoot = PrefabUtility.GetOutermostPrefabInstanceRoot(renderer.gameObject);
                    }

                    // If it's part of a prefab and we haven't processed this prefab root yet
                    if (prefabRoot != null && !processedPrefabRoots.ContainsKey(prefabRoot))
                    {
                        processedPrefabRoots[prefabRoot] = true;
                        uniqueMeshObjects.Add(prefabRoot);
                    }
                    // If it's not part of a prefab, add the renderer's gameObject directly
                    else if (prefabRoot == null)
                    {
                        uniqueMeshObjects.Add(renderer.gameObject);
                    }
                }
            }
        }
    }

    public void CreateAndAssign()
    {
        if (placementFilterInstance == null)
        {
            return;
        }
        AssetDatabase.CreateAsset(placementFilterInstance, placementFilterPath + placementFilterName + ".asset");
        EditorUtility.SetDirty(placementFilterInstance);

        for (int i = 0; i < filterAssignmentObjects.Count; i++)
        {
            filterAssignmentObjects[i].placementFilter = placementFilterInstance;
        }
    }
   
    public void Buildings()
    {
        m_editorUtils.Panel("BuildingConfigCreator", BuildingConfigCreatorPanel, true);
    }

    public void BuildingConfigCreatorPanel(bool helpActive)
    {
        switch (buildingConfigCreationStep)
        {
            case 0:
                //intended use(flat roof or no, alternate first floor wall, ect.)
                m_editorUtils.Label("Step1", helpActive);

                if (buildingBlocksEditor != null)
                {
                    DestroyImmediate(buildingBlocksEditor);
                    buildingBlocksEditor = null;
                }

                // Save the original label width
                float originalLabelWidth = EditorGUIUtility.labelWidth;
                // Set a wider label width to prevent truncation
                EditorGUIUtility.labelWidth = EditorGUIUtility.currentViewWidth * 0.7f;

                buildingConfig.generateInteriorLayout = m_editorUtils.Toggle("GenerateInteriorLayout", buildingConfig.generateInteriorLayout, helpActive);
                buildingConfig.useAlternateWallFirstLayer = m_editorUtils.Toggle("UseAlternateWallFirstLayer", buildingConfig.useAlternateWallFirstLayer, helpActive);
                buildingConfig.createFrontStep = m_editorUtils.Toggle("CreateFrontStep", buildingConfig.createFrontStep, helpActive);
                buildingConfig.roofType = (RoofType)m_editorUtils.EnumPopup("RoofType", buildingConfig.roofType, helpActive);
                buildingConfig.useRoofPeakJoinEdgeCaseFix = m_editorUtils.Toggle("UseRoofPeakJoinEdgeCaseFix", buildingConfig.useRoofPeakJoinEdgeCaseFix, helpActive);
                buildingConfig.XZGridSize = m_editorUtils.FloatField("XZGridSize", buildingConfig.XZGridSize, helpActive);
                buildingConfig.YGridSize = m_editorUtils.FloatField("YGridSize", buildingConfig.YGridSize, helpActive);
                buildingConfig.RoofYGridSize = m_editorUtils.FloatField("RoofYGridSize", buildingConfig.RoofYGridSize, helpActive);
                // Restore the original label width
                EditorGUIUtility.labelWidth = originalLabelWidth;
                break;
            case 1:
                //Define Grid Size and Define Building Block Set To use(or create new one) -the system should prompt you depending on your intended use case if a building blocks set has everything you need
                m_editorUtils.Label("Step2", helpActive);
                EditorGUILayout.Space();
                buildingConfig.generateInteriorLayout = m_editorUtils.Toggle("GenerateInteriorLayout", buildingConfig.generateInteriorLayout, helpActive);
                buildingConfig.useAlternateWallFirstLayer = m_editorUtils.Toggle("UseAlternateWallFirstLayer", buildingConfig.useAlternateWallFirstLayer, helpActive);
                buildingConfig.createFrontStep = m_editorUtils.Toggle("CreateFrontStep", buildingConfig.createFrontStep, helpActive);
                buildingConfig.roofType = (RoofType)m_editorUtils.EnumPopup("RoofType", buildingConfig.roofType, helpActive);
                buildingConfig.useRoofPeakJoinEdgeCaseFix = m_editorUtils.Toggle("UseRoofPeakJoinEdgeCaseFix", buildingConfig.useRoofPeakJoinEdgeCaseFix, helpActive);
                buildingConfig.XZGridSize = m_editorUtils.FloatField("XZGridSize", buildingConfig.XZGridSize, helpActive);
                buildingConfig.YGridSize = m_editorUtils.FloatField("YGridSize", buildingConfig.YGridSize, helpActive);
                buildingConfig.RoofYGridSize = m_editorUtils.FloatField("RoofYGridSize", buildingConfig.RoofYGridSize, helpActive);
                EditorGUI.BeginChangeCheck();
                PropField("BuildingBlocks", p_BuildingBlocks, helpActive);
                bool buildingBlocksChanged = EditorGUI.EndChangeCheck();
                EditorGUILayout.Space();
                if (buildingBlocksChanged)
                {
                    BuildingBlocks newbuildingBlocks = CreateInstance<BuildingBlocks>();
                    //copy does not work yet.
                    newbuildingBlocks.Copy((BuildingBlocks)p_BuildingBlocks.objectReferenceValue);
                    p_BuildingBlocks.objectReferenceValue = newbuildingBlocks;
                    serializedObject.ApplyModifiedProperties();

                    if (buildingBlocksEditor != null)
                    {
                        DestroyImmediate(buildingBlocksEditor);
                    }
                    // Create an editor for the sub-object and draw its inspector
                    buildingBlocksEditor = Editor.CreateEditor(buildingBlocks) as BuildingBlocksEditor;
                    buildingBlocksEditor.m_defaultOpenPanels = true;
                }
                else if (buildingBlocksEditor == null)
                {
                    // Create an editor for the sub-object and draw its inspector
                    buildingBlocksEditor = Editor.CreateEditor(buildingBlocks) as BuildingBlocksEditor;
                    buildingBlocksEditor.m_defaultOpenPanels = true;
                }
                buildingBlocksEditor.OnInspectorGUI();
                break;
            case 2:
                //Define first room set, repeated room set, optionally fill out hallway and staircase info in an advanced mode?
                m_editorUtils.Label("Step3", helpActive);
                roomsReorderable.DoLayoutList();
                break;
            case 3:
                //define asset buildingConfigPath and review/create!
                m_editorUtils.Label("Step4", helpActive);

                PropField("BuildingName", p_BuildingName, helpActive);
                PropField("Floors", p_Floors, helpActive);
                PropField("BuildingConfigPath", p_BuildingConfigPath, helpActive);
                break;
        }


        EditorGUILayout.BeginHorizontal();
        if (m_editorUtils.Button("BackButton", helpActive))
        {
            if (buildingConfigCreationStep == 3 && !buildingConfig.generateInteriorLayout)
            {
                buildingConfigCreationStep--;
            }

            if (buildingConfigCreationStep != 0)
            {
                buildingConfigCreationStep--;
            }
        }
        if (buildingConfigCreationStep != buildingConfigCreationEndStep)
        {
            if (m_editorUtils.Button("NextButton", helpActive))
            {
                if (buildingConfigCreationStep == 1 && !buildingConfig.generateInteriorLayout)
                {
                    buildingConfigCreationStep++;
                }

                buildingConfigCreationStep++;
            }
        }
        else
        {
            Color originalGUIColor = GUI.backgroundColor;
            GUI.backgroundColor = Color.green;
            if (m_editorUtils.Button("CreateButton", helpActive))
            {
                CreateBuildingConfig();
            }
            GUI.backgroundColor = originalGUIColor;
        }

        EditorGUILayout.EndHorizontal();
    }

    public void CreateBuildingConfig()
    {
        if (!buildingConfigPath.EndsWith("/"))
        {
            buildingConfigPath += "/";
        }
        


        if(buildingBlocks.wallBlocksList[0].wallBlocks == wallBlocks)
        {
            // Create directories for wall and roof blocks if they don't exist
            string wallBlocksDir = buildingConfigPath+ "/" + buildingName + " Wall Blocks/";
            // Ensure directories exist
            if (!System.IO.Directory.Exists(wallBlocksDir))
            {
                System.IO.Directory.CreateDirectory(wallBlocksDir);
            }

            // Save individual wall block assets
            SaveWallBlockAssets(wallBlocks, wallBlocksDir);

            AssetDatabase.CreateAsset(wallBlocks, buildingConfigPath + buildingName + " Wall Blocks" + ".asset");
            EditorUtility.SetDirty(wallBlocks);
        }

        if(buildingBlocks.roofBlocksList[0].roofBlocks == roofBlocks)
        {
            string roofBlocksDir = buildingConfigPath + "/" + buildingName + " Roof Blocks/";
            // Ensure directories exist
            if (!System.IO.Directory.Exists(roofBlocksDir))
            {
                System.IO.Directory.CreateDirectory(roofBlocksDir);
            }

            // Save individual roof block assets
            SaveRoofBlockAssets(roofBlocks, roofBlocksDir);

            AssetDatabase.CreateAsset(roofBlocks, buildingConfigPath + buildingName + " Roof Blocks" + ".asset");
            EditorUtility.SetDirty(roofBlocks);
        }
        
        // Save main assets
        AssetDatabase.CreateAsset(buildingBlocks, buildingConfigPath + buildingName + " Blocks" + ".asset");
        EditorUtility.SetDirty(buildingBlocks);
        
        // Refresh the asset database to ensure all assets are properly registered
        AssetDatabase.Refresh();
        
        buildingConfig.floors = floors;
        buildingConfig.buildingBlocks = buildingBlocks;
        buildingConfig.chanceOfWindow = 25;
        AssetDatabase.CreateAsset(buildingConfig, buildingConfigPath + buildingName + ".asset");
        EditorUtility.SetDirty(buildingConfig);

        if (buildingConfig.generateInteriorLayout)
        {
            CreateRooms(buildingConfigPath, buildingConfig);
        }
        
        EditorUtility.FocusProjectWindow();
        Selection.activeObject = buildingBlocks;
        EditorGUIUtility.PingObject(buildingBlocks);

        ResetBuildingConfigCreator();
    }

    public void CreateRooms(string buildingConfigPath, BuildingConfig buildingConfig)
    {
        List<(RoomInfo, SimpleRoomData)> allRoomsWithData = new List<(RoomInfo, SimpleRoomData)>();
        List<RoomInfo> allRooms = new List<RoomInfo>();

        RoomInfo staircaseRoom = CreateInstance<RoomInfo>();
        
        staircaseRoom.stairCase = true;
        staircaseRoom.hallway = false;
        staircaseRoom.expand = false;
        staircaseRoom.createWindows = true;
        staircaseRoom.priority = 10;
        staircaseRoom.desiredRoomSize = new Vector2Int(2, 2);
        staircaseRoom.desiredFloors = 0;
        staircaseRoom.useDesiredStartFloor = true;
        staircaseRoom.desiredStartFloor = 0;
        staircaseRoom.capRoomCount = true;
        staircaseRoom.maxRoomCount = 1;
        staircaseRoom.roomStoreyBehaviors = new List<RoomStoreyBehaviour>();
        staircaseRoom.persistLocationThroughFloors = true;
        staircaseRoom.edgeFitnessModifier = 1;
        staircaseRoom.connectivityFitnessModifier = 0;
        staircaseRoom.avoidFitnessModifier = 0;
        staircaseRoom.maxConnections = 5;
        staircaseRoom.connectionType = RoomConnectionType.SingleDoor;
        AssetDatabase.CreateAsset(staircaseRoom, buildingConfigPath + "Staircase Room.asset");
        EditorUtility.SetDirty(staircaseRoom);

        buildingConfig.rooms.Add(staircaseRoom);
        allRooms.Add(staircaseRoom);

        foreach (SimpleRoomData simpleRoomData in rooms)
        {
            RoomInfo newRoomInfo = CreateInstance<RoomInfo>();
            newRoomInfo.stairCase = false;
            newRoomInfo.hallway = false;
            newRoomInfo.expand = true;
            newRoomInfo.createWindows = true;
            newRoomInfo.priority = simpleRoomData.priority;
            newRoomInfo.desiredRoomSize = simpleRoomData.desiredRoomSize;
            newRoomInfo.desiredFloors = simpleRoomData.desiredFloors;
            newRoomInfo.roomStoreyBehaviors = simpleRoomData.roomStoreyBehaviors;
            newRoomInfo.persistLocationThroughFloors = true;
            newRoomInfo.edgeFitnessModifier = 1;
            newRoomInfo.connectivityFitnessModifier = 1;
            newRoomInfo.avoidFitnessModifier = 1;
            newRoomInfo.maxConnections = simpleRoomData.maxConnections;
            newRoomInfo.connectionType = simpleRoomData.connectionType;
            newRoomInfo.useDesiredStartFloor = simpleRoomData.useDesiredStartFloor;
            newRoomInfo.desiredStartFloor = simpleRoomData.desiredStartFloor;
            newRoomInfo.capRoomCount = simpleRoomData.capRoomCount;
            newRoomInfo.maxRoomCount = simpleRoomData.maxRoomCount;
            newRoomInfo.allowShifting = simpleRoomData.allowShifting;
            AssetDatabase.CreateAsset(newRoomInfo, buildingConfigPath + simpleRoomData.roomName + ".asset");
            EditorUtility.SetDirty(newRoomInfo);

            buildingConfig.rooms.Add(newRoomInfo);
            allRoomsWithData.Add((newRoomInfo, simpleRoomData));
            allRooms.Add(newRoomInfo);
        }



        foreach ((RoomInfo room, SimpleRoomData simpleRoomData) in allRoomsWithData)
        {
            if (simpleRoomData.connectToStaircase)
            {
                room.desiredConnections.Add(staircaseRoom);
            }
            foreach (string desiredConnection in simpleRoomData.desiredConnections)
            {
                foreach (RoomInfo currentRoom in allRooms)
                {
                    if (currentRoom.name == desiredConnection && !room.desiredConnections.Contains(currentRoom))
                    {
                        room.desiredConnections.Add(currentRoom);
                    }
                }
            }


            foreach (string undesiredConnection in simpleRoomData.undesiredConnections)
            {
                foreach (RoomInfo currentRoom in allRooms)
                {
                    if (currentRoom.name == undesiredConnection && !room.undesiredConnections.Contains(currentRoom))
                    {
                        room.undesiredConnections.Add(currentRoom);
                    }
                }
            }
            EditorUtility.SetDirty(room);

        }
    }

    public void ResetBuildingConfigCreator()
    {
        buildingBlocksEditor = null;
        buildingBlocks = null;
        p_BuildingConfig.objectReferenceValue = CreateInstance<BuildingConfig>();
        p_BuildingBlocks.objectReferenceValue = CreateInstance<BuildingBlocks>();
        p_WallBlocks.objectReferenceValue = CreateInstance<WallBlocks>();
        p_RoofBlocks.objectReferenceValue = CreateInstance<RoofBlocks>();
        buildingConfig = (BuildingConfig)p_BuildingConfig.objectReferenceValue;
        buildingBlocks = (BuildingBlocks)p_BuildingBlocks.objectReferenceValue;
        wallBlocks = (WallBlocks)p_WallBlocks.objectReferenceValue;
        roofBlocks = (RoofBlocks)p_RoofBlocks.objectReferenceValue;
        wallBlocks.parentBuildingConfig = buildingConfig;
        roofBlocks.parentBuildingConfig = buildingConfig;
        buildingBlocks.wallBlocksList.Add(new WallBlocksData(wallBlocks));
        buildingBlocks.roofBlocksList.Add(new RoofBlocksData(roofBlocks));
        
        // Create and assign BuildingBlock assets for all fields in WallBlocks and RoofBlocks
        CreateAndAssignWallBlockAssets(wallBlocks);
        CreateAndAssignRoofBlockAssets(roofBlocks);
        
        buildingConfigCreationStep = 0;
        //should add some sort of msg telling the user that the creation was succsessful
    }

    public void Cities()
    {
        m_editorUtils.Panel("CitiesPanel", CitiesPanel, true);
    }

    public void CitiesPanel(bool helpActive)
    {

    }

    public void Dungeons()
    {
        m_editorUtils.Panel("DungeonsPanel", DungeonsPanel, true);
    }

    public void DungeonsPanel(bool helpActive)
    {

    }

    private void PropField(string key, SerializedProperty prop, bool helpActive)
    {
        EditorGUI.showMixedValue = prop.hasMultipleDifferentValues;
        m_editorUtils.PropertyField(key, true, prop, helpActive);
        EditorGUI.showMixedValue = false;
    }

    /// <summary>
    /// Creates and assigns BuildingBlock assets for all fields in WallBlocks
    /// </summary>
    /// <param name="wallBlocks">The WallBlocks instance to populate</param>
    private void CreateAndAssignWallBlockAssets(WallBlocks wallBlocks)
    {
        // Create BuildingBlock assets for all fields in WallBlocks
        wallBlocks.foundation = CreateBuildingBlockAsset("Foundation");
        wallBlocks.frontStep = CreateBuildingBlockAsset("Front Step");
        wallBlocks.floor = CreateBuildingBlockAsset("Floor");
        wallBlocks.exteriorWall = CreateBuildingBlockAsset("Exterior Wall");
        wallBlocks.exteriorWallTrim = CreateBuildingBlockAsset("Exterior Wall Trim");
        wallBlocks.alternateExteriorWall = CreateBuildingBlockAsset("Alternate Exterior Wall");
        wallBlocks.alternateExteriorWallTrim = CreateBuildingBlockAsset("Alternate Exterior Wall Trim");
        wallBlocks.interiorWall = CreateBuildingBlockAsset("Interior Wall");
        wallBlocks.interiorWallTrim = CreateBuildingBlockAsset("Interior Wall Trim");
        wallBlocks.ceiling = CreateBuildingBlockAsset("Ceiling");
        wallBlocks.railing = CreateBuildingBlockAsset("Railing");
        wallBlocks.supportPillar = CreateBuildingBlockAsset("Support Pillar");
        wallBlocks.alternateWindow = CreateBuildingBlockAsset("Alternate Window");
        wallBlocks.window = CreateBuildingBlockAsset("Window");
        wallBlocks.interiorDoor = CreateBuildingBlockAsset("Interior Door");
        wallBlocks.exteriorDoor = CreateBuildingBlockAsset("Exterior Door");
        wallBlocks.nothing = CreateBuildingBlockAsset("Nothing");
        wallBlocks.staircase = CreateBuildingBlockAsset("Staircase");
    }

    /// <summary>
    /// Creates and assigns BuildingBlock assets for all fields in RoofBlocks
    /// </summary>
    /// <param name="roofBlocks">The RoofBlocks instance to populate</param>
    private void CreateAndAssignRoofBlockAssets(RoofBlocks roofBlocks)
    {
        // Create BuildingBlock assets for all fields in RoofBlocks
        roofBlocks.roofCorner = CreateBuildingBlockAsset("Roof Corner");
        roofBlocks.roofSegment = CreateBuildingBlockAsset("Roof Segment");
        roofBlocks.roofTop = CreateBuildingBlockAsset("Roof Top");
        roofBlocks.roofOuterConnection = CreateBuildingBlockAsset("Roof Outer Connection");
        roofBlocks.roofInnerCorner = CreateBuildingBlockAsset("Roof Inner Corner");
        roofBlocks.smallPeak = CreateBuildingBlockAsset("Small Peak");
        roofBlocks.roofPeakJoin = CreateBuildingBlockAsset("Roof Peak Join");
        roofBlocks.roofSlantEdgeTrim = CreateBuildingBlockAsset("Roof Slant Edge Trim");
        roofBlocks.roofEdgeTrim = CreateBuildingBlockAsset("Roof Edge Trim");
        roofBlocks.roofPeakTrim = CreateBuildingBlockAsset("Roof Peak Trim");
        roofBlocks.roofCornerToSmallPeakBlendLeft = CreateBuildingBlockAsset("Roof Corner To Small Peak Blend Left");
        roofBlocks.roofCornerToSmallPeakBlendRight = CreateBuildingBlockAsset("Roof Corner To Small Peak Blend Right");
        roofBlocks.roofStraightToSmallPeakBlend = CreateBuildingBlockAsset("Roof Straight To Small Peak Blend");
        roofBlocks.roofSmallCorner = CreateBuildingBlockAsset("Roof Small Corner");
    }

    /// <summary>
    /// Creates a new BuildingBlock asset with the specified name
    /// </summary>
    /// <param name="name">Name for the BuildingBlock</param>
    /// <returns>The created BuildingBlock instance</returns>
    private BuildingBlock CreateBuildingBlockAsset(string name)
    {
        BuildingBlock buildingBlock = CreateInstance<BuildingBlock>();
        buildingBlock.name = name;
        buildingBlock.blockObjList = new List<BuildingBlockData>();
        buildingBlock.allowedMaterialSets = new List<MaterialSet>();
        return buildingBlock;
    }
    
    /// <summary>
    /// Saves all BuildingBlock assets from WallBlocks to the specified directory
    /// </summary>
    /// <param name="wallBlocks">The WallBlocks instance containing BuildingBlock assets</param>
    /// <param name="directory">The directory to save the assets to</param>
    private void SaveWallBlockAssets(WallBlocks wallBlocks, string directory)
    {
        // Save each BuildingBlock asset if it exists
        SaveBuildingBlockIfExists(wallBlocks.foundation, directory, "Foundation");
        SaveBuildingBlockIfExists(wallBlocks.frontStep, directory, "Front_Step");
        SaveBuildingBlockIfExists(wallBlocks.floor, directory, "Floor");
        SaveBuildingBlockIfExists(wallBlocks.exteriorWall, directory, "Exterior_Wall");
        SaveBuildingBlockIfExists(wallBlocks.exteriorWallTrim, directory, "Exterior_Wall_Trim");
        SaveBuildingBlockIfExists(wallBlocks.alternateExteriorWall, directory, "Alternate_Exterior_Wall");
        SaveBuildingBlockIfExists(wallBlocks.alternateExteriorWallTrim, directory, "Alternate_Exterior_Wall_Trim");
        SaveBuildingBlockIfExists(wallBlocks.interiorWall, directory, "Interior_Wall");
        SaveBuildingBlockIfExists(wallBlocks.interiorWallTrim, directory, "Interior_Wall_Trim");
        SaveBuildingBlockIfExists(wallBlocks.ceiling, directory, "Ceiling");
        SaveBuildingBlockIfExists(wallBlocks.railing, directory, "Railing");
        SaveBuildingBlockIfExists(wallBlocks.supportPillar, directory, "Support_Pillar");
        SaveBuildingBlockIfExists(wallBlocks.alternateWindow, directory, "Alternate_Window");
        SaveBuildingBlockIfExists(wallBlocks.window, directory, "Window");
        SaveBuildingBlockIfExists(wallBlocks.interiorDoor, directory, "Interior_Door");
        SaveBuildingBlockIfExists(wallBlocks.exteriorDoor, directory, "Exterior_Door");
        SaveBuildingBlockIfExists(wallBlocks.nothing, directory, "Nothing");
        SaveBuildingBlockIfExists(wallBlocks.staircase, directory, "Staircase");
    }
    
    /// <summary>
    /// Saves all BuildingBlock assets from RoofBlocks to the specified directory
    /// </summary>
    /// <param name="roofBlocks">The RoofBlocks instance containing BuildingBlock assets</param>
    /// <param name="directory">The directory to save the assets to</param>
    private void SaveRoofBlockAssets(RoofBlocks roofBlocks, string directory)
    {
        // Save each BuildingBlock asset if it exists
        SaveBuildingBlockIfExists(roofBlocks.roofCorner, directory, "Roof_Corner");
        SaveBuildingBlockIfExists(roofBlocks.roofSegment, directory, "Roof_Segment");
        SaveBuildingBlockIfExists(roofBlocks.roofTop, directory, "Roof_Top");
        SaveBuildingBlockIfExists(roofBlocks.roofOuterConnection, directory, "Roof_Outer_Connection");
        SaveBuildingBlockIfExists(roofBlocks.roofInnerCorner, directory, "Roof_Inner_Corner");
        SaveBuildingBlockIfExists(roofBlocks.smallPeak, directory, "Small_Peak");
        SaveBuildingBlockIfExists(roofBlocks.roofPeakJoin, directory, "Roof_Peak_Join");
        SaveBuildingBlockIfExists(roofBlocks.roofSlantEdgeTrim, directory, "Roof_Slant_Edge_Trim");
        SaveBuildingBlockIfExists(roofBlocks.roofEdgeTrim, directory, "Roof_Edge_Trim");
        SaveBuildingBlockIfExists(roofBlocks.roofPeakTrim, directory, "Roof_Peak_Trim");
        SaveBuildingBlockIfExists(roofBlocks.roofCornerToSmallPeakBlendLeft, directory, "Roof_Corner_To_Small_Peak_Blend_Left");
        SaveBuildingBlockIfExists(roofBlocks.roofCornerToSmallPeakBlendRight, directory, "Roof_Corner_To_Small_Peak_Blend_Right");
        SaveBuildingBlockIfExists(roofBlocks.roofStraightToSmallPeakBlend, directory, "Roof_Straight_To_Small_Peak_Blend");
        SaveBuildingBlockIfExists(roofBlocks.roofSmallCorner, directory, "Roof_Small_Corner");
    }
    
    /// <summary>
    /// Saves a BuildingBlock asset to the specified directory if it exists
    /// </summary>
    /// <param name="buildingBlock">The BuildingBlock asset to save</param>
    /// <param name="directory">The directory to save the asset to</param>
    /// <param name="fileName">The file name for the asset</param>
    private void SaveBuildingBlockIfExists(BuildingBlock buildingBlock, string directory, string fileName)
    {
        if (buildingBlock != null)
        {
            string assetPath = directory + fileName + ".asset";
            AssetDatabase.CreateAsset(buildingBlock, assetPath);
            EditorUtility.SetDirty(buildingBlock);
        }
    }

}

[System.Serializable]
public class SimpleRoomData
{
    public string roomName;
    public int priority;
    public Vector2Int desiredRoomSize;
    public int desiredFloors;
    public bool useDesiredStartFloor;
    public int desiredStartFloor;
    public bool capRoomCount;
    public int maxRoomCount;
    public bool allowShifting;
    [SerializeField] public List<RoomStoreyBehaviour> roomStoreyBehaviors;
    public int maxConnections;
    public RoomConnectionType connectionType;
    public bool connectToStaircase;
    public List<string> desiredConnections;
    public List<string> undesiredConnections;

    // Constructor with defaults
    public SimpleRoomData()
    {
        roomName = "New Room";
        priority = 0;
        desiredRoomSize = new Vector2Int(2, 2);
        desiredFloors = 1;
        useDesiredStartFloor = false;
        desiredStartFloor = 0;
        capRoomCount = true;
        maxRoomCount = 4;
        allowShifting = true;
        roomStoreyBehaviors = new List<RoomStoreyBehaviour>();
        maxConnections = 3;
        connectToStaircase = false;
        desiredConnections = new List<string>();
        undesiredConnections = new List<string>();
    }
}
