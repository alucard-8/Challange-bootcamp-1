using System.Collections.Generic;
using UnityEngine;

namespace Soulbreak.Enemies
{
    [DefaultExecutionOrder(-40)]

    public class EnemyRegistry : MonoBehaviour
    {
        [SerializeField] private EnemyEventChannel spawned;
        [SerializeField] private EnemyEventChannel killed;
        [SerializeField] private EnemyEventChannel reachedTarget;

        private readonly HashSet<Enemy> alive = new HashSet<Enemy>();

        public IReadOnlyCollection<Enemy> Alive => alive;
        public int Count => alive.Count;

        private void OnEnable()
        {
            spawned.OnRaised += Add;
            killed.OnRaised += Remove;
            reachedTarget.OnRaised += Remove;
        }

        private void OnDisable()
        {
            spawned.OnRaised -= Add;
            killed.OnRaised -= Remove;
            reachedTarget.OnRaised -= Remove;
        }

        private void Add(Enemy e) => alive.Add(e);
        private void Remove(Enemy e) => alive.Remove(e);

        private void LateUpdate() => alive.RemoveWhere(e => e == null);
    }
}
