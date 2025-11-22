// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

Shader "PWStorm/Unlit/Preview3DVolume"
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
        _MainTex ("Texture", 3D) = "white" {}
        _ColorRamp("Color Ramp", 2D) = "white" {}
        _VoxelSize("Voxel Size", Vector) = (1, 1, 1, 1)
        _GlobalScale("Global Scale", Vector) = (1, 1, 1, 1)
        _InvScale("Inverse Scale", Vector) = (1, 1, 1, 1)
        _InvChannels("Inverse Channels", Vector) = (1, 1, 1, 1)
        _InvResolution("Inverse Resolution", Float) = 32
        _Alpha("Alpha Multiplier", Float) = 1
        _Ramp("Color Ramp", Float) = 0
        _FilterMode("Filter Mode", Float) = 0
        _Position("Position", Float) = 0
        _Quality("Quality", Float) = 32
        _IsNormalMap ("", Int) = 0
    }
    SubShader
    {
        Tags { "Queue" = "Transparent" "RenderType" = "Transparent" }
        Blend One OneMinusSrcAlpha
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vertMain
            #pragma fragment fragMain
            #pragma target 3.5

            #include "Preview3DBase.cginc"

            float4 _GlobalScale;
            float3 _InvChannels;
            float _Quality;
            float _InvResolution;
            float _Alpha;
            float _Ramp;
            float _FilterMode;

            // Some rendering methods like using `GL` API doesn't set view matrixes
            // therefore we must set those matrixes manually
            float4x4 _CamToW;
            float4x4 _WToCam;
            float4x4 _ObjToW;
            float4x4 _WToObj;

            v2f vertMain(appdata v, uint vertexID : SV_VertexID, uint instanceID : SV_InstanceID)
            {
                v2f o;
                if (vertexID == 0)
                {
                    v.vertex = float4(-1, -1, 0, 1);
                }
                else if (vertexID == 1)
                {
                    v.vertex = float4(-1, 1, 0, 1);
                }
                else if (vertexID == 2)
                {
                    v.vertex = float4(1, 1, 0, 1);
                }
                else if (vertexID == 3)
                {
                    v.vertex = float4(1, -1, 0, 1);
                }

                v.vertex.z = ((instanceID * _Quality) * 2 - 1);
                v.vertex.xyz *= length(_VoxelSize) * length(_GlobalScale.xyz) / 3;

                v.vertex.xyz = mul((float3x3)_CamToW, v.vertex.xyz);

                float3 objectWorldPos = float3(_ObjToW._m03, _ObjToW._m13, _ObjToW._m23);
                v.vertex.xyz += objectWorldPos;

                o.samplePos = mul(_WToObj, v.vertex).xyz;
                o.vertex = mul(UNITY_MATRIX_P, mul(_WToCam, v.vertex));

                // Cache opacity multiplier with view direction for later use in pixel shader
                o.alphaMul = saturate(_Alpha * 4 * (_Quality / _InvResolution));

                return o;
            }

            half4 fragMain(v2f i) : SV_Target
            {
                i.samplePos += (Noisy3D(uint3(i.vertex.xy, i.vertex.w * 2579)) - float3(0.5, 0.5, 0.5)) * _Quality;

                if (any(i.samplePos < -_VoxelSize / 2))
                    discard;
                if (any(i.samplePos > _VoxelSize / 2))
                    discard;

                float3 uv = i.samplePos * _InvScale + 0.5;

                float4 color = 0;
                if (_FilterMode == 0)
                {
                    color = _MainTex.Sample(custom_point_clamp_sampler, uv);
                }
                else if (_FilterMode == 1)
                {
                    color = _MainTex.Sample(custom_linear_clamp_sampler, uv);
                }
                else if (_FilterMode == 2)
                {
                    color = _MainTex.Sample(custom_trilinear_clamp_sampler, uv);
                }
                if (_IsNormalMap)
                {
                    color.rgb = 0.5f + 0.5f * UnpackNormal(color);
                    color.a = 1;
                }

                color.a *= i.alphaMul;

                if (_Ramp == 1)
                {
                    float prevAlpha = color.a;
                    color = SampleColorRamp(dot(color.rgb, _InvChannels));
                    color.a *= prevAlpha;
                }

                color.rgb *= color.a;

                return color;
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
