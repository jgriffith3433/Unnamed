using UnityEngine;
using MobileControllerFramework;

/// <summary>
/// Controls the rotation of a game object based on input from PlayerInputManager.
/// Designed to work as a Cinemachine camera follow and look-at target.
/// Works with both mobile analog sticks and desktop mouse/gamepad input.
/// </summary>
public class CameraTargetController : MonoBehaviour
{
    [Header("Input")]
    [Tooltip("Reference to the PlayerInputManager that provides input.")]
    public PlayerInputManager inputManager;

    [Header("Rotation Settings")]
    [Tooltip("Horizontal rotation sensitivity for mobile analog stick (degrees per second).")]
    [Range(0f, 500f)]
    public float horizontalSensitivityMobile = 100f;

    [Tooltip("Vertical rotation sensitivity for mobile analog stick (degrees per second).")]
    [Range(0f, 500f)]
    public float verticalSensitivityMobile = 100f;

    [Tooltip("Horizontal rotation sensitivity for desktop mouse/gamepad.")]
    [Range(0f, 10f)]
    public float horizontalSensitivityDesktop = 2f;

    [Tooltip("Vertical rotation sensitivity for desktop mouse/gamepad.")]
    [Range(0f, 10f)]
    public float verticalSensitivityDesktop = 2f;

    [Tooltip("Invert horizontal axis.")]
    public bool invertHorizontal = false;

    [Tooltip("Invert vertical axis.")]
    public bool invertVertical = false;

    [Header("Rotation Limits")]
    [Tooltip("Enable vertical rotation clamping.")]
    public bool clampVerticalRotation = true;

    [Tooltip("Minimum vertical angle (looking down).")]
    [Range(-90f, 0f)]
    public float minVerticalAngle = -80f;

    [Tooltip("Maximum vertical angle (looking up).")]
    [Range(0f, 90f)]
    public float maxVerticalAngle = 80f;

    private float currentYaw = 0f;
    private float currentPitch = 0f;

    private void Start()
    {
        // Initialize rotation from current transform rotation
        Vector3 currentRotation = transform.eulerAngles;
        currentYaw = currentRotation.y;
        currentPitch = NormalizeAngle(currentRotation.x);

        // Auto-find PlayerInputManager if not assigned
        if (inputManager == null)
        {
            inputManager = FindObjectOfType<PlayerInputManager>();
            if (inputManager == null)
            {
                Debug.LogError("CameraTargetController: No PlayerInputManager found in scene!");
            }
        }
    }

    private void Update()
    {
        if (inputManager == null)
        {
            return;
        }

        // Get input from PlayerInputManager
        Vector2 lookInput = inputManager.lookInput;

        // Determine which sensitivity to use based on mobile/desktop mode
        bool isMobile = inputManager.IsMobileMode();
        float horizontalSensitivity = isMobile ? horizontalSensitivityMobile : horizontalSensitivityDesktop;
        float verticalSensitivity = isMobile ? verticalSensitivityMobile : verticalSensitivityDesktop;

        // Apply sensitivity and inversion
        float horizontalInput = lookInput.x * (invertHorizontal ? -1 : 1);
        float verticalInput = lookInput.y * (invertVertical ? -1 : 1);

        // Update rotation (mobile uses deltaTime, desktop uses direct delta values)
        if (isMobile)
        {
            currentYaw += horizontalInput * horizontalSensitivity * Time.deltaTime;
            currentPitch -= verticalInput * verticalSensitivity * Time.deltaTime;
        }
        else
        {
            currentYaw += horizontalInput * horizontalSensitivity;
            currentPitch -= verticalInput * verticalSensitivity;
        }

        // Clamp vertical rotation if enabled
        if (clampVerticalRotation)
        {
            currentPitch = Mathf.Clamp(currentPitch, minVerticalAngle, maxVerticalAngle);
        }

        // Apply rotation to transform
        transform.rotation = Quaternion.Euler(currentPitch, currentYaw, 0f);
    }

    /// <summary>
    /// Normalizes an angle to the range [-180, 180].
    /// </summary>
    private float NormalizeAngle(float angle)
    {
        while (angle > 180f) angle -= 360f;
        while (angle < -180f) angle += 360f;
        return angle;
    }

    /// <summary>
    /// Resets the rotation to the initial values.
    /// </summary>
    public void ResetRotation()
    {
        currentYaw = 0f;
        currentPitch = 0f;
        transform.rotation = Quaternion.identity;
    }

    /// <summary>
    /// Sets the rotation to specific yaw and pitch values.
    /// </summary>
    public void SetRotation(float yaw, float pitch)
    {
        currentYaw = yaw;
        currentPitch = clampVerticalRotation ? Mathf.Clamp(pitch, minVerticalAngle, maxVerticalAngle) : pitch;
        transform.rotation = Quaternion.Euler(currentPitch, currentYaw, 0f);
    }
}
