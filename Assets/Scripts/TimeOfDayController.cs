using UnityEngine;
using Gaia;

/// <summary>
/// Controls the Gaia time of day system via a single float value.
/// This component can be animated with Unity Timeline to smoothly change the time of day.
/// </summary>
public class TimeOfDayController : MonoBehaviour
{
    [SerializeField, Range(0f, 24f), Tooltip("Time of day in hours (0-24). Can be animated with Timeline.")]
    private float timeOfDay = 12f;

    private float previousTimeOfDay;

    private void Start()
    {
        // Initialize previous time to current time
        previousTimeOfDay = timeOfDay;
        
        // Set initial time
        UpdateTimeOfDay();
    }

    private void Update()
    {
        // Check if the time has changed (useful for Timeline animations)
        if (!Mathf.Approximately(previousTimeOfDay, timeOfDay))
        {
            UpdateTimeOfDay();
            previousTimeOfDay = timeOfDay;
        }
    }

    /// <summary>
    /// Updates the Gaia time of day system based on the current timeOfDay value.
    /// </summary>
    private void UpdateTimeOfDay()
    {
        if (PWSkyStandalone.Instance == null)
        {
            Debug.LogWarning("TimeOfDayController: PWSkyStandalone instance not found in scene.");
            return;
        }

        // Clamp to valid range
        float clampedTime = Mathf.Clamp(timeOfDay, 0f, 24f);
        
        // Convert float hours to hours and minutes
        int hours = Mathf.FloorToInt(clampedTime);
        float minutes = (clampedTime - hours) * 60f;
        
        // Handle 24:00 case (should be 0:00)
        if (hours >= 24)
        {
            hours = 0;
            minutes = 0f;
        }

        // Update Gaia time of day
        PWSkyStandalone.Instance.m_gaiaTimeOfDay.m_todHour = hours;
        PWSkyStandalone.Instance.m_gaiaTimeOfDay.m_todMinutes = minutes;
        
        // Update the sky system
        PWSkyStandalone.Instance.UpdateGaiaTimeOfDay(false);
    }

    /// <summary>
    /// Sets the time of day programmatically (useful for scripts or Timeline tracks).
    /// </summary>
    /// <param name="time">Time in hours (0-24)</param>
    public void SetTimeOfDay(float time)
    {
        timeOfDay = Mathf.Clamp(time, 0f, 24f);
        UpdateTimeOfDay();
    }

    /// <summary>
    /// Gets the current time of day value.
    /// </summary>
    /// <returns>Time in hours (0-24)</returns>
    public float GetTimeOfDay()
    {
        return timeOfDay;
    }

#if UNITY_EDITOR
    [ContextMenu("Preview Time Of Day")]
    private void PreviewTimeOfDay()
    {
        UpdateTimeOfDay();
    }
#endif
}
