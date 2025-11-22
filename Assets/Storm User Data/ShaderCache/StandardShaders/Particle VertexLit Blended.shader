// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

Shader "PWStorm/Legacy Shaders/Particles/VertexLit Blended" {
//BEGIN STORM EDIT

CGINCLUDE
#pragma multi_compile_instancing
#pragma multi_compile STORM_ENABLED

#include "StormIndirectBase.cginc"
ENDCG

//END STORM EDIT

Properties {
    _EmisColor ("Emissive Color", Color) = (.2,.2,.2,0)
    _MainTex ("Particle Texture", 2D) = "white" {}
}

SubShader {
    Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane" }
    Tags { "LightMode" = "Vertex" }
    Cull Off
    Lighting On
    Material { Emission [_EmisColor] }
    ColorMaterial AmbientAndDiffuse
    ZWrite Off
    ColorMask RGB
    Blend SrcAlpha OneMinusSrcAlpha
    Pass {
        SetTexture [_MainTex] { combine primary * texture }
    }
}
}
