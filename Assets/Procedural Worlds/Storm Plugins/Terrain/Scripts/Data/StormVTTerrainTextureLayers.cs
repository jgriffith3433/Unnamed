using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using static ProceduralWorlds.Storm.Terrain.StormVTEnums;

namespace ProceduralWorlds.Storm.Terrain
{
    [CreateAssetMenu(fileName = "StormVTTerrainTextureLayerSettings", menuName = "Procedural Worlds/Storm/Terrain/TerrainTextureLayerSettings",
        order = 5)]
    [Serializable]
    public class StormVTTerrainTextureLayers : ScriptableObject
    {
        /// <summary>
        /// List of terrain texture layers
        /// </summary>
        public List<StormVTTerrainTextureLayer> stormVTTerrainTextureLayers = new List<StormVTTerrainTextureLayer>();

        /// <summary>
        /// Name to save the generated albedo texture array as
        /// </summary>
        public const string ALBEDO_ARRAY_NAME = "Albedo Texture Array";

        /// <summary>
        /// Name to save the generated normal texture array as
        /// </summary>
        public const string NORMAL_ARRAY_NAME = "Normal Texture Array";

        /// <summary>
        /// Reference to the generated albedo array
        /// </summary>
        public Texture2DArray albedoArray;

        /// <summary>
        /// Reference to the generated normal array
        /// </summary>
        public Texture2DArray normalArray;

        /// <summary>
        /// Update data if changed through the UI
        /// </summary>
        public void OnValidate()
        {
            for(int i = 0; i < stormVTTerrainTextureLayers.Count; i++)
            {
                stormVTTerrainTextureLayers[i].PackLayerSettings();
                stormVTTerrainTextureLayers[i].BroadcastTerrainTileLayerUpdated();
            }
        }

        /// <summary>
        /// Updates terrain layers based on if any changes occurred to the Unity Terrain Layers
        /// </summary>
        /// <param name="targetTerrain">Terrain to update</param>
        /// <param name="removeOldLayers">Whether to remove old layer no longer present from the texture layers</param>
        public void RefreshTerrainLayers(UnityEngine.Terrain targetTerrain = null, bool removeOldLayers = true)
        {

#if UNITY_EDITOR
            List<TerrainLayer> newTerrainLayers = new List<TerrainLayer>();
            //Check if a new terrain layer has been added

            //was a target terrain given? If yes, we only look on this terrain
            if (targetTerrain != null)
            {
                CollectNewLayers(targetTerrain.terrainData, newTerrainLayers);
            }
            else
            //no target terrain, we need to look at all terrains associated with this texture layer asset
            {
                StormVTTerrainDataContainer stormVTTerrainDataContainer = FetchTerrainDataContainer();
                if (stormVTTerrainDataContainer != null)
                {
                    //Loop through all TerrainDatas (they contain a list of terrain layers)
                    for (int terrainIndex = 0; terrainIndex < stormVTTerrainDataContainer.m_terrainDataList.Length; terrainIndex++)
                    {
                        string assetPath = AssetDatabase.GUIDToAssetPath(stormVTTerrainDataContainer.m_terrainDataList[terrainIndex]);
                        TerrainData td = AssetDatabase.LoadAssetAtPath<TerrainData>(assetPath);
                        //TerrainData terrain = stormVTTerrainDataContainer.m_terrainDataList[terrainIndex];
                        Debug.Log($"Fetching Layers from terrain index {terrainIndex}, Terrain Data file: {assetPath}");
                        CollectNewLayers(td, newTerrainLayers);
                        td = null;
                        Resources.UnloadUnusedAssets();
                    }
                }
                
               stormVTTerrainDataContainer = null;
            }

            for (int i = 0; i < newTerrainLayers.Count; i++)
            {
                if (newTerrainLayers[i] == null)
                {
                    continue;
                }
                StormVTTerrainTextureLayer newTerrainLayer = new StormVTTerrainTextureLayer();
                newTerrainLayer.SetTerrainLayer(newTerrainLayers[i]);
                stormVTTerrainTextureLayers.Add(newTerrainLayer);
                ConvertLayerTexturesToPackedFormat(newTerrainLayer);
            }

            if (removeOldLayers)
            {
                StormVTTerrainDataContainer stormVTTerrainDataContainer = FetchTerrainDataContainer();
                //Remove layers that are no longer part of the terrains
                List<StormVTTerrainTextureLayer> layersToRemove = new List<StormVTTerrainTextureLayer>();

                //Loop through all terrain texture layers, test if we have an old entry
                for (int stormLayerIndex = 0; stormLayerIndex < stormVTTerrainTextureLayers.Count; stormLayerIndex++)

                {
                    bool isInTerrain = false;
                    StormVTTerrainTextureLayer currentTerrainLayer = stormVTTerrainTextureLayers[stormLayerIndex];
                    //Loop through all TerrainDatas (they contain a list of terrain layers)
                    for (int terrainIndex = 0; terrainIndex < stormVTTerrainDataContainer.m_terrainDataList.Length; terrainIndex++)
                    {
                        if (currentTerrainLayer.m_terrainLayerReference == null)
                        { 
                            layersToRemove.Add(currentTerrainLayer);
                            continue;
                        }

                        //TerrainData terrain = stormVTTerrainDataContainer.m_terrainDataList[terrainIndex];
                        string assetPath = AssetDatabase.GUIDToAssetPath(stormVTTerrainDataContainer.m_terrainDataList[terrainIndex]);
                        TerrainData terrain = AssetDatabase.LoadAssetAtPath<TerrainData>(assetPath);
                        if (terrain.terrainLayers.Contains(currentTerrainLayer.m_terrainLayerReference))
                        {
                            isInTerrain = true;
                        }
                        terrain = null;
                        Resources.UnloadUnusedAssets();
                    }
                    if (!isInTerrain)
                    {
                        layersToRemove.Add(currentTerrainLayer);
                    }
                }
                foreach (var layer in layersToRemove)
                {
                    stormVTTerrainTextureLayers.Remove(layer);
                }
            }
            //Update Settings on grid as well
            StormVTTerrainGrid stormVTTerrainGrid = FetchTerrainGrid();
            if (targetTerrain != null)
            {
                stormVTTerrainGrid.UpdateTerrainIndices(targetTerrain, this);
            }
            else
            {
                stormVTTerrainGrid.UpdateTerrainIndices(null, this);
            }


            //Repack all data
            for (int i = 0; i < stormVTTerrainTextureLayers.Count; i++)
            {
                stormVTTerrainTextureLayers[i].PackLayerSettings();
                stormVTTerrainTextureLayers[i].BroadcastTerrainTileLayerUpdated();
            }

            //Set dirty edited settings, and assign container to null to reduce memory in editor
            EditorUtility.SetDirty(stormVTTerrainGrid);
            EditorUtility.SetDirty(this);
#endif

        }

        /// <summary>
        /// Looks at the layers in the given terrain data object and decides whether they are new layers or not
        /// </summary>
        /// <param name="td"></param>
        /// <param name="newTerrainLayers"></param>
        private void CollectNewLayers(TerrainData td, List<TerrainLayer> newTerrainLayers)
        {
            if (td == null)
            {
                Debug.LogError("The terrain data for this terrain is null! This terrain will be skipped in processing.");
                return;
            }
            if (newTerrainLayers == null)
            {
                Debug.LogWarning("The list of new terrain layers is null! This should not happen, starting a new list now.");
                newTerrainLayers = new List<TerrainLayer>();
            }

            //Loop through all terrain layers
            for (int terrainLayerIndex = 0; terrainLayerIndex < td.terrainLayers.Length; terrainLayerIndex++)
            {
                if (td.terrainLayers[terrainLayerIndex] == null)
                {
                    continue;
                }

                bool terrainLayerInList = false;
                //Loop through all Storm terrain layers
                for (int stormLayerIndex = 0; stormLayerIndex < stormVTTerrainTextureLayers.Count; stormLayerIndex++)
                {
                    if (stormVTTerrainTextureLayers[stormLayerIndex] == null)
                    {
                        continue;
                    }

                    //Compare if storm terrain layer contains this layer index
                    if (td.terrainLayers[terrainLayerIndex] == stormVTTerrainTextureLayers[stormLayerIndex].m_terrainLayerReference)
                    {
                        terrainLayerInList = true;
                        stormVTTerrainTextureLayers[stormLayerIndex].UpdateTerrainLayer(stormVTTerrainTextureLayers[stormLayerIndex].m_terrainLayerReference);
                    }
                }

                //If the storm layers don't already contain this layer then add it in
                if (!terrainLayerInList)
                {


                    if (!newTerrainLayers.Contains(td.terrainLayers[terrainLayerIndex]))
                    {
                        newTerrainLayers.Add(td.terrainLayers[terrainLayerIndex]);
                    }

                }
            }
        }

        /// <summary>
        /// Retreives the terrain grid asset
        /// </summary>
        /// <returns></returns>
        public StormVTTerrainGrid FetchTerrainGrid()
        {
            StormVTTerrainGrid stormVTTerrainGrid = null;
#if UNITY_EDITOR
            string vtGridFilePath = AssetDatabase.GetAssetPath(this).Replace("_TerrainTextureLayers.asset", "_TerrainGrid.asset");


            stormVTTerrainGrid = AssetDatabase.LoadAssetAtPath<StormVTTerrainGrid>(vtGridFilePath);
            
#endif
            return stormVTTerrainGrid;
        }

        /// <summary>
        /// Retrieves the terrain data container asset
        /// </summary>
        /// <returns></returns>
        public StormVTTerrainDataContainer FetchTerrainDataContainer()
        {
            StormVTTerrainDataContainer stormVTTerrainDataContainer = null;

#if UNITY_EDITOR
            //grid asset not loaded, try to load it in
            //switched to load based on the location of the active terrain texture layers asset - loading based on the currently active scene might not always be correct
            string vtDataContainerFilePath = AssetDatabase.GetAssetPath(this).Replace("_TerrainTextureLayers.asset", "_TerrainDataContainer.asset");

            //Load asset at the file path
            stormVTTerrainDataContainer =
                (StormVTTerrainDataContainer)AssetDatabase.LoadAssetAtPath(vtDataContainerFilePath, typeof(StormVTTerrainDataContainer));
#endif

            return stormVTTerrainDataContainer;
        }

        /// <summary>
        /// Retrieves the storm terrain settings asset
        /// </summary>
        /// <returns></returns>
        public StormVTSettings FetchTerrainSettings()
        {
            StormVTSettings stormVTSettings = null;
#if UNITY_EDITOR
            string vtSettingsFilePath = AssetDatabase.GetAssetPath(this).Replace("_TerrainTextureLayers.asset", "_StormVTSettings.asset");



            stormVTSettings = AssetDatabase.LoadAssetAtPath<StormVTSettings>(vtSettingsFilePath);
            
#endif
            return stormVTSettings;
        }

        /// <summary>
        /// Converts Albedo, Normal and Mask map into convert Packed Albedo and Packed Normal
        /// This consumes one less texture than 3 per terrain layers
        /// </summary>
        public void ConvertTexturesToPackedFormat()
        {
#if UNITY_EDITOR
            string assetsFolderPath = StormVTConstants.GetTerrainTextureAssetsFolder();
            List<Texture2D> packedDiffuseMaps = new List<Texture2D>();
            List<Texture2D> packedNormalMaps = new List<Texture2D>();

            // Setup our post processing converting materials
            Material convertToAlbedoMaterial = StormVTUtilities.convertToAlbedoMaterial;
            Material convertToNormalMaterial = StormVTUtilities.convertToNormalMaterial;
            Material createAlbedoMapMaterial = StormVTUtilities.createAlbedoMapMaterial;
            Material createNormalMapMaterial = StormVTUtilities.createNormalMapMaterial;

            
            int defaultSize = 2048;

            for (int i = 0; i < stormVTTerrainTextureLayers.Count; i++)
            {
                StormVTTerrainTextureLayer terrainLayer = stormVTTerrainTextureLayers[i];



                RenderTexture albedoRef;
                if (terrainLayer.m_diffuseMap == null)
                {
                    albedoRef = new RenderTexture(defaultSize, defaultSize, 0, RenderTextureFormat.Default, RenderTextureReadWrite.sRGB);
                    albedoRef.Create();
                    Graphics.Blit(null, albedoRef, createAlbedoMapMaterial);
                }
                else
                {
                    albedoRef = new RenderTexture(terrainLayer.m_diffuseMap.width, terrainLayer.m_diffuseMap.height, 0, RenderTextureFormat.Default, RenderTextureReadWrite.sRGB);
                    albedoRef.Create();
                    Graphics.Blit(terrainLayer.m_diffuseMap, albedoRef);
                }


                // Create Temp Mask Map Texture
                RenderTexture convertedMaskMap = new RenderTexture(albedoRef.width, albedoRef.height, 0, RenderTextureFormat.Default, RenderTextureReadWrite.sRGB);

                // No Mask Map provided
                if (terrainLayer.m_maskMap == null)
                {
                    Material createMaskMap = StormVTUtilities.convertToMaskMapMaterial;

                    // Does Albedo Have Smoothness in Alpha?
                    bool hasAlpha = false;
                    if (terrainLayer.m_diffuseMap != null)
                    {

                        string albedoMapPath = AssetDatabase.GetAssetPath(terrainLayer.m_diffuseMap);
                        TextureImporter importer = AssetImporter.GetAtPath(albedoMapPath) as TextureImporter;
                        hasAlpha = importer.DoesSourceTextureHaveAlpha();
                        createMaskMap.SetTexture("_Albedo", terrainLayer.m_diffuseMap);

                    }
                    else
                    {
                        createMaskMap.SetTexture("_Albedo", albedoRef);
                    }
                    createMaskMap.SetInt("_HasAlbedo", hasAlpha ? 1 : 0);

                    Graphics.Blit(null, convertedMaskMap, createMaskMap);
                }
                else
                {
                    Graphics.Blit(terrainLayer.m_maskMap, convertedMaskMap);
                }

                


                RenderTexture packedDiffuseRT = new RenderTexture(albedoRef.width, albedoRef.height, 0, RenderTextureFormat.Default, RenderTextureReadWrite.sRGB);
                packedDiffuseRT.name = "convertedAlbedo";
                packedDiffuseRT.enableRandomWrite = true;
                packedDiffuseRT.useMipMap = true;
                packedDiffuseRT.Create();
                convertToAlbedoMaterial.SetTexture("_Albedo", albedoRef);
                convertToAlbedoMaterial.SetTexture("_MaskMap", convertedMaskMap);
                Graphics.Blit(albedoRef, packedDiffuseRT, convertToAlbedoMaterial);
                
                RenderTexture normalRef;
                if (terrainLayer.m_normalMap == null)
                {
                    normalRef = new RenderTexture(albedoRef.width, albedoRef.height, 0, RenderTextureFormat.Default);
                    normalRef.Create();
                    Graphics.Blit(null, normalRef, createNormalMapMaterial);
                }
                else
                {
                    normalRef = new RenderTexture(terrainLayer.m_normalMap.width, terrainLayer.m_normalMap.width, 0, RenderTextureFormat.Default);
                    normalRef.Create();
                    Graphics.Blit(terrainLayer.m_normalMap, normalRef);
                }
                RenderTexture packedNormalRT = new RenderTexture(normalRef.width, normalRef.height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
                packedDiffuseRT.name = "convertedNormal";
                packedNormalRT.enableRandomWrite = true;
                packedNormalRT.useMipMap = true;
                packedNormalRT.Create();
                convertToNormalMaterial.SetTexture("_Normal", normalRef);
                convertToNormalMaterial.SetTexture("_MaskMap", convertedMaskMap);
                Graphics.Blit(normalRef, packedNormalRT, convertToNormalMaterial);


                RenderTexture.active = packedDiffuseRT;
                Texture2D packedDiffuseMapTex = new Texture2D(packedDiffuseRT.width, packedDiffuseRT.height, TextureFormat.RGBA32, false, false);
                packedDiffuseMapTex.name =  $"{terrainLayer.m_name}_PackedDiffuse";
                packedDiffuseMapTex.ReadPixels(new Rect(0, 0, packedDiffuseRT.width, packedDiffuseRT.height), 0, 0);
                packedDiffuseMapTex.Apply();

                RenderTexture.active = packedNormalRT;
                Texture2D packedNormalMapTex = new Texture2D(packedNormalRT.width, packedNormalRT.height, TextureFormat.RGBA32, false, true);
                packedNormalMapTex.name = $"{terrainLayer.m_name}_PackedNormal";
                packedNormalMapTex.ReadPixels(new Rect(0, 0, packedNormalRT.width, packedNormalRT.height), 0, 0);
                packedNormalMapTex.Apply();

                packedDiffuseMaps.Add(packedDiffuseMapTex);
                packedNormalMaps.Add(packedNormalMapTex);
            }

            SaveAndImportPackedTexturesList(ref packedDiffuseMaps, ref packedNormalMaps);

#endif
        }

        /// <summary>
        /// Converts a single terrain layer to the packed format of Packed Albedo and Packed Normal
        /// This consumes one less texture than 3 per terrain layers
        /// </summary>
        public void ConvertLayerTexturesToPackedFormat(StormVTTerrainTextureLayer terrainLayer)
        {
#if UNITY_EDITOR
            string assetsFolderPath = StormVTConstants.GetTerrainTextureAssetsFolder();
            List<Texture2D> packedDiffuseMaps = new List<Texture2D>();
            List<Texture2D> packedNormalMaps = new List<Texture2D>();

            // Setup our post processing converting materials
            Material convertToAlbedoMaterial = StormVTUtilities.convertToAlbedoMaterial;
            Material convertToNormalMaterial = StormVTUtilities.convertToNormalMaterial;
            Material createAlbedoMapMaterial = StormVTUtilities.createAlbedoMapMaterial;
            Material createNormalMapMaterial = StormVTUtilities.createNormalMapMaterial;


            int defaultSize = 2048;

            {




                RenderTexture albedoRef;
                if (terrainLayer.m_diffuseMap == null)
                {
                    albedoRef = new RenderTexture(defaultSize, defaultSize, 0, RenderTextureFormat.Default, RenderTextureReadWrite.sRGB);
                    albedoRef.Create();
                    Graphics.Blit(null, albedoRef, createAlbedoMapMaterial);
                }
                else
                {
                    albedoRef = new RenderTexture(terrainLayer.m_diffuseMap.width, terrainLayer.m_diffuseMap.height, 0, RenderTextureFormat.Default, RenderTextureReadWrite.sRGB);
                    albedoRef.Create();
                    Graphics.Blit(terrainLayer.m_diffuseMap, albedoRef);
                }


                // Create Temp Mask Map Texture
                RenderTexture convertedMaskMap = new RenderTexture(albedoRef.width, albedoRef.height, 0, RenderTextureFormat.Default, RenderTextureReadWrite.sRGB);

                // No Mask Map provided
                if (terrainLayer.m_maskMap == null)
                {
                    Material createMaskMap = StormVTUtilities.convertToMaskMapMaterial;

                    // Does Albedo Have Smoothness in Alpha?
                    bool hasAlpha = false;
                    if (terrainLayer.m_diffuseMap != null)
                    {

                        string albedoMapPath = AssetDatabase.GetAssetPath(terrainLayer.m_diffuseMap);
                        TextureImporter importer = AssetImporter.GetAtPath(albedoMapPath) as TextureImporter;
                        hasAlpha = importer.DoesSourceTextureHaveAlpha();
                        createMaskMap.SetTexture("_Albedo", terrainLayer.m_diffuseMap);

                    }
                    else
                    {
                        createMaskMap.SetTexture("_Albedo", albedoRef);
                    }
                    createMaskMap.SetInt("_HasAlbedo", hasAlpha ? 1 : 0);

                    Graphics.Blit(null, convertedMaskMap, createMaskMap);
                }
                else
                {
                    Graphics.Blit(terrainLayer.m_maskMap, convertedMaskMap);
                }


                RenderTexture packedDiffuseRT = new RenderTexture(albedoRef.width, albedoRef.height, 0, RenderTextureFormat.Default, RenderTextureReadWrite.sRGB);
                packedDiffuseRT.name = "convertedAlbedo";
                packedDiffuseRT.enableRandomWrite = true;
                packedDiffuseRT.useMipMap = true;
                packedDiffuseRT.Create();
                convertToAlbedoMaterial.SetTexture("_Albedo", albedoRef);
                convertToAlbedoMaterial.SetTexture("_MaskMap", convertedMaskMap);
                Graphics.Blit(albedoRef, packedDiffuseRT, convertToAlbedoMaterial);

                RenderTexture normalRef;
                if (terrainLayer.m_normalMap == null)
                {
                    normalRef = new RenderTexture(albedoRef.width, albedoRef.height, 0, RenderTextureFormat.Default);
                    normalRef.Create();
                    Graphics.Blit(null, normalRef, createNormalMapMaterial);
                }
                else
                {
                    normalRef = new RenderTexture(terrainLayer.m_normalMap.width, terrainLayer.m_normalMap.width, 0, RenderTextureFormat.Default);
                    normalRef.Create();
                    Graphics.Blit(terrainLayer.m_normalMap, normalRef);
                }
                RenderTexture packedNormalRT = new RenderTexture(normalRef.width, normalRef.height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
                packedDiffuseRT.name = "convertedNormal";
                packedNormalRT.enableRandomWrite = true;
                packedNormalRT.useMipMap = true;
                packedNormalRT.Create();
                convertToNormalMaterial.SetTexture("_Normal", normalRef);
                convertToNormalMaterial.SetTexture("_MaskMap", convertedMaskMap);
                Graphics.Blit(normalRef, packedNormalRT, convertToNormalMaterial);


                RenderTexture.active = packedDiffuseRT;
                Texture2D packedDiffuseMapTex = new Texture2D(packedDiffuseRT.width, packedDiffuseRT.height, TextureFormat.RGBA32, false, false);
                packedDiffuseMapTex.name = $"{terrainLayer.m_name}_PackedDiffuse";
                packedDiffuseMapTex.ReadPixels(new Rect(0, 0, packedDiffuseRT.width, packedDiffuseRT.height), 0, 0);
                packedDiffuseMapTex.Apply();

                RenderTexture.active = packedNormalRT;
                Texture2D packedNormalMapTex = new Texture2D(packedNormalRT.width, packedNormalRT.height, TextureFormat.RGBA32, false, true);
                packedNormalMapTex.name = $"{terrainLayer.m_name}_PackedNormal";
                packedNormalMapTex.ReadPixels(new Rect(0, 0, packedNormalRT.width, packedNormalRT.height), 0, 0);
                packedNormalMapTex.Apply();

                packedDiffuseMaps.Add(packedDiffuseMapTex);
                packedNormalMaps.Add(packedNormalMapTex);




                string diffuseFilePath = SaveTexture2DToDiskNoImport(assetsFolderPath, packedDiffuseMapTex);
                DestroyImmediate(packedDiffuseMapTex);

                string normalFilePath = SaveTexture2DToDiskNoImport(assetsFolderPath, packedNormalMapTex);
                DestroyImmediate(packedDiffuseMapTex);

                ImportNormalMap(normalFilePath);
                ImportDiffuseMap(diffuseFilePath);

                terrainLayer.m_packedDiffuseMap = AssetDatabase.LoadAssetAtPath<Texture2D>(diffuseFilePath);
                terrainLayer.m_packedNormalMap = AssetDatabase.LoadAssetAtPath<Texture2D>(normalFilePath);
            }

           

#endif
        }

        /// <summary>
        /// Saves the converted packed textures
        /// </summary>
        /// <param name="packedDiffuseList">List of packed diffuse maps</param>
        /// <param name="packedNormalList">List of packed normal maps</param>
        public void SaveAndImportPackedTexturesList(ref List<Texture2D> packedDiffuseList, ref List<Texture2D> packedNormalList)
        {
#if UNITY_EDITOR
            string assetsFolderPath = StormVTConstants.GetTerrainTextureAssetsFolder();
            string[] diffuseFilePaths = new string[packedDiffuseList.Count];
            string[] normalFilePaths = new string[packedNormalList.Count];

            //Save Textures
            try
            {
                AssetDatabase.StartAssetEditing();
                for (int i = 0; i < packedDiffuseList.Count; i++)
                {
                    string filePath = SaveTexture2DToDiskNoImport(assetsFolderPath, packedDiffuseList[i]);
                    diffuseFilePaths[i] = filePath;
                    DestroyImmediate(packedDiffuseList[i]);
                    packedDiffuseList[i] = null;

                    filePath = SaveTexture2DToDiskNoImport(assetsFolderPath, packedNormalList[i]);
                    normalFilePaths[i] = filePath;
                    DestroyImmediate(packedNormalList[i]);
                    packedNormalList[i] = null;

                }
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }

            //Import Textures
            try
            {
                AssetDatabase.StartAssetEditing();
                for(int i = 0; i < packedDiffuseList.Count; i++)
                {
                    ImportNormalMap(normalFilePaths[i]);
                    ImportDiffuseMap(diffuseFilePaths[i]);
                }
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                
            }

            //Assign Textures
            try
            {
                AssetDatabase.StartAssetEditing();
                for (int i = 0; i < packedDiffuseList.Count; i++)
                {
                    stormVTTerrainTextureLayers[i].m_packedDiffuseMap = AssetDatabase.LoadAssetAtPath<Texture2D>(diffuseFilePaths[i]);
                    stormVTTerrainTextureLayers[i].m_packedNormalMap = AssetDatabase.LoadAssetAtPath<Texture2D>(normalFilePaths[i]);
                }
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }

            packedDiffuseList.Clear();
            packedNormalList.Clear();
            EditorUtility.SetDirty(this);
#endif
        }

        /// <summary>
        /// Save a texture to the disc without peroforming Asset.Import
        /// </summary>
        /// <param name="userDataPath">Path of the asset</param>
        /// <param name="texture2D">Texture to save</param>
        /// <param name="filterMode">Filter mode on the texture</param>
        /// <returns></returns>
        public string SaveTexture2DToDiskNoImport(string userDataPath, Texture2D texture2D,
            FilterMode filterMode = FilterMode.Bilinear)
        {
            string path = string.Empty;

            if (texture2D == null)
                return path;

#if UNITY_EDITOR
            path = AssetDatabase.GetAssetPath(texture2D);
            if (string.IsNullOrEmpty(path))
            {
                string assetFilePath = $"{texture2D.name}.png";
                path = $"{userDataPath}/{assetFilePath}";
            }

            StormVTConstants.WriteTexture2D(path, texture2D);
            AssetDatabase.ImportAsset(path);
#endif
            return path;
        }

        /// <summary>
        /// Import a normalmap texture once it has been saved out
        /// </summary>
        /// <param name="path"></param>
        public void ImportNormalMap(string path)
        {
#if UNITY_EDITOR
            Texture2D texture2D = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            if (string.IsNullOrEmpty(path))
            {
                string assetFilePath = $"{texture2D.name}.png";
                path = $"{StormVTConstants.GetTerrainTextureAssetsFolder()}/{assetFilePath}";
            }

            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Default;
                importer.sRGBTexture = false;
                importer.alphaSource = TextureImporterAlphaSource.None;
                importer.alphaIsTransparency = false;
                importer.ignorePngGamma = false;
                // -> Advanced
                importer.npotScale = TextureImporterNPOTScale.None;
                importer.isReadable = false;
                importer.streamingMipmaps = false;
                importer.vtOnly = false;
                importer.mipmapEnabled = true;
                // <- Advanced
                importer.wrapMode = TextureWrapMode.Repeat;
                importer.filterMode = FilterMode.Bilinear;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.maxTextureSize = texture2D.width;
                // <- Importer Platform Settings
                TextureImporterPlatformSettings settings = importer.GetDefaultPlatformTextureSettings();
                settings.maxTextureSize = texture2D.width;
                settings.resizeAlgorithm = TextureResizeAlgorithm.Bilinear;
                settings.format = TextureImporterFormat.RGBA32;
                settings.textureCompression = TextureImporterCompression.Uncompressed;
                importer.SetPlatformTextureSettings(settings);
                importer.SaveAndReimport();
            }
#endif
        }

        /// <summary>
        /// Import a diffuse map once it has been saved out
        /// </summary>
        /// <param name="path"></param>
        public void ImportDiffuseMap(string path)
        {
#if UNITY_EDITOR
            //string path = AssetDatabase.GetAssetPath(texture2D);
            Texture2D texture2D = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            if (string.IsNullOrEmpty(path))
            {
                string assetFilePath = $"{texture2D.name}.png";
                path = $"{StormVTConstants.GetTerrainTextureAssetsFolder()}/{assetFilePath}";
            }

            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Default;
                importer.sRGBTexture = true;
                importer.alphaSource = TextureImporterAlphaSource.None;
                importer.alphaIsTransparency = false;
                importer.ignorePngGamma = false;
                // -> Advanced
                importer.npotScale = TextureImporterNPOTScale.None;
                importer.isReadable = false;
                importer.streamingMipmaps = false;
                importer.vtOnly = false;
                importer.mipmapEnabled = true;
                // <- Advanced
                importer.wrapMode = TextureWrapMode.Repeat;
                importer.filterMode = FilterMode.Bilinear;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.maxTextureSize = texture2D.width;
                // <- Importer Platform Settings
                TextureImporterPlatformSettings settings = importer.GetDefaultPlatformTextureSettings();
                settings.maxTextureSize = texture2D.width;
                settings.resizeAlgorithm = TextureResizeAlgorithm.Bilinear;
                settings.format = TextureImporterFormat.RGBA32;
                settings.textureCompression = TextureImporterCompression.Uncompressed;
                importer.SetPlatformTextureSettings(settings);
                importer.SaveAndReimport();
            }
#endif
        }

        /// <summary>
        /// Converts the packed textures into a packed albedo array and packed normal array
        /// </summary>
        public void ConvertLayersToTexture2DArray()
        {
#if UNITY_EDITOR
            StormVTSettings stormVTSettings = FetchTerrainSettings();

#if UNITY_2022_2_OR_NEWER
            int prevMasterTextureLimit = QualitySettings.globalTextureMipmapLimit;
            QualitySettings.globalTextureMipmapLimit = 0;
#else
            int prevMasterTextureLimit = QualitySettings.masterTextureLimit;
            QualitySettings.masterTextureLimit = 0;
#endif

            if (stormVTTerrainTextureLayers.Count == 0)
            {
                Debug.Log("Texture Array Generation Failed: One or more Terrains do not contain any Layers.");
                return;
            }

            int maxTextureSize = (int)stormVTSettings.m_maxTextureArraySize;
            int anisoLevel = 0;
            StormCompressionFormat compressionFormat = stormVTSettings.m_textureArrayCompressionFormat;
            TextureFormat textureFormat = (TextureFormat)compressionFormat;

            albedoArray = new Texture2DArray(maxTextureSize, maxTextureSize, stormVTTerrainTextureLayers.Count, textureFormat, true, false);
            albedoArray.anisoLevel = anisoLevel;

            normalArray = new Texture2DArray(maxTextureSize, maxTextureSize, stormVTTerrainTextureLayers.Count, textureFormat, true, true);
            normalArray.anisoLevel = anisoLevel;

            for (int i = 0; i < stormVTTerrainTextureLayers.Count; i++)
            {
                int width = stormVTTerrainTextureLayers[i].m_packedDiffuseMap.width;
                int height = stormVTTerrainTextureLayers[i].m_packedDiffuseMap.height;

                RenderTexture albedoPackedMap = new RenderTexture(width, height, 0, RenderTextureFormat.ARGB32);
                Graphics.Blit(stormVTTerrainTextureLayers[i].m_packedDiffuseMap, albedoPackedMap);

                RenderTexture normalPackedMap = new RenderTexture(width, height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
                Graphics.Blit(stormVTTerrainTextureLayers[i].m_packedNormalMap, normalPackedMap);

                RenderTexture.active = albedoPackedMap;
                Texture2D tempAlbedoTex = new Texture2D(width, height, StormVTConstants.DefaultTextureFormat, true, false);
                tempAlbedoTex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
                tempAlbedoTex.Apply();
                tempAlbedoTex = StormVTUtilities.ResizeTexture(tempAlbedoTex, textureFormat, anisoLevel, maxTextureSize, maxTextureSize, true, false);

                RenderTexture.active = normalPackedMap;
                Texture2D tempNormalTex = new Texture2D(width, height, StormVTConstants.DefaultTextureFormat, true, false);
                tempNormalTex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
                tempNormalTex.Apply();
                tempNormalTex = StormVTUtilities.ResizeTexture(tempNormalTex, textureFormat, anisoLevel, maxTextureSize, maxTextureSize, true, false);

                for (int m = 0; m < tempAlbedoTex.mipmapCount; m++)
                {
                    Graphics.CopyTexture(tempAlbedoTex, 0, m, albedoArray, i, m);
                    Graphics.CopyTexture(tempNormalTex, 0, m, normalArray, i, m);
                }

                DestroyImmediate(tempAlbedoTex);
                DestroyImmediate(tempNormalTex);
            }

            albedoArray.name = ALBEDO_ARRAY_NAME;
            normalArray.name = NORMAL_ARRAY_NAME;

            string albedoArrayPath = StormVTConstants.GetOrCreateAssetPath(ALBEDO_ARRAY_NAME);
            string normalArrayPath = StormVTConstants.GetOrCreateAssetPath(NORMAL_ARRAY_NAME);
            albedoArray = StormVTUtilities.CreateOrReplaceAsset(albedoArray, albedoArrayPath);
            normalArray = StormVTUtilities.CreateOrReplaceAsset(normalArray, normalArrayPath);

            EditorUtility.SetDirty(this);


#if UNITY_2022_2_OR_NEWER
            QualitySettings.globalTextureMipmapLimit = prevMasterTextureLimit;
#else
            QualitySettings.masterTextureLimit = prevMasterTextureLimit;
#endif
#endif

        }

    }
}