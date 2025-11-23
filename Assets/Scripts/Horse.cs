using UnityEngine;
using StarterAssets;

/// <summary>
/// Horse component that manages mount point and references to the horse controller
/// </summary>
[RequireComponent(typeof(HorseController))]
public class Horse : MonoBehaviour
{
    [Header("Mount Settings")]
    [Tooltip("The transform where the rider will be positioned (usually above the saddle)")]
    public Transform mountPoint;

    private HorseController _horseController;

    private void Awake()
    {
        _horseController = GetComponent<HorseController>();

        // Create mount point if not assigned
        if (mountPoint == null)
        {
            GameObject mountPointObj = new GameObject("MountPoint");
            mountPointObj.transform.SetParent(transform);
            mountPointObj.transform.localPosition = new Vector3(0f, 1.5f, 0f); // Adjust as needed
            mountPointObj.transform.localRotation = Quaternion.identity;
            mountPoint = mountPointObj.transform;
            Debug.LogWarning($"Horse '{name}' had no mount point assigned. Created one at default position.");
        }
    }

    /// <summary>
    /// Get the horse controller component
    /// </summary>
    public HorseController GetController()
    {
        return _horseController;
    }

    /// <summary>
    /// Get current horse speed
    /// </summary>
    public float GetCurrentSpeed()
    {
        return _horseController != null ? _horseController.GetCurrentSpeed() : 0f;
    }

    private void OnDrawGizmosSelected()
    {
        // Visualize mount point
        if (mountPoint != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(mountPoint.position, 0.2f);
            Gizmos.DrawLine(mountPoint.position, mountPoint.position + mountPoint.forward * 0.5f);
        }
    }
}
