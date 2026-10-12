using System.Collections.Generic;
using Soulbreak.Core;
using Soulbreak.Enemies;
using UnityEngine;

namespace Soulbreak.Weapons
{
    // Runtime object for Orbital Blades: rotates blade sprites around a center and damages on contact,
    // with a per-target cooldown so one enemy isn't hit every frame.
    public class OrbitBlades : MonoBehaviour
    {
        private OrbitShotPattern pattern;
        private readonly List<SpriteRenderer> blades = new List<SpriteRenderer>();
        private readonly Dictionary<Collider2D, float> lastHit = new Dictionary<Collider2D, float>();
        private float angle, age;
        private int damage;

        public static void Spawn(OrbitShotPattern pattern, Vector2 center, float startAngle, int damage)
        {
            var go = new GameObject("OrbitalBlades");
            go.transform.position = center;
            var ob = go.AddComponent<OrbitBlades>();
            ob.pattern = pattern;
            ob.angle = startAngle;
            ob.damage = damage;

            for (int i = 0; i < pattern.blades; i++)
            {
                var sr = new GameObject("Blade").AddComponent<SpriteRenderer>();
                sr.transform.SetParent(go.transform, false);
                sr.transform.localScale = new Vector3(pattern.bladeSize * 1.6f, pattern.bladeSize * 0.55f, 1f);
                sr.sprite = pattern.sprite;
                if (pattern.material != null) sr.sharedMaterial = pattern.material;
                sr.color = pattern.color;
                sr.sortingOrder = 6;
                ob.blades.Add(sr);
            }
            ob.Place();
        }

        private void Update()
        {
            age += Time.deltaTime;
            angle += pattern.angularSpeed * Time.deltaTime;

            // Spin up from the ring, then hold the orbit, fade out at the end.
            float grow = Mathf.Clamp01(age / 0.3f);
            float fade = Mathf.Clamp01((pattern.duration - age) / 0.5f);
            Place(grow);
            foreach (var b in blades) b.color = new Color(pattern.color.r, pattern.color.g, pattern.color.b, fade);

            foreach (var b in blades) HitAround(b.transform.position);
            if (age >= pattern.duration) Destroy(gameObject);
        }

        private void Place(float grow = 0f)
        {
            float r = Mathf.Lerp(1.9f, pattern.radius, grow);
            for (int i = 0; i < blades.Count; i++)
            {
                float a = (angle + i * 360f / blades.Count) * Mathf.Deg2Rad;
                var t = blades[i].transform;
                t.localPosition = new Vector3(Mathf.Cos(a), Mathf.Sin(a)) * r;
                t.localRotation = Quaternion.Euler(0f, 0f, a * Mathf.Rad2Deg + 90f);   // blade lies along the orbit
            }
        }

        private void HitAround(Vector2 pos)
        {
            foreach (var hit in Physics2D.OverlapCircleAll(pos, pattern.bladeSize * 0.6f))
            {
                bool target = hit.GetComponent<Enemy>() != null || hit.GetComponent<EnemyProjectile>() != null;
                if (!target || !hit.TryGetComponent(out IShootable s)) continue;

                var id = hit;
                if (lastHit.TryGetValue(id, out float t) && age - t < pattern.hitCooldown) continue;
                lastHit[id] = age;
                s.Shoot(damage);
            }
        }
    }
}
