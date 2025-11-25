using UnityEngine;
using UnityEngine.Playables;

/// <summary>
/// Plays a Timeline the first time the player mounts a horse, then quits the application when the timeline finishes.
/// </summary>
public class FirstMountTimelineController : MonoBehaviour
{
    [Header("Timeline Settings")]
    [Tooltip("The PlayableDirector that contains the timeline to play on first mount")]
    [SerializeField] private PlayableDirector playableDirector;

    [Tooltip("The horse to monitor for mounting")]
    [SerializeField] private PossessableHorseController horse;

    private bool hasPlayedTimeline = false;
    private bool isTimelinePlaying = false;

    private void Start()
    {
        // Validate references
        if (playableDirector == null)
        {
            Debug.LogError("FirstMountTimelineController: PlayableDirector is not assigned!");
            enabled = false;
            return;
        }

        if (horse == null)
        {
            Debug.LogError("FirstMountTimelineController: PossessableHorseController is not assigned!");
            enabled = false;
            return;
        }

        // Subscribe to the timeline stopped event
        playableDirector.stopped += OnTimelineStopped;
    }

    private void Update()
    {
        // Check if this is the first time the horse has been mounted
        if (!hasPlayedTimeline && !isTimelinePlaying && horse.IsPossessed())
        {
            PlayFirstMountTimeline();
        }
    }

    private void PlayFirstMountTimeline()
    {
        hasPlayedTimeline = true;
        isTimelinePlaying = true;

        Debug.Log("Playing first mount timeline...");
        
        // Play the timeline
        playableDirector.Play();
    }

    private void OnTimelineStopped(PlayableDirector director)
    {
        if (director == playableDirector && isTimelinePlaying)
        {
            Debug.Log("First mount timeline finished. Quitting application...");
            
            // Quit the application
            #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
            #else
                Application.Quit();
            #endif
        }
    }

    private void OnDestroy()
    {
        // Unsubscribe from the event to prevent memory leaks
        if (playableDirector != null)
        {
            playableDirector.stopped -= OnTimelineStopped;
        }
    }
}
