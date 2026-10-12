using System.Collections.Generic;
using Soulbreak.Meta;
using UnityEngine;
using UnityEngine.UI;

namespace Soulbreak.UI
{
    // Rotating tip line (main menu + pause menu). Mixes general tips with random
    // discovered perks from the Index. Runs on unscaled time so it works while paused.
    public class TipsCard : MonoBehaviour
    {
        private static readonly string[] General =
        {
            "Every shot goes through the ring slot your aim crosses. Glowing slots hold your techniques.",
            "A technique slot fires once, then reloads a random technique you own.",
            "The ring re-rolls its slots every wave: 5 to 12 slots, never fewer than your techniques.",
            "Gold ammo charges refill your clip instantly. They drop more often when you're low.",
            "Reload early with R or right click before a big wave hits.",
            "Nova only fires if enemies are near the core. Save it for emergencies.",
            "Overdrive slows time. Kills fill the bar, Q unleashes it.",
            "Green orbs restore a shield charge. Shoot them when the core is hurt.",
            "Elites and bosses always drop an ammo charge.",
            "Some techniques combine into synergies. The shop shows them in green.",
            "Reroll the shop if nothing fits your build.",
            "Headshots on weak points deal bonus damage and fill streaks faster.",
            "Ranged enemies stop and shoot: you can shoot their projectiles down.",
            "Check the Index to see what every enemy and perk does.",
            "Click the core in the main menu... a few times.",
        };

        [SerializeField] private CodexCatalog codex;
        [SerializeField] private Text body;
        [SerializeField] private CanvasGroup group;
        private float timer;
        private int last = -1;
        private const float Duration = 7f;

        private void OnEnable() { timer = 0f; NextTip(); }

        private void Update()
        {
            timer += Time.unscaledDeltaTime;
            // fade out the last half second, swap, fade back in
            float t = Duration - timer;
            group.alpha = Mathf.Clamp01(Mathf.Min(timer * 3f, t * 2f));
            if (timer >= Duration) { timer = 0f; NextTip(); }
        }

        private void NextTip()
        {
            // one in three tips is a perk you've discovered
            var perks = new List<Perks.PerkDefinition>();
            if (codex != null)
                foreach (var p in codex.perks)
                    if (p != null && MetaProgress.IsDiscovered(false, p.name)) perks.Add(p);

            if (perks.Count > 0 && Random.value < 0.34f)
            {
                var p = perks[Random.Range(0, perks.Count)];
                body.text = $"<b>{p.displayName}</b>  -  {p.description}";
                return;
            }

            int i;
            do i = Random.Range(0, General.Length); while (i == last && General.Length > 1);
            last = i;
            body.text = General[i];
        }
    }
}
