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

        public static bool OverdrivePressed()
        {
#if ENABLE_INPUT_SYSTEM
            return Keyboard.current != null && Keyboard.current.qKey.wasPressedThisFrame;
#else
            return Input.GetKeyDown(KeyCode.Q);
#endif
        }

        public static bool PausePressed()
        {
#if ENABLE_INPUT_SYSTEM
            return Keyboard.current != null && (Keyboard.current.escapeKey.wasPressedThisFrame || Keyboard.current.pKey.wasPressedThisFrame);
#else
            return Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P);
#endif
        }

        public static bool ConfirmPressed()
        {
#if ENABLE_INPUT_SYSTEM
            return Keyboard.current != null && (Keyboard.current.enterKey.wasPressedThisFrame || Keyboard.current.numpadEnterKey.wasPressedThisFrame);
#else
            return Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter);
#endif
        }

        public static bool AdminPressed()
        {
#if ENABLE_INPUT_SYSTEM
            return Keyboard.current != null && Keyboard.current.f1Key.wasPressedThisFrame;
#else
            return Input.GetKeyDown(KeyCode.F1);
#endif
        }

        public static bool KillSelfPressed()
        {
#if ENABLE_INPUT_SYSTEM
            return Keyboard.current != null && Keyboard.current.f2Key.wasPressedThisFrame;
#else
            return Input.GetKeyDown(KeyCode.F2);
#endif
        }

        public static bool PreviewGameOverPressed()
        {
#if ENABLE_INPUT_SYSTEM
            return Keyboard.current != null && Keyboard.current.f3Key.wasPressedThisFrame;
#else
            return Input.GetKeyDown(KeyCode.F3);
#endif
        }

        public static bool SkipTutorialPressed()
        {
#if ENABLE_INPUT_SYSTEM
            return Keyboard.current != null && Keyboard.current.tKey.wasPressedThisFrame;
#else
            return Input.GetKeyDown(KeyCode.T);
#endif
        }

        public static bool LeftPressed()
        {
#if ENABLE_INPUT_SYSTEM
            return Keyboard.current != null && (Keyboard.current.leftArrowKey.wasPressedThisFrame || Keyboard.current.aKey.wasPressedThisFrame);
#else
            return Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A);
#endif
        }

        public static bool RightPressed()
        {
#if ENABLE_INPUT_SYSTEM
            return Keyboard.current != null && (Keyboard.current.rightArrowKey.wasPressedThisFrame || Keyboard.current.dKey.wasPressedThisFrame);
#else
            return Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D);
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
