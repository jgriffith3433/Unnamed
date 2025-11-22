
using System.Collections.Generic;
using UnityEngine;
using ProceduralWorlds.Storm.UI;
using ProceduralWorlds.Storm.StructureGenCore;
using ProceduralWorlds.Storm.Core;
namespace ProceduralWorlds.Storm.CityBuilder
{
    [RequireComponent(typeof(PolygonMask))]
    public class CityDistrict : MonoBehaviour
    {
        public bool useCityPolygon = true;
        public List<Vector2> districtPolygon;
        public GlobalRoadSettings majorRoadSettings;
        public GlobalRoadSettings minorRoadSettings;

        public bool generateExteriorPlots = true;
        public float maxLotSize;
        public bool allowInternalLots;

        public float minLotSideLength;

        public float baseBuildingHeight;
        public float buildingRandomness;
        public AnimationCurve buildingHeightCurve;
        [SerializeField]
        public List<DistrictBuildingType> districtBuildingTypes = new List<DistrictBuildingType>();
        private bool weightsSet = false;
        private List<float> buildingWeights = new List<float>();

        // Panel expansion states
        [HideInInspector]
        public bool polygonPanelExpanded = true;
        [HideInInspector]
        public bool roadGenerationSettingsPanelExpanded = true;
        [HideInInspector]
        public bool lotGenerationSettingsPanelExpanded = true;
        [HideInInspector]
        public bool districtBuildingSettingsPanelExpanded = true;

        private void OnValidate()
        {
            SetWeights();
        }

        public void GetDistrictPolygon(PolygonMask polygonMask, Transform centerTransform)
        {
            districtPolygon = StructureCreatorUtils.GetPolygonPointsGlobal(polygonMask, centerTransform);
        }

        public DistrictBuildingType GetBuildingToSpawn()
        {
            if (weightsSet == false) { SetWeights(); }
            if (districtBuildingTypes.Count == 0) { return null; }

            int buildingId = CityBuilderUtils.WeightedChanceSelector(buildingWeights);
            DistrictBuildingType selectedBuilding = districtBuildingTypes[buildingId];
            selectedBuilding.currentSpawns++;
            if (selectedBuilding.currentSpawns >= selectedBuilding.maxSpawns)
            {
                buildingWeights[buildingId] = 0;
            }
            return selectedBuilding;
        }
        
        private void SetWeights()
        {
            buildingWeights.Clear();
            for (int i = 0; i < districtBuildingTypes.Count; i++)
            {
                buildingWeights.Add(districtBuildingTypes[i].weight);
            }
            weightsSet = true;
        }
        

    }
}

