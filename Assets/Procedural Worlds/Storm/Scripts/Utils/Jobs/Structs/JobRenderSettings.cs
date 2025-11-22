using Unity.Mathematics;

/// Copyright 2023 Procedural Worlds Ltd
/// Written by Jonathan Williams as part of Storm

namespace ProceduralWorlds.Storm.Utils
{
    public struct JobRenderSettings
    {
        public float3 ItemBoundsCenter;
        public float3 ItemBoundsExtents;

        public int NumLODs;
        public float LODFade;
        public float LODbias;
        public float LODMix;
        public float LODSplits_1;
        public float LODSplits_2;
        public float LODSplits_3;
        public float LODSplits_4;

        public float DensityFadeStart;
        public float DensityFadeStrength;
        public float DensityFadeScaleoutRange;
        public float ScaleoutRange;
        public float ScaleoutMinScale;
        public float LongShadowDist;
        public float MaxLODRange;
    }
}