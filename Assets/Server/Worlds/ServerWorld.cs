using System;
using System.Collections.Generic;
using Server.Entities;
using Server.Entities._Base;
using Shared.Entities;
using Shared.Enums;
using Shared.Loggers;
using Shared.Utils;

namespace Server.Worlds
{
    public class ServerWorld
    {
        private static uint _globalObjectId;

        private readonly Dictionary<uint, ServerEntityBase> _entities = new Dictionary<uint, ServerEntityBase>(8192);
        private readonly Dictionary<uint, ServerEntityBase> _addEntities = new Dictionary<uint, ServerEntityBase>(1024);
        private readonly List<uint> _removeEntities = new List<uint>();
        
        private readonly List<WorldSnapshot> _preprocessSnapshots = new List<WorldSnapshot>();
        private readonly List<WorldSnapshot> _snapshots = new List<WorldSnapshot>();

        public float time { get; private set; }
        private DateTime _lastTickTime;

        public void SetupTime()
        {
            _lastTickTime = DateTime.UtcNow; 
        }
        
        public uint AddEntity(uint objectId, ServerEntityBase entity)
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
        
        public ServerEntityBase FindEntity(uint objectId)
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

        public T FindEntity<T>(uint objectId, GameEntityType type) where T : ServerEntityBase
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

        private WorldSnapshot FindPreprocessSnapshotByTime(float targetTime)
        {
            for (int i = _preprocessSnapshots.Count - 1; i >= 0; i--)
            {
                if (_preprocessSnapshots[i].serverTime <= targetTime)
                {
                    return _preprocessSnapshots[i];
                }
            }

            return null;
        }

        private WorldSnapshot RewindWorld(float targetTime)
        {
            var snapshot = FindPreprocessSnapshotByTime(targetTime);

            if (snapshot == null) return null;
            
            var accuratelySnapshot = new WorldSnapshot(snapshot.serverTime);
            
            foreach (var pair in snapshot.entities)
            {
                var objectId = pair.Key;
                var entity = pair.Value;

                if (snapshot.messages.TryGetValue(objectId, out var messages))
                {
                    var currentTime = accuratelySnapshot.serverTime;
                    var player = entity.Clone();
                    accuratelySnapshot.AddEntity(objectId, player);
                    
                    for (int i = 0; i < messages.Count; i++)
                    {
                        if (currentTime >= targetTime)
                        {
                            break;
                        }
                        
                        var controlMessage = messages[i];

                        var position = player.position;
                        var rotation = player.rotation;
                        SharedPlayerBehaviour.Movement(ref position, ref rotation, controlMessage);
                        player.position = position;
                        player.rotation = rotation;
                            
                        currentTime += controlMessage.deltaTime;
                    }
                }
                else
                {
                    accuratelySnapshot.AddEntity(objectId, entity);
                }
            }

            return accuratelySnapshot;
        }
        
        public uint Shot(ServerEntityBase shooter, float shooterTime)
        {
            var shooterPlayer = (ServerPlayer) shooter;
            
            if (shooterPlayer.weapon.isInstant)
            {
                var snapshot = RewindWorld(shooterTime);
                
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
                            return checkEntity.Key;
                        }
                    }
                }
            }

            return 0;
        }

        public WorldSnapshot CreateSnapshot(float serverTime, bool preprocessSnapshot)
        {
            var snapshot  = new WorldSnapshot(serverTime);
            
            foreach (var pair in _entities)
            {
                snapshot.AddEntity(pair.Key, pair.Value.sharedEntity.Clone());
            }

            if (preprocessSnapshot)
            {
                _preprocessSnapshots.Add(snapshot);
                
                //save only 3 last seconds
                if (_preprocessSnapshots.Count > ServerSettings.TicksCount * 3 + 1)
                {
                    _preprocessSnapshots.RemoveAt(0);
                }
            }
            else
            {
                _snapshots.Add(snapshot);
                
                //save only 3 last seconds
                if (_snapshots.Count > ServerSettings.TicksCount * 3 + 1)
                {
                    _snapshots.RemoveAt(0);
                }
            }

            return snapshot;
        }
    }
}