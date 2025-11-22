Shader"Hidden/SH_TerrainTilePass"
{
    Properties
    {
        _TileTexture("TileTexture", 2D) = "black" {}
        _TerrainTexture("TerrainTexture", 2D) = "black" {}
        _Control0("_Control0", 2D) = "black" {}
        _Control1("_Control1", 2D) = "black" {}
        _Control2("_Control2", 2D) = "black" {}
        _Control3("_Control3", 2D) = "black" {}
        
        _TilePosition("TilePosition", Vector) = (0,0,0,0)
        _TileSize("TileSize", Vector) = (0,0,0,0)
        
        _TerrainPosition("TerrainPosition", Vector) = (0,0,0,0)
        _TerrainSize("TerrainSize", Vector) = (0,0,0,0)
        
        [NoScaleOffset]_ControlMap("ControlMap", 2D) = "black" {}

        [NoScaleOffset]_NormalMap("NormalMap", 2D) = "bump" {}

        [NoScaleOffset]_Colormap("Colormap", 2D) = "black" {}
        [NoScaleOffset]_ColorMapNormal("ColorMapNormal", 2D) = "bump" {}
        
        [NoScaleOffset]_TerrainHeightMap("_TerrainHeightMap", 2D) = "black" {}

        _RecipSize("RecipSize", Float) = 1
        _TerrainHeightmapRecipSize("_TerrainHeightmapRecipSize", Vector) = (0,0,0,0)

        [Toggle(_SNOW_ON)]
        _SNOW("_SNOW_ON", Int) = 0
        
    }
    SubShader
    {
        // No culling or depth
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            Name "Tile Copy"
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #include "StormVTIncludes.hlsl"
            #include "StormVTTerrainTileCommon.hlsl"

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

            //Create a black and white mask from uv
            float GetMask(float2 uv)
            {
                float mask = 0;
                if (uv.x >= 0 && uv.x <= 1 && uv.y >= 0 && uv.y <= 1)
                {
                    mask = 1;
                }
                return mask;
            }
            
            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.worldPos = GetWorldPositionFromMatrix(v.vertex, _TileTRSMatrix);
                return o;
            }
            
            TEXTURE2D(_TerrainTexture);
            float4 _TerrainHeightmapRecipSize;

            fixed4 frag(v2f i) : SV_Target
            {
    
                float2 tileUV;
                float mask;
                RemapTile(i.uv, _TilePosition, _TileSize, _TerrainPosition, _TerrainSize, tileUV, mask);
                
                float4 tile = _TileTexture.Sample(sampler_linear_repeat, i.uv);
                
                
                //Terrain remap
                tileUV = (tileUV / _TerrainHeightmapRecipSize.zw + 0.5f) * _TerrainHeightmapRecipSize.xy;
  
                float4 terrainTexture = _TerrainTexture.Sample(sampler_linear_repeat, tileUV);
    
                return lerp(tile, terrainTexture, mask);
    
            }
            ENDCG
        }


        Pass
        {
            Name "Height - Normal - Holes Copy"
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #include "StormVTIncludes.hlsl"
            #include "StormVTTerrainTileCommon.hlsl"

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

            //Create a black and white mask from uv
            float GetMask(float2 uv)
            {
                float mask = 0;
                if (uv.x >= 0 && uv.x <= 1 && uv.y >= 0 && uv.y <= 1)
                {
                    mask = 1;
                }
                return mask;
            }
            
            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.worldPos = GetWorldPositionFromMatrix(v.vertex, _TileTRSMatrix);
                return o;
            }
            
            TEXTURE2D(_TerrainHeightMap);
            TEXTURE2D(_TerrainWorldNormalMap);
            TEXTURE2D(_TerrainHolesMap);

            TEXTURE2D(_TileHeightMap);
            TEXTURE2D(_TileWorldNormalMap);

            float4 _TerrainHeightmapRecipSize;
            float _TerrainHeightmapScale;
            float4 _StormVT_TerrainHeightScale;

            struct FragOutput
            {
                half4 height : SV_Target0;
                half4 normal : SV_Target1;
            };

            FragOutput frag(v2f i)
            {
    
                FragOutput o;
    
                float2 tileUV;
                float mask;
                RemapTile(i.uv, _TilePosition, _TileSize, _TerrainPosition, _TerrainSize, tileUV, mask);
                
    
                //Terrain remap
                tileUV = (tileUV / _TerrainHeightmapRecipSize.zw + 0.5f) * _TerrainHeightmapRecipSize.xy;
    
                //Sample previous tile heightmap
                float4 heightTile = _TileHeightMap.Sample(sampler_linear_repeat, i.uv);
                //float4 terrainHeightTexture = _TerrainHeightMap.Sample(sampler_linear_repeat, tileUV);
                float4 terrainHeightTexture = UnpackHeightmap(_TerrainHeightMap.Sample(sampler_linear_repeat, tileUV)).rrrr;
                float terrainScalePercentage = _TerrainHeightmapScale / (_StormVT_TerrainHeightScale.x * 0.5);
                terrainHeightTexture.r *= terrainScalePercentage;
                o.height = lerp(heightTile, terrainHeightTexture, mask);
                

                //Sample previous tile holesmap
                float4 terrainHolesTexture = _TerrainHolesMap.Sample(sampler_linear_repeat, tileUV);

                //Sample previous tile normalmap
                float4 normalTile = _TileWorldNormalMap.Sample(sampler_linear_repeat, i.uv);
                float4 terrainNormalTexture = _TerrainWorldNormalMap.Sample(sampler_linear_repeat, tileUV);
                terrainNormalTexture = float4(terrainNormalTexture.xz, terrainNormalTexture.a, terrainHolesTexture.r);

                o.normal = lerp(normalTile, terrainNormalTexture, mask);
    

    
                return o;
            }
            ENDCG
        }


        Pass
        {
            Name "Color Sources Pass"
            CGPROGRAM
            #pragma target 5.0
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #include "StormVTIncludes.hlsl"
            #include "StormVTTerrainTileCommon.hlsl"

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

            //Create a black and white mask from uv
            float GetMask(float2 uv)
            {
                float mask = 0;
                if (uv.x >= 0 && uv.x <= 1 && uv.y >= 0 && uv.y <= 1)
                {
                    mask = 1;
                }
                return mask;
            }
            
            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.worldPos = GetWorldPositionFromMatrix(v.vertex, _TileTRSMatrix);
                return o;
            } 

            float4 _TerrainHeightmapRecipSize;

            struct FragOutput
            {
                float4 source0 : SV_Target0;
                float4 source1 : SV_Target1;
            };


            FragOutput frag(v2f i)
            {
    
                FragOutput o;
    
                float2 tileUV;
                float mask;
                RemapTile(i.uv, _TilePosition, _TileSize, _TerrainPosition, _TerrainSize, tileUV, mask);
                
                //Remap to terrain scale
                float value = 2048.0f / 2049.0f;
                float2 controlUV = (tileUV - 0.5) * value + 0.5;
    
                //Define outputs
                float4 source0 = 0;
                float4 source1 = 0;
    
                //Sample set of 4 layers
                LayerMix(i.worldPos, i.uv, tileUV, controlUV, _ControlMap, mask, source0, source1);
        
                o.source0 = source0;
                o.source1 = source1;

                return o;
            }
            ENDCG
        }

                Pass
        {
            Name "Color Sources Pass Height"
            CGPROGRAM
            #pragma target 5.0
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #include "StormVTIncludes.hlsl"
            #include "StormVTTerrainTileCommon.hlsl"

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

            //Create a black and white mask from uv
            float GetMask(float2 uv)
            {
                float mask = 0;
                if (uv.x >= 0 && uv.x <= 1 && uv.y >= 0 && uv.y <= 1)
                {
                    mask = 1;
                }
                return mask;
            }
            
            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.worldPos = GetWorldPositionFromMatrix(v.vertex, _TileTRSMatrix);
                return o;
            } 

            float4 _TerrainHeightmapRecipSize;

            struct FragOutput
            {
                float4 source0 : SV_Target0;
                float4 source1 : SV_Target1;
            };


            FragOutput frag(v2f i)
            {
    
                FragOutput o;
    
                float2 tileUV;
                float mask;
                RemapTile(i.uv, _TilePosition, _TileSize, _TerrainPosition, _TerrainSize, tileUV, mask);
                
                //Remap to terrain scale
                float value = 2048.0f / 2049.0f;
                float2 controlUV = (tileUV - 0.5) * value + 0.5;
    
                //Define outputs
                float4 source0 = 0;
                float4 source1 = 0;
    
                //Sample set of 4 layers
                HeightLayerMix(i.worldPos, i.uv, tileUV, controlUV, _ControlMap, mask, source0, source1);
        
                o.source0 = source0;
                o.source1 = source1;

                return o;
            }
            ENDCG
        }

        Pass
        {
            Name "World Normal Blend"
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #include "StormVTIncludes.hlsl" 
            #include "StormVTTerrainTileCommon.hlsl"

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
            
            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.worldPos = GetWorldPositionFromMatrix(v.vertex, _TileTRSMatrix);
                return o;
            }
            


            float3 worldSpaceNormalBlend(float3 terrainNormal, float3 surfaceNormal)
            {
                float3 terrainTangent = cross(terrainNormal, float3(0, 0, 1));
                float3 terrainBitangent = cross(terrainTangent, terrainNormal);
                return surfaceNormal.x * terrainTangent + surfaceNormal.y * terrainBitangent + surfaceNormal.z * terrainNormal;
            }

            fixed4 frag(v2f i) : SV_Target
            {

                float2 worldUV = i.worldPos.xz / 1024.0f;
                float4 deriv = float4(ddx(worldUV), ddy(worldUV));
    
                float4 worldNormal = _WorldNormal.Sample(sampler_linear_repeat, i.uv);

                float3 normal = float3(worldNormal.xy * 2 - 1, 0);
                normal = float3(normal.x, sqrt(1 - saturate(dot(normal.xy, normal.xy))), normal.y);
                normal = normalize(normal);
                worldNormal = float4(normal, 0);
                
                float4 tileSource1 = _TileSource1.Sample(sampler_linear_repeat, i.uv);
    
                float3 tangentSample = float3(tileSource1.xy * 2 - 1, 0.0);
                tangentSample.z = sqrt(1.0 - saturate(dot(tangentSample.rg, tangentSample.rg)));
                
                float3 worldSpaceBlend = worldSpaceNormalBlend(worldNormal, tangentSample);
                worldSpaceBlend = normalize(worldSpaceBlend);
    
                return float4(worldSpaceBlend.xz * 0.5 + 0.5, tileSource1.zw);
            }
            ENDCG
        }

        Pass
        {
            Name "Color Sources Pass Simple"
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #include "StormVTIncludes.hlsl"
            #include "StormVTTerrainTileCommon.hlsl"

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

                                    //Create a black and white mask from uv
            float GetMask(float2 uv)
            {
                float mask = 0;
                if (uv.x >= 0 && uv.x <= 1 && uv.y >= 0 && uv.y <= 1)
                {
                    mask = 1;
                }
                return mask;
            }
            
            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.worldPos = GetWorldPositionFromMatrix(v.vertex, _TileTRSMatrix);
                return o;
            }
           



            float4 _TerrainHeightmapRecipSize;

            struct FragOutput
            {
                half4 source0 : SV_Target0;
                half4 source1 : SV_Target1;
            };


            FragOutput frag(v2f i)
            {

                FragOutput o;
    
                float2 tileUV;
                float mask;
                RemapTile(i.uv, _TilePosition, _TileSize, _TerrainPosition, _TerrainSize, tileUV, mask);
                
                //Remap to terrain scale
                float value = 2048.0f / 2049.0f;
                float2 controlUV = (tileUV - 0.5) * value + 0.5;
    
                //Define outputs
                half4 source0 = half4(0, 0, 0, 0);
                half4 source1 = half4(0, 0, 0, 0);
    
                //Sample set of 4 layers
                SimpleLayerMix(i.worldPos, i.uv, tileUV, controlUV, _ControlMap, mask, source0, source1);
        
                o.source0 = source0;
                o.source1 = source1;
    
                return o;
            }
            ENDCG
        }

        Pass
        {
            Name "Snow Source Combined"
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #include "StormVTIncludes.hlsl"
            #include "StormVTTerrainTileCommon.hlsl"

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
            
            

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.worldPos = GetWorldPositionFromMatrix(v.vertex, _TileTRSMatrix);
                return o;
            }


            struct FragOutput
            {
                half4 source0 : SV_Target0;
                half4 source1 : SV_Target1;
            };

            //Unpack normal function from DXT compression
            float3 unpackNormal(float4 normal)
            {
                float3 finalNormal;
                #if UNITY_NO_DXT5nm
                    finalNormal.xy = normal.xy * 2 - 1;
                #else
                    finalNormal.xy = normal.wy * 2 - 1;
                #endif

                finalNormal.z = sqrt(1 - saturate(dot(finalNormal.xy, finalNormal.xy)));
                return finalNormal;
            }

            FragOutput frag(v2f i)
            {
    
                FragOutput o;
    
                float2 tileUV;
                float mask;
                RemapTile(i.uv, _TilePosition, _TileSize, _TerrainPosition, _TerrainSize, tileUV, mask);

                //Remap to terrain scale
                float value = 2048.0f / 2049.0f;
                float2 controlUV = (tileUV - 0.5) * value + 0.5;
    
                float curvature = GetSnowMask(controlUV);
    
                float2 worldUV = i.worldPos.xz / _SnowUVScale;
    
                float4 deriv = float4(ddx(worldUV), ddy(worldUV));
    
                float4 snowAlbedo;
                float3 snowNormal;
                float4 snowMask;
   
                snowAlbedo = _SnowAlbedo.SampleGrad(sampler_linear_repeat, worldUV, deriv.xy, deriv.zw);
                snowNormal = unpackNormal(_SnowNormal.SampleGrad(sampler_linear_repeat, worldUV, deriv.xy, deriv.zw));
                snowMask = _SnowMask.SampleGrad(sampler_linear_repeat, worldUV, deriv.xy, deriv.zw);
                snowNormal.xy = snowNormal.xy * 0.5 + 0.5;


                float4 tileSource0 = _TileSource0.Sample(sampler_linear_repeat, i.uv);
                float4 tileSource1 = _TileSource1.Sample(sampler_linear_repeat, i.uv);
    
                float tileHeight = tileSource0.a;
    
                curvature = HeightMask(tileHeight, snowMask.b, curvature);
                
                float4 tileWithSnow0 = lerp(tileSource0, float4(_SnowColor.rgb * snowAlbedo.rgb, snowMask.b), curvature);
                float4 tileWithSnow1 = lerp(tileSource1, float4(snowNormal.xy, snowMask.ga), curvature);
    
                o.source0 = lerp(tileSource0, tileWithSnow0, mask);
                o.source1 = lerp(tileSource1, tileWithSnow1, mask);
    
                return o;

            }
            ENDCG
        }


        Pass
        {
            Name "Max Height"
            CGPROGRAM
            #pragma target 5.0
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #include "StormVTIncludes.hlsl" 
            #include "StormVTTerrainTileCommon.hlsl"

            #define _HEIGHTBLEND

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

            //Create a black and white mask from uv
            float GetMask(float2 uv)
            {
                float mask = 0;
                if (uv.x >= 0 && uv.x <= 1 && uv.y >= 0 && uv.y <= 1)
                {
                    mask = 1;
                }
                return mask;
            }
            
            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.worldPos = GetWorldPositionFromMatrix(v.vertex, _TileTRSMatrix);
                return o;
            }
            


            fixed4 frag(v2f i) : SV_Target
            {
    
                float2 tileUV;
                float mask;
                RemapTile(i.uv, _TilePosition, _TileSize, _TerrainPosition, _TerrainSize, tileUV, mask);
                

                //Remap to terrain scale
                float value = 2048.0f / 2049.0f;
                float2 controlUV = (tileUV - 0.5) * value + 0.5;
    
                //Sample set of 4 layers
                float control = GetMaxHeightMix(i.worldPos, i.uv, tileUV, controlUV, _ControlMap, mask);
    
                return control;
    
            }
            ENDCG
        }

        Pass
        {
            Name "Sum Height"
            CGPROGRAM
            #pragma target 5.0
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #include "StormVTIncludes.hlsl" 
            #include "StormVTTerrainTileCommon.hlsl"

            #define _HEIGHTBLEND

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

            //Create a black and white mask from uv
            float GetMask(float2 uv)
            {
                float mask = 0;
                if (uv.x >= 0 && uv.x <= 1 && uv.y >= 0 && uv.y <= 1)
                {
                    mask = 1;
                }
                return mask;
            }
            
            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.worldPos = GetWorldPositionFromMatrix(v.vertex, _TileTRSMatrix);
                return o;
            }
            


            fixed4 frag(v2f i) : SV_Target
            {
    
                float2 tileUV;
                float mask;
                RemapTile(i.uv, _TilePosition, _TileSize, _TerrainPosition, _TerrainSize, tileUV, mask);
                

                //Remap to terrain scale
                float value = 2048.0f / 2049.0f;
                float2 controlUV = (tileUV - 0.5) * value + 0.5;
    
                //Sample set of 4 layers
                float control = GetSumHeightMix(i.worldPos, i.uv, tileUV, controlUV, _ControlMap, mask);
    
                return control;
    
            }
            ENDCG
        }

        Pass
        {
            Name "Height Copy"
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #include "StormVTIncludes.hlsl"
            #include "StormVTTerrainTileCommon.hlsl"

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

            //Create a black and white mask from uv
            float GetMask(float2 uv)
            {
                float mask = 0;
                if (uv.x >= 0 && uv.x <= 1 && uv.y >= 0 && uv.y <= 1)
                {
                    mask = 1;
                }
                return mask;
            }
            
            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.worldPos = GetWorldPositionFromMatrix(v.vertex, _TileTRSMatrix);
                return o;
            }
            
            TEXTURE2D(_TerrainHeightMap);
            TEXTURE2D(_TerrainWorldNormalMap);
            TEXTURE2D(_TerrainHolesMap);

            TEXTURE2D(_TileHeightMap);
            TEXTURE2D(_TileWorldNormalMap);
            TEXTURE2D(_TileHolesMap);

            float4 _TerrainHeightmapRecipSize;

            fixed4 frag(v2f i) : SV_Target
            {
    
                float2 tileUV;
                float mask;
                RemapTile(i.uv, _TilePosition, _TileSize, _TerrainPosition, _TerrainSize, tileUV, mask);
                
    
                //Terrain remap
                tileUV = (tileUV / _TerrainHeightmapRecipSize.zw + 0.5f) * _TerrainHeightmapRecipSize.xy;
    
                //Sample previous tile heightmap
                float4 heightTile = _TileHeightMap.Sample(sampler_linear_repeat, i.uv);
                float4 terrainHeightTexture = _TerrainHeightMap.Sample(sampler_linear_repeat, tileUV);
                return lerp(heightTile, terrainHeightTexture, mask);
    
            }
            ENDCG
        }

         Pass
        {
            Name "Procedural - Height Normal Holes"
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #include "StormVTIncludes.hlsl"
            #include "StormVTTerrainTileCommon.hlsl"

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

            
            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.worldPos = GetWorldPositionFromMatrix(v.vertex, _TileTRSMatrix);
                return o;
            }

            struct FragOutput
            {
                half4 height : SV_Target0;
                half4 normal : SV_Target1;
                half holes : SV_Target2;
            };

            inline float unity_noise_randomValue (float2 uv)
            {
                return frac(sin(dot(uv, float2(12.9898, 78.233)))*43758.5453);
            }

            inline float unity_noise_interpolate (float a, float b, float t)
            {
                return (1.0-t)*a + (t*b);
            }

            inline float unity_valueNoise (float2 uv)
            {
                float2 i = floor(uv);
                float2 f = frac(uv);
                f = f * f * (3.0 - 2.0 * f);

                uv = abs(frac(uv) - 0.5);
                float2 c0 = i + float2(0.0, 0.0);
                float2 c1 = i + float2(1.0, 0.0);
                float2 c2 = i + float2(0.0, 1.0);
                float2 c3 = i + float2(1.0, 1.0);
                float r0 = unity_noise_randomValue(c0);
                float r1 = unity_noise_randomValue(c1);
                float r2 = unity_noise_randomValue(c2);
                float r3 = unity_noise_randomValue(c3);

                float bottomOfGrid = unity_noise_interpolate(r0, r1, f.x);
                float topOfGrid = unity_noise_interpolate(r2, r3, f.x);
                float t = unity_noise_interpolate(bottomOfGrid, topOfGrid, f.y);
                return t;
            }

            float SimpleNoise(float2 UV, float Scale)
            {
                float t = 0.0;

                float freq = pow(2.0, float(0));
                float amp = pow(0.5, float(3-0));
                t += unity_valueNoise(float2(UV.x*Scale/freq, UV.y*Scale/freq))*amp;

                freq = pow(2.0, float(1));
                amp = pow(0.5, float(3-1));
                t += unity_valueNoise(float2(UV.x*Scale/freq, UV.y*Scale/freq))*amp;

                freq = pow(2.0, float(2));
                amp = pow(0.5, float(3-2));
                t += unity_valueNoise(float2(UV.x*Scale/freq, UV.y*Scale/freq))*amp;

                return t;
            }

            FragOutput frag(v2f i)
            {
    
                FragOutput o;
                
                float noise = SimpleNoise((i.worldPos / 200).xz, 1) * 0.25;

                float epsilon = 10;

                float noiseX = SimpleNoise((i.worldPos / 200).xz + float2(epsilon, 0), 1) * 0.25;
                float noiseY = SimpleNoise((i.worldPos / 200).xz + float2(0, epsilon), 1) * 0.25;

                float dU = (noiseX - noise) / epsilon;
                float dV = (noiseY - noise) / epsilon;

                // Normal in object/tangent space
                float3 tangentNormal = normalize(float3(-dU, -dV, 1.0));

                // Transform to world space
                //float3x3 TBN = float3x3(normalize(ddx(i.worldPos )), normalize(ddy(i.worldPos )), float3(0, 0, 1));
                //float3 WorldNormal = normalize(mul(tangentNormal, TBN));

                o.height = noise;
                //o.normal = half4(0, 1, 0, noise);
                o.normal = half4(tangentNormal, noise);
                o.holes = 1;

    
                return o;
            }
            ENDCG
        }

        Pass
        {
            Name "Procedural - Color Sources Pass"
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #include "StormVTIncludes.hlsl"
            #include "StormVTTerrainTileCommon.hlsl"

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
            
            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.worldPos = GetWorldPositionFromMatrix(v.vertex, _TileTRSMatrix);
                return o;
            }
           

            struct FragOutput
            {
                half4 source0 : SV_Target0;
                half4 source1 : SV_Target1;
            };


            FragOutput frag(v2f i)
            {

                FragOutput o;
    
                o.source0 = half4(1,0,0,0);
                o.source1 = half4(0,0,1,0);
    
                return o;
            }
            ENDCG
        }

        Pass
        {
            Name "Array Color Sources Pass"
            CGPROGRAM
            #pragma target 5.0
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #include "StormVTIncludes.hlsl"
            #include "StormVTTerrainTileCommon.hlsl"

            #pragma shader_feature _SNOW_ON

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

            //Create a black and white mask from uv
            float GetMask(float2 uv)
            {
                float mask = 0;
                if (uv.x >= 0 && uv.x <= 1 && uv.y >= 0 && uv.y <= 1)
                {
                    mask = 1;
                }
                return mask;
            }
            
            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.worldPos = GetWorldPositionFromMatrix(v.vertex, _TileTRSMatrix);
                return o;
            } 

            float4 _TerrainHeightmapRecipSize;

            struct FragOutput
            {
                half4 source0 : SV_Target0;
                half4 source1 : SV_Target1;
            };


            FragOutput frag(v2f i)
            {
    
                FragOutput o;
    
                float2 tileUV;
                float mask;
                RemapTile(i.uv, _TilePosition, _TileSize, _TerrainPosition, _TerrainSize, tileUV, mask);
                
                //Remap to terrain scale
                float value = (2048.0f / 2049.0f);
                float2 controlUV = (tileUV - 0.5) * value + 0.5;

                //Define outputs
                half4 source0 = float4(1,0,0,1);
                half4 source1 = float4(0.5,0.5,1,0);
    
                //Sample set of 4 layers
                SampleTerrain(i.worldPos, i.uv, tileUV, controlUV, mask, source0, source1);

                o.source0 = source0;
                o.source1 = source1;

                return o;
            }
            ENDCG
        }

        Pass
        {
            Name "Array Color Sources Pass Simple"
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #include "StormVTIncludes.hlsl"
            #include "StormVTTerrainTileCommon.hlsl"

            #pragma shader_feature _SNOW_ON

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

                                    //Create a black and white mask from uv
            float GetMask(float2 uv)
            {
                float mask = 0;
                if (uv.x >= 0 && uv.x <= 1 && uv.y >= 0 && uv.y <= 1)
                {
                    mask = 1;
                }
                return mask;
            }
            
            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.worldPos = GetWorldPositionFromMatrix(v.vertex, _TileTRSMatrix);
                return o;
            }
           



            float4 _TerrainHeightmapRecipSize;

            struct FragOutput
            {
                half4 source0 : SV_Target0;
                half4 source1 : SV_Target1;
            };


            FragOutput frag(v2f i)
            {

                FragOutput o;
    
                float2 tileUV;
                float mask;
                RemapTile(i.uv, _TilePosition, _TileSize, _TerrainPosition, _TerrainSize, tileUV, mask);
                
                //Remap to terrain scale
                float value = 2048.0f / 2049.0f;
                float2 controlUV = (tileUV - 0.5) * value + 0.5;
    
                //Define outputs
                half4 source0 = half4(0, 0, 0, 0);
                half4 source1 = half4(0, 0, 0, 0);
    
                //Sample set of 4 layers
                SimpleLayerMixArray(i.worldPos, i.uv, tileUV, controlUV, _ControlMap, mask, source0, source1);
        
                o.source0 = source0;
                o.source1 = source1;
    
                return o;
            }
            ENDCG
        }

        Pass
        {
            Name "All Sources"

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #include "StormVTIncludes.hlsl"
            #include "StormVTTerrainTileCommon.hlsl"
            
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }


            TEXTURE2D(_CaptureSource0);
            TEXTURE2D(_CaptureSource1);

            struct FragOutput
            {
                float4 source0 : SV_Target0;
                float4 source1 : SV_Target1;
            };

            FragOutput frag(v2f i)
            {
                FragOutput o;

                float4 tileSource0 = _TileSource0.Sample(sampler_linear_repeat, i.uv);
                float4 tileSource1 = _TileSource1.Sample(sampler_linear_repeat, i.uv);
                float4 captureSource0 = _CaptureSource0.Sample(sampler_linear_repeat, i.uv);
                float4 captureSource1 = _CaptureSource1.Sample(sampler_linear_repeat, i.uv);
    
    
                float mask = captureSource0.a;
                captureSource1.zw = captureSource1.wz;
    
                //Set displacement / height to 0
                captureSource0.a = 0;

                o.source0 = lerp(tileSource0, captureSource0, mask);
                o.source1 = lerp(tileSource1, captureSource1, mask);

                return o;

            }
            ENDCG
        }

        Pass
        {
            Name "Distance Map Copy"
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #include "StormVTIncludes.hlsl"
            #include "StormVTTerrainTileCommon.hlsl"

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

            //Create a black and white mask from uv
            float GetMask(float2 uv)
            {
                float mask = 0;
                if (uv.x >= 0 && uv.x <= 1 && uv.y >= 0 && uv.y <= 1)
                {
                    mask = 1;
                }
                return mask;
            }
            
            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.worldPos = GetWorldPositionFromMatrix(v.vertex, _TileTRSMatrix);
                return o;
            }
            
            TEXTURE2D(_TerrainDistanceMapNormal);
            TEXTURE2D(_TerrainGrassMapNormal);
            TEXTURE2D(_TerrainTreeMapNormal);

            TEXTURE2D(_TileDistanceMapNormal);


            TEXTURE2D(_TerrainGrassMapColor);
            TEXTURE2D(_TerrainTreeMapColor);
            TEXTURE2D(_TileGrassMapColor);
            TEXTURE2D(_TileTreeMapColor);

            float4 _TerrainHeightmapRecipSize;

            struct FragOutput
            {
                half4 grassColor : SV_Target0;
                half4 treeColor : SV_Target1;
                half4 normal : SV_Target2;
            };

            FragOutput frag(v2f i)
            {
    
                FragOutput o;
    
                float2 tileUV;
                float mask;
                RemapTile(i.uv, _TilePosition, _TileSize, _TerrainPosition, _TerrainSize, tileUV, mask);
                
    
                //Terrain remap
                tileUV = (tileUV / _TerrainHeightmapRecipSize.zw + 0.5f) * _TerrainHeightmapRecipSize.xy;
    
                //Sample previous tile colormap
                float4 grassTile = _TileGrassMapColor.Sample(sampler_linear_repeat, i.uv);
                float4 terrainGrassTexture = _TerrainGrassMapColor.Sample(sampler_linear_repeat, tileUV);
                
                o.grassColor = lerp(grassTile, terrainGrassTexture, mask);

                //Sample previous tile colormap
                float4 treeTile = _TileTreeMapColor.Sample(sampler_linear_repeat, i.uv);
                float4 terrainTreeTexture = _TerrainTreeMapColor.Sample(sampler_linear_repeat, tileUV);
                
                o.treeColor = lerp(treeTile, terrainTreeTexture, mask);

                //Sample previous tile normaltmap
                float4 normalTile = _TileDistanceMapNormal.Sample(sampler_linear_repeat, i.uv);
                float4 terrainNormalTexture = _TerrainDistanceMapNormal.Sample(sampler_linear_repeat, tileUV);
                

                float4 grassNormal = _TerrainGrassMapNormal.Sample(sampler_linear_repeat, tileUV);
                float4 treeNormal = _TerrainTreeMapNormal.Sample(sampler_linear_repeat, tileUV);
                float4 combinedNormal = float4(grassNormal.xz, treeNormal.xz);

                o.normal = lerp(normalTile, combinedNormal, mask);
               
    

    
                return o;
            }
            ENDCG
        }

        Pass
        {
            Name "Decal Blend"

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #include "StormVTIncludes.hlsl"
            #include "StormVTTerrainTileCommon.hlsl"
            
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }


            TEXTURE2D(_DecalSource0);
            TEXTURE2D(_DecalSource1);
            TEXTURE2D(_DecalSource2);

            struct FragOutput
            {
                float4 source0 : SV_Target0;
                float4 source1 : SV_Target1;
            };

            FragOutput frag(v2f i)
            {
                FragOutput o;

                float4 tileSource0 = _TileSource0.Sample(sampler_linear_repeat, i.uv);
                float4 tileSource1 = _TileSource1.Sample(sampler_linear_repeat, i.uv);
                float4 decalSource0 = _DecalSource0.Sample(sampler_linear_repeat, i.uv);
                float4 decalSource1 = _DecalSource1.Sample(sampler_linear_repeat, i.uv);
                float4 decalSource2 = _DecalSource2.Sample(sampler_linear_repeat, i.uv);
    
                //Diffuse RGB, Height
                float4 finalSource0 = float4(decalSource0.rgb, decalSource2.g);

                //World Normal X, Y, Ambient Occlusion, Smoothness
                float4 finalSource1 = float4(decalSource1.xyz, decalSource2.r);

                o.source0 = lerp(tileSource0, finalSource0, decalSource0.a);
                o.source1 = lerp(tileSource1, finalSource1, decalSource0.a);

                return o;

            }
            ENDCG
        }

    }
}
