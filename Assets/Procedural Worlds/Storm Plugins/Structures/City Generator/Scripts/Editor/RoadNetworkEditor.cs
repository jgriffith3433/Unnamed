using UnityEditor;
using ProceduralWorlds.Storm.StormEditor;
using PWCommon5;
using UnityEngine;

namespace ProceduralWorlds.Storm.CityBuilder
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(RoadNetwork), true)] // Enables inheritance
    public class RoadNetworkEditor : PWEditor, IPWEditor
    {
        public bool m_defaultOpenPanels = true;

        //Node button size constants
        private Vector2 m_nodeBrushSize = new Vector2(25f, 25f);
        private Vector2 m_halfNodeBrushSize = new Vector2(12.5f, 12.5f);

        public Texture2D m_nodeButtonTexture;
        protected EditorUtils m_editorUtils;

        private RoadNetwork m_RoadNetworkInstance;
        private RoadNetworkData m_RoadNetworkData;

        private const string m_nodeButtonTextureName = "UI/Skin/Knob.psd";

        SerializedProperty p_RoadNetworkData;
        SerializedProperty p_drawNodes;
        SerializedProperty p_MaxDrawDistance;
        SerializedProperty p_roadNodeSizeMultiplier;

        private const double NODE_CREATION_COOLDOWN = 0.6f; // 1 second cooldown
        private double m_LastNodeCreationTime;

        protected virtual void OnEnable()
        {
            Initialize();
            // Initialize RoadNetwork properties
            p_RoadNetworkData = serializedObject.FindProperty("roadNetworkData");
            p_drawNodes = serializedObject.FindProperty("drawNodes");
            p_MaxDrawDistance = serializedObject.FindProperty("MaxDrawDistance");
            p_roadNodeSizeMultiplier = serializedObject.FindProperty("roadNodeSizeMultiplier");
           
            Undo.undoRedoPerformed -= OnUndo;
            Undo.undoRedoPerformed += OnUndo;
        }

        /// <summary>
        /// Initialize the system
        /// </summary>
        private void Initialize()
        {
            //Create and initialize editor utils
            if (m_editorUtils == null)
            {
                m_editorUtils = StormApp.GetEditorUtils(this);
            }

            m_RoadNetworkInstance = (RoadNetwork)target;
            m_RoadNetworkInstance.selectedRoadPointIndex = 0;
            m_RoadNetworkData = m_RoadNetworkInstance.roadNetworkData;
            m_RoadNetworkInstance.GetAllRoadPoints();

            //Make sure we have our button texture loaded
            if (m_nodeButtonTexture == null)
            {
                m_nodeButtonTexture = AssetDatabase.GetBuiltinExtraResource<Texture2D>(m_nodeButtonTextureName);
            }

            //Register for editor events and 
            SceneView.duringSceneGui -= OnSceneGUIUpdate;
            SceneView.duringSceneGui += OnSceneGUIUpdate;
        }

        protected virtual void OnDisable()
        {
            Undo.undoRedoPerformed -= OnUndo;
            SceneView.duringSceneGui -= OnSceneGUIUpdate;
        }

        protected virtual void OnDestroy()
        {
            OnDisable();
        }

        public override void OnInspectorGUI()
        {
            if (m_editorUtils == null)
            {
                m_editorUtils = StormApp.GetEditorUtils(this);
            }
            m_RoadNetworkInstance = (RoadNetwork)target;

            m_editorUtils.Initialize();

            m_editorUtils.Panel("RoadNetworkPanel", RoadNetworkPanel, m_defaultOpenPanels);

            serializedObject.ApplyModifiedProperties();
        }

        protected virtual void RoadNetworkPanel(bool helpActive)
        {
            PropField("RoadNetworkData", p_RoadNetworkData, helpActive);
            PropField("DrawNodes", p_drawNodes, helpActive);
            PropField("MaxDrawDistance", p_MaxDrawDistance, helpActive);
            PropField("RoadNodeSizeMultiplier", p_roadNodeSizeMultiplier, helpActive);
        }

        public void OnSceneGUIUpdate(SceneView sceneView)
        {
            Event e = Event.current;
            if (e == null)
            {
                return;
            }
            
            if (sceneView == null)
            {
                return;
            }

            //Exit on unity center or right mouse button based nav based events
            if ((e.button == 1 || e.button == 2) && (!e.shift || !e.alt))
            {
                return;
            }

            //If shift is pressed, then we are in view and edit nodes mode
            if (e.shift)
            {
                DrawNodesAndProcessMovement(sceneView, e);
                return;
            }
            if (!m_RoadNetworkInstance.drawNodes)
            {
                return;
            }

            DrawAllNodes();
        }

        #region Road Network Editing Helpers

        private void CreateNewNode(SceneView sceneView, Event e, RoadType roadType)
        {
            // Check if enough time has passed since last node creation
            double currentTime = EditorApplication.timeSinceStartup;
            if (currentTime - m_LastNodeCreationTime < NODE_CREATION_COOLDOWN)
            {
                return; // Still in cooldown period
            }

            Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
            RaycastHit hit;
            Vector3 nodePosition;

            if (Physics.Raycast(ray, out hit))
            {
                // Use the exact hit point if we hit terrain or other geometry
                nodePosition = hit.point;
            }
            else
            {
                // If we don't hit anything, create a plane at the same height as the last point
                float planeHeight = 0f;

                Plane horizontalPlane = new Plane(Vector3.up, Vector3.zero);
                float distance;
                
                if (horizontalPlane.Raycast(ray, out distance))
                {
                    nodePosition = ray.GetPoint(distance);
                }
                else
                {
                    // Fallback - this should rarely happen unless looking exactly parallel to the plane
                    nodePosition = ray.GetPoint(20f);
                    nodePosition.y = planeHeight;
                }
            }
            
            m_RoadNetworkInstance.AddRoadPoint(nodePosition, roadType);
            m_LastNodeCreationTime = currentTime; // Update the last creation time
        }


        /// <summary>
        /// Draw all the nodes as a visualization
        /// </summary>
        private void DrawAllNodes()
        {
            //Just want to show the selected node and the next node
            if (m_RoadNetworkInstance.allRoadPoints.Count == 0)
            {
                return;
            }
            Color oldColor = Handles.color;
            Handles.BeginGUI();

            for (int i = 0;i < m_RoadNetworkInstance.allRoadPoints.Count; i++)
            {
                Color nodeColor = Color.red;
                if(i >= m_RoadNetworkInstance.roadNetworkData.majorRoadPoints.Count)
                {
                    nodeColor = Color.blue;
                }
                Vector3 node = m_RoadNetworkInstance.allRoadPoints[i];
                if (m_RoadNetworkInstance.IsOnScreen(node))
                {
                    if (i == m_RoadNetworkInstance.selectedRoadPointIndex)
                    {
                        //Draw the selected node button
                        DrawNode(HandleUtility.WorldToGUIPoint(node), Color.green);
                    }
                    else
                    {
                        DrawNode(HandleUtility.WorldToGUIPoint(node), nodeColor);
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
            Rect buttonRect = new Rect(position - m_halfNodeBrushSize * m_RoadNetworkInstance.roadNodeSizeMultiplier, m_nodeBrushSize * m_RoadNetworkInstance.roadNodeSizeMultiplier);
            GUI.DrawTexture(buttonRect, m_nodeButtonTexture, ScaleMode.ScaleToFit);
            GUI.color = oldColor;
        }

        /// <summary>
        /// Draw the nodes as buttons and process node editing and movement
        /// </summary>
        private void DrawNodesAndProcessMovement(SceneView sceneView, Event e)
        {
            //Handle node editing
            if (m_RoadNetworkInstance.selectedRoadPointIndex >= 0 && m_RoadNetworkInstance.selectedRoadPointIndex < m_RoadNetworkInstance.allRoadPoints.Count)
            {
                Vector3 activeNode = m_RoadNetworkInstance.allRoadPoints[m_RoadNetworkInstance.selectedRoadPointIndex];
                if (Tools.current == Tool.Move)
                {
                    Vector3 pos = Handles.PositionHandle(activeNode, Quaternion.identity);
                    if (pos != activeNode)
                    {
                        //Updates the RoadPointInfo connections and its neighbors connections with it
                        m_RoadNetworkInstance.UpdateRoadPoint(m_RoadNetworkInstance.selectedRoadPointIndex, pos, e.control);

                        return;
                    }
                }
            }

            if (e.keyCode == KeyCode.C)
            {
                CreateNewNode(sceneView, e, RoadType.Major);
            }
            else if (e.keyCode == KeyCode.V)
            {
                CreateNewNode(sceneView, e, RoadType.Minor);
            }


            //Draw the nodes buttons and select nodes. Show where current node is and next node would be added.
            Color color = Handles.color;
            Handles.BeginGUI();
            for (int i = 0; i < m_RoadNetworkInstance.allRoadPoints.Count; i++)
            {
                Color nodeColor = Color.red;
                if(i >= m_RoadNetworkInstance.roadNetworkData.majorRoadPoints.Count)
                {
                    nodeColor = Color.blue;
                }
                Vector3 node = m_RoadNetworkInstance.allRoadPoints[i];
                if (m_RoadNetworkInstance.IsOnScreen(node))
                {
                    if (i == m_RoadNetworkInstance.selectedRoadPointIndex)
                    {
                        //Draw the selected node button
                        if (DrawNodeButton(HandleUtility.WorldToGUIPoint(node), Color.green))
                        {
                            if(e.alt && e.button == 2)
                            {
                                m_RoadNetworkInstance.RemoveRoadPoint(m_RoadNetworkInstance.selectedRoadPointIndex);
                            }
                        }
                    }
                    else
                    {
                        //Draw the normal node button
                        if (DrawNodeButton(HandleUtility.WorldToGUIPoint(node), nodeColor))
                        {
                            if(e.alt && e.button == 0)
                            {          
                                //create connection to selected node
                                m_RoadNetworkInstance.CreateConnectionToPoint(m_RoadNetworkInstance.selectedRoadPointIndex, node);
                            }
                            else if(e.alt && e.button == 1)
                            {
                                //remove connection from selected node
                                m_RoadNetworkInstance.RemoveConnectionFromPoint(m_RoadNetworkInstance.selectedRoadPointIndex, node);
                            }
                            else if(e.alt && e.button == 2)
                            {
                                m_RoadNetworkInstance.RemoveRoadPoint(node);
                            }
                            else
                            {
                                m_RoadNetworkInstance.SelectNodeByIndex(i);
                            }

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
            if (Physics.Raycast(ray, out hit))
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
        protected void PropField(string key, SerializedProperty prop, bool helpActive)
        {
            EditorGUI.showMixedValue = prop.hasMultipleDifferentValues;
            m_editorUtils.PropertyField(key, true, prop, helpActive);
            EditorGUI.showMixedValue = false;
        }

        protected void OnUndo()
        {
            serializedObject.Update();
            serializedObject.ApplyModifiedProperties();
        }
    }
}
