#ifndef _COLOMAP_FUNCTIONS
#define _COLOMAP_FUNCTIONS

//float4 _GrassMapBlendData;
//float4 _TreesMapBlendData;
float4 _StormVT_GrassMapData;
float4 _StormVT_TreeMapData;
float4 _StormVT_ImposterMapData;

float3 Unity_Blend_Lighten(float3 Base, float3 Blend, float Opacity)
{
    float3 Out = max(Blend, Base);
    Out = lerp(Base, Out, Opacity);
    return Out;
}

void SampleColorMaps_float(UnityTexture2D grassColorMap, UnityTexture2D grassNormalMap, UnityTexture2D treesColorMap, UnityTexture2D treesNormalMap, float2 uv,
float3 diffuse, float3 normal, float3 worldPos, 
out float3 blendedDiffuse, out float3 blendedNormal)
{

    half4 grassSampleColor = 0;
    half4 grassSampleNormal = 0;

    half4 treeSampleColor = 0;
    half4 treeSampleNormal = 0;


    grassSampleColor = grassColorMap.Sample(SamplerState_Linear_Repeat, uv);
    grassSampleNormal = grassNormalMap.Sample(SamplerState_Linear_Repeat, uv) * 2 - 1;

    treeSampleColor = treesColorMap.Sample(SamplerState_Linear_Repeat, uv);
    treeSampleNormal = treesNormalMap.Sample(SamplerState_Linear_Repeat, uv) * 2 - 1;

    

    float grassDistance = saturate(length(_WorldSpaceCameraPos.xyz - worldPos) / _StormVT_GrassMapData.x);
    grassDistance = saturate(pow(grassDistance, max(_StormVT_GrassMapData.y, 0.01) * 100));
    float treeDistance = saturate(length(_WorldSpaceCameraPos.xyz - worldPos) / _StormVT_TreeMapData.x);
    treeDistance = saturate(pow(treeDistance, max(_StormVT_TreeMapData.y, 0.01) * 100));

    float alphaBlendGrass = lerp(1, grassSampleColor.a, _StormVT_GrassMapData.z);
    float alphaBlendTrees = lerp(1, treeSampleColor.a, _StormVT_TreeMapData.z);

    float3 blendedGrassNormal = normalize(float3(normal.x + grassSampleNormal.x, normal.y * grassSampleNormal.y, normal.z + grassSampleNormal.z));
    float3 blendedTreeNormal = normalize(float3(normal.x + treeSampleNormal.x, normal.y * treeSampleNormal.y, normal.z + treeSampleNormal.z));

    /*blendedDiffuse = lerp(diffuse, grassSampleColor.rgb, grassDistance * grassSampleColor.a * _StormVT_ImposterMapData.x * _StormVT_GrassMapData.z);
    blendedNormal = lerp(normal, grassSampleNormal.rgb, grassDistance * grassSampleColor.a * _StormVT_ImposterMapData.y * _StormVT_GrassMapData.w);

    blendedDiffuse = lerp(blendedDiffuse, treeSampleColor.rgb, treeDistance * treeSampleColor.a * _StormVT_ImposterMapData.z * _StormVT_TreeMapData.z);
    blendedNormal = lerp(blendedNormal, treeSampleNormal.rgb, treeDistance * treeSampleColor.a * _StormVT_ImposterMapData.w * _StormVT_TreeMapData.w);*/


    //blendedDiffuse = lerp(diffuse, grassSampleColor.rgb, grassDistance * grassSampleColor.a * _StormVT_ImposterMapData.x * _StormVT_GrassMapData.z);
    blendedDiffuse = Unity_Blend_Lighten(diffuse, grassSampleColor.rgb, grassDistance * grassSampleColor.a * _StormVT_ImposterMapData.x * _StormVT_GrassMapData.z);
    blendedNormal = lerp(normal, blendedGrassNormal, grassDistance * grassSampleColor.a * _StormVT_ImposterMapData.y * _StormVT_GrassMapData.w);

    blendedDiffuse = lerp(blendedDiffuse, treeSampleColor.rgb, treeDistance * treeSampleColor.a * _StormVT_ImposterMapData.z * _StormVT_TreeMapData.z);
    //blendedDiffuse = Unity_Blend_Lighten(blendedDiffuse, treeSampleColor.rgb, treeDistance * treeSampleColor.a * _StormVT_ImposterMapData.z * _StormVT_TreeMapData.z);
    blendedNormal = lerp(blendedNormal, blendedTreeNormal, treeDistance * treeSampleColor.a * _StormVT_ImposterMapData.w * _StormVT_TreeMapData.w);
}

#endif