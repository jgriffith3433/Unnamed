using System;
using Unity.Collections;

/// Copyright 2023 Procedural Worlds Ltd
/// Written by Jonathan Williams as part of Storm

namespace ProceduralWorlds.Storm.Utils
{
    public class NativeListRef<T> : IDisposable
        where T : unmanaged
    {
        public bool _showDebug;
        public string m_debugName;
        private int _useCount = 1;
        internal bool _dispose = false;
        internal int _capacity;
        public NativeList<T> Array;

        //public string allocation;
        //public string lastUse;
        //public string lastDispose;

        public NativeListRef(NativeList<T> array, bool showDebug, string name)
        {
            //allocation = Environment.StackTrace;
            _showDebug = showDebug;
            if (_showDebug)
                m_debugName = name;
            Array = array;
            _capacity = array.Capacity;
        }
        public NativeListRef(NativeArray<T> array, bool showDebug, string name)
        {
            //allocation = Environment.StackTrace;
            _showDebug = showDebug;
            if (_showDebug)
                m_debugName = name;
            _capacity = array.Length;
            Array = new NativeList<T>(_capacity, AllocatorManager.Persistent);
            Array.CopyFrom(array);
        }
        public NativeListRef(int capacity, bool showDebug, string name)
        {
            //allocation = Environment.StackTrace;
            _showDebug = showDebug;
            if (_showDebug)
            {
                m_debugName = name;
                UnityEngine.Debug.Log("Created NativeList " + m_debugName + " with capacity: " + capacity + " (" + (capacity * System.Runtime.InteropServices.Marshal.SizeOf(typeof(T))).ToString("N0") + "bytes)");
            }
            Array = new NativeList<T>(capacity, Allocator.Persistent);
            _capacity = capacity;
        }
        public NativeListRef(NativeListRef<T> original)
        {
            //allocation = Environment.StackTrace;
            _showDebug = original._showDebug;
            _capacity = original._capacity;
            m_debugName = original.m_debugName;
            Array = original.Array;
        }
        ~NativeListRef()
        {
            //if (_useCount > 0)
            //{
            //    UnityEngine.Debug.LogWarning(m_debugName + ": Destructor called with uses left. Allocated at:\n" + allocation);
            //    UnityEngine.Debug.LogWarning(m_debugName + ": Last Use() call:\n" + lastUse);
            //    UnityEngine.Debug.LogWarning(m_debugName + ": Last Dispose():\n" + lastDispose);
            //}
            FullDispose();
        }
        public void Use()
        {
            //lastUse = System.Environment.StackTrace;
            ++_useCount;
            if (_showDebug)
                UnityEngine.Debug.Log(m_debugName + ": Use() useCount=" + _useCount);
            if (!Array.IsCreated)
                Array = new NativeList<T>(Allocator.Persistent);
        }
        public void Dispose() => Dispose(false);
        public void Dispose(bool force)
        {
            //lastDispose = System.Environment.StackTrace;
            if (Array.IsCreated)
            {
                if (_showDebug)
                    UnityEngine.Debug.Log(m_debugName + ": Dispose("+(force?"T":"F")+ ") capacity: " + Array.Capacity + " (" + (Array.Capacity * System.Runtime.InteropServices.Marshal.SizeOf(typeof(T))).ToString("N0") + "bytes)" +
                        "\nuseCount=" + (_useCount - 1) + " & fullDispose=" + (_dispose?"T":"F"));
                if (--_useCount > 0 && !force)
                    return;
                _useCount = 0;
                if (_dispose)
                {
                    if (_showDebug)
                        UnityEngine.Debug.Log(m_debugName + ": Disposing...");
                    Array.Dispose();
                }
                else
                {
                    if (_showDebug)
                        UnityEngine.Debug.Log(m_debugName + ": Returning...");
                    NativeListCache<T>.Return(this);
                }
                Array = default;
            }
        }
        public void FullDispose()
        {
            _dispose = true;
            Dispose(true);
        }

        public bool IsCreated => Array.IsCreated;
        public int Length
        {
            get {
                if (Array.IsCreated)
                    return Array.Length;
                return 0;
            }
            set {
                if (Array.IsCreated)
                    Array.Length = value;
            }
        }
        public int Capacity
        {
            get => _capacity;
            set {
                _capacity = value;
                Array.SetCapacity(value);
            }
        }

        public void Add(T item)
        {
            if (Array.IsCreated)
                Array.Add(item);
        }
        public T this[int index]
        {
            get => Array[index];
            set => Array[index] = value;
        }

        public static implicit operator NativeList<T>(NativeListRef<T> r)
        {
            return r?.Array??default;
        }

        public static implicit operator NativeArray<T>(NativeListRef<T> r)
        {
            return r?.Array.AsArray()??default;
        }
    }
}