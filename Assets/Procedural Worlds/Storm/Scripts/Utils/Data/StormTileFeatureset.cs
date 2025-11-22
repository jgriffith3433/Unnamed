using System;
using UnityEngine;

/// Copyright 2023 Procedural Worlds Ltd
/// Written by Jonathan Williams as part of Storm

namespace ProceduralWorlds.Storm.Utils
{
    [Serializable]
    public class StormTileFeatureset
    {
        /// <summary>
        /// Position of this Tile in world space
        /// </summary>
        [HideInInspector]
        public Vector3 Position;
        /// <summary>
        /// Size of this Tile in world space
        /// </summary>
        [HideInInspector]
        public Vector3 Size;
        /// <summary>
        /// Helper to generate a Bounds of this tile from the Position and Size
        /// </summary>
        public Bounds Bounds => new Bounds(Position + (Size * 0.5f), Size);

        /// <summary>
        /// List of all featuresets that should be active when this tile is within load range
        /// </summary>
        public StormFeatureset[] m_featuresets = Array.Empty<StormFeatureset>();

        /// <summary>
        /// Action to be invoked when the featureset list on this tile changesww
        /// </summary>
        public Action OnFeaturesetChanged;
        [HideInInspector]
        public GameObject m_myObject;
    }
}