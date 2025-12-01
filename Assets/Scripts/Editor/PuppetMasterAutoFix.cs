using UnityEngine;
using UnityEditor;
using RootMotion.Dynamics;

/// <summary>
/// Editor script to automatically fix PuppetMaster settings with one click.
/// Place this script in an "Editor" folder in your Assets.
/// Right-click on PuppetMaster component and select "Fix Puppet Settings"
/// </summary>
[CustomEditor(typeof(PuppetMaster))]
public class PuppetMasterAutoFix : Editor
{
    public override void OnInspectorGUI()
    {
        // Draw default inspector
        DrawDefaultInspector();
        
        PuppetMaster puppet = (PuppetMaster)target;
        
        GUILayout.Space(10);
        GUILayout.Label("Quick Fix Tools", EditorStyles.boldLabel);
        
        if (GUILayout.Button("🔧 Auto-Fix All Puppet Settings", GUILayout.Height(30)))
        {
            FixAllSettings(puppet);
        }
        
        GUILayout.Space(5);
        
        if (GUILayout.Button("Reset to Standing Position"))
        {
            ResetToStanding(puppet);
        }
    }

    [ContextMenu("Fix Puppet Settings")]
    public static void FixAllSettings(PuppetMaster puppet)
    {
        if (puppet == null)
        {
            Debug.LogError("No PuppetMaster found!");
            return;
        }

        Undo.RecordObject(puppet.gameObject, "Fix Puppet Settings");
        
        Debug.Log("====== FIXING PUPPET SETTINGS ======");
        
        // 1. Fix PuppetMaster settings
        FixPuppetMasterSettings(puppet);
        
        // 2. Add/Fix BehaviourPuppet
        FixBehaviourPuppet(puppet);
        
        // 3. Add/Fix Animator
        FixAnimator(puppet);
        
        // 4. Fix Rigidbody settings
        FixRigidbodies(puppet);
        
        // 5. Update Cinemachine if present
        FixCinemachine(puppet);
        
        EditorUtility.SetDirty(puppet.gameObject);
        
        Debug.Log("====== PUPPET FIXED! ======");
        Debug.Log("Settings applied:");
        Debug.Log("- Muscle Spring: 250");
        Debug.Log("- Muscle Damper: 15");
        Debug.Log("- Collision Threshold: 8");
        Debug.Log("- Ground Layers: Default");
        Debug.Log("- Animator Update: Fixed Update");
        Debug.Log("- All components configured!");
    }

    static void FixPuppetMasterSettings(PuppetMaster puppet)
    {
        Debug.Log("Fixing PuppetMaster component...");
        
        // Set optimal muscle settings
        puppet.muscleSpring = 250f;
        puppet.muscleDamper = 15f;
        puppet.pinWeight = 1f;
        puppet.muscleWeight = 1f;
        puppet.mappingWeight = 1f;
        
        // Ensure it's alive and active
        puppet.state = PuppetMaster.State.Alive;
        puppet.mode = PuppetMaster.Mode.Active;
        
        Debug.Log("✓ PuppetMaster settings updated");
    }

    static void FixBehaviourPuppet(PuppetMaster puppet)
    {
        Debug.Log("Checking BehaviourPuppet...");
        
        BehaviourPuppet bp = puppet.GetComponent<BehaviourPuppet>();
        
        if (bp == null)
        {
            Debug.Log("Adding BehaviourPuppet component...");
            bp = Undo.AddComponent<BehaviourPuppet>(puppet.gameObject);
        }
        
        Undo.RecordObject(bp, "Fix BehaviourPuppet");
        
        // Set optimal behaviour settings
        bp.collisionThreshold = 8f;
        bp.getUpDelay = 1f;
        bp.minGetUpDuration = 1.5f;
        
        // Set ground layers to Default + common ground layers
        bp.groundLayers = LayerMask.GetMask("Default") | LayerMask.GetMask("Ground");
        
        // If no layers set, use everything except ignored layers
        if (bp.groundLayers == 0)
        {
            bp.groundLayers = ~LayerMask.GetMask("Ignore Raycast", "UI");
        }
        
        bp.enabled = true;
        
        Debug.Log("✓ BehaviourPuppet configured");
    }

    static void FixAnimator(PuppetMaster puppet)
    {
        Debug.Log("Checking Animator...");
        
        Animator animator = puppet.GetComponent<Animator>();
        
        if (animator == null)
        {
            Debug.Log("Adding Animator component...");
            animator = Undo.AddComponent<Animator>(puppet.gameObject);
        }
        
        Undo.RecordObject(animator, "Fix Animator");
        
        // Find appropriate animator controller if none assigned
        if (animator.runtimeAnimatorController == null)
        {
            // Search for Humanoid or Puppet animator controllers
            string[] guids = AssetDatabase.FindAssets("Humanoid Puppet t:AnimatorController");
            
            if (guids.Length == 0)
            {
                guids = AssetDatabase.FindAssets("t:AnimatorController");
            }
            
            if (guids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                RuntimeAnimatorController controller = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(path);
                animator.runtimeAnimatorController = controller;
                Debug.Log($"✓ Assigned animator controller: {controller.name}");
            }
            else
            {
                Debug.LogWarning("⚠ No animator controller found - please assign one manually");
            }
        }
        
        // Set optimal animator settings
        animator.updateMode = AnimatorUpdateMode.Fixed;
        animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
        animator.enabled = true;
        
        Debug.Log("✓ Animator configured");
    }

    static void FixRigidbodies(PuppetMaster puppet)
    {
        Debug.Log("Checking Rigidbodies...");
        
        Rigidbody[] rigidbodies = puppet.GetComponentsInChildren<Rigidbody>();
        
        foreach (var rb in rigidbodies)
        {
            Undo.RecordObject(rb, "Fix Rigidbody");
            
            // Set optimal physics settings
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.maxAngularVelocity = 50f;
            
            // Ensure not kinematic
            rb.isKinematic = false;
        }
        
        Debug.Log($"✓ Fixed {rigidbodies.Length} Rigidbodies");
    }

    static void FixCinemachine(PuppetMaster puppet)
    {
        // Check if Cinemachine is in the project
        var cinemachineType = System.Type.GetType("Cinemachine.CinemachineVirtualCamera, Cinemachine");
        
        if (cinemachineType == null) return;
        
        Debug.Log("Checking Cinemachine cameras...");
        
        var cameras = Object.FindObjectsOfType(cinemachineType);
        
        foreach (var cam in cameras)
        {
            var updateMethodProp = cinemachineType.GetProperty("m_UpdateMethod");
            if (updateMethodProp != null)
            {
                // Set to FixedUpdate (value 1)
                updateMethodProp.SetValue(cam, 1);
                Debug.Log($"✓ Set {(cam as Component).gameObject.name} to Fixed Update");
            }
        }
    }

    static void ResetToStanding(PuppetMaster puppet)
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning("Reset to Standing only works in Play Mode!");
            return;
        }
        
        Debug.Log("Resetting puppet to standing position...");
        
        // Reset puppet state
        puppet.state = PuppetMaster.State.Alive;
        puppet.muscleWeight = 1f;
        puppet.pinWeight = 1f;
        
        // Reset velocities
        Rigidbody[] rigidbodies = puppet.GetComponentsInChildren<Rigidbody>();
        foreach (var rb in rigidbodies)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
        
        // Move puppet slightly up to ensure ground contact
        Transform root = puppet.transform;
        root.position += Vector3.up * 0.1f;
        
        Debug.Log("✓ Puppet reset!");
    }

    [MenuItem("GameObject/PuppetMaster/Auto-Fix Selected Puppet", false, 0)]
    static void FixSelectedPuppet()
    {
        GameObject selected = Selection.activeGameObject;
        
        if (selected == null)
        {
            Debug.LogError("No GameObject selected!");
            return;
        }
        
        PuppetMaster puppet = selected.GetComponent<PuppetMaster>();
        
        if (puppet == null)
        {
            Debug.LogError("Selected GameObject doesn't have PuppetMaster component!");
            return;
        }
        
        FixAllSettings(puppet);
    }
}

// Additional context menu on GameObject
[InitializeOnLoad]
public class PuppetMasterContextMenu
{
    [MenuItem("CONTEXT/PuppetMaster/🔧 Fix All Settings")]
    static void FixPuppetContext(MenuCommand command)
    {
        PuppetMaster puppet = command.context as PuppetMaster;
        if (puppet != null)
        {
            PuppetMasterAutoFix.FixAllSettings(puppet);
        }
    }
    
    [MenuItem("CONTEXT/PuppetMaster/Reset to Standing (Play Mode Only)")]
    static void ResetPuppetContext(MenuCommand command)
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning("This option only works in Play Mode!");
            return;
        }
        
        PuppetMaster puppet = command.context as PuppetMaster;
        if (puppet != null && Application.isPlaying)
        {
            // Reset implementation
            puppet.state = PuppetMaster.State.Alive;
            puppet.muscleWeight = 1f;
            puppet.pinWeight = 1f;
        }
    }
}