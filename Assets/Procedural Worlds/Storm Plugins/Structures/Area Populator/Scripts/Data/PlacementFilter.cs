using System.Collections.Generic;
using UnityEngine;
using ProceduralWorlds.Storm.StructureGenCore;
using System.Linq;
namespace ProceduralWorlds.Storm.AreaPopulator
{
    public class PlacementFilter : ScriptableObject
    {
        public int idealPlacementLayer = 0;

        #region Editor Panel States
        [HideInInspector]
        public bool placementFilterPanelExpanded = false;
        #endregion

        public virtual List<PlacementData> GetPlacementData(DecorationObjectInfo objectInfo, Transform areaPopulatorTransform, Vector3 lowestPoint, List<Vector2> areaPolygon, DecorationSegment[,,] decorationSegments, float segementSize, Vector3 gridPadding, Dictionary<int, List<DecorationSegment[,]>> groundedAreas, List<DecorationSegment[,,]> corners, DecorationSegment[,,] center, DecorationSegment[,,] xAxis, DecorationSegment[,,] zAxis, XorshiftPlus random)
        {
            return null;
        }

        /// <summary>
        /// returns true if the point lies within the polygon, false if it does not
        /// </summary>
        /// <param name="polygon">a list of the polygon vertexes</param>
        /// <param name="point">the point to test</param>
        /// <returns></returns>
        public static bool IsPointInPolygon(List<Vector2> polygon, Vector2 point)
        {
            bool inside = false;
            //For each edge (In this case for each point of the polygon and the previous one)
            for (int i = 0, j = polygon.Count - 1; i < polygon.Count; i++)
            {
                //If a line from the point into infinity on the right crosses this edge
                if (((polygon[i].y > point.y) != (polygon[j].y > point.y))
                        && (point.x < (polygon[j].x - polygon[i].x) * (point.y - polygon[i].y) / (polygon[j].y - polygon[i].y) + polygon[i].x))
                {
                    inside = !inside;
                }
                j = i;
            }
            return inside;
        }

        public static int WeightedChanceSelector(List<float> weights, XorshiftPlus random)
        {
            float ratioSum = 0;
            ratioSum = weights.Sum(x => x);
            double randomNum = random.Next();
            double numericValue = randomNum * ratioSum;

            for (int i = 0; i < weights.Count; i++)
            {
                numericValue -= weights[i];

                if ((numericValue <= 0))
                {

                    return i;
                }
            }
            return default;
        }

        public Vector3 ApplyJitter(float jitter, Vector3 centerPoint, XorshiftPlus random = null)
        {
            if (random == null)
            {
                Vector2 randomPointInCircle = Random.insideUnitCircle * jitter;
                return randomPointInCircle;
            }


            float angle = random.Next() * Mathf.PI * 2;
            Vector3 returnPoint = centerPoint + new Vector3(Mathf.Cos(angle) * jitter, 0, Mathf.Sin(angle) * jitter);

            return returnPoint;
        }

        public static List<PlacementData> GetBranchObjectPlacementData(DecorationObjectInfo objectInfo, BranchObject branchObjectInfo, Transform areaPopulatorTransform, Transform parent, Vector3 lowestPoint, DecorationSegment[,,] decorationSegments, float segementSize, Vector3 gridPadding, XorshiftPlus random)
        {
            Vector3 placementLocationRelative = branchObjectInfo.relativePosition;

            // Apply parent's rotation to the relative position
            Vector3 rotatedRelativePosition = parent.rotation * placementLocationRelative;
            
            // Calculate global position by adding rotated relative position to parent position
            Vector3 placementLocationGlobal = parent.position + rotatedRelativePosition;
            Vector3 placementLocationLocal = placementLocationGlobal - lowestPoint;
            //Debug.DrawRay(placementLocationLocal, Vector3.up, Color.red, 100);
            float placementRotation = branchObjectInfo.reletiveRotation;

            SpawnPoint spawnPoint = new SpawnPoint(placementLocationRelative, placementRotation);
            List<DecorationSegment> segments = new List<DecorationSegment>();


            List<PlacementData> placementDatas = new List<PlacementData>();
            DecorationObjectInfo parentObject = null;
            if (branchObjectInfo.ignoreCellsOccupiedByParent)
            {
                parentObject = objectInfo;
            }


            if (branchObjectInfo.objectInfo.occupySpace == false)
            {
                PlacementData placementData = new PlacementData(spawnPoint, segments);
                placementDatas.Add(placementData);
            }
            else if (IsSpawnPointValid(branchObjectInfo.objectInfo, placementLocationLocal, placementLocationGlobal, placementRotation + parent.rotation.eulerAngles.y, branchObjectInfo.objectInfo.objectSize, branchObjectInfo.objectInfo.objectPolygon, branchObjectInfo.objectInfo.objectNoSpawnZone, 0, decorationSegments, segementSize, gridPadding, out segments, parentObject, false, branchObjectInfo.allowNullCellsInSpawns))
            {
                PlacementData placementData = new PlacementData(spawnPoint, segments);
                placementDatas.Add(placementData);
            }

            return placementDatas;
        }

        public Vector3 AverageWorldPostionOfSegments(List<DecorationSegment> segments)
        {
            Vector3 averagePosition = Vector3.zero;
            foreach (DecorationSegment segment in segments)
            {
                if (segment == null)
                {
                    continue;
                }
                averagePosition += segment.worldPosition;
            }
            averagePosition /= segments.Count;

            return averagePosition;

        }
        public Vector3 AverageLocalPostionOfSegments(List<DecorationSegment> segments)
        {
            Vector3 averagePosition = Vector3.zero;
            foreach (DecorationSegment segment in segments)
            {
                if (segment == null)
                {
                    continue;
                }
                averagePosition += segment.localPosition;
            }
            averagePosition /= segments.Count;
            return averagePosition;

        }

        public Vector2 GetInteriorNormal(Vector2 vertex1, Vector2 vertex2, List<Vector2> polygon)
        {
            // Calculate the edge vector
            Vector2 edge = vertex2 - vertex1;

            Vector2 edgeCenter = (vertex1 + vertex2) / 2;
            Vector2 normal = new Vector2(edge.y, -edge.x);
            normal.Normalize();
            Vector2 normalTestPoint = normal + edgeCenter;
            if (!StructureCreatorUtils.IsPointInPolygon(polygon, normalTestPoint))
            {
                normal = -normal;
            }

            return normal;
        }


        public Vector2 GetCentroid(List<Vector2> vertices)
        {
            Vector2 centroid = Vector2.zero;
            foreach (Vector2 vertex in vertices)
            {
                centroid += vertex;
            }
            return centroid / vertices.Count;
        }
        public static List<Vector2> ConvertPolygonAndPointToVectors(List<Vector2> polygon, Vector2 point)
        {
            List<Vector2> polygonVectors = new List<Vector2>();
            for (int i = 0; i < polygon.Count; i++)
            {
                polygonVectors.Add(polygon[i] - point);
            }
            return polygonVectors;
        }
        public static float CalcuateAngleBetweenPoints(Vector2 orignAngle, Vector2 connectionAngle)
        {
            float x = connectionAngle.x - orignAngle.x;
            float y = connectionAngle.y - orignAngle.y;

            return ClampToRad(Mathf.Atan2(y, x));


        }
        public static float ClampToRad(float unclampedRad)
        {
            if (unclampedRad > Mathf.PI * 2)
            {
                return unclampedRad - (Mathf.PI * 2);
            }
            else if (unclampedRad < 0)
            {
                return unclampedRad + (Mathf.PI * 2);
            }
            return unclampedRad;
        }

        public static List<Vector2> RotatePolygon(List<Vector2> polygon, Vector2 rotationPoint, float rotationAmount)
        {
            List<Vector2> rotatedPolygon = new List<Vector2>();
            List<Vector2> polygonVectors = ConvertPolygonAndPointToVectors(polygon, rotationPoint);
            List<float> angles = new List<float>();
            for (int i = 0; i < polygon.Count; i++)
            {
                angles.Add(CalcuateAngleBetweenPoints(rotationPoint, polygon[i]));
            }
            for (int i = 0; i < polygonVectors.Count; i++)
            {
                float vectorMagnitude = polygonVectors[i].magnitude;
                Vector2 newVector = new Vector2(Mathf.Cos(angles[i] + rotationAmount) * vectorMagnitude, Mathf.Sin(angles[i] + rotationAmount) * vectorMagnitude);
                Vector2 rotatedPoint = newVector + rotationPoint;
                rotatedPolygon.Add(rotatedPoint);
            }
            return rotatedPolygon;
        }

        public static List<DecorationSegment> GetSegmentsInObjectArea(Vector3 objectLocalPosition, Vector3 objectWorldPositon, float rotation, Bounds objectBounds, List<Vector2> objectPolygon, int yLevel, DecorationSegment[,,] decorationSegments, float segmentSize, Vector3 gridPadding, bool usePolygon = true)
        {
            objectLocalPosition = AlterLocalPositionBasedOnGridPadding(segmentSize, objectLocalPosition, gridPadding);
            List<DecorationSegment> resultSegments = new List<DecorationSegment>();
            Vector3 objectBottomLeft = objectLocalPosition - new Vector3(objectBounds.extents.x - 0.01f, 0, objectBounds.extents.z - 0.01f);
            Vector3 objectTopRight = objectLocalPosition + new Vector3(objectBounds.extents.x - 0.01f, 0, objectBounds.extents.z - 0.01f);
            List<Vector2> polygon = new List<Vector2>(objectPolygon);
            if (rotation != 0)
            {
                float cornerToConerDistance = Mathf.Sqrt(Mathf.Pow(objectBounds.extents.x, 2) + Mathf.Pow(objectBounds.extents.z, 2));
                objectBottomLeft = objectLocalPosition - new Vector3(cornerToConerDistance - 0.01f, 0, cornerToConerDistance - 0.01f);
                objectTopRight = objectLocalPosition + new Vector3(cornerToConerDistance - 0.01f, 0, cornerToConerDistance - 0.01f);
                // temp1 = objectWorldPositon - new Vector3(cornerToConerDistance - 0.01f, 0, cornerToConerDistance - 0.01f);
                //Vector3 temp2 = objectWorldPositon + new Vector3(cornerToConerDistance - 0.01f, 0, cornerToConerDistance - 0.01f);
                //Debug.DrawLine(temp1, temp1 + Vector3.up * 4, Color.blue, 30);
                //Debug.DrawLine(temp2, temp2 + Vector3.up * 4, Color.blue, 30);
                polygon = RotatePolygon(polygon, Vector2.zero, -rotation * Mathf.Deg2Rad);
            }
            Vector3Int bottomLeftSegment = new Vector3Int(Mathf.FloorToInt(objectBottomLeft.x / segmentSize), yLevel, Mathf.FloorToInt(objectBottomLeft.z / segmentSize));
            Vector3Int topRightSegment = new Vector3Int(Mathf.CeilToInt(objectTopRight.x / segmentSize), yLevel, Mathf.CeilToInt(objectTopRight.z / segmentSize));
            //Debug.Log("bottomLeftSegment: " + bottomLeftSegment + " topRightSegment: " + topRightSegment);
            int xSteps = topRightSegment.x - bottomLeftSegment.x;
            int zSteps = topRightSegment.z - bottomLeftSegment.z;
            int ySteps = Mathf.RoundToInt(objectBounds.extents.y * 2 / segmentSize);
            Vector2 objectWorldPosVector2 = new Vector2(objectWorldPositon.x, objectWorldPositon.z);
            for (int x = 0; x < xSteps; x++)
            {
                for (int y = 0; y < ySteps; y++)
                {
                    for (int z = 0; z < zSteps; z++)
                    {
                        Vector3Int currentPosition = new Vector3Int(bottomLeftSegment.x + x, yLevel + y, bottomLeftSegment.z + z);


                        if (IsSegmentValid(currentPosition, decorationSegments.GetLength(0), decorationSegments.GetLength(1), decorationSegments.GetLength(2)))
                        {
                            DecorationSegment segment = decorationSegments[currentPosition.x, currentPosition.y, currentPosition.z];
                            //FLAG
                            //basically sometimes segments are null
                            if (segment == null)
                            {
                                resultSegments.Add(segment);
                                continue;
                            }
                            if (usePolygon)
                            {
                                Vector3 segmentWorldPosition = segment.worldPosition;

                                Vector2 segmentWorldPosVector2 = new Vector2(segmentWorldPosition.x, segmentWorldPosition.z);
                                Vector2 segmentPoint1 = segmentWorldPosVector2 + new Vector2(segmentSize / 2, segmentSize / 2);
                                Vector2 segmentPoint2 = segmentWorldPosVector2 + new Vector2(segmentSize / 2, -segmentSize / 2);
                                Vector2 segmentPoint3 = segmentWorldPosVector2 + new Vector2(-segmentSize / 2, -segmentSize / 2);
                                Vector2 segmentPoint4 = segmentWorldPosVector2 + new Vector2(-segmentSize / 2, segmentSize / 2);
                                if (!IsPointInPolygonWorld(polygon, objectWorldPosVector2, segmentPoint1) && !IsPointInPolygonWorld(polygon, objectWorldPosVector2, segmentPoint2)
                                    && !IsPointInPolygonWorld(polygon, objectWorldPosVector2, segmentPoint3) && !IsPointInPolygonWorld(polygon, objectWorldPosVector2, segmentPoint4))
                                {
                                    continue;
                                }
                            }

                            resultSegments.Add(segment);
                        }
                        else
                        {
                            DecorationSegment segment = null;
                            resultSegments.Add(segment);
                        }
                    }
                }

            }
            return resultSegments;
        }

        public List<DecorationSegment> GetSegmentsInObjectArea(Vector3 objectPosition, Vector3 objectWorldPosition, List<Vector2> objectPolygon, Bounds objectBounds, int yLevel, DecorationSegment[,,] decorationSegments, float segmentSize, Vector3 gridPadding)
        {
            //objectPosition = AlterLocalPositionBasedOnGridPadding(segmentSize, objectPosition, gridPadding);
            List<DecorationSegment> resultSegments = new List<DecorationSegment>();
            float biggestExtents = Mathf.Max(objectBounds.extents.x, objectBounds.extents.z);
            Vector3 objectBottomLeft = objectPosition - new Vector3(biggestExtents, 0, biggestExtents);
            Vector3 objectTopRight = objectPosition + new Vector3(biggestExtents, 0, biggestExtents);
            Vector3Int bottomLeftSegment = new Vector3Int(Mathf.FloorToInt(objectBottomLeft.x / segmentSize), yLevel, Mathf.FloorToInt(objectBottomLeft.z / segmentSize));
            Vector3Int topRightSegment = new Vector3Int(Mathf.CeilToInt(objectTopRight.x / segmentSize), yLevel, Mathf.CeilToInt(objectTopRight.z / segmentSize));
            int xSteps = topRightSegment.x - bottomLeftSegment.x;
            int zSteps = topRightSegment.z - bottomLeftSegment.z;

            int ySteps = Mathf.RoundToInt(objectBounds.extents.y * 2 / segmentSize);

            List<Vector2> actualObjectPolygon = new List<Vector2>();
            for (int i = 0; i < objectPolygon.Count; i++)
            {

                actualObjectPolygon.Add(objectPolygon[i] + new Vector2(objectWorldPosition.x, objectWorldPosition.z));
            }
            for (int x = 0; x < xSteps; x++)
            {
                for (int y = 0; y < 1; y++)
                {
                    for (int z = 0; z < zSteps; z++)
                    {
                        Vector3Int currentPosition = new Vector3Int(bottomLeftSegment.x + x, yLevel + y, bottomLeftSegment.z + z);
                        if (IsSegmentValid(currentPosition, decorationSegments.GetLength(0), decorationSegments.GetLength(1), decorationSegments.GetLength(2)))
                        {
                            DecorationSegment segment = decorationSegments[currentPosition.x, currentPosition.y, currentPosition.z];

                            Vector2 segmentPos = new Vector2(segment.worldPosition.x, segment.worldPosition.z);
                            Debug.DrawLine(segment.worldPosition, segment.worldPosition + Vector3.up * 10f, Color.red, 100);

                            Vector2 segmentBottomLeft = segmentPos + new Vector2(-segmentSize / 2, -segmentSize / 2);
                            Vector2 segmentBottomRight = segmentPos + new Vector2(segmentSize / 2, -segmentSize / 2);
                            Vector2 segmentTopRight = segmentPos + new Vector2(segmentSize / 2, segmentSize / 2);
                            Vector2 segmentTopLeft = segmentPos + new Vector2(-segmentSize / 2, segmentSize / 2);
                            List<Vector2> segmentPolygon = new List<Vector2>
                        {
                            segmentBottomLeft,
                            segmentBottomRight,
                            segmentTopRight,
                            segmentTopLeft
                        };

                            if (DoPolygonsIntersect(segmentPolygon, actualObjectPolygon))
                            {
                                resultSegments.Add(segment);
                            }
                        }
                        else
                        {
                            DecorationSegment segment = null;
                            resultSegments.Add(segment);
                        }

                    }
                }

            }

            return resultSegments;
        }

        /// <summary>
        /// returns true if the point lies within the polygon, false if it does not
        /// </summary>
        /// <param name="polygon">a list of the polygon vertexes</param>
        /// <param name="point">the point to test</param>
        /// <returns></returns>
        public static bool IsPointInPolygonWorld(List<Vector2> localPolygon, Vector2 polygonPos, Vector2 point)
        {
            //Debug.DrawLine(new Vector3(point.x, 0, point.y), new Vector3(point.x, 0, point.y) + Vector3.up * 1f, Color.blue, 30);
            List<Vector2> polygon = new List<Vector2>(localPolygon);
            for (int i = 0; i < polygon.Count; i++)
            {
                polygon[i] = polygon[i] + polygonPos;


            }
            bool inside = false;
            //For each edge (In this case for each point of the polygon and the previous one)
            for (int i = 0, j = polygon.Count - 1; i < polygon.Count; i++)
            {
                //Debug.DrawLine(new Vector3(polygon[i].x, 0, polygon[i].y), new Vector3(polygon[j].x, 0, polygon[j].y), Color.red, 30);
                //If a line from the point into infinity on the right crosses this edge
                if (((polygon[i].y > point.y) != (polygon[j].y > point.y))
                        && (point.x < (polygon[j].x - polygon[i].x) * (point.y - polygon[i].y) / (polygon[j].y - polygon[i].y) + polygon[i].x))
                {
                    inside = !inside;
                }
                j = i;
            }
            return inside;
        }
        bool DoPolygonsIntersect(List<Vector2> poly1, List<Vector2> poly2)
        {
            for (int i = 0; i < poly1.Count; i++)
            {
                Vector2 a1 = poly1[i];
                Vector2 a2 = poly1[(i + 1) % poly1.Count];
                for (int j = 0; j < poly2.Count; j++)
                {
                    Vector2 b1 = poly2[j];
                    Vector2 b2 = poly2[(j + 1) % poly2.Count];
                    if (DoSegmentsIntersect(a1, a2, b1, b2))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        bool DoSegmentsIntersect(Vector2 a1, Vector2 a2, Vector2 b1, Vector2 b2)
        {
            float denominator = (a2.x - a1.x) * (b2.y - b1.y) - (a2.y - a1.y) * (b2.x - b1.x);
            float numerator1 = (a1.y - b1.y) * (b2.x - b1.x) - (a1.x - b1.x) * (b2.y - b1.y);
            float numerator2 = (a1.y - b1.y) * (a2.x - a1.x) - (a1.x - b1.x) * (a2.y - a1.y);

            if (denominator == 0) return numerator1 == 0 && numerator2 == 0;

            float r = numerator1 / denominator;
            float s = numerator2 / denominator;

            return (r >= 0 && r <= 1) && (s >= 0 && s <= 1);
        }

        public static Vector3 AlterLocalPositionBasedOnGridPadding(float segmentSize, Vector3 localPosition, Vector3 gridPadding)
        {
            return localPosition - new Vector3(gridPadding.x - (segmentSize / 2), gridPadding.y - (segmentSize / 2), gridPadding.z - (segmentSize / 2));
        }

        public int GetPlacementLayer(DecorationObjectInfo objectInfo, int idealPlacementLayer, int ySteps)
        {
            //this function is not implemented
            return idealPlacementLayer;
        }

        public static bool IsSegmentValid(Vector3Int currentPlacementTile, int xSteps, int ySteps, int zSteps)
        {
            if (currentPlacementTile.x < 0 || currentPlacementTile.x >= xSteps || currentPlacementTile.y < 0 || currentPlacementTile.y >= ySteps || currentPlacementTile.z < 0 || currentPlacementTile.z >= zSteps)
            {
                return false;
            }
            return true;
        }

        public virtual float GetPlacementDireciton(DecorationSegment segment, PlacementDirections placementDirection)
        {
            Vector2 direction = new Vector2(0, 0);
            if (placementDirection == PlacementDirections.Inconsequential)
            {
                direction = new Vector2(0, 0);
            }
            else if (placementDirection == PlacementDirections.FacingNearestEdge)
            {
                direction = segment.directionToClosestEdge;
            }
            else if (placementDirection == PlacementDirections.FacingTowardsCenter)
            {
                direction = -segment.directionToClosestEdge;
            }

            return 0;
        }

        public bool CheckIfPointIsInDistanceFromCenterRange(Vector2 vertex1, Vector2 distanceFromCenterRange, Vector2 roomCentroid, AxisType axis)
        {
            Vector2 distanceFromCentroid1 = new Vector2(Mathf.Abs(roomCentroid.x - vertex1.x), Mathf.Abs(roomCentroid.y - vertex1.y));
            if (axis == AxisType.X)
            {

                if (distanceFromCentroid1.x > distanceFromCenterRange.x && distanceFromCentroid1.x < distanceFromCenterRange.y)
                {
                    return true;
                }
            }
            else if (axis == AxisType.Z)
            {
                if (distanceFromCentroid1.y > distanceFromCenterRange.x && distanceFromCentroid1.y < distanceFromCenterRange.y)
                {
                    return true;
                }
            }
            return false;
        }

        public static bool IsSpawnPointValid(DecorationObjectInfo objectInfo, Vector3 localPostion, Vector3 worldPosition, float rotation, Bounds objectBounds, List<Vector2> objectPolygon, Bounds objectNoSpawnZone, int yLevel, DecorationSegment[,,] decorationSegments, float segmentSize, Vector3 gridPadding, out List<DecorationSegment> objectBoundsSegments, DecorationObjectInfo allowedOccupyObject, bool useObjectNoSpawnZone = false, bool allowNullCellsInSpawns = false, bool occupyImmediately = false)
        {
            objectBoundsSegments = GetSegmentsInObjectArea(localPostion, worldPosition, rotation, objectBounds, objectPolygon, yLevel, decorationSegments, segmentSize, gridPadding);
            List<DecorationSegment> objectNoSpawnZoneSegments = new List<DecorationSegment>();
            if (useObjectNoSpawnZone)
            {
                objectNoSpawnZoneSegments = GetSegmentsInObjectArea(localPostion, worldPosition, rotation, objectNoSpawnZone, objectPolygon, yLevel, decorationSegments, segmentSize, gridPadding, false);
                foreach (DecorationSegment segment in objectNoSpawnZoneSegments)
                {
                    if (segment == null)
                    {
                        return false;
                    }

                    if (!IsSegmentValid(segment.gridPosition, decorationSegments.GetLength(0), decorationSegments.GetLength(1), decorationSegments.GetLength(2)))
                    {
                        return false;
                    }
                    if(allowedOccupyObject != null)
                    {
                        if(segment.occupied && segment.occupySource.name == allowedOccupyObject.name)
                        {
                            continue;
                        }
                    }
                

                    if (segment.occupied)
                    {
                        return false;
                    }
                }
            }
            bool allNull = true;

            if(objectBoundsSegments.Count == 0)
            {
                Debug.LogWarning("Object bounds segments count is 0, " + objectInfo.gameObject.name + " probably has a polygon or bounds smaller than the segment size");
                return false;
            }

            foreach (DecorationSegment segment in objectBoundsSegments)
            {
                if (segment == null)
                {
                    if (allowNullCellsInSpawns)
                    {
                        continue;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    allNull = false;
                }

                if(allowedOccupyObject != null)
                {
                    if(segment.occupied && segment.occupySource != null && segment.occupySource.name == allowedOccupyObject.name)
                    {
                        continue;
                    }
                }

                if (segment.occupied)
                {
                    return false;
                }
            }
            if (allNull)
            {
                return false;
            }
            if (occupyImmediately)
            {
                foreach (DecorationSegment segment in objectBoundsSegments)
                {
                    if (segment == null)
                    {
                        continue;
                    }

                    segment.occupied = true;
                    if(segment.occupySource == null)
                    {
                        segment.occupySource = objectInfo;
                        segment.occupySourcePosition = worldPosition;
                    }

                }
            }
            return true;
        }
   
        public static Vector3 RotateVectorAroundPoint(Vector3 vector, Vector3 point, Quaternion rotation)
        {
            return rotation * (vector - point) + point;
        }
    }

    public enum AxisType
    {
        X,
        Z
    }





}
