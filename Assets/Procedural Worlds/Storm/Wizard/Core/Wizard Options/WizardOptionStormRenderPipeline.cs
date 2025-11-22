//#if PW_STORM_PRESENT
//using ProceduralWorlds.Storm.Utils;
//#endif
//using UnityEngine;

//namespace ProceduralWorlds.Storm.Wizard
//{
//    [CreateAssetMenu(fileName = "Storm Render Pipeline Wizard Option", menuName = "Procedural Worlds/Storm/Wizard/Storm Render Pipeline Wizard Option")]
//    public class WizardOptionStormRenderPipeline : StormWizardOption
//    {
//        /// <summary>
//        /// Called on startup
//        /// </summary>
//        private void OnEnable()
//        {
//            m_isPackageManagerRequired = false;
//            m_packageResult = StormPackageResult.Unknown;
//            m_RPBuiltIn = true;
//            m_RPHDRP = true;
//            m_RPURP = true;
//            m_name = "Storm Active Render Pipeline";
//            RefreshActivePipeline();

//            StormWizardRP actRP = GetActiveRP();
//            StormWizardRP tarRP = GetTargetRP();
//            m_infoTextOK = "Storms target render pipeline has been set to the currently active render pipeline that you are using. Currently active: " + actRP;
//            m_infoTextIssue = "Storm has not been configured to use the correct render pipeline. You are using: " + actRP + " but it is targeting: " + tarRP + ". Please switch Storm's target render pipeline to the currently active pipeline, or change the render pipeline of the project.";
//            m_wizardOptionMode = WizardOptionMode.Dropdown;
//            m_status = StormSettingStatus.Warning;
//            m_dropdownID = tarRP switch
//            {
//                StormWizardRP.Builtin => 0,
//                StormWizardRP.URP => 1,
//                StormWizardRP.HDRP => 2,
//                _ => 0
//            };

//            m_link = "https://docs.unity3d.com/Manual/render-pipelines.html";
//            m_linkDisplayText = "Unity Render Pipelines Manual";
//            m_wizardCategory = "Storm Setup";
//            m_dropdownOptions = new System.Collections.Generic.List<string> { "Built In", "Universal", "High Definition" };
//            onDropdownChanged -= DropdownChanged;
//            onDropdownChanged += DropdownChanged;
//            Initialize();
//        }

//        /// <summary>
//        /// Function used to check if this option is successlly setup or not
//        /// </summary>
//        /// <returns></returns>
//        public override bool PerformCheck()
//        {
//            StormWizardRP actRP = GetActiveRP(); 
//            StormWizardRP tarRP = GetTargetRP();
//            bool successful = actRP == tarRP;
//            m_wasFixed = successful;
//            m_status = successful ? StormSettingStatus.OK : StormSettingStatus.Warning;
//            m_infoTextOK = "Storms render pipeline configuration is matching the render pipeline of the project. Currently using: " + actRP;
//            m_infoTextIssue = "Storm has not been configured to use the correct render pipeline. The project uses: " + actRP + " but Storm is configured to use: " + tarRP;
//            return successful;
//        }
//        /// <summary>
//        /// Function that will apply the fix to this option
//        /// </summary>
//        /// <param name="autoFix"></param>
//        /// <returns></returns>
//        public override bool FixNow(bool autoFix = false)
//        {
//            if (!StormWizardUtils.StormCoreInstalled())
//            {
//                StormWizardUtils.DisplayStormCoreMissing();
//                return false;
//            }

//            SetTargetRP();
//            m_foldedOut = false;
//            StormWizardUtils.CheckWizardFinished = true;
//            return PerformCheck();
//        }
//        /// <summary>
//        /// Called when the dropdown has changed
//        /// </summary>
//        private void DropdownChanged()
//        {

//            SetTargetRP((StormWizardRP)GetDropdownValue());
//            if (!PerformCheck())
//            {
//                m_foldedOut = true;
//            }
//        }
//    }
//}