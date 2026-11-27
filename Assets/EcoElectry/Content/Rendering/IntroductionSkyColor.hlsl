#ifndef ECOELECTRY_INTRODUCTION_SKY_COLOR_INCLUDED
#define ECOELECTRY_INTRODUCTION_SKY_COLOR_INCLUDED

#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/EntityLighting.hlsl"

TEXTURECUBE(_SkyA);
SAMPLER(sampler_SkyA);
TEXTURECUBE(_SkyB);
SAMPLER(sampler_SkyB);

CBUFFER_START(UnityPerMaterial)
    half4 _SkyA_HDR, _SkyB_HDR;
    float4 _YawA, _YawB;
    half _Blend, _Luminance, _SunsetWarmth;
    half4 _FogColor;
    float4 _FogSettings; // Center XZ, start and end distances.
CBUFFER_END

float3 IntroductionSkyDirection(float3 direction, float2 yaw)
{
    return float3(yaw.x * direction.x - yaw.y * direction.z, direction.y,
                  yaw.y * direction.x + yaw.x * direction.z);
}

half3 SampleIntroductionSky(float3 ray)
{
    half3 a = DecodeHDREnvironment(SAMPLE_TEXTURECUBE(_SkyA, sampler_SkyA, IntroductionSkyDirection(ray, _YawA.xy)), _SkyA_HDR);
    half3 b = DecodeHDREnvironment(SAMPLE_TEXTURECUBE(_SkyB, sampler_SkyB, IntroductionSkyDirection(ray, _YawB.xy)), _SkyB_HDR);
    half3 sky = lerp(a, b, _Blend);
    half horizon = 1 - smoothstep(0.02, 0.52, ray.y);
    half3 tint = lerp(half3(1.05, 0.91, 0.78), half3(1.43, 0.88, 0.31), horizon);
    half brightness = dot(sky, half3(0.2126, 0.7152, 0.0722));
    half3 sunset = lerp(sky, brightness.xxx, 0.28 + 0.50 * horizon) * tint;
    sunset += half3(0.13, 0.057, 0.010) * horizon * horizon;
    half3 gradedSky = lerp(sky, sunset, _SunsetWarmth) * _Luminance;
    half haze = (1 - smoothstep(0.005, 0.17, ray.y)) * _FogColor.a;
    return lerp(gradedSky, _FogColor.rgb, haze);
}

#endif
