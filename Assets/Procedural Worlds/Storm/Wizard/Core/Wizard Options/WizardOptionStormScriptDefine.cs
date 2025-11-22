#if PW_STORM_PRESENT
#endif
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Build;
#endif

namespace ProceduralWorlds.Storm.Wizard
{
    [CreateAssetMenu(fileName = "Storm Script Define Wizard Option", menuName = "Procedural Worlds/Storm/Wizard/Storm Script Define Wizard Option")]
    public class WizardOptionStormScriptDefine : StormWizardOption
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
            m_name = "Storm Script Define";
            m_infoTextOK = "Storm script define 'PW_STORM_PRESENT', has been set in your project settings.";
            m_infoTextIssue = "Storm script define 'PW_STORM_PRESENT', has not been set in your project settings. This will affect some storm functions and plugins.";
            m_status = StormSettingStatus.Error;
            m_link = "https://docs.unity3d.com/Manual/CustomScriptingSymbols.html";
            m_linkDisplayText = "Adding Custom Script Defines";
            m_wizardCategory = "Storm Installation";
            Initialize();
        }

        /// <summary>
        /// Function used to check if this option is successlly setup or not
        /// </summary>
        /// <returns></returns>
        public override bool PerformCheck()
        {
#if UNITY_EDITOR
            string symbols = PlayerSettings.GetScriptingDefineSymbols(NamedBuildTarget.FromBuildTargetGroup(EditorUserBuildSettings.selectedBuildTargetGroup));
            if (!string.IsNullOrEmpty(symbols))
            {
                bool successful = symbols.Contains("PW_STORM_PRESENT");
                m_wasFixed = successful;
                m_status = successful ? StormSettingStatus.OK : StormSettingStatus.Error;
                return successful;
            }
            else
            {
                return false;
            }
#else
            m_status = StormSettingStatus.Error;
            return true;
#endif
        }
        /// <summary>
        /// Function that will apply the fix to this option
        /// </summary>
        /// <param name="autoFix"></param>
        /// <returns></returns>
        public override bool FixNow(bool autoFix = false)
        {
            if (!StormWizardUtils.StormCoreInstalled())
            {
                StormWizardUtils.DisplayStormCoreMissing();
                return false;
            }

#if UNITY_EDITOR
            //Add define
            string symbols = PlayerSettings.GetScriptingDefineSymbols(NamedBuildTarget.FromBuildTargetGroup(EditorUserBuildSettings.selectedBuildTargetGroup));
            bool changed = false;
            if (!symbols.Contains("PW_STORM_PRESENT"))
            {
                changed = true;
                if (symbols.Length > 0)
                {
                    symbols += ";PW_STORM_PRESENT";
                }
                else
                {
                    symbols += "PW_STORM_PRESENT";
                }
            }
            if (symbols.Contains("STORM2023_PRESENT"))
            {
                changed = true;
                int idx = symbols.IndexOf(";STORM2023_PRESENT");
                if (idx >= 0)
                    symbols.Remove(idx, ";STORM2023_PRESENT".Length);
                idx = symbols.IndexOf("STORM2023_PRESENT");
                if (idx >= 0)
                    symbols.Remove(idx, "STORM2023_PRESENT".Length);
            }
            if (changed)
            {
                PlayerSettings.SetScriptingDefineSymbols(NamedBuildTarget.FromBuildTargetGroup(EditorUserBuildSettings.selectedBuildTargetGroup), symbols);
            }
            m_foldedOut = false;
            m_wasFixed = true;
            m_status = StormSettingStatus.OK;
            StormWizardUtils.CheckWizardFinished = true;
#endif
            return true;
        }
    }
}