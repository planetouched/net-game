using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Basement.OEPFramework.UnityEngine;
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

        private readonly List<ControlMessage> _messages = new List<ControlMessage>(128);
        private readonly ConcurrentQueue<WorldSnapshotMessage> _snapshots = new ConcurrentQueue<WorldSnapshotMessage>();

        private uint _messageNum;
        private uint _lastSnapshotNum;
        private uint _gameId;

        //network
        private volatile SimpleTcpClient _tcpClient;
        private readonly ByteToMessageDecoder _byteToMessageDecoder;
        private readonly string _serverIp;
        private readonly int _port;

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
            if (_tcpClient != null) return;

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
            }
        }

        public void Stop()
        {
            if (_tcpClient == null) return;

            _tcpClient.Events.Connected -= Client_Connected;
            _tcpClient.Events.Disconnected -= Client_Disconnected;
            _tcpClient.Events.DataReceived -= Client_DataReceived;
            _tcpClient.Dispose();
            
            ClientLocalPlayer.localObjectId = 0;

            Sync.Add(() =>
            {
                Pause();
                _clientWorld?.Drop();
                _messages.Clear();
                while (_snapshots.TryDequeue(out _)) {}
                _tcpClient = null;
                Debug.Log("Client -> Stop");

            }, Loops.UPDATE);
        }

        public void Process()
        {
            var localPlayer = _clientWorld.FindEntity<ClientLocalPlayer>(ClientLocalPlayer.localObjectId, GameEntityType.Player);

            if (localPlayer != null)
            {
                var controlMessage = MessageFactory.Create<ControlMessage>(MessageIds.PlayerControl);
                FillControlMessage(controlMessage);

                _messageNum++;
                controlMessage.gameId = _gameId;
                controlMessage.objectId = ClientLocalPlayer.localObjectId;
                controlMessage.messageNum = _messageNum;
                controlMessage.deltaTime = Time.deltaTime;

                _messages.Add(controlMessage);

                //prediction
                localPlayer.AddControlMessage(controlMessage);
                localPlayer.Process();

                _tcpClient.Send(MessageBase.ConvertToBytes(controlMessage));
            }

            //rewind
            while (_snapshots.TryDequeue(out var snapshot))
            {
                if (snapshot.snapshotSize == 0) continue;

                var snapshotView = new ClientWorldSnapshot(snapshot);

                if (_lastSnapshotNum < snapshot.snapshotNum)
                {
                    if (localPlayer != null)
                    {
                        Rewind(localPlayer, snapshotView);
                    }

                    _clientWorld.AddWorldSnapshot(snapshotView);
                    _lastSnapshotNum = snapshot.snapshotNum;
                }
            }

            _clientWorld.Process();
        }

        private void Rewind(ClientLocalPlayer localPlayer, ClientWorldSnapshot clientWorldSnapshot)
        {
            var playerOnServer = clientWorldSnapshot.FindEntity<ClientLocalPlayer>(localPlayer.objectId, GameEntityType.Player);

            while (_messages.Count > 0)
            {
                var message = _messages[0];
                _messages.RemoveAt(0);

                if (message.messageNum == playerOnServer.lastMessageNum)
                {
                    localPlayer.position = playerOnServer.position;
                    localPlayer.rotation = playerOnServer.rotation;

                    for (int i = 0; i < _messages.Count; i++)
                    {
                        localPlayer.AddControlMessage(_messages[i]);
                    }

                    localPlayer.Process();
                    //Debug.Log("ok, applied messages: " + _messages.Count);
                    break;
                }
            }

            if (_messages.Count == 0 && _snapshots.Count > 0)
            {
                localPlayer.position = playerOnServer.position;
                localPlayer.rotation = playerOnServer.rotation;
                Debug.LogWarning("Client -> Something went wrong or no control messages");
            }
        }

        private void Client_DataReceived(object sender, DataReceivedFromServerEventArgs e)
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

        private void ProcessMessage(IMessage message)
        {
            if (ClientLocalPlayer.localObjectId != 0)
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
                    ClientLocalPlayer.localObjectId = connectAccepted.objectId;
                    _gameId = connectAccepted.gameId;
                    _clientWorld = new ClientWorld();
                    Sync.Add(Play, Loops.UPDATE);
                }
            }
        }

        private void Client_Disconnected(object sender, EventArgs e)
        {
            Debug.Log("Client -> Disconnect");
            Stop();
        }

        private void Client_Connected(object sender, EventArgs e)
        {
        }
        
        public override void Drop()
        {
            if (dropped) return;
            Stop();
            base.Drop();
        }
    }
}