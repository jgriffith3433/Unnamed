using UnityEngine;

/// <summary>
/// First-person controller that can be possessed by a Player
/// Handles first-person movement and camera control without animations
/// </summary>
[RequireComponent(typeof(UnityEngine.CharacterController))]
public class PossessableFirstPersonController : MonoBehaviour, IPossessable
{
    [Header("Movement")]
    [Tooltip("Move speed of the character in m/s")]
    public float MoveSpeed = 5.0f;

    [Tooltip("Sprint speed of the character in m/s")]
    public float SprintSpeed = 8.0f;

    [Tooltip("Acceleration and deceleration")]
    public float SpeedChangeRate = 10.0f;

    [Header("Jumping")]
    [Tooltip("Whether the character can jump")]
    public bool CanJump = true;

    [Tooltip("The height the character can jump")]
    public float JumpHeight = 1.5f;

    [Tooltip("Time required to pass before being able to jump again")]
    public float JumpTimeout = 0.30f;

    [Tooltip("Time required to pass before entering the fall state")]
    public float FallTimeout = 0.10f;

    [Header("Physics")]
    [Tooltip("The character uses its own gravity value")]
    public float Gravity = -20.0f;

    [Tooltip("Gravity multiplier when falling")]
    public float FallGravityMultiplier = 1.5f;

    [Header("Grounded")]
    [Tooltip("Useful for rough ground")]
    public float GroundedOffset = -0.14f;

    [Tooltip("The radius of the grounded check")]
    public float GroundedRadius = 0.28f;

    [Tooltip("What layers the character uses as ground")]
    public LayerMask GroundLayers;

    [Header("Camera")]
    [Tooltip("The camera target that follows this character")]
    public Transform cameraTarget;

    [Tooltip("How far in degrees can you move the camera up")]
    public float TopClamp = 70.0f;

    [Tooltip("How far in degrees can you move the camera down")]
    public float BottomClamp = -30.0f;

    [Tooltip("Camera sensitivity")]
    public float CameraSensitivity = 0.1f;

    // References
    private Player _player;
    [SerializeField] private UnityEngine.CharacterController _controller;
    private bool _isPossessed;

    // State
    private float _speed;
    private float _verticalVelocity;
    private float _terminalVelocity = 53.0f;
    private float _jumpTimeoutDelta;
    private float _fallTimeoutDelta;
    private bool _grounded = true;

    // Camera
    private float _cinemachineTargetYaw;
    private float _cinemachineTargetPitch;

    private void Awake()
    {
        // Lock and hide cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Get or add CharacterController
        if (_controller == null)
        {
            _controller = GetComponent<UnityEngine.CharacterController>();
        }

        if (_controller.enabled == false)
        {
            _controller.enabled = true;
        }

        // Start disabled
        _controller.enabled = false;
        enabled = false;
    }

    public void OnPossessed(Player player)
    {
        _player = player;
        _isPossessed = true;

        // Enable controller
        _controller.enabled = true;
        enabled = true;

        // Setup camera
        if (_player.GetCamera() != null && cameraTarget != null)
        {
            _player.GetCamera().Follow = cameraTarget;
            _player.GetCamera().LookAt = cameraTarget;

            _cinemachineTargetYaw = cameraTarget.rotation.eulerAngles.y;
            _cinemachineTargetPitch = cameraTarget.rotation.eulerAngles.x;
        }

        // Reset timeouts
        _jumpTimeoutDelta = JumpTimeout;
        _fallTimeoutDelta = FallTimeout;
    }

    public void OnUnpossessed()
    {
        _player = null;
        _isPossessed = false;

        // Disable controller
        _controller.enabled = false;
        enabled = false;
    }

    public bool IsPossessed()
    {
        return _isPossessed;
    }

    private void Update()
    {
        if (!_isPossessed || _player == null) return;

        GroundedCheck();
        JumpAndGravity();
        Move();
    }

    private void LateUpdate()
    {
        if (!_isPossessed || _player == null) return;

        CameraRotation();
    }

    private void GroundedCheck()
    {
        Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset,
            transform.position.z);
        _grounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers,
            QueryTriggerInteraction.Ignore);
    }

    private void CameraRotation()
    {
        PlayerInputManager input = _player.GetInput();
        if (input == null || cameraTarget == null) return;

        Vector2 lookInput = input.lookInput;

        if (lookInput.sqrMagnitude >= 0.01f)
        {
            _cinemachineTargetYaw += lookInput.x * CameraSensitivity;
            _cinemachineTargetPitch -= lookInput.y * CameraSensitivity; // Inverted for natural look up/down
        }

        _cinemachineTargetYaw = ClampAngle(_cinemachineTargetYaw, float.MinValue, float.MaxValue);
        _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp);

        // Apply rotation to camera target
        cameraTarget.rotation = Quaternion.Euler(_cinemachineTargetPitch, _cinemachineTargetYaw, 0.0f);

        // Also rotate the character body to match camera yaw (for first-person)
        transform.rotation = Quaternion.Euler(0.0f, _cinemachineTargetYaw, 0.0f);
    }

    private void Move()
    {
        PlayerInputManager input = _player.GetInput();
        if (input == null) return;

        Vector2 moveInput = input.moveInput;
        bool sprint = input.sprintPressed;

        // Set target speed
        float targetSpeed = sprint ? SprintSpeed : MoveSpeed;
        if (moveInput == Vector2.zero) targetSpeed = 0.0f;

        // Current horizontal speed
        float currentHorizontalSpeed = new Vector3(_controller.velocity.x, 0.0f, _controller.velocity.z).magnitude;

        float speedOffset = 0.1f;
        float inputMagnitude = moveInput.magnitude;

        // Accelerate or decelerate
        if (currentHorizontalSpeed < targetSpeed - speedOffset ||
            currentHorizontalSpeed > targetSpeed + speedOffset)
        {
            _speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude,
                Time.deltaTime * SpeedChangeRate);
            _speed = Mathf.Round(_speed * 1000f) / 1000f;
        }
        else
        {
            _speed = targetSpeed;
        }

        // Calculate movement direction relative to camera
        Vector3 inputDirection = new Vector3(moveInput.x, 0.0f, moveInput.y).normalized;
        
        // Convert input to world space relative to camera direction
        Vector3 moveDirection = transform.right * inputDirection.x + transform.forward * inputDirection.z;

        // Move the character
        _controller.Move(moveDirection.normalized * (_speed * Time.deltaTime) +
                         new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);
    }

    private void JumpAndGravity()
    {
        PlayerInputManager input = _player.GetInput();
        if (input == null) return;

        if (_grounded)
        {
            _fallTimeoutDelta = FallTimeout;

            if (_verticalVelocity < 0.0f)
            {
                _verticalVelocity = -2f;
            }

            // Jump
            if (input.jumpPressed && _jumpTimeoutDelta <= 0.0f && CanJump)
            {
                _verticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Gravity);
            }

            if (_jumpTimeoutDelta >= 0.0f)
            {
                _jumpTimeoutDelta -= Time.deltaTime;
            }
        }
        else
        {
            _jumpTimeoutDelta = JumpTimeout;

            if (_fallTimeoutDelta >= 0.0f)
            {
                _fallTimeoutDelta -= Time.deltaTime;
            }
        }

        // Apply gravity with fall multiplier for more realistic falling
        if (_verticalVelocity > -_terminalVelocity)
        {
            float gravityToApply = _verticalVelocity < 0 ? Gravity * FallGravityMultiplier : Gravity;
            _verticalVelocity += gravityToApply * Time.deltaTime;
        }
    }

    private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
    {
        if (lfAngle < -360f) lfAngle += 360f;
        if (lfAngle > 360f) lfAngle -= 360f;
        return Mathf.Clamp(lfAngle, lfMin, lfMax);
    }

    private void OnDrawGizmosSelected()
    {
        Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
        Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

        if (_grounded) Gizmos.color = transparentGreen;
        else Gizmos.color = transparentRed;

        Gizmos.DrawSphere(
            new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z),
            GroundedRadius);
    }

    public float GetCurrentSpeed()
    {
        return _speed;
    }
}
