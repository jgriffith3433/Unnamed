using UnityEngine;

/// Copyright 2023 Procedural Worlds Ltd
/// Written by Jonathan Williams as part of Storm

namespace ProceduralWorlds.Storm.Utils
{
    /// <summary>
    /// Scriptable object holds information about a Feature Set. A selection of items to render into the world from the available Data possibly masked to only cover a specific area
    /// </summary>
    [CreateAssetMenu(menuName = "Procedural Worlds/Storm/Feature Set")]
    public class StormFeatureset : ScriptableObject
    {
        /// <summary>
        /// Is this Featureset enabled for StormWorld to load?
        /// </summary>
        public bool IsEnabled = true;
        public StormLayerRemap[] LayerRemaps = new StormLayerRemap[0];
        /// <summary>
        /// Array of all Layer names to use for selecting this Feature Set
        /// </summary>
        public StormFeaturesetSourceLayerInfo[] SpawnLayers = new StormFeaturesetSourceLayerInfo[0];
        /// <summary>
        /// Array of all Layer names that block this Feature Set
        /// </summary>
        public StormFeaturesetSourceLayerInfo[] ExclusionLayers = new StormFeaturesetSourceLayerInfo[0];
        /// <summary>
        /// List of items to render for this Feature Set
        /// </summary>
        public StormFeatureItem[] Features;

        /// <summary>
        /// Gets index of requested Feature in Features array. -1 if not found.
        /// </summary>
        /// <param name="feature">Feature to search for</param>
        /// <returns>index of requested Feature in Features array. -1 if not found</returns>
        public int GetFeatureIndex(StormFeatureSettings feature)
        {
            for (int f = 0; f < Features?.Length; ++f)
            {
                if (Features[f].Feature == feature)
                    return f;
            }
            return -1;
        }
        /// <summary>
        /// Adds a Feature to this Featureset. If it already exists, the Spawn and Exclude layers will be merged in
        /// </summary>
        /// <param name="feature">Feature to add</param>
        /// <param name="spawnLayers">Spawn Layers to add</param>
        /// <param name="excludeLayers">Exclude Layers to add</param>
        public void AddFeature(StormFeatureSettings feature, StormFeaturesetSourceLayerInfo[] spawnLayers, StormFeaturesetSourceLayerInfo[] excludeLayers = null)
        {
            int idx = GetFeatureIndex(feature);
            if (idx < 0)
            {
                //Debug.Log("Adding Feature '" + feature.name + "' to Featureset '" + name + "'", this);
                idx = Features?.Length ?? 0;
                System.Array.Resize(ref Features, idx + 1);
                Features[idx] = new StormFeatureItem(feature, spawnLayers, excludeLayers);
                StormUtils.SetAssetDirty(this);
            }
            else
            {
                StormFeatureItem item = Features[idx];
                bool changed = false;
                for(int s = 0; s < spawnLayers?.Length; ++s)
                {
                    bool found = false;
                    for (int i = 0; i < item.SpawnLayers?.Length; ++i)
                    {
                        if (item.SpawnLayers[i].LayerName == spawnLayers[s].LayerName)
                        {
                            found = true;
                            break;
                        }
                    }
                    if (!found)
                    {
                        changed = true;
                        int len = item.SpawnLayers.Length;
                        System.Array.Resize(ref item.SpawnLayers, len + 1);
                        item.SpawnLayers[len] = spawnLayers[s];
                    }
                }
                for (int s = 0; s < excludeLayers?.Length; ++s)
                {
                    bool found = false;
                    for (int i = 0; i < item.ExclusionLayers.Length; ++i)
                    {
                        if (item.ExclusionLayers[i].LayerName == excludeLayers[s].LayerName)
                        {
                            found = true;
                            break;
                        }
                    }
                    if (!found)
                    {
                        changed = true;
                        int len = item.ExclusionLayers.Length;
                        System.Array.Resize(ref item.ExclusionLayers, len + 1);
                        item.ExclusionLayers[len] = excludeLayers[s];
                    }
                }
                if (changed)
                {
                    Features[idx] = item;
                    StormUtils.SetAssetDirty(this);
                }
            }
        }
    }
}