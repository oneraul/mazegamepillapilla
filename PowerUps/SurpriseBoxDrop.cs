using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MazeGamePillaPilla.PowerUps
{
    class SurpriseBoxDrop : Drop
    {
        private static Random rng = new Random();

        public static Texture2D modelTexture;
        private static readonly int RADIUS = 10;

        private AnimationFrame model;
        private float rotation;

        public SurpriseBoxDrop(int x, int y) : base(x, y, RADIUS, (pj, server) =>
        {
            int randomPowerUpType = rng.Next(Enum.GetNames(typeof(PowerUpTypes)).Length);
            server.AddPowerUp(randomPowerUpType, pj);
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
