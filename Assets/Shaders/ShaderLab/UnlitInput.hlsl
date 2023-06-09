#ifndef CUSTOM_UNLIT_INPUT_INCLUDED
#define CUSTOM_UNLIT_INPUT_INCLUDED


TEXTURE2D(_BaseMap);
SAMPLER(sampler_BaseMap);

// Unity Per Material
UNITY_INSTANCING_BUFFER_START(UnityPerMaterial)
	
	UNITY_DEFINE_INSTANCED_PROP(float4, _BaseMap_ST)
	UNITY_DEFINE_INSTANCED_PROP(float4, _BaseColor)
	UNITY_DEFINE_INSTANCED_PROP(float, _Cutoff)

UNITY_INSTANCING_BUFFER_END(UnityPerMaterial)

float2 TransformBaseUV(float2 baseUV)
{
    float4 st = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _BaseMap_ST);
    return baseUV * st.xy + st.zw;
}

float4 GetBaseColor(float2 uv)
{
    float4 baseColor = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _BaseColor);
    float4 baseMap = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, uv);

    return baseColor * baseMap;
}

float GetCutoff()
{
    return UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _Cutoff);
}

float GetMetallic()
{
    return 0.0;
}

float GetSmoothness()
{
    return 0.0;
}

float3 GetEmission(float2 uv)
{
    return GetBaseColor(uv).rgb;
}

#endif
