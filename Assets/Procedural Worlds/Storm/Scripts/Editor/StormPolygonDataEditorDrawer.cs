using UnityEditor;
using UnityEngine;

namespace ProceduralWorlds.Storm.StormEditor
{
    [CustomPropertyDrawer(typeof(Utils.PolygonData))]
    public class StormPolygonDataEditorDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float ret = EditorGUIUtility.singleLineHeight;
            if (property.isExpanded)
            {
                int lineCount;
                lineCount = 2;
                SerializedProperty polyProp = property.FindPropertyRelative("Data");
                if (polyProp.isExpanded)
                {
                    int c = polyProp.arraySize;
                    lineCount += c + 1; //"Polygon X/Delete line" for each + "Add Poly" button
                    for (int i = 0; i < c; ++i)
                    {
                        SerializedProperty pointProp = polyProp.GetArrayElementAtIndex(i);
                        if (pointProp.isExpanded)
                        {
                            lineCount += 3; //"Value" + "Falloff" + "Add Point" button
                            SerializedProperty points = pointProp.FindPropertyRelative("_points");
                            if (points != null)
                            {
                                lineCount += points.arraySize;
                            }
                        }
                    }
                }
                SerializedProperty layerProp = property.FindPropertyRelative("_layerName");
                ret += EditorGUI.GetPropertyHeight(layerProp) + ((EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing) * lineCount);
            }
            return ret;
        }
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SerializedProperty layerProp = property.FindPropertyRelative("_layerName");
            SerializedProperty PointsProp = property.FindPropertyRelative("Data");
            SerializedProperty ValuesProp = property.FindPropertyRelative("Values");
            SerializedProperty FalloffsProp = property.FindPropertyRelative("Falloffs");
            SerializedProperty PriorityProp = property.FindPropertyRelative("_priority");
            SerializedProperty ValueRangeProp = property.FindPropertyRelative("_valueRange");

            Rect r = position;
            r.height = EditorGUIUtility.singleLineHeight;
            if (property.isExpanded)
            {
                property.isExpanded = EditorGUI.Foldout(r, property.isExpanded, "", false);
                r.height = EditorGUI.GetPropertyHeight(layerProp);
                EditorGUI.PropertyField(r, layerProp);
            }
            else
            {
                property.isExpanded = EditorGUI.Foldout(r, property.isExpanded, layerProp.stringValue, true);
            }
            r.x += 10;
            if (property.isExpanded)
            {
                r.y += r.height + EditorGUIUtility.standardVerticalSpacing;
                r.height = EditorGUIUtility.singleLineHeight;
                PointsProp.isExpanded = EditorGUI.Foldout(r, PointsProp.isExpanded, "Polygons", true);
                r.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                if (PointsProp.isExpanded)
                {
                    if (ValuesProp.arraySize != PointsProp.arraySize)
                        ValuesProp.arraySize = PointsProp.arraySize;
                    if (FalloffsProp.arraySize != PointsProp.arraySize)
                        FalloffsProp.arraySize = PointsProp.arraySize;
                    int c = PointsProp.arraySize;
                    float rWidth = r.width;
                    float rX = r.x;

                    for (int i = 0; i < c; ++i)
                    {
                        SerializedProperty pointProp = PointsProp.GetArrayElementAtIndex(i);
                        SerializedProperty valueProp = ValuesProp.GetArrayElementAtIndex(i);
                        SerializedProperty falloffProp = FalloffsProp.GetArrayElementAtIndex(i);
                        int lineCount = 1;
                        //calc size for box
                        if (pointProp.isExpanded)
                        {
                            SerializedProperty pProp = pointProp.FindPropertyRelative("_points");
                            lineCount += pProp.arraySize + 3;
                        }
                        r.x = rX - 5;
                        r.width = rWidth + 10;
                        r.height = (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing) * lineCount;
                        GUI.Box(r, "", EditorStyles.helpBox);
                        r.height = EditorGUIUtility.singleLineHeight;

                        r.x = rX + 10;
                        r.width = (rWidth - 10) * 0.5f;
                        pointProp.isExpanded = EditorGUI.Foldout(r, pointProp.isExpanded, "Polygon " + (i + 1), true);
                        r.x += r.width;
                        if (GUI.Button(r, "Delete Polygon"))
                        {
                            PointsProp.DeleteArrayElementAtIndex(i);
                            ValuesProp.DeleteArrayElementAtIndex(i);
                            FalloffsProp.DeleteArrayElementAtIndex(i);
                            if (i >= --c)
                            {
                                r.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                                break;
                            }
                        }
                        r.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                        if (pointProp.isExpanded)
                        {
                            r.width = rWidth - 10;
                            r.x = rX + 10;
                            EditorGUI.BeginChangeCheck();
                            EditorGUI.showMixedValue = valueProp.hasMultipleDifferentValues;
                            float newValue = EditorGUI.FloatField(r, new GUIContent("Value", "The value provided by this polygon"), valueProp.floatValue);
                            if (EditorGUI.EndChangeCheck())
                                valueProp.floatValue = newValue;
                            r.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                            EditorGUI.BeginChangeCheck();
                            EditorGUI.showMixedValue = falloffProp.hasMultipleDifferentValues;
                            float newFalloff = EditorGUI.FloatField(r, new GUIContent("Falloff", "The area around the polygon to fade the edges"), falloffProp.floatValue);
                            if (EditorGUI.EndChangeCheck())
                                falloffProp.floatValue = newFalloff;
                            r.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                            SerializedProperty pProp = pointProp.FindPropertyRelative("_points");
                            int len = pProp.arraySize;
                            for (int p = 0; p < len; ++p)
                            {
                                SerializedProperty point = pProp.GetArrayElementAtIndex(p);
                                r.x = rX + 10;
                                r.width = rWidth + 30;
                                EditorGUI.BeginChangeCheck();
                                EditorGUI.showMixedValue = point.hasMultipleDifferentValues;
                                Vector2 newPoint = EditorGUI.Vector2Field(r, new GUIContent("Point " + (p + 1), "The relatative position of point " + (p + 1)), point.vector2Value);
                                if (EditorGUI.EndChangeCheck())
                                    point.vector2Value = newPoint;
                                EditorGUI.showMixedValue = false;
                                r.x += r.width - 60;
                                r.width = 20;
                                if (GUI.Button(r, "-"))
                                {
                                    pProp.DeleteArrayElementAtIndex(p);
                                    --p;
                                    --len;
                                }
                                r.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                            }
                            r.x = rX + 10;
                            r.width = rWidth - 10;
                            if (GUI.Button(r, "Add Point"))
                            {
                                pProp.InsertArrayElementAtIndex(pProp.arraySize);
                            }
                            r.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                        }
                    }
                    r.x = rX;
                    r.width = rWidth;
                    if (GUI.Button(r, "Add Polygon"))
                    {
                        PointsProp.InsertArrayElementAtIndex(c);
                    }
                    r.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                }
                EditorGUI.PropertyField(r, PriorityProp);
                r.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                EditorGUI.PropertyField(r, ValueRangeProp);
            }
            EditorGUI.showMixedValue = false;
        }
    }
}