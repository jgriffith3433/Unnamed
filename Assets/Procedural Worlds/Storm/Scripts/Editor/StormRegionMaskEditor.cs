using System;
using System.Collections.Generic;
using ProceduralWorlds.Storm.Utils;
using PWCommon5;
using UnityEditor;
using UnityEngine;

namespace ProceduralWorlds.Storm.StormEditor
{    
    [CustomEditor(typeof(StormRegionMask))]
    public class StormRegionMaskEditor : PWEditor
    {
        #region Menu
        [MenuItem("GameObject/Procedural Worlds/Storm/Storm Region Mask", priority = 10)]
        public static void CreateRegionMask() => CreateRegionMask(null);
        public static void CreateRegionMask(string name)
        {
            if (string.IsNullOrEmpty(name))
                name = $"Storm Region -{DateTime.Now:ffffff}";

            GameObject go = new GameObject(name);
            go.AddComponent<StormRegionMask>();
            Selection.activeGameObject = go;

            EditorGUIUtility.PingObject(go);
        }
        #endregion

        private EditorUtils m_editorUtils;
        private StormRegionMask m_regionMask;

        //Storm layer
        SerializedProperty p_layerProp;

        //Initialization state 
        private bool m_isInitialized = false;

        //The texture used as a button for editing nodes
        private Texture2D m_nodeButtonTexture;

        //The name of the texture used as a button for editing nodes
        private const string m_nodeButtonTextureName = "UI/Skin/Knob.psd";

        //Node button size constants
        private Vector2 m_nodeBrushSize = new Vector2(25f, 25f);
        private Vector2 m_halfNodeBrushSize = new Vector2(12.5f, 12.5f);

        //Current brush location
        private Vector3 m_brushPosition = Vector3.zero;

        //Current brush normal
        private Vector3 m_brushNormal = Vector3.up;

        //Brush states
        private bool m_isIntentToPaint = false;
        private bool m_isIntentToErase = false;
        private bool m_isPainting = false;
        private bool m_isErasing = false;


        private GUIContent AddButtonContent;
        private GUIContent RemoveButtonContent;

        //Regions
        SerializedProperty p_regionsProp;
        List<StormScriptableUI<StormRegion>> m_regionUIs = new List<StormScriptableUI<StormRegion>>();

        #region Initialization - setup and teardown

        /// <summary>
        /// Initialize the system for use with the region mask visualizer
        /// </summary>
        private void InitializeRegionMask()
        {
            //Create and initialize editor utils
            if (m_editorUtils == null)
            {
                m_editorUtils = StormApp.GetEditorUtils(this);
            }

            //Check for region mask
            if (m_regionMask == null)
            {
                m_regionMask = target as StormRegionMask;
                if (m_regionMask == null)
                {
                    return;
                }
            }

            //Make sure we have our button texture loaded
            if (m_nodeButtonTexture == null)
            {
                m_nodeButtonTexture = AssetDatabase.GetBuiltinExtraResource<Texture2D>(m_nodeButtonTextureName);
            }

            //Register for editor events and 
            if (!m_isInitialized)
            {
                SceneView.duringSceneGui -= OnSceneGUIUpdate;
                SceneView.duringSceneGui += OnSceneGUIUpdate;
                Undo.undoRedoPerformed -= OnUndo;
                Undo.undoRedoPerformed += OnUndo;
                m_isInitialized = true;
            }
            
            //Update the visualization
            m_regionMask.UpdateVisualization();
        }

        /// <summary>
        /// Uninitialize the region mask
        /// </summary>
        private void UnInitializeRegionMask()
        {
            SceneView.duringSceneGui -= OnSceneGUIUpdate;
            Undo.undoRedoPerformed -= OnUndo;
        }

        /// <summary>
        /// Handle undo
        /// </summary>
        void OnUndo()
        {
            m_regionMask.UpdateVisualization();
            m_regionMask.UpdateStormData();
        }

        #endregion

        #region Main event processing handlers

        /// <summary>
        /// On enable handler
        /// </summary>
        private void OnEnable()
        {
            InitializeRegionMask();
            m_regionMask.UpdateVisualization();
            p_layerProp = serializedObject.FindProperty("m_overrideID");
            p_regionsProp = serializedObject.FindProperty("m_regions");

            UpdateRegionUIs();
            AddButtonContent = EditorGUIUtility.IconContent("d_Toolbar Plus@2x");
            RemoveButtonContent = EditorGUIUtility.IconContent("d_Toolbar Minus@2x");
        }

        /// <summary>
        /// Update the region UIs
        /// </summary>
        void UpdateRegionUIs()
        {
            m_regionUIs.Clear();
            (target as StormRegionMask).UpdateRegions();
            for (int r = 0; r < p_regionsProp.arraySize; ++r)
            {
                int idx = r; //needed to capture actual value, not value after loop
                m_regionUIs.Add(new StormScriptableUI<StormRegion>(p_regionsProp.GetArrayElementAtIndex(r).objectReferenceValue as StormRegion, "", Repaint, m_editorUtils, (reg) => OnUpdateRegion(idx, reg), false, () => RegionCustomButtons(idx)));
            }
        }

        /// <summary>
        /// On disable handler
        /// </summary>
        private void OnDisable()
        {
            /*
            if (!m_regionMask.AlwaysShowVisualization)
            {
                m_regionMask.HideVisualizationMeshes();
            }
            */
            UnInitializeRegionMask();
        }

        /// <summary>
        /// On destroy handler
        /// </summary>
        private void OnDestroy()
        {
            UnInitializeRegionMask();
        }

        /// <summary>
        /// On inspector GUI - Main editing UX
        /// </summary>
        public override void OnInspectorGUI()
        {
            m_regionMask = target as StormRegionMask; //added because editor appears to lose reference to region mask sometimes
            //Create and initialize 
            InitializeRegionMask();
            if (m_regionMask != null && m_regionMask.transform.hasChanged)
                m_regionMask.ProcessTransformUpdate();

            //Do not remove this - required for the editor utils to work
            m_editorUtils.Initialize();

            //Display title etc
            m_editorUtils.Title("PolyMaskMainPanelTitle");
            GUILayout.BeginHorizontal(EditorStyles.helpBox);
            m_editorUtils.Text("PolyMaskMainPanelHelp");
            GUILayout.EndHorizontal();

            //Dislay the main panel 
            m_editorUtils.Panel("MainPanel", MainPanel, true);

            //Region panel
            m_editorUtils.Panel("RegionPanel", RegionPanel, true);
            serializedObject.ApplyModifiedProperties();
        }

        /// <summary>
        /// UX for the main panel for the mask
        /// </summary>
        /// <param name="helpEnabled"></param>
        private void MainPanel(bool helpEnabled)
        {
            //Otherwise handle the mask editing as normal
            EditorGUI.BeginChangeCheck();
            string originalLayerName = p_layerProp.stringValue;
            m_regionMask.OverrideRegionID = m_editorUtils.Toggle("OverrideRegionID", m_regionMask.OverrideRegionID, helpEnabled);
            if (m_regionMask.OverrideRegionID)
                m_editorUtils.PropertyField("RegionMaskName", p_layerProp, helpEnabled);
            float regionMaskFalloff = m_editorUtils.Slider("RegionMaskFalloff", m_regionMask.RegionFalloff, 0f, 20f, helpEnabled);
            float regionMaskStrength = m_editorUtils.Slider("RegionMaskStrength", m_regionMask.RegionStrength, 0f, 1f, helpEnabled);
            List<string> layerNames = new List<string>();
            for (int i = 0; i < 32; ++i)
            {
                string layerName = LayerMask.LayerToName(i);
                if (!string.IsNullOrEmpty(layerName))
                {
                    layerNames.Add(layerName);
                }
            }
            LayerMask groundLayers = m_editorUtils.MaskField("GroundLayers", m_regionMask.GroundLayers, layerNames.ToArray(), helpEnabled);
            Color color = m_editorUtils.ColorField("MaskColor", m_regionMask.MaskColor, helpEnabled);
            bool shadeVisualization = m_editorUtils.Toggle("ShadeVisualization", m_regionMask.ShadeVisualization, helpEnabled);

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(m_regionMask, "Changed Region Mask");

                //Causes layer name to update
                serializedObject.ApplyModifiedProperties(); 

                //Update the region mask name
                if (p_layerProp.stringValue != originalLayerName)
                {
                    m_regionMask.UpdateStormData();
                }

                //Update the region mask falloff
                if (regionMaskFalloff != m_regionMask.RegionFalloff)
                {
                    m_regionMask.RegionFalloff = regionMaskFalloff;
                }

                //Update the region mask strength
                if (regionMaskStrength != m_regionMask.RegionStrength)
                {
                    m_regionMask.RegionStrength = regionMaskStrength;
                }

                //Update the mask color
                if (color != m_regionMask.MaskColor)
                {
                    m_regionMask.MaskColor = color;
                }

                //Update the ground layers
                if (groundLayers != m_regionMask.GroundLayers)
                {
                    m_regionMask.GroundLayers = groundLayers;
                }

                //Update the always show visualization
                if (shadeVisualization != m_regionMask.ShadeVisualization)
                {
                    m_regionMask.ShadeVisualization = shadeVisualization;
                }

                //Mark the mask as dirty
                EditorUtility.SetDirty(m_regionMask);
            }

            if (m_editorUtils.Button("RecenterMaskGOButton", helpEnabled))
            {
                Undo.RecordObject(m_regionMask, "Moved Region Mask");
                m_regionMask.CenterTransform();
                EditorUtility.SetDirty(m_regionMask);
            }
        }

        /// <summary>
        /// UX for the Affected Region List
        /// </summary>
        /// <param name="helpEnabled"></param>
        private void RegionPanel(bool helpEnabled)
        {
            for (int r = 0; r < m_regionUIs.Count; ++r)
                m_regionUIs[r].DoLayout();
            GUIContent addButton = m_editorUtils.GetContent("AddRegionButton");
            addButton.image = AddButtonContent.image;
            if (GUILayout.Button(addButton, GUILayout.Height(EditorGUIUtility.singleLineHeight), GUILayout.ExpandWidth(true)))
            {
                p_regionsProp.InsertArrayElementAtIndex(p_regionsProp.arraySize);
                serializedObject.ApplyModifiedProperties();
                UpdateRegionUIs();
            }
        }

        void OnUpdateRegion(int index, StormRegion region)
        {
            p_regionsProp.GetArrayElementAtIndex(index).objectReferenceValue = region;
            serializedObject.ApplyModifiedProperties();
            (target as StormRegionMask).UpdateRegions();
        }

        void RegionCustomButtons(int index)
        {
            GUIContent remove = m_editorUtils.GetContent("RemoveRegionButton");
            remove.image = RemoveButtonContent.image;
            if (GUILayout.Button(remove, GUILayout.Height(EditorGUIUtility.singleLineHeight), GUILayout.ExpandWidth(false)))
            {
                p_regionsProp.DeleteArrayElementAtIndex(index);
                m_regionUIs.RemoveAt(index);
                serializedObject.ApplyModifiedProperties();
                UpdateRegionUIs();
            }
        }

        /// <summary>
        /// Event handling - needs to be handled here as we are doing brush events, and mouse events
        /// are not sent to OnSceneGUI. Registration is done in the InitializeRegionMask method.
        /// </summary>
        /// <param name="sceneView"></param>
        private void OnSceneGUIUpdate(SceneView sceneView)
        {
            //Check for valid event
            Event e = Event.current;
            if (e == null)
            {
                return;
            }

            //Check for valid scene view
            if (sceneView == null)
            {
                return;
            }

            //Check if region mask is null
            if (m_regionMask == null)
            {
                return;
            }

            //Exit on unity center or right mouse button based nav based events
            if (e.button == 1 || e.button == 2)
            {
                return;
            }

            //If shift is pressed, then we are in view and edit nodes mode
            if (e.shift)
            {
                DrawNodesAndProcessMovement();
                return;
            }

            //Exit if not a control or alt key related event
            if (!(e.control || e.alt))
            {
                return;
            }

            //Exit if we are not in the scene view window
            Vector2 mousePosition = e.mousePosition;
            if (mousePosition.x < 0 || mousePosition.y < 0 ||
                mousePosition.x > sceneView.position.width ||
                mousePosition.y > sceneView.position.height)
            {
                m_isIntentToPaint = false;
                m_isPainting = false;
                m_isIntentToErase = false;
                m_isErasing = false;
                return;
            }
            // Mouse wheel scroll adjusts brush size or scroll in scene view
            if (e.type == EventType.ScrollWheel)
            {
                m_regionMask.BrushRadius = Mathf.Clamp(m_regionMask.BrushRadius - (e.delta.y * 0.02f * m_regionMask.BrushRadius), 1, 100);
                e.Use();
                Repaint();
                return;
            }
            //Show where painting or deleting is happening
            if (e.control)
            {
                DrawActiveAndNextNode();
            }
            else
            {
                DrawAllNodes();
            }

            //Draw the brush if its a layout or repaint event
            if (e.type == EventType.Repaint)
            {
                DrawBrush(e);
                return;
            }

            if (!m_isErasing && !m_isPainting)
            {
                if (e.type != EventType.MouseMove && e.type != EventType.MouseDown && 
                    e.type != EventType.MouseDrag && e.type != EventType.MouseUp)
                {
                    if (e.control)
                    {
                        GetWorldPositionAtMouse(sceneView, e.mousePosition, out m_brushPosition, out m_brushNormal);
                        m_isIntentToPaint = true;
                        m_isPainting = false;
                        m_isIntentToErase = false;
                        m_isErasing = false;
                        sceneView.Repaint();
                        return;
                    }
                    else if (e.alt)
                    {
                        GetWorldPositionAtMouse(sceneView, e.mousePosition, out m_brushPosition, out m_brushNormal);
                        m_isIntentToPaint = false;
                        m_isPainting = false;
                        m_isIntentToErase = true;
                        m_isErasing = false;
                        sceneView.Repaint();
                        return;
                    }
                    else
                    {
                        m_isIntentToPaint = false;
                        m_isPainting = false;
                        m_isIntentToErase = false;
                        m_isErasing = false;
                        return;
                    }
                }
            }

            if (e.type == EventType.MouseMove)
            {
                //Process the event
                GetWorldPositionAtMouse(sceneView, e.mousePosition, out m_brushPosition, out m_brushNormal);
                if (e.control)
                {
                    m_isIntentToPaint = true;
                    m_isPainting = false;
                    m_isIntentToErase = false;
                    m_isErasing = false;
                }
                else if (e.alt)
                {
                    m_isIntentToPaint = false;
                    m_isPainting = false;
                    m_isIntentToErase = true;
                    m_isErasing = false;
                }
                sceneView.Repaint();
            }

            if (e.type == EventType.MouseDown)
            {
                //Process the event
                GetWorldPositionAtMouse(sceneView, e.mousePosition, out m_brushPosition, out m_brushNormal);
                if (e.control)
                {
                    m_isIntentToPaint = false;
                    m_isPainting = true;
                    m_isIntentToErase = false;
                    m_isErasing = false;
                    Undo.RecordObject(m_regionMask, "Added Point");
                    m_regionMask.AddPoint(m_brushPosition);
                }
                else if (e.alt)
                {
                    m_isIntentToPaint = false;
                    m_isPainting = false;
                    m_isIntentToErase = false;
                    m_isErasing = true;
                    Undo.RecordObject(m_regionMask, "Removed Point");
                    m_regionMask.RemovePoint(m_brushPosition);
                }
                else
                {
                    m_isIntentToPaint = false;
                    m_isPainting = false;
                    m_isIntentToErase = false;
                    m_isErasing = false;
                }
                e.Use();
                sceneView.Repaint();
                return;
            }

            if (e.type == EventType.MouseDrag)
            {
                //Process the event
                GetWorldPositionAtMouse(sceneView, e.mousePosition, out m_brushPosition, out m_brushNormal);
                if (m_isPainting)
                {
                    Undo.IncrementCurrentGroup();
                    Undo.RecordObject(m_regionMask, "Added Point");
                    m_regionMask.AddPoint(m_brushPosition, true);
                }
                else if (m_isErasing)
                {
                    Undo.IncrementCurrentGroup();
                    Undo.RecordObject(m_regionMask, "Removed Point");
                    m_regionMask.RemovePoint(m_brushPosition);
                }
                e.Use();
                sceneView.Repaint();
                return;
            }

            if (e.type == EventType.MouseUp)
            {
                //Process the event
                GetWorldPositionAtMouse(sceneView, e.mousePosition, out m_brushPosition, out m_brushNormal);
                if (e.control)
                {
                    m_isIntentToPaint = true;
                    m_isPainting = false;
                    m_isIntentToErase = false;
                    m_isErasing = false;
                }
                else if (e.alt)
                {
                    m_isIntentToPaint = false;
                    m_isPainting = false;
                    m_isIntentToErase = true;
                    m_isErasing = false;
                }
                else
                {
                    m_isIntentToPaint = false;
                    m_isPainting = false;
                    m_isIntentToErase = false;
                    m_isErasing = false;
                }
                e.Use();
                sceneView.Repaint();
                return;
            }
        }

        #endregion

        #region General command handlers and utilities

        /// <summary>
        /// Draw the brush
        /// </summary>
        private void DrawBrush(Event e)
        {
            Color oldColor = Handles.color;
            if (m_isIntentToPaint)
            {
                Handles.color = Color.blue;
            }
            else if (m_isIntentToErase)
            {
                Handles.color = Color.yellow;
            }
            else if (m_isPainting)
            {
                Handles.color = Color.green;
            }
            else if (m_isErasing)
            {
                Handles.color = Color.red;
            }
            else
            {
                Handles.color = Color.white;
            }
            Handles.DrawLine(m_brushPosition, m_brushPosition + (Vector3.up * 2f), 2f);
            Handles.DrawWireDisc(m_brushPosition, m_brushNormal, m_regionMask.BrushRadius, 2f);
            Handles.color = oldColor;
        }

        /// <summary>
        /// Draw the nodes as a visualization
        /// </summary>
        private void DrawActiveAndNextNode()
        {
            //Just want to show the selected node and the next node
            if (m_regionMask.ControlPoints.Count == 0)
            {
                return;
            }
            Color color = Handles.color;
            Handles.BeginGUI();
            //Draw the selected node button
            DrawNode(HandleUtility.WorldToGUIPoint(m_regionMask.ControlPoints[m_regionMask.SelectedNodeIndex].Position), Color.red);
            if (m_regionMask.SelectedNodeIndex < m_regionMask.ControlPoints.Count - 2)
            {
                DrawNode(HandleUtility.WorldToGUIPoint(m_regionMask.ControlPoints[m_regionMask.SelectedNodeIndex + 1].Position), Color.green);
            }
            else
            {
                DrawNode(HandleUtility.WorldToGUIPoint(m_regionMask.ControlPoints[0].Position), Color.green);
            }
            Handles.EndGUI();
        }

        /// <summary>
        /// Draw all the nodes as a visualization
        /// </summary>
        private void DrawAllNodes()
        {
            //Just want to show the selected node and the next node
            if (m_regionMask.ControlPoints.Count == 0)
            {
                return;
            }
            Color oldColor = Handles.color;
            Handles.BeginGUI();
            int nextNodeIndex = m_regionMask.SelectedNodeIndex + 1;
            if (nextNodeIndex >= m_regionMask.ControlPoints.Count)
            {
                nextNodeIndex = 0;
            }
            for (int i = 0;i < m_regionMask.ControlPoints.Count; i++)
            {
                StormRegionMaskNode node = m_regionMask.ControlPoints[i];
                if (m_regionMask.IsOnScreen(node.Position))
                {
                    if (i == m_regionMask.SelectedNodeIndex)
                    {
                        //Draw the selected node button
                        DrawNode(HandleUtility.WorldToGUIPoint(node.Position), Color.red);
                    }
                    else if (i == nextNodeIndex)
                    {
                        //Draw the direction button in a different color signifying the direction
                        DrawNode(HandleUtility.WorldToGUIPoint(node.Position), Color.green);
                    }
                    else
                    {
                        DrawNode(HandleUtility.WorldToGUIPoint(node.Position), Color.blue);
                    }
                }
            }
            Handles.EndGUI();
            Handles.color = oldColor;
        }

        /// <summary>
        /// Draw a node at the position with the color provided
        /// </summary>
        /// <param name="position"></param>
        /// <param name="color"></param>
        public void DrawNode(Vector2 position, Color color)
        {
            Color oldColor = GUI.color;
            GUI.color = color;
            Rect buttonRect = new Rect(position - m_halfNodeBrushSize, m_nodeBrushSize);
            GUI.DrawTexture(buttonRect, m_nodeButtonTexture, ScaleMode.ScaleToFit);
            GUI.color = oldColor;
        }

        /// <summary>
        /// Draw the nodes as buttons and process node editing and movement
        /// </summary>
        private void DrawNodesAndProcessMovement()
        {
            //Handle node editing
            if (m_regionMask.SelectedNodeIndex < m_regionMask.ControlPoints.Count)
            {
                StormRegionMaskNode activeNode = m_regionMask.ControlPoints[m_regionMask.SelectedNodeIndex];
                if (Tools.current == Tool.Move)
                {
                    Vector3 pos = Handles.PositionHandle(activeNode.Position, Quaternion.identity);
                    if (pos != activeNode.Position)
                    {
                        pos.y += 1000f;

                        //Pin it to the terrain
                        RaycastHit hit;
                        Ray ray = new Ray(pos, Vector3.down);
                        if (Physics.Raycast(ray, out hit, 3000f, m_regionMask.GroundLayers))
                        {
                            activeNode.Position = hit.point;
                        }
                        m_regionMask.UpdateVisualization();
                        m_regionMask.UpdateStormData();
                        return;
                    }
                }
                else if (Tools.current == Tool.Rotate)
                {
                    Quaternion rot = Handles.RotationHandle(Quaternion.identity, activeNode.Position);
                    if (rot != Quaternion.identity)
                    {
                        //Debug.Log("Ignoring node rotation");
                        return;
                    }
                }
                else if (Tools.current == Tool.Scale)
                {
                    Vector3 scale = Handles.ScaleHandle(Vector3.one, activeNode.Position, Quaternion.identity, 1f);
                    if (scale != Vector3.one)
                    {
                        //Debug.Log("Ignoring node scale");
                        return;
                    }
                }
            }

            //Draw the nodes buttons and select nodes. Show where current node is and next node would be added.
            Color color = Handles.color;
            Handles.BeginGUI();
            int nextNodeIndex = m_regionMask.SelectedNodeIndex + 1;
            if (nextNodeIndex >= m_regionMask.ControlPoints.Count)
            {
                nextNodeIndex = 0;
            }
            for (int i = 0; i < m_regionMask.ControlPoints.Count; i++)
            {
                StormRegionMaskNode node = m_regionMask.ControlPoints[i];
                if (m_regionMask.IsOnScreen(node.Position))
                {
                    if (i == m_regionMask.SelectedNodeIndex)
                    {
                        //Draw the selected node button
                        if (DrawNodeButton(HandleUtility.WorldToGUIPoint(node.Position), Color.red))
                        {
                            //Debug.Log("Clicked on node " + i);
                        }
                    }
                    else if (i == nextNodeIndex)
                    {
                        //Draw the direction button in a different color signifying the direction
                        if (DrawNodeButton(HandleUtility.WorldToGUIPoint(node.Position), Color.green))
                        {
                            //Debug.Log("Selecting node " + i);
                            m_regionMask.SelectNodeByIndex(i);
                        }
                    }
                    else
                    {
                        //Draw the normal node button
                        if (DrawNodeButton(HandleUtility.WorldToGUIPoint(node.Position), Color.blue))
                        {
                            //Debug.Log("Selecting node " + i);
                            m_regionMask.SelectNodeByIndex(i);
                        }
                    }
                }
            }
            Handles.color = color;
            Handles.EndGUI();
        }

        /// <summary>
        /// Draw a node button at the position with the texture and color provided
        /// </summary>
        /// <param name="position"></param>
        /// <param name="texture2D"></param>
        /// <param name="color"></param>
        /// <returns></returns>
        public bool DrawNodeButton(Vector2 position, Color color)
        {
            Color oldColor = GUI.color;
            GUI.color = color;
            Rect buttonRect = new Rect(position - m_halfNodeBrushSize, m_nodeBrushSize);
            bool result = GUI.Button(buttonRect, m_nodeButtonTexture, GUIStyle.none);
            GUI.color = oldColor;
            return result;
        }

        /// <summary>
        /// Get the world position from the mouse position
        /// </summary>
        /// <param name="sceneView">Scene we are in</param>
        /// <param name="mousePosition">Mouse position</param>
        /// <param name="hitPoint">Hitpoint returned</param>
        /// <param name="hitNormal">Hitnormal returned</param>
        /// <returns>True if we hit something</returns>
        private bool GetWorldPositionAtMouse(SceneView sceneView, Vector2 mousePosition, out Vector3 hitPoint, out Vector3 hitNormal)
        {
            Ray ray = HandleUtility.GUIPointToWorldRay(mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 500f, m_regionMask.GroundLayers))
            {
                hitPoint = hit.point;
                hitNormal = hit.normal;
                return true;
            }

            // If no hit, project to a plane at y=0
            Plane plane = new Plane(Vector3.up, Vector3.zero);
            float distance;
            if (plane.Raycast(ray, out distance))
            {
                hitPoint = ray.GetPoint(distance);
                hitNormal = Vector3.up;
                return true;
            }

            //General failure
            hitPoint = Vector3.zero;
            hitNormal = Vector3.up;
            return false;
        }

        #endregion
    }
}