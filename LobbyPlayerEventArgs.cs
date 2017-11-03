using System;

namespace MazeGamePillaPilla
{
    class LobbyPlayerEventArgs : EventArgs
    {
        internal string PlayerID;
        internal PlayerControllerIndex ControllerIndex;
        internal Pj.Type Type;
        internal float X;
        internal float Y;
    }

    class LobbyClientsCountEventArgs : EventArgs
    {
        internal int ClientsCount;
    }

    class LobbyOptionsArgs : EventArgs
    {
        internal int PlayersCount;
        internal int Map;
    }

    class GameplayUpdateEventArgs : EventArgs
    {
        internal StatePacket Packet;
    }
}
