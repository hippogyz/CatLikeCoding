#ifndef CUSTOM_UNLIT_PASS_INCLUDED
#define CUSTOM_UNLIT_PASS_INCLUDED

#include "ShaderLab/Common.hlsl"

//CBUFFER_START(UnityPerMaterial)
//	float4 _BaseColor;
//CBUFFER_END

// TEXTURE2D(_BaseMap);
// SAMPLER(sampler_BaseMap);

// // Unity Per Material
// UNITY_INSTANCING_BUFFER_START(UnityPerMaterial)
	
// 	UNITY_DEFINE_INSTANCED_PROP(float4, _BaseMap_ST)
// 	UNITY_DEFINE_INSTANCED_PROP(float4, _BaseColor)
// 	UNITY_DEFINE_INSTANCED_PROP(float, _Cutoff)

// UNITY_INSTANCING_BUFFER_END(UnityPerMaterial)

struct Attributes {
	float3 positionOS : POSITION;
	float2 baseUV : TEXCOORD0;
	UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct Varyings {
	float4 positionCS : SV_POSITION;
	float2 baseUV : VAR_BASE_UV;
	UNITY_VERTEX_INPUT_INSTANCE_ID
};

Varyings UnlitPassVertex(Attributes IN)
{
	Varyings OUT;

	UNITY_SETUP_INSTANCE_ID(IN);
	UNITY_TRANSFER_INSTANCE_ID(IN, OUT);

	float3 positionWS = TransformObjectToWorld(IN.positionOS.xyz);
	OUT.positionCS = TransformWorldToHClip(positionWS);

	OUT.baseUV = TransformBaseUV(IN.baseUV);

	return OUT;
}

float4 UnlitPassFragment(Varyings IN) : SV_TARGET
{
	UNITY_SETUP_INSTANCE_ID(IN);
	float4 color = GetBaseColor(IN.baseUV);
	
	// Discard frag while alpha is smaller than cutoff
	#if defined(_CLIPPING)
	float alphaCutoff = GetCutoff();
	clip(color.a - alphaCutoff);
	#endif

	return color;
}

#endif
