using Microsoft.Xna.Framework;

namespace MazeGamePillaPilla
{
    class TestPj : Pj
    {
        private PlayerControllerIndex PlayerControllerIndex;
        private bool oldStateValid;
        private int InputSequenceNumber = 0;

        public TestPj(string ID, PlayerControllerIndex playerControllerIndex, float x, float y, int palette) : base(ID, x, y, palette)
        {
            this.PlayerControllerIndex = playerControllerIndex;
        }

        internal override void ApplyInputOnTheServer(InputPacket input, Cell[,] maze) {}

        internal override void ProcessServerUpdate(StatePacket packet, Cell[,] maze) {}

        internal override void Update(float dt, Cell[,] maze)
        {
            InputPacket? PackageInputData()
            {
                bool action = Input.Controllers[this.PlayerControllerIndex].IsPressed(InputKeys.Action);
                Vector2 movement = Input.Controllers[PlayerControllerIndex].GetDirectionVector();

                if (movement.LengthSquared() == 0)
                {
                    if (action == false && oldStateValid)
                    {
                        oldStateValid = false;
                        return new InputPacket(this.ID, this.InputSequenceNumber, 0, 0, false);
                    }
                }
                else
                {
                    oldStateValid = true;
                    movement *= dt;
                    return new InputPacket(this.ID, this.InputSequenceNumber, movement.X, movement.Y, action);
                }

                return null;
            }


            InputPacket? rawInputData = PackageInputData();
            if (rawInputData == null) return;
            InputPacket inputData = (InputPacket)rawInputData;

            ApplyInput(inputData, maze);

            currentAnimation.Update(dt);
        }
    }
}
