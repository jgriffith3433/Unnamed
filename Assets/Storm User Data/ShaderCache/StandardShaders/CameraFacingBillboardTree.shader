// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

Shader "PWStorm/Hidden/TerrainEngine/CameraFacingBillboardTree" {
//BEGIN STORM EDIT

CGINCLUDE
#pragma multi_compile_instancing
#pragma multi_compile STORM_ENABLED

#include "StormIndirectBase.cginc"
ENDCG

//END STORM EDIT

    Properties{
        _MainTex("Base (RGB) Alpha (A)", 2D) = "white" {}
        _NormalTex("Base (RGB) Alpha (A)", 2D) = "white" {}
        _TranslucencyViewDependency("View dependency", Range(0,1)) = 0.7
        _TranslucencyColor("Translucency Color", Color) = (0.73,0.85,0.41,1)
        _AlphaToMask("AlphaToMask", Float) = 1.0 // On
    }
        SubShader{
            Tags {
                "IgnoreProjector" = "True" "RenderType" = "TreeBillboard" }

            Pass {
                ColorMask rgb
                ZWrite On Cull Off
                AlphaToMask [_AlphaToMask]

                CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag
                #pragma multi_compile_fog
                #include "UnityCG.cginc"
                #include "UnityBuiltin3xTreeLibrary.cginc"
#if SHADER_API_D3D11 || SHADER_API_GLCORE
#define ALBEDO_NORMAL_LIGHTING 1
#endif
                struct v2f {
                    float4 pos : SV_POSITION;
                    fixed4 color : COLOR0;
                    float3 uv : TEXCOORD0;
                    UNITY_FOG_COORDS(1)
                    UNITY_VERTEX_OUTPUT_STEREO
                    float4 screenPos : TEXCOORD2;
#if defined(ALBEDO_NORMAL_LIGHTING)
                    float3 viewDir : TEXCOORD3;
#endif
                };

#if defined(ALBEDO_NORMAL_LIGHTING)
                CBUFFER_START(UnityTerrainImposter)
                    float3 _TerrainTreeLightDirections[4];
                    float4 _TerrainTreeLightColors[4];
                CBUFFER_END
#endif
        void CameraFacingBillboardVert(inout float4 pos, float2 offset, float offsetz)
        {
            float3 vertViewVector = pos.xyz - _TreeBillboardCameraPos.xyz;
            float treeDistanceSqr = dot(vertViewVector, vertViewVector);
            float distance = sqrt(treeDistanceSqr);
            if (treeDistanceSqr > _TreeBillboardDistances.x)
                offset.xy = offsetz = 0.0;
            // Create LookAt matrix
            float3 up = float3(0, 1, 0);
            float3 zaxis = vertViewVector / distance; // distance won't be 0 since billboard would already be clipped by near plane
            float3 xaxis = normalize(cross(up, zaxis)); // direct top down view of billboard won't be visible due its orientation about yaxis
            float vertexCameraDistance = distance - _TreeBillboardDistances.z;
            float fadeAmount = saturate(vertexCameraDistance / _TreeBillboardDistances.w);
            pos.w = fadeAmount;
            if (vertexCameraDistance > _TreeBillboardDistances.w)
                pos.w = 1.0;

            // positioning of billboard vertices horizontally
            pos.xyz += xaxis * offset.x;
            float radius = offset.y;
            // positioning of billboard vertices veritally
            pos.xyz += up * radius;
        }

                v2f vert(appdata_tree_billboard v) {
                    v2f o;
                    UNITY_SETUP_INSTANCE_ID(v);
                    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                    CameraFacingBillboardVert(v.vertex, v.texcoord1.xy, v.texcoord.y);
                    o.uv.z = v.vertex.w;
                    o.pos = UnityObjectToClipPos(v.vertex);
                    o.uv.x = v.texcoord.x;
                    o.uv.y = v.texcoord.y > 0;
#if defined(ALBEDO_NORMAL_LIGHTING)
                    o.viewDir = normalize(ObjSpaceViewDir(v.vertex));
#endif
                    o.color = v.color;
                    o.screenPos = ComputeScreenPos(o.pos);
                    UNITY_TRANSFER_FOG(o,o.pos);
                    return o;
                }

                half3 CalcTreeLighting(half3 viewDir, half3 lightColor, half3 lightDir, half3 albedo, half3 normal, half backContribScale)
                {
                    half backContrib = saturate(dot(viewDir, -lightDir));
                    half ndotl = dot(lightDir, normal);
                    backContrib = lerp(saturate(-ndotl), backContrib, _TranslucencyViewDependency) * backContribScale;
                    half3 translucencyColor = backContrib * _TranslucencyColor;
                    const half diffuseWrap = 0.8;
                    ndotl = saturate(ndotl * diffuseWrap + (1 - diffuseWrap));
                    return albedo * (translucencyColor + ndotl) * lightColor;
                }

                sampler2D _MainTex;
                sampler2D _NormalTex;

                fixed4 frag(v2f input) : SV_Target
                {
                    fixed4 col = tex2D(_MainTex, input.uv.xy);
                    col.rgb *= input.color.rgb;
#if defined(ALBEDO_NORMAL_LIGHTING)
                    half3 normal = tex2D(_NormalTex, input.uv.xy).xyz;
                    normal = normalize(normal);
                    half3 albedo = col.rgb;
                    half3 light = UNITY_LIGHTMODEL_AMBIENT * albedo;
                    const half backContribScale = 0.2;

                    light += CalcTreeLighting(input.viewDir, _TerrainTreeLightColors[0].rgb, _TerrainTreeLightDirections[0], albedo, normal, backContribScale);
                    light += CalcTreeLighting(input.viewDir, _TerrainTreeLightColors[1].rgb, _TerrainTreeLightDirections[1], albedo, normal, backContribScale);
                    light += CalcTreeLighting(input.viewDir, _TerrainTreeLightColors[2].rgb, _TerrainTreeLightDirections[2], albedo, normal, backContribScale);
                    col.rgb = light;
#endif
#if defined(SHADER_API_D3D11) || defined(SHADER_API_GLCORE)
                    //use alpha-to-coverage with dithering only on desktop and not for Quest/mobile (too slow) (case 1308585)
                    float coverage = ComputeAlphaCoverage(input.screenPos, input.uv.z);
                    col.a *= coverage;
#endif
                    clip(col.a - _TreeBillboardCameraFront.w);
                    UNITY_APPLY_FOG(input.fogCoord, col);
                    return col;
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
