// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

Shader "PWStorm/Hidden/BlitCopyHDRTonemap" {
//BEGIN STORM EDIT

CGINCLUDE
#pragma multi_compile_instancing
#pragma multi_compile STORM_ENABLED

#include "StormIndirectBase.cginc"
ENDCG

//END STORM EDIT

    Properties
    {
        _MainTex ("Texture", any) = "" {}
        _NitsForPaperWhite("NitsForPaperWhite", Float) = 160.0
        _ColorGamut("ColorGamut", Int) = 0
    }
    SubShader {
        Pass {
            ZTest Always Cull Off ZWrite Off

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            UNITY_DECLARE_SCREENSPACE_TEXTURE(_MainTex);
            uniform float4 _MainTex_ST;
            uniform float _NitsForPaperWhite;
            uniform int _ColorGamut;

            struct appdata_t {
                float4 vertex : POSITION;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f {
                float4 vertex : SV_POSITION;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            v2f vert (appdata_t v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.texcoord = TRANSFORM_TEX(v.texcoord.xy, _MainTex);
                return o;
            }

            // These values must match the ColorGamut enum in ColorGamut.h
            #define kColorGamutSRGB         0
            #define kColorGamutRec709       1
            #define kColorGamutRec2020      2
            #define kColorGamutDisplayP3    3
            #define kColorGamutHDR10        4
            #define kColorGamutDolbyHDR     5

            float3 LinearToSRGB(float3 color)
            {
                // Approximately pow(color, 1.0 / 2.2)
                return color < 0.0031308 ? 12.92 * color : 1.055 * pow(abs(color), 1.0 / 2.4) - 0.055;
            }

            float3 SRGBToLinear(float3 color)
            {
                // Approximately pow(color, 2.2)
                return color < 0.04045 ? color / 12.92 : pow(abs(color + 0.055) / 1.055, 2.4);
            }

            static const float3x3 Rec709ToRec2020 =
            {
                0.627402, 0.329292, 0.043306,
                0.069095, 0.919544, 0.011360,
                0.016394, 0.088028, 0.895578
            };

            static const float3x3 Rec2020ToRec709 =
            {
                1.660496, -0.587656, -0.072840,
                -0.124547, 1.132895, -0.008348,
                -0.018154, -0.100597, 1.118751
            };

            float3 LinearToST2084(float3 color)
            {
                float m1 = 2610.0 / 4096.0 / 4;
                float m2 = 2523.0 / 4096.0 * 128;
                float c1 = 3424.0 / 4096.0;
                float c2 = 2413.0 / 4096.0 * 32;
                float c3 = 2392.0 / 4096.0 * 32;
                float3 cp = pow(abs(color), m1);
                return pow((c1 + c2 * cp) / (1 + c3 * cp), m2);
            }

            float4 frag (v2f i) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
                // The scene is rendered with linear gamma and Rec.709 primaries. (DXGI_COLOR_SPACE_RGB_FULL_G10_NONE_P709)
                float4 scene = UNITY_SAMPLE_SCREENSPACE_TEXTURE(_MainTex, i.texcoord);
                float3 result = IsGammaSpace() ? float3(GammaToLinearSpaceExact(scene.r), GammaToLinearSpaceExact(scene.g), GammaToLinearSpaceExact(scene.b)) : scene.rgb;

                if (_ColorGamut == kColorGamutSRGB)
                {
                    result = LinearToSRGB(result);
                }
                else if (_ColorGamut == kColorGamutHDR10)
                {
                    const float st2084max = 10000.0;
                    const float hdrScalar = _NitsForPaperWhite / st2084max;
                    // The HDR scene is in Rec.709, but the display is Rec.2020
                    result = mul(Rec709ToRec2020, result);
                    // Apply the ST.2084 curve to the scene.
                    result = LinearToST2084(result * hdrScalar);
                }
                else // _ColorGamut == kColorGamutRec709
                {
                    // Just pass through
                }

                return float4(result.rgb, scene.a);
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
    Fallback Off
}
