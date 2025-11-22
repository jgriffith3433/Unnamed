using ProceduralWorlds.Storm.Utils;
using PWCommon5;
using UnityEditor;

/// Copyright 2023 Procedural Worlds Ltd
/// Written by Jonathan Williams as part of Storm

namespace ProceduralWorlds.Storm.StormEditor
{
    [CustomEditor(typeof(StormLoaderData), true)]
    public class StormLoaderDataEditor : PWEditor, IPWEditor
    {
        SerializedProperty PositionProp;
        SerializedProperty SizeProp;
        SerializedProperty TilesListProp;

        TabSet m_tabs;
        EditorUtils m_editorUtils;

        private void OnEnable()
        {
            if (m_editorUtils == null)
                m_editorUtils = StormApp.GetEditorUtils(this);
            PositionProp = serializedObject.FindProperty("Position");
            SizeProp = serializedObject.FindProperty("Size");
            TilesListProp = serializedObject.FindProperty("Tiles");
            m_tabs = new TabSet(m_editorUtils, new Tab[] {
                new Tab("EditDataTab", EditDataTab),
                new Tab("VisualEditTab", VisualEditTab),
            });
        }
        public override void OnInspectorGUI()
        {
            m_editorUtils.Initialize();
            m_editorUtils.Tabs(m_tabs);
            serializedObject.ApplyModifiedProperties();
        }

        private void VisualEditTab()
        {
            StormEditorSettings editorSettings = StormEditorSettings.Instance;

        }
        private void EditDataTab()
        {
            m_editorUtils.PropertyField("mPosition", PositionProp);
            m_editorUtils.PropertyField("mSize", SizeProp);
            ++EditorGUI.indentLevel;
            m_editorUtils.PropertyField("mTiles", TilesListProp);
            --EditorGUI.indentLevel;
        }
    }
}