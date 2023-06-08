#ifndef SHADOW_CASTER_PASS_INCLUDED
#define SHADOW_CASTER_PASS_INCLUDED

struct Attributes{
    float3 positionOS : POSITION;
    float2 baseUV : TEXCOORD0;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct Varyings{
    float4 positionCS : SV_POSITION;
    float2 baseUV : VAR_BASE_UV;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

Varyings ShadowCasterPassVertex(Attributes IN)
{
    Varyings OUT;

    UNITY_SETUP_INSTANCE_ID(IN);
    UNITY_TRANSFER_INSTANCE_ID(IN, OUT);

    float3 positionWS = TransformObjectToWorld(IN.positionOS);
    OUT.positionCS = TransformWorldToHClip(positionWS);

    // eliminate shadow pancake
    #if UNITY_REVERSED_Z
        OUT.positionCS.z = min(OUT.positionCS.z, OUT.positionCS.w * UNITY_NEAR_CLIP_VALUE);
    #else
        OUT.positionCS.z = max(OUT.positionCS.z, OUT.positionCS.w * UNITY_NEAR_CLIP_VALUE);
    #endif

    OUT.baseUV = TransformBaseUV(IN.baseUV);

    return OUT;
}

void ShadowCasterPassFragment(Varyings IN)
{
    UNITY_SETUP_INSTANCE_ID(IN)
    float4 base = GetBaseColor(IN.baseUV);

    #if defined(_SHADOWS_CLIP)
        float alphaCutoff = GetCutoff();
        clip(base.a - alphaCutoff);
    #elif defined(_SHADOWS_DITHER)
        float dither = InterleavedGradientNoise(IN.positionCS.xy, 0);
        clip(base.a - dither);
    #endif
}

#endif