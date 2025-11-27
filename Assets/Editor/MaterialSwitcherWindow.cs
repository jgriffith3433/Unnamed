using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Rendering;

namespace CustomEditorTools
{
    /// <summary>
    /// Advanced Material Switcher for converting materials to URP with proper texture remapping
    /// </summary>
    public class MaterialSwitcherWindow : EditorWindow
    {
        private Vector2 scrollPosition;
        private List<MaterialInfo> materialsInScene = new List<MaterialInfo>();
        private bool showOnlyNonURP = true;
        private string searchFilter = "";
        private bool isScanning = false;

        private class MaterialInfo
        {
            public Material material;
            public List<GameObject> gameObjects = new List<GameObject>();
            public bool isURP;
            public string shaderName;
            public int rendererCount;
        }

        [MenuItem("Tools/Material Switcher")]
        public static void ShowWindow()
        {
            var window = GetWindow<MaterialSwitcherWindow>("Material Switcher");
            window.minSize = new Vector2(600, 400);
            window.Show();
        }

        private void OnEnable()
        {
            ScanScene();
        }

        private void OnGUI()
        {
            EditorGUILayout.BeginVertical("box");
            DrawHeader();
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(5);

            EditorGUILayout.BeginHorizontal();
            showOnlyNonURP = EditorGUILayout.ToggleLeft("Show Only Non-URP Materials", showOnlyNonURP, GUILayout.Width(200));
            
            GUILayout.FlexibleSpace();
            
            if (GUILayout.Button("Refresh Scene", GUILayout.Width(120)))
            {
                ScanScene();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(5);

            searchFilter = EditorGUILayout.TextField("Search:", searchFilter);

            EditorGUILayout.Space(5);

            DrawMaterialsList();

            EditorGUILayout.Space(5);

            DrawFooter();
        }

        private void DrawHeader()
        {
            EditorGUILayout.LabelField("Material Switcher for URP", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Scan and convert materials to URP Lit with proper texture remapping", EditorStyles.miniLabel);
            
            if (isScanning)
            {
                EditorGUILayout.HelpBox("Scanning scene...", MessageType.Info);
            }
            else
            {
                var nonURPCount = materialsInScene.Count(m => !m.isURP);
                if (nonURPCount > 0)
                {
                    EditorGUILayout.HelpBox($"Found {nonURPCount} non-URP materials that need conversion", MessageType.Warning);
                }
                else if (materialsInScene.Count > 0)
                {
                    EditorGUILayout.HelpBox("All materials are using URP shaders!", MessageType.Info);
                }
            }
        }

        private void DrawMaterialsList()
        {
            var filteredMaterials = materialsInScene
                .Where(m => (!showOnlyNonURP || !m.isURP) && 
                           (string.IsNullOrEmpty(searchFilter) || 
                            m.material.name.ToLower().Contains(searchFilter.ToLower())))
                .ToList();

            if (filteredMaterials.Count == 0)
            {
                EditorGUILayout.HelpBox("No materials found matching the current filter.", MessageType.Info);
                return;
            }

            EditorGUILayout.LabelField($"Materials Found: {filteredMaterials.Count}", EditorStyles.boldLabel);

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.ExpandHeight(true));

            foreach (var matInfo in filteredMaterials)
            {
                DrawMaterialEntry(matInfo);
            }

            EditorGUILayout.EndScrollView();
        }

        private void DrawMaterialEntry(MaterialInfo matInfo)
        {
            EditorGUILayout.BeginVertical("box");
            
            EditorGUILayout.BeginHorizontal();
            
            // Material icon and name
            EditorGUILayout.ObjectField(matInfo.material, typeof(Material), false, GUILayout.Width(200));
            
            // Shader info
            var statusColor = matInfo.isURP ? Color.green : Color.yellow;
            var previousColor = GUI.color;
            GUI.color = statusColor;
            EditorGUILayout.LabelField(matInfo.shaderName, EditorStyles.miniLabel, GUILayout.Width(150));
            GUI.color = previousColor;
            
            // Usage count
            EditorGUILayout.LabelField($"Used by {matInfo.rendererCount} renderer(s)", GUILayout.Width(130));
            
            GUILayout.FlexibleSpace();
            
            // Action buttons
            if (!matInfo.isURP)
            {
                if (GUILayout.Button("Fix to URP Lit", GUILayout.Width(100)))
                {
                    ConvertMaterialToURP(matInfo.material, false);
                    ScanScene();
                }
                
                if (GUILayout.Button("Fix to Transparent", GUILayout.Width(120)))
                {
                    ConvertMaterialToURP(matInfo.material, true);
                    ScanScene();
                }
            }
            else
            {
                if (GUILayout.Button("To Transparent", GUILayout.Width(120)))
                {
                    ConvertURPToTransparent(matInfo.material);
                    ScanScene();
                }
            }
            
            EditorGUILayout.EndHorizontal();
            
            // Show affected GameObjects
            if (matInfo.gameObjects.Count > 0)
            {
                EditorGUI.indentLevel++;
                var foldoutLabel = $"Used by {matInfo.gameObjects.Count} GameObject(s)";
                if (EditorGUILayout.Foldout(false, foldoutLabel, true))
                {
                    // Could expand to show list if needed
                }
                EditorGUI.indentLevel--;
            }
            
            EditorGUILayout.EndVertical();
        }

        private void DrawFooter()
        {
            EditorGUILayout.BeginHorizontal("box");
            
            EditorGUILayout.LabelField("Batch Operations:", EditorStyles.boldLabel, GUILayout.Width(120));
            
            var nonURPMaterials = materialsInScene.Where(m => !m.isURP).ToList();
            
            GUI.enabled = nonURPMaterials.Count > 0;
            
            if (GUILayout.Button($"Fix All to URP Lit ({nonURPMaterials.Count})", GUILayout.Height(30)))
            {
                if (EditorUtility.DisplayDialog("Batch Convert", 
                    $"Convert {nonURPMaterials.Count} materials to URP Lit?", "Yes", "Cancel"))
                {
                    ConvertAllMaterials(false);
                }
            }
            
            if (GUILayout.Button($"Fix All to Transparent ({nonURPMaterials.Count})", GUILayout.Height(30)))
            {
                if (EditorUtility.DisplayDialog("Batch Convert", 
                    $"Convert {nonURPMaterials.Count} materials to URP Lit (Transparent)?", "Yes", "Cancel"))
                {
                    ConvertAllMaterials(true);
                }
            }
            
            GUI.enabled = true;
            
            EditorGUILayout.EndHorizontal();
        }

        private void ScanScene()
        {
            isScanning = true;
            materialsInScene.Clear();

            // Find all renderers in the scene
            var allRenderers = FindObjectsOfType<Renderer>(true);
            var materialDict = new Dictionary<Material, MaterialInfo>();

            foreach (var renderer in allRenderers)
            {
                if (renderer.sharedMaterials == null) continue;

                foreach (var material in renderer.sharedMaterials)
                {
                    if (material == null) continue;

                    if (!materialDict.ContainsKey(material))
                    {
                        var matInfo = new MaterialInfo
                        {
                            material = material,
                            shaderName = material.shader != null ? material.shader.name : "None",
                            isURP = IsURPShader(material),
                            rendererCount = 0
                        };
                        materialDict[material] = matInfo;
                        materialsInScene.Add(matInfo);
                    }

                    materialDict[material].gameObjects.Add(renderer.gameObject);
                    materialDict[material].rendererCount++;
                }
            }

            // Sort by non-URP first, then alphabetically
            materialsInScene = materialsInScene
                .OrderBy(m => m.isURP)
                .ThenBy(m => m.material.name)
                .ToList();

            isScanning = false;
            Repaint();
        }

        private bool IsURPShader(Material material)
        {
            if (material == null || material.shader == null) return false;
            
            var shaderName = material.shader.name;
            return shaderName.StartsWith("Universal Render Pipeline/") || 
                   shaderName.StartsWith("URP/") ||
                   shaderName.StartsWith("Shader Graphs/");
        }

        private void ConvertMaterialToURP(Material material, bool transparent)
        {
            if (material == null) return;

            Undo.RecordObject(material, "Convert Material to URP");

            // Store all texture references before conversion
            var textureMap = ExtractTextureProperties(material);

            // Determine target shader
            var targetShader = Shader.Find(transparent ? "Universal Render Pipeline/Lit" : "Universal Render Pipeline/Lit");
            
            if (targetShader == null)
            {
                Debug.LogError("URP Lit shader not found! Make sure URP is properly installed.");
                return;
            }

            material.shader = targetShader;

            // Apply texture properties to URP material
            ApplyTexturePropertiesToURP(material, textureMap, transparent);

            // Set render mode for transparent
            if (transparent)
            {
                SetupTransparentMode(material);
            }
            else
            {
                SetupOpaqueMode(material);
            }

            EditorUtility.SetDirty(material);
            Debug.Log($"Converted '{material.name}' to URP Lit {(transparent ? "(Transparent)" : "(Opaque)")}");
        }

        private Dictionary<string, object> ExtractTextureProperties(Material material)
        {
            var textureMap = new Dictionary<string, object>();

            // Common texture properties to preserve
            var propertyMappings = new Dictionary<string, string[]>
            {
                // Standard shader -> URP mappings
                { "_MainTex", new[] { "_MainTex", "_BaseMap", "Albedo", "Diffuse" } },
                { "_BumpMap", new[] { "_BumpMap", "_NormalMap" } },
                { "_MetallicGlossMap", new[] { "_MetallicGlossMap", "_MetallicMap" } },
                { "_OcclusionMap", new[] { "_OcclusionMap" } },
                { "_EmissionMap", new[] { "_EmissionMap" } },
                { "_ParallaxMap", new[] { "_ParallaxMap", "_HeightMap" } },
                { "_DetailMask", new[] { "_DetailMask" } },
                { "_DetailAlbedoMap", new[] { "_DetailAlbedoMap" } },
                { "_DetailNormalMap", new[] { "_DetailNormalMap" } }
            };

            // Extract textures
            foreach (var mapping in propertyMappings)
            {
                foreach (var possibleProperty in mapping.Value)
                {
                    if (material.HasProperty(possibleProperty))
                    {
                        var texture = material.GetTexture(possibleProperty);
                        if (texture != null)
                        {
                            textureMap[$"tex_{mapping.Key}"] = texture;
                            
                            // Also store texture scale and offset
                            if (material.HasProperty(possibleProperty))
                            {
                                textureMap[$"scale_{mapping.Key}"] = material.GetTextureScale(possibleProperty);
                                textureMap[$"offset_{mapping.Key}"] = material.GetTextureOffset(possibleProperty);
                            }
                            break;
                        }
                    }
                }
            }

            // Extract color properties
            var colorMappings = new Dictionary<string, string[]>
            {
                { "_Color", new[] { "_Color", "_BaseColor", "_MainColor" } },
                { "_EmissionColor", new[] { "_EmissionColor" } },
                { "_SpecColor", new[] { "_SpecColor" } }
            };

            foreach (var mapping in colorMappings)
            {
                foreach (var possibleProperty in mapping.Value)
                {
                    if (material.HasProperty(possibleProperty))
                    {
                        var color = material.GetColor(possibleProperty);
                        textureMap[$"color_{mapping.Key}"] = color;
                        break;
                    }
                }
            }

            // Extract float properties
            var floatMappings = new Dictionary<string, string[]>
            {
                { "_Metallic", new[] { "_Metallic" } },
                { "_Glossiness", new[] { "_Glossiness", "_Smoothness" } },
                { "_BumpScale", new[] { "_BumpScale", "_NormalScale" } },
                { "_OcclusionStrength", new[] { "_OcclusionStrength" } },
                { "_Parallax", new[] { "_Parallax" } },
                { "_Cutoff", new[] { "_Cutoff" } }
            };

            foreach (var mapping in floatMappings)
            {
                foreach (var possibleProperty in mapping.Value)
                {
                    if (material.HasProperty(possibleProperty))
                    {
                        var value = material.GetFloat(possibleProperty);
                        textureMap[$"float_{mapping.Key}"] = value;
                        break;
                    }
                }
            }

            return textureMap;
        }

        private void ApplyTexturePropertiesToURP(Material material, Dictionary<string, object> textureMap, bool transparent)
        {
            // Apply Base Map (Albedo)
            if (textureMap.ContainsKey("tex__MainTex"))
            {
                material.SetTexture("_BaseMap", (Texture)textureMap["tex__MainTex"]);
                if (textureMap.ContainsKey("scale__MainTex"))
                    material.SetTextureScale("_BaseMap", (Vector2)textureMap["scale__MainTex"]);
                if (textureMap.ContainsKey("offset__MainTex"))
                    material.SetTextureOffset("_BaseMap", (Vector2)textureMap["offset__MainTex"]);
            }

            // Apply Base Color
            if (textureMap.ContainsKey("color__Color"))
            {
                material.SetColor("_BaseColor", (Color)textureMap["color__Color"]);
            }

            // Apply Normal Map
            if (textureMap.ContainsKey("tex__BumpMap"))
            {
                material.SetTexture("_BumpMap", (Texture)textureMap["tex__BumpMap"]);
                if (textureMap.ContainsKey("scale__BumpMap"))
                    material.SetTextureScale("_BumpMap", (Vector2)textureMap["scale__BumpMap"]);
                if (textureMap.ContainsKey("offset__BumpMap"))
                    material.SetTextureOffset("_BumpMap", (Vector2)textureMap["offset__BumpMap"]);
            }

            if (textureMap.ContainsKey("float__BumpScale"))
            {
                material.SetFloat("_BumpScale", (float)textureMap["float__BumpScale"]);
            }

            // Apply Metallic Map
            if (textureMap.ContainsKey("tex__MetallicGlossMap"))
            {
                material.SetTexture("_MetallicGlossMap", (Texture)textureMap["tex__MetallicGlossMap"]);
            }

            // Apply Metallic and Smoothness values
            if (textureMap.ContainsKey("float__Metallic"))
            {
                material.SetFloat("_Metallic", (float)textureMap["float__Metallic"]);
            }

            if (textureMap.ContainsKey("float__Glossiness"))
            {
                material.SetFloat("_Smoothness", (float)textureMap["float__Glossiness"]);
            }

            // Apply Occlusion Map
            if (textureMap.ContainsKey("tex__OcclusionMap"))
            {
                material.SetTexture("_OcclusionMap", (Texture)textureMap["tex__OcclusionMap"]);
            }

            if (textureMap.ContainsKey("float__OcclusionStrength"))
            {
                material.SetFloat("_OcclusionStrength", (float)textureMap["float__OcclusionStrength"]);
            }

            // Apply Emission
            if (textureMap.ContainsKey("tex__EmissionMap"))
            {
                material.SetTexture("_EmissionMap", (Texture)textureMap["tex__EmissionMap"]);
                material.EnableKeyword("_EMISSION");
            }

            if (textureMap.ContainsKey("color__EmissionColor"))
            {
                material.SetColor("_EmissionColor", (Color)textureMap["color__EmissionColor"]);
                if (((Color)textureMap["color__EmissionColor"]).maxColorComponent > 0)
                {
                    material.EnableKeyword("_EMISSION");
                }
            }

            // Apply Detail Maps if present
            if (textureMap.ContainsKey("tex__DetailMask"))
            {
                material.SetTexture("_DetailMask", (Texture)textureMap["tex__DetailMask"]);
            }

            if (textureMap.ContainsKey("tex__DetailAlbedoMap"))
            {
                material.SetTexture("_DetailAlbedoMap", (Texture)textureMap["tex__DetailAlbedoMap"]);
            }

            if (textureMap.ContainsKey("tex__DetailNormalMap"))
            {
                material.SetTexture("_DetailNormalMap", (Texture)textureMap["tex__DetailNormalMap"]);
            }
        }

        private void SetupOpaqueMode(Material material)
        {
            // Set Surface to Opaque
            material.SetFloat("_Surface", 0); // 0 = Opaque
            material.SetFloat("_Blend", 0);
            
            // Disable blending
            material.SetInt("_SrcBlend", (int)BlendMode.One);
            material.SetInt("_DstBlend", (int)BlendMode.Zero);
            material.SetInt("_ZWrite", 1);
            material.DisableKeyword("_SURFACE_TYPE_TRANSPARENT");
            material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            material.DisableKeyword("_ALPHATEST_ON");
            
            // Set render queue
            material.renderQueue = (int)RenderQueue.Geometry;
            
            material.SetShaderPassEnabled("ShadowCaster", true);
        }

        private void SetupTransparentMode(Material material)
        {
            // Set Surface to Transparent
            material.SetFloat("_Surface", 1); // 1 = Transparent
            material.SetFloat("_Blend", 0); // Alpha blending
            
            // Enable alpha blending
            material.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha);
            material.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
            material.SetInt("_ZWrite", 0);
            material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            material.EnableKeyword("_ALPHAPREMULTIPLY_ON");
            
            // Set render queue
            material.renderQueue = (int)RenderQueue.Transparent;
            
            material.SetShaderPassEnabled("ShadowCaster", true);
        }

        private void ConvertURPToTransparent(Material material)
        {
            if (material == null) return;

            Undo.RecordObject(material, "Convert URP Material to Transparent");
            SetupTransparentMode(material);
            EditorUtility.SetDirty(material);
            
            Debug.Log($"Converted '{material.name}' to Transparent mode");
        }

        private void ConvertAllMaterials(bool transparent)
        {
            var nonURPMaterials = materialsInScene.Where(m => !m.isURP).ToList();
            
            int successCount = 0;
            int failCount = 0;

            for (int i = 0; i < nonURPMaterials.Count; i++)
            {
                var matInfo = nonURPMaterials[i];
                
                EditorUtility.DisplayProgressBar("Converting Materials", 
                    $"Converting {matInfo.material.name}...", 
                    (float)i / nonURPMaterials.Count);

                try
                {
                    ConvertMaterialToURP(matInfo.material, transparent);
                    successCount++;
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"Failed to convert {matInfo.material.name}: {e.Message}");
                    failCount++;
                }
            }

            EditorUtility.ClearProgressBar();

            ScanScene();

            EditorUtility.DisplayDialog("Batch Conversion Complete", 
                $"Successfully converted {successCount} materials.\n{(failCount > 0 ? $"Failed: {failCount}" : "")}", 
                "OK");
        }
    }
}
