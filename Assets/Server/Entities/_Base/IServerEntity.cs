using Server.Worlds._Base;
using Shared.CommonInterfaces;
using Shared.Entities;

namespace Server.Entities._Base
{
    public interface IServerEntity : ISharedEntity, ISerializable
    {
        bool isRemoved { get; }
        IServerWorld world { get; set; }
        void Remove();
    }
}