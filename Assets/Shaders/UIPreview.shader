// Animated previews for Ascension cards (uGUI). Square Image, procedural content by _Kind:
//   0 = reactor skin (mini core in _Color with a shield ring in _Color2)
//   1 = technique   (the module's ring icon glowing in _Color)
//   2 = star        (mini burning star with corona)
//   3 = locked      (dim pulsing mystery orb)
Shader "Soulbreak/UIPreview"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        [HDR] _Color ("Color", Color) = (0.6, 1.6, 3, 1)
        [HDR] _Color2 ("Color 2", Color) = (0.5, 2.5, 3, 1)
        _Kind ("Kind", Float) = 0
        _Icon ("Icon", Float) = 0

        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane" }
        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }
        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            #include "SoulbreakIcons.hlsl"

            struct appdata { float4 vertex : POSITION; float4 color : COLOR; float2 uv : TEXCOORD0; };
            struct v2f { float4 pos : SV_POSITION; float4 color : COLOR; float2 uv : TEXCOORD0; };

            float4 _Color, _Color2;
            float _Kind, _Icon;

            float H(float2 p) { p = frac(p * float2(123.34, 456.21)); p += dot(p, p + 45.32); return frac(p.x * p.y); }
            float N(float2 p)
            {
                float2 i = floor(p), f = frac(p);
                float2 u = f * f * (3.0 - 2.0 * f);
                return lerp(lerp(H(i), H(i + float2(1, 0)), u.x), lerp(H(i + float2(0, 1)), H(i + 1), u.x), u.y);
            }
            float F(float2 p) { return 0.5 * N(p) + 0.25 * N(p * 2.03) + 0.125 * N(p * 4.01); }

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.color = v.color;
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 p = i.uv * 2.0 - 1.0;
                float d = length(p);
                float ang = atan2(p.y, p.x);
                float t = _Time.y;
                float3 col = 0;
                float a = 0;

                if (_Kind < 0.5)   // ---- reactor skin
                {
                    float core = smoothstep(0.42, 0.39, d);
                    float sw = F(float2(ang * 2.0 + t * 0.8 + d * 3.0, d * 5.0 - t * 1.5));
                    float hot = pow(saturate(1.0 - d / 0.42), 3.0);
                    col += _Color.rgb * (0.3 + sw + hot * 1.2) * core * 0.6;
                    float shell = smoothstep(0.03, 0.0, abs(d - 0.42));
                    float seg = frac((ang / 6.2831853 + 0.5) * 3.0);
                    float ring = smoothstep(0.05, 0.0, abs(d - 0.72)) * smoothstep(0.0, 0.06, seg) * smoothstep(1.0, 0.94, seg);
                    float aura = pow(saturate(1.0 - d), 4.0) * 0.6;
                    col += _Color.rgb * (shell * 0.8 + aura * 0.4) + _Color2.rgb * ring * (0.6 + 0.3 * sin(ang * 6.0 - t * 4.0));
                    a = saturate(core + shell + ring + aura);
                }
                else if (_Kind < 1.5)   // ---- technique icon
                {
                    float2 q = float2(p.y, -p.x) * float2(-1, 1) * 1.35;   // rotate so "outward" points up
                    float icon = SoulbreakIcon(q, _Icon, t);
                    float halo = smoothstep(0.85, 0.2, d) * 0.25 * (0.8 + 0.2 * sin(t * 3.0));
                    float ring = smoothstep(0.03, 0.0, abs(d - 0.86)) * 0.6;
                    col = _Color.rgb * (icon * 1.2 + halo + ring) * 0.55 + icon * 0.15;
                    a = saturate(icon + halo + ring);
                }
                else if (_Kind < 2.5)   // ---- star
                {
                    float R = 0.45;
                    float body = smoothstep(R, R - 0.02, d);
                    float2 sp = p / R;
                    float gran = F(sp * 4.0 + float2(t * 0.25, -t * 0.2));
                    float limb = sqrt(saturate(1.0 - (d / R) * (d / R)));
                    col += lerp(_Color.rgb * 0.45, _Color.rgb * 0.9 + 0.5, gran * gran * limb) * (0.35 + 0.5 * limb) * body;
                    float rays = F(float2((ang / 6.2831853 + 0.5) * 20.0, t * 0.6));
                    float corona = exp(-(d - R) * 7.0) * (0.3 + rays) * step(R, d);
                    col += _Color.rgb * corona * 0.45;
                    float aurora = smoothstep(0.04, 0.0, abs(d - (0.78 + 0.02 * sin(ang * 7.0 + t * 2.5))));
                    col += _Color2.rgb * aurora * 0.5;
                    a = saturate(body + corona + aurora);
                }
                else   // ---- locked mystery
                {
                    float pulse = 0.5 + 0.5 * sin(t * 2.0);
                    float swirl = F(float2(ang * 3.0 - t * 0.7, d * 4.0 + t * 0.4));
                    float orb = smoothstep(0.5, 0.2, d) * (0.3 + swirl * 0.7);
                    float ring = smoothstep(0.03, 0.0, abs(d - (0.62 + 0.05 * pulse)));
                    col = float3(0.35, 0.3, 0.5) * orb + float3(0.6, 0.5, 0.9) * ring * (0.4 + 0.6 * pulse);
                    a = saturate(orb + ring * 0.8);
                }

                return fixed4(col, a * i.color.a);
            }
            ENDCG
        }
    }
}
