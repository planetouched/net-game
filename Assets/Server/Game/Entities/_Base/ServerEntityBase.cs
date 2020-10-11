using Shared.Game.Entities._Base;

namespace Server.Game.Entities._Base
{
    public abstract class ServerEntityBase : SharedEntityBase, IServerEntity
    {
        public bool remove { get; set; }
        public IServerWorld world { get; set; }

        public abstract void Serialize(ref int offset, byte[] buffer);
    }
}