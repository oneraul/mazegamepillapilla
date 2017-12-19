using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MazeGamePillaPilla
{

    public class Game1 : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spritebatch;
        

        public Game1()
        {
            Content.RootDirectory = "Content";
            graphics = new GraphicsDeviceManager(this)
            {
                IsFullScreen = false,
                HardwareModeSwitch = false,  // false => borderless windowed, true => fullscreen
                PreferredBackBufferWidth = 800,
                PreferredBackBufferHeight = 600,
                PreferMultiSampling = false,
            };

            this.IsFixedTimeStep = true;
        }


        protected override void Initialize()
        {
            Input.Initialize();
            spritebatch = new SpriteBatch(GraphicsDevice);
            Button.Initialize(Content);
            ScreenManager.Initialize(GraphicsDevice, Content);
            ScreenManager.PushScreen(new MainMenuScreen());
            base.Initialize();
        }


        protected override void LoadContent()
        {
            base.LoadContent();
        }


        protected override void UnloadContent()
        {
            Content.Unload();
            base.UnloadContent();
        }


        protected override void Update(GameTime gameTime)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            Input.Update(dt);
            ScheduleManager.Update(dt);
            ScreenManager.CurrentScreen.Update(dt);
            base.Update(gameTime);
        }


        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);
            ScreenManager.CurrentScreen.Draw(spritebatch);
            base.Draw(gameTime);
        }
    }
}
