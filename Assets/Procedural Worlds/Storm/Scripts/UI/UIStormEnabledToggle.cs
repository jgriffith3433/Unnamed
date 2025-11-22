using ProceduralWorlds.Storm.Core;
using UnityEngine;
using UnityEngine.UI;

/// Copyright 2023 Procedural Worlds Ltd
/// Written by Jonathan Williams as part of Storm

namespace ProceduralWorlds.Storm.UI
{
    [AddComponentMenu("Procedural Worlds/Storm/UI/Storm Enabled Toggle")]
    public class UIStormEnabledToggle : MonoBehaviour
    {
        public Toggle m_toggle;
        public MonoBehaviour[] m_otherObjects;
        void Start()
        {
            StormWorld instance = StormWorld.Instance;
            if (m_toggle != null)
            {
                if (instance == null)
                {
                    m_toggle.interactable = false;
                    m_toggle.isOn = false;
                }
                else
                {
                    m_toggle.isOn = instance.isActiveAndEnabled;
                }
                m_toggle.onValueChanged.Invoke(m_toggle.isOn);
                m_toggle.onValueChanged.AddListener(OnToggleChanged);
            }

            if (StormWorld.Instance != null)
            {
                bool on = StormWorld.Instance.enabled;
                for (int o = 0; o < m_otherObjects.Length; ++o)
                    m_otherObjects[o].enabled = on;
            }
        }
        private void OnToggleChanged(bool on)
        {
            if (StormWorld.Instance != null)
            {
                StormWorld.Instance.enabled = on;
                StormWorld.Instance.gameObject.SetActive(on);
            }
            for (int o = 0; o < m_otherObjects.Length; ++o)
                m_otherObjects[o].enabled = on;
        }
    }
}