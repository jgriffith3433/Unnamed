using UnityEditor;
using UnityEngine;
using ProceduralWorlds.Storm.BuildingGen;

[CustomPropertyDrawer(typeof(RoofBlocksData))]
public class RoofBlocksDataDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        // Draw label
        position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

        // Calculate rects
        var indent = EditorGUI.indentLevel;
        EditorGUI.indentLevel = 0;

        var blocksRect = new Rect(position.x, position.y, position.width * 0.7f, position.height);
        var weightRect = new Rect(position.x + position.width * 0.72f, position.y, position.width * 0.28f, position.height);

        // Create content with tooltips
        GUIContent roofBlocksContent = new GUIContent(GUIContent.none);
        roofBlocksContent.tooltip = "The roof blocks asset that defines the visual style for roof segments, peaks, corners, and trim elements.";
        
        GUIContent weightContent = new GUIContent(GUIContent.none);
        weightContent.tooltip = "The relative probability of selecting this roof style during random generation. Higher values make this style more likely to be chosen.";

        // Draw fields - 70% width for blocks, 30% for weight
        EditorGUI.PropertyField(blocksRect, property.FindPropertyRelative("roofBlocks"), roofBlocksContent);
        EditorGUI.PropertyField(weightRect, property.FindPropertyRelative("weight"), weightContent);

        EditorGUI.indentLevel = indent;
        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return base.GetPropertyHeight(property, label) * 1f; // Single line height
    }
}