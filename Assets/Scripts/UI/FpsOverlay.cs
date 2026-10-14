using UnityEngine;

namespace Soulbreak.UI
{
    public class FpsOverlay : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            var go = new GameObject("FpsOverlay");
            DontDestroyOnLoad(go);
            go.AddComponent<FpsOverlay>();
        }

        private float accum;
        private int frames;
        private float fps;
        private float ms;

        private void Update()
        {
            accum += Time.unscaledDeltaTime;
            frames++;
            if (accum < 0.5f) return;

            fps = frames / accum;
            ms = accum * 1000f / frames;
            accum = 0f;
            frames = 0;
        }

        private void OnGUI()
        {
            if (!Core.GameSettings.FpsOverlay) return;

            var style = new GUIStyle(GUI.skin.label) { fontSize = 18, fontStyle = FontStyle.Bold };
            style.normal.textColor = fps >= 55f ? Color.green : fps >= 30f ? Color.yellow : Color.red;
            GUI.Label(new Rect(12f, 12f, 420f, 30f), $"{fps:0} FPS    {ms:0.0} ms", style);
        }
    }
}
