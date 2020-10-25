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
        private readonly List<ControlMessage> _sendToServer = new List<ControlMessage>(128);
        private readonly List<WorldSnapshotMessage> _worldSnapshotsPerTick = new List<WorldSnapshotMessage>();

        private uint _messageNum;
        private uint _lastMessageFromServerNum;
        private uint _lastProcessedSnapshotNum;
        private int _gameId;
        private bool _started;

        private readonly NetDataWriter _netDataWriter;
        private readonly ClientNetListener _clientNetListener;
        private float _timeToSendElapsed;

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
            message.serverTime = _clientWorld.serverTime;
        }

        public void StartSimulation()
        {
            if (_clientNetListener.IsConnected) return;
            _clientNetListener.Start();
            Log.Write("Client -> StartSimulation");
        }

        public void StopSimulation()
        {
            ClientLocalPlayer.localObjectId = 0;

            _clientWorld.Clear();
            _messagesHistory.Clear();
            _worldSnapshotsPerTick.Clear();
            _started = false;
            Log.Write("Client -> StopSimulation");
        }

        public void ProcessSimulation()
        {
            _clientNetListener.netManager.PollEvents();

            var localPlayer = _clientWorld.FindEntity<ClientLocalPlayer>(ClientLocalPlayer.localObjectId, GameEntityType.Player);

            if (localPlayer != null)
            {
                localPlayer.SetCamera(Camera.main);
                var controlMessage = new ControlMessage();
                controlMessage.SetObjectId(ClientLocalPlayer.localObjectId).SetGameId(_gameId);

                if (_started)
                {
                    FillControlMessage(controlMessage);
                }

                _messagesHistory.Add(controlMessage);
                _sendToServer.Add(controlMessage);

                //prediction
                localPlayer.AddControlMessage(controlMessage);
                localPlayer.Process();

                if (_clientNetListener.IsConnected)
                {
                    _timeToSendElapsed += Time.deltaTime;
                    
                    if (_timeToSendElapsed >= Time.fixedDeltaTime)
                    {
                        _timeToSendElapsed -= Time.fixedDeltaTime;

                        _netDataWriter.Reset();
                    
                        for (int i = 0; i < _sendToServer.Count; i++)
                        {
                            _sendToServer[i].SetMessageNum(++_messageNum);
                            _sendToServer[i].Serialize(_netDataWriter);
                        }
                    
                        _clientNetListener.netPeer.Send(_netDataWriter, DeliveryMethod.Unreliable);
                        _sendToServer.Clear();
                    }
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
                    var serverPlayer = lastSnapshotView.FindEntity<SharedPlayer>(localPlayer.objectId, GameEntityType.Player);
                    RewindPlayer(localPlayer, serverPlayer);
                }
            }

            _clientWorld.Process();
        }

        private void RewindPlayer(ClientLocalPlayer localPlayer, SharedPlayer serverPlayer)
        {
            bool find = false;
            
            for (int j = _messagesHistory.Count - 1; j >= 0; j--)
            {
                if (_messagesHistory[j].messageNum == serverPlayer.lastMessageNum)
                {
                    _messagesHistory.RemoveRange(0, j + 1);
                    localPlayer.SetPosition(serverPlayer.position, serverPlayer.rotation);
                    
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
                localPlayer.SetPosition(serverPlayer.position, serverPlayer.rotation);
                Log.WriteWarning("Client -> MessageNum not found");
            }
        }

        private void ClientNetListener_Disconnect()
        {
            StopSimulation();
        }

        private void ClientNetListener_Connect()
        {
            Log.Write("Client -> MTU: " + _clientNetListener.netPeer.Mtu);
            _clientNetListener.netPeer.Send(new EnterGameMessage().SetMessageNum(++_messageNum).Serialize(_netDataWriter), DeliveryMethod.Unreliable);
        }

        private void ClientNetListener_IncomingMessage(IMessage message)
        {
            if (message.messageNum <= _lastMessageFromServerNum)
            {
                Log.WriteWarning("message.messageNum <= _lastMessageFromServerNum");
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