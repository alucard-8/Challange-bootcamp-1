using UnityEngine;

namespace Soulbreak.Core
{
    // Something with a precise weak spot (enemy eyes). Hitting it counts as a headshot.
    public interface IWeakPoint
    {
        bool IsWeakPointHit(Vector2 worldPoint);
    }
}
