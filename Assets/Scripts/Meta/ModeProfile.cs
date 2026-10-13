using System;
using Soulbreak.Enemies;
using Soulbreak.Perks;
using UnityEngine;

namespace Soulbreak.Meta
{
    [Serializable]
    public struct LexiconEntry
    {
        public string key;
        public string text;
    }

    [CreateAssetMenu(menuName = "Soulbreak/Mode Profile", fileName = "ModeProfile")]
    public class ModeProfile : ScriptableObject
    {
        public string id = "reactor";
        public string displayName = "Soul Reactor";
        public string title = "SOULBREAK";
        public string tagline = "THE  REACTOR  MUST  NOT  FALL";

        [Header("Enemies")]
        public WaveEnemy[] roster;
        public EnemyConfig boss;
        public EnemyConfig eliteSplitInto;
        public EnemyConfig rejuvenator;

        [Header("Shop")]
        public PerkDefinition[] catalog;

        [Header("Visuals")]
        public Material coreMaterial;
        public Material backgroundMaterial;
        public Material enemyMaterial;
        [ColorUsage(false, true)] public Color soulOrbColor = new Color(1.6f, 0.7f, 3f);
        [ColorUsage(false, true)] public Color healOrbColor = new Color(0.6f, 3f, 1.2f);

        [Header("Audio")]
        public int musicVariant;

        [Header("Wording (key -> text)")]
        public LexiconEntry[] words;

        public string Lookup(string key)
        {
            if (words == null) return null;
            foreach (var w in words) if (w.key == key) return w.text;
            return null;
        }
    }
}
