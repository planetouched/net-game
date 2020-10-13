using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Basement.OEPFramework.UnityEngine.Behaviour;
using Basement.OEPFramework.UnityEngine.Loop;
using Client.Entities;
using Client.Worlds;
using Shared;
using Shared.Decoders;
using Shared.Enums;
using Shared.Factories;
using Shared.Messages._Base;
using Shared.Messages.FromClient;
using Shared.Messages.FromServer;
using Shared.Simulations;
using SimpleTcp;
using UnityEngine;

namespace Client.Simulations
{
    public class ClientSimulation : ControlLoopBehaviour, ISimulation
    {
        //simulation
        private ClientWorld _clientWorld;
        private ClientPlayer _localPlayer;

        private readonly List<ControlMessage> _messages = new List<ControlMessage>(128);
        private readonly ConcurrentQueue<WorldSnapshotMessage> _snapshots = new ConcurrentQueue<WorldSnapshotMessage>();

        private uint _messageNum;
        private uint _lastSnapshotNum;
        private uint _objectId;
        private uint _gameId;

        //network
        private SimpleTcpClient _tcpClient;
        private readonly ByteToMessageDecoder _byteToMessageDecoder;
        private readonly string _serverIp;
        private readonly int _port;
        
        private readonly object _readLock = new object();

        public ClientSimulation(string serverIp, int port)
        {
            _serverIp = serverIp;
            _port = port;
            _byteToMessageDecoder = new ByteToMessageDecoder(SharedSettings.MaxMessageSize);

            LoopOn(Loops.UPDATE, Process);
        }

        private void FillControlMessage(ControlMessage message)
        {
            if (Input.GetKey(KeyCode.W))
                message.forward = true;

            if (Input.GetKey(KeyCode.S))
                message.backward = true;

            if (Input.GetKey(KeyCode.A))
                message.left = true;

            if (Input.GetKey(KeyCode.D))
                message.right = true;

            message.mouseX = Input.GetAxis("Mouse X");
            message.mouseY = -Input.GetAxis("Mouse Y");
            message.sensitivity = ClientSettings.MouseSensitivity;
        }

        public void Start()
        {
            Stop();

            _tcpClient = new SimpleTcpClient(_serverIp, _port, false, null, null);
            _tcpClient.Events.Connected += Client_Connected;
            _tcpClient.Events.Disconnected += Client_Disconnected;
            _tcpClient.Events.DataReceived += Client_DataReceived;
            
            _tcpClient.Settings.MutuallyAuthenticate = false;
            _tcpClient.Settings.AcceptInvalidCertificates = true;
            
            _tcpClient.Connect();
            
            if (!_tcpClient.IsConnected)
            {
                Stop();
                return;
            }

            var connectMessage = MessageFactory.Create<ConnectMessage>(MessageIds.Connect);
            connectMessage.gameId = _gameId;

            //_tcpClient.Send(MessageBase.ConvertToBytes(connectMessage));
        }

        public void Stop()
        {
            if (_tcpClient == null) return;
            
            _tcpClient.Events.Connected -= Client_Connected;
            _tcpClient.Events.Disconnected -= Client_Disconnected;
            _tcpClient.Events.DataReceived -= Client_DataReceived;
            _tcpClient.Dispose();
            _tcpClient = null;
                
            Pause();
            _clientWorld?.Drop();
        }

        public void Process()
        {
            var controlMessage = MessageFactory.Create<ControlMessage>(MessageIds.PlayerControl);
            FillControlMessage(controlMessage);

            _messageNum++;
            controlMessage.gameId = _gameId;
            controlMessage.objectId = _objectId;
            controlMessage.messageNum = _messageNum;
            controlMessage.deltaTime = Time.deltaTime;

            _messages.Add(controlMessage);

            //prediction
            _localPlayer.AddControlMessage(controlMessage);
            _localPlayer.Process();

            //rewind
            while (_snapshots.TryDequeue(out var snapshot))
            {
                if (snapshot.snapshotSize == 0)
                {
                    continue;
                }

                var snapshotView = new ClientWorldSnapshot(snapshot);

                if (_lastSnapshotNum < snapshot.snapshotNum)
                {
                    Rewind(snapshotView);
                    _clientWorld.AddWorldSnapshot(snapshotView);
                    _lastSnapshotNum = snapshot.snapshotNum;
                }
            }

            _tcpClient.Send(MessageBase.ConvertToBytes(controlMessage));

            _clientWorld.Update();
        }

        public override void Drop()
        {
            if (dropped) return;
            Stop();
            base.Drop();
        }

        private void Rewind(ClientWorldSnapshot clientWorldSnapshot)
        {
            var playerOnServer = clientWorldSnapshot.FindEntity<ClientPlayer>(_objectId, GameEntityType.Player);

            if (playerOnServer == null)
            {
                //error?
                return;
            }

            while (_messages.Count > 0)
            {
                var message = _messages[0];
                _messages.RemoveAt(0);

                if (message.messageNum == playerOnServer.lastMessageNum)
                {
                    _localPlayer.position = playerOnServer.position;
                    _localPlayer.rotation = playerOnServer.rotation;

                    for (int i = 0; i < _messages.Count; i++)
                    {
                        _localPlayer.AddControlMessage(_messages[i]);
                    }

                    _localPlayer.Process();
                    //Debug.Log("ok, applied messages: " + _messages.Count);
                    break;
                }
            }

            if (_messages.Count == 0 && _snapshots.Count > 0)
            {
                _localPlayer.position = playerOnServer.position;
                _localPlayer.rotation = playerOnServer.rotation;
                Debug.LogWarning("desync or no control messages");
            }
        }

        private void Client_DataReceived(object sender, DataReceivedFromServerEventArgs e)
        {
            lock (_readLock)
            {
                var fullMessages = _byteToMessageDecoder.Decode(e.Data);
                
                if (fullMessages != null)
                {
                    for (int i = 0; i < fullMessages.Count; i++)
                    {
                        ProcessMessage(MessageFactory.Create(fullMessages[i]));
                    }
                }
            }
        }
        
        private void ProcessMessage(IMessage message)
        {
            if (_objectId != 0)
            {
                switch (message.GetMessageId())
                {
                    case MessageIds.WorldSnapshot:
                    {
                        var snapshot = (WorldSnapshotMessage) message;
                        _snapshots.Enqueue(snapshot);
                        break;
                    }
                }
            }
            else
            {
                if (message.GetMessageId() == MessageIds.ConnectAccepted)
                {
                    var connectAccepted = (ConnectAcceptedMessage) message;
                    _objectId = connectAccepted.objectId;
                    _gameId = connectAccepted.gameId;
                    _localPlayer = new ClientPlayer();
                    _localPlayer.isPlayer = true;
                    _clientWorld = new ClientWorld(_localPlayer);
                    
                    Play();
                }
            }
        }

        private void Client_Disconnected(object sender, EventArgs e)
        {
            Stop();
        }

        private void Client_Connected(object sender, EventArgs e)
        {
        }
    }
}