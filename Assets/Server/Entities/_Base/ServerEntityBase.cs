using Server.Worlds._Base;
using Shared.Entities;

namespace Server.Entities._Base
{
    public abstract class ServerEntityBase : SharedEntityBase, IServerEntity
    {
        public bool isRemoved { get; private set; }
        public IServerWorld world { get; set; }
        
        public void Remove()
        {
            isRemoved = true;
        }

        public abstract void Serialize(ref int offset, byte[] buffer);
    }
}