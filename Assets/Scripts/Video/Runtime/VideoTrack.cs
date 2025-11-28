using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[TrackColor(0.2f, 0.6f, 0.9f)]
[TrackClipType(typeof(VideoPlayableAsset))]
[TrackBindingType(typeof(GameObject))]
public class VideoTrack : TrackAsset
{
    public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
    {
        return ScriptPlayable<VideoMixerBehaviour>.Create(graph, inputCount);
    }
}
