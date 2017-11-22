using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MazeGamePillaPilla.PowerUps
{
    class TintaPowerUp : IPowerUp
    {
        public static Texture2D Icon;

        public void Action(Pj pj, Server server)
        {
            // TODO
        }

        public Texture2D GetIcon() => Icon;
        public Color GetColor() => Color.LawnGreen;
    }
}
