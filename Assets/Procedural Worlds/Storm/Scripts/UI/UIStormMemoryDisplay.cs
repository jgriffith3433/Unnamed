using UnityEngine;
using ProceduralWorlds.Storm.Core;
using ProceduralWorlds.Storm.Utils;
using TMPro;
using System.Collections.Generic;

namespace ProceduralWorlds.Storm.UI
{
    [AddComponentMenu("Procedural Worlds/Storm/UI/Memory Display")]
    public class UIStormMemoryDisplay : MonoBehaviour
    {
        [System.Serializable]
        public struct MemoryDisplaySet
        {
            public TMP_Text ArrayCountDisplay;
            public TMP_Text RAMDisplay;
            public TMP_Text GPUDisplay;
        }

        [Range(0, 30f)]
        public float UpdateInterval = 5f;

        public MemoryDisplaySet DataDisplays;
        public MemoryDisplaySet TileDisplays;
        public MemoryDisplaySet LoadingDisplays;
        public MemoryDisplaySet RenderingDisplays;
        public MemoryDisplaySet TotalDisplays;

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
                MemoryUsageData data = world.GetMemoryUsage();
                if (DataDisplays.ArrayCountDisplay != null)
                    DataDisplays.ArrayCountDisplay.text = data.Cache.Count.ToString("N0");
                if (DataDisplays.RAMDisplay != null)
                    DataDisplays.RAMDisplay.text = StormUtils.GetReadableFileSize(data.Cache.SizeCPU);
                if (DataDisplays.GPUDisplay != null)
                    DataDisplays.GPUDisplay.text = StormUtils.GetReadableFileSize(data.Cache.SizeGPU);

                if (TileDisplays.ArrayCountDisplay != null)
                    TileDisplays.ArrayCountDisplay.text = data.Data.Count.ToString("N0");
                if (TileDisplays.RAMDisplay != null)
                    TileDisplays.RAMDisplay.text = StormUtils.GetReadableFileSize(data.Data.SizeCPU);
                if (TileDisplays.GPUDisplay != null)
                    TileDisplays.GPUDisplay.text = StormUtils.GetReadableFileSize(data.Data.SizeGPU);

                if (LoadingDisplays.ArrayCountDisplay != null)
                    LoadingDisplays.ArrayCountDisplay.text = data.Loading.Count.ToString("N0");
                if (LoadingDisplays.RAMDisplay != null)
                    LoadingDisplays.RAMDisplay.text = StormUtils.GetReadableFileSize(data.Loading.SizeCPU);
                if (LoadingDisplays.GPUDisplay != null)
                    LoadingDisplays.GPUDisplay.text = StormUtils.GetReadableFileSize(data.Loading.SizeGPU);

                if (RenderingDisplays.ArrayCountDisplay != null)
                    RenderingDisplays.ArrayCountDisplay.text = data.Rendering.Count.ToString("N0");
                if (RenderingDisplays.RAMDisplay != null)
                    RenderingDisplays.RAMDisplay.text = StormUtils.GetReadableFileSize(data.Rendering.SizeCPU);
                if (RenderingDisplays.GPUDisplay != null)
                    RenderingDisplays.GPUDisplay.text = StormUtils.GetReadableFileSize(data.Rendering.SizeGPU);

                MemoryCategoryData totals = data.Cache + data.Data + data.Loading + data.Rendering;
                if (TotalDisplays.ArrayCountDisplay != null)
                    TotalDisplays.ArrayCountDisplay.text = totals.Count.ToString("N0");
                if (TotalDisplays.RAMDisplay != null)
                    TotalDisplays.RAMDisplay.text = StormUtils.GetReadableFileSize(totals.SizeCPU);
                if (TotalDisplays.GPUDisplay != null)
                    TotalDisplays.GPUDisplay.text = StormUtils.GetReadableFileSize(totals.SizeGPU);
            }
        }
    }
}