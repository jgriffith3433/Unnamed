Shader "Unlit/SH_FeedbackClipmap"
{
    Properties
    {
        _Scale("Scale", Float) = 1
        _PhysTexSize("PhysTexSize", Vector) = (0,0,0,0)
        _TileSize("TileSize", Float) = 65535
        _TerrainBottomLeftPos("TerrainBottomLeftPos", Vector) = (0,0,0,0)
        _NumTerrains("Num Terrain", Float) = 256
        _BoundsSize("Bounds Size", Float) = 4096
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }

        Pass
        {
            Name "Pass"

            Cull Back
            Blend One Zero
            ZTest LEqual
            ZWrite On
            CGPROGRAM
            #pragma target 3.0
            #pragma vertex vert
            #pragma fragment frag
            #pragma instancing_options procedural:setup assumeuniformscaling nolodfade

            #define UNITY_PROCEDURAL_INSTANCING_ENABLED

            #include "UnityCG.cginc"

            #include "../../Shaders/TerrainTile/StormVTIncludes.hlsl"
            #include "../../Shaders/Includes/StormVTClipmapFunctions.hlsl"
            #include "../../Shaders/Includes/StormVTHLSLFunctions.hlsl"

            float _Scale;
            float4 _PhysTexSize;
            float _TileSize;
            float4 _TerrainBottomLeftPos;
            float _NumTerrains;
            float _BoundsSize;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 worldPos : TEXCOORD1;
            };

            v2f vert (appdata v, uint instanceID : SV_InstanceID)
            {
                v2f o;
                setupCustom(instanceID);

                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;

                
                float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                float3 nonFloatingPointWorldPos = worldPos;
                worldPos += _StormVT_WorldOffset.xyz;

                float3 UVs;
                float2 fracUVs;
                float2 volumeUVs;
                float2 rawUVs;
                float zoomShift;

                VertSampleVirtualUVs(worldPos, sampler_point_repeat, UVs, fracUVs, volumeUVs, zoomShift);

                float height = SamplePhysicalTextureHeight(UVs, sampler_linear_repeat, 0) * _StormVT_TerrainHeightScale.r + _StormVT_TerrainHeightScale.g;

                worldPos.y = height;
                nonFloatingPointWorldPos.y = height;
        
                o.worldPos = worldPos;

                float3 worldToObjPos = mul((float3x3) unity_WorldToObject, worldPos);

                v.vertex.y = worldToObjPos.y;
        
                float4 pos = UnityObjectToClipPos(v.vertex);
                
                o.vertex = pos;
                return o;
            }

            void CreateMipFeedback(float2 rawUV, float2 uv, float2 physicalTextureSize, float tileSize, out float4 result, out float3 returnUV, out float mipLevel)
            {
                float mip_level = computeMipLevel(rawUV, -_StormVT_TileMipData.z, _StormVT_TileMipData.x, _StormVT_TileMipData.y, physicalTextureSize);
                float2 rawPageUVs = (uv * tileSize);
                if(mip_level < 8)
                {
                    rawPageUVs = fmod(rawPageUVs, 256.0);
                }

                float2 page_coords = floor(rawPageUVs / exp2(mip_level));

                 // Convert to [0,255] RGBA color and return:
                float4 page_id = float4(page_coords, mip_level + 1.0, 1.0);
                result = page_id / 255.0;

                returnUV = float3((uv * tileSize / exp2(mip_level)), mip_level);
                mipLevel = mip_level;
            }

            struct FragOutput
            {
                half4 feedback : SV_Target0;
                half4 terrain : SV_Target1;
            };

            FragOutput frag (v2f i)
            {
                FragOutput o;

                float2 terrainPos = (i.worldPos.xz - _TerrainBottomLeftPos) / _Scale;

                float4 mipTiles;
                float3 uv;
                float mipLevel;
                CreateMipFeedback(terrainPos, frac(terrainPos), _PhysTexSize.xy, _TileSize, mipTiles, uv, mipLevel);

                o.feedback = float4((mipTiles * GetTotalTerrainBoundsMask(i.worldPos)).xyz, 1);



                terrainPos = (i.worldPos.xz - _TerrainBottomLeftPos) / _BoundsSize;
                float2 terrainID = floor(frac(terrainPos) * _NumTerrains) / 255;
               
                o.terrain = float4(terrainID.xy, GetTotalTerrainBoundsMask(i.worldPos), 1);

                return o;
            }
            ENDCG
        }
    }
}
