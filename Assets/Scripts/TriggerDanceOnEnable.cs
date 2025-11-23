using UnityEngine;
using StarterAssets;

public class TriggerDanceOnEnable : MonoBehaviour
{
    [SerializeField] private ThirdPersonController thirdPersonController;

    private void OnEnable()
    {
        if (thirdPersonController != null)
        {
            thirdPersonController.TriggerDance();
        }
        else
        {
            Debug.LogWarning("ThirdPersonController reference not assigned on TriggerDanceOnEnable");
        }
    }
}
