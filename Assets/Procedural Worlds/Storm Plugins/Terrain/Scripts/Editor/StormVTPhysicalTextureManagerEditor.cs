using UnityEngine;
using UnityEditor;

namespace ProceduralWorlds.Storm.Terrain
{
    [CustomEditor(typeof(StormVTPhysicalTextureManager))]
    public class StormVTPhysicalTextureManagerEditor : Editor
    {
        /// <summary>
        /// References to variables defined in the Physical Texture Manager
        /// </summary>
        SerializedProperty stormVTSettingsProp;
        SerializedProperty stormVTTerrainGridProp;
        SerializedProperty stormVTPageTableClipmapProp;
        SerializedProperty stormVTTextureCompressorProp;
        /*SerializedProperty physicalTextureHeightProp;
        SerializedProperty physicalTextureNormalProp;
        SerializedProperty physicalTextureColorSource0Prop;
        SerializedProperty physicalTextureColorSource1Prop;
        SerializedProperty physicalTextureGrassColorProp;
        SerializedProperty physicalTextureTreeColorProp;
        SerializedProperty physicalTextureDistanceNormalProp;*/
        SerializedProperty quadTreeProp;
        SerializedProperty activePageCountProp;
        SerializedProperty terrainTileProp;

        /// <summary>
        /// Reference to the physical texture manager target
        /// </summary>
        private StormVTPhysicalTextureManager stormVTPhyiscalTextureManager;

        /// <summary>
        /// Find the properties associated with the serialized objects
        /// </summary>
        private void OnEnable()
        {
            if (target == null)
            {
                return;
            }

            stormVTPhyiscalTextureManager = (StormVTPhysicalTextureManager)target;
            stormVTSettingsProp = serializedObject.FindProperty("stormVTSettings");
            stormVTTerrainGridProp = serializedObject.FindProperty("stormVTTerrainGrid");
            stormVTPageTableClipmapProp = serializedObject.FindProperty("stormVTPageTableClipmap");
            stormVTTextureCompressorProp = serializedObject.FindProperty("stormVTTextureCompression");

            /*physicalTextureHeightProp = serializedObject.FindProperty("m_physicalTextureHeight");
            physicalTextureNormalProp = serializedObject.FindProperty("m_physicalTextureNormal");
            physicalTextureColorSource0Prop = serializedObject.FindProperty("m_physicalTextureColorSource0");
            physicalTextureColorSource1Prop = serializedObject.FindProperty("m_physicalTextureColorSource1");
            physicalTextureGrassColorProp = serializedObject.FindProperty("m_physicalTextureGrassColor");
            physicalTextureTreeColorProp = serializedObject.FindProperty("m_physicalTextureTreeColor");
            physicalTextureDistanceNormalProp = serializedObject.FindProperty("m_physicalTextureDistanceNormal");*/

            quadTreeProp = serializedObject.FindProperty("quadTree");

            activePageCountProp = serializedObject.FindProperty("m_activePageCount");
            terrainTileProp = serializedObject.FindProperty("m_stormVTTerrainTile");

        }

        /// <summary>
        /// Draw the inspector normally
        /// </summary>
        public override void OnInspectorGUI()
        {
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.ObjectField("Script", MonoScript.FromMonoBehaviour((StormVTPhysicalTextureManager)target), typeof(StormVTPhysicalTextureManager), false);
            EditorGUI.EndDisabledGroup();

            serializedObject.Update();
            
            EditorGUILayout.PropertyField(stormVTSettingsProp);
            EditorGUILayout.PropertyField(stormVTTerrainGridProp);
            EditorGUILayout.PropertyField(stormVTPageTableClipmapProp);
            EditorGUILayout.PropertyField(stormVTTextureCompressorProp);
            
            /*EditorGUILayout.PropertyField(physicalTextureHeightProp);
            EditorGUILayout.PropertyField(physicalTextureNormalProp);
            EditorGUILayout.PropertyField(physicalTextureColorSource0Prop);
            EditorGUILayout.PropertyField(physicalTextureColorSource1Prop);
            EditorGUILayout.PropertyField(physicalTextureGrassColorProp);
            EditorGUILayout.PropertyField(physicalTextureTreeColorProp);
            EditorGUILayout.PropertyField(physicalTextureDistanceNormalProp);*/
            EditorGUILayout.PropertyField(terrainTileProp);

            Object quadTreeObject = quadTreeProp.objectReferenceValue;

            //Add additional buttons

            EditorGUI.BeginChangeCheck();
            quadTreeObject = EditorGUILayout.ObjectField("Quad Tree", quadTreeObject, typeof(StormVTQuadTree), true);
            if (EditorGUI.EndChangeCheck())
            {
                quadTreeProp.objectReferenceValue = quadTreeObject;
            }

            if (GUILayout.Button("Refresh Tiles"))
            {
                stormVTPhyiscalTextureManager.RefreshTiles();
            }

            EditorGUILayout.LabelField("Active Pages: " + activePageCountProp.intValue + "/ " + stormVTPhyiscalTextureManager.stormVTSettings.m_tileCapacity);

            

            serializedObject.ApplyModifiedProperties();
        }
    }
}
