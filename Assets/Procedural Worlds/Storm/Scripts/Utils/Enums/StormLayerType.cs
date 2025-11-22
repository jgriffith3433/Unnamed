/// Copyright 2024 Procedural Worlds Ltd
/// Written by Jonathan Williams as part of Storm

namespace ProceduralWorlds.Storm.Utils
{
    [System.Flags]
    public enum StormLayerType : ushort
    {
        NONE = 0,
        TEXTURE = 0x0001,
        FLOAT = 0x0002,
        TRS = 0x0004,
        TRANSFORM = 0x0008,
        POLYGON = 0x0010,
        CUSTOM_FLOAT = 0x0020,
        CUSTOM_INSTANCE = 0x0040,
        FEATURESET_USED = 0x0080,
        REMAP_USED = 0x0100
    }
}