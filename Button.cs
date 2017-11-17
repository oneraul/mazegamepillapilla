using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using System;

namespace MazeGamePillaPilla
{
    class Button
    {
        public static SpriteFont Font { get; private set; }
        private static Texture2D NormalTexture;
        private static Texture2D HoverTexture;
        private static Texture2D ActiveTexture;

        public static void Initialize(ContentManager Content)
        {
            Button.Font          = Content.Load<SpriteFont>("font");
            Button.NormalTexture = Content.Load<Texture2D>("button_normal");
            Button.HoverTexture  = Content.Load<Texture2D>("button_hover");
            Button.ActiveTexture = Content.Load<Texture2D>("button_active");
        }


        public enum CurrentState
        {
            Normal, Hover, Active
        }


        public int X { get; private set; }
        public int Y { get; private set; }
        public int Width { get; private set; }
        public int Height { get; private set; }
        private Texture2D normalTexture;
        private Texture2D hoverTexture;
        private Texture2D activeTexture;
        private string text;

        public Button NextButtonUp { get; set; }
        public Button NextButtonLeft { get; set; }
        public Button NextButtonDown { get; set; }
        public Button NextButtonRight { get; set; }

        public EventHandler Click;

        public Button(int x, int y, int width, int height, string text = "", Texture2D normalTexture = null, Texture2D hoverTexture = null, Texture2D activeTexture = null)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
            this.normalTexture = normalTexture ?? Button.NormalTexture;
            this.hoverTexture = hoverTexture ?? Button.HoverTexture;
            this.activeTexture = activeTexture ?? Button.ActiveTexture;
            this.text = text;

            NextButtonUp = NextButtonLeft = NextButtonDown = NextButtonRight = this;
        }
        
        public void DrawNormal(SpriteBatch spritebatch)
        {
            Rectangle rectangle = new Rectangle(X, Y, Width, Height);
            spritebatch.Draw(normalTexture, rectangle, Color.White);

            Vector2 textLength = Button.Font.MeasureString(text);
            Vector2 position = new Vector2(X + Width/2 - textLength.X/2, Y + Height/2 - textLength.Y/2);
            spritebatch.DrawString(Button.Font, text, position, Color.White);
        }

        public void DrawHover(SpriteBatch spritebatch)
        {
            Rectangle rectangle = new Rectangle(X, Y, Width, Height);
            spritebatch.Draw(hoverTexture, rectangle, Color.White);

            Vector2 textLength = Button.Font.MeasureString(text);
            Vector2 position = new Vector2(X + Width / 2 - textLength.X / 2, Y + Height / 2 - textLength.Y / 2);
            spritebatch.DrawString(Button.Font, text, position, Color.White);
        }

        public void DrawActive(SpriteBatch spritebatch)
        {
            Rectangle rectangle = new Rectangle(X, Y, Width, Height);
            spritebatch.Draw(activeTexture, rectangle, Color.White);

            Vector2 textLength = Button.Font.MeasureString(text);
            Vector2 position = new Vector2(X + Width / 2 - textLength.X / 2, Y + Height / 2 - textLength.Y / 2);
            spritebatch.DrawString(Button.Font, text, position, Color.White);
        }
    }
}
