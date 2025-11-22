// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

Shader "PWStorm/Hidden/CubeBlur" {
//BEGIN STORM EDIT

CGINCLUDE
#pragma multi_compile_instancing
#pragma multi_compile STORM_ENABLED

#include "StormIndirectBase.cginc"
ENDCG

//END STORM EDIT

    Properties {
        _MainTex ("Main", CUBE) = "" {}
        _Texel ("Texel", Float) = 0.0078125
        _Level ("Level", Float) = 0.
        _Scale ("Scale", Float) = 1.
    }

    CGINCLUDE
    #pragma vertex vert
    #pragma fragment frag
    #include "UnityCG.cginc"
    #include "HLSLSupport.cginc"

    struct v2f {
        half4 pos : SV_POSITION;
        half4 uvw : TEXCOORD0;
    };

    v2f vert(appdata_base v)
    {
        v2f o;
        o.pos = UnityObjectToClipPos(v.vertex);
        o.uvw = v.texcoord;
        return o;
    }

    UNITY_DECLARE_TEXCUBE(_MainTex);

    #define UNITY_SAMPLE_TEXCUBE_LOD_CLAMPED(tex, dir,  lod) max(half4(0.0, 0.0, 0.0, 0.0), UNITY_SAMPLE_TEXCUBE_LOD(tex, dir, lod))

    half _Level; // Workaround for Metal driver bug: please keep this uniform aligned to 4 bytes (case 899153)
    half _Texel;
    half _Scale;

    #define zero    half3(0., 0., 0.)
    #define one     half3(1., 1., 1.)
    #define two     half3(2., 2., 2.)

    half3 fold(half3 st, half3 face)
    {
        half3 c = min(max(st, -one), one);
        half3 f = abs(st - c);
        half m = max(max(f.x, f.y), f.z);
        return c - m*face;
    }

    half3 gauss(half d)
    {
        // compute coefficients for positions .5*d/.5, 1.5*d/.5 and 2.5*d/.5
        // this assumes a sigma of .5 for a density of 1.
        half3 v = half3(d, 3.*d, 5.*d)*_Scale;
        return exp(-v*v);
    }

    half4 frag_default(v2f i)
    {
        half3 st;

        half3 face = lerp(zero, i.uvw.xyz, abs(i.uvw.xyz)==one);
        half3 u = face.zxy*_Texel;
        half3 v = face.yzx*_Texel;
        half4 s = half4(i.uvw.xyz*(one - abs(face)), 0.);

        // modulate coefficients based on position (texel density on projected sphere)
        half w = 1. / sqrt(1. + dot(s.xyz, s.xyz));
        half3 C = gauss(w*w*w);

        half4 s1, s2, s3;
        half3 c;

        half3 up1 = fold(i.uvw.xyz + 1.5*u, face);
        half3 um1 = fold(i.uvw.xyz - 1.5*u, face);
        half3 up2 = fold(i.uvw.xyz + 2.5*u, face);
        half3 um2 = fold(i.uvw.xyz - 2.5*u, face);

        half3 vp1 = fold(i.uvw.xyz + 1.5*v, face);
        half3 vm1 = fold(i.uvw.xyz - 1.5*v, face);
        half3 vp2 = fold(i.uvw.xyz + 2.5*v, face);
        half3 vm2 = fold(i.uvw.xyz - 2.5*v, face);

        s = 0.;
        w = 0.;

        // first row

        c = C.xyz*C.zzz;

        st = i.uvw.xyz - 2.5*u - 2.5*v;
        st = fold(st, face);
        s3 = UNITY_SAMPLE_TEXCUBE_LOD_CLAMPED(_MainTex, st, _Level);

        st = i.uvw.xyz - 1.5*u - 2.5*v;
        st = fold(st, face);
        s2 = UNITY_SAMPLE_TEXCUBE_LOD_CLAMPED(_MainTex, st, _Level);

        st = vm2 - .5*u;
        s1 = UNITY_SAMPLE_TEXCUBE_LOD_CLAMPED(_MainTex, st, _Level);

        st = vm2 + .5*u;
        s1 += UNITY_SAMPLE_TEXCUBE_LOD_CLAMPED(_MainTex, st, _Level);

        st = i.uvw.xyz + 1.5*u - 2.5*v;
        st = fold(st, face);
        s2 += UNITY_SAMPLE_TEXCUBE_LOD_CLAMPED(_MainTex, st, _Level);

        st = i.uvw.xyz + 2.5*u - 2.5*v;
        st = fold(st, face);
        s3 += UNITY_SAMPLE_TEXCUBE_LOD_CLAMPED(_MainTex, st, _Level);

        w += dot(c, two);
        s1 = c.x*s1 + c.y*s2;
        s += c.z*s3;
        s += s1;

        // second row

        c = C.xyz*C.yyy;

        st = i.uvw.xyz + 2.5*u - 1.5*v;
        st = fold(st, face);
        s3 = UNITY_SAMPLE_TEXCUBE_LOD_CLAMPED(_MainTex, st, _Level);

        st = i.uvw.xyz + 1.5*u - 1.5*v;
        st = fold(st, face);
        s2 = UNITY_SAMPLE_TEXCUBE_LOD_CLAMPED(_MainTex, st, _Level);

        st = vm1 + .5*u;
        s1 = UNITY_SAMPLE_TEXCUBE_LOD_CLAMPED(_MainTex, st, _Level);

        st = vm1 - .5*u;
        s1 += UNITY_SAMPLE_TEXCUBE_LOD_CLAMPED(_MainTex, st, _Level);

        st = i.uvw.xyz - 1.5*u - 1.5*v;
        st = fold(st, face);
        s2 += UNITY_SAMPLE_TEXCUBE_LOD_CLAMPED(_MainTex, st, _Level);

        st = i.uvw.xyz - 2.5*u - 1.5*v;
        st = fold(st, face);
        s3 += UNITY_SAMPLE_TEXCUBE_LOD_CLAMPED(_MainTex, st, _Level);

        w += dot(c, two);
        s1 = c.x*s1 + c.y*s2;
        s += c.z*s3;
        s += s1;

        // third row

        c = C.xyz*C.xxx;

        st = um2 - .5*v;
        s3 = UNITY_SAMPLE_TEXCUBE_LOD_CLAMPED(_MainTex, st, _Level);

        st = um1 - .5*v;
        s2 = UNITY_SAMPLE_TEXCUBE_LOD_CLAMPED(_MainTex, st, _Level);

        st = i.uvw.xyz - .5*u - .5*v;
        s1 = UNITY_SAMPLE_TEXCUBE_LOD_CLAMPED(_MainTex, st, _Level);

        st = i.uvw.xyz + .5*u - .5*v;
        s1 += UNITY_SAMPLE_TEXCUBE_LOD_CLAMPED(_MainTex, st, _Level);

        st = up1 - .5*v;
        s2 += UNITY_SAMPLE_TEXCUBE_LOD_CLAMPED(_MainTex, st, _Level);

        st = up2 - .5*v;
        s3 += UNITY_SAMPLE_TEXCUBE_LOD_CLAMPED(_MainTex, st, _Level);

        w += dot(c, two);
        s1 = c.x*s1 + c.y*s2;
        s += c.z*s3;
        s += s1;

        // fourth row

        c = C.xyz*C.xxx;

        st = up2 + .5*v;
        s3 = UNITY_SAMPLE_TEXCUBE_LOD_CLAMPED(_MainTex, st, _Level);

        st = up1 + .5*v;
        s2 = UNITY_SAMPLE_TEXCUBE_LOD_CLAMPED(_MainTex, st, _Level);

        st = i.uvw.xyz + .5*u + .5*v;
        s1 = UNITY_SAMPLE_TEXCUBE_LOD_CLAMPED(_MainTex, st, _Level);

        st = i.uvw.xyz - .5*u + .5*v;
        s1 += UNITY_SAMPLE_TEXCUBE_LOD_CLAMPED(_MainTex, st, _Level);

        st = um1 + .5*v;
        s2 += UNITY_SAMPLE_TEXCUBE_LOD_CLAMPED(_MainTex, st, _Level);

        st = um2 + .5*v;
        s3 += UNITY_SAMPLE_TEXCUBE_LOD_CLAMPED(_MainTex, st, _Level);

        w += dot(c, two);
        s1 = c.x*s1 + c.y*s2;
        s += c.z*s3;
        s += s1;

        // fifth row

        c = C.xyz*C.yyy;

        st = i.uvw.xyz - 2.5*u + 1.5*v;
        st = fold(st, face);
        s3 = UNITY_SAMPLE_TEXCUBE_LOD_CLAMPED(_MainTex, st, _Level);

        st = i.uvw.xyz - 1.5*u + 1.5*v;
        st = fold(st, face);
        s2 = UNITY_SAMPLE_TEXCUBE_LOD_CLAMPED(_MainTex, st, _Level);

        st = vp1 - .5*u;
        s1 = UNITY_SAMPLE_TEXCUBE_LOD_CLAMPED(_MainTex, st, _Level);

        st = vp1 + .5*u;
        s1 += UNITY_SAMPLE_TEXCUBE_LOD_CLAMPED(_MainTex, st, _Level);

        st = i.uvw.xyz + 1.5*u + 1.5*v;
        st = fold(st, face);
        s2 += UNITY_SAMPLE_TEXCUBE_LOD_CLAMPED(_MainTex, st, _Level);

        st = i.uvw.xyz + 2.5*u + 1.5*v;
        st = fold(st, face);
        s3 += UNITY_SAMPLE_TEXCUBE_LOD_CLAMPED(_MainTex, st, _Level);

        w += dot(c, two);
        s1 = c.x*s1 + c.y*s2;
        s += c.z*s3;
        s += s1;

        // sixth row

        c = C.xyz*C.zzz;

        st = i.uvw.xyz + 2.5*u + 2.5*v;
        st = fold(st, face);
        s3 = UNITY_SAMPLE_TEXCUBE_LOD_CLAMPED(_MainTex, st, _Level);

        st = i.uvw.xyz + 1.5*u + 2.5*v;
        st = fold(st, face);
        s2 = UNITY_SAMPLE_TEXCUBE_LOD_CLAMPED(_MainTex, st, _Level);

        st = vp2 + .5*u;
        s1 = UNITY_SAMPLE_TEXCUBE_LOD_CLAMPED(_MainTex, st, _Level);

        st = vp2 - .5*u;
        s1 += UNITY_SAMPLE_TEXCUBE_LOD_CLAMPED(_MainTex, st, _Level);

        st = i.uvw.xyz - 1.5*u + 2.5*v;
        st = fold(st, face);
        s2 += UNITY_SAMPLE_TEXCUBE_LOD_CLAMPED(_MainTex, st, _Level);

        st = i.uvw.xyz - 2.5*u + 2.5*v;
        st = fold(st, face);
        s3 += UNITY_SAMPLE_TEXCUBE_LOD_CLAMPED(_MainTex, st, _Level);

        w += dot(c, two);
        s1 = c.x*s1 + c.y*s2;
        s += c.z*s3;
        s += s1;

        return s/w;
    }

    half4 frag_loop(v2f i)
    {
        half3 face = lerp(zero, i.uvw.xyz, abs(i.uvw.xyz) == one);
        half3 u = face.zxy*_Texel;
        half3 v = face.yzx*_Texel;
        half3 s = i.uvw.xyz*(one - abs(face));

        // modulate coefficients based on position (texel density on projected sphere)
        half w = 1. / sqrt(1. + dot(s, s));
        half3 C = gauss(w*w*w);

        half3 accuColor = half3(0.0, 0.0, 0.0);
        half accuWeight = 0.0;

        for (int y = 2; y >= 0; --y)
        {
            for (int iy = 0; iy < 2; iy++)
            {
                half ySign = iy * 2 - 1;
                half fy = ySign * (half(y)+0.5h);

                for (int x = 2; x >= 0; --x)
                {
                    half3 rgb = half3(0.0, 0.0, 0.0);
                    UNITY_UNROLL for (int ix = 0; ix < 2; ix++)
                    {
                        half xSign = ix * 2 - 1;
                        half fx = xSign * (half(x) + 0.5h);

                        half3 uvw = i.uvw.xyz + fx * u + fy * v;
                        uvw = fold(uvw, face);
                        rgb += UNITY_SAMPLE_TEXCUBE_LOD(_MainTex, uvw, _Level).rgb;
                    }

                    half weight = C[x] * C[y];
                    accuColor += rgb * weight;
                    accuWeight += weight * 2;
                }
            }
        }

        half4 result = half4(accuColor / accuWeight, 1.0);

        return result;
    }

    half4 frag_noblur(v2f i)
    {
        return UNITY_SAMPLE_TEXCUBE_LOD(_MainTex, i.uvw.xyz, _Level);
    }

    half4 frag(v2f  i) : SV_Target
    {
        #if (SHADER_TARGET < 30 || defined(SHADER_API_GLES))
            return frag_noblur(i);
        #elif defined(SHADER_API_GLES3) || (defined(SHADER_API_VULKAN) && defined(SHADER_API_MOBILE))
            // frag_default uses too many registers for Mali, register spilling makes it load/store bound
            // This path also does not blur the alpha channel and it does not clamp the sampled color to values >= 0
            return frag_loop(i);
        #else
            // TODO: check if frag_loop performs well on all platforms so it could replace frag_default
            return frag_default(i);
        #endif
    }

    ENDCG

    SubShader {
        Tags { "RenderType"="Opaque" }
        LOD 200
        Pass {
            ZTest Always
            Blend Off
            AlphaTest off
            Cull Off
            ZWrite Off
            Fog { Mode off }
            CGPROGRAM
            #pragma target 3.0
            ENDCG
        }
    }

    SubShader {
        Tags { "RenderType"="Opaque" }
        LOD 200
        Pass {
            ZTest Always
            Blend Off
            AlphaTest off
            Cull Off
            ZWrite Off
            Fog { Mode off }
            CGPROGRAM
            #pragma target 2.0
            ENDCG
        }
    }

//BEGIN STORM EDIT

CGPROGRAM

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
ENDCG

//END STORM EDIT
}
