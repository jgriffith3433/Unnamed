Shader"Hidden/SH_GetWorldNormalMap"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Heightmap("_Heightmap", 2D) = "black" {}
        _Height("Height", Float) = 1000
        _TexelSize("_TexelSize", Float) = 513
    }
    SubShader
    {
        // No culling or depth
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            Name "Curvature"
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            

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

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            sampler2D_float _MainTex;
            sampler2D_float _Heightmap;


#define MAXSTEPS 32
#define MAXRADIALSTEPS 8
#define LOD 0

            //Blur
            float blur(float2 uv, sampler2D tex, float distance, float steps, float radialSteps)
            {
                float result = 0;
                float4 CurColor = float4(0, 0, 0, 0);
                float2 NewUV = uv;
                int incre = 0;
                float StepSize = distance / (int) steps;
                float CurDistance = 0;
                float2 CurOffset = 0;
                float SubOffset = 0;
                float TwoPi = 6.283185;
                float accumdist = 0;
                int RadialSteps = min(radialSteps, MAXRADIALSTEPS);
                float RadialOffset = 0.618;
                float KernelPower = 1;

                int loopSteps = min(steps, MAXSTEPS);
   
    
                if (steps < 1)
                {
                    result = UnpackHeightmap(tex2Dlod(tex, float4(uv, 0, LOD)));
                }
                else
                {
        
                    while (incre < loopSteps)
                    {
                        CurDistance += StepSize;
                        for (int j = 0; j < RadialSteps; j++)
                        {
                            SubOffset += 1;
                            CurOffset.x = cos(TwoPi * (SubOffset / RadialSteps));
                            CurOffset.y = sin(TwoPi * (SubOffset / RadialSteps));
                            NewUV.x = uv.x + CurOffset.x * CurDistance;
                            NewUV.y = uv.y + CurOffset.y * CurDistance;
                            float distpow = pow(CurDistance, KernelPower);

                            CurColor += UnpackHeightmap(tex2Dlod(tex, float4(NewUV, 0, LOD))) * distpow;

                            accumdist += distpow;
                        }
                        SubOffset += RadialOffset;
                        incre++;
                    }
                    CurColor = CurColor;
                    CurColor /= accumdist;

                    result = float4(CurColor);
                }
    
                return result;


            }




            fixed4 frag (v2f i) : SV_Target
            {

                float origin = UnpackHeightmap(tex2Dlod(_Heightmap, float4(i.uv, 0, LOD)));
    
                float blurred = blur(i.uv, _Heightmap, 0.001, 32, 8);

                float curv = abs(pow((origin - blurred) * -400, 0.7));
    
                return float4(curv,curv, curv, 1.0);
            }
            ENDCG
        }


        Pass
        {
            Name "World Normal"
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            

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

            sampler2D_float _Heightmap;
            float _Height;
            float _TexelSize;

            fixed4 frag(v2f i) : SV_Target
            {

                float2 uv = i.uv.xy;
    
                float texelSize = 1.0 / (_TexelSize - 1.0);
                float2 xOffset = float2(texelSize, 0.0);
                float2 yOffset = float2(0.0, texelSize);
    
                float heightPosX = tex2D(_Heightmap, uv + xOffset).r * _Height;
                float heightNegX = tex2D(_Heightmap, uv - xOffset).r * _Height;
    
                float heightPosY = tex2D(_Heightmap, uv + yOffset).r * _Height;
                float heightNegY = tex2D(_Heightmap, uv - yOffset).r * _Height;
    
                float3 worldNormal = normalize(float3(heightNegX - heightPosX, 1, heightNegY - heightPosY));
                worldNormal = worldNormal * 0.5 + 0.5;
    
                return float4(worldNormal, 1.0);
            }
            ENDCG
        }


    }
}
