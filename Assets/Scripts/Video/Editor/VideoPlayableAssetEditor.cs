#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.Video;
using UnityEditor;
using UnityEngine.Playables;

[CustomEditor(typeof(VideoPlayableAsset))]
public class VideoPlayableAssetEditor : Editor
{
    private bool isExtracting = false;

    public override void OnInspectorGUI()
    {
        VideoPlayableAsset asset = (VideoPlayableAsset)target;
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Video Settings", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        // Video Player reference
        SerializedProperty videoPlayerProp = serializedObject.FindProperty("videoPlayer");
        EditorGUILayout.PropertyField(videoPlayerProp, new GUIContent("Video Player"));
        
        // Preview FPS
        SerializedProperty fpsProp = serializedObject.FindProperty("previewFPS");
        EditorGUILayout.PropertyField(fpsProp, new GUIContent("Preview FPS", "FPS for frame extraction and preview"));

        serializedObject.ApplyModifiedProperties();

        // Get the actual VideoPlayer from the exposed reference
        var director = FindPlayableDirector();
        VideoPlayer videoPlayer = null;
        
        if (director != null)
        {
            videoPlayer = asset.videoPlayer.Resolve(director);
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Frame Extraction", EditorStyles.boldLabel);

        // Check if we have a video player and clip
        if (videoPlayer == null || videoPlayer.clip == null)
        {
            EditorGUILayout.HelpBox("Assign a VideoPlayer with a video clip to enable automatic frame extraction.", MessageType.Info);
            return;
        }

        // Get auto frame path
        string framePath = asset.GetAutoFramePath(videoPlayer);
        bool framesExist = asset.CheckIfFramesExist(videoPlayer);
        
        // Display frame info
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        {
            EditorGUILayout.LabelField("Video:", videoPlayer.clip.name);
            EditorGUILayout.LabelField("Frame Path:", framePath);
            
            if (framesExist)
            {
                int frameCount = asset.preloadedFrames != null ? asset.preloadedFrames.Length : 0;
                EditorGUILayout.LabelField("Status:", $"✓ {frameCount} frames loaded", EditorStyles.boldLabel);
            }
            else
            {
                EditorGUILayout.LabelField("Status:", "No frames extracted", EditorStyles.boldLabel);
            }
        }
        EditorGUILayout.EndVertical();

        EditorGUILayout.Space();

        // Show appropriate buttons based on state
        GUI.enabled = !isExtracting;
        
        if (framesExist && (asset.preloadedFrames == null || asset.preloadedFrames.Length == 0))
        {
            // Frames exist but not loaded
            EditorGUILayout.BeginHorizontal();
            {
                if (GUILayout.Button("Load Existing Frames", GUILayout.Height(40)))
                {
                    asset.AutoLoadFrames(videoPlayer);
                    EditorUtility.DisplayDialog("Success", $"Loaded {asset.preloadedFrames.Length} frames", "OK");
                }
                
                if (GUILayout.Button("Re-Extract", GUILayout.Height(40), GUILayout.Width(100)))
                {
                    if (EditorUtility.DisplayDialog("Re-Extract Frames", 
                        "Delete existing frames and extract new ones?", 
                        "Yes", "Cancel"))
                    {
                        if (AssetDatabase.IsValidFolder(framePath))
                        {
                            AssetDatabase.DeleteAsset(framePath);
                            AssetDatabase.Refresh();
                        }
                        ExtractAndLoadFrames(asset, videoPlayer, framePath);
                    }
                }
            }
            EditorGUILayout.EndHorizontal();
        }
        else if (!framesExist)
        {
            // No frames exist - extract them
            if (GUILayout.Button("Extract Frames Automatically", GUILayout.Height(40)))
            {
                ExtractAndLoadFrames(asset, videoPlayer, framePath);
            }
        }
        else
        {
            // Frames are loaded
            EditorGUILayout.BeginHorizontal();
            {
                if (GUILayout.Button("Reload Frames", GUILayout.Height(30)))
                {
                    asset.AutoLoadFrames(videoPlayer);
                    EditorUtility.DisplayDialog("Success", $"Reloaded {asset.preloadedFrames.Length} frames", "OK");
                }
                
                if (GUILayout.Button("Re-Extract Frames", GUILayout.Height(30)))
                {
                    if (EditorUtility.DisplayDialog("Re-Extract Frames", 
                        "This will delete existing frames and extract new ones. Continue?", 
                        "Yes", "Cancel"))
                    {
                        // Delete existing frames
                        if (AssetDatabase.IsValidFolder(framePath))
                        {
                            AssetDatabase.DeleteAsset(framePath);
                            AssetDatabase.Refresh();
                        }
                        
                        ExtractAndLoadFrames(asset, videoPlayer, framePath);
                    }
                }
            }
            EditorGUILayout.EndHorizontal();
        }
        
        GUI.enabled = true;

        if (isExtracting)
        {
            EditorGUILayout.HelpBox("Extracting frames... Please wait.", MessageType.Info);
        }

        // Show frame array for debugging (collapsed by default)
        EditorGUILayout.Space();
        SerializedProperty framesProp = serializedObject.FindProperty("preloadedFrames");
        framesProp.isExpanded = EditorGUILayout.Foldout(framesProp.isExpanded, "Preloaded Frames (Advanced)");
        
        if (framesProp.isExpanded)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(framesProp, true);
            EditorGUI.indentLevel--;
            serializedObject.ApplyModifiedProperties();
        }
    }

    private void ExtractAndLoadFrames(VideoPlayableAsset asset, VideoPlayer videoPlayer, string framePath)
    {
        isExtracting = true;
        Repaint();

        // Get video clip path
        string videoPath = AssetDatabase.GetAssetPath(videoPlayer.clip);
        
        // Try FFmpeg first (much faster), fallback to VideoPlayer method
        FFmpegFrameExtractor.ExtractFrames(videoPath, asset.previewFPS, framePath, (success, message) =>
        {
            if (success)
            {
                isExtracting = false;
                // Auto-load the extracted frames
                asset.AutoLoadFrames(videoPlayer);
                EditorUtility.DisplayDialog("Success", $"Extracted and loaded {asset.preloadedFrames.Length} frames using FFmpeg!", "OK");
                Repaint();
            }
            else
            {
                // FFmpeg failed, try VideoPlayer method
                if (message.Contains("FFmpeg not found"))
                {
                    // Show FFmpeg installation option
                    if (EditorUtility.DisplayDialog("FFmpeg Not Found", 
                        message + "\n\nWould you like to use the slower VideoPlayer extraction method instead?", 
                        "Yes, Use VideoPlayer", "Cancel"))
                    {
                        ExtractWithVideoPlayer(asset, videoPlayer, framePath);
                    }
                    else
                    {
                        isExtracting = false;
                        Repaint();
                    }
                }
                else
                {
                    isExtracting = false;
                    EditorUtility.DisplayDialog("Error", $"Frame extraction failed: {message}", "OK");
                    Repaint();
                }
            }
        });
    }
    
    private void ExtractWithVideoPlayer(VideoPlayableAsset asset, VideoPlayer videoPlayer, string framePath)
    {
        VideoFrameExtractor.ExtractFrames(videoPlayer, asset.previewFPS, framePath, (success, message) =>
        {
            isExtracting = false;
            
            if (success)
            {
                // Auto-load the extracted frames
                asset.AutoLoadFrames(videoPlayer);
                EditorUtility.DisplayDialog("Success", $"Extracted and loaded {asset.preloadedFrames.Length} frames!", "OK");
            }
            else
            {
                EditorUtility.DisplayDialog("Error", $"Frame extraction failed: {message}", "OK");
            }
            
            Repaint();
        });
    }

    private PlayableDirector FindPlayableDirector()
    {
        // Try to find the PlayableDirector that contains this asset
        var directors = FindObjectsOfType<PlayableDirector>();
        
        foreach (var director in directors)
        {
            if (director.playableAsset != null)
            {
                // Check if this Timeline contains our asset
                var timelineAsset = director.playableAsset as UnityEngine.Timeline.TimelineAsset;
                if (timelineAsset != null)
                {
                    foreach (var track in timelineAsset.GetOutputTracks())
                    {
                        var videoTrack = track as VideoTrack;
                        if (videoTrack != null)
                        {
                            foreach (var clip in videoTrack.GetClips())
                            {
                                if (clip.asset == target)
                                {
                                    return director;
                                }
                            }
                        }
                    }
                }
            }
        }
        
        return null;
    }
}
#endif
