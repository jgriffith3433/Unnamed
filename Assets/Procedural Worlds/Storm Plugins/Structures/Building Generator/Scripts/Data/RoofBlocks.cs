using UnityEngine;

namespace ProceduralWorlds.Storm.BuildingGen
{
    [CreateAssetMenu(fileName = "Roof Blocks", menuName = "Procedural Worlds/Storm/Building Generator/Roof Blocks")]
    public class RoofBlocks : ScriptableObject
    {
        public BuildingConfig parentBuildingConfig;



        public BuildingBlock roofCorner;

        public BuildingBlock roofSegment;

        public BuildingBlock roofTop;

        public BuildingBlock roofOuterConnection;

        public BuildingBlock roofInnerCorner;

        public BuildingBlock smallPeak;

        public BuildingBlock roofPeakJoin;

        public BuildingBlock roofSlantEdgeTrim;

        public BuildingBlock roofEdgeTrim;

        public BuildingBlock roofPeakTrim;

        public BuildingBlock roofCornerToSmallPeakBlendLeft;

        public BuildingBlock roofCornerToSmallPeakBlendRight;

        public BuildingBlock roofStraightToSmallPeakBlend;

        public BuildingBlock roofSmallCorner;

        public string CheckForMissingBlocks()
        {
            if(parentBuildingConfig == null)
            {
                return "Parent Building Config Missing, Please Assign One to Filter Out Unneeded Blocks";
            }

            string missingBlocks = "";
            
            switch (parentBuildingConfig.roofType)
            {
                case RoofType.Flat:
                    if(roofTop == null)
                    {
                        missingBlocks += "Roof Top, ";
                    }
                    break;
                case RoofType.Peaked:
                    if(roofCorner== null)
                    {
                        missingBlocks += "Roof Corner, ";
                    }
                    if(roofSegment== null)
                    {
                        missingBlocks += "Roof Segment, ";
                    }
                    if(roofOuterConnection== null)
                    {
                        missingBlocks += "Roof Outer Connection, ";
                    }
                    if(roofInnerCorner== null)
                    {
                        missingBlocks += "Roof Inner Corner, ";
                    }
                    if(smallPeak == null)
                    {
                        missingBlocks += "Small Peak, ";
                    }
                    if(roofCornerToSmallPeakBlendLeft== null)
                    {
                        missingBlocks += "Roof Corner to Small Peak Blend Left, ";
                    }
                    if(roofCornerToSmallPeakBlendRight== null)
                    {
                        missingBlocks += "Roof Corner to Small Peak Blend Right, ";
                    }
                    if(roofStraightToSmallPeakBlend== null)
                    {
                        missingBlocks += "Roof Straight to Small Peak Blend, ";
                    }
                    if(roofSmallCorner== null)
                    {
                        missingBlocks += "Roof Small Corner, ";
                    }
                


                    break;
                case RoofType.PartialPeaked:
                    if(roofTop == null)
                    {
                        missingBlocks += "Roof Top, ";
                    }
                    if(roofCorner== null)
                    {
                        missingBlocks += "Roof Corner, ";
                    }
                    if(roofSegment== null)
                    {
                        missingBlocks += "Roof Segment, ";
                    }
                    if(roofOuterConnection== null)
                    {
                        missingBlocks += "Roof Outer Connection, ";
                    }
                    if(roofInnerCorner== null)
                    {
                        missingBlocks += "Roof Inner Corner, ";
                    }
                    if(smallPeak == null)
                    {
                        missingBlocks += "Small Peak, ";
                    }
                    if(roofCornerToSmallPeakBlendLeft== null)
                    {
                        missingBlocks += "Roof Corner to Small Peak Blend Left, ";
                    }
                    if(roofCornerToSmallPeakBlendRight== null)
                    {
                        missingBlocks += "Roof Corner to Small Peak Blend Right, ";
                    }
                    if(roofStraightToSmallPeakBlend== null)
                    {
                        missingBlocks += "Roof Straight to Small Peak Blend, ";
                    }
                    if(roofSmallCorner== null)
                    {
                        missingBlocks += "Roof Small Corner, ";
                    }
                    break;
            }


            if (parentBuildingConfig.createRoofTrim)
            {
                if(roofSlantEdgeTrim == null)
                {
                    //missingBlocks += "Roof Slant Edge Trim Missing, ";
                }
                if(roofEdgeTrim == null)
                {
                    missingBlocks += "Roof Edge Trim, ";
                }
                if(roofPeakTrim == null)
                {
                    missingBlocks += "Roof Peak Trim, ";
                }
            }

            if (parentBuildingConfig.useRoofPeakJoinEdgeCaseFix)
            {
                if(roofPeakJoin == null)
                {
                    missingBlocks += "Roof Peak Join, ";
                }
            }
            if(missingBlocks.Length > 0)
            {
                return missingBlocks + "Are Missing, Please add the missing blocks to the roof blocks asset with this building config.";
            }
            return "";
        }

        public void CopyBlocks(RoofBlocks roofBlocks)
        {
            roofCorner = roofBlocks.roofCorner;
            roofSegment = roofBlocks.roofSegment;
            roofTop = roofBlocks.roofTop;
            roofOuterConnection = roofBlocks.roofOuterConnection;
            roofInnerCorner = roofBlocks.roofInnerCorner;
            smallPeak = roofBlocks.smallPeak;
            roofPeakJoin = roofBlocks.roofPeakJoin;
            roofSlantEdgeTrim = roofBlocks.roofSlantEdgeTrim;
            roofEdgeTrim = roofBlocks.roofEdgeTrim;
            roofPeakTrim = roofBlocks.roofPeakTrim;

            roofCornerToSmallPeakBlendLeft = roofBlocks.roofCornerToSmallPeakBlendLeft;
            roofCornerToSmallPeakBlendRight = roofBlocks.roofCornerToSmallPeakBlendRight;
            roofStraightToSmallPeakBlend = roofBlocks.roofStraightToSmallPeakBlend;
            roofSmallCorner = roofBlocks.roofSmallCorner;
        }
    }

    [System.Serializable]
    public class RoofBlocksData
    {
        public RoofBlocks roofBlocks;
        public float weight = 1;

        public RoofBlocksData(RoofBlocks roofBlocks)
        {
            this.roofBlocks = roofBlocks;
            this.weight = 1;
        }
    }
}
