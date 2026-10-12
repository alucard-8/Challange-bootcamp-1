using System.Text;
using Soulbreak.Economy;
using Soulbreak.Enemies;
using Soulbreak.GameFlow;
using Soulbreak.Perks;
using Soulbreak.Player;
using UnityEngine;
using UnityEngine.UI;

namespace Soulbreak.UI
{
    // Pure listener: subscribes to every system's events and redraws. No gameplay logic here.
    public class HUD : MonoBehaviour
    {
        [SerializeField] private Reactor.Reactor reactor;
        [SerializeField] private SoulWallet wallet;
        [SerializeField] private PerkShop shop;
        [SerializeField] private PlayerStats stats;
        [SerializeField] private NovaAbility nova;
        [SerializeField] private EnemySpawner spawner;
        [SerializeField] private GameManager gameManager;

        private Text statusText, perksText, centerText, messageText;
        private int health, maxHealth, shield, maxShield, souls, level;
        private float messageTimer;

        private void Awake() => BuildCanvas();

        private void OnEnable()
        {
            reactor.OnHealthChanged += HandleHealth;
            reactor.OnShieldChanged += HandleShield;
            wallet.OnSoulsChanged += HandleSouls;
            shop.OnPerkBought += HandlePerkBought;
            shop.OnPurchaseFailed += HandlePurchaseFailed;
            stats.OnStatsChanged += RefreshPerks;
            nova.OnReady += HandleNovaReady;
            spawner.OnDifficultyChanged += HandleDifficulty;
            gameManager.OnStateChanged += HandleState;
        }

        private void OnDisable()
        {
            reactor.OnHealthChanged -= HandleHealth;
            reactor.OnShieldChanged -= HandleShield;
            wallet.OnSoulsChanged -= HandleSouls;
            shop.OnPerkBought -= HandlePerkBought;
            shop.OnPurchaseFailed -= HandlePurchaseFailed;
            stats.OnStatsChanged -= RefreshPerks;
            nova.OnReady -= HandleNovaReady;
            spawner.OnDifficultyChanged -= HandleDifficulty;
            gameManager.OnStateChanged -= HandleState;
        }

        private void HandleHealth(int cur, int max) { health = cur; maxHealth = max; RefreshStatus(); }
        private void HandleShield(int cur, int max) { shield = cur; maxShield = max; RefreshStatus(); }
        private void HandleSouls(int value) { souls = value; RefreshStatus(); RefreshPerks(); }
        private void HandleDifficulty(int lvl) { level = lvl; RefreshStatus(); if (lvl > 0) ShowMessage($"THREAT LEVEL {lvl + 1}"); }
        private void HandlePerkBought(PerkConfig perk) { ShowMessage($"+ {perk.displayName}"); RefreshPerks(); }
        private void HandlePurchaseFailed(PerkConfig perk) => ShowMessage("Not enough souls");
        private void HandleNovaReady() => ShowMessage("NOVA READY");

        private void HandleState(GameState state)
        {
            if (state != GameState.GameOver) return;
            centerText.text = $"REACTOR DESTROYED\nSurvived {gameManager.SurvivalTime:0}s\n\nPress R to restart";
        }

        private void Update()
        {
            // Timer-driven values (time, cooldown) are read each frame; everything else is event-driven.
            RefreshStatus();
            if (messageTimer > 0f)
            {
                messageTimer -= Time.unscaledDeltaTime;
                if (messageTimer <= 0f) messageText.text = "";
            }
        }

        private void RefreshStatus()
        {
            if (statusText == null) return;
            string novaState = nova.IsReady ? "READY [SPACE]" : $"{nova.CooldownRemaining:0.0}s";
            statusText.text =
                $"REACTOR  {health}/{maxHealth}\n" +
                $"SHIELD   {shield}/{maxShield}\n" +
                $"SOULS    {souls}\n" +
                $"NOVA     {novaState}\n" +
                $"THREAT   {level + 1}    TIME {gameManager.SurvivalTime:0}s";
        }

        private void RefreshPerks()
        {
            if (perksText == null) return;
            var sb = new StringBuilder("PERKS\n");
            for (int i = 0; i < shop.PerkCount; i++)
            {
                int cost = shop.GetCost(i);
                string mark = souls >= cost ? "" : "  (locked)";
                sb.AppendLine($"[{i + 1}] {shop.GetPerk(i).displayName} - {cost}{mark}");
            }
            sb.Append($"DMG {stats.Damage}   SOUL x{stats.SoulMultiplier:0.0}");
            perksText.text = sb.ToString();
        }

        private void ShowMessage(string msg)
        {
            messageText.text = msg;
            messageTimer = 1.5f;
        }

        private void Start() => RefreshPerks();

        // --- Runtime UI construction (keeps the scene setup simple) ---
        private void BuildCanvas()
        {
            var canvasGo = new GameObject("HUD Canvas", typeof(Canvas), typeof(CanvasScaler));
            canvasGo.transform.SetParent(transform, false);
            canvasGo.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);

            statusText = MakeText(canvasGo.transform, TextAnchor.UpperLeft, new Vector2(0, 1), new Vector2(30, -30), 30);
            perksText = MakeText(canvasGo.transform, TextAnchor.LowerLeft, new Vector2(0, 0), new Vector2(30, 30), 28);
            centerText = MakeText(canvasGo.transform, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), Vector2.zero, 56);
            messageText = MakeText(canvasGo.transform, TextAnchor.UpperCenter, new Vector2(0.5f, 1), new Vector2(0, -40), 40);
        }

        private static Text MakeText(Transform parent, TextAnchor anchor, Vector2 pivot, Vector2 offset, int size)
        {
            var go = new GameObject("Text", typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = rt.pivot = pivot;
            rt.anchoredPosition = offset;
            rt.sizeDelta = new Vector2(900, 400);

            var t = go.GetComponent<Text>();
            t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            t.fontSize = size;
            t.alignment = anchor;
            t.color = Color.white;
            t.raycastTarget = false;
            return t;
        }
    }
}
