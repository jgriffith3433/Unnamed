using UnityEngine;



namespace ProceduralWorlds.Storm.CityBuilder
{

    [CreateAssetMenu(fileName = "Global Road Setting", menuName = "Procedural Worlds/Storm/City Generator/Global Road Setting")]
    public class GlobalRoadSettings : ScriptableObject
    {
        public RoadType roadType;
        public MajorRoadPlacementType majorRoadPlacementType;
        public MinorRoadPlacementType minorRoadPlacementType;

        public float roadSegmentLength;
        [Range(0, 90)]
        public float maxRoadCurve;
        [Range(0, 90)]
        public float degreeCheckInterval;
        [Range(0, 1)]
        public float roadBranchChance;
        //[Range(0, 1f)]
        //public float intersectionDeletionProbability;

        public float maxRoadConnections;
        [Range(0, 180f)]
        public float minAngleBetweenConnections;
        public float stretchConnectionRadius;

        public float roadWidth = 8;
        public float minorRoadOffshootFrequency = 250;
        public float offshootVariance;
        [Range(0, 1f)]
        public float offshootFailChance;

        public float sidewalkWidth;


        public bool seekPopulation;

        public bool failIfLowPopulation;
        [Range(0, 1)]
        public float basePopCutoffValue;
        [Range(-1, 1)]
        public float cutoffPopModifyMin;
        [Range(-1, 1)]
        public float cutoffPopModifyMax;



        public bool avoidWater;
        [Range(0, 1)]
        public float baseWaterCutoffValue;
        [Range(-1, 1)]
        public float cutoffWaterModifyMin;
        [Range(-1, 1)]
        public float cutoffWaterModifyMax;

        public bool allowBridges;
        public float bridgeStretchDistance;



        public bool reactToSlope;

        [Range(0, 90)]
        public float baseSlopeCutoffValue;
        [Range(-1, 1)]
        public float cutoffSlopeModifyMin;
        [Range(-1, 1)]
        public float cutoffSlopeModifyMax;
        
        // Panel expansion states
        [HideInInspector]
        public bool generalSettingsPanelExpanded = true;
        [HideInInspector]
        public bool populationMapSettingsPanelExpanded = true;
        [HideInInspector]
        public bool waterMapSettingsPanelExpanded = true;
        [HideInInspector]
        public bool slopeMapSettingsPanelExpanded = true;
    }

}
