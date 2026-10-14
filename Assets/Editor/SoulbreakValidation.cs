using System.IO;
using System.Linq;
using Soulbreak.Audio;
using UnityEditor;
using UnityEngine;

namespace Soulbreak.EditorTools
{
    public static class SoulbreakValidation
    {
        [MenuItem("Soulbreak/Validate Project")]
        public static void Validate()
        {
            int issues = 0;

            var lib = Resources.Load<AudioLibrary>("AudioLibrary");
            if (lib == null)
            {
                Debug.LogWarning("Validation: Resources/AudioLibrary.asset not found.");
                issues++;
            }
            else
            {
                int total = lib.sfx != null ? lib.sfx.Length : 0;
                int empty = lib.sfx != null ? lib.sfx.Count(s => s == null || s.clip == null) : 0;
                if (empty > 0)
                {
                    Debug.LogWarning($"Validation: {empty} of {total} SFX slots have no clip.");
                    issues++;
                }
                if (lib.menuMusic == null || lib.menuMusic.clip == null)
                {
                    Debug.LogWarning("Validation: menu music has no clip.");
                    issues++;
                }
                if (lib.gameTiers == null || lib.gameTiers.Length == 0)
                {
                    Debug.LogWarning("Validation: no game music tiers assigned.");
                    issues++;
                }
            }

            if (!File.Exists("Assets/Resources/UIStrings.asset"))
            {
                Debug.LogWarning("Validation: Assets/Resources/UIStrings.asset is missing.");
                issues++;
            }

            Debug.Log(issues == 0 ? "Validation: no issues found." : $"Validation: finished with {issues} warning(s).");
        }
    }
}
