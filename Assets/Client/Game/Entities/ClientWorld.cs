using System.Collections.Generic;
using Client.Game.Entities._Base;
using Shared.Game.Entities;

namespace Client.Game.Entities
{
    public class ClientWorld : IClientWorld
    {
        private readonly Dictionary<uint, IClientEntity> _entities = new Dictionary<uint, IClientEntity>(1024);
        
        public void Deserialize(ref int offset, byte[] buffer, int bufferSize)
        {
            while (offset < bufferSize)
            {
                var entity = ClientEntityFactory.Create(ref offset, buffer);
                _entities.Add(entity.objectId, entity);
            }
        }

        public IClientEntity FindEntity(uint objectId)
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
    }
}