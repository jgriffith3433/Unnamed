using System;
using System.Collections.Generic;
using Clipper2Lib;
using ProceduralWorlds.Storm.Core;
using ProceduralWorlds.Storm.Utils;
using PWCommon5;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditorInternal;
using UnityEngine;

namespace ProceduralWorlds.Storm.StormEditor
{
    public class StormPolygonDataEditor : PWEditor
    {
        #region Statics / Consts

        private const float HandleSize = 0.05f;
        private const float EdgeInsertionDistance = 0.01f;
        private const float EdgeRemovalDistance = 0.005f;
        private const float PointSelectionDistance = 0.005f;
        private static readonly RaycastHit[] m_allocatedHits = new RaycastHit[1000];
        private static Color m_applyStormColor = new Color(0.47f, 0.72f, 0f);
        private static GUIContent m_editContent;
        private static GUIStyle m_toggleButtonStyle;

        private static GUIStyle toggleButtonStyle
        {
            get
            {
                if (m_toggleButtonStyle == null)
                    m_toggleButtonStyle = new GUIStyle("Button")
                    {
                        
                    };
                return m_toggleButtonStyle;
            }
        }

        private static GUIStyle m_applyButtonStyle = null;

        private static GUIStyle applyButtonStyle
        {
            get
            {
                if (m_applyButtonStyle == null)
                {
                    m_applyButtonStyle = new GUIStyle("Button");
                }

                return m_applyButtonStyle;
            }
        }

        #endregion

        /// <summary>
        /// Unity callback for drawing Gizmos. This function is called once per frame in Editor mode.
        /// </summary>
        public struct GizmoPoints
        {
            public Color color;
            public Vector3[] points;
        }

        [NonSerialized] private List<GizmoPoints> m_pointsToDraw = new List<GizmoPoints>();

        public List<GizmoPoints> pointsToDraw => m_pointsToDraw;
        private bool editMode;
        private int selectedPolygonIndex = -1;
        private int selectedPointIndex = -1;
        private int selectedOffsetIndex = -1;

        protected EditorUtils m_editorUtils;
        private ReorderableList offsetList;

        // private Color m_discardColor = new Color(0.72f, 0.06f, 0f);
        private bool m_repaintSceneView = false;
        private bool m_repaintInspector = false;

        private Tool m_lastTool;

        private bool IsPointSelected => selectedPolygonIndex >= 0 && selectedPointIndex >= 0;

        private StormPolygonData m_polygonData;

        public StormPolygonData polygonData
        {
            get => m_polygonData;
            set
            {
                m_polygonData = value;
                RefreshList();
            }
        }

        private UnityEngine.Object m_ownerObject;

        public UnityEngine.Object ownerObject
        {
            get => m_ownerObject;
            set
            {
                m_ownerObject = value;
                RefreshList();
            }
        }

        public Transform transformObject;

        public bool debuggingEnabled = false;

        protected void Initialize()
        {
            if (m_editorUtils == null)
                return;
            m_editorUtils.Initialize();
        }

        private void OnEnable()
        {
            // If there isn't any Editor Utils Initialized
            if (m_editorUtils == null)
                m_editorUtils = StormApp.GetEditorUtils(this);

            if (m_editContent == null)
                m_editContent = EditorGUIUtility.IconContent("EditCollider");

            Undo.undoRedoPerformed -= UndoRedoPerformed;
            Undo.undoRedoPerformed += UndoRedoPerformed;
        }

        private void OnDestroy()
        {
            if (editMode)
                Tools.current = m_lastTool;
            Undo.undoRedoPerformed -= UndoRedoPerformed;
        }

        private void OnDisable()
        {
            if (editMode)
                Tools.current = m_lastTool;
            Undo.undoRedoPerformed -= UndoRedoPerformed;
            if (polygonData != null)
                polygonData.ApplyToStorm();
        }

        public void InspectorGUI(bool helpEnabled)
        {
            Initialize();

            DrawEditPolygonPanel(helpEnabled);
            DrawLayersList(helpEnabled);
            DrawApplyToStormButton();

            CheckForRepaint();
        }

        public void SceneGUI(bool drawProjection = true)
        {
            List<StormPolygon> polygons = polygonData.polygons;

            Event e = Event.current;
            Vector2 mousePosition = e.mousePosition;

            DrawPoints(drawProjection);

            if (e.control && !e.shift)
            {
                Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);

                // Define a plane at y = 0 (or at the height of your polygon)
                Plane xzPlane = new Plane(Vector3.up, transformObject.position);

                bool wasHit = xzPlane.Raycast(ray, out float hitDist);
                Vector3 hitPoint3D;
                StormPolygonHit hit;
                bool isRemovingEdge = false;
                // Check if the ray intersects with the XZ plane
                if (wasHit)
                {
                    // Get the intersection point on the XZ plane
                    hitPoint3D = ray.GetPoint(hitDist);

                    StormPolygonPoint hitPoint = hitPoint3D.ToStormPolygonPoint(transformObject);
                    bool foundClosestEdge = FindClosestPoint(hitPoint, out hit);
                    isRemovingEdge = editMode && foundClosestEdge && hit.distance < EdgeRemovalDistance * hitDist;
                    if (isRemovingEdge)
                    {
                        Vector3 point = polygonData.GetPoint3D(hit.polygonIndex, hit.pointIndex, transformObject);
                        float size = HandleUtility.GetHandleSize(point);
                        Handles.color = Color.red;
                        Handles.DotHandleCap(0, point, Quaternion.identity, size * HandleSize, EventType.Repaint);
                    }
                    else
                    {
                        if (GetTerrainHitPoint(e.mousePosition, out Vector3 terrainHitPoint))
                        {
                            hitPoint3D = terrainHitPoint;
                        }

                        Handles.color = Color.white;
                        Handles.CircleHandleCap(0, hitPoint3D, Quaternion.LookRotation(Vector3.up), 1f,
                            EventType.Repaint);
                    }
                }
                else
                {
                    wasHit = GetTerrainHitPoint(e.mousePosition, out hitPoint3D);
                    hit = default;
                }
                if (wasHit)
                {
                    switch (e.type)
                    {
                        case EventType.MouseDown:
                            if (e.button == 0)
                            {
                                // Set the hotControl to your control to lock focus
                                GUIUtility.hotControl = GUIUtility.GetControlID(FocusType.Passive);
                                // Create a unique control ID
                                if (isRemovingEdge)
                                {
                                    Undo.RecordObject(ownerObject, "Remove Point");
                                    polygonData.RemovePoint(hit.polygonIndex, hit.pointIndex);
                                }
                                else
                                {
                                    Undo.RecordObject(ownerObject, "Add Point");
                                    if (polygons.Count == 0)
                                    {
                                        polygons.Add(new StormPolygon());
                                    }
                                    polygonData.AddPoint(polygons.Count - 1, hitPoint3D.ToStormPolygonPoint(transformObject));
                                }
                                RefreshPointsToDraw();
                                EditorUtility.SetDirty(ownerObject);
                                GUIUtility.hotControl = 0;
                                e.Use();
                                m_repaintInspector = true;
                            }

                            break;

                        case EventType.MouseUp:
                            // Release the hotControl when done
                            GUIUtility.hotControl = 0;
                            e.Use();
                            break;
                    }
                }
            }
            else
            {
                if (editMode)
                {
                    // Constantly refresh Scene View while in Edit Mode.
                    SceneView.RepaintAll();

                    // Check if user is editing edge and a point is not yet selected
                    if (!CheckEditEdge(e) && !IsPointSelected)
                    {
                        // Perform edge insertion logic
                        HandleEdgeInsertion(e);
                    }

                    // Is a point selected?
                    if (IsPointSelected)
                    {
                        StormPolygon polygon = polygons[selectedPolygonIndex];
                        Vector3 newPoint3D = polygonData.GetPoint3D(selectedPolygonIndex, selectedPointIndex, transformObject);
                        if (e.type == EventType.MouseDrag)
                        {
                            // Convert the 2D mouse position into a Ray
                            Ray ray = HandleUtility.GUIPointToWorldRay(mousePosition);

                            // Define a plane at y = 0 (or any other Y value where your polygon lies)
                            Plane xzPlane = new Plane(Vector3.up, transformObject.position);

                            // Check if the ray intersects with the XZ plane
                            if (xzPlane.Raycast(ray, out float enter))
                            {
                                // Get the intersection point on the XZ plane
                                Vector3 hitPoint = ray.GetPoint(enter);

                                // Update the selected polygon point with the new X and Z coordinates
                                polygon.points[selectedPointIndex] = hitPoint.ToStormPolygonPoint(transformObject);
                                RefreshPointsToDraw();
                                e.Use();
                            }
                        }

                        if (e.type == EventType.MouseUp)
                        {
                            GUIUtility.hotControl = GUIUtility.GetControlID(FocusType.Passive);
                            Undo.RecordObject(ownerObject, "Move Point");
                            polygon.points[selectedPointIndex] = newPoint3D.ToStormPolygonPoint(transformObject);
                            EditorUtility.SetDirty(target);
                            m_repaintInspector = true;

                            selectedPolygonIndex = -1;
                            selectedPointIndex = -1;
                            GUIUtility.hotControl = 0;
                            e.Use();
                        }
                    }
                }
            }

            CheckForRepaint();
        }

        private void DrawPoints(bool drawProjection = true)
        {
            List<StormPolygon> polygons = polygonData.polygons;
            List<StormPolygonLayer> layers = polygonData.layers;
            Color originalColor = Handles.color;
            for (int polygonIndex = 0; polygonIndex < polygons.Count; polygonIndex++)
            {
                Vector3[] polygonPoints = polygonData.GetPoints3D(polygonIndex, transformObject);
                if(drawProjection)
                {
                    Vector3[] offsetPoints = new Vector3[polygonPoints.Length];
                    for (int offsetIndex = 0; offsetIndex < polygonPoints.Length; offsetIndex++)
                    {
                        offsetPoints[offsetIndex] = StormPolygonUtility.GetTerrainPosition(polygonPoints[offsetIndex]);
                    }
                    if (editMode)
                    {
                        Handles.color = Color.white;
                        DrawPolygonConnections(polygonPoints, offsetPoints);
                    }
                    Handles.color = Color.red;
                    DrawPolygon(offsetPoints);
                }

                Handles.color = Color.white;
                DrawPolygon(polygonPoints);
                if (editMode)
                {
                    Handles.color = new Color(0.6f, 0.6f, 0.6f, 1f);
                    DrawPolygonPoints(polygonPoints);
                }
                for (int layerIndex = 0; layerIndex < layers.Count; layerIndex++)
                {
                    StormPolygonLayer layer = polygonData.GetLayer(layerIndex);
                    Vector3[] offsetPoints = polygonData.GetOffsetPoints3D(polygonIndex, layerIndex, transformObject);
                    for (int offsetIndex = 0; offsetIndex < offsetPoints.Length; offsetIndex++)
                    {
                        offsetPoints[offsetIndex] = StormPolygonUtility.GetTerrainPosition(offsetPoints[offsetIndex]);
                    }
                    Handles.color = layer != null ? layer.color : Color.magenta;
                    DrawPolygon(offsetPoints);
                }
            }
            Handles.color = originalColor;
        }

        private void RefreshPointsToDraw()
        {
            if (polygonData != null)
                polygonData.ApplyToStorm();
            /*/
            m_pointsToDraw.Clear();

            var polygons = polygonData.polygons;
            var layers = polygonData.layers;
            for (int polygonIndex = 0; polygonIndex < polygons.Count; polygonIndex++)
            {
                var polygonPoints = polygonData.GetPoints3D(polygonIndex, transformObject);
                
                // Not in edit mode?
                if (!editMode)
                {
                    // Snap all points to terrain
                    for (int pointIndex = 0; pointIndex < polygonPoints.Length; pointIndex++)
                    {
                        polygonPoints[pointIndex] = StormPolygonUtility.GetTerrainPosition(polygonPoints[pointIndex]);
                    }
                }

                m_pointsToDraw.Add(new GizmoPoints
                {
                    color = Color.white,
                    points = polygonPoints
                });
                for (int layerIndex = 0; layerIndex < layers.Count; layerIndex++)
                {
                    StormPolygonLayer layer = polygonData.GetLayer(layerIndex);
                    var offsetPoints = polygonData.GetOffsetPoints3D(polygonIndex, layerIndex, transformObject);
                    for (int offsetIndex = 0; offsetIndex < offsetPoints.Length; offsetIndex++)
                    {
                        offsetPoints[offsetIndex] = StormPolygonUtility.GetTerrainPosition(offsetPoints[offsetIndex]);
                    }

                    m_pointsToDraw.Add(new GizmoPoints
                    {
                        color = layer != null ? layer.color : Color.magenta,
                        points = offsetPoints
                    });
                }
            }
            //*/
        }

        private void UndoRedoPerformed()
        {
            RefreshPointsToDraw();
            polygonData.ApplyToStorm();
        }

        /// <summary>
        /// Draws polygon in Unity Gizmos using a list of PolygonPoints.
        /// </summary>
        /// <param name="polygonPoints">List of PolygonPoints that form the polygon.</param>
        private void DrawPolygon(Vector3[] polygonPoints)
        {
            for (int i = 0; i < polygonPoints.Length; i++)
            {
                Vector3 currentPos = polygonPoints[i];
                Vector3 nextPos = polygonPoints[(i + 1) % polygonPoints.Length];
                Handles.DrawLine(currentPos, nextPos);

                if (debuggingEnabled)
                {
                    DrawString($"Node {i}", currentPos);
                }
            }
        }
        /// <summary>
        /// Draws polygon in Unity Gizmos using a list of PolygonPoints.
        /// </summary>
        /// <param name="polygonPoints">List of PolygonPoints that form the polygon.</param>
        private void DrawPolygonPoints(Vector3[] polygonPoints)
        {
            for (int i = 0; i < polygonPoints.Length; i++)
            {
                Vector3 currentPos = polygonPoints[i];
                Handles.DotHandleCap(0, currentPos, Quaternion.identity, HandleUtility.GetHandleSize(currentPos) * HandleSize, EventType.Repaint);
            }
        }

        /// <summary>
        /// Draws connections between two polygons in Unity Gizmos using two lists of PolygonPoints.
        /// </summary>
        /// <param name="polygonPoints">List of PolygonPoints that form the polygon.</param>
        private void DrawPolygonConnections(Vector3[] polygonPointsA, Vector3[] polygonPointsB)
        {
            for (int i = 0; i < polygonPointsA.Length && i < polygonPointsB.Length; i++)
            {
                Vector3 currentPos = polygonPointsA[i];
                Vector3 nextPos = polygonPointsB[i];
                Handles.DrawDottedLine(currentPos, nextPos, 5);
            }
        }

        public void DrawPolygons()
        {
            if (pointsToDraw == null)
                return;
            for (int i = 0; i < pointsToDraw.Count; i++)
            {
                GizmoPoints polygonToDraw = pointsToDraw[i];
                Handles.color = polygonToDraw.color;
                DrawPolygon(polygonToDraw.points);
            }
        }

        private void CheckForRepaint()
        {
            if (m_repaintSceneView)
            {
                m_repaintSceneView = false;
                SceneView.RepaintAll();
            }
            
            if (m_repaintInspector)
            {
                m_repaintInspector = false;
                Repaint();
            }
        }

        public void DrawApplyToStormButton()
        {
            EditorGUILayout.BeginHorizontal();
            {
                Color oldBackgroundColor = GUI.backgroundColor;
                GUI.backgroundColor = m_applyStormColor;
                if (m_editorUtils.Button("ApplyToStorm", applyButtonStyle))
                {
                    polygonData.ApplyToStorm();
                }
                GUI.backgroundColor = oldBackgroundColor;
                m_editorUtils.HelpToggle(ref buttonHelp);
            }
            EditorGUILayout.EndHorizontal();
            m_editorUtils.InlineHelp("ApplyToStorm", buttonHelp);
        }

        private bool buttonHelp = false;

        public void ProcessMeshes()
        {
            StormPolygonShapeType shapeType = polygonData.polygonShapeType;
            Undo.RecordObject(ownerObject, $"Calculate Polyygon: {shapeType}");

            polygonData.polygons = StormPolygonUtility.CalculatePolygons(transformObject, shapeType);
            RefreshPointsToDraw();

            SceneView.RepaintAll();
            EditorUtility.SetDirty(ownerObject);

            if (polygonData.layers.Count > 0)
            {
                polygonData.ApplyToStorm();
            }
        }

        /// <summary>
        /// Adds a layer to the polygonData dictionary and sets the offset value for the layer.
        /// </summary>
        private void OnAddLayer(string layer)
        {
            Undo.RecordObject(ownerObject, $"Added Layer: {layer}");
            int count = polygonData.layers.Count;
            // Set the offset to the index, which is the current count of elements in the dictionary
            polygonData.layers.Add(new StormPolygonLayer()
            {
                name = layer,
                color = StormPolygonUtility.GetUniqueColor(count),
                offset = count * 10,
                joinType = JoinType.Round
            });
            EditorUtility.SetDirty(ownerObject);
            RefreshPointsToDraw();
        }

        /// <summary>
        /// Refreshes the list of layers and offsets in the polygon data object.
        /// </summary>
        private SerializedObject m_serializedOwner;

        private SerializedProperty m_polygonDataProperty;
        private SerializedProperty m_layersProperty;

        private void RefreshList()
        {
            if (m_polygonData == null || ownerObject == null)
                return;
            RefreshPointsToDraw();
            try
            {
                m_serializedOwner = new SerializedObject(ownerObject);
                m_polygonDataProperty = m_serializedOwner.FindProperty("m_polygonData");
                m_layersProperty = m_polygonDataProperty.FindPropertyRelative("m_layers");
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                return;
            }

            offsetList = new ReorderableList(m_serializedOwner, m_layersProperty, false, true, true, true) {
                drawHeaderCallback = (Rect rect) => EditorGUI.LabelField(rect, "Layers"),
                elementHeight = (EditorGUIUtility.standardVerticalSpacing + EditorGUIUtility.singleLineHeight) * 2,
                drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
                {
                    if (isActive || isFocused)
                    {
                        selectedOffsetIndex = index;
                    }

                    if (index < polygonData.layers.Count)
                    {
                        StormPolygonLayer layer = polygonData.layers[index];

                        float paddingY = 2f;
                        float colorSquareWidth = 30f;
                        float colorSquareSize = EditorGUIUtility.singleLineHeight - 2 * paddingY;

                        // Draw Color Square
                        Rect colorSquareRect = new Rect(rect.x, rect.y + paddingY,
                            colorSquareSize + colorSquareWidth, colorSquareSize + 3f);
                        layer.color = EditorGUI.ColorField(colorSquareRect, layer.color);

                        // Adjusted layout using halves of the width
                        float offset = colorSquareSize + 10f + colorSquareWidth;

                        Rect layerNameRect = new Rect(rect.x + offset, rect.y + paddingY,
                            rect.width - offset - 5f, EditorGUIUtility.singleLineHeight);
                        Rect valueLabelRect = new Rect(rect.x, layerNameRect.yMax + EditorGUIUtility.standardVerticalSpacing,
                            (rect.width - 20f) * 0.165f, EditorGUIUtility.singleLineHeight);
                        Rect valueRect = new Rect(valueLabelRect.xMax + 5f, valueLabelRect.y,
                            (rect.width - 20f) * 0.33f, valueLabelRect.height);
                        Rect falloffLabelRect = new Rect(valueRect.xMax + 5f, valueLabelRect.y,
                            valueLabelRect.width, valueLabelRect.height);
                        Rect falloffRect = new Rect(falloffLabelRect.xMax + 5f, valueLabelRect.y,
                            valueRect.width, valueLabelRect.height);

                        SerializedProperty layerProperty = m_layersProperty.GetArrayElementAtIndex(index);
                        SerializedProperty layerNameProperty = layerProperty.FindPropertyRelative("name");
                        SerializedProperty polysProperty = m_polygonDataProperty.FindPropertyRelative("m_polygons");
                        while (polysProperty.arraySize <= index)
                            polysProperty.InsertArrayElementAtIndex(index);
                        SerializedProperty polyProperty = polysProperty.GetArrayElementAtIndex(index);
                        SerializedProperty valueProperty = polyProperty.FindPropertyRelative("Value");
                        SerializedProperty falloffProperty = polyProperty.FindPropertyRelative("Falloff");

                        EditorGUI.BeginChangeCheck();
                        {
                            EditorGUI.PropertyField(layerNameRect, layerNameProperty);
                            GUI.Label(valueLabelRect, m_editorUtils.GetContent("PolygonValue"));
                            valueProperty.floatValue = EditorGUI.FloatField(valueRect, valueProperty.floatValue);
                            GUI.Label(falloffLabelRect, m_editorUtils.GetContent("PolygonFalloff"));
                            falloffProperty.floatValue = EditorGUI.FloatField(falloffRect, falloffProperty.floatValue);
                        }
                        if (EditorGUI.EndChangeCheck())
                        {
                            polygonData.ApplyToStorm();
                        }
                    }
                },
                onCanAddCallback = (ReorderableList l) =>
                {
                    StormWorld stormWorld = StormWorld.Instance;
                    if (stormWorld == null)
                        return false;

                    StormLayerSet globalLayerSet = stormWorld.GlobalLayerSet;
                    if (globalLayerSet == null)
                        return false;

                    // Can add!
                    return true;
                },
                onAddDropdownCallback = (Rect buttonRect, ReorderableList l) =>
                {
                    StormWorld stormWorld = StormWorld.Instance;
                    StormLayerSet globalLayerSet = stormWorld.GlobalLayerSet;

                    List<string> layers = new List<string>(globalLayerSet.Layers);
                    List<StormLayerType> layerTypes = new List<StormLayerType>(globalLayerSet.LayerTypes);

                    StormLayerSearchProvider searchProvider =
                        ScriptableObject.CreateInstance<StormLayerSearchProvider>();
                    searchProvider.layers = layers.ToArray();
                    searchProvider.layerTypes = layerTypes.ToArray();
                    searchProvider.onSelectedCallback = result =>
                    {
                        if (result == "ADD_NEW_LAYER")
                        {
                            result = "\nADD_NEW_LAYER\n";
                        }
                        OnAddLayer(result);
                    };
                    var center = buttonRect.center;
                    var windowPosition = new Vector2(center.x, center.y);
                    var screenPosition = GUIUtility.GUIToScreenPoint(windowPosition);
                    SearchWindow.Open(new SearchWindowContext(screenPosition), searchProvider);
                },
                onRemoveCallback = (ReorderableList l) =>
                {
                    Undo.RecordObject(ownerObject, "Remove List Element");
                    int indexToRemove = selectedOffsetIndex;
                    if (indexToRemove < 0 || indexToRemove >= l.count)
                        return;
                    polygonData.layers.RemoveAt(indexToRemove);
                    EditorUtility.SetDirty(ownerObject);
                    SceneView.RepaintAll();
                }
            };
        }

        /// <summary>
        /// Displays a point in the scene using handles. Allows the user to select and move the point.
        /// </summary>
        private bool CheckEditEdge(Event e)
        {
            Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);

            // Define a plane at y = 0 (or at the height of your polygon)
            Plane xzPlane = new Plane(Vector3.up, transformObject.position);

            // Check if the ray intersects with the XZ plane
            if (xzPlane.Raycast(ray, out float hitDist))
            {
                // Get the intersection point on the XZ plane
                Vector3 hitPoint3D = ray.GetPoint(hitDist);

                StormPolygonPoint hitPoint = hitPoint3D.ToStormPolygonPoint(transformObject);

                if (FindClosestPoint(hitPoint, out StormPolygonHit hit))
                {
                    List<StormPolygon> polygons = polygonData.polygons;
                    if (hit.polygonIndex < 0 || hit.polygonIndex >= polygons.Count)
                        return false;

                    StormPolygon polygon = polygons[hit.polygonIndex];
                    if (polygon == null)
                        return false;

                    Vector3 point3D = polygonData.GetPoint3D(hit.polygonIndex, hit.pointIndex, transformObject);

                    float size = HandleUtility.GetHandleSize(point3D);

                    if (hit.distance < PointSelectionDistance * hitDist)
                    {
                        Handles.color = Color.white;
                        Handles.DotHandleCap(0, point3D, Quaternion.identity, size * HandleSize, EventType.Repaint);

                        if (e.type == EventType.MouseDown && e.button == 0)
                        {
                            GUIUtility.hotControl = GUIUtility.GetControlID(FocusType.Passive);
                            selectedPolygonIndex = hit.polygonIndex;
                            selectedPointIndex = hit.pointIndex;
                            GUIUtility.hotControl = 0;
                        }

                        return true;
                    }
                }
            }

            return false;
        }

        private bool GetTerrainHitPoint(Vector2 mousePosition, out Vector3 terrainHitPoint)
        {
            terrainHitPoint = Vector3.zero;
            Ray ray = HandleUtility.GUIPointToWorldRay(mousePosition);
            int hitAmount = Physics.RaycastNonAlloc(ray, m_allocatedHits);
            for (int i = 0; i < hitAmount; i++)
            {
                RaycastHit hit = m_allocatedHits[i];
                Transform hitTransform = hit.transform;
                if (hitTransform == null)
                    continue;
                UnityEngine.Terrain hitTerrain = hitTransform.GetComponent<UnityEngine.Terrain>();
                if (hitTerrain == null)
                    continue;
                terrainHitPoint = hit.point;
                terrainHitPoint.y = hitTerrain.SampleHeight(terrainHitPoint);
                return true;
            }

            return false;
        }


        /// <summary>
        /// Handles the insertion of an edge in the polygon.
        /// </summary>
        /// <param name="e">Current Event</param>
        private void HandleEdgeInsertion(Event e)
        {
            Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);

            // Define a plane at y = 0 (or at the height of your polygon)
            Plane xzPlane = new Plane(Vector3.up, transformObject.position);

            // Check if the ray intersects with the XZ plane
            if (xzPlane.Raycast(ray, out float hitDist))
            {
                // Get the intersection point on the XZ plane
                Vector3 hitPoint3D = ray.GetPoint(hitDist);

                StormPolygonPoint hitPoint = hitPoint3D.ToStormPolygonPoint(transformObject);

                if (FindClosestEdge(hitPoint, out StormPolygonHit hit))
                {
                    List<StormPolygon> polygons = polygonData.polygons;
                    if (hit.polygonIndex < 0 || hit.polygonIndex >= polygons.Count)
                        return;

                    StormPolygon polygon = polygons[hit.polygonIndex];
                    if (polygon == null)
                        return;

                    Vector3 closestPoint3D = hit.point.ToWorld(transformObject);
                    float size = HandleUtility.GetHandleSize(closestPoint3D); // Adjust as needed

                    // Highlight edge and draw circle at the closest point
                    if (hit.distance < EdgeInsertionDistance * hitDist)
                    {
                        Handles.color = Color.green;
                        Handles.DotHandleCap(0, closestPoint3D, Quaternion.identity, size * HandleSize,
                            EventType.Repaint);

                        if (e.type == EventType.MouseDown && e.button == 0)
                        {
                            GUIUtility.hotControl = GUIUtility.GetControlID(FocusType.Passive);

                            Undo.RecordObject(ownerObject, "Add Point");

                            int newPointIndex = hit.pointIndex + 1;
                            polygon.points.Insert(newPointIndex, closestPoint3D.ToStormPolygonPoint(transformObject));
                            polygonData.ApplyToStorm();

                            EditorUtility.SetDirty(target);

                            // Update the selected index to the newly created point
                            selectedPolygonIndex = hit.polygonIndex;
                            selectedPointIndex = newPointIndex;
                            GUIUtility.hotControl = 0;
                            e.Use();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Finds the closest edge to a given point in the StormPolygonData.
        /// </summary>
        /// <param name="point">The point to find the closest edge to.</param>
        /// <param name="stormPolygonHit">The struct that will hold information about the closest edge.</param>
        /// <returns>True if a closest edge is found, false otherwise.</returns>
        public bool FindClosestPoint(StormPolygonPoint point, out StormPolygonHit stormPolygonHit)
        {
            stormPolygonHit.polygonIndex = -1;
            stormPolygonHit.pointIndex = -1;
            stormPolygonHit.distance = double.MaxValue;
            stormPolygonHit.point = null;
            bool closestFound = false;

            List<StormPolygon> polygons = polygonData.polygons;
            for (int polygonIndex = 0; polygonIndex < polygons.Count; polygonIndex++)
            {
                StormPolygon polygon = polygons[polygonIndex];
                // Check distance to each edge
                List<StormPolygonPoint> points = polygon.points;
                for (int pointIndex = 0; pointIndex < points.Count; pointIndex++)
                {
                    StormPolygonPoint currentPoint = points[pointIndex];
                    double edgeDist = StormPolygonPoint.Distance(currentPoint, point);
                    if (edgeDist < stormPolygonHit.distance)
                    {
                        stormPolygonHit.polygonIndex = polygonIndex;
                        stormPolygonHit.pointIndex = pointIndex;
                        stormPolygonHit.distance = edgeDist;
                        stormPolygonHit.point = currentPoint;
                        closestFound = true;
                    }
                }
            }

            return closestFound;
        }

        /// <summary>
        /// Finds the closest edge to a given point in the StormPolygonData.
        /// </summary>
        /// <param name="point">The point to find the closest edge to.</param>
        /// <param name="stormPolygonHit">The struct that will hold information about the closest edge.</param>
        /// <returns>True if a closest edge is found, false otherwise.</returns>
        public bool FindClosestEdge(StormPolygonPoint point, out StormPolygonHit stormPolygonHit)
        {
            stormPolygonHit.polygonIndex = -1;
            stormPolygonHit.pointIndex = -1;
            stormPolygonHit.distance = double.MaxValue;
            stormPolygonHit.point = null;
            bool closestFound = false;

            List<StormPolygon> polygons = polygonData.polygons;
            for (int polygonIndex = 0; polygonIndex < polygons.Count; polygonIndex++)
            {
                StormPolygon polygon = polygons[polygonIndex];
                // Check distance to each edge
                List<StormPolygonPoint> points = polygon.points;
                for (int pointIndex = 0; pointIndex < points.Count; pointIndex++)
                {
                    StormPolygonPoint start = points[pointIndex];
                    StormPolygonPoint end = points[(pointIndex + 1) % points.Count];
                    StormPolygonPoint pointOnEdge = StormPolygonPoint.ClosestPointOnLineSegment(start, end, point);
                    double edgeDist = StormPolygonPoint.Distance(pointOnEdge, point);
                    if (edgeDist < stormPolygonHit.distance)
                    {
                        stormPolygonHit.polygonIndex = polygonIndex;
                        stormPolygonHit.pointIndex = pointIndex;
                        stormPolygonHit.distance = edgeDist;
                        stormPolygonHit.point = pointOnEdge;
                        closestFound = true;
                    }
                }
            }

            return closestFound;
        }

        /// <summary>
        /// Draws the "Add Point" button and handles the logic associated with it.
        /// </summary>
        public void DrawEditPolygonPanel(bool helpEnabled, bool simpleMode = false)
        {
            if(!simpleMode)
            {
                EditorGUILayout.BeginHorizontal();
                {
                    StormPolygonShapeType shapeType = polygonData.polygonShapeType;
                    StormPolygonShapeType oldShapeType = shapeType;
                    shapeType = (StormPolygonShapeType)m_editorUtils.EnumPopup("CalculatePolygon", shapeType);
                    if (shapeType != oldShapeType)
                    {
                        Undo.RecordObject(ownerObject, $"Polygon Shape Type: {shapeType}");
                        polygonData.polygonShapeType = shapeType;
                        EditorUtility.SetDirty(target);
                    }

                    if (m_editorUtils.Button("RecalculatePolygon", GUILayout.Height(EditorGUIUtility.singleLineHeight)))
                    {
                        ProcessMeshes();
                    }
                }
                EditorGUILayout.EndHorizontal();
                m_editorUtils.InlineHelp("CalculatePolygon", helpEnabled);
                m_editorUtils.InlineHelp("RecalculatePolygon", helpEnabled);
            }
            EditorGUILayout.BeginHorizontal();
            {
                if (editMode)
                {
                    toggleButtonStyle.normal = toggleButtonStyle.active;
                }

                if (editMode && Tools.current != Tool.None && Event.current.type == EventType.Layout)
                {
                    editMode = false;
                }
                bool oldEditMode = editMode;
                editMode = GUILayout.Toggle(editMode, "EditPolygon", toggleButtonStyle);    
                if (oldEditMode != editMode)
                {
                    if (editMode)
                    {
                        m_lastTool = Tools.current;
                        Tools.current = Tool.None;
                    }
                    else
                    {
                        Tools.current = m_lastTool;
                    }
                    RefreshPointsToDraw();
                    m_repaintSceneView = true;
                }

                if (m_editorUtils.Button("ClearAllPoints", helpEnabled))
                {
                    GUIContent clearDialog = m_editorUtils.GetContent("ClearAllPointsDialog");
                    GUIContent clearButtons = m_editorUtils.GetContent("ClearAllPointsButtons");
                    if (EditorUtility.DisplayDialog(clearDialog.text, clearDialog.tooltip, clearButtons.text, clearButtons.tooltip))
                    {
                        Undo.RecordObject(ownerObject, "Clear All Points");
                        polygonData.ClearAllPoints();
                        EditorUtility.SetDirty(ownerObject);
                        RefreshPointsToDraw();
                    }
                }
            }
            EditorGUILayout.EndHorizontal();
            m_editorUtils.InlineHelp("EditPolygon", helpEnabled);

        }

        private void DrawLayersList(bool helpEnabled)
        {
            serializedObject?.Update();

            m_serializedOwner?.Update();

            offsetList?.DoLayoutList();

            m_editorUtils.InlineHelp("Layers", helpEnabled);

            List<StormPolygonLayer> layers = polygonData.layers;
            // If there is a selected Offset in the List
            if (selectedOffsetIndex >= 0 && selectedOffsetIndex < layers.Count)
            {
                // Offset Label and Field
                StormPolygonLayer layer = layers[selectedOffsetIndex];

                EditorGUILayout.LabelField($"Selected Layer: {layer.name}", EditorStyles.boldLabel);
                EditorGUI.BeginChangeCheck();
                {
                    layer.joinType = (JoinType)m_editorUtils.EnumPopup("JoinType", layer.joinType, helpEnabled);
                    layer.offset = m_editorUtils.FloatField("Offset", layer.offset, helpEnabled);
                }
                if (EditorGUI.EndChangeCheck())
                {
                    RefreshPointsToDraw();
                    SceneView.RepaintAll();
                    polygonData.ApplyToStorm();
                }
            }

            m_serializedOwner?.ApplyModifiedProperties();

            serializedObject?.ApplyModifiedProperties();
        }

        public static void DrawString(string text, Vector3 worldPos, Vector2 screenOffset = default,
            GUIStyle guiStyle = null)
        {
            if (guiStyle == null)
                guiStyle = EditorStyles.numberField;
            Handles.BeginGUI();
            SceneView view = SceneView.currentDrawingSceneView;
            Vector3 screenPos = view.camera.WorldToScreenPoint(worldPos);
            if (screenPos.y < 0 || screenPos.y > Screen.height || screenPos.x < 0 || screenPos.x > Screen.width ||
                screenPos.z < 0)
            {
                Handles.EndGUI();
                return;
            }

            Handles.Label(TransformByPixel(worldPos, screenOffset), text, guiStyle);
            Handles.EndGUI();
        }

        public static Vector3 TransformByPixel(Vector3 position, Vector3 screenOffset)
        {
            Camera sceneCam = SceneView.currentDrawingSceneView.camera;
            if (sceneCam)
            {
                Vector3 guiPoint = sceneCam.WorldToScreenPoint(position);
                guiPoint += screenOffset;
                return sceneCam.ScreenToWorldPoint(guiPoint);
            }

            return position;
        }
    }
}