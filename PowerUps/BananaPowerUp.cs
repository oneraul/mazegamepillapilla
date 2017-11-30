using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MazeGamePillaPilla.PowerUps
{
    class BananaPowerUp : IPowerUp
    {
        public static Texture2D Icon;

        public void Action(Pj pj, Server server)
        {
            float angle = pj.rotation - MathHelper.PiOver2;
            Vector2 opositeDirectionOfThePlayer = new Vector2((float)Math.Sin(angle), -(float)Math.Cos(angle));
            int dropDistance = 30;
            int x = (int)(pj.x + dropDistance * opositeDirectionOfThePlayer.X);
            int y = (int)(pj.y + dropDistance * opositeDirectionOfThePlayer.Y);

            server.AddDrop((int)DropTypes.BananaDrop, x, y);
        }

        public Texture2D GetIcon() => Icon;
        public Color GetColor() => Color.Gold;
        public PjAnimationMachine.Animations GetAnimation() => PjAnimationMachine.Animations.Test;
    }
}
