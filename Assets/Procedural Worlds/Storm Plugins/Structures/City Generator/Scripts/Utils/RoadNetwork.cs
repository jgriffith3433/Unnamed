using System.Collections.Generic;
using UnityEngine;

namespace ProceduralWorlds.Storm.CityBuilder
{
    public class RoadNetwork : MonoBehaviour
    {
        public RoadNetworkData roadNetworkData;

        public List<Vector3> allRoadPoints = new List<Vector3>();

        public int selectedRoadPointIndex = 0;

        public bool drawNodes = true;

        public float MaxDrawDistance = 700f;

        public float roadNodeSizeMultiplier = 1f;

        public void AddRoadPoint(Vector3 position, RoadType roadType)
        {
            if(roadNetworkData.majorRoadPoints.ContainsKey(position) || roadNetworkData.minorRoadPoints.ContainsKey(position))
            {
                //Debug.LogError("Road point already exists at position " + position);
                return;
            }

            RoadPointInformation newRoadPoint = new RoadPointInformation(position, roadType, 0);
            if(roadType == RoadType.Major)
            {

                roadNetworkData.majorRoadPoints.Add(position, newRoadPoint);
            }
            else
            {
                roadNetworkData.minorRoadPoints.Add(position, newRoadPoint);
            }

            GetAllRoadPoints();

            SelectNodeByPosition(position);
        }

        public void RemoveRoadPoint(int selectedIndex)
        {
            if (selectedIndex < 0 || selectedIndex >= allRoadPoints.Count)
            {
                return;
            }
            Vector3 pointPosition = allRoadPoints[selectedIndex];
            Dictionary<Vector3, RoadPointInformation> sourceDictionary = roadNetworkData.majorRoadPoints;   
            if(selectedIndex > roadNetworkData.majorRoadPoints.Count)
            {
                sourceDictionary = roadNetworkData.minorRoadPoints;
            }

            RoadPointInformation roadPointInfo = sourceDictionary[pointPosition];

            //iterate backwards through connection list and remove them as we go
            for(int i = roadPointInfo.roadConnections.Count - 1; i >= 0; i--)
            {
                RoadConnection connection = roadPointInfo.roadConnections[i];
                RoadPointInformation connectedRoadPoint = null;
                if(roadNetworkData.majorRoadPoints.ContainsKey(connection.connectionLocation))
                {
                    connectedRoadPoint = roadNetworkData.majorRoadPoints[connection.connectionLocation];
                }
                else
                {
                    connectedRoadPoint = roadNetworkData.minorRoadPoints[connection.connectionLocation];
                }

                CityBuilderUtils.RemoveRoadConnection(ref roadPointInfo, ref connectedRoadPoint);
            }

            sourceDictionary.Remove(pointPosition);
            allRoadPoints.RemoveAt(selectedIndex);
            selectedRoadPointIndex = -1;
        }

        public void RemoveRoadPoint(Vector3 position)
        {
            int selectedIndex = allRoadPoints.IndexOf(position);
            if (selectedIndex < 0 || selectedIndex >= allRoadPoints.Count)
            {
                return;
            }
            Vector3 pointPosition = allRoadPoints[selectedIndex];
            Dictionary<Vector3, RoadPointInformation> sourceDictionary = roadNetworkData.majorRoadPoints;   
            if(selectedIndex > roadNetworkData.majorRoadPoints.Count)
            {
                sourceDictionary = roadNetworkData.minorRoadPoints;
            }

            RoadPointInformation roadPointInfo = sourceDictionary[pointPosition];

            //iterate backwards through connection list and remove them as we go
            for(int i = roadPointInfo.roadConnections.Count - 1; i >= 0; i--)
            {
                RoadConnection connection = roadPointInfo.roadConnections[i];
                RoadPointInformation connectedRoadPoint = null;
                if(roadNetworkData.majorRoadPoints.ContainsKey(connection.connectionLocation))
                {
                    connectedRoadPoint = roadNetworkData.majorRoadPoints[connection.connectionLocation];
                }
                else
                {
                    connectedRoadPoint = roadNetworkData.minorRoadPoints[connection.connectionLocation];
                }

                CityBuilderUtils.RemoveRoadConnection(ref roadPointInfo, ref connectedRoadPoint);
            }

            sourceDictionary.Remove(pointPosition);
            allRoadPoints.RemoveAt(selectedIndex);
            selectedRoadPointIndex = -1;
        }
        public void UpdateRoadPoint(int selectedIndex, Vector3 newPosition, bool snapToGround)
        {
            if (selectedIndex < 0 || selectedIndex >= allRoadPoints.Count)
            {
                return;
            }
            if (snapToGround)
            {
                float rayStartHeight = 10000f; // Start from high above
                float rayLength = 20000f; // Total ray length to check far below as well
                Ray ray = new Ray(new Vector3(newPosition.x, rayStartHeight, newPosition.z), Vector3.down);
                if (Physics.Raycast(ray, out RaycastHit hit, rayLength))
                {
                    newPosition.y = hit.point.y;
                }
                else
                {
                    newPosition.y = 0f;
                }
            }

            Vector3 oldPosition = allRoadPoints[selectedIndex];
            Dictionary<Vector3, RoadPointInformation> sourceDictionary = roadNetworkData.majorRoadPoints;   
            if(selectedIndex > roadNetworkData.majorRoadPoints.Count)
            {
                sourceDictionary = roadNetworkData.minorRoadPoints;
            }

            RoadPointInformation roadPointInfo = sourceDictionary[oldPosition];
            //set non-connection properties
            roadPointInfo.pointLocation = newPosition;
            roadPointInfo.currentAngle = 0;

            foreach(RoadConnection connection in roadPointInfo.roadConnections)
            {
                RoadPointInformation connectedRoadPoint = null;
                if(roadNetworkData.majorRoadPoints.ContainsKey(connection.connectionLocation))
                {
                    connectedRoadPoint = roadNetworkData.majorRoadPoints[connection.connectionLocation];
                }
                else
                {
                    connectedRoadPoint = roadNetworkData.minorRoadPoints[connection.connectionLocation];
                }
                RoadConnection siblingConnection = connectedRoadPoint.roadConnections.Find(x => x.connectionLocation == oldPosition);
                Vector3 originPos = newPosition;
                Vector3 connectionPos = connectedRoadPoint.pointLocation;
                float originAngle = connection.connectionAngle;
                if(siblingConnection.exitingConnection == true)
                {
                    originPos = connectedRoadPoint.pointLocation;
                    connectionPos = newPosition;
                    originAngle = siblingConnection.connectionAngle;
                }



                float angleChange = CityBuilderUtils.ClampToRad(CityBuilderUtils.CalcuateAngleBetweenPoints(originPos, connectionPos) - originAngle);
                //this might lead to incorrect angles because im not passing in the origin angle always(can fix this by checking if this is an exit connection)
                connection.UpdateConnection(newPosition, connection.connectionLocation, originAngle, angleChange, connection.exitingConnection, connection.isBridge, connectedRoadPoint.roadType);
                siblingConnection.UpdateConnection(connection.connectionLocation, newPosition, originAngle, angleChange, siblingConnection.exitingConnection, siblingConnection.isBridge, roadPointInfo.roadType);
            }

            sourceDictionary.Remove(oldPosition);
            sourceDictionary.Add(newPosition, roadPointInfo);

            allRoadPoints[selectedIndex] = newPosition;
        }  

        public void CreateConnectionToPoint(int selectedIndex, Vector3 connectionPosition)
        {
            if (selectedIndex < 0 || selectedIndex >= allRoadPoints.Count)
            {
                return;
            }
            Debug.Log("Creating connection to node");
            Vector3 sourcePosition = allRoadPoints[selectedIndex];
            Dictionary<Vector3, RoadPointInformation> sourceDictionary = roadNetworkData.majorRoadPoints;   
            if(selectedIndex > roadNetworkData.majorRoadPoints.Count)
            {
                sourceDictionary = roadNetworkData.minorRoadPoints;
            }

            RoadPointInformation sourceRoadPoint = sourceDictionary[sourcePosition];

            RoadPointInformation connectedRoadPoint = null;
            if(roadNetworkData.majorRoadPoints.ContainsKey(connectionPosition))
            {
                connectedRoadPoint = roadNetworkData.majorRoadPoints[connectionPosition];
            }
            else
            {
                connectedRoadPoint = roadNetworkData.minorRoadPoints[connectionPosition];
            }

            CityBuilderUtils.AddNewRoadConnection(ref sourceRoadPoint, ref connectedRoadPoint, false);
        }

        public void RemoveConnectionFromPoint(int selectedIndex, Vector3 connectionPosition)
        {
            if (selectedIndex < 0 || selectedIndex >= allRoadPoints.Count)
            {
                return;
            }
            Debug.Log("Removing connection from node");
            Vector3 sourcePosition = allRoadPoints[selectedIndex];
            Dictionary<Vector3, RoadPointInformation> sourceDictionary = roadNetworkData.majorRoadPoints;   
            if(selectedIndex > roadNetworkData.majorRoadPoints.Count)
            {
                sourceDictionary = roadNetworkData.minorRoadPoints;
            }

            RoadPointInformation sourceRoadPoint = sourceDictionary[sourcePosition];

            RoadPointInformation connectedRoadPoint = null;
            if(roadNetworkData.majorRoadPoints.ContainsKey(connectionPosition))
            {
                connectedRoadPoint = roadNetworkData.majorRoadPoints[connectionPosition];
            }
            else
            {
                connectedRoadPoint = roadNetworkData.minorRoadPoints[connectionPosition];
            }

            CityBuilderUtils.RemoveRoadConnection(ref sourceRoadPoint, ref connectedRoadPoint);
        }

        /// <summary>
        /// Check that the position is on screen and within a certain distance
        /// </summary>
        /// <param name="position"></param>
        /// <returns>True if on screen and within range</returns>
        public bool IsOnScreen(Vector3 position)
        {
            Vector3 onScreen = Camera.current.WorldToViewportPoint(position);
            return onScreen.z > 0f && onScreen.x > 0f &&
                   onScreen.y > 0f && onScreen.x < 1f &&
                   onScreen.y < 1f && onScreen.z < MaxDrawDistance;
        }

        /// <summary>
        /// Select a node by its index
        /// </summary>
        /// <param name="index"></param>
        public void SelectNodeByIndex(int index)
        {
            if (index < 0 || index >= allRoadPoints.Count)
            {
                //Signal error
                Debug.LogWarning("Node index out of bounds:" + index);
                return;
            }
            //Select the new node index
            selectedRoadPointIndex = index;
        }

        public void SelectNodeByPosition(Vector3 position)
        {
            int index = allRoadPoints.IndexOf(position);
            if(index == -1)
            {
                //Signal error
                Debug.LogWarning("Node position not found:" + position);
            }
            SelectNodeByIndex(index);
        }
    
        public void GetAllRoadPoints()
        {
            if(roadNetworkData == null)
            {
                return;
            }
            allRoadPoints.Clear();

            allRoadPoints = new List<Vector3>(roadNetworkData.majorRoadPoints.Keys);

            foreach(Vector3 index in roadNetworkData.minorRoadPoints.Keys)
            {
                allRoadPoints.Add(index);
            }
        }
    
        public void OnDrawGizmos()
        {
            if (roadNetworkData == null)
            {
                return;
            }

            foreach (RoadPointInformation roadPoint in roadNetworkData.majorRoadPoints.Values)
            {
                Gizmos.DrawSphere(roadPoint.pointLocation, 0.5f);
                for (int k = 0; k < roadPoint.roadConnections.Count; k++)
                {
                    if (roadPoint.roadConnections[k].exitingConnection == true)
                    {
                        if (roadPoint.roadConnections[k].connectionType == RoadType.Major)
                        {
                            Gizmos.color = Color.red;
                        }
                        else if (roadPoint.roadConnections[k].connectionType == RoadType.Minor)
                        {
                            Gizmos.color = Color.blue;
                        }

                        Gizmos.DrawLine(roadPoint.pointLocation, roadPoint.roadConnections[k].connectionLocation);
                    }
                }
            }
            foreach (RoadPointInformation roadPoint in roadNetworkData.minorRoadPoints.Values)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawSphere(roadPoint.pointLocation, 0.5f);
                for (int k = 0; k < roadPoint.roadConnections.Count; k++)
                {
                    if (roadPoint.roadConnections[k].exitingConnection == true)
                    {
                        if (roadPoint.roadConnections[k].connectionType == RoadType.Major)
                        {
                            Gizmos.color = Color.red;
                            Gizmos.DrawLine(roadPoint.pointLocation, roadPoint.roadConnections[k].connectionLocation);
                        }
                        else if (roadPoint.roadConnections[k].connectionType == RoadType.Minor)
                        {
                            Gizmos.color = Color.blue;
                            Gizmos.DrawLine(roadPoint.pointLocation, roadPoint.roadConnections[k].connectionLocation);
                        }
                    }
                }
            }
        }
    }
}
