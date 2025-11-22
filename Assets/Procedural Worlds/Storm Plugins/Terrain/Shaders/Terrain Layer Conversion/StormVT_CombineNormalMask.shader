Shader "Hidden/StormVT_CombineNormalMask"
{
    Properties
    {
        _Normal ("Normal", 2D) = "white" {}
        _MaskMap ("MaskMap", 2D) = "white" {}
    }
    SubShader
    {
        // No culling or depth
        Cull Off ZWrite Off ZTest Always

        Pass
        {
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

            sampler2D _Normal;
            sampler2D _MaskMap;

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

            fixed4 frag (v2f i) : SV_Target
            {
                fixed3 normal = unpackNormal(tex2D(_Normal, i.uv));
                fixed4 mask = tex2D(_MaskMap, i.uv);

                return float4(normal.x * 0.5 + 0.5, normal.y * 0.5 + 0.5, mask.g, mask.a);
            }
            ENDCG
        }
    }
}
