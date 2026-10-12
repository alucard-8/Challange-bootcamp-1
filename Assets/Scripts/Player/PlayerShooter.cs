using System;
using Soulbreak.Core;
using UnityEngine;

namespace Soulbreak.Player
{
    // Turns clicks into shots. Only depends on IShootable, never on Enemy or Orb directly.
    public class PlayerShooter : MonoBehaviour
    {
        [SerializeField] private PlayerStats stats;
        [SerializeField] private Camera cam;

        public event Action<Vector2> OnShotFired;
        public event Action<IShootable> OnTargetHit;

        private void Awake()
        {
            if (cam == null) cam = Camera.main;
        }

        private void Update()
        {
            if (Time.timeScale == 0f || !InputReader.FirePressed()) return;

            Vector2 world = cam.ScreenToWorldPoint(InputReader.PointerPosition());
            OnShotFired?.Invoke(world);

            foreach (var hit in Physics2D.OverlapPointAll(world))
            {
                if (!hit.TryGetComponent(out IShootable shootable)) continue;
                shootable.Shoot(stats.Damage);
                OnTargetHit?.Invoke(shootable);
                return; // one target per click
            }
        }
    }
}
