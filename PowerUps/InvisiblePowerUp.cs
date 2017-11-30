using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MazeGamePillaPilla.PowerUps
{
    class InvisiblePowerUp : IPowerUp
    {
        public static Texture2D Icon;

        public void Action(Pj pj, Server server)
        {
            server.AddBuff((int)BuffTypes.InvisibleBuff, pj);
        }

        public Texture2D GetIcon() => Icon;
        public Color GetColor() => Color.Cyan;
        public PjAnimationMachine.Animations GetAnimation() => PjAnimationMachine.Animations.Test;
    }
}
