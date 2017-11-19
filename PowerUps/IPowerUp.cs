using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MazeGamePillaPilla.PowerUps
{
    interface IPowerUp
    {
        void Action(Pj pj);
        Texture2D GetIcon();
        Color GetColor();
    }
}
