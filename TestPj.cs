using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

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
            this.PowerUp = new SprintPowerUp();
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
                    if (action)
                    {
                        return new InputPacket(this.ID, this.InputSequenceNumber, 0, 0, true);
                    }
                    else if (oldStateValid)
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

            // this is not implemented yet in the Pj class
            if (inputData.Action)
            {
                if (this.PowerUp != null)
                {
                    this.PowerUp.Action(this);
                    this.PowerUp = null;
                }
            }

            for (int i = Buffs.Count-1; i >= 0; i--)
            {
                Buff buff = Buffs[i];
                buff.Update(dt);
                if (buff.ToBeRemoved)
                {
                    buff.End();
                    Buffs.RemoveAt(i);
                }
            }

            //--------------------------------------

            currentAnimation.Update(dt);
        }
    }
}
