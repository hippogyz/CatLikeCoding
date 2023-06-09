#ifndef CUSTOM_META_PASS_INCLUDED
#define CUSTOM_META_PASS_INCLUDED

#include "ShaderLab/Surface.hlsl"
#include "ShaderLab/Shadows.hlsl"
#include "ShaderLab/Light.hlsl"
#include "ShaderLab/BRDF.hlsl"


struct Attributes
{
	float3 positionOS : POSITION;
	float2 baseUV : TEXCOORD0;
    float2 lightMapUV : TEXCOORD1;
};

struct Varyings
{
	float4 positionCS : SV_POSITION;
	float2 baseUV : VAR_BASE_UV;
};

Varyings MetaPassVertex(Attributes IN)
{
    Varyings OUT;

    IN.positionOS.xy = IN.lightMapUV * unity_LightmapST.xy + unity_LightmapST.zw;
    IN.positionOS.z = IN.positionOS > 0.0 ? FLT_MIN : 0.0;

    OUT.positionCS = TransformWorldToHClip(IN.positionOS);
	OUT.baseUV = TransformBaseUV(IN.baseUV);

    return OUT;
}

float4 MetaPassFragment(Varyings IN) : SV_TARGET
{
    float4 baseColor = GetBaseColor(IN.baseUV);

    Surface surface;
    ZERO_INITIALIZE(Surface, surface);
    surface.color = baseColor.rgb;
    surface.metallic = GetMetallic();
    surface.smoothness = GetSmoothness();

    BRDF brdf = GetBRDF(surface);
    
    float4 meta = 0.0;

    if(unity_MetaFragmentControl.x)
    {
        meta = float4(brdf.diffuse, 1.0);
        meta.rgb += brdf.specular * brdf.roughness * 0.5;
        meta.rgb = min(
            PositivePow(meta.rgb, unity_OneOverOutputBoost),
            unity_MaxOutputValue);
    }
    else if(unity_MetaFragmentControl.y)
    {
        meta = float4(GetEmission(IN.baseUV), 1.0);
    }

    return meta;
}


#endif