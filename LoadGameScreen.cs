﻿using System;
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
        private BiomeData.Biome biome;
        private int i;
        private volatile bool hasFinishedLoading = false;
        private List<LobbyPlayerEventArgs> charactersToInstantiate;
        private object locker = new object();
        private bool readyToStart = false;

        private LoadGameScreen(int PlayersCount, int MapId, int BiomeId)
        {
            ExpectedPlayers = PlayersCount;
            this.MapId = MapId;
            this.biome = BiomeData.GetBiome(BiomeId);
        }


        public LoadGameScreen(int PlayersCount, int MapId, int BiomeId, Client client) : this(PlayersCount, MapId, BiomeId)
        {
            this.client = client;
            gameScreen = new GameScreen(client);
        }


        public LoadGameScreen(int PlayersCount, int MapId, int BiomeId, Client client, Server server) : this(PlayersCount, MapId, BiomeId)
        {
            this.client = client;
            this.server = server;
            gameScreen = new ServerGameScreen(client, server);
        }


        public void Initialize(GraphicsDevice GraphicsDevice, ContentManager Content)
        {
            System.Diagnostics.Debug.WriteLine("Loading...");
            charactersToInstantiate = new List<LobbyPlayerEventArgs>();

            // Tile.Init has to be called on the main thread
            Tile.InitTextures(GraphicsDevice, biome);

            Thread t = new Thread(() =>
            {
                // load maze
                gameScreen.world.maze = Cell.ParseData(MapData.GetMap(MapId));

                // load graphic stuff
                gameScreen.pixel = Content.Load<Texture2D>("pixel");
                Pj.ColliderTexture = Content.Load<Texture2D>("pj");
                Pj.IdleTexture = Content.Load<Texture2D>("pj_idle");
                Pj.RunningTexture = Content.Load<Texture2D>("pj_running");
                Pj.StunnedTexture = Content.Load<Texture2D>("stunned");
                Pj.TeleportingTexture = Content.Load<Texture2D>("teleporting");
                Pj.TestTexture = Content.Load<Texture2D>("t");
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
                InvisiblePowerUp.Icon = Content.Load<Texture2D>("invisible");
                TintaPowerUp.Icon = Content.Load<Texture2D>("tinta");
                TintaSplash.Texture = Content.Load<Texture2D>("tinta_splash");
                ImmunePowerUp.Icon = Content.Load<Texture2D>("immune");
                RandomTeleportPowerUp.Icon = Content.Load<Texture2D>("randomTeleport");
                RelojPowerUp.Icon = Content.Load<Texture2D>("reloj");

                // Initialize rendering stuff
                gameScreen.renderTarget = new RenderTarget2D(GraphicsDevice, GraphicsDevice.PresentationParameters.BackBufferWidth, GraphicsDevice.PresentationParameters.BackBufferHeight);
                gameScreen.renderTargetRectangle = new Rectangle(0, 0, GraphicsDevice.PresentationParameters.BackBufferWidth, GraphicsDevice.PresentationParameters.BackBufferHeight);
                gameScreen.cameraMatrix = Matrix.CreateTranslation(GraphicsDevice.PresentationParameters.BackBufferWidth / 2 - Tile.Size * 11, Tile.Size, 0);
                gameScreen.floorRectangle = new Rectangle(0, 0, gameScreen.world.maze.GetLength(1) * Tile.Size, gameScreen.world.maze.GetLength(0) * Tile.Size);
                gameScreen.floorColor = biome.GroundColor;
                gameScreen.backgroundColor = biome.BackgroundColor;
                gameScreen.biomeTintColor = biome.TintColor;

                // instantiate the characters
                while (gameScreen.world.Pjs.Count < ExpectedPlayers)
                {
                    lock (locker)
                    {
                        foreach (LobbyPlayerEventArgs args in charactersToInstantiate)
                        {
                            switch (args.Type)
                            {
                                case Pj.Type.Local:
                                    LocalPj pj = new LocalPj(args.PlayerID, args.ControllerIndex, args.X, args.Y, 1);
                                    gameScreen.world.Pjs.Add(args.PlayerID, pj);
                                    gameScreen.LocalPlayers.Add(pj);
                                    break;

                                case Pj.Type.Remote:
                                    gameScreen.world.Pjs.Add(args.PlayerID, new RemotePj(args.PlayerID, args.X, args.Y, 1));
                                    break;

                                default:
                                    throw new System.ComponentModel.InvalidEnumArgumentException();
                            }
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
    }
}
