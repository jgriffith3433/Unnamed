// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

Shader "PWStorm/Hidden/Nature/Tree Creator Leaves Rendertex" {
//BEGIN STORM EDIT

CGINCLUDE
#pragma multi_compile_instancing
#pragma multi_compile STORM_ENABLED

#include "StormIndirectBase.cginc"
ENDCG

//END STORM EDIT

Properties {
    _TranslucencyColor ("Translucency Color", Color) = (0.73,0.85,0.41,1) // (187,219,106,255)
    _Cutoff ("Alpha cutoff", Range(0,1)) = 0.5
    _HalfOverCutoff ("0.5 / alpha cutoff", Range(0,1)) = 1.0
    _TranslucencyViewDependency ("View dependency", Range(0,1)) = 0.7

    _MainTex ("Base (RGB) Alpha (A)", 2D) = "white" {}
    _BumpSpecMap ("Normalmap (GA) Spec (R) Shadow Offset (B)", 2D) = "bump" {}
    _TranslucencyMap ("Trans (B) Gloss(A)", 2D) = "white" {}
}

SubShader {

    Pass {
CGPROGRAM
#pragma vertex vert
#pragma fragment frag
#include "UnityCG.cginc"
#include "UnityBuiltin3xTreeLibrary.cginc"

struct v2f {
    float4 pos : SV_POSITION;
    float2 uv : TEXCOORD0;
    float3 color : TEXCOORD1;
    float3 backContrib : TEXCOORD2;
    float3 nl : TEXCOORD3;
    float3 nh : TEXCOORD4;
    UNITY_VERTEX_OUTPUT_STEREO
};

CBUFFER_START(UnityTerrainImposter)
    float3 _TerrainTreeLightDirections[4];
    float4 _TerrainTreeLightColors[4];
CBUFFER_END

v2f vert (appdata_full v) {
    v2f o;
    UNITY_SETUP_INSTANCE_ID(v);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
    ExpandBillboard (UNITY_MATRIX_IT_MV, v.vertex, v.normal, v.tangent);
    o.pos = UnityObjectToClipPos(v.vertex);
    o.uv = v.texcoord.xy;
    float3 viewDir = normalize(ObjSpaceViewDir(v.vertex));

    for (int j = 0; j < 3; j++)
    {
        float3 lightDir = _TerrainTreeLightDirections[j];

        half nl = dot (v.normal, lightDir);

        // view dependent back contribution for translucency
        half backContrib = saturate(dot(viewDir, -lightDir));

        // normally translucency is more like -nl, but looks better when it's view dependent
        backContrib = lerp(saturate(-nl), backContrib, _TranslucencyViewDependency);
        o.backContrib[j] = backContrib * 2;

        // wrap-around diffuse
        nl = max (0, nl * 0.6 + 0.4);
        o.nl[j] = nl;

        half3 h = normalize (lightDir + viewDir);
        float nh = max (0, dot (v.normal, h));
        o.nh[j] = nh;
    }

    o.color = v.color.a;
    return o;
}

sampler2D _MainTex;
sampler2D _BumpSpecMap;
sampler2D _TranslucencyMap;
fixed _Cutoff;

half3 CalcTreeLighting(half3 lightColor, fixed3 albedo, half backContrib, half nl, half nh, half specular, half gloss)
{
    half3 translucencyColor = backContrib * _TranslucencyColor;

    half spec = pow (nh, specular) * gloss;
    return (albedo * (translucencyColor + nl) + _SpecColor.rgb * spec) * lightColor;
}

fixed4 frag (v2f i) : SV_Target {
    fixed4 col = tex2D (_MainTex, i.uv);
    clip (col.a - _Cutoff);

    fixed3 albedo = col.rgb * i.color;

    half specular = tex2D (_BumpSpecMap, i.uv).r * 128.0;

    fixed4 trngls = tex2D (_TranslucencyMap, i.uv);
    half gloss = trngls.a;

    half3 light = UNITY_LIGHTMODEL_AMBIENT * albedo;

    half3 backContribs = i.backContrib * trngls.b;

/*  This is unrolled below, indexing into a vec3 components is a terrible idea
    for (int j = 0; j < 3; j++)
    {
        half3 lightColor = _TerrainTreeLightColors[j].rgb;
        half3 translucencyColor = backContribs[j] * _TranslucencyColor;

        half nl = i.nl[j];
        half nh = i.nh[j];
        half spec = pow (nh, specular) * gloss;
        light += (albedo * (translucencyColor + nl) + _SpecColor.rgb * spec) * lightColor;
    }*/

    light += CalcTreeLighting(_TerrainTreeLightColors[0].rgb, albedo, backContribs.x, i.nl.x, i.nh.x, specular, gloss);
    light += CalcTreeLighting(_TerrainTreeLightColors[1].rgb, albedo, backContribs.y, i.nl.y, i.nh.y, specular, gloss);
    light += CalcTreeLighting(_TerrainTreeLightColors[2].rgb, albedo, backContribs.z, i.nl.z, i.nh.z, specular, gloss);

    fixed4 c;
    c.rgb = light;
    c.a = 1;
    return c;
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

FallBack Off
}
