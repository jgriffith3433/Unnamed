using System;
using System.Collections.Generic;
using System.IO;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;

/// Copyright 2023 Procedural Worlds Ltd
/// Written by Jonathan Williams as part of Storm

namespace ProceduralWorlds.Storm.Utils
{
    /// <summary>
    /// Abstract class to inherit from when providing data to Storm
    /// </summary>
    public abstract class StormDataProvider : IDisposable
    {
        /// <summary>
        /// Provider Name to display in debug messages when messaging this Provider
        /// </summary>
        public abstract string DebugProviderName { get; }
        /// <summary>
        /// Array of Layer names this Data Provider provides data for
        /// </summary>
        public abstract string[] Layers { get; }

        int[] m_layerIndici;
        /// <summary>
        /// Array of Layer indici this Data Provider provides data for (get index from StormUtils.GetDataLayerIndex and listen for StormAPI.OnLayerIndexesChanged calls to update)
        /// </summary>
        public virtual int[] LayerIndicis
        {
            get {
                if (m_layerIndici == null)
                {
                    StormAPI.OnLayerIndexesChanged -= ResetLayerIndex;
                    StormAPI.OnLayerIndexesChanged += ResetLayerIndex;
                    m_layerIndici = StormUtils.GetDataLayerIndex(Layers, StormLayerType.NONE);
                }
                return m_layerIndici;
            }
        }

        public void ResetLayerIndex()
        {
            m_layerIndici = null;
        }
        /// <summary>
        /// Function called before Storm first accesses data for this Provider
        /// </summary>
        public abstract void Init();
        /// <summary>
        /// Function called when Storm is disposing of this data Provider
        /// </summary>
        public abstract void Dispose();
        /// <summary>
        /// Get data for a layer from a specific area
        /// </summary>
        /// <param name="layer">Layer index to get data for</param>
        /// <param name="area">Area in world space to get data for</param>
        /// <param name="usedDisposables">List of "IDisposable" objects that will be disposed after the load operation finishes. Add any NativeArray or other temporary/cached data objects to this</param>
        /// <param name="outputData">LayerData object containing the output data</param>
        /// <returns>True if data is returned and can be used, false otherwise</returns>
        public abstract bool GetData(int layer, Rect area, List<IDisposable> usedDisposables, out LayerData outputData);

        /// <summary>
        /// Action to invoke with the layer index and affected area when data from this Provider has changed
        /// </summary>
        public Action<int, Rect> OnDataChanged;
    }
}