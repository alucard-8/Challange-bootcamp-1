using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace Soulbreak.Core
{
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

        public static bool AbilityPressed() => Down(Keybinds.Action.Nova);
        public static bool OverdrivePressed() => Down(Keybinds.Action.Overdrive);
        public static bool PausePressed() => Down(Keybinds.Action.Pause);
        public static bool RestartPressed() => Down(Keybinds.Action.Restart);
        public static bool SkipTutorialPressed() => Down(Keybinds.Action.SkipTutorial);

        public static bool LeftPressed() => Down(Keybinds.Action.Left) || ArrowPressed(true);
        public static bool RightPressed() => Down(Keybinds.Action.Right) || ArrowPressed(false);

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

        public static bool AutoAimPressed()
        {
#if ENABLE_INPUT_SYSTEM
            return Keyboard.current != null && Keyboard.current.f4Key.wasPressedThisFrame;
#else
            return Input.GetKeyDown(KeyCode.F4);
#endif
        }

        public static bool PerkPressed(int index)
        {
            if (index < 0 || index > 8) return false;
#if ENABLE_INPUT_SYSTEM
            return Keyboard.current != null && Keyboard.current[Key.Digit1 + index].wasPressedThisFrame;
#else
            return Input.GetKeyDown(KeyCode.Alpha1 + index);
#endif
        }

        private static bool Down(Keybinds.Action action)
        {
            string name = Keybinds.GetName(action);
            if (string.IsNullOrEmpty(name)) return false;
#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current == null) return false;
            return System.Enum.TryParse(name, out Key key) && Keyboard.current[key].wasPressedThisFrame;
#else
            return System.Enum.TryParse(name, out KeyCode key) && Input.GetKeyDown(key);
#endif
        }

        private static bool ArrowPressed(bool left)
        {
#if ENABLE_INPUT_SYSTEM
            return Keyboard.current != null && Keyboard.current[left ? Key.LeftArrow : Key.RightArrow].wasPressedThisFrame;
#else
            return Input.GetKeyDown(left ? KeyCode.LeftArrow : KeyCode.RightArrow);
#endif
        }
    }
}
