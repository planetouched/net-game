using System.Collections.Generic;
using LiteNetLib.Utils;
using Server.Worlds._Base;
using Shared.Entities._Base;
using Shared.Messages.FromClient;

namespace Server.Worlds
{
    public class WorldSnapshot : IWorldSnapshot
    {
        public Dictionary<uint, ISharedEntity> entities { get; } = new Dictionary<uint, ISharedEntity>(128);
        public Dictionary<uint, List<ControlMessage>> messages { get; } = new Dictionary<uint, List<ControlMessage>>();

        public float serverTime { get; }
        
        public WorldSnapshot(float serverTime)
        {
            this.serverTime = serverTime;
        }
        
        public void AddControlMessage(uint playerId, ControlMessage message)
        {
            if (messages.TryGetValue(playerId, out var list))
            {
                list.Add(message);
            }
            else
            {
                messages.Add(playerId, new List<ControlMessage> {message});
            }
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