using System.Collections.Generic;
using UnityEngine;
using ProceduralWorlds.Storm.StructureGenCore;

namespace ProceduralWorlds.Storm.AreaPopulator
{
    [CreateAssetMenu(fileName = "New Edge Placement", menuName = "Procedural Worlds/Storm/Area Populator/Placement Filters/Edge Placement")]
    public class EdgePlacement : PlacementFilter
    {
        #region Experimental Standard Placement Implementation
        public bool randomMode = false;
        public int targetPlacements = 0;
        #endregion

        public bool centerAlongEdge = false;
        public bool tileAlongEdge = false;
        public float forwardOffset;
        public float tilingOffset;
        public float tilingStep;

        public bool useDistanceFromCenter;
        public Vector2 distanceFromCenterRange;
        public AxisType distanceFromCenterAxisType;

        public bool allowNullCellsInSpawns = true;

        private Vector2 roomCentroid;

        public float previewAreaSize = 15f;

        #region Editor Panel States
        [HideInInspector]
        public bool edgePlacementPanelExpanded = false;
        [HideInInspector]
        public bool previewPanelExpanded = false;
        #endregion

        public override List<PlacementData> GetPlacementData(DecorationObjectInfo objectInfo, Transform areaPopulatorTransform, Vector3 lowestPoint, List<Vector2> areaPolygon, DecorationSegment[,,] decorationSegments, float segmentSize, Vector3 gridPadding, Dictionary<int, List<DecorationSegment[,]>> groundedAreas, List<DecorationSegment[,,]> corners, DecorationSegment[,,] center, DecorationSegment[,,] xAxis, DecorationSegment[,,] zAxis, XorshiftPlus random)
        {
            List<PlacementData> placements = new List<PlacementData>();
            int placementLayer = GetPlacementLayer(objectInfo, idealPlacementLayer, 0);
            float objectSize = objectInfo.objectSize.extents.x *2;

            roomCentroid = GetCentroid(areaPolygon);
            
            if(randomMode)
            {
                int runCount = 0;
                while (placements.Count < targetPlacements)
                {
                    if(runCount >= targetPlacements*5)
                    {
                        break;
                    }

                    int randomEdge = Random.Range(0, areaPolygon.Count);
                    int randomEdge2 = (randomEdge + 1) % areaPolygon.Count;
            
                    Vector2 interiorNormal = GetInteriorNormal(areaPolygon[randomEdge2], areaPolygon[randomEdge], areaPolygon); 
                    float edgeLength = Vector2.Distance(areaPolygon[randomEdge2], areaPolygon[randomEdge]);
                    if(edgeLength < objectSize)
                    {
                        continue;
                    }
                    Vector2 placementPos = GetRandomPlacementPostion(interiorNormal, areaPolygon[randomEdge], areaPolygon[randomEdge2], objectSize);
                    Vector3 placementPosVector3 = new Vector3(placementPos.x, placementLayer, placementPos.y);
                    Vector3 placementPosVector3World = areaPopulatorTransform.TransformPoint(placementPosVector3);
                    float rotation = GetPlacementRotation(interiorNormal);
                    List<DecorationSegment> segments = new List<DecorationSegment>();
                    List<DecorationSegment> objectBoundsSegments = new List<DecorationSegment>();
                    SpawnPoint spawnPoint = new SpawnPoint(new Vector3(placementPos.x, placementLayer, placementPos.y), rotation);
                    if (!objectInfo.occupySpace)
                    {
                        if (IsPointInPolygon(areaPolygon, placementPos))
                        {
                            PlacementData placementData = new PlacementData(spawnPoint, segments);
                            placements.Add(placementData);
                        }
    
                    }
                    else if (IsSpawnPointValid(objectInfo,placementPosVector3 - areaPopulatorTransform.InverseTransformPoint(lowestPoint), placementPosVector3World, rotation, objectInfo.objectSize, objectInfo.objectPolygon, objectInfo.objectNoSpawnZone, placementLayer, decorationSegments, segmentSize, gridPadding, out objectBoundsSegments, null, objectInfo.useObjectNoSpawnZone, allowNullCellsInSpawns, true))
                    {
                        PlacementData placementData = new PlacementData(spawnPoint, objectBoundsSegments);
                        placements.Add(placementData);
                    }
                    runCount++;
                }

            }
            else
            {


                int j = areaPolygon.Count - 1;
                for (int i = 0; i < areaPolygon.Count; i++)
                {

                    int placementCount = EdgePlacementCount(areaPolygon[i], areaPolygon[j], objectSize);
                    for (int k = 0; k < placementCount; k++)
                    {
                        Vector2 interiorNormal = GetInteriorNormal(areaPolygon[j], areaPolygon[i], areaPolygon);
                        Vector2 placementPos = GetPlacementPostion(k, interiorNormal, areaPolygon[i], areaPolygon[j]);
                        //Debug.DrawLine(new Vector3(placementPos.x, 0, placementPos.y), new Vector3(placementPos.x, 300, placementPos.y), Color.red, 40);
                        Vector3 placementPosVector3 = new Vector3(placementPos.x, placementLayer, placementPos.y);
                        Vector3 placementPosVector3World = areaPopulatorTransform.TransformPoint(placementPosVector3);
                        if (useDistanceFromCenter)
                        {
                            if (!CheckIfPointIsInDistanceFromCenterRange(placementPos, distanceFromCenterRange, roomCentroid, distanceFromCenterAxisType))
                            {
                                continue;
                            }
                        }

                        float rotation = GetPlacementRotation(interiorNormal);


                        List<DecorationSegment> segments = new List<DecorationSegment>();
                        List<DecorationSegment> objectBoundsSegments = new List<DecorationSegment>();
                        SpawnPoint spawnPoint = new SpawnPoint(new Vector3(placementPos.x, placementLayer, placementPos.y), rotation);
                        if (!objectInfo.occupySpace)
                        {
                            if (IsPointInPolygon(areaPolygon, placementPos))
                            {
                                PlacementData placementData = new PlacementData(spawnPoint, segments);
                                placements.Add(placementData);
                            }
                        }
                        else if (IsSpawnPointValid(objectInfo,placementPosVector3 - areaPopulatorTransform.InverseTransformPoint(lowestPoint), placementPosVector3World, rotation, objectInfo.objectSize, objectInfo.objectPolygon, objectInfo.objectNoSpawnZone, placementLayer, decorationSegments, segmentSize, gridPadding, out objectBoundsSegments, null, objectInfo.useObjectNoSpawnZone, allowNullCellsInSpawns, true))
                        {
                            PlacementData placementData = new PlacementData(spawnPoint, segments);
                            placements.Add(placementData);
                        }
                    }
                    
                    j = i;

                }
            }

            return placements;
        }

        public List<PlacementData> GetPreviewPlacementData(List<Vector2> areaPolygon)
        {
            List<PlacementData> placements = new List<PlacementData>();
            
            roomCentroid = GetCentroid(areaPolygon);
            int placementLayer = 0;

            if(randomMode)
            {
                int runCount = 0;
                while (placements.Count < targetPlacements)
                {
                    if(runCount >= targetPlacements*2)
                    {
                        break;
                    }

                    int randomEdge = Random.Range(0, areaPolygon.Count);
                    int randomEdge2 = (randomEdge + 1) % areaPolygon.Count;
            
                    Vector2 interiorNormal = GetInteriorNormal(areaPolygon[randomEdge2], areaPolygon[randomEdge], areaPolygon); 
                    Vector2 placementPos = GetRandomPlacementPostion(interiorNormal, areaPolygon[randomEdge], areaPolygon[randomEdge2], 0);

                    float rotation = GetPlacementRotation(interiorNormal);
                    List<DecorationSegment> segments = new List<DecorationSegment>();

                    SpawnPoint spawnPoint = new SpawnPoint(new Vector3(placementPos.x, placementLayer, placementPos.y), rotation);
                    if (IsPointInPolygon(areaPolygon, placementPos))
                    {
                        PlacementData placementData = new PlacementData(spawnPoint, segments);
                        placements.Add(placementData);
                    }
                    runCount++;
                }

            }
            else
            {
                int j = areaPolygon.Count - 1;
                for (int i = 0; i < areaPolygon.Count; i++)
                {
                    int placementCount = EdgePlacementCount(areaPolygon[i], areaPolygon[j], 0);
                    for (int k = 0; k < placementCount; k++)
                    {
                        Vector2 interiorNormal = GetInteriorNormal(areaPolygon[j], areaPolygon[i], areaPolygon);
                        Vector2 placementPos = GetPlacementPostion(k, interiorNormal, areaPolygon[i], areaPolygon[j]);

                        if (useDistanceFromCenter)
                        {
                            if (!CheckIfPointIsInDistanceFromCenterRange(placementPos, distanceFromCenterRange, roomCentroid, distanceFromCenterAxisType))
                            {
                                continue;
                            }
                        }

                        float rotation = GetPlacementRotation(interiorNormal);

                        List<DecorationSegment> segments = new List<DecorationSegment>();
                        List<DecorationSegment> objectBoundsSegments = new List<DecorationSegment>();
                        SpawnPoint spawnPoint = new SpawnPoint(new Vector3(placementPos.x, placementLayer, placementPos.y), rotation);
                        if (IsPointInPolygon(areaPolygon, placementPos))
                        {
                            PlacementData placementData = new PlacementData(spawnPoint, segments);
                            placements.Add(placementData);
                        }
                    }
                    j = i;
                }
            }
            

            return placements;
        }
        public int EdgePlacementCount(Vector2 edgePoint1, Vector2 edgePoint2, float objectSize)
        {
            float edgeLength = Vector2.Distance(edgePoint1, edgePoint2);
            //placement is one if we dont tile along the edge
            int placements = 1;
            if (tilingOffset > edgeLength)
            {
                placements = 0;
                return placements;
            }

            if (centerAlongEdge && tileAlongEdge)
            {
                placements = (Mathf.FloorToInt((edgeLength - tilingOffset - objectSize) / (tilingStep * 2)) * 2) + 2;
            }
            else if (!centerAlongEdge && tileAlongEdge)
            {
                placements = Mathf.FloorToInt((edgeLength - tilingOffset) / tilingStep) + 1;
            }
            return placements;
        }
        public float GetPlacementRotation(Vector2 normal)
        {
            Quaternion rotation = Quaternion.LookRotation(new Vector3(normal.x, 0, normal.y), Vector3.up);
            return rotation.eulerAngles.y;
        }
        public Vector2 GetPlacementPostion(int currentStep, Vector2 interiorNormal, Vector2 edgePoint1, Vector2 edgePoint2)
        {
            float edgeLength = Vector2.Distance(edgePoint1, edgePoint2);

            float distance = 0;

            if (tileAlongEdge)
            {
                if (centerAlongEdge)
                {


                    int moddedStep = Mathf.CeilToInt(((float)currentStep - 1) / 2);

                    if (currentStep % 2 == 0)
                    {
                        distance = (edgeLength / 2) + (tilingOffset / 2) + (tilingStep * moddedStep);
                    }
                    else
                    {
                        distance = (edgeLength / 2) - (tilingOffset / 2) - (tilingStep * moddedStep);
                    }
                }
                else
                {
                    distance = tilingOffset + (tilingStep * currentStep);
                }

            }
            else
            {
                if (centerAlongEdge)
                {
                    distance = edgeLength / 2 + tilingOffset;
                }
                else
                {
                    distance = tilingOffset;
                }
            }
            Vector2 unitVector = (edgePoint2 - edgePoint1) / edgeLength;

            Vector2 result = edgePoint1 + distance * unitVector;
            result += interiorNormal * forwardOffset;

            return result;
        }

        public Vector2 GetRandomPlacementPostion(Vector2 interiorNormal, Vector2 edgePoint1, Vector2 edgePoint2, float objectSize)
        {
            float edgeLength = Vector2.Distance(edgePoint1, edgePoint2);

            float distance = edgeLength * Random.value;
            distance = Mathf.Clamp(distance, objectSize/2, edgeLength - objectSize/2);

            Vector2 unitVector = (edgePoint2 - edgePoint1) / edgeLength;

            Vector2 result = edgePoint1 + distance * unitVector;
            result += interiorNormal * forwardOffset;

            return result;
        }
    }

}