using System.Collections.Generic;
using UnityEngine;

namespace Soulbreak.GameFlow
{
    [DefaultExecutionOrder(-90)]
    public class TimeScaleService : MonoBehaviour
    {
        private readonly Dictionary<string, float> layers = new Dictionary<string, float>();

        public void Set(string layer, float scale)
        {
            layers[layer] = Mathf.Max(0f, scale);
            Apply();
        }

        public void Clear(string layer)
        {
            if (layers.Remove(layer)) Apply();
        }

        private void Apply()
        {
            float s = 1f;
            foreach (var v in layers.Values) s *= v;
            Time.timeScale = s;
        }

        private void Awake() => Time.timeScale = 1f;
        private void OnDestroy() => Time.timeScale = 1f;
    }
}
