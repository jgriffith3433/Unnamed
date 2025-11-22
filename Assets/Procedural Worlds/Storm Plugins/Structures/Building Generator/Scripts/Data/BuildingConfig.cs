using System.Collections.Generic;
using UnityEngine;

namespace ProceduralWorlds.Storm.BuildingGen
{
    [CreateAssetMenu(fileName = "New Building Data", menuName = "Procedural Worlds/Storm/Building Generator/Building Data")]
    public class BuildingConfig : ScriptableObject
    {
        /// <summary>
        /// the yOffset of the spawn
        /// </summary>
        [HideInInspector]
        public float yOffset;

        public string styleTag;

        public WindowGenType windowGenType = WindowGenType.None;

        public float windowAngleIncrement = 0;

        public float chanceOfWindow = 0;

        public bool generateInteriorLayout = true;

        /// <summary>
        /// list of rooms to include when spawning in some modes
        /// </summary>
        public List<RoomInfo> roomsToInclude = new List<RoomInfo>();


        public int floors;
        
        /// <summary>
        /// list of information for each level, array position represents level number
        /// </summary>
        public List<BuildingLevelInfo> levelInfomation = new List<BuildingLevelInfo>();

        /// <summary>
        /// Grid Information
        /// </summary>
        public float XZGridSize = 1;
        public float YGridSize = 1;
        public float RoofYGridSize = 1;

        public int selectedRoomInt = 0;
        /// <summary>
        /// room set for when a polygon needs to be filled
        /// </summary>
        public List<RoomInfo> rooms = new List<RoomInfo>();


        [HideInInspector]
        public int extimatedTotalArea;
        /// <summary>
        /// dictionary of ploygons, key reperesents level
        /// </summary>
        public Dictionary<int, List<Vector2>> polygonLists = new Dictionary<int, List<Vector2>>();

        /// <summary>
        /// is the roof flat(like a skyscraper) or not
        /// </summary>
        public RoofType roofType = RoofType.Flat;

        /// <summary>
        /// weather or not to check for the roof peak join edge cases and use that building block
        /// </summary>
        public bool useRoofPeakJoinEdgeCaseFix = false;

        public bool createRoofTrim = false;

        /// <summary>
        /// should an alternate wall be used for the first layer
        /// </summary>
        public bool useAlternateWallFirstLayer = false;

        /// <summary>
        /// should a front step for all external doors be created
        /// </summary>
        public bool createFrontStep = false;

        public bool createWallTrim = false;

        public Vector3 spawnPos = Vector3.zero;

        public BuildingBlocks buildingBlocks;

        #region Inspector Variables
        public bool generalSettingsExpanded = false;
        public bool roomSettingsExpanded = false;
        public bool wallSettingsExpanded = false;
        public bool roofSettingsExpanded = false;
        #endregion
    }

    public enum RoofType
    {
        Flat,
        Peaked,
        PartialPeaked
    }
}
