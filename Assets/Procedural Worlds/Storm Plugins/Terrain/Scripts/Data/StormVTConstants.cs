using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ProceduralWorlds.Storm.Terrain
{
    public static class StormVTConstants
    {
        #region Public Variables
        /// <summary>
        /// Default texture format to use when performing editor side texture manipulation
        /// </summary>
        public const TextureFormat DefaultTextureFormat = TextureFormat.RGBA32;

        /// <summary>
        /// Name of the configuration file used for loading in files
        /// </summary>
        public const string ContentConfFileName = "StormVT Content.conf";

        /// <summary>
        /// Folder path of where to store all Storm Terrain generated data
        /// </summary>
        public const string AssetsFolderPath = "Assets/Storm User Data/Terrain";

        /// <summary>
        /// Old folder path of where to store all Storm Terrain generated data
        /// </summary>
        public const string OldAssetsFolderPath = "Assets/VTTerrain User Data";

        /// <summary>
        /// File path to save out generated tiles / terrain data to load in a build
        /// </summary>
        public const string TileFolderPath = "Tiles/Storm User Data/Terrain";
        #endregion

        #region Private Variables

        /// <summary>
        /// Member variable reference to the content folder path
        /// </summary>
        private static string m_contentFolderPath = null;

        /// <summary>
        /// Static public reference to the content folder path
        /// </summary>
        public static string ContentFolderPath
        {
            get
            {
                return m_contentFolderPath;
            }
            set { m_contentFolderPath = value; }
        }

        #endregion

        /// <summary>
        /// Returns the height map TextureFormat for each platform, as RFloat / R116 is not supported on every platform
        /// </summary>
        /// <returns>TextureFormat</returns>
        public static TextureFormat GetHeightmapTextureFormat()
        {
            TextureFormat format = TextureFormat.R16;
            
            if (SystemInfo.SupportsTextureFormat(TextureFormat.RFloat))
            {
                format = TextureFormat.RFloat;
            }
            else if (SystemInfo.SupportsTextureFormat(TextureFormat.R16))
            {
                format = TextureFormat.R16;
            }
            else if (SystemInfo.SupportsTextureFormat(TextureFormat.R8))
            {
                format = TextureFormat.R8;
            }
            else
            {
                format = TextureFormat.RGBA32;
            }
            
            #if UNITY_ANDROID || UNITY_IOS || UNITY_TVOS
                format = TextureFormat.R16;
            #endif

            return format;
        }

        /// <summary>
        /// Returns the height map RenderTextureFormat for each platform, as RFloat / R116 is not supported on every platform
        /// </summary>
        /// <returns>RenderTextureFormat</returns>
        public static RenderTextureFormat GetHeightmapRenderTextureFormat()
        {
            RenderTextureFormat format = RenderTextureFormat.R16;
            
            if (SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.RFloat))
            {
                format = RenderTextureFormat.RFloat;
            }
            else if (SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.R16))
            {
                format = RenderTextureFormat.R16;
            }
            else if (SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.R8))
            {
                format = RenderTextureFormat.R8;
            }
            else
            {
                format = RenderTextureFormat.ARGB32;
            }

            #if UNITY_ANDROID || UNITY_IOS || UNITY_TVOS
                format = RenderTextureFormat.R16;
            #endif
            
            return format;
        }



        /// <summary>
        /// Static reference to the storm terrain defaults path
        /// </summary>
        public static string StormVTDefaultsPath => $"{ContentFolderPath}/StormVT Defaults.asset";

        /// <summary>
        /// Static reference to the storm terrain gaia connector path
        /// </summary>
        public static string StormVTGaiaPath => $"{ContentFolderPath}/StormVT Gaia.asset";

        /// <summary>
        /// Gets the user data folder
        /// </summary>
        /// <param name="subFolderName"></param>
        /// <returns></returns>
        public static string GetAssetsFolder(string subFolderName = "")
        {
            string fullPath = $"{AssetsFolderPath}/{subFolderName}";
            if (!Directory.Exists(fullPath))
                Directory.CreateDirectory(fullPath);
            return fullPath;
        }

        /// <summary>
        /// Location on where to save storm terrain scriptable object data
        /// </summary>
        /// <param name="subFolderName">Sub folder to look in / create</param>
        /// <returns></returns>
        public static string GetAssetsFolderOld(string subFolderName = "")
        {
            string fullPath = $"{OldAssetsFolderPath}/{subFolderName}";
            if (!Directory.Exists(fullPath))
                Directory.CreateDirectory(fullPath);
            return fullPath;
        }

        /// <summary>
        /// Location on where to save storm terrain scriptable object data
        /// </summary>
        /// <returns></returns>
        public static string GetAssetsFolder()
        {
            var activeScene = SceneManager.GetActiveScene();
            if (!activeScene.IsValid())
                return string.Empty;
            string sceneName = activeScene.name;
            return GetAssetsFolder(sceneName);
        }

        /// <summary>
        /// Location on where to save the generated terrain textures
        /// </summary>
        /// <param name="subFolderName">Sub folder to look in / create</param>
        /// <returns></returns>
        public static string GetTerrainTextureAssetsFolder(string subFolderName = "")
        {
            string fullPath = $"{AssetsFolderPath}/{subFolderName}";
            if (!Directory.Exists(fullPath))
                Directory.CreateDirectory(fullPath);
            return fullPath;
        }

        /// <summary>
        /// Location on where to save the generated terrain textures
        /// </summary>
        /// <returns></returns>
        public static string GetTerrainTextureAssetsFolder()
        {
            var activeScene = SceneManager.GetActiveScene();
            if (!activeScene.IsValid())
                return string.Empty;
            string sceneName = activeScene.name + "/TerrainData/";
            return GetTerrainTextureAssetsFolder(sceneName);
        }


        /// <summary>
        /// Returns the folder to save tiles to. Used in a build
        /// </summary>
        /// <param name="subFolderName">Sub folder to look in / create</param>
        /// <returns></returns>
        public static string GetTileFolder(string subFolderName = "")
        {
            string dataFolder = Path.GetDirectoryName(Application.dataPath);
            string fullPath = $"{dataFolder}/{TileFolderPath}/{subFolderName}";
            if (!Directory.Exists(fullPath))
                Directory.CreateDirectory(fullPath);
            return fullPath;
        }

        /// <summary>
        /// Returns the folder to save tiles to. Used in a build
        /// </summary>
        /// <returns></returns>
        public static string GetTileFolder()
        {
            var activeScene = SceneManager.GetActiveScene();
            if (!activeScene.IsValid())
                return string.Empty;
            string sceneName = activeScene.name;
            return GetTileFolder(sceneName);
        }

        /// <summary>
        /// Returns the asset path from the asset file, and if not found, creates the directory
        /// </summary>
        /// <param name="assetName">Name of the asset to save</param>
        /// <returns></returns>
        public static string GetOrCreateAssetPath(string assetName)
        {
            string folderPath = GetTerrainTextureAssetsFolder();
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);
            return $"{folderPath}/{assetName}.asset";
        }

        /// <summary>
        /// Writes the bytes of a texture to disk
        /// </summary>
        /// <param name="path"></param>
        /// <param name="bytes"></param>
        public static void WriteAllBytes(string path, byte[] bytes)
        {
            using (Stream stream = (Stream)File.Create(path))
                stream.Write(bytes, 0, bytes.Length);
        }

        /// <summary>
        /// Write the texture to disk
        /// </summary>
        /// <param name="path"></param>
        /// <param name="sourceTexture"></param>
        public static void WriteTexture2D(string path, Texture2D sourceTexture, bool exr = false)
        {
            byte[] exrBytes;
            if(exr)
            {
                exrBytes = ImageConversion.EncodeToEXR(sourceTexture);
            }
            else
            {
                exrBytes = ImageConversion.EncodeToPNG(sourceTexture);
            }
            if (exrBytes != null)
            {
                WriteAllBytes(path, exrBytes);
            }
        }

        /// <summary>
        /// Check whether the value is a power of 2
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public static bool IsPowerOfTwo(ulong x)
        {
            return (x != 0) && ((x & (x - 1)) == 0);
        }

        /// <summary>
        /// Converts a non power of 2 grid dimensions into a power of 2 grid dimensions
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static int PowerOf2GridSize(int x, int y)
        {
            int highestSize = x > y ? x : y;
            highestSize |= highestSize >> 1;
            highestSize |= highestSize >> 2;
            highestSize |= highestSize >> 4;
            highestSize |= highestSize >> 8;
            highestSize |= highestSize >> 16;
            highestSize++;

            return highestSize;
        }

    }
}


