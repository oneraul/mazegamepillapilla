using LiteNetLib.Utils;

namespace MazeGamePillaPilla
{
    struct InputPacket
    {
        public string CharacterID;
        public long InputSequenceNumber;
        public float Horizontal, Vertical;
        public bool Action;


        internal InputPacket(string CharacterID, long InputSequenceNumber, float Horizontal, float Vertical, bool Action)
        {
            this.CharacterID = CharacterID;
            this.InputSequenceNumber = InputSequenceNumber + 1;
            this.Horizontal = Horizontal;
            this.Vertical = Vertical;
            this.Action = Action;
        }


        internal InputPacket(NetDataReader dataReader)
        {
            CharacterID = dataReader.GetString();
            InputSequenceNumber = dataReader.GetLong();
            Horizontal = dataReader.GetFloat();
            Vertical = dataReader.GetFloat();
            Action = dataReader.GetBool();
        }


        internal NetDataWriter Serialize()
        {
            NetDataWriter writer = new NetDataWriter();

            writer.Put((int)NetMessage.CharacterUpdate);
            writer.Put(CharacterID);
            writer.Put(InputSequenceNumber);
            writer.Put(Horizontal);
            writer.Put(Vertical);
            writer.Put(Action);

            return writer;
        }
    }
}
