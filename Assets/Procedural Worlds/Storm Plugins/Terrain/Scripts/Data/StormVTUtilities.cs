using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace ProceduralWorlds.Storm.Terrain
{
    public static class StormVTUtilities
    {
        #region Shaders & Materials

        //Create World Normal
        private static Shader m_createWorldNormalShader;

        public static Shader CreateWorldNormalShader
        {
            get
            {
                if (m_createWorldNormalShader == null)
                    m_createWorldNormalShader = Shader.Find("Hidden/SH_GetWorldNormalMap");
                return m_createWorldNormalShader;
            }
        }

        private static Material m_createWorldNormalMaterial;

        public static Material CreateWorldNormalMaterial
        {
            get
            {
                if (m_createWorldNormalMaterial == null)
                    m_createWorldNormalMaterial = new Material(CreateWorldNormalShader);
                return m_createWorldNormalMaterial;
            }
        }

        //Alpha Merge Shader
        private static Shader m_alphaMergeShader;

        public static Shader AlphaMergeShader
        {
            get
            {
                if (m_alphaMergeShader == null)
                    m_alphaMergeShader = Shader.Find("Hidden/SH_AlphaMerge");
                return m_alphaMergeShader;
            }
        }

        private static Material m_alphaMergeMaterial;

        public static Material AlphaMergeMaterial
        {
            get
            {
                if (m_alphaMergeMaterial == null)
                    m_alphaMergeMaterial = new Material(AlphaMergeShader);
                return m_alphaMergeMaterial;
            }
        }

        // Setup our post processing converting materials
        private static Shader m_convertToAlbedoShader;
        public static Shader convertToAlbedoShader
        {
            get
            {
                if (m_convertToAlbedoShader == null)
                    m_convertToAlbedoShader = Shader.Find("Hidden/StormVT_CombineAlbedoMetallic");
                return m_convertToAlbedoShader;
            }
        }
        private static Shader m_convertToNormalShader;
        public static Shader convertToNormalShader
        {
            get
            {
                if (m_convertToNormalShader == null)
                    m_convertToNormalShader = Shader.Find("Hidden/StormVT_CombineNormalMask");
                return m_convertToNormalShader;
            }
        }
        private static Shader m_convertMaskMapShader;
        public static Shader convertToMaskMapShader
        {
            get
            {
                if (m_convertMaskMapShader == null)
                    m_convertMaskMapShader = Shader.Find("Hidden/StormVT_ConvertMaskMap");
                return m_convertMaskMapShader;
            }
        }
        private static Shader m_createNormalMapShader;
        public static Shader createNormalMapShader
        {
            get
            {
                if (m_createNormalMapShader == null)
                    m_createNormalMapShader = Shader.Find("Hidden/StormVT_CreateBaseNormal");
                return m_createNormalMapShader;
            }
        }
        private static Shader m_createAlbedoMapShader;
        public static Shader createAlbedoMapShader
        {
            get
            {
                if (m_createAlbedoMapShader == null)
                    m_createAlbedoMapShader = Shader.Find("Hidden/StormVT_CreateBaseColor");
                return m_createAlbedoMapShader;
            }
        }

        private static Material m_convertToAlbedoMaterial;
        public static Material convertToAlbedoMaterial
        {
            get
            {
                if (m_convertToAlbedoMaterial == null)
                    m_convertToAlbedoMaterial = new Material(convertToAlbedoShader);
                return m_convertToAlbedoMaterial;
            }
        }
        private static Material m_convertToNormalMaterial;
        public static Material convertToNormalMaterial
        {
            get
            {
                if (m_convertToNormalMaterial == null)
                    m_convertToNormalMaterial = new Material(convertToNormalShader);
                return m_convertToNormalMaterial;
            }
        }
        private static Material m_convertToMaskMapMaterial;
        public static Material convertToMaskMapMaterial
        {
            get
            {
                if (m_convertToMaskMapMaterial == null)
                    m_convertToMaskMapMaterial = new Material(convertToMaskMapShader);
                return m_convertToMaskMapMaterial;
            }
        }
        private static Material m_createNormalMapMaterial;
        public static Material createNormalMapMaterial
        {
            get
            {
                if (m_createNormalMapMaterial == null)
                    m_createNormalMapMaterial = new Material(createNormalMapShader);
                return m_createNormalMapMaterial;
            }
        }
        private static Material m_createAlbedoMapMaterial;
        public static Material createAlbedoMapMaterial
        {
            get
            {
                if (m_createAlbedoMapMaterial == null)
                    m_createAlbedoMapMaterial = new Material(createAlbedoMapShader);
                return m_createAlbedoMapMaterial;
            }
        }


        #endregion

        #region Extension Methods


        #region RenderTextureFormat

        /// <summary>
        /// Returns the relevant TextureFormat that matches the RenderTextureFormat
        /// </summary>
        /// <param name="format"></param>
        /// <returns></returns>
        public static TextureFormat ToTextureFormat(this RenderTextureFormat format)
        {
            switch (format)
            {
                case RenderTextureFormat.ARGB32:
                    return TextureFormat.ARGB32;
                case RenderTextureFormat.RGB565:
                    return TextureFormat.RGB565;
                case RenderTextureFormat.ARGBHalf:
                    return TextureFormat.RGBAHalf;
                case RenderTextureFormat.RGHalf:
                    return TextureFormat.RGHalf;
                case RenderTextureFormat.RHalf:
                    return TextureFormat.RHalf;
                case RenderTextureFormat.ARGBFloat:
                    return TextureFormat.RGBAFloat;
                case RenderTextureFormat.RGFloat:
                    return TextureFormat.RGFloat;
                case RenderTextureFormat.RFloat:
                    return TextureFormat.RFloat;
                case RenderTextureFormat.R16:
                    return TextureFormat.R16;
                default:
                    return TextureFormat.ARGB32; // Default format or handle appropriately
            }
        }

        #endregion

        #region RenderTexture

        /// <summary>
        /// Converts a RenderTexture into a Texture2D
        /// </summary>
        /// <param name="texture">RenderTexture to convert</param>
        /// <returns></returns>
        public static Texture2D ToTexture2D(this RenderTexture texture)
        {
            int width = texture.width;
            int height = texture.height;
            bool useMipMap = texture.useMipMap;
            bool linear = !texture.sRGB;
            RenderTextureFormat format = texture.format;

            TextureFormat textureFormat = format.ToTextureFormat();

            Texture2D texture2D = new Texture2D(width, height, textureFormat, useMipMap, linear);

            RenderTexture.active = texture;

            texture2D.name = texture.name;
            texture2D.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            texture2D.Apply();

            RenderTexture.active = null;

            return texture2D;
        }

        #endregion

        #region List
        
        /// <summary>
        /// Remove and return the first item from the list
        /// </summary>
        /// <param name="list"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public static T Dequeue<T>(this List<T> list)
        {
            if (list.Count == 0)
            {
                throw new InvalidOperationException("The list is empty.");
            }
        
            T value = list[0];
            list.RemoveAt(0);
            return value;
        }

        /// <summary>
        /// Add an item to the end of the list
        /// </summary>
        /// <param name="list"></param>
        /// <param name="item"></param>
        /// <typeparam name="T"></typeparam>
        public static void Enqueue<T>(this List<T> list, T item)
        {
            list.Add(item);
        }

        /// <summary>
        /// View the first item without removing it
        /// </summary>
        /// <param name="list"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public static T Peek<T>(this List<T> list)
        {
            if (list.Count == 0)
            {
                throw new InvalidOperationException("The list is empty.");
            }

            return list[0];
        }
        
        #endregion

        /// <summary>
        /// Returns the current render pipeline that this project is in
        /// </summary>
        /// <returns></returns>
        public static StormVTEnums.StormVTPipeline GetCurrentPipeline()
        {

            #if UNITY_6000_0_OR_NEWER
                        RenderPipelineAsset pipelineAsset = GraphicsSettings.defaultRenderPipeline;
            #else
                        RenderPipelineAsset pipelineAsset = GraphicsSettings.renderPipelineAsset;
#endif

            if (pipelineAsset == null)
                return StormVTEnums.StormVTPipeline.BuiltIn;

            if (pipelineAsset.GetType().ToString().Contains("HDRenderPipelineAsset"))
                return StormVTEnums.StormVTPipeline.HDRP;
            //URP
            if (pipelineAsset.GetType().ToString().Contains("UniversalRenderPipelineAsset"))
                return StormVTEnums.StormVTPipeline.URP;

            return StormVTEnums.StormVTPipeline.BuiltIn;

        }
        
        
        
        /// <summary>
        /// Gets the appropriate Shaders for the current pipeline.
        /// Note: Gets this from the GTS-Pipeline-Material and Default-Pipeline-Material.
        /// </summary>
        /// <param name="gtsShader">Output GTS Shader</param>
        /// <param name="unityShader">Output Unity Shader</param>
        /// <returns>Returns true if both shaders are valid.</returns>
        public static bool GetPipelineShaders(out Shader gtsShader, out Shader unityShader)
        {
            gtsShader = null;
            unityShader = null;
            switch (GetCurrentPipeline())
            {
                case StormVTEnums.StormVTPipeline.BuiltIn:
                    gtsShader = Shader.Find("PW/StormVT_BuiltIn");
                    break;
                case StormVTEnums.StormVTPipeline.URP:
                    gtsShader = Shader.Find("PW/StormVT_URP");
                    break;
                case StormVTEnums.StormVTPipeline.HDRP:
                    gtsShader = Shader.Find("PW/StormVT_HDRP");
                    break;
                default:
                    return false;
            }

            return gtsShader != null;
        }
        
        /// <summary>
        /// Helper function for older versions of FindObjectsByType
        /// </summary>
        /// <typeparam name="T">Object to find</typeparam>
        /// <param name="includeInactive">Whether to search for inactive gameobjects</param>
        /// <returns></returns>
        public static T[] FindObjectsByType<T>(bool includeInactive = false) where T : Component
        {
#if UNITY_2022_3_OR_NEWER
            FindObjectsInactive findInactive = includeInactive ? FindObjectsInactive.Include : FindObjectsInactive.Exclude;
            FindObjectsSortMode sortMode = FindObjectsSortMode.None;
            T[] result = Object.FindObjectsByType<T>(findInactive, sortMode);
#else
            T[] result = Object.FindObjectsOfType<T>(includeInactive);
#endif
            return result;
        }

        /// <summary>
        /// Helper function for older versions of FindObjectByType
        /// </summary>
        /// <typeparam name="T">Object to find</typeparam>
        /// <param name="includeInactive">Whether to search for inactive gameobjects</param>
        public static T FindObjectByType<T>(bool includeInactive = false) where T : Component
        {
#if UNITY_2022_3_OR_NEWER
            FindObjectsInactive findInactive = includeInactive ? FindObjectsInactive.Include : FindObjectsInactive.Exclude;
            FindObjectsSortMode sortMode = FindObjectsSortMode.None;
            var foundList = Object.FindObjectsByType<T>(findInactive, sortMode);
            T result = null;
            if(foundList.Length > 0)
            {
                result = foundList[0];
            }
            //T result = Object.FindObjectsByType<T>(findInactive, sortMode)[0];
#else
            var foundList = Object.FindObjectsOfType<T>(includeInactive);
            T result = null;
            if(foundList.Length > 0)
            {
                result = foundList[0];
            }
            //T result = Object.FindObjectsOfType<T>(includeInactive)[0];
#endif
            return result;
        }



        #endregion

        public static long GetUnixTimestamp()
        {
            return new System.DateTimeOffset(System.DateTime.UtcNow).ToUnixTimeMilliseconds();
        }

        /// <summary>
        /// Resize the supplied texture, also handles non rw textures and makes them rm
        /// </summary>
        /// <param name="texture">Source texture</param>
        /// <param name="format">Texture Format of new Texture</param>
        /// <param name="aniso">Aniso Level of new Texture</param>
        /// <param name="width">Width of new texture</param>
        /// <param name="height">Height of new texture</param>
        /// <param name="mipmap">Generate mipmaps</param>
        /// <param name="linear">Use linear colour conversion</param>
        /// <param name="compress">Compresses Texture using Editor Compression.</param>
        /// <returns>New texture</returns>
        public static Texture2D ResizeTexture(Texture2D texture, TextureFormat format, int aniso, int width, int height, bool mipmap, bool linear)
        {
            RenderTexture rt = RenderTexture.GetTemporary(width, height, 0, RenderTextureFormat.Default, linear ? RenderTextureReadWrite.Linear : RenderTextureReadWrite.sRGB);
            bool prevRgbConversionState = GL.sRGBWrite;
            GL.sRGBWrite = !linear;
            Graphics.Blit(texture, rt);
            RenderTexture previous = RenderTexture.active;
            RenderTexture.active = rt;
            Texture2D newTexture = new Texture2D(width, height, StormVTConstants.DefaultTextureFormat, mipmap, linear);
            newTexture.name = texture.name + " X";
            newTexture.anisoLevel = aniso;
            newTexture.filterMode = texture.filterMode;
            newTexture.wrapMode = texture.wrapMode;
            newTexture.mipMapBias = texture.mipMapBias;
            newTexture.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
            newTexture.Apply(true);
            newTexture = CompressTexture(newTexture, format);
            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(rt);
            GL.sRGBWrite = prevRgbConversionState;
            return newTexture;
        }

        /// <summary>
        /// Returns a compressed Texture2D in the desired output format.
        /// </summary>
        /// <param name="texture">The input texture.</param>
        /// <param name="textureFormat">The compression format to apply.</param>
        /// <returns>Texture2D in the desired format.</returns>
        private static Texture2D CompressTexture(Texture2D texture, TextureFormat textureFormat)
        {
#if UNITY_EDITOR
            // Anything other than TextureCompressionQuality.Fast resulted in extreme (>2 hours!) 
            // Texture baking times for certain compression formats. Change only if truly needed.
#if UNITY_2018_3_OR_NEWER
            EditorUtility.CompressTexture(texture, textureFormat, UnityEditor.TextureCompressionQuality.Fast);
#else
            EditorUtility.CompressTexture(texture, textureFormat, TextureCompressionQuality.Fast);
#endif
#else
            // Fallback in case someone creates profiles during runtime
            texture.Compress(true);
#endif
            texture.Apply(true);
            return texture;
        }

#if UNITY_EDITOR
        #region Editor Utilities
        /// <summary>
        /// Creates an assets if it doesn't exist, otherwise re-save it out
        /// </summary>
        /// <typeparam name="T">Object to save</typeparam>
        /// <param name="asset">Asset to save</param>
        /// <param name="path">Location to save it to</param>
        /// <returns></returns>
        public static T CreateOrReplaceAsset<T>(T asset, string path) where T : Object
        {
            T existingAsset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (existingAsset == null)
            {
                AssetDatabase.CreateAsset(asset, path);
                existingAsset = asset;
            }
            else
            {
                EditorUtility.CopySerialized(asset, existingAsset);
            }
            return existingAsset;
        }
        #endregion
#endif

    }
}