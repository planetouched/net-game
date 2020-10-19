using System.Collections.Generic;
using LiteNetLib.Utils;
using Server.Entities._Base;
using Server.Worlds._Base;
using Shared.Entities;
using Shared.Enums;
using Shared.Messages.FromClient;

namespace Server.Entities
{
    public class ServerPlayer : SharedPlayerBase, IServerEntity
    {
        public bool isRemoved { get; private set; }
        public IServerWorld world { get; set; }
        public uint lastMessageId { get; set; }

        private readonly Queue<ControlMessage> _controlMessages = new Queue<ControlMessage>(256);

        public ServerPlayer()
        {
            type = GameEntityType.Player;
        }

        public override void AddControlMessage(ControlMessage message)
        {
            _controlMessages.Enqueue(message);
            lastMessageNum  = message.messageNum;
        }

        public override void Process()
        {
            while (_controlMessages.Count > 0)
            {
                var message = _controlMessages.Dequeue();
                Movement(message);
            }
        }
        
        public void Remove()
        {
            isRemoved = true;
        }

        public NetDataWriter Serialize(NetDataWriter netDataWriter, bool resetBeforeWriting = true)
        {
            if (resetBeforeWriting)
            {
                netDataWriter.Reset();
            }

            ServerEntityBase.WriteHeader(netDataWriter, this);
            netDataWriter.Put(lastMessageNum);
            
            return netDataWriter;
        }
    }
}