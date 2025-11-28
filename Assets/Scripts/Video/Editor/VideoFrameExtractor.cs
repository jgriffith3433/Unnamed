#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.Video;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

public class VideoFrameExtractor : EditorWindow
{
    private VideoPlayer videoPlayer;
    private float fps = 30f;
    private string outputPath = "Assets/VideoFrames/";
    private bool isExtracting = false;
    private float progress = 0f;

    [MenuItem("Tools/Video Frame Extractor")]
    public static void ShowWindow()
    {
        GetWindow<VideoFrameExtractor>("Video Frame Extractor");
    }

    void OnGUI()
    {
        GUILayout.Label("Video Frame Extraction", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        videoPlayer = (VideoPlayer)EditorGUILayout.ObjectField("Video Player", videoPlayer, typeof(VideoPlayer), true);
        fps = EditorGUILayout.FloatField("Preview FPS", fps);
        outputPath = EditorGUILayout.TextField("Output Path", outputPath);

        EditorGUILayout.Space();

        GUI.enabled = videoPlayer != null && !isExtracting;
        if (GUILayout.Button("Extract Frames", GUILayout.Height(30)))
        {
            ExtractFrames();
        }
        GUI.enabled = true;

        if (isExtracting)
        {
            EditorGUI.ProgressBar(EditorGUILayout.GetControlRect(), progress, $"Extracting... {Mathf.RoundToInt(progress * 100)}%");
        }

        EditorGUILayout.Space();
        EditorGUILayout.HelpBox(
            "This will extract all frames from the video at the specified FPS.\n" +
            "Frames will be saved as Texture2D assets in the output path.\n" +
            "You can then assign the folder to the Timeline clip's preloadedFrames array.",
            MessageType.Info
        );
    }

    void ExtractFrames()
    {
        if (videoPlayer == null || videoPlayer.clip == null)
        {
            EditorUtility.DisplayDialog("Error", "Please assign a VideoPlayer with a video clip.", "OK");
            return;
        }

        EditorCoroutineUtility.StartCoroutine(ExtractFramesCoroutine(), this);
    }
    
    /// <summary>
    /// Programmatic frame extraction with callback support
    /// </summary>
    public static void ExtractFrames(VideoPlayer videoPlayer, float fps, string outputPath, System.Action<bool, string> onComplete)
    {
        if (videoPlayer == null || videoPlayer.clip == null)
        {
            onComplete?.Invoke(false, "VideoPlayer or clip is null");
            return;
        }

        EditorCoroutineUtility.StartCoroutine(ExtractFramesCoroutine(videoPlayer, fps, outputPath, onComplete), null);
    }

    IEnumerator ExtractFramesCoroutine()
    {
        isExtracting = true;
        progress = 0f;

        // Create output directory
        if (!AssetDatabase.IsValidFolder(outputPath.TrimEnd('/')))
        {
            string parentFolder = System.IO.Path.GetDirectoryName(outputPath.TrimEnd('/'));
            string newFolder = System.IO.Path.GetFileName(outputPath.TrimEnd('/'));
            
            // Create parent folders recursively if needed
            CreateFolderRecursively(parentFolder);
            AssetDatabase.CreateFolder(parentFolder, newFolder);
        }

        // Setup video player for extraction
        RenderTexture tempRT = new RenderTexture((int)videoPlayer.clip.width, (int)videoPlayer.clip.height, 0, RenderTextureFormat.ARGB32);
        videoPlayer.targetTexture = tempRT;
        videoPlayer.renderMode = VideoRenderMode.RenderTexture;
        videoPlayer.isLooping = false;
        videoPlayer.playOnAwake = false;
        videoPlayer.Prepare();

        // Wait for video to be prepared
        while (!videoPlayer.isPrepared)
        {
            yield return null;
        }

        double videoDuration = videoPlayer.length;
        double frameInterval = 1.0 / fps;
        int totalFrames = Mathf.CeilToInt((float)(videoDuration * fps));

        for (int i = 0; i < totalFrames; i++)
        {
            double targetTime = i * frameInterval;
            videoPlayer.time = targetTime;
            videoPlayer.Play();

            // Wait a frame for video to render
            yield return null;
            yield return null;

            videoPlayer.Pause();

            // Read frame from RenderTexture
            Texture2D frameTexture = new Texture2D(tempRT.width, tempRT.height, TextureFormat.RGB24, false);
            RenderTexture.active = tempRT;
            frameTexture.ReadPixels(new Rect(0, 0, tempRT.width, tempRT.height), 0, 0);
            frameTexture.Apply();
            RenderTexture.active = null;

            // Save frame as asset
            string framePath = $"{outputPath}/frame_{i:D5}.asset";
            AssetDatabase.CreateAsset(frameTexture, framePath);

            progress = (float)(i + 1) / totalFrames;
            Repaint();

            if (i % 10 == 0)
            {
                yield return null; // Prevent editor freeze
            }
        }

        // Cleanup
        videoPlayer.Stop();
        tempRT.Release();
        DestroyImmediate(tempRT);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        isExtracting = false;
        progress = 1f;
        Repaint();

        EditorUtility.DisplayDialog("Success", $"Extracted {totalFrames} frames to {outputPath}", "OK");
    }
    
    static IEnumerator ExtractFramesCoroutine(VideoPlayer videoPlayer, float fps, string outputPath, System.Action<bool, string> onComplete)
    {
        // Create output directory
        if (!AssetDatabase.IsValidFolder(outputPath.TrimEnd('/')))
        {
            CreateFolderRecursively(outputPath);
        }

        // Setup video player for extraction
        RenderTexture tempRT = new RenderTexture((int)videoPlayer.clip.width, (int)videoPlayer.clip.height, 0, RenderTextureFormat.ARGB32);
        
        // Store original settings
        var originalTarget = videoPlayer.targetTexture;
        var originalRenderMode = videoPlayer.renderMode;
        var originalIsLooping = videoPlayer.isLooping;
        var originalPlayOnAwake = videoPlayer.playOnAwake;
        
        videoPlayer.targetTexture = tempRT;
        videoPlayer.renderMode = VideoRenderMode.RenderTexture;
        videoPlayer.isLooping = false;
        videoPlayer.playOnAwake = false;
        videoPlayer.Prepare();

        // Wait for video to be prepared
        int timeout = 0;
        while (!videoPlayer.isPrepared && timeout < 300)
        {
            timeout++;
            yield return null;
        }
        
        if (!videoPlayer.isPrepared)
        {
            tempRT.Release();
            onComplete?.Invoke(false, "Video failed to prepare");
            yield break;
        }

        double videoDuration = videoPlayer.length;
        double frameInterval = 1.0 / fps;
        int totalFrames = Mathf.CeilToInt((float)(videoDuration * fps));

        for (int i = 0; i < totalFrames; i++)
        {
            double targetTime = i * frameInterval;
            
            // Stop before seeking to ensure clean state
            videoPlayer.Stop();
            yield return null;
            
            // Seek to target time
            videoPlayer.time = targetTime;
            videoPlayer.Play();

            // Wait for video to actually seek and render the frame
            int seekWait = 0;
            while (Mathf.Abs((float)(videoPlayer.time - targetTime)) > 0.01f && seekWait < 30)
            {
                seekWait++;
                yield return null;
            }
            
            // Extra wait for frame to fully render
            yield return null;
            yield return null;

            videoPlayer.Pause();

            // Read frame from RenderTexture
            Texture2D frameTexture = new Texture2D(tempRT.width, tempRT.height, TextureFormat.RGB24, false);
            RenderTexture.active = tempRT;
            frameTexture.ReadPixels(new Rect(0, 0, tempRT.width, tempRT.height), 0, 0);
            frameTexture.Apply();
            RenderTexture.active = null;

            // Save frame as asset
            string framePath = $"{outputPath}/frame_{i:D5}.asset";
            AssetDatabase.CreateAsset(frameTexture, framePath);

            if (i % 10 == 0)
            {
                EditorUtility.DisplayProgressBar("Extracting Frames", $"Frame {i + 1}/{totalFrames}", (float)(i + 1) / totalFrames);
                yield return null;
            }
        }

        // Cleanup
        videoPlayer.Stop();
        
        // Restore original settings
        videoPlayer.targetTexture = originalTarget;
        videoPlayer.renderMode = originalRenderMode;
        videoPlayer.isLooping = originalIsLooping;
        videoPlayer.playOnAwake = originalPlayOnAwake;
        
        tempRT.Release();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        EditorUtility.ClearProgressBar();

        onComplete?.Invoke(true, $"Extracted {totalFrames} frames to {outputPath}");
    }
    
    static void CreateFolderRecursively(string path)
    {
        path = path.TrimEnd('/');
        if (string.IsNullOrEmpty(path) || path == "Assets") return;
        if (AssetDatabase.IsValidFolder(path)) return;
        
        string parentFolder = System.IO.Path.GetDirectoryName(path).Replace("\\", "/");
        string newFolder = System.IO.Path.GetFileName(path);
        
        CreateFolderRecursively(parentFolder);
        
        if (!string.IsNullOrEmpty(newFolder) && !AssetDatabase.IsValidFolder(path))
        {
            AssetDatabase.CreateFolder(parentFolder, newFolder);
        }
    }
}

// Helper class for editor coroutines
public static class EditorCoroutineUtility
{
    public static void StartCoroutine(IEnumerator routine, EditorWindow window)
    {
        if (window != null)
        {
            window.StartCoroutine(routine);
        }
        else
        {
            StartCoroutineStatic(routine);
        }
    }
    
    private static void StartCoroutineStatic(IEnumerator routine)
    {
        EditorApplication.CallbackFunction callback = null;
        callback = () =>
        {
            try
            {
                if (!routine.MoveNext())
                {
                    EditorApplication.update -= callback;
                }
            }
            catch (System.Exception e)
            {
                Debug.LogException(e);
                EditorApplication.update -= callback;
            }
        };
        EditorApplication.update += callback;
    }

    private static void StartCoroutine(this EditorWindow window, IEnumerator routine)
    {
        EditorApplication.CallbackFunction callback = null;
        callback = () =>
        {
            try
            {
                if (!routine.MoveNext())
                {
                    EditorApplication.update -= callback;
                }
            }
            catch (System.Exception e)
            {
                Debug.LogException(e);
                EditorApplication.update -= callback;
            }
        };
        EditorApplication.update += callback;
    }
}
#endif
