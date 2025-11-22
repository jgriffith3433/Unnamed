using System;
using UnityEngine;
using System.Collections.Generic;
#if PW_STORM_PRESENT
using ProceduralWorlds.Storm.Utils;
#endif
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace ProceduralWorlds.Storm.Wizard
{
    #region Enums

    public enum StormSettingStatus { Unknown, OK, Info, Warning, Error }
    public enum StormPackageResult { Unknown, Failed, Successful }
    public enum WizardOptionMode { Info, Dropdown }

    #endregion

    [Serializable]
    public class StormWizardOption : ScriptableObject
    {
        #region Variables
        public virtual bool m_autoCreateIfMissing { get; } = true;
        public bool m_foldedOut;
        public bool m_wasFixed;
        public bool m_ignored = false;
        public string m_wizardCategory;
        public bool m_installationOption;
        public int m_dropdownID;

        internal string m_name;
        internal StormSettingStatus m_status = StormSettingStatus.Unknown;
        internal bool m_RPBuiltIn;
        internal bool m_RPHDRP;
        internal bool m_RPURP;
        internal bool m_canRestore = false;
        internal bool m_canFix = true;
        internal bool m_canIgnore = true;
        internal string m_infoTextOK;
        internal string m_infoTextIssue;
        internal string m_link = "";
        internal string m_linkDisplayText = "";
        internal bool m_isPackageManagerRequired;
        internal StormPackageResult m_packageResult = StormPackageResult.Unknown;
        internal WizardOptionMode m_wizardOptionMode = WizardOptionMode.Info;
        internal List<string> m_dropdownOptions = new List<string>();
        internal string m_dropdownToolTip;

        private GUIStyle m_linkStyle;
        private GUIContent m_linkContent;
        private static GUIContent m_iconOKContent;
        private static GUIContent m_iconWarningContent;
        private static GUIContent m_iconErrorContent;

        private static StormWizardRP m_activePipeline;

        #endregion
        #region Events

        public delegate void OnDropdownChanged();
        public static OnDropdownChanged onDropdownChanged;

        #endregion
        #region Static Wizard Settings

        private static StormWizardSettings m_stormWizardSettings;
        private static StormWizardSettings StormWizardSettings
        {
            get
            {
                if (m_stormWizardSettings == null)
                {
                    m_stormWizardSettings = StormWizardUtils.GetStormWizardSettings();
                }
                return m_stormWizardSettings;
            }
        }

        #endregion
        #region Virtual Overrides

        /// <summary>
        /// Initializes this wizard option
        /// </summary>
        public virtual void Initialize()
        {
            if (!string.IsNullOrEmpty(m_link))
            {
                if (!string.IsNullOrEmpty(m_linkDisplayText))
                {
                    m_linkContent = new GUIContent(m_linkDisplayText, m_link);
                }
                else
                {
                    m_linkContent = new GUIContent(m_link, m_link);
                }
            }

            if (m_iconOKContent == null && StormWizardSettings != null)
            {
                m_iconOKContent = new GUIContent(StormWizardSettings.IconOK, "OK - This setting is configured correctly");
            }

            if (m_iconWarningContent == null && StormWizardSettings != null)
            {
                m_iconWarningContent = new GUIContent(StormWizardSettings.IconWarning, "Warning - This setting might cause issues with Storm");
            }

            if (m_iconErrorContent == null && StormWizardSettings != null)
            {
                m_iconErrorContent = new GUIContent(StormWizardSettings.IconError, "ERROR - This setting will cause severe issues / conflicts with Storm");
            }
        }
        /// <summary>
        /// Checks whether the setting is in an appropiate state for using Storm
        /// </summary>
        /// <returns></returns>
        public virtual bool PerformCheck()
        {
            return true;
        }
        /// <summary>
        /// Fixes the issue within the wizard
        /// </summary>
        /// <param name="autoFix"></param>
        /// <returns></returns>
        public virtual bool FixNow(bool autoFix = false)
        {
            return true;
        }
        /// <summary>
        /// Restores the original value for the setting before the fix was applied
        /// </summary>
        /// <returns></returns>
        public virtual bool RestoreOriginalValue()
        {
            return true;
        }
        /// <summary>
        /// Gets a string representation of the original value that was stored when the fix button was pressed
        /// </summary>
        /// <returns></returns>
        public virtual string GetOriginalValueString()
        {
            return "Unknown";
        }

        #endregion
        #region Public UI functions

        /// <summary>
        /// Ignores the setting so that it is not constantly highlighted as a problem in the UI
        /// </summary>
        public void ToggleIgnore()
        {
            m_ignored = !m_ignored;
            if (m_ignored)
            {
                m_foldedOut = false;
            }
        }
        /// <summary>
        /// Draws the UI for this entry in the Setup tab
        /// </summary>
        public void DrawUI()
        {
            if (m_linkStyle == null)
            {
                m_linkStyle = new GUIStyle(GUI.skin.label);
                m_linkStyle.fontStyle = FontStyle.Normal;
                m_linkStyle.wordWrap = false;
                m_linkStyle.normal.textColor = new Color(0x00 / 255f, 0x78 / 255f, 0xDA / 255f, 1f);
                m_linkStyle.stretchWidth = false;
            }

#if UNITY_EDITOR
            switch (m_wizardOptionMode)
            {
                case WizardOptionMode.Info:
                    {
                        InfoWizardMode();
                        break;
                    }
                case WizardOptionMode.Dropdown:
                    {
                        DropdownWizardMode();
                        break;
                    }
            }
            EditorGUILayout.Space(3f);
#endif
        }

        #endregion
        #region Private UI Functions

        /// <summary>
        /// Displays the default info wizard option this has title, info and fix/ignore button
        /// </summary>
        private void InfoWizardMode()
        {
#if UNITY_EDITOR
            if (m_iconOKContent == null || m_iconErrorContent == null || m_iconWarningContent == null)
            {
                Initialize();
            }

            EditorGUILayout.BeginHorizontal();
            if (m_status == StormSettingStatus.OK || m_ignored)
            {
                GUILayout.Label(m_iconOKContent);
            }
            else if (m_status == StormSettingStatus.Error)
            {
                GUILayout.Label(m_iconErrorContent);
            }
            else if (m_status == StormSettingStatus.Warning)
            {
                GUILayout.Label(m_iconWarningContent);
            }
            GUILayout.Space(5);

            m_foldedOut = EditorGUILayout.Foldout(m_foldedOut, m_name, true);
            GUILayout.FlexibleSpace();
            if ((m_status == StormSettingStatus.Error || m_status == StormSettingStatus.Warning) && !m_ignored && m_canIgnore)
            {
                if (GUILayout.Button(new GUIContent("Ignore", "If ignored this issue will no longer be checked."), GUILayout.Width(50)))
                {
                    ToggleIgnore();
                }
            }

            if ((m_status == StormSettingStatus.Error || m_status == StormSettingStatus.Warning) && !m_ignored && m_canFix)
            {
                if (m_installationOption)
                {
                    if (StormWizardUtils.StormCorePackageExists())
                    {
                        if (GUILayout.Button(new GUIContent("Install", "This will install / update the storm core package."), GUILayout.Width(50)))
                        {
                            FixNow();
                        }
                    }
                    else
                    {
                        if (GUILayout.Button(new GUIContent("Fix", "This will resolve the current issue."), GUILayout.Width(50)))
                        {
                            FixNow();
                        }
                    }
                }
                else
                {
                    if (GUILayout.Button(new GUIContent("Fix", "This will resolve the current issue."), GUILayout.Width(50)))
                    {
                        FixNow();
                    }
                }
            }
            EditorGUILayout.EndHorizontal();

            if (m_foldedOut)
            {
                EditorGUI.indentLevel += 3;
                if (m_status == StormSettingStatus.OK)
                {
                    EditorGUILayout.HelpBox(m_infoTextOK, MessageType.Info);
                }
                if (m_status == StormSettingStatus.Error)
                {
                    EditorGUILayout.HelpBox(m_infoTextIssue, MessageType.Error);
                }
                if (m_status == StormSettingStatus.Warning)
                {
                    EditorGUILayout.HelpBox(m_infoTextIssue, MessageType.Warning);
                }

                if (m_ignored)
                {
                    EditorGUILayout.HelpBox("You are currently ignoring this issue.", MessageType.Info);
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(45);
                    if (GUILayout.Button(new GUIContent("Stop Ignoring", "This will stop ignoring this issue and allow the wizard to check for this issue in the future.")))
                    {
                        ToggleIgnore();
                    }
                    EditorGUILayout.EndHorizontal();
                }

                if (m_status == StormSettingStatus.OK && m_wasFixed && m_canRestore)
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(45);
                    GUILayout.Label($"Original Value: {GetOriginalValueString()}");

                    GUILayout.Space(45);
                    if (GUILayout.Button(new GUIContent("Restore", "Restores the original value of this setting.")))
                    {
                        RestoreOriginalValue();
                    }
                    EditorGUILayout.EndHorizontal();
                }

                if (!string.IsNullOrEmpty(m_link))
                {
                    if (m_linkStyle == null || m_linkContent == null)
                    {
                        Initialize();
                    }

                    EditorGUILayout.LabelField("Please find more information by following this link:");
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(45);
                    if (ClickableHeaderCustomStyle(m_linkContent, m_linkStyle))
                    {
                        Application.OpenURL(m_link);
                    }
                    EditorGUILayout.EndHorizontal();
                }
                EditorGUI.indentLevel -= 3;
            }
#endif
        }
        /// <summary>
        /// Displays the dropdown wizard option that has a title, info and dropdown plus ignore button
        /// </summary>
        private void DropdownWizardMode()
        {
#if UNITY_EDITOR
            EditorGUILayout.BeginHorizontal();
            if (m_status == StormSettingStatus.OK || m_ignored)
            {
                GUILayout.Label(m_iconOKContent);
            }
            else if (m_status == StormSettingStatus.Error)
            {
                GUILayout.Label(m_iconErrorContent);
            }
            else if (m_status == StormSettingStatus.Warning)
            {
                GUILayout.Label(m_iconWarningContent);
            }
            GUILayout.Space(5);

            EditorGUILayout.BeginHorizontal();
            m_foldedOut = EditorGUILayout.Foldout(m_foldedOut, m_name, true);
            GUILayout.FlexibleSpace();
            if (m_dropdownOptions.Count > 0)
            {
                int id = m_dropdownID;
                id = EditorGUILayout.Popup(new GUIContent("", m_dropdownToolTip), id, m_dropdownOptions.ToArray(), GUILayout.Width(103));
                if (id != m_dropdownID)
                {
                    m_dropdownID = id;
                    onDropdownChanged?.Invoke();
                }
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndHorizontal();

            if (m_foldedOut)
            {
                EditorGUI.indentLevel += 3;
                if (m_status == StormSettingStatus.OK)
                {
                    EditorGUILayout.HelpBox(m_infoTextOK, MessageType.Info);
                }
                if (m_status == StormSettingStatus.Error)
                {
                    EditorGUILayout.HelpBox(m_infoTextIssue, MessageType.Error);
                }
                if (m_status == StormSettingStatus.Warning)
                {
                    EditorGUILayout.HelpBox(m_infoTextIssue, MessageType.Warning);
                }

                if (m_ignored)
                {
                    EditorGUILayout.HelpBox("You are currently ignoring this issue.", MessageType.Info);
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(45);
                    if (GUILayout.Button(new GUIContent("Stop Ignoring", "This will stop ignoring this issue and allow the wizard to check for this issue in the future.")))
                    {
                        ToggleIgnore();
                    }
                    EditorGUILayout.EndHorizontal();
                }

                if (m_status == StormSettingStatus.OK && m_wasFixed && m_canRestore)
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(45);
                    GUILayout.Label($"Original Value: {GetOriginalValueString()}");

                    GUILayout.Space(45);
                    if (GUILayout.Button(new GUIContent("Restore", "Restores the original value of this setting.")))
                    {
                        RestoreOriginalValue();
                    }
                    EditorGUILayout.EndHorizontal();
                }

                if (!string.IsNullOrEmpty(m_link))
                {
                    if (m_linkStyle == null || m_linkContent == null)
                    {
                        Initialize();
                    }

                    EditorGUILayout.LabelField("Please find more information by following this link:");
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(45);
                    if (ClickableHeaderCustomStyle(m_linkContent, m_linkStyle))
                    {
                        Application.OpenURL(m_link);
                    }
                    EditorGUILayout.EndHorizontal();
                }
                EditorGUI.indentLevel -= 3;
            }
#endif
        }

        #endregion
        #region Render Pipeline Functions

        /// <summary>
        /// Refreshes the active render pipeline
        /// </summary>
        public static void RefreshActivePipeline()
        {
            m_activePipeline = StormWizardUtils.GetActivePipeline();
        }
        /// <summary>
        /// Should this wizard option be displayed for the current render pipeline
        /// </summary>
        /// <returns></returns>
        public bool RelevantForRenderPipeline()
        {

            switch (m_activePipeline)
            {
                case StormWizardRP.Builtin:
                    if (m_RPBuiltIn)
                        return true;
                    break;
                case StormWizardRP.URP:
                    if (m_RPURP)
                        return true;
                    break;
                case StormWizardRP.HDRP:
                    if (m_RPHDRP)
                        return true;
                    break;
            }
            return true;
        }

        /// <summary>
        /// Sets the target render tipeline to the active render pipeline
        /// </summary>
        public void SetTargetRP()
        {
            RefreshActivePipeline();
            StormWizardSettings.SetTargetRenderPipeline(m_activePipeline);
#if UNITY_EDITOR
            EditorUtility.SetDirty(StormWizardSettings);
#endif
        }
        /// <summary>
        /// Sets the target render piepline to the passed in RP
        /// </summary>
        /// <param name="rp"></param>
        public void SetTargetRP(StormWizardRP rp)
        {
            RefreshActivePipeline();
            StormWizardSettings.SetTargetRenderPipeline(rp);
#if UNITY_EDITOR
            EditorUtility.SetDirty(StormWizardSettings);
#endif
        }

        /// <summary>
        /// Gets the active render pipeline
        /// </summary>
        /// <returns></returns>
        public StormWizardRP GetActiveRP()
        {
            return m_activePipeline;
        }
        /// <summary>
        /// Gets the targeted render pipeline in the wizard settings
        /// </summary>
        /// <returns></returns>
        public StormWizardRP GetTargetRP()
        {
            return StormWizardSettings.GetTargetRenderPipeline();
        }

        #endregion
        #region Wizard Util Functions

        /// <summary>
        /// Sets the quality ID
        /// </summary>
        public void ChangeWizardQualityID(int value)
        {
            StormWizardSettings.RenderQualityID = value;
#if UNITY_EDITOR
            EditorUtility.SetDirty(StormWizardSettings);
#endif
        }
        /// <summary>
        /// Gets the current render quality ID
        /// </summary>
        /// <returns></returns>
        public int GetWizardQualityID()
        {
            return StormWizardSettings.RenderQualityID;
        }
        /// <summary>
        /// Returns the current status of this option
        /// </summary>
        /// <returns></returns>
        public StormSettingStatus GetStatus()
        {
            return m_status;
        }
        /// <summary>
        /// Gets the storms render qualities as a list of string
        /// </summary>
        /// <returns></returns>
        public List<string> GetStormRenderQualites()
        {
            List<string> qualities = new List<string>();
#if PW_STORM_PRESENT
            for (int i = 0; i < StormWizardSettings.RenderQualities.Count; i++)
            {
                StormQualitySettings stormWorldSettings = StormWizardSettings.RenderQualities[i] as StormQualitySettings;
                if (stormWorldSettings == null)
                    continue;
                string qualityName = stormWorldSettings.name;
                qualityName = qualityName.Replace("StormDefaultQualitySettings ", "");
                qualities.Add(qualityName);
            }
#endif
            return qualities;
        }
        /// <summary>
        /// Gets the dropdown value
        /// </summary>
        /// <returns></returns>
        public int GetDropdownValue()
        {
            return m_dropdownID;
        }

        #endregion
        #region UI Editor Functions

#if UNITY_EDITOR
        /// <summary>
        /// Creates a clickable header using a custom style (Good for links)
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
#endif

        #endregion
    }
}
