using System;
using UnityEngine;

namespace Soulbreak.Core.Events
{
    public abstract class EventChannel<T> : ScriptableObject
    {
        [TextArea] public string description;

        public event Action<T> OnRaised;

        public void Raise(T value) => OnRaised?.Invoke(value);
    }
}
