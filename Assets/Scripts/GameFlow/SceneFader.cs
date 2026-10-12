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

    // Persistent full-screen fader: fade out -> load scene -> fade in.
    // The overlay is the Resources/SceneFader prefab, spawned on first use and kept across scenes.
    public class SceneFader : MonoBehaviour
    {
        private static SceneFader instance;
        [SerializeField] private CanvasGroup group;
        [SerializeField] private Image image;
        private bool busy;

        public static bool IsBusy => instance != null && instance.busy;

        private static SceneFader Instance
        {
            get
            {
                if (instance != null) return instance;
                instance = Instantiate(Resources.Load<SceneFader>("SceneFader"));
                DontDestroyOnLoad(instance.gameObject);
                instance.group.alpha = 0f;
                instance.group.blocksRaycasts = false;
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
