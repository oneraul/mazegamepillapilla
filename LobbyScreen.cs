using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace MazeGamePillaPilla
{
    class LobbyScreen : IScreen
    {
        protected List<MenuGuiManager> guis;
        protected ContentManager content;
        protected Client client;
        protected Dictionary<string, LobbyPlayer> players;

        private int clientsCount;
        private Texture2D pixel;

        public LobbyScreen(ContentManager Content)
        {
            content = new ContentManager(Content.ServiceProvider, "Content"); // TODO has to be disposed
            pixel = content.Load<Texture2D>("pixel");
            players = new Dictionary<string, LobbyPlayer>();

            int i = 0;
            guis = new List<MenuGuiManager>();
            foreach (KeyValuePair<PlayerControllerIndex, InputController> kvp in Input.Controllers)
            {
                PlayerControllerIndex index = kvp.Key;
                MenuGuiManager gui = new MenuGuiManager(index);
                guis.Add(gui);

                string inputButtonText = (index == PlayerControllerIndex.Keyboard) ? "[Enter]" : "(A)";
                int y = 200 + i * 75;
                Button button = new Button(100, y, 150, 60, $"Press {inputButtonText} to join");
                gui.AddButton(button);
                button.Click += (source, args) =>
                {
                    gui.RemoveButton(button);
                    client.RequestNewPlayer((int)index);
                };

                i++;
            }
        }

        protected virtual void QuitLobby(object gui, EventArgs args)
        {
            client.Close();
            ScreenManager.PopScreen();
        }

        public void Draw(SpriteBatch spritebatch)
        {
            spritebatch.Begin();
            for(int i = 0; i < players.Count; i++)
            {
                Rectangle rect = new Rectangle(100, 100+50*i, 200, 40);
                spritebatch.Draw(pixel, rect, Color.White);
                spritebatch.DrawString(Button.Font, players.ElementAt(i).Value.Name, new Vector2(120, 110 + 50 * i), Color.Black);
            }
            spritebatch.DrawString(Button.Font, clientsCount + " client(s) connected", new Vector2(300, 50), Color.White);
            foreach (MenuGuiManager gui in guis) gui.Draw(spritebatch);
            spritebatch.End();
        }

        public void Update(float dt)
        {
            client.LobbyUpdate(dt);
        }

        public void Initialize(GraphicsDevice GraphicsDevice, ContentManager Content)
        {
            client = new Client();

            // shouldn't these be unsubscribed?
            {
                client.ClientsCount += this.OnClientsCount;
                client.PlayerAdded += this.OnPlayerAdded;
                client.PlayerRemoved += this.OnPlayerRemoved;
                client.PreparingGameToStart += this.OnPreparingGameToStart;
                client.ServerClosed += this.OnDisconnectedFromServer;
            }
        }

        public void Enter()
        {
            foreach (MenuGuiManager gui in guis) gui.Enter();
            guis[0].BackButtonPressed += QuitLobby;
        }

        public void Exit()
        {
            foreach (MenuGuiManager gui in guis) gui.Exit();
            guis[0].BackButtonPressed -= QuitLobby;
        }

        public void OnClientsCount(object source, LobbyClientsCountEventArgs args)
        {
            clientsCount = args.ClientsCount;
        }

        public void OnPlayerAdded(object source, LobbyPlayerEventArgs args)
        {
            players.Add(args.PlayerID, new LobbyPlayer(args.PlayerID, args.ControllerIndex) { Name = args.PlayerID });
        }

        public void OnPlayerRemoved(object source, LobbyPlayerEventArgs args)
        {
            players.Remove(args.PlayerID);
        }

        public virtual void OnPreparingGameToStart(object source, LobbyOptionsArgs args)
        {
            ScreenManager.PushScreen(new LoadGameScreen(args.PlayersCount, args.Map, args.Biome, client));
        }

        public void OnDisconnectedFromServer(object source, EventArgs args)
        {
            client.Close();
            ScreenManager.ReplaceCurrent(
                new ErrorScreen("You've been disconned from the server.", "Exit to main menu"));
        }
    }


    class ServerLobbyScreen : LobbyScreen, IScreen
    {
        public Server Server { get; private set; }

        public ServerLobbyScreen(ContentManager Content) : base(Content)
        {
            Server = new Server();

            Button startGameButton = new Button(10, 10, 100, 80, "Start game");
            guis[0].AddButton(startGameButton);
            startGameButton.Click += (source, args) => client.RequestStartGame();

            guis[0].GetButton(0).NextButtonDown = guis[0].GetButton(0).NextButtonUp = startGameButton;
            startGameButton.NextButtonDown = startGameButton.NextButtonUp = guis[0].GetButton(0);
        }

        protected override void QuitLobby(object gui, EventArgs args)
        {
            Server.Close();
            base.QuitLobby(gui, args);
        }

        public new void Update(float dt)
        {
            Server.LobbyUpdate(dt);
            base.Update(dt);
        }

        public override void OnPreparingGameToStart(object source, LobbyOptionsArgs args)
        {
            ScreenManager.PushScreen(new LoadGameScreen(args.PlayersCount, args.Map, args.Biome, client, Server));
        }
    }
}
