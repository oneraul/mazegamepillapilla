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
        public Texture2D pixel;
        public RenderTarget2D renderTarget;
        public Rectangle renderTargetRectangle;
        public Rectangle floorRectangle;
        public Color floorColor;
        public Color backgroundColor;
        public Color biomeTintColor;

        public GameWorld world;
        public List<LocalPj> LocalPlayers;

        private Client client;

        public GameScreen(Client client)
        {
            this.client = client;
            this.world = new GameWorld();
            this.LocalPlayers = new List<LocalPj>();
        }


        public void Initialize(GraphicsDevice GraphicsDevice, ContentManager Content)
        {
            System.Diagnostics.Debug.WriteLine("Enters GameScreen");
        }

        public void Enter()
        {
            client.CharacterUpdated += world.OnCharacterUpdated;
            client.DropAdded += world.OnDropAdded;
            client.DropRemoved += world.OnDropRemoved;
            client.BuffAdded += world.OnBuffAdded;
            client.BuffRemoved += world.OnBuffRemoved;
            client.PowerUpAdded += world.OnPowerUpAdded;
            client.PowerUpRemoved += world.OnPowerUpRemoved;
        }

        public void Exit()
        {
            client.CharacterUpdated -= world.OnCharacterUpdated;
            client.DropAdded -= world.OnDropAdded;
            client.DropRemoved -= world.OnDropRemoved;
            client.BuffAdded -= world.OnBuffAdded;
            client.BuffRemoved -= world.OnBuffRemoved;
            client.PowerUpAdded -= world.OnPowerUpAdded;
            client.PowerUpRemoved -= world.OnPowerUpRemoved;
        }

        public void Update(float dt)
        {
            client.GameplayUpdate(dt);
            foreach (LocalPj pj in LocalPlayers) pj.ProcessInput(dt, client, world.maze);
            foreach (Pj pj in world.Pjs.Values) pj.Update(dt, world.maze);
            foreach (Drop drop in world.Drops.Values) drop.Update(dt);
        }

        public void Draw(SpriteBatch spritebatch)
        {
            List<IDrawable> SortDrawables()
            {
                List<IDrawable> drawables = new List<IDrawable>();

                List<IDrawable> itemsToInsert = new List<IDrawable>();
                itemsToInsert.AddRange(world.Pjs.Values);
                itemsToInsert.AddRange(world.Drops.Values);
                itemsToInsert.Sort((a, b) => b.GetSortY().CompareTo(a.GetSortY()));

                int mazeW = world.maze.GetLength(1);
                int mazeH = world.maze.GetLength(0);
                for (int y = 0; y < mazeH; y++)
                {
                    for (int i = itemsToInsert.Count - 1; i >= 0; i--)
                    {
                        IDrawable item = itemsToInsert[i];
                        Rectangle itemAabb = ((IIntersectable)item).GetAABB();

                        Cell leftCell = world.maze[y, itemAabb.Left / Tile.Size];
                        Cell rightCell = world.maze[y, itemAabb.Right / Tile.Size];

                        if (item.GetSortY() < leftCell.GetSortY() && item.GetSortY() < rightCell.GetSortY())
                        {
                            drawables.Add(item);
                            itemsToInsert.RemoveAt(i);
                        }
                    }

                    for (int x = 0; x < mazeW; x++)
                    {
                        drawables.Add(world.maze[y, x]);
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
                foreach (Pj pj in world.Pjs.Values)
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
                int sideMargin = (screenWidth - (portraitWidth + portraitPadding) * world.Pjs.Count) / 2; // the distance from the side of the screen to the first portrait
                int y = spritebatch.GraphicsDevice.PresentationParameters.BackBufferHeight - portraitWidth - 5;

                int i = 0;
                foreach (Pj pj in world.Pjs.Values)
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
                Point spawnPosition = SurpriseBoxDrop.SpawnInAnEmptyPosition(world.maze);
                server.AddDrop((int)DropTypes.SurpriseBoxDrop, spawnPosition.X, spawnPosition.Y);
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
