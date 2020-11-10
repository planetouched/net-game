using System;
using System.Collections.Generic;
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
using UnityEngine;
using Timer = Basement.OEPFramework.UnityEngine.Timer;

namespace Server.Simulations
{
    public class ServerSimulation : DroppableItemBase, ISimulation
    {
        private Timer _tickTimer;
        
        private ServerWorld _world;

        private static int _globalGameId = 1;

        private readonly Queue<ControlMessage> _messagesPerTick = new Queue<ControlMessage>();

        private readonly ServerNetListener _serverNetListener;
        private readonly NetDataWriter _worldDataWriter;

        private readonly Dictionary<NetPeer, ServerPlayer> _players = new Dictionary<NetPeer, ServerPlayer>();

        public ServerSimulation(int port)
        {
            _worldDataWriter = new NetDataWriter(false, ushort.MaxValue);
            _serverNetListener = new ServerNetListener(port);
            _serverNetListener.onIncomingMessage += ServerNetListener_IncomingMessage;
            _serverNetListener.onClientDisconnected += ServerNetListener_ClientDisconnected;
            _serverNetListener.onClientConnected += ServerNetListener_ClientConnected;
        }

        public void StartSimulation()
        {
            if (_serverNetListener.isStarted) return;

            _serverNetListener.Start();

            if (!_serverNetListener.isStarted)
            {
                return;
            }

            _world = new ServerWorld(_globalGameId++);
            _world.SetupTime();
            
            var tickDelay = 1 / (float) ServerSettings.TicksCount;
            _tickTimer = Timer.CreateRealtime(tickDelay, Server_Tick, this);

            Log.Write("Server -> StartSimulation");
        }
        
        private void Server_Tick()
        {
            _serverNetListener.PollEvents();
            ProcessSimulation();
        }
        
        public void StopSimulation()
        {
            if (!_serverNetListener.isStarted) return;

            foreach (var peer in _serverNetListener.netManager.ConnectedPeerList)
            {
                try
                {
                    _players.Remove(peer);
                    peer.Disconnect();
                }
                catch (Exception)
                {
                    // ignored
                }
            }

            _serverNetListener.Stop();
            _tickTimer.Drop();
            _world.Drop();
            
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
            if (!_players.ContainsKey(peer))
            {
                _players.Add(peer, new ServerPlayer(new SharedPlayer()));
            }
        }

        private void ServerNetListener_ClientDisconnected(NetPeer peer)
        {
            if (_players.ContainsKey(peer))
            {
                _world.RemoveEntity(_players[peer].sharedEntity.objectId);
                _players.Remove(peer);
            }
        }

        private void ServerNetListener_IncomingMessage(NetPeer peer, MessageBase message)
        {
            if (_players.TryGetValue(peer, out var player))
            {
                var sharedPlayer = (SharedPlayer) player.sharedEntity;
                
                if (!message.system && sharedPlayer.lastMessageNum >= message.messageNum) return;
                
                if (message.messageId == MessageIds.EnterGame)
                {
                    _world.AddEntity(_world.GetNewObjectId(), player);
                    peer.Send(new EnterGameAcceptedMessage()
                        .SetObjectId(sharedPlayer.objectId)
                        .SetGameId(_world.gameId)
                        .Serialize(new NetDataWriter()), DeliveryMethod.ReliableUnordered);
                    
                }
                else
                {
                    if (message.messageId == MessageIds.PlayerControl)
                    {
                        _messagesPerTick.Enqueue((ControlMessage) message);
                    }
                }
            }
        }

        public void ProcessSimulation()
        {
            try
            {
                var preprocessedSnapshot = _world.CreateSnapshot(_world.time, true);
                
                while (_messagesPerTick.Count > 0)
                {
                    var message = _messagesPerTick.Dequeue();
                    var player = _world.FindEntity<ServerPlayer>(message.objectId, GameEntityType.Player);
                    player?.AddControlMessage(message);
                    preprocessedSnapshot.AddControlMessage(message.objectId, message);
                }
                
                //_world.worldRoot.SetActive(true);
                _world.Process();
                
                var snapshot = _world.CreateSnapshot(_world.time, false);

                _worldDataWriter.Reset();
                var snapshotMessage = new WorldSnapshotMessage(_world.GetAndIncrementSnapshotNum(), snapshot.Serialize(_worldDataWriter), _world.time);
                snapshotMessage.SetMessageNum(_world.GetAndIncrementMessageNum()).SetGameId(_world.gameId);

                _serverNetListener.netManager.SendToAll(snapshotMessage.Serialize(new NetDataWriter()), DeliveryMethod.Unreliable);
                
                //_world.worldRoot.SetActive(false);
            }
            catch (Exception)
            {
                StopSimulation();
            }
        }
    }
}