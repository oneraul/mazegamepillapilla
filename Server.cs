using System;
using System.Linq;
using System.Collections.Generic;
using LiteNetLib;
using LiteNetLib.Utils;

namespace MazeGamePillaPilla
{
    class Server
    {
        internal float TickRate { get; private set; }
        internal int Port { get; private set; }
        internal int MaxClients { get; private set; }

        private EventBasedNetListener listener;
        private NetManager server;

        private Dictionary<string, LobbyPlayer> players;
        private Dictionary<NetPeer, List<string>> playersPerClient;

        private List<NetPeer> peersNotReadyToStartYet;

        internal Server()
        {
            Port = 9050;
            MaxClients = 4;
            TickRate = 1/5f;

            players = new Dictionary<string, LobbyPlayer>();
            playersPerClient = new Dictionary<NetPeer, List<string>>();

            listener = new EventBasedNetListener();
            server = new NetManager(listener, MaxClients, "SomeConnectionKey");

            if(!server.Start(Port)) System.Diagnostics.Debug.WriteLine("Error with Server.Start()");

            SetLobby();
        }
        

        ~Server()
        {
            server.Stop();
        }


        internal void Close()
        {
            NetDataWriter writer = new NetDataWriter();
            writer.Put((int)NetMessage.ServerClosed);
            server.SendToAll(writer, SendOptions.ReliableOrdered);

            server.Stop();
        }

        // Lobby ------------------------

        internal void LobbyUpdate(float dt)
        {
            server.PollEvents();

            if (peersNotReadyToStartYet?.Count == 0)
            {
                peersNotReadyToStartYet = null;
                this.SetGameplay();

                NetDataWriter writer = new NetDataWriter();
                writer.Put((int)NetMessage.StartGame);
                server.SendToAll(writer, SendOptions.ReliableOrdered);
            }
        }


        private void SetLobby()
        {
            System.Diagnostics.Debug.WriteLine("[SERVER] SetLobby");
            this.game = null;
            this.lastProcessedInputs = null;
            this.lastSentSnapshots = null;
            listener.NetworkReceiveEvent -= OnGameplayNetworkReceived;

            listener.PeerConnectedEvent += OnLobbyPeerConnected;
            listener.PeerDisconnectedEvent += OnLobbyPeerDisconnected;
            listener.NetworkReceiveEvent += OnLobbyReceived;
        }


        private void PrepareToStartGame()
        {
            peersNotReadyToStartYet = new List<NetPeer>();
            peersNotReadyToStartYet.AddRange(server.GetPeers());

            NetDataWriter writer = new NetDataWriter();
            writer.Put((int)NetMessage.PrepareToStartGame);
            writer.Put(players.Count);
            writer.Put(random.Next(MapData.MapsCount));
            server.SendToAll(writer, SendOptions.ReliableOrdered);

            // instantiate characters
            foreach (KeyValuePair<NetPeer, List<string>> entry in playersPerClient)
            {
                NetPeer peer = entry.Key;
                foreach (string id in entry.Value)
                {
                    LobbyPlayer character = players[id];

                    float x = Tile.Size * (random.Next(2, 10) + (float)random.NextDouble());
                    float y = Tile.Size * (random.Next(2, 10) + (float)random.NextDouble());

                    NetDataWriter localCharacterWriter = new NetDataWriter();
                    localCharacterWriter.Put((int)NetMessage.InstantiateCharacter);
                    localCharacterWriter.Put(id);
                    localCharacterWriter.Put((int)character.PlayerControllerIndex);
                    localCharacterWriter.Put((int)Pj.Type.Local);
                    localCharacterWriter.Put(x);
                    localCharacterWriter.Put(y);
                    peer.Send(localCharacterWriter, SendOptions.ReliableOrdered);

                    NetDataWriter remoteCharacterWriter = new NetDataWriter();
                    remoteCharacterWriter.Put((int)NetMessage.InstantiateCharacter);
                    remoteCharacterWriter.Put(id);
                    remoteCharacterWriter.Put((int)character.PlayerControllerIndex);
                    remoteCharacterWriter.Put((int)Pj.Type.Remote);
                    remoteCharacterWriter.Put(x);
                    remoteCharacterWriter.Put(y);
                    server.SendToAll(remoteCharacterWriter, SendOptions.ReliableOrdered, peer);
                }
            }
        }


        private void OnLobbyPeerConnected(NetPeer peer)
        {
            System.Diagnostics.Debug.WriteLine("[SERVER] client connected: {0}", peer.EndPoint);
            
            // set the new client up to date
            foreach(KeyValuePair<string, LobbyPlayer> kvp in players)
            {
                string PlayerID = kvp.Key;
                int PlayerControllerIndex = (int)kvp.Value.PlayerControllerIndex;

                NetDataWriter writer = new NetDataWriter();
                writer.Put((int)NetMessage.AddPlayer);
                writer.Put(PlayerID);
                writer.Put(PlayerControllerIndex);
                peer.Send(writer, SendOptions.ReliableOrdered);
            }

            // add the new client and tell everybody about them
            playersPerClient.Add(peer, new List<string>()); 
            UpdatePeersCount();
        }

        private void OnLobbyPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
        {
            System.Diagnostics.Debug.WriteLine($"[SERVER] client disconnected: {peer.EndPoint}");

            foreach (string playerID in playersPerClient[peer])
            {
                RemovePlayer(playerID);
            }
            playersPerClient.Remove(peer);

            UpdatePeersCount();
        }

        private void UpdatePeersCount()
        {
            NetDataWriter writer = new NetDataWriter();
            writer.Put((int)NetMessage.ClientsCount);
            writer.Put(server.PeersCount);
            server.SendToAll(writer, SendOptions.ReliableOrdered);
        }

        private void AddPlayer(NetPeer peer, int playerControllerIndex)
        {
            string playerID = RandomString(4);

            playersPerClient[peer].Add(playerID);
            players.Add(playerID, new LobbyPlayer(playerID, (PlayerControllerIndex)playerControllerIndex));

            NetDataWriter writer = new NetDataWriter();
            writer.Put((int)NetMessage.AddPlayer);
            writer.Put(playerID);
            writer.Put(playerControllerIndex);
            server.SendToAll(writer, SendOptions.ReliableOrdered);
        }

        private void RemovePlayer(string playerID)
        {
            NetDataWriter writer = new NetDataWriter();
            writer.Put((int)NetMessage.RemovePlayer);
            writer.Put(playerID);
            server.SendToAll(writer, SendOptions.ReliableOrdered);

            players.Remove(playerID);
        }

        private void OnLobbyReceived(NetPeer peer, NetDataReader dataReader)
        {
            NetMessage instruction = (NetMessage)dataReader.GetInt();
            switch(instruction)
            {
                case NetMessage.AddPlayer:
                    AddPlayer(peer, dataReader.GetInt());
                    break;

                case NetMessage.PrepareToStartGame:
                    if (peer == server.GetFirstPeer())
                    {
                        PrepareToStartGame();
                    }
                    break;

                case NetMessage.ReadyToStart:
                    peersNotReadyToStartYet?.Remove(peer);
                    break;
            }
        }


        // Gameplay ---------------------

        private float updateAccumulator;
        internal GameScreen game;
        internal Dictionary<string, long> lastProcessedInputs;
        internal Dictionary<string, long> lastSentSnapshots;


        private void SetGameplay()
        {
            System.Diagnostics.Debug.WriteLine("[SERVER] SetGameplay");
            listener.PeerConnectedEvent -= OnLobbyPeerConnected;
            listener.PeerDisconnectedEvent -= OnLobbyPeerDisconnected;
            listener.NetworkReceiveEvent -= OnLobbyReceived;

            listener.NetworkReceiveEvent += OnGameplayNetworkReceived;
        }


        internal void GameplayUpdate(float dt)
        {
            updateAccumulator += dt;
            if (updateAccumulator >= TickRate)
            {
                updateAccumulator -= TickRate;
                server.PollEvents();

                foreach (Pj pj in game.Pjs.Values ?? Enumerable.Empty<Pj>())
                {
                    if (lastProcessedInputs[pj.ID] > lastSentSnapshots[pj.ID])
                    {
                        lastSentSnapshots[pj.ID] = lastProcessedInputs[pj.ID];
                        StatePacket statePacket = new StatePacket(pj.ID, lastProcessedInputs[pj.ID], pj);
                        server.SendToAll(statePacket.Serialize(), SendOptions.Unreliable);
                    }
                }
            }
        }


        private void OnGameplayNetworkReceived(NetPeer peer, NetDataReader dataReader)
        {
            int instruction = dataReader.GetInt();
            switch (instruction)
            {
                case (int)NetMessage.CharacterUpdate:

                    InputPacket inputPacket = new InputPacket(dataReader);
                    if (game.Pjs.TryGetValue(inputPacket.CharacterID, out Pj pj))
                    {
                        lastProcessedInputs[inputPacket.CharacterID] = inputPacket.InputSequenceNumber;
                        pj.ApplyInputOnTheServer(inputPacket, game.maze);
                    }
                    break;
            }
        }


        internal void GoToScoresScreen()
        {
            NetDataWriter writer = new NetDataWriter();
            writer.Put((int)NetMessage.GoToScoresScreen);
            server.SendToAll(writer, SendOptions.ReliableOrdered);

            this.SetLobby();
        }


        // https://stackoverflow.com/a/1344242
        private static Random random = new Random();
        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}
