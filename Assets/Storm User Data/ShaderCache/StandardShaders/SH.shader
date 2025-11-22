// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

// Visualise Spherical Harmonics
// sampled with vertex normal
Shader "PWStorm/Hidden/SH" {
//BEGIN STORM EDIT

CGINCLUDE
#pragma multi_compile_instancing
#pragma multi_compile STORM_ENABLED

#include "StormIndirectBase.cginc"
ENDCG

//END STORM EDIT

Properties {
    _HandleSize ("HandleSize", Float) = 0.5
    _IsInGammaPipeline("IsInGammaPipeline", Float) = 1.0
    _Exposure ("Exposure", 2D) = "white" {}
}
SubShader {

CGINCLUDE
    #pragma vertex vert
    #pragma fragment frag
    #pragma target 2.0
    #pragma multi_compile __ UNITY_COLORSPACE_GAMMA
    #include "UnityCG.cginc"

    float _HandleSize;
    float _IsInGammaPipeline;
    sampler2D _Exposure;

    struct v2f {
        float4 pos : SV_POSITION;
        float3 color : COLOR0;
    };

    float3 Shade4DirLights (
        float4 toLightX, float4 toLightY, float4 toLightZ,
        float3 lightColor0, float3 lightColor1, float3 lightColor2, float3 lightColor3,
        float4 atten, float3 normal)
    {
        // squared lengths
        float4 lengthSq = 0;
        lengthSq += toLightX * toLightX;
        lengthSq += toLightY * toLightY;
        lengthSq += toLightZ * toLightZ;
        // NdotL
        float4 ndotl = 0;
        ndotl += toLightX * normal.x;
        ndotl += toLightY * normal.y;
        ndotl += toLightZ * normal.z;
        // correct NdotL
        float4 corr = rsqrt(lengthSq);
        ndotl = max (float4(0,0,0,0), ndotl * corr);
        // attenuation
        float4 diff = ndotl * atten;
        // final color
        float3 col = 0;
        col += lightColor0 * diff.x;
        col += lightColor1 * diff.y;
        col += lightColor2 * diff.z;
        col += lightColor3 * diff.w;
        return col;
    }

    v2f vert (appdata_base v)
    {
        v2f o;
        half4 pos = v.vertex;
        pos.xyz *= _HandleSize;
        o.pos = UnityObjectToClipPos(pos);

        // render SH
        // case 862215: We should instead rely on platform defines.
        // however at the moment this shader is compiled only once
        // at editor resources build time.
        // TODO remove workaround when "editor resources" will be built on-demands.
        half4 normal = half4(v.normal, 1);
        o.color = SHEvalLinearL0L1(normal) + SHEvalLinearL2(normal);
        o.color = (_IsInGammaPipeline > 0.0)?LinearToGammaSpace(o.color):o.color;

        // render occluded mixed lights on top of SH
        o.color += Shade4DirLights(
            unity_4LightPosX0, unity_4LightPosY0, unity_4LightPosZ0,
            unity_LightColor0.rgb, unity_LightColor1.rgb, unity_LightColor2.rgb, unity_LightColor3.rgb,
            unity_4LightAtten0,
            v.normal);

        float exposure = tex2Dlod(_Exposure, float4(0.5, 0.5, 0, 0)).r;
        o.color *= exposure;

        return o;
    }

ENDCG

    Tags { "ForceSupported" = "True" }
    Pass {

        CGPROGRAM
        half4 frag (v2f i) : COLOR
        {
            return half4 (i.color, 1);
        }
        ENDCG
    }

    // Same as the first pass, just with disabled z testing
    Pass {
        ZTest Always
        CGPROGRAM
#pragma target 2.0
        half4 frag (v2f i) : COLOR
        {
            return half4 (i.color, 1);
        }
        ENDCG
    }

//BEGIN STORM EDIT

CGPROGRAM

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
ENDCG

//END STORM EDIT
}

}
