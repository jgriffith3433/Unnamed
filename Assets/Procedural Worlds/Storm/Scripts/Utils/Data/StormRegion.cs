using System.Collections.Generic;
using UnityEngine;

/// Copyright 2023 Procedural Worlds Ltd
/// Written by Jonathan Williams as part of Storm

namespace ProceduralWorlds.Storm.Utils
{
    [CreateAssetMenu(menuName = "Procedural Worlds/Storm/Region")]
    public class StormRegion : ScriptableObject
    {
        /// <summary>
        /// Is this Region enabled for StormWorld to load?
        /// </summary>
        public bool IsEnabled = true;
        /// <summary>
        /// Array of all Layer names to use for selecting this Region
        /// </summary>
        public StormFeaturesetSourceLayerInfo[] SpawnLayers = System.Array.Empty<StormFeaturesetSourceLayerInfo>();
        /// <summary>
        /// Array of all Layer names that block this Region
        /// </summary>
        public StormFeaturesetSourceLayerInfo[] ExclusionLayers = System.Array.Empty<StormFeaturesetSourceLayerInfo>();


        /// <summary>
        /// List of Featuresets to render for this Region
        /// </summary>
        public StormFeatureset[] Featuresets = System.Array.Empty<StormFeatureset>();

        /// <summary>
        /// List of Region Masks currently registered to this Region
        /// </summary>
        [System.NonSerialized]
        public List<StormRegionMask> RegionMasks = new List<StormRegionMask>();
        public bool RequireRegionMasks;

        public void AddRegionMask(StormRegionMask mask)
        {
            if (!RegionMasks.Contains(mask))
            {
                RegionMasks.Add(mask);
                StormAPI.UpdateRegion(this);
            }
        }
        public void RemoveRegionMask(StormRegionMask mask)
        {
            if (RegionMasks.Remove(mask))
            {
                StormAPI.UpdateRegion(this);
            }
        }
    }
}