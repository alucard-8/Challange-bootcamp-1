using System.Collections.Generic;

using Soulbreak.Fx;
using UnityEngine;

namespace Soulbreak.Menu
{
    // The living main-menu backdrop: the reactor and its weapon ring spin, slots fire and reload,
    // monsters drift in and get vaporized, soul wisps rise, and the camera leans toward the mouse.
    // Pure presentation — reuses the game's shaders/materials but none of its gameplay systems.
    public class MenuAmbience : MonoBehaviour
    {
        private static readonly int StartId = Shader.PropertyToID("_Start");
        private static readonly int EndId = Shader.PropertyToID("_End");
        private static readonly int ColorId = Shader.PropertyToID("_Color");
        private static readonly int IconId = Shader.PropertyToID("_Icon");
        private static readonly int ActiveId = Shader.PropertyToID("_Active");
        private static readonly int EmptyId = Shader.PropertyToID("_Empty");
        private static readonly int LoadingId = Shader.PropertyToID("_Loading");
        private static readonly int LoadProgressId = Shader.PropertyToID("_LoadProgress");
        private static readonly int PopId = Shader.PropertyToID("_Pop");
        private static readonly int SpikesId = Shader.PropertyToID("_Spikes");
        private static readonly int SeedId = Shader.PropertyToID("_Seed");
        private static readonly int ThicknessId = Shader.PropertyToID("_Thickness");

        [SerializeField] private Transform reactor;
        [SerializeField] private Camera cam;
        [SerializeField] private Sprite quad;
        [SerializeField] private Material enemyMaterial, segmentMaterial, ringMaterial, orbMaterial, tracerMaterial;
        [SerializeField] private float ringDiameter = 5.6f;
        [SerializeField] private int slotCount = 8;
        [SerializeField] private float parallax = 0.5f;

        private static readonly Color[] IconColors =
        {
            new Color(0.5f, 0.75f, 1.2f), new Color(0.6f, 3f, 1.4f), new Color(2.4f, 0.9f, 4f), new Color(3.2f, 2.2f, 0.5f),
        };
        private static readonly Color[] EnemyColors =
        {
            new Color(2.6f, 0.55f, 0.6f), new Color(3f, 2f, 0.4f), new Color(2f, 0.5f, 2.6f), new Color(0.6f, 2.6f, 1f),
        };

        private class Slot { public SpriteRenderer sr; public int icon; public float load = 1f, loadSpeed, pop; }
        private class Drifter { public Transform t; public SpriteRenderer sr; public float speed, spin; public Color color; public float spikes; }
        private class Wisp { public Transform t; public SpriteRenderer sr; public float life, age; public Vector2 vel; }

        private readonly List<Slot> slots = new List<Slot>();
        private readonly List<Drifter> drifters = new List<Drifter>();
        private readonly List<Wisp> wisps = new List<Wisp>();
        private Transform ringPivot;
        private MaterialPropertyBlock block;
        private float enemyTimer, wispTimer, zoom, baseSize;
        private Vector3 camBase;
        private bool zooming;

        private void Start()
        {
            block = new MaterialPropertyBlock();
            camBase = cam.transform.position;
            baseSize = cam.orthographicSize;

            ringPivot = new GameObject("MenuRing").transform;
            ringPivot.position = reactor.position;
            for (int i = 0; i < slotCount; i++)
            {
                var sr = new GameObject($"Slot{i}").AddComponent<SpriteRenderer>();
                sr.transform.SetParent(ringPivot, false);
                sr.transform.localScale = Vector3.one * ringDiameter;
                sr.sprite = quad;
                sr.sharedMaterial = segmentMaterial;
                slots.Add(new Slot { sr = sr, icon = Random.Range(0, 4) });
            }
        }

        public void ZoomIn() => zooming = true;

        // Where the player is pointing (screen space) — fed by MainMenuInput, used for camera parallax.
        public void LookAt(Vector2 screenPoint) => lookScreenPoint = screenPoint;
        private Vector2 lookScreenPoint;

        private void Update()
        {
            float dt = Time.unscaledDeltaTime;

            // Ring spin + fake fire/reload cycle
            ringPivot.Rotate(0f, 0f, 14f * dt);
            for (int i = 0; i < slots.Count; i++) UpdateSlot(slots[i], i, dt);

            enemyTimer -= dt;
            if (enemyTimer <= 0f) { enemyTimer = Random.Range(0.5f, 1.2f); SpawnDrifter(); }
            for (int i = drifters.Count - 1; i >= 0; i--) UpdateDrifter(drifters[i], i, dt);

            wispTimer -= dt;
            if (wispTimer <= 0f) { wispTimer = 0.12f; SpawnWisp(); }
            for (int i = wisps.Count - 1; i >= 0; i--) UpdateWisp(wisps[i], i, dt);

            // Camera: parallax toward the mouse, and a dive into the reactor when Play is pressed.
            Vector2 mouse = lookScreenPoint;
            Vector2 n = new Vector2(mouse.x / Mathf.Max(1, Screen.width) - 0.5f, mouse.y / Mathf.Max(1, Screen.height) - 0.5f);
            if (zooming) zoom = Mathf.Min(1f, zoom + dt * 1.4f);
            float z = zoom * zoom * zoom;
            Vector3 target = camBase + (Vector3)(n * parallax * (1f - z));
            cam.transform.position = Vector3.Lerp(cam.transform.position, target, 1f - Mathf.Exp(-6f * dt));
            cam.orthographicSize = Mathf.Lerp(baseSize, 0.8f, z);
        }

        private void UpdateSlot(Slot s, int i, float dt)
        {
            if (s.load >= 1f && s.icon != 0 && Random.value < 0.25f * dt)
            {
                s.load = 0f;
                s.loadSpeed = 1f / Random.Range(1.5f, 3.5f);
            }
            if (s.load < 1f)
            {
                s.load += s.loadSpeed * dt;
                if (s.load >= 1f) { s.load = 1f; s.icon = Random.Range(0, 4); s.pop = 1f; }
            }
            s.pop = Mathf.Max(0f, s.pop - dt * 3f);

            float span = Mathf.PI * 2f / slots.Count;
            bool loading = s.load < 1f;
            s.sr.GetPropertyBlock(block);
            block.SetFloat(StartId, i * span);
            block.SetFloat(EndId, (i + 1) * span);
            block.SetColor(ColorId, loading ? new Color(0.9f, 0.9f, 1.4f) : IconColors[s.icon]);
            block.SetFloat(IconId, loading ? 0 : s.icon);
            block.SetFloat(ActiveId, 0.25f + 0.25f * Mathf.Sin(Time.unscaledTime * 2f + i));
            block.SetFloat(EmptyId, loading || s.icon == 0 ? 1f : 0f);
            block.SetFloat(LoadingId, loading ? 1f : 0f);
            block.SetFloat(LoadProgressId, s.load);
            block.SetFloat(PopId, s.pop);
            s.sr.SetPropertyBlock(block);
        }

        private void SpawnDrifter()
        {
            var go = new GameObject("MenuEnemy");
            float ang = Random.value * Mathf.PI * 2f;
            go.transform.position = reactor.position + new Vector3(Mathf.Cos(ang), Mathf.Sin(ang)) * 14f;
            go.transform.localScale = Vector3.one * Random.Range(0.8f, 1.8f);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = quad;
            sr.sharedMaterial = enemyMaterial;
            sr.sortingOrder = 2;

            var d = new Drifter
            {
                t = go.transform, sr = sr, speed = Random.Range(0.8f, 2f), spin = Random.Range(-60f, 60f),
                color = EnemyColors[Random.Range(0, EnemyColors.Length)], spikes = Random.Range(3, 10),
            };
            sr.GetPropertyBlock(block);
            block.SetColor(ColorId, d.color);
            block.SetFloat(SpikesId, d.spikes);
            block.SetFloat(SeedId, Random.value * 10f);
            sr.SetPropertyBlock(block);
            drifters.Add(d);
        }

        private void UpdateDrifter(Drifter d, int index, float dt)
        {
            d.t.position = Vector3.MoveTowards(d.t.position, reactor.position, d.speed * dt);
            d.t.Rotate(0f, 0f, d.spin * dt);
            if (Vector3.Distance(d.t.position, reactor.position) > Random.Range(3.4f, 6f)) return;

            // The reactor "shoots" it: tracer + shockwave + dissolve.
            Vector2 from = reactor.position + (d.t.position - reactor.position).normalized * (ringDiameter * 0.45f);
            var tracer = new GameObject("MenuTracer", typeof(LineRenderer), typeof(TracerFx));
            tracer.GetComponent<TracerFx>().Init(from, d.t.position, tracerMaterial, new Color(1.2f, 2.2f, 4f), 0.25f, 0.15f);

            var ring = new GameObject("MenuRingFx").AddComponent<SpriteRenderer>();
            ring.transform.position = d.t.position;
            ring.sprite = quad; ring.sharedMaterial = ringMaterial; ring.sortingOrder = 7;
            var rb = new MaterialPropertyBlock();
            rb.SetColor(ColorId, d.color);
            rb.SetFloat(ThicknessId, 0.15f);
            ring.gameObject.AddComponent<FxAnimator>().Init(rb, "_Progress", 0.4f, Vector3.one * 0.5f, Vector3.one * 3f);

            var db = new MaterialPropertyBlock();
            d.sr.GetPropertyBlock(db);
            d.t.gameObject.AddComponent<FxAnimator>().Init(db, "_Dissolve", 0.5f, d.t.localScale, d.t.localScale * 1.2f);
            drifters.RemoveAt(index);
        }

        private void SpawnWisp()
        {
            var go = new GameObject("Wisp");
            go.transform.position = new Vector3(Random.Range(-12f, 12f), Random.Range(-8f, -6f));
            go.transform.localScale = Vector3.one * Random.Range(0.15f, 0.45f);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = quad;
            sr.sharedMaterial = orbMaterial;
            sr.sortingOrder = -10;
            sr.color = Random.value < 0.7f ? new Color(1.6f, 0.7f, 3f, 0f) : new Color(0.6f, 1.6f, 3f, 0f);
            wisps.Add(new Wisp { t = go.transform, sr = sr, life = Random.Range(5f, 9f), vel = new Vector2(Random.Range(-0.3f, 0.3f), Random.Range(1f, 2.2f)) });
        }

        private void UpdateWisp(Wisp w, int index, float dt)
        {
            w.age += dt;
            w.t.position += (Vector3)(w.vel * dt) + new Vector3(Mathf.Sin(w.age * 2f + index) * 0.3f * dt, 0f);
            var c = w.sr.color;
            c.a = Mathf.Sin(Mathf.Clamp01(w.age / w.life) * Mathf.PI) * 0.8f;
            w.sr.color = c;
            if (w.age < w.life) return;
            Destroy(w.t.gameObject);
            wisps.RemoveAt(index);
        }
    }
}
