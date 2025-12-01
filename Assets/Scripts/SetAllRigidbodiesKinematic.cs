using UnityEngine;

public class SetAllRigidbodiesKinematic : MonoBehaviour
{
    private void OnEnable()
    {
        // Find all Rigidbody components in the scene
        Rigidbody[] allRigidbodies = FindObjectsOfType<Rigidbody>();
        
        // Set each one to kinematic
        foreach (Rigidbody rb in allRigidbodies)
        {
            rb.isKinematic = true;
        }
        
        Debug.Log($"Set {allRigidbodies.Length} Rigidbody components to kinematic.");
    }
}
