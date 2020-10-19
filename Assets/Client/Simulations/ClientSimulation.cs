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
        private readonly List<WorldSnapshotMessage> _worldSnapshotsPerTick = new List<WorldSnapshotMessage>();

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
            _started = false;
            Debug.Log("Client -> StopSimulation");
        }

        public void ProcessSimulation()
        {
            _clientNetListener.netManager.PollEvents();

            var localPlayer = _clientWorld.FindEntity<ClientLocalPlayer>(ClientLocalPlayer.localObjectId, GameEntityType.Player);

            if (localPlayer != null)
            {
                var controlMessage = new ControlMessage(++_messageNum, ClientLocalPlayer.localObjectId, _gameId);

                if (_started)
                {
                    FillControlMessage(controlMessage);
                }

                controlMessage.deltaTime = Time.deltaTime;

                _messagesHistory.Add(controlMessage);

                //prediction
                localPlayer.AddControlMessage(controlMessage);
                localPlayer.Process();

                if (_clientNetListener.IsConnected)
                {
                    _clientNetListener.netPeer.Send(controlMessage.Serialize(_netDataWriter), DeliveryMethod.Unreliable);
                }
                else
                {
                    StopSimulation();
                }
            }
            
            WorldSnapshotWrapper lastSnapshotView = null;
            
            for (int i = 0; i < _worldSnapshotsPerTick.Count; i++)
            {
                var snapshot = _worldSnapshotsPerTick[i];
                
                if (_lastProcessedSnapshotNum < snapshot.snapshotNum)
                {
                    lastSnapshotView = new WorldSnapshotWrapper(snapshot);
                    _clientWorld.AddWorldSnapshot(lastSnapshotView);
                    _lastProcessedSnapshotNum = snapshot.snapshotNum;
                }
            }

            _worldSnapshotsPerTick.Clear();
            
            //rewind player
            if (lastSnapshotView != null)
            {
                if (localPlayer != null)
                {
                    var serverPlayer = lastSnapshotView.FindEntity<ClientLocalPlayer>(localPlayer.objectId, GameEntityType.Player);
                    RewindPlayer(localPlayer, serverPlayer);
                }
            }

            _clientWorld.Process();
        }

        private void RewindPlayer(ClientLocalPlayer localPlayer, ClientLocalPlayer serverPlayer)
        {
            bool find = false;
            
            for (int j = _messagesHistory.Count - 1; j >= 0; j--)
            {
                if (_messagesHistory[j].messageNum == serverPlayer.lastMessageNum)
                {
                    _messagesHistory.RemoveRange(0, j + 1);
                    
                    localPlayer.position = serverPlayer.position;
                    localPlayer.rotation = serverPlayer.rotation;
                    
                    for (int i = 0; i < _messagesHistory.Count; i++)
                    {
                        localPlayer.AddControlMessage(_messagesHistory[i]);
                    }
                    
                    localPlayer.Process();

                    //Debug.Log("ok, applied messages: " + _messagesHistory.Count);
                    _started = find = true;
                    break;
                }
            }
            
            if (!find)
            {
                localPlayer.position = serverPlayer.position;
                localPlayer.rotation = serverPlayer.rotation;
                Debug.LogWarning("Client -> MessageNum not found");
            }
        }

        private void ClientNetListener_Disconnect()
        {
            StopSimulation();
        }

        private void ClientNetListener_Connect()
        {
            Debug.Log("Client -> MTU: " + _clientNetListener.netPeer.Mtu);
            _clientNetListener.netPeer.Send(new EnterGameMessage(++_messageNum).Serialize(_netDataWriter), DeliveryMethod.Unreliable);
        }

        private void ClientNetListener_IncomingMessage(IMessage message)
        {
            if (message.messageNum <= _lastMessageFromServerNum)
            {
                Debug.LogWarning("message.messageNum <= _lastMessageFromServerNum");
                return;
            }

            _lastMessageFromServerNum = message.messageNum;

            if (ClientLocalPlayer.localObjectId != 0)
            {
                switch (message.messageId)
                {
                    case MessageIds.WorldSnapshot:
                    {
                        var snapshot = (WorldSnapshotMessage) message;
                        _worldSnapshotsPerTick.Add(snapshot);
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