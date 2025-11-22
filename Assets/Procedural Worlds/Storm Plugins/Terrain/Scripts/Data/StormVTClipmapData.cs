using System;
using UnityEngine;

namespace ProceduralWorlds.Storm.Terrain
{
    /// <summary>
    ///  Mesh Buffer for indirect instancing command
    /// </summary>
    [CreateAssetMenu(fileName = "StormVTClipmapData", menuName = "Procedural Worlds/Storm/Terrain/StormVTClipmapData", order = 1)]
    public class StormVTClipmapData : ScriptableObject
    {
        #region Public Variables
        /// <summary>
        /// Resolution of the clipmap system, describes the density / size of each ring level
        /// </summary>
        [Range(7, 512)] public int m_clipmapResolution = 200;

        /// <summary>
        /// Number of ring levels for the clipmap system
        /// </summary>
        [Range(1, 15)] public int m_clipmapRingLevels = 15;

        /// <summary>
        /// Scale that should be applied to the heightmap to match world terrain value
        /// </summary>
        public float m_clipmapHeightScale = 20f;
        
        /// <summary>
        /// Starting world position offset in the Y axis for the clipmap's '0' value of height
        /// </summary>
        public float m_clipmapWorldStartPosY = 0f;

        /// <summary>
        /// The resolution of the 'Block' Shape used in the clipmap ring levels
        /// </summary>
        public int m_clipmapBlockSize;
        
        /// <summary>
        /// Offset used for calculating 'Block' Shape
        /// </summary>
        public int m_clipmapOffset;

        /// <summary>
        /// Height threshold adjustment to scale how quickly the ring levels get culled
        /// </summary>
        public float m_clipmapHeightCullThreshold = 100f;

        #endregion

        #region Private Variables

        #endregion
        
        #region Event System
        /// <summary>
        /// Event called when clipmap data is updated
        /// </summary>
        public delegate void UpdateClipmapData();
        public static event UpdateClipmapData OnUpdateClipmapData;
        #endregion

        #region Methods
        
        /// <summary>
        /// Calculate block sizes depending on resolution
        /// </summary>
        public void CalculateBlockData()
        {
            m_clipmapBlockSize = (m_clipmapResolution + 1) / 4;
            m_clipmapOffset = m_clipmapBlockSize - 1;
        }

        /// <summary>
        /// Updates the clipmap data
        /// </summary>
        public void UpdateData()
        {
            CalculateBlockData();
        }

        /// <summary>
        /// Update the data once the any variable has changed
        /// </summary>
        public void OnValidate()
        {
            UpdateData();
            if (OnUpdateClipmapData != null)
                OnUpdateClipmapData();
        }

        #endregion

    }
}