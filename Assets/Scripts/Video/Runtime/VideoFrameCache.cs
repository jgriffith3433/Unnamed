using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Global cache for video frames to avoid duplicate loading
/// </summary>
public static class VideoFrameCache
{
    // Dictionary structure: [VideoClipPath][FrameIndex] = Texture2D
    private static Dictionary<string, Dictionary<int, Texture2D>> frameCache = new Dictionary<string, Dictionary<int, Texture2D>>();

    /// <summary>
    /// Load all frames for a video into the cache
    /// </summary>
    public static void LoadFrames(string videoClipPath, Texture2D[] frames)
    {
        if (string.IsNullOrEmpty(videoClipPath) || frames == null || frames.Length == 0)
            return;

        if (!frameCache.ContainsKey(videoClipPath))
        {
            frameCache[videoClipPath] = new Dictionary<int, Texture2D>();
        }

        var videoCache = frameCache[videoClipPath];
        for (int i = 0; i < frames.Length; i++)
        {
            if (frames[i] != null)
            {
                videoCache[i] = frames[i];
            }
        }
    }

    /// <summary>
    /// Get a specific frame from the cache
    /// </summary>
    public static Texture2D GetFrame(string videoClipPath, int frameIndex)
    {
        if (string.IsNullOrEmpty(videoClipPath) || !frameCache.ContainsKey(videoClipPath))
            return null;

        var videoCache = frameCache[videoClipPath];
        if (videoCache.ContainsKey(frameIndex))
        {
            return videoCache[frameIndex];
        }

        return null;
    }

    /// <summary>
    /// Check if frames are loaded for a video
    /// </summary>
    public static bool HasFrames(string videoClipPath)
    {
        return !string.IsNullOrEmpty(videoClipPath) && 
               frameCache.ContainsKey(videoClipPath) && 
               frameCache[videoClipPath].Count > 0;
    }

    /// <summary>
    /// Get the number of frames loaded for a video
    /// </summary>
    public static int GetFrameCount(string videoClipPath)
    {
        if (!HasFrames(videoClipPath))
            return 0;

        return frameCache[videoClipPath].Count;
    }

    /// <summary>
    /// Clear all cached frames (call when changing scenes or to free memory)
    /// </summary>
    public static void ClearCache()
    {
        frameCache.Clear();
    }

    /// <summary>
    /// Clear frames for a specific video
    /// </summary>
    public static void ClearVideo(string videoClipPath)
    {
        if (frameCache.ContainsKey(videoClipPath))
        {
            frameCache.Remove(videoClipPath);
        }
    }
}
