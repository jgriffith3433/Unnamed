using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor.Build;
using UnityEditor;
#endif
 
#if PW_STORM_HDRP || PW_STORM_URP
using UnityEngine.Rendering;
#endif
#if PW_STORM_HDRP
using UnityEngine.Rendering.HighDefinition;
#endif
#if PW_STORM_URP
using UnityEngine.Rendering.Universal;
#endif
using Object = UnityEngine.Object;

/// Copyright 2023 Procedural Worlds Ltd
/// Written by Jonathan Williams as part of Storm

namespace ProceduralWorlds.Storm.Utils
{
    public enum StormRP
    {
        Builtin,
        URP,
        HDRP
    }
    public enum StormTextureExportFormat
    {
        EXR,
        PNG,
        TGA,
    }
#if UNITY_EDITOR
    /// <summary>
    /// use static functions to check that the script defines are present in the project
    /// </summary>
    [InitializeOnLoad]
    public static class StormDefineCheckUtil
    {
        static StormDefineCheckUtil()
        {
            StormUtils.DoDefineCheck();
        }
    }
#endif
    public static class StormUtils
    {

        public const string DefaultSavePath = "Assets/Storm User Data/";
        static char[] resourceSplit = new char[] { ':' };

        public static uint4 GetNewGUID()
        {
            byte[] data = Guid.NewGuid().ToByteArray();
            uint4 ret = new uint4(BitConverter.ToUInt32(data, 0), BitConverter.ToUInt32(data, 4), BitConverter.ToUInt32(data, 8), BitConverter.ToUInt32(data, 12));
            return ret;
        }

        public static string GetReadableFileSize(ulong bytes)
        {
            string ret;
            float val;

            if (bytes < 1024L) // < 1KB
            {
                val = bytes;
                ret = "{0:N2} B";
            }
            else if (bytes < 1048576L) // < 1MB (1024^2)
            {
                val = bytes / 1024f;
                ret = "{0:N2} KB";
            }
            else if (bytes < 1073741824L) // < 1GB (1024^3)
            {
                val = bytes / 1048576f;
                ret = "{0:N2} MB";
            }
            else if (bytes < 1099511627776L) // < 1TB (1024^4)
            {
                val = bytes / 1073741824f;
                ret = "{0:N2} GB";
            }
            else if (bytes < 1125899906842624L) // < 1PB (1024^5)
            {
                val = bytes / 1099511627776f;
                ret = "{0:N2} TB";
            }
            else if (bytes < 1152921504606846976L) // < 1EB (1024^5)
            {
                val = bytes / 1125899906842624f;
                ret = "{0:N2} PB";
            }
            else
            {
                val = bytes / 1152921504606846976f;
                ret = "{0:N2} EB";
            }

            return string.Format(ret, val);
        }

        public static uint WrapShift(uint value, int offset)
        {
            offset %= 32;
            if (offset < 0)
                offset += 32;
            return (value << offset) | (value >> (32 - offset));
        }
        public static int WrapShift(int value, int offset)
        {
            offset %= 32;
            if (offset < 0)
                offset += 32;
            return (int)((((uint)value) << offset) | (((uint)value) >> (32 - offset)));
        }
        public static ushort WrapShift(ushort value, int offset)
        {
            offset %= 16;
            if (offset < 0)
                offset += 16;
            return (ushort)((((uint)value) << offset) | (((uint)value) >> (32 - offset)));
        }
        public static short WrapShift(short value, int offset)
        {
            offset %= 16;
            if (offset < 0)
                offset += 16;
            return (short)((((uint)value) << offset) | (((uint)value) >> (32 - offset)));
        }
        static StormRP m_lastRP;
        static bool m_retrievedRP;
        public static StormRP GetRenderPipline()
        {
            if (m_retrievedRP)
                return m_lastRP;
            /*
            Type WizardSettingsType = Type.GetType("ProceduralWorlds.Storm.Wizard.StormWizardSettings, Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
            Type WizardUtilsType = Type.GetType("ProceduralWorlds.Storm.Wizard.StormWizardUtils, Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
            if (WizardUtilsType != null && WizardSettingsType != null)
            {
                MethodInfo getSettingsMethod = WizardUtilsType.GetMethod("GetStormWizardSettings", BindingFlags.Static | BindingFlags.Public);
                FieldInfo rpField = WizardSettingsType.GetField("m_targetRenderPipeline", BindingFlags.NonPublic | BindingFlags.Instance);
                object settingsObj = getSettingsMethod.Invoke(null, new object[0]);
                if (settingsObj != null)
                {
                    StormRP rpValue = (StormRP)rpField.GetValue(settingsObj);
                    m_lastRP = rpValue;
                    m_retrievedRP = true;
                    return rpValue;
                }
            }
            //*/
#if PW_STORM_HDRP || PW_STORM_URP
#if UNITY_6000_0_OR_NEWER
            if (GraphicsSettings.defaultRenderPipeline == null)
            {
                m_lastRP = StormRP.Builtin;
                m_retrievedRP = true;
               return StormRP.Builtin;
            }
            string pipelineName = GraphicsSettings.defaultRenderPipeline.GetType().ToString().ToLower();
#else
            if (GraphicsSettings.renderPipelineAsset == null)
            {
                m_lastRP = StormRP.Builtin;
                m_retrievedRP = true;
                return StormRP.Builtin;
            }
            string pipelineName = GraphicsSettings.renderPipelineAsset.GetType().ToString().ToLower();
#endif
            if (pipelineName.Contains("highdefinition") || pipelineName.Contains("hdrp"))
            {
                m_lastRP = StormRP.HDRP;
                m_retrievedRP = true;
                return StormRP.HDRP;
            }
            else
            {
                m_lastRP = StormRP.URP;
                m_retrievedRP = true;
                return StormRP.URP;
            }
#else
            m_lastRP = StormRP.Builtin;
            m_retrievedRP = true;
            return StormRP.Builtin;
#endif
        }

        public static void DoDefineCheck()
        {
#if UNITY_EDITOR
            AddEditorOnRecompile(DoDefineCheck);
            //Adds the detailer script define
            bool updateScripting = false;
            PlayerSettings.GetScriptingDefineSymbols(NamedBuildTarget.FromBuildTargetGroup(EditorUserBuildSettings.selectedBuildTargetGroup), out string[] symbolsArray);
            List<string> symbols = new List<string>(symbolsArray);
            AddDefine(symbols, ref updateScripting, "PW_STORM_PRESENT");
            if (updateScripting)
            {
                if (!EditorPrefs.HasKey("UnityEditor.ShaderGraph.VariantLimit") || EditorPrefs.GetInt("UnityEditor.ShaderGraph.VariantLimit") < 256)
                {
                    if (EditorUtility.DisplayDialog("Increase Shader Graph Variant Limit", "The shader graph variant limit is set below 256 for this machine in the unity preferences. Storm needs the limit to be increased to 256 or higher for certain features to work correctly across all rendering pipelines. Do you want to increase the limit to 256 now? (Recommended)", "Yes, set to 256", "No thanks"))
                    {
                        EditorPrefs.SetInt("UnityEditor.ShaderGraph.VariantLimit", 256);
                    }
                }
            }

            //Cleanup old defines
            RemoveDefine(symbols, ref updateScripting, "STORM2023_PRESENT");
            RemoveDefine(symbols, ref updateScripting, "PW_STORM_1_0_6");
            RemoveDefine(symbols, ref updateScripting, "STORM_INPUT_SYSTEM");
            
            //update changed defines
            if (AnyMatch(symbols, "Storm_HDRP_Installed"))
            {
                //Debug.Log("Replacing Define Storm_HDRP_Installed with PW_STORM_HDRP");
                symbols.Remove("Storm_HDRP_Installed");
                symbols.Add("PW_STORM_HDRP");
                updateScripting = true;
            }
            if (AnyMatch(symbols, "Storm_URP_Installed"))
            {
                //Debug.Log("Replacing Define Storm_URP_Installed with PW_STORM_URP");
                symbols.Remove("Storm_URP_Installed");
                symbols.Add("PW_STORM_URP");
                updateScripting = true;
            }

            //Input defines
            Type inputDataType = Type.GetType("UnityEngine.InputSystem.InputSystem, Unity.InputSystem");
            if (inputDataType != null)
                AddDefine(symbols, ref updateScripting, "PW_STORM_INPUT_SYSTEM");
            else
                RemoveDefine(symbols, ref updateScripting, "PW_STORM_INPUT_SYSTEM");


            //pipeline defines
            Type hdrpCamDataType = Type.GetType("UnityEngine.Rendering.HighDefinition.HDAdditionalCameraData, Unity.RenderPipelines.HighDefinition.Runtime");
            if (hdrpCamDataType != null)
                AddDefine(symbols, ref updateScripting, "PW_STORM_HDRP");
            else
                RemoveDefine(symbols, ref updateScripting, "PW_STORM_HDRP");

            Type urpCamDataType = Type.GetType("UnityEngine.Rendering.Universal.UniversalAdditionalCameraData, Unity.RenderPipelines.Universal.Runtime");
            if (urpCamDataType != null)
                AddDefine(symbols, ref updateScripting, "PW_STORM_URP");
            else
                RemoveDefine(symbols, ref updateScripting, "PW_STORM_URP");

            Type entityWorldType = Type.GetType("Unity.Entities.World, Unity.Entities");
            if (entityWorldType != null)
                AddDefine(symbols, ref updateScripting, "PW_STORM_ECS");
            else
                RemoveDefine(symbols, ref updateScripting, "PW_STORM_ECS");

            if (updateScripting)
            {
                //Debug.Log("Updating Defines:\nOld=" + Collapse(symbolsArray) + "\nNew=" + Collapse(symbols));
                PlayerSettings.SetScriptingDefineSymbols(NamedBuildTarget.FromBuildTargetGroup(EditorUserBuildSettings.selectedBuildTargetGroup), symbols.ToArray());
            }
#endif
        }

        private static void AddDefine(List<string> symbols, ref bool updateScripting, string define)
        {
            if (!symbols.Contains(define))
            {
                //Debug.Log("Adding Define: " + define);
                symbols.Add(define);
                updateScripting = true;
            }
        } 
        private static void RemoveDefine(List<string> symbols, ref bool updateScripting, string define)
        {
            if (symbols.Remove(define))
            {
                //Debug.Log("Removing Define: " + define);
                updateScripting = true;
            }
        }

        public static T FindObjectByType<T>(bool includeHidden = true) where T : Object
        {
#if UNITY_6000_0_OR_NEWER
            return Object.FindFirstObjectByType<T>(includeHidden ? FindObjectsInactive.Include : FindObjectsInactive.Exclude);
#else
            return Object.FindObjectOfType<T>(includeHidden);
#endif
        }

        public static void SetMaxFramerate()
        {
            Resolution[] allResolutions = Screen.resolutions;
            int maxRefresh = 60;
            for (int i = 0; i < allResolutions.Length; i++)
                maxRefresh = Mathf.Max(maxRefresh, (int)allResolutions[i].refreshRateRatio.value);
            // Clamp just in case bogus values are returned.
            maxRefresh = Mathf.Clamp(maxRefresh, 60, 360);
            Application.targetFrameRate = maxRefresh;
        }

        public static void ChangeBGColor(Camera cam, Color c)
        {
#if PW_STORM_HDRP
            HDAdditionalCameraData camData = cam.GetComponent<HDAdditionalCameraData>();
            camData.backgroundColorHDR = c;
#endif
        }
        public static List<ScriptableObject> DisablePostProcessing(Camera cam)
        {
            List<ScriptableObject> ret = new List<ScriptableObject>();
#if PW_STORM_HDRP
            Volume vol = StormUtils.FindObjectOfType<Volume>();
            if (vol != null)
            {
                for (int c = 0; c < vol.profile.components.Count; ++c)
                {
                    if (vol.profile.components[c].active)
                    {
                        vol.profile.components[c].active = false;
                        ret.Add(vol.profile.components[c]);
                    }
                }
            }
            cam.allowHDR = false;
            HDAdditionalCameraData camData = cam.GetComponent<HDAdditionalCameraData>();
            if (camData == null)
                camData = cam.gameObject.AddComponent<HDAdditionalCameraData>();

            camData.clearColorMode = HDAdditionalCameraData.ClearColorMode.Color;
            camData.backgroundColorHDR = new Color(0, 0, 0, 0);
            camData.defaultFrameSettings = FrameSettingsRenderType.Camera;
            camData.enabled = false;
            camData.SMAAQuality = HDAdditionalCameraData.SMAAQualityLevel.Low;

            camData.customRenderingSettings = true;
            FrameSettingsOverrideMask frameSettingsOverride = camData.renderingPathCustomFrameSettingsOverrideMask;
            frameSettingsOverride.mask[(uint)FrameSettingsField.Postprocess] = true;
            frameSettingsOverride.mask[(uint)FrameSettingsField.CustomPostProcess] = true;
            frameSettingsOverride.mask[(uint)FrameSettingsField.StopNaN] = true;
            frameSettingsOverride.mask[(uint)FrameSettingsField.DepthOfField] = true;
            frameSettingsOverride.mask[(uint)FrameSettingsField.MotionBlur] = true;
            frameSettingsOverride.mask[(uint)FrameSettingsField.PaniniProjection] = true;
            frameSettingsOverride.mask[(uint)FrameSettingsField.Bloom] = true;
            frameSettingsOverride.mask[(uint)FrameSettingsField.LensDistortion] = true;
            frameSettingsOverride.mask[(uint)FrameSettingsField.ChromaticAberration] = true;
            frameSettingsOverride.mask[(uint)FrameSettingsField.Vignette] = true;
            frameSettingsOverride.mask[(uint)FrameSettingsField.ColorGrading] = true;
            frameSettingsOverride.mask[(uint)FrameSettingsField.FilmGrain] = true;
            frameSettingsOverride.mask[(uint)FrameSettingsField.Dithering] = true;
            frameSettingsOverride.mask[(uint)FrameSettingsField.Antialiasing] = true;
            frameSettingsOverride.mask[(uint)FrameSettingsField.Tonemapping] = true;
            frameSettingsOverride.mask[(uint)FrameSettingsField.LensFlareDataDriven] = true;
            frameSettingsOverride.mask[(uint)FrameSettingsField.AfterPostprocess] = true;

            //these will additively increase impostor generation time in Unity 6
            frameSettingsOverride.mask[(uint)FrameSettingsField.SSAO] = true;
            frameSettingsOverride.mask[(uint)FrameSettingsField.SSR] = true;
            frameSettingsOverride.mask[(uint)FrameSettingsField.AsyncCompute] = true;

            camData.renderingPathCustomFrameSettingsOverrideMask = frameSettingsOverride;


            FrameSettings frameSettings = camData.renderingPathCustomFrameSettings;
            frameSettings.SetEnabled(FrameSettingsField.Postprocess, true);
            frameSettings.SetEnabled(FrameSettingsField.CustomPostProcess, false);
            frameSettings.SetEnabled(FrameSettingsField.StopNaN, false);
            frameSettings.SetEnabled(FrameSettingsField.DepthOfField, false);
            frameSettings.SetEnabled(FrameSettingsField.MotionBlur, false);
            frameSettings.SetEnabled(FrameSettingsField.PaniniProjection, false);
            frameSettings.SetEnabled(FrameSettingsField.Bloom, false);
            frameSettings.SetEnabled(FrameSettingsField.LensDistortion, false);
            frameSettings.SetEnabled(FrameSettingsField.ChromaticAberration, false);
            frameSettings.SetEnabled(FrameSettingsField.Vignette, false);
            frameSettings.SetEnabled(FrameSettingsField.ColorGrading, false);
            frameSettings.SetEnabled(FrameSettingsField.FilmGrain, false);
            frameSettings.SetEnabled(FrameSettingsField.Dithering, false);
            frameSettings.SetEnabled(FrameSettingsField.Antialiasing, false);
            frameSettings.SetEnabled(FrameSettingsField.Tonemapping, false);
            frameSettings.SetEnabled(FrameSettingsField.LensFlareDataDriven, false);
            frameSettings.SetEnabled(FrameSettingsField.AfterPostprocess, false);

            //these will additively increase impostor generation time in Unity 6
            frameSettings.SetEnabled(FrameSettingsField.SSAO, false);
            frameSettings.SetEnabled(FrameSettingsField.SSR, false);
            frameSettings.SetEnabled(FrameSettingsField.AsyncCompute, false);

            camData.renderingPathCustomFrameSettings = frameSettings;
#endif
            return ret;
        }
        public static void ReEnablePostProcessing(List<ScriptableObject> list)
        {
            if (list != null)
            {
                for (int l = 0; l < list.Count; ++l)
                {
#if PW_STORM_HDRP
                    if (list[l] is VolumeComponent comp)
                    {
                        comp.active = true;
                    }
#endif
                }
            }
        }
#if UNITY_EDITOR
        private static void AddIfMissing(UnityEditor.SerializedProperty prop, Object item)
        {
            bool found = false;
            for (int s = 0; s < prop.arraySize; ++s)
            {
                UnityEditor.SerializedProperty shader = prop.GetArrayElementAtIndex(s);
                if (shader.objectReferenceValue == item)
                {
                    found = true;
                    break;
                }
            }
            if (!found)
            {
                prop.InsertArrayElementAtIndex(prop.arraySize);
                UnityEditor.SerializedProperty shader = prop.GetArrayElementAtIndex(prop.arraySize - 1);
                shader.objectReferenceValue = item;
            }
        }
        private static void RemoveIfFound(UnityEditor.SerializedProperty prop, Object item)
        {
            for (int s = 0; s < prop.arraySize; ++s)
            {
                UnityEditor.SerializedProperty shader = prop.GetArrayElementAtIndex(s);
                if (shader.objectReferenceValue == item)
                {
                    prop.DeleteArrayElementAtIndex(s--);
                }
            }
        }
#endif
        public static void SetCrossSceneRefsAllowed(bool allow)
        {
#if UNITY_EDITOR
            UnityEditor.SceneManagement.EditorSceneManager.preventCrossSceneReferences = !allow;
#endif
        }
        public static bool IsEditor()
        {
#if UNITY_EDITOR
            return true;
#else
            return false;
#endif
        }
#if UNITY_EDITOR
        static Action s_editorUpdates;
        static UnityEditor.SceneView s_lastSceneView;
        static Vector3 s_lastCamPos;
        static Quaternion s_lastCamRot;
        static void OnEditorUpdate()
        {
            UnityEditor.SceneView sceneView = UnityEditor.EditorWindow.focusedWindow as UnityEditor.SceneView;
            if (sceneView == null || sceneView.camera == null)
                return;
            Vector3 camPos = sceneView.camera.transform.position;
            Quaternion camRot = sceneView.camera.transform.rotation;

            if (s_lastSceneView == sceneView && s_lastCamPos == camPos && s_lastCamRot == camRot)
                return;
            s_lastSceneView = sceneView;
            s_lastCamPos = camPos;
            s_lastCamRot = camRot;

            if (s_editorUpdates == null)
                UnityEditor.EditorApplication.update -= OnEditorUpdate;
            else
                s_editorUpdates.Invoke();
        }
        public static Action<SceneView> EditorGUICallback;
        static void OnEditorGUI(UnityEditor.SceneView view)
        {
            EditorGUICallback?.Invoke(view);
        }
#endif
        public static void AddEditorUpdate(Action callback)
        {
#if UNITY_EDITOR
            s_editorUpdates += callback;
            UnityEditor.EditorApplication.update -= OnEditorUpdate;
            UnityEditor.EditorApplication.update += OnEditorUpdate;
            UnityEditor.SceneView.duringSceneGui -= OnEditorGUI;
            UnityEditor.SceneView.duringSceneGui += OnEditorGUI;
#endif
        }
        public static void RemoveEditorUpdate(Action callback)
        {
#if UNITY_EDITOR
            s_editorUpdates -= callback;
            if (s_editorUpdates == null)
            {
                UnityEditor.EditorApplication.update -= OnEditorUpdate;
                UnityEditor.SceneView.duringSceneGui -= OnEditorGUI;
            }
#endif
        }
        public static void AddEditorOnRecompile(Action callback)
        {
#if UNITY_EDITOR
            OnCompiledAction -= callback;
            OnCompiledAction += callback;
            EditorApplication.playModeStateChanged -= EditorApplication_playModeStateChanged;
            EditorApplication.playModeStateChanged += EditorApplication_playModeStateChanged;
            UnityEditor.Compilation.CompilationPipeline.assemblyCompilationFinished -= CompilationPipeline_assemblyCompilationFinished;
            UnityEditor.Compilation.CompilationPipeline.assemblyCompilationFinished += CompilationPipeline_assemblyCompilationFinished;
            UnityEditor.Compilation.CompilationPipeline.compilationFinished -= CompilationPipeline_compilationFinished;
            UnityEditor.Compilation.CompilationPipeline.compilationFinished += CompilationPipeline_compilationFinished;
            UnityEditor.AssetDatabase.importPackageCompleted -= AssetDatabase_importPackageCompleted;
            UnityEditor.AssetDatabase.importPackageCompleted += AssetDatabase_importPackageCompleted;
#endif
        }
        public static void RemoveEditorOnRecompile(Action callback)
        {
#if UNITY_EDITOR
            OnCompiledAction -= callback;
#endif
        }
        public static T LoadResource<T>(string path) where T : Object
        {
            string[] pathParts = path.Split(resourceSplit, StringSplitOptions.None);
            T[] items = Resources.LoadAll<T>(pathParts[0]);
            if (items.Length > 0)
            {
                if (pathParts.Length > 1)
                {
                    for (int i = 0; i < items.Length; ++i)
                    {
                        if (Regex.IsMatch(items[i].name, pathParts[1]))
                            return items[i];
                    }
                }
                return items[0];
            }
            return null;
        }
        public static bool EnsureNonPreset<T>(ref T obj, string saveDir = "") where T : ScriptableObject
        {
            string path = GetAssetPath(obj).Replace('\\', '/');
            if (path.ToLowerInvariant().Contains("/wizard settings/storm render settings/"))
            {
                if (string.IsNullOrWhiteSpace(saveDir))
                    saveDir = DefaultSavePath + "Settings/";
                if (!saveDir.EndsWith('/') && !saveDir.EndsWith('\\'))
                    saveDir += "/";
                string filename = obj.name.Replace("StormPreset", "Custom");
                saveDir += filename + Path.GetExtension(path);
                T ret = LoadAsset<T>(saveDir);
                if (ret != null)
                    obj = ret;
                else
                {
                    obj = ScriptableObject.Instantiate<T>(obj);
                    obj.name = filename;
                    obj = SaveAsset<T>(obj, saveDir);
                }
                return true;
            }
            return false;
        }
        public static bool EnsureNonPreset<T>(ref T[] objs, string saveDir = "") where T : ScriptableObject
        {
            bool ret = false;
            for (int t = 0; t < objs.Length; ++t)
            {
                T obj = objs[t];
                string path = GetAssetPath(obj).Replace('\\', '/');
                if (path.ToLowerInvariant().Contains("/wizard settings/storm render settings/"))
                {
                    if (string.IsNullOrWhiteSpace(saveDir))
                        saveDir = DefaultSavePath + "Settings/";
                    if (!saveDir.EndsWith('/') && !saveDir.EndsWith('\\'))
                        saveDir += "/";
                    string filename = obj.name.Replace("StormPreset", "Custom");
                    saveDir += filename + Path.GetExtension(path);
                    T tval = LoadAsset<T>(saveDir);
                    if (tval != null)
                        obj = tval;
                    else
                    {
                        obj = ScriptableObject.Instantiate<T>(obj);
                        obj.name = filename;
                        obj = SaveAsset<T>(obj, saveDir);
                    }
                    objs[t] = obj;
                    ret = true;
                }
            }
            return ret;
        }
        public static bool EnsureNonPreset<T>(ref List<T> objs, string saveDir = "") where T : ScriptableObject
        {
            bool ret = false;
            for (int t = 0; t < objs.Count; ++t)
            {
                T obj = objs[t];
                string path = GetAssetPath(obj).Replace('\\', '/');
                if (path.ToLowerInvariant().Contains("/wizard settings/storm render settings/"))
                {
                    if (string.IsNullOrWhiteSpace(saveDir))
                        saveDir = DefaultSavePath + "Settings/";
                    if (!saveDir.EndsWith('/') && !saveDir.EndsWith('\\'))
                        saveDir += "/";
                    string filename = obj.name.Replace("StormPreset", "Custom");
                    saveDir += filename + Path.GetExtension(path);
                    T tval = LoadAsset<T>(saveDir);
                    if (tval != null)
                        obj = tval;
                    else
                    {
                        obj = ScriptableObject.Instantiate<T>(obj);
                        obj.name = filename;
                        obj = SaveAsset<T>(obj, saveDir);
                    }
                    objs[t] = obj;
                    ret = true;
                }
            }
            return ret;
        }

        public static GameObject GetEditorSelection()
        {
#if UNITY_EDITOR
            return UnityEditor.Selection.activeGameObject;
#else
            return null;
#endif
        }
        public static void SetEditorSelection(GameObject go)
        {
#if UNITY_EDITOR
            UnityEditor.Selection.activeGameObject = go;
#endif
        }

        public static void DrawSceneViewText(string str, Vector3 pos)
        {
#if UNITY_EDITOR
            GUIStyle labelStyle = new GUIStyle(EditorStyles.label);
            labelStyle.alignment = TextAnchor.MiddleCenter;
            Handles.Label(pos, str, labelStyle);
#endif
        }

        public static void ShowDialog(string title, string prompt)
        {
#if UNITY_EDITOR
            EditorUtility.DisplayDialog(title, prompt, "Ok");
#endif
        }
        public static bool GetDialogOptOut(string OptOutName)
        {
#if UNITY_EDITOR
            string unityVersion = Application.unityVersion;
            return EditorUtility.GetDialogOptOutDecision(DialogOptOutDecisionType.ForThisMachine, OptOutName + "-" + unityVersion);
#else
            return false;
#endif
        }
        public static bool ShowDialogOption(string title, string prompt, string OptOutName = "")
        {
#if UNITY_EDITOR
            if (!string.IsNullOrEmpty(OptOutName))
            {
                string unityVersion = Application.unityVersion;
                if (EditorUtility.GetDialogOptOutDecision(DialogOptOutDecisionType.ForThisMachine, OptOutName + "-" + unityVersion))
                    return false;
                int result = EditorUtility.DisplayDialogComplex(title, prompt, "Yes", "No", "No, Do Not Ask Again");
                if (result == 2)
                    EditorUtility.SetDialogOptOutDecision(DialogOptOutDecisionType.ForThisMachine, OptOutName + "-" + unityVersion, true);
                return result == 0;
            }
            else
            {
                return EditorUtility.DisplayDialog(title, prompt, "Yes", "No");
            }
#else
            return true;
#endif
        }

        public static string SaveFilePanelInProject(string title, string defaultName, string extension, string message)
        {
#if UNITY_EDITOR
            return UnityEditor.EditorUtility.SaveFilePanelInProject(title, defaultName, extension, message);
#else
            return "";
#endif
        }
        public static string SaveFilePanelInProject(string title, string defaultName, string extension, string message, string path)
        {
#if UNITY_EDITOR
            return UnityEditor.EditorUtility.SaveFilePanelInProject(title, defaultName, extension, message, path);
#else
            return "";
#endif
        }
        public static string SaveFolderPanel(string title, string folder, string defaultName)
        {
#if UNITY_EDITOR
            return UnityEditor.EditorUtility.SaveFolderPanel(title, folder, defaultName);
#else
            return "";
#endif
        }
        /// <summary>
        /// Returns the first asset that matches the file path and name passed. Will try
        /// full path first, then will try just the file name.
        /// </summary>
        /// <param name="fileNameOrPath">File name as standalone or fully pathed</param>
        /// <returns>Object or null if it was not found</returns>
        public static UnityEngine.Object GetAsset(string fileNameOrPath, Type assetType)
        {
#if UNITY_EDITOR
            if (!string.IsNullOrEmpty(fileNameOrPath))
            {
                UnityEngine.Object obj = AssetDatabase.LoadAssetAtPath(fileNameOrPath, assetType);
                if (obj != null)
                {
                    return obj;
                }
                else
                {
                    string path = GetAssetPath(Path.GetFileName(fileNameOrPath));
                    if (!string.IsNullOrEmpty(path))
                    {
                        return AssetDatabase.LoadAssetAtPath(path, assetType);
                    }
                }
            }
#endif
            return null;
        }

        /// <summary>
        /// Get the asset path of the first thing that matches the name
        /// </summary>
        /// <param name="fileName">File name to search for</param>
        /// <returns></returns>
        public static string GetAssetPath(string fileName)
        {
#if UNITY_EDITOR
            string fName = Path.GetFileNameWithoutExtension(fileName);
            string[] assets = AssetDatabase.FindAssets(fName, null);
            for (int idx = 0; idx < assets.Length; idx++)
            {
                string path = AssetDatabase.GUIDToAssetPath(assets[idx]);
                if (Path.GetFileName(path) == fileName)
                {
                    return path;
                }
            }
#endif
            return "";
        }

        /// <summary>
        /// Adds the given prefab with the matching file name as a tool under "Storm Tools" in the scene
        /// </summary>
        /// <param name="prefabFileName"></param>
        /// <param name="ping"></param>
        public static void AddPrefabAsTool(string prefabFileName, bool ping = true)
        {
            GameObject toolsGO = GetToolsGameObject();
            GameObject prefab = (GameObject)StormUtils.GetAsset(prefabFileName, (typeof(GameObject)));
            GameObject newTool = GameObject.Instantiate(prefab);
            newTool.name = newTool.name.Replace("(Clone)", "");
            newTool.transform.parent = toolsGO.transform;
#if UNITY_EDITOR
            if (ping)
            {
                EditorGUIUtility.PingObject(newTool);
                Selection.activeObject = newTool;
            }
#endif
        }

        /// <summary>
        /// Will open a file in the browser via "Application.OpenURL". This is a good workaround to open things like PDFs
        /// or images in a separate application, as e.g. opening the PDF reader application under different operating systems
        /// can be interesting - we can bypass all the complexity with this. Currently only works in the editor
        /// </summary>
        /// <param name="filnameAndExtension"></param>
        public static void OpenFileInBrowser(string filnameAndExtension)
        {
#if UNITY_EDITOR
            UnityEngine.Object guide = (UnityEngine.Object)StormUtils.GetAsset(filnameAndExtension, typeof(UnityEngine.Object));
            Application.OpenURL(GetFullFileSystemPath(AssetDatabase.GetAssetPath(guide)));
#endif
        }

        /// <summary>
        /// Expects a path starting at the asset folder, and will return a full file system path starting at the Drive Letter
        /// </summary>
        /// <param name="inputPath">Unity path starting at the asset folder</param>
        /// <returns></returns>
        public static string GetFullFileSystemPath(string inputPath)
        {
            return Application.dataPath.Substring(0, Application.dataPath.Length - "Assets".Length) + inputPath;
        }

        /// <summary>
        /// Checks if a directory exists, creating it if not. Returns full path minus filename
        /// </summary>
        /// <param name="path">Path to check for</param>
        /// <returns>Full path minus ending filename (if ends with a filename)</returns>
        public static string EnsureDirExists(string path)
        {
            string[] parts = path.Split(new char[] { '/', '\\' }, StringSplitOptions.RemoveEmptyEntries);
            string curPath = "";
            for (int p = 0; p < parts.Length; ++p)
            {
                if (p == parts.Length - 1)
                {
                    if (parts[p].Contains("."))
                        break;
                }
                curPath = curPath + (p == 0 ? "" : "/") + parts[p];
                if (!Directory.Exists(curPath))
                    Directory.CreateDirectory(curPath);
            }
            return curPath;
        }
        /// <summary>
        /// Checks if a directory is empty and deletes the folder if it is
        /// </summary>
        /// <param name="path">Path to check for</param>
        /// <returns>Full path minus ending filename (if ends with a filename)</returns>
        public static void CheckDirEmpty(string path)
        {
            string[] parts = path.Split(new char[] { '/', '\\' }, StringSplitOptions.RemoveEmptyEntries);
            for (int p = parts.Length - 1; p >= 0; ++p)
            {
                string curPath = "";
                for (int i = 0; i < p; ++i)
                    curPath += parts[i] + "/";

                DirectoryInfo dir = new DirectoryInfo(curPath);
                if (dir.Exists && dir.GetFiles().Length == 0 && dir.GetDirectories().Length == 0)
                {
                    dir.Delete();
                    FileInfo metaFile = new FileInfo(curPath.Substring(0, curPath.Length - 1) + ".meta");
                    if (metaFile.Exists)
                        metaFile.Delete();
                }
                else
                    return;
            }
        }
        public static string EnsureUniqueName(string path, string name, string extension)
        {
            if (!path.EndsWith("/") && !path.EndsWith("\\"))
                path += "/";
            string newName = name + "." + extension;
            ulong count = 0;
            while (File.Exists(path + newName))
                newName = name + "(" + (++count) + ")" + "." + extension;
            return newName;
        }
        public static string EnsureUniqueFolder(string path, string name)
        {
            if (!path.EndsWith("/") && !path.EndsWith("\\"))
                path += "/";
            string newName = name;
            ulong count = 0;
            while (Directory.Exists(path + newName))
                newName = name + "(" + (++count) + ")";
            return newName;
        }
        public static void ForceEditorSceneviewRedraw(GameObject worldObject = null)
        {
#if UNITY_EDITOR
            EditorApplication.QueuePlayerLoopUpdate();
            UnityEditor.SceneView.RepaintAll();
#endif
        }
        public static string GetAssetGUID(Object asset)
        {
#if UNITY_EDITOR
            if (UnityEditor.AssetDatabase.TryGetGUIDAndLocalFileIdentifier(asset, out string guid, out long _))
                return guid;
            return "";
#else
            return asset.GetInstanceID().ToString();
#endif
        }
        public static T FindAsset<T>(string typeString, string path, bool forcePathOnly = false) where T : Object
        {
#if UNITY_EDITOR
            string[] assetIDs;
            if (string.IsNullOrEmpty(path))
                assetIDs = UnityEditor.AssetDatabase.FindAssets(typeString);
            else
                assetIDs = UnityEditor.AssetDatabase.FindAssets(typeString, new string[] { path });
            if (assetIDs.Length > 0)
            {
                T ret = UnityEditor.AssetDatabase.LoadAssetAtPath<T>(UnityEditor.AssetDatabase.GUIDToAssetPath(assetIDs[0]));
                if (ret != null)
                    return ret;
            }
            if (!forcePathOnly)
            {
                assetIDs = UnityEditor.AssetDatabase.FindAssets(typeString);
                if (assetIDs.Length > 0)
                {
                    T ret = UnityEditor.AssetDatabase.LoadAssetAtPath<T>(UnityEditor.AssetDatabase.GUIDToAssetPath(assetIDs[0]));
                    return ret;
                }
            }
#else
            //fallback for runtime searching for assets
            T[] allAssets = Resources.FindObjectsOfTypeAll<T>();
            if (allAssets.Length > 0)
            {
                for (int a = 0; a < allAssets.Length; ++a)
                {
                    string assetPath = GetAssetPath(allAssets[a]);
                    if (assetPath.EndsWith(path))
                        return allAssets[a];
                }
                if (!forcePathOnly)
                    return allAssets[0];
            }
#endif
            return null;
        }
        public static T[] FindAssets<T>(string typeString, string path, bool forcePathOnly = false) where T : Object
        {
#if UNITY_EDITOR
            string[] assetIDs;
            if (string.IsNullOrEmpty(path))
                assetIDs = UnityEditor.AssetDatabase.FindAssets(typeString);
            else
                assetIDs = UnityEditor.AssetDatabase.FindAssets(typeString, new string[] { path });
            if (assetIDs.Length > 0)
            {
                T[] ret = new T[assetIDs.Length];
                for (int i = 0; i < assetIDs.Length; ++i)
                {
                    T item = UnityEditor.AssetDatabase.LoadAssetAtPath<T>(UnityEditor.AssetDatabase.GUIDToAssetPath(assetIDs[i]));
                    if (item != null)
                        ret[i] = item;
                }
                return ret;
            }
            if (!forcePathOnly)
            {
                assetIDs = UnityEditor.AssetDatabase.FindAssets(typeString);
                if (assetIDs.Length > 0)
                {
                    T[] ret = new T[assetIDs.Length];
                    for (int i = 0; i < assetIDs.Length; ++i)
                    {
                        T item = UnityEditor.AssetDatabase.LoadAssetAtPath<T>(UnityEditor.AssetDatabase.GUIDToAssetPath(assetIDs[i]));
                        if (item != null)
                            ret[i] = item;
                    }
                    return ret;
                }
            }
//#else
            //fallback for runtime searching for assets
            T[] allAssets = Resources.FindObjectsOfTypeAll<T>();
            if (allAssets.Length > 0)
            {
                List<T> retList = new List<T>(allAssets.Length);
                for (int a = 0; a < allAssets.Length; ++a)
                {
                    string assetPath = GetAssetPath(allAssets[a]);
                    if (assetPath.EndsWith(path))
                        retList.Add(allAssets[a]);
                }
                if (!forcePathOnly && retList.Count == 0)
                    return allAssets;
                return retList.ToArray();
            }
#endif
            return new T[0];
        }

        public static T LoadResourceOrPreset<T>(string name) where T : ScriptableObject
        {
            //first just try to load from Resources folders with the expected name
            T ret = Resources.Load<T>(name);
            if (ret == null)
            {
                //nothing so search for asset in Storm User Data folder
                ret = FindAsset<T>("t:" + typeof(T).Name, DefaultSavePath, true);
                if (ret == null)
                {
                    //still nothing so search for a preset
                    ret = FindAsset<T>("t:" + typeof(T).Name + " StormPreset", "");
                    if (ret == null)
                    {
                        //no asset of this type exist ... just make a new one I guess
                        ret = ScriptableObject.CreateInstance<T>();
                        ret.name = name;
                    }
                }
                //save the found asset in our Resources folder
                ret = SaveResource(ret, ret.name + ".asset") as T;
            }
            //make sure that what we send out isn't in the wizard preset folder
            EnsureNonPreset(ref ret);
            return ret;
        }
        public static Texture2D MakeReadable(Texture2D source, TextureFormat format = TextureFormat.RGBA32, bool mipmaps = false)
        {
            if (source == null || source.isReadable)
                return source;
#if UNITY_EDITOR
            string assetPath = UnityEditor.AssetDatabase.GetAssetPath(source);
            var tImporter = UnityEditor.AssetImporter.GetAtPath(assetPath) as UnityEditor.TextureImporter;
            if (tImporter != null)
            {
                tImporter.isReadable = true;

                UnityEditor.AssetDatabase.ImportAsset(assetPath);
                UnityEditor.AssetDatabase.Refresh();
            }
            return source;
#else
            int sourceWidth = source.width;
            int sourceHeight = source.height;
            RenderTexture originalRT = RenderTexture.active;
            RenderTexture renderTex = RenderTexture.GetTemporary(
                        sourceWidth,
                        sourceHeight,
                        0,
                        RenderTextureFormat.DefaultHDR,
                        RenderTextureReadWrite.Default);

            Graphics.Blit(source, renderTex);
            RenderTexture.active = renderTex;
            Texture2D readableTex = new Texture2D(sourceWidth, sourceHeight, format, mipmaps);
            readableTex.ReadPixels(new Rect(0, 0, sourceWidth, sourceHeight), 0, 0, mipmaps);
            readableTex.Apply();
            RenderTexture.active = originalRT;
            RenderTexture.ReleaseTemporary(renderTex);
            return readableTex;
#endif

        }
        public static Mesh MakeReadable(Mesh source)
        {
            if (source == null || source.isReadable)
                return source;
#if UNITY_EDITOR
            string assetPath = UnityEditor.AssetDatabase.GetAssetPath(source);
            var tImporter = UnityEditor.AssetImporter.GetAtPath(assetPath) as UnityEditor.ModelImporter;
            if (tImporter != null)
            {
                tImporter.isReadable = true;

                UnityEditor.AssetDatabase.ImportAsset(assetPath);
                UnityEditor.AssetDatabase.Refresh();
            }
#endif
            return source;
        }
        public static Texture2D SaveRenderTexture(RenderTexture rt, string filename, StormTextureExportFormat format, TextureFormat fileFormat = TextureFormat.RGBAFloat, bool reload = false)
        {
            Texture2D readableTex = new Texture2D(rt.width, rt.height, fileFormat, false);
            RenderTexture oldRT = RenderTexture.active;
            RenderTexture.active = rt;
            readableTex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
            RenderTexture.active = oldRT;
            readableTex.Apply(true, false);
            return SaveTexture(readableTex, filename, format, reload);
        }
        public static Texture2D SaveTexture(Texture2D t, string filename, StormTextureExportFormat format, bool reload = true, bool SetImportSettings = false)
        {
            if (!t.isReadable)
            {
                t = MakeReadable(t);
            }
            byte[] buf = format switch {
                StormTextureExportFormat.EXR => ImageConversion.EncodeToEXR(t),
                StormTextureExportFormat.PNG => ImageConversion.EncodeToPNG(t),
                StormTextureExportFormat.TGA => ImageConversion.EncodeToTGA(t),
                _ => ImageConversion.EncodeToPNG(t),
            };
            if (buf == null || buf.Length == 0)
            {
                Debug.Log("Unable to get Texture's " + format + " encoded data");
                return null;
            }
            if (filename.StartsWith(Application.dataPath))
                filename = filename.Substring(Application.dataPath.Length - 6);
            string fileExtension = Path.GetExtension(filename).ToLower();
            switch (format)
            {
                case StormTextureExportFormat.EXR:
                    if (fileExtension != ".exr")
                        filename = filename.Substring(0, filename.Length - fileExtension.Length) + ".exr";
                    break;
                case StormTextureExportFormat.PNG:
                default:
                    if (fileExtension != ".png")
                        filename = filename.Substring(0, filename.Length - fileExtension.Length) + ".png";
                    break;
                case StormTextureExportFormat.TGA:
                    if (fileExtension != ".tga")
                        filename = filename.Substring(0, filename.Length - fileExtension.Length) + ".tga";
                    break;
            }
            EnsureDirExists(filename);
            if (File.Exists(filename))
                File.Delete(filename);
            var img = File.Create(filename);
            if (img == null || !img.CanWrite)
            {
                Debug.Log("Unable to create image file: " + filename);
                return null;
            }
            img.Write(buf, 0, buf.Length);
            img.Close();
#if UNITY_EDITOR
            if (reload)
                UnityEditor.AssetDatabase.Refresh();

            if (SetImportSettings)
            {
                var importer = AssetImporter.GetAtPath(filename) as TextureImporter;
                if (importer != null)
                {
                    importer.isReadable = true;
                    importer.alphaSource = TextureImporterAlphaSource.None;
                    importer.alphaIsTransparency = false;
                    importer.sRGBTexture = false;
                    importer.maxTextureSize = math.max(t.width, t.height);
                    importer.wrapMode = TextureWrapMode.Clamp;
                    importer.mipmapEnabled = false;
                    importer.npotScale = TextureImporterNPOTScale.None;
                    TextureImporterPlatformSettings texImpPlatSet = new TextureImporterPlatformSettings();
                    texImpPlatSet.maxTextureSize = math.max(t.width, t.height);
                    texImpPlatSet.format = TextureImporterFormat.RFloat;
                    texImpPlatSet.textureCompression = TextureImporterCompression.Uncompressed;
                    importer.SetPlatformTextureSettings(texImpPlatSet);
                    importer.SaveAndReimport();
                }
                else
                    Debug.Log("No Importer found for " + filename);
            }

            return UnityEditor.AssetDatabase.LoadAssetAtPath<Texture2D>(filename);
#else
            return t;
#endif
        }
        public static void SaveTexture(Texture2D t, Stream stream, StormTextureExportFormat format)
        {
            if (!t.isReadable)
                t = MakeReadable(t);
            byte[] buf = format switch {
                StormTextureExportFormat.EXR => t.EncodeToEXR(),
                StormTextureExportFormat.PNG => t.EncodeToPNG(),
                StormTextureExportFormat.TGA => t.EncodeToTGA(),
                _ => t.EncodeToPNG(),
            };
            byte[] data = BitConverter.GetBytes(buf.Length);
            stream.Write(data, 0, data.Length);
            stream.Write(buf, 0, buf.Length);
        }
        public static Texture2D SaveTexture(Texture2D texture, StormTextureExportFormat format, string saveDir, BakeShaderOutputType outType)
        {
#if UNITY_EDITOR
            if (!texture.isReadable)
            {
                Debug.LogWarning("Unable to save texture '" + texture.name + "'.\nPassed Texture Is Not Readable!", texture);
                return texture;
            }
            string SaveName;
            SaveName = UnityEditor.AssetDatabase.GetAssetPath(texture);
            if (!string.IsNullOrEmpty(SaveName))
            {
                //already saved. just update if we can.
                if ((format == StormTextureExportFormat.PNG && !SaveName.EndsWith(".png", StringComparison.OrdinalIgnoreCase)) ||
                    (format == StormTextureExportFormat.TGA && !SaveName.EndsWith(".tga", StringComparison.OrdinalIgnoreCase)) ||
                    (format == StormTextureExportFormat.EXR && !SaveName.EndsWith(".exr", StringComparison.OrdinalIgnoreCase)))
                {
                    UnityEditor.AssetDatabase.DeleteAsset(SaveName);
                    SaveName = null;
                }
            }
            if (string.IsNullOrEmpty(SaveName))
            {
                saveDir = saveDir.Replace('\\', '/');
                if (!saveDir.ToLower().StartsWith("assets"))
                    saveDir = "Assets/" + saveDir;
                if (!saveDir.EndsWith("/"))
                    saveDir += "/";
                SaveName = saveDir + EnsureUniqueName(saveDir, texture.name, format switch {
                    StormTextureExportFormat.PNG => "png",
                    StormTextureExportFormat.TGA => "tga",
                    StormTextureExportFormat.EXR => "exr",
                    _ => "png"
                });
            }
            EnsureDirExists(SaveName);

            var data = format switch {
                StormTextureExportFormat.EXR => texture.EncodeToEXR(),
                StormTextureExportFormat.PNG => texture.EncodeToPNG(),
                StormTextureExportFormat.TGA => texture.EncodeToTGA(),
                _ => texture.EncodeToPNG(),
            };
            File.WriteAllBytes(SaveName, data);
            int textureSize = Mathf.Max(texture.height, texture.width);
            Object.DestroyImmediate(texture);
            UnityEditor.AssetDatabase.Refresh();
            UnityEditor.TextureImporter texImporter = UnityEditor.AssetImporter.GetAtPath(SaveName) as UnityEditor.TextureImporter;
            if (texImporter != null)
            {
                texImporter.maxTextureSize = textureSize;
                texImporter.alphaSource = UnityEditor.TextureImporterAlphaSource.FromInput;
                texImporter.mipmapEnabled = false;
                if (outType == BakeShaderOutputType.NORMAL)
                    texImporter.textureType = UnityEditor.TextureImporterType.NormalMap;
                else
                {
                    texImporter.textureType = UnityEditor.TextureImporterType.Default;
                    texImporter.alphaSource = UnityEditor.TextureImporterAlphaSource.FromInput;
                    texImporter.sRGBTexture = texImporter.alphaIsTransparency = (outType != BakeShaderOutputType.MASK);
                }
                texImporter.SaveAndReimport();
                texture = UnityEditor.AssetDatabase.LoadAssetAtPath<Texture2D>(SaveName);
            }
#endif
            return texture;
        }
        public static void SetExportedTextureSettings(Texture2D texture)
        {
            SetExportedTextureSettings(GetAssetPath(texture));
        }
        public static void SetExportedTextureSettings(Texture2D[] textures)
        {
            for (int t = 0; t < textures.Length; ++t)
                SetExportedTextureSettings(GetAssetPath(textures[t]));
        }
        public static void SetExportedTextureSettings(string textureFileName)
        {
#if UNITY_EDITOR
            var importer = AssetImporter.GetAtPath(textureFileName) as TextureImporter;
            if (importer != null)
            {
                importer.alphaSource = TextureImporterAlphaSource.None;
                importer.alphaIsTransparency = false;
                importer.sRGBTexture = false;
                importer.maxTextureSize = 8192;
                importer.wrapMode = TextureWrapMode.Clamp;
                importer.mipmapEnabled = false;
                importer.npotScale = TextureImporterNPOTScale.None;
                TextureImporterPlatformSettings texImpPlatSet = new TextureImporterPlatformSettings();
                texImpPlatSet.maxTextureSize = 8192;
                texImpPlatSet.format = TextureImporterFormat.R16;
                texImpPlatSet.textureCompression = TextureImporterCompression.Uncompressed;
                importer.SetPlatformTextureSettings(texImpPlatSet);
                AssetDatabase.ImportAsset(textureFileName);
            }
#endif
        }
        public static void SaveFloats(float[] data, string filename)
        {
            Stream stream;
            if (File.Exists(filename))
                stream = File.OpenWrite(filename);
            else
                stream = File.Create(filename);
            System.IO.Compression.GZipStream zipStream = new System.IO.Compression.GZipStream(stream, System.IO.Compression.CompressionMode.Compress);
            SaveFloats(data, zipStream);
            zipStream.Close();
            zipStream.Dispose();
            stream.Close();
            stream.Dispose();
        }
        public static void SaveFloats(float[] data, Stream stream)
        {
            byte[] buf = BitConverter.GetBytes(data.Length);
            stream.Write(buf, 0, buf.Length);
            for (int i = 0; i < data.Length; ++i)
            {
                buf = BitConverter.GetBytes(data[i]);
                stream.Write(buf, 0, buf.Length);
            }
        }
        public static float[] ReadFloats(string filename)
        {
            if (!File.Exists(filename))
            {
                Debug.LogError("Unable to load float data. File not Found: " + filename);
                return Array.Empty<float>();
            }
            Stream stream = File.OpenRead(filename);
            System.IO.Compression.GZipStream zipStream = new System.IO.Compression.GZipStream(stream, System.IO.Compression.CompressionMode.Decompress);
            float[] ret = ReadFloats(zipStream);
            zipStream.Close();
            zipStream.Dispose();
            stream.Close();
            stream.Dispose();
            return ret;
        }
        public static float[] ReadFloats(Stream stream)
        {
            byte[] buf = new byte[4];
            if (stream.Read(buf, 0, 4) < 4)
            {
                Debug.LogError("Unable to load float data. Unexpected EOS");
                return Array.Empty<float>();
            }
            int len = BitConverter.ToInt32(buf, 0);
            if (len < 0)
            {
                Debug.LogError("Unable to load float data. Invalid Length: " + len);
                return Array.Empty<float>();
            }
            float[] ret = new float[len];
            for (int i = 0; i < len; ++i)
            {
                if (stream.Read(buf, 0, 4) < 4)
                {
                    Debug.LogError("Unable to load float data. Unexpected EOS");
                    return Array.Empty<float>();
                }
                ret[i] = BitConverter.ToSingle(buf, 0);
            }
            return ret;
        }
        public static void SaveUShorts(ushort[] data, string filename)
        {
            Stream stream;
            if (File.Exists(filename))
                stream = File.OpenWrite(filename);
            else
                stream = File.Create(filename);
            System.IO.Compression.GZipStream zipStream = new System.IO.Compression.GZipStream(stream, System.IO.Compression.CompressionMode.Compress);
            SaveUShorts(data, zipStream);
            zipStream.Close();
            zipStream.Dispose();
            stream.Close();
            stream.Dispose();
        }
        public static void SaveUShorts(ushort[] data, Stream stream)
        {
            byte[] buf = BitConverter.GetBytes(data.Length);
            stream.Write(buf, 0, buf.Length);
            for (int i = 0; i < data.Length; ++i)
            {
                buf = BitConverter.GetBytes(data[i]);
                stream.Write(buf, 0, buf.Length);
            }
        }
        public static ushort[] ReadUShorts(string filename)
        {
            if (!File.Exists(filename))
            {
                Debug.LogError("Unable to load ushort data. File not Found: " + filename);
                return Array.Empty<ushort>();
            }
            Stream stream = File.OpenRead(filename);
            System.IO.Compression.GZipStream zipStream = new System.IO.Compression.GZipStream(stream, System.IO.Compression.CompressionMode.Decompress);
            ushort[] ret = ReadUShorts(zipStream);
            zipStream.Close();
            zipStream.Dispose();
            stream.Close();
            stream.Dispose();
            return ret;
        }
        public static ushort[] ReadUShorts(Stream stream)
        {
            byte[] buf = new byte[4];
            if (stream.Read(buf, 0, 4) < 4)
            {
                Debug.LogError("Unable to load ushort data. Unexpected EOS");
                return Array.Empty<ushort>();
            }
            int len = BitConverter.ToInt32(buf, 0);
            if (len < 0)
            {
                Debug.LogError("Unable to load ushort data. Invalid Length: " + len);
                return Array.Empty<ushort>();
            }
            ushort[] ret = new ushort[len];
            buf = new byte[len * 2];
            if (stream.Read(buf, 0, buf.Length) < buf.Length)
            {
                Debug.LogError("Unable to load ushort data. Unexpected EOS");
                return Array.Empty<ushort>();
            }

            for (int i = 0; i < len; ++i)
            {
                ret[i] = BitConverter.ToUInt16(buf, i * 2);
            }
            return ret;
        }
        public static void ClearTexture(RenderTexture rt, Color clearColor)
        {
            Texture2D readableText = new Texture2D(rt.width, rt.height, TextureFormat.RGBAFloat, false);
            RenderTexture oldRT = RenderTexture.active;
            RenderTexture.active = rt;
            readableText.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
            readableText.Apply(true, false);
            Color[] pixels = readableText.GetPixels();
            for (int p = 0; p < pixels.Length; ++p)
            {
                if (pixels[p] == clearColor)
                    pixels[p] = Color.clear;
            }
            readableText.SetPixels(pixels);
            Graphics.Blit(readableText, rt);
            RenderTexture.active = oldRT;
        }
        public static void CopyFromAlpha(RenderTexture source, RenderTexture target)
        {
            RenderTexture oldRT = RenderTexture.active;

            Texture2D readableText = new Texture2D(source.width, source.height, TextureFormat.RGBAFloat, false);
            RenderTexture.active = source;
            readableText.ReadPixels(new Rect(0, 0, source.width, source.height), 0, 0);
            readableText.Apply(true, false);
            Color[] sourcePixels = readableText.GetPixels();

            readableText = new Texture2D(target.width, target.height, TextureFormat.RGBAFloat, false);
            RenderTexture.active = target;
            readableText.ReadPixels(new Rect(0, 0, target.width, target.height), 0, 0);
            readableText.Apply(true, false);
            Color[] targetPixels = readableText.GetPixels();

            for (int p = 0; p < targetPixels.Length; ++p)
            {
                Color targetColor = targetPixels[p];
                if (sourcePixels[p].a > 0)
                    targetColor.a = 1;
                else
                    targetColor.a = 0;
                targetPixels[p] = targetColor;
            }
            readableText.SetPixels(targetPixels);
            Graphics.Blit(readableText, target);
            RenderTexture.active = oldRT;
        }
        public static Object SaveResource(Object obj, string path)
        {
#if UNITY_EDITOR
            if (!UnityEditor.AssetDatabase.Contains(obj))
            {
                if (!Directory.Exists("Assets/Storm User Data/Resources/"))
                    Directory.CreateDirectory("Assets/Storm User Data/Resources/");
                UnityEditor.AssetDatabase.CreateAsset(obj, "Assets/Storm User Data/Resources/" + path);
                obj = Resources.Load(path);
            }
            else
                UnityEditor.EditorUtility.SetDirty(obj);
#endif
            return obj;
        }
        public static void SaveAsset(Object obj, string path)
        {
#if UNITY_EDITOR
            if (!UnityEditor.AssetDatabase.Contains(obj))
            {
                EnsureDirExists(path);
                UnityEditor.AssetDatabase.CreateAsset(obj, path);
            }
            else
                UnityEditor.EditorUtility.SetDirty(obj);
#endif
        }
        public static string GetAssetPath(Object obj)
        {
#if UNITY_EDITOR
            return UnityEditor.AssetDatabase.GetAssetPath(obj) ?? "";
#else
            return "";
#endif
        }
        public static long GetAssetWriteTime(Object obj)
        {
#if UNITY_EDITOR
            string tPath = StormUtils.GetAssetPath(obj);
            if (!string.IsNullOrEmpty(tPath))
            {
                FileInfo tInfo = new FileInfo(tPath);
                if (tInfo.Exists)
                    return tInfo.LastWriteTime.ToFileTime();
            }
#endif
            return 0;
        }
        public static T LoadAssetGUID<T>(string guid) where T : Object
        {
            T obj = default(T);
#if UNITY_EDITOR
            obj = UnityEditor.AssetDatabase.LoadAssetAtPath<T>(UnityEditor.AssetDatabase.GUIDToAssetPath(guid));
#endif
            return obj;
        }
        public static T SaveAsset<T>(T obj, string path) where T : Object
        {
            if (obj == null)
            {
                return null;
            }

#if UNITY_EDITOR
            if (!UnityEditor.AssetDatabase.Contains(obj))
            {
                EnsureDirExists(path);
                UnityEditor.AssetDatabase.CreateAsset(obj, path);
                UnityEditor.AssetDatabase.Refresh();
                obj = UnityEditor.AssetDatabase.LoadAssetAtPath<T>(path);
            }
            else
            {
                UnityEditor.EditorUtility.SetDirty(obj);
                UnityEditor.AssetDatabase.SaveAssetIfDirty(obj);
            }
#endif
            return obj;
        }
        public static T LoadAsset<T>(string path) where T : Object
        {
            T obj = default(T);
#if UNITY_EDITOR
            obj = UnityEditor.AssetDatabase.LoadAssetAtPath<T>(path);
#endif
            return obj;
        }
        public static void DeleteAsset(Object asset)
        {
            if (asset == null)
                return;
#if UNITY_EDITOR
            string path = UnityEditor.AssetDatabase.GetAssetPath(asset);
            if (path != null)
                UnityEditor.AssetDatabase.DeleteAsset(path);
            CheckDirEmpty(path);
#endif
        }
        public static bool IsPrefab(GameObject go)
        {
#if UNITY_EDITOR
            return UnityEditor.PrefabUtility.GetPrefabAssetType(go) != UnityEditor.PrefabAssetType.NotAPrefab;
#else
            return false;
#endif
        }
        public static GameObject GetPrefab(GameObject go)
        {
#if UNITY_EDITOR
            string path = UnityEditor.PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(go);
            if (!string.IsNullOrEmpty(path))
                return UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(path);
#endif
            return go;
        }
        public static void SetAssetDirty(Object obj)
        {
#if UNITY_EDITOR
            if (obj != null)
                UnityEditor.EditorUtility.SetDirty(obj);
#endif
        }
        public static void RefreshAssetDatabase()
        {
#if UNITY_EDITOR
            UnityEditor.AssetDatabase.Refresh();
#endif
        }
        private static Action OnUpdateMessage;
        private static bool hasBeenSetup = false;
        public static void AddUpdateListener(Action callback)
        {
            OnUpdateMessage += callback;
            if (!hasBeenSetup)
            {
                hasBeenSetup = true;
#if UNITY_EDITOR
                EditorApplication.playModeStateChanged -= EditorApplication_playModeStateChanged;
                EditorApplication.playModeStateChanged += EditorApplication_playModeStateChanged;
                UnityEditor.Compilation.CompilationPipeline.assemblyCompilationFinished -= CompilationPipeline_assemblyCompilationFinished;
                UnityEditor.Compilation.CompilationPipeline.assemblyCompilationFinished += CompilationPipeline_assemblyCompilationFinished;
                UnityEditor.Compilation.CompilationPipeline.compilationFinished -= CompilationPipeline_compilationFinished;
                UnityEditor.Compilation.CompilationPipeline.compilationFinished += CompilationPipeline_compilationFinished;
                UnityEditor.AssetDatabase.importPackageCompleted -= AssetDatabase_importPackageCompleted;
                UnityEditor.AssetDatabase.importPackageCompleted += AssetDatabase_importPackageCompleted;
#endif
            }
        }
        public static void RemoveUpdateListener(Action callback)
        {
            OnUpdateMessage -= callback;
        }

        public static bool AnyMatch<T>(ICollection<StormSourceIndexInfo> a, T b) where T : IEquatable<T>
        {
            if (a == null || b == null)
                return false;
            foreach (StormSourceIndexInfo x in a)
                if (b.Equals(x.Index))
                    return true;
            return false;
        }
        public static bool AnyMatch<T>(ICollection<StormSourceIndexInfo> a, ICollection<T> b) where T : IEquatable<T>
        {
            if (a == null || b == null)
                return false;
            foreach (StormSourceIndexInfo x in a)
                foreach (T y in b)
                    if (y.Equals(x.Index))
                        return true;
            return false;
        }
        public static bool AnyMatch<T>(ICollection<T> a, ICollection<T> b) where T : IEquatable<T>
        {
            if (a == null || b == null)
                return false;
            foreach (T x in a)
                foreach (T y in b)
                    if (y.Equals(x))
                        return true;
            return false;
        }
        public static bool AnyMatch<T>(ICollection<T> a, T b) where T : IEquatable<T>
        {
            if (a == null)
                return false;
            foreach (T x in a)
                if (x.Equals(b))
                    return true;
            return false;
        }
        public delegate bool MatchDelegate<T>(T a, T b);
        public static bool AnyMatch<T>(ICollection<T> a, T b, MatchDelegate<T> comparisonDelegate)
        {
            if (a == null)
                return false;
            foreach (T x in a)
                if (comparisonDelegate(x, b))
                    return true;
            return false;
        }
        public static bool AnyMatch(ICollection<Object> a, ICollection<Object> b)
        {
            if (a == null || b == null)
                return false;
            foreach (Object x in a)
                foreach (Object y in b)
                    if (y.Equals(x))
                        return true;
            return false;
        }
        public static bool AnyMatch(ICollection<Object> a, Object b)
        {
            if (a == null)
                return false;
            foreach (Object x in a)
                if (x != null && x.Equals(b))
                    return true;
            return false;
        }
        public static bool Match<T>(ICollection<T> a, ICollection<T> b)
        {
            if (a == null)
                return b == null || b.Count == 0;
            else if (b == null)
                return a.Count == 0;

            if (a.Count != b.Count)
                return false;
            var aEnum = a.GetEnumerator();
            var bEnum = b.GetEnumerator();
            while (aEnum.MoveNext() && bEnum.MoveNext())
            {
                if (!aEnum.Current.Equals(bEnum.Current))
                    return false;
            }

            return true;
        }
        public static bool Match<T>(Span<T> a, Span<T> b)
        {
            if (a == null)
                return b == null || b.Length == 0;
            else if (b == null)
                return a.Length == 0;

            if (a.Length != b.Length)
                return false;
            var aEnum = a.GetEnumerator();
            var bEnum = b.GetEnumerator();
            while (aEnum.MoveNext() && bEnum.MoveNext())
            {
                if (!aEnum.Current.Equals(bEnum.Current))
                    return false;
            }

            return true;
        }
        public static bool Match<T>(ReadOnlySpan<T> a, ReadOnlySpan<T> b)
        {
            if (a == null)
                return b == null || b.Length == 0;
            else if (b == null)
                return a.Length == 0;

            if (a.Length != b.Length)
                return false;
            var aEnum = a.GetEnumerator();
            var bEnum = b.GetEnumerator();
            while (aEnum.MoveNext() && bEnum.MoveNext())
            {
                if (!aEnum.Current.Equals(bEnum.Current))
                    return false;
            }

            return true;
        }
        /// <summary>
        /// Returns true if all items in A are found in B
        /// </summary>
        public static bool MatchLoose<T>(ICollection<T> a, ICollection<T> b)
        {
            if (a == null)
                return b == null || b.Count == 0;
            else if (b == null)
                return a.Count == 0;

            if (a.Count != b.Count)
                return false;
            var aEnum = a.GetEnumerator();
            bool[] foundB = new bool[b.Count];
            while (aEnum.MoveNext())
            {
                var bEnum = b.GetEnumerator();
                bool found = false;
                int bIdx = -1;
                while (bEnum.MoveNext())
                {
                    ++bIdx;
                    if (aEnum.Current.Equals(bEnum.Current))
                    {
                        foundB[bIdx] = true;
                        found = true;
                        break;
                    }
                }
                if (!found)
                    return false;
            }
            for (int i = 0; i < foundB.Length; ++i)
                if (!foundB[i])
                    return false;
            return true;
        }
        /// <summary>
        /// Returns true if all items in A are found in B
        /// </summary>
        public static bool MatchLoose<T>(Span<T> a, Span<T> b)
        {
            if (a == null)
                return b == null || b.Length == 0;
            else if (b == null)
                return a.Length == 0;

            if (a.Length != b.Length)
                return false;
            var aEnum = a.GetEnumerator();
            bool[] foundB = new bool[b.Length];
            while (aEnum.MoveNext())
            {
                var bEnum = b.GetEnumerator();
                bool found = false;
                int bIdx = -1;
                while (bEnum.MoveNext())
                {
                    ++bIdx;
                    if (aEnum.Current.Equals(bEnum.Current))
                    {
                        foundB[bIdx] = true;
                        found = true;
                        break;
                    }
                }
                if (!found)
                    return false;
            }
            for (int i = 0; i < foundB.Length; ++i)
                if (!foundB[i])
                    return false;
            return true;
        }
        /// <summary>
        /// Returns true if all items in A are found in B
        /// </summary>
        public static bool MatchLoose<T>(ReadOnlySpan<T> a, ReadOnlySpan<T> b)
        {
            if (a == null)
                return b == null || b.Length == 0;
            else if (b == null)
                return a.Length == 0;

            if (a.Length != b.Length)
                return false;
            var aEnum = a.GetEnumerator();
            bool[] foundB = new bool[b.Length];
            while (aEnum.MoveNext())
            {
                var bEnum = b.GetEnumerator();
                bool found = false;
                int bIdx = -1;
                while (bEnum.MoveNext())
                {
                    ++bIdx;
                    if (aEnum.Current.Equals(bEnum.Current))
                    {
                        foundB[bIdx] = true;
                        found = true;
                        break;
                    }
                }
                if (!found)
                    return false;
            }
            for (int i = 0; i < foundB.Length; ++i)
                if (!foundB[i])
                    return false;
            return true;
        }
        public static T[] CopyArray<T>(T[] original)
        {
            T[] ret = new T[original.Length];
            Array.Copy(original, ret, ret.Length);
            return ret;
        }
        public static T[] CopyArray<T>(NativeArray<T> original) where T : unmanaged
        {
            int len = original.Length;
            T[] ret = new T[len];
            for (int i = 0; i < len; ++i)
                ret[i] = original[i];
            return ret;
        }
        public static T[] CopyArray<T>(ICollection<T> original)
        {
            T[] ret = new T[original.Count];
            int i = 0;
            foreach (T item in original)
                ret[i++] = item;
            return ret;
        }
        public static T[] CopyArray<T>(IReadOnlyCollection<T> original)
        {
            T[] ret = new T[original.Count];
            int i = 0;
            foreach (T item in original)
                ret[i++] = item;
            return ret;
        }
        public delegate T ObjectCopyDelegate<T>(T item);
        public static T[] CopyObjectArray<T>(ICollection<T> original, ObjectCopyDelegate<T> copyFunc)
        {
            T[] ret = new T[original.Count];
            int i = 0;
            foreach (T item in original)
                ret[i++] = copyFunc(item);
            return ret;
        }
        public static void AddIfNew<T>(List<T> list, T item)
        {
            if (!list.Contains(item))
                list.Add(item);
        }
        public static void AddIfNew<T>(ICollection<T> list, ICollection<T> items)
        {
            foreach (T item in items)
            {
                if (!list.Contains(item))
                    list.Add(item);
            }
        }
        public static void AddIfNew<T>(ICollection<T> list, ICollection<T> items, Comparison<T> comparer)
        {
            foreach (T item in items)
            {
                bool found = false;
                foreach (T lItem in list)
                {
                    if (comparer(item, lItem) == 0)
                    {
                        found = true;
                        break;
                    }
                }
                if (!found)
                    list.Add(item);
            }
        }
        public static void AddIfNew<T>(ref T[] array, T item)
        {
            int len = array?.Length ?? 0;
            for (int i = 0; i < len; ++i)
            {
                if (array[i].Equals(item))
                    return;
            }
            Array.Resize(ref array, len + 1);
            array[len] = item;
        }
        public static void AddIfNew<T>(ref T[] array, T item, Comparison<T> comparer)
        {
            int len = array?.Length ?? 0;
            for (int i = 0; i < len; ++i)
            {
                if (comparer.Invoke(array[i], item) == 0)
                    return;
            }
            Array.Resize(ref array, len + 1);
            array[len] = item;
        }
        public static void AddIfNew<T>(ref T[] array, ICollection<T> items)
        {
            int len = array?.Length ?? 0;
            int toAdd = 0;
            foreach (T item in items)
            {
                for (int i = 0; i < len; ++i)
                {
                    if (!array[i].Equals(item))
                        ++toAdd;
                }
            }
            if (toAdd == 0)
                return;
            Array.Resize(ref array, len + toAdd);
            toAdd = 0;
            foreach (T item in items)
            {
                for (int i = 0; i < len; ++i)
                {
                    if (!array[i].Equals(item))
                        array[toAdd++] = item;
                }
            }
        }
        public static void AddIfNew<T>(ref T[] array, ICollection<T> items, Comparison<T> comparer)
        {
            int len = array?.Length ?? 0;
            int toAdd = 0;
            foreach (T item in items)
            {
                for (int i = 0; i < len; ++i)
                {
                    if (comparer.Invoke(array[i], item) != 0)
                        ++toAdd;
                }
            }
            if (toAdd == 0)
                return;
            Array.Resize(ref array, len + toAdd);
            toAdd = 0;
            foreach (T item in items)
            {
                for (int i = 0; i < len; ++i)
                {
                    if (comparer.Invoke(array[i], item) != 0)
                        array[toAdd++] = item;
                }
            }
        }
        public static void Add<T>(ICollection<T> list, ICollection<T> items)
        {
            foreach (T item in items)
                list.Add(item);
        }
        public static void Add<T>(ref T[] array, T item)
        {
            int len = array?.Length ?? 0;
            Array.Resize(ref array, len + 1);
            array[len] = item;
        }
        public static void Add<T>(ref T[] array, ICollection<T> items)
        {
            int len = array?.Length ?? 0;
            Array.Resize(ref array, len + items.Count);
            int idx = len;
            foreach (T item in items)
            {
                array[idx++] = item;
            }
        }
        public static void Insert<T>(ref T[] array, int index, T item)
        {
            int len = array?.Length ?? 0;
            if (index < 0 || index > len)
                throw new IndexOutOfRangeException("Index " + index + " is outside the range [0-" + len + "]");
            Array.Resize(ref array, len + 1);
            for (int i = len - 1; i >= index; --i)
                array[i + 1] = array[i];
            array[index] = item;
        }
        public static void Insert<T>(ref T[] array, int index, ICollection<T> items, int iIndex = 0, int iCount = -1)
        {
            int len = array?.Length ?? 0;
            if (index < 0 || index > len)
                throw new IndexOutOfRangeException("Index " + index + " is outside the range [0-" + len + "]");
            if (iCount < 0)
                iCount = items.Count;
            if (iIndex < 0 || iIndex >= items.Count)
                throw new IndexOutOfRangeException("iIndex " + iIndex + " is outside the range [0-" + items.Count + ")");
            if (iIndex + iCount > items.Count)
                throw new IndexOutOfRangeException("iIndex " + iIndex + " + iCount " + iCount + " > items Length " + items.Count);
            Array.Resize(ref array, len + iCount);
            for (int i = len - 1; i >= index; --i)
                array[i + iCount] = array[i];
            var iEnum = items.GetEnumerator();
            for (int i = 0; i < iIndex; ++i)
                iEnum.MoveNext();
            int iIdx = 0;
            while (iIdx++ < iCount && iEnum.MoveNext())
                array[index++] = iEnum.Current;
        }

        /// <summary>
        /// Returns true if any of the strings in "parts" are found in the string
        /// </summary>
        /// <param name="str">String to check</param>
        /// <param name="parts">Parts to search for inside of String</param>
        /// <returns></returns>
        public static bool ContainsAny(string str, string[] parts, StringComparison comparison = StringComparison.CurrentCultureIgnoreCase)
        {
            for (int i = 0; i < parts.Length; ++i)
                if (str.Contains(parts[i], comparison))
                    return true;
            return false;
        }

        public static void RemoveAt<T>(ref T[] array, int indexToRemove)
        {
            if (indexToRemove < 0 || indexToRemove >= array?.Length)
                return;
            for (int i = indexToRemove; i < array.Length - 1; ++i)
            {
                array[i] = array[i + 1];
            }
            Array.Resize(ref array, array.Length - 1);
        }

        public static Texture2D[] GetDetailTextures(TerrainData terrainData, TextureFormat textureFormat = TextureFormat.RGBA32, bool readable = false)
        {
            float[][,] Detail = GetDetailMaps(terrainData);
            int detailCount = Detail.Length;
            if (detailCount == 0)
                return Array.Empty<Texture2D>();
            int mapResolutionX = Detail[0].GetLength(0);
            int mapResolutionY = Detail[0].GetLength(1);
            int numMaps = Mathf.CeilToInt(detailCount / 4f);
            Color[][] pixels = new Color[numMaps][];
            for (int t = 0; t < numMaps; ++t)
                pixels[t] = new Color[mapResolutionX * mapResolutionY];
            for (int y = 0; y < mapResolutionY; ++y)
            {
                for (int x = 0; x < mapResolutionX; ++x)
                {
                    for (int t = 0; t < numMaps; ++t)
                    {
                        int layerStart = t * 4;
                        Color c = Color.clear;
                        c.r = Detail[layerStart][y, x] / 16f;
                        if (layerStart + 1 < detailCount)
                            c.g = Detail[layerStart + 1][y, x] / 16f;
                        if (layerStart + 2 < detailCount)
                            c.b = Detail[layerStart + 2][y, x] / 16f;
                        if (layerStart + 3 < detailCount)
                            c.a = Detail[layerStart + 3][y, x] / 16f;
                        pixels[t][y * mapResolutionX + x] = c;
                    }
                }
            }
            Texture2D[] ret = new Texture2D[numMaps];
            for (int t = 0; t < numMaps; ++t)
            {
                ret[t] = new Texture2D(mapResolutionX, mapResolutionY, textureFormat, true);
                ret[t].SetPixels(pixels[t]);
                ret[t].Apply(true, !readable);
            }
            return ret;
        }

        public static float[][,] GetDetailMaps(TerrainData terrainData)
        {
            float[][,] detailAlphas = new float[terrainData.detailPrototypes.Length][,];
            for (int i = 0; i < terrainData.detailPrototypes.Length; i++)
            {
                float[,] detail = new float[terrainData.detailWidth, terrainData.detailHeight];
                var layer = terrainData.GetDetailLayer(0, 0, terrainData.detailWidth, terrainData.detailHeight, i);
                for (var y = 0; y < terrainData.detailHeight; y++)
                {
                    for (var x = 0; x < terrainData.detailWidth; x++)
                    {
                        detail[x, y] = layer[x, y];
                    }
                }
                detailAlphas[i] = detail;
            }
            return detailAlphas;
        }
        public static void GetDetailSettings(DetailPrototype detailPrototype, out float AlignToUp, out Vector4 Jitter)
        {
#if UNITY_2022_2_OR_NEWER
            AlignToUp = 1f - detailPrototype.alignToGround;
            if (QualitySettings.useLegacyDetailDistribution)
                Jitter = new Vector4(1, 1, 0, 0);
            else
                Jitter = new Vector4(detailPrototype.positionJitter, detailPrototype.positionJitter, 0, 0);
#else
            AlignToUp = 0;
            Jitter = 1;
#endif
        }
#if UNITY_EDITOR
        private static void AssetDatabase_importPackageCompleted(string packageName)
        {
            OnUpdateMessage?.Invoke();
        }
        private static Action OnCompiledAction;
        private static void CompilationPipeline_assemblyCompilationFinished(string arg1, UnityEditor.Compilation.CompilerMessage[] arg2)
        {
            OnUpdateMessage?.Invoke();
            OnCompiledAction?.Invoke();
        }

        private static void CompilationPipeline_compilationFinished(object obj)
        {
            OnUpdateMessage?.Invoke();
            OnCompiledAction?.Invoke();
        }

        private static void EditorApplication_playModeStateChanged(PlayModeStateChange stateChange)
        {
            if (stateChange == PlayModeStateChange.ExitingPlayMode || stateChange == PlayModeStateChange.ExitingEditMode)
                OnCompiledAction?.Invoke();
        }

#endif
        public static Mesh GetDebugMesh(float[] data, int dataResolution, float dataScale, int dataPadding)
        {
            Mesh ret = new Mesh();
            float step = 1f / (dataResolution - 1 - (dataPadding * 2));
            float offset = -dataPadding * step;
            Vector3[] positions = new Vector3[dataResolution * dataResolution];
            int indexResolution = dataResolution - 1;
            int[] indices = new int[indexResolution * indexResolution * 6];
            for (int y = 0; y < dataResolution; ++y)
            {
                for (int x = 0; x < dataResolution; ++x)
                {
                    int index = y * dataResolution + x;
                    Vector3 pos = new Vector3(offset + x * step, 0, offset + y * step);
                    if (data.Length <= index)
                    {
                        Debug.LogError("Index " + index + " is out of range for Array with length " + data.Length + ". Resolution=" + dataResolution);
                        return null;
                    }
                    pos.y = data[index] * dataScale;
                    positions[index] = pos;
                }
            }
            for (int y = 0; y < indexResolution; ++y)
            {
                for (int x = 0; x < indexResolution; ++x)
                {
                    int startIdx = (y * indexResolution + x) * 6;
                    indices[startIdx] = y * dataResolution + x;
                    indices[startIdx + 1] = (y + 1) * dataResolution + x;
                    indices[startIdx + 2] = y * dataResolution + x + 1;
                    indices[startIdx + 3] = indices[startIdx + 2];
                    indices[startIdx + 4] = indices[startIdx + 1];
                    indices[startIdx + 5] = (y + 1) * dataResolution + x + 1;
                }
            }
            ret.SetVertices(positions);
            ret.SetIndices(indices, MeshTopology.Triangles, 0);
            ret.RecalculateBounds();
            ret.RecalculateTangents();
            ret.RecalculateNormals();
            ret.UploadMeshData(true);
            return ret;
        }
        public static Texture2D GenerateTexture(float[] values, float min, float range, int2 resolution, TextureFormat format = TextureFormat.RGBAFloat)
        {
            Texture2D ret = new Texture2D(resolution.x, resolution.y, format, false);
            Color[] pixels = new Color[resolution.x * resolution.y];
            for (int i = 0; i < pixels.Length; ++i)
            {
                float val = (values[i] - min) / range;
                pixels[i] = new Color(val, val, val, 1);
            }
            ret.SetPixels(pixels);
            ret.Apply();
            return ret;
        }
        public static Texture2D GenerateTexture(float[] values, float min, float range, int2 resolution, StormTextureChannel channel, TextureFormat format = TextureFormat.RGBAFloat)
        {
            Texture2D ret = new Texture2D(resolution.x, resolution.y, format, false);
            Color[] pixels = new Color[resolution.x * resolution.y];
            for (int i = 0; i < pixels.Length; ++i)
            {
                float val = (values[i] - min) / range;
                pixels[i] = GetColor(channel, val);
            }
            ret.SetPixels(pixels);
            ret.Apply();
            return ret;
        }
        public static Texture2D GenerateTexture(int[] values, int min, int range, int2 resolution, TextureFormat format = TextureFormat.RGBAFloat)
        {
            Texture2D ret = new Texture2D(resolution.x, resolution.y, format, false);
            Color[] pixels = new Color[resolution.x * resolution.y];
            for (int i = 0; i < pixels.Length; ++i)
            {
                float val = (values[i] - min) / (float)range;
                pixels[i] = new Color(val, val, val, 1);
            }
            ret.SetPixels(pixels);
            ret.Apply();
            return ret;
        }
        public static Texture2D GenerateTexture(int[] values, int min, int range, int2 resolution, StormTextureChannel channel, TextureFormat format = TextureFormat.RGBAFloat)
        {
            Texture2D ret = new Texture2D(resolution.x, resolution.y, format, false);
            Color[] pixels = new Color[resolution.x * resolution.y];
            for (int i = 0; i < pixels.Length; ++i)
            {
                float val = (values[i] - min) / (float)range;
                pixels[i] = GetColor(channel, val);
            }
            ret.SetPixels(pixels);
            ret.Apply();
            return ret;
        }
        public static Texture2D GenerateTexture(ushort[] values, int2 resolution, TextureFormat format = TextureFormat.RGBAFloat)
        {
            Texture2D ret = new Texture2D(resolution.x, resolution.y, format, false);
            Color[] pixels = new Color[resolution.x * resolution.y];
            for (int i = 0; i < pixels.Length; ++i)
            {
                float val = values[i] / (float)ushort.MaxValue;
                pixels[i] = new Color(val, val, val, 1);
            }
            ret.SetPixels(pixels);
            ret.Apply();
            return ret;
        }
        public static Texture2D GenerateTexture(ushort[] values, int2 resolution, StormTextureChannel channel, TextureFormat format = TextureFormat.RGBAFloat)
        {
            Texture2D ret = new Texture2D(resolution.x, resolution.y, format, false);
            Color[] pixels = new Color[resolution.x * resolution.y];
            for (int i = 0; i < pixels.Length; ++i)
            {
                float val = values[i] / (float)ushort.MaxValue;
                pixels[i] = GetColor(channel, val);
            }
            ret.SetPixels(pixels);
            ret.Apply();
            return ret;
        }
        public static Texture2D GenerateTexture(bool[] values, float TrueValue, float FalseValue, int2 resolution, TextureFormat format = TextureFormat.RGBAFloat)
        {
            Texture2D ret = new Texture2D(resolution.x, resolution.y, format, false);
            Color[] pixels = new Color[resolution.x * resolution.y];
            for (int i = 0; i < pixels.Length; ++i)
            {
                float val = values[i] ? TrueValue : FalseValue;
                pixels[i] = new Color(val, val, val, 1);
            }
            ret.SetPixels(pixels);
            ret.Apply();
            return ret;
        }
        public static Texture2D GenerateTexture(float[,] values, float min, float range, TextureFormat format = TextureFormat.RGBAFloat)
        {
            int2 resolution = math.int2(values.GetLength(0), values.GetLength(1));
            Texture2D ret = new Texture2D(resolution.x, resolution.y, format, false);
            Color[] pixels = new Color[resolution.x * resolution.y];
            for (int y = 0; y < resolution.y; ++y)
            {
                for (int x = 0; x < resolution.x; ++x)
                {
                    float val = (values[x, y] - min) / range;
                    pixels[y * resolution.x + x] = new Color(val, val, val, 1);
                }
            }
            ret.SetPixels(pixels);
            ret.Apply();
            return ret;
        }
        public static Texture2D GenerateTexture(float[,] values, float min, float range, StormTextureChannel channel, TextureFormat format = TextureFormat.RGBAFloat)
        {
            int2 resolution = math.int2(values.GetLength(0), values.GetLength(1));
            Texture2D ret = new Texture2D(resolution.x, resolution.y, format, false);
            Color[] pixels = new Color[resolution.x * resolution.y];
            for (int y = 0; y < resolution.y; ++y)
            {
                for (int x = 0; x < resolution.x; ++x)
                {
                    float val = (values[x, y] - min) / range;
                    pixels[y * resolution.x + x] = GetColor(channel, val);
                }
            }
            ret.SetPixels(pixels);
            ret.Apply();
            return ret;
        }
        public static Texture2D GenerateTexture(int[,] values, int min, int range, TextureFormat format = TextureFormat.RGBAFloat)
        {
            int2 resolution = new int2(values.GetLength(0), values.GetLength(1));
            Texture2D ret = new Texture2D(resolution.x, resolution.y, format, false);
            Color[] pixels = new Color[resolution.x * resolution.y];
            for (int y = 0; y < resolution.y; ++y)
            {
                for (int x = 0; x < resolution.x; ++x)
                {
                    float val = (values[x,y] - min) / (float)range;
                    pixels[y * resolution.x + x] = new Color(val, val, val, 1);
                }
            }
            ret.SetPixels(pixels);
            ret.Apply();
            return ret;
        }
        public static Texture2D GenerateTexture(int[,] values, int min, int range, StormTextureChannel channel, TextureFormat format = TextureFormat.RGBAFloat)
        {
            int2 resolution = new int2(values.GetLength(0), values.GetLength(1));
            Texture2D ret = new Texture2D(resolution.x, resolution.y, format, false);
            Color[] pixels = new Color[resolution.x * resolution.y];
            for (int y = 0; y < resolution.y; ++y)
            {
                for (int x = 0; x < resolution.x; ++x)
                {
                    float val = (values[x, y] - min) / (float)range;
                    pixels[y * resolution.x + x] = GetColor(channel, val);
                }
            }
            ret.SetPixels(pixels);
            ret.Apply();
            return ret;
        }
        public static Texture2D GenerateTexture(float[,] red, float[,] green, float[,] blue, float[,] alpha, float min, float range, TextureFormat format = TextureFormat.RGBAFloat)
        {
            int2 resolution = int2.zero;
            if (red != null)
            {
                int resX = red.GetLength(0);
                int resY = red.GetLength(1);
                if (resolution.x == 0 || resolution.x > resX)
                    resolution.x = resX;
                if (resolution.y == 0 || resolution.y > resY)
                    resolution.y = resY;
            }
            if (green != null)
            {
                int resX = green.GetLength(0);
                int resY = green.GetLength(1);
                if (resolution.x == 0 || resolution.x > resX)
                    resolution.x = resX;
                if (resolution.y == 0 || resolution.y > resY)
                    resolution.y = resY;
            }
            if (blue != null)
            {
                int resX = blue.GetLength(0);
                int resY = blue.GetLength(1);
                if (resolution.x == 0 || resolution.x > resX)
                    resolution.x = resX;
                if (resolution.y == 0 || resolution.y > resY)
                    resolution.y = resY;
            }
            if (alpha != null)
            {
                int resX = alpha.GetLength(0);
                int resY = alpha.GetLength(1);
                if (resolution.x == 0 || resolution.x > resX)
                    resolution.x = resX;
                if (resolution.y == 0 || resolution.y > resY)
                    resolution.y = resY;
            }
            Texture2D ret = new Texture2D(resolution.x, resolution.y, format, false);
            Color[] pixels = new Color[resolution.x * resolution.y];
            for (int y = 0; y < resolution.y; ++y)
            {
                for (int x = 0; x < resolution.x; ++x)
                {
                    float r = red == null ? 0 : (red[x, y] - min) / range;
                    float g = green == null ? 0 : (green[x, y] - min) / range;
                    float b = blue == null ? 0 : (blue[x, y] - min) / range;
                    float a = alpha == null ? 0 : (alpha[x, y] - min) / range;
                    pixels[y * resolution.x + x] = new Color(r, g, b, a);
                }
            }
            ret.SetPixels(pixels);
            ret.Apply();
            return ret;
        }
        public static Texture2D GenerateTexture(bool[,] values, float TrueValue, float FalseValue, TextureFormat format = TextureFormat.RGBAFloat)
        {
            int2 resolution = new int2(values.GetLength(0), values.GetLength(1));
            Texture2D ret = new Texture2D(resolution.x, resolution.y, format, false);
            Color[] pixels = new Color[resolution.x * resolution.y];
            for (int y = 0; y < resolution.y; ++y)
            {
                for (int x = 0; x < resolution.x; ++x)
                    {
                    float val = values[x, y] ? TrueValue : FalseValue;
                    pixels[y * resolution.x + x] = new Color(val, val, val, 1);
                }
            }
            ret.SetPixels(pixels);
            ret.Apply();
            return ret;
        }
        public static Texture2D GenerateTexture(bool[,] values, float TrueValue, float FalseValue, StormTextureChannel channel, TextureFormat format = TextureFormat.RGBAFloat)
        {
            int2 resolution = new int2(values.GetLength(0), values.GetLength(1));
            Texture2D ret = new Texture2D(resolution.x, resolution.y, format, false);
            Color[] pixels = new Color[resolution.x * resolution.y];
            for (int y = 0; y < resolution.y; ++y)
            {
                for (int x = 0; x < resolution.x; ++x)
                    {
                    float val = values[x, y] ? TrueValue : FalseValue;
                    pixels[y * resolution.x + x] = GetColor(channel, val);
                }
            }
            ret.SetPixels(pixels);
            ret.Apply();
            return ret;
        }
        public static Texture2D GenerateSplitTexture(float[,,] values, int splitIndex, float min, float range, TextureFormat format = TextureFormat.RGBAFloat)
        {
            int2 resolution = math.int2(values.GetLength(0), values.GetLength(1));
            Texture2D ret = new Texture2D(resolution.x, resolution.y, format, false);
            Color[] pixels = new Color[resolution.x * resolution.y];
            for (int y = 0; y < resolution.y; ++y)
            {
                for (int x = 0; x < resolution.x; ++x)
                {
                    float val = (values[x, y, splitIndex] - min) / range;
                    pixels[y * resolution.x + x] = new Color(val, val, val, 1);
                }
            }
            ret.SetPixels(pixels);
            ret.Apply();
            return ret;
        }
        public static Texture2D GenerateSplitTexture(float[,,] values, int splitIndex, float min, float range, StormTextureChannel channel, TextureFormat format = TextureFormat.RGBAFloat)
        {
            int2 resolution = math.int2(values.GetLength(0), values.GetLength(1));
            Texture2D ret = new Texture2D(resolution.x, resolution.y, format, false);
            Color[] pixels = new Color[resolution.x * resolution.y];
            for (int y = 0; y < resolution.y; ++y)
            {
                for (int x = 0; x < resolution.x; ++x)
                {
                    float val = (values[x, y, splitIndex] - min) / range;
                    pixels[y * resolution.x + x] = GetColor(channel, val);
                }
            }
            ret.SetPixels(pixels);
            ret.Apply();
            return ret;
        }
        public static Texture2D[] GenerateSplitTextures(float[,,] values, float min, float range, TextureFormat format = TextureFormat.RGBAFloat)
        {
            int2 resolution = math.int2(values.GetLength(0), values.GetLength(1));
            Texture2D[] ret = new Texture2D[values.GetLength(2)];
            for (int s = 0; s < ret.Length; ++s)
            {
                Texture2D tex = new Texture2D(resolution.x, resolution.y, format, false);
                Color[] pixels = new Color[resolution.x * resolution.y];
                for (int y = 0; y < resolution.y; ++y)
                {
                    for (int x = 0; x < resolution.x; ++x)
                    {
                        float val = (values[x, y, s] - min) / range;
                        pixels[y * resolution.x + x] = new Color(val, val, val, 1);
                    }
                }
                tex.SetPixels(pixels);
                tex.Apply();
                ret[s] = tex;
            }
            return ret;
        }
        public static Texture2D[] GenerateSplitTextures(float[,,] values, float min, float range, StormTextureChannel channel, TextureFormat format = TextureFormat.RGBAFloat)
        {
            int2 resolution = math.int2(values.GetLength(0), values.GetLength(1));
            Texture2D[] ret = new Texture2D[values.GetLength(2)];
            for (int s = 0; s < ret.Length; ++s)
            {
                Texture2D tex = new Texture2D(resolution.x, resolution.y, format, false);
                Color[] pixels = new Color[resolution.x * resolution.y];
                for (int y = 0; y < resolution.y; ++y)
                {
                    for (int x = 0; x < resolution.x; ++x)
                    {
                        float val = (values[x, y, s] - min) / range;
                        pixels[y * resolution.x + x] = GetColor(channel, val);
                    }
                }
                tex.SetPixels(pixels);
                tex.Apply();
                ret[s] = tex;
            }
            return ret;
        }
        public static Texture2D GenerateTexture(NativeArray<float> values, float min, float range, int2 resolution, TextureFormat format = TextureFormat.RGBAFloat)
        {
            Texture2D ret = new Texture2D(resolution.x, resolution.y, format, false);
            Color[] pixels = new Color[resolution.x * resolution.y];
            for (int i = 0; i < pixels.Length; ++i)
            {
                float val = (values[i] - min) / range;
                pixels[i] = new Color(val, val, val, 1);
            }
            ret.SetPixels(pixels);
            ret.Apply();
            return ret;
        }
        public static Texture2D GenerateTexture(NativeArray<float> values, float min, float range, int2 resolution, StormTextureChannel channel, TextureFormat format = TextureFormat.RGBAFloat)
        {
            Texture2D ret = new Texture2D(resolution.x, resolution.y, format, false);
            Color[] pixels = new Color[resolution.x * resolution.y];
            for (int i = 0; i < pixels.Length; ++i)
            {
                float val = (values[i] - min) / range;
                pixels[i] = GetColor(channel, val);
            }
            ret.SetPixels(pixels);
            ret.Apply();
            return ret;
        }
        public static Texture2D GenerateTexture(NativeArray<float> values, float min, float range, Color colorMin, Color colorMax, int2 resolution, TextureFormat format = TextureFormat.RGBAFloat)
        {
            Texture2D ret = new Texture2D(resolution.x, resolution.y, format, false);
            Color[] pixels = new Color[resolution.x * resolution.y];
            for (int i = 0; i < pixels.Length; ++i)
            {
                float val = (values[i] - min) / range;
                pixels[i] = Color.Lerp(colorMin, colorMax, val);
            }
            ret.SetPixels(pixels);
            ret.Apply();
            return ret;
        }
        public static Texture2D GenerateTexture(NativeArray<int> values, int min, int range, int2 resolution, TextureFormat format = TextureFormat.RGBAFloat)
        {
            Texture2D ret = new Texture2D(resolution.x, resolution.y, format, false);
            Color[] pixels = new Color[resolution.x * resolution.y];
            for (int i = 0; i < pixels.Length; ++i)
            {
                float val = (values[i] - min) / (float)range;
                pixels[i] = new Color(val, val, val, 1);
            }
            ret.SetPixels(pixels);
            ret.Apply();
            return ret;
        }
        public static Texture2D GenerateTexture(NativeArray<byte> values, int2 resolution, TextureFormat format = TextureFormat.RGBAFloat)
        {
            Texture2D ret = new Texture2D(resolution.x, resolution.y, format, false);
            Color[] pixels = new Color[resolution.x * resolution.y];
            for (int i = 0; i < pixels.Length; ++i)
            {
                float val = values[i] / 255f;
                pixels[i] = new Color(val, val, val, 1);
            }
            ret.SetPixels(pixels);
            ret.Apply();
            return ret;
        }
        public static Texture2D GenerateTexture(NativeArray<byte> values, int2 resolution, StormTextureChannel channel, TextureFormat format = TextureFormat.RGBAFloat)
        {
            Texture2D ret = new Texture2D(resolution.x, resolution.y, format, false);
            Color[] pixels = new Color[resolution.x * resolution.y];
            for (int i = 0; i < pixels.Length; ++i)
            {
                float val = values[i] / 255f;
                pixels[i] = GetColor(channel, val);
            }
            ret.SetPixels(pixels);
            ret.Apply();
            return ret;
        }
        public static Texture2D GenerateTexture(NativeArray<bool> values, float TrueValue, float FalseValue, int2 resolution, TextureFormat format = TextureFormat.RGBAFloat)
        {
            Texture2D ret = new Texture2D(resolution.x, resolution.y, format, false);
            Color[] pixels = new Color[resolution.x * resolution.y];
            for (int i = 0; i < pixels.Length; ++i)
            {
                float val = values[i] ? TrueValue : FalseValue;
                pixels[i] = new Color(val, val, val, 1);
            }
            ret.SetPixels(pixels);
            ret.Apply();
            return ret;
        }
#if UNITY_2021_2_OR_NEWER
        public static Texture2D GenerateTexture(Span<float> values, float min, float range, int2 resolution, TextureFormat format = TextureFormat.RGBAFloat)
        {
            Texture2D ret = new Texture2D(resolution.x, resolution.y, format, false);
            Color[] pixels = new Color[resolution.x * resolution.y];
            for (int i = 0; i < pixels.Length; ++i)
            {
                float val = (values[i] - min) / range;
                pixels[i] = new Color(val, val, val, 1);
            }
            ret.SetPixels(pixels);
            ret.Apply();
            return ret;
        }
        public static Texture2D GenerateTexture(Span<float> values, float min, float range, int2 resolution, StormTextureChannel channel, TextureFormat format = TextureFormat.RGBAFloat)
        {
            Texture2D ret = new Texture2D(resolution.x, resolution.y, format, false);
            Color[] pixels = new Color[resolution.x * resolution.y];
            for (int i = 0; i < pixels.Length; ++i)
            {
                float val = (values[i] - min) / range;
                pixels[i] = GetColor(channel, val);
            }
            ret.SetPixels(pixels);
            ret.Apply();
            return ret;
        }
#endif

        public static Texture2D GenerateYFlippedTexture(ushort[] values, int2 resolution, TextureFormat format = TextureFormat.RGBAFloat)
        {
            Texture2D ret = new Texture2D(resolution.x, resolution.y, format, false);
            Color[] pixels = new Color[resolution.x * resolution.y];
            for (int y = 0; y < resolution.y; ++y)
            {
                for (int x = 0; x < resolution.x; ++x)
                {
                    float val = values[(resolution.y - 1 - y) * resolution.x + x] / (float)ushort.MaxValue;
                    pixels[y * resolution.x + x] = new Color(val, val, val, 1);
                }
            }
            ret.SetPixels(pixels);
            ret.Apply();
            return ret;
        }
        public static Texture2D GenerateYFlippedTexture(ushort[] values, int2 resolution, StormTextureChannel channel, TextureFormat format = TextureFormat.RGBAFloat)
        {
            Texture2D ret = new Texture2D(resolution.x, resolution.y, format, false);
            Color[] pixels = new Color[resolution.x * resolution.y];
            for (int y = 0; y < resolution.y; ++y)
            {
                for (int x = 0; x < resolution.x; ++x)
                {
                    float val = values[(resolution.y - 1 - y) * resolution.x + x] / (float)ushort.MaxValue;
                    pixels[y * resolution.x + x] = GetColor(channel, val);
                }
            }
            ret.SetPixels(pixels);
            ret.Apply();
            return ret;
        }
        public static Texture2D GenerateYFlippedTexture(bool[] values, float TrueValue, float FalseValue, int2 resolution, TextureFormat format = TextureFormat.RGBAFloat)
        {
            Texture2D ret = new Texture2D(resolution.x, resolution.y, format, false);
            Color[] pixels = new Color[resolution.x * resolution.y];
            for (int y = 0; y < resolution.y; ++y)
            {
                for (int x = 0; x < resolution.x; ++x)
                {
                    float val = values[(resolution.y - 1 - y) * resolution.x + x] ? TrueValue : FalseValue;
                    pixels[y * resolution.x + x] = new Color(val, val, val, 1);
                }
            }
            ret.SetPixels(pixels);
            ret.Apply();
            return ret;
        }
        public static Texture2D GenerateYFlippedTexture(NativeArray<float> values, float min, float range, int2 resolution, TextureFormat format = TextureFormat.RGBAFloat)
        {
            Texture2D ret = new Texture2D(resolution.x, resolution.y, format, false);
            Color[] pixels = new Color[resolution.x * resolution.y];
            for (int y = 0; y < resolution.y; ++y)
            {
                for (int x = 0; x < resolution.x; ++x)
                {
                    float val = (values[(resolution.y - 1 - y) * resolution.x + x] - min) / range;
                    pixels[y * resolution.x + x] = new Color(val, val, val, 1);
                }
            }
            ret.SetPixels(pixels);
            ret.Apply();
            return ret;
        }
        public static Texture2D GenerateYFlippedTexture(NativeArray<float> values, float min, float range, Color colorMin, Color colorMax, int2 resolution, TextureFormat format = TextureFormat.RGBAFloat)
        {
            Texture2D ret = new Texture2D(resolution.x, resolution.y, format, false);
            Color[] pixels = new Color[resolution.x * resolution.y];
            for (int y = 0; y < resolution.y; ++y)
            {
                for (int x = 0; x < resolution.x; ++x)
                {
                    float val = (values[(resolution.y - 1 - y) * resolution.x + x] - min) / range;
                    pixels[y * resolution.x + x] = Color.Lerp(colorMin, colorMax, val);
                }
            }
            ret.SetPixels(pixels);
            ret.Apply(true, false);
            return ret;
        }
        public static Texture2D GenerateYFlippedTexture(NativeArray<float> values, float min, float range, int2 resolution, StormTextureChannel channel, TextureFormat format = TextureFormat.RGBAFloat)
        {
            Texture2D ret = new Texture2D(resolution.x, resolution.y, format, false);
            Color[] pixels = new Color[resolution.x * resolution.y];
            for (int y = 0; y < resolution.y; ++y)
            {
                for (int x = 0; x < resolution.x; ++x)
                {
                    float val = (values[(resolution.y - 1 - y) * resolution.x + x] - min) / range;
                    pixels[y * resolution.x + x] = GetColor(channel, val);
                }
            }
            ret.SetPixels(pixels);
            ret.Apply();
            return ret;
        }
        public static Texture2D GenerateYFlippedTexture(NativeArray<byte> values, int2 resolution, StormTextureChannel channel, TextureFormat format = TextureFormat.RGBAFloat)
        {
            Texture2D ret = new Texture2D(resolution.x, resolution.y, format, false);
            Color[] pixels = new Color[resolution.x * resolution.y];
            for (int y = 0; y < resolution.y; ++y)
            {
                for (int x = 0; x < resolution.x; ++x)
                {
                    float val = values[(resolution.y - 1 - y) * resolution.x + x] / 255f;
                    pixels[y * resolution.x + x] = GetColor(channel, val);
                }
            }
            ret.SetPixels(pixels);
            ret.Apply();
            return ret;
        }
        public static Texture2D GenerateYFlippedTexture(NativeArray<int> values, int min, int range, int2 resolution, TextureFormat format = TextureFormat.RGBAFloat)
        {
            Texture2D ret = new Texture2D(resolution.x, resolution.y, format, false);
            Color[] pixels = new Color[resolution.x * resolution.y];
            for (int y = 0; y < resolution.y; ++y)
            {
                for (int x = 0; x < resolution.x; ++x)
                {
                    float val = (values[(resolution.y - 1 - y) * resolution.x + x] - min) / (float)range;
                    pixels[y * resolution.x + x] = new Color(val, val, val, 1);
                }
            }
            ret.SetPixels(pixels);
            ret.Apply();
            return ret;
        }
        public static Texture2D GenerateYFlippedTexture(NativeArray<byte> values, int2 resolution, TextureFormat format = TextureFormat.RGBAFloat)
        {
            Texture2D ret = new Texture2D(resolution.x, resolution.y, format, false);
            Color[] pixels = new Color[resolution.x * resolution.y];
            for (int y = 0; y < resolution.y; ++y)
            {
                for (int x = 0; x < resolution.x; ++x)
                {
                    float val = values[(resolution.y - 1 - y) * resolution.x + x] / 255f;
                    pixels[y * resolution.x + x] = new Color(val, val, val, 1);
                }
            }
            ret.SetPixels(pixels);
            ret.Apply();
            return ret;
        }
        public static Texture2D GenerateYFlippedTexture(NativeArray<bool> values, float TrueValue, float FalseValue, int2 resolution, TextureFormat format = TextureFormat.RGBAFloat)
        {
            Texture2D ret = new Texture2D(resolution.x, resolution.y, format, false);
            Color[] pixels = new Color[resolution.x * resolution.y];
            for (int y = 0; y < resolution.y; ++y)
            {
                for (int x = 0; x < resolution.x; ++x)
                {
                    float val = values[(resolution.y - 1 - y) * resolution.x + x] ? TrueValue : FalseValue;
                    pixels[y * resolution.x + x] = new Color(val, val, val, 1);
                }
            }
            ret.SetPixels(pixels);
            ret.Apply();
            return ret;
        }
        public static Texture2D GenerateYFlippedTextureSplit(NativeArray<float> values, int splitCount, int splitLayer, float min, float range, int2 resolution, TextureFormat format = TextureFormat.RGBAFloat)
        {
            Texture2D ret = new Texture2D(resolution.x, resolution.y, format, false);
            Color[] pixels = new Color[resolution.x * resolution.y];
            for (int y = 0; y < resolution.y; ++y)
            {
                for (int x = 0; x < resolution.x; ++x)
                {
                    float val = (values[((resolution.y - 1 - y) * resolution.x * splitCount) + (x * splitCount) + splitLayer] - min) / range;
                    pixels[y * resolution.x + x] = new Color(val, val, val, 1);
                }
            }
            ret.SetPixels(pixels);
            ret.Apply();
            return ret;
        }

        public static Texture2D GenerateFlippedTexture(float[,] values, float min, float range, TextureFormat format = TextureFormat.RGBAFloat)
        {
            int2 resolution = math.int2(values.GetLength(0), values.GetLength(1));
            Texture2D ret = new Texture2D(resolution.x, resolution.y, format, false);
            Color[] pixels = new Color[resolution.x * resolution.y];
            for (int y = 0; y < resolution.y; ++y)
            {
                for (int x = 0; x < resolution.x; ++x)
                {
                    float val = (values[y, x] - min) / range;
                    pixels[y * resolution.x + x] = new Color(val, val, val, 1);
                }
            }
            ret.SetPixels(pixels);
            ret.Apply();
            return ret;
        }
        public static Texture2D GenerateFlippedTexture(float[,] values, float min, float range, StormTextureChannel channel, TextureFormat format = TextureFormat.RGBAFloat)
        {
            int2 resolution = math.int2(values.GetLength(0), values.GetLength(1));
            Texture2D ret = new Texture2D(resolution.x, resolution.y, format, false);
            Color[] pixels = new Color[resolution.x * resolution.y];
            for (int y = 0; y < resolution.y; ++y)
            {
                for (int x = 0; x < resolution.x; ++x)
                {
                    float val = (values[y, x] - min) / range;
                    pixels[y * resolution.x + x] = GetColor(channel, val);
                }
            }
            ret.SetPixels(pixels);
            ret.Apply();
            return ret;
        }
        public static Texture2D GenerateFlippedTexture(int[,] values, int min, int range, TextureFormat format = TextureFormat.RGBAFloat)
        {
            int2 resolution = new int2(values.GetLength(0), values.GetLength(1));
            Texture2D ret = new Texture2D(resolution.y, resolution.x, format, false);
            Color[] pixels = new Color[resolution.x * resolution.y];
            for (int y = 0; y < resolution.x; ++y)
            {
                for (int x = 0; x < resolution.y; ++x)
                {
                    float val = (values[y, x] - min) / (float)range;
                    pixels[y * resolution.x + x] = new Color(val, val, val, 1);
                }
            }
            ret.SetPixels(pixels);
            ret.Apply();
            return ret;
        }
        public static Texture2D GenerateFlippedTexture(int[,] values, int min, int range, StormTextureChannel channel, TextureFormat format = TextureFormat.RGBAFloat)
        {
            int2 resolution = new int2(values.GetLength(0), values.GetLength(1));
            Texture2D ret = new Texture2D(resolution.y, resolution.x, format, false);
            Color[] pixels = new Color[resolution.x * resolution.y];
            for (int y = 0; y < resolution.x; ++y)
            {
                for (int x = 0; x < resolution.y; ++x)
                {
                    float val = (values[y, x] - min) / (float)range;
                    pixels[y * resolution.x + x] = GetColor(channel, val);
                }
            }
            ret.SetPixels(pixels);
            ret.Apply();
            return ret;
        }
        public static Texture2D GenerateFlippedTexture(float[,] red, float[,] green, float[,] blue, float[,] alpha, float min, float range, int StartX, int StartY, int EndX, int EndY, TextureFormat format = TextureFormat.RGBAFloat)
        {
            int2 resolution = int2.zero;
            if (red != null)
            {
                int resX = red.GetLength(0);
                int resY = red.GetLength(1);
                if (resolution.x == 0 || resolution.x > resX)
                    resolution.x = resX;
                if (resolution.y == 0 || resolution.y > resY)
                    resolution.y = resY;
            }
            if (green != null)
            {
                int resX = green.GetLength(0);
                int resY = green.GetLength(1);
                if (resolution.x == 0 || resolution.x > resX)
                    resolution.x = resX;
                if (resolution.y == 0 || resolution.y > resY)
                    resolution.y = resY;
            }
            if (blue != null)
            {
                int resX = blue.GetLength(0);
                int resY = blue.GetLength(1);
                if (resolution.x == 0 || resolution.x > resX)
                    resolution.x = resX;
                if (resolution.y == 0 || resolution.y > resY)
                    resolution.y = resY;
            }
            if (alpha != null)
            {
                int resX = alpha.GetLength(0);
                int resY = alpha.GetLength(1);
                if (resolution.x == 0 || resolution.x > resX)
                    resolution.x = resX;
                if (resolution.y == 0 || resolution.y > resY)
                    resolution.y = resY;
            }
            int rX = Mathf.Min(EndX - StartX, resolution.x);
            int rY = Mathf.Min(EndY - StartY, resolution.y);
            Texture2D ret = new Texture2D(rX, rY, format, false);
            Color[] pixels = new Color[rX * rY];
            for (int y = 0; y < rY; ++y)
            {
                if (y + StartY < 0 || y + StartY >= resolution.y)
                    continue;
                for (int x = 0; x < rX; ++x)
                {
                    if (x + StartX < 0 || x + StartX >= resolution.x)
                        continue;
                    float r = red == null ? 0 : (red[y + StartY, x + StartX] - min) / range;
                    float g = green == null ? 0 : (green[y + StartY, x + StartX] - min) / range;
                    float b = blue == null ? 0 : (blue[y + StartY, x + StartX] - min) / range;
                    float a = alpha == null ? 0 : (alpha[y + StartY, x + StartX] - min) / range;
                    pixels[y * rX + x] = new Color(r, g, b, a);
                }
            }
            ret.SetPixels(pixels);
            ret.Apply();
            return ret;
        }
        public static Texture2D GenerateFlippedTexture(bool[,] values, float TrueValue, float FalseValue, TextureFormat format = TextureFormat.RGBAFloat)
        {
            int2 resolution = new int2(values.GetLength(0), values.GetLength(1));
            Texture2D ret = new Texture2D(resolution.x, resolution.y, format, false);
            Color[] pixels = new Color[resolution.x * resolution.y];
            for (int y = 0; y < resolution.y; ++y)
            {
                for (int x = 0; x < resolution.x; ++x)
                {
                    float val = values[y, x] ? TrueValue : FalseValue;
                    pixels[y * resolution.x + x] = new Color(val, val, val, 1);
                }
            }
            ret.SetPixels(pixels);
            ret.Apply();
            return ret;
        }
        public static Texture2D GenerateFlippedTexture(bool[,] values, float TrueValue, float FalseValue, StormTextureChannel channel, TextureFormat format = TextureFormat.RGBAFloat)
        {
            int2 resolution = new int2(values.GetLength(0), values.GetLength(1));
            Texture2D ret = new Texture2D(resolution.x, resolution.y, format, false);
            Color[] pixels = new Color[resolution.x * resolution.y];
            for (int y = 0; y < resolution.y; ++y)
            {
                for (int x = 0; x < resolution.x; ++x)
                {
                    float val = values[y, x] ? TrueValue : FalseValue;
                    pixels[y * resolution.x + x] = GetColor(channel, val);
                }
            }
            ret.SetPixels(pixels);
            ret.Apply();
            return ret;
        }
        public static Texture2D GenerateFlippedTexture(NativeArray<float> values, float min, float range, Color colorMin, Color colorMax, int2 resolution, TextureFormat format = TextureFormat.RGBAFloat)
        {
            Texture2D ret = new Texture2D(resolution.x, resolution.y, format, false);
            Color[] pixels = new Color[resolution.x * resolution.y];
            for (int y = 0; y < resolution.y; ++y)
            {
                for (int x = 0; x < resolution.x; ++x)
                {
                    float val = (values[y * resolution.x + x] - min) / range;
                    pixels[x * resolution.y + y] = Color.Lerp(colorMin, colorMax, val);
                }
            }
            ret.SetPixels(pixels);
            ret.Apply();
            return ret;
        }
        public static Texture2D GenerateFlippedSplitTexture(float[,,] values, int splitIndex, float min, float range, TextureFormat format = TextureFormat.RGBAFloat)
        {
            int2 resolution = math.int2(values.GetLength(0), values.GetLength(1));
            Texture2D ret = new Texture2D(resolution.x, resolution.y, format, false);
            Color[] pixels = new Color[resolution.x * resolution.y];
            for (int y = 0; y < resolution.y; ++y)
            {
                for (int x = 0; x < resolution.x; ++x)
                {
                    float val = (values[y, x, splitIndex] - min) / range;
                    pixels[y * resolution.x + x] = new Color(val, val, val, 1);
                }
            }
            ret.SetPixels(pixels);
            ret.Apply();
            return ret;
        }
        public static Texture2D GenerateFlippedSplitTexture(float[,,] values, int splitIndex, float min, float range, StormTextureChannel channel, TextureFormat format = TextureFormat.RGBAFloat)
        {
            int2 resolution = math.int2(values.GetLength(0), values.GetLength(1));
            Texture2D ret = new Texture2D(resolution.x, resolution.y, format, false);
            Color[] pixels = new Color[resolution.x * resolution.y];
            for (int y = 0; y < resolution.y; ++y)
            {
                for (int x = 0; x < resolution.x; ++x)
                {
                    float val = (values[y, x, splitIndex] - min) / range;
                    pixels[y * resolution.x + x] = GetColor(channel, val);
                }
            }
            ret.SetPixels(pixels);
            ret.Apply();
            return ret;
        }
        public static Texture2D[] GenerateFlippedSplitTextures(float[,,] values, float min, float range, TextureFormat format = TextureFormat.RGBAFloat)
        {
            int2 resolution = math.int2(values.GetLength(1), values.GetLength(0));
            Texture2D[] ret = new Texture2D[values.GetLength(2)];
            for (int s = 0; s < ret.Length; ++s)
            {
                Texture2D tex = new Texture2D(resolution.x, resolution.y, format, false);
                Color[] pixels = new Color[resolution.x * resolution.y];
                for (int y = 0; y < resolution.y; ++y)
                {
                    for (int x = 0; x < resolution.x; ++x)
                    {
                        float val = (values[y, x, s] - min) / range;
                        pixels[y * resolution.x + x] = new Color(val, val, val, 1);
                    }
                }
                tex.SetPixels(pixels);
                tex.Apply();
                ret[s] = tex;
            }
            return ret;
        }
        public static Texture2D[] GenerateFlippedSplitTextures(float[,,] values, float min, float range, StormTextureChannel channel, TextureFormat format = TextureFormat.RGBAFloat)
        {
            int2 resolution = math.int2(values.GetLength(1), values.GetLength(0));
            Texture2D[] ret = new Texture2D[values.GetLength(2)];
            for (int s = 0; s < ret.Length; ++s)
            {
                Texture2D tex = new Texture2D(resolution.x, resolution.y, format, false);
                Color[] pixels = new Color[resolution.x * resolution.y];
                for (int y = 0; y < resolution.y; ++y)
                {
                    for (int x = 0; x < resolution.x; ++x)
                    {
                        float val = (values[y, x, s] - min) / range;
                        pixels[y * resolution.x + x] = GetColor(channel, val);
                    }
                }
                tex.SetPixels(pixels);
                tex.Apply();
                ret[s] = tex;
            }
            return ret;
        }

        static Color GetColor(StormTextureChannel channel, float val)
        {
            switch (channel)
            {
                default:
                case StormTextureChannel.Red:
                    return new Color(val, 0, 0, 0);
                case StormTextureChannel.Green:
                    return new Color(0, val, 0, 0);
                case StormTextureChannel.Blue:
                    return new Color(0, 0, val, 0);
                case StormTextureChannel.Alpha:
                    return new Color(0, 0, 0, val);
            }
        }
        static Color GetColor(Color original, StormTextureChannel channel, float val)
        {
            switch (channel)
            {
                default:
                case StormTextureChannel.Red:
                    original.r = val;
                    break;
                case StormTextureChannel.Green:
                    original.g = val;
                    break;
                case StormTextureChannel.Blue:
                    original.b = val;
                    break;
                case StormTextureChannel.Alpha:
                    original.a = val;
                    break;
            }
            return original;
        }
        public static float BilinearSample(float[] data, int width, int height, float posX, float posY)
        {
            posX *= width;
            posY *= height;
            int X1 = (int)posX;
            int Y1 = (int)posY;
            posX -= X1;
            posY -= Y1;

            if (X1 == -1 && posX >= 0.5f)
                X1 = 0;
            else if (X1 == width && posX < 0.5f)
                X1 = width - 1;
            if (Y1 == -1 && posY >= 0.5f)
                Y1 = 0;
            else if (Y1 == height && posY < 0.5f)
                Y1 = height - 1;
            int X2 = X1 + 1;
            int Y2 = Y1 + 1;
            if (X2 == -1 && posX >= 0.5f)
                X2 = 0;
            else if (X2 == width && posX < 0.5f)
                X2 = width - 1;
            if (Y2 == -1 && posY >= 0.5f)
                Y2 = 0;
            else if (Y2 == height && posY < 0.5f)
                Y2 = height - 1;
            float data11;
            float data12;
            float data21;
            float data22;
            if (X1 < 0 || X1 >= width || Y1 < 0 || Y1 >= height)
                data11 = 0;
            else
                data11 = data[(Y1 * width) + X1];

            if (X2 < 0 || X2 >= width || Y1 < 0 || Y1 >= height)
                data12 = 0;
            else
                data12 = data[(Y1 * width) + X1];

            if (X1 < 0 || X1 >= width || Y2 < 0 || Y2 >= height)
                data21 = 0;
            else
                data21 = data[(Y1 * width) + X1];

            if (X2 < 0 || X2 >= width || Y2 < 0 || Y2 >= height)
                data22 = 0;
            else
                data22 = data[(Y1 * width) + X1];

            posX -= X1;
            posY -= Y1;
            return (data11 * (1 - posX) + data12 * posX) * (1f - posY) + (data21 * (1 - posX) + data22 * posX) * posY;
        }
        public static float BilinearSample(int[] data, int width, int height, float posX, float posY)
        {
            posX *= width;
            posY *= height;
            int X1 = (int)posX;
            int Y1 = (int)posY;
            posX -= X1;
            posY -= Y1;

            if (X1 == -1 && posX >= 0.5f)
                X1 = 0;
            else if (X1 == width && posX < 0.5f)
                X1 = width - 1;
            if (Y1 == -1 && posY >= 0.5f)
                Y1 = 0;
            else if (Y1 == height && posY < 0.5f)
                Y1 = height - 1;
            int X2 = X1 + 1;
            int Y2 = Y1 + 1;
            if (X2 == -1 && posX >= 0.5f)
                X2 = 0;
            else if (X2 == width && posX < 0.5f)
                X2 = width - 1;
            if (Y2 == -1 && posY >= 0.5f)
                Y2 = 0;
            else if (Y2 == height && posY < 0.5f)
                Y2 = height - 1;
            float data11;
            float data12;
            float data21;
            float data22;

            if (X1 < 0 || X1 >= width || Y1 < 0 || Y1 >= height)
                data11 = 0;
            else
                data11 = data[(Y1 * width) + X1];

            if (X2 < 0 || X2 >= width || Y1 < 0 || Y1 >= height)
                data12 = 0;
            else
                data12 = data[(Y1 * width) + X1];

            if (X1 < 0 || X1 >= width || Y2 < 0 || Y2 >= height)
                data21 = 0;
            else
                data21 = data[(Y1 * width) + X1];

            if (X2 < 0 || X2 >= width || Y2 < 0 || Y2 >= height)
                data22 = 0;
            else
                data22 = data[(Y1 * width) + X1];

            return (data11 * (1 - posX) + data12 * posX) * (1f - posY) + (data21 * (1 - posX) + data22 * posX) * posY;
        }
        public static float BilinearSample(bool[] data, float valueTrue, float valueFalse, int width, int height, float posX, float posY)
        {
            posX *= width;
            posY *= height;
            int X1 = (int)posX;
            int Y1 = (int)posY;
            posX -= X1;
            posY -= Y1;

            if (X1 == -1 && posX >= 0.5f)
                X1 = 0;
            else if (X1 == width && posX < 0.5f)
                X1 = width - 1;
            if (Y1 == -1 && posY >= 0.5f)
                Y1 = 0;
            else if (Y1 == height && posY < 0.5f)
                Y1 = height - 1;
            int X2 = X1 + 1;
            int Y2 = Y1 + 1;
            if (X2 == -1 && posX >= 0.5f)
                X2 = 0;
            else if (X2 == width && posX < 0.5f)
                X2 = width - 1;
            if (Y2 == -1 && posY >= 0.5f)
                Y2 = 0;
            else if (Y2 == height && posY < 0.5f)
                Y2 = height - 1;
            float data11;
            float data12;
            float data21;
            float data22;

            if (X1 < 0 || X1 >= width || Y1 < 0 || Y1 >= height)
                data11 = 0;
            else
                data11 = data[(Y1 * width) + X1] ? valueTrue : valueFalse;

            if (X2 < 0 || X2 >= width || Y1 < 0 || Y1 >= height)
                data12 = 0;
            else
                data12 = data[(Y1 * width) + X1] ? valueTrue : valueFalse;

            if (X1 < 0 || X1 >= width || Y2 < 0 || Y2 >= height)
                data21 = 0;
            else
                data21 = data[(Y1 * width) + X1] ? valueTrue : valueFalse;

            if (X2 < 0 || X2 >= width || Y2 < 0 || Y2 >= height)
                data22 = 0;
            else
                data22 = data[(Y1 * width) + X1] ? valueTrue : valueFalse;

            return (data11 * (1 - posX) + data12 * posX) * (1f - posY) + (data21 * (1 - posX) + data22 * posX) * posY;
        }
        public static float BilinearSample(float[,] data, int width, int height, float posX, float posY)
        {
            posX *= width;
            posY *= height;
            int X1 = (int)posX;
            int Y1 = (int)posY;
            posX -= X1;
            posY -= Y1;

            if (X1 == -1 && posX >= 0.5f)
                X1 = 0;
            else if (X1 == width && posX < 0.5f)
                X1 = width - 1;
            if (Y1 == -1 && posY >= 0.5f)
                Y1 = 0;
            else if (Y1 == height && posY < 0.5f)
                Y1 = height - 1;
            int X2 = X1 + 1;
            int Y2 = Y1 + 1;
            if (X2 == -1 && posX >= 0.5f)
                X2 = 0;
            else if (X2 == width && posX < 0.5f)
                X2 = width - 1;
            if (Y2 == -1 && posY >= 0.5f)
                Y2 = 0;
            else if (Y2 == height && posY < 0.5f)
                Y2 = height - 1;
            float data11;
            float data12;
            float data21;
            float data22;

            if (X1 < 0 || X1 >= width || Y1 < 0 || Y1 >= height)
                data11 = 0;
            else
                data11 = data[Y1, X1];

            if (X2 < 0 || X2 >= width || Y1 < 0 || Y1 >= height)
                data12 = 0;
            else
                data12 = data[Y1, X2];

            if (X1 < 0 || X1 >= width || Y2 < 0 || Y2 >= height)
                data21 = 0;
            else
                data21 = data[Y2, X1];

            if (X2 < 0 || X2 >= width || Y2 < 0 || Y2 >= height)
                data22 = 0;
            else
                data22 = data[Y2, X2];

            return (data11 * (1 - posX) + data12 * posX) * (1f - posY) + (data21 * (1 - posX) + data22 * posX) * posY;
        }
        public static float BilinearSample(int[,] data, int width, int height, float posX, float posY)
        {
            posX *= width;
            posY *= height;
            int X1 = (int)posX;
            int Y1 = (int)posY;
            posX -= X1;
            posY -= Y1;

            if (X1 == -1 && posX >= 0.5f)
                X1 = 0;
            else if (X1 == width && posX < 0.5f)
                X1 = width - 1;
            if (Y1 == -1 && posY >= 0.5f)
                Y1 = 0;
            else if (Y1 == height && posY < 0.5f)
                Y1 = height - 1;
            int X2 = X1 + 1;
            int Y2 = Y1 + 1;
            if (X2 == -1 && posX >= 0.5f)
                X2 = 0;
            else if (X2 == width && posX < 0.5f)
                X2 = width - 1;
            if (Y2 == -1 && posY >= 0.5f)
                Y2 = 0;
            else if (Y2 == height && posY < 0.5f)
                Y2 = height - 1;
            float data11;
            float data12;
            float data21;
            float data22;

            if (X1 < 0 || X1 >= width || Y1 < 0 || Y1 >= height)
                data11 = 0;
            else
                data11 = data[Y1, X1];

            if (X2 < 0 || X2 >= width || Y1 < 0 || Y1 >= height)
                data12 = 0;
            else
                data12 = data[Y1, X2];

            if (X1 < 0 || X1 >= width || Y2 < 0 || Y2 >= height)
                data21 = 0;
            else
                data21 = data[Y2, X1];

            if (X2 < 0 || X2 >= width || Y2 < 0 || Y2 >= height)
                data22 = 0;
            else
                data22 = data[Y2, X2];

            return (data11 * (1 - posX) + data12 * posX) * (1f - posY) + (data21 * (1 - posX) + data22 * posX) * posY;
        }
        public static float BilinearSample(bool[,] data, int width, int height, float valueTrue, float valueFalse, float posX, float posY)
        {
            posX *= width;
            posY *= height;
            int X1 = (int)posX;
            int Y1 = (int)posY;
            posX -= X1;
            posY -= Y1;

            if (X1 == -1 && posX >= 0.5f)
                X1 = 0;
            else if (X1 == width && posX < 0.5f)
                X1 = width - 1;
            if (Y1 == -1 && posY >= 0.5f)
                Y1 = 0;
            else if (Y1 == height && posY < 0.5f)
                Y1 = height - 1;
            int X2 = X1 + 1;
            int Y2 = Y1 + 1;
            if (X2 == -1 && posX >= 0.5f)
                X2 = 0;
            else if (X2 == width && posX < 0.5f)
                X2 = width - 1;
            if (Y2 == -1 && posY >= 0.5f)
                Y2 = 0;
            else if (Y2 == height && posY < 0.5f)
                Y2 = height - 1;
            float data11;
            float data12;
            float data21;
            float data22;

            if (X1 < 0 || X1 >= width || Y1 < 0 || Y1 >= height)
                data11 = 0;
            else
                data11 = data[Y1, X1] ? valueTrue : valueFalse;

            if (X2 < 0 || X2 >= width || Y1 < 0 || Y1 >= height)
                data12 = 0;
            else
                data12 = data[Y1, X2] ? valueTrue : valueFalse;

            if (X1 < 0 || X1 >= width || Y2 < 0 || Y2 >= height)
                data21 = 0;
            else
                data21 = data[Y2, X1] ? valueTrue : valueFalse;

            if (X2 < 0 || X2 >= width || Y2 < 0 || Y2 >= height)
                data22 = 0;
            else
                data22 = data[Y2, X2] ? valueTrue : valueFalse;

            return (data11 * (1 - posX) + data12 * posX) * (1f - posY) + (data21 * (1 - posX) + data22 * posX) * posY;
        }
        public static float TrilinearSample(float[,,] data, int width, int height, int depth, float posX, float posY, float posZ)
        {
            posX *= width;
            posY *= height;
            posZ *= depth;
            int X1 = Mathf.FloorToInt(posX);
            int Y1 = Mathf.FloorToInt(posY);
            int Z1 = Mathf.FloorToInt(posZ);
            posX -= X1;
            posY -= Y1;
            posZ -= Z1;

            if (X1 == -1 && posX >= 0.5f)
                X1 = 0;
            else if (X1 == width && posX < 0.5f)
                X1 = width - 1;
            if (Y1 == -1 && posY >= 0.5f)
                Y1 = 0;
            else if (Y1 == height && posY < 0.5f)
                Y1 = height - 1;
            if (Z1 == -1 && posZ >= 0.5f)
                Z1 = 0;
            else if (Z1 == depth && posZ < 0.5f)
                Z1 = depth - 1;
            int X2 = X1 + 1;
            int Y2 = Y1 + 1;
            int Z2 = Z1 + 1;
            if (X2 == -1 && posX >= 0.5f)
                X2 = 0;
            else if (X2 == width && posX < 0.5f)
                X2 = width - 1;
            if (Y2 == -1 && posY >= 0.5f)
                Y2 = 0;
            else if (Y2 == height && posY < 0.5f)
                Y2 = height - 1;
            if (Z2 == -1 && posZ >= 0.5f)
                Z2 = 0;
            else if (Z2 == depth && posZ < 0.5f)
                Z2 = depth - 1;
            float data111;
            float data121;
            float data211;
            float data221;
            float data112;
            float data122;
            float data212;
            float data222;

            if (X1 < 0 || X1 >= width || Y1 < 0 || Y1 >= height || Z1 < 0 || Z1 >= depth)
                data111 = 0;
            else
                data111 = data[Y1, X1, Z1];

            if (X2 < 0 || X2 >= width || Y1 < 0 || Y1 >= height || Z1 < 0 || Z1 >= depth)
                data121 = 0;
            else
                data121 = data[Y1, X2, Z1];

            if (X1 < 0 || X1 >= width || Y2 < 0 || Y2 >= height || Z1 < 0 || Z1 >= depth)
                data211 = 0;
            else
                data211 = data[Y2, X1, Z1];

            if (X2 < 0 || X2 >= width || Y2 < 0 || Y2 >= height || Z1 < 0 || Z1 >= depth)
                data221 = 0;
            else
                data221 = data[Y2, X2, Z1];

            if (X1 < 0 || X1 >= width || Y1 < 0 || Y1 >= height || Z2 < 0 || Z2 >= depth)
                data112 = 0;
            else
                data112 = data[Y1, X1, Z2];

            if (X2 < 0 || X2 >= width || Y1 < 0 || Y1 >= height || Z2 < 0 || Z2 >= depth)
                data122 = 0;
            else
                data122 = data[Y1, X2, Z2];

            if (X1 < 0 || X1 >= width || Y2 < 0 || Y2 >= height || Z2 < 0 || Z2 >= depth)
                data212 = 0;
            else
                data212 = data[Y2, X1, Z2];

            if (X2 < 0 || X2 >= width || Y2 < 0 || Y2 >= height || Z2 < 0 || Z2 >= depth)
                data222 = 0;
            else
                data222 = data[Y2, X2, Z2];

            return (((data111 * (1 - posX) + data121 * posX) * (1f - posY) + (data211 * (1 - posX) + data221 * posX) * posY) * (1 - posZ)) +
                    (((data112 * (1 - posX) + data122 * posX) * (1f - posY) + (data212 * (1 - posX) + data222 * posX) * posY) * posZ);
        }
        public static float BilinearSample(NativeArray<float> data, int width, int height, float posX, float posY)
        {
            posX *= width;
            posY *= height;
            int X1 = (int)posX;
            int Y1 = (int)posY;
            posX -= X1;
            posY -= Y1;

            if (X1 == -1 && posX >= 0.5f)
                X1 = 0;
            else if (X1 == width && posX < 0.5f)
                X1 = width - 1;
            if (Y1 == -1 && posY >= 0.5f)
                Y1 = 0;
            else if (Y1 == height && posY < 0.5f)
                Y1 = height - 1;
            int X2 = X1 + 1;
            int Y2 = Y1 + 1;
            if (X2 == -1 && posX >= 0.5f)
                X2 = 0;
            else if (X2 == width && posX < 0.5f)
                X2 = width - 1;
            if (Y2 == -1 && posY >= 0.5f)
                Y2 = 0;
            else if (Y2 == height && posY < 0.5f)
                Y2 = height - 1;
            float data11;
            float data12;
            float data21;
            float data22;

            if (X1 < 0 || X1 >= width || Y1 < 0 || Y1 >= height)
                data11 = 0;
            else
                data11 = data[(Y1 * width) + X1];

            if (X2 < 0 || X2 >= width || Y1 < 0 || Y1 >= height)
                data12 = 0;
            else
                data12 = data[(Y1 * width) + X1];

            if (X1 < 0 || X1 >= width || Y2 < 0 || Y2 >= height)
                data21 = 0;
            else
                data21 = data[(Y1 * width) + X1];

            if (X2 < 0 || X2 >= width || Y2 < 0 || Y2 >= height)
                data22 = 0;
            else
                data22 = data[(Y1 * width) + X1];

            return (data11 * (1 - posX) + data12 * posX) * (1f - posY) + (data21 * (1 - posX) + data22 * posX) * posY;
        }
        public static float BilinearSample(NativeArray<int> data, int width, int height, float posX, float posY)
        {
            posX *= width;
            posY *= height;
            int X1 = (int)posX;
            int Y1 = (int)posY;
            posX -= X1;
            posY -= Y1;

            if (X1 == -1 && posX >= 0.5f)
                X1 = 0;
            else if (X1 == width && posX < 0.5f)
                X1 = width - 1;
            if (Y1 == -1 && posY >= 0.5f)
                Y1 = 0;
            else if (Y1 == height && posY < 0.5f)
                Y1 = height - 1;
            int X2 = X1 + 1;
            int Y2 = Y1 + 1;
            if (X2 == -1 && posX >= 0.5f)
                X2 = 0;
            else if (X2 == width && posX < 0.5f)
                X2 = width - 1;
            if (Y2 == -1 && posY >= 0.5f)
                Y2 = 0;
            else if (Y2 == height && posY < 0.5f)
                Y2 = height - 1;
            float data11;
            float data12;
            float data21;
            float data22;

            if (X1 < 0 || X1 >= width || Y1 < 0 || Y1 >= height)
                data11 = 0;
            else
                data11 = data[(Y1 * width) + X1];

            if (X2 < 0 || X2 >= width || Y1 < 0 || Y1 >= height)
                data12 = 0;
            else
                data12 = data[(Y1 * width) + X1];

            if (X1 < 0 || X1 >= width || Y2 < 0 || Y2 >= height)
                data21 = 0;
            else
                data21 = data[(Y1 * width) + X1];

            if (X2 < 0 || X2 >= width || Y2 < 0 || Y2 >= height)
                data22 = 0;
            else
                data22 = data[(Y1 * width) + X1];

            return (data11 * (1 - posX) + data12 * posX) * (1f - posY) + (data21 * (1 - posX) + data22 * posX) * posY;
        }
        public static byte BilinearSample(NativeArray<byte> data, int width, int height, float posX, float posY)
        {
            posX *= width;
            posY *= height;
            int X1 = (int)posX;
            int Y1 = (int)posY;
            posX -= X1;
            posY -= Y1;

            if (X1 == -1 && posX >= 0.5f)
                X1 = 0;
            else if (X1 == width && posX < 0.5f)
                X1 = width - 1;
            if (Y1 == -1 && posY >= 0.5f)
                Y1 = 0;
            else if (Y1 == height && posY < 0.5f)
                Y1 = height - 1;
            int X2 = X1 + 1;
            int Y2 = Y1 + 1;
            if (X2 == -1 && posX >= 0.5f)
                X2 = 0;
            else if (X2 == width && posX < 0.5f)
                X2 = width - 1;
            if (Y2 == -1 && posY >= 0.5f)
                Y2 = 0;
            else if (Y2 == height && posY < 0.5f)
                Y2 = height - 1;
            byte data11;
            byte data12;
            byte data21;
            byte data22;

            if (X1 < 0 || X1 >= width || Y1 < 0 || Y1 >= height)
                data11 = 0;
            else
                data11 = data[(Y1 * width) + X1];

            if (X2 < 0 || X2 >= width || Y1 < 0 || Y1 >= height)
                data12 = 0;
            else
                data12 = data[(Y1 * width) + X1];

            if (X1 < 0 || X1 >= width || Y2 < 0 || Y2 >= height)
                data21 = 0;
            else
                data21 = data[(Y1 * width) + X1];

            if (X2 < 0 || X2 >= width || Y2 < 0 || Y2 >= height)
                data22 = 0;
            else
                data22 = data[(Y1 * width) + X1];

            return (byte)((data11 * (1 - posX) + data12 * posX) * (1f - posY) + (data21 * (1 - posX) + data22 * posX) * posY);
        }
        public static Color BilinearSample(NativeArray<Color> data, int width, int height, float posX, float posY)
        {
            posX *= width;
            posY *= height;
            int X1 = (int)posX;
            int Y1 = (int)posY;
            posX -= X1;
            posY -= Y1;

            if (X1 == -1 && posX >= 0.5f)
                X1 = 0;
            else if (X1 == width && posX < 0.5f)
                X1 = width - 1;
            if (Y1 == -1 && posY >= 0.5f)
                Y1 = 0;
            else if (Y1 == height && posY < 0.5f)
                Y1 = height - 1;
            int X2 = X1 + 1;
            int Y2 = Y1 + 1;
            if (X2 == -1 && posX >= 0.5f)
                X2 = 0;
            else if (X2 == width && posX < 0.5f)
                X2 = width - 1;
            if (Y2 == -1 && posY >= 0.5f)
                Y2 = 0;
            else if (Y2 == height && posY < 0.5f)
                Y2 = height - 1;
            Color data11;
            Color data12;
            Color data21;
            Color data22;

            if (X1 < 0 || X1 >= width || Y1 < 0 || Y1 >= height)
                data11 = Color.clear;
            else
                data11 = data[(Y1 * width) + X1];

            if (X2 < 0 || X2 >= width || Y1 < 0 || Y1 >= height)
                data12 = Color.clear;
            else
                data12 = data[(Y1 * width) + X1];

            if (X1 < 0 || X1 >= width || Y2 < 0 || Y2 >= height)
                data21 = Color.clear;
            else
                data21 = data[(Y1 * width) + X1];

            if (X2 < 0 || X2 >= width || Y2 < 0 || Y2 >= height)
                data22 = Color.clear;
            else
                data22 = data[(Y1 * width) + X1];

            return (data11 * (1 - posX) + data12 * posX) * (1f - posY) + (data21 * (1 - posX) + data22 * posX) * posY;
        }
        public static Color32 BilinearSample(NativeArray<Color32> data, int width, int height, float posX, float posY)
        {
            posX *= width;
            posY *= height;
            int X1 = (int)posX;
            int Y1 = (int)posY;
            posX -= X1;
            posY -= Y1;

            if (X1 == -1 && posX >= 0.5f)
                X1 = 0;
            else if (X1 == width && posX < 0.5f)
                X1 = width - 1;
            if (Y1 == -1 && posY >= 0.5f)
                Y1 = 0;
            else if (Y1 == height && posY < 0.5f)
                Y1 = height - 1;
            int X2 = X1 + 1;
            int Y2 = Y1 + 1;
            if (X2 == -1 && posX >= 0.5f)
                X2 = 0;
            else if (X2 == width && posX < 0.5f)
                X2 = width - 1;
            if (Y2 == -1 && posY >= 0.5f)
                Y2 = 0;
            else if (Y2 == height && posY < 0.5f)
                Y2 = height - 1;
            Color32 data11;
            Color32 data12;
            Color32 data21;
            Color32 data22;

            if (X1 < 0 || X1 >= width || Y1 < 0 || Y1 >= height)
                data11 = Color.clear;
            else
                data11 = data[(Y1 * width) + X1];

            if (X2 < 0 || X2 >= width || Y1 < 0 || Y1 >= height)
                data12 = Color.clear;
            else
                data12 = data[(Y1 * width) + X1];

            if (X1 < 0 || X1 >= width || Y2 < 0 || Y2 >= height)
                data21 = Color.clear;
            else
                data21 = data[(Y1 * width) + X1];

            if (X2 < 0 || X2 >= width || Y2 < 0 || Y2 >= height)
                data22 = Color.clear;
            else
                data22 = data[(Y1 * width) + X1];

            return Color32.Lerp(Color32.Lerp(data11, data12, posX), Color32.Lerp(data21, data22, posX), posY);
        }

        /* //Incomplete function to convert raw byte data of a texture into a Color ... simple for some but DXT and others are harder
        Color GetPixel(float2 uv, NativeArray<byte> TextureData, TextureFormat format)
        {
            Color ret = Color.clear;
            switch (format)
            {
                case TextureFormat.Alpha8:
                    {
                        ret.a = TextureData[index] / 0x00FF;
                        ret.r = ret.a;
                        ret.g = ret.a;
                        ret.b = ret.a;
                    }
                    break;
                case TextureFormat.ARGB4444:
                    {
                        byte data1 = TextureData[index * 2];
                        byte data2 = TextureData[(index * 2) + 1];
                        ret.r = (byte)(data1 & 0x000F) / 0x000F;
                        ret.g = (byte)((data1 & 0x00F0) >> 4) / 0x000F;
                        ret.b = (byte)(data2 & 0x000F) / 0x000F;
                        ret.a = (byte)((data2 & 0x00F0) >> 4) / 0x000F;
                    }
                    break;
                case TextureFormat.RGB24:
                    {
                        ret.r = TextureData[index * 3] / 0x00FF;
                        ret.g = TextureData[(index * 3) + 1] / 0x00FF;
                        ret.b = TextureData[(index * 3) + 2] / 0x00FF;
                        ret.a = 255;
                    }
                    break;
                case TextureFormat.RGBA32:
                    {
                        ret.r = TextureData[index * 4] / 0x00FF;
                        ret.g = TextureData[(index * 4) + 1] / 0x00FF;
                        ret.b = TextureData[(index * 4) + 2] / 0x00FF;
                        ret.a = TextureData[(index * 4) + 3] / 0x00FF;
                    }
                    break;
                case TextureFormat.ARGB32:
                    ret.a = TextureData[index * 4] / 0x00FF;
                    ret.r = TextureData[(index * 4) + 1] / 0x00FF;
                    ret.g = TextureData[(index * 4) + 2] / 0x00FF;
                    ret.b = TextureData[(index * 4) + 3] / 0x00FF;
                    ret.a = 1;
                    break;
                case TextureFormat.RGB565:
                    {
                        byte data1 = TextureData[index * 2];
                        byte data2 = TextureData[(index * 2) + 1];
                        ret.r = (data1 & 0x1F) / 0x1F;
                        ret.g = (((data1 & 0xE0) >> 5) | ((data2 & 0x07) << 3)) / 0x3F;
                        ret.b = ((data2 & 0xF8) >> 3) / 0x1F;
                        ret.a = 1;
                    }
                    break;
                case TextureFormat.R16:
                    {
                        ret.r = (TextureData[index * 2] | (TextureData[(index * 2) + 1] << 8)) / 0xFFFF;
                        ret.g = ret.r;
                        ret.b = ret.r;
                        ret.a = ret.r;
                    }
                    break;
                case TextureFormat.DXT1:
                    break;
                case TextureFormat.DXT5:
                    break;
                case TextureFormat.RGBA4444:
                    break;
                case TextureFormat.BGRA32:
                    break;
                case TextureFormat.RHalf:
                    break;
                case TextureFormat.RGHalf:
                    break;
                case TextureFormat.RGBAHalf:
                    break;
                case TextureFormat.RFloat:
                    break;
                case TextureFormat.RGFloat:
                    break;
                case TextureFormat.RGBAFloat:
                    break;
                case TextureFormat.YUY2:
                    break;
                case TextureFormat.RGB9e5Float:
                    break;
                case TextureFormat.BC4:
                    break;
                case TextureFormat.BC5:
                    break;
                case TextureFormat.BC6H:
                    break;
                case TextureFormat.BC7:
                    break;
                case TextureFormat.DXT1Crunched:
                    break;
                case TextureFormat.DXT5Crunched:
                    break;
                case TextureFormat.PVRTC_RGB2:
                    break;
                case TextureFormat.PVRTC_RGBA2:
                    break;
                case TextureFormat.PVRTC_RGB4:
                    break;
                case TextureFormat.PVRTC_RGBA4:
                    break;
                case TextureFormat.ETC_RGB4:
                    break;
                case TextureFormat.EAC_R:
                    break;
                case TextureFormat.EAC_R_SIGNED:
                    break;
                case TextureFormat.EAC_RG:
                    break;
                case TextureFormat.EAC_RG_SIGNED:
                    break;
                case TextureFormat.ETC2_RGB:
                    break;
                case TextureFormat.ETC2_RGBA1:
                    break;
                case TextureFormat.ETC2_RGBA8:
                    break;
                case TextureFormat.ASTC_4x4:
                    break;
                case TextureFormat.ASTC_5x5:
                    break;
                case TextureFormat.ASTC_6x6:
                    break;
                case TextureFormat.ASTC_8x8:
                    break;
                case TextureFormat.ASTC_10x10:
                    break;
                case TextureFormat.ASTC_12x12:
                    break;
                case TextureFormat.RG16:
                    break;
                case TextureFormat.R8:
                    break;
                case TextureFormat.ETC_RGB4Crunched:
                    break;
                case TextureFormat.ETC2_RGBA8Crunched:
                    break;
                case TextureFormat.ASTC_HDR_4x4:
                    break;
                case TextureFormat.ASTC_HDR_5x5:
                    break;
                case TextureFormat.ASTC_HDR_6x6:
                    break;
                case TextureFormat.ASTC_HDR_8x8:
                    break;
                case TextureFormat.ASTC_HDR_10x10:
                    break;
                case TextureFormat.ASTC_HDR_12x12:
                    break;
                case TextureFormat.RG32:
                    break;
                case TextureFormat.RGB48:
                    break;
                case TextureFormat.RGBA64:
                    break;
            }
            return ret;
        }
        //*/

        public static float NearestNeighborSample(float[,,] data, float posX, float posY, float posZ)
        {
            int depth = data.GetLength(2);
            int width = data.GetLength(1);
            int height = data.GetLength(0);
            int X = Mathf.RoundToInt(posX * (width - 1));
            int Y = Mathf.RoundToInt(posY * (height - 1));
            int Z = Mathf.RoundToInt(posZ * (depth - 1));
            if (X < 0 || X >= width || Y < 0 || Y >= height || Z < 0 || Z >= depth)
                return 0;
            return data[Y, X, Z];
        }
        public static int NearestNeighborSample(int[,,] data, float posX, float posY, float posZ)
        {
            int depth = data.GetLength(2);
            int width = data.GetLength(1);
            int height = data.GetLength(0);
            int X = Mathf.RoundToInt(posX * (width - 1));
            int Y = Mathf.RoundToInt(posY * (height - 1));
            int Z = Mathf.RoundToInt(posZ * (depth - 1));
            if (X < 0 || X >= width || Y < 0 || Y >= height || Z < 0 || Z >= depth)
                return 0;
            return data[Y, X, Z];
        }
        public static float NearestNeighborSample(float[,] data, float posX, float posY)
        {
            int width = data.GetLength(1);
            int height = data.GetLength(0);
            int X = Mathf.RoundToInt(posX * (width - 1));
            int Y = Mathf.RoundToInt(posY * (height - 1));
            if (X < 0 || X >= width || Y < 0 || Y >= height)
                return 0;
            return data[Y, X];
        }
        public static int NearestNeighborSample(int[,] data, float posX, float posY)
        {
            int width = data.GetLength(1);
            int height = data.GetLength(0);
            int X = Mathf.RoundToInt(posX * (width - 1));
            int Y = Mathf.RoundToInt(posY * (height - 1));
            if (X < 0 || X >= width || Y < 0 || Y >= height)
                return 0;
            return data[Y, X];
        }
        public static float NearestNeighborSample(bool[,] data, float valueTrue, float valueFalse, float posX, float posY)
        {
            int width = data.GetLength(1);
            int height = data.GetLength(0);
            int X = Mathf.RoundToInt(posX * (width - 1));
            int Y = Mathf.RoundToInt(posY * (height - 1));
            if (X < 0 || X >= width || Y < 0 || Y >= height)
                return valueFalse;
            return data[Y, X] ? valueTrue : valueFalse;
        }
        public static float NearestNeighborSample(NativeArray<float> data, int width, int height, float posX, float posY)
        {
            int X = Mathf.RoundToInt(posX * (width - 1));
            int Y = Mathf.RoundToInt(posY * (height - 1));
            if (X < 0 || X >= width || Y < 0 || Y >= height)
                return 0;
            return data[(Y * width) + X];
        }
        public static int NearestNeighborSample(NativeArray<int> data, int width, int height, float posX, float posY)
        {
            int X = Mathf.RoundToInt(posX * (width - 1));
            int Y = Mathf.RoundToInt(posY * (height - 1));
            if (X < 0 || X >= width || Y < 0 || Y >= height)
                return 0;
            return data[(Y * width) + X];
        }

        public static bool SetMeshReadableFlag(ref Mesh mesh, bool readable)
        {
#if UNITY_EDITOR
            string assetPath = UnityEditor.AssetDatabase.GetAssetPath(mesh);
            UnityEditor.ModelImporter importer = UnityEditor.AssetImporter.GetAtPath(assetPath) as UnityEditor.ModelImporter;
            if (importer != null)
            {
                importer.isReadable = readable;
                importer.SaveAndReimport();
                //mesh = AssetDatabase.LoadAssetAtPath<Mesh>(assetPath);
            }
            else
                Debug.LogError("Unable to create Inverted mesh for " + mesh.name + "! The Mesh is not set to be readable.", mesh);
            return true;
#else
            return false;
#endif
        }
        public static void Destroy(Object go)
        {
            if (Application.isPlaying)
                Object.Destroy(go);
            else
                Object.DestroyImmediate(go);
        }

        public static string Collapse<T>(T[] items, string separator = ", ")
        {
            if (items == null)
                return "NULL";
            System.Text.StringBuilder ret = new System.Text.StringBuilder();
            int numItems = items.Length;
            ret.Append("(").Append(numItems).Append(")[");
            for (int n = 0; n < numItems; ++n)
            {
                if (n > 0)
                    ret.Append(separator);
                ret.Append(items[n].ToString());
            }
            ret.Append(']');
            return ret.ToString();
        }
        public static string Collapse<T>(ICollection<T> items, string separator = ", ")
        {
            if (items == null)
                return "NULL";
            System.Text.StringBuilder ret = new System.Text.StringBuilder();
            int numItems = items.Count;
            IEnumerator<T> enumerator = items.GetEnumerator();
            ret.Append("(").Append(numItems).Append(")[");
            for (int n = 0; n < numItems; ++n)
            {
                enumerator.MoveNext();
                if (n > 0)
                    ret.Append(separator);
                ret.Append(enumerator.Current.ToString());
            }
            ret.Append(']');
            return ret.ToString();
        }

        public struct LayerNameSourceData
        {
            public string Name;
            public StormLayerType Source;
            public LayerNameSourceData(string n, StormLayerType s)
            {
                Name = n;
                Source = s;
            }
        }

        static bool s_dataLayerNamesDirty = false;
        static List<LayerNameSourceData> s_dataLayerNames = new List<LayerNameSourceData>();
        static LayerNameSourceData[] s_dataLayerNamesSorted = Array.Empty<LayerNameSourceData>();
        public static void ClearDataLayers()
        {
            s_dataLayerNames.Clear();
            s_dataLayerNamesSorted = Array.Empty<LayerNameSourceData>();
            s_dataLayerNamesDirty = false;
            StormAPI.OnLayerIndexesChanged?.Invoke();
        }
        public static int GetDataLayerIndex(string name, StormLayerType source)
        {
            if (string.IsNullOrEmpty(name))
                return -1;
            int idx = s_dataLayerNames.FindIndex(d => d.Name == name);
            if (idx < 0)
            {
                idx = s_dataLayerNames.Count;
                s_dataLayerNames.Add(new LayerNameSourceData(name, source));
                s_dataLayerNamesDirty = true;
            }
            else if (source != StormLayerType.NONE)
            {
                LayerNameSourceData data = s_dataLayerNames[idx];
                data.Source |= source;
                s_dataLayerNames[idx] = data;
            }
            return idx;
        }
        public static int GetDataLayerIndex(StormSourceLayerInfo name, StormLayerType source)
        {
            if (string.IsNullOrEmpty(name.LayerName))
                return -1;
            int idx = s_dataLayerNames.FindIndex(d => d.Name == name.LayerName);
            if (idx < 0)
            {
                idx = s_dataLayerNames.Count;
                s_dataLayerNames.Add(new LayerNameSourceData(name.LayerName, source));
                s_dataLayerNamesDirty = true;
            }
            else if (source != StormLayerType.NONE)
            {
                LayerNameSourceData data = s_dataLayerNames[idx];
                data.Source |= source;
                s_dataLayerNames[idx] = data;
            }
            return idx;
        }
        public static int GetDataLayerIndex(StormFeaturesetSourceLayerInfo name, StormLayerType source)
        {
            if (string.IsNullOrEmpty(name.LayerName))
                return -1;
            int idx = s_dataLayerNames.FindIndex(d => d.Name == name.LayerName);
            if (idx < 0)
            {
                idx = s_dataLayerNames.Count;
                s_dataLayerNames.Add(new LayerNameSourceData(name.LayerName, source));
                s_dataLayerNamesDirty = true;
            }
            else if (source != StormLayerType.NONE)
            {
                LayerNameSourceData data = s_dataLayerNames[idx];
                data.Source |= source;
                s_dataLayerNames[idx] = data;
            }
            return idx;
        }
        public static int[] GetDataLayerIndex(string[] names, StormLayerType source)
        {
            if (names == null)
            {
                return Array.Empty<int>();
            }
            int[] ret = new int[names.Length];
            for (int n = 0; n < names.Length; ++n)
            {
                if (string.IsNullOrEmpty(names[n]))
                    ret[n] = -1;
                else
                {
                    ret[n] = s_dataLayerNames.FindIndex(d => d.Name == names[n]);
                    if (ret[n] < 0)
                    {
                        ret[n] = s_dataLayerNames.Count;
                        s_dataLayerNames.Add(new LayerNameSourceData(names[n], source));
                        s_dataLayerNamesDirty = true;
                    }
                    else if (source != StormLayerType.NONE)
                    {
                        LayerNameSourceData data = s_dataLayerNames[ret[n]];
                        data.Source |= source;
                        s_dataLayerNames[ret[n]] = data;
                    }
                }
            }
            return ret;
        }
        public static int[] GetDataLayerIndex(StormSourceLayerInfo[] layers, StormLayerType source)
        {
            if (layers == null)
            {
                return Array.Empty<int>();
            }
            int[] ret = new int[layers.Length];
            for (int n = 0; n < layers.Length; ++n)
            {
                if (string.IsNullOrEmpty(layers[n].LayerName))
                    ret[n] = -1;
                else
                {
                    ret[n] = s_dataLayerNames.FindIndex(d => d.Name == layers[n].LayerName);
                    if (ret[n] < 0)
                    {
                        ret[n] = s_dataLayerNames.Count;
                        s_dataLayerNames.Add(new LayerNameSourceData(layers[n].LayerName, source));
                        s_dataLayerNamesDirty = true;
                    }
                    else if (source != StormLayerType.NONE)
                    {
                        LayerNameSourceData data = s_dataLayerNames[ret[n]];
                        data.Source |= source;
                        s_dataLayerNames[ret[n]] = data;
                    }
                }
            }
            return ret;
        }
        public static int[] GetDataLayerIndex(StormFeaturesetSourceLayerInfo[] layers, StormLayerType source)
        {
            if (layers == null)
            {
                return Array.Empty<int>();
            }
            int[] ret = new int[layers.Length];
            for (int n = 0; n < layers.Length; ++n)
            {
                if (string.IsNullOrEmpty(layers[n].LayerName))
                    ret[n] = -1;
                else
                {
                    ret[n] = s_dataLayerNames.FindIndex(d => d.Name == layers[n].LayerName);
                    if (ret[n] < 0)
                    {
                        ret[n] = s_dataLayerNames.Count;
                        s_dataLayerNames.Add(new LayerNameSourceData(layers[n].LayerName, source));
                        s_dataLayerNamesDirty = true;
                    }
                    else if (source != StormLayerType.NONE)
                    {
                        LayerNameSourceData data = s_dataLayerNames[ret[n]];
                        data.Source |= source;
                        s_dataLayerNames[ret[n]] = data;
                    }
                }
            }
            return ret;
        }
        public static StormSourceIndexInfo GetDataLayerInfo(StormSourceLayerInfo layer, StormLayerType source)
        {
            if (string.IsNullOrEmpty(layer.LayerName))
                return new StormSourceIndexInfo(-1, layer.Strength);
            int idx = s_dataLayerNames.FindIndex(d => d.Name == layer.LayerName);
            if (idx < 0)
            {
                idx = s_dataLayerNames.Count;
                s_dataLayerNames.Add(new LayerNameSourceData(layer.LayerName, source));
                s_dataLayerNamesDirty = true;
            }
            else if (source != StormLayerType.NONE)
            {
                LayerNameSourceData data = s_dataLayerNames[idx];
                data.Source |= source;
                s_dataLayerNames[idx] = data;
            }
            return new StormSourceIndexInfo(idx, layer.Strength);
        }
        public static StormSourceIndexInfo GetDataLayerInfo(string layer, StormLayerType source)
        {
            if (string.IsNullOrEmpty(layer))
                return new StormSourceIndexInfo(-1, 1);
            int idx = s_dataLayerNames.FindIndex(d => d.Name == layer);
            if (idx < 0)
            {
                idx = s_dataLayerNames.Count;
                s_dataLayerNames.Add(new LayerNameSourceData(layer, source));
                s_dataLayerNamesDirty = true;
            }
            else if (source != StormLayerType.NONE)
            {
                LayerNameSourceData data = s_dataLayerNames[idx];
                data.Source |= source;
                s_dataLayerNames[idx] = data;
            }
            return new StormSourceIndexInfo(idx, 1);
        }
        public static StormSourceIndexInfo[] GetDataLayerInfo(StormSourceLayerInfo[] layers, StormLayerType source)
        {
            if (layers == null)
            {
                return Array.Empty<StormSourceIndexInfo>();
            }
            StormSourceIndexInfo[] ret = new StormSourceIndexInfo[layers.Length];
            for (int n = 0; n < layers.Length; ++n)
            {
                if (string.IsNullOrEmpty(layers[n].LayerName))
                    ret[n] = new StormSourceIndexInfo(-1, layers[n].Strength);
                else
                {
                    int retIdx = s_dataLayerNames.FindIndex(d => d.Name == layers[n].LayerName);
                    if (retIdx < 0)
                    {
                        retIdx = s_dataLayerNames.Count;
                        s_dataLayerNames.Add(new LayerNameSourceData(layers[n].LayerName, source));
                        s_dataLayerNamesDirty = true;
                    }
                    else if (source != StormLayerType.NONE)
                    {
                        LayerNameSourceData data = s_dataLayerNames[retIdx];
                        data.Source |= source;
                        s_dataLayerNames[retIdx] = data;
                    }
                    ret[n] = new StormSourceIndexInfo(retIdx, layers[n].Strength);
                }
            }
            return ret;
        }
        public static StormSourceIndexInfo[] GetDataLayerInfo(StormFeaturesetSourceLayerInfo[] layers, StormLayerType source)
        {
            if (layers == null)
            {
                return Array.Empty<StormSourceIndexInfo>();
            }
            StormSourceIndexInfo[] ret = new StormSourceIndexInfo[layers.Length];
            for (int n = 0; n < layers.Length; ++n)
            {
                if (string.IsNullOrEmpty(layers[n].LayerName))
                    ret[n] = new StormSourceIndexInfo(-1, layers[n].Strength);
                else
                {
                    int retIdx = s_dataLayerNames.FindIndex(d => d.Name == layers[n].LayerName);
                    if (retIdx < 0)
                    {
                        retIdx = s_dataLayerNames.Count;
                        s_dataLayerNames.Add(new LayerNameSourceData(layers[n].LayerName, source));
                        s_dataLayerNamesDirty = true;
                    }
                    else if (source != StormLayerType.NONE)
                    {
                        LayerNameSourceData data = s_dataLayerNames[retIdx];
                        data.Source |= source;
                        s_dataLayerNames[retIdx] = data;
                    }
                    ret[n] = new StormSourceIndexInfo(retIdx, layers[n].Strength);
                }
            }
            return ret;
        }
        public static StormSourceIndexInfo[] GetDataLayerInfo(List<StormSourceLayerInfo> layers, StormLayerType source)
        {
            if (layers == null)
            {
                return Array.Empty<StormSourceIndexInfo>();
            }
            StormSourceIndexInfo[] ret = new StormSourceIndexInfo[layers.Count];
            for (int n = 0; n < layers.Count; ++n)
            {
                if (string.IsNullOrEmpty(layers[n].LayerName))
                    ret[n] = new StormSourceIndexInfo(-1, layers[n].Strength);
                else
                {
                    int retIdx = s_dataLayerNames.FindIndex(d => d.Name == layers[n].LayerName);
                    if (retIdx < 0)
                    {
                        retIdx = s_dataLayerNames.Count;
                        s_dataLayerNames.Add(new LayerNameSourceData(layers[n].LayerName, source));
                        s_dataLayerNamesDirty = true;
                    }
                    else if (source != StormLayerType.NONE)
                    {
                        LayerNameSourceData data = s_dataLayerNames[retIdx];
                        data.Source |= source;
                        s_dataLayerNames[retIdx] = data;
                    }
                    ret[n] = new StormSourceIndexInfo(retIdx, layers[n].Strength);
                }
            }
            return ret;
        }
        public static StormSourceIndexInfo[] GetDataLayerInfo(List<StormFeaturesetSourceLayerInfo> layers, StormLayerType source)
        {
            if (layers == null)
            {
                return Array.Empty<StormSourceIndexInfo>();
            }
            StormSourceIndexInfo[] ret = new StormSourceIndexInfo[layers.Count];
            for (int n = 0; n < layers.Count; ++n)
            {
                if (string.IsNullOrEmpty(layers[n].LayerName))
                    ret[n] = new StormSourceIndexInfo(-1, layers[n].Strength);
                else
                {
                    int retIdx = s_dataLayerNames.FindIndex(d => d.Name == layers[n].LayerName);
                    if (retIdx < 0)
                    {
                        retIdx = s_dataLayerNames.Count;
                        s_dataLayerNames.Add(new LayerNameSourceData(layers[n].LayerName, source));
                        s_dataLayerNamesDirty = true;
                    }
                    else if (source != StormLayerType.NONE)
                    {
                        LayerNameSourceData data = s_dataLayerNames[retIdx];
                        data.Source |= source;
                        s_dataLayerNames[retIdx] = data;
                    }
                    ret[n] = new StormSourceIndexInfo(retIdx, layers[n].Strength);
                }
            }
            return ret;
        }
        public static string GetDataLayerName(int index)
        {
            if (index < 0 || index >= s_dataLayerNames.Count)
                return string.Empty;
            return s_dataLayerNames[index].Name;
        }
        public static StormLayerType GetDataLayerSource(int index)
        {
            if (index < 0 || index >= s_dataLayerNames.Count)
                return StormLayerType.NONE;
            return s_dataLayerNames[index].Source;
        }
#if UNITY_2021_2_OR_NEWER
        public static ReadOnlySpan<LayerNameSourceData> GetDataLayerNames()
        {
            if (s_dataLayerNamesDirty)
            {
                int len = s_dataLayerNames.Count;
                List<LayerNameSourceData> sorted = new List<LayerNameSourceData>(s_dataLayerNames);
                sorted.Sort((a, b) => String.Compare(b.Name, a.Name, StringComparison.Ordinal));
                Array.Resize(ref s_dataLayerNamesSorted, len);
                sorted.CopyTo(s_dataLayerNamesSorted);
            }
            return s_dataLayerNamesSorted.AsSpan();
        }
#else
        public static LayerNameSourceData[] GetDataLayerNames()
        {
            if (s_dataLayerNamesDirty)
            {
                int len = s_dataLayerNames.Count;
                List<LayerNameSourceData> sorted = new List<LayerNameSourceData>(s_dataLayerNames);
                sorted.Sort((a,b)=>b.Name.CompareTo(a.Name));
                Array.Resize(ref s_dataLayerNamesSorted, len);
                sorted.CopyTo(s_dataLayerNamesSorted);
            }
            return s_dataLayerNamesSorted;
        }
#endif
        public static Mesh CreateInverse(Mesh normalMesh)
        {
            if (normalMesh == null)
                return null;
            bool startedReadable = normalMesh.isReadable;
            if (!startedReadable)
            {
                startedReadable = false;
                if (!StormUtils.SetMeshReadableFlag(ref normalMesh, true))
                {
                    Debug.LogError("Unable to inverse non-readable mesh at runtime: " + normalMesh.name);
                    return normalMesh;
                }
            }
            Mesh inverseMesh = new Mesh();
            inverseMesh.name = normalMesh.name + "_Inverse";
            inverseMesh.vertices = normalMesh.vertices;
            inverseMesh.uv = normalMesh.uv;
            inverseMesh.uv2 = normalMesh.uv2;
            inverseMesh.uv3 = normalMesh.uv3;
            inverseMesh.uv4 = normalMesh.uv4;
            inverseMesh.uv5 = normalMesh.uv5;
            inverseMesh.uv6 = normalMesh.uv6;
            inverseMesh.uv7 = normalMesh.uv7;
            inverseMesh.uv8 = normalMesh.uv8;
            inverseMesh.colors = normalMesh.colors;
            inverseMesh.normals = normalMesh.normals;
            inverseMesh.bounds = normalMesh.bounds;
            inverseMesh.tangents = normalMesh.tangents;
            inverseMesh.subMeshCount = normalMesh.subMeshCount;
            for (int sm = 0; sm < normalMesh.subMeshCount; ++sm)
            {
                int[] indici = normalMesh.GetIndices(sm);
                int cnt = indici.Length / 3;
                for (int t = 0; t < cnt; ++t)
                {
                    //swap the 1st and 3rd indici
                    indici[t * 3] ^= indici[t * 3 + 2];
                    indici[t * 3 + 2] ^= indici[t * 3];
                    indici[t * 3] ^= indici[t * 3 + 2];
                }
                inverseMesh.SetIndices(indici, normalMesh.GetTopology(sm), sm);
            }
            //reset the readable flag if we altered it. (since we save the meshes now, we don't need to keep the mesh readable for this at runtime)
            if (!startedReadable)
                StormUtils.SetMeshReadableFlag(ref normalMesh, false);
            return inverseMesh;
        }
        public static IndirectDrawIndexedArgs[] GetArgs(Mesh mesh, int count = -1, StormExtraMaterialMode mode = StormExtraMaterialMode.LAST)
        {
            int actualSubmeshCount = mesh.subMeshCount;
            if (count < 0)
                count = actualSubmeshCount;
            if (mode == StormExtraMaterialMode.NONE && count > actualSubmeshCount)
                count = actualSubmeshCount;
            IndirectDrawIndexedArgs[] args = new IndirectDrawIndexedArgs[count > 0 ? count : actualSubmeshCount];
            for (int s = 0; s < args.Length; ++s)
            {
                int meshIdx = mode switch {
                    StormExtraMaterialMode.LOOP => s % actualSubmeshCount,
                    _ => math.min(s, actualSubmeshCount - 1)
                };
                args[s].baseVertexIndex = mesh.GetBaseVertex(meshIdx);
                args[s].indexCountPerInstance = mesh.GetIndexCount(meshIdx);
                args[s].startIndex = mesh.GetIndexStart(meshIdx);
                args[s].startInstance = 0;
                args[s].instanceCount = 0;
            }
            return args;
        }
        //Helper to deal with the FindObjectOfType API changes
        public static T FindObjectOfType<T>(bool findInactive = false) where T : UnityEngine.Object
        {
#if UNITY_6000_0_OR_NEWER
            return GameObject.FindFirstObjectByType(typeof(T), findInactive ? FindObjectsInactive.Include : FindObjectsInactive.Exclude) as T;
#else
            return GameObject.FindObjectOfType(typeof(T), findInactive) as T;
#endif
        }
        //Helper to deal with the FindObjectsOfType API changes
        public static T[] FindObjectsOfType<T>(bool findInactive = false) where T : UnityEngine.Object
        {
#if UNITY_6000_0_OR_NEWER
            return GameObject.FindObjectsByType(typeof(T), findInactive ? FindObjectsInactive.Include : FindObjectsInactive.Exclude, FindObjectsSortMode.None) as T[];
#else
            return GameObject.FindObjectsOfType(typeof(T), findInactive) as T[];
#endif
        }

        /// <summary>
        /// Checks if the color is black - used in some UIs to figure out if a GUI Style has lost its color and turned black
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public static bool IsBlack(Color c)
        {
            return c.r == 0 && c.g == 0 && c.b == 0 && c.a == 0;
        }

        /// <summary>
        /// Used to create a clickable text GUI on the Editor UI
        /// </summary>
        /// <param name="content"></param>
        /// <param name="style"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static bool ClickableHeaderCustomStyle(GUIContent content, GUIStyle style, GUILayoutOption[] options = null)
        {
#if UNITY_EDITOR
            var position = GUILayoutUtility.GetRect(content, style, options);
            Handles.BeginGUI();
            Color oldColor = Handles.color;
            Handles.color = style.normal.textColor;
            Handles.DrawLine(new Vector3(position.xMin, position.yMax), new Vector3(position.xMax, position.yMax));
            Handles.color = oldColor;
            Handles.EndGUI();
            EditorGUIUtility.AddCursorRect(position, MouseCursor.Link);
            return GUI.Button(position, content, style);
#else
            return false;
#endif

        }

        #region Outline Util
        static Type _SpriteUtilityType = null;
        static Type SpriteUtilityType
        {
            get {
                return (_SpriteUtilityType == null) ? _SpriteUtilityType = Type.GetType("UnityEditor.Sprites.SpriteUtility, UnityEditor") : _SpriteUtilityType;
            }
        }

        public static Vector2[] GetOutline(Texture2D mainTex, int steps, float tolerance, int maxVertici, float normalScale, out Vector2 minPixel, out Vector2 maxPixel)
        {
            Vector2[] ret;
            minPixel = Vector2.one;
            maxPixel = Vector2.zero;
#if UNITY_EDITOR
            if (mainTex.isReadable)
            {
                int tmpWidth = Mathf.CeilToInt(mainTex.width / (float)steps);
                int tmpHeight = Mathf.CeilToInt(mainTex.height / (float)steps);
                Color[] tmpPixels = new Color[tmpWidth * tmpHeight];
                for (int X = 0; X < steps; ++X)
                {
                    for (int Y = 0; Y < steps; ++Y)
                    {
                        Color[] mtPixels = mainTex.GetPixels(X * tmpWidth, Y * tmpHeight, tmpWidth, tmpHeight);
                        for (int p = 0; p < mtPixels.Length; ++p)
                            tmpPixels[p] = (tmpPixels[p] * tmpPixels[p].a) + (mtPixels[p] * (1f - tmpPixels[p].a));
                    }
                }
                Texture2D tmpTexture = new Texture2D(tmpWidth, tmpHeight, mainTex.format, false);
                tmpTexture.SetPixels(tmpPixels);
                tmpTexture.Apply();
                //File.WriteAllBytes("assets/" + mainTex.name + ".tga", tmpTexture.EncodeToTGA());

                object[] Params = new object[] { tmpTexture, new Rect(0, 0, tmpWidth, tmpHeight), tolerance, (byte)254, false, new Vector2[0][] };
                SpriteUtilityType.GetMethod("GenerateOutline", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic).Invoke(null, Params);
                Vector2[][] paths = (Vector2[][])Params[5];
                int sum = 0;
                for (int i = 0; i < paths.Length; i++)
                    sum += paths[i].Length;
                Vector2 scale = new Vector2(1f / tmpWidth, 1f / tmpHeight);
                Vector2 offset = new Vector2(tmpWidth * 0.5f, tmpHeight * 0.5f);
                if (sum < 3)
                {
                    paths = new Vector2[][] {
                        new Vector2[] {
                            new Vector2(-offset.x, -offset.y),
                            new Vector2(-offset.x, offset.y),
                            new Vector2(offset.x, offset.y),
                            new Vector2(offset.x, -offset.y),
                        },
                    };
                    sum = 4;
                }
                ret = new Vector2[sum];
                int index = 0;
                for (int i = 0; i < paths.Length; i++)
                    for (int j = 0; j < paths[i].Length; j++)
                        ret[index++] = Vector2.Scale(paths[i][j] + offset, scale);

                for (int i = 0; i < ret.Length; i++)
                {
                    minPixel.x = Mathf.Min(ret[i].x, minPixel.x);
                    minPixel.y = Mathf.Min(ret[i].y, minPixel.y);
                    maxPixel.x = Mathf.Max(ret[i].x, maxPixel.x);
                    maxPixel.y = Mathf.Max(ret[i].y, maxPixel.y);
                }

                Hull(ref ret);
                ret = ReduceVertices(ret, maxVertici);
                ret = ScaleVectors(ret, normalScale);
                for (int i = 0; i < ret.Length; i++)
                {
                    ret[i].x = Mathf.Clamp01(ret[i].x);
                    ret[i].y = Mathf.Clamp01(ret[i].y);
                }
                Hull(ref ret);
                for (int i = 0; i < ret.Length; i++)
                    ret[i] = new Vector2(ret[i].x, 1 - ret[i].y);
            }
            else
            {
#endif
                ret = new Vector2[] {
                    new Vector2(0.15f, 0f),
                    new Vector2(0.85f, 0f),
                    new Vector2(1f, 0.15f),
                    new Vector2(1f, 0.85f),
                    new Vector2(0.85f, 1f),
                    new Vector2(0.15f, 1f),
                    new Vector2(0f, 0.85f),
                    new Vector2(0f, 0.15f),
                };
#if UNITY_EDITOR
            }
#endif
            return ret;
        }
        static int PointSort(Vector2 a, Vector2 b)
        {
            if (a.x == b.x)
                return a.y.CompareTo(b.y);
            else
                return a.x.CompareTo(b.x);
        }
        static void Hull(ref Vector2[] ret)
        {
            if (ret.Length > 1)
            {
                int n = ret.Length, k = 0;
                Vector2[] shell = new Vector2[2 * n];
                Array.Sort(ret, PointSort);
                for (int i = 0; i < n; ++i)
                {
                    while (k >= 2 && Vector2Cross(ret[i], shell[k - 2], shell[k - 1]) <= 0)
                        k--;
                    shell[k++] = ret[i];
                }
                for (int i = n - 2, t = k + 1; i >= 0; i--)
                {
                    while (k >= t && Vector2Cross(ret[i], shell[k - 2], shell[k - 1]) <= 0)
                        k--;
                    shell[k++] = ret[i];
                }
                if (k > 1)
                    Array.Resize(ref shell, k - 1);
                ret = shell;
            }
        }
        static float Vector2Cross(Vector2 O, Vector2 A, Vector2 B)
        {
            return (A.x - O.x) * (B.y - O.y) - (A.y - O.y) * (B.x - O.x);
        }

        static Vector2[] ReduceVertices(Vector2[] P, int maxVertices)
        {
            if (maxVertices == 4)
            {
                // turn into a box
                Rect newBox = new Rect(P[0].x, P[0].y, 0f, 0f);
                for (int i = 0; i < P.Length; i++)
                {
                    newBox.xMin = Mathf.Min(newBox.xMin, P[i].x);
                    newBox.xMax = Mathf.Max(newBox.xMax, P[i].x);
                    newBox.yMin = Mathf.Min(newBox.yMin, P[i].y);
                    newBox.yMax = Mathf.Max(newBox.yMax, P[i].y);
                }

                P = new Vector2[]
                {
                    new Vector2(newBox.xMin, newBox.yMin),
                    new Vector2(newBox.xMax, newBox.yMin),
                    new Vector2(newBox.xMax, newBox.yMax),
                    new Vector2(newBox.xMin, newBox.yMax),
                };
            }
            else
            {
                // remove vertices to target count (naive implementation)
                int reduction = Math.Max(0, P.Length - maxVertices);
                for (int k = 0; k < reduction; k++)
                {
                    float currentArea = 0;
                    int smallestIndex = 0;
                    int replacementIndex = 0;
                    Vector2 newPos = Vector2.zero;
                    for (int i = 0; i < P.Length; i++)
                    {
                        int next = i + 1;
                        int upNext = i + 2;
                        int finalNext = i + 3;
                        if (next >= P.Length)
                            next -= P.Length;
                        if (upNext >= P.Length)
                            upNext -= P.Length;
                        if (finalNext >= P.Length)
                            finalNext -= P.Length;

                        Vector2 intersect;
                        float tmp = (P[finalNext].x - P[upNext].x) * (P[next].y - P[i].y) - (P[finalNext].y - P[upNext].y) * (P[next].x - P[i].x);

                        if (tmp == 0)
                        {
                            intersect = ((Vector2.Lerp(P[next], P[upNext], 0.5f) - (Vector2.one * 0.5f)) * 1000) + (Vector2.one * 500f);//Vector2.positiveInfinity;// Vector2.zero;
                        }
                        else
                        {
                            float mu = ((P[i].x - P[upNext].x) * (P[next].y - P[i].y) - (P[i].y - P[upNext].y) * (P[next].x - P[i].x)) / tmp;
                            intersect = new Vector2(
                                P[upNext].x + (P[finalNext].x - P[upNext].x) * mu,
                                P[upNext].y + (P[finalNext].y - P[upNext].y) * mu
                            );
                        }

                        if (i == 0)
                        {
                            currentArea = Triangle(intersect, P[next], P[upNext]);

                            if (Outside(intersect) > 0)
                                currentArea = currentArea + Outside(intersect) * 1;

                            smallestIndex = next;
                            replacementIndex = upNext;
                            newPos = intersect;
                        }
                        else
                        {
                            float newArea = Triangle(intersect, P[next], P[upNext]);

                            if (Outside(intersect) > 0)
                                newArea = newArea + Outside(intersect) * 1;

                            if (newArea < currentArea && Outside(intersect) <= 0)
                            {
                                currentArea = newArea;
                                smallestIndex = next;
                                replacementIndex = upNext;
                                newPos = intersect;
                            }
                        }
                    }

                    P[replacementIndex] = newPos;
                    P = Array.FindAll<Vector2>(P, x => Array.IndexOf(P, x) != smallestIndex);
                }
            }

            return P;
        }
        static float Triangle(Vector2 O, Vector2 A, Vector2 B)
        {
            return Mathf.Abs((A.x - B.x) * (O.y - A.y) - (A.x - O.x) * (B.y - A.y)) * 0.5f;
        }
        static float Outside(Vector2 P)
        {
            P -= (Vector2.one * 0.5f);
            float vert = Mathf.Clamp01(Mathf.Abs(P.y) - 0.5f);
            float hori = Mathf.Clamp01(Mathf.Abs(P.x) - 0.5f);
            return hori + vert;
        }
        static Vector2[] ScaleVectors(Vector2[] P, float scaleAmount)
        {
            Vector2[] normals = new Vector2[P.Length];
            for (int i = 0; i < normals.Length; i++)
            {
                int prev = i - 1;
                int next = i + 1;
                if (i == 0)
                    prev = P.Length - 1;
                if (i == P.Length - 1)
                    next = 0;

                Vector2 ba = P[i] - P[prev];
                Vector2 bc = P[i] - P[next];
                Vector2 normal = (ba.normalized + bc.normalized).normalized;
                normals[i] = normal;
            }

            for (int i = 0; i < normals.Length; i++)
            {
                P[i] = P[i] + normals[i] * scaleAmount;
            }

            return P;
        }

        /// <summary>
        /// Get if a rect overlaps another rect
        /// </summary>
        /// <param name="pos1">Min Pos of Rect A</param>
        /// <param name="size1">Size of Rect A</param>
        /// <param name="pos2">Min Pos of Rect B</param>
        /// <param name="size2">Size of Rect B</param>
        /// <returns>True if the rects overlap</returns>
        public static bool Overlaps(float2 pos1, float2 size1, float2 pos2, float2 size2)
        {
            return pos1.x + size1.x > pos2.x && pos1.x < pos2.x + size2.x && pos1.y + size1.y > pos2.y && pos1.y < pos2.y + size2.y;
        }

        public static void AddDiffusionProfile(ref Object profile)
        {
#if PW_STORM_HDRP
            DiffusionProfileSettings currentProfile = profile as DiffusionProfileSettings;
            RenderPipelineGlobalSettings currentSettings = GraphicsSettings.GetSettingsForRenderPipeline<HDRenderPipeline>();
            Type pipelineType = currentSettings.GetType();
            FieldInfo profileField = pipelineType.GetField("m_DefaultVolumeProfile", BindingFlags.Instance | BindingFlags.NonPublic);
            if (profileField != null)
            {
                VolumeProfile curVP = profileField.GetValue(currentSettings) as VolumeProfile;
                if (curVP != null)
                {
                    DiffusionProfileList profileList = curVP.components.Find(x => x.GetType() == typeof(DiffusionProfileList)) as DiffusionProfileList;
                    if (profileList == null)
                    {
                        profileList = curVP.Add<DiffusionProfileList>();
                        profileList.diffusionProfiles.value = new DiffusionProfileSettings[0];
                    }
                    bool found = false;
                    for (int i = 0; i < profileList.diffusionProfiles.value.Length; i++)
                    {
                        DiffusionProfileSettings currentDPSettings = profileList.diffusionProfiles.value[i];
                        if (currentDPSettings == currentProfile)
                        {
                            found = true;
                            break;
                        }
                    }
                    if (!found)
                    {
                        DiffusionProfileSettings[] theList = profileList.diffusionProfiles.value;
                        int len = theList.Length;
                        if (len >= 15)
                        {
                            //We are at the profile limit already.
                            //We need to use one that already is set instead but I have no way to know which one should be used.
                            //Select the first I guess
                            profile = theList[0];
                            return;
                        }
                        Array.Resize(ref theList, len + 1);
                        theList[len] = currentProfile;
                        profileList.diffusionProfiles.value = theList;
                    }
                }
            }
#endif
        }
        public static float GetDiffusionProfileHash(Object profile)
        {
#if PW_STORM_HDRP
            DiffusionProfileSettings currentProfile = profile as DiffusionProfileSettings;
            Type profileType = currentProfile.GetType();
            FieldInfo profileField = profileType.GetField("profile", BindingFlags.Instance | BindingFlags.NonPublic);
            if (profileField != null)
            {
                object p = profileField.GetValue(currentProfile);
                if (p != null)
                {
                    Type pType = p.GetType();
                    FieldInfo hashField = pType.GetField("hash", BindingFlags.Instance | BindingFlags.Public);
                    if (hashField != null)
                    {
                        uint hash = (uint)hashField.GetValue(p);
                        return math.asfloat(hash);
                    }
                }
            }
#endif
            return 0;
        }
        public static Vector4 GetDiffusionProfileAssetID(Object profile)
        {
#if UNITY_EDITOR
            return ConvertGUIDToVector4(UnityEditor.AssetDatabase.AssetPathToGUID(UnityEditor.AssetDatabase.GetAssetPath(profile)));
#else
            return default;
#endif
        }


        // ** Copied from HDRP's HDUtils class since it was marked internal **
        public static Vector4 ConvertGUIDToVector4(string guid)
        {
            if (guid == null || guid.Length < 32)
                return default;
            byte[] bytes = new byte[16];

            for (int i = 0; i < 16; i++)
                bytes[((i / 4) * 4) + (3 - (i % 4))] = byte.Parse(guid.Substring(i * 2, 2), System.Globalization.NumberStyles.HexNumber);
            Vector4 vector = new Vector4(
                math.asfloat(BitConverter.ToUInt32(bytes, 0)),
                math.asfloat(BitConverter.ToUInt32(bytes, 4)),
                math.asfloat(BitConverter.ToUInt32(bytes, 8)),
                math.asfloat(BitConverter.ToUInt32(bytes, 12))
            );
            return vector;
        }
        #endregion
        #region Input System
        public static KeyCode InputPathToKey(string path)
        {
            string[] parts = path.ToLowerInvariant().Split('/', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length >= 2)
            {
                if (parts[0].Contains("keyboard"))
                {
                    switch (parts[1])
                    {
                        case "backquote":
                        case "#(`)":
                            return KeyCode.BackQuote;
                        case "space":
                            return KeyCode.Space;
                        case "enter":
                            return KeyCode.Return;
                        case "tab":
                            return KeyCode.Tab;
                        case "shift":
                        case "leftshift":
                            return KeyCode.LeftShift;
                        case "rightshift":
                            return KeyCode.RightShift;
                        case "alt":
                        case "leftalt":
                            return KeyCode.LeftAlt;
                        case "rightalt":
                            return KeyCode.RightAlt;
                        case "ctrl":
                        case "leftctrl":
                            return KeyCode.LeftControl;
                        case "rightctrl":
                            return KeyCode.RightControl;
                        case "leftmeta":
                            return KeyCode.LeftMeta;
                        case "rightmeta":
                            return KeyCode.RightMeta;
                        case "contextmenu":
                            return KeyCode.Menu;
                        case "backspace":
                            return KeyCode.Backspace;
                        case "pagedown":
                            return KeyCode.PageDown;
                        case "pageup":
                            return KeyCode.PageUp;
                        case "home":
                            return KeyCode.Home;
                        case "end":
                            return KeyCode.End;
                        case "insert":
                            return KeyCode.Insert;
                        case "delete":
                            return KeyCode.Delete;
                        case "capslock":
                            return KeyCode.CapsLock;
                        case "numlock":
                            return KeyCode.Numlock;
                        case "printscreen":
                            return KeyCode.Print;
                        case "scrolllock":
                            return KeyCode.ScrollLock;
                        case "pause":
                            return KeyCode.Pause;
                        case "numpadenter":
                            return KeyCode.KeypadEnter;
                        case "numpaddivide":
                            return KeyCode.KeypadDivide;
                        case "numpadmultiply":
                            return KeyCode.KeypadMultiply;
                        case "numpadplus":
                            return KeyCode.KeypadPlus;
                        case "numpadminus":
                            return KeyCode.KeypadMinus;
                        case "numpadperiod":
                            return KeyCode.KeypadPeriod;
                        case "numpadequals":
                            return KeyCode.KeypadEquals;
                        case "numpad0":
                            return KeyCode.Keypad0;
                        case "numpad1":
                            return KeyCode.Keypad1;
                        case "numpad2":
                            return KeyCode.Keypad2;
                        case "numpad3":
                            return KeyCode.Keypad3;
                        case "numpad4":
                            return KeyCode.Keypad4;
                        case "numpad5":
                            return KeyCode.Keypad5;
                        case "numpad6":
                            return KeyCode.Keypad6;
                        case "numpad7":
                            return KeyCode.Keypad7;
                        case "numpad8":
                            return KeyCode.Keypad8;
                        case "numpad9":
                            return KeyCode.Keypad9;
                        case "f1":
                            return KeyCode.F1;
                        case "f2":
                            return KeyCode.F2;
                        case "f3":
                            return KeyCode.F3;
                        case "f4":
                            return KeyCode.F4;
                        case "f5":
                            return KeyCode.F5;
                        case "f6":
                            return KeyCode.F6;
                        case "f7":
                            return KeyCode.F7;
                        case "f8":
                            return KeyCode.F8;
                        case "f9":
                            return KeyCode.F9;
                        case "f10":
                            return KeyCode.F10;
                        case "f11":
                            return KeyCode.F11;
                        case "f12":
                            return KeyCode.F12;
                        case "uparrow":
                            return KeyCode.UpArrow;
                        case "downarrow":
                            return KeyCode.DownArrow;
                        case "leftarrow":
                            return KeyCode.LeftArrow;
                        case "rightarrow":
                            return KeyCode.RightArrow;
                        case "quote":
                        case "#(')":
                            return KeyCode.Quote;
                        case "semicolon":
                        case "#(;)":
                            return KeyCode.Semicolon;
                        case "comma":
                        case "#(,)":
                            return KeyCode.Comma;
                        case "period":
                        case "#(.)":
                            return KeyCode.Period;
                        case "slash":
                        case "#(/)":
                            return KeyCode.Slash;
                        case "backslash":
                        case "#(\\)":
                            return KeyCode.Backslash;
                        case "leftbracket":
                        case "#([)":
                            return KeyCode.LeftBracket;
                        case "rightbracket":
                        case "#(])":
                            return KeyCode.RightBracket;
                        case "a":
                        case "#(a)":
                            return KeyCode.A;
                        case "b":
                        case "#(b)":
                            return KeyCode.B;
                        case "c":
                        case "#(c)":
                            return KeyCode.C;
                        case "d":
                        case "#(d)":
                            return KeyCode.D;
                        case "e":
                        case "#(e)":
                            return KeyCode.E;
                        case "f":
                        case "#(f)":
                            return KeyCode.F;
                        case "g":
                        case "#(g)":
                            return KeyCode.G;
                        case "h":
                        case "#(h)":
                            return KeyCode.H;
                        case "i":
                        case "#(i)":
                            return KeyCode.I;
                        case "j":
                        case "#(j)":
                            return KeyCode.J;
                        case "k":
                        case "#(k)":
                            return KeyCode.K;
                        case "l":
                        case "#(l)":
                            return KeyCode.L;
                        case "m":
                        case "#(m)":
                            return KeyCode.M;
                        case "n":
                        case "#(n)":
                            return KeyCode.N;
                        case "o":
                        case "#(o)":
                            return KeyCode.O;
                        case "p":
                        case "#(p)":
                            return KeyCode.P;
                        case "q":
                        case "#(q)":
                            return KeyCode.Q;
                        case "r":
                        case "#(r)":
                            return KeyCode.R;
                        case "s":
                        case "#(s)":
                            return KeyCode.S;
                        case "t":
                        case "#(t)":
                            return KeyCode.T;
                        case "u":
                        case "#(u)":
                            return KeyCode.U;
                        case "v":
                        case "#(v)":
                            return KeyCode.V;
                        case "w":
                        case "#(w)":
                            return KeyCode.W;
                        case "x":
                        case "#(x)":
                            return KeyCode.X;
                        case "y":
                        case "#(y)":
                            return KeyCode.Y;
                        case "z":
                        case "#(z)":
                            return KeyCode.Z;
                        case "0":
                        case "#(0)":
                            return KeyCode.Alpha0;
                        case "1":
                        case "#(1)":
                            return KeyCode.Alpha1;
                        case "2":
                        case "#(2)":
                            return KeyCode.Alpha2;
                        case "3":
                        case "#(3)":
                            return KeyCode.Alpha3;
                        case "4":
                        case "#(4)":
                            return KeyCode.Alpha4;
                        case "5":
                        case "#(5)":
                            return KeyCode.Alpha5;
                        case "6":
                        case "#(6)":
                            return KeyCode.Alpha6;
                        case "7":
                        case "#(7)":
                            return KeyCode.Alpha7;
                        case "8":
                        case "#(8)":
                            return KeyCode.Alpha8;
                        case "9":
                        case "#(9)":
                            return KeyCode.Alpha9;
                        case "escape":
                            return KeyCode.Escape;
                    }
                }
            }
            return KeyCode.None;
        }
        public static string KeyToInputPath(KeyCode key)
        {
            return "/keyboard/" + (key switch {
                KeyCode.BackQuote => "backquote",
                KeyCode.Quote => "quote",
                KeyCode.Space => "space",
                KeyCode.Return => "enter",
                KeyCode.Tab => "tab",
                KeyCode.LeftShift => "leftShift",
                KeyCode.RightShift => "rightShift",
                KeyCode.LeftAlt => "leftAlt",
                KeyCode.LeftControl => "leftControl",
                KeyCode.LeftMeta => "leftMeta",
                KeyCode.RightMeta => "rightMeta",
                KeyCode.Menu => "contextmenu",
                KeyCode.Backspace => "backspace",
                KeyCode.PageDown => "pageDown",
                KeyCode.PageUp => "pageUp",
                KeyCode.Home => "home",
                KeyCode.End => "end",
                KeyCode.Insert => "insert",
                KeyCode.Delete => "delete",
                KeyCode.CapsLock => "capsLock",
                KeyCode.Numlock => "numsLock",
                KeyCode.Print => "printScreen",
                KeyCode.ScrollLock => "scrollLock",
                KeyCode.Pause => "pause",
                KeyCode.KeypadEnter => "numpadEnter",
                KeyCode.KeypadDivide => "numpadDivide",
                KeyCode.KeypadMultiply => "numpadMultiply",
                KeyCode.KeypadPlus => "numpadPlus",
                KeyCode.KeypadMinus => "numpadMinus",
                KeyCode.KeypadPeriod => "numpadPeriod",
                KeyCode.KeypadEquals => "numpadEquals",
                KeyCode.Keypad0 => "numpad0",
                KeyCode.Keypad1 => "numpad1",
                KeyCode.Keypad2 => "numpad2",
                KeyCode.Keypad3 => "numpad3",
                KeyCode.Keypad4 => "numpad4",
                KeyCode.Keypad5 => "numpad5",
                KeyCode.Keypad6 => "numpad6",
                KeyCode.Keypad7 => "numpad7",
                KeyCode.Keypad8 => "numpad8",
                KeyCode.Keypad9 => "numpad9",
                KeyCode.F1 => "f1",
                KeyCode.F2 => "f2",
                KeyCode.F3 => "f3",
                KeyCode.F4 => "f4",
                KeyCode.F5 => "f5",
                KeyCode.F6 => "f6",
                KeyCode.F7 => "f7",
                KeyCode.F8 => "f8",
                KeyCode.F9 => "f9",
                KeyCode.F10 => "f10",
                KeyCode.F11 => "f11",
                KeyCode.F12 => "f12",
                KeyCode.UpArrow => "upArrow",
                KeyCode.DownArrow => "downArrow",
                KeyCode.LeftArrow => "leftArrow",
                KeyCode.RightArrow => "rightArrow",
                KeyCode.Semicolon => "semiColon",
                KeyCode.Comma => "comma",
                KeyCode.Period => "period",
                KeyCode.Slash => "slash",
                KeyCode.Backslash => "backslash",
                KeyCode.LeftBracket => "leftBracket",
                KeyCode.RightBracket => "rightBracket",
                KeyCode.A => "a",
                KeyCode.B => "b",
                KeyCode.C => "c",
                KeyCode.D => "d",
                KeyCode.E => "e",
                KeyCode.F => "f",
                KeyCode.G => "g",
                KeyCode.H => "h",
                KeyCode.I => "i",
                KeyCode.J => "j",
                KeyCode.K => "k",
                KeyCode.L => "l",
                KeyCode.M => "m",
                KeyCode.N => "n",
                KeyCode.O => "o",
                KeyCode.P => "p",
                KeyCode.Q => "q",
                KeyCode.R => "r",
                KeyCode.S => "s",
                KeyCode.T => "t",
                KeyCode.U => "u",
                KeyCode.V => "v",
                KeyCode.W => "w",
                KeyCode.X => "x",
                KeyCode.Y => "y",
                KeyCode.Z => "z",
                KeyCode.Alpha0 => "0",
                KeyCode.Alpha1 => "1",
                KeyCode.Alpha2 => "2",
                KeyCode.Alpha3 => "3",
                KeyCode.Alpha4 => "4",
                KeyCode.Alpha5 => "5",
                KeyCode.Alpha6 => "6",
                KeyCode.Alpha7 => "7",
                KeyCode.Alpha8 => "8",
                KeyCode.Alpha9 => "9",
                KeyCode.Escape => "escape",
                _ => ""
            });
        }

        /// <summary>
        /// Gets or creates the Storm Tools Game Object in the Scene
        /// </summary>
        /// <param name="create"></param>
        private static GameObject GetToolsGameObject(bool create = true)
        {
            //TODO: Move to a constant
            string name = "Storm Tools";
            GameObject stormToolsObj = GameObject.Find(name);
            if (stormToolsObj == null && create)
            {
                stormToolsObj = new GameObject(name);

                //Prevent tool contents from ending up in the build
                //We might need to introduce "Storm Runtime Tools" as well as we might want to
                //have runtime tools in the scene at some point
                stormToolsObj.tag = "EditorOnly";
                stormToolsObj.hideFlags = HideFlags.DontSaveInBuild;
            }
            return stormToolsObj;
        }
        #endregion
        static bool lastHasGaiaSeaLevel = false;
        static float lastGaiaSeaLevel = 0f;
        static float nextGaiaSeaLevelCheck = 0f;
        static float nextHasGaiaSeaLevelCheck = 0f;

        static bool GaiaExistCheckDone = false;
        static bool DoesGaiaExist = false;

        static MethodInfo gaiaWaterInstanceGettter = null;
        static MethodInfo gaiaWaterSetSeaLevel = null;
        static MethodInfo gaiaGetSessionManager = null;
        static MethodInfo gaiaGetSeaLevel = null;
        static MethodInfo gaiaSetSeaLevel = null;

        static Object gaiaWaterSystemInstance = null;
        static Object gaiaSessionManagerInstance = null;

        public static bool HasGaiaSeaLevel()
        {
            if (!GaiaExistCheckDone)
            {
                GaiaExistCheckDone = true;
                Type wlType = Type.GetType("Gaia.PWS_WaterSystem, GaiaCore, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
                Type smType = Type.GetType("Gaia.GaiaSessionManager, GaiaCore, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
                Type apiType = Type.GetType("Gaia.GaiaAPI, GaiaCore, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
                if (wlType != null && smType != null && apiType != null)
                {
                    PropertyInfo instanceProp = wlType.GetProperty("Instance", BindingFlags.Static | BindingFlags.GetProperty | BindingFlags.Public);
                    if (instanceProp != null)
                        gaiaWaterInstanceGettter = instanceProp.GetMethod;
                    PropertyInfo sealevelProp = wlType.GetProperty("SeaLevel", BindingFlags.Instance | BindingFlags.SetProperty | BindingFlags.Public);
                    if (sealevelProp != null)
                        gaiaWaterSetSeaLevel = sealevelProp.SetMethod;
                    gaiaGetSessionManager = smType.GetMethod("GetSessionManager", BindingFlags.Static | BindingFlags.Public);
                    gaiaGetSeaLevel = apiType.GetMethod("GetSeaLevel", BindingFlags.Public | BindingFlags.Static);
                    gaiaSetSeaLevel = apiType.GetMethod("SetSeaLevel", BindingFlags.Public | BindingFlags.Static);
                }

                DoesGaiaExist = gaiaSetSeaLevel != null && gaiaGetSeaLevel != null && gaiaGetSessionManager != null && gaiaWaterSetSeaLevel != null && gaiaWaterInstanceGettter != null;
            }
            if (DoesGaiaExist)
            {
#if UNITY_EDITOR
                float now = Application.isPlaying ? Time.time : (float)EditorApplication.timeSinceStartup;
#else
                float now = Time.time;
#endif
                if (nextHasGaiaSeaLevelCheck > now)
                    return lastHasGaiaSeaLevel;
                nextHasGaiaSeaLevelCheck = now + 10;

                if (gaiaWaterSystemInstance == null)
                    gaiaWaterSystemInstance = gaiaWaterInstanceGettter.Invoke(null, new object[0]) as Object;
                if (gaiaSessionManagerInstance == null)
                    gaiaSessionManagerInstance = gaiaGetSessionManager.Invoke(null, new object[] { false, false }) as Object;
                if (gaiaWaterSystemInstance != null || gaiaSessionManagerInstance != null)
                {
                    lastHasGaiaSeaLevel = true;
                    return true;
                }
                lastHasGaiaSeaLevel = false;
            }
            return false;
        }
        public static float GetGaiaSeaLevel()
        {
            if (DoesGaiaExist)
            {
#if UNITY_EDITOR
                float now = Application.isPlaying ? Time.time : (float)EditorApplication.timeSinceStartup;
#else
                float now = Time.time;
#endif
                if (nextGaiaSeaLevelCheck > now)
                    return lastGaiaSeaLevel;
                nextGaiaSeaLevelCheck = now + 10;
                lastGaiaSeaLevel = (float)gaiaGetSeaLevel.Invoke(null, new object[0]);
            }
            return lastGaiaSeaLevel;
        }
        public static void SetGaiaSeaLevel(float val)
        {
            if (DoesGaiaExist)
            {
                lastGaiaSeaLevel = val;
                gaiaSetSeaLevel.Invoke(null, new object[1] { val });
                if (!Application.isPlaying && gaiaWaterSystemInstance != null)
                    gaiaWaterSetSeaLevel.Invoke(gaiaWaterSystemInstance, new object[] { val });
            }
        }
    }
}