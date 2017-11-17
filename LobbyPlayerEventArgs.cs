using System;

namespace MazeGamePillaPilla
{
    class LobbyPlayerEventArgs : EventArgs
    {
        public string PlayerID;
        public PlayerControllerIndex ControllerIndex;
        public Pj.Type Type;
        public float X;
        public float Y;
    }

    class LobbyClientsCountEventArgs : EventArgs
    {
        public int ClientsCount;
    }

    class LobbyOptionsArgs : EventArgs
    {
        public int PlayersCount;
        public int Map;
    }

    class GameplayUpdateEventArgs : EventArgs
    {
        public StatePacket Packet;
    }
}
