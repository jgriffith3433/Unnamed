using UnityEngine;

/// <summary>
/// Turkey controller that can be possessed by a Player
/// Handles turkey movement, jumping, and flying mechanics
/// </summary>
[RequireComponent(typeof(UnityEngine.CharacterController))]
public class PossessableTurkeyController : MonoBehaviour, IPossessable
{
    [Header("Movement")]
    [Tooltip("Move speed of the turkey in m/s")]
    public float MoveSpeed = 2.0f;

    [Tooltip("Sprint speed of the turkey in m/s")]
    public float SprintSpeed = 5.335f;

    [Tooltip("How fast the turkey turns to face movement direction")]
    [Range(0.0f, 0.3f)]
    public float RotationSmoothTime = 0.12f;

    [Tooltip("Acceleration and deceleration")]
    public float SpeedChangeRate = 10.0f;

    [Header("Jumping")]
    [Tooltip("The height the turkey can jump")]
    private float JumpHeight = .5f;

    [Tooltip("Time required to pass before being able to jump again")]
    private float JumpTimeout = 0.50f;

    [Tooltip("Time required to pass before entering the fall state")]
    private float FallTimeout = 0.15f;

    [Header("Flying")]
    [Tooltip("Flying speed when holding space in air")]
    private float FlyingSpeed = 3.0f;

    [Tooltip("Upward force applied when flying")]
    public float FlyingUpwardForce = .5f;

    [Tooltip("Maximum height above ground the turkey can fly (in meters)")]
    private float MaxFlyingHeight = 3.5f;

    [Header("Physics")]
    [Tooltip("The turkey uses its own gravity value")]
    public float Gravity = -15.0f;

    [Header("Grounded")]
    [Tooltip("Useful for rough ground")]
    private float GroundedOffset = -0.04f;

    [Tooltip("The radius of the grounded check")]
    private float GroundedRadius = 0.08f;

    [Tooltip("What layers the turkey uses as ground")]
    public LayerMask GroundLayers;

    [Header("Camera")]
    [Tooltip("The camera target that follows this turkey")]
    public Transform cameraTarget;

    [Tooltip("The camera look-at target")]
    public Transform lookAtTarget;

    [Tooltip("How far in degrees can you move the camera up")]
    public float TopClamp = 70.0f;

    [Tooltip("How far in degrees can you move the camera down")]
    public float BottomClamp = -30.0f;

    [Header("Hands")]
    [Tooltip("Left hand transform")]
    public Transform leftHand;

    [Tooltip("Right hand transform")]
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
    private bool _isFlying = false;
    private float _groundHeight = 0f;
    private int _frameCounter = 0;

    // Camera
    private float _cinemachineTargetYaw;
    private float _cinemachineTargetPitch;

    // Animation IDs
    private int _animIDSpeed;
    private int _animIDGrounded;
    private int _animIDJump;
    private int _animIDFlying;

    private void Awake()
    {
        // Get CharacterController if not assigned
        if (_controller == null)
        {
            _controller = GetComponent<UnityEngine.CharacterController>();
        }
        
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

        // Lock and hide cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Start disabled
        _controller.enabled = false;
        enabled = false;
    }

    private void AssignAnimationIDs()
    {
        _animIDSpeed = Animator.StringToHash("Speed");
        _animIDGrounded = Animator.StringToHash("Grounded");
        _animIDJump = Animator.StringToHash("Jump");
        _animIDFlying = Animator.StringToHash("Flying");
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
        UpdateGroundHeight();
        HandleFlyingAndJumping();
        Move();
    }

    private void LateUpdate()
    {
        if (!_isPossessed || _player == null) return;

        CameraRotation();
    }

    private void GroundedCheck()
    {
        _grounded = _controller.isGrounded;

        if (_hasAnimator)
        {
            _animator.SetBool(_animIDGrounded, _grounded);
        }
    }

    private void UpdateGroundHeight()
    {
        // Update ground height every 10 frames for performance
        _frameCounter++;
        if (_frameCounter >= 10)
        {
            _frameCounter = 0;
            
            // Raycast downward to find ground height
            RaycastHit hit;
            if (Physics.Raycast(transform.position, Vector3.down, out hit, 1000f, GroundLayers, QueryTriggerInteraction.Ignore))
            {
                _groundHeight = hit.point.y;
            }
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

        // Set target speed - increase speed when in the air (not grounded)
        float targetSpeed;
        if (!_grounded)
        {
            targetSpeed = FlyingSpeed * 1.3f; // 30% faster in air
        }
        else
        {
            targetSpeed = sprint ? SprintSpeed : MoveSpeed;
        }
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

        // Rotate to face input direction with smoother rotation in air
        if (moveInput != Vector2.zero)
        {
            _targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg +
                              _mainCamera.transform.eulerAngles.y;
            
            // Use slower rotation when in the air for smoother turning
            float smoothTime = _grounded ? RotationSmoothTime : RotationSmoothTime * 2.5f;
            float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRotation, ref _rotationVelocity,
                smoothTime);

            transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
        }

        Vector3 targetDirection = Quaternion.Euler(0.0f, _targetRotation, 0.0f) * Vector3.forward;

        // Move the turkey
        _controller.Move(targetDirection.normalized * (_speed * Time.deltaTime) +
                         new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);

        // Update animator
        if (_hasAnimator)
        {
            _animator.SetFloat(_animIDSpeed, _animationBlend);
        }
    }

    private void HandleFlyingAndJumping()
    {
        PlayerInputManager input = _player.GetInput();
        if (input == null) return;

        if (_grounded)
        {
            _fallTimeoutDelta = FallTimeout;

            // Stop flying when grounded
            _isFlying = false;
            if (_hasAnimator)
            {
                _animator.SetBool(_animIDFlying, false);
            }

            if (_verticalVelocity < 0.0f)
            {
                _verticalVelocity = -2f;
            }

            // Jump - only when grounded and not already jumping
            if (input.jumpPressed && _jumpTimeoutDelta <= 0.0f && _verticalVelocity <= 0f)
            {
                _verticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Gravity);

                if (_hasAnimator)
                {
                    _animator.SetTrigger(_animIDJump);
                }
            }

            if (_jumpTimeoutDelta >= 0.0f)
            {
                _jumpTimeoutDelta -= Time.deltaTime;
            }
        }
        else // In the air
        {
            _jumpTimeoutDelta = JumpTimeout;

            // Check current height above ground - add small offset to avoid ground interference
            float currentHeightAboveGround = Mathf.Max(0, transform.position.y - _groundHeight - 0.5f);

            // Flying logic: holding space while in air (set Flying true regardless of height)
            if (input.jumpPressed)
            {
                _isFlying = true;
                
                // Calculate how close we are to max height (0 = at ground, 1 = at max height)
                float heightRatio = Mathf.Clamp01(currentHeightAboveGround / MaxFlyingHeight);
                
                // Scale the upward force based on height - less force as we get higher
                float scaledUpwardForce = FlyingUpwardForce * (1f - heightRatio * 0.7f);
                
                // Apply upward force gradually, fighting against gravity
                _verticalVelocity += scaledUpwardForce * Time.deltaTime * 3f;
                
                // Clamp max upward velocity
                _verticalVelocity = Mathf.Min(_verticalVelocity, FlyingUpwardForce * 0.8f);
                
                if (_hasAnimator)
                {
                    _animator.SetBool(_animIDFlying, true);
                }
            }
            else
            {
                _isFlying = false;
                
                if (_hasAnimator)
                {
                    _animator.SetBool(_animIDFlying, true); // Still play flying animation when gliding
                }

                // Handle fall timeout for FreeFall animation
                if (_fallTimeoutDelta >= 0.0f)
                {
                    _fallTimeoutDelta -= Time.deltaTime;
                }
                else
                {
                    if (_hasAnimator)
                    {
                        _animator.SetBool(_animIDFlying, false);
                    }
                }
            }
        }

        // Apply gravity - always active but reduced when actively flying
        if (_verticalVelocity > -_terminalVelocity)
        {
            // Reduced gravity when flying, normal-ish when gliding
            float gravityMultiplier = _isFlying ? 0.4f : 0.6f;
            _verticalVelocity += Gravity * gravityMultiplier * Time.deltaTime;
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
