using Microsoft.Xna.Framework;

namespace MazeGamePillaPilla
{
    class RemotePj : Pj
    {
        public float oldX;
        public float oldY;
        public float newX;
        public float newY;
        public float oldRotation;
        public float newRotation;
        public float interpolationTimer;

        public RemotePj(string ID, float x, float y, int palette) : base(ID, x, y, palette)
        {
            oldX = newX = x;
            oldY = newY = y;
            oldRotation = newRotation = rotation;
        }


        public override void Update(float dt, Cell[,] maze)
        {
            AnimationMachine.Update(dt);

            interpolationTimer += dt;
            float a = MathHelper.Clamp(interpolationTimer / Server.TickRate, 0, 1);
            this.x = oldX * (1 - a) + newX * a;
            this.y = oldY * (1 - a) + newY * a;
            this.rotation = oldRotation * (1 - a) + newRotation * a;
        }


        public override void ProcessServerUpdate(StatePacket packet, Cell[,] maze)
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

            if(AnimationMachine.CurrentAnimationId != packet.Animation)
            {
                AnimationMachine.SetAnimation(packet.Animation);
            }
        }


        public override void ApplyInputOnTheServer(InputPacket input, Cell[,] maze)
        {
            ApplyInput(input, maze);
        }


        public new void SetPosition(int x, int y)
        {
            interpolationTimer = 0;
            this.x = oldX = newX = x;
            this.y = oldY = newY = y;
        }
    }
}
