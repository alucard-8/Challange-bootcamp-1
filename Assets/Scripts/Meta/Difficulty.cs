using UnityEngine;

namespace Soulbreak.Meta
{
    public enum Difficulty { Easy, Normal, Hard, Nightmare }

    public struct DifficultyInfo
    {
        public string Name, Description;
        public Color Color;
        public float EnemyHealth, EnemySpeed, EnemyCount, EliteChance, CoreHealth, ScoreMultiplier;
    }

    public static class DifficultyTable
    {
        public static readonly Difficulty[] All = { Difficulty.Easy, Difficulty.Normal, Difficulty.Hard, Difficulty.Nightmare };

        public static DifficultyInfo Get(Difficulty d)
        {
            switch (d)
            {
                case Difficulty.Easy:
                    return new DifficultyInfo
                    {
                        Name = "EASY", Description = "Weaker, slower enemies.\nA sturdier core.\nScore x0.75",
                        Color = new Color(0.4f, 1f, 0.6f),
                        EnemyHealth = 0.7f, EnemySpeed = 0.85f, EnemyCount = 0.8f, EliteChance = 0.5f, CoreHealth = 1.5f, ScoreMultiplier = 0.75f,
                    };
                case Difficulty.Hard:
                    return new DifficultyInfo
                    {
                        Name = "HARD", Description = "Tougher, faster swarms.\nMore elites.\nScore x1.5",
                        Color = new Color(1f, 0.6f, 0.25f),
                        EnemyHealth = 1.35f, EnemySpeed = 1.15f, EnemyCount = 1.2f, EliteChance = 1.6f, CoreHealth = 0.8f, ScoreMultiplier = 1.5f,
                    };
                case Difficulty.Nightmare:
                    return new DifficultyInfo
                    {
                        Name = "NIGHTMARE", Description = "Relentless hordes.\nElites everywhere. Fragile core.\nScore x2.5",
                        Color = new Color(1f, 0.25f, 0.35f),
                        EnemyHealth = 1.8f, EnemySpeed = 1.3f, EnemyCount = 1.45f, EliteChance = 2.5f, CoreHealth = 0.6f, ScoreMultiplier = 2.5f,
                    };
                default:
                    return new DifficultyInfo
                    {
                        Name = "NORMAL", Description = "The intended experience.\n\nScore x1",
                        Color = new Color(0.45f, 0.85f, 1f),
                        EnemyHealth = 1f, EnemySpeed = 1f, EnemyCount = 1f, EliteChance = 1f, CoreHealth = 1f, ScoreMultiplier = 1f,
                    };
            }
        }

        public static DifficultyInfo Current => Get(MetaProgress.SelectedDifficulty);
    }
}
