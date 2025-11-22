// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

Shader "PWStorm/Hidden/Mesh-MultiPreview"
{
//BEGIN STORM EDIT

CGINCLUDE
#pragma multi_compile_instancing
#pragma multi_compile STORM_ENABLED

#include "StormIndirectBase.cginc"
ENDCG

//END STORM EDIT

    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _UVChannel ("UV Channel", Int) = 0

        // this goes up to 4
        // 0 - flat UVs
        // 1 - vertex color
        // 2 - normals as color
        // 3 - tangents as color
        // 4 - checkerboard UVs
        _Mode ("Draw mode", Int) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vertShader
            #pragma fragment fragShader

            #include "UnityCG.cginc"

            float _WireThickness;
            int _UVChannel;
            int _Mode;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv0 : TEXCOORD0;
                float2 uv1 : TEXCOORD1;
                float2 uv2 : TEXCOORD2;
                float2 uv3 : TEXCOORD3;
                float2 uv4 : TEXCOORD4;
                float2 uv5 : TEXCOORD5;
                float2 uv6 : TEXCOORD6;
                float2 uv7 : TEXCOORD7;
                float4 tangent : TANGENT;
                float3 normal : NORMAL;
                fixed4 color : COLOR;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                fixed4 color : COLOR;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            float2 GetUV(appdata v)
            {
                if(_UVChannel == 0)
                    return v.uv0.xy;
                if(_UVChannel == 1)
                    return v.uv1.xy;
                if(_UVChannel == 2)
                    return v.uv2.xy;
                if(_UVChannel == 3)
                    return v.uv3.xy;
                if(_UVChannel == 4)
                    return v.uv4.xy;
                if(_UVChannel == 5)
                    return v.uv5.xy;
                if(_UVChannel == 6)
                    return v.uv6.xy;
                if(_UVChannel == 7)
                    return v.uv7.xy;
                return v.uv0.xy;
            }

            v2f vertShader (appdata v)
            {
                v2f o;

                if(_Mode == 0)
                {
                    o.uv = GetUV(v);
                    o.vertex = UnityObjectToClipPos(float4(o.uv.x, o.uv.y, 0, 1));
                    o.color = fixed4(1.0, 0.9, 0.6, 1.0);
                }

                if(_Mode > 0)
                {
                    o.vertex = UnityObjectToClipPos(v.vertex);
                }

                if(_Mode == 1)
                {
                    o.color = v.color;
                }

                if(_Mode == 2)
                {
                    o.color = float4(normalize(v.normal.xyz) * 0.5 + 0.5, 1);
                }

                if(_Mode == 3)
                {
                    o.color = float4(normalize(v.tangent.xyz) * 0.5 + 0.5, 1);
                }

                if(_Mode == 4)
                {
                    o.uv = TRANSFORM_TEX(GetUV(v), _MainTex);
                    // a little bit of shading based on object space normal
                    half3 skyColor = half3(0.212, 0.227, 0.259)*4;
                    half3 groundColor = half3(0.047, 0.043, 0.035)*4;
                    float a = v.normal.y * 0.5 + 0.5;
                    o.color.rgb = lerp(groundColor, skyColor, a);
                    o.color.a = 1;
                }

                return o;
            }

            fixed4 fragShader (v2f i) : SV_Target
            {
                if(_Mode == 4)
                {
                    half4 checker = tex2D(_MainTex, i.uv);
                    i.color *= checker;
                }
                return i.color;
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
