using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace MazeGamePillaPilla
{
    class AiPj : Pj
    {
        public List<Vector2> path;
        public bool NeedsToRecalculatePath;
        private float recalculatePathAccumulator;
        private static readonly float recalculatePathTime = 1;

        public AiPj(string ID, float x, float y, int palette) : base(ID, x, y, palette)
        {
            path = new List<Vector2>();
        }

        public override void ApplyInputOnTheServer(InputPacket input, Cell[,] maze) {}

        public override void ProcessServerUpdate(StatePacket packet, Cell[,] maze) {}

        public override void Update(float dt, Cell[,] maze)
        {
            currentAnimation.Update(dt);

            if (path.Count == 0)
            {
                SetIdle();
            }
            else
            {
                float dstThresholdSquare = 5 * 5;

                Vector2 pos = new Vector2(this.x, this.y);
                Vector2 nextNode = path[path.Count - 1];
                Vector2 dir = nextNode - pos;
                if (dir.LengthSquared() > dstThresholdSquare)
                {
                    dir.Normalize();
                    this.x += dir.X * this.v * dt;
                    this.y += dir.Y * this.v * dt;
                    this.rotation = ((float)Math.Atan2(dir.Y, dir.X) + MathHelper.TwoPi) % MathHelper.TwoPi;
                }
                else
                {
                    path.RemoveAt(path.Count - 1);
                }

                currentAnimation = Animations[(int)AnimationID.Running];
            }


            recalculatePathAccumulator += dt;
            if (recalculatePathAccumulator >= recalculatePathTime)
            {
                recalculatePathAccumulator -= recalculatePathTime;
                NeedsToRecalculatePath = true;
            }
        }
    }
}
