using System;
using System.Collections.Generic;
using System.IO;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

/// Copyright 2023 Procedural Worlds Ltd
/// Written by Jonathan Williams as part of Storm

namespace ProceduralWorlds.Storm.Utils
{
    public class PolygonSet : IDisposable
    {
        public NativeArrayRef<int> Counts;
        public NativeArrayRef<float2> Points;
        public NativeArrayRef<float4> Bounds;
        public NativeArrayRef<float> Values;

        int usedCount = 1;

        public PolygonSet(PolyPointSet[] polys, float[] values, float[] falloffs)
        {
            int totalCount = 0;
            int numPolys = polys.Length;
            Counts = new NativeArrayRef<int>(numPolys, null, false, "PolyDataCounts", NativeArrayOptions.UninitializedMemory);
            Bounds = new NativeArrayRef<float4>(numPolys, null, false, "PolyDataBounds", NativeArrayOptions.UninitializedMemory);
            Values = new NativeArrayRef<float>(numPolys * 2, null, false, "PolyDataValues", NativeArrayOptions.UninitializedMemory);
#if UNITY_2022 || UNITY_2022_1_OR_NEWER
                Span<int> CountsSpan = Counts.Array.AsSpan();
                Span<float4> BoundsSpan = Bounds.Array.AsSpan();
                Span<float> ValuesSpan = Values.Array.AsSpan();
#endif
            for (int i = 0; i < numPolys; ++i)
            {
                int numPoints = polys[i].Length;
                totalCount += numPoints;
                float val = values != null && values.Length > i ? values[i] : 1f;
                float falloff = falloffs != null && falloffs.Length > i ? falloffs[i] : 0f;
#if UNITY_2022 || UNITY_2022_1_OR_NEWER
                CountsSpan[i] = numPoints;
                ValuesSpan[i*2] = val;
                ValuesSpan[i*2+1] = falloff;
#else
                Counts[i] = numPoints;
                Values[i*2] = val;
                Values[i*2+1] = falloff;
#endif
            }
            int idx = 0;
            Points = new NativeArrayRef<float2>(totalCount, null, false, "PolyDataPoints", NativeArrayOptions.UninitializedMemory);
#if UNITY_2022 || UNITY_2022_1_OR_NEWER
                Span<float2> PointsSpan = Points.Array.AsSpan();
#endif
            for (int i = 0; i < numPolys; ++i)
            {
                PolyPointSet poly = (PolyPointSet)polys[i];
                float falloff = falloffs != null && falloffs.Length > i ? falloffs[i] : 0f;
                int numPoints = poly.Length;
                Vector4 bounds = Vector4.zero;
                for (int p = 0; p < numPoints; ++p)
                {
                    Vector2 point = poly[p];
                    if (p == 0)
                        bounds = new Vector4(point.x - falloff, point.y - falloff, point.x + falloff, point.y + falloff);
                    else
                    {
                        if (bounds.x > point.x - falloff)
                            bounds.x = point.x - falloff;
                        if (bounds.y > point.y - falloff)
                            bounds.y = point.y - falloff;
                        if (bounds.z < point.x + falloff)
                            bounds.z = point.x + falloff;
                        if (bounds.w < point.y + falloff)
                            bounds.w = point.y + falloff;
                    }
#if UNITY_2022 || UNITY_2022_1_OR_NEWER
                    PointsSpan[idx++] = point;
#else
                    Points[idx++] = point;
#endif
                }
#if UNITY_2022 || UNITY_2022_1_OR_NEWER
                BoundsSpan[i] = bounds;
#else
                Bounds[i] = bounds;
#endif
            }
        }
        ~PolygonSet()
        {
            if (Counts != null)
                Counts.Dispose(true);
            if (Points != null)
                Points.Dispose(true);
            if (Bounds != null)
                Bounds.Dispose(true);
            if (Values != null)
                Values.Dispose(true);
        }
        public bool IsCreated
        {
            get => Points.IsCreated;
        }

        public void Use()
        {
            ++usedCount;
        }

        public void Dispose()
        {
            if (--usedCount > 0)
                return;
            if (Counts != null)
                Counts.Dispose();
            if (Points != null)
                Points.Dispose();
            if (Bounds != null)
                Bounds.Dispose();
            if (Values != null)
                Values.Dispose();
        }
    }
    [Serializable]
    public struct PolyPointSet
    {
        [SerializeField]
        private Vector2[] _points;
        public ReadOnlySpan<Vector2> Points => _points;
        public PolyPointSet(Vector2[] points)
        {
            if (points == null)
                points = Array.Empty<Vector2>();
            _points = points;
        }

        public Vector2 this[int index]
        {
            get => _points[index];
            set => _points[index] = value;
        }
        public int Length
        {
            get => _points.Length;
            set {
                Array.Resize(ref _points, value);
            }
        }

        public static implicit operator Vector2[](PolyPointSet pointSet)
        {
            return pointSet._points;
        }
        public static explicit operator PolyPointSet(Vector2[] points)
        {
            return new PolyPointSet(points);
        }
        public static bool operator ==(PolyPointSet l, PolyPointSet r)
        {
            if (l._points == null || r._points == null)
                return false;
            if (l._points.Length != r._points.Length)
                return false;
            for (int i = 0; i < r._points.Length; ++i)
                if (r._points[i] != l._points[i])
                    return false;
            return true;
        }
        public static bool operator !=(PolyPointSet l, PolyPointSet r)
        {
            if (l._points == null || r._points == null)
                return true;
            if (l._points.Length != r._points.Length)
                return true;
            for (int i = 0; i < r._points.Length; ++i)
                if (r._points[i] != l._points[i])
                    return true;
            return false;
        }
        public static bool operator ==(PolyPointSet l, ICollection<Vector2> r)
        {
            if (l._points == null || r == null)
                return false;
            if (l._points.Length != r.Count)
                return false;
            int i = 0;
            foreach(Vector2 rp in r)
                if (rp != l._points[i++])
                    return false;
            return true;
        }
        public static bool operator !=(PolyPointSet l, ICollection<Vector2> r)
        {
            if (l._points == null || r == null)
                return true;
            if (l._points.Length != r.Count)
                return true;
            int i = 0;
            foreach (Vector2 rp in r)
                if (rp != l._points[i++])
                    return true;
            return false;
        }
        public override bool Equals(object obj)
        {
            if (obj is PolyPointSet oSet)
            {
                if (oSet._points.Length != _points.Length)
                    return false;
                for (int i = 0; i < _points.Length; ++i)
                    if (_points[i] != oSet._points[i])
                        return false;
                return true;
            }
            return base.Equals(obj);
        }
        public override int GetHashCode()
        {
            if (_points == null)
                return 0;
            return _points.GetHashCode();
        }

        public void Save(Stream stream)
        {
            stream.WriteByte(1); //version
            byte[] buf = BitConverter.GetBytes(Length);
            stream.Write(buf, 0, buf.Length);
            for (int p = 0; p < Length; ++p)
            {
                buf = BitConverter.GetBytes(_points[p].x);
                stream.Write(buf, 0, buf.Length);
                buf = BitConverter.GetBytes(_points[p].y);
                stream.Write(buf, 0, buf.Length);
            }
        }

        public static PolyPointSet Load(Stream stream)
        {
            PolyPointSet ret = new PolyPointSet();

            if (stream == null || !stream.CanRead)
            {
                Debug.LogError("Unable to load Polygon, Invalid stream");
                return ret;
            }
            int version = stream.ReadByte();
            if (version < 1 || version > 1)
            {
                Debug.LogError("Unable to load Polygon. Invalid file version: " + version);
                return ret;
            }
            byte[] buf = new byte[8];
            if(stream.Read(buf, 0, 4) < 4)
            {
                Debug.LogError("Unable to load Polygon. Unexpected EOS");
                return ret;
            }
            int len = BitConverter.ToInt32(buf, 0);
            if (len < 0 || len > 268435456)
            {
                Debug.LogError("Unable to load Polygon. Invalid Length " + len);
                return ret;
            }
            Vector2[] points = new Vector2[len];
            for (int p = 0; p < len; ++p)
            {
                if (stream.Read(buf, 0, 8) < 8)
                {
                    Debug.LogError("Unable to load Polygon. Unexpected EOS");
                    return ret;
                }
                points[p] = new Vector2(BitConverter.ToInt32(buf, 0), BitConverter.ToInt32(buf, 4));
            }
            ret._points = points;
            return ret;
        }
    }
}