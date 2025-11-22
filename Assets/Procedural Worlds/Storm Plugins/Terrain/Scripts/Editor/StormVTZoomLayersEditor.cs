using ProceduralWorlds.Storm.Terrain;
using UnityEditor;

[InitializeOnLoad]
public static class StormVTZoomLayersEditor
{
    /// <summary>
    /// Initialize the zoom layers upon start
    /// </summary>
    static StormVTZoomLayersEditor()
    {
        StormVTZoomLayers.InitializeVTZoomLayers();
    }
}
