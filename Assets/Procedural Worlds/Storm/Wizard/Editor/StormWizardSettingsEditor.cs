using UnityEditor;
using UnityEngine;

namespace ProceduralWorlds.Storm.Wizard
{
    [CustomEditor(typeof(StormWizardSettings))]
    public class StormWizardSettingsEditor : Editor
    {
        private StormWizardSettings m_settings;
        private SerializedProperty m_renderQualities;

        private void OnEnable()
        {
            m_settings = (StormWizardSettings)target;
            m_renderQualities = serializedObject.FindProperty("m_renderQualities");
        }
        public override void OnInspectorGUI()
        {
            if (m_settings == null)
            {
                m_settings = (StormWizardSettings)target;
            }

            Texture IconOk = m_settings.IconOK;
            Texture IconWarning = m_settings.IconWarning;
            Texture IconError = m_settings.IconError;

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Wizard Settings", EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(m_renderQualities);
            IconOk = (Texture)EditorGUILayout.ObjectField("Icon Ok", IconOk, typeof(Texture), false, GUILayout.MaxHeight(16f));
            IconWarning = (Texture)EditorGUILayout.ObjectField("Icon Warning", IconWarning, typeof(Texture), false, GUILayout.MaxHeight(16f));
            IconError = (Texture)EditorGUILayout.ObjectField("Icon Error", IconError, typeof(Texture), false, GUILayout.MaxHeight(16f));
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(m_settings, "Wizard Settings Changed");
                m_settings.IconOK = IconOk;
                m_settings.IconWarning = IconWarning;
                m_settings.IconError = IconError;
                serializedObject.ApplyModifiedProperties();
            }
        }
    }
}