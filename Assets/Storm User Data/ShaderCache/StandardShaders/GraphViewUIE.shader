// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

Shader "PWStorm/Hidden/GraphView/GraphViewUIE"
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
        // Establish sensible default values
        [HideInInspector] _MainTex("Atlas", 2D) = "white" {}
        [HideInInspector] _FontTex("Font", 2D) = "black" {}
        [HideInInspector] _CustomTex("Custom", 2D) = "black" {}
        [HideInInspector] _Color("Tint", Color) = (1,1,1,1)
    }

    CGINCLUDE
    #include "EditorUIE.cginc"

    float _GraphViewScale;
    float _EditorPixelsPerPoint;

    v2f ProcessEdge(appdata_t v, inout float4 clipSpacePos)
    {
        UNITY_SETUP_INSTANCE_ID(v);
        uie_vert_load_payload(v);
        v.vertex.xyz = mul(uie_toWorldMat, v.vertex);
        v.uv.xy = mul(uie_toWorldMat, float3(v.uv.xy,0)).xy;

        static const float k_MinEdgeWidth = 1.75f;
        const float halfWidth = length(v.uv.xy);
        const float edgeWidth = halfWidth + halfWidth;
        const float realWidth = max(edgeWidth, k_MinEdgeWidth / _GraphViewScale);
        const float _ZoomCorrection = realWidth / edgeWidth;
        const float _ZoomFactor = _GraphViewScale * _ZoomCorrection * _EditorPixelsPerPoint;
        const float vertexHalfWidth = halfWidth + 1; // One more pixel is enough for our geometric AA
        const float sideSign = v.vertex.z;
        const float2 normal = v.uv.xy * vertexHalfWidth / halfWidth * sideSign; // Thickness direction relengthed to cover one more pixel to give custom AA space to work

        float2 vertex = v.vertex.xy + normal * _ZoomCorrection;

        v2f o;
        UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
        clipSpacePos = UnityObjectToClipPos(float3(vertex.xy, kUIEMeshZ));
        o.clipPos.xy = clipSpacePos.xy / clipSpacePos.w;
        o.uvXY.xy = float2(vertexHalfWidth*sideSign, halfWidth);
        o.uvXY.zw = vertex.xy;
        o.flags.x = 2.0f; // Marking as an edge
        o.flags.y = _ZoomFactor;
        o.flags.zw = (fixed2)0;
        o.svgFlags = (fixed3)0;

        o.clipRectOpacityUVs = uie_std_vert_shader_info(v, o.color);
#if UIE_SHADER_INFO_IN_VS
        o.clipRect = tex2Dlod(_ShaderInfoTex, float4(o.clipRectOpacityUVs.xy, 0, 0)),
#endif // UIE_SHADER_INFO_IN_VS

        o.color.a *= edgeWidth / realWidth; // make up for bigger edge by fading it.
        return o;
    }

    v2f vert(appdata_t v, out float4 clipSpacePos : SV_POSITION)
    {
        if (v.idsFlags.w*255.0f == kUIEVertexLastFlagValue)
            return ProcessEdge(v, clipSpacePos);
        return uie_std_vert(v, clipSpacePos);
    }

    fixed4 frag(v2f IN) : SV_Target
    {
        uie_fragment_clip(IN);
        if (IN.flags.x == 2.0f) // Is it an edge?
        {
            float distanceSat = saturate((IN.uvXY.y - abs(IN.uvXY.x)) * IN.flags.y + 0.5);
            return fixed4(IN.color.rgb, IN.color.a * distanceSat);
        }

        return uie_editor_frag(IN);
    }
    ENDCG

    Category
    {
        Lighting Off
        Blend SrcAlpha OneMinusSrcAlpha

        // Users pass depth between [Near,Far] = [-1,1]. This gets stored on the depth buffer in [Near,Far] [0,1] regardless of the underlying graphics API.
        Cull Off    // Two sided rendering is crucial for immediate clipping
        ZWrite Off
        Stencil
        {
            Ref         255 // 255 for ease of visualization in RenderDoc, but can be just one bit
            ReadMask    255
            WriteMask   255

            CompFront Always
            PassFront Keep
            ZFailFront Replace
            FailFront Keep

            CompBack Equal
            PassBack Keep
            ZFailBack Zero
            FailBack Keep
        }

        Tags
        {
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
            "RenderType" = "Transparent"
            "PreviewType" = "Plane"
        }

        // SM3.5 version
        SubShader
        {
            Tags { "UIE_VertexTexturingIsAvailable" = "1" }
            Pass
            {
                CGPROGRAM
                #pragma target 3.5
                #pragma vertex vert
                #pragma fragment frag
                #pragma require samplelod
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

        // SM2.0 version
        SubShader
        {
            Pass
            {
                CGPROGRAM
                #pragma target 2.0
                #pragma vertex vert
                #pragma fragment frag
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
    } // Category
}
