#if PW_STORM_PRESENT
using ProceduralWorlds.Storm.Core;
#endif
using UnityEngine;
#if UNITY_EDITOR
#endif

namespace ProceduralWorlds.Storm.Wizard
{
    [CreateAssetMenu(fileName = "Storm License Wizard Option", menuName = "Procedural Worlds/Storm/Wizard/Storm License Wizard Option")]
    public class WizardOptionStormLicense : StormWizardOption
    {
        /// <summary>
        /// Called on startup
        /// </summary>
        private void OnEnable()
        {
            m_isPackageManagerRequired = false;
            m_packageResult = StormPackageResult.Unknown;
            m_canFix = false;
            m_canIgnore = false;
            m_RPBuiltIn = true;
            m_RPHDRP = true;
            m_RPURP = true;
            m_name = "License Check";
            m_infoTextOK = "You have entered a valid Storm license key.";
            m_infoTextIssue = "You have not yet entered your Storm license key or there is an issue with you Storm license.";
            m_status = StormSettingStatus.Error;
            m_link = "";
            m_linkDisplayText = "";
            m_wizardCategory = "Storm License";
            Initialize();
        }

        /// <summary>
        /// Function used to check if this option is successlly setup or not
        /// </summary>
        /// <returns></returns>
        public override bool PerformCheck()
        {
#if UNITY_EDITOR && PW_STORM_PRESENT
            if (Licensing.IsLicenseValid())
            {
                m_status = StormSettingStatus.OK;
                return true;
            }
            else
            {
                m_infoTextIssue = "You have not yet entered your Storm license key or there is an issue with you Storm license.";
                if (!string.IsNullOrEmpty(Licensing.LastStatus))
                {
                    m_infoTextIssue += $"\nLast Status: {Licensing.LastStatus}";                
                }
                m_status = StormSettingStatus.Error;
                return false;
            }
#else
            m_status = StormSettingStatus.Error;
            return true;
#endif
        }
    }
}