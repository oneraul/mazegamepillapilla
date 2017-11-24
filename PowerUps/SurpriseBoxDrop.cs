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

        private AnimationMachine animationMachine;
        private float rotation;

        public SurpriseBoxDrop(int x, int y) : base(x, y, RADIUS, (pj, server) =>
        {
            int randomPowerUpType = rng.Next(Enum.GetNames(typeof(PowerUpTypes)).Length);
            server.AddPowerUp(randomPowerUpType, pj);
        })
        {
            animationMachine = new SingleFrameAnimationMachine(new Animation(modelTexture, 1, 16, 16, 16));
            rotation = (float)(rng.NextDouble() * Math.PI*2);
        }

        public override void Draw(SpriteBatch spritebatch, Matrix cameraMatrix)
        {
            spritebatch.Draw(SprintPowerUp.pixel, GetAABB(), Color.GreenYellow);
            animationMachine.Draw(spritebatch, GetAABB().Center.X, GetAABB().Center.Y, rotation);
        }

        public override void Update(float dt)
        {
            rotation = (float)((rotation + dt) % (Math.PI * 2));
        }
    }
}
