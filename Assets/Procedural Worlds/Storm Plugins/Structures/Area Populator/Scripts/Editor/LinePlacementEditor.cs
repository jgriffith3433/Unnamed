using UnityEditor;
using ProceduralWorlds.Storm.StormEditor;
using ProceduralWorlds.Storm.StructureGenCore;
using System.Collections.Generic;
using UnityEngine;
namespace ProceduralWorlds.Storm.AreaPopulator
{
    [CustomEditor(typeof(LinePlacement))]
    public class LinePlacementEditor : PlacementFilterEditor
    {
        // Panel expansion state properties
        protected bool linePlacementPanelExpanded { get { return m_LinePlacementInstance.linePlacementPanelExpanded; } set { m_LinePlacementInstance.linePlacementPanelExpanded = value; } }
        protected bool previewPanelExpanded { get { return m_LinePlacementInstance.previewPanelExpanded; } set { m_LinePlacementInstance.previewPanelExpanded = value; } }

        public Texture2D sourceImage;

        // Serialized Properties specific to LinePlacement
        SerializedProperty p_Jitter;
        SerializedProperty p_LineVector;
        SerializedProperty p_TileAlongMainAxis;
        SerializedProperty p_TileAlongSecondaryAxis;
        SerializedProperty p_TilingDistance;
        SerializedProperty p_FirstStepDistance;
        SerializedProperty p_CenterTileing;
        SerializedProperty p_UseMainAxisCountOveride;
        SerializedProperty p_UseSecondaryAxisOveride;
        SerializedProperty p_MainAxisCountOveride;
        SerializedProperty p_SecondaryAxisCountOveride;
        SerializedProperty p_AlignObjectsToLine;
        SerializedProperty p_AllowNullCellsInSpawns;
        SerializedProperty p_PreviewAreaSize;

        LinePlacement m_LinePlacementInstance = null;

        List<Vector2> spawnPointPolygon = new List<Vector2>() {new Vector2(0, 0.5f), new Vector2(-0.5f, -0.5f), new Vector2(0.5f, -0.5f)};

        List<Vector2> previewArea = new List<Vector2>();
        protected override void OnEnable()
        {
            base.OnEnable();

            // Initialize LinePlacement properties
            p_Jitter = serializedObject.FindProperty("jitter");
            p_LineVector = serializedObject.FindProperty("lineVector");
            p_TileAlongMainAxis = serializedObject.FindProperty("tileAlongMainAxis");
            p_TileAlongSecondaryAxis = serializedObject.FindProperty("tileAlongSecondaryAxis");
            p_TilingDistance = serializedObject.FindProperty("tilingDistance");
            p_FirstStepDistance = serializedObject.FindProperty("firstStepDistance");
            p_CenterTileing = serializedObject.FindProperty("centerTileing");
            p_UseMainAxisCountOveride = serializedObject.FindProperty("useMainAxisCountOveride");
            p_UseSecondaryAxisOveride = serializedObject.FindProperty("useSecondaryAxisOveride");
            p_MainAxisCountOveride = serializedObject.FindProperty("mainAxisCountOveride");
            p_SecondaryAxisCountOveride = serializedObject.FindProperty("secondaryAxisCountOveride");
            p_AlignObjectsToLine = serializedObject.FindProperty("alignObjectsToLine");
            p_AllowNullCellsInSpawns = serializedObject.FindProperty("allowNullCellsInSpawns");
            p_PreviewAreaSize = serializedObject.FindProperty("previewAreaSize");
            
            m_LinePlacementInstance = (LinePlacement)target;
            SetPreviewArea();
            UpdateImage(m_LinePlacementInstance);
        }

        public override void OnInspectorGUI()
        {
            if (m_editorUtils == null)
            {
                m_editorUtils = StormApp.GetEditorUtils(this);
            }
            m_LinePlacementInstance = (LinePlacement)target;

            m_editorUtils.Initialize();

            base.OnInspectorGUI(); // Draw the base PlacementFilter properties

            if(m_defaultOpenPanels)
            {
                linePlacementPanelExpanded = m_editorUtils.Panel("LinePlacementPanel", LinePlacementPanel, m_defaultOpenPanels);
                previewPanelExpanded = m_editorUtils.Panel("PreviewPanel", PreviewPanel, m_defaultOpenPanels);
            }
            else
            {
                linePlacementPanelExpanded = m_editorUtils.Panel("LinePlacementPanel", LinePlacementPanel, linePlacementPanelExpanded);
                previewPanelExpanded = m_editorUtils.Panel("PreviewPanel", PreviewPanel, previewPanelExpanded);
            }
            
            if (serializedObject.ApplyModifiedProperties())
            {
                SetPreviewArea();
                UpdateImage(m_LinePlacementInstance);
            }
        }

        private void LinePlacementPanel(bool helpActive)
        {
            PropField("Jitter", p_Jitter, helpActive);
            PropField("LineVector", p_LineVector, helpActive);
            PropField("TileAlongMainAxis", p_TileAlongMainAxis, helpActive);
            PropField("TileAlongSecondaryAxis", p_TileAlongSecondaryAxis, helpActive);

            EditorGUI.indentLevel++;
            PropField("TilingDistance", p_TilingDistance, helpActive);
            PropField("FirstStepDistance", p_FirstStepDistance, helpActive);
            EditorGUI.indentLevel--;

            PropField("CenterTileing", p_CenterTileing, helpActive);

            PropField("UseMainAxisCountOveride", p_UseMainAxisCountOveride, helpActive);
            if (m_LinePlacementInstance.useMainAxisCountOveride)
            {
                EditorGUI.indentLevel++;
                PropField("MainAxisCountOveride", p_MainAxisCountOveride, helpActive);
                EditorGUI.indentLevel--;
            }

            PropField("UseSecondaryAxisOveride", p_UseSecondaryAxisOveride, helpActive);
            if (m_LinePlacementInstance.useSecondaryAxisOveride)
            {
                EditorGUI.indentLevel++;
                PropField("SecondaryAxisCountOveride", p_SecondaryAxisCountOveride, helpActive);
                EditorGUI.indentLevel--;
            }

            PropField("AlignObjectsToLine", p_AlignObjectsToLine, helpActive);
            PropField("AllowNullCellsInSpawns", p_AllowNullCellsInSpawns, helpActive);

        }

        private void PreviewPanel(bool helpActive)
        {
            PropField("PreviewAreaSize", p_PreviewAreaSize, helpActive);
            
            GUILayout.Box(sourceImage);
        }

        
        public void UpdateImage(LinePlacement linePlacement)
        {
            ImageUtils.SetImageToColor(sourceImage, Color.black);

            Vector2 imageWidthHeight = new Vector2(10, 10);

            if (previewArea.Count > 2)
            {
                imageWidthHeight = StructureCreatorUtils.GetSizeOfPolygonBoundingBox(previewArea);
                imageWidthHeight = Mathf.Max(imageWidthHeight.x, imageWidthHeight.y) * Vector2.one;
            }
            ImageUtils.DrawPolygonOnImage(sourceImage, imageWidthHeight, previewArea, new Color(0.1568f, 0.1568f, 0.1568f));

            List<PlacementData> placementData = linePlacement.GetPreviewPlacementData(previewArea, Vector2.one);
            for (int i = 0; i < placementData.Count; i++)
            {
                PlacementData data = placementData[i];
                Vector2 spawnPos = new Vector2(data.spawnPoint.position.x, data.spawnPoint.position.z);
                List<Vector2> polygon = StructureCreatorUtils.AddPositionToPolygon(spawnPos, spawnPointPolygon);
                polygon = StructureCreatorUtils.RotatePolygon(polygon, spawnPos, -data.spawnPoint.rotation * Mathf.Deg2Rad);
                ImageUtils.DrawPolygonOnImage(sourceImage, imageWidthHeight, polygon, new Color(0.67f, 0.019f, 0.005f));
            }
        }

        public void SetPreviewArea()
        {
            previewArea = new List<Vector2>() { 
                new Vector2(0, m_LinePlacementInstance.previewAreaSize), 
                new Vector2(m_LinePlacementInstance.previewAreaSize, m_LinePlacementInstance.previewAreaSize), 
                new Vector2(m_LinePlacementInstance.previewAreaSize, 0),
                new Vector2(0, 0)};
        }
        
    }
}




