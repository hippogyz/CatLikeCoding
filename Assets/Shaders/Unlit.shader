﻿Shader "Custom RP/Unlit"
{
    Properties
    {
        [Space]
        _BaseMap("Texture", 2D) = "white" {}
        [HDR] _BaseColor("Color", Color) = (1.0, 1.0, 1.0, 1.0)

        [Space]
        [KeywordEnum(On, Clip, Dither, Off)] _Shadows("Shadows", Float) = 0

        [Space]
        [Toggle(_CLIPPING)] _Clipping ("Alpha Clip", Float) = 0
        _Cutoff("Alpha Cutoff", Range(0, 1)) = 0
    
        [Space]
        [Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend("Src Blend", Float) = 1
        [Enum(UnityEngine.Rendering.BlendMode)] _DstBlend("Dst Blend", Float) = 0
        
        [Space]
        [Enum(Off, 0, On, 1)] _ZWrite("Z Write", Float) = 1

        [HideInInspector] _MainTex("Texture for Lightmap", 2D) = "white" {}
		[HideInInspector] _Color("Color for Lightmap", Color) = (0.5, 0.5, 0.5, 1.0)
    }
    SubShader
    {
        HLSLINCLUDE
        #include "ShaderLab/Common.hlsl"
        #include "ShaderLab/UnlitInput.hlsl"
        ENDHLSL
            
        Pass
        {
            Blend [_SrcBlend] [_DstBlend]
            ZWrite [_ZWrite]

            HLSLPROGRAM

            // Custom
            #pragma shader_feature _CLIPPING

            #pragma multi_compile_instancing
            #pragma vertex UnlitPassVertex
            #pragma fragment UnlitPassFragment
            #include "UnlitPass.hlsl"

            ENDHLSL
        }
        
        Pass
        {
            Tags
            {
                "LightMode" = "ShadowCaster"
            }

            ColorMask 0

            HLSLPROGRAM
            #pragma target 3.5

            // Custom
            // #pragma shader_feature _CLIPPING
            #pragma shader_feature _ _SHADOWS_CLIP _SHADOWS_DITHER
            
            #pragma multi_compile_instancing
            #pragma vertex ShadowCasterPassVertex
            #pragma fragment ShadowCasterPassFragment
            #include "ShadowCasterPass.hlsl"

            ENDHLSL
        }
        
        Pass
        {
            Tags
            {
                "LightMode" = "Meta"
            }

            Cull Off

            HLSLPROGRAM

            #pragma target 3.5
            #pragma vertex MetaPassVertex
            #pragma fragment MetaPassFragment
            #include "MetaPass.hlsl"
            
            ENDHLSL
        }
    }
    

    CustomEditor "ShaderEditor.CustomShaderGUI"
}
