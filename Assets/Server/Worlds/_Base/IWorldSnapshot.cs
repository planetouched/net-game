using System.Collections.Generic;
using Shared.CommonInterfaces;
using Shared.Entities._Base;
using Shared.Messages.FromClient;

namespace Server.Worlds._Base
{
    public interface IWorldSnapshot : ISerializable
    {
        float serverTime { get; }
        Dictionary<uint, ISharedEntity> entities { get; }
        Dictionary<uint, List<ControlMessage>> messages { get; }
        
        void AddEntity(uint objectId, ISharedEntity entity);
        void AddControlMessage(uint playerId, ControlMessage message);
    }
}