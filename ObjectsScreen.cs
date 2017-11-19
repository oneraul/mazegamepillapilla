using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

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

            Random rng = new Random();
            ScheduleManager.ScheduleInLoop(3, () => SpawnSurpriseBox(rng));

            // Initialize rendering stuff
            renderTarget = new RenderTarget2D(GraphicsDevice, GraphicsDevice.PresentationParameters.BackBufferWidth, GraphicsDevice.PresentationParameters.BackBufferHeight);
            renderTargetRectangle = new Rectangle(0, 0, GraphicsDevice.PresentationParameters.BackBufferWidth, GraphicsDevice.PresentationParameters.BackBufferHeight);
            cameraMatrix = Matrix.CreateTranslation(GraphicsDevice.PresentationParameters.BackBufferWidth/2 - Tile.Size * 11, Tile.Size, 0);
            floorRectangle = new Rectangle(0, 0, maze.GetLength(1) * Tile.Size, maze.GetLength(0) * Tile.Size);
            floorColor = new Color(182f / 255, 186f / 255, 159f / 255);
            backgroundColor = new Color(77f / 255, 174f / 255, 183f / 255);
            biomeTintColor = Color.Wheat;
        }

        private void SpawnSurpriseBox(Random rng)
        {
            int x = rng.Next(2 * Tile.Size, (maze.GetLength(1) - 1) * Tile.Size);
            int y = rng.Next(2 * Tile.Size, (maze.GetLength(0) - 1) * Tile.Size);
            SurpriseBoxDrop box = new SurpriseBoxDrop(x, y);

            bool isFree(SurpriseBoxDrop drop)
            {
                foreach (Vector2 vertex in drop.GetVertices())
                {
                    int currentCellX = (int)(vertex.X / Tile.Size);
                    int currentCellY = (int)(vertex.Y / Tile.Size);

                    if (currentCellX >= 0 && currentCellX < maze.GetLength(1)
                    && currentCellY >= 0 && currentCellY < maze.GetLength(0))
                    {
                        Cell cell = maze[currentCellY, currentCellX];
                        if (drop.AabbAabbIntersectionTest(cell))
                        {
                            if (drop.SatIntersectionTest(cell))
                            {
                                return false;
                            }
                        }
                    }
                }

                return true;
            }

            int iteraciones = 0;
            while (!isFree(box))
            {
                iteraciones++;
                x = rng.Next(2 * Tile.Size, (maze.GetLength(1) - 1) * Tile.Size);
                y = rng.Next(2 * Tile.Size, (maze.GetLength(0) - 1) * Tile.Size);
                box.SetPosition(x, y);
            }
            Drops.Add(new SurpriseBoxDrop(x, y));
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


    interface IPowerUp
    {
        void Action(Pj pj);
        Texture2D GetIcon();
        Color GetColor();
    }

    class SprintPowerUp : IPowerUp
    {
        public static Texture2D pixel;
        public static Texture2D Icon;

        public void Action(Pj pj)
        {
            pj.Buffs.Add(new SprintBuff(pj));
        }

        public Texture2D GetIcon() => Icon;
        public Color GetColor() => Color.Crimson;
    }

    class TraverseWallsPowerUp : IPowerUp
    {
        public static Texture2D Icon;

        public void Action(Pj pj)
        {
            pj.Buffs.Add(new TraverseWallsBuff(pj));
        }

        public Texture2D GetIcon() => Icon;
        public Color GetColor() => Color.Aqua;
    }

    abstract class Buff
    {
        public bool ToBeRemoved { get; protected set; }

        public virtual void Activate() {}
        public virtual void Update(float dt) {}
        public virtual void End() {}
        public virtual void Draw(SpriteBatch spritebatch, Matrix cameraMatrix) {}
    }

    abstract class DurationBuff : Buff
    {
        public float Duration { get; private set; }
        protected float Timer { get; private set; }

        protected DurationBuff(float duration)
        {
            Duration = duration;
            Timer = 0;
        }

        public override void Update(float dt)
        {
            Timer += dt;
            if (Timer >= Duration)
            {
                ToBeRemoved = true;
            }
        }
    }

    class SprintBuff : DurationBuff
    {
        private static float duration = 2.5f;
        private static float velocityBuffAmount = 300;
        public static Texture2D texture;

        private Pj pj;

        public SprintBuff(Pj pj) : base(duration)
        {
            this.pj = pj;
            this.Activate();
        }

        public override void Activate()
        {
            pj.v += velocityBuffAmount;
        }

        public override void Draw(SpriteBatch spritebatch, Matrix cameraMatrix)
        {
            spritebatch.Draw(SprintBuff.texture, new Rectangle((int)(pj.x - 16), (int)(pj.y - 16), 32, 32), Color.Crimson);
        }

        public override void End()
        {
            pj.v -= velocityBuffAmount;
        }
    }

    class TraverseWallsBuff : DurationBuff
    {
        private static float duration = 1.5f;
        public static Texture2D texture;

        private Pj pj;

        public TraverseWallsBuff(Pj pj) : base(duration)
        {
            this.pj = pj;
            this.Activate();
        }

        public override void Activate()
        {
            pj.CanTraverseWalls = true;
        }

        public override void Draw(SpriteBatch spritebatch, Matrix cameraMatrix)
        {
            spritebatch.Draw(SprintBuff.texture, new Rectangle((int)(pj.x - 16), (int)(pj.y - 16), 32, 32), Color.Aqua);
        }

        public override void End()
        {
            pj.CanTraverseWalls = false;
        }
    }

    abstract class Drop : IIntersectable, IDrawable
    {
        private static Vector2[] ProjectionAxes = new Vector2[] { Vector2.UnitX, Vector2.UnitY };

        public Action<Pj> Callback { get; private set; }
        private int x;
        private int y;
        private int radius;

        protected Drop(int x, int y, int radius, Action<Pj> callback)
        {
            this.x = x;
            this.y = y;
            this.radius = radius;
            this.Callback = callback;
        }

        public void SetPosition(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public Rectangle GetAABB()
        {
            return new Rectangle(x - radius, y - radius, radius * 2, radius * 2);
        }

        public Vector2[] GetSatProjectionAxes()
        {
            return ProjectionAxes;
        }

        public Vector2[] GetVertices()
        {
            return new Vector2[]
            {
                new Vector2(x+radius, y-radius),
                new Vector2(x+radius, y+radius),
                new Vector2(x-radius, y+radius),
                new Vector2(x-radius, y-radius)
            };
        }

        public abstract void Draw(SpriteBatch spritebatch, Matrix cameraMatrix);

        public float GetSortY()
        {
            return y;
        }

        public virtual void Update(float dt) {}
    }

    class SurpriseBoxDrop : Drop
    {
        private static Random rng = new Random();

        public static Texture2D modelTexture;
        private static int radius = 10;

        private AnimationFrame model;
        private float rotation;

        public SurpriseBoxDrop(int x, int y) : base(x, y, radius, (pj) =>
        {
            int roll = rng.Next(2);
            switch (roll)
            {
                case 0: pj.PowerUp = new SprintPowerUp(); break;
                case 1: pj.PowerUp = new TraverseWallsPowerUp(); break;
            }
        })
        {
            int layers = 16;
            int side = 16;
            model = new AnimationFrame(layers);
            for (int i = 0; i < layers; i++)
            {
                model.Rectangles[i] = new Rectangle(i * layers, 0, side, side);
            }
        }

        public override void Draw(SpriteBatch spritebatch, Matrix cameraMatrix)
        {
            spritebatch.Draw(SprintPowerUp.pixel, GetAABB(), Color.GreenYellow);
            model.Draw(modelTexture, spritebatch, GetAABB().Center.X, GetAABB().Center.Y, rotation, 1, 8, 8);
        }

        public override void Update(float dt)
        {
            rotation = (float)((rotation + dt) % (Math.PI * 2));
        }
    }
}
