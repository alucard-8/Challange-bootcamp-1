// Spiky monster with glowing rim and eye, hit flash, health ring and noise dissolve on death.
// Per-enemy values (_Color, _Spikes, _Flash, _HealthFrac, _Dissolve) come from a MaterialPropertyBlock.
Shader "Soulbreak/Enemy"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite", 2D) = "white" {}
        [HDR] _Color ("Color", Color) = (2, 0.5, 0.5, 1)
        [HDR] _EdgeColor ("Dissolve Edge", Color) = (4, 2, 0.6, 1)
        _Spikes ("Spikes", Float) = 6
        _Seed ("Seed", Float) = 0
        _Flash ("Hit Flash", Range(0, 1)) = 0
        _HealthFrac ("Health", Range(0, 1)) = 1
        _Dissolve ("Dissolve", Range(0, 1)) = 0
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "RenderPipeline"="UniversalPipeline" "IgnoreProjector"="True" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            Tags { "LightMode"="Universal2D" }
            HLSLPROGRAM
            #pragma vertex SoulbreakVert
            #pragma fragment frag
            #include "SoulbreakCommon.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _Color;
                half4 _EdgeColor;
                float _Spikes;
                float _Seed;
                float _Flash;
                float _HealthFrac;
                float _Dissolve;
            CBUFFER_END

            half4 frag(Varyings i) : SV_Target
            {
                float2 p = Centered(i.uv);
                float d = length(p);
                float ang = atan2(p.y, p.x);
                float t = _Time.y + _Seed;

                // Wobbly spiky silhouette
                float spikes = 0.07 * sin(ang * _Spikes + t * 3.0) + 0.03 * sin(ang * 3.0 - t * 2.0);
                float r = 0.72 + spikes;
                float body = smoothstep(r, r - 0.05, d);
                float rim = body * smoothstep(r - 0.22, r - 0.05, d);

                // Inner swirl and glowing eye
                float swirl = Fbm(float2(ang * 1.5 + t * 0.6, d * 4.0 - t));
                float eye = smoothstep(0.2, 0.1, d) * (0.7 + 0.3 * sin(t * 8.0));

                float3 col = _Color.rgb * (0.12 + 0.35 * swirl) * body;
                col += _Color.rgb * rim * 1.4;
                col += _Color.rgb * eye * 2.5 + eye * 0.5;

                // Health ring (only visible when damaged)
                float hp01 = (atan2(p.x, p.y) / 6.2831853) + 0.5;
                float hpRing = smoothstep(0.035, 0.0, abs(d - 0.9)) * step(hp01, _HealthFrac) * step(_HealthFrac, 0.999);
                col += _Color.rgb * hpRing * 1.5;

                float alpha = max(body, hpRing);

                // Hit flash
                col = lerp(col, float3(4, 4, 4), _Flash * body);

                // Noise dissolve with a hot edge
                float n = Fbm(p * 3.0 + _Seed);
                float cut = n - _Dissolve * 1.1 + 0.05;
                float edge = smoothstep(0.08, 0.0, cut) * step(0.0, cut) * step(0.001, _Dissolve);
                col += _EdgeColor.rgb * edge * 2.0;
                alpha *= step(0.0, cut);

                return half4(col, alpha * i.color.a);
            }
            ENDHLSL
        }
    }
}
