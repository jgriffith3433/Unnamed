using System.Collections.Generic;
using UnityEngine;
using ProceduralWorlds.Storm.AreaPopulator;

namespace ProceduralWorlds.Storm.BuildingGen
{

    [CreateAssetMenu(fileName = "New Room", menuName = "Procedural Worlds/Storm/Building Generator/Room")]
    public class RoomInfo : ScriptableObject
    {
        public bool expand = true;
        public bool persistLocationThroughFloors = false;
        public bool stairCase = false;
        public bool forceDesiredConnections = false;
        public bool hallway = false;
        public bool capRoomCount = false;
        public int maxRoomCount = 1;

        public RoomConnectionType connectionType;

        public bool createWindows = true;

        public bool decorateRoom = false;
        public DecorationData decorationData;

        public Vector2Int desiredRoomSize = new Vector2Int(1, 1);
        public int firstFloorDesiredTiles;
        public int extraFloorsDesiredTiles;

        public bool useExitCellWeighting = false;
        public float exitCellWeight;
        public float edgeFitnessModifier = 1;
        public float connectivityFitnessModifier = 1;
        public float avoidFitnessModifier = 1;

        public bool useDesiredStartFloor = false;
        public int desiredStartFloor;
        public bool allowShifting = false;
        public int desiredFloors = 1;
        public float priority;

        public List<RoomInfo> desiredConnections = new List<RoomInfo>();
        public List<RoomInfo> undesiredConnections = new List<RoomInfo>();
        public int maxConnections = 3;

        

        [SerializeField]
        public List<RoomStoreyBehaviour> roomStoreyBehaviors = new List<RoomStoreyBehaviour>();

        public Color roomVisualizationColor = Color.blue;
        private void OnValidate()
        {
            SetFirstFloorDesiredTiles();
            //put this in recently so editor is accurate, may need to remove later
            GetExtraFloorTiles(desiredFloors);
        }
        public void SetFirstFloorDesiredTiles()
        {
            firstFloorDesiredTiles = desiredRoomSize.x * desiredRoomSize.y;
        }
        public int GetExtraFloorTiles(int floors)
        {
            SetFirstFloorDesiredTiles();
            if(desiredFloors == 0)
            {
                extraFloorsDesiredTiles = firstFloorDesiredTiles * (floors - 1);
            }
            else
            {
                int actualSpawnFloors = Mathf.Min(desiredFloors, floors);
                extraFloorsDesiredTiles = firstFloorDesiredTiles * (actualSpawnFloors - 1);
            }

            return extraFloorsDesiredTiles;
        }
    }
    


    [System.Serializable]
    public class RoomStoreyBehaviour
    {
        public bool allowExpansion = true;
        public RoomStoreyType roomStoreyType = RoomStoreyType.Full;
        public Vector2Int balconySize;
        public bool addSupportsToBalcony = false;
    }
}

