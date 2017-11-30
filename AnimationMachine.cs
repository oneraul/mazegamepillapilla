using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MazeGamePillaPilla.PowerUps;

namespace MazeGamePillaPilla
{
    abstract class AnimationMachine
    {
        protected int defaultAnimation;
        protected Animation[] animations;
        private int currentFrame;
        private float timer;

        public int CurrentAnimationId { get; private set; }
        Animation CurrentAnimation { get => animations[CurrentAnimationId]; }
        AnimationFrame CurrentFrame { get => CurrentAnimation.Frames[currentFrame]; }

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
                        CurrentAnimation.Callback?.Invoke();
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

        public void ForceSetAnimation(int animationId)
        {
            timer = 0;
            currentFrame = 0;
            CurrentAnimationId = animationId;
        }

        public void SetAnimation(int animationId)
        {
            if (!CurrentAnimation.LocksUntilCompletion && CurrentAnimationId != animationId)
            {
                ForceSetAnimation(animationId);
            }
        }
    }

    class Animation
    {
        public Texture2D texture;
        public AnimationFrame[] Frames;
        public bool Loop;
        public bool LocksUntilCompletion;
        public Action Callback;

        public Animation(Texture2D texture, int frames, int layersPerFrame, int layerWidth, int layerHeight,
                            float frameDuration = 1, bool loop = true, bool locksUntilCompletion = false, Action callback = null)
        {
            this.texture = texture;
            this.Loop = loop;
            this.LocksUntilCompletion = locksUntilCompletion;
            this.Callback = callback;

            Frames = new AnimationFrame[frames];
            for (int i = 0; i < frames; i++)
            {
                Frames[i] = new AnimationFrame(frameDuration, i, layersPerFrame, layerWidth, layerHeight);
            }
        }
    }

    class AnimationFrame
    {
        public float Duration { get; private set; }
        public Rectangle[] Layers { get; private set; }

        public AnimationFrame(float duration, int frame, int layers, int layerWidth, int layerHeight)
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
            Idle, Running, Stunned, Teleporting, Test
        }

        public PjAnimationMachine()
        {
            defaultAnimation = (int)Animations.Idle;
            animations = new Animation[Enum.GetNames(typeof(Animations)).Length];
            animations[(int)Animations.Idle] =        new Animation(Pj.IdleTexture,        1, 28, 18, 16);
            animations[(int)Animations.Running] =     new Animation(Pj.RunningTexture,     8, 28, 18, 16, 0.08f);
            animations[(int)Animations.Stunned] =     new Animation(Pj.StunnedTexture,     1, 28, 18, 16);
            animations[(int)Animations.Teleporting] = new Animation(Pj.TeleportingTexture, 1, 28, 18, 16, RandomTeleportPowerUp.ANIMATION_DURATION, false);
            animations[(int)Animations.Test] =        new Animation(Pj.TestTexture,        1, 28, 18, 16, 0.3f, false, true);
        }
    }


    class SingleFrameAnimationMachine : AnimationMachine
    {
        public SingleFrameAnimationMachine(Animation animation)
        {
            animations = new Animation[] { animation };
        }
    }
}
