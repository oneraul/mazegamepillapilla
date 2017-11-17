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
        public static Dictionary<PlayerControllerIndex, InputController> Controllers;
        public static float KeyRepeatDelay { get; private set; }


        public static void Initialize()
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

        
        public static void SetKeyRepeatDelay(float delay)
        {
            KeyRepeatDelay = delay;
            foreach (InputController controller in Controllers.Values)
            {
                controller.ResetKeyRepeatAccumulators();
            }
        }

        
        public static void Update(float dt)
        {
            foreach (InputController controller in Controllers.Values)
            {
                controller.Update(dt);
            }
        }


        public static event EventHandler<InputStateEventArgs> Pressed;
        public static event EventHandler<InputStateEventArgs> Released;

        public static void RaisePressedEvent(object source, InputStateEventArgs args)
        {
            Pressed?.Invoke(source, args);
        }

        public static void RaiseReleasedEvent(object source, InputStateEventArgs args)
        {
            Released?.Invoke(source, args);
        }
    }


    class InputStateEventArgs : EventArgs
    {
        public PlayerControllerIndex PlayerIndex;
        public InputKeys Button;

        public InputStateEventArgs(PlayerControllerIndex PlayerIndex, InputKeys Button)
        {
            this.PlayerIndex = PlayerIndex;
            this.Button = Button;
        }
    }
}

