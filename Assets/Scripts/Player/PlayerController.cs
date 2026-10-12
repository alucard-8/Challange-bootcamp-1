using Soulbreak.Core;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Soulbreak.Player
{
    // Input layer for the player: reads InputReader and sends commands (aim, fire, reload, nova).
    // The gameplay components it drives never touch input themselves.
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] private PlayerShooter shooter;
        [SerializeField] private AmmoSystem ammo;
        [SerializeField] private NovaAbility nova;

        private void Update()
        {
            if (Time.timeScale == 0f) return;

            shooter.AimAtScreenPoint(InputReader.PointerPosition());

            bool overUI = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
            if (InputReader.FirePressed() && !overUI) shooter.Fire();
            if (InputReader.ReloadPressed()) ammo.Reload();
            if (InputReader.AbilityPressed()) nova.TryUse();
        }
    }
}
