using System;
using System.Linq;
using System.Collections.Generic;
using LiteNetLib;
using LiteNetLib.Utils;
using MazeGamePillaPilla.PowerUps;

namespace MazeGamePillaPilla
{
    class Server
    {
        public static readonly float TickRate = 1f/30;

        public int Port { get; private set; }
        public int MaxClients { get; private set; }

        private EventBasedNetListener listener;
        private NetManager server;

        private Dictionary<string, LobbyPlayer> players;
        private Dictionary<NetPeer, List<string>> playersPerClient;

        private List<NetPeer> peersNotReadyToStartYet;

        public Server()
        {
            Port = 9050;
            MaxClients = 4;

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


        public void Close()
        {
            NetDataWriter writer = new NetDataWriter();
            writer.Put((int)NetMessage.ServerClosed);
            server.SendToAll(writer, SendOptions.ReliableOrdered);

            server.Stop();
        }

        // Lobby ------------------------

        public void LobbyUpdate(float dt)
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
            this.updateAccumulator = 0;
            this.dropsCount = 0;
            this.world = null;
            this.lastProcessedInputs = null;
            this.lastSentSnapshots = null;
            this.lastBuff = null;
            listener.NetworkReceiveEvent -= OnGameplayNetworkReceived;

            listener.PeerConnectedEvent += OnLobbyPeerConnected;
            listener.PeerDisconnectedEvent += OnLobbyPeerDisconnected;
            listener.NetworkReceiveEvent += OnLobbyReceived;
        }


        private void PrepareToStartGame()
        {
            peersNotReadyToStartYet = new List<NetPeer>();
            peersNotReadyToStartYet.AddRange(server.GetPeers());

            int mapId = random.Next(MapData.MapsCount);

            world = new GameWorld { maze = Cell.ParseData(MapData.GetMap(mapId)) };
            lastProcessedInputs = new Dictionary<string, long>();
            lastSentSnapshots = new Dictionary<string, long>();
            lastBuff = new Dictionary<string, int>();

            NetDataWriter writer = new NetDataWriter();
            writer.Put((int)NetMessage.PrepareToStartGame);
            writer.Put(players.Count);
            writer.Put(mapId);
            server.SendToAll(writer, SendOptions.ReliableOrdered);

            // instantiate characters
            foreach (KeyValuePair<NetPeer, List<string>> entry in playersPerClient)
            {
                NetPeer peer = entry.Key;
                foreach (string id in entry.Value)
                {
                    LobbyPlayer character = players[id];

                    RemotePj pjCopyInTheServer = new RemotePj(id, 0, 0, 1);
                    pjCopyInTheServer.SpawnInAnEmptyPosition(world.maze);
                    world.Pjs.Add(id, pjCopyInTheServer);
                    lastProcessedInputs.Add(id, 0);
                    lastSentSnapshots.Add(id, 0);
                    lastBuff.Add(id, 0);

                    float x = pjCopyInTheServer.x;
                    float y = pjCopyInTheServer.y;

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
            System.Diagnostics.Debug.WriteLine($"[SERVER] client connected: {peer.EndPoint}");
            
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
        private int dropsCount;
        public Dictionary<string, long> lastProcessedInputs;
        public Dictionary<string, long> lastSentSnapshots;
        public Dictionary<string, int> lastBuff;

        public GameWorld world;


        private void SetGameplay()
        {
            System.Diagnostics.Debug.WriteLine("[SERVER] SetGameplay");
            listener.PeerConnectedEvent -= OnLobbyPeerConnected;
            listener.PeerDisconnectedEvent -= OnLobbyPeerDisconnected;
            listener.NetworkReceiveEvent -= OnLobbyReceived;

            listener.NetworkReceiveEvent += OnGameplayNetworkReceived;
        }


        public void GameplayUpdate(float dt)
        {
            if (world == null) return;

            updateAccumulator += dt;
            if (updateAccumulator >= TickRate)
            {
                updateAccumulator -= TickRate;
                server.PollEvents();

                foreach (Pj pj in world.Pjs.Values ?? Enumerable.Empty<Pj>())
                {
                    long lastInput = lastProcessedInputs[pj.ID];
                    if (lastInput > lastSentSnapshots[pj.ID])
                    {
                        lastSentSnapshots[pj.ID] = lastInput;
                        StatePacket statePacket = new StatePacket(lastInput, pj);
                        server.SendToAll(statePacket.Serialize(), SendOptions.Unreliable);
                    }

                    List<int> buffsToRemove = new List<int>();
                    foreach (KeyValuePair<int, Buff> kvp in pj.Buffs)
                    {
                        Buff buff = kvp.Value;
                        buff.Update(TickRate);
                        if (buff.ToBeRemoved)
                        {
                            buffsToRemove.Add(kvp.Key);
                        }
                    }
                    foreach (int buffId in buffsToRemove)
                    {
                        RemoveBuff(pj, buffId);
                    }
                }

                List<int> dropsToRemove = new List<int>();
                foreach (KeyValuePair<int, Drop> kvp in world.Drops)
                {
                    Drop drop = kvp.Value;
                    foreach (Pj pj in world.Pjs.Values)
                    {
                        if (drop.AabbAabbIntersectionTest(pj))
                        {
                            drop.Callback(pj, this);
                            dropsToRemove.Add(kvp.Key);
                        }
                    }
                }
                foreach (int index in dropsToRemove)
                {
                    RemoveDrop(index);
                }
            }
        }


        private void OnGameplayNetworkReceived(NetPeer peer, NetDataReader dataReader)
        {
            int instruction = dataReader.GetInt();
            switch (instruction)
            {
                case (int)NetMessage.CharacterUpdate:
                    ProcessInputPacket(new InputPacket(dataReader));
                    break;
            }
        }


        private void ProcessInputPacket(InputPacket inputPacket)
        {
            if (world.Pjs.TryGetValue(inputPacket.CharacterID, out Pj pj))
            {
                lastProcessedInputs[inputPacket.CharacterID] = inputPacket.InputSequenceNumber;
                pj.ApplyInputOnTheServer(inputPacket, world.maze);

                if (inputPacket.Action)
                {
                    if (pj.PowerUp != null && !pj.Stunned)
                    {
                        pj.PowerUp.Action(pj, this);
                        RemovePowerUp(pj);
                    }
                }
            }
            else throw new System.ComponentModel.InvalidEnumArgumentException();
        }


        public void GoToScoresScreen()
        {
            NetDataWriter writer = new NetDataWriter();
            writer.Put((int)NetMessage.GoToScoresScreen);
            server.SendToAll(writer, SendOptions.ReliableOrdered);

            this.SetLobby();
        }


        public void AddDrop(int type, int x, int y)
        {
            NetDataWriter writer = new NetDataWriter();
            writer.Put((int)NetMessage.AddDrop);
            writer.Put(dropsCount); // id
            writer.Put(type);
            writer.Put(x);
            writer.Put(y);
            server.SendToAll(writer, SendOptions.ReliableUnordered);

            world.OnDropAdded(this, new GameplayDropEventArgs()
                { Id = dropsCount, Type = type, X = x, Y = y });

            dropsCount++;
        }

        public void RemoveDrop(int id)
        {
            NetDataWriter writer = new NetDataWriter();
            writer.Put((int)NetMessage.RemoveDrop);
            writer.Put(id);
            server.SendToAll(writer, SendOptions.ReliableUnordered);

            world.OnDropRemoved(this, new GameplayDropEventArgs() { Id = id });
        }

        public void AddBuff(int type, Pj pj)
        {
            int buffId = lastBuff[pj.ID]++;

            NetDataWriter writer = new NetDataWriter();
            writer.Put((int)NetMessage.AddBuff);
            writer.Put(type);
            writer.Put(pj.ID);
            writer.Put(buffId);
            server.SendToAll(writer, SendOptions.ReliableUnordered);

            world.OnBuffAdded(this, new GameplayBuffEventArgs()
                { BuffType = type, PlayerId = pj.ID, BuffId = buffId });
        }

        public void RemoveBuff(Pj pj, int buffId)
        {
            NetDataWriter writer = new NetDataWriter();
            writer.Put((int)NetMessage.RemoveBuff);
            writer.Put(pj.ID);
            writer.Put(buffId);
            server.SendToAll(writer, SendOptions.ReliableUnordered);

            world.OnBuffRemoved(this, new GameplayBuffEventArgs()
                { PlayerId = pj.ID, BuffId = buffId });
        }

        public void AddPowerUp(int type, Pj pj)
        {
            NetDataWriter writer = new NetDataWriter();
            writer.Put((int)NetMessage.AddPowerUp);
            writer.Put(pj.ID);
            writer.Put(type);
            server.SendToAll(writer, SendOptions.ReliableUnordered);

            world.OnPowerUpAdded(this, new GameplayPowerUpEventArgs()
                { PlayerId = pj.ID, Type = type });
        }

        public void RemovePowerUp(Pj pj)
        {
            NetDataWriter writer = new NetDataWriter();
            writer.Put((int)NetMessage.RemovePowerUp);
            writer.Put(pj.ID);
            server.SendToAll(writer, SendOptions.ReliableUnordered);

            world.OnPowerUpRemoved(this, new GameplayPowerUpEventArgs() { PlayerId = pj.ID });
        }

        // Forces all the clients to teleport the pj to the same position as it is in the server
        public void TeleportPj(Pj pj)
        {
            NetDataWriter writer = new NetDataWriter();
            writer.Put((int)NetMessage.Teleport);
            writer.Put(pj.ID);
            writer.Put(pj.x);
            writer.Put(pj.y);
            server.SendToAll(writer, SendOptions.ReliableUnordered);
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
