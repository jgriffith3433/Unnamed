using System.Collections.Generic;
using UnityEngine;

namespace ProceduralWorlds.Storm.BuildingGen
{
    public class BuildingLevelInfo
    {
        /// <summary>
        /// the level number
        /// </summary>
        public int levelNumber = 0;


        /// <summary>
        /// Information about the polygon for this level
        /// </summary>
        public List<Vector2> polygonVertecies = new List<Vector2>();
        public int xSteps;
        public int zSteps;
        public Vector2 lowestPointOfPolygon;
        public Vector2 highestPointOfPolygon;

        /// <summary>
        /// list of the tiles that are reserved for staircase
        /// </summary>
        public List<Vector2Int> stairCaseTiles = new List<Vector2Int>();
       
        /// <summary>
        /// list of the rooms in the level
        /// </summary>
        public List<RoomData> rooms = new List<RoomData>();

        public List<RoomStoreyData> roomStoreyDatas = new List<RoomStoreyData>();

        public Dictionary<RoomData, List<CellFitness>> cellFitnessSnapshots;


        public List<BorderCell> exitCells;

        public RoomData nullRoom;

        public int totalRoomSpace = 0;
        /// <summary>
        /// All Cell information for this level
        /// </summary>
        public List<Vector2> gridCenters = new List<Vector2>();
        public CellInfo[,] gridCellTable;


        /// <summary>
        /// gridsize of the building
        /// </summary>
        public float XZGridSize;

        /// <summary>
        /// list of all public and private cells
        /// </summary>
        public List<Vector2Int> publicCells = new List<Vector2Int>();
        public List<Vector2Int> privateCells = new List<Vector2Int>();

        /// <summary>
        /// bool weather or not islands should be connected
        /// </summary>
        public bool connectIslands;
        /// <summary>
        /// how many exits should be added to the building(maxes out when there is an exit off every public room)
        /// </summary>
        public int exits = 0;

        public GameObject levelParent = null;
    }
}

