#if PW_STORM_PRESENT
using ProceduralWorlds.Storm.Utils;
#endif
using UnityEditor;
using UnityEditor.Compilation;
using System.IO;
using System.Diagnostics;
using UnityEngine;

namespace ProceduralWorlds.Storm.Wizard
{
    /// <summary>
    /// Initialization class used to setup some wizard settings and to see if the wizard needs to be shown on startup
    /// </summary>
    [InitializeOnLoad]
    public class StormWizardInitialization : AssetPostprocessor
    {
        #region Private Static Variables

        /// <summary>
        /// Set to true when the event compilationStarted is called
        /// Sets back to false on the event compilationFinished is called
        /// </summary>
        private static bool m_compliationInProgress;

        /// <summary>
        /// Static access to the wizard settings
        /// </summary>
        private static StormWizardSettings WizardSettings
        {
            get
            {
                if (m_wizardSettings == null)
                {
                    m_wizardSettings = StormWizardUtils.GetStormWizardSettings();
                }

                return m_wizardSettings;
            }
        }
        private static StormWizardSettings m_wizardSettings;
        private const string MaintenanceTokenFilename = "StormMaintenanceToken.dat";

        #endregion
        #region Initialization

        /// <summary>
        /// Called when the editor class is initialzed
        /// </summary>
        public static void Initialization()
        {
            //Compliation Events
            CompilationPipeline.compilationStarted -= CompliationStarted;
            CompilationPipeline.compilationStarted += CompliationStarted;
            //Editor Quitting Events
            EditorApplication.wantsToQuit -= OnQuittingApplication;
            EditorApplication.wantsToQuit += OnQuittingApplication;
#if UNITY_2022_3_OR_NEWER
            //On Focus Changed Event (Used and is called when user opens the project)
            EditorApplication.focusChanged -= OnOpeningProject;
            EditorApplication.focusChanged += OnOpeningProject;
#else
            EditorApplication.hierarchyChanged -= OnOpeningProject;
            EditorApplication.hierarchyChanged += OnOpeningProject;
#endif

            //We only open the wizard from here if Storm is already installed and there is no update package left to install anymore
            //in the other cases the wizard will be opened from the asset post-processor part below.
            if (StormWizardUtils.StormCoreInstalled() || !StormWizardUtils.StormCorePackageExists())
            {
                StormWizard.CheckShowWizard(false, false);
            }

        }

        #endregion
        #region Public Static Utils

        /// <summary>
        /// This is used to find all storm world settings in the Storm Render Settings found in the wizard folder.
        /// This will then load them and assign them to the wizard settings
        /// </summary>
        public static void CheckRenderQualies()
        {
#if PW_STORM_PRESENT
            if (WizardSettings.RenderQualities.Count < 1)
            {
                string[] allSWGUIDs = AssetDatabase.FindAssets("t:StormQualitySettings", new string[1] { StormWizardUtils.GetStormWorldRenderSettingsDirectory() + "/" });
                for (int i = 0; i < allSWGUIDs.Length; i++)
                {
                    string guid = allSWGUIDs[i];
                    StormQualitySettings sWSetting = (StormQualitySettings)AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(guid), typeof(StormQualitySettings));
                    if (sWSetting != null)
                    {
                        WizardSettings.RenderQualities.Add(sWSetting);
                    }
                }

                WizardSettings.RenderQualities.Sort();

#if UNITY_EDITOR
                EditorUtility.SetDirty(WizardSettings);
#endif
            }
#endif
        }

        #endregion
        #region Private Static Callback Events

        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths, bool didDomainReload)
        {
            //Check if the "Storm Maintenance Token" exists, if yes, this is after the storm package was imported and we need to show the setup window for the installation / update
            if (!System.IO.Directory.Exists(StormWizardUtils.GetAssetPath("Dev Utilities")) && File.Exists(StormWizardUtils.GetTokenDirectory() + "/" + MaintenanceTokenFilename))
            {
                //Prepare cryptlex dylibs on macOs for first access
                if (SystemInfo.operatingSystemFamily == OperatingSystemFamily.MacOSX)
                {
                    string assetsPath = Application.dataPath;
                    string cryptlexPath = Path.Combine(assetsPath, "Procedural Worlds/Storm/Plugins/Cryptlex");

                    if (!Directory.Exists(cryptlexPath))
                    {
                        UnityEngine.Debug.LogError("Cryptlex folder not found at: " + cryptlexPath);
                        return;
                    }

                    string[] dylibFiles = Directory.GetFiles(cryptlexPath, "*.dylib", SearchOption.AllDirectories);

                    if (dylibFiles.Length == 0)
                    {
                        UnityEngine.Debug.LogWarning("No .dylib files found in: " + cryptlexPath);
                        return;
                    }

                    foreach (string dylibPath in dylibFiles)
                    {
                        StormWizardUtils.RunXattrCommand(dylibPath);
                    }
                }
                StormWizard.CheckShowWizard(false, true);
                RemoveMaintenanceToken();
            }

#if PW_STORM_PRESENT
            //Always refresh the wizard window on asset post-processing
            if (EditorWindow.HasOpenInstances<StormWizard>())
            {
                StormWizard.CheckForSetupIssues(true);
            }
#endif
        }

        private static void RemoveMaintenanceToken()
        {
            if (!System.IO.Directory.Exists(StormWizardUtils.GetAssetPath("Dev Utilities")))
            {
                FileUtil.DeleteFileOrDirectory(StormWizardUtils.GetTokenDirectory() + "\\" + MaintenanceTokenFilename);
                FileUtil.DeleteFileOrDirectory(StormWizardUtils.GetTokenDirectory() + "\\" + MaintenanceTokenFilename + ".meta");
                AssetDatabase.Refresh();
            }
        }



#if UNITY_2022_3_OR_NEWER
        /// <summary>
        /// Used for the focusChanged callback event
        /// </summary>
        /// <param name="state"></param>
        private static void OnOpeningProject(bool state)
        {
            if (!m_compliationInProgress)
            {
                CompliationFinished(null);
            }

            EditorApplication.focusChanged -= OnOpeningProject;
        }
#endif

        /// <summary>
        /// Used for the focusChanged callback event
        /// </summary>
        /// <param name="state"></param>
        private static void OnOpeningProject()
        {
            if (!m_compliationInProgress)
            {
                CompliationFinished(null);
            }

            EditorApplication.hierarchyChanged -= OnOpeningProject;
        }
        /// <summary>
        /// Used for the wantsToQuit callback event
        /// </summary>
        /// <returns></returns>
        private static bool OnQuittingApplication()
        {
            WizardSettings.SetCanShowOnStartup(true);
            EditorUtility.SetDirty(m_wizardSettings);
            AssetDatabase.SaveAssets();
            EditorApplication.wantsToQuit -= OnQuittingApplication;
            return true;
        }
        /// <summary>
        /// Used for the compilationStarted callback event
        /// </summary>
        /// <param name="obj"></param>
        private static void CompliationStarted(object obj)
        {
            m_compliationInProgress = true;
            CompilationPipeline.compilationFinished -= CompliationFinished;
            CompilationPipeline.compilationFinished += CompliationFinished;
        }
        /// <summary>
        /// Used for the compilationFinished callback event
        /// </summary>
        /// <param name="obj"></param>
        private static void CompliationFinished(object obj)
        {
            CompilationPipeline.compilationFinished -= CompliationFinished;
            //Always refresh the wizard window on compilation
            if (EditorWindow.HasOpenInstances<StormWizard>())
            {
                StormWizard.CheckForSetupIssues(true);
            }
            m_compliationInProgress = false;
        }

        #endregion
    }
}