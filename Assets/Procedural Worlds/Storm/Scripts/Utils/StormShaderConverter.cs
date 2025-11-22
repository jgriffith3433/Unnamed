using UnityEngine;
using System;
using System.IO;
using System.IO.Compression;

/// Copyright 2023 Procedural Worlds Ltd
/// Written by Jonathan Williams as part of Storm

namespace ProceduralWorlds.Storm.Utils
{
    public class StormShaderConverter
    {
        enum ScriptType
        {
            CG,
            GLSL,
            HLSL,
        }

        public const string DefaultSaveDir = "Assets/Storm User Data/ShaderCache/";
        public const uint ConverterVersion = 6;
        const string nameInsert = "PWStorm/";
        const string surfInsert = "\nCGINCLUDE\n#pragma multi_compile_instancing\n#pragma multi_compile STORM_ENABLED\n#pragma instancing_options procedural:storm_setup\n#include \"StormIndirectBase.cginc\"\nENDCG\n";
        //why this cannot be in a .cginc file is beyond me ... but it works
        const string vertFuncs = @"
#ifndef STORM_INDIRECT_VERT
    #define STORM_INDIRECT_VERT
    struct RenderData {
        column_major float4x4 m;
        half2 d;
    };
    //source: https://github.com/glslify/glsl-inverse
    //License: 2014 Mikola Lysenko. MIT License
    float4x4 storminverse(float4x4 m) {
        float
            a00 = m[0][0], a01 = m[0][1], a02 = m[0][2], a03 = m[0][3],
            a10 = m[1][0], a11 = m[1][1], a12 = m[1][2], a13 = m[1][3],
            a20 = m[2][0], a21 = m[2][1], a22 = m[2][2], a23 = m[2][3],
            a30 = m[3][0], a31 = m[3][1], a32 = m[3][2], a33 = m[3][3],

            b00 = a00 * a11 - a01 * a10,
            b01 = a00 * a12 - a02 * a10,
            b02 = a00 * a13 - a03 * a10,
            b03 = a01 * a12 - a02 * a11,
            b04 = a01 * a13 - a03 * a11,
            b05 = a02 * a13 - a03 * a12,
            b06 = a20 * a31 - a21 * a30,
            b07 = a20 * a32 - a22 * a30,
            b08 = a20 * a33 - a23 * a30,
            b09 = a21 * a32 - a22 * a31,
            b10 = a21 * a33 - a23 * a31,
            b11 = a22 * a33 - a23 * a32,

            det = b00 * b11 - b01 * b10 + b02 * b09 + b03 * b08 - b04 * b07 + b05 * b06;

        return float4x4(
            a11 * b11 - a12 * b10 + a13 * b09,
            a02 * b10 - a01 * b11 - a03 * b09,
            a31 * b05 - a32 * b04 + a33 * b03,
            a22 * b04 - a21 * b05 - a23 * b03,
            a12 * b08 - a10 * b11 - a13 * b07,
            a00 * b11 - a02 * b08 + a03 * b07,
            a32 * b02 - a30 * b05 - a33 * b01,
            a20 * b05 - a22 * b02 + a23 * b01,
            a10 * b10 - a11 * b08 + a13 * b06,
            a01 * b08 - a00 * b10 - a03 * b06,
            a30 * b04 - a31 * b02 + a33 * b00,
            a21 * b02 - a20 * b04 - a23 * b00,
            a11 * b07 - a10 * b09 - a12 * b06,
            a00 * b09 - a01 * b07 + a02 * b06,
            a31 * b01 - a30 * b03 - a32 * b00,
            a20 * b03 - a21 * b01 + a22 * b00) / det;
    }
    StructuredBuffer<RenderData> storm_buffer;
    half2 storm_data; //Color, Fade values
    void storm_vert() {
        #if defined(STORM_ENABLED) | defined(_STORM_ENABLED)
	        //not sure why this is required to fix HDRP instancing but it is
	        #undef unity_ObjectToWorld
	        #undef unity_WorldToObject
	        #define unity_ObjectToWorld unity_ObjectToWorld
	        #define unity_WorldToObject unity_WorldToObject

	        #ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
		        unity_ObjectToWorld = storm_buffer[unity_InstanceID].m;
		        unity_WorldToObject = storminverse(unity_ObjectToWorld);
		        storm_data = storm_buffer[unity_InstanceID].d;
	        #endif
        #endif
    }
#endif
";
        const string vertInsert = @"
#pragma multi_compile_instancing
#pragma multi_compile STORM_ENABLED
#pragma instancing_options procedural:storm_vert" + vertFuncs;
        const string proceduralFunction = "storm_alt_setup";
        const string proceduralInsertPre = "\nvoid storm_alt_setup()\n{\n\t";
        const string proceduralInsertPost = "();\n\tstorm_vert();\n}";

        const string CommentStart = "\n//BEGIN STORM EDIT\n";
        const string CommentEnd = "\n//END STORM EDIT\n";
        const string indirectBaseText = @"#ifndef STORM_INDIRECT_BASE_INCLUDE
    #define STORM_INDIRECT_BASE_INCLUDE
    #ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
        struct RenderData {
	        column_major float4x4 m;
	        half2 d;
        };
        StructuredBuffer<RenderData> storm_buffer;
        half2 storm_data; //Color, Fade values
        #ifdef STORM_FRAGMENT
            float4x4 OriginalMatrix;
        #endif	
    #endif

    //source: https://github.com/glslify/glsl-inverse
    //License: 2014 Mikola Lysenko. MIT License
    float4x4 storm_inverse(float4x4 m) {
        float
            a00 = m[0][0], a01 = m[0][1], a02 = m[0][2], a03 = m[0][3],
            a10 = m[1][0], a11 = m[1][1], a12 = m[1][2], a13 = m[1][3],
            a20 = m[2][0], a21 = m[2][1], a22 = m[2][2], a23 = m[2][3],
            a30 = m[3][0], a31 = m[3][1], a32 = m[3][2], a33 = m[3][3],

            b00 = a00 * a11 - a01 * a10,
            b01 = a00 * a12 - a02 * a10,
            b02 = a00 * a13 - a03 * a10,
            b03 = a01 * a12 - a02 * a11,
            b04 = a01 * a13 - a03 * a11,
            b05 = a02 * a13 - a03 * a12,
            b06 = a20 * a31 - a21 * a30,
            b07 = a20 * a32 - a22 * a30,
            b08 = a20 * a33 - a23 * a30,
            b09 = a21 * a32 - a22 * a31,
            b10 = a21 * a33 - a23 * a31,
            b11 = a22 * a33 - a23 * a32,

            det = b00 * b11 - b01 * b10 + b02 * b09 + b03 * b08 - b04 * b07 + b05 * b06;

        return float4x4(
            a11 * b11 - a12 * b10 + a13 * b09,
            a02 * b10 - a01 * b11 - a03 * b09,
            a31 * b05 - a32 * b04 + a33 * b03,
            a22 * b04 - a21 * b05 - a23 * b03,
            a12 * b08 - a10 * b11 - a13 * b07,
            a00 * b11 - a02 * b08 + a03 * b07,
            a32 * b02 - a30 * b05 - a33 * b01,
            a20 * b05 - a22 * b02 + a23 * b01,
            a10 * b10 - a11 * b08 + a13 * b06,
            a01 * b08 - a00 * b10 - a03 * b06,
            a30 * b04 - a31 * b02 + a33 * b00,
            a21 * b02 - a20 * b04 - a23 * b00,
            a11 * b07 - a10 * b09 - a12 * b06,
            a00 * b09 - a01 * b07 + a02 * b06,
            a31 * b01 - a30 * b03 - a32 * b00,
            a20 * b03 - a21 * b01 + a22 * b00) / det;
    }

    void storm_setup()
    {
    #if defined(STORM_ENABLED) | defined(_STORM_ENABLED)
	    //not sure why this is required to fix HDRP instancing but it is
	    #undef unity_ObjectToWorld
	    #undef unity_WorldToObject
	    #define unity_ObjectToWorld unity_ObjectToWorld
	    #define unity_WorldToObject unity_WorldToObject

	    #ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
		    unity_ObjectToWorld = storm_buffer[unity_InstanceID].m;
		    unity_WorldToObject = storm_inverse(unity_ObjectToWorld);
		    storm_data = storm_buffer[unity_InstanceID].d;
	    #endif
    #endif
    }

    //Dummy function to insert Indirect Instancing Pre Processors
    void StormIndirect_float(float3 inPos, out float3 outPos)
    {
	    outPos = inPos;
    }

    void StormIndirect_half(half3 inPos, out half3 outPos)
    {
	    outPos = inPos;
    }
    void lodFade_float(float inDummy, out float outFade, out float outColor)
    {
    #ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
	    outFade = saturate(storm_data.y);
	    outColor = saturate(storm_data.x);
    #else
	    outFade = 1;
	    outColor = 1;
    #endif
    }
    void lodFade_half(float inDummy, out half outFade, out half outColor)
    {
    #ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
	    outFade = saturate(storm_data.y);
	    outColor = saturate(storm_data.x);
    #else
	    outFade = 1;
	    outColor = 1;
    #endif
    }
#endif";

        public static Shader Convert(Shader Original)
        {
            if (Original.name.StartsWith("PWStorm", StringComparison.Ordinal))
                return Original;
            string shaderPath = StormUtils.GetAssetPath(Original);
            //Debug.Log("path: " + shaderPath, Original);
            Shader ret;
            if (string.IsNullOrWhiteSpace(shaderPath) || shaderPath == "Library/unity default resources" || shaderPath == "Resources/unity_builtin_extra" || !StormUtils.IsEditor())
            {
                ret = Shader.Find("PWStorm/" + Original.name);
                //Debug.Log("found: " + ret.name, ret);
                if (ret == null)
                    ret = Original;
            }
            else
            {
                string newShaderPath = DefaultSaveDir + "Shaders/" + Path.GetFileName(shaderPath);
                string shaderName = Convert(shaderPath, newShaderPath);
                //Debug.Log(newShaderPath + " = " + shaderName);
                if (string.IsNullOrEmpty(shaderName))
                    return Original;
                ret = StormUtils.LoadAsset<Shader>(newShaderPath);
                if (ret == null)
                {
                    ret = Shader.Find(shaderName);
                    if (ret == null)
                        ret = Original;
                }
            }
            //Debug.Log("returning: " + ret.name, ret);
            return ret;
        }
        public static string Convert(string oldFilename, string newFilename)
        {
            FileInfo oldShaderInfo = new FileInfo(oldFilename);
            if (!oldShaderInfo.Exists)
            {
                return null;
            }
            oldFilename = oldFilename.Replace('\\', '/');
            int idx = oldFilename.LastIndexOf('/') + 1;
            string oldPath = oldFilename.Substring(0, idx);

            newFilename = newFilename.Replace('\\', '/');
            idx = newFilename.LastIndexOf('/') + 1;
            string newPath = newFilename.Substring(0, idx);
            newFilename = newFilename.Substring(idx);

            EnsureIndirectBase(newPath);

            var fStream = File.Open(oldFilename, FileMode.Open, FileAccess.Read);
            string ret;
            if (Path.GetExtension(oldFilename).ToLower() == ".shadergraph")
                ret = ConvertShadergraph(fStream, oldPath, newPath, newFilename);
            else
                ret = ConvertShader(fStream, oldPath, newPath, newFilename);
#if UNITY_EDITOR
            if (!string.IsNullOrEmpty(ret))
                UnityEditor.AssetDatabase.Refresh();
#endif
            fStream.Close();
            return ret;
        }
        public static string ConvertShader(Stream oldFile, string oldPath, string savePath, string newFilename)
        {
            if (oldFile == null)
                return null;
            //Debug.Log("Convering: " + newFilename);
            string fileText = new StreamReader(oldFile).ReadToEnd();
            int namePos = fileText.IndexOf("shader \"", StringComparison.OrdinalIgnoreCase);
            if (namePos < 0)
                namePos = fileText.IndexOf("shader\"", StringComparison.OrdinalIgnoreCase);
            string ret;
            if (namePos >= 0)
            {
                namePos += 8;
                fileText = fileText.Insert(namePos, nameInsert);
                ret = fileText.Substring(namePos, fileText.IndexOf("\"", namePos) - namePos);
            }
            else
            {
                Debug.LogError("Unable to find Shader Name for file:" + newFilename);
                return null;
            }
            //update #include statements that don't have a path that exist in the same directory
            int Pos = fileText.IndexOf("#include ");
            while (Pos > 0)
            {
                int QuotePos = fileText.IndexOf("\"", Pos);
                int QuoteEnd = fileText.IndexOf("\"", QuotePos + 1);
                if (QuotePos > 0 && QuoteEnd > 0)
                {
                    string includeName = fileText.Substring(QuotePos + 1, (QuoteEnd - QuotePos) - 1);
                    if (File.Exists(oldPath + includeName))
                        fileText = fileText.Insert(QuotePos + 1, oldPath);
                }
                Pos = fileText.IndexOf("#include ", Pos + 8);
            }
            //now for the harder part ... modifying the shader code ... first find any surface lines
            //write the shader line at the beginning of the shader code
            Pos = fileText.IndexOf('{', namePos);
            if (Pos >= 0)
                fileText = fileText.Insert(Pos + 1, CommentStart + surfInsert + CommentEnd);
            else
                Debug.LogError("Unable to find opening of shader block for file:" + newFilename);
            //First search for existing Instancing option settings and remove the setup as we will use our own
            Pos = fileText.IndexOf("#pragma instancing_options ", StringComparison.OrdinalIgnoreCase);
            while (Pos >= 0)
            {
                int end = fileText.IndexOf('\n', Pos);
                int proceduralPos = fileText.IndexOf("procedural:", Pos, end - Pos, StringComparison.OrdinalIgnoreCase);
                if (proceduralPos >= 0)
                {
                    fileText = fileText.Substring(0, Pos) + fileText.Substring(end);
                }
                if (Pos >= fileText.Length)
                    Pos = -1;
                else
                    Pos = fileText.IndexOf("#pragma instancing_options ", Pos + 1, StringComparison.OrdinalIgnoreCase);
            }
            //Next add in the Vertex shader support lines if needed.
            Pos = fileText.IndexOf("#pragma vertex ", StringComparison.OrdinalIgnoreCase);
            int lastPos = 0;
            while (Pos >= 0)
            {
                //find beginning and end of that CGPROGRAM block (or GLSL)
                ScriptType sType = ScriptType.CG;
                int begin = fileText.LastIndexOf("CGPROGRAM", Pos, Pos - lastPos, StringComparison.Ordinal);
                if (begin < 0)
                {
                    begin = fileText.LastIndexOf("GLSLPROGRAM", Pos, Pos - lastPos, StringComparison.Ordinal);
                    if (begin < 0)
                    {
                        begin = fileText.LastIndexOf("HLSLPROGRAM", Pos, Pos - lastPos, StringComparison.Ordinal);
                        if (begin >= 0)
                            sType = ScriptType.HLSL;
                    }
                    else
                        sType = ScriptType.GLSL;
                }
                if (begin >= 0)
                {
                    int end = fileText.IndexOf(sType == ScriptType.GLSL ? "ENDGLSL" : (sType == ScriptType.HLSL ? "ENDHLSL" : "ENDCG"), begin);
                    if (end >= 0)
                    {
                        //see if we already have a #pragma instancing_options with a "procedural:" entry
                        int proceduralPos = fileText.IndexOf("#pragma instancing_options", begin, end - begin, StringComparison.OrdinalIgnoreCase);
                        bool foundProcedural = false;
                        string proceduralName = "";
                        //Debug.Log("proceduralPos=" + proceduralPos + " Range:" + begin + " - " + (end - begin));
                        //Debug.Log(fileText.Substring(begin, end - begin));
                        if (proceduralPos >= 0)
                        {
                            proceduralPos = fileText.IndexOf("procedural:", proceduralPos, fileText.IndexOf('\n', proceduralPos, end - proceduralPos) - proceduralPos, StringComparison.OrdinalIgnoreCase);

                            //Debug.Log("proceduralPos2=" + proceduralPos);
                            if (proceduralPos >= 0)
                            {
                                proceduralPos += 11;
                                int endL = proceduralPos;
                                for (; endL < end; ++endL)
                                {
                                    if (fileText[endL] == '\n' || fileText[endL] == ' ')
                                        break;
                                }
                                foundProcedural = true;
                                proceduralName = fileText.Substring(proceduralPos, endL - proceduralPos);
                                fileText = fileText.Substring(0, proceduralPos) + proceduralFunction + fileText.Substring(endL);
                                end += proceduralFunction.Length - proceduralName.Length;
                                if (Pos > proceduralPos)
                                    Pos += proceduralFunction.Length - proceduralName.Length;
                            }
                        }
                        //now insert our code before "ENDCG"
                        string toInsert = CommentStart + (foundProcedural ? vertFuncs + proceduralInsertPre + proceduralName + proceduralInsertPost : vertInsert) + CommentEnd;
                        int iPos = fileText.LastIndexOf('\n', end, end - Pos);
                        if (iPos < 0)
                            iPos = end;
                        fileText = fileText.Insert(iPos, toInsert);
                        Pos = end + 5;
                    }
                    else
                    {
                        if (sType == ScriptType.CG)
                            Debug.LogError("Found 'CGPROGRAM' at " + begin + " but no ENDCG? for file:" + newFilename);
                        else if (sType == ScriptType.GLSL)
                            Debug.LogError("Found 'GLSLPROGRAM' at " + begin + " but no ENDGLSL? for file:" + newFilename);
                        else if (sType == ScriptType.HLSL)
                            Debug.LogError("Found 'HLSLPROGRAM' at " + begin + " but no ENDHLSL? for file:" + newFilename);
                    }
                }
                else
                {
                    //create our own CGPROGRAM lines
                    int bracketCount = 0;
                    for (int p = Pos + 15; p < fileText.Length; ++p)
                    {
                        if (fileText[p] == '{')
                        {
                            ++bracketCount;
                        }
                        else if (fileText[p] == '}')
                        {
                            if (--bracketCount < 0)
                            {
                                //we found the closing bracket of the section we are in
                                fileText = fileText.Insert(p, CommentStart + "\nCGPROGRAM\n" + vertInsert + "ENDCG\n" + CommentEnd);
                                Pos = p + CommentStart.Length + vertInsert.Length + 15 + CommentEnd.Length;
                                break;
                            }
                        }
                    }
                }
                lastPos = Pos;
                if (Pos >= fileText.Length)
                {
                    //Debug.Log("Position > Length (" + Pos + ", " + fileText.Length + ") for file: " + newFilename);
                    Pos = -1;
                }
                else
                    Pos = fileText.IndexOf("#pragma vertex ", Pos + 1, StringComparison.OrdinalIgnoreCase);
            }
            fileText = fileText.Replace("\r\n", "\n");
            newFilename = savePath + newFilename;
            StormUtils.EnsureDirExists(newFilename);
            File.WriteAllText(newFilename, fileText);
            return ret;
        }
        public static string ConvertShadergraph(Stream oldFile, string oldPath, string savePath, string newFilename)
        {

            return "";
        }
        public static void EnsureIndirectBase(string path)
        {
            path = path.Replace('\\', '/');
            if (!path.EndsWith("/"))
            {
                //either a file path or a dir without a slash
                if (path.LastIndexOf('.') > path.LastIndexOf('/'))
                    path = path.Substring(0, path.LastIndexOf('/') + 1);
                else
                    path += "/";
            }
            path += "StormIndirectBase.cginc";
            StormUtils.EnsureDirExists(path);
            if (!File.Exists(path))
                File.WriteAllText(path, indirectBaseText);
        }
        public static void UpdateStandardShaders(bool userRequested = false)
        {
#if UNITY_EDITOR
            if (Application.isPlaying)
                return; //no upates while playing
            string unityVersion = Application.unityVersion;
            string path = DefaultSaveDir + "shaderData.asset";
            StormUtils.EnsureDirExists(path);
            StormShaderData shaderData = UnityEditor.AssetDatabase.LoadAssetAtPath<StormShaderData>(path);
            if (shaderData == null)
            {
                shaderData = ScriptableObject.CreateInstance<StormShaderData>();
                UnityEditor.AssetDatabase.CreateAsset(shaderData, path);
            }
            if (!userRequested)
            {
                if (shaderData.IsCustom)
                {
                    if (string.IsNullOrEmpty(shaderData.ShaderVersion) || shaderData.ShaderConverterVersion == StormShaderConverter.ConverterVersion)
                        return;
                    UpdateStandardShaders(shaderData.ShaderVersion);
                    return;
                }
                else if (shaderData.ShaderVersion == unityVersion && shaderData.ShaderConverterVersion == StormShaderConverter.ConverterVersion)
                {
                    return; //no auto-update required or was custom shader pack
                }
                if (UnityEditor.EditorUtility.GetDialogOptOutDecision(UnityEditor.DialogOptOutDecisionType.ForThisMachine, "StormStandardShadersIgnore-" + unityVersion))
                {
                    return;
                }
                int dialogResult = UnityEditor.EditorUtility.DisplayDialogComplex("Update Standard Shaders?", "The Standard Unity shaders appear to be out of date for this Editor. Do you want to update Storm versions of the Standard shaders?\nThese are usually only needed for the built-in pipeline with the exception of the \"Invalid shader\" shader.\n\nThe Standard Shader pack will be downloaded from the Unity Archives then processed through Storm.", "Yes", "No", "No, Do not ask again for this version.");
                if (dialogResult == 1)
                    return;
                else if (dialogResult == 2)
                {
                    UnityEditor.EditorUtility.SetDialogOptOutDecision(UnityEditor.DialogOptOutDecisionType.ForThisMachine, "StormStandardShadersIgnore-" + unityVersion, true);
                    return;
                }
            }
            try
            {
                /*/
                string uri = "https://unity3d.com/get-unity/download/archive";
                string shaderStr = "builtin_shaders-" + unityVersion;
                if (unityVersion.IndexOf('b') > unityVersion.Length - 4)
                {
                    uri = "https://unity3d.com/beta/" + unityVersion.Substring(0, unityVersion.IndexOf('b') - 2) + "b";
                    shaderStr = "builtin_shaders.zip";
                    //Debug.Log("new URI: " + uri);
                }
                //*/
                string uri = "https://unity3d.com/download_unity/builtin_shaders.zip";
                var page = new UnityEngine.Networking.UnityWebRequest(uri);
                page.SetRequestHeader("Accept", @"text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8");
                page.SetRequestHeader("User-Agent", @"Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/51.0.2704.106 Safari/537.36");
                page.downloadHandler = new UnityEngine.Networking.DownloadHandlerBuffer();
                page.SendWebRequest();
                StormEditorProgress.Show("Updating Storm Standard Shaders", "Getting Download URL", 100);
                /*/
                while (!page.isDone)
                {
                    if (StormEditorProgress.Update(Mathf.RoundToInt(page.downloadProgress * 100)))
                    {
                        page.Abort();
                        StormEditorProgress.Clear();
                        return;
                    }
                }
                if (page.result == UnityEngine.Networking.UnityWebRequest.Result.ProtocolError)
                {
                    StormEditorProgress.Clear();
                    UnityEditor.EditorUtility.DisplayDialog("Updating Storm Standard Shaders", "Error updating Unity standard shaders.\nHTTP Error: " + page.error, "Ok");
                    return;
                }
                if (page.result == UnityEngine.Networking.UnityWebRequest.Result.ConnectionError)
                {
                    StormEditorProgress.Clear();
                    UnityEditor.EditorUtility.DisplayDialog("Updating Storm Standard Shaders", "Error updating Unity standard shaders.\nConnection Error: " + page.error, "Ok");
                    return;
                }
                string pageText = page.downloadHandler.text;
                int idx = pageText.IndexOf(shaderStr);
                if (idx < 0)
                {
                    StormEditorProgress.Clear();
                    UnityEditor.EditorUtility.DisplayDialog("Updating Storm Standard Shaders", "Error updating Unity standard shaders.\nUnity version '" + unityVersion + "' not found in download list.", "Ok");
                    return;
                }
                int sIdx = pageText.LastIndexOf('"', idx, idx) + 1;
                if (sIdx <= 0)
                {
                    StormEditorProgress.Clear();
                    UnityEditor.EditorUtility.DisplayDialog("Updating Storm Standard Shaders", "Error parsing download list.\nUnable to find start of download link.", "Ok");
                    return;
                }
                idx = pageText.IndexOf(".zip\"", sIdx + 1) + 4;
                if (idx <= 3)
                {
                    StormEditorProgress.Clear();
                    UnityEditor.EditorUtility.DisplayDialog("Updating Storm Standard Shaders", "Error parsing download list.\nUnable to find end of download link.", "Ok");
                    return;
                }
                path = pageText.Substring(sIdx, idx - sIdx);
                page = new UnityEngine.Networking.UnityWebRequest(path);
                page.downloadHandler = new UnityEngine.Networking.DownloadHandlerBuffer();
                page.SendWebRequest();
                //*/
                StormEditorProgress.Update("Downloading Unity Shaders", 0, 100, true);
                while (!page.isDone)
                {
                    if (StormEditorProgress.Update(Mathf.RoundToInt(page.downloadProgress * 100)))
                    {
                        Debug.Log("cancelled");
                        page.Abort();
                        StormEditorProgress.Clear();
                        return;
                    }
                }
                Debug.Log("result=" + page.result + "\nerror:" + page.error);
                if (page.result == UnityEngine.Networking.UnityWebRequest.Result.ProtocolError)
                {
                    StormEditorProgress.Clear();
                    UnityEditor.EditorUtility.DisplayDialog("Updating Storm Standard Shaders", "Error Downloading " + path + ".\nHTTP Error: " + page.error, "Ok");
                    return;
                }
                if (page.result == UnityEngine.Networking.UnityWebRequest.Result.ConnectionError)
                {
                    StormEditorProgress.Clear();
                    UnityEditor.EditorUtility.DisplayDialog("Updating Storm Standard Shaders", "Error Downloading " + path + ".\nConnection Error: " + page.error, "Ok");
                    return;
                }
                MemoryStream zipStream = new MemoryStream(page.downloadHandler.data);
                ZipArchive zip = new ZipArchive(zipStream, ZipArchiveMode.Read);
                var files = zip.Entries;
                int numFiles = files.Count;
                path = DefaultSaveDir + "StandardShaders/";
                if (Directory.Exists(path))
                    Directory.Delete(path, true);
                EnsureIndirectBase(path);
                for (int f = 0; f < numFiles; ++f)
                {
                    StormEditorProgress.Update("Converting Unity Shaders", f, numFiles, true);
                    if (files[f].Name.EndsWith(".shader"))
                    {
                        var fStream = files[f].Open();
                        if (fStream != null)
                        {
                            string ret = ConvertShader(fStream, "", path, files[f].Name);
                            fStream.Close();
                        }
                    }
                    else if (files[f].Name.EndsWith("license.txt") || files[f].Name.EndsWith(".cginc"))
                    {
                        files[f].ExtractToFile(path + files[f].Name);
                    }
                }
                shaderData.ShaderConverterVersion = StormShaderConverter.ConverterVersion;
                shaderData.ShaderVersion = unityVersion;
                shaderData.IsCustom = false;
                UnityEditor.EditorUtility.SetDirty(shaderData);
                StormEditorProgress.Clear();
                UnityEditor.AssetDatabase.Refresh();
                UnityEditor.EditorUtility.DisplayDialog("Updating Storm Standard Shaders", "Update Complete.\nErrors in some \"standard shaders\" are expected and can be ignored unless they cause issues.", "Ok");
            }
            catch (Exception e)
            {
                StormEditorProgress.Clear();
                Debug.LogException(e);
            }
#endif
        }
        public static void UpdateStandardShaders(string pathToPack)
        {
#if UNITY_EDITOR
            if (Application.isPlaying)
                return; //no upates while playing
            string path = DefaultSaveDir + "shaderData.asset";
            StormUtils.EnsureDirExists(path);
            StormShaderData shaderData = UnityEditor.AssetDatabase.LoadAssetAtPath<StormShaderData>(path);
            if (shaderData == null)
            {
                shaderData = ScriptableObject.CreateInstance<StormShaderData>();
                UnityEditor.AssetDatabase.CreateAsset(shaderData, path);
            }
            try
            {
                FileStream zipStream = File.Open(pathToPack, FileMode.Open);
                if (zipStream != null)
                {
                    ZipArchive zip = new ZipArchive(zipStream, ZipArchiveMode.Read);
                    var files = zip.Entries;
                    int numFiles = files.Count;
                    path = DefaultSaveDir + "StandardShaders/";
                    if (Directory.Exists(path))
                        Directory.Delete(path, true);
                    EnsureIndirectBase(path);
                    for (int f = 0; f < numFiles; ++f)
                    {
                        if (files[f].Name.EndsWith(".shader"))
                        {
                            var fStream = files[f].Open();
                            if (fStream != null)
                            {
                                string ret = StormShaderConverter.ConvertShader(fStream, "", path, files[f].Name);
                                fStream.Close();
                            }
                        }
                        else if (files[f].Name.EndsWith("license.txt"))
                        {
                            files[f].ExtractToFile(path + files[f].Name);
                        }
                    }
                    shaderData.ShaderConverterVersion = StormShaderConverter.ConverterVersion;
                    shaderData.ShaderVersion = pathToPack;
                    shaderData.IsCustom = true;
                    UnityEditor.EditorUtility.SetDirty(shaderData);
                    StormEditorProgress.Clear();
                    UnityEditor.AssetDatabase.Refresh();
                }
            }
            catch (Exception e)
            {
                StormEditorProgress.Clear();
                throw e;
            }
#endif
        }
    }
}
