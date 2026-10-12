using System;
using Soulbreak.Weapons;
using UnityEngine;

namespace Soulbreak.Meta
{
    public enum MetaItemType { StartingTechnique, ReactorSkin, GameMode }

    [Serializable]
    public class MetaItem
    {
        public string id;
        public string displayName;
        [TextArea] public string description;
        public MetaItemType type;
        [Min(0)] public int cost;
        [Header("Starting technique")]
        public ShotPattern technique;
        [Header("Game mode (secret unlock)")]
        public string modeId;
        [TextArea] public string unlockHint;
        [Header("Reactor skin (HDR)")]
        [ColorUsage(true, true)] public Color coreColor = new Color(0.6f, 1.6f, 3f);
        [ColorUsage(true, true)] public Color shieldColor = new Color(0.5f, 2.5f, 3f);
    }

    // Everything purchasable between runs with Soul Essence. Data only.
    [CreateAssetMenu(menuName = "Soulbreak/Meta Catalog", fileName = "MetaCatalog")]
    public class MetaCatalog : ScriptableObject
    {
        public MetaItem[] items;

        public MetaItem Find(string id)
        {
            if (string.IsNullOrEmpty(id) || items == null) return null;
            foreach (var i in items) if (i.id == id) return i;
            return null;
        }
    }
}
