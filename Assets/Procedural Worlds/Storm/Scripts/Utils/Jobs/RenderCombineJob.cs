using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

/// Copyright 2023 Procedural Worlds Ltd
/// Written by Jonathan Williams as part of Storm

namespace ProceduralWorlds.Storm.Utils
{
    /// <summary>
    /// Checks Instances vs ViewFrustrum and splits into LODs for rendering
    /// </summary>
    [BurstCompile(FloatMode = FloatMode.Fast, FloatPrecision = FloatPrecision.Low, OptimizeFor = OptimizeFor.Performance)]
#if UNITY_2022_2_OR_NEWER
    [GenerateTestsForBurstCompatibility(CompileTarget = GenerateTestsForBurstCompatibilityAttribute.BurstCompatibleCompileTarget.PlayerAndEditor)]
#else
    [BurstCompatible(CompileTarget = BurstCompatibleAttribute.BurstCompatibleCompileTarget.PlayerAndEditor)]
#endif
    public struct RenderCombineJob : IJobParallelFor
    {
        public RenderCombineJob(NativeArray<StormItemData> BlankItemList, NativeArray<StormSubgridData> BlankSubgridList)
        {
            Mode = StormCombineType.STANDARD;
            Offset0 = float3.zero;
            ItemList0 = BlankItemList;
            Subgrids0 = BlankSubgridList;
            UseSubgrids0 = false;
            ShadowsOnly0 = false;
            FullyVisible0 = false;
            Offset1 = float3.zero;
            ItemList1 = BlankItemList;
            Subgrids1 = BlankSubgridList;
            UseSubgrids1 = false;
            ShadowsOnly1 = false;
            FullyVisible1 = false;
            Offset2 = float3.zero;
            ItemList2 = BlankItemList;
            Subgrids2 = BlankSubgridList;
            UseSubgrids2 = false;
            ShadowsOnly2 = false;
            FullyVisible2 = false;
            Offset3 = float3.zero;
            ItemList3 = BlankItemList;
            Subgrids3 = BlankSubgridList;
            UseSubgrids3 = false;
            ShadowsOnly3 = false;
            FullyVisible3 = false;
            Offset4 = float3.zero;
            ItemList4 = BlankItemList;
            Subgrids4 = BlankSubgridList;
            UseSubgrids4 = false;
            ShadowsOnly4 = false;
            FullyVisible4 = false;
            Offset5 = float3.zero;
            ItemList5 = BlankItemList;
            Subgrids5 = BlankSubgridList;
            UseSubgrids5 = false;
            ShadowsOnly5 = false;
            FullyVisible5 = false;
            Offset6 = float3.zero;
            ItemList6 = BlankItemList;
            Subgrids6 = BlankSubgridList;
            UseSubgrids6 = false;
            ShadowsOnly6 = false;
            FullyVisible6 = false;
            Offset7 = float3.zero;
            ItemList7 = BlankItemList;
            Subgrids7 = BlankSubgridList;
            UseSubgrids7 = false;
            ShadowsOnly7 = false;
            FullyVisible7 = false;
            Offset8 = float3.zero;
            ItemList8 = BlankItemList;
            Subgrids8 = BlankSubgridList;
            UseSubgrids8 = false;
            ShadowsOnly8 = false;
            FullyVisible8 = false;
            Offset9 = float3.zero;
            ItemList9 = BlankItemList;
            Subgrids9 = BlankSubgridList;
            UseSubgrids9 = false;
            ShadowsOnly9 = false;
            FullyVisible9 = false;
            Offset10 = float3.zero;
            ItemList10 = BlankItemList;
            Subgrids10 = BlankSubgridList;
            UseSubgrids10 = false;
            ShadowsOnly10 = false;
            FullyVisible10 = false;
            Offset11 = float3.zero;
            ItemList11 = BlankItemList;
            Subgrids11 = BlankSubgridList;
            UseSubgrids11 = false;
            ShadowsOnly11 = false;
            FullyVisible11 = false;
            Offset12 = float3.zero;
            ItemList12 = BlankItemList;
            Subgrids12 = BlankSubgridList;
            UseSubgrids12 = false;
            ShadowsOnly12 = false;
            FullyVisible12 = false;
            Offset13 = float3.zero;
            ItemList13 = BlankItemList;
            Subgrids13 = BlankSubgridList;
            UseSubgrids13 = false;
            ShadowsOnly13 = false;
            FullyVisible13 = false;
            Offset14 = float3.zero;
            ItemList14 = BlankItemList;
            Subgrids14 = BlankSubgridList;
            UseSubgrids14 = false;
            ShadowsOnly14 = false;
            FullyVisible14 = false;
            Offset15 = float3.zero;
            ItemList15 = BlankItemList;
            Subgrids15 = BlankSubgridList;
            UseSubgrids15 = false;
            ShadowsOnly15 = false;
            FullyVisible15 = false;
            Offset16 = float3.zero;
            ItemList16 = BlankItemList;
            Subgrids16 = BlankSubgridList;
            UseSubgrids16 = false;
            ShadowsOnly16 = false;
            FullyVisible16 = false;
            Offset17 = float3.zero;
            ItemList17 = BlankItemList;
            Subgrids17 = BlankSubgridList;
            UseSubgrids17 = false;
            ShadowsOnly17 = false;
            FullyVisible17 = false;
            Offset18 = float3.zero;
            ItemList18 = BlankItemList;
            Subgrids18 = BlankSubgridList;
            UseSubgrids18 = false;
            ShadowsOnly18 = false;
            FullyVisible18 = false;
            Offset19 = float3.zero;
            ItemList19 = BlankItemList;
            Subgrids19 = BlankSubgridList;
            UseSubgrids19 = false;
            ShadowsOnly19 = false;
            FullyVisible19 = false;
            CameraData = default;
            RenderSettings = default;
            OutputLOD0 = default;
            OutputLOD1 = default;
            OutputLOD2 = default;
            OutputLOD3 = default;
            OutputLOD4 = default;
            OutputShadows = default;
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
        #region Selection Variables
        public float3 Offset0;
        public float3 Offset1;
        public float3 Offset2;
        public float3 Offset3;
        public float3 Offset4;
        public float3 Offset5;
        public float3 Offset6;
        public float3 Offset7;
        public float3 Offset8;
        public float3 Offset9;
        public float3 Offset10;
        public float3 Offset11;
        public float3 Offset12;
        public float3 Offset13;
        public float3 Offset14;
        public float3 Offset15;
        public float3 Offset16;
        public float3 Offset17;
        public float3 Offset18;
        public float3 Offset19;

        public bool FullyVisible0;
        public bool FullyVisible1;
        public bool FullyVisible2;
        public bool FullyVisible3;
        public bool FullyVisible4;
        public bool FullyVisible5;
        public bool FullyVisible6;
        public bool FullyVisible7;
        public bool FullyVisible8;
        public bool FullyVisible9;
        public bool FullyVisible10;
        public bool FullyVisible11;
        public bool FullyVisible12;
        public bool FullyVisible13;
        public bool FullyVisible14;
        public bool FullyVisible15;
        public bool FullyVisible16;
        public bool FullyVisible17;
        public bool FullyVisible18;
        public bool FullyVisible19;

        public bool ShadowsOnly0;
        public bool ShadowsOnly1;
        public bool ShadowsOnly2;
        public bool ShadowsOnly3;
        public bool ShadowsOnly4;
        public bool ShadowsOnly5;
        public bool ShadowsOnly6;
        public bool ShadowsOnly7;
        public bool ShadowsOnly8;
        public bool ShadowsOnly9;
        public bool ShadowsOnly10;
        public bool ShadowsOnly11;
        public bool ShadowsOnly12;
        public bool ShadowsOnly13;
        public bool ShadowsOnly14;
        public bool ShadowsOnly15;
        public bool ShadowsOnly16;
        public bool ShadowsOnly17;
        public bool ShadowsOnly18;
        public bool ShadowsOnly19;

        public bool UseSubgrids0;
        public bool UseSubgrids1;
        public bool UseSubgrids2;
        public bool UseSubgrids3;
        public bool UseSubgrids4;
        public bool UseSubgrids5;
        public bool UseSubgrids6;
        public bool UseSubgrids7;
        public bool UseSubgrids8;
        public bool UseSubgrids9;
        public bool UseSubgrids10;
        public bool UseSubgrids11;
        public bool UseSubgrids12;
        public bool UseSubgrids13;
        public bool UseSubgrids14;
        public bool UseSubgrids15;
        public bool UseSubgrids16;
        public bool UseSubgrids17;
        public bool UseSubgrids18;
        public bool UseSubgrids19;

        [ReadOnly]
        public NativeArray<StormItemData> ItemList0;
        [ReadOnly]
        public NativeArray<StormItemData> ItemList1;
        [ReadOnly]
        public NativeArray<StormItemData> ItemList2;
        [ReadOnly]
        public NativeArray<StormItemData> ItemList3;
        [ReadOnly]
        public NativeArray<StormItemData> ItemList4;
        [ReadOnly]
        public NativeArray<StormItemData> ItemList5;
        [ReadOnly]
        public NativeArray<StormItemData> ItemList6;
        [ReadOnly]
        public NativeArray<StormItemData> ItemList7;
        [ReadOnly]
        public NativeArray<StormItemData> ItemList8;
        [ReadOnly]
        public NativeArray<StormItemData> ItemList9;
        [ReadOnly]
        public NativeArray<StormItemData> ItemList10;
        [ReadOnly]
        public NativeArray<StormItemData> ItemList11;
        [ReadOnly]
        public NativeArray<StormItemData> ItemList12;
        [ReadOnly]
        public NativeArray<StormItemData> ItemList13;
        [ReadOnly]
        public NativeArray<StormItemData> ItemList14;
        [ReadOnly]
        public NativeArray<StormItemData> ItemList15;
        [ReadOnly]
        public NativeArray<StormItemData> ItemList16;
        [ReadOnly]
        public NativeArray<StormItemData> ItemList17;
        [ReadOnly]
        public NativeArray<StormItemData> ItemList18;
        [ReadOnly]
        public NativeArray<StormItemData> ItemList19;


        [ReadOnly]
        public NativeArray<StormSubgridData> Subgrids0;
        [ReadOnly]
        public NativeArray<StormSubgridData> Subgrids1;
        [ReadOnly]
        public NativeArray<StormSubgridData> Subgrids2;
        [ReadOnly]
        public NativeArray<StormSubgridData> Subgrids3;
        [ReadOnly]
        public NativeArray<StormSubgridData> Subgrids4;
        [ReadOnly]
        public NativeArray<StormSubgridData> Subgrids5;
        [ReadOnly]
        public NativeArray<StormSubgridData> Subgrids6;
        [ReadOnly]
        public NativeArray<StormSubgridData> Subgrids7;
        [ReadOnly]
        public NativeArray<StormSubgridData> Subgrids8;
        [ReadOnly]
        public NativeArray<StormSubgridData> Subgrids9;
        [ReadOnly]
        public NativeArray<StormSubgridData> Subgrids10;
        [ReadOnly]
        public NativeArray<StormSubgridData> Subgrids11;
        [ReadOnly]
        public NativeArray<StormSubgridData> Subgrids12;
        [ReadOnly]
        public NativeArray<StormSubgridData> Subgrids13;
        [ReadOnly]
        public NativeArray<StormSubgridData> Subgrids14;
        [ReadOnly]
        public NativeArray<StormSubgridData> Subgrids15;
        [ReadOnly]
        public NativeArray<StormSubgridData> Subgrids16;
        [ReadOnly]
        public NativeArray<StormSubgridData> Subgrids17;
        [ReadOnly]
        public NativeArray<StormSubgridData> Subgrids18;
        [ReadOnly]
        public NativeArray<StormSubgridData> Subgrids19;
        #endregion

        [WriteOnly]
        public NativeList<StormItemData>.ParallelWriter OutputLOD0;
        [WriteOnly]
        public NativeList<StormItemData>.ParallelWriter OutputLOD1;
        [WriteOnly]
        public NativeList<StormItemData>.ParallelWriter OutputLOD2;
        [WriteOnly]
        public NativeList<StormItemData>.ParallelWriter OutputLOD3;
        [WriteOnly]
        public NativeList<StormItemData>.ParallelWriter OutputLOD4;
        [WriteOnly]
        public NativeList<StormItemData>.ParallelWriter OutputShadows;

        public bool SplitShadows;
        public StormCombineType Mode;
        public float ShadowDistance;
        #endregion
        public void Execute(int idx)
        {
            if (Mode == StormCombineType.SIMPLE)
                ExecuteSimple(idx);
            else if (Mode == StormCombineType.NO_CULL)
                ExecuteNoCull(idx);
            else
                ExecuteFull(idx);
        }
        public void ExecuteFull(int l)
        {
            JobRenderSettings renderSettings = RenderSettings[0];
            JobCameraData camData = CameraData[0];
            float3 shadowBoundsCenter = float3.zero;
            float3 shadowBoundsExtents = float3.zero;
            bool doShadowCheck = math.any(camData.LightDir);
            if (doShadowCheck)
            {
                float d = 1 - math.dot(camData.LightDir, math.float3(0, -1, 0));
                JobCameraData.ExpandBounds(renderSettings.ItemBoundsExtents, camData.LightDir, math.length(renderSettings.ItemBoundsExtents) * 10 * d, out shadowBoundsCenter, out shadowBoundsExtents);
            }
            //for (int l = 0; l < 20; ++l)
            {
                #region Variable Selection
                NativeArray<StormItemData> iList = l switch {
                    0 => ItemList0,
                    1 => ItemList1,
                    2 => ItemList2,
                    3 => ItemList3,
                    4 => ItemList4,
                    5 => ItemList5,
                    6 => ItemList6,
                    7 => ItemList7,
                    8 => ItemList8,
                    9 => ItemList9,
                    10 => ItemList10,
                    11 => ItemList11,
                    12 => ItemList12,
                    13 => ItemList13,
                    14 => ItemList14,
                    15 => ItemList15,
                    16 => ItemList16,
                    17 => ItemList17,
                    18 => ItemList18,
                    19 => ItemList19,
                    _ => ItemList0,
                };
                float3 offset = l switch {
                    0 => Offset0,
                    1 => Offset1,
                    2 => Offset2,
                    3 => Offset3,
                    4 => Offset4,
                    5 => Offset5,
                    6 => Offset6,
                    7 => Offset7,
                    8 => Offset8,
                    9 => Offset9,
                    10 => Offset10,
                    11 => Offset11,
                    12 => Offset12,
                    13 => Offset13,
                    14 => Offset14,
                    15 => Offset15,
                    16 => Offset16,
                    17 => Offset17,
                    18 => Offset18,
                    19 => Offset19,
                    _ => Offset0,
                };
                bool fullyVisible = l switch {
                    0 => FullyVisible0,
                    1 => FullyVisible1,
                    2 => FullyVisible2,
                    3 => FullyVisible3,
                    4 => FullyVisible4,
                    5 => FullyVisible5,
                    6 => FullyVisible6,
                    7 => FullyVisible7,
                    8 => FullyVisible8,
                    9 => FullyVisible9,
                    10 => FullyVisible10,
                    11 => FullyVisible11,
                    12 => FullyVisible12,
                    13 => FullyVisible13,
                    14 => FullyVisible14,
                    15 => FullyVisible15,
                    16 => FullyVisible16,
                    17 => FullyVisible17,
                    18 => FullyVisible18,
                    19 => FullyVisible19,
                    _ => FullyVisible0,
                };
                bool shadowsOnly = l switch {
                    0 => ShadowsOnly0,
                    1 => ShadowsOnly1,
                    2 => ShadowsOnly2,
                    3 => ShadowsOnly3,
                    4 => ShadowsOnly4,
                    5 => ShadowsOnly5,
                    6 => ShadowsOnly6,
                    7 => ShadowsOnly7,
                    8 => ShadowsOnly8,
                    9 => ShadowsOnly9,
                    10 => ShadowsOnly10,
                    11 => ShadowsOnly11,
                    12 => ShadowsOnly12,
                    13 => ShadowsOnly13,
                    14 => ShadowsOnly14,
                    15 => ShadowsOnly15,
                    16 => ShadowsOnly16,
                    17 => ShadowsOnly17,
                    18 => ShadowsOnly18,
                    19 => ShadowsOnly19,
                    _ => ShadowsOnly0,
                };
                bool useSubgrids = l switch {
                    0 => UseSubgrids0,
                    1 => UseSubgrids1,
                    2 => UseSubgrids2,
                    3 => UseSubgrids3,
                    4 => UseSubgrids4,
                    5 => UseSubgrids5,
                    6 => UseSubgrids6,
                    7 => UseSubgrids7,
                    8 => UseSubgrids8,
                    9 => UseSubgrids9,
                    10 => UseSubgrids10,
                    11 => UseSubgrids11,
                    12 => UseSubgrids12,
                    13 => UseSubgrids13,
                    14 => UseSubgrids14,
                    15 => UseSubgrids15,
                    16 => UseSubgrids16,
                    17 => UseSubgrids17,
                    18 => UseSubgrids18,
                    19 => UseSubgrids19,
                    _ => UseSubgrids0,
                };
                NativeArray<StormSubgridData> Subgrids = l switch {
                    0 => Subgrids0,
                    1 => Subgrids1,
                    2 => Subgrids2,
                    3 => Subgrids3,
                    4 => Subgrids4,
                    5 => Subgrids5,
                    6 => Subgrids6,
                    7 => Subgrids7,
                    8 => Subgrids8,
                    9 => Subgrids9,
                    10 => Subgrids10,
                    11 => Subgrids11,
                    12 => Subgrids12,
                    13 => Subgrids13,
                    14 => Subgrids14,
                    15 => Subgrids15,
                    16 => Subgrids16,
                    17 => Subgrids17,
                    18 => Subgrids18,
                    19 => Subgrids19,
                    _ => Subgrids0,
                };
                #endregion
                int len = iList.Length;
                if (useSubgrids && !fullyVisible)
                {
                    int numSubgrids = Subgrids.Length;
                    int indexOffset = 0;
                    for (int sg = 0; sg < numSubgrids; ++sg)
                    {
                        StormSubgridData gridData = Subgrids[sg];
                        if (gridData.ItemCount < 1)
                            continue;

                        float3 bExtents = (gridData.Bounds.c1 - gridData.Bounds.c0) * 0.5f;
                        float3 bCenter = (gridData.Bounds.c0 + bExtents) + offset;
                        float dist = math.length(math.clamp(camData.CamPos, bCenter - bExtents, bCenter + bExtents) - camData.CamPos);
                        NativeSlice<StormItemData> data = iList.Slice(indexOffset, gridData.ItemCount);

                        if (!shadowsOnly && camData.IsAABBVisible(bCenter, bExtents, dist, out bool _))
                        {
                            for (int index = 0; index < gridData.ItemCount; ++index)
                            {
                                StormItemData item = data[index];
                                item.Translation = math.float3(item.Translation.x * camData.CellSize + offset.x, item.Translation.y + offset.y, item.Translation.z * camData.CellSize + offset.z);
                                //float bRadius = math.length(renderSettings.ItemBoundsExtents * item.Scale);
                                dist = math.length(item.Translation - camData.CamPos);
                                float distNorm = math.unlerp(0, renderSettings.MaxLODRange, dist);
                                if (DistUpdates(ref renderSettings, distNorm, ref item))
                                {
                                    if (SplitShadows && dist < ShadowDistance)
                                        AddShadows(item);
                                    dist = math.clamp(math.pow(distNorm, renderSettings.LODbias) + (renderSettings.LODMix * (2 * (item.ExtraData.y - 0.5f))), 0, 1);
                                    CheckOutput(ref renderSettings, ref item, dist, -1, renderSettings.LODSplits_1, OutputLOD0);
                                    if (renderSettings.NumLODs > 1)
                                        CheckOutput(ref renderSettings, ref item, dist, renderSettings.LODSplits_1, renderSettings.LODSplits_2, OutputLOD1);
                                    if (renderSettings.NumLODs > 2)
                                        CheckOutput(ref renderSettings, ref item, dist, renderSettings.LODSplits_2, renderSettings.LODSplits_3, OutputLOD2);
                                    if (renderSettings.NumLODs > 3)
                                        CheckOutput(ref renderSettings, ref item, dist, renderSettings.LODSplits_3, renderSettings.LODSplits_4, OutputLOD3);
                                    if (renderSettings.NumLODs > 4)
                                        CheckOutput(ref renderSettings, ref item, dist, renderSettings.LODSplits_4, 2, OutputLOD4);
                                }
                            }
                        }
                        else
                        {
                            if (doShadowCheck && dist < ShadowDistance)
                            {
                                if ((renderSettings.LongShadowDist < 0 || dist < renderSettings.LongShadowDist) &&
                                    (camData.IsAABBVisible(bCenter + shadowBoundsCenter, shadowBoundsExtents, dist, out bool _)))
                                {
                                    for (int index = 0; index < gridData.ItemCount; ++index)
                                    {
                                        StormItemData item = data[index];
                                        item.Translation = math.float3(item.Translation.x * camData.CellSize + offset.x, item.Translation.y + offset.y, item.Translation.z * camData.CellSize + offset.z);
                                        //float bRadius = math.length(renderSettings.ItemBoundsExtents * item.Scale);
                                        dist = math.length(item.Translation - camData.CamPos);
                                        float distNorm = math.unlerp(0, renderSettings.MaxLODRange, dist);
                                        if (DistUpdates(ref renderSettings, distNorm, ref item))
                                        {
                                            AddShadows(item);
                                        }
                                    }
                                }
                            }
                        }
                        indexOffset += gridData.ItemCount;
                    }
                }
                else
                {
                    for (int index = 0; index < len; ++index)
                    {
                        StormItemData item = iList[index];
                        item.Translation = math.float3(item.Translation.x * camData.CellSize + offset.x, item.Translation.y + offset.y, item.Translation.z * camData.CellSize + offset.z);
                        float3 bCenter = math.rotate(item.Rotation, renderSettings.ItemBoundsCenter * item.Scale) + item.Translation;
                        float3 bExtents = renderSettings.ItemBoundsExtents * item.Scale;
                        //float bRadius = math.length(renderSettings.ItemBoundsExtents * item.Scale);
                        float dist = math.length(math.clamp(camData.CamPos, bCenter - bExtents, bCenter + bExtents) - camData.CamPos);
                        float distNorm = math.unlerp(0, renderSettings.MaxLODRange, dist);

                        if (fullyVisible || (!shadowsOnly && camData.IsAABBVisible(bCenter, bExtents, dist, out bool _)))
                        {
                            if (DistUpdates(ref renderSettings, distNorm, ref item))
                            {
                                if (SplitShadows && dist < ShadowDistance)
                                    AddShadows(item);
                                dist = math.clamp(math.pow(distNorm, renderSettings.LODbias) + (renderSettings.LODMix * (2 * (item.ExtraData.y - 0.5f))), 0, 1);
                                CheckOutput(ref renderSettings, ref item, dist, -1, renderSettings.LODSplits_1, OutputLOD0);
                                if (renderSettings.NumLODs > 1)
                                    CheckOutput(ref renderSettings, ref item, dist, renderSettings.LODSplits_1, renderSettings.LODSplits_2, OutputLOD1);
                                if (renderSettings.NumLODs > 2)
                                    CheckOutput(ref renderSettings, ref item, dist, renderSettings.LODSplits_2, renderSettings.LODSplits_3, OutputLOD2);
                                if (renderSettings.NumLODs > 3)
                                    CheckOutput(ref renderSettings, ref item, dist, renderSettings.LODSplits_3, renderSettings.LODSplits_4, OutputLOD3);
                                if (renderSettings.NumLODs > 4)
                                    CheckOutput(ref renderSettings, ref item, dist, renderSettings.LODSplits_4, 2, OutputLOD4);
                            }
                        }
                        else
                        {
                            if (doShadowCheck && dist < ShadowDistance)
                            {
                                if ((renderSettings.LongShadowDist < 0 || dist < renderSettings.LongShadowDist) &&
                                    (camData.IsAABBVisible(item.Translation + shadowBoundsCenter, shadowBoundsExtents * math.length(item.Scale), dist, out bool _)))
                                {
                                    if (DistUpdates(ref renderSettings, distNorm, ref item))
                                    {
                                        AddShadows(item);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        public void ExecuteSimple(int l)
        {
            JobRenderSettings renderSettings = RenderSettings[0];
            JobCameraData camData = CameraData[0];
            float3 shadowBoundsCenter = float3.zero;
            float3 shadowBoundsExtents = float3.zero;
            bool doShadowCheck = math.any(camData.LightDir);
            if (doShadowCheck)
            {
                float d = 1 - math.dot(camData.LightDir, math.float3(0, -1, 0));
                JobCameraData.ExpandBounds(renderSettings.ItemBoundsExtents, camData.LightDir, math.length(renderSettings.ItemBoundsExtents) * 10 * d, out shadowBoundsCenter, out shadowBoundsExtents);
            }
            float boundsSize = math.length(math.abs(renderSettings.ItemBoundsCenter) + renderSettings.ItemBoundsExtents);
            float shadowSize = math.length(shadowBoundsExtents);
            //for (int l = 0; l < 20; ++l)
            {
                #region Variable Selection
                NativeArray<StormItemData> iList = l switch {
                    0 => ItemList0,
                    1 => ItemList1,
                    2 => ItemList2,
                    3 => ItemList3,
                    4 => ItemList4,
                    5 => ItemList5,
                    6 => ItemList6,
                    7 => ItemList7,
                    8 => ItemList8,
                    9 => ItemList9,
                    10 => ItemList10,
                    11 => ItemList11,
                    12 => ItemList12,
                    13 => ItemList13,
                    14 => ItemList14,
                    15 => ItemList15,
                    16 => ItemList16,
                    17 => ItemList17,
                    18 => ItemList18,
                    19 => ItemList19,
                    _ => ItemList0,
                };
                float3 offset = l switch {
                    0 => Offset0,
                    1 => Offset1,
                    2 => Offset2,
                    3 => Offset3,
                    4 => Offset4,
                    5 => Offset5,
                    6 => Offset6,
                    7 => Offset7,
                    8 => Offset8,
                    9 => Offset9,
                    10 => Offset10,
                    11 => Offset11,
                    12 => Offset12,
                    13 => Offset13,
                    14 => Offset14,
                    15 => Offset15,
                    16 => Offset16,
                    17 => Offset17,
                    18 => Offset18,
                    19 => Offset19,
                    _ => Offset0,
                };
                bool fullyVisible = l switch {
                    0 => FullyVisible0,
                    1 => FullyVisible1,
                    2 => FullyVisible2,
                    3 => FullyVisible3,
                    4 => FullyVisible4,
                    5 => FullyVisible5,
                    6 => FullyVisible6,
                    7 => FullyVisible7,
                    8 => FullyVisible8,
                    9 => FullyVisible9,
                    10 => FullyVisible10,
                    11 => FullyVisible11,
                    12 => FullyVisible12,
                    13 => FullyVisible13,
                    14 => FullyVisible14,
                    15 => FullyVisible15,
                    16 => FullyVisible16,
                    17 => FullyVisible17,
                    18 => FullyVisible18,
                    19 => FullyVisible19,
                    _ => FullyVisible0,
                };
                bool shadowsOnly = l switch {
                    0 => ShadowsOnly0,
                    1 => ShadowsOnly1,
                    2 => ShadowsOnly2,
                    3 => ShadowsOnly3,
                    4 => ShadowsOnly4,
                    5 => ShadowsOnly5,
                    6 => ShadowsOnly6,
                    7 => ShadowsOnly7,
                    8 => ShadowsOnly8,
                    9 => ShadowsOnly9,
                    10 => ShadowsOnly10,
                    11 => ShadowsOnly11,
                    12 => ShadowsOnly12,
                    13 => ShadowsOnly13,
                    14 => ShadowsOnly14,
                    15 => ShadowsOnly15,
                    16 => ShadowsOnly16,
                    17 => ShadowsOnly17,
                    18 => ShadowsOnly18,
                    19 => ShadowsOnly19,
                    _ => ShadowsOnly0,
                };
                bool useSubgrids = l switch {
                    0 => UseSubgrids0,
                    1 => UseSubgrids1,
                    2 => UseSubgrids2,
                    3 => UseSubgrids3,
                    4 => UseSubgrids4,
                    5 => UseSubgrids5,
                    6 => UseSubgrids6,
                    7 => UseSubgrids7,
                    8 => UseSubgrids8,
                    9 => UseSubgrids9,
                    10 => UseSubgrids10,
                    11 => UseSubgrids11,
                    12 => UseSubgrids12,
                    13 => UseSubgrids13,
                    14 => UseSubgrids14,
                    15 => UseSubgrids15,
                    16 => UseSubgrids16,
                    17 => UseSubgrids17,
                    18 => UseSubgrids18,
                    19 => UseSubgrids19,
                    _ => UseSubgrids0,
                };
                NativeArray<StormSubgridData> Subgrids = l switch {
                    0 => Subgrids0,
                    1 => Subgrids1,
                    2 => Subgrids2,
                    3 => Subgrids3,
                    4 => Subgrids4,
                    5 => Subgrids5,
                    6 => Subgrids6,
                    7 => Subgrids7,
                    8 => Subgrids8,
                    9 => Subgrids9,
                    10 => Subgrids10,
                    11 => Subgrids11,
                    12 => Subgrids12,
                    13 => Subgrids13,
                    14 => Subgrids14,
                    15 => Subgrids15,
                    16 => Subgrids16,
                    17 => Subgrids17,
                    18 => Subgrids18,
                    19 => Subgrids19,
                    _ => Subgrids0,
                };
                #endregion
                int len = iList.Length;
                if (useSubgrids && !fullyVisible)
                {
                    int numSubgrids = Subgrids.Length;
                    int indexOffset = 0;
                    for (int sg = 0; sg < numSubgrids; ++sg)
                    {
                        StormSubgridData gridData = Subgrids[sg];

                        float3 bExtents = (gridData.Bounds.c1 - gridData.Bounds.c0) * 0.5f;
                        float3 bCenter = (gridData.Bounds.c0 + bExtents) + offset;
                        float dist = math.length(math.clamp(camData.CamPos, bCenter - bExtents, bCenter + bExtents) - camData.CamPos);
                        NativeSlice<StormItemData> data = iList.Slice(indexOffset, gridData.ItemCount);

                        if (!shadowsOnly && camData.IsSphereVisible(bCenter, math.length(bExtents), dist))
                        {
                            for (int index = 0; index < gridData.ItemCount; ++index)
                            {
                                StormItemData item = data[index];
                                item.Translation = math.float3(item.Translation.x * camData.CellSize + offset.x, item.Translation.y + offset.y, item.Translation.z * camData.CellSize + offset.z);
                                dist = math.length(item.Translation - camData.CamPos);
                                float distNorm = math.unlerp(0, renderSettings.MaxLODRange, dist);
                                //float bRadius = math.length(renderSettings.ItemBoundsExtents * item.Scale);
                                if (DistUpdates(ref renderSettings, distNorm, ref item))
                                {
                                    if (SplitShadows && dist < ShadowDistance)
                                        AddShadows(item);
                                    dist = math.clamp(math.pow(distNorm, renderSettings.LODbias) + (renderSettings.LODMix * (2 * (item.ExtraData.y - 0.5f))), 0, 1);
                                    CheckOutput(ref renderSettings, ref item, dist, -1, renderSettings.LODSplits_1, OutputLOD0);
                                    if (renderSettings.NumLODs > 1)
                                        CheckOutput(ref renderSettings, ref item, dist, renderSettings.LODSplits_1, renderSettings.LODSplits_2, OutputLOD1);
                                    if (renderSettings.NumLODs > 2)
                                        CheckOutput(ref renderSettings, ref item, dist, renderSettings.LODSplits_2, renderSettings.LODSplits_3, OutputLOD2);
                                    if (renderSettings.NumLODs > 3)
                                        CheckOutput(ref renderSettings, ref item, dist, renderSettings.LODSplits_3, renderSettings.LODSplits_4, OutputLOD3);
                                    if (renderSettings.NumLODs > 4)
                                        CheckOutput(ref renderSettings, ref item, dist, renderSettings.LODSplits_4, 2, OutputLOD4);
                                }
                            }
                        }
                        else
                        {
                            if (doShadowCheck && dist < ShadowDistance)
                            {
                                if ((renderSettings.LongShadowDist < 0 || dist < renderSettings.LongShadowDist) && camData.IsSphereVisible(bCenter + shadowBoundsCenter, shadowSize, dist))
                                {
                                    for (int index = 0; index < gridData.ItemCount; ++index)
                                    {
                                        StormItemData item = data[index];
                                        item.Translation = math.float3(item.Translation.x * camData.CellSize + offset.x, item.Translation.y + offset.y, item.Translation.z * camData.CellSize + offset.z);
                                        //float bRadius = math.length(renderSettings.ItemBoundsExtents * item.Scale);
                                        dist = math.length(item.Translation - camData.CamPos);
                                        float distNorm = math.unlerp(0, renderSettings.MaxLODRange, dist);
                                        if (DistUpdates(ref renderSettings, distNorm, ref item))
                                        {
                                            AddShadows(item);
                                        }
                                    }
                                }
                            }
                        }
                        indexOffset += gridData.ItemCount;
                    }
                }
                else
                {
                    for (int index = 0; index < len; ++index)
                    {
                        StormItemData item = iList[index];
                        item.Translation = math.float3(item.Translation.x * camData.CellSize + offset.x, item.Translation.y + offset.y, item.Translation.z * camData.CellSize + offset.z);
                        float itemBoundsSize = boundsSize * math.cmax(item.Scale);
                        float dist = math.max(0, math.length(item.Translation - camData.CamPos) - itemBoundsSize);
                        float distNorm = math.unlerp(0, renderSettings.MaxLODRange, dist);

                        if (fullyVisible || (!shadowsOnly && camData.IsSphereVisible(item.Translation, itemBoundsSize, dist)))
                        {
                            if (DistUpdates(ref renderSettings, distNorm, ref item))
                            {
                                if (SplitShadows && dist < ShadowDistance)
                                    AddShadows(item);
                                dist = math.clamp(math.pow(distNorm, renderSettings.LODbias) + (renderSettings.LODMix * (2 * (item.ExtraData.y - 0.5f))), 0, 1);
                                CheckOutput(ref renderSettings, ref item, dist, -1, renderSettings.LODSplits_1, OutputLOD0);
                                if (renderSettings.NumLODs > 1)
                                    CheckOutput(ref renderSettings, ref item, dist, renderSettings.LODSplits_1, renderSettings.LODSplits_2, OutputLOD1);
                                if (renderSettings.NumLODs > 2)
                                    CheckOutput(ref renderSettings, ref item, dist, renderSettings.LODSplits_2, renderSettings.LODSplits_3, OutputLOD2);
                                if (renderSettings.NumLODs > 3)
                                    CheckOutput(ref renderSettings, ref item, dist, renderSettings.LODSplits_3, renderSettings.LODSplits_4, OutputLOD3);
                                if (renderSettings.NumLODs > 4)
                                    CheckOutput(ref renderSettings, ref item, dist, renderSettings.LODSplits_4, 2, OutputLOD4);
                            }
                        }
                        else
                        {
                            if (doShadowCheck && dist < ShadowDistance && (renderSettings.LongShadowDist < 0 || dist < renderSettings.LongShadowDist) &&
                                (camData.IsSphereVisible(item.Translation + shadowBoundsCenter, shadowSize * math.cmax(item.Scale), dist)) &&
                                DistUpdates(ref renderSettings, distNorm, ref item))
                            {
                                AddShadows(item);
                            }
                        }
                    }
                }
            }
        }
        public void ExecuteNoCull(int l)
        {
            JobRenderSettings renderSettings = RenderSettings[0];
            JobCameraData camData = CameraData[0];
            bool doShadowCheck = math.any(camData.LightDir);
            //for (int l = 0; l < 20; ++l)
            {
                #region Variable Selection
                NativeArray<StormItemData> iList = l switch {
                    0 => ItemList0,
                    1 => ItemList1,
                    2 => ItemList2,
                    3 => ItemList3,
                    4 => ItemList4,
                    5 => ItemList5,
                    6 => ItemList6,
                    7 => ItemList7,
                    8 => ItemList8,
                    9 => ItemList9,
                    10 => ItemList10,
                    11 => ItemList11,
                    12 => ItemList12,
                    13 => ItemList13,
                    14 => ItemList14,
                    15 => ItemList15,
                    16 => ItemList16,
                    17 => ItemList17,
                    18 => ItemList18,
                    19 => ItemList19,
                    _ => ItemList0,
                };
                float3 offset = l switch {
                    0 => Offset0,
                    1 => Offset1,
                    2 => Offset2,
                    3 => Offset3,
                    4 => Offset4,
                    5 => Offset5,
                    6 => Offset6,
                    7 => Offset7,
                    8 => Offset8,
                    9 => Offset9,
                    10 => Offset10,
                    11 => Offset11,
                    12 => Offset12,
                    13 => Offset13,
                    14 => Offset14,
                    15 => Offset15,
                    16 => Offset16,
                    17 => Offset17,
                    18 => Offset18,
                    19 => Offset19,
                    _ => Offset0,
                };
                bool shadowsOnly = l switch {
                    0 => ShadowsOnly0,
                    1 => ShadowsOnly1,
                    2 => ShadowsOnly2,
                    3 => ShadowsOnly3,
                    4 => ShadowsOnly4,
                    5 => ShadowsOnly5,
                    6 => ShadowsOnly6,
                    7 => ShadowsOnly7,
                    8 => ShadowsOnly8,
                    9 => ShadowsOnly9,
                    10 => ShadowsOnly10,
                    11 => ShadowsOnly11,
                    12 => ShadowsOnly12,
                    13 => ShadowsOnly13,
                    14 => ShadowsOnly14,
                    15 => ShadowsOnly15,
                    16 => ShadowsOnly16,
                    17 => ShadowsOnly17,
                    18 => ShadowsOnly18,
                    19 => ShadowsOnly19,
                    _ => ShadowsOnly0,
                };
                #endregion
                int len = iList.Length;
                for (int index = 0; index < len; ++index)
                {
                    StormItemData item = iList[index];
                    item.Translation = math.float3(item.Translation.x * camData.CellSize + offset.x, item.Translation.y + offset.y, item.Translation.z * camData.CellSize + offset.z);

                    float dist = math.length(item.Translation - camData.CamPos);
                    float distNorm = math.unlerp(0, renderSettings.MaxLODRange, dist);

                    if (!shadowsOnly)
                    {
                        if (DistUpdates(ref renderSettings, distNorm, ref item))
                        {
                            if (SplitShadows && dist < ShadowDistance)
                                AddShadows(item);
                            dist = math.clamp(math.pow(distNorm, renderSettings.LODbias) + (renderSettings.LODMix * (2 * (item.ExtraData.y - 0.5f))), 0, 1);
                            CheckOutput(ref renderSettings, ref item, dist, -1, renderSettings.LODSplits_1, OutputLOD0);
                            if (renderSettings.NumLODs > 1)
                                CheckOutput(ref renderSettings, ref item, dist, renderSettings.LODSplits_1, renderSettings.LODSplits_2, OutputLOD1);
                            if (renderSettings.NumLODs > 2)
                                CheckOutput(ref renderSettings, ref item, dist, renderSettings.LODSplits_2, renderSettings.LODSplits_3, OutputLOD2);
                            if (renderSettings.NumLODs > 3)
                                CheckOutput(ref renderSettings, ref item, dist, renderSettings.LODSplits_3, renderSettings.LODSplits_4, OutputLOD3);
                            if (renderSettings.NumLODs > 4)
                                CheckOutput(ref renderSettings, ref item, dist, renderSettings.LODSplits_4, 2, OutputLOD4);
                        }
                    }
                    else
                    {
                        if (doShadowCheck && dist < ShadowDistance && (renderSettings.LongShadowDist < 0 || dist < renderSettings.LongShadowDist) && DistUpdates(ref renderSettings, distNorm, ref item))
                        {
                            AddShadows(item);
                        }
                    }
                }
            }
        }
        bool DistUpdates(ref JobRenderSettings renderSettings, float distNorm, ref StormItemData item)
        {
            float fade;
            //remove items based on "Extra Data.y" value vs Range
            if (renderSettings.DensityFadeStrength > 0 && item.ExtraData.y > 0 && renderSettings.DensityFadeStart < 1 && (fade = math.unlerp(renderSettings.DensityFadeStart, 1, distNorm) - math.pow(item.ExtraData.y, renderSettings.DensityFadeStrength)) > 0)
            {
                //if our new range is >= our fade range, just cull it or if our new range is above the begin fade (0) and FadeRange, scale it
                if (fade > 0 && fade < renderSettings.DensityFadeScaleoutRange)
                    //scale item by a normalized value from 0 to FadeRange as (0-1)
                    item.Scale *= 1 - (fade / renderSettings.DensityFadeScaleoutRange);
                else
                    return false;
            }
            if ((fade = (1 - distNorm) / renderSettings.ScaleoutRange) < 1 && fade > 0)
                item.Scale *= math.lerp(renderSettings.ScaleoutMinScale, 1, fade);
            return fade > 0;
        }
        void CheckOutput(ref JobRenderSettings renderSettings, ref StormItemData rData, float dist, float fadeStart, float fadeEnd, NativeList<StormItemData>.ParallelWriter Output)
        {
            float fade =
                //renderSettings.LODFade <= 0 ?
                (dist >= fadeStart && dist <= fadeEnd ? 1 : 0)
                //: math.min(dist - (fadeStart - renderSettings.LODFade), (fadeEnd + renderSettings.LODFade) - dist) / renderSettings.LODFade
                ;
            if (fade > 0)
            {
                rData.ExtraData.y = math.clamp(fade, 0, 2);
                Output.AddNoResize(rData);
            }
        }
        void AddShadows(StormItemData rData)
        {
            rData.ExtraData.y = 0;
            OutputShadows.AddNoResize(rData);
        }
    }
}