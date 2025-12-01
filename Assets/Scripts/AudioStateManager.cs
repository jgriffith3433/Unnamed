using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AudioStateManager : MonoBehaviour
{
    private AudioSource audioSource;
    private bool wasPlayingBeforeDisable;
    private bool hasStarted;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        Debug.Log("AudioStateManager: Awake");
    }

    private void OnEnable()
    {
        Debug.Log("AudioStateManager: OnEnable - hasStarted: " + hasStarted + ", wasPlayingBeforeDisable: " + wasPlayingBeforeDisable);
        
        if (audioSource == null) return;

        // If we haven't started yet, wait for Start() to play
        if (!hasStarted) return;

        // Resume audio that was paused
        if (wasPlayingBeforeDisable)
        {
            audioSource.UnPause();
            Debug.Log("AudioStateManager: RESUMING audio from: " + audioSource.time);
        }
    }

    private void Start()
    {
        hasStarted = true;
        Debug.Log("AudioStateManager: Start - playOnAwake: " + audioSource.playOnAwake);
        
        // Play audio on first start if it has a clip and Play On Awake is off
        if (audioSource != null && audioSource.clip != null && !audioSource.playOnAwake)
        {
            audioSource.Play();
            Debug.Log("AudioStateManager: PLAYING audio from Start");
        }
    }

    private void OnDisable()
    {
        if (audioSource == null) return;
        
        Debug.Log("AudioStateManager: OnDisable - isPlaying: " + audioSource.isPlaying + ", time: " + audioSource.time + ", clip: " + (audioSource.clip != null ? audioSource.clip.name : "null"));

        wasPlayingBeforeDisable = audioSource.isPlaying;
        
        if (audioSource.isPlaying)
        {
            audioSource.Pause();
            Debug.Log("AudioStateManager: PAUSING audio at: " + audioSource.time);
        }
        else
        {
            Debug.Log("AudioStateManager: Audio was NOT playing, won't resume on enable");
        }
    }
}
