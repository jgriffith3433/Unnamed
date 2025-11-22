#ifndef _STORM_VT_FUNCTIONS
#define _STORM_VT_FUNCTIONS

//Scripting Defines
//#define MAXITERATIONS 100
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
float4 _StormVT_TileMipData;
int _StormVT_Mode;


#define SamplerState_Point_Repeat sampler_point_repeat
SamplerState SamplerState_Point_Repeat;

#define SamplerState_Linear_Repeat sampler_linear_repeat
SamplerState SamplerState_Linear_Repeat;





void GetFloatingPointOffset_float(out float3 offset)
{
    offset = _StormVT_WorldOffset.xyz;
}

void GetBoolInitializedTextures_float(out bool initializedMask)
{
    initializedMask = StormVT_PhysicalTexturesInitialized;
}

void GetTotalTerrainBoundsMask_float(float3 position, out float mask)
{
    if (_StormVT_Mode == 2)
    {
        mask = 1;
    }
    else
    {
       float2 uv = (position.xz - _StormVT_TotalTerrainBounds.xy) / _StormVT_TotalTerrainBounds.zw + 0.5;
    
        mask = 1;
        mask = ceil(saturate(1 - max(
                max(uv.x, 1 - uv.x),
                max(uv.y, 1 - uv.y)
            )));
    }
    

}

float GetTotalTerrainBoundsMask(float3 position)
{
    float mask = 1;
    
    if (_StormVT_Mode != 2)
    {
        float2 uv = (position.xz - _StormVT_TotalTerrainBounds.xy) / _StormVT_TotalTerrainBounds.zw + 0.5;
    
        mask = 1;
        mask = ceil(saturate(1 - max(
                max(uv.x, 1 - uv.x),
                max(uv.y, 1 - uv.y)
            )));
    }

    return mask;
}


#define MAXARRAYELEMENTS 16
uniform float4 _StormVT_PageTableDataArray[MAXARRAYELEMENTS];

//Physical Texture Defines
const float4 _StormVT_PageFracScale;
const float4 _StormVT_TexScale;
const float4 _StormVT_TexBias;
TEXTURE2D(_PageTable);
TEXTURE2D(_PhysicalTextureHeight);

TEXTURE2D(_StormVT_PhysicalTextureHeight);
TEXTURE2D(_StormVT_PhysicalTextureNormal);
TEXTURE2D(_StormVT_PhysicalTextureSource0);
TEXTURE2D(_StormVT_PhysicalTextureSource1);
TEXTURE2D(_StormVT_PhysicalTextureSatellite);
TEXTURE2D(_StormVT_PhysicalTextureHoles);

void GetTerrainHeightScale_float(out float height, out float offset)
{
    height = _StormVT_TerrainHeightScale.r;
    offset = _StormVT_TerrainHeightScale.g;
}

void GetTerrainDisplacementAmount_float(out float displacementAmount)
{
    displacementAmount = _StormVT_TerrainHeightScale.z;
}

void GetDebugTiles_float(out bool debugTiles)
{
    debugTiles = _StormVT_DebugTiles == 1 ? true : false;
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

void CreateMipBias_float(float2 uv, float scale, float bias, out float mipLevel)
{
    float2 ddx_ = ddx(uv) * scale;
    float2 ddy_ = ddy(uv) * scale;
    float d = max(dot(ddx_, ddx_), dot(ddy_, ddy_));
    mipLevel = max(0.5 * log2(d) - bias, 0.0);
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


void VirtualTextureUVs_float(float3 position, float2 uv, UnitySamplerState ss, float Index, float Lod, float volumeMask, out float3 physicalTextureCoords, out float2 fracUVs, out float2 rawUVs, out float zoom)
{
#ifdef _BRANCHING_ON
    UNITY_BRANCH
    if(volumeMask > 0.0)
    {
        float4 physPage = SAMPLE_TEXTURE2D_ARRAY_LOD(_StormVT_PageTableTex2DArray, ss.samplerstate, uv, Index, Lod) * _StormVT_TexScale + _StormVT_TexBias;
        float nodeScale =  (1 << (int)physPage.z) * 0.0625f;
    
        float2 pageFrac = frac(position.xz / nodeScale ); 
    
        physicalTextureCoords = float3(pageFrac * _StormVT_PageFracScale + physPage.xy, ((physPage.z == 0) ? 0 : 1) * volumeMask);
        fracUVs = pageFrac;
        rawUVs = position.xz / nodeScale;
        zoom = 1;
    }
    else
    {
        physicalTextureCoords = float3(0,0,volumeMask);
        fracUVs = 0;
        rawUVs = 0;
        zoom = 0;
    }
#else
    float4 physPage = SAMPLE_TEXTURE2D_ARRAY_LOD(_StormVT_PageTableTex2DArray, ss.samplerstate, uv, Index, Lod) * _StormVT_TexScale + _StormVT_TexBias;
    float nodeScale =  (1 << (int)physPage.z) * 0.0625f;
    
    float2 pageFrac = frac(position.xz / nodeScale ); 
    
    
    physicalTextureCoords = float3(pageFrac * _StormVT_PageFracScale.xy + physPage.xy, ((physPage.z == 0) ? 0 : 1) * volumeMask);
    fracUVs = pageFrac;
    rawUVs = position.xz / nodeScale;
    zoom = 1;
#endif
   
}


void VirtualTextureUVs(float3 position, float2 uv, SamplerState ss, float Index, float Lod, float volumeMask, out float3 physicalTextureCoords, out float2 fracUVs, out float2 rawUVs, out float zoom)
{
    #ifdef _BRANCHING_ON
    UNITY_BRANCH
    if(volumeMask > 0.0)
    {
        float4 physPage = SAMPLE_TEXTURE2D_ARRAY_LOD(_StormVT_PageTableTex2DArray, ss, uv, Index, Lod) * _StormVT_TexScale + _StormVT_TexBias;


        float nodeScale =  (1 << (int)physPage.z) * 0.0625f;
    
        float2 pageFrac = frac(position.xz / nodeScale ); 
    
    
        physicalTextureCoords = float3(pageFrac * _StormVT_PageFracScale + physPage.xy, ((physPage.z == 0) ? 0 : 1) * volumeMask);
        fracUVs = pageFrac;
        rawUVs = position.xz / nodeScale;
        zoom = 1;
    }
    else
    {
        physicalTextureCoords = float3(0,0,volumeMask);
        fracUVs = 0;
        rawUVs = 0;
        zoom = 0;
    }
    #else
    float4 physPage = SAMPLE_TEXTURE2D_ARRAY_LOD(_StormVT_PageTableTex2DArray, ss, uv, Index, Lod) * _StormVT_TexScale + _StormVT_TexBias;


    float nodeScale =  (1 << (int)physPage.z) * 0.0625f;
    
    float2 pageFrac = frac(position.xz / nodeScale ); 
    
    
    physicalTextureCoords = float3(pageFrac * _StormVT_PageFracScale.xy + physPage.xy, ((physPage.z == 0) ? 0 : 1) * volumeMask);
    fracUVs = pageFrac;
    rawUVs = position.xz / nodeScale;
    zoom = 1;
    #endif
   
}

void SamplePageTable_float(float2 uv, UnitySamplerState ss, float Index, float Lod, out float4 Result)
{
    Result = SAMPLE_TEXTURE2D_ARRAY_LOD(_StormVT_PageTableTex2DArray, ss.samplerstate, uv, Index, Lod);
}

void SamplePageTableGrad_float(float2 uv, UnitySamplerState ss, float Index, float2 dpdx, float2 dpdy, out float4 Result)
{
    Result = SAMPLE_TEXTURE2D_ARRAY_GRAD(_StormVT_PageTableTex2DArray, ss.samplerstate, uv, Index, dpdx, dpdy);
}

void SamplePhysicalTextureHeight_float(float3 uv, UnitySamplerState ss, float Lod, out float Result)
{
#ifdef _BRANCHING_ON
    UNITY_BRANCH
    if(uv.z> 0)
    {
            Result = SAMPLE_TEXTURE2D_LOD(_StormVT_PhysicalTextureHeight, ss.samplerstate, uv.xy, Lod).r * uv.z;
    }
    else
    {
        Result = 0;
    }

#else
    Result = SAMPLE_TEXTURE2D_LOD(_StormVT_PhysicalTextureHeight, ss.samplerstate, uv.xy, Lod).r * uv.z;
#endif
    
}

void SamplePhysicalTextureNormal_float(float3 uv, UnitySamplerState ss, float Lod, out float4 Result)
{
    Result = SAMPLE_TEXTURE2D_LOD(_StormVT_PhysicalTextureNormal, ss.samplerstate, uv.xy, Lod);
    Result = lerp(float4(0.5,1,0.5,1), Result, uv.z);
}

void SamplePhysicalTextureHoles_float(float3 uv, UnitySamplerState ss, float Lod, out float Result)
{
#ifdef _BRANCHING_ON
    UNITY_BRANCH
    if(uv.z> 0)
    {
            Result = SAMPLE_TEXTURE2D_LOD(_StormVT_PhysicalTextureHoles, ss.samplerstate, uv.xy, Lod).r * uv.z;
    }
    else
    {
        Result = 0;
    }

#else
    Result = SAMPLE_TEXTURE2D_LOD(_StormVT_PhysicalTextureHoles, ss.samplerstate, uv.xy, Lod).r * uv.z;
#endif
    
}

void SamplePhysicalTextureSource0_float(float3 uv, UnitySamplerState ss, float Lod, out float4 Result)
{
    Result = SAMPLE_TEXTURE2D_LOD(_StormVT_PhysicalTextureSource0, ss.samplerstate, uv.xy, Lod) * uv.z;
}

void SamplePhysicalTextureSource1_float(float3 uv, UnitySamplerState ss, float Lod, out float4 Result)
{
    Result = SAMPLE_TEXTURE2D_LOD(_StormVT_PhysicalTextureSource1, ss.samplerstate, uv.xy, Lod) * uv.z;
}

void SamplePhysicalTextureDisplacement_float(float3 uv, UnitySamplerState ss, float Lod, float nearDistance, out float Displacement)
{
#ifdef _BRANCHING_ON
    UNITY_BRANCH
    if (nearDistance > 0.0)
    {
        Displacement = SAMPLE_TEXTURE2D_LOD(_StormVT_PhysicalTextureSource0, ss.samplerstate, uv.xy, Lod).a * uv.z * nearDistance;
    }
    else
    {
        Displacement = 0;
    }
#else
    Displacement = SAMPLE_TEXTURE2D_LOD(_StormVT_PhysicalTextureSource0, ss.samplerstate, uv.xy, Lod).a * uv.z * nearDistance;
#endif
        
}

float SamplePhysicalTextureDisplacement(float3 uv, SamplerState ss, float Lod)
{
    return SAMPLE_TEXTURE2D_LOD(_StormVT_PhysicalTextureSource0, ss, uv.xy, Lod).a * uv.z;
}


void SamplePhysicalTextureSatellite_float(float3 uv, UnitySamplerState ss, float Lod, out float4 Result)
{
    Result = SAMPLE_TEXTURE2D_LOD(_StormVT_PhysicalTextureSatellite, ss.samplerstate, uv.xy, Lod) * uv.z;
}


void SampleVirtualUVs(float3 worldPos, SamplerState ss, out float3 UVs, out float2 fracUVs, out float2 rawUVs, out float zoomShift, out float3 UVsMip1)
{
    float2 pageUVs;
    float ringIndex;
    float volumeMask;

    
    PageTableUVs_float(worldPos, pageUVs, ringIndex, volumeMask);

    float3 mip0PhysicalTextureCoords;
    float2 mip0FracUVs;
    float2 mip0RawUVs;
    float mip0Zoom;

    VirtualTextureUVs(worldPos, pageUVs, ss, ringIndex, 0, volumeMask, mip0PhysicalTextureCoords, mip0FracUVs, mip0RawUVs, mip0Zoom);

    UVs = mip0PhysicalTextureCoords;
    fracUVs = mip0FracUVs;
    rawUVs = mip0RawUVs;
    zoomShift = mip0Zoom;
    UVsMip1 = UVs;
}

float4 SamplePhysicalTextureSource0(float3 uv, SamplerState ss, float Lod)
{
    return SAMPLE_TEXTURE2D_LOD(_StormVT_PhysicalTextureSource0, ss, uv.xy, Lod);// * uv.z;
}

float4 SamplePhysicalTextureSource1(float3 uv, SamplerState ss, float Lod)
{
    return SAMPLE_TEXTURE2D_LOD(_StormVT_PhysicalTextureSource1, ss, uv.xy, Lod);// * uv.z;
}

void SamplePBR(float3 uv, int lod, out float3 diffuse, out float3 normal, out float4 mask)
{
    float4 source0 = SamplePhysicalTextureSource0(uv, sampler_linear_repeat, 0);
    float4 source1 = SamplePhysicalTextureSource1(uv, sampler_linear_repeat, 0);
    
    diffuse = source0.rgb;
    mask = float4(0, source1.b, source0.a, source1.a);
    normal.xy = source1.xy * 2 - 1;
    //normal.y = normal.y * -1;
    normal.z = sqrt(1 - saturate(dot(normal.xy, normal.xy)));
    normal = normal.xzy;
}

void SampleMaterialPBR(float3 uvMip0, float3 uvMip1, float2 rawUVs, float mipBias, SamplerState ss, out float3 diffuse, out float3 normal, out float4 mask)
{
    diffuse = 0;
    normal = 0;
    mask = 0;

    float3 diffuseMip0;
    float3 normalMip0;
    float4 maskMip0;

    float3 diffuseMip1;
    float3 normalMip1;
    float4 maskMip1;

    SamplePBR(uvMip0, 0, diffuseMip0, normalMip0, maskMip0);
    SamplePBR(uvMip0, 1, diffuseMip1, normalMip1, maskMip1);

    float mipBlend = 0;

#if !defined(SHADER_STAGE_RAY_TRACING)

    const float2 _ddx = ddx(uvMip0.xy);
    const float2 _ddy = ddy(uvMip0.xy);
    mipBlend = saturate(max(log2(max(dot(_ddx, _ddx), dot(_ddy, _ddy))) * 0.5 + _StormVT_PageFracScale.z, 0));

#endif
    
    diffuse = lerp(diffuseMip0, diffuseMip1, mipBlend);
    normal = lerp(normalMip0, normalMip1, mipBlend);
    mask = lerp(maskMip0, maskMip1, mipBlend);
    
    //diffuse = diffuseMip0;
    //normal = normalMip0;
    //mask = maskMip0;
}



//Vertex only functions


//Functions
void VertPageTableUVs_float(float3 position, out float2 UVs, out float ringIndex, out float volumeMask)
{
    
    //Sample Page UVs
    float2 newUV = 0;
    
    //const int numLoops = (int) min(_StormVT_PageTableArrayData.w - _StormVT_PageTableArrayData.y, MAXARRAYELEMENTS);
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
}


void VertVirtualTextureUVs_float(float3 position, float2 uv, SamplerState ss, float Index, float Lod, float volumeMask, out float3 physicalTextureCoords, out float2 fracUVs, out float2 rawUVs, out float zoom)
{
    UNITY_BRANCH

    if (volumeMask > 0.0)
    {
        float4 physPage = SAMPLE_TEXTURE2D_ARRAY_LOD(_StormVT_PageTableTex2DArray, ss, uv, Index, Lod) * _StormVT_TexScale + _StormVT_TexBias;

        float nodeScale = (1 << (int) physPage.z) * 0.0625f;
    
        float2 pageFrac = frac(position.xz / nodeScale);
    
    
        physicalTextureCoords = float3(pageFrac * _StormVT_PageFracScale.xy + physPage.xy, ((physPage.z == 0) ? 0 : 1) * volumeMask);
        fracUVs = pageFrac;
        rawUVs = position.xz / nodeScale;
        zoom = 1;
    }
    else
    {
        physicalTextureCoords = float3(0, 0, volumeMask);
        fracUVs = 0;
        rawUVs = 0;
        zoom = 0;
    }
   
}


void VertSampleVirtualUVs(float3 worldPos, SamplerState ss, out float3 UVs, out float2 fracUVs, out float2 rawUVs, out float zoomShift, out float3 UVsMip1)
{
    float2 pageUVs;
    float ringIndex;
    float volumeMask;

    
    VertPageTableUVs_float(worldPos, pageUVs, ringIndex, volumeMask);

    float3 mip0PhysicalTextureCoords;
    float2 mip0FracUVs;
    float2 mip0RawUVs;
    float mip0Zoom;

    VertVirtualTextureUVs_float(worldPos, pageUVs, ss, ringIndex, 0, volumeMask, mip0PhysicalTextureCoords, mip0FracUVs, mip0RawUVs, mip0Zoom);

    UVs = mip0PhysicalTextureCoords;
    fracUVs = mip0FracUVs;
    rawUVs = mip0RawUVs;
    zoomShift = mip0Zoom;
    UVsMip1 = UVs;

    
    
}








void PBRBlend_float( in float3 worldPosition, in float3 triPlanarWorldPos, in float mipBias, in float3 diffuse, in float3 normal, in float4 mask, in float blendBias, in float blendDistance, in float distance, out float3 o_diffuse, out float3 o_normal, out float4 o_mask)
{
    o_diffuse = diffuse;
    o_normal = normal;
    o_mask = mask;

    UNITY_BRANCH
    if(distance > 0.0)
    {
        
        float3 UVs;
        float2 fracUVs;
        float2 volumeUVs;
        float2 rawUVs;
        float zoomShift;
        float3 UVsMip1;
        
        VertSampleVirtualUVs(worldPosition, sampler_point_repeat, UVs, fracUVs, rawUVs, zoomShift, UVsMip1);
        
        float3 d_UVs;
        float2 d_fracUVs;
        float2 d_volumeUVs;
        float2 d_rawUVs;
        float d_zoomShift;
        float3 d_UVsMip1;
        SampleVirtualUVs(worldPosition, sampler_point_repeat, d_UVs, d_fracUVs, d_rawUVs, d_zoomShift, d_UVsMip1);
     

        
        //Get heightmap
        float terrainHeight = (SAMPLE_TEXTURE2D_LOD(_StormVT_PhysicalTextureHeight, sampler_linear_repeat, UVs.xy, 0).r * _StormVT_TerrainHeightScale.r) + _StormVT_TerrainHeightScale.g;
        float terrainDisplacement = SAMPLE_TEXTURE2D_LOD(_StormVT_PhysicalTextureSource0, sampler_linear_repeat, d_UVs.xy, 0).a;
        //float terrainDisplacement = SamplePhysicalTextureDisplacement(UVs, sampler_linear_repeat, 0);
        terrainDisplacement = lerp(-_StormVT_TerrainHeightScale.z, _StormVT_TerrainHeightScale.z, terrainDisplacement);
        terrainHeight += terrainDisplacement;
        
        //Create blend mask
        float blendMask = worldPosition.y - terrainHeight;
        blendMask = saturate((blendMask - blendBias) / blendDistance);

        blendMask = StormVT_PhysicalTexturesInitialized == 1 ? blendMask : 1;
        
        
        
        //Sample color
        SampleVirtualUVs(triPlanarWorldPos, sampler_point_repeat, UVs, fracUVs, rawUVs, zoomShift, UVsMip1);
        float3 blendedDiffuse, blendedNormal;
        float4 blendedMask;

        SampleMaterialPBR(UVs, UVsMip1, rawUVs, mipBias, sampler_linear_repeat, blendedDiffuse, blendedNormal, blendedMask);
        
        
        
        o_diffuse = lerp(blendedDiffuse, diffuse, blendMask);
        o_normal = lerp(blendedNormal, normal, blendMask);
        o_mask = lerp(blendedMask, mask, blendMask);

        o_diffuse = lerp(o_diffuse,diffuse, 1-distance);
        o_normal = lerp(o_normal, normal, 1-distance);
        o_mask = lerp(o_mask, mask, 1-distance);
    }
    
}

void SkirtBlend_float(in SamplerState ss, in float3 worldVertex, in float3 objectVertex, in float3 pivotPosition, in float blendAmount, in float distance, out float3 skirtPosition)
{

    skirtPosition = objectVertex;
    
    UNITY_BRANCH
    if(distance > 0.0)
    {
        
        float3 worldPos = worldVertex + _StormVT_WorldOffset.xyz;

        float2 pageUVs, volumeUVs;
        float ringIndex;
        float volumeMask;

    
        PageTableUVs_float(worldPos, pageUVs, ringIndex, volumeMask);

        float3 mip0PhysicalTextureCoords;
        float2 mip0FracUVs;
        float2 mip0RawUVs;
        float mip0Zoom;

        VirtualTextureUVs(worldPos, pageUVs, ss, ringIndex, 0, volumeMask, mip0PhysicalTextureCoords, mip0FracUVs, mip0RawUVs, mip0Zoom);

        //Get heightmap
        float terrainHeight = (SAMPLE_TEXTURE2D_LOD(_StormVT_PhysicalTextureHeight, ss, mip0PhysicalTextureCoords.xy, 0).r * _StormVT_TerrainHeightScale.r) + _StormVT_TerrainHeightScale.g;
        
        //Create pivot displacement difference
        float pivotDifference = terrainHeight - pivotPosition.y;
        float3 worldPosVertex = worldVertex + float3(0, pivotDifference, 0);
        //Transform worldVertex pos to object position
        float3 objectPosVertex = mul((float3x3)UNITY_MATRIX_I_M, worldPosVertex);

        skirtPosition = lerp(objectPosVertex, objectVertex, blendAmount);
        skirtPosition = StormVT_PhysicalTexturesInitialized == 1 ? skirtPosition : objectVertex;
        
    }


}


void GetZoomLevelData_float(out float minZoomLevel, out float maxZoomLevel, out float numZoomLevels)
{
    minZoomLevel = _StormVT_ZoomLevelData.x;
    maxZoomLevel = _StormVT_ZoomLevelData.y;
    numZoomLevels = _StormVT_ZoomLevelData.z;
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

void mipDebug_float(float2 rawUV, float2 uv, float lodBias, float minMipLevel, float maxMipLevel, float2 physicalTextureSize, float tileSize, out float4 result, out float3 returnUV)
{
    //float mip_level = computeMipLevel(uv, lodBias, minMipLevel, maxMipLevel, physicalTextureSize);
    float mip_level = computeMipLevel(rawUV, -_StormVT_TileMipData.z, _StormVT_TileMipData.x, _StormVT_TileMipData.y, physicalTextureSize);
    float2 page_coords = floor(uv * tileSize / exp2(mip_level));

     // Convert to [0,255] RGBA color and return:
    float4 page_id = float4(page_coords, mip_level + 1.0, 1.0);
    //result = page_id / 65535.0;
    result = page_id / 255.0;

    returnUV = float3((uv * tileSize / exp2(mip_level)), mip_level);
}

void CreateMipFeedback_float(float2 rawUV, float2 uv, float lodBias, float minMipLevel, float maxMipLevel, float2 physicalTextureSize, float tileSize, out float4 result, out float3 returnUV, out float mipLevel)
{
    //float mip_level = computeMipLevel(rawUV, lodBias, minMipLevel, maxMipLevel, physicalTextureSize);
    float mip_level = computeMipLevel(rawUV, -_StormVT_TileMipData.z, _StormVT_TileMipData.x, _StormVT_TileMipData.y, physicalTextureSize);
    float2 rawPageUVs = (uv * tileSize);
    if(mip_level < 8)
    {
        rawPageUVs = fmod(rawPageUVs, 256.0);
    }

    float2 page_coords = floor(rawPageUVs / exp2(mip_level));

     // Convert to [0,255] RGBA color and return:
    float4 page_id = float4(page_coords, mip_level + 1.0, 1.0);
    result = page_id / 255.0;

    returnUV = float3((uv * tileSize / exp2(mip_level)), mip_level);
    mipLevel = mip_level;
}

float Dither(float4 In, float4 ScreenPosition)
{
    float2 uv = ScreenPosition.xy * _ScreenParams.xy;
    float DITHER_THRESHOLDS[16] =
    {
        1.0 / 17.0, 9.0 / 17.0, 3.0 / 17.0, 11.0 / 17.0,
        13.0 / 17.0, 5.0 / 17.0, 15.0 / 17.0, 7.0 / 17.0,
        4.0 / 17.0, 12.0 / 17.0, 2.0 / 17.0, 10.0 / 17.0,
        16.0 / 17.0, 8.0 / 17.0, 14.0 / 17.0, 6.0 / 17.0
    };
    uint index = (uint(uv.x) % 4) * 4 + uint(uv.y) % 4;
    return (In - DITHER_THRESHOLDS[index]).r;
}

void TriPlanarWorldVTSample_float(float3 worldPos, float3 worldNormal, SamplerState ss, float blendDistance, float dither, out float3 editedWorldPosition)
{
    UNITY_BRANCH
    if (blendDistance > 0.0)
    {
        float3 UVs;
        float2 fracUVs;
        float2 volumeUVs;
        float2 rawUVs;
        float zoomShift;
        float3 UVsMip1;
        
        VertSampleVirtualUVs(worldPos, sampler_point_repeat, UVs, fracUVs, rawUVs, zoomShift, UVsMip1);
        
        float3 d_UVs;
        float2 d_fracUVs;
        float2 d_volumeUVs;
        float2 d_rawUVs;
        float d_zoomShift;
        float3 d_UVsMip1;
        SampleVirtualUVs(worldPos, sampler_point_repeat, d_UVs, d_fracUVs, d_rawUVs, d_zoomShift, d_UVsMip1);
       
        
        //Get heightmap
        float terrainHeight = (SAMPLE_TEXTURE2D_LOD(_StormVT_PhysicalTextureHeight, sampler_linear_repeat, UVs.xy, 0).r * _StormVT_TerrainHeightScale.r) + _StormVT_TerrainHeightScale.g;
        float terrainDisplacement = SAMPLE_TEXTURE2D_LOD(_StormVT_PhysicalTextureSource0, sampler_linear_repeat, d_UVs.xy, 0).a;
        terrainDisplacement = lerp(-_StormVT_TerrainHeightScale.z, _StormVT_TerrainHeightScale.z, terrainDisplacement);
        terrainHeight += terrainDisplacement;
        
        
        float heightDifference = worldPos.y - terrainHeight;
        
        //X Coord
        float2 xCoordPlane = float2(1, 0) * heightDifference * sign(dot(worldNormal, float3(-1, 0, 0)));
        float3 xCoord = worldPos + (float3(xCoordPlane.x, 0, xCoordPlane.y));
        
        //Z Coord
        float2 zCoordPlane = float2(0, 1) * heightDifference * sign(dot(worldNormal, float3(0, 0, -1)));
        float3 zCoord = worldPos + (float3(zCoordPlane.x, 0, zCoordPlane.y));
        
        //Triplanar Blend Weight
        float3 triplanarBlendWeight = pow(abs(worldNormal), 10);
        triplanarBlendWeight = saturate(triplanarBlendWeight) / dot(triplanarBlendWeight, 1);
        
        float xAxisSmoothStep = smoothstep(2, -1, triplanarBlendWeight.x);
        float xAxis = ((1 - xAxisSmoothStep) * xAxisSmoothStep) * dither;
        xAxis = round(saturate(xAxis + xAxisSmoothStep));
        
        float zAxisSmoothStep = smoothstep(-1, 2, triplanarBlendWeight.z);
        float zAxis = ((1 - zAxisSmoothStep) * zAxisSmoothStep) * dither;
        zAxis = round(saturate(zAxis + zAxisSmoothStep));
        
        //final output
        editedWorldPosition = lerp(xCoord, worldPos, xAxis);
        editedWorldPosition = lerp(editedWorldPosition, zCoord, zAxis);

    }
    else
    {
        editedWorldPosition = worldPos;
    }
    
    
}

#endif