using UnityEngine;
#if GAIA_2023_PRO
using Gaia;
#endif

public class StartThunder : MonoBehaviour
{
    [SerializeField] private bool startOnEnable = true;
#if GAIA_2023_PRO
    [SerializeField] private ThunderStrike thunderStrike;
#endif

    private void OnEnable()
    {
        if (startOnEnable)
        {
            StartThunderStorm();
        }
    }

    public void StartThunderStorm()
    {
#if GAIA_2023_PRO
        if (thunderStrike != null)
        {
            thunderStrike.PlayThunderSystem();
            Debug.Log("Started thunder weather effect");
        }
        else
        {
            Debug.LogWarning("ThunderStrike component not assigned");
        }
#else
        Debug.LogWarning("GAIA_2023_PRO is not defined. Weather features are not available.");
#endif
    }

    public void StopThunderStorm()
    {
#if GAIA_2023_PRO
        if (thunderStrike != null)
        {
            thunderStrike.m_playThunder = false;
            Debug.Log("Stopped thunder weather effect");
        }
#endif
    }
}
