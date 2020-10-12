using System.Collections.Generic;
using Server.Game.Entities._Base;
using Server.Game.Worlds._Base;
using Shared.Enums;

namespace Server.Game.Worlds
{
    public class ServerWorld : IServerWorld
    {
        private static uint _globalObjectId;

        private readonly Dictionary<uint, IServerEntity> _entities = new Dictionary<uint, IServerEntity>(ushort.MaxValue + 1);
        private readonly List<(uint objectId, IServerEntity entity)> _addEntities = new List<(uint, IServerEntity)>();
        private readonly List<uint> _removeEntities = new List<uint>();

        public uint AddEntity(uint objectId, IServerEntity entity)
        {
            _addEntities.Add((objectId, entity));
            entity.world = this;
            entity.objectId = objectId;
            return objectId;
        }

        public void RemoveEntity(uint objectId)
        {
            var entity = FindEntity(objectId);
            if (entity != null)
            {
                _removeEntities.Add(objectId);
                entity.remove = true;
            }
        }

        public IServerEntity FindEntity(uint objectId)
        {
            if (_entities.TryGetValue(objectId, out var entity))
            {
                return entity;
            }

            return null;
        }

        public T FindEntity<T>(uint objectId, GameEntityType type) where T : class
        {
            var entity = FindEntity(objectId);
            if (entity != null && entity.type == type)
            {
                return (T) entity;
            }

            return null;
        }

        public void Process()
        {
            if (_addEntities.Count > 0)
            {
                for (int i = 0; i < _addEntities.Count; i++)
                {
                    var pair = _addEntities[i];
                    _entities.Add(pair.objectId, pair.entity);
                }

                _addEntities.Clear();
            }

            foreach (var entity in _entities)
            {
                if (!entity.Value.remove)
                {
                    entity.Value.Process();
                }
            }

            if (_removeEntities.Count > 0)
            {
                for (int i = 0; i < _removeEntities.Count; i++)
                {
                    _entities.Remove(_removeEntities[i]);
                }

                _removeEntities.Clear();
            }
        }

        public uint GetNewObjectId()
        {
            return ++_globalObjectId;
        }

        public void Serialize(ref int offset, byte[] buffer)
        {
            foreach (var pair in _entities)
            {
                pair.Value.Serialize(ref offset, buffer);
            }
        }
    }
}