using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Basement.OEPFramework.UnityEngine;
using Basement.OEPFramework.UnityEngine._Base;
using Basement.OEPFramework.UnityEngine.Loop;
using Basement.OEPFramework.UnityEngine.Transit;
using Client.Test;
using Shared.Game;
using Shared.Messages;
using Shared.ObjectPools;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Client.Game
{
    public class ClientSimulation : SimulationBase, IDroppableItem
    {
        private readonly int _mainThreadId;
        
        public bool dropped { get; }
        public event Action<IDroppableItem> onDrop;

        private readonly WorldView _worldView;
        private readonly World _world;

        private uint _messageNum;
        private readonly ObjectPoolNts<ControlMessage> _messagesPool = new ObjectPoolNts<ControlMessage>(() => new ControlMessage());
        private readonly List<ControlMessage> _messages = new List<ControlMessage>(128);
        private readonly Queue<WorldSnapshot> _snapshots = new Queue<WorldSnapshot>(128);

        private LoopTransit _loopTransit;

        private uint _lastSnapshotNum;

        public ClientSimulation(World world) : base(world)
        {
            _mainThreadId = Thread.CurrentThread.ManagedThreadId;
            
            _world = world;
            _worldView = new WorldView(world);
        }

        public void AddSnapshot(WorldSnapshot snapshot)
        {
            if (Thread.CurrentThread.ManagedThreadId == _mainThreadId)
            {
                _snapshots.Enqueue(snapshot);
            }
            else
            {
                Sync.Add(() => { _snapshots.Enqueue(snapshot); }, Loops.UPDATE);
            }
        }

        private void SetControlState(ControlMessage message)
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

        public override void Start()
        {
            _loopTransit = new LoopTransit();
            _loopTransit.LoopOn(Loops.UPDATE, Update);

            Bridge.serverSimulation.tickComplete += AddSnapshot;
        }

        public override void Update()
        {
            var newMessage = _messagesPool.Take();
            newMessage.Clear();
            SetControlState(newMessage);

            newMessage.messageNum = _messageNum++;
            newMessage.deltaTime = Time.deltaTime;

            _messages.Add(newMessage);

            //prediction
            _world.localPlayer.Calculate(newMessage);
            
            Rewind();

            _worldView.Update();

            Bridge.serverSimulation.AddMessage(newMessage);
        }

        private void Rewind()
        {
            while (_snapshots.Count > 0)
            {
                var snapshot = _snapshots.Dequeue();

                if (_lastSnapshotNum > snapshot.snapshotNum)
                {
                    continue;
                }

                _lastSnapshotNum = snapshot.snapshotNum;

                while (_messages.Count > 0)
                {
                    var message = _messages[0];
                    _messages.RemoveAt(0);
                    
                    if (message.messageNum == snapshot.lastMessageNum)
                    {
                        _world.localPlayer.position = snapshot.lastPosition;
                        _world.localPlayer.rotation = snapshot.lastRotation;

                        for (int i = 0; i < _messages.Count; i++)
                        {
                            _world.localPlayer.Calculate(_messages[i]);
                        }
                        
                        Debug.Log("OK");
                        
                        break;
                    }
                }

                if (_messages.Count == 0 && _snapshots.Count > 0)
                {
                    //desync
                    _world.localPlayer.position = snapshot.lastPosition;
                    _world.localPlayer.rotation = snapshot.lastRotation;
                    Debug.LogWarning("DESYNC");
                }
            }
        }

        public void Drop()
        {
            if (dropped) return;
            _loopTransit.Drop();
            _worldView.Drop();
            onDrop?.Invoke(this);
        }
    }
}