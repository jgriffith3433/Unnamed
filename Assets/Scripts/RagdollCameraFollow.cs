using UnityEngine;

public class RagdollCameraFollow : MonoBehaviour
{
    public Transform ragdollRoot;
    public Transform cameraTarget;
    public float smoothSpeed = 10f;

    void LateUpdate()
    {
        // Lerp to ragdoll root position to smooth
        cameraTarget.position = Vector3.Lerp(cameraTarget.position, ragdollRoot.position, smoothSpeed * Time.deltaTime);
    }
}
