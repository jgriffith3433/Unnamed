using UnityEngine;
using Object = UnityEngine.Object;

namespace ProceduralWorlds.Storm.Terrain
{
    [CreateAssetMenu(fileName = "StormVT Defaults", menuName = "Procedural Worlds/Storm/Terrain/StormVT Defaults", order = 0)]
    public class StormVTDefaults : ScriptableObject
    {
        #region Singleton

        /// <summary>
        /// Memeber variable of the defaults instance
        /// </summary>
        private static StormVTDefaults m_instance = null;

        /// <summary>
        /// Public variable of the defaults instance
        /// </summary>
        public static StormVTDefaults Instance
        {

            get
            {
                if (m_instance == null)
                {
                #if UNITY_EDITOR
                    m_instance = UnityEditor.AssetDatabase.LoadAssetAtPath<StormVTDefaults>(StormVTConstants.StormVTDefaultsPath);
                #endif
                }

                return m_instance;
            }
            set { m_instance = value; }

        }

        #endregion

        #region Variables

        /// <summary>
        /// Reference to the BuiltIn Storm Terrain Material
        /// </summary>
        [SerializeField] public Material builtInVirtualTerrainMaterial;

        /// <summary>
        /// Reference to the URP Storm Terrain Material
        /// </summary>
        [SerializeField] public Material urpVirtualTerrainMaterial;

        /// <summary>
        /// Reference to the HDRP Storm Terrain Material
        /// </summary>
        [SerializeField] public Material hdrpVirtualTerrainMaterial;

        /// <summary>
        /// Reference to the BuiltIn Unity Terrain Material
        /// </summary>
        [SerializeField] public Material builtInTerrainMaterial;

        /// <summary>
        /// Reference to the URP Unity Terrain Material
        /// </summary>
        [SerializeField] public Material urpTerrainMaterial;

        /// <summary>
        /// Reference to the HDRP Unity Terrain Material
        /// </summary>
        [SerializeField] public Material hdrpTerrainMaterial;

        /// <summary>
        /// Reference to the BuiltIn Clipmap Terrain Shader
        /// </summary>
        [SerializeField] public Shader builtinClipmapTerrainShader;

        /// <summary>
        /// Reference to the BuiltIn Unity Terrain Shader
        /// </summary>
        [SerializeField] public Shader builtinUnityTerrainShader;

        /// <summary>
        /// Reference to the HDRP Clipmap Terrain Shader
        /// </summary>
        [SerializeField] public Shader hdrpClipmapTerrainShader;

        /// <summary>
        /// Reference to the HDRP Unity Terrain Shader
        /// </summary>
        [SerializeField] public Shader hdrpUnityTerrainShader;

        /// <summary>
        /// Reference to the URP Clipmap Terrain Shader
        /// </summary>
        [SerializeField] public Shader urpClipmapTerrainShader;

        /// <summary>
        /// Reference to the URP Unity Terrain Shader
        /// </summary>
        [SerializeField] public Shader urpUnityTerrainShader;


        /// <summary>
        /// Reference to the BuiltIn Storm Terrain Shader Package
        /// </summary>
        [SerializeField] public Object builtInPackage;

        /// <summary>
        /// Reference to the URP Storm Terrain Shader Package
        /// </summary>
        [SerializeField] public Object urpPackage;

        /// <summary>
        /// Reference to the HDRP Storm Terrain Shader Package
        /// </summary>
        [SerializeField] public Object hdrpPackage;
        
        /// <summary>
        /// Reference to the Storm Terrain Prefab asset
        /// </summary>
        [SerializeField] public GameObject vtPrefab;
        
        /// <summary>
        /// Reference to the shader packages folder
        /// </summary>
        [SerializeField] public Object shaderFolder;

        /// <summary>
        /// Reference to the default snow diffuse map
        /// </summary>
        [SerializeField] public Texture snowDiffuse;

        /// <summary>
        /// Reference to the default snow normal map
        /// </summary>
        [SerializeField] public Texture snowNormal;

        /// <summary>
        /// Reference to the default snow mask map
        /// </summary>
        [SerializeField] public Texture snowMask;

        /// <summary>
        /// Reference to the default macro variation map
        /// </summary>
        [SerializeField] public Texture macroVariation;

        /// <summary>
        /// Reference to the default default normal map
        /// </summary>
        [SerializeField] public Texture detailNormal;
        #endregion

        #region Properties

        /// <summary>
        /// Keeps track of whether the pipeline specific packages have been imported yet or not
        /// </summary>
        public bool packagesImported
        {
            get
            {
                StormVTUtilities.GetPipelineShaders(out Shader gtsShader, out Shader unityShader);
                if (gtsShader == null)
                    return false;
                return true;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Returns the Storm Virtual Terrain Material, depending on the pipeline
        /// </summary>
        /// <returns></returns>
        public static Material GetVirtualTerrainMaterial()
        {
            StormVTDefaults defaults = Instance;
            if (defaults == null)
            {
                return null;
            }
                
            StormVTEnums.StormVTPipeline currentPipeline = StormVTUtilities.GetCurrentPipeline();
            switch (currentPipeline)
            {
                case StormVTEnums.StormVTPipeline.BuiltIn:
                    return defaults.builtInVirtualTerrainMaterial;
                case StormVTEnums.StormVTPipeline.URP:
                    return defaults.urpVirtualTerrainMaterial;
                case StormVTEnums.StormVTPipeline.HDRP:
                    return defaults.hdrpVirtualTerrainMaterial;
            }

            return null;
        }

        /// <summary>
        /// Returns the Unity default terrain material, depending on the pipeline
        /// </summary>
        /// <returns></returns>
        public static Material GetDefaultTerrainMaterial()
        {
            StormVTDefaults defaults = Instance;
            if (defaults == null)
                return null;
            StormVTEnums.StormVTPipeline currentPipeline = StormVTUtilities.GetCurrentPipeline();
            switch (currentPipeline)
            {
                case StormVTEnums.StormVTPipeline.BuiltIn:
                    return defaults.builtInTerrainMaterial;
                case StormVTEnums.StormVTPipeline.URP:
                    return defaults.urpTerrainMaterial;
                case StormVTEnums.StormVTPipeline.HDRP:
                    return defaults.hdrpTerrainMaterial;
            }

            return null;
        }

        /// <summary>
        /// Returns the clipmap terrain shader, depending on the rendering pipeline
        /// </summary>
        /// <returns></returns>
        public static Shader GetClipmapTerrainShader()
        {
            StormVTDefaults defaults = Instance;
            if (defaults == null)
            {
                return null;
            }

            StormVTEnums.StormVTPipeline currentPipeline = StormVTUtilities.GetCurrentPipeline();
            switch (currentPipeline)
            {
                case StormVTEnums.StormVTPipeline.BuiltIn:
                    return defaults.builtinClipmapTerrainShader;
                case StormVTEnums.StormVTPipeline.URP:
                    return defaults.urpClipmapTerrainShader;
                case StormVTEnums.StormVTPipeline.HDRP:
                    return defaults.hdrpClipmapTerrainShader;
            }

            return null;
        }

        /// <summary>
        /// Returns the default unity terrain material, depending on the rendering pipeline
        /// </summary>
        /// <returns></returns>
        public static Shader GetUnityTerrainShader()
        {
            StormVTDefaults defaults = Instance;
            if (defaults == null)
            {
                return null;
            }

            StormVTEnums.StormVTPipeline currentPipeline = StormVTUtilities.GetCurrentPipeline();
            switch (currentPipeline)
            {
                case StormVTEnums.StormVTPipeline.BuiltIn:
                    return defaults.builtinUnityTerrainShader;
                case StormVTEnums.StormVTPipeline.URP:
                    return defaults.urpUnityTerrainShader;
                case StormVTEnums.StormVTPipeline.HDRP:
                    return defaults.hdrpUnityTerrainShader;
            }

            return null;
        }

        /// <summary>
        /// Returns the Storm Terrain prefab asset
        /// </summary>
        /// <returns></returns>
        public static GameObject GetVTPrefab()
        {
            StormVTDefaults defaults = Instance;
            return defaults.vtPrefab;
        }

        #endregion
    }
}