using System.Collections.Generic;
using Basement.OEPFramework.UnityEngine.Behaviour;
using Basement.OEPFramework.UnityEngine.Loop;
using Client.Entities;
using Client.Network;
using Client.Worlds;
using LiteNetLib;
using LiteNetLib.Utils;
using Shared.Entities;
using Shared.Enums;
using Shared.Loggers;
using Shared.Messages._Base;
using Shared.Messages.FromClient;
using Shared.Messages.FromServer;
using Shared.Simulations;
using UnityEngine;

namespace Client.Simulations
{
    public class ClientSimulation : LoopBehaviour, ISimulation
    {
        private readonly ClientWorld _clientWorld;

        private readonly List<ControlMessage> _messagesHistory = new List<ControlMessage>(128);
        private WorldSnapshotMessage _lastSnapshot;

        private uint _messageNum;
        private uint _lastMessageFromServerNum;
        private uint _lastProcessedSnapshotNum;
        private int _gameId;
        private bool _started;

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

            if (Input.GetMouseButton(0))
                message.mouseButton0 = true;
            
            if (Input.GetMouseButton(1))
                message.mouseButton1 = true;

            message.mouseX = Input.GetAxis("Mouse X");
            message.mouseY = -Input.GetAxis("Mouse Y");
            message.sensitivity = ClientSettings.MouseSensitivity;
            message.deltaTime = Time.deltaTime;
            message.serverTime = _clientWorld.currentServerTime;
        }

        public void StartSimulation()
        {
            if (_clientNetListener.IsConnected) return;
            _clientNetListener.Start();

            _clientWorld.onAddLocalPlayer += entity =>
            {
                ((ClientLocalPlayer)entity).SetCamera(Camera.main);
            }; 
            
            Log.Write("Client -> StartSimulation");
        }

        public void StopSimulation()
        {
            ClientLocalPlayer.localObjectId = 0;

            _clientWorld.Clear();
            _messagesHistory.Clear();
            _lastSnapshot = null;
            _started = false;
            Log.Write("Client -> StopSimulation");
        }

        public void ProcessSimulation()
        {
            _clientNetListener.PollEvents();

            var localPlayer = _clientWorld.FindEntity<ClientLocalPlayer>(ClientLocalPlayer.localObjectId, GameEntityType.Player);

            if (localPlayer != null)
            {
                var controlMessage = new ControlMessage();
                controlMessage.SetObjectId(ClientLocalPlayer.localObjectId).SetGameId(_gameId).SetMessageNum(++_messageNum);

                if (_started)
                {
                    FillControlMessage(controlMessage);
                }

                _messagesHistory.Add(controlMessage);

                //prediction
                localPlayer.AddControlMessage(controlMessage, true);
                localPlayer.Process();

                if (_clientNetListener.IsConnected)
                {
                    _netDataWriter.Reset();
                    _clientNetListener.netPeer.Send(controlMessage.Serialize(_netDataWriter), DeliveryMethod.Unreliable);
                }
                else
                {
                    StopSimulation();
                }
            }
            
            if (_lastSnapshot != null && _lastProcessedSnapshotNum < _lastSnapshot.snapshotNum)
            {
                var snapshotWrapper = new WorldSnapshotWrapper(_lastSnapshot);
                _clientWorld.AddWorldSnapshot(snapshotWrapper);
                _lastProcessedSnapshotNum = _lastSnapshot.snapshotNum;
                _lastSnapshot = null;
                
                //rewind player
                if (localPlayer != null)
                {
                    //_started = true;
                    var serverPlayer = snapshotWrapper.FindEntity<SharedPlayer>(localPlayer.objectId, GameEntityType.Player);
                    RewindPlayer(localPlayer, serverPlayer);
                }
            }

            _clientWorld.Process();
        }

        private void RewindPlayer(ClientLocalPlayer localPlayer, SharedPlayer serverPlayer)
        {
            for (int j = _messagesHistory.Count - 1; j >= 0; j--)
            {
                if (_messagesHistory[j].messageNum == serverPlayer.lastMessageNum)
                {
                    _messagesHistory.RemoveRange(0, j + 1);
                    localPlayer.SetPosition(serverPlayer.position, serverPlayer.rotation);
                    
                    for (int i = 0; i < _messagesHistory.Count; i++)
                    {
                        localPlayer.AddControlMessage(_messagesHistory[i], false);
                    }
                    
                    localPlayer.Process();

                    _started = true;
                    break;
                }
            }
        }

        private void ClientNetListener_Disconnect()
        {
            StopSimulation();
        }

        private void ClientNetListener_Connect()
        {
            Log.Write("Client -> MTU: " + _clientNetListener.netPeer.Mtu);
            _clientNetListener.netPeer.Send(new EnterGameMessage().Serialize(_netDataWriter), DeliveryMethod.ReliableUnordered);
        }

        private void ClientNetListener_IncomingMessage(MessageBase message)
        {
            if (ClientLocalPlayer.localObjectId > 0)
            {
                //in game
                if (message.messageNum <= _lastMessageFromServerNum)
                {
                    Log.WriteWarning("message.messageNum <= _lastMessageFromServerNum");
                    return;
                }

                _lastMessageFromServerNum = message.messageNum;
                
                switch (message.messageId)
                {
                    case MessageIds.WorldSnapshot:
                    {
                        var snapshot = (WorldSnapshotMessage) message;
                        _lastSnapshot = snapshot;
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