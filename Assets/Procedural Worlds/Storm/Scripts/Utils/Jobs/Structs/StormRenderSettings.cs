using Unity.Mathematics;

/// Copyright 2023 Procedural Worlds Ltd
/// Written by Jonathan Williams as part of Storm

namespace ProceduralWorlds.Storm.Utils
{
    /// <summary>
    /// Contains Settings used in render jobs for a Feature
    /// </summary>
    public struct StormRenderSettings
    {
        public float LODFade;
        public float LODbias;
        public float LODMix;
        public float4 LODSplits;
        public float DensityFadeStart;
        public float DensityFadeStrength;
        public float DensityFadeScaleoutRange;
        public float ScaleoutRange;
        public float ScaleoutMinScale;
        public float MaxShadowDist;
    }
}