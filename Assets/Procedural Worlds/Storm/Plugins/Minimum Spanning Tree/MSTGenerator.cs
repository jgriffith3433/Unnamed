using UnityEngine;
using System.Collections.Generic;


namespace ProceduralWorlds.Storm.DungeonGen
{

    public class DelaunayMST
    {
        private List<Vector2> points;
        private List<Edge> edges;

        // The Delaunay triangulation graph
        private Dictionary<Vector2, List<Edge>> graph;

        // The minimum spanning tree
        private List<Edge> mst;

        public DelaunayMST(List<Vector2> points, List<Edge> edges)
        {
            this.points = points;
            this.edges = edges;
            graph = new Dictionary<Vector2, List<Edge>>();
            mst = new List<Edge>();

            // Create the graph
            foreach (Vector2 point in points)
            {
                graph[point] = new List<Edge>();
            }

            foreach (Edge edge in edges)
            {
                graph[edge.p1].Add(edge);
                graph[edge.p2].Add(edge);
            }
        }

        // Run Prim's algorithm to generate the minimum spanning tree
        public List<Edge> GenerateMST()
        {
            HashSet<Vector2> visited = new HashSet<Vector2>();
            List<Edge> heap = new List<Edge>();

            // Start at any point
            Vector2 start = points[0];
            visited.Add(start);

            // Add all edges from the starting point to the heap
            foreach (Edge edge in graph[start])
            {
                heap.Add(edge);
            }

            while (visited.Count < points.Count)
            {
                // Find the edge with the minimum weight that connects a visited point to an unvisited point
                Edge minEdge = null;
                foreach (Edge edge in heap)
                {
                    if (visited.Contains(edge.p1) != visited.Contains(edge.p2))
                    {
                        if (minEdge == null || edge.weight < minEdge.weight)
                        {
                            minEdge = edge;
                        }
                    }
                }

                // Add the edge to the minimum spanning tree
                mst.Add(minEdge);

                // Add the new point to the visited set
                Vector2 p = visited.Contains(minEdge.p1) ? minEdge.p2 : minEdge.p1;
                visited.Add(p);

                // Add all edges from the new point to the heap
                foreach (Edge edge in graph[p])
                {
                    if (!heap.Contains(edge))
                    {
                        heap.Add(edge);
                    }
                }
            }

            return mst;
        }
    }

    // Represents an edge in the Delaunay triangulation graph
    [System.Serializable]
    public class Edge
    {
        public Vector2 p1;
        public Vector2 p2;
        public float weight;

        public Edge(Vector2 p1, Vector2 p2)
        {
            this.p1 = p1;
            this.p2 = p2;
            weight = Vector2.Distance(p1, p2);
        }
    }


}


