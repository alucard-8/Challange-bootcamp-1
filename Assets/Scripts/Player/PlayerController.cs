using Soulbreak.Core;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Soulbreak.Player
{
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] private PlayerShooter shooter;
        [SerializeField] private NovaAbility nova;
        [SerializeField] private OverdriveSystem overdrive;

        private void Update()
        {
            if (Time.timeScale == 0f) return;

            shooter.AimAtScreenPoint(InputReader.PointerPosition());

            bool overUI = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
            if (InputReader.FirePressed() && !overUI) shooter.Fire();
            if (InputReader.AbilityPressed()) nova.TryUse();
            if (InputReader.OverdrivePressed()) overdrive.TryActivate();
        }
    }
}
