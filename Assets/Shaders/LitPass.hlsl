#ifndef CUSTOM_LIT_PASS_INCLUDED
#define CUSTOM_LIT_PASS_INCLUDED

#include "ShaderLab/Surface.hlsl"
#include "ShaderLab/Shadows.hlsl"
#include "ShaderLab/Light.hlsl"
#include "ShaderLab/BRDF.hlsl"
#include "ShaderLab/GI.hlsl"
#include "ShaderLab/Lighting.hlsl"

//CBUFFER_START(UnityPerMaterial)
//	float4 _BaseColor;
//CBUFFER_END


struct Attributes {
	float3 positionOS : POSITION;
	float3 normalOS : NORMAL;
	float2 baseUV : TEXCOORD0;

	GI_ATTRIBUTE_DATA
	UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct Varyings {
	float4 positionCS : SV_POSITION;
	float3 positionWS : VAR_POSITION;
	float3 normalWS : VAR_NORMAL;
	float2 baseUV : VAR_BASE_UV;

	GI_VARYINGS_DATA
	UNITY_VERTEX_INPUT_INSTANCE_ID
};

Varyings LitPassVertex(Attributes IN)
{
	Varyings OUT;

	UNITY_SETUP_INSTANCE_ID(IN);
	UNITY_TRANSFER_INSTANCE_ID(IN, OUT);
	TRANSFER_GI_DATA(IN, OUT);

	OUT.positionWS = TransformObjectToWorld(IN.positionOS.xyz);
	OUT.positionCS = TransformWorldToHClip(OUT.positionWS);

	OUT.normalWS = TransformObjectToWorldNormal(IN.normalOS);

	OUT.baseUV = TransformBaseUV(IN.baseUV);

	return OUT;
}

float4 LitPassFragment(Varyings IN) : SV_TARGET
{
	UNITY_SETUP_INSTANCE_ID(IN);
	float4 base = GetBaseColor(IN.baseUV);

	// Discard frag while alpha is smaller than cutoff
	#if defined(_CLIPPING)
	float alphaCutoff = GetCutoff();
	clip(base.a - alphaCutoff);
	#endif

	// Lighting
	Surface surface;
	surface.position = IN.positionWS;
	surface.normal = normalize(IN.normalWS);
	surface.viewDirection = normalize(_WorldSpaceCameraPos - IN.positionWS);
	surface.depth = -TransformWorldToView(IN.positionWS).z;
	surface.color = base.xyz;
	surface.alpha = base.a;
	surface.metallic = GetMetallic();
	surface.smoothness = GetSmoothness();
	surface.dither = InterleavedGradientNoise(IN.positionCS.xy, 0);

	#if defined(_PREMULTIPLY_ALPHA)
		BRDF brdf = GetBRDF(surface, true);
	#else
		BRDF brdf = GetBRDF(surface);
	#endif

	GI gi = GetGI(GI_FRAGMENT_DATA(IN), surface);
	
	float3 color = GetLighting(surface, brdf, gi);

	return float4(color, surface.alpha);
}

#endif
