using System;
using UnityEngine;

/// Copyright 2023 Procedural Worlds Ltd
/// Written by Jonathan Williams as part of Storm

namespace ProceduralWorlds.Storm.Utils
{
    public enum StormColliderType : byte
    {
        SPHERE = 1,
        BOX = 2,
        CAPSULE = 3,
        MESH = 4,

        CIRCLE_2D = 10,
        BOX_2D = 11,
        CAPSULE_2D = 12,
        POLYGON_2D = 13,
        EDGE_2D = 14,
    }
    [Serializable]
    public struct StormColliderData
    {
        [SerializeField]
        public StormColliderType Type;

        [SerializeField]
        public Vector3 Offset;
        [SerializeField]
        public Quaternion Rotation;
        [SerializeField]
        public Vector3 Scale;
        [SerializeField]
        public float[] Data;
        [SerializeField]
        [HideInInspector]
        [Obsolete("MeshID has been moved to a direct Mesh reference")]
        public byte[] MeshID; //this was originally to allow cross-engine support but that can just be done in binary data export. direct reference simplifies editor.
        [SerializeField]
        public Mesh Mesh;
        [SerializeField]
        public int LayerID;
        [SerializeField]
        public string Tag;
    }
}