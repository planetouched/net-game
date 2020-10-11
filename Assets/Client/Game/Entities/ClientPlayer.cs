using System.Collections.Generic;
using System.Numerics;
using Client.Game.Entities._Base;
using Shared;
using Shared.Game.Entities;
using Shared.Game.Entities._Base;
using Shared.Messages._Base;
using Shared.Utils;

namespace Client.Game.Entities
{
    public class ClientPlayer : SharedPlayerBase, IClientEntity
    {
        private readonly Queue<IPlayerControlMessage> _controlMessages = new Queue<IPlayerControlMessage>(64);
        
        public ClientPlayer()
        {
            type = GameEntityType.Player;
        }

        public override void AddControlMessage(IPlayerControlMessage message)
        {
            _controlMessages.Enqueue(message);
            lastMessageNum = message.messageNum;
        }
        
        public override void Process()
        {
            while (_controlMessages.Count > 0)
            {
                var message = _controlMessages.Dequeue();
                Movement(message);
            }
        }

        public void Deserialize(ref int offset, byte[] buffer)
        {
            type = (GameEntityType)SerializeUtil.GetInt(ref offset, buffer);
            objectId = SerializeUtil.GetUInt(ref offset, buffer);
            lastMessageNum = SerializeUtil.GetUInt(ref offset, buffer);
            position = SerializeUtil.GetVector3(ref offset, buffer);
            rotation = SerializeUtil.GetVector3(ref offset, buffer);
        }
    }
}