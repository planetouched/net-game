using System;
using System.Collections.Generic;
using System.Threading;
using LiteNetLib;
using LiteNetLib.Utils;
using Server.Entities;
using Server.Network;
using Server.Worlds;
using Server.Worlds._Base;
using Shared.Entities;
using Shared.Enums;
using Shared.Messages._Base;
using Shared.Messages.FromClient;
using Shared.Messages.FromServer;
using Shared.Simulations;
using UnityEngine;
using Logger = Shared.Loggers.Logger;
using Vector3 = System.Numerics.Vector3;

namespace Server.Simulations
{
    public class ServerSimulation : ISimulation
    {
        private readonly IServerWorld _world;

        private uint _snapshotNum;
        private uint _messageNum;
        private Thread _tickThread;

        private volatile bool _update;

        private readonly List<ControlMessage> _messagesPerTick = new List<ControlMessage>();
        private readonly List<WorldSnapshotMessage> _snapshots = new List<WorldSnapshotMessage>();

        private readonly int _gameId;
        private readonly ServerNetListener _serverNetListener;
        private NetDataWriter _worldDataWriter;

        private DateTime _lastTime;

        private readonly Dictionary<NetPeer, ServerPlayer> _players = new Dictionary<NetPeer, ServerPlayer>();

        public ServerSimulation(int port, int gameId)
        {
            _gameId = gameId;
            _worldDataWriter = new NetDataWriter(false, ushort.MaxValue);
            _serverNetListener = new ServerNetListener(port);
            _serverNetListener.onIncomingMessage += ServerNetListener_IncomingMessage;
            _serverNetListener.onClientDisconnected += ServerNetListener_ClientDisconnected;
            _serverNetListener.onClientConnected += ServerNetListener_ClientConnected;
            _world = new ServerWorld();
        }

        public void StartSimulation()
        {
            if (_serverNetListener.isStarted) return;

            _serverNetListener.Start();

            if (!_serverNetListener.isStarted)
            {
                return;
            }

            _tickThread = new Thread(Thread_Tick) {IsBackground = true};
            _tickThread.Start();

            Logger.Log("Server -> StartSimulation");
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

        private void ServerNetListener_IncomingMessage(NetPeer peer, IMessage message)
        {
            if (_players.TryGetValue(peer, out var player))
            {
                var sharedPlayer = (SharedPlayer) player.sharedEntity;
                
                if (sharedPlayer.lastMessageNum >= message.messageNum) return;
                
                if (message.messageId == MessageIds.EnterGame)
                {
                    _world.AddEntity(_world.GetNewObjectId(), player);
                    peer.Send(new EnterGameAcceptedMessage(++_messageNum, sharedPlayer.objectId, _gameId).Serialize(new NetDataWriter()), DeliveryMethod.Unreliable);
                    sharedPlayer.position = new Vector3(0, 1, 0);
                }
                else
                {
                    if (!_world.Exists(message.objectId)) return;
                    
                    if (message.messageId == MessageIds.PlayerControl)
                    {
                        var controlMessage = (ControlMessage) message;
                        player.AddControlMessage(controlMessage);
                        _messagesPerTick.Add(controlMessage);
                    }
                }
            }
        }

        public void ProcessSimulation()
        {
            var currentTime = DateTime.UtcNow;
            var deltaTime = (float)(currentTime - _lastTime).TotalSeconds; 
            _lastTime = currentTime;
            
            _world.Process(deltaTime);

            try
            {
                _worldDataWriter = _world.Serialize(_worldDataWriter);

                var snapshot = new WorldSnapshotMessage(++_messageNum, MessageIds.WorldSnapshot, ++_snapshotNum, _worldDataWriter, _gameId, deltaTime);
                snapshot.messages.AddRange(_messagesPerTick);
                _messagesPerTick.Clear();

                _snapshots.Add(snapshot);

                if (_snapshots.Count > 1024)
                {
                    _snapshots.RemoveAt(0);
                }

                var snapshotDataWriter = snapshot.Serialize(new NetDataWriter());
                _serverNetListener.netManager.SendToAll(snapshotDataWriter, DeliveryMethod.Unreliable);
            }
            catch (Exception)
            {
                StopSimulation();
            }
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

            _update = false;
            _tickThread?.Join();
            Logger.Log("Server -> StopSimulation");
        }

        private void Thread_Tick()
        {
            Logger.Log("Thread_Tick: " + Thread.CurrentThread.ManagedThreadId);
            _update = true;
            int tickDelay = (int) (1 / (float) ServerSettings.TicksCount * 1000);
            _lastTime = DateTime.UtcNow;
            Thread.Sleep(tickDelay);

            while (_update)
            {
                ProcessSimulation();
                _serverNetListener.PollEvents();
                Thread.Sleep(tickDelay);
            }
        }
    }
}