using Soulbreak.Player;
using UnityEngine;
using UnityEngine.Rendering;

namespace Soulbreak.Fx
{
    public class OverdriveEffect : MonoBehaviour
    {
        [SerializeField] private OverdriveSystem overdrive;
        [SerializeField] private Volume volume;

        private void Update()
        {
            float target = overdrive.IsActive ? 1f : 0f;
            volume.weight = Mathf.MoveTowards(volume.weight, target, Time.unscaledDeltaTime * 3f);
        }
    }
}
