#ifndef CUSTOM_SHADOWS_INCLUDED
#define CUSTOM_SHADOWS_INCLUDED



#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Shadow/ShadowSamplingTent.hlsl"

#if defined(_DIRECTIONAL_PCF3)
	#define DIRECTIONAL_FILTER_SAMPLES 4
	#define DIRECTIONAL_FILTER_SETUP SampleShadow_ComputeSamples_Tent_3x3
#endif

#if defined(_DIRECTIONAL_PCF5)
	#define DIRECTIONAL_FILTER_SAMPLES 9
	#define DIRECTIONAL_FILTER_SETUP SampleShadow_ComputeSamples_Tent_5x5
#endif

#if defined(_DIRECTIONAL_PCF7)
	#define DIRECTIONAL_FILTER_SAMPLES 16
	#define DIRECTIONAL_FILTER_SETUP SampleShadow_ComputeSamples_Tent_7x7
#endif



#define MAX_SHADOWED_DIRECTIONAL_LIGHT_COUNT 4
#define MAX_CASCADE_COUNT 4

TEXTURE2D_SHADOW(_DirectionalShadowAtlas);
 //Specific sampler for shadow
#define SHADOW_SAMPLER sampler_linear_clamp_compare
SAMPLER_CMP(SHADOW_SAMPLER);

CBUFFER_START(_CustomShadows)
    float4 _ShadowDistanceFade;
    int _CascadeCount;
    float4 _CascadeData[MAX_CASCADE_COUNT];
    float4 _CascadeCullingSpheres[MAX_CASCADE_COUNT];
    float4 _ShadowAtlasSize;
    float4x4 _DirectionalShadowMatrices[MAX_SHADOWED_DIRECTIONAL_LIGHT_COUNT * MAX_CASCADE_COUNT];
CBUFFER_END

struct ShadowData
{
    int cascadeIndex;
    float cascadeBlend;
    float strength;
};

struct DirectionalShadowData
{
    float strength;
    // int tileIndex;
    int tileGroupIndex;
    float normalBias;
};

float FadeShadowStrength(float distance, float scale, float fadeFactor)
{
    return saturate((1.0 - distance * scale) * fadeFactor);
}

ShadowData GetShadowData(Surface surface)
{
    ShadowData data;
    data.cascadeBlend = 1.0;
    data.strength = FadeShadowStrength(surface.depth, _ShadowDistanceFade.x, _ShadowDistanceFade.y);

    int i;
    for(i = 0; i < _CascadeCount; i++)
    {
        float3 center = _CascadeCullingSpheres[i].xyz;
        float radiusSquare = _CascadeCullingSpheres[i].w;

        float distanceSquare = DistanceSquare(surface.position, center);

        if(distanceSquare < radiusSquare)
        {
            float fade = FadeShadowStrength(distanceSquare, _CascadeData[i].x, _ShadowDistanceFade.z);
            if(i == _CascadeCount - 1)
            {
                data.strength *= fade;
            }
            else
            {
                data.cascadeBlend = fade;
            }

            break;
        }
    }

    if(i == _CascadeCount)
    {
        data.strength = 0.0;
    }
    #if defined(_CASCADE_BLEND_DITHER)
    else if(data.cascadeBlend < surface.dither)
    {
        i += 1;
    }
    #endif

    #if !defined(_CASCADE_BLEND_SOFT)
        data.cascadeBlend = 1.0;
    #endif

    data.cascadeIndex = i;

    return data;
}

float SampleDirectionalShadowAtlas(float3 positionSTS)
{
    return SAMPLE_TEXTURE2D_SHADOW(_DirectionalShadowAtlas, SHADOW_SAMPLER, positionSTS);
}

float FilterDirectionalShadow(float3 positionSTS)
{
    #if defined(DIRECTIONAL_FILTER_SAMPLES)
        // Sample by macro
        float weights[DIRECTIONAL_FILTER_SAMPLES];
        float2 positions[DIRECTIONAL_FILTER_SAMPLES];
        float4 size = _ShadowAtlasSize.yyxx;
        DIRECTIONAL_FILTER_SETUP(size, positionSTS.xy, weights, positions);

        float shadow = 0;
        for(int i = 0; i < DIRECTIONAL_FILTER_SAMPLES; i++)
        {
            shadow += weights[i] * SampleDirectionalShadowAtlas(float3(positions[i].xy, positionSTS.z));
        }
        return shadow;
    #else
        return SampleDirectionalShadowAtlas(positionSTS);
    #endif
}

float GetCascadeShadow(DirectionalShadowData directional, int cascadeIndex, Surface surface)
{
    float texelSize = _CascadeData[cascadeIndex].y; // eliminate self-shadowing
    float3 normalBias = surface.normal * (directional.normalBias * texelSize); // eliminate wrong shadow in crossing boundary
    float3 surfacePos = surface.position + normalBias;

    float3 positionSTS = mul(_DirectionalShadowMatrices[directional.tileGroupIndex + cascadeIndex], float4(surfacePos, 1.0)).xyz;
    float shadow = FilterDirectionalShadow(positionSTS);

    return shadow;
}

float GetDirectionalShadowAttenuation(DirectionalShadowData directional, ShadowData shadowData, Surface surface)
{
    #if !defined(_RECEIVE_SHADOW)
        return 1.0;
    #endif

    if(directional.strength <= 0.0)
    {
        return 1.0;
    }

    float shadow = GetCascadeShadow(directional, shadowData.cascadeIndex, surface);

    if(shadowData.cascadeBlend < 1.0)
    {
        float blendShadow = GetCascadeShadow(directional, shadowData.cascadeIndex + 1, surface);
        shadow = lerp(blendShadow, shadow, shadowData.cascadeBlend);
    }

    return lerp(1.0, shadow, directional.strength); // 1 represents no shadow
}

#endif