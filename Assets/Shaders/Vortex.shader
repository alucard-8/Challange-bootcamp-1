// Black hole: dark event-horizon core, bright accretion ring and spiral arms being sucked in.
// _Color (HDR) and _Power (0..1, grows while active) come from a MaterialPropertyBlock.
Shader "Soulbreak/Vortex"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite", 2D) = "white" {}
        [HDR] _Color ("Color", Color) = (2.2, 0.6, 4.5, 1)
        _Power ("Power", Range(0, 1)) = 1
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
                float _Power;
            CBUFFER_END

            half4 frag(Varyings i) : SV_Target
            {
                float2 p = Centered(i.uv);
                float d = length(p);
                float ang = atan2(p.y, p.x);
                float t = _Time.y;

                float arms = pow(0.5 + 0.5 * sin(ang * 3.0 + d * 14.0 - t * 9.0), 5.0);
                arms *= smoothstep(1.0, 0.35, d) * smoothstep(0.18, 0.4, d);
                float dust = Fbm(float2(ang * 2.0 + t * 2.5 - d * 4.0, d * 6.0)) * smoothstep(1.0, 0.4, d) * smoothstep(0.2, 0.45, d);

                float ringR = 0.3;
                float ring = smoothstep(0.07, 0.0, abs(d - ringR)) * (0.8 + 0.4 * sin(ang * 6.0 - t * 12.0));
                float core = smoothstep(ringR, ringR - 0.04, d);

                float3 col = _Color.rgb * (arms * 1.4 + dust * 0.8 + ring * 3.0) * (0.4 + 0.6 * _Power);
                col *= 1.0 - core;
                float alpha = saturate(core + arms + dust * 0.7 + ring) * i.color.a;
                return half4(col, alpha);
            }
            ENDHLSL
        }
    }
}
