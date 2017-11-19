using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MazeGamePillaPilla.PowerUps;

namespace MazeGamePillaPilla
{
    class GameScreen : IScreen
    {
        public Matrix cameraMatrix;
        public Cell[,] maze;
        public Dictionary<int, Drop> Drops;
        public Texture2D pixel;
        public RenderTarget2D renderTarget;
        public Rectangle renderTargetRectangle;
        public Rectangle floorRectangle;
        public Color floorColor;
        public Color backgroundColor;
        public Color biomeTintColor;

        public Dictionary<string, Pj> Pjs;
        public List<LocalPj> LocalPlayers;

        private Client client;

        public GameScreen(Client client)
        {
            this.client = client;
            Pjs = new Dictionary<string, Pj>();
            LocalPlayers = new List<LocalPj>();
            Drops = new Dictionary<int, Drop>();
        }


        public void Initialize(GraphicsDevice GraphicsDevice, ContentManager Content)
        {
            System.Diagnostics.Debug.WriteLine("Enters GameScreen");
        }

        public void Enter()
        {
            client.CharacterUpdated += OnCharacterUpdated;
            client.DropAdded += OnDropAdded;
            client.DropRemoved += OnDropRemoved;
            client.BuffAdded += OnBuffAdded;
            client.BuffRemoved += OnBuffRemoved;
            client.PowerUpAdded += OnPowerUpAdded;
            client.PowerUpRemoved += OnPowerUpRemoved;
        }

        public void Exit()
        {
            client.CharacterUpdated -= OnCharacterUpdated;
            client.DropAdded -= OnDropAdded;
            client.DropRemoved -= OnDropRemoved;
            client.BuffAdded -= OnBuffAdded;
            client.BuffRemoved -= OnBuffRemoved;
            client.PowerUpAdded -= OnPowerUpAdded;
            client.PowerUpRemoved -= OnPowerUpRemoved;
        }


        public void OnCharacterUpdated(object source, GameplayUpdateEventArgs args)
        {
            Pjs[args.Packet.CharacterID].ProcessServerUpdate(args.Packet, maze);
        }

        public void OnDropAdded(object source, GameplayDropEventArgs args)
        {
            Drop drop = null;
            switch (args.Type)
            {
                case (int)DropTypes.SurpriseBoxDrop:
                    drop = new SurpriseBoxDrop(args.X, args.Y);
                    break;

                case (int)DropTypes.BananaDrop:
                    drop = new BananaDrop(args.X, args.Y);
                    break;
            }
            System.Diagnostics.Debug.Assert(drop != null);
            Drops.Add(args.Id, drop);
        }

        public void OnDropRemoved(object source, GameplayDropEventArgs args)
        {
            Drops.Remove(args.Id);
        }

        public void OnBuffAdded(object source, GameplayBuffEventArgs args)
        {
            Pjs.TryGetValue(args.PlayerId, out Pj pj);
            System.Diagnostics.Debug.Assert(pj != null);
            switch(args.BuffId)
            {
                case (int)BuffTypes.SprintBuff:
                    pj.Buffs.Add(new SprintBuff(pj));
                    break;

                case (int)BuffTypes.TraverseWallsBuff:
                    pj.Buffs.Add(new TraverseWallsBuff(pj));
                    break;

                case (int)BuffTypes.BananaStunBuff:
                    pj.Buffs.Add(new BananaStunBuff(pj));
                    break;

                throw new System.Exception();
            }
        }

        public void OnBuffRemoved(object source, GameplayBuffEventArgs args)
        {
            Pjs.TryGetValue(args.PlayerId, out Pj pj);
            System.Diagnostics.Debug.Assert(pj != null);
            if (args.BuffId < pj.Buffs.Count && pj.Buffs[args.BuffId] != null)
            {
                pj.Buffs[args.BuffId].End();
                pj.Buffs.RemoveAt(args.BuffId);
            }
        }

        public void OnPowerUpAdded(object source, GameplayPowerUpEventArgs args)
        {
            Pjs.TryGetValue(args.PlayerId, out Pj pj);
            System.Diagnostics.Debug.Assert(pj != null);
            if (pj != null)
            {
                switch (args.Type)
                {
                    case (int)PowerUpTypes.SprintPowerUp:
                        pj.PowerUp = new SprintPowerUp();
                        break;

                    case (int)PowerUpTypes.TraverseWallsPowerUp:
                        pj.PowerUp = new TraverseWallsPowerUp();
                        break;

                    case (int)PowerUpTypes.BananaPowerUp:
                        pj.PowerUp = new BananaPowerUp();
                        break;
                }
            }
        }

        public void OnPowerUpRemoved(object source, GameplayPowerUpEventArgs args)
        {
            Pjs.TryGetValue(args.PlayerId, out Pj pj);
            if (pj != null) pj.PowerUp = null;
        }

        public void Update(float dt)
        {
            client.GameplayUpdate(dt);
            foreach (LocalPj pj in LocalPlayers) pj.ProcessInput(dt, client, maze);
            foreach (Pj pj in Pjs.Values) pj.Update(dt, maze);
            foreach (Drop drop in Drops.Values) drop.Update(dt);
        }

        public void Draw(SpriteBatch spritebatch)
        {
            List<IDrawable> SortDrawables()
            {
                List<IDrawable> drawables = new List<IDrawable>();

                List<IDrawable> itemsToInsert = new List<IDrawable>();
                itemsToInsert.AddRange(Pjs.Values);
                itemsToInsert.AddRange(Drops.Values);
                itemsToInsert.Sort((a, b) => b.GetSortY().CompareTo(a.GetSortY()));

                int mazeW = maze.GetLength(1);
                int mazeH = maze.GetLength(0);
                for (int y = 0; y < mazeH; y++)
                {
                    for (int i = itemsToInsert.Count - 1; i >= 0; i--)
                    {
                        IDrawable item = itemsToInsert[i];
                        Rectangle itemAabb = ((IIntersectable)item).GetAABB();

                        Cell leftCell = maze[y, itemAabb.Left / Tile.Size];
                        Cell rightCell = maze[y, itemAabb.Right / Tile.Size];

                        if (item.GetSortY() < leftCell.GetSortY() && item.GetSortY() < rightCell.GetSortY())
                        {
                            drawables.Add(item);
                            itemsToInsert.RemoveAt(i);
                        }
                    }

                    for (int x = 0; x < mazeW; x++)
                    {
                        drawables.Add(maze[y, x]);
                    }
                }

                return drawables;
            }


            // Draw the game onto a texture
            spritebatch.GraphicsDevice.SetRenderTarget(renderTarget);
            {
                spritebatch.GraphicsDevice.Clear(backgroundColor);

                spritebatch.Begin(SpriteSortMode.Deferred, null, SamplerState.PointClamp, null, null, null, cameraMatrix);
                spritebatch.Draw(pixel, floorRectangle, floorColor);

                foreach (IDrawable drawable in SortDrawables())
                {
                    drawable.Draw(spritebatch, cameraMatrix);
                }

                spritebatch.End();
            }

            // Post-process the texture and draw it to the back buffer
            spritebatch.GraphicsDevice.SetRenderTarget(null);
            spritebatch.GraphicsDevice.Clear(Color.Crimson);
            {
                spritebatch.Begin();
                spritebatch.Draw(renderTarget, renderTargetRectangle, biomeTintColor);
                spritebatch.End();
            }

            // Draw extra gameplay elements (projected into world space but not affected by post-processing)
            {
                spritebatch.Begin(SpriteSortMode.Deferred, null, SamplerState.PointClamp, null, null, null, cameraMatrix);
                foreach (Pj pj in Pjs.Values)
                {
                    spritebatch.DrawString(Button.Font, pj.ID, new Vector2(pj.x - Button.Font.MeasureString(pj.ID).X / 2, pj.y - 40), Color.White);
                }
                spritebatch.End();
            }

            // Draw GUI
            {
                spritebatch.Begin();

                int screenWidth = spritebatch.GraphicsDevice.PresentationParameters.BackBufferWidth;
                int portraitWidth = 80;
                int portraitPadding = 15; // the separation from one portrait to the next
                int sideMargin = (screenWidth - (portraitWidth + portraitPadding) * Pjs.Count) / 2; // the distance from the side of the screen to the first portrait
                int y = spritebatch.GraphicsDevice.PresentationParameters.BackBufferHeight - portraitWidth - 5;

                int i = 0;
                foreach (Pj pj in Pjs.Values)
                {
                    spritebatch.Draw(pixel, new Rectangle(sideMargin + i * (portraitWidth + portraitPadding), y, portraitWidth, portraitWidth), new Color(Color.Black, 0.16f));
                    if (pj.PowerUp != null) spritebatch.Draw(pj.PowerUp.GetIcon(),
                        new Rectangle(sideMargin + i * (portraitWidth + portraitPadding) + 5, y + 5, portraitWidth - 10, portraitWidth - 10), pj.PowerUp.GetColor());

                    spritebatch.DrawString(Button.Font, pj.ID, new Vector2(sideMargin + i * (portraitWidth + portraitPadding), y), Color.White);

                    i++;
                }

                spritebatch.End();
            }
        }
    }

    class ServerGameScreen : GameScreen, IScreen
    {
        private Server server;

        public ServerGameScreen(Client client, Server server) : base(client)
        {
            this.server = server;
        }

        public new void Enter()
        {
            Input.Pressed += OnBackPressed;
            base.Enter();

            ScheduleManager.ScheduleInLoop(3, () => {
                Point spawnPosition = SurpriseBoxDrop.SpawnInAnEmptyPosition(maze);
                server.AddDrop((int)DropTypes.SurpriseBoxDrop, spawnPosition.Y, spawnPosition.Y);
            });
        }

        public new void Exit()
        {
            Input.Pressed -= OnBackPressed;
            base.Exit();
        }

        public new void Update(float dt)
        {
            server.GameplayUpdate(dt);
            base.Update(dt);
        }

        public new void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);

            spriteBatch.Begin(SpriteSortMode.Deferred, null, SamplerState.PointClamp, null, null, null, cameraMatrix);
            spriteBatch.DrawString(Button.Font, "SERVER", new Vector2(20, 20), Color.Red);
            spriteBatch.End();
        }

        protected void OnBackPressed(object source, InputStateEventArgs args)
        {
            if ((args.PlayerIndex == PlayerControllerIndex.Keyboard || args.PlayerIndex == PlayerControllerIndex.One)
            && args.Button == InputKeys.Back)
            {
                server.GoToScoresScreen();
            }
        }
    }
}
