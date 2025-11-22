using ProceduralWorlds.Storm.Core;
using ProceduralWorlds.Storm.Utils;
using PWCommon5;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEditorInternal;
using UnityEngine;

/// Copyright 2023 Procedural Worlds Ltd
/// Written by Jonathan Williams as part of Storm

namespace ProceduralWorlds.Storm.StormEditor
{
    [CustomEditor(typeof(StormTile), true)]
    [CanEditMultipleObjects]
    public class StormTileEditor : PWEditor
    {
        private EditorUtils m_editorUtils;

        SerializedProperty m_updatePosition;
        SerializedProperty m_updateScale;
        SerializedProperty m_tileData;
        SerializedProperty m_tileRegions;
        SerializedProperty m_savedRegion;
        SerializedProperty m_disableTerrainDetails;
        SerializedProperty m_displayPolygonsInEditor;
        SerializedProperty m_saveTileSingleFile;
        SerializedProperty m_saveRawAsTexture;
        SerializedProperty m_ingestCaptureAlphamaps;
        SerializedProperty m_ingestCaptureDisabled;
        SerializedProperty m_ingestCreateFeatures;
        SerializedProperty ShowSelectionGizmo;

        SerializedProperty m_tileDataPosition;
        SerializedProperty m_tileDataSize;
        SerializedProperty m_tileFeaturesetPosition;
        SerializedProperty m_tileFeaturesetSize;

#if false// GAIA_2023_PRO || GAIA_PRO_PRESENT
        bool doingGaiaSplit = false;
        List<StormTileData> gaiaSplitStartTiles;
        int gaiaSplitProgress = 0;
        bool[][] gaiaSplitMovedPoly;
        bool[][] gaiaSplitMovedTRS;
        bool[][] gaiaSplitMovedTransform;
        bool[][] gaiaSplitMovedTexture;
        bool[][] gaiaSplitMovedTerrain;
#endif

        List<string> BlacklistedLayers = new List<string>();
        bool[] editingLayer = new bool[0];
        ReorderableList Blacklist;

        AnimBool[] outputExpanded = new AnimBool[0];

        public void OnEnable()
        {
            m_editorUtils = StormApp.GetEditorUtils(this);
            m_updatePosition = serializedObject.FindProperty("UpdatePosition");
            m_updateScale = serializedObject.FindProperty("UpdateScale");
            m_tileData = serializedObject.FindProperty("TileData");
            if (m_tileData != null)
            {
                m_tileDataPosition = m_tileData.FindPropertyRelative("Position");
                m_tileDataSize = m_tileData.FindPropertyRelative("Size");
            }
            m_tileRegions = serializedObject.FindProperty("TileRegions");
            if (m_tileRegions != null)
            {
                m_tileFeaturesetPosition = m_tileRegions.FindPropertyRelative("Position");
                m_tileFeaturesetSize = m_tileRegions.FindPropertyRelative("Size");
            }
            m_savedRegion = serializedObject.FindProperty("SavedRegion");
            m_disableTerrainDetails = serializedObject.FindProperty("DisableTerrainDetails");
            m_displayPolygonsInEditor = serializedObject.FindProperty("DisplayPolygonsInEditor");
            m_saveTileSingleFile = serializedObject.FindProperty("SaveTileSingleFile");
            m_saveRawAsTexture = serializedObject.FindProperty("SaveRawAsTexture");
            m_ingestCaptureAlphamaps = serializedObject.FindProperty("IngestCaptureAlphamaps");
            m_ingestCaptureDisabled = serializedObject.FindProperty("IngestCaptureDisabled");
            m_ingestCreateFeatures = serializedObject.FindProperty("IngestCreateFeatures");
            ShowSelectionGizmo = serializedObject.FindProperty("ShowSelectionGizmo");

            Undo.undoRedoPerformed -= OnUndo;
            Undo.undoRedoPerformed += OnUndo;
        }
        private void OnDisable()
        {
            Undo.undoRedoPerformed -= OnUndo;
        }

        public override void OnInspectorGUI()
        {
            m_editorUtils.Initialize();
            //Set up the box style
            m_editorUtils.Panel("ConfigurationPanel", ConfigurationPanel, true);

            bool hasTerrain = false;
            for (int t = 0; t < targets.Length; ++t)
            {
                if (targets[t] is StormTile)
                {
                    StormTile tile = (StormTile)targets[t];
                    if (tile.TileData.IngestedTerrains.Length > 0)
                    {
                        hasTerrain = true;
                        break;
                    }
                }
            }
            m_editorUtils.Panel("OutputPanel", OutputPanel, false);
            if (hasTerrain)
                m_editorUtils.Panel("TerrainPanel", TerrainPanel, true);
            m_editorUtils.Panel("UtilitiesPanel", UtilitiesPanel, true);
            m_editorUtils.Panel("TileSplitPanel", TileSplitPanel, false);
            serializedObject.ApplyModifiedProperties();
#if false// GAIA_2023_PRO || GAIA_PRO_PRESENT
            if (doingGaiaSplit)
            {
                Gaia.TerrainLoaderManager terrainLoader = Gaia.TerrainLoaderManager.Instance;
                int numTerrains = terrainLoader.TerrainSceneStorage?.m_terrainScenes?.Count ?? 0;
                if (gaiaSplitProgress < numTerrains)
                {
                    Gaia.TerrainScene scene = terrainLoader.TerrainSceneStorage.m_terrainScenes[gaiaSplitProgress];
                    bool canceled = StormEditorProgress.Update(scene.GetTerrainName(), gaiaSplitProgress);
                    if (!canceled)
                    {
                        scene.AddRegularReference((target as StormTile).gameObject);
                        scene.UpdateWithCurrentData();

                        StormEditorSettings editorSettings = StormEditorSettings.Instance;
                        for (int t = 0; t < targets.Length; ++t)
                        {
                            StormTile tile = targets[t] as StormTile;
                            List<StormTileData> overlapping = GetOverlappingTiles(t);
                            RemoveExisting(gaiaSplitStartTiles, overlapping);
                            StormInternalUtils.MergeTileData(tile.TileData, overlapping.ToArray(), editorSettings.TileSplitPolygons, editorSettings.TileSplitTRS, editorSettings.TileSplitTextures, editorSettings.TileSplitTerrain, BlacklistedLayers, ref gaiaSplitMovedPoly[t], ref gaiaSplitMovedTRS[t], ref gaiaSplitMovedTransform[t], ref gaiaSplitMovedTexture[t], ref gaiaSplitMovedTerrain[t]);
                        }
                        UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();
                        scene.RemoveRegularReference((target as StormTile).gameObject);
                        scene.UpdateWithCurrentData();
                    }
                    else
                    {
                        doingGaiaSplit = false;
                        StormEditorProgress.Clear();
                    }
                }
                if (++gaiaSplitProgress >= numTerrains)
                {
                    doingGaiaSplit = false;
                    StormEditorProgress.Clear();
                    StormEditorSettings editorSettings = StormEditorSettings.Instance;
                    if (editorSettings.TileSplitRemoveMerged)
                    {
                        for (int t = 0; t < targets.Length; ++t)
                            RemoveData(targets[t] as StormTile, gaiaSplitMovedPoly[t], gaiaSplitMovedTexture[t], gaiaSplitMovedTerrain[t], gaiaSplitMovedTRS[t], gaiaSplitMovedTransform[t]);
                    }
                }
                Repaint();
            }
#endif
        }

        public void ConfigurationPanel(bool helpActive)
        {
            m_editorUtils.PropertyField("mUpdatePosition", m_updatePosition, helpActive);
            EditorGUI.BeginChangeCheck();
            m_editorUtils.PropertyField("mDataPosition", m_tileDataPosition, helpActive);
            if (EditorGUI.EndChangeCheck())
                m_tileFeaturesetPosition.vector3Value = m_tileDataPosition.vector3Value;
            m_editorUtils.PropertyField("mUpdateScale", m_updateScale, helpActive);
            EditorGUI.BeginChangeCheck();
            m_editorUtils.PropertyField("mDataSize", m_tileDataSize, helpActive);
            if (EditorGUI.EndChangeCheck())
                m_tileFeaturesetSize.vector3Value = m_tileDataSize.vector3Value;
            EditorGUI.indentLevel++;
            EditorGUI.BeginChangeCheck();
            m_editorUtils.PropertyField("mTileData", m_tileData, helpActive);
            m_editorUtils.PropertyField("mTileRegions", m_tileRegions, helpActive);
            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
                for (int t = 0; t < targets.Length; ++t)
                    if (targets[t] is StormTile tile)
                        tile.UpdateData(true);
            }
            EditorGUI.indentLevel--;
            m_editorUtils.PropertyField("mDisableTerrainDetails", m_disableTerrainDetails, helpActive);
        }
        public void OutputPanel(bool helpActive)
        {
            StormTile tile = target as StormTile;
            if (tile != null)
            {
                string[] AllLayers = tile.TileData.GetLayerNames();
                IReadOnlyCollection<StormTileLayerData> allTileData = StormDataCache.AllDataTiles;
                StormTileLayerData tileData = null;
                foreach (StormTileLayerData lData in allTileData)
                {
                    if (lData.Data == tile.TileData)
                    {
                        tileData = lData;
                        break;
                    }
                }
                bool dataWasCreated = false;
                if (tileData == null)
                {
                    dataWasCreated = true;
                    tileData = new StormTileLayerData(tile.TileData);
                }

                if (outputExpanded.Length != AllLayers.Length)
                {
                    Array.Resize(ref outputExpanded, AllLayers.Length);
                }
                ++EditorGUI.indentLevel;
                for (int l = 0; l < AllLayers.Length; ++l)
                {
                    if (outputExpanded[l] == null)
                    {
                        outputExpanded[l] = new AnimBool(false, Repaint);
                    }
                    GUILayout.BeginVertical(EditorStyles.helpBox);
                    outputExpanded[l].target = EditorGUILayout.Foldout(outputExpanded[l].target, new GUIContent(AllLayers[l]), true);
                    if (EditorGUILayout.BeginFadeGroup(outputExpanded[l].faded))
                    {
                        List<IDisposable> usedLists = new List<IDisposable>();
                        bool hasLayer = tileData.GetLayer(StormUtils.GetDataLayerIndex(AllLayers[l], StormLayerType.NONE), usedLists, out var layerData, false, "", false);
                        if (hasLayer)
                        {
                            if (layerData.IsLoading)
                            {
                                layerData.FloatHandle.Complete();
                                layerData.InstanceHandle.Complete();
                                layerData.PolygonHandle.Complete();
                            }
                            if (layerData.HasInstances)
                            {
                                GUILayout.Label("Contains " + layerData.Instances.Length + " instances.");
                            }
                            if (layerData.HasPolygons)
                            {
                                GUILayout.Label("Contains " + layerData.PolygonCounts.Length + " polygons.");
                            }
                            if (layerData.HasFloats)
                            {
                                GUILayout.BeginHorizontal();
                                GUILayout.FlexibleSpace();
                                Rect r = GUILayoutUtility.GetRect(200, 200, GUILayout.ExpandWidth(false));
                                GUILayout.FlexibleSpace();
                                GUILayout.EndHorizontal();
                                Texture2D tex = StormUtils.GenerateTexture(layerData.Floats.Array, 0, 1, layerData.FloatResolution);
                                GUI.DrawTexture(r, tex);
                            }
                        }
                        usedLists.ForEach((l) => l.Dispose());
                    }
                    EditorGUILayout.EndFadeGroup();
                    GUILayout.EndVertical();
                }
                --EditorGUI.indentLevel;
                if (dataWasCreated)
                    tileData.Dispose();
            }
        }
        public void TerrainPanel(bool helpActive)
        {
            bool needsExportPrefabs = false;
            bool needsExportFeatureset = false;
            if (m_editorUtils.Button("ExportPrefabsButton", helpActive))
                needsExportPrefabs = true;
            EditorGUILayout.Space();
            m_editorUtils.PropertyField("mSavedRegion", m_savedRegion, helpActive);
            if (m_editorUtils.Button("ExportFeaturesetButton", helpActive))
                needsExportFeatureset = true;
            if (needsExportPrefabs || needsExportFeatureset)
            {
                string exportPrefabPath = "";
                if (needsExportPrefabs)
                    exportPrefabPath = StormUtils.SaveFolderPanel("Export Prefabs", "Assets/Storm User Data/", "");


                for (int t = 0; t < targets.Length; ++t)
                {
                    if (targets[t] is StormTile)
                    {
                        StormTile tile = (StormTile)targets[t];
                        if (tile.TileData != null && tile.TileData.IngestedTerrains != null)
                        {
                            if (needsExportPrefabs && !string.IsNullOrEmpty(exportPrefabPath))
                                StormInternalUtils.ExportPrefabs(tile, exportPrefabPath);
                            if (needsExportFeatureset)
                                StormInternalUtils.ExportRegion(tile);
                        }
                    }
                }
            }
        }
        public void UtilitiesPanel(bool helpActive)
        {

            bool needsRefresh = false;
            bool needsExportSpawns = false;
            bool needsImportSpawns = false;
            m_editorUtils.PropertyField("ShowSelectionGizmo", ShowSelectionGizmo, helpActive);
            m_editorUtils.PropertyField("mSaveRawAsTexture", m_saveRawAsTexture, helpActive);
            m_editorUtils.PropertyField("mSaveTileSingleFile", m_saveTileSingleFile, helpActive);
            if (m_editorUtils.Button("ExportSpawnsButton"))
            {
                needsExportSpawns = true;
            }
            if (m_editorUtils.Button("ImportSpawnsButton"))
            {
                needsImportSpawns = true;
            }
            EditorGUILayout.Space();
            m_editorUtils.PropertyField("mIngestCaptureAlphamaps", m_ingestCaptureAlphamaps, helpActive);
            m_editorUtils.PropertyField("mIngestCaptureDisabled", m_ingestCaptureDisabled, helpActive);
            m_editorUtils.PropertyField("IngestCreateFeatures", m_ingestCreateFeatures, helpActive);
            if (m_editorUtils.Button("RefreshTerrainData", helpActive))
                needsRefresh = true;

            m_editorUtils.PropertyField("mDisplayPolygonsInEditor", m_displayPolygonsInEditor, helpActive);
            if (needsRefresh)
            {
                for (int t = 0; t < targets.Length; ++t)
                {
                    if (targets[t] is StormTile tile)
                    {
                        StormInternalUtils.ResetTileData(tile);
                    }
                }
            }
            if (needsExportSpawns)
            {
                string exportSpawnPath = StormUtils.SaveFolderPanel("Export Spawns", "Assets/Storm User Data/", "");
                if (!string.IsNullOrEmpty(exportSpawnPath))
                {
                    for (int t = 0; t < targets.Length; ++t)
                    {
                        if (targets[t] is StormTile tile)
                        {
                            StormInternalUtils.ExportTileData(tile.TileData, tile.name, tile.SaveTileSingleFile, tile.SaveRawAsTexture, exportSpawnPath);
                            StormUtils.SetAssetDirty(tile);
                            StormUtils.RefreshAssetDatabase();
                        }
                        GUIUtility.ExitGUI();
                    }
                }
            }
            else if (needsImportSpawns)
            {
                string importSpawnPath = EditorUtility.OpenFilePanel("Export Spawns", "Assets/Storm User Data/", "stormTile");
                for (int t = 0; t < targets.Length; ++t)
                {
                    if (targets[t] is StormTile)
                    {
                        StormTile tile = (StormTile)targets[t];
                        if (!string.IsNullOrEmpty(importSpawnPath))
                        {
                            tile.TileData = StormInternalUtils.ImportTileData(importSpawnPath);
                            StormUtils.SetAssetDirty(tile);
                            StormUtils.RefreshAssetDatabase();
                        }
                    }
                }
                GUIUtility.ExitGUI();
            }
        }
        public void TileSplitPanel(bool helpActive)
        {
            StormEditorSettings editorSettings = StormEditorSettings.Instance;
            editorSettings.TileSplitPolygons = m_editorUtils.Toggle("SplitPolygons", editorSettings.TileSplitPolygons, helpActive);
            editorSettings.TileSplitTerrain = m_editorUtils.Toggle("SplitTerrainData", editorSettings.TileSplitTerrain, helpActive);
            editorSettings.TileSplitTextures = m_editorUtils.Toggle("SplitTextures", editorSettings.TileSplitTextures, helpActive);
            editorSettings.TileSplitTRS = m_editorUtils.Toggle("SplitTRS", editorSettings.TileSplitTRS, helpActive);
            editorSettings.TileSplitRemoveMerged = m_editorUtils.Toggle("SplitRemoveMerged", editorSettings.TileSplitRemoveMerged, helpActive);

            if (Blacklist == null)
            {
                BlacklistedLayers = new List<string>(editorSettings.TileBlacklistedLayers);
                Blacklist = new ReorderableList(BlacklistedLayers, typeof(string));
                Blacklist.drawElementCallback += OnDrawLayerListElement;
                Blacklist.drawHeaderCallback += OnDrawBlacklistHeader;
                Blacklist.onAddDropdownCallback += OnAddBlacklist;
                Blacklist.onRemoveCallback += OnRemoveBlacklist;
            }
            if (editingLayer.Length != BlacklistedLayers.Count)
                System.Array.Resize(ref editingLayer, BlacklistedLayers.Count);
            Blacklist.DoLayoutList();
            m_editorUtils.InlineHelp("SplitBlacklistHeader", helpActive);

            EditorGUILayout.Space();
            m_editorUtils.Label("TileSplitHeader", EditorStyles.boldLabel, helpActive);
            EditorGUI.BeginChangeCheck();
            Vector2 newSplit = m_editorUtils.Vector2Field("TileSplitCount", editorSettings.TileSplitCount, helpActive);
            if (EditorGUI.EndChangeCheck())
            {
                editorSettings.TileSplitCount = new Vector2Int((int)newSplit.x, (int)newSplit.y);
            }
            if (m_editorUtils.Button("TileSplitButton", helpActive))
            {
                for (int t = 0; t < targets.Length; ++t)
                {
                    StormTile tile = targets[t] as StormTile;
                    StormTileData[] newTiles = StormInternalUtils.SplitTiles(tile.TileData, Unity.Mathematics.math.int2(editorSettings.TileSplitCount.x, editorSettings.TileSplitCount.y), editorSettings.TileSplitPolygons, editorSettings.TileSplitTRS, editorSettings.TileSplitTextures, editorSettings.TileSplitTerrain, BlacklistedLayers);
                    for (int x = 0; x < newTiles.Length; ++x)
                    {
                        if (!newTiles[x].HasData())
                            continue;
                        GameObject go = new GameObject(tile.gameObject.name + "_" + x);
                        go.transform.position = newTiles[x].Position;
                        go.transform.localScale = Vector3.Scale(newTiles[x].Size, tile.TileData.Size);
                        go.SetActive(false);
                        go.transform.SetParent(tile.transform, true);
                        StormTile newTile = go.AddComponent<StormTile>();
                        newTile.TileData = newTiles[x];
                        go.SetActive(true);
                    }
                }
            }

#if false// GAIA_2023_PRO || GAIA_PRO_PRESENT
            GameObject terrainManagerGO = Gaia.GaiaUtils.GetTerrainLoaderManagerObject(false);
            if (terrainManagerGO != null)
            {
                Gaia.TerrainLoaderManager terrainManager = terrainManagerGO.GetComponent<Gaia.TerrainLoaderManager>();
                int numTerrains = terrainManager.TerrainSceneStorage?.m_terrainScenes?.Count ?? 0;
                EditorGUILayout.Space();
                m_editorUtils.Label("TileTerrainMergeHeader", EditorStyles.boldLabel, helpActive);
                m_editorUtils.LabelField("TileTerrainCount", new GUIContent(numTerrains.ToString()), helpActive);
                EditorGUI.BeginDisabledGroup(numTerrains == 0);
                if (m_editorUtils.Button("TileTerrainMergeButton", helpActive))
                {
                    gaiaSplitStartTiles = GetOverlappingTiles();

                    int numTargets = targets.Length;
                    gaiaSplitMovedPoly = new bool[numTargets][];
                    gaiaSplitMovedTerrain = new bool[numTargets][];
                    gaiaSplitMovedTexture = new bool[numTargets][];
                    gaiaSplitMovedTransform = new bool[numTargets][];
                    gaiaSplitMovedTRS = new bool[numTargets][];
                    for (int t = 0; t < numTargets; ++t)
                    {
                        StormTile tile = targets[t] as StormTile;
                        gaiaSplitMovedPoly[t] = new bool[tile.TileData.PolygonData.Length];
                        gaiaSplitMovedTerrain[t] = new bool[tile.TileData.TerrainData.Length];
                        gaiaSplitMovedTexture[t] = new bool[tile.TileData.Textures.Length];
                        gaiaSplitMovedTransform[t] = new bool[tile.TileData.TransformData.Length];
                        gaiaSplitMovedTRS[t] = new bool[tile.TileData.TRSData.Length];
                    }

                    doingGaiaSplit = true;
                    gaiaSplitProgress = 0;
                    Gaia.TerrainLoaderManager.Instance.UnloadAll(true);
                    GUIContent progressContent = m_editorUtils.GetContent("TileTerrainMergeDialog");
                    StormEditorProgress.Show(progressContent.text, progressContent.tooltip, numTerrains);
                }
                EditorGUI.EndDisabledGroup();
            }
#endif

            EditorGUILayout.Space();
            m_editorUtils.Label("TileMergeHeader", EditorStyles.boldLabel, helpActive);
            List<StormTileData> overlappingTiles = GetOverlappingTiles();
            m_editorUtils.LabelField("TileMergeCount", new GUIContent(overlappingTiles.Count.ToString()), helpActive);
            EditorGUI.BeginDisabledGroup(overlappingTiles.Count == 0);
            if (m_editorUtils.Button("TileMergeButton", helpActive))
            {
                for (int t = 0; t < targets.Length; ++t)
                {
                    StormTile tile = targets[t] as StormTile;

                    bool[] movedPoly = new bool[tile.TileData.PolygonData.Length];
                    bool[] movedTRS = new bool[tile.TileData.TRSData.Length];
                    bool[] movedTransform = new bool[tile.TileData.TransformData.Length];
                    bool[] movedTexture = new bool[tile.TileData.Textures.Length];
                    bool[] movedTextureColor = new bool[tile.TileData.TextureColors.Length];
                    bool[] movedTerrain = new bool[tile.TileData.TerrainData.Length];
                    StormInternalUtils.MergeTileData(tile.TileData, GetOverlappingTiles(t).ToArray(), editorSettings.TileSplitPolygons, editorSettings.TileSplitTRS, editorSettings.TileSplitTextures, editorSettings.TileSplitTerrain, BlacklistedLayers, ref movedPoly, ref movedTRS, ref movedTransform, ref movedTexture, ref movedTextureColor, ref movedTerrain);
                    if (editorSettings.TileSplitRemoveMerged)
                        RemoveData(tile, movedPoly, movedTexture, movedTextureColor, movedTerrain, movedTRS, movedTransform);
                }
            }
            EditorGUI.EndDisabledGroup();
        }
        private void RemoveData(StormTile tile, bool[] movedPoly, bool[] movedTexture, bool[] movedTextureColor, bool[] movedTerrain, bool[] movedTRS, bool[] movedTransform)
        {
            {
                int num = 0;
                for (int i = 0; i < movedPoly.Length; ++i)
                    if (movedPoly[i])
                        ++num;
                if (num > 0)
                {
                    PolygonData[] newPolys = new PolygonData[movedPoly.Length - num];
                    num = 0;
                    for (int i = 0; i < movedPoly.Length; ++i)
                    {
                        if (!movedPoly[i])
                            newPolys[num++] = tile.TileData.PolygonData[i];
                    }
                    tile.TileData.PolygonData = newPolys;
                }
            }
            {
                int num = 0;
                for (int i = 0; i < movedTexture.Length; ++i)
                {
                    if (movedTexture[i])
                        ++num;
                    else
                    {
                        //Texture might not be removed because of only one layer ... check them against blacklist
                        TextureDataBase tData = tile.TileData.Textures[i];
                        int numLayers = tData.GetLayerCount();
                        for (int l = 0; l < numLayers; ++l)
                        {
                            if (BlacklistedLayers.Contains(tData.GetLayerName(l)))
                                tData.SetLayerName(l, "");
                        }
                    }
                }
                if (num > 0)
                {
                    TextureData[] newTextures = new TextureData[movedTexture.Length - num];
                    num = 0;
                    for (int i = 0; i < movedTexture.Length; ++i)
                    {
                        if (!movedTexture[i])
                            newTextures[num++] = tile.TileData.Textures[i];
                    }
                    tile.TileData.Textures = newTextures;
                }
            }
            {
                int num = 0;
                for (int i = 0; i < movedTextureColor.Length; ++i)
                {
                    if (movedTextureColor[i])
                        ++num;
                    else
                    {
                        //Texture might not be removed because of only one layer ... check them against blacklist
                        TextureDataBase tData = tile.TileData.TextureColors[i];
                        int numLayers = tData.GetLayerCount();
                        for (int l = 0; l < numLayers; ++l)
                        {
                            if (BlacklistedLayers.Contains(tData.GetLayerName(l)))
                                tData.SetLayerName(l, "");
                        }
                    }
                }
                if (num > 0)
                {
                    TextureColorData[] newTextures = new TextureColorData[movedTextureColor.Length - num];
                    num = 0;
                    for (int i = 0; i < movedTextureColor.Length; ++i)
                    {
                        if (!movedTextureColor[i])
                            newTextures[num++] = tile.TileData.TextureColors[i];
                    }
                    tile.TileData.TextureColors = newTextures;
                }
            }
            {
                int num = 0;
                for (int i = 0; i < movedTerrain.Length; ++i)
                    if (movedTerrain[i])
                        ++num;
                if (num > 0)
                {
                    TerrainSourceData[] newTerrains = new TerrainSourceData[movedTerrain.Length - num];
                    num = 0;
                    for (int i = 0; i < movedTerrain.Length; ++i)
                    {
                        if (!movedTerrain[i])
                            newTerrains[num++] = tile.TileData.TerrainData[i];
                    }
                    tile.TileData.TerrainData = newTerrains;
                }
            }
            {
                int num = 0;
                for (int i = 0; i < movedTRS.Length; ++i)
                    if (movedTRS[i])
                        ++num;
                if (num > 0)
                {
                    TRSData[] newTRS = new TRSData[movedTRS.Length - num];
                    num = 0;
                    for (int i = 0; i < movedTRS.Length; ++i)
                    {
                        if (!movedTRS[i])
                            newTRS[num++] = tile.TileData.TRSData[i];
                    }
                    tile.TileData.TRSData = newTRS;
                }
            }
            {
                int num = 0;
                for (int i = 0; i < movedTransform.Length; ++i)
                    if (movedTransform[i])
                        ++num;
                if (num > 0)
                {
                    TransformData[] newTransforms = new TransformData[movedTransform.Length - num];
                    num = 0;
                    for (int i = 0; i < movedTransform.Length; ++i)
                    {
                        if (!movedTransform[i])
                            newTransforms[num++] = tile.TileData.TransformData[i];
                    }
                    tile.TileData.TransformData = newTransforms;
                }
            }
            tile.TileData.UpdateDatasets();
        }
        private List<StormTileData> GetOverlappingTiles(int index = -1)
        {
            List<StormTileData> ret = new List<StormTileData>();
            if (index >= targets.Length)
                return ret;
            StormTile[] Targets = new StormTile[targets.Length];
            for (int t = 0; t < targets.Length; ++t)
                Targets[t] = targets[t] as StormTile;
            StormTile[] AllTiles = StormUtils.FindObjectsOfType<StormTile>();
            for (int i = 0; i < AllTiles.Length; ++i)
            {
                if (index < 0)
                {
                    if (StormUtils.AnyMatch(Targets, AllTiles[i]))
                        continue;
                    for (int t = 0; t < targets.Length; ++t)
                    {
                        if (new Rect(AllTiles[i].TileData.Position.x, AllTiles[i].TileData.Position.z, AllTiles[i].TileData.Size.x, AllTiles[i].TileData.Size.z).Overlaps(new Rect(Targets[t].TileData.Position.x, Targets[t].TileData.Position.z, Targets[t].TileData.Size.x, Targets[t].TileData.Size.z)))
                            ret.Add(AllTiles[i].TileData);
                    }
                }
                else
                {
                    if (Targets[index] == AllTiles[i])
                        continue;
                    if (new Rect(AllTiles[i].TileData.Position.x, AllTiles[i].TileData.Position.z, AllTiles[i].TileData.Size.x, AllTiles[i].TileData.Size.z).Overlaps(new Rect(Targets[index].TileData.Position.x, Targets[index].TileData.Position.z, Targets[index].TileData.Size.x, Targets[index].TileData.Size.z)))
                        ret.Add(AllTiles[i].TileData);
                }
            }
            return ret;
        }
        private void RemoveExisting(List<StormTileData> oldList, List<StormTileData> newList)
        {
            newList.RemoveAll(data => {
                for (int t = 0; t < targets.Length; ++t)
                    if ((targets[t] as StormTile).TileData == data)
                        return true;
                if (oldList.Contains(data))
                    return true;
                return false;
            });
        }
        private void OnDrawBlacklistHeader(Rect rect)
        {
            GUI.Label(rect, m_editorUtils.GetContent("SplitBlacklistHeader"));
        }
        private void OnAddBlacklist(Rect buttonRect, ReorderableList list)
        {
            StormLayerDrawer.ShowLayerDropdown(GUIUtility.GUIToScreenPoint(buttonRect.min), s => {
                if (s == "ADD_NEW_LAYER")
                {
                    int idx = BlacklistedLayers.Count;
                    System.Array.Resize(ref editingLayer, idx + 1);
                    editingLayer[idx] = true;
                    s = "";
                }
                BlacklistedLayers.Add(s);
                StormEditorSettings.Instance.TileBlacklistedLayers = BlacklistedLayers.ToArray();
            });
        }
        private void OnRemoveBlacklist(ReorderableList list)
        {
            if (list.multiSelect)
                for (int i = list.selectedIndices.Count - 1; i >= 0; --i)
                {
                    BlacklistedLayers.RemoveAt(list.selectedIndices[i]);
                }
            else
                BlacklistedLayers.RemoveAt(list.index);
            StormEditorSettings.Instance.TileBlacklistedLayers = BlacklistedLayers.ToArray();
        }
        private void OnDrawLayerListElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            StormLayerDrawer.DrawStormLayerDropdown(rect, null, BlacklistedLayers[index], editingLayer[index],
                s => {
                    if (s == "ADD_NEW_LAYER")
                    {
                        editingLayer[index] = true;
                    }
                    else
                    {
                        BlacklistedLayers[index] = s;
                        StormEditorSettings.Instance.TileBlacklistedLayers = BlacklistedLayers.ToArray();
                    }
                },
                s => {
                    BlacklistedLayers[index] = s;
                    editingLayer[index] = false;
                    StormEditorSettings.Instance.TileBlacklistedLayers = BlacklistedLayers.ToArray();
                });
        }
        private void OnUndo()
        {
            serializedObject.Update();
            serializedObject.ApplyModifiedProperties();
        }
    }
}