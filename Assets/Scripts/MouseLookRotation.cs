using UnityEngine;
using UnityEngine.InputSystem;

public class MouseLookRotation : MonoBehaviour
{
    [Header("Rotation Settings")]
    [SerializeField] private float sensitivityX = 2f;
    [SerializeField] private float sensitivityY = 2f;
    
    [Header("Rotation Limits")]
    [SerializeField] private float minXRotation = -180f;
    [SerializeField] private float maxXRotation = 180f;
    [SerializeField] private float minYRotation = -90f;
    [SerializeField] private float maxYRotation = 90f;
    
    [Header("Damping")]
    [SerializeField] private float dampingX = 10f;
    [SerializeField] private float dampingY = 10f;
    [SerializeField] private float zeroCrossingRange = 20f;
    [SerializeField] private float zeroCrossingDampingMultiplier = 3f;
    
    private Vector2 mouseInput;
    private float currentRotationX;
    private float currentRotationY;
    private float targetRotationX;
    private float targetRotationY;

    private void OnEnable()
    {
        // Initialize rotation values to current rotation
        currentRotationY = transform.localEulerAngles.x;
        currentRotationX = transform.localEulerAngles.y;
        
        // Normalize the Y rotation to -180 to 180 range
        if (currentRotationY > 180f)
            currentRotationY -= 360f;
            
        targetRotationX = currentRotationX;
        targetRotationY = currentRotationY;
    }

    private void Update()
    {
        // Get mouse delta from new Input System
        mouseInput = Mouse.current != null ? Mouse.current.delta.ReadValue() : Vector2.zero;
        
        // Calculate target rotations
        targetRotationX += mouseInput.x * sensitivityX;
        // Clamp rotations
        targetRotationX = Mathf.Clamp(targetRotationX, minXRotation, maxXRotation);
        targetRotationY = Mathf.Clamp(targetRotationY, minYRotation, maxYRotation);
        
        // Calculate adaptive damping near zero crossing
        float distanceFromZeroX = Mathf.Abs(currentRotationX);
        float distanceFromZeroY = Mathf.Abs(currentRotationY);
        
        float dampingMultiplierX = 1f;
        float dampingMultiplierY = 1f;
        
        // Only apply extra damping if we're near zero AND crossing it
        if (distanceFromZeroX < zeroCrossingRange && Mathf.Sign(currentRotationX) != Mathf.Sign(targetRotationX))
        {
            // More damping closer to zero
            float normalizedDistance = distanceFromZeroX / zeroCrossingRange;
            dampingMultiplierX = Mathf.Lerp(zeroCrossingDampingMultiplier, 1f, normalizedDistance);
        }
        
        if (distanceFromZeroY < zeroCrossingRange && Mathf.Sign(currentRotationY) != Mathf.Sign(targetRotationY))
        {
            // More damping closer to zero
            float normalizedDistance = distanceFromZeroY / zeroCrossingRange;
            dampingMultiplierY = Mathf.Lerp(zeroCrossingDampingMultiplier, 1f, normalizedDistance);
        }
        
        // Apply damping with adaptive multiplier
        float adaptiveDampingX = dampingX * Time.deltaTime / dampingMultiplierX;
        float adaptiveDampingY = dampingY * Time.deltaTime / dampingMultiplierY;
        
        currentRotationX = Mathf.Lerp(currentRotationX, targetRotationX, adaptiveDampingX);
        currentRotationY = Mathf.Lerp(currentRotationY, targetRotationY, adaptiveDampingY);
        currentRotationX = Mathf.Lerp(currentRotationX, targetRotationX, dampingX * Time.deltaTime);
        currentRotationY = Mathf.Lerp(currentRotationY, targetRotationY, dampingY * Time.deltaTime);
        
        // Apply rotation to transform
        transform.localRotation = Quaternion.Euler(currentRotationY, currentRotationX, 0f);
    }
}
