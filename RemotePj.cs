﻿using Microsoft.Xna.Framework;

namespace MazeGamePillaPilla
{
    class RemotePj : Pj
    {
        internal float oldX;
        internal float oldY;
        internal float newX;
        internal float newY;
        internal float oldRotation;
        internal float newRotation;
        internal float interpolationTimer;

        internal RemotePj(string ID, float x, float y, int palette) : base(ID, x, y, palette)
        {
            oldX = newX = x;
            oldY = newY = y;
            oldRotation = newRotation = rotation;
        }


        internal override void Update(float dt, Cell[,] maze)
        {
            currentAnimation.Update(dt);

            interpolationTimer += dt;
            float serverTickrate = 1f/5;
            float a = MathHelper.Clamp(interpolationTimer / serverTickrate, 0, 1);
            this.x = oldX * (1 - a) + newX * a;
            this.y = oldY * (1 - a) + newY * a;
            this.rotation = oldRotation * (1 - a) + newRotation * a;
        }


        internal override void ProcessServerUpdate(StatePacket packet, Cell[,] maze)
        {
            // no interpolation
            /*
            oldX = newX = packet.x;
            oldY = newY = packet.y;
            oldRotation = newRotation = packet.Dir;
            currentAnimation = Animations[packet.Animation];
            */

            interpolationTimer = 0;
            oldX = newX;
            oldY = newY;
            oldRotation = newRotation;
            
            newX = packet.X;
            newY = packet.Y;
            newRotation = packet.Rotation;

            currentAnimation = Animations[packet.Animation];
        }


        internal override void ApplyInputOnTheServer(InputPacket input, Cell[,] maze)
        {
            ApplyInput(input, maze);
        }
    }
}