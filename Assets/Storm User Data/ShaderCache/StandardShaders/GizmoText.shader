// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

// text (used by icons)
Shader "PWStorm/Hidden/Editor Gizmo Text"
{
//BEGIN STORM EDIT

CGINCLUDE
#pragma multi_compile_instancing
#pragma multi_compile STORM_ENABLED

#include "StormIndirectBase.cginc"
ENDCG

//END STORM EDIT

    Properties
    {
        _MainTex ("", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "ForceSupported" = "True" "Queue" = "Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off Cull Off Fog { Mode Off }
        Pass // regular pass
        {
            ZTest LEqual
            SetTexture [_MainTex] { combine primary, texture alpha * primary }
        }
        Pass // occluded pass
        {
            ZTest Greater
            SetTexture [_MainTex] { combine primary, texture alpha * primary }
        }
    }
}
