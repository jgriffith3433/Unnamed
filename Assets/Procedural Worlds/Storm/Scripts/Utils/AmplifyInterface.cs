using UnityEngine;

namespace ProceduralWorlds.Storm.Utils
{
    public class AmplifyInterface {
        /// <summary>
        /// Creates an Impostor using AmplifyImpostors
        /// </summary>
        /// <param name="Target">Transform of object to create an Impostor from</param>
        /// <param name="impostor">Reference to Impostor asset to update</param>
        /// <param name="BakePreset">Preset to use in AmplifyImpostors bake process</param>
        /// <returns>True if Impostor creation was successful</returns>
        public static bool CreateImpostor(Transform Target, Impostor impostor, ScriptableObject BakePreset = null) {
#if AMPLIFY_IMPOSTORS_PRESENT && UNITY_EDITOR
            if (Target == null)
                return false;
            bool createdTarget = false;
            if (!Target.gameObject.scene.IsValid()) {
                //not in a scene, place an object somewhere and we will delete it later
                Target = Object.Instantiate(Target, Vector3.zero, Quaternion.identity, null);
                createdTarget = true;
            }
            var amp = Target.gameObject.GetComponent<AmplifyImpostors.AmplifyImpostor>();
            bool createdAmp = false;
            if (amp == null) {
                amp = Target.gameObject.AddComponent<AmplifyImpostors.AmplifyImpostor>();
                if (amp == null)
                    return false;
                createdAmp = true;
            }
            amp.m_cutMode = AmplifyImpostors.CutMode.Automatic;
            amp.m_lodReplacement = AmplifyImpostors.LODReplacement.DoNothing;
            amp.m_impostorName = Target.name;
            amp.RootTransform = Target;
            amp.Renderers = GetRenderers(Target);
            var asset = ScriptableObject.CreateInstance<AmplifyImpostors.AmplifyImpostorAsset>();
            asset = StormUtils.SaveAsset(asset, StormUtils.DefaultSavePath + "Impostors/Amplify/", amp.m_impostorName, "asset");
            amp.Data = asset;
            asset.HorizontalFrames = impostor.m_frameCount;
            asset.VerticalFrames = impostor.m_frameCount;
            asset.LockedSizes = true;
            if (Mathf.IsPowerOfTwo(impostor.m_targetResolution))
                asset.SelectedSize = impostor.m_targetResolution;
            else
                asset.SelectedSize = Mathf.NextPowerOfTwo(impostor.m_targetResolution);
            asset.TexSize = new Vector2(asset.SelectedSize, asset.SelectedSize);
            System.Type AmplifyImpostorType = typeof(AmplifyImpostors.AmplifyImpostor);
            if (BakePreset != null && BakePreset is AmplifyImpostors.AmplifyImpostorBakePreset)
                asset.Preset = BakePreset as AmplifyImpostors.AmplifyImpostorBakePreset;
            else {
                amp.DetectRenderPipeline();
                if (amp.m_renderPipelineInUse == AmplifyImpostors.RenderPipelineInUse.HD) {
                    string HDPreset = AmplifyImpostorType.GetField("HDPreset", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static).GetValue(amp) as string;
                    asset.Preset = UnityEditor.AssetDatabase.LoadAssetAtPath<AmplifyImpostors.AmplifyImpostorBakePreset>(UnityEditor.AssetDatabase.GUIDToAssetPath(HDPreset));
                }
                else if (amp.m_renderPipelineInUse == AmplifyImpostors.RenderPipelineInUse.LW) {
                    string LWPreset = AmplifyImpostorType.GetField("LWPreset", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static).GetValue(amp) as string;
                    asset.Preset = UnityEditor.AssetDatabase.LoadAssetAtPath<AmplifyImpostors.AmplifyImpostorBakePreset>(UnityEditor.AssetDatabase.GUIDToAssetPath(LWPreset));
                }
                else if (amp.m_renderPipelineInUse == AmplifyImpostors.RenderPipelineInUse.URP) {
                    string UPreset = AmplifyImpostorType.GetField("UPreset", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static).GetValue(amp) as string;
                    asset.Preset = UnityEditor.AssetDatabase.LoadAssetAtPath<AmplifyImpostors.AmplifyImpostorBakePreset>(UnityEditor.AssetDatabase.GUIDToAssetPath(UPreset));
                }
                else {
                    string StandardPreset = AmplifyImpostorType.GetField("StandardPreset", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static).GetValue(amp) as string;
                    asset.Preset = UnityEditor.AssetDatabase.LoadAssetAtPath<AmplifyImpostors.AmplifyImpostorBakePreset>(UnityEditor.AssetDatabase.GUIDToAssetPath(StandardPreset));
                }
            }
            if (impostor.m_type == ImpostorType.AMPLIFY_OCTAHEDRAL)
                asset.ImpostorType = AmplifyImpostors.ImpostorType.Octahedron;
            else if (impostor.m_type == ImpostorType.AMPLIFY_OCTAHEDRAL_HALF)
                asset.ImpostorType = AmplifyImpostors.ImpostorType.HemiOctahedron;
            else if (impostor.m_type == ImpostorType.AMPLIFY_SPHERICAL)
                asset.ImpostorType = AmplifyImpostors.ImpostorType.Spherical;
            try {
                amp.RenderCombinedAlpha(asset);
                amp.GenerateAutomaticMesh(asset);
                amp.RenderAllDeferredGroups(asset);
            }
            catch (System.Exception e) {
                UnityEditor.EditorUtility.ClearProgressBar();
                Debug.LogWarning("Error creating AmplifyImpostor:\n" + e.Message + e.StackTrace);
            }
            impostor.name = amp.name;
            float xyFitSize = asset.Material.GetFloat("_ImpostorSize");
            float zSize = asset.Material.GetFloat("_DepthSize");
            impostor.m_offset = asset.Material.GetVector("_Offset");

            impostor.m_customFloats = new ImpostorCustomFloat[] {
                new ImpostorCustomFloat("_ImpostorSize", xyFitSize),
                new ImpostorCustomFloat("_AI_ImpostorSize", xyFitSize),
                new ImpostorCustomFloat("_DepthSize", zSize),
                new ImpostorCustomFloat("_AI_DepthSize", zSize),
            };
            impostor.m_customVectors = new ImpostorCustomVector[] {
                new ImpostorCustomVector("_Offset", impostor.m_offset),
                new ImpostorCustomVector("_AI_Offset", impostor.m_offset),
                new ImpostorCustomVector("_AI_SizeOffset", asset.Material.GetVector("_AI_SizeOffset")),
            };
            bool half = asset.ImpostorType != AmplifyImpostors.ImpostorType.HemiOctahedron;
            impostor.m_customKeywords = new ImpostorCustomBool[] {
                new ImpostorCustomBool("_HEMI_ON", half),
            };
            impostor.m_customInts = new ImpostorCustomInt[] {
                new ImpostorCustomInt("_Hemi", half ? 1 : 0),
            };

            try {
                asset.Mesh = amp.GenerateMesh(asset.ShapePoints, impostor.m_offset, xyFitSize, xyFitSize, true);
            }
            catch (System.Exception e) {
                UnityEditor.EditorUtility.ClearProgressBar();
                Debug.LogWarning("Error creating AmplifyImpostor Mesh:\n" + e.Message + e.StackTrace);
            }
            impostor.m_scale = new Vector3(xyFitSize, xyFitSize, zSize);
            impostor.m_meshPoints = asset.ShapePoints;
            impostor.m_savedMesh = asset.Mesh;
            impostor.m_savedObject = asset;
            impostor.m_savedInverseMesh = StormUtils.CreateInverse(impostor.m_savedMesh);
            impostor.m_savedMesh = StormUtils.SaveAsset(impostor.m_savedMesh, StormUtils.DefaultSavePath + "Impostors/Meshes/" + impostor.name + ".mesh");
            impostor.m_savedInverseMesh = StormUtils.SaveAsset(impostor.m_savedInverseMesh, StormUtils.DefaultSavePath + "Impostors/Meshes/" + impostor.name + "_inv.mesh");
            impostor.m_savedMaterial = asset.Material;
            //grab the textures to assign to the material in case we need to create it later.
            string[] textureProps = impostor.m_savedMaterial.GetTexturePropertyNames();
            impostor.m_textures = new ImpostorTexture[textureProps.Length];
            for (int t = 0; t < textureProps.Length; ++t)
                impostor.m_textures[t] = new ImpostorTexture(textureProps[t], impostor.m_savedMaterial.GetTexture(textureProps[t]) as Texture2D, t == asset.Preset.AlphaIndex ? BakeShaderOutputType.DEFAULT : BakeShaderOutputType.MASK, AlphaUpdateType.DEFAULT);
            impostor.m_textures = new ImpostorTexture[] {
                new ImpostorTexture("_Albedo", impostor.m_savedMaterial.GetTexture("_Albedo") as Texture2D, BakeShaderOutputType.DEFAULT, AlphaUpdateType.NONE),
                new ImpostorTexture("_Normals", impostor.m_savedMaterial.GetTexture("_Normals") as Texture2D, BakeShaderOutputType.NORMAL, AlphaUpdateType.NONE),
                new ImpostorTexture("_Specular", impostor.m_savedMaterial.GetTexture("_Specular") as Texture2D, BakeShaderOutputType.MASK, AlphaUpdateType.NONE),
                new ImpostorTexture("_Emission", impostor.m_savedMaterial.GetTexture("_Emission") as Texture2D, BakeShaderOutputType.MASK, AlphaUpdateType.NONE),
            };
            //switch out the shader for a storm version
            Shader stormShader = Shader.Find("Storm/" + impostor.m_savedMaterial.shader.name);
            if (stormShader == null) {
                string shaderPath = UnityEditor.AssetDatabase.GetAssetPath(impostor.m_savedMaterial.shader);
                if (!string.IsNullOrEmpty(shaderPath)) {
                    string newShaderPath = StormUtils.DefaultSavePath + "Shaders" + shaderPath.Substring(shaderPath.LastIndexOf("/"));
                    string newShader = StormShaderConverter.Convert(shaderPath, newShaderPath);
                    UnityEditor.AssetDatabase.Refresh();
                    stormShader = Shader.Find(newShader);
                }
            }
            if (stormShader != null)
                impostor.m_savedMaterial.shader = stormShader;
            if (amp.m_lastImpostor != null)
                if (Application.isPlaying)
                    Object.Destroy(amp.m_lastImpostor);
                else
                    Object.DestroyImmediate(amp.m_lastImpostor);
            if (createdTarget) {
                if (Application.isPlaying)
                    Object.Destroy(Target.gameObject);
                else
                    Object.DestroyImmediate(Target.gameObject);
            }
            else {
                if (createdAmp) {
                    if (Application.isPlaying)
                        Object.Destroy(amp);
                    else
                        Object.DestroyImmediate(amp);
                }
            }
            impostor.m_args = StormUtils.GetArgs(impostor.m_savedMesh);
            return true;
#else
            return false;
#endif
        }
        /// <summary>
        /// Gets Data about Impostor from AmplifyImpostors
        /// </summary>
        /// <param name="Target">Transform of object Impostor is based on</param>
        /// <param name="impostor">Reference to Impostor asset to update</param>
        /// <returns>True if Impostor Data was successfully found</returns>
        public static bool GetImpostorData(Transform Target, Impostor impostor) {
#if AMPLIFY_IMPOSTORS_PRESENT && UNITY_EDITOR
            if (Target == null)
                return false;
            var amp = Target.gameObject.GetComponent<AmplifyImpostors.AmplifyImpostor>();
            bool createdAmp = false;
            if (amp == null) {
                amp = Target.gameObject.AddComponent<AmplifyImpostors.AmplifyImpostor>();
                if (amp == null)
                    return false;
                createdAmp = true;
            }
            //setup all the basic settings
            amp.m_cutMode = AmplifyImpostors.CutMode.Automatic;
            amp.m_lodReplacement = AmplifyImpostors.LODReplacement.DoNothing;
            amp.m_impostorName = Target.name;
            amp.RootTransform = Target;
            amp.Renderers = GetRenderers(Target);
            //create the "alphaTex" texture which is a combination of all frames stacked on top of eachother)
            Texture2D mainTex = impostor.m_textures[0].m_texture;
            int tmpWidth = Mathf.CeilToInt(mainTex.width / (float)impostor.m_frameCount);
            int tmpHeight = Mathf.CeilToInt(mainTex.height / (float)impostor.m_frameCount);
            Color[] tmpPixels = new Color[tmpWidth * tmpHeight];
            for (int X = 0; X < impostor.m_frameCount; ++X) {
                for (int Y = 0; Y < impostor.m_frameCount; ++Y) {
                    Color[] mtPixels = mainTex.GetPixels(X * tmpWidth, Y * tmpHeight, tmpWidth, tmpHeight);
                    for (int p = 0; p < mtPixels.Length; ++p)
                        tmpPixels[p] = (tmpPixels[p] * tmpPixels[p].a) + (mtPixels[p] * (1f - tmpPixels[p].a));
                }
            }
            Texture2D tmpTexture = new Texture2D(tmpWidth, tmpHeight, mainTex.format, false);
            tmpTexture.SetPixels(tmpPixels);
            tmpTexture.Apply();
            amp.m_alphaTex = tmpTexture;

            AmplifyImpostors.ImpostorType impType = AmplifyImpostors.ImpostorType.Octahedron;
            if (impostor.m_type == ImpostorType.AMPLIFY_OCTAHEDRAL)
                impType = AmplifyImpostors.ImpostorType.Octahedron;
            else if (impostor.m_type == ImpostorType.AMPLIFY_OCTAHEDRAL_HALF)
                impType = AmplifyImpostors.ImpostorType.HemiOctahedron;
            else if (impostor.m_type == ImpostorType.AMPLIFY_SPHERICAL)
                impType = AmplifyImpostors.ImpostorType.Spherical;
            amp.Data = ScriptableObject.CreateInstance<AmplifyImpostors.AmplifyImpostorAsset>();
            amp.Data.HorizontalFrames = impostor.m_frameCount;
            amp.Data.VerticalFrames = impostor.m_frameCount;
            amp.Data.DecoupleAxisFrames = false;
            amp.CalculateSheetBounds(impType);
            //get calculated xzSize, depthSize, and bounds from the amplify object
            System.Type AmplifyImpostorType = typeof(AmplifyImpostors.AmplifyImpostor);
            float XYFit = (float)AmplifyImpostorType.GetField("m_xyFitSize", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(amp);
            float DFit = (float)AmplifyImpostorType.GetField("m_depthFitSize", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(amp);
            Bounds originalBounds = (Bounds)AmplifyImpostorType.GetField("m_originalBound", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(amp);
            Vector2 mins = new Vector2(impostor.m_meshData.x, impostor.m_meshData.y);
            Vector2 height = (new Vector2(impostor.m_meshData.z, impostor.m_meshData.w) - mins);
            float maxBound = Mathf.Max(height.x, height.y);
            Vector2 center = mins + (height * 0.5f);
            Vector2 pixelOffset = (center - (Vector2.one * 0.5f)) * XYFit;
            XYFit *= maxBound;
            DFit *= maxBound;

            Vector4 offset = new Vector4(originalBounds.center.x, originalBounds.center.y, originalBounds.center.z, -pixelOffset.y / XYFit );
            Vector4 sizeOffset = new Vector4(XYFit, DFit, (pixelOffset.x / XYFit) / (float)impostor.m_frameCount, -offset.w / impostor.m_frameCount);
            impostor.m_customVectors = new ImpostorCustomVector[] {
                new ImpostorCustomVector("_Offset", offset),
                new ImpostorCustomVector("_AI_Offset", offset),
                new ImpostorCustomVector("_SizeOffset", sizeOffset),
                new ImpostorCustomVector("_AI_SizeOffset", sizeOffset),
            };
            impostor.m_customFloats = new ImpostorCustomFloat[] {
                new ImpostorCustomFloat("_ImpostorSize", XYFit),
                new ImpostorCustomFloat("_AI_ImpostorSize", XYFit),
                new ImpostorCustomFloat("_DepthSize", DFit),
                new ImpostorCustomFloat("_AI_DepthSize", DFit),
            };
            impostor.m_offset = offset;
            impostor.m_meshData.x = XYFit;
            impostor.m_meshData.y = DFit;
            impostor.m_meshData.z = pixelOffset.x;
            impostor.m_meshData.w = pixelOffset.y;
            if (createdAmp) {
                if (Application.isPlaying)
                    Object.Destroy(amp);
                else
                    Object.DestroyImmediate(amp);
            }
            return true;
#else
            return false;
#endif
        }
    }
}