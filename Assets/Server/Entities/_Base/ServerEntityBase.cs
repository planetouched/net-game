using Server.Worlds;
using Shared.Entities._Base;

namespace Server.Entities._Base
{
    public abstract class ServerEntityBase
    {
        public ISharedEntity sharedEntity { get; }
        public bool isRemoved { get; private set; }
        public ServerWorld world { get; set; }

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