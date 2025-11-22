using ProceduralWorlds.Storm.Core;
using ProceduralWorlds.Storm.Utils;
using PWCommon5;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.Mathematics;
using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Events;

/// Copyright 2023 Procedural Worlds Ltd
/// Written by Jonathan Williams and Josh Savage as part of Storm

namespace ProceduralWorlds.Storm.StormEditor
{
    [CustomEditor(typeof(StormWorld), true)]
    public class StormWorldEditor : PWEditor
    {
        #region Variables
        public bool IsFromMainWindow = false;
        public Action ParentWindowRepaint = null;

        private EditorUtils m_editorUtils;
        private StormWorld m_stormWorld;
        private GUIStyle m_boxStyle;
        private Texture2D m_headerBackground;
        private GUIStyle m_headerStyle;

        private GUIContent AddButtonContent;
        private GUIContent RemoveButtonContent;
        private GUIContent EnableButtonContent;
        private GUIContent DisableButtonContent;

        private List<StormLayerRemap> m_worldRemaps;
        private StormScriptableUI<StormLayerRemap>[] m_worldRemapUIs = new StormScriptableUI<StormLayerRemap>[0];
        bool IsPickingRemap;
        int pickingRemapIndex;

        private List<StormRegion> m_worldRegions;
        private StormScriptableUI<StormRegion>[] m_worldRegionUIs = new StormScriptableUI<StormRegion>[0];
        private StormScriptableUI<StormRegion>[] m_tileRegionUIs = new StormScriptableUI<StormRegion>[0];
        private string m_newRegionName;

        private ReorderableList m_heightNamesList;
        private List<StormSourceLayerInfo> m_heightNames;
        private bool m_heightNamesChanged = false;
        SerializedProperty m_heightNamesProp;

        private ReorderableList m_holeNamesList;
        private List<StormSourceLayerInfo> m_holeNames;
        private bool m_holeNamesChanged = false;
        SerializedProperty m_holeNamesProp;

        private ReorderableList m_cullingCamerasList;
        private List<Camera> m_cullingCameras;
        private bool m_cullingCamerasChanged = false;
        SerializedProperty m_cullingCamerasProp;

        AnimBool m_stormDebugExpanded;
        AnimBool[] m_featureExpanded = new AnimBool[0];

        StormScriptableUI<StormSystemSettings> m_systemSettingsUI;
        StormScriptableUI<StormQualitySettings> m_qualitySettingsUI;

        SerializedProperty m_systemSettingsProp;
        SerializedProperty m_qualitySettingsProp;

        private StormScriptableUI<StormColorTargetData>[] m_colorTargetUIs = new StormScriptableUI<StormColorTargetData>[0];

        //these are used to serialize the expansion of the panels
        SerializedProperty m_WorldSeedProp;
        SerializedProperty m_LayerRemapsProp;
        SerializedProperty m_RegionsProp;
        SerializedProperty m_UseSeaLevelProp;
        SerializedProperty m_m_worldRandomPositionProp;
        SerializedProperty m_SunLightProp;
        SerializedProperty m_worldOffsetProp;
        SerializedProperty m_GlobalLayersetProp;
        SerializedProperty m_DebugSettingsProp;
        bool WorldHelpActive;
        bool RemapsHelpActive;
        bool RegionsHelpActive;
        bool FeaturesHelpActive;
        bool TilesHelpActive;
        bool ColorsHelpActive;
        bool AdvancedHelpActive;
        bool MemoryHelpActive;
        bool DebugHelpActive;


        GUIContent RandomizeButtonContent;
        GUIContent WarningIconContent;

        StormColorTargetData m_displayedTarget = null;

        #endregion
        #region Unity Functions

        /// <summary>
        /// Called on enable when the window starts up
        /// </summary>
        public void OnEnable()
        {
            StormUtils.EditorGUICallback -= DuringSceneGUI;
            StormUtils.EditorGUICallback += DuringSceneGUI;
            if (target == null)
                return;
            RandomizeButtonContent = EditorGUIUtility.IconContent("d_RotateTool On@2x");
            WarningIconContent = EditorGUIUtility.IconContent("console.warnicon");
            StormUtils.GetRenderPipline();
            m_stormDebugExpanded = new AnimBool(false, Repaint);
            m_editorUtils = StormApp.GetEditorUtils(this);

            BuildReorderableLists();
            
            m_heightNamesProp = serializedObject.FindProperty("m_heightNames");
            m_holeNamesProp = serializedObject.FindProperty("m_holeNames");
            m_cullingCamerasProp = serializedObject.FindProperty("m_cullingCameras");
            m_systemSettingsProp = serializedObject.FindProperty("m_systemSettings");
            m_qualitySettingsProp = serializedObject.FindProperty("m_qualitySettings");

            m_WorldSeedProp = serializedObject.FindProperty("m_worldSeed");
            m_LayerRemapsProp = serializedObject.FindProperty("m_layerRemaps");
            m_RegionsProp = serializedObject.FindProperty("m_regions");
            m_UseSeaLevelProp = serializedObject.FindProperty("UseGaiaSeaLevel");
            m_m_worldRandomPositionProp = serializedObject.FindProperty("m_worldRandomPosition");
            m_SunLightProp = serializedObject.FindProperty("SunLight");
            m_worldOffsetProp = serializedObject.FindProperty("m_worldOffset");
            m_GlobalLayersetProp = serializedObject.FindProperty("m_globalLayerSet");
            m_DebugSettingsProp = serializedObject.FindProperty("DebugSettings");

            AddButtonContent = EditorGUIUtility.IconContent("d_Toolbar Plus@2x");
            RemoveButtonContent = EditorGUIUtility.IconContent("d_Toolbar Minus@2x");
            EnableButtonContent = EditorGUIUtility.IconContent("d_scenevis_hidden_hover@2x");
            DisableButtonContent = EditorGUIUtility.IconContent("d_scenevis_visible_hover@2x");

            //Subscribe to Undo event
            Undo.undoRedoPerformed -= OnUndo;
            Undo.undoRedoPerformed += OnUndo;

            StormAPI.OnStormTileRegionRemoved -= OnRegionsChanged;
            StormAPI.OnStormTileRegionRemoved += OnRegionsChanged;
        }
        /// <summary>
        /// Called when the window is destroyed (closed)
        /// </summary>
        private void OnDestroy()
        {
            if (m_editorUtils != null)
            {
                m_editorUtils.Dispose();
            }

            Undo.undoRedoPerformed -= OnUndo;
        }
        /// <summary>
        /// Called when this object is deselected or disabled
        /// </summary>
        private void OnDisable()
        {
            StormAPI.OnStormTileRegionRemoved -= OnRegionsChanged;
            Undo.undoRedoPerformed -= OnUndo;
        }

        private void OnRegionsChanged(StormTileRegion region)
        {
            m_tileRegionUIs = new StormScriptableUI<StormRegion>[0];
        }

        public void OnSceneGUI()
        {
            StormLoaderData loaderData = m_stormWorld?.LoaderData;
            if (loaderData == null)
                return;
            
            Color startGizmoColor = Handles.color;
            int numTiles = loaderData.Tiles?.Length ?? 0;
            for (int t = 0; t < numTiles; ++t)
            {
                StormLoaderItem tile = loaderData.Tiles[t];
                Vector3 size = tile.Extents;
                size.y = 0f;
                Vector3 center = (Vector3)(loaderData.Position + tile.Offset) + (size * 0.5f);
                Handles.color = Color.cyan;
                Handles.DrawWireCube(center, size);
            }
            {
                Vector3 size = loaderData.Size;
                size.y = 0f;
                Vector3 center = (Vector3)loaderData.Position + (size * 0.5f);
                Handles.color = Color.magenta;
                Handles.DrawWireCube(center, size);
            }

            if (m_displayedTarget != null)
            {
                for (int t = 0; t < m_displayedTarget.UsedByTiles.Count; ++t)
                {
                    StormTileData tileData = m_displayedTarget.UsedByTiles[t];
                    Handles.color = new Color(1f, 0f, 1f, 0.3f);
                    Handles.DrawWireCube(tileData.Position + (tileData.Size * 0.5f), tileData.Size);
                }
            }

            Handles.color = startGizmoColor;
        }
        /// <summary>
        /// Overrides the OnInspectorGUI function call
        /// </summary>
        public override void OnInspectorGUI()
        {
            if (target is StormWorld world and not UI.StormWorldManager)
            {
                Debug.LogWarning("Found direct reference to DLL version of StormWorld. Changing reference to use StormWorldManager subclass.", world.gameObject);
                //target is wrong type. using Internal Core version. change it to use un-obfuscated script version
                GameObject go = world.gameObject;
                StormWorld newWorld = go.AddComponent<UI.StormWorldManager>();
                newWorld.DebugSettings = world.DebugSettings;
                newWorld.SystemSettings = world.SystemSettings;
                newWorld.QualitySettings = world.QualitySettings;
                newWorld.LayerRemaps = world.LayerRemaps;
                newWorld.CullingCameras = StormUtils.CopyArray(world.CullingCameras);
                newWorld.DrawOnOtherCams = world.DrawOnOtherCams;
                newWorld.OtherCamsUseIndex = world.OtherCamsUseIndex;
                newWorld.SunLight = world.SunLight;
                newWorld.WorldSeed = world.WorldSeed;
                newWorld.Regions = new List<StormRegion>(world.Regions);
                newWorld.GlobalLayerSet = world.GlobalLayerSet;
                newWorld.HeightNames = world.HeightNames;
                newWorld.HoleNames = world.HoleNames;
                newWorld.WorldOffset = world.WorldOffset;
                newWorld.WorldRandomPosition = world.WorldRandomPosition;
                newWorld.WorldRandomScale = world.WorldRandomScale;
                EditorUtility.SetDirty(go);
                DestroyImmediate(world);
                if (!IsFromMainWindow)
                    GUIUtility.ExitGUI();
                return;
            }

            if (m_editorUtils == null)
                m_editorUtils = StormApp.GetEditorUtils(this);
            m_editorUtils.Initialize();
            if (m_stormWorld == null)
            {
                m_stormWorld = (StormWorld)target;
                BuildReorderableLists();
            }

            //Set up the box style
            if (m_boxStyle == null)
            {
                m_boxStyle = new GUIStyle(GUI.skin.box);
                m_boxStyle.normal.textColor = GUI.skin.label.normal.textColor;
                m_boxStyle.fontStyle = FontStyle.Bold;
                m_boxStyle.alignment = TextAnchor.UpperLeft;
            }
            if (m_headerStyle == null)
            {
                m_headerStyle = new GUIStyle(EditorStyles.boldLabel);
                m_headerBackground = new Texture2D(2, 2);
                m_headerBackground.SetPixels32(new Color32[] { new Color(0, 0, 0, 0.25f), new Color(0, 0, 0, 0.25f), new Color(0, 0, 0, 0.25f), new Color(0, 0, 0, 0.25f), });
                m_headerBackground.Apply();
                m_headerStyle.normal.background = m_headerBackground;
                m_headerStyle.fontSize += 2;
                m_headerStyle.padding = new RectOffset(15,15,0,0);
            }
            EditorGUILayout.BeginVertical(m_boxStyle);
            m_editorUtils.Label("SettingsHeader", m_headerStyle);

            if (m_systemSettingsUI == null)
                m_systemSettingsUI = new StormScriptableUI<StormSystemSettings>(m_stormWorld.SystemSettings, "SystemSettings", IsFromMainWindow ? ParentWindowRepaint : Repaint, m_editorUtils, OnSystemSettingsChanged, (v) => m_systemSettingsProp.isExpanded = v, true, null, m_systemSettingsProp.isExpanded);
            if (m_qualitySettingsUI == null)
                m_qualitySettingsUI = new StormScriptableUI<StormQualitySettings>(m_stormWorld.QualitySettings, "QualitySettings", IsFromMainWindow ? ParentWindowRepaint : Repaint, m_editorUtils, OnQualitySettingsChanged, (v) => m_qualitySettingsProp.isExpanded = v, true, null, m_qualitySettingsProp.isExpanded);
            
            m_qualitySettingsUI.DoLayout();
            m_systemSettingsUI.DoLayout();


            bool WorldExpanded = !m_WorldSeedProp.isExpanded;
            bool RemapsExpanded = m_LayerRemapsProp.isExpanded;
            bool RegionsExpanded = m_RegionsProp.isExpanded;
            bool FeaturesExpanded = m_UseSeaLevelProp.isExpanded;
            bool ColorsExpanded = m_m_worldRandomPositionProp.isExpanded;
            bool TilesExpanded = m_SunLightProp.isExpanded;
            bool AdvancedExpanded = m_worldOffsetProp.isExpanded;
            bool MemoryExpanded = m_GlobalLayersetProp.isExpanded;
            bool DebugExpanded = m_DebugSettingsProp.isExpanded;

            m_editorUtils.Panel("GlobalSettings", WorldPanel, ref WorldExpanded, ref WorldHelpActive);
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
            EditorGUILayout.BeginVertical(m_boxStyle);
            m_editorUtils.Label("DataHeader", m_headerStyle);
            m_editorUtils.Panel("LayerRemaps", LayerRemapPanel, ref RemapsExpanded, ref RemapsHelpActive);
            if (!IsFromMainWindow)
            {
                m_editorUtils.Panel("Tiles", TilesPanel, ref TilesExpanded, ref TilesHelpActive);
                m_editorUtils.Panel("ColorsPanel", ColorsPanel, ref ColorsExpanded, ref ColorsHelpActive);
                //m_editorUtils.Panel("DataVisualizer", DataVisualizerPanel);
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
            EditorGUILayout.BeginVertical(m_boxStyle);
            m_editorUtils.Label("RegionsHeader", m_headerStyle);
            m_editorUtils.Panel("RegionsPanel", RegionsPanel, ref RegionsExpanded, ref RegionsHelpActive);
            if (!IsFromMainWindow)
            {
                m_editorUtils.Panel("Features", FeaturesPanel, ref FeaturesExpanded, ref FeaturesHelpActive);
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
            EditorGUILayout.BeginVertical(m_boxStyle);
            m_editorUtils.Label("AdvancedHeader", m_headerStyle);
            m_editorUtils.Panel("AdvancedSettings", AdvancedPanel, ref AdvancedExpanded, ref AdvancedHelpActive);
            m_editorUtils.Panel("MemoryPanel", MemoryPanel, ref MemoryExpanded, ref MemoryHelpActive);
            if (!IsFromMainWindow)
                m_editorUtils.Panel("DebugSettings", DebugPanel, ref DebugExpanded, ref DebugHelpActive);

            m_WorldSeedProp.isExpanded = !WorldExpanded;
            m_LayerRemapsProp.isExpanded = RemapsExpanded;
            m_RegionsProp.isExpanded = RegionsExpanded;
            m_UseSeaLevelProp.isExpanded = FeaturesExpanded;
            m_m_worldRandomPositionProp.isExpanded = ColorsExpanded;
            m_SunLightProp.isExpanded = TilesExpanded;
            m_worldOffsetProp.isExpanded = AdvancedExpanded;
            m_GlobalLayersetProp.isExpanded = MemoryExpanded;
            m_DebugSettingsProp.isExpanded = DebugExpanded;

            serializedObject.ApplyModifiedProperties();
            if (m_editorUtils.Button("ForceFullUpdate"))
            {
                m_stormWorld.ForceFullRefresh();
            }
            EditorGUILayout.EndVertical();
            if (!IsFromMainWindow && m_editorUtils.Button("OpenManager"))
            {
                StormManagerWindow.ShowManager();
            }

            if (IsPickingRemap)
            {
                string commandName = Event.current.commandName;
                if (commandName == "ObjectSelectorUpdated")
                {
                    UnityEngine.Object obj = EditorGUIUtility.GetObjectPickerObject();
                    if (obj is StormLayerRemap remap)
                    {
                        if (pickingRemapIndex < 0)
                        {
                            pickingRemapIndex = m_worldRemaps.Count;
                            m_worldRemaps.Add(null);
                        }
                        UpdateRemap(remap, pickingRemapIndex);
                    }
                }
                else if (commandName == "ObjectSelectorClosed")
                {
                    IsPickingRemap = false;
                    UnityEngine.Object obj = EditorGUIUtility.GetObjectPickerObject();
                    if (obj is StormLayerRemap remap)
                    {
                        if (pickingRemapIndex < 0)
                        {
                            pickingRemapIndex = m_worldRemaps.Count;
                            m_worldRemaps.Add(null);
                        }
                        UpdateRemap(remap, pickingRemapIndex);
                    }
                }
            }
        }
        #endregion
        #region GUI Panels

        private void LayerRemapPanel(bool helpEnabled)
        {
            if (m_worldRemaps.Count == 0)
            {
                m_editorUtils.Label("RemapEmptyMessage", EditorStyles.boldLabel);
            }
            else
            {
                for (int r = 0; r < m_worldRemaps.Count; ++r)
                {
                    GetRemapUI(r).DoLayout();
                }
            }
            EditorGUILayout.Space();
            GUIContent addButton = m_editorUtils.GetContent("RemapAddButton");
            addButton.image = AddButtonContent.image;
            if (GUILayout.Button(addButton, EditorGUIUtility.currentViewWidth < 300 ? EditorStyles.miniButtonRight : EditorStyles.miniButtonMid, GUILayout.Height(EditorGUIUtility.singleLineHeight), GUILayout.ExpandWidth(false)))
            {
                Undo.RecordObject(target, "Added Remap");
                IsPickingRemap = true;
                pickingRemapIndex = m_worldRemaps.Count;
                m_worldRemaps.Add(null);
                m_worldRemapUIs = new StormScriptableUI<StormLayerRemap>[m_worldRemaps.Count];
                EditorGUIUtility.ShowObjectPicker<StormLayerRemap>(null, true, "t:StormLayerRemap", EditorGUIUtility.GetControlID(FocusType.Passive));
            }
        }
        /// <summary>
        /// World render panel that contains all world settings
        /// </summary>
        /// <param name="helpEnabled"></param>
        private void WorldPanel(bool helpEnabled)
        {
            //World options
            int WorldSeed = m_stormWorld.WorldSeed;
            float seaLevel = StormAPI.SeaLevel;
            Transform SunLight = m_stormWorld.SunLight;
            bool hasGaiaSeaLevel = StormUtils.HasGaiaSeaLevel();
            bool useGaiaSeaLevel = m_stormWorld.UseGaiaSeaLevel;
            bool drawOtherCams = m_stormWorld.DrawOnOtherCams;
            int otherCamIdx = m_stormWorld.OtherCamsUseIndex;

            EditorGUILayout.BeginVertical(m_boxStyle);
            EditorGUILayout.LabelField("Setup", EditorStyles.boldLabel);
            ++EditorGUI.indentLevel;
            m_cullingCamerasProp.isExpanded = DrawReorderableList("CullingCamera", m_cullingCamerasProp.isExpanded, m_cullingCamerasList, m_editorUtils, helpEnabled);
            --EditorGUI.indentLevel;
            if (m_cullingCamerasChanged)
            {
                m_cullingCamerasChanged = false;
                Undo.RecordObject(m_stormWorld, "CullingCamera Changed");
                m_stormWorld.CullingCameras = m_cullingCameras.ToArray();
                m_stormWorld.RefreshAll();
            }
            EditorGUI.BeginChangeCheck();
            if (m_cullingCamerasProp.arraySize > 0)
            {
                drawOtherCams = m_editorUtils.Toggle("DrawOtherCams", drawOtherCams, helpEnabled);
                if (drawOtherCams)
                    otherCamIdx = m_editorUtils.IntSlider("OtherCamIdx", otherCamIdx, 0, m_cullingCamerasProp.arraySize - 1, helpEnabled);
            }
            SunLight = (Transform)m_editorUtils.ObjectField("SunLight", SunLight, typeof(Transform), true, helpEnabled);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(m_stormWorld, "World Settings Changed");
                m_stormWorld.SunLight = SunLight;
                m_stormWorld.DrawOnOtherCams = drawOtherCams;
                m_stormWorld.OtherCamsUseIndex = otherCamIdx;
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical(m_boxStyle);
            EditorGUILayout.LabelField("World Configuration", EditorStyles.boldLabel);
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.BeginHorizontal();
            WorldSeed = m_editorUtils.DelayedIntField("WorldSeed", WorldSeed, helpEnabled);
            if (GUILayout.Button(RandomizeButtonContent, GUILayout.ExpandWidth(false), GUILayout.Height(EditorGUIUtility.singleLineHeight)))
            {
                WorldSeed = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
                Undo.RecordObject(m_stormWorld, "World Seed Randomized");
                m_stormWorld.WorldSeed = WorldSeed;
            }
            EditorGUILayout.EndHorizontal();
            if (hasGaiaSeaLevel)
                useGaiaSeaLevel = m_editorUtils.Toggle("UseGaiaSeaLevel", useGaiaSeaLevel, helpEnabled);
            seaLevel = m_editorUtils.FloatField("SeaLevel", seaLevel, helpEnabled);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(m_stormWorld, "World Settings Changed");
                m_stormWorld.WorldSeed = WorldSeed;
                StormAPI.SeaLevel = seaLevel;
                m_stormWorld.UseGaiaSeaLevel = useGaiaSeaLevel;
                SceneView.RepaintAll();
            }
            EditorGUILayout.EndVertical();
        }
        private void RegionsPanel(bool helpEnabled)
        {
            EditorGUILayout.BeginVertical(m_boxStyle);
            m_editorUtils.Label("WorldRegions", EditorStyles.boldLabel, helpEnabled);
            if (m_worldRegions.Count == 0)
            {
                m_editorUtils.Label("RegionEmptyMessage", EditorStyles.boldLabel);
            }
            else
            {
                for (int r = 0; r < m_worldRegions.Count; ++r)
                {
                    GetRegionUI(r).DoLayout();
                }
            }
            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            m_newRegionName = EditorGUILayout.TextField(m_newRegionName, GUILayout.ExpandWidth(true));
            GUIContent addButton = m_editorUtils.GetContent("RegionAddButton");
            addButton.image = AddButtonContent.image;
            if (GUILayout.Button(addButton, EditorGUIUtility.currentViewWidth < 300 ? EditorStyles.miniButtonRight : EditorStyles.miniButtonMid, GUILayout.Height(EditorGUIUtility.singleLineHeight), GUILayout.ExpandWidth(false)))
            {
                Undo.RecordObject(target, "Added Region");
                if (string.IsNullOrEmpty(m_newRegionName))
                    m_newRegionName = $"Storm Region -{DateTime.Now:ffffff}";
                StormRegion newRegion = ScriptableObject.CreateInstance<StormRegion>();
                newRegion.name = m_newRegionName;
                newRegion.RequireRegionMasks = true;
                m_worldRegions.Add(newRegion);
                m_stormWorld.Regions = m_worldRegions;

                StormRegionMask mask = new GameObject(m_newRegionName).AddComponent<StormRegionMask>();
                mask.AddRegionLink(newRegion);
                Selection.activeGameObject = mask.gameObject;

                StormUtils.SetAssetDirty(m_stormWorld);
                m_newRegionName = "";
                GUIUtility.hotControl = 0;
                EditorGUIUtility.editingTextField = false;
            }
            if (EditorGUIUtility.currentViewWidth < 300)
            {
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
            }
            addButton = m_editorUtils.GetContent("RegionAddGlobalButton");
            addButton.image = AddButtonContent.image;
            if (GUILayout.Button(addButton, EditorGUIUtility.currentViewWidth < 300 ? EditorStyles.miniButton : EditorStyles.miniButtonRight, GUILayout.Height(EditorGUIUtility.singleLineHeight), GUILayout.ExpandWidth(false)))
            {
                Undo.RecordObject(target, "Added Region");
                StormRegion newRegion = ScriptableObject.CreateInstance<StormRegion>();
                newRegion.name = m_newRegionName;
                m_newRegionName = "";
                GUIUtility.hotControl = 0;
                EditorGUIUtility.editingTextField = false;
                m_worldRegions.Add(newRegion);
                m_stormWorld.Regions = m_worldRegions;
                StormUtils.SetAssetDirty(m_stormWorld);
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();

            List<StormRegion> allRegions = StormDataCache.AllKnownRegions;
            allRegions.RemoveAll(m_worldRegions.Contains);
            if (allRegions.Count > 0)
            {
                EditorGUILayout.BeginVertical(m_boxStyle);
                m_editorUtils.Label("TileRegions", EditorStyles.boldLabel, helpEnabled);
                for (int r = 0; r < allRegions.Count; ++r)
                {
                    GetTileRegionUI(allRegions[r]).DoLayout();
                }
                EditorGUILayout.EndVertical();
            }
        }
        /// <summary>
        /// Debug render panel that contains all the debug settings
        /// </summary>
        /// <param name="helpEnabled"></param>
        private void DebugPanel(bool helpEnabled)
        {
            //Debug options
            bool InputTiles = m_stormWorld.DebugSettings.ShowInputTiles;
            bool RenderCells = m_stormWorld.DebugSettings.ShowRenderCells;
            bool ShowCellLoadingArea = m_stormWorld.DebugSettings.ShowCellLoadingArea;
            bool ShowLODs = m_stormWorld.DebugSettings.ShowLODs;
            bool ShowLoading = m_stormWorld.DebugSettings.LogLoading;
            bool ShowUnloading = m_stormWorld.DebugSettings.LogUnloading;
            bool ShowGeneration = m_stormWorld.DebugSettings.LogGeneration;
            bool ShowJobDebug = m_stormWorld.DebugSettings.LogJobDebug;
            bool ShowLoadingJobs = m_stormWorld.DebugSettings.LogLoadingJobs;
            bool ShowRendering = m_stormWorld.DebugSettings.LogRendering;
            bool ShowRenderingNativeArray = m_stormWorld.DebugSettings.LogRenderingNativeArray;
            bool ShowNativeArray = m_stormWorld.DebugSettings.LogNativeArray;
            bool Verbose = m_stormWorld.DebugSettings.VerboseLogs;
            bool ShowInstances = m_stormWorld.DebugSettings.ShowInstances;
            bool ShowCellData = m_stormWorld.DebugSettings.ShowCellData;
            float ShowCellDataDist = m_stormWorld.DebugSettings.ShowCellDataDist;
            bool ShowCellLayerData = m_stormWorld.DebugSettings.ShowCellLayerData;
            bool ShowCellLoadedData = m_stormWorld.DebugSettings.ShowCellLoadedData;
            bool ShowCameraData = m_stormWorld.DebugSettings.ShowCameraData;
            bool DumpGenerationSplats = m_stormWorld.DebugSettings.DumpGenerationSplats;
            bool DumpLoadingSplats = m_stormWorld.DebugSettings.DumpLoadingSplats;
            bool StormLoaderCreateGameObjects = m_stormWorld.DebugSettings.StormLoaderCreateGameObjects;

            //Render debug options
            EditorGUI.BeginChangeCheck();
            InputTiles = m_editorUtils.Toggle("InputTiles", InputTiles, helpEnabled);
            RenderCells = m_editorUtils.Toggle("RenderCells", RenderCells, helpEnabled);
            if (RenderCells)
            {
                ShowCellData = m_editorUtils.Toggle("ShowCellData", ShowCellData, helpEnabled);
                if (ShowCellData)
                {
                    ShowCellDataDist = m_editorUtils.FloatField("ShowCellDataDist", ShowCellDataDist, helpEnabled);
                    ShowCellLayerData = m_editorUtils.Toggle("ShowCellLayerData", ShowCellLayerData, helpEnabled);
                    ShowCellLoadedData = m_editorUtils.Toggle("ShowCellLoadedData", ShowCellLoadedData, helpEnabled);
                }
            }
            ShowCellLoadingArea = m_editorUtils.Toggle("RenderCellLoadingArea", ShowCellLoadingArea, helpEnabled);
            ShowCameraData = m_editorUtils.Toggle("ShowCameraData", ShowCameraData, helpEnabled);
            ShowLODs = m_editorUtils.Toggle("ShowLODs", ShowLODs, helpEnabled);
            ShowLoading = m_editorUtils.Toggle("ShowLoading", ShowLoading, helpEnabled);
            ShowUnloading = m_editorUtils.Toggle("ShowUnloading", ShowUnloading, helpEnabled);
            ShowGeneration = m_editorUtils.Toggle("ShowGeneration", ShowGeneration, helpEnabled);
            ShowLoadingJobs = m_editorUtils.Toggle("ShowLoadingJobs", ShowLoadingJobs, helpEnabled);
            ShowJobDebug = m_editorUtils.Toggle("ShowJobDebug", ShowJobDebug, helpEnabled);
            ShowRendering = m_editorUtils.Toggle("ShowRendering", ShowRendering, helpEnabled);
            ShowRenderingNativeArray = m_editorUtils.Toggle("ShowRenderingNativeArray", ShowRenderingNativeArray, helpEnabled);
            ShowNativeArray = m_editorUtils.Toggle("ShowNativeArray", ShowNativeArray, helpEnabled);
            Verbose = m_editorUtils.Toggle("Verbose", Verbose, helpEnabled);
            ShowInstances = m_editorUtils.Toggle("ShowInstances", ShowInstances, helpEnabled);
            DumpLoadingSplats = m_editorUtils.Toggle("DumpLoadingSplats", DumpLoadingSplats, helpEnabled);
            DumpGenerationSplats = m_editorUtils.Toggle("DumpGenerationSplats", DumpGenerationSplats, helpEnabled);
            StormLoaderCreateGameObjects = m_editorUtils.Toggle("StormLoaderCreateGameObjects", StormLoaderCreateGameObjects, helpEnabled);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(m_stormWorld, "Storm Debug Changed");
                m_stormWorld.DebugSettings.ShowInputTiles = InputTiles;
                m_stormWorld.DebugSettings.ShowRenderCells = RenderCells;
                m_stormWorld.DebugSettings.ShowCellLoadingArea = ShowCellLoadingArea;
                m_stormWorld.DebugSettings.ShowLODs = ShowLODs;
                m_stormWorld.DebugSettings.LogLoading = ShowLoading;
                m_stormWorld.DebugSettings.LogUnloading = ShowUnloading;
                m_stormWorld.DebugSettings.LogGeneration = ShowGeneration;
                m_stormWorld.DebugSettings.LogLoadingJobs = ShowLoadingJobs;
                m_stormWorld.DebugSettings.LogJobDebug = ShowJobDebug;
                m_stormWorld.DebugSettings.LogRendering = ShowRendering;
                m_stormWorld.DebugSettings.LogRenderingNativeArray = ShowRenderingNativeArray;
                m_stormWorld.DebugSettings.LogNativeArray = ShowNativeArray;
                m_stormWorld.DebugSettings.VerboseLogs = Verbose;
                m_stormWorld.DebugSettings.ShowInstances = ShowInstances;
                m_stormWorld.DebugSettings.ShowCellData = ShowCellData;
                m_stormWorld.DebugSettings.ShowCellDataDist = ShowCellDataDist;
                m_stormWorld.DebugSettings.ShowCellLayerData = ShowCellLayerData;
                m_stormWorld.DebugSettings.ShowCellLoadedData = ShowCellLoadedData;
                m_stormWorld.DebugSettings.ShowCameraData = ShowCameraData;
                m_stormWorld.DebugSettings.StormLoaderCreateGameObjects = StormLoaderCreateGameObjects;
                m_stormWorld.DebugSettings.DumpGenerationSplats = DumpGenerationSplats;
                m_stormWorld.DebugSettings.DumpLoadingSplats = DumpLoadingSplats;
                StormDataCache.UpdateWorldSettings(m_stormWorld);
            }

            //Buttons
            if (m_editorUtils.Button("DebugPolys"))
            {
                DebugInputTiles(true, false);
            }
            if (m_editorUtils.Button("DebugTRS"))
            {
                DebugInputTiles(false, true);
            }
        }
        private void FeaturesPanel(bool helpEnabled)
        {
            List<StormFeatureset> featureSets = StormDataCache.AllKnownFeatureSets;
            ulong StormTotalInstances = 0;
            ulong StormTotalVerts = 0;
            ulong StormTotalDrawCalls = 0;
            ulong[] StormTypeInstances = new ulong[7];
            ulong[] StormTypeVerts = new ulong[7];
            ulong[] StormTypeDrawCalls = new ulong[7];
            bool CollectAll = m_stormDebugExpanded.target;
            List<StormFeatureSettings> collectedFeatures = new List<StormFeatureSettings>(StormDataCache.AllKnownFeatures.Count);
            int totalCount = 0;
            for (int fs = 0; fs < featureSets.Count; ++fs)
            {
                if (featureSets[fs] != null && featureSets[fs].Features != null)
                    totalCount += 1 + featureSets[fs].Features.Length;
                else
                    ++totalCount;
            }
            if (totalCount != m_featureExpanded.Length)
                Array.Resize(ref m_featureExpanded, totalCount);
            EditorGUILayout.BeginHorizontal();
            if (m_editorUtils.Button("ExpandAllButton", GUILayout.ExpandWidth(true)))
            {
                for (int i = 0; i < totalCount; ++i)
                {
                    if (m_featureExpanded[i] == null)
                        m_featureExpanded[i] = new AnimBool(true, Repaint);
                    else
                        m_featureExpanded[i].target = true;
                }
                m_stormDebugExpanded.target = true;
            }
            if (m_editorUtils.Button("CollapseAllButton", GUILayout.ExpandWidth(true)))
            {
                for (int i = 0; i < totalCount; ++i)
                {
                    if (m_featureExpanded[i] == null)
                        m_featureExpanded[i] = new AnimBool(false, Repaint);
                    else
                        m_featureExpanded[i].target = false;
                }
                m_stormDebugExpanded.target = false;
            }
            EditorGUILayout.EndHorizontal();
            totalCount = 0;
            int curSolo = m_stormWorld.Debug_SoloFeatureIdx;
            bool needsRefresh = false;
            for(int fs = 0; fs < featureSets.Count; ++fs)
            {
                if (m_featureExpanded[totalCount] == null)
                    m_featureExpanded[totalCount] = new AnimBool(true, Repaint);
                AnimBool expanded = m_featureExpanded[totalCount++];
                if (fs != 0)
                    EditorGUILayout.Space();
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                StormFeatureset featureSet = featureSets[fs];
                EditorGUILayout.BeginHorizontal();
                Rect r = GUILayoutUtility.GetRect(15, EditorGUIUtility.singleLineHeight, GUILayout.ExpandWidth(false));
                r.xMin += 15;
                expanded.target = EditorGUI.Foldout(r, expanded.target, "", true);

                Color startColor = GUI.backgroundColor;
                GUIContent enableButton;
                if (!featureSet.IsEnabled)
                {
                    enableButton = m_editorUtils.GetContent("RegionEnableButton");
                    enableButton.image = EnableButtonContent.image;
                    GUI.backgroundColor = Color.grey;
                }
                else
                {
                    enableButton = m_editorUtils.GetContent("RegionDisableButton");
                    enableButton.image = DisableButtonContent.image;
                }
                if (GUILayout.Button(enableButton, GUILayout.Height(EditorGUIUtility.singleLineHeight), GUILayout.Width(80)))
                {
                    featureSet.IsEnabled = !featureSet.IsEnabled;
                    m_stormWorld.UpdateFeaturesets(false);
                    StormUtils.ForceEditorSceneviewRedraw(m_stormWorld.gameObject);
                    m_stormWorld.Update();
                }
                GUI.backgroundColor = startColor;

                EditorGUILayout.ObjectField(featureSet, typeof(StormFeatureset), false, GUILayout.ExpandWidth(true));
                EditorGUILayout.EndHorizontal();
                if (featureSet != null)
                {
                    if (EditorGUILayout.BeginFadeGroup(expanded.faded) && featureSet.Features != null)
                    {
                        for (int f = 0; f < featureSet.Features.Length; ++f)
                        {
                            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                            if (m_featureExpanded[totalCount] == null)
                                m_featureExpanded[totalCount] = new AnimBool(false, Repaint);
                            expanded = m_featureExpanded[totalCount++];
                            StormFeatureSettings feature = featureSet.Features[f].Feature;
                            if (feature == null)
                                continue;
                            int featureIdx = StormDataCache.GetFeatureIndex(feature);
                            bool disable = m_stormWorld.SystemSettings.IsFeatureDisabled(featureSet, feature);
                            bool isSolo = !disable && curSolo == featureIdx && featureIdx >= 0;
                            EditorGUILayout.BeginHorizontal();
                            r = GUILayoutUtility.GetRect(25, EditorGUIUtility.singleLineHeight, GUILayout.ExpandWidth(false));
                            r.xMin += 20;
                            expanded.target = EditorGUI.Foldout(r, expanded.target, "", true);
                            startColor = GUI.backgroundColor;
                            if (disable)
                            {
                                enableButton = m_editorUtils.GetContent("RegionEnableButton");
                                enableButton.image = EnableButtonContent.image;
                                GUI.backgroundColor = Color.grey;
                            }
                            else
                            {
                                enableButton = m_editorUtils.GetContent("RegionDisableButton");
                                enableButton.image = DisableButtonContent.image;
                            }
                            if (GUILayout.Button(enableButton, GUILayout.Height(EditorGUIUtility.singleLineHeight), GUILayout.Width(80)))
                            {
                                if (m_stormWorld.SystemSettings.SetFeatureDisabled(featureSet, feature, !disable))
                                    m_stormWorld.UpdateFeaturesets(false);
                                StormUtils.ForceEditorSceneviewRedraw(m_stormWorld.gameObject);
                                m_stormWorld.Update();
                            }
                            GUI.backgroundColor = startColor;
                            EditorGUI.BeginChangeCheck();
                            EditorGUI.BeginDisabledGroup(disable);
                            isSolo = GUILayout.Toggle(isSolo, m_editorUtils.GetContent("FeatureSoloButton"), EditorStyles.miniButton, GUILayout.ExpandWidth(false));
                            EditorGUI.EndDisabledGroup();
                            if (EditorGUI.EndChangeCheck())
                            {
                                m_stormWorld.Debug_SoloFeatureIdx = (isSolo ? featureIdx : -1);
                                StormUtils.ForceEditorSceneviewRedraw(m_stormWorld.gameObject);
                                m_stormWorld.Update();
                            }
                            EditorGUI.BeginChangeCheck();
                            EditorGUILayout.ObjectField(feature, typeof(StormFeatureSettings), false, GUILayout.ExpandWidth(true));
                            EditorGUILayout.EndHorizontal();
                            if (EditorGUILayout.BeginFadeGroup(expanded.faded))
                            {
                                needsRefresh = true;
                                ++EditorGUI.indentLevel;
                                var debugData = m_stormWorld.GetInstanceCount(feature);
                                m_editorUtils.LabelField("FeatureInstanceCount", new GUIContent(debugData.VisibleCellCount.ToString("N0")));
                                ulong totalInstances = 0;
                                ulong totalVerts = 0;
                                ulong totalDrawCalls = (ulong)debugData.DrawCalls;
                                for (int lod = 0; lod < debugData.LODCounts?.Length; ++lod)
                                {
                                    totalInstances += (ulong)debugData.LODCounts[lod];
                                    totalVerts += (ulong)debugData.LODVerts[lod];
                                    EditorGUILayout.LabelField(string.Format(m_editorUtils.GetContent("FeatureLODCount").text, lod), debugData.LODCounts[lod].ToString("N0"));
                                    EditorGUILayout.LabelField(string.Format(m_editorUtils.GetContent("FeatureLODVerts").text, lod), debugData.LODVerts[lod].ToString("N0"));
                                }
                                m_editorUtils.LabelField("FeatureDrawCalls", new GUIContent(debugData.DrawCalls.ToString("N0")));
                                m_editorUtils.LabelField("FeatureTotalInstances", new GUIContent(totalInstances.ToString("N0")));
                                m_editorUtils.LabelField("FeatureTotalVerts", new GUIContent(totalVerts.ToString("N0")));

                                if (!collectedFeatures.Contains(feature))
                                {
                                    collectedFeatures.Add(feature);
                                    StormTotalInstances += totalInstances;
                                    StormTotalVerts += totalVerts;
                                    StormTotalDrawCalls += totalDrawCalls;
                                    int typeIdx = feature.ObjectType switch {
                                        StormFeatureType.GRASS => 0,
                                        StormFeatureType.DETAIL => 1,
                                        StormFeatureType.PLANT => 2,
                                        StormFeatureType.TREE => 3,
                                        StormFeatureType.SMALL_OBJECT => 4,
                                        StormFeatureType.MEDIUM_OBJECT => 5,
                                        StormFeatureType.LARGE_OBJECT => 6,
                                        _ => 0,
                                    };
                                    StormTypeInstances[typeIdx] += totalInstances;
                                    StormTypeVerts[typeIdx] += totalVerts;
                                    StormTypeDrawCalls[typeIdx] += totalDrawCalls;
                                }
                                --EditorGUI.indentLevel;

                            }
                            else if (CollectAll && !collectedFeatures.Contains(feature))
                            {
                                collectedFeatures.Add(feature);
                                var debugData = m_stormWorld.GetInstanceCount(feature);
                                int typeIdx = feature.ObjectType switch {
                                    StormFeatureType.GRASS => 0,
                                    StormFeatureType.DETAIL => 1,
                                    StormFeatureType.PLANT => 2,
                                    StormFeatureType.TREE => 3,
                                    StormFeatureType.SMALL_OBJECT => 4,
                                    StormFeatureType.MEDIUM_OBJECT => 5,
                                    StormFeatureType.LARGE_OBJECT => 6,
                                    _ => 0,
                                };
                                for (int lod = 0; lod < debugData.LODCounts?.Length; ++lod)
                                {
                                    ulong totalInstances = (ulong)debugData.LODCounts[lod];
                                    ulong totalVerts = (ulong)debugData.LODVerts[lod];
                                    StormTotalInstances += totalInstances;
                                    StormTotalVerts += totalVerts;
                                    StormTypeInstances[typeIdx] += totalInstances;
                                    StormTypeVerts[typeIdx] += totalVerts;
                                }
                                ulong totalDrawCalls = (ulong)debugData.DrawCalls;
                                StormTotalDrawCalls += totalDrawCalls;
                                StormTypeDrawCalls[typeIdx] += totalDrawCalls;
                            }
                            EditorGUILayout.EndFadeGroup();
                            EditorGUILayout.EndVertical();
                        }
                    }
                    else
                    {
                        if (CollectAll && featureSet.Features != null)
                        {
                            for (int f = 0; f < featureSet.Features.Length; ++f)
                            {
                                StormFeatureSettings feature = featureSet.Features[f].Feature;
                                if (feature != null && !collectedFeatures.Contains(feature))
                                {
                                    collectedFeatures.Add(feature);
                                    var debugData = m_stormWorld.GetInstanceCount(feature);
                                    int typeIdx = feature.ObjectType switch {
                                        StormFeatureType.GRASS => 0,
                                        StormFeatureType.DETAIL => 1,
                                        StormFeatureType.PLANT => 2,
                                        StormFeatureType.TREE => 3,
                                        StormFeatureType.SMALL_OBJECT => 4,
                                        StormFeatureType.MEDIUM_OBJECT => 5,
                                        StormFeatureType.LARGE_OBJECT => 6,
                                        _ => 0,
                                    };
                                    for (int lod = 0; lod < debugData.LODCounts.Length; ++lod)
                                    {
                                        ulong totalInstances = (ulong)debugData.LODCounts[lod];
                                        ulong totalVerts = (ulong)debugData.LODVerts[lod];
                                        StormTotalInstances += totalInstances;
                                        StormTypeInstances[typeIdx] += totalInstances;
                                        StormTotalVerts += totalVerts;
                                        StormTypeVerts[typeIdx] += totalVerts;
                                    }
                                    StormTotalDrawCalls += (ulong)debugData.DrawCalls;
                                    StormTypeDrawCalls[typeIdx] += (ulong)debugData.DrawCalls;
                                }
                            }
                        }
                        totalCount += featureSet.Features?.Length ?? 0;
                    }
                    EditorGUILayout.EndFadeGroup();
                }
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.Space();
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            ++EditorGUI.indentLevel;
            m_stormDebugExpanded.target = m_editorUtils.Foldout(m_stormDebugExpanded.target, "StormDebugTotals", true, helpEnabled);
            if (EditorGUILayout.BeginFadeGroup(m_stormDebugExpanded.faded))
            {
                needsRefresh = true;
                m_editorUtils.LabelField("StormTotalInstances", new GUIContent(StormTotalInstances.ToString("N0")));
                m_editorUtils.LabelField("StormTotalDrawCalls", new GUIContent(StormTotalDrawCalls.ToString("N0")));
                m_editorUtils.LabelField("StormTotalVerts", new GUIContent(StormTotalVerts.ToString("N0")));
                for (int typeIdx = 0; typeIdx < StormTypeInstances.Length; ++typeIdx)
                {
                    string typename = typeIdx switch {
                        0 => "Grass",
                        1 => "Detail",
                        2 => "Plant",
                        3 => "Tree",
                        4 => "Small",
                        5 => "Medium",
                        6 => "Large",
                        _ => ""
                    };
                    EditorGUILayout.Space();
                    m_editorUtils.LabelField("Storm" + typename + "Instances", new GUIContent(StormTypeInstances[typeIdx].ToString("N0")));
                    m_editorUtils.LabelField("Storm" + typename + "DrawCalls", new GUIContent(StormTypeDrawCalls[typeIdx].ToString("N0")));
                    m_editorUtils.LabelField("Storm" + typename + "Verts", new GUIContent(StormTypeVerts[typeIdx].ToString("N0")));
                }
            }
            --EditorGUI.indentLevel;
            EditorGUILayout.EndFadeGroup();
            EditorGUILayout.EndVertical();
            if (needsRefresh)
                Repaint();
        }
        private void TilesPanel(bool helpEnabled)
        {
            ++EditorGUI.indentLevel;
            var allDataTiles = StormDataCache.AllDataTiles;
            int cnt = allDataTiles.Count;
            GUILayout.Label(string.Format(m_editorUtils.GetContent("AllDataTilesHeader").text, cnt), EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            for (int t = 0; t < cnt; ++t)
            {
                var tData = allDataTiles.ElementAtOrDefault(t);
                GameObject go = tData?.Data?.m_myObject;
                if (go != null)
                {
                    EditorGUILayout.ObjectField(go, typeof(GameObject), true);
                }
            }
            EditorGUILayout.EndVertical();
            var allRegionTiles = StormDataCache.AllRegionTiles;
            cnt = allRegionTiles.Count;
            GUILayout.Label(string.Format(m_editorUtils.GetContent("AllFeaturesetTilesHeader").text, cnt), EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            for (int t = 0; t < cnt; ++t)
            {
                var tData = allRegionTiles.ElementAtOrDefault(t);
                GameObject go = tData?.m_myObject;
                if (go != null)
                {
                    EditorGUILayout.ObjectField(go, typeof(GameObject), true);
                }
            }
            EditorGUILayout.EndVertical();
            --EditorGUI.indentLevel;
        }
        private void ColorsPanel(bool helpEnabled)
        {
            ++EditorGUI.indentLevel;
            var allColorTargets = StormDataCache.AllKnownColorTargets;
            if (m_colorTargetUIs.Length != allColorTargets.Count)
                Array.Resize(ref m_colorTargetUIs, allColorTargets.Count);
            for (int c =0; c < m_colorTargetUIs.Length; ++c)
            {
                StormColorTargetData targets = allColorTargets[c];
                StormScriptableUI<StormColorTargetData> ui = m_colorTargetUIs[c];
                if (ui == null || ui.Target != targets)
                {
                    ui = new StormScriptableUI<StormColorTargetData>(targets, null, Repaint, m_editorUtils, (c) => { }, true, () => DrawColorUIButtons(targets));
                    m_colorTargetUIs[c] = ui;
                }
                ui.DoLayout();
            }
            --EditorGUI.indentLevel;
        }
        private void DrawColorUIButtons(StormColorTargetData targets)
        {
            bool isActive = m_displayedTarget == targets;
            bool shouldBeActive = m_editorUtils.Toggle(isActive, "ShowColorAreaButton", EditorStyles.miniButton, GUILayout.ExpandWidth(false));
            if (shouldBeActive != isActive)
                m_displayedTarget = shouldBeActive ? targets : null;
        }
        private void DataVisualizerPanel(bool helpEnabled)
        {
            GUIContent visModeContent = m_editorUtils.GetContent("DataVisualizerMode");
            GUIContent[] visModePopupContent = new GUIContent[] {
                m_editorUtils.GetContent("VisMode_Disabled"),
                m_editorUtils.GetContent("VisMode_Single"),
                m_editorUtils.GetContent("VisMode_Featureset"),
                m_editorUtils.GetContent("VisMode_Custom"),
            };

            StormWorld.DataVisualizerMode visModeIndex = m_stormWorld.VisualizerMode;
            EditorGUI.BeginChangeCheck();
            visModeIndex = (StormWorld.DataVisualizerMode)EditorGUILayout.Popup(visModeContent, (int)visModeIndex, visModePopupContent);
            if (EditorGUI.EndChangeCheck())
            {
                m_stormWorld.VisualizerMode =  visModeIndex;
            }
            if (visModeIndex == StormWorld.DataVisualizerMode.SINGLE)
            {

            }
            else if (visModeIndex == StormWorld.DataVisualizerMode.FEATURESET)
            {

            }
            else if (visModeIndex == StormWorld.DataVisualizerMode.CUSTOM)
            {

            }
        }
        private void MemoryPanel(bool helpEnabled)
        {
            MemoryUsageData memData = m_stormWorld.GetMemoryUsage();
            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
            GUIContent labelContent = m_editorUtils.GetContent("MemoryPanelWarning");
            labelContent.image = WarningIconContent.image;
            m_editorUtils.Label(labelContent, EditorStyles.wordWrappedMiniLabel);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();
            m_editorUtils.Label("Mem_Cache", EditorStyles.boldLabel, helpEnabled);
            m_editorUtils.LabelField("Mem_Count", new GUIContent(memData.Cache.Count.ToString("N0")));
            m_editorUtils.LabelField("Mem_SizeCPU", new GUIContent(StormUtils.GetReadableFileSize(memData.Cache.SizeCPU)));
            m_editorUtils.LabelField("Mem_SizeGPU", new GUIContent(StormUtils.GetReadableFileSize(memData.Cache.SizeGPU)));

            EditorGUILayout.Space();
            m_editorUtils.Label("Mem_Data", EditorStyles.boldLabel, helpEnabled);
            m_editorUtils.LabelField("Mem_Count", new GUIContent(memData.Data.Count.ToString("N0")));
            m_editorUtils.LabelField("Mem_SizeCPU", new GUIContent(StormUtils.GetReadableFileSize(memData.Data.SizeCPU)));
            m_editorUtils.LabelField("Mem_SizeGPU", new GUIContent(StormUtils.GetReadableFileSize(memData.Data.SizeGPU)));

            EditorGUILayout.Space();
            m_editorUtils.Label("Mem_Loading", EditorStyles.boldLabel, helpEnabled);
            m_editorUtils.LabelField("Mem_Count", new GUIContent(memData.Loading.Count.ToString("N0")));
            m_editorUtils.LabelField("Mem_SizeCPU", new GUIContent(StormUtils.GetReadableFileSize(memData.Loading.SizeCPU)));
            m_editorUtils.LabelField("Mem_SizeGPU", new GUIContent(StormUtils.GetReadableFileSize(memData.Loading.SizeGPU)));

            EditorGUILayout.Space();
            m_editorUtils.Label("Mem_Rendering", EditorStyles.boldLabel, helpEnabled);
            m_editorUtils.LabelField("Mem_Count", new GUIContent(memData.Rendering.Count.ToString("N0")));
            m_editorUtils.LabelField("Mem_SizeCPU", new GUIContent(StormUtils.GetReadableFileSize(memData.Rendering.SizeCPU)));
            m_editorUtils.LabelField("Mem_SizeGPU", new GUIContent(StormUtils.GetReadableFileSize(memData.Rendering.SizeGPU)));

            EditorGUILayout.Space();
            m_editorUtils.Label("Mem_Total", EditorStyles.boldLabel, helpEnabled);
            MemoryCategoryData total = memData.Cache + memData.Data + memData.Loading + memData.Rendering;
            m_editorUtils.LabelField("Mem_Count", new GUIContent(total.Count.ToString("N0")));
            m_editorUtils.LabelField("Mem_SizeCPU", new GUIContent(StormUtils.GetReadableFileSize(total.SizeCPU)));
            m_editorUtils.LabelField("Mem_SizeGPU", new GUIContent(StormUtils.GetReadableFileSize(total.SizeGPU)));
        }
        private void AdvancedPanel(bool helpEnabled)
        {
            double3 WorldOffset = m_stormWorld.WorldOffset;
            Vector2 WorldRandomPosition = m_stormWorld.WorldRandomPosition;
            Vector2 WorldRandomScale = m_stormWorld.WorldRandomScale;

            EditorGUILayout.BeginVertical(m_boxStyle);
            EditorGUILayout.LabelField("Global Layer Set Configuration", EditorStyles.boldLabel);
            m_stormWorld.GlobalLayerSet = (StormLayerSet)m_editorUtils.ObjectField("GlobalLayerSet", m_stormWorld.GlobalLayerSet, typeof(StormLayerSet), false, helpEnabled);
            if (!IsFromMainWindow)
                EditorGUI.indentLevel++;
            m_heightNamesProp.isExpanded = DrawReorderableList("HeightNames", m_heightNamesProp.isExpanded, m_heightNamesList, m_editorUtils, helpEnabled);
            if (m_heightNamesChanged)
            {
                m_heightNamesChanged = false;
                Undo.RecordObject(m_stormWorld, "Height Names Changed");
                m_stormWorld.HeightNames = m_heightNames.ToArray().AsSpan();
                m_stormWorld.RefreshAll();
            }
            m_holeNamesProp.isExpanded = DrawReorderableList("HoleNames", m_holeNamesProp.isExpanded, m_holeNamesList, m_editorUtils, helpEnabled);
            if (m_holeNamesChanged)
            {
                m_holeNamesChanged = false;
                Undo.RecordObject(m_stormWorld, "Hole Names Changed");
                m_stormWorld.HoleNames = m_holeNames.ToArray().AsSpan();
                m_stormWorld.RefreshAll();
            }
            if (!IsFromMainWindow)
                EditorGUI.indentLevel--;
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical(m_boxStyle);
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.LabelField("World Offset Settings", EditorStyles.boldLabel);
            WorldOffset = (float3)m_editorUtils.Vector3Field("WorldOffset", (float3)WorldOffset, helpEnabled);
            WorldRandomPosition = m_editorUtils.Vector2Field("WorldRandomPosition", WorldRandomPosition, helpEnabled);
            WorldRandomScale = m_editorUtils.Vector2Field("WorldRandomScale", WorldRandomScale, helpEnabled);
            EditorGUILayout.EndVertical();
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(m_stormWorld, "World Settings Changed");
                m_stormWorld.WorldOffset = WorldOffset;
                m_stormWorld.WorldRandomPosition = WorldRandomPosition;
                m_stormWorld.WorldRandomScale = WorldRandomScale;
            }
        }

        #endregion
        #region GUI Reorderable List Utils

        /// <summary>
        /// Draws a foldout reorderable list
        /// </summary>
        /// <param name="headerKey"></param>
        /// <param name="foldout"></param>
        /// <param name="reorderableList"></param>
        /// <returns></returns>
        public static bool DrawReorderableList(string headerKey, bool foldout, ReorderableList reorderableList, EditorUtils editorUtils, bool helpEnabled)
        {
            if (foldout)
            {
                Rect r = EditorGUILayout.GetControlRect(GUILayout.Height(reorderableList.GetHeight()));
                reorderableList.DoList(r);
                r.height = EditorGUIUtility.singleLineHeight;
                foldout = EditorGUI.Foldout(r, foldout, "", true);
            }
            else
            {
                foldout = EditorGUILayout.Foldout(foldout, editorUtils.GetContent(headerKey), true);
            }
            EditorGUILayout.Space(3f);
            editorUtils.InlineHelp(headerKey, helpEnabled);
            return foldout;
        }
        /*
        private void DrawWorldRegionListHeader(Rect rect)
        {
            if (IsFromMainWindow)
            {
                rect.x += 10;
                rect.width -= 10;
            }
            EditorGUI.LabelField(rect, m_editorUtils.GetContent("GlobalFeatureSets"));
        }
        private float GetWorldRegionListHeight(int index)
        {
            return GetRegionUI(index).GetGUIHeight();
        }
        private void DrawWorldRegionElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            GetRegionUI(index).DoGUI(rect);
        }
        private void AddWorldRegionElement(ReorderableList list)
        {
            Undo.RecordObject(m_stormWorld, "Reorder List Changed");
            list.list.Add(null);
            m_worldRegionsChanged = true;
        }
        private void OnRemoveGlobalFeaturesetElement(ReorderableList list)
        {
            Undo.RecordObject(m_stormWorld, "Reorder List Changed");
            list.list.RemoveAt(list.index);
            m_worldRegionsChanged = true;
        }
        //*/
        private void DrawHeightNamesHeader(Rect rect)
        {
            if (IsFromMainWindow)
            {
                rect.x += 10;
                rect.width -= 10;
            }
            EditorGUI.LabelField(rect, m_editorUtils.GetContent("HeightNames"));
        }
        private float GetHeightNamesElementHeight(int index)
        {
            SerializedProperty layerProp = m_heightNamesProp.GetArrayElementAtIndex(index);
            SerializedProperty nameProp = layerProp.FindPropertyRelative("LayerName");
            return EditorGUI.GetPropertyHeight(nameProp) + EditorGUIUtility.standardVerticalSpacing + EditorGUIUtility.singleLineHeight;
        }
        private void DrawHeightNamesElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            Rect elementRect = new Rect(rect);
            StormSourceLayerInfo info = m_heightNames[index];
            SerializedProperty layerProp = m_heightNamesProp.GetArrayElementAtIndex(index);
            SerializedProperty nameProp = layerProp.FindPropertyRelative("LayerName");
            SerializedProperty strengthProp = layerProp.FindPropertyRelative("Strength");
            elementRect.height = EditorGUI.GetPropertyHeight(nameProp);
            EditorGUI.PropertyField(elementRect, nameProp, m_editorUtils.GetContent("HeightNameLayerName"));
            elementRect.y += elementRect.height;
            elementRect.height = EditorGUIUtility.singleLineHeight;
            EditorGUI.PropertyField(elementRect, strengthProp, m_editorUtils.GetContent("HeightNameLayerStrength"));
           
            if (info.LayerName != nameProp.stringValue || info.Strength != strengthProp.floatValue)
            {
                info.LayerName = nameProp.stringValue;
                info.Strength = strengthProp.floatValue;
                m_heightNames[index] = info;
                m_heightNamesChanged = true;
            }
        }
        private void AddHeightNamesElement(ReorderableList list)
        {       
            Undo.RecordObject(m_stormWorld, "Reorder List Changed");
            list.list.Add(new StormSourceLayerInfo());
            m_heightNamesChanged = true;
        }

        private void OnRemoveHeightNamesElement(ReorderableList list)
        {
            Undo.RecordObject(m_stormWorld, "Reorder List Changed");
            list.list.RemoveAt(list.index);
            m_heightNamesChanged = true;
        }

        private void DrawHoleNamesHeader(Rect rect)
        {
            if (IsFromMainWindow)
            {
                rect.x += 10;
                rect.width -= 10;
            }
            EditorGUI.LabelField(rect, m_editorUtils.GetContent("HoleNames"));
        }
        private float GetHoleNamesElementHeight(int index)
        {
            SerializedProperty layerProp = m_holeNamesProp.GetArrayElementAtIndex(index);
            SerializedProperty nameProp = layerProp.FindPropertyRelative("LayerName");
            return EditorGUI.GetPropertyHeight(nameProp) + EditorGUIUtility.standardVerticalSpacing + EditorGUIUtility.singleLineHeight;
        }
        private void DrawHoleNamesElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            Rect elementRect = new Rect(rect);
            StormSourceLayerInfo info = m_holeNames[index];
            SerializedProperty layerProp = m_holeNamesProp.GetArrayElementAtIndex(index);
            SerializedProperty nameProp = layerProp.FindPropertyRelative("LayerName");
            SerializedProperty strengthProp = layerProp.FindPropertyRelative("Strength");
            elementRect.height = EditorGUI.GetPropertyHeight(nameProp);
            EditorGUI.PropertyField(elementRect, nameProp, m_editorUtils.GetContent("HoleNameLayerName"));
            elementRect.y += elementRect.height;
            elementRect.height = EditorGUIUtility.singleLineHeight;
            EditorGUI.PropertyField(elementRect, strengthProp, m_editorUtils.GetContent("HoleNameLayerStrength"));
           
            if (info.LayerName != nameProp.stringValue || info.Strength != strengthProp.floatValue)
            {
                info.LayerName = nameProp.stringValue;
                info.Strength = strengthProp.floatValue;
                m_holeNames[index] = info;
                m_holeNamesChanged = true;
            }
        }
        private void AddHoleNamesElement(ReorderableList list)
        {
            list.list.Add(new StormSourceLayerInfo());
            m_holeNamesChanged = true;
        }

        private void OnRemoveHoleNamesElement(ReorderableList list)
        {
            list.list.RemoveAt(list.index);
            m_holeNamesChanged = true;
        }

        private void DrawCamerasHeader(Rect rect)
        {
            if (IsFromMainWindow)
            {
                rect.x += 10;
                rect.width -= 10;
            }
            EditorGUI.LabelField(rect, m_editorUtils.GetContent("CullingCamera"));
        }
        private void DrawCamerasElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            EditorGUI.BeginChangeCheck();
            Camera newCam = EditorGUI.ObjectField(rect, m_cullingCameras[index], typeof(Camera), true) as Camera;
            if (EditorGUI.EndChangeCheck())
            {
                m_cullingCameras[index] = newCam;
                m_cullingCamerasChanged = true;
            }
        }
        private void AddCamerasElement(ReorderableList list)
        {
            list.list.Add(null);
            m_cullingCamerasChanged = true;
        }

        private void OnRemoveCamerasElement(ReorderableList list)
        {
            list.list.RemoveAt(list.index);
            m_cullingCamerasChanged = true;
        }

        /// <summary>
        /// Builds the reorderable lists
        /// </summary>
        private void BuildReorderableLists()
        {
            //Build Reorderable Lists
            if (m_stormWorld != null)
            {
                //Global Features Sets
                m_worldRegions = m_stormWorld.Regions;
                /*
                m_worldRegionList = new ReorderableList(m_worldRegions, typeof(StormRegion));
                m_worldRegionList.headerHeight = EditorGUIUtility.singleLineHeight;
                m_worldRegionList.drawHeaderCallback += DrawWorldRegionListHeader;
                m_worldRegionList.elementHeightCallback += GetWorldRegionListHeight;
                m_worldRegionList.drawElementCallback += DrawWorldRegionElement;
                m_worldRegionList.onAddCallback += AddWorldRegionElement;
                m_worldRegionList.onRemoveCallback += OnRemoveGlobalFeaturesetElement;
                //*/

                //Hole Names
                m_cullingCameras = new List<Camera>(m_stormWorld.CullingCameras);
                m_cullingCamerasList = new ReorderableList(m_cullingCameras, typeof(Camera));
                m_cullingCamerasList.headerHeight = EditorGUIUtility.singleLineHeight;
                m_cullingCamerasList.drawHeaderCallback += DrawCamerasHeader;
                m_cullingCamerasList.drawElementCallback += DrawCamerasElement;
                m_cullingCamerasList.onAddCallback += AddCamerasElement;
                m_cullingCamerasList.onRemoveCallback += OnRemoveCamerasElement;

                //Height Names
                m_heightNames = new List<StormSourceLayerInfo>(m_stormWorld.HeightNames.ToArray());
                m_heightNamesList = new ReorderableList(m_heightNames, typeof(StormSourceLayerInfo));
                m_heightNamesList.headerHeight = EditorGUIUtility.singleLineHeight;
                m_heightNamesList.drawHeaderCallback += DrawHeightNamesHeader;
                m_heightNamesList.elementHeightCallback = GetHeightNamesElementHeight;
                m_heightNamesList.drawElementCallback += DrawHeightNamesElement;
                m_heightNamesList.onAddCallback += AddHeightNamesElement;
                m_heightNamesList.onRemoveCallback += OnRemoveHeightNamesElement;

                //Hole Names
                m_holeNames = new List<StormSourceLayerInfo>(m_stormWorld.HoleNames.ToArray());
                m_holeNamesList = new ReorderableList(m_holeNames, typeof(StormSourceLayerInfo));
                m_holeNamesList.headerHeight = EditorGUIUtility.singleLineHeight;
                m_holeNamesList.drawHeaderCallback += DrawHoleNamesHeader;
                m_holeNamesList.elementHeightCallback = GetHoleNamesElementHeight;
                m_holeNamesList.drawElementCallback += DrawHoleNamesElement;
                m_holeNamesList.onAddCallback += AddHoleNamesElement;
                m_holeNamesList.onRemoveCallback += OnRemoveHoleNamesElement;

                m_worldRemaps = new List<StormLayerRemap>(m_stormWorld.LayerRemaps);
            }
        }

        #endregion
        #region Utils
        StormScriptableUI<StormLayerRemap> GetRemapUI(int index)
        {
            if (index < 0 || index >= m_worldRemaps.Count)
                return null;
            if (m_worldRemapUIs.Length != m_worldRemaps.Count)
                Array.Resize(ref m_worldRemapUIs, m_worldRemaps.Count);
            if (m_worldRemapUIs[index] == null)
                m_worldRemapUIs[index] = new StormScriptableUI<StormLayerRemap>(m_worldRemaps[index], null, IsFromMainWindow ? ParentWindowRepaint : Repaint, m_editorUtils, (r) => UpdateRemap(r, index), true, () => RemapButtons(index));
            return m_worldRemapUIs[index];
        }
        void UpdateRemap(StormLayerRemap remap, int index)
        {
            Debug.Log("Updating Remap " + index + " to " + (remap == null ? "NULL" : remap.name));
            if (m_worldRemaps[index] != remap)
            {
                Undo.RecordObject(target, "Changed Remap");
                m_worldRemaps[index] = remap;
                m_stormWorld.LayerRemaps = m_worldRemaps.ToArray();
                GetRemapUI(index).Target = remap;
                StormUtils.SetAssetDirty(m_stormWorld);
            }
        }
        void RemapButtons(int index)
        {
            GUIContent remove = m_editorUtils.GetContent("RemapRemoveButton");
            remove.image = RemoveButtonContent.image;
            if (GUILayout.Button(remove, GUILayout.Height(EditorGUIUtility.singleLineHeight), GUILayout.ExpandWidth(false)))
            {
                Undo.RecordObject(target, "Removed Remap");
                m_worldRemaps.RemoveAt(index);
                m_stormWorld.LayerRemaps = m_worldRemaps.ToArray();
                m_worldRemapUIs = new StormScriptableUI<StormLayerRemap>[0];
                StormUtils.SetAssetDirty(m_stormWorld);
            }
        }
        StormScriptableUI<StormRegion> GetRegionUI(int index)
        {
            if (index < 0 || index >= m_worldRegions.Count)
                return null;
            if (m_worldRegionUIs.Length != m_worldRegions.Count)
                Array.Resize(ref m_worldRegionUIs, m_worldRegions.Count);
            if (m_worldRegionUIs[index] == null)
                m_worldRegionUIs[index] = new StormScriptableUI<StormRegion>(m_worldRegions[index], null, IsFromMainWindow ? ParentWindowRepaint : Repaint, m_editorUtils, (r) => UpdateRegion(r, index), true, () => RegionButtons(index));
            return m_worldRegionUIs[index];
        }
        void UpdateRegion(StormRegion region, int index)
        {
            Undo.RecordObject(target, "Changed Region");
            string grPath = StormUtils.GetAssetPath(region);
            if (!string.IsNullOrEmpty(grPath))
            {
                bool changed = false;
                for (int fs = 0; fs < region.Featuresets?.Length; ++fs)
                {
                    StormFeatureset featureset = region.Featuresets[fs];
                    string gfPath = StormUtils.GetAssetPath(featureset);
                    if (string.IsNullOrEmpty(gfPath))
                    {
                        gfPath = grPath.Substring(0, grPath.Length - Path.GetFileName(grPath).Length);
                        gfPath += StormUtils.EnsureUniqueName(gfPath, string.IsNullOrEmpty(featureset.name) ? region.name + "_Featureset" : featureset.name, "asset");
                        featureset = StormUtils.SaveAsset(featureset, gfPath);
                        region.Featuresets[fs] = featureset;
                        changed = true;
                        Debug.LogWarning("Embedded Featuresets do not persist inside of a Region asset.\nYour featureset has been saved at the path: " + gfPath, featureset);
                    }
                }
                if (changed)
                    StormUtils.SetAssetDirty(region);
            }

            m_worldRegions[index] = region;
            m_stormWorld.Regions = m_worldRegions;
            StormUtils.SetAssetDirty(m_stormWorld);
        }
        void RegionButtons(int index)
        {
            Color startColor = GUI.backgroundColor;
            GUIContent enableButton;
            if (m_worldRegions[index] != null && m_worldRegions[index].IsEnabled)
            {
                enableButton = m_editorUtils.GetContent("RegionDisableButton");
                enableButton.image = DisableButtonContent.image;
            }
            else
            {
                enableButton = m_editorUtils.GetContent("RegionEnableButton");
                enableButton.image = EnableButtonContent.image;
                GUI.backgroundColor = Color.grey;
            }
            if (GUILayout.Button(enableButton, GUILayout.Height(EditorGUIUtility.singleLineHeight), GUILayout.Width(80)))
            {
                Undo.RecordObject(target, "Show/Hide Region");
                m_worldRegions[index].IsEnabled = !m_worldRegions[index].IsEnabled;
                StormUtils.SetAssetDirty(m_worldRegions[index]);
                m_stormWorld.UpdateFeaturesets();
            }
            GUI.backgroundColor = startColor;
            GUIContent remove = m_editorUtils.GetContent("RegionRemoveButton");
            remove.image = RemoveButtonContent.image;
            if (GUILayout.Button(remove, GUILayout.Height(EditorGUIUtility.singleLineHeight), GUILayout.ExpandWidth(false)))
            {
                Undo.RecordObject(target, "Removed Region");
                m_worldRegions.RemoveAt(index);
                m_stormWorld.Regions = m_worldRegions;
                m_worldRegionUIs = new StormScriptableUI<StormRegion>[0];
                StormUtils.SetAssetDirty(m_stormWorld);
            }
        }
        StormScriptableUI<StormRegion> GetTileRegionUI(StormRegion region)
        {
            int numUIs = m_tileRegionUIs.Length;
            for (int r = 0; r < numUIs; ++r)
            {
                if (m_tileRegionUIs[r].Target == region)
                {
                    return m_tileRegionUIs[r];
                }
            }
            Array.Resize(ref m_tileRegionUIs, numUIs + 1);
            StormScriptableUI<StormRegion> ui = new StormScriptableUI<StormRegion>(region, null, IsFromMainWindow ? ParentWindowRepaint : Repaint, m_editorUtils, null, true, () => TileRegionButtons(region));
            m_tileRegionUIs[numUIs] = ui;
            return ui;
        }
        void TileRegionButtons(StormRegion region)
        {
            Color startColor = GUI.backgroundColor;
            GUIContent enableButton;
            if (region.IsEnabled)
            {
                enableButton = m_editorUtils.GetContent("RegionDisableButton");
                enableButton.image = DisableButtonContent.image;
            }
            else
            {
                enableButton = m_editorUtils.GetContent("RegionEnableButton");
                enableButton.image = EnableButtonContent.image;
                GUI.backgroundColor = Color.grey;
            }
            if (GUILayout.Button(enableButton, GUILayout.Height(EditorGUIUtility.singleLineHeight), GUILayout.Width(80)))
            {
                Undo.RecordObject(target, "Show/Hide Region");
                region.IsEnabled = !region.IsEnabled;
                StormUtils.SetAssetDirty(region);
                m_stormWorld.UpdateFeaturesets();
            }
            GUI.backgroundColor = startColor;
        }
        /// <summary>
        /// Called when undo is performed
        /// </summary>
        private void OnUndo()
        {
            BuildReorderableLists();
        }

        /// <summary>
        /// Function that allows to debug polys and trs lists
        /// </summary>
        /// <param name="poly"></param>
        /// <param name="trs"></param>
        private void DebugInputTiles(bool poly, bool trs)
        {
            StormWorld world = target as StormWorld;
            int t = world.transform.childCount;
            var items = StormDataCache.AllDataTiles;
            foreach (var data in items)
            {
                bool found = false;
                if (poly && data.Data.PolygonData.Length > 0)
                    found = true;
                if (trs && data.Data.TRSData.Length > 0)
                    found = true;
                if (!found)
                    continue;
                GameObject go = new GameObject("Tile - " + (t++));
                go.SetActive(false);
                go.transform.SetParent(world.transform);
                go.transform.SetPositionAndRotation(data.Data.Position, Quaternion.identity);
                go.transform.localScale = data.Data.Size;

                StormTile tile = go.AddComponent<StormTile>();
                tile.UpdatePosition = false;
                tile.UpdateScale = false;
                tile.TileData = data.Data;
            }
        }
        /// <summary>
        /// Converts list of List<StormSourceLayerInfo> to a Span<StormSourceLayerInfo>
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        private Span<StormSourceLayerInfo> CreateHeightNamesList(List<StormSourceLayerInfo> list)
        {
            Span<StormSourceLayerInfo> layerInfo = new Span<StormSourceLayerInfo>(list.ToArray());
            return layerInfo;
        }

        #endregion
        #region Callbacks
        void OnSystemSettingsChanged(StormSystemSettings settings)
        {
            Undo.RecordObject(m_stormWorld, "System Settings Changed");
            m_stormWorld.SystemSettings = settings;
            m_systemSettingsUI.Target = m_stormWorld.SystemSettings;
        }
        void OnQualitySettingsChanged(StormQualitySettings settings)
        {
            Undo.RecordObject(m_stormWorld, "Quality Settings Changed");
            m_stormWorld.QualitySettings = settings;
            m_qualitySettingsUI.Target = m_stormWorld.QualitySettings;
        }
        #endregion

        static GUIStyle _loadingLabelStyle;
        static GUIStyle LoadingLabelStyle
        {
            get {
                if (_loadingLabelStyle == null)
                {
                    _loadingLabelStyle = new GUIStyle(EditorStyles.boldLabel);
                    _loadingLabelStyle.alignment = TextAnchor.MiddleCenter;
                }
                return _loadingLabelStyle;
            }
        }
        static GUIStyle _guiBackgroundStyle;
        static GUIStyle GUIBackgroundStyle
        {
            get {
                if (_guiBackgroundStyle == null)
                {
                    _guiBackgroundStyle = new GUIStyle();
                    _guiBackgroundStyle.padding = new RectOffset(0, 0, 0, 0);
                    _guiBackgroundStyle.margin = new RectOffset(0, 0, 0, 0);
                    _guiBackgroundStyle.alignment = TextAnchor.MiddleCenter;
                    _guiBackgroundStyle.border = new RectOffset(2, 2, 2, 2);
                    _guiBackgroundStyle.imagePosition = ImagePosition.ImageOnly;
                    if (EditorGUIUtility.isProSkin)
                    {
                        _guiBackgroundStyle.normal.background = Resources.Load<Texture2D>("OriginUIbackground_Pro");
                        _guiBackgroundStyle.normal.textColor = Color.white;
                    }
                    else
                    {
                        _guiBackgroundStyle.normal.background = Resources.Load<Texture2D>("OriginUIbackground");
                        _guiBackgroundStyle.normal.textColor = Color.black;
                    }
                }
                return _guiBackgroundStyle;
            }
        }

        static bool IsLoading = false;
        static float LoadingProgress = 0f;
        static void DuringSceneGUI(SceneView view)
        {
            if (Application.isPlaying)
                return;
            StormDataCache.Update();
            if (Event.current.type == EventType.Layout)
            {
                IsLoading = StormDataCache.IsLoading;
                LoadingProgress = StormDataCache.LoadingProgress;
                if (IsLoading)
                {
                    StormUtils.ForceEditorSceneviewRedraw();
                    if (StormWorld.Instance != null)
                        StormWorld.Instance.Update();
                }
            }
            if (IsLoading)
            {
                float scaledScreenWidth = (view.camera.pixelRect.size.x / EditorGUIUtility.pixelsPerPoint);
                float scaledScreenHeight = (view.camera.pixelRect.size.y / EditorGUIUtility.pixelsPerPoint);


                float sizeX = scaledScreenWidth * 0.3f;
                float sizeY = EditorGUIUtility.singleLineHeight;
                float x = (scaledScreenWidth * 0.5f) - (sizeX / 2f);
                float y = scaledScreenHeight - sizeY - 5f;
                GUILayout.BeginArea(new Rect(x, y, sizeX, sizeY));
                {
                    GUILayout.BeginHorizontal(GUIBackgroundStyle);
                    {
                        Rect r = GUILayoutUtility.GetRect(sizeX, sizeY);
                        Rect LoadBarRect = r;
                        LoadBarRect.width *= LoadingProgress;
                        EditorGUI.DrawRect(LoadBarRect, Color.blue);
                        GUI.Label(r, "Loading Storm Data: " + LoadingProgress.ToString("P"), LoadingLabelStyle);
                    }
                    GUILayout.EndHorizontal();
                }
                GUILayout.EndArea();
            }
        }
    }
}