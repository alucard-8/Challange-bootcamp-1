using UnityEngine;
using UnityEngine.UI;

namespace Soulbreak.UI
{
    // Sits on every UI canvas. OS fonts (Bahnschrift) can't be saved in prefabs or shipped in the repo,
    // so the prefabs use Unity's built-in font and this swaps in the theme font when the canvas wakes up.
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
