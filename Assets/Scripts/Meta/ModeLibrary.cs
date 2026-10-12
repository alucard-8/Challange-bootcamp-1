using UnityEngine;

namespace Soulbreak.Meta
{
    // All game modes. Lives in Resources so both scenes can find it without wiring.
    [CreateAssetMenu(menuName = "Soulbreak/Mode Library", fileName = "ModeLibrary")]
    public class ModeLibrary : ScriptableObject
    {
        public ModeProfile[] modes;

        public ModeProfile Find(string id)
        {
            if (modes == null) return null;
            foreach (var m in modes) if (m != null && m.id == id) return m;
            return null;
        }
    }
}
