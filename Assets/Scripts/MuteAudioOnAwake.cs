using UnityEngine;

public class MuteAudioOnAwake : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;

    private void Awake()
    {
        if (audioSource != null)
        {
            audioSource.mute = true;
        }
    }
}
