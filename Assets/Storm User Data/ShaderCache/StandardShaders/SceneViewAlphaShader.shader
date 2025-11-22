// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

Shader "PWStorm/Hidden/Sceneview Alpha Shader" {
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
            ZTest Always
            Blend DstAlpha Zero
            Color (1,1,1,1)
        }
    }
}
