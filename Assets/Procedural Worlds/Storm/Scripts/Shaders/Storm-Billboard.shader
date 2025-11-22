
Shader "PWStorm/Billboard"
{
    Properties
    {
        [hdr]_Color ("Color", Color) = (0.5,0.5,0.5,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _BumpMap ("Bumpmap", 2D) = "bump" {}
        _MetallicGlossMap("Metallic", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        [Gamma] _Metallic ("Metallic", Range(0,1)) = 0.0
        _OcclusionStrength("Strength", Range(0.0, 1.0)) = 0.5
        
        _Cutoff("Alpha Cutoff", Range(0.0, 1.0)) = 0.4
        
        [Enum(UnityEngine.Rendering.CullMode)] _Culling ("Cull Mode", Int) = 2
		[Enum(Off,0,On,1)] _ZWrite("ZWrite", Int) = 1
		[Enum(UnityEngine.Rendering.CompareFunction)] _ZTest ("ZTest", Int) = 4
		[Enum(None,0,Alpha,1,Red,8,Green,4,Blue,2,RGB,14,RGBA,15)] _ColorMask("Color Mask", Int) = 15
    }
    SubShader
    {
        Tags { "RenderType"="TransparentCutout" "Queue"="AlphaTest" }
        LOD 200
        
        Cull [_Culling]
		Offset [_Offset], [_Offset]
		ZWrite [_ZWrite]
		ZTest [_ZTest]
		ColorMask [_ColorMask]
        
        CGPROGRAM

        #pragma multi_compile _ LOD_FADE_CROSSFADE

        #pragma surface surf Standard vertex:vert addshadow
        #pragma multi_compile_instancing
        #pragma instancing_options procedural:storm_setup
        #pragma multi_compile STORM_ENABLED
        #include "StormIndirectBase.cginc"
        #pragma target 3.0
        
        sampler2D _MainTex;
        sampler2D _BumpMap;
        sampler2D _MetallicGlossMap;
        
        half _Cutoff , _OcclusionStrength;


        struct Input
        {
            float4 screenPos;
            float2 uv_MainTex;
            float faceFade;
            //fixed facing : VFACE;
        };

        half _Glossiness;
        half _Metallic;
        fixed4 _Color;
        
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)
        
        void vert (inout appdata_full v, out Input o) {
          UNITY_INITIALIZE_OUTPUT(Input,o);
          
          //float3 viewDir = UNITY_MATRIX_IT_MV[2].xyz;
          float3 viewDir = normalize(mul(v.vertex,UNITY_MATRIX_IT_MV));
          o.faceFade = saturate(dot(v.normal,viewDir));
          o.faceFade *= o.faceFade;
          o.faceFade *= o.faceFade;
          
      }

        void surf(Input IN, inout SurfaceOutputStandard o)
        {
        
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
            clip(c.a * IN.faceFade - _Cutoff);
        
            #ifdef LOD_FADE_CROSSFADE
                float2 vpos = IN.screenPos.xy / IN.screenPos.w * _ScreenParams.xy;
                UnityApplyDitherCrossFade(vpos);
            #endif
            
            half4 packedPBR = tex2D (_MetallicGlossMap, IN.uv_MainTex);
            float3 normal =  UnpackNormal (tex2D (_BumpMap, IN.uv_MainTex));
            
            o.Normal = normal;
            ///normal.z *= IN.facing;
            
            o.Albedo = c.rgb;
            
            o.Metallic = packedPBR.r * _Metallic;
            o.Occlusion = packedPBR.g * _OcclusionStrength;
            o.Smoothness = packedPBR.a * _Glossiness;
            
            o.Alpha = c.a;
        }
        ENDCG
    }
}

