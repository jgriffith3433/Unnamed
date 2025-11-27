using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Resets the GameObject's transform to its original position when exiting play mode.
/// Useful when Timeline or other animation systems leave objects in animated states.
/// </summary>
public class ResetTransformOnPlayModeExit : MonoBehaviour
{
#if UNITY_EDITOR
    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private Vector3 originalScale;
    
    private void Awake()
    {
        // Store original transform values when entering play mode
        originalPosition = transform.localPosition;
        originalRotation = transform.localRotation;
        originalScale = transform.localScale;
        
        // Subscribe to play mode state changes
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }
    
    private void OnDestroy()
    {
        // Unsubscribe when destroyed
        EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
    }
    
    private void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        // When exiting play mode, reset the transform
        if (state == PlayModeStateChange.ExitingPlayMode)
        {
            transform.localPosition = originalPosition;
            transform.localRotation = originalRotation;
            transform.localScale = originalScale;
            
            Debug.Log($"Reset transform for {gameObject.name}");
        }
    }
#endif
}
