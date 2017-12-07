using LiteNetLib.Utils;

namespace MazeGamePillaPilla
{
    struct StatePacket
    {
        public string CharacterID;
        public long InputSequenceNumber;
        public float X, Y;
        public float Rotation;
        public int Palette;
        public int Animation;
        public int Timestamp;


        public StatePacket(string CharacterID, long InputSequenceNumber, float X, float Y, float Rotation, int Palette, int Animation, int Timestamp)
        {
            this.CharacterID = CharacterID;
            this.InputSequenceNumber = InputSequenceNumber;
            this.X = X;
            this.Y = Y;
            this.Rotation = Rotation;
            this.Palette = Palette;
            this.Animation = Animation;
            this.Timestamp = Timestamp;
        }


        public StatePacket(NetDataReader dataReader)
        {
            this.CharacterID = dataReader.GetString();
            this.InputSequenceNumber = dataReader.GetLong();
            this.X = dataReader.GetFloat();
            this.Y = dataReader.GetFloat();
            this.Rotation = dataReader.GetFloat();
            this.Palette = dataReader.GetInt();
            this.Animation = dataReader.GetInt();
            this.Timestamp = dataReader.GetInt();
        }


        public StatePacket(long InputSequenceNumber, ServerPj pj)
        {
            this.CharacterID = pj.ID;
            this.InputSequenceNumber = InputSequenceNumber;
            this.X = pj.x;
            this.Y = pj.y;
            this.Rotation = pj.rotation;
            this.Palette = pj.palette;
            this.Animation = pj.AnimationMachine.CurrentAnimationId;
            this.Timestamp = pj.LastProccessedInputTimestamp;
        }


        public NetDataWriter Serialize()
        {
            NetDataWriter writer = new NetDataWriter();

            writer.Put((int)NetMessage.CharacterUpdate);
            writer.Put(CharacterID);
            writer.Put(InputSequenceNumber);
            writer.Put(X);
            writer.Put(Y);
            writer.Put(Rotation);
            writer.Put(Palette);
            writer.Put(Animation);
            writer.Put(Timestamp);

            return writer;
        }
    }
}
