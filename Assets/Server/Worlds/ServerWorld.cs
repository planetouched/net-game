using System;
using System.Collections.Generic;
using Server.Entities;
using Server.Entities._Base;
using Server.Worlds._Base;
using Shared.Enums;
using Shared.Loggers;
using Shared.Messages.FromClient;
using Shared.Utils;

namespace Server.Worlds
{
    public class ServerWorld : IServerWorld
    {
        private static uint _globalObjectId;

        private readonly Dictionary<uint, IServerEntity> _entities = new Dictionary<uint, IServerEntity>(8192);
        private readonly Dictionary<uint, IServerEntity> _addEntities = new Dictionary<uint, IServerEntity>(1024);
        private readonly List<uint> _removeEntities = new List<uint>();
        
        private readonly List<WorldSnapshot> _snapshots = new List<WorldSnapshot>();

        public float time { get; private set; }
        private DateTime _lastTickTime;

        public void SetupTime()
        {
            _lastTickTime = DateTime.UtcNow; 
        }
        
        public uint AddEntity(uint objectId, IServerEntity entity)
        {
            _addEntities.Add(objectId, entity);
            entity.world = this;
            entity.sharedEntity.objectId = objectId;
            return objectId;
        }

        public void RemoveEntity(uint objectId)
        {
            var entity = FindEntity(objectId);
            entity?.Remove();
        }

        public bool Exists(uint objectId)
        {
            return _entities.ContainsKey(objectId);
        }
        
        public IServerEntity FindEntity(uint objectId)
        {
            if (_entities.TryGetValue(objectId, out var entity))
            {
                if (entity.isRemoved)
                {
                    return null;
                }
                
                return entity;
            }
            
            if (_addEntities.TryGetValue(objectId, out var newEntity))
            {
                if (newEntity.isRemoved)
                {
                    return null;
                }
                
                return newEntity;
            }

            return null;
        }

        public T FindEntity<T>(uint objectId, GameEntityType type) where T : class
        {
            var entity = FindEntity(objectId);
            if (entity != null && entity.sharedEntity.type == type)
            {
                return (T) entity;
            }

            return null;
        }

        private void AddEntities()
        {
            if (_addEntities.Count > 0)
            {
                foreach (var pair in _addEntities)
                {
                    _entities.Add(pair.Key, pair.Value);
                }
                
                _addEntities.Clear();
            }
        }

        private void RemoveEntities()
        {
            if (_removeEntities.Count > 0)
            {
                for (int i = 0; i < _removeEntities.Count; i++)
                {
                    _entities.Remove(_removeEntities[i]);
                }

                _removeEntities.Clear();
            }            
        }
        
        public void Process()
        {
            var currentTime = DateTime.UtcNow;
            var deltaTime = (float)(currentTime - _lastTickTime).TotalSeconds;
            _lastTickTime = currentTime;
            
            AddEntities();
            
            foreach (var pair in _entities)
            {
                var entity = pair.Value;
                var objectId = pair.Key;
                
                if (!entity.isRemoved)
                {
                    entity.Process(deltaTime);
                }
                else
                {
                    _removeEntities.Add(objectId);
                }
            }

            RemoveEntities();

            time += deltaTime;
        }

        public uint GetNewObjectId()
        {
            return ++_globalObjectId;
        }

        private IWorldSnapshot FindSnapshotByTime(float targetTime)
        {
            for (int i = 0; i < _snapshots.Count; i++)
            {
                IWorldSnapshot next = null;

                if (i + 1 < _snapshots.Count)
                {
                    next = _snapshots[i + 1];
                }
                
                if (_snapshots[i].serverTime >= targetTime && (next == null || next.serverTime < targetTime))
                {
                    return _snapshots[i];
                }
            }

            return null;
        }
        
        public void Shot(IServerEntity shooter, ControlMessage message)
        {
            var shooterPlayer = (ServerPlayer) shooter;
            
            if (shooterPlayer.weapon.isInstant)
            {
                var snapshot = FindSnapshotByTime(message.serverTime);
                
                if (snapshot != null)
                {
                    foreach (var checkEntity in snapshot.entities)
                    {
                        //skip self
                        if (checkEntity.Key == shooterPlayer.sharedEntity.objectId) continue;

                        var hit = MathUtil.IntersectRaySphere(shooterPlayer.sharedEntity.position, shooterPlayer.sharedEntity.rotation, checkEntity.Value.position, 0.5f);
                        
                        if (hit)
                        {
                            Log.Write("Hit to: " + checkEntity.Key);
                        }
                    }
                }
            }
        }

        public IWorldSnapshot CreateSnapshot(float serverTime, bool keep)
        {
            var snapshot  = new WorldSnapshot(serverTime);
            
            foreach (var pair in _entities)
            {
                snapshot.AddEntity(pair.Key, pair.Value.sharedEntity.Clone());
            }

            if (keep)
            {
                _snapshots.Add(snapshot);
                
                //save only 2 last seconds
                if (_snapshots.Count > ServerSettings.TicksCount * 2 + 1)
                {
                    _snapshots.RemoveAt(0);
                }
            }

            return snapshot;
        }
    }
}