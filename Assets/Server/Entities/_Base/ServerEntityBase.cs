using Server.Worlds._Base;
using Shared.Entities._Base;

namespace Server.Entities._Base
{
    public abstract class ServerEntityBase : IServerEntity
    {
        public ISharedEntity sharedEntity { get; }
        public bool isRemoved { get; private set; }
        public IServerWorld world { get; set; }

        protected ServerEntityBase(ISharedEntity sharedEntity)
        {
            this.sharedEntity = sharedEntity;
        }
        
        public void Remove()
        {
            isRemoved = true;
        }

        public abstract void Process(float deltaTime);
    }
}