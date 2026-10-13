using System;
using System.Collections.Generic;
using UnityEngine;

namespace Soulbreak.Weapons
{
    [DefaultExecutionOrder(-50)]
    public class WeaponRing : MonoBehaviour
    {
        [SerializeField] private ShotPattern basicShot;
        [Tooltip("Each shuffle rolls a random slot count in this range (never below the modules owned).")]
        [SerializeField, Min(1)] private int minSlots = 5;
        [SerializeField, Min(1)] private int maxSlots = 10;
        [Tooltip("Degrees per second. Makes modules sweep around so aiming direction matters.")]
        [SerializeField] private float rotationSpeed = 12f;

        public int SlotCount => slots.Count;
        public int MaxSlots => maxSlots;
        public float Rotation { get; private set; }
        public float SlotAngle => 360f / slots.Count;
        public ShotPattern BasicShot => basicShot;

        public ShotPattern GetSlot(int i) => slots[i];
        public ShotPattern GetPatternAt(int i) => slots[i] != null ? slots[i] : basicShot;
        public bool IsLoading(int i) => slots[i] == null;
        public float LoadProgress(int i) => IsLoading(i) ? 1f - loadTimers[i] / loadDurations[i] : 1f;

        public event Action OnLayoutChanged;
        public event Action<int> OnSlotEmptied;
        public event Action<int> OnSlotLoaded;

        private readonly List<ShotPattern> slots = new List<ShotPattern>();
        private readonly List<float> loadTimers = new List<float>();
        private readonly List<float> loadDurations = new List<float>();
        private readonly Dictionary<ShotPattern, int> modules = new Dictionary<ShotPattern, int>();

        private void Awake() => Shuffle();

        private void Update()
        {
            Rotation = Mathf.Repeat(Rotation + rotationSpeed * Time.deltaTime, 360f);

            for (int i = 0; i < slots.Count; i++)
            {
                if (slots[i] != null) continue;
                loadTimers[i] -= Time.deltaTime;
                if (loadTimers[i] > 0f) continue;
                slots[i] = RandomModule();
                OnSlotLoaded?.Invoke(i);
            }
        }

        public IEnumerable<ShotPattern> OwnedModules => modules.Keys;
        public bool Owns(ShotPattern pattern) => pattern != null && modules.ContainsKey(pattern);

        public int ModuleCount
        {
            get { int n = 0; foreach (var kv in modules) n += kv.Value; return n; }
        }

        public bool AddModule(ShotPattern pattern, int count = 1)
        {
            if (pattern == null || ModuleCount >= maxSlots) return false;
            modules.TryGetValue(pattern, out int n);
            modules[pattern] = n + Mathf.Min(count, maxSlots - ModuleCount);
            Shuffle();
            return true;
        }

        public void Consume(int i)
        {
            var used = slots[i];
            if (used == null || used == basicShot) return;
            slots[i] = null;
            loadDurations[i] = loadTimers[i] = used.reloadTime;
            OnSlotEmptied?.Invoke(i);
        }

        public void Shuffle()
        {
            int count = UnityEngine.Random.Range(Mathf.Max(minSlots, ModuleCount), Mathf.Max(minSlots, maxSlots) + 1);

            slots.Clear();
            foreach (var kv in modules)
                for (int i = 0; i < kv.Value; i++) slots.Add(kv.Key);
            while (slots.Count < count) slots.Add(basicShot);

            for (int i = slots.Count - 1; i > 0; i--)
            {
                int j = UnityEngine.Random.Range(0, i + 1);
                (slots[i], slots[j]) = (slots[j], slots[i]);
            }

            loadTimers.Clear(); loadDurations.Clear();
            for (int i = 0; i < slots.Count; i++) { loadTimers.Add(0f); loadDurations.Add(1f); }
            OnLayoutChanged?.Invoke();
        }

        public int GetSlotIndex(Vector2 direction)
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - Rotation;
            angle = Mathf.Repeat(angle, 360f);
            return Mathf.Clamp(Mathf.FloorToInt(angle / SlotAngle), 0, slots.Count - 1);
        }

        private ShotPattern RandomModule()
        {
            int total = ModuleCount;
            if (total == 0) return basicShot;
            int roll = UnityEngine.Random.Range(0, total);
            foreach (var kv in modules)
            {
                roll -= kv.Value;
                if (roll < 0) return kv.Key;
            }
            return basicShot;
        }
    }
}
