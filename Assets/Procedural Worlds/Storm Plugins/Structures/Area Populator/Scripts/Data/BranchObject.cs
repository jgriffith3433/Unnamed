using UnityEngine;
namespace ProceduralWorlds.Storm.AreaPopulator
{
    [System.Serializable]
    public class BranchObject
    {
        public DecorationObjectInfo objectInfo;

        public bool allowNullCellsInSpawns = false;
        public bool ignoreCellsOccupiedByParent = false;
        public Vector3 relativePosition;
        public float reletiveRotation;
        public float spawnChance = 1;

    }
}
