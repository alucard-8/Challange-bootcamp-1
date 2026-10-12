using UnityEngine;

namespace Soulbreak.Core
{
    // Anything weapons should target (enemies, enemy bullets). Weapons depend on this abstraction,
    // never on the Enemy class itself (Dependency Inversion).
    public interface IHostile : IShootable
    {
        Transform transform { get; }
        bool IsAlive { get; }
        // Heavy targets (bosses) resist being pulled or knocked around.
        bool IsHeavy { get; }
    }
}
