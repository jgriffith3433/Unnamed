using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine;

[RequireComponent(typeof(CharacterController))]
public class FirstPersonController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private bool enableMovement = true;
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float sprintSpeed = 8f;
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private float jumpHeight = 1.5f;

    [Header("Mouse Look Settings")]
    [SerializeField] private bool enableMouseLook = true;
    [SerializeField] private float mouseSensitivity = 2f;
    [SerializeField] private Vector2 verticalLookLimits = new Vector2(-80f, 80f);

    [Header("Camera")]
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private CinemachineCamera cinemachineCamera;

    // Input values
    private Vector2 moveInput;
    private Vector2 lookInput;
    private bool sprintInput;
    private bool jumpInput;

    // Components
    private CharacterController characterController;

    // State
    private float verticalVelocity;
    private float rotationX = 0f;

    private void OnValidate()
    {
        if (characterController == null)
        {
            characterController = GetComponent<CharacterController>();
        }

        // Setup or find camera
        if (cameraTransform == null)
        {
            Transform existingCam = transform.Find("Camera");
            if (existingCam != null)
            {
                cameraTransform = existingCam;
            }
            else
            {
                GameObject camObj = new GameObject("Camera");
                cameraTransform = camObj.transform;
                cameraTransform.SetParent(transform);
                cameraTransform.localPosition = new Vector3(0, 1.6f, 0);
                cameraTransform.localRotation = Quaternion.identity;
            }
        }

        // Setup CinemachineCamera
        if (cinemachineCamera == null && cameraTransform != null)
        {
            cinemachineCamera = cameraTransform.GetComponent<CinemachineCamera>();
            if (cinemachineCamera == null)
            {
                cinemachineCamera = cameraTransform.gameObject.AddComponent<CinemachineCamera>();
                cinemachineCamera.Priority.Value = 10;
            }
        }
    }

    private void Awake()
    {
        if (enableMouseLook)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    private void Update()
    {
        HandleMovement();
        HandleMouseLook();
        HandleGravityAndJump();
    }

    private void HandleMovement()
    {
        if (!enableMovement) return;

        Vector3 move = transform.right * moveInput.x + transform.forward * moveInput.y;
        float currentSpeed = sprintInput ? sprintSpeed : moveSpeed;
        characterController.Move(move * currentSpeed * Time.deltaTime);
    }

    private void HandleMouseLook()
    {
        if (!enableMouseLook || cameraTransform == null) return;

        // Rotate player body horizontally
        float mouseX = lookInput.x * mouseSensitivity;
        transform.Rotate(Vector3.up * mouseX);

        // Rotate camera vertically with clamping
        float mouseY = lookInput.y * mouseSensitivity;
        rotationX -= mouseY;
        rotationX = Mathf.Clamp(rotationX, verticalLookLimits.x, verticalLookLimits.y);
        cameraTransform.localRotation = Quaternion.Euler(rotationX, 0f, 0f);
    }

    private void HandleGravityAndJump()
    {
        if (characterController.isGrounded && verticalVelocity < 0)
        {
            verticalVelocity = -2f;
        }

        if (jumpInput && characterController.isGrounded && enableMovement)
        {
            verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
            jumpInput = false;
        }

        verticalVelocity += gravity * Time.deltaTime;
        characterController.Move(Vector3.up * verticalVelocity * Time.deltaTime);
    }

    // Input System callbacks
    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    public void OnLook(InputValue value)
    {
        lookInput = value.Get<Vector2>();
    }

    public void OnSprint(InputValue value)
    {
        sprintInput = value.isPressed;
    }

    public void OnJump(InputValue value)
    {
        jumpInput = value.isPressed;
    }

    private void OnApplicationFocus(bool focus)
    {
        if (focus && enableMouseLook)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    // Public methods to toggle features at runtime
    public void SetMouseLookEnabled(bool enabled)
    {
        enableMouseLook = enabled;
        Cursor.lockState = enabled ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !enabled;
    }

    public void SetMovementEnabled(bool enabled)
    {
        enableMovement = enabled;
    }
}
