using System;
using System.Collections.Generic;
using System.Threading;
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
using Vector3 = System.Numerics.Vector3;

namespace Server.Simulations
{
    public class ServerSimulation : ISimulation
    {
        private readonly ServerWorld _world;

        private uint _snapshotNum;
        private uint _messageNum;
        private Thread _tickThread;

        private volatile bool _update;

        private readonly Queue<ControlMessage> _messagesPerTick = new Queue<ControlMessage>();

        private readonly int _gameId;
        private readonly ServerNetListener _serverNetListener;
        private readonly NetDataWriter _worldDataWriter;

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

            Log.Write("Server -> StartSimulation");
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
                    peer.Send(new EnterGameAcceptedMessage()
                        .SetMessageNum(++_messageNum)
                        .SetObjectId(sharedPlayer.objectId)
                        .SetGameId(_gameId)
                        .Serialize(new NetDataWriter()), DeliveryMethod.ReliableUnordered);
                    
                    sharedPlayer.position = new Vector3(0, 1, 0);
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
                var preprocessSnapshot = (WorldSnapshot)_world.CreateSnapshot(_world.time, true);
                
                while (_messagesPerTick.Count > 0)
                {
                    var message = _messagesPerTick.Dequeue();
                    var player = _world.FindEntity<ServerPlayer>(message.objectId, GameEntityType.Player);
                    player?.AddControlMessage(message);
                    preprocessSnapshot.AddControlMessage(message.objectId, message);
                }
                
                _world.Process();
                
                var snapshot = _world.CreateSnapshot(_world.time, false);

                _worldDataWriter.Reset();
                var snapshotMessage = new WorldSnapshotMessage(++_snapshotNum, snapshot.Serialize(_worldDataWriter), _world.time);
                snapshotMessage.SetMessageNum(++_messageNum).SetGameId(_gameId);

                _serverNetListener.netManager.SendToAll(snapshotMessage.Serialize(new NetDataWriter()), DeliveryMethod.Unreliable);
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
            Log.Write("Server -> StopSimulation");
        }

        private void Thread_Tick()
        {
            int tickDelay = (int) (1 / (float) ServerSettings.TicksCount * 1000);
            
            _update = true;
            _world.SetupTime();

            while (_update)
            {
                ProcessSimulation();
                _serverNetListener.PollEvents();
                Thread.Sleep(tickDelay);
            }
        }
    }
}