#ifndef SOULBREAK_COMMON_INCLUDED
#define SOULBREAK_COMMON_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

// Shared vertex setup for all Soulbreak sprite shaders.
// Shapes are drawn procedurally from UVs, so every shader works on a plain square sprite.

struct Attributes
{
    float4 positionOS : POSITION;
    float4 color      : COLOR;
    float2 uv         : TEXCOORD0;
};

struct Varyings
{
    float4 positionCS : SV_POSITION;
    float4 color      : COLOR;
    float2 uv         : TEXCOORD0;
    float3 positionWS : TEXCOORD1;
};

Varyings SoulbreakVert(Attributes i)
{
    Varyings o;
    o.positionWS = TransformObjectToWorld(i.positionOS.xyz);
    o.positionCS = TransformWorldToHClip(o.positionWS);
    o.color = i.color;
    o.uv = i.uv;
    return o;
}

// Centered coords: -1..1, length 1 at the sprite edge.
float2 Centered(float2 uv) { return uv * 2.0 - 1.0; }

float Hash21(float2 p)
{
    p = frac(p * float2(123.34, 456.21));
    p += dot(p, p + 45.32);
    return frac(p.x * p.y);
}

float ValueNoise(float2 p)
{
    float2 i = floor(p);
    float2 f = frac(p);
    float2 u = f * f * (3.0 - 2.0 * f);
    float a = Hash21(i);
    float b = Hash21(i + float2(1, 0));
    float c = Hash21(i + float2(0, 1));
    float d = Hash21(i + float2(1, 1));
    return lerp(lerp(a, b, u.x), lerp(c, d, u.x), u.y);
}

float Fbm(float2 p)
{
    float v = 0.0, a = 0.5;
    for (int k = 0; k < 4; k++) { v += a * ValueNoise(p); p *= 2.03; a *= 0.5; }
    return v;
}

#endif
