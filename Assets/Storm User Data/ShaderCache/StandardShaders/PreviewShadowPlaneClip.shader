// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

Shader "PWStorm/Hidden/Preview Shadow Plane Clip" {
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
            ZWrite Off
            Cull Off
            SetTexture[_Dummy] { constantColor(0,0,0,0) combine constant }
        }
    }
}
