// Shop card. Same rounded glowing frame as UIPanel, plus a style-specific animated face:
//   _Style 0 = BUFF      : holographic diagonal sheen + sparkles drifting upward + warm bottom glow
//   _Style 1 = TECHNIQUE : scrolling circuit grid with data pulses, crackling electric border.
// Both share a card face: art window (category sigil or module icon), rarity gems, divider,
// corner brackets; epic+ get a running border dash, legendary a rainbow foil edge.
// _Accent is the rarity color; fill = Image color.
Shader "Soulbreak/UICard"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Accent ("Accent", Color) = (0.45, 0.85, 1, 1)
        _Style ("Style (0 buff, 1 technique)", Float) = 0
        _Icon ("Icon", Float) = 0
        _Radius ("Corner Radius (px)", Float) = 18
        _BorderWidth ("Border Width (px)", Float) = 2
        _Dim ("Dim (sold)", Range(0, 1)) = 0
        _Rarity ("Rarity (0-3)", Float) = 0
        _Category ("Category (0-4)", Float) = 0
        _Hover ("Hover", Range(0, 1)) = 0

        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane" "CanUseSpriteAtlas"="True" }

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

            fixed4 _Accent;
            float _Style, _Icon, _Radius, _BorderWidth, _Dim, _Rarity, _Category, _Hover;

            float Ring(float2 q, float r, float w) { return smoothstep(w, w * 0.4, abs(length(q) - r)); }
            float Seg(float2 q, float2 a, float2 b, float w)
            {
                float2 pa = q - a, ba = b - a;
                float h = saturate(dot(pa, ba) / dot(ba, ba));
                return smoothstep(w, w * 0.4, length(pa - ba * h));
            }

            // Buff sigils, one per category, drawn in a unit box (q in -1..1).
            float Sigil(float2 q, float cat, float t)
            {
                float v = 0.0;
                if (cat < 0.5)        // OFFENSE: crosshair
                {
                    v = Ring(q, 0.55, 0.06) + Ring(q, 0.12, 0.05);
                    v += Seg(q, float2(0, 0.3), float2(0, 0.95), 0.06) + Seg(q, float2(0, -0.3), float2(0, -0.95), 0.06);
                    v += Seg(q, float2(0.3, 0), float2(0.95, 0), 0.06) + Seg(q, float2(-0.3, 0), float2(-0.95, 0), 0.06);
                }
                else if (cat < 1.5)   // ECONOMY: faceted soul gem
                {
                    float dm = abs(q.x) + abs(q.y * 0.8);
                    v = smoothstep(0.06, 0.02, abs(dm - 0.8)) + smoothstep(0.42, 0.38, dm) * 0.6;
                    v += Seg(q, float2(-0.8, 0), float2(0.8, 0), 0.04) * 0.7;
                }
                else if (cat < 2.5)   // DEFENSE: hex shield
                {
                    float2 a = abs(q);
                    float hex = max(a.x * 0.866 + a.y * 0.5, a.y);
                    v = smoothstep(0.07, 0.02, abs(hex - 0.8)) + smoothstep(0.06, 0.02, abs(hex - 0.5)) * 0.6;
                }
                else if (cat < 3.5)   // ABILITY: four-point star
                {
                    float2 a = abs(q);
                    float star = pow(a.x, 0.5) + pow(a.y, 0.5);
                    v = smoothstep(1.02, 0.95, star) * (0.6 + 0.4 * sin(t * 4.0)) + Ring(q, 0.75, 0.04) * 0.5;
                }
                else                  // WEAPON: up-chevrons
                {
                    float2 r = float2(abs(q.x), q.y);
                    for (int k = 0; k < 3; k++)
                    {
                        float y = 0.55 - k * 0.5 + frac(t * 0.6) * 0.1;
                        v += Seg(r, float2(0, y), float2(0.7, y - 0.45), 0.08) * (1.0 - k * 0.25);
                    }
                }
                return saturate(v);
            }

            float Hash(float2 p) { p = frac(p * float2(123.34, 456.21)); p += dot(p, p + 45.32); return frac(p.x * p.y); }

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.color = v.color;
                #if !defined(UNITY_COLORSPACE_GAMMA)
                o.color.rgb = GammaToLinearSpace(v.color.rgb);
                #endif
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float t = _Time.y;
                float2 fw = max(fwidth(i.uv), 1e-5);
                float2 size = 1.0 / fw;
                float2 p = (i.uv - 0.5) * size;

                float r = min(_Radius, min(size.x, size.y) * 0.5);
                float2 q = abs(p) - (size * 0.5 - r);
                float d = length(max(q, 0.0)) + min(max(q.x, q.y), 0.0) - r;
                float inside = saturate(0.5 - d);

                float3 accent = _Accent.rgb;
                float3 col = i.color.rgb * lerp(0.75, 1.25, i.uv.y);
                float bw = _BorderWidth;
                float3 borderCol = accent;

                if (_Style < 0.5)
                {
                    // ---------- BUFF: holographic sheen + rising sparkles ----------
                    float diag = (i.uv.x + i.uv.y) * 0.5;
                    float sweepPos = frac(t * 0.25) * 2.2 - 0.6;
                    float sheen = smoothstep(0.12, 0.0, abs(diag - sweepPos));
                    float3 holo = 0.5 + 0.5 * cos(6.2831 * (diag * 1.5 + t * 0.1 + float3(0.0, 0.33, 0.67)));
                    col += sheen * lerp(accent, holo, 0.5) * 0.35;

                    float2 cellP = (p + float2(0.0, -t * 40.0)) / 34.0;
                    float2 cell = floor(cellP);
                    float h = Hash(cell);
                    float2 spot = float2(Hash(cell + 7.1), Hash(cell + 3.7)) * 0.7 + 0.15;
                    float sparkle = smoothstep(0.12, 0.0, length(frac(cellP) - spot)) * step(0.72, h);
                    sparkle *= 0.5 + 0.5 * sin(t * 6.0 + h * 40.0);
                    sparkle *= smoothstep(1.0, 0.3, i.uv.y);
                    col += accent * sparkle * 0.9;

                    col += accent * pow(saturate(1.0 - i.uv.y), 3.0) * 0.35;   // warm glow from below
                }
                else
                {
                    // ---------- TECHNIQUE: circuit grid + data pulses + big icon + electric border ----------
                    float2 g = p / 26.0 + float2(0.0, t * 0.6);
                    float2 gl = abs(frac(g) - 0.5);
                    float line_ = smoothstep(0.04, 0.0, min(gl.x, gl.y) - 0.0);
                    float2 gc = floor(g);
                    float lit = step(0.6, Hash(gc + floor(t * 2.0) * 0.01));
                    float pulse = smoothstep(0.15, 0.0, abs(frac(g.y * 0.25 - t * 0.8 + Hash(float2(gc.x, 0))) - 0.5));
                    col += accent * line_ * (0.06 + 0.25 * lit * pulse);


                    float scan = smoothstep(6.0, 0.0, abs(p.y - (frac(t * 0.35) * 2.0 - 1.0) * size.y * 0.6));
                    col += accent * scan * 0.12;

                    float n = Hash(float2(floor(atan2(p.y, p.x) * 20.0), floor(t * 18.0)));
                    bw += n * 2.5;
                    borderCol = accent * (1.0 + n * 1.5);
                }

                // ---------- Card face: art window, sigil, rarity gems, divider ----------
                float top = size.y * 0.5;
                float2 wc = float2(0.0, top - 116.0);
                float2 wh = float2(size.x * 0.5 - 18.0, 64.0);
                float2 wq = abs(p - wc) - (wh - 10.0);
                float wd = length(max(wq, 0.0)) + min(max(wq.x, wq.y), 0.0) - 10.0;
                float win = saturate(0.5 - wd);
                float2 wuv = (p - wc) / wh;
                // window: darker inset with a radial glow of the accent behind the sigil
                col = lerp(col, col * 0.3 + accent * 0.12 * exp(-dot(wuv, wuv) * 2.0), win);
                float rays = pow(saturate(0.5 + 0.5 * sin(atan2(wuv.y, wuv.x) * 8.0 + t * 0.8)), 6.0) * exp(-length(wuv) * 1.8);
                col += accent * rays * 0.15 * win;
                float2 sq = (p - wc) / 50.0;
                float art = _Style < 0.5 ? Sigil(sq, _Category, t) : SoulbreakIcon(float2(sq.y, -sq.x) * float2(-1, 1) * 1.3, _Icon, t);
                col += accent * art * (1.3 + 0.3 * sin(t * 2.0)) * win;
                col += accent * smoothstep(2.0, 0.0, abs(wd)) * 0.9;             // window frame line

                // rarity gems under the window: filled = rarity + 1
                for (int gi = 0; gi < 4; gi++)
                {
                    float2 gp = p - float2((gi - 1.5) * 20.0, top - 194.0);
                    float gd = abs(gp.x) + abs(gp.y);
                    float filled = step((float)gi, _Rarity + 0.5);
                    col += accent * smoothstep(7.5, 6.0, gd) * lerp(0.15, 1.6, filled);
                }

                // ornament divider between name and description
                float dy = abs(p.y - (top - 254.0));
                float dline = smoothstep(1.5, 0.0, dy) * smoothstep(size.x * 0.4, size.x * 0.1, abs(p.x));
                float ddia = smoothstep(6.0, 4.5, abs(p.x) + dy);
                col += accent * (dline * 0.6 + ddia * 1.2);

                // corner brackets
                float2 cq = abs(p) - (size * 0.5 - 12.0);
                float brk = (step(abs(cq.x), 1.2) * step(-22.0, cq.y) * step(cq.y, 0.0))
                          + (step(abs(cq.y), 1.2) * step(-22.0, cq.x) * step(cq.x, 0.0));
                col += accent * saturate(brk) * 0.9;

                // rarity border: epic+ get a bright dash running around the edge, legendary is rainbow foil
                float ang = atan2(p.y, p.x) / 6.2831 + 0.5;
                if (_Rarity > 1.5)
                {
                    float dash = smoothstep(0.08, 0.0, abs(frac(ang - t * 0.25) - 0.5) - 0.42);
                    borderCol += float3(1, 1, 1) * dash * 1.5;
                    bw += dash * 1.5;
                }
                if (_Rarity > 2.5)
                    borderCol = lerp(borderCol, 0.6 + 0.6 * cos(6.2831 * (ang * 2.0 + t * 0.3 + float3(0.0, 0.33, 0.67))), 0.55) * 1.4;

                // hover: thicker, brighter edge and a lifted face
                bw += _Hover * 1.5;
                borderCol *= 1.0 + _Hover * 0.6;
                col *= 1.0 + _Hover * 0.08;

                float border = saturate(bw + 0.5 + d) * inside;
                float edgeGlow = exp(-max(-d, 0.0) / 20.0) * (0.35 + _Hover * 0.4);
                col = lerp(col + accent * edgeGlow, borderCol * 1.2, border);
                col = lerp(col, dot(col, float3(0.3, 0.59, 0.11)) * 0.5, _Dim);

                float a = inside * max(i.color.a, border);
                return fixed4(col, a);
            }
            ENDCG
        }
    }
}
