using System.Collections.Generic;
using Microsoft.Xna.Framework;
using LiteNetLib;
using LiteNetLib.Utils;

namespace MazeGamePillaPilla
{
    class LocalPj : Pj
    {
        public PlayerControllerIndex PlayerControllerIndex { get; private set; }
        public List<InputPacket> PendingInputs;
        public long InputSequenceNumber;
        private bool oldStateValid;


        public LocalPj(string ID, PlayerControllerIndex playerControllerIndex, float x, float y, int palette) : base(ID, x, y, palette)
        {
            PlayerControllerIndex = playerControllerIndex;
            PendingInputs = new List<InputPacket>();
            InputSequenceNumber = 0;
        }


        public override void Update(float dt, Cell[,] maze)
        {
            currentAnimation.Update(dt);
        }


        public override void ProcessServerUpdate(StatePacket packet, Cell[,] maze)
        {
            x = packet.X;
            y = packet.Y;
            rotation = packet.Rotation;

            // reconciliation
            for (int i = PendingInputs.Count-1; i >= 0; i--)
            {
                InputPacket pending = PendingInputs[i];
                if (pending.InputSequenceNumber <= packet.InputSequenceNumber)
                    PendingInputs.RemoveAt(i);
                else
                    ApplyInput(pending, maze);
            }
        }


        public override void ApplyInputOnTheServer(InputPacket input, Cell[,] maze)
        {
            // already applied locally as client-side prediction
        }


        public void ProcessInput(float dt, Client client, Cell[,] maze)
        {
            InputPacket? PackageInputData()
            {
                bool action = Input.Controllers[PlayerControllerIndex].IsPressed(InputKeys.Action);
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

            InputSequenceNumber++;
            NetDataWriter writer = inputData.Serialize();

            // Send the packet
            client.Send(writer, SendOptions.ReliableOrdered);

            // Client-side prediction
            ApplyInput(inputData, maze);

            // Save the input packet for reconciliation
            PendingInputs.Add(inputData);
        }
    }
}
