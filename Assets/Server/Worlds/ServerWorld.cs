using System.Collections.Generic;
using LiteNetLib.Utils;
using Server.Entities;
using Server.Entities._Base;
using Server.Worlds._Base;
using Shared.Enums;
using Shared.Messages.FromClient;

namespace Server.Worlds
{
    public class ServerWorld : IServerWorld
    {
        private static uint _globalObjectId;

        private readonly Dictionary<uint, IServerEntity> _entities = new Dictionary<uint, IServerEntity>(8192);
        private readonly Dictionary<uint, IServerEntity> _addEntities = new Dictionary<uint, IServerEntity>(1024);
        private readonly List<uint> _removeEntities = new List<uint>();

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
        
        public void Process(float deltaTime)
        {
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
        }

        public uint GetNewObjectId()
        {
            return ++_globalObjectId;
        }

        public void Shot(IServerEntity entity, ControlMessage message)
        {
            var player = (ServerPlayer) entity;
            
            if (player.weapon.isInstant)
            {
                //rewind world
            }
        }
        
        public NetDataWriter Serialize(NetDataWriter netDataWriter)
        {
            netDataWriter.Reset();

            foreach (var entity in _entities.Values)
            {
                entity.sharedEntity.Serialize(netDataWriter);
            }
            
            return netDataWriter;
        }
    }
}