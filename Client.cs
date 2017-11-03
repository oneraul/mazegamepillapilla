using System;
using LiteNetLib;
using LiteNetLib.Utils;

namespace MazeGamePillaPilla
{
    class Client
    {
        private EventBasedNetListener listener;
        private NetManager client;

        internal Client()
        {
            string Ip = "localhost";
            int Port = 9050;

            listener = new EventBasedNetListener();
            client = new NetManager(listener, "SomeConnectionKey");

            if (!client.Start()) System.Diagnostics.Debug.WriteLine("Error with Client.Start()");
            client.Connect(Ip, Port);

            SetLobby();
        }


        internal void Close()
        {
            NetDataWriter writer = new NetDataWriter();
            writer.Put((int)NetMessage.ClientClosed);
            client.SendToAll(writer, SendOptions.ReliableOrdered);

            client.Stop();
        }


        internal void Send(NetDataWriter writer, SendOptions sendOptions)
        {
            client.SendToAll(writer, sendOptions);
        }


        internal void LobbyUpdate(float dt)
        {
            client.PollEvents();
        }


        private void SetLobby()
        {
            System.Diagnostics.Debug.WriteLine("[CLIENT] SetLobby");
            listener.NetworkReceiveEvent -= OnGameplayNetworkReceived;

            listener.PeerConnectedEvent += OnLobbyPeerConnected;
            listener.NetworkReceiveEvent += OnLobbyNetworkReceived;
            listener.PeerDisconnectedEvent += OnLobbyPeerDisconnected;
        }


        private void OnLobbyPeerConnected(NetPeer peer)
        {
            System.Diagnostics.Debug.WriteLine("[CLIENT] client connected");
        }


        private void OnLobbyPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
        {
            System.Diagnostics.Debug.WriteLine("[CLIENT] disconnected {0}", disconnectInfo.Reason);
            OnServerClosed();
        }


        private void OnLobbyNetworkReceived(NetPeer peer, NetDataReader dataReader)
        {
            NetMessage instruction = (NetMessage)dataReader.GetInt();
            switch (instruction)
            {
                case NetMessage.ServerClosed:
                    OnServerClosed();
                    break;

                case NetMessage.ClientsCount:
                    int count = dataReader.GetInt();
                    OnClientsCount(count);
                    break;

                case NetMessage.AddPlayer:
                    PlayerAdded?.Invoke(this, new LobbyPlayerEventArgs()
                    {
                        PlayerID = dataReader.GetString(),
                        ControllerIndex = (PlayerControllerIndex)dataReader.GetInt()
                    });
                    break;

                case NetMessage.RemovePlayer:
                    PlayerRemoved?.Invoke(this, new LobbyPlayerEventArgs() { PlayerID = dataReader.GetString() });
                    break;

                case NetMessage.PrepareToStartGame:
                    PreparingGameToStart?.Invoke(this, new LobbyOptionsArgs {
                        PlayersCount = dataReader.GetInt(),
                        Map = dataReader.GetInt()
                    });
                    break;

                case NetMessage.InstantiateCharacter:
                    InstantiateCharacter?.Invoke(this, new LobbyPlayerEventArgs {
                        PlayerID = dataReader.GetString(),
                        ControllerIndex = (PlayerControllerIndex)dataReader.GetInt(),
                        Type = (Pj.Type)dataReader.GetInt(),
                        X = dataReader.GetFloat(), Y = dataReader.GetFloat()
                    });
                    break;

                case NetMessage.StartGame:
                    this.SetGameplay();
                    GameStarted?.Invoke(this, EventArgs.Empty);
                    break;
            }
        }


        public event EventHandler ServerClosed;
        public event EventHandler<LobbyClientsCountEventArgs> ClientsCount;
        public event EventHandler<LobbyPlayerEventArgs> PlayerAdded;
        public event EventHandler<LobbyPlayerEventArgs> PlayerRemoved;
        public event EventHandler<LobbyPlayerEventArgs> InstantiateCharacter;
        public event EventHandler<LobbyOptionsArgs> PreparingGameToStart;
        public event EventHandler GameStarted;


        protected virtual void OnServerClosed()
        {
            ServerClosed?.Invoke(this, EventArgs.Empty);
        }


        protected virtual void OnClientsCount(int Count)
        {
            ClientsCount?.Invoke(this, new LobbyClientsCountEventArgs() { ClientsCount = Count });
        }


        internal void RequestNewPlayer(int playerIndex)
        {
            NetDataWriter writer = new NetDataWriter();
            writer.Put((int)NetMessage.AddPlayer);
            writer.Put(playerIndex);
            client.SendToAll(writer, SendOptions.ReliableOrdered);
        }


        internal void RequestStartGame()
        {
            NetDataWriter writer = new NetDataWriter();
            writer.Put((int)NetMessage.PrepareToStartGame);
            client.SendToAll(writer, SendOptions.ReliableOrdered);
        }


        internal void SignalReadyToStart()
        {
            NetDataWriter writer = new NetDataWriter();
            writer.Put((int)NetMessage.ReadyToStart);
            client.SendToAll(writer, SendOptions.ReliableOrdered);
        }

        // ================================


        internal void SendReadyToStart()
        {
            NetDataWriter writer = new NetDataWriter();
            writer.Put((int)NetMessage.ReadyToStart);
            client.SendToAll(writer, SendOptions.ReliableOrdered);
        }


        private void SetGameplay()
        {
            System.Diagnostics.Debug.WriteLine("[CLIENT] SetGameplay");

            listener.PeerConnectedEvent -= OnLobbyPeerConnected;
            listener.NetworkReceiveEvent -= OnLobbyNetworkReceived;
            listener.PeerDisconnectedEvent -= OnLobbyPeerDisconnected;

            listener.NetworkReceiveEvent += OnGameplayNetworkReceived;
        }


        internal void GameplayUpdate(float dt)
        {
            client.PollEvents();
        }


        public event EventHandler<GameplayUpdateEventArgs> CharacterUpdated;


        private void OnGameplayNetworkReceived(NetPeer peer, NetDataReader dataReader)
        {
            NetMessage instruction = (NetMessage)dataReader.GetInt();
            switch (instruction)
            {
                case NetMessage.ServerClosed:
                    OnServerClosed();
                    break;

                case NetMessage.CharacterUpdate:
                    CharacterUpdated?.Invoke(this, new GameplayUpdateEventArgs() { Packet = new StatePacket(dataReader) });
                    break;

                case NetMessage.GoToScoresScreen:
                    this.SetLobby();
                    ScreenManager.ReplaceCurrent(new ScoresScreen());
                    break;
            }
        }
    }
}
