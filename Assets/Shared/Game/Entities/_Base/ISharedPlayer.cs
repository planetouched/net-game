using Shared.Messages.FromClient;

namespace Shared.Game.Entities._Base
{
    public interface ISharedPlayer : ISharedEntity
    {
        uint lastMessageNum { get; }
        void AddControlMessage(ControlMessage message);
    }
}