using System;
using UnityEngine;

/// Copyright 2023 Procedural Worlds Ltd
/// Written by Jonathan Williams as part of Storm

namespace ProceduralWorlds.Storm.Utils
{
    [Serializable]
    public class StormTileRegion
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
        [HideInInspector]
        public Bounds LastBounds;

        /// <summary>
        /// List of all Regions that should be active when this tile is within load range
        /// </summary>
        public StormRegion[] Regions = Array.Empty<StormRegion>();

        private StormRegion[] lastRegions = Array.Empty<StormRegion>();
        /// <summary>
        /// Action to be invoked when the Region list on this tile changes
        /// </summary>
        public Action<Bounds> OnRegionsChanged;

        public StormTileRegion()
        {
            LastBounds = Bounds;
            if (Regions == null)
                Regions = Array.Empty<StormRegion>();
        }

        public void UpdateData()
        {
            if (!StormUtils.Match(Regions, lastRegions))
            {
                lastRegions = StormUtils.CopyArray(Regions);
                StormAPI.UpdateStormTile(this, LastBounds);
            }
        }

        public void DoRegionsChanged()
        {
            Bounds newBounds = Bounds;
            LastBounds.Encapsulate(newBounds);
            OnRegionsChanged?.Invoke(LastBounds);
            LastBounds = newBounds;
        }

        [HideInInspector]
        public GameObject m_myObject;
    }
}