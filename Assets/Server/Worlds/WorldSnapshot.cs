using System.Collections.Generic;
using LiteNetLib.Utils;
using Shared.Entities._Base;
using Shared.Messages.FromClient;

namespace Server.Worlds
{
    public class WorldSnapshot
    {
        public Dictionary<uint, SharedEntityBase> entities { get; } = new Dictionary<uint, SharedEntityBase>(128);
        public Dictionary<uint, List<ControlMessage>> messages { get; } = new Dictionary<uint, List<ControlMessage>>();

        public float serverTime { get; }
        
        public WorldSnapshot(float serverTime)
        {
            this.serverTime = serverTime;
        }
        
        public void AddEntity(uint objectId, SharedEntityBase entity)
        {
            entities.Add(objectId, entity);
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