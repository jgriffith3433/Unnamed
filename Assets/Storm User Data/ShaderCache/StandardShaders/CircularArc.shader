// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

Shader "PWStorm/Hidden/Handles Circular Arc"
{
//BEGIN STORM EDIT

CGINCLUDE
#pragma multi_compile_instancing
#pragma multi_compile STORM_ENABLED

#include "StormIndirectBase.cginc"
ENDCG

//END STORM EDIT

    CGINCLUDE
    #include "UnityCG.cginc"

    float4 ClipToScreen(float4 v)
    {
        v.xy /= v.w;
        v.xy = v.xy * .5 + .5;
        v.xy *= _ScreenParams.xy;
        return v;
    }

    float4x4 _HandlesMatrix;

    // need to make torus section side constant in screen
    // space, no matter what kind of projection/FOV is used;
    // use a similar code to how editor manipulator handle overall
    // size calculation does (HandleUtility.GetHandleSize)
    float GetHandleSize(float3 pos)
    {
        float4 viewPos1 = float4(UnityObjectToViewPos(mul(_HandlesMatrix, float4(pos, 1))), 1);
        float4 viewPos2 = viewPos1 + float4(1,0,0,0);
        float4 clipPos1 = mul(UNITY_MATRIX_P, viewPos1);
        float4 clipPos2 = mul(UNITY_MATRIX_P, viewPos2);
        float4 screenPos1 = ClipToScreen(clipPos1);
        float4 screenPos2 = ClipToScreen(clipPos2);
        float size = 1.0 / length(screenPos1.xy - screenPos2.xy);
        return size;
    }

    // Rodrigues' rotation formula
    // https://en.wikipedia.org/wiki/Rodrigues%27_rotation_formula
    float3 RotateVector(float3 v, float3 axis, float angleRad)
    {
        float cosA = cos(angleRad);
        float sinA = sin(angleRad);
        if (dot(axis,axis) < 0.0001)
            axis = float3(1,0,0);
        return v * cosA + cross(axis, v) * sinA + axis * dot(axis, v) * (1 - cosA);
    }

    float3 GetCircleOffset(float3 pos, float3 bitangent, float3 tangent, float thickness, float indexAlongSide, float countInSides)
    {
        float size = GetHandleSize(pos);

        tangent = normalize(tangent);
        float sideAngle = indexAlongSide / countInSides * UNITY_TWO_PI;
        float cs = cos(sideAngle);
        float ss = sin(sideAngle);
        float3 res = tangent * cs + bitangent * ss;
        return res * (thickness * size);
    }

    float4 _ArcCenterRadius;
    float4 _ArcNormalAngle;
    float4 _ArcFromCount;
    float4 _ArcThicknessSides;
    float4x4 unity_GUIClipTextureMatrix;

    struct v2f
    {
        float2 clipUV : TEXCOORD0;
        float4 vertex : SV_POSITION;
    };

    v2f VertexTail(float3 vertex)
    {
        v2f o;
        vertex = mul(_HandlesMatrix, float4(vertex,1)).xyz;
        o.vertex = UnityObjectToClipPos(vertex);
        float3 screenUV = UnityObjectToViewPos(vertex);
        o.clipUV = mul(unity_GUIClipTextureMatrix, float4(screenUV.xy, 0, 1));
        return o;
    }

    half4 _Color;
    bool _UseGUIClip;
    sampler2D _GUIClipTexture;

    half4 frag (v2f i) : SV_Target
    {
        half4 col = _Color;
        // if we need to apply IMGUI region clipping, do that
        if (_UseGUIClip)
            col.a *= tex2D(_GUIClipTexture, i.clipUV).a;
        return col;
    }
    ENDCG

    Properties
    {
        _HandleZTest ("_HandleZTest", Int) = 8
    }
    SubShader
    {
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Back
        ZWrite Off
        ZTest [_HandleZTest]

        // 0: draw circular arc
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma require instancing

            float3 CalcDiscSectionPoint(
                float3 center, float3 from, float angle,
                float indexAlongArc, float countInArc,
                float3 normal, float radius,
                float indexAlongSide, float countInSides, float thickness)
            {
                normal = normalize(normal);
                from = normalize(from);
                float rotAngle = angle / (countInArc-1) * indexAlongArc;
                float3 tangent = RotateVector(from * radius, normal, rotAngle);
                float3 onSide = 0;
                if (thickness > 0)
                    onSide = GetCircleOffset(center + tangent, normal, tangent, thickness, indexAlongSide, countInSides);
                return center + tangent + onSide;
            }

            v2f vert (uint vid : SV_VertexID)
            {
                // "thin" arcs are rendered as a line strip, vertex ID
                // in that case is just vertex along the arc length.
                float indexAlongArc = vid;
                float indexAlongSide = 0;
                float thickness = _ArcThicknessSides.x;
                float sides = _ArcThicknessSides.y;
                if (thickness > 0)
                {
                    // "thick" arcs are rendered as a section of a torus,
                    // with an index buffer. vertex ID goes in "sides" circles
                    // along the arc length.
                    uint arcSegment = vid / (uint)sides;
                    uint side = vid % (uint)sides;
                    indexAlongArc = arcSegment;
                    indexAlongSide = side;
                }
                float3 vertex = CalcDiscSectionPoint(
                    _ArcCenterRadius.xyz,
                    _ArcFromCount.xyz,
                    _ArcNormalAngle.w,
                    indexAlongArc,
                    _ArcFromCount.w,
                    _ArcNormalAngle.xyz,
                    _ArcCenterRadius.w,
                    indexAlongSide, sides, thickness);
                return VertexTail(vertex);
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

        // 1: draw thick line
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma require instancing

            v2f vert (uint vid : SV_VertexID)
            {
                // "thick" lines are rendered as a section of a cylinder,
                // with an index buffer. vertex ID goes in "sides" circles
                // along the arc length.
                float thickness = _ArcThicknessSides.x;
                float sides = _ArcThicknessSides.y;

                uint arcSegment = vid / (uint)sides;
                uint side = vid % (uint)sides;
                float indexAlongSide = side;

                // line start position is _ArcCenterRadius.xyz,
                // end position is _ArcFromCount.xyz
                float3 pos1 = _ArcCenterRadius.xyz;
                float3 pos2 = _ArcFromCount.xyz;
                float3 vertex = arcSegment==0 ? pos1 : pos2;

                // get orthonormal frame
                float3 normal = normalize(pos2-pos1);
                float3 tangent = cross(normal, float3(0,1,0));
                if (dot(tangent,tangent) < 0.01)
                    tangent = cross(normal, float3(1,0,0));
                float3 bitangent = normalize(cross(tangent, normal));

                // place vertex so it forms a cylinder along the line
                vertex = vertex + GetCircleOffset(vertex, bitangent, tangent, thickness, indexAlongSide, sides);
                return VertexTail(vertex);
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
