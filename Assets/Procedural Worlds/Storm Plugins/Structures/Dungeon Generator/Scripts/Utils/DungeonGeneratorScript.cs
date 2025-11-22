using System;
using System.Collections.Generic;
using UnityEngine;
using ProceduralWorlds.Storm.StructureGenCore;
using ProceduralWorlds.Storm.BuildingGen;
using ProceduralWorlds.Storm.AreaPopulator;
#if PW_STORM_HDRP
using UnityEngine.Rendering;
#endif


namespace ProceduralWorlds.Storm.DungeonGen
{
    public class DungeonGeneratorScript : MonoBehaviour
    {
        public int spawns;
        public int seed;
        [SerializeField]
        public List<DungeonLayer> dungoenLayers = new List<DungeonLayer>();
        public int selectedDungeonLayerInt = 0;
        public BuildingConfig hallwayBuildingInfo;
        public float gridSize;
        public float verticalGridSize;
        public Vector3 startPoint;

        public bool useStorm = true;

        [Header("Gizmos")]
        public bool showGizmos = false;
        public int viewingLevel;
        public int layerViewingLevel;
        public List<Vector2Int> resultList = new List<Vector2Int>();
        public Vector2Int initalstart;
        public Vector2Int rect;
        public int raduis;

        [Header("Interior Lighting")]
        public GameObject volume;
        //private int chamberCount = 0;

        private XorshiftPlus randomGen;

        public DecorationData hallwayDecorationData;

        public BuildingGenerator houseBuilder;
        private ObjectInstantiationManager objectInstantiationManager;
        //private bool useStorm = false;


        private List<GameObject> spawnedObjects = new List<GameObject>();

        private Action<GameObject> finished;

        public GeneratedBuildingParent buildingParentPrefab;

#if PW_STORM_HDRP
        public VolumeProfile volumeProfile;
#endif
        public int terrainHoleSize;
        public Vector3 terrainHolePosition;

        public GameObject entranceRoomPrefab;
        public Vector3 entranceRoomPos;

        /*
        private bool EvaluateDiagonalPotential(Vector2Int currentCell, Vector2Int lastCell, Vector2Int nextCell, out float rotation)
        {
            rotation = 0;
            Vector2Int lastCellDelta = lastCell - currentCell;
            Vector2Int nextCellDelta = nextCell - currentCell;
            if(lastCell.x != nextCell.x && lastCell.y != nextCell.y)
            {
                if((lastCell.y < currentCell.y && nextCell.x > currentCell.x) || (lastCell.x < currentCell.x && nextCell.y > currentCell.y))
                {
                    rotation = 135;
                    
                    return true;
                }
                else if((nextCell.y < currentCell.y && lastCell.x > currentCell.x) || (nextCell.x < currentCell.x && lastCell.y > currentCell.y))
                {
                    rotation = 135;
                    
                    return true;
                }
                else
                {
                    rotation = 45;
                    
                    return true;
                }
                
                
            }
            
            return false;
        }
        */


        public void OnExitDungeon()
        {
            gameObject.SetActive(false);
        }

        public void OnEnterDungeon()
        {
            gameObject.SetActive(true);
        }

        private void OnDrawGizmosSelected()
        {
            if (showGizmos == false)
            {
                return;
            }
            DungeonLayer dungeonLayer = dungoenLayers[layerViewingLevel];
            if(viewingLevel < 0 || viewingLevel >= dungeonLayer.layerLevels.Count)
            {
                return;
            }
            DungeonLevel dungoenViewingLevel = dungeonLayer.layerLevels[viewingLevel];

            GridCell[,] gridCellMap = dungoenViewingLevel.gridCellMapData.gridCellMap;
            Vector3 origin = dungoenViewingLevel.gridCellMapData.origin;


            for (int i = 0; i < gridCellMap.GetLength(0); i++)
            {
                for (int k = 0; k < gridCellMap.GetLength(1); k++)
                {
                    //this position is not currentl accurate
                    GridCell cell = gridCellMap[i, k];
                    Vector2 cellPosition = cell.position;
                    if (cell.cellStatus == CellStatus.Room)
                    {
                        Gizmos.color = Color.blue;
                    }
                    else
                    {
                        Gizmos.color = new Color(1, 1, 1, 0.5f);
                        //continue;
                    }
                    if(cell.parentStory != null)
                    {
                        if (cell.parentStory.parentChamber.hallsAvoidChamber)
                        {
                            Gizmos.color = Color.yellow;
                        }

                    }
                    float yLevel = (dungoenViewingLevel.yLevel + verticalGridSize / 2);
                    
                    Gizmos.DrawWireCube(new Vector3(cellPosition.x, yLevel, cellPosition.y), new Vector3(gridSize, verticalGridSize, gridSize));
                }
            }

            for (int i = 0; i < dungoenViewingLevel.hallways.Count; i++)
            {
                Hallway hallway = dungoenViewingLevel.hallways[i];
                float colorStrength = 0.1f * i;
                Color hallwayColor = new Color(colorStrength, colorStrength, colorStrength, 1);
                if (hallway == null)
                {
                    continue;
                }
                float yLevel = (dungoenViewingLevel.yLevel + verticalGridSize / 2);
                foreach (Vector2Int position in hallway.hallwayCells)
                {
                    GridCell cell = gridCellMap[position.x, position.y];
                    Vector2 cellPosition = cell.position;
                    Gizmos.color = hallwayColor;
                    Gizmos.DrawWireCube(new Vector3(cellPosition.x, yLevel, cellPosition.y), new Vector3(gridSize, verticalGridSize, gridSize));
                }

                foreach (BorderCell exitBorderCell in hallway.exitCells)
                {
                    Vector2Int exitCellGridPos = exitBorderCell.cellPos;
                    GridCell exitCell = gridCellMap[exitCellGridPos.x, exitCellGridPos.y];
                    Vector2 exitCellPos = exitCell.position;
                    Gizmos.color = Color.magenta;
                    Gizmos.DrawWireCube(new Vector3(exitCellPos.x, yLevel, exitCellPos.y), new Vector3(gridSize, verticalGridSize, gridSize));
                }

            }
            Gizmos.color = Color.blue;
            for (int i = 0; i < resultList.Count; i++)
            {
                GridCell cell = gridCellMap[resultList[i].x, resultList[i].y];
                Vector2 cellPosition = cell.position;
                Gizmos.DrawWireCube(new Vector3(cellPosition.x, dungoenViewingLevel.yLevel - viewingLevel * verticalGridSize, cellPosition.y), new Vector3(gridSize, verticalGridSize, gridSize));
            }


        }

        private void OnDestroy()
        {
            /*
            if (m_lightingManager != null)
            {
                ProceduralWorlds.Arketype.API.World.OnSwitchToInsideRendering -= m_lightingManager.Refresh;
            }

            ProceduralWorlds.Arketype.API.World.OnSwitchToInsideRendering -= OnEnterDungeon;
            ProceduralWorlds.Arketype.API.World.OnSwitchToOutsideRendering -= OnExitDungeon;
            */
        }
    }
}
