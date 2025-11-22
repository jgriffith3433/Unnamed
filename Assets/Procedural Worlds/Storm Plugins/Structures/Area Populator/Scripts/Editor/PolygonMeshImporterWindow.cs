using ProceduralWorlds.Storm.Core;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace ProceduralWorlds.Storm.AreaPopulator
{
    /// <summary>
    /// The PolygonMeshImporterWindow class represents a custom editor window for importing polygon mesh data into a Unity scene.
    /// </summary>
    public class PolygonMeshImporterWindow : EditorWindow
    {
        [SerializeField] private StormPolygonMask polygonData;
        [SerializeField] private bool useCrossSection;
        [SerializeField] private float crossSectionY;
        [SerializeField] private float tolerance;

        private static readonly Color crossSectionFaceColor = new Color(0f, 1f, 0f, .25f);
        private static readonly Color crossSectionOutlineColor = new Color(0f, 0.22f, 0f);

        private GameObject meshReference;
        private Bounds meshReferenceBounds;
        private MeshFilter[] meshFilters;
        private MeshRenderer[] meshRenderers;

        /// <summary>
        /// Opens the "Polygon Mesh Importer" Window.
        /// </summary>
        [MenuItem("Window/Polygon Mesh Importer")]
        public static void ShowWindow()
        {
            GetWindow<PolygonMeshImporterWindow>("Polygon Mesh Importer");
        }

        /// <summary>
        /// Opens the Polygon Mesh Importer window and displays the specified polygon data.
        /// </summary>
        /// <param name="polygonData">The data of the polygon to be displayed in the window.</param>
        public static void ShowWindow(StormPolygonMask polygonData)
        {
            PolygonMeshImporterWindow polygonMeshWindow =
                GetWindow<PolygonMeshImporterWindow>("Polygon Mesh Importer");
            polygonMeshWindow.polygonData = polygonData;
        }

        /// <summary>
        /// Calculates the overall bounds of an array of MeshRenderers.
        /// </summary>
        /// <param name="renderers">The array of MeshRenderers to calculate bounds from.</param>
        /// <returns>The overall bounds that encapsulate all MeshRenderers in the array.</returns>
        public static Bounds CalculateBounds(MeshRenderer[] renderers)
        {
            if (renderers == null || renderers.Length == 0)
                return new Bounds();

            MeshRenderer firstRenderer = renderers[0];
            Transform firstTransform = firstRenderer.transform;
            Bounds firstBounds = firstRenderer.bounds;
            Vector3 boundsCenter = firstBounds.center - firstTransform.position;
            Vector3 boundsSize = firstBounds.size;
            Bounds bounds = new Bounds(boundsCenter, boundsSize);
            foreach (MeshRenderer renderer in renderers)
            {
                Transform rendererTransform = renderer.transform;
                Bounds rendererBounds = renderer.bounds;
                boundsCenter = rendererBounds.center - rendererTransform.position;
                boundsSize = rendererBounds.size;
                bounds.Encapsulate(new Bounds(boundsCenter, boundsSize));
            }

            return bounds;
        }

        private void OnEnable()
        {
            SceneView.duringSceneGui -= OnSceneGUI;
            SceneView.duringSceneGui += OnSceneGUI;
        }

        private void OnDisable()
        {
            SceneView.duringSceneGui -= OnSceneGUI;
        }

        private void OnDestroy()
        {
            SceneView.duringSceneGui -= OnSceneGUI;
        }

        private void OnGUI()
        {
            polygonData = (StormPolygonMask)EditorGUILayout.ObjectField(polygonData, typeof(StormPolygonMask), false);
            if (polygonData == null)
                return;

            UpdateMeshReference();

            if (meshReference == null)
                return;

            tolerance = EditorGUILayout.Slider("Tolerance", tolerance, -10f, 100f);
            UpdateCrossSectionUsage();
            UpdateSceneIfChanged();
            ProcessMeshOnButtonPress();
        }

        private void OnSceneGUI(SceneView sceneView)
        {
            if (useCrossSection && meshReference != null)
            {
                Vector3 boundsSize = meshReferenceBounds.size * 2.0f;
                Vector3 halfBoundsSize = boundsSize * 0.5f;

                Rect rect = new Rect(-halfBoundsSize.x, -halfBoundsSize.z, boundsSize.x, boundsSize.z);

                Vector3 center = meshReference.transform.position + Vector3.up * crossSectionY;

                Handles.matrix = Matrix4x4.TRS(center, Quaternion.Euler(90f, 0f, 0f), Vector3.one);
                Handles.zTest = CompareFunction.LessEqual;
                Handles.DrawSolidRectangleWithOutline(rect, crossSectionFaceColor, crossSectionOutlineColor);

                float cubeSize = 0.1f; // Adjustable size of cubes

                Vector3[] corners = new Vector3[]
                {
                    new Vector3(rect.xMin, rect.yMin, 0),
                    new Vector3(rect.xMax, rect.yMin, 0),
                    new Vector3(rect.xMin, rect.yMax, 0),
                    new Vector3(rect.xMax, rect.yMax, 0)
                };

                foreach (Vector3 corner in corners)
                {
                    Handles.CubeHandleCap(0, corner, Quaternion.identity, cubeSize, EventType.Repaint);
                }
            }
        }

        private void UpdateMeshReference()
        {
            EditorGUI.BeginChangeCheck();
            {
                meshReference = (GameObject)EditorGUILayout.ObjectField(meshReference, typeof(GameObject), true);
            }
            if (EditorGUI.EndChangeCheck())
            {
                UpdateMeshRenderersAndBounds();
            }
        }

        private void UpdateMeshRenderersAndBounds()
        {
            if (meshReference != null)
            {
                meshFilters = meshReference.GetComponentsInChildren<MeshFilter>();
                meshRenderers = meshReference.GetComponentsInChildren<MeshRenderer>();
                meshReferenceBounds = CalculateBounds(meshRenderers);
            }
        }

        private void UpdateCrossSectionUsage()
        {
            EditorGUI.BeginChangeCheck();
            {
                useCrossSection = EditorGUILayout.Toggle("Use Cross Section", useCrossSection);
                if (useCrossSection)
                {
                    EditorGUI.indentLevel++;
                    {
                        crossSectionY = EditorGUILayout.FloatField("Cross Section (Y)", crossSectionY);
                    }
                    EditorGUI.indentLevel--;
                }
            }
        }

        private void UpdateSceneIfChanged()
        {
            if (EditorGUI.EndChangeCheck())
            {
                SceneView.RepaintAll();
            }
        }

        private void ProcessMeshOnButtonPress()
        {
            if (GUILayout.Button("Process Mesh"))
            {
                CreatePolygonsFromMesh();
                SceneView.RepaintAll();
            }
        }

        private void CreatePolygonsFromMesh()
        {
            UpdateMeshRenderersAndBounds();

            // List<StormPolygonPoint> polygonPoints = new List<StormPolygonPoint>();
            // for (int i = 0; i < meshFilters.Length; i++)
            // {
            //     var mesh = meshFilters[i].sharedMesh;
            //     var points = ConcaveHullGenerator.GetBorder(mesh);
            //     for (int x = 0; x < points.Count; x++)
            //     {
            //         Vector3 point = points[x];
            //         polygonPoints.Add(new StormPolygonPoint(point.x, point.z));
            //     }
            // }

            // List<StormPolygonPoint> polygonPoints = new List<StormPolygonPoint>();
            // ConvexHullCreationResult<DefaultVertex2D> result = ConvexHull.Create2D(vector3s, tolerance);
            // if (result.Outcome == ConvexHullCreationResultOutcome.Success)
            // {
            //     IList<DefaultVertex2D> value = result.Result;
            //     foreach (DefaultVertex2D point in value)
            //     {
            //         StormPolygonPoint polygonPoint = new StormPolygonPoint(point.X, point.Y);
            //         polygonPoints.Add(polygonPoint);
            //     }
            // }
            // Hull.setConvexHull(nodes);

            // List<List<Vector3>> points = ConcaveHullDrawer.CalculateEdgeLoops(meshes);
            //
            // List<StormPolygonPoint> polygonPoints = new List<StormPolygonPoint>();
            // foreach (List<Vector3> pair in points)
            // {
            //     for (int i = 0; i < pair.Count; i++)
            //     {
            //         Vector3 point = pair[i];
            //         StormPolygonPoint polygonPoint = new StormPolygonPoint(point.x, point.z);
            //         polygonPoints.Add(polygonPoint);
            //     }
            // }


            // polygonData.points = polygonPoints;

            SceneView.RepaintAll();
        }
    }
}