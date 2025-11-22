/// Copyright 2025 Procedural Worlds Ltd
/// Written by Jonathan Williams as part of Storm

namespace ProceduralWorlds.Storm.Utils
{
    public struct MemoryCategoryData
    {
        public int Count;
        public ulong SizeCPU;
        public ulong SizeGPU;

        public static MemoryCategoryData operator +(MemoryCategoryData a, MemoryCategoryData b)
        {
            MemoryCategoryData ret = a;
            ret.Count += b.Count;
            ret.SizeCPU += b.SizeCPU;
            ret.SizeGPU += b.SizeGPU;
            return ret;
        }
    }
    public struct MemoryUsageData
    {
        public MemoryCategoryData Loading;
        public MemoryCategoryData Rendering;
        public MemoryCategoryData Cache;
        public MemoryCategoryData Data;
    }
}