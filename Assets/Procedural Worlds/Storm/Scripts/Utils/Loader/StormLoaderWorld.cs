using UnityEngine;
using Unity.Mathematics;

/// Copyright 2024 Procedural Worlds Ltd
/// Written by Jonathan Williams as part of Storm

namespace ProceduralWorlds.Storm.Utils
{
    public class StormLoaderWorld : ScriptableObject
    {
        public double2 StartingWorldOffset;
        public StormLoaderData[] WorldTiles;
    }
}