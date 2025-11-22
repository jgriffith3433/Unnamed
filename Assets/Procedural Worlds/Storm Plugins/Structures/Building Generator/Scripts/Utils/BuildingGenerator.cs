using System.Collections.Generic;
using UnityEngine;
using ProceduralWorlds.Storm.StructureGenCore;
using ProceduralWorlds.Storm.Core;
using XorshiftPlus = ProceduralWorlds.Storm.StructureGenCore.XorshiftPlus;
using ProceduralWorlds.Storm.UI;

namespace ProceduralWorlds.Storm.BuildingGen
{
    [RequireComponent(typeof(PolygonMask))]
    [RequireComponent(typeof(ObjectInstantiationManager))]
    [ExecuteInEditMode]
    /// <summary>
    /// Currently some very large buildings wont work becuase of complications with the hallways, also buildings of very odd shape would not work either but its not fatal anymore, i protect against a stack overflow by preventing resets going over 30
    /// </summary>
    public class BuildingGenerator : MonoBehaviour
    {
        #region Generation Settings
        // Instance for storing house information
        public BuildingConfig buildingConfig;

        // Prefab for instantiating the building parent GameObject
        public GeneratedBuildingParentWrapper buildingParentPrefab;

        // Seed for random number generation
        public int m_seed;
        
        public bool decorateInteriors = false;

        public bool flattenTerrain = false;

        public bool useEntranceDirection = false;
        public float entranceDirectionAngle = 0;


        #endregion

        #region Storm Settings
        public bool useStorm = false;
        [SerializeField]
        public List<StormPolygonLayerSettings> polygonLayers = new List<StormPolygonLayerSettings>();

        public bool convertObjectsToStormOnApply = true;
        #endregion

        #region Public Variables
        [HideInInspector]
        public List<Vector2> accuratePolygon = new List<Vector2>();

        // Parent GameObject for the generated building
        public GeneratedBuildingParent buildingParent;
        #endregion

        #region Private Variables



        // Grid size for building generation
        private float m_gridSize;
        // Rotation amount applied to the building
        public float rotationAmount;
        // Current centroid of the building
        public Vector3 currentCentrioid;

        // Start time for tracking building generation performance
        private float timeStart;
        // Time taken for building generation
        private float timeTaken;


        #endregion

        #region Component References
        [Header("Component References")]
        // Instance of the random number generator
        private XorshiftPlus randomGen;

        [HideInInspector]
        public ObjectInstantiationManager objectInstantiationManager;

        public StormPolygonMask polygonObject;
        #endregion

        #region Debug Settings
        // Toggle for displaying debug information
        public bool showVisualization = false;

        public BuildingvisualizationType visualizationType;

        public int fitnessDebugIndex;
        public Color roomLabelColor = new Color(1, 0.8f, 0.4f, 1);

        // RoomInfo to inspect the weighting of
        public RoomInfo currentDebugRoom;

        //Building Level of debugging information to display
        public int debugLevel;
        #endregion

        #region Inspector Varaibles
        public bool generationSettingsExpanded = false;
        public bool polygonPanelExpanded = true;
        public bool visualizationSettingsExpanded = false;
        public bool buildingConfigExpanded = false;
        public bool stormSettingsExpanded = false;
        #endregion

        public void Start()
        {
            SnapToTerrain();
        }

        public void SnapToTerrain()
        {
            float yLevel = StructureCreatorUtils.GetHeightAtPointVec2(new Vector2(transform.position.x, transform.position.z));
            transform.position = new Vector3(transform.position.x, yLevel, transform.position.z);
        }

        
        /// <summary>
        /// draws gizmos for visualization
        /// </summary>
        private void OnDrawGizmosSelected()
        {
            if (showVisualization == false || buildingConfig == null || buildingConfig.levelInfomation == null || buildingConfig.levelInfomation.Count == 0)
            {
                return;
            }

            m_gridSize = buildingConfig.XZGridSize;
            float yOffset = buildingConfig.yOffset + debugLevel * buildingConfig.YGridSize;

            BuildingLevelInfo structureLevelInfo = buildingConfig.levelInfomation[debugLevel];
            if (structureLevelInfo == null) return;

            CellInfo[,] cellTable = structureLevelInfo.gridCellTable;

            // Define the rotation pivot point and rotation speed

            Quaternion rotation = Quaternion.Euler(0, rotationAmount, 0);

            RoomData fitnessRoomData = null;
            if (fitnessDebugIndex >= 0 && fitnessDebugIndex < structureLevelInfo.rooms.Count)
            {
                fitnessRoomData = structureLevelInfo.rooms[fitnessDebugIndex];
                currentDebugRoom = fitnessRoomData.info;
            }

            if (visualizationType == BuildingvisualizationType.Fitness && fitnessRoomData != null && structureLevelInfo.cellFitnessSnapshots.ContainsKey(fitnessRoomData))
            {
                List<CellFitness> cellFitnesses = structureLevelInfo.cellFitnessSnapshots[fitnessRoomData];
                float highestFitness = 0;
                for (int i = 0; i < cellFitnesses.Count; i++)
                {
                    if (cellFitnesses[i].fitness > highestFitness)
                    {
                        highestFitness = cellFitnesses[i].fitness;
                    }
                }

                for (int i = 0; i < cellFitnesses.Count; i++)
                {
                    CellFitness cellFitness = cellFitnesses[i];
                    CellInfo currentCell = cellFitness.cellInfo;
                    if (currentCell.forbidden)
                    {
                        continue;
                    }

                    Vector3 cellCenter = new Vector3(currentCell.cellCenter.x, yOffset, currentCell.cellCenter.y);

                    // Calculate rotated position around the pivot point
                    Vector3 direction2 = cellCenter - currentCentrioid;
                    Vector3 rotatedPosition = currentCentrioid + rotation * direction2;

                    // Set the transformation matrix for the gizmo
                    Gizmos.matrix = Matrix4x4.TRS(rotatedPosition, rotation, new Vector3(m_gridSize, m_gridSize, m_gridSize));
                    float normalizedFitness = cellFitness.fitness / highestFitness;

                    float redness = 1 - normalizedFitness;

                    Gizmos.color = new Color(redness, normalizedFitness, 0);
                    Gizmos.DrawCube(Vector3.zero, Vector3.one);
                }
            }
            else if (visualizationType == BuildingvisualizationType.Layout)
            {
                Dictionary<RoomInfo, List<RoomData>> roomAppearanceCount = new Dictionary<RoomInfo, List<RoomData>>();
                Dictionary<RoomInfo, int> roomTotals = new Dictionary<RoomInfo, int>();
                foreach (RoomData roomData in structureLevelInfo.rooms)
                {
                    if(roomTotals.ContainsKey(roomData.info))
                    {
                        roomTotals[roomData.info]++;
                    }
                    else
                    {
                        roomTotals.Add(roomData.info, 1);
                        roomAppearanceCount.Add(roomData.info, new List<RoomData>());
                    }
                }
                for (int i = 0; i < cellTable.GetLength(0); i++)
                {
                    for (int l = 0; l < cellTable.GetLength(1); l++)
                    {
                        CellInfo cellInfo = cellTable[i, l];
                        Vector3 cellCenter = new Vector3(cellInfo.cellCenter.x, yOffset, cellInfo.cellCenter.y);

                        // Calculate rotated position around the pivot point
                        Vector3 direction2 = cellCenter - currentCentrioid;
                        Vector3 rotatedPosition = currentCentrioid + rotation * direction2;

                        // Set the transformation matrix for the gizmo
                        Gizmos.matrix = Matrix4x4.TRS(rotatedPosition, rotation, new Vector3(m_gridSize, m_gridSize, m_gridSize));

                        if (cellInfo.forbidden)
                        {
                            //Gizmos.color = Color.black;
                            //Gizmos.DrawWireSphere(Vector3.zero, 0.1f);
                            //Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
                        }
                        else if (cellInfo.cellRoomData != null)
                        {
                            RoomData roomData = cellInfo.cellRoomData;
                            RoomInfo roomInfo = roomData.info;
                            
                            if (!roomAppearanceCount[roomInfo].Contains(roomData))
                            {
                                roomAppearanceCount[roomInfo].Add(roomData);
                            }
                            Gizmos.color = GetGizmoColor(cellInfo.cellRoomData, roomAppearanceCount[roomInfo].IndexOf(roomData), roomTotals);
                            //Gizmos.DrawWireSphere(Vector3.zero, 0.1f);
                            Gizmos.DrawCube(Vector3.zero, Vector3.one);
                        }
                        else
                        {
                            Gizmos.color = Color.magenta;
                            //Gizmos.DrawWireSphere(Vector3.zero, 0.1f);
                            Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
                        }
                        // Reset Gizmos matrix after each draw to avoid unintended transformations
                        Gizmos.matrix = Matrix4x4.identity;
                    }
                }
            }

            if (useEntranceDirection)
            {
                // Draw an arrow from the object's position in the direction of entranceDirectionAngle
                Vector3 startPosition = currentCentrioid;
                float arrowLength = 20.0f; // Length of the arrow
                float arrowHeadLength = 3f; // Length of the arrowhead
                float arrowHeadAngle = 20.0f; // Angle of the arrowhead

                // Calculate the direction vector
                Vector3 direction = Quaternion.Euler(0, -entranceDirectionAngle+90, 0) * Vector3.forward;
                Vector3 endPosition = startPosition + direction * arrowLength;

                // Draw the arrow line
                Gizmos.color = Color.blue;
                Gizmos.DrawLine(startPosition, endPosition);

                // Draw the arrowhead
                Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 + arrowHeadAngle, 0) * Vector3.forward;
                Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 - arrowHeadAngle, 0) * Vector3.forward;
                Gizmos.DrawRay(endPosition, right * arrowHeadLength);
                Gizmos.DrawRay(endPosition, left * arrowHeadLength);
            }
        }

        private Color GetGizmoColor(RoomData roomData, int roomIndex, Dictionary<RoomInfo, int> roomTotals)
        {
            float totalRooms = roomTotals[roomData.info];
            totalRooms += 2;
            float lerpValue = roomIndex / totalRooms;

            return Color.Lerp(roomData.info.roomVisualizationColor, Color.black, lerpValue);
        }
    }

    public enum BuildingvisualizationType
    {
        Layout,
        Fitness,
    }
}