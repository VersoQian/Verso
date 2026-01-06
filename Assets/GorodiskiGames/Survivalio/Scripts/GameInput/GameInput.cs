using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.Inputs
{
    public sealed class GameInput : MonoBehaviour
    {
        public event Action ON_NAVIGATE_UP;
        public event Action ON_NAVIGATE_DOWN;
        public event Action ON_NAVIGATE_RIGHT;
        public event Action ON_NAVIGATE_LEFT;
        public event Action ON_SELECTION;
        public event Action ON_ESCAPE;
        public event Action ON_NAVIGATION_RELEASED;

        private bool _wasNavigationPressedLastFrame;

        private void Update()
        {

#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_WEBGL

            bool upPressed = Keyboard.current?.wKey.isPressed == true || Keyboard.current?.upArrowKey.isPressed == true ||
                 (Gamepad.current != null && Gamepad.current.leftStick.up.isPressed);

            bool downPressed = Keyboard.current?.sKey.isPressed == true || Keyboard.current?.downArrowKey.isPressed == true ||
                               (Gamepad.current != null && Gamepad.current.leftStick.down.isPressed);

            bool leftPressed = Keyboard.current?.aKey.isPressed == true || Keyboard.current?.leftArrowKey.isPressed == true ||
                               (Gamepad.current != null && Gamepad.current.leftStick.left.isPressed);

            bool rightPressed = Keyboard.current?.dKey.isPressed == true || Keyboard.current?.rightArrowKey.isPressed == true ||
                                (Gamepad.current != null && Gamepad.current.leftStick.right.isPressed);

            if (upPressed)
                FireNavigateUp();
            else if (downPressed)
                FireNavigateDown();
            else if (leftPressed)
                FireNavigateLeft();
            else if (rightPressed)
                FireNavigateRight();

            bool anyNavigationPressed = upPressed || downPressed || leftPressed || rightPressed;

            if (_wasNavigationPressedLastFrame && !anyNavigationPressed)
            {
                ON_NAVIGATION_RELEASED?.Invoke();
            }

            _wasNavigationPressedLastFrame = anyNavigationPressed;

            if (Keyboard.current?.enterKey.wasPressedThisFrame == true ||
                (Gamepad.current != null && Gamepad.current.buttonSouth.wasPressedThisFrame))
                FireSelection();

            if ((Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame) ||
                (Gamepad.current != null && Gamepad.current.startButton.wasPressedThisFrame))
                FireEscape();

#endif

        }

        private void FireNavigateUp() => ON_NAVIGATE_UP?.Invoke();
        private void FireNavigateDown() => ON_NAVIGATE_DOWN?.Invoke();
        private void FireNavigateRight() => ON_NAVIGATE_RIGHT?.Invoke();
        private void FireNavigateLeft() => ON_NAVIGATE_LEFT?.Invoke();
        private void FireSelection() => ON_SELECTION?.Invoke();
        private void FireEscape() => ON_ESCAPE?.Invoke();
    }
}

