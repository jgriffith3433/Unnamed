#ifndef STORM_INDIRECT_BASE_INCLUDE
#define STORM_INDIRECT_BASE_INCLUDE
#ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
struct RenderData {
	column_major float4x4 m;
	half2 d;
};
StructuredBuffer<RenderData> storm_buffer;
half2 storm_data; //Color, Fade values
#ifdef STORM_FRAGMENT
float4x4 OriginalMatrix;
#endif	
#endif

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

void storm_setup()
{
#if defined(STORM_ENABLED) | defined(_STORM_ENABLED)
#ifdef STORM_FRAGMENT
	OriginalMatrix = unity_WorldToObject;
#endif	
	//not sure why this is required to fix HDRP instancing but it is
	#undef unity_ObjectToWorld
	#undef unity_WorldToObject
	#define unity_ObjectToWorld unity_ObjectToWorld
	#define unity_WorldToObject unity_WorldToObject

	#ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
		unity_ObjectToWorld = storm_buffer[unity_InstanceID].m;
		unity_WorldToObject = StormInverse(unity_ObjectToWorld);
		storm_data = storm_buffer[unity_InstanceID].d;
#endif
#endif
}

//Dummy function to insert Indirect Instancing Pre Processors
void StormIndirect_float(float3 inPos, out float3 outPos)
{
	outPos = inPos;
}

void StormIndirect_half(half3 inPos, out half3 outPos)
{
	outPos = inPos;
}
void lodFade_float(float inDummy, out float outFade, out float outColor)
{
#ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
	outFade = saturate(storm_data.y);
	outColor = saturate(storm_data.x);
#else
	outFade = 1;
	outColor = 1;
#endif
}
void lodFade_half(float inDummy, out half outFade, out half outColor)
{
#ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
	outFade = saturate(storm_data.y);
	outColor = saturate(storm_data.x);
#else
	outFade = 1;
	outColor = 1;
#endif
}
#endif