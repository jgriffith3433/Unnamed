//#if PW_STORM_PRESENT
//using ProceduralWorlds.Storm.Core;
//#endif
//using UnityEngine;

//namespace ProceduralWorlds.Storm.Wizard
//{
//    [CreateAssetMenu(fileName = "Storm World Setup Wizard Option", menuName = "Procedural Worlds/Storm/Wizard/Storm World Setup Wizard Option")]
//    public class WizardOptionStormWorldSetup : StormWizardOption
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
//            m_name = "Storm World In Active Scene";
//            m_infoTextOK = "Storm World was found in your scene. You can now use storm to ingest your world.";
//            m_infoTextIssue = "Storm World was not found in your scene, this is require to use storm and ingest and manage your world.";
//            m_status = StormSettingStatus.Error;
//            m_wizardCategory = "Storm Setup";
//            Initialize();
//        }

//        /// <summary>
//        /// Function used to check if this option is successlly setup or not
//        /// </summary>
//        /// <returns></returns>
//        public override bool PerformCheck()
//        {
//#if PW_STORM_PRESENT
//            bool successful = StormWorld.Instance != null;
//            m_wasFixed = successful;
//            m_status = successful ? StormSettingStatus.OK : StormSettingStatus.Error;
//            return successful;
//#else
//            m_status = StormSettingStatus.Error;
//            return false;
//#endif
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

//#if PW_STORM_PRESENT
//            StormWizardUtils.CreateStormWorld();
//            m_foldedOut = false;
//            m_wasFixed = true;
//            m_status = StormSettingStatus.OK;
//            return true;
//#else
//            return false;
//#endif
//        }
//    }
//}