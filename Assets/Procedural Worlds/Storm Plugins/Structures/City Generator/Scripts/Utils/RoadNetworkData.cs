using System.Collections.Generic;
using UnityEngine;

namespace ProceduralWorlds.Storm.CityBuilder
{
    [CreateAssetMenu(fileName = "RoadNetworkData", menuName = "Procedural Worlds/Storm/City Generator/Road Network Data")]
    public class RoadNetworkData : ScriptableObject
    {
        public Dictionary<Vector3, RoadPointInformation> majorRoadPoints =
            new Dictionary<Vector3, RoadPointInformation>();

        public Dictionary<Vector3, RoadPointInformation> minorRoadPoints =
            new Dictionary<Vector3, RoadPointInformation>();

        public List<IntersectionInformation> intersections = new List<IntersectionInformation>();
    }
}
