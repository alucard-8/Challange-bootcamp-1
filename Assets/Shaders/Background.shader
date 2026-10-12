// Animated arena floor: drifting soul nebula, faint grid and a glow toward the reactor.
// Uses world position, so it can be stretched over any size without distortion.
Shader "Soulbreak/Background"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite", 2D) = "white" {}
        _ColorA ("Deep Color", Color) = (0.02, 0.015, 0.05, 1)
        _ColorB ("Nebula Color", Color) = (0.18, 0.06, 0.32, 1)
        [HDR] _GridColor ("Grid Color", Color) = (0.4, 0.6, 1.2, 1)
        _GridSize ("Grid Size", Float) = 1
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
                half4 _ColorA;
                half4 _ColorB;
                half4 _GridColor;
                float _GridSize;
            CBUFFER_END

            half4 frag(Varyings i) : SV_Target
            {
                float2 w = i.positionWS.xy;
                float t = _Time.y;
                float dist = length(w);

                float neb = Fbm(w * 0.18 + float2(t * 0.02, -t * 0.015));
                neb *= Fbm(w * 0.35 - float2(t * 0.03, t * 0.01) + 7.0);
                float3 col = lerp(_ColorA.rgb, _ColorB.rgb, saturate(neb * 2.2));

                float2 g = abs(frac(w / _GridSize) - 0.5);
                float grid = smoothstep(0.015, 0.0, 0.5 - max(g.x, g.y));
                float ripple = pow(0.5 + 0.5 * sin(dist * 1.5 - t * 1.2), 4.0);
                col += _GridColor.rgb * grid * (0.015 + 0.06 * ripple) * saturate(1.1 - dist / 12.0);

                col += _ColorB.rgb * 0.6 * pow(saturate(1.0 - dist / 5.0), 2.0);
                return half4(col, 1.0);
            }
            ENDHLSL
        }
    }
}
