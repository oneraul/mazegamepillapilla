﻿using System;
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
                itemsToInsert.Sort((a, b) => b.GetSortY().CompareTo(a.GetSortY()));

                int mazeW = maze.GetLength(1);
                int mazeH = maze.GetLength(0);
                for (int y = 0; y < mazeH; y++)
                {
                    for (int i = itemsToInsert.Count - 1; i >= 0; i--)
                    {
                        IDrawable item = itemsToInsert[i];
                        Rectangle itemAabb = ((IIntersectable)item).GetAABB();

                        Cell leftCell = maze[y, (int)((itemAabb.Left) / Tile.Size)];
                        Cell rightCell = maze[y, (int)((itemAabb.Right) / Tile.Size)];

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


            spritebatch.GraphicsDevice.Clear(new Color(77f / 255, 174f / 255, 183f / 255));

            spritebatch.Begin(SpriteSortMode.Deferred, null, SamplerState.PointClamp, null, null, null, cameraMatrix);
            spritebatch.Draw(pixel, new Rectangle(0, 0, maze.GetLength(1) * Tile.Size, maze.GetLength(0) * Tile.Size), new Color(182f / 255, 186f / 255, 159f / 255));

            foreach (IDrawable drawable in SortDrawables())
            {
                drawable.Draw(spritebatch, cameraMatrix);
            }

            foreach (Drop drop in Drops)
            {
                drop.Draw(spritebatch, cameraMatrix);
            }

            spritebatch.End();

            spritebatch.Begin(SpriteSortMode.Deferred, null, SamplerState.PointClamp);
            foreach (Pj pj in Pjs.Values)
            {
                if (pj.PowerUp != null)
                {
                    spritebatch.Draw(pixel, new Rectangle(100, 550, 40, 40), Color.Red);
                }
            }
            spritebatch.End();
        }

        public void Enter()
        {
            
        }

        public void Exit()
        {
        }

        public void Initialize(GraphicsDevice GraphicsDevice, ContentManager Content)
        {
            cameraMatrix = Matrix.CreateTranslation(400 - Tile.Size * 11, Tile.Size, 0);

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

            this.Pjs = new Dictionary<string, Pj>();
            Pjs.Add("Player", new TestPj("Player", PlayerControllerIndex.Keyboard, 18.5f * Tile.Size, 2.5f * Tile.Size, 1));

            Drops = new List<Drop>();
            Drops.Add(new SurpriseBoxDrop(270, 305));
        }

        public void Update(float dt)
        {
            foreach (Pj pj in Pjs.Values) pj.Update(dt, maze);

            for (int i = Drops.Count-1; i >= 0; i--)
            {
                Drop drop = Drops[i];
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
        void Draw(SpriteBatch spritebatch, Matrix cameraMatrix);
    }

    class SprintPowerUp : IPowerUp
    {
        public static Texture2D pixel;

        public void Action(Pj pj)
        {
            pj.Buffs.Add(new SprintBuff(pj));
        }

        public void Draw(SpriteBatch spritebatch, Matrix cameraMatrix)
        {
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
        private static float velocityBuffAmount = 500;

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
            spritebatch.Draw(SprintPowerUp.pixel, new Rectangle(100, 100, 100, 100), Color.Red);
        }

        public override void End()
        {
            pj.v -= velocityBuffAmount;
        }
    }

    abstract class Drop : IIntersectable, IDrawable
    {
        private static Vector2[] ProjectionAxes = new Vector2[] { Vector2.UnitX, Vector2.UnitY };

        private Rectangle Aabb;
        private Vector2[] vertices;
        public Action<Pj> Callback { get; private set; }

        protected Drop(int x, int y, int radius, Action<Pj> callback)
        {
            Aabb = new Rectangle(x - radius, y - radius, radius * 2, radius * 2);
            vertices = new Vector2[]
            {
                new Vector2(x+radius, y-radius),
                new Vector2(x+radius, y+radius),
                new Vector2(x-radius, y+radius),
                new Vector2(x-radius, y-radius)
            };

            this.Callback = callback;
        }

        public Rectangle GetAABB()
        {
            return Aabb;
        }

        public Vector2[] GetSatProjectionAxes()
        {
            return ProjectionAxes;
        }

        public Vector2[] GetVertices()
        {
            return vertices;
        }

        public void Draw(SpriteBatch spritebatch, Matrix cameraMatrix)
        {
            spritebatch.Draw(SprintPowerUp.pixel, Aabb, Color.GreenYellow);
        }

        public float GetSortY()
        {
            return Aabb.Center.Y;
        }
    }

    class SurpriseBoxDrop : Drop
    {
        private static int radius = 10;

        public SurpriseBoxDrop(int x, int y) : base(x, y, radius, (pj) =>
        {
            pj.PowerUp = new SprintPowerUp();

        }) {}
    }
}