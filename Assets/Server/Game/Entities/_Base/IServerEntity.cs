using Server.Game.Worlds._Base;
using Shared.CommonInterfaces;
using Shared.Game.Entities._Base;
using Shared.Messages._Base;

namespace Server.Game.Entities._Base
{
    public interface IServerEntity : ISharedEntity, ISerializable
    {
        bool remove { get; set; }
        IServerWorld world { get; set; }
    }
}