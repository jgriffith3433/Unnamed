using UnityEngine;
using UnityEditor;
using System.Linq;

/// <summary>
/// Custom editor for StyleTagDirectoryAsset to provide better UX
/// </summary>
[CustomEditor(typeof(StyleTagDirectoryAssetWrapper))]
public class StyleTagDirectoryEditor : Editor
{
    private SerializedProperty styleTagsProperty;
    private bool showValidation = true;
    
    private void OnEnable()
    {
        styleTagsProperty = serializedObject.FindProperty("styleTags");
    }
    
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Style Tag Directory", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("This asset defines style tags that can be accessed globally at runtime via StyleTagDirectory.GetStyleTag(name).", MessageType.Info);
        
        EditorGUILayout.Space();
        
        // Validation section
        showValidation = EditorGUILayout.Foldout(showValidation, "Validation", true);
        if (showValidation)
        {
            ValidateStyleTags();
        }
        
        EditorGUILayout.Space();
        
        // Style tags list
        EditorGUILayout.LabelField("Style Tags", EditorStyles.boldLabel);
        
        if (styleTagsProperty.arraySize == 0)
        {
            EditorGUILayout.HelpBox("No style tags defined. Click 'Add Style Tag' to create your first one.", MessageType.Warning);
        }
        
        // Add button
        if (GUILayout.Button("Add Style Tag"))
        {
            styleTagsProperty.arraySize++;
            var newElement = styleTagsProperty.GetArrayElementAtIndex(styleTagsProperty.arraySize - 1);
            var nameProperty = newElement.FindPropertyRelative("name");
            var materialsProperty = newElement.FindPropertyRelative("materials");
            
            nameProperty.stringValue = $"StyleTag{styleTagsProperty.arraySize}";
            materialsProperty.ClearArray();
        }
        
        EditorGUILayout.Space();
        
        // Draw style tags
        for (int i = 0; i < styleTagsProperty.arraySize; i++)
        {
            DrawStyleTag(i);
        }
        
        EditorGUILayout.Space();
        
        // Runtime reload button
        if (Application.isPlaying)
        {
            if (GUILayout.Button("Reload Runtime Data"))
            {
                StyleTagDirectory.ReloadStyleTags();
                Debug.Log("StyleTagDirectory reloaded from asset.");
            }
        }
        
        serializedObject.ApplyModifiedProperties();
    }
    
    private void DrawStyleTag(int index)
    {
        var element = styleTagsProperty.GetArrayElementAtIndex(index);
        var nameProperty = element.FindPropertyRelative("name");
        var materialsProperty = element.FindPropertyRelative("materials");
        
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        
        // Header with name and delete button
        EditorGUILayout.BeginHorizontal();
        
        string displayName = string.IsNullOrEmpty(nameProperty.stringValue) ? $"Style Tag {index + 1}" : nameProperty.stringValue;
        EditorGUILayout.LabelField(displayName, EditorStyles.boldLabel);
        
        GUILayout.FlexibleSpace();
        
        if (GUILayout.Button("×", GUILayout.Width(20), GUILayout.Height(20)))
        {
            styleTagsProperty.DeleteArrayElementAtIndex(index);
            return;
        }
        
        EditorGUILayout.EndHorizontal();
        
        // Name field
        EditorGUILayout.PropertyField(nameProperty, new GUIContent("Name"));
        
        // Materials list
        EditorGUILayout.PropertyField(materialsProperty, new GUIContent("Materials"), true);
        
        // Material count info
        if (materialsProperty.isExpanded)
        {
            EditorGUILayout.LabelField($"Material Count: {materialsProperty.arraySize}", EditorStyles.miniLabel);
        }
        
        EditorGUILayout.EndVertical();
        EditorGUILayout.Space();
    }
    
    private void ValidateStyleTags()
    {
        var asset = (StyleTagDirectoryAsset)target;
        bool hasIssues = false;
        
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        
        // Check for duplicate names
        var duplicateNames = asset.styleTags
            .Where(tag => tag != null && !string.IsNullOrEmpty(tag.name))
            .GroupBy(tag => tag.name)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .ToList();
            
        if (duplicateNames.Any())
        {
            hasIssues = true;
            EditorGUILayout.HelpBox($"Duplicate style tag names found: {string.Join(", ", duplicateNames)}", MessageType.Error);
        }
        
        // Check for empty names
        int emptyNameCount = asset.styleTags.Count(tag => tag != null && string.IsNullOrEmpty(tag.name));
        if (emptyNameCount > 0)
        {
            hasIssues = true;
            EditorGUILayout.HelpBox($"{emptyNameCount} style tag(s) have empty names.", MessageType.Warning);
        }
        
        // Check for null entries
        int nullCount = asset.styleTags.Count(tag => tag == null);
        if (nullCount > 0)
        {
            hasIssues = true;
            EditorGUILayout.HelpBox($"{nullCount} null style tag entries found.", MessageType.Error);
        }
        
        // Check for tags with no materials
        int noMaterialsCount = asset.styleTags.Count(tag => tag != null && (tag.materials == null || tag.materials.Count == 0));
        if (noMaterialsCount > 0)
        {
            EditorGUILayout.HelpBox($"{noMaterialsCount} style tag(s) have no materials assigned.", MessageType.Info);
        }
        
        if (!hasIssues)
        {
            EditorGUILayout.HelpBox("All style tags are valid!", MessageType.Info);
        }
        
        EditorGUILayout.EndVertical();
    }
}
