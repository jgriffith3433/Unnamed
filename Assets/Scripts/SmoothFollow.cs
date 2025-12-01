using UnityEngine;

public class SmoothFollow : MonoBehaviour
{
    [Header("Follow Settings")]
    [SerializeField] private Transform target;
    [SerializeField] private float followSpeed = 5f;
    
    [Header("Offset")]
    [SerializeField] private Vector3 offset = Vector3.zero;
    
    private void LateUpdate()
    {
        if (target == null)
            return;
        
        // Calculate target position with offset
        Vector3 targetPosition = target.position + offset;
        
        // Smoothly move towards target position
        transform.position = Vector3.Lerp(transform.position, targetPosition, followSpeed * Time.deltaTime);
    }
}
