// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

Shader "PWStorm/Hidden/Physics/DebugVisWire" {
//BEGIN STORM EDIT

CGINCLUDE
#pragma multi_compile_instancing
#pragma multi_compile STORM_ENABLED

#include "StormIndirectBase.cginc"
ENDCG

//END STORM EDIT

    SubShader {
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent" }
        Lighting Off
        Color [_Color] Blend SrcAlpha OneMinusSrcAlpha
            ZTest LEqual ZWrite Off
            Offset -1, -20
            Pass { Cull Back }
    }
}
