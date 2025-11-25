using UnityEngine;

/// <summary>
/// Horse controller that can be possessed by a Player
/// Handles horse movement with walk/trot/gallop speeds
/// </summary>
[RequireComponent(typeof(UnityEngine.CharacterController))]
public class PossessableHorseController : MonoBehaviour, IPossessable
{
    [Header("Movement")]
    [Tooltip("Walk speed of the horse in m/s")]
    public float WalkSpeed = 1.5f;

    [Tooltip("Trot speed of the horse in m/s")]
    public float TrotSpeed = 3.5f;

    [Tooltip("Gallop speed of the horse in m/s")]
    public float GallopSpeed = 8.0f;

    [Tooltip("Speed when backing up")]
    public float BackupSpeed = 1.0f;

    [Tooltip("Time in seconds to ramp from walk to max gallop speed")]
    public float SpeedRampTime = 3.0f;

    [Tooltip("Rotation speed when standing still (degrees/second)")]
    public float StandingTurnSpeed = 90.0f;

    [Tooltip("Rotation speed when walking (degrees/second)")]
    public float WalkingTurnSpeed = 60.0f;

    [Tooltip("Rotation speed when galloping (degrees/second)")]
    public float GallopingTurnSpeed = 30.0f;

    [Tooltip("Acceleration and deceleration")]
    public float SpeedChangeRate = 10.0f;

    [Header("Physics")]
    [Tooltip("The horse uses its own gravity value")]
    public float Gravity = -20.0f;

    [Tooltip("Maximum slope angle the horse can walk on (degrees)")]
    [Range(0f, 89f)]
    public float MaxSlopeAngle = 45f;

    [Tooltip("Speed at which horse sticks to slopes when moving downhill")]
    public float SlopeForce = 8.0f;

    [Header("Jumping")]
    [Tooltip("Whether the horse can jump")]
    public bool CanJump = false;

    [Tooltip("The height the horse can jump")]
    public float JumpHeight = 2.5f;

    [Tooltip("Time required to pass before being able to jump again")]
    public float JumpCooldown = 1.5f;

    [Header("Grounded")]
    [Tooltip("Useful for rough ground")]
    public float GroundedOffset = -0.2f;

    [Tooltip("The radius of the grounded check")]
    public float GroundedRadius = 0.6f;

    [Tooltip("Distance to check for ground ahead of horse")]
    public float GroundCheckDistance = 2.0f;

    [Tooltip("What layers the horse uses as ground")]
    public LayerMask GroundLayers;

    [Header("Camera")]
    [Tooltip("Camera follow target when riding (child of horse)")]
    public Transform horseCameraTarget;

    [Tooltip("Camera look-at target when riding (child of horse)")]
    public Transform horseLookAtTarget;

    [Header("Mount Point")]
    [Tooltip("Where the rider sits (child of horse)")]
    public Transform mountPoint;

    [Tooltip("Collider used for mount detection (separate from movement collider)")]
    public Collider mountTrigger;

    [Tooltip("Transform to reparent character to during jump animation")]
    public Transform jumpParent;

    [Header("Reign Handles")]
    [Tooltip("Left reign handle that will follow the character's left hand")]
    [SerializeField] private Transform leftReignHandle;

    [Tooltip("Right reign handle that will follow the character's right hand")]
    [SerializeField] private Transform rightReignHandle;

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
    private float _verticalVelocity;
    private float _terminalVelocity = 53.0f;
    private bool _grounded = true;
    private bool _wasGroundedLastFrame = true;
    private float _jumpCooldownTimer = 0f;
    private Vector3 _groundNormal = Vector3.up;
    private float _slopeAngle = 0f;
    private float _currentSpeedTime = 0f; // Time spent moving forward at current speed

    // Camera
    private float _cinemachineTargetYaw;
    private float _cinemachineTargetPitch;

    // Reign handles
    private Vector3 _originalLeftReignPosition;
    private Quaternion _originalLeftReignRotation;
    private Vector3 _originalRightReignPosition;
    private Quaternion _originalRightReignRotation;
    private PossessableCharacterController _mountedCharacter;
    private Animator _mountedCharacterAnimator;

    // Mount point tracking
    private Vector3 _originalMountPointLocalPosition;
    private bool _isJumping = false;
    private float _jumpTime = 0f;
    private const float JUMP_DURATION = 1.0f;

    // Animation IDs
    private int _animIDSpeed;
    private int _animIDJump;
    private int _animIDGrounded;
    private int _animIDFreeFall;

    private void Awake()
    {
        _animator = GetComponentInChildren<Animator>();
        _hasAnimator = _animator != null;

        if (_mainCamera == null)
        {
            _mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
        }

        // Create mount point if not assigned
        if (mountPoint == null)
        {
            GameObject mountPointObj = new GameObject("MountPoint");
            mountPointObj.transform.SetParent(transform);
            mountPointObj.transform.localPosition = new Vector3(0f, 1.5f, 0f);
            mountPointObj.transform.localRotation = Quaternion.identity;
            mountPoint = mountPointObj.transform;
            Debug.LogWarning($"Horse '{name}' had no mount point assigned. Created one at default position.");
        }

        AssignAnimationIDs();

        // Store original mount point position
        if (mountPoint != null)
        {
            _originalMountPointLocalPosition = mountPoint.localPosition;
        }

        // Start disabled
        enabled = false;
    }

    private void AssignAnimationIDs()
    {
        _animIDSpeed = Animator.StringToHash("Speed");
        _animIDJump = Animator.StringToHash("Jump");
        _animIDGrounded = Animator.StringToHash("Grounded");
        _animIDFreeFall = Animator.StringToHash("FreeFall");
    }

    public void OnPossessed(Player player)
    {
        _player = player;
        _isPossessed = true;

        // Disable mount trigger when possessed
        if (mountTrigger != null)
        {
            mountTrigger.enabled = false;
        }

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
            if (horseCameraTarget != null)
                _player.GetCamera().Follow = horseCameraTarget;
            if (horseLookAtTarget != null)
                _player.GetCamera().LookAt = horseLookAtTarget;

            _cinemachineTargetYaw = horseCameraTarget != null ? horseCameraTarget.rotation.eulerAngles.y : transform.eulerAngles.y;
        }

        // Parent the character controller to mount point
        // Note: Player is a separate manager, we need to find and parent the character controller
        PossessableCharacterController character = FindObjectOfType<PossessableCharacterController>();
        if (character != null && mountPoint != null)
        {
            character.transform.SetParent(mountPoint);
            character.transform.localPosition = Vector3.zero;
            character.transform.localRotation = Quaternion.identity;

            // Store character reference and animator
            _mountedCharacter = character;
            _mountedCharacterAnimator = character.GetComponent<Animator>();

            // Set OnHorse animator bool
            if (_mountedCharacterAnimator != null)
            {
                _mountedCharacterAnimator.SetBool("OnHorse", true);
            }

            // Store original reign positions
            if (leftReignHandle != null)
            {
                _originalLeftReignPosition = leftReignHandle.position;
                _originalLeftReignRotation = leftReignHandle.rotation;
            }
            if (rightReignHandle != null)
            {
                _originalRightReignPosition = rightReignHandle.position;
                _originalRightReignRotation = rightReignHandle.rotation;
            }
        }
    }

    public void OnUnpossessed()
    {
        // Reset reign handle positions
        if (leftReignHandle != null)
        {
            leftReignHandle.position = _originalLeftReignPosition;
            leftReignHandle.rotation = _originalLeftReignRotation;
        }
        if (rightReignHandle != null)
        {
            rightReignHandle.position = _originalRightReignPosition;
            rightReignHandle.rotation = _originalRightReignRotation;
        }

        // Unparent the character controller BEFORE disabling the horse
        PossessableCharacterController character = GetComponentInChildren<PossessableCharacterController>();
        if (character != null)
        {
            // Clear OnHorse animator bool
            if (_mountedCharacterAnimator != null)
            {
                _mountedCharacterAnimator.SetBool("OnHorse", false);
            }

            character.transform.SetParent(null);
            // Position character next to the horse
            character.transform.position = transform.position + transform.right * 1.5f;
        }

        _mountedCharacter = null;
        _mountedCharacterAnimator = null;

        _player = null;
        _isPossessed = false;

        // Re-enable mount trigger when unpossessed
        if (mountTrigger != null)
        {
            mountTrigger.enabled = true;
        }

        // disable MountSystem
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

        GroundedCheck();
        JumpAndGravity();
        Move();
        UpdateReignHandles();

        // Update jump timer
        if (_isJumping)
        {
            _jumpTime += Time.deltaTime;
        }

        // Update timers
        if (_jumpCooldownTimer > 0f)
        {
            _jumpCooldownTimer -= Time.deltaTime;
        }
    }

    private void LateUpdate()
    {
        if (!_isPossessed || _player == null) return;

        CameraRotation();
    }

    private void GroundedCheck()
    {
        _wasGroundedLastFrame = _grounded;

        // Sphere check for ground
        Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset,
            transform.position.z);
        _grounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers,
            QueryTriggerInteraction.Ignore);

        // Get ground normal and slope angle
        if (_grounded)
        {
            RaycastHit hit;
            if (Physics.Raycast(transform.position + Vector3.up * 0.1f, Vector3.down, out hit, 
                GroundedOffset + 1f, GroundLayers, QueryTriggerInteraction.Ignore))
            {
                _groundNormal = hit.normal;
                _slopeAngle = Vector3.Angle(Vector3.up, _groundNormal);
            }
            else
            {
                _groundNormal = Vector3.up;
                _slopeAngle = 0f;
            }
        }
        else
        {
            _groundNormal = Vector3.up;
            _slopeAngle = 0f;
        }

        // Update animator
        if (_hasAnimator)
        {
            _animator.SetBool(_animIDGrounded, _grounded);
            if (!_grounded && _wasGroundedLastFrame)
            {
                _animator.SetBool(_animIDFreeFall, true);
            }
            else if (_grounded && !_wasGroundedLastFrame)
            {
                _animator.SetBool(_animIDFreeFall, false);
            }
        }
    }

    private void CameraRotation()
    {
        PlayerInputManager input = _player.GetInput();
        if (input == null || horseCameraTarget == null) return;

        Vector2 lookInput = input.lookInput;

        if (lookInput.sqrMagnitude >= 0.01f)
        {
            _cinemachineTargetYaw += lookInput.x * Time.deltaTime;
            _cinemachineTargetPitch += lookInput.y * Time.deltaTime;
        }

        // Clamp pitch similar to character controller
        _cinemachineTargetYaw = ClampAngle(_cinemachineTargetYaw, float.MinValue, float.MaxValue);
        _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, -30f, 70f);

        horseCameraTarget.rotation = Quaternion.Euler(_cinemachineTargetPitch, _cinemachineTargetYaw, 0.0f);
    }

    private void Move()
    {
        PlayerInputManager input = _player.GetInput();
        if (input == null) return;

        Vector2 moveInput = input.moveInput;

        // Separate vertical (forward/back) and horizontal (turn) input
        float forwardInput = moveInput.y;  // -1 to 1
        float turnInput = moveInput.x;     // -1 to 1

        // Determine target speed based on forward input
        float targetSpeed = 0.0f;
        bool isBackingUp = false;

        if (forwardInput > 0.1f)
        {
            // Moving forward - ramp speed over time
            _currentSpeedTime += Time.deltaTime;
            
            // Calculate ramped speed from walk to gallop based on time
            float speedProgress = Mathf.Clamp01(_currentSpeedTime / SpeedRampTime);
            float maxSpeed = Mathf.Lerp(WalkSpeed, GallopSpeed, speedProgress);
            
            targetSpeed = maxSpeed * forwardInput; // Scale by input magnitude

            // Reduce speed on steep slopes
            if (_slopeAngle > MaxSlopeAngle * 0.7f)
            {
                targetSpeed *= Mathf.Lerp(1f, 0.3f, (_slopeAngle - MaxSlopeAngle * 0.7f) / (MaxSlopeAngle * 0.3f));
            }
        }
        else if (forwardInput < -0.1f)
        {
            // Backing up
            targetSpeed = BackupSpeed * Mathf.Abs(forwardInput);
            isBackingUp = true;
            _currentSpeedTime = 0f; // Reset speed ramp
        }
        else
        {
            // Not moving forward - reset speed ramp
            _currentSpeedTime = 0f;
        }

        // Current horizontal speed
        float currentHorizontalSpeed = new Vector3(_controller.velocity.x, 0.0f, _controller.velocity.z).magnitude;

        // If we hit something and velocity dropped significantly, reset the ramp
        if (currentHorizontalSpeed < _speed * 0.5f && forwardInput > 0.1f)
        {
            _currentSpeedTime = Mathf.Max(0f, _currentSpeedTime - Time.deltaTime * 2f);
        }

        float speedOffset = 0.1f;

        // Accelerate or decelerate
        if (currentHorizontalSpeed < targetSpeed - speedOffset ||
            currentHorizontalSpeed > targetSpeed + speedOffset)
        {
            _speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed, Time.deltaTime * SpeedChangeRate);
            _speed = Mathf.Round(_speed * 1000f) / 1000f;
        }
        else
        {
            _speed = targetSpeed;
        }

        _animationBlend = Mathf.Lerp(_animationBlend, targetSpeed, Time.deltaTime * SpeedChangeRate);
        if (_animationBlend < 0.01f) _animationBlend = 0f;

        // Handle rotation based on turn input
        if (Mathf.Abs(turnInput) > 0.1f)
        {
            // Determine turn speed based on current movement state
            float turnSpeed = StandingTurnSpeed;
            
            if (_speed > 0.1f)
            {
                // Interpolate turn speed based on actual speed
                float speedRatio = Mathf.Clamp01(_speed / GallopSpeed);
                turnSpeed = Mathf.Lerp(WalkingTurnSpeed, GallopingTurnSpeed, speedRatio);
            }

            // Apply rotation
            float rotation = turnInput * turnSpeed * Time.deltaTime;
            transform.Rotate(0, rotation, 0);
        }

        // Calculate movement direction
        Vector3 moveDirection = isBackingUp ? -transform.forward : transform.forward;

        // Project movement onto slope if grounded and on a slope
        if (_grounded && _slopeAngle > 0.1f && _slopeAngle < MaxSlopeAngle)
        {
            moveDirection = Vector3.ProjectOnPlane(moveDirection, _groundNormal).normalized;
        }

        // Apply movement with vertical velocity
        Vector3 movement = moveDirection * (_speed * Time.deltaTime) + 
                          new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime;

        // Add slope stick force when moving downhill
        if (_grounded && _slopeAngle > 5f && _speed > 0.1f)
        {
            movement.y -= SlopeForce * Time.deltaTime;
        }

        _controller.Move(movement);

        // Update animator
        if (_hasAnimator)
        {
            // Use signed speed for animation (negative when backing up)
            float animSpeed = isBackingUp ? -_animationBlend : _animationBlend;
            _animator.SetFloat(_animIDSpeed, animSpeed);
        }
    }

    private void UpdateReignHandles()
    {
        if (_mountedCharacter == null) return;

        // Update left reign handle to follow left hand
        if (leftReignHandle != null && _mountedCharacter.leftHand != null)
        {
            leftReignHandle.position = _mountedCharacter.leftHand.position;
            leftReignHandle.rotation = _mountedCharacter.leftHand.rotation;
        }

        // Update right reign handle to follow right hand
        if (rightReignHandle != null && _mountedCharacter.rightHand != null)
        {
            rightReignHandle.position = _mountedCharacter.rightHand.position;
            rightReignHandle.rotation = _mountedCharacter.rightHand.rotation;
        }

        // Update character animator speed to match horse speed
        if (_mountedCharacterAnimator != null)
        {
            _mountedCharacterAnimator.SetFloat("Speed", _animationBlend);
        }
    }

    private void UpdateMountPointPosition()
    {
        // No longer needed - character is reparented during jump
    }

    private void JumpAndGravity()
    {
        PlayerInputManager input = _player.GetInput();
        if (input == null) return;

        if (_grounded)
        {
            // Reset vertical velocity when grounded
            if (_verticalVelocity < 0.0f)
            {
                _verticalVelocity = -2f;
            }

            // Check if we just landed from a jump
            if (_isJumping && _jumpTime >= JUMP_DURATION)
            {
                _isJumping = false;
                _jumpTime = 0f;
                
                // Reparent character back to mount point
                if (_mountedCharacter != null && mountPoint != null)
                {
                    _mountedCharacter.transform.SetParent(mountPoint);
                    _mountedCharacter.transform.localPosition = new Vector3(0f, 0f, 0f);
                    _mountedCharacter.transform.localRotation = Quaternion.identity;
                }
            }

            // Handle jumping - only jump if grounded AND cooldown is finished
            if (input.jumpPressed && _jumpCooldownTimer <= 0f && CanJump && _slopeAngle < MaxSlopeAngle)
            {
                // Calculate jump velocity
                _verticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Gravity);
                _jumpCooldownTimer = JumpCooldown;

                // Reparent character to jump parent
                if (_mountedCharacter != null && jumpParent != null)
                {
                    _mountedCharacter.transform.SetParent(jumpParent);
                }

                // Trigger jump animation
                if (_hasAnimator)
                {
                    _animator.SetTrigger(_animIDJump);
                }

                // Trigger jump animation on mounted character
                if (_mountedCharacterAnimator != null)
                {
                    _mountedCharacterAnimator.SetTrigger("Jump");
                }

                // Start jump tracking
                _isJumping = true;
                _jumpTime = 0f;
            }
        }

        // Apply gravity over time if above terminal velocity (falling)
        if (_verticalVelocity > -_terminalVelocity)
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

        // Visualize mount point
        if (mountPoint != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(mountPoint.position, 0.2f);
            Gizmos.DrawLine(mountPoint.position, mountPoint.position + mountPoint.forward * 0.5f);
        }
    }

    public float GetCurrentSpeed()
    {
        return _speed;
    }
}
