#ifndef PW_DETAILINDIRECT_INCLUDED
#define PW_DETAILINDIRECT_INCLUDED

//Indirect Instancing Struct Data
struct DetailData {
    float4 positionScale;
};

//Indirect Instancing Setup
#ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
#if defined(SHADER_API_GLCORE) || defined(SHADER_API_D3D11) || defined(SHADER_API_GLES3) || defined(SHADER_API_METAL) || defined(SHADER_API_VULKAN) || defined(SHADER_API_PSSL) || defined(SHADER_API_XBOXONE)
StructuredBuffer<DetailData> detailBuffer;
StructuredBuffer<DetailData> shadowBuffer;
#endif	
#endif

int _StormVT_ClipmapTerrainState;

//Define Global Variables
float4 _ClipmapCamPos;
float4 _ClipmapData0;
float4 _ClipmapData1;

//Instancing Variables
float ringIndex;
float rotatedDirection;
float shapeIndex;

//source: https://github.com/glslify/glsl-inverse
//License: 2014 Mikola Lysenko. MIT License
float4x4 StormInverse(float4x4 m) {
    float
        a00 = m[0][0], a01 = m[0][1], a02 = m[0][2], a03 = m[0][3],
        a10 = m[1][0], a11 = m[1][1], a12 = m[1][2], a13 = m[1][3],
        a20 = m[2][0], a21 = m[2][1], a22 = m[2][2], a23 = m[2][3],
        a30 = m[3][0], a31 = m[3][1], a32 = m[3][2], a33 = m[3][3],

        b00 = a00 * a11 - a01 * a10,
        b01 = a00 * a12 - a02 * a10,
        b02 = a00 * a13 - a03 * a10,
        b03 = a01 * a12 - a02 * a11,
        b04 = a01 * a13 - a03 * a11,
        b05 = a02 * a13 - a03 * a12,
        b06 = a20 * a31 - a21 * a30,
        b07 = a20 * a32 - a22 * a30,
        b08 = a20 * a33 - a23 * a30,
        b09 = a21 * a32 - a22 * a31,
        b10 = a21 * a33 - a23 * a31,
        b11 = a22 * a33 - a23 * a32,

        det = b00 * b11 - b01 * b10 + b02 * b09 + b03 * b08 - b04 * b07 + b05 * b06;

    return float4x4(
        a11 * b11 - a12 * b10 + a13 * b09,
        a02 * b10 - a01 * b11 - a03 * b09,
        a31 * b05 - a32 * b04 + a33 * b03,
        a22 * b04 - a21 * b05 - a23 * b03,
        a12 * b08 - a10 * b11 - a13 * b07,
        a00 * b11 - a02 * b08 + a03 * b07,
        a32 * b02 - a30 * b05 - a33 * b01,
        a20 * b05 - a22 * b02 + a23 * b01,
        a10 * b10 - a11 * b08 + a13 * b06,
        a01 * b08 - a00 * b10 - a03 * b06,
        a30 * b04 - a31 * b02 + a33 * b00,
        a21 * b02 - a20 * b04 - a23 * b00,
        a11 * b07 - a10 * b09 - a12 * b06,
        a00 * b09 - a01 * b07 + a02 * b06,
        a31 * b01 - a30 * b03 - a32 * b00,
        a20 * b03 - a21 * b01 + a22 * b00) / det;
}

float4x4 BuildObjectToWorldMatrix(DetailData data) {
    // Start with identity
     float4x4 m = {
        1, 0, 0, 0,
        0, 1, 0, 0,
        0, 0, 1, 0,
        0, 0, 0, 1
    };

    // Apply uniform scale
    m[0][0] = data.positionScale.w;
    m[1][1] = data.positionScale.w;
    m[2][2] = data.positionScale.w;
    
    // Apply Y rotation
    float s = sin(data.positionScale.z);
    float c = cos(data.positionScale.z);
    
    m[0][0] = c * data.positionScale.w;
    m[0][2] = s * data.positionScale.w;
    m[2][0] = -s * data.positionScale.w;
    m[2][2] = c * data.positionScale.w;
    
    // Apply translation
    m[0][3] = data.positionScale.x;
    m[1][3] = 0;
    m[2][3] = data.positionScale.y;
    
    return m;
}

//Instancing Setup Functions
void setup()
{

#undef unity_ObjectToWorld 
#undef unity_WorldToObject 

#define unity_ObjectToWorld unity_ObjectToWorld
#define unity_WorldToObject unity_WorldToObject

#ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED


#ifdef _SHADOWFIX_ON
    #if (SHADERPASS == SHADERPASS_SHADOWS)
        unity_ObjectToWorld = BuildObjectToWorldMatrix(detailBuffer[unity_InstanceID]);
        unity_WorldToObject = StormInverse(unity_ObjectToWorld);
    
    #else
        unity_ObjectToWorld = BuildObjectToWorldMatrix(detailBuffer[unity_InstanceID]);
        unity_WorldToObject = StormInverse(unity_ObjectToWorld);
    #endif
#else
    unity_ObjectToWorld = BuildObjectToWorldMatrix(detailBuffer[unity_InstanceID]);
    unity_WorldToObject = StormInverse(unity_ObjectToWorld);
    //unity_WorldToObject = detailBuffer[unity_InstanceID].world2Obj;
#endif
    unity_ObjectToWorld._41 = 0;
    unity_ObjectToWorld._42 = 0;
    unity_ObjectToWorld._43 = 0;
    unity_ObjectToWorld._44 = 1;


#endif
}

void setupCustom(uint instanceID)
{

#undef unity_ObjectToWorld 
#undef unity_WorldToObject 

#define unity_ObjectToWorld unity_ObjectToWorld
#define unity_WorldToObject unity_WorldToObject

#ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED

    unity_ObjectToWorld = BuildObjectToWorldMatrix(detailBuffer[instanceID]);
    unity_WorldToObject = StormInverse(unity_ObjectToWorld);
    
    unity_ObjectToWorld._41 = 0;
    unity_ObjectToWorld._42 = 0;
    unity_ObjectToWorld._43 = 0;
    unity_ObjectToWorld._44 = 1;

#endif
}

void GetWorldStartPosY_float(out float worldStartPosY)
{
    worldStartPosY =_ClipmapData1.x;
}

void GetWorldStartPosY_half(out float worldStartPosY)
{
    worldStartPosY =_ClipmapData1.x;
}

void GetHeightScale_float(out float heightScale)
{
    heightScale = _ClipmapData1.y;
}

void GetHeightScale_half(out float heightScale)
{
    heightScale = _ClipmapData1.y;
}

//Dummy function to insert Indirect Instancing Pre Processors
void DummyIndirect_float(float3 inPos, out float3 outPos)
{
    outPos = inPos;
}

void DummyIndirect_half(float3 inPos, out float3 outPos)
{
    outPos = inPos;
}

//Return the ring index of the clipmap
void getRingIndex_float(out float index)
{
    index = ringIndex;
}

void getRingIndex_half(out half index)
{
    index = ringIndex;
}

//Return the rotation to correct tangent space normals 
void getDirection_float(out float direction)
{
    direction = -rotatedDirection;
}

void getDirection_half(out half direction)
{
    direction = -rotatedDirection;
}

//Return the shape index
void getShapeIndex_float(out float index)
{
    index = shapeIndex;
}

void getShapeIndex_half(out half index)
{
    index = shapeIndex;
}

//Clipmap Global Data
void getClipmapGlobals_float(out float Levels, out float N)
{
    Levels = _ClipmapData0.y;
    N = _ClipmapData0.x;
}

void Remap_float(float2 value, float2 from1, float2 to1, float2 from2, float2 to2, out float2 uv) {
    uv = (value - from1) / (to1 - from1) * (to2 - from2) + from2;
}

void getCameraPos_float(out float2 cameraPos)
{
    cameraPos = _ClipmapCamPos.xy;
}

void createRingMask_float(float2 uv, float edgeRadius, out float mask)
{
    float2 maskedStep = 1 - smoothstep(edgeRadius, 1, abs(uv * 2 - 1));
    mask = maskedStep.x * maskedStep.y;
}

void getRingUVs_float(float2 worldPos, float level, out float2 uv)
{
    int scale = 1 << (int)level;
    int nextScale = scale * 2;
    //_ClipmapData0.z = blockSize
    int levelSize = ((((_ClipmapData0.z - 1) * 4) + 2) * scale) * 0.5;

    float2 camPos = _ClipmapCamPos.xy;

    float2 curLevelPos = float2(floor(camPos.x / scale) * scale, floor(camPos.y / scale) * scale);
    float2 nextLevelPos = float2(floor(camPos.x / nextScale) * nextScale, floor(camPos.y / nextScale) * nextScale);

    int displace = pow(2, level);

    float2 offset = curLevelPos + (nextLevelPos - curLevelPos) + float2(displace, displace + 1);

    Remap_float(worldPos, offset - levelSize, offset + levelSize, float2(0, 0), float2(1, 1), uv);
}

#endif
