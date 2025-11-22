using Unity.Collections;

/// Copyright 2023 Procedural Worlds Ltd
/// Written by Jonathan Williams as part of Storm

namespace ProceduralWorlds.Storm.Utils
{
    public class NativeArrayRef<T> : System.IDisposable
        where T : struct
    {
        public string m_debugName;
        private NativeArrayCache<T> _cache;
        private int _useCount = 1;
        private int _length;
        public bool m_showDebug;
        public NativeArray<T> Array;

        //public string allocation;
        //public string lastUse;
        //public string lastDispose;

        public NativeArrayRef(int length, NativeArrayCache<T> cache, bool showDebug, string debugName, NativeArrayOptions options = NativeArrayOptions.ClearMemory)
        {
            //allocation = System.Environment.StackTrace;
            m_debugName = debugName;
            m_showDebug = showDebug;
            if (m_showDebug)
            {
                UnityEngine.Debug.Log(m_debugName + ": Creating at size: " + length + " (" + (length * System.Runtime.InteropServices.Marshal.SizeOf(typeof(T))).ToString("N0") + "bytes)");
            }
            _cache = cache;
            _length = length;
            Array = new NativeArray<T>(length, Allocator.Persistent, options);
        }
        public NativeArrayRef(T[] array, NativeArrayCache<T> cache, bool showDebug, string debugName, Allocator allocator = Allocator.Persistent)
        {
            //allocation = System.Environment.StackTrace;
            m_debugName = debugName;
            m_showDebug = showDebug;
            if (m_showDebug)
            {
                UnityEngine.Debug.Log(m_debugName + ": Creating at size: " + array.Length + " (" + (array.Length * System.Runtime.InteropServices.Marshal.SizeOf(typeof(T))).ToString("N0") + "bytes)");
            }
            _cache = cache;
            Array = new NativeArray<T>(array, allocator);
            _length = Array.Length;
        }
        public NativeArrayRef(NativeArray<T> array, NativeArrayCache<T> cache, bool showDebug, string debugName)
        {
            //allocation = System.Environment.StackTrace;
            m_debugName = debugName;
            m_showDebug = showDebug;
            if (m_showDebug)
            {
                UnityEngine.Debug.Log(m_debugName + ": Creating at size: " + array.Length + " (" + (array.Length * System.Runtime.InteropServices.Marshal.SizeOf(typeof(T))).ToString("N0") + "bytes)");
            }
            _cache = cache;
            _length = array.Length;
            Array = array;
        }
        ~NativeArrayRef()
        {
            //if (_useCount > 0)
            //{
            //    UnityEngine.Debug.LogWarning(m_debugName + ": Destructor called with uses left. Allocated at:\n" + allocation);
            //    UnityEngine.Debug.LogWarning(m_debugName + ": Last Use() call:\n" + lastUse);
            //    UnityEngine.Debug.LogWarning(m_debugName + ": Last Dispose():\n" + lastDispose);
            //}
            FullDispose();
        }

        public T this[int index]
        {
            get => Array.IsCreated && Array.Length > index ? Array[index] : default;
            set => Array[index] = value;
        }
        public void CopyFrom(T[] other) => Array.CopyFrom(other);
        public void CopyFrom(NativeArray<T> other) => Array.CopyFrom(other);
        public void CopyFrom(NativeArrayRef<T> other) => Array.CopyFrom(other.Array);

        public void Use()
        {
            //lastUse = System.Environment.StackTrace;
            ++_useCount;
            if (m_showDebug)
                UnityEngine.Debug.Log(m_debugName + ": Use(). UseCount=" + _useCount);
            if (!Array.IsCreated)
                Array = new NativeArray<T>(_length, Allocator.Persistent);
        }
        public void Dispose() => Dispose(false);
        public void Dispose(bool force)
        {
            //lastDispose = System.Environment.StackTrace;
            if (Array.IsCreated)
            {
                if (m_showDebug)
                    UnityEngine.Debug.Log(m_debugName + ": Dispose(" + (force ? "T" : "F") + ") capacity: " + Array.Length + " (" + (Array.Length * System.Runtime.InteropServices.Marshal.SizeOf(typeof(T))).ToString("N0") + "bytes)" +
                        "\nUseCount=" + (_useCount - 1));
                if (--_useCount > 0 && !force)
                    return;
                _useCount = 0;
                if (_cache != null)
                {
                    if (m_showDebug)
                        UnityEngine.Debug.Log(m_debugName + ": Returning...");
                    _cache.Return(this);
                }
                else
                {
                    if (m_showDebug)
                        UnityEngine.Debug.Log(m_debugName + ": Disposing...");
                    Array.Dispose();
                }
            }
        }
        public void FullDispose()
        {
            if (m_showDebug)
                UnityEngine.Debug.Log(m_debugName + ": FullDispose.");
            _cache = null;
            Dispose(true);
        }
        public bool IsCreated => Array.IsCreated;
        public int Length => Array.IsCreated ? _length : 0;
        public int UseCount => _useCount;

        public static implicit operator NativeArray<T>(NativeArrayRef<T> r)
        {
            return r?.Array ?? default;
        }
    }
}