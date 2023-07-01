#ifndef CUSTOM_LIGHT_INCLUDED
#define CUSTOM_LIGHT_INCLUDED

#define MAX_DIRECTIONAL_LIGHT_COUNT 4

CBUFFER_START(_CustomLight)
    int _DirectionalLightCount;
    float4 _DirectionalLightColors[MAX_DIRECTIONAL_LIGHT_COUNT];
    float4 _DirectionalLightDirections[MAX_DIRECTIONAL_LIGHT_COUNT];
    float4 _DirectionalLightShadowData[MAX_DIRECTIONAL_LIGHT_COUNT];
CBUFFER_END

struct Light
{
    float3 color;
    float3 direction;
    float attenuation;
};

int GetDirectionalLightCount()
{
    return _DirectionalLightCount;
}

DirectionalShadowData GetDirectionalShadowData(int idx, ShadowData shadowData)
{
    DirectionalShadowData data;
    // In order to blend shadowmask, shadowData.strength is no longer combined here.
    data.strength = _DirectionalLightShadowData[idx].x; // * shadowData.strength; 
    data.tileGroupIndex = _DirectionalLightShadowData[idx].y;
    data.normalBias = _DirectionalLightShadowData[idx].z;
    data.shadowMaskChannel = _DirectionalLightShadowData[idx].w;
    
    return data;
}

Light GetDirectionalLight(int idx, Surface surface, ShadowData shadowData)
{
    Light light;
    light.color = _DirectionalLightColors[idx].rgb;
    light.direction = _DirectionalLightDirections[idx].xyz;

    DirectionalShadowData dirShadowData = GetDirectionalShadowData(idx, shadowData);
    light.attenuation = GetDirectionalShadowAttenuation(dirShadowData, shadowData, surface);
    
    #if defined(_CASCADE_DEBUG)
        light.attenuation = shadowData.cascadeIndex * 0.25;  // Show cascade sphere
    #endif
    
    return light;
}

#endif