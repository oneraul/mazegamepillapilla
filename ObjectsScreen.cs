using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MazeGamePillaPilla.PowerUps;

namespace MazeGamePillaPilla
{
    class ObjectsScreen : IScreen
    {
        private Cell[,] maze;
        private Texture2D pixel;
        private RenderTarget2D renderTarget;
        private Rectangle renderTargetRectangle;
        private Rectangle floorRectangle;
        private Color floorColor;
        private Color backgroundColor;
        private Color biomeTintColor;
        private Matrix cameraMatrix;
        private Dictionary<string, Pj> Pjs;
        private List<Drop> Drops;

        public void Draw(SpriteBatch spritebatch)
        {
            List<IDrawable> SortDrawables()
            {
                List<IDrawable> drawables = new List<IDrawable>();

                List<IDrawable> itemsToInsert = new List<IDrawable>();
                itemsToInsert.AddRange(Pjs.Values);
                itemsToInsert.AddRange(Drops);
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

        public void Enter() {}

        public void Exit() {}

        public void Initialize(GraphicsDevice GraphicsDevice, ContentManager Content)
        {
            Tile.Init(GraphicsDevice);

            // load maze
            int MapId = 0;
            maze = Cell.ParseData(Content, MapData.GetMap(MapId));

            // load graphic stuff
            pixel = Content.Load<Texture2D>("pixel");
            Pj.ColliderTexture = Content.Load<Texture2D>("pj");
            Pj.IdleTexture = Content.Load<Texture2D>("pj_idle");
            Pj.RunningTexture = Content.Load<Texture2D>("pj_running");
            Pj.PaletteTexture = Content.Load<Texture2D>("pj_palette");
            Pj.effect = Content.Load<Effect>("pj_shader");
            Pj.effect.Parameters["u_lut"].SetValue(Pj.PaletteTexture);
            SprintPowerUp.pixel = pixel;
            SprintPowerUp.Icon = Content.Load<Texture2D>("sprint");
            TraverseWallsPowerUp.Icon = Content.Load<Texture2D>("traverseWalls");
            SprintBuff.texture = Content.Load<Texture2D>("circle");
            TraverseWallsBuff.texture = Content.Load<Texture2D>("circle");
            SurpriseBoxDrop.modelTexture = Content.Load<Texture2D>("surpriseBox");

            this.Pjs = new Dictionary<string, Pj>();
            Pjs.Add("Player", new TestPj("Player", PlayerControllerIndex.Keyboard, 18.5f * Tile.Size, 2.5f * Tile.Size, 1));
            Pjs.Add("Bot1", new AiPj("Bot1", 2.5f * Tile.Size, 2.5f * Tile.Size, 2));
            Pjs.Add("Bot2", new AiPj("Bot2", 2.5f * Tile.Size, 4.5f * Tile.Size, 0));
            Pjs.Add("Bot3", new AiPj("Bot3", 2.5f * Tile.Size, 6.5f * Tile.Size, 3));
            Pjs.Add("Bot4", new AiPj("Bot4", 2.5f * Tile.Size, 8.5f * Tile.Size, 0));

            Drops = new List<Drop>();
            ScheduleManager.ScheduleInLoop(3, () => Drops.Add(SurpriseBoxDrop.SpawnInAnEmptyPosition(maze)));

            // Initialize rendering stuff
            renderTarget = new RenderTarget2D(GraphicsDevice, GraphicsDevice.PresentationParameters.BackBufferWidth, GraphicsDevice.PresentationParameters.BackBufferHeight);
            renderTargetRectangle = new Rectangle(0, 0, GraphicsDevice.PresentationParameters.BackBufferWidth, GraphicsDevice.PresentationParameters.BackBufferHeight);
            cameraMatrix = Matrix.CreateTranslation(GraphicsDevice.PresentationParameters.BackBufferWidth/2 - Tile.Size * 11, Tile.Size, 0);
            floorRectangle = new Rectangle(0, 0, maze.GetLength(1) * Tile.Size, maze.GetLength(0) * Tile.Size);
            floorColor = new Color(182f / 255, 186f / 255, 159f / 255);
            backgroundColor = new Color(77f / 255, 174f / 255, 183f / 255);
            biomeTintColor = Color.Wheat;
        }

        public void Update(float dt)
        {
            foreach (Pj pj in Pjs.Values) pj.Update(dt, maze);

            for (int i = Drops.Count-1; i >= 0; i--)
            {
                Drop drop = Drops[i];

                drop.Update(dt);

                foreach (Pj pj in Pjs.Values)
                {
                    if (drop.AabbAabbIntersectionTest(pj))
                    {
                        drop.Callback(pj);
                        Drops.RemoveAt(i);
                        break;
                    }
                }
            }
        }
    }
}
