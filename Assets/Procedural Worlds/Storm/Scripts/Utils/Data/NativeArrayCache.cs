using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

/// Copyright 2023 Procedural Worlds Ltd
/// Written by Jonathan Williams as part of Storm

namespace ProceduralWorlds.Storm.Utils
{
    public class NativeArrayCache<T> where T : struct
    {
        static int m_maxCacheSize = 20;
        public static int MaxCacheSize {
            get => m_maxCacheSize;
            set {
                if (m_maxCacheSize != value)
                {
                    m_maxCacheSize = Mathf.Max(0, value);
                    foreach (KeyValuePair<int, NativeArrayCache<T>> cache in _staticCaches)
                    {
                        while (cache.Value.m_arrays.Count > MaxCacheSize)
                        {
                            cache.Value.m_arrays.Dequeue().Dispose();
                        }
                    }
                }
            }
        }
        static Dictionary<int, NativeArrayCache<T>> _staticCaches = new Dictionary<int, NativeArrayCache<T>>();
        public static NativeArrayRef<T> Get(int size, bool initialize = false, T initVal = default, bool showDebug = false, string name = "_unset_")
        {
            NativeArrayCache<T> source;
            if (!_staticCaches.TryGetValue(size, out source))
            {
                source = new NativeArrayCache<T>(size, 1, showDebug, name);
                _staticCaches.Add(size, source);
                StormAPI.OnStormInstanceChanged -= OnStormInstanceChanged;
                StormAPI.OnStormInstanceChanged += OnStormInstanceChanged;
            }
            return source.Get(initialize, initVal);
        }
        public static void DisposeGlobal()
        {
            StormAPI.OnStormInstanceChanged -= OnStormInstanceChanged;
            foreach (var c in _staticCaches)
                if (c.Value != null)
                    c.Value.Dispose();
            _staticCaches.Clear();
        }
        static void OnStormInstanceChanged(object obj)
        {
            if (obj == null)
                DisposeGlobal();
        }

        public static MemoryCategoryData GetCacheMemoryUsage()
        {
            MemoryCategoryData ret = new MemoryCategoryData();

            foreach (KeyValuePair<int, NativeArrayCache<T>> cache in _staticCaches)
                ret += cache.Value.GetMemoryUsage();

            return ret;
        }

        int m_debugIndex = 0;
        int m_capacity;
        Queue<NativeArray<T>> m_arrays;
        bool _showDebug;
        readonly string m_debugName;

        public int Capacity
        {
            get => m_capacity;
            set {
                if (m_capacity != value)
                {
                    if (_showDebug)
                        Debug.Log(m_debugName + ":Cache: NativeArrayCache<" + typeof(T).Name + ">.Capacity is changing from " + m_capacity + " to " + value);
                    m_capacity = value;
                    while (m_arrays.Count > 0)
                        m_arrays.Dequeue().Dispose();
                }
            }
        }

        public NativeArrayCache(int capacity, int preGenCount = 0, bool showDebug = false, string name = "_unset_")
        {
            if (showDebug)
            {
                if (string.IsNullOrEmpty(name))
                    name = "<color=yellow>" + typeof(T).Name + "</color>";
                else
                    name = "<color=yellow>" + name + "</color>";
            }
            m_debugName = name;
            _showDebug = showDebug;
            if (_showDebug)
            {
                Debug.Log(m_debugName + ":Cache: new NativeArrayCache<" + typeof(T).Name + ">(" + capacity + ", " + preGenCount + ")");
            }
            m_capacity = capacity;
            m_arrays = new Queue<NativeArray<T>>(preGenCount);
            while (preGenCount-- > 0)
                m_arrays.Enqueue(new NativeArray<T>(capacity, Allocator.Persistent, NativeArrayOptions.UninitializedMemory));
            //add a callback to automatically dispose lists if we forgot to elsewhere
            Application.unloading -= Dispose;
            Application.unloading += Dispose;
            //and clear our cache if we start running low on memory as well ... might be because of this
            Application.lowMemory -= Dispose;
            Application.lowMemory += Dispose;
        }

        public void Dispose()
        {
            if(_showDebug)
                Debug.Log(m_debugName + ":Cache: NativeArrayCache<" + typeof(T).Name + ">.Dispose()");
#if UNITY_2021_2_OR_NEWER
            while (m_arrays.TryDequeue(out var a))
            {
#else
            while (m_arrays.Count > 0)
            {
                var a = m_arrays.Dequeue();
#endif
                if (a.IsCreated)
                    a.Dispose();
            }
            m_arrays.Clear();
            m_capacity = -1;
        }

        public NativeArrayRef<T> Get(bool initialize = false, T initVal = default)
        {
            if (_showDebug)
                Debug.Log(m_debugName + ":Cache: NativeArrayCache<" + typeof(T).Name + ">.Get(" + (initialize ? "True)" : "False)"));
            NativeArray<T> ret;
            NativeArrayRef<T> retVal;
#if UNITY_2021_2_OR_NEWER
            if (!m_arrays.TryDequeue(out ret) || !ret.IsCreated)
            {
#else
            if (m_arrays.Count > 0)
            {
                ret = m_arrays.Dequeue();
                if (!ret.IsCreated)
                {
                    ret = new NativeArray<T>(m_capacity, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
                    if (_showDebug)
                        Debug.Log(m_debugName + ":Cache: Dequeued invalid array. Created new");
                }
            }
            else
            {
#endif
                ret = new NativeArray<T>(m_capacity, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
                if (_showDebug)
                    Debug.Log(m_debugName + ":Cache: Created new array");
            }
            retVal = new NativeArrayRef<T>(ret, this, _showDebug, _showDebug ? m_debugName + ":" + m_debugIndex++ : "");
            if (_showDebug)
                Debug.Log(m_debugName + ":Cache: Got " + retVal.m_debugName);
            if (initialize)
            {
                unsafe
                {
                    UnsafeUtility.MemCpyReplicate(NativeArrayUnsafeUtility.GetUnsafePtr(ret), UnsafeUtility.AddressOf(ref initVal), Marshal.SizeOf<T>(), m_capacity);
                }
                /*/
#if  UNITY_2022 || UNITY_2022_1_OR_NEWER
                System.Span<T> retSpan = ret.AsSpan();
                retSpan.Fill(initVal);
#else
                for (int i = 0; i < m_capacity; ++i)
                    ret[i] = initVal;
#endif
                //*/
            }
            return retVal;
        }
        public void Return(NativeArrayRef<T> array)
        {
            if (array.Array.IsCreated)
            {
                if (array.Array.Length == m_capacity && m_arrays.Count < MaxCacheSize)
                {
                    if (_showDebug)
                        Debug.Log(m_debugName + ":Cache: NativeArrayCache<" + typeof(T).Name + ">.Return(" + array.m_debugName + ") Returning");
                    m_arrays.Enqueue(array.Array);
                }
                else
                {
                    if (_showDebug)
                        Debug.Log(m_debugName + ":Cache: NativeArrayCache<" + typeof(T).Name + ">.Return(" + array.m_debugName + ") Disposing...");
                    array.Array.Dispose();
                }
                array.Array = default;
            }
        }

        public MemoryCategoryData GetMemoryUsage()
        {
            MemoryCategoryData ret = new MemoryCategoryData();
            
            foreach (NativeArray<T> array in m_arrays)
            {
                if (array.IsCreated)
                {
                    ++ret.Count;
                    ret.SizeCPU += (ulong)(array.Length * Marshal.SizeOf<T>());
                }
            }

            return ret;
        }
    }
}