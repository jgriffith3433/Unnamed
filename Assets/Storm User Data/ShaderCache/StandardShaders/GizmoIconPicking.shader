// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

// Shader used for picking of gizmos/icons in the scene view.
// Just outputs vertex color, using gizmo-like depth testing passes.
Shader "PWStorm/Hidden/Editor Gizmo Icon Picking"
{
//BEGIN STORM EDIT

CGINCLUDE
#pragma multi_compile_instancing
#pragma multi_compile STORM_ENABLED

#include "StormIndirectBase.cginc"
ENDCG

//END STORM EDIT

    SubShader
    {
        Fog { Mode Off }

        Pass // regular pass
        {
            ZTest LEqual
            ZWrite On
        }
        Pass // occluded pass
        {
            ZTest Greater
            ZWrite Off
        }
    }
}
