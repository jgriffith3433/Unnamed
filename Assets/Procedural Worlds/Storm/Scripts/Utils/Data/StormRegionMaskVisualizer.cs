using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace ProceduralWorlds.Storm.Utils
{
    /// <summary>
    /// Visualizes a storm region mask in the editor.
    /// </summary>
    [ExecuteInEditMode]
    public class StormRegionMaskVisualizer : MonoBehaviour
    {
        #region Serialized Private Fields

        [SerializeField]
        private List<Vector3> m_points = new List<Vector3>(); // the polygon points we are visualizing in local space

        [SerializeField]
        private LayerMask m_groundLayers = ~0;

        [SerializeField]
        private float m_gridResolution = 1f;

        [SerializeField]
        private int m_maxMeshVertices = 8000;

        [SerializeField]
        private Color m_visualizationColour = new(.2f, .8f, 1f, .35f);

        [SerializeField]
        private bool m_showVisualization = true;

        #endregion

        #region Non Serialized Private Fields
        private bool m_needsPointsUpdate = true;  
        private bool m_needsMeshRebuild = true;
        private bool m_needsColorUpdate = true;
        private Matrix4x4 m_lastParentL2W; 
        private Mesh m_mesh;
        private Bounds m_worldBounds;
        private Material m_visualizationMat;
        private ComputeBuffer m_visualizationBuffer;
        private static int _PointsId = int.MinValue;
        private static int _PointCountId = int.MinValue;
        private static int _PolyColorId = int.MinValue;
        private static RaycastHit[] _hits = new RaycastHit[1];

        #endregion

        #region Public properties

        [Tooltip("Show the visualization")]
        /// <summary>
        /// Whether to show the visualization mesh
        /// </summary>
        public bool ShowVisualization
        {
            get 
            { 
                return m_showVisualization; 
            }
            set 
            { 
                if (value != m_showVisualization)
                {
                    m_showVisualization = value;
                    m_needsPointsUpdate = true;
                    m_needsMeshRebuild = true;
                    m_needsColorUpdate = true;
                }
            }
        }

        [Tooltip("Region vertices as a closed polygon in world space, CW or CCW, >= 3")]
        /// <summary>
        /// The points we are rendering in world space.
        /// </summary>
        public List<Vector3> Points 
        {
            get
            {
                return m_points;
            }
            set
            {
                m_needsPointsUpdate = true;
                m_points.Clear();
                if (value != null)
                {
                    m_points.AddRange(value);
                }
                if (m_points.Count >= 3 && !WorldBoundsContainAll(m_points))
                {
                    m_needsMeshRebuild = true;
                }
            }
        }

        [Tooltip("The layers that are considered ground for the raycast for the visualization mesh.")]
        /// <summary>
        /// The layers that are considered ground for the raycast for the visualization mesh.
        /// </summary>
        public LayerMask GroundLayers
        {
            get
            {
                return m_groundLayers;
            }
            set
            {
                m_groundLayers = value;
                m_needsPointsUpdate = true;
                m_needsMeshRebuild = true;
            }
        }

        [Tooltip("The resolution of the grid used to visualize the region.")]
        /// <summary>
        /// The resolution of the grid used to visualize the region.
        /// </summary>
        public float VisualizationResolution
        {
            get
            {
                return m_gridResolution;
            }
            set
            {
                m_gridResolution = value;
                m_needsPointsUpdate = true;
                m_needsMeshRebuild = true;
            }
        }

        [Tooltip("The maximum number of vertices in the visualization mesh.")]
        /// <summary>
        /// The maximum number of vertices in the visualization mesh.
        /// </summary>
        public int MaxVisualizationMeshVertices
        {
            get
            {
                return m_maxMeshVertices;
            }
            set
            {
                m_maxMeshVertices = value;
                m_needsPointsUpdate = true;
                m_needsMeshRebuild = true;
            }
        }

        [Tooltip("The visualization color of the region.")]
        /// <summary>
        /// Visualization color of the region.
        /// </summary>
        public Color VisualizationColor
        {
            get
            {
                return m_visualizationColour;
            }
            set
            {
                m_visualizationColour = value;
                m_needsColorUpdate = true;
            }
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Update the visualization
        /// </summary>
        public void UpdateVisualization()
        {
            m_needsPointsUpdate = true;
            m_needsMeshRebuild = true;
            m_needsColorUpdate = true;
        }

        #endregion

        #region Unity event handling

        /// <summary>
        /// Whether the visualizer is initialized
        /// </summary>
        /// <returns></returns>
        private bool IsInitialized()
        {
            if (m_mesh == null)
            {
                return false;
            }

            if (m_visualizationMat == null)
            {
                return false;
            }

            if (_PointsId == int.MinValue)
            {
                return false;
            }

            if (!m_visualizationMat.HasProperty(_PointsId))
            {
                return false;
            }

            if (m_visualizationBuffer == null || m_visualizationBuffer.count != m_points.Count)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Initialize and update the visualizer
        /// </summary>
        private void Initialize()
        {
            //Debug.Log($"[StormRegionMaskVisualizer] Initialize");

            // Store parent matrix if parent exists
            if (transform.parent != null)
            {
                m_lastParentL2W = transform.parent.localToWorldMatrix;
            }
            else
            {
                m_lastParentL2W = Matrix4x4.identity;
            }

            // Initialize the shader property IDs
            if (_PointsId == int.MinValue)
            {
                _PointsId = Shader.PropertyToID("_Points");
                _PointCountId = Shader.PropertyToID("_PointCount");
                _PolyColorId = Shader.PropertyToID("_PolyColor");
            }

            // Get or create mesh filter
            var mf = GetComponent<MeshFilter>();
            if (mf == null)
            {
                mf = gameObject.AddComponent<MeshFilter>();
            }
            
            // Preserve mesh if possible, otherwise create new one
            if (mf.sharedMesh != null && mf.sharedMesh.name == "RegionVisGrid")
            {
                m_mesh = mf.sharedMesh;
            }
            else
            {
                m_mesh = new Mesh { name = "RegionVisGrid", indexFormat = IndexFormat.UInt32 };
                m_mesh.MarkDynamic();
                mf.sharedMesh = m_mesh;
            }

            // Setup material 
            if (m_visualizationMat == null)
            {
                Shader shader = Shader.Find("PWStorm/PolygonOverlay");
                if (shader == null)
                {
                    Debug.LogError("Could not find 'PWStorm/PolygonOverlay' shader");
                }
                m_visualizationMat = new Material(shader)
                { 
                    hideFlags = HideFlags.HideAndDontSave 
                };
            }

            // Get or create mesh renderer
            var mr = GetComponent<MeshRenderer>();
            if (mr == null)
            {
                mr = gameObject.AddComponent<MeshRenderer>();
                mr.shadowCastingMode = ShadowCastingMode.Off;
                mr.receiveShadows = false;
            }
            mr.sharedMaterial = m_visualizationMat;

            // Build the visualization mesh and upload points to GPU
            m_needsColorUpdate = true;
            m_needsPointsUpdate = true;
            m_needsMeshRebuild = true;
        }


        /// <summary>
        /// Update the visualization 
        /// </summary>
        private void Update()
        {
            // Initialize if needed
            if (!IsInitialized())
            {
                Initialize();
            }

            //Detect if parent has changed orientation, if so then we need to rebuild the mesh
            if (transform.parent != null && transform.parent.localToWorldMatrix != m_lastParentL2W)
            {
                m_lastParentL2W = transform.parent.localToWorldMatrix;
                m_needsMeshRebuild = true;
            }

            // Rebuild mesh if needed
            if (m_needsMeshRebuild)
            {
                BuildVisualizationMesh();
                if (m_needsMeshRebuild)
                    return;
            }
            
            // Update GPU if needed
            if (m_needsPointsUpdate)
            {
                UploadPointsToGPU();
            }

            // Update color if needed
            if (m_needsColorUpdate)
            {
                UpdateVisualizationColor();
            }
        }

        private void OnDestroy()
        {
            if (m_visualizationBuffer != null)
            {
                m_visualizationBuffer.Release();
                m_visualizationBuffer = null;
            }
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Builds the visualization mesh based on the points provided.
        /// </summary>
        private void BuildVisualizationMesh()
        {
            if (m_points.Count < 1)
                return;
            // 1. world-space X-Z bounds of polygon
            float minX = m_points[0].x, maxX = minX;
            float minZ = m_points[0].z, maxZ = minZ;
            foreach (var p in m_points)
            {
                if (p.x < minX) minX = p.x;
                if (p.x > maxX) maxX = p.x;
                if (p.z < minZ) minZ = p.z;
                if (p.z > maxZ) maxZ = p.z;
            }

            // 2. pad by half a cell and snap to grid 
            float h = m_gridResolution * 0.5f;

            minX = Mathf.Floor((minX - h) / m_gridResolution) * m_gridResolution;
            maxX = Mathf.Ceil((maxX + h) / m_gridResolution) * m_gridResolution;
            minZ = Mathf.Floor((minZ - h) / m_gridResolution) * m_gridResolution;
            maxZ = Mathf.Ceil((maxZ + h) / m_gridResolution) * m_gridResolution;

            float width = maxX - minX;          // after padding+snap
            float depth = maxZ - minZ;

            // 3. cell counts & vertex-budget down-sample 
            int cellsX = Mathf.Max(1, Mathf.RoundToInt(width / m_gridResolution));
            int cellsZ = Mathf.Max(1, Mathf.RoundToInt(depth / m_gridResolution));
            int proposedVerts = (cellsX + 1) * (cellsZ + 1);

            if (proposedVerts > m_maxMeshVertices)
            {
                // enlarge gridResolution just enough to hit the budget
                float scale = Mathf.Sqrt((float)proposedVerts / m_maxMeshVertices);
                m_gridResolution *= scale;

                /* re-apply padding + snap with the *new* grid size */
                h = m_gridResolution * 0.5f;

                minX = Mathf.Floor((minX - h) / m_gridResolution) * m_gridResolution;
                maxX = Mathf.Ceil((maxX + h) / m_gridResolution) * m_gridResolution;
                minZ = Mathf.Floor((minZ - h) / m_gridResolution) * m_gridResolution;
                maxZ = Mathf.Ceil((maxZ + h) / m_gridResolution) * m_gridResolution;

                width = maxX - minX;
                depth = maxZ - minZ;
                cellsX = Mathf.Max(1, Mathf.RoundToInt(width / m_gridResolution));
                cellsZ = Mathf.Max(1, Mathf.RoundToInt(depth / m_gridResolution));
            }

            // 4. allocate buffers 
            int vertCount = (cellsX + 1) * (cellsZ + 1);
            var vertsL = new List<Vector3>(vertCount);                 
            var uvs = new List<Vector2>(vertCount);
            var tris = new List<int>(cellsX * cellsZ * 6);

            // 5. sample height and build vertex grid
            for (int z = 0; z <= cellsZ; ++z)
                for (int x = 0; x <= cellsX; ++x)
                {
                    float wx = minX + x * m_gridResolution;
                    float wz = minZ + z * m_gridResolution;
                    Vector3 w = new Vector3(wx, 0f, wz);

                    Vector3 start = w + Vector3.up * 5000f;
                    if (Physics.RaycastNonAlloc(start, Vector3.down, _hits, 10000f, m_groundLayers) > 0)
                        w.y = _hits[0].point.y;                       // hit → real ground Y

                    vertsL.Add(transform.InverseTransformPoint(w));   // world → local
                    uvs.Add(new Vector2((float)x / cellsX, (float)z / cellsZ));
                }

            // 6. triangles 
            for (int z = 0; z < cellsZ; ++z)
                for (int x = 0; x < cellsX; ++x)
                {
                    int i0 = z * (cellsX + 1) + x;
                    int i1 = i0 + 1;
                    int i2 = i0 + cellsX + 1;
                    int i3 = i2 + 1;

                    tris.Add(i0); tris.Add(i2); tris.Add(i1);
                    tris.Add(i1); tris.Add(i2); tris.Add(i3);
                }

            // 7. upload to mesh 
            m_mesh.Clear();
            m_mesh.SetVertices(vertsL);
            m_mesh.SetTriangles(tris, 0);
            m_mesh.SetUVs(0, uvs);
            m_mesh.RecalculateNormals();
            m_mesh.RecalculateBounds();

            // store world AABB straight from Renderer — always correct
            m_worldBounds = GetComponent<Renderer>().bounds;
            m_needsMeshRebuild = false;
        }

        /// <summary>
        /// Upload points to gpu
        /// </summary>
        private void UploadPointsToGPU()
        {
            // Make sure we have a valid material
            if (m_visualizationMat == null)
            {
                return;
            }
            //this should handle 0 points by uploading a 1 size buffer and still setting point count to 0
            int numPoints = m_points.Count;
            int buffSize = Mathf.Max(1, numPoints);

            // Create or resize buffer if needed
            if (m_visualizationBuffer == null || m_visualizationBuffer.count != buffSize)
            {
                m_visualizationBuffer?.Release();
                m_visualizationBuffer = new ComputeBuffer(buffSize, sizeof(float) * 3);
            }
            
            m_visualizationBuffer.SetData(m_points);
            m_visualizationMat.SetInt(_PointCountId, numPoints);
            m_visualizationMat.SetBuffer(_PointsId, m_visualizationBuffer);

            //Signal done
            m_needsPointsUpdate = false;
        }

        /// <summary>
        /// Updates the visualization color
        /// </summary>
        private void UpdateVisualizationColor()
        {
            if (m_visualizationMat != null)
            {
                if (!m_showVisualization)
                {
                    m_visualizationMat.SetColor(_PolyColorId, Color.clear);
                }
                else
                {
                    m_visualizationMat.SetColor(_PolyColorId, m_visualizationColour);
                }
                m_needsColorUpdate = false;
            }
        }

        /// <summary>
        /// Checks if all points are within the world bounds.
        /// </summary>
        /// <param name="pts">List of points in world space</param>
        /// <returns></returns>
        private bool WorldBoundsContainAll(List<Vector3> pts)
        {
            foreach (var p in pts)
            {
                if (!m_worldBounds.Contains(p))
                { 
                    return false;
                }
            }
            return true;
        }

        #endregion
    }
}