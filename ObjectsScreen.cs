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

            spritebatch.GraphicsDevice.SetRenderTarget(null);
            spritebatch.GraphicsDevice.Clear(Color.Crimson);
            {
                spritebatch.Begin();

                spritebatch.Draw(renderTarget, renderTargetRectangle, biomeTintColor);

                int i = 0;
                foreach (Pj pj in Pjs.Values)
                {
                    spritebatch.DrawString(Button.Font, pj.ID, new Vector2(100 + i * 50, 520), Color.White);

                    if (pj.PowerUp != null)
                    {
                        pj.PowerUp.Draw(spritebatch, i);
                    }

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
            SprintPowerUp.GuiTexture = Content.Load<Texture2D>("sprint");
            SprintBuff.texture = Content.Load<Texture2D>("circle");
            SurpriseBoxDrop.modelTexture = Content.Load<Texture2D>("surpriseBox");

            this.Pjs = new Dictionary<string, Pj>();
            Pjs.Add("Player", new TestPj("Player", PlayerControllerIndex.Keyboard, 18.5f * Tile.Size, 2.5f * Tile.Size, 1));
            Pjs.Add("Bot1", new AiPj("Bot1", 2.5f * Tile.Size, 2.5f * Tile.Size, 2));
            Pjs.Add("Bot2", new AiPj("Bot2", 2.5f * Tile.Size, 4.5f * Tile.Size, 0));
            Pjs.Add("Bot3", new AiPj("Bot3", 2.5f * Tile.Size, 6.5f * Tile.Size, 3));

            Drops = new List<Drop>();

            Random rng = new Random();
            ScheduleManager.ScheduleInLoop(3, () => SpawnSurpriseBox(rng));

            // Initialize rendering stuff
            renderTarget = new RenderTarget2D(GraphicsDevice, GraphicsDevice.PresentationParameters.BackBufferWidth, GraphicsDevice.PresentationParameters.BackBufferHeight);
            renderTargetRectangle = new Rectangle(0, 0, GraphicsDevice.PresentationParameters.BackBufferWidth, GraphicsDevice.PresentationParameters.BackBufferHeight);
            cameraMatrix = Matrix.CreateTranslation(400 - Tile.Size * 11, Tile.Size, 0);
            floorRectangle = new Rectangle(0, 0, maze.GetLength(1) * Tile.Size, maze.GetLength(0) * Tile.Size);
            floorColor = new Color(182f / 255, 186f / 255, 159f / 255);
            backgroundColor = new Color(77f / 255, 174f / 255, 183f / 255);
            biomeTintColor = Color.White;
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
        void Draw(SpriteBatch spritebatch, int pjIndex);
    }

    class SprintPowerUp : IPowerUp
    {
        public static Texture2D pixel;
        public static Texture2D GuiTexture;

        public void Action(Pj pj)
        {
            pj.Buffs.Add(new SprintBuff(pj));
        }

        public void Draw(SpriteBatch spritebatch, int pjIndex)
        {
            spritebatch.Draw(GuiTexture, new Rectangle(100 + pjIndex * 50, 550, 40, 40), Color.Red);
        }
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
        private static float duration = 5;
        private static float velocityBuffAmount = 250;
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
            spritebatch.Draw(SprintBuff.texture, new Rectangle((int)(pj.x-16), (int)(pj.y-16), 32, 32), Color.Red);
        }

        public override void End()
        {
            pj.v -= velocityBuffAmount;
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
        public static Texture2D modelTexture;
        private static int radius = 10;

        private AnimationFrame model;
        private float rotation;

        public SurpriseBoxDrop(int x, int y) : base(x, y, radius, (pj) =>
        {
            pj.PowerUp = new SprintPowerUp();
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
