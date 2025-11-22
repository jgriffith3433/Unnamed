using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
using static ProceduralWorlds.Storm.Terrain.StormVTEnums;

namespace ProceduralWorlds.Storm.Terrain
{
    public class StormVTTerrainColliderManager : MonoBehaviour
    {
        #region Public Variables

        /// <summary>
        /// Reference to the storm terrain settings
        /// </summary>
        public StormVTSettings stormVTSettings;

        /// <summary>
        /// Reference to the storm terrain grid
        /// </summary>
        public StormVTTerrainGrid stormVTTerrainGrid;

        /// <summary>
        /// Reference to the storm terrain texture manager
        /// </summary>
        public StormVTTerrainTextureManager stormVTTerrainTextureManager;

        /// <summary>
        /// The compute shader to create a collision mesh from a single terrain
        /// </summary>
        public ComputeShader generateTerrainSliceComputeShader;

        /// <summary>
        /// The amount of vertices per collider mesh
        /// </summary>
        [HideInInspector]
        public int m_meshResolution = 64;

        /// <summary>
        /// The world space size of each collider mesh
        /// </summary>
        [HideInInspector]
        public float m_patchSize = 32;

        /// <summary>
        /// The world space size of how many vertices per meter the collider meshes take up.
        /// </summary>
        [HideInInspector]
        public float m_density = 1.0f;

        /// <summary>
        /// The quality of the collision mesh resolution. Describes how it differs from full Unity Terrain partiy.
        /// </summary>
        [HideInInspector]
        public StormVTEnums.StormVTCollisionResolution m_collisionQuality = StormVTCollisionResolution._Full;

        /// <summary>
        /// The amount of collider meshes that fill a single terrain
        /// </summary>
        [HideInInspector]
        public int m_numPatchesPerTerrain;

        /// <summary>
        /// The distance in meters to try and create collider patches from
        /// </summary>
        [HideInInspector]
        public float m_colliderLoadingRange = 250.0f;

        /// <summary>
        /// The current main camera, used for tracking player's position
        /// </summary>
        public Camera m_mainCamera;

        /// <summary>
        /// The debug material for the collider meshes
        /// </summary>
        public Material m_previewMat;

        /// <summary>
        /// The total amount of vertices being drawn by this collision system
        /// </summary>
        [HideInInspector]
        public int m_totalVertexCount;

        /// <summary>
        /// The closet terrain coordinate to the player
        /// </summary>
        [HideInInspector]
        public int2 m_closetTerrainCoord;

        /// <summary>
        /// The search radius from the closet coord to test for
        /// </summary>
        [HideInInspector]
        public int m_numPatchNeighbours = 1;

        /// <summary>
        /// Stores a list of the surrounding colliders from the patch coord
        /// </summary>
        [HideInInspector]
        public List<int4> m_closetPatches = new List<int4>();

        /// <summary>
        /// The current patch coordinate of the terrain
        /// </summary>
        [HideInInspector]
        public int2 m_patchCoord;
        #endregion


        #region Private Variables

        /// <summary>
        /// A table of current collider gameobjects in use in the system
        /// </summary>
        private DictionaryQueue<int4, GameObject> m_terrainColliderQueue = new DictionaryQueue<int4, GameObject>();

        /// <summary>
        /// The kernal id to dispatch the collision mesh generation compute shader
        /// </summary>
        private int m_kernalID;

        /// <summary>
        /// The current list of vertices edited by the compute shader
        /// </summary>
        private ComputeBuffer m_vertexBuffer;

        /// <summary>
        /// A reference the collider mesh's original vertices, used for UV coordinate calculation in the compute shader
        /// </summary>
        private ComputeBuffer m_baseVertexBuffer;

        /// <summary>
        /// The size of each terrain in the scene
        /// </summary>
        private float m_terrainWorldSize;

        /// <summary>
        /// The previous list of collider meshes that surrounded the player
        /// </summary>
        private List<int4> m_previousNodes = new List<int4>();

        /// <summary>
        /// The old collider meshes that are no longer needed in the current position of the player
        /// </summary>
        private List<int4> m_oldNodes = new List<int4>();

        /// <summary>
        /// A list of collider generation requests that need to be completed
        /// </summary>
        private List<int4> m_pendingRuntimeLoadRequests = new List<int4>();

        /// <summary>
        /// Whether a new collider request is running. Prevents multiple requests running at once
        /// </summary>
        private bool m_requestRunning = false;

        private GameObject m_colliderParent;
        /// <summary>
        /// The base reference of the mesh used for generating collider meshes
        /// </summary>
        private Mesh m_gridMesh;

        /// <summary>
        /// Tracks whether the storm terrain settings have been updated
        /// </summary>
        private bool m_settingsUpdated = false;


        #endregion

        // Start is called before the first frame update
        public void Start()
        {
            if (stormVTSettings.m_stormVTMode == StormVTMode.Clipmap && stormVTSettings.m_collisionEnabled)
            {
                m_colliderParent = GameObject.Find("Storm Terrain Colliders");
                if(m_colliderParent == null)
                    m_colliderParent = new GameObject("Storm Terrain Colliders");
                m_terrainWorldSize = stormVTTerrainGrid.m_terrainSize;

                StormSettingsUpdated();
                StormVTTerrainTextureManager.OnNewTerrainAdded += NewTerrainTextureAdded;
                StormVTSettings.OnUpdateStormVTSettings += StormSettingsUpdated;

                //Check if there are already terrain colliders existing in the scene
                PopulateCollisionQueue();
            }
        }

        /// <summary>
        /// Update the values based on the values in the storm terrain settings
        /// </summary>
        private void StormSettingsUpdated()
        {
            if (stormVTSettings != null)
            {
                m_colliderLoadingRange = stormVTSettings.m_colliderLoadingRange;
                m_meshResolution = stormVTSettings.m_collisionMeshResolution;
                m_collisionQuality = stormVTSettings.m_collisionQuality;

                m_settingsUpdated = true;
            }
        }

        /// <summary>
        /// Every update, calculate if there is any new colliders that need to be created
        /// And work through the current list of colliders that still need to be created. 
        /// </summary>
        private void FixedUpdate()
        {
            if (stormVTSettings.m_stormVTMode == StormVTMode.Clipmap && stormVTSettings.m_collisionEnabled)
            {
                if (m_settingsUpdated)
                {
                    m_settingsUpdated = false;
                    SetupMeshCompute();
                    UpdateColliders();
                    UpdateDebugColliders();
                }

                UpdateColliders();
                if (m_pendingRuntimeLoadRequests.Count <= 0)
                {
                    return;
                }
                if (!m_requestRunning)
                {
                    int nodeIndex = m_pendingRuntimeLoadRequests.Count - 1;
                    //Get last request in queue
                    int4 m_activeRequest = m_pendingRuntimeLoadRequests[nodeIndex];
                    //Remove last request from queue
                    m_pendingRuntimeLoadRequests.RemoveAt(nodeIndex);
                    GenerateCollider(m_activeRequest);
                }
            }
            
        }

        /// <summary>
        /// Checks for previously generated terrain colliders, and fills the queue before more are addded
        /// </summary>
        public void PopulateCollisionQueue()
        {
            if (m_colliderParent != null)
            {
                Transform[] colliders = m_colliderParent.GetComponentsInChildren<Transform>();
                for (int i = 0; i < colliders.Length; i++)
                {
                    if (!colliders[i].name.Equals(m_colliderParent.name))
                    {
                        GameObject colliderGO = colliders[i].gameObject;
                        string patchName = colliderGO.name;
                        int4 coordinate = ExtractPatchCoordFromName(patchName);
                        if(m_terrainColliderQueue.ContainsKey(coordinate))
                        {
                            m_terrainColliderQueue[coordinate] = colliderGO;
                        }
                        else
                        {
                            m_terrainColliderQueue.Add(coordinate, colliderGO);
                        }


                    }
                }
            }
        }

        /// <summary>
        /// Extracts the int4 (terrain coord, patch coord) value from the name of a gameobject
        /// </summary>
        /// <param name="gameobjectName">The name of the patch gameobject</param>
        /// <returns></returns>
        public static int4 ExtractPatchCoordFromName(string gameobjectName)
        {
            // Split the input string by the underscore character
            string[] parts = gameobjectName.Split('_');

            // Get the last four elements and parse them to integers
            int x = int.Parse(parts[parts.Length - 4]);
            int y = int.Parse(parts[parts.Length - 3]);
            int z = int.Parse(parts[parts.Length - 2]);
            int w = int.Parse(parts[parts.Length - 1]);

            // Create and return the int4
            return new int4(x, y, z, w);
        }

        /// <summary>
        /// Updates the visual state of the colliders based on whether to display a debug material over them
        /// </summary>
        public void UpdateDebugColliders()
        {
            if(m_colliderParent != null)
            {
                Transform[] colliders = m_colliderParent.GetComponentsInChildren<Transform>();
                for(int i = 0; i < colliders.Length; i++)
                {
                    if (colliders[i] != m_colliderParent)
                    {
                        GameObject colliderGO = colliders[i].gameObject;
                        if (colliderGO != null)
                        {
                            MeshRenderer meshRenderer = colliderGO.GetComponent<MeshRenderer>();
                            if (stormVTSettings.m_debugColliderMeshes)
                            {
                                if (meshRenderer == null)
                                {
                                    meshRenderer = colliderGO.AddComponent<MeshRenderer>();
                                }
                                meshRenderer.enabled = true;
                            }
                            else
                            {
                                if (meshRenderer != null)
                                {
                                    meshRenderer.enabled = false;
                                }
                            }

                        }
                    }
                    
                }
            }
        }

        /// <summary>
        /// Computes the current colliders around the player
        /// </summary>
        public void UpdateColliders()
        {

            //Grab current player position
            Vector3 camPos = m_mainCamera.transform.position;

            Vector2 relCamPos = new Vector2(camPos.x - stormVTTerrainGrid.m_terrainBottomLeftPosition.x, camPos.z - stormVTTerrainGrid.m_terrainBottomLeftPosition.y);

            //Calculate world space terrain coord
            m_closetTerrainCoord = new int2(
                    (int)Mathf.Floor(relCamPos.x / m_terrainWorldSize),
                    (int)Mathf.Floor(relCamPos.y / m_terrainWorldSize));

            //Check that the closet terrain coordinate is a valid entry
            if (m_closetTerrainCoord.x > -1 && m_closetTerrainCoord.x < stormVTTerrainGrid.m_numTerrainsX && m_closetTerrainCoord.y > -1 && m_closetTerrainCoord.y < stormVTTerrainGrid.m_numTerrainsY)
            {
                int closetTerrainIndex = stormVTTerrainGrid.GetIndex(m_closetTerrainCoord.x, m_closetTerrainCoord.y);
                Vector2 bottomLeftTerrianPos = stormVTTerrainGrid.m_gridList[closetTerrainIndex].position;

                //Relative cam pos
                Vector2 relativeCamPos = new Vector2(camPos.x - bottomLeftTerrianPos.x, camPos.z - bottomLeftTerrianPos.y);
                m_patchCoord = new int2(
                    (int)Mathf.Floor(relativeCamPos.x / m_patchSize),
                    (int)Mathf.Floor(relativeCamPos.y / m_patchSize));

            }

            m_closetPatches.Clear();
            for (int x = -m_numPatchNeighbours; x <= m_numPatchNeighbours; x++)
            {
                for (int y = -m_numPatchNeighbours; y <= m_numPatchNeighbours; y++)
                {
                    int2 terrainCoord = m_closetTerrainCoord;
                    int2 currentPatch = m_patchCoord + new int2(x, y);

                    if(currentPatch.x < 0)
                    {
                        int terrainOffset = (int)(Mathf.Ceil(Mathf.Abs((float)currentPatch.x) / (float)m_numPatchesPerTerrain));
                        currentPatch.x = m_numPatchesPerTerrain + currentPatch.x;

                        terrainCoord.x = terrainCoord.x - terrainOffset;
                    }
                    if (currentPatch.y < 0)
                    {
                        int terrainOffset = (int)(Mathf.Ceil(Mathf.Abs((float)currentPatch.y) / (float)m_numPatchesPerTerrain));
                        currentPatch.y = m_numPatchesPerTerrain + currentPatch.y;

                        terrainCoord.y = terrainCoord.y - terrainOffset;
                    }
                    if(currentPatch.x > (m_numPatchesPerTerrain-1))
                    {
                        int terrainOffset = (int)(Mathf.Floor((float)currentPatch.x / (float)m_numPatchesPerTerrain));
                        currentPatch.x = currentPatch.x - m_numPatchesPerTerrain;

                        terrainCoord.x = terrainCoord.x + terrainOffset;
                    }
                    if (currentPatch.y > (m_numPatchesPerTerrain - 1))
                    {
                        int terrainOffset = (int)(Mathf.Floor((float)currentPatch.y / (float)m_numPatchesPerTerrain));
                        currentPatch.y = currentPatch.y - m_numPatchesPerTerrain;

                        terrainCoord.y = terrainCoord.y + terrainOffset;
                    }

                    int newTerrainIndex = stormVTTerrainGrid.GetIndex(terrainCoord.x, terrainCoord.y);
                    if (newTerrainIndex > -1 && newTerrainIndex < stormVTTerrainGrid.m_gridList.Length)
                    {
                      m_closetPatches.Add(new int4(terrainCoord, currentPatch));
                    }

                    

                }
            }

            //Delete old patches
            m_oldNodes = m_previousNodes.Except(m_closetPatches).ToList();
            for (int i = 0; i < m_oldNodes.Count; i++)
            {
                int4 coordinate = m_oldNodes[i];

                //Remove old requests are no longer needed
                if(m_pendingRuntimeLoadRequests.Contains(coordinate))
                {
                    m_pendingRuntimeLoadRequests.Remove(coordinate);
                }

                GameObject kv = null;
                if (m_terrainColliderQueue.TryGetValue(coordinate, out kv))
                {
                    m_terrainColliderQueue.Remove(coordinate);
                    DestroyImmediate(kv);
                }
            }

            for (int i = 0; i < m_closetPatches.Count; i++)
            {
                if (!m_terrainColliderQueue.ContainsKey(m_closetPatches[i]) && !m_pendingRuntimeLoadRequests.Contains(m_closetPatches[i]))
                        m_pendingRuntimeLoadRequests.Add(m_closetPatches[i]);
            }


            m_previousNodes.Clear();
            for (int i = 0; i < m_closetPatches.Count; i++)
            {
                m_previousNodes.Add(m_closetPatches[i]);
            }

            m_totalVertexCount = m_terrainColliderQueue.Count * (m_meshResolution * m_meshResolution);

        }
        
        /// <summary>
        /// Adds the coordinate into the dictionary and calls the calculation function for mesh collider
        /// </summary>
        /// <param name="coordinate"></param>
        public void GenerateCollider(int4 coordinate)
        {
            m_requestRunning = true;
            m_terrainColliderQueue.Add(coordinate, null);

            CalculateMeshDataForPatch(coordinate);
        }

        /// <summary>
        /// Unsubscribe from event callbacks when disabled
        /// </summary>
        private void OnDisable()
        {
            StormVTTerrainTextureManager.OnNewTerrainAdded -= NewTerrainTextureAdded;
            StormVTSettings.OnUpdateStormVTSettings -= StormSettingsUpdated;
        }

        /// <summary>
        /// When a new terrain texture is added (i.e. a higher quality version of the heightmap), 
        /// check if it needs to update existing colliders that were created.
        /// </summary>
        /// <param name="terrainIndex"></param>
        /// <param name="lowRes"></param>
        public void NewTerrainTextureAdded(int2 terrainIndex, bool lowRes)
        {
            //Loop through all current collider meshes, check if new terrain texture overlaps each one
            foreach(var val in m_terrainColliderQueue)
            {
                int4 coordinate = val.Key;
                if(coordinate.x == terrainIndex.x && coordinate.y == terrainIndex.y)
                {
                    CalculateMeshDataForPatch(coordinate);
                }
            }
        }

        /// <summary>
        /// Grabs the heightmap if available, and calcualtes terrain and tile position data to generate a tile
        /// </summary>
        /// <param name="coordinate"></param>
        public void CalculateMeshDataForPatch(int4 coordinate)
        {
            Texture heightmap = null;
            //Grab heightamp
            StormVTTerrainTextureData textureData = stormVTTerrainTextureManager.GetTerrainData(coordinate.xy, true);
            if (textureData != null)
            {
                heightmap = textureData.heightmap;
            }

            //Grab terrain index
            int index = stormVTTerrainGrid.GetIndex(coordinate.x, coordinate.y);

            //Get terrain's pivot
            Vector2 terrainPos = stormVTTerrainGrid.m_gridList[index].position;
            //Compute tile's pivot based off terrain's pivot
            Vector2 tilePos = new Vector2(coordinate.z * m_patchSize, coordinate.w * m_patchSize) + terrainPos;

            GenerateMesh(coordinate, heightmap, terrainPos, tilePos);
        }


        /// <summary>
        /// Sets up the necessary data for compute mesh shader
        /// </summary>
        public void SetupMeshCompute()
        {
            
            //Calculate size of each collision patch
            m_density = (float)(stormVTTerrainGrid.m_terrainSize / (float)(stormVTTerrainGrid.m_heightmapResolution - 1)) / (float)(1.0f / (int)m_collisionQuality);
            m_patchSize = m_meshResolution * m_density;
            m_numPatchesPerTerrain = (int)Mathf.Round((float)stormVTTerrainGrid.m_terrainSize / (float)m_patchSize);

            m_numPatchNeighbours = math.max((int)((m_colliderLoadingRange / m_patchSize) / 2.0f), 1);

            //Grab kernal id
            m_kernalID = generateTerrainSliceComputeShader.FindKernel("CSMain");

            //Create base mesh for vertex buffer
            m_gridMesh = GenerateMesh(m_meshResolution, m_meshResolution);
            List<Vector3> vertices = new List<Vector3>();
            m_gridMesh.GetVertices(vertices);

            //Create vertex buffer, assign to compute shader
            m_vertexBuffer = new ComputeBuffer(vertices.Count, sizeof(float) * 3, ComputeBufferType.Structured);
            m_vertexBuffer.SetData(vertices);
            generateTerrainSliceComputeShader.SetBuffer(m_kernalID, "m_vertexBuffer", m_vertexBuffer);

            m_baseVertexBuffer = new ComputeBuffer(vertices.Count, sizeof(float) * 3, ComputeBufferType.Structured);
            m_baseVertexBuffer.SetData(vertices);
            generateTerrainSliceComputeShader.SetBuffer(m_kernalID, "m_baseVertexBuffer", m_baseVertexBuffer);

            //Assign constant variables
            generateTerrainSliceComputeShader.SetFloat("heightScale", stormVTTerrainGrid.m_terrainHeightScale * 2);
            generateTerrainSliceComputeShader.SetFloat("heightOffset", stormVTTerrainGrid.m_terrainHeightOffset);
            generateTerrainSliceComputeShader.SetInt("heightmapResolution", stormVTTerrainGrid.m_heightmapResolution);
            generateTerrainSliceComputeShader.SetFloat("terrainScale", m_terrainWorldSize * 8);
            generateTerrainSliceComputeShader.SetFloat("tileScale", m_patchSize);
            generateTerrainSliceComputeShader.SetFloat("gridSize", m_meshResolution);
        }

        /// <summary>
        /// Sets data for the compute shader and dispatches the request
        /// </summary>
        /// <param name="coord">Tile cooridnate to create a mesh from</param>
        /// <param name="heightmap">Terrain heightmap to read</param>
        /// <param name="terrainPos">Position of  the terrain</param>
        /// <param name="tilePos">Position of the tile</param>
        public void GenerateMesh(int4 coord, Texture heightmap, Vector2 terrainPos, Vector2 tilePos)
        {
            if(heightmap != null)
            {
                generateTerrainSliceComputeShader.SetTexture(m_kernalID, "Heightmap", heightmap);

                generateTerrainSliceComputeShader.SetVector("terrainPivotPos", terrainPos);
                generateTerrainSliceComputeShader.SetVector("tilePivotPos", tilePos);

                generateTerrainSliceComputeShader.Dispatch(m_kernalID, 9, 9, 1);

                AsyncGPUReadback.Request(m_vertexBuffer, request =>
                {
                    CollisionRequest(request, coord, tilePos);
                });
            }
            else
            {
                m_requestRunning = false;
            }
                
        }

        /// <summary>
        /// Callback function once the compute shader has finished. 
        /// Generate the gameobject and read the vertices back into a mesh
        /// </summary>
        /// <param name="request">Async request that stores the vertices</param>
        /// <param name="coordinate">coordinate of tile we are creating</param>
        /// <param name="tilePos">The position of the tile</param>
        private void CollisionRequest(AsyncGPUReadbackRequest request, int4 coordinate, Vector2 tilePos)
        {
            if (request.hasError)
            {
                return;
            }

            GameObject collider;
            m_terrainColliderQueue.TryGetValue(coordinate, out collider);
            if(collider == null)
            {
                collider = new GameObject($"TerrainCollider_{coordinate.x}_{coordinate.y}_{coordinate.z}_{coordinate.w}");
            }

            collider.transform.parent = m_colliderParent.transform;

            NativeArray<Vector3> data = request.GetData<Vector3>();
            List<Vector3> vertices = new List<Vector3>(data);

            Mesh newColliderMesh = new Mesh();

            newColliderMesh.SetVertices(vertices);
            newColliderMesh.SetTriangles(m_gridMesh.triangles, 0);
            newColliderMesh.RecalculateBounds();

            MeshFilter meshFilter = collider.GetComponent<MeshFilter>();
            if(meshFilter == null)
                meshFilter = collider.AddComponent<MeshFilter>();
            meshFilter.mesh = newColliderMesh;

            MeshCollider meshCollider = collider.GetComponent<MeshCollider>();
            if(meshCollider == null)
                meshCollider = collider.AddComponent<MeshCollider>();
            meshCollider.sharedMesh = newColliderMesh;

  
            MeshRenderer meshRenderer = collider.GetComponent<MeshRenderer>();
            if(meshRenderer == null)
                meshRenderer = collider.AddComponent<MeshRenderer>();
            meshRenderer.sharedMaterial = m_previewMat;


            if(stormVTSettings.m_debugColliderMeshes)
            {
                meshRenderer.enabled = true;
            }
            else
            {
                meshRenderer.enabled = false;
            }

            GameObject kv;
            if (m_terrainColliderQueue.TryGetValue(coordinate, out kv))
            {
                m_terrainColliderQueue[coordinate] = collider;
            }

            collider.transform.position = new Vector3(tilePos.x, 0, tilePos.y);

            m_requestRunning = false;
        }

        #region Mesh Generation

        /// <summary>
        /// 
        /// </summary>
        /// <param name="xSize">Amount of faces across</param>
        /// <param name="ySize">Amount of faces down</param>
        /// <returns></returns>
        private Vector3[] GenerateVertices(int xSize, int ySize)
        {
            Vector3[] vertices = new Vector3[(xSize + 1) * (ySize + 1)];
            for (int i = 0, y = 0; y <= ySize; y++)
            {
                for (int x = 0; x <= xSize; x++, i++)
                {
                    vertices[i] = new Vector3(x, 0, y);
                }
            }

            return vertices;
        }

        /// <summary>
        /// Generates triangles for sizes passed in
        /// </summary>
        /// <param name="xSize">Amount of faces across</param>
        /// <param name="ySize">Amount of faces down</param>
        /// <returns></returns>
        private int[] GenerateTriangles(int xSize, int ySize)
        {
            int[] triangles = new int[xSize * ySize * 6];
            for (int ti = 0, vi = 0, y = 0; y < ySize; y++, vi++)
            {
                for (int x = 0; x < xSize; x++, ti += 6, vi++)
                {
                    // First triangle
                    triangles[ti] = vi;
                    triangles[ti + 1] = vi + xSize + 1;
                    triangles[ti + 2] = vi + xSize + 2;

                    // Second triangle
                    triangles[ti + 3] = vi;
                    triangles[ti + 4] = vi + xSize + 2;
                    triangles[ti + 5] = vi + 1;
                }
            }

            return triangles;
        }

        /// <summary>
        /// Generate s mesh based on the size across and down
        /// </summary>
        /// <param name="xSize">Amount of faces across</param>
        /// <param name="ySize">Amount of faces down</param>
        /// <returns></returns>
        private Mesh GenerateMesh(int xSize, int ySize)
        {
            Mesh mesh = new Mesh();

            Vector3[] vertices = GenerateVertices(xSize, ySize);
            mesh.vertices = vertices;
            mesh.triangles = GenerateTriangles(xSize, ySize);

            return mesh;
        }

        #endregion
    }
}

