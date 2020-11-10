using System;
using System.Collections.Generic;
using Basement.OEPFramework.UnityEngine._Base;
using Server.Entities._Base;
using Shared.Entities;
using Shared.Enums;
using UnityEngine;

namespace Server.Worlds
{
    public class ServerWorld : DroppableItemBase
    {
        private uint _globalObjectId;
        
        private uint _snapshotNum = 1;
        private uint _messageNum = 1;

        private readonly Dictionary<uint, ServerEntityBase> _entities = new Dictionary<uint, ServerEntityBase>(8192);
        private readonly Dictionary<uint, ServerEntityBase> _addEntities = new Dictionary<uint, ServerEntityBase>(1024);
        private readonly List<uint> _removeEntities = new List<uint>();
        
        private readonly List<WorldSnapshot> _preprocessSnapshots = new List<WorldSnapshot>();
        private readonly List<WorldSnapshot> _snapshots = new List<WorldSnapshot>();

        public float time { get; private set; }
        private DateTime _lastTickTime;
        
        public GameObject worldRoot { get; }
        public int gameId { get; }

        public ServerWorld(int gameId)
        {
            worldRoot = new GameObject("ServerWorld_" + gameId);
            
            //level
            var collider = new GameObject("collider");
            collider.AddComponent<BoxCollider>();

            AddGameObject(collider);
            collider.transform.localPosition = new Vector3(0, 0.5f, 0);
            collider.transform.localScale = new Vector3(3, 1, 3);
            
            worldRoot.SetActive(false);
            this.gameId = gameId;
        }

        public void AddGameObject(GameObject gameObject)
        {
            gameObject.transform.SetParent(worldRoot.transform);
        }
        
        public uint GetAndIncrementSnapshotNum()
        {
            return _snapshotNum++;
        }
        
        public uint GetAndIncrementMessageNum()
        {
            return _messageNum++;
        }
        
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
                    pair.Value.Create();
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
                    var removedEntity = _removeEntities[i];
                    _entities[removedEntity].Drop();
                    _entities.Remove(removedEntity);
                }

                _removeEntities.Clear();
            }            
        }
        
        public void Process()
        {
            var currentTime = DateTime.UtcNow;
            var deltaTime = (float)(currentTime - _lastTickTime).TotalSeconds;
            _lastTickTime = currentTime;
            
            worldRoot.SetActive(true);
            
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
            
            worldRoot.SetActive(false);

            time += deltaTime;
        }

        public uint GetNewObjectId()
        {
            return ++_globalObjectId;
        }

        private WorldSnapshot FindPreprocessedSnapshotByTime(float targetTime)
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

        public WorldSnapshot RewindWorld(float targetTime)
        {
            var snapshot = FindPreprocessedSnapshotByTime(targetTime);

            if (snapshot == null) return null;
            
            var accuratelySnapshot = new WorldSnapshot(snapshot.serverTime);
            
            //only for players
            foreach (var pair in snapshot.messages)
            {
                var objectId = pair.Key;
                var messagesList = pair.Value;

                if (snapshot.entities.TryGetValue(objectId, out var entity))
                {
                    var currentTime = accuratelySnapshot.serverTime;
                    var player = entity.Clone();
                    accuratelySnapshot.AddEntity(objectId, player);
                    
                    for (int i = 0; i < messagesList.Count; i++)
                    {
                        if (currentTime >= targetTime)
                        {
                            break;
                        }
                        
                        var controlMessage = messagesList[i];

                        var position = player.position;
                        var rotation = player.rotation;
                        SharedPlayerBehaviour.Movement(ref position, ref rotation, controlMessage);
                        player.position = position;
                        player.rotation = rotation;
                            
                        currentTime += controlMessage.deltaTime;
                    }
                }
            }

            return accuratelySnapshot;
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

        public override void Drop()
        {
            if (dropped) return;

            foreach (var entity in _entities.Values)
            {
                entity.Drop();
            }
            
            _entities.Clear();
            _addEntities.Clear();
            _removeEntities.Clear();
            
            _preprocessSnapshots.Clear();
            _snapshots.Clear();

            UnityEngine.Object.Destroy(worldRoot);
            
            base.Drop();
        }
    }
}