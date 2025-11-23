using UnityEngine;
#if GAIA_2023_PRO
using Gaia;
#endif

public class StartRain : MonoBehaviour
{
    [SerializeField] private bool startOnEnable = true;
    [SerializeField] [Range(0f, 1f)] private float rainIntensity = 0.3f;

    private void OnEnable()
    {
        if (startOnEnable)
        {
            StartRaining();
        }
    }

    public void StartRaining()
    {
#if GAIA_2023_PRO
        if (ProceduralWorldsGlobalWeather.Instance != null)
        {
            if (!ProceduralWorldsGlobalWeather.Instance.IsRaining)
            {
                ProceduralWorldsGlobalWeather.Instance.RainIntensity = rainIntensity;
                ProceduralWorldsGlobalWeather.Instance.PlayRain();
                Debug.Log($"Started light rain weather effect (intensity: {rainIntensity})");
            }
            else
            {
                Debug.Log("Rain is already active");
            }
        }
        else
        {
            Debug.LogWarning("ProceduralWorldsGlobalWeather instance not found in scene");
        }
#else
        Debug.LogWarning("GAIA_2023_PRO is not defined. Weather features are not available.");
#endif
    }

    public void StopRaining()
    {
#if GAIA_2023_PRO
        if (ProceduralWorldsGlobalWeather.Instance != null)
        {
            if (ProceduralWorldsGlobalWeather.Instance.IsRaining)
            {
                ProceduralWorldsGlobalWeather.Instance.StopRain();
                Debug.Log("Stopped rain weather effect");
            }
        }
#endif
    }
}
