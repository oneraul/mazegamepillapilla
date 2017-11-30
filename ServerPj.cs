using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MazeGamePillaPilla
{
    class ServerPj : Pj
    {
        public long LastProcessedInput;
        public long LastSentSnapshot;
        public int LastBuff;

        public ServerPj(string id) : base(id, 0, 0, 1) {}

        public override void ProcessServerUpdate(StatePacket packet, Cell[,] maze) {}
    }
}
