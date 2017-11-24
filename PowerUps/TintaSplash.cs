using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MazeGamePillaPilla.PowerUps
{
    class TintaSplash
    {
        public static Texture2D Texture;

        private int x;
        private int y;
        private float rotation;
        private float timer;
        private float duration;
        public bool ToBeRemoved { get; protected set; }

        public TintaSplash(int x, int y, float rotation, float duration)
        {
            this.x = x;
            this.y = y;
            this.rotation = rotation;
            this.duration = duration;
        }

        public void Draw(SpriteBatch batch, Matrix cameraMatrix)
        {
            Vector2 position = new Vector2(x, y);
            Vector2 origin = new Vector2(Texture.Width / 2, Texture.Height / 2);
            float scale = 1;
            batch.Draw(Texture, position, null, Color.LawnGreen, rotation, origin, scale, SpriteEffects.None, 0);
        }

        public void Update(float dt)
        {
            timer += dt;
            if (timer >= duration)
            {
                ToBeRemoved = true;
            }
        }
    }
}
