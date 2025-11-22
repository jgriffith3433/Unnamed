using System.Collections.Generic;
using UnityEditor;
using ProceduralWorlds.Storm.StormEditor;
using ProceduralWorlds.Storm.StructureGenCore;
using UnityEngine;

namespace ProceduralWorlds.Storm.AreaPopulator
{
    [CustomEditor(typeof(EdgePlacement))]
    public class EdgePlacementEditor : PlacementFilterEditor
    {
        // Panel expansion state properties
        protected bool edgePlacementPanelExpanded { get { return m_EdgePlacementInstance.edgePlacementPanelExpanded; } set { m_EdgePlacementInstance.edgePlacementPanelExpanded = value; } }
        protected bool previewPanelExpanded { get { return m_EdgePlacementInstance.previewPanelExpanded; } set { m_EdgePlacementInstance.previewPanelExpanded = value; } }

        public Texture2D sourceImage;

        // Serialized Properties specific to EdgePlacement

        SerializedProperty p_RandomMode;
        SerializedProperty p_TargetPlacements;
        SerializedProperty p_CenterAlongEdge;
        SerializedProperty p_TileAlongEdge;
        SerializedProperty p_ForwardOffset;
        SerializedProperty p_TilingOffset;
        SerializedProperty p_TilingStep;
        SerializedProperty p_UseDistanceFromCenter;
        SerializedProperty p_DistanceFromCenterRange;
        SerializedProperty p_DistanceFromCenterAxisType;
        SerializedProperty p_AllowNullCellsInSpawns;
        SerializedProperty p_PreviewAreaSize;

        EdgePlacement m_EdgePlacementInstance = null;

        List<Vector2> spawnPointPolygon = new List<Vector2>() {new Vector2(0, 0.5f), new Vector2(-0.5f, -0.5f), new Vector2(0.5f, -0.5f)};

        List<Vector2> previewArea = new List<Vector2>();

        protected override void OnEnable()
        {
            base.OnEnable();

            // Initialize EdgePlacement properties
            p_RandomMode = serializedObject.FindProperty("randomMode");
            p_TargetPlacements = serializedObject.FindProperty("targetPlacements");
            p_CenterAlongEdge = serializedObject.FindProperty("centerAlongEdge");
            p_TileAlongEdge = serializedObject.FindProperty("tileAlongEdge");
            p_ForwardOffset = serializedObject.FindProperty("forwardOffset");
            p_TilingOffset = serializedObject.FindProperty("tilingOffset");
            p_TilingStep = serializedObject.FindProperty("tilingStep");
            p_UseDistanceFromCenter = serializedObject.FindProperty("useDistanceFromCenter");
            p_DistanceFromCenterRange = serializedObject.FindProperty("distanceFromCenterRange");
            p_DistanceFromCenterAxisType = serializedObject.FindProperty("distanceFromCenterAxisType");
            p_AllowNullCellsInSpawns = serializedObject.FindProperty("allowNullCellsInSpawns");
            p_PreviewAreaSize = serializedObject.FindProperty("previewAreaSize");

            m_EdgePlacementInstance = (EdgePlacement)target;
            SetPreviewArea();
            UpdateImage(m_EdgePlacementInstance);
        }

        public override void OnInspectorGUI()
        {
            if (m_editorUtils == null)
            {
                m_editorUtils = StormApp.GetEditorUtils(this);
            }
            m_EdgePlacementInstance = (EdgePlacement)target;

            m_editorUtils.Initialize();

            base.OnInspectorGUI(); // Draw the base PlacementFilter properties

            if(m_defaultOpenPanels)
            {
                edgePlacementPanelExpanded = m_editorUtils.Panel("EdgePlacementPanel", EdgePlacementPanel, m_defaultOpenPanels);
                previewPanelExpanded = m_editorUtils.Panel("PreviewPanel", PreviewPanel, m_defaultOpenPanels);
            }
            else
            {
                edgePlacementPanelExpanded = m_editorUtils.Panel("EdgePlacementPanel", EdgePlacementPanel, edgePlacementPanelExpanded);
                previewPanelExpanded = m_editorUtils.Panel("PreviewPanel", PreviewPanel, previewPanelExpanded);
            }

            if (serializedObject.ApplyModifiedProperties())
            {
                SetPreviewArea();
                UpdateImage(m_EdgePlacementInstance);
            }
        }

        private void EdgePlacementPanel(bool helpActive)
        {
            //test
            PropField("RandomMode", p_RandomMode, helpActive);
            if (p_RandomMode.boolValue)
            {
                PropField("TargetPlacements", p_TargetPlacements, helpActive);
                PropField("ForwardOffset", p_ForwardOffset, helpActive);
            }
            else
            {
                //standard
                PropField("CenterAlongEdge", p_CenterAlongEdge, helpActive);
                PropField("TileAlongEdge", p_TileAlongEdge, helpActive);
                PropField("ForwardOffset", p_ForwardOffset, helpActive);
                PropField("TilingOffset", p_TilingOffset, helpActive);
                PropField("TilingStep", p_TilingStep, helpActive);

                PropField("UseDistanceFromCenter", p_UseDistanceFromCenter, helpActive);
                if (m_EdgePlacementInstance.useDistanceFromCenter)
                {
                    ++EditorGUI.indentLevel;
                    PropField("DistanceFromCenterRange", p_DistanceFromCenterRange, helpActive);
                    PropField("DistanceFromCenterAxisType", p_DistanceFromCenterAxisType, helpActive);
                    --EditorGUI.indentLevel;
                }
            }

            PropField("AllowNullCellsInSpawns", p_AllowNullCellsInSpawns, helpActive);


        }

        private void PreviewPanel(bool helpActive)
        {
            PropField("PreviewAreaSize", p_PreviewAreaSize, helpActive);

            GUILayout.Box(sourceImage);
        }

        public void UpdateImage(EdgePlacement edgePlacement)
        {
            ImageUtils.SetImageToColor(sourceImage, Color.black);

            Vector2 imageWidthHeight = new Vector2(10, 10);

            if (previewArea.Count > 2)
            {
                imageWidthHeight = StructureCreatorUtils.GetSizeOfPolygonBoundingBox(previewArea);
                imageWidthHeight = Mathf.Max(imageWidthHeight.x, imageWidthHeight.y) * Vector2.one;
            }
            ImageUtils.DrawPolygonOnImage(sourceImage, imageWidthHeight, previewArea, new Color(0.1568f, 0.1568f, 0.1568f));

            List<PlacementData> placementData = edgePlacement.GetPreviewPlacementData(previewArea);
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
                new Vector2(0, m_EdgePlacementInstance.previewAreaSize), 
                new Vector2(m_EdgePlacementInstance.previewAreaSize, m_EdgePlacementInstance.previewAreaSize), 
                new Vector2(m_EdgePlacementInstance.previewAreaSize, 0),
                new Vector2(0, 0)};
        }
    }
}
