using System.Collections.Generic;

using UnityEngine;
using Game.Utils.Triangulation;
using Game.Utils.Math;
using ProceduralWorlds.Storm.StructureGenCore;
namespace ProceduralWorlds.Storm.DungeonGen
{
    public class DungeonLevel
    {
        //world space y value of the levels FLOOR
        public float yLevel;
        public int levelNumber;
        public Dictionary<Vector2, GridCell> cellDictionary;
        public GridCellMapData gridCellMapData = new GridCellMapData();
        public List<DungeonChamberStory> roomsOnLevel = new List<DungeonChamberStory>();
        public Dictionary<Vector2Int, DungeonChamberStory> chambersOnLevelDictionary = new Dictionary<Vector2Int, DungeonChamberStory>();
        public List<Vector2Int> chamberCenters = new List<Vector2Int>();
        public List<Vector2Int> availibleCells = new List<Vector2Int>();
        public Bounds bounds = new Bounds();

        public List<Hallway> hallways = new List<Hallway>();

        public List<Edge> chamberConnections = new List<Edge>();
        public bool useAStarHalls;
        public List<Edge> GetChamberConnections(float loopingChance, XorshiftPlus randomGen)
        {
            DelaunayTriangulation triangulator = new DelaunayTriangulation();
            DelaunayMST mstGen;
            List<Vector2> points = new List<Vector2>();
            for (int i = 0; i < roomsOnLevel.Count; i++)
            {
                if (!roomsOnLevel[i].connectToNetwork)
                {
                    continue;
                }
                if (roomsOnLevel[i].exitCellsGlobal.Count > 0)
                {
                    for (int k = 0; k < roomsOnLevel[i].exitCellsGlobal.Count; k++)
                    {
                        if (!points.Contains(roomsOnLevel[i].exitCellsGlobal[k].cellPos))
                        {
                            points.Add(roomsOnLevel[i].exitCellsGlobal[k].cellPos);
                        }
                        else
                        {
                            Debug.LogWarning("Point is already in list");
                        }

                    }
                }
                else
                {
                    if (!points.Contains(roomsOnLevel[i].chamberCenter))
                    {
                        points.Add(roomsOnLevel[i].chamberCenter);
                    }
                    else
                    {
                        Debug.LogWarning("Point is already in list");
                    }

                }

            }

            List<Triangle2D> m_outputTriangles = new List<Triangle2D>();
        
            List<Edge> outputEdges = new List<Edge>();
            if (points.Count == 2)
            {
                outputEdges.Add(new Edge(points[0], points[1]));
                chamberConnections = outputEdges;
                return outputEdges;
            }
            else if (points.Count < 3)
            {
                Debug.LogError("not enough points to triangulate");
                return null;
            }
            List<Vector2> outputPoints = new List<Vector2>();
            List<Vector2Int> vectors = new List<Vector2Int>();
            List<Edge> MSTEdges = new List<Edge>();
            List<Edge> loopingEdges = new List<Edge>();
            triangulator.Triangulate(points);
            triangulator.GetTrianglesDiscardingHoles(m_outputTriangles);

            for (int i = 0; i < m_outputTriangles.Count; i++)
            {
                Vector2 p1 = m_outputTriangles[i].p0;
                Vector2 p2 = m_outputTriangles[i].p1;
                Vector2 p3 = m_outputTriangles[i].p2;
                if (!outputPoints.Contains(p1))
                {
                    outputPoints.Add(p1);
                }
                if (!outputPoints.Contains(p2))
                {
                    outputPoints.Add(p2);
                }
                if (!outputPoints.Contains(p3))
                {
                    outputPoints.Add(p3);
                }
                bool containsEdge1 = false;
                bool containsEdge2 = false;
                bool containsEdge3 = false;
                for (int k = 0; k < outputEdges.Count; k++)
                {
                    Edge edge = outputEdges[k];
                    if ((edge.p1 == p1 || edge.p2 == p1) && (edge.p1 == p2 || edge.p2 == p2))
                    {
                        containsEdge1 = true;
                    }
                    if ((edge.p1 == p2 || edge.p2 == p2) && (edge.p1 == p3 || edge.p2 == p3))
                    {
                        containsEdge2 = true;
                    }
                    if ((edge.p1 == p3 || edge.p2 == p3) && (edge.p1 == p1 || edge.p2 == p1))
                    {
                        containsEdge3 = true;
                    }
                }
                if (containsEdge1 == false)
                {
                    Edge newEdge = new Edge(p1, p2);
                    if(randomGen.Next() < loopingChance)
                    {
                        loopingEdges.Add(newEdge);
                        outputEdges.Add(newEdge);
                    }
                    else
                    {
                        outputEdges.Add(newEdge);
                    }
                    
                }
                if (containsEdge2 == false)
                {
                    Edge newEdge = new Edge(p2, p3);
                    if (randomGen.Next() < loopingChance)
                    {
                        loopingEdges.Add(newEdge);
                        outputEdges.Add(newEdge);
                    }
                    else
                    {
                        outputEdges.Add(newEdge);
                    }
                }
                if (containsEdge3 == false)
                {
                    Edge newEdge = new Edge(p3, p1);
                    if (randomGen.Next() < loopingChance)
                    {
                        loopingEdges.Add(newEdge);
                        outputEdges.Add(newEdge);
                    }
                    else
                    {
                        outputEdges.Add(newEdge);
                    }
                }
            }
            mstGen = new DelaunayMST(outputPoints, outputEdges);
            MSTEdges = mstGen.GenerateMST();
            
            for (int i = 0; i < loopingEdges.Count; i++)
            {
                if (!MSTEdges.Contains(loopingEdges[i]))
                {
                    MSTEdges.Add(loopingEdges[i]);
                }
            }
            //iterating backwards through this
            for (int j = MSTEdges.Count-1; j < 0; j--)
            {
                Edge edge = MSTEdges[j];
                for (int i = 0; i < roomsOnLevel.Count; i++)
                {
                    int roomExitsInEdgeCount = 0;
                    for (int k = 0; k < roomsOnLevel[i].exitCellsGlobal.Count; k++)
                    {
                        //these point comparisons may need to be ~ the same instead of ==
                        if (roomsOnLevel[i].exitCellsGlobal[k].cellPos == edge.p1 || roomsOnLevel[i].exitCellsGlobal[k].cellPos == edge.p2)
                        {
                            roomExitsInEdgeCount++;
                        }
                    }
                    if (roomExitsInEdgeCount == 2)
                    {
                        MSTEdges.Remove(edge);
                    }
                }
            }


            chamberConnections = MSTEdges;
            
            return MSTEdges;
        }
        public void GenerateHallwaysBetweenRooms(List<Edge> edges)
        { 
            if(edges.Count <= 0)
            {
                Debug.LogError("no edges to generate hallways between");
                return;
            }
            for (int i = 0; i < edges.Count; i++)
            {
                Vector2Int p1 = new Vector2Int(Mathf.RoundToInt(edges[i].p1.x), Mathf.RoundToInt(edges[i].p1.y));
                Vector2Int p2 = new Vector2Int(Mathf.RoundToInt(edges[i].p2.x), Mathf.RoundToInt(edges[i].p2.y));
                Hallway newHallway = DungeonGeneratorUtils.CreateNewHallwayBetweenTwoPoints(p1, p2, 1, gridCellMapData, useAStarHalls, hallways);
                if(newHallway != null)
                {
                    hallways.Add(newHallway);
                }

            }
        }

        public void CreateRoomsOnLevel(ref DungeonLayer dungeonLayer, ref int chamberCountNumber, XorshiftPlus randomGen, Transform parent = null, bool mirror = false)
        {
            bool connectToNextLv = true;
            int chamberCount = dungeonLayer.chamberCount - roomsOnLevel.Count;
            if (mirror)
            {
                chamberCount = Mathf.FloorToInt(chamberCount);
            }
            for (int i = 0; i < chamberCount; i++)
            {
                DungeonChamber newChamber = DungeonGeneratorUtils.CreateNewChamber(ref dungeonLayer, levelNumber, chamberCountNumber, randomGen, connectToNextLv, null, null, mirror);
                connectToNextLv = false;
                chamberCountNumber++;
            }
        }
    }
}
