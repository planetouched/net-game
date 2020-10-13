using System.Collections.Generic;
using Client.Entities._Base;
using Client.Utils;
using Shared.Entities;
using Shared.Enums;
using Shared.Messages.FromClient;
using Shared.Utils;
using UnityEngine;

namespace Client.Entities
{
    public class ClientPlayer : SharedPlayerBase, IClientEntity
    {
        private readonly Queue<ControlMessage> _controlMessages = new Queue<ControlMessage>(64);
        public bool isPlayer { get; set; }
        
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
            
            Camera.main.transform.rotation = Quaternion.Euler(rotation.ToUnity());
            Camera.main.transform.position = position.ToUnity();
        }

        public void Deserialize(ref int offset, byte[] buffer)
        {
            type = (GameEntityType)SerializeUtil.GetByte(ref offset, buffer);
            objectId = SerializeUtil.GetUInt(ref offset, buffer);
            lastMessageNum = SerializeUtil.GetUInt(ref offset, buffer);
            position = SerializeUtil.GetVector3(ref offset, buffer);
            rotation = SerializeUtil.GetVector3(ref offset, buffer);
        }

        public void Drop()
        {
        }
    }
}