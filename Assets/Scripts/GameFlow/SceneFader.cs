using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Soulbreak.GameFlow
{
    public static class SceneNames
    {
        public const string MainMenu = "MainMenu";
        public const string Game = "SampleScene";
    }

    // Persistent full-screen fader: fade out -> load scene -> fade in. Created on first use.
    public class SceneFader : MonoBehaviour
    {
        private static SceneFader instance;
        private CanvasGroup group;
        private Image image;
        private bool busy;

        public static bool IsBusy => instance != null && instance.busy;

        private static SceneFader Instance
        {
            get
            {
                if (instance != null) return instance;
                var go = new GameObject("SceneFader", typeof(Canvas), typeof(CanvasGroup));
                DontDestroyOnLoad(go);
                var canvas = go.GetComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.sortingOrder = 1000;
                instance = go.AddComponent<SceneFader>();
                instance.group = go.GetComponent<CanvasGroup>();
                instance.group.alpha = 0f;
                instance.group.blocksRaycasts = false;

                var img = new GameObject("Fade", typeof(RectTransform), typeof(Image));
                img.transform.SetParent(go.transform, false);
                var rt = (RectTransform)img.transform;
                rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
                rt.offsetMin = rt.offsetMax = Vector2.zero;
                instance.image = img.GetComponent<Image>();
                instance.image.color = Color.black;
                return instance;
            }
        }

        public static void LoadScene(string sceneName, float duration = 0.6f) => LoadScene(sceneName, Color.black, duration);

        public static void LoadScene(string sceneName, Color color, float duration = 0.6f)
        {
            var f = Instance;
            if (f.busy) return;
            f.image.color = color;
            f.StartCoroutine(f.Load(sceneName, duration));
        }

        public static void FadeFromBlack(float duration = 1.2f)
        {
            var f = Instance;
            if (f.busy) return;
            f.image.color = Color.black;
            f.group.alpha = 1f;
            f.StartCoroutine(f.Fade(1f, 0f, duration));
        }

        private IEnumerator Load(string sceneName, float duration)
        {
            busy = true;
            group.blocksRaycasts = true;
            yield return Fade(group.alpha, 1f, duration);
            Time.timeScale = 1f;
            SceneManager.LoadScene(sceneName);
            yield return null;
            yield return Fade(1f, 0f, duration);
            group.blocksRaycasts = false;
            busy = false;
        }

        private IEnumerator Fade(float from, float to, float duration)
        {
            for (float t = 0f; t < duration; t += Time.unscaledDeltaTime)
            {
                group.alpha = Mathf.Lerp(from, to, Mathf.SmoothStep(0f, 1f, t / duration));
                yield return null;
            }
            group.alpha = to;
        }
    }
}
