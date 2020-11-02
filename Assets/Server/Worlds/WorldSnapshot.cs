using System.Collections.Generic;
using LiteNetLib.Utils;
using Server.Worlds._Base;
using Shared.Entities._Base;

namespace Server.Worlds
{
    public class WorldSnapshot : IWorldSnapshot
    {
        public Dictionary<uint, ISharedEntity> entities { get; } = new Dictionary<uint, ISharedEntity>(128);

        public float serverTime { get; }
        
        public WorldSnapshot(float serverTime)
        {
            this.serverTime = serverTime;
        }
        
        public void AddEntity(uint objectId, ISharedEntity entity)
        {
            entities.Add(objectId, entity);
        }

        public NetDataWriter Serialize(NetDataWriter netDataWriter)
        {
            netDataWriter.Reset();

            foreach (var entity in entities.Values)
            {
                entity.Serialize(netDataWriter);
            }
            
            return netDataWriter;
        }
    }
}