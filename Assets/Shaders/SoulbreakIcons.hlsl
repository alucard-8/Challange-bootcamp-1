#ifndef SOULBREAK_ICONS_INCLUDED
#define SOULBREAK_ICONS_INCLUDED

// Procedural weapon-module icons, shared by the ring slots and the shop cards.
// q is in icon space (-1..1), x pointing "outward". Returns 0..1 coverage.
//  0 laser dot   1 triple shot   2 cannon      3 seekers
//  4 lightning   5 lance         6 blades      7 black hole

float IconCircle(float2 q, float2 c, float r) { return smoothstep(r, r - 0.06, length(q - c)); }

float IconSeg(float2 q, float2 a, float2 b, float w)
{
    float2 pa = q - a, ba = b - a;
    float h = saturate(dot(pa, ba) / dot(ba, ba));
    return smoothstep(w, w - 0.05, length(pa - ba * h));
}

float IconTri(float2 q, float2 c, float s)
{
    q = (q - c) / s;
    float inside = step(-0.5, q.x) * step(q.x, 0.5) * step(abs(q.y), (0.5 - q.x) * 0.75);
    float hole = step(-0.3, q.x) * step(q.x, 0.3) * step(abs(q.y), (0.3 - q.x) * 0.6);
    return inside * (1.0 - hole);
}

float SoulbreakIcon(float2 q, float id, float time)
{
    if (id < 0.5) return IconCircle(q, float2(0, 0), 0.12) * 0.5;
    if (id < 1.5) return saturate(IconCircle(q, float2(0.35, 0), 0.2) + IconCircle(q, float2(-0.25, 0.38), 0.2) + IconCircle(q, float2(-0.25, -0.38), 0.2));
    if (id < 2.5) return saturate(smoothstep(0.08, 0.0, abs(length(q) - 0.48)) + IconCircle(q, float2(0, 0), 0.18));
    if (id < 3.5) return saturate(IconTri(q, float2(0.0, 0.4), 0.7) + IconTri(q, float2(0.0, -0.4), 0.7));
    if (id < 4.5)   // lightning bolt
        return saturate(IconSeg(q, float2(-0.65, -0.45), float2(0.1, -0.05), 0.1) +
                        IconSeg(q, float2(0.1, -0.05), float2(-0.1, 0.08), 0.1) +
                        IconSeg(q, float2(-0.1, 0.08), float2(0.65, 0.45), 0.1));
    if (id < 5.5)   // lance: shaft + arrow head
        return saturate(IconSeg(q, float2(-0.75, 0), float2(0.35, 0), 0.07) +
                        IconSeg(q, float2(0.75, 0), float2(0.3, 0.3), 0.08) +
                        IconSeg(q, float2(0.75, 0), float2(0.3, -0.3), 0.08));
    if (id < 6.5)   // orbiting blades: two spinning crescents
    {
        float a = atan2(q.y, q.x) + time * 3.0;
        float arc = smoothstep(0.09, 0.0, abs(length(q) - 0.5));
        float sectors = step(0.0, sin(a)) * smoothstep(0.0, 0.6, frac(a / 3.14159265));
        return saturate(arc * sectors + IconCircle(q, float2(0, 0), 0.12));
    }
    // black hole: spiral arms around a dark core
    float d = length(q);
    float arms = smoothstep(0.35, 0.0, abs(frac(atan2(q.y, q.x) / 6.2831853 * 2.0 + d * 1.4 - time * 0.5) - 0.5) - 0.15);
    return saturate(arms * smoothstep(0.85, 0.6, d) * smoothstep(0.15, 0.25, d) + smoothstep(0.08, 0.0, abs(d - 0.2)));
}

#endif
