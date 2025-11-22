#if UNITY_EDITOR
using System.Drawing;
using UnityEditor;
using UnityEditor.PackageManager;
#endif
using UnityEngine;

namespace ProceduralWorlds.Storm.Wizard
{
    [CreateAssetMenu(fileName = "WizardOptionMathematicsPackage", menuName = "Procedural Worlds/Storm/Wizard/Mathematics Wizard Option")]
    public class WizardOptionMathematicsPackage : StormWizardOption
    {
        const string PackageCheckMessage = "Mathematics Package - Checking Installation, Please Wait...";
        bool m_packageCheckRunning = false;
        string m_originalName = string.Empty;
        string m_minVersion = "1.3.2";
        string m_packageName = "com.unity.mathematics";

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
            m_name = "Mathematics Package";
            m_originalName = m_name;
            m_infoTextOK = "This project has the Mathematics package installed. This is required for storm.";
            m_infoTextIssue = $"This project does not have the Mathematics package version {m_minVersion} or higher installed. Storm will not work.";
            m_status = StormSettingStatus.Warning;
            m_link = "https://docs.unity3d.com/Packages/com.unity.mathematics@1.0/manual/index.html";
            m_linkDisplayText = "Unity Mathematics 1.0+";
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
            StormPackageUtils.SearchForPackage(m_packageName, PackageMathmaticsSearchReceived);
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
            if (EditorUtility.DisplayDialog("Install Mathematics Package?", "This will download & install the Mathematics package via the Package Manager. Please be patient and wait for the installation to start, it can take a moment until the download finishes and Unity displays a progress bar.", "Install now", "Cancel"))
            {
                StormPackageUtils.AddPackage(m_packageName + "@" + m_minVersion);
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
            m_foldedOut = successful ? false : true;
            StormPackageUtils.onAddReceived -= PackageReceived;
        }
        private void PackageMathmaticsSearchReceived(bool successful, PackageCollection packages)
        {
            m_name = m_originalName;
            m_packageCheckRunning = false;
            if (successful)
            {
                bool versionMatch = false;
                foreach (UnityEditor.PackageManager.PackageInfo packageInfo in packages)
                {
                    //Debug.Log($"Name: " + packageInfo.name + " ID: " + packageInfo.packageId + " " + packageInfo.version);

                    if (packageInfo.name.Equals(m_packageName))
                    {
                        if (StormWizardUtils.IsVersionGreaterOrEqual(packageInfo.version, m_minVersion))
                        {
                            versionMatch = true; 
                            break;
                        }
                    }

                    foreach (DependencyInfo dependencyInfo in packageInfo.resolvedDependencies)
                    {
                        if (dependencyInfo.name.Equals(m_packageName))
                        {
                            if (StormWizardUtils.IsVersionGreaterOrEqual(dependencyInfo.version, m_minVersion))
                            {
                                versionMatch = true;
                                break;
                            }
                        }
                    }
                }
                successful = versionMatch;
            }
            m_packageResult = successful ? StormPackageResult.Successful : StormPackageResult.Failed;
            m_wasFixed = successful;
            m_status = successful ? StormSettingStatus.OK : StormSettingStatus.Error;
        }
#endif
    }
}