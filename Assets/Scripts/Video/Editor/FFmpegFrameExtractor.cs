#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Diagnostics;
using System.IO;
using System.Collections;
using System.Linq;

public static class FFmpegFrameExtractor
{
    private static string ffmpegPath = null;
    
    /// <summary>
    /// Extract frames using FFmpeg (much faster than VideoPlayer method)
    /// </summary>
    public static void ExtractFrames(string videoPath, float fps, string outputPath, System.Action<bool, string> onComplete)
    {
        // Check if FFmpeg is available
        if (!IsFFmpegAvailable())
        {
            onComplete?.Invoke(false, "FFmpeg not found. Please install FFmpeg and add it to PATH, or use the slower VideoPlayer extraction method.");
            return;
        }

        // Create output directory
        if (!Directory.Exists(outputPath))
        {
            Directory.CreateDirectory(outputPath);
        }

        // Convert Unity asset path to absolute path
        string absoluteVideoPath = Path.GetFullPath(videoPath);
        string absoluteOutputPath = Path.GetFullPath(outputPath);

        // FFmpeg command to extract frames
        // -i input.mp4 : input file
        // -vf fps=30 : extract at 30 fps
        // -start_number 0 : start frame numbering from 0
        // frame_%05d.png : output pattern (frame_00000.png, frame_00001.png, etc.)
        string outputPattern = Path.Combine(absoluteOutputPath, "frame_%05d.png");
        string arguments = $"-i \"{absoluteVideoPath}\" -vf fps={fps} -start_number 0 \"{outputPattern}\"";

        // Fire and forget - just start FFmpeg process in background
        ProcessStartInfo startInfo = new ProcessStartInfo
        {
            FileName = ffmpegPath,
            Arguments = arguments,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };

        try
        {
            Process.Start(startInfo);
            onComplete?.Invoke(true, "FFmpeg extraction started. Frames will appear in the folder shortly.");
        }
        catch (System.Exception e)
        {
            onComplete?.Invoke(false, $"Failed to start FFmpeg: {e.Message}");
        }
    }

    private static IEnumerator RunFFmpegProcess(string arguments, string outputPath, System.Action<bool, string> onComplete)
    {
        // Use full path to ffmpeg
        ProcessStartInfo startInfo = new ProcessStartInfo
        {
            FileName = ffmpegPath,
            Arguments = arguments,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };

        Process process = new Process { StartInfo = startInfo };
        bool hasError = false;
        string errorMessage = "";
        
        try
        {
            process.Start();
        }
        catch (System.Exception e)
        {
            hasError = true;
            errorMessage = $"FFmpeg error: {e.Message}";
        }

        if (hasError)
        {
            EditorUtility.ClearProgressBar();
            onComplete?.Invoke(false, errorMessage);
            yield break;
        }

        // Wait for process to complete
        EditorUtility.DisplayProgressBar("Extracting Frames", "FFmpeg is extracting frames...", 0.5f);
        
        while (!process.HasExited)
        {
            yield return null;
        }

        int exitCode = process.ExitCode;
        process.Close();

        EditorUtility.ClearProgressBar();

        if (exitCode == 0)
        {
            // Just refresh assets and let Unity handle the rest
            AssetDatabase.Refresh();
            
            // Count extracted frames
            string[] pngFiles = Directory.GetFiles(outputPath, "frame_*.png");
            onComplete?.Invoke(true, $"Extracted {pngFiles.Length} frames using FFmpeg");
        }
        else
        {
            onComplete?.Invoke(false, $"FFmpeg extraction failed with exit code {exitCode}");
        }
    }

    private static IEnumerator ConvertPNGsToAssets(string outputPath, System.Action<bool, string> onComplete)
    {
        // Get all PNG files
        string[] pngFiles = Directory.GetFiles(outputPath, "frame_*.png");
        
        if (pngFiles.Length == 0)
        {
            EditorUtility.ClearProgressBar();
            onComplete?.Invoke(false, "No frames were extracted");
            yield break;
        }

        // Convert relative path for Unity
        string unityOutputPath = outputPath.Replace("\\", "/");
        if (unityOutputPath.StartsWith(Application.dataPath))
        {
            unityOutputPath = "Assets" + unityOutputPath.Substring(Application.dataPath.Length);
        }

        // First refresh to see the PNG files
        EditorUtility.DisplayProgressBar("Converting Frames", "Importing PNG files...", 0f);
        AssetDatabase.Refresh();
        
        // Wait for import to complete
        for (int i = 0; i < 10; i++)
        {
            yield return null;
        }

        int count = 0;
        foreach (string pngFile in pngFiles)
        {
            string relativePath = pngFile.Replace("\\", "/");
            if (relativePath.StartsWith(Application.dataPath))
            {
                relativePath = "Assets" + relativePath.Substring(Application.dataPath.Length);
            }

            // Import the PNG as readable
            TextureImporter importer = AssetImporter.GetAtPath(relativePath) as TextureImporter;
            if (importer != null)
            {
                importer.isReadable = true;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.SaveAndReimport();
            }
            
            count++;
            if (count % 5 == 0)
            {
                EditorUtility.DisplayProgressBar("Converting Frames", $"Importing textures {count}/{pngFiles.Length}...", (float)count / pngFiles.Length * 0.5f);
                yield return null;
            }
        }

        // Wait for all imports to finish
        AssetDatabase.Refresh();
        for (int i = 0; i < 5; i++)
        {
            yield return null;
        }

        // Now convert PNGs to assets
        count = 0;
        foreach (string pngFile in pngFiles)
        {
            string relativePath = pngFile.Replace("\\", "/");
            if (relativePath.StartsWith(Application.dataPath))
            {
                relativePath = "Assets" + relativePath.Substring(Application.dataPath.Length);
            }

            // Load PNG as Texture2D
            Texture2D png = AssetDatabase.LoadAssetAtPath<Texture2D>(relativePath);
            if (png != null)
            {
                // Create a readable copy as an asset
                Texture2D texture = new Texture2D(png.width, png.height, TextureFormat.RGB24, false);
                
                // Copy pixel data
                RenderTexture rt = RenderTexture.GetTemporary(png.width, png.height, 0, RenderTextureFormat.ARGB32);
                Graphics.Blit(png, rt);
                RenderTexture previous = RenderTexture.active;
                RenderTexture.active = rt;
                texture.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
                texture.Apply();
                RenderTexture.active = previous;
                RenderTexture.ReleaseTemporary(rt);
                
                string assetPath = relativePath.Replace(".png", ".asset");
                AssetDatabase.CreateAsset(texture, assetPath);
                
                // Delete the PNG file
                AssetDatabase.DeleteAsset(relativePath);
            }

            count++;
            if (count % 5 == 0)
            {
                float progress = 0.5f + ((float)count / pngFiles.Length * 0.5f);
                EditorUtility.DisplayProgressBar("Converting Frames", $"Converting {count}/{pngFiles.Length}...", progress);
                AssetDatabase.SaveAssets();
                yield return null;
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorUtility.ClearProgressBar();

        onComplete?.Invoke(true, $"Extracted {pngFiles.Length} frames using FFmpeg");
    }

    private static bool IsFFmpegAvailable()
    {
        if (ffmpegPath != null)
            return true;
            
        // Try to find ffmpeg using multiple methods
        
        // Method 1: Check common WinGet installation paths
        string[] possiblePaths = new string[]
        {
            Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData), 
                "Microsoft\\WinGet\\Links\\ffmpeg.exe"),
            Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData), 
                "Microsoft\\WinGet\\Packages\\Gyan.FFmpeg_Microsoft.Winget.Source_8wekyb3d8bbwe\\ffmpeg-8.0.1-full_build\\bin\\ffmpeg.exe")
        };
        
        foreach (string path in possiblePaths)
        {
            if (File.Exists(path))
            {
                ffmpegPath = path;
                UnityEngine.Debug.Log($"Found FFmpeg at: {ffmpegPath}");
                return true;
            }
        }
        
        // Method 2: Try to find ffmpeg in PATH using 'where' command
        try
        {
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = "/c where ffmpeg",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            using (Process process = Process.Start(startInfo))
            {
                string output = process.StandardOutput.ReadToEnd().Trim();
                process.WaitForExit(2000);
                
                if (process.ExitCode == 0 && !string.IsNullOrEmpty(output))
                {
                    // Get first line (in case multiple ffmpeg installations)
                    string[] lines = output.Split(new[] { '\r', '\n' }, System.StringSplitOptions.RemoveEmptyEntries);
                    if (lines.Length > 0)
                    {
                        ffmpegPath = lines[0].Trim();
                        UnityEngine.Debug.Log($"Found FFmpeg at: {ffmpegPath}");
                        return true;
                    }
                }
            }
        }
        catch (System.Exception e)
        {
            UnityEngine.Debug.LogError($"FFmpeg PATH search exception: {e.Message}");
        }
        
        // Method 3: Search WinGet packages directory
        string wingetPackages = Path.Combine(
            System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData),
            "Microsoft\\WinGet\\Packages");
            
        if (Directory.Exists(wingetPackages))
        {
            string[] ffmpegDirs = Directory.GetDirectories(wingetPackages, "Gyan.FFmpeg*");
            foreach (string dir in ffmpegDirs)
            {
                string[] binDirs = Directory.GetDirectories(dir, "*", SearchOption.AllDirectories)
                    .Where(d => d.EndsWith("\\bin")).ToArray();
                    
                foreach (string binDir in binDirs)
                {
                    string exePath = Path.Combine(binDir, "ffmpeg.exe");
                    if (File.Exists(exePath))
                    {
                        ffmpegPath = exePath;
                        UnityEngine.Debug.Log($"Found FFmpeg at: {ffmpegPath}");
                        return true;
                    }
                }
            }
        }
        
        UnityEngine.Debug.LogError("FFmpeg not found. Searched WinGet paths and system PATH.");
        return false;
    }
}
#endif
