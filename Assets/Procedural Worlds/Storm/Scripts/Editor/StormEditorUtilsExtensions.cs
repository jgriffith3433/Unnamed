// Copyright © 2018 Procedural Worlds Pty Limited.  All Rights Reserved.
using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using PWCommon5;
using PWCommon5.Localization;
using UnityEngine.Networking;
using System.Linq;

namespace ProceduralWorlds.Storm.StormEditor
{
    public static class StormEditorUtilsExtensions
    {


        /// <summary>
        /// Create a Panel and return its status (true = open, false = closed)
        /// </summary>
        /// <param name="nameKey">Language pack key of the text label and help text of the panel.</param>
        /// <param name="contentMethod">Pass in the method that draws the content of this tab</param>
        /// <param name="ignoreGuiChange">If true panel status and help switch changes will be ignored for GUI change. Note: this is not consistent with Unity GUI behaviour and should be avoided.</param>
        /// <param name="IsExpanded">Should the panel be opened or closed by default?</param>
        /// <param name="IsHelpActive">Should help be displayed?</param>
        /// <param name="options">An option list of layout options that specify extra layouting properties of the panel.
        /// Any values passed in here will override settings defined by the sytle.
        /// See Aslo: GUILayout.Width, GUILayout.Height, GUILayout.MinWidth, GUILayout.MaxWidth,
        /// GUILayout.MinHeight, GUILayout.MaxHeight, GUILayout.ExpandWidth, GUILayout.ExpandHeight</param>
        /// <returns>Panel status (true = open, false = closed).</returns>
        public static bool Panel(this EditorUtils m_editorUtils, string nameKey, Action<bool> contentMethod, bool ignoreGuiChange, ref bool IsExpanded, ref bool IsHelpActive, params GUILayoutOption[] options)
        {
            return m_editorUtils.Panel(m_editorUtils.GetContent(nameKey), nameKey, contentMethod, ignoreGuiChange, ref IsExpanded, ref IsHelpActive, options);
        }

        /// <summary>
        /// Create a Panel and return its status (true = open, false = closed)
        /// </summary>
        /// <param name="nameKey">Language pack key of the text label and help text of the panel.</param>
        /// <param name="contentMethod">Pass in the method that draws the content of this tab</param>
        /// <param name="labelStyle">Custom label style (optional)</param>
        /// <param name="ignoreGuiChange">If true panel status and help switch changes will be ignored for GUI change. Note: this is not consistent with Unity GUI behaviour and should be avoided.</param>
        /// <param name="IsExpanded">Should the panel be opened or closed by default?</param>
        /// <param name="IsHelpActive">Should help be displayed?</param>
        /// <param name="options">An option list of layout options that specify extra layouting properties of the panel.
        /// Any values passed in here will override settings defined by the sytle.
        /// See Aslo: GUILayout.Width, GUILayout.Height, GUILayout.MinWidth, GUILayout.MaxWidth,
        /// GUILayout.MinHeight, GUILayout.MaxHeight, GUILayout.ExpandWidth, GUILayout.ExpandHeight</param>
        /// <returns>Panel status (true = open, false = closed).</returns>
        public static bool Panel(this EditorUtils m_editorUtils, string nameKey, Action<bool> contentMethod, GUIStyle labelStyle, bool ignoreGuiChange, ref bool IsExpanded, ref bool IsHelpActive, params GUILayoutOption[] options)
        {
            return m_editorUtils.Panel(m_editorUtils.GetContent(nameKey), nameKey, contentMethod, labelStyle, ignoreGuiChange, ref IsExpanded, ref IsHelpActive, options);
        }

        /// <summary>
        /// Create a Panel with custom label (not localized text, texture, etc) and return its status (true = open, false = closed)
        /// </summary>
        /// <param name="panelLabel">Label content to be displayed for the panel.</param>
        /// <param name="helpKey">Language pack key of the help text to be displayed for the panel (when help is active).</param>
        /// <param name="contentMethod">Pass in the method that draws the content of this tab</param>
        /// <param name="ignoreGuiChange">If true panel status and help switch changes will be ignored for GUI change. Note: this is not consistent with Unity GUI behaviour and should be avoided.</param>
        /// <param name="IsExpanded">Should the panel be opened or closed by default?</param>
        /// <param name="IsHelpActive">Should help be displayed?</param>
        /// <param name="options">An option list of layout options that specify extra layouting properties of the panel.
        /// Any values passed in here will override settings defined by the sytle.
        /// See Aslo: GUILayout.Width, GUILayout.Height, GUILayout.MinWidth, GUILayout.MaxWidth,
        /// GUILayout.MinHeight, GUILayout.MaxHeight, GUILayout.ExpandWidth, GUILayout.ExpandHeight</param>
        /// <returns>Panel status (true = open, false = closed).</returns>
        public static bool Panel(this EditorUtils m_editorUtils, GUIContent panelLabel, string helpKey, Action<bool> contentMethod, bool ignoreGuiChange, ref bool IsExpanded, ref bool IsHelpActive, params GUILayoutOption[] options)
        {
            return m_editorUtils.Panel(panelLabel, helpKey, contentMethod, m_editorUtils.Styles.panelLabel, ignoreGuiChange, ref IsExpanded, ref IsHelpActive, options);
        }

        /// <summary>
        /// Create a Panel and return its status (true = open, false = closed)
        /// </summary>
        /// <param name="nameKey">Language pack key of the text label and help text of the panel.</param>
        /// <param name="contentMethod">Pass in the method that draws the content of this tab</param>
        /// <param name="IsExpanded">Should the panel be opened or closed by default?</param>
        /// <param name="IsHelpActive">Should help be displayed?</param>
        /// <param name="options">An option list of layout options that specify extra layouting properties of the panel.
        /// Any values passed in here will override settings defined by the sytle.
        /// See Aslo: GUILayout.Width, GUILayout.Height, GUILayout.MinWidth, GUILayout.MaxWidth,
        /// GUILayout.MinHeight, GUILayout.MaxHeight, GUILayout.ExpandWidth, GUILayout.ExpandHeight</param>
        /// <returns>Panel status (true = open, false = closed).</returns>
        public static bool Panel(this EditorUtils m_editorUtils, string nameKey, Action<bool> contentMethod, ref bool IsExpanded, ref bool IsHelpActive, params GUILayoutOption[] options)
        {
            return m_editorUtils.Panel(m_editorUtils.GetContent(nameKey), nameKey, contentMethod, false, ref IsExpanded, ref IsHelpActive, options);
        }

        /// <summary>
        /// Create a Panel and return its status (true = open, false = closed)
        /// </summary>
        /// <param name="nameKey">Language pack key of the text label and help text of the panel.</param>
        /// <param name="contentMethod">Pass in the method that draws the content of this tab</param>
        /// <param name="labelStyle">Custom label style (optional)</param>
        /// <param name="IsExpanded">Should the panel be opened or closed by default?</param>
        /// <param name="IsHelpActive">Should help be displayed?</param>
        /// <param name="options">An option list of layout options that specify extra layouting properties of the panel.
        /// Any values passed in here will override settings defined by the sytle.
        /// See Aslo: GUILayout.Width, GUILayout.Height, GUILayout.MinWidth, GUILayout.MaxWidth,
        /// GUILayout.MinHeight, GUILayout.MaxHeight, GUILayout.ExpandWidth, GUILayout.ExpandHeight</param>
        /// <returns>Panel status (true = open, false = closed).</returns>
        public static bool Panel(this EditorUtils m_editorUtils, string nameKey, Action<bool> contentMethod, GUIStyle labelStyle, ref bool IsExpanded, ref bool IsHelpActive, params GUILayoutOption[] options)
        {
            return m_editorUtils.Panel(m_editorUtils.GetContent(nameKey), nameKey, contentMethod, labelStyle, ref IsExpanded, ref IsHelpActive, options);
        }

        /// <summary>
        /// Create a Panel with custom label (not localized text, texture, etc) and return its status (true = open, false = closed)
        /// </summary>
        /// <param name="panelLabel">Label content to be displayed for the panel.</param>
        /// <param name="helpKey">Language pack key of the help text to be displayed for the panel (when help is active).</param>
        /// <param name="contentMethod">Pass in the method that draws the content of this tab</param>
        /// <param name="IsExpanded">Should the panel be opened or closed by default?</param>
        /// <param name="IsHelpActive">Should help be displayed?</param>
        /// <param name="options">An option list of layout options that specify extra layouting properties of the panel.
        /// Any values passed in here will override settings defined by the sytle.
        /// See Aslo: GUILayout.Width, GUILayout.Height, GUILayout.MinWidth, GUILayout.MaxWidth,
        /// GUILayout.MinHeight, GUILayout.MaxHeight, GUILayout.ExpandWidth, GUILayout.ExpandHeight</param>
        /// <returns>Panel status (true = open, false = closed).</returns>
        public static bool Panel(this EditorUtils m_editorUtils, GUIContent panelLabel, string helpKey, Action<bool> contentMethod, ref bool IsExpanded, ref bool IsHelpActive, params GUILayoutOption[] options)
        {
            return m_editorUtils.Panel(panelLabel, helpKey, contentMethod, m_editorUtils.Styles.panelLabel, ref IsExpanded, ref IsHelpActive, options);
        }

        /// <summary>
        /// Create a Panel with custom label (not localized text, texture, etc) and return its status (true = open, false = closed)
        /// </summary>
        /// <param name="panelLabel">Label content to be displayed for the panel.</param>
        /// <param name="helpKey">Language pack key of the help text to be displayed for the panel (when help is active).</param>
        /// <param name="contentMethod">Pass in the method that draws the content of this tab</param>
        /// <param name="labelStyle">Custom label style (optional)</param>
        /// <param name="IsExpanded">Should the panel be opened or closed by default?</param>
        /// <param name="IsHelpActive">Should help be displayed?</param>
        /// <param name="options">An option list of layout options that specify extra layouting properties of the panel.
        /// Any values passed in here will override settings defined by the sytle.
        /// See Aslo: GUILayout.Width, GUILayout.Height, GUILayout.MinWidth, GUILayout.MaxWidth,
        /// GUILayout.MinHeight, GUILayout.MaxHeight, GUILayout.ExpandWidth, GUILayout.ExpandHeight</param>
        /// <returns>Panel status (true = open, false = closed).</returns>
        public static bool Panel(this EditorUtils m_editorUtils, GUIContent panelLabel, string helpKey, Action<bool> contentMethod, GUIStyle labelStyle, ref bool IsExpanded, ref bool IsHelpActive, params GUILayoutOption[] options)
        {
            return m_editorUtils.Panel(panelLabel, helpKey, contentMethod, m_editorUtils.Styles.panelLabel, false, ref IsExpanded, ref IsHelpActive, options);
        }

        /// <summary>
        /// Create a Panel with custom label (not localized text, texture, etc) and return its status (true = open, false = closed)
        /// </summary>
        /// <param name="panelLabel">Label content to be displayed for the panel.</param>
        /// <param name="contentMethod">Pass in the method that draws the content of this tab</param>
        /// <param name="IsExpanded">Should the panel be opened or closed by default?</param>
        /// <param name="IsHelpActive">Should help be displayed?</param>
        /// <param name="options">An option list of layout options that specify extra layouting properties of the panel.
        /// Any values passed in here will override settings defined by the sytle.
        /// See Aslo: GUILayout.Width, GUILayout.Height, GUILayout.MinWidth, GUILayout.MaxWidth,
        /// GUILayout.MinHeight, GUILayout.MaxHeight, GUILayout.ExpandWidth, GUILayout.ExpandHeight</param>
        /// <returns>Panel status (true = open, false = closed).</returns>
        public static bool Panel(this EditorUtils m_editorUtils, GUIContent panelLabel, Action<bool> contentMethod, ref bool IsExpanded, ref bool IsHelpActive, params GUILayoutOption[] options)
        {
            return m_editorUtils.Panel(panelLabel, null, contentMethod, m_editorUtils.Styles.panelLabel, ref IsExpanded, ref IsHelpActive, options);
        }

        /// <summary>
        /// Create a Panel with custom label (not localized text, texture, etc) and return its status (true = open, false = closed)
        /// </summary>
        /// <param name="panelLabel">Label content to be displayed for the panel.</param>
        /// <param name="helpKey">Language pack key of the help text to be displayed for the panel (when help is active).</param>
        /// <param name="contentMethod">Pass in the method that draws the content of this tab</param>
        /// <param name="labelStyle">Custom label style (optional)</param>
        /// <param name="ignoreGuiChange">If true panel status and help switch changes will be ignored for GUI change. Note: this is not consistent with Unity GUI behaviour and should be avoided.</param>
        /// <param name="IsExpanded">Should the panel be opened or closed by default?</param>
        /// <param name="IsHelpActive">Should help be displayed?</param>
        /// <param name="options">An option list of layout options that specify extra layouting properties of the panel.
        /// Any values passed in here will override settings defined by the sytle.
        /// See Aslo: GUILayout.Width, GUILayout.Height, GUILayout.MinWidth, GUILayout.MaxWidth,
        /// GUILayout.MinHeight, GUILayout.MaxHeight, GUILayout.ExpandWidth, GUILayout.ExpandHeight</param>
        /// <returns>Panel status (true = open, false = closed).</returns>
        public static bool Panel(this EditorUtils m_editorUtils, GUIContent panelLabel, string helpKey, Action<bool> contentMethod, GUIStyle labelStyle, bool ignoreGuiChange, ref bool IsExpanded, ref bool IsHelpActive, params GUILayoutOption[] options)
        {
            GUILayout.BeginVertical(m_editorUtils.Styles.panelFrame, options);
            {
                bool guiChaned = GUI.changed;

                GUILayout.BeginHorizontal();
                {
                    IsExpanded = GUILayout.Toggle(IsExpanded, IsExpanded ? "-" : "+", m_editorUtils.Styles.panelLabel);
                    GUILayout.Space(-5f);
                    IsExpanded = GUILayout.Toggle(IsExpanded, panelLabel, labelStyle);
                    GUILayout.FlexibleSpace();
                    m_editorUtils.HelpToggle(ref IsHelpActive);
                }
                GUILayout.EndHorizontal();
                if (IsHelpActive && !string.IsNullOrWhiteSpace(helpKey))
                {
                    GUILayout.Space(2f);
                    m_editorUtils.InlineHelp(helpKey, IsHelpActive);
                }

                if (ignoreGuiChange)
                {
                    GUI.changed = guiChaned;
                }

                if (IsExpanded)
                {
                    GUILayout.BeginVertical(m_editorUtils.Styles.panel);
                    {
                        contentMethod.Invoke(IsHelpActive);
                    }
                    GUILayout.EndVertical();
                }
            }
            GUILayout.EndVertical();
            return IsExpanded;
        }
    }
}