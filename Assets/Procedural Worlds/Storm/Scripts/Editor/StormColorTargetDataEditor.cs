using UnityEditor;
using UnityEngine;
using ProceduralWorlds.Storm.Utils;
using System.Collections.Generic;
using UnityEditorInternal;
using Unity.Collections;
using Unity.Jobs;
using System;
using System.Collections;
using Unity.Mathematics;
using Unity.Jobs.LowLevel.Unsafe;
using PWCommon5;
using ProceduralWorlds.Storm.Core;
using UnityEditor.Experimental.GraphView;

namespace ProceduralWorlds.Storm.StormEditor
{
    [CustomEditor(typeof(StormColorTargetData))]
    public class StormColorTargetDataEditor : PWEditor
    {
        class ColorSetData
        {
            EditorUtils m_editorUtils;
            SerializedObject m_serializedObject;
            SerializedProperty m_setProp;
            SerializedProperty m_nameProp;
            SerializedProperty m_retargetProp;
            SerializedProperty m_listProp;
            public ReorderableList LayerList;

            Action OnChanged;


            public ColorSetData(EditorUtils editorUtils, SerializedObject serializedObject, SerializedProperty prop, Action onChanged)
            {
                m_editorUtils = editorUtils;
                m_serializedObject = serializedObject;
                m_setProp = prop;
                OnChanged = onChanged;
                m_nameProp = prop.FindPropertyRelative("Name");
                m_listProp = m_setProp.FindPropertyRelative("Colors");
                m_retargetProp = m_setProp.FindPropertyRelative("AllowRetargeting");
                LayerList = new ReorderableList(m_serializedObject, m_listProp, true, true, true, true);
                LayerList.headerHeight = 2 * EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                LayerList.drawHeaderCallback += DrawLayerHeader;
                LayerList.elementHeightCallback += GetLayerElementHeight;
                LayerList.drawElementCallback += DrawLayerElement;
                LayerList.onAddCallback += AddLayerElement;
            }

            float GetLayerElementHeight(int index)
            {
                return 3 * (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing) + EditorGUI.GetPropertyHeight(m_listProp.GetArrayElementAtIndex(index).FindPropertyRelative("Layer"));
            }
            void DrawLayerHeader(Rect r)
            {
                Rect nameRect = r;
                nameRect.height = EditorGUIUtility.singleLineHeight;
                Rect toggleRect = nameRect;
                toggleRect.y += toggleRect.height + EditorGUIUtility.standardVerticalSpacing;
                nameRect.width -= 25;
                Rect buttonRect = nameRect;
                buttonRect.width = 20;
                buttonRect.x = nameRect.xMax + 5;
                if (m_nameProp.isExpanded)
                    m_nameProp.stringValue = EditorGUI.TextField(nameRect, m_nameProp.stringValue);
                else
                    GUI.Label(nameRect, m_nameProp.stringValue);
                m_nameProp.isExpanded = GUI.Toggle(buttonRect, m_nameProp.isExpanded, EditorGUIUtility.IconContent(m_nameProp.isExpanded ? "LockIcon" : "LockIcon-On"), GUI.skin.button);
                EditorGUI.PropertyField(toggleRect, m_retargetProp, m_editorUtils.GetContent("AllowRetargeting"));
            }
            void DrawLayerElement(Rect r, int index, bool active, bool focused)
            {
                r.height = EditorGUIUtility.singleLineHeight;
                SerializedProperty item = m_listProp.GetArrayElementAtIndex(index);
                EditorGUI.BeginChangeCheck();
                SerializedProperty layerProp = item.FindPropertyRelative("Layer");
                r.height = EditorGUI.GetPropertyHeight(layerProp);
                EditorGUI.PropertyField(r, layerProp, new GUIContent());
                r.y += r.height + EditorGUIUtility.standardVerticalSpacing;
                r.height = EditorGUIUtility.singleLineHeight;
                EditorGUI.PropertyField(r, item.FindPropertyRelative("Color"), new GUIContent());
                r.y += r.height + EditorGUIUtility.standardVerticalSpacing;
                r.height = EditorGUIUtility.singleLineHeight;
                EditorGUI.PropertyField(r, item.FindPropertyRelative("Range"), m_editorUtils.GetContent("ColorRange"));
                r.y += r.height + EditorGUIUtility.standardVerticalSpacing;
                r.height = EditorGUIUtility.singleLineHeight;
                EditorGUI.PropertyField(r, item.FindPropertyRelative("Falloff"), m_editorUtils.GetContent("ColorFalloff"));
                if (EditorGUI.EndChangeCheck())
                    OnChanged?.Invoke();
            }
            void AddLayerElement(ReorderableList list)
            {
                int index = list.selectedIndices.Count > 0 ? list.selectedIndices[0] : m_listProp.arraySize;
                m_listProp.InsertArrayElementAtIndex(index);
                if (m_listProp.arraySize == 1)
                {
                    SerializedProperty item = m_listProp.GetArrayElementAtIndex(index);
                    item.FindPropertyRelative("Color").colorValue = Color.white;
                    item.FindPropertyRelative("Range").floatValue = 0.24f;
                    item.FindPropertyRelative("Falloff").floatValue = 0.1f;
                }
            }
        }

        EditorUtils m_editorUtils;

        SerializedProperty ColorSetsProp;
        SerializedProperty ReferenceImageProp;
        SerializedProperty ReferenceColorRangeProp;
        SerializedProperty ReferenceColorFalloffProp;
        ReorderableList ColorSetList;
        ColorSetData[] LayerLists = new ColorSetData[0];

        SerializedProperty LastUpdateTSProp;

        StormCoroutine m_imageCoro;
        ReorderableList m_imageList;
        bool visualizeData = false;
        Color m_visualizeColor = Color.red;
        Texture2D m_visualization;
        NativeArrayRef<float> m_visualizationNativeArray;

        Texture2D m_lastImage;
        float m_lastColorRange;
        float m_lastColorFalloff;
        Color lastVisualizeColor = Color.red;
        int lastVisualizeIndex = -1;

        List<ColorRangeData> m_colorsList;
        NativeArrayRef<Color32> m_imageNativeArray;
        NativeListRef<ColorRangeData> m_colorsNativeList;
        bool IsProcessing = false;
        JobHandle m_imageJob;
        int2 m_imageRes;

        bool HasDataChanged = false;
        Vector2 lastClickPos;

        GUIStyle _centeredLabelStyle;
        GUIStyle CenteredLabelStyle
        {
            get {
                if (_centeredLabelStyle == null)
                {
                    _centeredLabelStyle = new GUIStyle(EditorStyles.boldLabel);
                    _centeredLabelStyle.alignment = TextAnchor.MiddleCenter;
                }
                return _centeredLabelStyle;
            }
        }

        GUIContent _addButton;
        GUIContent AddButton
        {
            get {
                if (_addButton == null)
                    _addButton = EditorGUIUtility.IconContent("d_Toolbar Plus More@2x");
                return _addButton;
            }
        }

        private void OnEnable()
        {
            IsProcessing = false;
            _centeredLabelStyle = null;
            m_colorsList = new List<ColorRangeData>();
            ColorSetsProp = serializedObject.FindProperty("ColorSets");
            ColorSetList = new ReorderableList(serializedObject, ColorSetsProp);
            ColorSetList.drawElementCallback += DrawColorSetElement;
            ColorSetList.elementHeightCallback += GetColorSetElementHeight;
            ColorSetList.drawHeaderCallback += DrawColorSetHeader;
            ColorSetList.onAddCallback += AddColorSetElement;

            ReferenceImageProp = serializedObject.FindProperty("ReferenceImage");
            ReferenceColorRangeProp = serializedObject.FindProperty("ReferenceColorRange");
            ReferenceColorFalloffProp = serializedObject.FindProperty("ReferenceColorFalloff");

            LastUpdateTSProp = serializedObject.FindProperty("LastUpdateTS");
        }
        private void OnDisable()
        {
            if (IsProcessing)
            {
                m_imageJob.Complete();
                IsProcessing = false;
            }
            if (m_imageCoro != null)
                m_imageCoro = null;
            if (m_imageNativeArray != null)
                m_imageNativeArray.Dispose();
            if (m_colorsNativeList != null)
                m_colorsNativeList.Dispose();
            if (m_visualizationNativeArray != null)
                m_visualizationNativeArray.Dispose();
            if (HasDataChanged)
            {
                LastUpdateTSProp.longValue = DateTime.Now.ToFileTime();
                serializedObject.ApplyModifiedProperties();
                for (int o = 0; o < serializedObject.targetObjects.Length; ++o)
                {
                    StormColorTargetData targetData = serializedObject.targetObjects[o] as StormColorTargetData;
                    targetData.OnColorTargetsChanged?.Invoke(targetData, null);
                }
                HasDataChanged = false;
            }
        }

        public override void OnInspectorGUI()
        {
            if (m_editorUtils == null)
                m_editorUtils = StormApp.GetEditorUtils(this);
            m_editorUtils.Initialize();

            if (m_imageCoro != null)
            {
                if (!m_imageCoro.Update())
                {
                    IsProcessing = false;
                    m_imageCoro = null;
                }
                else
                    Repaint();
            }

            ColorSetList.DoLayoutList();
            EditorGUI.BeginDisabledGroup(!HasDataChanged);
            if (m_editorUtils.Button("ApplyChangesButton"))
            {
                LastUpdateTSProp.longValue = DateTime.Now.ToFileTime();
                serializedObject.ApplyModifiedProperties();
                for (int o = 0; o < serializedObject.targetObjects.Length; ++o)
                {
                    StormColorTargetData targetData = serializedObject.targetObjects[o] as StormColorTargetData;
                    targetData.LastUpdateTS = DateTime.Now.ToFileTime();
                    targetData.OnColorTargetsChanged?.Invoke(targetData, null);
                }
                HasDataChanged = false;
            }
            EditorGUI.EndDisabledGroup();
            m_editorUtils.Panel("CreationPanel", CreationPanel, false);
            serializedObject.ApplyModifiedProperties();
        }
        void CreationPanel(bool helpActive)
        {
            if (m_lastImage != ReferenceImageProp.objectReferenceValue)
                UpdateImage();
            EditorGUI.BeginDisabledGroup(IsProcessing);
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label(m_editorUtils.GetContent("ReferenceImage"));
            Texture2D tex = EditorGUILayout.ObjectField(ReferenceImageProp.objectReferenceValue, typeof(Texture2D), false) as Texture2D;
            EditorGUILayout.EndHorizontal();
            float colorRange = m_editorUtils.Slider("ReferenceColorRange", ReferenceColorRangeProp.floatValue, 0.01f, 2f);
            float colorFalloff = m_editorUtils.Slider("ReferenceColorFalloff", ReferenceColorFalloffProp.floatValue, 0.01f, 2f);
            if (tex != ReferenceImageProp.objectReferenceValue || colorRange != ReferenceColorRangeProp.floatValue || colorFalloff != ReferenceColorFalloffProp.floatValue)
            {
                ReferenceImageProp.objectReferenceValue = tex;
                ReferenceColorRangeProp.floatValue = colorRange;
                ReferenceColorFalloffProp.floatValue = colorFalloff;
                UpdateImage();
            }
            if (tex != null)
            {
                float height = tex.height * (400f / tex.width);
                Rect r = GUILayoutUtility.GetRect(400f, height);
                GUI.DrawTexture(r, tex, ScaleMode.ScaleToFit);
                if (visualizeData && m_visualization != null)
                {
                    GUI.DrawTexture(r, m_visualization, ScaleMode.ScaleToFit, true);
                }
                if (IsProcessing)
                    GUI.Label(r, m_editorUtils.GetContent("UpdatingMessage"), CenteredLabelStyle);
            }
            else
            {
                GUILayout.Label(m_editorUtils.GetContent("UpdatingMessage"), CenteredLabelStyle);
            }
            if (m_imageList != null)
            {
                m_imageList.DoLayoutList();
                bool disable = m_imageList.selectedIndices.Count == 0;
                EditorGUI.BeginDisabledGroup(disable);
                bool shouldVisualize = GUILayout.Toggle(visualizeData, "Visualize Data", GUI.skin.button) && !disable;
                Color visualizeColor = EditorGUILayout.ColorField(m_visualizeColor);
                if (shouldVisualize != visualizeData || visualizeColor != m_visualizeColor)
                {
                    visualizeData = shouldVisualize;
                    m_visualizeColor = visualizeColor;
                    UpdateVisualization();
                }
                EditorGUI.EndDisabledGroup();
            }
            EditorGUI.EndDisabledGroup();
        }
        IEnumerator UpdateImageCoro()
        {
            //Debug.Log("Starting Image Update...");
            //double startTime = EditorApplication.timeSinceStartup;
            yield return new WaitUntil(() => m_imageJob.IsCompleted);
            //Debug.Log("Finished Image Update after " + (EditorApplication.timeSinceStartup - startTime) + " seconds");
            m_imageJob.Complete();
            ReadOnlySpan<ColorRangeData> colorSpan = m_colorsNativeList.Array.AsArray().AsReadOnlySpan();
            m_colorsList.Clear();
            m_colorsList.Capacity = colorSpan.Length;
            for (int c = 0; c < colorSpan.Length; ++c)
                m_colorsList.Add(colorSpan[c]);
            if (m_imageList == null)
            {
                m_imageList = new ReorderableList(m_colorsList, typeof(ColorRangeData), false, true, false, false);
                m_imageList.elementHeight = EditorGUIUtility.singleLineHeight;
                m_imageList.drawElementCallback += DrawColorElement;
                m_imageList.drawHeaderCallback += DrawColorHeader;
                m_imageList.onSelectCallback += OnSelectElement;
            }
            else
                m_imageList.list = m_colorsList;
            IsProcessing = false;
            Repaint();
        }
        void UpdateImage()
        {
            Texture2D image = ReferenceImageProp.objectReferenceValue as Texture2D;
            if (!IsProcessing && image != null && (image != m_lastImage || m_lastColorRange != ReferenceColorRangeProp.floatValue || m_lastColorFalloff != ReferenceColorFalloffProp.floatValue))
            {
                m_lastImage = image;
                m_lastColorRange = ReferenceColorRangeProp.floatValue;
                m_lastColorFalloff = ReferenceColorFalloffProp.floatValue;
                m_imageRes = new int2(image.width, image.height);
                if (m_imageNativeArray == null)
                    m_imageNativeArray = new NativeArrayRef<Color32>(StormUtils.MakeReadable(image).GetPixels32(0), null, false, "", Allocator.Persistent);
                else if (m_imageNativeArray.Length == m_imageRes.x * m_imageRes.y)
                    m_imageNativeArray.Array.CopyFrom(StormUtils.MakeReadable(image).GetPixels32(0));
                else
                {
                    m_imageNativeArray.Dispose();
                    m_imageNativeArray = new NativeArrayRef<Color32>(StormUtils.MakeReadable(image).GetPixels32(0), null, false, "", Allocator.Persistent);
                }

                if (m_colorsNativeList == null || !m_colorsNativeList.IsCreated)
                    m_colorsNativeList = new NativeListRef<ColorRangeData>(32, false, "");
                else
                    m_colorsNativeList.Array.Clear();
                ProccessTextureColorJob textureJob = new ProccessTextureColorJob() {
                    Pixels = m_imageNativeArray,
                    ColorRange = m_lastColorRange,
                    ColorFalloff = m_lastColorFalloff,
                    Output = m_colorsNativeList,
                };
                m_imageJob = textureJob.Schedule();
                IsProcessing = true;
                m_imageCoro = new StormCoroutine(UpdateImageCoro());
            }
        }

        IEnumerator UpdateVisualizationCoro()
        {
            //Debug.Log("Starting Visualization Update...");
            //double startTime = EditorApplication.timeSinceStartup;
            yield return new WaitUntil(() => m_imageJob.IsCompleted);
            //Debug.Log("Finished Visualization Update after " + (EditorApplication.timeSinceStartup - startTime) + " seconds");
            m_imageJob.Complete();
            Color clearColor = m_visualizeColor;
            clearColor.a = 0;
            m_visualization = StormUtils.GenerateTexture(m_visualizationNativeArray, 0, 1, clearColor, m_visualizeColor, new int2(m_imageRes.x, m_imageRes.y));
            IsProcessing = false;
            Repaint();
        }
        void UpdateVisualization()
        {
            if (IsProcessing || !visualizeData)
                return;

            var selected = m_imageList.selectedIndices;
            if (selected.Count <= 0)
                return;
            int colorIdx = selected[0];

            if (lastVisualizeColor == m_visualizeColor && lastVisualizeIndex == colorIdx)
                return;
            lastVisualizeIndex = colorIdx;
            lastVisualizeColor = m_visualizeColor;

            int pixelCount = m_imageRes.x * m_imageRes.y;

            if (m_visualizationNativeArray != null && m_visualizationNativeArray.Length != pixelCount)
            {
                m_visualizationNativeArray.Dispose();
                m_visualizationNativeArray = null;
            }

            if (m_visualizationNativeArray == null || !m_visualizationNativeArray.IsCreated)
                m_visualizationNativeArray = new NativeArrayRef<float>(pixelCount, null, false, "");

            IsProcessing = true;
            Color c = m_colorsList[colorIdx].Color;
            PrepareTextureColorJob tJob = new PrepareTextureColorJob() {
                TextureData = m_imageNativeArray,
                TextureResolution = m_imageRes,
                Rect = new float4(0, 0, 1, 1),
                TargetColor = new float4(c.r, c.g, c.b, c.a),
                TargetRange = m_colorsList[colorIdx].Range,
                Falloff = m_colorsList[colorIdx].Falloff,
                Output = m_visualizationNativeArray,
                OutputResolution = m_imageRes
            };

            m_imageJob = tJob.Schedule(pixelCount, pixelCount / Math.Max(1, JobsUtility.JobWorkerCount / 2));
            m_imageCoro = new StormCoroutine(UpdateVisualizationCoro());
        }

        ColorSetData GetSetData(int setIdx)
        {
            if (LayerLists.Length != ColorSetsProp.arraySize)
            {
                Array.Resize(ref LayerLists, ColorSetsProp.arraySize);
            }
            if (setIdx < 0 || setIdx > ColorSetsProp.arraySize)
                return null;
            if (LayerLists[setIdx] == null)
                LayerLists[setIdx] = new ColorSetData(m_editorUtils, serializedObject, ColorSetsProp.GetArrayElementAtIndex(setIdx), OnColorsetChanged);
            return LayerLists[setIdx];
        }
        void OnColorsetChanged()
        {
            HasDataChanged = true;
        }

        float GetColorSetElementHeight(int index)
        {
            return GetSetData(index)?.LayerList.GetHeight() ?? 0;
        }
        void DrawColorSetElement(Rect r, int index, bool active, bool focused)
        {
            GetSetData(index)?.LayerList.DoList(r);
        }
        void DrawColorSetHeader(Rect r)
        {
            GUI.Label(r, m_editorUtils.GetContent("ColorList_Header"), EditorStyles.boldLabel);

        }
        void AddColorSetElement(ReorderableList list)
        {
            int index = list.selectedIndices.Count > 0 ? list.selectedIndices[0] + 1 : ColorSetsProp.arraySize;
            ColorSetsProp.InsertArrayElementAtIndex(index);
            SerializedProperty newColor = ColorSetsProp.GetArrayElementAtIndex(index);
            newColor.FindPropertyRelative("Name").stringValue = "Color Set " + (ColorSetsProp.arraySize);
            newColor.FindPropertyRelative("Colors").ClearArray();
        }

        void DrawColorHeader(Rect rect)
        {
            GUI.Label(rect, m_editorUtils.GetContent("ImageColors_Header"), EditorStyles.boldLabel);
        }
        void DrawColorElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            Rect hRect = rect;
            hRect.width = 13f;
            hRect.x += 2f;
            Rect bRect = rect;
            bRect.x += 20;
            bRect.width = 30;
            rect.width -= 60;
            rect.x += 52;
            Rect cRect = rect;
            cRect.width -= 50f;
            Rect rRect = cRect;
            rRect.x += rRect.width;
            hRect.y += hRect.height * 0.5f - 6f;
            hRect.height = 1f;
            Color hColor = Color.white;
            hColor.a = 0.25f;
            EditorGUI.DrawRect(hRect, hColor);
            hRect.y += 4f;
            EditorGUI.DrawRect(hRect, hColor);
            hRect.y += 4f;
            EditorGUI.DrawRect(hRect, hColor);
            EditorGUI.BeginChangeCheck();
            if (GUI.Button(bRect, AddButton))
            {
                lastClickPos = GUIUtility.GUIToScreenPoint(Event.current.mousePosition);
                GUIContent[] setList = new GUIContent[ColorSetsProp.arraySize + 1];
                setList[0] = m_editorUtils.GetContent("AddColorSetNew");
                for (int s = 0; s < ColorSetsProp.arraySize; ++s)
                    setList[s + 1] = new GUIContent(ColorSetsProp.GetArrayElementAtIndex(s).FindPropertyRelative("Name").stringValue);
                EditorUtility.DisplayCustomMenu(bRect, setList, -1, OnSelectAddOption, index);
            }
            Color newColor = EditorGUI.ColorField(cRect, m_colorsList[index].Color);
            GUI.Label(rRect, m_colorsList[index].Range.ToString("F2"));
            if (EditorGUI.EndChangeCheck())
            {
                m_colorsList[index] = new ColorRangeData(newColor, m_colorsList[index].Range, m_colorsList[index].Falloff);
                if (lastVisualizeIndex == index && visualizeData)
                {
                    lastVisualizeIndex = -1;
                    UpdateVisualization();
                }
            }
        }
        void OnSelectAddOption(object userData, string[] options, int selected)
        {
            int index = (int)userData;
            StormWorld stormWorld = StormWorld.Instance;
            if (stormWorld != null)
            {
                ColorRangeData selectedColor = m_colorsList[index];
                StormLayerSet globalLayerSet = stormWorld.GlobalLayerSet;

                List<string> layers = new List<string>(globalLayerSet.Layers);
                List<StormLayerType> layerTypes = new List<StormLayerType>(globalLayerSet.LayerTypes);

                StormLayerSearchProvider searchProvider =
                    ScriptableObject.CreateInstance<StormLayerSearchProvider>();
                searchProvider.layers = layers.ToArray();
                searchProvider.layerTypes = layerTypes.ToArray();
                searchProvider.onSelectedCallback = result => {
                    if (result == "ADD_NEW_LAYER")
                        result = "\nADD_NEW_LAYER\n";
                    SerializedProperty ColorListProp;
                    if (selected == 0)
                    {
                        selected = ColorSetsProp.arraySize + 1;
                        ColorSetsProp.InsertArrayElementAtIndex(ColorSetsProp.arraySize);
                        SerializedProperty colorSet = ColorSetsProp.GetArrayElementAtIndex(selected - 1);
                        colorSet.FindPropertyRelative("Name").stringValue = "Color Set " + selected;
                        ColorListProp = colorSet.FindPropertyRelative("Colors");
                        ColorListProp.ClearArray();
                    }
                    else
                        ColorListProp = ColorSetsProp.GetArrayElementAtIndex(selected - 1).FindPropertyRelative("Colors");
                    int idx = ColorListProp.arraySize;
                    ColorListProp.InsertArrayElementAtIndex(idx);
                    SerializedProperty newColorProp = ColorListProp.GetArrayElementAtIndex(idx);
                    SerializedProperty colorProp = newColorProp.FindPropertyRelative("Color");
                    SerializedProperty layerProp = newColorProp.FindPropertyRelative("Layer");
                    SerializedProperty rangeProp = newColorProp.FindPropertyRelative("Range");
                    SerializedProperty falloffProp = newColorProp.FindPropertyRelative("Falloff");
                    colorProp.colorValue = selectedColor.Color;
                    layerProp.stringValue = result;
                    rangeProp.floatValue = selectedColor.Range;
                    falloffProp.floatValue = selectedColor.Falloff;

                    OnColorsetChanged();
                };
                var screenPosition = lastClickPos;
                SearchWindow.Open(new SearchWindowContext(screenPosition), searchProvider);
            }
        }
        void OnSelectElement(ReorderableList list)
        {
            UpdateVisualization();
        }
    }
}