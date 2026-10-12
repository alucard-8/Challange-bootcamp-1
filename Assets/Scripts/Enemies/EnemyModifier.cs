using System;
using UnityEngine;

namespace Soulbreak.Enemies
{
    // Elite modifiers rolled onto enemies in later waves.
    [Flags]
    public enum EnemyModifier
    {
        None = 0,
        Shielded = 1,    // absorbs the first few hits
        Swift = 2,       // much faster
        Splitting = 4,   // bursts into extra enemies on death
        Blink = 8,       // teleports toward the reactor
    }

    public static class EnemyModifierInfo
    {
        public static readonly EnemyModifier[] All = { EnemyModifier.Shielded, EnemyModifier.Swift, EnemyModifier.Splitting, EnemyModifier.Blink };

        // Aura color drawn by the enemy shader (HDR).
        public static Color AuraColor(EnemyModifier m)
        {
            if ((m & EnemyModifier.Shielded) != 0) return new Color(0.5f, 2.2f, 3.5f);
            if ((m & EnemyModifier.Blink) != 0) return new Color(2.4f, 0.6f, 3.5f);
            if ((m & EnemyModifier.Swift) != 0) return new Color(3.5f, 2.6f, 0.4f);
            if ((m & EnemyModifier.Splitting) != 0) return new Color(0.6f, 3.2f, 1f);
            return Color.clear;
        }
    }
}
