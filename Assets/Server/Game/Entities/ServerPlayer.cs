using System.Collections.Generic;
using System.Numerics;
using Server.Game.Entities._Base;
using Server.Game.Worlds._Base;
using Shared;
using Shared.Decoders;
using Shared.Enums;
using Shared.Game.Entities;
using Shared.Game.Entities._Base;
using Shared.Messages;
using Shared.Messages._Base;
using Shared.Messages.FromClient;
using Shared.Utils;
using UnityEditor.VersionControl;

namespace Server.Game.Entities
{
    public class ServerPlayer : SharedPlayerBase, IServerEntity
    {
        public bool remove { get; set; }
        public IServerWorld world { get; set; }
        public string ipPort { get; }
        public ByteToMessageDecoder byteToMessageDecoder { get; } = new ByteToMessageDecoder(SharedSettings.MaxMessageSize);

        private readonly Queue<ControlMessage> _controlMessages = new Queue<ControlMessage>(64);

        public ServerPlayer(string ipPort)
        {
            type = GameEntityType.Player;
            this.ipPort = ipPort;
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
        
        public void Serialize(ref int offset, byte[] buffer)
        {
            SerializeUtil.SetByte((byte)type, ref offset, buffer);
            SerializeUtil.SetUInt(objectId, ref offset, buffer);
            SerializeUtil.SetUInt(lastMessageNum, ref offset, buffer);
            SerializeUtil.SetVector3(position, ref offset, buffer);
            SerializeUtil.SetVector3(rotation, ref offset, buffer);
        }
    }
}