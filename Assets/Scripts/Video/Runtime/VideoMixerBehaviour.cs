using UnityEngine.Playables;

public class VideoMixerBehaviour : PlayableBehaviour
{
    // Simple mixer - no blending needed for video clips
    public override void ProcessFrame(Playable playable, FrameData info, object playerData)
    {
        // Individual clips handle their own processing
    }
}
