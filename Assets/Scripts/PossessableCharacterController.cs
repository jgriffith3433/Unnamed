using UnityEngine;

/// <summary>
/// Character controller that can be possessed by a Player
/// Handles humanoid character movement, jumping, and animations
/// </summary>
[RequireComponent(typeof(UnityEngine.CharacterController))]
public class PossessableCharacterController : MonoBehaviour, IPossessable
{
    [Header("Movement")]
    [Tooltip("Move speed of the character in m/s")]
    public float MoveSpeed = 2.0f;

    [Tooltip("Sprint speed of the character in m/s")]
    public float SprintSpeed = 5.335f;

    [Tooltip("How fast the character turns to face movement direction")]
    [Range(0.0f, 0.3f)]
    public float RotationSmoothTime = 0.12f;

    [Tooltip("Acceleration and deceleration")]
    public float SpeedChangeRate = 10.0f;

    [Header("Jumping")]
    [Tooltip("Whether the character can jump")]
    public bool CanJump = true;

    [Tooltip("The height the character can jump")]
    public float JumpHeight = 1.2f;

    [Tooltip("Time required to pass before being able to jump again")]
    public float JumpTimeout = 0.50f;

    [Tooltip("Time required to pass before entering the fall state")]
    public float FallTimeout = 0.15f;

    [Header("Physics")]
    [Tooltip("The character uses its own gravity value")]
    public float Gravity = -15.0f;

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

    [Tooltip("The camera look-at target")]
    public Transform lookAtTarget;

    [Tooltip("How far in degrees can you move the camera up")]
    public float TopClamp = 70.0f;

    [Tooltip("How far in degrees can you move the camera down")]
    public float BottomClamp = -30.0f;

    [Header("Hands")]
    [Tooltip("Left hand transform (for holding reigns)")]
    public Transform leftHand;

    [Tooltip("Right hand transform (for holding reigns)")]
    public Transform rightHand;

    // References
    private Player _player;
    [SerializeField] private UnityEngine.CharacterController _controller;
    private Animator _animator;
    private GameObject _mainCamera;
    private bool _hasAnimator;
    private bool _isPossessed;

    // State
    private float _speed;
    private float _animationBlend;
    private float _targetRotation = 0.0f;
    private float _rotationVelocity;
    private float _verticalVelocity;
    private float _terminalVelocity = 53.0f;
    private float _jumpTimeoutDelta;
    private float _fallTimeoutDelta;
    private bool _grounded = true;

    // Camera
    private float _cinemachineTargetYaw;
    private float _cinemachineTargetPitch;

    // Animation IDs
    private int _animIDSpeed;
    private int _animIDGrounded;
    private int _animIDJump;
    private int _animIDFreeFall;
    private int _animIDMotionSpeed;

    private void Awake()
    {
        if (_controller.enabled == false)
        {
            _controller.enabled = true;
        }
        _hasAnimator = TryGetComponent(out _animator);

        if (_mainCamera == null)
        {
            _mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
        }

        AssignAnimationIDs();

        // Start disabled
        _controller.enabled = false;
        enabled = false;
    }

    private void AssignAnimationIDs()
    {
        _animIDSpeed = Animator.StringToHash("Speed");
        _animIDGrounded = Animator.StringToHash("Grounded");
        _animIDJump = Animator.StringToHash("Jump");
        _animIDFreeFall = Animator.StringToHash("FreeFall");
        _animIDMotionSpeed = Animator.StringToHash("MotionSpeed");
    }

    public void OnPossessed(Player player)
    {
        _player = player;
        _isPossessed = true;

        // Enable controller and MountSystem
        _controller.enabled = true;
        enabled = true;
        
        // Enable MountSystem if it exists
        MountSystem mountSystem = GetComponent<MountSystem>();
        if (mountSystem != null)
        {
            mountSystem.enabled = true;
        }

        // Setup camera
        if (_player.GetCamera() != null)
        {
            if (cameraTarget != null)
                _player.GetCamera().Follow = cameraTarget;
            if (lookAtTarget != null)
                _player.GetCamera().LookAt = lookAtTarget;

            _cinemachineTargetYaw = cameraTarget.rotation.eulerAngles.y;
        }

        // Reset timeouts
        _jumpTimeoutDelta = JumpTimeout;
        _fallTimeoutDelta = FallTimeout;
    }

    public void OnUnpossessed()
    {
        _player = null;
        _isPossessed = false;

        // Disable controller and MountSystem
        _controller.enabled = false;
        enabled = false;
        
        // Disable MountSystem if it exists
        MountSystem mountSystem = GetComponent<MountSystem>();
        if (mountSystem != null)
        {
            mountSystem.enabled = false;
        }
    }

    public bool IsPossessed()
    {
        return _isPossessed;
    }

    private void Update()
    {
        if (!_isPossessed || _player == null) return;

        _hasAnimator = TryGetComponent(out _animator);

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

        if (_hasAnimator)
        {
            _animator.SetBool(_animIDGrounded, _grounded);
        }
    }

    private void CameraRotation()
    {
        PlayerInputManager input = _player.GetInput();
        if (input == null || cameraTarget == null) return;

        Vector2 lookInput = input.lookInput;

        if (lookInput.sqrMagnitude >= 0.01f)
        {
            _cinemachineTargetYaw += lookInput.x * Time.deltaTime;
            _cinemachineTargetPitch += lookInput.y * Time.deltaTime;
        }

        _cinemachineTargetYaw = ClampAngle(_cinemachineTargetYaw, float.MinValue, float.MaxValue);
        _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp);

        cameraTarget.rotation = Quaternion.Euler(_cinemachineTargetPitch, _cinemachineTargetYaw, 0.0f);
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

        _animationBlend = Mathf.Lerp(_animationBlend, targetSpeed, Time.deltaTime * SpeedChangeRate);
        if (_animationBlend < 0.01f) _animationBlend = 0f;

        // Normalize input direction
        Vector3 inputDirection = new Vector3(moveInput.x, 0.0f, moveInput.y).normalized;

        // Rotate to face input direction
        if (moveInput != Vector2.zero)
        {
            _targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg +
                              _mainCamera.transform.eulerAngles.y;
            float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRotation, ref _rotationVelocity,
                RotationSmoothTime);

            transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
        }

        Vector3 targetDirection = Quaternion.Euler(0.0f, _targetRotation, 0.0f) * Vector3.forward;

        // Move the character
        _controller.Move(targetDirection.normalized * (_speed * Time.deltaTime) +
                         new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);

        // Update animator
        if (_hasAnimator)
        {
            _animator.SetFloat(_animIDSpeed, _animationBlend);
            _animator.SetFloat(_animIDMotionSpeed, inputMagnitude);
        }
    }

    private void JumpAndGravity()
    {
        PlayerInputManager input = _player.GetInput();
        if (input == null) return;

        if (_grounded)
        {
            _fallTimeoutDelta = FallTimeout;

            if (_hasAnimator)
            {
                _animator.SetBool(_animIDJump, false);
                _animator.SetBool(_animIDFreeFall, false);
            }

            if (_verticalVelocity < 0.0f)
            {
                _verticalVelocity = -2f;
            }

            // Jump
            if (input.jumpPressed && _jumpTimeoutDelta <= 0.0f && CanJump)
            {
                _verticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Gravity);

                if (_hasAnimator)
                {
                    _animator.SetBool(_animIDJump, true);
                }
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
            else
            {
                if (_hasAnimator)
                {
                    _animator.SetBool(_animIDFreeFall, true);
                }
            }
        }

        if (_verticalVelocity < _terminalVelocity)
        {
            _verticalVelocity += Gravity * Time.deltaTime;
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
