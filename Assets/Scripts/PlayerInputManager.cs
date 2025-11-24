using UnityEngine;
using UnityEngine.InputSystem;
using MobileControllerFramework;
using System;

/// <summary>
/// Centralized input manager that handles both mobile and desktop input
/// Can be shared across different controllers (player, horse, vehicle, etc.)
/// </summary>
public class PlayerInputManager : MonoBehaviour
{
    [Header("Mobile Controls")]
    [Tooltip("Mobile analog stick for movement")]
    public Analog movementAnalog;

    [Tooltip("Mobile analog stick for camera rotation")]
    public Analog cameraAnalog;

    [Tooltip("Mobile button for jumping")]
    public Button jumpButton;

    [Tooltip("Mobile button for sprinting")]
    public Button sprintButton;

    [Tooltip("Mobile button for mounting/dismounting")]
    public Button mountButton;

    [Header("Input System")]
    [Tooltip("Input actions asset for desktop controls")]
    public InputActionAsset inputActions;

    [Header("Input State")]
    public Vector2 moveInput;
    public Vector2 lookInput;
    public bool jumpPressed;
    public bool sprintPressed;

    // Event for mount button
    public event Action OnMountPressed;

    private bool _useMobileControls;
    private InputActionMap _playerActionMap;
    private InputAction _moveAction;
    private InputAction _lookAction;
    private InputAction _jumpAction;
    private InputAction _sprintAction;
    private InputAction _mountAction;

    private void Awake()
    {
        // Determine if we should use mobile controls
#if UNITY_EDITOR && (UNITY_ANDROID || UNITY_IOS)
        _useMobileControls = true;
#elif !UNITY_EDITOR
        _useMobileControls = Application.isMobilePlatform;
#endif

        // Hook up mobile button events
        if (_useMobileControls)
        {
            if (jumpButton != null)
            {
                jumpButton.TapStartEvent.AddListener(() => jumpPressed = true);
                jumpButton.TapEndEvent.AddListener(() => jumpPressed = false);
            }

            if (sprintButton != null)
            {
                sprintButton.TapStartEvent.AddListener(() => sprintPressed = true);
                sprintButton.TapEndEvent.AddListener(() => sprintPressed = false);
            }

            if (mountButton != null)
            {
                mountButton.TapEndEvent.AddListener(() => OnMountPressed?.Invoke());
            }
        }
        else
        {
            // Set up Input System for desktop
            SetupInputSystem();
        }
    }

    private void SetupInputSystem()
    {
        if (inputActions == null)
        {
            Debug.LogWarning("PlayerInputManager: InputActionAsset is not assigned!");
            return;
        }

        _playerActionMap = inputActions.FindActionMap("Player");
        if (_playerActionMap == null)
        {
            Debug.LogError("PlayerInputManager: Could not find 'Player' action map!");
            return;
        }

        _moveAction = _playerActionMap.FindAction("Move");
        _lookAction = _playerActionMap.FindAction("Look");
        _jumpAction = _playerActionMap.FindAction("Jump");
        _sprintAction = _playerActionMap.FindAction("Sprint");
        _mountAction = _playerActionMap.FindAction("Mount");

        // Subscribe to mount button
        if (_mountAction != null)
        {
            _mountAction.performed += ctx => OnMountPressed?.Invoke();
        }

        _playerActionMap.Enable();
    }

    private void OnEnable()
    {
        _playerActionMap?.Enable();
    }

    private void OnDisable()
    {
        _playerActionMap?.Disable();
    }

    private void Update()
    {
        GatherInput();
    }

    private void GatherInput()
    {
        if (_useMobileControls)
        {
            // Mobile input
            if (movementAnalog != null)
            {
                moveInput = movementAnalog.GetStickPosition();
            }
            else
            {
                moveInput = Vector2.zero;
            }

            if (cameraAnalog != null)
            {
                lookInput = cameraAnalog.GetStickPosition();
            }
            else
            {
                lookInput = Vector2.zero;
            }

            // Jump and sprint handled by button events
        }
        else
        {
            // Desktop input using Input System
            if (_moveAction != null)
            {
                moveInput = _moveAction.ReadValue<Vector2>();
            }

            if (_lookAction != null)
            {
                lookInput = _lookAction.ReadValue<Vector2>();
            }

            if (_jumpAction != null)
            {
                jumpPressed = _jumpAction.IsPressed();
            }

            if (_sprintAction != null)
            {
                sprintPressed = _sprintAction.IsPressed();
            }
        }
    }

    public bool IsMobileMode()
    {
        return _useMobileControls;
    }
}
