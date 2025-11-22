using UnityEngine;
using UnityEditor;
using ProceduralWorlds.Storm.Core;
using PWCommon5;

namespace ProceduralWorlds.Storm.StormEditor
{
    /// <summary>
    /// Custom editor for the PolygonGizmoDrawer class.
    /// </summary>
    [CustomEditor(typeof(StormPolygonMask),true)]
    public class StormPolygonMaskEditor : PWEditor
    {
        private StormPolygonMask _mask;
        public StormPolygonDataEditor polygonEditor;

        // PW Editor
        private static bool m_showQuickStart = true;
        private static bool m_showOverviewPanel = true;
        private static bool m_showAdvancedPanel = false;
        protected EditorUtils m_editorUtils;

        SerializedProperty keepMissingProp;
        SerializedProperty createTilesProp;

        protected void Initialize()
        {
            if (m_editorUtils == null)
                return;
            m_editorUtils.Initialize();
        }

        private void OnEnable()
        {
            if (target == null)
                return;
            // If there isn't any Editor Utils Initialized
            if (m_editorUtils == null)
                m_editorUtils = StormApp.GetEditorUtils(this);

            _mask = (StormPolygonMask)target;

            polygonEditor = (StormPolygonDataEditor)CreateEditor(this, typeof(StormPolygonDataEditor));
            polygonEditor.transformObject = _mask.transform;
            SetPolygonDataObject(_mask.polygonDataObject);
            
            Undo.undoRedoPerformed -= UndoRedoPerformed;
            Undo.undoRedoPerformed += UndoRedoPerformed;

            keepMissingProp = serializedObject.FindProperty("KeepIfMissing");
            createTilesProp = serializedObject.FindProperty("CreateTiles");
        } 

        private void OnDestroy()
        {
            Undo.undoRedoPerformed -= UndoRedoPerformed;
            DestroyImmediate(polygonEditor);
        }

        private void OnDisable()
        {
            Undo.undoRedoPerformed -= UndoRedoPerformed;
            DestroyImmediate(polygonEditor);
        }

        private void UndoRedoPerformed()
        {
            RefreshPolygonDataEditor();
        }
        
        private void OnSceneGUI()
        {
            polygonEditor.SceneGUI();
        }

        public void QuickStartPanel(bool helpEnabled)
        {
            m_editorUtils.Label("AddPoints", EditorStyles.wordWrappedLabel, helpEnabled);
            m_editorUtils.Label("InsertPoints", EditorStyles.wordWrappedLabel, helpEnabled);
            m_editorUtils.Label("RemovePoints", EditorStyles.wordWrappedLabel, helpEnabled);
        }

        private void OverviewPanel(bool helpEnabled)
        {
            DrawPolygonDataObject(helpEnabled);
        }

        private void AdvancedPanel(bool helpEnabled)
        {
            DrawDebuggingButton(helpEnabled);
        }

        public override void OnInspectorGUI()
        {
            if (target is StormPolygonMask mask and not UI.PolygonMask)
            {
                Debug.LogWarning("Found direct reference to DLL version of StormPolygonMask. Changing reference to use UI.PolygonMask subclass.", mask.gameObject);
                //target is wrong type. using Internal Core version. change it to use un-obfuscated script version
                GameObject go = mask.gameObject;
                StormPolygonMask newMask = go.AddComponent<UI.PolygonMask>();
                newMask.CopyMaskData(mask, true);
                EditorUtility.SetDirty(go);
                DestroyImmediate(mask);
                GUIUtility.ExitGUI();
                return;
            }

            Initialize();

            serializedObject.Update();

            m_showQuickStart = m_editorUtils.Panel("QuickStartPanel", QuickStartPanel, m_showQuickStart);
            m_showOverviewPanel = m_editorUtils.Panel("OverviewPanel", OverviewPanel, m_showOverviewPanel);
            m_showAdvancedPanel = m_editorUtils.Panel("AdvancedPanel", AdvancedPanel, m_showAdvancedPanel);

            serializedObject.ApplyModifiedProperties();
        }

        private void SetPolygonDataObject(StormPolygonDataObject newPolygonDataObject)
        {
            Undo.RecordObject(_mask, "Polygon Data Changed");
            _mask.polygonDataObject = newPolygonDataObject;
            EditorUtility.SetDirty(_mask);
            RefreshPolygonDataEditor();
        }

        private void RefreshPolygonDataEditor()
        {
            if (_mask.polygonDataObject == null)
            {
                polygonEditor.ownerObject = _mask;
                polygonEditor.polygonData = _mask.polygonData;
            }
            else
            {
                polygonEditor.ownerObject = _mask.polygonDataObject;
                polygonEditor.polygonData = _mask.polygonDataObject.m_polygonData;
            }
        }
        
        /// <summary>
        /// Draws the Add Polygon Data button.
        /// </summary>
        private void DrawPolygonDataObject(bool helpEnabled)
        {
            EditorGUILayout.BeginHorizontal();
            {
                StormPolygonDataObject oldPolygonDataObject = _mask.polygonDataObject;
                StormPolygonDataObject newPolygonDataObject = (StormPolygonDataObject)m_editorUtils.ObjectField("PolygonData",
                    _mask.polygonDataObject, typeof(StormPolygonDataObject), false);

                if (newPolygonDataObject != oldPolygonDataObject)
                {
                    SetPolygonDataObject(newPolygonDataObject);
                    SceneView.RepaintAll();
                }

                CreateNewPolygonData();
            }
            EditorGUILayout.EndHorizontal();

            m_editorUtils.InlineHelp("PolygonData", helpEnabled);

            // Only draw the Edit Polygon Panel if there is polygon data
            // DrawEditPolygonPanel(helpEnabled);
            // DrawLayersList(helpEnabled);
            polygonEditor.InspectorGUI(helpEnabled);
        }

        public void DrawPolygonDataObjectNoLayers(bool helpEnabled, bool simpleMode = false)
        {
            if(!simpleMode)
            {
                EditorGUILayout.BeginHorizontal();
                {
                    StormPolygonDataObject oldPolygonDataObject = _mask.polygonDataObject;
                    StormPolygonDataObject newPolygonDataObject = (StormPolygonDataObject)m_editorUtils.ObjectField("PolygonData",
                        _mask.polygonDataObject, typeof(StormPolygonDataObject), false);

                    if (newPolygonDataObject != oldPolygonDataObject)
                    {
                        SetPolygonDataObject(newPolygonDataObject);
                        SceneView.RepaintAll();
                    }

                    CreateNewPolygonData();
                }
                EditorGUILayout.EndHorizontal();

                m_editorUtils.InlineHelp("PolygonData", helpEnabled);
            }
            // Only draw the Edit Polygon Panel if there is polygon data
            // DrawEditPolygonPanel(helpEnabled);
            // DrawLayersList(helpEnabled);
            polygonEditor.DrawEditPolygonPanel(helpEnabled, simpleMode);
        }

        private void DrawDebuggingButton(bool helpEnabled)
        {
            StormPolygonMask mask = (StormPolygonMask)target;
            mask.AllowLivePolyUpdates = m_editorUtils.Toggle("AllowLivePolyUpdates", mask.AllowLivePolyUpdates, helpEnabled);
            polygonEditor.debuggingEnabled =
                m_editorUtils.Toggle("Debugging", polygonEditor.debuggingEnabled, helpEnabled);
            createTilesProp.boolValue = m_editorUtils.Toggle("CreateTiles", createTilesProp.boolValue, helpEnabled);
            keepMissingProp.boolValue = m_editorUtils.Toggle("KeepMissing", keepMissingProp.boolValue, helpEnabled);

        }

        /// <summary>
        /// Creates a new instance of PolygonData and assigns it to the drawer's polygonData property, and saves it as a ScriptableObject asset in the project.
        /// </summary>
        private void CreateNewPolygonData()
        {
            if (GUILayout.Button("New", GUILayout.Width(45f)))
            {
                StormPolygonDataObject newPolygonDataObject = CreateInstance<StormPolygonDataObject>();

                string path = AssetDatabase.GenerateUniqueAssetPath("Assets/NewPolygonData.asset");
                AssetDatabase.CreateAsset(newPolygonDataObject, path);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                
                SetPolygonDataObject(newPolygonDataObject);
                
                polygonEditor.ProcessMeshes();
            }
        }
    }
}