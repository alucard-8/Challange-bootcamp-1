using System.Collections;
using Soulbreak.Meta;
using Soulbreak.UI;
using UnityEngine;

namespace Soulbreak.Menu
{
    public class ReactorEasterEgg : MonoBehaviour
    {
        public const string SecretId = "mode_star";

        [SerializeField] private Transform core;
        [SerializeField] private MainMenuView view;
        [SerializeField] private int clicksNeeded = 7;
        [SerializeField] private float window = 2.5f;
        [SerializeField] private UIStrings strings;

        private UIStrings S => strings != null ? strings : UIFactory.Strings;

        private int clicks;
        private float lastClick;
        private Vector3 baseScale;
        private float punch;
        private bool busy;

        private void Start() => baseScale = core.localScale;

        public void RegisterClick(Vector2 worldPoint)
        {
            if (busy || Vector2.Distance(worldPoint, core.position) > baseScale.x * 0.25f) return;

            clicks = Time.unscaledTime - lastClick > window ? 1 : clicks + 1;
            lastClick = Time.unscaledTime;
            punch = 1f;
            if (clicks >= 3 && clicks < clicksNeeded) view.ShowToast(new string('.', clicks));
            if (clicks >= clicksNeeded) StartCoroutine(Transform());
        }

        private void Update()
        {
            if (busy || punch <= 0f) return;
            punch = Mathf.Max(0f, punch - Time.unscaledDeltaTime * 5f);
            core.localScale = baseScale * (1f - 0.08f * punch + 0.04f * clicks / clicksNeeded);
        }

        private IEnumerator Transform()
        {
            busy = true;
            clicks = 0;

            for (float t = 0f; t < 0.7f; t += Time.unscaledDeltaTime)
            {
                float k = t / 0.7f;
                core.localScale = baseScale * Mathf.Lerp(1f, 0.08f, k * k);
                core.Rotate(0f, 0f, 900f * Time.unscaledDeltaTime * k);
                yield return null;
            }

            view.Flash();
            bool firstTime = !MetaProgress.IsSecretUnlocked(SecretId);
            MetaProgress.UnlockSecret(SecretId);
            MetaProgress.SelectedMode = GameModeService.IsStar ? "" : GameModeService.StarMode;
            core.localRotation = Quaternion.identity;

            for (float t = 0f; t < 0.6f; t += Time.unscaledDeltaTime)
            {
                float k = t / 0.6f;
                float s = 1f + 2.2f * Mathf.Pow(k - 1f, 3f) + 1.2f * Mathf.Pow(k - 1f, 2f);
                core.localScale = baseScale * Mathf.Max(0.08f, s);
                yield return null;
            }
            core.localScale = baseScale;

            if (firstTime) view.ShowToast(S.toastStarCoreUnlocked);
            else view.ShowToast(GameModeService.IsStar ? S.toastStarCoreMode : S.toastSoulReactorMode);
            busy = false;
        }
    }
}
