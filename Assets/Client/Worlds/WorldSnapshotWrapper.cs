using System.Collections.Generic;
using Client.Entities._Base;
using Client.Factories;
using LiteNetLib.Utils;
using Shared.Enums;
using Shared.Messages.FromServer;

namespace Client.Worlds
{
    public class WorldSnapshotWrapper
    {
        public Dictionary<uint, IClientEntity> snapshotEntities { get; } = new Dictionary<uint, IClientEntity>(256);

        public WorldSnapshotWrapper(WorldSnapshotMessage message)
        {
            var netDataReader = new NetDataReader(message.worldData);
            
            while (!netDataReader.EndOfData)
            {
                var entity = ClientEntityFactory.Create(netDataReader);
                snapshotEntities.Add(entity.objectId, entity);
            }
        }

        public IClientEntity FindEntity(uint objectId)
        {
            if (snapshotEntities.TryGetValue(objectId, out var entity))
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