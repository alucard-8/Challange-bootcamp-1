// Soul reactor: swirling energy core that dims with health, segmented shield ring
// (one segment per charge) and a red flash when hit. Driven by ReactorView.
Shader "Soulbreak/Reactor"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite", 2D) = "white" {}
        [HDR] _Color ("Core Color", Color) = (0.6, 1.6, 3, 1)
        [HDR] _ShieldColor ("Shield Color", Color) = (0.5, 2.5, 3, 1)
        [HDR] _HitColor ("Hit Color", Color) = (5, 0.6, 0.6, 1)
        _Health01 ("Health", Range(0, 1)) = 1
        _Shield ("Shield Charges", Float) = 3
        _ShieldMax ("Max Shield", Float) = 3
        _Flash ("Hit Flash", Range(0, 1)) = 0
        _Cracks ("Cracks", Range(0, 1)) = 0
        _Alarm ("Alarm", Range(0, 1)) = 0
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
                half4 _ShieldColor;
                half4 _HitColor;
                float _Health01;
                float _Shield;
                float _ShieldMax;
                float _Flash;
                float _Cracks;
                float _Alarm;
            CBUFFER_END

            // Cellular noise edge distance: small near cell borders = crack lines.
            float CrackLines(float2 p)
            {
                float2 i = floor(p), f = frac(p);
                float d1 = 8.0, d2 = 8.0;
                for (int y = -1; y <= 1; y++)
                for (int x = -1; x <= 1; x++)
                {
                    float2 g = float2(x, y);
                    float2 o = float2(Hash21(i + g), Hash21(i + g + 17.3));
                    float d = length(g + o - f);
                    if (d < d1) { d2 = d1; d1 = d; } else if (d < d2) d2 = d;
                }
                return d2 - d1;
            }

            half4 frag(Varyings i) : SV_Target
            {
                float2 p = Centered(i.uv);
                float d = length(p);
                float ang = atan2(p.y, p.x);
                float t = _Time.y;

                // --- Core: swirling energy, dims and flickers as health drops
                float coreR = 0.42;
                float core = smoothstep(coreR, coreR - 0.03, d);
                float swirl = Fbm(float2(ang * 2.0 + t * 0.8 + d * 3.0, d * 5.0 - t * 1.5));
                float flicker = lerp(0.6 + 0.4 * sin(t * 30.0), 1.0, _Health01);
                float energy = (0.15 + 0.9 * swirl * swirl) * (0.3 + 0.7 * _Health01) * flicker;
                float hot = pow(saturate(1.0 - d / coreR), 4.0);
                float bands = 0.5 + 0.5 * sin(d * 40.0 - t * 6.0);
                float3 col = _Color.rgb * (energy * (0.6 + 0.4 * bands) + hot * 0.9) * core;

                // Shell line around the core
                float shell = smoothstep(0.025, 0.0, abs(d - coreR));
                col += _Color.rgb * shell * 1.5;

                // --- Shield ring: one segment per max charge, lit if charged
                float segPos = (ang / 6.2831853 + 0.5) * _ShieldMax;
                float segId = floor(segPos);
                float segF = frac(segPos);
                float gap = smoothstep(0.0, 0.06, segF) * smoothstep(1.0, 0.94, segF);
                float lit = step(segId + 0.5, _Shield);
                float ringR = 0.72 + 0.015 * sin(t * 3.0);
                float ringMask = smoothstep(0.06, 0.0, abs(d - ringR)) * gap * step(0.5, _ShieldMax);
                float shimmer = 0.8 + 0.4 * sin(ang * 6.0 - t * 4.0);
                col += _ShieldColor.rgb * ringMask * lerp(0.08, shimmer, lit);

                // Soft outer aura
                float aura = pow(saturate(1.0 - d), 4.0) * 0.6;
                col += _Color.rgb * aura * (0.3 + 0.7 * _Health01);

                float alpha = saturate(core + shell + ringMask * lerp(0.25, 1.0, lit) + aura);

                // Cracks spreading over the core as it loses health (glowing hot red/orange)
                float crack = smoothstep(0.06, 0.0, CrackLines(p * 6.0));
                float reveal = step(Hash21(floor(p * 6.0) + 3.1), _Cracks * 1.2) * step(0.001, _Cracks);
                float crackGlow = crack * reveal * (core + shell) * (0.8 + 0.4 * sin(t * 12.0 + d * 20.0));
                col = lerp(col, float3(4.0, 0.9, 0.3) * 1.2, saturate(crackGlow));

                // Low-health alarm: the whole reactor throbs red
                float throb = (0.5 + 0.5 * sin(t * 9.0)) * _Alarm;
                col = lerp(col, _HitColor.rgb * 0.8, throb * 0.45 * saturate(core + shell + aura * 2.0));
                alpha = saturate(alpha + throb * aura * 0.5);

                // Hit flash
                col = lerp(col, _HitColor.rgb, _Flash * saturate(core + ringMask));

                return half4(col, alpha * i.color.a);
            }
            ENDHLSL
        }
    }
}
