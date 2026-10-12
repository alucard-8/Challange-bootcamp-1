// Expanding shockwave ring for impacts, kills and the Nova blast. _Progress goes 0 -> 1.
Shader "Soulbreak/Ring"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite", 2D) = "white" {}
        [HDR] _Color ("Color", Color) = (2, 2, 2, 1)
        _Progress ("Progress", Range(0, 1)) = 0
        _Thickness ("Thickness", Range(0.01, 0.5)) = 0.15
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
                half4 _Color;
                float _Progress;
                float _Thickness;
            CBUFFER_END

            half4 frag(Varyings i) : SV_Target
            {
                float2 p = Centered(i.uv);
                float d = length(p);
                float ease = 1.0 - pow(1.0 - _Progress, 3.0);      // ease-out
                float radius = ease * 0.95;
                float thick = lerp(_Thickness, 0.01, _Progress);
                float ring = smoothstep(thick, 0.0, abs(d - radius));
                float fill = smoothstep(radius, 0.0, d) * 0.15 * (1.0 - _Progress);
                float fade = 1.0 - _Progress;

                float3 col = _Color.rgb * i.color.rgb * (ring * 2.0 + fill);
                return half4(col, saturate(ring + fill) * fade * i.color.a);
            }
            ENDHLSL
        }
    }
}
