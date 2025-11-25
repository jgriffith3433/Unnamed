using UnityEngine;

/// <summary>
/// Handles mounting and dismounting for possessable controllers
/// Attach to each possessable (character, horse, vehicle, etc.)
/// </summary>
public class MountSystem : MonoBehaviour
{
    [Header("Detection")]
    [Tooltip("Trigger collider for detecting nearby mountables")]
    public Collider detectionTrigger;

    [Header("UI")]
    [Tooltip("UI GameObject to show when mounting is available")]
    public GameObject mountPromptUI;

    private Player _player;
    private IPossessable _thisPossessable;
    private IPossessable _nearbyMountable;

    private void Awake()
    {
        _thisPossessable = GetComponent<IPossessable>();
        if (_thisPossessable == null)
        {
            Debug.LogError($"[MountSystem] No IPossessable component found on {gameObject.name}");
        }

        // Find player in scene
        _player = FindObjectOfType<Player>();
        if (_player == null)
        {
            Debug.LogError("[MountSystem] No Player found in scene");
        }
    }

    private void OnEnable()
    {
        // Subscribe to mount input when this controller is possessed
        if (_player != null && _player.GetInput() != null)
        {
            _player.GetInput().OnMountPressed += OnMountPressed;
        }

        // Enable detection trigger
        if (detectionTrigger != null)
        {
            detectionTrigger.enabled = true;
        }

        // Hide UI initially
        UpdateMountUI();
    }

    private void OnDisable()
    {
        // Unsubscribe when this controller is unpossessed
        if (_player != null && _player.GetInput() != null)
        {
            _player.GetInput().OnMountPressed -= OnMountPressed;
        }

        // Disable detection trigger
        if (detectionTrigger != null)
        {
            detectionTrigger.enabled = false;
        }

        // Clear nearby mountable
        _nearbyMountable = null;

        // Hide UI
        UpdateMountUI();
    }

    private void OnTriggerEnter(Collider other)
    {
        IPossessable possessable = other.GetComponent<IPossessable>();
        if (IsValidMountTarget(possessable))
        {
            _nearbyMountable = possessable;
            UpdateMountUI();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        IPossessable possessable = other.GetComponent<IPossessable>();
        if (possessable == _nearbyMountable)
        {
            _nearbyMountable = null;
            UpdateMountUI();
        }
    }

    private void OnMountPressed()
    {
        if (!IsCurrentlyPossessed()) return;

        // Dismount from vehicle/horse
        if (ShouldDismount())
        {
            Dismount();
        }
        // Mount nearby target
        else if (_nearbyMountable != null)
        {
            Mount(_nearbyMountable);
        }
    }

    private bool IsCurrentlyPossessed()
    {
        return _player != null && _player.GetCurrentController() == _thisPossessable;
    }

    private bool IsValidMountTarget(IPossessable possessable)
    {
        return possessable != null && 
               possessable != _thisPossessable && 
               !possessable.IsPossessed();
    }

    private bool ShouldDismount()
    {
        // Horses and vehicles should dismount, characters shouldn't
        return _thisPossessable is PossessableHorseController;
    }

    private void Dismount()
    {
        // Find character controller to dismount to
        PossessableCharacterController character = FindObjectOfType<PossessableCharacterController>();
        if (character != null)
        {
            _player.Possess(character);
        }
    }

    private void Mount(IPossessable target)
    {
        _player.Possess(target);
    }

    private void UpdateMountUI()
    {
        if (mountPromptUI == null) return;

        // Show UI when we can mount (nearby mountable exists) or dismount (on a horse)
        bool canMount = _nearbyMountable != null;
        bool canDismount = ShouldDismount() && IsCurrentlyPossessed();
        
        mountPromptUI.SetActive(canMount || canDismount);
    }
}
