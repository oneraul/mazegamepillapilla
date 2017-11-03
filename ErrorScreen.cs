using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace MazeGamePillaPilla
{
    class ErrorScreen : IScreen
    {
        private MenuGuiManager gui;
        private string errorStr, buttonStr;

        public ErrorScreen(string errorString, string buttonString)
        {
            this.errorStr = errorString;
            this.buttonStr = buttonString;
        }

        public void Draw(SpriteBatch spritebatch)
        {
            spritebatch.Begin();
            spritebatch.DrawString(Button.Font, errorStr, new Vector2(100, 100), Color.BlueViolet);
            gui.Draw(spritebatch);
            spritebatch.End();
        }

        public void Initialize(GraphicsDevice GraphicsDevice, ContentManager Content)
        {
            gui = new MenuGuiManager();
            gui.AddButton(new Button(100, 200, 230, 110, buttonStr));
            gui.GetButton(0).Click += (source, args) => ScreenManager.GoBackToMainMenu();
        }

        public void Enter()
        {
            gui.Enter();
        }

        public void Exit()
        {
            gui.Exit();
        }

        public void Update(float dt) { }
    }
}
