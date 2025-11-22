using System.Collections.Generic;
using UnityEngine;

namespace ProceduralWorlds.Storm.AreaPopulator
{
    public class AreaPopulatorData
    {

    }

    public class DecorationSegment
    {
        public bool occupied = false;
        public DecorationObjectInfo occupySource;
        public Vector3 occupySourcePosition;
        public bool grounded = false;
        //negative one is quite far from an edge
        public int distanceToClosestEdge = -1;
        public Vector2Int directionToClosestEdge;
        public Vector3 worldPosition;
        public Vector3 localPosition;
        public Vector3Int gridPosition;
    }

    public enum PlacementDirections
    {
        FacingNearestEdge,
        FacingTowardsCenter,
        Inconsequential,
    }

    public class PlacementData
    {
        public SpawnPoint spawnPoint;
        public List<DecorationSegment> decorationSegments;
        public PlacementData(SpawnPoint spawnPoint, List<DecorationSegment> decorationSegments)
        {
            this.spawnPoint = spawnPoint;
            this.decorationSegments = decorationSegments;
        }
    }

    public struct SpawnPoint
    {
        public Vector3 position;
        public float rotation;

        public SpawnPoint(Vector3 position, float rotation)
        {
            this.position = position;
            this.rotation = rotation;
        }
    }

    public enum PlacementFilterType
    {
        Line,
        Edge,
    }
}
