#if PW_STORM_PRESENT
#endif
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace ProceduralWorlds.Storm.Wizard
{
    [CreateAssetMenu(fileName = "Storm Unsafe Code Wizard Option", menuName = "Procedural Worlds/Storm/Wizard/Storm Unsafe Code Wizard Option")]
    public class WizardOptionStormUnsafeCode : StormWizardOption
    {
        /// <summary>
        /// Called on startup
        /// </summary>
        private void OnEnable()
        {
            m_isPackageManagerRequired = false;
            m_packageResult = StormPackageResult.Unknown;
            m_RPBuiltIn = true;
            m_RPHDRP = true;
            m_RPURP = true;
            m_name = "Unsafe Code Enabled";
            m_infoTextOK = "Unsafe Code is enabled in the project.";
            m_infoTextIssue = "Unsafe Code is not enabled in the project - Storm will not compile without errors. Please allow unsafe code in the Player Settings under 'Other' settings.";
            m_status = StormSettingStatus.Error;
            m_link = "https://docs.unity3d.com/Manual/playersettings-windows.html";
            m_linkDisplayText = "Unity Manual: Player Settings";
            m_wizardCategory = "Dependencies";
            Initialize();
        }

        /// <summary>
        /// Function used to check if this option is successlly setup or not
        /// </summary>
        /// <returns></returns>
        public override bool PerformCheck()
        {
            bool successful = false;
#if UNITY_EDITOR
            successful = PlayerSettings.allowUnsafeCode;
#endif
            m_status = successful ? StormSettingStatus.OK : StormSettingStatus.Error;
            return successful;
        }
        /// <summary>
        /// Function that will apply the fix to this option
        /// </summary>
        /// <param name="autoFix"></param>
        /// <returns></returns>
        public override bool FixNow(bool autoFix = false)
        {
#if UNITY_EDITOR
            PlayerSettings.allowUnsafeCode = true;
#endif
            return true;
        }
    }
}