using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;

namespace MazeGamePillaPilla
{
    class MenuGuiManager
    {
        private List<Button> buttons = new List<Button>();
        private Button currentButton;
        private Button.CurrentState currentButtonState = Button.CurrentState.Hover;
        public event EventHandler<EventArgs> BackButtonPressed;
        public PlayerControllerIndex? PlayerControllerIndex { get; private set; }
        
        public MenuGuiManager() {}
        public MenuGuiManager(PlayerControllerIndex index) => PlayerControllerIndex = index;


        public void Enter()
        {
            currentButton = null;
            ScheduleManager.Schedule(0.2f, () =>
            {
                Input.SetKeyRepeatDelay(0.3f);
                if(buttons.Count > 0) currentButton = buttons[0];
                Input.Pressed += this.OnKeyPressed;
                Input.Released += this.OnKeyReleased;
            });
        }


        public void Exit()
        {
            ScheduleManager.Clear();
            currentButtonState = Button.CurrentState.Hover;
            Input.Pressed -= this.OnKeyPressed;
            Input.Released -= this.OnKeyReleased;
            Input.SetKeyRepeatDelay(-1);
        }


        private void OnKeyPressed(object source, InputStateEventArgs args)
        {
            if (PlayerControllerIndex != null && args.PlayerIndex != PlayerControllerIndex) return;

            switch (args.Button)
            {
                case InputKeys.Up:
                    currentButton = currentButton?.NextButtonUp;
                    break;

                case InputKeys.Left:
                    currentButton = currentButton?.NextButtonLeft;
                    break;

                case InputKeys.Down:
                    currentButton = currentButton?.NextButtonDown;
                    break;

                case InputKeys.Right:
                    currentButton = currentButton?.NextButtonRight;
                    break;

                case InputKeys.Action:
                    currentButtonState = Button.CurrentState.Active;
                    break;

                case InputKeys.Back:
                    BackButtonPressed?.Invoke(this, EventArgs.Empty);
                    break;
            }
        }


        private void OnKeyReleased(object source, InputStateEventArgs args)
        {
            if (PlayerControllerIndex != null && args.PlayerIndex != PlayerControllerIndex) return;

            if (args.Button == InputKeys.Action)
            {
                currentButton?.Click?.Invoke(currentButton, EventArgs.Empty);
            }

            currentButtonState = Button.CurrentState.Hover;
        }


        public void Draw(SpriteBatch spritebatch)
        {
            foreach (Button button in buttons)
            {
                if (button == currentButton)
                {
                    switch (currentButtonState)
                    {
                        case Button.CurrentState.Hover:
                            button.DrawHover(spritebatch);
                            break;

                        case Button.CurrentState.Active:
                            button.DrawActive(spritebatch);
                            break;
                    }
                }
                else
                {
                    button.DrawNormal(spritebatch);
                }
            }
        }


        public void AddButton(Button button)
        {
            if (!buttons.Contains(button)) buttons.Add(button);
        }

        public void RemoveButton(Button button)
        {
            if (buttons.Contains(button))
            {
                button.NextButtonUp.NextButtonDown    = button.NextButtonDown;
                button.NextButtonLeft.NextButtonRight = button.NextButtonRight;
                button.NextButtonDown.NextButtonUp    = button.NextButtonUp;
                button.NextButtonRight.NextButtonLeft = button.NextButtonLeft;
                buttons.Remove(button);

                if (currentButton == button)
                {
                    currentButton = button.NextButtonDown;
                    if (currentButton == button) currentButton = null;
                }
            }
        }

        public Button GetButton(int index)
        {
            return buttons[index];
        }

        public void SetCurrentButton(Button button)
        {
            if (buttons.Contains(button)) currentButton = button;
        }
    }
}
