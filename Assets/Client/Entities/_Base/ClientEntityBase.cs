using System;
using System.Numerics;
using Basement.OEPFramework.UnityEngine._Base;
using LiteNetLib.Utils;
using Shared.Entities;

namespace Client.Entities._Base
{
    public abstract class ClientEntityBase : SharedEntityBase, IClientEntity
    {
        public bool isUsed { get; private set; }
        public bool dropped { get; private set; }
        public event Action<IDroppableItem> onDrop;
        
        public virtual void Drop()
        {
            if (dropped) return;
            dropped = true;
            onDrop?.Invoke(this);
        }

        public void UnUse()
        {
            isUsed = false;
        }

        public void Use()
        {
            isUsed = true;
        }

        public abstract void Create();

        public abstract void Deserialize(NetDataReader netDataReader);

        public static void ReadHeader(NetDataReader netDataReader, IClientEntity entity)
        {
            //skip type
            entity.objectId = netDataReader.GetUInt();
            
            var pX = netDataReader.GetFloat();
            var pY = netDataReader.GetFloat();
            var pZ = netDataReader.GetFloat();
            
            var rX = netDataReader.GetFloat();
            var rY = netDataReader.GetFloat();
            var rZ = netDataReader.GetFloat();
            
            entity.position = new Vector3(pX, pY, pZ);
            entity.rotation = new Vector3(rX, rY, rZ);
        }
        
        protected void ReadHeader(NetDataReader netDataReader)
        {
            ReadHeader(netDataReader, this);
        }
    }
}