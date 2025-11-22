using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProceduralWorlds.Storm.Utils
{
    /// <summary>
    /// The StormLayerSet class represents a set of layers for the StormWorld class.
    /// </summary>
    [CreateAssetMenu(menuName = "Procedural Worlds/Storm/Layer Set")]
    public class StormLayerSet : ScriptableObject
    {
        /// <summary>
        /// Represents a set of layers in Storm.
        /// </summary>
        /// <remarks>
        /// This class provides a list of string values that represents the different layers in the system.
        /// </remarks>
        public List<string> Layers = new List<string>();
        public List<StormLayerType> LayerTypes
        {
            get {
                if (m_layerTypes == null)
                {
                    m_layerTypes = new List<StormLayerType>(Layers?.Count ?? 0);
                }
                else
                {
                    int nLayers = Layers?.Count ?? 0;
                    int nTypes = m_layerTypes.Count;
                    if (nTypes < nLayers)
                        m_layerTypes.AddRange(new StormLayerType[nLayers - nTypes]);
                    else if (nTypes > nLayers)
                        m_layerTypes.RemoveRange(nLayers, nTypes - nLayers);
                }
                return m_layerTypes;
            }
            set {
                m_layerTypes = value;
            }
        }
        [UnityEngine.Serialization.FormerlySerializedAs("LayerTypes")]
        [SerializeField]
        private List<StormLayerType> m_layerTypes = new List<StormLayerType>();

        public void ClearAllLayers()
        {
            Layers.Clear();
            LayerTypes.Clear();
            StormUtils.SetAssetDirty(this);
        }

        public void AddStormLayers()
        {
            ReadOnlySpan<StormUtils.LayerNameSourceData> stormLayers = StormUtils.GetDataLayerNames();
            foreach (StormUtils.LayerNameSourceData stormLayer in stormLayers)
                AddLayer(stormLayer);
            StormUtils.SetAssetDirty(this);
        }

        public int FindLayer(string layerName)
        {
            return Layers.IndexOf(layerName);
            /*/
            for (int i = 0; i < Layers.Count; i++)
            {
                string layer = Layers[i];
                if (layer == layerName)
                    return i;
            }

            return -1;
            //*/
        }

        public bool LayerExists(string layerName)
        {
            return Layers.Contains(layerName);
            /*/
            int index = FindLayer(layerName);
            return index >= 0;
            //*/
        }

        public void AddLayer(StormUtils.LayerNameSourceData layerNameSourceData)
        {
            AddLayer(layerNameSourceData.Name, layerNameSourceData.Source);
        }

        public void AddLayer(string layer = "", StormLayerType type = StormLayerType.NONE)
        {
            if (LayerExists(layer))
                return;
            Layers.Add(layer);
            LayerTypes.Add(type);
            StormUtils.SetAssetDirty(this);
        }

        public void RemoveLayer(int index)
        {
            if (index != -1)
            {
                Layers.RemoveAt(index);
                LayerTypes.RemoveAt(index);
            }
        }
        
        public void RemoveLayer(string layer)
        {
            int index = Layers.IndexOf(layer);
            RemoveLayer(index);
        }

        public void RemoveLayer(StormLayerType type)
        {
            int index = LayerTypes.IndexOf(type);
            RemoveLayer(index);
        }
    }
}