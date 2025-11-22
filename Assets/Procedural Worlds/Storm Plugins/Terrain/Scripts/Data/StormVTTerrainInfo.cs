using UnityEngine;
using Unity.Mathematics;
using static ProceduralWorlds.Storm.Terrain.StormVTEnums;
using UnityEditor;

namespace ProceduralWorlds.Storm.Terrain
{
    [ExecuteAlways]
    public class StormVTTerrainInfo : MonoBehaviour
    {
        #region Public Variables
        /// <summary>
        /// Reference to the Storm Terrain Settings
        /// </summary>
        public StormVTSettings stormVTSettings;

        /// <summary>
        /// The world position of the terrain
        /// </summary>
        public Vector3 m_terrrainOrigin;

        /// <summary>
        /// The size in meters of this terrain
        /// </summary>
        public float m_terrainSize;

        /// <summary>
        /// The grid index of this terrain
        /// </summary>
        public int2 terrainGridIndex;

        /// <summary>
        /// A reference to the per terrain material, used for editor painting of more than 8 terrain layers
        /// </summary>
        public Material virtualTerrainMat;

        /// <summary>
        /// A reference to the terrain data on this gameobject
        /// </summary>
        public UnityEngine.Terrain terrain;

        /// <summary>
        /// A reference to the saved out normalmap (that contains curvature)
        /// </summary>
        public Texture2D normalmap;

        /// <summary>
        /// A reference to the heightmap of this terrain
        /// </summary>
        public RenderTexture heightmap;

        /// <summary>
        /// A reference to the holesmaps of this terrain
        /// </summary>
        public Texture holesmap;

        /// <summary>
        /// A reference to the list of splatmaps that this terrain has
        /// </summary>
        public Texture2D[] splatmaps;

        /// <summary>
        /// A reference to the baked out grass color map for this terrain
        /// </summary>
        public Texture2D grassMapColor;

        /// <summary>
        /// A reference to the baked out grass normal map for this terrain
        /// </summary>
        public Texture2D grassMapNormal;

        /// <summary>
        /// A reference to the baked out tree color map for this terrain
        /// </summary>
        public Texture2D treeMapColor;

        /// <summary>
        /// A reference to the baked out tee normal map for this terrain
        /// </summary>
        public Texture2D treeMapNormal;

        /// <summary>
        /// A reference to the storm terrain manager
        /// </summary>
        private StormVTTerrainManager managerInstance;
        #endregion

        /// <summary>
        /// Subscribe to terrain editing events
        /// </summary>
        public void SubscribeEvents()
        {
            StormVTSettings.OnUpdateStormVTSettings += StormSettingsUpdated;
#if UNITY_EDITOR
            EditorApplication.playModeStateChanged += OnPlaymodeStateChanged;

            TerrainCallbacks.textureChanged += OnTerrainTextureChanged;
#endif
        }

        /// <summary>
        /// Unsubscribe from terrain editing events
        /// </summary>
        public void UnSubscribeEvents()
        {
            StormVTSettings.OnUpdateStormVTSettings -= StormSettingsUpdated;
#if UNITY_EDITOR
            EditorApplication.playModeStateChanged -= OnPlaymodeStateChanged;

            TerrainCallbacks.textureChanged -= OnTerrainTextureChanged;
#endif
        }

        /// <summary>
        /// Swaps the material based on the toggled state of the Storm Terrain
        /// </summary>
        public void StormSettingsUpdated()
        {
            if (Application.isPlaying && stormVTSettings.m_stormTerrainActive)
            {
                if (stormVTSettings.m_toggleTerrain)
                {
                    terrain.materialTemplate = virtualTerrainMat;
                }

                else
                {
                    terrain.materialTemplate = stormVTSettings.m_stormTerrainMaterial;
                }

            }

        }

        /// <summary>
        /// Called when a terrain is painted on
        /// </summary>
        /// <param name="terrain">Terrain was that painted on</param>
        /// <param name="textureName">Name of the terrain</param>
        /// <param name="texelRegion">Area in which the painting occurred</param>
        /// <param name="synched">Whether or not the changes were synced</param>
        private void OnTerrainTextureChanged(UnityEngine.Terrain terrain, string textureName, RectInt texelRegion, bool synched)
        {
#if UNITY_EDITOR
            UpdateTerrainLayers();
#endif
        }

#if UNITY_EDITOR
        /// <summary>
        /// Called when the application changes playmode state
        /// </summary>
        /// <param name="change"></param>
        private void OnPlaymodeStateChanged(PlayModeStateChange change)
        {
            if(change == PlayModeStateChange.EnteredEditMode)
            {
                UpdateTerrainLayers();
                if (terrain != null)
                {
                    terrain.drawHeightmap = true;
                }
            }
            if(change == PlayModeStateChange.EnteredPlayMode)
            {
                if(stormVTSettings.m_stormTerrainActive)
                {
                    if(terrain != null)
                        terrain.materialTemplate = stormVTSettings.m_stormTerrainMaterial;
                }

                if(stormVTSettings.m_disableGaiaTerrainLoadedScenes)
                {
                    if(terrain != null)
                        terrain.drawHeightmap = false;
                }
            }
        }
#endif
        /// <summary>
        /// Add the terrain to the terrain manager list
        /// </summary>
        private void OnEnable()
        {

            if (StormVTTerrainManager.Instance == null)
            {
                return;
            }

            stormVTSettings = StormVTTerrainManager.Instance.GetSettings();

            if(terrain != null)
            {
                heightmap = terrain.terrainData.heightmapTexture;
                holesmap = terrain.terrainData.holesTexture;
                splatmaps = terrain.terrainData.alphamapTextures;
            }

            UpdateTerrainLayers();
            SubscribeEvents();

            managerInstance = StormVTTerrainManager.Instance;
            if (managerInstance != null)
            {
                managerInstance.AddTerrain(this);
            }

            if (Application.isPlaying && stormVTSettings.m_stormTerrainActive)
            {
                terrain.materialTemplate = stormVTSettings.m_stormTerrainMaterial;

            }

        }

        /// <summary>
        /// Unsubscribe from events when this component is destroyed, reset terrain drawing ability
        /// </summary>
        private void OnDisable()
        {
            UnSubscribeEvents();

            if (managerInstance != null)
                managerInstance.RemoveTerrain(this);

            if(stormVTSettings != null)
            {
                if (stormVTSettings.m_stormVTMode == StormVTEnums.StormVTMode.Clipmap)
                    terrain.drawHeightmap = true;
            }

        }

       
        /// <summary>
        /// Setup correct terrain to display Storm Terrain rather than Unity terrain
        /// </summary>
        void Start()
        {
            if (StormVTTerrainManager.Instance == null)
            {
                return;
            }

            stormVTSettings = StormVTTerrainManager.Instance.GetSettings();

            if (Application.isPlaying && stormVTSettings.m_stormTerrainActive)
            {
                if(terrain != null)
                    terrain.materialTemplate = stormVTSettings.m_stormTerrainMaterial;

                if(stormVTSettings.m_stormVTMode == StormVTMode.Clipmap)
                {
                    terrain.drawHeightmap = false;
                }    

            }

            if (terrain != null)
            {
                heightmap = terrain.terrainData.heightmapTexture;
                holesmap = terrain.terrainData.holesTexture;
                splatmaps = terrain.terrainData.alphamapTextures;
            }

        }


        #region Edit Mode Terrain Variables
        /// <summary>
        /// Number of terrain layers
        /// </summary>
        int layerCount = 0;

        /// <summary>
        /// Number of splatmaps used
        /// </summary>
        int splatCount = 0;

        /// <summary>
        /// Shader Property ID - The tiling of the terrain layer 
        /// </summary>
        public static int[] _Splat_ST;

        /// <summary>
        /// Shader Property ID - Metallic value
        /// </summary>
        public static int[] _Metallic;

        /// <summary>
        /// Shader Property ID - Smoothness value
        /// </summary>
        public static int[] _Smoothness;

        /// <summary>
        /// Shader Property ID - Normal scale
        /// </summary>
        public static int[] _NormalScale;

        /// <summary>
        /// Shader Property ID - Diffuse remap scale
        /// </summary>
        public static int[] _DiffuseRemapScale;

        /// <summary>
        /// Shader Property ID - MaskMapRemapOffset
        /// </summary>
        public static int[] _MaskMapRemapOffset;

        /// <summary>
        /// Shader Property ID - MaskMapRemapScale
        /// </summary>
        public static int[] _MaskMapRemapScale;

        /// <summary>
        /// Shader Property ID - Whether or not the layer has a mask map texture
        /// </summary>
        public static int[] _LayerHasMask;

        /// <summary>
        /// Shader Property ID - Layer Diffuse map reference
        /// </summary>
        public static int[] _Splat;

        /// <summary>
        /// Shader Property ID - Layer Normal map reference
        /// </summary>
        public static int[] _Normal;

        /// <summary>
        /// Shader Property ID - Layer Mask Map reference
        /// </summary>
        public static int[] _Mask;

        /// <summary>
        /// Terrain splatmap reference
        /// </summary>
        public static int[] _Control;

        /// <summary>
        /// Terrain splatmap texelsize
        /// </summary>
        public static int[] _Control_TexelSize;

        /// <summary>
        /// Shader Property ID - Number of supported layers
        /// </summary>
        public static readonly int m_stormVT_LayerCount = Shader.PropertyToID("_LAYERCOUNT");

        /// <summary>
        /// Keyword Property ID - Number of supported layers
        /// </summary>
        public static readonly string m_stormVT_LayerCountOn = "_LAYERCOUNT";

        /// <summary>
        /// Keyword Enum values - supported number of layers
        /// </summary>
        public static readonly string[] m_stormVT_LayerCountKeywords = { "_LAYERCOUNT_FOUR", "_LAYERCOUNT_EIGHT", "_LAYERCOUNT_TWELVE", "_LAYERCOUNT_SIXTEEN" };
        #endregion

        /// <summary>
        /// Sets terrain layer data on the editor material, to support more than 8 terrain layers
        /// </summary>
        public void UpdateTerrainLayers()
        {
            
            if(terrain != null)
            {
                TerrainLayer[] terrainLayers = terrain.terrainData.terrainLayers;

                layerCount = terrainLayers.Length;
                splatCount = (int)math.ceil((float)layerCount / 4.0f);
                UpdateShaderIDs();

                terrain.materialTemplate = virtualTerrainMat;

                for (int i = 0; i < splatCount; i++)
                {
                    virtualTerrainMat.SetTexture(_Control[i], terrain.terrainData.alphamapTextures[i]);
                    virtualTerrainMat.SetVector(_Control_TexelSize[i], new Vector4(
                        1.0f / terrain.terrainData.alphamapTextures[i].width,
                        1.0f / terrain.terrainData.alphamapTextures[i].height,
                        terrain.terrainData.alphamapTextures[i].width,
                        terrain.terrainData.alphamapTextures[i].height));
                }

                //Update Shader Data
                for (int i = 0; i < layerCount; i++)
                {
                    if( i < terrainLayers.Length)
                    {
                        if (terrainLayers[i] != null)
                        {
                            //Set Textures
                            if (terrainLayers[i].diffuseTexture != null)
                                virtualTerrainMat.SetTexture(_Splat[i], terrainLayers[i].diffuseTexture);
                            if (terrainLayers[i].normalMapTexture != null)
                                virtualTerrainMat.SetTexture(_Normal[i], terrainLayers[i].normalMapTexture);
                            if (terrainLayers[i].maskMapTexture != null)
                                virtualTerrainMat.SetTexture(_Mask[i], terrainLayers[i].maskMapTexture);

                            virtualTerrainMat.SetVector(_Splat_ST[i], new Vector4(terrain.terrainData.size.x / terrainLayers[i].tileSize.x, terrain.terrainData.size.z / terrainLayers[i].tileSize.y, terrainLayers[i].tileOffset.x, terrainLayers[i].tileOffset.y));
                            virtualTerrainMat.SetFloat(_Metallic[i], terrainLayers[i].metallic);
                            virtualTerrainMat.SetFloat(_Smoothness[i], terrainLayers[i].smoothness);
                            virtualTerrainMat.SetFloat(_NormalScale[i], terrainLayers[i].normalScale);
                            virtualTerrainMat.SetVector(_DiffuseRemapScale[i], terrainLayers[i].diffuseRemapMax);
                            virtualTerrainMat.SetVector(_MaskMapRemapOffset[i], terrainLayers[i].maskMapRemapMin);
                            virtualTerrainMat.SetVector(_MaskMapRemapScale[i], terrainLayers[i].maskMapRemapMax);
                            virtualTerrainMat.SetFloat(_LayerHasMask[i], terrainLayers[i].maskMapTexture != null ? 1.0f : 0.0f);
                        }
                        
                    }

                }

                SetLayerCount(virtualTerrainMat);
            }
            

        }

        /// <summary>
        /// Changes the keyword on the editor material to allow the proper number of terrain layers
        /// </summary>
        /// <param name="material">Material to set the keyword on</param>
        void SetLayerCount(Material material)
        {
            material.SetInt(m_stormVT_LayerCount, (splatCount - 1));

            int currentAmount = splatCount - 1;

            if(currentAmount >= 0)
            {
                //Enable keyword
                material.EnableKeyword(m_stormVT_LayerCountKeywords[currentAmount]);

                //Disable the other keywords
                for (int i = 0; i < 4; i++)
                {
                    if (i != currentAmount)
                    {
                        material.DisableKeyword(m_stormVT_LayerCountKeywords[i]);
                    }
                }
            }
            

        }

        /// <summary>
        /// Sets the terrain layer variables on the editor material
        /// </summary>
        public void UpdateShaderIDs()
        {
            _Splat_ST = new int[layerCount];
            _Metallic = new int[layerCount];
            _Smoothness = new int[layerCount];
            _NormalScale = new int[layerCount];
            _DiffuseRemapScale = new int[layerCount];
            _MaskMapRemapOffset = new int[layerCount];
            _MaskMapRemapScale = new int[layerCount];
            _LayerHasMask = new int[layerCount];

            _Splat = new int[layerCount];
            _Normal = new int[layerCount];
            _Mask = new int[layerCount];

            _Control = new int[splatCount];
            _Control_TexelSize = new int[splatCount];

            for (int i = 0; i < layerCount; i++)
            {
                _Splat_ST[i] = Shader.PropertyToID($"_Splat{i}_ST");
                _Metallic[i] = Shader.PropertyToID($"_Metallic{i}");
                _Smoothness[i] = Shader.PropertyToID($"_Smoothness{i}");
                _NormalScale[i] = Shader.PropertyToID($"_NormalScale{i}");
                _DiffuseRemapScale[i] = Shader.PropertyToID($"_DiffuseRemapScale{i}");
                _MaskMapRemapOffset[i] = Shader.PropertyToID($"_MaskMapRemapOffset{i}");
                _MaskMapRemapScale[i] = Shader.PropertyToID($"_MaskMapRemapScale{i}");
                _LayerHasMask[i] = Shader.PropertyToID($"_LayerHasMask{i}");

                _Splat[i] = Shader.PropertyToID($"_Splat{i}");
                _Normal[i] = Shader.PropertyToID($"_Normal{i}");
                _Mask[i] = Shader.PropertyToID($"_Mask{i}");
            }

            for (int i = 0; i < splatCount; i++)
            {
                _Control[i] = Shader.PropertyToID($"_Control{i}");
                _Control_TexelSize[i] = Shader.PropertyToID($"_Control{i}_TexelSize");
            }
        }


    }
}
