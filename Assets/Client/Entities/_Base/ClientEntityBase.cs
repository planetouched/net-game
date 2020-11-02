using Basement.OEPFramework.UnityEngine._Base;
using Client.Worlds;
using Shared.Entities._Base;
using Shared.Enums;

namespace Client.Entities._Base
{
    public abstract class ClientEntityBase : DroppableItemBase
    {
        public GameEntityType type => current.type;
        public uint objectId => current.objectId;
        
        public bool isUsed { get; private set; }
        
        public ISharedEntity current { get; private set; }
        public ISharedEntity previous { get; private set; }
        protected float serverDeltaTime;
        protected readonly ClientWorld clientWorld;

        protected ClientEntityBase(ClientWorld clientWorld)
        {
            this.clientWorld = clientWorld;
        }
        
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

        public void SetSnapshotDeltaTime(float sDeltaTime)
        {
            serverDeltaTime = sDeltaTime;
        }

        public abstract void Process();

        public abstract void Create();
    }
}