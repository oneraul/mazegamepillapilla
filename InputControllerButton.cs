using System.Collections.Generic;
using Microsoft.Xna.Framework.Input;

namespace MazeGamePillaPilla
{
    abstract class InputControllerButton
    {
        public PlayerControllerIndex PlayerIndex { get; private set; }
        public InputKeys Button { get; private set; }
        public bool IsPressed { get; set; }
        public float KeyRepeatAccumulator { private get; set; }
        protected bool wasPressed;

        protected InputControllerButton(PlayerControllerIndex PlayerIndex, InputKeys id)
        {
            this.PlayerIndex = PlayerIndex;
            Button = id;
        }

        protected void CheckEvents(float dt)
        {
            if (!wasPressed)
            {
                if (IsPressed)
                {
                    wasPressed = true;
                    Input.RaisePressedEvent(this, new InputStateEventArgs(PlayerIndex, Button));
                }
            }
            else
            {
                if (IsPressed)
                {
                    if (Input.KeyRepeatDelay > 0)
                    {
                        KeyRepeatAccumulator += dt;
                        if (KeyRepeatAccumulator >= Input.KeyRepeatDelay)
                        {
                            KeyRepeatAccumulator -= Input.KeyRepeatDelay;
                            Input.RaisePressedEvent(this, new InputStateEventArgs(PlayerIndex, Button));
                        }
                    }
                }
                else
                {
                    wasPressed = false;
                    KeyRepeatAccumulator = 0;
                    Input.RaiseReleasedEvent(this, new InputStateEventArgs(PlayerIndex, Button));
                }
            }
        }
    }


    class GamepadInputButtonController : InputControllerButton
    {
        private static readonly Dictionary<InputKeys, Buttons[]> keyBindings = new Dictionary<InputKeys, Buttons[]>
        {
            { InputKeys.Up,     new Buttons[] { Buttons.LeftThumbstickUp, Buttons.DPadUp } },
            { InputKeys.Left,   new Buttons[] { Buttons.LeftThumbstickLeft, Buttons.DPadLeft } },
            { InputKeys.Down,   new Buttons[] { Buttons.LeftThumbstickDown, Buttons.DPadDown } },
            { InputKeys.Right,  new Buttons[] { Buttons.LeftThumbstickRight, Buttons.DPadRight } },
            { InputKeys.Action, new Buttons[] { Buttons.A } },
            { InputKeys.Back,   new Buttons[] { Buttons.B } }
        };

        private Buttons[] _buttons;

        public GamepadInputButtonController(PlayerControllerIndex PlayerIndex, InputKeys id) : base(PlayerIndex, id)
        {
            _buttons = keyBindings[id];
        }

        public void Update(float dt, GamePadState gamepadState)
        {
            wasPressed = IsPressed;
            IsPressed = false;
            for (int i = 0; i < _buttons.Length; i++)
            {
                IsPressed = gamepadState.IsButtonDown(_buttons[i]);
                if (IsPressed) break;
            }
            CheckEvents(dt);
        }
    }


    class KeyboardInputButtonController : InputControllerButton
    {
        private static readonly Dictionary<InputKeys, Keys[]> keyBindings = new Dictionary<InputKeys, Keys[]>
        {
            { InputKeys.Up,     new Keys[] { Keys.W, Keys.Up } },
            { InputKeys.Left,   new Keys[] { Keys.A, Keys.Left } },
            { InputKeys.Down,   new Keys[] { Keys.S, Keys.Down } },
            { InputKeys.Right,  new Keys[] { Keys.D, Keys.Right } },
            { InputKeys.Action, new Keys[] { Keys.Enter, Keys.Space } },
            { InputKeys.Back,   new Keys[] { Keys.Escape } }
        };


        private Keys[] _keys;

        public KeyboardInputButtonController(InputKeys id) : base(PlayerControllerIndex.Keyboard, id)
        {
            _keys = keyBindings[id];

            Dictionary<int, Keys[]> d = new Dictionary<int, Keys[]>()
            {
                { 1, new Keys[] { Keys.W, Keys.Up } },
            };
        }

        public void Update(float dt, KeyboardState keyboardState)
        {
            wasPressed = IsPressed;
            IsPressed = false;
            for(int i = 0; i < _keys.Length; i++)
            {
                IsPressed = keyboardState.IsKeyDown(_keys[i]);
                if (IsPressed) break;
            }
            CheckEvents(dt);
        }
    }
}
