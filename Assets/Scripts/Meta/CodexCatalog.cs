using Soulbreak.Enemies;
using Soulbreak.Perks;
using UnityEngine;

namespace Soulbreak.Meta
{
    [CreateAssetMenu(menuName = "Soulbreak/Codex Catalog", fileName = "CodexCatalog")]
    public class CodexCatalog : ScriptableObject
    {
        public EnemyConfig[] enemies;
        public PerkDefinition[] perks;

        public static string Describe(EnemyConfig c)
        {
            var sb = new System.Text.StringBuilder();
            if (c.isBoss) sb.Append("Boss. ");
            if (c.isBoss && c.minion != null) sb.Append($"Summons {Name(c.minion)}. ");
            if (c.gravityPull > 0f) sb.Append("Pulls enemies toward the core. ");
            if (c.stopDistance > 0f) sb.Append("Stops at range and shoots at the core. ");
            if (c.splitInto != null && c.splitCount > 0) sb.Append($"Splits into {c.splitCount} {Name(c.splitInto)} on death. ");
            if (c.movement != null) sb.Append($"Moves in a {c.movement.name.Replace("Movement", "").ToLower()} pattern. ");
            if (c.speed >= 2.5f) sb.Append("Very fast. ");
            else if (c.speed <= 0.9f) sb.Append("Slow but sturdy. ");
            if (sb.Length == 0) sb.Append("Charges straight at the core. ");
            return sb.ToString().Trim();
        }

        public static string Name(EnemyConfig c) => string.IsNullOrEmpty(c.displayName) ? c.name : c.displayName;
    }
}
