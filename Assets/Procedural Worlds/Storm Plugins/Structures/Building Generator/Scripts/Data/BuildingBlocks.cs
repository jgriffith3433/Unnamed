using System.Collections.Generic;
using UnityEngine;
using ProceduralWorlds.Storm.StructureGenCore;
namespace ProceduralWorlds.Storm.BuildingGen
{
    [CreateAssetMenu(fileName = "Building Blocks", menuName = "Procedural Worlds/Storm/Building Generator/Building Blocks")]
    public class BuildingBlocks : ScriptableObject
    {
        public List<WallBlocksData> wallBlocksList = new List<WallBlocksData>();
        private List<float> wallBlockWeights = new List<float>();

        public int selectedWallBlocksInt = 0;
        public List<RoofBlocksData> roofBlocksList = new List<RoofBlocksData>();

        private List<float> roofBlockWeights = new List<float>();

        public int selectedRoofBlocksInt = 0;
        public WallBlocks GetWallBlocks(XorshiftPlus randomGen)
        {
            int numToSpawn = StructureCreatorUtils.WeightedChanceSelector(wallBlockWeights, randomGen);
            return wallBlocksList[numToSpawn].wallBlocks;
        }

        public RoofBlocks GetRoofBlocks(XorshiftPlus randomGen)
        {
            int numToSpawn = StructureCreatorUtils.WeightedChanceSelector(roofBlockWeights, randomGen);
            return roofBlocksList[numToSpawn].roofBlocks;
        }

        public (List<WallBlocksData>, List<RoofBlocksData>) Copy(BuildingBlocks buildingBlocks)
        {
            wallBlocksList = buildingBlocks.wallBlocksList;
            roofBlocksList = buildingBlocks.roofBlocksList;
            selectedWallBlocksInt = buildingBlocks.selectedWallBlocksInt;
            selectedRoofBlocksInt = buildingBlocks.selectedRoofBlocksInt;
            SyncWallBlocksWeights();
            SyncRoofBlocksWeights();

            return (wallBlocksList, roofBlocksList);
        }


        public void OnValidate()
        {
            SyncWallBlocksWeights();
            SyncRoofBlocksWeights();
        }

        private void SyncRoofBlocksWeights()
        {
            // Synchronize weights
            if (roofBlockWeights == null)
            {
                roofBlockWeights = new List<float>();
            }
            
            // Resize list if needed
            while (roofBlockWeights.Count < roofBlocksList.Count)
            {
                roofBlockWeights.Add(0f);
            }
            while (roofBlockWeights.Count > roofBlocksList.Count)
            {
                roofBlockWeights.RemoveAt(roofBlockWeights.Count - 1);
            }
            
            // Copy weights
            for (int i = 0; i < roofBlocksList.Count; i++)
            {
                if (roofBlocksList[i] != null)
                {
                    roofBlockWeights[i] = roofBlocksList[i].weight;
                }
            }
        }

        
        private void SyncWallBlocksWeights()
        {
            // Synchronize weights
            if (wallBlockWeights == null)
            {
                wallBlockWeights = new List<float>();
            }
            
            // Resize list if needed
            while (wallBlockWeights.Count < wallBlocksList.Count)
            {
                wallBlockWeights.Add(0f);
            }
            while (wallBlockWeights.Count > wallBlocksList.Count)
            {
                wallBlockWeights.RemoveAt(wallBlockWeights.Count - 1);
            }
            
            // Copy weights
            for (int i = 0; i < wallBlocksList.Count; i++)
            {
                if (wallBlocksList[i] != null)
                {
                    wallBlockWeights[i] = wallBlocksList[i].weight;
                }
            }
        }
    }

}

