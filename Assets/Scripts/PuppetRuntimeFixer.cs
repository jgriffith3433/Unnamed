using UnityEngine;
using RootMotion.Dynamics;
using UnityEngine.InputSystem;

/// <summary>
/// Forces correct PuppetMaster settings at runtime.
/// Attach this to the same GameObject as PuppetMaster.
/// This will override any incorrect settings when Play starts.
/// </summary>
[RequireComponent(typeof(PuppetMaster))]
public class PuppetRuntimeFixer : MonoBehaviour
{
    [Header("Muscle Strength Settings")]
    [Tooltip("Spring force - how strongly muscles pull toward target pose")]
    public float muscleSpring = 300f;
    
    [Tooltip("Damping - reduces oscillation and wobble")]
    public float muscleDamper = 20f;

    [Header("Balance Settings")]
    [Tooltip("How hard to hit before ragdolling")]
    public float collisionThreshold = 8f;
    
    [Tooltip("Delay before attempting to get up after falling")]
    public float getUpDelay = 1f;

    [Header("Debug")]
    public bool showDebugInfo = true;
    public bool forceSettingsEveryFrame = true;

    private PuppetMaster puppetMaster;
    private BehaviourPuppet behaviourPuppet;
    private Animator animator;

    void Awake()
    {
        puppetMaster = GetComponent<PuppetMaster>();
        behaviourPuppet = GetComponent<BehaviourPuppet>();
        animator = GetComponent<Animator>();

        // Force settings immediately on awake
        ApplySettings();
        
        Debug.Log("=== PUPPET RUNTIME FIXER APPLIED ===");
    }

    void Start()
    {
        // Apply again on Start to ensure they stick
        ApplySettings();
        
        if (showDebugInfo)
        {
            LogCurrentSettings();
        }
    }

    void Update()
    {
        // Continuously force settings if enabled
        if (forceSettingsEveryFrame)
        {
            ApplySettings();
        }

        // Debug key to manually reapply
        if (Keyboard.current != null && Keyboard.current.fKey.wasPressedThisFrame)
        {
            Debug.Log("Manual fix applied!");
            ApplySettings();
            LogCurrentSettings();
        }
    }

    void ApplySettings()
    {
        if (puppetMaster == null) return;

        // Force muscle strength
        puppetMaster.muscleSpring = muscleSpring;
        puppetMaster.muscleDamper = muscleDamper;
        puppetMaster.muscleWeight = 1f;
        puppetMaster.pinWeight = 1f;
        puppetMaster.mappingWeight = 1f;

        // Ensure state is correct
        if (puppetMaster.state != PuppetMaster.State.Alive)
        {
            puppetMaster.state = PuppetMaster.State.Alive;
        }

        // Fix BehaviourPuppet if present
        if (behaviourPuppet != null)
        {
            behaviourPuppet.collisionThreshold = collisionThreshold;
            behaviourPuppet.getUpDelay = getUpDelay;
            
            // CRITICAL: Ensure it's enabled
            if (!behaviourPuppet.enabled)
            {
                behaviourPuppet.enabled = true;
                Debug.LogWarning("BehaviourPuppet was disabled - re-enabled it!");
            }
        }

        // Fix Animator if present
        if (animator != null && !animator.enabled)
        {
            animator.enabled = true;
            Debug.LogWarning("Animator was disabled - re-enabled it!");
        }
    }

    void LogCurrentSettings()
    {
        Debug.Log($"Current Settings:");
        Debug.Log($"  Muscle Spring: {puppetMaster.muscleSpring}");
        Debug.Log($"  Muscle Damper: {puppetMaster.muscleDamper}");
        Debug.Log($"  Muscle Weight: {puppetMaster.muscleWeight}");
        Debug.Log($"  Pin Weight: {puppetMaster.pinWeight}");
        Debug.Log($"  State: {puppetMaster.state}");
        
        if (behaviourPuppet != null)
        {
            Debug.Log($"  BehaviourPuppet Enabled: {behaviourPuppet.enabled}");
            Debug.Log($"  Collision Threshold: {behaviourPuppet.collisionThreshold}");
        }
        
        if (animator != null)
        {
            Debug.Log($"  Animator Enabled: {animator.enabled}");
            Debug.Log($"  Animator State: {GetCurrentAnimatorState()}");
        }
    }

    string GetCurrentAnimatorState()
    {
        if (animator == null || animator.runtimeAnimatorController == null) 
            return "No Controller";
        
        var stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        return $"Hash: {stateInfo.shortNameHash}";
    }

    // Gizmo to show when this is active
    void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;
        
        // Draw a green sphere above the puppet to show fixer is active
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position + Vector3.up * 2f, 0.2f);
    }
}