// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

Shader "PWStorm/Unlit/Preview3DSDF"
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
        _MainTex("Texture", 3D) = "white" {}
        _ColorRamp("Color Ramp", 2D) = "white" {}
        _VoxelSize("Voxel Size", Vector) = (1, 1, 1, 1)
        _InvScale("Inverse Scale", Vector) = (1, 1, 1, 1)
        _GlobalScale("Global Scale", Vector) = (1, 1, 1, 1)
        _InvResolution("Inverse Resolution", Float) = 1
        _Scale("Scale", Float) = 1
        _Offset("Offset", Float) = 0
        _IsNormalMap ("", Int) = 0
    }

    SubShader
    {
        Tags { "Queue" = "Transparent" "RenderType" = "Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        LOD 100

        Pass
        {
            Cull Front

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Preview3DBase.cginc"

            float4 _GlobalScale;
            float _InvResolution;
            float _Scale;
            float _Offset;

            float sampleSurface(float3 pos)
            {
                return (_MainTex.Sample(sampler_MainTex, pos).x + _Offset * _InvResolution) * _Scale * _InvResolution;
            }

            f2s raymarch(f2s fragOut, float3 origin, float3 direction, float2 minmaxt, float minSurfaceDist)
            {
                float t = minmaxt.x;
                UNITY_LOOP for (int it = 0; it < 2048 && t < minmaxt.y; it++)
                {
                    float3 position = origin + direction * t;
                    float3 scaledPosition = position * _InvScale;
                    float sampleDistance = sampleSurface(scaledPosition + float3(0.5, 0.5, 0.5));
                    t += sampleDistance;

                    if (sampleDistance < minSurfaceDist)
                    {
                        float3 deltaShift = 2 * _InvScale * _InvResolution;

                        float3 delta = float3(sampleSurface(scaledPosition + float3(0.5 + deltaShift.x, 0.5, 0.5)),
                            sampleSurface(scaledPosition + float3(0.5, 0.5 + deltaShift.y, 0.5)),
                            sampleSurface(scaledPosition + float3(0.5, 0.5, 0.5 + deltaShift.z))) - sampleDistance;

                        float3 normal = normalize(float3(delta.x / deltaShift.x, delta.y / deltaShift.y, delta.z / deltaShift.z));

                        float3 eyeNormal = normalize(mul((float3x3)UNITY_MATRIX_IT_MV, normal).xyz);

                        float3 farPoint = -direction * length(_GlobalScale) / 2;
                        float3 nearPoint = -farPoint;
                        float ratio = length(position - nearPoint) / length(farPoint - nearPoint);
                        fragOut.color.rgb = SampleColorRamp(1 - ratio);

                        float rim = clamp(pow(1 - abs(eyeNormal.z), 3), 0, 1);
                        fragOut.color.rgb = lerp(fragOut.color.rgb, float3(0.1, 0.1, 0.1), rim);

                        float4 clipPos = UnityObjectToClipPos(origin + direction * max(t, 0.1f));
                        fragOut.depth = clipPos.z / clipPos.w;
                        fragOut.color.a = 1;
                        break;
                    }
                }

                return fragOut;
            }

            f2s frag(v2f i)
            {
                f2s fragOut;
                fragOut.color = float4(0, 0, 0, 0);
                fragOut.depth = 0;

                float3 rayOrigin = _WorldSpaceCameraPos;
                float3 rayDirection = normalize(i.samplePos.xyz - rayOrigin);

                rayOrigin = mul(unity_WorldToObject, float4(rayOrigin, 1)).xyz;
                rayDirection = mul(unity_WorldToObject, float4(rayDirection, 0)).xyz;

                float minSurfaceDist = pow(_InvResolution, 2);

                float2 isect = RayBoxIntersection(rayOrigin, rayDirection, 0.5 * _VoxelSize);
                if (isect.y < 0.0)
                {
                    fragOut.color = 1 - fragOut.color;
                }
                else
                {
                    isect.x = max(isect.x, 0.0);
                    fragOut = raymarch(fragOut, rayOrigin, rayDirection, isect, minSurfaceDist);
                }

                return fragOut;
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
