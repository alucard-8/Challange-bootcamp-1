using UnityEngine;

namespace Soulbreak.Core.Events
{
    // A channel that carries a world position (e.g. "headshot happened here").
    [CreateAssetMenu(menuName = "Soulbreak/Events/Vector2 Event Channel", fileName = "Vector2EventChannel")]
    public class Vector2EventChannel : EventChannel<Vector2> { }
}
