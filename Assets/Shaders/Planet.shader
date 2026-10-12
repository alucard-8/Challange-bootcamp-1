// Planets / asteroids / comets for Star Core mode. Lit from the star at the world origin (day/night
// terminator), with surface types picked by _Spikes: 0-3 rocky + craters, 4-7 gas bands, 8+ icy + glow.
// Same property interface as the Enemy shader (flash, health ring, dissolve, elite aura, shield bubble),
// so EnemyView and the dissolve FX drive it unchanged.
Shader "Soulbreak/Planet"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite", 2D) = "white" {}
        [HDR] _Color ("Color", Color) = (2, 1, 0.6, 1)
        [HDR] _EdgeColor ("Dissolve Edge", Color) = (4, 2, 0.6, 1)
        _Spikes ("Surface Type", Float) = 0
        _Seed ("Seed", Float) = 0
        _Flash ("Hit Flash", Range(0, 1)) = 0
        _HealthFrac ("Health", Range(0, 1)) = 1
        _Dissolve ("Dissolve", Range(0, 1)) = 0
        [HDR] _AuraColor ("Elite Aura", Color) = (0, 0, 0, 0)
        _ShieldBubble ("Shield Bubble", Range(0, 1)) = 0
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
            #pragma vertex vert
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
                half4 _AuraColor;
                float _ShieldBubble;
            CBUFFER_END

            struct V2F
            {
                float4 positionCS : SV_POSITION;
                float4 color : COLOR;
                float2 uv : TEXCOORD0;
                float3 centerWS : TEXCOORD1;
            };

            V2F vert(Attributes v)
            {
                V2F o;
                float3 ws = TransformObjectToWorld(v.positionOS.xyz);
                o.positionCS = TransformWorldToHClip(ws);
                o.color = v.color;
                o.uv = v.uv;
                o.centerWS = TransformObjectToWorld(float3(0, 0, 0));
                return o;
            }

            half4 frag(V2F i) : SV_Target
            {
                float2 p = Centered(i.uv);
                float d = length(p);
                float ang = atan2(p.y, p.x);
                float t = _Time.y + _Seed;
                float R = 0.72;
                float body = smoothstep(R, R - 0.03, d);

                // Sphere normal and lighting from the star at the origin (rotation-independent)
                float3 n = float3(p / R, sqrt(saturate(1.0 - (d / R) * (d / R))));
                float2 toStar = normalize(-i.centerWS.xy + 0.0001);
                float3 l = normalize(float3(toStar, 0.6));
                float light = saturate(dot(n, l));
                float terminator = smoothstep(0.0, 0.25, light);

                // Surface
                float2 sp = p / R;
                float3 surf;
                if (_Spikes < 3.5)            // rocky with craters
                {
                    float rock = Fbm(sp * 3.0 + _Seed);
                    float craters = smoothstep(0.08, 0.0, abs(Fbm(sp * 6.0 + _Seed * 3.0) - 0.45)) * 0.4;
                    surf = _Color.rgb * (0.35 + 0.7 * rock) - craters * 0.3;
                }
                else if (_Spikes < 7.5)       // gas giant bands
                {
                    float bands = sin(sp.y * 14.0 + Fbm(sp * 2.0 + float2(t * 0.15, 0)) * 4.0);
                    surf = _Color.rgb * (0.55 + 0.35 * bands) + float3(0.2, 0.1, 0.05) * Fbm(sp * 8.0 - t * 0.2);
                }
                else                          // icy, faintly glowing
                {
                    float ice = Fbm(sp * 5.0 + _Seed);
                    surf = lerp(_Color.rgb * 0.6, float3(2.2, 2.6, 3.0), ice * 0.6);
                }

                float3 col = surf * (0.08 + 1.1 * light * terminator) * body;

                // Atmosphere rim lit on the star side
                float rim = smoothstep(R - 0.12, R, d) * body;
                col += _Color.rgb * rim * (0.25 + 0.9 * light);

                // Health ring (only when damaged)
                float hp01 = (atan2(p.x, p.y) / 6.2831853) + 0.5;
                float hpRing = smoothstep(0.035, 0.0, abs(d - 0.88)) * step(hp01, _HealthFrac) * step(_HealthFrac, 0.999);
                col += _Color.rgb * hpRing * 1.3;
                float alpha = max(body, hpRing);

                // Elite aura + shield bubble (same look as monsters)
                float auraOn = step(0.01, _AuraColor.a + _AuraColor.r + _AuraColor.g + _AuraColor.b);
                float flame = Fbm(float2(ang * 3.0 - t * 4.0, d * 6.0 - t * 3.0));
                float aura = smoothstep(0.98, 0.78, d) * smoothstep(R - 0.02, R + 0.1, d) * (0.4 + flame) * auraOn;
                col += _AuraColor.rgb * aura * 0.9;
                alpha = max(alpha, saturate(aura * 0.8));
                float bubble = (smoothstep(0.05, 0.0, abs(d - 0.82)) + smoothstep(0.82, 0.5, d) * 0.1) * _ShieldBubble;
                col += float3(0.5, 2.0, 3.2) * bubble;
                alpha = max(alpha, saturate(bubble));

                // Hit flash + dissolve
                col = lerp(col, float3(4, 4, 4), _Flash * body);
                float nz = Fbm(p * 3.0 + _Seed);
                float cut = nz - _Dissolve * 1.1 + 0.05;
                float edge = smoothstep(0.08, 0.0, cut) * step(0.0, cut) * step(0.001, _Dissolve);
                col += _EdgeColor.rgb * edge * 2.0;
                alpha *= step(0.0, cut);

                return half4(col, alpha * i.color.a);
            }
            ENDHLSL
        }
    }
}
