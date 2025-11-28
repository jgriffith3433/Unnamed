using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Video;
using UnityEngine.UI;

public class VideoBehaviour : PlayableBehaviour
{
    public VideoPlayer videoPlayer;
    public Texture2D[] preloadedFrames;
    public float previewFPS = 30f;
    public GameObject boundGameObject;
    public string videoClipPath;

    private RawImage targetRawImage;
    private int lastFrameIndex = -1;

    public override void OnGraphStart(Playable playable)
    {
        // Find RawImage on the bound GameObject
        if (boundGameObject != null)
        {
            targetRawImage = boundGameObject.GetComponentInChildren<RawImage>();
        }

        // Ensure VideoPlayer is disabled (kept for frame extraction only)
        if (videoPlayer != null)
        {
            videoPlayer.enabled = false;
            videoPlayer.Stop();
        }
    }

    public override void ProcessFrame(Playable playable, FrameData info, object playerData)
    {
        // Lazily find RawImage if not found yet
        if (targetRawImage == null && boundGameObject != null)
        {
            targetRawImage = boundGameObject.GetComponentInChildren<RawImage>();
        }

        if (targetRawImage == null) return;

        double time = playable.GetTime();
        
        // Calculate frame index from timeline time
        int frameIndex = Mathf.FloorToInt((float)time * previewFPS);
        
        // Always update if frame changed OR if we don't have a frame displayed yet
        if (frameIndex != lastFrameIndex || targetRawImage.texture == null)
        {
            Texture2D frameTexture = null;

            // Try to get frame from cache first
            if (!string.IsNullOrEmpty(videoClipPath))
            {
                frameTexture = VideoFrameCache.GetFrame(videoClipPath, frameIndex);
            }

            // Fallback to preloaded frames array
            if (frameTexture == null && preloadedFrames != null && frameIndex >= 0 && frameIndex < preloadedFrames.Length)
            {
                frameTexture = preloadedFrames[frameIndex];
            }

            // Update RawImage texture
            if (frameTexture != null)
            {
                targetRawImage.texture = frameTexture;
                lastFrameIndex = frameIndex;
            }
        }
    }

    public override void OnBehaviourPause(Playable playable, FrameData info)
    {
        // Keep displaying the current frame when paused
        // ProcessFrame will handle updates
    }

    public override void OnGraphStop(Playable playable)
    {
        // Don't restore texture - leave the last frame visible
        lastFrameIndex = -1;
    }
}
