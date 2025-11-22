using System;

/// Copyright 2023 Procedural Worlds Ltd
/// Written by Jonathan Williams as part of Storm

namespace ProceduralWorlds.Storm.Utils
{
    /// <summary>
    /// Holds information about an Feature for a FeatureSet
    /// </summary>
    [Serializable]
    public struct StormFeatureItem
    {
        /// <summary>
        /// Array of all Layer names to use for generation
        /// </summary>
        public StormFeaturesetSourceLayerInfo[] SpawnLayers;

        /// <summary>
        /// Array of all Layer names that block generation
        /// </summary>
        public StormFeaturesetSourceLayerInfo[] ExclusionLayers;

        /// <summary>
        /// Reference to Storm Feature Settings scriptable object
        /// </summary>
        [UnityEngine.Serialization.FormerlySerializedAs("Settings")]
        public StormFeatureSettings Feature;

        public StormFeatureItem(StormFeatureSettings settings, StormFeaturesetSourceLayerInfo[] sources = null, StormFeaturesetSourceLayerInfo[] excludes = null)
        {
            if (sources == null)
                sources = Array.Empty<StormFeaturesetSourceLayerInfo>();
            if (excludes == null)
                excludes = Array.Empty<StormFeaturesetSourceLayerInfo>();
            SpawnLayers = sources;
            ExclusionLayers = excludes;
            Feature = settings;
        }
    }
}