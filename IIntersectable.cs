using Microsoft.Xna.Framework;

namespace MazeGamePillaPilla
{
    interface IIntersectable
    {
        Vector2[] GetVertices();
        Vector2[] GetSatProjectionAxes();
        Rectangle GetAABB();
    }
}
