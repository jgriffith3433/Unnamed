/// Copyright 2023 Procedural Worlds Ltd
/// Written by Jonathan Williams as part of Storm

namespace ProceduralWorlds.Storm.Utils
{
    /// <summary>
    /// Enumeration of all types of Objects Storm can render
    /// </summary>
    [System.Flags]
    public enum StormDebugFeatureType : byte
    {
        NONE = 0,
        GRASS = 0x01,
        DETAIL = 0x02,
        PLANT = 0x04,
        TREE = 0x08,
        SMALL_OBJECT = 0x10,
        MEDIUM_OBJECT = 0x20,
        LARGE_OBJECT = 0x40,
        COLLIDERS = 0x80,
        ALL = 0xFF,
    }
    public enum StormFeatureType : byte
    {
        GRASS = 0x01,
        DETAIL = 0x02,
        PLANT = 0x04,
        TREE = 0x08,
        SMALL_OBJECT = 0x10,
        MEDIUM_OBJECT = 0x20,
        LARGE_OBJECT = 0x40,
        COLLIDERS = 0x80,
    }
}
