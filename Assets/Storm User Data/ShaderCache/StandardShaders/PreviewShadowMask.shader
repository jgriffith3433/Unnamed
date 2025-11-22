// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

// Draw a quad close to far plane,
// this will produce white color everywhere where
// anything was rendered.
Shader "PWStorm/Hidden/Preview Shadow Mask" {
//BEGIN STORM EDIT

CGINCLUDE
#pragma multi_compile_instancing
#pragma multi_compile STORM_ENABLED

#include "StormIndirectBase.cginc"
ENDCG

//END STORM EDIT

    SubShader {
        Tags { "ForceSupported" = "True" }
        Pass {
            ZTest GEqual
            ZWrite Off
            Cull Off
            SetTexture[_Dummy] { constantColor(1,1,1,1) combine constant }
        }
    }
}
