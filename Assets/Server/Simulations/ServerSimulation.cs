using System;
using System.Collections.Generic;
using System.Linq;
using Basement.OEPFramework.UnityEngine._Base;
using LiteNetLib;
using LiteNetLib.Utils;
using Server.Entities;
using Server.Network;
using Server.Worlds;
using Shared.Entities;
using Shared.Enums;
using Shared.Loggers;
using Shared.Messages._Base;
using Shared.Messages.FromClient;
using Shared.Messages.FromServer;
using Shared.Simulations;

namespace Server.Simulations
{
    public class ServerSimulation : DroppableItemBase, ISimulation
    {
        private static int _globalWorldId = 1;

        private readonly Queue<ControlMessage> _messagesPerTick = new Queue<ControlMessage>();

        private readonly ServerNetListener _serverNetListener;
        private readonly NetDataWriter _worldDataWriter;

        private readonly Dictionary<int, ServerWorld> _worlds = new Dictionary<int, ServerWorld>();
        private readonly Dictionary<int, Dictionary<NetPeer, ServerPlayer>> _world2Players = new Dictionary<int, Dictionary<NetPeer, ServerPlayer>>();
        private readonly Dictionary<NetPeer, ServerPlayer> _allPlayers = new Dictionary<NetPeer, ServerPlayer>();
        
        private readonly List<int> _worldsToRemove = new List<int>();
        
        public ServerSimulation(int port)
        {
            _worldDataWriter = new NetDataWriter(false, ushort.MaxValue);
            _serverNetListener = new ServerNetListener(port);
            _serverNetListener.onIncomingMessage += ServerNetListener_IncomingMessage;
            _serverNetListener.onClientDisconnected += ServerNetListener_ClientDisconnected;
            _serverNetListener.onClientConnected += ServerNetListener_ClientConnected;
        }
        
        public void CreateWorld()
        {
            var world = new ServerWorld(_globalWorldId++);
            world.SetupTime();
            _worlds.Add(world.worldId, world);
            _world2Players.Add(world.worldId, new Dictionary<NetPeer, ServerPlayer>());
        }

        private void RemoveWorld(int gameId)
        {
            foreach (var pair in _world2Players[gameId])
            {
                try
                {
                    _allPlayers.Remove(pair.Key);
                    pair.Key.Disconnect();
                }
                catch
                {
                    //ignored
                }                    
            }

            _world2Players.Remove(gameId);
            _worlds[gameId].Drop();
            _worlds.Remove(gameId);
        }
        
        public void StartSimulation()
        {
            if (_serverNetListener.isStarted) return;

            _serverNetListener.Start();

            if (!_serverNetListener.isStarted)
            {
                return;
            }

            Log.Write("Server -> StartSimulation");
        }
        
        public void StopSimulation()
        {
            if (!_serverNetListener.isStarted) return;

            _serverNetListener.Stop();

            foreach (var worldId in _worlds.Keys.ToArray())
            {
                RemoveWorld(worldId);
            }
            
            Log.Write("Server -> StopSimulation");
        }
        
        public override void Drop()
        {
            if (dropped) return;
            StopSimulation();
            base.Drop();
        }

        private void ServerNetListener_ClientConnected(NetPeer peer)
        {
            peer.Send(new GetWorldsListMessage(_worlds).Serialize(new NetDataWriter()), DeliveryMethod.ReliableUnordered);
        }

        private void ServerNetListener_ClientDisconnected(NetPeer peer)
        {
            if (_allPlayers.ContainsKey(peer))
            {
                var player = _allPlayers[peer];
                player.world.RemoveEntity(player.sharedEntity.objectId);
                _allPlayers.Remove(peer);
                _world2Players[player.world.worldId].Remove(peer);
            }
        }

        private void ServerNetListener_IncomingMessage(NetPeer peer, MessageBase message)
        {
            if (_allPlayers.TryGetValue(peer, out var player))
            {
                if (message.system) return;
                
                var sharedPlayer = (SharedPlayer) player.sharedEntity;
                
                if (sharedPlayer.lastMessageNum >= message.messageNum) return;
                
                if (message.messageId == MessageIds.PlayerControl)
                {
                    _messagesPerTick.Enqueue((ControlMessage) message);
                }
            }
            else
            {
                if (!message.system) return;
                
                if (message.messageId == MessageIds.EnterGame)
                {
                    var newPlayer = new ServerPlayer(new SharedPlayer()); 
                    
                    var world = _worlds[message.worldId];
                    uint newObjectId = world.AddEntity(newPlayer);
                    
                    _allPlayers.Add(peer, newPlayer);
                    _world2Players[message.worldId].Add(peer, newPlayer);
                    
                    peer.Send(new EnterGameAcceptedMessage()
                        .SetObjectId(newObjectId)
                        .SetWorldId(world.worldId)
                        .Serialize(new NetDataWriter()), DeliveryMethod.ReliableUnordered);
                }
            }
        }

        public void ProcessSimulation()
        {
            _serverNetListener.PollEvents();

            foreach (var world in _worlds.Values)
            {
                try
                {
                    var preprocessedSnapshot = world.CreateSnapshot(world.time, true);
                
                    while (_messagesPerTick.Count > 0)
                    {
                        var message = _messagesPerTick.Dequeue();
                        var player = world.FindEntity<ServerPlayer>(message.objectId, GameEntityType.Player);
                        player?.AddControlMessage(message);
                        preprocessedSnapshot.AddControlMessage(message.objectId, message);
                    }
                
                    world.Process();
                
                    var snapshot = world.CreateSnapshot(world.time, false);

                    _worldDataWriter.Reset();
                    var snapshotMessage = new WorldSnapshotMessage(world.GetAndIncrementSnapshotNum(), snapshot.Serialize(_worldDataWriter), world.time);
                    snapshotMessage.SetMessageNum(world.GetAndIncrementMessageNum()).SetWorldId(world.worldId);

                    _serverNetListener.netManager.SendToAll(snapshotMessage.Serialize(new NetDataWriter()), DeliveryMethod.Unreliable);
                }
                catch (Exception)
                {
                    //something went wrong
                    _worldsToRemove.Add(world.worldId);
                }
            }

            for (int i = 0; i < _worldsToRemove.Count; i++)
            {
                RemoveWorld(_worldsToRemove[i]);
            }
            
            _worldsToRemove.Clear();
        }
    }
}