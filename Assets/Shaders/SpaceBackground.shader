// Deep space for Star Core mode: three parallax layers of twinkling stars, faint colorful nebulae,
// a distant galaxy band and a warm glow around the star at the center.
Shader "Soulbreak/SpaceBackground"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite", 2D) = "white" {}
        _Deep ("Deep Color", Color) = (0.005, 0.008, 0.02, 1)
        _NebulaA ("Nebula A", Color) = (0.25, 0.05, 0.3, 1)
        _NebulaB ("Nebula B", Color) = (0.03, 0.15, 0.3, 1)
        [HDR] _StarGlow ("Star Glow", Color) = (0.6, 0.3, 0.08, 1)
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
                half4 _Deep;
                half4 _NebulaA;
                half4 _NebulaB;
                half4 _StarGlow;
            CBUFFER_END

            float StarLayer(float2 w, float density, float size, float t)
            {
                float2 cell = floor(w * density);
                float2 f = frac(w * density);
                float h = Hash21(cell);
                float2 pos = float2(Hash21(cell + 1.7), Hash21(cell + 9.2)) * 0.8 + 0.1;
                float star = smoothstep(size, 0.0, length(f - pos)) * step(0.82, h);
                float twinkle = 0.6 + 0.4 * sin(t * (2.0 + h * 5.0) + h * 40.0);
                return star * twinkle * (0.5 + h);
            }

            half4 frag(Varyings i) : SV_Target
            {
                float2 w = i.positionWS.xy;
                float t = _Time.y;
                float dist = length(w);

                float3 col = _Deep.rgb;

                // Nebulae
                float na = Fbm(w * 0.12 + float2(t * 0.01, 0.0));
                float nb = Fbm(w * 0.09 - float2(0.0, t * 0.008) + 11.0);
                col += _NebulaA.rgb * pow(na, 3.0) * 1.6 + _NebulaB.rgb * pow(nb, 3.0) * 1.6;

                // Galaxy band across the sky
                float band = exp(-abs(w.y * 0.9 + w.x * 0.35 + 1.5) * 0.35) * Fbm(w * 0.4);
                col += float3(0.25, 0.22, 0.35) * band * 0.5;

                // Parallax star layers (slow drift so it feels alive)
                float s = 0.0;
                s += StarLayer(w + float2(t * 0.02, 0), 1.6, 0.06, t) * 0.6;
                s += StarLayer(w * 0.7 + float2(t * 0.01, 5.0), 2.4, 0.05, t) * 0.8;
                s += StarLayer(w * 0.45 + 3.0, 4.0, 0.06, t) * 0.5;
                col += float3(0.9, 0.95, 1.0) * s;

                // Warm light around the central star
                col += _StarGlow.rgb * pow(saturate(1.0 - dist / 7.0), 2.5);
                return half4(col, 1.0);
            }
            ENDHLSL
        }
    }
}
