using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MazeGamePillaPilla
{
    abstract class AnimationMachine
    {
        protected int defaultAnimation;
        protected NewAnimation[] animations;
        private int currentFrame;
        private float timer;

        public int CurrentAnimationId { get; private set; }
        NewAnimation CurrentAnimation { get => animations[CurrentAnimationId]; }
        NewAnimationFrame CurrentFrame { get => CurrentAnimation.Frames[currentFrame]; }

        public void Update(float dt)
        {
            timer += dt;
            if (timer >= CurrentFrame.Duration)
            {
                timer -= CurrentFrame.Duration;
                currentFrame++;

                if (currentFrame >= CurrentAnimation.Frames.Length)
                {
                    currentFrame = 0;
                    if (!CurrentAnimation.Loop)
                    {
                        CurrentAnimation?.Callback();
                        ForceSetAnimation(defaultAnimation);
                    }
                }
            }
        }

        public void Draw(SpriteBatch spritebatch, float x, float y, float rotation)
        {
            float scale = 1;
            Vector2 origin = new Vector2(
                (CurrentAnimation.texture.Width/CurrentFrame.Layers.Length)/2, 
                (CurrentAnimation.texture.Height)/CurrentAnimation.Frames.Length/2);

            for (int i = 0; i < CurrentFrame.Layers.Length; i++)
            {
                spritebatch.Draw(CurrentAnimation.texture, new Vector2(x, y - i), CurrentFrame.Layers[i],
                    Color.White, rotation, origin, scale, SpriteEffects.None, 0);
            }
        }

        private void ForceSetAnimation(int animationId)
        {
            timer = 0;
            currentFrame = 0;
            CurrentAnimationId = animationId;
        }

        public void SetAnimation(int animationId)
        {
            if (!CurrentAnimation.LocksUntilCompletion)
            {
                ForceSetAnimation(animationId);
            }
        }
    }

    class NewAnimation
    {
        public Texture2D texture;
        public NewAnimationFrame[] Frames;
        public bool Loop;
        public bool LocksUntilCompletion;
        public Action Callback;

        public NewAnimation(Texture2D texture, float frameDuration, int frames, int layersPerFrame, int layerWidth, int layerHeight, 
                            bool loop = true, bool locksUntilCompletion = false, Action callback = null)
        {
            this.texture = texture;
            this.Loop = loop;
            this.LocksUntilCompletion = locksUntilCompletion;
            this.Callback = callback;

            Frames = new NewAnimationFrame[frames];
            for (int i = 0; i < frames; i++)
            {
                Frames[i] = new NewAnimationFrame(frameDuration, i, layersPerFrame, layerWidth, layerHeight);
            }
        }
    }

    class NewAnimationFrame
    {
        public float Duration { get; private set; }
        public Rectangle[] Layers { get; private set; }

        public NewAnimationFrame(float duration, int frame, int layers, int layerWidth, int layerHeight)
        {
            this.Duration = duration;

            Layers = new Rectangle[layers];
            for (int i = 0; i < layers; i++)
            {
                Layers[i] = new Rectangle(i * layerWidth, frame * layerHeight, layerWidth, layerHeight);
            }
        }
    }


    class PjAnimationMachine : AnimationMachine
    {
        public enum Animations
        {
            Idle, Running, Test
        }

        public PjAnimationMachine()
        {
            defaultAnimation = (int)Animations.Idle;
            animations = new NewAnimation[]
            {
                new NewAnimation(Pj.IdleTexture, 10, 1, 28, 18, 16),
                new NewAnimation(Pj.RunningTexture, 0.08f, 8, 28, 18, 16),
                new NewAnimation(Pj.TestTexture, 1, 1, 28, 18, 16, false, true)
            };
        }
    }
}
