using System.Collections.Generic;
using UnityEngine;
using ProceduralWorlds.Storm.StructureGenCore;

using ProceduralWorlds.Storm.BuildingGen;
namespace ProceduralWorlds.Storm.DungeonGen
{
    [CreateAssetMenu(fileName = "Dungeon Layer", menuName = "Procedural Worlds/Storm/Dungeon Generator/DungeonLayer")]
    public class DungeonLayer : ScriptableObject
    {
        public int levelSize = 90;
        public int maxBounds;
        public int levels;
        public List<DungeonLevel> layerLevels = new List<DungeonLevel>();
        public List<CustomChamber> customChambers = new List<CustomChamber>();
        //if for a level there is no entry or the list has less than 3 nodes it will revert to creating an expanse around the grid based on the default bounds
        //will need a visualizer for this later
        public bool mirrorRooms = false;

        public bool useAstarHalls = true;
        public ChamberFormationProfile defaultChamberFormationProfile;
        public BuildingConfig defaultChamberTheme;


        public Vector2 startPoint;

        public LayerMask collisionCheckLayer;
        //must be an odd number
        public int checkResolution;



        public int chamberCount;

        public int layerConnectionCount;

        [HideInInspector]
        public float gridSize;
        public void CreateLevelsAndAssignGrid(float gridSize, float verticalGridSize, Vector3 startPoint)
        {
            layerLevels.Clear();
            //determine the actual start point(if start level is not 1 then it will be upwards
            Vector3 level1StartPoint = new Vector3(startPoint.x, startPoint.y - verticalGridSize, startPoint.z);

            //find the x-z bottom left start point of the default grid gen
            float x = level1StartPoint.x - maxBounds / 2;
            float y = level1StartPoint.z - maxBounds / 2;
            //x = Mathf.RoundToInt(x / gridSize) * gridSize;
            //y = Mathf.RoundToInt(y / gridSize) * gridSize;
            Vector2 defaultStartPoint = new Vector2(x, y);

            for (int i = 0; i < levels; i++)
            {
                
                DungeonLevel level = new DungeonLevel();
                //THERE IS A BUG WITH THE LEVEL NUMBER THAT MEANS WHEN USING MORE THAN 1 LAYER IT WILL NOT GENERATE THE CORRECT LEVEL NUMBER
                level.levelNumber = i;
                level.yLevel = startPoint.y - ((i + 1) * verticalGridSize);
                level.useAStarHalls = useAstarHalls;
                //Debug.Log(level.yLevel);
                level.bounds = DungeonGeneratorUtils.GetLevelBounds(this, level, startPoint);
                //Debug.Log(level.bounds.extents);
                level.gridCellMapData = DungeonGeneratorUtils.GenerateGrid(gridSize, defaultStartPoint, level.bounds, ref level.availibleCells);
                layerLevels.Add(level);
                continue;
            }
        }

        public void CreateRooms(ref DungeonLayer dungeonLayer, ref int chamberCountNumber, XorshiftPlus randomGen, Transform parent = null)
        {
            for (int i = 0; i < customChambers.Count; i++)
            {
                DungeonGeneratorUtils.CreateNewChamber(ref dungeonLayer, customChambers[i].startingLevel, chamberCountNumber, randomGen, false, customChambers[i], parent);
                chamberCountNumber++;
            }

            for (int k = 0; k < layerLevels.Count; k++)
            {
                DungeonLevel dungeonLevel = layerLevels[k];
                //this bool makes it so that the first room generated is a multi level room
                dungeonLevel.CreateRoomsOnLevel(ref dungeonLayer, ref chamberCountNumber, randomGen, parent, mirrorRooms);
            }


        }
        public void CreateHalls(XorshiftPlus randomGen)
        {
            for (int i = 0; i < layerLevels.Count; i++)
            {
                DungeonLevel dungeonLevel = layerLevels[i];

                dungeonLevel.GetChamberConnections(0.5f, randomGen);
                dungeonLevel.GenerateHallwaysBetweenRooms(dungeonLevel.chamberConnections);
            }
        }

        public void SetLayerChamberExits(XorshiftPlus randomGen)
        {
            for (int k = 0; k < layerLevels.Count; k++)
            {
                DungeonLevel dungeonLevel = layerLevels[k];
                //this bool makes it so that the first room generated is a multi level room
                for (int i = 0; i < dungeonLevel.roomsOnLevel.Count; i++)
                {
                    DungeonChamberStory chamberStory = dungeonLevel.roomsOnLevel[i];
                    if (chamberStory.parentChamber.custom)
                    {
                        DungeonGeneratorUtils.SetChamberExits(ref chamberStory, this, dungeonLevel, randomGen);
                    }
                    //Debug.Log("setting chamber story exits");

                }
            }
        }

    }
}

