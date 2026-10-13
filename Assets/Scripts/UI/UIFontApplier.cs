using UnityEngine;
using UnityEngine.UI;

namespace Soulbreak.UI
{
    [DisallowMultipleComponent]
    public class UIFontApplier : MonoBehaviour
    {
        private void Awake()
        {
            var font = UIFactory.Font;
            foreach (var t in GetComponentsInChildren<Text>(true)) t.font = font;
        }
    }
}
