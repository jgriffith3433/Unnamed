#if PW_STORM_PRESENT
using ProceduralWorlds.Storm.Core;
#endif
using ProceduralWorlds.Storm.StormEditor;
using PWCommon5;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.PackageManager.UI;
using UnityEngine;

namespace ProceduralWorlds.Storm.Wizard
{
    /// <summary>
    /// The main wizard class that draws the storm wizard window
    /// </summary>
    public class StormWizard : EditorWindow, IPWEditor
    {
        #region Editor Vatiables

        public bool PositionChecked { get; set; }

        private Vector2 m_scrollPos;
        private EditorUtils m_editorUtils;
        private GUIStyle m_boxStyle;
        private GUIStyle m_boldHeader;
        private GUIStyle m_boldHeaderLeft;
        private GUIStyle m_buttonAlignRight;
        private GUIStyle m_linkStyle;

        #endregion
        #region Links

        private const string StormWebPage = "https://www.procedural-worlds.com/products/enterprise/storm/";
        private const string StormDocumentationPage = "https://canopy.procedural-worlds.com/library/tools/storm/";

        #endregion
        #region Wizard Settings

        //Storm Wizard Settings
        private static List<StormWizardOption> m_allWizardSettings = new List<StormWizardOption>();
        private static List<UICategory> m_uiCategories = new List<UICategory>();
        private static StormWizardSettings m_wizardSettings;
        private static bool m_ignoredOpeningStormManager;
#if PW_STORM_PRESENT
        private static bool m_showHiddenLicenseKey = false;
        private static string m_currentLicenseKey = null;
#endif
        private GUIContent m_PDFGuideContent;
        private GUIStyle m_helpBoxStyle;

#endregion
        #region Unity Functions

        /// <summary>
        /// Called on enable when the window starts up
        /// </summary>
        private void OnEnable()
        {
            CheckForSetupIssues(false);

            m_editorUtils = StormApp.GetEditorUtils(this);
            m_ignoredOpeningStormManager = false;

            StormWizardInitialization.Initialization();

            if (m_PDFGuideContent == null || m_PDFGuideContent.text == "")
            {
                m_PDFGuideContent = new GUIContent("Open Storm Manual PDF", "Opens the Storm Manual PDF document.");
            }


            if (m_wizardSettings == null)
            {
                m_wizardSettings = StormWizardUtils.GetStormWizardSettings();
            }

            if (StormPackageUtils.ListRequestInstances.Count > 0)
            {
                EditorApplication.update -= RefreshWindow;
                EditorApplication.update += RefreshWindow;
            }

            ClearGUIStyles();
        }

        private static void RefreshWindow()
        {
            if (StormPackageUtils.ListRequestInstances.Count <= 0)
            {
                StormWizard window = (StormWizard)EditorWindow.GetWindow(typeof(StormWizard));
                //Manager can be null if the dependency package installation is started upon opening the manager window.
                if (window != null)
                {
                    window.Repaint();
                }
                EditorApplication.update -= RefreshWindow;
            }
        }

        /// <summary>
        /// Called when the window is destroyed (closed)
        /// </summary>
        private void OnDestroy()
        {
            if (m_editorUtils != null)
            {
                m_editorUtils.Dispose();
            }
        }

        /// <summary>
        /// Called every frame when the window is open
        /// </summary>
        private void OnGUI()
        {
            m_editorUtils.Initialize();
            CreateGUIStyles();
            m_editorUtils.Panel("StormWizard", DrawWizardUI, true);
            CheckAllWizardOptionsAreSetupCorrectly();
        }

        #endregion
        #region GUI Functions
        private void CreateGUIStyles()
        {
            //Set up the box style
            if (m_boxStyle == null || IsBlack(m_boxStyle.normal.textColor))
            {
                m_boxStyle = new GUIStyle(GUI.skin.box);
                m_boxStyle.normal.textColor = GUI.skin.label.normal.textColor;
                m_boxStyle.fontStyle = FontStyle.Bold;
                m_boxStyle.alignment = TextAnchor.UpperLeft;
            }
            if (m_helpBoxStyle == null || (m_helpBoxStyle.normal.textColor.r + m_helpBoxStyle.normal.textColor.g + m_helpBoxStyle.normal.textColor.b == 0))
            {
                m_helpBoxStyle = new GUIStyle(GUI.skin.box);
                m_helpBoxStyle.normal.textColor = GUI.skin.label.normal.textColor;
                m_helpBoxStyle.margin = new RectOffset(0, 0, 0, 0);
                m_helpBoxStyle.padding = new RectOffset(5, 5, 5, 5);
                m_helpBoxStyle.alignment = TextAnchor.UpperLeft;
                m_helpBoxStyle.stretchWidth = true;
                m_helpBoxStyle.richText = true;
                m_helpBoxStyle.wordWrap = true;
            }

            if (m_boldHeader == null || IsBlack(m_boldHeader.normal.textColor))
            {
                m_boldHeader = new GUIStyle(EditorStyles.boldLabel);
                m_boldHeader.fontStyle = FontStyle.Bold;
                m_boldHeader.alignment = TextAnchor.MiddleCenter;
                m_boldHeader.fontSize = 22;
            }
            if (m_boldHeaderLeft == null || IsBlack(m_boldHeaderLeft.normal.textColor))
            {
                m_boldHeaderLeft = new GUIStyle(EditorStyles.boldLabel);
                m_boldHeaderLeft.fontStyle = FontStyle.Bold;
                m_boldHeaderLeft.alignment = TextAnchor.MiddleLeft;
                m_boldHeaderLeft.fontSize = 22;
            }
            if (m_buttonAlignRight == null || IsBlack(m_buttonAlignRight.normal.textColor))
            {
                m_buttonAlignRight = new GUIStyle(EditorStyles.miniButtonRight);
            }
            if (m_linkStyle == null || IsBlack(m_linkStyle.normal.textColor))
            {
                m_linkStyle = new GUIStyle(GUI.skin.label);
                m_linkStyle.fontStyle = FontStyle.Normal;
                m_linkStyle.wordWrap = false;
                m_linkStyle.normal.textColor = new Color(0x00 / 255f, 0x78 / 255f, 0xDA / 255f, 1f);
                m_linkStyle.stretchWidth = false;
            }
        }
        private bool IsBlack(Color c)
        {
            return c.r == 0 && c.g == 0 && c.b == 0 && c.a == 0;
        }
        private void ClearGUIStyles()
        {
            //Set up the box style
            m_boxStyle = null;
            m_boldHeader = null;
            m_boldHeaderLeft = null;
            m_buttonAlignRight = null;
            m_linkStyle = null;
        }
        /// <summary>
        /// Draws the whole wizard
        /// </summary>
        /// <param name="helpEnabled"></param>
        private void DrawWizardUI(bool helpEnabled)
        {
            m_scrollPos = GUILayout.BeginScrollView(m_scrollPos);
            DrawHeader();
            DrawStatus(helpEnabled);
            DrawFooter(helpEnabled);
            GUILayout.EndScrollView();
        }
        /// <summary>
        /// Draws the header
        /// </summary>
        private void DrawHeader()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.BeginVertical();
            GUILayout.Label("Version: " + StormApp.CONF.Version, GUILayout.MaxWidth(100f));
            //if (ClickableHeaderCustomStyle(new GUIContent("View Documentation", StormDocumentationPage), m_linkStyle))
            //{
            //    Application.OpenURL(StormDocumentationPage);
            //}
            if (ClickableHeaderCustomStyle(m_PDFGuideContent, m_linkStyle))
            {
                UnityEngine.Object guide = (UnityEngine.Object)StormWizardUtils.GetAsset("Storm - User Guide.pdf", typeof(UnityEngine.Object));
                if (guide != null)
                {
                    Application.OpenURL(StormWizardUtils.GetFullFileSystemPath(AssetDatabase.GetAssetPath(guide)));
                    //EditorGUIUtility.PingObject(guide);
                }
            }
            EditorGUILayout.Space(5);

            GUILayout.Label("<b>Welcome to Storm!</b>", m_helpBoxStyle);
            GUILayout.Label("This wizard will guide you through the Storm setup process to make sure Storm and its dependencies are set up correctly.", m_helpBoxStyle);
            GUILayout.Label("<b>1. DEPENDENCIES</b>: These are additional Unity packages that you need in your project in order to use Storm. Please install the required packages from the Unity Package Manager. You can also use the 'Fix' buttons next to the entry to let the Storm Wizard do this for you.", m_helpBoxStyle);
            GUILayout.Label("<b>2. STORM INSTALLATION</b>: Once all packages are installed, you can access the Core Storm Installation here. If a Storm Update is later installed, the core installation can be updated here as well.", m_helpBoxStyle);
            GUILayout.Label("<b>3. STORM LICENSE</b>: Once all packages are installed, you can enter the Storm license key (trial or full license) here. Once the key has been entered, it will display information about the validity of the key.", m_helpBoxStyle);
            GUILayout.Label("<b>4. SCENE SETUP</b>: After the license key has been entered, Storm is ready to use. In the 'Scene Setup' panel you can initiate adding Storm to the scene and review its configuration state.", m_helpBoxStyle);

            EditorGUILayout.EndVertical();
            //GUILayout.Label("       ", m_boldHeader);
            //if (m_editorUtils.Button("CheckForUpdate", GUILayout.MaxWidth(125f)))
            //{
            //    Application.OpenURL(StormWebPage);
            //}
            EditorGUILayout.EndHorizontal();


            m_editorUtils.Label("CheckList", m_boldHeaderLeft);
        }
        /// <summary>
        /// Draws all the wizard settings
        /// </summary>
        /// <param name="obj"></param>
        private void DrawStatus(bool helpEnabled)
        {
            //it can happen through recompiles etc. that the category list gets emptied while the window is still open
            if (m_uiCategories.Count() == 0)
            {
                RefreshWizardSettings();
                CheckForSetupIssues(false);
            }

            for (int i = 0; i < m_uiCategories.Count; i++)
            {
                UICategory uICategory = m_uiCategories[i];
                if (uICategory.Options.Count > 0)
                {
                    EditorGUILayout.BeginVertical(m_boxStyle);
                    EditorGUILayout.LabelField(uICategory.CategoryName, EditorStyles.boldLabel);


                    //Restrict a bit if we actually want to show all wizard options - if e.g. no depencies are being installed, we only want to show
                    //the options to install the dependency packages, then we can unlock the option to install storm, and only after that we can display all options.
                    bool display = true;

                    switch (uICategory.CategoryName)
                    {
                        case "Storm Installation":
                            if (m_uiCategories.Any(x => x.CategoryName == "Dependencies" && x.Options.Any(y => y.GetStatus() != StormSettingStatus.OK)))
                            {
                                EditorGUILayout.HelpBox("Please complete the entries listed under 'Dependencies' above first. Storm requires those to work. More options will become available here then.", MessageType.Info);
                                display = false;
                            }
                            else if (!StormWizardUtils.StormCoreInstalled())
                            {
                                //All dependencies are installed, but Storm itself is not, this is where we offer the single option to install of the core storm package.
                                StormWizardOption swo = uICategory.Options.Find(x => x.m_installationOption == true);
                                if (swo != null)
                                {
                                    swo.DrawUI();
                                }
                                else
                                {
                                    Debug.LogError("Could not find the wizard option that installs the Core Storm Installation package?");
                                }

                                //still do not display any other options though
                                display = false;

                            }
                            break;
                        case "Storm Setup":
                            if (m_uiCategories.Any(x => x.CategoryName == "Dependencies" && x.Options.Any(y => y.GetStatus() != StormSettingStatus.OK)))
                            {
                                EditorGUILayout.HelpBox("Please install the Unity packages listed under 'Dependencies' above first. Storm requires those to work. More options will become available here then.", MessageType.Info);
                                display = false;
                            }
                            else if (!StormWizardUtils.StormCoreInstalled() || m_uiCategories.Any(x => x.CategoryName == "Storm Installation" && x.Options.Any(y => y.GetStatus() != StormSettingStatus.OK)))
                            {
                                EditorGUILayout.HelpBox("Please install / update the Storm Core Application first. (See warning displayed above) More options will become available here then.", MessageType.Info);
                                display = false;
                            }
                            break;
                        case "Storm License":

#if PW_STORM_PRESENT
                            float keyInputFieldWidth = 315;
                            if (m_currentLicenseKey == null)
                                m_currentLicenseKey = Licensing.CurrentKey;
                            EditorGUILayout.BeginHorizontal();

                            if (Licensing.LastCheckResult)
                            {
                                m_editorUtils.Label("LicenseKey", GUILayout.Width(EditorGUIUtility.labelWidth));
                                bool originalGUIState = GUI.enabled;
                                GUI.enabled = false;
                                GUILayout.Space(-5);
                                if (m_showHiddenLicenseKey)
                                {
                                    EditorGUILayout.TextField(m_currentLicenseKey, GUILayout.Width(keyInputFieldWidth));
                                }
                                else
                                {
                                    EditorGUILayout.PasswordField(m_currentLicenseKey, GUILayout.Width(keyInputFieldWidth));
                                }
                                GUI.enabled = originalGUIState;
                            }
                            else
                            {
                                if (m_showHiddenLicenseKey)
                                {
                                    m_currentLicenseKey = m_editorUtils.TextField("LicenseKey", m_currentLicenseKey, helpEnabled, GUILayout.Width(keyInputFieldWidth + EditorGUIUtility.labelWidth));
                                }
                                else
                                {
                                    m_currentLicenseKey = m_editorUtils.PasswordField("LicenseKey", m_currentLicenseKey, helpEnabled, GUILayout.Width(keyInputFieldWidth + EditorGUIUtility.labelWidth));
                                }
                            }

                            if (string.IsNullOrWhiteSpace(m_currentLicenseKey))
                            {
                                m_showHiddenLicenseKey = true;
                            }

                            GUILayout.Space(10);


                            if (!m_showHiddenLicenseKey)
                            {
                                if (m_editorUtils.Button("Show"))
                                {
                                    m_showHiddenLicenseKey = true;
                                }
                            }
                            else
                            {
                                if (m_editorUtils.Button("Hide"))
                                {
                                    m_showHiddenLicenseKey = false;
                                }
                            }
                            if (Licensing.LastCheckResult)
                            {
                                if (m_editorUtils.Button("Remove"))
                                {
                                    if (EditorUtility.DisplayDialog("Remove Storm License?", "Do you really want to remove the current Storm License Information from this computer?\r\n\r\nYou will need to enter a valid license key afterwards or you will not be able to use Storm anymore on this machine.", "Remove Key", "Cancel"))
                                    {
                                        Licensing.DeactivateKey();
                                        Licensing.ResetData();
                                        Licensing.CurrentKey = " ";
                                        m_currentLicenseKey = " ";
                                        m_showHiddenLicenseKey = true;
                                        for (int j = 0; j < uICategory.Options.Count; j++)
                                        {
                                            StormWizardOption option = uICategory.Options[j];
                                            if (option != null)
                                            {
                                                option.PerformCheck();
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                if (m_editorUtils.Button("Submit"))
                                {
                                    Licensing.ResetData();
                                    m_currentLicenseKey = m_currentLicenseKey.Trim();
                                    Licensing.IsLicenseValid(m_currentLicenseKey);
                                    m_showHiddenLicenseKey = false;
                                    for (int j = 0; j < uICategory.Options.Count; j++)
                                    {
                                        StormWizardOption option = uICategory.Options[j];
                                        if (option != null)
                                        {
                                            option.PerformCheck();
                                        }
                                    }
                                }
                            }

                            EditorGUILayout.EndHorizontal();

                            if (!String.IsNullOrEmpty(m_currentLicenseKey) && Licensing.LicenseExpires > DateTime.Now)
                            {
                                EditorGUILayout.LabelField("Expiration Info", $"This license will expire on {Licensing.LicenseExpires.ToShortDateString()} {Licensing.LicenseExpires.ToShortTimeString()}.");
                            }

#endif
                            if (m_uiCategories.Any(x => x.CategoryName == "Dependencies" && x.Options.Any(y => y.GetStatus() != StormSettingStatus.OK)))
                            {
                                EditorGUILayout.HelpBox("Please install the Unity packages listed under 'Dependencies' above first. Storm requires those to work. More options will become available here then.", MessageType.Info);
                                display = false;
                            }
                            else if (!StormWizardUtils.StormCoreInstalled() || m_uiCategories.Any(x => x.CategoryName == "Storm Installation" && x.Options.Any(y => y.GetStatus() != StormSettingStatus.OK)))
                            {
                                EditorGUILayout.HelpBox("Please install / update the Storm Core Application first. (See warning displayed above) More options will become available here then.", MessageType.Info);
                                display = false;
                            }
                            break;

                        default:
                            break;
                    }

                    if (display)
                    {
                        for (int j = 0; j < uICategory.Options.Count; j++)
                        {
                            StormWizardOption option = uICategory.Options[j];
                            if (option != null)
                            {
                                option.DrawUI();
                            }
                        }
                    }
                    EditorGUILayout.EndVertical();
                }

            }
        }
        /// <summary>
        /// Draws the footer
        /// </summary>
        private void DrawFooter(bool helpEnabled)
        {
            EditorGUILayout.BeginHorizontal();
            bool currentGUIState = GUI.enabled;
#if !PW_STORM_PRESENT
            GUI.enabled = false;
#endif
            if (m_editorUtils.Button("OpenStormManager", GUILayout.MaxWidth(150f)))
            {
                OpenStormManagerWithReflection();
            }

            GUI.enabled = currentGUIState;

            GUILayout.Label("       ", m_boldHeader);
            if (m_editorUtils.Button("CloseWizard", GUILayout.MaxWidth(105f)))
            {
                StormWizardUtils.CheckWizardFinished = true;
                if (CheckIfSetupIsComplete())
                {
                    this.Close();
                }
            }
            EditorGUILayout.EndHorizontal();
        }
        /// <summary>
        /// Used to create a clickable text GUI
        /// </summary>
        /// <param name="content"></param>
        /// <param name="style"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        private bool ClickableHeaderCustomStyle(GUIContent content, GUIStyle style, GUILayoutOption[] options = null)
        {
            var position = GUILayoutUtility.GetRect(content, style, options);
            Handles.BeginGUI();
            Color oldColor = Handles.color;
            Handles.color = style.normal.textColor;
            Handles.DrawLine(new Vector3(position.xMin, position.yMax), new Vector3(position.xMax, position.yMax));
            Handles.color = oldColor;
            Handles.EndGUI();
            EditorGUIUtility.AddCursorRect(position, MouseCursor.Link);
            return GUI.Button(position, content, style);
        }
        private bool CheckAllWizardOptionsAreSetupCorrectly(bool showStormManagerPopup = true)
        {
            if (!StormWizardUtils.CheckWizardFinished)
            {
                return false;
            }

            int successCount = 0;
            for (int i = 0; i < m_allWizardSettings.Count; i++)
            {
                if (m_allWizardSettings[i].GetStatus() == StormSettingStatus.OK)
                {
                    successCount++;
                }
            }

            if (showStormManagerPopup)
            {
                if (successCount == m_allWizardSettings.Count && !m_ignoredOpeningStormManager)
                {
                    if (EditorUtility.DisplayDialog("Storm Is Ready To Use", "The setup and configuration are complete. \r\n\r\n Do you want to open the Storm Manager now to begin integrating it into your scene?", "Open Storm Manager", "No thanks"))
                    {
                        StormWizardUtils.CheckWizardFinished = false;
#if PW_STORM_PRESENT
                        OpenStormManagerWithReflection();
#endif
                        this.Close();
                        return true;
                    }
                    //else
                    //{
                    //    StormWizardUtils.CheckWizardFinished = false;
                    //    m_ignoredOpeningStormManager = true;
                    //    EditorUtility.DisplayDialog("Finding The Storm Manager", "You can open the manager at any time by going to Window/Procedural Worlds/Storm/Main Window...", "Ok Thanks");
                    //    return true;
                    //}
                }
            }
            StormWizardUtils.CheckWizardFinished = false;
            return false;
        }

        private void OpenStormManagerWithReflection()
        {
            var type = Type.GetType("ProceduralWorlds.Storm.StormEditor.StormManagerWindow,Assembly-CSharp-Editor");
            if (type != null && typeof(EditorWindow).IsAssignableFrom(type))
            {
                EditorWindow window = EditorWindow.GetWindow(type, false, "Storm Manager");
                Vector2 initialSize = new Vector2(600f, 450f);
                window.position = new Rect(new Vector2((Screen.currentResolution.width - initialSize.x) / 2f, (Screen.currentResolution.height - initialSize.y) / 2f), initialSize);
                window.Show();
            }
            else
            {
                Debug.LogError("Could not find the StormManagerWindow class to open it. You can normally open the Storm Manager via Window > Procedural Worlds > Storm > Storm Manager...");
            }
        }

        private bool CheckIfSetupIsComplete(bool showManagerDialog = true)
        {
            if (!CheckAllWizardOptionsAreSetupCorrectly(showManagerDialog))
            {
                if (EditorUtility.DisplayDialog("Storm Setup Issues", "We've noticed that Storm has not been fully configured/setup in your scene/project. Are you sure you still want to close the Storm Wizard?", "Yes", "No"))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return true;
            }
        }
#endregion


        /// <summary>
        /// Checks to see if we have any issues
        /// </summary>
        /// <returns></returns>
        public static bool CheckForSetupIssues(bool updateFoldoutState)
        {
            bool noIssueFound = true;
            for (int i = 0; i < m_allWizardSettings.Count; i++)
            {
                StormWizardOption stormWizardSetting = m_allWizardSettings[i];
                if (!stormWizardSetting.PerformCheck() && !stormWizardSetting.m_ignored)
                {
                    stormWizardSetting.m_foldedOut = true;
                    noIssueFound = false;
                }
                else
                {
                    if (updateFoldoutState)
                    {
                        stormWizardSetting.m_foldedOut = false;
                    }
                }
            }

            return noIssueFound;
        }
        #region Private Static Wizard Functions
        /// <summary>
        /// Adds the wizard option to the category
        /// </summary>
        /// <param name="option"></param>
        private static void AddToCategory(StormWizardOption option)
        {
            if (option != null)
            {
                SetupCategory(option).Options.Add(option);
            }
        }
        /// <summary>
        /// Sets yp the category and will create it if it does not exist so never returns null
        /// </summary>
        /// <param name="option"></param>
        /// <returns></returns>
        private static UICategory SetupCategory(StormWizardOption option)
        {
            if (string.IsNullOrEmpty(option.m_wizardCategory))
            {
                option.m_wizardCategory = "Other";
            }

            bool exists = false;
            int existID = -1;
            for (int i = 0; i < m_uiCategories.Count; i++)
            {
                if (m_uiCategories[i].CategoryName == option.m_wizardCategory)
                {
                    exists = true;
                    existID = i;
                    break;
                }
            }

            if (!exists)
            {
                m_uiCategories.Add(new UICategory { CategoryName = option.m_wizardCategory });
                existID = m_uiCategories.Count - 1;
            }

            return m_uiCategories[existID];
        }

        #endregion
        #region Public Static Wizard Functions

        /// <summary>
        /// Show Storm Wizard editor window
        /// </summary>
        [MenuItem("Window/" + PWConst.COMMON_MENU + "/Storm/Storm Installation Wizard...", false, 200)]
        public static void ShowStormWizard()
        {
            RefreshWizardSettings();
            StormWizard manager = EditorWindow.GetWindow<StormWizard>(false, "Storm Installation Wizard");
            //Manager can be null if the dependency package installation is started upon opening the manager window.
            if (manager != null)
            {
                Vector2 initialSize = new Vector2(660f, 725f);
                manager.position = new Rect(new Vector2(Screen.currentResolution.width / 2f - initialSize.x / 2f, Screen.currentResolution.height / 2f - initialSize.y / 2f), initialSize);
                manager.Show();
                m_wizardSettings.SetCanShowOnStartup(false);
            }
        }
        /// <summary>
        /// Static function that can be called to setup and show the wizard if either an issue is present or you want to force show
        /// </summary>
        /// <param name="checkShowWizard"></param>
        /// <param name="forceShowWizard"></param>
        public static void CheckShowWizard(bool checkShowWizard, bool forceShowWizard = false)
        {
            RefreshWizardSettings();

            if (!CheckForSetupIssues(true))
            {
                if (checkShowWizard)
                {
                    ShowStormWizard();
                    return;
                }
            }

            if (forceShowWizard)
            {
                ShowStormWizard();
            }
        }

        private static void RefreshWizardSettings()
        {
            if (m_wizardSettings == null)
            {
                m_wizardSettings = StormWizardUtils.GetStormWizardSettings();
            }

            if (m_wizardSettings == null)
            {
                //Cannot show wizard if settings are null
                return;
            }

            //Get all storm wizard options entries
            m_allWizardSettings.Clear();
            m_uiCategories.Clear();
            StormWizardOption.RefreshActivePipeline();
            string[] allSWOGUIDs = AssetDatabase.FindAssets("t:StormWizardOption", new string[1] { StormWizardUtils.GetStormSettingsDirectory() });
            for (int i = 0; i < allSWOGUIDs.Length; i++)
            {
                string guid = allSWOGUIDs[i];
                StormWizardOption sWSetting = (StormWizardOption)AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(guid), typeof(StormWizardOption));
                if (sWSetting.RelevantForRenderPipeline())
                {
                    m_allWizardSettings.Add(sWSetting);
                }
            }

            //Add GWS entries that might be missing
            List<Type> allSWOTypes = StormWizardUtils.GetInheritedTypes<StormWizardOption>();
            foreach (Type t in allSWOTypes)
            {
                var tField = t.GetProperty("m_autoCreateIfMissing");
                if (!m_allWizardSettings.Any(x => x.GetType() == t))
                {
                    var newInstance = ScriptableObject.CreateInstance(t);
                    if ((bool)tField.GetMethod.Invoke(newInstance, new object[0]))
                    {
                        AssetDatabase.CreateAsset(newInstance, StormWizardUtils.GetStormSettingsDirectory() + "/" + t.Name + ".asset");
                        m_allWizardSettings.Add((StormWizardOption)newInstance);
                    }
                }
            }

            for (int i = 0; i < m_allWizardSettings.Count; i++)
            {
                for (int x = i + 1; x < m_allWizardSettings.Count; x++)
                {
                    if (m_allWizardSettings[x].GetType() == m_allWizardSettings[i].GetType())
                    {
                        m_allWizardSettings.RemoveAt(x--);
                    }
                }
                AddToCategory(m_allWizardSettings[i]);
            }
        }

        #endregion
    }

    /// <summary>
    /// Storm wizard option category used to sort and organize the display wizard options
    /// </summary>
    [Serializable]
    public class UICategory
    {
        public string CategoryName;
        public List<StormWizardOption> Options = new List<StormWizardOption>();
    }
}