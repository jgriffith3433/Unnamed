Shader "Hidden/PWStorm/ImpostorMaskBake"
{
    Properties
    {
        _Color("Color", Color) = (1,1,1,1)
        _MainTex("Albedo", 2D) = "white" {}
        _Cutoff("Alpha Cutoff", Range(0.0, 1.0)) = 1

        _Glossiness("Smoothness", Range(0.0, 1.0)) = 0.5
        _GlossMapScale("Smoothness Scale", Range(0.0, 1.0)) = 1.0
        [Enum(Metallic Alpha,0,Albedo Alpha,1)] _SmoothnessTextureChannel("Smoothness texture channel", Float) = 0

        [Gamma] _Metallic("Metallic", Range(0.0, 1.0)) = 0.0
        _MetallicGlossMap("Metallic", 2D) = "white" {}

        _OcclusionStrength("Strength", Range(0.0, 1.0)) = 1.0
        _OcclusionMap("Occlusion", 2D) = "white" {}

    }
    SubShader
    {
        Tags { "RenderType" = "Geometry" "Queue" = "Geometry" }
        LOD 100
        Pass //Packed Metallic
        {
            Name "Mask"
            Tags { "LightMode" = "Always" "_OutputName" = "mask" "_OutputField" = "_MetallicGlossMap" "_OutputType" = "Mask" }
            Cull[_CullMode]
            
            CGPROGRAM
            
            #include "UnityCG.cginc"
            #include "UnityPBSLighting.cginc"
            
            #pragma vertex vert
            #pragma fragment frag
            
            
            UNITY_INSTANCING_BUFFER_START(Props)
            half4   _Color;
            UNITY_INSTANCING_BUFFER_END(Props)

            sampler2D 	_MainTex;
            half     	_Cutoff;

            float       _Glossiness;
            float       _GlossMapScale;
            float       _SmoothnessTextureChannel;

            sampler2D   _MetallicGlossMap;
            half        _Metallic;

            sampler2D   _OcclusionMap;
            half        _OcclusionStrength;


            struct v2f
            {
                float4 worldPos : SV_POSITION;
                float2 uv : TEXCOORD0;
                float depth : TEXCOORD1;
            };

            v2f vert(appdata_full v)
            {
                v2f o;
                UNITY_INITIALIZE_OUTPUT(v2f, o);
                o.worldPos = UnityObjectToClipPos(v.vertex);
                o.uv = v.texcoord;
                o.depth = COMPUTE_DEPTH_01;
                return o;
            }

            half4 frag(v2f i) : SV_Target {
                half alpha = (tex2D(_MainTex, i.uv) * UNITY_ACCESS_INSTANCED_PROP(Props, _Color)).a;
                clip(alpha - _Cutoff);

                half4 map = tex2D(_MetallicGlossMap, i.uv);
                half4 occ = tex2D(_OcclusionMap, i.uv);

                half Smoothness = _Glossiness * (_SmoothnessTextureChannel * alpha + (1 - _SmoothnessTextureChannel) * map.a) * _GlossMapScale;
                half Metallic   = _Metallic * map.r;
                half Occlusion  = _OcclusionStrength * occ.g;
                half Depth      = 1.0 - i.depth;

                return half4(Metallic, Occlusion, Depth, Smoothness);
            }

            ENDCG
        }
    }
}
