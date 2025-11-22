#if UNITY_EDITOR
using UnityEditor.PackageManager;
#endif
using UnityEngine;

namespace ProceduralWorlds.Storm.Wizard
{
    [CreateAssetMenu(fileName = "WizardOptionTextMeshProPackage", menuName = "Procedural Worlds/Storm/Wizard/Text Mesh Pro Wizard Option")]
    public class WizardOptionTextMeshProPackage : StormWizardOption
    {
        const string PackageCheckMessage = "Unity UI Package - Checking Installation, Please Wait...";
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

#if UNITY_2022_3_OR_NEWER
            m_name = "Unity UI Package";
            m_originalName = m_name;
            m_infoTextOK = "This project has the Unity UI package installed. This is required for the Storm runtime UI.";
            m_infoTextIssue = "This project does not have the Unity UI package installed. The Storm runtime UI will not work.";
            m_status = StormSettingStatus.Error;
            m_link = "https://docs.unity3d.com/Packages/com.unity.ugui@2.0/manual/index.html";
            m_linkDisplayText = "Unity UI Package";
#else
            m_name = "Text Mesh Pro Package";
            m_infoTextOK = "This project has the Text Mesh Pro package installed. This is required for the storm runtime UI.";
            m_infoTextIssue = "This project does not have the Text Mesh Pro package installed. Storm runtime UI will not work.";
            m_status = StormSettingStatus.Error;
            m_link = "https://docs.unity3d.com/Packages/com.unity.textmeshpro@3.0/manual/index.html";
            m_linkDisplayText = "Text Mesh Pro";
#endif



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
#if UNITY_2022_3_OR_NEWER
            StormPackageUtils.SearchForPackage("com.unity.ugui", PackageTMPSearchReceived);

#else
            StormPackageUtils.SearchForPackage("com.unity.textmeshpro", PackageTMPSearchReceived);
#endif
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
            StormPackageUtils.AddPackage("com.unity.textmeshpro");
            StormPackageUtils.onAddReceived -= PackageReceived;
            StormPackageUtils.onAddReceived += PackageReceived;
            return true;
        }

        private void PackageReceived(bool successful)
        {
            m_packageResult = successful ? StormPackageResult.Successful : StormPackageResult.Failed;
            m_status = successful ? StormSettingStatus.OK : StormSettingStatus.Error;
            m_foldedOut = successful ? false : true;

#if !UNITY_2022_3_OR_NEWER
            if (!m_wasFixed && !File.Exists(StormWizardUtils.GetAssetPath("TMP_SDF-Mobile.shader")))
            {
                MethodInfo method = StormWizardUtils.GetMethodFromType("TMPro.TMP_PackageResourceImporter", "ImportResources");
                if (method != null)
                {
                    method.Invoke(null, new object[3] { true, false, false });
                    m_wasFixed = true;
                }
            }
#endif

            StormPackageUtils.onAddReceived -= PackageReceived;
        }
#if UNITY_EDITOR
        private void PackageTMPSearchReceived(bool successful, PackageCollection packages)
        {
            m_name = m_originalName;
            m_packageCheckRunning = false;

            m_packageResult = successful ? StormPackageResult.Successful : StormPackageResult.Failed;
            m_status = successful ? StormSettingStatus.OK : StormSettingStatus.Error;
            if (successful)
            {
                #if !UNITY_2022_3_OR_NEWER
                if (!m_wasFixed && !File.Exists(StormWizardUtils.GetAssetPath("TMP_SDF-Mobile.shader")))
                {
                    MethodInfo method = StormWizardUtils.GetMethodFromType("TMPro.TMP_PackageResourceImporter", "ImportResources");
                    if (method != null)
                    {
                        method.Invoke(null, new object[3] { true, false, false });
                        m_wasFixed = true;
                    }
                }
                else
                {
                    m_wasFixed = true;
                }
                #endif
            }
        }
#endif
    }
}