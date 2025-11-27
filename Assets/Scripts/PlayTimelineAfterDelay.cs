using UnityEngine;
using UnityEngine.Playables;

public class PlayTimelineAfterDelay : MonoBehaviour
{
    [SerializeField] private PlayableDirector playableDirector;
    [SerializeField] private float delayTime = 2f;

    private void Start()
    {
        if (playableDirector != null)
        {
            Invoke(nameof(PlayTimeline), delayTime);
        }
    }

    private void PlayTimeline()
    {
        playableDirector.Play();
    }
}
