using UnityEngine;

namespace Soulbreak.Enemies
{
    // Planets: spiral inward on a decaying orbit. Ranged enemies hold their orbit once in range.
    [CreateAssetMenu(menuName = "Soulbreak/Movement/Spiral", fileName = "SpiralMovement")]
    public class SpiralMovement : MovementPattern
    {
        [Tooltip("Sideways (orbital) speed relative to the enemy speed")]
        public float tangential = 1.1f;
        [Tooltip("Inward speed relative to the enemy speed")]
        public float radial = 0.5f;

        public override Vector2 Step(Enemy enemy, Vector2 position, Vector2 center, float speed, float dt, ref MotionState state)
        {
            Init(ref state);
            float inward = enemy.InRange ? 0f : speed * radial * dt;
            return Orbit(position, center, speed * tangential * dt, inward, ref state);
        }
    }
}
