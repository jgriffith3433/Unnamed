using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

/// Copyright 2023 Procedural Worlds Ltd
/// Written by Jonathan Williams as part of Storm

namespace ProceduralWorlds.Storm.Utils
{
    public static class StormNativeArrayExtensions
    {
        public static ref T GetRef<T>(this NativeArray<T> array, int index)
            where T : unmanaged
        {
            // You might want to validate the index first, as the unsafe method won't do that.
            if (index < 0 || index >= array.Length)
                throw new ArgumentOutOfRangeException(nameof(index));
            unsafe
            {
                return ref UnsafeUtility.ArrayElementAsRef<T>(array.GetUnsafePtr(), index);
            }
        }
        public static void Set<T>(this NativeArray<T> array, int index, T val)
            where T : unmanaged
        {
            // You might want to validate the index first, as the unsafe method won't do that.
            if (index < 0 || index >= array.Length)
                throw new ArgumentOutOfRangeException(nameof(index));
            unsafe
            {
                UnsafeUtility.WriteArrayElement<T>(array.GetUnsafePtr(), index, val);
            }
        }
        public static void SetRef<T>(this NativeArray<T> array, int index, ref T val)
            where T : unmanaged
        {
            // You might want to validate the index first, as the unsafe method won't do that.
            if (index < 0 || index >= array.Length)
                throw new ArgumentOutOfRangeException(nameof(index));
            unsafe
            {
                UnsafeUtility.WriteArrayElement<T>(array.GetUnsafePtr(), index, val);
            }
        }
        public static ref T GetRef<T>(this NativeList<T> array, int index)
            where T : unmanaged
        {
            // You might want to validate the index first, as the unsafe method won't do that.
            if (index < 0 || index >= array.Length)
                throw new ArgumentOutOfRangeException(nameof(index));
            unsafe
            {
                return ref UnsafeUtility.ArrayElementAsRef<T>(array.GetUnsafePtr(), index);
            }
        }
        public static void SetRef<T>(this NativeList<T> array, int index, ref T val)
            where T : unmanaged
        {
            // You might want to validate the index first, as the unsafe method won't do that.
            if (index < 0 || index >= array.Length)
                throw new ArgumentOutOfRangeException(nameof(index));
            unsafe
            {
                UnsafeUtility.WriteArrayElement<T>(array.GetUnsafePtr(), index, val);
            }
        }
    }
}