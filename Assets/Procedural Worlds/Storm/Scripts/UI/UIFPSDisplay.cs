using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// Copyright 2023 Procedural Worlds Ltd
/// Written by Jonathan Williams as part of Storm

namespace ProceduralWorlds.Storm
{
    [AddComponentMenu("Procedural Worlds/Storm/UI/FPS Display")]
    public class UIFPSDisplay : MonoBehaviour
    {
        public string Prefix = "FPS: ";
        public Text TextDisplay;
        public TextMeshProUGUI TMProDisplay;
        public int UpdateFrequency = 10;

        private float m_currentFPS;
        private int m_frameCounter;
        private float m_frameCountStart;

        public void OnEnable()
        {
            Application.targetFrameRate = 300;
            m_currentFPS = 0;
            m_frameCounter = 0;
            m_frameCountStart = Time.unscaledTime;
        }

        public void Update()
        {
            if (++m_frameCounter >= UpdateFrequency)
            {
                float now = Time.unscaledTime;
                if (m_frameCountStart > 0)
                {
                    m_currentFPS = (m_frameCounter / (now - m_frameCountStart));
                }
                m_frameCountStart = now;
                m_frameCounter = 0;
                if (TextDisplay != null)
                    TextDisplay.text = Prefix + m_currentFPS.ToString("N0");
                if (TMProDisplay != null)
                    TMProDisplay.text = Prefix + m_currentFPS.ToString("N0");
            }
        }
    }
}