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

    class GameplayDropEventArgs : EventArgs
    {
        public int Id;
        public int Type;
        public int X;
        public int Y;
    }

    class GameplayBuffEventArgs : EventArgs
    {
        public string PlayerId;
        public int BuffType;
        public int BuffId;
    }

    class GameplayPowerUpEventArgs : EventArgs
    {
        public string PlayerId;
        public int Type;
    }

    class GameplayCharacterTeleportedEventArgs
    {
        public string PlayerId;
        public int X;
        public int Y;
    }

    class GameplayTintaSplashEventArgs
    {
        public int Id;
        public int X;
        public int Y;
        public float Rotation;
        public float Duration;
    }
}
