#if PW_STORM_PRESENT
using ProceduralWorlds.Storm.Utils;
#endif
using System.Collections.Generic;
using UnityEngine;

namespace ProceduralWorlds.Storm.Wizard
{
    /// <summary>
    /// This is the wizard settings class, information and options that is used in the wizard
    /// </summary>
    [CreateAssetMenu(fileName = "Storm Wizard Settings", menuName = "Procedural Worlds/Storm/Wizard/Storm Wizard Settings", order = 0)]
    public class StormWizardSettings : ScriptableObject
    {
        #region Wizard Variables

        /// <summary>
        /// Icon for when a wizard option has no errors
        /// </summary>
        public Texture IconOK;
        /// <summary>
        /// Icon for when a wizard option has errors but shown as a warning
        /// </summary>
        public Texture IconWarning;
        /// <summary>
        /// Icon for when a wizard option has errors
        /// </summary>
        public Texture IconError;
        /// <summary>
        /// List of the storm world render quality settings
        /// </summary>
        public List<ScriptableObject> RenderQualities
        {
            get { return m_renderQualities; }
            set { m_renderQualities = value; }
        }
        [SerializeField] private List<ScriptableObject> m_renderQualities = new List<ScriptableObject>();

        /// <summary>
        /// What we have set storm to use
        /// </summary>
        [SerializeField] private StormWizardRP m_targetRenderPipeline = StormWizardRP.Builtin;
        /// <summary>
        /// The render quality ID that is used to load a [RenderQualities]
        /// </summary>
        [HideInInspector] public int RenderQualityID;

        /// <summary>
        /// The directories to include in the Storm Installation package - that is what will make up the core installation.
        /// </summary>
        public List<string> m_directoriesToIncludeInInstallPackage;

        /// <summary>
        /// The directories to delete before the Storm Core package is installed - this assures a "clean" install without any leftovers from previous installations. This will mostly be the same directories that are in the install package, 
        /// but it possible that the list gets extended with obsolete legacy directories as well that need to be removed before the next core install.
        /// </summary>
        public List<string> m_directoriesToDeleteOnInstall;

        #endregion
        #region Public Wizard Util Functions


        /// <summary>
        /// Sets the targeted render pipeline
        /// </summary>
        /// <param name="rp"></param>
        public void SetTargetRenderPipeline(StormWizardRP rp)
        {
            m_targetRenderPipeline = rp;
        }

        /// <summary>
        /// Gets the targeted render pipeline
        /// </summary>
        /// <returns></returns>
        public StormWizardRP GetTargetRenderPipeline()
        {
            return m_targetRenderPipeline;
        }

        /// <summary>
        /// Sets the render quality ID
        /// </summary>
        /// <param name="id"></param>
        public void SetRenderQualityID(int id)
        {
            RenderQualityID = id;
        }
        /// <summary>
        /// Sets the can show on start up bool if this is true it will show the wizard if there is an issue or show on start up is set to true
        /// </summary>
        /// <param name="value"></param>
        public void SetCanShowOnStartup(bool value)
        {
            PlayerPrefs.SetInt(StormWizardUtils.CanShowOnStartup, value ? 1 : 0);
            PlayerPrefs.Save();
        }

        #endregion
        #region Public Static Get Storm World Quality Setting

#if PW_STORM_PRESENT
        /// <summary>
        /// Gets the default or first render quality
        /// </summary>
        /// <returns></returns>
        public static StormQualitySettings GetDefaultRenderQuality()
        {
            StormWizardSettings wizardSettings = StormWizardUtils.GetStormWizardSettings();
            if (wizardSettings != null)
            {
                if (wizardSettings.RenderQualities.Count > 0)
                {
                    for (int i = 0; i < wizardSettings.RenderQualities.Count; i++)
                    {
                        if (wizardSettings.RenderQualities[i] != null && wizardSettings.RenderQualities[i].name.Contains("Medium"))
                        {
                            return wizardSettings.RenderQualities[i] as StormQualitySettings;
                        }
                    }

                    return wizardSettings.RenderQualities[0] as StormQualitySettings;
                }
            }

            return null;
        }
        /// <summary>
        /// Gets the render quality
        /// </summary>
        /// <returns></returns>
        public static StormQualitySettings GetRenderQuality()
        {
            StormWizardSettings wizardSettings = StormWizardUtils.GetStormWizardSettings();
            if (wizardSettings != null)
            {
                if (wizardSettings.RenderQualities.Count > 0)
                {
                    if (wizardSettings.RenderQualityID <= wizardSettings.RenderQualities.Count - 1)
                    {
                        return wizardSettings.RenderQualities[wizardSettings.RenderQualityID] as StormQualitySettings;
                    }       
                }
            }

            return null;
        }
#endif

    #endregion
}
}