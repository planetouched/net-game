using Server.Worlds._Base;
using Shared.CommonInterfaces;
using Shared.Entities;

namespace Server.Entities._Base
{
    public interface IServerEntity : ISharedEntity, ISerializable
    {
        bool remove { get; set; }
        IServerWorld world { get; set; }
    }
}