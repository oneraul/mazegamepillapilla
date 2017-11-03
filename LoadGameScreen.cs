﻿using System;
using System.Threading;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

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


        internal LoadGameScreen(int PlayersCount, int MapId, Client client) : this(PlayersCount, MapId)
        {
            this.client = client;
            gameScreen = new GameScreen(client);
        }


        internal LoadGameScreen(int PlayersCount, int MapId, Client client, Server server) : this(PlayersCount, MapId)
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
