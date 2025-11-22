// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

Shader "PWStorm/Hidden/Preview Encoded Lightmap doubleLDR"
{
//BEGIN STORM EDIT

CGINCLUDE
#pragma multi_compile_instancing
#pragma multi_compile STORM_ENABLED

#include "StormIndirectBase.cginc"
ENDCG

//END STORM EDIT

    Properties {
        _MainTex ("Texture", Any) = "white" { }
        _Mip ("Mip", Float) = -1.0 // mip level to display; negative does regular sample
        _ColorMask ("Color Mask", Color) = (1, 1, 1, 1)
        _Exposure ("Exposure", Float) = 0.0
    }
    Subshader
    {
        Tags { "ForceSupported" = "True" "RenderType" = "Opaque" }
        Cull Off ZWrite Off ZTest Always
        Pass
        {
            CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag
                #pragma require sampleLOD
                #include "UnityCG.cginc"
                struct v2f {
                    float4 vertex : SV_POSITION;
                    float2 _uv0 : TEXCOORD0;
                    float2 _uv1 : TEXCOORD1;
                    fixed4 color : COLOR;
                };
                struct appdata_t {
                    float4 vertex : POSITION;
                    fixed4 color : COLOR;
                    float2 texcoord : TEXCOORD0;
                };
                uniform float4 _MainTex_ST;
                uniform float4 _MainTex_HDR;
                uniform float4x4 unity_GUIClipTextureMatrix;
                v2f vert (appdata_t v)
                {
                    v2f o;
                    o.vertex = UnityObjectToClipPos(v.vertex);
                    float3 texgen = UnityObjectToViewPos(v.vertex);
                    o._uv1 = mul(unity_GUIClipTextureMatrix, float4(texgen.xy, 0, 1.0)).xy;
                    o.color = v.color;
                    o._uv0 = TRANSFORM_TEX(v.texcoord,_MainTex);
                    return o;
                }
                sampler2D _MainTex;
                sampler2D _GUIClipTexture;
                uniform float _Mip;
                uniform fixed4 _ColorMask;
                float _Exposure;
                uniform bool _ManualTex2SRGB;

                fixed4 frag( v2f i ) : COLOR
                {
                    fixed4 lmc = tex2D(_MainTex, i._uv0) * _ColorMask, lmcmip = tex2Dlod(_MainTex, float4(i._uv0.x, i._uv0.y, 0, _Mip)) * _ColorMask;
                    if (_Mip >= 0.0) lmc = lmcmip;
                    half3 lightmap = DecodeLightmapDoubleLDR(lmc, _MainTex_HDR);
                    half alpha = UNITY_SAMPLE_1CHANNEL(_GUIClipTexture, i._uv1);

                    fixed4 col = i.color;
                    col.rgb *= lightmap;
                    col.rgb *= exp2(_Exposure);
                    col.a   *= alpha;
                    clip (col.a - 0.001);

                    if (_ManualTex2SRGB) col.rgb = LinearToGammaSpace(col.rgb);
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
}
