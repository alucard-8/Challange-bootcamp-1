using System.Collections.Generic;
using Soulbreak.Player;
using UnityEngine;

namespace Soulbreak.Weapons
{
    public class WeaponRingView : MonoBehaviour
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

        [SerializeField] private WeaponRing ring;
        [SerializeField] private PlayerShooter shooter;
        [SerializeField] private Sprite quad;
        [SerializeField] private Material segmentMaterial;
        [SerializeField] private Material needleMaterial;
        [SerializeField] private float diameter = 4.2f;
        [SerializeField, ColorUsage(true, true)] private Color basicColor = new Color(0.5f, 0.75f, 1.2f, 1f);
        [SerializeField, ColorUsage(true, true)] private Color loadingColor = new Color(0.9f, 0.9f, 1.4f, 1f);
        [SerializeField, ColorUsage(true, true)] private Color needleColor = new Color(1.5f, 2.5f, 4f, 1f);

        private readonly List<SpriteRenderer> segments = new List<SpriteRenderer>();
        private MaterialPropertyBlock block;
        private Transform pivot;
        private LineRenderer needle;
        private float[] activeAmount, pop;

        private void Awake()
        {
            block = new MaterialPropertyBlock();
            pivot = new GameObject("RingPivot").transform;
            pivot.SetParent(transform, false);
            EnsureSegments();

            needle = new GameObject("Needle").AddComponent<LineRenderer>();
            needle.transform.SetParent(transform, false);
            needle.useWorldSpace = true;
            needle.positionCount = 2;
            needle.sharedMaterial = needleMaterial;
            needle.widthMultiplier = 0.12f;
            needle.numCapVertices = 4;
            needle.sortingOrder = 1;
            needle.startColor = needle.endColor = needleColor;
        }

        private void OnEnable()
        {
            ring.OnLayoutChanged += Redraw;
            ring.OnSlotEmptied += DrawSlot;
            ring.OnSlotLoaded += HandleSlotLoaded;
        }

        private void OnDisable()
        {
            ring.OnLayoutChanged -= Redraw;
            ring.OnSlotEmptied -= DrawSlot;
            ring.OnSlotLoaded -= HandleSlotLoaded;
        }

        private void Start() => Redraw();

        private void HandleSlotLoaded(int i)
        {
            if (i < pop.Length) pop[i] = 1f;
            DrawSlot(i);
        }

        private void LateUpdate()
        {
            pivot.localRotation = Quaternion.Euler(0f, 0f, ring.Rotation);

            int active = ring.GetSlotIndex(shooter.AimDirection);
            for (int i = 0; i < segments.Count; i++)
            {
                float next = Mathf.MoveTowards(activeAmount[i], i == active ? 1f : 0f, Time.deltaTime * 8f);
                bool changed = !Mathf.Approximately(next, activeAmount[i]);
                activeAmount[i] = next;

                if (pop[i] > 0f) { pop[i] = Mathf.Max(0f, pop[i] - Time.deltaTime * 3f); changed = true; }
                if (ring.IsLoading(i)) changed = true;
                if (changed) DrawSlot(i);
            }

            Vector2 c = transform.position;
            needle.SetPosition(0, c + shooter.AimDirection * 0.75f);
            needle.SetPosition(1, c + shooter.AimDirection * (diameter * 0.5f));
            var pattern = ring.GetPatternAt(active);
            var col = pattern == ring.BasicShot ? needleColor : pattern.color;
            needle.startColor = new Color(col.r, col.g, col.b, 0.2f);
            needle.endColor = col;
        }

        private void Redraw()
        {
            EnsureSegments();
            for (int i = 0; i < segments.Count; i++) DrawSlot(i);
        }

        private void EnsureSegments()
        {
            while (segments.Count < ring.SlotCount)
            {
                var go = new GameObject($"Slot{segments.Count}");
                go.transform.SetParent(pivot, false);
                go.transform.localScale = Vector3.one * diameter;
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = quad;
                sr.sharedMaterial = segmentMaterial;
                segments.Add(sr);
            }
            while (segments.Count > ring.SlotCount)
            {
                var old = segments[segments.Count - 1].gameObject;
                old.SetActive(false);
                Destroy(old);
                segments.RemoveAt(segments.Count - 1);
            }
            if (activeAmount == null || activeAmount.Length != segments.Count)
            {
                activeAmount = new float[segments.Count];
                pop = new float[segments.Count];
            }
        }

        private void DrawSlot(int i)
        {
            if (i >= segments.Count) return;
            bool loading = ring.IsLoading(i);
            var pattern = ring.GetSlot(i);
            bool empty = !loading && pattern == ring.BasicShot;
            float start = i * ring.SlotAngle * Mathf.Deg2Rad;

            segments[i].GetPropertyBlock(block);
            block.SetFloat(StartId, start);
            block.SetFloat(EndId, start + ring.SlotAngle * Mathf.Deg2Rad);
            block.SetColor(ColorId, loading ? loadingColor : empty ? basicColor : pattern.color);
            block.SetFloat(IconId, loading || empty ? 0 : pattern.icon);
            block.SetFloat(ActiveId, activeAmount[i]);
            block.SetFloat(EmptyId, empty || loading ? 1f : 0f);
            block.SetFloat(LoadingId, loading ? 1f : 0f);
            block.SetFloat(LoadProgressId, ring.LoadProgress(i));
            block.SetFloat(PopId, pop[i]);
            segments[i].SetPropertyBlock(block);
        }
    }
}
