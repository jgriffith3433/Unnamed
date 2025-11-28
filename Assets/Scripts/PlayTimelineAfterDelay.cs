using System.Collections;
using UnityEngine;
using UnityEngine.Playables;

public class PlayTimelineAfterDelay : MonoBehaviour
{
    [SerializeField] private PlayableDirector playableDirector;
    [SerializeField] private float delayTime = 2f;

#if UNITY_EDITOR
    [SerializeField] private bool startAtSpecificTime = false;
    [SerializeField] private float editorStartTime = 0f;
#endif

    private void Start()
    {
        if (playableDirector != null)
        {
#if UNITY_EDITOR
            if (startAtSpecificTime)
            {
                playableDirector.time = editorStartTime;
                playableDirector.Play();
                return;
            }
#endif
            StartCoroutine(PlayTimelineAfterDelayCoroutine());
        }
    }

    private IEnumerator PlayTimelineAfterDelayCoroutine()
    {
        yield return new WaitForSeconds(delayTime);
        playableDirector.Play();
    }
}
