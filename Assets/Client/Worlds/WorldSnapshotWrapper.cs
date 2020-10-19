using System.Collections.Generic;
using Client.Entities._Base;
using LiteNetLib.Utils;
using Shared.Entities._Base;
using Shared.Enums;
using Shared.Factories;
using Shared.Messages.FromServer;

namespace Client.Worlds
{
    public class WorldSnapshotWrapper
    {
        public Dictionary<uint, ISharedEntity> snapshotEntities { get; } = new Dictionary<uint, ISharedEntity>(256);
        public float serverDeltaTime { get; }

        public WorldSnapshotWrapper(WorldSnapshotMessage message)
        {
            serverDeltaTime = message.deltaTime;
            var netDataReader = new NetDataReader(message.worldData);
            
            while (!netDataReader.EndOfData)
            {
                var entity = SharedEntityFactory.Create(netDataReader);
                snapshotEntities.Add(entity.objectId, entity);
            }
        }

        public ISharedEntity FindEntity(uint objectId)
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