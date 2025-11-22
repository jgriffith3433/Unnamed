using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using UnityEditorInternal;
using Unity.Mathematics;
using ProceduralWorlds.Storm.Utils;
using ProceduralWorlds.Storm.Core;
using Unity.Collections;
using Unity.Jobs;

namespace ProceduralWorlds.Storm.StormEditor
{
    [CustomPropertyDrawer(typeof(Utils.TextureColorData))]
    public class StormTextureColorDataDrawer : PropertyDrawer
    {
        private class ColorElementDrawer
        {
            public SerializedProperty Property;
            public SerializedProperty ColorsProperty;
            public ReorderableList List;

            public bool ShowOverlay;
            public Texture2D PreviewOverlay;

            public ColorElementDrawer(SerializedProperty property)
            {
                Property = property;
                ColorsProperty = property.FindPropertyRelative("m_colors");
                List = new ReorderableList(property.serializedObject, ColorsProperty);
                List.drawElementCallback += DrawElement;
                List.elementHeightCallback += GetElementHeight;
                List.drawHeaderCallback += DrawHeader;
                List.onAddCallback += OnAdd;
                List.onSelectCallback += OnSelect;
                List.multiSelect = false;
            }
            float GetElementHeight(int index)
            {
                SerializedProperty elementProp = ColorsProperty.GetArrayElementAtIndex(index);
                SerializedProperty LayerProp = elementProp.FindPropertyRelative("Layer");
                return 5 * EditorGUIUtility.singleLineHeight + 6 * EditorGUIUtility.standardVerticalSpacing + EditorGUI.GetPropertyHeight(LayerProp) + 4;
            }
            void DrawHeader(Rect rect)
            {
                GUI.Label(rect, "Color Selections", EditorStyles.boldLabel);
            }
            void DrawElement(Rect rect, int index, bool isActive, bool isFocused)
            {
                rect.height -= EditorGUIUtility.standardVerticalSpacing;
                rect.x -= 1;
                rect.width += 2;
                GUI.Box(rect, "", EditorStyles.helpBox);
                rect.x += 2;
                rect.width -= 4;
                rect.y += 2;
                SerializedProperty elementProp = ColorsProperty.GetArrayElementAtIndex(index);

                SerializedProperty LayerProp = elementProp.FindPropertyRelative("Layer");
                SerializedProperty prioProp = elementProp.FindPropertyRelative("Priority");
                SerializedProperty colorProp = elementProp.FindPropertyRelative("Color");
                SerializedProperty rangeProp = elementProp.FindPropertyRelative("ColorRange");
                SerializedProperty falloffProp = elementProp.FindPropertyRelative("ColorFalloff");
                SerializedProperty valueRangeProp = elementProp.FindPropertyRelative("ValueRange");

                EditorGUI.BeginChangeCheck();
                rect.height = EditorGUI.GetPropertyHeight(LayerProp);
                EditorGUI.PropertyField(rect, LayerProp);
                rect.y += rect.height + EditorGUIUtility.standardVerticalSpacing;
                rect.height = EditorGUIUtility.singleLineHeight;
                EditorGUI.PropertyField(rect, prioProp);

                if (EditorGUI.EndChangeCheck() && Core.StormWorld.Instance != null)
                    Core.StormWorld.Instance.Refresh(LayerProp.stringValue);

                EditorGUI.BeginChangeCheck();
                rect.y += rect.height + EditorGUIUtility.standardVerticalSpacing;
                EditorGUI.PropertyField(rect, colorProp);
                rect.y += rect.height + EditorGUIUtility.standardVerticalSpacing;
                EditorGUI.PropertyField(rect, rangeProp);
                rect.y += rect.height + EditorGUIUtility.standardVerticalSpacing;
                EditorGUI.PropertyField(rect, falloffProp);
                rect.y += rect.height + EditorGUIUtility.standardVerticalSpacing;
                EditorGUI.PropertyField(rect, valueRangeProp);

                if (EditorGUI.EndChangeCheck() && Core.StormWorld.Instance != null)
                {
                    Core.StormWorld.Instance.Refresh(LayerProp.stringValue);
                    if (isActive)
                        UpdatePreview();
                }
            }
            void OnAdd(ReorderableList list)
            {
                ColorsProperty.InsertArrayElementAtIndex(ColorsProperty.arraySize);
                if (ColorsProperty.arraySize == 1)
                {
                    SerializedProperty elementProp = ColorsProperty.GetArrayElementAtIndex(0);
                    SerializedProperty colorProp = elementProp.FindPropertyRelative("Color");
                    SerializedProperty crProp = elementProp.FindPropertyRelative("ColorRange");
                    SerializedProperty cfProp = elementProp.FindPropertyRelative("ColorFalloff");
                    SerializedProperty vrProp = elementProp.FindPropertyRelative("ValueRange");
                    colorProp.colorValue = Color.black;
                    crProp.floatValue = 0.2f;
                    cfProp.floatValue = 0.05f;
                    vrProp.vector2Value = Vector2.up;

                }
            }
            void OnSelect(ReorderableList list)
            {
                UpdatePreview();
            }

            public void UpdatePreview()
            {
                if (!ShowOverlay)
                    return;
                SerializedProperty tProp = Property.FindPropertyRelative("_texture");
                if (tProp.isExpanded && List.selectedIndices.Count > 0)
                {
                    SerializedProperty previewColorProp = Property.FindPropertyRelative("_previewColor");
                    int idx = List.selectedIndices[0];
                    if (idx >= 0 && idx < List.count && tProp.objectReferenceValue != null)
                    {
                        Texture2D texture = tProp.objectReferenceValue as Texture2D;
                        int2 imageRes = new int2(texture.width, texture.height);
                        SerializedProperty rectProp = Property.FindPropertyRelative("_rect");
                        int2 outputRes = (int2)math.ceil(math.mul(imageRes, rectProp.rectValue.size));

                        SerializedProperty elementProp = ColorsProperty.GetArrayElementAtIndex(idx);

                        SerializedProperty colorProp = elementProp.FindPropertyRelative("Color");
                        SerializedProperty rangeProp = elementProp.FindPropertyRelative("ColorRange");
                        SerializedProperty falloffProp = elementProp.FindPropertyRelative("ColorFalloff");

                        NativeArray<Color32> m_imageNativeArray = new NativeArray<Color32>(texture.GetPixels32(), Allocator.TempJob);
                        NativeArray<float> m_visualizationNativeArray = new NativeArray<float>(outputRes.x * outputRes.y, Allocator.TempJob);

                        Color c = colorProp.colorValue;

                        PrepareTextureColorJob tJob = new PrepareTextureColorJob() {
                            TextureData = m_imageNativeArray,
                            TextureResolution = imageRes,
                            Rect = new float4(rectProp.rectValue.x, rectProp.rectValue.y, rectProp.rectValue.width, rectProp.rectValue.height),
                            TargetColor = new float4(c.r, c.g, c.b, c.a),
                            TargetRange = rangeProp.floatValue,
                            Falloff = falloffProp.floatValue,
                            Output = m_visualizationNativeArray,
                            OutputResolution = outputRes
                        };
                        tJob.Schedule(outputRes.x * outputRes.y, 128).Complete();
                        Color fadeColor = previewColorProp.colorValue;
                        fadeColor.a = 0;
                        PreviewOverlay = StormUtils.GenerateTexture(m_visualizationNativeArray, 0, 1, fadeColor, previewColorProp.colorValue, outputRes);
                        m_imageNativeArray.Dispose();
                        m_visualizationNativeArray.Dispose();
                    }
                }
            }
        }

        static Dictionary<string, ColorElementDrawer> ColorData = new Dictionary<string, ColorElementDrawer>();
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (!hasInit)
            {
                hasInit = true;
                Selection.selectionChanged -= OnSelectionChanged;
                Selection.selectionChanged += OnSelectionChanged;
                ColorData.Clear();
            }
            if (property.isExpanded)
            {
                float ret = (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing) * 6 + EditorGUIUtility.standardVerticalSpacing;
                
                string id = property.propertyPath;
                if (!ColorData.TryGetValue(id, out ColorElementDrawer colorList))
                {
                    colorList = new ColorElementDrawer(property);
                    ColorData.Add(id, colorList);
                }
                ret += colorList.List.GetHeight();

                SerializedProperty tProp = property.FindPropertyRelative("_texture");
                float areaWidth = (EditorGUIUtility.currentViewWidth * 0.75f);
                if (tProp.objectReferenceValue != null && !(tProp.objectReferenceValue as Texture2D).isReadable)
                {
                    GUIContent message = new GUIContent("The selected Texture is not marked as Readable. In order to use this Texture for Storm data, it needs to be readable.");
                    ret += EditorStyles.wordWrappedLabel.CalcHeight(message, areaWidth - 86) + 4;
                }
                if (tProp.isExpanded)
                    ret += 128;
                return ret;
                }
            return EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
        }
        static bool hasInit = false;
        static void OnSelectionChanged()
        {
            Selection.selectionChanged -= OnSelectionChanged;
            hasInit = false;
            ColorData.Clear();
        }
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            Rect r = position;
            r.height = EditorGUIUtility.singleLineHeight;
            string text = "";
            SerializedProperty colorsProp = property.FindPropertyRelative("m_colors");
            SerializedProperty TargetsProp = property.FindPropertyRelative("m_targetData");
            bool isFirstName = true;
            for (int c = 0; c < colorsProp.arraySize; ++c)
            {
                if (isFirstName)
                    isFirstName = false;
                else
                    text += ", ";
                text += colorsProp.GetArrayElementAtIndex(c).FindPropertyRelative("Layer").stringValue;
            }
            if (isFirstName)
                text = label.text;

            property.isExpanded = EditorGUI.Foldout(r, property.isExpanded, text, true);
            if (property.isExpanded)
            {
                SerializedProperty rectProp = property.FindPropertyRelative("_rect");
                SerializedProperty tProp = property.FindPropertyRelative("_texture");

                bool changed = false;
                string id = property.propertyPath;
                if (!ColorData.TryGetValue(id, out ColorElementDrawer colorList))
                {
                    colorList = new ColorElementDrawer(colorsProp);
                    ColorData.Add(id, colorList);
                }
                {

                    r.y += r.height + EditorGUIUtility.standardVerticalSpacing;
                    Rect fieldRect = r;
                    fieldRect.width -= 105;
                    Rect buttonRect = r;
                    buttonRect.width = 100f;
                    buttonRect.x += fieldRect.width + 5;

                    EditorGUI.BeginChangeCheck();
                    EditorGUI.ObjectField(fieldRect, TargetsProp, typeof(StormColorTargetData));
                    bool shouldUpdateTargets = false;
                    if (EditorGUI.EndChangeCheck())
                        shouldUpdateTargets = true;
                    EditorGUI.BeginDisabledGroup(TargetsProp.objectReferenceValue == null);
                    if (GUI.Button(buttonRect, new GUIContent("Recalculate", "This will re-run the texture through color target ingestion using the provided target list")) || shouldUpdateTargets)
                    {
                        Vector2 valueRange = Vector2.up;
                        if (colorsProp.arraySize > 0)
                            valueRange = colorsProp.GetArrayElementAtIndex(0).FindPropertyRelative("ValueRange").vector2Value;
                        StormColorTargetData targets = TargetsProp.objectReferenceValue as StormColorTargetData;
                        ColorLayerData[] colors = Core.StormInternalUtils.CreateTextureColorData(tProp.objectReferenceValue as Texture2D, valueRange, targets);
                        colorsProp.ClearArray();
                        for (int c = 0; c < colors.Length; ++c)
                        {
                            colorsProp.InsertArrayElementAtIndex(c);
                            SerializedProperty cProp = colorsProp.GetArrayElementAtIndex(c);
                            cProp.FindPropertyRelative("Color").colorValue = colors[c].Color;
                            cProp.FindPropertyRelative("ColorRange").floatValue = colors[c].ColorRange;
                            cProp.FindPropertyRelative("ColorFalloff").floatValue = colors[c].ColorFalloff;
                            cProp.FindPropertyRelative("Layer").stringValue = colors[c].Layer;
                            cProp.FindPropertyRelative("ValueRange").vector2Value = colors[c].ValueRange;
                        }
                        for (int o = 0; o < property.serializedObject.targetObjects.Length; ++o)
                        {
                            TextureColorData tcData = GetData(property, out StormTileData tileData, o);
                            if (tcData != null && tcData.m_targetData != null)
                            {
                                tcData.LastUpdateTS = targets.LastUpdateTS;
                                tcData.UpdateColorData(tcData.m_targetData, tileData);
                            }
                        }
                    }
                    EditorGUI.EndDisabledGroup();
                }

                r.y += r.height + EditorGUIUtility.standardVerticalSpacing;
                r.height = colorList.List.GetHeight();
                colorList.List.DoList(r);
                
                r.y += r.height + EditorGUIUtility.standardVerticalSpacing;
                r.height = EditorGUIUtility.standardVerticalSpacing + (EditorGUIUtility.singleLineHeight * 2);
                r.width = position.width;
                if (rectProp.rectValue == Rect.zero)
                    rectProp.rectValue = new Rect(0, 0, 1, 1);
                EditorGUI.BeginChangeCheck();
                EditorGUI.PropertyField(r, rectProp, new GUIContent("Rect"));
                float MainWidth = position.width * 0.75f;
                float PreviewWidth = position.width - MainWidth;
                Rect PreviewRect = r;
                PreviewRect.height = EditorGUIUtility.singleLineHeight;
                PreviewRect.width = PreviewWidth;
                PreviewRect.x += MainWidth;
                r.width = MainWidth;
                if (tProp.objectReferenceValue != null && !(tProp.objectReferenceValue as Texture2D).isReadable)
                {
                    r.y += r.height + EditorGUIUtility.standardVerticalSpacing + 2;
                    r.x += 2;
                    r.width -= 2;
                    GUIContent message = new GUIContent("The selected Texture is not marked as Readable. In order to use this Texture for Storm data, it needs to be readable.");
                    r.height = EditorStyles.wordWrappedLabel.CalcHeight(message, r.width);
                    Rect boxRect = r;
                    boxRect.x -= 2;
                    boxRect.y -= 2;
                    boxRect.height += 4;
                    boxRect.width = position.width + 2;
                    GUI.Box(boxRect, "", EditorStyles.helpBox);

                    GUI.Label(r, message, EditorStyles.wordWrappedLabel);
                    r.width += 2;
                    r.x -= 2;

                    PreviewRect.y = r.y;
                    PreviewRect.width -= 2;
                    if (GUI.Button(PreviewRect, new GUIContent("Fix", EditorGUIUtility.IconContent("console.warnicon").image)))
                    {
                        StormUtils.MakeReadable(tProp.objectReferenceValue as Texture2D);
                    }
                    PreviewRect.y += r.height + EditorGUIUtility.standardVerticalSpacing;
                    PreviewRect.width += 2;
                }
                r.y += r.height + EditorGUIUtility.standardVerticalSpacing;
                PreviewRect.y = r.y;
                r.height = EditorGUIUtility.singleLineHeight;
                EditorGUI.PropertyField(r, tProp);
                if (EditorGUI.EndChangeCheck() && Core.StormWorld.Instance != null)
                    changed = true;
                bool isValidTexture = tProp.objectReferenceValue != null;
                EditorGUI.BeginDisabledGroup(!isValidTexture);
                bool isExpanded = GUI.Toggle(PreviewRect, tProp.isExpanded, "Preview", EditorStyles.miniButton);
                r.y += r.height + EditorGUIUtility.standardVerticalSpacing;
                r.x = position.x + EditorGUIUtility.labelWidth;
                r.width -= EditorGUIUtility.labelWidth;
                if (!isValidTexture && isExpanded)
                    isExpanded = false;
                if (isExpanded)
                {
                    Texture2D tex = tProp.objectReferenceValue as Texture2D;
                    r.height = math.min(128, r.width);
                    r.width = r.height * (tex.width / tex.height);
                    GUI.Box(r, new GUIContent(""), EditorStyles.helpBox);
                    Rect coords = rectProp.rectValue;
                    //coords.y = (1f - coords.y) - coords.height;
                    GUI.DrawTextureWithTexCoords(r, tex, coords, true);

                    PreviewRect.y += PreviewRect.height + EditorGUIUtility.standardVerticalSpacing;
                    bool noSelection = colorList.List.selectedIndices.Count == 0 || colorList.List.selectedIndices[0] < 0;
                    EditorGUI.BeginDisabledGroup(noSelection);
                    EditorGUI.BeginChangeCheck();
                    colorList.ShowOverlay = GUI.Toggle(PreviewRect, colorList.ShowOverlay, new GUIContent("Overlay?", "Will overlay a display of what data the selected color from the above list will output"), EditorStyles.miniButton) && !noSelection;
                    if (EditorGUI.EndChangeCheck() && colorList.ShowOverlay)
                        colorList.UpdatePreview();
                    EditorGUI.EndDisabledGroup();
                    if (noSelection)
                    {
                        PreviewRect.y += PreviewRect.height + EditorGUIUtility.standardVerticalSpacing;
                        GUIContent message = new GUIContent( "Select a Color above to overlay");
                        PreviewRect.height = EditorStyles.wordWrappedLabel.CalcHeight(message, PreviewRect.width);
                        GUI.Label(PreviewRect, message, EditorStyles.wordWrappedLabel);
                    }
                    if (colorList.ShowOverlay)
                    {
                        PreviewRect.y += PreviewRect.height + EditorGUIUtility.standardVerticalSpacing;
                        PreviewRect.height = EditorGUIUtility.singleLineHeight;
                        GUI.Label(PreviewRect, "Overlay Color");
                        PreviewRect.y += PreviewRect.height + EditorGUIUtility.standardVerticalSpacing;
                        SerializedProperty previewColorProp = property.FindPropertyRelative("_previewColor");
                        EditorGUI.BeginChangeCheck();
                        EditorGUI.PropertyField(PreviewRect, previewColorProp, new GUIContent(""));
                        if (EditorGUI.EndChangeCheck() || (isExpanded && !tProp.isExpanded))
                            colorList.UpdatePreview();

                        PreviewRect.y += PreviewRect.height + EditorGUIUtility.standardVerticalSpacing;
                        if (GUI.Button(PreviewRect, "Invert Color"))
                        {
                            int idx = colorList.List.selectedIndices[0];
                            SerializedProperty elementProp = colorsProp.GetArrayElementAtIndex(idx);
                            SerializedProperty colorProp = elementProp.FindPropertyRelative("Color");
                            Color c = colorProp.colorValue;
                            c.r = 1f - c.r;
                            c.g = 1f - c.g;
                            c.b = 1f - c.b;
                            previewColorProp.colorValue = c;
                            colorList.UpdatePreview();
                        }

                        if (colorList.PreviewOverlay != null)
                            GUI.DrawTexture(r, colorList.PreviewOverlay, ScaleMode.ScaleToFit);
                    }
                }
                tProp.isExpanded = isExpanded;
                EditorGUI.EndDisabledGroup();
                if (changed)
                    for (int i = 0; i < colorsProp.arraySize; ++i)
                        Core.StormWorld.Instance.Refresh(colorsProp.GetArrayElementAtIndex(i).FindPropertyRelative("Layer").stringValue);
            }
        }
        private TextureColorData GetData(SerializedProperty property, out StormTileData tileData, int objectIndex = 0)
        {
            tileData = null;
            int startPos = property.propertyPath.LastIndexOf('[');
            int endPos = property.propertyPath.LastIndexOf(']');
            string indexStr = property.propertyPath.Substring(startPos + 1, endPos - startPos - 1);
            if (int.TryParse(indexStr, out int index))
            {
                StormTile tile = (StormTile)property.serializedObject.targetObjects[objectIndex];
                if (tile != null)
                {
                    tileData = tile.TileData;
                    return tile.TileData.TextureColors[index];
                }
            }
            return null;
        }
    }
}