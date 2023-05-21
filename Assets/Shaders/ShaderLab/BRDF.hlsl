#ifndef CUSTOM_BRDF_INCLUDED
#define CUSTOM_BRDF_INCLUDED

// BRDF - bidirectional reflectance distribution function

struct BRDF
{
    float3 diffuse;
    float3 specular;
    float roughness;
};

// There is always little reflection. 
#define MIN_REFLECTIVITY 0.04

float OneMinusReflectivity(float metallic)
{
    return (1.0 - MIN_REFLECTIVITY) * (1.0 - metallic);
}

// Init BRDF
BRDF GetBRDF(Surface surface, bool applyAlphaToDiffuse = false)
{
    float oneMinusReflectivity = OneMinusReflectivity(surface.metallic);

    BRDF brdf;
     // The reflected part won't be included to diffusion
    brdf.diffuse = surface.color * oneMinusReflectivity;
    if(applyAlphaToDiffuse)
    {
        // Transparency only affects diffusion, but not specular. 
        // Premultiply alpha for diffusion. Note: SrcBlend should be ONE. 
        brdf.diffuse *= surface.alpha;
    }

    // Specular is defined by metallic
    brdf.specular = lerp(MIN_REFLECTIVITY, surface.color, surface.metallic);

    // Roughness is opposite to smoothness
    // use method from "Packages/com.unity.render-pipelines.core/ShaderLibrary/CommonMaterial.hlsl"
    float perceptual_roughness = PerceptualSmoothnessToPerceptualRoughness(surface.smoothness);
    brdf.roughness = PerceptualRoughnessToRoughness(perceptual_roughness);

    return brdf;
}

// For lighting, same formula used in URP.
float SpecularStrength(Surface surface, BRDF brdf, Light light)
{
    float r2 = Square(brdf.roughness);
    
    float3 h = SafeNormalize(light.direction + surface.viewDirection);
    float nh2 = Square(saturate(dot(surface.normal, h)));
    float lh2 = Square(saturate(dot(light.direction, h)));

    float d2 = Square(nh2 * (r2 - 1.0) + 1.0001);
    float normalization = 4.0 * brdf.roughness + 2.0;

    return r2 / (d2 * max(0.1, lh2) * normalization);
}

// Final
float3 DirectBRDF(Surface surface, BRDF brdf, Light light)
{
    return SpecularStrength(surface, brdf, light) * brdf.specular + brdf.diffuse;
}

#endif