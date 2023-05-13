#ifndef CUSTOM_UNLIT_PASS_INCLUDED
#define CUSTOM_UNLIT_PASS_INCLUDED

#include "ShaderLab/Common.hlsl"

//CBUFFER_START(UnityPerMaterial)
//	float4 _BaseColor;
//CBUFFER_END

UNITY_INSTANCING_BUFFER_START(UnityPerMaterial)
	UNITY_DEFINE_INSTANCED_PROP(float4, _BaseColor)
UNITY_INSTANCING_BUFFER_END(UnityPerMaterial)

struct Attributes {
	float3 positionOS : POSITION;
	UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct Varyings {
	float4 positionCS : SV_POSITION;
	UNITY_VERTEX_INPUT_INSTANCE_ID
};

Varyings UnlitPassVertex(Attributes IN)
{
	Varyings OUT;

	UNITY_SETUP_INSTANCE_ID(IN);
	UNITY_TRANSFER_INSTANCE_ID(IN, OUT);

	float3 positionWS = TransformObjectToWorld(IN.positionOS.xyz);
	OUT.positionCS = TransformWorldToHClip(positionWS);

	return OUT;
}

float4 UnlitPassFragment(Varyings IN) : SV_TARGET
{
	UNITY_SETUP_INSTANCE_ID(IN);
	return UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _BaseColor);
}

#endif
