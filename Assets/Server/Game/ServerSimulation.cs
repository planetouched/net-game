using System;
using System.Collections.Concurrent;
using System.Threading;
using Server.Game.Entities;
using Server.Game.Entities._Base;
using Shared.Game._Base;
using Shared.Game.Entities;
using Shared.Messages;
using Shared.Messages._Base;

namespace Server.Game
{
    public class ServerSimulation : ISimulation
    {
        private uint _snapshotNum;

        private Thread _tickThread;
        private volatile bool _update;
        private bool _disposed;
        public IServerWorld world { get; }

        private readonly ConcurrentQueue<IPlayerControlMessage> _messages = new ConcurrentQueue<IPlayerControlMessage>();

        public event Action<WorldSnapshot> tickComplete;
        private byte[] _buffer = new byte[1024 * 1024];

        public ServerSimulation(IServerWorld world)
        {
            this.world = world;
        }

        public void AddPlayerMessage(IPlayerControlMessage message)
        {
            _messages.Enqueue(message);
        }

        public void Start()
        {
            _tickThread = new Thread(Thread_Tick) {IsBackground = true};
            _tickThread.Start();
        }

        public void Process()
        {
            while (_messages.TryDequeue(out var message))
            {
                var player = world.FindEntity<ServerPlayer>(message.objectId, GameEntityType.Player);

                if (player != null)
                {
                    if (player.lastMessageNum < message.messageNum)
                    {
                        player.AddControlMessage(message);
                    }
                }
            }

            world.Process();

            _snapshotNum++;
            
            int offset = 0;
            bool tryToSerialize = true;
            
            while (tryToSerialize)
            {
                tryToSerialize = false;

                try
                {
                    world.Serialize(ref offset, _buffer);
                }
                catch (ArgumentOutOfRangeException)
                {
                    if (_buffer.Length < 1024 * 1024 * 8)
                    {
                        offset = 0;
                        tryToSerialize = true;
                        _buffer = new byte[_buffer.Length * 2];
                    }
                    else
                    {
                        throw new Exception("error... stop simulation");
                    }
                }
            }

            var snapshot = new WorldSnapshot
            {
                snapshotSize = offset,
                snapshotNum = _snapshotNum, 
                data = _buffer
            };

            tickComplete?.Invoke(snapshot);
        }

        public void Stop()
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
            int delay = (int) (1 / (float) ServerSettings.TicksCount * 1000);

            while (_update)
            {
                Process();
                Thread.Sleep(delay);
            }
        }

        ~ServerSimulation()
        {
            Stop();
        }
    }
}