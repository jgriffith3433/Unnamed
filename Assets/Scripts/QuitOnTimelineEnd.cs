using UnityEngine;
using System.Collections;
using UnityEngine.Playables;

public class QuitOnTimelineEnd : MonoBehaviour
{
    [SerializeField] private PlayableDirector playableDirector;

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
        playableDirector.gameObject.SetActive(false);
        StartCoroutine(DeactivateAfterTime(3f));
    }

    private IEnumerator DeactivateAfterTime(float t)
    {
        yield return new WaitForSeconds(t);
        
        Debug.Log("Timeline ended. Quitting application...");
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
