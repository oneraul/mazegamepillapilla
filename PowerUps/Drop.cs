using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MazeGamePillaPilla.PowerUps
{
    public enum DropTypes
    {
        SurpriseBoxDrop,
        BananaDrop,
    }

    abstract class Drop : IDrawable, IIntersectable, ISpawnable
    {
        private static Vector2[] ProjectionAxes = new Vector2[] { Vector2.UnitX, Vector2.UnitY };

        public Action<Pj, Server> Callback { get; private set; }
        private int x;
        private int y;
        private int radius;

        protected Drop(int x, int y, int radius, Action<Pj, Server> callback)
        {
            this.x = x;
            this.y = y;
            this.radius = radius;
            this.Callback = callback;
        }

        public void SetPosition(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public Rectangle GetAABB()
        {
            return new Rectangle(x - radius, y - radius, radius * 2, radius * 2);
        }

        public Vector2[] GetSatProjectionAxes()
        {
            return ProjectionAxes;
        }

        public Vector2[] GetVertices()
        {
            return new Vector2[]
            {
                new Vector2(x+radius, y-radius),
                new Vector2(x+radius, y+radius),
                new Vector2(x-radius, y+radius),
                new Vector2(x-radius, y-radius)
            };
        }

        public abstract void Draw(SpriteBatch spritebatch, Matrix cameraMatrix);

        public float GetSortY()
        {
            return y;
        }

        public virtual void Update(float dt) { }
    }
}
