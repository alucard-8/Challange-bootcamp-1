using Soulbreak.Audio;
using Soulbreak.Meta;
using UnityEngine;

namespace Soulbreak.Menu
{
    // Re-themes the main menu for the selected mode (core + background + enemies + music + title)
    // and re-applies whenever the selection changes.
    [DefaultExecutionOrder(-100)]
    public class MenuModeApplier : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer core;
        [SerializeField] private SpriteRenderer background;
        [SerializeField] private MenuAmbience ambience;
        [SerializeField] private MainMenuView view;

        private string applied;

        private void Awake() => Apply();
        private void OnEnable() => MetaProgress.OnChanged += Apply;
        private void OnDisable() => MetaProgress.OnChanged -= Apply;

        private void Apply()
        {
            var mode = GameModeService.Current;
            if (mode == null || mode.id == applied) return;
            applied = mode.id;

            if (mode.coreMaterial != null) core.sharedMaterial = mode.coreMaterial;
            if (mode.backgroundMaterial != null) background.sharedMaterial = mode.backgroundMaterial;
            if (mode.enemyMaterial != null) ambience.SetEnemyMaterial(mode.enemyMaterial);
            view.SetTitle(mode.title, mode.tagline);
            Audio.AudioManager.Instance.PlayMenuMusic();
        }
    }
}
