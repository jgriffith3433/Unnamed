using UnityEngine;

/// Copyright 2024 Procedural Worlds Ltd
/// Written by Jonathan Williams as part of Storm

namespace ProceduralWorlds.Storm.Utils
{
    public class StormLicensingInfo : ScriptableObject
    {
        [SerializeField]
        private string m_data;
        public string Data
        {
            get => m_data;
            set {
                if (m_data != value)
                {
                    m_data = value;
                    StormUtils.SetAssetDirty(this);
                }
            }
        }


        private static StormLicensingInfo m_instance;
        public static StormLicensingInfo Instance
        {
            get {
                if (m_instance == null)
                {
                    m_instance = Resources.Load<StormLicensingInfo>("StormLicenseInfo");
                    if (m_instance == null)
                    {
                        m_instance = CreateInstance<StormLicensingInfo>();

                        StormUtils.SaveAsset(m_instance, StormUtils.DefaultSavePath + "Resources/StormLicenseInfo.asset");
                    }
                }
                return m_instance;
            }
        }
    }
}