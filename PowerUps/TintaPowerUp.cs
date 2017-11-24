using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MazeGamePillaPilla.PowerUps
{
    class TintaPowerUp : IPowerUp
    {
        private static readonly int SPLASHES = 5;
        private static readonly int DISPLACEMENT_RADIUS = 50;
        private static readonly float SPLASH_BASE_DURATION = 4;
        private static readonly float SPLASH_EXTRA_MAX_DURATION = 1;
        private static Random rng = new Random();
        public static Texture2D Icon;

        public void Action(Pj pj, Server server)
        {
            for (int i = 0; i < SPLASHES; i++)
            {
                float displacementAngle = rng.Next(360);
                int x = (int)(pj.x + DISPLACEMENT_RADIUS * Math.Cos(displacementAngle));
                int y = (int)(pj.y + DISPLACEMENT_RADIUS * Math.Sin(displacementAngle));
                float rotation = (float)(rng.NextDouble() * Math.PI * 2);
                float duration = SPLASH_BASE_DURATION + (float)rng.NextDouble() * SPLASH_EXTRA_MAX_DURATION;
                server.AddTintaSplash(x, y, rotation, duration);
            }
        }

        public Texture2D GetIcon() => Icon;
        public Color GetColor() => Color.LawnGreen;
    }
}
