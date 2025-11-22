using UnityEngine;

namespace ProceduralWorlds.Storm.Wizard
{
    [CreateAssetMenu(fileName = "Storm Installed Wizard Option", menuName = "Procedural Worlds/Storm/Wizard/Storm Installed Wizard Option")]
    public class WizardOptionStormInstalled : StormWizardOption
    {
        public override bool m_autoCreateIfMissing { get; } = false;
        /// <summary>
        /// Called on startup
        /// </summary>
        private void OnEnable()
        {
            m_isPackageManagerRequired = false;
            m_packageResult = StormPackageResult.Unknown;
            m_RPBuiltIn = false;
            m_RPHDRP = false;
            m_RPURP = false;
            m_name = "Storm Core Installation";
            m_infoTextOK = "The core storm installation was found in this project.";
            m_infoTextIssue = "Storm is not installed or there is a newer installation package - please press the 'Install' button to perform the core storm installation / update.";
            m_status = StormSettingStatus.Error;
            m_wizardCategory = "Storm Installation";
            m_installationOption = true;
            Initialize();
        }

        /// <summary>
        /// Function used to check if this option is successlly setup or not
        /// </summary>
        /// <returns></returns>
        public override bool PerformCheck()
        {
            bool successful = StormWizardUtils.StormCoreInstalled();
            m_wasFixed = successful;
            m_status = successful ? StormSettingStatus.OK : StormSettingStatus.Error;

            if (StormWizardUtils.StormCoreInstalled())
            {
                if (StormWizardUtils.StormCorePackageExists())
                {
                    m_name = "Storm Core Update";
                    m_status = StormSettingStatus.Warning;
                    m_infoTextIssue = "An newer version of Storm was found in this project. Please click the 'Install' button to unpack and apply the contents of the new version.";
                    successful = false;
                }
                else
                {
                    m_name = "Storm Core Installation";
                }
            }
            else
            {
                m_name = "Storm Core Installation";
            }

            return successful;
        }
        /// <summary>
        /// Function that will apply the fix to this option
        /// </summary>
        /// <param name="autoFix"></param>
        /// <returns></returns>
        public override bool FixNow(bool autoFix = false)
        {
            //Install storm
            if (StormWizardUtils.InstallStormCore())
            {
                m_foldedOut = false;
                StormWizardUtils.CheckWizardFinished = true;
                return true;
            }

            return false;
        }
    }
}