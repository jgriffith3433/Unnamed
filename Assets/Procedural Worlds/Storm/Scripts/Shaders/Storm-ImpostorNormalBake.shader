Shader "Hidden/PWStorm/ImpostorNormalBake"
{
    Properties
    {
        [Enum(UnityEngine.Rendering.CullMode)] _CullMode("Cull Mode", Int) = 0
        [HDR] _Color("Color", Color) = (1,1,1,1)
        [HDR]_MainTex("Albedo (RGB)", 2D) = "white" {}
        _Cutoff("Alpha Cutoff", Range(0,1)) = 0.01
        [NoScaleOffset][Normal]_BumpMap("", 2D) = "bump" {}
        _BumpMapScale("BumpMap Scale", Range(0, 3)) = 1.0
    }
    SubShader
    {
        Tags { "RenderType" = "Geometry" "Queue" = "Geometry" }
        LOD 100

        Pass //Normal
        {
            Name "Normal"
            Tags { "LightMode" = "Always" "_OutputName" = "normal" "_OutputField" = "_Normals"  "_OutputType" = "Normal" }
            Cull[_CullMode]

            CGPROGRAM

            #include "UnityCG.cginc"
            #include "UnityPBSLighting.cginc"

            #pragma vertex vert
            #pragma fragment frag

            struct v2f
            {
                float4 worldPos : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 TtoW0 : TEXCOORD1;
                float3 TtoW1 : TEXCOORD2;
                float3 TtoW2 : TEXCOORD3;
            };


			UNITY_INSTANCING_BUFFER_START(Props)
			half4   _Color;
			UNITY_INSTANCING_BUFFER_END(Props)

            sampler2D 	_MainTex;
            sampler2D 	_BumpMap;
            half 		_BumpMapScale;
            half     	_Cutoff;


            v2f vert(appdata_full v)
            {
                v2f o;
                UNITY_INITIALIZE_OUTPUT(v2f, o);
                o.worldPos = UnityObjectToClipPos(v.vertex);
                o.uv = v.texcoord;

                float3 Normal = UnityObjectToWorldNormal(v.normal);
                float3 Tangent = UnityObjectToWorldDir(v.tangent.xyz);
                float3 Binormal = cross(Normal, Tangent) * v.tangent.w;

                o.TtoW0 = float3(Tangent.x, Binormal.x, Normal.x);
                o.TtoW1 = float3(Tangent.y, Binormal.y, Normal.y);
                o.TtoW2 = float3(Tangent.z, Binormal.z, Normal.z);
				return o;
            }

            half4 frag(v2f i) : SV_Target {
				half alpha = (tex2D(_MainTex, i.uv) * UNITY_ACCESS_INSTANCED_PROP(Props, _Color)).a;
				clip(alpha - _Cutoff);

                float3 Normal = UnpackScaleNormal(tex2D(_BumpMap, i.uv), _BumpMapScale);
                Normal = mul(UNITY_MATRIX_V, normalize(float3(dot(i.TtoW0, Normal), dot(i.TtoW1, Normal), dot(i.TtoW2, Normal))));
                return half4((Normal * 0.5 + 0.5), 1);
			}
            ENDCG
        }
    }
}
