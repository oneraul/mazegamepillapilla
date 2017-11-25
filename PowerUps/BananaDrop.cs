using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MazeGamePillaPilla.PowerUps
{
    class BananaDrop : Drop
    {
        public static Texture2D modelTexture;
        private static readonly int RADIUS = 8; // from the center to the vertices of the aabb

        private AnimationMachine animationMachine;
        private float rotation;

        public BananaDrop(int x, int y) : base(x, y, RADIUS, (pj, server) =>
        {
            if (!pj.Immune)
            {
                server.AddBuff((int)BuffTypes.BananaStunBuff, pj);
            }
        })
        {
            animationMachine = new SingleFrameAnimationMachine(new Animation(modelTexture, 1, 16, 16, 16));
        }

        public override void Update(float dt)
        {
            float w = 0.5f;
            rotation = (float)((rotation + dt * w) % (Math.PI * 2));
        }

        public override void Draw(SpriteBatch spritebatch, Matrix cameraMatrix)
        {
            animationMachine.Draw(spritebatch, GetAABB().Center.X, GetAABB().Center.Y, rotation);
        }
    }
}
