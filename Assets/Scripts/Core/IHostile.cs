using UnityEngine;

namespace Soulbreak.Core
{
    public interface IHostile : IShootable
    {
        Transform transform { get; }
        bool IsAlive { get; }
        bool IsHeavy { get; }
    }
}
