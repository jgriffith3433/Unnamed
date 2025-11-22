using System;

/// Copyright 2023 Procedural Worlds Ltd
/// Written by Jonathan Williams as part of Storm

namespace ProceduralWorlds.Storm.Utils
{
    [Serializable]
    public struct StormLoaderTypeData
    {
        /// <summary>
        /// Name of the Type
        /// </summary>
        public string TypeName;
        /// <summary>
        /// Distance to begin loading this Type
        /// </summary>
        public float LoadRange;
        /// <summary>
        /// Distance to unload this Type
        /// </summary>
        public float UnloadRange;
    }
}