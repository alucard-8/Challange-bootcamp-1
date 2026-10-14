using UnityEngine;

namespace Soulbreak.UI
{
    [RequireComponent(typeof(RectTransform))]
    public class SafeAreaFitter : MonoBehaviour
    {
        private RectTransform rt;
        private Rect lastSafeArea;
        private ScreenOrientation lastOrientation;

        private void Awake()
        {
            rt = GetComponent<RectTransform>();
            Apply();
        }

        private void Update()
        {
            if (Screen.safeArea != lastSafeArea || Screen.orientation != lastOrientation) Apply();
        }

        private void Apply()
        {
            lastSafeArea = Screen.safeArea;
            lastOrientation = Screen.orientation;

            if (Screen.width <= 0 || Screen.height <= 0) return;

            Vector2 min = lastSafeArea.position;
            Vector2 max = lastSafeArea.position + lastSafeArea.size;
            min.x /= Screen.width;
            min.y /= Screen.height;
            max.x /= Screen.width;
            max.y /= Screen.height;

            rt.anchorMin = min;
            rt.anchorMax = max;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }
    }
}
