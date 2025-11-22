#ifndef _STORM_VT_FUNCTIONS
#define _STORM_VT_FUNCTIONS

#define SamplerState_Point_Repeat sampler_point_repeat
SamplerState SamplerState_Point_Repeat;

#define SamplerState_Point_Clamp sampler_point_clamp
SamplerState SamplerState_Point_Clamp;

#define SamplerState_Linear_Clamp sampler_linear_clamp
SamplerState SamplerState_Linear_Clamp;

#define SamplerState_Linear_Repeat sampler_linear_repeat
SamplerState SamplerState_Linear_Repeat;

#define SamplerState_Trilinear_Repeat_Aniso8 sampler_trilinear_repeat
SamplerState SamplerState_Trilinear_Repeat_Aniso8;



//Scripting Defines
#define MAXARRAYELEMENTS 16
#define DEBUG_TILE_SCALE .96
#define MIP_BIAS 0

//Page Table Defines
float4 _StormVT_PageTableArrayData;
float4 _StormVT_ZoomLevelData;
TEXTURE2D_ARRAY(_StormVT_PageTableTex2DArray);
float4 _StormVT_SnappedCameraPosition;
float4 _StormVT_TerrainHeightScale;
float4 _StormVT_TotalTerrainBounds;
float4 _StormVT_WorldOffset;
int StormVT_PhysicalTexturesInitialized;
float _StormVT_DisplacementAmount;
int _StormVT_DebugTiles;
float _MipBias;
float4 _StormVT_PhysicalTextureGridSize;
float4 _StormVT_PhysicalTextureTileData;
float4 _StormVT_TileMipData;
float4 _StormVT_TileFeedbackPositionData;
float4 _StormVT_DistanceMapData;
float4 _StormVT_GrassMapData;
float4 _StormVT_TreeMapData;
float4 _StormVT_ImposterMapData;

void GetTotalTerrainBoundsMask_float(float3 position, out half mask)
{
    float2 uv = (position.xz - _StormVT_TotalTerrainBounds.xy) / _StormVT_TotalTerrainBounds.zw + 0.5;
    
    mask = 1;
    mask = ceil(saturate(1 - max(
            max(uv.x, 1 - uv.x),
            max(uv.y, 1 - uv.y)
        )));
}

half GetTotalTerrainBoundsMask(float3 position)
{
    float2 uv = (position.xz - _StormVT_TotalTerrainBounds.xy) / _StormVT_TotalTerrainBounds.zw + 0.5;
    
    half mask = 1.0;
    mask = ceil(saturate(1 - max(
        max(uv.x, 1 - uv.x),
        max(uv.y, 1 - uv.y)
    )));
    return mask;
}


uniform float4 _StormVT_PageTableDataArray[MAXARRAYELEMENTS];

struct PageTableData
{
    float2 centerPos;
    float worldScale;
    float padding;
};

//Physical Texture Defines
const float4 _StormVT_PageFracScale;
const float4 _StormVT_TexScale;
const float4 _StormVT_TexBias;
const float4 _StormVT_Terrain_PageFracScale;
const float4 _StormVT_Terrain_TexScale;
const float4 _StormVT_Terrain_TexBias;

TEXTURE2D(_PageTable);
TEXTURE2D(_PhysicalTextureHeight);

TEXTURE2D(_StormVT_PhysicalTextureHeight);
TEXTURE2D(_StormVT_PhysicalTextureNormal);
TEXTURE2D(_StormVT_PhysicalTextureSource0);
TEXTURE2D(_StormVT_PhysicalTextureSource1);
TEXTURE2D(_StormVT_PhysicalTextureDistanceMapColor);
TEXTURE2D(_StormVT_PhysicalTextureDistanceMapNormal);
TEXTURE2D(_StormVT_PhysicalTextureGrassColor);
TEXTURE2D(_StormVT_PhysicalTextureTreeColor);


SamplerState sampler_StormVT_PhysicalTextureSource0_Trilinear_Repeat_Aniso8;



void GetTerrainHeightScale_float(out float height, out float offset)
{
    height = _StormVT_TerrainHeightScale.r;
    offset = _StormVT_TerrainHeightScale.g;
}

void GetSnappedCameraPos_float(out float2 cameraPos)
{
    cameraPos = _StormVT_SnappedCameraPosition.xz;
}

void GetVolumeWorldSize_float(out float size)
{
    size = _StormVT_PageTableArrayData.z;

}

void GetVolumeUVs_float(float3 position, out float2 volumeUVs)
{
    const int lastIndex = min(_StormVT_PageTableArrayData.w - _StormVT_PageTableArrayData.x, MAXARRAYELEMENTS) - 1;
    volumeUVs = (position.xz - _StormVT_PageTableDataArray[lastIndex].xy) / _StormVT_PageTableDataArray[lastIndex].z + 0.5;
}

float2 Remap(float2 value, float2 from1, float2 to1, float2 from2, float2 to2)
{
    return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
}


//Functions
void PageTableUVs_float(float3 position, out float2 UVs, out float ringIndex, out float volumeMask)
{
    //Sample Page UVs
    float2 newUV = 0;
    
    //Calculate initial derivates, otherwise we get seams between page table sizes
    float2 initialUV = (position.xz - _StormVT_SnappedCameraPosition.xy) / _StormVT_PageTableArrayData.z;
    
    const int numLoops = (int) min(_StormVT_PageTableArrayData.w - _StormVT_PageTableArrayData.x, MAXARRAYELEMENTS);
    
    const float baseSize = _StormVT_PageTableArrayData.z;
    const float2 snapPos = _StormVT_SnappedCameraPosition.xy;
    const int maxIndex = (int)(_StormVT_PageTableArrayData.w - 1);
    for (int i = 0; i < numLoops; i++)
    {
        const int index = maxIndex - i;
        
        // Use bit shift for power of 2 division - more efficient
        half size = baseSize / (1 << (i + 2));
        float2 centerPos = floor(snapPos / size) * size + size;
        newUV = (position.xz - centerPos) / (size * 4) + 0.5;

        // More efficient bounds check using a single operation
        half mask = ceil(saturate(1 - max(
            max(newUV.x, 1 - newUV.x),
            max(newUV.y, 1 - newUV.y)
        )));

        if(i == 0)
        {
            volumeMask = mask;
        }
        UVs = lerp(UVs, newUV, mask);
        ringIndex = lerp(ringIndex, index, mask);
    }
   
}


void VirtualTextureUVs_float(float3 position, float2 uv, SamplerState ss, float Index, float Lod, float volumeMask, out float3 physicalTextureCoords, out float2 fracUVs, out float zoom)
{
#ifdef _BRANCHING_ON
    UNITY_BRANCH
    if(volumeMask > 0.0)
    {
        float4 physPage = SAMPLE_TEXTURE2D_ARRAY_LOD(_StormVT_PageTableTex2DArray, ss, uv, Index, Lod) * _StormVT_TexScale + _StormVT_TexBias;

        float nodeScale =  (1 << (int)physPage.z) * 0.0625f;
        float2 pageFrac = frac(position.xz / nodeScale ); 
    
    
        physicalTextureCoords = float3(pageFrac * _StormVT_PageFracScale.xy + physPage.xy, ((physPage.z == 0) ? 0 : 1) * volumeMask);
        fracUVs = pageFrac;
        zoom = 1;
    }
    else
    {
        physicalTextureCoords = float3(0,0,volumeMask);
        fracUVs = 0;
        zoom = 0;
    }
#else
    float4 physPage = SAMPLE_TEXTURE2D_ARRAY_LOD(_StormVT_PageTableTex2DArray, ss, uv, Index, Lod) * _StormVT_TexScale + _StormVT_TexBias;

    float nodeScale =  (1 << (int)physPage.z) * 0.0625f;
    float2 pageFrac = frac(position.xz / nodeScale ); 
    
    
    physicalTextureCoords = float3(pageFrac * _StormVT_PageFracScale.xy + physPage.xy, ((physPage.z == 0) ? 0 : 1) * volumeMask);
    fracUVs = pageFrac;
    zoom = 1;
#endif
   
}


void SampleVirtualUVs(float3 worldPos, SamplerState ss, out float3 UVs, out float2 fracUVs, out float zoomShift)
{
    float2 pageUVs;
    float ringIndex;
    float volumeMask;

    
    PageTableUVs_float(worldPos, pageUVs, ringIndex, volumeMask);

    float3 mip0PhysicalTextureCoords;
    float2 mip0FracUVs;
    float mip0Zoom;
    float2 mip0RawUVs;
    
    VirtualTextureUVs_float(worldPos, pageUVs, ss, ringIndex, 0, volumeMask, mip0PhysicalTextureCoords, mip0FracUVs, mip0Zoom);
    
    UVs = mip0PhysicalTextureCoords;
    fracUVs = mip0FracUVs;
    zoomShift = mip0Zoom;
}

float4 DebugPageTable(float3 worldPos, SamplerState ss, int lod)
{
    float2 pageUVs;
    float ringIndex;
    float volumeMask;

    
    PageTableUVs_float(worldPos, pageUVs, ringIndex, volumeMask);

    float4 physPage = SAMPLE_TEXTURE2D_ARRAY_LOD(_StormVT_PageTableTex2DArray, ss, pageUVs, ringIndex, lod) * _StormVT_TexScale + _StormVT_TexBias;
    return physPage;
}

float4 SamplePhysicalTextureNormal(float3 uv, SamplerState ss, float Lod, float nearDistance)
{

    float4 rawTerrainData = SAMPLE_TEXTURE2D_LOD(_StormVT_PhysicalTextureNormal, ss, uv.xy, Lod);
    float3 normal = float3(rawTerrainData.xy * 2 - 1, 0);
    normal = float3(normal.x, sqrt(1 - saturate(dot(normal.xy, normal.xy))), normal.y);
    normal = normalize(normal);
    return float4(normal.rgb, rawTerrainData.b);
}

float4 SamplePhysicalTextureSource0(float3 uv, SamplerState ss, float Lod)
{
    return SAMPLE_TEXTURE2D_LOD(_StormVT_PhysicalTextureSource0, ss, uv.xy, Lod) * uv.z;
}

float4 SamplePhysicalTextureSource1(float3 uv, SamplerState ss, float Lod)
{
    return SAMPLE_TEXTURE2D_LOD(_StormVT_PhysicalTextureSource1, ss, uv.xy, Lod) * uv.z;
}

float4 SamplePhysicalTextureSource0Grad(float3 uv, SamplerState ss)
{
    return SAMPLE_TEXTURE2D_LOD(_StormVT_PhysicalTextureSource0, ss, uv.xy, 0) * uv.z;
}

float4 SamplePhysicalTextureSource1Grad(float3 uv, SamplerState ss)
{
    return SAMPLE_TEXTURE2D_LOD(_StormVT_PhysicalTextureSource1, ss, uv.xy, 0) * uv.z;
}

float4 SampleDistanceMapColor(float2 uv, SamplerState ss)
{
    return SAMPLE_TEXTURE2D_LOD(_StormVT_PhysicalTextureDistanceMapColor, ss, uv.xy, 0);
}

float3 SampleDistanceMapNormal(float2 uv, SamplerState ss)
{
    return SAMPLE_TEXTURE2D_LOD(_StormVT_PhysicalTextureDistanceMapNormal, ss, uv.xy, 0).rgb * 2 - 1;
}

void BlendDistanceData(float2 uv, SamplerState ss, float distance, float3 diffuse, float3 normal, out float3 blendedColor, out float3 blendedNormal)
{
    float4 distanceColor = float4(diffuse, 0);
    float3 distanceNormal = normal;

    UNITY_BRANCH
    if(_StormVT_DistanceMapData.y)
    {
        distanceColor = SampleDistanceMapColor(uv, ss);
    }
    UNITY_BRANCH
    if(_StormVT_DistanceMapData.z)
    {
        distanceNormal = SampleDistanceMapNormal(uv, ss);
    }
    float alphablend = lerp(1, distanceColor.a, _StormVT_DistanceMapData.w);

    blendedColor = lerp(diffuse, distanceColor.rgb, distance * alphablend);
    blendedNormal = lerp(normal, distanceNormal, distance * alphablend);
}

float3 Unity_Blend_Lighten(float3 Base, float3 Blend, float Opacity)
{
    float3 Out = max(Blend, Base);
    Out = lerp(Base, Out, Opacity);
    return Out;
}

void SampleColorMaps(float2 uv, SamplerState ss,
float3 diffuse, float3 normal, float3 worldPos, 
out float3 blendedDiffuse, out float3 blendedNormal)
{

    half4 grassSampleColor = 0;
    half4 grassSampleNormal = 0;

    half4 treeSampleColor = 0;
    half4 treeSampleNormal = 0;

    half4 combinedNormals = 0;

    grassSampleColor = SAMPLE_TEXTURE2D_LOD(_StormVT_PhysicalTextureGrassColor, ss, uv.xy, 0);
    treeSampleColor = SAMPLE_TEXTURE2D_LOD(_StormVT_PhysicalTextureTreeColor, ss, uv.xy, 0);
    combinedNormals = SAMPLE_TEXTURE2D_LOD(_StormVT_PhysicalTextureDistanceMapNormal, ss, uv.xy, 0) * 2 - 1;

    float grassDistance = saturate(length(_WorldSpaceCameraPos.xyz - worldPos) / _StormVT_GrassMapData.x);
    grassDistance = saturate(pow(grassDistance, max(_StormVT_GrassMapData.y, 0.01) * 100));
    float treeDistance = saturate(length(_WorldSpaceCameraPos.xyz - worldPos) / _StormVT_TreeMapData.x);
    treeDistance = saturate(pow(treeDistance, max(_StormVT_TreeMapData.y, 0.01) * 100));

    //float alphaBlendGrass = lerp(1, grassSampleColor.a, _StormVT_GrassMapData.z);
    //float alphaBlendTrees = lerp(1, treeSampleColor.a, _StormVT_TreeMapData.z);

    float3 blendedGrassNormal = float3(normal.x + combinedNormals.x, 0, normal.z + combinedNormals.y);
    blendedGrassNormal.y = sqrt(1 - saturate(dot(blendedGrassNormal.xz, blendedGrassNormal.xz)));

    float3 blendedTreeNormal = float3(normal.x + combinedNormals.z, 0, normal.z + combinedNormals.w);
    blendedTreeNormal.y = sqrt(1 - saturate(dot(blendedTreeNormal.xz, blendedTreeNormal.xz)));

    //blendedDiffuse = lerp(diffuse, grassSampleColor.rgb, grassDistance * grassSampleColor.a * _StormVT_ImposterMapData.x * _StormVT_GrassMapData.z);
    blendedDiffuse = lerp(diffuse, grassSampleColor.rgb, grassDistance * grassSampleColor.a * _StormVT_ImposterMapData.x * _StormVT_GrassMapData.z);
    blendedNormal = lerp(normal, blendedGrassNormal, grassDistance * grassSampleColor.a * _StormVT_ImposterMapData.y * _StormVT_GrassMapData.w);

    blendedDiffuse = lerp(blendedDiffuse, treeSampleColor.rgb, treeDistance * treeSampleColor.a * _StormVT_ImposterMapData.z * _StormVT_TreeMapData.z);
    //blendedDiffuse = Unity_Blend_Lighten(blendedDiffuse, treeSampleColor.rgb, treeDistance * treeSampleColor.a * _StormVT_ImposterMapData.z * _StormVT_TreeMapData.z);
    blendedNormal = lerp(blendedNormal, blendedTreeNormal, treeDistance * treeSampleColor.a * _StormVT_ImposterMapData.w * _StormVT_TreeMapData.w);
}

//Declare snow variables
float _SnowCurvatureInfluence;
float4 _SnowColor;
float _SnowHeight;
float _SnowFalloff;
float _SnowUVScale;

void SamplePBR(float3 uv, SamplerState ss, int lod, out half3 diffuse, out half3 normal, out half4 mask)
{
    half4 source0 = SamplePhysicalTextureSource0Grad(uv, ss);
    half4 source1 = SamplePhysicalTextureSource1Grad(uv, ss);
    
    diffuse = source0.rgb;
    mask = half4(0, source1.b, source0.a, source1.a);
    normal.xy = source1.xy * 2 - 1;
    normal.z = sqrt(1 - saturate(dot(normal.xy, normal.xy)));
    normal = normal.xzy;
}



void SampleMaterialPBR(float3 uvMip0, float3 uvMip1, SamplerState ss, out half3 diffuse, out half3 normal, out half4 mask)
{
    half3 diffuseMip0;
    half3 normalMip0;
    half4 maskMip0;
    
    half3 diffuseMip1;
    half3 normalMip1;
    half4 maskMip1;

    SamplePBR(uvMip0, ss, 0, diffuseMip0, normalMip0, maskMip0);

    diffuse = diffuseMip0;
    normal = normalMip0;
    mask = maskMip0;
    
   
   
    
}

float RectangleMask(float2 UV, float Width, float Height)
{
#if !defined(SHADER_STAGE_RAY_TRACING)
    float2 d = abs(UV * 2 - 1) - float2(Width, Height);
    d = 1 - d / max(fwidth(d), 0.01);
    return saturate(min(d.x, d.y));
#else
    return 1;
#endif
}

float PageMask(float2 UV)
{
    float2 centerUV = abs(UV - 0.5);
    float mask = 1 - max(centerUV.x, centerUV.y);
    mask = saturate(pow(abs(mask * 2), -40));
    return mask;
}

void SamplePBRTextures(float2 uv, SamplerState ss, float4 deriv, float lod, out half3 diffuse, out half3 normal, out half4 mask)
{
    half4 source0 = SAMPLE_TEXTURE2D_LOD(_StormVT_PhysicalTextureSource0, ss, uv.xy, 0);
    half4 source1 = SAMPLE_TEXTURE2D_LOD(_StormVT_PhysicalTextureSource1, ss, uv.xy, 0);
    
    diffuse = source0.rgb;
    mask = half4(0, source1.b, source0.a, source1.a);
    normal.xy = source1.xy * 2 - 1;
    normal.z = sqrt(1 - saturate(dot(normal.xy, normal.xy)));
    normal = normal.xzy;
}

float computeMipLevel(float2 uv, float lodBias, float minMipLevel, float maxMipLevel, float2 physicalTextureSize)
{

    float2 coord_pixels = uv * physicalTextureSize;
    
    float2 x_deriv = ddx(coord_pixels);
    float2 y_deriv = ddy(coord_pixels);
    
    float d = max(dot(x_deriv, x_deriv), dot(y_deriv, y_deriv));
    float m = 0.5 * log2(d) - lodBias;

    // Clamp the mip level to the range [minMipLevel, maxMipLevel]
    m = clamp(m, minMipLevel, maxMipLevel);
    
    return floor(m);
}

void mipDebug_float(float2 uv, float lodBias, float minMipLevel, float maxMipLevel, float2 physicalTextureSize, float tileSize, out float4 result, out float3 returnUV)
{
    float mip_level = computeMipLevel(uv, lodBias, minMipLevel, maxMipLevel, physicalTextureSize);
    float2 page_coords = floor(uv * tileSize / exp2(mip_level));

     // Convert to [0,255] RGBA color and return:
    float4 page_id = float4(page_coords, mip_level + 1.0, 1.0);
    result = page_id / 65535.0;

    returnUV = float3((uv * tileSize / exp2(mip_level)), mip_level);
}

void SamplePhysicalTextures(float3 worldPos, SamplerState pbrSampler, out half3 diffuse, out half3 normal, out half4 mask)
{
    //Sample Page UVs
    half volumeMask = 1;
    int ringIndex = 0;
    float2 volumeUVs = 0;
    float2 newUV = 0;
    
    //Calculate initial derivates, otherwise we get seams between page table sizes
    float2 initialUV = (worldPos.xz - _StormVT_SnappedCameraPosition.xy) / _StormVT_PageTableArrayData.z;
    
    const int numLoops = (int) min(_StormVT_PageTableArrayData.w - _StormVT_PageTableArrayData.x, MAXARRAYELEMENTS);
    
    const float baseSize = _StormVT_PageTableArrayData.z;
    const float2 snapPos = _StormVT_SnappedCameraPosition.xy;
    const int maxIndex = (int)(_StormVT_PageTableArrayData.w - 1);
    for (int i = 0; i < numLoops; i++)
    {
        const int index = maxIndex - i;
        
        // Use bit shift for power of 2 division - more efficient
        half size = baseSize / (1 << (i + 2));
        float2 centerPos = floor(snapPos / size) * size + size;
        newUV = (worldPos.xz - centerPos) / (size * 4) + 0.5;
        
        // More efficient bounds check using a single operation
        volumeMask = ceil(saturate(1 - max(
            max(newUV.x, 1 - newUV.x),
            max(newUV.y, 1 - newUV.y)
        )));

        volumeUVs = lerp(volumeUVs, newUV, volumeMask);
        ringIndex = lerp(ringIndex, index, volumeMask);
    }

    float2 terrainPosUV = (worldPos.xz - _StormVT_TileFeedbackPositionData.xy) / 4096;
    
    half anisoLOD = 0;

#ifndef _STORMVT_LOW_END_ON
    //Calculate anisotropic blend
    float maxAniso = 4;
    float maxAnisoLog2 = log2(maxAniso);
    float2 dx = ddx(terrainPosUV);
    float2 dy = ddy(terrainPosUV);
    float px = dot(dx, dx);
    float py = dot(dy, dy);
    float maxLod = 0.5 * log2(max(px, py));
    float minLod = 0.5 * log2(min(px, py));
    anisoLOD = maxLod - min(maxLod - minLod, maxAnisoLog2);
#endif
    


    //Sample Page Table Texture
    float4 pageTableMip0 = SAMPLE_TEXTURE2D_ARRAY_LOD(_StormVT_PageTableTex2DArray, sampler_point_repeat, volumeUVs, ringIndex, anisoLOD - 0.5) * _StormVT_TexScale + _StormVT_TexBias;


#ifndef _STORMVT_LOW_END_ON
    float4 pageTableMip1 = SAMPLE_TEXTURE2D_ARRAY_LOD(_StormVT_PageTableTex2DArray, sampler_point_repeat, volumeUVs, ringIndex, anisoLOD + 0.5) * _StormVT_TexScale + _StormVT_TexBias;
#endif
    //Create uv space tile sizes
    float nodeScaleMip0 = (1 << (int) pageTableMip0.z) * 0.0625f;
    float2 pageFracMip0 = frac(worldPos.xz / nodeScaleMip0);
#ifndef _STORMVT_LOW_END_ON
    float nodeScaleMip1 = (1 << (int) pageTableMip1.z) * 0.0625f;
    float2 pageFracMip1 = frac(worldPos.xz / nodeScaleMip1);
#endif
    
    //Calculate uv physical texture coordinates
    float2 physicalTextureCoordsMip0 = pageFracMip0 * _StormVT_PageFracScale.xy + pageTableMip0.xy;
#ifndef _STORMVT_LOW_END_ON
    float2 physicalTextureCoordsMip1 = pageFracMip1 * _StormVT_PageFracScale.xy + pageTableMip1.xy;
#endif
    
    //Declare PBR Outputs
    half3 diffuseMip0, normalMip0, diffuseMip1, normalMip1;
    half4 maskMip0, maskMip1;
   
    //Create derivates of both mips
    float2 pageMip0 = (worldPos.xz / nodeScaleMip0)* _StormVT_PageFracScale.xy;
    float4 derivMip0 = float4(ddx(pageMip0), ddy(pageMip0));

#ifndef _STORMVT_LOW_END_ON
    float2 pageMip1 = (worldPos.xz / nodeScaleMip1)* _StormVT_PageFracScale.xy;
    float4 derivMip1 = float4(ddx(pageMip1), ddy(pageMip1));
#endif

#ifndef _STORMVT_LOW_END_ON
    //Compute miplevel blending
    float mipLevel = frac(computeMipLevel(terrainPosUV, -_StormVT_TileMipData.z, _StormVT_TileMipData.x, _StormVT_TileMipData.y, float2(8192, 8192)));
#endif

    //Sample PBR Textures
    SamplePBRTextures(physicalTextureCoordsMip0, pbrSampler, derivMip0, 0, diffuseMip0, normalMip0, maskMip0);
#ifndef _STORMVT_LOW_END_ON
    SamplePBRTextures(physicalTextureCoordsMip1, pbrSampler, derivMip1, 1, diffuseMip1, normalMip1, maskMip1);
#endif
    

    //Create Debug Lines
    half debugLinesMip0 = lerp(1, RectangleMask(pageFracMip0, DEBUG_TILE_SCALE, DEBUG_TILE_SCALE), _StormVT_DebugTiles);
    #ifndef _STORMVT_LOW_END_ON
    half debugLinesMip1 = lerp(1, RectangleMask(pageFracMip1, DEBUG_TILE_SCALE, DEBUG_TILE_SCALE), _StormVT_DebugTiles);
    #endif
    

#ifndef _STORMVT_LOW_END_ON
    //Lerp final outputs
    diffuse = lerp(diffuseMip0 * debugLinesMip0, diffuseMip1 * debugLinesMip1, mipLevel);
    normal = lerp(normalMip0, normalMip1, mipLevel);
    mask = lerp(maskMip0, maskMip1, mipLevel);
#else
    diffuse = diffuseMip0 * debugLinesMip0;
    normal = normalMip0;
    mask = maskMip0;
#endif

#ifndef _STORMVT_LOW_END_ON
    float3 camSub = _WorldSpaceCameraPos.xyz - worldPos;
    float nearDistance = saturate(length(camSub) / _StormVT_DistanceMapData.x);
    float3 finalDiffuse = 0;
    float3 finalNormal;

    SampleColorMaps(physicalTextureCoordsMip0, pbrSampler, diffuse, normal, worldPos, finalDiffuse, finalNormal);


    diffuse = finalDiffuse;
    normal = finalNormal;
#endif

}

float SamplePhysicalTextureHeight(float3 uv, SamplerState ss, float Lod)
{
    #ifdef _BRANCHING_ON
    UNITY_BRANCH
    if(uv.z> 0)
    {
        return SAMPLE_TEXTURE2D_LOD(_StormVT_PhysicalTextureHeight, ss, uv.xy, Lod).r * uv.z;
    }
    else
    {
        return 0;
    }

    #else
    return SAMPLE_TEXTURE2D_LOD(_StormVT_PhysicalTextureHeight, ss, uv.xy, Lod).r * uv.z;
    #endif
    
}

float SamplePhysicalTextureDisplacement(float3 uv, SamplerState ss, float Lod, float nearDistance)
{
    #ifdef _BRANCHING_ON
    UNITY_BRANCH
    if (nearDistance > 0.0)
    {
        return SAMPLE_TEXTURE2D_LOD(_StormVT_PhysicalTextureSource0, ss, uv.xy, Lod).a * uv.z * nearDistance;
    }
    else
    {
        return 0;
    }
    #else
    return SAMPLE_TEXTURE2D_LOD(_StormVT_PhysicalTextureSource0, ss, uv.xy, Lod).a * uv.z * nearDistance;
    #endif
        
}

float SamplePhysicalTextureHoles(float3 uv, SamplerState ss, float Lod)
{
    float Result = 1;
#ifdef _BRANCHING_ON
    UNITY_BRANCH
    if(uv.z> 0)
    {
            Result = SAMPLE_TEXTURE2D_LOD(_StormVT_PhysicalTextureNormal, ss, uv.xy, Lod).a;// * uv.z;
    }
    else
    {
        Result = 0;
    }

#else
    Result = SAMPLE_TEXTURE2D_LOD(_StormVT_PhysicalTextureNormal, ss, uv.xy, Lod).a;// * uv.z;
#endif
    return Result;
}

float3 SamplePageTable(float2 uv, SamplerState ss, float Index, float Lod)
{
    return SAMPLE_TEXTURE2D_ARRAY_LOD(_StormVT_PageTableTex2DArray, ss, uv, Index, Lod).rgb;
}



TEXTURE2D(_SnowAlbedo);
TEXTURE2D(_SnowNormal);
TEXTURE2D(_SnowMask);

float linearDodge(float Base, float Blend, float Opacity)
{
    float Out = Base + Blend;
    return lerp(Base, Out, Opacity);
}

float MyRemap(float value, float from1, float to1, float from2, float to2)
{
    return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
}

float HeightMask(float height1, float height2, float mask)
{
    mask = mask - 0.5f;
    float blend = (height2 + mask) - (height1 + (mask * -1));
    blend = saturate((blend * 4) + 0.5f);
    return blend;
}

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

float3 worldSpaceNormalBlend(float3 terrainNormal, float3 surfaceNormal)
{
    float3 terrainTangent = cross(terrainNormal, float3(0, 0, 1));
    float3 terrainBitangent = cross(terrainTangent, terrainNormal);
    return surfaceNormal.x * terrainTangent + surfaceNormal.y * terrainBitangent + surfaceNormal.z * terrainNormal;
}

void Snow(float3 UVs, float3 worldPosition, SamplerState ss, float3 diffuse, float3 normal, float4 mask, out float3 blendedDiffuse, out float3 blendedNormal, out float4 blendedMask)
{
    float4 worldNormalWithCurvature = SamplePhysicalTextureNormal(UVs, ss, 0, 1);
    float3 worldNormal = worldNormalWithCurvature.xyz;
    float curvature = worldNormalWithCurvature.a;
    
    float topBlend = saturate(pow(worldNormal.y, 1) * 0.5);
    float curvatureRemap = saturate(curvature * 1);
    
    //Curvature influence
    curvature = linearDodge(topBlend, curvatureRemap, _SnowCurvatureInfluence);
    
    //Snow Height
    float snowHeight = smoothstep(-_SnowFalloff, _SnowFalloff, (worldPosition.y - _SnowHeight));
    
    curvature *= snowHeight;
    curvature = saturate(MyRemap(curvature, 0, 1, -1, 3));
    
    float2 worldUV = worldPosition.xz / _SnowUVScale;
    float3 snowAlbedo = SAMPLE_TEXTURE2D(_SnowAlbedo, ss, worldUV).rgb;
    float4 snowMask = SAMPLE_TEXTURE2D(_SnowMask, ss, worldUV);
    float3 snowNormal = unpackNormal(SAMPLE_TEXTURE2D(_SnowNormal, ss, worldUV));
    
    curvature = HeightMask(mask.b, snowMask.b, curvature);
    
    blendedDiffuse = lerp(diffuse, _SnowColor.rgb * snowAlbedo, curvature);
    
    float3 finalWorldSpaceSnowNormal = worldSpaceNormalBlend(worldNormal, snowNormal);
    
    blendedNormal = lerp(normal, finalWorldSpaceSnowNormal, curvature);
    blendedMask = lerp(mask, snowMask, curvature);
 
}







//Vertex only functions


//Functions
void VertPageTableUVs_float(float3 position, out float2 UVs, out float2 volumeUVs, out float ringIndex, out float volumeMask)
{
    //Sample Page UVs
    float2 newUV = 0;
    
    const int numLoops = (int) min(_StormVT_PageTableArrayData.w - (_StormVT_PageTableArrayData.w - 1), MAXARRAYELEMENTS);
    
    const float baseSize = _StormVT_PageTableArrayData.z;
    const float2 snapPos = _StormVT_SnappedCameraPosition.xy;
    const int maxIndex = (int)(_StormVT_PageTableArrayData.w - 1);
    for (int i = 0; i < numLoops; i++)
    {
        const int index = maxIndex - i;
        
        // Use bit shift for power of 2 division - more efficient
        half size = baseSize / (1 << (i + 2));
        float2 centerPos = floor(snapPos / size) * size + size;
        newUV = (position.xz - centerPos) / (size * 4) + 0.5;
        
        // More efficient bounds check using a single operation
        volumeMask = ceil(saturate(1 - max(
            max(newUV.x, 1 - newUV.x),
            max(newUV.y, 1 - newUV.y)
        )));

        UVs  = lerp(UVs, newUV, volumeMask);
        ringIndex = lerp(ringIndex, index, volumeMask);
    }
    volumeUVs = UVs;
    
}

//Functions
void DisplacementPageTableUVs_float(float3 position, out float2 UVs, out float ringIndex, out float volumeMask)
{
    //Sample Page UVs
    float2 newUV = 0;
    
    //Calculate initial derivates, otherwise we get seams between page table sizes
    float2 initialUV = (position.xz - _StormVT_SnappedCameraPosition.xy) / _StormVT_PageTableArrayData.z;
    
    const int numLoops = (int) min(_StormVT_PageTableArrayData.w - _StormVT_PageTableArrayData.y, MAXARRAYELEMENTS);
    
    const float baseSize = _StormVT_PageTableArrayData.z;
    const float2 snapPos = _StormVT_SnappedCameraPosition.xy;
    const int maxIndex = (int)(_StormVT_PageTableArrayData.w - 1);
    for (int i = 0; i < numLoops; i++)
    {
        const int index = maxIndex - i;
        
        // Use bit shift for power of 2 division - more efficient
        half size = baseSize / (1 << (i + 2));
        float2 centerPos = floor(snapPos / size) * size + size;
        newUV = (position.xz - centerPos) / (size * 4) + 0.5;
        
        // More efficient bounds check using a single operation
        half mask = ceil(saturate(1 - max(
            max(newUV.x, 1 - newUV.x),
            max(newUV.y, 1 - newUV.y)
        )));

        if(i == 0)
        {
            volumeMask = mask;
        }
        UVs = lerp(UVs, newUV, mask);
        ringIndex = lerp(ringIndex, index, mask);
    }
    
}

void VertVirtualTextureUVs_float(float3 position, float2 uv, SamplerState ss, float Index, float Lod, float volumeMask, out float3 physicalTextureCoords, out float2 fracUVs, out float zoom)
{
    UNITY_BRANCH
    if(volumeMask > 0.0)
    {
        float4 physPage = SAMPLE_TEXTURE2D_ARRAY_LOD(_StormVT_PageTableTex2DArray, ss, uv, Index, Lod) * _StormVT_TexScale + _StormVT_TexBias;

        float nodeScale =  (1 << (int)physPage.z) * 0.0625f;
    
        float2 pageFrac = frac(position.xz / nodeScale ); 
    
    
        physicalTextureCoords = float3(pageFrac * _StormVT_PageFracScale.xy + physPage.xy, ((physPage.z == 0) ? 0 : 1) * volumeMask);
        fracUVs = pageFrac;

        zoom = 1;
    }
    else
    {
        physicalTextureCoords = float3(0,0,volumeMask);
        fracUVs = 0;
        zoom = 0;
    }
   
}


void VertSampleVirtualUVs(float3 worldPos, SamplerState ss, out float3 UVs, out float2 fracUVs, out float2 volumeUVs, out float zoomShift)
{
    float2 pageUVs;
    float ringIndex;
    float volumeMask;

    
    VertPageTableUVs_float(worldPos, pageUVs, volumeUVs, ringIndex, volumeMask);

    float3 mip0PhysicalTextureCoords;
    float2 mip0FracUVs;
    float mip0Zoom;

    VertVirtualTextureUVs_float(worldPos, pageUVs, ss, ringIndex, 0, volumeMask, mip0PhysicalTextureCoords, mip0FracUVs, mip0Zoom);

    UVs = mip0PhysicalTextureCoords;
    fracUVs = mip0FracUVs;
    zoomShift = mip0Zoom;
}

void DisplacementSampleVirtualUVs(float3 worldPos, SamplerState ss, out float3 UVs, out float2 fracUVs, out float zoomShift)
{
    float2 pageUVs;
    float ringIndex;
    float volumeMask;

    
    DisplacementPageTableUVs_float(worldPos, pageUVs, ringIndex, volumeMask);

    float3 mip0PhysicalTextureCoords;
    float2 mip0FracUVs;
    float mip0Zoom;

    VertVirtualTextureUVs_float(worldPos, pageUVs, ss, ringIndex, 0, volumeMask, mip0PhysicalTextureCoords, mip0FracUVs, mip0Zoom);

    UVs = mip0PhysicalTextureCoords;
    fracUVs = mip0FracUVs;
    zoomShift = mip0Zoom;
}

//Functions
void SnowPageTableUVs_float(float3 position, out float2 UVs, out float2 volumeUVs, out float ringIndex, out float volumeMask)
{
    

    //Sample Page UVs
    float2 newUV = 0;

    const int numLoops = (int) min(_StormVT_PageTableArrayData.w, MAXARRAYELEMENTS);
    
    const float baseSize = _StormVT_PageTableArrayData.z;
    const float2 snapPos = _StormVT_SnappedCameraPosition.xy;
    const int maxIndex = (int)(_StormVT_PageTableArrayData.w - 1);
    for (int i = 0; i < numLoops; i++)
    {
        const int index = maxIndex - i;
        
        // Use bit shift for power of 2 division - more efficient
        half size = baseSize / (1 << (i + 2));
        float2 centerPos = floor(snapPos / size) * size + size;
        newUV = (position.xz - centerPos) / (size * 4) + 0.5;
        
        half mask = 1;

        mask = ceil(saturate(1 - max(
                    max(newUV.x, 1 - newUV.x),
                    max(newUV.y, 1 - newUV.y)
                )));


        UVs = lerp(UVs, newUV, mask);
        ringIndex = lerp(ringIndex, index, mask);
    }
    volumeUVs = 1;
    volumeMask = 1;

    
}

void SnowSampleVirtualUVs(float3 worldPos, SamplerState ss, out float3 UVs, out float2 fracUVs, out float2 volumeUVs, out float zoomShift)
{
    float2 pageUVs;
    float ringIndex;
    float volumeMask;

    
    SnowPageTableUVs_float(worldPos, pageUVs, volumeUVs, ringIndex, volumeMask);

    float3 mip0PhysicalTextureCoords;
    float2 mip0FracUVs;
    float mip0Zoom;

    VertVirtualTextureUVs_float(worldPos, pageUVs, ss, ringIndex, 0, volumeMask, mip0PhysicalTextureCoords, mip0FracUVs, mip0Zoom);

    UVs = mip0PhysicalTextureCoords;
    fracUVs = mip0FracUVs;
    zoomShift = mip0Zoom;
}




#endif