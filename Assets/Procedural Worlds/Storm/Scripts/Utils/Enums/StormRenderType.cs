/// Copyright 2023 Procedural Worlds Ltd
/// Written by Jonathan Williams as part of Storm

namespace ProceduralWorlds.Storm.Utils
{
    /// <summary>
    /// Enumeration of how items should be rendered
    /// Default - Uses setting in current WorldSettings (or RENDER_INDIRECT as fallback)
    /// RENDER_INDIRECT - Draws all instances in a single draw call. this is the best option in most cases.
    /// RENDER_INSTANCED - Draws instances in groups of up to 1023 instances per batch
    /// DRAW_INDIRECT - Draws all instances in a single draw call using legacy functions (this is the best of legacy)
    /// DRAW_INSTANCED - Draws instances in groups of up to 1023 instances per batch using legacy functions
    /// DRAWMESH - Draws each item separately using "DrawMesh" function. This is the worst option and only a last resort
    /// ECS_EXPERIMENTAL - Draws items using the ECS Entity system
    /// </summary>
    public enum StormRenderType
    {
        DEFAULT = 0,
        RENDER_INDIRECT = -2,
        RENDER_INSTANCED = -1,
        DRAW_INDIRECT = 1,
        DRAW_INSTANCED = 2,
        DRAWMESH = 3,
#if PW_STORM_ECS
        ECS_EXPERIMENTAL = 4,
#endif
    }
}