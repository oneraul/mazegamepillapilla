using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MazeGamePillaPilla
{
    class Animation
    {
        private Texture2D Texture;

        private int Layers;
        private int LayerWidth;
        private int LayerHeight;
        private AnimationFrame[] Frames;

        private float FrameDuration;
        private float Timer;
        private int CurrentFrame;

        public int ID { get; }

        public Animation(int id, Texture2D texture, int frames, int layers, int width, int height)
        {
            ID = id;
            Texture = texture;
            Layers = layers;
            LayerWidth = width;
            LayerHeight = height;

            List<AnimationFrame> animationFrames = new List<AnimationFrame>();
            for (int frame = 0; frame < frames; frame++)
            {
                AnimationFrame animationFrame = new AnimationFrame(layers);
                animationFrames.Add(animationFrame);
                for (int i = 0; i < layers; i++)
                {
                    animationFrame.Rectangles[i] = new Rectangle(i*LayerWidth, frame*LayerHeight, LayerWidth, LayerHeight);
                }
            }
            Frames = animationFrames.ToArray();

            FrameDuration = 0.08f;
            CurrentFrame = 0;
            Timer = 0;
        }


        public void Reset()
        {
            Timer = 0;
            CurrentFrame = 0;
        }


        public void Draw(SpriteBatch batch, float x, float y, float rotation, float scale, float originX, float originY)
        {
            Frames[CurrentFrame].Draw(Texture, batch, x, y, rotation, scale, originX, originY);
        }


        public void Update(float dt)
        {
            Timer += dt;
            if (Timer >= FrameDuration)
            {
                Timer -= FrameDuration;
                CurrentFrame++;
                if (CurrentFrame >= Frames.Length)
                {
                    CurrentFrame = 0;
                }
            }
        }
    }


    class AnimationFrame
    {
        public Rectangle[] Rectangles;

        public AnimationFrame(int layers)
        {
            Rectangles = new Rectangle[layers];
        }

        public void Draw(Texture2D texture, SpriteBatch batch, float x, float y, float rotation, float scale, float originX, float originY)
        {
            for(int i = 0; i < Rectangles.Length; i++)
            {
                batch.Draw(texture, new Vector2(x, y - i), Rectangles[i], Color.White,
                    rotation, new Vector2(originX, originY), scale, SpriteEffects.None, 0);
            }
        }
    }
}
