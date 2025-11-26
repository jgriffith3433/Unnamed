using UnityEngine;
using UnityEngine.Playables;
using System.Collections;

public class TimelineQuitListener : MonoBehaviour
{
    [SerializeField] private PlayableDirector playableDirector;
    [SerializeField] private float delayBeforeQuit = 3f;

    private void OnEnable()
    {
        if (playableDirector != null)
        {
            playableDirector.stopped += OnPlayableDirectorStopped;
        }
    }

    private void OnDisable()
    {
        if (playableDirector != null)
        {
            playableDirector.stopped -= OnPlayableDirectorStopped;
        }
    }

    private void OnPlayableDirectorStopped(PlayableDirector director)
    {
        StartCoroutine(QuitAfterDelay());
    }

    private IEnumerator QuitAfterDelay()
    {
        yield return new WaitForSeconds(delayBeforeQuit);
        
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }
}
