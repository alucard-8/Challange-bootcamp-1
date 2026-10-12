// UI panel for uGUI Images: rounded corners, glowing border with a light sweep running around it,
// soft inner glow, vertical gradient and faint scanlines. Pixel-size aware via fwidth, so corners
// stay round on any rect size. Fill color = the Image color.
Shader "Soulbreak/UIPanel"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _BorderColor ("Border Color", Color) = (0.45, 0.85, 1, 0.9)
        _Radius ("Corner Radius (px)", Float) = 14
        _BorderWidth ("Border Width (px)", Float) = 2
        _SweepSpeed ("Sweep Speed", Float) = 0.12
        _SweepStrength ("Sweep Strength", Float) = 1
        _Scanlines ("Scanlines", Range(0, 1)) = 0.12

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

            struct appdata { float4 vertex : POSITION; float4 color : COLOR; float2 uv : TEXCOORD0; };
            struct v2f { float4 pos : SV_POSITION; float4 color : COLOR; float2 uv : TEXCOORD0; };

            fixed4 _BorderColor;
            float _Radius, _BorderWidth, _SweepSpeed, _SweepStrength, _Scanlines;

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.color = v.color;
                // Canvas vertex colors arrive in gamma space; convert so dark colors stay dark in a linear project.
                #if !defined(UNITY_COLORSPACE_GAMMA)
                o.color.rgb = GammaToLinearSpace(v.color.rgb);
                #endif
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 fw = max(fwidth(i.uv), 1e-5);
                float2 size = 1.0 / fw;                       // rect size in pixels
                float2 p = (i.uv - 0.5) * size;               // pixel coords from the center

                float r = min(_Radius, min(size.x, size.y) * 0.5);
                float2 q = abs(p) - (size * 0.5 - r);
                float d = length(max(q, 0.0)) + min(max(q.x, q.y), 0.0) - r;   // rounded-rect SDF (px)

                float inside = saturate(0.5 - d);
                float border = saturate(_BorderWidth + 0.5 + d) * inside;

                float ang = atan2(p.y, p.x) / 6.2831853 + 0.5;
                float sweep = pow(frac(ang - _Time.y * _SweepSpeed), 14.0) * _SweepStrength;

                float grad = lerp(0.8, 1.2, i.uv.y);
                float scan = 1.0 - _Scanlines * step(0.5, frac(p.y * 0.25));
                float3 fill = i.color.rgb * grad * scan;

                float3 bcol = _BorderColor.rgb * (0.7 + sweep * 2.0);
                float3 col = lerp(fill, bcol, border);
                float innerGlow = exp(-max(-d, 0.0) / 22.0) * 0.35;
                col += _BorderColor.rgb * innerGlow * inside * _BorderColor.a;

                float a = inside * max(i.color.a, border * _BorderColor.a);
                return fixed4(col, a);
            }
            ENDCG
        }
    }
}
