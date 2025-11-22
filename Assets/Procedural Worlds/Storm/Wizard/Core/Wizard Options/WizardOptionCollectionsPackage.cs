#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.PackageManager;

#endif

using UnityEngine;

namespace ProceduralWorlds.Storm.Wizard
{
    [CreateAssetMenu(fileName = "WizardOptionCollectionsPackage", menuName = "Procedural Worlds/Storm/Wizard/Collections Wizard Option")]
    public class WizardOptionCollectionsPackage : StormWizardOption
    {
        const string NoCollectionMessage = "This project does not have the Collections package 2.1.4 or higher installed. Storm requires the Collections package to function.";
        const string WrongCollectionMessage = "This project has version %v% of the Collections package but 2.1.4 or higher is required. Storm requires the Collections package to function.";
        const string PackageCheckMessage = "Collections Package - Checking Installation, Please Wait...";
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
            m_name = "Collections Package";
            m_originalName = m_name;
            m_infoTextOK = "This project has the Collections package 2.1.4 or higher installed. Storm requires the Collections package to function.";
            m_infoTextIssue = NoCollectionMessage;
            m_status = StormSettingStatus.Warning;
            m_link = "https://docs.unity3d.com/Packages/com.unity.collections@2.1/manual/index.html";
            m_linkDisplayText = "Unity Collections 2.1+";
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
            StormPackageUtils.SearchForPackage("com.unity.collections", PackageCollectionSearchReceived);
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
            if (EditorUtility.DisplayDialog("Install Collections Package?", "This will download & install the Collections package via the Package Manager. Please be patient and wait for the installation to start, it can take a moment until the download finishes and Unity displays a progress bar.", "Install now", "Cancel"))
            {
                StormPackageUtils.onAddReceived -= PackageReceived;
                StormPackageUtils.onAddReceived += PackageReceived;
#if UNITY_2022_3_OR_NEWER
                StormPackageUtils.AddPackage("com.unity.collections@2.1.4");
#else
            StormPackageUtils.AddPackage("com.unity.collections@1.5.1");
#endif
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
            StormWizardUtils.CheckWizardFinished = true;
            StormPackageUtils.onAddReceived -= PackageReceived;
        }
        private void PackageCollectionSearchReceived(bool successful, PackageCollection packageInfos)
        {
            m_name = m_originalName;
            m_packageCheckRunning = false;

            foreach (var item in packageInfos)
            {
                if (item.name == "com.unity.collections")
                {
                    bool versionCheckOK = VersionCheck(item.version, 2, 1, 4);

                    if (versionCheckOK)
                    {
                        m_packageResult = successful ? StormPackageResult.Successful : StormPackageResult.Failed;
                        m_wasFixed = successful;
                        m_status = successful ? StormSettingStatus.OK : StormSettingStatus.Error;
                    }
                    else
                    {
                        m_infoTextIssue = WrongCollectionMessage.Replace("%v%", item.version);
                        m_packageResult = StormPackageResult.Failed;
                        m_wasFixed = false;
                        m_status = StormSettingStatus.Error;
                    }
                    return;
                }
                else
                {
                    foreach (var dependancy in item.resolvedDependencies)
                    {
                        if (dependancy.name == "com.unity.collections")
                        {
                            bool versionCheckOK = VersionCheck(dependancy.version, 2, 1, 4);

                            if (versionCheckOK)
                            {
                                m_packageResult = successful ? StormPackageResult.Successful : StormPackageResult.Failed;
                                m_wasFixed = successful;
                                m_status = successful ? StormSettingStatus.OK : StormSettingStatus.Error;
                            }
                            else
                            {
                                m_infoTextIssue = WrongCollectionMessage.Replace("%v%", dependancy.version);
                                m_packageResult = StormPackageResult.Failed;
                                m_wasFixed = false;
                                m_status = StormSettingStatus.Error;
                            }
                            return;
                        }
                    }
                }
            }

            m_infoTextIssue = NoCollectionMessage;
            //package not found? We need to consider this failed / not installed as well
            m_packageResult = StormPackageResult.Failed;
            m_wasFixed = false;
            m_status = StormSettingStatus.Error;

        }
        bool VersionCheck(string version, int major, int minor, int patch)
        {
            int majorVersion, minorVersion, patchVersion = 0;
            if (!int.TryParse(version.Split('.')[0], out majorVersion))
                return false;
            if (!int.TryParse(version.Split('.')[1], out minorVersion))
                return false;
            if(!int.TryParse(version.Split('.')[2], out patchVersion))
                return false;

            if (majorVersion > major)
                return true;
            else if (majorVersion == major && minorVersion > minor)
                return true;
            else if (majorVersion == major && minorVersion == minor && patchVersion >= patch)
                return true;
            return false;
        }
#endif
    }

}