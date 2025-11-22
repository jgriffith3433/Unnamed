// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

// Simplified SDF shader with SSD


Shader "PWStorm/Hidden/TextCore/Distance Field SSD"
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
        _FaceColor("Face Color", Color) = (1,1,1,1)
        _FaceDilate("Face Dilate", Range(-1,1)) = 0

        _OutlineColor("Outline Color", Color) = (0,0,0,1)
        _OutlineWidth("Outline Thickness", Range(0,1)) = 0
        _OutlineSoftness("Outline Softness", Range(0,1)) = 0

        _UnderlayColor("Border Color", Color) = (0,0,0,.5)
        _UnderlayOffsetX("Border OffsetX", Range(-1,1)) = 0
        _UnderlayOffsetY("Border OffsetY", Range(-1,1)) = 0
        _UnderlayDilate("Border Dilate", Range(-1,1)) = 0
        _UnderlaySoftness("Border Softness", Range(0,1)) = 0

        _WeightNormal("Weight Normal", float) = 0
        _WeightBold("Weight Bold", float) = .5

        _ShaderFlags("Flags", float) = 0
        _ScaleRatioA("Scale RatioA", float) = 1
        _ScaleRatioB("Scale RatioB", float) = 1
        _ScaleRatioC("Scale RatioC", float) = 1

        _MainTex("Font Atlas", 2D) = "white" {}
        _TextureWidth("Texture Width", float) = 512
        _TextureHeight("Texture Height", float) = 512
        _GradientScale("Gradient Scale", float) = 5
        _ScaleX("Scale X", float) = 1
        _ScaleY("Scale Y", float) = 1
        _Sharpness("Sharpness", Range(-1, 1)) = 0

        _VertexOffsetX("Vertex OffsetX", float) = 0
        _VertexOffsetY("Vertex OffsetY", float) = 0
    }

    SubShader
    {
        Tags
        {
            "ForceSupported" = "True"
        }

        Lighting Off
        Blend One OneMinusSrcAlpha
        Cull Off
        ZWrite Off
        ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex VertShader
            #pragma fragment PixShader
            //#pragma shader_feature __ OUTLINE_ON
            //#pragma shader_feature __ UNDERLAY_ON UNDERLAY_INNER

            #include "UnityCG.cginc"
            #include "TextCoreProperties.cginc"

            sampler2D _GUIClipTexture;
            uniform float4x4 unity_GUIClipTextureMatrix;

            struct vertex_t {
                float4  vertex          : POSITION;
                fixed4  color           : COLOR;
                float2  texcoord0       : TEXCOORD0;
                float2  texcoord1       : TEXCOORD1;
            };

            struct pixel_t {
                float4  vertex          : SV_POSITION;
                fixed4  faceColor       : COLOR;
                fixed4  outlineColor    : COLOR1;
                float2  texcoord0       : TEXCOORD0;    // Texture UV, Mask UV
                float2  clipUV          : TEXCOORD1;

                //#if (UNDERLAY_ON | UNDERLAY_INNER)
                //float2  texcoord1       : TEXCOORD2;    // Texture UV, alpha, reserved
                //fixed4  underlayColor   : TEXCOORD3;    // Scale(x), Bias(y)
                //#endif
            };


            pixel_t VertShader(vertex_t input)
            {
                float4 vert = input.vertex;
                vert.x += _VertexOffsetX;
                vert.y += _VertexOffsetY;
                float4 vPosition = UnityObjectToClipPos(vert);

                float opacity = input.color.a;
                //#if (UNDERLAY_ON | UNDERLAY_INNER)
                //    opacity = 1.0;
                //#endif

                fixed4 faceColor = fixed4(input.color.rgb, opacity) * _FaceColor;
                faceColor.rgb *= faceColor.a;

                fixed4 outlineColor = _OutlineColor;
                outlineColor.a *= opacity;
                outlineColor.rgb *= outlineColor.a;

                //#if (UNDERLAY_ON | UNDERLAY_INNER)
                //    float4 underlayColor = _UnderlayColor;
                //    underlayColor.a *= opacity;
                //    underlayColor.rgb *= underlayColor.a;
                //
                //    float x = -(_UnderlayOffsetX * _ScaleRatioC) * _GradientScale / _TextureWidth;
                //    float y = -(_UnderlayOffsetY * _ScaleRatioC) * _GradientScale / _TextureHeight;
                //    float2 underlayOffset = float2(x, y);
                //#endif

                // Generate UV for the Clip Texture
                float3 eyePos = UnityObjectToViewPos(input.vertex);
                float2 clipUV = mul(unity_GUIClipTextureMatrix, float4(eyePos.xy, 0, 1.0));

                // Structure for pixel shader
                pixel_t output = {
                    vPosition,
                    faceColor,
                    outlineColor,
                    float2(input.texcoord0.x, input.texcoord0.y),
                    clipUV,
                    //#if (UNDERLAY_ON | UNDERLAY_INNER)
                    //    input.texcoord0 + underlayOffset,
                    //    underlayColor,
                    //#endif
                };

                return output;
            }

            half transition(half2 range, half distance)
            {
                return smoothstep(range.x, range.y, distance);
            }

            // PIXEL SHADER
            fixed4 PixShader(pixel_t input) : SV_Target
            {
                half distanceSample = tex2D(_MainTex, input.texcoord0).a;
                half smoothing = fwidth(distanceSample) * (1 - _Sharpness) + _OutlineSoftness * _ScaleRatioA;
                half contour = 0.5 - _FaceDilate * _ScaleRatioA * 0.5;
                half2 edgeRange = half2(contour - smoothing, contour + smoothing);

                half4 c = input.faceColor;

                //#ifdef OUTLINE_ON
                //    half halfOutlineSize = _OutlineWidth * _ScaleRatioC * 0.5;
                //    half2 faceToOutlineRange = edgeRange + halfOutlineSize;
                //    edgeRange -= halfOutlineSize;
                //
                //    half faceToOutlineTransition = transition(faceToOutlineRange, distanceSample);
                //    c = lerp(input.outlineColor, input.faceColor, faceToOutlineTransition);
                //#endif

                half edgeTransition = transition(edgeRange, distanceSample);
                c *= edgeTransition;

                //#if UNDERLAY_ON
                //    half underlayDistanceSample = tex2D(_MainTex, input.texcoord1).a;
                //    half underlaySmoothing = fwidth(underlayDistanceSample) * 0.5 + _UnderlaySoftness * _ScaleRatioC;
                //    half underlayContour = 0.5 - _UnderlayDilate * _ScaleRatioC * 0.5;
                //    half2 underlayEdgeRange = half2(underlayContour - underlaySmoothing, underlayContour + underlaySmoothing);
                //    half underlayEdgeTransition = transition(underlayEdgeRange, underlayDistanceSample);
                //
                //    c += input.underlayColor * (underlayEdgeTransition * (1 - c.a));
                //#endif

                c *= tex2D(_GUIClipTexture, input.clipUV).a;

                return c;
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
    //CustomEditor "TMPro.EditorUtilities.TMP_SDFShaderGUI"
}
