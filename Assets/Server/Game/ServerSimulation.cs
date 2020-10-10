using System;
using System.Collections.Concurrent;
using System.Threading;
using Shared.Game;
using Shared.Messages;

namespace Server.Game
{
    public class ServerSimulation : SimulationBase, IDisposable
    {
        private uint _snapshotNum;
        
        private Thread _tickThread;
        private volatile bool _update;
        private bool _disposed;

        private readonly World _world;
        
        private readonly ConcurrentQueue<ControlMessage> _messages = new ConcurrentQueue<ControlMessage>();

        public event Action<WorldSnapshot> tickComplete;
        
        public ServerSimulation(World world) : base(world)
        {
            _world = world;
        }

        public void AddMessage(ControlMessage message)
        {
            _messages.Enqueue(message);
        }

        public override void Start()
        {
            _tickThread = new Thread(Thread_Tick) {IsBackground = true};
            _tickThread.Start();
        }

        public override void Update()
        {
            uint lastMessageNum = 0;
            bool updated = false;
            
            while (_messages.TryDequeue(out var message))
            {
                _world.localPlayer.Calculate(message);
                lastMessageNum = message.messageNum;
                updated = true;
            }

            if (updated)
            {
                _snapshotNum++;
                
                var snapshot = new WorldSnapshot();
                snapshot.snapshotNum = _snapshotNum;
                snapshot.lastMessageNum = lastMessageNum;
                snapshot.lastPosition = _world.localPlayer.position;
                snapshot.lastRotation = _world.localPlayer.rotation;
                tickComplete?.Invoke(snapshot);
            }
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            _update = false;
            _tickThread.Join();
            
            tickComplete = null;
        }

        private void Thread_Tick()
        {
            _update = true;
            int delay = (int)(1 / (float)ServerSettings.TicksCount) * 1000;

            while (_update)
            {
                Update();
                Thread.Sleep(delay);
            }
        }
        
        ~ServerSimulation()
        {
            Dispose();
        }
    }
}