using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.Collections;
using UnityEngine;

/// Copyright 2023 Procedural Worlds Ltd
/// Written by Jonathan Williams as part of Storm

namespace ProceduralWorlds.Storm.Utils
{
    public static class NativeListCache<T> where T : unmanaged
    {
        static int m_maxCacheSize = 20;
        public static int MaxCacheSize
        {
            get => m_maxCacheSize;
            set {
                if (m_maxCacheSize != value)
                {
                    m_maxCacheSize = Mathf.Max(0, value);
                    while (m_arrays.Count > MaxCacheSize)
                    {
                        m_arrays.Dequeue().Dispose();
                    }
                }
            }
        }
        public static bool ShowDebug = false;
        static bool m_hasInit = false;
        static Queue<NativeList<T>> m_arrays;
        static int m_arrayDebugIndex = 0;

        public static void Init(bool showDebug = false)
        {
            ShowDebug = showDebug;
            if (ShowDebug)
                Debug.Log(typeof(T).Name + ":Cache: Init(" + (showDebug ? "T)" : "F)"));
            if (!m_hasInit)
            {
                Dispose();
                if (m_arrays == null)
                    m_arrays = new Queue<NativeList<T>>();
                m_hasInit = true;
                //add a callback to automatically dispose lists if we forgot to elsewhere
                Application.wantsToQuit -= OnWantsToQuit;
                Application.wantsToQuit += OnWantsToQuit;
                Application.quitting -= Dispose;
                Application.quitting += Dispose;
                Application.unloading -= Dispose;
                Application.unloading += Dispose;
                //and clear our cache if we start running low on memory as well ... might be because of this
                Application.lowMemory -= Dispose;
                Application.lowMemory += Dispose;
            }
        }

        private static bool OnWantsToQuit()
        {
            Dispose();
            return true;
        }

        public static NativeListRef<T> Get(int capacity, bool initialize = true, bool showDebug = false, string debugName = "")
        {
            if (!m_hasInit)
                Init();
            if (showDebug)
            {
                if (string.IsNullOrEmpty(debugName))
                    debugName = "<color=orange>" + typeof(T).Name + "</color>:" + m_arrayDebugIndex++;
                else
                    debugName = "<color=orange>" + debugName + "</color>";
            }
            NativeListRef<T> ret = null;
            if (m_arrays.Count > 0)
                ret = new NativeListRef<T>(m_arrays.Dequeue(), showDebug, debugName);
            if (ret == null || !ret.IsCreated)
            {
                ret = new NativeListRef<T>(capacity, showDebug, debugName);
            }
            if (initialize)
                ret.Array.Clear();
            if (ret.Array.Capacity < capacity)
            {
                if (!initialize && ret.Array.Length > capacity)
                    ret.Array.Length = capacity;
                ret.Array.Capacity = capacity;
            }
            if (ShowDebug)
                Debug.Log(typeof(T).Name + ":Cache: Get(" + capacity + ", " + (initialize ? "True)" : "False) capacity: " + ret.Array.Capacity + " (" + (ret.Array.Capacity * System.Runtime.InteropServices.Marshal.SizeOf(typeof(T))).ToString("N0") + "bytes)" + "\nArrayIndex=" + ret.m_debugName));
            return ret;
        }
        public static void Return(NativeListRef<T> array)
        {
            if (ShowDebug)
                Debug.Log(typeof(T).Name + ": Return(" + array.m_debugName + ") capacity: " + array.Array.Capacity + " (" + (array.Array.Capacity * System.Runtime.InteropServices.Marshal.SizeOf(typeof(T))).ToString("N0") + "bytes)");
            array.Array.Clear();
            if (m_hasInit && m_arrays.Count < MaxCacheSize)
            {
                m_arrays.Enqueue(array);
            }
            else
                array.FullDispose();
        }

        public static void Dispose()
        {
            if (ShowDebug)
                Debug.Log(typeof(T).Name + ":Cache: Dispose()");
            m_hasInit = false;
            if (m_arrays != null)
            {
                while (m_arrays.Count > 0)
                {
                    NativeList<T> a = m_arrays.Dequeue();
                    if (a.IsCreated)
                        a.Dispose();
                }
                m_arrays.TrimExcess();
            }
            Application.wantsToQuit -= OnWantsToQuit;
            Application.quitting -= Dispose;
            Application.unloading -= Dispose;
            Application.lowMemory -= Dispose;
        }

        public static MemoryCategoryData GetMemoryUsage()
        {
            MemoryCategoryData ret = new MemoryCategoryData();

            foreach (NativeList<T> array in m_arrays)
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