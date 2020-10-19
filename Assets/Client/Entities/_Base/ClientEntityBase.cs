using Basement.OEPFramework.UnityEngine._Base;
using Shared.Entities._Base;
using Shared.Enums;

namespace Client.Entities._Base
{
    public abstract class ClientEntityBase : DroppableItemBase, IClientEntity
    {
        public GameEntityType type => current.type;
        public uint objectId => current.objectId;
        
        public bool isUsed { get; private set; }
        
        protected ISharedEntity current;
        protected ISharedEntity previous;
        protected float serverDeltaTime;

        public void UnUse()
        {
            isUsed = false;
        }

        public void Use()
        {
            isUsed = true;
        }

        public virtual void SetCurrentEntity(ISharedEntity entity)
        {
            previous = current;
            current = entity;
        }

        public void SetServerDeltaTime(float sDeltaTime)
        {
            serverDeltaTime = sDeltaTime;
        }

        public abstract void Process();

        public abstract void Create();
    }
}