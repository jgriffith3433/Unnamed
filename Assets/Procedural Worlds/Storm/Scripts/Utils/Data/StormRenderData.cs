using System;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

/// Copyright 2023 Procedural Worlds Ltd
/// Written by Jonathan Williams as part of Storm

namespace ProceduralWorlds.Storm.Utils
{
    public interface StormBufferData
    {
        public static int Size { get; }
    }
    /// <summary>
    /// Contains instance information used by final Rendering shader
    /// </summary>
    [Serializable]
    public struct StormRenderData : StormBufferData
    {
        public static int Size => 72; // ([4x4 floats]4x4 + [float2]2) x [4 bytes per float]4 = (16 + 2) * 4 = 18 * 4 = 72
        /// <summary>
        /// Matrix used to draw this item
        /// </summary>
        public Matrix4x4 matrix;
        /// <summary>
        /// Extra data used by shader (Color, Fade)
        /// </summary>
        public float2 ExtraData;

        public StormRenderData(StormItemData item)
        {
            matrix = Matrix4x4.TRS(item.Translation, item.Rotation, item.Scale);
            ExtraData = item.ExtraData;
        }

        public override bool Equals(object obj)
        {
            if (obj is StormRenderData data)
                return data == this;
            return false;
        }
        public override int GetHashCode()
        {
            return matrix.GetHashCode() ^ ExtraData.GetHashCode();
        }
        public static bool operator==(StormRenderData a, StormRenderData b)
        {
            bool4x4 matrixMatches = ((float4x4)a.matrix) == ((float4x4)b.matrix);
            return math.all(matrixMatches.c0) && math.all(matrixMatches.c1) && math.all(matrixMatches.c2) && math.all(matrixMatches.c3) && a.ExtraData.x == b.ExtraData.x;
        }
        public static bool operator!=(StormRenderData a, StormRenderData b)
        {
            return a.matrix != b.matrix || a.ExtraData.x != b.ExtraData.x;
        }
    }
    /// <summary>
    /// Contains instance information used to store in the TransformList (mathematics types don't act the same as Unity ones in editor scripts)
    /// </summary>
    [Serializable]
    public struct StormUnityItemData
    {
        public Vector3 Translation;
        public Quaternion Rotation;
        public Vector3 Scale;
        public Vector2 ExtraData;
        public StormUnityItemData(Transform t, Vector3 Offset, Vector3 positionScale)
        {
            Translation = t.position - Offset;
            Translation.x /= positionScale.x;
            Translation.y /= positionScale.y;
            Translation.z /= positionScale.z;
            Rotation = t.rotation;
            Scale = t.lossyScale;
            ExtraData = new Vector2(0, UnityEngine.Random.Range(0f,1f));
        }
        public StormUnityItemData(Transform t, Vector3 Offset, Vector3 positionScale, Vector2 extraData)
        {
            Translation = t.position - Offset;
            Translation.x /= positionScale.x;
            Translation.y /= positionScale.y;
            Translation.z /= positionScale.z;
            Rotation = t.rotation;
            Scale = t.lossyScale;
            ExtraData = extraData;
        }
        public StormUnityItemData(Vector3 t, Quaternion r, Vector3 s, float extra = 0)
        {
            Translation = t;
            Rotation = math.normalizesafe(r);
            Scale = s;
            ExtraData = new Vector2(extra, UnityEngine.Random.Range(0f, 1f));
        }
        public StormUnityItemData(Vector3 t, Quaternion r, Vector3 s, Vector2 extra)
        {
            Translation = t;
            Rotation = math.normalizesafe(r);
            Scale = s;
            ExtraData = extra;
        }

        public static implicit operator StormItemData(StormUnityItemData item)
        {
            return new StormItemData(item.Translation, item.Rotation, item.Scale, item.ExtraData);
        }
        public static implicit operator StormUnityItemData(StormItemData item)
        {
            return new StormUnityItemData(item.Translation, item.Rotation, item.Scale, item.ExtraData);
        }

        public override bool Equals(object obj)
        {
            if (obj is StormUnityItemData other)
            {
                return other.Translation == Translation && other.Rotation == Rotation && other.Scale == Scale;
            }
            return false;
        }
        public override int GetHashCode()
        {
            return Translation.GetHashCode() ^ Rotation.GetHashCode() ^ Scale.GetHashCode();
        }
        public static bool operator==(StormUnityItemData a, StormUnityItemData b)
        {
            return a.Equals(b);
        }
        public static bool operator!=(StormUnityItemData a, StormUnityItemData b)
        {
            return !a.Equals(b);
        }
    }
    /// <summary>
    /// Contains instance information output by generation
    /// </summary>
    public struct StormItemData : StormBufferData
    {
#if false //switch from float to half
        public static int Size => 30; //9 x (half)2 + 3 x (float)4 = 30

        public float3 Translation
        {
            get => m_translation;
            set {
                m_translation = value;
            }
        }
        public quaternion Rotation
        {
            get => math.normalize(math.quaternion(m_rotation));
            set => m_rotation = math.half4(value.value);
        }
        public float3 Scale
        {
            get => math.float3(m_scale);
            set => m_scale = math.half3(value);
        }
        public float2 ExtraData
        {
            get => math.float2(m_extraData);
            set => m_extraData = math.half2(value);
        }

        public float3 m_translation;
        public half4 m_rotation;
        public half3 m_scale;
        public half2 m_extraData;
        public StormItemData(Transform t)
        {
            m_translation = t.position;
            quaternion rot = t.rotation;
            m_rotation = math.half4(rot.value);
            m_scale = math.half3(t.lossyScale);
            m_extraData = half2.zero;
        }
        public StormItemData(float3 t, quaternion r, float3 s, float2 extra = default)
        {
            m_translation = t;
            m_rotation = new half4(r.value);
            m_scale = new half3(s);
            m_extraData = new half2(extra);
        }
        public StormItemData(float3 t, half4 r, half3 s, half2 extra = default)
        {
            m_translation = t;
            m_rotation = r;
            m_scale = s;
            m_extraData = extra;
        }
#else
        public static int Size => 48; //12 x (float)4 = 48

        public float3 Translation;
        public quaternion Rotation;
        public float3 Scale;
        public float2 ExtraData;
        public StormItemData(Transform t)
        {
            Translation = t.position;
            Rotation = math.normalizesafe(t.rotation);
            Scale = t.lossyScale;
            ExtraData = float2.zero;
        }
        public StormItemData(float3 t, quaternion r, float3 s, float2 extra = default)
        {
            Translation = t;
            Rotation = math.normalizesafe(r);
            Scale = s;
            ExtraData = extra;
        }
        public StormItemData(float3 t, half4 r, half3 s, half2 extra = default)
        {
            Translation = t;
            Rotation = new quaternion(math.normalizesafe(r));
            Scale = s;
            ExtraData = extra;
        }
#endif

        public Matrix4x4 GetMatrix(float3 offset)
        {
            return Matrix4x4.TRS(Translation + offset, Rotation, Scale);
        }

        public static bool operator ==(StormItemData a, StormItemData b)
        {
            return math.all(a.Translation == b.Translation) && math.all(a.Rotation.value == b.Rotation.value) && math.all(a.Scale == b.Scale) && math.all(a.ExtraData == b.ExtraData);
        }
        public static bool operator !=(StormItemData a, StormItemData b)
        {
            return math.any(a.Translation != b.Translation) || math.any(a.Rotation.value != b.Rotation.value) || math.any(a.Scale != b.Scale) || math.any(a.ExtraData != b.ExtraData);
        }
        public override bool Equals(object o)
        {
            if (o is StormItemData d)
            {
                return this == d;
            }
            return false;
        }
        public override int GetHashCode()
        {
            return Translation.GetHashCode() ^ Rotation.GetHashCode() ^ Scale.GetHashCode() ^ ExtraData.GetHashCode();
        }
    }
    /// <summary>
    /// Contains instance information to be used by compute shaders
    /// </summary>
    public struct StormComputeItemData : StormBufferData
    {
        public static int Size => 48; //12 x (float)4 = 48

        public float3 Translation;
        public float4 Rotation;
        public float3 Scale;
        public float2 ExtraData;

        public StormComputeItemData(Transform t)
        {
            Translation = t.position;
            Rotation = ((quaternion)t.rotation).value;
            Scale = t.lossyScale;
            ExtraData = float2.zero;
        }
        public StormComputeItemData(float3 t, quaternion r, float3 s, float2 extra = default)
        {
            Translation = t;
            Rotation = r.value;
            Scale = s;
            ExtraData = extra;
        }
        public StormComputeItemData(float3 t, half4 r, half3 s, half2 extra = default)
        {
            Translation = t;
            Rotation = r;
            Scale = s;
            ExtraData = extra;
        }

        public Matrix4x4 GetMatrix(float3 offset)
        {
            return Matrix4x4.TRS(Translation + offset, (quaternion)Rotation, Scale);
        }

        public static implicit operator StormComputeItemData(StormItemData item)
        {
            return new StormComputeItemData(item.Translation, item.Rotation, item.Scale, item.ExtraData);
        }

        public static bool operator ==(StormComputeItemData a, StormComputeItemData b)
        {
            return math.all(a.Translation == b.Translation) && math.all(a.Rotation == b.Rotation) && math.all(a.Scale == b.Scale) && math.all(a.ExtraData == b.ExtraData);
        }
        public static bool operator !=(StormComputeItemData a, StormComputeItemData b)
        {
            return math.all(a.Translation != b.Translation) || math.all(a.Rotation != b.Rotation) || math.all(a.Scale != b.Scale) || math.all(a.ExtraData != b.ExtraData);
        }
        public override bool Equals(object o)
        {
            if (o is StormComputeItemData d)
            {
                return this == d;
            }
            return false;
        }
        public override int GetHashCode()
        {
            return Translation.GetHashCode() ^ Rotation.GetHashCode() ^ Scale.GetHashCode() ^ ExtraData.GetHashCode();
        }

        public static void CopyToCompute(NativeArray<StormItemData> array, GraphicsBuffer Dest)
        {
            if (StormItemData.Size == Size)
            { //if we are using full floats, we can just do a straight copy.
                Dest.SetData(array);
            }
            else
            {
                ReadOnlySpan<StormItemData> arraySpan = array.AsReadOnlySpan();
                StormComputeItemData[] arr = new StormComputeItemData[math.min(Dest.count, arraySpan.Length)];
                for (int i = 0; i < arr.Length; ++i)
                    arr[i] = arraySpan[i];
                Dest.SetData(arr);
            }
        }
        public static void CopyToCompute(NativeList<StormItemData> array, GraphicsBuffer Dest)
        {
            if (StormItemData.Size == Size)
            { //if we are using full floats, we can just do a straight copy.
                Dest.SetData(array.AsArray());
            }
            else
            { //otherwise we need to translate the items first then transfer
                ReadOnlySpan<StormItemData> arraySpan = array.AsArray().AsReadOnlySpan();
                StormComputeItemData[] arr = new StormComputeItemData[math.min(Dest.count, arraySpan.Length)];
                for (int i = 0; i < arr.Length; ++i)
                    arr[i] = arraySpan[i];
                Dest.SetData(arr);
            }
        }
    }
}