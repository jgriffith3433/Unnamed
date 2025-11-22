#ifndef _STORM_VT_FUNCTIONS
#define _STORM_VT_FUNCTIONS


#define SamplerState_Point_Repeat sampler_point_repeat
SamplerState SamplerState_Point_Repeat;

#define SamplerState_Linear_Repeat sampler_linear_repeat
SamplerState SamplerState_Linear_Repeat;


TEXTURE2D(_SnowAlbedo);
TEXTURE2D(_SnowNormal);
TEXTURE2D(_SnowMask);

//Declare snow variables
float _SnowCurvatureInfluence;
float4 _SnowColor;
float _SnowHeight;
float _SnowFalloff;
float _SnowUVScale;

//Unpack normal function from DXT compression
float3 unpackNormal(float4 normal)
{
    float3 finalNormal;
#if UNITY_NO_DXT5nm
        finalNormal.xy = normal.xy * 2 - 1;
#else
    finalNormal.xy = normal.wy * 2 - 1;
#endif

    finalNormal.z = sqrt(1 - saturate(dot(finalNormal.xy, finalNormal.xy)));
    return finalNormal;
}

void GetSnowVariables_float(out float curvature, out float4 snowColor, out float snowHeight, out float snowFalloff, out float snowUVScale)
{
    curvature = _SnowCurvatureInfluence;
    snowColor = _SnowColor;
    snowHeight = _SnowHeight;
    snowFalloff = _SnowFalloff;
    snowUVScale = _SnowUVScale;
}

void SampleSnowTextures_float(float3 worldPosition, out float3 snowAlbedo, out float3 snowNormal, out float4 snowMask)
{
    float2 worldUV = worldPosition.xz / _SnowUVScale;
    snowAlbedo = SAMPLE_TEXTURE2D(_SnowAlbedo, sampler_linear_repeat, worldUV).rgb;
    snowMask = SAMPLE_TEXTURE2D(_SnowMask, sampler_linear_repeat, worldUV);
    snowNormal = unpackNormal(SAMPLE_TEXTURE2D(_SnowNormal, sampler_linear_repeat, worldUV));
}

void HeightMask_float(float height1, float height2, float mask, out float blend)
{
    mask = mask - 0.5f;
    blend = (height2 + mask) - (height1 + (mask * -1));
    blend = saturate((blend * 4) + 0.5f);
}

float3 WorldSpaceNormalBlend_float(float3 terrainNormal, float3 surfaceNormal, out float3 worldNormalBlended)
{
    float3 terrainTangent = cross(terrainNormal, float3(0, 0, 1));
    float3 terrainBitangent = cross(terrainTangent, terrainNormal);
    worldNormalBlended = surfaceNormal.x * terrainTangent + surfaceNormal.y * terrainBitangent + surfaceNormal.z * terrainNormal;
}

#endif