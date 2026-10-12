// One slot of the weapon ring: an arc between _Start and _End (radians) with a glowing border,
// a fill that lights up when aimed at (_Active) and a procedural icon for the module type.
Shader "Soulbreak/RingSegment"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite", 2D) = "white" {}
        [HDR] _Color ("Color", Color) = (0.5, 2.5, 1.2, 1)
        _Start ("Start Angle", Float) = 0
        _End ("End Angle", Float) = 0.785
        _Inner ("Inner Radius", Range(0, 1)) = 0.66
        _Outer ("Outer Radius", Range(0, 1)) = 0.94
        _Icon ("Icon", Float) = 0
        _Active ("Active", Range(0, 1)) = 0
        _Empty ("Empty", Range(0, 1)) = 0
        _Loading ("Loading", Range(0, 1)) = 0
        _LoadProgress ("Load Progress", Range(0, 1)) = 1
        _Pop ("Loaded Pop", Range(0, 1)) = 0
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
                float _Start;
                float _End;
                float _Inner;
                float _Outer;
                float _Icon;
                float _Active;
                float _Empty;
                float _Loading;
                float _LoadProgress;
                float _Pop;
            CBUFFER_END

            #include "SoulbreakIcons.hlsl"
            #define TAU 6.2831853

            float Icon(float2 q) { return SoulbreakIcon(q, _Icon, _Time.y); }

            half4 frag(Varyings i) : SV_Target
            {
                float2 p = Centered(i.uv);
                float d = length(p);
                float ang = atan2(p.y, p.x);
                if (ang < 0.0) ang += TAU;

                float span = _End - _Start;
                float a = fmod(ang - _Start + TAU, TAU);          // angle inside this slot
                float arc = min(a, span - a) * d;                  // distance to the side edges (world-ish)
                float inSlot = step(a, span);

                float radial = min(d - _Inner, _Outer - d);       // distance to inner/outer edges
                float edgeDist = min(arc - 0.02, radial);         // 0.02 = gap between slots
                float shape = smoothstep(0.0, 0.008, edgeDist) * inSlot;
                float border = shape * smoothstep(0.03, 0.012, edgeDist);

                // Icon in local slot space: x = radial, y = tangential, about -1..1
                float mid = _Start + span * 0.5;
                float c = cos(-mid), s = sin(-mid);
                float2 q = float2(p.x * c - p.y * s, p.x * s + p.y * c);
                float rMid = (_Inner + _Outer) * 0.5;
                float2 local = (q - float2(rMid, 0)) / ((_Outer - _Inner) * 0.5);
                float icon = Icon(local) * shape;

                float pulse = 0.85 + 0.15 * sin(_Time.y * 8.0);
                float fill = lerp(0.12, 0.45 * pulse, _Active) * lerp(1.0, 0.5, _Empty);
                float borderBoost = lerp(0.7, 1.6, _Active) * lerp(1.0, 0.6, _Empty);

                // Loading: the slot fills up along its arc like a progress bar, with a bright leading edge.
                float swept = a / max(span, 0.0001);
                float sweep = step(swept, _LoadProgress) * _Loading;
                float leading = smoothstep(0.06, 0.0, abs(swept - _LoadProgress)) * _Loading;
                fill = lerp(fill, 0.08 + sweep * 0.35, _Loading);
                borderBoost *= lerp(1.0, 0.5 + 0.3 * sin(_Time.y * 10.0), _Loading);

                // Pop: flash when a new module finishes loading.
                float popGlow = _Pop * _Pop;

                float3 col = _Color.rgb * (fill + border * borderBoost + icon * lerp(1.0, 2.0, _Active) + leading * 1.5);
                col += popGlow * (_Color.rgb * 1.5 + 1.0);
                float alpha = saturate(fill + border * 0.9 + icon + leading + popGlow) * shape;
                return half4(col, alpha * i.color.a);
            }
            ENDHLSL
        }
    }
}
