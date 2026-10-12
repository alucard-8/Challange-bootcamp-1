using Soulbreak.GameFlow;
using UnityEngine;
using UnityEngine.Rendering;

namespace Soulbreak.Fx
{
    // Blends in the pause post-processing (desaturation, heavy vignette, chromatic split, lens warp)
    // following the PauseController's eased blend value.
    public class PauseEffect : MonoBehaviour
    {
        [SerializeField] private PauseController pause;
        [SerializeField] private Volume volume;

        private void Update() => volume.weight = pause.Blend;
    }
}
