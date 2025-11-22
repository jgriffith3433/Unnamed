
using ProceduralWorlds.Storm.StructureGenCore;
using System.Collections.Generic;
using UnityEngine;
namespace ProceduralWorlds.Storm.AreaPopulator
{
    [CreateAssetMenu(fileName = "New Line Placement", menuName = "Procedural Worlds/Storm/Area Populator/Placement Filters/Line Placement")]
    public class LinePlacement : PlacementFilter
    {
        public float jitter = 0;
        public Vector2 lineVector = Vector2.zero;

        public bool tileAlongMainAxis = false;
        public bool tileAlongSecondaryAxis = false;
        public Vector2 tilingDistance; // x is main axis, y is secondary axis
        public Vector2 firstStepDistance; // x is main axis, y is secondary axis
        public bool centerTileing;

        public bool useMainAxisCountOveride;
        public bool useSecondaryAxisOveride;
        public int mainAxisCountOveride;
        public int secondaryAxisCountOveride;

        //public bool useDistanceFromCenter;
        //public Vector2 distanceFromCenterRange;
        public bool alignObjectsToLine = false;

        public bool allowNullCellsInSpawns = false;

        private Line? line;

        private Line perpendicularLine;

        int mainAxisCount = 1;
        int secondaryAxisCount = 1;



        private Vector2 areaCentroid;
        List<Vector2> areaPolygon;

        Vector3 lowestPoint;

        Transform areaPopulatorTransform;

        public float previewAreaSize = 15;

        #region Editor Panel States
        [HideInInspector]
        public bool linePlacementPanelExpanded = false;
        [HideInInspector]
        public bool previewPanelExpanded = false;
        #endregion


        public void OnValidate()
        {
            //Debug.Log("validating");
        }

        public override List<PlacementData> GetPlacementData(DecorationObjectInfo objectInfo, Transform areaPopulatorTransform, Vector3 lowestPoint, List<Vector2> areaPolygon, DecorationSegment[,,] decorationSegments, float segmentSize, Vector3 gridPadding, Dictionary<int, List<DecorationSegment[,]>> groundedAreas, List<DecorationSegment[,,]> corners, DecorationSegment[,,] center, DecorationSegment[,,] xAxis, DecorationSegment[,,] zAxis, XorshiftPlus random)
        {
            this.areaPolygon = areaPolygon;
            this.lowestPoint = lowestPoint;
            this.areaPopulatorTransform = areaPopulatorTransform;
            List<PlacementData> placements = new List<PlacementData>();
            int xSteps = decorationSegments.GetLength(0);
            int ySteps = decorationSegments.GetLength(1);
            int zSteps = decorationSegments.GetLength(2);
            int placementLayer = GetPlacementLayer(objectInfo, idealPlacementLayer, ySteps);
            areaCentroid = GetCentroid(areaPolygon);
            if (!GetLineData(true))
            {
                return placements;
            }

            mainAxisCount = 1;
            secondaryAxisCount = 1;
            if (tileAlongMainAxis)
            {
                mainAxisCount = GetTiledObjectCount(line.Value.Length, tilingDistance.x, firstStepDistance.x);
                if (useMainAxisCountOveride)
                {
                    mainAxisCount = mainAxisCountOveride;
                }
            }
            if (tileAlongSecondaryAxis)
            {
                //problem here currently where we dont acctually know the length of the secondary axis
                secondaryAxisCount = GetTiledObjectCount(line.Value.Length, tilingDistance.y, firstStepDistance.y, true);
                if (useSecondaryAxisOveride)
                {
                    secondaryAxisCount = secondaryAxisCountOveride;
                }

            }
            else
            {
                secondaryAxisCount = 1;
            }

            float placementRotation = 0;
            if (alignObjectsToLine)
            {
                placementRotation = GetPlacementRotation(line.Value.Vector);
            }
            for (int i = 0; i < mainAxisCount; i++)
            {
               
                for (int k = 0; k < secondaryAxisCount; k++)
                {
                    Vector2 basePlacementPos = GetPlacementPos(i, k);
                    Vector3 currentPlacement = new Vector3(basePlacementPos.x, placementLayer * segmentSize, basePlacementPos.y);
                    //currentPlacement = RotateVectorAroundPoint(currentPlacement, Vector3.zero, areaPopulatorTransform.rotation);
                    Vector3 currentWorldPlacement = areaPopulatorTransform.TransformPoint(currentPlacement);
                    if (jitter > 0)
                    {
                        currentPlacement = ApplyJitter(jitter, currentPlacement, random);
                    }

                    List<DecorationSegment> placementSegments = new List<DecorationSegment>();
                    if (!objectInfo.occupySpace)
                    {
                        
                        if (IsPointInPolygon(areaPolygon, new Vector2(currentPlacement.x, currentPlacement.z)))
                        {
                            PlacementData placementData = new PlacementData(new SpawnPoint(currentPlacement, placementRotation), placementSegments);
                            placements.Add(placementData);
                        }

                    }
                    else if (IsSpawnPointValid(objectInfo,currentPlacement - areaPopulatorTransform.InverseTransformPoint(lowestPoint), currentWorldPlacement, placementRotation, objectInfo.objectSize, objectInfo.objectPolygon, objectInfo.objectNoSpawnZone, placementLayer, decorationSegments, segmentSize, gridPadding, out placementSegments, null, objectInfo.useObjectNoSpawnZone, allowNullCellsInSpawns))
                    {
                        PlacementData placementData = new PlacementData(new SpawnPoint(currentPlacement, placementRotation), placementSegments);
                        placements.Add(placementData);
                    }

                    

                }
            }

            return placements;
        }

        public List<PlacementData> GetPreviewPlacementData(List<Vector2> areaPolygon, Vector2 segmentSize)
        {
            this.areaPolygon = areaPolygon;
            List<PlacementData> placements = new List<PlacementData>();
            Vector2 polygonSize = StructureCreatorUtils.GetSizeOfPolygonBoundingBox(areaPolygon);
            Vector2Int steps = new Vector2Int(Mathf.FloorToInt(polygonSize.x / segmentSize.x), Mathf.FloorToInt(polygonSize.y / segmentSize.y));
            int xSteps = steps.x;
            int zSteps = steps.y;

            areaCentroid = GetCentroid(areaPolygon);
            if (!GetLineData(false))
            {
                return placements;
            }
            mainAxisCount = 1;
            secondaryAxisCount = 1;
            if (tileAlongMainAxis)
            {
                mainAxisCount = GetTiledObjectCount(line.Value.Length, tilingDistance.x, firstStepDistance.x);
                if (useMainAxisCountOveride)
                {
                    mainAxisCount = mainAxisCountOveride;
                }
            }
            if (tileAlongSecondaryAxis)
            {
                //problem here currently where we dont acctually know the length of the secondary axis
                secondaryAxisCount = GetTiledObjectCount(line.Value.Length, tilingDistance.y, firstStepDistance.y, true);
                if (useSecondaryAxisOveride)
                {
                    secondaryAxisCount = secondaryAxisCountOveride;
                }
            }
            else
            {
                secondaryAxisCount = 1;
            }

            float placementRotation = 0;
            if (alignObjectsToLine)
            {
                placementRotation = GetPlacementRotation(line.Value.Vector);
            }

            for (int i = 0; i < mainAxisCount; i++)
            {
                for (int k = 0; k < secondaryAxisCount; k++)
                {
                    Vector2 basePlacementPos = GetPlacementPos(i, k);
                    Vector3 currentPlacement = new Vector3(basePlacementPos.x, 0, basePlacementPos.y);
                    if (jitter > 0)
                    {
                        currentPlacement = ApplyJitter(jitter, currentPlacement);
                    }


                    List<DecorationSegment> placementSegments = new List<DecorationSegment>();
                    if (IsPointInPolygon(areaPolygon, new Vector2(currentPlacement.x, currentPlacement.z)))
                    {
                        PlacementData placementData = new PlacementData(new SpawnPoint(currentPlacement, placementRotation), placementSegments);
                        placements.Add(placementData);
                    }
                }
            }

            return placements;
        }

        public int GetTiledObjectCount(float length, float step, float firstStep, bool secondaryAxis = false)
        {
            float lengthWithoutFirstStep = length - firstStep;

            if (secondaryAxis)
            {
                lengthWithoutFirstStep = length - firstStep * 2;
            }
            int count = Mathf.FloorToInt(lengthWithoutFirstStep / step) + 1;
            return count;
        }

        public bool GetLineData(bool useAreaPopPos = true)
        {
            line = GetLineFromVectorAndPoint(lineVector, areaCentroid);

            if (line == null)
            {
                return false;
            }
            Vector2? point1 = null;
            Vector2? point2 = null;
            int j = areaPolygon.Count - 1;
            for (int i = 0; i < areaPolygon.Count; i++)
            {
                Line? polygonLine = GetLineFromTwoPoints(areaPolygon[j], areaPolygon[i]);
                //DrawLine(polygonLine.Value, Color.red, useAreaPopPos);
                if (polygonLine == null)
                {
                    j = i;
                    continue;
                }
                if (!point1.HasValue)
                {
                    point1 = FindIntersection(line.Value, polygonLine.Value);
                    j = i;
                    continue;
                }
                if (!point2.HasValue)
                {
                    point2 = FindIntersection(line.Value, polygonLine.Value);
                    if (point2.HasValue)
                    {
                        if (Vector2.Distance(point1.Value, point2.Value) < 0.1f)
                        {
                            point2 = null;
                        }
                    }
                    j = i;
                    continue;
                }
                j = i;
            }

            Vector2 farVectorPoint = new Vector2(-lineVector.x, -lineVector.y) * 10000;

            Vector2 lineStartPoint;
            Vector2 lineEndPoint;

            if (Vector2.Distance(point1.Value, farVectorPoint) < Vector2.Distance(point2.Value, farVectorPoint))
            {
                lineStartPoint = point1.Value;
                lineEndPoint = point2.Value;
            }
            else
            {
                lineStartPoint = point2.Value;
                lineEndPoint = point1.Value;
            }
            float lineLength = Vector2.Distance(lineStartPoint, lineEndPoint);
            line = new Line(line.Value.Type, line.Value.Slope, line.Value.Intercept, lineLength, lineStartPoint, lineEndPoint);
            DrawLine(line.Value, Color.blue, useAreaPopPos);
            perpendicularLine = GetPerpendicularLine(line.Value);
            return true;
        }


        public Line? GetLineFromTwoPoints(Vector2 startPoint, Vector2 endPoint)
        {
            Vector2 vector = endPoint - startPoint;

            LineType lineType;
            float lineSlope;
            float lineIntercept;
            if (vector == Vector2.zero)
            {
                return null;
            }
            else if (vector.y == 0)
            {
                lineType = LineType.Horizontal;
                lineSlope = 0;
                lineIntercept = startPoint.y;
            }
            else if (vector.x == 0)
            {
                lineType = LineType.Vertical;
                if (vector.y > 0)
                {
                    lineSlope = 999999999;
                }
                else
                {
                    lineSlope = -999999999;
                }
                lineIntercept = startPoint.x;
            }
            else
            {
                lineType = LineType.Diagonal;
                lineSlope = vector.y / vector.x;
                lineIntercept = startPoint.y - (lineSlope * startPoint.x);
            }

            Line line = new Line(lineType, lineSlope, lineIntercept, startPoint, endPoint);
            return line;
        }

        public Line? GetLineFromVectorAndPoint(Vector2 vector, Vector2 point)
        {
            LineType lineType;
            float lineSlope;
            float lineIntercept;
            if (vector == Vector2.zero)
            {
                return null;
            }
            else if (vector.y == 0)
            {
                lineType = LineType.Horizontal;
                lineSlope = 0;
                lineIntercept = point.y;
            }
            else if (vector.x == 0)
            {
                lineType = LineType.Vertical;
                if (vector.y > 0)
                {
                    lineSlope = 999999999;
                }
                else
                {
                    lineSlope = -999999999;
                }
                lineIntercept = point.x;
            }
            else
            {
                lineType = LineType.Diagonal;
                lineSlope = vector.y / vector.x;
                lineIntercept = point.y - (lineSlope * point.x);
            }
            Line line = new Line(lineType, lineSlope, lineIntercept);
            return line;
        }


        public Line GetPerpendicularLine(Line line)
        {
            LineType perpendicularLineType;
            float perpendicularLineSlope;
            float perpendicularLineIntercept;
            switch (line.Type)
            {
                case LineType.Horizontal:
                    perpendicularLineType = LineType.Vertical;
                    perpendicularLineSlope = 999999999;
                    perpendicularLineIntercept = areaCentroid.x;
                    break;
                case LineType.Vertical:
                    perpendicularLineType = LineType.Horizontal;
                    perpendicularLineSlope = 0;
                    perpendicularLineIntercept = areaCentroid.y;
                    break;
                default:
                    perpendicularLineType = LineType.Diagonal;
                    perpendicularLineSlope = -1 / line.Slope;
                    perpendicularLineIntercept = areaCentroid.y - (perpendicularLineSlope * areaCentroid.x);
                    break;
            }
            Line perpendicularLine = new Line(perpendicularLineType, perpendicularLineSlope, perpendicularLineIntercept);

            return perpendicularLine;
        }

        public Vector2? FindIntersection(Line line1, Line line2)
        {
            float y = 0;
            float x = 0;
            Vector2? intersection = null;

            if (line1.Type == LineType.Vertical && line2.Type != LineType.Vertical)
            {

                y = line2.Slope * line1.Intercept + line2.Intercept;
                intersection = new Vector2(line1.Intercept, y);

            }
            else if (line1.Type != LineType.Vertical && line2.Type == LineType.Vertical)
            {
                y = line1.Slope * line2.Intercept + line1.Intercept;
                intersection = new Vector2(line2.Intercept, y);
            }
            else if (line1.Type != LineType.Vertical && line2.Type != LineType.Vertical)
            {
                if (line1.Slope == line2.Slope)
                {
                    return null; // Parallel lines don't intersect
                }

                x = (line2.Intercept - line1.Intercept) / (line1.Slope - line2.Slope);
                y = line1.Slope * x + line1.Intercept;
                intersection = new Vector2(x, y);
            }

            // Check if the intersection is within line2's segment
            if (intersection.HasValue)
            {
                Vector2 point = intersection.Value;
                if (IsPointInLineSegment(point, line2.StartPoint, line2.EndPoint))
                {
                    return point;
                }
            }

            return null;

        }

        private static bool IsPointInLineSegment(Vector2 point, Vector2 start, Vector2 end)
        {
            return (point.x >= Mathf.Min(start.x, end.x) && point.x <= Mathf.Max(start.x, end.x) &&
                    point.y >= Mathf.Min(start.y, end.y) && point.y <= Mathf.Max(start.y, end.y));
        }

        public void DrawLine(Line line, Color color, bool useAreaPopPos = true)
        {
            Vector2 point1V2 = GetPointAlongLine(line, 0, line.StartPoint);
            Vector3 point1 = new Vector3(point1V2.x, 0, point1V2.y);
            if (useAreaPopPos)
            {
                point1 = areaPopulatorTransform.position + new Vector3(point1V2.x, 0, point1V2.y);
            }



            Vector2 point2V2 = GetPointAlongLine(line, 50, line.StartPoint);
            Vector3 point2 = new Vector3(point2V2.x, 0, point2V2.y);
            if (useAreaPopPos)
            {
                point2 = areaPopulatorTransform.position + new Vector3(point2V2.x, 0, point2V2.y);
            }

        }
        public Vector2 GetPointAlongLine(Line lineToUse, float distance, Vector2? startPoint = null, bool invert = false)
        {
            // Validate inputs to prevent NaN results
            if (float.IsNaN(distance) || float.IsInfinity(distance))
            {
                Debug.LogWarning("Invalid distance provided to GetPointAlongLine, using 0");
                distance = 0f;
            }
            
            // Check if the line vector is valid
            if (float.IsNaN(lineToUse.Vector.x) || float.IsNaN(lineToUse.Vector.y) ||
                float.IsInfinity(lineToUse.Vector.x) || float.IsInfinity(lineToUse.Vector.y))
            {
                Debug.LogError("Invalid line vector detected in GetPointAlongLine");
                return startPoint.HasValue ? startPoint.Value : Vector2.zero;
            }
            
            // Scale line's unit vector by the desired distance
            Vector2 displacement = lineToUse.Vector * distance;
            
            // Add displacement to the start point
            Vector2 point;
            
            if (invert)
            {
                point = new Vector2(-displacement.x, -displacement.y);
            }
            else
            {
                point = displacement;
            }
            
            if (startPoint.HasValue)
            {
                if (invert)
                {
                    point = startPoint.Value - displacement;
                }
                else
                {
                    point = startPoint.Value + displacement;
                }
            }
            
            // Final validation to ensure we don't return NaN
            if (float.IsNaN(point.x) || float.IsNaN(point.y) ||
                float.IsInfinity(point.x) || float.IsInfinity(point.y))
            {
                Debug.LogError("NaN or Infinity detected in calculated point");
                return startPoint.HasValue ? startPoint.Value : Vector2.zero;
            }
            
            return point;
        }

        public Vector2 GetPlacementPos(int currentMainSpawn, int currentSecondarySpawn)
        {
            // Validate that line has a value before proceeding
            if (!line.HasValue)
            {
                Debug.LogError("Line is null in GetPlacementPos");
                return Vector2.zero;
            }

            // Ensure mainAxisCount is valid to prevent division by zero
            if (mainAxisCount <= 0)
            {
                mainAxisCount = 1;
            }

            // Calculate the main axis distance with validation
            float mainAxisDistance = GetMainAxisDistance(currentMainSpawn, mainAxisCount, line.Value.Length);
            if (float.IsNaN(mainAxisDistance) || float.IsInfinity(mainAxisDistance))
            {
                Debug.LogError($"Invalid main axis distance: {mainAxisDistance}");
                mainAxisDistance = 0;
            }

            // Get the initial position along the main line
            Vector2 initalPos = GetPointAlongLine(line.Value, mainAxisDistance, line.Value.StartPoint);
            Vector2 displacement = Vector2.zero;

            // Calculate displacement along secondary axis if needed
            if (tileAlongSecondaryAxis && secondaryAxisCount > 0)
            {
                float axisDistance = GetPerpendicularAxisDistance(currentSecondarySpawn);
                if (float.IsNaN(axisDistance) || float.IsInfinity(axisDistance))
                {
                    Debug.LogError($"Invalid perpendicular axis distance: {axisDistance}");
                    axisDistance = 0;
                }

                if (currentSecondarySpawn % 2 != 0)
                {
                    displacement = GetPointAlongLine(perpendicularLine, axisDistance, null, true);
                }
                else
                {
                    displacement = GetPointAlongLine(perpendicularLine, axisDistance);
                }
            }
            
            Vector2 objectPosition = initalPos + displacement;
            
            // Final validation to ensure we never return NaN
            if (float.IsNaN(objectPosition.x) || float.IsNaN(objectPosition.y) ||
                float.IsInfinity(objectPosition.x) || float.IsInfinity(objectPosition.y))
            {
                Debug.LogError($"NaN or Infinity detected in final position: {objectPosition}");
                return Vector2.zero;
            }
            
            return objectPosition;
        }
        public float GetMainAxisDistance(int currentSpawn, int totalSpawns, float totalAxisLength)
        {
            // Validate inputs to prevent NaN results
            if (totalSpawns <= 0)
            {
                totalSpawns = 1;
                Debug.LogWarning("Total spawns adjusted to 1 to prevent division by zero");
            }
            
            if (float.IsNaN(totalAxisLength) || float.IsInfinity(totalAxisLength))
            {
                totalAxisLength = 1f;
                Debug.LogWarning("Invalid total axis length detected, using default value");
            }
            
            float resultDistance = firstStepDistance.x + (tilingDistance.x * currentSpawn);

            if (centerTileing)
            {
                // Prevent division by zero
                if (totalSpawns == 1)
                {
                    resultDistance = firstStepDistance.x;
                }
                else
                {
                    float centeredTileingDistance = (totalAxisLength - firstStepDistance.x) / (totalSpawns - 1);
                    
                    // Validate the calculated distance
                    if (float.IsNaN(centeredTileingDistance) || float.IsInfinity(centeredTileingDistance))
                    {
                        Debug.LogWarning("Invalid centered tiling distance calculated, using default");
                        centeredTileingDistance = tilingDistance.x;
                    }
                    
                    resultDistance = firstStepDistance.x + (centeredTileingDistance * currentSpawn);
                }
            }
            
            // Validate the result before applying adjustments
            if (float.IsNaN(resultDistance) || float.IsInfinity(resultDistance))
            {
                Debug.LogWarning("Invalid result distance calculated, using default");
                resultDistance = 0.001f;
            }
            
            // These are here to fix floating point errors
            if (resultDistance > 0.001f)
            {
                resultDistance -= 0.001f;
            }
            if (resultDistance <= 0)
            {
                resultDistance = 0.001f;
            }

            return resultDistance;
        }

        /// <summary>
        /// Gets the distance from the main axis the object should be placed
        /// CURRENTLY DOES NOT SUPPORT CENTERING ALONG SECONDARY AXIS AS WE DONT KNOW HOW LONG THAT LINE IS
        /// </summary>
        /// <param name="currentSpawn">Current spawn index along the secondary axis</param>
        /// <returns>Distance from the main axis</returns>
        public float GetPerpendicularAxisDistance(int currentSpawn)
        {
            // Validate input to prevent issues
            if (currentSpawn < 0)
            {
                Debug.LogWarning("Negative spawn count detected in GetPerpendicularAxisDistance, using 0");
                currentSpawn = 0;
            }

            // Calculate the divided spawn count
            float dividedSpawnCount = (float)currentSpawn / 2;

            // Was some weirdness here where round to int was not turning 0.5f into 1 so had to change it to ceil
            int currentXLevel = Mathf.CeilToInt(dividedSpawnCount);

            // Validate the calculated level
            if (currentXLevel < 0)
            {
                Debug.LogWarning("Negative X level calculated, using 0");
                currentXLevel = 0;
            }

            // Calculate the result distance
            float resultDistance = firstStepDistance.y + (tilingDistance.y * currentXLevel);
            
            // Validate the result to ensure we don't return NaN or infinity
            if (float.IsNaN(resultDistance) || float.IsInfinity(resultDistance))
            {
                Debug.LogWarning("Invalid perpendicular axis distance calculated, using default");
                resultDistance = 0.001f;
            }
            
            return resultDistance;
        }

        public float GetPlacementRotation(Vector2 vector)
        {
            Quaternion rotation = Quaternion.LookRotation(new Vector3(vector.x, 0, vector.y), Vector3.up);
            return rotation.eulerAngles.y;
        }

    }

    public enum LineType
    {
        Horizontal,
        Vertical,
        Diagonal
    }

    public struct Line
    {
        public LineType Type;
        public float Slope;
        public float Intercept; // X intercept for Vertical, Y intercept for Horizontal and Diagonal
        public float Length;
        public Vector2 StartPoint;
        public Vector2 EndPoint;
        public Vector2 Vector;
        public Line(LineType type, float slope, float intercept)
        {
            Type = type;
            Slope = slope;
            Intercept = intercept;
            Length = 0;
            StartPoint = Vector2.zero;
            EndPoint = Vector2.zero;
            Vector2 vector;

            switch (type)
            {
                case LineType.Horizontal:
                    vector = new Vector2(1, 0); // Right direction
                    break;

                case LineType.Vertical:
                    vector = new Vector2(0, 1); // Up direction
                    break;

                default:
                    vector = new Vector2(1, slope); // Direction based on slope
                                                    // Normalize to make it a unit vector
                    break;
            }
            Vector = vector.normalized;
        }
        public Line(LineType type, float slope, float intercept, Vector2 startPoint, Vector2 endPoint)
        {
            Type = type;
            Slope = slope;
            Intercept = intercept;
            Length = Vector2.Distance(startPoint, endPoint);
            StartPoint = startPoint;
            EndPoint = endPoint;
            Vector2 vector = endPoint - startPoint;
            Vector = vector.normalized;
        }
        public Line(LineType type, float slope, float intercept, float length, Vector2 startPoint, Vector2 endPoint)
        {
            Type = type;
            Slope = slope;
            Intercept = intercept;
            Length = length;
            StartPoint = startPoint;
            EndPoint = endPoint;
            Vector2 vector = endPoint - startPoint;
            Vector = vector.normalized;


        }
    }

}




