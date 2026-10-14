using System;
using System.Collections.Generic;
using UnityEngine;

namespace Soulbreak.Player
{
    public enum StatType
    {
        Damage, CritChance, CritMultiplier, SplashRadius, HeadshotMultiplier,
        ClipSize, ReloadTime,
        SoulGain, OrbValue, OrbLifetime, OrbDropChance, HealOrbChance, ComboWindow, Interest,
        NovaCooldown, NovaRadius
    }

    [Serializable]
    public struct StatValue
    {
        public StatType stat;
        public float value;
        public StatValue(StatType stat, float value) { this.stat = stat; this.value = value; }
    }

    [DefaultExecutionOrder(-100)]
    public class PlayerStats : MonoBehaviour
    {
        [SerializeField] private StatValue[] baseValues =
        {
            new StatValue(StatType.Damage, 1),
            new StatValue(StatType.CritChance, 0.05f),
            new StatValue(StatType.CritMultiplier, 2f),
            new StatValue(StatType.SplashRadius, 0f),
            new StatValue(StatType.HeadshotMultiplier, 2f),
            new StatValue(StatType.ClipSize, 6),
            new StatValue(StatType.ReloadTime, 1.7f),
            new StatValue(StatType.SoulGain, 1f),
            new StatValue(StatType.OrbValue, 1f),
            new StatValue(StatType.OrbLifetime, 4f),
            new StatValue(StatType.OrbDropChance, 0f),
            new StatValue(StatType.HealOrbChance, 0f),
            new StatValue(StatType.ComboWindow, 2f),
            new StatValue(StatType.Interest, 0f),
            new StatValue(StatType.NovaCooldown, 15f),
            new StatValue(StatType.NovaRadius, 3.5f),
        };

        private readonly Dictionary<StatType, float> values = new Dictionary<StatType, float>();

        public event Action<StatType, float> OnStatChanged;

        private void Awake()
        {
            foreach (StatType s in Enum.GetValues(typeof(StatType))) values[s] = 0f;
            foreach (var v in baseValues) values[v.stat] = v.value;
        }

        public float Get(StatType stat)
        {
            float v = values[stat];
            switch (stat)
            {
                case StatType.CritChance: return Mathf.Clamp01(v);
                case StatType.ReloadTime: return Mathf.Max(0.1f, v);
                case StatType.NovaCooldown: return Mathf.Max(3f, v);
                default: return v;
            }
        }

        public int GetInt(StatType stat) => Mathf.RoundToInt(Get(stat));

        public void Add(StatType stat, float amount)
        {
            values[stat] += amount;
            OnStatChanged?.Invoke(stat, Get(stat));
        }

        public void Multiply(StatType stat, float factor)
        {
            values[stat] *= factor;
            OnStatChanged?.Invoke(stat, Get(stat));
        }
    }
}
