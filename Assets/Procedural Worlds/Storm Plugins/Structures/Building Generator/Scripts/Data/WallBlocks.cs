using UnityEngine;

namespace ProceduralWorlds.Storm.BuildingGen
{
    [CreateAssetMenu(fileName = "Wall Blocks", menuName = "Procedural Worlds/Storm/Building Generator/Wall Blocks")]
    public class WallBlocks : ScriptableObject
    {
        public BuildingConfig parentBuildingConfig;

        public BuildingBlock foundation;

        public BuildingBlock frontStep;

        public BuildingBlock floor;

        public BuildingBlock exteriorWall;

        public BuildingBlock exteriorWallTrim;

        public BuildingBlock alternateExteriorWall;

        public BuildingBlock alternateExteriorWallTrim;

        public BuildingBlock interiorWall;

        public BuildingBlock interiorWallTrim;

        public BuildingBlock ceiling;


        public BuildingBlock railing;

        public BuildingBlock supportPillar;

        public BuildingBlock alternateWindow;

        public BuildingBlock window;

        public BuildingBlock interiorDoor;

        public BuildingBlock exteriorDoor;

        public BuildingBlock nothing;

        public BuildingBlock staircase;
        public string CheckForMissingBlocks()
        {
            if(parentBuildingConfig == null)
            {
                return "Parent Building Config Missing, Please Assign One to Filter Out Unneeded Blocks";
            }
            
            string missingBlocks = "";

            if(foundation == null)
            {
                missingBlocks += "Foundation, ";
            }
            
            if(exteriorWall == null)
            {
                missingBlocks += "Exterior Wall, ";
            }
            
            if (parentBuildingConfig.createWallTrim)
            {
                if(exteriorWallTrim == null)
                {
                    missingBlocks += "Exterior Wall Trim, ";
                }
            }

            if (parentBuildingConfig.generateInteriorLayout)
            {
                if(exteriorDoor == null)
                {
                    missingBlocks += "Exterior Door, ";
                }
                
                if(interiorWall == null)
                {
                    missingBlocks += "Interior Wall, ";
                }
                
                if (parentBuildingConfig.createWallTrim)
                {
                    if(interiorWallTrim == null)
                    {
                        missingBlocks += "Interior Wall Trim, ";
                    }
                }

                if (supportPillar == null)
                {
                    //missingBlocks += "Support Pillar, ";
                }
                
                if(window == null)
                {
                    missingBlocks += "Window, ";
                }

                if (interiorDoor == null)
                {
                    missingBlocks += "Door, ";
                }

                if(ceiling == null)
                {
                    missingBlocks += "Ceiling, ";
                }
                if(floor == null)
                {
                    missingBlocks += "Floor, ";
                }

                if(railing == null)
                {
                    //missingBlocks += "Railing, ";
                }

                if(staircase == null)
                {
                    missingBlocks += "Staircase, ";
                }

                if(nothing == null)
                {
                    missingBlocks += "Nothing, ";
                }

                if (parentBuildingConfig.createFrontStep)
                {
                    if(frontStep == null)
                    {
                        //missingBlocks += "Front Step, ";
                    }
                }
            }
            if (parentBuildingConfig.useAlternateWallFirstLayer)
            {
                if(alternateExteriorWall == null)
                {
                    missingBlocks += "Alternate Exterior Wall, ";
                }
                if(alternateWindow == null)
                {
                    missingBlocks += "Alternate Window, ";
                }
                if (parentBuildingConfig.createWallTrim)
                {
                    if(alternateExteriorWallTrim == null)
                    {
                        missingBlocks += "Alternate Exterior Wall Trim, ";
                    }
                }
            }

            if(missingBlocks.Length > 0)
            {
                return missingBlocks + "Are Missing, Please add the missing blocks to the wall blocks asset with this building config.";
            }
            return "";
        }
    
        public void CopyBlocks(WallBlocks wallBlocks)
        {
            foundation = wallBlocks.foundation;
            frontStep = wallBlocks.frontStep;
            floor = wallBlocks.floor;
            exteriorWall = wallBlocks.exteriorWall;
            exteriorWallTrim = wallBlocks.exteriorWallTrim;
            alternateExteriorWall = wallBlocks.alternateExteriorWall;
            alternateExteriorWallTrim = wallBlocks.alternateExteriorWallTrim;
            interiorWall = wallBlocks.interiorWall;
            interiorWallTrim = wallBlocks.interiorWallTrim;
            ceiling = wallBlocks.ceiling;
            railing = wallBlocks.railing;
            supportPillar = wallBlocks.supportPillar;
            alternateWindow = wallBlocks.alternateWindow;
            window = wallBlocks.window;
            interiorDoor = wallBlocks.interiorDoor;
            exteriorDoor = wallBlocks.exteriorDoor;
            nothing = wallBlocks.nothing;
            staircase = wallBlocks.staircase;

        }
    }

    [System.Serializable]
    public class WallBlocksData
    {
        public WallBlocks wallBlocks;

        public float weight = 1;

        public WallBlocksData(WallBlocks wallBlocks)
        {
            this.wallBlocks = wallBlocks;
            this.weight = 1;
        }

    }
}
