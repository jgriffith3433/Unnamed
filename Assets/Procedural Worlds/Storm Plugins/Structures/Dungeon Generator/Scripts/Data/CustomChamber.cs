using System.Collections.Generic;
using UnityEngine;
using ProceduralWorlds.Storm.BuildingGen;

namespace ProceduralWorlds.Storm.DungeonGen
{
    [CreateAssetMenu(fileName = "Custom Chamber", menuName = "Procedural Worlds/Storm/Dungeon Generator/Custom Chamber")]
    public class CustomChamber : ScriptableObject
    {
        public bool procedural;
        public ChamberFormationProfile chamberFormationProfile;
        public BuildingConfig chamberTheme;

        public Vector2Int origin;
        public Vector2Int bounds;
        public List<List<Vector2Int>> entranceTiles;
        public int levelHeight;
        public int startingLevel;

        public GameObject roomPrefab;

        public List<List<Vector2Int>> roomFloorTiles;
        public ChamberConnectionType chamberConnectionType;
        public DefinedEnteranceType definedEnteranceType;
        public List<bool> allowedFloors = new List<bool>();
        public bool hallsAvoidChamber = false;
    }
}
