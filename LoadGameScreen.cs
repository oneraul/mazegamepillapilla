using System;
using System.Threading;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MazeGamePillaPilla.PowerUps;

namespace MazeGamePillaPilla
{
    class LoadGameScreen : IScreen
    {
        private GameScreen gameScreen;
        private Client client;
        private Server server;
        private int ExpectedPlayers;
        private int MapId;
        private int i;
        private volatile bool hasFinishedLoading = false;
        private List<LobbyPlayerEventArgs> charactersToInstantiate;
        private object locker = new object();
        private bool readyToStart = false;

        private LoadGameScreen(int PlayersCount, int MapId)
        {
            ExpectedPlayers = PlayersCount;
            this.MapId = MapId;
        }


        public LoadGameScreen(int PlayersCount, int MapId, Client client) : this(PlayersCount, MapId)
        {
            this.client = client;
            gameScreen = new GameScreen(client);
        }


        public LoadGameScreen(int PlayersCount, int MapId, Client client, Server server) : this(PlayersCount, MapId)
        {
            this.client = client;
            this.server = server;
            gameScreen = new ServerGameScreen(client, server);

            // these should be moved to be done serverside
            {
                server.game = gameScreen;
                server.lastProcessedInputs = new Dictionary<string, long>();
                server.lastSentSnapshots = new Dictionary<string, long>();
            }
        }


        public void Initialize(GraphicsDevice GraphicsDevice, ContentManager Content)
        {
            System.Diagnostics.Debug.WriteLine("Loading...");
            charactersToInstantiate = new List<LobbyPlayerEventArgs>();

            // Tile.Init has to be called on the main thread
            Tile.Init(GraphicsDevice);

            Thread t = new Thread(() =>
            {
                // load maze
                gameScreen.maze = Cell.ParseData(Content, MapData.GetMap(MapId));

                // load graphic stuff
                gameScreen.pixel = Content.Load<Texture2D>("pixel");
                Pj.ColliderTexture = Content.Load<Texture2D>("pj");
                Pj.IdleTexture = Content.Load<Texture2D>("pj_idle");
                Pj.RunningTexture = Content.Load<Texture2D>("pj_running");
                Pj.PaletteTexture = Content.Load<Texture2D>("pj_palette");
                Pj.effect = Content.Load<Effect>("pj_shader");
                Pj.effect.Parameters["u_lut"].SetValue(Pj.PaletteTexture);
                SprintPowerUp.pixel = Content.Load<Texture2D>("pixel");
                SprintPowerUp.Icon = Content.Load<Texture2D>("sprint");
                TraverseWallsPowerUp.Icon = Content.Load<Texture2D>("traverseWalls");
                SprintBuff.texture = Content.Load<Texture2D>("circle");
                TraverseWallsBuff.texture = Content.Load<Texture2D>("circle");
                SurpriseBoxDrop.modelTexture = Content.Load<Texture2D>("surpriseBox");
                BananaPowerUp.Icon = Content.Load<Texture2D>("bananaIcon");
                BananaDrop.modelTexture = Content.Load<Texture2D>("bananaDrop");

                // Initialize rendering stuff
                gameScreen.renderTarget = new RenderTarget2D(GraphicsDevice, GraphicsDevice.PresentationParameters.BackBufferWidth, GraphicsDevice.PresentationParameters.BackBufferHeight);
                gameScreen.renderTargetRectangle = new Rectangle(0, 0, GraphicsDevice.PresentationParameters.BackBufferWidth, GraphicsDevice.PresentationParameters.BackBufferHeight);
                gameScreen.cameraMatrix = Matrix.CreateTranslation(GraphicsDevice.PresentationParameters.BackBufferWidth / 2 - Tile.Size * 11, Tile.Size, 0);
                gameScreen.floorRectangle = new Rectangle(0, 0, gameScreen.maze.GetLength(1) * Tile.Size, gameScreen.maze.GetLength(0) * Tile.Size);
                gameScreen.floorColor = new Color(182f / 255, 186f / 255, 159f / 255);
                gameScreen.backgroundColor = new Color(77f / 255, 174f / 255, 183f / 255);
                gameScreen.biomeTintColor = Color.Wheat;

                // instantiate the characters
                while (gameScreen.Pjs.Count < ExpectedPlayers)
                {
                    lock (locker)
                    {
                        foreach (LobbyPlayerEventArgs args in charactersToInstantiate)
                        {
                            InstantiateCharacter(args);
                        }

                        charactersToInstantiate.Clear();
                    }
                }

                Thread.Sleep(500);
                hasFinishedLoading = true;
            });

            t.Start();
        }


        public void Update(float dt)
        {
            server?.LobbyUpdate(dt);
            client.LobbyUpdate(dt);

            if (hasFinishedLoading && !readyToStart)
            {
                System.Diagnostics.Debug.WriteLine("...finished");
                client.SignalReadyToStart();
                readyToStart = true;
            }
        }


        public void Draw(SpriteBatch spritebatch)
        {
            i++;

            spritebatch.Begin();
            spritebatch.DrawString(Button.Font, "Loading... " + i, new Vector2(100, 100), Color.White);
            spritebatch.End();
        }


        public void Enter()
        {
            client.GameStarted += this.OnGameStarted;
            client.InstantiateCharacter += this.OnCharacterInstantiated;
        }


        public void Exit()
        {
            client.GameStarted -= this.OnGameStarted;
            client.InstantiateCharacter -= this.OnCharacterInstantiated;
        }


        public void OnGameStarted(object source, EventArgs args)
        {
            ScreenManager.ReplaceCurrent(gameScreen);
        }


        public void OnCharacterInstantiated(object source, LobbyPlayerEventArgs args)
        {
            lock (locker) charactersToInstantiate.Add(args);
        }

        private void InstantiateCharacter(LobbyPlayerEventArgs args)
        {
            switch (args.Type)
            {
                case Pj.Type.Local:
                    LocalPj pj = new LocalPj(args.PlayerID, args.ControllerIndex, args.X, args.Y, 1);
                    gameScreen.Pjs.Add(args.PlayerID, pj);
                    gameScreen.LocalPlayers.Add(pj);
                    server?.lastProcessedInputs.Add(args.PlayerID, 0);
                    server?.lastSentSnapshots.Add(args.PlayerID, 0);
                    break;

                case Pj.Type.Remote:
                    gameScreen.Pjs.Add(args.PlayerID, new RemotePj(args.PlayerID, args.X, args.Y, 1));
                    server?.lastProcessedInputs.Add(args.PlayerID, 0);
                    server?.lastSentSnapshots.Add(args.PlayerID, 0);
                    break;

                throw new Exception();
            }
        }
    }
}
