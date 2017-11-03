using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace MazeGamePillaPilla
{
    abstract class InputController
    {
        internal PlayerControllerIndex Index { get; private set; }
        protected Dictionary<InputKeys, InputControllerButton> buttons;
        protected Vector2 directionVector;

        protected InputController(PlayerControllerIndex Index)
        {
            this.Index = Index;
            directionVector = new Vector2();
        }

        internal bool IsPressed(InputKeys button)
        {
            return buttons[button].IsPressed;
        }

        internal abstract void Update(float dt);

        internal void ResetKeyRepeatAccumulators()
        {
            foreach (InputControllerButton inputButtonController in buttons.Values)
            {
                inputButtonController.KeyRepeatAccumulator = 0;
            }
        }

        protected void SetDirectionVector(float x, float y)
        {
            directionVector.X = x;
            directionVector.Y = y;
        }

        internal Vector2 GetDirectionVector() => new Vector2(directionVector.X, directionVector.Y);
    }


    class GamepadInputController : InputController
    {
        internal GamepadInputController(PlayerControllerIndex index) : base(index)
        {
            buttons = new Dictionary<InputKeys, InputControllerButton>()
            {
                { InputKeys.Up,     new GamepadInputButtonController(this.Index, InputKeys.Up) },
                { InputKeys.Left,   new GamepadInputButtonController(this.Index, InputKeys.Left) },
                { InputKeys.Down,   new GamepadInputButtonController(this.Index, InputKeys.Down) },
                { InputKeys.Right,  new GamepadInputButtonController(this.Index, InputKeys.Right) },
                { InputKeys.Action, new GamepadInputButtonController(this.Index, InputKeys.Action) },
                { InputKeys.Back,   new GamepadInputButtonController(this.Index, InputKeys.Back) },
            };
        }

        internal override void Update(float dt)
        {
            GamePadState gamepadState = GamePad.GetState((PlayerIndex)this.Index);
            foreach (GamepadInputButtonController button in buttons.Values)
            {
                button.Update(dt, gamepadState);
            }

            directionVector.X = gamepadState.ThumbSticks.Left.X;
            directionVector.Y = -gamepadState.ThumbSticks.Left.Y;
        }
    }


    class KeyboardInputController : InputController
    {
        internal KeyboardInputController() : base(PlayerControllerIndex.Keyboard)
        {
            buttons = new Dictionary<InputKeys, InputControllerButton>()
            {
                { InputKeys.Up,     new KeyboardInputButtonController(InputKeys.Up) },
                { InputKeys.Left,   new KeyboardInputButtonController(InputKeys.Left) },
                { InputKeys.Down,   new KeyboardInputButtonController(InputKeys.Down) },
                { InputKeys.Right,  new KeyboardInputButtonController(InputKeys.Right) },
                { InputKeys.Action, new KeyboardInputButtonController(InputKeys.Action) },
                { InputKeys.Back,   new KeyboardInputButtonController(InputKeys.Back) },
            };
        }

        internal override void Update(float dt)
        {
            KeyboardState keyboardState = Keyboard.GetState();
            foreach (KeyboardInputButtonController button in buttons.Values)
            {
                button.Update(dt, keyboardState);
            }

            directionVector.X = 0 + (IsPressed(InputKeys.Left) ? -1 : 0) + (IsPressed(InputKeys.Right) ? 1 : 0);
            directionVector.Y = 0 + (IsPressed(InputKeys.Up)   ? -1 : 0) + (IsPressed(InputKeys.Down)  ? 1 : 0);
            if (directionVector.LengthSquared() != 0) directionVector.Normalize();
        }
    }
}
