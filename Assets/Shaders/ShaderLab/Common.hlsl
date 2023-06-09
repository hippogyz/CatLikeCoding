﻿#ifndef CUSTOM_COMMON_INCLUDED
#define CUSTOM_COMMON_INCLUDED

#ifdef REAL_IS_HALF
    #define real4 half4
#else
    #define real4 float4
#endif

// for gpu instancing
#if defined(_SHADOW_MASK_ALWAYS) || defined(_SHADOW_MASK_DISTANCE)
    #define SHADOWS_SHADOWMASK
#endif

#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/CommonMaterial.hlsl"

#include "UnityInput.hlsl"

#define UNITY_MATRIX_M unity_ObjectToWorld
#define UNITY_MATRIX_I_M unity_WorldToObject
#define UNITY_MATRIX_V unity_MatrixV
#define UNITY_MATRIX_VP unity_MatrixVP
#define UNITY_MATRIX_P glstate_matrix_projection

#define UNITY_PREV_MATRIX_M unity_PrevObjectToWorld
#define UNITY_PREV_MATRIX_I_M unity_WorldToPrevObject


//
//float3 TransformObjectToWorld(float3 positionOS)
//{
//	return mul(unity_ObjectToWorld, float4(positionsOS, 1.0)).xyz
//}
//
//float4 TransformWorldToHClip(float3 positionWS)
//{
//	return mul(unity_MatrixVP, float4(positionWS, 1.0));
//}

#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/UnityInstancing.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/SpaceTransforms.hlsl"

float Square(float v)
{
    return v * v;
}

float DistanceSquare(float3 p1, float3 p2)
{
    return dot(p1 - p2, p1 - p2);
}

#endif
