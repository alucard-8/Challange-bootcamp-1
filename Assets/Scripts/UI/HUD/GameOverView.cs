using Soulbreak.Economy;
using Soulbreak.GameFlow;
using UnityEngine;
using UnityEngine.UI;

namespace Soulbreak.UI
{
    // Game-over overlay: run summary + Restart / Main Menu buttons. Fades in when the reactor falls.
    public class GameOverView : MonoBehaviour
    {
        [SerializeField] private HUDCanvas hud;
        [SerializeField] private GameManager game;
        [SerializeField] private SoulWallet wallet;

        private CanvasGroup group;
        private Text text;

        private void Awake() => Build();
        private void OnEnable() => game.OnStateChanged += HandleState;
        private void OnDisable() => game.OnStateChanged -= HandleState;

        private void HandleState(GameState state)
        {
            if (state != GameState.GameOver) return;
            group.alpha = 0.01f;   // Update fades it in
            group.blocksRaycasts = group.interactable = true;
            text.text =
                "<size=90><b>REACTOR DESTROYED</b></size>\n\n" +
                $"Waves cleared  <b>{game.WavesCleared}</b>\n" +
                $"Time survived  <b>{game.SurvivalTime:0}s</b>\n" +
                $"Souls banked  <b>{wallet.Souls}</b>\n\n" +
                "<size=30>R  restart</size>\n\n\n\n";
        }

        private void Update()
        {
            if (group.alpha > 0f && group.alpha < 1f)
                group.alpha = Mathf.Min(1f, group.alpha + Time.unscaledDeltaTime * 2f);
        }

        private void Build()
        {
            var bg = UIFactory.Panel(hud.Root, new Color(0.03f, 0f, 0.05f, 0.85f), "GameOver");
            bg.rectTransform.Fill();
            bg.raycastTarget = true;
            group = bg.gameObject.AddComponent<CanvasGroup>();
            group.alpha = 0f;
            group.blocksRaycasts = group.interactable = false;
            group.ignoreParentGroups = true;   // independent of the HUD fade

            text = UIFactory.Label(bg.transform, "", 40, TextAnchor.MiddleCenter, Color.white);
            text.rectTransform.Fill();
            text.gameObject.AddComponent<GlitchText>().burstChance = 0.6f;

            var restart = UIFactory.Button(bg.transform, "RESTART", new Color(0.1f, 0.42f, 0.55f, 0.95f), () => game.Restart(), out _, 32);
            ((RectTransform)restart.transform).Place(new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(-190, 150), new Vector2(340, 80));
            var menu = UIFactory.Button(bg.transform, "MAIN MENU", UIFactory.Theme.buttonColor, () => game.ToMainMenu(), out _, 32);
            ((RectTransform)menu.transform).Place(new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(190, 150), new Vector2(340, 80));
        }
    }
}
