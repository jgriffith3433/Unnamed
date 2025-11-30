using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using UnityEngine.Video;
using UnityEngine.SceneManagement;
using Gaia;

/// <summary>
/// Loads all video frames before playing the timeline
/// </summary>
public class PlayTimelineAfterLoad : MonoBehaviour
{
    [SerializeField] private PlayableDirector playableDirector;
    [SerializeField] private bool showLoadingProgress = true;
    private double startTime = 0.0;
    
    private int totalFramesToLoad = 0;
    private int framesLoaded = 0;
    private bool isLoading = false;

    [ContextMenu("Clear Checkpoint")]
    private void ClearCheckpoint()
    {
        PlayerPrefs.DeleteKey("startFight");
        PlayerPrefs.Save();
        Debug.Log("[PlayTimelineAfterLoad] Checkpoint cleared!");
    }

    private void Start()
    {
        // Check for checkpoint
        if (PlayerPrefs.GetInt("startFight", 0) == 1)
        {
            startTime = 173.0;
        }
        
        if (playableDirector != null)
        {
            // Subscribe to the stopped event
            playableDirector.stopped += OnTimelineStopped;
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
        }
        else
        {
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
                Debug.Log($"[PlayTimelineAfterLoad] Finished loading {framesLoaded} frames.");
            }

            // Disable all VideoPlayer components (keep for frame extraction fallback)
            DisableAllVideoPlayers();
        }

        // Wait for all terrains to load
        Debug.Log("[PlayTimelineAfterLoad] Waiting for terrain loading to complete...");
        yield return StartCoroutine(WaitForTerrainsToLoad());
        Debug.Log("[PlayTimelineAfterLoad] All terrains loaded. Starting timeline playback.");

        // Lock and hide cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        // Start timeline playback
        playableDirector.time = startTime;
        playableDirector.Play();
        isLoading = false;
    }

    private IEnumerator WaitForTerrainsToLoad()
    {
        TerrainLoaderManager loaderManager = TerrainLoaderManager.Instance;
        if (loaderManager == null || loaderManager.TerrainSceneStorage == null)
        {
            Debug.LogWarning("[PlayTimelineAfterLoad] TerrainLoaderManager not found, skipping terrain load wait.");
            yield break;
        }

        // Force all terrains to load by requesting them from all loaders
        TerrainLoader[] allLoaders = FindObjectsOfType<TerrainLoader>(true);
        Debug.Log($"[PlayTimelineAfterLoad] Found {allLoaders.Length} TerrainLoader components");
        
        foreach (TerrainLoader loader in allLoaders)
        {
            if (loader.enabled && loader.gameObject.activeInHierarchy)
            {
                loader.UpdateTerrains();
            }
        }

        // Wait until all terrains are loaded
        int maxWaitFrames = 600; // 10 seconds at 60fps
        int frameCount = 0;
        
        while (frameCount < maxWaitFrames)
        {
            bool allLoaded = true;
            int loadedCount = 0;
            int totalCount = 0;

            foreach (var terrainScene in loaderManager.TerrainSceneStorage.m_terrainScenes)
            {
                totalCount++;
                
                // Check if this terrain has any references (meaning something requested it)
                bool hasReferences = terrainScene.RegularReferences.Count > 0 || terrainScene.ImpostorReferences.Count > 0;
                
                if (hasReferences)
                {
                    // If it has references, check if it's loaded
                    if (terrainScene.m_regularLoadState == LoadState.Loaded || terrainScene.m_impostorLoadState == LoadState.Loaded)
                    {
                        loadedCount++;
                    }
                    else
                    {
                        allLoaded = false;
                    }
                }
            }

            if (frameCount % 30 == 0) // Log every half second
            {
                Debug.Log($"[PlayTimelineAfterLoad] Terrain loading progress: {loadedCount} terrains loaded");
            }

            if (allLoaded && loadedCount > 0)
            {
                Debug.Log($"[PlayTimelineAfterLoad] All {loadedCount} referenced terrains are loaded!");
                break;
            }

            frameCount++;
            yield return null;
        }

        if (frameCount >= maxWaitFrames)
        {
            Debug.LogWarning("[PlayTimelineAfterLoad] Timeout waiting for terrains to load. Proceeding anyway.");
        }
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

    private void OnTimelineStopped(PlayableDirector director)
    {
        // Unlock and show cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        // Check if the "finished" PlayerPref is set
        if (!PlayerPrefs.HasKey("finished"))
        {
            // PlayerPref is not set, reload the current scene
            Debug.Log("[PlayTimelineAfterLoad] Timeline stopped and 'finished' PlayerPref is not set. Reloading scene.");
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }

    private void OnDestroy()
    {
        // Unsubscribe from the stopped event
        if (playableDirector != null)
        {
            playableDirector.stopped -= OnTimelineStopped;
        }
        
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
