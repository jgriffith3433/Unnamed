#ifndef PW_DITHERFADE_INCLUDED
#define PW_DITHERFADE_INCLUDED

float GetThreshold(int index)
{
    switch (index) {
    case 0:
        return 1.0 / 17.0;
    case 1:
        return 9.0 / 17.0;
    case 2:
        return 3.0 / 17.0;
    case 3:
        return 11.0 / 17.0;
    case 4:
        return 13.0 / 17.0;
    case 5:
        return 5.0 / 17.0;
    case 6:
        return 15.0 / 17.0;
    case 7:
        return 7.0 / 17.0;
    case 8:
        return 4.0 / 17.0;
    case 9:
        return 12.0 / 17.0;
    case 10:
        return 2.0 / 17.0;
    case 11:
        return 10.0 / 17.0;
    case 12:
        return 16.0 / 17.0;
    case 13:
        return 8.0 / 17.0;
    case 14:
        return 14.0 / 17.0;
    case 15:
        return 6.0 / 17.0;
    }
}

void DitherFade_float(float4 screenPos, float fade, float cutoff, out float alpha)
{
#if (SHADERPASS == SHADERPASS_SHADOWS) || (SHADERPASS == SHADERPASS_RAYTRACING_VISIBILITY)
    alpha = fade;
#else

    float2 uv = screenPos.xy * _ScreenParams.xy;
    float dither = GetThreshold((uint(uv.x) % 4) * 4 + uint(uv.y) % 4);

    alpha = saturate(fade - dither);
    clip(alpha - cutoff);
#endif
}
void DitherFade_half(half4 screenPos, half fade, half cutoff, out half alpha)
{
#if (SHADERPASS == SHADERPASS_SHADOWS) || (SHADERPASS == SHADERPASS_RAYTRACING_VISIBILITY)
    alpha = fade;
#else

    half2 uv = screenPos.xy * _ScreenParams.xy;
    half dither = GetThreshold((uint(uv.x) % 4) * 4 + uint(uv.y) % 4);

    alpha = saturate(fade - dither) * (1 - cutoff) + cutoff;
    clip(alpha - cutoff);
#endif
}
#endif
