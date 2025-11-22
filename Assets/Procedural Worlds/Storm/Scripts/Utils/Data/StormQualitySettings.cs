using UnityEngine;

/// Copyright 2025 Procedural Worlds Ltd
/// Written by Jonathan Williams as part of Storm

namespace ProceduralWorlds.Storm.Utils
{
    /// <summary>
    /// Scriptable Object to hold quality settings used by Storm for LODing, render distance, etc
    /// </summary>
    [CreateAssetMenu(menuName = "Procedural Worlds/Storm/Quality Settings")]
    public class StormQualitySettings : ScriptableObject
    {
        [Header("LOD Bias")]
        /// <summary>
        /// LODBias applied to all distances for LOD selection (as Pow() function)
        /// </summary>
        [Min(0.001f)]
        [Tooltip("LODBias applied to all distances for LOD selection (as Pow() function)")]
        public float m_globalLODBias = 1;
        public float m_globalLODBiasMaxUI = 10;
        /// <summary>
        /// LODBias applied to Grass distances for LOD selection (as Pow() function)
        /// </summary>
        [Min(0.001f)]
        [Tooltip("LODBias applied to Grass distances for LOD selection (as Pow() function)")]
        public float m_grassLODBias = 1;
        public float m_grassLODBiasMaxUI = 10;
        /// <summary>
        /// LODBias applied to Detail distances for LOD selection (as Pow() function)
        /// </summary>
        [Min(0.001f)]
        [Tooltip("LODBias applied to Detail distances for LOD selection (as Pow() function)")]
        public float m_detailLODBias = 1;
        public float m_detailLODBiasMaxUI = 10;
        /// <summary>
        /// LODBias applied to Plant distances for LOD selection (as Pow() function)
        /// </summary>
        [Min(0.001f)]
        [Tooltip("LODBias applied to Plant distances for LOD selection (as Pow() function)")]
        public float m_plantLODBias = 1;
        public float m_plantLODBiasMaxUI = 10;
        /// <summary>
        /// LODBias applied to Tree distances for LOD selection (as Pow() function)
        /// </summary>
        [Min(0.001f)]
        [Tooltip("LODBias applied to Tree distances for LOD selection (as Pow() function)")]
        public float m_treeLODBias = 1;
        public float m_treeLODBiasMaxUI = 10;
        /// <summary>
        /// LODBias applied to Small Object distances for LOD selection (as Pow() function)
        /// </summary>
        [Min(0.001f)]
        [Tooltip("LODBias applied to Small Object distances for LOD selection (as Pow() function)")]
        public float m_smallObjLODBias = 1;
        public float m_smallObjLODBiasMaxUI = 10;
        /// <summary>
        /// LODBias applied to Medium Object distances for LOD selection (as Pow() function)
        /// </summary>
        [Min(0.001f)]
        [Tooltip("LODBias applied to Medium Object distances for LOD selection (as Pow() function)")]
        public float m_mediumObjLODBias = 1;
        public float m_mediumObjLODBiasMaxUI = 10;
        /// <summary>
        /// LODBias applied to Large Object distances for LOD selection (as Pow() function)
        /// </summary>
        [Min(0.001f)]
        [Tooltip("LODBias applied to Large Object distances for LOD selection (as Pow() function)")]
        public float m_largeObjLODBias = 1;
        public float m_largeObjLODBiasMaxUI = 10;

        [Header("Density Settings")]
        /// <summary>
        /// Density multiplier to apply to Grass objects
        /// </summary>
        [Tooltip("Density multiplier to apply to Grass objects")]
        public float m_grassDensity = 1;
        public float m_grassDensityMaxUI = 10;
        /// <summary>
        /// Density multiplier to apply to Detail objects
        /// </summary>
        [Tooltip("Density multiplier to apply to Detail objects")]
        public float m_detailDensity = 1;
        public float m_detailDensityMaxUI = 10;
        /// <summary>
        /// Density multiplier to apply to Plant objects
        /// </summary>
        [Tooltip("Density multiplier to apply to Plant objects")]
        public float m_plantDensity = 1;
        public float m_plantDensityMaxUI = 10;
        /// <summary>
        /// Density multiplier to apply to Tree objects
        /// </summary>
        [Tooltip("Density multiplier to apply to Tree objects")]
        public float m_treeDensity = 1;
        public float m_treeDensityMaxUI = 10;
        /// <summary>
        /// Density multiplier to apply to Small objects
        /// </summary>
        [Tooltip("Density multiplier to apply to Small objects")]
        public float m_smallObjDensity = 1;
        public float m_smallObjDensityMaxUI = 10;
        /// <summary>
        /// Density multiplier to apply to Medium objects
        /// </summary>
        [Tooltip("Density multiplier to apply to Medium objects")]
        public float m_mediumObjDensity = 1;
        public float m_mediumObjDensityMaxUI = 10;
        /// <summary>
        /// Density multiplier to apply to Large objects
        /// </summary>
        [Tooltip("Density multiplier to apply to Large objects")]
        public float m_largeObjDensity = 1;
        public float m_largeObjDensityMaxUI = 10;

        [Header("Draw Distances")]
        /// <summary>
        /// Distance to draw Grass objects out to
        /// </summary>
        [Tooltip("Distance to draw Grass objects out to")]
        public float m_grassDistance = 50;
        public float m_grassDistanceMaxUI = 1024;
        /// <summary>
        /// Distance to draw Detail objects out to
        /// </summary>
        [Tooltip("Distance to draw Detail objects out to")]
        public float m_detailDistance = 100;
        public float m_detailDistanceMaxUI = 1024;
        /// <summary>
        /// Distance to draw Plant objects out to
        /// </summary>
        [Tooltip("Distance to draw Plant objects out to")]
        public float m_plantDistance = 200;
        public float m_plantDistanceMaxUI = 2048;
        /// <summary>
        /// Distance to draw Tree objects out to
        /// </summary>
        [Tooltip("Distance to draw Tree objects out to")]
        public float m_treeDistance = 1200;
        public float m_treeDistanceMaxUI = 4096;
        /// <summary>
        /// Distance to draw Small objects out to
        /// </summary>
        [Tooltip("Distance to draw Small objects out to")]
        public float m_smallObjDistance = 50;
        public float m_smallObjDistanceMaxUI = 1024;
        /// <summary>
        /// Distance to draw Medium objects out to
        /// </summary>
        [Tooltip("Distance to draw Medium objects out to")]
        public float m_mediumObjDistance = 100;
        public float m_mediumObjDistanceMaxUI = 2048;
        /// <summary>
        /// Distance to draw Large objects out to
        /// </summary>
        [Tooltip("Distance to draw Large objects out to")]
        public float m_largeObjDistance = 1200;
        public float m_largeObjDistanceMaxUI = 4098;
        /// <summary>
        /// Distance to create Colliders out to
        /// </summary>
        [Tooltip("Distance to create Colliders out to")]
        public float m_colliderDistance = 200;
        public float m_colliderDistanceMaxUI = 1024;

        [Header("Scaleout Multipliers")]
        /// <summary>
        /// Multiplier to apply to all Grass Scaleout values
        /// </summary>
        [Min(0)]
        [Tooltip("Multiplier to apply to all Grass Scaleout values")]
        public float m_grassScaleoutMult = 1;
        public float m_grassScaleoutMultMaxUI = 100;
        /// <summary>
        /// Multiplier to apply to all Detail Scaleout values
        /// </summary>
        [Min(0)]
        [Tooltip("Multiplier to apply to all Detail Scaleout values")]
        public float m_detailScaleoutMult = 1;
        public float m_detailScaleoutMultMaxUI = 100;
        /// <summary>
        /// Multiplier to apply to all Plant Scaleout values
        /// </summary>
        [Min(0)]
        [Tooltip("Multiplier to apply to all Plant Scaleout values")]
        public float m_plantScaleoutMult = 1;
        public float m_plantScaleoutMultMaxUI = 100;
        /// <summary>
        /// Multiplier to apply to all Tree Scaleout values
        /// </summary>
        [Min(0)]
        [Tooltip("Multiplier to apply to all Tree Scaleout values")]
        public float m_treeScaleoutMult = 1;
        public float m_treeScaleoutMultMaxUI = 100;
        /// <summary>
        /// Multiplier to apply to all Small Object Scaleout values
        /// </summary>
        [Min(0)]
        [Tooltip("Multiplier to apply to all Small Object Scaleout values")]
        public float m_smallObjScaleoutMult = 1;
        public float m_smallObjScaleoutMultMaxUI = 100;
        /// <summary>
        /// Multiplier to apply to all Medium Object Scaleout values
        /// </summary>
        [Min(0)]
        [Tooltip("Multiplier to apply to all Medium Object Scaleout values")]
        public float m_mediumObjScaleoutMult = 1;
        public float m_mediumObjScaleoutMultMaxUI = 100;
        /// <summary>
        /// Multiplier to apply to all Large Object Scaleout values
        /// </summary>
        [Min(0)]
        [Tooltip("Multiplier to apply to all Large Object Scaleout values")]
        public float m_largeObjScaleoutMult = 1;
        public float m_largeObjScaleoutMultMaxUI = 100;

        [Header("LOD Mix Multipliers")]
        /// <summary>
        /// Multiplier to apply to all Grass LODMix values
        /// </summary>
        [Min(0)]
        [Tooltip("Multiplier to apply to all Grass LODMix values")]
        public float m_grassLODMixMult = 1;
        public float m_grassLODMixMultMaxUI = 100;
        /// <summary>
        /// Multiplier to apply to all Detail LODMix values
        /// </summary>
        [Min(0)]
        [Tooltip("Multiplier to apply to all Detail LODMix values")]
        public float m_detailLODMixMult = 1;
        public float m_detailLODMixMultMaxUI = 100;
        /// <summary>
        /// Multiplier to apply to all Plant LODMix values
        /// </summary>
        [Min(0)]
        [Tooltip("Multiplier to apply to all Plant LODMix values")]
        public float m_plantLODMixMult = 1;
        public float m_plantLODMixMultMaxUI = 100;
        /// <summary>
        /// Multiplier to apply to all Tree LODMix values
        /// </summary>
        [Min(0)]
        [Tooltip("Multiplier to apply to all Tree LODMix values")]
        public float m_treeLODMixMult = 1;
        public float m_treeLODMixMultMaxUI = 100;
        /// <summary>
        /// Multiplier to apply to all Small Object LODMix values
        /// </summary>
        [Min(0)]
        [Tooltip("Multiplier to apply to all Small Object LODMix values")]
        public float m_smallObjLODMixMult = 1;
        public float m_smallObjLODMixMultMaxUI = 100;
        /// <summary>
        /// Multiplier to apply to all Medium Object LODMix values
        /// </summary>
        [Min(0)]
        [Tooltip("Multiplier to apply to all Medium Object LODMix values")]
        public float m_mediumObjLODMixMult = 1;
        public float m_mediumObjLODMixMultMaxUI = 100;
        /// <summary>
        /// Multiplier to apply to all Large Object LODMix values
        /// </summary>
        [Min(0)]
        [Tooltip("Multiplier to apply to all Large Object LODMix values")]
        public float m_largeObjLODMixMult = 1;
        public float m_largeObjLODMixMultMaxUI = 100;

        [Header("Density Fade Multipliers")]
        /// <summary>
        /// Multiplier to apply to all Grass Density Fade values
        /// </summary>
        [Min(0)]
        [Tooltip("Multiplier to apply to all Grass Density Fade values")]
        public float m_grassDensityFadeMult = 1;
        public float m_grassDensityFadeMultMaxUI = 100;
        /// <summary>
        /// Multiplier to apply to all Detail Density Fade values
        /// </summary>
        [Min(0)]
        [Tooltip("Multiplier to apply to all Detail Density Fade values")]
        public float m_detailDensityFadeMult = 1;
        public float m_detailDensityFadeMultMaxUI = 100;
        /// <summary>
        /// Multiplier to apply to all Plant Density Fade values
        /// </summary>
        [Min(0)]
        [Tooltip("Multiplier to apply to all Plant Density Fade values")]
        public float m_plantDensityFadeMult = 1;
        public float m_plantDensityFadeMultMaxUI = 100;
        /// <summary>
        /// Multiplier to apply to all Tree Density Fade values
        /// </summary>
        [Min(0)]
        [Tooltip("Multiplier to apply to all Tree Density Fade values")]
        public float m_treeDensityFadeMult = 1;
        public float m_treeDensityFadeMultMaxUI = 100;
        /// <summary>
        /// Multiplier to apply to all Small Object Density Fade values
        /// </summary>
        [Min(0)]
        [Tooltip("Multiplier to apply to all Small Object Density Fade values")]
        public float m_smallObjDensityFadeMult = 1;
        public float m_smallObjDensityFadeMultMaxUI = 100;
        /// <summary>
        /// Multiplier to apply to all Medium Object Density Fade values
        /// </summary>
        [Min(0)]
        [Tooltip("Multiplier to apply to all Medium Object Density Fade values")]
        public float m_mediumObjDensityFadeMult = 1;
        public float m_mediumObjDensityFadeMultMaxUI = 100;
        /// <summary>
        /// Multiplier to apply to all Large Object Density Fade values
        /// </summary>
        [Min(0)]
        [Tooltip("Multiplier to apply to all Large Object Density Fade values")]
        public float m_largeObjDensityFadeMult = 1;
        public float m_largeObjDensityFadeMultMaxUI = 100;

        public void CopyFrom(StormQualitySettings settings)
        {
            m_grassDistance = settings.m_grassDistance;
            m_detailDistance = settings.m_detailDistance;
            m_plantDistance = settings.m_plantDistance;
            m_treeDistance = settings.m_treeDistance;
            m_smallObjDistance = settings.m_smallObjDistance;
            m_mediumObjDistance = settings.m_mediumObjDistance;
            m_largeObjDistance = settings.m_largeObjDistance;
            m_colliderDistance = settings.m_colliderDistance;
            m_grassDensity = settings.m_grassDensity;
            m_detailDensity = settings.m_detailDensity;
            m_plantDensity = settings.m_plantDensity;
            m_treeDensity = settings.m_treeDensity;
            m_smallObjDensity = settings.m_smallObjDensity;
            m_mediumObjDensity = settings.m_mediumObjDensity;
            m_largeObjDensity = settings.m_largeObjDensity;
            m_grassDensityFadeMult = settings.m_grassDensityFadeMult;
            m_detailDensityFadeMult = settings.m_detailDensityFadeMult;
            m_plantDensityFadeMult = settings.m_plantDensityFadeMult;
            m_treeDensityFadeMult = settings.m_treeDensityFadeMult;
            m_smallObjDensityFadeMult = settings.m_smallObjDensityFadeMult;
            m_mediumObjDensityFadeMult = settings.m_mediumObjDensityFadeMult;
            m_largeObjDensityFadeMult = settings.m_largeObjDensityFadeMult;
            m_globalLODBias = settings.m_globalLODBias;
            m_grassLODBias = settings.m_grassLODBias;
            m_detailLODBias = settings.m_detailLODBias;
            m_plantLODBias = settings.m_plantLODBias;
            m_treeLODBias = settings.m_treeLODBias;
            m_smallObjLODBias = settings.m_smallObjLODBias;
            m_mediumObjLODBias = settings.m_mediumObjLODBias;
            m_largeObjLODBias = settings.m_largeObjLODBias;
            m_grassLODMixMult = settings.m_grassLODMixMult;
            m_detailLODMixMult = settings.m_detailLODMixMult;
            m_plantLODMixMult = settings.m_plantLODMixMult;
            m_treeLODMixMult = settings.m_treeLODMixMult;
            m_smallObjLODMixMult = settings.m_smallObjLODMixMult;
            m_mediumObjLODMixMult = settings.m_mediumObjLODMixMult;
            m_largeObjLODMixMult = settings.m_largeObjLODMixMult;
            m_grassScaleoutMult = settings.m_grassScaleoutMult;
            m_detailScaleoutMult = settings.m_detailScaleoutMult;
            m_plantScaleoutMult = settings.m_plantScaleoutMult;
            m_treeScaleoutMult = settings.m_treeScaleoutMult;
            m_smallObjScaleoutMult = settings.m_smallObjScaleoutMult;
            m_mediumObjScaleoutMult = settings.m_mediumObjScaleoutMult;
            m_largeObjScaleoutMult = settings.m_largeObjScaleoutMult;
            m_grassDistanceMaxUI = settings.m_grassDistanceMaxUI;
            m_detailDistanceMaxUI = settings.m_detailDistanceMaxUI;
            m_plantDistanceMaxUI = settings.m_plantDistanceMaxUI;
            m_treeDistanceMaxUI = settings.m_treeDistanceMaxUI;
            m_smallObjDistanceMaxUI = settings.m_smallObjDistanceMaxUI;
            m_mediumObjDistanceMaxUI = settings.m_mediumObjDistanceMaxUI;
            m_largeObjDistanceMaxUI = settings.m_largeObjDistanceMaxUI;
            m_colliderDistanceMaxUI = settings.m_colliderDistanceMaxUI;
            m_grassDensityMaxUI = settings.m_grassDensityMaxUI;
            m_detailDensityMaxUI = settings.m_detailDensityMaxUI;
            m_plantDensityMaxUI = settings.m_plantDensityMaxUI;
            m_treeDensityMaxUI = settings.m_treeDensityMaxUI;
            m_smallObjDensityMaxUI = settings.m_smallObjDensityMaxUI;
            m_mediumObjDensityMaxUI = settings.m_mediumObjDensityMaxUI;
            m_largeObjDensityMaxUI = settings.m_largeObjDensityMaxUI;
            m_grassDensityFadeMultMaxUI = settings.m_grassDensityFadeMultMaxUI;
            m_detailDensityFadeMultMaxUI = settings.m_detailDensityFadeMultMaxUI;
            m_plantDensityFadeMultMaxUI = settings.m_plantDensityFadeMultMaxUI;
            m_treeDensityFadeMultMaxUI = settings.m_treeDensityFadeMultMaxUI;
            m_smallObjDensityFadeMultMaxUI = settings.m_smallObjDensityFadeMultMaxUI;
            m_mediumObjDensityFadeMultMaxUI = settings.m_mediumObjDensityFadeMultMaxUI;
            m_largeObjDensityFadeMultMaxUI = settings.m_largeObjDensityFadeMultMaxUI;
            m_globalLODBiasMaxUI = settings.m_globalLODBiasMaxUI;
            m_grassLODBiasMaxUI = settings.m_grassLODBiasMaxUI;
            m_detailLODBiasMaxUI = settings.m_detailLODBiasMaxUI;
            m_plantLODBiasMaxUI = settings.m_plantLODBiasMaxUI;
            m_treeLODBiasMaxUI = settings.m_treeLODBiasMaxUI;
            m_smallObjLODBiasMaxUI = settings.m_smallObjLODBiasMaxUI;
            m_mediumObjLODBiasMaxUI = settings.m_mediumObjLODBiasMaxUI;
            m_largeObjLODBiasMaxUI = settings.m_largeObjLODBiasMaxUI;
            m_grassLODMixMultMaxUI = settings.m_grassLODMixMultMaxUI;
            m_detailLODMixMultMaxUI = settings.m_detailLODMixMultMaxUI;
            m_plantLODMixMultMaxUI = settings.m_plantLODMixMultMaxUI;
            m_treeLODMixMultMaxUI = settings.m_treeLODMixMultMaxUI;
            m_smallObjLODMixMultMaxUI = settings.m_smallObjLODMixMultMaxUI;
            m_mediumObjLODMixMultMaxUI = settings.m_mediumObjLODMixMultMaxUI;
            m_largeObjLODMixMultMaxUI = settings.m_largeObjLODMixMultMaxUI;
            m_grassScaleoutMultMaxUI = settings.m_grassScaleoutMultMaxUI;
            m_detailScaleoutMultMaxUI = settings.m_detailScaleoutMultMaxUI;
            m_plantScaleoutMultMaxUI = settings.m_plantScaleoutMultMaxUI;
            m_treeScaleoutMultMaxUI = settings.m_treeScaleoutMultMaxUI;
            m_smallObjScaleoutMultMaxUI = settings.m_smallObjScaleoutMultMaxUI;
            m_mediumObjScaleoutMultMaxUI = settings.m_mediumObjScaleoutMultMaxUI;
            m_largeObjScaleoutMultMaxUI = settings.m_largeObjScaleoutMultMaxUI;
        }
#pragma warning disable CS0618 // Type or member is obsolete
        public void CopyFrom(StormWorldSettings settings)
#pragma warning restore CS0618 // Type or member is obsolete
        {
            m_grassDistance = settings.m_grassDistance;
            m_detailDistance = settings.m_detailDistance;
            m_plantDistance = settings.m_plantDistance;
            m_treeDistance = settings.m_treeDistance;
            m_smallObjDistance = settings.m_smallObjDistance;
            m_mediumObjDistance = settings.m_mediumObjDistance;
            m_largeObjDistance = settings.m_largeObjDistance;
            m_colliderDistance = settings.m_colliderDistance;
            m_grassDensity = settings.m_grassDensity;
            m_detailDensity = settings.m_detailDensity;
            m_plantDensity = settings.m_plantDensity;
            m_treeDensity = settings.m_treeDensity;
            m_smallObjDensity = settings.m_smallObjDensity;
            m_mediumObjDensity = settings.m_mediumObjDensity;
            m_largeObjDensity = settings.m_largeObjDensity;
            m_grassDensityFadeMult = settings.m_grassDensityFadeMult;
            m_detailDensityFadeMult = settings.m_detailDensityFadeMult;
            m_plantDensityFadeMult = settings.m_plantDensityFadeMult;
            m_treeDensityFadeMult = settings.m_treeDensityFadeMult;
            m_smallObjDensityFadeMult = settings.m_smallObjDensityFadeMult;
            m_mediumObjDensityFadeMult = settings.m_mediumObjDensityFadeMult;
            m_largeObjDensityFadeMult = settings.m_largeObjDensityFadeMult;
            m_globalLODBias = settings.m_globalLODBias;
            m_grassLODBias = settings.m_grassLODBias;
            m_detailLODBias = settings.m_detailLODBias;
            m_plantLODBias = settings.m_plantLODBias;
            m_treeLODBias = settings.m_treeLODBias;
            m_smallObjLODBias = settings.m_smallObjLODBias;
            m_mediumObjLODBias = settings.m_mediumObjLODBias;
            m_largeObjLODBias = settings.m_largeObjLODBias;
            m_grassLODMixMult = settings.m_grassLODMixMult;
            m_detailLODMixMult = settings.m_detailLODMixMult;
            m_plantLODMixMult = settings.m_plantLODMixMult;
            m_treeLODMixMult = settings.m_treeLODMixMult;
            m_smallObjLODMixMult = settings.m_smallObjLODMixMult;
            m_mediumObjLODMixMult = settings.m_mediumObjLODMixMult;
            m_largeObjLODMixMult = settings.m_largeObjLODMixMult;
            m_grassScaleoutMult = settings.m_grassScaleoutMult;
            m_detailScaleoutMult = settings.m_detailScaleoutMult;
            m_plantScaleoutMult = settings.m_plantScaleoutMult;
            m_treeScaleoutMult = settings.m_treeScaleoutMult;
            m_smallObjScaleoutMult = settings.m_smallObjScaleoutMult;
            m_mediumObjScaleoutMult = settings.m_mediumObjScaleoutMult;
            m_largeObjScaleoutMult = settings.m_largeObjScaleoutMult;
            m_grassDistanceMaxUI = settings.m_grassDistanceMaxUI;
            m_detailDistanceMaxUI = settings.m_detailDistanceMaxUI;
            m_plantDistanceMaxUI = settings.m_plantDistanceMaxUI;
            m_treeDistanceMaxUI = settings.m_treeDistanceMaxUI;
            m_smallObjDistanceMaxUI = settings.m_smallObjDistanceMaxUI;
            m_mediumObjDistanceMaxUI = settings.m_mediumObjDistanceMaxUI;
            m_largeObjDistanceMaxUI = settings.m_largeObjDistanceMaxUI;
            m_colliderDistanceMaxUI = settings.m_colliderDistanceMaxUI;
            m_grassDensityMaxUI = settings.m_grassDensityMaxUI;
            m_detailDensityMaxUI = settings.m_detailDensityMaxUI;
            m_plantDensityMaxUI = settings.m_plantDensityMaxUI;
            m_treeDensityMaxUI = settings.m_treeDensityMaxUI;
            m_smallObjDensityMaxUI = settings.m_smallObjDensityMaxUI;
            m_mediumObjDensityMaxUI = settings.m_mediumObjDensityMaxUI;
            m_largeObjDensityMaxUI = settings.m_largeObjDensityMaxUI;
            m_grassDensityFadeMultMaxUI = settings.m_grassDensityFadeMultMaxUI;
            m_detailDensityFadeMultMaxUI = settings.m_detailDensityFadeMultMaxUI;
            m_plantDensityFadeMultMaxUI = settings.m_plantDensityFadeMultMaxUI;
            m_treeDensityFadeMultMaxUI = settings.m_treeDensityFadeMultMaxUI;
            m_smallObjDensityFadeMultMaxUI = settings.m_smallObjDensityFadeMultMaxUI;
            m_mediumObjDensityFadeMultMaxUI = settings.m_mediumObjDensityFadeMultMaxUI;
            m_largeObjDensityFadeMultMaxUI = settings.m_largeObjDensityFadeMultMaxUI;
            m_globalLODBiasMaxUI = settings.m_globalLODBiasMaxUI;
            m_grassLODBiasMaxUI = settings.m_grassLODBiasMaxUI;
            m_detailLODBiasMaxUI = settings.m_detailLODBiasMaxUI;
            m_plantLODBiasMaxUI = settings.m_plantLODBiasMaxUI;
            m_treeLODBiasMaxUI = settings.m_treeLODBiasMaxUI;
            m_smallObjLODBiasMaxUI = settings.m_smallObjLODBiasMaxUI;
            m_mediumObjLODBiasMaxUI = settings.m_mediumObjLODBiasMaxUI;
            m_largeObjLODBiasMaxUI = settings.m_largeObjLODBiasMaxUI;
            m_grassLODMixMultMaxUI = settings.m_grassLODMixMultMaxUI;
            m_detailLODMixMultMaxUI = settings.m_detailLODMixMultMaxUI;
            m_plantLODMixMultMaxUI = settings.m_plantLODMixMultMaxUI;
            m_treeLODMixMultMaxUI = settings.m_treeLODMixMultMaxUI;
            m_smallObjLODMixMultMaxUI = settings.m_smallObjLODMixMultMaxUI;
            m_mediumObjLODMixMultMaxUI = settings.m_mediumObjLODMixMultMaxUI;
            m_largeObjLODMixMultMaxUI = settings.m_largeObjLODMixMultMaxUI;
            m_grassScaleoutMultMaxUI = settings.m_grassScaleoutMultMaxUI;
            m_detailScaleoutMultMaxUI = settings.m_detailScaleoutMultMaxUI;
            m_plantScaleoutMultMaxUI = settings.m_plantScaleoutMultMaxUI;
            m_treeScaleoutMultMaxUI = settings.m_treeScaleoutMultMaxUI;
            m_smallObjScaleoutMultMaxUI = settings.m_smallObjScaleoutMultMaxUI;
            m_mediumObjScaleoutMultMaxUI = settings.m_mediumObjScaleoutMultMaxUI;
            m_largeObjScaleoutMultMaxUI = settings.m_largeObjScaleoutMultMaxUI;
        }
    }
}