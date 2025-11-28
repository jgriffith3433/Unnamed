using UnityEngine;
#if GAIA_2023_PRO
using Gaia;
#endif

/// <summary>
/// Controls the Gaia weather system with simple parameters that can be animated with Unity Timeline.
/// Allows switching between Clear, Rain, and Snow weather states with intensity controls.
/// </summary>
public class WeatherController : MonoBehaviour
{
    public enum WeatherType
    {
        Clear = 0,
        Rain = 1,
        Snow = 2
    }

    [Header("Weather State")]
    [SerializeField, Tooltip("Current weather type: Clear, Rain, or Snow. Can be animated with Timeline.")]
    private WeatherType weatherType = WeatherType.Clear;

    [Header("Rain Settings")]
    [SerializeField, Range(0f, 1f), Tooltip("Rain intensity (0 = no rain, 1 = heavy rain)")]
    private float rainIntensity = 0.5f;

    [SerializeField, Tooltip("Enable rain audio (ambient rain sounds)")]
    private bool enableRainAudio = true;

    [Header("Snow Settings")]
    [SerializeField, Range(0f, 1f), Tooltip("Snow intensity (0 = no snow, 1 = heavy snow)")]
    private float snowIntensity = 0.5f;

    [SerializeField, Tooltip("Enable snow audio (wind and snow sounds)")]
    private bool enableSnowAudio = true;

    private WeatherType previousWeatherType;
    private float previousRainIntensity;
    private float previousSnowIntensity;

    private void Start()
    {
        // Initialize previous values
        previousWeatherType = weatherType;
        previousRainIntensity = rainIntensity;
        previousSnowIntensity = snowIntensity;

        // Apply initial weather
        UpdateWeather();
    }

    private void Update()
    {
#if GAIA_2023_PRO
        var weather = ProceduralWorldsGlobalWeather.Instance;
        if (weather == null) return;
        
        // Check if weather type has changed
        bool weatherTypeChanged = previousWeatherType != weatherType;
        bool rainIntensityChanged = !Mathf.Approximately(previousRainIntensity, rainIntensity);
        bool snowIntensityChanged = !Mathf.Approximately(previousSnowIntensity, snowIntensity);

        if (weatherTypeChanged)
        {
            UpdateWeather();
            previousWeatherType = weatherType;
            previousRainIntensity = rainIntensity;
            previousSnowIntensity = snowIntensity;
        }
        
        // IMPORTANT: Only override intensity if the weather is ACTUALLY active
        // Don't interfere with Gaia's fade-out transitions
        if (weatherType == WeatherType.Rain && weather.IsRaining)
        {
            UpdateRainIntensity();
            if (rainIntensityChanged)
            {
                previousRainIntensity = rainIntensity;
            }
        }
        else if (weatherType == WeatherType.Snow && weather.IsSnowing)
        {
            UpdateSnowIntensity();
            if (snowIntensityChanged)
            {
                previousSnowIntensity = snowIntensity;
            }
        }
#endif
    }

    /// <summary>
    /// Updates the weather based on the current weather type.
    /// </summary>
    private void UpdateWeather()
    {
#if GAIA_2023_PRO
        if (ProceduralWorldsGlobalWeather.Instance == null)
        {
            Debug.LogWarning("WeatherController: ProceduralWorldsGlobalWeather instance not found in scene.");
            return;
        }

        var weather = ProceduralWorldsGlobalWeather.Instance;

        switch (weatherType)
        {
            case WeatherType.Clear:
                // Enable systems before stopping (required by Gaia)
                weather.EnableRain = true;
                weather.EnableSnow = true;
                
                // Stop both rain and snow
                if (weather.IsRaining)
                {
                    weather.StopRain();
                }
                if (weather.IsSnowing)
                {
                    weather.StopSnow();
                    // Immediately stop snow audio to prevent it from continuing
                    StopSnowAudio(weather);
                }
                break;

            case WeatherType.Rain:
                // Enable systems before making changes
                weather.EnableRain = true;
                weather.EnableSnow = true;
                
                // Stop snow if active
                if (weather.IsSnowing)
                {
                    weather.StopSnow();
                    // Immediately stop snow audio to prevent it from continuing
                    StopSnowAudio(weather);
                }
                
                // Set intensity BEFORE starting rain
                weather.RainIntensity = rainIntensity;
                
                // Start rain if not already raining
                if (!weather.IsRaining)
                {
                    weather.PlayRain();
                }
                break;

            case WeatherType.Snow:
                // Enable systems before making changes
                weather.EnableRain = true;
                weather.EnableSnow = true;
                
                // Stop rain if active
                if (weather.IsRaining)
                {
                    weather.StopRain();
                    // Immediately stop rain audio to prevent it from continuing
                    StopRainAudio(weather);
                }
                
                // Set intensity BEFORE starting snow
                weather.SnowIntensity = snowIntensity;
                
                // Start snow if not already snowing
                if (!weather.IsSnowing)
                {
                    weather.PlaySnow();
                }
                break;
        }
#else
        Debug.LogWarning("WeatherController: GAIA_2023_PRO is not defined. Weather features are not available.");
#endif
    }

    /// <summary>
    /// Updates only the rain intensity without changing the weather state.
    /// Gaia's weather system constantly lerps intensity to 1.0, so we must override every frame.
    /// </summary>
    private void UpdateRainIntensity()
    {
#if GAIA_2023_PRO
        var weather = ProceduralWorldsGlobalWeather.Instance;
        if (weather != null && weather.IsRaining)
        {
            // Force override the intensity - Gaia reads this value and applies it to shaders
            // We must set it every frame because Gaia lerps it to 1.0
            weather.RainIntensity = rainIntensity;
            
            // Control rain audio based on checkbox
            if (weather.m_rainParticles != null)
            {
                AudioSource rainAudio = weather.m_rainParticles.GetComponent<AudioSource>();
                if (rainAudio != null)
                {
                    if (enableRainAudio)
                    {
                        if (!rainAudio.isPlaying)
                        {
                            rainAudio.Play();
                        }
                    }
                    else
                    {
                        if (rainAudio.isPlaying)
                        {
                            rainAudio.Stop();
                        }
                        rainAudio.volume = 0f;
                    }
                }
            }
            
            // Also scale the rain particle material alpha to control visual density
            // Gaia uses a mesh renderer for rain particles
            if (weather.m_rainParticles != null && weather.m_rainParticles.sharedMaterial != null)
            {
                // Scale the particle alpha based on our intensity (0.95 is Gaia's max)
                weather.m_rainParticles.sharedMaterial.SetFloat("_PW_VFX_Weather_Intensity", 0.95f * rainIntensity);
            }
        }
#endif
    }

    /// <summary>
    /// Updates only the snow intensity without changing the weather state.
    /// Gaia's weather system constantly lerps intensity to 1.0, so we must override every frame.
    /// </summary>
    private void UpdateSnowIntensity()
    {
#if GAIA_2023_PRO
        var weather = ProceduralWorldsGlobalWeather.Instance;
        if (weather != null && weather.IsSnowing)
        {
            // Force override the intensity - Gaia reads this value and applies it to shaders
            // We must set it every frame because Gaia lerps it to 1.0
            weather.SnowIntensity = snowIntensity;
            
            // Control snow audio based on checkbox
            if (weather.m_snowParticles != null)
            {
                AudioSource snowAudio = weather.m_snowParticles.GetComponent<AudioSource>();
                if (snowAudio != null)
                {
                    if (enableSnowAudio)
                    {
                        if (!snowAudio.isPlaying)
                        {
                            snowAudio.Play();
                        }
                    }
                    else
                    {
                        if (snowAudio.isPlaying)
                        {
                            snowAudio.Stop();
                        }
                        snowAudio.volume = 0f;
                    }
                }
            }
            
            // Also scale the snow particle emission rates to control visual density
            if (weather.m_snowParticles != null)
            {
                var emission = weather.m_snowParticles.emission;
                var rateOverTime = emission.rateOverTime;
                rateOverTime.constant = 512f * snowIntensity; // 512 is Gaia's max
                emission.rateOverTime = rateOverTime;
            }
            
            if (weather.m_snowVFX != null)
            {
                var emission = weather.m_snowVFX.emission;
                var rateOverTime = emission.rateOverTime;
                rateOverTime.constant = 256f * snowIntensity; // 256 is Gaia's max for VFX
                emission.rateOverTime = rateOverTime;
            }
        }
#endif
    }

    /// <summary>
    /// Immediately stops snow audio source to prevent it from continuing after stopping snow.
    /// </summary>
    private void StopSnowAudio(ProceduralWorldsGlobalWeather weather)
    {
        if (weather.m_snowParticles != null)
        {
            AudioSource snowAudio = weather.m_snowParticles.GetComponent<AudioSource>();
            if (snowAudio != null && snowAudio.isPlaying)
            {
                snowAudio.Stop();
                snowAudio.volume = 0f;
            }
        }
    }

    /// <summary>
    /// Immediately stops rain audio source to prevent it from continuing after stopping rain.
    /// </summary>
    private void StopRainAudio(ProceduralWorldsGlobalWeather weather)
    {
        if (weather.m_rainParticles != null)
        {
            AudioSource rainAudio = weather.m_rainParticles.GetComponent<AudioSource>();
            if (rainAudio != null && rainAudio.isPlaying)
            {
                rainAudio.Stop();
                rainAudio.volume = 0f;
            }
        }
    }

    /// <summary>
    /// Sets the weather type programmatically.
    /// </summary>
    /// <param name="type">Weather type to set</param>
    public void SetWeatherType(WeatherType type)
    {
        weatherType = type;
        UpdateWeather();
    }

    /// <summary>
    /// Sets the weather type by integer (0=Clear, 1=Rain, 2=Snow).
    /// Useful for Timeline Signal tracks or UnityEvents.
    /// </summary>
    /// <param name="typeIndex">Weather type index</param>
    public void SetWeatherTypeByIndex(int typeIndex)
    {
        weatherType = (WeatherType)Mathf.Clamp(typeIndex, 0, 2);
        UpdateWeather();
    }

    /// <summary>
    /// Sets the rain intensity programmatically.
    /// </summary>
    /// <param name="intensity">Rain intensity (0-1)</param>
    public void SetRainIntensity(float intensity)
    {
        rainIntensity = Mathf.Clamp01(intensity);
        if (weatherType == WeatherType.Rain)
        {
            UpdateRainIntensity();
        }
    }

    /// <summary>
    /// Sets the snow intensity programmatically.
    /// </summary>
    /// <param name="intensity">Snow intensity (0-1)</param>
    public void SetSnowIntensity(float intensity)
    {
        snowIntensity = Mathf.Clamp01(intensity);
        if (weatherType == WeatherType.Snow)
        {
            UpdateSnowIntensity();
        }
    }

    /// <summary>
    /// Gets the current weather type.
    /// </summary>
    /// <returns>Current weather type</returns>
    public WeatherType GetWeatherType()
    {
        return weatherType;
    }

#if UNITY_EDITOR
    // This allows changes in the Inspector to update immediately in edit mode
    private void OnValidate()
    {
        if (Application.isPlaying)
        {
            UpdateWeather();
        }
    }
#endif
}
