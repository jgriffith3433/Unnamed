using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

/// <summary>
/// Editor window to extract animation clips from model files and convert Legacy to Generic
/// </summary>
public class AnimationExtractorWindow : EditorWindow
{
    private List<GameObject> selectedModels = new List<GameObject>();
    private string outputFolder = "Assets/Animations/Extracted";
    private Vector2 scrollPosition;
    private Vector2 modelScrollPosition;
    private List<AnimationClipInfo> clipInfos = new List<AnimationClipInfo>();

    private class AnimationClipInfo
    {
        public string name;
        public bool selected = true;
        public AnimationClip clip;
        public string sourceName;
        public bool isLegacy;
    }

    [MenuItem("Tools/Animation Extractor")]
    public static void ShowWindow()
    {
        GetWindow<AnimationExtractorWindow>("Animation Extractor");
    }

    private void OnGUI()
    {
        GUILayout.Label("Extract Animation Clips from Model(s)", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        EditorGUILayout.LabelField("Model Files:", EditorStyles.boldLabel);
        
        EditorGUILayout.BeginVertical(GUI.skin.box);
        modelScrollPosition = EditorGUILayout.BeginScrollView(modelScrollPosition, GUILayout.Height(100));
        
        for (int i = 0; i < selectedModels.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();
            selectedModels[i] = (GameObject)EditorGUILayout.ObjectField(selectedModels[i], typeof(GameObject), false);
            if (GUILayout.Button("X", GUILayout.Width(25)))
            {
                selectedModels.RemoveAt(i);
                RefreshAnimationList();
                i--;
            }
            EditorGUILayout.EndHorizontal();
        }
        
        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Add Model"))
        {
            selectedModels.Add(null);
        }
        if (GUILayout.Button("Clear All"))
        {
            selectedModels.Clear();
            RefreshAnimationList();
        }
        if (GUILayout.Button("Refresh List"))
        {
            RefreshAnimationList();
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();
        outputFolder = EditorGUILayout.TextField("Output Folder", outputFolder);

        if (GUILayout.Button("Browse..."))
        {
            string path = EditorUtility.OpenFolderPanel("Select Output Folder", "Assets", "");
            if (!string.IsNullOrEmpty(path))
            {
                // Convert absolute path to relative
                if (path.StartsWith(Application.dataPath))
                {
                    outputFolder = "Assets" + path.Substring(Application.dataPath.Length);
                }
            }
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        EditorGUILayout.Space();

        if (clipInfos.Count > 0)
        {
            EditorGUILayout.LabelField($"Found {clipInfos.Count} animation clip(s) from {selectedModels.FindAll(m => m != null).Count} model(s):", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Select All"))
            {
                foreach (var info in clipInfos)
                    info.selected = true;
            }
            if (GUILayout.Button("Deselect All"))
            {
                foreach (var info in clipInfos)
                    info.selected = false;
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            
            foreach (var info in clipInfos)
            {
                EditorGUILayout.BeginHorizontal();
                info.selected = EditorGUILayout.Toggle(info.selected, GUILayout.Width(20));
                EditorGUILayout.LabelField(info.name, GUILayout.MinWidth(150));
                EditorGUILayout.LabelField($"({info.clip.length:F2}s)", GUILayout.Width(60));
                if (info.isLegacy)
                {
                    EditorGUILayout.LabelField("Legacy", EditorStyles.boldLabel, GUILayout.Width(50));
                }
                EditorGUILayout.LabelField($"[{info.sourceName}]", EditorStyles.miniLabel);
                EditorGUILayout.EndHorizontal();
            }
            
            EditorGUILayout.EndScrollView();

            EditorGUILayout.Space();
            
            GUI.enabled = clipInfos.FindAll(c => c.selected).Count > 0;
            if (GUILayout.Button("Extract Selected Clips", GUILayout.Height(30)))
            {
                ExtractSelectedClips();
            }
            GUI.enabled = true;
        }
        else if (selectedModels.FindAll(m => m != null).Count > 0)
        {
            EditorGUILayout.HelpBox("No animation clips found in the selected model(s).", MessageType.Warning);
        }
        else
        {
            EditorGUILayout.HelpBox("Add model files with animations to extract.", MessageType.Info);
        }
    }

    private void RefreshAnimationList()
    {
        clipInfos.Clear();

        foreach (var model in selectedModels)
        {
            if (model == null)
                continue;

            string assetPath = AssetDatabase.GetAssetPath(model);
            Object[] objects = AssetDatabase.LoadAllAssetsAtPath(assetPath);

            foreach (Object obj in objects)
            {
                if (obj is AnimationClip clip)
                {
                    // Skip the __preview__ clip
                    if (clip.name.Contains("__preview__"))
                        continue;

                    clipInfos.Add(new AnimationClipInfo
                    {
                        name = clip.name,
                        clip = clip,
                        selected = true,
                        sourceName = model.name,
                        isLegacy = clip.legacy
                    });
                }
            }
        }

        clipInfos.Sort((a, b) => a.name.CompareTo(b.name));
    }

    private void ExtractSelectedClips()
    {
        if (clipInfos.Count == 0)
        {
            EditorUtility.DisplayDialog("Error", "No clips to extract!", "OK");
            return;
        }

        // Create output directory if it doesn't exist
        if (!AssetDatabase.IsValidFolder(outputFolder))
        {
            string[] folders = outputFolder.Split('/');
            string currentPath = folders[0];
            
            for (int i = 1; i < folders.Length; i++)
            {
                string newPath = currentPath + "/" + folders[i];
                if (!AssetDatabase.IsValidFolder(newPath))
                {
                    AssetDatabase.CreateFolder(currentPath, folders[i]);
                }
                currentPath = newPath;
            }
        }

        int extractedCount = 0;
        int convertedCount = 0;
        
        foreach (var info in clipInfos)
        {
            if (!info.selected)
                continue;

            string outputPath = $"{outputFolder}/{info.name}.anim";
            
            // Check if file already exists
            if (File.Exists(outputPath))
            {
                if (!EditorUtility.DisplayDialog("File Exists", 
                    $"Animation '{info.name}' already exists at:\n{outputPath}\n\nOverwrite?", 
                    "Yes", "Skip"))
                {
                    continue;
                }
            }

            AnimationClip newClip;

            if (info.isLegacy)
            {
                // Convert Legacy to Generic while preserving keyframes
                newClip = ConvertLegacyToGeneric(info.clip);
                convertedCount++;
            }
            else
            {
                // Create a copy of the animation clip
                newClip = new AnimationClip();
                EditorUtility.CopySerialized(info.clip, newClip);
            }
            
            AssetDatabase.CreateAsset(newClip, outputPath);
            extractedCount++;
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        string message = $"Extracted {extractedCount} animation clip(s)";
        if (convertedCount > 0)
        {
            message += $"\nConverted {convertedCount} Legacy clip(s) to Generic";
        }
        message += $"\n\nSaved to: {outputFolder}";

        EditorUtility.DisplayDialog("Success", message, "OK");
        
        // Ping the output folder
        Object folderObj = AssetDatabase.LoadAssetAtPath<Object>(outputFolder);
        EditorGUIUtility.PingObject(folderObj);
    }

    private AnimationClip ConvertLegacyToGeneric(AnimationClip legacyClip)
    {
        // Create a new non-legacy clip
        AnimationClip genericClip = new AnimationClip();
        genericClip.legacy = false;
        genericClip.name = legacyClip.name;
        genericClip.frameRate = legacyClip.frameRate;
        genericClip.wrapMode = legacyClip.wrapMode;

        // Get all curve bindings from the legacy clip
        EditorCurveBinding[] curveBindings = AnimationUtility.GetCurveBindings(legacyClip);
        
        // Copy each curve to the new clip
        foreach (EditorCurveBinding binding in curveBindings)
        {
            AnimationCurve curve = AnimationUtility.GetEditorCurve(legacyClip, binding);
            if (curve != null)
            {
                AnimationUtility.SetEditorCurve(genericClip, binding, curve);
            }
        }

        // Copy object reference curves (for object references like sprites)
        EditorCurveBinding[] objectBindings = AnimationUtility.GetObjectReferenceCurveBindings(legacyClip);
        foreach (EditorCurveBinding binding in objectBindings)
        {
            ObjectReferenceKeyframe[] keyframes = AnimationUtility.GetObjectReferenceCurve(legacyClip, binding);
            if (keyframes != null)
            {
                AnimationUtility.SetObjectReferenceCurve(genericClip, binding, keyframes);
            }
        }

        // Copy animation events
        AnimationEvent[] events = AnimationUtility.GetAnimationEvents(legacyClip);
        AnimationUtility.SetAnimationEvents(genericClip, events);

        return genericClip;
    }
}
