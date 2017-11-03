using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace MazeGamePillaPilla
{
    interface IScreen
    {
        void Initialize(GraphicsDevice GraphicsDevice, ContentManager Content);
        void Enter();
        void Update(float dt);
        void Draw(SpriteBatch spritebatch);
        void Exit();
    }
}
