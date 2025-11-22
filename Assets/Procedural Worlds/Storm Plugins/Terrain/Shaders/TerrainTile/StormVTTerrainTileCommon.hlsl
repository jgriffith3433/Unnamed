#ifndef STORMVT_TERRAINTILECOMMON_INCLUDES
#define STORMVT_TERRAINTILECOMMON_INCLUDES

struct TerrainLayer {
    float4 tilingOffset;
    float4 remapMin;
    float4 remapMax;
    float4 color;
    float4 puddlesColor;
    float4 puddlesData;
    float4 puddlesRandomMask;
    float4 macroVariationColor;
    float4 macroVariationMask;
    float4 heightPackedData;
    float4 displacementPackedData;
    float4 stochasticPackedData;
    float4 detailNormalPackedData;
    float2 worldSpaceSize;
    float normalStrength;
    uint packedLayerSettings;
};


StructuredBuffer<TerrainLayer> terrainLayers;

#if !defined(SamplerState_Linear_Repeat)
    #define SamplerState_Linear_Repeat sampler_linear_repeat
    SamplerState SamplerState_Linear_Repeat;
#endif

#if !defined(SamplerState_Linear_Clamp)
#define SamplerState_Linear_Clamp sampler_linear_clamp
SamplerState SamplerState_Linear_Clamp;
#endif


#if !defined(SamplerState_Point_Repeat)
    #define SamplerState_Point_Repeat sampler_point_repeat
    SamplerState SamplerState_Point_Repeat;
#endif

//Declare Variables
float2 _TilePosition;
float2 _TileSize;
float2 _TerrainPosition;
float2 _TerrainSize;
float _RemapSize;
float4x4 _TileTRSMatrix;
float4 _StormVT_HeightBlendData;

//Current tile texture (will be null on first pass)
TEXTURE2D(_TileTexture);
TEXTURE2D(_ControlMap);

TEXTURE2D(TerrainLayerDiffuse0);
TEXTURE2D(TerrainLayerDiffuse1);
TEXTURE2D(TerrainLayerDiffuse2);
TEXTURE2D(TerrainLayerDiffuse3);

TEXTURE2D(TerrainLayerNormal0);
TEXTURE2D(TerrainLayerNormal1);
TEXTURE2D(TerrainLayerNormal2);
TEXTURE2D(TerrainLayerNormal3);

TEXTURE2D(TerrainLayerMask0);
TEXTURE2D(TerrainLayerMask1);
TEXTURE2D(TerrainLayerMask2);
TEXTURE2D(TerrainLayerMask3);

TEXTURE2D(_MacroVariationMap);
TEXTURE2D(_DetailNormalMap);

//Remap float
float Remap (float value, float from1, float to1, float from2, float to2) {
    return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
}

//Remap Tile
void RemapTile(float2 uv, float2 tilePos, float2 tileSize, float2 terrainPos, float2 terrainSize, out float2 tileUV, out float mask)
{
    //Create difference between positions
    float2 difference = tilePos - terrainPos;
    
    //Remap difference to uv space
    difference.x = (difference.x - 0) / (tileSize.x - 0) * (1 - 0) + 0;
    difference.y = (difference.y - 0) / (tileSize.y - 0) * (1 - 0) + 0;
    
    tileUV = uv + difference;
    float2 tileUVSize = tileSize / terrainSize;
    tileUV = (tileUV - 0.5) * tileUVSize + 0.5;
    
    //Create mask from tileUV
    mask = 0;
    if (tileUV.x >= 0 && tileUV.x <= 1 && tileUV.y >= 0 && tileUV.y <= 1)
    {
        mask = 1;
    }
    
}

float3 GetWorldPositionFromMatrix(float4 vertex, float4x4 tileMatrix)
{
    const float4x4 rotationMatrix = float4x4(
                    1, 0, 0, 0,
                    0, 0, -1, 0,
                    0, 1, 0, 0,
                    0, 0, 0, 1
                );
    
    vertex += float4(-0.5, -0.5, 0, 0);
    float4 fakeWorldVertex = mul(rotationMatrix, vertex);
    fakeWorldVertex = mul(tileMatrix, fakeWorldVertex);
    return mul(unity_ObjectToWorld, float4(fakeWorldVertex.xyz, 1.0)).xyz;
}



float heightBlend(float height, float mask, float blendFactor)
{
    float contrast = blendFactor * 10;
    float parabola = pow((4 * mask) * (1 - mask), blendFactor);
    float finalHeight = lerp(0, ((1 - height) - 0.5f), contrast) * parabola;
    return saturate(lerp(0.5, saturate(mask + finalHeight), contrast));

}



//Unpack normal function from DXT compression
float3 unpackNormal(float4 normal, float scale)
{
    float3 finalNormal;
#   if UNITY_NO_DXT5nm
    finalNormal.xy = normal.xy * 2 - 1;
    #else
    finalNormal.xy = normal.wy * 2 - 1;
    #endif
    finalNormal.xy *= scale;
    finalNormal.z = sqrt(1 - saturate(dot(finalNormal.xy, finalNormal.xy)));
    return finalNormal;
}

float3 unpackNormalNoScale(float4 normal)
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

float4 MacroVariation(float3 worldPos)
{
    float darken = 0.1;
    
    float2 scaleA = worldPos.xz / 10;
    float2 scaleB = worldPos.xz / 100;
    float2 scaleC = worldPos.xz / 1000;
    
    float4 variationC = _MacroVariationMap.Sample(sampler_linear_repeat, scaleC) + darken;
    float4 variationA = _MacroVariationMap.Sample(sampler_linear_repeat, scaleA) + darken;
    float4 variationB = _MacroVariationMap.Sample(sampler_linear_repeat, scaleB) + darken;
    
    float4 macroVariation =  saturate(variationA * variationB * variationC);
    macroVariation = saturate(pow(macroVariation, 2) * 8);
    return macroVariation;

}

/*
* Return random value from float2
*/
float2 hash(float2 p)
{
    return frac(sin(mul(float2x2(127.1, 311.7, 269.5, 183.3), p)) * 43758.5453);
    //float a = frac(sin(dot(p, float2(12.9898, 78.233))) * 43758.5453123);
    //return float2(a, a);

}

/*
* Stochastic Tiling Method
* Source: https://github.com/UnityLabs/procedural-stochastic-texturing
*/
void TriangleGrid(float2 uv,
    out float w1, out float w2, out float w3,
    out float2 vertex1, out float2 vertex2, out float2 vertex3)
{
    uv = uv * 3.464;

    const float2x2 gridToSkewedGrid = float2x2(1.0, 0.0, -0.57735027, 1.15470054);
    float2 skewedCoord = mul(gridToSkewedGrid, uv);

    int2 baseId = int2(floor(skewedCoord));
    float3 temp = float3(frac(skewedCoord), 0);
    temp.z = 1.0 - temp.x - temp.y;
    if (temp.z > 0.0)
    {
        w1 = temp.z;
        w2 = temp.y;
        w3 = temp.x;
        vertex1 = baseId;
        vertex2 = baseId + int2(0, 1);
        vertex3 = baseId + int2(1, 0);
    }
    else
    {
        w1 = -temp.z;
        w2 = 1.0 - temp.y;
        w3 = 1.0 - temp.x;
        vertex1 = baseId + int2(1, 1);
        vertex2 = baseId + int2(1, 0);
        vertex3 = baseId + int2(0, 1);
    }
    

}

float3 heightBlendWeights(float3 splatControl, float heightA, float heightB, float heightC, float heightBlend)
{
    //Height Blend
    float masks[] = { heightA, heightB, heightC };
    float3 splatHeight = float3(masks[0], masks[1], masks[2]) * splatControl.rgb;
    float maxHeight = max(splatHeight.r, max(splatHeight.g, splatHeight.b));
    float heightTransition = heightBlend;
    float transition = max(heightTransition, 1e-5);
    half3 weightedHeights = splatHeight + transition - maxHeight.xxx;
    weightedHeights = max(0, weightedHeights);
    weightedHeights = (weightedHeights + 1e-6) * splatControl;
    float sumHeight = max(dot(weightedHeights, float3(1, 1, 1)), 1e-6);
    return weightedHeights / sumHeight.xxx;
}

//Samples the packed albedo and normal maps, based on whether it's triplanar or not
void SeamlessSample_float(int index, Texture2D albedoTex, Texture2D normalTex, Texture2D maskTex, float2 uv, float4 deriv, float normalScale,
out float3 blendedAlbedo, out float3 blendedNormal, out float4 blendedMask)
{
    float w1, w2, w3;
    float2 vertex1, vertex2, vertex3;
    TriangleGrid(uv, w1, w2, w3, vertex1, vertex2, vertex3);

    float2 hash1 = hash(vertex1);
    float2 hash2 = hash(vertex2);
    float2 hash3 = hash(vertex3);
    
    float2 dir1 = normalize(hash1 * 2 - 1);
    float2 dir2 = normalize(hash2 * 2 - 1);
    float2 dir3 = normalize(hash3 * 2 - 1);
    
    float2x2 rotation1 = float2x2(dir1.y, dir1.x, -dir1.x, dir1.y);
    float2x2 rotation2 = float2x2(dir2.y, dir2.x, -dir2.x, dir2.y);
    float2x2 rotation3 = float2x2(dir3.y, dir3.x, -dir3.x, dir3.y);
    
    float2 uv1 = uv + hash1;
    float2 uv2 = uv + hash2;
    float2 uv3 = uv + hash3;
    
    uv1 = mul(float2x2(dir1.y, -dir1.x, dir1.x, dir1.y), uv1);
    uv2 = mul(float2x2(dir2.y, -dir2.x, dir2.x, dir2.y), uv2);
    uv3 = mul(float2x2(dir3.y, -dir3.x, dir3.x, dir3.y), uv3);
    
    float4 normalDeriv1 = normalTex.SampleGrad(sampler_linear_repeat, uv1, deriv.xy, deriv.zw);
    float4 normalDeriv2 = normalTex.SampleGrad(sampler_linear_repeat, uv2, deriv.xy, deriv.zw);
    float4 normalDeriv3 = normalTex.SampleGrad(sampler_linear_repeat, uv3, deriv.xy, deriv.zw);
    
    float4 mask1 = maskTex.SampleGrad(sampler_linear_repeat, uv1, deriv.xy, deriv.zw);
    float4 mask2 = maskTex.SampleGrad(sampler_linear_repeat, uv2, deriv.xy, deriv.zw);
    float4 mask3 = maskTex.SampleGrad(sampler_linear_repeat, uv3, deriv.xy, deriv.zw);
    
    float4 albedo1 = albedoTex.SampleGrad(sampler_linear_repeat, uv1, deriv.xy, deriv.zw);
    float4 albedo2 = albedoTex.SampleGrad(sampler_linear_repeat, uv2, deriv.xy, deriv.zw);
    float4 albedo3 = albedoTex.SampleGrad(sampler_linear_repeat, uv3, deriv.xy, deriv.zw);
    
    float3 normal1 = float3(normalDeriv1.ag * 2 - 1, 0.0);
    float3 normal2 = float3(normalDeriv2.ag * 2 - 1, 0.0);
    float3 normal3 = float3(normalDeriv3.ag * 2 - 1, 0.0);
    
    //Unrotate the normals
    normal1.xy = mul(rotation1, normal1.xy) * normalScale;
    normal2.xy = mul(rotation2, normal2.xy) * normalScale;
    normal3.xy = mul(rotation3, normal3.xy) * normalScale;
    
    //Reconstruct normal z
    normal1.z = sqrt(1 - saturate(dot(normal1.xy, normal1.xy)));
    normal2.z = sqrt(1 - saturate(dot(normal2.xy, normal2.xy)));
    normal3.z = sqrt(1 - saturate(dot(normal3.xy, normal3.xy)));
    
    //Normalize
    normal1 = normalize(normal1);
    normal2 = normalize(normal2);
    normal3 = normalize(normal3);
    
    //Remap height for stochastic height blend
    float height1 = (pow(abs(mask1.b), terrainLayers[index].stochasticPackedData.x) * terrainLayers[index].stochasticPackedData.y) + terrainLayers[index].stochasticPackedData.z;
    float height2 = (pow(abs(mask2.b), terrainLayers[index].stochasticPackedData.x) * terrainLayers[index].stochasticPackedData.y) + terrainLayers[index].stochasticPackedData.z;
    float height3 = (pow(abs(mask3.b), terrainLayers[index].stochasticPackedData.x) * terrainLayers[index].stochasticPackedData.y) + terrainLayers[index].stochasticPackedData.z;
    
    float3 splatControl = heightBlendWeights(float3(w1, w2, w3), height1, height2, height3, terrainLayers[index].stochasticPackedData.w);
    w1 = splatControl.r;
    w2 = splatControl.g;
    w3 = splatControl.b;

    blendedAlbedo = (w1 * albedo1) + (w2 * albedo2) + (w3 * albedo3);
    blendedNormal = (w1 * normal1) + (w2 * normal2) + (w3 * normal3);
    blendedMask = (w1 * mask1) + (w2 * mask2) + (w3 * mask3);

}

//Samples the packed albedo and normal maps, based on whether it's triplanar or not
void SeamlessSamplePacked_float(int index, Texture2D albedoTex, Texture2D normalTex, float2 uv, float4 deriv, float normalScale,
out float3 blendedAlbedo, out float3 blendedNormal, out float4 blendedMask)
{
    float w1, w2, w3;
    float2 vertex1, vertex2, vertex3;
    TriangleGrid(uv, w1, w2, w3, vertex1, vertex2, vertex3);

    float2 hash1 = hash(vertex1);
    float2 hash2 = hash(vertex2);
    float2 hash3 = hash(vertex3);
    
    float2 dir1 = normalize(hash1 * 2 - 1);
    float2 dir2 = normalize(hash2 * 2 - 1);
    float2 dir3 = normalize(hash3 * 2 - 1);
    
    float2x2 rotation1 = float2x2(dir1.y, dir1.x, -dir1.x, dir1.y);
    float2x2 rotation2 = float2x2(dir2.y, dir2.x, -dir2.x, dir2.y);
    float2x2 rotation3 = float2x2(dir3.y, dir3.x, -dir3.x, dir3.y);
    
    float2 uv1 = uv + hash1;
    float2 uv2 = uv + hash2;
    float2 uv3 = uv + hash3;
    
    uv1 = mul(float2x2(dir1.y, -dir1.x, dir1.x, dir1.y), uv1);
    uv2 = mul(float2x2(dir2.y, -dir2.x, dir2.x, dir2.y), uv2);
    uv3 = mul(float2x2(dir3.y, -dir3.x, dir3.x, dir3.y), uv3);
    
    float4 packedNormalDeriv1 = normalTex.SampleGrad(sampler_linear_repeat, uv1, deriv.xy, deriv.zw);
    float4 packedNormalDeriv2 = normalTex.SampleGrad(sampler_linear_repeat, uv2, deriv.xy, deriv.zw);
    float4 packedNormalDeriv3 = normalTex.SampleGrad(sampler_linear_repeat, uv3, deriv.xy, deriv.zw);
    
    float4 packedAlbedo1 = albedoTex.SampleGrad(sampler_linear_repeat, uv1, deriv.xy, deriv.zw);
    float4 packedAlbedo2 = albedoTex.SampleGrad(sampler_linear_repeat, uv2, deriv.xy, deriv.zw);
    float4 packedAlbedo3 = albedoTex.SampleGrad(sampler_linear_repeat, uv3, deriv.xy, deriv.zw);
    
    float3 normal1 = float3(packedNormalDeriv1.xy * 2 - 1, 0.0);
    float3 normal2 = float3(packedNormalDeriv2.xy * 2 - 1, 0.0);
    float3 normal3 = float3(packedNormalDeriv3.xy * 2 - 1, 0.0);
    
    //Unrotate the normals
    normal1.xy = mul(rotation1, normal1.xy) * normalScale;
    normal2.xy = mul(rotation2, normal2.xy) * normalScale;
    normal3.xy = mul(rotation3, normal3.xy) * normalScale;
    
    //Reconstruct normal z
    normal1.z = sqrt(1 - saturate(dot(normal1.xy, normal1.xy)));
    normal2.z = sqrt(1 - saturate(dot(normal2.xy, normal2.xy)));
    normal3.z = sqrt(1 - saturate(dot(normal3.xy, normal3.xy)));
    
    //Normalize
    normal1 = normalize(normal1);
    normal2 = normalize(normal2);
    normal3 = normalize(normal3);
    
    //Remap height for stochastic height blend
    float height1 = (pow(abs(packedAlbedo1.a), terrainLayers[index].stochasticPackedData.x) * terrainLayers[index].stochasticPackedData.y) + terrainLayers[index].stochasticPackedData.z;
    float height2 = (pow(abs(packedAlbedo2.a), terrainLayers[index].stochasticPackedData.x) * terrainLayers[index].stochasticPackedData.y) + terrainLayers[index].stochasticPackedData.z;
    float height3 = (pow(abs(packedAlbedo3.a), terrainLayers[index].stochasticPackedData.x) * terrainLayers[index].stochasticPackedData.y) + terrainLayers[index].stochasticPackedData.z;
    
    float3 splatControl = heightBlendWeights(float3(w1, w2, w3), height1, height2, height3, terrainLayers[index].stochasticPackedData.w);
    w1 = splatControl.r;
    w2 = splatControl.g;
    w3 = splatControl.b;

    float4 mask1 = float4(0, packedNormalDeriv1.b, packedAlbedo1.a, packedNormalDeriv1.a);
    float4 mask2 = float4(0, packedNormalDeriv2.b, packedAlbedo2.a, packedNormalDeriv2.a);
    float4 mask3 = float4(0, packedNormalDeriv3.b, packedAlbedo3.a, packedNormalDeriv3.a);
    
    blendedAlbedo = (w1 * packedAlbedo1) + (w2 * packedAlbedo2) + (w3 * packedAlbedo3);
    blendedNormal = (w1 * normal1) + (w2 * normal2) + (w3 * normal3);
    blendedMask = (w1 * mask1) + (w2 * mask2) + (w3 * mask3);

}

void SeamlessSampleSource0_float(int index, Texture2D albedoTex, Texture2D maskTex, float2 uv, float4 deriv,
out float4 blendedAlbedo, out float4 blendedMask)
{
    float w1, w2, w3;
    float2 vertex1, vertex2, vertex3;
    TriangleGrid(uv, w1, w2, w3, vertex1, vertex2, vertex3);

    float2 hash1 = hash(vertex1);
    float2 hash2 = hash(vertex2);
    float2 hash3 = hash(vertex3);
    
    float2 dir1 = normalize(hash1 * 2 - 1);
    float2 dir2 = normalize(hash2 * 2 - 1);
    float2 dir3 = normalize(hash3 * 2 - 1);
    
    float2x2 rotation1 = float2x2(dir1.y, dir1.x, -dir1.x, dir1.y);
    float2x2 rotation2 = float2x2(dir2.y, dir2.x, -dir2.x, dir2.y);
    float2x2 rotation3 = float2x2(dir3.y, dir3.x, -dir3.x, dir3.y);
    
    float2 uv1 = uv + hash1;
    float2 uv2 = uv + hash2;
    float2 uv3 = uv + hash3;
    
    uv1 = mul(float2x2(dir1.y, -dir1.x, dir1.x, dir1.y), uv1);
    uv2 = mul(float2x2(dir2.y, -dir2.x, dir2.x, dir2.y), uv2);
    uv3 = mul(float2x2(dir3.y, -dir3.x, dir3.x, dir3.y), uv3);

    float4 albedo1 = albedoTex.SampleGrad(sampler_linear_repeat, uv1, deriv.xy, deriv.zw);
    float4 albedo2 = albedoTex.SampleGrad(sampler_linear_repeat, uv2, deriv.xy, deriv.zw);
    float4 albedo3 = albedoTex.SampleGrad(sampler_linear_repeat, uv3, deriv.xy, deriv.zw);
    
    float4 mask1 = maskTex.SampleGrad(sampler_linear_repeat, uv1, deriv.xy, deriv.zw);
    float4 mask2 = maskTex.SampleGrad(sampler_linear_repeat, uv2, deriv.xy, deriv.zw);
    float4 mask3 = maskTex.SampleGrad(sampler_linear_repeat, uv3, deriv.xy, deriv.zw);
    
    float heightBlended = (mask1.b * w1 + mask2.b * w2 + mask3.b * w3);
    
    mask1.b = (pow(abs(mask1.b), terrainLayers[index].heightPackedData.x) * terrainLayers[index].heightPackedData.y) + terrainLayers[index].heightPackedData.z;
    mask2.b = (pow(abs(mask2.b), terrainLayers[index].heightPackedData.x) * terrainLayers[index].heightPackedData.y) + terrainLayers[index].heightPackedData.z;
    mask3.b = (pow(abs(mask3.b), terrainLayers[index].heightPackedData.x) * terrainLayers[index].heightPackedData.y) + terrainLayers[index].heightPackedData.z;
    
    float3 splatControl = heightBlendWeights(float3(w1, w2, w3), mask1.b, mask2.b, mask3.b, 0.1);
    w1 = splatControl.r;
    w2 = splatControl.g;
    w3 = splatControl.b;


    blendedAlbedo = (w1 * albedo1) + (w2 * albedo2) + (w3 * albedo3);
    blendedMask = (w1 * mask1) + (w2 * mask2) + (w3 * mask3);
    blendedMask.b = heightBlended;

}


void SeamlessSampleSource1_float(int index, Texture2D normalTex, Texture2D maskTex, float2 uv, float4 deriv, float normalScale,
out float4 blendedNormal, out float4 blendedMask)
{
    float w1, w2, w3;
    float2 vertex1, vertex2, vertex3;
    TriangleGrid(uv, w1, w2, w3, vertex1, vertex2, vertex3);

    float2 hash1 = hash(vertex1);
    float2 hash2 = hash(vertex2);
    float2 hash3 = hash(vertex3);
    
    float2 dir1 = normalize(hash1 * 2 - 1);
    float2 dir2 = normalize(hash2 * 2 - 1);
    float2 dir3 = normalize(hash3 * 2 - 1);
    
    float2x2 rotation1 = float2x2(dir1.y, dir1.x, -dir1.x, dir1.y);
    float2x2 rotation2 = float2x2(dir2.y, dir2.x, -dir2.x, dir2.y);
    float2x2 rotation3 = float2x2(dir3.y, dir3.x, -dir3.x, dir3.y);
    
    float2 uv1 = uv + hash1;
    float2 uv2 = uv + hash2;
    float2 uv3 = uv + hash3;
    
    uv1 = mul(float2x2(dir1.y, -dir1.x, dir1.x, dir1.y), uv1);
    uv2 = mul(float2x2(dir2.y, -dir2.x, dir2.x, dir2.y), uv2);
    uv3 = mul(float2x2(dir3.y, -dir3.x, dir3.x, dir3.y), uv3);
    
    float4 normalDeriv1 = normalTex.SampleGrad(sampler_linear_repeat, uv1, deriv.xy, deriv.zw);
    float4 normalDeriv2 = normalTex.SampleGrad(sampler_linear_repeat, uv2, deriv.xy, deriv.zw);
    float4 normalDeriv3 = normalTex.SampleGrad(sampler_linear_repeat, uv3, deriv.xy, deriv.zw);
    
    float3 normal1 = float3(normalDeriv1.ag * 2 - 1, 0.0);
    float3 normal2 = float3(normalDeriv2.ag * 2 - 1, 0.0);
    float3 normal3 = float3(normalDeriv3.ag * 2 - 1, 0.0);
    
    //Unrotate the normals
    normal1.xy = mul(rotation1, normal1.xy) * normalScale;
    normal2.xy = mul(rotation2, normal2.xy) * normalScale;
    normal3.xy = mul(rotation3, normal3.xy) * normalScale;
    
    //Reconstruct normal z
    normal1.z = sqrt(1 - saturate(dot(normal1.xy, normal1.xy)));
    normal2.z = sqrt(1 - saturate(dot(normal2.xy, normal2.xy)));
    normal3.z = sqrt(1 - saturate(dot(normal3.xy, normal3.xy)));
    
    //Normalize
    normal1 = normalize(normal1);
    normal2 = normalize(normal2);
    normal3 = normalize(normal3);
    
    
    float4 mask1 = maskTex.SampleGrad(sampler_linear_repeat, uv1, deriv.xy, deriv.zw);
    float4 mask2 = maskTex.SampleGrad(sampler_linear_repeat, uv2, deriv.xy, deriv.zw);
    float4 mask3 = maskTex.SampleGrad(sampler_linear_repeat, uv3, deriv.xy, deriv.zw);
    
    mask1.b = (pow(abs(mask1.b), terrainLayers[index].heightPackedData.x) * terrainLayers[index].heightPackedData.y) + terrainLayers[index].heightPackedData.z;
    mask2.b = (pow(abs(mask2.b), terrainLayers[index].heightPackedData.x) * terrainLayers[index].heightPackedData.y) + terrainLayers[index].heightPackedData.z;
    mask3.b = (pow(abs(mask3.b), terrainLayers[index].heightPackedData.x) * terrainLayers[index].heightPackedData.y) + terrainLayers[index].heightPackedData.z;
    
    float3 splatControl = heightBlendWeights(float3(w1, w2, w3), mask1.b, mask2.b, mask3.b, 0.1);
    w1 = splatControl.r;
    w2 = splatControl.g;
    w3 = splatControl.b;

    blendedNormal = float4((w1 * normal1) + (w2 * normal2) + (w3 * normal3), 0.0);
    blendedMask = (w1 * mask1) + (w2 * mask2) + (w3 * mask3);
}


bool GetFeatureToggle(uint packedSettings, uint position)
{
    return (packedSettings & (1U << position)) != 0;
}

void SampleDetailNormal(float3 worldPos, float4 packedData, out float3 nearDetailNormal, out float3 farDetailNormal)
{
    float2 nearScale = worldPos.xz / packedData.z;
    float2 farScale = worldPos.xz / packedData.w;
    
    nearDetailNormal = unpackNormal(_DetailNormalMap.Sample(sampler_linear_repeat, nearScale), packedData.x);
    farDetailNormal = unpackNormal(_DetailNormalMap.Sample(sampler_linear_repeat, farScale), packedData.y);
}

half4 SampleDetailNormal(float3 worldPos)
{
    float2 nearScale = worldPos.xz / 10;
    float2 farScale = worldPos.xz / 1000;
    
    
    half4 packedDetailNormal = 0;
    
    half4 detailNearSample = _DetailNormalMap.Sample(sampler_linear_repeat, nearScale);
    half4 detailFarSample = _DetailNormalMap.Sample(sampler_linear_repeat, farScale);
    
#if UNITY_NO_DXT5nm
    packedDetailNormal.xy = detailNearSample.xy * 2 - 1;
    packedDetailNormal.zw = detailFarSample.xy * 2 - 1;
#else
    packedDetailNormal.xy = detailNearSample.wy * 2 - 1;
    packedDetailNormal.zw = detailFarSample.wy * 2 - 1;
#endif
    
    return packedDetailNormal;
}

TEXTURE2D(_NormalMap);
TEXTURE2D(_HeightMap);
float _HeightScale;
float _HeightOffset;


void SampleLayer(int index, float3 worldPos, float2 baseUV, Texture2D albedoMap, Texture2D normalMap, half4 macroVariationSample, half4 detailNormalSample, out
float4 source0, out
float4 source1, out
float height)
{
    //Grab settings
    bool worldTiling = GetFeatureToggle(terrainLayers[index].packedLayerSettings, 0);
    bool stochastic = GetFeatureToggle(terrainLayers[index].packedLayerSettings, 1);
#ifndef _HEIGHTBLEND
    bool puddles = GetFeatureToggle(terrainLayers[index].packedLayerSettings, 2);
    bool puddlesMask = GetFeatureToggle(terrainLayers[index].packedLayerSettings, 3);
    bool macroVariation = GetFeatureToggle(terrainLayers[index].packedLayerSettings, 4);
    bool detailNormal = GetFeatureToggle(terrainLayers[index].packedLayerSettings, 5);
#endif
    
    //Create uv data
    float2 layerUV;
    if (worldTiling)
    {
        layerUV = worldPos.xz / terrainLayers[index].worldSpaceSize;
    }
    else
    {
        layerUV = baseUV * terrainLayers[index].tilingOffset.xy + terrainLayers[index].tilingOffset.zw;
    }
    
    float4 deriv = float4(ddx(layerUV), ddy(layerUV));
    
    //Set variables
    float3 baseColor;
    float3 normal;
    float4 mask;
    
#ifndef _HEIGHTBLEND
    
    //float3 nearDetailNormal = 0;
    //float3 farDetailNormal = 0;
    

    
    //If Detail Normal
    /*UNITY_BRANCH
    if (detailNormal)
    {
        SampleDetailNormal(worldPos, terrainLayers[index].detailNormalPackedData, nearDetailNormal, farDetailNormal);
    }*/
#endif

    
    //If Stochastic
    UNITY_BRANCH
    if (stochastic)
    {
        SeamlessSamplePacked_float(index, albedoMap, normalMap, layerUV, deriv, terrainLayers[index].normalStrength, baseColor, normal, mask);
        
#ifndef _HEIGHTBLEND
        if (detailNormal)
        {
            normal.xy += (detailNormalSample.xy * terrainLayers[index].detailNormalPackedData.x);
            normal.xy += (detailNormalSample.zw * terrainLayers[index].detailNormalPackedData.y);
            normal.z = sqrt(1.0 - saturate(dot(normal.xy, normal.xy)));
            normal = normalize(normal);
        }
#endif

    }
    else
    {
        //Else
        float4 albedoPacked = albedoMap.SampleGrad(sampler_linear_repeat, layerUV, deriv.xy, deriv.zw);
        //float4 albedoPacked = albedoMap.Sample(sampler_linear_repeat, layerUV);


        #ifndef _HEIGHTBLEND
            baseColor = albedoPacked.rgb;
            float4 normalPacked = normalMap.SampleGrad(sampler_linear_repeat, layerUV, deriv.xy, deriv.zw);
            //float4 normalPacked = normalMap.Sample(sampler_linear_repeat, layerUV);
            normal.xy = normalPacked.xy * 2 - 1;
            normal.xy *= terrainLayers[index].normalStrength;

            if (detailNormal)
            {
                normal.xy += (detailNormalSample.xy * terrainLayers[index].detailNormalPackedData.x);
                normal.xy += (detailNormalSample.zw * terrainLayers[index].detailNormalPackedData.y);
            }

            normal.z = sqrt(1.0 - saturate(dot(normal.xy, normal.xy)));
            normal = normalize(normal);
        
            mask = float4(0, normalPacked.b, albedoPacked.a, normalPacked.a);
        #else
            mask = float4(0, 0, albedoPacked.a, 0);
        #endif

    }
        
#ifndef _HEIGHTBLEND
    //Multiply color onto base color
    baseColor *= terrainLayers[index].color.rgb;
    
    //Remap AO
    mask.g = saturate(Remap(mask.g, 0, 1, terrainLayers[index].remapMin.g, terrainLayers[index].remapMax.g));
    //Remap Smoothness
    mask.a = saturate(Remap(mask.a, 0, 1, terrainLayers[index].remapMin.a, terrainLayers[index].remapMax.a));
    
#endif
    
    //Remap Displacement
    float displacement = (pow(abs(mask.b), terrainLayers[index].displacementPackedData.x) * terrainLayers[index].displacementPackedData.y) + terrainLayers[index].displacementPackedData.z;
    //Remap Height
    height = saturate((pow(abs(mask.b), terrainLayers[index].heightPackedData.x) * terrainLayers[index].heightPackedData.y) + terrainLayers[index].heightPackedData.z);
    
#ifndef _HEIGHTBLEND
    //Global Macro Variation Sample
    /*float4 macroVariationSample = 0;
    UNITY_BRANCH
    if (macroVariation || puddlesMask)
    {
        macroVariationSample = MacroVariation(worldPos);
    }*/
    
    //Blend macro variation for base color
    UNITY_BRANCH
    if (macroVariation)
    {
        float macroVariation = dot(macroVariationSample, terrainLayers[index].macroVariationMask);
        float4 macroVariationColor = terrainLayers[index].macroVariationColor;
        baseColor.rgb = lerp(baseColor.rgb, baseColor.rgb * saturate(macroVariationColor.rgb), macroVariation * macroVariationColor.a);
    }
    
    //Blend puddles in
    UNITY_BRANCH
    if (puddles)
    {
        float puddlesMacroVariation = 1;
        if (puddlesMask)
        {
            puddlesMacroVariation = 1 - saturate(dot(macroVariationSample, terrainLayers[index].puddlesRandomMask));
        }
        
        float puddlesHeight = heightBlend(mask.b, terrainLayers[index].puddlesData.r * puddlesMacroVariation, terrainLayers[index].puddlesData.w);
        float4 puddlesColor = terrainLayers[index].puddlesColor;
        float3 puddlesColorBlended = lerp(baseColor.rgb * puddlesColor.rgb, puddlesColor.rgb, puddlesColor.w);
        
        //Blend Base Color, Displacement
        baseColor.rgb = lerp(baseColor.rgb, puddlesColorBlended, puddlesHeight);
        displacement = lerp(displacement, 0, puddlesHeight);
        
        //Blend Normals, Mask
        float4 puddlesMask = float4(0, 1, 0, terrainLayers[index].puddlesData.g);
        normal.xy = lerp(normal.xy, normal.xy * terrainLayers[index].puddlesData.b, puddlesHeight);
        mask = lerp(mask, puddlesMask, puddlesHeight);
    }
    
#endif
    
#ifndef _HEIGHTBLEND
    //Encode final normal back to 0-1 range
    normal.xy = normal.xy * 0.5 + 0.5;
#endif
    

#ifndef _HEIGHTBLEND
    //Produce outputs
    source0 = float4(baseColor.rgb, displacement);
    source1 = float4(normal.rg, mask.g, mask.a);
#else
    source0 = float4(0,0,0,0);
    source1 = float4(0,0,0,0);
#endif
}

half blendByHeight(half texture1height, half texture2height, half control1height, half control2height, half overlapDepth, out half textureHeightOut, out half controlHeightOut)
{
    half texture1heightPrefilter = texture1height * sign(control1height);
    half texture2heightPrefilter = texture2height * sign(control2height);
    half height1 = texture1heightPrefilter + control1height;
    half height2 = texture2heightPrefilter + control2height;
    half blendFactor = (clamp(((height1 - height2) / overlapDepth), -1, 1) + 1) / 2;
    // Substract positive differences of the other control height to not make one texture height benefit too much from the other.
    textureHeightOut = max(0, texture1heightPrefilter - max(0, control2height - control1height)) * blendFactor + max(0, texture2heightPrefilter - max(0, control1height - control2height)) * (1 - blendFactor);
    // Propagate sum of control heights to not loose height.
    controlHeightOut = saturate(control1height + control2height);
    return blendFactor;
}

TEXTURE2D(_TileSource0);
TEXTURE2D(_TileSource1);
TEXTURE2D(_PreviousControl);
TEXTURE2D(_WorldNormal);


float ControlMix(float3 worldPos, float2 blitUV, float2 uv, float2 controlUV, Texture2D controlTex, float mask)
{
    float previousControlSum = 0;
    float previousHeight = 0;
    
    
    float4 splat = controlTex.Sample(SamplerState_Linear_Clamp, controlUV) * mask;
    
     //Global Macro Variation Sample
    half4 macroVariationSample = 0;
    macroVariationSample = MacroVariation(worldPos);

    //Global Detail Normal Sample
    half4 detailNormalCombined = 0;
    detailNormalCombined = SampleDetailNormal(worldPos);
    
    previousControlSum = _PreviousControl.Sample(sampler_linear_repeat, blitUV).r;
    previousHeight = _TileSource0.Sample(sampler_linear_repeat, blitUV).a;
    
    float4 sourceA_0, sourceA_1, sourceB_0, sourceB_1, sourceC_0, sourceC_1, sourceD_0, sourceD_1;
    float sourceA_height, sourceB_height, sourceC_height, sourceD_height;
    SampleLayer(0, worldPos, uv, TerrainLayerDiffuse0, TerrainLayerNormal0, macroVariationSample, detailNormalCombined, sourceA_0, sourceA_1, sourceA_height);
    SampleLayer(1, worldPos, uv, TerrainLayerDiffuse1, TerrainLayerNormal1, macroVariationSample, detailNormalCombined, sourceB_0, sourceB_1, sourceB_height);
    SampleLayer(2, worldPos, uv, TerrainLayerDiffuse2, TerrainLayerNormal2, macroVariationSample, detailNormalCombined, sourceC_0, sourceC_1, sourceC_height);
    SampleLayer(3, worldPos, uv, TerrainLayerDiffuse3, TerrainLayerNormal3, macroVariationSample, detailNormalCombined, sourceD_0, sourceD_1, sourceD_height);
    
    float heightA_BlendFactor = terrainLayers[0].heightPackedData.w;
    float heightB_BlendFactor = terrainLayers[1].heightPackedData.w;
    float heightC_BlendFactor = terrainLayers[2].heightPackedData.w;
    float heightD_BlendFactor = terrainLayers[3].heightPackedData.w;
    
    // Calculate Blend factors
    half textHeight0, textHeight1, textHeight2, textHeight3;
    half ctrlHeight0, ctrlHeight1, ctrlHeight2, ctrlHeight3;
    half blendFactor0 = blendByHeight(previousHeight, sourceA_height, previousControlSum, splat.r, heightA_BlendFactor, textHeight0, ctrlHeight0);
    half blendFactor01 = blendByHeight(textHeight0, sourceB_height, ctrlHeight0, splat.g, heightB_BlendFactor, textHeight1, ctrlHeight1);
    half blendFactor12 = blendByHeight(textHeight1, sourceC_height, ctrlHeight1, splat.b, heightC_BlendFactor, textHeight2, ctrlHeight2);
    half blendFactor23 = blendByHeight(textHeight2, sourceD_height, ctrlHeight2, splat.a, heightD_BlendFactor, textHeight3, ctrlHeight3);
    
    return lerp(previousControlSum, ctrlHeight3, mask);
   
}

void UnpackNormalSimpleLayer(inout float4 normalPacked, float normalStrength)
{
    float4 finalPackedNormal;
    float3 nP0Normal = float3((normalPacked.xy * 2 - 1) * normalStrength, 0);
    nP0Normal.z = sqrt(1.0 - saturate(dot(nP0Normal.xy, nP0Normal.xy)));
    nP0Normal = normalize(nP0Normal);
    normalPacked.xy = nP0Normal.xy * 0.5 + 0.5;
}

void SimpleLayerMix(float3 worldPos, float2 blitUV, float2 uv, float2 controlUV, Texture2D controlTex, float mask, out half4 baseColorMix, out half4 normalColorMix)
{
    baseColorMix = _TileSource0.Sample(sampler_linear_repeat, blitUV);
    normalColorMix = _TileSource1.Sample(sampler_linear_repeat, blitUV);
    
    half4 splat = controlTex.Sample(SamplerState_Linear_Clamp, controlUV) * mask;

    float4 tilingOffset0 = terrainLayers[0].tilingOffset;
    float4 tilingOffset1 = terrainLayers[1].tilingOffset;
    float4 tilingOffset2 = terrainLayers[2].tilingOffset;
    float4 tilingOffset3 = terrainLayers[3].tilingOffset;
    
    float2 UV0 = uv * tilingOffset0.xy + tilingOffset0.zw;
    float2 UV1 = uv * tilingOffset1.xy + tilingOffset1.zw;
    float2 UV2 = uv * tilingOffset2.xy + tilingOffset2.zw;
    float2 UV3 = uv * tilingOffset3.xy + tilingOffset3.zw;
    
    half4 aP0 = TerrainLayerDiffuse0.Sample(sampler_linear_repeat, UV0);
    half4 nP0 = TerrainLayerNormal0.Sample(sampler_linear_repeat, UV0);
    half4 aP1 = TerrainLayerDiffuse1.Sample(sampler_linear_repeat, UV1);
    half4 nP1 = TerrainLayerNormal1.Sample(sampler_linear_repeat, UV1);
    half4 aP2 = TerrainLayerDiffuse2.Sample(sampler_linear_repeat, UV2);
    half4 nP2 = TerrainLayerNormal2.Sample(sampler_linear_repeat, UV2);
    half4 aP3 = TerrainLayerDiffuse3.Sample(sampler_linear_repeat, UV3);
    half4 nP3 = TerrainLayerNormal3.Sample(sampler_linear_repeat, UV3);
    
    UnpackNormalSimpleLayer(nP0, terrainLayers[0].normalStrength);
    UnpackNormalSimpleLayer(nP1, terrainLayers[1].normalStrength);
    UnpackNormalSimpleLayer(nP2, terrainLayers[2].normalStrength);
    UnpackNormalSimpleLayer(nP3, terrainLayers[3].normalStrength);
    
    nP0.g = saturate(Remap(nP0.g, 0, 1, terrainLayers[0].remapMin.g, terrainLayers[0].remapMax.g));
    nP0.a = saturate(Remap(nP0.a, 0, 1, terrainLayers[0].remapMin.a, terrainLayers[0].remapMax.a));
    
    nP1.g = saturate(Remap(nP1.g, 0, 1, terrainLayers[1].remapMin.g, terrainLayers[1].remapMax.g));
    nP1.a = saturate(Remap(nP1.a, 0, 1, terrainLayers[1].remapMin.a, terrainLayers[1].remapMax.a));
    
    nP2.g = saturate(Remap(nP2.g, 0, 1, terrainLayers[2].remapMin.g, terrainLayers[2].remapMax.g));
    nP2.a = saturate(Remap(nP2.a, 0, 1, terrainLayers[2].remapMin.a, terrainLayers[2].remapMax.a));

    nP3.g = saturate(Remap(nP3.g, 0, 1, terrainLayers[3].remapMin.g, terrainLayers[3].remapMax.g));
    nP3.a = saturate(Remap(nP3.a, 0, 1, terrainLayers[3].remapMin.a, terrainLayers[3].remapMax.a));
    
    baseColorMix += aP0 * splat.r + aP1 * splat.g + aP2 * splat.b + aP3 * splat.a;
    normalColorMix += nP0 * splat.r + nP1 * splat.g + nP2 * splat.b + nP3 * splat.a;
}

TEXTURE2D(_PreviousMaxHeight);
TEXTURE2D(_PreviousSumHeight);

float GetMaxHeightMix(float3 worldPos, float2 blitUV, float2 uv, float2 controlUV, Texture2D controlTex, float mask)
{
    
    float4 splat = controlTex.Sample(sampler_linear_repeat, controlUV) * mask;
    
    float previousHeight = _PreviousMaxHeight.Sample(sampler_linear_repeat, blitUV).r;
    
    float4 sourceA_0, sourceA_1, sourceB_0, sourceB_1, sourceC_0, sourceC_1, sourceD_0, sourceD_1;
    float sourceA_height, sourceB_height, sourceC_height, sourceD_height;
    SampleLayer(0, worldPos, uv, TerrainLayerDiffuse0, TerrainLayerNormal0, 0, 0, sourceA_0, sourceA_1, sourceA_height);
    SampleLayer(1, worldPos, uv, TerrainLayerDiffuse1, TerrainLayerNormal1, 0, 0, sourceB_0, sourceB_1, sourceB_height);
    SampleLayer(2, worldPos, uv, TerrainLayerDiffuse2, TerrainLayerNormal2, 0, 0, sourceC_0, sourceC_1, sourceC_height);
    SampleLayer(3, worldPos, uv, TerrainLayerDiffuse3, TerrainLayerNormal3, 0, 0, sourceD_0, sourceD_1, sourceD_height);
    
    float maxHeight = max(previousHeight, sourceA_height * splat.r);
    maxHeight = max(maxHeight, sourceB_height * splat.g);
    maxHeight = max(maxHeight, sourceC_height * splat.b);
    maxHeight = max(maxHeight, sourceD_height * splat.a);
    
    maxHeight = saturate(maxHeight);
    
    return lerp(previousHeight, maxHeight, mask);
}

float GetSumHeightMix(float3 worldPos, float2 blitUV, float2 uv, float2 controlUV, Texture2D controlTex, float mask)
{
    
    float4 splat = controlTex.Sample(sampler_linear_repeat, controlUV) * mask;
    
    float previousSumHeight = _PreviousSumHeight.Sample(sampler_linear_repeat, blitUV).r;
    float maxHeight = _PreviousMaxHeight.Sample(sampler_linear_repeat, blitUV).r;
    
    float4 sourceA_0, sourceA_1, sourceB_0, sourceB_1, sourceC_0, sourceC_1, sourceD_0, sourceD_1;
    float sourceA_height, sourceB_height, sourceC_height, sourceD_height;
    SampleLayer(0, worldPos, uv, TerrainLayerDiffuse0, TerrainLayerNormal0, 0, 0, sourceA_0, sourceA_1, sourceA_height);
    SampleLayer(1, worldPos, uv, TerrainLayerDiffuse1, TerrainLayerNormal1, 0, 0, sourceB_0, sourceB_1, sourceB_height);
    SampleLayer(2, worldPos, uv, TerrainLayerDiffuse2, TerrainLayerNormal2, 0, 0, sourceC_0, sourceC_1, sourceC_height);
    SampleLayer(3, worldPos, uv, TerrainLayerDiffuse3, TerrainLayerNormal3, 0, 0, sourceD_0, sourceD_1, sourceD_height);
    
    float4 splatHeight = float4(sourceA_height, sourceB_height, sourceC_height, sourceD_height) * splat;
    
    //float4 heightTransition = float4(terrainLayers[0].heightPackedData.w, terrainLayers[1].heightPackedData.w, terrainLayers[2].heightPackedData.w, terrainLayers[3].heightPackedData.w);
    
    //Per pass loop 2 - create summed height for every 4 layers
    float4 transition = max(_StormVT_HeightBlendData.y, 1e-5);
    half4 weightedHeights = splatHeight + transition - maxHeight.xxxx;
    weightedHeights = max(0, weightedHeights);
    weightedHeights = (weightedHeights + 1e-6) * splat;
    float sumHeight = max(dot(weightedHeights, float4(1, 1, 1, 1)), 1e-6);
    
    
    //Add sum heights together
    sumHeight = saturate(previousSumHeight + sumHeight);
    
    return lerp(previousSumHeight, sumHeight, mask);
}

void HeightLayerMix(float3 worldPos, float2 blitUV, float2 uv, float2 controlUV, Texture2D controlTex, float mask, out half4 baseColorMix, out half4 normalColorMix)
{
    
    float4 previousSource0 = _TileSource0.Sample(sampler_linear_repeat, blitUV);
    float4 previousSource1 = _TileSource1.Sample(sampler_linear_repeat, blitUV);
    
    float4 splat = controlTex.Sample(sampler_linear_repeat, controlUV) * mask;
    
    //Global Macro Variation Sample
    half4 macroVariationSample = 0;
    macroVariationSample = MacroVariation(worldPos);

    //Global Detail Normal Sample
    half4 detailNormalCombined = 0;
    detailNormalCombined = SampleDetailNormal(worldPos);
    
    float previousSumHeight = _PreviousSumHeight.Sample(sampler_linear_repeat, blitUV).r;
    float maxHeight = _PreviousMaxHeight.Sample(sampler_linear_repeat, blitUV).r;
    
    float4 sourceA_0, sourceA_1, sourceB_0, sourceB_1, sourceC_0, sourceC_1, sourceD_0, sourceD_1;
    float sourceA_height, sourceB_height, sourceC_height, sourceD_height;
    SampleLayer(0, worldPos, uv, TerrainLayerDiffuse0, TerrainLayerNormal0, macroVariationSample, detailNormalCombined, sourceA_0, sourceA_1, sourceA_height);
    SampleLayer(1, worldPos, uv, TerrainLayerDiffuse1, TerrainLayerNormal1, macroVariationSample, detailNormalCombined, sourceB_0, sourceB_1, sourceB_height);
    SampleLayer(2, worldPos, uv, TerrainLayerDiffuse2, TerrainLayerNormal2, macroVariationSample, detailNormalCombined, sourceC_0, sourceC_1, sourceC_height);
    SampleLayer(3, worldPos, uv, TerrainLayerDiffuse3, TerrainLayerNormal3, macroVariationSample, detailNormalCombined, sourceD_0, sourceD_1, sourceD_height);
    
    float4 splatHeight = float4(sourceA_height, sourceB_height, sourceC_height, sourceD_height) * splat;
    
    //float4 heightTransition = float4(terrainLayers[0].heightPackedData.w, terrainLayers[1].heightPackedData.w, terrainLayers[2].heightPackedData.w, terrainLayers[3].heightPackedData.w);
    
    //Per pass loop 2 - create summed height for every 4 layers
    float4 transition = max(_StormVT_HeightBlendData.y, 1e-5);
    half4 weightedHeights = splatHeight + transition - maxHeight.xxxx;
    weightedHeights = max(0, weightedHeights);
    weightedHeights = (weightedHeights + 1e-6) * splat;

    splat = (weightedHeights / previousSumHeight);
    
    baseColorMix = previousSource0 + (sourceA_0 * splat.r);
    baseColorMix +=  sourceB_0 * splat.g;
    baseColorMix +=  sourceC_0 * splat.b;
    baseColorMix +=  sourceD_0 * splat.a;
    
    normalColorMix = previousSource1 + (sourceA_1 * splat.r);
    normalColorMix += sourceB_1 * splat.g;
    normalColorMix += sourceC_1 * splat.b;
    normalColorMix += sourceD_1 * splat.a;
    
    baseColorMix = lerp(previousSource0, baseColorMix, mask);
    normalColorMix = lerp(previousSource1, normalColorMix, mask);
}

void LayerMix(float3 worldPos, float2 blitUV, float2 uv, float2 controlUV, Texture2D controlTex, float mask, out half4 baseColorMix, out half4 normalColorMix)
{
    float4 previousSource0 = _TileSource0.Sample(sampler_linear_repeat, blitUV);
    float4 previousSource1 = _TileSource1.Sample(sampler_linear_repeat, blitUV);
    
    float4 splat = controlTex.Sample(sampler_linear_repeat, controlUV) * mask;
    
    //Global Macro Variation Sample
    half4 macroVariationSample = 0;
    macroVariationSample = MacroVariation(worldPos);

    //Global Detail Normal Sample
    half4 detailNormalCombined = 0;
    detailNormalCombined = SampleDetailNormal(worldPos);
    
    float4 sourceA_0, sourceA_1, sourceB_0, sourceB_1, sourceC_0, sourceC_1, sourceD_0, sourceD_1;
    float sourceA_height, sourceB_height, sourceC_height, sourceD_height;
    SampleLayer(0, worldPos, uv, TerrainLayerDiffuse0, TerrainLayerNormal0, macroVariationSample, detailNormalCombined, sourceA_0, sourceA_1, sourceA_height);
    SampleLayer(1, worldPos, uv, TerrainLayerDiffuse1, TerrainLayerNormal1, macroVariationSample, detailNormalCombined, sourceB_0, sourceB_1, sourceB_height);
    SampleLayer(2, worldPos, uv, TerrainLayerDiffuse2, TerrainLayerNormal2, macroVariationSample, detailNormalCombined, sourceC_0, sourceC_1, sourceC_height);
    SampleLayer(3, worldPos, uv, TerrainLayerDiffuse3, TerrainLayerNormal3, macroVariationSample, detailNormalCombined, sourceD_0, sourceD_1, sourceD_height);
    
    baseColorMix = previousSource0 + sourceA_0 * splat.r;
    baseColorMix +=  sourceB_0 * splat.g;
    baseColorMix +=  sourceC_0 * splat.b;
    baseColorMix +=  sourceD_0 * splat.a;
    
    normalColorMix = previousSource1 + sourceA_1 * splat.r;
    normalColorMix += sourceB_1 * splat.g;
    normalColorMix += sourceC_1 * splat.b;
    normalColorMix += sourceD_1 * splat.a;

    baseColorMix = lerp(previousSource0, baseColorMix, mask);
    normalColorMix = lerp(previousSource1, normalColorMix, mask);
}



//Declare snow variables
float _SnowCurvatureInfluence;
float4 _SnowColor;
float _SnowHeight;
float _SnowFalloff;
float _SnowUVScale;



TEXTURE2D(_SnowAlbedo);
TEXTURE2D(_SnowNormal);
TEXTURE2D(_SnowMask);

float4 linearDodge(float4 Base, float4 Blend, float Opacity)
{
    float4 Out = Base + Blend;
    return lerp(Base, Out, Opacity);
}

float HeightMask(float height1, float height2, float mask)
{
    mask = mask - 0.5f;
    float blend = (height2 + mask) - (height1 + (mask * -1));
    blend = saturate((blend * 4) + 0.5f);
    return blend;
}

float GetSnowMask(float2 controlUV)
{
    //Sample textures
    float4 normalMap = _NormalMap.Sample(SamplerState_Linear_Repeat, controlUV);
    float height = _HeightMap.Sample(SamplerState_Linear_Repeat, controlUV).r * _HeightScale + _HeightOffset;
                
    //Create mask
    float3 worldNormal = normalMap.xyz * 2 - 1;
    float curvature = normalMap.a;
    float topBlend = saturate(pow(worldNormal.y, 1) * 0.5);
    float curvatureRemap = saturate(curvature * 1);
    
    //Curvature influence
    curvature = linearDodge(topBlend, curvatureRemap, _SnowCurvatureInfluence);

    //Snow Height
    float snowHeight = smoothstep(-_SnowFalloff, _SnowFalloff, (height - _SnowHeight));
    
    //Final curvature remap
    curvature *= snowHeight;
    curvature = saturate(Remap(curvature, 0, 1, -1, 3));
    
    return curvature;
}

//Declare texture arrays
TEXTURE2D_ARRAY(_AlbedoArray);
TEXTURE2D_ARRAY(_NormalArray);

//Declare control maps
TEXTURE2D(_Control0);
TEXTURE2D(_Control1);
TEXTURE2D(_Control2);
TEXTURE2D(_Control3);

//Samples the packed albedo and normal maps, based on whether it's triplanar or not
void SeamlessSamplePackedArray(int index, float2 uv, float4 deriv, float normalScale,
out float3 blendedAlbedo, out float3 blendedNormal, out float4 blendedMask)
{
    float w1, w2, w3;
    float2 vertex1, vertex2, vertex3;
    TriangleGrid(uv, w1, w2, w3, vertex1, vertex2, vertex3);

    float2 hash1 = hash(vertex1);
    float2 hash2 = hash(vertex2);
    float2 hash3 = hash(vertex3);
    
    float2 dir1 = normalize(hash1 * 2 - 1);
    float2 dir2 = normalize(hash2 * 2 - 1);
    float2 dir3 = normalize(hash3 * 2 - 1);
    
    float2x2 rotation1 = float2x2(dir1.y, dir1.x, -dir1.x, dir1.y);
    float2x2 rotation2 = float2x2(dir2.y, dir2.x, -dir2.x, dir2.y);
    float2x2 rotation3 = float2x2(dir3.y, dir3.x, -dir3.x, dir3.y);
    
    float2 uv1 = uv + hash1;
    float2 uv2 = uv + hash2;
    float2 uv3 = uv + hash3;
    
    uv1 = mul(float2x2(dir1.y, -dir1.x, dir1.x, dir1.y), uv1);
    uv2 = mul(float2x2(dir2.y, -dir2.x, dir2.x, dir2.y), uv2);
    uv3 = mul(float2x2(dir3.y, -dir3.x, dir3.x, dir3.y), uv3);
    
    float4 packedNormalDeriv1 = _NormalArray.SampleGrad(sampler_linear_repeat, float3(uv1, index), deriv.xy, deriv.zw);
    float4 packedNormalDeriv2 = _NormalArray.SampleGrad(sampler_linear_repeat, float3(uv2, index), deriv.xy, deriv.zw);
    float4 packedNormalDeriv3 = _NormalArray.SampleGrad(sampler_linear_repeat, float3(uv3, index), deriv.xy, deriv.zw);
    
    float4 packedAlbedo1 = _AlbedoArray.SampleGrad(sampler_linear_repeat, float3(uv1, index), deriv.xy, deriv.zw);
    float4 packedAlbedo2 = _AlbedoArray.SampleGrad(sampler_linear_repeat, float3(uv2, index), deriv.xy, deriv.zw);
    float4 packedAlbedo3 = _AlbedoArray.SampleGrad(sampler_linear_repeat, float3(uv3, index), deriv.xy, deriv.zw);
    
    float3 normal1 = float3(packedNormalDeriv1.xy * 2 - 1, 0.0);
    float3 normal2 = float3(packedNormalDeriv2.xy * 2 - 1, 0.0);
    float3 normal3 = float3(packedNormalDeriv3.xy * 2 - 1, 0.0);
    
    //Unrotate the normals
    normal1.xy = mul(rotation1, normal1.xy) * normalScale;
    normal2.xy = mul(rotation2, normal2.xy) * normalScale;
    normal3.xy = mul(rotation3, normal3.xy) * normalScale;
    
    //Reconstruct normal z
    normal1.z = sqrt(1 - saturate(dot(normal1.xy, normal1.xy)));
    normal2.z = sqrt(1 - saturate(dot(normal2.xy, normal2.xy)));
    normal3.z = sqrt(1 - saturate(dot(normal3.xy, normal3.xy)));
    
    //Normalize
    normal1 = normalize(normal1);
    normal2 = normalize(normal2);
    normal3 = normalize(normal3);
    
    //Remap height for stochastic height blend
    float height1 = (pow(abs(packedAlbedo1.a), terrainLayers[index].stochasticPackedData.x) * terrainLayers[index].stochasticPackedData.y) + terrainLayers[index].stochasticPackedData.z;
    float height2 = (pow(abs(packedAlbedo2.a), terrainLayers[index].stochasticPackedData.x) * terrainLayers[index].stochasticPackedData.y) + terrainLayers[index].stochasticPackedData.z;
    float height3 = (pow(abs(packedAlbedo3.a), terrainLayers[index].stochasticPackedData.x) * terrainLayers[index].stochasticPackedData.y) + terrainLayers[index].stochasticPackedData.z;
    
    float3 splatControl = heightBlendWeights(float3(w1, w2, w3), height1, height2, height3, terrainLayers[index].stochasticPackedData.w);
    w1 = splatControl.r;
    w2 = splatControl.g;
    w3 = splatControl.b;

    float4 mask1 = float4(0, packedNormalDeriv1.b, packedAlbedo1.a, packedNormalDeriv1.a);
    float4 mask2 = float4(0, packedNormalDeriv2.b, packedAlbedo2.a, packedNormalDeriv2.a);
    float4 mask3 = float4(0, packedNormalDeriv3.b, packedAlbedo3.a, packedNormalDeriv3.a);
    
    blendedAlbedo = (w1 * packedAlbedo1) + (w2 * packedAlbedo2) + (w3 * packedAlbedo3);
    blendedNormal = (w1 * normal1) + (w2 * normal2) + (w3 * normal3);
    blendedMask = (w1 * mask1) + (w2 * mask2) + (w3 * mask3);

}

void SampleLayerArray(int index, float3 worldPos, float2 baseUV, half4 macroVariationSample, half4 detailNormalSample, out half4 source0, out half4 source1, out half height)
{
    //Set variables
    half3 baseColor;
    half3 normal;
    half4 mask;
    
    //Grab settings
    bool worldTiling = GetFeatureToggle(terrainLayers[index].packedLayerSettings, 0);
    bool stochastic = GetFeatureToggle(terrainLayers[index].packedLayerSettings, 1);
    bool puddles = GetFeatureToggle(terrainLayers[index].packedLayerSettings, 2);
    bool puddlesMask = GetFeatureToggle(terrainLayers[index].packedLayerSettings, 3);
    bool macroVariation = GetFeatureToggle(terrainLayers[index].packedLayerSettings, 4);
    bool detailNormal = GetFeatureToggle(terrainLayers[index].packedLayerSettings, 5);
    
    //Create uv data
    float2 layerUV;
    if (worldTiling)
    {
        layerUV = worldPos.xz / terrainLayers[index].worldSpaceSize;
    }
    else
    {
        layerUV = baseUV * terrainLayers[index].tilingOffset.xy + terrainLayers[index].tilingOffset.zw;
    }
    
    float4 deriv = float4(ddx(layerUV), ddy(layerUV));
    
    //If Stochastic
    UNITY_BRANCH
    if (stochastic)
    {
        SeamlessSamplePackedArray(index, layerUV, deriv, terrainLayers[index].normalStrength, baseColor, normal, mask);
        
        if (detailNormal)
        {
            normal.xy += (detailNormalSample.xy * terrainLayers[index].detailNormalPackedData.x);
            normal.xy += (detailNormalSample.zw * terrainLayers[index].detailNormalPackedData.y);
            normal.z = sqrt(1.0 - saturate(dot(normal.xy, normal.xy)));
            normal = normalize(normal);
        }

    }
    else
    {
        //Else
        half4 albedoPacked = _AlbedoArray.SampleGrad(sampler_linear_repeat, float3(layerUV, index), deriv.xy, deriv.zw);
        half4 normalPacked = _NormalArray.SampleGrad(sampler_linear_repeat, float3(layerUV, index), deriv.xy, deriv.zw);
        baseColor = albedoPacked.rgb;
        normal.xy = normalPacked.xy * 2 - 1;
        normal.xy *= terrainLayers[index].normalStrength;

        if (detailNormal)
        {
            normal.xy += (detailNormalSample.xy * terrainLayers[index].detailNormalPackedData.x);
            normal.xy += (detailNormalSample.zw * terrainLayers[index].detailNormalPackedData.y);
        }

        normal.z = sqrt(1.0 - saturate(dot(normal.xy, normal.xy)));
        normal = normalize(normal);
        
        mask = half4(0, normalPacked.b, albedoPacked.a, normalPacked.a);

    }
        
    //Multiply color onto base color
    baseColor *= terrainLayers[index].color.rgb;
    
    //Remap AO
    mask.g = saturate(Remap(mask.g, 0, 1, terrainLayers[index].remapMin.g, terrainLayers[index].remapMax.g));
    //Remap Smoothness
    mask.a = saturate(Remap(mask.a, 0, 1, terrainLayers[index].remapMin.a, terrainLayers[index].remapMax.a));
    
    
    //Remap Displacement
    half displacement = (pow(abs(mask.b), terrainLayers[index].displacementPackedData.x) * terrainLayers[index].displacementPackedData.y) + terrainLayers[index].displacementPackedData.z;
    //Remap Height
    height = saturate((pow(abs(mask.b), terrainLayers[index].heightPackedData.x) * terrainLayers[index].heightPackedData.y) + terrainLayers[index].heightPackedData.z);
    
    //Blend macro variation for base color
    UNITY_BRANCH
    if (macroVariation)
    {
        half macroVariation = dot(macroVariationSample, terrainLayers[index].macroVariationMask);
        half4 macroVariationColor = terrainLayers[index].macroVariationColor;
        baseColor.rgb = lerp(baseColor.rgb, baseColor.rgb * saturate(macroVariationColor.rgb), macroVariation * macroVariationColor.a);
    }
    
    //Blend puddles in
    UNITY_BRANCH
    if (puddles)
    {
        half puddlesMacroVariation = 1;
        if (puddlesMask)
        {
            puddlesMacroVariation = 1 - saturate(dot(macroVariationSample, terrainLayers[index].puddlesRandomMask));
        }
        
        half puddlesHeight = heightBlend(mask.b, terrainLayers[index].puddlesData.r * puddlesMacroVariation, terrainLayers[index].puddlesData.w);
        half4 puddlesColor = terrainLayers[index].puddlesColor;
        half3 puddlesColorBlended = lerp(baseColor.rgb * puddlesColor.rgb, puddlesColor.rgb, puddlesColor.w);
        
        //Blend Base Color, Displacement
        baseColor.rgb = lerp(baseColor.rgb, puddlesColorBlended, puddlesHeight);
        displacement = lerp(displacement, 0, puddlesHeight);
        
        //Blend Normals, Mask
        half4 puddlesMask = half4(0, 1, 0, terrainLayers[index].puddlesData.g);
        normal.xy = lerp(normal.xy, normal.xy * terrainLayers[index].puddlesData.b, puddlesHeight);
        mask = lerp(mask, puddlesMask, puddlesHeight);
    }
    
    
    //Encode final normal back to 0-1 range
    normal.xy = normal.xy * 0.5 + 0.5;

    //Produce outputs
    source0 = half4(baseColor.rgb, displacement);
    source1 = half4(normal.rg, mask.g, mask.a);
}

void SampleTerrain(float3 worldPos, float2 blitUV, float2 uv, float2 controlUV, float mask, out half4 baseColorMix, out half4 normalColorMix)
{
    
    baseColorMix = half4(0, 0, 0, 0);
    normalColorMix = half4(0, 0, 0, 0);
    
    half4 sourceA_0 = 0;
    half4 sourceA_1 = 0;
    half4 sourceB_0 = 0;
    half4 sourceB_1 = 0;
    half4 sourceC_0 = 0;
    half4 sourceC_1 = 0;
    half4 sourceD_0 = 0;
    half4 sourceD_1 = 0;
    half sourceA_height = 0;
    half sourceB_height = 0;
    half sourceC_height = 0;
    half sourceD_height = 0;
    
    //Sample control maps
    half4 splat0 = _Control0.Sample(sampler_linear_clamp, controlUV) * mask;
    half4 splat1 = _Control1.Sample(sampler_linear_clamp, controlUV) * mask;
    half4 splat2 = _Control2.Sample(sampler_linear_clamp, controlUV) * mask;
    half4 splat3 = _Control3.Sample(sampler_linear_clamp, controlUV) * mask;
    
    //Global Macro Variation Sample
    half4 macroVariationSample = MacroVariation(worldPos);

    //Global Detail Normal Sample
    half4 detailNormalCombined = SampleDetailNormal(worldPos);

    
    half4 previousSource0 = _TileSource0.Sample(sampler_linear_repeat, blitUV);
    half4 previousSource1 = _TileSource1.Sample(sampler_linear_repeat, blitUV);
    
    half2 weightData[] =
    {
        half2(splat0.r, 0),
        half2(splat0.g, 1),
        half2(splat0.b, 2),
        half2(splat0.a, 3),
        half2(splat1.r, 4),
        half2(splat1.g, 5),
        half2(splat1.b, 6),
        half2(splat1.a, 7),
        half2(splat2.r, 8),
        half2(splat2.g, 9),
        half2(splat2.b, 10),
        half2(splat2.a, 11),
        half2(splat3.r, 12),
        half2(splat3.g, 13),
        half2(splat3.b, 14),
        half2(splat3.a, 15)
    };
    
    // Bubble sort for simplicity (optimized for small arrays)
    [unroll]
    for (int i = 0; i < 7; i++)
    {
        [unroll]
        for (int j = 0; j < 7 - i; j++)
        {
            if (weightData[j].x < weightData[j + 1].x)
            {
                // Swap
                half2 temp = weightData[j];
                weightData[j] = weightData[j + 1];
                weightData[j + 1] = temp;
            }
        }
    }
    
    half4 weights = half4(weightData[0].x, weightData[1].x, weightData[2].x, weightData[3].x);
    half4 weightedIndex = half4(weightData[0].y, weightData[1].y, weightData[2].y, weightData[3].y);
    
    //Sample Layer A
    SampleLayerArray(weightedIndex.x, worldPos, uv, macroVariationSample, detailNormalCombined, sourceA_0, sourceA_1, sourceA_height);
    
    //Sample Layer B
    UNITY_BRANCH
    if(weights.y > 0)
    {
        SampleLayerArray(weightedIndex.y, worldPos, uv, macroVariationSample, detailNormalCombined, sourceB_0, sourceB_1, sourceB_height);
    }
    
    //Sample Layer C
    UNITY_BRANCH
    if (weights.z > 0)
    {
        SampleLayerArray(weightedIndex.z, worldPos, uv, macroVariationSample, detailNormalCombined, sourceC_0, sourceC_1, sourceC_height);
    }
    
    //Sample Layer D
    UNITY_BRANCH
    if (weights.w > 0)
    {
        SampleLayerArray(weightedIndex.w, worldPos, uv, macroVariationSample, detailNormalCombined, sourceD_0, sourceD_1, sourceD_height);
    }
    
    half heightBlendFactor = _StormVT_HeightBlendData.x == 1 ? _StormVT_HeightBlendData.y : 1 - 0.001f;

    //Height blend
    half masks[] = { sourceA_height, sourceB_height, sourceC_height, sourceD_height };
    half4 splatHeight = half4(masks[0], masks[1], masks[2], masks[3]) * weights;
    half maxHeight = max(splatHeight.r, max(splatHeight.g, max(splatHeight.b, splatHeight.a)));
    half transition = max(heightBlendFactor, 1e-5);
    half4 weightedHeights = splatHeight + transition - maxHeight.xxxx;
    weightedHeights = max(0, weightedHeights);
    weightedHeights = (weightedHeights + 1e-6) * weights;
    half sumHeight = max(dot(weightedHeights, half4(1, 1, 1, 1)), 1e-6);
    weights = weightedHeights / sumHeight.xxxx;
    
    
       
    baseColorMix = sourceA_0 * weights.r + sourceB_0 * weights.g + sourceC_0 * weights.b + sourceD_0 * weights.a;
    normalColorMix = sourceA_1 * weights.r + sourceB_1 * weights.g + sourceC_1 * weights.b + sourceD_1 * weights.a;
    
    #ifdef _SNOW_ON
        float curvature = GetSnowMask(blitUV);
    
        float2 worldUV = worldPos.xz / _SnowUVScale;
    
        float4 deriv = float4(ddx(worldUV), ddy(worldUV));
    
        float4 snowAlbedo;
        float3 snowNormal;
        float4 snowMask;
   
        snowAlbedo = _SnowAlbedo.SampleGrad(sampler_linear_repeat, worldUV, deriv.xy, deriv.zw);
        snowNormal = unpackNormalNoScale(_SnowNormal.SampleGrad(sampler_linear_repeat, worldUV, deriv.xy, deriv.zw));
        snowMask = _SnowMask.SampleGrad(sampler_linear_repeat, worldUV, deriv.xy, deriv.zw);
        snowNormal.xy = snowNormal.xy * 0.5 + 0.5;
    
        float tileHeight = baseColorMix.a;
    
        curvature = HeightMask(tileHeight, snowMask.b, curvature);
                
        float4 tileWithSnow0 = lerp(baseColorMix, float4(_SnowColor.rgb * snowAlbedo.rgb, snowMask.b), curvature);
        float4 tileWithSnow1 = lerp(normalColorMix, float4(snowNormal.xy, snowMask.ga), curvature);
    
        baseColorMix = lerp(baseColorMix, tileWithSnow0, mask);
        normalColorMix = lerp(normalColorMix, tileWithSnow1, mask);
    
    #endif
    
    
    baseColorMix = lerp(previousSource0, baseColorMix, mask);
    normalColorMix = lerp(previousSource1, normalColorMix, mask);
}

void SimpleLayerMixArray(float3 worldPos, float2 blitUV, float2 uv, float2 controlUV, Texture2D controlTex, float mask, out half4 baseColorMix, out half4 normalColorMix)
{
    baseColorMix = _TileSource0.Sample(sampler_linear_repeat, blitUV);
    normalColorMix = _TileSource1.Sample(sampler_linear_repeat, blitUV);
    
    half4 splat = _Control0.Sample(SamplerState_Linear_Clamp, controlUV) * mask;

    float4 tilingOffset0 = terrainLayers[0].tilingOffset;
    float4 tilingOffset1 = terrainLayers[1].tilingOffset;
    float4 tilingOffset2 = terrainLayers[2].tilingOffset;
    float4 tilingOffset3 = terrainLayers[3].tilingOffset;
    
    float2 UV0 = uv * tilingOffset0.xy + tilingOffset0.zw;
    float2 UV1 = uv * tilingOffset1.xy + tilingOffset1.zw;
    float2 UV2 = uv * tilingOffset2.xy + tilingOffset2.zw;
    float2 UV3 = uv * tilingOffset3.xy + tilingOffset3.zw;
    
    half4 aP0 = _AlbedoArray.Sample(sampler_linear_repeat, float3(UV0, 0));
    half4 nP0 = _NormalArray.Sample(sampler_linear_repeat, float3(UV0, 0));
    half4 aP1 = _AlbedoArray.Sample(sampler_linear_repeat, float3(UV1, 1));
    half4 nP1 = _NormalArray.Sample(sampler_linear_repeat, float3(UV1, 1));
    half4 aP2 = _AlbedoArray.Sample(sampler_linear_repeat, float3(UV2, 2));
    half4 nP2 = _NormalArray.Sample(sampler_linear_repeat, float3(UV2, 2));
    half4 aP3 = _AlbedoArray.Sample(sampler_linear_repeat, float3(UV3, 3));
    half4 nP3 = _NormalArray.Sample(sampler_linear_repeat, float3(UV3, 3));
    
    UnpackNormalSimpleLayer(nP0, terrainLayers[0].normalStrength);
    UnpackNormalSimpleLayer(nP1, terrainLayers[1].normalStrength);
    UnpackNormalSimpleLayer(nP2, terrainLayers[2].normalStrength);
    UnpackNormalSimpleLayer(nP3, terrainLayers[3].normalStrength);
    
    nP0.g = saturate(Remap(nP0.g, 0, 1, terrainLayers[0].remapMin.g, terrainLayers[0].remapMax.g));
    nP0.a = saturate(Remap(nP0.a, 0, 1, terrainLayers[0].remapMin.a, terrainLayers[0].remapMax.a));
    
    nP1.g = saturate(Remap(nP1.g, 0, 1, terrainLayers[1].remapMin.g, terrainLayers[1].remapMax.g));
    nP1.a = saturate(Remap(nP1.a, 0, 1, terrainLayers[1].remapMin.a, terrainLayers[1].remapMax.a));
    
    nP2.g = saturate(Remap(nP2.g, 0, 1, terrainLayers[2].remapMin.g, terrainLayers[2].remapMax.g));
    nP2.a = saturate(Remap(nP2.a, 0, 1, terrainLayers[2].remapMin.a, terrainLayers[2].remapMax.a));

    nP3.g = saturate(Remap(nP3.g, 0, 1, terrainLayers[3].remapMin.g, terrainLayers[3].remapMax.g));
    nP3.a = saturate(Remap(nP3.a, 0, 1, terrainLayers[3].remapMin.a, terrainLayers[3].remapMax.a));
    
    baseColorMix += aP0 * splat.r + aP1 * splat.g + aP2 * splat.b + aP3 * splat.a;
    normalColorMix += nP0 * splat.r + nP1 * splat.g + nP2 * splat.b + nP3 * splat.a;
    
}

#endif