using UnityEngine;

namespace Soulbreak.Enemies
{
    [CreateAssetMenu(menuName = "Soulbreak/Movement/Comet", fileName = "CometMovement")]
    public class CometMovement : MovementPattern
    {
        public float slingshotTangential = 2.4f;
        public float slingshotRadial = 0.35f;
        [Tooltip("How far around the target (in turns) before diving")]
        public float turnsBeforeDive = 1f;
        public float diveSpeedMultiplier = 2.2f;

        public override Vector2 Step(Enemy enemy, Vector2 position, Vector2 center, float speed, float dt, ref MotionState state)
        {
            Init(ref state);
            if (state.Phase == 0)
            {
                var next = Orbit(position, center, speed * slingshotTangential * dt, speed * slingshotRadial * dt, ref state, 2.4f);
                if (state.Travelled >= turnsBeforeDive * Mathf.PI * 2f) state.Phase = 1;
                return next;
            }
            return Vector2.MoveTowards(position, center, speed * diveSpeedMultiplier * dt);
        }
    }
}
