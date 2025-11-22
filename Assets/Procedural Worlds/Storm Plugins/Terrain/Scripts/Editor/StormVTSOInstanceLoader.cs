using ProceduralWorlds.Storm.Terrain;
using System.IO;
using UnityEditor;

/// <summary>
/// Loads instances for scriptable objects on initialization. Those scriptable objects may be accessed from elsewhere in the application
/// </summary>
[InitializeOnLoad]
public class StormVTSOInstanceLoader
{
    static StormVTSOInstanceLoader()
    {
        string contentFolderPath = GetAssetPath(StormVTConstants.ContentConfFileName);
        contentFolderPath = Path.GetDirectoryName(contentFolderPath);
        StormVTConstants.ContentFolderPath = contentFolderPath;
        StormVTDefaults.Instance = AssetDatabase.LoadAssetAtPath<StormVTDefaults>(StormVTConstants.StormVTDefaultsPath);
        StormVTGaia.Instance = AssetDatabase.LoadAssetAtPath<StormVTGaia>(StormVTConstants.StormVTGaiaPath);

    }

    /// <summary>
    /// Returns the asset data path based on a file name if the asset exists
    /// </summary>
    /// <param name="fileName"></param>
    /// <returns></returns>
    public static string GetAssetPath(string fileName)
    {
        foreach (string asset in AssetDatabase.FindAssets(Path.GetFileNameWithoutExtension(fileName), (string[])null))
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(asset);
            if (Path.GetFileName(assetPath) == fileName)
                return assetPath;
        }
        return "";
    }
}
