// Laser tracer for shots (used on a LineRenderer). Bright core, soft falloff, fades via vertex alpha.
Shader "Soulbreak/Tracer"
{
    Properties
    {
        [HDR] _Color ("Color", Color) = (1.5, 2.5, 4, 1)
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
            CBUFFER_END

            half4 frag(Varyings i) : SV_Target
            {
                float across = abs(i.uv.y * 2.0 - 1.0);
                float beam = pow(saturate(1.0 - across), 2.5);
                float core = smoothstep(0.3, 0.0, across);
                // brighter toward the impact end
                float along = lerp(0.4, 1.0, i.uv.x);
                float3 col = (_Color.rgb * i.color.rgb * beam + core * 2.0) * along;
                return half4(col, beam * i.color.a);
            }
            ENDHLSL
        }
    }
}
