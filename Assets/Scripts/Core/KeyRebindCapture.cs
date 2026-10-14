using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace Soulbreak.Core
{
    public class KeyRebindCapture : MonoBehaviour
    {
        private static KeyRebindCapture instance;

        public static Keybinds.Action Target { get; private set; }

        public static bool IsListening { get; private set; }

        public static void Begin(Keybinds.Action action)
        {
            EnsureInstance();
            Target = action;
            IsListening = true;
        }

        public static void Cancel() => IsListening = false;

        private static void EnsureInstance()
        {
            if (instance != null) return;
            var go = new GameObject("KeyRebindCapture");
            DontDestroyOnLoad(go);
            instance = go.AddComponent<KeyRebindCapture>();
        }

        private void Update()
        {
            if (!IsListening) return;

#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current == null) return;
            foreach (var value in System.Enum.GetValues(typeof(Key)))
            {
                var key = (Key)value;
                if (key == Key.None) continue;
                var control = Keyboard.current[key];
                if (control != null && control.wasPressedThisFrame) { Apply(key.ToString()); return; }
            }
#else
            foreach (var value in System.Enum.GetValues(typeof(KeyCode)))
            {
                var key = (KeyCode)value;
                if (key == KeyCode.None) continue;
                if (Input.GetKeyDown(key)) { Apply(key.ToString()); return; }
            }
#endif
        }

        private void Apply(string keyName)
        {
            Keybinds.Set(Target, keyName);
            IsListening = false;
        }
    }
}
