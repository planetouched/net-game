using Shared.Game.Entities._Base;

namespace Server.Game.Entities._Base
{
    public interface IServerEntity : ISharedEntity, ISerializable
    {
        bool remove { get; set; }
        IServerWorld world { get; set; }
    }
}