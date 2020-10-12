using System.Collections.Generic;
using System.Numerics;
using Client.Game.Entities._Base;
using Shared;
using Shared.Enums;
using Shared.Game.Entities;
using Shared.Game.Entities._Base;
using Shared.Messages;
using Shared.Messages._Base;
using Shared.Messages.FromClient;
using Shared.Utils;

namespace Client.Game.Entities
{
    public class ClientPlayer : SharedPlayerBase, IClientEntity
    {
        private readonly Queue<ControlMessage> _controlMessages = new Queue<ControlMessage>(64);
        
        public ClientPlayer()
        {
            type = GameEntityType.Player;
        }

        public override void AddControlMessage(ControlMessage message)
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