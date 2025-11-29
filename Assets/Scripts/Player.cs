using UnityEngine;
using Unity.Cinemachine;

/// <summary>
/// Player manager that handles possession of different controllers (character, horse, vehicle, etc.)
/// Manages input, camera, and controller switching
/// </summary>
public class Player : MonoBehaviour
{
    [Header("Input")]
    public PlayerInputManager inputManager;

    [Header("Camera")]
    public CinemachineCamera virtualCamera;

    [Header("Initial Setup")]
    public PossessableFirstPersonController initialController;
    public bool possessInitialControllerOnStart = true;

    private IPossessable _currentController;
    private Transform _originalParent;

    private void Awake()
    {
        if (inputManager == null)
        {
            inputManager = GetComponent<PlayerInputManager>();
            if (inputManager == null)
            {
                Debug.LogError("[Player] No PlayerInputManager found!");
            }
        }

        _originalParent = transform.parent;
    }

    private void Start()
    {
        if (possessInitialControllerOnStart && initialController != null)
        {
            Possess(initialController);
        }
    }

    /// <summary>
    /// Possess a new controller (character, horse, vehicle, etc.)
    /// </summary>
    public void Possess(IPossessable newController)
    {
        if (newController == null)
        {
            Debug.LogWarning("[Player] Cannot possess null controller");
            return;
        }

        // Unpossess current controller
        if (_currentController != null)
        {
            _currentController.OnUnpossessed();
        }

        // Possess new controller
        _currentController = newController;
        _currentController.OnPossessed(this);
    }

    /// <summary>
    /// Unpossess current controller
    /// </summary>
    public void Unpossess()
    {
        if (_currentController != null)
        {
            _currentController.OnUnpossessed();
            _currentController = null;
        }
    }

    /// <summary>
    /// Get the currently possessed controller
    /// </summary>
    public IPossessable GetCurrentController()
    {
        return _currentController;
    }

    /// <summary>
    /// Check if currently possessing a controller
    /// </summary>
    public bool IsPossessing()
    {
        return _currentController != null && _currentController.IsPossessed();
    }

    /// <summary>
    /// Get the player input manager
    /// </summary>
    public PlayerInputManager GetInput()
    {
        return inputManager;
    }

    /// <summary>
    /// Get the player's camera
    /// </summary>
    public CinemachineCamera GetCamera()
    {
        return virtualCamera;
    }

    /// <summary>
    /// Get the player's original parent transform
    /// </summary>
    public Transform GetOriginalParent()
    {
        return _originalParent;
    }
}
