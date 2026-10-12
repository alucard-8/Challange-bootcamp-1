// Glowing pulsing orb with a swirling ring. Additive + HDR color so bloom makes it glow.
// Color comes from the SpriteRenderer color (vertex color), so one material serves every orb.
Shader "Soulbreak/Orb"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite", 2D) = "white" {}
        _Intensity ("Intensity", Float) = 1.5
        _PulseSpeed ("Pulse Speed", Float) = 6
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "RenderPipeline"="UniversalPipeline" "IgnoreProjector"="True" }
        Blend SrcAlpha One
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
                float _Intensity;
                float _PulseSpeed;
            CBUFFER_END

            half4 frag(Varyings i) : SV_Target
            {
                float2 p = Centered(i.uv);
                float d = length(p);
                float t = _Time.y;
                float seed = i.positionWS.x * 1.7 + i.positionWS.y * 3.1;   // de-sync neighbours

                float pulse = 0.5 + 0.5 * sin(t * _PulseSpeed + seed);
                float core = smoothstep(0.32 + 0.06 * pulse, 0.0, d);
                float glow = pow(saturate(1.0 - d), 3.0);

                float ang = atan2(p.y, p.x);
                float swirl = 0.5 + 0.5 * sin(ang * 3.0 + t * 5.0 - d * 12.0);
                float ringR = 0.6 + 0.08 * pulse;
                float ring = smoothstep(0.09, 0.0, abs(d - ringR)) * swirl;

                float3 col = i.color.rgb * (core * 2.5 + glow * 1.2 + ring * 1.5) * _Intensity;
                col += core * 0.8;   // white-hot center
                float a = saturate(core + glow + ring) * i.color.a;
                return half4(col, a);
            }
            ENDHLSL
        }
    }
}
