using UnityEngine;

namespace Soulbreak.Core
{
    public interface IWeakPoint
    {
        bool IsWeakPointHit(Vector2 worldPoint);
    }
}
