using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(BuildingBlockData))]
public class BuildingBlockDataDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        // Draw label
        position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

        // Calculate rects
        var indent = EditorGUI.indentLevel;
        EditorGUI.indentLevel = 0;

        var prefabRect = new Rect(position.x, position.y, position.width * 0.7f, position.height);
        var weightRect = new Rect(position.x + position.width * 0.72f, position.y, position.width * 0.28f, position.height);

        // Create content with tooltips
        GUIContent prefabContent = new GUIContent(GUIContent.none);
        prefabContent.tooltip = "The 3D model prefab to use for this building block variant. Must contain appropriate mesh and materials.";
        
        GUIContent weightContent = new GUIContent(GUIContent.none);
        weightContent.tooltip = "The relative probability of selecting this variant when randomly choosing from the block list. Higher values make this variant more likely to be chosen.";

        // Draw fields
        EditorGUI.PropertyField(prefabRect, property.FindPropertyRelative("prefab"), prefabContent);
        EditorGUI.PropertyField(weightRect, property.FindPropertyRelative("weight"), weightContent);

        EditorGUI.indentLevel = indent;
        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return base.GetPropertyHeight(property, label) * 1f; // Single line height
    }
}