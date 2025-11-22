using System.Collections.Generic;
using UnityEngine;
namespace ProceduralWorlds.Storm.BuildingGen
{
    public class BuildingGenData
    {

    }

    public enum WindowGenType
    {
        None,
        RoomBased,
        AngleBased,
        Random
    }



    public class RoomData
    {
        public RoomInfo info;
        public List<RoomStoreyData> storeyData = new List<RoomStoreyData>();
        public int lowestStorey;
        public GameObject roomParent;
    }

    public class RoomStoreyData
    {
        public RoomData parentRoom;
        public RoomStoreyType roomStoreyType;
        public bool expandable;
        public Vector2Int balconySize;
        public bool addSupportsToBalcony;
        public List<Vector2Int> cells = new List<Vector2Int>();
        public Dictionary<RoomData, List<(Vector2Int position, GridDirection direction)>> connections = new Dictionary<RoomData, List<(Vector2Int position, GridDirection direction)>>();
        public Dictionary<RoomData, List<BorderCell>> borderCellLists = new Dictionary<RoomData, List<BorderCell>>();
        public int storey;

        public RoomStoreyData(RoomData parentRoom, RoomStoreyType roomStoreyType, bool expandable, Vector2Int balconlySize, bool addSupportsToBalcony, int storey)
        {
            this.parentRoom = parentRoom;
            this.roomStoreyType = roomStoreyType;
            this.expandable = expandable;
            this.balconySize = balconlySize;
            this.addSupportsToBalcony = addSupportsToBalcony;
            this.storey = storey;

        }
    }

    public class CellInfo
    {
        public float fitness = 0;
        public bool forbidden;
        public Vector2 cellCenter;
        public Vector2Int cellTablePos;

        public int distanceToEdge = 100000;
        public int distanceToDesiredConnections = 100000;
        public int distanceToUndesiredConnection = 0;


        public RoomData cellRoomData;

    }

    public struct CellFitness
    { 
        public float fitness;
        public CellInfo cellInfo;

        public CellFitness(CellInfo cell)
        {
            fitness = cell.fitness;
            cellInfo = cell;
        }
    }


    public class BorderCell
    {
        public Vector2Int cellPos;
        public GridDirection gridDirection;

        public BorderCell(Vector2Int cellPos, GridDirection gridDirection)
        {
            this.cellPos = cellPos;
            this.gridDirection = gridDirection;
        }
    }

    public class RoofTileInfo
    {
        public Vector2Int mapPosition;
        public float yRotation;
        public float yPos;
        public BuildingBlock type;
        public CellRoofType cellRoofType;
        public int distanceFromEdge = 100;

        public RoofTileInfo(Vector2Int mapPosition, float yRotation, float yPos, BuildingBlock type, CellRoofType cellRoofType, int distanceFromEdge)
        {
            this.mapPosition = mapPosition;
            this.yRotation = yRotation;
            this.yPos = yPos;
            this.type = type;
            this.cellRoofType = cellRoofType;
            this.distanceFromEdge = distanceFromEdge;
        }
    }

    public enum RoomConnectionType
    {
        SingleDoor,
        SingleOpenWall,
        DoubleOpenWall,
        NoWall,
        InheritConnectionType
    }

    public enum RoomStoreyType
    {
        Full,
        Balconey,
        Terrace,
        Empty,
    }

    public enum CellRoofType
    {
        Small,
        Large,
        Blend,
        Flat,
        Unknown,
        Inconsequential,
        Spawned,
        None
    }

    public struct PredictedCellTypeMatirx
    {
        public CellRoofType centerType;
        public CellRoofType northType;
        public CellRoofType northEastType;
        public CellRoofType eastType;
        public CellRoofType southEastType;
        public CellRoofType southType;
        public CellRoofType southWestType;
        public CellRoofType westType;
        public CellRoofType northWestType;

        public PredictedCellTypeMatirx(CellRoofType centerType, CellRoofType northType, CellRoofType northEastType, CellRoofType eastType, CellRoofType southEastType
            , CellRoofType southType, CellRoofType southWestType, CellRoofType westType, CellRoofType northWestType)
        {
            this.centerType = centerType;
            this.northType = northType;
            this.northEastType = northEastType;
            this.eastType = eastType;
            this.southEastType = southEastType;
            this.southType = southType;
            this.southWestType = southWestType;
            this.westType = westType;
            this.northWestType = northWestType;
        }

    }

    public struct RoofIDMatirx
    {
        public CellRoofType northType;
        public CellRoofType northEastType;
        public CellRoofType eastType;
        public CellRoofType southEastType;
        public CellRoofType southType;
        public CellRoofType southWestType;
        public CellRoofType westType;
        public CellRoofType northWestType;

        public RoofIDMatirx(CellRoofType northWestType, CellRoofType northType, CellRoofType northEastType, CellRoofType westType,
            CellRoofType eastType, CellRoofType southWestType, CellRoofType southType, CellRoofType southEastType
            )
        {
            this.northType = northType;
            this.northEastType = northEastType;
            this.eastType = eastType;
            this.southEastType = southEastType;
            this.southType = southType;
            this.southWestType = southWestType;
            this.westType = westType;
            this.northWestType = northWestType;
        }

    }

    public enum WinnerListID
    {
        Original,
        Left,
        BottomLeft,
        Bottom
    }



    public enum GridDirection
    {
        Up,
        Down,
        Left,
        Right,
    }

    public enum ExpansionDirection
    {
        RightAndForward,
        LeftAndBackward
    }

    public class CellInstantiationData
    {
        public bool hasCeiling = false;
        public bool hasFloor = false;
        public bool fakeFilling = false;
    }

    public class WallInstatiationData
    {
        public bool spawnedWallDown;
        public bool spawnedWallLeft;
    }
}
