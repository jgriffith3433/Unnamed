using UnityEngine;
namespace ProceduralWorlds.Storm.DungeonGen
{
    [CreateAssetMenu(fileName = "ChamberFormationProfile", menuName = "Procedural Worlds/Storm/Dungeon Generator/Chamber Formation Profile")]
    public class ChamberFormationProfile : ScriptableObject
    {
        public Vector2Int shapeIterationRange;

        public Vector2 circleRadisRange;

        public Vector2 rectangleLengthRange;
        public Vector2 rectangleWidthRange;

        public Vector2Int chamberLevelRange;
        [Range(0, 1)]
        public float rectangleChance;
    }
}

