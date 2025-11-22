// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

Shader "PWStorm/Hidden/TerrainEngine/GenerateNormalmap" {
//BEGIN STORM EDIT

CGINCLUDE
#pragma multi_compile_instancing
#pragma multi_compile STORM_ENABLED

#include "StormIndirectBase.cginc"
ENDCG

//END STORM EDIT

    Properties { _MainTex ("Texture", any) = "" {} }
    SubShader {
        Pass {
            ZTest Always Cull Off ZWrite Off

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment CalculateNormalSobel
            #pragma target 3.0

            #include "UnityCG.cginc"

            sampler2D _MainTex;
            uniform float4 _MainTex_ST;
            uniform float4 _TerrainNormalmapGenSize;    // (1.0f / (float)m_Width, 1.0f / (float)m_Height, 1.0f / hmScale.x, 1.0f / hmScale.z);
            uniform float4 _TerrainTilesScaleOffsets[9]; // ((65535.0f / kMaxHeight) * hmScale.y, terrainPosition.y, 0.0f, 0.0f)

            struct appdata_t {
                float4 vertex : POSITION;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f {
                float4 vertex : SV_POSITION;
                float4 bt   : TEXCOORD0;
                float4 lr   : TEXCOORD1;
                float4 blbr : TEXCOORD2;
                float4 tltr : TEXCOORD3;
            };

            v2f vert (appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.bt   = float4(v.texcoord.x, v.texcoord.y - _TerrainNormalmapGenSize.y, v.texcoord.x, v.texcoord.y + _TerrainNormalmapGenSize.y);
                o.lr   = float4(v.texcoord.x - _TerrainNormalmapGenSize.x, v.texcoord.y, v.texcoord.x + _TerrainNormalmapGenSize.x, v.texcoord.y);
                o.blbr = float4(v.texcoord.x - _TerrainNormalmapGenSize.x, v.texcoord.y - _TerrainNormalmapGenSize.y, v.texcoord.x + _TerrainNormalmapGenSize.x, v.texcoord.y - _TerrainNormalmapGenSize.y);
                o.tltr = float4(v.texcoord.x - _TerrainNormalmapGenSize.x, v.texcoord.y + _TerrainNormalmapGenSize.y, v.texcoord.x + _TerrainNormalmapGenSize.x, v.texcoord.y + _TerrainNormalmapGenSize.y);
                return o;
            }

            int ComputeTileIndex(float2 texcoord)
            {
                int2 idx = int2(floor(texcoord + 1.0f));
                return idx.y * 3 + idx.x;
            }

            float4 CalculateNormalSobel (v2f i) : SV_Target
            {
                float2 scaleOffsetB  = _TerrainTilesScaleOffsets[ComputeTileIndex(i.bt.xy)];
                float2 scaleOffsetT  = _TerrainTilesScaleOffsets[ComputeTileIndex(i.bt.zw)];
                float2 scaleOffsetL  = _TerrainTilesScaleOffsets[ComputeTileIndex(i.lr.xy)];
                float2 scaleOffsetR  = _TerrainTilesScaleOffsets[ComputeTileIndex(i.lr.zw)];
                float2 scaleOffsetBL = _TerrainTilesScaleOffsets[ComputeTileIndex(i.blbr.xy)];
                float2 scaleOffsetBR = _TerrainTilesScaleOffsets[ComputeTileIndex(i.blbr.zw)];
                float2 scaleOffsetTL = _TerrainTilesScaleOffsets[ComputeTileIndex(i.tltr.xy)];
                float2 scaleOffsetTR = _TerrainTilesScaleOffsets[ComputeTileIndex(i.tltr.zw)];

                float2 d;

                // Do X sobel filter
                d.x  = (UnpackHeightmap(tex2D(_MainTex, TRANSFORM_TEX(i.blbr.xy, _MainTex))) * scaleOffsetBL.x + scaleOffsetBL.y) * -1.0F;
                d.x += (UnpackHeightmap(tex2D(_MainTex, TRANSFORM_TEX(i.lr.xy, _MainTex)))   * scaleOffsetL.x  + scaleOffsetL.y)  * -2.0F;
                d.x += (UnpackHeightmap(tex2D(_MainTex, TRANSFORM_TEX(i.tltr.xy, _MainTex))) * scaleOffsetTL.x + scaleOffsetTL.y) * -1.0F;
                d.x += (UnpackHeightmap(tex2D(_MainTex, TRANSFORM_TEX(i.blbr.zw, _MainTex))) * scaleOffsetBR.x + scaleOffsetBR.y) *  1.0F;
                d.x += (UnpackHeightmap(tex2D(_MainTex, TRANSFORM_TEX(i.lr.zw, _MainTex)))   * scaleOffsetR.x  + scaleOffsetR.y)  *  2.0F;
                d.x += (UnpackHeightmap(tex2D(_MainTex, TRANSFORM_TEX(i.tltr.zw, _MainTex))) * scaleOffsetTR.x + scaleOffsetTR.y) *  1.0F;

                // Do Y sobel filter
                d.y  = (UnpackHeightmap(tex2D(_MainTex, TRANSFORM_TEX(i.blbr.xy, _MainTex))) * scaleOffsetBL.x + scaleOffsetBL.y) * -1.0F;
                d.y += (UnpackHeightmap(tex2D(_MainTex, TRANSFORM_TEX(i.bt.xy, _MainTex)))   * scaleOffsetB.x  + scaleOffsetB.y)  * -2.0F;
                d.y += (UnpackHeightmap(tex2D(_MainTex, TRANSFORM_TEX(i.blbr.zw, _MainTex))) * scaleOffsetBR.x + scaleOffsetBR.y) * -1.0F;
                d.y += (UnpackHeightmap(tex2D(_MainTex, TRANSFORM_TEX(i.tltr.xy, _MainTex))) * scaleOffsetTL.x + scaleOffsetTL.y) *  1.0F;
                d.y += (UnpackHeightmap(tex2D(_MainTex, TRANSFORM_TEX(i.bt.zw, _MainTex)))   * scaleOffsetT.x  + scaleOffsetT.y)  *  2.0F;
                d.y += (UnpackHeightmap(tex2D(_MainTex, TRANSFORM_TEX(i.tltr.zw, _MainTex))) * scaleOffsetTR.x + scaleOffsetTR.y) *  1.0F;
                d *= _TerrainNormalmapGenSize.zw;

                // Cross Product of components of gradient reduces to
                float3 normal = float3(-d.x, 8.0f, -d.y);
                return float4(0.5f + 0.5f * normalize(normal), 1.0f);
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
