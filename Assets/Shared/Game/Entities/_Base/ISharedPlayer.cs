using Shared.Messages._Base;

namespace Shared.Game.Entities._Base
{
    public interface ISharedPlayer : ISharedEntity
    {
        uint lastMessageNum { get; }
        void AddControlMessage(IPlayerControlMessage message);
    }
}