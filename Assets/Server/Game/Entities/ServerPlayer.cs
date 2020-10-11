using System.Collections.Generic;
using System.Numerics;
using Server.Game.Entities._Base;
using Shared;
using Shared.Game.Entities;
using Shared.Game.Entities._Base;
using Shared.Messages._Base;
using Shared.Utils;

namespace Server.Game.Entities
{
    public class ServerPlayer : SharedPlayerBase, IServerEntity
    {
        public bool remove { get; set; }
        public IServerWorld world { get; set; }
        
        private readonly Queue<IPlayerControlMessage> _controlMessages = new Queue<IPlayerControlMessage>(64);

        public ServerPlayer()
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
        
        public void Serialize(ref int offset, byte[] buffer)
        {
            SerializeUtil.SetInt((int)type, ref offset, buffer);
            SerializeUtil.SetUInt(objectId, ref offset, buffer);
            SerializeUtil.SetUInt(lastMessageNum, ref offset, buffer);
            SerializeUtil.SetVector3(position, ref offset, buffer);
            SerializeUtil.SetVector3(rotation, ref offset, buffer);
        }
    }
}