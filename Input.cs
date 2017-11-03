using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace MazeGamePillaPilla
{
    enum InputKeys
    {
        Up,
        Left,
        Down,
        Right,
        Action,
        Back
    }


    enum PlayerControllerIndex
    {
        One = PlayerIndex.One,
        Two = PlayerIndex.Two,
        Three = PlayerIndex.Three,
        Four = PlayerIndex.Four,
        Keyboard = 4
    }


    class Input
    {
        internal static Dictionary<PlayerControllerIndex, InputController> Controllers;
        internal static float KeyRepeatDelay { get; private set; }


        internal static void Initialize()
        {
            Controllers = new Dictionary<PlayerControllerIndex, InputController>()
            {
                { PlayerControllerIndex.Keyboard, new KeyboardInputController() },
            };

            for (int i = 0; i < 4; i++)
            {
                if (GamePad.GetState(i).IsConnected)
                {
                    PlayerControllerIndex index = (PlayerControllerIndex)i;
                    Controllers.Add(index, new GamepadInputController(index));
                }
            }
        }

        
        internal static void SetKeyRepeatDelay(float delay)
        {
            KeyRepeatDelay = delay;
            foreach (InputController controller in Controllers.Values)
            {
                controller.ResetKeyRepeatAccumulators();
            }
        }

        
        internal static void Update(float dt)
        {
            foreach (InputController controller in Controllers.Values)
            {
                controller.Update(dt);
            }
        }


        internal static event EventHandler<InputStateEventArgs> Pressed;
        internal static event EventHandler<InputStateEventArgs> Released;

        internal static void RaisePressedEvent(object source, InputStateEventArgs args)
        {
            Pressed?.Invoke(source, args);
        }

        internal static void RaiseReleasedEvent(object source, InputStateEventArgs args)
        {
            Released?.Invoke(source, args);
        }
    }


    class InputStateEventArgs : EventArgs
    {
        internal PlayerControllerIndex PlayerIndex;
        internal InputKeys Button;

        internal InputStateEventArgs(PlayerControllerIndex PlayerIndex, InputKeys Button)
        {
            this.PlayerIndex = PlayerIndex;
            this.Button = Button;
        }
    }
}

