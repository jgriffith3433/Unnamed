Shader "PW/StormVTDecal"
{
    Properties
    {
        _BaseColorMap("Base Color Map", 2D) = "white" {}
        _BaseColor("Base Color", Vector) = (1,1,1,1)
        [NoScaleOffset]_NormalMap("NormalMap", 2D) = "bump" {}
        [NoScaleOffset]_MaskMap("MaskMap", 2D) = "white" {}
        _NormalScale("Normal Scale", Float) = 1
        _DecalType("Decal Type", Int) = 0
        _DisplacementHeight("_DisplacementHeight", Float) = 1
        _DisplacementMultiplier("DisplacementMultiplier", Float) = 1
        _FadeFactor("_FadeFactor", Float) = 1
        _TerrainScale("_TerrainScale", Vector) = (2048, 0,0,0)

        _RoadBlendRange("_RoadBlendRange", Float) = 2
        _RoadBlendWidth("_RoadBlendWidth", Float) = 1
        _RoadBlendContrast("_RoadBlendContrast", Float) = 0.5
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Cull off
        ZWrite Off
        LOD 100

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha // Traditional transparency
            
            CGPROGRAM
            // Upgrade NOTE: excluded shader from OpenGL ES 2.0 because it uses non-square matrices
            #pragma exclude_renderers gles
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
                float4 tangent : TANGENT;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 worldNormal : TEXCOORD1;
                float3 worldTangent : TEXCOORD2;
                float3 worldBitangent : TEXCOORD3;
                float3 normal : TEXCOORD4;
                float4 tangent : TEXCOORD5;
                float3 worldPos : TEXCOORD6;
            };

            // Define a struct for multiple render targets
            struct FragmentOutput
            {
                float4 target0 : SV_Target0;  // Stores color and mask b
                float4 target1 : SV_Target1;  // Stores normal.xz, ambient occlusion and smoothness
                float4 target2 : SV_Target2; // Stores alpha
            };

            sampler2D _BaseColorMap;
            float4 _BaseColorMap_ST;
            sampler2D _NormalMap;
            sampler2D _MaskMap;
            float _NormalScale;
            int _DecalType;
            float _DisplacementHeight;
            float _DisplacementMultiplier;
            float _FadeFactor;
            float4 _TerrainScale;
            sampler2D _TileSource0;
            float4 _BaseColor;

            float _TileSize;
            float2 _TilePos;

            float _RoadBlendRange;
            float _RoadBlendContrast;
            float _RoadBlendWidth;

            v2f vert (appdata v)
            {
                v2f o;
                float4 worldPos = mul(unity_ObjectToWorld, v.vertex);
                o.worldPos = worldPos.xyz;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _BaseColorMap);

                // Transform normal and tangent to world space
                o.worldNormal = UnityObjectToWorldNormal(v.normal);
                o.worldTangent = UnityObjectToWorldDir(v.tangent.xyz);
                o.worldBitangent = cross(o.worldNormal, o.worldTangent) * v.tangent.w;
                o.normal = v.normal;
                o.tangent = v.tangent;
                return o;
            }

            // Unpack normal function from DXT compression
            float3 unpackNormal(float4 normal, float scale)
            {
                float3 finalNormal;
                #if UNITY_NO_DXT5nm
                finalNormal.xy = normal.xy * 2 - 1;
                #else
                finalNormal.xy = normal.wy * 2 - 1;
                #endif
                finalNormal.xy *= scale;
                finalNormal.z = sqrt(1 - saturate(dot(finalNormal.xy, finalNormal.xy)));
                finalNormal = normalize(finalNormal);
                return finalNormal;
            }

            float GetOddNegativeScale()
            {
                return unity_WorldTransformParams.w;
            }

            float3x3 CreateWorldToTangent(float3 normal, float3 tangent, float flipSign)
            {
                // For odd-negative scale transforms we need to flip the sign
                float sgn = flipSign * GetOddNegativeScale();
                float3 bitangent = cross(normal, tangent) * sgn;
                return float3x3(tangent, bitangent, normal);
            }

            float3 TransformTangentToWorld(float3 dirTS, float3x3 worldToTangent)
            {
                // Use transpose transformation to go from tangent to world as the matrix is orthogonal
                return mul(dirTS, worldToTangent);
            }

            float Remap(float value, float from1, float to1, float from2, float to2) {
	            return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
            }

            float heightBlend(float height, float transitionPhase, float contrast)
            {
                float precomputedHeight = saturate((height - 1) + (transitionPhase * 2));
                return saturate(lerp(0 - contrast, contrast + 1, precomputedHeight));
            }

            FragmentOutput frag (v2f i)
            {
                FragmentOutput output;

                // Sample the texture
                float4 col = tex2D(_BaseColorMap, i.uv);
                float alpha = col.a;
                if (_DecalType > 0)
                {
                    alpha = 1;
                }

                alpha *= _FadeFactor;

                float3 normal = unpackNormal(tex2D(_NormalMap, i.uv), _NormalScale);
                float4 mask = tex2D(_MaskMap, i.uv);

                // Create tangent to world transformation matrix
                float3x3 tangentToWorld = float3x3(
                    normalize(i.worldTangent),
                    normalize(i.worldBitangent),
                    normalize(i.worldNormal)
                );

                // Transform normal from tangent to world space
                float3 worldNormal = normalize(mul(normal, tangentToWorld));


                UNITY_BRANCH
                if(_DecalType == 2)
                {
                    float2 tileUV = frac(((i.worldPos.xz - _TilePos) / _TileSize) + 0.5);
                    float4 source0Height = tex2D(_TileSource0, tileUV);
                    float transitionPhase = dot((i.uv.x * 2 - 1), (i.uv.x * 2 - 1));
                    transitionPhase = 1 - saturate(pow(transitionPhase * _RoadBlendWidth, _RoadBlendRange));
                    float heightBlendLerp = heightBlend(source0Height.a, transitionPhase, _RoadBlendContrast);
                    alpha = alpha * heightBlendLerp;
                }
                

                mask.b = lerp(_DisplacementHeight, mask.b, _DisplacementMultiplier);

                // Write different data to each target
                output.target0 = float4(col.rgb * _BaseColor.rgb, alpha);  // First target gets color and custom alpha
                output.target1 = float4(worldNormal.xz * 0.5 + 0.5, mask.g, alpha);  // Second target gets UVs in RG channels
                output.target2 = float4(mask.a, mask.b, 0, alpha);

                return output;
            }

            ENDCG
        }
    }
}
