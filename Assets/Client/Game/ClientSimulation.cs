using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using Basement.OEPFramework.UnityEngine._Base;
using Basement.OEPFramework.UnityEngine.Loop;
using Basement.OEPFramework.UnityEngine.Transit;
using Client.Game.Entities;
using Client.Game.Entities._Base;
using Client.Test;
using Server.Game.Entities;
using Shared.Game._Base;
using Shared.Game.Entities;
using Shared.Game.Entities._Base;
using Shared.Messages;
using Shared.ObjectPools;
using UnityEngine;

namespace Client.Game
{
    public class ClientSimulation : ISimulation
    {
        public bool dropped { get; private set; }
        public event Action<IDroppableItem> onDrop;

        private WorldView _worldView;
        
        private uint _messageNum;
        private readonly ObjectPoolNts<PlayerControlMessage> _messagesPool = new ObjectPoolNts<PlayerControlMessage>(() => new PlayerControlMessage());
        private readonly List<PlayerControlMessage> _messages = new List<PlayerControlMessage>(128);
        private readonly ConcurrentQueue<WorldSnapshot> _snapshots = new ConcurrentQueue<WorldSnapshot>();

        private ControlLoopTransit _loopTransit;
        private uint _lastSnapshotNum;
        private uint _objectHash;

        private ISharedPlayer _localClientPlayer;

        public void IncomingSnapshot(WorldSnapshot snapshot)
        {
            _snapshots.Enqueue(snapshot);
        }

        private void FillControlState(PlayerControlMessage message)
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
            message.mouseSensitivity = ClientSettings.MouseSensitivity;
        }

        public void Start()
        {
            _loopTransit = new ControlLoopTransit();
            _loopTransit.LoopOn(Loops.UPDATE, Process);

            Bridge.serverSimulation.tickComplete += IncomingSnapshot;
            
            //----------------------------------------------------------------
            //async connect to server world
            var serverWorld = Bridge.serverSimulation.world;
            var serverPlayer = new ServerPlayer {position = new System.Numerics.Vector3(0, 1, 0)};
            _objectHash = serverWorld.AddEntity(serverWorld.GetNewObjectId(), serverPlayer);
            
            //sync position and rotation
            _localClientPlayer = new ClientPlayer {position = serverPlayer.position};
            _worldView = new WorldView(_localClientPlayer);

            _loopTransit.Play();
            //----------------------------------------------------------------
        }

        public void Stop()
        {
            _loopTransit.Drop();
            _worldView.Drop();
        }

        public void Process()
        {
            var newMessage = _messagesPool.Take();
            newMessage.Clear();
            FillControlState(newMessage);

            _messageNum++;
            newMessage.objectId = _objectHash;
            newMessage.messageNum = _messageNum;
            newMessage.deltaTime = Time.deltaTime;

            _messages.Add(newMessage);

            //prediction
            _localClientPlayer.AddControlMessage(newMessage);
            _localClientPlayer.Process();

            //rewind
            while (_snapshots.TryDequeue(out var snapshot))
            {
                if (snapshot.snapshotSize == 0)
                {
                    continue;
                }
                
                try
                {
                    var world = new ClientWorld();
                    int offset = 0;
                    world.Deserialize(ref offset, snapshot.data, snapshot.snapshotSize);

                    if (_lastSnapshotNum < snapshot.snapshotNum)
                    {
                        Rewind(world);
                        _worldView.AddClientWorldSnapshot(world);
                        _lastSnapshotNum = snapshot.snapshotNum;
                    }

                }
                catch (ArgumentOutOfRangeException)
                {
                }
            }

            _worldView.Update();

            Bridge.serverSimulation.AddPlayerMessage(newMessage);        
        }

        private void Rewind(IClientWorld world)
        {
            var playerOnServer = world.FindEntity<ClientPlayer>(_objectHash, GameEntityType.Player);

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
                    _localClientPlayer.position = playerOnServer.position;
                    _localClientPlayer.rotation = playerOnServer.rotation;

                    for (int i = 0; i < _messages.Count; i++)
                    {
                        _localClientPlayer.AddControlMessage(_messages[i]);
                    }
                        
                    _localClientPlayer.Process();
                        
                    //Debug.Log("ok, applied messages: " + _messages.Count);
                    break;
                }
            }

            if (_messages.Count == 0 && _snapshots.Count > 0)
            {
                _localClientPlayer.position = playerOnServer.position;
                _localClientPlayer.rotation = playerOnServer.rotation;
                Debug.LogWarning("desync or no control messages");
            }
        }
    }
}