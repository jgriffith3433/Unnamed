using System;
using Unity.Jobs;
using Unity.Mathematics;

namespace ProceduralWorlds.Storm.Utils
{
    public class LayerData : IDisposable
    {
        public bool IsLoading;
        public bool HasInstances;
        public bool HasFloats;
        public bool HasPolygons;

        /// <summary>
        /// Minimum Position this Data covers
        /// </summary>
        public float3 Position;
        /// <summary>
        /// Size of Area covered by this Data
        /// </summary>
        public float3 Size;

        public NativeArrayRef<float> Floats;
        public NativeArrayRef<byte> FloatPriorities;
        public int2 FloatResolution;
        public NativeListRef<StormItemData> Instances;
        public int InstanceCount;
        public NativeListRef<int> PolygonCounts;
        public NativeListRef<float2> PolygonPoints;
        public NativeListRef<float4> PolygonBounds;
        public NativeListRef<float> PolygonValues;
        public NativeListRef<byte> PolygonPriorities;

        public JobHandle FloatHandle;
        public JobHandle InstanceHandle;
        public JobHandle PolygonHandle;

        public bool IsComplete()
        {
            if (IsLoading && FloatHandle.IsCompleted && InstanceHandle.IsCompleted && PolygonHandle.IsCompleted)
            {
                FloatHandle.Complete();
                InstanceHandle.Complete();
                PolygonHandle.Complete();
                IsLoading = false;
                return true;
            }
            return false;
        }
        ~LayerData()
        {
            Dispose();
        }

        public void Dispose()
        {
            if (Floats != null)
                Floats.Dispose();
            Floats = null;
            if (FloatPriorities != null)
                FloatPriorities.Dispose();
            FloatPriorities = null;
            if (Instances != null)
                Instances.Dispose();
            Instances = null;
            if (PolygonCounts != null)
                PolygonCounts.Dispose();
            PolygonCounts = null;
            if (PolygonPoints != null)
                PolygonPoints.Dispose();
            PolygonPoints = null;
            if (PolygonBounds != null)
                PolygonBounds.Dispose();
            PolygonBounds = null;
            if (PolygonValues != null)
                PolygonValues.Dispose();
            PolygonValues = null;
            if (PolygonPriorities != null)
                PolygonPriorities.Dispose();
            PolygonPriorities = null;
        }
    }
}