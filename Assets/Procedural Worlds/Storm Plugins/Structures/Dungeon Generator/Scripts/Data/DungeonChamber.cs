using System.Collections.Generic;
using UnityEngine;

using ProceduralWorlds.Storm.BuildingGen;
namespace ProceduralWorlds.Storm.DungeonGen
{
    public class DungeonChamber
    {
        public List<DungeonChamberStory> chamberStories = new List<DungeonChamberStory>();
        public MultiLevelChamberArchType chamberArchType;
        public MultiLevelChamberType chamberType;
        public int chamberID;
        public bool custom = false;
        public BuildingConfig chamberTheme;

        public ChamberConnectionType chamberConnectionType;
        public DefinedEnteranceType definedEnteranceType;
        public List<bool> allowedFloors = new List<bool>();
        public bool hallsAvoidChamber = false;
        public bool hallsAddExtraCell = false;

        public Vector3 spawnPoint = Vector3.zero;
        public GameObject roomPrefab;


        public Vector2Int lowestPoint;
        public Vector2Int highestPoint;
        public Vector2Int bounds;
    }

    public enum ChamberConnectionType
    {
        Open,
        None,
        SingleDoor,
        DefinedEntrance,
    }

    public enum DefinedEnteranceType
    {
        None,
        XAxisCenterPlus,
        XAxisCenterMinus,
        ZAxisCenterPlus,
        ZAxisCenterMinus,
        All,


    }

    public enum MultiLevelChamberArchType
    {
        //floors which go outside the original levels bounds but still have a point where they connection
        DynanamicFloor,
        //floors which stay within the original levels bounds
        ConstrainedFloor,
    }

    public enum MultiLevelChamberType
    {
        FullLevel,
        ParitalLevel,
    }
}
