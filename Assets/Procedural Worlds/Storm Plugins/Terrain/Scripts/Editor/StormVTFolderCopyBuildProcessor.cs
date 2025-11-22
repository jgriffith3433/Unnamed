using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using System.IO;

public class StormVTFolderCopyBuildProcessor
{
    /// <summary>
    /// Folder path on where to store the saved out tiles in a build
    /// </summary>
    public const string FOLDER_PATH = "Tiles";
    
    /// <summary>
    /// Copy Tiles folder from /Assets to /Build folder
    /// </summary>
    /// <param name="target"></param>
    /// <param name="pathToBuiltProject"></param>
    [PostProcessBuild]
    public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject)
    {
        // Define the source folder path and the target path
        string sourceFolder = Path.Combine(Path.GetDirectoryName(Application.dataPath), FOLDER_PATH);
        string targetFolder = Path.Combine(Path.GetDirectoryName(pathToBuiltProject), FOLDER_PATH);

        // Copy the folder
        CopyEntireDirectory(sourceFolder, targetFolder);
    }

    /// <summary>
    /// Copies every item in the source folder into the destination folder
    /// </summary>
    /// <param name="sourcePath"></param>
    /// <param name="targetPath"></param>
    private static void CopyEntireDirectory(string sourcePath, string targetPath)
    {
        // Check if the source directory exists
        if (!Directory.Exists(sourcePath))
        {
            return;
        }

        // Create the target directory if it doesn't exist
        if (!Directory.Exists(targetPath))
        {
            Directory.CreateDirectory(targetPath);
        }

        // Copy each file into the new directory
        foreach (string file in Directory.GetFiles(sourcePath))
        {
            string fileName = Path.GetFileName(file);
            string targetFilePath = Path.Combine(targetPath, fileName);
            File.Copy(file, targetFilePath, true);
        }

        // Copy each subdirectory using recursion
        foreach (string directory in Directory.GetDirectories(sourcePath))
        {
            string directoryName = Path.GetFileName(directory);
            string targetDirectoryPath = Path.Combine(targetPath, directoryName);
            CopyEntireDirectory(directory, targetDirectoryPath);
        }
    }
}
