using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using MIConvexHull;
using UnityEditor;
using ProceduralWorlds.Storm.BuildingGen;


namespace ProceduralWorlds.Storm.CityBuilder
{

    public static class CityBuilderUtils
    {
        public static float GetHeightAtPointVec3(Vector3 point)
        {
            float height = 0;
            RaycastHit hit;
            if (Physics.Raycast(point + Vector3.up * 3000, Vector3.down, out hit, 5000))
            {
                height = hit.point.y;
                //Debug.DrawLine(point + Vector3.up * 100, hit.point, Color.red, 30f);
            }


            return height;
        }

        public static float GetHeightAtPointVec2(Vector2 point)
        {
            float height = 0;
            RaycastHit hit;
            Vector3 vec3Point = new Vector3(point.x, 0, point.y);
            if (Physics.Raycast(vec3Point + Vector3.up * 3000, Vector3.down, out hit, 5000))
            {
                height = hit.point.y;
                //Debug.DrawLine(point + Vector3.up * 100, hit.point, Color.red, 30f);
            }


            return height;
        }

        public static Vector3 GetHeightVector3AtPoint(Vector3 point)
        {
            RaycastHit hit;
            if (Physics.Raycast(point + Vector3.up * 3000, Vector3.down, out hit, 5000))
            {
                return hit.point;
            }

            return point;
        }

        public static float GetSlopeAtPoint(Vector3 point)
        {
            float slope = 0;
            RaycastHit hit;
            Physics.Raycast(point + Vector3.up * 3000, Vector3.down, out hit, 5000);
            slope = Vector3.Angle(hit.normal, Vector3.up);
            return slope;
        }


        /// <summary>
        /// Gets the adverage postion of the points in the list
        /// </summary>
        /// <param name="points"></param>
        /// <returns></returns>
        public static Vector2 CenterOfPoints(List<Vector3> points)
        {
            float x = 0;
            float z = 0;

            for (int i = 0; i < points.Count; i++)
            {
                x += points[i].x;
                z += points[i].z;

            }
            x = x / points.Count;
            z = z / points.Count;

            return new Vector2(x, z);
        }

                /// <summary>
        /// Gets the adverage postion of the points in the list
        /// </summary>
        /// <param name="points"></param>
        /// <returns></returns>
        public static Vector2 CenterOfPoints(List<Vector2> points)
        {
            float x = 0;
            float y = 0;

            for (int i = 0; i < points.Count; i++)
            {
                x += points[i].x;
                y += points[i].y;

            }
            x = x / points.Count;
            y = y / points.Count;

            return new Vector2(x, y);
        }

        /// <summary>
        /// Clamps rads which may be more than 2pi or less than 0 to be within those numbers while maintaining thier value
        /// </summary>
        /// <param name="unclampedRad">the unclamped rad value</param>
        /// <returns></returns>
        public static float ClampToRad(float unclampedRad)
        {
            if(unclampedRad > Mathf.PI * 2)
            {
                return unclampedRad - (Mathf.PI * 2);
            }
            else if(unclampedRad < 0)
            {
                return unclampedRad + (Mathf.PI * 2);
            }
            return unclampedRad;
        }

        /// <summary>
        /// calcualtes the slope between two points(only uses x and z)
        /// </summary>
        /// <param name="point1"></param>
        /// <param name="point2"></param>
        /// <returns></returns>
        public static float CalculateSlope(Vector3 point1, Vector3 point2)
        {
            float slope = (point2.z - point1.z) / (point2.x - point1.x);
            return slope;
        }

        public static int WeightedChanceSelector(List<float> weights)
        {
            float ratioSum = 0f;

            ratioSum = weights.Sum(x => x);

            System.Random rand = new System.Random();
            double randomNum = rand.NextDouble();
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

        /// <summary>
        /// Calcuates the angle at which a road is entering or exiting a connection
        /// </summary>
        /// <param name="originAngle">the original angle</param>
        /// <param name="angleChange">the angle change</param>
        /// <param name="exitingConnection">if the road is entering or exiting the connection</param>
        /// <returns></returns>
        public static float CalculateRoadConnectionAngle(float originAngle, float angleChange, bool exitingConnection)
        {
            if(exitingConnection == true)
            {
                return ClampToRad(originAngle + angleChange);
            }
            else
            {
                return ClampToRad((originAngle + angleChange) + Mathf.PI);
            }
            
        }

        /// <summary>
        /// calcualtes the angle between two vector3s(only uses x and z
        /// </summary>
        /// <param name="orignAngle">the original point</param>
        /// <param name="connectionAngle">the connection point</param>
        /// <returns></returns>
        public static float CalcuateAngleBetweenPoints(Vector3 orignAngle, Vector3 connectionAngle)
        {
            float x = connectionAngle.x - orignAngle.x;
            float z = connectionAngle.z - orignAngle.z;

            return ClampToRad(Mathf.Atan2(z, x));

            
        }

        /// <summary>
        /// Adds a new mutual road connection between the origin and the connecttion
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="connection"></param>
        /// <param name="angleChange">the angle change from the origins direction</param>
        public static void AddNewRoadConnection(ref RoadPointInformation origin, ref RoadPointInformation connection, float angleChange, bool isBridge)
        {
            if(AreRoadsConnected(origin, connection) == true)
            {
                Debug.LogWarning("These roads are already connected");
                return;
            }
            RoadConnection roadConnectionFromOrigin = new RoadConnection(origin.pointLocation, connection.pointLocation, origin.currentAngle, angleChange, true, isBridge, connection.roadType);
            origin.roadConnections.Add(roadConnectionFromOrigin);

            
            RoadConnection roadConnectionFromConnection = new RoadConnection(connection.pointLocation, origin.pointLocation, origin.currentAngle, angleChange, false, isBridge, origin.roadType);
            connection.roadConnections.Add(roadConnectionFromConnection);
            
        }

        /// <summary>
        /// adds a new mutual road connection between the origin and the connection without knowing the angle change
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="connection"></param>
        public static void AddNewRoadConnection(ref RoadPointInformation origin, ref RoadPointInformation connection, bool isBridge)
        {
            if(AreRoadsConnected(origin, connection) == true)
            {
                Debug.LogWarning("These roads are already connected");
                return;
            }
            float angleChange = ClampToRad(CalcuateAngleBetweenPoints(origin.pointLocation, connection.pointLocation) - origin.currentAngle);
            RoadConnection roadConnectionFromOrigin = new RoadConnection(origin.pointLocation, connection.pointLocation, origin.currentAngle, angleChange, true, isBridge, connection.roadType);
            origin.roadConnections.Add(roadConnectionFromOrigin);


            RoadConnection roadConnectionFromConnection = new RoadConnection(connection.pointLocation, origin.pointLocation, origin.currentAngle, angleChange, false, isBridge, origin.roadType);
            connection.roadConnections.Add(roadConnectionFromConnection);
        }

        public static bool AreRoadsConnected(RoadPointInformation origin, RoadPointInformation connection)
        {
            return origin.roadConnections.Find(x => x.connectionLocation == connection.pointLocation) != null;
        }
        /// <summary>
        /// removes any road connections between the two points
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="connection"></param>
        public static void RemoveRoadConnection(ref RoadPointInformation origin, ref RoadPointInformation connection)
        {
            RoadConnection connectionToRemove = null;
            for (int i = 0; i < origin.roadConnections.Count; i++)
            {
                if(origin.roadConnections[i].connectionLocation == connection.pointLocation)
                {
                    connectionToRemove = origin.roadConnections[i];
                }
            }
            if(connectionToRemove != null)
            {
                origin.roadConnections.Remove(connectionToRemove);
            }
            else
            {
                Debug.LogWarning("there is no connection between these two points");
            }

            RoadConnection connectionToRemove2 = null;
            for (int i = 0; i < connection.roadConnections.Count; i++)
            {
                if (connection.roadConnections[i].connectionLocation == origin.pointLocation)
                {
                    connectionToRemove2 = connection.roadConnections[i];
                }
            }
            if (connectionToRemove2 != null)
            {
                connection.roadConnections.Remove(connectionToRemove2);
            }
            else
            {
                Debug.LogWarning("there is no connection between these two points");
            }
        }

        /// <summary>
        /// gets the grayness of a pxiel in a texture 2d(return value from 0-1
        /// </summary>
        /// <param name="texture">texture to use</param>
        /// <param name="dimensions">demiensions of the area the texture covers(in raidus)</param>
        /// <param name="point">the point to check</param>
        /// <returns></returns>
        public static float GetPixelGrayness(Texture2D texture, float dimensions, Vector2 point)
        {
            float textureWidth = texture.width;
            float textureHight = texture.height;
            float x = textureWidth / 2 + ((textureWidth / dimensions / 2) * point.x);
            float y = textureHight / 2 + ((textureHight / dimensions / 2) * point.y);

            int finalX = Mathf.RoundToInt(x);
            int finalY = Mathf.RoundToInt(y);
            float pixelGreyScale = texture.GetPixel(finalX, finalY).grayscale;
            return pixelGreyScale;

        }
        
        /// <summary>
        /// Gets the centroid of the polygon points inputed
        /// </summary>
        /// <param name="points"></param>
        /// <returns></returns>
        public static Vector3 GetCentroid(List<Vector3> points)
        {
            float x = 0;
            float y = 0;
            float z = 0;
            for (int i = 0; i < points.Count; i++)
            {
                x += points[i].x;
                y += points[i].y;
                z += points[i].z;
            }
            x = x / points.Count;
            y = y / points.Count;
            z = z / points.Count;

            return new Vector3(x, y, z);
        }

        /// <summary>
        /// Gets the centroid of the polygon points inputed
        /// </summary>
        /// <param name="points"></param>
        /// <returns></returns>
        public static Vector2 GetCentroid(List<Vector2> points)
        {
            float x = 0;
            float y = 0;
            for (int i = 0; i < points.Count; i++)
            {
                x += points[i].x;
                y += points[i].y;
            }
            x = x / points.Count;
            y = y / points.Count;

            return new Vector2(x, y);
        }

        /// <summary>
        /// Gets the x,y,z bounds of a list of points
        /// </summary>
        /// <param name="points"></param>
        /// <returns></returns>
        public static Vector3 GetBounds(List<Vector3> points)
        {
            float largestX = int.MinValue;
            float largestY = int.MinValue;
            float largestZ = int.MinValue;

            float smallestX = int.MaxValue;
            float smallestY = int.MaxValue;
            float smallestZ = int.MaxValue;

            for (int i = 0; i < points.Count; i++)
            {
                if (points[i].x > largestX)
                {
                    largestX = points[i].x;
                }
                if (points[i].x < smallestX)
                {
                    smallestX = points[i].x;
                }
                if (points[i].y > largestY)
                {
                    largestY = points[i].y;
                }
                if (points[i].y < smallestY)
                {
                    smallestY = points[i].y;
                }
                if (points[i].z > largestZ)
                {
                    largestZ = points[i].z;
                }
                if (points[i].z < smallestZ)
                {
                    smallestZ = points[i].z;
                }
            }

            return new Vector3(largestX - smallestX, largestY - smallestY, largestZ - smallestZ);
        }

        public static List<Vector3> CreateRadialFormationVector3(Vector3 center, float sectionLength, float radius)
        {
            List<Vector3> pointList = new List<Vector3>();
            int steps = Mathf.RoundToInt(Mathf.PI * 2 * radius / sectionLength);
            if(steps < 3)
            {
                return null;
            }
            float radienStep = (Mathf.PI * 2) / steps;
            float currentRadiens = 0;
            for (int i = 0; i < steps; i++)
            {
                currentRadiens = radienStep * i;
                Vector3 point = new Vector3(Mathf.Cos(currentRadiens) * radius, 0, Mathf.Sin(currentRadiens) * radius);

                pointList.Add(point);
            }
            return pointList;
        }

        public static List<RoadPointInformation> CreateRadialFormationRoadPoint(Vector3 center, float sectionLength, float radius, RoadType roadType, bool flipRoadDirection)
        {
            //this currently doesnt give a shit about local rules, may want to change that in the future, this would require this method to access the water, slope, and height maps
            List<RoadPointInformation> roadPointList = new List<RoadPointInformation>();
            int steps = Mathf.RoundToInt(Mathf.PI * 2 * radius / sectionLength);
            if (steps < 3)
            {
                return null;
            }
            float radienStep = (Mathf.PI * 2) / steps;
            float currentRadiens = 0;
            float roadAngleAddition = 0;
            if (flipRoadDirection)
            {
                roadAngleAddition = Mathf.PI;
            }

            for (int i = 0; i < steps; i++)
            {
                currentRadiens = radienStep * i;
                Vector3 point = new Vector3(Mathf.Cos(currentRadiens) * radius, 0, Mathf.Sin(currentRadiens) * radius);
                RoadPointInformation newRoadPoint = new RoadPointInformation();
                newRoadPoint.pointLocation = point;
                newRoadPoint.roadType = roadType;
                
                newRoadPoint.currentAngle = currentRadiens + roadAngleAddition;
                roadPointList.Add(newRoadPoint);
            }
            int j = roadPointList.Count - 1;
            for (int i = 0; i < roadPointList.Count; i++)
            {
                RoadPointInformation roadPoint1 = roadPointList[j];
                RoadPointInformation roadPoint2 = roadPointList[i];
                AddNewRoadConnection(ref roadPoint1, ref roadPoint2, false);
                roadPointList[j] = roadPoint1;
                roadPointList[i] = roadPoint2;
                j = i;
            }
            return roadPointList;
        }




        /// <summary>
        /// Subdivides polygon recusivly until it falls within the desired size range
        /// </summary>
        /// <param name="polygon"></param>
        /// <param name="maxCellArea"></param>
        /// <param name="minCellArea"></param>
        /// <param name="allowInternalCells"></param>
        /// <param name="random"></param>
        /// <returns></returns>
        public static List<List<Vector3>> SubdividePolygon(List<Vector3> originalPolygon, float maxLotSize, bool allowInternalCells)
        {
            List<List<Vector3>> outputPolygons = new List<List<Vector3>>();

            float originalPolygonArea = GetAreaOfPolygon(originalPolygon);
            if (originalPolygonArea <= maxLotSize)
            {
                outputPolygons.Add(originalPolygon);
                return outputPolygons;
            }


            List<List<Vector3>> lotsOverMaxSize = new List<List<Vector3>>();
            lotsOverMaxSize.Add(originalPolygon);



            int count = 0;
            while(lotsOverMaxSize.Count > 0)
            {
                count++;
                if (count > 10)
                {
                    break;
                }
                
                for (int i = lotsOverMaxSize.Count-1; i >= 0; i--) 
                {
                    List<Vector3> currentLot = lotsOverMaxSize[i];

                    // get bounding box and determine cut points
                    Vector2 cutStartpoint;
                    Vector2 cutEndpoint;
                    GetPolygonCutPoints(currentLot, out cutStartpoint, out cutEndpoint);



                    List<List<Vector3>> childPolygons = new List<List<Vector3>>();

                    //cut
                    CutPolygon(currentLot, cutStartpoint, cutEndpoint, childPolygons);
                    foreach (List<Vector3> childPolygon in childPolygons)
                    {
                        //check children, add them to final list(output polygons) or add them to the lots over max size list,
                        //if allow internal lots is false then check if they share points and remove them totally if not
                        float polygonArea = GetAreaOfPolygon(childPolygon);

                        if (!allowInternalCells)
                        {
                            if(!DoPolygonsSharePoints(childPolygon, originalPolygon))
                            {
                                continue;
                            }
                        }

                        if(polygonArea > maxLotSize)
                        {
                            lotsOverMaxSize.Add(childPolygon);
                        }
                        else
                        {
                            outputPolygons.Add(childPolygon);
                        }
                    }

                    //remove current lot from list
                    lotsOverMaxSize.RemoveAt(i);
                }
            }

            foreach (List<Vector3> childPolygon in outputPolygons)
            {
                for (int i = 0; i < childPolygon.Count; i++)
                {
                    Vector2 point = new Vector2(childPolygon[i].x, childPolygon[i].z);
                    childPolygon[i] = new Vector3(point.x, GetHeightAtPointVec2(point), point.y);
                }
            }
            return outputPolygons;
        }

        private static void GetPolygonCutPoints(List<Vector3> polygon, out Vector2 cutStartpoint, out Vector2 cutEndpoint)
        {
            List<Vector2> boundingBox = GetBoundingBox(polygon);
            Vector2 lengthVector = boundingBox[1] - boundingBox[0];
            float boxLength = lengthVector.magnitude;
            Vector2 widthVector = boundingBox[2] - boundingBox[1];

            float boxWidth = widthVector.magnitude;

            cutStartpoint = Vector2.zero;
            cutEndpoint = Vector2.zero;
            if (boxLength >= boxWidth)
            {
                //cut along width

                cutStartpoint = (boundingBox[0] + boundingBox[1]) / 2;
                cutEndpoint = (boundingBox[3] + boundingBox[2]) / 2;

            }
            else if (boxWidth > boxLength)
            {
                //cut along length
                cutStartpoint = (boundingBox[1] + boundingBox[2]) / 2;
                cutEndpoint = (boundingBox[0] + boundingBox[3]) / 2;
            }
        }

        /// <summary>
        /// Gets the area of a polygon
        /// </summary>
        /// <param name="polygon"></param>
        /// <returns></returns>
        public static float GetAreaOfPolygon(List<Vector2> polygon)
        {
            List<Vector2> newPolygon = polygon.ToList();
            if(newPolygon.Count == 0)
            {
                return 0;
            }
            newPolygon.Add(newPolygon[0]);
            float area = Math.Abs(newPolygon.Take(newPolygon.Count - 1).Select((p, i) => (newPolygon[i + 1].x - p.x) * (newPolygon[i + 1].y + p.y)).Sum() / 2);
            return area;
        }

        public static float GetAreaOfPolygon(List<Vector3> polygon)
        {
            List<Vector3> newPolygon = polygon.ToList();
            if (newPolygon.Count == 0)
            {
                return 0;
            }
            newPolygon.Add(newPolygon[0]);
            float area = Math.Abs(newPolygon.Take(newPolygon.Count - 1).Select((p, i) => (newPolygon[i + 1].x - p.x) * (newPolygon[i + 1].z + p.z)).Sum() / 2);
            return area;
        }

        /// <summary>
        /// cuts a polygon in half along a line represented by two points
        /// </summary>
        /// <param name="polygon"></param>
        /// <param name="cutLinePoint1"></param>
        /// <param name="cutLinePoint2"></param>
        /// <param name="dividedPolygon1"></param>
        /// <param name="dividedPolygon2"></param>
        public static void CutPolygon(List<Vector3> polygon, Vector2 cutStartpoint, Vector2 cutEndpoint, List<List<Vector3>> childPolygons)
        {
            //STILL HAVING PROBLEMS WITH THIS SEED IN THE SMALL CITY 81272941, 110506542
            List<Vector3> dividedPolygon1 = new List<Vector3>();
            List<Vector3> dividedPolygon2 = new List<Vector3>();
            //Debug.DrawLine(new Vector3(cutStartpoint.x, 0, cutStartpoint.y), new Vector3(cutEndpoint.x, 0, cutEndpoint.y));

            // Calculate the slope and intercept of the cut line
            float cutLineSlope = (cutEndpoint.y - cutStartpoint.y) / (cutEndpoint.x - cutStartpoint.x);
            if (Mathf.Abs(cutEndpoint.x - cutStartpoint.x) < 1.5f)
            {
                cutLineSlope = float.MaxValue; // Handle vertical lines directly
            }
            float cutLineIntercept = cutStartpoint.y - (cutLineSlope * cutStartpoint.x);

            List<PolygonPointData> polygonWithIntersections = new List<PolygonPointData>();
            int j = polygon.Count - 1;

            int intersectionCount = 0;
            //bool found = false;
            // Find intersection points with polygon edges
            for (int i = 0; i < polygon.Count; i++)
            {
                PolygonPointData pointData = new PolygonPointData();
                pointData.isIntersection = false;
                pointData.point = polygon[i];

                float segmentSlope = (polygon[i].z - polygon[j].z) / (polygon[i].x - polygon[j].x);
                if (Mathf.Abs(polygon[i].x - polygon[j].x) < 0.1f)
                {
                    segmentSlope = float.MaxValue; // Handle vertical segment
                }
                float segmentIntercept = polygon[j].z - (segmentSlope * polygon[j].x);

                if (cutLineSlope == segmentSlope)
                {
                    // Parallel lines, no intersection
                    polygonWithIntersections.Add(pointData);
                    j = i;
                    continue;
                }

                // Calculate intersection point
                float pointX;
                float pointY;
                if (cutLineSlope == float.MaxValue)
                {
                    // Vertical cut line
                    pointX = cutStartpoint.x;
                    pointY = segmentSlope * pointX + segmentIntercept;
                }
                else if (segmentSlope == float.MaxValue)
                {
                    // Vertical segment
                    pointX = polygon[j].x;
                    pointY = cutLineSlope * pointX + cutLineIntercept;
                }
                else
                {
                    pointX = (segmentIntercept - cutLineIntercept) / (cutLineSlope - segmentSlope);
                    pointY = cutLineSlope * pointX + cutLineIntercept;
                }

                // Check if the intersection point lies within the segment bounds
                if (pointX >= Mathf.Min(polygon[j].x, polygon[i].x) &&
                    pointX <= Mathf.Max(polygon[j].x, polygon[i].x) &&
                    pointY >= Mathf.Min(polygon[j].z, polygon[i].z) &&
                    pointY <= Mathf.Max(polygon[j].z, polygon[i].z))
                {
                    
                    PolygonPointData intersectionPointData = new PolygonPointData();
                    intersectionPointData.point = new Vector3(pointX, 0, pointY);
                    //Debug.Log("Found Intersection At " + new Vector2(pointX, pointY));
                    intersectionPointData.isIntersection = true;
                    intersectionCount++;
                    polygonWithIntersections.Add(intersectionPointData);
                }
               
                polygonWithIntersections.Add(pointData);
                j = i;
            }

            if(intersectionCount < 2)
            {
                //Debug.LogWarning("Interesction count LESS THAN 2");
                //Debug.Log(Mathf.Abs(cutEndpoint.x - cutStartpoint.x));
                //Debug.Log(cutLineSlope);
            }

            // Distribute points into two polygons
            bool sideSwitch = false;
            for (int i = 0; i < polygonWithIntersections.Count; i++)
            {
                if(polygonWithIntersections[i].point.x == Mathf.NegativeInfinity)
                {
                    //Debug.LogWarning("Skipping vertex as it has a negative infinity x value.");
                    continue;
                }
                
                Vector2 currentPoint = polygonWithIntersections[i].point;
                if (polygonWithIntersections[i].isIntersection)
                {
                    
                    // Add intersection point to both polygons
                    dividedPolygon1.Add(polygonWithIntersections[i].point);
                    dividedPolygon2.Add(polygonWithIntersections[i].point);
                    sideSwitch = !sideSwitch; // Switch sides after intersection
                }
                else
                {
                    // Add point to the appropriate polygon based on the current side
                    if (!sideSwitch)
                    {
                        dividedPolygon1.Add(polygonWithIntersections[i].point);
                    }
                    else
                    {
                        dividedPolygon2.Add(polygonWithIntersections[i].point);
                    }
                }
            }

            // Add the divided polygons to the output list
            if (dividedPolygon1.Count >= 3) childPolygons.Add(dividedPolygon1);
            if (dividedPolygon2.Count >= 3) childPolygons.Add(dividedPolygon2);
        }

        /// <summary>
        /// Gets an object orented bounding box
        /// </summary>
        /// <param name="polygonPoints"></param>
        /// <returns></returns>
        public static List<Vector2> GetBoundingBox(List<Vector3> polygonPoints)
        {
            List<Vector2> winningBox = new List<Vector2>();
            float winningBoundingBoxSize = int.MaxValue;

            List<Vector2> convexHull = GetConvexHull(polygonPoints);
            int j = convexHull.Count - 1;
            for (int k = 0; k < convexHull.Count; k++)
            {
                List<Vector2> pointsRelativeToOrigin = new List<Vector2>();
                for (int i = 0; i < convexHull.Count; i++)
                {
                    Vector2 newPoint = new Vector2(convexHull[i].x - convexHull[j].x, convexHull[i].y - convexHull[j].y);
                    pointsRelativeToOrigin.Add(newPoint);

                }

                List<Vector2> boundingBox = new List<Vector2>();

                Vector2 edgeVector = pointsRelativeToOrigin[k];
                float divisionNumber = edgeVector.sqrMagnitude;
                float edgeVectorMagnitude = edgeVector.magnitude;
                float edgeVectorAngle = CalcuateAngleBetweenPoints(Vector3.zero, new Vector3(edgeVector.x, 0, edgeVector.y));

                Vector2 secondaryEdgeVector = Vector2.zero;
                float secondaryEdgeVectorMagnatudie = 0;

                Vector2 orthognalVector = Vector2.zero;
                float orthognalVectorLength = 0;
                
                for (int i = 0; i < pointsRelativeToOrigin.Count; i++)
                {
                    Vector2 pointVector = pointsRelativeToOrigin[i];
                    float pointVectorDotStartingVector = Vector2.Dot(pointVector, edgeVector);
                    Vector2 projectionVector = pointVectorDotStartingVector / divisionNumber * edgeVector;
                    float projectionVectorMagnatuide = projectionVector.magnitude;
                    float projectionVectorAngle = CalcuateAngleBetweenPoints(Vector3.zero, new Vector3(projectionVector.x, 0, projectionVector.y));
                    Vector2 newOrthognalVector = pointVector - projectionVector;
                    float newOrthognalVectorLength = newOrthognalVector.magnitude;
                    float newOrthognalVectorAngle = CalcuateAngleBetweenPoints(Vector3.zero, new Vector3(newOrthognalVector.x, 0, newOrthognalVector.y));
                    if (Mathf.Abs(projectionVectorAngle - edgeVectorAngle) > Mathf.PI - 0.1f && Mathf.Abs(projectionVectorAngle - edgeVectorAngle) < Mathf.PI + 0.1f)
                    {
                        if (projectionVectorMagnatuide > secondaryEdgeVectorMagnatudie)
                        {
                            secondaryEdgeVector = projectionVector;
                            secondaryEdgeVectorMagnatudie = projectionVectorMagnatuide;
                        }
                    }
                    else
                    {
                        if (projectionVectorMagnatuide > edgeVectorMagnitude)
                        {
                            edgeVector = projectionVector;
                            edgeVectorMagnitude = projectionVectorMagnatuide;
                            divisionNumber = edgeVector.sqrMagnitude;
                        }
                    }
                    if (newOrthognalVectorLength > orthognalVectorLength)
                    {
                        orthognalVector = newOrthognalVector;
                        orthognalVectorLength = newOrthognalVectorLength;
                    }
                }

                boundingBox.Add(secondaryEdgeVector);
                boundingBox.Add(orthognalVector + secondaryEdgeVector);
                boundingBox.Add(orthognalVector + edgeVector);
                boundingBox.Add(edgeVector);



                float boundingBoxSize = (edgeVector.magnitude + secondaryEdgeVector.magnitude) * orthognalVector.magnitude;
                
                
                //this is a very temporary patch to this error i am getting, need to examine and fix
                if(boundingBoxSize == 0)
                {
                    continue;
                }
                if (boundingBoxSize < winningBoundingBoxSize)
                {
                    for (int i = 0; i < boundingBox.Count; i++)
                    {
                        boundingBox[i] = new Vector2(boundingBox[i].x + convexHull[j].x, boundingBox[i].y + convexHull[j].y);
                    }
                    winningBoundingBoxSize = boundingBoxSize;
                    winningBox = boundingBox;
                }

                j = k;
            }

            return winningBox;
        }



        public static List<Vector2> GetConvexHull(List<Vector3> polygon)
        {
            List<DefaultVertex2D> concavePolygonList = new List<DefaultVertex2D>();
            for (int i = 0; i < polygon.Count; i++)
            {
                if (polygon[i].x == Mathf.NegativeInfinity || polygon[i].x == Mathf.Infinity || polygon[i].z == Mathf.NegativeInfinity || polygon[i].z == Mathf.Infinity)
                {
                    //Debug.LogWarning("Skipping vertex as it has a negative infinity x value.");
                    continue;
                }
                DefaultVertex2D currentVertex = new DefaultVertex2D(polygon[i].x, polygon[i].z);
                concavePolygonList.Add(currentVertex);
            }
            List<DefaultVertex2D> convexPolygonList = ConvexHull2DAlgorithm.Create(concavePolygonList, 1f);
            List<Vector2> convexPolygonListVector2 = new List<Vector2>();

            for (int i = 0; i < convexPolygonList.Count; i++)
            {
                Vector2 currentPoint = new Vector2((float)convexPolygonList[i].X, (float)convexPolygonList[i].Y);
                convexPolygonListVector2.Add(currentPoint);
            }
            return convexPolygonListVector2;
        }


        /// <summary>
        /// checks if polygons share points
        /// </summary>
        /// <param name="polygon1"></param>
        /// <param name="polygon2"></param>
        /// <returns></returns>
        private static bool DoPolygonsSharePoints(List<Vector3> polygon1, List<Vector3> polygon2)
        {
            int previousIndex1 = polygon1.Count - 1;
            for (int currentIndex1 = 0; currentIndex1 < polygon1.Count; currentIndex1++)
            {
                float slope1 = (polygon1[previousIndex1].z - polygon1[currentIndex1].z) / (polygon1[previousIndex1].x - polygon1[currentIndex1].x);
                float intercept1 = polygon1[previousIndex1].z - (slope1 * polygon1[previousIndex1].x);
                int previousIndex2 = polygon2.Count - 1;
                for (int currentIndex2 = 0; currentIndex2 < polygon2.Count; currentIndex2++)
                {
                    float slope2 = (polygon2[previousIndex2].z - polygon2[currentIndex2].z) / (polygon2[previousIndex2].x - polygon2[currentIndex2].x);
                    float intercept2 = polygon2[previousIndex2].z - (slope1 * polygon2[previousIndex2].x);

                    if (Mathf.Abs(slope1 - slope2) < 0.1f && Mathf.Abs(intercept1 - intercept2) < 0.1f)
                    {
                        return true;
                    }
                    previousIndex2 = currentIndex2;
                }
                previousIndex1 = currentIndex1;
            }
            return false;
        }


       




        public static Vector3 GetLineInteresectionPoint(Vector3 line1Point1, Vector3 line1Point2, Vector3 line2Point1, Vector3 line2Point2)
        {
            float line1Slope = (line1Point2.z - line1Point1.z) / (line1Point2.x-line1Point1.x);
            if (line1Point2.x - line1Point1.x == 0)
            {
                line1Slope = 1000000;
                
            }
            float line1InitalValue = line1Point1.z - (line1Slope * line1Point1.x);

            float line2Slope = (line2Point2.z - line2Point1.z) / (line2Point2.x - line2Point1.x);
            if (line2Point2.x - line2Point1.x == 0)
            {
                line1Slope = 1000000;
            }
            float line2InitalValue = line2Point1.z - (line2Slope * line2Point1.x);

            float pointX = (line1InitalValue - line2InitalValue) / (line1Slope - line2Slope);
            pointX -= pointX * 2;
            if (line1Slope - line2Slope == 0)
            {
                return Vector3.zero;
            }
            float pointZ = line1Slope * pointX + line1InitalValue;


            return new Vector3(pointX, 0, pointZ);
        }
    }

    [Serializable]
    public class DistrictBuildingType
    {
        public BuildingConfig houseInfo;
        public bool generateInteriors;
        public bool decorateInteriors;
        
        public float failChance;
        public float weight;
        public int maxSpawns;
        public int currentSpawns;
    }

    [Serializable]
    public struct RadialRoadPoint
    {
        public Vector3 center;
        public float radius;
    }
    /// <summary>
    /// Stores information about roads
    /// </summary>
    public class RoadPointInformation
    {
        public Vector3 pointLocation;
        public RoadType roadType;
        public float currentAngle;
        public List<RoadConnection> roadConnections = new List<RoadConnection>();

        public RoadPointInformation(Vector3 pointLocation, RoadType roadType, float currentAngle)
        {
            this.pointLocation = pointLocation;
            this.roadType = roadType;
            this.currentAngle = currentAngle;
        }

        public RoadPointInformation()
        {
            
        }
    }

    /// <summary>
    /// stores information about road points
    /// </summary>
    public class IntersectionInformation
    {
        public RoadPointInformation roadPoint;
    } 

    /// <summary>
    /// stores information about roadconnections
    /// </summary>
    public class RoadConnection
    {
        public Vector3 connectionLocation;
        public float connectionDistance;
        public float connectionAngle;
        public RoadType connectionType;
        public bool isBridge;
        public float connectionSlope;
        public bool exitingConnection;

        public RoadConnection(Vector3 startLocation, Vector3 connectionLocation, float originAngle, float angleChange, bool exitingConnection, bool isBridge, RoadType roadType)
        {
            this.connectionLocation = connectionLocation;
            this.connectionDistance = Vector3.Distance(startLocation, connectionLocation);
            this.connectionSlope = CityBuilderUtils.CalculateSlope(startLocation, connectionLocation);
            this.connectionAngle = CityBuilderUtils.CalculateRoadConnectionAngle(originAngle, angleChange, exitingConnection);
            this.exitingConnection = exitingConnection;
            this.connectionType = roadType;
            this.isBridge = isBridge;
        }

        public RoadConnection()
        {

        }

        public void UpdateConnection(Vector3 startLocation, Vector3 connectionLocation, float originAngle, float angleChange, bool exitingConnection, bool isBridge, RoadType roadType)
        {
            this.connectionLocation = connectionLocation;
            this.connectionDistance = Vector3.Distance(startLocation, connectionLocation);
            this.connectionSlope = CityBuilderUtils.CalculateSlope(startLocation, connectionLocation);
            this.connectionAngle = CityBuilderUtils.CalculateRoadConnectionAngle(originAngle, angleChange, exitingConnection);
            this.exitingConnection = exitingConnection;
            this.connectionType = roadType;
            this.isBridge = isBridge;
        }
    }

    /// <summary>
    /// Stores information about point sets for the CreatePlotsAlongLine method
    /// </summary>
    public class PointSet
    {
        public bool deadEnd = false;
        public Vector3 originPoint;
        public Vector3 secondaryPoint;
    }


    public class BlockInfo
    {
        public bool subdivideLots;
        public CityDistrict blockDistrict;
        public List<RoadPointInformation> surroundingRoadPoints;
        public List<Vector3> blockPoints;

        public List<LandPlot> blockLandPlots;

        public BlockInfo(List<RoadPointInformation> surroundingRoadPoints)
        {
            this.surroundingRoadPoints = surroundingRoadPoints;
        }
    }

    public struct LandPlot
    {
        public List<Vector3> lotPoints;
        public float angleToRoad;

        public Vector3 centroid;
        public LandPlot(List<Vector3> lotPoints)
        {
            this.lotPoints = lotPoints;
            this.angleToRoad = 0;
            this.centroid = Vector3.zero;
        }
        public LandPlot(List<Vector3> lotPoints, float angleToRoad, Vector3 centroid)
        {
            this.lotPoints = lotPoints;
            this.angleToRoad = angleToRoad;
            this.centroid = centroid;
        }

    }



    public enum RoadType
    {
        Major,
        Minor
    }

    public enum MinorRoadPlacementType
    {
        Roster,
        Block,
        Radial
    }

    public enum MajorRoadPlacementType
    {
        Roster,
        Radial
    }

    public enum MajorRoadStartType
    {
        Intersection,
        //Radial
    }

    /// <summary>
    /// used in the CutPolygon method to store polygonpoint data
    /// </summary>
    public struct PolygonPointData
    {
        public Vector3 point;
        public bool isIntersection;
    }




}

