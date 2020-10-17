using System.Collections.Generic;
using Basement.OEPFramework.UnityEngine.Behaviour;
using Basement.OEPFramework.UnityEngine.Loop;
using Client.Entities;
using Client.Network;
using Client.Worlds;
using LiteNetLib;
using LiteNetLib.Utils;
using Shared.Enums;
using Shared.Messages._Base;
using Shared.Messages.FromClient;
using Shared.Messages.FromServer;
using Shared.Simulations;
using UnityEngine;

namespace Client.Simulations
{
    public class ClientSimulation : LoopBehaviour, ISimulation
    {
        //simulation
        private readonly ClientWorld _clientWorld;

        private readonly List<ControlMessage> _messagesHistory = new List<ControlMessage>(128);
        private readonly Queue<WorldSnapshotMessage> _worldSnapshotsPerTick = new Queue<WorldSnapshotMessage>();

        private uint _messageNum;
        private uint _lastMessageFromServerNum;
        private uint _lastProcessedSnapshotNum;
        private int _gameId;

        private readonly NetDataWriter _netDataWriter;
        private readonly ClientNetListener _clientNetListener;
        
        public ClientSimulation(string serverIp, int port)
        {
            _netDataWriter = new NetDataWriter(false, ushort.MaxValue);
            _clientNetListener = new ClientNetListener(serverIp, port);
            _clientNetListener.onIncomingMessage += ClientNetListener_IncomingMessage;
            _clientNetListener.onConnect += ClientNetListener_Connect;
            _clientNetListener.onDisconnect += ClientNetListener_Disconnect;

            _clientWorld = new ClientWorld();
            
            LoopOn(Loops.UPDATE, ProcessSimulation);
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

        public void StartSimulation()
        {
            if (_clientNetListener.IsConnected) return;
            _clientNetListener.Start();
            Debug.Log("Client -> StartSimulation");
        }

        public void StopSimulation()
        {
            if (!_clientNetListener.IsConnected) return;
            _clientNetListener.Stop();
            
            ClientLocalPlayer.localObjectId = 0;

            _clientWorld.Clear();
            _messagesHistory.Clear();
            _worldSnapshotsPerTick.Clear();
            Debug.Log("Client -> StopSimulation");
        }

        public void ProcessSimulation()
        {
            _clientNetListener.netManager.PollEvents();

            if (!_clientNetListener.IsConnected)
            {
                return;
            }
            
            var localPlayer = _clientWorld.FindEntity<ClientLocalPlayer>(ClientLocalPlayer.localObjectId, GameEntityType.Player);

            if (localPlayer != null)
            {
                var controlMessage = new ControlMessage(++_messageNum, ClientLocalPlayer.localObjectId, _gameId);
                FillControlMessage(controlMessage);

                controlMessage.deltaTime = Time.deltaTime;

                _messagesHistory.Add(controlMessage);

                //prediction
                localPlayer.AddControlMessage(controlMessage);
                localPlayer.Process();

                if (_clientNetListener.IsConnected)
                {
                    _clientNetListener.clientPeer.Send(controlMessage.Serialize(_netDataWriter), DeliveryMethod.Unreliable);
                }
                else
                {
                    StopSimulation();
                }
            }

            //rewind
            while (_worldSnapshotsPerTick.Count > 0)
            {
                var snapshot = _worldSnapshotsPerTick.Dequeue();
                
                var snapshotView = new WorldSnapshotWrapper(snapshot);

                if (_lastProcessedSnapshotNum < snapshot.snapshotNum)
                {
                    if (localPlayer != null)
                    {
                        RewindPlayer(localPlayer, snapshotView);
                    }

                    _clientWorld.AddWorldSnapshot(snapshotView);
                    _lastProcessedSnapshotNum = snapshot.snapshotNum;
                }
            }

            _clientWorld.Process();
        }

        private void RewindPlayer(ClientLocalPlayer localPlayer, WorldSnapshotWrapper worldSnapshotWrapper)
        {
            var playerOnServer = worldSnapshotWrapper.FindEntity<ClientLocalPlayer>(localPlayer.objectId, GameEntityType.Player);

            while (_messagesHistory.Count > 0)
            {
                var message = _messagesHistory[0];
                _messagesHistory.RemoveAt(0);

                if (message.messageNum == playerOnServer.lastMessageNum)
                {
                    localPlayer.position = playerOnServer.position;
                    localPlayer.rotation = playerOnServer.rotation;

                    for (int i = 0; i < _messagesHistory.Count; i++)
                    {
                        localPlayer.AddControlMessage(_messagesHistory[i]);
                    }

                    localPlayer.Process();
                    Debug.Log("ok, applied messages: " + _messagesHistory.Count);
                    break;
                }
            }

            if (_messagesHistory.Count == 0 && _worldSnapshotsPerTick.Count > 0)
            {
                localPlayer.position = playerOnServer.position;
                localPlayer.rotation = playerOnServer.rotation;
                Debug.LogWarning("Client -> Something went wrong or no control messages");
            }
        }

        private void ClientNetListener_Disconnect()
        {
            StopSimulation();
        }

        private void ClientNetListener_Connect()
        {
            Debug.Log("Client -> MTU: " + _clientNetListener.clientPeer.Mtu);
            _clientNetListener.clientPeer.Send(new EnterGameMessage(++_messageNum).Serialize(_netDataWriter), DeliveryMethod.Unreliable);
        }
        
        private void ClientNetListener_IncomingMessage(IMessage message)
        {
            if (message.messageNum <= _lastMessageFromServerNum) return;
            
            _lastMessageFromServerNum = message.messageNum; 
            
            if (ClientLocalPlayer.localObjectId != 0)
            {
                switch (message.messageId)
                {
                    case MessageIds.WorldSnapshot:
                    {
                        var snapshot = (WorldSnapshotMessage) message;
                        _worldSnapshotsPerTick.Enqueue(snapshot);
                        break;
                    }
                }
            }
            else
            {
                if (message.messageId == MessageIds.ConnectAccepted)
                {
                    var connectAccepted = (EnterGameAcceptedMessage) message;
                    ClientLocalPlayer.localObjectId = connectAccepted.objectId;
                    _gameId = connectAccepted.gameId;
                }
            }
        }
        
        public override void Drop()
        {
            if (dropped) return;
            
            _clientNetListener.onIncomingMessage -= ClientNetListener_IncomingMessage;
            _clientNetListener.onConnect -= ClientNetListener_Connect;
            _clientNetListener.onDisconnect -= ClientNetListener_Disconnect;
            
            StopSimulation();
            _clientNetListener.netManager.Stop();
            base.Drop();
        }
    }
}