// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

Shader "PWStorm/Nature/Tree Creator Leaves Fast" {
//BEGIN STORM EDIT

CGINCLUDE
#pragma multi_compile_instancing
#pragma multi_compile STORM_ENABLED

#include "StormIndirectBase.cginc"
ENDCG

//END STORM EDIT

Properties {
    _Color ("Main Color", Color) = (1,1,1,1)
    _TranslucencyColor ("Translucency Color", Color) = (0.73,0.85,0.41,1) // (187,219,106,255)
    _Cutoff ("Alpha cutoff", Range(0,1)) = 0.3
    _TranslucencyViewDependency ("View dependency", Range(0,1)) = 0.7
    _ShadowStrength("Shadow Strength", Range(0,1)) = 1.0

    _MainTex ("Base (RGB) Alpha (A)", 2D) = "white" {}

    // These are here only to provide default values
    [HideInInspector] _TreeInstanceColor ("TreeInstanceColor", Vector) = (1,1,1,1)
    [HideInInspector] _TreeInstanceScale ("TreeInstanceScale", Vector) = (1,1,1,1)
    [HideInInspector] _SquashAmount ("Squash", Float) = 1
}

SubShader {
    Tags {
        "IgnoreProjector"="True"
        "RenderType" = "TreeLeaf"
    }
    LOD 200

    Pass {
        Tags { "LightMode" = "ForwardBase" }
        Name "ForwardBase"

    CGPROGRAM
        #include "UnityBuiltin3xTreeLibrary.cginc"

        #pragma vertex VertexLeaf
        #pragma fragment FragmentLeaf
        #pragma multi_compile_fwdbase nolightmap
        #pragma multi_compile_fog

        sampler2D _MainTex;
        float4 _MainTex_ST;

        fixed _Cutoff;
        sampler2D _ShadowMapTexture;

        struct v2f_leaf {
            float4 pos : SV_POSITION;
            fixed4 diffuse : COLOR0;
        #if defined(SHADOWS_SCREEN)
            fixed4 mainLight : COLOR1;
        #endif
            float2 uv : TEXCOORD0;
        #if defined(SHADOWS_SCREEN)
            float4 screenPos : TEXCOORD1;
        #endif
            UNITY_FOG_COORDS(2)
            UNITY_VERTEX_OUTPUT_STEREO
        };

        v2f_leaf VertexLeaf (appdata_full v)
        {
            v2f_leaf o;
            UNITY_SETUP_INSTANCE_ID(v);
            UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
            TreeVertLeaf(v);
            o.pos = UnityObjectToClipPos(v.vertex);

            fixed ao = v.color.a;
            ao += 0.1; ao = saturate(ao * ao * ao); // emphasize AO

            fixed3 color = v.color.rgb * ao;

            float3 worldN = UnityObjectToWorldNormal (v.normal);

            fixed4 mainLight;
            mainLight.rgb = ShadeTranslucentMainLight (v.vertex, worldN) * color;
            mainLight.a = v.color.a;
            o.diffuse.rgb = ShadeTranslucentLights (v.vertex, worldN) * color;
            o.diffuse.a = 1;
        #if defined(SHADOWS_SCREEN)
            o.mainLight = mainLight;
            o.screenPos = ComputeScreenPos (o.pos);
        #else
            o.diffuse += mainLight;
        #endif
            o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
            UNITY_TRANSFER_FOG(o,o.pos);
            return o;
        }

        fixed4 FragmentLeaf (v2f_leaf IN) : SV_Target
        {
            fixed4 albedo = tex2D(_MainTex, IN.uv);
            fixed alpha = albedo.a;
            clip (alpha - _Cutoff);

        #if defined(SHADOWS_SCREEN)
            half4 light = IN.mainLight;
            half atten = tex2Dproj(_ShadowMapTexture, UNITY_PROJ_COORD(IN.screenPos)).r;
            light.rgb *= lerp(1, atten, _ShadowStrength);
            light.rgb += IN.diffuse.rgb;
        #else
            half4 light = IN.diffuse;
        #endif

            fixed4 col = fixed4 (albedo.rgb * light, 0.0);
            UNITY_APPLY_FOG(IN.fogCoord, col);
            return col;
        }

//BEGIN STORM EDIT

#pragma multi_compile_instancing
#pragma multi_compile STORM_ENABLED
#pragma instancing_options procedural:storm_vert
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

//END STORM EDIT

    ENDCG
    }
}

Dependency "OptimizedShader" = "Hidden/Nature/Tree Creator Leaves Fast Optimized"
FallBack "Diffuse"
}
