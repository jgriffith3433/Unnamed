#if PW_STORM_PRESENT
using ProceduralWorlds.Storm.Core;
#endif
using UnityEngine;

namespace ProceduralWorlds.Storm.Wizard
{
    [CreateAssetMenu(fileName = "WizardOptionStormRenderQuality", menuName = "Procedural Worlds/Storm/Wizard/Storm Render Quality Wizard Option")]
    public class WizardOptionStormRenderQuality : StormWizardOption
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

            m_name = "Storm Render Quality";
            m_infoTextIssue = "It seems that maybe Storm Core is not installed, install Storm Core and resolve the script define to fix this warning.";
            m_status = StormSettingStatus.Warning;

            m_infoTextOK = "This allows you to set the world render settings, these settings control the quality of storm such as LOD, Draw Distance, LOD Mixing, Density and much more.";
            m_wizardCategory = "Storm Setup";
            m_wizardOptionMode = WizardOptionMode.Dropdown;
            m_dropdownOptions = GetStormRenderQualites();
            onDropdownChanged -= DropdownChanged;
            onDropdownChanged += DropdownChanged;
            Initialize();
        }

        /// <summary>
        /// Function used to check if this option is successlly setup or not
        /// </summary>
        /// <returns></returns>
        public override bool PerformCheck()
        {
            m_dropdownID = GetWizardQualityID();
#if PW_STORM_PRESENT
            bool successful = StormWizardSettings.GetDefaultRenderQuality() != null;
#else
            bool successful = false;
#endif
            m_wasFixed = successful;
            m_status = successful ? StormSettingStatus.OK : StormSettingStatus.Warning;
            return successful;
        }
        /// <summary>
        /// Function that will apply the fix to this option
        /// </summary>
        /// <param name="autoFix"></param>
        /// <returns></returns>
        public override bool FixNow(bool autoFix = false)
        {
#if PW_STORM_PRESENT
            GameObject go = new GameObject("Storm World");
            go.transform.position = Vector3.zero;
            go.transform.localScale = Vector3.zero;
            go.transform.rotation = Quaternion.identity;
            go.AddComponent<StormWorld>();
            m_foldedOut = false;
            m_wasFixed = true;
            m_status = StormSettingStatus.OK;
            StormWizardUtils.CheckWizardFinished = true;
#endif
            return true;
        }
        /// <summary>
        /// Called when the dropdown has changed
        /// </summary>
        private void DropdownChanged()
        {
            ChangeWizardQualityID(GetDropdownValue());
#if PW_STORM_PRESENT
            StormWorld stormWorld = StormWorld.Instance;
            if (stormWorld != null)
            {
                stormWorld.QualitySettings = StormWizardSettings.GetRenderQuality();
            }
#endif
        }
    }
}