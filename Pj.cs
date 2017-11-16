﻿using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MazeGamePillaPilla
{
    abstract class Pj : IDrawable, IIntersectable
    {
        internal enum Type { Local, Remote, Bot }


        private static Vector2[] ProjectionAxes = new Vector2[] { Vector2.UnitX, Vector2.UnitY };
        public static Texture2D ColliderTexture;
        public static Texture2D IdleTexture;
        public static Texture2D RunningTexture;
        public static Texture2D PaletteTexture;
        public static Effect effect;

        private EffectParameter paletteParameter;
        private EffectPass effectPass;

        internal string ID;
        internal float x;
        internal float y;
        internal float v = 150;
        internal float rotation = MathHelper.PiOver2;   // in radians, range [0, 2*pi)
        internal float hw = (0.5f * Tile.Size) / 2;
        internal float hh = (0.5f * Tile.Size) / 2;

        internal int palette;
        internal Animation[] Animations;
        internal Animation currentAnimation;

        public IPowerUp PowerUp;
        public List<Buff> Buffs;


        protected enum AnimationID { Idle, Running }


        protected Pj(string ID, float x, float y, int palette)
        {
            this.ID = ID;
            this.x = x;
            this.y = y;

            this.palette = palette;

            Animations = new Animation[] {
                new Animation((int)AnimationID.Idle, IdleTexture, 1, 28, 18, 16), // idle2 -> 1, 48, 10, 48
                new Animation((int)AnimationID.Running, RunningTexture, 8, 28, 18, 16)
            };

            currentAnimation = Animations[(int)AnimationID.Idle];

            paletteParameter = Pj.effect.Parameters["u_palette"];
            effectPass = Pj.effect.Techniques[0].Passes[0];

            Buffs = new List<Buff>();
        }


        internal abstract void Update(float dt, Cell[,] maze);


        protected void SetIdle()
        {
            currentAnimation = Animations[(int)AnimationID.Idle];
            currentAnimation.Reset();
            Animations[(int)AnimationID.Running].Reset();
        }


        protected Cell[] GetSurroundingCells(Cell[,] maze, Vector2 dir)
        {
            List<Cell> cells = new List<Cell>();

            int currentCellX = (int)(x / Tile.Size);
            int currentCellY = (int)(y / Tile.Size);

            int xSign = dir.X >= 0 ? 1 : -1;
            int ySign = dir.Y >= 0 ? 1 : -1;

            int smallX = currentCellX - 1;
            int largeX = currentCellX + 1;

            int smallY = currentCellY - 1;
            int largeY = currentCellY + 1;

            int fromX = xSign == 1 ? smallX : largeX;
            int toX = xSign == 1 ? largeX : smallX;

            int fromY = ySign == 1 ? smallY : largeY;
            int toY = ySign == 1 ? largeY : smallY;

            Func<int, int, int, bool> checkLimit = (i, sign, limit) => {
                if (sign == 1) { return i <= limit; }
                else { return i >= limit; }
            };


            for (int y = fromY; checkLimit(y, ySign, toY); y += ySign)
            {
                for (int x = fromX; checkLimit(x, xSign, toX); x += xSign)
                {
                    Cell cell = maze[y, x];
                    if (cell.Tile.Id != 0)
                    {
                        cells.Add(cell);
                    }
                }
            }

            return cells.ToArray();
        }


        public void Draw(SpriteBatch batch, Matrix cameraMatrix)
        {
            Color color = Color.Wheat;
            switch (palette)
            {
                case 1: color = Color.Red; break;
                case 2: color = Color.Blue; break;
                case 3: color = Color.DarkGreen; break;
                case 4: color = Color.DarkOrange; break;
            }

            batch.Draw(ColliderTexture, GetAABB(), color);
            batch.End();

            batch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.PointClamp, null, null, Pj.effect, cameraMatrix);
            paletteParameter.SetValue(palette);
            currentAnimation.Draw(batch, x, y, rotation, 1, hw, hh);
            batch.End();

            batch.Begin(SpriteSortMode.Deferred, null, SamplerState.PointClamp, null, null, null, cameraMatrix);

            batch.DrawString(Button.Font, this.ID, new Vector2(x - Button.Font.MeasureString(this.ID).X / 2, y - 40), Color.White);


            foreach (Buff buff in Buffs)
            {
                buff.Draw(batch, cameraMatrix);
            }
        }


        public float GetSortY()
        {
            return y;
        }


        internal abstract void ApplyInputOnTheServer(InputPacket input, Cell[,] maze);


        internal void ApplyInput(InputPacket input, Cell[,] maze)
        {
            if (input.Horizontal == 0 && input.Vertical == 0)
            {
                SetIdle();
                return;
            }

            x += v * input.Horizontal;
            y += v * input.Vertical;

            x = MathHelper.Clamp(x, hw + 1, maze.GetLength(1) * Tile.Size - hw - 1);
            y = MathHelper.Clamp(y, hh + 1, maze.GetLength(0) * Tile.Size - hh - 1);

            rotation = ((float)Math.Atan2(input.Vertical, input.Horizontal) + MathHelper.TwoPi) % MathHelper.TwoPi;
            currentAnimation = Animations[(int)AnimationID.Running];

            // Collision
            Vector2 dir = new Vector2(input.Horizontal, input.Vertical);
            dir.Normalize();
            foreach (Cell cell in GetSurroundingCells(maze, dir))
            {
                if (this.AabbAabbIntersectionTest(cell))
                {
                    Vector2 mtv = this.SatIntersectionTestGetMtv(cell) ?? Vector2.Zero;
                    x -= mtv.X;
                    y -= mtv.Y;
                }
            }
        }


        internal abstract void ProcessServerUpdate(StatePacket packet, Cell[,] maze);

        public Vector2[] GetVertices()
        {
            return new Vector2[4]
            {
                new Vector2(x-hw, y-hh),
                new Vector2(x+hw, y-hh),
                new Vector2(x-hw, y+hh),
                new Vector2(x+hw, y+hh)
            };
        }

        public Vector2[] GetSatProjectionAxes()
        {
            return ProjectionAxes;
        }

        public Rectangle GetAABB()
        {
            return new Rectangle((int)(x-hw), (int)(y-hh), (int)(hw*2), (int)(hh*2));
        }
    }
}