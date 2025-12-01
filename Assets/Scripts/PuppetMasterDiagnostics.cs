using UnityEngine;
using RootMotion.Dynamics;
using System.Text;
using UnityEngine.InputSystem;

public class PuppetMasterDiagnostics : MonoBehaviour
{
    [Header("Drag your Puppet GameObject here")]
    public GameObject puppetObject;
    
    private StringBuilder log = new StringBuilder();
    private PuppetMaster pm;
    private BehaviourPuppet bp;
    private BehaviourFall bf;
    private Animator anim;

    void Start()
    {
        if (puppetObject == null)
        {
            Debug.LogError("=== PUPPET DIAGNOSTICS: No puppet assigned! Drag your puppet into the field ===");
            return;
        }

        log.AppendLine("====== PUPPETMASTER DIAGNOSTICS ======");
        log.AppendLine($"Time: {System.DateTime.Now}");
        log.AppendLine($"Puppet Object: {puppetObject.name}");
        log.AppendLine();

        // Get components
        pm = puppetObject.GetComponent<PuppetMaster>();
        bp = puppetObject.GetComponent<BehaviourPuppet>();
        bf = puppetObject.GetComponent<BehaviourFall>();
        anim = puppetObject.GetComponent<Animator>();

        CheckPuppetMaster();
        CheckBehaviourPuppet();
        CheckBehaviourFall();
        CheckAnimator();
        CheckPhysics();
        CheckLayers();
        CheckGroundDetection();
        CheckMuscles();
        CheckColliders();
        
        log.AppendLine("====== END DIAGNOSTICS ======");
        Debug.Log(log.ToString());
        
        // Also save to file
        System.IO.File.WriteAllText(Application.dataPath + "/PuppetDiagnostics.txt", log.ToString());
        Debug.Log("Diagnostics saved to: " + Application.dataPath + "/PuppetDiagnostics.txt");
    }

    void CheckPuppetMaster()
    {
        log.AppendLine("=== PUPPETMASTER COMPONENT ===");
        if (pm == null)
        {
            log.AppendLine("❌ CRITICAL: No PuppetMaster component found!");
            return;
        }

        log.AppendLine($"✓ PuppetMaster found");
        log.AppendLine($"  State: {pm.state}");
        log.AppendLine($"  Active: {pm.isAlive}");
        log.AppendLine($"  Mode: {pm.mode}");
        log.AppendLine($"  Muscle Weight: {pm.muscleWeight}");
        log.AppendLine($"  Pin Weight: {pm.pinWeight}");
        log.AppendLine($"  Mapping Weight: {pm.mappingWeight}");
        log.AppendLine($"  Muscle Spring: {pm.muscleSpring}");
        log.AppendLine($"  Muscle Damper: {pm.muscleDamper}");
        log.AppendLine($"  Update Mode: {pm.updateMode}");
        
        if (pm.state != PuppetMaster.State.Alive)
            log.AppendLine("⚠️ WARNING: Puppet is not in Alive state!");
        if (pm.muscleWeight < 0.9f)
            log.AppendLine("⚠️ WARNING: Muscle Weight is low - puppet may be weak");
        if (pm.muscleSpring < 100f)
            log.AppendLine("⚠️ WARNING: Muscle Spring is very low - puppet may collapse");
        
        log.AppendLine();
    }

    void CheckBehaviourPuppet()
    {
        log.AppendLine("=== BEHAVIOUR PUPPET ===");
        if (bp == null)
        {
            log.AppendLine("❌ No BehaviourPuppet component found!");
            log.AppendLine("   This might be why puppet isn't getting up");
            log.AppendLine();
            return;
        }

        log.AppendLine($"✓ BehaviourPuppet found");
        log.AppendLine($"  Enabled: {bp.enabled}");
        log.AppendLine($"  Collision Threshold: {bp.collisionThreshold}");
        log.AppendLine($"  Collision Resistance: {bp.collisionResistance}");
        log.AppendLine($"  Get Up Delay: {bp.getUpDelay}");
        log.AppendLine($"  Min Get Up Duration: {bp.minGetUpDuration}");
        
        // Check ground layers
        log.AppendLine($"  Ground Layers: {LayerMaskToString(bp.groundLayers)}");
        
        if (bp.collisionThreshold < 2f)
            log.AppendLine("⚠️ WARNING: Collision Threshold is low - puppet ragdolls easily");
        if (bp.groundLayers.value == 0)
            log.AppendLine("❌ CRITICAL: Ground Layers is set to Nothing!");
        
        log.AppendLine();
    }

    void CheckBehaviourFall()
    {
        log.AppendLine("=== BEHAVIOUR FALL ===");
        if (bf == null)
        {
            log.AppendLine("  No BehaviourFall component (this is optional)");
            log.AppendLine();
            return;
        }

        log.AppendLine($"✓ BehaviourFall found");
        log.AppendLine($"  Enabled: {bf.enabled}");
        log.AppendLine();
    }

    void CheckAnimator()
    {
        log.AppendLine("=== ANIMATOR ===");
        if (anim == null)
        {
            log.AppendLine("❌ No Animator component found!");
            log.AppendLine();
            return;
        }

        log.AppendLine($"✓ Animator found");
        log.AppendLine($"  Enabled: {anim.enabled}");
        log.AppendLine($"  Controller: {(anim.runtimeAnimatorController != null ? anim.runtimeAnimatorController.name : "None")}");
        log.AppendLine($"  Update Mode: {anim.updateMode}");
        log.AppendLine($"  Culling Mode: {anim.cullingMode}");
        
        if (anim.runtimeAnimatorController != null)
        {
            log.AppendLine("  Current State Info:");
            for (int i = 0; i < anim.layerCount; i++)
            {
                var stateInfo = anim.GetCurrentAnimatorStateInfo(i);
                log.AppendLine($"    Layer {i}: {GetStateName(anim, i)} (normalized time: {stateInfo.normalizedTime})");
            }
            
            // List parameters
            log.AppendLine("  Parameters:");
            foreach (var param in anim.parameters)
            {
                string value = "";
                switch (param.type)
                {
                    case AnimatorControllerParameterType.Bool:
                        value = anim.GetBool(param.name).ToString();
                        break;
                    case AnimatorControllerParameterType.Float:
                        value = anim.GetFloat(param.name).ToString("F2");
                        break;
                    case AnimatorControllerParameterType.Int:
                        value = anim.GetInteger(param.name).ToString();
                        break;
                    case AnimatorControllerParameterType.Trigger:
                        value = "(trigger)";
                        break;
                }
                log.AppendLine($"    {param.name} ({param.type}): {value}");
            }
        }
        
        log.AppendLine();
    }

    void CheckPhysics()
    {
        log.AppendLine("=== PHYSICS SETTINGS ===");
        log.AppendLine($"  Fixed Timestep: {Time.fixedDeltaTime}");
        log.AppendLine($"  Gravity: {Physics.gravity}");
        log.AppendLine($"  Default Solver Iterations: {Physics.defaultSolverIterations}");
        log.AppendLine($"  Default Solver Velocity Iterations: {Physics.defaultSolverVelocityIterations}");
        log.AppendLine();
    }

    void CheckLayers()
    {
        log.AppendLine("=== LAYER SETUP ===");
        log.AppendLine($"  Puppet Layer: {LayerMask.LayerToName(puppetObject.layer)} ({puppetObject.layer})");
        
        // Check ground
        RaycastHit hit;
        if (Physics.Raycast(puppetObject.transform.position, Vector3.down, out hit, 10f))
        {
            log.AppendLine($"  Ground Below: {hit.collider.gameObject.name}");
            log.AppendLine($"  Ground Layer: {LayerMask.LayerToName(hit.collider.gameObject.layer)} ({hit.collider.gameObject.layer})");
            log.AppendLine($"  Distance to ground: {hit.distance:F2}m");
            
            if (bp != null)
            {
                bool groundInMask = ((1 << hit.collider.gameObject.layer) & bp.groundLayers) != 0;
                if (groundInMask)
                    log.AppendLine("  ✓ Ground layer IS in BehaviourPuppet's ground layers");
                else
                    log.AppendLine("  ❌ CRITICAL: Ground layer is NOT in BehaviourPuppet's ground layers!");
            }
        }
        else
        {
            log.AppendLine("  ❌ CRITICAL: No ground detected below puppet!");
        }
        log.AppendLine();
    }

    void CheckGroundDetection()
    {
        log.AppendLine("=== GROUND DETECTION TEST ===");
        if (bp != null)
        {
            // Multiple raycasts to check ground
            Vector3[] testPoints = new Vector3[]
            {
                puppetObject.transform.position,
                puppetObject.transform.position + Vector3.forward * 0.5f,
                puppetObject.transform.position + Vector3.back * 0.5f,
                puppetObject.transform.position + Vector3.left * 0.5f,
                puppetObject.transform.position + Vector3.right * 0.5f
            };

            int groundHits = 0;
            foreach (var point in testPoints)
            {
                if (Physics.Raycast(point, Vector3.down, 2f, bp.groundLayers))
                    groundHits++;
            }
            
            log.AppendLine($"  Ground detection hits: {groundHits}/5 test points");
            if (groundHits == 0)
                log.AppendLine("  ❌ CRITICAL: Cannot detect ground with current layer mask!");
        }
        log.AppendLine();
    }

    void CheckMuscles()
    {
        log.AppendLine("=== MUSCLES ===");
        if (pm == null)
        {
            log.AppendLine("  Cannot check - no PuppetMaster");
            log.AppendLine();
            return;
        }

        log.AppendLine($"  Total Muscles: {pm.muscles.Length}");
        
        int weakMuscles = 0;
        int unpinnedMuscles = 0;
        
        foreach (var muscle in pm.muscles)
        {
            if (muscle.state.muscleWeightMlp < 0.5f)
                weakMuscles++;
            if (muscle.state.pinWeightMlp < 0.5f)
                unpinnedMuscles++;
        }
        
        if (weakMuscles > 0)
            log.AppendLine($"  ⚠️ {weakMuscles} muscles have low weight");
        if (unpinnedMuscles > 0)
            log.AppendLine($"  ⚠️ {unpinnedMuscles} muscles are unpinned");
        
        log.AppendLine();
    }

    void CheckColliders()
    {
        log.AppendLine("=== COLLIDERS ===");
        var colliders = puppetObject.GetComponentsInChildren<Collider>();
        log.AppendLine($"  Total Colliders: {colliders.Length}");
        
        int triggers = 0;
        foreach (var col in colliders)
        {
            if (col.isTrigger) triggers++;
        }
        
        log.AppendLine($"  Trigger Colliders: {triggers}");
        log.AppendLine($"  Physical Colliders: {colliders.Length - triggers}");
        
        if (colliders.Length == 0)
            log.AppendLine("  ❌ CRITICAL: No colliders found!");
        
        log.AppendLine();
    }

    string LayerMaskToString(LayerMask mask)
    {
        if (mask.value == 0) return "Nothing";
        if (mask.value == -1) return "Everything";
        
        System.Collections.Generic.List<string> layers = new System.Collections.Generic.List<string>();
        for (int i = 0; i < 32; i++)
        {
            if ((mask.value & (1 << i)) != 0)
            {
                string layerName = LayerMask.LayerToName(i);
                if (!string.IsNullOrEmpty(layerName))
                    layers.Add($"{layerName}({i})");
            }
        }
        return string.Join(", ", layers.ToArray());
    }

    string GetStateName(Animator anim, int layer)
    {
        var stateInfo = anim.GetCurrentAnimatorStateInfo(layer);
        var clips = anim.runtimeAnimatorController.animationClips;
        
        foreach (var clip in clips)
        {
            if (Animator.StringToHash(clip.name) == stateInfo.shortNameHash)
                return clip.name;
        }
        
        return $"Unknown (hash: {stateInfo.shortNameHash})";
    }

    void Update()
    {
        // Real-time monitoring
        if (pm != null && Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            Debug.Log($"[LIVE] State: {pm.state}, Muscle Weight: {pm.muscleWeight}, Pin Weight: {pm.pinWeight}");
            if (bp != null)
            {
                Debug.Log($"[LIVE] Behaviour Active: {bp.enabled}");
            }
        }
    }
}