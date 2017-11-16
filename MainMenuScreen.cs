using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace MazeGamePillaPilla
{
    class MainMenuScreen : IScreen
    {
        private MenuGuiManager gui;

        public void Draw(SpriteBatch spritebatch)
        {
            spritebatch.Begin();
            gui.Draw(spritebatch);
            spritebatch.End();
        }

        public void Initialize(GraphicsDevice GraphicsDevice, ContentManager Content)
        {
            gui = new MenuGuiManager();
            gui.AddButton(new Button(100, 100, 230, 110, "create"));
            gui.AddButton(new Button(100, 250, 230, 110, "join"));
            gui.AddButton(new Button(350, 250, 115,  55, "pathfinding"));
            gui.AddButton(new Button(490, 250, 115,  55, "objects"));
            gui.AddButton(new Button(120, 400, 115,  55, "quit game"));

            gui.GetButton(0).NextButtonUp = gui.GetButton(4);
            gui.GetButton(0).NextButtonDown = gui.GetButton(1);
            gui.GetButton(0).Click += (source, args) => ScreenManager.PushScreen(new ServerLobbyScreen(Content));

            gui.GetButton(1).NextButtonUp = gui.GetButton(0);
            gui.GetButton(1).NextButtonDown = gui.GetButton(4);
            gui.GetButton(1).NextButtonRight = gui.GetButton(2);
            gui.GetButton(1).NextButtonLeft = gui.GetButton(3);
            gui.GetButton(1).Click += (source, args) => ScreenManager.PushScreen(new LobbyScreen(Content));

            gui.GetButton(2).NextButtonLeft = gui.GetButton(1);
            gui.GetButton(2).NextButtonRight = gui.GetButton(3);
            gui.GetButton(2).Click += (source, args) => ScreenManager.PushScreen(new PathfinderScreen());

            gui.GetButton(3).NextButtonLeft = gui.GetButton(2);
            gui.GetButton(3).NextButtonRight = gui.GetButton(1);
            gui.GetButton(3).Click += (source, args) => ScreenManager.PushScreen(new ObjectsScreen());

            gui.GetButton(4).NextButtonUp = gui.GetButton(1);
            gui.GetButton(4).NextButtonDown = gui.GetButton(0);
            gui.GetButton(4).Click += (source, args) => System.Environment.Exit(0);
        }

        public void Enter()
        {
            gui.Enter();
        }

        public void Exit()
        {
            gui.Exit();
        }

        public void Update(float dt) {}
    }
}
