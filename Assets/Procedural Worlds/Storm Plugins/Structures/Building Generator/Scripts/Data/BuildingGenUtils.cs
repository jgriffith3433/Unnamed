using System.Collections.Generic;
using UnityEngine;
namespace ProceduralWorlds.Storm.BuildingGen
{
    public static class BuildingGenUtils
    {
        public static RoomStoreyBehaviour GetRoomStoreyBehaviour(RoomData roomData, int storey)
        {
            if (roomData.info.roomStoreyBehaviors.Count >= storey + 1)
            {
                return roomData.info.roomStoreyBehaviors[storey];
            }
            else
            {
                return new RoomStoreyBehaviour();
            }

        }

        public static bool DoesCellTableContainCell(CellInfo[,] cellTable, Vector2Int point)
        {
            int xLength = cellTable.GetLength(0);
            int yLength = cellTable.GetLength(1);
            if (point.x < 0 || point.y < 0 || point.x >= xLength || point.y >= yLength)
            {
                return false;
            }
            return true;
        }
        public static bool IsCellValid(CellInfo[,] cellTable, Vector2Int point)
        {
            int xLength = cellTable.GetLength(0);
            int yLength = cellTable.GetLength(1);
            if (point.x < 0 || point.y < 0 || point.x >= xLength || point.y >= yLength)
            {
                return false;
            }
            CellInfo pointCell = cellTable[point.x, point.y];
            if (pointCell.forbidden)
            {
                return false;
            }
            return true;
        }

        public static bool IsCellDesired(CellInfo[,] cellTable, Vector2Int point, List<RoomInfo> desiredConnections)
        {
            int xLength = cellTable.GetLength(0);
            int yLength = cellTable.GetLength(1);
            if (point.x < 0 || point.y < 0 || point.x >= xLength || point.y >= yLength)
            {
                return false;
            }
            CellInfo pointCell = cellTable[point.x, point.y];
            if (pointCell.forbidden || pointCell.cellRoomData == null)
            {
                return false;
            }
            if (desiredConnections.Contains(pointCell.cellRoomData.info))
            {
                return true;
            }
            return false;
        }

        public static bool IsCellUndesired(CellInfo[,] cellTable, Vector2Int point, List<RoomInfo> undesiredConnections)
        {
            int xLength = cellTable.GetLength(0);
            int yLength = cellTable.GetLength(1);
            if (point.x < 0 || point.y < 0 || point.x >= xLength || point.y >= yLength)
            {
                return false;
            }
            CellInfo pointCell = cellTable[point.x, point.y];
            if (undesiredConnections.Contains(pointCell.cellRoomData.info))
            {
                return true;
            }
            return false;

        }


    }


}
