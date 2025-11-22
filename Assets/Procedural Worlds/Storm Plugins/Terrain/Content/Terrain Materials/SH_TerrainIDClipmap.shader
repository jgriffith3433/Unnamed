Shader "Unlit/SH_TerrainIDClipmap"
{
    Properties
    {
        _TerrainBottomLeftPos("TerrainBottomLeftPos", Vector) = (0,0,0,0)
        _NumTerrains("Num Terrain", Float) = 256
        _BoundsSize("Bounds Size", Float) = 4096
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            Name "Pass"

            Cull Back
            Blend One Zero
            ZTest LEqual
            ZWrite On
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma instancing_options procedural:setup assumeuniformscaling nolodfade

            #define UNITY_PROCEDURAL_INSTANCING_ENABLED

            #include "UnityCG.cginc"

            #include "../../Shaders/TerrainTile/StormVTIncludes.hlsl"
            #include "../../Shaders/Includes/StormVTClipmapFunctions.hlsl"
            #include "../../Shaders/Includes/StormVTHLSLFunctions.hlsl"

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


            fixed4 frag (v2f i) : SV_Target
            {
                float2 terrainPos = (i.worldPos.xz - _TerrainBottomLeftPos) / _BoundsSize;
                float2 terrainID = floor(frac(terrainPos) * _NumTerrains) / 255;
               
                return float4(terrainID.xy, GetTotalTerrainBoundsMask(i.worldPos), 1);
            }
            ENDCG
        }
    }
}
