using UnityEngine;

namespace Soulbreak.Enemies
{
    // Per-enemy scratch state used by movement strategies.
    public struct MotionState
    {
        public bool Initialized;
        public float Direction;     // +1 / -1 orbit direction
        public float Travelled;     // radians travelled around the target
        public int Phase;
    }

    // Strategy pattern for enemy movement. EnemyConfig.movement picks one; null = straight line.
    // New movement = new subclass/asset, Enemy never changes.
    public abstract class MovementPattern : ScriptableObject
    {
        public abstract Vector2 Step(Enemy enemy, Vector2 position, Vector2 center, float speed, float dt, ref MotionState state);

        protected static void Init(ref MotionState s)
        {
            if (s.Initialized) return;
            s.Initialized = true;
            s.Direction = Random.value < 0.5f ? -1f : 1f;
        }

        // Moves around "center" by an angular step and inward by a radial step.
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
