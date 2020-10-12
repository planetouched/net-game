using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using Server.Game.Entities;
using Server.Game.Requests;
using Server.Game.Requests._Base;
using Server.Game.Worlds;
using Server.Game.Worlds._Base;
using Shared;
using Shared.Enums;
using Shared.Game._Base;
using Shared.Messages._Base;
using Shared.Messages.FromClient;
using Shared.Messages.FromServer;
using SimpleTcp;

namespace Server.Game
{
    public class ServerSimulation : ISimulation
    {
        private readonly IServerWorld _world;
        
        private uint _snapshotNum;
        private Thread _tickThread;
        private volatile bool _update;
        private readonly ConcurrentQueue<IRequest> _requests = new ConcurrentQueue<IRequest>();
        private readonly Queue<IMessage> _messages = new Queue<IMessage>();
        
        private readonly byte[] _outputBuffer = new byte[SharedSettings.MaxMessageSize];

        private SimpleTcpServer _server;
        private readonly string _serverIp;
        private readonly int _port;
        private readonly uint _gameId;
        
        private readonly Dictionary<string, ServerPlayer> _players = new Dictionary<string, ServerPlayer>();

        public ServerSimulation(string serverIp, int port, uint gameId)
        {
            _gameId = gameId;
            _serverIp = serverIp;
            _port = port;
            
            _world = new ServerWorld();
        }

        public void Start()
        {
            if (_server != null) return;
            
            _server = new SimpleTcpServer(_serverIp, _port, false, null, null);
            
            _server.Settings.MutuallyAuthenticate = false;
            _server.Settings.AcceptInvalidCertificates = true;
            
            _server.Events.ClientConnected += Client_Connected;
            _server.Events.ClientDisconnected += Client_Disconnected;
            _server.Events.DataReceived += Client_DataReceived;
            
            _tickThread = new Thread(Thread_Tick) {IsBackground = true};
            _tickThread.Start();
            
            _server.Start();
        }

        private void Client_DataReceived(object sender, DataReceivedFromClientEventArgs e)
        {
            _requests.Enqueue(new DataReceiveRequest(e.IpPort, e.Data, _players, _messages));
        }

        private void Client_Disconnected(object sender, ClientDisconnectedEventArgs e)
        {
            UnityEngine.Debug.Log("disconnect player");
            _requests.Enqueue(new DisconnectPlayerRequest(e.IpPort, _world, _players));
        }

        private void Client_Connected(object sender, ClientConnectedEventArgs e)
        {
            UnityEngine.Debug.Log("connect player");
            _requests.Enqueue(new ConnectPlayerRequest(_server, e.IpPort, _world, _gameId, _players));
        }

        private void ProcessMessage(IMessage message)
        {
            if (message.GetMessageId() == MessageIds.PlayerControl)
            {
                var controlMessage = (ControlMessage) message;
                
                var player = _world.FindEntity<ServerPlayer>(controlMessage.objectId, GameEntityType.Player);
                
                if (player != null)
                {
                    if (player.lastMessageNum < controlMessage.messageNum)
                    {
                        player.AddControlMessage(controlMessage);
                    }
                }
            }
        }
        
        public void Process()
        {
            while (_requests.TryDequeue(out var request))
            {
                request.Process();
            }
            
            while (_messages.Count > 0)
            {
                ProcessMessage(_messages.Dequeue());
            }

            _world.Process();
            
            try
            {
                _snapshotNum++;
                int offset = 0;
                
                _world.Serialize(ref offset, _outputBuffer);

                var message = new WorldSnapshotMessage
                {
                    snapshotNum = _snapshotNum, 
                    snapshotSize = offset, 
                    data = _outputBuffer
                };

                var bytes = MessageBase.ConvertToBytes(message);
                
                foreach (var pair in _players)
                {
                    try
                    {
                        _server.Send(pair.Value.ipPort, bytes);
                    }
                    catch (ArgumentNullException)
                    {
                        _server.DisconnectClient(pair.Value.ipPort);
                    }
                }
            }
            catch (Exception)
            {
                Stop();
                throw new Exception("error... stop simulation");
            }
        }

        public void Stop()
        {
            if (_server == null) return;
            
            foreach (var pair in _players)
            {
                try
                {
                    _server.DisconnectClient(pair.Value.ipPort);
                }
                catch (Exception)
                {
                    // ignored
                }
            }            
            
            _server.Events.ClientConnected -= Client_Connected;
            _server.Events.ClientDisconnected -= Client_Disconnected;
            _server.Events.DataReceived -= Client_DataReceived;
            
            _server.Dispose();
            _server = null;
            
            _update = false;
            _tickThread?.Join();
        }

        private void Thread_Tick()
        {
            _update = true;
            int delay = (int) (1 / (float) ServerSettings.TicksCount * 1000);

            while (_update)
            {
                Process();
                Thread.Sleep(delay);
            }
        }
    }
}