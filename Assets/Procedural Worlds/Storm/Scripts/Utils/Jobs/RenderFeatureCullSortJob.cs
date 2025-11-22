using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

/// Copyright 2023 Procedural Worlds Ltd
/// Written by Jonathan Williams as part of Storm

namespace ProceduralWorlds.Storm.Utils
{
    /// <summary>
    /// Checks Instances vs ViewFrustrum and splits into LODs for rendering
    /// </summary>
    [BurstCompile]
#if UNITY_2022_2_OR_NEWER
    [GenerateTestsForBurstCompatibility(CompileTarget = GenerateTestsForBurstCompatibilityAttribute.BurstCompatibleCompileTarget.PlayerAndEditor)]
#else
    [BurstCompatible(CompileTarget = BurstCompatibleAttribute.BurstCompatibleCompileTarget.PlayerAndEditor)]
#endif
    public struct RenderFeatureCullSortJob : IJob
    {
        public RenderFeatureCullSortJob(NativeArray<StormItemData> itemList, NativeArray<int2> lodCounts)
        {
            Mode = StormCombineType.STANDARD;
            ItemList = itemList;
            LODCounts = lodCounts;
            ShadowsOnly = false;
            FullyVisible = false;
            CameraData = default;
            RenderSettings = default;
            ShadowDistance = -1;
            SplitShadows = false;
        }
        #region Variables
        /// <summary>
        /// NativeArray containing shared Camera Data for jobs
        /// </summary>
        [ReadOnly]
        public NativeArray<JobCameraData> CameraData;
        /// <summary>
        /// NativeArray containing shared Render Settings for a Feature
        /// </summary>
        [ReadOnly]
        public NativeArray<JobRenderSettings> RenderSettings;

        public bool FullyVisible;

        public bool ShadowsOnly;
        /// <summary>
        /// NativeArray containing list of instances on a Render Cell
        /// </summary>
        public NativeArray<StormItemData> ItemList;
        public NativeArray<int2> LODCounts;

        public bool SplitShadows;
        public StormCombineType Mode;
        public float ShadowDistance;
        #endregion
        public void Execute()
        {
            JobRenderSettings renderSettings = RenderSettings[0];
            JobCameraData camData = CameraData[0];
            bool doShadowCheck = math.any(camData.LightDir);
            int numItems = ItemList.Length;
            for (int i = 0; i < numItems; ++i)
            {
                StormItemData item = ItemList[i];
                //get distance ... set normalized distance in ExtraData.y with modifications
                //check visibility
                //reorder item if needed ... LOD0Visible, LOD0Hidden, LOD1Visible, ...
            }
        }
    }
}