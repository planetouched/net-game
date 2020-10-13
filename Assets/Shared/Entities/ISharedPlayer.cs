using Shared.Messages.FromClient;

namespace Shared.Entities
{
    public interface ISharedPlayer : ISharedEntity
    {
        uint lastMessageNum { get; }
        void AddControlMessage(ControlMessage message);
    }
}