using UnityEngine;
using UnityEditor;

namespace ProceduralWorlds.Storm.Terrain
{
    [CustomEditor(typeof(StormVTManager))]
    public class StormVTManagerEditor : Editor
    {
        /// <summary>
        /// Reference to the storm terrain manager
        /// </summary>
        private StormVTManager stormVTManager;

        /// <summary>
        /// Get a reference to the manager target
        /// </summary>
        private void OnEnable()
        {
            stormVTManager = (StormVTManager)target;
        }

        /// <summary>
        /// When the GUI is drawn
        /// </summary>
        public override void OnInspectorGUI()
        {
            //Draw the inspector normally
            DrawDefaultInspector();
            
            //Add additional buttons
            if (GUILayout.Button("Update Settings"))
            {
                stormVTManager.Initialize();
            }
            
            if (GUILayout.Button("Save Terrain Tiles"))
            {
                stormVTManager.Initialize();
                stormVTManager.StartEditorUpdates();
            }

            if(GUILayout.Button("Export Terrain Data"))
            {
                stormVTManager.StartTerrainDataBulkExportEditorUpdates();
            }
            if(GUILayout.Button("Export Edited Terrains"))
            {
                stormVTManager.StartTerrainDataListExportEditorUpdates();
            }
            if(GUILayout.Button("Collect All Terrain Layers"))
            {
                stormVTManager.GetAllTerrainLayers();
            }
        }

    }
}