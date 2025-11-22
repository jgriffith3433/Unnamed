using UnityEngine;
using System;

namespace ProceduralWorlds.Storm.Utils
{
    public enum BakeShaderOutputType {
        DEFAULT,
        MASK,
        NORMAL,
    }
    public enum AlphaUpdateType {
        DEFAULT,
        SATURATE,
        FILL,
        NONE,
    }
    [Serializable]
    public struct ShaderOption<T>
    {
        public string Key;
        public T Value;
    }
    [Serializable]
    public struct PostProcessingInfo
    {
        public Shader Shader;
        public string Input;
        public string Output;
        public string OutputField;
        public BakeShaderOutputType Type;
        public AlphaUpdateType AlphaUpdate;
        public ShaderOption<string[]>[] Textures;
        public ShaderOption<bool>[] OptionsBool;
        public ShaderOption<float>[] OptionsFloat;
        public ShaderOption<int>[] OptionsInt;
        public ShaderOption<Vector4>[] m_optionsVector;
        public bool IsTemporary;
        public bool IsMainAlpha;
    }
    [Serializable]
    public struct BakeShaderInfo {
        public Shader m_shader;
        public string m_texName;
        public string m_fieldName;
        public BakeShaderOutputType m_type;
        public AlphaUpdateType m_alphaUpdate;
        public ShaderOption<string[]>[] m_optionsToCopy;
        public ShaderOption<bool>[] m_optionsBool;
        public ShaderOption<float>[] m_optionsFloat;
        public ShaderOption<int>[] m_optionsInt;
        public ShaderOption<Vector4>[] m_optionsVector;
        public bool m_isTemporary;
        public bool m_isMainAlpha;
    }
    [Serializable]
    public struct ImpostorShaderInfo
    {
        public Shader m_shader;
        public ShaderOption<string>[] m_propertyReplacements;
    }
    public enum ImpostorType {
        NONE = -1,
        FLAT_X,
        FLAT_Y,
        FLAT_Z,
        FIVE_SLICE,
        NINE_SLICE,
#if AMPLIFY_IMPOSTORS_PRESENT
        STORM_OCTAHEDRAL_HALF = 5,
        STORM_OCTAHEDRAL,
        AMPLIFY_SPHERICAL = 7,
        AMPLIFY_OCTAHEDRAL_HALF,
        AMPLIFY_OCTAHEDRAL,
#endif
    }
    [Serializable]
    public struct ImpostorTexture {
        public string m_name;
        public Texture2D m_texture;
        public BakeShaderOutputType m_type;
        public AlphaUpdateType m_alphaUpdate;
        public bool m_mainAlpha;

        public ImpostorTexture(string name, Texture2D texture, BakeShaderOutputType ty, AlphaUpdateType alphaUpdate, bool isMainAlpha) {
            m_name = name;
            m_texture = texture;
            m_type = ty;
            m_alphaUpdate = alphaUpdate;
            m_mainAlpha = isMainAlpha;
        }
    }
    public class Impostor : ScriptableObject, ISerializationCallbackReceiver {
        public GameObject m_sourcePrefab = null;
        public Mesh m_sourceMesh = null;
        public Material[] m_sourceMaterials = new Material[0];
        public int m_targetResolution = 0;

        public ImpostorTexture[] m_textures;
        public Vector3 m_scale;
        public float m_size;
        public Vector4[] m_textureCoordinates;
        [Obsolete("topDownCoordinates is no longer used. It has been moved to the last index in textureCoordinates")]
        public Vector4 m_topDownCoordinates;
        public Vector3 m_offset;
        public ImpostorType m_type;
        public int m_frameCount;

        public float m_occlusionStrength = 1f;
        public float m_metallicStrength = 0f;
        public float m_glossiness = 0f;
        public float m_alphaCutoff = 0.25f;

        public Material m_savedMaterial = null;
        public Mesh m_savedMesh = null;

        //for Amplified Impostor currently
        public ScriptableObject m_savedObject = null;

        public ShaderOption<Vector4>[] m_customVectors = new ShaderOption<Vector4>[0];
        public ShaderOption<int>[] m_customInts = new ShaderOption<int>[0];
        public ShaderOption<float>[] m_customFloats = new ShaderOption<float>[0];
        public ShaderOption<bool>[] m_customKeywords = new ShaderOption<bool>[0];
        public IndirectDrawIndexedArgs[] m_args = null;
        public Vector2[] m_meshPoints = new Vector2[0];
        public Vector4 m_meshData = Vector4.zero;

        public void OnEnable()
        {
            StormSystemSettings.OnImpostorEnabled?.Invoke(this);
        }

        public void OnAfterDeserialize()
        {
#pragma warning disable CS0618 // Type or member is obsolete
            if (m_topDownCoordinates != Vector4.zero)
            {
                int idx = m_textureCoordinates.Length;
                Array.Resize(ref m_textureCoordinates, idx + 1);
                m_textureCoordinates[idx] = m_topDownCoordinates;
                m_topDownCoordinates = Vector4.zero;
            }
#pragma warning restore CS0618 // Type or member is obsolete
        }

        public void OnBeforeSerialize()
        {
        }
    }
}