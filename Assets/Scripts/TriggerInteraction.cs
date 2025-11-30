using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

/// <summary>
/// Handles trigger interactions - when player enters trigger and presses Q,
/// sets PlayerPref and toggles game objects
/// </summary>
public class TriggerInteraction : MonoBehaviour
{
    private string playerPrefsKey = "startFight";
    [SerializeField] private GameObject[] objectsToDeactivate;
    [SerializeField] private GameObject[] objectsToActivate;
    [SerializeField] private GameObject objectToActivateWhenInTrigger;
    [SerializeField] private PlayableDirector playableDirector;
    
    private bool playerInTrigger = false;

    private void OnTriggerEnter(Collider other)
    {
        // Check if the player entered the trigger
        if (other.CompareTag("Player"))
        {
            if (objectToActivateWhenInTrigger != null)
            {
                objectToActivateWhenInTrigger.SetActive(true);
            }
            playerInTrigger = true;
            Debug.Log($"[TriggerInteraction] Player entered trigger zone. Press Q to interact.");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // Check if the player exited the trigger
        if (other.CompareTag("Player"))
        {
            if (objectToActivateWhenInTrigger != null)
            {
                objectToActivateWhenInTrigger.SetActive(false);
            }
            playerInTrigger = false;
            Debug.Log("[TriggerInteraction] Player exited trigger zone.");
        }
    }

    private void Update()
    {
        // Check if player is in trigger and presses the interaction key
        if (playerInTrigger && Keyboard.current != null && Keyboard.current.qKey.wasPressedThisFrame)
        {
            OnInteract();
        }
    }

    private void OnInteract()
    {
        Debug.Log($"[TriggerInteraction] Interaction triggered! Setting PlayerPref '{playerPrefsKey}' and toggling game objects.");
        
        // Set the PlayerPref
        PlayerPrefs.SetInt(playerPrefsKey, 0);
        PlayerPrefs.Save();
        
        // Pause the PlayableDirector
        if (playableDirector != null)
        {
            playableDirector.Pause();
            Debug.Log($"[TriggerInteraction] Paused PlayableDirector: {playableDirector.name}");
        }
        else
        {
            Debug.LogWarning("[TriggerInteraction] PlayableDirector is not assigned!");
        }
        
        // Deactivate the specified game objects
        if (objectsToDeactivate != null && objectsToDeactivate.Length > 0)
        {
            foreach (var obj in objectsToDeactivate)
            {
                if (obj != null)
                {
                    obj.SetActive(false);
                    Debug.Log($"[TriggerInteraction] Deactivated: {obj.name}");
                }
            }
        }

        if (objectToActivateWhenInTrigger != null)
        {
            objectToActivateWhenInTrigger.SetActive(false);
        }
        
        // Activate the specified game objects
        if (objectsToActivate != null && objectsToActivate.Length > 0)
        {
            foreach (var obj in objectsToActivate)
            {
                if (obj != null)
                {
                    obj.SetActive(true);
                    Debug.Log($"[TriggerInteraction] Activated: {obj.name}");
                }
            }
        }
        Application.Quit();
    }

    // Optional: Context menu for testing
    [ContextMenu("Clear PlayerPref")]
    private void ClearPlayerPref()
    {
        PlayerPrefs.DeleteKey(playerPrefsKey);
        PlayerPrefs.Save();
        Debug.Log($"[TriggerInteraction] Cleared PlayerPref '{playerPrefsKey}'");
    }
}
