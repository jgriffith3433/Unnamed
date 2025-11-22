using UnityEngine;

#if PW_STORM_HDRP
using UnityEngine.Rendering.HighDefinition;
#endif

#if PW_STORM_URP
using UnityEngine.Rendering.Universal;
#endif

namespace ProceduralWorlds.Storm.Terrain
{
    public class StormVTDecal : MonoBehaviour
    {
        #region Decal Properties

        /// <summary>
        /// Type of the decal (Decal, Mesh, Road)
        /// </summary>
        public StormVTEnums.StormVTDecalType m_decalType = StormVTEnums.StormVTDecalType._Mesh;


#if PW_STORM_HDRP
        /// <summary>
        /// Reference to the decal projector
        /// </summary>
        [HideInInspector]
        public UnityEngine.Rendering.HighDefinition.DecalProjector m_decalProjector;
#elif PW_STORM_URP
        /// <summary>
        /// Reference to the decal projector
        /// </summary>
        [HideInInspector]
        public UnityEngine.Rendering.Universal.DecalProjector m_decalProjector;
#endif

        /// <summary>
        /// Reference to the material to retrieve texture data from.
        /// This could be through the MeshRenderer or DecalProjector component, depending on type
        /// </summary>
        public Material m_decalMaterialReference;

        private Texture2D m_defaultMaskMap;

        [Header("Decal Properties")]

        /// <summary>
        /// Diffuse map retrieved from material reference
        /// </summary>
        public Texture2D m_DiffuseMap;

        /// <summary>
        /// Normal map retrieved from material reference
        /// </summary>
        public Texture2D m_NormalMap;

        /// <summary>
        /// Mask map retrieved from material reference
        /// </summary>
        public Texture2D m_MaskMap;

        /// <summary>
        /// Reference to the Mesh to use to draw into the render texture
        /// </summary>
        [HideInInspector]
        public Mesh m_Mesh;

        /// <summary>
        /// Reference to the MeshFilter component, used for assigning the Mesh correctly
        /// </summary>
        [HideInInspector]
        public MeshFilter m_MeshFilter;

        /// <summary>
        /// Whether to use the Diffuse Map's alpha channel as opacity
        /// </summary>
        public bool m_AffectAlpha = true;

        /// <summary>
        /// Base color from the material reference
        /// </summary>
        private Vector4 m_baseColor = Vector4.zero;

        private Vector4 m_baseMapTiling;

        /// <summary>
        /// Overall displacement value of the decal
        /// A value of 1 matches StormVTSettings.DisplacmentAmount
        /// </summary>
        [Range(0f, 1f)]
        public float m_baseHeight = 1f;

        /// <summary>
        /// Influences the heightmap intensity of the decal
        /// </summary>
        [Range(0.0f, 1.0f)]
        public float m_heightMultiplier = 1f;

        /// <summary>
        /// The opacity of the decal to draw on the terrain
        /// </summary>
        [Range(0.0f, 1.0f)]
        public float m_fadeFactor = 1.0f;

        [Header("Road Properties")]
        /// <summary>
        /// Transition phase of blending the road into the terrain
        /// </summary>
        [Range(0f, 10f)]
        public float m_roadBlendRange = 2;

        /// <summary>
        /// Width of the road to blend against the terrain
        /// </summary>
        public float m_roadBlendWidth = 1f;

        /// <summary>
        /// Height blend contrast amount
        /// </summary>
        [Range(0.001f, 1f)]
        public float m_roadBlendContrast = 0.5f;

        [Header("Rendering Properties")]

        /// <summary>
        /// Reference to the Decal material, used for drawing into the render texture
        /// </summary>
        [HideInInspector]
        public Material m_decalMaterial;

        /// <summary>
        /// Reference to the Renderer component
        /// </summary>
        [HideInInspector]
        public Renderer m_renderer;

        /// <summary>
        /// Reference to the decal's transform
        /// </summary>
        [HideInInspector]
        public Matrix4x4 m_transform;

        /// <summary>
        /// World Y position pivot location used to sort decal order
        /// </summary>
        [HideInInspector]
        public float m_pivot;
        #endregion

        #region Decal Shader Property IDs
        //Static shader property IDs for faster material assignments
        private static readonly int m_DisplacementBaseHeight = Shader.PropertyToID("_DisplacementHeight");
        private static readonly int m_DisplacementMultiplierShaderID = Shader.PropertyToID("_DisplacementMultiplier");
        private static readonly int m_FadeFactorShaderID = Shader.PropertyToID("_FadeFactor");
        private static readonly int m_TerrainScaleShaderID = Shader.PropertyToID("_TerrainScale");
        private static readonly int m_DecalTypeShaderID = Shader.PropertyToID("_DecalType");

        private static readonly int m_RoadBlendRangeShaderID = Shader.PropertyToID("_RoadBlendRange");
        private static readonly int m_RoadBlendWidthShaderID = Shader.PropertyToID("_RoadBlendWidth");
        private static readonly int m_RoadBlendContrastShaderID = Shader.PropertyToID("_RoadBlendContrast");

        private static readonly int m_BaseColorMapShaderID = Shader.PropertyToID("_BaseColorMap");
        private static readonly int m_BaseColorShaderID = Shader.PropertyToID("_BaseColor");
        private static readonly int m_BaseColorMapSTShaderID = Shader.PropertyToID("_BaseColorMap_ST");
        private static readonly int m_NormalMapShaderID = Shader.PropertyToID("_NormalMap");
        private static readonly int m_MaskMapShaderID = Shader.PropertyToID("_MaskMap");
        #endregion

        #region Events
        /// <summary>
        /// Events to broadcast when a decal's setting is updated
        /// </summary>
        public delegate void UpdateStormVTDecalSettings();
        public static event UpdateStormVTDecalSettings OnUpdateStormVTDecalSettings;
        #endregion


        /// <summary>
        /// Reference to the decal manager instance, for drawing decals to the terrain
        /// </summary>
        [HideInInspector]
        public StormVTDecalManager m_managerInstance;

        /// <summary>
        /// Setup decal data, and add it to the decal manager
        /// </summary>
        public void OnEnable()
        {
            //Store a reference to the manager instance
            m_managerInstance = StormVTDecalManager.Instance;
            //Find a projector component if we have one
            FindDecalProjector();
            //Update the material settings
            UpdateDecalSettings();
            //Add this decal into the managed list
            if (m_managerInstance != null)
                m_managerInstance.AddDecal(this);


        }

        public void OnValidate()
        {
            FindDecalProjector();
            UpdateMaterial();
        }

        private void OnDestroy()
        {
            if(m_managerInstance != null)
                m_managerInstance.RemoveDecal(this);
        }

        private void OnDisable()
        {
            if(m_managerInstance != null)
                m_managerInstance.RemoveDecal(this);
        }

        /// <summary>
        /// Finds the decal projector if this gameobject has any
        /// </summary>
        public void FindDecalProjector()
        {

#if PW_STORM_HDRP
            m_decalProjector = null;
            m_decalProjector = GetComponent<UnityEngine.Rendering.HighDefinition.DecalProjector>();
            if(m_decalProjector != null)
                m_decalType = StormVTEnums.StormVTDecalType._Decal;
#elif PW_STORM_URP
            m_decalProjector = null;
            m_decalProjector = GetComponent<UnityEngine.Rendering.Universal.DecalProjector>();
            if(m_decalProjector != null)
                m_decalType = StormVTEnums.StormVTDecalType._Decal;
#endif
        }

        /// <summary>
        /// Force an update on the decal properties
        /// </summary>
        public void UpdateDecalSettings()
        {
            //Grab the decal properties
            switch(m_decalType)
            {
                case StormVTEnums.StormVTDecalType._Decal:
                MatchDecalProjector();
                break;
                case StormVTEnums.StormVTDecalType._Mesh:
                MatchMesh();
                break;
                case StormVTEnums.StormVTDecalType._Road:
                MatchMesh();
                break;
            }
        }

        /// <summary>
        /// Fill data for the projector option
        /// </summary>
        public void MatchDecalProjector()
        {
            // Create a primitive Quad
            GameObject quad = GameObject.CreatePrimitive(PrimitiveType.Quad);

            // Get the mesh from the MeshFilter
            m_Mesh = quad.GetComponent<MeshFilter>().sharedMesh;

            if(m_renderer == null)
            {
                m_renderer = gameObject.AddComponent<MeshRenderer>();
                m_renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                m_renderer.enabled = false;
            }
            else
            {
                m_renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                m_renderer.enabled = false;
            }

            if(m_MeshFilter == null)
            {
                m_MeshFilter = gameObject.AddComponent<MeshFilter>();
                m_MeshFilter.sharedMesh = m_Mesh;
            }
            else
            {
                m_MeshFilter.sharedMesh = m_Mesh;
            }



#if PW_STORM_HDRP || PW_STORM_URP
            if (m_decalProjector != null)
            {
                float projectionDepth = m_decalProjector.size.z;
                quad.transform.position = gameObject.transform.position + new Vector3(0, (gameObject.transform.localScale.z * 0.5f) * projectionDepth, 0);
                quad.transform.rotation = gameObject.transform.rotation;
                quad.transform.localScale = gameObject.transform.localScale;

                m_decalMaterialReference = m_decalProjector.material;

                m_DiffuseMap = (Texture2D)m_decalMaterialReference.GetTexture(m_BaseColorMapShaderID);
                m_NormalMap = (Texture2D)m_decalMaterialReference.GetTexture(m_NormalMapShaderID);
                m_MaskMap = (Texture2D)m_decalMaterialReference.GetTexture(m_MaskMapShaderID);
                m_baseMapTiling = m_decalMaterialReference.GetVector(m_BaseColorMapSTShaderID);
                if (m_MaskMap == null)
                {
                    m_MaskMap = m_defaultMaskMap;
                }
                m_baseColor = m_decalMaterialReference.GetColor(m_BaseColorShaderID);

                m_transform = quad.transform.localToWorldMatrix;
                m_pivot = quad.transform.position.y;
                CreateDecalMaterial();
                m_decalProjector.enabled = false;
            }
#endif

            Destroy(quad);

        }

        /// <summary>
        /// Updates the transform information for the decal projector
        /// </summary>
        public void UpdateDecalTransform()
        {
#if Storm_HRP_Installed || Storm_HRP_Installed
            float projectionDepth = m_decalProjector.size.z;
            Vector3 position = gameObject.transform.position + new Vector3(0, (gameObject.transform.localScale.z * 0.5f) * projectionDepth, 0);
            Matrix4x4 TRS = Matrix4x4.TRS(position, gameObject.transform.rotation, gameObject.transform.localScale);
            m_transform = TRS;
            m_pivot = gameObject.transform.position.y + (gameObject.transform.localScale.z * 0.5f) * projectionDepth;
#endif
        }

        /// <summary>
        /// Updates the transform information for the mesh decal option
        /// </summary>
        public void UpdateMeshTransform()
        {
            MeshFilter meshFilter = GetComponent<MeshFilter>();
            m_transform = meshFilter.transform.localToWorldMatrix;
            m_pivot = m_renderer.bounds.min.y;
        }

        /// <summary>
        /// Fill data for the mesh decal option
        /// </summary>
        public void MatchMesh()
        {
            MeshFilter meshFilter = GetComponent<MeshFilter>();
            m_renderer = GetComponent<MeshRenderer>();

            m_Mesh = meshFilter.sharedMesh;
            m_decalMaterialReference = m_renderer.sharedMaterial;

            m_DiffuseMap = (Texture2D)m_decalMaterialReference.GetTexture(m_BaseColorMapShaderID);
            m_NormalMap = (Texture2D)m_decalMaterialReference.GetTexture(m_NormalMapShaderID);
            m_MaskMap = (Texture2D)m_decalMaterialReference.GetTexture(m_MaskMapShaderID);
            m_baseMapTiling = m_decalMaterialReference.GetVector(m_BaseColorMapSTShaderID);
            if (m_MaskMap == null)
            {
                CreateDefaultMaskMap();
                m_MaskMap = m_defaultMaskMap;
            }
            m_baseColor = m_decalMaterialReference.GetColor(m_BaseColorShaderID);

            m_transform = meshFilter.transform.localToWorldMatrix;
            m_pivot = m_renderer.bounds.min.y;
            m_renderer.enabled = false;
            CreateDecalMaterial();
        }


        /// <summary>
        /// Creates the decal material used when drawing decals to the texture
        /// </summary>
        public void CreateDecalMaterial()
        {
            m_decalMaterial = new Material(StormVTDecalManager.Instance.m_decalShader);
            m_decalMaterial.SetTexture(m_BaseColorMapShaderID, m_DiffuseMap);
            m_decalMaterial.SetTexture(m_NormalMapShaderID, m_NormalMap);
            if (m_MaskMap == null)
            {
                CreateDefaultMaskMap();
                m_MaskMap = m_defaultMaskMap;
            }
            m_decalMaterial.SetTexture(m_MaskMapShaderID, m_MaskMap);
            m_decalMaterial.SetColor(m_BaseColorShaderID, m_baseColor);
            m_decalMaterial.SetVector(m_BaseColorMapSTShaderID, m_baseMapTiling);

            switch(m_decalType)
            {
                case StormVTEnums.StormVTDecalType._Decal:
                m_decalMaterial.SetInt(m_DecalTypeShaderID, 0);
                m_decalMaterial.renderQueue = 3100;
                break;
                case StormVTEnums.StormVTDecalType._Mesh:
                    if(m_AffectAlpha)
                    {
                        m_decalMaterial.SetInt(m_DecalTypeShaderID, 0);
                    }
                    else
                    {
                        m_decalMaterial.SetInt(m_DecalTypeShaderID, 1);
                    }
                        
                m_decalMaterial.renderQueue = 3000;
                break;
                case StormVTEnums.StormVTDecalType._Road:
                    m_decalMaterial.SetInt(m_DecalTypeShaderID, 2);
                    m_decalMaterial.renderQueue = 3000;
                    break;
            }

            m_decalMaterial.SetFloat(m_DisplacementMultiplierShaderID, m_heightMultiplier);
            m_decalMaterial.SetVector(m_TerrainScaleShaderID, new Vector4(StormVTDecalManager.Instance.stormVTTerrainGrid.m_terrainHeightScale, StormVTDecalManager.Instance.stormVTTerrainGrid.m_terrainHeightOffset, 0,0));
            UpdateMaterial();
        }

        public void CreateDefaultMaskMap()
        {
            m_defaultMaskMap = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            Color[] pixels = new Color[1];
            pixels[0] = new Color(0, 1, 0, 0);
            m_defaultMaskMap.SetPixels(pixels);
            m_defaultMaskMap.Apply(false, true);
        }

        /// <summary>
        /// Updates data on the decal material
        /// </summary>
        public void UpdateMaterial()
        {
            if(m_decalMaterial != null)
            {
                switch (m_decalType)
                {
                    case StormVTEnums.StormVTDecalType._Decal:
                        m_decalMaterial.SetInt(m_DecalTypeShaderID, 0);
                        m_decalMaterial.renderQueue = 3800;
                        UpdateDecalTransform();
                        break;
                    case StormVTEnums.StormVTDecalType._Mesh:
                        UpdateMeshTransform();
                        if (m_AffectAlpha)
                        {
                            m_decalMaterial.SetInt(m_DecalTypeShaderID, 0);
                        }
                        else
                        {
                            m_decalMaterial.SetInt(m_DecalTypeShaderID, 1);
                        }

                        m_decalMaterial.renderQueue = 3000;
                        break;
                    case StormVTEnums.StormVTDecalType._Road:
                        UpdateMeshTransform();
                        m_decalMaterial.SetInt(m_DecalTypeShaderID, 2);
                        m_decalMaterial.renderQueue = 3000;
                        m_decalMaterial.SetFloat(m_RoadBlendRangeShaderID, m_roadBlendRange);
                        m_decalMaterial.SetFloat(m_RoadBlendContrastShaderID, m_roadBlendContrast);
                        m_decalMaterial.SetFloat(m_RoadBlendWidthShaderID, m_roadBlendWidth);
                        break;
                }
                m_decalMaterial.SetFloat(m_DisplacementBaseHeight, m_baseHeight);
                m_decalMaterial.SetFloat(m_DisplacementMultiplierShaderID, m_heightMultiplier);
                m_decalMaterial.SetFloat(m_FadeFactorShaderID, m_fadeFactor);



                if (OnUpdateStormVTDecalSettings != null)
                    OnUpdateStormVTDecalSettings();
            }
        }
    }
}
