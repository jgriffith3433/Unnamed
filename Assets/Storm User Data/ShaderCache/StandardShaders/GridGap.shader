// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

Shader "PWStorm/Hidden/SceneView/GridGap" {
//BEGIN STORM EDIT

CGINCLUDE
#pragma multi_compile_instancing
#pragma multi_compile STORM_ENABLED

#include "StormIndirectBase.cginc"
ENDCG

//END STORM EDIT


    Properties {
        _Gap("Gap", Vector) = (0, 0, 0, 0)
        _Stride("Stride", Vector) = (0, 0, 0, 0)
    }

    SubShader {
        Tags { "ForceSupported" = "True" "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off Cull Off Fog { Mode Off }
        Pass {
            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag
            #pragma target 4.0
            #include "UnityCG.cginc"

            struct vertexInput {
                float4 vertex       : POSITION;     // current point position
                float4 otherVertex  : NORMAL;       // other point position
                float2 texcoord0    : TEXCOORD0;
                float2 value        : TEXCOORD1;    // current point value
                float2 otherValue   : TEXCOORD2;    // other point value
                fixed4 color        : COLOR;
            };

            struct fragmentInput {
                float4 position                     : SV_POSITION;
                float2 texcoord0                    : TEXCOORD0;
                nointerpolation float2 value        : TEXCOORD1;    // current point value
                nointerpolation float2 otherValue   : TEXCOORD2;    // other point value
                nointerpolation float4 clipPos      : TEXCOORD3;    // current point clip position
                nointerpolation float4 otherClipPos : TEXCOORD4;    // other point clip position
                float3 screenPos                    : TEXCOORD5;
                fixed4 color                        : COLOR;
            };

            float4 _Gap;
            float4 _Stride;

            fragmentInput vert(vertexInput i) {
                fragmentInput o;
                o.position = UnityObjectToClipPos(i.vertex);
                o.texcoord0 = i.texcoord0;
                o.value = i.value;
                o.otherValue = i.otherValue;
                o.clipPos = o.position;
                o.otherClipPos = UnityObjectToClipPos(i.otherVertex);
                o.screenPos = UnityObjectToViewPos(i.vertex);
                o.color = i.color;
                return o;
            }

            float4 frag(fragmentInput i) : COLOR {

                float3 ndc = float3(i.position.x / _ScreenParams.x, i.position.y / _ScreenParams.y, i.position.z);
                float3 clipPos;

#if defined(SHADER_API_GLCORE)
                clipPos = ndc * 2.0 - 1.0;
#else
                ndc.y = 1.0 - ndc.y;
                clipPos.xy = ndc.xy * 2.0 - 1.0;
                clipPos.z = ndc.z;
#endif

                clipPos *= i.position.w;

                // This is to workaround an interpolator precision issue on some AMD GPUs.
                // Instead of relying on the value produced by the hardware interpolator (which doesn't have sufficient precision for our use case),
                // we do the interpolation manually here in the pixel shader using the clip space positions of the current pixel and the two end points of the line.
                // Note that because we are doing the interpolation ourselves, 'nointerpolation' modifier is used on some vertex attributes.
                float fullDistance = length(i.otherClipPos.xyz - i.clipPos.xyz);
                float distanceToOther = length(i.otherClipPos.xyz - clipPos);
                float2 interpValue = lerp(i.otherValue, i.value, distanceToOther / fullDistance);

                float m = 1.0;
                float2 r = fmod(interpValue, _Stride.xy);
                if (any(r > _Gap.xy))
                    m = 0.0;
                m = m * saturate(i.texcoord0.x / -i.screenPos.z - 0.1);
                return fixed4(i.color.r, i.color.g, i.color.b, m * i.color.a);
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
