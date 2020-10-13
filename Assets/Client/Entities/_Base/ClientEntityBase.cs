using System;
using Basement.OEPFramework.UnityEngine._Base;
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

        public abstract void Deserialize(ref int offset, byte[] buffer);
        public abstract void Create();
    }
}