// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

Shader "Nature/Terrain/MyStandard" {
    Properties {
        // used in fallback on old cards & base map
        [HideInInspector] _MainTex ("BaseMap (RGB)", 2D) = "white" {}
        [HideInInspector] _Color ("Main Color", Color) = (1,1,1,1)
        [HideInInspector] _TerrainHolesTexture("Holes Map (RGB)", 2D) = "white" {}

        [Toggle(_UNITY_TERRAIN_ON)]
        _UNITY_TERRAIN("_UNITY_TERRAIN_ON", Int) = 0

        [Toggle(_CLIPMAP_ON)]
        _CLIPMAP("_CLIPMAP_ON", Int) = 0
        
        [Toggle(_DISPLACEMENT_ON)]
        _DISPLACEMENT("_DISPLACEMENT_ON", Int) = 0
        
        [Toggle(_BRANCHING_ON)]
        _BRANCHING("_BRANCHING_ON", Int) = 0

        [Toggle(_ALPHACLIP_ON)]
        _ALPHACLIP("_ALPHACLIP_ON", Int) = 0  

        [Toggle(_SNOW_ON)]
        _SNOW("_SNOW_ON", Int) = 0

        [Toggle(_SHADOWFIX_ON)]
        _SHADOWFIX("_SHADOWFIX_ON", Int) = 1
        

        [HDR]_SnowColor("Snow Color", Color) = (1,1,1,1)
        _SnowHeight("Snow Height", Float) = 1000
        _SnowFalloff("Snow Falloff", Float) = 140
        _SnowCurvatureInfluence("Snow Curvature Influence", Range(0.0, 1.0)) = 0.5
        _SnowUVScale("Snow UV Scale", Float) = 10
        _MipBias("Mip Bias", Float) = 10
        [NoScaleOffset]_SnowAlbedo("_SnowAlbedo", 2D) = "white" {}
        [NoScaleOffset]_SnowNormal("_SnowNormal", 2D) = "bump" {}
        [NoScaleOffset]_SnowMask("_SnowMask", 2D) = "black" {}

        [HideInInspector]_StormVT_ClipmapTerrainState("_StormVT_ClipmapTerrainState", Int) = 0

    }

    SubShader {
        Tags {
            "Queue" = "Geometry-100"
            "RenderType" = "Opaque"
            "TerrainCompatible" = "True"
        }

        CGPROGRAM
        #pragma surface surf Standard vertex:SplatmapVert finalcolor:SplatmapFinalColor finalgbuffer:SplatmapFinalGBuffer addshadow fullforwardshadows
        #pragma instancing_options assumeuniformscaling nomatrices nolightprobe nolightmap forwardadd procedural:setup
        #pragma multi_compile_fog // needed because finalcolor oppresses fog code generation.
        #pragma target 3.0
        #include "UnityPBSLighting.cginc"

        #pragma multi_compile_local_fragment __ _ALPHATEST_ON
        #pragma multi_compile_local __ _NORMALMAP

        #define TERRAIN_STANDARD_SHADER
        #define TERRAIN_INSTANCED_PERPIXEL_NORMAL
        #define TERRAIN_SURFACE_OUTPUT SurfaceOutputStandard

        #pragma shader_feature _UNITY_TERRAIN_ON
        #pragma shader_feature _DISPLACEMENT_ON
        #pragma shader_feature _BRANCHING_ON
        #pragma shader_feature _CLIPMAP_ON
        #pragma shader_feature _ALPHACLIP_ON
        #pragma shader_feature _SNOW_ON
        #pragma shader_feature _SHADOWFIX_ON


        half _Metallic0;
        half _Metallic1;
        half _Metallic2;
        half _Metallic3;

        half _Smoothness0;
        half _Smoothness1;
        half _Smoothness2;
        half _Smoothness3;

        #include "../../../Shaders/Includes/StormVTClipmapFunctions.hlsl"
        #include "Custom_BuiltIn_Includes.hlsl"
        #include "../../../Shaders/Includes/StormVTHLSLFunctions.hlsl"
        #include "MyTerrainSplatmapCommon.cginc"

        float Rectangle(float2 UV, float Width, float Height)
        {
            float2 d = abs(UV * 2 - 1) - float2(Width, Height);
            d = 1 - d / fwidth(d);
            return saturate(min(d.x, d.y));
        }

        void surf (Input IN, inout SurfaceOutputStandard o) {
            #ifdef _UNITY_TERRAIN_ON
                half4 splat_control;
                half weight;
                fixed4 mixedDiffuse;
                half4 defaultSmoothness = half4(_Smoothness0, _Smoothness1, _Smoothness2, _Smoothness3);
                SplatmapMix(IN, defaultSmoothness, splat_control, weight, mixedDiffuse, o.Normal);
                o.Albedo = mixedDiffuse.rgb;
                o.Alpha = weight;
                o.Smoothness = mixedDiffuse.a;
                o.Metallic = dot(splat_control, half4(_Metallic0, _Metallic1, _Metallic2, _Metallic3));
            #else
                fixed3 diffuse = fixed3(1, 0, 0);
                fixed3 normal = fixed3(0, 1, 0);
                fixed4 mask = fixed4(0, 1, 0, 0);

                
                float3 worldPosition = IN.worldPos + _StormVT_WorldOffset.xyz;

                float3 UVs;
                float2 fracUVs;
                float2 volumeUVs;
                float2 rawUVs;
                float zoomShift;


                SampleVirtualUVs(worldPosition, sampler_point_repeat, UVs, fracUVs, zoomShift);
                
            #ifdef _ALPHACLIP_ON
                clip(UVs.z == 0.0f ? -1 : 1);
            #endif

            #ifdef _ALPHACLIP_ON
                float holesMask = SamplePhysicalTextureHoles(UVs, sampler_point_repeat, 0);
                clip(holesMask * 2 - 1);
            #endif


                SamplePhysicalTextures(worldPosition, sampler_StormVT_PhysicalTextureSource0_Trilinear_Repeat_Aniso8, diffuse, normal, mask);
        
                #ifdef _SNOW_ON
                    float3 blendedDiffuse, blendedNormal;
                    float4 blendedMask;
                    SnowSampleVirtualUVs(worldPosition, sampler_point_repeat, UVs, fracUVs, volumeUVs, zoomShift);
                    Snow(UVs, worldPosition, sampler_linear_repeat, diffuse, normal, mask, blendedDiffuse, blendedNormal, blendedMask);

                    diffuse = blendedDiffuse;
                    normal = blendedNormal;
                    mask = blendedMask;
                #endif
        
        
        
                float3 worldNormal = WorldNormalVector(IN, float3(0, 0, 1));
                float3 worldTangent = WorldNormalVector(IN, float3(1, 0, 0));
                float3 worldBitangent = float3(0,0,0);
                //Fix Bitangents
                #ifdef _CLIPMAP_ON
                    //worldBitangent = cross(worldNormal, worldTangent);
                    worldBitangent = WorldNormalVector(IN, float3(0, 1, 0));
                #else
                    worldBitangent = WorldNormalVector(IN, float3(0, 1, 0));
                #endif
                
                float3x3 worldToTangent = float3x3(worldTangent, worldBitangent, worldNormal);
                o.Normal = mul(worldToTangent, normal);
            
                o.Albedo = diffuse;
                o.Alpha = 1;
                o.Smoothness = mask.a;
                o.Metallic = 0;
            #endif
        }
        ENDCG

        UsePass "Hidden/Nature/Terrain/Utilities/PICKING"
        UsePass "Hidden/Nature/Terrain/Utilities/SELECTION"
    }

    Dependency "AddPassShader"    = "Hidden/TerrainEngine/Splatmap/MyStandard-AddPass"

    Fallback "Nature/Terrain/Diffuse"
}
