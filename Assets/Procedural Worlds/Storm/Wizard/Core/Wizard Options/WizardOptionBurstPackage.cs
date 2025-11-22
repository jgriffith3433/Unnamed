#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.PackageManager;
#endif
using UnityEngine;

namespace ProceduralWorlds.Storm.Wizard
{
    [CreateAssetMenu(fileName = "WizardOptionBurstPackage", menuName = "Procedural Worlds/Storm/Wizard/Burst Wizard Option")]
    public class WizardOptionBurstPackage : StormWizardOption
    {
        const string PackageCheckMessage = "Burst Package - Checking Installation, Please Wait...";
        bool m_packageCheckRunning = false;
        string m_originalName = string.Empty;

        public bool IsPackageCheckRunning
        {
            get { return m_packageCheckRunning; }
        }

        /// <summary>
        /// Called on startup
        /// </summary>
        private void OnEnable()
        {
            m_isPackageManagerRequired = true;
            m_packageResult = StormPackageResult.Unknown;
            m_RPBuiltIn = true;
            m_RPHDRP = true;
            m_RPURP = true;
            m_name = "Burst Package";
            m_originalName = m_name;
            m_infoTextOK = "This project has the Burst package installed. This is required for storm.";
            m_infoTextIssue = "This project does not have the Burst package installed. Storm will not work.";
            m_status = StormSettingStatus.Warning;
            m_link = "https://docs.unity3d.com/Packages/com.unity.burst@1.8/manual/index.html";
            m_linkDisplayText = "Unity Burst 1.8+";
            m_wizardCategory = "Dependencies";
            Initialize();
        }

        /// <summary>
        /// Function used to check if this option is successlly setup or not
        /// </summary>
        /// <returns></returns>
        public override bool PerformCheck()
        {
#if UNITY_EDITOR
            //Set the status to "Warning" while we wait for the package search
            m_status = StormSettingStatus.Warning;
            StormPackageUtils.SearchForPackage("com.unity.burst", PackageBurstSearchReceived);
            //replace the name of the Package with a check message while the check is running - better UX
            m_name = PackageCheckMessage;
            m_packageCheckRunning = true;
#endif
            return true;
        }
        /// <summary>
        /// Function that will apply the fix to this option
        /// </summary>
        /// <param name="autoFix"></param>
        /// <returns></returns>
        public override bool FixNow(bool autoFix = false)
        {
#if UNITY_EDITOR
            if (EditorUtility.DisplayDialog("Install Burst Package?", "This will download & install the Burst package via the Package Manager. Please be patient and wait for the installation to start, it can take a moment until the download finishes and Unity displays a progress bar.", "Install now", "Cancel"))
            {
                StormPackageUtils.AddPackage("com.unity.burst@1.8.7");
                StormPackageUtils.onAddReceived -= PackageReceived;
                StormPackageUtils.onAddReceived += PackageReceived;
                return true;
            }
#endif
            return false;
        }
#if UNITY_EDITOR
        private void PackageReceived(bool successful)
        {
            m_packageResult = successful ? StormPackageResult.Successful : StormPackageResult.Failed;
            m_wasFixed = successful;
            m_status = successful ? StormSettingStatus.OK : StormSettingStatus.Error;
            m_foldedOut = successful ? false: true;
            StormWizardUtils.CheckWizardFinished = true;
            StormPackageUtils.onAddReceived -= PackageReceived;
        }
        private void PackageBurstSearchReceived(bool successful, PackageCollection packages)
        {
            m_name = m_originalName;
            m_packageCheckRunning = false;
            m_packageResult = successful ? StormPackageResult.Successful : StormPackageResult.Failed;
            m_wasFixed = successful;
            m_status = successful ? StormSettingStatus.OK : StormSettingStatus.Error;
        }
#endif
    }
}