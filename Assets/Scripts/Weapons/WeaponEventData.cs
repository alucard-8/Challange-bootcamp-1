using System.Collections.Generic;
using UnityEngine;

namespace Soulbreak.Weapons
{
    public struct ExplosionInfo
    {
        public Vector2 Position;
        public float Radius;
        public Color Color;
    }

    public struct ChainInfo
    {
        public List<Vector2> Points;
        public Color Color;
    }

    public struct BeamInfo
    {
        public Vector2 From, To;
        public float Width;
        public Color Color;
    }
}
