using System;
using Microsoft.Xna.Framework;

namespace MazeGamePillaPilla
{
    class VisibilityRay : IIntersectable
    {
        private Rectangle aabb;
        private Vector2[] vertices;
        private Vector2[] projectionAxis;

        public Vector2 From { get; private set; }
        public Vector2 To { get; private set; }
        public float Slope { get; private set; }
        public Vector2 Direction { get; private set; }

        public VisibilityRay(Vector2 from, Vector2 to)
        {
            this.From = from;
            this.To = to;

            float x = Math.Min(from.X, to.X);
            float y = Math.Min(from.Y, to.Y);
            float w = Math.Abs(from.X - to.X);
            float h = Math.Abs(from.Y - to.Y);
            aabb = new Rectangle((int)x, (int)y, (int)w, (int)h);

            vertices = new Vector2[] { from, to };

            Vector2 rayProjectionAxis = to - from;
            rayProjectionAxis.Normalize();
            projectionAxis = new Vector2[] { new Vector2(rayProjectionAxis.Y, -rayProjectionAxis.X) };

            Slope = (to.Y-from.Y) / (to.X-from.X);
            Direction = to - from;
            Direction.Normalize();
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
