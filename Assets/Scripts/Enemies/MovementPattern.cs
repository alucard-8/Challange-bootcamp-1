using UnityEngine;

namespace Soulbreak.Enemies
{
    public struct MotionState
    {
        public bool Initialized;
        public float Direction;
        public float Travelled;
        public int Phase;
    }

    public abstract class MovementPattern : ScriptableObject
    {
        public abstract Vector2 Step(Enemy enemy, Vector2 position, Vector2 center, float speed, float dt, ref MotionState state);

        protected static void Init(ref MotionState s)
        {
            if (s.Initialized) return;
            s.Initialized = true;
            s.Direction = Random.value < 0.5f ? -1f : 1f;
        }

        protected static Vector2 Orbit(Vector2 position, Vector2 center, float tangentialDistance, float radialDistance,
                                       ref MotionState s, float minRadius = 0.1f)
        {
            Vector2 rel = position - center;
            float r = Mathf.Max(0.5f, rel.magnitude);
            float angle = Mathf.Atan2(rel.y, rel.x);
            float dAngle = s.Direction * tangentialDistance / r;
            s.Travelled += Mathf.Abs(dAngle);
            angle += dAngle;
            float newR = Mathf.Max(minRadius, rel.magnitude - radialDistance);
            return center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * newR;
        }
    }
}
