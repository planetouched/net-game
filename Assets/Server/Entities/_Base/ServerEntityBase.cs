using Basement.OEPFramework.UnityEngine._Base;
using Server.Worlds;
using Shared.Entities._Base;

namespace Server.Entities._Base
{
    public abstract class ServerEntityBase : DroppableItemBase
    {
        public SharedEntityBase sharedEntity { get; }
        public bool isRemoved { get; private set; }
        public ServerWorld world { get; set; }

        protected ServerEntityBase(SharedEntityBase sharedEntity)
        {
            this.sharedEntity = sharedEntity;
        }
        
        public void Remove()
        {
            isRemoved = true;
        }

        public abstract void Process(float deltaTime);
        public abstract void Create();
    }
}