using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using UnityEngine.Video;

[System.Serializable]
public class VideoPlayableAsset : PlayableAsset, ITimelineClipAsset
{
    public ExposedReference<VideoPlayer> videoPlayer;
    public Texture2D[] preloadedFrames;
    public float previewFPS = 30f;
    
    [HideInInspector]
    public bool framesExtracted = false;
    [HideInInspector]
    public string cachedVideoClipPath = "";
    
    [System.NonSerialized]
    private PlayableDirector cachedDirector;

    public ClipCaps clipCaps => ClipCaps.None;
    
    public override double duration
    {
        get
        {
            // Get duration from VideoPlayer using cached director
            if (cachedDirector != null)
            {
                var vp = videoPlayer.Resolve(cachedDirector);
                if (vp != null && vp.clip != null)
                {
                    return vp.clip.length;
                }
            }
            
            // Fallback: calculate from frame count if frames are loaded
            if (preloadedFrames != null && preloadedFrames.Length > 0 && previewFPS > 0)
            {
                return preloadedFrames.Length / (double)previewFPS;
            }

            return base.duration;
        }
    }

    public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
    {
        // Cache the director from the resolver
        var resolver = graph.GetResolver();
        if (resolver is PlayableDirector director)
        {
            cachedDirector = director;
        }
        
        var playable = ScriptPlayable<VideoBehaviour>.Create(graph);
        var behaviour = playable.GetBehaviour();

        behaviour.videoPlayer = videoPlayer.Resolve(graph.GetResolver());
        behaviour.preloadedFrames = preloadedFrames;
        behaviour.previewFPS = previewFPS;
        
        // Get the actual bound GameObject from the track, not the timeline owner
        behaviour.boundGameObject = null;
        if (cachedDirector != null)
        {
            // Try to get the VideoPlayer's GameObject which should be bound to the track
            var vp = behaviour.videoPlayer;
            if (vp != null)
            {
                behaviour.boundGameObject = vp.gameObject;
            }
        }
        
        // Fallback to owner if we couldn't find the bound object
        if (behaviour.boundGameObject == null)
        {
            behaviour.boundGameObject = owner;
        }
        
        // Pass video clip path for cache lookup
        behaviour.videoClipPath = cachedVideoClipPath;
        if (string.IsNullOrEmpty(behaviour.videoClipPath) && behaviour.videoPlayer != null && behaviour.videoPlayer.clip != null)
        {
#if UNITY_EDITOR
            behaviour.videoClipPath = UnityEditor.AssetDatabase.GetAssetPath(behaviour.videoPlayer.clip);
#else
            behaviour.videoClipPath = behaviour.videoPlayer.clip.name;
#endif
        }

        return playable;
    }
    
#if UNITY_EDITOR
    /// <summary>
    /// Gets the automatic frame extraction path based on the video clip location
    /// </summary>
    public string GetAutoFramePath(VideoPlayer vp)
    {
        if (vp == null || vp.clip == null) return null;
        
        string videoPath = UnityEditor.AssetDatabase.GetAssetPath(vp.clip);
        if (string.IsNullOrEmpty(videoPath)) return null;
        
        string directory = System.IO.Path.GetDirectoryName(videoPath);
        string videoName = System.IO.Path.GetFileNameWithoutExtension(videoPath);
        string framePath = $"{directory}/{videoName}_frames";
        
        return framePath;
    }
    
    /// <summary>
    /// Checks if frames have already been extracted for this video
    /// </summary>
    public bool CheckIfFramesExist(VideoPlayer vp)
    {
        string framePath = GetAutoFramePath(vp);
        if (string.IsNullOrEmpty(framePath)) return false;
        
        return UnityEditor.AssetDatabase.IsValidFolder(framePath) && 
               (System.IO.Directory.GetFiles(framePath, "frame_*.asset").Length > 0 ||
                System.IO.Directory.GetFiles(framePath, "frame_*.png").Length > 0);
    }
    
    /// <summary>
    /// Auto-loads frames from the automatic frame path
    /// </summary>
    public void AutoLoadFrames(VideoPlayer vp)
    {
        string framePath = GetAutoFramePath(vp);
        if (string.IsNullOrEmpty(framePath)) return;
        
        // Check for .png files first
        var pngFiles = System.IO.Directory.GetFiles(framePath, "frame_*.png");
        if (pngFiles.Length > 0)
        {
            System.Array.Sort(pngFiles);
            preloadedFrames = new Texture2D[pngFiles.Length];
            
            for (int i = 0; i < pngFiles.Length; i++)
            {
                string assetPath = pngFiles[i].Replace("\\", "/");
                preloadedFrames[i] = UnityEditor.AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
            }
        }
        else
        {
            // Fall back to .asset files
            var frameFiles = System.IO.Directory.GetFiles(framePath, "frame_*.asset");
            System.Array.Sort(frameFiles);
            
            preloadedFrames = new Texture2D[frameFiles.Length];
            for (int i = 0; i < frameFiles.Length; i++)
            {
                string assetPath = frameFiles[i].Replace("\\", "/");
                preloadedFrames[i] = UnityEditor.AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
            }
        }
        
        framesExtracted = true;
        cachedVideoClipPath = UnityEditor.AssetDatabase.GetAssetPath(vp.clip);
        
        UnityEditor.EditorUtility.SetDirty(this);
    }
#endif
}
