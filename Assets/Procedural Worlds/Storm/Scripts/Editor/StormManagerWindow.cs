using System.Collections.Generic;
using System.IO;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditorInternal;
using UnityEditor.SceneManagement;
using PWCommon5;
using ProceduralWorlds.Storm.Core;
using ProceduralWorlds.Storm.Utils;
using ProceduralWorlds.Storm.Wizard;
using System;
using UnityEditor.AnimatedValues;
using System.Collections;

/// Copyright 2023 Procedural Worlds Ltd
/// Written by Jonathan Williams and Josh Savage as part of Storm

namespace ProceduralWorlds.Storm.StormEditor
{
    public class StormManagerWindow : EditorWindow, IPWEditor
    {
        class PluginEditorData : IDisposable
        {
            readonly StormManagerWindow m_parent;
            readonly Type m_windowType;
            StormPluginEditorBase m_editor;
            public StormPluginEditorBase Editor
            {
                get {
                    if (m_editor == null)
                        CreateInstance();
                    return m_editor;
                }
            }
            public AnimBool m_isExpanded;
            public bool IsWindowOpened => Editor.IsWindowOpened(m_windowType);

            public PluginEditorData(Type windowType, StormManagerWindow parent)
            {
                m_parent = parent;
                m_windowType = windowType;
                CreateInstance();
                m_isExpanded = new AnimBool(false, parent.Repaint);
            }
            void CreateInstance()
            {
                m_editor = EditorWindow.CreateInstance(m_windowType) as StormPluginEditorBase;
                m_editor.IsEmbedded = true;
                m_editor.OnOpen += OnOpened;
                m_editor.SetEmbeddedInstance(m_windowType, m_editor);
            }
            void OnOpened()
            {
                m_isExpanded.target = false;
                m_parent.Repaint();
            }
            public void Dispose()
            {
                if (m_editor != null)
                {
                    m_editor.Show();
                    m_editor.Close();
                    m_editor.SetEmbeddedInstance(m_windowType, null);
                }
                m_editor = null;
            }
        }

        #region Varibles

        private TabSet m_mainTabs;
        private TabSet m_worldTabs;
        private Type[] m_plugins = null;
        private PluginEditorData[] m_pluginEditors;
        private EditorUtils m_editorUtils;
        private Vector2 m_scrollPosition = Vector2.zero;
        private GUIStyle m_boxStyle,m_linkStyle;

        public bool PositionChecked { get; set; }

        private StormWorldEditor m_worldEditor;

        private ReorderableList m_featuresetList;
        private List<StormFeatureset> m_featuresets;

        private ReorderableList m_dataTileList;
        private List<StormTile> m_dataTiles;
        private bool updateTiles = false;

        private bool m_sceneIngestFoldout = true;
        private ReorderableList m_ingestSceneList;
        private List<SceneAsset> m_ingestScenes;

        private ReorderableList m_loadTilesList;
        private List<string> m_loadTiles;

        private bool m_helpPanelHelpActive;

        bool showIngestGizmos = false;
        Bounds m_ingestBounds;
        Bounds[] m_ingestTileBounds = new Bounds[0];
        GameObject m_ingestObject = null;
        bool m_needsIngest = false;
        bool isIngesting = false;

        //Bake Variables
        bool showBakeGizmos = false;
        Bounds m_bakeBounds;
        Bounds[] m_bakeTileBounds = new Bounds[0];
        List<StormFeatureset> m_bakeFeaturesets = new List<StormFeatureset>();
        StormSourceLayerInfo[] m_bakeHeightLayers = new StormSourceLayerInfo[0];
        bool m_bakeFeaturesetsFoldout = true;
        ReorderableList m_bakeFeaturesetsList = null;
        Transform m_bakeTarget;
        bool m_needsBake = false;
        bool m_isBaking = false;
        StormInternalUtils.BakeProgress m_bakeProgress;
        int m_bakeFeatureCount = 0;

        bool m_multiSceneBake = false;
        List<SceneAsset> m_bakeScenesList = new List<SceneAsset>();
        ReorderableList m_bakeScenes;
        bool m_bakeScenesFoldout = true;

        string[] m_bakeSaveTypeList = new string[]
        {
            "BakeSaveType_Single",
            "BakeSaveType_OneEach",
            "BakeSaveType_Area",
        };

        //Baking Progress Variables
        int m_bakeSceneProgress = 0;
        Scene m_bakingSaveScene;
        GameObject m_bakingGameObject;
        Scene m_bakingScene;
        string[] m_baking_curScenes;
        bool[] m_baking_curScenesActive;
        Scene[] m_baking_curOpenScenes;
        Scene m_baking_activeScene;
        int m_baking_activeSceneIdx;
        bool m_baking_createdWorld = false;
        bool m_bakeProgress_hasRetried = false;
        int m_bakeProgress_timeout;
        Dictionary<int2, Scene> m_bakingAreaScenes = new Dictionary<int2, Scene>();

        //Feature Creation
        private bool isAPrefab;
        private int isAPrefabErrorCode;

        //Gizmo variables
        bool g_first = true;
        int g_x1HandleID;
        int g_y1HandleID;
        int g_z1HandleID;
        int g_x2HandleID;
        int g_y2HandleID;
        int g_z2HandleID;
        int g_selDirection;
        Vector2 g_mouseLastPos;

        //Impostor variables
        int m_impostorMode = 0;
        GameObject m_impostorObjToUpdate = null;
        Mesh m_impostorMeshToUpdate = null;
        List<Material> m_impostorMatsToUpdate = new List<Material>();
        ReorderableList m_impostorMatsList = null;
        ImpostorType m_impostorType = ImpostorType.FIVE_SLICE;
        bool[] m_impostorListExpanded = new bool[0];
        bool[] m_impostorMatsExpanded = new bool[0];
        string[] m_impostorTypeKeys = new string[] {
            "ImpostorType_FLAT_X",
            "ImpostorType_FLAT_Y",
            "ImpostorType_FLAT_Z",
            "ImpostorType_FIVE_SLICE",
            "ImpostorType_NINE_SLICE",
//#if AMPLIFY_IMPOSTORS_PRESENT
//            "ImpostorType_STORM_OCTAHEDRAL_HALF",
//            "ImpostorType_STORM_OCTAHEDRAL",
//            "ImpostorType_AMPLIFY_SPHERICAL",
//            "ImpostorType_AMPLIFY_OCTAHEDRAL_HALF",
//            "ImpostorType_AMPLIFY_OCTAHEDRAL",
//#endif
        };

        //Features Variables
        bool[] m_featureListExpanded = new bool[0];
        bool[] m_featureMatsExpanded = new bool[0];

        bool m_impostorSettingsHelpSwitch = false;
        SerializedObject m_impostorSettingsObject = null;
        SerializedProperty m_impostorSettingsBillboardStrings;
        SerializedProperty m_impostorSettingsBakeShaders;
        SerializedProperty m_impostorSettingsPostProcess;
        SerializedProperty m_impostorSettingsFlatReplacement;

        ReorderableList m_impostorSettingsBillboardStringsList = null;
        ReorderableList m_impostorSettingsBakeShadersList = null;
        ReorderableList m_impostorSettingsPostProcessList = null;

        StormScriptableUI<StormShaderData> m_shaderDataUI;
        #endregion
        #region Menu Items

        [MenuItem("Window/" + PWConst.COMMON_MENU + "/Storm/Storm Manager...", false, 10)]
        public static void ShowManager()
        {
            var window = GetWindow<StormManagerWindow>(false, "Storm Manager");
            Vector2 initialSize = new Vector2(600f, 450f);
            window.position = new Rect(new Vector2((Screen.currentResolution.width - initialSize.x) / 2f, (Screen.currentResolution.height - initialSize.y) / 2f), initialSize);
            window.Show();
        }

        bool hasInitIngest = false;
        bool hasInitBake = false;

        SerializedObject StormSettingsObject;
        SerializedProperty p_IngestIgnoredPrefabs;
        SerializedProperty p_IngestIgnoredLayers;
        SerializedProperty p_IngestIgnoredTags;
        SerializedProperty p_IngestStatic;
        SerializedProperty p_IngestNonStatic;

        SerializedProperty p_MeshTerrainResolution;
        SerializedProperty p_MeshTerrainTags;
        SerializedProperty p_DefaultColorTargetData;
        SerializedProperty p_ColorTargetRange;
        SerializedProperty p_ColorTargetFalloff;

        SerializedObject StormEditorSettingsObject;
        SerializedProperty p_CreateFeatureLayer;
        StormFeatureSettings m_createFeatureSettingsRef;

        [MenuItem("GameObject/Procedural Worlds/Storm/Data Tile", priority = 10)]
        public static void CreateDataTile()
        {
            GameObject go = new GameObject("Storm Data Tile");
            StormTile tile = go.AddComponent<StormTile>();
            Selection.activeGameObject = go;
            EditorGUIUtility.PingObject(tile);
        }
        [MenuItem("GameObject/Procedural Worlds/Storm/Storm World", priority = 10)]
        public static void CreateStormWorld()
        {
            StormWorld world;
            if (StormWorld.Instance != null)
                world = StormWorld.Instance;
            else
#if PW_STORM_PRESENT
                world = StormWizardUtils.CreateStormWorld();
            Selection.activeGameObject = world.gameObject;
            EditorGUIUtility.PingObject(world);
#else
                StormWizardUtils.CreateStormWorld();
#endif
        }
        [MenuItem("GameObject/Procedural Worlds/Storm/Runtime UI", priority = 10)]
        public static void CreateStormRuntimeUI()
        {
            StormEditorUtils.AddRuntimeUI();
        }
        [MenuItem("GameObject/Procedural Worlds/Storm/Polygon Mask", priority = 10)]
        public static void CreatePolygonMask()
        {
            GameObject go = Selection.activeGameObject;
            if (go == null)
            {
                go = new GameObject("Storm Polygon Mask");
            }

            StormPolygonMask polyMask = go.AddComponent<UI.PolygonMask>();
            
            EditorGUIUtility.PingObject(polyMask);
        }
        #endregion
        #region Unity Functions

        /// <summary>
        /// Called when the window opens
        /// </summary>
        private void OnEnable()
        {
            m_worldEditor = null;
            if (StormWorld.Instance != null)
                m_bakeHeightLayers = StormWorld.Instance.HeightNames.ToArray();
            StormSettingsObject = new SerializedObject(StormSettings.SystemSettings);
            p_IngestIgnoredPrefabs = StormSettingsObject.FindProperty("IgnoredPrefabs");
            p_IngestIgnoredLayers = StormSettingsObject.FindProperty("IgnoredLayers");
            p_IngestIgnoredTags = StormSettingsObject.FindProperty("IgnoredTags");
            p_IngestStatic = StormSettingsObject.FindProperty("IngestStatic");
            p_IngestNonStatic = StormSettingsObject.FindProperty("IngestNonStatic");
            p_MeshTerrainTags = StormSettingsObject.FindProperty("MeshTerrainTags");
            p_MeshTerrainResolution = StormSettingsObject.FindProperty("MeshTerrainResolution");
            p_DefaultColorTargetData = StormSettingsObject.FindProperty("DefaultColorTargetData");
            p_ColorTargetRange = StormSettingsObject.FindProperty("ColorTargetRange");
            p_ColorTargetFalloff = StormSettingsObject.FindProperty("ColorTargetFalloff");
            StormEditorSettingsObject = new SerializedObject(StormEditorSettings.Instance);
            p_CreateFeatureLayer = StormEditorSettingsObject.FindProperty("m_featureLayerName");
            m_isBaking = false;
            m_needsBake = false;
            m_bakeSceneProgress = 0;
            m_bakeProgress = null;
            StormUtils.DoDefineCheck();
            StormWorld.DrawGizmosCallback -= DrawGizmos;
            StormWorld.DrawGizmosCallback += DrawGizmos;
            SceneView.duringSceneGui -= OnSceneGUI;
            SceneView.duringSceneGui += OnSceneGUI;
            if (m_editorUtils == null)
            {
                // Get editor utils for this
                m_editorUtils = StormApp.GetEditorUtils(this);
            }
            Tab[] mainTabs = new Tab[]
            {
                new Tab ("StormWorldTab", StormWorldTab),
                new Tab ("StormPluginsTab", StormPluginsTab),
                new Tab ("HelpTab", HelpTab),
            };
            Tab[] worldTabs = new Tab[]
            {
                new Tab ("SetupTab", SetupTab),
                new Tab ("IngestionTab", IngestionTab),
                new Tab ("UtilsTab", UtilsTab),
            };
            m_mainTabs = new TabSet(m_editorUtils, mainTabs);
            m_worldTabs = new TabSet(m_editorUtils, worldTabs);
            m_scrollPosition = Vector2.zero;
            isIngesting = false;

            BuildReOrderableLists();
            UpdateDataTiles();

            

            m_impostorSettingsObject = new SerializedObject(StormSettings.SystemSettings);
            m_impostorSettingsBillboardStrings = m_impostorSettingsObject.FindProperty("m_billboardStrings");
            m_impostorSettingsBillboardStringsList = new ReorderableList(m_impostorSettingsObject, m_impostorSettingsBillboardStrings);
            m_impostorSettingsBillboardStringsList.drawHeaderCallback += DrawBillboardStringsHeader;
            m_impostorSettingsBillboardStringsList.drawElementCallback += DrawBillboardStringsElement;

            StormRP pipeline = StormUtils.GetRenderPipline();
            m_impostorSettingsBakeShaders = pipeline switch {
                StormRP.HDRP => m_impostorSettingsObject.FindProperty("m_impostorBakeShadersHDRP"),
                StormRP.URP => m_impostorSettingsObject.FindProperty("m_impostorBakeShadersURP"),
                _ => m_impostorSettingsObject.FindProperty("m_impostorBakeShadersBuiltIn")
            };
            m_impostorSettingsBakeShadersList = new ReorderableList(m_impostorSettingsObject, m_impostorSettingsBakeShaders);
            m_impostorSettingsBakeShadersList.drawHeaderCallback += DrawBakeShadersHeader;
            m_impostorSettingsBakeShadersList.drawElementCallback += DrawBakeShadersElement;
            m_impostorSettingsBakeShadersList.elementHeightCallback += GetBakeShadersElementHeight;
            m_impostorSettingsBakeShadersList.drawNoneElementCallback += DrawBakeShadersNoElement;

            m_impostorSettingsPostProcess = pipeline switch {
                StormRP.HDRP => m_impostorSettingsObject.FindProperty("m_impostorPostProcessHDRP"),
                StormRP.URP => m_impostorSettingsObject.FindProperty("m_impostorPostProcessURP"),
                _ => m_impostorSettingsObject.FindProperty("m_impostorPostProcessBuiltIn")
            };
            m_impostorSettingsFlatReplacement = pipeline switch {
                StormRP.HDRP => m_impostorSettingsObject.FindProperty("m_impostorFlatReplacementHDRP"),
                StormRP.URP => m_impostorSettingsObject.FindProperty("m_impostorFlatReplacementURP"),
                _ => m_impostorSettingsObject.FindProperty("m_impostorFlatReplacementBuiltIn")
            };
            _ = StormSettings.ImpostorBakeShaders;
            _ = StormSettings.ImpostorPostProcess;
            _ = StormSettings.ImpostorFlatReplacement; //call get functions to check for empty/null

            m_impostorSettingsPostProcessList = new ReorderableList(m_impostorSettingsObject, m_impostorSettingsPostProcess);
            m_impostorSettingsPostProcessList.drawHeaderCallback += DrawPostProcessHeader;
            m_impostorSettingsPostProcessList.drawElementCallback += DrawPostProcessElement;
            m_impostorSettingsPostProcessList.elementHeightCallback += GetPostProcessElementHeight;
            m_impostorSettingsPostProcessList.drawNoneElementCallback += DrawPostProcessNoElement;

            CheckDefaultImpostorSettings();

            StormWorld stormWorld = StormWorld.Instance;
            if (stormWorld != null)
            {
                isAPrefab = IsCreationAPrefab(out isAPrefabErrorCode);
            }
            StormUtils.RemoveEditorOnRecompile(OnEnable);
            StormUtils.AddEditorOnRecompile(OnEnable);


            string path = StormShaderConverter.DefaultSaveDir + "shaderData.asset";
            StormShaderData shaderData = AssetDatabase.LoadAssetAtPath<StormShaderData>(path);
            if (shaderData == null)
            {
                shaderData = CreateInstance<StormShaderData>();
                StormUtils.EnsureDirExists(path);
                AssetDatabase.CreateAsset(shaderData, path);
            }
            m_shaderDataUI = new StormScriptableUI<StormShaderData>(shaderData, "StormStandardShaders", Repaint, m_editorUtils, null, false);
        }

        private void CheckDefaultImpostorSettings()
        {
            if (StormSettings.BillboardStrings == null || StormSettings.BillboardStrings.Length <= 0)
            {
                StormSettings.BillboardStrings = null;
                StormSettings.BillboardStrings = StormSettings.BillboardStrings;
                m_impostorSettingsObject.Update();
            }

            if (StormSettings.ImpostorBakeShaders == null || StormSettings.ImpostorBakeShaders.Length <= 0)
            {
                StormSettings.ImpostorBakeShaders = null;
                StormSettings.ImpostorBakeShaders = StormSettings.ImpostorBakeShaders;
                m_impostorSettingsObject.Update();
            }

            if (StormSettings.ImpostorPostProcess == null || StormSettings.ImpostorPostProcess.Length <= 0)
            {
                StormSettings.ImpostorPostProcess = null;
                StormSettings.ImpostorPostProcess = StormSettings.ImpostorPostProcess;
                m_impostorSettingsObject.Update();
            }
        }

        /// <summary>
        /// Called when the window is destroyed
        /// </summary>
        private void OnDestroy()
        {
            StormUtils.RemoveEditorOnRecompile(OnEnable);
            StormWorld.DrawGizmosCallback -= DrawGizmos;
            SceneView.duringSceneGui -= OnSceneGUI;
            if (m_editorUtils != null)
            {
                m_editorUtils.Dispose();
                m_editorUtils = null;
            }
            if (m_worldEditor != null)
            {
                DestroyImmediate(m_worldEditor);
                m_worldEditor = null;
            }
            if (m_pluginEditors != null)
            {
                foreach (PluginEditorData plugin in m_pluginEditors)
                {
                    if (plugin != null)
                        plugin.Dispose();
                }
            }
            m_pluginEditors = Array.Empty<PluginEditorData>();
        }

        private void OnFocus()
        {
            if (m_dataTiles != null)
                StormUtils.AddIfNew(m_dataTiles, StormUtils.FindObjectsOfType<StormTile>());
        }
        /// <summary>
        /// Callback function used to be called onDrawGizmos
        /// </summary>
        private void DrawGizmos()
        {
            if (showIngestGizmos)
            {
                Color startColor = Gizmos.color;
                Gizmos.color = Color.magenta;

                if (m_ingestTileBounds.Length < 150)
                    for (int i = 0; i < m_ingestTileBounds.Length; ++i)
                        Gizmos.DrawWireCube(m_ingestTileBounds[i].center, m_ingestTileBounds[i].size);

                Gizmos.color = startColor;
            }
            if (showBakeGizmos)
            {
                Color startColor = Gizmos.color;
                Gizmos.color = Color.magenta;

                for (int i = 0; i < m_bakeTileBounds.Length; ++i)
                    Gizmos.DrawWireCube(m_bakeTileBounds[i].center, m_bakeTileBounds[i].size);

                Gizmos.color = startColor;
            }
        }
        /// <summary>
        /// Called when OnSceneGUI is called
        /// </summary>
        /// <param name="view"></param>
        public void OnSceneGUI(SceneView view)
        {
            if (g_first)
            {
                g_x1HandleID = GUIUtility.GetControlID(FocusType.Passive);
                g_y1HandleID = GUIUtility.GetControlID(FocusType.Passive);
                g_z1HandleID = GUIUtility.GetControlID(FocusType.Passive);
                g_x2HandleID = GUIUtility.GetControlID(FocusType.Passive);
                g_y2HandleID = GUIUtility.GetControlID(FocusType.Passive);
                g_z2HandleID = GUIUtility.GetControlID(FocusType.Passive);
                g_selDirection = -1;
                g_first = false;
            }
            if (showIngestGizmos && StormEditorSettings.Instance.IngestManualBounds)
            {
                Event curEvent = Event.current;
                if (curEvent.type is EventType.Repaint or EventType.Layout)
                {
                    Vector3 min = m_ingestBounds.min;
                    Vector3 max = m_ingestBounds.max;
                    Vector3 center = m_ingestBounds.center;
                    Vector3 size = m_ingestBounds.size;

                    Handles.color = Handles.xAxisColor;
                    Vector3 pos = new Vector3(min.x, center.y, center.z);
                    Handles.ArrowHandleCap(g_x1HandleID, pos, Quaternion.Euler(0, -90, 0), HandleUtility.GetHandleSize(pos), curEvent.type);
                    pos = new Vector3(max.x, center.y, center.z);
                    Handles.ArrowHandleCap(g_x2HandleID, pos, Quaternion.Euler(0, 90, 0), HandleUtility.GetHandleSize(pos), curEvent.type);

                    Handles.color = Handles.yAxisColor;
                    pos = new Vector3(center.x, min.y, center.z);
                    Handles.ArrowHandleCap(g_y1HandleID, pos, Quaternion.Euler(90, 0, 0), HandleUtility.GetHandleSize(pos), curEvent.type);
                    pos = new Vector3(center.x, max.y, center.z);
                    Handles.ArrowHandleCap(g_y2HandleID, pos, Quaternion.Euler(-90, 0, 0), HandleUtility.GetHandleSize(pos), curEvent.type);

                    Handles.color = Handles.zAxisColor;
                    pos = new Vector3(center.x, center.y, min.z);
                    Handles.ArrowHandleCap(g_z1HandleID, pos, Quaternion.Euler(180, 0, 0), HandleUtility.GetHandleSize(pos), curEvent.type);
                    pos = new Vector3(center.x, center.y, max.z);
                    Handles.ArrowHandleCap(g_z2HandleID, pos, Quaternion.Euler(0, 0, 0), HandleUtility.GetHandleSize(pos), curEvent.type);
                }
                else if (curEvent.type is EventType.MouseDown)
                {
                    int selID = HandleUtility.nearestControl;
                    if (selID == g_x1HandleID)
                        g_selDirection = 0;
                    else if (selID == g_y1HandleID)
                        g_selDirection = 1;
                    else if (selID == g_z1HandleID)
                        g_selDirection = 2;
                    else if (selID == g_x2HandleID)
                        g_selDirection = 3;
                    else if (selID == g_y2HandleID)
                        g_selDirection = 4;
                    else if (selID == g_z2HandleID)
                        g_selDirection = 5;
                    else
                        g_selDirection = -1;
                    g_mouseLastPos = curEvent.mousePosition;
                }
                else if (curEvent.type is EventType.MouseDrag && g_selDirection >= 0)
                {
                    Vector3 min = m_ingestBounds.min;
                    Vector3 max = m_ingestBounds.max;
                    Vector3 center = m_ingestBounds.center;

                    Vector3 dir;
                    Vector3 pos;
                    switch (g_selDirection)
                    {
                        default:
                            pos = new Vector3(min.x, center.y, center.z);
                            dir = Vector3.left;
                            break;
                        case 3:
                            pos = new Vector3(max.x, center.y, center.z);
                            dir = Vector3.left;
                            break;
                        case 1:
                            pos = new Vector3(center.x, min.y, center.z);
                            dir = Vector3.up;
                            break;
                        case 4:
                            pos = new Vector3(center.x, max.y, center.z);
                            dir = Vector3.up;
                            break;
                        case 2:
                            pos = new Vector3(center.x, center.y, min.z);
                            dir = Vector3.forward;
                            break;
                        case 5:
                            pos = new Vector3(center.x, center.y, max.z);
                            dir = Vector3.forward;
                            break;
                    }

                    float move = HandleUtility.CalcLineTranslation(g_mouseLastPos, curEvent.mousePosition, pos, dir);
                    switch (g_selDirection)
                    {
                        default:
                            min.x -= move;
                            break;
                        case 3:
                            max.x -= move;
                            break;
                        case 1:
                            min.y += move;
                            break;
                        case 4:
                            max.y += move;
                            break;
                        case 2:
                            min.z += move;
                            break;
                        case 5:
                            max.z += move;
                            break;
                    }
                    m_ingestBounds = new Bounds((min * 0.5f) + (max * 0.5f), max - min);
                    StormEditorSettings.Instance.IngestBounds = m_ingestBounds;
                    m_ingestTileBounds = CalcTiles(m_ingestBounds, m_dataTiles);
                    g_mouseLastPos = curEvent.mousePosition;
                }
                else if (curEvent.type is EventType.MouseUp)
                {
                    g_selDirection = -1;
                }
            }
        }
        /// <summary>
        /// Called every frame when the window is open
        /// </summary>
        public void OnGUI()
        {
            //Set up the box style
            if (m_boxStyle == null)
            {
                m_boxStyle = new GUIStyle(GUI.skin.box);
                m_boxStyle.normal.textColor = GUI.skin.label.normal.textColor;
                m_boxStyle.fontStyle = FontStyle.Bold;
                m_boxStyle.alignment = TextAnchor.UpperLeft;
            }

            //set up a link style for the PDF links on the help tab
            if (m_linkStyle == null || StormUtils.IsBlack(m_linkStyle.normal.textColor))
            {
                m_linkStyle = new GUIStyle(GUI.skin.label);
                m_linkStyle.fontStyle = FontStyle.Normal;
                m_linkStyle.wordWrap = false;
                m_linkStyle.normal.textColor = new Color(0x00 / 255f, 0x78 / 255f, 0xDA / 255f, 1f);
                m_linkStyle.stretchWidth = false;
            }



            showIngestGizmos = false;
            m_editorUtils.Initialize(); // Do not remove this!
            m_editorUtils.Styles.tabSelected.fontStyle = FontStyle.Bold;
            m_editorUtils.GUIHeader();

            m_scrollPosition = EditorGUILayout.BeginScrollView(m_scrollPosition);
            EditorGUI.BeginDisabledGroup(isIngesting);
            m_editorUtils.Tabs(m_mainTabs);
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndScrollView();
            m_editorUtils.GUINewsFooter();
            if (m_needsIngest)
            {
                m_needsIngest = false;
                isIngesting = true;
                IngestionCoro = DoIngest();
            }
            if (isIngesting)
            {
                UpdateCoro(IngestionCoro);
                Repaint();
            }
            UpdateBake();
            StormSettingsObject.ApplyModifiedProperties();
        }
        #endregion
        #region Reorderable Lists
        bool UpdateCoro(IEnumerator coro)
        {
            if (coro.Current is IEnumerator subCoro)
                if (UpdateCoro(subCoro))
                    return true;
            return coro.MoveNext();
        }
        IEnumerator IngestionCoro;

        private void DrawFeaturesetListHeader(Rect rect)
        {
            Event curEvent = Event.current;
            if (curEvent.type == EventType.DragUpdated && rect.Contains(curEvent.mousePosition))
            {
                for (int o = 0; o < DragAndDrop.objectReferences.Length; ++o)
                {
                    if (DragAndDrop.objectReferences[o] is StormFeatureset)
                    {
                        DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                        curEvent.Use();
                        break;
                    }
                }
            }
            else if (curEvent.type == EventType.DragPerform && rect.Contains(curEvent.mousePosition))
            {
                bool added = false;
                DragAndDrop.AcceptDrag();
                curEvent.Use();
                for (int o = 0; o < DragAndDrop.objectReferences.Length; ++o)
                {
                    if (DragAndDrop.objectReferences[o] is StormFeatureset fs)
                    {
                        StormUtils.AddIfNew(m_featuresets, fs);
                        added = true;
                    }
                }
                if (added)
                {
                    StormWorld stormWorld = StormWorld.Instance;
                    if (stormWorld != null)
                        stormWorld.UpdateFeaturesets();
                }
            }
            ++EditorGUI.indentLevel;
            EditorGUI.LabelField(rect, m_editorUtils.GetContent("FeaturesetList"));
            --EditorGUI.indentLevel;
        }
        private void DrawFeaturesetElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            EditorGUI.BeginChangeCheck();
            m_featuresets[index] = EditorGUI.ObjectField(rect, m_featuresets[index], typeof(StormFeatureset), false) as StormFeatureset;
            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(StormWorld.Instance);
                StormWorld stormWorld = StormWorld.Instance;
                if (stormWorld != null)
                    stormWorld.UpdateFeaturesets();
            }
        }
        private void AddFeaturesetElement(ReorderableList list)
        {
            StormWorld stormWorld = StormWorld.Instance;
            if (stormWorld != null)
            {
                Undo.RecordObject(stormWorld, "Reorder List Changed");
                StormWorld.Instance.UpdateFeaturesets();
            }
            list.list.Add(null);
        }

        private void DrawDataTileListHeader(Rect rect)
        {
            ++EditorGUI.indentLevel;
            EditorGUI.LabelField(rect, m_editorUtils.GetContent("GeneratedTiles"));
            --EditorGUI.indentLevel;
        }
        private void DrawDataTileElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            EditorGUI.ObjectField(rect, m_dataTiles[index], typeof(StormTile), false);
        }
        private void AddDataTile(ReorderableList list)
        {
            StormWorld stormWorld = StormWorld.Instance;
            if (stormWorld != null)
            {
                Undo.RecordObject(stormWorld, "Reorder List Changed");
            }
            GameObject go = new GameObject("Storm Data Tile", new Type[] { typeof(StormTile) });
            go.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
            go.transform.localScale = Vector3.one;
            Selection.activeObject = go;
            Undo.RegisterCreatedObjectUndo(go, "Created Data Tile");
            m_dataTiles.Add(go.GetComponent<StormTile>());
            EditorGUIUtility.PingObject(go);
            Selection.activeGameObject = go;
        }
        private void RemoveDataTile(ReorderableList list)
        {
            StormWorld stormWorld = StormWorld.Instance;
            if (stormWorld != null)
            {
                Undo.RecordObject(stormWorld, "Reorder List Changed");
            }
            var selected = list.selectedIndices;
            for (int i = 0; i < selected.Count; ++i)
            {
                StormTile tile = m_dataTiles[selected[i]];
                GameObject go = tile.gameObject;
                if (go.GetComponents<Component>().Length > 2)
                {
                    if (Application.isPlaying)
                        Destroy(tile);
                    else
                        Undo.DestroyObjectImmediate(tile);
                }
                else
                {
                    if (Application.isPlaying)
                        Destroy(go);
                    else
                        Undo.DestroyObjectImmediate(go);
                }
            }
            updateTiles = true;
        }

        private void DrawSceneListHeader(Rect rect)
        {
            ++EditorGUI.indentLevel;
            EditorGUI.LabelField(rect, m_editorUtils.GetContent("IngestScene"));
            --EditorGUI.indentLevel;
        }
        private void DrawSceneElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            Event curEvent = Event.current;
            if (curEvent.type == EventType.DragUpdated && rect.Contains(curEvent.mousePosition))
            {
                for (int o = 0; o < DragAndDrop.objectReferences.Length; ++o)
                {
                    if (DragAndDrop.objectReferences[o] is SceneAsset)
                    {
                        DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                        curEvent.Use();
                        break;
                    }
                }
            }
            else if (curEvent.type == EventType.DragPerform && rect.Contains(curEvent.mousePosition))
            {
                DragAndDrop.AcceptDrag();
                curEvent.Use();
                for (int o = 0; o < DragAndDrop.objectReferences.Length; ++o)
                {
                    if (DragAndDrop.objectReferences[o] is SceneAsset sa)
                    {
                        StormUtils.AddIfNew(m_ingestScenes, sa);
                    }
                }
            }
            m_ingestScenes[index] = EditorGUI.ObjectField(rect, m_ingestScenes[index], typeof(SceneAsset), false) as SceneAsset;
        }
        private void AddSceneElement(ReorderableList list)
        {
            StormWorld stormWorld = StormWorld.Instance;
            if (stormWorld != null)
            {
                Undo.RecordObject(stormWorld, "Reorder List Changed");
            }
            list.list.Add(null);
        }

        private void DrawLoadTileListHeader(Rect rect)
        {
            Event curEvent = Event.current;
            if (rect.Contains(curEvent.mousePosition))
            {
                if (curEvent.type == EventType.DragUpdated)
                {
                    int numPaths = DragAndDrop.paths.Length;
                    if (DragAndDrop.paths != null && numPaths > 0)
                    {
                        DragAndDrop.visualMode = DragAndDropVisualMode.Link;
                        Event.current.Use();
                    }
                }
                else if (curEvent.type == EventType.DragPerform)
                {
                    string[] paths = DragAndDrop.paths;
                    if (paths != null && paths.Length > 0)
                    {
                        DragAndDrop.AcceptDrag();
                        for (int i = 1; i < paths.Length; ++i)
                        {
                            string path = DragAndDrop.paths[i].Replace('\\', '/');
                            if (path.StartsWith(Application.dataPath))
                                path = path.Substring(Application.dataPath.Length - 6);
                            paths[i] = path;
                        }
                        StormUtils.AddIfNew(m_loadTiles, paths);
                        Event.current.Use();
                    }
                }
            }
            ++EditorGUI.indentLevel;
            EditorGUI.LabelField(rect, m_editorUtils.GetContent("LoadTileList"));
            --EditorGUI.indentLevel;
        }
        private void DrawLoadTileElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            Event curEvent = Event.current;
            if (rect.Contains(curEvent.mousePosition))
            {
                if (curEvent.type == EventType.DragUpdated)
                {
                    int numPaths = DragAndDrop.paths.Length;
                    if (DragAndDrop.paths != null && numPaths > 0)
                    {
                        DragAndDrop.visualMode = DragAndDropVisualMode.Link;
                        Event.current.Use();
                    }
                }
                else if (curEvent.type == EventType.DragPerform)
                {
                    string[] paths = DragAndDrop.paths;
                    if (paths != null && paths.Length > 0)
                    {
                        DragAndDrop.AcceptDrag();
                        m_loadTiles.RemoveAt(index);
                        for (int i = 1; i < paths.Length; ++i)
                        {
                            string path = DragAndDrop.paths[i].Replace('\\', '/');
                            if (path.StartsWith(Application.dataPath))
                                path = path.Substring(Application.dataPath.Length - 6);
                            paths[i] = path;
                            if (!m_loadTiles.Contains(path))
                                m_loadTiles.Insert(index++, path);
                        }
                        Event.current.Use();
                    }
                }
            }
            Rect r = rect;
            r.xMax -= 100;
            rect.xMin = r.xMax;
            m_loadTiles[index] = EditorGUI.TextField(r, m_loadTiles[index]);
            rect.width = 100f;
            if (GUI.Button(rect, m_editorUtils.GetContent("LoadTileBrowseButton")))
            {
                GUIContent dialog = m_editorUtils.GetContent("LoadTileDialog");
                string path = EditorUtility.OpenFilePanel(dialog.text, "", "bytes");
                if (!string.IsNullOrWhiteSpace(path))
                {
                    path = path.Replace('\\', '/');
                    if (path.StartsWith(Application.dataPath))
                        path = path.Substring(Application.dataPath.Length - 6);
                    m_loadTiles[index] = path;
                }
            }
        }
        private void AddLoadTileElement(ReorderableList list)
        {
            StormWorld stormWorld = StormWorld.Instance;
            if (stormWorld != null)
            {
                Undo.RecordObject(stormWorld, "Reorder List Changed");
            }
            list.list.Add("");
        }
        private void OnRemoveElement(ReorderableList list)
        {
            StormWorld stormWorld = StormWorld.Instance;
            if (stormWorld != null)
            {
                Undo.RecordObject(stormWorld, "Reorder List Changed");
            }
            list.list.RemoveAt(list.index);
        }


        private void DrawBillboardStringsHeader(Rect rect)
        {
            rect.xMin += 10;
            if (rect.height > EditorGUIUtility.singleLineHeight)
            {
                float dif = (rect.height - EditorGUIUtility.singleLineHeight) * 0.5f;
                rect.yMin += dif;
                rect.yMax -= dif;
            }
            rect.width -= 100;
            GUI.Label(rect, m_editorUtils.GetContent("ImpSettings_BillboardStrings"));
            rect.xMin += rect.width;
            rect.width = 100;
            if (GUI.Button(rect, m_editorUtils.GetContent("DefaultButton")))
            {
                StormSettings.BillboardStrings = null;
                StormSettings.BillboardStrings = StormSettings.BillboardStrings;
                m_impostorSettingsObject.Update();
            }
        }
        private void DrawBillboardStringsElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            if (rect.height > EditorGUIUtility.singleLineHeight)
            {
                float dif = (rect.height - EditorGUIUtility.singleLineHeight) * 0.5f;
                rect.yMin += dif;
                rect.yMax -= dif;
            }
            SerializedProperty prop = m_impostorSettingsBillboardStrings.GetArrayElementAtIndex(index);
            prop.stringValue = EditorGUI.TextField(rect, prop.stringValue);
        }

        private void DrawBakeShadersHeader(Rect rect)
        {
            rect.xMin += 10;
            if (rect.height > EditorGUIUtility.singleLineHeight)
            {
                float dif = (rect.height - EditorGUIUtility.singleLineHeight) * 0.5f;
                rect.yMin += dif;
                rect.yMax -= dif;
            }
            GUI.Label(rect, m_editorUtils.GetContent("ImpSettings_BakeShaders"));
            rect.xMin += rect.width - 100;
            rect.width = 100;
            if (GUI.Button(rect, m_editorUtils.GetContent("DefaultButton")))
            {
                StormSettings.ImpostorBakeShaders = null;
                StormSettings.ImpostorBakeShaders = StormSettings.ImpostorBakeShaders;
                m_impostorSettingsObject.Update();
            }
        }
        private void DrawBakeShadersElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            SerializedProperty prop = m_impostorSettingsBakeShaders.GetArrayElementAtIndex(index);
            SerializedProperty nameProp = prop.FindPropertyRelative("m_texName");
            string name;
            if (nameProp == null || string.IsNullOrEmpty(nameProp.stringValue))
                name = string.Format(m_editorUtils.GetContent("ImpSettings_BakeShadersElementDefault").text, index);
            else
                name = nameProp.stringValue;
            EditorGUI.PropertyField(rect, prop, new GUIContent(name), true);
        }
        private float GetBakeShadersElementHeight(int index)
        {
            return EditorGUI.GetPropertyHeight(m_impostorSettingsBakeShaders.GetArrayElementAtIndex(index), true);
        }
        private void DrawBakeShadersNoElement(Rect rect)
        {
            GUIContent button = m_editorUtils.GetContent("ImpSettings_UseDefaultsButton");
            Vector2 size = EditorStyles.miniButton.CalcSize(button);
            rect.xMax -= size.x;
            GUI.Label(rect, m_editorUtils.GetContent("ImpSettings_BakeShadersEmpty"), EditorStyles.boldLabel);
            rect.xMin = rect.xMax;
            rect.xMax = rect.xMin + size.x;
            if (GUI.Button(rect, button, EditorStyles.miniButton))
            {
                StormSettings.ImpostorBakeShaders = null;
                StormSettings.ImpostorBakeShaders = StormSettings.ImpostorBakeShaders;
                m_impostorSettingsObject.Update();
            }
        }

        private void DrawPostProcessHeader(Rect rect)
        {
            rect.xMin += 10;
            if (rect.height > EditorGUIUtility.singleLineHeight)
            {
                float dif = (rect.height - EditorGUIUtility.singleLineHeight) * 0.5f;
                rect.yMin += dif;
                rect.yMax -= dif;
            }
            GUI.Label(rect, m_editorUtils.GetContent("ImpSettings_PostProcess"));
            rect.xMin += rect.width - 100;
            rect.width = 100;
            if (GUI.Button(rect, m_editorUtils.GetContent("DefaultButton")))
            {
                StormSettings.ImpostorPostProcess = null;
                StormSettings.ImpostorPostProcess = StormSettings.ImpostorPostProcess;
                m_impostorSettingsObject.Update();
            }
        }
        private void DrawPostProcessElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            SerializedProperty prop = m_impostorSettingsPostProcess.GetArrayElementAtIndex(index);
            SerializedProperty nameProp = prop.FindPropertyRelative("Output");
            string name;
            if (nameProp == null || string.IsNullOrEmpty(nameProp.stringValue))
                name = string.Format(m_editorUtils.GetContent("ImpSettings_PostProcessElementDefault").text, index);
            else
                name = nameProp.stringValue;
            EditorGUI.PropertyField(rect, prop, new GUIContent(name), true);
        }
        private float GetPostProcessElementHeight(int index)
        {
            return EditorGUI.GetPropertyHeight(m_impostorSettingsPostProcess.GetArrayElementAtIndex(index), true);
        }
        private void DrawPostProcessNoElement(Rect rect)
        {
            GUIContent button = m_editorUtils.GetContent("ImpSettings_UseDefaultsButton");
            Vector2 size = EditorStyles.miniButton.CalcSize(button);
            rect.xMax -= size.x;
            GUI.Label(rect, m_editorUtils.GetContent("ImpSettings_PostProcessEmpty"), EditorStyles.boldLabel);
            rect.xMin = rect.xMax;
            rect.xMax = rect.xMin + size.x;
            if (GUI.Button(rect, button, EditorStyles.miniButton))
            {
                StormSettings.ImpostorPostProcess = StormSettings.ImpostorPostProcess;
                m_impostorSettingsObject.Update();
            }
        }

        private void DrawBakeFeaturesetsHeader(Rect rect)
        {
            rect.xMin += 10;
            if (rect.height > EditorGUIUtility.singleLineHeight)
            {
                float dif = (rect.height - EditorGUIUtility.singleLineHeight) * 0.5f;
                rect.yMin += dif;
                rect.yMax -= dif;
            }
            GUI.Label(rect, m_editorUtils.GetContent("BakeFeaturesets"));
        }
        private void DrawBakeFeaturesetsElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            m_bakeFeaturesets[index] = EditorGUI.ObjectField(rect, m_bakeFeaturesets[index], typeof(StormFeatureset), false) as StormFeatureset;
        }
        private void AddBakeFeaturesetsElement(ReorderableList list)
        {
            list.list.Add(null);
        }
        private void OnRemoveBakeFeaturesetsElement(ReorderableList list)
        {
            list.list.RemoveAt(list.index);
        }

        private void DrawBakeScenesHeader(Rect rect)
        {
            rect.xMin += 10;
            if (rect.height > EditorGUIUtility.singleLineHeight)
            {
                float dif = (rect.height - EditorGUIUtility.singleLineHeight) * 0.5f;
                rect.yMin += dif;
                rect.yMax -= dif;
            }
            GUI.Label(rect, m_editorUtils.GetContent("BakeScenes"));
        }
        private void DrawBakeScenesElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            m_bakeScenesList[index] = EditorGUI.ObjectField(rect, m_bakeScenesList[index], typeof(SceneAsset), false) as SceneAsset;
        }
        #endregion
        #region Panels
        private void StormWorldTab()
        {
            m_editorUtils.Tabs(m_worldTabs);
        }
        private void StormPluginsTab()
        {
            if (m_plugins == null)
            {
                m_plugins = StormEditorUtils.GetAllPlugins();
                m_pluginEditors = new PluginEditorData[m_plugins.Length];
                for (int p = 0; p < m_plugins.Length; ++p)
                {
                    m_pluginEditors[p] = new PluginEditorData(m_plugins[p], this);
                }
            }
            if (m_plugins.Length == 0)
            {
                m_editorUtils.LabelField("NoPluginsMessage", GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
                return;
            }
            //m_pluginIndex = m_editorUtils.TabBar(m_pluginIndex, m_pluginLabels, m_editorUtils.Styles.tab, m_editorUtils.Styles.tabSelected, GUILayout.ExpandWidth(true));
            for (int e = 0; e < m_pluginEditors.Length; ++e)
                PluginPanel(m_pluginEditors[e]);
        }
        private void HelpTab()
        {


            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            m_editorUtils.HelpToggle(ref m_helpPanelHelpActive);
            EditorGUILayout.EndHorizontal();
            m_editorUtils.InlineHelp("HelpTabDemoHelp", m_helpPanelHelpActive);
            m_editorUtils.Text("HelpMessage", GUILayout.Height(50));
            EditorGUILayout.Space();

            m_editorUtils.Heading("HeadingStormCoreDoc");

            if (StormUtils.ClickableHeaderCustomStyle(m_editorUtils.GetContent("StormReadMeLink"), m_linkStyle))
            {
                StormUtils.OpenFileInBrowser("Storm - Read Me First.pdf");
            }

            if (StormUtils.ClickableHeaderCustomStyle(m_editorUtils.GetContent("StormReleaseNotesLink"), m_linkStyle))
            {
                StormUtils.OpenFileInBrowser("Storm - Release Notes.pdf");
            }

            if (StormUtils.ClickableHeaderCustomStyle(m_editorUtils.GetContent("StormQuickstartLink"), m_linkStyle))
            {
                StormUtils.OpenFileInBrowser("Storm - Quick Start Guide.pdf");
            }

            if (StormUtils.ClickableHeaderCustomStyle(m_editorUtils.GetContent("StormUserGuideLink"), m_linkStyle))
            {
                StormUtils.OpenFileInBrowser("Storm - User Guide.pdf");
            }
            EditorGUILayout.Space();
            EditorGUILayout.Space();

            m_editorUtils.Heading("HeadingStormPluginDoc");

            if (StormUtils.ClickableHeaderCustomStyle(m_editorUtils.GetContent("StormAreaPopulatorLink"), m_linkStyle))
            {
                StormUtils.OpenFileInBrowser("Storm Plugin - Area Populator - User Guide.pdf");
            }

            if (StormUtils.ClickableHeaderCustomStyle(m_editorUtils.GetContent("StormBuildingGeneratorLink"), m_linkStyle))
            {
                StormUtils.OpenFileInBrowser("Storm Plugin - Building Generator - User Guide.pdf");
            }

            if (StormUtils.ClickableHeaderCustomStyle(m_editorUtils.GetContent("StormCityGeneratorLink"), m_linkStyle))
            {
                StormUtils.OpenFileInBrowser("Storm Plugin - City Generator - User Guide.pdf");
            }

            if (StormUtils.ClickableHeaderCustomStyle(m_editorUtils.GetContent("StormDungeonGeneratorLink"), m_linkStyle))
            {
                StormUtils.OpenFileInBrowser("Storm Plugin - Dungeon Generator - User Guide.pdf");
            }

            if (StormUtils.ClickableHeaderCustomStyle(m_editorUtils.GetContent("StormTerrainQuickstartLink"), m_linkStyle))
            {
                StormUtils.OpenFileInBrowser("Storm Plugin - Terrain - Quick Start Guide.pdf");
            }

            if (StormUtils.ClickableHeaderCustomStyle(m_editorUtils.GetContent("StormTerrainUserGuideLink"), m_linkStyle))
            {
                StormUtils.OpenFileInBrowser("Storm Plugin - Terrain - User Guide.pdf");
            }

        }


        private void SetupTab()
        {
            m_editorUtils.Panel("World", WorldPanel, true);
            StormWorld world = StormWorld.Instance;
            if (world != null)
            {
                if (m_worldEditor == null || m_worldEditor.target != world)
                {
                    if (m_worldEditor != null)
                        DestroyImmediate(m_worldEditor);
                    m_worldEditor = Editor.CreateEditor(world) as StormWorldEditor;
                    m_worldEditor.ParentWindowRepaint = Repaint;
                    m_worldEditor.IsFromMainWindow = true;
                }
                m_worldEditor.OnInspectorGUI();
                if (world == null) //our world inspector can now change out the world object, check if it changed and exit to prevent errors
                    GUIUtility.ExitGUI();
            }
            else if (m_worldEditor != null)
                m_worldEditor = null;
        }
        private void WorldPanel(bool helpActive)
        {
            updateTiles = true;
            StormWorld world = StormWorld.Instance;
            EditorGUI.BeginDisabledGroup(true);
            m_editorUtils.ObjectField("WorldInstance", world, typeof(StormWorld), true);
            EditorGUI.EndDisabledGroup();
            m_editorUtils.InlineHelp("WorldInstance", helpActive);
            if (world == null)
            {
                if (m_editorUtils.Button("CreateWorldButton", helpActive))
                {
                    StormWizardUtils.CreateStormWorld();
                }
            }
            else
            {
                if (m_editorUtils.Button("SelectWorldButton", helpActive))
                {
                    StormUtils.SetEditorSelection(world.gameObject);
                    EditorGUIUtility.PingObject(world.gameObject);
                }
            }
        }

        private void IngestionTab()
        {
            if (!hasInitIngest)
            {
                hasInitIngest = true;

                if (!StormEditorSettings.Instance.IngestManualBounds)
                {
                    CheckBounds(m_ingestObject, out m_ingestBounds);
                    StormEditorSettings.Instance.IngestBounds = m_ingestBounds;
                }
                else
                    m_ingestBounds = StormEditorSettings.Instance.IngestBounds;
                m_ingestTileBounds = CalcTiles(m_ingestBounds, m_dataTiles);
            }

            m_editorUtils.Panel("Ingestion", IngestionPanel, true);

            m_editorUtils.Heading("AssetCreationHeader");
            m_editorUtils.Panel("ImpostorSettingsPanel", ImpostorSettingsPanel, false);
            m_editorUtils.Panel("FeatureCreation", FeatureCreationPanel, false);
            m_editorUtils.Panel("CreateImpostorPanel", CreateImpostorPanel, false);
            m_editorUtils.Panel("CacheTerrainPanel", CacheTerrainPanel, false);

            m_editorUtils.Heading("IngestedAssetsHeader");
            m_editorUtils.Panel("DataTiles", TilesPanel, false);
            m_editorUtils.Panel("AllFeatures", AllFeaturesPanel, false);
            m_editorUtils.Panel("AllImpostorsPanel", AllImpostorsPanel, false);
        }
        private void IngestionPanel(bool helpActive)
        {
            StormEditorSettings EditorSettings = StormEditorSettings.Instance;
            int IngestSourceType = EditorSettings.IngestSourceType;

            EditorGUILayout.BeginVertical(m_boxStyle);
            m_editorUtils.LabelField("IngestTypeHeader", EditorStyles.boldLabel);
            showIngestGizmos = IngestSourceType == 0 && EditorSettings.IngestTileSize > 10;
            EditorGUI.BeginChangeCheck();
            if (IngestSourceType == 0)
            {
                IngestSourceType = m_editorUtils.Popup("IngestSourceType", IngestSourceType, new string[] { "IngestSource_Object", "IngestSource_Scene" }, helpActive);
                GameObject go = m_editorUtils.ObjectField("IngestObject", m_ingestObject, typeof(GameObject), true, helpActive) as GameObject;
                if (go != m_ingestObject)
                {
                    m_ingestObject = go;
                    if (!EditorSettings.IngestManualBounds)
                    {
                        CheckBounds(m_ingestObject, out m_ingestBounds);
                        StormEditorSettings.Instance.IngestBounds = m_ingestBounds;
                    }
                    else
                        m_ingestBounds = StormEditorSettings.Instance.IngestBounds;
                    m_ingestTileBounds = CalcTiles(m_ingestBounds, m_dataTiles);
                }
                Vector3 min = m_ingestBounds.min;
                Vector3 max = m_ingestBounds.max;
                EditorGUILayout.BeginHorizontal();
                bool manualBounds = EditorSettings.IngestManualBounds;
                EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true));
                if (manualBounds)
                {
                    EditorGUI.BeginChangeCheck();
                    min = m_editorUtils.Vector3Field("IngestAreaMin", min, helpActive);
                    max = m_editorUtils.Vector3Field("IngestAreaMax", max, helpActive);
                    if (EditorGUI.EndChangeCheck())
                    {
                        m_ingestBounds = new Bounds((min * 0.5f) + (max * 0.5f), max - min);
                        StormEditorSettings.Instance.IngestBounds = m_ingestBounds;
                    }
                }
                else
                {
                    m_editorUtils.LabelField("IngestAreaMin", new GUIContent(min.ToString()));
                    m_editorUtils.LabelField("IngestAreaMax", new GUIContent(max.ToString()));
                }
                EditorGUILayout.EndVertical();
                EditorGUILayout.BeginVertical(GUILayout.Width(100));
                m_editorUtils.ToggleButton(manualBounds ? "ManualBoundsButton" : "AutoBoundsButton", ref manualBounds, GUILayout.Width(100));
                if (!manualBounds)
                {
                    if (m_editorUtils.Button("IngestRecalcBoundsButton", GUILayout.Width(100)))
                    {
                        CheckBounds(m_ingestObject, out m_ingestBounds);
                        StormEditorSettings.Instance.IngestBounds = m_ingestBounds;
                    }
                }
                else
                    m_ingestBounds = StormEditorSettings.Instance.IngestBounds;
                m_ingestTileBounds = CalcTiles(m_ingestBounds, m_dataTiles);
                EditorGUILayout.EndVertical();
                if (manualBounds != EditorSettings.IngestManualBounds)
                {
                    Undo.RecordObject(EditorSettings, "Bounds Mode Changed");
                    EditorSettings.IngestManualBounds = manualBounds;
                    if (manualBounds)
                        EditorSettings.IngestBounds = m_ingestBounds;
                    else
                    {
                        CheckBounds(m_ingestObject, out m_ingestBounds);
                    }
                    m_ingestTileBounds = CalcTiles(m_ingestBounds, m_dataTiles);
                }
                EditorGUILayout.EndHorizontal();
                m_editorUtils.InlineHelp("AutoBoundsButton", helpActive);
            }
            else
            {
                IngestSourceType = m_editorUtils.Popup("IngestSourceType", IngestSourceType, new string[] { "IngestSource_Object", "IngestSource_Scene" }, helpActive);
                GUILayout.BeginHorizontal();
                GUILayout.Space(EditorGUIUtility.labelWidth);
                if (m_editorUtils.Button("LoadBuildScenes"))
                {
                    LoadBuildSettingScenes(m_ingestScenes, m_ingestSceneList);
                }
                if (m_editorUtils.Button("AddCurrentSceneButton"))
                {
                    LoadCurrentScenes(m_ingestScenes, m_ingestSceneList);
                }
#if GAIA_PRO_PRESENT || GAIA_2023_PRO
                Gaia.TerrainLoaderManager terrainLoader = StormUtils.FindObjectOfType<Gaia.TerrainLoaderManager>();
                if (m_editorUtils.Button("AddGaiaScenesButton"))
                {
                    LoadGaiaScenes(m_ingestScenes, m_ingestSceneList, terrainLoader);
                }
#endif
                GUILayout.EndHorizontal();
                m_sceneIngestFoldout = StormWorldEditor.DrawReorderableList("SceneToIngest", m_sceneIngestFoldout, m_ingestSceneList, m_editorUtils, helpActive);
                m_editorUtils.InlineHelp("IngestScene", helpActive);
            }
            EditorGUILayout.EndVertical();
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(EditorSettings, "Ingestion Type Changed");
                EditorSettings.IngestSourceType = IngestSourceType;
            }

            int IngestTargetType = EditorSettings.IngestTargetType;
            bool IngestClearTile = EditorSettings.IngestClearTile;
            string IngestFilePath = EditorSettings.IngestFilePath;
            bool IngestRawAsTexture = EditorSettings.IngestRawAsTexture;
            bool IngestSingleFile = EditorSettings.IngestSingleFile;
            bool IngestCaptureDisabled = EditorSettings.IngestCaptureDisabled;
            bool IngestCreateFeatures = EditorSettings.IngestCreateFeatures;
            bool IngestCreateFeaturesets = EditorSettings.IngestCreateFeaturesets;

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.BeginVertical(m_boxStyle);
            m_editorUtils.LabelField("IngestTargetHeader", EditorStyles.boldLabel);
            IngestTargetType = m_editorUtils.Popup("IngestTargetType", IngestTargetType, new string[] { "IngestTarget_TILE", "IngestTarget_DATA" }, helpActive);
            if (IngestTargetType == 0)
            {
                IngestClearTile = m_editorUtils.Toggle("IngestClearTiles", IngestClearTile, helpActive);
                int tileSize = Mathf.Max(1, m_editorUtils.IntField("IngestTileSize", EditorSettings.IngestTileSize, helpActive));
                if (IngestSourceType == 0)
                {
                    if (tileSize != EditorSettings.IngestTileSize)
                    {
                        Undo.RecordObject(EditorSettings, "Tile Size Changed");
                        EditorSettings.IngestTileSize = tileSize;
                        m_ingestTileBounds = CalcTiles(m_ingestBounds, m_dataTiles);
                    }
                    Vector3 ingestSize = m_ingestBounds.size;
                    ingestSize.x /= EditorSettings.IngestTileSize;
                    ingestSize.z /= EditorSettings.IngestTileSize;
                    m_editorUtils.LabelField("IngestExpectedTileCountX", new GUIContent(Mathf.Max(1, Mathf.CeilToInt(ingestSize.x)).ToString()), helpActive);
                    m_editorUtils.LabelField("IngestExpectedTileCountZ", new GUIContent(Mathf.Max(1, Mathf.CeilToInt(ingestSize.z)).ToString()), helpActive);
                }
                else
                {
                    if (tileSize != EditorSettings.IngestTileSize)
                    {
                        Undo.RecordObject(EditorSettings, "Tile Size Changed");
                        EditorSettings.IngestTileSize = tileSize;
                        m_ingestTileBounds = Array.Empty<Bounds>();
                    }
                }
            }
            else
            {
                EditorGUILayout.BeginHorizontal();
                IngestFilePath = m_editorUtils.TextField("IngestTargetPath", IngestFilePath, GUILayout.ExpandWidth(true));
                if (m_editorUtils.Button("IngestTargetBrowseButton", GUILayout.ExpandWidth(false)))
                {
                    GUIContent dialog = m_editorUtils.GetContent("IngestTargetDialog");
                    string path = EditorUtility.SaveFolderPanel(dialog.text, IngestFilePath, "");
                    if (!string.IsNullOrWhiteSpace(path))
                    {
                        path = path.Replace('\\', '/');
                        if (path.StartsWith(Application.dataPath))
                            path = path.Substring(Application.dataPath.Length - 6);
                        if (!path.EndsWith('/'))
                            path += "/";

                        Undo.RecordObject(EditorSettings, "Ingest Path Changed");
                        EditorSettings.IngestFilePath = path;
                    }
                }
                EditorGUILayout.EndHorizontal();
                IngestRawAsTexture = m_editorUtils.Toggle("IngestRawAsTexture", IngestRawAsTexture, helpActive);
                IngestSingleFile = m_editorUtils.Toggle("IngestSingleFile", IngestSingleFile, helpActive);
                m_editorUtils.InlineHelp("IngestTargetPath", helpActive);
                int tileSize = Mathf.Max(1, m_editorUtils.IntField("IngestTileSize", EditorSettings.IngestTileSize, helpActive));
                if (tileSize != EditorSettings.IngestTileSize)
                {
                    Undo.RecordObject(EditorSettings, "Tile Size Changed");
                    EditorSettings.IngestTileSize = tileSize;
                    if (IngestSourceType == 0)
                        m_ingestTileBounds = CalcTiles(m_ingestBounds, m_dataTiles);
                }
                if (IngestSourceType == 0)
                {
                    Vector3 ingestSize = m_ingestBounds.size;
                    ingestSize.x /= EditorSettings.IngestTileSize;
                    ingestSize.z /= EditorSettings.IngestTileSize;
                    m_editorUtils.LabelField("IngestExpectedTileCountX", new GUIContent(Mathf.Max(1, Mathf.CeilToInt(ingestSize.x)).ToString()), helpActive);
                    m_editorUtils.LabelField("IngestExpectedTileCountZ", new GUIContent(Mathf.Max(1, Mathf.CeilToInt(ingestSize.z)).ToString()), helpActive);
                }
            }
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(EditorSettings, "Ingestion Target Changed");
                EditorSettings.IngestTargetType = IngestTargetType;
                EditorSettings.IngestClearTile = IngestClearTile;
                EditorSettings.IngestFilePath = IngestFilePath;
                EditorSettings.IngestRawAsTexture = IngestRawAsTexture;
                EditorSettings.IngestSingleFile = IngestSingleFile;
            }

            bool IngestCaptureAlphamaps = EditorSettings.IngestCaptureAlphamaps;
            EditorGUILayout.EndVertical();
            EditorGUILayout.BeginVertical(m_boxStyle);
            m_editorUtils.LabelField("IngestSourceHeader", EditorStyles.boldLabel);
            IngestCaptureAlphamaps = m_editorUtils.Toggle("IngestCaptureAlphamaps", IngestCaptureAlphamaps, helpActive);
            IngestCaptureDisabled = m_editorUtils.Toggle("IngestCaptureDisabled", IngestCaptureDisabled, helpActive);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(EditorSettings, "Ingestion SourceSettings Changed");
                EditorSettings.IngestCaptureAlphamaps = IngestCaptureAlphamaps;
                EditorSettings.IngestCaptureDisabled = IngestCaptureDisabled;
            }

            m_editorUtils.PropertyField("IngestStatic", p_IngestStatic, helpActive);
            m_editorUtils.PropertyField("IngestNonStatic", p_IngestNonStatic, helpActive);
            m_editorUtils.PropertyField("IngestIgnoredPrefabs", p_IngestIgnoredPrefabs, helpActive);
            m_editorUtils.PropertyField("IngestIgnoredLayers", p_IngestIgnoredLayers, helpActive);
            m_editorUtils.PropertyField("IngestIgnoredTags", p_IngestIgnoredTags, helpActive);

            EditorGUILayout.Space();
            m_editorUtils.LabelField("MeshTerrainHeader", EditorStyles.boldLabel);

            m_editorUtils.PropertyField("MeshTerrainResolution", p_MeshTerrainResolution, helpActive);
            m_editorUtils.PropertyField("MeshTerrainTags", p_MeshTerrainTags, helpActive);
            m_editorUtils.PropertyField("DefaultColorTargetData", p_DefaultColorTargetData, helpActive);
            m_editorUtils.PropertyField("ColorTargetRange", p_ColorTargetRange, helpActive);
            m_editorUtils.PropertyField("ColorTargetFalloff", p_ColorTargetFalloff, helpActive);

            EditorGUILayout.EndVertical();

            StormRegion GlobalRegion = EditorSettings.GlobalRegion;
            StormFeatureset GlobalFeatureset = EditorSettings.GlobalFeatureset;
            float GlobalFeaturesetAddReq = EditorSettings.GlobalRegionAddReq;

            EditorGUILayout.BeginVertical(m_boxStyle);
            EditorGUI.BeginChangeCheck();
            m_editorUtils.LabelField("IngestGlobalFeatureSetHeader", EditorStyles.boldLabel);
            IngestCreateFeatures = m_editorUtils.Toggle("IngestCreateFeatures", IngestCreateFeatures, helpActive);
            if (EditorGUI.EndChangeCheck())
                EditorSettings.IngestCreateFeatures = IngestCreateFeatures;
            if (IngestCreateFeatures)
            {
                EditorGUI.BeginChangeCheck();
                StormSystemSettings settings = StormSettings.SystemSettings;
                settings.CreateImpostors = m_editorUtils.Toggle("ImpSettings_CreateImpostors", settings.CreateImpostors, helpActive);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(EditorSettings, "CreateImpostors Changed");
                    StormUtils.SetAssetDirty(settings);
                }
                EditorGUI.BeginChangeCheck();
                IngestCreateFeaturesets = m_editorUtils.Toggle("IngestCreateFeaturesets", IngestCreateFeaturesets, helpActive);
                if (EditorGUI.EndChangeCheck())
                    EditorSettings.IngestCreateFeaturesets = IngestCreateFeaturesets;
                if (IngestCreateFeaturesets)
                {
                    EditorGUI.BeginChangeCheck();
                    GlobalRegion = m_editorUtils.ObjectField("IngestGlobalRegion", GlobalRegion, typeof(StormRegion), false, helpActive) as StormRegion;
                    GlobalFeatureset = m_editorUtils.ObjectField("IngestGlobalFeatureset", GlobalFeatureset, typeof(StormFeatureset), false, helpActive) as StormFeatureset;
                    GlobalFeaturesetAddReq = m_editorUtils.Slider("IngestGlobalFeaturesetAddReq", GlobalFeaturesetAddReq, 0f, 1f, helpActive);
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(EditorSettings, "Global Feature Set Changed");
                        EditorSettings.GlobalRegion = GlobalRegion;
                        EditorSettings.GlobalFeatureset = GlobalFeatureset;
                        EditorSettings.GlobalRegionAddReq = GlobalFeaturesetAddReq;
                    }
                }
            }
            EditorGUILayout.EndVertical();

            if (m_editorUtils.Button("IngestButton", helpActive))
            {
                if (EditorUtility.DisplayDialog(m_editorUtils.GetTextValue("AboutToIngest"), m_editorUtils.GetTooltip("AboutToIngest"), "Yes", "No"))
                {
                    m_needsIngest = true;
                }
            }
        }
        private void TilesPanel(bool helpActive)
        {
            UpdateDataTiles();
            m_dataTiles.RemoveAll((t) => t == null);
            m_dataTileList.DoLayoutList();
            m_editorUtils.InlineHelp("GeneratedTiles", helpActive);
        }
        private void LoadTiles(bool helpActive)
        {
            m_loadTilesList.DoLayoutList();
            m_editorUtils.InlineHelp("LoadTileList", helpActive);
            if (m_editorUtils.Button("LoadTileButton"))
            {
                for (int p = 0; p < m_loadTiles.Count; ++p)
                {
                    string path = m_loadTiles[p];
                    StormTileData data = StormInternalUtils.ImportTileData(path);
                    if (data != null)
                    {
                        GameObject tileGO = new GameObject(Path.GetFileNameWithoutExtension(path));
                        tileGO.transform.position = data.Position;
                        tileGO.transform.localScale = data.Size;
                        StormTile tile = tileGO.AddComponent<StormTile>();
                        tile.TileData = data;
                    }
                }
            }
        }

        private void ImpostorsTab()
        {
            m_editorUtils.Panel("ImpostorSettingsPanel", ImpostorSettingsPanel, false);
            m_editorUtils.Panel("CreateImpostorPanel", CreateImpostorPanel, true);
            m_editorUtils.Panel("AllImpostorsPanel", AllImpostorsPanel, false);
        }
        private void ImpostorSettingsPanel(bool helpActive)
        {
            m_impostorSettingsHelpSwitch = helpActive;
            StormSystemSettings settings = StormSettings.SystemSettings;
            settings.CreateImpostors = m_editorUtils.Toggle(settings.CreateImpostors, "ImpSettings_CreateImpostors", helpActive);
            settings.DefaultDetailCreateImpostor = m_editorUtils.Toggle(settings.DefaultDetailCreateImpostor, "ImpSettings_CreateDetailImpostors", helpActive);
            m_impostorSettingsBillboardStrings.isExpanded = StormWorldEditor.DrawReorderableList("ImpSettings_BillboardStrings", m_impostorSettingsBillboardStrings.isExpanded, m_impostorSettingsBillboardStringsList, m_editorUtils, helpActive);
            m_impostorSettingsBakeShaders.isExpanded = StormWorldEditor.DrawReorderableList("ImpSettings_BakeShaders", m_impostorSettingsBakeShaders.isExpanded, m_impostorSettingsBakeShadersList, m_editorUtils, helpActive);
            m_impostorSettingsPostProcess.isExpanded = StormWorldEditor.DrawReorderableList("ImpSettings_PostProcess", m_impostorSettingsPostProcess.isExpanded, m_impostorSettingsPostProcessList, m_editorUtils, helpActive);
            m_editorUtils.PropertyField("ImpSettings_FlatReplacement", m_impostorSettingsFlatReplacement, helpActive);

            Rect rect = GUILayoutUtility.GetLastRect();
            rect.x += rect.width - 100;
            rect.width = 100;
            rect.height = EditorGUIUtility.singleLineHeight;
            if (GUI.Button(rect, m_editorUtils.GetContent("DefaultButton")))
            {
                StormSettings.ImpostorFlatReplacement = StormSystemSettings.GetDefaultFlatReplacement(StormUtils.GetRenderPipline());
                m_impostorSettingsObject.Update();
            }

            settings.ImpostorType = (ImpostorType)m_editorUtils.Popup("ImpSettings_Type", (int)settings.ImpostorType, m_impostorTypeKeys, helpActive);
            settings.AllowImpostorTypeOverride = m_editorUtils.Toggle(settings.AllowImpostorTypeOverride, "ImpSettings_AllowTypeOverride", helpActive);
            settings.ImpostorResolution = m_editorUtils.IntField("ImpSettings_Resolution", settings.ImpostorResolution, helpActive);
            settings.ImpostorOctoSteps = m_editorUtils.IntField("ImpSettings_OctoSteps", settings.ImpostorOctoSteps, helpActive);
            EditorGUILayout.BeginHorizontal();
            settings.ImpostorExportDirectory = m_editorUtils.TextField("ImpSettings_ExportDirectory", settings.ImpostorExportDirectory);
            if (m_editorUtils.Button("DefaultButton", GUILayout.ExpandWidth(false)))
                settings.ImpostorExportDirectory = StormUtils.DefaultSavePath + "Impostors";
            EditorGUILayout.EndHorizontal();
            m_editorUtils.InlineHelp("ImpSettings_ExportDirectory", helpActive);
            m_impostorSettingsObject.ApplyModifiedProperties();
        }

        private void ImpostorMatsListHeader(Rect rect)
        {
            GUI.Label(rect, m_editorUtils.GetContent("ImpostorCreateMats"));
        }
        private void ImpostorMatsListOnAdd(ReorderableList list)
        {
            if (list.list.Count > 0)
                list.list.Add(list.list[list.list.Count - 1]);
            else
                list.list.Add(null);
        }
        private void ImpostorMatsListDrawElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            m_impostorMatsToUpdate[index] = EditorGUI.ObjectField(rect, m_impostorMatsToUpdate[index], typeof(Material), false) as Material;
        }
        private void ImpostorMatsListDrawEmptyElement(Rect rect)
        {
            if (rect.height > EditorGUIUtility.singleLineHeight)
                rect.height = EditorGUIUtility.singleLineHeight;
            GUI.Label(rect, m_editorUtils.GetContent("ImpostorMatsListEmpty"), EditorStyles.boldLabel);
        }
        private void CreateImpostorPanel(bool helpActive)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUI.BeginChangeCheck();
            bool modePrefab = GUILayout.Toggle(m_impostorMode == 0, m_editorUtils.GetContent("ImpostorModePrefab"), EditorStyles.miniButtonLeft, GUILayout.ExpandWidth(true));
            bool modeMeshMat = GUILayout.Toggle(m_impostorMode == 1, m_editorUtils.GetContent("ImpostorModeMeshMat"), EditorStyles.miniButtonRight, GUILayout.ExpandWidth(true));
            if (EditorGUI.EndChangeCheck())
            {
                if (modePrefab && m_impostorMode != 0)
                {
                    m_impostorMode = 0;
                }
                else if (modeMeshMat && m_impostorMode != 1)
                {
                    m_impostorMode = 1;
                }
            }
            EditorGUILayout.EndHorizontal();
            if (modePrefab)
            {
                m_impostorObjToUpdate = m_editorUtils.ObjectField("ImpostorCreatePrefab", m_impostorObjToUpdate, typeof(GameObject), false, GUILayout.ExpandWidth(true)) as GameObject;
                m_editorUtils.InlineHelp("ImpostorCreatePrefab", helpActive);
            }
            else if (modeMeshMat)
            {
                m_impostorMeshToUpdate = m_editorUtils.ObjectField("ImpostorCreateMesh", m_impostorMeshToUpdate, typeof(Mesh), false, GUILayout.ExpandWidth(true)) as Mesh;
                m_editorUtils.InlineHelp("ImpostorCreateMesh", helpActive);
                if (m_impostorMatsList == null)
                {
                    m_impostorMatsList = new ReorderableList(m_impostorMatsToUpdate, typeof(Material));
                    m_impostorMatsList.drawHeaderCallback += ImpostorMatsListHeader;
                    m_impostorMatsList.onAddCallback += ImpostorMatsListOnAdd;
                    m_impostorMatsList.drawNoneElementCallback += ImpostorMatsListDrawEmptyElement;
                    m_impostorMatsList.drawElementCallback += ImpostorMatsListDrawElement;
                }
                m_impostorMatsList.DoLayoutList();
                m_editorUtils.InlineHelp("ImpostorCreateMats", helpActive);
            }
            m_impostorType = (ImpostorType)m_editorUtils.Popup("ImpostorCreateType", (int)m_impostorType, m_impostorTypeKeys);
            m_editorUtils.InlineHelp("ImpostorCreateType", helpActive);
            if (m_editorUtils.Button("ImpostorCreateButton", helpActive))
            {
                if (modePrefab)
                    StormAssetUtils.GetImpostor(m_impostorObjToUpdate.transform, m_impostorType, true);
                else if (modeMeshMat)
                    StormAssetUtils.GetImpostor(m_impostorMeshToUpdate, m_impostorMatsToUpdate.ToArray(), m_impostorType, true);
            }
        }
        private void AllImpostorsPanel(bool helpActive)
        {
            ReadOnlySpan<Impostor> allImpostors = StormAssetUtils.AllImpostors;
            if (allImpostors != null)
            {
                if (m_impostorListExpanded.Length != allImpostors.Length)
                    Array.Resize(ref m_impostorListExpanded, allImpostors.Length);
                if (m_impostorMatsExpanded.Length != allImpostors.Length)
                    Array.Resize(ref m_impostorMatsExpanded, allImpostors.Length);
                for (int i = 0; i < allImpostors.Length; ++i)
                {
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    Impostor imp = allImpostors[i];
                    GUILayout.BeginHorizontal();
                    Rect rect = EditorGUILayout.GetControlRect(GUILayout.Width(15), GUILayout.ExpandWidth(false));
                    m_impostorListExpanded[i] = EditorGUI.Foldout(rect, m_impostorListExpanded[i], new GUIContent("", imp == null ? "NULL" : imp.name), true);
                    EditorGUILayout.ObjectField(imp, typeof(Impostor), false);
                    GUILayout.EndHorizontal();
                    if (imp != null && m_impostorListExpanded[i])
                    {
                        if (imp.m_sourcePrefab != null)
                        {
                            EditorGUILayout.ObjectField(m_editorUtils.GetContent("ImpostorPrefab"), imp.m_sourcePrefab, typeof(GameObject), false);
                            m_editorUtils.InlineHelp("ImpostorPrefab", helpActive);
                        }
                        else
                        {
                            EditorGUILayout.ObjectField(m_editorUtils.GetContent("ImpostorMesh"), imp.m_sourceMesh, typeof(Mesh), false);
                            m_editorUtils.InlineHelp("ImpostorMesh", helpActive);
                            m_impostorMatsExpanded[i] = m_editorUtils.Foldout(m_impostorMatsExpanded[i], "ImpostorMats");
                            if (m_impostorMatsExpanded[i])
                            {
                                for (int m = 0; m < imp.m_sourceMaterials.Length; ++m)
                                {
                                    EditorGUILayout.ObjectField(imp.m_sourceMaterials[m], typeof(Material), false);
                                }
                            }
                            m_editorUtils.InlineHelp("ImpostorMats", helpActive);
                        }
                        imp.m_type = (ImpostorType)m_editorUtils.Popup("ImpostorType", (int)imp.m_type, m_impostorTypeKeys);
                        m_editorUtils.InlineHelp("ImpostorType", helpActive);
                        if (m_editorUtils.Button("ImpostorListRecreateButton", helpActive))
                        {
                            Impostor ret;
                            if (imp.m_sourcePrefab != null)
                                ret = StormAssetUtils.GetImpostor(imp.m_sourcePrefab.transform, StormSettings.ImpostorResolution, StormSettings.ImpostorType, null, StormSettings.ImpostorOctoSteps, StormSettings.ImpostorBakeShaders, StormSettings.ImpostorPostProcess, StormSettings.ImpostorExportDirectory, "", "", StormSettings.AllowImpostorTypeOverride, true);
                            else
                                ret = StormAssetUtils.GetImpostor(imp.m_sourceMesh, imp.m_sourceMaterials, StormSettings.ImpostorResolution, StormSettings.ImpostorType, null, StormSettings.ImpostorOctoSteps, StormSettings.ImpostorBakeShaders, StormSettings.ImpostorPostProcess, StormSettings.ImpostorExportDirectory, StormSettings.AllowImpostorTypeOverride, true);
                            StormAssetUtils.RefreshImpostors();
                        }
                    }
                    EditorGUILayout.EndVertical();
                }
                if (m_editorUtils.Button("ImpostorListRecreateAllButton", helpActive))
                {
                    GUIContent dialog = m_editorUtils.GetContent("ImpostorListRecreateAllDialog");
                    GUIContent yesno = m_editorUtils.GetContent("YesNo");
                    if (EditorUtility.DisplayDialog(dialog.text, dialog.tooltip, yesno.text, yesno.tooltip))
                    {
                        for (int i = 0; i < allImpostors.Length; ++i)
                        {
                            Impostor imp = allImpostors[i];
                            if (imp.m_sourcePrefab != null)
                                StormAssetUtils.GetImpostor(imp.m_sourcePrefab.transform, StormSettings.ImpostorResolution, StormSettings.ImpostorType, null, StormSettings.ImpostorOctoSteps, StormSettings.ImpostorBakeShaders, StormSettings.ImpostorPostProcess, StormSettings.ImpostorExportDirectory, "", "", StormSettings.AllowImpostorTypeOverride, true);
                            else
                                StormAssetUtils.GetImpostor(imp.m_sourceMesh, imp.m_sourceMaterials, StormSettings.ImpostorResolution, StormSettings.ImpostorType, null, StormSettings.ImpostorOctoSteps, StormSettings.ImpostorBakeShaders, StormSettings.ImpostorPostProcess, StormSettings.ImpostorExportDirectory, StormSettings.AllowImpostorTypeOverride, true);
                        }
                        StormAssetUtils.RefreshImpostors();
                    }
                }
            }
        }


        TerrainData[] m_allTerrains = null;
        bool[] m_terrainSelected = new bool[0];
        bool m_isAllSelected = false;
        Vector2 m_terrainListScroll;
        GUIStyle m_terrainListStyleA;
        GUIStyle m_terrainListStyleB;
        Texture2D m_terrainListTexture;

        private void CacheTerrainPanel(bool helpActive)
        {
            StormSettings.CacheTerrainData = m_editorUtils.Toggle(StormSettings.CacheTerrainData, "CacheTerrainDataSetting", helpActive);
            if (!StormSettings.CacheTerrainData)
                return;

            EditorGUILayout.Space();

            bool AutoMarkTerrains = false;
            if (m_allTerrains == null)
            {
                m_allTerrains = StormUtils.FindAssets<TerrainData>("t:TerrainData", "");
                AutoMarkTerrains = true;
            }
            int numTerrains = m_allTerrains.Length;
            if (m_terrainSelected.Length != numTerrains)
            {
                int startCount = m_terrainSelected.Length;
                Array.Resize(ref m_terrainSelected, numTerrains);
                if (m_isAllSelected)
                {
                    for (int t = startCount; t < numTerrains; ++t)
                        m_terrainSelected[t] = true;
                }
            }
            EditorGUILayout.BeginHorizontal();
            bool selectAllValue = GUILayout.Toggle(m_isAllSelected, m_editorUtils.GetContent("Terrain_SelectAllToggle"), EditorStyles.toggle, GUILayout.ExpandWidth(true));
            if (m_editorUtils.Button("Terrain_AutoSelect", GUILayout.ExpandWidth(false)))
                AutoMarkTerrains = true;
            EditorGUILayout.EndHorizontal();
            m_editorUtils.InlineHelp("Terrain_SelectAllToggle", helpActive);
            if (selectAllValue != m_isAllSelected)
            {
                m_isAllSelected = selectAllValue;
                for (int t = 0; t < m_terrainSelected.Length; ++t)
                    m_terrainSelected[t] = selectAllValue;
            }
            if (m_terrainListStyleA == null)
            {
                if (m_terrainListTexture == null)
                    m_terrainListTexture = new Texture2D(2, 2);
                m_terrainListTexture.SetPixels32(new Color32[] { new Color(1, 1, 1, 0.1f), new Color(1, 1, 1, 0.1f), new Color(1, 1, 1, 0.1f), new Color(1, 1, 1, 0.1f) });
                m_terrainListTexture.Apply();
                m_terrainListStyleA = new GUIStyle();
                m_terrainListStyleB = new GUIStyle();
                m_terrainListStyleB.normal.background = m_terrainListTexture;
            }
            bool atLeastOneSelected = false;
            bool selectedHasData = false;

            m_terrainListScroll = GUILayout.BeginScrollView(m_terrainListScroll, EditorStyles.helpBox, GUILayout.Height(math.min(300, numTerrains * (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing))));
            for (int t = 0; t < numTerrains; ++t)
            {
                GUILayout.BeginHorizontal(t % 2 == 0 ? m_terrainListStyleA : m_terrainListStyleB);
                {
                    bool wasSelected = m_terrainSelected[t];
                    bool isSelected = GUILayout.Toggle(wasSelected, new GUIContent(), GUILayout.ExpandWidth(false));
                    if (isSelected != wasSelected)
                    {
                        m_terrainSelected[t] = isSelected;
                        selectAllValue = true;
                        for (int i = 0; i < m_terrainSelected.Length; ++i)
                            if (!m_terrainSelected[i])
                            {
                                selectAllValue = false;
                                break;
                            }
                        m_isAllSelected = selectAllValue;
                    }
                    if (isSelected)
                        atLeastOneSelected = true;
                    EditorGUI.BeginDisabledGroup(true);
                    EditorGUILayout.ObjectField(m_allTerrains[t], typeof(UnityEngine.Terrain), false);
                    EditorGUI.EndDisabledGroup();

                    if (StormAssetUtils.HasTerrainData(m_allTerrains[t], out bool isOutOfDate))
                    {
                        if (isSelected)
                            selectedHasData = true;
                        if (isOutOfDate)
                        {
                            if (AutoMarkTerrains)
                                m_terrainSelected[t] = true;
                            GUILayout.Label(new GUIContent(EditorGUIUtility.IconContent("console.warnicon").image, "Needs Update"), GUILayout.Width(EditorGUIUtility.singleLineHeight), GUILayout.Height(EditorGUIUtility.singleLineHeight));
                        }
                        else
                        {
                            GUILayout.Label(new GUIContent(EditorGUIUtility.IconContent("d_CacheServerConnected@2x").image, "Cached"), GUILayout.Width(EditorGUIUtility.singleLineHeight), GUILayout.Height(EditorGUIUtility.singleLineHeight));
                        }
                    }
                    else
                    {
                        if (AutoMarkTerrains)
                            m_terrainSelected[t] = true;
                        GUILayout.Label(new GUIContent(EditorGUIUtility.IconContent("d_CacheServerDisconnected@2x").image, "Not Cached"), GUILayout.Width(EditorGUIUtility.singleLineHeight), GUILayout.Height(EditorGUIUtility.singleLineHeight));
                    }
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.EndScrollView();
            EditorGUILayout.BeginHorizontal();
            {
                EditorGUI.BeginDisabledGroup(!atLeastOneSelected);
                if (m_editorUtils.Button("Terrain_CacheButton"))
                {
                    GUIContent dialogContent = m_editorUtils.GetContent("Terrain_CacheProgressDialog");

                    int numSelected = 0;
                    for (int i = 0; i < m_terrainSelected.Length; ++i)
                        if (m_terrainSelected[i])
                            ++numSelected;

                    StormEditorProgress.Show(dialogContent.text, dialogContent.tooltip, numSelected);
                    numSelected = 0;
                    for (int i = 0; i < m_allTerrains.Length; ++i)
                    {
                        if (m_terrainSelected[i])
                        {
                            StormEditorProgress.Update(m_allTerrains[i].name, numSelected++, numSelected == 1);
                            StormAssetUtils.GetTerrainTexture(m_allTerrains[i], TerrainSourceData.TerrainSource.HEIGHT, 0);
                        }
                    }
                    StormEditorProgress.Clear();
                    dialogContent = m_editorUtils.GetContent("Terrain_CacheCompleteDialog");
                    EditorUtility.DisplayDialog(dialogContent.text, dialogContent.tooltip, "Ok");
                    GUIUtility.ExitGUI();
                }
                EditorGUI.EndDisabledGroup();

                EditorGUI.BeginDisabledGroup(!selectedHasData);
                if (m_editorUtils.Button("Terrain_DeleteCacheButton"))
                {
                    GUIContent confirmDialog = m_editorUtils.GetContent("Terrain_DeleteCacheConfirmDialog");
                    if (EditorUtility.DisplayDialog(confirmDialog.text, confirmDialog.tooltip, "Yes", "No, Cancel"))
                    {
                        GUIContent dialogContent = m_editorUtils.GetContent("Terrain_DeleteCacheProgressDialog");

                        int numSelected = 0;
                        for (int i = 0; i < m_terrainSelected.Length; ++i)
                            if (m_terrainSelected[i])
                                ++numSelected;

                        StormEditorProgress.Show(dialogContent.text, dialogContent.tooltip, numSelected);
                        numSelected = 0;
                        for (int i = 0; i < m_allTerrains.Length; ++i)
                        {
                            if (m_terrainSelected[i])
                            {
                                StormEditorProgress.Update(m_allTerrains[i].name, numSelected++, numSelected == 1);
                                StormAssetUtils.DeleteTerrainData(m_allTerrains[i]);
                            }
                        }
                        StormEditorProgress.Clear();
                        dialogContent = m_editorUtils.GetContent("Terrain_DeleteCacheCompleteDialog");
                        EditorUtility.DisplayDialog(dialogContent.text, dialogContent.tooltip, "Ok");
                        GUIUtility.ExitGUI();
                    }
                }
                EditorGUI.EndDisabledGroup();
            }
            EditorGUILayout.EndHorizontal();
            m_editorUtils.InlineHelp("Terrain_CacheButton", helpActive);
        }

        private void FeaturesTab()
        {
            m_editorUtils.Panel("FeatureCreation", FeatureCreationPanel, true);
            m_editorUtils.Panel("AllFeatures", AllFeaturesPanel, false);
        }
        private void FeatureCreationPanel(bool helpActive)
        {
            StormEditorSettings editorSettings = StormEditorSettings.Instance;
            GameObject CreationPrefab = editorSettings.FeatureCreationPrefab;
            StormFeatureType FeatureType = editorSettings.FeatureFeatureType;
            string SavePath = editorSettings.FeatureSavePath;
            bool AddToGlobalFeatureSet = editorSettings.FeatureAddToFeatureSet;
            StormFeatureset GlobalFeatureSet = editorSettings.FeatureFeatureSet;
            bool SelectAfterCreation = editorSettings.FeatureSelectAfterCreation;

            EditorGUI.BeginChangeCheck();
            CreationPrefab = (GameObject)m_editorUtils.ObjectField("CreationPrefab", CreationPrefab, typeof(GameObject), true, helpActive);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(editorSettings, "Creation Settings Changed");
                editorSettings.FeatureCreationPrefab = CreationPrefab;
                m_createFeatureSettingsRef = StormAssetUtils.GetExistingObjectSettings(CreationPrefab);
                if (m_createFeatureSettingsRef == null)
                    editorSettings.FeatureFeatureType = StormAssetUtils.GetExpectedFeatureType(CreationPrefab);
                else
                    editorSettings.FeatureFeatureType = m_createFeatureSettingsRef.ObjectType;
                p_CreateFeatureLayer.stringValue = CreationPrefab == null ? "" : CreationPrefab.name;
                isAPrefab = IsCreationAPrefab(out isAPrefabErrorCode);
            }
            bool disable = false;
            if (CreationPrefab == null)
            {
                disable = true;
            }
            else
            {
                if (!isAPrefab)
                {
                    switch (isAPrefabErrorCode)
                    {
                        case 0:
                            {
                                EditorGUILayout.HelpBox("The prefab object you have selected: '" + CreationPrefab.name + "' is not a prefab. Only prefabs are supported.", MessageType.Warning);
                                break;
                            }
                        default:
                            {
                                EditorGUILayout.HelpBox("The prefab object you have selected: '" + CreationPrefab.name + "' is a prefab but no mesh renderer were found inside the prefab.", MessageType.Warning);
                                break;
                            }
                    }

                    disable = true;
                }
            }
            EditorGUI.BeginDisabledGroup(true);
            m_editorUtils.ObjectField("CreateFeatureExistingSettings", m_createFeatureSettingsRef, typeof(StormFeatureSettings), false);
            EditorGUI.EndDisabledGroup();
            EditorGUI.BeginDisabledGroup(disable);
            m_editorUtils.InlineHelp("CreateFeatureExistingSettings", helpActive);
            StormSystemSettings settings = StormSettings.SystemSettings;
            EditorGUI.BeginChangeCheck();
            FeatureType = (StormFeatureType)m_editorUtils.EnumPopup("FeatureType", FeatureType, helpActive);
            EditorGUILayout.BeginHorizontal();
            SavePath = m_editorUtils.TextField("SavePath", SavePath);
            if (m_editorUtils.Button("SelectPath", GUILayout.MaxWidth(100f)))
            {
                string newPath = EditorUtility.OpenFolderPanel("Target Folder", SavePath, "");
                if (!string.IsNullOrEmpty(newPath))
                {
                    newPath = newPath.Replace(Application.dataPath, "Assets");
                    if (!newPath.EndsWith("/"))
                    {
                        newPath += "/";
                    }

                    if (SavePath != newPath)
                    {
                        SavePath = newPath;
                        editorSettings.FeatureSavePath = SavePath;
                    }
                }
            }
            EditorGUILayout.EndHorizontal();
            m_editorUtils.InlineHelp("SavePath", helpActive);
            settings.CreateImpostors = m_editorUtils.Toggle("ImpSettings_CreateImpostors", settings.CreateImpostors, helpActive);

            SelectAfterCreation = m_editorUtils.Toggle("SelectAfterCreation", SelectAfterCreation, helpActive);
            AddToGlobalFeatureSet = m_editorUtils.Toggle("AddToGlobalFeatureSet", AddToGlobalFeatureSet, helpActive);
            if (AddToGlobalFeatureSet)
            {
                EditorGUI.indentLevel++;
                GlobalFeatureSet = (StormFeatureset)m_editorUtils.ObjectField("GlobalFeatureSet", GlobalFeatureSet, typeof(StormFeatureset), false, helpActive);
                m_editorUtils.PropertyField("CreateFeatureLayerName", p_CreateFeatureLayer, helpActive);
                EditorGUI.indentLevel--;
            }

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(settings, "Creation Settings Changed");
                StormUtils.SetAssetDirty(settings);
                editorSettings.FeatureFeatureType = FeatureType;
                editorSettings.FeatureSavePath = SavePath;
                editorSettings.FeatureAddToFeatureSet = AddToGlobalFeatureSet;
                editorSettings.FeatureFeatureSet = GlobalFeatureSet;
                editorSettings.FeatureSelectAfterCreation = SelectAfterCreation;
            }
            StormEditorSettingsObject.ApplyModifiedProperties();
            if (m_editorUtils.Button(m_createFeatureSettingsRef != null ? "UpdateFeatureButton" : "CreateFeatureButton", helpActive))
            {
                StormFeatureSettings setting = BeginFeatureCreation();
                if (setting != null && SelectAfterCreation)
                {
                    Selection.activeObject = setting;
                    EditorGUIUtility.PingObject(setting);
                }
            }
            EditorGUI.EndDisabledGroup();
        }
        private void AllFeaturesPanel(bool helpActive)
        {
            ReadOnlySpan<StormFeatureSettings> allFeatures = StormAssetUtils.AllFeatures;
            if (allFeatures != null)
            {
                if (m_featureListExpanded.Length != allFeatures.Length)
                    Array.Resize(ref m_featureListExpanded, allFeatures.Length);
                if (m_featureMatsExpanded.Length != allFeatures.Length)
                    Array.Resize(ref m_featureMatsExpanded, allFeatures.Length);
                for (int i = 0; i < allFeatures.Length; ++i)
                {
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    StormFeatureSettings feature = allFeatures[i];
                    GUILayout.BeginHorizontal();
                    Rect rect = EditorGUILayout.GetControlRect(GUILayout.Width(15), GUILayout.ExpandWidth(false));
                    m_featureListExpanded[i] = EditorGUI.Foldout(rect, m_featureListExpanded[i], new GUIContent("", feature == null ? "NULL" : feature.name), true);
                    EditorGUILayout.ObjectField(feature, typeof(StormFeatureSettings), false);
                    GUILayout.EndHorizontal();
                    if (feature != null && m_featureListExpanded[i])
                    {
                        switch (feature.SourceType)
                        {
                            case StormFeatureSource.PREFAB:
                                feature.SourcePrefab = m_editorUtils.ObjectField("FeatureSourcePrefab", feature.SourcePrefab, typeof(GameObject), false, helpActive) as GameObject;
                                EditorGUI.BeginDisabledGroup(feature.SourcePrefab == null);
                                if (m_editorUtils.Button("FeatureListRecreateButton", helpActive))
                                {
                                    StormAssetUtils.CreateSettingsFromPrefab(feature.SourcePrefab, feature.ObjectType, StormUtils.GetAssetPath(feature), true, feature);
                                    if (StormWorld.Instance != null)
                                        StormWorld.Instance.ForceFullRefresh(true);
                                }
                                if (m_editorUtils.Button("FeatureListUpdateImpostorButton", helpActive))
                                {
                                    StormAssetUtils.AddOrUpdateImpostor(feature);
                                    GUIUtility.ExitGUI();
                                    return;
                                }
                                EditorGUI.EndDisabledGroup();
                                break;
                            case StormFeatureSource.TERRAIN_DETAIL:
                                feature.SourceTerrain = m_editorUtils.ObjectField("FeatureSourceDetailTerrain", feature.SourceTerrain, typeof(TerrainData), false, helpActive) as TerrainData;
                                EditorGUI.BeginDisabledGroup(feature.SourceTerrain == null);
                                feature.SourceDetailIndex = m_editorUtils.IntSlider("FeatureSourceDetailIndex", feature.SourceDetailIndex, 0, feature.SourceTerrain == null ? 0 : feature.SourceTerrain.detailPrototypes.Length, helpActive);
                                if (m_editorUtils.Button("FeatureListRecreateButton", helpActive))
                                {
                                    StormAssetUtils.CreateSettingsFromDetail(feature.SourceTerrain, feature.SourceDetailIndex, StormSettings.SystemSettings, StormUtils.GetAssetPath(feature), false, feature);
                                    if (StormWorld.Instance != null)
                                        StormWorld.Instance.ForceFullRefresh(true);
                                }
                                EditorGUI.EndDisabledGroup();
                                break;
                            default:
                            case StormFeatureSource.CUSTOM:
                                GUILayout.Label(m_editorUtils.GetContent("FeatureTypeCustomLabel"), EditorStyles.boldLabel);
                                break;
                        }
                    }
                    EditorGUILayout.EndVertical();
                }
                if (m_editorUtils.Button("FeatureListRecreateAllButton", helpActive))
                {
                    GUIContent dialog = m_editorUtils.GetContent("FeatureListRecreateAllDialog");
                    GUIContent yesno = m_editorUtils.GetContent("YesNo");
                    if (EditorUtility.DisplayDialog(dialog.text, dialog.tooltip, yesno.text, yesno.tooltip))
                    {
                        for (int i = 0; i < allFeatures.Length; ++i)
                        {
                            StormFeatureSettings feature = allFeatures[i];
                            if (feature != null)
                            {
                                switch (feature.SourceType)
                                {
                                    case StormFeatureSource.PREFAB:
                                        if (feature.SourcePrefab != null)
                                        {
                                            StormAssetUtils.CreateSettingsFromPrefab(feature.SourcePrefab, feature.ObjectType, StormUtils.GetAssetPath(feature), true, feature);
                                            if (StormWorld.Instance != null)
                                                StormWorld.Instance.ForceFullRefresh(true);
                                        }
                                        break;
                                    default:
                                        break;
                                }
                            }
                        }
                    }
                }
            }
        }

        private bool IsCreationAPrefab(out int errorCode)
        {
            errorCode = -1;
            StormEditorSettings editorSettings = StormEditorSettings.Instance;
            GameObject creationPrefab = editorSettings.FeatureCreationPrefab;
            if (creationPrefab == null)
            {
                return false;
            }

            bool isPrefab = PrefabUtility.IsPartOfAnyPrefab(creationPrefab);
            if (isPrefab)
            {
                bool hasRenderer = false;
                Renderer[] renderers = creationPrefab.GetComponentsInChildren<Renderer>();
                foreach (Renderer r in renderers)
                {
                    if (r.GetComponent<MeshFilter>() != null)
                    {
                        hasRenderer = true;
                        break;
                    }
                }
                if (!hasRenderer)
                {
                    errorCode = 1;
                    return false;
                }
                else
                {
                    return true;
                }
            }
            else
            {
                errorCode = 0;
            }


            return false;
        }
        StormFeatureSettings BeginFeatureCreation()
        {
            StormEditorSettings editorSettings = StormEditorSettings.Instance;
            string path = editorSettings.FeatureSavePath;
            if (string.IsNullOrEmpty(path))
            {
                path = StormUtils.DefaultSavePath + StormAssetUtils.FeatureSubPath;
            }

            m_createFeatureSettingsRef = StormAssetUtils.GetObjectSettings(editorSettings.FeatureCreationPrefab, editorSettings.FeatureFeatureType, path, false, false);
            if (m_createFeatureSettingsRef != null && editorSettings.FeatureAddToFeatureSet && editorSettings.FeatureFeatureSet != null)
                StormInternalUtils.UpdateFeatureset(editorSettings.FeatureFeatureSet, m_createFeatureSettingsRef, p_CreateFeatureLayer.stringValue);

            return m_createFeatureSettingsRef;
        }

        private void UtilsTab()
        {
            StormWorld world = StormWorld.Instance;
            if (!isIngesting)
            {
                if (world == null)
                {
                    EditorGUILayout.HelpBox(m_editorUtils.GetTooltip("StormWorldMissingHelp"), MessageType.Warning);
                }
            }
            EditorGUI.BeginDisabledGroup(world == null);
            m_editorUtils.Panel("UtilsPanel", UtilsPanel, true);
            m_shaderDataUI.DoLayout();
            m_editorUtils.Panel("CleanUp", CleanUpPanel);
            m_editorUtils.Panel("LoadTiles", LoadTiles);
            EditorGUI.BeginDisabledGroup(m_isBaking);
            m_editorUtils.Panel("BakePanel", BakePanel, false);
            EditorGUI.EndDisabledGroup();
            EditorGUI.EndDisabledGroup();
        }
        private void UtilsPanel(bool helpActive)
        {
            if (m_editorUtils.Button("ResetErrorsButton", helpActive))
            {
                StormAssetUtils.ResetErroredPrefabs();
            }

            StormEditorUtils.ClearWorldGUI(m_editorUtils);
            StormEditorUtils.ReEnableIngestedGUI(m_editorUtils);
            if (m_editorUtils.Button("AddRuntimeUI"))
                StormEditorUtils.AddRuntimeUI();
            EditorGUILayout.Space();
            EditorGUI.BeginDisabledGroup(Core.StormWorld.Instance == null);
            int numUnhidden = StormEditorUtils.GetUnhiddenCount();
            if (numUnhidden < 1)
            {
                if (m_editorUtils.Button("UnhideObjectsButton"))
                {
                    StormEditorUtils.UnhideObjects();
                }
            }
            else
            {
                m_editorUtils.LabelField("HiddenObjectCount", new GUIContent(numUnhidden.ToString()));
                if (m_editorUtils.Button("HideObjectsButton"))
                {
                    StormEditorUtils.RehideObjects();
                }
            }
            EditorGUI.EndDisabledGroup();
        }
        private void CleanUpPanel(bool helpActive)
        {
            StormEditorUtils.CleanUpFeatureSetsGUI(m_editorUtils, StormEditorSettings.Instance, helpActive);
        }
        private void BakePanel(bool helpActive)
        {
            StormEditorSettings editorSettings = StormEditorSettings.Instance;
            bool editorSettingsChanged = false;
            bool manualBounds = editorSettings.BakeManualBounds;
            if (!hasInitBake)
            {
                hasInitBake = true;
                if (manualBounds)
                    editorSettings.BakeBounds = m_bakeBounds;
                else
                {
                    CalcDataTiles(out m_bakeBounds);
                }
                m_bakeTileBounds = CalcTiles(m_bakeBounds);
                string activeScene = SceneManager.GetActiveScene().path;
                editorSettings.BakeSaveScene = string.IsNullOrWhiteSpace(activeScene) ? null : AssetDatabase.LoadAssetAtPath<SceneAsset>(activeScene);
                editorSettingsChanged = true;
            }
            m_multiSceneBake = m_editorUtils.Toggle("BakeMultiScene", m_multiSceneBake, helpActive);
            if (m_multiSceneBake)
            {
                if (m_editorUtils.Button("AddBakeBuildScenes", GUILayout.MaxWidth(130f)))
                {
                    LoadBuildSettingScenes(m_bakeScenesList, m_bakeScenes);
                }
                m_bakeScenesFoldout = StormWorldEditor.DrawReorderableList("BakeScenes", m_bakeScenesFoldout, m_bakeScenes, m_editorUtils, helpActive);
                /*
                int tileSize = Mathf.Max(1, m_editorUtils.IntField("BakeTileSize", editorSettings.BakeTileSize, helpActive));
                if (tileSize != editorSettings.BakeTileSize)
                {
                    editorSettingsChanged = true;
                    editorSettings.BakeTileSize = tileSize;
                }
                //*/
                int bakeSaveType = m_editorUtils.Popup("BakeSaveType", editorSettings.BakeSaveType, m_bakeSaveTypeList, helpActive);
                if (bakeSaveType != editorSettings.BakeSaveType)
                {
                    editorSettingsChanged = true;
                    editorSettings.BakeSaveType = bakeSaveType;
                }
                if (bakeSaveType == 0) //single
                {
                    SceneAsset saveScene = m_editorUtils.ObjectField("BakeTargetScene", editorSettings.BakeSaveScene, typeof(SceneAsset), true, helpActive) as SceneAsset;
                    int saveFreq = Mathf.Max(1, m_editorUtils.IntField("BakeSaveFrequency", editorSettings.BakeSaveFrequency, helpActive));
                    if (saveFreq != editorSettings.BakeSaveFrequency || saveScene != editorSettings.BakeSaveScene)
                    {
                        editorSettingsChanged = true;
                        editorSettings.BakeSaveFrequency = saveFreq;
                        editorSettings.BakeSaveScene = saveScene;
                    }
                }
                else if (bakeSaveType == 1) //one each
                {
                    EditorGUILayout.BeginHorizontal();
                    string savePath = m_editorUtils.TextField("BakeSavePath", editorSettings.BakeSavePath, helpActive);
                    if (m_editorUtils.Button("BrowseButton", GUILayout.ExpandWidth(false)))
                    {
                        GUIContent dialogContent = m_editorUtils.GetContent("BakeSaveBrowseDialog");
                        string newPath = EditorUtility.SaveFolderPanel(dialogContent.text, "Assets/", dialogContent.tooltip);
                        if (!string.IsNullOrEmpty(newPath) && newPath.StartsWith(Application.dataPath))
                            savePath = newPath.Substring(Application.dataPath.Length - 6) + "/";
                    }
                    EditorGUILayout.EndHorizontal();
                    if (savePath != editorSettings.BakeSavePath)
                    {
                        editorSettingsChanged = true;
                        editorSettings.BakeSavePath = savePath;
                    }
                    string savePrefix = m_editorUtils.TextField("BakeSavePrefix", editorSettings.BakeSavePrefix, helpActive);
                    if (savePrefix != editorSettings.BakeSavePath)
                    {
                        editorSettingsChanged = true;
                        editorSettings.BakeSavePrefix = savePrefix;
                    }
                }
                else //area based
                {
                    EditorGUILayout.BeginHorizontal();
                    string savePath = m_editorUtils.TextField("BakeSavePath", editorSettings.BakeSavePath, helpActive);
                    if (m_editorUtils.Button("BrowseButton", GUILayout.ExpandWidth(false)))
                    {
                        GUIContent dialogContent = m_editorUtils.GetContent("BakeSaveBrowseDialog");
                        string newPath = EditorUtility.SaveFolderPanel(dialogContent.text, "Assets/", dialogContent.tooltip);
                        if (!string.IsNullOrEmpty(newPath) && newPath.StartsWith(Application.dataPath))
                            savePath = newPath.Substring(Application.dataPath.Length - 6) + "/";
                    }
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.BeginHorizontal();
                    string savePrefix = m_editorUtils.TextField("BakeSavePrefix", editorSettings.BakeSavePrefix);
                    m_editorUtils.Label("BakeSavePrefixText", GUILayout.ExpandWidth(false));
                    EditorGUILayout.EndHorizontal();
                    m_editorUtils.InlineHelp("BakeSavePrefix", helpActive);
                    float saveArea = m_editorUtils.FloatField("BakeSaveArea", editorSettings.BakeSaveSceneSize, helpActive);
                    if (savePath != editorSettings.BakeSavePath || savePrefix != editorSettings.BakeSavePath || saveArea != editorSettings.BakeSaveSceneSize)
                    {
                        editorSettingsChanged = true;
                        editorSettings.BakeSavePath = savePath;
                        editorSettings.BakeSavePrefix = savePrefix;
                        editorSettings.BakeSaveSceneSize = saveArea;
                    }
                }

            }
            else
            {
                m_bakeTarget = m_editorUtils.ObjectField("BakeTarget", m_bakeTarget, typeof(Transform), true, helpActive) as Transform;
                Vector3 min = m_bakeBounds.min;
                Vector3 max = m_bakeBounds.max;
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.BeginVertical();
                if (manualBounds)
                {
                    EditorGUI.BeginChangeCheck();
                    min = m_editorUtils.Vector3Field("BakeAreaMin", min, helpActive);
                    max = m_editorUtils.Vector3Field("BakeAreaMax", max, helpActive);
                    if (EditorGUI.EndChangeCheck())
                    {
                        m_bakeBounds = new Bounds((min * 0.5f) + (max * 0.5f), max - min);
                    }
                }
                else
                {
                    m_editorUtils.LabelField("BakeArea", new GUIContent(min + " - " + max), helpActive);
                }
                EditorGUILayout.EndVertical();
                m_editorUtils.ToggleButton(manualBounds ? "ManualBoundsButton" : "AutoBoundsButton", ref manualBounds, GUILayout.ExpandWidth(false));
                if (manualBounds != editorSettings.BakeManualBounds)
                {
                    editorSettingsChanged = true;
                    editorSettings.BakeManualBounds = manualBounds;
                    if (manualBounds)
                        editorSettings.BakeBounds = m_bakeBounds;
                    else
                    {
                        CalcDataTiles(out m_bakeBounds);
                    }
                    m_bakeTileBounds = CalcTiles(m_bakeBounds);
                }
                EditorGUILayout.EndHorizontal();
                /*
                int tileSize = Mathf.Max(1, m_editorUtils.IntField("BakeTileSize", editorSettings.BakeTileSize, helpActive));
                if (tileSize != editorSettings.BakeTileSize)
                {
                    editorSettingsChanged = true;
                    editorSettings.BakeTileSize = tileSize;
                    m_ingestTileBounds = CalcTiles(m_ingestBounds, m_dataTiles);
                }
                //*/
                Vector3 ingestSize = m_bakeBounds.size;
                ingestSize.x /= editorSettings.BakeTileSize;
                ingestSize.z /= editorSettings.BakeTileSize;
                m_editorUtils.LabelField("BakeExpectedTileCountX", new GUIContent(Mathf.Max(1, Mathf.CeilToInt(ingestSize.x)).ToString()), helpActive);
                m_editorUtils.LabelField("BakeExpectedTileCountZ", new GUIContent(Mathf.Max(1, Mathf.CeilToInt(ingestSize.z)).ToString()), helpActive);
            }
            EditorGUI.BeginChangeCheck();
            m_bakeFeaturesetsFoldout = StormWorldEditor.DrawReorderableList("BakeFeaturesets", m_bakeFeaturesetsFoldout, m_bakeFeaturesetsList, m_editorUtils, helpActive);
            if (EditorGUI.EndChangeCheck())
                CalcBakeFeaturesets();
            m_editorUtils.LabelField("BakeFeatureCount", new GUIContent(m_bakeFeatureCount.ToString()), helpActive);
            GameObject bakePrefab = m_editorUtils.ObjectField("BakePrefab", editorSettings.BakePrefab, typeof(GameObject), false, helpActive) as GameObject;
            if (bakePrefab != editorSettings.BakePrefab)
            {
                editorSettingsChanged = true;
                editorSettings.BakePrefab = bakePrefab;
            }
            EditorGUI.BeginDisabledGroup(m_bakeFeatureCount < 1);
            if (m_editorUtils.Button("DoBakeButton"))
            {
                m_needsBake = true;
            }
            EditorGUI.EndDisabledGroup();
            if (editorSettingsChanged)
            {
                Undo.RecordObject(editorSettings, "BakeSettings Changed");
                EditorUtility.SetDirty(editorSettings);
            }
        }

        #endregion
        #region Util Functions
        List<Type> allTypes = null;
        string[] allTypeNames;
        void UpdateTypes()
        {
            allTypes = new List<Type>();
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies)
            {
                var types = assembly.GetTypes();
                foreach (var type in types)
                {
                    if (!type.IsAbstract && type.IsClass && type.IsSubclassOf(typeof(Component)))
                    {
                        allTypes.Add(type);
                    }
                }
            }
            allTypes.Sort((t1, t2) => t1.Name.CompareTo(t2.Name));
            allTypeNames = new string[allTypes.Count];
            for (int i = 0; i < allTypeNames.Length; ++i)
                allTypeNames[i] = allTypes[i].Name + " (" + allTypes[i].Namespace + ")";
        }
        Type SelectType(string key, Type type, bool helpActive)
        {
            if (allTypes == null)
                UpdateTypes();
            int idx = EditorGUILayout.Popup(m_editorUtils.GetContent(key), allTypes.IndexOf(type), allTypeNames);
            m_editorUtils.InlineHelp(key, helpActive);
            return idx < 0 || idx > allTypeNames.Length ? null : allTypes[idx];
        }

        /// <summary>
        /// Builds the reorderable lists for this editor
        /// </summary>
        private void BuildReOrderableLists()
        {
            m_featuresets = new List<StormFeatureset>();
            m_featuresetList = new ReorderableList(m_featuresets, typeof(StormFeatureset));
            m_featuresetList.headerHeight = EditorGUIUtility.singleLineHeight;
            m_featuresetList.drawHeaderCallback -= DrawFeaturesetListHeader;
            m_featuresetList.drawHeaderCallback += DrawFeaturesetListHeader;
            m_featuresetList.elementHeight = EditorGUIUtility.singleLineHeight;
            m_featuresetList.drawElementCallback -= DrawFeaturesetElement;
            m_featuresetList.drawElementCallback += DrawFeaturesetElement;
            m_featuresetList.onAddCallback -= AddFeaturesetElement;
            m_featuresetList.onAddCallback += AddFeaturesetElement;
            m_featuresetList.onRemoveCallback -= OnRemoveElement;
            m_featuresetList.onRemoveCallback += OnRemoveElement;

            m_dataTiles = new List<StormTile>();
            m_dataTileList = new ReorderableList(m_dataTiles, typeof(StormTile), false, true, true, false);
            m_dataTileList.elementHeight = EditorGUIUtility.singleLineHeight;
            m_dataTileList.drawHeaderCallback -= DrawDataTileListHeader;
            m_dataTileList.drawHeaderCallback += DrawDataTileListHeader;
            m_dataTileList.drawElementCallback -= DrawDataTileElement;
            m_dataTileList.drawElementCallback += DrawDataTileElement;
            m_dataTileList.onAddCallback -= AddDataTile;
            m_dataTileList.onAddCallback += AddDataTile;
            m_dataTileList.onRemoveCallback -= RemoveDataTile;
            m_dataTileList.onRemoveCallback += RemoveDataTile;
            m_dataTileList.onRemoveCallback -= OnRemoveElement;
            m_dataTileList.onRemoveCallback += OnRemoveElement;

            m_ingestScenes = new List<SceneAsset>();
            m_ingestSceneList = new ReorderableList(m_ingestScenes, typeof(SceneAsset));
            m_ingestSceneList.headerHeight = EditorGUIUtility.singleLineHeight;
            m_ingestSceneList.drawHeaderCallback -= DrawSceneListHeader;
            m_ingestSceneList.drawHeaderCallback += DrawSceneListHeader;
            m_ingestSceneList.elementHeight = EditorGUIUtility.singleLineHeight;
            m_ingestSceneList.drawElementCallback -= DrawSceneElement;
            m_ingestSceneList.drawElementCallback += DrawSceneElement;
            m_ingestSceneList.onAddCallback -= AddSceneElement;
            m_ingestSceneList.onAddCallback += AddSceneElement;
            m_ingestSceneList.onRemoveCallback -= OnRemoveElement;
            m_ingestSceneList.onRemoveCallback += OnRemoveElement;

            m_loadTiles = new List<string>();
            m_loadTilesList = new ReorderableList(m_loadTiles, typeof(string));
            m_loadTilesList.headerHeight = EditorGUIUtility.singleLineHeight;
            m_loadTilesList.drawHeaderCallback -= DrawLoadTileListHeader;
            m_loadTilesList.drawHeaderCallback += DrawLoadTileListHeader;
            m_loadTilesList.elementHeight = EditorGUIUtility.singleLineHeight;
            m_loadTilesList.drawElementCallback -= DrawLoadTileElement;
            m_loadTilesList.drawElementCallback += DrawLoadTileElement;
            m_loadTilesList.onAddCallback -= AddLoadTileElement;
            m_loadTilesList.onAddCallback += AddLoadTileElement;
            m_loadTilesList.onRemoveCallback -= OnRemoveElement;
            m_loadTilesList.onRemoveCallback += OnRemoveElement;

            m_bakeFeaturesetsList = new ReorderableList(m_bakeFeaturesets, typeof(StormFeatureset));
            m_bakeFeaturesetsList.headerHeight = EditorGUIUtility.singleLineHeight;
            m_bakeFeaturesetsList.drawHeaderCallback -= DrawBakeFeaturesetsHeader;
            m_bakeFeaturesetsList.drawHeaderCallback += DrawBakeFeaturesetsHeader;
            m_bakeFeaturesetsList.elementHeight = EditorGUIUtility.singleLineHeight;
            m_bakeFeaturesetsList.drawElementCallback -= DrawBakeFeaturesetsElement;
            m_bakeFeaturesetsList.drawElementCallback += DrawBakeFeaturesetsElement;
            m_bakeFeaturesetsList.onAddCallback -= AddBakeFeaturesetsElement;
            m_bakeFeaturesetsList.onAddCallback += AddBakeFeaturesetsElement;
            m_bakeFeaturesetsList.onRemoveCallback -= OnRemoveBakeFeaturesetsElement;
            m_bakeFeaturesetsList.onRemoveCallback += OnRemoveBakeFeaturesetsElement;

            m_bakeScenes = new ReorderableList(m_bakeScenesList, typeof(SceneAsset));
            m_bakeScenes.headerHeight = EditorGUIUtility.singleLineHeight;
            m_bakeScenes.drawHeaderCallback -= DrawBakeScenesHeader;
            m_bakeScenes.drawHeaderCallback += DrawBakeScenesHeader;
            m_bakeScenes.elementHeight = EditorGUIUtility.singleLineHeight;
            m_bakeScenes.drawElementCallback -= DrawBakeScenesElement;
            m_bakeScenes.drawElementCallback += DrawBakeScenesElement;
            m_bakeScenes.onAddCallback -= AddBakeFeaturesetsElement;
            m_bakeScenes.onAddCallback += AddBakeFeaturesetsElement;
            m_bakeScenes.onRemoveCallback -= OnRemoveBakeFeaturesetsElement;
            m_bakeScenes.onRemoveCallback += OnRemoveBakeFeaturesetsElement;
        }
        #endregion
        #region Ingestion
        /// <summary>
        /// Performs the scene(s) ingestion
        /// </summary>
        private IEnumerator DoIngest()
        {
            if (m_worldEditor != null)
            {
                DestroyImmediate(m_worldEditor);
                m_worldEditor = null;
            }
            StormWorld world = StormWorld.Instance;
            StormEditorSettings editorSettings = StormEditorSettings.Instance;
            if (world == null && editorSettings.IngestSourceType == 0)
            {
                //no StormWorld in scene. Create one.
                //The PW_STORM_PRESENT define check is required
                //otherwise there is a compile error on install
                //as the CreateStormWorld function then can't return a Storm World yet
#if PW_STORM_PRESENT
                world = StormWizardUtils.CreateStormWorld();
#endif
            }
            if (editorSettings.IngestCreateFeatures && editorSettings.IngestCreateFeaturesets)
            {
                if (editorSettings.GlobalRegion == null)
                {
                    StormRegion globalRegion = CreateInstance<StormRegion>();
                    globalRegion.name = "New Global Region";
                    editorSettings.GlobalRegion = globalRegion;
                }
                if (editorSettings.GlobalFeatureset == null)
                {
                    StormFeatureset globalFeatureset = CreateInstance<StormFeatureset>();
                    globalFeatureset.name = "New Global Featureset";
                    editorSettings.GlobalFeatureset = globalFeatureset;
                }
                StormUtils.AddIfNew(ref editorSettings.GlobalRegion.Featuresets, editorSettings.GlobalFeatureset);
            }
            isIngesting = true;
            //Debug.Log("A");
            if (editorSettings.IngestSourceType == 0)
            {
                yield return DoIngestScene(world.transform, m_ingestObject);
                //Debug.Log("a1");
            }
            else if (editorSettings.IngestSourceType == 1)
            {
                if (editorSettings.IngestTargetType == 1 && string.IsNullOrWhiteSpace(editorSettings.IngestFilePath))
                {
                    GUIContent dialog = m_editorUtils.GetContent("IngestNoPath");
                    EditorUtility.DisplayDialog(dialog.text, dialog.tooltip, m_editorUtils.GetTextValue("OKButton"));
                    isIngesting = false;
                    yield break;
                }
                //ingest scenes
                m_ingestScenes.RemoveAll((s) => s == null);
                if (m_ingestScenes.Count == 0)
                {
                    GUIContent dialog = m_editorUtils.GetContent("IngestNoScenes");
                    EditorUtility.DisplayDialog(dialog.text, dialog.tooltip, m_editorUtils.GetTextValue("OKButton"));
                    isIngesting = false;
                    yield break;
                }
                if (editorSettings.IngestCreateFeatures && editorSettings.IngestCreateFeaturesets && string.IsNullOrEmpty(StormUtils.GetAssetPath(editorSettings.GlobalRegion)))
                {
                    GUIContent dialog = m_editorUtils.GetContent("IngestSaveRegionAsk");
                    int save = EditorUtility.DisplayDialogComplex(dialog.text, dialog.tooltip, m_editorUtils.GetTextValue("YesButton"), m_editorUtils.GetTextValue("NoButton"), m_editorUtils.GetTextValue("CancelButton"));
                    if (save == 0)
                    {
                        //yes pressed
                        dialog = m_editorUtils.GetContent("IngestSaveRegion");
                        string path = EditorUtility.SaveFilePanelInProject(dialog.text, editorSettings.GlobalRegion.name, "asset", dialog.tooltip);
                        if (!string.IsNullOrEmpty(path))
                        {
                            editorSettings.GlobalRegion = StormUtils.SaveAsset(editorSettings.GlobalRegion, path);
                        }
                    }
                    else if (save == 1)
                    {
                        //no pressed
                        editorSettings.GlobalRegion = null;
                    }
                    else if (save == 2)
                    {
                        //cancel button pressed
                        isIngesting = false;
                        yield break;
                    }
                }
                EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
                string[] curScenes = new string[SceneManager.sceneCount];
                bool[] curScenesActive = new bool[SceneManager.sceneCount];
                Scene[] curOpenScenes = new Scene[SceneManager.sceneCount];
                Scene activeScene = EditorSceneManager.GetActiveScene();
                int activeSceneIdx = 0;
                for (int s = 0; s < curScenes.Length; ++s)
                {
                    curOpenScenes[s] = SceneManager.GetSceneAt(s);
                    if (curOpenScenes[s].path == activeScene.path)
                        activeSceneIdx = s;
                    curScenes[s] = curOpenScenes[s].path;
                    curScenesActive[s] = curOpenScenes[s].isLoaded;
                }
                for (int s = 1; s < curScenes.Length; ++s)
                {
                    EditorSceneManager.CloseScene(curOpenScenes[s], true);
                }
                for (int s = 0; s < m_ingestScenes.Count; ++s)
                {
                    SceneAsset sceneAsset = m_ingestScenes[s];
                    if (sceneAsset == null)
                        continue;
                    string scenePath = AssetDatabase.GetAssetPath(sceneAsset);
                    Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                    yield return null;
                    CheckBounds(null, out Bounds objBounds);
                    List<StormTile> dataTiles = new List<StormTile>(StormUtils.FindObjectsOfType<StormTile>(true));
                    if (editorSettings.IngestClearTile)
                    {
                        foreach (StormTile tile in dataTiles)
                        {
                            if (tile.GetComponents<Component>().Length > 2)
                                DestroyImmediate(tile);
                            else
                                DestroyImmediate(tile.gameObject);
                        }
                        dataTiles.Clear();
                    }
                    Transform tileBase;
                    if (StormWorld.Instance != null)
                    {
                        tileBase = StormWorld.Instance.transform;
                    }
                    else
                    {
                        GameObject go = GameObject.Find("StormData");
                        if (go == null)
                        {
                            go = new GameObject("StormData");
                            go.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
                        }
                        tileBase = go.transform;
                    }
                    yield return DoIngestScene(tileBase, null, CalcTiles(objBounds, dataTiles));
                    //after ingestion, reset the global region and/or featureset if they are not an asset
                    //(prevents issue where region we create during ingestion changes to "Missing" after a scene load)
                    if (string.IsNullOrEmpty(StormUtils.GetAssetPath(editorSettings.GlobalRegion)))
                    {
                        editorSettings.GlobalRegion = null;
                        editorSettings.CheckGlobalRegion = true;
                    }
                    if (string.IsNullOrEmpty(StormUtils.GetAssetPath(editorSettings.GlobalFeatureset)))
                        editorSettings.GlobalFeatureset = null;
                    EditorSceneManager.SaveScene(scene);
                    GC.Collect();
                }
                if (editorSettings.IngestTargetType == 1)
                    StormUtils.RefreshAssetDatabase();
                Scene lastScene = EditorSceneManager.GetSceneAt(0);
                bool isLastLoaded = false;
                for (int s = 0; s < curScenes.Length; ++s)
                {
                    if (!string.IsNullOrEmpty(curScenes[s]))
                    {
                        curOpenScenes[s] = EditorSceneManager.OpenScene(curScenes[s], curScenesActive[s] ? OpenSceneMode.Additive : OpenSceneMode.AdditiveWithoutLoading);
                        if (curOpenScenes[s].path == lastScene.path)
                            isLastLoaded = true;
                    }
                }
                if (!isLastLoaded)
                    EditorSceneManager.CloseScene(lastScene, true);
                EditorSceneManager.SetActiveScene(curOpenScenes[activeSceneIdx]);
            }
            //Debug.Log("End");
            GUIContent dialogContent = m_editorUtils.GetContent("IngestionFinishedDialog");
            EditorUtility.DisplayDialog(dialogContent.text, dialogContent.tooltip, "Ok");
            if (StormWorld.Instance != null)
                StormWorld.Instance.UpdateFeaturesets();
            if (editorSettings.IngestCreateFeatures && editorSettings.IngestCreateFeaturesets && editorSettings.IngestTargetType == 0 && editorSettings.IngestSourceType == 0)
                StormUtils.AddIfNew(world.Regions, editorSettings.GlobalRegion);
            isIngesting = false;
        }
        private IEnumerator DoIngestScene(Transform dataParent, GameObject targetObject = null, Bounds[] tileBoundsArray = null)
        {
            if (tileBoundsArray == null)
                tileBoundsArray = m_ingestTileBounds;
            //Debug.Log("DoIngestScene");
            StormEditorSettings editorSettings = StormEditorSettings.Instance;
            if (editorSettings.IngestTargetType == 0)
            {
                //Debug.Log("B");
                //ingest to Tiles ... create StormTiles if needed and set up a default TileData for them
                StormTile[] tiles = new StormTile[tileBoundsArray.Length];
                StormTileData[] tileData = new StormTileData[tiles.Length];
                StormTileRegion[] tileRegions = new StormTileRegion[tiles.Length];
                updateTiles = true;
                UpdateDataTiles();
                if (editorSettings.IngestClearTile)
                {
                    foreach (StormTile tile in m_dataTiles)
                    {
                        if (tile.GetComponents<Component>().Length > 2)
                            DestroyImmediate(tile);
                        else
                            DestroyImmediate(tile.gameObject);
                    }
                    m_dataTiles.Clear();
                }
                for (int i = 0; i < tiles.Length; ++i)
                {
                    Bounds tileBounds = tileBoundsArray[i];
                    bool found = false;
                    for (int t = 0; t < m_dataTiles.Count; ++t)
                    {
                        StormTile data = m_dataTiles[t];
                        if (data.TileData.Bounds == tileBounds)
                        {
                            tileData[i] = data.TileData;
                            tileRegions[i] = data.TileRegions;
                            tiles[i] = data;
                            found = true;
                            break;
                        }
                    }
                    if (!found)
                    {
                        GameObject tileGO = new GameObject("StormTile - " + i);
                        tileGO.SetActive(false); //disable go before adding StormTile so it won't initialize until after we are done
                        tileGO.transform.SetParent(dataParent);
                        StormTile tile = tileGO.AddComponent<StormTile>();
                        tile.UpdatePosition = false;
                        tile.UpdateScale = false;
                        tile.IngestCaptureAlphamaps = editorSettings.IngestCaptureAlphamaps;
                        tile.IngestCaptureDisabled = editorSettings.IngestCaptureDisabled;
                        tile.IngestCreateFeatures = editorSettings.IngestCreateFeatures;
                        tileGO.transform.position = tileBounds.min;
                        tileGO.transform.localScale = tileBounds.size;
                        tileData[i] = new StormTileData();
                        tile.TileData = tileData[i];
                        tile.TileData.Position = tileBounds.min;
                        tile.TileData.Size = tileBounds.size;
                        tileRegions[i] = new StormTileRegion();
                        if (editorSettings.IngestCreateFeatures)
                            tile.TileRegions = tileRegions[i];
                        tiles[i] = tile;
                    }
                }
                //Debug.Log("C");
                if (targetObject == null)
                    yield return StormInternalUtils.IngestObjects(tileData, tileRegions, dataParent, editorSettings.IngestCaptureAlphamaps, editorSettings.IngestCaptureDisabled, editorSettings.IngestCreateFeatures, editorSettings.IngestCreateFeatures && editorSettings.IngestCreateFeaturesets);
                else
                    yield return StormInternalUtils.IngestObject(tileData, tileRegions, dataParent, targetObject, editorSettings.IngestCaptureAlphamaps, editorSettings.IngestCaptureDisabled, editorSettings.IngestCreateFeatures, editorSettings.IngestCreateFeatures && editorSettings.IngestCreateFeaturesets);
                //Debug.Log("D");
                if (editorSettings.IngestCreateFeatures && editorSettings.IngestCreateFeaturesets && editorSettings.GlobalRegion != null)
                {
                    List<StormFeatureset> featuresets = new List<StormFeatureset>();
                    for (int t = 0; t < tiles.Length; ++t)
                    {
                        for (int r = 0; r < tiles[t].TileRegions?.Regions?.Length; ++r)
                        {
                            featuresets.AddRange(tiles[t].TileRegions.Regions[r].Featuresets);
                        }
                    }
                    string grPath = StormUtils.GetAssetPath(editorSettings.GlobalRegion);
                    if (!string.IsNullOrEmpty(grPath))
                    {
                        string gfPath = StormUtils.GetAssetPath(editorSettings.GlobalFeatureset);
                        if (string.IsNullOrEmpty(gfPath))
                        {
                            gfPath = grPath.Substring(0, grPath.Length - Path.GetFileName(grPath).Length);
                            gfPath += StormUtils.EnsureUniqueName(gfPath, editorSettings.GlobalRegion.name + "_Featureset", "asset");
                            editorSettings.GlobalFeatureset = StormUtils.SaveAsset(editorSettings.GlobalFeatureset, gfPath);
                            StormUtils.SetAssetDirty(editorSettings.GlobalRegion);
                            Debug.LogWarning("Embedded Featuresets do not persist inside of a Region asset.\nYour featureset has been saved at the path: " + gfPath, editorSettings.GlobalFeatureset);
                        }
                    }

                    //Debug.Log("E");
                    StormInternalUtils.CombineFeaturesets(editorSettings.GlobalFeatureset, featuresets, editorSettings.GlobalRegionAddReq);
                    //remove null or empty regions, featuresets, and features
                    for (int i = 0; i < tiles.Length; ++i)
                    {
                        for (int r = 0; r < tiles[i].TileRegions?.Regions?.Length; ++r)
                        {
                            for (int fs = 0; fs < tiles[i].TileRegions.Regions[r].Featuresets?.Length; ++fs)
                            {
                                for (int f = 0; f < tiles[i].TileRegions.Regions[r].Featuresets[fs].Features?.Length; ++f)
                                {
                                    if (tiles[i].TileRegions.Regions[r].Featuresets[fs].Features[f].Feature == null)
                                    {
                                        StormUtils.RemoveAt(ref tiles[i].TileRegions.Regions[r].Featuresets[fs].Features, f--);
                                    }
                                }
                                if (tiles[i].TileRegions.Regions[r].Featuresets[fs].Features?.Length == 0)
                                {
                                    StormUtils.RemoveAt(ref tiles[i].TileRegions.Regions[r].Featuresets, fs--);
                                }
                            }
                            if (tiles[i].TileRegions.Regions[r].Featuresets.Length == 0)
                            {
                                StormUtils.RemoveAt(ref tiles[i].TileRegions.Regions, r--);
                            }
                        }
                    }
                    StormWorld world = StormWorld.Instance;
                    if (world != null)
                        StormUtils.AddIfNew(world.Regions, editorSettings.GlobalRegion);
                }
                for (int i = 0; i < tiles.Length; ++i)
                {
                    StormTile tile = tiles[i];
                    if (!tile.TileData.HasData())
                        DestroyImmediate(tile.gameObject);
                    else
                    {
                        Undo.RegisterCreatedObjectUndo(tile, "Created Tile " + tile.name);
                        StormUtils.SetAssetDirty(tile);
                        tile.gameObject.SetActive(true); //now enable the go so the tile will initalize
                    }
                }
            }
            else if (editorSettings.IngestTargetType == 1)
            {
                //Debug.Log("F");
                //ingest to Files ... create StormTileData objects without the Tile container
                if (string.IsNullOrWhiteSpace(editorSettings.IngestFilePath))
                {
                    GUIContent dialog = m_editorUtils.GetContent("IngestNoPath");
                    EditorUtility.DisplayDialog(dialog.text, dialog.tooltip, m_editorUtils.GetTextValue("OKButton"));
                    yield break;
                }
                StormTileRegion regionTile = new StormTileRegion();
                string FSpath = editorSettings.IngestFilePath;
                if (!FSpath.EndsWith('/'))
                    FSpath += "/";
                FSpath += "Featureset.asset";
                StormFeatureset featureset = AssetDatabase.LoadAssetAtPath<StormFeatureset>(FSpath);
                if (featureset != null)
                    regionTile.Regions = new StormRegion[] {
                            new StormRegion() {
                                Featuresets = new StormFeatureset[] { featureset },
                            }
                        };
                for (int i = 0; i < m_ingestTileBounds.Length; ++i)
                {
                    Bounds tileBounds = m_ingestTileBounds[i];
                    StormTileData newTile = StormInternalUtils.CreateTileData(tileBounds.min, tileBounds.size, regionTile, editorSettings.IngestCaptureAlphamaps, m_ingestObject == null, editorSettings.IngestCaptureDisabled, editorSettings.IngestCreateFeatures, editorSettings.IngestCreateFeatures && editorSettings.IngestCreateFeaturesets);
                    newTile.Position = tileBounds.min;
                    newTile.Size = tileBounds.size;
                    StormInternalUtils.ExportTileData(newTile, "Tile_" + i, editorSettings.IngestSingleFile, editorSettings.IngestRawAsTexture, editorSettings.IngestFilePath);
                }
                if (regionTile.Regions[0] != null)
                    StormUtils.SaveAsset(regionTile.Regions[0], FSpath);
            }
            //Debug.Log("G");
        }
        private bool CheckBounds(GameObject go, out Bounds bounds, float stepProgress = 1f)
        {
            bounds = new Bounds(Vector3.zero, Vector3.zero);
            if (go == null)
            {
                bool isFirst = true;
                int numScenes = SceneManager.sceneCount;
                float sceneProgress = 1f / numScenes;
                for (int s = 0; s < numScenes; ++s)
                {
                    Scene scn = SceneManager.GetSceneAt(s);
                    if (!scn.isLoaded)
                        continue;
                    GameObject[] objs = scn.GetRootGameObjects();
                    int numObjs = objs.Length;
                    float objProgress = 1f / numObjs / sceneProgress;
                    for (int o = 0; o < numObjs; ++o)
                    {
                        if (!StormEditorSettings.Instance.IngestCaptureDisabled && !objs[o].activeSelf)
                            continue;
                        if (objs[o].isStatic)
                        {
                            if (!StormSettings.IngestStatic)
                                continue;
                        }
                        else
                        {
                            if (!StormSettings.IngestNonStatic)
                                continue;
                        }
                        bool r;
                        if (isFirst)
                        {
                            r = CheckBounds(objs[o], out bounds, objProgress);
                            isFirst = false;
                        }
                        else
                            r = CheckBoundsRecursive(objs[o], ref bounds, (s * sceneProgress) + (o * objProgress), objProgress);
                        if (!r)
                        {
                            EditorUtility.ClearProgressBar();
                            return false;
                        }
                    }
                }
                EditorUtility.ClearProgressBar();
                return true;
            }
            else
            {
                bool r = CheckBoundsRecursive(go, ref bounds, 0, stepProgress);
                if (!r || stepProgress == 1f)
                    EditorUtility.ClearProgressBar();
                return r;
            }
        }
        private bool CheckAllBounds(GameObject go, out Bounds bounds, float stepProgress = 1f)
        {
            bool first = false;
            bounds = new Bounds(go.transform.position, Vector3.zero);
            bool r = CheckAllBoundsRecursive(go, ref bounds, ref first);
            return r;
        }
        private Bounds[] CalcTiles(Bounds objectBounds, List<StormTile> tiles = null)
        {
            Vector3 ingestSize = objectBounds.size;
            Vector3 ingestPos = objectBounds.min;
            int countX = Mathf.Max(1, Mathf.CeilToInt(ingestSize.x / StormEditorSettings.Instance.IngestTileSize));
            int countZ = Mathf.Max(1, Mathf.CeilToInt(ingestSize.z / StormEditorSettings.Instance.IngestTileSize));
            Vector3 tileSize = new Vector3(ingestSize.x / countX, ingestSize.y, ingestSize.z / countZ);
            Bounds[] ret = new Bounds[countZ * countX];
            Vector3 offset = new Vector3(0, ingestPos.y + (0.5f * tileSize.y), 0);
            for (int z = 0; z < countZ; ++z)
            {
                offset.z = ingestPos.z + ((0.5f + z) * tileSize.z);
                for (int x = 0; x < countX; ++x)
                {
                    offset.x = ingestPos.x + ((0.5f + x) * tileSize.x);
                    ret[z * countX + x] = new Bounds(offset, tileSize);
                }
            }
            return ret;
        }
        private bool CheckBoundsRecursive(GameObject go, ref Bounds b, float startProgress, float stepProgress, int depth = 0)
        {
            //Debug.Log("Checking " + go.name, go);
            if (depth > 50)
                return true;
            if (EditorUtility.DisplayCancelableProgressBar("Storm", "Getting Ingestion Bounds. Checking " + go.name, startProgress))
                return false;
            if (!StormEditorSettings.Instance.IngestCaptureDisabled && !go.activeSelf)
                return true;
            if (go.isStatic)
            {
                if (!StormSettings.IngestStatic)
                    return true;
            }
            else
            {
                if (!StormSettings.IngestNonStatic)
                    return true;
            }
            Transform t = go.transform;
            UnityEngine.Terrain ter = go.GetComponent<UnityEngine.Terrain>();
            if (ter != null && ter.terrainData != null)
            {
                //Debug.Log("Found Terrain", ter);
                Vector3 tSize = ter.terrainData.size;
                Encapsulate(ref b, new Bounds(ter.GetPosition() + Vector3.Scale(tSize, new Vector3(0.5f, 0.5f, 0.5f)), tSize));
            }
            else if (StormUtils.AnyMatch(StormSettings.MeshTerrainTags, go.tag))
            {
                //Debug.Log("Found MeshTerrain by tag:" + go.tag, go);
                MeshFilter mf = go.GetComponent<MeshFilter>();
                if (mf != null && mf.sharedMesh != null)
                {
                    Bounds mBounds = mf.sharedMesh.bounds;
                    Vector3 goScale = t.lossyScale;
                    
                    mBounds.extents = Vector3.Scale(mBounds.extents, goScale);
                    mBounds.center = Vector3.Scale(mBounds.center, goScale);
                    mBounds.center += t.position;
                    Encapsulate(ref b, mBounds);
                }
            }
            else if (StormUtils.IsPrefab(go))
            {
                if (!StormUtils.AnyMatch(StormSettings.IgnoredPrefabs, StormUtils.GetPrefab(go)))
                {
                    Renderer[] renderers = go.GetComponentsInChildren<Renderer>();
                    foreach (Renderer r in renderers)
                        if (r.GetComponent<MeshFilter>() != null)
                            Encapsulate(ref b, r.bounds);
                }
                return true;
            }
            float childProgress = stepProgress / t.childCount;
            for (int c = 0; c < t.childCount; ++c)
            {
                if (!CheckBoundsRecursive(t.GetChild(c).gameObject, ref b, startProgress + (c * childProgress), childProgress, depth + 1))
                    return false;
            }
            return true;
        }
        void Encapsulate(ref Bounds b, Bounds other)
        {
            if (b.size == Vector3.zero)
                b = other;
            else
                b.Encapsulate(other);
        }
        private bool CheckAllBoundsRecursive(GameObject go, ref Bounds b, ref bool first, int depth = 0)
        {
            if (depth > 50)
                return true;
            Transform t = go.transform;
            UnityEngine.Terrain ter = go.GetComponent<UnityEngine.Terrain>();
            if (ter != null)
            {
                Vector3 tSize = ter.terrainData.size;
                if (first)
                {
                    first = false;
                    b = new Bounds(ter.GetPosition() + Vector3.Scale(tSize, new Vector3(0.5f, 0.5f, 0.5f)), tSize);
                }
                else
                    b.Encapsulate(new Bounds(ter.GetPosition() + Vector3.Scale(tSize, new Vector3(0.5f, 0.5f, 0.5f)), tSize));
            }
            Collider col = go.GetComponent<Collider>();
            if (col != null)
            {
                if (first)
                {
                    b = col.bounds;
                    first = false;
                }
                else
                    b.Encapsulate(col.bounds);
            }
            for (int c = 0; c < t.childCount; ++c)
            {
                if (!CheckAllBoundsRecursive(t.GetChild(c).gameObject, ref b, ref first, depth + 1))
                    return false;
            }
            return true;
        }
        private void UpdateDataTiles()
        {
            if (updateTiles)
            {
                var tiles = StormUtils.FindObjectsOfType<StormTile>(true);
                if (m_dataTiles.Count > tiles.Length)
                    m_dataTiles.RemoveRange(tiles.Length, m_dataTiles.Count - tiles.Length);
                for (int t = 0; t < tiles.Length; ++t)
                {
                    if (m_dataTiles.Count <= t)
                        m_dataTiles.Add(tiles[t]);
                    else
                        m_dataTiles[t] = tiles[t];
                }
                updateTiles = false;
            }
        }
        private void LoadBuildSettingScenes(List<SceneAsset> theList, ReorderableList reorderableList)
        {
            bool ignoreMenuSplashScreen = false;
            if (EditorUtility.DisplayDialog("Load Menu And Splash Screens!", "Would you like to ignore scenes that contains the name 'Menu' and Splash Screen'?", "Yes Please", "No Thanks"))
            {
                ignoreMenuSplashScreen = true;
            }

            EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes;
            if (scenes.Length < 1)
            {
                EditorUtility.DisplayDialog("No Scenes Found!", "It seems that you have no scenes configured in your build settings. Make sure you assign them and then try again.", "Ok");
                return;
            }
            foreach (EditorBuildSettingsScene scene in scenes)
            {
                SceneAsset sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(scene.path);
                if (sceneAsset != null)
                {
                    if (ignoreMenuSplashScreen)
                    {
                        if (!sceneAsset.name.ToUpper().Contains("MENU") && !sceneAsset.name.ToUpper().Contains("SPLASH SCREEN") && !sceneAsset.name.ToUpper().Contains("SPLASHSCREEN"))
                        {
                            if (!theList.Contains(sceneAsset))
                            {
                                theList.Add(sceneAsset);
                            }
                        }
                    }
                    else
                    {
                        if (!theList.Contains(sceneAsset))
                        {
                            theList.Add(sceneAsset);
                        }
                    }
                }
            }

            reorderableList.list = theList;
        }
        private void LoadCurrentScenes(List<SceneAsset> theList, ReorderableList reorderableList)
        {
            int numScenes = SceneManager.sceneCount;
            for (int s = 0; s < numScenes; ++s)
            {
                Scene scene = SceneManager.GetSceneAt(s);
                if (!scene.isLoaded)
                    continue;

                SceneAsset sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(scene.path);
                if (sceneAsset != null && !theList.Contains(sceneAsset))
                {
                    theList.Add(sceneAsset);
                }
            }
        }
#if GAIA_PRO_PRESENT || GAIA_2023_PRO
        private void LoadGaiaScenes(List<SceneAsset> theList, ReorderableList reorderableList, Gaia.TerrainLoaderManager terrainLoader)
        {
            if (terrainLoader != null && terrainLoader.TerrainSceneStorage != null && terrainLoader.TerrainSceneStorage.m_terrainScenes != null)
            {
                foreach (Gaia.TerrainScene gaiaScene in terrainLoader.TerrainSceneStorage.m_terrainScenes)
                {
                    if (gaiaScene != null)
                    {
                        SceneAsset sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(gaiaScene.m_scenePath);
                        if (sceneAsset != null && !theList.Contains(sceneAsset))
                        {
                            theList.Add(sceneAsset);
                        }
                    }
                }
            }
        }
#endif

        private void CalcDataTiles(out Bounds dataBounds)
        {
            StormWorld world = StormWorld.Instance;
            if (world == null)
            {
                dataBounds = new Bounds(Vector3.zero, Vector3.zero);
                return;
            }
            dataBounds = new Bounds();
            bool first = true;
            var tileData = StormDataCache.AllDataTiles;
            foreach (var tile in tileData)
            {
                if (first)
                {
                    first = false;
                    dataBounds.center = tile.Data.Position + (tile.Data.Size * 0.5f);
                    dataBounds.size = tile.Data.Size;
                }
                else
                {
                    dataBounds.Encapsulate(tile.Data.Position);
                    dataBounds.Encapsulate(tile.Data.Position + tile.Data.Size);
                }
            }
        }
#endregion
        #region Collider Bake Functions
        private void CalcBakeFeaturesets()
        {
            m_bakeFeatureCount = 0;
            foreach (StormFeatureset fs in m_bakeFeaturesets)
            {
                if (fs != null && fs.Features != null)
                {
                    foreach (StormFeatureItem item in fs.Features)
                    {
                        if (item.Feature != null && item.Feature.Colliders.Length > 0)
                            ++m_bakeFeatureCount;
                    }
                }
            }
        }
        void UpdateBake()
        {
            if (m_multiSceneBake)
            {
                if (m_needsBake)
                {
                    m_needsBake = false;
                    m_isBaking = true;
                    m_bakeSceneProgress = 0;
                    StartBakeScene();
                }
                if (m_isBaking)
                {
                    if (m_bakeProgress == null || m_bakeProgress.IsDone || StormEditorProgress.Cancelled)
                    {
                        if (m_bakeSceneProgress >= m_bakeScenesList.Count || StormEditorProgress.Cancelled)
                        {
                            if (StormWorld.Instance != null)
                                StormWorld.Instance.ForceFullRefresh(true);
                            StormEditorProgress.Clear();
                            m_isBaking = false;
                            FinishBakeScene();
                        }
                        else
                        {
                            UpdateBakeScene();
                        }
                    }
                    else
                    {
                        if (++m_bakeProgress_timeout > 300)
                        {
                            if (m_bakeProgress_hasRetried)
                            {
                                Debug.Log("Bake progress appears halted ... Retrying scene");
                                m_bakeProgress_hasRetried = true;
                                StormUtils.Destroy(m_bakingGameObject);
                                --m_bakeSceneProgress;
                                m_bakeProgress_timeout = 0;
                                UpdateBakeScene();
                            }
                            else
                            {
                                Debug.Log("Bake progress appears halted ... Continuing to next scene\nScene:" + m_bakingGameObject.name + " Completed " + m_bakeProgress.NumberDone + " of " + m_bakeProgress.NumberTotal, m_bakeScenesList[m_bakeSceneProgress]);
                                m_bakeProgress_hasRetried = false;
                                m_bakeProgress_timeout = 0;
                                UpdateBakeScene();
                            }
                        }
                        StormUtils.ForceEditorSceneviewRedraw(StormWorld.Instance == null ? null : StormWorld.Instance.gameObject);
                        StormEditorProgress.UpdateSubValue("set " + m_bakeProgress.NumberDone + " of " + m_bakeProgress.NumberTotal, m_bakeProgress.NumberDone);
                        Repaint();
                    }
                }
            }
            else
            {
                if (m_needsBake)
                {
                    m_needsBake = false;
                    Vector3 min = m_bakeBounds.min;
                    Vector3 size = m_bakeBounds.size;
                    m_bakeProgress = StormInternalUtils.BakeColliders(new Vector2(min.x, min.z), new Vector2(size.x, size.z), StormEditorSettings.Instance.BakeTileSize, m_bakeFeaturesets.ToArray(), StormUtils.GetDataLayerInfo(m_bakeHeightLayers, StormLayerType.NONE), StormEditorSettings.Instance.BakePrefab, m_bakeTarget);
                    if (m_bakeProgress != null)
                    {
                        m_bakeProgress.OnUpdate += OnUpdateBakeProgress;
                        StormEditorProgress.Show("Baking Colliders", "Step", m_bakeProgress.NumberTotal);
                        m_isBaking = true;
                    }
                }
                if (m_isBaking)
                {
                    if (m_bakeProgress == null || m_bakeProgress.IsDone || StormEditorProgress.Cancelled)
                    {
                        if (StormWorld.Instance != null)
                            StormWorld.Instance.ForceFullRefresh();
                        StormEditorProgress.Clear();
                        m_isBaking = false;
                    }
                    else
                    {
                        StormUtils.ForceEditorSceneviewRedraw(StormWorld.Instance == null ? null : StormWorld.Instance.gameObject);
                        StormEditorProgress.Update(m_bakeProgress.NumberDone, true);
                        Repaint();
                    }
                }
            }
        }
        void OnUpdateBakeProgress()
        {
            m_bakeProgress_timeout = 0;
        }
        void StartBakeScene()
        {
            m_bakingAreaScenes.Clear();
            EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
            m_baking_curScenes = new string[SceneManager.sceneCount];
            m_baking_curScenesActive = new bool[SceneManager.sceneCount];
            m_baking_curOpenScenes = new Scene[SceneManager.sceneCount];
            m_baking_activeScene = EditorSceneManager.GetActiveScene();
            m_baking_activeSceneIdx = 0;
            for (int s = 0; s < m_baking_curScenes.Length; ++s)
            {
                m_baking_curOpenScenes[s] = SceneManager.GetSceneAt(s);
                if (m_baking_curOpenScenes[s].path == m_baking_activeScene.path)
                    m_baking_activeSceneIdx = s;
                m_baking_curScenes[s] = m_baking_curOpenScenes[s].path;
                m_baking_curScenesActive[s] = m_baking_curOpenScenes[s].isLoaded;
            }
            if (StormEditorSettings.Instance.BakeSaveType == 0) //single
            {
                if (StormEditorSettings.Instance.BakeSaveScene == null)
                {
                    m_bakingSaveScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
                    if (StormEditorSettings.Instance.BakeSaveFrequency >= 0)
                        EditorSceneManager.SaveScene(m_bakingSaveScene);
                }
                else
                {
                    m_bakingSaveScene = EditorSceneManager.OpenScene(AssetDatabase.GetAssetPath(StormEditorSettings.Instance.BakeSaveScene), OpenSceneMode.Single);
                }
            }
            else if (StormEditorSettings.Instance.BakeSaveType == 2)//area based
            {
                m_bakingSaveScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            }
            m_baking_createdWorld = StormWorld.Instance == null;
            if (m_baking_createdWorld)
                new GameObject("StormWorld", typeof(UI.StormWorldManager));
            StormEditorProgress.Show("Baking Colliders", "Scene: ", m_bakeScenesList.Count);
            StormEditorProgress.ShowProgress = false;
            m_bakeSceneProgress = -1;
            UpdateBakeScene();
        }
        void UpdateBakeScene()
        {
            StormEditorProgress.ClearSubValue();
            StormEditorProgress.Value = ++m_bakeSceneProgress;
            if (m_bakingScene.IsValid() && m_bakingScene.isLoaded)
            {
                EditorSceneManager.CloseScene(m_bakingScene, true);
            }
            if (m_bakeSceneProgress >= m_bakeScenesList.Count)
                return;
            if (StormEditorSettings.Instance.BakeSaveType == 0) //single
            {
                //check if we need to save our bake scene
                if (m_bakeSceneProgress % StormEditorSettings.Instance.BakeSaveFrequency == 0)
                    EditorSceneManager.SaveScene(m_bakingSaveScene);
            }
            else if (StormEditorSettings.Instance.BakeSaveType == 2) //area based
            {
                UpdateAreaBake();
            }
            SceneAsset sceneAsset = m_bakeScenesList[m_bakeSceneProgress];
            if (sceneAsset == null)
                return;
            string scenePath = AssetDatabase.GetAssetPath(sceneAsset);
            m_bakingScene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);
            StormEditorProgress.Message = "Scene: (" + m_bakeSceneProgress + " of " + m_bakeScenesList.Count + ") " + m_bakingScene.name;
            StormWorld.Instance.ForceFullRefresh(true);
            CalcDataTiles(out Bounds sceneBounds);
            Vector3 min = sceneBounds.min;
            Vector3 size = sceneBounds.size;

            if (StormEditorSettings.Instance.BakeSaveType == 1) //one each
            {
                if (m_bakingSaveScene.IsValid() && m_bakingSaveScene.isLoaded)
                {
                    EditorSceneManager.SaveScene(m_bakingSaveScene);
                    EditorSceneManager.CloseScene(m_bakingSaveScene, true);
                }
                string path = StormEditorSettings.Instance.BakeSavePath.Replace('\\', '/');
                if (!path.EndsWith('/'))
                    path += "/";
                path += StormEditorSettings.Instance.BakeSavePrefix + m_bakingScene.name + ".unity";
                StormUtils.EnsureDirExists(path.Substring(0, path.LastIndexOf('/')));
                if (File.Exists(path))
                    m_bakingSaveScene = EditorSceneManager.OpenScene(AssetDatabase.GetAssetPath(StormEditorSettings.Instance.BakeSaveScene), OpenSceneMode.Additive);
                else
                {
                    m_bakingSaveScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
                    EditorSceneManager.SaveScene(m_bakingSaveScene, path);
                }
                EditorSceneManager.SetActiveScene(m_bakingSaveScene);
            }

            m_bakingGameObject = new GameObject(m_bakingScene.name);
            m_bakingGameObject.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            m_bakingGameObject.transform.localScale = Vector3.one;
            m_bakeProgress = StormInternalUtils.BakeColliders(new Vector2(min.x, min.z), new Vector2(size.x, size.z), StormEditorSettings.Instance.BakeTileSize, m_bakeFeaturesets.ToArray(), StormUtils.GetDataLayerInfo(m_bakeHeightLayers, StormLayerType.NONE), StormEditorSettings.Instance.BakePrefab, m_bakingGameObject.transform);
            if (m_bakeProgress != null)
            {
                m_bakeProgress.OnUpdate += OnUpdateBakeProgress;
                StormEditorProgress.UpdateSubValue(0, m_bakeProgress.NumberTotal, true);
                m_isBaking = true;
            }
        }
        void UpdateAreaBake()
        {
            //move objects into the correct scene
            if (m_bakingGameObject != null)
            {
                int bakedChildren = m_bakingGameObject.transform.childCount;
                for (int c = 0; c < bakedChildren; ++c)
                {
                    Transform child = m_bakingGameObject.transform.GetChild(0);
                    CheckAllBounds(m_bakingGameObject, out Bounds bounds);
                    Vector3 center = bounds.center;
                    int2 tileID = new int2(Mathf.RoundToInt(center.x / StormEditorSettings.Instance.BakeSaveSceneSize), Mathf.RoundToInt(center.z / StormEditorSettings.Instance.BakeSaveSceneSize));
                    if (!m_bakingAreaScenes.TryGetValue(tileID, out Scene scene))
                    {
                        string path = StormEditorSettings.Instance.BakeSavePath.Replace('\\', '/');
                        if (!path.EndsWith('/'))
                            path += "/";
                        path += StormEditorSettings.Instance.BakeSavePrefix + "." + tileID.x + "_" + tileID.y + ".unity";
                        StormUtils.EnsureDirExists(path);
                        if (File.Exists(path))
                        {
                            scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
                        }
                        else
                        {
                            EditorSceneManager.SaveScene(m_bakingSaveScene, path);
                            scene = m_bakingSaveScene;
                            m_bakingSaveScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
                            GameObject[] rootObjects = scene.GetRootGameObjects();
                            foreach (GameObject go in rootObjects)
                                EditorSceneManager.MoveGameObjectToScene(go, m_bakingSaveScene);
                        }
                        m_bakingAreaScenes.Add(tileID, scene);
                    }
                    GameObject targetGO = null;
                    GameObject[] sceneGOs = scene.GetRootGameObjects();
                    for (int s = 0; s < sceneGOs.Length; ++s)
                    {
                        GameObject sceneChild = sceneGOs[s];
                        if (sceneChild.name == m_bakingGameObject.name)
                        {
                            for (int sc = 0; sc < sceneChild.transform.childCount; ++sc)
                            {
                                Transform subChild = sceneChild.transform.GetChild(sc);
                                if (subChild.name == child.name)
                                {
                                    //tile go already exists. move all children onto it
                                    targetGO = subChild.gameObject;
                                    int numChildren = targetGO.transform.childCount;
                                    while (numChildren-- > 0)
                                    {
                                        Transform targetChild = targetGO.transform.GetChild(0);
                                        targetChild.SetParent(null, true);
                                        EditorSceneManager.MoveGameObjectToScene(targetChild.gameObject, scene);
                                        targetChild.SetParent(targetGO.transform, true);
                                    }
                                    break;
                                }
                            }
                            if (targetGO == null)
                            {
                                //no target found. just move the whole object
                                child.SetParent(null, false);
                                targetGO = child.gameObject;
                                EditorSceneManager.MoveGameObjectToScene(targetGO, scene);
                                child.SetParent(sceneChild.transform, false);
                                child.transform.SetPositionAndRotation(Vector3.zero, quaternion.identity);
                                child.transform.localScale = Vector3.one;
                            }
                            break;
                        }
                    }
                    if (targetGO == null)
                    {
                        //no object found for this scene. just make a new one
                        GameObject sceneChild = new GameObject(m_bakingGameObject.name);
                        EditorSceneManager.MoveGameObjectToScene(sceneChild, scene);
                        sceneChild.transform.SetPositionAndRotation(Vector3.zero, quaternion.identity);
                        sceneChild.transform.localScale = Vector3.one;
                        child.SetParent(sceneChild.transform, false);
                        child.transform.SetPositionAndRotation(Vector3.zero, quaternion.identity);
                        child.transform.localScale = Vector3.one;
                    }
                }
                if (m_bakingGameObject.transform.childCount > 0)
                    Debug.LogError("There are still children on the Baking object. This should not happen.", m_bakingGameObject);
                else
                    StormUtils.Destroy(m_bakingGameObject);
            }
            foreach (KeyValuePair<int2, Scene> scene in m_bakingAreaScenes)
            {
                EditorSceneManager.SaveScene(scene.Value);
                EditorSceneManager.CloseScene(scene.Value, true);
            }
            m_bakingAreaScenes.Clear();
        }
        void FinishBakeScene()
        {
            StormEditorProgress.ClearSubValue(true);
            StormEditorProgress.Clear();
            if (StormEditorSettings.Instance.BakeSaveType == 2) //area based
            {
                UpdateAreaBake();
            }
            if (m_bakingSaveScene.IsValid() && m_bakingSaveScene.isLoaded)
            {
                if (StormEditorSettings.Instance.BakeSaveType == 0) //single
                    EditorSceneManager.SaveScene(m_bakingSaveScene);
                else
                    EditorSceneManager.CloseScene(m_bakingSaveScene, true);
            }
            foreach (KeyValuePair<int2, Scene> scene in m_bakingAreaScenes)
            {
                EditorSceneManager.SaveScene(scene.Value);
                EditorSceneManager.CloseScene(scene.Value, true);
            }
            m_bakingAreaScenes.Clear();
            for (int s = 0; s < m_baking_curScenes.Length; ++s)
            {
                if (!string.IsNullOrEmpty(m_baking_curScenes[s]) && File.Exists(m_baking_curScenes[s]))
                    m_baking_curOpenScenes[s] = EditorSceneManager.OpenScene(m_baking_curScenes[s], m_baking_curScenesActive[s] ? OpenSceneMode.Additive : OpenSceneMode.AdditiveWithoutLoading);
            }
            if (m_bakingScene.IsValid() && m_bakingScene.isLoaded)
                EditorSceneManager.CloseScene(m_bakingScene, true);
            if (m_baking_activeSceneIdx >= 0 && m_baking_activeSceneIdx < m_baking_curOpenScenes.Length && m_baking_curOpenScenes[m_baking_activeSceneIdx].IsValid())
                EditorSceneManager.SetActiveScene(m_baking_curOpenScenes[m_baking_activeSceneIdx]);
            if (m_baking_createdWorld && StormWorld.Instance != null)
                StormUtils.Destroy(StormWorld.Instance.gameObject);
        }
        #endregion
        #region Plugins
        GUIContent m_embeddedIcon;
        GUIContent m_windowedIcon;
        GUIContent EmbeddedIcon
        {
            get {
                if (m_embeddedIcon == null || m_embeddedIcon.image == null)
                {
#if UNITY_6000_0_OR_NEWER
                    m_embeddedIcon = EditorGUIUtility.IconContent("d_ScaleTool");
#else
                    m_embeddedIcon = EditorGUIUtility.IconContent("d_winbtn_win_max");
#endif
                }
                return m_embeddedIcon;
            }
        }
        GUIContent WindowedIcon
        {
            get {
                if (m_windowedIcon == null || m_windowedIcon.image == null)
                {
#if UNITY_6000_0_OR_NEWER
                    m_windowedIcon = EditorGUIUtility.IconContent("InProject");
#else
                    m_windowedIcon = EditorGUIUtility.IconContent("d_winbtn_win_restore");
#endif
                }
                return m_windowedIcon;
            }
        }
        void PluginPanel(PluginEditorData plugin)
        {
            EditorGUILayout.BeginVertical(m_editorUtils.Styles.panelFrame);
            {
                EditorGUILayout.BeginHorizontal();
                {
                    bool isExpanded = plugin.m_isExpanded.target;
                    bool isEmbedded = !plugin.IsWindowOpened;
                    isExpanded = GUILayout.Toggle(isExpanded, isExpanded ? "-" : "+", m_editorUtils.Styles.panelLabel, GUILayout.ExpandWidth(false));
                    isExpanded = GUILayout.Toggle(isExpanded, plugin.Editor.Label, m_editorUtils.Styles.panelLabel, GUILayout.ExpandWidth(true));
                    bool nowIsEmbedded = GUILayout.Toggle(isEmbedded, isEmbedded ? EmbeddedIcon : WindowedIcon, EditorStyles.iconButton, GUILayout.ExpandWidth(false));
                    if (nowIsEmbedded != isEmbedded)
                    {
                        if (nowIsEmbedded)
                        {
                            plugin.Editor.HideWindow();
                        }
                        else
                        {
                            plugin.m_isExpanded.target = false;
                            isExpanded = false;
                            plugin.Editor.OpenWindow();
                        }
                    }
                    if (isExpanded != plugin.m_isExpanded.target)
                    {
                        plugin.m_isExpanded.target = isExpanded;
                        if (isExpanded && !isEmbedded)
                        {
                            plugin.Editor.HideWindow();
                        }
                    }
                }
                EditorGUILayout.EndHorizontal();
                if (EditorGUILayout.BeginFadeGroup(plugin.m_isExpanded.faded))
                {
                    EditorGUILayout.Space();
                    plugin.Editor.OnGUI();
                }
                EditorGUILayout.EndFadeGroup();
            }
            EditorGUILayout.EndVertical();
        }
        #endregion
    }
}