using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MazeGamePillaPilla.PowerUps
{
    class BananaDrop : Drop
    {
        public static Texture2D modelTexture;
        private static int radius = 8;

        private AnimationFrame model;
        private float rotation;

        public BananaDrop(int x, int y) : base(x, y, radius, (pj) =>
        {
            pj.Buffs.Add(new BananaStunBuff(pj));
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

        public override void Update(float dt)
        {
            float w = 0.5f;
            rotation = (float)((rotation + dt * w) % (Math.PI * 2));
        }

        public override void Draw(SpriteBatch spritebatch, Matrix cameraMatrix)
        {
            model.Draw(modelTexture, spritebatch, GetAABB().Center.X, GetAABB().Center.Y, rotation, 1, 8, 8);
        }
    }
}
