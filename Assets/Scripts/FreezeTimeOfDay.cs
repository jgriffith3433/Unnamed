using UnityEngine;
#if GAIA_2023_PRO
using Gaia;
#endif

public class FreezeTimeOfDay : MonoBehaviour
{
    private void OnEnable()
    {
#if GAIA_2023_PRO
        if (PWSkyStandalone.Instance != null)
        {
            PWSkyStandalone.Instance.m_gaiaTimeOfDay.m_todDayTimeScale = 0f;
            PWSkyStandalone.Instance.UpdateGaiaTimeOfDay(false);
            Debug.Log("Time of Day time scale set to 0 (frozen)");
        }
        else
        {
            Debug.LogWarning("PWSkyStandalone instance not found");
        }
#else
        Debug.LogWarning("GAIA_2023_PRO is not defined. Time of Day features are not available.");
#endif
    }
}
