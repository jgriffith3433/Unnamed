using UnityEditor;
using ProceduralWorlds.Storm.StormEditor;
using PWCommon5;

namespace ProceduralWorlds.Storm.AreaPopulator
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(PlacementFilter), true)] // Enables inheritance
    public class PlacementFilterEditor : PWEditor
    {
        public bool m_defaultOpenPanels = false;

        // Panel expansion state properties
        protected bool placementFilterPanelExpanded { get { return m_PlacementFilterInstance.placementFilterPanelExpanded; } set { m_PlacementFilterInstance.placementFilterPanelExpanded = value; } }

        protected EditorUtils m_editorUtils;

        // Serialized Properties from PlacementFilter
        protected SerializedProperty p_IdealPlacementLayer;
        protected PlacementFilter m_PlacementFilterInstance;

        protected virtual void OnEnable()
        {
            // Initialize the instance first to ensure it's available for property accessors
            m_PlacementFilterInstance = (PlacementFilter)target;
            
            // Initialize PlacementFilter properties
            p_IdealPlacementLayer = serializedObject.FindProperty("idealPlacementLayer");

            Undo.undoRedoPerformed -= OnUndo;
            Undo.undoRedoPerformed += OnUndo;
        }

        protected virtual void OnDisable()
        {
            Undo.undoRedoPerformed -= OnUndo;
        }

        protected virtual void OnDestroy()
        {
            OnDisable();
        }

        public override void OnInspectorGUI()
        {
            if (m_editorUtils == null)
            {
                m_editorUtils = StormApp.GetEditorUtils(this);
            }
            m_PlacementFilterInstance = (PlacementFilter)target;

            m_editorUtils.Initialize();

            if(m_defaultOpenPanels)
            {
                placementFilterPanelExpanded = m_editorUtils.Panel("PlacementFilterPanel", PlacementFilterPanel, m_defaultOpenPanels);
            }
            else
            {
                placementFilterPanelExpanded = m_editorUtils.Panel("PlacementFilterPanel", PlacementFilterPanel, placementFilterPanelExpanded);
            }

            serializedObject.ApplyModifiedProperties();
        }

        protected virtual void PlacementFilterPanel(bool helpActive)
        {
            PropField("IdealPlacementLayer", p_IdealPlacementLayer, helpActive);
        }

        protected void PropField(string key, SerializedProperty prop, bool helpActive)
        {
            EditorGUI.showMixedValue = prop.hasMultipleDifferentValues;
            m_editorUtils.PropertyField(key, true, prop, helpActive);
            EditorGUI.showMixedValue = false;
        }

        protected void OnUndo()
        {
            serializedObject.Update();
            serializedObject.ApplyModifiedProperties();
        }
    }
}
