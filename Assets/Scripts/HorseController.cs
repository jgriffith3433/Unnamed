using UnityEngine;
#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
using UnityEngine.InputSystem;
#endif

namespace StarterAssets
{
    /// <summary>
    /// Horse movement controller - similar to ThirdPersonController but for horse locomotion
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
    [RequireComponent(typeof(PlayerInput))]
#endif
    public class HorseController : MonoBehaviour
    {
        [Header("Horse Movement")]
        [Tooltip("Walk speed of the horse in m/s")]
        public float WalkSpeed = 2.0f;

        [Tooltip("Trot speed of the horse in m/s")]
        public float TrotSpeed = 4.0f;

        [Tooltip("Gallop speed of the horse in m/s")]
        public float GallopSpeed = 7.0f;

        [Tooltip("How fast the horse turns")]
        [Range(0.0f, 0.3f)]
        public float RotationSmoothTime = 0.12f;

        [Tooltip("Acceleration and deceleration")]
        public float SpeedChangeRate = 10.0f;

        [Header("Horse Grounded")]
        [Tooltip("If the horse is grounded or not")]
        public bool Grounded = true;

        [Tooltip("Useful for rough ground")]
        public float GroundedOffset = -0.14f;

        [Tooltip("The radius of the grounded check")]
        public float GroundedRadius = 0.5f;

        [Tooltip("What layers the horse uses as ground")]
        public LayerMask GroundLayers;

        [Header("Horse Settings")]
        [Tooltip("The character uses its own gravity value")]
        public float Gravity = -15.0f;

        [Tooltip("Can the horse be controlled?")]
        public bool CanMove = false; // Disabled by default until mounted

        [Header("Camera Settings")]
        [Tooltip("Horse's camera follow target")]
        public Transform horseCameraTarget;

        [Tooltip("Horse's camera look-at target")]
        public Transform horseLookAtTarget;

        // Private variables
        private float _speed;
        private float _animationBlend;
        private float _targetRotation = 0.0f;
        private float _rotationVelocity;
        private float _verticalVelocity;
        private float _terminalVelocity = 53.0f;

        private int _animIDSpeed;
        private int _animIDGrounded;
        private int _animIDMotionSpeed;

#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
        private PlayerInput _playerInput;
#endif
        private Animator _animator;
        private CharacterController _controller;
        private StarterAssetsInputs _input;
        private GameObject _mainCamera;

        private const float _threshold = 0.01f;
        private bool _hasAnimator;

        private void Awake()
        {
            if (_mainCamera == null)
            {
                _mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
            }

            // Initialize components in Awake to avoid race condition with mounting
            _animator = GetComponentInChildren<Animator>();
            _hasAnimator = _animator != null;
            _controller = GetComponent<CharacterController>();
            // Don't get StarterAssetsInputs here - it will be set when mounting

#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
            _playerInput = GetComponent<PlayerInput>();
            if (_playerInput != null)
            {
                _playerInput.enabled = false; // Start disabled
            }
#endif

            AssignAnimationIDs();

            // Disable controller by default (will be enabled when mounted)
            if (_controller != null)
            {
                _controller.enabled = false;
            }
        }

        private void Start()
        {
        }

        private void Update()
        {
            // Don't run if controller is disabled
            if (_controller == null || !_controller.enabled)
                return;

            GroundedCheck();
            Move();
            ApplyGravity();
        }

        private void AssignAnimationIDs()
        {
            _animIDSpeed = Animator.StringToHash("Speed");
            _animIDGrounded = Animator.StringToHash("Grounded");
            _animIDMotionSpeed = Animator.StringToHash("MotionSpeed");
        }

        private void GroundedCheck()
        {
            Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset,
                transform.position.z);
            Grounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers,
                QueryTriggerInteraction.Ignore);

            if (_hasAnimator)
            {
                _animator.SetBool(_animIDGrounded, Grounded);
            }
        }

        private void Move()
        {
            if (!CanMove || _input == null)
            {
                if (_hasAnimator)
                {
                    _animator.SetFloat(_animIDSpeed, 0f);
                    _animator.SetFloat(_animIDMotionSpeed, 0f);
                }
                return;
            }

            // Determine target speed based on sprint input
            // Walk = normal, Trot = when moving, Gallop = when sprinting
            float targetSpeed = WalkSpeed;
            
            if (_input.move != Vector2.zero)
            {
                targetSpeed = _input.sprint ? GallopSpeed : TrotSpeed;
            }

            if (_input.move == Vector2.zero) targetSpeed = 0.0f;

            float currentHorizontalSpeed = new Vector3(_controller.velocity.x, 0.0f, _controller.velocity.z).magnitude;

            float speedOffset = 0.1f;
            float inputMagnitude = _input.analogMovement ? _input.move.magnitude : 1f;

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

            Vector3 inputDirection = new Vector3(_input.move.x, 0.0f, _input.move.y).normalized;

            if (_input.move != Vector2.zero)
            {
                _targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg +
                                  _mainCamera.transform.eulerAngles.y;
                float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRotation, ref _rotationVelocity,
                    RotationSmoothTime);

                transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
            }

            Vector3 targetDirection = Quaternion.Euler(0.0f, _targetRotation, 0.0f) * Vector3.forward;

            _controller.Move(targetDirection.normalized * (_speed * Time.deltaTime) +
                             new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);

            if (_hasAnimator)
            {
                _animator.SetFloat(_animIDSpeed, _animationBlend);
                _animator.SetFloat(_animIDMotionSpeed, inputMagnitude);
            }
        }

        private void ApplyGravity()
        {
            if (Grounded)
            {
                if (_verticalVelocity < 0.0f)
                {
                    _verticalVelocity = -2f;
                }
            }

            if (_verticalVelocity < _terminalVelocity)
            {
                _verticalVelocity += Gravity * Time.deltaTime;
            }
        }

        /// <summary>
        /// Enable horse control (called when player mounts)
        /// </summary>
        public void EnableControl(StarterAssetsInputs sharedInput)
        {
            // Use the shared input component from the player
            _input = sharedInput;

            // Enable controller first
            if (_controller != null)
            {
                _controller.enabled = true;
            }
            
#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
            if (_playerInput != null)
            {
                _playerInput.enabled = true;
            }
#endif

            // Then allow movement
            CanMove = true;
        }

        /// <summary>
        /// Disable horse control (called when player dismounts)
        /// </summary>
        public void DisableControl()
        {
            // Disable movement first
            CanMove = false;
            
            // Clear input reference
            _input = null;
            
            if (_controller != null)
            {
                _controller.enabled = false;
            }
            
#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
            if (_playerInput != null)
            {
                _playerInput.enabled = false;
            }
#endif
        }

        /// <summary>
        /// Get current horse speed for animation syncing
        /// </summary>
        public float GetCurrentSpeed()
        {
            return _speed;
        }

        private void OnDrawGizmosSelected()
        {
            Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
            Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

            if (Grounded) Gizmos.color = transparentGreen;
            else Gizmos.color = transparentRed;

            Gizmos.DrawSphere(
                new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z),
                GroundedRadius);
        }
    }
}
