// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

Shader "PWStorm/Hidden/TerrainEngine/CrossBlendNeighbors"
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
        _TopTex ("Top Texture", any) = "black" {}
        _BottomTex ("Bottom Texture", any) = "black" {}
        _LeftTex ("Left Texture", any) = "black" {}
        _RightTex ("Right Texture", any) = "black" {}
    }
    SubShader
    {
        Pass
        {
            ZTest Always Cull Off ZWrite Off

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment CrossBlendNeighbors
            #pragma target 3.0

            #include "UnityCG.cginc"

            uniform float4 _TexCoordOffsetScale;
            uniform float4 _Offsets; // bottom, top, left, right
            uniform float4 _SlopeEnableFlags; // bottom, top, left, right; 0.0f - neighbor exists, 1.0f - no neighbor
            uniform float  _AddressMode; // 0.0f - clamp, 1.0f - mirror

            sampler2D _TopTex;
            sampler2D _BottomTex;
            sampler2D _LeftTex;
            sampler2D _RightTex;

            struct appdata_t
            {
                float4 vertex : POSITION;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float4 texcoord : TEXCOORD0;
            };

            v2f vert (appdata_t v)
            {
                v2f o;

                o.vertex = UnityObjectToClipPos(v.vertex);
                o.texcoord.xy = v.texcoord;
                o.texcoord.zw = (v.texcoord + _TexCoordOffsetScale.xy) * _TexCoordOffsetScale.zw;
                return o;
            }

            float4 CrossBlendNeighbors(v2f i) : SV_Target
            {
                // All slope offset data is static, but we calculate it on GPU because we don't want to access height data on CPU
                float2 topSlope    = float2(UnpackHeightmap(tex2Dlod(_LeftTex,   float4(1.0f, 1.0f, 0.0f, 0.0f))), UnpackHeightmap(tex2Dlod(_RightTex, float4(0.0f, 1.0f, 0.0f, 0.0f)))) + _Offsets.zw;
                float2 bottomSlope = float2(UnpackHeightmap(tex2Dlod(_LeftTex,   float4(1.0f, 0.0f, 0.0f, 0.0f))), UnpackHeightmap(tex2Dlod(_RightTex, float4(0.0f, 0.0f, 0.0f, 0.0f)))) + _Offsets.zw;
                float2 leftSlope   = float2(UnpackHeightmap(tex2Dlod(_BottomTex, float4(0.0f, 1.0f, 0.0f, 0.0f))), UnpackHeightmap(tex2Dlod(_TopTex,   float4(0.0f, 0.0f, 0.0f, 0.0f)))) + _Offsets.xy;
                float2 rightSlope  = float2(UnpackHeightmap(tex2Dlod(_BottomTex, float4(1.0f, 1.0f, 0.0f, 0.0f))), UnpackHeightmap(tex2Dlod(_TopTex,   float4(1.0f, 0.0f, 0.0f, 0.0f)))) + _Offsets.xy;
                float2 topSlopeOffset    = _Offsets.y + _SlopeEnableFlags.y * topSlope;
                float2 bottomSlopeOffset = _Offsets.x + _SlopeEnableFlags.x * bottomSlope;
                float2 leftSlopeOffset   = _Offsets.z + _SlopeEnableFlags.z * leftSlope;
                float2 rightSlopeOffset  = _Offsets.w + _SlopeEnableFlags.w * rightSlope;

                float2 blendPos = saturate(i.texcoord.zw);

                float4 weights = 1.0f / max(float4(1.0f - blendPos.y, blendPos.y, blendPos.x, 1.0f - blendPos.x), 0.0000001f);
                weights /= dot(weights, 1.0f);

                float4 heights = float4(
                    UnpackHeightmap(tex2D(_TopTex,    float2(i.texcoord.x, (1.0f - i.texcoord.y) * _AddressMode.x))),
                    UnpackHeightmap(tex2D(_BottomTex, float2(i.texcoord.x,  1.0f - i.texcoord.y  * _AddressMode.x))),
                    UnpackHeightmap(tex2D(_LeftTex,   float2( 1.0f - i.texcoord.x  * _AddressMode.x, i.texcoord.y))),
                    UnpackHeightmap(tex2D(_RightTex,  float2((1.0f - i.texcoord.x) * _AddressMode.x, i.texcoord.y)))
                );

                heights += float4(
                    lerp(topSlopeOffset.x,    topSlopeOffset.y, blendPos.x),
                    lerp(bottomSlopeOffset.x, bottomSlopeOffset.y, blendPos.x),
                    lerp(leftSlopeOffset.x,   leftSlopeOffset.y, blendPos.y),
                    lerp(rightSlopeOffset.x,  rightSlopeOffset.y, blendPos.y)
                );

                return PackHeightmap(dot(heights, weights));
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
