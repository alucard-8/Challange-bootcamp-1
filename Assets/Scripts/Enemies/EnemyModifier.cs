using System;
using UnityEngine;

namespace Soulbreak.Enemies
{
    [Flags]
    public enum EnemyModifier
    {
        None = 0,
        Shielded = 1,
        Swift = 2,
        Splitting = 4,
        Blink = 8,
    }

    public static class EnemyModifierInfo
    {
        public static readonly EnemyModifier[] All = { EnemyModifier.Shielded, EnemyModifier.Swift, EnemyModifier.Splitting, EnemyModifier.Blink };

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
