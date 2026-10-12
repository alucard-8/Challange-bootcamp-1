// Star Core: boiling plasma surface with limb darkening, solar corona and flares, magnetosphere
// (shield) ring, sunspots as it takes damage and red-giant instability at low health.
// Same property interface as the Reactor shader, so ReactorView / CoreUpgradeView drive it unchanged.
//   upgrades: offense = solar prominences, defense = asteroid belt, economy = outer corona halo.
Shader "Soulbreak/Star"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite", 2D) = "white" {}
        [HDR] _Color ("Star Color", Color) = (3.2, 1.8, 0.6, 1)
        [HDR] _ShieldColor ("Magnetosphere Color", Color) = (0.6, 2.2, 3.4, 1)
        [HDR] _HitColor ("Hit Color", Color) = (5, 1.2, 0.4, 1)
        _Health01 ("Health", Range(0, 1)) = 1
        _Shield ("Shield Charges", Float) = 3
        _ShieldMax ("Max Shield", Float) = 3
        _Flash ("Hit Flash", Range(0, 1)) = 0
        _Cracks ("Sunspots", Range(0, 1)) = 0
        _Alarm ("Instability", Range(0, 1)) = 0
        _UpOffense ("Upgrades: Offense", Float) = 0
        _UpDefense ("Upgrades: Defense", Float) = 0
        _UpEconomy ("Upgrades: Economy", Float) = 0
        _Tier ("Tier", Float) = 0
        _TierPulse ("Tier Pulse", Range(0, 1)) = 0
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
                float _UpOffense;
                float _UpDefense;
                float _UpEconomy;
                float _Tier;
                float _TierPulse;
            CBUFFER_END

            half4 frag(Varyings i) : SV_Target
            {
                float2 p = Centered(i.uv);
                float d = length(p);
                float ang = atan2(p.y, p.x);
                float a01 = ang / 6.2831853 + 0.5;
                float t = _Time.y;

                // Instability makes the star wobble and redden as it dies
                float wobble = _Alarm * 0.02 * sin(t * 13.0 + ang * 5.0);
                float starR = 0.4 + wobble + 0.01 * min(_Tier, 6.0);
                float body = smoothstep(starR, starR - 0.012, d);

                // Plasma surface: two layers of moving granulation + limb darkening
                float2 sp = p / starR;
                float gran = Fbm(sp * 5.0 + float2(t * 0.25, -t * 0.18));
                gran = 0.55 * gran + 0.45 * Fbm(sp * 11.0 - float2(t * 0.4, t * 0.3));
                float limb = sqrt(saturate(1.0 - (d / starR) * (d / starR)));
                float3 hot = lerp(_Color.rgb * 0.45, _Color.rgb * 0.9 + 0.5, pow(gran, 2.0) * limb);
                float3 red = float3(2.6, 0.4, 0.15);
                hot = lerp(hot, red * (0.4 + gran), (1.0 - _Health01) * 0.7);
                float3 col = hot * (0.3 + 0.5 * limb) * body * (1.0 + 0.05 * min(_Tier, 4.0));

                // Sunspots (damage)
                float spots = smoothstep(0.62, 0.7, Fbm(sp * 4.0 + 13.0)) * step(0.001, _Cracks) * step(Fbm(sp * 2.0 + 3.0), _Cracks * 1.1);
                col = lerp(col, col * 0.15, spots * body);

                // Corona: flickering rays fading out from the surface
                float outside = step(starR, d);
                float rays = Fbm(float2(a01 * 24.0, t * 0.6)) * Fbm(float2(a01 * 9.0 - t * 0.2, d * 3.0));
                float corona = exp(-(d - starR) * (9.0 - 3.0 * saturate(_UpEconomy))) * (0.35 + 0.9 * rays) * outside;
                col += _Color.rgb * corona * 0.5;

                // Offense: solar prominences — arcs looping off the surface
                float off = min(_UpOffense, 8.0);
                float promCount = 3.0 + off;
                float pa = frac(a01 * promCount + t * 0.03);
                float arcH = (0.05 + 0.015 * off) * sin(saturate(pa) * 3.14159);
                float prom = smoothstep(0.018, 0.0, abs(d - (starR + arcH))) * step(0.5, off) * smoothstep(0.0, 0.15, pa) * smoothstep(1.0, 0.85, pa);
                col += float3(4.0, 1.6, 0.4) * prom * (1.2 + 0.4 * sin(t * 5.0 + pa * 20.0));

                // Defense: asteroid belt — segments of rocky dots filling in
                float def = min(_UpDefense, 8.0);
                float beltOn = step(floor(a01 * 8.0) + 0.5, def);
                float2 bp = float2(a01 * 90.0 - t * 1.5, (d - 0.57) * 40.0);
                float rock = step(0.82, Hash21(floor(bp))) * smoothstep(0.6, 0.0, length(frac(bp) - 0.5));
                float belt = rock * smoothstep(0.045, 0.0, abs(d - 0.57)) * beltOn;
                col += float3(0.9, 0.8, 0.7) * belt * 1.6;

                // Magnetosphere (shield): wavy aurora ring, one segment per charge
                float segPos = a01 * _ShieldMax;
                float segId = floor(segPos);
                float segF = frac(segPos);
                float gap = smoothstep(0.0, 0.06, segF) * smoothstep(1.0, 0.94, segF);
                float lit = step(segId + 0.5, _Shield);
                float ringR = 0.74 + 0.02 * sin(ang * 7.0 + t * 2.5);
                float aurora = smoothstep(0.05, 0.0, abs(d - ringR)) * gap * step(0.5, _ShieldMax);
                float flow = 0.7 + 0.5 * sin(ang * 12.0 - t * 5.0);
                col += _ShieldColor.rgb * aurora * lerp(0.07, flow, lit);

                // Economy: big soft outer glow
                float glow = pow(saturate(1.0 - d), 3.0) * (0.25 + 0.3 * saturate(_UpEconomy));
                col += _Color.rgb * glow * 0.35;

                // Tier-up pulse
                float pulseR = starR + (1.0 - _TierPulse) * 0.55;
                float pulse = smoothstep(0.05, 0.0, abs(d - pulseR)) * _TierPulse;
                col += float3(3.5, 3.0, 2.5) * pulse;

                // Instability throb + hit flash
                float throb = (0.5 + 0.5 * sin(t * 10.0)) * _Alarm;
                col = lerp(col, red, throb * 0.35 * body);
                col = lerp(col, _HitColor.rgb, _Flash * saturate(body + aurora));

                float alpha = saturate(body + corona + prom + belt + aurora * lerp(0.25, 1.0, lit) + glow + pulse);
                return half4(col, alpha * i.color.a);
            }
            ENDHLSL
        }
    }
}
