using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;

#endif
namespace GAD213.Shotgun
{
    // Reads the controls using the input system enabled in Unity.
    public static class PrototypeInput
    {
        public static bool ReloadHeld
        {
            get
            {
#if ENABLE_INPUT_SYSTEM
                return Keyboard.current != null && Keyboard.current.rKey.isPressed;
#else
                return Input.GetKey(KeyCode.R);
#endif
            }
        }

        public static bool MenuHeld
        {
            get
            {
#if ENABLE_INPUT_SYSTEM
                return Keyboard.current != null && Keyboard.current.qKey.isPressed;
#else
                return Input.GetKey(KeyCode.Q);
#endif
            }
        }

        public static bool Pressed(KeyCode key)
        {
#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current == null)
            {
                return false;
            }

            switch (key)
            {
                case KeyCode.Space:
                    return Keyboard.current.spaceKey.wasPressedThisFrame;
                case KeyCode.R:
                    return Keyboard.current.rKey.wasPressedThisFrame;
                case KeyCode.X:
                    return Keyboard.current.xKey.wasPressedThisFrame;
                case KeyCode.T:
                    return Keyboard.current.tKey.wasPressedThisFrame;
                case KeyCode.Alpha1:
                    return Keyboard.current.digit1Key.wasPressedThisFrame;
                case KeyCode.Alpha2:
                    return Keyboard.current.digit2Key.wasPressedThisFrame;
                case KeyCode.F1:
                    return Keyboard.current.f1Key.wasPressedThisFrame;
                case KeyCode.F2:
                    return Keyboard.current.f2Key.wasPressedThisFrame;
                case KeyCode.Escape:
                    return Keyboard.current.escapeKey.wasPressedThisFrame;
                default:
                    return false;
            }
#else
            return Input.GetKeyDown(key);
#endif
        }

        public static bool FirePressed
        {
            get
            {
#if ENABLE_INPUT_SYSTEM
                return Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
#else
                return Input.GetMouseButtonDown(0);
#endif
            }
        }

        public static Vector2 Pointer
        {
            get
            {
#if ENABLE_INPUT_SYSTEM
                return Mouse.current != null ? Mouse.current.position.ReadValue() : Vector2.zero;
#else
                return Input.mousePosition;
#endif
            }
        }
        public static void CentrePointer(Vector2 centre)
        {
#if ENABLE_INPUT_SYSTEM
            if (Mouse.current != null) Mouse.current.WarpCursorPosition(centre);
#endif
        }

        public static Vector2 Movement
        {
            get
            {
#if ENABLE_INPUT_SYSTEM
                var keyboard = Keyboard.current;
                if (keyboard == null)
                {
                    return Vector2.zero;
                }

                return new Vector2((keyboard.dKey.isPressed ? 1 : 0) - (keyboard.aKey.isPressed ? 1 : 0), (keyboard.wKey.isPressed ? 1 : 0) - (keyboard.sKey.isPressed ? 1 : 0));
#else
                return new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
#endif
            }
        }

        public static Vector2 Look
        {
            get
            {
#if ENABLE_INPUT_SYSTEM
                return Mouse.current != null ? Mouse.current.delta.ReadValue() * 0.1f : Vector2.zero;
#else
                return new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
#endif
            }
        }
    }
}

