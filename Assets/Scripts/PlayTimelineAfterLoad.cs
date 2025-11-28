using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using UnityEngine.Video;

/// <summary>
/// Loads all video frames before playing the timeline
/// </summary>
public class PlayTimelineAfterLoad : MonoBehaviour
{
    [SerializeField] private PlayableDirector playableDirector;
    [SerializeField] private bool showLoadingProgress = true;
    
#if UNITY_EDITOR
    [SerializeField] private double startTime = 0.0;
#endif
    
    private int totalFramesToLoad = 0;
    private int framesLoaded = 0;
    private bool isLoading = false;

    private void Start()
    {
        if (playableDirector != null)
        {
            StartCoroutine(LoadFramesAndPlay());
        }
    }

    private IEnumerator LoadFramesAndPlay()
    {
        isLoading = true;
        
        if (showLoadingProgress)
        {
            Debug.Log("[PlayTimelineAfterLoad] Starting frame loading...");
        }

        // Get all VideoPlayableAssets from the timeline
        List<VideoPlayableAsset> videoAssets = GetAllVideoAssets();
        
        if (videoAssets.Count == 0)
        {
            Debug.LogWarning("[PlayTimelineAfterLoad] No VideoPlayableAssets found in timeline.");
            playableDirector.Play();
            isLoading = false;
            yield break;
        }

        // Calculate total frames to load
        totalFramesToLoad = 0;
        foreach (var asset in videoAssets)
        {
            if (asset.preloadedFrames != null)
            {
                totalFramesToLoad += asset.preloadedFrames.Length;
            }
        }

        if (showLoadingProgress)
        {
            Debug.Log($"[PlayTimelineAfterLoad] Loading {totalFramesToLoad} frames from {videoAssets.Count} video(s)...");
        }

        // Load frames into cache
        framesLoaded = 0;
        foreach (var asset in videoAssets)
        {
            yield return StartCoroutine(LoadAssetFrames(asset));
        }

        if (showLoadingProgress)
        {
            Debug.Log($"[PlayTimelineAfterLoad] Finished loading {framesLoaded} frames. Starting timeline playback.");
        }

        // Disable all VideoPlayer components (keep for frame extraction fallback)
        DisableAllVideoPlayers();

        // Start timeline playback
#if UNITY_EDITOR
        playableDirector.time = startTime;
#endif
        playableDirector.Play();
        isLoading = false;
    }

    private IEnumerator LoadAssetFrames(VideoPlayableAsset asset)
    {
        if (asset.preloadedFrames == null || asset.preloadedFrames.Length == 0)
            yield break;

        // Get video clip path for cache key
        string videoClipPath = asset.cachedVideoClipPath;
        if (string.IsNullOrEmpty(videoClipPath))
        {
            // Try to resolve from VideoPlayer reference
            var vp = asset.videoPlayer.Resolve(playableDirector);
            if (vp != null && vp.clip != null)
            {
#if UNITY_EDITOR
                videoClipPath = UnityEditor.AssetDatabase.GetAssetPath(vp.clip);
#else
                videoClipPath = vp.clip.name;
#endif
            }
        }

        // Skip if already loaded
        if (VideoFrameCache.HasFrames(videoClipPath))
        {
            framesLoaded += asset.preloadedFrames.Length;
            yield break;
        }

        // Load frames into cache
        VideoFrameCache.LoadFrames(videoClipPath, asset.preloadedFrames);
        framesLoaded += asset.preloadedFrames.Length;

        if (showLoadingProgress && framesLoaded % 50 == 0)
        {
            Debug.Log($"[PlayTimelineAfterLoad] Progress: {framesLoaded}/{totalFramesToLoad} frames loaded ({(framesLoaded * 100f / totalFramesToLoad):F1}%)");
        }

        // Yield every 10 frames to avoid blocking
        if (framesLoaded % 10 == 0)
        {
            yield return null;
        }
    }

    private List<VideoPlayableAsset> GetAllVideoAssets()
    {
        List<VideoPlayableAsset> assets = new List<VideoPlayableAsset>();

        if (playableDirector.playableAsset == null)
            return assets;

        var timelineAsset = playableDirector.playableAsset as TimelineAsset;
        if (timelineAsset == null)
            return assets;

        // Iterate through all tracks
        foreach (var track in timelineAsset.GetOutputTracks())
        {
            var videoTrack = track as VideoTrack;
            if (videoTrack != null)
            {
                // Get all clips in this track
                foreach (var clip in videoTrack.GetClips())
                {
                    var videoAsset = clip.asset as VideoPlayableAsset;
                    if (videoAsset != null)
                    {
                        assets.Add(videoAsset);
                    }
                }
            }
        }

        return assets;
    }

    private void DisableAllVideoPlayers()
    {
        // Get all VideoPlayer components referenced by the timeline
        var videoAssets = GetAllVideoAssets();
        foreach (var asset in videoAssets)
        {
            var vp = asset.videoPlayer.Resolve(playableDirector);
            if (vp != null)
            {
                vp.enabled = false;
            }
        }
    }

    private void OnDestroy()
    {
        // Optional: Clear cache when this object is destroyed
        // VideoFrameCache.ClearCache();
    }

    // Public methods for external control
    public bool IsLoading => isLoading;
    public float LoadProgress => totalFramesToLoad > 0 ? (float)framesLoaded / totalFramesToLoad : 0f;

#if UNITY_EDITOR
    [ContextMenu("Sample Current Timeline Time")]
    private void SampleCurrentTimelineTime()
    {
        if (playableDirector != null)
        {
            startTime = playableDirector.time;
            Debug.Log($"[PlayTimelineAfterLoad] Sampled timeline time: {startTime:F3} seconds");
            UnityEditor.EditorUtility.SetDirty(this);
        }
        else
        {
            Debug.LogWarning("[PlayTimelineAfterLoad] No PlayableDirector assigned!");
        }
    }
#endif
}
