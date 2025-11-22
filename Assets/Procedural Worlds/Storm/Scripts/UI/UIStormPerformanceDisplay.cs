using UnityEngine;
using ProceduralWorlds.Storm.Core;
using ProceduralWorlds.Storm.Utils;
using TMPro;
using System.Collections.Generic;

namespace ProceduralWorlds.Storm.UI
{
    [AddComponentMenu("Procedural Worlds/Storm/UI/Performance Display")]
    public class UIStormPerformanceDisplay : MonoBehaviour
    {
        [System.Serializable]
        public struct FeatureDisplaySet
        {
            public StormFeatureSettings Feature;
            public TMP_Text VisibleCountDisplay;
            public TMP_Text LOD0InstanceCountDisplay;
            public TMP_Text LOD0VertexCountDisplay;
            public GameObject LOD1;
            public TMP_Text LOD1InstanceCountDisplay;
            public TMP_Text LOD1VertexCountDisplay;
            public GameObject LOD2;
            public TMP_Text LOD2InstanceCountDisplay;
            public TMP_Text LOD2VertexCountDisplay;
            public GameObject LOD3;
            public TMP_Text LOD3InstanceCountDisplay;
            public TMP_Text LOD3VertexCountDisplay;
            public GameObject LOD4;
            public TMP_Text LOD4InstanceCountDisplay;
            public TMP_Text LOD4VertexCountDisplay;
            public TMP_Text DrawCallDisplay;
            public TMP_Text InstanceCountDisplay;
            public TMP_Text VertexCountDisplay;
        }
        [System.Serializable]
        public struct TotalDisplaySet
        {
            public TMP_Text DrawCallDisplay;
            public TMP_Text InstanceCountDisplay;
            public TMP_Text VertexCountDisplay;
        }
        [Range(0, 30f)]
        public float UpdateInterval = 5f;

        public FeatureDisplaySet[] FeatureDisplays;
        public TotalDisplaySet GrassDisplay;
        public TotalDisplaySet DetailDisplay;
        public TotalDisplaySet PlantDisplay;
        public TotalDisplaySet TreeDisplay;
        public TotalDisplaySet SmallDisplay;
        public TotalDisplaySet MediumDisplay;
        public TotalDisplaySet LargeDisplay;
        public TotalDisplaySet TotalsDisplay;

        float lastUpdateTime = float.MinValue;
        StormWorld world;

        public void Update()
        {
            float now = Time.time;
            //see if we have waited long enough since the last update
            if (lastUpdateTime + UpdateInterval < now)
            {
                lastUpdateTime = now;
                if (world == null)
                {
                    //we haven't grabbed the world instance ... try now
                    world = StormWorld.Instance;
                    if (world == null)
                        return; //no world exists ... just exit
                }
                List<StormFeatureSettings> allFeatures = StormDataCache.AllEnabledFeatures;
                StormInstanceDebugData[] results = new StormInstanceDebugData[allFeatures.Count];
                for (int f = 0; f < results.Length; ++f)
                    results[f] = world.GetInstanceCount(allFeatures[f]);
                long totalVert;
                long totalCount;
                for (int d = 0; d < FeatureDisplays.Length; ++d)
                {
                    int idx = -1;
                    for (int f = 0; f < results.Length; ++f)
                    {
                        if (allFeatures[f] == FeatureDisplays[d].Feature)
                        {
                            idx = f;
                            break;
                        }
                    }
                    if (idx < 0)
                        continue;

                    totalVert = 0;
                    totalCount = 0;
                    Set(FeatureDisplays[d].DrawCallDisplay, results[idx].DrawCalls.ToString("N0"));
                    for (int lod = 0; lod < results[idx].LODCounts.Length; ++lod)
                    {
                        totalCount += results[idx].LODCounts[lod];
                        totalVert += results[idx].LODVerts[lod];
                        switch (lod)
                        {
                            default:
                            case 0:
                                Set(FeatureDisplays[d].LOD0InstanceCountDisplay, results[idx].LODCounts[lod].ToString("N0"));
                                Set(FeatureDisplays[d].LOD0VertexCountDisplay, results[idx].LODVerts[lod].ToString("N0"));
                                break;
                            case 1:
                                if (FeatureDisplays[d].LOD1 != null)
                                    FeatureDisplays[d].LOD1.SetActive(true);
                                Set(FeatureDisplays[d].LOD1InstanceCountDisplay, results[idx].LODCounts[lod].ToString("N0"));
                                Set(FeatureDisplays[d].LOD1VertexCountDisplay, results[idx].LODVerts[lod].ToString("N0"));
                                break;
                            case 2:
                                if (FeatureDisplays[d].LOD2 != null)
                                    FeatureDisplays[d].LOD2.SetActive(true);
                                Set(FeatureDisplays[d].LOD2InstanceCountDisplay, results[idx].LODCounts[lod].ToString("N0"));
                                Set(FeatureDisplays[d].LOD2VertexCountDisplay, results[idx].LODVerts[lod].ToString("N0"));
                                break;
                            case 3:
                                if (FeatureDisplays[d].LOD3 != null)
                                    FeatureDisplays[d].LOD3.SetActive(true);
                                Set(FeatureDisplays[d].LOD3InstanceCountDisplay, results[idx].LODCounts[lod].ToString("N0"));
                                Set(FeatureDisplays[d].LOD3VertexCountDisplay, results[idx].LODVerts[lod].ToString("N0"));
                                break;
                            case 4:
                                if (FeatureDisplays[d].LOD4 != null)
                                    FeatureDisplays[d].LOD4.SetActive(true);
                                Set(FeatureDisplays[d].LOD4InstanceCountDisplay, results[idx].LODCounts[lod].ToString("N0"));
                                Set(FeatureDisplays[d].LOD4VertexCountDisplay, results[idx].LODVerts[lod].ToString("N0"));
                                break;
                        }
                    }
                    for (int lod = results[idx].LODCounts.Length; lod < 5; ++lod)
                    {
                        switch (lod)
                        {
                            default:
                                break;
                            case 1:
                                if (FeatureDisplays[d].LOD1 != null)
                                    FeatureDisplays[d].LOD1.SetActive(false);
                                break;
                            case 2:
                                if (FeatureDisplays[d].LOD2 != null)
                                    FeatureDisplays[d].LOD2.SetActive(false);
                                break;
                            case 3:
                                if (FeatureDisplays[d].LOD3 != null)
                                    FeatureDisplays[d].LOD3.SetActive(false);
                                break;
                            case 4:
                                if (FeatureDisplays[d].LOD4 != null)
                                    FeatureDisplays[d].LOD4.SetActive(false);
                                break;
                        }
                    }
                    Set(FeatureDisplays[d].VisibleCountDisplay, results[idx].VisibleCellCount.ToString("N0"));
                    Set(FeatureDisplays[d].DrawCallDisplay, results[idx].DrawCalls.ToString("N0"));
                    Set(FeatureDisplays[d].InstanceCountDisplay, totalCount.ToString("N0"));
                    Set(FeatureDisplays[d].VertexCountDisplay, totalVert.ToString("N0"));
                }

                totalVert = 0;
                totalCount = 0;
                int totalDrawCalls = 0;
                long grassVert = 0;
                long grassCount = 0;
                int grassDrawCalls = 0;
                long detailVert = 0;
                long detailCount = 0;
                int detailDrawCalls = 0;
                long plantVert = 0;
                long plantCount = 0;
                int plantDrawCalls = 0;
                long treeVert = 0;
                long treeCount = 0;
                int treeDrawCalls = 0;
                long smallVert = 0;
                long smallCount = 0;
                int smallDrawCalls = 0;
                long mediumVert = 0;
                long mediumCount = 0;
                int mediumDrawCalls = 0;
                long largeVert = 0;
                long largeCount = 0;
                int largeDrawCalls = 0;
                //recount the totals in case we just don't have displays for some of the features or duplicates
                for (int f = 0; f < results.Length; ++f)
                {
                    long Vert = 0;
                    long Count = 0;
                    int DrawCalls = results[f].DrawCalls;
                    for (int lod = 0; lod < results[f].LODCounts.Length; ++lod)
                    {
                        Vert += results[f].LODVerts[lod];
                        Count += results[f].LODCounts[lod];
                    }
                    totalVert += Vert;
                    totalCount += Count;
                    totalDrawCalls += DrawCalls;
                    switch (allFeatures[f].ObjectType)
                    {
                        case StormFeatureType.GRASS:
                            grassVert += Vert;
                            grassCount += Count;
                            grassDrawCalls += DrawCalls;
                            break;
                        case StormFeatureType.DETAIL:
                            detailVert += Vert;
                            detailCount += Count;
                            detailDrawCalls += DrawCalls;
                            break;
                        case StormFeatureType.PLANT:
                            plantVert += Vert;
                            plantCount += Count;
                            plantDrawCalls += DrawCalls;
                            break;
                        case StormFeatureType.TREE:
                            treeVert += Vert;
                            treeCount += Count;
                            treeDrawCalls += DrawCalls;
                            break;
                        case StormFeatureType.SMALL_OBJECT:
                            smallVert += Vert;
                            smallCount += Count;
                            smallDrawCalls += DrawCalls;
                            break;
                        case StormFeatureType.MEDIUM_OBJECT:
                            mediumVert += Vert;
                            mediumCount += Count;
                            mediumDrawCalls += DrawCalls;
                            break;
                        case StormFeatureType.LARGE_OBJECT:
                            largeVert += Vert;
                            largeCount += Count;
                            largeDrawCalls += DrawCalls;
                            break;
                    }
                }

                Set(GrassDisplay.DrawCallDisplay, grassDrawCalls.ToString("N0"));
                Set(GrassDisplay.InstanceCountDisplay, grassCount.ToString("N0"));
                Set(GrassDisplay.VertexCountDisplay, grassVert.ToString("N0"));

                Set(DetailDisplay.DrawCallDisplay, detailDrawCalls.ToString("N0"));
                Set(DetailDisplay.InstanceCountDisplay, detailCount.ToString("N0"));
                Set(DetailDisplay.VertexCountDisplay, detailVert.ToString("N0"));

                Set(PlantDisplay.DrawCallDisplay, plantDrawCalls.ToString("N0"));
                Set(PlantDisplay.InstanceCountDisplay, plantCount.ToString("N0"));
                Set(PlantDisplay.VertexCountDisplay, plantVert.ToString("N0"));

                Set(TreeDisplay.DrawCallDisplay, treeDrawCalls.ToString("N0"));
                Set(TreeDisplay.InstanceCountDisplay, treeCount.ToString("N0"));
                Set(TreeDisplay.VertexCountDisplay, treeVert.ToString("N0"));

                Set(SmallDisplay.DrawCallDisplay, smallDrawCalls.ToString("N0"));
                Set(SmallDisplay.InstanceCountDisplay, smallCount.ToString("N0"));
                Set(SmallDisplay.VertexCountDisplay, smallVert.ToString("N0"));

                Set(MediumDisplay.DrawCallDisplay, mediumDrawCalls.ToString("N0"));
                Set(MediumDisplay.InstanceCountDisplay, mediumCount.ToString("N0"));
                Set(MediumDisplay.VertexCountDisplay, mediumVert.ToString("N0"));

                Set(LargeDisplay.DrawCallDisplay, largeDrawCalls.ToString("N0"));
                Set(LargeDisplay.InstanceCountDisplay, largeCount.ToString("N0"));
                Set(LargeDisplay.VertexCountDisplay, largeVert.ToString("N0"));

                Set(TotalsDisplay.DrawCallDisplay, totalDrawCalls.ToString("N0"));
                Set(TotalsDisplay.InstanceCountDisplay, totalCount.ToString("N0"));
                Set(TotalsDisplay.VertexCountDisplay, totalVert.ToString("N0"));
            }
        }
        void Set(TMP_Text txt, string str)
        {
            if (txt != null)
                txt.text = str;
        }

        public void SetUpdateRate(float rate)
        {
            UpdateInterval = rate;
        }
    }
}