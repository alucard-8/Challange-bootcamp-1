using System;
using UnityEngine;

namespace Soulbreak.Core.Events
{
    // Observer pattern via ScriptableObject "event channels" (as in Unity's design-patterns e-book).
    // A channel is an asset: broadcasters call Raise(), listeners subscribe to OnRaised.
    // Neither side references the other — both only reference the shared channel asset.
    public abstract class EventChannel<T> : ScriptableObject
    {
        [TextArea] public string description;

        public event Action<T> OnRaised;

        public void Raise(T value) => OnRaised?.Invoke(value);
    }
}
