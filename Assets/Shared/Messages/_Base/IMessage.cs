using Shared.CommonInterfaces;
using Shared.Enums;
using Shared.Factories;

namespace Shared.Messages._Base
{
    public interface IMessage : ISerializable, IDeserializable
    {
        int messageSize { get; }
        uint gameId { get; }
        byte messageId { get; }
        uint objectId { get; }

        MessageIds GetMessageId();
        int MessageSize();
    }
}