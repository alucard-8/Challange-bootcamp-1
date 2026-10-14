using Soulbreak.Core;
using Soulbreak.Economy;
using Soulbreak.Enemies;
using Soulbreak.Player;
using UnityEngine;

namespace Soulbreak.Perks
{
    public class AdminCheats : MonoBehaviour
    {
        [SerializeField] private PerkShop shop;
        [SerializeField] private SoulWallet wallet;
        [SerializeField] private int souls = 9999;

        [SerializeField] private Reactor.Reactor reactor;

        [SerializeField] private UI.GameOverView gameOverView;

        [Header("Auto aim (F4)")]
        [SerializeField] private PlayerShooter shooter;
        [SerializeField] private EnemyRegistry enemies;
        [SerializeField] private float fireInterval = 0.12f;
        [SerializeField] private bool infiniteAmmo = true;

        private bool autoAim;
        private float fireTimer;

        public bool AutoAimActive => autoAim;

        private void Awake()
        {
            if (!Application.isEditor && !Debug.isDebugBuild)
            {
                enabled = false;
                return;
            }

            if (shooter == null) shooter = FindFirstObjectByType<PlayerShooter>();
            if (enemies == null) enemies = FindFirstObjectByType<EnemyRegistry>();
        }

        private void OnDisable()
        {
            if (shooter != null) shooter.IgnoreAmmo = false;
        }

        private void Update()
        {
            if (InputReader.AutoAimPressed()) ToggleAutoAim();

            if (InputReader.PreviewGameOverPressed() && gameOverView != null) gameOverView.PreviewNewBest();

            if (InputReader.KillSelfPressed() && reactor != null && !reactor.IsDestroyed)
            {
                while (!reactor.IsDestroyed) reactor.TakeDamage(999);
                Debug.Log("ADMIN: core destroyed.");
            }

            if (!InputReader.AdminPressed()) return;
            if (shop != null) shop.GrantAll();
            if (wallet != null) wallet.Add(souls, transform.position);
            Debug.Log("ADMIN: granted every perk and " + souls + " souls.");
        }

        private void LateUpdate()
        {
            if (!autoAim || Time.timeScale == 0f) return;
            if (shooter == null || enemies == null) return;

            Enemy target = NearestEnemy();
            if (target == null) return;

            shooter.AimAtWorldPoint(target.transform.position);

            fireTimer -= Time.deltaTime;
            if (fireTimer > 0f) return;
            fireTimer = fireInterval;
            shooter.Fire();
        }

        private void ToggleAutoAim()
        {
            autoAim = !autoAim;
            fireTimer = 0f;
            if (shooter != null) shooter.IgnoreAmmo = autoAim && infiniteAmmo;
            Debug.Log(autoAim ? "ADMIN: auto aim ON." : "ADMIN: auto aim OFF.");
        }

        private Enemy NearestEnemy()
        {
            Vector2 origin = shooter.MuzzlePosition;
            Enemy best = null;
            float bestSqr = float.MaxValue;
            foreach (var e in enemies.Alive)
            {
                if (e == null || !e.IsAlive) continue;
                float d = ((Vector2)e.transform.position - origin).sqrMagnitude;
                if (d < bestSqr) { bestSqr = d; best = e; }
            }
            return best;
        }
    }
}
