using System.Collections.Generic;
using UnityEngine;
using ProceduralWorlds.Storm.BuildingGen;
namespace ProceduralWorlds.Storm.DungeonGen
{
    public class DungeonChamberStory
    {
        public Vector2Int bottomLeft;
        public Vector2Int topRight;
        public List<Vector2Int> storyCells = new List<Vector2Int>();
        public int level;
        public DungeonChamber parentChamber;
        //change this bool before rooms are generated and it wont be connected to the hall network
        public bool connectToNetwork = true;


        public Vector2Int chamberCenter;

        public List<BorderCell> edgeCells = new List<BorderCell>();
        public List<BorderCell> borderCells = new List<BorderCell>();
        public List<BorderCell> exitCellsGlobal = new List<BorderCell>();
        public List<BorderCell> exitCellsLocal = new List<BorderCell>();

        public ChamberConnectionType chamberConnectionType;
        public DefinedEnteranceType definedEnteranceType;

        public void AddExitCell(BorderCell borderCell)
        {
            exitCellsGlobal.Add(borderCell);
            exitCellsLocal.Add(new BorderCell(borderCell.cellPos-bottomLeft, borderCell.gridDirection));
        }
    }
}
