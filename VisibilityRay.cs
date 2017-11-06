using System;
using Microsoft.Xna.Framework;

namespace MazeGamePillaPilla
{
    class VisibilityRay : IIntersectable
    {
        private Vector2 from;
        private Vector2 to;
        private Rectangle aabb;
        private Vector2[] vertices;
        private Vector2[] projectionAxis;

        public VisibilityRay(Vector2 from, Vector2 to)
        {
            this.from = from;
            this.to = to;

            float x = Math.Min(from.X, to.X);
            float y = Math.Min(from.Y, to.Y);
            float w = Math.Abs(from.X - to.X);
            float h = Math.Abs(from.Y - to.Y);
            aabb = new Rectangle((int)x, (int)y, (int)w, (int)h);

            vertices = new Vector2[] { from, to };

            Vector2 rayProjectionAxis = to - from;
            rayProjectionAxis.Normalize();
            projectionAxis = new Vector2[] { new Vector2(rayProjectionAxis.Y, -rayProjectionAxis.X) };
        }

        public Vector2[] GetVertices()
        {
            return vertices;
        }

        public Vector2[] GetSatProjectionAxes()
        {
            return projectionAxis;
        }

        public Rectangle GetAABB()
        {
            return aabb;
        }
    }
}
