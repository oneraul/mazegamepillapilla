using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MazeGamePillaPilla
{
    interface IDrawable
    {
        void Draw(SpriteBatch batch, Matrix cameraMatrix);
        float GetSortY();
    }
}
