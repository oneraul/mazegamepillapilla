using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace MazeGamePillaPilla
{
    class RemotePj : Pj
    {
        private const bool INTERPOLATION_ENABLED = true;

        struct Snapshot
        {
            public float X;
            public float Y;
            public float Rotation;
            public float Timestamp;
        }

        public DateTime GameStartedTime;
        private List<Snapshot> snapshots;

        public RemotePj(string ID, float x, float y, int palette) : base(ID, x, y, palette)
        {
            snapshots = new List<Snapshot>() { new Snapshot() { X = x, Y = y, Rotation = this.rotation, Timestamp = 0 } };
        }


        public override void Update(float dt)
        {
            base.Update(dt);

            if (INTERPOLATION_ENABLED)
            {
                if (snapshots.Count >= 2)
                {
                    TimeSpan gameElapsedTime = DateTime.UtcNow.Subtract(GameStartedTime);
                    int now = (int)gameElapsedTime.TotalMilliseconds;
                    int interpolationDelayMilliseconds = 80;
                    int renderTime = now - interpolationDelayMilliseconds;

                    while (snapshots.Count >= 2 && snapshots[1].Timestamp <= renderTime)
                    {
                        snapshots.RemoveAt(0);
                    }

                    if (snapshots.Count >= 2)
                    {
                        Snapshot Old = snapshots[0];
                        Snapshot New = snapshots[1];

                        if (Old.Timestamp <= renderTime && renderTime <= New.Timestamp)
                        {
                            float alpha = (renderTime - Old.Timestamp) / (New.Timestamp - Old.Timestamp);
                            this.x = MathHelper.Lerp(Old.X, New.X, alpha);
                            this.y = MathHelper.Lerp(Old.Y, New.Y, alpha);
                            this.rotation = MathHelper.Lerp(Old.Rotation, New.Rotation, alpha);
                        }
                    }
                }
            }
        }

        public override void ProcessServerUpdate(StatePacket packet, Cell[,] maze)
        {
            if (INTERPOLATION_ENABLED)
            {
                snapshots.Add(new Snapshot()
                {
                    X = packet.X, Y = packet.Y, Rotation = packet.Rotation, Timestamp = packet.Timestamp
                });
            }
            else
            {
                #pragma warning disable CS0162 // Unreachable code detected
                this.x = packet.X;
                this.y = packet.Y;
                this.rotation = packet.Rotation;
                #pragma warning restore CS0162 // Unreachable code detected
            }
        }

        public new void SetPosition(int x, int y)
        {
            this.x = x;
            this.y = y;

            if (INTERPOLATION_ENABLED)
            {
                float lastTimestamp = snapshots[0].Timestamp;
                snapshots.Clear();
                snapshots.Add(new Snapshot() { X = x, Y = y, Rotation = this.rotation, Timestamp =  lastTimestamp });
            }
        }
    }
}
