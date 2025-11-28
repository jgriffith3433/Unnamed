using UnityEngine;
using Unity.Cinemachine;
using ProceduralWorlds.Storm.Core;

/// <summary>
/// Forces Storm to update immediately when Cinemachine camera switches occur.
/// This prevents vegetation pop-in during camera cuts in Timeline or live gameplay.
/// </summary>
public class StormCinemachineForceUpdate : MonoBehaviour
{
    [Header("Cinemachine")]
    [Tooltip("The Cinemachine Brain to monitor for camera blends")]
    [SerializeField] private CinemachineBrain cinemachineBrain;

    [Header("Storm")]
    [Tooltip("The StormWorld instance to force update. Leave empty to find automatically.")]
    [SerializeField] private StormWorld stormWorld;

    [Header("Settings")]
    [Tooltip("Force update when a new camera becomes active (recommended for Timeline cuts)")]
    [SerializeField] private bool updateOnCameraChange = true;

    [Tooltip("Force update during camera blends (may cause small hitches but ensures smooth transitions)")]
    [SerializeField] private bool updateDuringBlends = false;

    [Tooltip("Number of frames to wait after a camera cut before forcing update (0 = immediate)")]
    [SerializeField] private int framesToWaitAfterCut = 0;

    private CinemachineVirtualCameraBase lastActiveCamera;
    private int frameCounter = 0;
    private bool pendingUpdate = false;

    private void Awake()
    {
        // Find CinemachineBrain if not assigned
        if (cinemachineBrain == null)
        {
            cinemachineBrain = FindFirstObjectByType<CinemachineBrain>();
            if (cinemachineBrain == null)
            {
                Debug.LogError("[StormCinemachineForceUpdate] No CinemachineBrain found in scene!");
                enabled = false;
                return;
            }
        }

        // Find StormWorld if not assigned
        if (stormWorld == null)
        {
            stormWorld = StormWorld.Instance;
            if (stormWorld == null)
            {
                Debug.LogWarning("[StormCinemachineForceUpdate] No StormWorld found in scene. Storm updates will be skipped.");
            }
        }
    }

    private void OnEnable()
    {
        if (cinemachineBrain != null)
        {
            // Subscribe to Cinemachine camera activation events
            CinemachineCore.CameraActivatedEvent.AddListener(OnCameraActivated);
        }
    }

    private void OnDisable()
    {
        if (cinemachineBrain != null)
        {
            CinemachineCore.CameraActivatedEvent.RemoveListener(OnCameraActivated);
        }
    }

    private void OnCameraActivated(ICinemachineCamera.ActivationEventParams evt)
    {
        if (!updateOnCameraChange || stormWorld == null)
            return;

        // Check if this is actually a new camera (not just a re-activation)
        if (evt.IncomingCamera != null && evt.IncomingCamera != lastActiveCamera)
        {
            lastActiveCamera = evt.IncomingCamera as CinemachineVirtualCameraBase;

            if (framesToWaitAfterCut > 0)
            {
                // Schedule delayed update
                pendingUpdate = true;
                frameCounter = 0;
            }
            else
            {
                // Immediate update
                ForceStormUpdate();
            }
        }
    }

    private void LateUpdate()
    {
        // Handle delayed updates
        if (pendingUpdate)
        {
            frameCounter++;
            if (frameCounter >= framesToWaitAfterCut)
            {
                ForceStormUpdate();
                pendingUpdate = false;
            }
        }

        // Handle updates during blends
        if (updateDuringBlends && cinemachineBrain != null && cinemachineBrain.IsBlending)
        {
            ForceStormUpdate();
        }
    }

    /// <summary>
    /// Forces Storm to update immediately, triggering culling and loading
    /// </summary>
    private void ForceStormUpdate()
    {
        if (stormWorld == null)
            return;

        try
        {
            // Force Storm to update its culling and loading systems multiple times
            // This helps ensure jobs complete before the frame is rendered
            stormWorld.Update();
            
            // Force a second update to ensure any scheduled jobs are processed
            // The first update schedules jobs, the second processes their results
            //stormWorld.Update();
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"[StormCinemachineForceUpdate] Failed to force Storm update: {e.Message}");
        }
    }

    /// <summary>
    /// Manually trigger a Storm update (can be called from Timeline signals or other scripts)
    /// </summary>
    public void ManualForceUpdate()
    {
        ForceStormUpdate();
    }
}
