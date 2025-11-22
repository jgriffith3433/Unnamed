
using System.ComponentModel;
using UnityEngine;

/// Copyright 2023 Procedural Worlds Ltd
/// Written by Jonathan Williams as part of Storm
namespace ProceduralWorlds.Storm.Utils
{
    /// <summary>
    /// Enumeration of available Culling types.
    /// </summary>
    public enum StormCullType
    {
        /// <summary>
        /// Compute shader used for culling
        /// </summary>
        [InspectorName("GPU - Preview, Unstable")]
        GPU,
        /// <summary>
        /// Job used for culling
        /// </summary>
        CPU,
        //GPU_SPLIT,
    }
}