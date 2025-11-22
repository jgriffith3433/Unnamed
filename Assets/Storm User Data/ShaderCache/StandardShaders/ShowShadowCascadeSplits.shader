// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

// Visualizes shadow cascade splits in the editor.
// Follows the logic of Internal-ScreenSpaceShadows fairly closely.

Shader "PWStorm/Hidden/ShowShadowCascadeSplits" {
//BEGIN STORM EDIT

CGINCLUDE
#pragma multi_compile_instancing
#pragma multi_compile STORM_ENABLED

#include "StormIndirectBase.cginc"
ENDCG

//END STORM EDIT


CGINCLUDE
#include "UnityCG.cginc"

struct appdata {
    float4 vertex : POSITION;
    float2 texcoord : TEXCOORD0;
#ifdef UNITY_STEREO_INSTANCING_ENABLED
    float3 ray[2] : TEXCOORD1;
#else
    float3 ray : TEXCOORD1;
#endif
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct v2f {

    float4 pos : SV_POSITION;

    // xy uv / zw screenpos
    float4 uv : TEXCOORD0;
    // View space ray, for perspective case
    float3 ray : TEXCOORD1;
    // Orthographic view space positions (need xy as well for oblique matrices)
    float3 orthoPosNear : TEXCOORD2;
    float3 orthoPosFar  : TEXCOORD3;
    UNITY_VERTEX_INPUT_INSTANCE_ID
        UNITY_VERTEX_OUTPUT_STEREO
};

v2f vert(appdata v)
{
    v2f o;
    UNITY_SETUP_INSTANCE_ID(v);
    UNITY_TRANSFER_INSTANCE_ID(v, o);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
    float4 clipPos = UnityObjectToClipPos(v.vertex);
    o.pos = clipPos;
    o.uv.xy = v.texcoord;

    // unity_CameraInvProjection at the PS level.
    o.uv.zw = ComputeNonStereoScreenPos(clipPos);

    // Perspective case
#ifdef UNITY_STEREO_INSTANCING_ENABLED
    o.ray = v.ray[unity_StereoEyeIndex];
#else
    o.ray = v.ray;
#endif

    // To compute view space position from Z buffer for orthographic case,
    // we need different code than for perspective case. We want to avoid
    // doing matrix multiply in the pixel shader: less operations, and less
    // constant registers used. Particularly with constant registers, having
    // unity_CameraInvProjection in the pixel shader would push the PS over SM2.0
    // limits.
    clipPos.y *= _ProjectionParams.x;
    float3 orthoPosNear = mul(unity_CameraInvProjection, float4(clipPos.x, clipPos.y, -1, 1)).xyz;
    float3 orthoPosFar = mul(unity_CameraInvProjection, float4(clipPos.x, clipPos.y, 1, 1)).xyz;
    orthoPosNear.z *= -1;
    orthoPosFar.z *= -1;
    o.orthoPosNear = orthoPosNear;
    o.orthoPosFar = orthoPosFar;

    return o;
}

UNITY_DECLARE_DEPTH_TEXTURE(_CameraDepthTexture);

UNITY_DECLARE_SHADOWMAP(_ShadowMapTexture);

//
// Keywords based defines
//
#if defined (SHADOWS_SPLIT_SPHERES)
    #define GET_CASCADE_WEIGHTS(wpos, z)    getCascadeWeights_splitSpheres(wpos)
#else
    #define GET_CASCADE_WEIGHTS(wpos, z)    getCascadeWeights( wpos, z )
#endif

// prototypes
inline fixed4 getCascadeWeights(float3 wpos, float z);      // calculates the cascade weights based on the world position of the fragment and plane positions
inline fixed4 getCascadeWeights_splitSpheres(float3 wpos);  // calculates the cascade weights based on world pos and split spheres positions

/**
 * Gets the cascade weights based on the world position of the fragment.
 * Returns a float4 with only one component set that corresponds to the appropriate cascade.
 */
inline fixed4 getCascadeWeights(float3 wpos, float z)
{
    fixed4 zNear = float4( z >= _LightSplitsNear );
    fixed4 zFar = float4( z < _LightSplitsFar );
    fixed4 weights = zNear * zFar;
    return weights;
}

/**
 * Gets the cascade weights based on the world position of the fragment and the poisitions of the split spheres for each cascade.
 * Returns a float4 with only one component set that corresponds to the appropriate cascade.
 */
inline fixed4 getCascadeWeights_splitSpheres(float3 wpos)
{
    float3 fromCenter0 = wpos.xyz - unity_ShadowSplitSpheres[0].xyz;
    float3 fromCenter1 = wpos.xyz - unity_ShadowSplitSpheres[1].xyz;
    float3 fromCenter2 = wpos.xyz - unity_ShadowSplitSpheres[2].xyz;
    float3 fromCenter3 = wpos.xyz - unity_ShadowSplitSpheres[3].xyz;
    float4 distances2 = float4(dot(fromCenter0,fromCenter0), dot(fromCenter1,fromCenter1), dot(fromCenter2,fromCenter2), dot(fromCenter3,fromCenter3));
    fixed4 weights = float4(distances2 < unity_ShadowSplitSqRadii);
    weights.yzw = saturate(weights.yzw - weights.xyz);
    return weights;
}


fixed4 frag (v2f i) : SV_Target
{
    float zdepth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.uv);
    float linearDepth = Linear01Depth(zdepth);
#if defined(UNITY_REVERSED_Z)
    zdepth = 1 - zdepth;
#endif

    // 0..1 linear depth, 0 at near plane, 1 at far plane.
    float depth = lerp (linearDepth, zdepth, unity_OrthoParams.w);

    // view position calculation for perspective & ortho cases
    float3 vposPersp = i.ray * depth;
    float3 vposOrtho = lerp(i.orthoPosNear, i.orthoPosFar, zdepth);
    // pick the perspective or ortho position as needed
    float3 vpos = lerp (vposPersp, vposOrtho, unity_OrthoParams.w);

    float4 wpos = mul (unity_CameraToWorld, float4(vpos,1));

    float4 cascadeWeights = GET_CASCADE_WEIGHTS (wpos, vpos.z);
    float cascadeIndex = dot(cascadeWeights, float4(1,2,3,4)) - 1.0; // beyond shadow distance will end up -1
    fixed4 color = fixed4(1,1,1,0);
    if (cascadeIndex >= 0.0)
    {
        // Slightly intensified colors of ShadowCascadeSplitGUI
        const fixed4 kCascadeColors[4] =
        {
            fixed4(0.5, 0.5, 0.7, 0.5),
            fixed4(0.5, 0.7, 0.5, 0.5),
            fixed4(0.7, 0.7, 0.5, 0.5),
            fixed4(0.7, 0.5, 0.5, 0.5),
        };
        color = kCascadeColors[cascadeIndex];

        /* no blending yet
        // cascade blending (5% of cascade range blended with previous cascade)
        #if !defined (SHADOWS_SPLIT_SPHERES)
            float4 num = (float4(vpos.z,vpos.z,vpos.z,vpos.z) - _LightSplitsNear);
            float4 denom = (_LightSplitsFar - _LightSplitsNear);
            float alpha = dot( (num / denom) * cascadeWeights, float4(1,1,1,1));
            if (alpha < 0.05f && cascadeIndex != 0.0)
            {
                color = lerp(kCascadeColors[cascadeIndex-1], color, alpha*20.0f);
            }
        #endif
        */
    }
    return color;
}
ENDCG

SubShader {
    Pass {
        ZWrite Off ZTest Always Cull Off
        Blend SrcAlpha OneMinusSrcAlpha

        CGPROGRAM
        #pragma vertex vert
        #pragma fragment frag
        #pragma target 2.0
        #pragma multi_compile_shadowcollector
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
