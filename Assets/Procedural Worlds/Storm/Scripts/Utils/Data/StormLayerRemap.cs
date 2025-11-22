using System;
using System.Collections.Generic;
using UnityEngine;

/// Copyright 2023 Procedural Worlds Ltd
/// Written by Jonathan Williams as part of Storm

namespace ProceduralWorlds.Storm.Utils
{
    [CreateAssetMenu(menuName = "Procedural Worlds/Storm/Layer Remap")]
    public class StormLayerRemap : ScriptableObject
    {
        [Serializable]
        public class RemapData
        {
            [StormLayer("", StormLayerRemapMode.HIDE)]
            public string m_original;
            public string Original
            {
                get => m_original;
                set {
                    if (m_original != value)
                    {
                        m_original = value;
                        m_remappedLayerIndici = null;
                        m_originalIndex = -1;
                    }
                }
            }
            [StormLayer("", StormLayerRemapMode.HIDE)]
            public string[] m_remappedLayers;
            public string[] RemappedLayers
            {
                get => m_remappedLayers;
                set {
                    if (m_remappedLayers != value)
                    {
                        m_remappedLayers = value;
                        m_remappedLayerIndici = null;
                        m_originalIndex = -1;
                    }
                }
            }

            [NonSerialized]
            private int m_originalIndex;
            public int OriginalIndex
            {
                get {
                    if (m_remappedLayerIndici == null)
                    {
                        m_originalIndex = StormUtils.GetDataLayerIndex(m_original, StormLayerType.REMAP_USED);
                        m_remappedLayerIndici = new int[m_remappedLayers.Length];
                        for (int i = 0; i < m_remappedLayers?.Length; ++i)
                            m_remappedLayerIndici[i] = StormUtils.GetDataLayerIndex(m_remappedLayers[i], StormLayerType.REMAP_USED);
                    }
                    return m_originalIndex;
                }
                set {
                    if (m_originalIndex != value)
                        m_originalIndex = value;
                }
            }
            [NonSerialized]
            private int[] m_remappedLayerIndici;
            public int[] RemappedLayerIndici
            {
                get {
                    if (m_remappedLayerIndici == null)
                    {
                        m_originalIndex = StormUtils.GetDataLayerIndex(m_original, StormLayerType.REMAP_USED);
                        m_remappedLayerIndici = new int[m_remappedLayers.Length];
                        for (int i = 0; i < m_remappedLayers?.Length; ++i)
                            m_remappedLayerIndici[i] = StormUtils.GetDataLayerIndex(m_remappedLayers[i], StormLayerType.REMAP_USED);
                    }
                    return m_remappedLayerIndici;
                }
                set {
                    if (m_remappedLayerIndici != value)
                        m_remappedLayerIndici = value;
                }
            }
        }

        public RemapData[] Remaps = new RemapData[0];
        public Action OnChanged;

        public void ResetLayerIndici()
        {
            for(int r = 0; r < Remaps.Length; ++r)
            {
                Remaps[r].OriginalIndex = -1;
                Remaps[r].RemappedLayerIndici = null;
            }
        }

        /// <summary>
        /// Get count of remaps for a specified layer name
        /// </summary>
        /// <param name="original">current ID of original layer name</param>
        /// <returns>number of remapped layers existing</returns>
        public int GetLayerRemapCount(int original)
        {
            int ret = 0;
            for (int l = 0; l < Remaps.Length; ++l)
                if (Remaps[l].OriginalIndex == original)
                    ret += Remaps[l].m_remappedLayers.Length;
            return ret;
        }
        /// <summary>
        /// Get count of remaps for a specified layer name
        /// </summary>
        /// <param name="original">original layer name</param>
        /// <returns>number of remapped layers existing</returns>
        public int GetLayerRemapCount(string original)
        {
            int ret = 0;
            for (int l = 0; l < Remaps.Length; ++l)
                if (Remaps[l].m_original == original)
                    ret += Remaps[l].m_remappedLayers.Length;
            return ret;
        }
        /// <summary>
        /// Get count of remaps for a specified layer name
        /// </summary>
        /// <param name="original">current ID info of original layer</param>
        /// <returns>number of remapped layers existing</returns>
        public int GetLayerRemapCount(StormSourceIndexInfo original)
        {
            int ret = 0;
            for (int l = 0; l < Remaps.Length; ++l)
                if (Remaps[l].OriginalIndex == original.Index)
                    ret += Remaps[l].RemappedLayerIndici.Length;
            return ret;
        }
        /// <summary>
        /// Get count of remaps for a specified layer name
        /// </summary>
        /// <param name="original">original layer info</param>
        /// <returns>number of remapped layers existing</returns>
        public int GetLayerRemapCount(StormSourceLayerInfo original)
        {
            int ret = 0;
            for (int l = 0; l < Remaps.Length; ++l)
                if (Remaps[l].m_original == original.LayerName)
                    ret += Remaps[l].RemappedLayerIndici.Length;
            return ret;
        }

        /// <summary>
        /// Get count of layers the passed layer is remapped to
        /// </summary>
        /// <param name="remap">current ID of layer name to check for</param>
        /// <returns>Number of layers this layer is remapped to</returns>
        public int GetRemapedLayerCount(int remap)
        {
            int ret = 0;
            for (int l = 0; l < Remaps.Length; ++l)
            {
                for (int r = 0; r < Remaps[l].RemappedLayerIndici?.Length; ++r)
                    if (Remaps[l].RemappedLayerIndici[r] == remap)
                    {
                        ++ret;
                        break;
                    }
            }
            return ret;
        }
        /// <summary>
        /// Get count of layers the passed layer is remapped to
        /// </summary>
        /// <param name="remap">Layer name to check for</param>
        /// <returns>Number of layers this layer is remapped to</returns>
        public int GetRemapedLayerCount(string remap)
        {
            int ret = 0;
            for (int l = 0; l < Remaps.Length; ++l)
            {
                for (int r = 0; r < Remaps[l].m_remappedLayers?.Length; ++r)
                    if (Remaps[l].m_remappedLayers[r] == remap)
                    {
                        ++ret;
                        break;
                    }
            }
            return ret;
        }
        /// <summary>
        /// Get count of layers the passed layer is remapped to
        /// </summary>
        /// <param name="remap">Current ID Info of layer name to check for</param>
        /// <returns>Number of layers this layer is remapped to</returns>
        public int GetRemapedLayerCount(StormSourceIndexInfo remap)
        {
            int ret = 0;
            for (int l = 0; l < Remaps.Length; ++l)
            {
                for (int r = 0; r < Remaps[l].RemappedLayerIndici?.Length; ++r)
                    if (Remaps[l].RemappedLayerIndici[r] == remap.Index)
                    {
                        ++ret;
                        break;
                    }
            }
            return ret;
        }
        /// <summary>
        /// Get count of layers the passed layer is remapped to
        /// </summary>
        /// <param name="remap">Layer info to check for</param>
        /// <returns>Number of layers this layer is remapped to</returns>
        public int GetRemapedLayerCount(StormSourceLayerInfo remap)
        {
            int ret = 0;
            for (int l = 0; l < Remaps.Length; ++l)
            {
                for (int r = 0; r < Remaps[l].m_remappedLayers?.Length; ++r)
                    if (Remaps[l].m_remappedLayers[r] == remap.LayerName)
                    {
                        ++ret;
                        break;
                    }
            }
            return ret;
        }

        /// <summary>
        /// Get List of Layer IDs this layer is remapped to
        /// </summary>
        /// <param name="original">Current ID of layer name</param>
        /// <returns>List of Layer IDs remapped to this layer</returns>
        public List<int> GetLayerRemaps(int original)
        {
            List<int> ret = new List<int>();
            ret.Add(original);
            for (int l = 0; l < Remaps.Length; ++l)
                if (Remaps[l].OriginalIndex == original)
                    ret.AddRange(Remaps[l].RemappedLayerIndici);
            return ret;
        }
        /// <summary>
        /// Get List of Layer IDs these layers are remapped to
        /// </summary>
        /// <param name="original">List of Current ID of layer name</param>
        /// <returns>List of Layer IDs remapped to these layers</returns>
        public List<int> GetLayerRemaps(int[] original)
        {
            List<int> ret = new List<int>(original.Length);
            ret.AddRange(original);
            for (int o = 0; o < original.Length; ++o)
                for (int l = 0; l < Remaps.Length; ++l)
                    if (Remaps[l].OriginalIndex == original[o])
                        ret.AddRange(Remaps[l].RemappedLayerIndici);
            return ret;
        }
        /// <summary>
        /// Get List of Layers this layer is remapped to
        /// </summary>
        /// <param name="original">Layer name</param>
        /// <returns>List of Layers remapped to this layer</returns>
        public List<string> GetLayerRemaps(string original)
        {
            List<string> ret = new List<string>();
            ret.Add(original);
            for (int l = 0; l < Remaps.Length; ++l)
                if (Remaps[l].m_original == original)
                    ret.AddRange(Remaps[l].m_remappedLayers);
            return ret;
        }
        /// <summary>
        /// Get List of Layers these layers are remapped to
        /// </summary>
        /// <param name="original">List of Layer name</param>
        /// <returns>List of Layers remapped to these layers</returns>
        public List<string> GetLayerRemaps(string[] original)
        {
            List<string> ret = new List<string>(original.Length);
            ret.AddRange(original);
            for (int o = 0; o < original.Length; ++o)
            {
                for (int l = 0; l < Remaps.Length; ++l)
                    if (Remaps[l].m_original == original[o])
                        ret.AddRange(Remaps[l].m_remappedLayers);
            }
            return ret;
        }

        /// <summary>
        /// Get List of Layer ID Infos this layer is remapped to
        /// </summary>
        /// <param name="original">Current ID of layer name</param>
        /// <param name="info">Layer info to get strength value from</param>
        /// <returns>List of Layer ID Infos remapped to this layer</returns>
        public List<StormSourceIndexInfo> GetLayerRemaps(int original, StormSourceLayerInfo info)
        {
            List<StormSourceIndexInfo> ret = new List<StormSourceIndexInfo>();
            ret.Add(new StormSourceIndexInfo(original, info.Strength));
            for (int l = 0; l < Remaps.Length; ++l)
                if (Remaps[l].OriginalIndex == original)
                    for (int i = 0; i < Remaps[l].RemappedLayerIndici.Length; ++i)
                        ret.Add(new StormSourceIndexInfo(Remaps[l].RemappedLayerIndici[i], info.Strength));
            return ret;
        }
        /// <summary>
        /// Get List of Layer ID Infos these layers are remapped to
        /// </summary>
        /// <param name="original">List of Current IDs of layer names</param>
        /// <param name="info">List of Layer infos to get strength values from</param>
        /// <returns>List of Layer ID Infos remapped to these layers</returns>
        public List<StormSourceIndexInfo> GetLayerRemaps(int[] original, StormSourceLayerInfo[] info)
        {
            List<StormSourceIndexInfo> ret = new List<StormSourceIndexInfo>(original.Length);
            for (int i = 0; i < original.Length; ++i)
                ret.Add(new StormSourceIndexInfo(original[i], info[i].Strength));
            for (int o = 0; o < original.Length; ++o)
                for (int l = 0; l < Remaps.Length; ++l)
                    if (Remaps[l].OriginalIndex == original[o])
                        for (int i = 0; i < Remaps[l].RemappedLayerIndici.Length; ++i)
                            ret.Add(new StormSourceIndexInfo(Remaps[l].RemappedLayerIndici[i], info[o].Strength));
            return ret;
        }
        /// <summary>
        /// Get List of Layer ID Infos this layer is remapped to
        /// </summary>
        /// <param name="original">Layer ID Info of layer</param>
        /// <returns>List of Layer ID Infos remapped to this layer</returns>
        public List<StormSourceIndexInfo> GetLayerRemaps(StormSourceIndexInfo original)
        {
            List<StormSourceIndexInfo> ret = new List<StormSourceIndexInfo>();
            ret.Add(original);
            for (int l = 0; l < Remaps.Length; ++l)
                if (Remaps[l].OriginalIndex == original.Index)
                    for (int i = 0; i < Remaps[l].RemappedLayerIndici.Length; ++i)
                        ret.Add(new StormSourceIndexInfo(Remaps[l].RemappedLayerIndici[i], original.Strength));
            return ret;
        }
        /// <summary>
        /// Get List of Layer ID Infos these layers are remapped to
        /// </summary>
        /// <param name="original">List of Current ID Infos of layer names</param>
        /// <returns>List of Layer ID Infos remapped to these layers</returns>
        public List<StormSourceIndexInfo> GetLayerRemaps(StormSourceIndexInfo[] original)
        {
            List<StormSourceIndexInfo> ret = new List<StormSourceIndexInfo>(original.Length);
            ret.AddRange(original);
            for (int o = 0; o < original.Length; ++o)
                for (int l = 0; l < Remaps.Length; ++l)
                    if (Remaps[l].OriginalIndex == original[o].Index)
                        for (int i = 0; i < Remaps[l].RemappedLayerIndici.Length; ++i)
                            ret.Add(new StormSourceIndexInfo(Remaps[l].RemappedLayerIndici[i], original[o].Strength));
            return ret;
        }
        /// <summary>
        /// Get List of Layer Infos this layer is remapped to
        /// </summary>
        /// <param name="original">Layer Info of layer</param>
        /// <returns>List of Layer Infos remapped to this layer</returns>
        public List<StormSourceLayerInfo> GetLayerRemaps(StormFeaturesetSourceLayerInfo original)
        {
            List<StormSourceLayerInfo> ret = new List<StormSourceLayerInfo>();
            ret.Add(original);
            for (int l = 0; l < Remaps.Length; ++l)
                if (Remaps[l].m_original == original.LayerName)
                    for (int i = 0; i < Remaps[l].m_remappedLayers.Length; ++i)
                        ret.Add(new StormSourceLayerInfo(Remaps[l].m_remappedLayers[i], original.Strength));
            return ret;
        }
        /// <summary>
        /// Get List of Layer Infos these layers are remapped to
        /// </summary>
        /// <param name="original">List of Current Infos of layer names</param>
        /// <returns>List of Layer Infos remapped to these layers</returns>
        public List<StormSourceLayerInfo> GetLayerRemaps(StormFeaturesetSourceLayerInfo[] original)
        {
            List<StormSourceLayerInfo> ret = new List<StormSourceLayerInfo>(original.Length);
            for (int o = 0; o < original.Length; ++o)
            {
                ret.Add(original[o]);
                for (int l = 0; l < Remaps.Length; ++l)
                    if (Remaps[l].m_original == original[o].LayerName)
                        for (int i = 0; i < Remaps[l].m_remappedLayers.Length; ++i)
                            ret.Add(new StormSourceLayerInfo(Remaps[l].m_remappedLayers[i], original[o].Strength));
            }
            return ret;
        }
        /// <summary>
        /// Get List of Layer Infos this layer is remapped to
        /// </summary>
        /// <param name="original">Layer Info of layer</param>
        /// <returns>List of Layer Infos remapped to this layer</returns>
        public List<StormSourceLayerInfo> GetLayerRemaps(StormSourceLayerInfo original)
        {
            List<StormSourceLayerInfo> ret = new List<StormSourceLayerInfo>();
            ret.Add(original);
            for (int l = 0; l < Remaps.Length; ++l)
                if (Remaps[l].m_original == original.LayerName)
                    for (int i = 0; i < Remaps[l].m_remappedLayers.Length; ++i)
                        ret.Add(new StormSourceLayerInfo(Remaps[l].m_remappedLayers[i], original.Strength));
            return ret;
        }
        /// <summary>
        /// Get List of Layer Infos these layers are remapped to
        /// </summary>
        /// <param name="original">List of Current Infos of layer names</param>
        /// <returns>List of Layer Infos remapped to these layers</returns>
        public List<StormSourceLayerInfo> GetLayerRemaps(StormSourceLayerInfo[] original)
        {
            List<StormSourceLayerInfo> ret = new List<StormSourceLayerInfo>(original.Length);
            ret.AddRange(original);
            for (int o = 0; o < original.Length; ++o)
            {
                for (int l = 0; l < Remaps.Length; ++l)
                    if (Remaps[l].m_original == original[o].LayerName)
                        for (int i = 0; i < Remaps[l].m_remappedLayers.Length; ++i)
                            ret.Add(new StormSourceLayerInfo(Remaps[l].m_remappedLayers[i], original[o].Strength));
            }
            return ret;
        }

        /// <summary>
        /// Get List of layer IDs remapped to use this layer
        /// </summary>
        /// <param name="remap">Layer ID to find remaps for</param>
        /// <param name="selfIfEmpty">Should we return the original string if no remap is found?</param>
        /// <returns>List of layer IDs remapped to use this layer</returns>
        public List<int> GetRemapedLayer(int remap, bool selfIfEmpty = true)
        {
            List<int> ret = new List<int>();
            bool found = false;
            for (int l = 0; l < Remaps.Length; ++l)
            {
                for (int r = 0; r < Remaps[l].RemappedLayerIndici?.Length; ++r)
                    if (Remaps[l].RemappedLayerIndici[r] == remap)
                    {
                        found = true;
                        ret.Add(Remaps[l].OriginalIndex);
                        break;
                    }
            }
            if (!found && selfIfEmpty)
                ret.Add(remap);
            return ret;
        }
        /// <summary>
        /// Get List of layer IDs remapped to use these layers
        /// </summary>
        /// <param name="remap">List of Layer IDs to find remaps for</param>
        /// <param name="selfIfEmpty">Should we return the original string if no remap is found?</param>
        /// <returns>List of layer IDs remapped to use these layers</returns>
        public List<int> GetRemapedLayer(int[] remap, bool selfIfEmpty = true)
        {
            List<int> ret = new List<int>(remap.Length);
            for (int o = 0; o < remap.Length; ++o)
            {
                bool found = false;
                for (int l = 0; l < Remaps.Length; ++l)
                {
                    for (int r = 0; r < Remaps[l].RemappedLayerIndici?.Length; ++r)
                        if (Remaps[l].RemappedLayerIndici[r] == remap[o])
                        {
                            found = true;
                            ret.Add(Remaps[l].OriginalIndex);
                            break;
                        }
                }
                if (!found && selfIfEmpty)
                    ret.Add(remap[o]);
            }
            return ret;
        }
        /// <summary>
        /// Get List of layers remapped to use this layer
        /// </summary>
        /// <param name="remap">Layer to find remaps for</param>
        /// <param name="selfIfEmpty">Should we return the original string if no remap is found?</param>
        /// <returns>List of layers remapped to use this layer</returns>
        public List<string> GetRemapedLayer(string remap, bool selfIfEmpty = true)
        {
            List<string> ret = new List<string>();
            bool found = false;
            for (int l = 0; l < Remaps.Length; ++l)
            {
                for (int r = 0; r < Remaps[l].m_remappedLayers.Length; ++r)
                {
                    if (Remaps[l].m_remappedLayers[r] == remap)
                    {
                        found = true;
                        ret.Add(Remaps[l].m_original);
                    }
                }
            }
            if (!found && selfIfEmpty)
                ret.Add(remap);
            return ret;
        }
        /// <summary>
        /// Get List of layers remapped to use these layers
        /// </summary>
        /// <param name="remap">List of Layers to find remaps for</param>
        /// <param name="selfIfEmpty">Should we return the original string if no remap is found?</param>
        /// <returns>List of layers remapped to use these layers</returns>
        public List<string> GetRemapedLayer(string[] remap, bool selfIfEmpty = true)
        {
            List<string> ret = new List<string>(remap.Length);
            for (int o = 0; o < remap.Length; ++o)
            {
                bool found = false;
                for (int l = 0; l < Remaps.Length; ++l)
                {
                    for (int r = 0; r < Remaps[l].m_remappedLayers?.Length; ++r)
                        if (Remaps[l].m_remappedLayers[r] == remap[o])
                        {
                            found = true;
                            ret.Add(Remaps[l].m_original);
                            break;
                        }
                }
                if (!found && selfIfEmpty)
                    ret.Add(remap[o]);
            }
            return ret;
        }

        /// <summary>
        /// Set a layer's remap list
        /// </summary>
        /// <param name="original">Layer to set remap list for</param>
        /// <param name="remaps">List of remapped layers to set for original layer</param>
        public void SetLayerRemap(string original, string[] remaps)
        {
            int len = Remaps.Length;
            for (int l = 0; l < len; ++l)
            {
                if (Remaps[l].m_original == original)
                {
                    Remaps[l].m_remappedLayers = remaps;
                    StormUtils.SetAssetDirty(this);
                    OnChanged?.Invoke();
                    return;
                }
            }
            Array.Resize(ref Remaps, len + 1);
            Remaps[len] = new RemapData() {
                m_original = original,
                m_remappedLayers = remaps
            };
            StormUtils.SetAssetDirty(this);
            OnChanged?.Invoke();
        }
        /// <summary>
        /// Set an entry in layer's remap list
        /// </summary>
        /// <param name="original">Layer to set remap list for</param>
        /// <param name="index">Index of remap to set</param>
        /// <param name="remap">Remapped layer to set</param>
        public void SetLayerRemap(string original, int index, string remap)
        {
            int len = Remaps.Length;
            for (int l = 0; l < len; ++l)
            {
                if (Remaps[l].m_original == original)
                {
                    if (index >= 0 && index < Remaps[l].m_remappedLayers.Length)
                    {
                        Remaps[l].m_remappedLayers[index] = remap;
                        StormUtils.SetAssetDirty(this);
                        OnChanged?.Invoke();
                    }
                    return;
                }
            }
        }
        /// <summary>
        /// Set an entry in layer's remap list
        /// </summary>
        /// <param name="original">Layer to set remap list for</param>
        /// <param name="oldRemap">Remapped layer to replace</param>
        /// <param name="newRemap">Remapped layer to set</param>
        public void SetLayerRemap(string original, string oldRemap, string newRemap)
        {
            int len = Remaps.Length;
            for (int l = 0; l < len; ++l)
            {
                if (Remaps[l].m_original == original)
                {
                    for (int r = 0; r < Remaps[l].RemappedLayers.Length; ++r)
                    {
                        if (Remaps[l].m_remappedLayers[r] == oldRemap)
                        {
                            Remaps[l].m_remappedLayers[r] = newRemap;
                            StormUtils.SetAssetDirty(this);
                            OnChanged?.Invoke();
                            return;
                        }
                    }
                    return;
                }
            }
        }
        /// <summary>
        /// Add a remap for a layer
        /// </summary>
        /// <param name="original">Layer to add remap to</param>
        /// <param name="remap">Layer to remap</param>
        public void AddLayerRemap(string original, string remap)
        {
            int len = Remaps.Length;
            for (int l = 0; l < len; ++l)
            {
                if (Remaps[l].m_original == original)
                {
                    for (int i = 0; i < Remaps[l].m_remappedLayers.Length; ++i)
                    {
                        if (Remaps[l].m_remappedLayers[i] == remap)
                        {
                            return;
                        }
                    }
                    int remapLen = Remaps[l].m_remappedLayers.Length;
                    Array.Resize(ref Remaps[l].m_remappedLayers, remapLen + 1);
                    Remaps[l].m_remappedLayers[remapLen] = remap;
                    StormUtils.SetAssetDirty(this);
                    OnChanged?.Invoke();
                    return;
                }
            }
            Array.Resize(ref Remaps, len + 1);
            Remaps[len] = new RemapData() {
                m_original = original,
                m_remappedLayers = new string[] { remap }
            };
            StormUtils.SetAssetDirty(this);
            OnChanged?.Invoke();
        }
        /// <summary>
        /// Remove all remaps for a specific layer
        /// </summary>
        /// <param name="original">Layer to remove remaps from</param>
        public void RemoveLayerRemap(string original)
        {
            int len = Remaps.Length;
            for (int l = 0; l < len; ++l)
            {
                if (Remaps[l].m_original == original)
                {
                    StormUtils.RemoveAt(ref Remaps, l);
                    StormUtils.SetAssetDirty(this);
                    OnChanged?.Invoke();
                    return;
                }
            }
        }
        /// <summary>
        /// Remove a specific remap from a layer
        /// </summary>
        /// <param name="original">Layer to remove remap from</param>
        /// <param name="remap">Remapped layer to remove from remap list</param>
        public void RemoveLayerRemap(string original, string remap)
        {
            int len = Remaps.Length;
            for (int l = 0; l < len; ++l)
            {
                if (Remaps[l].m_original == original)
                {
                    for (int i = 0; i < Remaps[l].m_remappedLayers.Length; ++i)
                    {
                        if (Remaps[l].m_remappedLayers[i] == remap)
                        {
                            if (Remaps[l].m_remappedLayers.Length == 1)
                            {
                                StormUtils.RemoveAt(ref Remaps, l);
                            }
                            else
                            {
                                StormUtils.RemoveAt(ref Remaps[l].m_remappedLayers, i);
                            }
                            StormUtils.SetAssetDirty(this);
                            OnChanged?.Invoke();
                            return;
                        }
                    }
                }
            }
        }
        /// <summary>
        /// Remove a specific remap from a layer
        /// </summary>
        /// <param name="original">Layer to remove remap from</param>
        /// <param name="index">Index to remove from remap list</param>
        public void RemoveLayerRemap(string original, int index)
        {
            int len = Remaps.Length;
            for (int l = 0; l < len; ++l)
            {
                if (Remaps[l].m_original == original)
                {
                    if (index >= 0 && index < Remaps[l].m_remappedLayers.Length)
                    {
                        if (Remaps[l].m_remappedLayers.Length == 1)
                        {
                            StormUtils.RemoveAt(ref Remaps, l);
                        }
                        else
                        {
                            StormUtils.RemoveAt(ref Remaps[l].m_remappedLayers, index);
                        }
                        StormUtils.SetAssetDirty(this);
                        OnChanged?.Invoke();
                        return;
                    }
                }
            }
        }
    }
}