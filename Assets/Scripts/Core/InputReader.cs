using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace Soulbreak.Core
{
    // Single place that talks to the input backend, so gameplay systems don't care which one is active.
    public static class InputReader
    {
        public static bool FirePressed()
        {
#if ENABLE_INPUT_SYSTEM
            return Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
#else
            return Input.GetMouseButtonDown(0);
#endif
        }

        public static Vector2 PointerPosition()
        {
#if ENABLE_INPUT_SYSTEM
            return Mouse.current != null ? Mouse.current.position.ReadValue() : Vector2.zero;
#else
            return Input.mousePosition;
#endif
        }

        public static bool AbilityPressed()
        {
#if ENABLE_INPUT_SYSTEM
            return Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame;
#else
            return Input.GetKeyDown(KeyCode.Space);
#endif
        }

        public static bool RestartPressed()
        {
#if ENABLE_INPUT_SYSTEM
            return Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame;
#else
            return Input.GetKeyDown(KeyCode.R);
#endif
        }

        // index 0 -> key "1", index 1 -> key "2", ...
        public static bool PerkPressed(int index)
        {
            if (index < 0 || index > 8) return false;
#if ENABLE_INPUT_SYSTEM
            return Keyboard.current != null && Keyboard.current[Key.Digit1 + index].wasPressedThisFrame;
#else
            return Input.GetKeyDown(KeyCode.Alpha1 + index);
#endif
        }
    }
}
