using System;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace MazeGamePillaPilla
{
    class ScoresScreen : IScreen
    {
        private MenuGuiManager gui;

        public void Draw(SpriteBatch spritebatch)
        {
            spritebatch.Begin();
            gui.Draw(spritebatch);
            spritebatch.End();
        }

        public void Enter()
        {
            gui.Enter();
        }

        public void Exit()
        {
            gui.Exit();
        }

        public void Initialize(GraphicsDevice GraphicsDevice, ContentManager Content)
        {
            gui = new MenuGuiManager();
            gui.AddButton(new Button(300, 350, 200, 90, "Back to the lobby"));

            void BackToLobby(object source, EventArgs args)
            {
                ScreenManager.PopScreen();
            }

            gui.BackButtonPressed += BackToLobby;
            gui.GetButton(0).Click += BackToLobby;
        }

        public void Update(float dt)
        {
        }
    }
}
