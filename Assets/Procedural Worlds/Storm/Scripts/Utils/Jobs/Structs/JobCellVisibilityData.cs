using Unity.Mathematics;

/// Copyright 2023 Procedural Worlds Ltd
/// Written by Jonathan Williams as part of Storm

namespace ProceduralWorlds.Storm.Utils
{
    /// <summary>
    /// Used by culling job output to hold visibility and distance information
    /// </summary>
    public struct JobCellLODData
    {
        public const ulong Size = (sizeof(int) * 3) + (sizeof(float) * 2) + (sizeof(StormFeatureType) * 2) + 1;
        /// <summary>
        /// Index of this tile in the original list
        /// </summary>
        public int2 CellIndex;
        /// <summary>
        /// Index of this tile in the original list
        /// </summary>
        public int Index;
        /// <summary>
        /// Is this tile Visible? (according to this camera)
        /// </summary>
        public bool Visible;
        /// <summary>
        /// Is every corner of this tile Visible? (according to this camera)
        /// </summary>
        public bool FullyVisible;
        /// <summary>
        /// Should this tile only be processed for shadows? (according to this camera)
        /// </summary>
        public bool ShadowOnly;
        /// <summary>
        /// Should this tile be deleted? (according to this camera)
        /// </summary>
        public bool ShouldDelete;
        /// <summary>
        /// Squared distance from this camera to this tile
        /// </summary>
        public float DistanceMin;
        /// <summary>
        /// Squared distance from this camera to this tile
        /// </summary>
        public float DistanceMax;

        public StormFeatureType TypesToLoad;
        public StormFeatureType TypesToUnload;
    }
}