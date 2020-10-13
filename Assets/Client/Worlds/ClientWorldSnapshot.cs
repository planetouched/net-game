using System.Collections.Generic;
using Client.Entities._Base;
using Client.Factories;
using Shared.Enums;
using Shared.Messages.FromServer;

namespace Client.Worlds
{
    public class ClientWorldSnapshot
    {
        private readonly Dictionary<uint, IClientEntity> _entities = new Dictionary<uint, IClientEntity>(1024);

        public ClientWorldSnapshot(WorldSnapshotMessage message)
        {
            int offset = 0;
            Deserialize(ref offset, message.data, message.snapshotSize);
        }
        
        private void Deserialize(ref int offset, byte[] buffer, int bufferSize)
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