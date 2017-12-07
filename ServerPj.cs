namespace MazeGamePillaPilla
{
    class ServerPj : Pj
    {
        public long LastProcessedInput;
        public long LastSentSnapshot;
        public int LastBuff;
        public int LastProccessedInputTimestamp;

        public ServerPj(string id) : base(id, 0, 0, 1) {}

        public override void ProcessServerUpdate(StatePacket packet, Cell[,] maze) {}
    }
}
