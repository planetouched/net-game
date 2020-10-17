using System;
using System.Collections.Generic;
using Basement.OEPFramework.UnityEngine._Base;
using Client.Entities._Base;
using Client.Utils;
using LiteNetLib.Utils;
using Shared.Entities;
using Shared.Enums;
using Shared.Messages.FromClient;
using UnityEngine;

namespace Client.Entities
{
    public class ClientLocalPlayer : SharedPlayerBase, IClientEntity
    {
        public bool isUsed { get; private set; }
        public bool dropped { get; private set; }
        public event Action<IDroppableItem> onDrop;
        
        public static uint localObjectId { get; set; } 

        private readonly Queue<ControlMessage> _controlMessages = new Queue<ControlMessage>(64);

        public ClientLocalPlayer()
        {
            type = GameEntityType.Player;
        }

        public override void AddControlMessage(ControlMessage message)
        {
            _controlMessages.Enqueue(message);
            lastMessageNum = message.messageNum;
        }

        public void Create()
        {
            Camera.main.transform.rotation = Quaternion.Euler(rotation.ToUnity());
            Camera.main.transform.position = position.ToUnity();
        }

        public void UnUse()
        {
            isUsed = false;
        }

        public void Use()
        {
            isUsed = true;
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

        public void Drop()
        {
            if (dropped) return;
            dropped = true;
            onDrop?.Invoke(this);
        }

        public void Deserialize(NetDataReader netDataReader)
        {
            ClientEntityBase.ReadHeader(netDataReader, this);
            lastMessageNum = netDataReader.GetUInt();
        }
    }
}