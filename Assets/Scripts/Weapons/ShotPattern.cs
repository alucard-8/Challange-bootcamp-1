using Soulbreak.Player;
using UnityEngine;

namespace Soulbreak.Weapons
{
    // Everything a shot needs to know when it's fired.
    public struct ShotContext
    {
        public Vector2 Origin;      // reactor
        public Vector2 Target;      // cursor in world space
        public Vector2 Direction;   // normalized Origin -> Target
        public int Damage;
        public bool Crit;
        public PlayerStats Stats;
    }

    // Strategy pattern: each weapon module is one "how to fire" strategy.
    // The shooter and the ring never switch on weapon types — they just call Fire().
    public abstract class ShotPattern : ScriptableObject
    {
        public string displayName = "Shot";
        [Tooltip("Icon drawn by the ring shader: 0 none, 1 three dots, 2 big circle, 3 triangles")]
        public int icon;
        [ColorUsage(true, true)] public Color color = Color.cyan;
        [Tooltip("Draw the instant laser tracer for this shot")]
        public bool drawsTracer;
        [Tooltip("Seconds for an emptied ring slot to load a new module after this one is fired")]
        [Min(0.1f)] public float reloadTime = 3f;

        public abstract void Fire(ShotContext context);
    }
}
